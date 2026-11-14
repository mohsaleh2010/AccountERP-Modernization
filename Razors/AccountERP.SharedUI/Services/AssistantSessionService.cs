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

        /// <summary>P4: رسالة المساعد التى تنتظر إجابة سؤاله (آخر رسالة فقط)؛ null إن لا يوجد.</summary>
        public AiChatMessage? PendingAsk =>
            Messages.Count > 0 && Messages[^1] is { Role: "assistant", Ask.Pending: true } m ? m : null;

        /// <summary>رسالة جديدة؛ وإن كان للمساعد سؤال معلّق فالنص المكتوب إجابة حرة عليه (P4). P6: المرفقات المرفوعة تُرسل معها.</summary>
        public Task SendAsync(string text, AiScreenContext context)
        {
            text = (text ?? "").Trim();
            if (_gw == null || IsSending || IsUploading || (text.Length == 0 && PendingAttachments.Count == 0)) return Task.CompletedTask;
            var ask = PendingAsk;
            if (ask != null && PendingAttachments.Count == 0) return AnswerAsync(ask.Id, new List<string>(), text, context);
            var gw = _gw; long convId = ConversationId;
            var user = BuildUser();
            var files = PendingAttachments.ToList();
            var ids = files.Select(f => f.Id).ToList();
            PendingAttachments = new();
            string shown = text.Length > 0 ? text : string.Join("، ", files.Select(f => f.FileName));
            PendingUserFiles = files;
            return RunTurnAsync(shown, convId, ct => gw.SendAsync(convId, text, ids, context, user, OnDelta, OnToolStatus, ct), null, files);
        }

        // ───────────── P6: المرفقات ─────────────

        /// <summary>ملفات رُفعت ولم تُرسل بعد (شرائح فوق مربع الكتابة).</summary>
        public List<AiAttachmentView> PendingAttachments { get; private set; } = new();
        /// <summary>مرفقات الرسالة الجارية (تُعرض فوراً مع نصها).</summary>
        public List<AiAttachmentView> PendingUserFiles { get; private set; } = new();
        public bool IsUploading { get; private set; }
        private AiUploadLimits? _limits;

        public AiUploadLimits UploadLimits
        {
            get
            {
                if (_limits == null && _gw != null)
                {
                    try { _limits = _gw.GetUploadLimits(); } catch (Exception ex) { operations.DataDictionaryLogerException(ex); }
                }
                return _limits ?? new AiUploadLimits { Accept = "" };
            }
        }

        public bool CanUpload => HasAccess && UploadLimits.Accept.Length > 0;

        /// <summary>يرفع ملفاً (يتحقق الخادم من النوع والحجم). يرجع رسالة الخطأ أو null.</summary>
        public async Task<string?> UploadAsync(string fileName, string contentType, Stream content, CancellationToken ct = default)
        {
            if (_gw == null || !CanUpload) return "رفع الملفات غير متاح.";
            if (PendingAttachments.Count >= UploadLimits.MaxFiles) return $"حد الملفات فى الرسالة {UploadLimits.MaxFiles}.";
            var gw = _gw; long uid = UserId;
            IsUploading = true; Notify();
            try
            {
                var view = await gw.UploadAttachmentAsync(uid, fileName, contentType, content, ct);
                PendingAttachments = PendingAttachments.Append(view).ToList();
                return null;
            }
            catch (Exception ex) { return Err(ex); }
            finally { IsUploading = false; Notify(); }
        }

        public async Task RemovePendingAsync(long attachmentId)
        {
            if (_gw == null) return;
            PendingAttachments = PendingAttachments.Where(a => a.Id != attachmentId).ToList();
            Notify();
            var gw = _gw; long uid = UserId;
            try { await Bg(() => gw.RemoveAttachment(attachmentId, uid)); }
            catch (Exception ex) { operations.DataDictionaryLogerException(ex); }
        }

        /// <summary>الملف للمعاينة (null إن انتهت مدة الاحتفاظ).</summary>
        public async Task<AiAttachmentFile?> GetAttachmentFileAsync(long attachmentId)
        {
            if (_gw == null || !HasAccess) return null;
            var gw = _gw; long uid = UserId;
            return await Bg(() => gw.GetAttachmentFile(attachmentId, uid));
        }

        /// <summary>P4: إجابة سؤال المساعد (خيارات مختارة و/أو نص حر) ثم استئناف الرد.</summary>
        public Task AnswerAsync(long messageId, List<string> selected, string? freeText, AiScreenContext context)
        {
            if (_gw == null || IsSending || ConversationId == 0) return Task.CompletedTask;
            var answer = new AiAskAnswer { Selected = selected ?? new(), FreeText = (freeText ?? "").Trim() };
            if (answer.Selected.Count == 0 && answer.FreeText.Length == 0) return Task.CompletedTask;
            string shown = string.Join("، ", answer.Selected);
            if (answer.FreeText.Length > 0) shown = shown.Length > 0 ? shown + " — " + answer.FreeText : answer.FreeText;
            var gw = _gw; long convId = ConversationId;
            var user = BuildUser();
            return RunTurnAsync(shown, convId, ct => gw.AnswerAsync(convId, messageId, answer, context, user, OnDelta, OnToolStatus, ct), messageId);
        }

        private async Task RunTurnAsync(string shownText, long convId, Func<CancellationToken, Task<AiChatTurnResult>> call, long? askMessageId,
                                        List<AiAttachmentView>? files = null)
        {
            _cts = new CancellationTokenSource();
            var ct = _cts.Token;
            lock (_lock) _streaming.Clear();
            IsSending = true; Error = null; PendingUserText = shownText;
            Notify();
            bool resync = false;
            try
            {
                var result = await Task.Run(async () =>
                {
                    AccountCultureGuard.EnsureInvariant();
                    return await call(ct);
                });
                // المستخدم ربما فتح محادثة أخرى أثناء البث: لا نخلط الرسائل.
                if (ConversationId == convId)
                {
                    ConversationId = result.ConversationId;
                    // السؤال المعلّق السابق أُغلق على الخادم (بإجابة أو بتجاوزه): نعيد قراءة الرسائل بدل ترقيعها.
                    if (askMessageId != null || Messages.Any(x => x.Ask?.Pending == true)) resync = true;
                    else
                    {
                        Messages.Add(result.UserMessage);
                        Messages.Add(result.AssistantMessage);
                    }
                    if (convId == 0) ConversationTitle = TitleOf(shownText);
                }
            }
            catch (Exception ex)
            {
                Error = Err(ex);
                resync = askMessageId != null;   // «أُجيب بالفعل» من نافذة أخرى: نعرض الحالة الحقيقية
                // P6: لم تُرسل الرسالة ⇒ المرفقات تعود لمربع الكتابة (إن لم تُربط برسالة على الخادم يمكن إرسالها مجدداً)
                if (files != null && files.Count > 0 && ConversationId == convId) PendingAttachments = files.Concat(PendingAttachments).ToList();
            }
            finally
            {
                IsSending = false;
                PendingUserText = null;
                PendingUserFiles = new();
                ToolStatus = null;
                lock (_lock) _streaming.Clear();
                _cts?.Dispose(); _cts = null;
            }
            if (resync && ConversationId == convId && convId != 0 && _gw != null)
            {
                var gw = _gw; long uid = UserId;
                try { Messages = await Bg(() => gw.GetMessages(convId, uid)); }
                catch (Exception ex) { Error ??= Err(ex); }
            }
            Notify();
            OnReplyCompleted?.Invoke();
            await RefreshAsync();
            if (ConversationId != 0)
                ConversationTitle = Conversations.FirstOrDefault(c => c.Id == ConversationId)?.Title ?? ConversationTitle;
        }

        /// <summary>
        /// P2: ما يحق له قراءته = نفس شاشات قائمة الويب (RazorPath)؛ المساعد لا يرى أداة/View لشاشة غير مسموحة.
        /// P3: نفس الشاشات بعناوينها لأداتى find_screen/open_screen.
        /// </summary>
        public AiUserContext BuildUser()
        {
            var menus = (_menu.UserAllowedMenus ?? new()).Where(m => !string.IsNullOrWhiteSpace(m.RazorPath)).ToList();
            return new AiUserContext
            {
                UserId = UserId,
                BranchId = BranchId,
                IsAdmin = (_session?.Is_User_Admin ?? 0) == 1,
                AllowedRoutes = menus.Select(m => m.RazorPath!.Trim()).Distinct().ToList(),
                AllowedScreens = menus.Select(m => new AiScreenRef { Path = m.RazorPath!.Trim(), Title = (m.Description ?? "").Trim() })
                                      .GroupBy(x => x.Path, StringComparer.OrdinalIgnoreCase).Select(g => g.First()).ToList(),
            };
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

        // ───────────── P3: التقارير ─────────────

        /// <summary>«تقاريرى»: المحفوظة، المثبّتة أولاً.</summary>
        public List<AiSavedReportView> SavedReports { get; private set; } = new();

        public async Task RefreshSavedReportsAsync()
        {
            if (!HasAccess || _gw == null) return;
            var gw = _gw; long uid = UserId;
            try { SavedReports = await Bg(() => gw.GetSavedReports(uid)); }
            catch (Exception ex) { Error = Err(ex); }
            Notify();
        }

        /// <summary>التقرير للعرض؛ يرمى InvalidOperationException برسالة عربية.</summary>
        public async Task<AiReportDoc> GetReportAsync(long reportId)
        {
            if (_gw == null || !HasAccess) throw new InvalidOperationException("المساعد غير متاح.");
            var gw = _gw; long uid = UserId;
            return await Bg(() => gw.GetReport(reportId, uid)) ?? throw new InvalidOperationException("التقرير غير موجود.");
        }

        public async Task SaveReportAsync(long reportId, string title, bool pinned)
        {
            if (_gw == null) return;
            var gw = _gw; long uid = UserId;
            await Bg(() => gw.SaveReport(reportId, title, pinned, uid));
            await RefreshSavedReportsAsync();
        }

        public async Task UnsaveReportAsync(long reportId)
        {
            if (_gw == null) return;
            var gw = _gw; long uid = UserId;
            await Bg(() => gw.UnsaveReport(reportId, uid));
            await RefreshSavedReportsAsync();
        }

        /// <summary>إعادة التشغيل ببيانات اليوم وبفترة جديدة اختيارياً (الصلاحيات الحالية تُطبَّق).</summary>
        public async Task<AiReportDoc> RerunReportAsync(long reportId, DateTime? from, DateTime? to)
        {
            if (_gw == null || !HasAccess) throw new InvalidOperationException("المساعد غير متاح.");
            var gw = _gw; var user = BuildUser();
            var doc = await Task.Run(async () =>
            {
                AccountCultureGuard.EnsureInvariant();
                return await gw.RerunReportAsync(reportId, from, to, user);
            });
            await RefreshSavedReportsAsync();
            return doc;
        }

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
            SavedReports = new();
            PendingAttachments = new();
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
