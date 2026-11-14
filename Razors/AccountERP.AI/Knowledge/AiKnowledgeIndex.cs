using AccountModelsLib;

namespace AccountERP.AI.Knowledge
{
    /// <summary>
    /// P7.2 (ai-assistant-p7-plan.md D2، D8): متجهات AI_Knowledge فى الذاكرة لنموذج التضمين الحالى (كاش 5 دقائق، يُلغى عند أى كتابة).
    /// المقارنة بجيب التمام على المقاطع المرئية للمستخدم فقط، والدمج مع ترتيب الكلمات بـ RRF (k=60، أفضل 20 من كلٍّ) → أفضل 6.
    /// صف بمتجه نموذج آخر أو بطول مختلف أو بمحتوى تغيّر بعد التضمين لا يدخل (قديم).
    /// </summary>
    public sealed class AiKnowledgeIndex
    {
        public const int RrfK = 60;
        public const int Depth = 20;
        private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

        private readonly AiKnowledgeCLS _cls;
        private readonly object _gate = new();
        private long _modelId;
        private Dictionary<long, (string? Hash, float[] Vec)>? _vectors;
        private DateTime _loadedAt;

        public AiKnowledgeIndex(AiKnowledgeCLS? cls = null) => _cls = cls ?? new AiKnowledgeCLS();

        public void Invalidate()
        {
            lock (_gate) _vectors = null;
        }

        /// <summary>متجهات نموذج معيّن (من الكاش أو القاعدة). يرمى عند فشل القراءة.</summary>
        public Dictionary<long, (string? Hash, float[] Vec)> VectorsFor(long modelId)
        {
            lock (_gate)
                if (_vectors != null && _modelId == modelId && DateTime.UtcNow - _loadedAt < CacheTtl) return _vectors;
            var map = new Dictionary<long, (string?, float[])>();
            foreach (var v in _cls.ListVectors(modelId))
            {
                if (v.Data == null || v.Data.Length < 4 || v.Data.Length % 4 != 0) continue;
                var vec = AiVectorMath.FromBytes(v.Data);
                if (v.Dim > 0 && vec.Length != v.Dim) continue;
                map[v.Id] = (v.ContentHash, vec);
            }
            lock (_gate) { _vectors = map; _modelId = modelId; _loadedAt = DateTime.UtcNow; }
            return map;
        }

        /// <summary>ترتيب المقاطع المرئية بجيب التمام (الأفضل أولاً، حتى Depth). المقاطع بلا متجه صالح تُتجاهل.</summary>
        public static List<(AiKnowledgeRow Row, float Score)> VectorRank(float[] query, IEnumerable<AiKnowledgeRow> visible,
            IReadOnlyDictionary<long, (string? Hash, float[] Vec)> vectors, int depth = Depth)
        {
            var hits = new List<(AiKnowledgeRow, float)>();
            foreach (var r in visible)
            {
                if (!vectors.TryGetValue(r.Id, out var v)) continue;
                if (v.Vec.Length != query.Length) continue;
                if (!string.Equals(v.Hash ?? "", r.ContentHash ?? "", StringComparison.OrdinalIgnoreCase)) continue;
                hits.Add((r, AiVectorMath.Dot(query, v.Vec)));
            }
            return hits.OrderByDescending(h => h.Item2).ThenBy(h => h.Item1.Id).Take(depth).ToList();
        }

        /// <summary>
        /// دمج RRF: درجة = Σ 1/(k + الترتيب) فى القائمتين. التعادل: ترتيب الكلمات أولاً ثم ترتيب المتجهات.
        /// قائمة متجهات فارغة = نفس ترتيب الكلمات حرفياً.
        /// </summary>
        public static List<AiKnowledgeRow> Fuse(IReadOnlyList<AiKnowledgeRow> keyword, IReadOnlyList<AiKnowledgeRow> vector, int top, int k = RrfK)
        {
            var score = new Dictionary<long, double>();
            var kwRank = new Dictionary<long, int>();
            var vecRank = new Dictionary<long, int>();
            var rows = new Dictionary<long, AiKnowledgeRow>();
            for (int i = 0; i < keyword.Count; i++)
            {
                var r = keyword[i];
                if (kwRank.ContainsKey(r.Id)) continue;
                kwRank[r.Id] = i;
                rows[r.Id] = r;
                score[r.Id] = 1.0 / (k + i + 1);
            }
            for (int i = 0; i < vector.Count; i++)
            {
                var r = vector[i];
                if (vecRank.ContainsKey(r.Id)) continue;
                vecRank[r.Id] = i;
                rows[r.Id] = r;
                score[r.Id] = score.GetValueOrDefault(r.Id) + 1.0 / (k + i + 1);
            }
            return score.Keys
                .OrderByDescending(id => score[id])
                .ThenBy(id => kwRank.TryGetValue(id, out int a) ? a : int.MaxValue)
                .ThenBy(id => vecRank.TryGetValue(id, out int b) ? b : int.MaxValue)
                .Take(top).Select(id => rows[id]).ToList();
        }

        /// <summary>النص الذى يُضمَّن لكل مقطع (العنوان والمحتوى والكلمات المفتاحية).</summary>
        public static string EmbedText(AiKnowledgeRow r) =>
            (r.Kind == AiKnowledgeKinds.View ? "ai." + r.KeyName + " — " : "") + r.TitleAr + "\n" + r.Content + (string.IsNullOrWhiteSpace(r.Keywords) ? "" : "\n" + r.Keywords);
    }
}
