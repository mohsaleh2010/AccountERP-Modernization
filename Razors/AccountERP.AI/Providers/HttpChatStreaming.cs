using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using AccountERP.AI.Chat;
using AccountModelsLib;

namespace AccountERP.AI.Providers
{
    /// <summary>
    /// P1/P2: بثّ المحادثة للمزوّدين عبر HTTP (OpenAI-compatible / Azure / Gemini / Ollama) مع الأدوات (function calling).
    /// كل مزوّد يحوَّل لنفس AiChatOutcome (نص + طلبات أدوات + كتل الرد)، والأخطاء رسائل عربية بلا مفاتيح. Claude عبر حزمة Anthropic الرسمية فى AnthropicAdapter.
    /// </summary>
    internal static class HttpChatStreaming
    {
        private static Uri Url(ProviderConnection c, string relative) => new(c.BaseUrl.AbsoluteUri.TrimEnd('/') + "/" + relative.TrimStart('/'));

        // ───────────── OpenAI Chat Completions (OpenAI / OpenRouter / LM Studio / مخصص / Azure) ─────────────

        private static JsonArray OpenAiMessages(AiChatCall call, bool argumentsAsObject)
        {
            var messages = new JsonArray();
            if (!string.IsNullOrWhiteSpace(call.System)) messages.Add(new JsonObject { ["role"] = "system", ["content"] = call.System });
            foreach (var m in call.Messages)
            {
                if (m.Blocks == null) { messages.Add(new JsonObject { ["role"] = m.Role, ["content"] = m.Text }); continue; }
                if (m.Role == "assistant")
                {
                    string text = string.Concat(m.Blocks.Where(b => b.Kind == AiBlock.KText).Select(b => b.Text));
                    var msg = new JsonObject { ["role"] = "assistant", ["content"] = text.Length > 0 ? text : null };
                    var calls = new JsonArray();
                    foreach (var b in m.Blocks.Where(b => b.Kind == AiBlock.KToolUse))
                    {
                        JsonNode args = argumentsAsObject ? ParseObject(b.InputJson) : JsonValue.Create(b.InputJson)!;
                        var fn = new JsonObject { ["name"] = b.ToolName, ["arguments"] = args };
                        calls.Add(argumentsAsObject
                            ? new JsonObject { ["function"] = fn }
                            : new JsonObject { ["id"] = b.ToolId, ["type"] = "function", ["function"] = fn });
                    }
                    if (calls.Count > 0) msg["tool_calls"] = calls;
                    messages.Add(msg);
                }
                else
                {
                    foreach (var b in m.Blocks)
                    {
                        if (b.Kind == AiBlock.KToolResult)
                            messages.Add(argumentsAsObject
                                ? new JsonObject { ["role"] = "tool", ["content"] = b.Text, ["tool_name"] = b.ToolName }
                                : new JsonObject { ["role"] = "tool", ["tool_call_id"] = b.ToolId, ["content"] = b.Text });
                        else if (b.Kind == AiBlock.KText && b.Text.Length > 0)
                            messages.Add(new JsonObject { ["role"] = "user", ["content"] = b.Text });
                    }
                }
            }
            return messages;
        }

        private static JsonArray OpenAiTools(AiChatCall call)
        {
            var tools = new JsonArray();
            foreach (var t in call.Tools)
                tools.Add(new JsonObject
                {
                    ["type"] = "function",
                    ["function"] = new JsonObject { ["name"] = t.Name, ["description"] = t.Description, ["parameters"] = t.Schema.DeepClone() },
                });
            return tools;
        }

        public static async Task<AiChatOutcome> OpenAiAsync(ProviderConnection c, Uri url, AiChatCall call, string? defaultAuthHeader,
                                                             bool useMaxCompletionTokens, Action<string> onDelta, CancellationToken ct)
        {
            var body = new JsonObject
            {
                ["model"] = call.Model,
                ["messages"] = OpenAiMessages(call, false),
                ["stream"] = true,
                ["stream_options"] = new JsonObject { ["include_usage"] = true },
                [useMaxCompletionTokens ? "max_completion_tokens" : "max_tokens"] = call.MaxTokens,
            };
            if (call.Tools.Count > 0) body["tools"] = OpenAiTools(call);
            if (call.Temperature.HasValue) body["temperature"] = (double)call.Temperature.Value;

            var outcome = new AiChatOutcome();
            var text = new StringBuilder();
            // طلبات الأدوات تصل أجزاء بفهرسها: المعرّف والاسم أولاً ثم arguments نصاً مقسّماً.
            var calls = new SortedDictionary<int, (string Id, string Name, StringBuilder Args)>();
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
                        if (ch.TryGetProperty("delta", out var delta) && delta.ValueKind == JsonValueKind.Object)
                        {
                            if (delta.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.String)
                            {
                                string piece = content.GetString() ?? "";
                                if (piece.Length > 0) { text.Append(piece); onDelta(piece); }
                            }
                            if (delta.TryGetProperty("tool_calls", out var tcs) && tcs.ValueKind == JsonValueKind.Array)
                                foreach (var tc in tcs.EnumerateArray())
                                {
                                    int idx = tc.TryGetProperty("index", out var ix) && ix.TryGetInt32(out int n) ? n : calls.Count;
                                    if (!calls.TryGetValue(idx, out var cur)) cur = ("", "", new StringBuilder());
                                    if (tc.TryGetProperty("id", out var id) && id.ValueKind == JsonValueKind.String) cur.Id = id.GetString() ?? cur.Id;
                                    if (tc.TryGetProperty("function", out var fn) && fn.ValueKind == JsonValueKind.Object)
                                    {
                                        if (fn.TryGetProperty("name", out var nm) && nm.ValueKind == JsonValueKind.String) cur.Name += nm.GetString();
                                        if (fn.TryGetProperty("arguments", out var ar) && ar.ValueKind == JsonValueKind.String) cur.Args.Append(ar.GetString());
                                    }
                                    calls[idx] = cur;
                                }
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
            if (outcome.Text.Length > 0) outcome.Blocks.Add(AiBlock.TextBlock(outcome.Text));
            foreach (var (_, cl) in calls)
            {
                if (string.IsNullOrEmpty(cl.Name)) continue;
                string id = string.IsNullOrEmpty(cl.Id) ? "call_" + Guid.NewGuid().ToString("N")[..12] : cl.Id;
                AddToolCall(outcome, id, cl.Name, cl.Args.Length > 0 ? cl.Args.ToString() : "{}", null);
            }
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
            {
                var parts = new JsonArray();
                if (m.Blocks == null) parts.Add(new JsonObject { ["text"] = m.Text });
                else
                    foreach (var b in m.Blocks)
                    {
                        if (b.Kind == AiBlock.KText && b.Text.Length > 0) parts.Add(new JsonObject { ["text"] = b.Text });
                        else if (b.Kind == AiBlock.KToolUse)
                        {
                            var part = new JsonObject { ["functionCall"] = new JsonObject { ["name"] = b.ToolName, ["args"] = ParseObject(b.InputJson) } };
                            if (!string.IsNullOrEmpty(b.ProviderSignature)) part["thoughtSignature"] = b.ProviderSignature;
                            parts.Add(part);
                        }
                        else if (b.Kind == AiBlock.KToolResult)
                            parts.Add(new JsonObject
                            {
                                ["functionResponse"] = new JsonObject
                                {
                                    ["name"] = b.ToolName,
                                    ["response"] = new JsonObject { [b.IsError ? "error" : "content"] = b.Text },
                                },
                            });
                    }
                if (parts.Count == 0) continue;
                contents.Add(new JsonObject { ["role"] = m.Role == "assistant" ? "model" : "user", ["parts"] = parts });
            }
            var gen = new JsonObject { ["maxOutputTokens"] = call.MaxTokens };
            if (call.Temperature.HasValue) gen["temperature"] = (double)call.Temperature.Value;
            var body = new JsonObject { ["contents"] = contents, ["generationConfig"] = gen };
            if (!string.IsNullOrWhiteSpace(call.System))
                body["systemInstruction"] = new JsonObject { ["parts"] = new JsonArray { new JsonObject { ["text"] = call.System } } };
            if (call.Tools.Count > 0)
            {
                var decls = new JsonArray();
                foreach (var t in call.Tools)
                {
                    var d = new JsonObject { ["name"] = t.Name, ["description"] = t.Description };
                    // Gemini يرفض كائن parameters بلا خصائص: الأداة بلا مدخلات تُعرَّف بلا parameters.
                    if (t.Schema["properties"] is JsonObject props && props.Count > 0) d["parameters"] = t.Schema.DeepClone();
                    decls.Add(d);
                }
                body["tools"] = new JsonArray { new JsonObject { ["functionDeclarations"] = decls } };
            }

            string model = call.Model.StartsWith("models/", StringComparison.Ordinal) ? call.Model["models/".Length..] : call.Model;
            var url = Url(c, "models/" + Uri.EscapeDataString(model) + ":streamGenerateContent?alt=sse");
            var outcome = new AiChatOutcome();
            var text = new StringBuilder();
            var fnCalls = new List<(string Name, string Args, string? Sig)>();
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
                            if (part.TryGetProperty("functionCall", out var fc) && fc.ValueKind == JsonValueKind.Object)
                            {
                                string name = fc.TryGetProperty("name", out var nm) ? nm.GetString() ?? "" : "";
                                string args = fc.TryGetProperty("args", out var ar) ? ar.GetRawText() : "{}";
                                string? sig = part.TryGetProperty("thoughtSignature", out var ts) && ts.ValueKind == JsonValueKind.String ? ts.GetString() : null;
                                if (name.Length > 0) fnCalls.Add((name, args, sig));
                                continue;
                            }
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
            if (outcome.Text.Length > 0) outcome.Blocks.Add(AiBlock.TextBlock(outcome.Text));
            foreach (var (name, args, sig) in fnCalls)
                AddToolCall(outcome, "call_" + Guid.NewGuid().ToString("N")[..12], name, args, sig);
            outcome.Truncated = outcome.StopReason == "MAX_TOKENS";
            outcome.Refused |= outcome.StopReason is "SAFETY" or "PROHIBITED_CONTENT" or "BLOCKLIST" or "SPII" or "RECITATION";
            return outcome;
        }

        // ───────────── Ollama (/api/chat، NDJSON) ─────────────

        public static async Task<AiChatOutcome> OllamaAsync(ProviderConnection c, AiChatCall call, Action<string> onDelta, CancellationToken ct)
        {
            var options = new JsonObject { ["num_predict"] = call.MaxTokens };
            if (call.Temperature.HasValue) options["temperature"] = (double)call.Temperature.Value;
            var body = new JsonObject { ["model"] = call.Model, ["messages"] = OpenAiMessages(call, true), ["stream"] = true, ["options"] = options };
            if (call.Tools.Count > 0) body["tools"] = OpenAiTools(call);

            var outcome = new AiChatOutcome();
            var text = new StringBuilder();
            var fnCalls = new List<(string Name, string Args)>();
            using var resp = await AiHttp.PostStreamAsync(c, Url(c, "api/chat"), body.ToJsonString(), null, ct);
            await foreach (var line in AiHttp.ReadLinesAsync(resp, ct))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                using var doc = Parse(line);
                if (doc == null) continue;
                var root = doc.RootElement;
                if (AiHttp.StreamError(root, c) is { } err) throw new AiProviderException(err);
                if (root.TryGetProperty("message", out var msg) && msg.ValueKind == JsonValueKind.Object)
                {
                    if (msg.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.String)
                    {
                        string piece = content.GetString() ?? "";
                        if (piece.Length > 0) { text.Append(piece); onDelta(piece); }
                    }
                    if (msg.TryGetProperty("tool_calls", out var tcs) && tcs.ValueKind == JsonValueKind.Array)
                        foreach (var tc in tcs.EnumerateArray())
                            if (tc.TryGetProperty("function", out var fn) && fn.TryGetProperty("name", out var nm))
                                fnCalls.Add((nm.GetString() ?? "", fn.TryGetProperty("arguments", out var ar)
                                    ? (ar.ValueKind == JsonValueKind.String ? ar.GetString() ?? "{}" : ar.GetRawText()) : "{}"));
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
            if (outcome.Text.Length > 0) outcome.Blocks.Add(AiBlock.TextBlock(outcome.Text));
            foreach (var (name, args) in fnCalls.Where(f => f.Name.Length > 0))
                AddToolCall(outcome, "call_" + Guid.NewGuid().ToString("N")[..12], name, args, null);
            outcome.Truncated = outcome.StopReason == "length";
            return outcome;
        }

        // ───────────── أدوات ─────────────

        private static void AddToolCall(AiChatOutcome outcome, string id, string name, string args, string? providerSignature)
        {
            outcome.ToolCalls.Add(new AiToolUse { Id = id, Name = name, InputJson = args });
            outcome.Blocks.Add(new AiBlock { Kind = AiBlock.KToolUse, ToolId = id, ToolName = name, InputJson = args, ProviderSignature = providerSignature });
        }

        private static JsonObject ParseObject(string json)
        {
            try { return JsonNode.Parse(string.IsNullOrWhiteSpace(json) ? "{}" : json) as JsonObject ?? new JsonObject(); }
            catch (JsonException) { return new JsonObject(); }
        }

        private static JsonDocument? Parse(string json)
        {
            try { return JsonDocument.Parse(json); }
            catch (JsonException) { return null; }   // سطر غير مكتمل أو تعليق: يُتجاهل
        }

        private static int? Int(JsonElement obj, string name) =>
            obj.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.Number && v.TryGetInt32(out int i) ? i : null;
    }
}
