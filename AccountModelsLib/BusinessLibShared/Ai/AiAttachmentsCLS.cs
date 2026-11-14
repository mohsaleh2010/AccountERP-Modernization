using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib
{
    public static class AiAttachmentStatus
    {
        public const string Active = "Active";
        public const string Expired = "Expired";
    }

    /// <summary>P6: أنواع المستندات التى يستخرجها المساعد (extract_document.doc_type).</summary>
    public static class AiExtractDocTypes
    {
        public const string PurchaseInvoice = "purchase_invoice";
        public const string SalesInvoice = "sales_invoice";
        public const string ItemsList = "items_list";
        public const string Other = "other";

        public static readonly string[] All = { PurchaseInvoice, SalesInvoice, ItemsList, Other };
    }

    /// <summary>صف AI_Attachments (الملف نفسه على القرص؛ StoragePath نسبى لمجلد المرفقات).</summary>
    public class AiAttachmentRow
    {
        public long Id { get; set; }
        public long User_Id { get; set; }
        public long? Conversation_Id { get; set; }
        public long? Message_Id { get; set; }
        public string FileName { get; set; } = "";
        public string MimeType { get; set; } = "";
        public long SizeBytes { get; set; }
        public string Sha256 { get; set; } = "";
        public string StoragePath { get; set; } = "";
        public string Status { get; set; } = AiAttachmentStatus.Active;
        public string ExtractDocType { get; set; }
        public string ExtractJson { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// P6: مرفقات المساعد (AI_Attachments، جدول جديد). كل دالة مقيّدة بـ User_Id. لا قراءة ولا كتابة فى جداول النظام القائمة هنا.
    /// </summary>
    public class AiAttachmentsCLS
    {
        private readonly Func<operations> _newOpp;

        public AiAttachmentsCLS(Func<operations> newOpp = null)
        {
            _newOpp = newOpp ?? (() => new operations());
        }

        public void Insert(AiAttachmentRow r)
        {
            DateTime now = DateTime.Now;
            _newOpp().db.Database.ExecuteSqlInterpolated($@"
INSERT INTO AI_Attachments (Id, User_Id, Conversation_Id, Message_Id, FileName, MimeType, SizeBytes, Sha256, StoragePath, Status, CreatedBy, CreatedAt)
VALUES ({r.Id}, {r.User_Id}, {r.Conversation_Id}, NULL, {Cut(r.FileName, 260)}, {Cut(r.MimeType, 100)}, {r.SizeBytes}, {r.Sha256}, {Cut(r.StoragePath, 400)},
        {AiAttachmentStatus.Active}, {r.User_Id}, {now})");
            r.CreatedAt = now;
        }

        private const string Select = @"SELECT Id, User_Id, Conversation_Id, Message_Id, FileName, MimeType, SizeBytes, Sha256, StoragePath, Status, ExtractDocType, ExtractJson, CreatedAt
FROM AI_Attachments";

        private static AiAttachmentRow Read(DbDataReader r) => new AiAttachmentRow
        {
            Id = r.GetInt64(0),
            User_Id = r.GetInt64(1),
            Conversation_Id = r.IsDBNull(2) ? (long?)null : r.GetInt64(2),
            Message_Id = r.IsDBNull(3) ? (long?)null : r.GetInt64(3),
            FileName = r.IsDBNull(4) ? "" : r.GetString(4),
            MimeType = r.IsDBNull(5) ? "" : r.GetString(5),
            SizeBytes = r.IsDBNull(6) ? 0 : r.GetInt64(6),
            Sha256 = r.IsDBNull(7) ? "" : r.GetString(7).Trim(),
            StoragePath = r.IsDBNull(8) ? "" : r.GetString(8),
            Status = r.IsDBNull(9) ? AiAttachmentStatus.Active : r.GetString(9),
            ExtractDocType = r.IsDBNull(10) ? null : r.GetString(10),
            ExtractJson = r.IsDBNull(11) ? null : r.GetString(11),
            CreatedAt = r.IsDBNull(12) ? DateTime.MinValue : r.GetDateTime(12),
        };

        public AiAttachmentRow Get(long id, long userId)
        {
            AiAttachmentRow found = null;
            Exec(_newOpp().db, Select + " WHERE Id = @id AND User_Id = @u", cmd => { Add(cmd, "@id", id); Add(cmd, "@u", userId); }, r => found = Read(r));
            return found;
        }

        public List<AiAttachmentRow> GetMany(IEnumerable<long> ids, long userId)
        {
            var list = new List<AiAttachmentRow>();
            var all = ids?.Where(i => i > 0).Distinct().ToList() ?? new List<long>();
            if (all.Count == 0) return list;
            Exec(_newOpp().db, Select + " WHERE User_Id = @u AND Id IN (" + string.Join(",", all) + ")", cmd => Add(cmd, "@u", userId), r => list.Add(Read(r)));
            return list;
        }

        /// <summary>مرفقات محادثة (المرسلة) بترتيب الرفع.</summary>
        public List<AiAttachmentRow> ListForConversation(long conversationId, long userId)
        {
            var list = new List<AiAttachmentRow>();
            Exec(_newOpp().db, Select + " WHERE Conversation_Id = @c AND User_Id = @u AND Message_Id IS NOT NULL ORDER BY CreatedAt",
                cmd => { Add(cmd, "@c", conversationId); Add(cmd, "@u", userId); }, r => list.Add(Read(r)));
            return list;
        }

        /// <summary>يربط المرفقات (غير المرسلة بعد) برسالة المستخدم. يرجع عدد ما رُبط.</summary>
        public int AttachToMessage(IEnumerable<long> ids, long conversationId, long messageId, long userId)
        {
            int n = 0;
            var db = _newOpp().db;
            foreach (var id in ids?.Where(i => i > 0).Distinct() ?? Enumerable.Empty<long>())
                n += db.Database.ExecuteSqlInterpolated($@"UPDATE AI_Attachments SET Conversation_Id = {conversationId}, Message_Id = {messageId}
WHERE Id = {id} AND User_Id = {userId} AND Message_Id IS NULL AND Status = {AiAttachmentStatus.Active}");
            return n;
        }

        /// <summary>يحفظ آخر استخراج (يُعاد استخدامه بلا نموذج لنفس النوع).</summary>
        public void SetExtraction(long id, long userId, string docType, string json, long? modelId, int? inTok, int? outTok, decimal? cost, string docNo)
        {
            DateTime now = DateTime.Now;
            _newOpp().db.Database.ExecuteSqlInterpolated($@"
UPDATE AI_Attachments SET ExtractDocType = {docType}, ExtractJson = {json}, ExtractModel_Id = {modelId}, ExtractInputTokens = {inTok},
       ExtractOutputTokens = {outTok}, ExtractCost = {AiSqlParams.Cost(cost)}, ExtractedAt = {now}, Doc_No = {Cut(docNo, 60)}, UpdatedBy = {userId}, UpdatedAt = {now}
WHERE Id = {id} AND User_Id = {userId}");
        }

        /// <summary>رُفع نفس الملف (نفس البصمة) من قبل: أحدث 5 صفوف غيره (لأى مستخدم؛ يُعرض التاريخ فقط).</summary>
        public List<(long Id, DateTime CreatedAt, bool SameUser)> FindSameFile(string sha256, long excludeId, long userId)
        {
            var list = new List<(long, DateTime, bool)>();
            Exec(_newOpp().db, "SELECT TOP 5 Id, CreatedAt, User_Id FROM AI_Attachments WHERE Sha256 = @h AND Id <> @x ORDER BY CreatedAt DESC",
                cmd => { Add(cmd, "@h", sha256); Add(cmd, "@x", excludeId); },
                r => list.Add((r.GetInt64(0), r.IsDBNull(1) ? DateTime.MinValue : r.GetDateTime(1), !r.IsDBNull(2) && r.GetInt64(2) == userId)));
            return list;
        }

        /// <summary>مرفق لم يُرسل بعد (أزاله المستخدم قبل الإرسال): يُحذف صفه (جديد لم يُستخدم). يرجع مسار الملف لحذفه.</summary>
        public string DeleteUnsent(long id, long userId)
        {
            var row = Get(id, userId);
            if (row == null || row.Message_Id != null) return null;
            _newOpp().db.Database.ExecuteSqlInterpolated($"DELETE FROM AI_Attachments WHERE Id = {id} AND User_Id = {userId} AND Message_Id IS NULL");
            return row.StoragePath;
        }

        /// <summary>المرفقات النشطة الأقدم من مدة الاحتفاظ، أو مرفقات محادثة محذوفة (conversationId)؛ تُعلَّم Expired بعد حذف ملفاتها.</summary>
        public List<(long Id, string Path)> ExpiryCandidates(int retentionDays, long? conversationId = null, long userId = 0, int top = 200)
        {
            var list = new List<(long, string)>();
            if (conversationId != null)
                Exec(_newOpp().db, "SELECT TOP (@t) Id, StoragePath FROM AI_Attachments WHERE Status = @s AND Conversation_Id = @c AND User_Id = @u",
                    cmd => { Add(cmd, "@t", top); Add(cmd, "@s", AiAttachmentStatus.Active); Add(cmd, "@c", conversationId.Value); Add(cmd, "@u", userId); },
                    r => list.Add((r.GetInt64(0), r.IsDBNull(1) ? "" : r.GetString(1))));
            else
                Exec(_newOpp().db, "SELECT TOP (@t) Id, StoragePath FROM AI_Attachments WHERE Status = @s AND CreatedAt < @d ORDER BY CreatedAt",
                    cmd => { Add(cmd, "@t", top); Add(cmd, "@s", AiAttachmentStatus.Active); Add(cmd, "@d", DateTime.Now.AddDays(-Math.Max(1, retentionDays))); },
                    r => list.Add((r.GetInt64(0), r.IsDBNull(1) ? "" : r.GetString(1))));
            return list;
        }

        public void MarkExpired(IEnumerable<long> ids)
        {
            var all = ids?.Where(i => i > 0).Distinct().ToList() ?? new List<long>();
            if (all.Count == 0) return;
            DateTime now = DateTime.Now;
            var db = _newOpp().db;
            foreach (var id in all)
                db.Database.ExecuteSqlInterpolated($"UPDATE AI_Attachments SET Status = {AiAttachmentStatus.Expired}, ExpiredAt = {now} WHERE Id = {id} AND Status = {AiAttachmentStatus.Active}");
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
