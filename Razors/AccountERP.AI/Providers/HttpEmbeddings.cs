using System.Text.Json;
using System.Text.Json.Nodes;

namespace AccountERP.AI.Providers
{
    /// <summary>
    /// P7.2: طلبات التضمين لكل بروتوكول (ai-assistant-p7-plan.md D3). كلها POST JSON بلا بث:
    /// OpenAI المتوافق {base}/embeddings، Azure deployments/{name}/embeddings، Gemini models/{m}:batchEmbedContents (taskType)، Ollama api/embed.
    /// </summary>
    internal static class HttpEmbeddings
    {
        public static async Task<List<float[]>> OpenAiCompatibleAsync(ProviderConnection c, string model, IReadOnlyList<string> texts, TimeSpan timeout, CancellationToken ct) =>
            await OpenAiShapeAsync(c, "embeddings", new JsonObject { ["model"] = model, ["input"] = Texts(texts) }, null, texts.Count, timeout, ct);

        public static async Task<List<float[]>> AzureAsync(ProviderConnection c, string model, IReadOnlyList<string> texts, TimeSpan timeout, CancellationToken ct)
        {
            // على Azure اسم النموذج المسجّل هو اسم الـ deployment (نموذج التضمين غالباً deployment مختلف عن المحادثة).
            string deployment = string.IsNullOrWhiteSpace(model) ? c.AzureDeployment ?? "" : model;
            if (deployment.Length == 0) throw new AiProviderException("أدخل اسم الـ deployment لنموذج التضمين على Azure.");
            string path = "openai/deployments/" + Uri.EscapeDataString(deployment) + "/embeddings?api-version=" + Uri.EscapeDataString(c.AzureApiVersion);
            return await OpenAiShapeAsync(c, path, new JsonObject { ["input"] = Texts(texts) }, "api-key", texts.Count, timeout, ct);
        }

        private static async Task<List<float[]>> OpenAiShapeAsync(ProviderConnection c, string path, JsonObject body, string? auth, int count, TimeSpan timeout, CancellationToken ct)
        {
            using var doc = await AiHttp.PostJsonAsync(c, path, body.ToJsonString(), auth, timeout, ct);
            if (!doc.RootElement.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array)
                throw new AiProviderException("ردّ التضمين بلا قائمة data؛ تأكد أن النموذج نموذج تضمين (embedding).");
            var result = new float[count][];
            int pos = 0;
            foreach (var item in data.EnumerateArray())
            {
                int idx = item.TryGetProperty("index", out var ix) && ix.TryGetInt32(out int i) ? i : pos;
                pos++;
                if (idx < 0 || idx >= count || !item.TryGetProperty("embedding", out var emb)) continue;
                result[idx] = Vector(emb);
            }
            return Complete(result);
        }

        public static async Task<List<float[]>> GeminiAsync(ProviderConnection c, string model, IReadOnlyList<string> texts, bool isQuery, TimeSpan timeout, CancellationToken ct)
        {
            string id = model.StartsWith("models/", StringComparison.Ordinal) ? model["models/".Length..] : model;
            var requests = new JsonArray();
            foreach (var t in texts)
                requests.Add(new JsonObject
                {
                    ["model"] = "models/" + id,
                    ["content"] = new JsonObject { ["parts"] = new JsonArray(new JsonObject { ["text"] = t }) },
                    ["taskType"] = isQuery ? "RETRIEVAL_QUERY" : "RETRIEVAL_DOCUMENT",
                });
            using var doc = await AiHttp.PostJsonAsync(c, "models/" + Uri.EscapeDataString(id) + ":batchEmbedContents",
                new JsonObject { ["requests"] = requests }.ToJsonString(), "x-goog-api-key", timeout, ct);
            if (!doc.RootElement.TryGetProperty("embeddings", out var embs) || embs.ValueKind != JsonValueKind.Array)
                throw new AiProviderException("ردّ Gemini بلا embeddings؛ تأكد أن النموذج يدعم embedContent.");
            var list = embs.EnumerateArray().Select(e => e.TryGetProperty("values", out var v) ? Vector(v) : null).ToArray();
            return Complete(list, texts.Count);
        }

        public static async Task<List<float[]>> OllamaAsync(ProviderConnection c, string model, IReadOnlyList<string> texts, TimeSpan timeout, CancellationToken ct)
        {
            using var doc = await AiHttp.PostJsonAsync(c, "api/embed", new JsonObject { ["model"] = model, ["input"] = Texts(texts) }.ToJsonString(), null, timeout, ct);
            if (!doc.RootElement.TryGetProperty("embeddings", out var embs) || embs.ValueKind != JsonValueKind.Array)
                throw new AiProviderException("ردّ Ollama بلا embeddings؛ تأكد أن النموذج نموذج تضمين (مثل nomic-embed-text) وأن إصدار Ollama يدعم /api/embed.");
            return Complete(embs.EnumerateArray().Select(Vector).ToArray(), texts.Count);
        }

        private static JsonArray Texts(IReadOnlyList<string> texts) => new(texts.Select(t => (JsonNode)JsonValue.Create(t)!).ToArray());

        private static float[]? Vector(JsonElement e)
        {
            if (e.ValueKind != JsonValueKind.Array) return null;
            var v = new float[e.GetArrayLength()];
            int i = 0;
            foreach (var x in e.EnumerateArray()) v[i++] = x.GetSingle();
            return v;
        }

        private static List<float[]> Complete(float[]?[] list, int? expected = null)
        {
            if (expected.HasValue && list.Length != expected.Value)
                throw new AiProviderException($"ردّ التضمين بعدد متجهات ({list.Length}) لا يساوى عدد النصوص ({expected.Value}).");
            if (list.Any(v => v == null || v.Length == 0))
                throw new AiProviderException("ردّ التضمين ناقص (متجه فارغ لبعض النصوص).");
            int dim = list[0]!.Length;
            if (list.Any(v => v!.Length != dim)) throw new AiProviderException("ردّ التضمين بأطوال متجهات مختلفة.");
            return list.Select(v => v!).ToList();
        }
    }
}
