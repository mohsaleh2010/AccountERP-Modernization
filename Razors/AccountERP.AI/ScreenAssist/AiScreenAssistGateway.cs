using AccountERP.AI.Providers;
using AccountERP.AI.SignIn;
using AccountModelsLib;

namespace AccountERP.AI.ScreenAssist
{
    /// <summary>
    /// P8.9.1 — بوابة المساعد داخل الشاشات (Scoped، ويب فقط — ص27). الصلاحية = المساعد (menuItemAiAssistant) + مسار الشاشة (ص6)،
    /// تُبنى على الخادم بـ AiUserContextFactory.ForUser. الحقائق من AiScreenFactsBuilder؛ الفقرة من AiScreenExplainer (P8.9.2).
    /// الإعدادات لمدير النظام أو مالك /settings/ai. الأخطاء AiScreenAssistException بمفتاح resx.
    /// </summary>
    public sealed class AiScreenAssistGateway : IAiScreenAssistGateway
    {
        private readonly AiScreenFactsBuilder _builder;
        private readonly AiScreenExplainer? _explainer;
        private readonly AiProvidersCLS _providers;
        private readonly Func<long, AiServerUser> _users;
        private readonly AiToolAuditCLS _audit = new();

        public Action<string, string, long>? SettingWriter { get; set; }
        public Func<IReadOnlyDictionary<string, string>>? SettingsSource { get; set; }
        public Func<DateTime> Today { get; set; } = () => DateTime.Today;
        /// <summary>اختبار: عدّاد الاستعمال اليومى بدل AI_ToolCalls_Audit.</summary>
        public Func<int>? UsedTodaySource { get; set; }

        public AiScreenAssistGateway(AiScreenFactsBuilder builder, ProviderRegistry registry, AiScreenExplainer? explainer = null)
            : this(builder, explainer, null, null) { }

        public AiScreenAssistGateway(AiScreenFactsBuilder builder, AiScreenExplainer? explainer, AiProvidersCLS? providers, Func<long, AiServerUser>? users)
        {
            _builder = builder;
            _explainer = explainer;
            _providers = providers ?? new AiProvidersCLS();
            _users = users ?? AiUserContextFactory.ForUser;
        }

        public IReadOnlyDictionary<string, string> RawSettings() => SettingsSource?.Invoke() ?? _providers.GetSettings();
        public AiScreenAssistSettings Settings() => AiScreenAssistSettings.From(RawSettings());

        // ───────────── الاستعمال ─────────────

        public AiScreenAssistState State(AiUserContext user)
        {
            var st = new AiScreenAssistState();
            if (user == null || user.UserId <= 0) return st;
            var s = Settings();
            st.Narrative = s.Narrative && s.MaxPerDay > 0;
            st.Ambient = s.Ambient;
            if (!s.Enabled) return st;
            var server = _users(user.UserId);
            if (!HasAssistant(server)) return st;
            st.Kinds = AiScreenKinds.All.Where(k => AiScreenAssistRules.RoutesOf(k).Any(server.HasRoute)).ToList();
            st.Enabled = st.Kinds.Count > 0;
            return st;
        }

        public AiScreenFacts Build(AiScreenPayload payload, AiUserContext user)
        {
            var (server, p) = EnsureUse(payload, user);
            return _builder.Build(p, server, RawSettings(), CancellationToken.None).ForClient();
        }

        /// <summary>للمحادثة (P8.9.4): الحقائق الكاملة بما فيها ChatJson. نفس التحقق.</summary>
        public AiScreenFacts BuildServer(AiScreenPayload payload, long userId)
        {
            var (server, p) = EnsureUse(payload, new AiUserContext { UserId = userId });
            return _builder.Build(p, server, RawSettings(), CancellationToken.None);
        }

        public async Task<AiScreenNarrative> ExplainAsync(AiScreenPayload payload, string factsHash, string language, AiUserContext user, CancellationToken ct)
        {
            var (server, p) = EnsureUse(payload, user);
            var s = Settings();
            if (!s.Narrative || _explainer == null) return new AiScreenNarrative { Source = AiBriefNarrativeSource.Off, FactsHash = factsHash ?? "" };
            var facts = _builder.Cached(server.UserId, factsHash ?? "");
            bool rebuilt = false;
            if (facts == null) { facts = _builder.Build(p, server, RawSettings(), ct); rebuilt = true; }
            bool minimize = (RawSettings().TryGetValue(AiSettingKeys.DataMinimization, out var dm) ? dm : "1") != "0";
            var n = await _explainer.ExplainAsync(facts, language, minimize, s.MaxPerDay, server.UserId, ct).ConfigureAwait(false);
            if (rebuilt || facts.Hash != factsHash) n.Facts = facts.ForClient();
            return n;
        }

        /// <summary>الحمولة منظَّفة + مستخدم صالح يملك المساعد ومسار النوع. يرمى AiScreenAssistException.</summary>
        public (AiServerUser Server, AiScreenPayload Payload) EnsureUse(AiScreenPayload payload, AiUserContext user)
        {
            if (!Settings().Enabled) throw new AiScreenAssistException(AiScreenAssistErrors.Off, "المساعد داخل الشاشات موقوف من الإعدادات.");
            if (user == null || user.UserId <= 0) throw new AiScreenAssistException(AiScreenAssistErrors.NoPermission, "لا يوجد مستخدم.");
            var p = AiScreenAssistRules.Normalize(payload);
            var server = _users(user.UserId);
            if (!HasAssistant(server)) throw new AiScreenAssistException(AiScreenAssistErrors.NoPermission, "لا تملك صلاحية المساعد.");
            if (!AiScreenAssistRules.RoutesOf(p.Kind).Any(server.HasRoute))
                throw new AiScreenAssistException(AiScreenAssistErrors.NoPermission, "لا تملك صلاحية هذه الشاشة.");
            return (server, p);
        }

        /// <summary>السياق المحيط (ص22): يُعاد المنظَّف أو null بصمت (موقوف، بلا صلاحية، حمولة غير صالحة).</summary>
        public AiScreenPayload? AmbientFor(AiScreenPayload? payload, long userId)
        {
            if (payload == null) return null;
            try
            {
                var s = Settings();
                if (!s.Enabled || !s.Ambient) return null;
                var (_, p) = EnsureUse(payload, new AiUserContext { UserId = userId });
                return p;
            }
            catch (AiScreenAssistException) { return null; }
        }

        // ───────────── الإدارة ─────────────

        public AiScreenAssistSettings GetSettings(AiUserContext admin)
        {
            EnsureAdmin(admin);
            var s = Settings();
            s.UsedToday = UsedToday();
            return s;
        }

        public void SaveSettings(AiScreenAssistSettings settings, AiUserContext admin)
        {
            long uid = EnsureAdmin(admin);
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            var errors = settings.Validate();
            if (errors.Count > 0) throw new AiScreenAssistException(errors[0], "إعدادات غير صالحة.");
            var write = SettingWriter ?? ((k, v, by) => _providers.SetSetting(k, v, by));
            foreach (var p in settings.ToPairs()) write(p.Key, p.Value, uid);
            _explainer?.ClearCache();
        }

        public int UsedToday()
        {
            if (UsedTodaySource != null) return UsedTodaySource();
            try { return _audit.CountSince(AiScreenAssistConst.ExplainAuditTool, AiToolOutcome.Ok, Today().Date); }
            catch (Exception ex) { try { operations.DataDictionaryLogerException(ex); } catch { } return 0; }
        }

        // ───────────── مساعدات ─────────────

        private static bool HasAssistant(AiServerUser s) =>
            s.Usable && (s.User.IsAdmin || s.HasMenuKey(AiScreenAssistConst.MenuKey));

        private static bool IsAdmin(AiServerUser s) =>
            s.Usable && (s.User.IsAdmin || s.HasMenuKey(AiSignInService.MenuKey) || s.HasRoute(AiScreenAssistConst.SettingsRoute));

        private long EnsureAdmin(AiUserContext user)
        {
            long uid = user != null && user.UserId > 0 ? user.UserId : throw new AiScreenAssistException(AiScreenAssistErrors.NotAdmin, "لا يوجد مستخدم.");
            if (!IsAdmin(_users(uid))) throw new AiScreenAssistException(AiScreenAssistErrors.NotAdmin, "إعدادات المساعد داخل الشاشات للمدير فقط.");
            return uid;
        }
    }
}
