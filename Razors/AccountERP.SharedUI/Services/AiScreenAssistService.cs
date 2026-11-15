using System.Globalization;
using System.Text;
using AccountModelsLib;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;

namespace AccountERP.SharedUI.Services
{
    /// <summary>
    /// P8.9.2 — المساعد داخل الشاشات فى الواجهة (Scoped، نمط AiEInvoiceService). واجهة رقيقة فوق IAiScreenAssistGateway الذى يسجّله مضيف الويب فقط
    /// (ص27) — بدونه IsAvailable = false فلا يظهر أى زر. الصلاحيات والحساب على الخادم؛ هنا محددات الشاشة فقط (ص2). كل الاستدعاءات خارج خيط الرسم.
    /// </summary>
    public sealed class AiScreenAssistService
    {
        private readonly AssistantSessionService? _chat;
        private readonly IAiScreenAssistGateway? _gw;
        private AiScreenAssistState? _state;
        private long _stateUser;

        private readonly UserPreferencesService? _prefs;

        public AiScreenAssistService(IServiceProvider sp)
        {
            _chat = sp.GetService<AssistantSessionService>();
            _gw = sp.GetService<IAiScreenAssistGateway>();
            _prefs = sp.GetService<UserPreferencesService>();
        }

        /// <summary>الحالة تُحمَّل مرة لكل دائرة ومستخدم، وتُجدَّد بعد حفظ الإعدادات (Refresh).</summary>
        public AiScreenAssistState State
        {
            get
            {
                if (_gw == null || _chat == null || _chat.UserId <= 0) return new AiScreenAssistState();
                if (_state == null || _stateUser != _chat.UserId)
                {
                    try { _state = _gw.State(_chat.BuildUser()); }
                    catch (Exception ex) { try { operations.DataDictionaryLogerException(ex); } catch { } _state = new AiScreenAssistState(); }
                    _stateUser = _chat.UserId;
                }
                return _state;
            }
        }

        public void Refresh() => _state = null;

        /// <summary>البوابة + صلاحية المساعد + الميزة مفعّلة + النوع ضمن صلاحيات المستخدم.</summary>
        public bool IsAvailable(string kind) =>
            _gw != null && _chat != null && _chat.HasAccess && State.Enabled && State.Kinds.Contains(kind);

        public bool NarrativeOn => State.Narrative;

        private static Task<T> Bg<T>(Func<T> f) => Task.Run(() => { AccountCultureGuard.EnsureInvariant(); return f(); });
        private IAiScreenAssistGateway Gw => _gw ?? throw new InvalidOperationException("المساعد داخل الشاشات غير متاح.");
        private AiUserContext User() => _chat?.BuildUser() ?? throw new InvalidOperationException("المساعد داخل الشاشات غير متاح.");

        public async Task<AiScreenFacts> BuildAsync(AiScreenPayload p)
        {
            var gw = Gw; var user = User();
            return await Bg(() => gw.Build(p, user));
        }

        public async Task<AiScreenNarrative> ExplainAsync(AiScreenPayload p, string hash, string language, CancellationToken ct)
        {
            var gw = Gw; var user = User();
            return await Task.Run(async () => { AccountCultureGuard.EnsureInvariant(); return await gw.ExplainAsync(p, hash, language, user, ct); }, ct);
        }

        public async Task<AiScreenAssistSettings> GetSettingsAsync()
        {
            var gw = Gw; var user = User();
            return await Bg(() => gw.GetSettings(user));
        }

        public async Task SaveSettingsAsync(AiScreenAssistSettings s)
        {
            var gw = Gw; var user = User();
            await Bg(() => { gw.SaveSettings(s, user); return true; });
            Refresh();
        }

        public bool GatewayRegistered => _gw != null && _chat != null;
        public bool CanManage => GatewayRegistered && _chat!.UserId > 0 && (_chat.BuildUser().IsAdmin || _chat.CanOpenSettings);

        // ───── «اسأل المساعد عن هذا» والسياق المحيط (P8.9.4) ─────
        public enum AskResult { Busy, OpenWidget, OpenPage }

        public bool CanAskAssistant => _chat != null && _chat.HasAccess;

        /// <summary>النافذة العائمة ظاهرة حسب تفضيل المستخدم (وإلا يُفتح /assistant — ص23).</summary>
        public bool WidgetVisible => _prefs?.ShowAiAssistantIcon ?? false;

        /// <summary>
        /// ص20/ص23: محادثة جديدة بنص ثابت + الحمولة بـ Attach = true (الخادم يبنى الحقائق ويحفظها كتلة screen_facts)، ثم فتح النافذة فى مكانها.
        /// </summary>
        public AskResult AskAssistant(AiScreenPayload p, string text, string route, string title, string language)
        {
            if (_chat == null || !_chat.HasAccess || p == null) return AskResult.Busy;
            var copy = p.Clone(); copy.Attach = true;
            bool ok = _chat.TryAskNew(text, new AiScreenContext { Route = route ?? "", Title = title ?? "", Language = language ?? "ar", Payload = copy });
            if (!ok) return AskResult.Busy;
            if (WidgetVisible) { _chat.RequestOpen(); return AskResult.OpenWidget; }
            return AskResult.OpenPage;
        }

        /// <summary>تسجّله الشاشة بعد عرض بياناتها (لا شىء إن السياق المحيط موقوف أو النوع غير متاح).</summary>
        public void SetContext(string route, AiScreenPayload p, string label)
        {
            if (_chat == null || p == null || !State.Ambient || !IsAvailable(p.Kind)) return;
            _chat.SetAmbient(route, p, label);
        }

        public void ClearContext(string route) => _chat?.ClearAmbient(route);

        /// <summary>يفتح حوار «شرح المساعد» للحمولة.</summary>
        public async Task OpenDialogAsync(IDialogService dialogs, AiScreenPayload p, string route, string title)
        {
            var prm = new DialogParameters<Components.Assistant.ScreenExplainDialog>
            {
                { x => x.Payload, p },
                { x => x.Route, route ?? "" },
                { x => x.ScreenTitle, title ?? "" },
            };
            var o = new DialogOptions { CloseOnEscapeKey = true, CloseButton = true, MaxWidth = MaxWidth.Medium, FullWidth = true };
            await dialogs.ShowAsync<Components.Assistant.ScreenExplainDialog>(title, prm, o);
        }
    }

    /// <summary>نصوص الحوار من الـ resx (ص28): الحقائق محايدة لغوياً وتُرسم هنا.</summary>
    public static class AiScreenAssistText
    {
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
        private static readonly HashSet<string> IntKeys = new(new[] { "count", "days", "months" });

        public static string TitleKey(string kind, string question) => "AIH_T_" + kind + "_" + question;

        public static string Number(decimal v, CultureInfo c) => v.ToString("N2", c);

        public static string Value(AiScreenValue v, CultureInfo c, Func<string, string> tr) => v.Format switch
        {
            "int" => (v.Number ?? 0).ToString("N0", c),
            "pct" => (v.Number ?? 0).ToString("0.#", c) + "%",
            "date" => v.Date?.ToString("yyyy/MM/dd", Inv) ?? "—",
            "code" => tr("AIH_" + (v.Text ?? "")),
            _ => Number(v.Number ?? 0, c),
        };

        /// <summary>نص النتيجة من قالبها: {name} ⇒ القيمة منسَّقة بثقافة الواجهة.</summary>
        public static string Finding(AiScreenFinding f, CultureInfo c, Func<string, string> tr)
        {
            string text = tr("AIH_F_" + f.Code);
            foreach (var kv in f.Values)
            {
                string shown;
                if (kv.Key == "source") shown = tr("AIH_Src_" + kv.Value);
                else if (kv.Key == "entry_no") shown = kv.Value;
                else if (decimal.TryParse(kv.Value, NumberStyles.Number, Inv, out var d))
                    shown = IntKeys.Contains(kv.Key) ? d.ToString("N0", c) : kv.Key == "pct" ? d.ToString("0.#", c) : d.ToString("N2", c);
                else shown = kv.Value;
                text = text.Replace("{" + kv.Key + "}", shown);
            }
            return text;
        }

        public static string Cell(string value, string type, CultureInfo c, Func<string, string> tr)
        {
            if (string.IsNullOrEmpty(value)) return "—";
            switch (type)
            {
                case "code": return tr("AIH_" + value);
                case "number": return decimal.TryParse(value, NumberStyles.Number, Inv, out var d) ? d.ToString("N2", c) : value;
                case "int": return decimal.TryParse(value, NumberStyles.Number, Inv, out var i) ? i.ToString("N0", c) : value;
                case "pct": return decimal.TryParse(value, NumberStyles.Number, Inv, out var p) ? p.ToString("0.#", c) + "%" : value;
                case "date": return DateTime.TryParse(value, Inv, DateTimeStyles.None, out var dt) ? dt.ToString("yyyy/MM/dd", Inv) : value;
                default: return value;
            }
        }

        public static (Color Color, string Icon) Severity(string s) => s switch
        {
            AiScreenSeverity.High => (Color.Error, Icons.Material.Outlined.ErrorOutline),
            AiScreenSeverity.Warn => (Color.Warning, Icons.Material.Outlined.WarningAmber),
            _ => (Color.Info, Icons.Material.Outlined.Info),
        };

        public static string SeverityKey(string s) => s switch
        {
            AiScreenSeverity.High => "AIH_Sev_high",
            AiScreenSeverity.Warn => "AIH_Sev_warn",
            _ => "AIH_Sev_info",
        };

        /// <summary>«نسخ»: نص عادى للحقائق والفقرة.</summary>
        public static string PlainText(AiScreenFacts f, AiScreenNarrative? n, CultureInfo c, Func<string, string> tr)
        {
            var sb = new StringBuilder();
            sb.Append(tr(TitleKey(f.Kind, f.Question)));
            if (!string.IsNullOrWhiteSpace(f.Subject)) sb.Append(" — ").Append(f.Subject);
            sb.Append('\n');
            foreach (var v in f.Values)
            {
                sb.Append(tr("AIH_V_" + v.Key)).Append(": ").Append(Value(v, c, tr));
                if (!string.IsNullOrEmpty(v.Side)) sb.Append(' ').Append(tr(v.Side == "debit" ? "AIH_Side_Debit" : "AIH_Side_Credit"));
                sb.Append('\n');
            }
            if (f.Findings.Count > 0)
            {
                sb.Append('\n').Append(tr("AIH_FactsTitle")).Append(":\n");
                foreach (var x in f.Findings) sb.Append("- ").Append(Finding(x, c, tr)).Append('\n');
            }
            if (n != null && n.Source == AiBriefNarrativeSource.Model && !string.IsNullOrWhiteSpace(n.Text))
                sb.Append('\n').Append(tr("AIH_NarrTitle")).Append(":\n").Append(n.Text.Trim()).Append('\n');
            return sb.ToString().Trim();
        }

        public static string ErrorKey(Exception ex) => ex is AiScreenAssistException a && a.Code.StartsWith("AIH_", StringComparison.Ordinal) ? a.Code : "AIH_Err_Failed";
    }
}
