using System.Globalization;
using System.Text.Json;
using AccountERP.AI.Tools;
using AccountModelsLib;

namespace AccountERP.AI.Anomalies
{
    /// <summary>نتيجة مسح نافذة.</summary>
    public sealed class AiAnomalyScanResult
    {
        public int EntriesScanned { get; set; }
        /// <summary>كل (قيد، قاعدة) اجتاز إعادة التحقق، جديداً كان أو مكتشفاً من قبل.</summary>
        public int Found { get; set; }
        public int Inserted { get; set; }
        public bool Truncated { get; set; }
        /// <summary>قاعدة ← رسالة الخطأ (الباقى يكمل).</summary>
        public Dictionary<string, string> Errors { get; } = new(StringComparer.Ordinal);
        /// <summary>ملاحظات غير خطأ (قاعدة تُخطّيت لغياب مرجع كافٍ …).</summary>
        public List<string> Notes { get; } = new();
        public List<AiAnomalyRow> InsertedRows { get; } = new();
        /// <summary>زمن كل استعلام (الاسم، ms) — لقياس الأداء (فحص 3).</summary>
        public List<(string Name, long Ms)> QueryTimes { get; } = new();
    }

    /// <summary>
    /// P8.4.2 — ماسح القواعد الخمس (ai-assistant-p84-plan.md §1، ش1، ش8–ش15، ش29). يقرأ ai.v_gl_entries فقط عبر ReadOnlySqlExecutor
    /// (SELECT مدقَّق داخل معاملة تُلغى)، ويعيد التحقق من كل مرشح بـ AiAnomalyRules قبل الإدراج، ويكتب على AI_Anomalies فقط.
    /// النافذة [from, to) على inserted_at (المجدول) أو entry_date (المسح التاريخى)، مقسّمة يوماً بيوم؛ خط الأساس يسبق كل يوم تماماً.
    /// </summary>
    public sealed class AiAnomalyScanner
    {
        public const int CandidateTop = 2000;
        /// <summary>أقل امتداد لخط الأساس (أيام) لتشغيل قاعدة الحساب النادر — مرجع قصير يجعل كل حساب «نادراً».</summary>
        public const int RareMinBaselineDays = 30;
        private static readonly HashSet<string> Allowed = new(new[] { "v_gl_entries" }, StringComparer.OrdinalIgnoreCase);
        private static readonly JsonSerializerOptions Json = new() { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

        private readonly ReadOnlySqlExecutor _sql;
        private readonly AiAnomalyCLS _dal;
        private readonly AiToolAuditCLS _audit;

        /// <summary>خطاف اختبار: بيانات مصطنعة بدل المنفّذ (النص يُدقَّق بـ Validate دائماً).</summary>
        public Func<string, IReadOnlyDictionary<string, object>, AiQueryResult>? QuerySource { get; set; }
        /// <summary>آخر استعلامات النداء الأخير (للاختبار).</summary>
        public List<string> LastSql { get; } = new();
        /// <summary>false فى الاختبار يمنع كتابة التدقيق.</summary>
        public bool AuditEnabled { get; set; } = true;

        public AiAnomalyScanner(ReadOnlySqlExecutor sql, AiAnomalyCLS? dal = null, AiToolAuditCLS? audit = null)
        {
            _sql = sql;
            _dal = dal ?? new AiAnomalyCLS();
            _audit = audit ?? new AiToolAuditCLS();
        }

        private sealed class Candidate
        {
            public long EntryId;
            public string Rule = "";
            public long? LineId;
            public long? AccountId;
            public string? AccountName;
            public decimal? Amount;
            public decimal? Score;
            public Dictionary<string, object?> Details = new();
            public long? TwinId;
        }

        private sealed class Head
        {
            public long EntryId;
            public long? EntryNo;
            public DateTime? EntryDate;
            public DateTime? InsertedAt;
            public int? JournalCode;
            public long BranchId;
            public int? ReferenceType;
            public long? UserId;
            public decimal Total;
        }

        /// <summary>
        /// يمسح [from, to). byEntryDate = المسح التاريخى (تاريخ القيد؛ to حصرى). baselineFloor = أقدم لحظة يقرؤها خط الأساس
        /// (AnomalyBaselineHistory = 0 ⇒ لحظة التفعيل). budget = أقصى صفوف جديدة (الأعلى خطورة أولاً).
        /// </summary>
        public AiAnomalyScanResult Scan(DateTime from, DateTime to, AiAnomalySettings s, bool byEntryDate, bool backfill, long runId, int budget,
                                        CancellationToken ct, DateTime? baselineFloor = null)
        {
            LastSql.Clear();
            var result = new AiAnomalyScanResult();
            if (s == null || to <= from) return result;
            var cands = new List<Candidate>();
            string ts = byEntryDate ? "entry_date" : "inserted_at";

            foreach (var (dFrom, dTo) in AiAnomalyRules.DayChunks(from, to))
            {
                ct.ThrowIfCancellationRequested();
                DateTime baseFrom = dFrom.AddDays(-s.BaselineDays);
                if (baselineFloor.HasValue && baselineFloor.Value > baseFrom) baseFrom = baselineFloor.Value;
                var p = new Dictionary<string, object> { ["from"] = dFrom, ["to"] = dTo };

                try
                {
                    var c = Q("count", $"SELECT COUNT(DISTINCT entry_id) FROM ai.v_gl_entries WHERE {ts} >= @from AND {ts} < @to AND is_opening = 0 AND is_closing = 0", p, result, dFrom, dTo);
                    result.EntriesScanned += c.Rows.Count == 0 ? 0 : I(c.Rows[0][0]);
                }
                catch (OperationCanceledException) { throw; }
                catch (Exception ex) { result.Errors["count"] = ex.Message; }

                if (s.RuleOn(AiAnomalyRuleCodes.AmountOutlier))
                    Rule(AiAnomalyRuleCodes.AmountOutlier, result, () => Outliers(s, ts, dFrom, dTo, baseFrom, result, cands));
                if (s.RuleOn(AiAnomalyRuleCodes.RareAccount))
                {
                    if ((dFrom - baseFrom).TotalDays < RareMinBaselineDays)
                        AddNote(result, "rare_account: baseline shorter than " + RareMinBaselineDays + " days — skipped");
                    else
                        Rule(AiAnomalyRuleCodes.RareAccount, result, () => Rare(s, ts, dFrom, dTo, baseFrom, result, cands));
                }
                if (s.RuleOn(AiAnomalyRuleCodes.Duplicate))
                    Rule(AiAnomalyRuleCodes.Duplicate, result, () => Duplicates(s, ts, byEntryDate, dFrom, dTo, result, cands));
                if (s.RuleOn(AiAnomalyRuleCodes.OffHours) || s.RuleOn(AiAnomalyRuleCodes.RoundLarge))
                    Rule(s.RuleOn(AiAnomalyRuleCodes.OffHours) ? AiAnomalyRuleCodes.OffHours : AiAnomalyRuleCodes.RoundLarge, result,
                        () => HeadRules(s, ts, dFrom, dTo, result, cands));
            }

            // قاعدة على مستوى السطر: سطر واحد يمثّل (قيد، قاعدة) = الأعلى درجة/مبلغاً (ش17).
            var best = cands.GroupBy(c => (c.EntryId, c.Rule))
                .Select(g => g.OrderByDescending(c => c.Score ?? 0).ThenByDescending(c => c.Amount ?? 0).First()).ToList();
            if (best.Count == 0) return result;

            var heads = Heads(best.Select(c => c.EntryId).Concat(best.Where(c => c.TwinId.HasValue).Select(c => c.TwinId!.Value)).Distinct().ToList(), result);
            var rows = new List<AiAnomalyRow>();
            foreach (var c in best)
            {
                if (!heads.TryGetValue(c.EntryId, out var h)) continue;   // اختفى بين الاستعلامين
                if (c.TwinId.HasValue && heads.TryGetValue(c.TwinId.Value, out var twin))
                {
                    c.Details["twin_entry_no"] = twin.EntryNo;
                    c.Details["twin_date"] = twin.EntryDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                }
                if (c.Rule is AiAnomalyRuleCodes.RoundLarge or AiAnomalyRuleCodes.OffHours or AiAnomalyRuleCodes.Duplicate)
                    c.Amount ??= h.Total;
                rows.Add(new AiAnomalyRow
                {
                    Entry_Id = c.EntryId, RuleCode = c.Rule, Line_Id = c.LineId, Score = c.Score, Amount = c.Amount, EntryTotal = h.Total,
                    Severity = AiAnomalyRules.Severity(c.Rule, c.Score, h.Total, s),
                    Account_Id = c.AccountId, AccountName = c.AccountName, Branch_Id = h.BranchId, EntryNo = h.EntryNo, EntryDate = h.EntryDate,
                    EntryInsertedAt = h.InsertedAt, JournalCode = h.JournalCode, ReferenceType = h.ReferenceType, EntryUser_Id = h.UserId,
                    DetailsJson = JsonSerializer.Serialize(c.Details, Json), Run_Id = runId, IsBackfill = backfill, DetectedAt = DateTime.Now,
                });
            }
            result.Found = rows.Count;

            var existing = _dal.ExistingKeys(rows.Select(r => r.Entry_Id).Distinct().ToList());
            var fresh = rows.Where(r => !existing.Contains(r.Entry_Id.ToString(CultureInfo.InvariantCulture) + "|" + r.RuleCode))
                .OrderByDescending(r => AiAnomalySeverity.Rank(r.Severity)).ThenByDescending(r => r.Score ?? 0).ThenByDescending(r => r.Amount ?? 0)
                .ThenBy(r => r.Entry_Id).ToList();
            foreach (var r in fresh)
            {
                ct.ThrowIfCancellationRequested();
                if (result.Inserted >= Math.Max(0, budget)) { result.Truncated = true; break; }
                if (_dal.InsertIfNew(r)) { result.Inserted++; result.InsertedRows.Add(r); }
            }
            return result;
        }

        // ─────────────── القواعد ───────────────

        private string Manual(AiAnomalySettings s, string rule) => s.ManualScope(rule) ? " AND is_manual = 1" : "";

        private void Outliers(AiAnomalySettings s, string ts, DateTime from, DateTime to, DateTime baseFrom, AiAnomalyScanResult res, List<Candidate> cands)
        {
            string sql = $@"WITH w AS (
SELECT line_id, entry_id, account_id, account_name, CASE WHEN debit_equivalent > 0 THEN 1 ELSE 0 END AS side,
       debit_equivalent + credit_equivalent AS amount
FROM ai.v_gl_entries
WHERE {ts} >= @from AND {ts} < @to AND is_opening = 0 AND is_closing = 0{Manual(s, AiAnomalyRuleCodes.AmountOutlier)}
  AND debit_equivalent + credit_equivalent >= @min_amount
), b AS (
SELECT account_id, CASE WHEN debit_equivalent > 0 THEN 1 ELSE 0 END AS side, COUNT(*) AS n,
       AVG(CAST(debit_equivalent + credit_equivalent AS float)) AS mean,
       STDEV(CAST(debit_equivalent + credit_equivalent AS float)) AS sd
FROM ai.v_gl_entries
WHERE {ts} >= @base_from AND {ts} < @from AND is_opening = 0 AND is_closing = 0
  AND account_id IN (SELECT account_id FROM w)
GROUP BY account_id, CASE WHEN debit_equivalent > 0 THEN 1 ELSE 0 END
HAVING COUNT(*) >= @min_sample AND STDEV(CAST(debit_equivalent + credit_equivalent AS float)) > 0
)
SELECT TOP ({CandidateTop}) w.line_id, w.entry_id, w.account_id, w.account_name, w.side, w.amount, b.n, b.mean, b.sd, (w.amount - b.mean) / b.sd AS z
FROM w INNER JOIN b ON b.account_id = w.account_id AND b.side = w.side
WHERE (w.amount - b.mean) / b.sd >= @z
ORDER BY z DESC";
            var r = Q(AiAnomalyRuleCodes.AmountOutlier, sql, new Dictionary<string, object>
            {
                ["from"] = from, ["to"] = to, ["base_from"] = baseFrom, ["min_amount"] = s.MinAmount, ["min_sample"] = s.MinSample, ["z"] = (double)s.ZThreshold,
            }, res, from, to);
            if (r.Truncated || r.Rows.Count >= CandidateTop) res.Truncated = true;
            foreach (var row in r.Rows)
            {
                decimal amount = M(row[5]); int n = I(row[6]); double mean = F(row[7]), sd = F(row[8]);
                if (!AiAnomalyRules.IsOutlier(amount, mean, sd, n, s, out decimal z)) continue;
                cands.Add(new Candidate
                {
                    EntryId = L(row[1]), Rule = AiAnomalyRuleCodes.AmountOutlier, LineId = L(row[0]), AccountId = NL(row[2]), AccountName = Str(row[3]),
                    Amount = amount, Score = z,
                    Details = new()
                    {
                        ["side"] = I(row[4]) == 1 ? "debit" : "credit", ["amount"] = amount, ["mean"] = Math.Round((decimal)mean, 2), ["sd"] = Math.Round((decimal)sd, 2),
                        ["n"] = n, ["z"] = z, ["threshold"] = s.ZThreshold,
                    },
                });
            }
        }

        private void Rare(AiAnomalySettings s, string ts, DateTime from, DateTime to, DateTime baseFrom, AiAnomalyScanResult res, List<Candidate> cands)
        {
            string sql = $@"WITH w AS (
SELECT line_id, entry_id, account_id, account_name, debit_equivalent + credit_equivalent AS amount
FROM ai.v_gl_entries
WHERE {ts} >= @from AND {ts} < @to AND is_opening = 0 AND is_closing = 0{Manual(s, AiAnomalyRuleCodes.RareAccount)}
  AND debit_equivalent + credit_equivalent >= @min_amount
), b AS (
SELECT account_id, COUNT(DISTINCT entry_id) AS uses
FROM ai.v_gl_entries
WHERE {ts} >= @base_from AND {ts} < @from AND account_id IN (SELECT account_id FROM w)
GROUP BY account_id
)
SELECT TOP ({CandidateTop}) w.line_id, w.entry_id, w.account_id, w.account_name, w.amount, ISNULL(b.uses, 0) AS uses
FROM w LEFT JOIN b ON b.account_id = w.account_id
WHERE ISNULL(b.uses, 0) <= @max_uses
ORDER BY w.amount DESC";
            var r = Q(AiAnomalyRuleCodes.RareAccount, sql, new Dictionary<string, object>
            {
                ["from"] = from, ["to"] = to, ["base_from"] = baseFrom, ["min_amount"] = s.MinAmount, ["max_uses"] = s.RareMaxUses,
            }, res, from, to);
            if (r.Truncated || r.Rows.Count >= CandidateTop) res.Truncated = true;
            int window = (int)Math.Round((from - baseFrom).TotalDays);
            foreach (var row in r.Rows)
            {
                decimal amount = M(row[4]); int uses = I(row[5]);
                if (!AiAnomalyRules.IsRare(uses, amount, null, DateTime.Now, s)) continue;
                cands.Add(new Candidate
                {
                    EntryId = L(row[1]), Rule = AiAnomalyRuleCodes.RareAccount, LineId = L(row[0]), AccountId = NL(row[2]), AccountName = Str(row[3]), Amount = amount,
                    Details = new() { ["uses"] = uses, ["max_uses"] = s.RareMaxUses, ["window_days"] = window, ["amount"] = amount },
                });
            }
        }

        private void Duplicates(AiAnomalySettings s, string ts, bool byEntryDate, DateTime from, DateTime to, AiAnomalyScanResult res, List<Candidate> cands)
        {
            string tsExpr = byEntryDate ? "CAST(entry_date AS datetime)" : "inserted_at";
            string sql = $@"WITH e AS (
SELECT entry_id, branch_id, MIN({tsExpr}) AS ts, COUNT(*) AS lines, SUM(debit_equivalent) AS total
FROM ai.v_gl_entries
WHERE {ts} >= @dup_from AND {ts} < @to AND is_opening = 0 AND is_closing = 0{Manual(s, AiAnomalyRuleCodes.Duplicate)}
GROUP BY entry_id, branch_id
)
SELECT TOP ({CandidateTop}) c.entry_id, t.entry_id AS twin_id, c.lines, c.total, DATEDIFF(day, t.ts, c.ts) AS days_apart
FROM e c INNER JOIN e t ON t.branch_id = c.branch_id AND t.lines = c.lines AND ABS(t.total - c.total) <= 0.005 AND t.entry_id <> c.entry_id
     AND (t.ts < c.ts OR (t.ts = c.ts AND t.entry_id < c.entry_id)) AND t.ts >= DATEADD(day, -@dup_days, c.ts)
WHERE c.ts >= @from AND c.ts < @to
ORDER BY c.entry_id, t.ts DESC";
            var r = Q(AiAnomalyRuleCodes.Duplicate, sql, new Dictionary<string, object>
            {
                ["from"] = from, ["to"] = to, ["dup_from"] = from.AddDays(-s.DupDays), ["dup_days"] = s.DupDays,
            }, res, from, to);
            if (r.Truncated || r.Rows.Count >= CandidateTop) res.Truncated = true;
            if (r.Rows.Count == 0) return;
            var pairs = r.Rows.Select(x => (Entry: L(x[0]), Twin: L(x[1]), Lines: I(x[2]), Total: M(x[3]), Days: I(x[4]))).ToList();
            var lines = Lines(pairs.SelectMany(x => new[] { x.Entry, x.Twin }).Distinct().ToList(), res, from, to);
            foreach (var g in pairs.GroupBy(x => x.Entry))
            {
                // أقرب توأم (الأحدث قبله) تطابق سطوره.
                foreach (var pr in g)
                {
                    if (!lines.TryGetValue(pr.Entry, out var a) || !lines.TryGetValue(pr.Twin, out var b) || !AiAnomalyRules.SameLines(a, b)) continue;
                    cands.Add(new Candidate
                    {
                        EntryId = pr.Entry, Rule = AiAnomalyRuleCodes.Duplicate, TwinId = pr.Twin, Amount = pr.Total,
                        Details = new() { ["twin_entry_id"] = pr.Twin, ["lines"] = pr.Lines, ["total"] = pr.Total, ["days_apart"] = pr.Days },
                    });
                    break;
                }
            }
        }

        private void HeadRules(AiAnomalySettings s, string ts, DateTime from, DateTime to, AiAnomalyScanResult res, List<Candidate> cands)
        {
            // off_hours وround_large كلاهما فى ManualOnly فنطاقهما واحد.
            string sql = $@"SELECT TOP ({ReadOnlySqlExecutor.MaxRows}) entry_id, MIN(inserted_at) AS inserted_at, SUM(debit_equivalent) AS total
FROM ai.v_gl_entries
WHERE {ts} >= @from AND {ts} < @to AND is_opening = 0 AND is_closing = 0{Manual(s, AiAnomalyRuleCodes.OffHours)}
GROUP BY entry_id";
            var r = Q("entry_heads", sql, new Dictionary<string, object> { ["from"] = from, ["to"] = to }, res, from, to);
            if (r.Truncated) res.Truncated = true;
            foreach (var row in r.Rows)
            {
                long id = L(row[0]); DateTime? at = row[1] == null ? null : Convert.ToDateTime(row[1]); decimal total = M(row[2]);
                if (s.RuleOn(AiAnomalyRuleCodes.OffHours) && at.HasValue && AiAnomalyRules.IsOffHours(at.Value, s, out string? reason))
                    cands.Add(new Candidate
                    {
                        EntryId = id, Rule = AiAnomalyRuleCodes.OffHours, Amount = total,
                        Details = new()
                        {
                            ["reason"] = reason, ["hour"] = at.Value.ToString("HH:mm", CultureInfo.InvariantCulture), ["weekday"] = (int)at.Value.DayOfWeek,
                            ["date"] = at.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                            ["work_start"] = s.WorkStart.ToString(@"hh\:mm", CultureInfo.InvariantCulture), ["work_end"] = s.WorkEnd.ToString(@"hh\:mm", CultureInfo.InvariantCulture),
                        },
                    });
                if (s.RuleOn(AiAnomalyRuleCodes.RoundLarge) && AiAnomalyRules.IsRoundLarge(total, s))
                    cands.Add(new Candidate
                    {
                        EntryId = id, Rule = AiAnomalyRuleCodes.RoundLarge, Amount = total,
                        Details = new() { ["total"] = total, ["unit"] = s.RoundUnit, ["min_amount"] = s.RoundMinAmount },
                    });
            }
        }

        // ─────────────── استعلامات مساعدة ───────────────

        private Dictionary<long, List<AiAnomalyLineKey>> Lines(List<long> ids, AiAnomalyScanResult res, DateTime from, DateTime to)
        {
            var map = new Dictionary<long, List<AiAnomalyLineKey>>();
            foreach (var chunk in ids.Chunk(400))
            {
                var p = new Dictionary<string, object>();
                for (int i = 0; i < chunk.Length; i++) p["e" + i] = chunk[i];
                var r = Q("dup_lines", "SELECT entry_id, account_id, debit_equivalent, credit_equivalent FROM ai.v_gl_entries WHERE entry_id IN ("
                    + string.Join(", ", chunk.Select((_, i) => "@e" + i)) + ")", p, res, from, to);
                foreach (var row in r.Rows)
                {
                    long e = L(row[0]);
                    if (!map.TryGetValue(e, out var list)) map[e] = list = new List<AiAnomalyLineKey>();
                    list.Add(new AiAnomalyLineKey(L(row[1]), M(row[2]), M(row[3])));
                }
            }
            return map;
        }

        private Dictionary<long, Head> Heads(List<long> ids, AiAnomalyScanResult res)
        {
            var map = new Dictionary<long, Head>();
            foreach (var chunk in ids.Chunk(400))
            {
                var p = new Dictionary<string, object>();
                for (int i = 0; i < chunk.Length; i++) p["e" + i] = chunk[i];
                var r = Q("snapshot", @"SELECT entry_id, MAX(entry_no) AS entry_no, MAX(entry_date) AS entry_date, MIN(inserted_at) AS inserted_at, MAX(journal_code) AS journal_code,
       MAX(branch_id) AS branch_id, MAX(reference_type) AS reference_type, MAX(user_id) AS user_id, SUM(debit_equivalent) AS total
FROM ai.v_gl_entries WHERE entry_id IN (" + string.Join(", ", chunk.Select((_, i) => "@e" + i)) + ") GROUP BY entry_id", p, res, DateTime.MinValue, DateTime.MinValue);
                foreach (var row in r.Rows)
                    map[L(row[0])] = new Head
                    {
                        EntryId = L(row[0]), EntryNo = NL(row[1]), EntryDate = row[2] == null ? null : Convert.ToDateTime(row[2]).Date,
                        InsertedAt = row[3] == null ? null : Convert.ToDateTime(row[3]), JournalCode = row[4] == null ? null : I(row[4]),
                        BranchId = L(row[5]), ReferenceType = row[6] == null ? null : I(row[6]), UserId = NL(row[7]), Total = M(row[8]),
                    };
            }
            return map;
        }

        private void Rule(string rule, AiAnomalyScanResult res, Action body)
        {
            try { body(); }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                res.Errors[rule] = ex.Message;
                try { operations.DataDictionaryLogerException(ex); } catch { }
            }
        }

        private static void AddNote(AiAnomalyScanResult res, string note) { if (!res.Notes.Contains(note)) res.Notes.Add(note); }

        private AiQueryResult Q(string name, string sql, Dictionary<string, object> p, AiAnomalyScanResult res, DateTime from, DateTime to)
        {
            sql = sql.Trim();
            LastSql.Add(sql);
            var args = new { query = name, from = from == DateTime.MinValue ? null : from.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
                             to = to == DateTime.MinValue ? null : to.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) };
            try
            {
                AiQueryResult r;
                if (QuerySource != null)
                {
                    ReadOnlySqlExecutor.Validate(sql, Allowed, new HashSet<string>(p.Keys, StringComparer.OrdinalIgnoreCase));
                    r = QuerySource(sql, p);
                }
                else r = _sql.Execute(sql, Allowed, ReadOnlySqlExecutor.SessionFor(-1, 0), p, CancellationToken.None);
                res.QueryTimes.Add((name, r.DurationMs));
                Audit(name, sql, args, r.Rows.Count, r.DurationMs, AiToolOutcome.Ok, null);
                return r;
            }
            catch (Exception ex)
            {
                Audit(name, sql, args, null, null, AiToolOutcome.Error, ex.Message);
                throw;
            }
        }

        private void Audit(string name, string sql, object args, int? rows, long? ms, string outcome, string? error)
        {
            if (!AuditEnabled) return;
            try
            {
                _audit.Insert(null, 0, AiAnomalyConst.ScanAuditTool, JsonSerializer.Serialize(args, Json), sql, rows,
                              ms == null ? null : (int)Math.Min(int.MaxValue, ms.Value), outcome, error);
            }
            catch (Exception ex) { try { operations.DataDictionaryLogerException(ex); } catch { } }
        }

        private static long L(object? o) => o == null || o is DBNull ? 0 : Convert.ToInt64(o, CultureInfo.InvariantCulture);
        private static long? NL(object? o) => o == null || o is DBNull ? null : Convert.ToInt64(o, CultureInfo.InvariantCulture);
        private static int I(object? o) => o == null || o is DBNull ? 0 : Convert.ToInt32(o, CultureInfo.InvariantCulture);
        private static decimal M(object? o) => o == null || o is DBNull ? 0 : Convert.ToDecimal(o, CultureInfo.InvariantCulture);
        private static double F(object? o) => o == null || o is DBNull ? double.NaN : Convert.ToDouble(o, CultureInfo.InvariantCulture);
        private static string? Str(object? o) => o == null || o is DBNull ? null : Convert.ToString(o, CultureInfo.InvariantCulture);
    }
}
