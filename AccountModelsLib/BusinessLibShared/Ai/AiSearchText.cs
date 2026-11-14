using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AccountModelsLib
{
    /// <summary>
    /// P8.2 (س9–س11، س17، س31): تطبيع النص العربى وتحليل ما يكتبه المستخدم فى لوحة البحث وترتيب الشاشات. دوال نقية بلا قاعدة بيانات.
    /// نفس التطبيع (جزء الحروف منه) يُعاد داخل SQL بسلسلة REPLACE فى بوابة البحث.
    /// </summary>
    public static class AiSearchText
    {
        /// <summary>
        /// أ/إ/آ/ٱ→ا، ة→ه، ى/ی→ي، ک→ك، حذف التشكيل والتطويل، الأرقام الهندية والفارسية→لاتينية، أحرف صغيرة، ضم المسافات.
        /// </summary>
        public static string Normalize(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            var sb = new StringBuilder(s.Length);
            bool space = false;
            foreach (char ch in s)
            {
                char c = ch;
                if ((c >= 'ً' && c <= 'ٟ') || c == 'ٰ' || c == 'ـ') continue;   // تشكيل، ألف خنجرية، تطويل
                if (c >= '٠' && c <= '٩') c = (char)('0' + (c - '٠'));
                else if (c >= '۰' && c <= '۹') c = (char)('0' + (c - '۰'));
                else if (c == 'أ' || c == 'إ' || c == 'آ' || c == 'ٱ') c = 'ا';
                else if (c == 'ة') c = 'ه';
                else if (c == 'ى' || c == 'ی') c = 'ي';
                else if (c == 'ک') c = 'ك';
                if (char.IsWhiteSpace(c) || char.IsControl(c))
                {
                    space = sb.Length > 0;
                    continue;
                }
                if (space) { sb.Append(' '); space = false; }
                sb.Append(char.ToLowerInvariant(c));
            }
            return sb.ToString();
        }

        // جدول البادئات (س31) مطبّعاً. القيمة = Kind أو DocScope.
        private static readonly Dictionary<string, string> ScopeWords = BuildScopeWords();

        private static Dictionary<string, string> BuildScopeWords()
        {
            var d = new Dictionary<string, string>(StringComparer.Ordinal);
            void Add(string scope, params string[] words)
            {
                foreach (var w in words) d[Normalize(w)] = scope;
            }
            Add(AiSearchKinds.Customer, "عميل", "عملاء", "العميل", "العملاء", "customer", "customers", "kunde", "client");
            Add(AiSearchKinds.Supplier, "مورد", "موردين", "المورد", "الموردين", "supplier", "suppliers", "lieferant", "fournisseur");
            Add(AiSearchKinds.Item, "صنف", "أصناف", "الصنف", "الأصناف", "item", "items", "artikel", "article");
            Add(AiSearchKinds.DocScope, "فاتورة", "الفاتورة", "مرتجع", "المرتجع", "invoice", "inv", "return", "rechnung", "facture");
            return d;
        }

        public static AiSearchQuery Parse(string raw)
        {
            var q = new AiSearchQuery();
            string r = (raw ?? "").Trim();
            if (r.Length > AiSearchConst.MaxQueryChars) r = r.Substring(0, AiSearchConst.MaxQueryChars).Trim();
            q.Raw = r;

            string n = Normalize(r);
            if (n.Length > 0 && (IsAskMark(n[0]) || IsAskMark(n[n.Length - 1])))
            {
                q.AskOnly = true;
                n = n.Trim('?', '؟', ' ');
            }
            q.Normalized = n;
            q.DataText = n;

            if (n.StartsWith("#", StringComparison.Ordinal))
            {
                string rest = n.Substring(1).Trim();
                long? num = DocNumberOf(rest);
                if (num != null)
                {
                    q.Scope = AiSearchKinds.DocScope;
                    q.DocNumber = num;
                    q.DataText = rest;
                }
            }
            else
            {
                int sp = n.IndexOf(' ');
                string first = sp < 0 ? n : n.Substring(0, sp);
                string rest = sp < 0 ? "" : n.Substring(sp + 1).Trim();
                string scope;
                if (rest.Length > 0 && ScopeWords.TryGetValue(first, out scope))
                {
                    if (scope == AiSearchKinds.DocScope)
                    {
                        long? num = DocNumberOf(rest);
                        if (num != null)
                        {
                            q.Scope = scope;
                            q.DocNumber = num;
                            q.DataText = rest;
                        }
                    }
                    else
                    {
                        q.Scope = scope;
                        q.DataText = rest;
                    }
                }
                else
                {
                    q.DocNumber = DocNumberOf(n);
                }
            }

            q.Tokens = q.DataText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(t => t.Length >= AiSearchConst.MinChars || IsDigits(t))
                .Distinct(StringComparer.Ordinal)
                .Take(AiSearchConst.MaxTokens)
                .ToList();
            return q;
        }

        /// <summary>
        /// ترتيب شاشة (0 = لا تطابق): كل كلمة يجب أن تطابق فى أحد العناوين (أى لغة) أو مسار الآباء.
        /// كلمة كاملة 100، بادئة كلمة 60، جزء 30، فى مسار الآباء فقط 10 (تُجرَّب مع «ال» وبدونها)؛ العنوان = النص +200، يبدأ به +100؛ التكرار min(hit,10)×5.
        /// </summary>
        public static int Score(string query, string title, IEnumerable<string> altTitles, string parentPath, int hitCount)
        {
            string q = Normalize(query);
            if (q.Length == 0) return 0;
            var titles = new List<string>();
            string t0 = Normalize(title);
            if (t0.Length > 0) titles.Add(t0);
            if (altTitles != null)
                foreach (var a in altTitles)
                {
                    string na = Normalize(a);
                    if (na.Length > 0 && !titles.Contains(na)) titles.Add(na);
                }
            if (titles.Count == 0) return 0;
            string parent = Normalize(parentPath);

            var titleWords = titles.Select(t => t.Split(' ')).ToList();
            int score = 0;
            foreach (var w in q.Split(' ').Distinct(StringComparer.Ordinal))
            {
                var forms = Forms(w);
                int best = 0;
                foreach (var words in titleWords)
                    foreach (var tw in words)
                        foreach (var f in forms)
                        {
                            int s = tw == f ? 100 : tw.StartsWith(f, StringComparison.Ordinal) ? 60 : tw.IndexOf(f, StringComparison.Ordinal) >= 0 ? 30 : 0;
                            if (s > best) best = s;
                        }
                if (best == 0 && parent.Length > 0 && forms.Any(f => parent.IndexOf(f, StringComparison.Ordinal) >= 0)) best = 10;
                if (best == 0) return 0;
                score += best;
            }
            if (titles.Any(t => t == q)) score += 200;
            else if (titles.Any(t => t.StartsWith(q, StringComparison.Ordinal))) score += 100;
            score += Math.Min(Math.Max(hitCount, 0), 10) * 5;
            return score;
        }

        /// <summary>الكلمة بـ«ال» وبدونها (كلمة «ال» وحدها كما هى).</summary>
        private static List<string> Forms(string w)
        {
            var list = new List<string> { w };
            if (w.StartsWith("ال", StringComparison.Ordinal) && w.Length > 3) list.Add(w.Substring(2));
            else if (!IsDigits(w) && w.Length >= 2) list.Add("ال" + w);
            return list;
        }

        /// <summary>تهريب قيمة لاستعمالها داخل LIKE (نفس نمط find_entity).</summary>
        public static string EscapeLike(string s) =>
            (s ?? "").Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]");

        /// <summary>
        /// نطاقات الأرقام التى تبدأ بـ n وأطول منه بخانة إلى ثلاث (تستعمل الفهرس بدل LIKE). لا نطاقات لـ n ≤ 0 أو أطول من MaxPrefixDigits أو عند تجاوز long.
        /// </summary>
        public static List<(long From, long To)> PrefixRanges(long n)
        {
            var list = new List<(long From, long To)>();
            if (n <= 0 || n.ToString().Length > AiSearchConst.MaxPrefixDigits) return list;
            long pow = 1;
            for (int k = 1; k <= 3; k++)
            {
                pow *= 10;
                if (n > (long.MaxValue - (pow - 1)) / pow) break;
                long from = n * pow;
                list.Add((from, from + pow - 1));
            }
            return list;
        }

        /// <summary>رابط داخلى آمن للتنقل (س17): يبدأ بـ / وبلا // ولا : ولا \ ولا مسافات أو أحرف تحكم.</summary>
        public static bool IsSafeInternalUrl(string url)
        {
            if (string.IsNullOrEmpty(url) || url.Length > 400 || url[0] != '/') return false;
            if (url.Contains("//") || url.Contains(":") || url.Contains("\\")) return false;
            foreach (char c in url)
                if (char.IsWhiteSpace(c) || char.IsControl(c)) return false;
            return true;
        }

        public static bool IsDigits(string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            foreach (char c in s)
                if (c < '0' || c > '9') return false;
            return true;
        }

        private static long? DocNumberOf(string s)
        {
            if (!IsDigits(s) || s.Length > AiSearchConst.MaxDocDigits) return null;
            long v;
            return long.TryParse(s, out v) ? v : (long?)null;
        }

        private static bool IsAskMark(char c) => c == '?' || c == '؟';
    }
}
