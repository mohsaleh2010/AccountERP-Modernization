using System.Globalization;
using AccountModelsLib;
using AccountModelsLib.BusinessLibShared.GeneralLedger;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace AccountERP.SharedUI.Services
{
    /// <summary>
    /// P8.1.5: الموجز اليومى فى الواجهة (Scoped لكل دائرة). واجهة رقيقة فوق IAiBriefGateway (يسجّله مضيف الويب فقط — ق28؛
    /// بدونه IsAvailable = false فلا تظهر الصفحة ولا البطاقة ولا الإشعار)، وصندوق وارد يحوّل إشارة AiBriefSignal (خيط خلفى)
    /// إلى حدث Arrived؛ المكوّن AiBriefInbox ينقله لسياق الدائرة ثم يستدعى CatchUpAsync الذى يضيف الإشعار.
    /// كل الاستدعاءات خارج خيط الرسم بـ Task.Run، والصلاحيات تتحقق منها البوابة على الخادم.
    /// </summary>
    public sealed class AiBriefService : IDisposable
    {
        private readonly NotificationCenterService _nc;
        private readonly LanguageService _lang;
        private readonly MenuStateService _menu;
        private readonly IStringLocalizer _loc;
        private readonly AssistantSessionService? _chat;
        private readonly IAiBriefGateway? _gw;
        private bool _subscribed;
        private long _uid;

        /// <summary>وصل موجز جديد لهذا المستخدم (يُرفع من خيط خلفى؛ المشترك يستدعى InvokeAsync).</summary>
        public event Action? Arrived;

        public AiBriefService(IServiceProvider sp, NotificationCenterService nc, LanguageService lang, MenuStateService menu,
                              IStringLocalizer<SharedUIAssemblyMarker> loc)
        {
            _nc = nc;
            _lang = lang;
            _menu = menu;
            _loc = loc;
            _chat = sp.GetService<AssistantSessionService>();
            _gw = sp.GetService<IAiBriefGateway>();
            _menu.OnAuthChanged += OnAuthChanged;
        }

        /// <summary>البوابة مسجّلة + مستخدم داخل + يملك شاشة «الموجز اليومى».</summary>
        public bool IsAvailable => _gw != null && _chat != null && _chat.UserId > 0 && HasRoute(AiBriefConst.Route);

        /// <summary>مدير الموجز (P8.1.6): مدير النظام أو من يملك «إعدادات الذكاء الاصطناعى». البوابة تتحقق مرة أخرى.</summary>
        public bool CanManage => _gw != null && _chat != null && _chat.UserId > 0 && (_chat.BuildUser().IsAdmin || _chat.CanOpenSettings);

        public long UserId => _chat?.UserId ?? 0;

        /// <summary>لغة الموجز = لغة الواجهة الحالية (ar-EG / en-US / de-DE / fr-FR).</summary>
        public string Language => AiBriefConst.NormalizeLanguage(_lang.CurrentLanguage);

        private bool HasRoute(string route) =>
            _chat!.BuildUser().AllowedRoutes.Any(r => string.Equals(r, route, StringComparison.OrdinalIgnoreCase));

        public string ErrorText(Exception ex) => AiBriefText.Error(_loc, _lang.CurrentCultureInfo, ex);

        private static Task<T> Bg<T>(Func<T> f) => Task.Run(() => { AccountCultureGuard.EnsureInvariant(); return f(); });
        private static Task Bg(Action f) => Task.Run(() => { AccountCultureGuard.EnsureInvariant(); f(); });

        private IAiBriefGateway Gw => _gw ?? throw new InvalidOperationException("الموجز اليومى غير متاح.");
        private AiUserContext User() => _chat?.BuildUser() ?? throw new InvalidOperationException("الموجز اليومى غير متاح.");

        // ───────────── الصفحة والبطاقة ─────────────

        /// <summary>موجز بعينه (يشترط صف تسليم للمستخدم) أو أحدث موجز إن كان briefId = 0. null = لا يوجد.</summary>
        public async Task<AiBriefDetail?> LoadAsync(long briefId, string language)
        {
            var gw = Gw; var user = User();
            return await Bg(() => briefId > 0 ? gw.GetById(briefId, user, language) : gw.GetLatest(user, language));
        }

        public async Task<List<AiBriefDeliveryRow>> HistoryAsync()
        {
            var gw = Gw; var user = User();
            return await Bg(() => gw.ListHistory(user, AiBriefConst.HistoryDays));
        }

        public async Task MarkReadAsync(long briefId)
        {
            var gw = Gw; long uid = UserId;
            await Bg(() => gw.MarkRead(briefId, uid));
        }

        public async Task DismissHomeAsync(long briefId)
        {
            var gw = Gw; long uid = UserId;
            await Bg(() => gw.DismissHome(briefId, uid));
        }

        public async Task<AiBriefSubscriptionRow> GetSubscriptionAsync()
        {
            var gw = Gw; var user = User();
            return await Bg(() => gw.GetSubscription(user));
        }

        public async Task SaveSubscriptionAsync(AiBriefSubscriptionRow row)
        {
            var gw = Gw; var user = User();
            await Bg(() => gw.SaveSubscription(row, user));
        }

        /// <summary>فروع المستخدم المسموحة (نفس مصدر البوابة) لاختيار فروع الاشتراك.</summary>
        public async Task<List<(long Id, string Name)>> BranchesAsync() =>
            await Bg(() => new FinalAccountsService().GetBranches().Select(b => (b.Id, b.Name)).ToList());

        /// <summary>«تحديث الآن»: يعيد جمع موجز اليوم (مجاناً)؛ السرد للمدير مرة فى الساعة (ق25).</summary>
        public async Task<AiBriefDetail> RefreshAsync(string language)
        {
            var gw = Gw; var user = User();
            return await Task.Run(async () =>
            {
                AccountCultureGuard.EnsureInvariant();
                return await gw.RefreshNowAsync(user, language, CancellationToken.None);
            });
        }

        /// <summary>«اسأل المساعد» (ق22): محادثة أول رسالة فيها بطاقة الموجز، وتُفتح فى جلسة المساعد ليعرضها /assistant.</summary>
        public async Task<long> AskAsync(long briefId)
        {
            var gw = Gw; var user = User();
            long convId = await Bg(() => gw.StartConversation(briefId, user));
            if (_chat != null)
            {
                await _chat.RefreshAsync();
                await _chat.OpenAsync(convId);
            }
            return convId;
        }

        // ───────────── الإعدادات (P8.1.6) ─────────────

        public async Task<AiBriefSettings> GetSettingsAsync()
        {
            var gw = Gw; var user = User();
            return await Bg(() => gw.GetSettings(user));
        }

        public async Task SaveSettingsAsync(AiBriefSettings s)
        {
            var gw = Gw; var user = User();
            await Bg(() => gw.SaveSettings(s, user));
        }

        public async Task<List<AiBriefSubscriptionRow>> SubscribersAsync()
        {
            var gw = Gw; var user = User();
            return await Bg(() => gw.ListSubscribers(user));
        }

        public async Task SetSubscriberAsync(long userId, bool enabled)
        {
            var gw = Gw; var user = User();
            await Bg(() => gw.SetSubscriber(userId, enabled, user));
        }

        /// <summary>المستخدمون (قراءة فقط) لاختيار مشترك جديد؛ التحقق من صلاحية القائمة يتم عند الإضافة على الخادم.</summary>
        public async Task<List<UserAdminRow>> UsersAsync() =>
            CanManage ? await Bg(() => new UserAdminService().GetUsers()) : new List<UserAdminRow>();

        public async Task<List<AiBriefRunView>> RunsAsync()
        {
            var gw = Gw; var user = User();
            return await Bg(() => gw.ListRuns(user, 30));
        }

        public async Task<AiBriefDetail> RunNowForMeAsync(string language)
        {
            var gw = Gw; var user = User();
            return await Task.Run(async () =>
            {
                AccountCultureGuard.EnsureInvariant();
                return await gw.RunNowForMeAsync(user, language, CancellationToken.None);
            });
        }

        // ───────────── صندوق الوارد (ق2) ─────────────

        /// <summary>يشترك فى الإشارة مرة واحدة إن كان للمستخدم وصول. true = مشترك.</summary>
        public bool TryStart()
        {
            if (_subscribed) return true;
            if (!IsAvailable) return false;
            _uid = _chat!.UserId;
            AiBriefSignal.Raised += OnSignal;
            _subscribed = true;
            return true;
        }

        private void Stop()
        {
            if (!_subscribed) return;
            AiBriefSignal.Raised -= OnSignal;
            _subscribed = false;
            _uid = 0;
        }

        private void OnSignal(long userId, long deliveryId)
        {
            if (_subscribed && userId == _uid) Arrived?.Invoke();
        }

        /// <summary>دخول/خروج مستخدم على نفس الدائرة: لا يرث إشارات المستخدم السابق.</summary>
        private void OnAuthChanged()
        {
            if (_subscribed && _uid != (_chat?.UserId ?? 0)) Stop();
        }

        /// <summary>
        /// يضيف إشعاراً لكل موجز غير مقروء فى آخر 7 أيام (المفتاح brief:&lt;id&gt; يمنع التكرار). يُستدعى عند فتح الدائرة
        /// (غير المتصل وقت الإصدار يراه هنا) وعند كل إشارة. يُستدعى فى سياق الدائرة فقط.
        /// </summary>
        public async Task CatchUpAsync()
        {
            if (!_subscribed || _gw == null || _chat == null) return;
            var gw = _gw; var user = _chat.BuildUser();
            List<AiBriefDeliveryRow> rows;
            try { rows = await Bg(() => gw.ListHistory(user, 7)); }
            catch (Exception ex) { operations.DataDictionaryLogerException(ex); return; }
            var culture = _lang.CurrentCultureInfo;
            foreach (var d in rows.Where(d => d.ReadAt == null).OrderBy(d => d.BriefDate).ThenBy(d => d.CreatedAt).TakeLast(5))
            {
                string date = AiBriefText.Day(d.BriefDate);
                string text = d.AlertCount > 0
                    ? AiBriefText.T(_loc, culture, "AIB_NotifyHeadlineAlerts", date, d.AlertCount)
                    : AiBriefText.T(_loc, culture, "AIB_NotifyHeadline", date);
                _nc.Add(text, d.AlertCount > 0 ? Severity.Warning : Severity.Info, "/assistant/brief/" + d.Brief_Id.ToString(CultureInfo.InvariantCulture),
                        "brief:" + d.Brief_Id.ToString(CultureInfo.InvariantCulture));
            }
        }

        public void Dispose()
        {
            _menu.OnAuthChanged -= OnAuthChanged;
            Stop();
        }
    }
}
