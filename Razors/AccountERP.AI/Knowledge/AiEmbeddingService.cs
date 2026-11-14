using AccountERP.AI.Providers;
using AccountModelsLib;

namespace AccountERP.AI.Knowledge
{
    /// <summary>
    /// P7.2 (ai-assistant-p7-plan.md D3، D4، D9): نموذج التضمين من ربط مهمة «تضمين» (أقل Priority، مفعّل، ويحترم «محلى فقط»).
    /// سؤال البحث يُضمَّن تلقائياً (كاش LRU 200، مهلة 3 ثوانٍ)، وأى خطأ يوقف البحث الدلالى 5 دقائق (قاطع) فيرجع البحث للكلمات ولا تفشل search_schema.
    /// المقاطع لا تُضمَّن إلا بطلب المدير («بناء الفهرس») على دفعات 64. لا شىء عند بدء التشغيل.
    /// </summary>
    public sealed class AiEmbeddingService
    {
        public static readonly TimeSpan QueryTimeout = TimeSpan.FromSeconds(3);
        public static readonly TimeSpan BreakerTime = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan RouteTtl = TimeSpan.FromSeconds(60);
        public const int BatchSize = 64;
        private const int LruSize = 200;

        private readonly ProviderRegistry _registry;
        private readonly AiProvidersCLS _providers = new();
        private readonly object _gate = new();
        private Route? _route;
        private bool _routeLoaded;
        private DateTime _routeAt;
        private DateTime _pausedUntil = DateTime.MinValue;
        private readonly LinkedList<(string Key, float[] Vec)> _lru = new();
        private readonly Dictionary<string, LinkedListNode<(string Key, float[] Vec)>> _lruMap = new();

        public AiEmbeddingService(ProviderRegistry registry) => _registry = registry;

        /// <summary>النموذج المربوط وصف مزوّده.</summary>
        public sealed record Route(AiModelRow Model, AiProviderRow Provider);

        public string? LastError { get; private set; }

        public DateTime? PausedUntilUtc { get { lock (_gate) return _pausedUntil > DateTime.UtcNow ? _pausedUntil : null; } }

        /// <summary>يُستدعى بعد حفظ الربط/النماذج حتى لا ينتظر دقيقة.</summary>
        public void InvalidateRoute()
        {
            lock (_gate) { _routeLoaded = false; _route = null; }
        }

        /// <summary>يعيد فتح البحث الدلالى فوراً (بعد إصلاح المزوّد أو فى الاختبار).</summary>
        public void ResetBreaker()
        {
            lock (_gate) { _pausedUntil = DateTime.MinValue; LastError = null; }
        }

        public void ClearQueryCache()
        {
            lock (_gate) { _lru.Clear(); _lruMap.Clear(); }
        }

        /// <summary>نموذج التضمين الحالى أو null (لا ربط / معطّل / «محلى فقط» والمزوّد سحابى). reason للعرض.</summary>
        public Route? CurrentRoute(out string reason)
        {
            lock (_gate)
            {
                if (_routeLoaded && DateTime.UtcNow - _routeAt < RouteTtl)
                {
                    reason = _route == null ? _routeReason : "";
                    return _route;
                }
            }
            var r = LoadRoute(out reason);
            lock (_gate) { _route = r; _routeReason = reason; _routeLoaded = true; _routeAt = DateTime.UtcNow; }
            return r;
        }

        private string _routeReason = "";

        private Route? LoadRoute(out string reason)
        {
            try
            {
                var routing = _providers.GetRouting().Where(r => r.TaskKey == AiTaskKeys.Embedding).OrderBy(r => r.Priority).ToList();
                if (routing.Count == 0)
                {
                    reason = "لا يوجد نموذج لمهمة «تضمين»؛ البحث بالكلمات فقط.";
                    return null;
                }
                var models = _providers.GetModels().ToDictionary(m => m.Id);
                var providers = _providers.GetProviders().ToDictionary(p => p.Id);
                var settings = _providers.GetSettings();
                bool localOnly = settings.TryGetValue(AiSettingKeys.LocalOnly, out var lo) && lo == "1";
                foreach (var r in routing)
                {
                    if (!models.TryGetValue(r.Model_Id, out var m) || !m.IsEnabled) continue;
                    if (!providers.TryGetValue(m.Provider_Id, out var p) || !p.IsEnabled) continue;
                    if (localOnly && !p.IsLocal) continue;
                    reason = "";
                    return new Route(m, p);
                }
                reason = localOnly
                    ? "وضع «محلى فقط» مفعّل ولا يوجد نموذج تضمين محلى مفعّل؛ البحث بالكلمات فقط."
                    : "نموذج «تضمين» المختار أو مزوّده معطّل؛ البحث بالكلمات فقط.";
                return null;
            }
            catch (Exception ee)
            {
                operations.DataDictionaryLogerException(ee);
                reason = "تعذّرت قراءة إعدادات التضمين.";
                return null;
            }
        }

        /// <summary>
        /// متجه سؤال البحث (مطبّع) أو null = ارجع للكلمات: لا ربط، القاطع مفتوح، مهلة 3 ثوانٍ، أو خطأ مزوّد (يفتح القاطع 5 دقائق).
        /// لا يرمى إلا OperationCanceledException عند إلغاء المستخدم.
        /// </summary>
        public async Task<(float[]? Vector, long ModelId)> EmbedQueryAsync(string text, CancellationToken ct)
        {
            var route = CurrentRoute(out _);
            if (route == null) return (null, 0);
            lock (_gate) if (_pausedUntil > DateTime.UtcNow) return (null, route.Model.Id);

            string key = route.Model.Id + "\n" + text;
            lock (_gate)
                if (_lruMap.TryGetValue(key, out var node))
                {
                    _lru.Remove(node);
                    _lru.AddFirst(node);
                    return (node.Value.Vec, route.Model.Id);
                }
            try
            {
                var conn = _registry.Connect(route.Provider);
                var adapter = _registry.AdapterFor(conn.Protocol);
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                cts.CancelAfter(QueryTimeout + TimeSpan.FromMilliseconds(250));   // حارس فوق مهلة HTTP نفسها
                var vecs = await adapter.EmbedAsync(conn, route.Model.ModelId, new[] { text }, true, QueryTimeout, cts.Token);
                var v = AiVectorMath.Normalize(vecs[0]);
                lock (_gate)
                {
                    var node = _lru.AddFirst((key, v));
                    _lruMap[key] = node;
                    while (_lru.Count > LruSize)
                    {
                        _lruMap.Remove(_lru.Last!.Value.Key);
                        _lru.RemoveLast();
                    }
                }
                return (v, route.Model.Id);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
            catch (Exception ee)
            {
                Trip(ee is OperationCanceledException ? "انتهت مهلة التضمين (3 ثوانٍ)." : ee.Message);
                return (null, route.Model.Id);
            }
        }

        private void Trip(string message)
        {
            lock (_gate)
            {
                _pausedUntil = DateTime.UtcNow + BreakerTime;
                LastError = message;
            }
        }

        /// <summary>متجهات المقاطع (مطبّعة) على دفعات 64 بمهلة المزوّد. يرمى AiProviderException/InvalidOperationException برسالة عربية.</summary>
        public async Task<List<float[]>> EmbedDocumentsAsync(Route route, IReadOnlyList<string> texts, CancellationToken ct)
        {
            var conn = _registry.Connect(route.Provider);
            var adapter = _registry.AdapterFor(conn.Protocol);
            var all = new List<float[]>(texts.Count);
            for (int i = 0; i < texts.Count; i += BatchSize)
            {
                var batch = texts.Skip(i).Take(BatchSize).ToList();
                var vecs = await adapter.EmbedAsync(conn, route.Model.ModelId, batch, false, conn.Timeout, ct);
                if (vecs.Count != batch.Count) throw new AiProviderException("عدد متجهات التضمين لا يساوى عدد المقاطع.");
                all.AddRange(vecs.Select(AiVectorMath.Normalize));
            }
            return all;
        }
    }

    /// <summary>P7.2: حساب المتجهات (D1، D2): تطبيع L2، تحويل float32 LE، وجيب التمام = حاصل الضرب لأن الكل مطبّع.</summary>
    public static class AiVectorMath
    {
        public static float[] Normalize(float[] v)
        {
            double sum = 0;
            foreach (var x in v) sum += (double)x * x;
            if (sum <= 0) return (float[])v.Clone();
            float inv = (float)(1.0 / Math.Sqrt(sum));
            var r = new float[v.Length];
            for (int i = 0; i < v.Length; i++) r[i] = v[i] * inv;
            return r;
        }

        public static byte[] ToBytes(float[] v)
        {
            var b = new byte[v.Length * 4];
            for (int i = 0; i < v.Length; i++)
                System.Buffers.Binary.BinaryPrimitives.WriteSingleLittleEndian(b.AsSpan(i * 4), v[i]);
            return b;
        }

        public static float[] FromBytes(byte[] b)
        {
            var v = new float[b.Length / 4];
            for (int i = 0; i < v.Length; i++)
                v[i] = System.Buffers.Binary.BinaryPrimitives.ReadSingleLittleEndian(b.AsSpan(i * 4));
            return v;
        }

        public static float Dot(float[] a, float[] b)
        {
            int n = Math.Min(a.Length, b.Length);
            float s = 0;
            for (int i = 0; i < n; i++) s += a[i] * b[i];
            return s;
        }
    }
}
