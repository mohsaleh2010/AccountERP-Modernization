using System.Text;
using AccountModelsLib;
using Microsoft.Extensions.DependencyInjection;

namespace AccountERP.SharedUI.Services
{
    /// <summary>
    /// 🤖 المساعد الذكى P1 (ai-assistant-plan.md §7): حالة المحادثة الحالية لكل دائرة (Scoped)، فتتشارك صفحة /assistant والنافذة العائمة
    /// نفس المحادثة والبث تلقائياً. كل الاستدعاءات عبر IAiChatGateway (يسجّله مضيف الويب فقط)؛ بدونه IsAvailable = false ويختفى المساعد.
    /// المحادثات محفوظة فى القاعدة، فإعادة التحميل أو انقطاع الدائرة لا يضيّعها.
    /// </summary>
    public sealed class AssistantSessionService : IDisposable
    {
        public const string MenuKey = "menuItemAiAssistant";
        public const string SettingsMenuKey = "menuItemAiSettings";

        private readonly IAiChatGateway? _gw;
        private readonly UserSession? _session;
        private readonly MenuStateService _menu;
        private readonly object _lock = new();
        private readonly StringBuilder _streaming = new();
        private CancellationTokenSource? _cts;
        private DateTime _lastNotify = DateTime.MinValue;

        private readonly IServiceProvider _sp;

        public AssistantSessionService(IServiceProvider sp, MenuStateService menu)
        {
            _sp = sp;
            _gw = sp.GetService<IAiChatGateway>();
            _session = sp.GetService<UserSession>();
            _menu = menu;
            _menu.OnAuthChanged += OnAuthChanged;
        }

        public bool IsAvailable => _gw != null;
        public long UserId => _session?.User_Id ?? 0;
        private long BranchId => _session?.Branch_Id ?? 0;

        public bool HasAccess => IsAvailable && UserId > 0 && HasMenu(MenuKey);
        public bool CanOpenSettings => HasMenu(SettingsMenuKey);

        public AiChatAvailability Availability { get; private set; } = new() { Ready = false };
        public bool AvailabilityLoaded { get; private set; }
        public long ConversationId { get; private set; }
        public string ConversationTitle { get; private set; } = "";
        public List<AiChatMessage> Messages { get; private set; } = new();
        public List<AiConversationView> Conversations { get; private set; } = new();
        public string Search { get; private set; } = "";
        public bool IsSending { get; private set; }
        public bool IsLoading { get; private set; }
        /// <summary>الرسالة التى يكتبها المستخدم الآن (تُعرض فوراً قبل حفظها).</summary>
        public string? PendingUserText { get; private set; }
        public string? Error { get; private set; }

        public string StreamingText { get { lock (_lock) return _streaming.ToString(); } }
        /// <summary>P2: ما ينفّذه المساعد الآن («استعلام قراءة…»)؛ null عند عدم تنفيذ أداة.</summary>
        public string? ToolStatus { get; private set; }

        /// <summary>أى تغيير فى الحالة (يُستدعى من خيوط خلفية أثناء البث؛ المكوّن يستدعى InvokeAsync(StateHasChanged)).</summary>
        public event Action? OnChanged;
        /// <summary>اكتمل رد (للشارة على الأيقونة العائمة).</summary>
        public event Action? OnReplyCompleted;

        private bool HasMenu(string key) =>
            (_menu.UserAllowedMenus ?? new()).Any(m => string.Equals((m.KeysData ?? "").Trim(), key, StringComparison.OrdinalIgnoreCase));

        private void Notify() => OnChanged?.Invoke();

        private static Task<T> Bg<T>(Func<T> f) => Task.Run(() => { AccountCultureGuard.EnsureInvariant(); return f(); });
        private static Task Bg(Action f) => Task.Run(() => { AccountCultureGuard.EnsureInvariant(); f(); });

        private static string Err(Exception ex) => ex is InvalidOperationException ? ex.Message : (ex.InnerException?.Message ?? ex.Message);

        /// <summary>يحمّل الجاهزية وقائمة المحادثات (مرة عند فتح الصفحة/النافذة، وبعد كل رد).</summary>
        public async Task RefreshAsync()
        {
            if (!HasAccess || _gw == null) return;
            var gw = _gw; long uid = UserId; string search = Search;
            try
            {
                var (av, list) = await Bg(() => (gw.GetAvailability(), gw.GetConversations(uid, search)));
                Availability = av;
                Conversations = list;
                AvailabilityLoaded = true;
            }
            catch (Exception ex) { Error = Err(ex); }
            Notify();
        }

        public async Task SetSearchAsync(string? text)
        {
            Search = (text ?? "").Trim();
            await RefreshAsync();
        }

        public async Task OpenAsync(long conversationId)
        {
            if (_gw == null || IsSending || conversationId == ConversationId) return;
            var gw = _gw; long uid = UserId;
            IsLoading = true; Error = null; Notify();
            try
            {
                Messages = await Bg(() => gw.GetMessages(conversationId, uid));
                ConversationId = conversationId;
                ConversationTitle = Conversations.FirstOrDefault(c => c.Id == conversationId)?.Title ?? "";
            }
            catch (Exception ex) { Error = Err(ex); }
            finally { IsLoading = false; Notify(); }
        }

        public void NewConversation()
        {
            if (IsSending) return;
            ConversationId = 0;
            ConversationTitle = "";
            Messages = new();
            Error = null;
            Notify();
        }

        public async Task SendAsync(string text, AiScreenContext context)
        {
            text = (text ?? "").Trim();
            if (_gw == null || IsSending || text.Length == 0) return;
            var gw = _gw; long convId = ConversationId;
            // P2: ما يحق له قراءته = نفس شاشات قائمة الويب (RazorPath)؛ المساعد لا يرى أداة/View لشاشة غير مسموحة.
            var user = new AiUserContext
            {
                UserId = UserId,
                BranchId = BranchId,
                IsAdmin = (_session?.Is_User_Admin ?? 0) == 1,
                AllowedRoutes = (_menu.UserAllowedMenus ?? new()).Select(m => m.RazorPath ?? "").Where(p => p.Length > 0).Distinct().ToList(),
            };
            _cts = new CancellationTokenSource();
            var ct = _cts.Token;
            lock (_lock) _streaming.Clear();
            IsSending = true; Error = null; PendingUserText = text;
            Notify();
            try
            {
                var result = await Task.Run(async () =>
                {
                    AccountCultureGuard.EnsureInvariant();
                    return await gw.SendAsync(convId, text, context, user, OnDelta, OnToolStatus, ct);
                });
                // المستخدم ربما فتح محادثة أخرى أثناء البث: لا نخلط الرسائل.
                if (ConversationId == convId)
                {
                    ConversationId = result.ConversationId;
                    Messages.Add(result.UserMessage);
                    Messages.Add(result.AssistantMessage);
                    if (convId == 0) ConversationTitle = TitleOf(text);
                }
            }
            catch (Exception ex) { Error = Err(ex); }
            finally
            {
                IsSending = false;
                PendingUserText = null;
                ToolStatus = null;
                lock (_lock) _streaming.Clear();
                _cts?.Dispose(); _cts = null;
            }
            Notify();
            OnReplyCompleted?.Invoke();
            await RefreshAsync();
            if (ConversationId != 0)
                ConversationTitle = Conversations.FirstOrDefault(c => c.Id == ConversationId)?.Title ?? ConversationTitle;
        }

        private static string TitleOf(string text)
        {
            string t = string.Join(' ', text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
            return t.Length > 60 ? t[..60].TrimEnd() + "…" : t;
        }

        /// <summary>حالة الأداة الجارية (خيط خلفى): تُعرض فوراً.</summary>
        private void OnToolStatus(string? status)
        {
            ToolStatus = string.IsNullOrEmpty(status) ? null : status;
            Notify();
        }

        /// <summary>جزء نصى من البث (خيط خلفى): يُجمع، وتحديث الواجهة كل 60ms على الأكثر.</summary>
        private void OnDelta(string piece)
        {
            bool notify;
            lock (_lock)
            {
                _streaming.Append(piece);
                var now = DateTime.UtcNow;
                notify = (now - _lastNotify).TotalMilliseconds >= 60;
                if (notify) _lastNotify = now;
            }
            if (notify) Notify();
        }

        /// <summary>زر الإيقاف: يُحفظ ما كُتب حتى الآن بحالة «أُوقف».</summary>
        public void Stop()
        {
            try { _cts?.Cancel(); } catch (ObjectDisposedException) { }
        }

        public async Task RenameAsync(long conversationId, string title)
        {
            if (_gw == null) return;
            var gw = _gw; long uid = UserId;
            try
            {
                await Bg(() => gw.RenameConversation(conversationId, title, uid));
                if (conversationId == ConversationId) ConversationTitle = title.Trim();
            }
            catch (Exception ex) { Error = Err(ex); }
            await RefreshAsync();
        }

        public async Task DeleteAsync(long conversationId)
        {
            if (_gw == null || (IsSending && conversationId == ConversationId)) return;
            var gw = _gw; long uid = UserId;
            try
            {
                await Bg(() => gw.DeleteConversation(conversationId, uid));
                if (conversationId == ConversationId) NewConversation();
            }
            catch (Exception ex) { Error = Err(ex); }
            await RefreshAsync();
        }

        public void ClearError() { Error = null; Notify(); }

        // ───────────── الإعداد السريع (للمدير) ─────────────

        /// <summary>المدير (صلاحية إعدادات الذكاء الاصطناعى) يختار نموذج المحادثة من صفحة المساعد نفسها بدل الذهاب للإعدادات.</summary>
        public bool CanQuickSetup => _admin != null && HasMenu(SettingsMenuKey);

        private IAiAdminGateway? _admin => _adminLazy ??= _sp.GetService<IAiAdminGateway>();
        private IAiAdminGateway? _adminLazy;

        /// <summary>النماذج الصالحة للمحادثة: مفعّلة، ليست embeddings، ومزوّدها مفعّل.</summary>
        public async Task<List<AiModelRow>> GetChatModelsAsync()
        {
            var admin = _admin;
            if (admin == null || !CanQuickSetup) return new();
            return await Bg(() =>
            {
                var enabledProviders = admin.GetProviders().Where(p => p.IsEnabled).Select(p => p.Id).ToHashSet();
                return admin.GetModels().Where(m => m.IsEnabled && !m.SupportsEmbeddings && enabledProviders.Contains(m.Provider_Id))
                    .OrderBy(m => m.ModelId, StringComparer.OrdinalIgnoreCase).ToList();
            });
        }

        /// <summary>يجعل النموذج أساسياً لمهمة «محادثة» (يحل محل ربط المحادثة فقط؛ ربط المهام الأخرى كما هو).</summary>
        public async Task SetChatModelAsync(long modelId)
        {
            var admin = _admin;
            if (admin == null || !CanQuickSetup) return;
            long uid = UserId;
            try
            {
                await Bg(() =>
                {
                    var rows = admin.GetRouting().Where(r => r.TaskKey != AiTaskKeys.Chat).ToList();
                    rows.Add(new AiTaskRoutingRow { TaskKey = AiTaskKeys.Chat, Model_Id = modelId, Priority = 1, Effort = "" });
                    admin.SaveRouting(rows, uid);
                });
                Error = null;
            }
            catch (Exception ex) { Error = Err(ex); }
            await RefreshAsync();
        }

        /// <summary>خروج أو دخول مستخدم آخر على نفس الدائرة: لا يرث محادثة المستخدم السابق.</summary>
        private void OnAuthChanged()
        {
            Stop();
            ConversationId = 0;
            ConversationTitle = "";
            Messages = new();
            Conversations = new();
            Search = "";
            Error = null;
            AvailabilityLoaded = false;
            Notify();
        }

        public void Dispose()
        {
            _menu.OnAuthChanged -= OnAuthChanged;
            Stop();
        }
    }
}
