using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib
{
    /// <summary>صف AI_Messages كما يُخزَّن؛ ContentJson يفسّره AccountERP.AI (كتل نص الآن، وأدوات فى P2).</summary>
    public class AiMessageRow
    {
        public long Id { get; set; }
        public long Conversation_Id { get; set; }
        public int Seq { get; set; }
        public string Role { get; set; } = "user";
        public string ContentJson { get; set; }
        public long? Model_Id { get; set; }
        public string ModelName { get; set; }
        public int? InputTokens { get; set; }
        public int? OutputTokens { get; set; }
        public int? CacheReadTokens { get; set; }
        public decimal? CostEstimate { get; set; }
        public string Status { get; set; }
        public string ErrorText { get; set; }
        public int? DurationMs { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// طبقة بيانات المحادثات (P1): AI_Conversations / AI_Messages. كل استعلام مقيّد بـ User_Id صاحب المحادثة،
    /// فلا يرى مستخدم محادثة غيره. الحذف لصفوف هذه الجداول الجديدة فقط ولمحادثات المستخدم نفسه.
    /// </summary>
    public class AiChatCLS
    {
        private readonly Func<operations> _newOpp;

        public AiChatCLS(Func<operations> newOpp = null)
        {
            _newOpp = newOpp ?? (() => new operations());
        }

        public const string StatusActive = "Active";
        public const string StatusArchived = "Archived";

        /// <summary>archived = false: النشطة (الافتراضى)؛ true: المؤرشفة (بعد مدة الاحتفاظ أو يدوياً).</summary>
        public List<AiConversationView> GetConversations(long userId, string search, int top = 200, bool archived = false)
        {
            var db = _newOpp().db;
            string like = string.IsNullOrWhiteSpace(search) ? null : "%" + search.Trim().Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]") + "%";
            var list = new List<AiConversationView>();
            Exec(db, @"SELECT TOP (@top) c.Id, c.Title, c.CreatedAt, c.LastMessageAt FROM AI_Conversations c
                       WHERE c.User_Id = @u AND c.Status = @st
                         AND (@q IS NULL OR c.Title LIKE @q
                              OR EXISTS (SELECT 1 FROM AI_Messages m WHERE m.Conversation_Id = c.Id AND m.ContentJson LIKE @q))
                       ORDER BY ISNULL(c.LastMessageAt, c.CreatedAt) DESC",
                cmd => { Add(cmd, "@top", top); Add(cmd, "@u", userId); Add(cmd, "@q", like); Add(cmd, "@st", archived ? StatusArchived : StatusActive); },
                r => list.Add(new AiConversationView
                {
                    IsArchived = archived,
                    Id = r.GetInt64(0),
                    Title = r.IsDBNull(1) ? "" : r.GetString(1),
                    CreatedAt = r.IsDBNull(2) ? DateTime.Now : r.GetDateTime(2),
                    LastMessageAt = r.IsDBNull(3) ? (DateTime?)null : r.GetDateTime(3),
                }));
            return list;
        }

        /// <summary>includeArchived = true للقراءة والحذف والاستعادة؛ الإرسال يحتاج محادثة نشطة.</summary>
        public bool OwnsConversation(long conversationId, long userId, bool includeArchived = false)
        {
            if (conversationId <= 0 || userId <= 0) return false;
            return (includeArchived
                    ? _newOpp().db.Database.SqlQuery<int>($"SELECT COUNT(*) AS Value FROM AI_Conversations WHERE Id = {conversationId} AND User_Id = {userId} AND Status IN (N'Active', N'Archived')")
                    : _newOpp().db.Database.SqlQuery<int>($"SELECT COUNT(*) AS Value FROM AI_Conversations WHERE Id = {conversationId} AND User_Id = {userId} AND Status = N'Active'"))
                .AsEnumerable().First() > 0;
        }

        /// <summary>هل المحادثة مؤرشفة (لصاحبها).</summary>
        public bool IsArchived(long conversationId, long userId) =>
            conversationId > 0 && userId > 0 &&
            _newOpp().db.Database.SqlQuery<int>($"SELECT COUNT(*) AS Value FROM AI_Conversations WHERE Id = {conversationId} AND User_Id = {userId} AND Status = N'Archived'").AsEnumerable().First() > 0;

        /// <summary>أرشفة/استعادة يدوية بواسطة صاحب المحادثة.</summary>
        public bool SetArchived(long conversationId, bool archived, long userId)
        {
            string to = archived ? StatusArchived : StatusActive, from = archived ? StatusActive : StatusArchived;
            return _newOpp().db.Database.ExecuteSqlInterpolated(
                $"UPDATE AI_Conversations SET Status = {to}, UpdatedBy = {userId}, UpdatedAt = GETDATE() WHERE Id = {conversationId} AND User_Id = {userId} AND Status = {from}") > 0;
        }

        /// <summary>
        /// مدة الاحتفاظ (ChatRetentionDays، قرار 2026-11-13: بالأرشفة لا الحذف): المحادثات النشطة التى لم تصلها رسالة منذ المدة تصير «مؤرشفة».
        /// لا حذف لأى رسالة؛ تُستعاد من قائمة «المؤرشفة». يعيد عدد ما أُرشف (دفعة واحدة بحد top).
        /// </summary>
        public int ArchiveInactive(int retentionDays, int top = 1000)
        {
            if (retentionDays < 1) return 0;
            DateTime cutoff = DateTime.Now.AddDays(-retentionDays);
            return _newOpp().db.Database.ExecuteSqlInterpolated($@"
UPDATE TOP ({top}) AI_Conversations SET Status = N'Archived', UpdatedAt = GETDATE()
WHERE Status = N'Active' AND ISNULL(LastMessageAt, CreatedAt) < {cutoff}");
        }

        public long CreateConversation(long userId, long branchId, string title)
        {
            long id = CallSnowflakeIdGenerate.GenerateNewId();
            _newOpp().db.Database.ExecuteSqlInterpolated($@"
INSERT INTO AI_Conversations (Id, User_Id, Branch_Id, Title, Status, LastMessageAt, CreatedBy, CreatedAt)
VALUES ({id}, {userId}, {(branchId > 0 ? branchId : (long?)null)}, {Cut(title, 200)}, N'Active', GETDATE(), {userId}, GETDATE())");
            return id;
        }

        public void RenameConversation(long conversationId, string title, long userId) =>
            _newOpp().db.Database.ExecuteSqlInterpolated(
                $"UPDATE AI_Conversations SET Title = {Cut(title, 200)}, UpdatedBy = {userId}, UpdatedAt = GETDATE() WHERE Id = {conversationId} AND User_Id = {userId}");

        /// <summary>يحذف المحادثة ورسائلها (صفوف أنشأها المستخدم نفسه فى جداول المساعد) فى معاملة واحدة.</summary>
        public void DeleteConversation(long conversationId, long userId)
        {
            var db = _newOpp().db;
            using var tx = db.Database.BeginTransaction();
            db.Database.ExecuteSqlInterpolated($"DELETE FROM AI_Messages WHERE Conversation_Id IN (SELECT Id FROM AI_Conversations WHERE Id = {conversationId} AND User_Id = {userId})");
            db.Database.ExecuteSqlInterpolated($"DELETE FROM AI_Conversations WHERE Id = {conversationId} AND User_Id = {userId}");
            tx.Commit();
        }

        public List<AiMessageRow> GetMessages(long conversationId)
        {
            var db = _newOpp().db;
            var list = new List<AiMessageRow>();
            Exec(db, @"SELECT Id, Conversation_Id, Seq, Role, ContentJson, Model_Id, ModelName, InputTokens, OutputTokens, CacheReadTokens,
                              CostEstimate, Status, ErrorText, DurationMs, CreatedAt
                       FROM AI_Messages WHERE Conversation_Id = @c ORDER BY Seq",
                cmd => Add(cmd, "@c", conversationId),
                r => list.Add(new AiMessageRow
                {
                    Id = r.GetInt64(0),
                    Conversation_Id = r.GetInt64(1),
                    Seq = r.GetInt32(2),
                    Role = r.IsDBNull(3) ? "user" : r.GetString(3),
                    ContentJson = r.IsDBNull(4) ? null : r.GetString(4),
                    Model_Id = r.IsDBNull(5) ? (long?)null : r.GetInt64(5),
                    ModelName = r.IsDBNull(6) ? null : r.GetString(6),
                    InputTokens = r.IsDBNull(7) ? (int?)null : r.GetInt32(7),
                    OutputTokens = r.IsDBNull(8) ? (int?)null : r.GetInt32(8),
                    CacheReadTokens = r.IsDBNull(9) ? (int?)null : r.GetInt32(9),
                    CostEstimate = r.IsDBNull(10) ? (decimal?)null : r.GetDecimal(10),
                    Status = r.IsDBNull(11) ? null : r.GetString(11),
                    ErrorText = r.IsDBNull(12) ? null : r.GetString(12),
                    DurationMs = r.IsDBNull(13) ? (int?)null : r.GetInt32(13),
                    CreatedAt = r.IsDBNull(14) ? DateTime.Now : r.GetDateTime(14),
                }));
            return list;
        }

        /// <summary>يضيف رسالة بالرقم التالى فى المحادثة ويحدّث وقت آخر رسالة. يعيد الصف بعد ضبط Id/Seq.</summary>
        public AiMessageRow AddMessage(AiMessageRow m, long userId)
        {
            var db = _newOpp().db;
            using var tx = db.Database.BeginTransaction(IsolationLevel.Serializable);
            m.Seq = db.Database.SqlQuery<int>($"SELECT ISNULL(MAX(Seq), 0) + 1 AS Value FROM AI_Messages WITH (UPDLOCK, HOLDLOCK) WHERE Conversation_Id = {m.Conversation_Id}")
                .AsEnumerable().First();
            m.Id = CallSnowflakeIdGenerate.GenerateNewId();
            m.CreatedAt = DateTime.Now;
            db.Database.ExecuteSqlInterpolated($@"
INSERT INTO AI_Messages (Id, Conversation_Id, Seq, Role, ContentJson, Model_Id, ModelName, InputTokens, OutputTokens, CacheReadTokens,
                         CostEstimate, Status, ErrorText, DurationMs, CreatedBy, CreatedAt)
VALUES ({m.Id}, {m.Conversation_Id}, {m.Seq}, {m.Role}, {m.ContentJson}, {m.Model_Id}, {Cut(m.ModelName, 200)}, {m.InputTokens}, {m.OutputTokens}, {m.CacheReadTokens},
        {m.CostEstimate}, {m.Status}, {Cut(m.ErrorText, 1000)}, {m.DurationMs}, {userId}, {m.CreatedAt})");
            db.Database.ExecuteSqlInterpolated($"UPDATE AI_Conversations SET LastMessageAt = {m.CreatedAt} WHERE Id = {m.Conversation_Id}");
            tx.Commit();
            return m;
        }

        // ───────────── P4: السؤال المعلّق (ask_user) ─────────────

        /// <summary>يحفظ حالة الحلقة المتوقفة على رسالة المساعد (رسائل المحادثة الجديدة فقط).</summary>
        public void SetPending(long messageId, string pendingJson) =>
            _newOpp().db.Database.ExecuteSqlInterpolated($"UPDATE AI_Messages SET PendingJson = {pendingJson} WHERE Id = {messageId}");

        /// <summary>حالة الحلقة المعلّقة لرسالة فى محادثة يملكها المستخدم؛ null إن لم توجد.</summary>
        public string GetPending(long messageId, long userId)
        {
            var db = _newOpp().db;
            string json = null;
            Exec(db, @"SELECT m.PendingJson FROM AI_Messages m JOIN AI_Conversations c ON c.Id = m.Conversation_Id
                       WHERE m.Id = @m AND c.User_Id = @u AND c.Status = N'Active'",
                cmd => { Add(cmd, "@m", messageId); Add(cmd, "@u", userId); },
                r => json = r.IsDBNull(0) ? null : r.GetString(0));
            return json;
        }

        /// <summary>
        /// يُغلق السؤال ذرّياً: يكتب المحتوى الجديد (السؤال مع إجابته) ويمسح الحالة بشرط أنها ما زالت معلّقة،
        /// فإجابتان متزامنتان لا تستأنفان الحلقة مرتين. يعيد true لمن أغلقه فقط.
        /// </summary>
        public bool ClosePending(long messageId, string contentJson, string status, long userId)
        {
            int n = _newOpp().db.Database.ExecuteSqlInterpolated($@"
UPDATE m SET m.ContentJson = {contentJson}, m.Status = {status}, m.PendingJson = NULL
FROM AI_Messages m JOIN AI_Conversations c ON c.Id = m.Conversation_Id
WHERE m.Id = {messageId} AND c.User_Id = {userId} AND m.Status = N'awaiting'");
            return n == 1;
        }

        // ───────────── أدوات ─────────────

        private static string Cut(string s, int max) => s == null ? null : (s.Length > max ? s.Substring(0, max) : s);

        private static void Add(DbCommand cmd, string name, object value)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }

        private static void Exec(My_DbContext db, string sql, Action<DbCommand> bind, Action<DbDataReader> row)
        {
            var conn = db.Database.GetDbConnection();
            bool wasClosed = conn.State != ConnectionState.Open;
            if (wasClosed) conn.Open();
            try
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                bind(cmd);
                using var r = cmd.ExecuteReader();
                while (r.Read()) row(r);
            }
            finally { if (wasClosed) conn.Close(); }
        }
    }
}
