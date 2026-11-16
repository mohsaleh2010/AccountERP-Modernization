using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace AccountERP.AI.Providers
{
    /// <summary>
    /// P8.11.1 (ai-assistant-p811-plan.md ص2): تفريغ الصوت لكل بروتوكول. OpenAI المتوافق (OpenAI، Groq، خوادم Whisper المحلية) {base}/audio/transcriptions multipart،
    /// Azure deployments/{name}/audio/transcriptions، Gemini generateContent بصوت inline مع تعليمة «فرّغ حرفياً». Anthropic وOllama غير مدعومين.
    /// </summary>
    public static class HttpTranscription
    {
        public static bool Supports(string protocol) => protocol is not (Models.Anthropic or "ollama");

        private static class Models { public const string Anthropic = AccountModelsLib.AiApiProtocols.Anthropic; }

        public static async Task<string> TranscribeAsync(ProviderConnection c, string model, byte[] audio, string mime, string language, string? prompt, TimeSpan timeout, CancellationToken ct)
        {
            switch (c.Protocol)
            {
                case AccountModelsLib.AiApiProtocols.Gemini:
                    return await GeminiAsync(c, model, audio, mime, language, prompt, timeout, ct);
                case "azure":
                {
                    string deployment = string.IsNullOrWhiteSpace(model) ? c.AzureDeployment ?? "" : model;
                    if (deployment.Length == 0) throw new AiProviderException("أدخل اسم الـ deployment لنموذج الصوت على Azure.");
                    return await OpenAiShapeAsync(c, "openai/deployments/" + Uri.EscapeDataString(deployment) + "/audio/transcriptions?api-version=" + Uri.EscapeDataString(c.AzureApiVersion),
                                                  null, audio, mime, language, prompt, "api-key", timeout, ct);
                }
                case AccountModelsLib.AiApiProtocols.Anthropic:
                case "ollama":
                    throw new AiProviderException("هذا المزوّد لا يدعم تفريغ الصوت. اختر نموذج Whisper أو gpt-4o-transcribe أو Gemini لمهمة «الصوت».");
                default:
                    return await OpenAiShapeAsync(c, "audio/transcriptions", model, audio, mime, language, prompt, null, timeout, ct);
            }
        }

        private static async Task<string> OpenAiShapeAsync(ProviderConnection c, string path, string? model, byte[] audio, string mime, string language, string? prompt,
                                                           string? auth, TimeSpan timeout, CancellationToken ct)
        {
            using var form = new MultipartFormDataContent();
            var file = new ByteArrayContent(audio);
            file.Headers.ContentType = new MediaTypeHeaderValue(BaseMime(mime));
            form.Add(file, "file", "speech" + Extension(mime));
            if (!string.IsNullOrWhiteSpace(model)) form.Add(new StringContent(model), "model");
            if (!string.IsNullOrWhiteSpace(language)) form.Add(new StringContent(language), "language");
            if (!string.IsNullOrWhiteSpace(prompt)) form.Add(new StringContent(prompt), "prompt");
            form.Add(new StringContent("json"), "response_format");
            using var doc = await AiHttp.PostContentAsync(c, path, form, auth, timeout, ct);
            if (doc.RootElement.TryGetProperty("text", out var t) && t.ValueKind == JsonValueKind.String) return t.GetString() ?? "";
            throw new AiProviderException("ردّ التفريغ بلا text؛ تأكد أن النموذج نموذج تفريغ صوت.");
        }

        private static async Task<string> GeminiAsync(ProviderConnection c, string model, byte[] audio, string mime, string language, string? prompt, TimeSpan timeout, CancellationToken ct)
        {
            string id = model.StartsWith("models/", StringComparison.Ordinal) ? model["models/".Length..] : model;
            string instruction = "Transcribe this audio verbatim in its spoken language (" + language + "). Output only the transcript, no comments." +
                                 (string.IsNullOrWhiteSpace(prompt) ? "" : " Vocabulary hints: " + prompt);
            var body = new JsonObject
            {
                ["contents"] = new JsonArray(new JsonObject
                {
                    ["role"] = "user",
                    ["parts"] = new JsonArray(
                        new JsonObject { ["text"] = instruction },
                        new JsonObject { ["inline_data"] = new JsonObject { ["mime_type"] = BaseMime(mime), ["data"] = Convert.ToBase64String(audio) } }),
                }),
                ["generationConfig"] = new JsonObject { ["temperature"] = 0 },
            };
            using var doc = await AiHttp.PostJsonAsync(c, "models/" + Uri.EscapeDataString(id) + ":generateContent", body.ToJsonString(), "x-goog-api-key", timeout, ct);
            var sb = new System.Text.StringBuilder();
            if (doc.RootElement.TryGetProperty("candidates", out var cands) && cands.ValueKind == JsonValueKind.Array)
                foreach (var cand in cands.EnumerateArray())
                {
                    if (!cand.TryGetProperty("content", out var content) || !content.TryGetProperty("parts", out var parts)) continue;
                    foreach (var p in parts.EnumerateArray())
                        if (p.TryGetProperty("text", out var tx)) sb.Append(tx.GetString());
                    break;
                }
            return sb.ToString().Trim();
        }

        public static string BaseMime(string? mime)
        {
            string m = (mime ?? "").Split(';')[0].Trim().ToLowerInvariant();
            return m.Length == 0 ? "audio/webm" : m;
        }

        private static string Extension(string? mime) => BaseMime(mime) switch
        {
            "audio/webm" => ".webm",
            "audio/ogg" => ".ogg",
            "audio/mp4" or "audio/m4a" or "audio/x-m4a" => ".m4a",
            "audio/mpeg" or "audio/mp3" => ".mp3",
            "audio/wav" or "audio/x-wav" or "audio/wave" => ".wav",
            _ => ".webm",
        };

        /// <summary>الصيغ المقبولة من المتصفح (ص3).</summary>
        public static bool AllowedMime(string? mime) => BaseMime(mime) is "audio/webm" or "audio/ogg" or "audio/mp4" or "audio/m4a" or "audio/x-m4a" or "audio/mpeg" or "audio/wav" or "audio/x-wav";
    }
}
