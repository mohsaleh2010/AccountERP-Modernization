using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using AccountERP.AI.Chat;
using AccountERP.AI.Collections;
using AccountModelsLib;

namespace AccountERP.AI.Tools
{
    /// <summary>
    /// P8.5.5 (ai-assistant-p85-plan.md ت30): أداة get_collections_priority — نفس حاسب الشاشة ونطاق المستخدم (فروعه وصلاحيته الضريبية)،
    /// قراءة فقط: لا مسودة ولا إرسال ولا رقم هاتف. لمن يملك شاشة أولويات التحصيل.
    /// </summary>
    public static class AiCollectTools
    {
        public const int DefaultTop = 10, MaxTop = 20;
        private static readonly JsonSerializerOptions Json = new() { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

        public static AiToolDef Def() => new()
        {
            Name = AiCollectConst.ToolName,
            Description = "قائمة أولويات التحصيل كما فى شاشة النظام: العملاء المدينون مرتبون بدرجة ثابتة (عمر الدين، المبلغ المتأخر، سلوك السداد، حد الائتمان، آخر تحصيل) " +
                          "مع شرائح الأعمار وحالة المتابعة. للقراءة فقط؛ لا ترسل رسائل ولا تكتب مسودات. لفتح الشاشة استخدم open_screen بالمسار " + AiCollectConst.Route + ".",
            Schema = new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    ["top"] = new JsonObject { ["type"] = "integer", ["description"] = "عدد العملاء (1–20، الافتراضى 10)" },
                    ["priority"] = new JsonObject { ["type"] = "string", ["description"] = "all / high / medium / low", ["enum"] = new JsonArray("all", "high", "medium", "low") },
                    ["branch_id"] = new JsonObject { ["type"] = "integer", ["description"] = "فرع واحد من فروع المستخدم (اختيارى)" },
                    ["customer_id"] = new JsonObject { ["type"] = "integer", ["description"] = "عميل واحد (اختيارى)" },
                },
            },
        };

        public static bool Allowed(AiToolContext c) => c.HasRoute(AiCollectConst.Route);

        public static AiToolResult Run(JsonObject a, AiToolContext c, AiCollectionsScorer scorer, Func<AiCollectSettings>? settings = null, Func<AiToolContext, List<long>?>? branches = null)
        {
            int top = Int(a, "top") is long t && t > 0 ? (int)Math.Min(MaxTop, t) : DefaultTop;
            string pr = Str(a, "priority");
            long branch = Int(a, "branch_id") ?? 0;
            long customer = Int(a, "customer_id") ?? 0;
            var s = settings?.Invoke() ?? AiCollectSettings.From(new AiProvidersCLS().GetSettings());
            var scope = new AiCollectScope
            {
                UserId = c.User.UserId, IsAdmin = c.User.IsAdmin, TaxFilter = c.TaxFilter,
                BranchIds = branches != null ? branches(c) : c.User.IsAdmin ? null
                          : new AccountModelsLib.BusinessLibShared.GeneralLedger.FinalAccountsService().GetBranches().Select(b => b.Id).Distinct().ToList(),
            };
            if (branch != 0)
            {
                if (scope.BranchIds != null && !scope.BranchIds.Contains(branch))
                    return new AiToolResult { Content = JsonSerializer.Serialize(new { customers = Array.Empty<object>(), message = "الفرع خارج فروعك." }, Json), Title = Title };
                scope.BranchIds = new List<long> { branch };
            }
            var res = scorer.Compute(scope, s, false, c.Ct);
            IEnumerable<AiCollectRow> rows = res.Rows;
            if (pr is AiCollectPriority.High or AiCollectPriority.Medium or AiCollectPriority.Low) rows = rows.Where(r => r.Priority == pr);
            if (customer != 0) rows = rows.Where(r => r.CustomerId == customer);
            var list = rows.Take(top).ToList();
            var inv = CultureInfo.InvariantCulture;
            var payload = new JsonObject
            {
                ["as_of"] = res.ComputedAt.ToString("yyyy-MM-dd HH:mm", inv),
                ["currency_note"] = "المبالغ بعملة حساب كل عميل",
                ["tax_restricted"] = res.TaxRestricted,
                ["truncated"] = res.Truncated,
                ["screen"] = AiCollectConst.Route,
                ["totals"] = new JsonObject
                {
                    ["overdue"] = Math.Round(res.Rows.Sum(r => r.Overdue), 2),
                    ["overdue_customers"] = res.Rows.Count(r => r.Overdue > AiCollectRules.Tolerance),
                    ["high_count"] = res.Rows.Count(r => r.Priority == AiCollectPriority.High),
                    ["buckets"] = Buckets(res.Rows.Sum(r => r.NotDue), res.Rows.Sum(r => r.D1_30), res.Rows.Sum(r => r.D31_60), res.Rows.Sum(r => r.D61_90), res.Rows.Sum(r => r.DOver90), res.Rows.Sum(r => r.Unallocated)),
                },
                ["customers"] = new JsonArray(list.Select(r => (JsonNode)new JsonObject
                {
                    ["customer_id"] = r.CustomerId, ["code"] = r.Code, ["name"] = r.Name, ["score"] = r.Score, ["priority"] = r.Priority,
                    ["balance"] = Math.Round(r.Balance, 2), ["overdue"] = Math.Round(r.Overdue, 2), ["oldest_days"] = r.OldestDays,
                    ["buckets"] = Buckets(r.NotDue, r.D1_30, r.D31_60, r.D61_90, r.DOver90, r.Unallocated),
                    ["dso_days"] = r.Dso, ["last_receipt"] = r.LastReceiptDate?.ToString("yyyy-MM-dd", inv), ["credit_limit"] = r.CreditLimit,
                    ["state"] = r.State, ["promise_date"] = r.PromiseDate?.ToString("yyyy-MM-dd", inv),
                    ["parts"] = new JsonObject { ["age"] = r.Parts[0], ["amount"] = r.Parts[1], ["behaviour"] = r.Parts[2], ["limit"] = r.Parts[3], ["idle"] = r.Parts[4] },
                }).ToArray()),
            };
            if (customer != 0 && list.Count == 0) payload["message"] = "العميل ليس فى قائمة التحصيل لنطاقك.";
            var ds = new AiDataset
            {
                Columns = { "customer_name", "customer_code", "score", "priority", "overdue", "balance", "oldest_days", "state" },
                Types = { "text", "text", "number", "text", "number", "number", "number", "text" },
                Truncated = res.Truncated,
            };
            foreach (var r in list) ds.Rows.Add(new object?[] { r.Name, r.Code, (decimal)r.Score, r.Priority, Math.Round(r.Overdue, 2), Math.Round(r.Balance, 2), (decimal)r.OldestDays, r.State });
            return new AiToolResult { Content = payload.ToJsonString(Json), Title = Title, RowCount = list.Count, Dataset = ds };
        }

        public const string Title = "أولويات التحصيل";

        private static JsonObject Buckets(decimal notDue, decimal d30, decimal d60, decimal d90, decimal over, decimal unalloc) => new()
        {
            ["not_due"] = Math.Round(notDue, 2), ["d1_30"] = Math.Round(d30, 2), ["d31_60"] = Math.Round(d60, 2), ["d61_90"] = Math.Round(d90, 2),
            ["over_90"] = Math.Round(over, 2), ["unallocated"] = Math.Round(unalloc, 2),
        };

        private static long? Int(JsonObject a, string k)
        {
            if (a[k] is not JsonValue v) return null;
            if (v.TryGetValue<long>(out var l)) return l;
            if (v.TryGetValue<double>(out var d)) return (long)d;
            if (v.TryGetValue<string>(out var s) && long.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var p)) return p;
            return null;
        }

        private static string Str(JsonObject a, string k) => a[k] is JsonValue v && v.TryGetValue<string>(out var s) ? s.Trim().ToLowerInvariant() : "";
    }
}
