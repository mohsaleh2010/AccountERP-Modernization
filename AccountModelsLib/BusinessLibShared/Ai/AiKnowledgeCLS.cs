using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib
{
    /// <summary>P7: أنواع مقاطع المعرفة فى AI_Knowledge.</summary>
    public static class AiKnowledgeKinds
    {
        public const string View = "view";
        public const string Glossary = "glossary";
        public const string Rule = "rule";
        public const string Example = "example";
        public const string Screen = "screen";

        public static readonly string[] All = { View, Glossary, Rule, Example, Screen };
    }

    /// <summary>P7: مصدر المقطع — seed (من الكود) / manual (أضافه المدير) / feedback (من تقييمات المستخدمين بعد المراجعة).</summary>
    public static class AiKnowledgeSources
    {
        public const string Seed = "seed";
        public const string Manual = "manual";
        public const string Feedback = "feedback";
    }

    /// <summary>صف AI_Knowledge (بلا متجه التضمين نفسه؛ يُقرأ منفصلاً فى P7.2).</summary>
    public class AiKnowledgeRow
    {
        public long Id { get; set; }
        public string Kind { get; set; } = "";
        public string KeyName { get; set; } = "";
        public string TitleAr { get; set; } = "";
        public string Content { get; set; } = "";
        public string Keywords { get; set; }
        /// <summary>الـ Views التى يلزم أن يملكها المستخدم كلها ليرى المقطع (فارغ = للجميع).</summary>
        public string ViewsCsv { get; set; }
        /// <summary>شاشات تمنح المقطع (أى واحدة تكفى؛ فارغ = بلا شرط).</summary>
        public string RoutesCsv { get; set; }
        public string Source { get; set; } = AiKnowledgeSources.Seed;
        public long? Source_Ref_Id { get; set; }
        public int Version { get; set; } = 1;
        public bool IsEdited { get; set; }
        public bool IsActive { get; set; } = true;
        public string ContentHash { get; set; }
        public long? EmbeddingModel_Id { get; set; }
        public int? EmbeddingDim { get; set; }
        public DateTime? EmbeddedAt { get; set; }
    }

    /// <summary>
    /// P7: مقاطع معرفة المساعد (AI_Knowledge، جدول جديد): قاموس الـ Views والمسرد والقواعد والأمثلة. لا قراءة ولا كتابة فى جداول النظام القائمة.
    /// </summary>
    public class AiKnowledgeCLS
    {
        private readonly Func<operations> _newOpp;

        public AiKnowledgeCLS(Func<operations> newOpp = null)
        {
            _newOpp = newOpp ?? (() => new operations());
        }

        private const string Select = @"SELECT Id, Kind, KeyName, TitleAr, Content, Keywords, ViewsCsv, RoutesCsv, Source, Source_Ref_Id, Version, IsEdited, IsActive,
       ContentHash, EmbeddingModel_Id, EmbeddingDim, EmbeddedAt
FROM AI_Knowledge";

        private static AiKnowledgeRow Read(DbDataReader r) => new AiKnowledgeRow
        {
            Id = r.GetInt64(0),
            Kind = r.IsDBNull(1) ? "" : r.GetString(1),
            KeyName = r.IsDBNull(2) ? "" : r.GetString(2),
            TitleAr = r.IsDBNull(3) ? "" : r.GetString(3),
            Content = r.IsDBNull(4) ? "" : r.GetString(4),
            Keywords = r.IsDBNull(5) ? null : r.GetString(5),
            ViewsCsv = r.IsDBNull(6) ? null : r.GetString(6),
            RoutesCsv = r.IsDBNull(7) ? null : r.GetString(7),
            Source = r.IsDBNull(8) ? AiKnowledgeSources.Seed : r.GetString(8),
            Source_Ref_Id = r.IsDBNull(9) ? (long?)null : r.GetInt64(9),
            Version = r.IsDBNull(10) ? 1 : r.GetInt32(10),
            IsEdited = !r.IsDBNull(11) && r.GetBoolean(11),
            IsActive = r.IsDBNull(12) || r.GetBoolean(12),
            ContentHash = r.IsDBNull(13) ? null : r.GetString(13).Trim(),
            EmbeddingModel_Id = r.IsDBNull(14) ? (long?)null : r.GetInt64(14),
            EmbeddingDim = r.IsDBNull(15) ? (int?)null : r.GetInt32(15),
            EmbeddedAt = r.IsDBNull(16) ? (DateTime?)null : r.GetDateTime(16),
        };

        /// <summary>كل الصفوف (المفعّلة وغيرها) بترتيب الإدراج.</summary>
        public List<AiKnowledgeRow> ListAll()
        {
            var list = new List<AiKnowledgeRow>();
            Exec(_newOpp().db, Select + " ORDER BY Id", _ => { }, r => list.Add(Read(r)));
            return list;
        }

        public AiKnowledgeRow Get(long id)
        {
            AiKnowledgeRow found = null;
            Exec(_newOpp().db, Select + " WHERE Id = @id", cmd => Add(cmd, "@id", id), r => found = Read(r));
            return found;
        }

        public void Insert(AiKnowledgeRow k, long? userId)
        {
            if (k.Id == 0) k.Id = CallSnowflakeIdGenerate.GenerateNewId();
            _newOpp().db.Database.ExecuteSqlInterpolated($@"
INSERT INTO AI_Knowledge (Id, Kind, KeyName, TitleAr, Content, Keywords, ViewsCsv, RoutesCsv, Source, Source_Ref_Id, Version, IsEdited, IsActive, ContentHash, CreatedBy, CreatedAt)
VALUES ({k.Id}, {Cut(k.Kind, 20)}, {Cut(k.KeyName, 150)}, {Cut(k.TitleAr, 300)}, {k.Content ?? ""}, {Cut(k.Keywords, 1000)}, {Cut(k.ViewsCsv, 500)}, {Cut(k.RoutesCsv, 1000)},
        {Cut(k.Source, 30)}, {k.Source_Ref_Id}, {k.Version}, {k.IsEdited}, {k.IsActive}, {k.ContentHash}, {userId}, {DateTime.Now})");
        }

        /// <summary>تحديث المحتوى من الكود (seed). المتجه يُمسح إن تغيّر المحتوى ليُعاد بناؤه.</summary>
        public void UpdateFromSeed(AiKnowledgeRow k)
        {
            _newOpp().db.Database.ExecuteSqlInterpolated($@"
UPDATE AI_Knowledge SET TitleAr = {Cut(k.TitleAr, 300)}, Content = {k.Content ?? ""}, Keywords = {Cut(k.Keywords, 1000)}, ViewsCsv = {Cut(k.ViewsCsv, 500)},
       RoutesCsv = {Cut(k.RoutesCsv, 1000)}, Version = {k.Version}, ContentHash = {k.ContentHash},
       Embedding = CASE WHEN ISNULL(ContentHash, '') = {k.ContentHash ?? ""} THEN Embedding ELSE NULL END,
       EmbeddingModel_Id = CASE WHEN ISNULL(ContentHash, '') = {k.ContentHash ?? ""} THEN EmbeddingModel_Id ELSE NULL END,
       EmbeddingDim = CASE WHEN ISNULL(ContentHash, '') = {k.ContentHash ?? ""} THEN EmbeddingDim ELSE NULL END,
       EmbeddedAt = CASE WHEN ISNULL(ContentHash, '') = {k.ContentHash ?? ""} THEN EmbeddedAt ELSE NULL END,
       UpdatedAt = {DateTime.Now}
WHERE Id = {k.Id}");
        }

        public void SetActive(long id, bool active, long? userId)
        {
            _newOpp().db.Database.ExecuteSqlInterpolated(
                $"UPDATE AI_Knowledge SET IsActive = {active}, UpdatedBy = {userId}, UpdatedAt = {DateTime.Now} WHERE Id = {id}");
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
