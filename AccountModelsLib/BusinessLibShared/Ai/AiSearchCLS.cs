using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib
{
    /// <summary>
    /// P8.2 (س19–س21): «الأخيرة» فى لوحة البحث (AI_SearchRecent، جدول جديد) بنمط AiBriefCLS. صف لكل (مستخدم، نوع، مفتاح).
    /// «مسح الأخيرة» يضع HiddenAt ولا يحذف؛ إعادة الاستعمال تمسحه. لا يكتب إلا على هذا الجدول.
    /// </summary>
    public class AiSearchCLS
    {
        private readonly Func<operations> _newOpp;

        public AiSearchCLS(Func<operations> newOpp = null)
        {
            _newOpp = newOpp ?? (() => new operations());
        }

        /// <summary>يسجّل فتح عنصر: يزيد HitCount ويحدّث اللقطة إن وُجد الصف، وإلا يُدرج صفاً بـ HitCount = 1.</summary>
        public void Touch(long userId, string kind, string refKey, string title, string subtitle, string url)
        {
            if (userId <= 0) throw new ArgumentException("userId");
            kind = Cut((kind ?? "").Trim(), 20);
            refKey = Cut((refKey ?? "").Trim(), 300);
            if (kind.Length == 0 || refKey.Length == 0 || string.IsNullOrWhiteSpace(url)) throw new ArgumentException("kind/refKey/url");
            title = Cut(title ?? "", 300);
            subtitle = Cut(subtitle, 300);
            url = Cut(url, 400);

            var db = _newOpp().db;
            if (Update(db, userId, kind, refKey, title, subtitle, url) > 0) return;
            int inserted = 0;
            try
            {
                long id = CallSnowflakeIdGenerate.GenerateNewId();
                DateTime now = DateTime.Now;
                inserted = db.Database.ExecuteSqlInterpolated($@"
INSERT INTO AI_SearchRecent (Id, User_Id, Kind, RefKey, Title, Subtitle, Url, HitCount, LastUsedAt, CreatedBy, CreatedAt)
SELECT {id}, {userId}, {kind}, {refKey}, {title}, {subtitle}, {url}, 1, {now}, {userId}, {now}
WHERE NOT EXISTS (SELECT 1 FROM AI_SearchRecent WITH (UPDLOCK, HOLDLOCK) WHERE User_Id = {userId} AND Kind = {kind} AND RefKey = {refKey})");
            }
            catch (Exception)
            {
                // سباق نادر مع إدراج متزامن (الفهرس الفريد): الصف موجود الآن فيُحدَّث.
                if (Update(db, userId, kind, refKey, title, subtitle, url) > 0) return;
                throw;
            }
            // سبقه إدراج متزامن بين التحديث والإدراج: لا تضيع هذه المرة.
            if (inserted == 0) Update(db, userId, kind, refKey, title, subtitle, url);
        }

        private static int Update(My_DbContext db, long userId, string kind, string refKey, string title, string subtitle, string url)
        {
            DateTime now = DateTime.Now;
            return db.Database.ExecuteSqlInterpolated($@"
UPDATE AI_SearchRecent SET HitCount = HitCount + 1, LastUsedAt = {now}, HiddenAt = NULL, Title = {title}, Subtitle = {subtitle}, Url = {url},
       UpdatedBy = {userId}, UpdatedAt = {now}
WHERE User_Id = {userId} AND Kind = {kind} AND RefKey = {refKey}");
        }

        /// <summary>العناصر الظاهرة (HiddenAt IS NULL) الأحدث أولاً.</summary>
        public List<AiSearchRecentRow> List(long userId, int top)
        {
            var list = new List<AiSearchRecentRow>();
            Exec($@"SELECT TOP ({Math.Max(1, Math.Min(top, 200))}) Id, User_Id, Kind, RefKey, Title, Subtitle, Url, HitCount, LastUsedAt, HiddenAt
FROM AI_SearchRecent WHERE User_Id = @u AND HiddenAt IS NULL ORDER BY LastUsedAt DESC, Id DESC",
                cmd => Add(cmd, "@u", userId), r => list.Add(new AiSearchRecentRow
                {
                    Id = r.GetInt64(0),
                    User_Id = r.GetInt64(1),
                    Kind = S(r, 2) ?? "",
                    RefKey = S(r, 3) ?? "",
                    Title = S(r, 4) ?? "",
                    Subtitle = S(r, 5),
                    Url = S(r, 6) ?? "",
                    HitCount = Convert.ToInt32(r.GetValue(7)),
                    LastUsedAt = r.GetDateTime(8),
                    HiddenAt = r.IsDBNull(9) ? (DateTime?)null : r.GetDateTime(9),
                }));
            return list;
        }

        /// <summary>«مسح الأخيرة» (س20): إخفاء بلا حذف. يعيد عدد الصفوف المخفاة.</summary>
        public int HideAll(long userId)
        {
            DateTime now = DateTime.Now;
            return _newOpp().db.Database.ExecuteSqlInterpolated($@"
UPDATE AI_SearchRecent SET HiddenAt = {now}, UpdatedBy = {userId}, UpdatedAt = {now}
WHERE User_Id = {userId} AND HiddenAt IS NULL");
        }

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
