using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace AccountModelsLib
{
    /// <summary>
    /// P8.3 (§2): محلّل ومطابق أخطاء ETA — نقى بلا قاعدة بيانات ولا نموذج (ف3، ف4).
    /// Parse يستخرج الأكواد ومسارات الحقول وخطوات التحقق والنص المطبّع من رسالة ملصوقة (JSON رد البوابة، نص خطوات التحقق، رسائل النظام).
    /// Match يقابلها بمفاتيح Keywords فى مقاطع eta_error بصيغة «code:… ; path:… ; step:… ; phrase:…» (ف19).
    /// </summary>
    public static class AiEtaErrorMatcher
    {
        private static readonly Regex StepLine = new Regex(@"֎\s*(?<name>[^\r\n֎]+?)\s*Status\s*:\s*(?<status>[A-Za-z]+)", RegexOptions.Compiled);
        private static readonly Regex HttpCode = new Regex(@"(?<![\d.])(400|401|403|404|413|422|429|500|502|503)(?![\d.])", RegexOptions.Compiled);
        private static readonly Regex CodeToken = new Regex(@"\b([A-Z]{1,4}\d{2,4}|[A-Za-z]+(?:Exceeded|Submitter|Structure|Submission|Validator|NotFound|NotValid))\b", RegexOptions.Compiled);
        private static readonly Regex Index = new Regex(@"\[\d+\]", RegexOptions.Compiled);

        private static readonly string[][] HttpWords =
        {
            new[] { "unauthorized", "http_401" }, new[] { "invalid_client", "http_401" }, new[] { "forbidden", "http_403" },
            new[] { "toomanyrequests", "http_429" }, new[] { "too many requests", "http_429" }, new[] { "unprocessable", "http_422" },
            new[] { "internal server error", "http_500" }, new[] { "service unavailable", "http_503" }, new[] { "request entity too large", "http_413" },
        };

        public static AiEtaSignals Parse(string raw)
        {
            var s = new AiEtaSignals();
            string text = (raw ?? "").Trim();
            if (text.Length == 0) return s;
            if (text.Length > AiEInvoiceConst.MaxErrorChars) text = text.Substring(0, AiEInvoiceConst.MaxErrorChars);

            var fragments = new List<string>();

            // 1) JSON (متسامح: أول { إلى آخر })
            int a = text.IndexOf('{'), b = text.LastIndexOf('}');
            if (a >= 0 && b > a)
            {
                try
                {
                    var tok = JToken.Parse(text.Substring(a, b - a + 1));
                    s.IsJson = true;
                    Walk(tok, s, fragments);
                }
                catch (Exception) { /* ليس JSON صالحاً: يكمل النص */ }
            }

            // 2) نص خطوات التحقق «֎ name Status: Invalid»
            foreach (Match m in StepLine.Matches(text))
            {
                string status = m.Groups["status"].Value;
                if (!string.Equals(status, "Valid", StringComparison.OrdinalIgnoreCase))
                    AddUnique(s.Steps, AiSearchText.Normalize(m.Groups["name"].Value));
            }

            // 3) رموز HTTP وكلماتها
            foreach (Match m in HttpCode.Matches(text)) AddUnique(s.Codes, "http_" + m.Value);
            string lower = text.ToLowerInvariant();
            foreach (var w in HttpWords)
                if (lower.Contains(w[0])) AddUnique(s.Codes, w[1]);

            // 4) رموز بشكل الكود
            foreach (Match m in CodeToken.Matches(text)) AddUnique(s.Codes, m.Value.ToLowerInvariant());

            s.NormalizedText = AiSearchText.Normalize(text);

            foreach (var line in text.Split(new[] { '\r', '\n', '֎' }, StringSplitOptions.RemoveEmptyEntries))
                fragments.Add(line);
            foreach (var f in fragments)
            {
                string t = (f ?? "").Trim().Trim('"', ',', '{', '}', '[', ']').Trim();
                if (t.Length < 3) continue;
                if (t.Length > AiEInvoiceConst.MaxFragment) t = t.Substring(0, AiEInvoiceConst.MaxFragment);
                if (!s.Fragments.Contains(t)) s.Fragments.Add(t);
                if (s.Fragments.Count >= AiEInvoiceConst.MaxFragments) break;
            }
            return s;
        }

        private static void Walk(JToken tok, AiEtaSignals s, List<string> fragments)
        {
            var obj = tok as JObject;
            if (obj != null)
            {
                var own = new List<string>();
                string code = Str(obj, "code");
                if (code.Length > 0) { AddUnique(s.Codes, code.ToLowerInvariant()); own.Add(code.ToLowerInvariant()); }
                foreach (var k in new[] { "propertyPath", "target" })
                {
                    string p = Str(obj, k);
                    if (p.Length > 0) { AddUnique(s.Paths, NormPath(p)); own.Add(NormPath(p)); }
                }
                string msg = Str(obj, "message");
                if (msg.Length > 0)
                {
                    fragments.Add(msg);
                    string fk = msg.Trim();
                    if (fk.Length > AiEInvoiceConst.MaxFragment) fk = fk.Substring(0, AiEInvoiceConst.MaxFragment);
                    List<string> list;
                    if (!s.FragmentKeys.TryGetValue(fk, out list)) s.FragmentKeys[fk] = list = new List<string>();
                    foreach (var o in own) if (!list.Contains(o)) list.Add(o);
                }
                // خطوة تحقق: { name, status, error }
                string name = Str(obj, "name"), status = Str(obj, "status");
                if (name.Length > 0 && status.Length > 0 && !string.Equals(status, "Valid", StringComparison.OrdinalIgnoreCase))
                    AddUnique(s.Steps, AiSearchText.Normalize(name));
                foreach (var p in obj.Properties()) Walk(p.Value, s, fragments);
                return;
            }
            var arr = tok as JArray;
            if (arr != null) foreach (var x in arr) Walk(x, s, fragments);
        }

        private static string Str(JObject o, string name)
        {
            JToken v;
            if (!o.TryGetValue(name, StringComparison.OrdinalIgnoreCase, out v) || v == null) return "";
            if (v.Type != JTokenType.String && v.Type != JTokenType.Integer) return "";
            return (v.ToString() ?? "").Trim();
        }

        /// <summary>مسار حقل مطبّع: أحرف صغيرة بلا فهارس المصفوفات (invoiceLines[0].itemCode ⇒ invoicelines.itemcode).</summary>
        public static string NormPath(string p) => Index.Replace((p ?? "").Trim(), "").ToLowerInvariant().Trim('.');

        private static void AddUnique(List<string> list, string v)
        {
            if (!string.IsNullOrEmpty(v) && !list.Contains(v)) list.Add(v);
        }

        /// <summary>مفاتيح المطابقة من Keywords: «code:a|b ; path:x ; step:y ; phrase:z». الأجزاء بلا بادئة معروفة تُهمل.</summary>
        public static List<AiEtaKey> ParseKeys(string keywords)
        {
            var list = new List<AiEtaKey>();
            foreach (var seg in (keywords ?? "").Split(';'))
            {
                int c = seg.IndexOf(':');
                if (c <= 0) continue;
                string type = seg.Substring(0, c).Trim().ToLowerInvariant();
                if (type != AiEtaMatchTypes.Code && type != AiEtaMatchTypes.Path && type != AiEtaMatchTypes.Step && type != AiEtaMatchTypes.Phrase) continue;
                foreach (var raw in seg.Substring(c + 1).Split('|'))
                {
                    string v = raw.Trim();
                    if (v.Length == 0) continue;
                    v = type == AiEtaMatchTypes.Path ? NormPath(v) : type == AiEtaMatchTypes.Code ? v.ToLowerInvariant() : AiSearchText.Normalize(v);
                    if (type == AiEtaMatchTypes.Phrase && v.Length < 6) continue;
                    if (v.Length > 0) list.Add(new AiEtaKey { Type = type, Value = v });
                }
            }
            return list;
        }

        /// <summary>المطابقة الثابتة (ف4): أدق نوع لكل مقطع، ثم الترتيب بالنوع وموضع الظهور فى النص؛ حتى MaxMatches.</summary>
        public static List<AiEtaMatch> Match(AiEtaSignals s, IEnumerable<AiKnowledgeRow> rows)
        {
            var found = new List<KeyValuePair<int, AiEtaMatch>>();
            if (s == null || s.IsEmpty || rows == null) return new List<AiEtaMatch>();
            foreach (var r in rows)
            {
                AiEtaMatch best = null;
                foreach (var k in ParseKeys(r.Keywords))
                {
                    bool hit = false;
                    if (k.Type == AiEtaMatchTypes.Code) hit = s.Codes.Contains(k.Value);
                    else if (k.Type == AiEtaMatchTypes.Path) hit = s.Paths.Any(p => p == k.Value || p.EndsWith("." + k.Value, StringComparison.Ordinal));
                    else if (k.Type == AiEtaMatchTypes.Step) hit = s.Steps.Any(x => x.Contains(k.Value)) || s.NormalizedText.Contains(k.Value + " status");
                    else if (k.Type == AiEtaMatchTypes.Phrase) hit = s.NormalizedText.Contains(k.Value);
                    if (!hit) continue;
                    if (best == null || AiEtaMatchTypes.Rank(k.Type) < AiEtaMatchTypes.Rank(best.MatchType))
                        best = new AiEtaMatch { RowId = r.Id, Key = r.KeyName, Title = r.TitleAr, Content = r.Content, MatchType = k.Type, MatchedOn = k.Value };
                }
                if (best == null) continue;
                int pos = Position(s, best);
                found.Add(new KeyValuePair<int, AiEtaMatch>(pos, best));
            }
            return found
                .GroupBy(x => x.Value.Key, StringComparer.OrdinalIgnoreCase).Select(g => g.First())
                .OrderBy(x => AiEtaMatchTypes.Rank(x.Value.MatchType)).ThenBy(x => x.Key)
                .Take(AiEInvoiceConst.MaxMatches).Select(x => x.Value).ToList();
        }

        private static int Position(AiEtaSignals s, AiEtaMatch m)
        {
            int i;
            if (m.MatchType == AiEtaMatchTypes.Code) i = s.Codes.IndexOf(m.MatchedOn);
            else if (m.MatchType == AiEtaMatchTypes.Path) i = s.Paths.FindIndex(p => p == m.MatchedOn || p.EndsWith("." + m.MatchedOn, StringComparison.Ordinal));
            else i = s.NormalizedText.IndexOf(m.MatchedOn, StringComparison.Ordinal);
            return i < 0 ? int.MaxValue : i;
        }

        /// <summary>أسطر النص التى لم تفسّرها أى مطابقة (حتى MaxUnmatched).</summary>
        public static List<string> Unmatched(AiEtaSignals s, IList<AiEtaMatch> matches)
        {
            var list = new List<string>();
            if (s == null) return list;
            var values = (matches ?? new List<AiEtaMatch>()).Select(m => m.MatchedOn).Where(v => !string.IsNullOrEmpty(v)).ToList();
            foreach (var f in s.Fragments)
            {
                string n = AiSearchText.Normalize(f), p = NormPath(f);
                List<string> own;
                s.FragmentKeys.TryGetValue(f, out own);
                bool covered = values.Any(v => n.Contains(v) || p.Contains(v)
                    || (own != null && own.Any(o => o == v || o.EndsWith("." + v, StringComparison.Ordinal))));
                if (!covered) list.Add(f);
                if (list.Count >= AiEInvoiceConst.MaxUnmatched) break;
            }
            return list;
        }
    }
}
