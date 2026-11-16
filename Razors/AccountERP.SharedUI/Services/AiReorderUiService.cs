using AccountModelsLib;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace AccountERP.SharedUI.Services
{
    /// <summary>
    /// P8.6.3 (ai-assistant-p86-plan.md §2): إعادة الطلب الذكية فى الواجهة (Scoped). غلاف رقيق فوق IAiReorderGateway الذى يسجّله مضيف الويب فقط
    /// (MAUI: HasGateway = false فلا يظهر التبويب).
    /// </summary>
    public sealed class AiReorderUiService
    {
        private readonly LanguageService _lang;
        private readonly IStringLocalizer _loc;
        private readonly AssistantSessionService? _chat;
        private readonly IAiReorderGateway? _gw;

        public AiReorderUiService(IServiceProvider sp, LanguageService lang, IStringLocalizer<SharedUIAssemblyMarker> loc)
        {
            _lang = lang;
            _loc = loc;
            _chat = sp.GetService<AssistantSessionService>();
            _gw = sp.GetService<IAiReorderGateway>();
        }

        public bool HasGateway => _gw != null && _chat != null;
        public bool CanManage => HasGateway && _chat!.UserId > 0 && (_chat.BuildUser().IsAdmin || _chat.CanOpenSettings);
        public string Language => AiBriefConst.NormalizeLanguage(_lang.CurrentLanguage);

        private static Task<T> Bg<T>(Func<T> f) => Task.Run(() => { AccountCultureGuard.EnsureInvariant(); return f(); });
        private IAiReorderGateway Gw => _gw ?? throw new InvalidOperationException("إعادة الطلب الذكية غير متاحة.");
        private AiUserContext User() => _chat?.BuildUser() ?? throw new InvalidOperationException("إعادة الطلب الذكية غير متاحة.");

        public bool CanView() { try { return HasGateway && _chat!.UserId > 0 && Gw.CanView(User()); } catch { return false; } }

        public Task<AiReorderPage> ListAsync(AiReorderFilter f) { var gw = Gw; var u = User(); return Bg(() => gw.List(f, u)); }
        public Task<bool> DismissAsync(long id) { var gw = Gw; var u = User(); return Bg(() => gw.Dismiss(id, u)); }
        public Task<AiReorderDraftResult> CreateDraftsAsync(IList<long> ids) { var gw = Gw; var u = User(); return Bg(() => gw.CreateDrafts(ids, u)); }

        public Task<string> ExplainAsync(long id)
        {
            var gw = Gw; var u = User(); var lang = Language;
            return Task.Run(async () => { AccountCultureGuard.EnsureInvariant(); return await gw.ExplainAsync(id, lang, u, CancellationToken.None); });
        }

        public Task<AiReorderRunRow> RunNowAsync()
        {
            var gw = Gw; var u = User();
            return Task.Run(async () => { AccountCultureGuard.EnsureInvariant(); return await gw.RunNowAsync(u, CancellationToken.None); });
        }

        public Task<AiReorderSettings> GetSettingsAsync() { var gw = Gw; var u = User(); return Bg(() => gw.GetSettings(u)); }
        public Task SaveSettingsAsync(AiReorderSettings s) { var gw = Gw; var u = User(); return Bg(() => { gw.SaveSettings(s, u); return true; }); }
        public Task<List<(long SupplierId, string Name, int LeadDays)>> SupplierLeadsAsync() { var gw = Gw; var u = User(); return Bg(() => gw.SupplierLeads(u)); }
        public Task SetSupplierLeadAsync(long id, int? days) { var gw = Gw; var u = User(); return Bg(() => { gw.SetSupplierLead(id, days, u); return true; }); }

        public string ErrorText(Exception ex)
        {
            string key = ex is AiReorderException re && !string.IsNullOrEmpty(re.Code) ? re.Code : "AIQ_ErrGeneric";
            string t = AiBriefText.T(_loc, _lang.CurrentCultureInfo, key);
            return string.IsNullOrEmpty(t) || t == key ? ex.Message : t;
        }
    }
}
