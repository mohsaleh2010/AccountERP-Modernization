using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using AccountERP.SharedUI.Components.Assistant;
using AccountModelsLib;
using Microsoft.Extensions.Localization;

namespace AccountERP.SharedUI.Services
{
    /// <summary>
    /// P8.1.5 (ق7): الحقائق والتنبيهات محايدة لغوياً، وكل نص يُرسم هنا من resx بثقافة الواجهة الحالية،
    /// فتبديل اللغة يغيّر العناوين والقالب فوراً. سرد النموذج وحده مخزّن بلغته.
    /// </summary>
    public static class AiBriefText
    {
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        /// <summary>الجامع يكتب camelCase؛ القراءة لا تهتم بحالة الأحرف.</summary>
        public static readonly JsonSerializerOptions Json = new()
        {
            PropertyNameCaseInsensitive = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        public static AiBriefFacts? ParseFacts(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            try { return JsonSerializer.Deserialize<AiBriefFacts>(json, Json); }
            catch (JsonException) { return null; }
        }

        public static List<AiBriefAlert> ParseAlerts(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new();
            try { return JsonSerializer.Deserialize<List<AiBriefAlert>>(json, Json) ?? new(); }
            catch (JsonException) { return new(); }
        }

        /// <summary>نص من resx بثقافة محددة (نفس حيلة Tr فى المكونات: تبديل اللغة لا يغيّر CurrentUICulture).</summary>
        public static string T(IStringLocalizer loc, CultureInfo culture, string key, params object[] args)
        {
            var prev = CultureInfo.CurrentUICulture;
            try
            {
                CultureInfo.CurrentUICulture = culture;
                return args.Length == 0 ? loc[key] : loc[key, args];
            }
            finally { CultureInfo.CurrentUICulture = prev; }
        }

        public static string Money(decimal v) => AiReportFmt.Number(v, AiReportFormats.Amount);
        public static string Count(decimal v) => AiReportFmt.Number(v, AiReportFormats.Int);
        public static string Pct(decimal? v) => v == null ? "—" : AiReportFmt.Number(Math.Round(v.Value, 1), AiReportFormats.Percent);
        public static string Day(DateTime d) => d.ToString("yyyy-MM-dd", Inv);

        /// <summary>قيمة داخل نص تنبيه: الأعداد الصحيحة بلا كسور، وغيرها بخانتين على الأكثر.</summary>
        private static string Val(decimal v) => v == decimal.Truncate(v) ? v.ToString("#,0", Inv) : v.ToString("#,0.##", Inv);

        public static string SeverityKey(string severity) => severity switch
        {
            AiBriefSeverity.High => "AIB_SevHigh",
            AiBriefSeverity.Medium => "AIB_SevMedium",
            _ => "AIB_SevInfo",
        };

        public static string SectionKey(string section) => section switch
        {
            AiBriefSections.Sales => "AIB_SecSales",
            AiBriefSections.Collections => "AIB_SecCollections",
            AiBriefSections.Liquidity => "AIB_SecLiquidity",
            AiBriefSections.Receivables => "AIB_SecReceivables",
            AiBriefSections.Purchases => "AIB_SecPurchases",
            _ => "AIB_SecAlerts",
        };

        public static string AlertTitle(IStringLocalizer loc, CultureInfo c, AiBriefAlert a) => T(loc, c, "AIB_Al_" + a.Code);

        /// <summary>نص التنبيه: القالب يحمل أسماء القيم بين أقواس ({net}، {items}) فلا يعتمد على ترتيبها.</summary>
        public static string AlertText(IStringLocalizer loc, CultureInfo c, AiBriefAlert a)
        {
            string text = T(loc, c, "AIB_Al_" + a.Code + "_Text");
            foreach (var (k, v) in a.Values ?? new Dictionary<string, decimal>())
                text = text.Replace("{" + k + "}", Val(v));
            return text.Replace("{items}", string.Join("، ", (a.Items ?? new List<string>()).Take(5)));
        }

        /// <summary>رابط «افتح الشاشة» للتنبيه: مسار داخلى فقط + معاملاته.</summary>
        public static string? AlertUrl(AiBriefAlert a)
        {
            string r = a.Route ?? "";
            if (!r.StartsWith('/') || r.StartsWith("//")) return null;
            var q = (a.Params ?? new Dictionary<string, string>())
                .Where(p => !string.IsNullOrWhiteSpace(p.Key) && p.Value != null)
                .Select(p => Uri.EscapeDataString(p.Key) + "=" + Uri.EscapeDataString(p.Value));
            string qs = string.Join("&", q);
            return qs.Length == 0 ? r : r + "?" + qs;
        }

        /// <summary>
        /// ق6: السرد القالبى (بلا نموذج). المبالغ المذكورة بالعملة المحلية؛ الخزينة تُذكر فقط حين تكون عملة واحدة (لا جمع بين العملات).
        /// </summary>
        public static string Template(IStringLocalizer loc, CultureInfo c, AiBriefFacts? f, IReadOnlyCollection<AiBriefAlert> alerts)
        {
            if (f == null) return "";
            var parts = new List<string>();
            var s = f.Sales;
            if (s != null)
            {
                parts.Add(T(loc, c, "AIB_T_Sales", Money(s.NetSales), Count(s.Invoices)));
                if (s.VsWeekdayPct is decimal p && p != 0)
                    parts.Add(T(loc, c, p > 0 ? "AIB_T_SalesUp" : "AIB_T_SalesDown", Pct(Math.Abs(p)).TrimEnd('%')));
            }
            var col = f.Collections?.ByCurrency;
            if (col is { Count: 1 }) parts.Add(T(loc, c, "AIB_T_Collections", Money(col[0].Receipts), Money(col[0].Payments)));
            var liq = f.Liquidity?.ByCurrency;
            if (liq is { Count: 1 }) parts.Add(T(loc, c, "AIB_T_Liquidity", Money(liq[0].Balance)));
            if (f.Sections.Contains(AiBriefSections.Alerts))
                parts.Add(alerts.Count > 0 ? T(loc, c, "AIB_T_Alerts", alerts.Count) : T(loc, c, "AIB_T_NoAlerts"));
            return string.Join(" ", parts);
        }

        /// <summary>رسالة خطأ البوابة بلغة الواجهة (Code = مفتاح resx)، وإلا النص العربى الاحتياطى.</summary>
        public static string Error(IStringLocalizer loc, CultureInfo c, Exception ex)
        {
            if (ex is AiBriefException b && b.Code.Length > 0)
            {
                var s = T(loc, c, b.Code);
                if (s != b.Code) return s;
            }
            return ex is InvalidOperationException ? ex.Message : (ex.InnerException?.Message ?? ex.Message);
        }
    }
}
