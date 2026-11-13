using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using AccountERP.AI.Chat;
using AccountModelsLib;

namespace AccountERP.AI.Providers
{
    /// <summary>
    /// P1: بثّ المحادثة للمزوّدين عبر HTTP (OpenAI-compatible / Azure / Gemini / Ollama). كل مزوّد يحوَّل لنفس AiChatOutcome،
    /// والأخطاء رسائل عربية بلا مفاتيح. Claude عبر حزمة Anthropic الرسمية فى AnthropicAdapter.
    /// </summary>
    internal static class HttpChatStreaming
    {
        private static Uri Url(ProviderConnection c, string relative) => new(c.BaseUrl.AbsoluteUri.TrimEnd('/') + "/" + relative.TrimStart('/'));

        // ───────────── OpenAI Chat Completions (OpenAI / OpenRouter / LM Studio / مخصص / Azure) ─────────────

        public static async Task<AiChatOutcome> OpenAiAsync(ProviderConnection c, Uri url, AiChatCall call, string? defaultAuthHeader,
                                                             bool useMaxCompletionTokens, Action<string> onDelta, CancellationToken ct)
        {
            var messages = new JsonArray();
            if (!string.IsNullOrWhiteSpace(call.System)) messages.Add(new JsonObject { ["role"] = "system", ["content"] = call.System });
            foreach (var m in call.Messages) messages.Add(new JsonObject { ["role"] = m.Role, ["content"] = m.Text });
            var body = new JsonObject
            {
                ["model"] = call.Model,
                ["messages"] = messages,
                ["stream"] = true,
                ["stream_options"] = new JsonObject { ["include_usage"] = true },
                [useMaxCompletionTokens ? "max_completion_tokens" : "max_tokens"] = call.MaxTokens,
            };
            if (call.Temperature.HasValue) body["temperature"] = (double)call.Temperature.Value;

            var outcome = new AiChatOutcome();
            var text = new StringBuilder();
            using var resp = await AiHttp.PostStreamAsync(c, url, body.ToJsonString(), defaultAuthHeader, ct);
            await foreach (var line in AiHttp.ReadLinesAsync(resp, ct))
            {
                string? data = AiHttp.SseData(line);
                if (data == null) continue;
                if (data == "[DONE]") break;
                using var doc = Parse(data);
                if (doc == null) continue;
                var root = doc.RootElement;
                if (AiHttp.StreamError(root, c) is { } err) throw new AiProviderException(err);
                if (root.TryGetProperty("choices", out var choices) && choices.ValueKind == JsonValueKind.Array)
                    foreach (var ch in choices.EnumerateArray())
                    {
                        if (ch.TryGetProperty("delta", out var delta) && delta.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.String)
                        {
                            string piece = content.GetString() ?? "";
                            if (piece.Length > 0) { text.Append(piece); onDelta(piece); }
                        }
                        if (ch.TryGetProperty("finish_reason", out var fr) && fr.ValueKind == JsonValueKind.String)
                            outcome.StopReason = fr.GetString() ?? "";
                    }
                if (root.TryGetProperty("usage", out var usage) && usage.ValueKind == JsonValueKind.Object)
                {
                    outcome.InputTokens = Int(usage, "prompt_tokens") ?? outcome.InputTokens;
                    outcome.OutputTokens = Int(usage, "completion_tokens") ?? outcome.OutputTokens;
                    if (usage.TryGetProperty("prompt_tokens_details", out var det) && det.ValueKind == JsonValueKind.Object)
                        outcome.CacheReadTokens = Int(det, "cached_tokens") ?? outcome.CacheReadTokens;
                }
            }
            outcome.Text = text.ToString();
            outcome.Truncated = outcome.StopReason == "length";
            outcome.Refused = outcome.StopReason == "content_filter";
            return outcome;
        }

        public static Task<AiChatOutcome> OpenAiCompatibleAsync(ProviderConnection c, AiChatCall call, Action<string> onDelta, CancellationToken ct) =>
            // OpenAI نفسه يعتمد max_completion_tokens (ونماذج الاستدلال ترفض max_tokens)، والخدمات المتوافقة الأخرى تقبل max_tokens.
            OpenAiAsync(c, Url(c, "chat/completions"), call, null, c.ProviderType == AiProviderTypes.OpenAI, onDelta, ct);

        public static Task<AiChatOutcome> AzureAsync(ProviderConnection c, AiChatCall call, Action<string> onDelta, CancellationToken ct)
        {
            // على Azure اسم النموذج هو اسم الـ deployment.
            string deployment = string.IsNullOrWhiteSpace(call.Model) ? c.AzureDeployment ?? "" : call.Model;
            if (deployment.Length == 0) throw new AiProviderException("أدخل اسم الـ deployment لمزوّد Azure.");
            var url = Url(c, "openai/deployments/" + Uri.EscapeDataString(deployment) + "/chat/completions?api-version=" + Uri.EscapeDataString(c.AzureApiVersion));
            return OpenAiAsync(c, url, call, "api-key", true, onDelta, ct);
        }

        // ───────────── Google Gemini ─────────────

        public static async Task<AiChatOutcome> GeminiAsync(ProviderConnection c, AiChatCall call, Action<string> onDelta, CancellationToken ct)
        {
            var contents = new JsonArray();
            foreach (var m in call.Messages)
                contents.Add(new JsonObject
                {
                    ["role"] = m.Role == "assistant" ? "model" : "user",
                    ["parts"] = new JsonArray { new JsonObject { ["text"] = m.Text } },
                });
            var gen = new JsonObject { ["maxOutputTokens"] = call.MaxTokens };
            if (call.Temperature.HasValue) gen["temperature"] = (double)call.Temperature.Value;
            var body = new JsonObject { ["contents"] = contents, ["generationConfig"] = gen };
            if (!string.IsNullOrWhiteSpace(call.System))
                body["systemInstruction"] = new JsonObject { ["parts"] = new JsonArray { new JsonObject { ["text"] = call.System } } };

            string model = call.Model.StartsWith("models/", StringComparison.Ordinal) ? call.Model["models/".Length..] : call.Model;
            var url = Url(c, "models/" + Uri.EscapeDataString(model) + ":streamGenerateContent?alt=sse");
            var outcome = new AiChatOutcome();
            var text = new StringBuilder();
            using var resp = await AiHttp.PostStreamAsync(c, url, body.ToJsonString(), "x-goog-api-key", ct);
            await foreach (var line in AiHttp.ReadLinesAsync(resp, ct))
            {
                string? data = AiHttp.SseData(line);
                if (data == null) continue;
                using var doc = Parse(data);
                if (doc == null) continue;
                var root = doc.RootElement;
                if (AiHttp.StreamError(root, c) is { } err) throw new AiProviderException(err);
                if (root.TryGetProperty("promptFeedback", out var pf) && pf.TryGetProperty("blockReason", out var br) && br.ValueKind == JsonValueKind.String)
                {
                    outcome.Refused = true;
                    outcome.StopReason = br.GetString() ?? "blocked";
                }
                if (root.TryGetProperty("candidates", out var cands) && cands.ValueKind == JsonValueKind.Array && cands.GetArrayLength() > 0)
                {
                    var cand = cands[0];
                    if (cand.TryGetProperty("content", out var content) && content.TryGetProperty("parts", out var parts) && parts.ValueKind == JsonValueKind.Array)
                        foreach (var part in parts.EnumerateArray())
                        {
                            if (part.TryGetProperty("thought", out var th) && th.ValueKind == JsonValueKind.True) continue;   // ملخص التفكير لا يُعرض كرد
                            if (part.TryGetProperty("text", out var t) && t.ValueKind == JsonValueKind.String)
                            {
                                string piece = t.GetString() ?? "";
                                if (piece.Length > 0) { text.Append(piece); onDelta(piece); }
                            }
                        }
                    if (cand.TryGetProperty("finishReason", out var fr) && fr.ValueKind == JsonValueKind.String)
                        outcome.StopReason = fr.GetString() ?? "";
                }
                if (root.TryGetProperty("usageMetadata", out var um) && um.ValueKind == JsonValueKind.Object)
                {
                    outcome.InputTokens = Int(um, "promptTokenCount") ?? outcome.InputTokens;
                    outcome.OutputTokens = Int(um, "candidatesTokenCount") ?? outcome.OutputTokens;
                    outcome.CacheReadTokens = Int(um, "cachedContentTokenCount") ?? outcome.CacheReadTokens;
                }
            }
            outcome.Text = text.ToString();
            outcome.Truncated = outcome.StopReason == "MAX_TOKENS";
            outcome.Refused |= outcome.StopReason is "SAFETY" or "PROHIBITED_CONTENT" or "BLOCKLIST" or "SPII" or "RECITATION";
            return outcome;
        }

        // ───────────── Ollama (/api/chat، NDJSON) ─────────────

        public static async Task<AiChatOutcome> OllamaAsync(ProviderConnection c, AiChatCall call, Action<string> onDelta, CancellationToken ct)
        {
            var messages = new JsonArray();
            if (!string.IsNullOrWhiteSpace(call.System)) messages.Add(new JsonObject { ["role"] = "system", ["content"] = call.System });
            foreach (var m in call.Messages) messages.Add(new JsonObject { ["role"] = m.Role, ["content"] = m.Text });
            var options = new JsonObject { ["num_predict"] = call.MaxTokens };
            if (call.Temperature.HasValue) options["temperature"] = (double)call.Temperature.Value;
            var body = new JsonObject { ["model"] = call.Model, ["messages"] = messages, ["stream"] = true, ["options"] = options };

            var outcome = new AiChatOutcome();
            var text = new StringBuilder();
            using var resp = await AiHttp.PostStreamAsync(c, Url(c, "api/chat"), body.ToJsonString(), null, ct);
            await foreach (var line in AiHttp.ReadLinesAsync(resp, ct))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                using var doc = Parse(line);
                if (doc == null) continue;
                var root = doc.RootElement;
                if (AiHttp.StreamError(root, c) is { } err) throw new AiProviderException(err);
                if (root.TryGetProperty("message", out var msg) && msg.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.String)
                {
                    string piece = content.GetString() ?? "";
                    if (piece.Length > 0) { text.Append(piece); onDelta(piece); }
                }
                if (root.TryGetProperty("done", out var done) && done.ValueKind == JsonValueKind.True)
                {
                    outcome.StopReason = root.TryGetProperty("done_reason", out var dr) && dr.ValueKind == JsonValueKind.String ? dr.GetString() ?? "stop" : "stop";
                    outcome.InputTokens = Int(root, "prompt_eval_count");
                    outcome.OutputTokens = Int(root, "eval_count");
                    break;
                }
            }
            outcome.Text = text.ToString();
            outcome.Truncated = outcome.StopReason == "length";
            return outcome;
        }

        // ───────────── أدوات ─────────────

        private static JsonDocument? Parse(string json)
        {
            try { return JsonDocument.Parse(json); }
            catch (JsonException) { return null; }   // سطر غير مكتمل أو تعليق: يُتجاهل
        }

        private static int? Int(JsonElement obj, string name) =>
            obj.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.Number && v.TryGetInt32(out int i) ? i : null;
    }
}
