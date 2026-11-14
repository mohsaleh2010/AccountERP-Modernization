using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using AccountERP.AI.Tools;
using AccountModelsLib;

namespace AccountERP.AI.Knowledge
{
    /// <summary>
    /// P7.1: معرفة المساعد من جدول AI_Knowledge بدل الكود (ai-assistant-p7-plan.md D5–D7).
    /// المزامنة (مرة لكل عملية): صفوف view تُنسخ دائماً من AiSchemaCatalog.Views (مرتبطة بـ AiReadViewsCLS والصلاحيات)؛
    /// المسرد/القواعد/الأمثلة تُدرج إن نقصت، وتُحدَّث فقط إن كانت مزروعة وغير معدَّلة ونسختها فى الكود أعلى.
    /// البحث بالكلمات بنفس منطق P2، مع فلترة الصلاحية قبل الترتيب: صف view = الـ View مسموح، وغيره = كل ViewsCsv مسموحة وأى من RoutesCsv.
    /// إن تعذّرت قراءة الجدول يُستخدم بحث الكود القديم فلا تفشل search_schema أبداً.
    /// </summary>
    public sealed class AiKnowledgeService
    {
        private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan SyncRetry = TimeSpan.FromMinutes(1);
        private static readonly Regex ViewRef = new(@"\bai\s*\.\s*\[?(v_[a-z0-9_]+)\]?", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly AiKnowledgeCLS _cls;
        private readonly object _gate = new();
        private bool _synced;
        private DateTime _syncFailedAt = DateTime.MinValue;
        private List<AiKnowledgeRow>? _cache;
        private DateTime _cacheAt;

        public AiKnowledgeService(AiKnowledgeCLS? cls = null)
        {
            _cls = cls ?? new AiKnowledgeCLS();
        }

        /// <summary>آخر خطأ مزامنة/قراءة (للعرض فى تبويب المعرفة).</summary>
        public string? LastError { get; private set; }

        /// <summary>هل استُخدم بحث الكود الاحتياطى فى آخر بحث (للاختبار والعرض).</summary>
        public bool LastSearchUsedFallback { get; private set; }

        public void Invalidate()
        {
            lock (_gate) _cache = null;
        }

        /// <summary>يزامن الكود مع الجدول مرة لكل عملية (ويعيد المحاولة بعد دقيقة إن فشل).</summary>
        public void EnsureSynced()
        {
            lock (_gate)
            {
                if (_synced || DateTime.UtcNow - _syncFailedAt < SyncRetry) return;
                try
                {
                    SyncSeed();
                    _synced = true;
                    _cache = null;
                }
                catch (Exception ee)
                {
                    _syncFailedAt = DateTime.UtcNow;
                    LastError = ee.Message;
                    operations.DataDictionaryLogerException(ee);
                }
            }
        }

        /// <summary>المزامنة الفعلية (عامة للاختبار). لا تحذف صفاً ولا تمس صفاً عدّله المدير.</summary>
        public void SyncSeed()
        {
            var existing = _cls.ListAll().ToDictionary(r => r.Kind + "|" + r.KeyName, StringComparer.OrdinalIgnoreCase);

            foreach (var row in SeedRows())
            {
                if (!existing.TryGetValue(row.Kind + "|" + row.KeyName, out var cur))
                {
                    _cls.Insert(row, null);
                    continue;
                }
                bool isView = row.Kind == AiKnowledgeKinds.View;
                bool seedOwned = cur.Source == AiKnowledgeSources.Seed && !cur.IsEdited;
                if (isView ? cur.ContentHash != row.ContentHash : seedOwned && row.Version > cur.Version)
                {
                    row.Id = cur.Id;
                    _cls.UpdateFromSeed(row);
                }
            }
        }

        /// <summary>صفوف الكود بشكل AI_Knowledge: الـ Views بترتيبها ثم المسرد والقواعد والأمثلة.</summary>
        public static List<AiKnowledgeRow> SeedRows()
        {
            var list = new List<AiKnowledgeRow>();
            foreach (var v in AiSchemaCatalog.Views)
                list.Add(Hash(new AiKnowledgeRow
                {
                    Kind = AiKnowledgeKinds.View,
                    KeyName = v.Name,
                    TitleAr = v.TitleAr,
                    Content = v.Description + "\nالأعمدة: " + string.Join("، ", v.Columns.Select(c => c.Col + " (" + c.Desc + ")")),
                    Keywords = v.Name,
                    ViewsCsv = v.Name,
                    RoutesCsv = v.Routes.Length == 0 ? null : string.Join(",", v.Routes),
                    Source = AiKnowledgeSources.Seed,
                    Version = 1,
                }));
            foreach (var k in AiSchemaCatalog.Knowledge)
                list.Add(Hash(new AiKnowledgeRow
                {
                    Kind = k.Kind,
                    KeyName = k.Key,
                    TitleAr = k.Title,
                    Content = k.Content,
                    Keywords = k.Keywords,
                    ViewsCsv = k.Kind == AiKnowledgeKinds.Example ? ViewsOf(k.Content) : null,
                    Source = AiKnowledgeSources.Seed,
                    Version = k.Version,
                }));
            return list;
        }

        /// <summary>الـ Views التى يشير إليها استعلام مثال (ai.v_xxx) — كلها لازمة لرؤية المثال.</summary>
        public static string? ViewsOf(string sql)
        {
            var names = ViewRef.Matches(sql ?? "").Select(m => m.Groups[1].Value.ToLowerInvariant()).Distinct().OrderBy(n => n).ToList();
            return names.Count == 0 ? null : string.Join(",", names);
        }

        public static AiKnowledgeRow Hash(AiKnowledgeRow r)
        {
            string raw = r.TitleAr + "\n" + r.Content + "\n" + r.Keywords + "\n" + r.ViewsCsv + "\n" + r.RoutesCsv;
            r.ContentHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw))).ToLowerInvariant();
            return r;
        }

        /// <summary>الصفوف المفعّلة (مخزّنة 5 دقائق، وتُلغى عند أى كتابة من الإدارة). null = تعذّرت القراءة.</summary>
        public List<AiKnowledgeRow>? ActiveRows()
        {
            EnsureSynced();
            lock (_gate)
            {
                if (_cache != null && DateTime.UtcNow - _cacheAt < CacheTtl) return _cache;
            }
            try
            {
                var rows = _cls.ListAll().Where(r => r.IsActive).ToList();
                // الترتيب = ترتيب P2: الـ Views بترتيب الكتالوج، ثم الباقى بترتيب الإدراج.
                var viewOrder = AiSchemaCatalog.Views.Select((v, i) => (v.Name, i)).ToDictionary(x => x.Name, x => x.i, StringComparer.OrdinalIgnoreCase);
                rows = rows.OrderBy(r => r.Kind == AiKnowledgeKinds.View ? 0 : 1)
                           .ThenBy(r => r.Kind == AiKnowledgeKinds.View && viewOrder.TryGetValue(r.KeyName, out int i) ? i : int.MaxValue)
                           .ThenBy(r => r.Id).ToList();
                lock (_gate) { _cache = rows; _cacheAt = DateTime.UtcNow; }
                return rows;
            }
            catch (Exception ee)
            {
                LastError = ee.Message;
                operations.DataDictionaryLogerException(ee);
                return null;
            }
        }

        /// <summary>هل يرى المستخدم هذا المقطع (قبل الترتيب).</summary>
        public static bool Visible(AiKnowledgeRow r, ISet<string> allowedViews, Func<string, bool> hasRoute)
        {
            if (r.Kind == AiKnowledgeKinds.View) return allowedViews.Contains(r.KeyName);
            if (!string.IsNullOrWhiteSpace(r.ViewsCsv)
                && !r.ViewsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).All(allowedViews.Contains))
                return false;
            if (!string.IsNullOrWhiteSpace(r.RoutesCsv)
                && !r.RoutesCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Any(hasRoute))
                return false;
            return true;
        }

        /// <summary>ترتيب بالكلمات للمقاطع المرئية (الـ View يأخذ +1 كما فى P2). يعيد (الصف، الدرجة) بالترتيب.</summary>
        public static List<(AiKnowledgeRow Row, int Score)> KeywordRank(string query, IEnumerable<AiKnowledgeRow> visible)
        {
            var words = Words(query);
            var hits = new List<(AiKnowledgeRow, int)>();
            foreach (var r in visible)
            {
                // اسم الـ View جزء من نص البحث (كما فى P2)؛ مفتاح المقاطع الأخرى داخلى فلا يدخل.
                string hay = AiSchemaCatalog.Normalize(((r.Kind == AiKnowledgeKinds.View ? r.KeyName + " " : "") + r.TitleAr + " " + r.Content + " " + r.Keywords).ToLowerInvariant());
                int s = words.Count(w => hay.Contains(w));
                if (s > 0) hits.Add((r, r.Kind == AiKnowledgeKinds.View ? s + 1 : s));
            }
            return hits.OrderByDescending(h => h.Item2).ToList();
        }

        public static List<string> Words(string? query) => (query ?? "").ToLowerInvariant()
            .Split(new[] { ' ', '،', ',', '.', '?', '؟', '-', '_', '/' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(AiSchemaCatalog.Normalize).Where(w => w.Length >= 2).Distinct().ToList();

        /// <summary>نص المقطع كما يراه النموذج (نفس شكل P2).</summary>
        public static string Format(AiKnowledgeRow r) => r.Kind == AiKnowledgeKinds.View
            ? "[view] ai." + r.KeyName + " — " + r.TitleAr + ": " + r.Content
            : "[" + r.Kind + "] " + r.TitleAr + ": " + r.Content;

        /// <summary>بحث search_schema: من الجدول، وإلا من الكود.</summary>
        public string Search(string query, ISet<string> allowedViews, Func<string, bool> hasRoute, int top = 6)
        {
            var rows = ActiveRows();
            if (rows == null || rows.Count == 0)
            {
                LastSearchUsedFallback = true;
                return AiSchemaCatalog.Search(query, AiSchemaCatalog.Views.Where(v => allowedViews.Contains(v.Name)), top);
            }
            LastSearchUsedFallback = false;
            var best = KeywordRank(query, rows.Where(r => Visible(r, allowedViews, hasRoute))).Take(top).Select(h => Format(h.Row)).ToList();
            return best.Count == 0 ? "لا نتائج. الـ Views المتاحة مذكورة فى التعليمات." : string.Join("\n\n", best);
        }
    }
}
