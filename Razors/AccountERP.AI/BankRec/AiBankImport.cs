using System.Globalization;
using System.Text;
using AccountModelsLib;
using MiniExcelLibs;

namespace AccountERP.AI.BankRec
{
    /// <summary>
    /// P8.12.1 (ai-assistant-p812-plan.md ب1، ب2): قراءة كشف البنك من CSV (UTF-8 وإلا Windows-1256) أو Excel (xlsx أول ورقة)، ثم تحويل الصفوف إلى أسطر
    /// بتعيين الأعمدة. الأرقام: أرقام عربية، فواصل آلاف، أقواس = سالب. التواريخ: dmy/mdy/ymd أو تاريخ Excel. صف بلا تاريخ ولا مبلغ يُتخطى.
    /// </summary>
    public static class AiBankImport
    {
        public const int MaxRows = 5000;

        public static bool IsExcel(string fileName) => fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase);
        public static bool IsCsv(string fileName) => fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase);

        /// <summary>الجدول: أول صف غير فارغ = العناوين.</summary>
        public static (List<string> Headers, List<object?[]> Rows) ReadTable(byte[] data, string fileName)
        {
            var rows = new List<object?[]>();
            if (IsExcel(fileName))
            {
                using var ms = new MemoryStream(data);
                foreach (IDictionary<string, object?> row in MiniExcel.Query(ms, useHeaderRow: false))
                {
                    var cells = row.Values.ToArray();
                    if (cells.All(c => c == null || string.IsNullOrWhiteSpace(Convert.ToString(c, CultureInfo.InvariantCulture)))) continue;
                    rows.Add(cells);
                    if (rows.Count > MaxRows) break;
                }
            }
            else if (IsCsv(fileName))
            {
                string text = Decode(data);
                char sep = DetectSeparator(text);
                foreach (var raw in text.Split('\n'))
                {
                    string line = raw.TrimEnd('\r');
                    if (line.Trim().Length == 0) continue;
                    rows.Add(SplitCsv(line, sep).Cast<object?>().ToArray());
                    if (rows.Count > MaxRows) break;
                }
            }
            else throw new AiBankException("AIN_ErrFormat", "صيغة الملف غير مدعومة (CSV أو xlsx).");
            if (rows.Count < 2) throw new AiBankException("AIN_ErrEmpty", "الملف بلا صفوف بيانات.");
            var headers = rows[0].Select((h, i) => { var s = (Convert.ToString(h, CultureInfo.InvariantCulture) ?? "").Trim(); return s.Length == 0 ? "col" + (i + 1) : s; }).ToList();
            return (headers, rows.Skip(1).ToList());
        }

        public static List<AiBankLine> ToLines(List<string> headers, List<object?[]> rows, AiBankColumnMap map, out List<string> warnings)
        {
            warnings = new List<string>();
            int Ix(string? name) => string.IsNullOrWhiteSpace(name) ? -1 : headers.FindIndex(h => string.Equals(h, name.Trim(), StringComparison.OrdinalIgnoreCase));
            int iDate = Ix(map.Date), iVal = Ix(map.ValueDate), iDesc = Ix(map.Description), iRef = Ix(map.Reference),
                iDr = Ix(map.Debit), iCr = Ix(map.Credit), iAmt = Ix(map.Amount), iBal = Ix(map.Balance);
            if (iDate < 0) throw new AiBankException("AIN_ErrMapDate", "حدّد عمود التاريخ.");
            if (iAmt < 0 && iDr < 0 && iCr < 0) throw new AiBankException("AIN_ErrMapAmount", "حدّد عمود المبلغ أو عمودى المدين والدائن.");
            var list = new List<AiBankLine>();
            int no = 0, skipped = 0;
            foreach (var r in rows)
            {
                object? Cell(int i) => i >= 0 && i < r.Length ? r[i] : null;
                var date = ParseDate(Cell(iDate), map.DateOrder);
                decimal dr = 0, cr = 0;
                if (iAmt >= 0)
                {
                    var a = ParseNumber(Cell(iAmt));
                    if (a is decimal av) { if (av < 0) dr = -av; else cr = av; }
                }
                else
                {
                    dr = Math.Abs(ParseNumber(Cell(iDr)) ?? 0);
                    cr = Math.Abs(ParseNumber(Cell(iCr)) ?? 0);
                }
                if (date == null && dr == 0 && cr == 0) { skipped++; continue; }
                if (date == null) { warnings.Add("row_no_date"); continue; }
                list.Add(new AiBankLine
                {
                    LineNo = ++no, Date = date, ValueDate = ParseDate(Cell(iVal), map.DateOrder),
                    Description = Str(Cell(iDesc), 500), Reference = Str(Cell(iRef), 100), Debit = dr, Credit = cr, Balance = ParseNumber(Cell(iBal)),
                });
            }
            if (skipped > 0) warnings.Add("rows_skipped:" + skipped.ToString(CultureInfo.InvariantCulture));
            return list;
        }

        public static string? Str(object? o, int max)
        {
            string s = (Convert.ToString(o, CultureInfo.InvariantCulture) ?? "").Trim();
            if (s.Length == 0) return null;
            return s.Length > max ? s.Substring(0, max) : s;
        }

        public static decimal? ParseNumber(object? o)
        {
            if (o == null) return null;
            if (o is decimal d) return d;
            if (o is double db) return (decimal)db;
            if (o is int i) return i;
            if (o is long l) return l;
            string s = Convert.ToString(o, CultureInfo.InvariantCulture) ?? "";
            var sb = new StringBuilder();
            bool neg = false;
            foreach (char ch in s.Trim())
            {
                char c = ch;
                if (c >= '٠' && c <= '٩') c = (char)('0' + (c - '٠'));
                else if (c >= '۰' && c <= '۹') c = (char)('0' + (c - '۰'));
                else if (c == '٫') c = '.';
                if (c == '(' || c == '-' || c == '−') { neg = true; continue; }
                if (char.IsDigit(c) || c == '.') sb.Append(c);
            }
            if (sb.Length == 0) return null;
            if (!decimal.TryParse(sb.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var v)) return null;
            return neg ? -v : v;
        }

        public static DateTime? ParseDate(object? o, string? order)
        {
            if (o == null) return null;
            if (o is DateTime dt) return dt.Date;
            if (o is double serial && serial > 20000 && serial < 80000) return DateTime.FromOADate(serial).Date;
            string s = (Convert.ToString(o, CultureInfo.InvariantCulture) ?? "").Trim();
            if (s.Length == 0) return null;
            var norm = new StringBuilder();
            foreach (char ch in s) norm.Append(ch >= '٠' && ch <= '٩' ? (char)('0' + (ch - '٠')) : ch);
            s = norm.ToString().Split(' ')[0];
            var parts = s.Split(new[] { '/', '-', '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3 || !parts.All(p => p.All(char.IsDigit))) return null;
            int a = int.Parse(parts[0], CultureInfo.InvariantCulture), b = int.Parse(parts[1], CultureInfo.InvariantCulture), c = int.Parse(parts[2], CultureInfo.InvariantCulture);
            int y, m, d;
            if (parts[0].Length == 4 || order == "ymd") { y = a; m = b; d = c; }
            else if (order == "mdy") { m = a; d = b; y = c; }
            else { d = a; m = b; y = c; }
            if (y < 100) y += 2000;
            try { return new DateTime(y, m, d); } catch (ArgumentOutOfRangeException) { return null; }
        }

        private static string Decode(byte[] data)
        {
            try { return new UTF8Encoding(false, true).GetString(data).TrimStart('﻿'); }
            catch (DecoderFallbackException)
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                return Encoding.GetEncoding(1256).GetString(data);
            }
        }

        private static char DetectSeparator(string text)
        {
            string first = text.Split('\n').FirstOrDefault(l => l.Trim().Length > 0) ?? "";
            int c = first.Count(x => x == ','), sc = first.Count(x => x == ';'), t = first.Count(x => x == '\t');
            return t > c && t > sc ? '\t' : sc > c ? ';' : ',';
        }

        public static List<string> SplitCsv(string line, char sep)
        {
            var list = new List<string>();
            var sb = new StringBuilder();
            bool q = false;
            for (int i = 0; i < line.Length; i++)
            {
                char ch = line[i];
                if (q)
                {
                    if (ch == '"' && i + 1 < line.Length && line[i + 1] == '"') { sb.Append('"'); i++; }
                    else if (ch == '"') q = false;
                    else sb.Append(ch);
                }
                else if (ch == '"') q = true;
                else if (ch == sep) { list.Add(sb.ToString()); sb.Clear(); }
                else sb.Append(ch);
            }
            list.Add(sb.ToString());
            return list;
        }
    }
}
