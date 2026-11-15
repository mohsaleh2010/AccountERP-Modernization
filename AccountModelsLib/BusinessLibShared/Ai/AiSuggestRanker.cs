using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace AccountModelsLib
{
    /// <summary>
    /// P8.8.1 (ai-assistant-p88-plan.md §1): تطبيع وتقطيع نص الاقتراح. يعيد استعمال AiSearchText.Normalize (أ/إ/آ، ة، ى، التشكيل، الأرقام).
    /// </summary>
    public static class AiSuggestText
    {
        private static readonly HashSet<string> Stop = new HashSet<string>(new[]
        {
            "مصروف", "مصروفات", "مصاريف", "دفع", "سداد", "قيمه", "عن", "الي", "الى", "الي", "من", "في", "فى", "علي", "على", "و", "ال",
            "شهر", "لشهر", "عدد", "رقم", "مبلغ", "the", "for", "of", "to", "and", "payment", "paid", "fee", "fees"
        }.Select(AiSearchText.Normalize), StringComparer.Ordinal);

        public static string Norm(string s) => AiSearchText.Normalize(s ?? "");

        /// <summary>رموز مميزة (≥ 2 حرف، بلا أرقام خالصة، بلا كلمات وقف). «ال» التعريف تُنزع من أول الكلمة الطويلة.</summary>
        public static string[] Tokens(string norm)
        {
            if (string.IsNullOrEmpty(norm)) return new string[0];
            var list = new List<string>();
            var sb = new StringBuilder();
            foreach (char c in norm + " ")
            {
                if (char.IsLetterOrDigit(c)) { sb.Append(c); continue; }
                if (sb.Length > 0) { AddToken(list, sb.ToString()); sb.Clear(); }
            }
            return list.Distinct(StringComparer.Ordinal).ToArray();
        }

        private static void AddToken(List<string> list, string t)
        {
            if (t.Length > 4 && t.StartsWith("ال", StringComparison.Ordinal)) t = t.Substring(2);
            else if (t.Length > 5 && (t.StartsWith("وال", StringComparison.Ordinal) || t.StartsWith("بال", StringComparison.Ordinal) || t.StartsWith("لل", StringComparison.Ordinal)))
                t = t.StartsWith("لل", StringComparison.Ordinal) ? t.Substring(2) : t.Substring(3);
            if (t.Length < 2 || AiSearchText.IsDigits(t) || Stop.Contains(t)) return;
            list.Add(t);
        }

        /// <summary>ص12: تقليل البيانات — حذف الأرقام الطويلة (≥ 6 أرقام متتالية).</summary>
        public static string Minimize(string s)
        {
            if (string.IsNullOrEmpty(s)) return s ?? "";
            var sb = new StringBuilder(s.Length);
            int run = 0, start = 0;
            for (int i = 0; i <= s.Length; i++)
            {
                bool digit = i < s.Length && (char.IsDigit(s[i]));
                if (digit) { if (run == 0) start = i; run++; continue; }
                if (run > 0)
                {
                    sb.Append(run >= 6 ? "#" : s.Substring(start, run));
                    run = 0;
                }
                if (i < s.Length) sb.Append(s[i]);
            }
            return sb.ToString();
        }

        public static string Hash(string norm)
        {
            using (var sha = SHA256.Create())
            {
                var b = sha.ComputeHash(Encoding.UTF8.GetBytes(norm ?? ""));
                var sb = new StringBuilder(64);
                foreach (var x in b) sb.Append(x.ToString("x2"));
                return sb.ToString();
            }
        }
    }

    /// <summary>فهرس السوابق فى الذاكرة لنوع واحد (ص7): القائمة + فهرس مقلوب + فهرس النص المطبّع.</summary>
    public class AiSuggestCorpus
    {
        public List<AiSuggestPrecedent> Items { get; private set; } = new List<AiSuggestPrecedent>();
        public Dictionary<string, List<int>> Inverted { get; private set; } = new Dictionary<string, List<int>>(StringComparer.Ordinal);
        public Dictionary<string, List<int>> ByNorm { get; private set; } = new Dictionary<string, List<int>>(StringComparer.Ordinal);
        public DateTime BuiltAt { get; set; }
        public string Error { get; set; }

        public int DistinctTexts => ByNorm.Count;

        public static AiSuggestCorpus Build(IEnumerable<AiSuggestPrecedent> rows)
        {
            var c = new AiSuggestCorpus { BuiltAt = DateTime.UtcNow };
            foreach (var p in rows ?? Enumerable.Empty<AiSuggestPrecedent>())
            {
                if (p == null || p.TargetId <= 0 || string.IsNullOrWhiteSpace(p.Text)) continue;
                if (string.IsNullOrEmpty(p.TextNorm)) p.TextNorm = AiSuggestText.Norm(p.Text);
                if (p.Tokens == null || p.Tokens.Length == 0) p.Tokens = AiSuggestText.Tokens(p.TextNorm);
                if (p.Tokens.Length == 0) continue;
                int idx = c.Items.Count;
                c.Items.Add(p);
                foreach (var t in p.Tokens)
                {
                    List<int> l;
                    if (!c.Inverted.TryGetValue(t, out l)) c.Inverted[t] = l = new List<int>();
                    l.Add(idx);
                }
                List<int> n;
                if (!c.ByNorm.TryGetValue(p.TextNorm, out n)) c.ByNorm[p.TextNorm] = n = new List<int>();
                n.Add(idx);
            }
            return c;
        }
    }

    /// <summary>
    /// P8.8.1 (ص2، ص9): ترتيب المرشحين بلا نموذج. الدرجة لكل سابقة = (تطابق تام ×3 أو BM25 مبسط) × وزن الحداثة (نصف عمر 180 يوماً)،
    /// ثم تجميع لكل حساب/فئة، والمركز/المجموعة = الأثقل بين سوابق المرشح. التضمين (اختيارى) يضيف تشابه النص المميز.
    /// </summary>
    public static class AiSuggestRanker
    {
        public const double HalfLifeDays = 180;
        public const double ExactBoost = 3.0;
        public const double MinTokenScoreRatio = 0.35;
        public const double MinVectorSim = 0.72;

        public static double Recency(DateTime date, DateTime now)
        {
            double days = Math.Max(0, (now - date).TotalDays);
            return Math.Pow(0.5, days / HalfLifeDays);
        }

        /// <param name="filter">مرشح على السابقة (نوع الحركة، الصلاحية…)؛ null = الكل.</param>
        /// <param name="vectorSims">تشابه نص الاستعلام مع النصوص المطبّعة (من التضمين)؛ null = لا تضمين.</param>
        public static List<AiSuggestCandidate> Rank(string text, AiSuggestCorpus corpus, DateTime now, Func<AiSuggestPrecedent, bool> filter = null,
            IDictionary<string, double> vectorSims = null, int top = 8)
        {
            var result = new List<AiSuggestCandidate>();
            if (corpus == null || corpus.Items.Count == 0) return result;
            string norm = AiSuggestText.Norm(text);
            var qTokens = AiSuggestText.Tokens(norm);
            if (qTokens.Length == 0 && (vectorSims == null || vectorSims.Count == 0)) return result;

            var scores = new Dictionary<int, double>();
            var exact = new HashSet<int>();

            // (1) تطابق تام بعد التطبيع
            List<int> ex;
            if (corpus.ByNorm.TryGetValue(norm, out ex))
                foreach (var i in ex) { exact.Add(i); scores[i] = ExactBoost; }

            // (2) BM25 مبسط (k1 = 1.2، الوثائق قصيرة فلا تطبيع طول)
            if (qTokens.Length > 0)
            {
                int n = corpus.Items.Count;
                var tok = new Dictionary<int, double>();
                double maxPossible = 0;
                foreach (var t in qTokens)
                {
                    List<int> posting;
                    if (!corpus.Inverted.TryGetValue(t, out posting)) { maxPossible += Idf(n, 0); continue; }
                    double idf = Idf(n, posting.Count);
                    maxPossible += idf;
                    foreach (var i in posting)
                    {
                        double v;
                        tok.TryGetValue(i, out v);
                        tok[i] = v + idf;
                    }
                }
                if (maxPossible > 0)
                    foreach (var kv in tok)
                    {
                        // تغطية الاستعلام وتغطية السابقة معاً (Dice مرجّح) حتى لا تفوز سابقة طويلة بكلمة واحدة
                        var p = corpus.Items[kv.Key];
                        double qCover = kv.Value / maxPossible;
                        int shared = p.Tokens.Count(t => Array.IndexOf(qTokens, t) >= 0);
                        double pCover = p.Tokens.Length == 0 ? 0 : (double)shared / p.Tokens.Length;
                        double s = 2 * qCover * pCover / Math.Max(1e-9, qCover + pCover);
                        if (s < MinTokenScoreRatio) continue;
                        double old;
                        if (!scores.TryGetValue(kv.Key, out old) || old < s) scores[kv.Key] = s;
                    }
            }

            // (3) التضمين: تشابه النص المميز
            if (vectorSims != null)
                foreach (var kv in vectorSims)
                {
                    if (kv.Value < MinVectorSim) continue;
                    List<int> idxs;
                    if (!corpus.ByNorm.TryGetValue(kv.Key, out idxs)) continue;
                    double s = (kv.Value - 0.5) * 2;   // 0.72 → 0.44 … 1.0 → 1.0
                    foreach (var i in idxs)
                    {
                        double old;
                        if (!scores.TryGetValue(i, out old) || old < s) scores[i] = Math.Max(old, s);
                    }
                }

            // تجميع لكل هدف
            var byTarget = new Dictionary<long, AiSuggestCandidate>();
            var secondary = new Dictionary<long, Dictionary<long, double>>();
            var evid = new Dictionary<long, List<KeyValuePair<double, AiSuggestPrecedent>>>();
            foreach (var kv in scores)
            {
                var p = corpus.Items[kv.Key];
                if (filter != null && !filter(p)) continue;
                double w = kv.Value * Recency(p.Date, now);
                AiSuggestCandidate c;
                if (!byTarget.TryGetValue(p.TargetId, out c))
                {
                    byTarget[p.TargetId] = c = new AiSuggestCandidate { TargetId = p.TargetId, TargetCode = p.TargetCode, TargetName = p.TargetName };
                    secondary[p.TargetId] = new Dictionary<long, double>();
                    evid[p.TargetId] = new List<KeyValuePair<double, AiSuggestPrecedent>>();
                }
                c.Score += w;
                if (exact.Contains(kv.Key)) c.ExactHit = true;
                if (p.Date > DateTime.MinValue && string.IsNullOrEmpty(c.TargetName)) c.TargetName = p.TargetName;
                var sec = secondary[p.TargetId];
                double sv;
                sec.TryGetValue(p.SecondaryId, out sv);
                sec[p.SecondaryId] = sv + w;
                evid[p.TargetId].Add(new KeyValuePair<double, AiSuggestPrecedent>(w, p));
            }
            if (byTarget.Count == 0) return result;

            double total = byTarget.Values.Sum(x => x.Score);
            foreach (var c in byTarget.Values)
            {
                c.Share = total <= 0 ? 0 : c.Score / total;
                var best = secondary[c.TargetId].OrderByDescending(x => x.Value).First();
                c.SecondaryId = best.Key;
                if (best.Key != 0)
                    c.SecondaryName = evid[c.TargetId].Where(e => e.Value.SecondaryId == best.Key).Select(e => e.Value.SecondaryName).FirstOrDefault(x => !string.IsNullOrEmpty(x));
                // الأحدث للعرض (أسماء الحساب قد تتغير)
                var latest = evid[c.TargetId].OrderByDescending(e => e.Value.Date).First().Value;
                if (!string.IsNullOrEmpty(latest.TargetName)) c.TargetName = latest.TargetName;
                if (!string.IsNullOrEmpty(latest.TargetCode)) c.TargetCode = latest.TargetCode;
                c.Evidence = evid[c.TargetId].OrderByDescending(e => e.Key).ThenByDescending(e => e.Value.Date)
                    .GroupBy(e => e.Value.TextNorm).Select(g => g.First().Value).Take(3)
                    .Select(p => new AiSuggestEvidence { Date = p.Date, Text = p.Text, TargetName = p.TargetName }).ToList();
            }
            return byTarget.Values.OrderByDescending(x => x.Score).ThenBy(x => x.TargetId).Take(Math.Max(1, top)).ToList();
        }

        private static double Idf(int n, int df) => Math.Log(1 + (n - df + 0.5) / (df + 0.5));

        /// <summary>الثقة (ص9): نصيب الأول، مع رفع خفيف للتطابق التام المتكرر.</summary>
        public static double Confidence(IList<AiSuggestCandidate> ranked)
        {
            if (ranked == null || ranked.Count == 0) return 0;
            double c = ranked[0].Share;
            if (ranked[0].ExactHit) c = Math.Min(1, c + 0.1);
            return Math.Round(c, 4);
        }

        /// <summary>ص2: غموض = لا مرشحين، أو الفرق بين الأول والثانى أقل من الهامش ولا تطابق تام للأول.</summary>
        public static bool IsAmbiguous(IList<AiSuggestCandidate> ranked, double margin)
        {
            if (ranked == null || ranked.Count == 0) return true;
            if (ranked.Count == 1) return false;
            if (ranked[0].ExactHit && !ranked[1].ExactHit) return false;
            return ranked[0].Share - ranked[1].Share < margin;
        }
    }
}
