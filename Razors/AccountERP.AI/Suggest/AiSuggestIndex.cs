using AccountERP.AI.Knowledge;
using AccountModelsLib;

namespace AccountERP.AI.Suggest
{
    /// <summary>
    /// P8.8.2 (ai-assistant-p88-plan.md ص7، ص8): فهرس السوابق فى الذاكرة لكل نوع (expense / item)، يُبنى عند أول طلب ويُحدَّث كل 6 ساعات،
    /// ومتجهات النصوص المميزة (إن وُجد نموذج تضمين) محمّلة من AI_Suggestions_Vectors. لا نصوص سوابق فى جداول جديدة.
    /// </summary>
    public sealed class AiSuggestIndex
    {
        public static readonly TimeSpan RefreshEvery = TimeSpan.FromHours(6);
        public const int MaxVectorHits = 60;

        private readonly AiSuggestCLS _dal;
        private readonly object _gate = new();
        private readonly Dictionary<string, AiSuggestCorpus> _corpora = new(StringComparer.Ordinal);
        private readonly Dictionary<string, object> _buildLocks = new(StringComparer.Ordinal) { [AiSuggestKinds.Expense] = new object(), [AiSuggestKinds.Item] = new object() };
        // المتجهات: kind → (modelId, norm → vector)
        private readonly Dictionary<string, (long ModelId, Dictionary<string, float[]> Vectors, DateTime LoadedAt)> _vectors = new(StringComparer.Ordinal);

        /// <summary>اختبار: محمّل سوابق بديل.</summary>
        public Func<string, AiSuggestSettings, List<AiSuggestPrecedent>>? Loader { get; set; }

        public AiSuggestIndex(AiSuggestCLS? dal = null) => _dal = dal ?? new AiSuggestCLS();

        public AiSuggestCLS Dal => _dal;

        public AiSuggestCorpus Get(string kind, AiSuggestSettings s)
        {
            lock (_gate)
                if (_corpora.TryGetValue(kind, out var c) && DateTime.UtcNow - c.BuiltAt < RefreshEvery) return c;
            if (!_buildLocks.TryGetValue(kind, out var bl)) throw new ArgumentException("unknown kind", nameof(kind));
            lock (bl)
            {
                lock (_gate)
                    if (_corpora.TryGetValue(kind, out var c2) && DateTime.UtcNow - c2.BuiltAt < RefreshEvery) return c2;
                AiSuggestCorpus built;
                try
                {
                    var rows = Loader != null ? Loader(kind, s)
                             : kind == AiSuggestKinds.Expense ? _dal.LoadExpenseHistory(s.HistoryMonths, s.MaxRows)
                             : _dal.LoadItemClasses(s.MaxRows);
                    built = AiSuggestCorpus.Build(rows);
                }
                catch (Exception ex)
                {
                    try { operations.DataDictionaryLogerException(ex); } catch { }
                    // فشل التحميل: فهرس فارغ يُعاد بناؤه بعد 10 دقائق لا 6 ساعات
                    built = AiSuggestCorpus.Build(null);
                    built.Error = ex.Message;
                    built.BuiltAt = DateTime.UtcNow - RefreshEvery + TimeSpan.FromMinutes(10);
                }
                lock (_gate) _corpora[kind] = built;
                return built;
            }
        }

        public void Invalidate()
        {
            lock (_gate)
            {
                _corpora.Clear();
                _vectors.Clear();
            }
        }

        public void InvalidateVectors()
        {
            lock (_gate) _vectors.Clear();
        }

        /// <summary>متجهات النصوص المميزة لنموذج التضمين الحالى (تُحمَّل مرة وتُحدَّث كل 6 ساعات أو عند تغيير النموذج).</summary>
        public Dictionary<string, float[]> Vectors(string kind, long modelId)
        {
            lock (_gate)
                if (_vectors.TryGetValue(kind, out var v) && v.ModelId == modelId && DateTime.UtcNow - v.LoadedAt < RefreshEvery) return v.Vectors;
            var map = new Dictionary<string, float[]>(StringComparer.Ordinal);
            try
            {
                foreach (var kv in _dal.LoadVectors(kind, modelId))
                    map[kv.Value.Key] = AiVectorMath.FromBytes(kv.Value.Value);
            }
            catch (Exception ex) { try { operations.DataDictionaryLogerException(ex); } catch { } }
            lock (_gate) _vectors[kind] = (modelId, map, DateTime.UtcNow);
            return map;
        }

        /// <summary>تشابه الاستعلام مع النصوص المميزة (أعلى MaxVectorHits).</summary>
        public static Dictionary<string, double> Similar(float[] query, Dictionary<string, float[]> vectors)
        {
            var result = new Dictionary<string, double>(StringComparer.Ordinal);
            if (query == null || vectors == null || vectors.Count == 0) return result;
            var top = new List<(string Text, double Sim)>();
            foreach (var kv in vectors)
            {
                if (kv.Value.Length != query.Length) continue;
                double sim = AiVectorMath.Dot(query, kv.Value);
                if (sim >= AiSuggestRanker.MinVectorSim) top.Add((kv.Key, sim));
            }
            foreach (var t in top.OrderByDescending(x => x.Sim).Take(MaxVectorHits)) result[t.Text] = t.Sim;
            return result;
        }

        public AiSuggestIndexStatus Status(string kind)
        {
            AiSuggestCorpus? c;
            lock (_gate) _corpora.TryGetValue(kind, out c);
            int vec = 0;
            try { vec = _dal.CountVectors(kind); } catch { }
            return new AiSuggestIndexStatus
            {
                Kind = kind, Rows = c?.Items.Count ?? 0, DistinctTexts = c?.DistinctTexts ?? 0, Vectors = vec,
                BuiltAt = c?.BuiltAt.ToLocalTime(), Error = c?.Error,
            };
        }
    }
}
