using AccountModelsLib;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace AccountERP.SharedUI.Services
{
    /// <summary>P8.12.4 (ai-assistant-p812-plan.md §2): المطابقة البنكية فى الواجهة (Scoped، ويب فقط). غلاف رقيق فوق IAiBankRecGateway.</summary>
    public sealed class AiBankRecUiService
    {
        private readonly LanguageService _lang;
        private readonly IStringLocalizer _loc;
        private readonly AssistantSessionService? _chat;
        private readonly IAiBankRecGateway? _gw;

        public AiBankRecUiService(IServiceProvider sp, LanguageService lang, IStringLocalizer<SharedUIAssemblyMarker> loc)
        {
            _lang = lang;
            _loc = loc;
            _chat = sp.GetService<AssistantSessionService>();
            _gw = sp.GetService<IAiBankRecGateway>();
        }

        public bool HasGateway => _gw != null && _chat != null;
        public bool CanManage => HasGateway && _chat!.UserId > 0 && (_chat.BuildUser().IsAdmin || _chat.CanOpenSettings);

        private static Task<T> Bg<T>(Func<T> f) => Task.Run(() => { AccountCultureGuard.EnsureInvariant(); return f(); });
        private IAiBankRecGateway Gw => _gw ?? throw new InvalidOperationException("المطابقة البنكية غير متاحة.");
        private AiUserContext User() => _chat?.BuildUser() ?? throw new InvalidOperationException("المطابقة البنكية غير متاحة.");

        public bool CanView() { try { return HasGateway && _chat!.UserId > 0 && Gw.CanView(User()); } catch { return false; } }

        public Task<List<(long Id, string Name)>> BanksAsync() { var gw = Gw; var u = User(); return Bg(() => gw.Banks(u)); }
        public Task<List<string>> HeadersAsync(byte[] f, string n) { var gw = Gw; var u = User(); return Bg(() => gw.ReadHeaders(f, n, u)); }
        public Task<AiBankColumnMap> ProfileAsync(long bank) { var gw = Gw; var u = User(); return Bg(() => gw.GetProfile(bank, u)); }
        public Task<AiBankImportResult> ImportAsync(long bank, byte[] f, string n, AiBankColumnMap m, decimal? o, decimal? c) { var gw = Gw; var u = User(); return Bg(() => gw.Import(bank, f, n, m, o, c, u)); }
        public Task<AiBankImportResult> ExtractAsync(long bank, byte[] f, string n, string mime)
        {
            var gw = Gw; var u = User();
            return Task.Run(async () => { AccountCultureGuard.EnsureInvariant(); return await gw.ImportExtractAsync(bank, f, n, mime, u, CancellationToken.None); });
        }
        public Task<List<AiBankStatementRow>> StatementsAsync(long bank) { var gw = Gw; var u = User(); return Bg(() => gw.Statements(bank, u)); }
        public Task<AiBankView> GetAsync(long id) { var gw = Gw; var u = User(); return Bg(() => gw.Get(id, u)); }
        public Task<int> MatchAsync(long id)
        {
            var gw = Gw; var u = User();
            return Task.Run(async () => { AccountCultureGuard.EnsureInvariant(); return await gw.MatchAsync(id, u, CancellationToken.None); });
        }
        public Task<int> ConfirmAsync(long st, IList<long> ids) { var gw = Gw; var u = User(); return Bg(() => gw.Confirm(st, ids, u)); }
        public Task<bool> RejectAsync(long id) { var gw = Gw; var u = User(); return Bg(() => gw.Reject(id, u)); }
        public Task<long> ManualAsync(long st, IList<int> lines, IList<long> refs) { var gw = Gw; var u = User(); return Bg(() => gw.ManualMatch(st, lines, refs, u)); }
        public Task<long> JournalDraftAsync(long st, int line, long acc) { var gw = Gw; var u = User(); return Bg(() => gw.JournalDraft(st, line, acc, u)); }
        public Task<AiBankSettings> GetSettingsAsync() { var gw = Gw; var u = User(); return Bg(() => gw.GetSettings(u)); }
        public Task SaveSettingsAsync(AiBankSettings s) { var gw = Gw; var u = User(); return Bg(() => { gw.SaveSettings(s, u); return true; }); }

        public string ErrorText(Exception ex)
        {
            string key = ex is AiBankException be && !string.IsNullOrEmpty(be.Code) ? be.Code : "AIN_ErrGeneric";
            string t = AiBriefText.T(_loc, _lang.CurrentCultureInfo, key);
            if (string.IsNullOrEmpty(t) || t == key) return ex.Message;
            return key is "AIN_ErrExtract" or "AIN_ErrNoModel" ? t + " " + ex.Message : t;
        }
    }
}
