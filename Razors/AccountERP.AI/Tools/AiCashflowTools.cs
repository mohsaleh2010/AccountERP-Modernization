using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using AccountERP.AI.Cashflow;
using AccountERP.AI.Chat;
using AccountModelsLib;

namespace AccountERP.AI.Tools
{
    /// <summary>
    /// P8.7.4 (ai-assistant-p87-plan.md و15): أداة get_cashflow_forecast — نفس خدمة الشاشة ونطاق المستخدم. تعيد اللقطة إن كانت حديثة (&lt; 6 ساعات)
    /// وإلا تحسب. قراءة فقط. لمن يملك شاشة توقع التدفق النقدى وكانت الميزة مفعّلة.
    /// </summary>
    public static class AiCashflowTools
    {
        public const string ToolName = "get_cashflow_forecast";
        public const string Title = "توقع التدفق النقدى";
        private static readonly JsonSerializerOptions Json = new() { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

        public static AiToolDef Def() => new()
        {
            Name = ToolName,
            Description = "توقع التدفق النقدى لـ 13 أسبوعاً كما فى شاشة النظام: الرصيد الحالى للخزائن والبنوك، والمقبوضات المتوقعة (عملاء آجل، شيكات واردة، مبيعات نقدية، تعديلات يدوية) " +
                          "والمدفوعات المتوقعة (موردون آجل، شيكات صادرة، مصروفات متكررة)، والرصيد آخر كل أسبوع، بسيناريو أساسى ومتحفظ. توقع تقديرى لا تقرير محاسبى. " +
                          "لفتح الشاشة استخدم open_screen بالمسار " + AiCashflowConst.Route + ".",
            Schema = new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    ["scenario"] = new JsonObject { ["type"] = "string", ["description"] = "base (الافتراضى) / conservative / both", ["enum"] = new JsonArray("base", "conservative", "both") },
                    ["branch_id"] = new JsonObject { ["type"] = "integer", ["description"] = "فرع واحد من فروع المستخدم (اختيارى)" },
                },
            },
        };

        public static bool Allowed(AiToolContext c) => c.HasRoute(AiCashflowConst.Route);

        public static async Task<AiToolResult> RunAsync(JsonObject a, AiToolContext c, AiCashflowService svc)
        {
            if (!svc.Settings().Enabled)
                return new AiToolResult { Content = "{\"message\":\"توقع التدفق النقدى موقوف من إعدادات المساعد.\"}", Title = Title };
            string scenario = a["scenario"] is JsonValue sv && sv.TryGetValue<string>(out var s) ? s.Trim().ToLowerInvariant() : "base";
            long branch = a["branch_id"] is JsonValue bv && bv.TryGetValue<long>(out var b) ? b : 0;
            var server = AiUserContextFactory.ForUser(c.User.UserId);
            if (!server.Usable) return new AiToolResult { Content = "{\"message\":\"مستخدم غير متاح.\"}", Title = Title };
            var all = AiCashflowGateway.BranchesFor(server);
            IList<long>? branches = all;
            if (branch > 0)
            {
                if (all != null && !all.Contains(branch)) return new AiToolResult { Content = "{\"message\":\"الفرع خارج فروعك.\"}", Title = Title };
                branches = new List<long> { branch };
            }
            var r = await svc.GetOrComputeAsync(server, branches, false, null, "tool", c.Ct);
            var inv = CultureInfo.InvariantCulture;
            JsonObject One(AiCashflowFacts f) => new()
            {
                ["opening"] = Math.Round(f.Opening, 2), ["lowest_closing"] = Math.Round(f.LowestClosing, 2), ["lowest_week"] = f.LowestWeek + 1,
                ["first_week_below_min"] = f.LowWeek >= 0 ? f.LowWeek + 1 : null, ["estimated_due_share"] = f.EstimatedShare,
                ["weeks"] = new JsonArray(f.Weeks.Select(w => (JsonNode)new JsonObject
                {
                    ["week"] = w.Index + 1, ["start"] = w.Start.ToString("yyyy-MM-dd", inv),
                    ["customers"] = w.Customers, ["cheques_in"] = w.ChequesIn, ["cash_sales"] = w.CashSales, ["manual_in"] = w.ManualIn,
                    ["suppliers"] = w.Suppliers, ["cheques_out"] = w.ChequesOut, ["recurring"] = w.Recurring, ["manual_out"] = w.ManualOut,
                    ["net"] = w.Net, ["closing"] = w.Closing,
                }).ToArray()),
            };
            var payload = new JsonObject
            {
                ["as_of"] = r.Base.AsOf.ToString("yyyy-MM-dd", inv), ["computed_at"] = r.ComputedAt?.ToString("yyyy-MM-dd HH:mm", inv),
                ["currency_note"] = "المبالغ بالعملة المكافئة (عملة القيود)", ["min_balance"] = r.Base.MinBalance, ["screen"] = AiCashflowConst.Route,
                ["note"] = "توقع تقديرى؛ استحقاقات بلا تاريخ مسجّل محسوبة بمدة السداد.",
            };
            if (scenario != "conservative") payload["base"] = One(r.Base);
            if (scenario != "base") payload["conservative"] = One(r.Conservative);
            var f0 = scenario == "conservative" ? r.Conservative : r.Base;
            var ds = new AiDataset
            {
                Columns = { "week", "start", "inflows", "outflows", "net", "closing" },
                Types = { "number", "date", "number", "number", "number", "number" },
            };
            foreach (var w in f0.Weeks) ds.Rows.Add(new object?[] { (decimal)(w.Index + 1), w.Start, w.Inflows, w.Outflows, w.Net, w.Closing });
            return new AiToolResult { Content = payload.ToJsonString(Json), Title = Title, RowCount = f0.Weeks.Count, Dataset = ds };
        }
    }
}
