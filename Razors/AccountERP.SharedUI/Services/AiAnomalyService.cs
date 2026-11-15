using System.Globalization;
using System.Text.Json;
using AccountModelsLib;
using AccountModelsLib.BusinessLibShared.GeneralLedger;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace AccountERP.SharedUI.Services
{
    /// <summary>
    /// P8.4.3: شذوذ القيود فى الواجهة (Scoped لكل دائرة). واجهة رقيقة فوق IAiAnomalyGateway (يسجّله مضيف الويب فقط — ش31؛
    /// بدونه IsAvailable = false فتعرض الصفحة «غير متاح»). كل الاستدعاءات خارج خيط الرسم بـ Task.Run، والصلاحيات تتحقق منها البوابة.
    /// </summary>
    public sealed class AiAnomalyService
    {
        private readonly LanguageService _lang;
        private readonly IStringLocalizer _loc;
        private readonly AssistantSessionService? _chat;
        private readonly IAiAnomalyGateway? _gw;

        public AiAnomalyService(IServiceProvider sp, LanguageService lang, IStringLocalizer<SharedUIAssemblyMarker> loc)
        {
            _lang = lang;
            _loc = loc;
            _chat = sp.GetService<AssistantSessionService>();
            _gw = sp.GetService<IAiAnomalyGateway>();
        }

        /// <summary>البوابة مسجّلة (الويب) — بدونها الصفحة «غير متاح».</summary>
        public bool HasGateway => _gw != null && _chat != null;

        /// <summary>البوابة + مستخدم داخل + يملك شاشة «شذوذ القيود».</summary>
        public bool IsAvailable => HasGateway && _chat!.UserId > 0 && (_chat.BuildUser().IsAdmin || HasRoute(AiAnomalyConst.Route));

        /// <summary>مدير الكشف: مدير النظام أو من يملك «إعدادات الذكاء الاصطناعى». البوابة تتحقق مرة أخرى.</summary>
        public bool CanManage => HasGateway && _chat!.UserId > 0 && (_chat.BuildUser().IsAdmin || _chat.CanOpenSettings);

        public string Language => AiBriefConst.NormalizeLanguage(_lang.CurrentLanguage);

        private bool HasRoute(string route) =>
            _chat!.BuildUser().AllowedRoutes.Any(r => string.Equals(r, route, StringComparison.OrdinalIgnoreCase));

        private static Task<T> Bg<T>(Func<T> f) => Task.Run(() => { AccountCultureGuard.EnsureInvariant(); return f(); });
        private static Task Bg(Action f) => Task.Run(() => { AccountCultureGuard.EnsureInvariant(); f(); });

        private IAiAnomalyGateway Gw => _gw ?? throw new InvalidOperationException("كشف الشذوذ غير متاح.");
        private AiUserContext User() => _chat?.BuildUser() ?? throw new InvalidOperationException("كشف الشذوذ غير متاح.");

        // ───────────── الصفحة ─────────────

        public Task<AiAnomalySummary> SummaryAsync() { var gw = Gw; var u = User(); return Bg(() => gw.Summary(u)); }
        public Task<AiAnomalyPage> ListAsync(AiAnomalyFilter f) { var gw = Gw; var u = User(); return Bg(() => gw.List(f, u)); }
        public Task<AiAnomalyEntryDetail> GetEntryAsync(long entryId) { var gw = Gw; var u = User(); return Bg(() => gw.GetEntry(entryId, u)); }
        public Task SetStatusAsync(IList<long> ids, string status, string note) { var gw = Gw; var u = User(); return Bg(() => gw.SetStatus(ids, status, note, u)); }

        public Task<AiAnomalyExplainRow> ExplainAsync(long entryId, bool regenerate)
        {
            var gw = Gw; var u = User(); string lang = Language;
            return Task.Run(async () => { AccountCultureGuard.EnsureInvariant(); return await gw.ExplainAsync(entryId, lang, regenerate, u, CancellationToken.None); });
        }

        public Task<List<(long Id, string Name)>> BranchesAsync() =>
            Bg(() => new FinalAccountsService().GetBranches().Select(b => (b.Id, b.Name)).ToList());

        // ───────────── الإدارة ─────────────

        public Task<AiAnomalySettings> GetSettingsAsync() { var gw = Gw; var u = User(); return Bg(() => gw.GetSettings(u)); }
        public Task SaveSettingsAsync(AiAnomalySettings s) { var gw = Gw; var u = User(); return Bg(() => gw.SaveSettings(s, u)); }
        public Task<List<AiAnomalyRunRow>> RunsAsync() { var gw = Gw; var u = User(); return Bg(() => gw.ListRuns(u, 30)); }
        public Task<long> QueueBackfillAsync(DateTime from, DateTime to) { var gw = Gw; var u = User(); return Bg(() => gw.QueueBackfill(from, to, u)); }

        public Task<AiAnomalyRunRow> RunNowAsync()
        {
            var gw = Gw; var u = User();
            return Task.Run(async () => { AccountCultureGuard.EnsureInvariant(); return await gw.RunNowAsync(u, CancellationToken.None); });
        }

        public string ErrorText(Exception ex) => AiAnomalyText.Error(_loc, _lang.CurrentCultureInfo, ex);
    }

    /// <summary>نصوص شذوذ القيود: الترجمة، الخطأ، وجملة «لماذا عُلِّم» القالبية من DetailsJson (ش25).</summary>
    public static class AiAnomalyText
    {
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        public static string T(IStringLocalizer loc, CultureInfo culture, string key, params object[] args) => AiBriefText.T(loc, culture, key, args);

        public static string Error(IStringLocalizer loc, CultureInfo c, Exception ex)
        {
            if (ex is AiAnomalyException a && a.Code.Length > 0)
            {
                var s = T(loc, c, a.Code);
                if (s != a.Code) return s;
            }
            return AiBriefText.Error(loc, c, ex);
        }

        public static string Money(decimal v) => AiBriefText.Money(v);
        public static string Day(DateTime? d) => d.HasValue ? d.Value.ToString("yyyy-MM-dd", Inv) : "—";
        public static string Stamp(DateTime? d) => d.HasValue ? d.Value.ToString("yyyy-MM-dd HH:mm", Inv) : "—";

        /// <summary>جملة السبب لقاعدة من أرقام DetailsJson (المبالغ بتنسيق التقارير، الأيام بلغة الواجهة).</summary>
        public static string Why(IStringLocalizer loc, CultureInfo c, AiAnomalyRow r)
        {
            JsonElement d = default;
            try { if (!string.IsNullOrWhiteSpace(r.DetailsJson)) d = JsonDocument.Parse(r.DetailsJson).RootElement; } catch { }
            decimal N(string k) => d.ValueKind == JsonValueKind.Object && d.TryGetProperty(k, out var v) && v.ValueKind == JsonValueKind.Number ? v.GetDecimal() : 0;
            string S(string k) => d.ValueKind == JsonValueKind.Object && d.TryGetProperty(k, out var v) ? (v.ValueKind == JsonValueKind.String ? v.GetString() ?? "" : v.ToString()) : "";
            string acc = string.IsNullOrWhiteSpace(r.AccountName) ? "—" : r.AccountName!;
            switch (r.RuleCode)
            {
                case AiAnomalyRuleCodes.AmountOutlier:
                    return T(loc, c, "AIJ_Why_amount_outlier", Money(N("amount")), acc, N("z").ToString("0.0", Inv), Money(N("mean")), ((int)N("n")).ToString(Inv));
                case AiAnomalyRuleCodes.Duplicate:
                    return T(loc, c, "AIJ_Why_duplicate", S("twin_entry_no"), S("twin_date"), ((int)N("days_apart")).ToString(Inv));
                case AiAnomalyRuleCodes.OffHours:
                    string reason = S("reason");
                    if (reason == AiAnomalyOffReason.Night) return T(loc, c, "AIJ_Why_off_hours_night", S("hour"), S("work_start"), S("work_end"));
                    string dayName = int.TryParse(S("weekday"), out int wd) && wd >= 0 && wd <= 6 ? c.DateTimeFormat.GetDayName((DayOfWeek)wd) : S("date");
                    return T(loc, c, reason == AiAnomalyOffReason.Holiday ? "AIJ_Why_off_hours_holiday" : "AIJ_Why_off_hours_weekend",
                             reason == AiAnomalyOffReason.Holiday ? S("date") : dayName);
                case AiAnomalyRuleCodes.RareAccount:
                    return T(loc, c, "AIJ_Why_rare_account", acc, ((int)N("uses")).ToString(Inv), ((int)N("window_days")).ToString(Inv));
                case AiAnomalyRuleCodes.RoundLarge:
                    return T(loc, c, "AIJ_Why_round_large", Money(N("total")), Money(N("unit")), Money(N("min_amount")));
                default: return r.RuleCode;
            }
        }

        /// <summary>أيقونة الخطورة (مع اللون والنص؛ لا لون وحده).</summary>
        public static string SeverityIcon(string sev) => sev switch
        {
            AiAnomalySeverity.High => MudBlazor.Icons.Material.Filled.Error,
            AiAnomalySeverity.Medium => MudBlazor.Icons.Material.Filled.Warning,
            _ => MudBlazor.Icons.Material.Filled.Info,
        };

        public static MudBlazor.Color SeverityColor(string sev) => sev switch
        {
            AiAnomalySeverity.High => MudBlazor.Color.Error,
            AiAnomalySeverity.Medium => MudBlazor.Color.Warning,
            _ => MudBlazor.Color.Info,
        };
    }
}
