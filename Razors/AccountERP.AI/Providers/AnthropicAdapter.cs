using AccountModelsLib;
using Anthropic;
using Anthropic.Exceptions;
using Anthropic.Models.Messages;
using Anthropic.Models.Models;

namespace AccountERP.AI.Providers
{
    /// <summary>
    /// Anthropic (Claude) والمزوّد المخصص بالبروتوكول Anthropic-compatible، عبر حزمة NuGet الرسمية Anthropic (لا shim متوافق مع OpenAI).
    /// اختبار الاتصال = قائمة النماذج (GET /v1/models): يتحقق من المفتاح والعنوان بلا استهلاك توكنز.
    /// </summary>
    internal sealed class AnthropicAdapter : IAiProviderAdapter
    {
        public const string DefaultBaseUrl = "https://api.anthropic.com";

        private static AnthropicClient CreateClient(ProviderConnection c, HttpClient http, TimeSpan timeout)
        {
            if (string.IsNullOrEmpty(c.ApiKey))
                throw new AiProviderException("أدخل مفتاح الـ API الخاص بـ Anthropic أولاً.");
            // Header مخصص للمفتاح (مزوّد مخصص خلف بوابة): يُرسل لنفس العنوان المسجّل فقط.
            if (!string.IsNullOrWhiteSpace(c.AuthHeaderName) && !c.AuthHeaderName.Equals("x-api-key", StringComparison.OrdinalIgnoreCase))
                http.DefaultRequestHeaders.TryAddWithoutValidation(c.AuthHeaderName.Trim(), c.ApiKey);
            return new AnthropicClient
            {
                ApiKey = c.ApiKey,
                BaseUrl = c.BaseUrl.AbsoluteUri.TrimEnd('/'),
                HttpClient = http,
                Timeout = timeout,
                MaxRetries = 0,
            };
        }

        /// <summary>
        /// P1: محادثة نصية بالبث. التفكير يُترك على الافتراضى (adaptive دائماً على نماذج Claude 5.x؛ لا يُرسل budget_tokens، ولا temperature
        /// لنموذج لا يدعمها)، والجهد effort من ربط المهام. Prompt caching تلقائى على بادئة المحادثة لتقليل تكلفة الأدوار المتتالية.
        /// stop_reason = refusal يعود Refused=true فيجرّب المنسّق النموذج البديل فى ربط المهام.
        /// </summary>
        public async Task<Chat.AiChatOutcome> StreamChatAsync(ProviderConnection c, Chat.AiChatCall call, Action<string> onDelta, CancellationToken ct)
        {
            using var http = AiHttp.CreateClient(c);
            // البث قد يطول دقائق: المهلة هنا للطلب كاملاً (أوسع من مهلة الاتصال)، والإيقاف بالـ ct.
            var client = CreateClient(c, http, TimeSpan.FromMinutes(15));
            var p = new MessageCreateParams
            {
                Model = call.Model,
                MaxTokens = call.MaxTokens,
                System = call.System,
                Messages = call.Messages.Select(ToParam).ToList(),
                CacheControl = new CacheControlEphemeral(),
            };
            if (call.Tools.Count > 0)
                p = p with { Tools = call.Tools.Select(ToTool).ToList() };
            var effort = ParseEffort(call.Effort);
            if (effort != null) p = p with { OutputConfig = new OutputConfig { Effort = effort.Value } };
            // temperature لا يُرسل لـ Claude: الحزمة الحالية لا تعرضه والنماذج الحالية ترفضه (400).

            var outcome = new Chat.AiChatOutcome();
            var text = new System.Text.StringBuilder();
            // الكتل بترتيب فهرسها فى البث: نص / تفكير (مع التوقيع) / طلب أداة (المدخلات تصل أجزاء JSON).
            var blocks = new SortedDictionary<long, Chat.AiBlock>();
            var inputs = new Dictionary<long, System.Text.StringBuilder>();
            try
            {
                await foreach (RawMessageStreamEvent ev in client.Messages.CreateStreaming(p, ct).WithCancellation(ct))
                {
                    if (ev.TryPickStart(out var start))
                    {
                        outcome.InputTokens = (int)start.Message.Usage.InputTokens;
                        outcome.CacheReadTokens = (int?)start.Message.Usage.CacheReadInputTokens;
                    }
                    else if (ev.TryPickContentBlockStart(out var cbs))
                    {
                        long i = cbs.Index;
                        if (cbs.ContentBlock.TryPickToolUse(out var tu))
                        {
                            blocks[i] = new Chat.AiBlock { Kind = Chat.AiBlock.KToolUse, ToolId = tu.ID, ToolName = tu.Name };
                            inputs[i] = new System.Text.StringBuilder();
                        }
                        else if (cbs.ContentBlock.TryPickThinking(out var th))
                            blocks[i] = new Chat.AiBlock { Kind = Chat.AiBlock.KThinking, Text = th.Thinking ?? "", Signature = th.Signature };
                        else if (cbs.ContentBlock.TryPickText(out _))
                            blocks[i] = Chat.AiBlock.TextBlock("");
                    }
                    else if (ev.TryPickContentBlockDelta(out var cbd))
                    {
                        long i = cbd.Index;
                        if (cbd.Delta.TryPickText(out var t))
                        {
                            if (!string.IsNullOrEmpty(t.Text))
                            {
                                text.Append(t.Text); onDelta(t.Text);
                                if (blocks.TryGetValue(i, out var b)) b.Text += t.Text; else blocks[i] = Chat.AiBlock.TextBlock(t.Text);
                            }
                        }
                        else if (cbd.Delta.TryPickInputJson(out var ij))
                        {
                            if (inputs.TryGetValue(i, out var sb)) sb.Append(ij.PartialJson);
                        }
                        else if (cbd.Delta.TryPickThinking(out var td))
                        {
                            if (blocks.TryGetValue(i, out var b)) b.Text += td.Thinking;
                        }
                        else if (cbd.Delta.TryPickSignature(out var sd))
                        {
                            if (blocks.TryGetValue(i, out var b)) b.Signature = sd.Signature;
                        }
                    }
                    else if (ev.TryPickDelta(out var md))
                    {
                        if (md.Delta.StopReason != null) outcome.StopReason = StopReasonText(md.Delta.StopReason);
                        outcome.OutputTokens = (int)md.Usage.OutputTokens;
                    }
                }
            }
            catch (AnthropicUnauthorizedException) { throw new AiProviderException(AiHttp.Describe(System.Net.HttpStatusCode.Unauthorized, "")); }
            catch (AnthropicForbiddenException ex) { throw new AiProviderException(AiHttp.Describe(System.Net.HttpStatusCode.Forbidden, c.Scrub(ex.Message))); }
            catch (AnthropicNotFoundException) { throw new AiProviderException(AiHttp.Describe(System.Net.HttpStatusCode.NotFound, "النموذج " + call.Model)); }
            catch (AnthropicRateLimitException) { throw new AiProviderException(AiHttp.Describe((System.Net.HttpStatusCode)429, "")); }
            catch (AnthropicBadRequestException ex) { throw new AiProviderException("رفض Anthropic الطلب (400): " + c.Scrub(ex.Message)); }
            catch (AnthropicApiException ex) { throw new AiProviderException("رفض Anthropic الطلب: " + c.Scrub(ex.Message)); }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
            catch (OperationCanceledException) { throw new AiProviderException("انتهت مهلة الرد من Anthropic."); }
            catch (AnthropicException ex) { throw new AiProviderException("تعذّر الاتصال بـ Anthropic: " + c.Scrub(ex.InnerException?.Message ?? ex.Message)); }
            catch (HttpRequestException ex) { throw new AiProviderException("تعذّر الوصول للعنوان " + c.BaseUrl.Host + ": " + c.Scrub(ex.Message)); }

            foreach (var (i, b) in blocks)
            {
                if (b.Kind == Chat.AiBlock.KToolUse)
                {
                    string json = inputs.TryGetValue(i, out var sb) && sb.Length > 0 ? sb.ToString() : "{}";
                    b.InputJson = json;
                    outcome.ToolCalls.Add(new Chat.AiToolUse { Id = b.ToolId, Name = b.ToolName, InputJson = json });
                }
                outcome.Blocks.Add(b);
            }
            outcome.Text = text.ToString();
            outcome.Refused = outcome.StopReason == "refusal";
            outcome.Truncated = outcome.StopReason == "max_tokens";
            return outcome;
        }

        /// <summary>دور محايد ← رسالة Anthropic. الأدوار النصية القديمة نص فقط؛ أدوار دورة الأدوات بكتلها (التفكير يُعاد بتوقيعه دون تغيير).</summary>
        private static MessageParam ToParam(Chat.AiChatTurn m)
        {
            if (m.Blocks == null)
                return m.Role == "assistant" ? new AssistantMessageParam { Content = m.Text } : new UserMessageParam { Content = m.Text };
            var content = new List<ContentBlockParam>();
            foreach (var b in m.Blocks)
            {
                switch (b.Kind)
                {
                    case Chat.AiBlock.KText:
                        if (!string.IsNullOrEmpty(b.Text)) content.Add(new TextBlockParam { Text = b.Text });
                        break;
                    case Chat.AiBlock.KThinking:
                        if (!string.IsNullOrEmpty(b.Signature)) content.Add(new ThinkingBlockParam { Thinking = b.Text ?? "", Signature = b.Signature });
                        break;
                    case Chat.AiBlock.KToolUse:
                        content.Add(new ToolUseBlockParam { ID = b.ToolId, Name = b.ToolName, Input = ParseInput(b.InputJson) });
                        break;
                    case Chat.AiBlock.KToolResult:
                        content.Add(new ToolResultBlockParam { ToolUseID = b.ToolId, Content = b.Text, IsError = b.IsError });
                        break;
                }
            }
            return m.Role == "assistant" ? new AssistantMessageParam { Content = content } : new UserMessageParam { Content = content };
        }

        private static IReadOnlyDictionary<string, System.Text.Json.JsonElement> ParseInput(string json)
        {
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(string.IsNullOrWhiteSpace(json) ? "{}" : json);
                if (doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Object)
                    return doc.RootElement.EnumerateObject().ToDictionary(pr => pr.Name, pr => pr.Value.Clone());
            }
            catch (System.Text.Json.JsonException) { }
            return new Dictionary<string, System.Text.Json.JsonElement>();
        }

        /// <summary>تعريف الأداة من JSON Schema المحايد. البث الفورى للمدخلات مفعّل؛ المدخلات تُتحقق على الخادم قبل أى تنفيذ.</summary>
        private static ToolUnion ToTool(Chat.AiToolDef t)
        {
            var props = new Dictionary<string, System.Text.Json.JsonElement>();
            if (t.Schema["properties"] is System.Text.Json.Nodes.JsonObject po)
                foreach (var kv in po)
                    props[kv.Key] = System.Text.Json.JsonSerializer.SerializeToElement(kv.Value);
            var required = (t.Schema["required"] as System.Text.Json.Nodes.JsonArray)?.Select(x => x!.GetValue<string>()).ToList() ?? new List<string>();
            return new Tool
            {
                Name = t.Name,
                Description = t.Description,
                InputSchema = new() { Properties = props, Required = required },
                EagerInputStreaming = true,
            };
        }

        private static Effort? ParseEffort(string? e) => (e ?? "").Trim().ToLowerInvariant() switch
        {
            "low" => Effort.Low,
            "medium" => Effort.Medium,
            "high" => Effort.High,
            "xhigh" => Effort.Xhigh,
            "max" => Effort.Max,
            _ => null,
        };

        private static string StopReasonText(object sr)
        {
            string s = (sr.ToString() ?? "").Trim().Trim('"');   // ApiEnum يعيد القيمة الخام بصيغة JSON (بين علامتى تنصيص)
            string norm = s.Replace("_", "").ToLowerInvariant();
            return norm switch
            {
                "refusal" => "refusal",
                "maxtokens" => "max_tokens",
                "endturn" => "end_turn",
                _ => s.ToLowerInvariant(),
            };
        }

        public async Task<List<AiRemoteModel>> ListModelsAsync(ProviderConnection c, CancellationToken ct)
        {
            using var http = AiHttp.CreateClient(c);
            var client = CreateClient(c, http, c.Timeout);
            try
            {
                var page = await client.Models.List(new ModelListParams { Limit = 1000 }, ct);
                return page.Items.Select(m => new AiRemoteModel
                {
                    ModelId = m.ID,
                    DisplayName = string.IsNullOrWhiteSpace(m.DisplayName) ? m.ID : m.DisplayName,
                    SupportsVision = true,
                }).ToList();
            }
            catch (AnthropicUnauthorizedException) { throw new AiProviderException(AiHttp.Describe(System.Net.HttpStatusCode.Unauthorized, "")); }
            catch (AnthropicForbiddenException ex) { throw new AiProviderException(AiHttp.Describe(System.Net.HttpStatusCode.Forbidden, c.Scrub(ex.Message))); }
            catch (AnthropicNotFoundException) { throw new AiProviderException(AiHttp.Describe(System.Net.HttpStatusCode.NotFound, "")); }
            catch (AnthropicRateLimitException) { throw new AiProviderException(AiHttp.Describe((System.Net.HttpStatusCode)429, "")); }
            catch (AnthropicApiException ex) { throw new AiProviderException("رفض Anthropic الطلب: " + c.Scrub(ex.Message)); }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                throw new AiProviderException($"انتهت مهلة الاتصال ({(int)c.Timeout.TotalSeconds} ثانية).");
            }
            catch (AnthropicException ex) { throw new AiProviderException("تعذّر الاتصال بـ Anthropic: " + c.Scrub(ex.InnerException?.Message ?? ex.Message)); }
            catch (HttpRequestException ex) { throw new AiProviderException("تعذّر الوصول للعنوان " + c.BaseUrl.Host + ": " + c.Scrub(ex.Message)); }
        }
    }
}
