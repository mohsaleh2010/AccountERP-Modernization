using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib
{
    /// <summary>
    /// P8.1: طبقة بيانات الموجز اليومى (AI_Briefs، AI_BriefNarratives، AI_BriefDeliveries، AI_BriefSubscriptions) بنمط AiFeedbackCLS.
    /// كل الكتابة على هذه الجداول الجديدة فقط. الحجز والإدراج «مرة واحدة» بإدراج مشروط ذرّى (UPDLOCK, HOLDLOCK) والفهرس الفريد احتياط.
    /// </summary>
    public class AiBriefCLS
    {
        private readonly Func<operations> _newOpp;

        public AiBriefCLS(Func<operations> newOpp = null)
        {
            _newOpp = newOpp ?? (() => new operations());
        }

        // ───────────── الموجزات ─────────────

        private const string BriefSelect = @"SELECT b.Id, b.BriefDate, b.PeriodFrom, b.PeriodTo, b.ScopeKey, b.BranchIdsCsv, b.SectionsCsv, b.TaxFilter, b.FactsJson, b.AlertsJson,
       b.AlertCount, b.ErrorsJson, b.Status, b.CollectorVersion, b.DurationMs, b.TriggeredBy, b.CreatedAt, b.UpdatedAt
FROM AI_Briefs b";

        private static AiBriefRow ReadBrief(DbDataReader r) => new AiBriefRow
        {
            Id = r.GetInt64(0),
            BriefDate = r.GetDateTime(1),
            PeriodFrom = r.GetDateTime(2),
            PeriodTo = r.GetDateTime(3),
            ScopeKey = (S(r, 4) ?? "").Trim(),
            BranchIdsCsv = S(r, 5),
            SectionsCsv = S(r, 6),
            TaxFilter = Convert.ToInt32(r.GetValue(7)),
            FactsJson = S(r, 8),
            AlertsJson = S(r, 9),
            AlertCount = Convert.ToInt32(r.GetValue(10)),
            ErrorsJson = S(r, 11),
            Status = S(r, 12) ?? "",
            CollectorVersion = Convert.ToInt32(r.GetValue(13)),
            DurationMs = r.IsDBNull(14) ? (int?)null : Convert.ToInt32(r.GetValue(14)),
            TriggeredBy = S(r, 15),
            CreatedAt = r.IsDBNull(16) ? (DateTime?)null : r.GetDateTime(16),
            UpdatedAt = r.IsDBNull(17) ? (DateTime?)null : r.GetDateTime(17),
        };

        /// <summary>
        /// يحجز (BriefDate, ScopeKey) بإدراج صف Running. يعيد معرّف الصف الجديد، أو 0 إن سبقه غيره (نسخة خادم أخرى أو نبضة سابقة).
        /// </summary>
        public long TryReserve(AiBriefRow b, long userId)
        {
            long id = b.Id != 0 ? b.Id : CallSnowflakeIdGenerate.GenerateNewId();
            DateTime day = b.BriefDate.Date;
            string key = (b.ScopeKey ?? "").Trim();
            int n = _newOpp().db.Database.ExecuteSqlInterpolated($@"
INSERT INTO AI_Briefs (Id, BriefDate, PeriodFrom, PeriodTo, ScopeKey, BranchIdsCsv, SectionsCsv, TaxFilter, Status, CollectorVersion, TriggeredBy, CreatedBy, CreatedAt)
SELECT {id}, {day}, {b.PeriodFrom.Date}, {b.PeriodTo.Date}, {key}, {Cut(b.BranchIdsCsv, 500)}, {Cut(b.SectionsCsv, 200)}, {b.TaxFilter}, {AiBriefStatus.Running},
       {b.CollectorVersion}, {Cut(b.TriggeredBy, 20)}, {userId}, {DateTime.Now}
WHERE NOT EXISTS (SELECT 1 FROM AI_Briefs WITH (UPDLOCK, HOLDLOCK) WHERE BriefDate = {day} AND ScopeKey = {key})");
            if (n == 1) b.Id = id;
            return n == 1 ? id : 0;
        }

        /// <summary>
        /// يستولى على صف Running عالق (توقف الخادم أثناء الجمع) إن لم يتحرك منذ staleMinutes. يعيد true لمن نجح وحده.
        /// </summary>
        public bool TakeOverStale(long id, int staleMinutes, long userId) =>
            _newOpp().db.Database.ExecuteSqlInterpolated($@"
UPDATE AI_Briefs SET UpdatedBy = {userId}, UpdatedAt = {DateTime.Now}
WHERE Id = {id} AND Status = {AiBriefStatus.Running} AND ISNULL(UpdatedAt, CreatedAt) < {DateTime.Now.AddMinutes(-Math.Max(1, staleMinutes))}") == 1;

        /// <summary>نتيجة الجمع (أو إعادته عند «تحديث الآن»): الحقائق والتنبيهات والأخطاء والحالة النهائية.</summary>
        public void Complete(long id, string factsJson, string alertsJson, int alertCount, string errorsJson, string status, int durationMs, long userId) =>
            _newOpp().db.Database.ExecuteSqlInterpolated($@"
UPDATE AI_Briefs SET FactsJson = {factsJson}, AlertsJson = {alertsJson}, AlertCount = {alertCount}, ErrorsJson = {errorsJson}, Status = {status},
       DurationMs = {durationMs}, UpdatedBy = {userId}, UpdatedAt = {DateTime.Now}
WHERE Id = {id}");

        /// <summary>«تحديث الآن»: يعيد الصف إلى Running ليجمع من جديد (الصف نفسه؛ لا صف جديد ولا حذف).</summary>
        public bool MarkRunning(long id, string triggeredBy, long userId) =>
            _newOpp().db.Database.ExecuteSqlInterpolated($@"
UPDATE AI_Briefs SET Status = {AiBriefStatus.Running}, TriggeredBy = {Cut(triggeredBy, 20)}, UpdatedBy = {userId}, UpdatedAt = {DateTime.Now}
WHERE Id = {id} AND Status <> {AiBriefStatus.Running}") == 1;

        public AiBriefRow Get(long id)
        {
            AiBriefRow found = null;
            Exec(BriefSelect + " WHERE b.Id = @id", cmd => Add(cmd, "@id", id), r => found = ReadBrief(r));
            return found;
        }

        public AiBriefRow GetByScope(DateTime briefDate, string scopeKey)
        {
            AiBriefRow found = null;
            Exec(BriefSelect + " WHERE b.BriefDate = @d AND b.ScopeKey = @k",
                cmd => { Add(cmd, "@d", briefDate.Date); Add(cmd, "@k", (scopeKey ?? "").Trim()); }, r => found = ReadBrief(r));
            return found;
        }

        /// <summary>آخر يوم غطّاه موجز صادر (Ready/Partial) لهذا النطاق قبل اليوم المعطى — لتغطية الفجوة (ق14).</summary>
        public DateTime? LastCoveredDate(string scopeKey, DateTime beforeBriefDate)
        {
            DateTime? found = null;
            Exec(@"SELECT MAX(PeriodTo) FROM AI_Briefs WHERE ScopeKey = @k AND BriefDate < @d AND Status IN (N'Ready', N'Partial')",
                cmd => { Add(cmd, "@k", (scopeKey ?? "").Trim()); Add(cmd, "@d", beforeBriefDate.Date); },
                r => { if (!r.IsDBNull(0)) found = r.GetDateTime(0); });
            return found;
        }

        public List<AiBriefRunView> ListRuns(int top)
        {
            var list = new List<AiBriefRunView>();
            Exec($@"SELECT TOP ({Math.Max(1, Math.Min(top, 500))}) b.Id, b.BriefDate, b.PeriodFrom, b.PeriodTo, b.Status, b.TriggeredBy, b.AlertCount, b.DurationMs, b.ErrorsJson, b.CreatedAt,
       (SELECT COUNT(*) FROM AI_BriefDeliveries d WHERE d.Brief_Id = b.Id) AS Recipients,
       ISNULL((SELECT STRING_AGG(n.Language + N':' + n.Source, N', ') FROM AI_BriefNarratives n WHERE n.Brief_Id = b.Id), N'') AS Sources,
       ISNULL((SELECT SUM(ISNULL(n.CostEstimate, 0)) FROM AI_BriefNarratives n WHERE n.Brief_Id = b.Id), 0) AS Cost
FROM AI_Briefs b ORDER BY b.CreatedAt DESC", _ => { }, r => list.Add(new AiBriefRunView
            {
                BriefId = r.GetInt64(0),
                BriefDate = r.GetDateTime(1),
                PeriodFrom = r.GetDateTime(2),
                PeriodTo = r.GetDateTime(3),
                Status = S(r, 4) ?? "",
                TriggeredBy = S(r, 5) ?? "",
                AlertCount = Convert.ToInt32(r.GetValue(6)),
                DurationMs = r.IsDBNull(7) ? (int?)null : Convert.ToInt32(r.GetValue(7)),
                ErrorsJson = S(r, 8),
                CreatedAt = r.IsDBNull(9) ? (DateTime?)null : r.GetDateTime(9),
                Recipients = Convert.ToInt32(r.GetValue(10)),
                NarrativeSources = S(r, 11) ?? "",
                NarrativeCost = Convert.ToDecimal(r.GetValue(12)),
            }));
            return list;
        }

        // ───────────── السرد ─────────────

        private const string NarrativeSelect = @"SELECT n.Id, n.Brief_Id, n.Language, n.NarrativeText, n.QuestionsJson, n.Source, n.Model_Id, n.ModelName, n.InputTokens, n.OutputTokens,
       n.CostEstimate, n.RejectReason, n.CreatedAt, n.UpdatedAt
FROM AI_BriefNarratives n";

        private static AiBriefNarrativeRow ReadNarrative(DbDataReader r) => new AiBriefNarrativeRow
        {
            Id = r.GetInt64(0),
            Brief_Id = r.GetInt64(1),
            Language = S(r, 2) ?? "",
            NarrativeText = S(r, 3),
            QuestionsJson = S(r, 4),
            Source = S(r, 5) ?? "",
            Model_Id = r.IsDBNull(6) ? (long?)null : r.GetInt64(6),
            ModelName = S(r, 7),
            InputTokens = r.IsDBNull(8) ? (int?)null : Convert.ToInt32(r.GetValue(8)),
            OutputTokens = r.IsDBNull(9) ? (int?)null : Convert.ToInt32(r.GetValue(9)),
            CostEstimate = r.IsDBNull(10) ? (decimal?)null : Convert.ToDecimal(r.GetValue(10)),
            RejectReason = S(r, 11),
            CreatedAt = r.IsDBNull(12) ? (DateTime?)null : r.GetDateTime(12),
            UpdatedAt = r.IsDBNull(13) ? (DateTime?)null : r.GetDateTime(13),
        };

        public AiBriefNarrativeRow GetNarrative(long briefId, string language)
        {
            AiBriefNarrativeRow found = null;
            Exec(NarrativeSelect + " WHERE n.Brief_Id = @b AND n.Language = @l",
                cmd => { Add(cmd, "@b", briefId); Add(cmd, "@l", language ?? ""); }, r => found = ReadNarrative(r));
            return found;
        }

        public List<AiBriefNarrativeRow> ListNarratives(long briefId)
        {
            var list = new List<AiBriefNarrativeRow>();
            Exec(NarrativeSelect + " WHERE n.Brief_Id = @b ORDER BY n.Language", cmd => Add(cmd, "@b", briefId), r => list.Add(ReadNarrative(r)));
            return list;
        }

        /// <summary>يحفظ سرد (موجز، لغة): تحديث الموجود وإلا إدراج — صف واحد دائماً لكل لغة.</summary>
        public void SaveNarrative(AiBriefNarrativeRow n, long userId)
        {
            var db = _newOpp().db;
            string lang = Cut(n.Language, 5) ?? "";
            int updated = db.Database.ExecuteSqlInterpolated($@"
UPDATE AI_BriefNarratives SET NarrativeText = {n.NarrativeText}, QuestionsJson = {Cut(n.QuestionsJson, 1000)}, Source = {Cut(n.Source, 20)}, Model_Id = {n.Model_Id},
       ModelName = {Cut(n.ModelName, 150)}, InputTokens = {n.InputTokens}, OutputTokens = {n.OutputTokens}, CostEstimate = {Money(n.CostEstimate)},
       RejectReason = {Cut(n.RejectReason, 300)}, UpdatedBy = {userId}, UpdatedAt = {DateTime.Now}
WHERE Brief_Id = {n.Brief_Id} AND Language = {lang}");
            if (updated > 0) return;
            long id = n.Id != 0 ? n.Id : CallSnowflakeIdGenerate.GenerateNewId();
            db.Database.ExecuteSqlInterpolated($@"
INSERT INTO AI_BriefNarratives (Id, Brief_Id, Language, NarrativeText, QuestionsJson, Source, Model_Id, ModelName, InputTokens, OutputTokens, CostEstimate, RejectReason, CreatedBy, CreatedAt)
SELECT {id}, {n.Brief_Id}, {lang}, {n.NarrativeText}, {Cut(n.QuestionsJson, 1000)}, {Cut(n.Source, 20)}, {n.Model_Id}, {Cut(n.ModelName, 150)}, {n.InputTokens}, {n.OutputTokens},
       {Money(n.CostEstimate)}, {Cut(n.RejectReason, 300)}, {userId}, {DateTime.Now}
WHERE NOT EXISTS (SELECT 1 FROM AI_BriefNarratives WITH (UPDLOCK, HOLDLOCK) WHERE Brief_Id = {n.Brief_Id} AND Language = {lang})");
            n.Id = id;
        }

        /// <summary>تكلفة السرد منذ تاريخ (للحد الشهرى).</summary>
        public decimal NarrativeCostSince(DateTime from)
        {
            decimal total = 0;
            Exec("SELECT ISNULL(SUM(ISNULL(CostEstimate, 0)), 0) FROM AI_BriefNarratives WHERE ISNULL(UpdatedAt, CreatedAt) >= @f",
                cmd => Add(cmd, "@f", from), r => total = Convert.ToDecimal(r.GetValue(0)));
            return total;
        }

        // ───────────── التسليم ─────────────

        private const string DeliverySelect = @"SELECT d.Id, d.Brief_Id, d.User_Id, d.Language, d.ReadAt, d.HomeDismissedAt, d.Conversation_Id, d.CreatedAt,
       b.BriefDate, b.PeriodFrom, b.PeriodTo, b.AlertCount, b.Status
FROM AI_BriefDeliveries d INNER JOIN AI_Briefs b ON b.Id = d.Brief_Id";

        private static AiBriefDeliveryRow ReadDelivery(DbDataReader r) => new AiBriefDeliveryRow
        {
            Id = r.GetInt64(0),
            Brief_Id = r.GetInt64(1),
            User_Id = r.GetInt64(2),
            Language = S(r, 3) ?? "",
            ReadAt = r.IsDBNull(4) ? (DateTime?)null : r.GetDateTime(4),
            HomeDismissedAt = r.IsDBNull(5) ? (DateTime?)null : r.GetDateTime(5),
            Conversation_Id = r.IsDBNull(6) ? (long?)null : r.GetInt64(6),
            CreatedAt = r.IsDBNull(7) ? (DateTime?)null : r.GetDateTime(7),
            BriefDate = r.GetDateTime(8),
            PeriodFrom = r.GetDateTime(9),
            PeriodTo = r.GetDateTime(10),
            AlertCount = Convert.ToInt32(r.GetValue(11)),
            BriefStatus = S(r, 12) ?? "",
        };

        /// <summary>يسلّم الموجز لمستلم مرة واحدة. يعيد معرّف التسليم الجديد، أو 0 إن كان مسلّماً من قبل.</summary>
        public long AddDelivery(long briefId, long userId, string language, long createdBy)
        {
            long id = CallSnowflakeIdGenerate.GenerateNewId();
            int n = _newOpp().db.Database.ExecuteSqlInterpolated($@"
INSERT INTO AI_BriefDeliveries (Id, Brief_Id, User_Id, Language, CreatedBy, CreatedAt)
SELECT {id}, {briefId}, {userId}, {Cut(language, 5) ?? ""}, {createdBy}, {DateTime.Now}
WHERE NOT EXISTS (SELECT 1 FROM AI_BriefDeliveries WITH (UPDLOCK, HOLDLOCK) WHERE Brief_Id = {briefId} AND User_Id = {userId})");
            return n == 1 ? id : 0;
        }

        public AiBriefDeliveryRow GetDelivery(long briefId, long userId)
        {
            AiBriefDeliveryRow found = null;
            Exec(DeliverySelect + " WHERE d.Brief_Id = @b AND d.User_Id = @u",
                cmd => { Add(cmd, "@b", briefId); Add(cmd, "@u", userId); }, r => found = ReadDelivery(r));
            return found;
        }

        public AiBriefDeliveryRow GetDeliveryById(long deliveryId)
        {
            AiBriefDeliveryRow found = null;
            Exec(DeliverySelect + " WHERE d.Id = @id", cmd => Add(cmd, "@id", deliveryId), r => found = ReadDelivery(r));
            return found;
        }

        /// <summary>موجزات المستخدم منذ تاريخ (الأحدث أولاً) — صفحة الموجز والسجل والإشعار عند الدخول.</summary>
        public List<AiBriefDeliveryRow> ListDeliveries(long userId, DateTime fromBriefDate)
        {
            var list = new List<AiBriefDeliveryRow>();
            Exec(DeliverySelect + " WHERE d.User_Id = @u AND b.BriefDate >= @f ORDER BY b.BriefDate DESC, d.CreatedAt DESC",
                cmd => { Add(cmd, "@u", userId); Add(cmd, "@f", fromBriefDate.Date); }, r => list.Add(ReadDelivery(r)));
            return list;
        }

        public int UnreadCount(long userId)
        {
            int n = 0;
            Exec("SELECT COUNT(*) FROM AI_BriefDeliveries WHERE User_Id = @u AND ReadAt IS NULL", cmd => Add(cmd, "@u", userId), r => n = r.GetInt32(0));
            return n;
        }

        public bool MarkRead(long briefId, long userId) =>
            _newOpp().db.Database.ExecuteSqlInterpolated($@"
UPDATE AI_BriefDeliveries SET ReadAt = {DateTime.Now}, UpdatedBy = {userId}, UpdatedAt = {DateTime.Now}
WHERE Brief_Id = {briefId} AND User_Id = {userId} AND ReadAt IS NULL") == 1;

        public bool DismissHome(long briefId, long userId) =>
            _newOpp().db.Database.ExecuteSqlInterpolated($@"
UPDATE AI_BriefDeliveries SET HomeDismissedAt = {DateTime.Now}, UpdatedBy = {userId}, UpdatedAt = {DateTime.Now}
WHERE Brief_Id = {briefId} AND User_Id = {userId} AND HomeDismissedAt IS NULL") == 1;

        public void SetConversation(long briefId, long userId, long conversationId) =>
            _newOpp().db.Database.ExecuteSqlInterpolated($@"
UPDATE AI_BriefDeliveries SET Conversation_Id = {conversationId}, UpdatedBy = {userId}, UpdatedAt = {DateTime.Now}
WHERE Brief_Id = {briefId} AND User_Id = {userId}");

        // ───────────── الاشتراكات ─────────────

        private const string SubSelect = @"SELECT s.Id, s.User_Id, ISNULL(u.USER_NAME, N''), CAST(ISNULL(u.Active, 0) AS bit), s.IsEnabled, s.BranchIdsCsv, s.SectionsCsv, s.Language,
       s.CreatedAt, s.UpdatedAt
FROM AI_BriefSubscriptions s LEFT JOIN USER_DETAILES u ON u.Id = s.User_Id";

        private static AiBriefSubscriptionRow ReadSub(DbDataReader r) => new AiBriefSubscriptionRow
        {
            Id = r.GetInt64(0),
            User_Id = r.GetInt64(1),
            UserName = S(r, 2) ?? "",
            UserActive = Convert.ToBoolean(r.GetValue(3)),
            IsEnabled = Convert.ToBoolean(r.GetValue(4)),
            BranchIdsCsv = S(r, 5),
            SectionsCsv = S(r, 6),
            Language = S(r, 7) ?? "ar-EG",
            CreatedAt = r.IsDBNull(8) ? (DateTime?)null : r.GetDateTime(8),
            UpdatedAt = r.IsDBNull(9) ? (DateTime?)null : r.GetDateTime(9),
        };

        public AiBriefSubscriptionRow GetSubscription(long userId)
        {
            AiBriefSubscriptionRow found = null;
            Exec(SubSelect + " WHERE s.User_Id = @u", cmd => Add(cmd, "@u", userId), r => found = ReadSub(r));
            return found;
        }

        /// <summary>اشتراك المستخدم: تحديث الموجود وإلا إدراج (صف واحد لكل مستخدم).</summary>
        public void SaveSubscription(AiBriefSubscriptionRow s, long by)
        {
            var db = _newOpp().db;
            string lang = AiBriefConst.NormalizeLanguage(s.Language);
            int updated = db.Database.ExecuteSqlInterpolated($@"
UPDATE AI_BriefSubscriptions SET IsEnabled = {s.IsEnabled}, BranchIdsCsv = {Cut(s.BranchIdsCsv, 500)}, SectionsCsv = {Cut(s.SectionsCsv, 200)}, Language = {lang},
       UpdatedBy = {by}, UpdatedAt = {DateTime.Now}
WHERE User_Id = {s.User_Id}");
            if (updated > 0) return;
            long id = s.Id != 0 ? s.Id : CallSnowflakeIdGenerate.GenerateNewId();
            db.Database.ExecuteSqlInterpolated($@"
INSERT INTO AI_BriefSubscriptions (Id, User_Id, IsEnabled, BranchIdsCsv, SectionsCsv, Language, CreatedBy, CreatedAt)
SELECT {id}, {s.User_Id}, {s.IsEnabled}, {Cut(s.BranchIdsCsv, 500)}, {Cut(s.SectionsCsv, 200)}, {lang}, {by}, {DateTime.Now}
WHERE NOT EXISTS (SELECT 1 FROM AI_BriefSubscriptions WITH (UPDLOCK, HOLDLOCK) WHERE User_Id = {s.User_Id})");
            s.Id = id;
        }

        public List<AiBriefSubscriptionRow> ListSubscriptions(bool enabledOnly)
        {
            var list = new List<AiBriefSubscriptionRow>();
            Exec(SubSelect + (enabledOnly ? " WHERE s.IsEnabled = 1" : "") + " ORDER BY u.USER_NAME", _ => { }, r => list.Add(ReadSub(r)));
            return list;
        }

        // ───────────── مساعدات ─────────────

        private static string S(DbDataReader r, int i) => r.IsDBNull(i) ? null : Convert.ToString(r.GetValue(i));

        private static string Cut(string s, int max) => s == null ? null : (s.Length > max ? s.Substring(0, max) : s);

        /// <summary>تكلفة بدقة 6 خانات: المعامل العشرى غير المحدد فى ExecuteSqlInterpolated يُرسل decimal(18,2) فتصير 0.003 صفراً.</summary>
        private static Microsoft.Data.SqlClient.SqlParameter Money(decimal? value) =>
            new Microsoft.Data.SqlClient.SqlParameter { SqlDbType = SqlDbType.Decimal, Precision = 18, Scale = 6, Value = (object)value ?? DBNull.Value };

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
    }
}
