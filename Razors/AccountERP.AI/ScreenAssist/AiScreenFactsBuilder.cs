using System.Collections.Concurrent;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using AccountERP.AI.Collections;
using AccountERP.AI.Tools;
using AccountModelsLib;
using AccountModelsLib.BusinessLibShared.GeneralLedger;
using R = AccountModelsLib.AiScreenAssistRules;

namespace AccountERP.AI.ScreenAssist
{
    /// <summary>
    /// P8.9.1 — بانى حقائق الشاشة (ai-assistant-p89-plan.md §1، ص2، ص7–ص14، ص17). Singleton.
    /// يعيد الحساب على الخادم من محددات الشاشة فقط: ميزان المراجعة بمحرك الشاشة (FinalAccountsService، قراءة فقط) بجلسة المستخدم،
    /// والباقى من Views المساعد عبر ReadOnlySqlExecutor بصلاحيته الضريبية. لا استدعاء لمحرك كشف العميل (يكتب فى جدول وسيط).
    /// لا يكتب شيئاً إلا التدقيق screen_facts. ذاكرة لكل (مستخدم، بصمة الحمولة، الفلتر الضريبى) 5 دقائق.
    /// </summary>
    public sealed class AiScreenFactsBuilder
    {
        private static readonly HashSet<string> Allowed = new(new[]
        {
            "v_gl_entries", "v_customer_ledger", "v_customers", "v_customer_credit", "v_sales", "v_sales_lines",
            "v_sales_returns", "v_sales_return_lines", "v_einvoice_status",
        }, StringComparer.OrdinalIgnoreCase);
        private static readonly JsonSerializerOptions Json = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
        private static readonly CultureInfo C = CultureInfo.InvariantCulture;

        private readonly ReadOnlySqlExecutor _sql;
        private readonly AiCollectionsScorer? _collections;
        private readonly AiToolAuditCLS _audit;
        private readonly ConcurrentDictionary<string, (DateTime At, AiScreenFacts Facts)> _cache = new();

        // ───── خطافات الاختبار ─────
        public Func<string, IReadOnlyDictionary<string, object>, AiQueryResult>? QuerySource { get; set; }
        public Func<AiScreenPayload, AiServerUser, FinalAccountsService.TrialBalanceResult>? TrialBalanceSource { get; set; }
        public Func<AiServerUser, List<FinalAccountsService.Option>>? BranchSource { get; set; }
        public Func<AiServerUser, List<FinalAccountsService.PeriodRow>>? PeriodSource { get; set; }
        public Func<DateTime> Today { get; set; } = () => DateTime.Today;
        public Func<DateTime> Now { get; set; } = () => DateTime.Now;
        public List<string> LastSql { get; } = new();
        public bool AuditEnabled { get; set; } = true;
        /// <summary>عدد مرات الحساب الفعلى (للاختبار: الذاكرة).</summary>
        public int BuildCount;

        public AiScreenFactsBuilder(ReadOnlySqlExecutor sql, AiCollectionsScorer? collections = null, AiToolAuditCLS? audit = null)
        {
            _sql = sql;
            _collections = collections;
            _audit = audit ?? new AiToolAuditCLS();
        }

        public void ClearCache() => _cache.Clear();

        // ───────────── الواجهة ─────────────

        /// <summary>الحمولة منظَّفة والصلاحية متحقق منها فى البوابة. settings = AI_Settings (لإعدادات الأعمار).</summary>
        public AiScreenFacts Build(AiScreenPayload p, AiServerUser server, IReadOnlyDictionary<string, string> settings, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(p.Question)) throw new AiScreenAssistException(AiScreenAssistErrors.BadRequest, "السؤال مطلوب.");
            string key = CacheKey(server.UserId, p.Fingerprint(), server.TaxFilter);
            if (_cache.TryGetValue(key, out var hit) && hit.At > Now().AddMinutes(-AiScreenAssistConst.FactsCacheMinutes)) return hit.Facts;

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var args = new { kind = p.Kind, question = p.Question, from = p.From?.ToString("yyyy-MM-dd", C), to = p.To?.ToString("yyyy-MM-dd", C),
                             branch = p.BranchId, currency = p.CurrencyId, filter = p.Filter, mode = p.Mode, entity = p.EntityId };
            try
            {
                Interlocked.Increment(ref BuildCount);
                AiScreenFacts f = p.Kind switch
                {
                    AiScreenKinds.TrialBalance => p.Question == AiScreenQuestions.ExplainAccount ? Account(p, server, ct) : TrialBalance(p, server, ct),
                    AiScreenKinds.CustomerStatement => Customer(p, server, settings, ct),
                    _ => Invoice(p, server, ct),
                };
                f.Kind = p.Kind; f.Question = p.Question; f.ComputedAt = Now();
                f.Hash = Sha(f.RawJson);
                Audit(server.UserId, args, sw.ElapsedMilliseconds, AiToolOutcome.Ok, null);
                if (_cache.Count >= AiScreenAssistConst.CacheMaxEntries) Trim();
                _cache[key] = (Now(), f);
                return f;
            }
            catch (AiScreenAssistException ex)
            {
                Audit(server.UserId, args, sw.ElapsedMilliseconds, AiToolOutcome.Denied, ex.Code);
                throw;
            }
            catch (Exception ex) when (ex is AiSqlRejectedException or ArgumentException)
            {
                Audit(server.UserId, args, sw.ElapsedMilliseconds, AiToolOutcome.Error, ex.Message);
                throw new AiScreenAssistException(AiScreenAssistErrors.Failed, ex.Message);
            }
        }

        /// <summary>حقائق محسوبة سابقاً لنفس المستخدم بهذه البصمة (للفقرة)، أو null إن انتهت.</summary>
        public AiScreenFacts? Cached(long userId, string hash)
        {
            var lim = Now().AddMinutes(-AiScreenAssistConst.FactsCacheMinutes);
            string prefix = userId.ToString(C) + "|";
            foreach (var kv in _cache)
                if (kv.Key.StartsWith(prefix, StringComparison.Ordinal) && kv.Value.At > lim && kv.Value.Facts.Hash == hash) return kv.Value.Facts;
            return null;
        }

        private static string CacheKey(long userId, string fp, int tax) => userId.ToString(C) + "|" + fp + "|" + tax.ToString(C);

        private void Trim()
        {
            var lim = Now().AddMinutes(-AiScreenAssistConst.FactsCacheMinutes);
            foreach (var kv in _cache.Where(k => k.Value.At <= lim).ToList()) _cache.TryRemove(kv.Key, out _);
            if (_cache.Count >= AiScreenAssistConst.CacheMaxEntries)
                foreach (var kv in _cache.OrderBy(k => k.Value.At).Take(_cache.Count / 2).ToList()) _cache.TryRemove(kv.Key, out _);
        }

        // ───────────── الصلاحيات (ص7) ─────────────

        public static bool CanDetail(AiServerUser s) =>
            s.HasRoute(AiScreenAssistConst.RJournalEntries) || s.HasRoute(AiScreenAssistConst.REntryAnomalies) || s.HasRoute(AiScreenAssistConst.RLedger);
        public static bool CanCredit(AiServerUser s) => s.HasRoute(AiScreenAssistConst.RCollections);
        public static bool CanBalance(AiServerUser s) =>
            s.HasRoute(AiScreenAssistConst.RStatementNormal) || s.HasRoute(AiScreenAssistConst.RStatementPeriod) || s.HasRoute(AiScreenAssistConst.RCustomerBalances);

        public List<FinalAccountsService.Option> Branches(AiServerUser s) =>
            BranchSource != null ? BranchSource(s) : s.Run(() => new FinalAccountsService().GetBranches());

        // ───────────── ميزان المراجعة (§1-أ، ص9، ص10) ─────────────

        public const string UnbalancedSql = @"WITH u AS (
SELECT entry_id, entry_no, entry_date, journal_name, reference_type,
       SUM(debit_equivalent) AS debit, SUM(credit_equivalent) AS credit,
       SUM(debit_equivalent) - SUM(credit_equivalent) AS diff
FROM ai.v_gl_entries
WHERE entry_no IS NOT NULL AND branch_id = @b AND entry_date >= @d1 AND entry_date <= @d2
GROUP BY entry_id, entry_no, entry_date, journal_name, reference_type
HAVING ABS(SUM(debit_equivalent) - SUM(credit_equivalent)) > 0.005
)
SELECT TOP (@top) entry_id, entry_no, entry_date, journal_name, reference_type, debit, credit, diff,
       (SELECT COUNT(*) FROM u) AS n, (SELECT SUM(diff) FROM u) AS total
FROM u ORDER BY ABS(diff) DESC";

        private const string AccountWhere = "entry_no IS NOT NULL AND branch_id = @b AND account_id = @acc AND entry_date >= @d1 AND entry_date <= @d2";

        public const string AccountSourceSql = @"SELECT reference_type, is_opening, is_closing, SUM(debit_equivalent) AS debit, SUM(credit_equivalent) AS credit, COUNT(DISTINCT entry_id) AS entries
FROM ai.v_gl_entries WHERE " + AccountWhere + @"
GROUP BY reference_type, is_opening, is_closing";

        public const string AccountTopSql = @"SELECT TOP (5) entry_id, entry_no, entry_date, journal_name, reference_type, SUM(debit_equivalent) AS debit, SUM(credit_equivalent) AS credit
FROM ai.v_gl_entries WHERE " + AccountWhere + @"
GROUP BY entry_id, entry_no, entry_date, journal_name, reference_type
ORDER BY SUM(debit_equivalent) + SUM(credit_equivalent) DESC";

        public const string AccountMonthlySql = @"SELECT YEAR(entry_date) AS y, MONTH(entry_date) AS m, SUM(debit_equivalent - credit_equivalent) AS net
FROM ai.v_gl_entries WHERE " + AccountWhere + @"
GROUP BY YEAR(entry_date), MONTH(entry_date)";

        private (FinalAccountsService.PeriodRow Period, FinalAccountsService.Option Branch, FinalAccountsService.TrialBalanceResult Tb) TbContext(AiScreenPayload p, AiServerUser s)
        {
            if (!p.From.HasValue || !p.To.HasValue || p.BranchId <= 0) throw new AiScreenAssistException(AiScreenAssistErrors.BadRequest, "الفترة والفرع مطلوبان.");
            var periods = PeriodSource != null ? PeriodSource(s) : s.Run(() => new FinalAccountsService().GetPeriods());
            var period = periods.FirstOrDefault(x => p.From.Value >= x.DateFrom.Date && p.To.Value <= x.DateTo.Date)
                         ?? throw new AiScreenAssistException(AiScreenAssistErrors.Period, "التاريخان خارج فترة مالية واحدة.");
            var branch = Branches(s).FirstOrDefault(b => b.Id == p.BranchId)
                         ?? throw new AiScreenAssistException(AiScreenAssistErrors.Branch, "الفرع غير مسموح.");
            if (p.CurrencyId > 0 && TrialBalanceSource == null && !s.Run(() => new FinalAccountsService().GetCurrencies("")).Any(c => c.Id == p.CurrencyId))
                throw new AiScreenAssistException(AiScreenAssistErrors.BadRequest, "عملة غير معروفة.");
            var tb = TrialBalanceSource != null ? TrialBalanceSource(p, s)
                   : s.Run(() => new FinalAccountsService().GetTrialBalance(period, p.BranchId, p.CurrencyId, p.From.Value, p.To.Value, FinalAccountsService.BalanceFilter.All));
            return (period, branch, tb);
        }

        private sealed class Unbal
        {
            public int Count;
            public decimal Total;
            public List<object?[]> Rows = new();
        }

        private Unbal Unbalanced(long branch, DateTime d1, DateTime d2, AiServerUser s)
        {
            var r = Q(UnbalancedSql, new() { ["b"] = branch, ["d1"] = d1, ["d2"] = d2, ["top"] = AiScreenAssistConst.TopEntries }, s);
            var u = new Unbal { Rows = r.Rows };
            if (r.Rows.Count > 0) { u.Count = I(r.Rows[0][8]); u.Total = M(r.Rows[0][9]); }
            return u;
        }

        private AiScreenFacts TrialBalance(AiScreenPayload p, AiServerUser s, CancellationToken ct)
        {
            var (period, branch, tb) = TbContext(p, s);
            var f = NewFacts(p, branch);
            decimal od = tb.OpeningDifference, md = tb.MovementDifference, cd = tb.ClosingDifference;
            f.Values.Add(new AiScreenValue { Key = "opening_diff", Number = od });
            f.Values.Add(new AiScreenValue { Key = "movement_diff", Number = md });
            f.Values.Add(new AiScreenValue { Key = "closing_diff", Number = cd });
            bool detail = CanDetail(s);
            var move = new Unbal();
            var prior = new Unbal();
            bool balanced = tb.IsBalanced;
            if (!balanced && p.CurrencyId == 0)
            {
                move = Unbalanced(p.BranchId, p.From!.Value, p.To!.Value, s);
                if (p.From.Value > period.DateFrom.Date) prior = Unbalanced(p.BranchId, period.DateFrom.Date, p.From.Value.AddDays(-1), s);
            }
            f.Findings = R.TrialBalanceFindings(od, md, p.CurrencyId, move.Count, move.Total, prior.Count, prior.Total);
            f.DetailHidden = !detail && (move.Count > 0 || prior.Count > 0);
            if (detail)
            {
                if (move.Rows.Count > 0) f.Tables.Add(EntriesTable("entries", move.Rows));
                if (prior.Rows.Count > 0) f.Tables.Add(EntriesTable("prior_entries", prior.Rows));
            }
            if (!balanced) AddLink(f, s, "integrity", AiScreenAssistConst.RIntegrity);
            if (!balanced && detail) AddLink(f, s, "entries", AiScreenAssistConst.RJournalEntries);

            JsonObject Doc(Namer n)
            {
                var o = Head(f, n);
                o["period_from"] = R.D(period.DateFrom);
                o["currency"] = p.CurrencyId == 0 ? "equivalent" : "single";
                o["differences"] = new JsonObject { ["opening"] = od, ["movement"] = md, ["closing"] = cd };
                o["findings"] = Findings(f);
                if (detail)
                {
                    o["entries"] = EntriesJson(move.Rows, n);
                    if (prior.Rows.Count > 0) o["entries_before_from"] = EntriesJson(prior.Rows, n);
                }
                o["detail_hidden"] = f.DetailHidden;
                return o;
            }
            return Finish(f, Doc);
        }

        private static AiScreenTable EntriesTable(string key, List<object?[]> rows) => new()
        {
            Key = key,
            Columns = new[] { "entry_no", "date", "journal", "source", "debit", "credit", "diff" },
            Types = new[] { "text", "date", "text", "code", "number", "number", "number" },
            Rows = rows.Select(x => new[] { Str(x[1]), Dt(x[2]), Str(x[3]), "Src_" + R.SourceOf(I(x[4]), false, false), N(x[5]), N(x[6]), N(x[7]) }).ToList(),
        };

        private static JsonArray EntriesJson(List<object?[]> rows, Namer n)
        {
            var a = new JsonArray();
            foreach (var x in rows)
            {
                var e = new JsonObject { ["entry_no"] = Str(x[1]), ["date"] = Dt(x[2]) };
                var j = n.Journal(Str(x[3]));
                if (j != null) e["journal"] = j;
                e["source"] = R.SourceOf(I(x[4]), false, false);
                e["debit"] = M(x[5]); e["credit"] = M(x[6]); e["diff"] = M(x[7]);
                a.Add(e);
            }
            return a;
        }

        private AiScreenFacts Account(AiScreenPayload p, AiServerUser s, CancellationToken ct)
        {
            var (period, branch, tb) = TbContext(p, s);
            var grp = tb.Groups.FirstOrDefault(g => g.Rows.Any(r => r.AccountId == p.EntityId))
                      ?? throw new AiScreenAssistException(AiScreenAssistErrors.NotFound, "الحساب غير موجود فى الميزان المعروض.");
            var row = grp.Rows.First(r => r.AccountId == p.EntityId);
            var f = NewFacts(p, branch);
            f.Subject = row.AccountName ?? ""; f.SubjectCode = row.Code ?? "";
            decimal openNet = row.FirstMadin - row.FirstDa2n, closeNet = row.LastMadin - row.LastDa2n;
            decimal groupNet = grp.Total.LastMadin - grp.Total.LastDa2n;
            decimal? groupPct = groupNet == 0 || grp.Rows.Count < 2 ? null : R.Pct(closeNet, groupNet);
            f.Values.Add(new AiScreenValue { Key = "opening", Number = Math.Abs(openNet), Side = Side(openNet) });
            f.Values.Add(new AiScreenValue { Key = "movement_debit", Number = row.Import });
            f.Values.Add(new AiScreenValue { Key = "movement_credit", Number = row.Export });
            f.Values.Add(new AiScreenValue { Key = "closing", Number = Math.Abs(closeNet), Side = Side(closeNet) });
            if (groupPct.HasValue) f.Values.Add(new AiScreenValue { Key = "group_share", Number = groupPct, Format = "pct" });

            bool detail = CanDetail(s) && p.CurrencyId == 0;
            var sources = new List<(string Source, decimal Debit, decimal Credit, int Count)>();
            var top = new List<object?[]>();
            var monthly = new List<(int Y, int M, decimal Net)>();
            int entries = 0;
            if (detail)
            {
                var prm = new Dictionary<string, object> { ["b"] = p.BranchId, ["acc"] = p.EntityId, ["d1"] = p.From!.Value, ["d2"] = p.To!.Value };
                foreach (var x in Q(AccountSourceSql, prm, s).Rows)
                {
                    string src = R.SourceOf(I(x[0]), I(x[1]) == 1, I(x[2]) == 1);
                    int k = sources.FindIndex(z => z.Source == src);
                    var add = (src, M(x[3]), M(x[4]), I(x[5]));
                    if (k < 0) sources.Add(add);
                    else sources[k] = (src, sources[k].Debit + add.Item2, sources[k].Credit + add.Item3, sources[k].Count + add.Item4);
                    entries += I(x[5]);
                }
                top = Q(AccountTopSql, prm, s).Rows;
                monthly = Q(AccountMonthlySql, prm, s).Rows.Select(x => (I(x[0]), I(x[1]), M(x[2]))).OrderBy(z => z.Item1).ThenBy(z => z.Item2)
                          .TakeLast(AiScreenAssistConst.MaxMonths).ToList();
                f.Values.Add(new AiScreenValue { Key = "entries_count", Number = entries, Format = "int" });
            }
            else if (CanDetail(s) == false && row.Import + row.Export > 0) f.DetailHidden = true;
            decimal moveAll = row.Import + row.Export;
            var manual = sources.FirstOrDefault(z => z.Source == "manual");
            string topNo = top.Count > 0 ? Str(top[0][1]) : "";
            decimal topAmt = top.Count > 0 ? M(top[0][5]) + M(top[0][6]) : 0;
            f.Findings = R.AccountFindings(row.Import, row.Export, groupPct,
                sources.Select(z => new KeyValuePair<string, decimal>(z.Source, z.Debit + z.Credit)).ToList(), topNo, topAmt,
                manual.Source == null ? 0 : manual.Debit + manual.Credit, manual.Source == null ? 0 : manual.Count, detail);
            if (detail)
            {
                if (sources.Count > 0)
                    f.Tables.Add(new AiScreenTable
                    {
                        Key = "sources", Columns = new[] { "source", "debit", "credit", "count", "share" }, Types = new[] { "code", "number", "number", "int", "pct" },
                        Rows = sources.OrderByDescending(z => z.Debit + z.Credit).Select(z => new[] { "Src_" + z.Source, N(z.Debit), N(z.Credit), z.Count.ToString(C), R.P1(R.Pct(z.Debit + z.Credit, moveAll)) }).ToList(),
                    });
                if (top.Count > 0)
                    f.Tables.Add(new AiScreenTable
                    {
                        Key = "top_entries", Columns = new[] { "entry_no", "date", "journal", "source", "debit", "credit" }, Types = new[] { "text", "date", "text", "code", "number", "number" },
                        Rows = top.Select(x => new[] { Str(x[1]), Dt(x[2]), Str(x[3]), "Src_" + R.SourceOf(I(x[4]), false, false), N(x[5]), N(x[6]) }).ToList(),
                    });
                if (monthly.Count > 0)
                    f.Tables.Add(new AiScreenTable
                    {
                        Key = "monthly", Columns = new[] { "month", "net" }, Types = new[] { "text", "number" },
                        Rows = monthly.Select(z => new[] { Month(z.Y, z.M), N(z.Net) }).ToList(),
                    });
            }
            AddLink(f, s, "ledger", AiScreenAssistConst.RLedger);

            JsonObject Doc(Namer n)
            {
                var o = Head(f, n);
                o["account"] = n.Account(row.AccountName ?? "");
                o["code"] = row.Code ?? "";
                o["opening_debit"] = row.FirstMadin; o["opening_credit"] = row.FirstDa2n;
                o["movement_debit"] = row.Import; o["movement_credit"] = row.Export;
                o["closing_debit"] = row.LastMadin; o["closing_credit"] = row.LastDa2n;
                if (groupPct.HasValue) o["group_share_pct"] = groupPct.Value;
                if (detail)
                {
                    o["entries_count"] = entries;
                    o["by_source"] = new JsonArray(sources.Select(z => (JsonNode)new JsonObject
                    { ["source"] = z.Source, ["debit"] = z.Debit, ["credit"] = z.Credit, ["count"] = z.Count, ["share_pct"] = R.Pct(z.Debit + z.Credit, moveAll) }).ToArray());
                    o["top_entries"] = new JsonArray(top.Select(x =>
                    {
                        var e = new JsonObject { ["entry_no"] = Str(x[1]), ["date"] = Dt(x[2]) };
                        var j = n.Journal(Str(x[3])); if (j != null) e["journal"] = j;
                        e["source"] = R.SourceOf(I(x[4]), false, false); e["debit"] = M(x[5]); e["credit"] = M(x[6]);
                        return (JsonNode)e;
                    }).ToArray());
                    o["monthly"] = new JsonArray(monthly.Select(z => (JsonNode)new JsonObject { ["month"] = Month(z.Y, z.M), ["net"] = z.Net }).ToArray());
                }
                o["findings"] = Findings(f);
                o["detail_hidden"] = f.DetailHidden;
                return o;
            }
            return Finish(f, Doc);
        }

        // ───────────── كشف حساب العميل (§1-ب، ص12، ص13) ─────────────

        public const string CustomerSql = @"SELECT reference_type, ledger_source,
       SUM(CASE WHEN movement_date < @from THEN debit - credit ELSE 0 END) AS before_net,
       SUM(CASE WHEN movement_date >= @from AND movement_date <= @to THEN debit ELSE 0 END) AS range_debit,
       SUM(CASE WHEN movement_date >= @from AND movement_date <= @to THEN credit ELSE 0 END) AS range_credit,
       SUM(CASE WHEN movement_date >= @from AND movement_date <= @to THEN 1 ELSE 0 END) AS range_count,
       SUM(debit - credit) AS total_net, MAX(movement_date) AS last_date
FROM ai.v_customer_ledger WHERE customer_id = @c
GROUP BY reference_type, ledger_source";

        public const string CustomerTopSql = @"SELECT TOP (5) movement_date, reference_type, ledger_source, debit, credit
FROM ai.v_customer_ledger WHERE customer_id = @c AND movement_date >= @from AND movement_date <= @to
ORDER BY debit + credit DESC";

        public const string CustomerHeadSql = "SELECT customer_code, customer_name FROM ai.v_customers WHERE customer_id = @c";
        public const string CustomerCreditSql = "SELECT credit_limit FROM ai.v_customer_credit WHERE customer_id = @c";

        private AiScreenFacts Customer(AiScreenPayload p, AiServerUser s, IReadOnlyDictionary<string, string> settings, CancellationToken ct)
        {
            var head = Q(CustomerHeadSql, new() { ["c"] = p.EntityId }, s);
            if (head.Rows.Count == 0) throw new AiScreenAssistException(AiScreenAssistErrors.NotFound, "العميل غير موجود.");
            string name = Str(head.Rows[0][1]), code = Str(head.Rows[0][0]);
            var today = Today().Date;
            DateTime from = p.From ?? new DateTime(1900, 1, 1), to = p.To ?? today;
            var prm = new Dictionary<string, object> { ["c"] = p.EntityId, ["from"] = from, ["to"] = to };
            var groups = Q(CustomerSql, prm, s).Rows;
            decimal opening = 0, rDebit = 0, rCredit = 0, balance = 0;
            DateTime? lastReceipt = null;
            var types = new List<(string Type, decimal Debit, decimal Credit, int Count)>();
            foreach (var x in groups)
            {
                string t = R.TypeOf(x[0] == null ? (int?)null : I(x[0]), Str(x[1]));
                opening += M(x[2]); rDebit += M(x[3]); rCredit += M(x[4]); balance += M(x[6]);
                if (t == "receipts" && x[7] != null) { var d = DtV(x[7]); if (!lastReceipt.HasValue || d > lastReceipt) lastReceipt = d; }
                int k = types.FindIndex(z => z.Type == t);
                if (k < 0) types.Add((t, M(x[3]), M(x[4]), I(x[5])));
                else types[k] = (t, types[k].Debit + M(x[3]), types[k].Credit + M(x[4]), types[k].Count + I(x[5]));
            }
            decimal closing = opening + rDebit - rCredit;
            var top = Q(CustomerTopSql, prm, s).Rows;

            var cs = AiCollectSettings.From(settings ?? new Dictionary<string, string>());
            decimal[]? buckets = null;
            int overdueCount = 0, oldest = 0, openInv = 0;
            bool taxRestricted = s.TaxFilter != -1;
            if (balance > AiScreenAssistConst.Tolerance && _collections != null)
            {
                var scope = new AiCollectScope
                {
                    UserId = s.UserId, IsAdmin = s.User.IsAdmin, TaxFilter = s.TaxFilter,
                    BranchIds = s.User.IsAdmin ? null : Branches(s).Select(b => b.Id).Distinct().ToList(),
                };
                var inv = _collections.Invoices(p.EntityId, balance, scope, cs, ct);
                buckets = R.Buckets(inv, balance);
                overdueCount = inv.Count(i => i.DaysOverdue > 0);
                openInv = inv.Count;
                oldest = inv.Count == 0 ? 0 : inv.Max(i => i.DaysOverdue);
                if (buckets[5] > 0) oldest = Math.Max(oldest, AiCollectConst.UnallocatedMinDays);
            }
            decimal? limit = null;
            if (CanCredit(s))
            {
                var cr = Q(CustomerCreditSql, new() { ["c"] = p.EntityId }, s);
                if (cr.Rows.Count > 0 && cr.Rows[0][0] != null) limit = M(cr.Rows[0][0]);
            }
            int? sinceReceipt = lastReceipt.HasValue ? (today - lastReceipt.Value.Date).Days : null;

            var f = NewFacts(p, null);
            f.Subject = name; f.SubjectCode = code; f.CustomerId = p.EntityId;
            f.TaxRestricted = taxRestricted && buckets != null; f.StatementNote = true;
            f.Values.Add(new AiScreenValue { Key = "balance", Number = Math.Abs(balance), Side = Side(balance) });
            if (p.From.HasValue) f.Values.Add(new AiScreenValue { Key = "opening", Number = Math.Abs(opening), Side = Side(opening) });
            f.Values.Add(new AiScreenValue { Key = "range_debit", Number = rDebit });
            f.Values.Add(new AiScreenValue { Key = "range_credit", Number = rCredit });
            if (p.From.HasValue || p.To.HasValue) f.Values.Add(new AiScreenValue { Key = "closing", Number = Math.Abs(closing), Side = Side(closing) });
            if (buckets != null)
            {
                f.Values.Add(new AiScreenValue { Key = "overdue", Number = buckets[1] + buckets[2] + buckets[3] + buckets[4] + buckets[5] });
                if (oldest > 0) f.Values.Add(new AiScreenValue { Key = "oldest_days", Number = oldest, Format = "int" });
            }
            if (lastReceipt.HasValue) f.Values.Add(new AiScreenValue { Key = "last_receipt", Date = lastReceipt, Format = "date" });
            if (limit.HasValue) f.Values.Add(new AiScreenValue { Key = "credit_limit", Number = limit });
            f.Findings = R.CustomerFindings(balance, buckets, overdueCount, oldest, cs.InvoiceMonths, lastReceipt, today, limit,
                                            p.From.HasValue ? closing - opening : (decimal?)null);
            if (types.Count > 0)
                f.Tables.Add(new AiScreenTable
                {
                    Key = "types", Columns = new[] { "type", "debit", "credit", "count" }, Types = new[] { "code", "number", "number", "int" },
                    Rows = types.Where(z => z.Count > 0 || z.Debit != 0 || z.Credit != 0).OrderByDescending(z => z.Debit + z.Credit)
                                .Select(z => new[] { "Ty_" + z.Type, N(z.Debit), N(z.Credit), z.Count.ToString(C) }).ToList(),
                });
            if (top.Count > 0)
                f.Tables.Add(new AiScreenTable
                {
                    Key = "top_moves", Columns = new[] { "date", "type", "debit", "credit" }, Types = new[] { "date", "code", "number", "number" },
                    Rows = top.Select(x => new[] { Dt(x[0]), "Ty_" + R.TypeOf(x[1] == null ? (int?)null : I(x[1]), Str(x[2])), N(x[3]), N(x[4]) }).ToList(),
                });
            if (buckets != null)
            {
                string[] bk = { "B_not_due", "B_1_30", "B_31_60", "B_61_90", "B_over_90", "B_unallocated" };
                f.Tables.Add(new AiScreenTable
                {
                    Key = "aging", Columns = new[] { "bucket", "amount" }, Types = new[] { "code", "number" },
                    Rows = Enumerable.Range(0, 6).Where(i => buckets[i] != 0).Select(i => new[] { bk[i], N(buckets[i]) }).ToList(),
                });
            }
            AddLink(f, s, "collections", AiScreenAssistConst.RCollections);

            JsonObject Doc(Namer n)
            {
                var o = Head(f, n);
                var cust = n.Customer(name);
                if (cust != null) o["customer"] = cust; else o["customer_id"] = p.EntityId;
                o["balance"] = Math.Abs(balance); o["balance_side"] = Side(balance) ?? "zero";
                o["opening"] = opening; o["range_debit"] = rDebit; o["range_credit"] = rCredit; o["closing"] = closing;
                o["by_type"] = new JsonArray(types.Select(z => (JsonNode)new JsonObject { ["type"] = z.Type, ["debit"] = z.Debit, ["credit"] = z.Credit, ["count"] = z.Count }).ToArray());
                o["top_movements"] = new JsonArray(top.Select(x => (JsonNode)new JsonObject
                { ["date"] = Dt(x[0]), ["type"] = R.TypeOf(x[1] == null ? (int?)null : I(x[1]), Str(x[2])), ["debit"] = M(x[3]), ["credit"] = M(x[4]) }).ToArray());
                if (lastReceipt.HasValue) { o["last_receipt"] = R.D(lastReceipt.Value); o["days_since_receipt"] = sinceReceipt; }
                if (buckets != null)
                    o["aging"] = new JsonObject
                    {
                        ["not_due"] = buckets[0], ["d1_30"] = buckets[1], ["d31_60"] = buckets[2], ["d61_90"] = buckets[3], ["over_90"] = buckets[4],
                        ["unallocated"] = buckets[5], ["oldest_days"] = oldest, ["open_invoices"] = openInv,
                    };
                if (limit.HasValue) o["credit_limit"] = limit.Value;
                o["findings"] = Findings(f);
                o["tax_restricted"] = f.TaxRestricted;
                return o;
            }
            return Finish(f, Doc);
        }

        // ───────────── الفاتورة / المرتجع (§1-ج، ص14) ─────────────

        public static string InvoiceHeadSql(bool isReturn) => isReturn
            ? @"SELECT return_id, return_no, return_date, customer_id, customer_name, branch_id, total, commission, sales_tax, ats05_value, stamp_duty, net_total,
       is_tax_invoice, CAST(NULL AS int) AS is_tax_per_item, is_credit, CAST(NULL AS decimal(18, 2)) AS paid_value, CAST(NULL AS decimal(18, 2)) AS rest_value,
       CAST(NULL AS int) AS is_posted, CAST(NULL AS date) AS due_date
FROM ai.v_sales_returns WHERE return_id = @id"
            : @"SELECT sale_id, invoice_no, invoice_date, customer_id, customer_name, branch_id, total, commission, sales_tax, ats05_value, stamp_duty, net_total,
       is_tax_invoice, is_tax_per_item, is_credit, paid_value, rest_value, is_posted, due_date
FROM ai.v_sales WHERE sale_id = @id";

        public static string InvoiceLinesSql(bool isReturn) =>
            "SELECT COUNT(*) AS n, SUM(line_total) AS t, SUM(line_sales_tax) AS tax, SUM(line_net) AS net FROM ai." +
            (isReturn ? "v_sales_return_lines WHERE return_id = @id" : "v_sales_lines WHERE sale_id = @id");

        public static string InvoiceTopSql(bool isReturn) =>
            "SELECT TOP (5) item_name, qty, unit_name, unit_price, line_net FROM ai." +
            (isReturn ? "v_sales_return_lines WHERE return_id = @id" : "v_sales_lines WHERE sale_id = @id") + " ORDER BY line_net DESC";

        public const string InvoiceEInvoiceSql = "SELECT upload_state, attempts FROM ai.v_einvoice_status WHERE doc_kind = @k AND sale_id = @id";
        public const string CustomerBalanceSql = "SELECT SUM(debit - credit) AS balance FROM ai.v_customer_ledger WHERE customer_id = @c";

        private AiScreenFacts Invoice(AiScreenPayload p, AiServerUser s, CancellationToken ct)
        {
            bool ret = p.Kind == AiScreenKinds.SalesReturn;
            var h = Q(InvoiceHeadSql(ret), new() { ["id"] = p.EntityId }, s);
            if (h.Rows.Count == 0) throw new AiScreenAssistException(AiScreenAssistErrors.NotVisible, "المستند غير موجود أو لا يحق لك رؤيته.");
            var x = h.Rows[0];
            long branchId = L(x[5]);
            FinalAccountsService.Option? branch = null;
            if (!s.User.IsAdmin || BranchSource != null)
            {
                branch = Branches(s).FirstOrDefault(b => b.Id == branchId);
                if (branch == null) throw new AiScreenAssistException(AiScreenAssistErrors.NotVisible, "المستند من فرع غير مسموح.");
            }
            string no = Str(x[1]), custName = Str(x[4]);
            long custId = L(x[3]);
            DateTime? date = x[2] == null ? null : DtV(x[2]);
            decimal total = M(x[6]), commission = M(x[7]), tax = M(x[8]), ats = M(x[9]), stamp = M(x[10]), net = M(x[11]);
            bool isTax = I(x[12]) == 1, perItem = I(x[13]) == 1, credit = I(x[14]) == 1, posted = ret || I(x[17]) == 1;
            decimal paid = M(x[15]), rest = M(x[16]);
            DateTime? due = x[18] == null ? null : DtV(x[18]);
            var today = Today().Date;

            var ln = Q(InvoiceLinesSql(ret), new() { ["id"] = p.EntityId }, s).Rows;
            int lines = ln.Count > 0 ? I(ln[0][0]) : 0;
            decimal lt = ln.Count > 0 ? M(ln[0][1]) : 0, ltax = ln.Count > 0 ? M(ln[0][2]) : 0, lnet = ln.Count > 0 ? M(ln[0][3]) : 0;
            var top = Q(InvoiceTopSql(ret), new() { ["id"] = p.EntityId }, s).Rows;
            string? einv = null;
            if (isTax)
            {
                var e = Q(InvoiceEInvoiceSql, new() { ["k"] = ret ? "return" : "invoice", ["id"] = p.EntityId }, s).Rows;
                if (e.Count > 0) einv = Str(e[0][0]);
            }
            decimal? custBal = null;
            if (CanBalance(s) && custId > 0)
            {
                var b = Q(CustomerBalanceSql, new() { ["c"] = custId }, s).Rows;
                if (b.Count > 0 && b[0][0] != null) custBal = M(b[0][0]);
            }
            int overdue = ret ? 0 : R.DaysOverdue(credit, rest, due, today);

            var f = NewFacts(p, branch);
            f.Subject = custName; f.SubjectCode = no; f.CustomerId = custId; f.From = date; f.To = date;
            f.Values.Add(new AiScreenValue { Key = "total", Number = total });
            if (commission != 0) f.Values.Add(new AiScreenValue { Key = "commission", Number = commission });
            f.Values.Add(new AiScreenValue { Key = "sales_tax", Number = tax });
            if (ats != 0) f.Values.Add(new AiScreenValue { Key = "ats05", Number = ats });
            if (stamp != 0) f.Values.Add(new AiScreenValue { Key = "stamp", Number = stamp });
            f.Values.Add(new AiScreenValue { Key = "net", Number = net });
            if (!ret) { f.Values.Add(new AiScreenValue { Key = "paid", Number = paid }); f.Values.Add(new AiScreenValue { Key = "rest", Number = rest }); }
            if (due.HasValue) f.Values.Add(new AiScreenValue { Key = "due_date", Date = due, Format = "date" });
            f.Values.Add(new AiScreenValue { Key = "lines", Number = lines, Format = "int" });
            if (custBal.HasValue) f.Values.Add(new AiScreenValue { Key = "customer_balance", Number = Math.Abs(custBal.Value), Side = Side(custBal.Value) });
            if (einv != null) f.Values.Add(new AiScreenValue { Key = "einvoice", Text = "E_" + einv, Format = "code" });
            f.Findings = R.InvoiceFindings(ret, total, lines > 0 ? lt : (decimal?)null, tax, lines > 0 && perItem ? ltax : (decimal?)null,
                                           isTax, perItem, credit, rest, due, today, einv ?? "", posted);
            if (top.Count > 0)
                f.Tables.Add(new AiScreenTable
                {
                    Key = "top_lines", Columns = new[] { "item", "qty", "price", "net" }, Types = new[] { "text", "number", "number", "number" },
                    Rows = top.Select(t => new[] { Str(t[0]), N(t[1]) + (Str(t[2]).Length > 0 ? " " + Str(t[2]) : ""), N(t[3]), N(t[4]) }).ToList(),
                });
            if (custId > 0 && (s.HasRoute(AiScreenAssistConst.RStatementNormal)))
                f.Links.Add(new AiScreenLink { Key = "statement", Url = AiScreenAssistConst.RStatementNormal + "?customer=" + custId.ToString(C) });
            AddLink(f, s, "collections", AiScreenAssistConst.RCollections);

            JsonObject Doc(Namer n)
            {
                var o = Head(f, n);
                o["doc"] = ret ? "sales_return" : "sales_invoice";
                o["invoice_no"] = no;
                if (date.HasValue) o["date"] = R.D(date.Value);
                var cust = n.Customer(custName);
                if (cust != null) o["customer"] = cust; else o["customer_id"] = custId;
                o["is_tax_invoice"] = isTax; o["is_tax_per_item"] = perItem; o["is_credit"] = credit;
                o["total"] = total; o["commission"] = commission; o["sales_tax"] = tax; o["ats05_value"] = ats; o["stamp_duty"] = stamp; o["net_total"] = net;
                if (!ret) { o["paid"] = paid; o["rest"] = rest; }
                if (due.HasValue) o["due_date"] = R.D(due.Value);
                if (overdue > 0) o["days_overdue"] = overdue;
                o["lines_count"] = lines; o["lines_total"] = lt; o["lines_tax"] = ltax; o["lines_net"] = lnet;
                o["top_lines"] = new JsonArray(top.Select(t => (JsonNode)new JsonObject
                { ["item"] = n.Item(Str(t[0])), ["qty"] = M(t[1]), ["unit_price"] = M(t[3]), ["line_net"] = M(t[4]) }).ToArray());
                if (einv != null) o["einvoice_state"] = einv;
                if (custBal.HasValue) o["customer_balance"] = custBal.Value;
                o["findings"] = Findings(f);
                return o;
            }
            return Finish(f, Doc);
        }

        // ───────────── بناء JSON بثلاث صيغ (ص19، ص21) ─────────────

        /// <summary>
        /// يحوّل الأسماء حسب الصيغة: Full = الأسماء؛ Prompt = العميل [C1]؛ PromptMin = كل الأسماء رموز واليومية محذوفة؛ Chat = بلا اسم العميل.
        /// </summary>
        internal sealed class Namer
        {
            public enum Mode { Full, Prompt, PromptMin, Chat }
            private readonly Mode _m;
            private readonly Dictionary<string, string> _acc = new(), _item = new();
            public readonly Dictionary<string, string> Tokens = new();
            public Namer(Mode m) { _m = m; }

            public string? Customer(string name)
            {
                if (_m == Mode.Chat) return null;
                if (_m == Mode.Full) return name ?? "";
                Tokens[AiScreenAssistConst.CustomerToken] = name ?? "";
                return AiScreenAssistConst.CustomerToken;
            }
            public string Account(string name) => Tok(name, _acc, "A");
            public string Item(string name) => Tok(name, _item, "I");
            public string? Journal(string name) => _m == Mode.PromptMin ? null : name ?? "";

            private string Tok(string name, Dictionary<string, string> map, string prefix)
            {
                name ??= "";
                if (_m != Mode.PromptMin || name.Length == 0) return name;
                if (!map.TryGetValue(name, out var t)) { t = "[" + prefix + (map.Count + 1).ToString(C) + "]"; map[name] = t; Tokens[t] = name; }
                return t;
            }
        }

        private AiScreenFacts Finish(AiScreenFacts f, Func<Namer, JsonObject> doc)
        {
            f.RawJson = Cap(doc(new Namer(Namer.Mode.Full)));
            var pr = new Namer(Namer.Mode.Prompt);
            f.PromptJson = Cap(doc(pr));
            var min = new Namer(Namer.Mode.PromptMin);
            f.PromptJsonMin = Cap(doc(min));
            f.ChatJson = Cap(doc(new Namer(Namer.Mode.Chat)));
            foreach (var kv in pr.Tokens) f.Names[kv.Key] = kv.Value;
            foreach (var kv in min.Tokens) f.Names[kv.Key] = kv.Value;
            return f;
        }

        /// <summary>≤ MaxFactsChars: تُقص أطول القوائم من آخرها.</summary>
        public static string Cap(JsonObject o)
        {
            string s = o.ToJsonString(Json);
            int guard = 0;
            while (s.Length > AiScreenAssistConst.MaxFactsChars && guard++ < 500)
            {
                var arr = o.Select(kv => kv.Value as JsonArray).Where(a => a != null && a.Count > 0).OrderByDescending(a => a!.ToJsonString(Json).Length).FirstOrDefault();
                if (arr == null) break;
                arr.RemoveAt(arr.Count - 1);
                o["truncated"] = true;
                s = o.ToJsonString(Json);
            }
            return s;
        }

        private static JsonObject Head(AiScreenFacts f, Namer n)
        {
            var o = new JsonObject { ["screen"] = f.Kind.Length > 0 ? f.Kind : null, ["question"] = f.Question };
            if (f.From.HasValue) o["from"] = R.D(f.From.Value);
            if (f.To.HasValue) o["to"] = R.D(f.To.Value);
            if (f.BranchId > 0) { o["branch_id"] = f.BranchId; var b = n.Journal(f.BranchName); if (b != null && b.Length > 0) o["branch"] = b; }
            return o;
        }

        private static JsonArray Findings(AiScreenFacts f) => new(f.Findings.Select(x =>
        {
            var o = new JsonObject { ["code"] = x.Code, ["severity"] = x.Severity };
            foreach (var kv in x.Values) o[kv.Key] = decimal.TryParse(kv.Value, NumberStyles.Number, C, out var d) && kv.Key != "entry_no" ? JsonValue.Create(d) : JsonValue.Create(kv.Value);
            return (JsonNode)o;
        }).ToArray());

        private static AiScreenFacts NewFacts(AiScreenPayload p, FinalAccountsService.Option? branch) => new()
        {
            Kind = p.Kind, Question = p.Question, From = p.From, To = p.To,
            BranchId = branch?.Id ?? 0, BranchName = branch?.Name ?? "",
        };

        private static void AddLink(AiScreenFacts f, AiServerUser s, string key, string route)
        {
            if (s.HasRoute(route)) f.Links.Add(new AiScreenLink { Key = key, Url = route });
        }

        // ───────────── الاستعلام والتدقيق ─────────────

        private AiQueryResult Q(string sql, Dictionary<string, object> p, AiServerUser s)
        {
            LastSql.Add(sql);
            if (QuerySource != null)
            {
                ReadOnlySqlExecutor.Validate(sql, Allowed, new HashSet<string>(p.Keys, StringComparer.OrdinalIgnoreCase));
                return QuerySource(sql, p);
            }
            return _sql.Execute(sql, Allowed, ReadOnlySqlExecutor.SessionFor(s.TaxFilter, s.UserId), p, CancellationToken.None);
        }

        private void Audit(long userId, object args, long ms, string outcome, string? error)
        {
            if (!AuditEnabled) return;
            try
            {
                _audit.Insert(null, userId, AiScreenAssistConst.FactsAuditTool, JsonSerializer.Serialize(args, Json), null, null,
                              (int)Math.Min(int.MaxValue, ms), outcome, error);
            }
            catch (Exception ex) { try { operations.DataDictionaryLogerException(ex); } catch { } }
        }

        internal static string Sha(string s) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s ?? ""))).Substring(0, 32);

        private static string? Side(decimal v) => v > AiScreenAssistConst.Tolerance ? "debit" : v < -AiScreenAssistConst.Tolerance ? "credit" : null;
        private static string Month(int y, int m) => y.ToString("0000", C) + "-" + m.ToString("00", C);
        private static long L(object? o) => o == null || o is DBNull ? 0 : Convert.ToInt64(o, C);
        private static int I(object? o) => o == null || o is DBNull ? 0 : o is bool b ? (b ? 1 : 0) : Convert.ToInt32(o, C);
        private static decimal M(object? o) => o == null || o is DBNull ? 0 : Convert.ToDecimal(o, C);
        private static string Str(object? o) => o == null || o is DBNull ? "" : Convert.ToString(o, C) ?? "";
        private static string N(object? o) => R.Amt(M(o));
        private static DateTime DtV(object o) => Convert.ToDateTime(o, C);
        private static string Dt(object? o) => o == null || o is DBNull ? "" : R.D(DtV(o));
    }
}
