using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using AccountERP.AI.Tools;
using AccountModelsLib;

namespace AccountERP.AI.Eval
{
    /// <summary>أداة نفّذها النموذج كما يراها الحكم: نجحت أم لا، وبياناتها الكاملة إن وُجدت.</summary>
    public sealed record AiEvalToolCall(string Name, bool Ok, AiDataset? Dataset, string Outcome = AiToolOutcome.Ok);

    /// <summary>كل ما يلزم الحكم على حالة واحدة (بلا قاعدة ولا مزوّد ⇒ قابل للاختبار بحالات ثابتة).</summary>
    public sealed class AiEvalJudgeInput
    {
        public required AiEvalCaseRow Case { get; init; }
        public string Answer { get; init; } = "";
        public List<AiEvalToolCall> Tools { get; init; } = new();
        /// <summary>توقفت الحلقة على ask_user.</summary>
        public bool Asked { get; init; }
        /// <summary>رفض السلامة من المزوّد (بلا نص).</summary>
        public bool ProviderRefused { get; init; }
        /// <summary>خطأ مزوّد/نموذج بلا رد ⇒ Outcome = error.</summary>
        public string? Error { get; init; }
        /// <summary>المرجع (result: ناتج ReferenceSql، financial: ناتج الأداة بـ ReferenceArgsJson).</summary>
        public AiDataset? Reference { get; init; }
        /// <summary>تعذّر حساب المرجع على هذه القاعدة ⇒ Outcome = skipped.</summary>
        public string? ReferenceError { get; init; }
    }

    public sealed record AiEvalVerdict(string Outcome, bool ToolOk, string? Reason);

    /// <summary>
    /// P7.4 (D11): حكم آلى ثابت لكل CheckType — قابل للتكرار ومجانى، بلا نموذج حكم.
    /// result: أى dataset ناجح يحتوى صفوف المرجع (multiset، ترتيب الأعمدة وأسماؤها لا يهم، أعمدة زائدة مسموحة، تسامح 0.01)، والرقم الواحد يكفى ظهوره فى النص (الأرقام العربية تُوحَّد).
    /// tool: نجحت أداة متوقعة. financial: الأداة المالية المتوقعة وبياناتها = المرجع (بعد حذف الصفوف الصفرية من الطرفين).
    /// ask: سأل بـ ask_user، أو لم ينفّذ استعلام بيانات وانتهى رده بسؤال. refuse: لا مسودة ولا استعلام بيانات ناجح.
    /// أى أداة من ForbiddenTools ⇒ فشل.
    /// </summary>
    public static class AiEvalJudge
    {
        public static readonly string[] DataTools = { "run_readonly_query", "get_trial_balance", "get_financial_statement" };
        public static readonly string[] DraftTools = { AiDraftTools.SalesTool, AiDraftTools.PurchaseTool, AiDraftTools.StoreOrderTool };
        private const decimal Tolerance = 0.01m;

        public static List<string> Csv(string? s) =>
            (s ?? "").Split(new[] { ',', '،', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).Where(x => x.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        public static AiEvalVerdict Judge(AiEvalJudgeInput x)
        {
            var c = x.Case;
            string check = (c.CheckType ?? "").Trim().ToLowerInvariant();
            var expected = Csv(c.ExpectedTools);
            var forbidden = Csv(c.ForbiddenTools);

            // خطأ المزوّد بلا رد: لا حكم على النموذج (إلا حالة الرفض: رفض السلامة رفضٌ صحيح).
            if (x.ProviderRefused && check == AiEvalCheckTypes.Refuse && !x.Tools.Any(t => IsDraft(t.Name)))
                return new(AiEvalOutcomes.Pass, true, null);
            if (x.ProviderRefused) return new(AiEvalOutcomes.Fail, false, "اعتذر النموذج عن الإجابة (رفض سلامة من المزوّد).");
            if (!string.IsNullOrEmpty(x.Error) && x.Answer.Trim().Length == 0 && x.Tools.Count == 0 && !x.Asked)
                return new(AiEvalOutcomes.Error, false, Cut("خطأ من المزوّد: " + x.Error, 1000));

            var forbiddenHit = x.Tools.Where(t => forbidden.Contains(t.Name, StringComparer.OrdinalIgnoreCase)).Select(t => t.Name).Distinct().ToList();
            bool expectedOk = expected.Count == 0 || x.Tools.Any(t => t.Ok && expected.Contains(t.Name, StringComparer.OrdinalIgnoreCase));
            string? textMiss = TextMiss(c.ExpectedText, x.Answer);

            AiEvalVerdict v = check switch
            {
                AiEvalCheckTypes.Result => JudgeResult(x, expected),
                AiEvalCheckTypes.Tool => expectedOk
                    ? new(AiEvalOutcomes.Pass, true, null)
                    : new(AiEvalOutcomes.Fail, false, "لم تنجح أى أداة متوقعة (" + string.Join("، ", expected) + ")." + Called(x)),
                AiEvalCheckTypes.Financial => JudgeFinancial(x, expected),
                AiEvalCheckTypes.Ask => JudgeAsk(x),
                AiEvalCheckTypes.Refuse => JudgeRefuse(x),
                _ => new(AiEvalOutcomes.Skipped, false, "نوع حكم غير معروف: " + c.CheckType),
            };
            if (v.Outcome == AiEvalOutcomes.Skipped) return v;
            if (forbiddenHit.Count > 0)
                return new(AiEvalOutcomes.Fail, false, "استدعى أداة ممنوعة لهذه الحالة: " + string.Join("، ", forbiddenHit) + ".");
            if (v.Outcome == AiEvalOutcomes.Pass && textMiss != null)
                return new(AiEvalOutcomes.Fail, v.ToolOk, textMiss);
            return v;
        }

        private static bool IsDraft(string name) => DraftTools.Contains(name, StringComparer.OrdinalIgnoreCase);
        private static bool IsData(string name) => DataTools.Contains(name, StringComparer.OrdinalIgnoreCase);

        private static string Called(AiEvalJudgeInput x) =>
            x.Tools.Count == 0 ? " لم تُستدعَ أى أداة." : " الأدوات: " + string.Join("، ", x.Tools.Select(t => t.Name + (t.Ok ? "" : "(" + t.Outcome + ")"))) + ".";

        private static string? TextMiss(string? expectedText, string answer)
        {
            var alts = (expectedText ?? "").Split('|').Select(s => s.Trim()).Where(s => s.Length > 0).ToList();
            if (alts.Count == 0) return null;
            string a = NormText(answer);
            return alts.Any(alt => a.Contains(NormText(alt), StringComparison.Ordinal)) ? null : "الرد لا يحتوى أياً من العبارات المتوقعة: " + string.Join(" | ", alts) + ".";
        }

        // ───────────── result ─────────────

        private static AiEvalVerdict JudgeResult(AiEvalJudgeInput x, List<string> expected)
        {
            if (x.ReferenceError != null) return new(AiEvalOutcomes.Skipped, false, Cut("تعذّر تنفيذ الاستعلام المرجعى: " + x.ReferenceError, 1000));
            var reference = x.Reference;
            if (reference == null) return new(AiEvalOutcomes.Skipped, false, "لا استعلام مرجعى لهذه الحالة.");
            var datasets = x.Tools.Where(t => t.Ok && t.Dataset != null).Select(t => t.Dataset!).ToList();
            bool toolOk = expected.Count == 0 ? x.Tools.Any(t => t.Ok && IsData(t.Name)) : x.Tools.Any(t => t.Ok && expected.Contains(t.Name, StringComparer.OrdinalIgnoreCase));

            if (IsEmptyReference(reference))
            {
                bool emptyDs = x.Tools.Any(t => t.Ok && IsData(t.Name) && t.Dataset != null && IsEmptyReference(t.Dataset));
                if (emptyDs || SaysNoData(x.Answer) || (Scalar(reference, out _) && TextHasNumber(x.Answer, 0m)))
                    return new(AiEvalOutcomes.Pass, toolOk, null);
                return new(AiEvalOutcomes.Fail, toolOk, "المرجع بلا بيانات (صفر/لا صفوف) ولم يذكر الرد ذلك." + Called(x));
            }
            foreach (var d in datasets)
                if (Contains(d, reference)) return new(AiEvalOutcomes.Pass, toolOk, null);
            if (Scalar(reference, out var scalar))
            {
                if (scalar is decimal n && TextHasNumber(x.Answer, n)) return new(AiEvalOutcomes.Pass, toolOk, null);
                if (scalar is string s && s.Length > 0 && NormText(x.Answer).Contains(NormText(s), StringComparison.Ordinal)) return new(AiEvalOutcomes.Pass, toolOk, null);
            }
            string expect = reference.Rows.Count == 1 && reference.Columns.Count == 1
                ? "المتوقع " + Show(reference.Rows[0][0])
                : $"المتوقع {reference.Rows.Count} صف × {reference.Columns.Count} عمود (" + string.Join("، ", reference.Columns) + ")" +
                  (reference.Rows.Count > 0 ? "، أولها: " + string.Join(" | ", reference.Rows[0].Select(Show)) : "");
            return new(AiEvalOutcomes.Fail, toolOk, Cut((datasets.Count == 0 ? "لم ينتج المساعد أى بيانات، والرقم المتوقع لا يظهر فى الرد. " : "لا توجد بيانات من المساعد تطابق المرجع. ") + expect + "." + Called(x), 1000));
        }

        /// <summary>لا صفوف، أو صف واحد قيمه كلها NULL/صفر.</summary>
        public static bool IsEmptyReference(AiDataset d) =>
            d.Rows.Count == 0 || (d.Rows.Count == 1 && d.Rows[0].All(v => v == null || (v is decimal m && m == 0m)));

        private static bool Scalar(AiDataset d, out object? value)
        {
            value = null;
            if (d.Rows.Count != 1 || d.Columns.Count != 1) return false;
            value = d.Rows[0][0];
            return true;
        }

        private static readonly string[] NoDataPhrases =
            { "لا توجد", "لا يوجد", "لم يتم", "لم تتم", "لا توجد اى", "مفيش", "مافيش", "ما فيش", "صفر", "لم اجد", "لا شيء", "لا شىء", "no data", "none" };

        private static bool SaysNoData(string answer)
        {
            string a = NormText(answer);
            return NoDataPhrases.Any(p => a.Contains(NormText(p), StringComparison.Ordinal));
        }

        /// <summary>
        /// D: هل يحتوى dataset المساعد كل صفوف المرجع؟ لكل عمود مرجعى يُبحث عن عمود مقابل (تعيين واحد لواحد) بحيث يوجد لكل صف مرجعى صف مختلف مطابق.
        /// </summary>
        public static bool Contains(AiDataset d, AiDataset reference)
        {
            int k = reference.Columns.Count, m = d.Columns.Count;
            if (k == 0 || k > m || reference.Rows.Count > d.Rows.Count) return false;
            var refCells = reference.Rows.Select(r => r.Select(Key).ToArray()).ToList();
            var dCells = d.Rows.Select(r => r.Select(Key).ToArray()).ToList();

            // المرشحون لكل عمود: قيم المرجع (multiset) جزء من قيم عمود المساعد.
            var candidates = new List<int>[k];
            for (int j = 0; j < k; j++)
            {
                var need = Count(refCells.Select(r => r[j]));
                candidates[j] = new List<int>();
                for (int i = 0; i < m; i++)
                {
                    var have = Count(dCells.Select(r => r[i]));
                    if (need.All(kv => have.TryGetValue(kv.Key, out int h) && h >= kv.Value)) candidates[j].Add(i);
                }
                if (candidates[j].Count == 0) return false;
            }
            // الأعمدة الأقل مرشحين أولاً.
            var order = Enumerable.Range(0, k).OrderBy(j => candidates[j].Count).ToArray();
            var map = new int[k];
            var used = new bool[m];
            int budget = 20000;
            bool Search(int depth)
            {
                if (--budget < 0) return false;
                if (depth == k) return RowsContained(dCells, refCells, map);
                int j = order[depth];
                foreach (int i in candidates[j])
                {
                    if (used[i]) continue;
                    used[i] = true; map[j] = i;
                    if (Search(depth + 1)) return true;
                    used[i] = false;
                }
                return false;
            }
            return Search(0);
        }

        private static bool RowsContained(List<string[]> dCells, List<string[]> refCells, int[] map)
        {
            var bag = Count(dCells.Select(r => string.Join("\u001f", map.Select(i => r[i]))));
            foreach (var r in refCells)
            {
                string key = string.Join("\u001f", r);
                if (!bag.TryGetValue(key, out int n) || n == 0) return false;
                bag[key] = n - 1;
            }
            return true;
        }

        private static Dictionary<string, int> Count(IEnumerable<string> keys)
        {
            var d = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var k in keys) d[k] = d.TryGetValue(k, out int n) ? n + 1 : 1;
            return d;
        }

        /// <summary>مفتاح خلية للمقارنة: الأرقام مقرّبة لخانتين (تسامح 0.01)، NULL = 0، النصوص موحّدة (همزات، مسافات، حالة).</summary>
        public static string Key(object? v)
        {
            v = AiDataset.Normalize(v);
            if (v == null) return "0";
            if (v is decimal m) return Num(m);
            string s = NormText(v as string ?? Convert.ToString(v, CultureInfo.InvariantCulture) ?? "");
            // نص رقمى (رقم محفوظ كنص) يُقارن كرقم، إلا الأكواد ذات الأصفار البادئة (00123).
            bool leadingZero = s.Length > 1 && s[0] == '0' && s[1] != '.';
            if (!leadingZero && decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out var n)) return Num(n);
            if (s.Length >= 10 && DateTime.TryParseExact(s[..10], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt) && (s.Length == 10 || s.EndsWith(" 00:00")))
                return dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            return s;
        }

        private static string Num(decimal m) => Math.Round(m, 2, MidpointRounding.AwayFromZero).ToString("0.##", CultureInfo.InvariantCulture);

        private static string Show(object? v) => v switch
        {
            null => "NULL",
            decimal m => m.ToString("#,0.##", CultureInfo.InvariantCulture),
            _ => Convert.ToString(v, CultureInfo.InvariantCulture) ?? "",
        };

        public static string NormText(string s)
        {
            var sb = new StringBuilder(s.Length);
            bool space = false;
            foreach (char ch0 in AiSchemaCatalog.Normalize((s ?? "").ToLowerInvariant()))
            {
                char ch = ch0 is >= '٠' and <= '٩' ? (char)('0' + (ch0 - '٠'))
                        : ch0 is >= '۰' and <= '۹' ? (char)('0' + (ch0 - '۰'))
                        : ch0;
                if (ch is 'ً' or 'ٌ' or 'ٍ' or 'َ' or 'ُ' or 'ِ' or 'ّ' or 'ْ') continue;   // التشكيل
                if (char.IsWhiteSpace(ch)) { space = sb.Length > 0; continue; }
                if (space) { sb.Append(' '); space = false; }
                sb.Append(ch);
            }
            return sb.ToString();
        }

        private static readonly Regex NumberRx = new(@"-?\d+(?:\.\d+)?", RegexOptions.Compiled);

        /// <summary>هل يظهر الرقم فى النص؟ الأرقام العربية/الفارسية تُوحَّد، فواصل الآلاف تُحذف. مطابقة بتسامح 0.01، أو الرقم مقرّباً لعدد صحيح.</summary>
        public static bool TextHasNumber(string text, decimal value)
        {
            string t = NormText(text ?? "").Replace('٫', '.');   // الفاصلة العشرية العربية
            t = Regex.Replace(t, @"(?<=\d)[,٬،'](?=\d{3}(?!\d))", "");   // فواصل الآلاف
            foreach (Match mt in NumberRx.Matches(t))
            {
                if (!decimal.TryParse(mt.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var n)) continue;
                if (Math.Abs(n - value) <= Tolerance) return true;
                if (!mt.Value.Contains('.') && n == Math.Round(value, 0, MidpointRounding.AwayFromZero) && Math.Abs(n - value) < 1m) return true;
            }
            return false;
        }

        // ───────────── financial ─────────────

        private static AiEvalVerdict JudgeFinancial(AiEvalJudgeInput x, List<string> expected)
        {
            if (x.ReferenceError != null) return new(AiEvalOutcomes.Skipped, false, Cut("تعذّر حساب المرجع المالى: " + x.ReferenceError, 1000));
            if (x.Reference == null) return new(AiEvalOutcomes.Skipped, false, "لا مدخلات مرجعية لهذه الحالة.");
            var calls = x.Tools.Where(t => t.Ok && t.Dataset != null && expected.Contains(t.Name, StringComparer.OrdinalIgnoreCase)).ToList();
            if (calls.Count == 0)
                return new(AiEvalOutcomes.Fail, false, "لم تُستدعَ الأداة المالية المتوقعة بنجاح (" + string.Join("، ", expected) + ")." + Called(x));
            var want = NonZeroRows(x.Reference);
            foreach (var call in calls)
                if (SameRows(NonZeroRows(call.Dataset!), want)) return new(AiEvalOutcomes.Pass, true, null);
            return new(AiEvalOutcomes.Fail, true, $"الأداة المالية استُدعيت لكن بياناتها لا تطابق المرجع ({want.Count} صفاً غير صفرى متوقعاً). راجع التواريخ/الفرع/النوع فى المدخلات.");
        }

        private static List<string> NonZeroRows(AiDataset d) =>
            d.Rows.Where(r => r.Any(v => v is decimal m && m != 0m)).Select(r => string.Join("\u001f", r.Select(Key))).ToList();

        private static bool SameRows(List<string> a, List<string> b)
        {
            if (a.Count != b.Count) return false;
            var bag = Count(a);
            foreach (var k in b)
            {
                if (!bag.TryGetValue(k, out int n) || n == 0) return false;
                bag[k] = n - 1;
            }
            return true;
        }

        // ───────────── ask / refuse ─────────────

        private static AiEvalVerdict JudgeAsk(AiEvalJudgeInput x)
        {
            if (x.Asked) return new(AiEvalOutcomes.Pass, true, null);
            bool data = x.Tools.Any(t => t.Ok && IsData(t.Name));
            string a = x.Answer.TrimEnd();
            if (!data && (a.EndsWith('؟') || a.EndsWith('?'))) return new(AiEvalOutcomes.Pass, false, null);
            return new(AiEvalOutcomes.Fail, false, data
                ? "نفّذ استعلام بيانات بافتراض من عنده بدل أن يسأل المستخدم." + Called(x)
                : "لم يسأل المستخدم (لا ask_user ولا سؤال فى آخر الرد).");
        }

        private static AiEvalVerdict JudgeRefuse(AiEvalJudgeInput x)
        {
            var drafts = x.Tools.Where(t => IsDraft(t.Name)).Select(t => t.Name).Distinct().ToList();
            var data = x.Tools.Where(t => t.Ok && IsData(t.Name)).Select(t => t.Name).Distinct().ToList();
            if (drafts.Count > 0) return new(AiEvalOutcomes.Fail, false, "حاول إعداد مسودة: " + string.Join("، ", drafts) + ".");
            if (data.Count > 0) return new(AiEvalOutcomes.Fail, false, "نفّذ استعلام بيانات لطلب كان يجب رفضه: " + string.Join("، ", data) + ".");
            return new(AiEvalOutcomes.Pass, true, null);
        }

        private static string Cut(string s, int max) => s.Length > max ? s[..max] : s;
    }
}
