using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib
{
    // ───────────── P7.6: ملاحظات 👍/👎 على ردود المساعد وقائمة مراجعتها ─────────────

    public static class AiFeedbackRatings
    {
        public const int Up = 1;
        public const int Down = -1;
    }

    /// <summary>سبب عدم الرضا (يُختار مع 👎).</summary>
    public static class AiFeedbackReasons
    {
        public const string WrongResult = "wrong_result";
        public const string Misunderstood = "misunderstood";
        public const string Incomplete = "incomplete";
        public const string Slow = "slow";
        public const string Other = "other";
        public static readonly string[] All = { WrongResult, Misunderstood, Incomplete, Slow, Other };
    }

    public static class AiFeedbackStatus
    {
        public const string New = "New";
        public const string Accepted = "Accepted";
        public const string Rejected = "Rejected";
    }

    /// <summary>ملاحظة المستخدم كما تظهر تحت رد المساعد. Reviewed = راجعها المدير فلا تُغيَّر.</summary>
    public class AiFeedbackView
    {
        public int Rating { get; set; }
        public string Reason { get; set; } = "";
        public string Comment { get; set; } = "";
        public bool Reviewed { get; set; }
    }

    /// <summary>صف AI_Feedback لقائمة المراجعة (تبويب «الملاحظات»).</summary>
    public class AiFeedbackRow
    {
        public long Id { get; set; }
        public long Message_Id { get; set; }
        public long Conversation_Id { get; set; }
        public long User_Id { get; set; }
        public string UserName { get; set; } = "";
        public int Rating { get; set; }
        public string Reason { get; set; }
        public string Comment { get; set; }
        public string QuestionText { get; set; }
        public string AnswerText { get; set; }
        public string SqlText { get; set; }
        public string ToolsJson { get; set; }
        public long? Model_Id { get; set; }
        public string ModelName { get; set; }
        public string ReviewStatus { get; set; } = AiFeedbackStatus.New;
        public long? ReviewedBy { get; set; }
        public string ReviewedByName { get; set; } = "";
        public DateTime? ReviewedAt { get; set; }
        public string ReviewNote { get; set; }
        public long? Knowledge_Id { get; set; }
        public long? EvalCase_Id { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>عدّادات رأس قائمة المراجعة.</summary>
    public class AiFeedbackCounts
    {
        public int New { get; set; }
        public int NewDown { get; set; }
        public int Accepted { get; set; }
        public int Rejected { get; set; }
        public int Total { get; set; }
    }

    /// <summary>اعتماد رد كمثال فى «المعرفة»: العنوان (السؤال) واستعلام القراءة (قابل للتصحيح قبل الاعتماد).</summary>
    public class AiFeedbackExampleRequest
    {
        public long FeedbackId { get; set; }
        public string TitleAr { get; set; } = "";
        public string Sql { get; set; } = "";
        public string Keywords { get; set; } = "";
    }

    /// <summary>
    /// P7.6: قائمة مراجعة الملاحظات (للمدير: صلاحية شاشة /settings/ai). تُنفَّذ فى AccountERP.AI ويسجّلها مضيف الويب فقط.
    /// الأخطاء رسائل عربية (InvalidOperationException).
    /// </summary>
    public interface IAiFeedbackGateway
    {
        /// <summary>status: "" = الكل، New / Accepted / Rejected. rating: 0 = الكل.</summary>
        List<AiFeedbackRow> GetFeedback(string status, int rating, string search, int top = 300);
        AiFeedbackCounts GetCounts();
        /// <summary>يضيف مثالاً فى AI_Knowledge (Source = feedback) ويعلّم الملاحظة Accepted. يعيد Id المقطع.</summary>
        long AcceptAsExample(AiFeedbackExampleRequest request, long userId);
        /// <summary>يضيف حالة تقييم (Source = feedback) ويعلّم الملاحظة Accepted. يعيد Id الحالة.</summary>
        long AddAsEvalCase(long feedbackId, AiEvalCaseSaveRequest request, long userId);
        void Reject(long feedbackId, string note, long userId);
        /// <summary>يعيد ملاحظة مرفوضة إلى «جديدة».</summary>
        void Reopen(long feedbackId, long userId);
    }

    /// <summary>
    /// P7.6: جدول AI_Feedback (جديد). لا قراءة ولا كتابة فى جداول النظام القائمة (إلا اسم المستخدم من USER_DETAILES للعرض،
    /// ورسائل/محادثات المساعد للتحقق من الملكية).
    /// </summary>
    public class AiFeedbackCLS
    {
        private readonly Func<operations> _newOpp;

        public AiFeedbackCLS(Func<operations> newOpp = null)
        {
            _newOpp = newOpp ?? (() => new operations());
        }

        /// <summary>رسالة المساعد إن كانت فى محادثة يملكها المستخدم: (المحادثة، الدور، الحالة)، وإلا null.</summary>
        public (long ConversationId, string Role, string Status)? OwnedMessage(long messageId, long userId)
        {
            (long, string, string)? found = null;
            Exec(_newOpp().db, @"SELECT m.Conversation_Id, m.Role, m.Status FROM AI_Messages m INNER JOIN AI_Conversations c ON c.Id = m.Conversation_Id
                                 WHERE m.Id = @m AND c.User_Id = @u",
                cmd => { Add(cmd, "@m", messageId); Add(cmd, "@u", userId); },
                r => found = (r.GetInt64(0), S(r, 1) ?? "", S(r, 2) ?? ""));
            return found;
        }

        private const string Select = @"SELECT f.Id, f.Message_Id, f.Conversation_Id, f.User_Id, u.USER_NAME AS UserName, f.Rating, f.Reason, f.Comment, f.QuestionText, f.AnswerText,
       f.SqlText, f.ToolsJson, f.Model_Id, f.ModelName, f.ReviewStatus, f.ReviewedBy, rv.USER_NAME AS ReviewerName, f.ReviewedAt, f.ReviewNote, f.Knowledge_Id, f.EvalCase_Id,
       f.CreatedAt, f.UpdatedAt
FROM AI_Feedback f LEFT JOIN USER_DETAILES u ON u.Id = f.User_Id LEFT JOIN USER_DETAILES rv ON rv.Id = f.ReviewedBy";

        private static AiFeedbackRow Read(DbDataReader r) => new AiFeedbackRow
        {
            Id = r.GetInt64(0),
            Message_Id = r.GetInt64(1),
            Conversation_Id = r.GetInt64(2),
            User_Id = r.GetInt64(3),
            UserName = S(r, 4) ?? "",
            Rating = Convert.ToInt32(r.GetValue(5)),
            Reason = S(r, 6),
            Comment = S(r, 7),
            QuestionText = S(r, 8),
            AnswerText = S(r, 9),
            SqlText = S(r, 10),
            ToolsJson = S(r, 11),
            Model_Id = r.IsDBNull(12) ? (long?)null : r.GetInt64(12),
            ModelName = S(r, 13),
            ReviewStatus = S(r, 14) ?? AiFeedbackStatus.New,
            ReviewedBy = r.IsDBNull(15) ? (long?)null : r.GetInt64(15),
            ReviewedByName = S(r, 16) ?? "",
            ReviewedAt = r.IsDBNull(17) ? (DateTime?)null : r.GetDateTime(17),
            ReviewNote = S(r, 18),
            Knowledge_Id = r.IsDBNull(19) ? (long?)null : r.GetInt64(19),
            EvalCase_Id = r.IsDBNull(20) ? (long?)null : r.GetInt64(20),
            CreatedAt = r.IsDBNull(21) ? (DateTime?)null : r.GetDateTime(21),
            UpdatedAt = r.IsDBNull(22) ? (DateTime?)null : r.GetDateTime(22),
        };

        public AiFeedbackRow Get(long id)
        {
            AiFeedbackRow found = null;
            Exec(_newOpp().db, Select + " WHERE f.Id = @id", cmd => Add(cmd, "@id", id), r => found = Read(r));
            return found;
        }

        public AiFeedbackRow GetByMessage(long messageId, long userId)
        {
            AiFeedbackRow found = null;
            Exec(_newOpp().db, Select + " WHERE f.Message_Id = @m AND f.User_Id = @u",
                cmd => { Add(cmd, "@m", messageId); Add(cmd, "@u", userId); }, r => found = Read(r));
            return found;
        }

        /// <summary>ملاحظات المستخدم على رسائل محادثة (لعرض الحالة تحت كل رد).</summary>
        public Dictionary<long, AiFeedbackView> ForConversation(long conversationId, long userId)
        {
            var map = new Dictionary<long, AiFeedbackView>();
            Exec(_newOpp().db, "SELECT Message_Id, Rating, Reason, Comment, ReviewStatus FROM AI_Feedback WHERE Conversation_Id = @c AND User_Id = @u",
                cmd => { Add(cmd, "@c", conversationId); Add(cmd, "@u", userId); },
                r => map[r.GetInt64(0)] = new AiFeedbackView
                {
                    Rating = Convert.ToInt32(r.GetValue(1)),
                    Reason = S(r, 2) ?? "",
                    Comment = S(r, 3) ?? "",
                    Reviewed = (S(r, 4) ?? AiFeedbackStatus.New) != AiFeedbackStatus.New,
                });
            return map;
        }

        public void Insert(AiFeedbackRow f, long userId)
        {
            if (f.Id == 0) f.Id = CallSnowflakeIdGenerate.GenerateNewId();
            _newOpp().db.Database.ExecuteSqlInterpolated($@"
INSERT INTO AI_Feedback (Id, Message_Id, Conversation_Id, User_Id, Rating, Reason, Comment, QuestionText, AnswerText, SqlText, ToolsJson, Model_Id, ModelName,
                         ReviewStatus, CreatedBy, CreatedAt)
VALUES ({f.Id}, {f.Message_Id}, {f.Conversation_Id}, {f.User_Id}, {(short)f.Rating}, {Cut(f.Reason, 30)}, {Cut(f.Comment, 1000)}, {f.QuestionText}, {f.AnswerText},
        {f.SqlText}, {f.ToolsJson}, {f.Model_Id}, {Cut(f.ModelName, 200)}, {AiFeedbackStatus.New}, {userId}, {DateTime.Now})");
        }

        /// <summary>تغيير التقييم/السبب/التعليق ما دامت الملاحظة «جديدة». يعيد false إن رُوجعت.</summary>
        public bool UpdateRating(long id, int rating, string reason, string comment, long userId) =>
            _newOpp().db.Database.ExecuteSqlInterpolated($@"
UPDATE AI_Feedback SET Rating = {(short)rating}, Reason = {Cut(reason, 30)}, Comment = {Cut(comment, 1000)}, UpdatedBy = {userId}, UpdatedAt = {DateTime.Now}
WHERE Id = {id} AND User_Id = {userId} AND ReviewStatus = {AiFeedbackStatus.New}") == 1;

        /// <summary>سحب الملاحظة (ضغط نفس الزر مرة أخرى) ما دامت «جديدة». يعيد false إن رُوجعت.</summary>
        public bool Delete(long id, long userId) =>
            _newOpp().db.Database.ExecuteSqlInterpolated(
                $"DELETE FROM AI_Feedback WHERE Id = {id} AND User_Id = {userId} AND ReviewStatus = {AiFeedbackStatus.New}") == 1;

        public List<AiFeedbackRow> List(string status, int rating, string search, int top)
        {
            var list = new List<AiFeedbackRow>();
            string q = (search ?? "").Trim();
            Exec(_newOpp().db, $@"SELECT TOP ({Math.Max(1, Math.Min(top, 2000))}) * FROM ({Select}
WHERE (@st = N'' OR f.ReviewStatus = @st) AND (@rt = 0 OR f.Rating = @rt)
  AND (@q = N'' OR f.QuestionText LIKE @like OR f.AnswerText LIKE @like OR f.Comment LIKE @like OR u.USER_NAME LIKE @like OR f.ModelName LIKE @like)) x
ORDER BY CASE WHEN x.ReviewStatus = N'New' THEN 0 ELSE 1 END, x.CreatedAt DESC",
                cmd =>
                {
                    Add(cmd, "@st", status ?? "");
                    Add(cmd, "@rt", rating);
                    Add(cmd, "@q", q);
                    Add(cmd, "@like", "%" + q.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]") + "%");
                },
                r => list.Add(Read(r)));
            return list;
        }

        public AiFeedbackCounts Counts()
        {
            var c = new AiFeedbackCounts();
            Exec(_newOpp().db, @"SELECT ISNULL(SUM(CASE WHEN ReviewStatus = N'New' THEN 1 ELSE 0 END), 0),
       ISNULL(SUM(CASE WHEN ReviewStatus = N'New' AND Rating < 0 THEN 1 ELSE 0 END), 0),
       ISNULL(SUM(CASE WHEN ReviewStatus = N'Accepted' THEN 1 ELSE 0 END), 0),
       ISNULL(SUM(CASE WHEN ReviewStatus = N'Rejected' THEN 1 ELSE 0 END), 0), COUNT(*)
FROM AI_Feedback", _ => { }, r =>
            {
                c.New = r.GetInt32(0); c.NewDown = r.GetInt32(1); c.Accepted = r.GetInt32(2); c.Rejected = r.GetInt32(3); c.Total = r.GetInt32(4);
            });
            return c;
        }

        /// <summary>تُعلَّم Accepted مع ربطها بالمقطع أو الحالة الناتجة (الربط الآخر يبقى كما هو).</summary>
        public void MarkAccepted(long id, long? knowledgeId, long? evalCaseId, long userId) =>
            _newOpp().db.Database.ExecuteSqlInterpolated($@"
UPDATE AI_Feedback SET ReviewStatus = {AiFeedbackStatus.Accepted}, Knowledge_Id = COALESCE({knowledgeId}, Knowledge_Id), EvalCase_Id = COALESCE({evalCaseId}, EvalCase_Id),
       ReviewedBy = {userId}, ReviewedAt = {DateTime.Now}, UpdatedBy = {userId}, UpdatedAt = {DateTime.Now}
WHERE Id = {id}");

        public void SetStatus(long id, string status, string note, long userId) =>
            _newOpp().db.Database.ExecuteSqlInterpolated($@"
UPDATE AI_Feedback SET ReviewStatus = {status}, ReviewNote = {Cut(note, 1000)},
       ReviewedBy = CASE WHEN {status} = N'New' THEN NULL ELSE {userId} END, ReviewedAt = CASE WHEN {status} = N'New' THEN NULL ELSE {DateTime.Now} END,
       UpdatedBy = {userId}, UpdatedAt = {DateTime.Now}
WHERE Id = {id}");

        // ───────────── مساعدات ─────────────

        private static string S(DbDataReader r, int i) => r.IsDBNull(i) ? null : Convert.ToString(r.GetValue(i));

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
