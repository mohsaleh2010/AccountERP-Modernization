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
        private readonly AiKnowledgeIndex _index;
        private readonly AiEmbeddingService? _embed;
        private readonly object _gate = new();
        private bool _synced;
        private DateTime _syncFailedAt = DateTime.MinValue;
        private List<AiKnowledgeRow>? _cache;
        private DateTime _cacheAt;

        public AiKnowledgeService(AiKnowledgeCLS? cls = null, AiEmbeddingService? embed = null, AiKnowledgeIndex? index = null)
        {
            _cls = cls ?? new AiKnowledgeCLS();
            _embed = embed;
            _index = index ?? new AiKnowledgeIndex(_cls);
        }

        public AiEmbeddingService? Embeddings => _embed;

        /// <summary>آخر خطأ مزامنة/قراءة (للعرض فى تبويب المعرفة).</summary>
        public string? LastError { get; private set; }

        /// <summary>هل استُخدم بحث الكود الاحتياطى فى آخر بحث (للاختبار والعرض).</summary>
        public bool LastSearchUsedFallback { get; private set; }

        /// <summary>P7.2: طريقة آخر بحث: keyword / hybrid / fallback (للاختبار وشارة «تجربة البحث» فى P7.3).</summary>
        public string LastSearchMode { get; private set; } = SearchModes.Keyword;

        public static class SearchModes
        {
            public const string Keyword = "keyword";
            public const string Hybrid = "hybrid";
            public const string Fallback = "fallback";
        }

        public void Invalidate()
        {
            lock (_gate) _cache = null;
            _index.Invalidate();
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

        /// <summary>
        /// P7.2: البحث الهجين (D4، D8). بلا نموذج تضمين / قاطع مفتوح / خطأ / مهلة → نفس ناتج Search بالكلمات حرفياً.
        /// المتجهات تُقارن بالمقاطع المرئية للمستخدم فقط، فلا يظهر مقطع خارج صلاحيته.
        /// </summary>
        public async Task<string> SearchAsync(string query, ISet<string> allowedViews, Func<string, bool> hasRoute, CancellationToken ct, int top = 6)
        {
            var (rows, mode) = await RankAsync(query, allowedViews, hasRoute, ct, top);
            LastSearchMode = mode;
            LastSearchUsedFallback = mode == SearchModes.Fallback;
            if (rows == null) return AiSchemaCatalog.Search(query, AiSchemaCatalog.Views.Where(v => allowedViews.Contains(v.Name)), top);
            return rows.Count == 0 ? NoResults : string.Join("\n\n", rows.Select(Format));
        }

        /// <summary>
        /// P7.3: الترتيب نفسه الذى تستخدمه SearchAsync، بصفوف منظَّمة (لـ «تجربة البحث»). Rows = null ⇒ fallback (بحث الكود).
        /// </summary>
        public Task<(List<AiKnowledgeRow>? Rows, string Mode)> RankAsync(string query, ISet<string> allowedViews, Func<string, bool> hasRoute, CancellationToken ct, int top = 6) =>
            RankAsync(query, allowedViews, hasRoute, ct, top, null, 1);

        private async Task<(List<AiKnowledgeRow>? Rows, string Mode)> RankAsync(string query, ISet<string> allowedViews, Func<string, bool> hasRoute, CancellationToken ct, int top,
                                                                                Func<AiKnowledgeRow, bool>? only, int minKeywordScore)
        {
            var rows = ActiveRows();
            if (rows == null || rows.Count == 0) return (null, SearchModes.Fallback);

            var visible = rows.Where(r => Visible(r, allowedViews, hasRoute) && (only == null || only(r))).ToList();
            if (visible.Count == 0) return (new List<AiKnowledgeRow>(), SearchModes.Keyword);
            var keyword = KeywordRank(query, visible).Where(h => h.Score >= minKeywordScore).Select(h => h.Row).ToList();
            if (_embed == null) return (keyword.Take(top).ToList(), SearchModes.Keyword);
            List<AiKnowledgeRow> vector = new();
            // فهرس فارغ لهذا النموذج (لم يُبنَ بعد) = لا داعى لتضمين السؤال وصرف رصيد بلا فائدة.
            var route = _embed.CurrentRoute(out _);
            Dictionary<long, (string? Hash, float[] Vec)>? vectors = null;
            if (route != null)
            {
                try { vectors = _index.VectorsFor(route.Model.Id); }
                catch (Exception ee)
                {
                    LastError = ee.Message;
                    operations.DataDictionaryLogerException(ee);
                }
            }
            if (vectors != null && vectors.Count > 0)
            {
                var (qv, modelId) = await _embed.EmbedQueryAsync(query, ct);
                if (qv != null && modelId == route!.Model.Id)
                    vector = AiKnowledgeIndex.VectorRank(qv, visible, vectors).Select(h => h.Row).ToList();
            }
            if (vector.Count == 0) return (keyword.Take(top).ToList(), SearchModes.Keyword);
            return (AiKnowledgeIndex.Fuse(keyword.Take(AiKnowledgeIndex.Depth).ToList(), vector, top), SearchModes.Hybrid);
        }

        // ───────────── P7.7: الأمثلة التلقائية (few-shot) ─────────────

        /// <summary>
        /// P7.7: أقرب أمثلة معتمدة (Kind=example) لسؤال المستخدم من المرئى له فقط، بنفس الترتيب الهجين. بالكلمات وحدها يُشترط تطابق كلمتين
        /// (أو كل كلمات سؤال قصير) حتى لا يُلحق مثال بعيد. exclude: يُستبعد ما يطابق نصه (التقييم يستبعد مثالاً هو السؤال نفسه). لا يرمى.
        /// </summary>
        public async Task<List<AiKnowledgeRow>> ExamplesAsync(string query, ISet<string> allowedViews, Func<string, bool> hasRoute, int count, CancellationToken ct,
                                                              Func<AiKnowledgeRow, bool>? exclude = null)
        {
            if (count <= 0 || string.IsNullOrWhiteSpace(query)) return new();
            try
            {
                int words = Words(query).Count;
                if (words == 0) return new();
                var (rows, _) = await RankAsync(query, allowedViews, hasRoute, ct, count, r => r.Kind == AiKnowledgeKinds.Example && (exclude == null || !exclude(r)), Math.Min(2, words));
                return rows ?? new();
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                LastError = ex.Message;
                operations.DataDictionaryLogerException(ex);
                return new();
            }
        }

        /// <summary>P7.7: كتلة الأمثلة كما تُلحق بآخر رسالة للمستخدم (للنموذج فقط، لا تُحفظ فى المحادثة).</summary>
        public static string ExamplesBlock(IReadOnlyList<AiKnowledgeRow> examples)
        {
            if (examples == null || examples.Count == 0) return "";
            var sb = new System.Text.StringBuilder("\n\n---\n(أمثلة معتمدة مشابهة من قاعدة المعرفة، للاسترشاد فقط: تأكّد أنها تناسب السؤال وعدّل الاستعلام حسبه، ولا تذكرها للمستخدم.)");
            int i = 0;
            foreach (var r in examples) sb.Append('\n').Append(++i).Append(") ").Append(r.TitleAr).Append(":\n").Append(r.Content);
            return sb.ToString();
        }

        /// <summary>P7.7: عدد المقاطع المفعّلة من نوع مثال.</summary>
        public int ActiveExampleCount() => ActiveRows()?.Count(r => r.Kind == AiKnowledgeKinds.Example) ?? 0;

        // ───────────── P7.3: إدارة المقاطع من تبويب «المعرفة» ─────────────

        public const int MaxTitle = 300, MaxContent = 4000, MaxKeywords = 1000;

        /// <summary>كل المقاطع للإدارة (بعد المزامنة): الـ Views بترتيب الكتالوج ثم المسرد والقواعد والأمثلة.</summary>
        public List<AiKnowledgeRow> ListForAdmin()
        {
            EnsureSynced();
            var viewOrder = AiSchemaCatalog.Views.Select((v, i) => (v.Name, i)).ToDictionary(x => x.Name, x => x.i, StringComparer.OrdinalIgnoreCase);
            return _cls.ListAll()
                .OrderBy(r => { int k = Array.IndexOf(AiKnowledgeKinds.All, r.Kind); return k < 0 ? 99 : k; })
                .ThenBy(r => r.Kind == AiKnowledgeKinds.View && viewOrder.TryGetValue(r.KeyName, out int i) ? i : int.MaxValue)
                .ThenBy(r => r.Id).ToList();
        }

        /// <summary>
        /// حفظ مسرد/قاعدة/مثال. الـ Views للقراءة فقط (من الكود). المثال = SELECT واحد على Views المساعد (ReadOnlySqlExecutor.Validate)
        /// وViewsCsv مستنتج منه. المقطع المزروع يُعلَّم «معدَّل» فلا يغيّره الكود بعد ذلك. يرمى InvalidOperationException برسالة عربية.
        /// </summary>
        public long SaveAdmin(AiKnowledgeSaveRequest r, long userId)
        {
            string kind = (r.Kind ?? "").Trim().ToLowerInvariant();
            string title = (r.TitleAr ?? "").Trim(), content = (r.Content ?? "").Trim(), keywords = (r.Keywords ?? "").Trim();
            if (kind != AiKnowledgeKinds.Glossary && kind != AiKnowledgeKinds.Rule && kind != AiKnowledgeKinds.Example)
                throw new InvalidOperationException("نوع المقطع غير مسموح (مسرد أو قاعدة أو مثال فقط).");
            if (title.Length == 0) throw new InvalidOperationException("أدخل عنوان المقطع.");
            if (content.Length == 0) throw new InvalidOperationException(kind == AiKnowledgeKinds.Example ? "أدخل استعلام المثال." : "أدخل نص المقطع.");
            if (title.Length > MaxTitle) throw new InvalidOperationException($"العنوان أطول من {MaxTitle} حرف.");
            if (content.Length > MaxContent) throw new InvalidOperationException($"النص أطول من {MaxContent} حرف.");
            if (keywords.Length > MaxKeywords) throw new InvalidOperationException($"الكلمات المفتاحية أطول من {MaxKeywords} حرف.");

            string? views = null;
            if (kind == AiKnowledgeKinds.Example)
            {
                var all = new HashSet<string>(AiSchemaCatalog.Views.Select(v => v.Name), StringComparer.OrdinalIgnoreCase);
                try { ReadOnlySqlExecutor.Validate(content, all, new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "date_from", "date_to" }); }
                catch (AiSqlRejectedException ee) { throw new InvalidOperationException("استعلام المثال مرفوض: " + ee.Message); }
                views = ViewsOf(content) ?? throw new InvalidOperationException("استعلام المثال لا يستخدم أى View من Views المساعد (ai.v_...).");
            }

            EnsureSynced();
            long id;
            if (r.Id == 0)
            {
                var row = Hash(new AiKnowledgeRow
                {
                    Id = CallSnowflakeIdGenerate.GenerateNewId(),
                    Kind = kind, TitleAr = title, Content = content, Keywords = keywords.Length == 0 ? null : keywords, ViewsCsv = views,
                    Source = r.Source == AiKnowledgeSources.Feedback ? AiKnowledgeSources.Feedback : AiKnowledgeSources.Manual,
                    Source_Ref_Id = r.Source == AiKnowledgeSources.Feedback ? r.Source_Ref_Id : null, Version = 1, IsActive = true,
                });
                row.KeyName = row.Source + "_" + row.Id.ToString(System.Globalization.CultureInfo.InvariantCulture);
                _cls.Insert(row, userId);
                id = row.Id;
            }
            else
            {
                var cur = _cls.Get(r.Id) ?? throw new InvalidOperationException("المقطع غير موجود (ربما حُذف).");
                if (cur.Kind == AiKnowledgeKinds.View || cur.Kind == AiKnowledgeKinds.Screen)
                    throw new InvalidOperationException("مقاطع الـ Views تُدار من الكود ولا تُعدَّل من هنا.");
                if (cur.Kind != kind) throw new InvalidOperationException("لا يمكن تغيير نوع مقطع موجود؛ أضف مقطعاً جديداً.");
                var row = Hash(new AiKnowledgeRow
                {
                    Id = cur.Id, Kind = kind, TitleAr = title, Content = content, Keywords = keywords.Length == 0 ? null : keywords,
                    ViewsCsv = views, RoutesCsv = cur.RoutesCsv, Version = cur.Version,
                });
                if (row.ContentHash == cur.ContentHash) return cur.Id;   // لا تغيير
                _cls.UpdateByAdmin(row, edited: cur.Source == AiKnowledgeSources.Seed || cur.IsEdited, userId);
                id = cur.Id;
            }
            Invalidate();
            return id;
        }

        public void SetActiveAdmin(long id, bool active, long userId)
        {
            var cur = _cls.Get(id) ?? throw new InvalidOperationException("المقطع غير موجود (ربما حُذف).");
            if (cur.Kind == AiKnowledgeKinds.View) throw new InvalidOperationException("مقاطع الـ Views تتبع صلاحيات المستخدم ولا تُعطَّل من هنا.");
            _cls.SetActive(id, active, userId);
            Invalidate();
        }

        /// <summary>يعيد مقطعاً مزروعاً معدَّلاً إلى نص الكود الحالى ويرفع علامة «معدَّل».</summary>
        public void RestoreSeedAdmin(long id, long userId)
        {
            var cur = _cls.Get(id) ?? throw new InvalidOperationException("المقطع غير موجود (ربما حُذف).");
            if (cur.Source != AiKnowledgeSources.Seed || !cur.IsEdited) throw new InvalidOperationException("هذا المقطع ليس مقطعاً أصلياً معدَّلاً.");
            var seed = SeedRows().FirstOrDefault(s => s.Kind == cur.Kind && string.Equals(s.KeyName, cur.KeyName, StringComparison.OrdinalIgnoreCase))
                       ?? throw new InvalidOperationException("لم يعد هذا المقطع موجوداً فى الكود.");
            seed.Id = cur.Id;
            _cls.UpdateByAdmin(seed, edited: false, userId);
            Invalidate();
        }

        /// <summary>«تجربة البحث» كمدير: كل الـ Views وكل الشاشات، بنفس ترتيب search_schema.</summary>
        public async Task<AiKnowledgeSearchTest> TestSearchAsync(string query, CancellationToken ct)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var all = new HashSet<string>(AiSchemaCatalog.Views.Select(v => v.Name), StringComparer.OrdinalIgnoreCase);
            var (rows, mode) = await RankAsync(query ?? "", all, _ => true, ct);
            var res = new AiKnowledgeSearchTest { Mode = mode, Rows = rows ?? new() };
            res.Text = rows == null ? AiSchemaCatalog.Search(query ?? "", AiSchemaCatalog.Views, 6)
                     : rows.Count == 0 ? NoResults : string.Join("\n\n", rows.Select(Format));
            res.DurationMs = sw.ElapsedMilliseconds;
            return res;
        }

        private const string NoResults = "لا نتائج. الـ Views المتاحة مذكورة فى التعليمات.";

        /// <summary>P7.2: حالة الفهرس «مفهرس n من m» لنموذج التضمين الحالى.</summary>
        public AiKnowledgeIndexStatus GetIndexStatus()
        {
            var st = new AiKnowledgeIndexStatus();
            string reason = "خدمة التضمين غير مفعّلة.";
            var route = _embed?.CurrentRoute(out reason);
            List<AiKnowledgeRow> all;
            try
            {
                EnsureSynced();
                all = _cls.ListAll().Where(r => r.IsActive).ToList();
            }
            catch (Exception ee)
            {
                st.LastError = ee.Message;
                st.Message = "تعذّرت قراءة جدول المعرفة.";
                return st;
            }
            st.Total = all.Count;
            st.PausedUntilUtc = _embed?.PausedUntilUtc;
            st.LastError = _embed?.LastError ?? LastError;
            if (route == null)
            {
                st.Message = reason;
                st.Stale = all.Count(r => r.EmbeddingModel_Id.HasValue);
                return st;
            }
            st.HasModel = true;
            st.ModelId = route.Model.Id;
            st.ModelName = route.Model.DisplayName;
            st.Indexed = all.Count(r => r.EmbeddingModel_Id == route.Model.Id && r.EmbeddedAt.HasValue);
            st.Stale = all.Count(r => r.EmbeddingModel_Id.HasValue && r.EmbeddingModel_Id != route.Model.Id);
            st.Message = st.Indexed == st.Total
                ? $"مفهرس {st.Indexed} من {st.Total} بالنموذج «{st.ModelName}»."
                : $"مفهرس {st.Indexed} من {st.Total} بالنموذج «{st.ModelName}» — اضغط «بناء الفهرس» لإكمال الباقى" + (st.Stale > 0 ? $" ({st.Stale} مفهرس بنموذج سابق)." : ".");
            return st;
        }

        /// <summary>
        /// P7.2: «بناء الفهرس» (D9): يضمّن المقاطع المفعّلة غير المفهرسة بالنموذج الحالى (أو كلها إن all=true). يكتب المتجه فقط
        /// إن لم يتغيّر محتوى الصف أثناء البناء. يرمى InvalidOperationException برسالة عربية إن لم يوجد نموذج أو فشل المزوّد.
        /// </summary>
        public async Task<AiKnowledgeRebuildResult> RebuildIndexAsync(bool all, CancellationToken ct)
        {
            if (_embed == null) throw new InvalidOperationException("خدمة التضمين غير مفعّلة.");
            _embed.InvalidateRoute();
            var route = _embed.CurrentRoute(out string reason) ?? throw new InvalidOperationException(reason);
            EnsureSynced();
            var rows = _cls.ListAll().Where(r => r.IsActive).ToList();
            var todo = rows.Where(r => all || r.EmbeddingModel_Id != route.Model.Id || !r.EmbeddedAt.HasValue).ToList();
            var res = new AiKnowledgeRebuildResult { Skipped = rows.Count - todo.Count };
            for (int i = 0; i < todo.Count; i += AiEmbeddingService.BatchSize)
            {
                ct.ThrowIfCancellationRequested();
                var batch = todo.Skip(i).Take(AiEmbeddingService.BatchSize).ToList();
                List<float[]> vecs;
                try
                {
                    vecs = await _embed.EmbedDocumentsAsync(route, batch.Select(AiKnowledgeIndex.EmbedText).ToList(), ct);
                }
                catch (Providers.AiProviderException ee)
                {
                    Invalidate();
                    throw new InvalidOperationException((res.Embedded > 0 ? $"فُهرس {res.Embedded} مقطعاً ثم توقف: " : "تعذّر بناء الفهرس: ") + ee.Message);
                }
                for (int j = 0; j < batch.Count; j++)
                {
                    if (_cls.SetEmbedding(batch[j].Id, batch[j].ContentHash, AiVectorMath.ToBytes(vecs[j]), route.Model.Id, vecs[j].Length)) res.Embedded++;
                    else res.Failed++;
                }
            }
            Invalidate();
            _embed.ResetBreaker();
            _embed.ClearQueryCache();
            res.Message = $"تمت فهرسة {res.Embedded} مقطعاً بالنموذج «{route.Model.DisplayName}»" + (res.Skipped > 0 ? $"، و{res.Skipped} مفهرس من قبل" : "")
                        + (res.Failed > 0 ? $"، و{res.Failed} تغيّر محتواه أثناء البناء (أعد البناء)." : ".");
            return res;
        }

        /// <summary>بحث search_schema: من الجدول، وإلا من الكود.</summary>
        public string Search(string query, ISet<string> allowedViews, Func<string, bool> hasRoute, int top = 6)
        {
            var rows = ActiveRows();
            if (rows == null || rows.Count == 0)
            {
                LastSearchUsedFallback = true;
                LastSearchMode = SearchModes.Fallback;
                return AiSchemaCatalog.Search(query, AiSchemaCatalog.Views.Where(v => allowedViews.Contains(v.Name)), top);
            }
            LastSearchUsedFallback = false;
            LastSearchMode = SearchModes.Keyword;
            var best = KeywordRank(query, rows.Where(r => Visible(r, allowedViews, hasRoute))).Take(top).Select(h => Format(h.Row)).ToList();
            return best.Count == 0 ? NoResults : string.Join("\n\n", best);
        }
    }
}
