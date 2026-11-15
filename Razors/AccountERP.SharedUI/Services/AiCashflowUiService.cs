using AccountModelsLib;
using AccountModelsLib.BusinessLibShared.GeneralLedger;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace AccountERP.SharedUI.Services
{
    /// <summary>
    /// P8.7.3 (ai-assistant-p87-plan.md §2): توقع التدفق النقدى فى الواجهة (Scoped، ويب فقط). غلاف رقيق فوق IAiCashflowGateway.
    /// بدون البوابة HasGateway = false فتعرض الصفحة «غير متاح».
    /// </summary>
    public sealed class AiCashflowUiService
    {
        private readonly LanguageService _lang;
        private readonly IStringLocalizer _loc;
        private readonly AssistantSessionService? _chat;
        private readonly IAiCashflowGateway? _gw;

        public AiCashflowUiService(IServiceProvider sp, LanguageService lang, IStringLocalizer<SharedUIAssemblyMarker> loc)
        {
            _lang = lang;
            _loc = loc;
            _chat = sp.GetService<AssistantSessionService>();
            _gw = sp.GetService<IAiCashflowGateway>();
        }

        public bool HasGateway => _gw != null && _chat != null;
        public bool CanManage => HasGateway && _chat!.UserId > 0 && (_chat.BuildUser().IsAdmin || _chat.CanOpenSettings);
        public string Language => AiBriefConst.NormalizeLanguage(_lang.CurrentLanguage);

        private static Task<T> Bg<T>(Func<T> f) => Task.Run(() => { AccountCultureGuard.EnsureInvariant(); return f(); });
        private IAiCashflowGateway Gw => _gw ?? throw new InvalidOperationException("توقع التدفق النقدى غير متاح.");
        private AiUserContext User() => _chat?.BuildUser() ?? throw new InvalidOperationException("توقع التدفق النقدى غير متاح.");

        public bool CanView() { try { return HasGateway && _chat!.UserId > 0 && Gw.CanView(User()); } catch { return false; } }

        public Task<AiCashflowView> GetAsync(long branchId, bool recompute)
        {
            var gw = Gw; var u = User(); var lang = Language;
            return Task.Run(async () => { AccountCultureGuard.EnsureInvariant(); return await gw.GetAsync(branchId, recompute, lang, u, CancellationToken.None); });
        }

        public Task<long> AddAdjustmentAsync(AiCashflowAdjustmentRow r) { var gw = Gw; var u = User(); return Bg(() => gw.AddAdjustment(r, u)); }
        public Task<bool> CancelAdjustmentAsync(long id) { var gw = Gw; var u = User(); return Bg(() => gw.CancelAdjustment(id, u)); }
        public Task<List<AiCashflowRecurringRow>> RecurringAsync() { var gw = Gw; var u = User(); return Bg(() => gw.Recurring(u)); }
        public Task SetExcludedAsync(long id, bool ex) { var gw = Gw; var u = User(); return Bg(() => { gw.SetExcluded(id, ex, u); return true; }); }
        public Task<AiCashflowSettings> GetSettingsAsync() { var gw = Gw; var u = User(); return Bg(() => gw.GetSettings(u)); }
        public Task SaveSettingsAsync(AiCashflowSettings s) { var gw = Gw; var u = User(); return Bg(() => { gw.SaveSettings(s, u); return true; }); }

        public Task<List<(long Id, string Name)>> BranchesAsync() =>
            Bg(() => new FinalAccountsService().GetBranches().Select(b => (b.Id, b.Name)).ToList());

        public string ErrorText(Exception ex)
        {
            string key = ex is AiCashflowException ce && !string.IsNullOrEmpty(ce.Code) ? ce.Code : "AIW_ErrGeneric";
            string t = AiBriefText.T(_loc, _lang.CurrentCultureInfo, key);
            return string.IsNullOrEmpty(t) || t == key ? ex.Message : t;
        }
    }
}
