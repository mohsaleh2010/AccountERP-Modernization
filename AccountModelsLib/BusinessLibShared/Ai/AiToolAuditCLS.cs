using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib
{
    /// <summary>
    /// P2: سجل تدقيق أدوات المساعد (AI_ToolCalls_Audit، جدول جديد). كل استدعاء يُدرج لحظة تنفيذه (المقبول والمرفوض والخاطئ)،
    /// ثم يُربط برسالة المساعد بعد حفظها. لا يحذف ولا يعدّل إلا Message_Id لصفوفه هو.
    /// </summary>
    public class AiToolAuditCLS
    {
        private readonly Func<operations> _newOpp;

        public AiToolAuditCLS(Func<operations> newOpp = null)
        {
            _newOpp = newOpp ?? (() => new operations());
        }

        public long Insert(long? conversationId, long userId, string toolName, string argsJson, string sqlText,
                           int? rowCount, int? durationMs, string outcome, string errorText)
        {
            long id = CallSnowflakeIdGenerate.GenerateNewId();
            _newOpp().db.Database.ExecuteSqlInterpolated($@"
INSERT INTO AI_ToolCalls_Audit (Id, Conversation_Id, Message_Id, User_Id, ToolName, ArgsJson, SqlText, [RowCount], DurationMs, Outcome, ErrorText, CreatedBy, CreatedAt)
VALUES ({id}, {conversationId}, NULL, {userId}, {Cut(toolName, 100)}, {argsJson}, {sqlText}, {rowCount}, {durationMs}, {Cut(outcome, 20)}, {Cut(errorText, 1000)}, {userId}, {DateTime.Now})");
            return id;
        }

        /// <summary>يربط صفوف التدقيق التى أُدرجت أثناء الرد برسالة المساعد المحفوظة.</summary>
        public void LinkMessage(IEnumerable<long> auditIds, long messageId)
        {
            var ids = auditIds?.Where(i => i > 0).Distinct().ToList() ?? new List<long>();
            if (ids.Count == 0) return;
            var db = _newOpp().db;
            foreach (var id in ids)
                db.Database.ExecuteSqlInterpolated($"UPDATE AI_ToolCalls_Audit SET Message_Id = {messageId} WHERE Id = {id} AND Message_Id IS NULL");
        }

        /// <summary>أحدث الاستدعاءات (للمدير فى /settings/ai). outcome فارغ = الكل؛ search فى اسم الأداة والاستعلام والمستخدم.</summary>
        public List<AiToolAuditView> List(string outcome, string search, int top = 300)
        {
            if (top <= 0 || top > 2000) top = 300;
            string like = string.IsNullOrWhiteSpace(search) ? null : "%" + search.Trim().Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]") + "%";
            string oc = string.IsNullOrWhiteSpace(outcome) ? null : outcome.Trim();
            var list = new List<AiToolAuditView>();
            Exec(_newOpp().db, @"SELECT TOP (@top) a.Id, a.CreatedAt, a.User_Id, u.USER_NAME, a.Conversation_Id, a.ToolName, a.ArgsJson, a.SqlText,
                                        a.[RowCount], a.DurationMs, a.Outcome, a.ErrorText
                                 FROM AI_ToolCalls_Audit a LEFT JOIN USER_DETAILES u ON u.Id = a.User_Id
                                 WHERE (@o IS NULL OR a.Outcome = @o)
                                   AND (@q IS NULL OR a.ToolName LIKE @q OR a.SqlText LIKE @q OR a.ArgsJson LIKE @q OR u.USER_NAME LIKE @q)
                                 ORDER BY a.CreatedAt DESC, a.Id DESC",
                cmd => { Add(cmd, "@top", top); Add(cmd, "@o", oc); Add(cmd, "@q", like); },
                r => list.Add(new AiToolAuditView
                {
                    Id = r.GetInt64(0),
                    CreatedAt = r.IsDBNull(1) ? DateTime.MinValue : r.GetDateTime(1),
                    UserId = r.GetInt64(2),
                    UserName = r.IsDBNull(3) ? "" : Convert.ToString(r.GetValue(3)) ?? "",
                    ConversationId = r.IsDBNull(4) ? (long?)null : r.GetInt64(4),
                    ToolName = r.IsDBNull(5) ? "" : r.GetString(5),
                    ArgsJson = r.IsDBNull(6) ? "" : r.GetString(6),
                    SqlText = r.IsDBNull(7) ? "" : r.GetString(7),
                    RowCount = r.IsDBNull(8) ? (int?)null : r.GetInt32(8),
                    DurationMs = r.IsDBNull(9) ? (int?)null : r.GetInt32(9),
                    Outcome = r.IsDBNull(10) ? "" : r.GetString(10),
                    ErrorText = r.IsDBNull(11) ? "" : r.GetString(11),
                }));
            return list;
        }

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
