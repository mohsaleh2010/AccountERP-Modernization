using System.Globalization;
using AccountModelsLib;
using AccountModelsLib.BusinessLibShared.GeneralLedger;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace AccountERP.SharedUI.Services
{
    /// <summary>
    /// P8.5.2: أولويات التحصيل فى الواجهة (Scoped لكل دائرة). واجهة رقيقة فوق IAiCollectionsGateway (يسجّله مضيف الويب فقط — ت33؛
    /// بدونه HasGateway = false فتعرض الصفحة «غير متاح»). كل الاستدعاءات خارج خيط الرسم، والصلاحيات تتحقق منها البوابة.
    /// </summary>
    public sealed class AiCollectService
    {
        private readonly LanguageService _lang;
        private readonly IStringLocalizer _loc;
        private readonly AssistantSessionService? _chat;
        private readonly IAiCollectionsGateway? _gw;

        public AiCollectService(IServiceProvider sp, LanguageService lang, IStringLocalizer<SharedUIAssemblyMarker> loc)
        {
            _lang = lang;
            _loc = loc;
            _chat = sp.GetService<AssistantSessionService>();
            _gw = sp.GetService<IAiCollectionsGateway>();
        }

        public bool HasGateway => _gw != null && _chat != null;
        public bool IsAvailable => HasGateway && _chat!.UserId > 0 && (_chat.BuildUser().IsAdmin || HasRoute(AiCollectConst.Route));
        public bool CanManage => HasGateway && _chat!.UserId > 0 && (_chat.BuildUser().IsAdmin || _chat.CanOpenSettings);
        public bool CanSeeInvoices => HasGateway && (_chat!.BuildUser().IsAdmin || HasRoute(AiCollectConst.SalesRoute));
        public bool CanSeeBalances => HasGateway && (_chat!.BuildUser().IsAdmin || HasRoute(AiCollectConst.BalancesRoute));
        public string Language => AiBriefConst.NormalizeLanguage(_lang.CurrentLanguage);

        private bool HasRoute(string route) =>
            _chat!.BuildUser().AllowedRoutes.Any(r => string.Equals((r ?? "").Split('?')[0].TrimEnd('/'), route, StringComparison.OrdinalIgnoreCase));

        private static Task<T> Bg<T>(Func<T> f) => Task.Run(() => { AccountCultureGuard.EnsureInvariant(); return f(); });
        private static Task Bg(Action f) => Task.Run(() => { AccountCultureGuard.EnsureInvariant(); f(); });
        private IAiCollectionsGateway Gw => _gw ?? throw new InvalidOperationException("أولويات التحصيل غير متاحة.");
        private AiUserContext User() => _chat?.BuildUser() ?? throw new InvalidOperationException("أولويات التحصيل غير متاحة.");

        public Task<AiCollectSummary> SummaryAsync() { var gw = Gw; var u = User(); return Bg(() => gw.Summary(u)); }
        public Task<AiCollectPage> ListAsync(AiCollectFilter f, bool refresh) { var gw = Gw; var u = User(); return Bg(() => gw.List(f, refresh, u)); }
        public Task<AiCollectDetail> GetCustomerAsync(long id) { var gw = Gw; var u = User(); return Bg(() => gw.GetCustomer(id, u)); }
        public Task<long> AddActionAsync(AiCollectActionInput i) { var gw = Gw; var u = User(); return Bg(() => gw.AddAction(i, u)); }
        public Task CloseActionAsync(long id, bool cancel) { var gw = Gw; var u = User(); return Bg(() => gw.CloseAction(id, cancel, u)); }
        public Task<long> LogCopiedAsync(long id, string text, string source, string lang) { var gw = Gw; var u = User(); return Bg(() => gw.LogCopied(id, text, source, lang, u)); }
        public Task<AiCollectSendPreview> PreviewSendAsync(long id, int channel) { var gw = Gw; var u = User(); return Bg(() => gw.PreviewSend(id, channel, u)); }
        public Task<AiCollectSendResult> SendAsync(long id, int channel, string text, string source, string lang) { var gw = Gw; var u = User(); return Bg(() => gw.Send(id, channel, text, source, lang, true, u)); }
        public Task<AiCollectSettings> GetSettingsAsync() { var gw = Gw; var u = User(); return Bg(() => gw.GetSettings(u)); }
        public Task SaveSettingsAsync(AiCollectSettings s) { var gw = Gw; var u = User(); return Bg(() => gw.SaveSettings(s, u)); }

        public Task<AiCollectDraft> DraftAsync(long id, string lang, bool useModel, bool regenerate)
        {
            var gw = Gw; var u = User();
            return Task.Run(async () => { AccountCultureGuard.EnsureInvariant(); return await gw.DraftAsync(id, lang, useModel, regenerate, u, CancellationToken.None); });
        }

        public Task<List<(long Id, string Name)>> BranchesAsync() =>
            Bg(() => new FinalAccountsService().GetBranches().Select(b => (b.Id, b.Name)).ToList());

        /// <summary>هل مزوّد إشعارات العملاء مفعّل (لتبويب الإعدادات).</summary>
        public Task<bool> NotifyProviderOnAsync() => Bg(() =>
        {
            try { return new AccountModelsLib.SalesRep.SalesRepNotifyService().GetSettings().Provider != 0; } catch { return false; }
        });

        public string ErrorText(Exception ex) => AiCollectText.Error(_loc, _lang.CurrentCultureInfo, ex);
    }

    /// <summary>نصوص أولويات التحصيل: الخطأ، الأولوية والحالة (لون + أيقونة + نص)، جمل «لماذا»، والقالب الثابت (ت21).</summary>
    public static class AiCollectText
    {
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        public static string T(IStringLocalizer loc, CultureInfo c, string key, params object[] args) => AiBriefText.T(loc, c, key, args);

        public static string Error(IStringLocalizer loc, CultureInfo c, Exception ex)
        {
            if (ex is AiCollectException a && a.Code.Length > 0)
            {
                var s = T(loc, c, a.Code);
                if (s != a.Code) return s;
            }
            return AiBriefText.Error(loc, c, ex);
        }

        public static string Money(decimal v) => AiBriefText.Money(v);
        public static string Day(DateTime? d) => d.HasValue ? d.Value.ToString("yyyy-MM-dd", Inv) : "—";

        public static string PriorityKey(string p) => p == AiCollectPriority.High ? "AIL_PrHigh" : p == AiCollectPriority.Medium ? "AIL_PrMedium" : "AIL_PrLow";
        public static string StateKey(string s) => s switch
        {
            AiCollectState.Promised => "AIL_StPromised", AiCollectState.Broken => "AIL_StBroken", AiCollectState.Snoozed => "AIL_StSnoozed",
            AiCollectState.Disputed => "AIL_StDisputed", _ => "AIL_StDue",
        };
        public static string LevelKey(int level) => level >= 3 ? "AIL_Level3" : level == 2 ? "AIL_Level2" : "AIL_Level1";

        public static string PriorityIcon(string p) => p switch
        {
            AiCollectPriority.High => MudBlazor.Icons.Material.Filled.PriorityHigh,
            AiCollectPriority.Medium => MudBlazor.Icons.Material.Filled.Warning,
            _ => MudBlazor.Icons.Material.Filled.LowPriority,
        };

        public static MudBlazor.Color PriorityColor(string p) => p switch
        {
            AiCollectPriority.High => MudBlazor.Color.Error,
            AiCollectPriority.Medium => MudBlazor.Color.Warning,
            _ => MudBlazor.Color.Info,
        };

        public static string StateIcon(string s) => s switch
        {
            AiCollectState.Promised => MudBlazor.Icons.Material.Filled.Handshake,
            AiCollectState.Broken => MudBlazor.Icons.Material.Filled.HeartBroken,
            AiCollectState.Snoozed => MudBlazor.Icons.Material.Filled.Snooze,
            AiCollectState.Disputed => MudBlazor.Icons.Material.Filled.Gavel,
            _ => MudBlazor.Icons.Material.Filled.PendingActions,
        };

        /// <summary>جمل «لماذا هذه الدرجة» (ت12): لكل عامل نقاطه من وزنه.</summary>
        public static List<string> Why(IStringLocalizer loc, CultureInfo c, AiCollectRow r, AiCollectSettings s)
        {
            var w = s?.Weights is { Length: 5 } ww ? ww : new decimal[] { 35, 25, 15, 15, 10 };
            decimal sum = w.Sum() <= 0 ? 100 : w.Sum();
            string Max(int i) => Math.Round(w[i] * 100 / sum, 1).ToString("0.#", Inv);
            string Pts(int i) => (r.Parts != null && r.Parts.Length > i ? r.Parts[i] : 0).ToString("0.#", Inv);
            string R(string key, params (string K, string V)[] vals)
            {
                string t = T(loc, c, key);
                foreach (var (k, v) in vals) t = t.Replace("{" + k + "}", v);
                return t;
            }
            var list = new List<string>
            {
                R("AIL_WhyAge", ("days", r.OldestDays.ToString(Inv)), ("pts", Pts(0)), ("max", Max(0))),
                R("AIL_WhyAmount", ("amount", Money(r.Overdue)), ("pts", Pts(1)), ("max", Max(1))),
                r.Dso.HasValue ? R("AIL_WhyDso", ("days", r.Dso.Value.ToString(Inv)), ("pts", Pts(2)), ("max", Max(2))) : R("AIL_WhyDsoNone", ("pts", Pts(2)), ("max", Max(2))),
                r.CreditLimit.HasValue ? R("AIL_WhyLimit", ("balance", Money(r.Balance)), ("limit", Money(r.CreditLimit.Value)), ("pts", Pts(3)), ("max", Max(3)))
                                       : R("AIL_WhyLimitNone", ("max", Max(3))),
                r.IdleDays.HasValue ? R("AIL_WhyIdle", ("days", r.IdleDays.Value.ToString(Inv)), ("pts", Pts(4)), ("max", Max(4))) : R("AIL_WhyIdleNever", ("pts", Pts(4)), ("max", Max(4))),
            };
            if (r.BrokenBonus) list.Add(R("AIL_WhyBroken", ("pts", (s?.BrokenPromiseBonus ?? 10).ToString(Inv))));
            return list;
        }

        /// <summary>القالب الثابت بلغة الرسالة (ت21): {customer}، {amount}، {days}.</summary>
        public static string Template(IStringLocalizer loc, string language, AiCollectRow r)
        {
            var culture = CultureInfo.GetCultureInfo(AiBriefConst.NormalizeLanguage(language));
            string t = T(loc, culture, "AIL_Tpl_L" + Math.Max(1, Math.Min(3, r.Level)).ToString(Inv));
            return t.Replace("{customer}", r.Name ?? "").Replace("{amount}", Money(r.Overdue > 0 ? r.Overdue : r.Balance)).Replace("{days}", r.OldestDays.ToString(Inv));
        }
    }
}
