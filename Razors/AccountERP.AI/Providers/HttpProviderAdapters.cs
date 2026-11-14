using System.Text.Json;
using AccountModelsLib;

namespace AccountERP.AI.Providers
{
    /// <summary>OpenAI وLM Studio وأى خدمة متوافقة مع OpenAI (OpenRouter / LiteLLM / Groq / DeepSeek / Mistral / vLLM): GET {base}/models.</summary>
    internal sealed class OpenAiCompatibleAdapter : IAiProviderAdapter
    {
        public Task<Chat.AiChatOutcome> StreamChatAsync(ProviderConnection c, Chat.AiChatCall call, Action<string> onDelta, CancellationToken ct) =>
            HttpChatStreaming.OpenAiCompatibleAsync(c, call, onDelta, ct);

        public Task<List<float[]>> EmbedAsync(ProviderConnection c, string model, IReadOnlyList<string> texts, bool isQuery, TimeSpan timeout, CancellationToken ct) =>
            HttpEmbeddings.OpenAiCompatibleAsync(c, model, texts, timeout, ct);

        public async Task<List<AiRemoteModel>> ListModelsAsync(ProviderConnection c, CancellationToken ct)
        {
            using var doc = await AiHttp.GetJsonAsync(c, "models", null, ct);
            var list = new List<AiRemoteModel>();
            var root = doc.RootElement;
            var data = root.ValueKind == JsonValueKind.Array ? root : root.TryGetProperty("data", out var d) ? d : default;
            if (data.ValueKind != JsonValueKind.Array)
                throw new AiProviderException("ردّت الخدمة بدون قائمة نماذج (data). إن كانت لا تدعم /models أدخل معرّف النموذج يدوياً.");
            foreach (var m in data.EnumerateArray())
            {
                string id = m.TryGetProperty("id", out var i) ? i.GetString() ?? "" : "";
                if (id.Length == 0) continue;
                int? ctx = null;
                foreach (var key in new[] { "context_length", "context_window", "max_context_length" })
                    if (m.TryGetProperty(key, out var cl) && cl.TryGetInt32(out int v)) { ctx = v; break; }
                list.Add(new AiRemoteModel
                {
                    ModelId = id,
                    DisplayName = m.TryGetProperty("name", out var n) && n.ValueKind == JsonValueKind.String ? n.GetString() ?? id : id,
                    ContextTokens = ctx,
                    SupportsEmbeddings = id.Contains("embed", StringComparison.OrdinalIgnoreCase) ? true : null,
                });
            }
            return list;
        }
    }

    /// <summary>Ollama: GET {base}/api/tags (العنوان الافتراضى http://localhost:11434).</summary>
    internal sealed class OllamaAdapter : IAiProviderAdapter
    {
        public Task<Chat.AiChatOutcome> StreamChatAsync(ProviderConnection c, Chat.AiChatCall call, Action<string> onDelta, CancellationToken ct) =>
            HttpChatStreaming.OllamaAsync(c, call, onDelta, ct);

        public Task<List<float[]>> EmbedAsync(ProviderConnection c, string model, IReadOnlyList<string> texts, bool isQuery, TimeSpan timeout, CancellationToken ct) =>
            HttpEmbeddings.OllamaAsync(c, model, texts, timeout, ct);

        public async Task<List<AiRemoteModel>> ListModelsAsync(ProviderConnection c, CancellationToken ct)
        {
            using var doc = await AiHttp.GetJsonAsync(c, "api/tags", null, ct);
            var list = new List<AiRemoteModel>();
            if (!doc.RootElement.TryGetProperty("models", out var models) || models.ValueKind != JsonValueKind.Array)
                throw new AiProviderException("ردّ غير متوقع من Ollama. تأكد أن العنوان هو عنوان Ollama نفسه (مثل http://localhost:11434) بدون /v1.");
            foreach (var m in models.EnumerateArray())
            {
                string id = m.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "";
                if (id.Length == 0) continue;
                list.Add(new AiRemoteModel
                {
                    ModelId = id,
                    DisplayName = id,
                    SupportsEmbeddings = id.Contains("embed", StringComparison.OrdinalIgnoreCase) ? true : null,
                });
            }
            return list;
        }
    }

    /// <summary>Google Gemini: GET {base}/models بـ x-goog-api-key (العنوان الافتراضى https://generativelanguage.googleapis.com/v1beta).</summary>
    internal sealed class GeminiAdapter : IAiProviderAdapter
    {
        public Task<Chat.AiChatOutcome> StreamChatAsync(ProviderConnection c, Chat.AiChatCall call, Action<string> onDelta, CancellationToken ct) =>
            HttpChatStreaming.GeminiAsync(c, call, onDelta, ct);

        public Task<List<float[]>> EmbedAsync(ProviderConnection c, string model, IReadOnlyList<string> texts, bool isQuery, TimeSpan timeout, CancellationToken ct) =>
            HttpEmbeddings.GeminiAsync(c, model, texts, isQuery, timeout, ct);

        public async Task<List<AiRemoteModel>> ListModelsAsync(ProviderConnection c, CancellationToken ct)
        {
            using var doc = await AiHttp.GetJsonAsync(c, "models?pageSize=1000", "x-goog-api-key", ct);
            var list = new List<AiRemoteModel>();
            if (!doc.RootElement.TryGetProperty("models", out var models) || models.ValueKind != JsonValueKind.Array)
                throw new AiProviderException("ردّ غير متوقع من Gemini (لا توجد قائمة models).");
            foreach (var m in models.EnumerateArray())
            {
                string name = m.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "";
                if (name.Length == 0) continue;
                string id = name.StartsWith("models/", StringComparison.Ordinal) ? name["models/".Length..] : name;
                var methods = m.TryGetProperty("supportedGenerationMethods", out var sm) && sm.ValueKind == JsonValueKind.Array
                    ? sm.EnumerateArray().Select(x => x.GetString() ?? "").ToList() : new List<string>();
                list.Add(new AiRemoteModel
                {
                    ModelId = id,
                    DisplayName = m.TryGetProperty("displayName", out var dn) ? dn.GetString() ?? id : id,
                    ContextTokens = m.TryGetProperty("inputTokenLimit", out var lim) && lim.TryGetInt32(out int v) ? v : null,
                    SupportsEmbeddings = methods.Contains("embedContent") ? true : null,
                    SupportsVision = methods.Contains("generateContent") ? true : null,
                });
            }
            return list;
        }
    }

    /// <summary>
    /// Azure OpenAI: التحقق من المفتاح بـ GET {endpoint}/openai/models?api-version=… بالـ Header api-key.
    /// النموذج المستخدم فعلياً على Azure هو اسم الـ deployment، فإن كان مُدخلاً يُعاد وحده كنموذج قابل للإضافة.
    /// </summary>
    internal sealed class AzureOpenAiAdapter : IAiProviderAdapter
    {
        public Task<Chat.AiChatOutcome> StreamChatAsync(ProviderConnection c, Chat.AiChatCall call, Action<string> onDelta, CancellationToken ct) =>
            HttpChatStreaming.AzureAsync(c, call, onDelta, ct);

        public Task<List<float[]>> EmbedAsync(ProviderConnection c, string model, IReadOnlyList<string> texts, bool isQuery, TimeSpan timeout, CancellationToken ct) =>
            HttpEmbeddings.AzureAsync(c, model, texts, timeout, ct);

        public async Task<List<AiRemoteModel>> ListModelsAsync(ProviderConnection c, CancellationToken ct)
        {
            using var doc = await AiHttp.GetJsonAsync(c, "openai/models?api-version=" + Uri.EscapeDataString(c.AzureApiVersion), "api-key", ct);
            if (!string.IsNullOrWhiteSpace(c.AzureDeployment))
                return new List<AiRemoteModel> { new() { ModelId = c.AzureDeployment.Trim(), DisplayName = c.AzureDeployment.Trim() + " (deployment)" } };
            var list = new List<AiRemoteModel>();
            if (doc.RootElement.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array)
                foreach (var m in data.EnumerateArray())
                {
                    string id = m.TryGetProperty("id", out var i) ? i.GetString() ?? "" : "";
                    if (id.Length > 0) list.Add(new AiRemoteModel { ModelId = id, DisplayName = id });
                }
            return list;
        }
    }
}
