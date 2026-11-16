using AccountModelsLib;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace AccountERP.SharedUI.Services
{
    /// <summary>P8.10.3 (ai-assistant-p810-plan.md م12): توكنات MCP فى الواجهة (Scoped، ويب فقط). غلاف رقيق فوق IAiMcpGateway.</summary>
    public sealed class AiMcpUiService
    {
        private readonly LanguageService _lang;
        private readonly IStringLocalizer _loc;
        private readonly AssistantSessionService? _chat;
        private readonly IAiMcpGateway? _gw;

        public AiMcpUiService(IServiceProvider sp, LanguageService lang, IStringLocalizer<SharedUIAssemblyMarker> loc)
        {
            _lang = lang;
            _loc = loc;
            _chat = sp.GetService<AssistantSessionService>();
            _gw = sp.GetService<IAiMcpGateway>();
        }

        public bool HasGateway => _gw != null && _chat != null;
        public bool CanManage => HasGateway && _chat!.UserId > 0 && (_chat.BuildUser().IsAdmin || _chat.CanOpenSettings);
        public bool HasAssistant => HasGateway && _chat!.HasAccess;

        private static Task<T> Bg<T>(Func<T> f) => Task.Run(() => { AccountCultureGuard.EnsureInvariant(); return f(); });
        private IAiMcpGateway Gw => _gw ?? throw new InvalidOperationException("MCP غير متاح.");
        private AiUserContext User() => _chat?.BuildUser() ?? throw new InvalidOperationException("MCP غير متاح.");

        public Task<AiMcpSettings> GetSettingsAsync() { var gw = Gw; var u = User(); return Bg(() => gw.GetSettings(u)); }
        public Task SaveSettingsAsync(AiMcpSettings s) { var gw = Gw; var u = User(); return Bg(() => { gw.SaveSettings(s, u); return true; }); }
        public Task<AiApiTokenCreated> CreateAsync(string name, int days, bool sql) { var gw = Gw; var u = User(); return Bg(() => gw.CreateToken(name, days, sql, u)); }
        public Task<List<AiApiTokenRow>> MineAsync() { var gw = Gw; var u = User(); return Bg(() => gw.MyTokens(u)); }
        public Task<List<AiApiTokenRow>> AllAsync() { var gw = Gw; var u = User(); return Bg(() => gw.AllTokens(u)); }
        public Task<bool> RevokeAsync(long id) { var gw = Gw; var u = User(); return Bg(() => gw.Revoke(id, u)); }

        public string ErrorText(Exception ex)
        {
            string key = ex is AiMcpException me && !string.IsNullOrEmpty(me.Code) ? me.Code : "AIM_ErrGeneric";
            string t = AiBriefText.T(_loc, _lang.CurrentCultureInfo, key);
            return string.IsNullOrEmpty(t) || t == key ? ex.Message : t;
        }
    }
}
