using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using AccountERP.AI.Providers;
using AccountModelsLib;

namespace AccountERP.AI.Chat
{
    /// <summary>
    /// P1 (ai-assistant-plan.md §7): محادثة نصية بلا أدوات. يختار النموذج من ربط مهمة «محادثة» (الأساسى ثم البدائل)، يطبّق وضع «محلى فقط»،
    /// يبنى تاريخ المحادثة، يبثّ الرد، ويحفظ رسالتى المستخدم والمساعد مع التوكنز والتكلفة التقديرية. لا يسجّل أى مفتاح.
    /// </summary>
    public sealed class AiChatGateway : IAiChatGateway
    {
        public const int MaxUserChars = 8000;
        /// <summary>أقصى ما يُرسل من التاريخ (بالأحرف) حتى لا تكبر التكلفة بلا حد؛ الأقدم يُسقط أولاً.</summary>
        private const int MaxHistoryChars = 60000;
        private const int DefaultMaxTokens = 8000;

        private readonly AiProvidersCLS _providers = new();
        private readonly AiChatCLS _chat = new();
        private readonly ProviderRegistry _registry;

        public AiChatGateway(ProviderRegistry registry) => _registry = registry;

        /// <summary>نموذج مرشح للمحادثة بعد التحقق من التفعيل ووضع «محلى فقط».</summary>
        private sealed record Candidate(AiTaskRoutingRow Route, AiModelRow Model, AiProviderRow Provider);

        private List<Candidate> Candidates(out string reason)
        {
            reason = "";
            var routing = _providers.GetRouting().Where(r => r.TaskKey == AiTaskKeys.Chat).OrderBy(r => r.Priority).ToList();
            if (routing.Count == 0)
            {
                reason = "لم يُحدَّد نموذج للمحادثة بعد. من «إعدادات الذكاء الاصطناعى» ← «النماذج والمهام» اختر نموذجاً لمهمة «محادثة».";
                return new();
            }
            var models = _providers.GetModels().ToDictionary(m => m.Id);
            var providers = _providers.GetProviders().ToDictionary(p => p.Id);
            var settings = _providers.GetSettings();
            bool localOnly = settings.TryGetValue(AiSettingKeys.LocalOnly, out var lo) && lo == "1";
            var list = new List<Candidate>();
            foreach (var r in routing)
            {
                if (!models.TryGetValue(r.Model_Id, out var m) || !m.IsEnabled) continue;
                if (!providers.TryGetValue(m.Provider_Id, out var p) || !p.IsEnabled) continue;
                if (localOnly && !p.IsLocal) continue;
                list.Add(new Candidate(r, m, p));
            }
            if (list.Count == 0)
                reason = localOnly
                    ? "وضع «محلى فقط» مفعّل ولا يوجد نموذج محلى مفعّل لمهمة «محادثة»."
                    : "النماذج المختارة لمهمة «محادثة» أو مزوّدوها معطّلون. راجع «إعدادات الذكاء الاصطناعى».";
            return list;
        }

        public AiChatAvailability GetAvailability()
        {
            try
            {
                var c = Candidates(out string reason);
                return c.Count == 0
                    ? new AiChatAvailability { Ready = false, Message = reason }
                    : new AiChatAvailability { Ready = true, ModelName = c[0].Model.DisplayName };
            }
            catch (Exception ex)
            {
                operations.DataDictionaryLogerException(ex);
                return new AiChatAvailability { Ready = false, Message = "تعذّرت قراءة إعدادات الذكاء الاصطناعى." };
            }
        }

        public List<AiConversationView> GetConversations(long userId, string search) =>
            userId <= 0 ? new() : _chat.GetConversations(userId, search);

        public List<AiChatMessage> GetMessages(long conversationId, long userId)
        {
            if (!_chat.OwnsConversation(conversationId, userId)) throw new InvalidOperationException("المحادثة غير موجودة.");
            return _chat.GetMessages(conversationId).Select(ToView).ToList();
        }

        public void RenameConversation(long conversationId, string title, long userId)
        {
            title = (title ?? "").Trim();
            if (title.Length == 0) throw new InvalidOperationException("أدخل عنوان المحادثة.");
            if (!_chat.OwnsConversation(conversationId, userId)) throw new InvalidOperationException("المحادثة غير موجودة.");
            _chat.RenameConversation(conversationId, title, userId);
        }

        public void DeleteConversation(long conversationId, long userId)
        {
            if (!_chat.OwnsConversation(conversationId, userId)) throw new InvalidOperationException("المحادثة غير موجودة.");
            _chat.DeleteConversation(conversationId, userId);
        }

        public async Task<AiChatTurnResult> SendAsync(long conversationId, string text, AiScreenContext context, long userId, long branchId,
                                                      Action<string> onDelta, CancellationToken ct = default)
        {
            if (userId <= 0) throw new InvalidOperationException("سجّل الدخول أولاً.");
            text = (text ?? "").Trim();
            if (text.Length == 0) throw new InvalidOperationException("اكتب رسالتك أولاً.");
            if (text.Length > MaxUserChars) throw new InvalidOperationException($"الرسالة أطول من الحد ({MaxUserChars} حرف). قسّمها على أكثر من رسالة.");

            var candidates = Candidates(out string reason);
            if (candidates.Count == 0) throw new InvalidOperationException(reason);

            if (conversationId == 0)
                conversationId = _chat.CreateConversation(userId, branchId, TitleFrom(text));
            else if (!_chat.OwnsConversation(conversationId, userId))
                throw new InvalidOperationException("المحادثة غير موجودة (ربما حُذفت).");

            var history = _chat.GetMessages(conversationId);
            var userRow = _chat.AddMessage(new AiMessageRow
            {
                Conversation_Id = conversationId,
                Role = "user",
                ContentJson = TextJson(text),
                Status = AiMessageStatus.Ok,
            }, userId);

            var turns = BuildTurns(history, text);
            string system = SystemPrompt(context);

            var sw = Stopwatch.StartNew();
            var streamed = new StringBuilder();
            void Relay(string piece) { streamed.Append(piece); onDelta(piece); }

            Candidate? used = null;
            AiChatOutcome? outcome = null;
            string? error = null;
            bool stopped = false;
            var tried = new List<string>();
            foreach (var cand in candidates)
            {
                used = cand;
                tried.Add(cand.Model.DisplayName);
                try
                {
                    var conn = _registry.Connect(cand.Provider);
                    var call = new AiChatCall
                    {
                        Model = cand.Model.ModelId,
                        System = system,
                        Messages = turns,
                        MaxTokens = cand.Route.MaxTokens is > 0 ? cand.Route.MaxTokens.Value : DefaultMaxTokens,
                        Effort = string.IsNullOrWhiteSpace(cand.Route.Effort) ? null : cand.Route.Effort,
                        Temperature = cand.Model.SupportsTemperature ? cand.Route.Temperature : null,
                    };
                    outcome = await _registry.AdapterFor(conn.Protocol).StreamChatAsync(conn, call, Relay, ct);
                    error = null;
                    // رفض السلامة بلا نص: نجرّب النموذج البديل التالى فى ربط المهام (إن وجد).
                    if (outcome.Refused && streamed.Length == 0 && cand != candidates[^1]) { error = "refused"; continue; }
                    break;
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested) { stopped = true; break; }
                catch (Exception ex) when (ex is AiProviderException or InvalidOperationException)
                {
                    error = ex.Message;
                    // البديل فقط إن لم يُعرض للمستخدم أى نص بعد (وإلا يختلط ردّان).
                    if (streamed.Length > 0) break;
                }
                catch (Exception ex)
                {
                    operations.DataDictionaryLogerException(ex);
                    error = "خطأ غير متوقع أثناء الاتصال بالمزوّد.";
                    if (streamed.Length > 0) break;
                }
            }
            sw.Stop();

            string finalText = stopped ? streamed.ToString() : outcome?.Text is { Length: > 0 } t ? t : streamed.ToString();
            string status = stopped ? AiMessageStatus.Stopped : error != null && outcome == null ? AiMessageStatus.Error : AiMessageStatus.Ok;
            string? errorText = null;
            if (status == AiMessageStatus.Error)
                errorText = tried.Count > 1 ? error + $" (جُرّبت النماذج: {string.Join("، ", tried)})" : error;
            else if (outcome?.Refused == true)
            {
                status = AiMessageStatus.Error;
                errorText = "اعتذر النموذج عن الإجابة على هذا الطلب لأسباب تتعلق بالسلامة. أعد صياغة السؤال.";
            }
            else if (outcome?.Truncated == true)
                errorText = "توقّف الرد عند الحد الأقصى للتوكنز. اطلب «أكمل» أو ارفع الحد من إعدادات المهام.";

            var model = used?.Model;
            decimal? cost = null;
            if (model != null && outcome != null && (model.InputPricePerM != null || model.OutputPricePerM != null))
                cost = Math.Round(((outcome.InputTokens ?? 0) * (model.InputPricePerM ?? 0) + (outcome.OutputTokens ?? 0) * (model.OutputPricePerM ?? 0)) / 1_000_000m, 6);

            var asstRow = _chat.AddMessage(new AiMessageRow
            {
                Conversation_Id = conversationId,
                Role = "assistant",
                ContentJson = TextJson(finalText),
                Model_Id = model?.Id,
                ModelName = model?.DisplayName,
                InputTokens = outcome?.InputTokens,
                OutputTokens = outcome?.OutputTokens,
                CacheReadTokens = outcome?.CacheReadTokens,
                CostEstimate = cost,
                Status = status,
                ErrorText = errorText,
                DurationMs = (int)Math.Min(int.MaxValue, sw.ElapsedMilliseconds),
            }, userId);

            return new AiChatTurnResult { ConversationId = conversationId, UserMessage = ToView(userRow), AssistantMessage = ToView(asstRow) };
        }

        // ───────────── التاريخ والتعليمات ─────────────

        /// <summary>
        /// يبنى أدوار المحادثة: يتجاهل ردود الخطأ والردود الفارغة، يدمج الأدوار المتتالية من نفس النوع (تبادل user/assistant صارم لدى بعض المزوّدين)،
        /// ويُسقط الأقدم إن تجاوز الحد. أول دور دائماً من المستخدم.
        /// </summary>
        internal static List<AiChatTurn> BuildTurns(IEnumerable<AiMessageRow> history, string newUserText)
        {
            var raw = new List<AiChatTurn>();
            foreach (var m in history)
            {
                if (m.Role == "assistant" && m.Status == AiMessageStatus.Error) continue;
                string t = ReadText(m.ContentJson);
                if (string.IsNullOrWhiteSpace(t)) continue;
                raw.Add(new AiChatTurn(m.Role == "assistant" ? "assistant" : "user", t));
            }
            raw.Add(new AiChatTurn("user", newUserText));

            var merged = new List<AiChatTurn>();
            foreach (var turn in raw)
            {
                if (merged.Count > 0 && merged[^1].Role == turn.Role)
                    merged[^1] = merged[^1] with { Text = merged[^1].Text + "\n\n" + turn.Text };
                else merged.Add(turn);
            }

            int total = merged.Sum(x => x.Text.Length);
            while (merged.Count > 1 && total > MaxHistoryChars)
            {
                total -= merged[0].Text.Length;
                merged.RemoveAt(0);
            }
            while (merged.Count > 1 && merged[0].Role != "user") merged.RemoveAt(0);
            return merged;
        }

        /// <summary>
        /// تعليمات النظام: ثابتة فى أولها (لتستفيد من التخزين المؤقت)، ثم سياق الشاشة. تقليل البيانات: لا اسم مستخدم ولا بيانات حساب.
        /// </summary>
        internal static string SystemPrompt(AiScreenContext? ctx)
        {
            var sb = new StringBuilder();
            sb.AppendLine("أنت «المساعد الذكى» داخل نظام AccountERP، نظام محاسبة وتخطيط موارد مصرى (حسابات عامة، مخازن، مبيعات ومشتريات، فواتير إلكترونية، فروع وعملات).");
            sb.AppendLine("تساعد المحاسبين ومستخدمى النظام: تشرح المفاهيم المحاسبية والضريبية المصرية، وطريقة استخدام شاشات النظام، وتصيغ الردود والمراسلات.");
            sb.AppendLine("في هذه المرحلة لا تملك أى وصول لبيانات الشركة ولا لقاعدة البيانات ولا تستطيع تنفيذ أى عملية داخل النظام.");
            sb.AppendLine("لذلك: لا تخترع أرقاماً أو أرصدة أو أسماء عملاء أو أصناف، ولا تدّعِ أنك نفّذت أو حفظت شيئاً. إن سُئلت عن بيانات فعلية فاشرح أين يجدها المستخدم في النظام (الشاشة أو التقرير المناسب).");
            sb.AppendLine("أجب بلغة المستخدم (العربية افتراضياً، والعامية المصرية مفهومة). كن موجزاً وواضحاً، واستخدم Markdown بسيطاً (عناوين قصيرة، قوائم، جداول عند الحاجة).");
            sb.AppendLine("إن كان السؤال غامضاً فاسأل سؤالاً توضيحياً واحداً قبل الإجابة.");
            if (ctx != null && (!string.IsNullOrWhiteSpace(ctx.Route) || !string.IsNullOrWhiteSpace(ctx.Title)))
            {
                sb.AppendLine();
                sb.Append("الشاشة المفتوحة الآن لدى المستخدم: ");
                if (!string.IsNullOrWhiteSpace(ctx.Title)) sb.Append('«').Append(Clean(ctx.Title, 120)).Append("» ");
                if (!string.IsNullOrWhiteSpace(ctx.Route)) sb.Append('(').Append(Clean(ctx.Route, 200)).Append(')');
                sb.AppendLine();
            }
            if (ctx != null && !string.IsNullOrWhiteSpace(ctx.Language) && !ctx.Language.StartsWith("ar", StringComparison.OrdinalIgnoreCase))
                sb.AppendLine("لغة واجهة المستخدم: " + Clean(ctx.Language, 10) + ".");
            return sb.ToString().TrimEnd();
        }

        private static string Clean(string s, int max)
        {
            s = new string(s.Where(ch => !char.IsControl(ch)).ToArray()).Trim();
            return s.Length > max ? s[..max] : s;
        }

        internal static string TitleFrom(string text)
        {
            string t = string.Join(' ', text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
            return t.Length > 60 ? t[..60].TrimEnd() + "…" : t;
        }

        // ───────────── المحتوى ─────────────

        /// <summary>العربى يُحفظ كما هو (لا رموز \u) حتى يعمل البحث بـ LIKE ولا يتضخم الحجم؛ الناتج للقاعدة لا لـ HTML.</summary>
        private static readonly JsonSerializerOptions StoreJson = new() { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

        internal static string TextJson(string text) =>
            new JsonArray { new JsonObject { ["type"] = "text", ["text"] = text ?? "" } }.ToJsonString(StoreJson);

        internal static string ReadText(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return "";
            try
            {
                using var doc = JsonDocument.Parse(json);
                var sb = new StringBuilder();
                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                    foreach (var b in doc.RootElement.EnumerateArray())
                        if (b.TryGetProperty("type", out var ty) && ty.GetString() == "text" && b.TryGetProperty("text", out var tx))
                            sb.Append(tx.GetString());
                return sb.ToString();
            }
            catch (JsonException) { return json; }
        }

        private static AiChatMessage ToView(AiMessageRow r) => new()
        {
            Id = r.Id,
            Seq = r.Seq,
            Role = r.Role,
            Text = ReadText(r.ContentJson),
            Status = string.IsNullOrEmpty(r.Status) ? AiMessageStatus.Ok : r.Status,
            ErrorText = r.ErrorText ?? "",
            ModelName = r.ModelName ?? "",
            InputTokens = r.InputTokens,
            OutputTokens = r.OutputTokens,
            CostEstimate = r.CostEstimate,
            CreatedAt = r.CreatedAt,
        };
    }
}
