using System.Globalization;
using AccountModelsLib;

namespace AccountERP.SharedUI.Components.Assistant
{
    /// <summary>P3: تنسيق قيم تقارير المساعد (الأرقام بالأرقام اللاتينية كباقى شاشات النظام، invariant).</summary>
    public static class AiReportFmt
    {
        public static string Format(object? v, string format)
        {
            if (v == null) return "";
            if (v is decimal m) return Number(m, format);
            return Convert.ToString(v, CultureInfo.InvariantCulture) ?? "";
        }

        public static string Number(decimal? v, string format)
        {
            if (v == null) return "—";
            decimal m = v.Value;
            return format switch
            {
                AiReportFormats.Qty => m.ToString(m == decimal.Truncate(m) ? "N0" : "N3", CultureInfo.InvariantCulture),
                AiReportFormats.Int => m.ToString("N0", CultureInfo.InvariantCulture),
                AiReportFormats.Percent => m.ToString("0.##", CultureInfo.InvariantCulture) + "%",
                AiReportFormats.Text or AiReportFormats.Date => m.ToString(CultureInfo.InvariantCulture),
                _ => m.ToString("N2", CultureInfo.InvariantCulture),
            };
        }

        /// <summary>اختصار لمحاور الرسوم: 1.2K، 3.4M.</summary>
        public static string Compact(decimal v)
        {
            decimal a = Math.Abs(v);
            if (a >= 1_000_000_000m) return (v / 1_000_000_000m).ToString("0.#", CultureInfo.InvariantCulture) + "B";
            if (a >= 1_000_000m) return (v / 1_000_000m).ToString("0.#", CultureInfo.InvariantCulture) + "M";
            if (a >= 10_000m) return (v / 1_000m).ToString("0.#", CultureInfo.InvariantCulture) + "K";
            return v.ToString(a < 10 && v != decimal.Truncate(v) ? "0.##" : "N0", CultureInfo.InvariantCulture);
        }

        public static bool IsNumeric(string format) =>
            format is AiReportFormats.Amount or AiReportFormats.Qty or AiReportFormats.Int or AiReportFormats.Percent;
    }
}
