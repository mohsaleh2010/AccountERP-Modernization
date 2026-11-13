using System.Globalization;
using AccountModelsLib;

namespace AccountERP.SharedUI.Services
{
    /// <summary>
    /// P3: تصدير تقرير المساعد إلى Excel عبر MiniExcel (الموجود): ورقة «ملخص» (العنوان، الفترة، المؤشرات)، ثم ورقة لكل جدول أو رسم
    /// بالقيم الرقمية كأرقام (لا نصوص) حتى تعمل الصيغ فى Excel.
    /// </summary>
    public static class AiReportExcel
    {
        public static byte[] Build(AiReportDoc doc, string summaryName, string totalLabel)
        {
            var sheets = new Dictionary<string, object>();
            var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var summary = new List<Dictionary<string, object?>>
            {
                Row(("A", doc.Title), ("B", "")),
            };
            if (!string.IsNullOrWhiteSpace(doc.Subtitle)) summary.Add(Row(("A", doc.Subtitle), ("B", "")));
            if (doc.PeriodFrom != null && doc.PeriodTo != null)
                summary.Add(Row(("A", D(doc.PeriodFrom) + " → " + D(doc.PeriodTo)), ("B", "")));
            foreach (var k in doc.Kpis) summary.Add(Row(("A", k.Label), ("B", k.Value)));
            foreach (var s in doc.Sections.Where(s => s.Kind == "text"))
                summary.Add(Row(("A", string.IsNullOrWhiteSpace(s.Title) ? s.Text : s.Title + ": " + s.Text), ("B", "")));
            sheets[Name(summaryName, used)] = summary;

            int n = 0;
            foreach (var s in doc.Sections)
            {
                n++;
                if (s.Kind == "table")
                {
                    var rows = new List<Dictionary<string, object?>>();
                    var headers = Headers(s.Columns.Select(c => c.Label));
                    foreach (var r in s.Rows)
                    {
                        var d = new Dictionary<string, object?>();
                        for (int j = 0; j < headers.Count; j++) d[headers[j]] = j < r.Length ? Cell(r[j]) : null;
                        rows.Add(d);
                    }
                    if (s.ShowTotals)
                    {
                        var d = new Dictionary<string, object?>();
                        for (int j = 0; j < headers.Count; j++)
                            d[headers[j]] = j < s.Totals.Count && s.Totals[j] != null ? s.Totals[j] : j == 0 ? totalLabel : null;
                        rows.Add(d);
                    }
                    if (rows.Count == 0) rows.Add(headers.ToDictionary(h => h, h => (object?)null));
                    sheets[Name(string.IsNullOrWhiteSpace(s.Title) ? "Table " + n : s.Title, used)] = rows;
                }
                else if (s.Kind == "chart" && s.ChartLabels.Count > 0)
                {
                    var headers = Headers(new[] { "-" }.Concat(s.Series.Select(x => x.Name)));
                    var rows = new List<Dictionary<string, object?>>();
                    for (int i = 0; i < s.ChartLabels.Count; i++)
                    {
                        var d = new Dictionary<string, object?> { [headers[0]] = s.ChartLabels[i] };
                        for (int j = 0; j < s.Series.Count; j++) d[headers[j + 1]] = i < s.Series[j].Values.Count ? s.Series[j].Values[i] : null;
                        rows.Add(d);
                    }
                    sheets[Name(string.IsNullOrWhiteSpace(s.Title) ? "Chart " + n : s.Title, used)] = rows;
                }
            }

            using var ms = new MemoryStream();
            MiniExcelLibs.MiniExcel.SaveAs(ms, sheets);
            return ms.ToArray();
        }

        private static Dictionary<string, object?> Row(params (string K, object? V)[] cells) => cells.ToDictionary(c => c.K, c => c.V);

        private static string D(DateTime? d) => d?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "";

        /// <summary>التاريخ (yyyy-MM-dd) يُكتب تاريخاً حقيقياً؛ الباقى كما هو.</summary>
        private static object? Cell(object? v) =>
            v is string s && s.Length == 10 && DateTime.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d : v;

        /// <summary>عناوين أعمدة فريدة وغير فارغة (MiniExcel يستخدمها مفاتيح).</summary>
        private static List<string> Headers(IEnumerable<string> labels)
        {
            var list = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var l0 in labels)
            {
                string l = string.IsNullOrWhiteSpace(l0) ? "col" : l0.Trim(), u = l;
                for (int i = 2; !seen.Add(u); i++) u = l + " (" + i + ")";
                list.Add(u);
            }
            return list;
        }

        /// <summary>اسم ورقة صالح: بلا []:*?/\ وحتى 31 حرفاً وفريد.</summary>
        private static string Name(string title, HashSet<string> used)
        {
            var chars = (title ?? "").Where(c => !"[]:*?/\\".Contains(c)).ToArray();
            string b = new string(chars).Trim();
            if (b.Length == 0) b = "Sheet";
            if (b.Length > 28) b = b[..28];
            string n = b;
            for (int i = 2; !used.Add(n); i++) n = b + " " + i;
            return n;
        }
    }
}
