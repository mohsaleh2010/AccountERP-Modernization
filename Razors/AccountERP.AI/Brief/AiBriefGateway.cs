using System.Text.Json.Nodes;
using AccountERP.AI.Chat;
using AccountERP.AI.SignIn;
using AccountModelsLib;

namespace AccountERP.AI.Brief
{
    /// <summary>
    /// P8.1.4 — بوابة الموجز اليومى (Scoped، للويب فقط — ق28). كل قراءة لموجز تشترط صف تسليم للمستخدم (§3 الخصوصية).
    /// الصلاحيات تُبنى على الخادم بـ AiUserContextFactory.ForUser (لا تُؤخذ من الواجهة). دوال الإعدادات للمدير:
    /// مدير النظام أو من يملك شاشة «إعدادات الذكاء الاصطناعى» (نفس قاعدة AiFeedbackGateway).
    /// الأخطاء AiBriefException بمفتاح resx فى Code.
    /// </summary>
    public sealed class AiBriefGateway : IAiBriefGateway
    {
        /// <summary>ق25: «تحديث الآن» يعيد السرد للمدير مرة فى الساعة فقط.</summary>
        public static readonly TimeSpan RenarrateEvery = TimeSpan.FromHours(1);

        private readonly AiBriefJob _job;
        private readonly AiBriefCLS _dal;
        private readonly AiProvidersCLS _providers;
        private readonly AiChatCLS _chat;
        private readonly Func<long, AiServerUser> _users;

        public AiBriefGateway(AiBriefJob job) : this(job, null, null, null, null) { }

        public AiBriefGateway(AiBriefJob job, AiBriefCLS? dal, AiProvidersCLS? providers, AiChatCLS? chat, Func<long, AiServerUser>? users)
        {
            _job = job;
            _dal = dal ?? new AiBriefCLS();
            _providers = providers ?? new AiProvidersCLS();
            _chat = chat ?? new AiChatCLS();
            _users = users ?? AiUserContextFactory.ForUser;
        }

        // ───────────── المستخدم ─────────────

        public AiBriefDetail GetLatest(AiUserContext user, string language)
        {
            long uid = Uid(user);
            var last = _dal.ListDeliveries(uid, Today().AddDays(-AiBriefConst.HistoryDays)).FirstOrDefault();
            return last == null ? null! : Detail(last, language);
        }

        public AiBriefDetail GetById(long briefId, AiUserContext user, string language)
        {
            var d = _dal.GetDelivery(briefId, Uid(user));
            return d == null ? null! : Detail(d, language);
        }

        private AiBriefDetail Detail(AiBriefDeliveryRow d, string language)
        {
            var brief = _dal.Get(d.Brief_Id);
            if (brief == null) return null!;
            string lang = AiBriefConst.NormalizeLanguage(string.IsNullOrWhiteSpace(language) ? d.Language : language);
            // سرد اللغة المطلوبة، وإلا سرد لغة التسليم (الواجهة تعرض «السرد بلغة أخرى» أو القالب).
            var n = _dal.GetNarrative(brief.Id, lang) ?? (lang == d.Language ? null : _dal.GetNarrative(brief.Id, d.Language));
            return new AiBriefDetail { Brief = brief, Narrative = n, Delivery = d };
        }

        public List<AiBriefDeliveryRow> ListHistory(AiUserContext user, int days = AiBriefConst.HistoryDays) =>
            _dal.ListDeliveries(Uid(user), Today().AddDays(-Math.Clamp(days, 1, 3650)));

        public int UnreadCount(long userId) => userId > 0 ? _dal.UnreadCount(userId) : 0;

        public void MarkRead(long briefId, long userId) { if (userId > 0) _dal.MarkRead(briefId, userId); }

        public void DismissHome(long briefId, long userId) { if (userId > 0) _dal.DismissHome(briefId, userId); }

        public AiBriefSubscriptionRow GetSubscription(AiUserContext user)
        {
            long uid = Uid(user);
            return _dal.GetSubscription(uid) ?? new AiBriefSubscriptionRow { User_Id = uid, IsEnabled = false, Language = AiBriefConst.Languages[0] };
        }

        /// <summary>اشتراك المستخدم نفسه: يشترط صلاحية قائمة الموجز؛ الفروع تُقصر على المسموحة له والأقسام على المعروفة.</summary>
        public void SaveSubscription(AiBriefSubscriptionRow subscription, AiUserContext user)
        {
            if (subscription == null) throw new ArgumentNullException(nameof(subscription));
            long uid = Uid(user);
            var server = _users(uid);
            if (!HasBriefMenu(server))
                throw new AiBriefException(AiBriefErrors.NoPermission, "لا تملك صلاحية شاشة «الموجز اليومى».");
            var allowedBranches = server.Run(() => new AccountModelsLib.BusinessLibShared.GeneralLedger.FinalAccountsService().GetBranches()).Select(b => b.Id).ToHashSet();
            var row = new AiBriefSubscriptionRow
            {
                User_Id = uid,
                IsEnabled = subscription.IsEnabled,
                BranchIdsCsv = NullIfEmpty(AiBriefCsv.Join(subscription.BranchIds.Where(allowedBranches.Contains))),
                SectionsCsv = NullIfEmpty(AiBriefCsv.Join(subscription.Sections)),
                Language = AiBriefConst.NormalizeLanguage(subscription.Language),
            };
            _dal.SaveSubscription(row, uid);
        }

        /// <summary>
        /// «تحديث الآن»: يعيد جمع موجز اليوم لنطاق المستخدم مجاناً (الصف نفسه)، أو يُصدره إن لم يصدر بعد. السرد يُعاد للمدير مرة فى الساعة فقط (ق25)،
        /// وسرد قديم لم يعد يطابق الأرقام يتحول للقالب.
        /// </summary>
        public async Task<AiBriefDetail> RefreshNowAsync(AiUserContext user, string language, CancellationToken ct)
        {
            long uid = Uid(user);
            var settings = _job.Settings();
            var server = _users(uid);
            bool admin = IsAdmin(server);
            if (!settings.Enabled && !admin) throw new AiBriefException(AiBriefErrors.Disabled, "الموجز اليومى غير مفعّل.");
            var scope = ResolveOrThrow(server, _dal.GetSubscription(uid));
            string lang = AiBriefConst.NormalizeLanguage(language);
            DateTime today = _job.LocalNow(settings).Date;
            List<string>? renarrate = null;
            if (admin)
            {
                var existing = _dal.GetByScope(today, scope.ScopeKey);
                var n = existing == null ? null : _dal.GetNarrative(existing.Id, lang);
                if (n != null && (n.UpdatedAt ?? n.CreatedAt ?? DateTime.MinValue) < DateTime.Now - RenarrateEvery) renarrate = new() { lang };
            }
            var p = await _job.ProduceAsync(scope, new[] { (uid, lang) }, today, AiBriefTrigger.Manual, settings, recollect: true, renarrate, ct);
            if (p.Busy) throw new AiBriefException(AiBriefErrors.Busy, "الموجز قيد التحديث الآن؛ أعد المحاولة بعد قليل.");
            return GetById(p.Brief!.Id, user, lang);
        }

        /// <summary>
        /// «اسأل المساعد» (ق22): محادثة جديدة أول رسالة فيها من المساعد بكتلة brief (تتمدد إلى الحقائق عند بناء أدوار النموذج — P8.1.5).
        /// يعيد المحادثة السابقة لنفس الموجز إن بقيت نشطة.
        /// </summary>
        public long StartConversation(long briefId, AiUserContext user)
        {
            long uid = Uid(user);
            var d = _dal.GetDelivery(briefId, uid) ?? throw new AiBriefException(AiBriefErrors.NotFound, "الموجز غير موجود.");
            if (d.Conversation_Id is long existing && _chat.OwnsConversation(existing, uid) && !_chat.IsArchived(existing, uid)) return existing;
            if (d.BriefStatus is not (AiBriefStatus.Ready or AiBriefStatus.Partial)) throw new AiBriefException(AiBriefErrors.NotFound, "الموجز غير جاهز.");

            string title = "الموجز اليومى " + d.BriefDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
            long convId = _chat.CreateConversation(uid, user.BranchId, title);
            var block = new JsonObject
            {
                ["type"] = "brief",
                ["brief_id"] = briefId.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["brief_date"] = d.BriefDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
                ["period_from"] = d.PeriodFrom.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
                ["period_to"] = d.PeriodTo.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
                ["language"] = d.Language,
            };
            _chat.AddMessage(new AiMessageRow
            {
                Conversation_Id = convId,
                Role = "assistant",
                ContentJson = AiChatGateway.ContentJson("", Array.Empty<AiToolCallView>(), new[] { block }),
                Status = AiMessageStatus.Ok,
            }, uid);
            _dal.SetConversation(briefId, uid, convId);
            return convId;
        }

        // ───────────── المدير ─────────────

        public AiBriefSettings GetSettings(AiUserContext admin)
        {
            EnsureAdmin(admin);
            return _job.Settings();
        }

        /// <summary>تحقق خادمى (P8.1.6): الوقت، المنطقة الزمنية، الأيام، النسب 1–100، المعامل 1–100، التكلفة ≥ 0.</summary>
        public void SaveSettings(AiBriefSettings s, AiUserContext admin)
        {
            long uid = EnsureAdmin(admin);
            Validate(s);
            foreach (var (k, v) in s.ToPairs()) _providers.SetSetting(k, v, uid);
        }

        public static void Validate(AiBriefSettings s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            if (s.Time < TimeSpan.Zero || s.Time >= TimeSpan.FromDays(1) || s.Time.Seconds != 0 || s.Time.Milliseconds != 0)
                throw new AiBriefException(AiBriefErrors.Time, "وقت الموجز غير صالح (HH:mm).");
            if (!string.IsNullOrWhiteSpace(s.TimeZone) && !AiBriefJob.TryZone(s.TimeZone, out _))
                throw new AiBriefException(AiBriefErrors.TimeZone, "المنطقة الزمنية غير معروفة.");
            if (s.WeekDays == null || s.WeekDays.Any(d => d < 0 || d > 6))
                throw new AiBriefException(AiBriefErrors.Days, "أيام الموجز غير صالحة.");
            if (s.SalesDropPct is < 1 or > 100 || s.ReturnsPct is < 1 or > 100)
                throw new AiBriefException(AiBriefErrors.Pct, "النسب يجب أن تكون من 1 إلى 100.");
            if (s.LargePaymentFactor < 1 || s.LargePaymentFactor > 100)
                throw new AiBriefException(AiBriefErrors.Factor, "معامل المدفوعات الكبيرة يجب أن يكون من 1 إلى 100.");
            if (s.MaxCostPerMonth < 0 || s.MaxCostPerMonth > 1_000_000)
                throw new AiBriefException(AiBriefErrors.Cost, "الحد الشهرى للتكلفة غير صالح.");
            if (s.OverdueDays is < 0 or > 3650)
                throw new AiBriefException(AiBriefErrors.OverdueDays, "أيام التأخير غير صالحة.");
        }

        public List<AiBriefSubscriptionRow> ListSubscribers(AiUserContext admin)
        {
            EnsureAdmin(admin);
            return _dal.ListSubscriptions(enabledOnly: false);
        }

        /// <summary>إضافة/إيقاف مشترك. التفعيل يشترط أن يملك المستخدم صلاحية قائمة الموجز وأن يكون مفعّلاً.</summary>
        public void SetSubscriber(long userId, bool enabled, AiUserContext admin)
        {
            long by = EnsureAdmin(admin);
            var sub = _dal.GetSubscription(userId);
            if (enabled)
            {
                var server = _users(userId);
                if (!server.Usable || !HasBriefMenu(server))
                    throw new AiBriefException(AiBriefErrors.NoPermission, "المستخدم لا يملك صلاحية شاشة «الموجز اليومى» أو غير مفعّل.");
            }
            else if (sub == null) return;
            sub ??= new AiBriefSubscriptionRow { User_Id = userId, Language = AiBriefConst.Languages[0] };
            sub.IsEnabled = enabled;
            _dal.SaveSubscription(sub, by);
        }

        /// <summary>«تشغيل الآن لى (تجربة)»: يُصدر موجز اليوم لنطاق المدير ولو كانت الميزة موقوفة أو قبل الموعد، ويعيد جمعه وسرده.</summary>
        public async Task<AiBriefDetail> RunNowForMeAsync(AiUserContext admin, string language, CancellationToken ct)
        {
            long uid = EnsureAdmin(admin);
            var settings = _job.Settings();
            var scope = ResolveOrThrow(_users(uid), _dal.GetSubscription(uid));
            string lang = AiBriefConst.NormalizeLanguage(language);
            var p = await _job.ProduceAsync(scope, new[] { (uid, lang) }, _job.LocalNow(settings).Date, AiBriefTrigger.Test, settings,
                                            recollect: true, new[] { lang }, ct);
            if (p.Busy) throw new AiBriefException(AiBriefErrors.Busy, "الموجز قيد التحديث الآن؛ أعد المحاولة بعد قليل.");
            return GetById(p.Brief!.Id, admin, lang);
        }

        public List<AiBriefRunView> ListRuns(AiUserContext admin, int top = 30)
        {
            EnsureAdmin(admin);
            return _dal.ListRuns(top);
        }

        // ───────────── مساعدات ─────────────

        private DateTime Today() => _job.LocalNow(_job.Settings()).Date;

        private static long Uid(AiUserContext user) =>
            user != null && user.UserId > 0 ? user.UserId : throw new AiBriefException(AiBriefErrors.NoPermission, "لا يوجد مستخدم.");

        private static bool HasBriefMenu(AiServerUser s) => s.Usable && (s.HasRoute(AiBriefConst.Route) || s.HasMenuKey(AiBriefConst.MenuKey));

        private static bool IsAdmin(AiServerUser s) =>
            s.Usable && (s.User.IsAdmin || s.HasMenuKey(AiSignInService.MenuKey) || s.HasRoute("/settings/ai"));

        private long EnsureAdmin(AiUserContext user)
        {
            long uid = Uid(user);
            if (!IsAdmin(_users(uid)))
                throw new AiBriefException(AiBriefErrors.NotAdmin, "إعدادات الموجز متاحة لمن يملك صلاحية شاشة «إعدادات الذكاء الاصطناعى» فقط.");
            return uid;
        }

        private static AiBriefScope ResolveOrThrow(AiServerUser server, AiBriefSubscriptionRow? sub)
        {
            var res = AiBriefScopes.Resolve(server, sub);
            if (res.Scope != null) return res.Scope;
            if (res.SkipReason == AiBriefScopes.Skip.NoMenu)
                throw new AiBriefException(AiBriefErrors.NoPermission, "لا تملك صلاحية شاشة «الموجز اليومى».");
            throw new AiBriefException(AiBriefErrors.Skipped, "لا يوجد ما يُعرض فى الموجز لصلاحياتك (" + res.SkipReason + ").");
        }

        private static string? NullIfEmpty(string? s) => string.IsNullOrWhiteSpace(s) ? null : s;
    }
}
