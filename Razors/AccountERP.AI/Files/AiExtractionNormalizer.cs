using System.Globalization;
using System.Text;
using System.Text.Json.Nodes;
using AccountModelsLib;

namespace AccountERP.AI.Files
{
    /// <summary>
    /// P6: يحوّل JSON الذى أعاده نموذج الرؤية إلى AiExtraction موثوق: الأرقام الهندية (٠-٩ و۰-۹) والفواصل العربية، صيغ التاريخ الشائعة،
    /// ثقة كل حقل (0..1)، وتحقق حسابى (الكمية × السعر = الإجمالى، مجموع البنود، الإجمالى النهائى). ما لا يتسق يُعلَّم «يحتاج مراجعة» ولا يُصحَّح تخميناً.
    /// </summary>
    public static class AiExtractionNormalizer
    {
        public const double LowConfidence = 0.75;
        public const int MaxLines = 200;
        public const int MaxText = 20000;

        public static readonly (string Key, string Label)[] HeaderFields =
        {
            ("party_name", "الجهة (المورد/العميل)"),
            ("party_tax_id", "الرقم الضريبى للجهة"),
            ("doc_no", "رقم المستند"),
            ("doc_date", "تاريخ المستند"),
            ("currency", "العملة"),
            ("subtotal", "الإجمالى قبل الخصم والضريبة"),
            ("discount", "الخصم"),
            ("tax", "الضريبة"),
            ("total", "الإجمالى النهائى"),
        };

        private static readonly HashSet<string> NumberKeys = new() { "subtotal", "discount", "tax", "total" };

        // ───────────── أرقام وتواريخ ─────────────

        /// <summary>أرقام لاتينية بدل الهندية/الفارسية، والفاصلة العشرية العربية «٫» نقطة، وفاصل الآلاف «٬» يُحذف.</summary>
        public static string LatinDigits(string? s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            var sb = new StringBuilder(s.Length);
            foreach (char ch in s)
            {
                if (ch >= '٠' && ch <= '٩') sb.Append((char)('0' + (ch - '٠')));
                else if (ch >= '۰' && ch <= '۹') sb.Append((char)('0' + (ch - '۰')));
                else if (ch == '٫') sb.Append('.');
                else if (ch == '٬') { }
                else sb.Append(ch);
            }
            return sb.ToString();
        }

        /// <summary>رقم من نص أو رقم: «1,234.50» و«1.234,50» و«١٬٢٣٤٫٥٠» و«EGP 1,200» → 1234.50. null إن لم يكن رقماً.</summary>
        public static decimal? ParseNumber(JsonNode? n)
        {
            if (n is not JsonValue v) return null;
            if (v.TryGetValue<decimal>(out decimal m)) return m;
            if (v.TryGetValue<double>(out double d) && !double.IsNaN(d) && !double.IsInfinity(d)) return (decimal)d;
            if (!v.TryGetValue<string>(out var s)) return null;
            return ParseNumber(s);
        }

        public static decimal? ParseNumber(string? raw)
        {
            string s = LatinDigits(raw).Trim();
            if (s.Length == 0) return null;
            bool negative = s.StartsWith('-') || (s.StartsWith('(') && s.EndsWith(')'));
            s = new string(s.Where(ch => char.IsDigit(ch) || ch == '.' || ch == ',').ToArray());
            if (s.Length == 0 || !s.Any(char.IsDigit)) return null;
            int lastDot = s.LastIndexOf('.'), lastComma = s.LastIndexOf(',');
            if (lastDot >= 0 && lastComma >= 0)
            {
                // الأخير هو الفاصلة العشرية
                if (lastComma > lastDot) s = s.Replace(".", "").Replace(',', '.');
                else s = s.Replace(",", "");
            }
            else if (lastComma >= 0)
            {
                // فاصلة واحدة يتبعها رقمان أو أقل ⇒ عشرية (1234,50)، وإلا آلاف (1,234 / 1,234,567)
                int digitsAfter = s.Length - lastComma - 1;
                s = s.Count(ch => ch == ',') == 1 && digitsAfter is > 0 and <= 2 ? s.Replace(',', '.') : s.Replace(",", "");
            }
            else if (s.Count(ch => ch == '.') > 1) s = s.Replace(".", "");   // 1.234.567
            if (!decimal.TryParse(s, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal r)) return null;
            return negative ? -r : r;
        }

        private static readonly string[] DateFormats =
        {
            "yyyy-MM-dd", "yyyy-M-d", "yyyy/MM/dd", "yyyy/M/d", "dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy", "d-M-yyyy", "dd.MM.yyyy", "d.M.yyyy",
            "dd/MM/yy", "d/M/yy", "yyyyMMdd",
        };

        /// <summary>التاريخ بالصيغ الشائعة فى مصر (يوم/شهر/سنة). null إن لم يُفهم أو كان بعيداً جداً.</summary>
        public static DateTime? ParseDate(string? raw)
        {
            string s = LatinDigits(raw).Trim().Replace('\\', '/');
            if (s.Length == 0) return null;
            if (s.Length > 10 && s[10] == 'T') s = s[..10];
            if (DateTime.TryParseExact(s, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) && d.Year is >= 2000 and <= 2100)
                return d.Date;
            return null;
        }

        // ───────────── الحقول ─────────────

        /// <summary>حقل النموذج بأى شكل: {"v":..,"c":..} أو {"value":..,"confidence":..} أو قيمة مباشرة (ثقة 0.6).</summary>
        private static (JsonNode? Value, double Conf) Read(JsonObject o, string key)
        {
            var n = o[key];
            if (n is JsonObject f)
            {
                var val = f["v"] ?? f["value"];
                double c = Conf(f["c"] ?? f["confidence"]);
                return (val, c);
            }
            return (n, n == null ? 0 : 0.6);
        }

        private static double Conf(JsonNode? n)
        {
            double c = n is JsonValue v && (v.TryGetValue<double>(out double d) || double.TryParse(LatinDigits(v.ToString()), NumberStyles.Float, CultureInfo.InvariantCulture, out d)) ? d : 0.5;
            if (c > 1 && c <= 100) c /= 100;   // نسبة مئوية
            return Math.Clamp(c, 0, 1);
        }

        private static string Str(JsonNode? n, int max)
        {
            string s = n is JsonValue v ? (v.TryGetValue<string>(out var t) ? t : v.ToString()) : "";
            s = new string((s ?? "").Where(ch => !char.IsControl(ch) || ch == '\n').ToArray()).Trim();
            if (s is "null" or "-" or "—" or "N/A") s = "";
            return s.Length > max ? s[..max] : s;
        }

        private static string Money(decimal v) => v.ToString("0.##", CultureInfo.InvariantCulture);

        private static bool Near(decimal a, decimal b) => Math.Abs(a - b) <= Math.Max(0.05m, Math.Abs(b) * 0.01m);

        public static AiExtraction Normalize(JsonObject raw, string docType)
        {
            var x = new AiExtraction { DocType = AiExtractDocTypes.All.Contains(docType) ? docType : AiExtractDocTypes.Other };
            if (x.DocType == AiExtractDocTypes.Other)
            {
                x.Text = Str(raw["text"], MaxText);
                if (x.Text.Length == 0) x.Text = Str(raw["summary"], MaxText);
            }

            decimal? subtotal = null, discount = null, tax = null, total = null;
            if (x.DocType is AiExtractDocTypes.PurchaseInvoice or AiExtractDocTypes.SalesInvoice)
            {
                foreach (var (key, label) in HeaderFields)
                {
                    var (val, c) = Read(raw, key);
                    string value;
                    if (NumberKeys.Contains(key))
                    {
                        decimal? num = ParseNumber(val);
                        if (val != null && num == null && Str(val, 50).Length > 0) c = Math.Min(c, 0.3);
                        value = num == null ? "" : Money(num.Value);
                        switch (key) { case "subtotal": subtotal = num; break; case "discount": discount = num; break; case "tax": tax = num; break; case "total": total = num; break; }
                    }
                    else if (key == "doc_date")
                    {
                        string s = Str(val, 40);
                        var d = ParseDate(s);
                        if (s.Length > 0 && d == null) { c = Math.Min(c, 0.3); x.Warnings.Add($"تعذّر فهم تاريخ المستند «{s}»؛ راجعه."); }
                        value = d?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "";
                    }
                    else if (key == "doc_no" || key == "party_tax_id") value = LatinDigits(Str(val, 60));
                    else value = Str(val, key == "party_name" ? 200 : 20).Trim();
                    if (value.Length == 0) c = 0;
                    x.Fields.Add(new AiExtractField { Key = key, Label = label, Value = value, Confidence = Math.Round(c, 2), Low = value.Length > 0 && c < LowConfidence });
                }
                x.Currency = x.Field("currency");
            }

            if (raw["lines"] is JsonArray arr)
            {
                int no = 0;
                foreach (var n in arr)
                {
                    if (n is not JsonObject o) continue;
                    if (x.Lines.Count >= MaxLines) { x.Warnings.Add($"المستند به أكثر من {MaxLines} بند؛ قُرئ أول {MaxLines} فقط."); break; }
                    var line = new AiExtractLine { No = ++no };
                    var confs = new List<double>();
                    // الحقول الموجودة فقط تدخل فى ثقة السطر؛ نص لا يُفهم رقماً = ثقة 0.3 ويحتاج مراجعة.
                    string S(string k, int max)
                    {
                        var (v, c) = Read(o, k);
                        string s = Str(v, max);
                        if (s.Length > 0) { confs.Add(c); if (c < LowConfidence) line.LowFields.Add(k); }
                        return s;
                    }
                    decimal? N(string k)
                    {
                        var (v, c) = Read(o, k);
                        decimal? num = ParseNumber(v);
                        if (num != null) { confs.Add(c); if (c < LowConfidence) line.LowFields.Add(k); }
                        else if (Str(v, 50).Length > 0) { confs.Add(0.3); line.LowFields.Add(k); }
                        return num;
                    }
                    line.Description = S("description", 300);
                    line.Code = LatinDigits(S("code", 60));
                    line.Qty = N("qty");
                    line.Unit = S("unit", 40);
                    line.UnitPrice = N("unit_price");
                    line.Discount = N("discount");
                    line.Tax = N("tax");
                    line.LineTotal = N("line_total");
                    if (line.Description.Length == 0 && line.Code.Length == 0 && line.Qty == null && line.LineTotal == null) { no--; continue; }

                    // الكمية × السعر − الخصم (± الضريبة) = إجمالى السطر
                    if (line.Qty is decimal q && line.UnitPrice is decimal p && line.LineTotal is decimal lt)
                    {
                        decimal net = q * p - (line.Discount ?? 0);
                        if (!Near(net, lt) && !Near(net + (line.Tax ?? 0), lt))
                        {
                            foreach (var k in new[] { "qty", "unit_price", "line_total" }) if (!line.LowFields.Contains(k)) line.LowFields.Add(k);
                            x.Warnings.Add($"البند {line.No}: الكمية × السعر ({Money(net)}) لا يساوى إجمالى السطر ({Money(lt)}).");
                            confs.Add(0.4);
                        }
                    }
                    if (line.Qty is decimal qq && qq <= 0) { if (!line.LowFields.Contains("qty")) line.LowFields.Add("qty"); x.Warnings.Add($"البند {line.No}: كمية غير صالحة."); }
                    line.Confidence = Math.Round(confs.Count == 0 ? 0.5 : confs.Min(), 2);
                    x.Lines.Add(line);
                }
            }

            // مجموع البنود والإجمالى النهائى
            if (x.DocType is AiExtractDocTypes.PurchaseInvoice or AiExtractDocTypes.SalesInvoice)
            {
                var totals = x.Lines.Where(l => l.LineTotal != null).ToList();
                if (subtotal is decimal st && totals.Count == x.Lines.Count && totals.Count > 0)
                {
                    decimal sum = totals.Sum(l => l.LineTotal!.Value);
                    decimal sumNet = totals.Sum(l => l.LineTotal!.Value - (l.Tax ?? 0));
                    if (!Near(sum, st) && !Near(sumNet, st))
                    {
                        Mark(x, "subtotal");
                        x.Warnings.Add($"مجموع البنود ({Money(sum)}) لا يساوى الإجمالى قبل الخصم ({Money(st)}). ربما بند لم يُقرأ.");
                    }
                }
                if (total is decimal t)
                {
                    decimal? expected = subtotal != null ? subtotal - (discount ?? 0) + (tax ?? 0) : null;
                    if (expected != null && !Near(expected.Value, t))
                    {
                        Mark(x, "total");
                        x.Warnings.Add($"الإجمالى النهائى ({Money(t)}) لا يساوى الإجمالى − الخصم + الضريبة ({Money(expected.Value)}).");
                    }
                }
                if (x.Field("doc_no").Length == 0 && x.DocType == AiExtractDocTypes.PurchaseInvoice)
                    x.Warnings.Add("لم يُقرأ رقم فاتورة المورد؛ لن يمكن كشف التكرار به.");
            }
            if (x.Lines.Count == 0 && x.DocType != AiExtractDocTypes.Other) x.Warnings.Add("لم تُقرأ أى بنود من المستند.");
            int low = x.Fields.Count(f => f.Low) + x.Lines.Count(l => l.LowFields.Count > 0);
            if (low > 0) x.Warnings.Insert(0, $"{low} حقل/بند بثقة ضعيفة (مميّز) — راجعه قبل الاعتماد عليه.");
            return x;
        }

        private static void Mark(AiExtraction x, string key)
        {
            var f = x.Fields.FirstOrDefault(f => f.Key == key);
            if (f != null && f.Value.Length > 0) { f.Low = true; f.Confidence = Math.Min(f.Confidence, 0.5); }
        }
    }
}
