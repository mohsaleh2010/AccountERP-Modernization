using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib
{
    /// <summary>صف من AI_SavedReports كما هو (الـ JSON يُفسَّر فى AccountERP.AI).</summary>
    public class AiReportRow
    {
        public long Id { get; set; }
        public long User_Id { get; set; }
        public long? Conversation_Id { get; set; }
        public long? Message_Id { get; set; }
        public string Title { get; set; } = "";
        public string SpecJson { get; set; } = "";
        public string SourcesJson { get; set; }
        public string DataJson { get; set; }
        public DateTime? PeriodFrom { get; set; }
        public DateTime? PeriodTo { get; set; }
        public bool IsSaved { get; set; }
        public bool IsPinned { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastRunAt { get; set; }
    }

    /// <summary>
    /// P3: تقارير المساعد (AI_SavedReports، جدول جديد). كل دالة مقيّدة بـ User_Id نفسه (لا يرى مستخدم تقرير غيره).
    /// لا تمس أى جدول قائم.
    /// </summary>
    public class AiReportsCLS
    {
        private readonly Func<operations> _newOpp;

        public AiReportsCLS(Func<operations> newOpp = null)
        {
            _newOpp = newOpp ?? (() => new operations());
        }

        public long Insert(AiReportRow r)
        {
            long id = CallSnowflakeIdGenerate.GenerateNewId();
            DateTime now = DateTime.Now;
            _newOpp().db.Database.ExecuteSqlInterpolated($@"
INSERT INTO AI_SavedReports (Id, User_Id, Conversation_Id, Message_Id, Title, SpecJson, SourcesJson, DataJson, PeriodFrom, PeriodTo,
                             IsSaved, IsPinned, IsShared, LastRunAt, CreatedBy, CreatedAt)
VALUES ({id}, {r.User_Id}, {r.Conversation_Id}, NULL, {Cut(r.Title, 200)}, {r.SpecJson}, {r.SourcesJson}, {r.DataJson}, {r.PeriodFrom}, {r.PeriodTo},
        0, 0, 0, {now}, {r.User_Id}, {now})");
            return id;
        }

        /// <summary>يربط التقارير التى رُسمت أثناء الرد برسالة المساعد المحفوظة.</summary>
        public void LinkMessage(IEnumerable<long> ids, long messageId, long userId)
        {
            var list = ids?.Where(i => i > 0).Distinct().ToList() ?? new List<long>();
            if (list.Count == 0) return;
            var db = _newOpp().db;
            foreach (var id in list)
                db.Database.ExecuteSqlInterpolated($"UPDATE AI_SavedReports SET Message_Id = {messageId} WHERE Id = {id} AND User_Id = {userId} AND Message_Id IS NULL");
        }

        public AiReportRow Get(long id, long userId)
        {
            AiReportRow found = null;
            Exec(_newOpp().db, Select + " WHERE Id = @id AND User_Id = @u",
                cmd => { Add(cmd, "@id", id); Add(cmd, "@u", userId); },
                r => found = Read(r));
            return found;
        }

        public List<AiSavedReportView> ListSaved(long userId, int top = 100)
        {
            var list = new List<AiSavedReportView>();
            Exec(_newOpp().db, @"SELECT TOP (@top) Id, Title, IsPinned, CreatedAt, LastRunAt, PeriodFrom, PeriodTo, ISNULL(IsShared, 0) FROM AI_SavedReports
                                 WHERE User_Id = @u AND IsSaved = 1 ORDER BY IsPinned DESC, ISNULL(LastRunAt, CreatedAt) DESC, Id DESC",
                cmd => { Add(cmd, "@top", top); Add(cmd, "@u", userId); },
                r => list.Add(new AiSavedReportView
                {
                    Id = r.GetInt64(0),
                    Title = r.IsDBNull(1) ? "" : r.GetString(1),
                    IsPinned = !r.IsDBNull(2) && r.GetBoolean(2),
                    CreatedAt = r.IsDBNull(3) ? DateTime.MinValue : r.GetDateTime(3),
                    LastRunAt = r.IsDBNull(4) ? (DateTime?)null : r.GetDateTime(4),
                    PeriodFrom = r.IsDBNull(5) ? (DateTime?)null : r.GetDateTime(5),
                    PeriodTo = r.IsDBNull(6) ? (DateTime?)null : r.GetDateTime(6),
                    IsShared = Convert.ToBoolean(r.GetValue(7)),
                }));
            return list;
        }

        // ───────────── بقايا ب1: المشاركة ─────────────

        /// <summary>مشاركة/إلغاء مشاركة تقرير محفوظ — لصاحبه فقط.</summary>
        public bool SetShared(long id, bool shared, long userId) =>
            _newOpp().db.Database.ExecuteSqlInterpolated($@"
UPDATE AI_SavedReports SET IsShared = {shared}, UpdatedBy = {userId}, UpdatedAt = {DateTime.Now}
WHERE Id = {id} AND User_Id = {userId} AND IsSaved = 1") == 1;

        /// <summary>التقارير المشتركة من مستخدمين آخرين (بلا البيانات).</summary>
        public List<AiSavedReportView> ListShared(long excludeUserId, int top = 100)
        {
            var list = new List<AiSavedReportView>();
            Exec(_newOpp().db, @"SELECT TOP (@top) r.Id, r.Title, r.CreatedAt, r.LastRunAt, r.PeriodFrom, r.PeriodTo, ISNULL(u.USER_NAME, N'')
FROM AI_SavedReports r LEFT JOIN USER_DETAILES u ON u.Id = r.User_Id
WHERE r.IsShared = 1 AND r.IsSaved = 1 AND r.User_Id <> @u ORDER BY ISNULL(r.LastRunAt, r.CreatedAt) DESC, r.Id DESC",
                cmd => { Add(cmd, "@top", top); Add(cmd, "@u", excludeUserId); },
                r => list.Add(new AiSavedReportView
                {
                    Id = r.GetInt64(0), Title = r.IsDBNull(1) ? "" : r.GetString(1), CreatedAt = r.IsDBNull(2) ? DateTime.MinValue : r.GetDateTime(2),
                    LastRunAt = r.IsDBNull(3) ? (DateTime?)null : r.GetDateTime(3), PeriodFrom = r.IsDBNull(4) ? (DateTime?)null : r.GetDateTime(4),
                    PeriodTo = r.IsDBNull(5) ? (DateTime?)null : r.GetDateTime(5), IsShared = true, OwnerName = r.IsDBNull(6) ? "" : r.GetString(6),
                }));
            return list;
        }

        /// <summary>نسخة خاصة (غير محفوظة) من تقرير مشترك للمستخدم، بلا بيانات صاحبه (DataJson = NULL). 0 = غير مشترك.</summary>
        public long CopyShared(long id, long toUserId)
        {
            long newId = CallSnowflakeIdGenerate.GenerateNewId();
            DateTime now = DateTime.Now;
            int n = _newOpp().db.Database.ExecuteSqlInterpolated($@"
INSERT INTO AI_SavedReports (Id, User_Id, Conversation_Id, Message_Id, Title, SpecJson, SourcesJson, DataJson, PeriodFrom, PeriodTo,
                             IsSaved, IsPinned, IsShared, LastRunAt, CreatedBy, CreatedAt)
SELECT {newId}, {toUserId}, NULL, NULL, Title, SpecJson, SourcesJson, NULL, PeriodFrom, PeriodTo, 0, 0, 0, NULL, {toUserId}, {now}
FROM AI_SavedReports WHERE Id = {id} AND IsShared = 1 AND IsSaved = 1");
            return n == 1 ? newId : 0;
        }

        /// <summary>حذف نسخة غير محفوظة للمستخدم نفسه (فشل فتح تقرير مشترك).</summary>
        public void DeleteUnsaved(long id, long userId) =>
            _newOpp().db.Database.ExecuteSqlInterpolated($"DELETE FROM AI_SavedReports WHERE Id = {id} AND User_Id = {userId} AND IsSaved = 0");

        public void SetSaved(long id, string title, bool saved, bool pinned, long userId)
        {
            string t = string.IsNullOrWhiteSpace(title) ? null : Cut(title.Trim(), 200);
            _newOpp().db.Database.ExecuteSqlInterpolated($@"
UPDATE AI_SavedReports SET IsSaved = {saved}, IsPinned = {saved && pinned}, Title = ISNULL({t}, Title), UpdatedBy = {userId}, UpdatedAt = {DateTime.Now}
WHERE Id = {id} AND User_Id = {userId}");
        }

        /// <summary>بيانات إعادة التشغيل: لقطة جديدة ومصادر بتواريخها الجديدة.</summary>
        public void UpdateData(long id, string sourcesJson, string dataJson, DateTime? from, DateTime? to, long userId)
        {
            DateTime now = DateTime.Now;
            _newOpp().db.Database.ExecuteSqlInterpolated($@"
UPDATE AI_SavedReports SET SourcesJson = {sourcesJson}, DataJson = {dataJson}, PeriodFrom = {from}, PeriodTo = {to}, LastRunAt = {now},
       UpdatedBy = {userId}, UpdatedAt = {now}
WHERE Id = {id} AND User_Id = {userId}");
        }

        /// <summary>مع حذف المحادثة: تُحذف تقاريرها غير المحفوظة فقط؛ المحفوظ يبقى فى «تقاريرى» بلا محادثة.</summary>
        public void DeleteForConversation(long conversationId, long userId)
        {
            var db = _newOpp().db;
            db.Database.ExecuteSqlInterpolated($"DELETE FROM AI_SavedReports WHERE Conversation_Id = {conversationId} AND User_Id = {userId} AND IsSaved = 0");
            db.Database.ExecuteSqlInterpolated($"UPDATE AI_SavedReports SET Conversation_Id = NULL, Message_Id = NULL WHERE Conversation_Id = {conversationId} AND User_Id = {userId}");
        }

        private const string Select = @"SELECT Id, User_Id, Conversation_Id, Message_Id, Title, SpecJson, SourcesJson, DataJson, PeriodFrom, PeriodTo,
                                               IsSaved, IsPinned, CreatedAt, LastRunAt FROM AI_SavedReports";

        private static AiReportRow Read(DbDataReader r) => new AiReportRow
        {
            Id = r.GetInt64(0),
            User_Id = r.GetInt64(1),
            Conversation_Id = r.IsDBNull(2) ? (long?)null : r.GetInt64(2),
            Message_Id = r.IsDBNull(3) ? (long?)null : r.GetInt64(3),
            Title = r.IsDBNull(4) ? "" : r.GetString(4),
            SpecJson = r.IsDBNull(5) ? "" : r.GetString(5),
            SourcesJson = r.IsDBNull(6) ? null : r.GetString(6),
            DataJson = r.IsDBNull(7) ? null : r.GetString(7),
            PeriodFrom = r.IsDBNull(8) ? (DateTime?)null : r.GetDateTime(8),
            PeriodTo = r.IsDBNull(9) ? (DateTime?)null : r.GetDateTime(9),
            IsSaved = !r.IsDBNull(10) && r.GetBoolean(10),
            IsPinned = !r.IsDBNull(11) && r.GetBoolean(11),
            CreatedAt = r.IsDBNull(12) ? DateTime.MinValue : r.GetDateTime(12),
            LastRunAt = r.IsDBNull(13) ? (DateTime?)null : r.GetDateTime(13),
        };

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
