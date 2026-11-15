using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib
{
    /// <summary>
    /// P8.4: طبقة بيانات شذوذ القيود (AI_Anomalies، AI_AnomalyRuns، AI_AnomalyExplains) بنمط AiBriefCLS.
    /// الكتابة على هذه الجداول الجديدة فقط؛ القيود نفسها لا تُقرأ ولا تُكتب هنا (القراءة عبر ReadOnlySqlExecutor فى الماسح).
    /// الحجز والإدراج «مرة واحدة» بإدراج مشروط ذرّى (UPDLOCK, HOLDLOCK) والفهرس الفريد احتياط.
    /// branchIds = null ⇒ بلا قيد فرع (المدير بكل الفروع)؛ قائمة فارغة ⇒ لا شىء.
    /// </summary>
    public class AiAnomalyCLS
    {
        private readonly Func<operations> _newOpp;

        public AiAnomalyCLS(Func<operations> newOpp = null)
        {
            _newOpp = newOpp ?? (() => new operations());
        }

        // ───────────── التشغيلات ─────────────

        private const string RunSelect = @"SELECT r.Id, r.RunKey, r.TriggeredBy, r.FromTs, r.ToTs, r.Status, r.RulesCsv, r.EntriesScanned, r.Found, r.Inserted, r.Truncated,
       r.DurationMs, r.ErrorText, r.RequestedBy, r.CreatedAt, r.UpdatedAt
FROM AI_AnomalyRuns r";

        private static AiAnomalyRunRow ReadRun(DbDataReader r) => new AiAnomalyRunRow
        {
            Id = r.GetInt64(0),
            RunKey = S(r, 1) ?? "",
            TriggeredBy = S(r, 2) ?? "",
            FromTs = D(r, 3),
            ToTs = D(r, 4),
            Status = S(r, 5) ?? "",
            RulesCsv = S(r, 6),
            EntriesScanned = Convert.ToInt32(r.GetValue(7)),
            Found = Convert.ToInt32(r.GetValue(8)),
            Inserted = Convert.ToInt32(r.GetValue(9)),
            Truncated = Convert.ToBoolean(r.GetValue(10)),
            DurationMs = r.IsDBNull(11) ? (int?)null : Convert.ToInt32(r.GetValue(11)),
            ErrorText = S(r, 12),
            RequestedBy = r.IsDBNull(13) ? (long?)null : r.GetInt64(13),
            CreatedAt = D(r, 14),
            UpdatedAt = D(r, 15),
        };

        /// <summary>يحجز RunKey بإدراج صف (Running أو Queued حسب r.Status). يعيد المعرّف الجديد، أو 0 إن سبقه غيره.</summary>
        public long TryReserveRun(AiAnomalyRunRow r, long by)
        {
            long id = r.Id != 0 ? r.Id : CallSnowflakeIdGenerate.GenerateNewId();
            string key = Cut((r.RunKey ?? "").Trim(), 40);
            string status = string.IsNullOrEmpty(r.Status) ? AiAnomalyRunStatus.Running : r.Status;
            int n = _newOpp().db.Database.ExecuteSqlInterpolated($@"
INSERT INTO AI_AnomalyRuns (Id, RunKey, TriggeredBy, FromTs, ToTs, Status, RulesCsv, RequestedBy, CreatedBy, CreatedAt)
SELECT {id}, {key}, {Cut(r.TriggeredBy ?? AiAnomalyTrigger.Schedule, 20)}, {r.FromTs}, {r.ToTs}, {Cut(status, 20)}, {Cut(r.RulesCsv, 200)}, {r.RequestedBy}, {by}, {DateTime.Now}
WHERE NOT EXISTS (SELECT 1 FROM AI_AnomalyRuns WITH (UPDLOCK, HOLDLOCK) WHERE RunKey = {key})");
            if (n == 1) { r.Id = id; r.Status = status; r.RunKey = key; }
            return n == 1 ? id : 0;
        }

        /// <summary>يستولى على تشغيل Running عالق (توقف الخادم) إن لم يتحرك منذ minutes. true لمن نجح وحده.</summary>
        public bool TakeOverStale(long id, int minutes, long by) =>
            _newOpp().db.Database.ExecuteSqlInterpolated($@"
UPDATE AI_AnomalyRuns SET UpdatedBy = {by}, UpdatedAt = {DateTime.Now}
WHERE Id = {id} AND Status = {AiAnomalyRunStatus.Running} AND ISNULL(UpdatedAt, CreatedAt) < {DateTime.Now.AddMinutes(-Math.Max(1, minutes))}") == 1;

        /// <summary>Queued أو Failed ⇒ Running (بدء طلب منتظر أو إعادة تشغيل فاشل). true لمن نجح وحده.</summary>
        public bool MarkRunning(long id, long by) =>
            _newOpp().db.Database.ExecuteSqlInterpolated($@"
UPDATE AI_AnomalyRuns SET Status = {AiAnomalyRunStatus.Running}, ErrorText = NULL, UpdatedBy = {by}, UpdatedAt = {DateTime.Now}
WHERE Id = {id} AND Status IN ({AiAnomalyRunStatus.Queued}, {AiAnomalyRunStatus.Failed})") == 1;

        /// <summary>نتيجة التشغيل. fromTs/toTs = null يبقيان كما هما.</summary>
        public void CompleteRun(long id, string status, DateTime? fromTs, DateTime? toTs, int scanned, int found, int inserted, bool truncated,
            int durationMs, string errorText, string rulesCsv, long by) =>
            // معامل datetime فارغ يُرسل nvarchar فيُفسد ISNULL؛ لذا علم صريح وقيمة غير فارغة دائماً.
            _newOpp().db.Database.ExecuteSqlInterpolated($@"
UPDATE AI_AnomalyRuns SET Status = {Cut(status, 20)},
       FromTs = CASE WHEN {(fromTs.HasValue ? 1 : 0)} = 1 THEN {fromTs ?? DateTime.Now} ELSE FromTs END,
       ToTs = CASE WHEN {(toTs.HasValue ? 1 : 0)} = 1 THEN {toTs ?? DateTime.Now} ELSE ToTs END, EntriesScanned = {scanned}, Found = {found},
       Inserted = {inserted}, Truncated = {truncated}, DurationMs = {durationMs}, ErrorText = {Cut(errorText, 1000)}, RulesCsv = ISNULL({Cut(rulesCsv, 200)}, RulesCsv),
       UpdatedBy = {by}, UpdatedAt = {DateTime.Now}
WHERE Id = {id}");

        public AiAnomalyRunRow GetRun(long id)
        {
            AiAnomalyRunRow found = null;
            Exec(RunSelect + " WHERE r.Id = @id", cmd => Add(cmd, "@id", id), r => found = ReadRun(r));
            return found;
        }

        public AiAnomalyRunRow GetRunByKey(string key)
        {
            AiAnomalyRunRow found = null;
            Exec(RunSelect + " WHERE r.RunKey = @k", cmd => Add(cmd, "@k", (key ?? "").Trim()), r => found = ReadRun(r));
            return found;
        }

        /// <summary>العلامة المائية: أكبر ToTs لتشغيل Done مجدول أو يدوى (التاريخى والاختبارى والفاشل لا يُحسب).</summary>
        public DateTime? LastDoneTo()
        {
            DateTime? found = null;
            Exec(@"SELECT MAX(ToTs) FROM AI_AnomalyRuns WHERE Status = @s AND TriggeredBy IN (@t1, @t2)",
                cmd => { Add(cmd, "@s", AiAnomalyRunStatus.Done); Add(cmd, "@t1", AiAnomalyTrigger.Schedule); Add(cmd, "@t2", AiAnomalyTrigger.Manual); },
                r => found = D(r, 0));
            return found;
        }

        public List<AiAnomalyRunRow> ListQueued()
        {
            var list = new List<AiAnomalyRunRow>();
            Exec(RunSelect + " WHERE r.Status = @s ORDER BY r.CreatedAt, r.Id", cmd => Add(cmd, "@s", AiAnomalyRunStatus.Queued), r => list.Add(ReadRun(r)));
            return list;
        }

        public List<AiAnomalyRunRow> ListRuns(int top)
        {
            var list = new List<AiAnomalyRunRow>();
            int n = Math.Max(1, Math.Min(top, 500));
            Exec("SELECT TOP (" + n.ToString(CultureInfo.InvariantCulture) + ") " + RunSelect.Substring("SELECT ".Length) + " ORDER BY r.CreatedAt DESC, r.Id DESC",
                _ => { }, r => list.Add(ReadRun(r)));
            return list;
        }

        // ───────────── الشذوذ ─────────────

        private const string AnomalySelect = @"SELECT a.Id, a.Entry_Id, a.RuleCode, a.Line_Id, a.Severity, a.Score, a.Amount, a.EntryTotal, a.Account_Id, a.AccountName, a.Branch_Id,
       a.EntryNo, a.EntryDate, a.EntryInsertedAt, a.JournalCode, a.ReferenceType, a.EntryUser_Id, a.DetailsJson, a.Run_Id, a.IsBackfill, a.DetectedAt,
       a.Status, a.ReviewedBy, a.ReviewedAt, a.ReviewNote";

        private static AiAnomalyRow ReadAnomaly(DbDataReader r) => new AiAnomalyRow
        {
            Id = r.GetInt64(0),
            Entry_Id = r.GetInt64(1),
            RuleCode = S(r, 2) ?? "",
            Line_Id = L(r, 3),
            Severity = S(r, 4) ?? AiAnomalySeverity.Info,
            Score = M(r, 5),
            Amount = M(r, 6),
            EntryTotal = M(r, 7),
            Account_Id = L(r, 8),
            AccountName = S(r, 9),
            Branch_Id = r.IsDBNull(10) ? 0 : r.GetInt64(10),
            EntryNo = L(r, 11),
            EntryDate = D(r, 12),
            EntryInsertedAt = D(r, 13),
            JournalCode = r.IsDBNull(14) ? (int?)null : Convert.ToInt32(r.GetValue(14)),
            ReferenceType = r.IsDBNull(15) ? (int?)null : Convert.ToInt32(r.GetValue(15)),
            EntryUser_Id = L(r, 16),
            DetailsJson = S(r, 17),
            Run_Id = r.GetInt64(18),
            IsBackfill = Convert.ToBoolean(r.GetValue(19)),
            DetectedAt = D(r, 20),
            Status = S(r, 21) ?? AiAnomalyStatus.New,
            ReviewedBy = L(r, 22),
            ReviewedAt = D(r, 23),
            ReviewNote = S(r, 24),
        };

        /// <summary>ش17: صف واحد لكل (قيد، قاعدة). true إن أُدرج الآن؛ false إن كان موجوداً (إعادة المسح لا تكرر).</summary>
        public bool InsertIfNew(AiAnomalyRow a)
        {
            long id = a.Id != 0 ? a.Id : CallSnowflakeIdGenerate.GenerateNewId();
            string rule = Cut(a.RuleCode, 30) ?? "";
            DateTime? entryDate = a.EntryDate.HasValue ? a.EntryDate.Value.Date : (DateTime?)null;
            int n = _newOpp().db.Database.ExecuteSqlInterpolated($@"
INSERT INTO AI_Anomalies (Id, Entry_Id, RuleCode, Line_Id, Severity, Score, Amount, EntryTotal, Account_Id, AccountName, Branch_Id, EntryNo, EntryDate,
                          EntryInsertedAt, JournalCode, ReferenceType, EntryUser_Id, DetailsJson, Run_Id, IsBackfill, DetectedAt, Status, CreatedBy, CreatedAt)
SELECT {id}, {a.Entry_Id}, {rule}, {a.Line_Id}, {Cut(a.Severity, 10) ?? AiAnomalySeverity.Info}, {AiSqlParams.Dec(a.Score, 18, 4)}, {AiSqlParams.Dec(a.Amount, 18, 3)},
       {AiSqlParams.Dec(a.EntryTotal, 18, 3)}, {a.Account_Id}, {Cut(a.AccountName, 500)}, {a.Branch_Id}, {a.EntryNo}, {entryDate},
       {a.EntryInsertedAt}, {a.JournalCode}, {a.ReferenceType}, {a.EntryUser_Id}, {a.DetailsJson}, {a.Run_Id}, {a.IsBackfill}, {a.DetectedAt ?? DateTime.Now},
       {AiAnomalyStatus.New}, {0L}, {DateTime.Now}
WHERE NOT EXISTS (SELECT 1 FROM AI_Anomalies WITH (UPDLOCK, HOLDLOCK) WHERE Entry_Id = {a.Entry_Id} AND RuleCode = {rule})");
            if (n == 1) { a.Id = id; a.Status = AiAnomalyStatus.New; }
            return n == 1;
        }

        /// <summary>هل للقيد صف لهذه القاعدة (لتخطى المرشح قبل الإدراج وعدم احتسابه «جديداً»).</summary>
        public HashSet<string> ExistingKeys(IList<long> entryIds)
        {
            var set = new HashSet<string>(StringComparer.Ordinal);
            if (entryIds == null || entryIds.Count == 0) return set;
            foreach (var chunk in Chunks(entryIds.Distinct().ToList(), 500))
                Exec("SELECT Entry_Id, RuleCode FROM AI_Anomalies WHERE Entry_Id IN (" + InList(chunk) + ")", _ => { },
                    r => set.Add(r.GetInt64(0).ToString(CultureInfo.InvariantCulture) + "|" + (S(r, 1) ?? "")));
            return set;
        }

        public List<AiAnomalyRow> ListForEntry(long entryId)
        {
            var list = new List<AiAnomalyRow>();
            Exec(AnomalySelect + " FROM AI_Anomalies a WHERE a.Entry_Id = @e ORDER BY a.DetectedAt, a.Id", cmd => Add(cmd, "@e", entryId), r => list.Add(ReadAnomaly(r)));
            return list;
        }

        /// <summary>
        /// صفحة القيود الشاذة: تجميع بالقيد (قواعده كلها)، خطورة القيد بـ AiAnomalyRules.EntrySeverity، الترتيب بالخطورة ثم الأحدث كشفاً.
        /// الفلاتر على مستوى القيد: الحالة (كل صفوف القيد بنفس الحالة)، القاعدة (للقيد صف بها)، الخطورة (خطورة القيد).
        /// </summary>
        public AiAnomalyPage ListEntries(AiAnomalyFilter f, IList<long> branchIds)
        {
            f = f ?? new AiAnomalyFilter();
            var page = new AiAnomalyPage();
            if (branchIds != null && branchIds.Count == 0) return page;
            var where = new List<string>();
            var binds = new List<Action<DbCommand>>();
            if (!string.IsNullOrWhiteSpace(f.Status)) { where.Add("a.Status = @st"); binds.Add(c => Add(c, "@st", f.Status.Trim())); }
            if (!f.IncludeBackfill) where.Add("a.IsBackfill = 0");
            if (branchIds != null) where.Add("a.Branch_Id IN (" + InList(branchIds) + ")");
            if (f.BranchId != 0) { where.Add("a.Branch_Id = @br"); binds.Add(c => Add(c, "@br", f.BranchId)); }
            if (f.From.HasValue) { where.Add("a.EntryDate >= @fr"); binds.Add(c => Add(c, "@fr", f.From.Value.Date)); }
            if (f.To.HasValue) { where.Add("a.EntryDate <= @to"); binds.Add(c => Add(c, "@to", f.To.Value.Date)); }
            if (f.EntryNo.HasValue) { where.Add("a.EntryNo = @no"); binds.Add(c => Add(c, "@no", f.EntryNo.Value)); }
            if (!string.IsNullOrWhiteSpace(f.RuleCode))
            {
                where.Add("EXISTS (SELECT 1 FROM AI_Anomalies x WHERE x.Entry_Id = a.Entry_Id AND x.RuleCode = @rc)");
                binds.Add(c => Add(c, "@rc", f.RuleCode.Trim()));
            }
            string sql = "SELECT TOP (20000) " + AnomalySelect.Substring("SELECT ".Length) + @",
       ISNULL(b.Branch_Name, N''), ISNULL(u.USER_NAME, N'')
FROM AI_Anomalies a
LEFT JOIN Account_Branches b ON b.Id = a.Branch_Id
LEFT JOIN USER_DETAILES u ON u.Id = a.ReviewedBy" + (where.Count > 0 ? " WHERE " + string.Join(" AND ", where) : "") + " ORDER BY a.DetectedAt DESC, a.Id DESC";
            var rows = new List<(AiAnomalyRow Row, string Branch, string Reviewer)>();
            Exec(sql, cmd => { foreach (var b in binds) b(cmd); }, r => rows.Add((ReadAnomaly(r), S(r, 25) ?? "", S(r, 26) ?? "")));

            var entries = rows.GroupBy(x => x.Row.Entry_Id).Select(g => ToView(g.Select(x => x.Row).ToList(), g.First().Branch, g.First().Reviewer)).ToList();
            if (!string.IsNullOrWhiteSpace(f.Severity))
                entries = entries.Where(e => e.Severity == f.Severity.Trim()).ToList();
            entries = entries.OrderByDescending(e => AiAnomalySeverity.Rank(e.Severity)).ThenByDescending(e => e.DetectedAt).ThenByDescending(e => e.EntryId).ToList();
            page.Total = entries.Count;
            int p = Math.Max(0, f.Page);
            page.Items = entries.Skip(p * AiAnomalyConst.PageSize).Take(AiAnomalyConst.PageSize).ToList();
            return page;
        }

        /// <summary>قيد واحد مجمّع (null إن لم يكن فى القائمة أو خارج الفروع).</summary>
        public AiAnomalyEntryView GetEntryView(long entryId, IList<long> branchIds)
        {
            if (branchIds != null && branchIds.Count == 0) return null;
            var rows = new List<(AiAnomalyRow Row, string Branch, string Reviewer)>();
            Exec(AnomalySelect + @", ISNULL(b.Branch_Name, N''), ISNULL(u.USER_NAME, N'')
FROM AI_Anomalies a
LEFT JOIN Account_Branches b ON b.Id = a.Branch_Id
LEFT JOIN USER_DETAILES u ON u.Id = a.ReviewedBy
WHERE a.Entry_Id = @e" + (branchIds != null ? " AND a.Branch_Id IN (" + InList(branchIds) + ")" : "") + " ORDER BY a.DetectedAt, a.Id",
                cmd => Add(cmd, "@e", entryId), r => rows.Add((ReadAnomaly(r), S(r, 25) ?? "", S(r, 26) ?? "")));
            return rows.Count == 0 ? null : ToView(rows.Select(x => x.Row).ToList(), rows[0].Branch, rows[0].Reviewer);
        }

        private static AiAnomalyEntryView ToView(List<AiAnomalyRow> rules, string branchName, string reviewer)
        {
            var first = rules.OrderBy(r => r.DetectedAt ?? DateTime.MaxValue).First();
            var latestReview = rules.Where(r => r.ReviewedAt.HasValue).OrderByDescending(r => r.ReviewedAt).FirstOrDefault();
            return new AiAnomalyEntryView
            {
                EntryId = first.Entry_Id,
                EntryNo = first.EntryNo,
                EntryDate = first.EntryDate,
                InsertedAt = first.EntryInsertedAt,
                JournalCode = first.JournalCode ?? 0,
                ReferenceType = first.ReferenceType ?? 0,
                BranchId = first.Branch_Id,
                BranchName = branchName,
                EntryTotal = rules.Select(r => r.EntryTotal ?? 0).Max(),
                Severity = AiAnomalyRules.EntrySeverity(rules),
                // كل صفوف القيد بنفس الحالة (SetStatus على مستوى القيد)؛ إن اختلفت لسبب ما فالجديدة تغلب.
                Status = rules.Any(r => r.Status == AiAnomalyStatus.New) ? AiAnomalyStatus.New : (latestReview ?? first).Status,
                IsBackfill = rules.All(r => r.IsBackfill),
                DetectedAt = rules.Max(r => r.DetectedAt ?? DateTime.MinValue),
                ReviewNote = latestReview == null ? null : latestReview.ReviewNote,
                ReviewedBy = latestReview == null ? null : latestReview.ReviewedBy,
                ReviewedByName = latestReview == null ? null : reviewer,
                ReviewedAt = latestReview == null ? null : latestReview.ReviewedAt,
                Rules = rules.OrderByDescending(r => AiAnomalySeverity.Rank(r.Severity)).ThenBy(r => Array.IndexOf(AiAnomalyRuleCodes.All, r.RuleCode)).ToList(),
            };
        }

        /// <summary>ش19: حالة المراجعة لكل قواعد القيود المعطاة داخل فروع المستخدم فقط. يعيد عدد الصفوف المحدَّثة.</summary>
        public int SetStatus(IList<long> entryIds, string status, string note, long by, IList<long> branchIds)
        {
            if (entryIds == null || entryIds.Count == 0 || !AiAnomalyStatus.IsKnown(status)) return 0;
            if (branchIds != null && branchIds.Count == 0) return 0;
            int total = 0;
            foreach (var chunk in Chunks(entryIds.Distinct().ToList(), 500))
            {
                string sql = "UPDATE AI_Anomalies SET Status = @st, ReviewedBy = @by, ReviewedAt = @at, ReviewNote = @note, UpdatedBy = @by, UpdatedAt = @at " +
                             "WHERE Entry_Id IN (" + InList(chunk) + ")" + (branchIds != null ? " AND Branch_Id IN (" + InList(branchIds) + ")" : "");
                total += NonQuery(sql, cmd =>
                {
                    Add(cmd, "@st", status);
                    Add(cmd, "@by", by);
                    Add(cmd, "@at", DateTime.Now);
                    Add(cmd, "@note", string.IsNullOrWhiteSpace(note) ? null : Cut(note.Trim(), 500));
                });
            }
            return total;
        }

        /// <summary>مؤشرات الصفحة. Enabled تملؤه البوابة.</summary>
        public AiAnomalySummary Summary(IList<long> branchIds)
        {
            var s = new AiAnomalySummary();
            if (branchIds == null || branchIds.Count > 0)
            {
                string br = branchIds != null ? " AND Branch_Id IN (" + InList(branchIds) + ")" : "";
                var open = new List<AiAnomalyRow>();
                Exec("SELECT Entry_Id, RuleCode, Severity FROM AI_Anomalies WHERE Status = @st AND IsBackfill = 0" + br,
                    cmd => Add(cmd, "@st", AiAnomalyStatus.New),
                    r => open.Add(new AiAnomalyRow { Entry_Id = r.GetInt64(0), RuleCode = S(r, 1) ?? "", Severity = S(r, 2) ?? "" }));
                var groups = open.GroupBy(x => x.Entry_Id).ToList();
                s.NewCount = groups.Count;
                s.HighCount = groups.Count(g => AiAnomalyRules.EntrySeverity(g) == AiAnomalySeverity.High);
                var monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                Exec("SELECT COUNT(DISTINCT Entry_Id) FROM AI_Anomalies WHERE Status IN (@r1, @r2) AND ReviewedAt >= @m" + br,
                    cmd => { Add(cmd, "@r1", AiAnomalyStatus.Reviewed); Add(cmd, "@r2", AiAnomalyStatus.Dismissed); Add(cmd, "@m", monthStart); },
                    r => s.ReviewedThisMonth = Convert.ToInt32(r.GetValue(0)));
            }
            Exec(@"SELECT TOP (1) ISNULL(UpdatedAt, CreatedAt), Status FROM AI_AnomalyRuns WHERE Status IN (@d, @f) AND TriggeredBy <> @t ORDER BY ISNULL(UpdatedAt, CreatedAt) DESC",
                cmd => { Add(cmd, "@d", AiAnomalyRunStatus.Done); Add(cmd, "@f", AiAnomalyRunStatus.Failed); Add(cmd, "@t", AiAnomalyTrigger.Test); },
                r => { s.LastRunAt = D(r, 0); s.LastRunStatus = S(r, 1); });
            return s;
        }

        /// <summary>
        /// ش27: أعداد تنبيه الموجز — غير المراجَع وغير التاريخى فى فروع النطاق. NewCount/HighCount لما كُشف منذ since، OpenTotal لكل المفتوح،
        /// Top = أعلى 5 خطورةً مما كُشف منذ since.
        /// </summary>
        public AiAnomalyBriefCounts BriefCounts(IList<long> branchIds, DateTime since)
        {
            var c = new AiAnomalyBriefCounts();
            if (branchIds != null && branchIds.Count == 0) return c;
            string br = branchIds != null ? " AND Branch_Id IN (" + InList(branchIds) + ")" : "";
            var open = new List<AiAnomalyRow>();
            Exec("SELECT Entry_Id, RuleCode, Severity, DetectedAt, EntryNo, AccountName, Line_Id FROM AI_Anomalies WHERE Status = @st AND IsBackfill = 0" + br,
                cmd => Add(cmd, "@st", AiAnomalyStatus.New),
                r => open.Add(new AiAnomalyRow
                {
                    Entry_Id = r.GetInt64(0), RuleCode = S(r, 1) ?? "", Severity = S(r, 2) ?? "", DetectedAt = D(r, 3), EntryNo = L(r, 4),
                    AccountName = S(r, 5), Line_Id = L(r, 6),
                }));
            var groups = open.GroupBy(x => x.Entry_Id).ToList();
            c.OpenTotal = groups.Count;
            var recent = groups.Where(g => g.Max(x => x.DetectedAt ?? DateTime.MinValue) >= since).ToList();
            c.NewCount = recent.Count;
            var withSev = recent.Select(g => new { G = g, Sev = AiAnomalyRules.EntrySeverity(g) }).ToList();
            c.HighCount = withSev.Count(x => x.Sev == AiAnomalySeverity.High);
            c.Top = withSev.OrderByDescending(x => AiAnomalySeverity.Rank(x.Sev)).ThenByDescending(x => x.G.Max(y => y.DetectedAt ?? DateTime.MinValue))
                .Take(5).Select(x => new AiAnomalyTopItem
                {
                    EntryId = x.G.Key,
                    EntryNo = x.G.First().EntryNo,
                    AccountName = x.G.OrderByDescending(y => AiAnomalySeverity.Rank(y.Severity)).Select(y => y.AccountName).FirstOrDefault(n => !string.IsNullOrWhiteSpace(n)),
                    Severity = x.Sev,
                }).ToList();
            return c;
        }

        // ───────────── الشرح ─────────────

        private const string ExplainSelect = @"SELECT e.Id, e.Entry_Id, e.Language, e.ExplainText, e.Source, e.Model_Id, e.ModelName, e.InputTokens, e.OutputTokens,
       e.CostEstimate, e.RejectReason, e.CreatedAt, e.UpdatedAt
FROM AI_AnomalyExplains e";

        public AiAnomalyExplainRow GetExplain(long entryId, string language)
        {
            AiAnomalyExplainRow found = null;
            Exec(ExplainSelect + " WHERE e.Entry_Id = @e AND e.Language = @l",
                cmd => { Add(cmd, "@e", entryId); Add(cmd, "@l", language ?? ""); },
                r => found = new AiAnomalyExplainRow
                {
                    Id = r.GetInt64(0),
                    Entry_Id = r.GetInt64(1),
                    Language = S(r, 2) ?? "",
                    ExplainText = S(r, 3),
                    Source = S(r, 4) ?? "",
                    Model_Id = L(r, 5),
                    ModelName = S(r, 6),
                    InputTokens = r.IsDBNull(7) ? (int?)null : Convert.ToInt32(r.GetValue(7)),
                    OutputTokens = r.IsDBNull(8) ? (int?)null : Convert.ToInt32(r.GetValue(8)),
                    CostEstimate = M(r, 9),
                    RejectReason = S(r, 10),
                    CreatedAt = D(r, 11),
                    UpdatedAt = D(r, 12),
                });
            return found;
        }

        /// <summary>شرح (قيد، لغة): تحديث الموجود وإلا إدراج — صف واحد دائماً لكل لغة.</summary>
        public void SaveExplain(AiAnomalyExplainRow e, long by)
        {
            var db = _newOpp().db;
            string lang = Cut(e.Language, 5) ?? "";
            int updated = db.Database.ExecuteSqlInterpolated($@"
UPDATE AI_AnomalyExplains SET ExplainText = {e.ExplainText}, Source = {Cut(e.Source, 20)}, Model_Id = {e.Model_Id}, ModelName = {Cut(e.ModelName, 150)},
       InputTokens = {e.InputTokens}, OutputTokens = {e.OutputTokens}, CostEstimate = {AiSqlParams.Cost(e.CostEstimate)}, RejectReason = {Cut(e.RejectReason, 300)},
       UpdatedBy = {by}, UpdatedAt = {DateTime.Now}
WHERE Entry_Id = {e.Entry_Id} AND Language = {lang}");
            if (updated > 0) return;
            long id = e.Id != 0 ? e.Id : CallSnowflakeIdGenerate.GenerateNewId();
            db.Database.ExecuteSqlInterpolated($@"
INSERT INTO AI_AnomalyExplains (Id, Entry_Id, Language, ExplainText, Source, Model_Id, ModelName, InputTokens, OutputTokens, CostEstimate, RejectReason, CreatedBy, CreatedAt)
SELECT {id}, {e.Entry_Id}, {lang}, {e.ExplainText}, {Cut(e.Source, 20)}, {e.Model_Id}, {Cut(e.ModelName, 150)}, {e.InputTokens}, {e.OutputTokens},
       {AiSqlParams.Cost(e.CostEstimate)}, {Cut(e.RejectReason, 300)}, {by}, {DateTime.Now}
WHERE NOT EXISTS (SELECT 1 FROM AI_AnomalyExplains WITH (UPDLOCK, HOLDLOCK) WHERE Entry_Id = {e.Entry_Id} AND Language = {lang})");
            e.Id = id;
        }

        /// <summary>عدد استدعاءات النموذج للشرح منذ لحظة (للحد اليومى): model / rejected / error فقط (capped/off بلا استدعاء).</summary>
        public int ExplainCountSince(DateTime from)
        {
            int n = 0;
            Exec("SELECT COUNT(*) FROM AI_AnomalyExplains WHERE Source IN (@a, @b, @c) AND ISNULL(UpdatedAt, CreatedAt) >= @f",
                cmd =>
                {
                    Add(cmd, "@a", AiBriefNarrativeSource.Model); Add(cmd, "@b", AiBriefNarrativeSource.Rejected);
                    Add(cmd, "@c", AiBriefNarrativeSource.Error); Add(cmd, "@f", from);
                }, r => n = Convert.ToInt32(r.GetValue(0)));
            return n;
        }

        // ───────────── مساعدات ─────────────

        private static string S(DbDataReader r, int i) => r.IsDBNull(i) ? null : Convert.ToString(r.GetValue(i));
        private static DateTime? D(DbDataReader r, int i) => r.IsDBNull(i) ? (DateTime?)null : Convert.ToDateTime(r.GetValue(i));
        private static long? L(DbDataReader r, int i) => r.IsDBNull(i) ? (long?)null : Convert.ToInt64(r.GetValue(i));
        private static decimal? M(DbDataReader r, int i) => r.IsDBNull(i) ? (decimal?)null : Convert.ToDecimal(r.GetValue(i));

        private static string Cut(string s, int max) => s == null ? null : (s.Length > max ? s.Substring(0, max) : s);

        /// <summary>قائمة أرقام آمنة للحقن (long فقط بصيغة ثابتة).</summary>
        private static string InList(IEnumerable<long> ids)
        {
            var list = (ids ?? Enumerable.Empty<long>()).Distinct().Select(x => x.ToString(CultureInfo.InvariantCulture)).ToList();
            return list.Count == 0 ? "NULL" : string.Join(",", list);
        }

        private static IEnumerable<List<long>> Chunks(List<long> ids, int size)
        {
            for (int i = 0; i < ids.Count; i += size) yield return ids.Skip(i).Take(size).ToList();
        }

        private static void Add(DbCommand cmd, string name, object value)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }

        private void Exec(string sql, Action<DbCommand> bind, Action<DbDataReader> row)
        {
            var db = _newOpp().db;
            var conn = db.Database.GetDbConnection();
            bool wasClosed = conn.State != ConnectionState.Open;
            if (wasClosed) conn.Open();
            try
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    bind(cmd);
                    using (var r = cmd.ExecuteReader())
                        while (r.Read()) row(r);
                }
            }
            finally { if (wasClosed) conn.Close(); }
        }

        private int NonQuery(string sql, Action<DbCommand> bind)
        {
            var db = _newOpp().db;
            var conn = db.Database.GetDbConnection();
            bool wasClosed = conn.State != ConnectionState.Open;
            if (wasClosed) conn.Open();
            try
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    bind(cmd);
                    return cmd.ExecuteNonQuery();
                }
            }
            finally { if (wasClosed) conn.Close(); }
        }
    }
}
