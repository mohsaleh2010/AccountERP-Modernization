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
    /// P8.5: طبقة بيانات أولويات التحصيل (AI_CollectionActions، AI_CollectionDrafts) بنمط AiAnomalyCLS.
    /// الكتابة على هذين الجدولين فقط؛ SalesRep_Notify_Outbox يُقرأ هنا (حالة الإرسال) ولا يُكتب.
    /// </summary>
    public class AiCollectCLS
    {
        private readonly Func<operations> _newOpp;

        public AiCollectCLS(Func<operations> newOpp = null)
        {
            _newOpp = newOpp ?? (() => new operations());
        }

        // ───────────── الإجراءات ─────────────

        private const string ActionCols = @"a.Id, a.Customer_Id, a.ActionType, a.Status, a.Note, a.PromiseDate, a.PromiseAmount, a.NextActionDate, a.Channel, a.MessageText,
       a.MessageSource, a.Language, a.DunningLevel, a.Outbox_Id, a.BalanceAt, a.OverdueAt, a.OldestDaysAt, a.ScoreAt, a.Branch_Id, a.ClosedBy, a.ClosedAt,
       a.CreatedBy, a.CreatedAt";

        private static AiCollectActionRow ReadAction(DbDataReader r) => new AiCollectActionRow
        {
            Id = r.GetInt64(0),
            Customer_Id = r.GetInt64(1),
            ActionType = S(r, 2) ?? "",
            Status = S(r, 3) ?? "",
            Note = S(r, 4),
            PromiseDate = D(r, 5),
            PromiseAmount = M(r, 6),
            NextActionDate = D(r, 7),
            Channel = I(r, 8),
            MessageText = S(r, 9),
            MessageSource = S(r, 10),
            Language = S(r, 11),
            DunningLevel = I(r, 12),
            Outbox_Id = L(r, 13),
            BalanceAt = M(r, 14),
            OverdueAt = M(r, 15),
            OldestDaysAt = I(r, 16),
            ScoreAt = I(r, 17),
            Branch_Id = r.IsDBNull(18) ? 0 : r.GetInt64(18),
            ClosedBy = L(r, 19),
            ClosedAt = D(r, 20),
            CreatedBy = L(r, 21),
            CreatedAt = D(r, 22),
        };

        public long InsertAction(AiCollectActionRow a, long by)
        {
            long id = a.Id != 0 ? a.Id : CallSnowflakeIdGenerate.GenerateNewId();
            DateTime? pd = a.PromiseDate.HasValue ? a.PromiseDate.Value.Date : (DateTime?)null;
            DateTime? nd = a.NextActionDate.HasValue ? a.NextActionDate.Value.Date : (DateTime?)null;
            _newOpp().db.Database.ExecuteSqlInterpolated($@"
INSERT INTO AI_CollectionActions (Id, Customer_Id, ActionType, Status, Note, PromiseDate, PromiseAmount, NextActionDate, Channel, MessageText, MessageSource,
       Language, DunningLevel, Outbox_Id, BalanceAt, OverdueAt, OldestDaysAt, ScoreAt, Branch_Id, CreatedBy, CreatedAt)
VALUES ({id}, {a.Customer_Id}, {Cut(a.ActionType, 20)}, {Cut(a.Status ?? AiCollectActionStatus.Open, 20)}, {Cut(a.Note, 1000)}, {pd}, {AiSqlParams.Dec(a.PromiseAmount, 18, 3)},
       {nd}, {a.Channel}, {Cut(a.MessageText, 1000)}, {Cut(a.MessageSource, 20)}, {Cut(a.Language, 5)}, {a.DunningLevel}, {a.Outbox_Id},
       {AiSqlParams.Dec(a.BalanceAt, 18, 3)}, {AiSqlParams.Dec(a.OverdueAt, 18, 3)}, {a.OldestDaysAt}, {a.ScoreAt}, {a.Branch_Id}, {by}, {DateTime.Now})");
            a.Id = id;
            return id;
        }

        /// <summary>يغلق/يلغى إجراءً مفتوحاً (صفّنا فقط). false إن لم يكن مفتوحاً.</summary>
        public bool CloseAction(long id, string status, long by) =>
            _newOpp().db.Database.ExecuteSqlInterpolated($@"
UPDATE AI_CollectionActions SET Status = {Cut(status, 20)}, ClosedBy = {by}, ClosedAt = {DateTime.Now}, UpdatedBy = {by}, UpdatedAt = {DateTime.Now}
WHERE Id = {id} AND Status = {AiCollectActionStatus.Open}") == 1;

        /// <summary>وعد/تأجيل/نزاع جديد يغلق المفتوح السابق من نفس النوع لنفس العميل (ت18).</summary>
        public int CloseOpenOfType(long customerId, string actionType, long exceptId, long by) =>
            _newOpp().db.Database.ExecuteSqlInterpolated($@"
UPDATE AI_CollectionActions SET Status = {AiCollectActionStatus.Closed}, ClosedBy = {by}, ClosedAt = {DateTime.Now}, UpdatedBy = {by}, UpdatedAt = {DateTime.Now}
WHERE Customer_Id = {customerId} AND ActionType = {actionType} AND Status = {AiCollectActionStatus.Open} AND Id <> {exceptId}");

        public void SetOutboxId(long actionId, long outboxId) =>
            _newOpp().db.Database.ExecuteSqlInterpolated($"UPDATE AI_CollectionActions SET Outbox_Id = {outboxId}, UpdatedAt = {DateTime.Now} WHERE Id = {actionId}");

        public AiCollectActionRow GetAction(long id)
        {
            AiCollectActionRow found = null;
            Exec("SELECT " + ActionCols + " FROM AI_CollectionActions a WHERE a.Id = @id", c => Add(c, "@id", id), r => found = ReadAction(r));
            return found;
        }

        /// <summary>سجل العميل (الأحدث أولاً) مع اسم المستخدم وحالة صف الطابور قراءةً.</summary>
        public List<AiCollectActionRow> ListForCustomer(long customerId, int top)
        {
            var list = new List<AiCollectActionRow>();
            bool hasOutbox = TableExists("SalesRep_Notify_Outbox");
            string sql = "SELECT TOP (" + Math.Max(1, Math.Min(top, 500)).ToString(CultureInfo.InvariantCulture) + ") " + ActionCols + @", ISNULL(u.USER_NAME, N'')" +
                         (hasOutbox ? ", o.Status, o.Last_Error" : ", CAST(NULL AS int), CAST(NULL AS nvarchar(10))") + @"
FROM AI_CollectionActions a LEFT JOIN USER_DETAILES u ON u.Id = a.CreatedBy" +
                         (hasOutbox ? " LEFT JOIN SalesRep_Notify_Outbox o ON o.Id = a.Outbox_Id" : "") +
                         " WHERE a.Customer_Id = @c ORDER BY a.CreatedAt DESC, a.Id DESC";
            Exec(sql, c => Add(c, "@c", customerId), r =>
            {
                var a = ReadAction(r);
                a.UserName = S(r, 23);
                a.OutboxStatus = I(r, 24);
                a.OutboxError = S(r, 25);
                list.Add(a);
            });
            return list;
        }

        /// <summary>
        /// لكل عميل: الوعود والتأجيلات والنزاعات المفتوحة + آخر إجراء + آخر تذكير مرسل (بلا تكرار)، على دفعات 1000.
        /// </summary>
        public Dictionary<long, List<AiCollectActionRow>> OpenStates(IList<long> customerIds)
        {
            var map = new Dictionary<long, List<AiCollectActionRow>>();
            if (customerIds == null || customerIds.Count == 0) return map;
            var ids = customerIds.Distinct().ToList();
            for (int i = 0; i < ids.Count; i += 1000)
            {
                string inList = string.Join(",", ids.Skip(i).Take(1000).Select(x => x.ToString(CultureInfo.InvariantCulture)));
                string sql = "SELECT " + ActionCols + @" FROM AI_CollectionActions a
WHERE a.Customer_Id IN (" + inList + @") AND (
      (a.Status = N'Open' AND a.ActionType IN (N'promise', N'snooze', N'dispute'))
   OR a.Id IN (SELECT TOP (1) WITH TIES x.Id FROM AI_CollectionActions x WHERE x.Customer_Id = a.Customer_Id AND x.Status <> N'Cancelled' ORDER BY x.CreatedAt DESC)
   OR a.Id IN (SELECT TOP (1) WITH TIES y.Id FROM AI_CollectionActions y WHERE y.Customer_Id = a.Customer_Id AND y.ActionType = N'reminder_sent' AND y.Status <> N'Cancelled' ORDER BY y.CreatedAt DESC))";
                Exec(sql, _ => { }, r =>
                {
                    var a = ReadAction(r);
                    if (!map.TryGetValue(a.Customer_Id, out var l)) map[a.Customer_Id] = l = new List<AiCollectActionRow>();
                    if (l.All(x => x.Id != a.Id)) l.Add(a);
                });
            }
            return map;
        }

        public DateTime? LastSentAt(long customerId)
        {
            DateTime? found = null;
            Exec("SELECT MAX(CreatedAt) FROM AI_CollectionActions WHERE Customer_Id = @c AND ActionType = N'reminder_sent' AND Status <> N'Cancelled'",
                c => Add(c, "@c", customerId), r => found = D(r, 0));
            return found;
        }

        // ───────────── المسودات ─────────────

        public AiCollectDraftRow GetDraft(long customerId, string lang)
        {
            AiCollectDraftRow found = null;
            Exec(@"SELECT Id, Customer_Id, Language, DunningLevel, FactsHash, DraftText, Source, Model_Id, ModelName, InputTokens, OutputTokens, CostEstimate, RejectReason,
       CreatedAt, UpdatedAt FROM AI_CollectionDrafts WHERE Customer_Id = @c AND Language = @l",
                c => { Add(c, "@c", customerId); Add(c, "@l", lang ?? ""); },
                r => found = new AiCollectDraftRow
                {
                    Id = r.GetInt64(0), Customer_Id = r.GetInt64(1), Language = S(r, 2) ?? "", DunningLevel = I(r, 3) ?? 0, FactsHash = S(r, 4) ?? "",
                    DraftText = S(r, 5), Source = S(r, 6) ?? "", Model_Id = L(r, 7), ModelName = S(r, 8), InputTokens = I(r, 9), OutputTokens = I(r, 10),
                    CostEstimate = M(r, 11), RejectReason = S(r, 12), CreatedAt = D(r, 13), UpdatedAt = D(r, 14),
                });
            return found;
        }

        /// <summary>(عميل، لغة): تحديث الموجود وإلا إدراج — صف واحد دائماً.</summary>
        public void SaveDraft(AiCollectDraftRow d, long by)
        {
            var db = _newOpp().db;
            string lang = Cut(d.Language, 5) ?? "";
            int updated = db.Database.ExecuteSqlInterpolated($@"
UPDATE AI_CollectionDrafts SET DunningLevel = {d.DunningLevel}, FactsHash = {Cut(d.FactsHash, 64) ?? ""}, DraftText = {d.DraftText}, Source = {Cut(d.Source, 20)},
       Model_Id = {d.Model_Id}, ModelName = {Cut(d.ModelName, 150)}, InputTokens = {d.InputTokens}, OutputTokens = {d.OutputTokens},
       CostEstimate = {AiSqlParams.Cost(d.CostEstimate)}, RejectReason = {Cut(d.RejectReason, 300)}, UpdatedBy = {by}, UpdatedAt = {DateTime.Now}
WHERE Customer_Id = {d.Customer_Id} AND Language = {lang}");
            if (updated > 0) return;
            long id = d.Id != 0 ? d.Id : CallSnowflakeIdGenerate.GenerateNewId();
            db.Database.ExecuteSqlInterpolated($@"
INSERT INTO AI_CollectionDrafts (Id, Customer_Id, Language, DunningLevel, FactsHash, DraftText, Source, Model_Id, ModelName, InputTokens, OutputTokens, CostEstimate,
       RejectReason, CreatedBy, CreatedAt)
SELECT {id}, {d.Customer_Id}, {lang}, {d.DunningLevel}, {Cut(d.FactsHash, 64) ?? ""}, {d.DraftText}, {Cut(d.Source, 20)}, {d.Model_Id}, {Cut(d.ModelName, 150)},
       {d.InputTokens}, {d.OutputTokens}, {AiSqlParams.Cost(d.CostEstimate)}, {Cut(d.RejectReason, 300)}, {by}, {DateTime.Now}
WHERE NOT EXISTS (SELECT 1 FROM AI_CollectionDrafts WITH (UPDLOCK, HOLDLOCK) WHERE Customer_Id = {d.Customer_Id} AND Language = {lang})");
            d.Id = id;
        }

        /// <summary>استدعاءات النموذج منذ لحظة (model / rejected / error) — للحد اليومى.</summary>
        public int DraftCountSince(DateTime from)
        {
            int n = 0;
            Exec("SELECT COUNT(*) FROM AI_CollectionDrafts WHERE Source IN (N'model', N'rejected', N'error') AND ISNULL(UpdatedAt, CreatedAt) >= @f",
                c => Add(c, "@f", from), r => n = Convert.ToInt32(r.GetValue(0)));
            return n;
        }

        // ───────────── مساعدات ─────────────

        private bool TableExists(string name)
        {
            bool found = false;
            Exec("SELECT CASE WHEN OBJECT_ID(@n, N'U') IS NULL THEN 0 ELSE 1 END", c => Add(c, "@n", "dbo." + name), r => found = Convert.ToInt32(r.GetValue(0)) == 1);
            return found;
        }

        private static string S(DbDataReader r, int i) => r.IsDBNull(i) ? null : Convert.ToString(r.GetValue(i), CultureInfo.InvariantCulture);
        private static DateTime? D(DbDataReader r, int i) => r.IsDBNull(i) ? (DateTime?)null : Convert.ToDateTime(r.GetValue(i), CultureInfo.InvariantCulture);
        private static long? L(DbDataReader r, int i) => r.IsDBNull(i) ? (long?)null : Convert.ToInt64(r.GetValue(i), CultureInfo.InvariantCulture);
        private static int? I(DbDataReader r, int i) => r.IsDBNull(i) ? (int?)null : Convert.ToInt32(r.GetValue(i), CultureInfo.InvariantCulture);
        private static decimal? M(DbDataReader r, int i) => r.IsDBNull(i) ? (decimal?)null : Convert.ToDecimal(r.GetValue(i), CultureInfo.InvariantCulture);
        private static string Cut(string s, int max) => s == null ? null : (s.Length > max ? s.Substring(0, max) : s);

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
