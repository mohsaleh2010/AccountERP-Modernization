using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using AccountERP.AI.Chat;
using AccountERP.AI.Reorder;
using AccountModelsLib;

namespace AccountERP.AI.Tools
{
    /// <summary>
    /// P8.6.4 (ai-assistant-p86-plan.md ط16): أداة get_reorder_suggestions — آخر تشغيل أسبوعى لإعادة الطلب لفروع المستخدم، مرتب بأقرب نفاد.
    /// قراءة فقط: لا مسودات (المسودة من تبويب «الاقتراح الذكى» أو create_purchase_invoice_draft).
    /// </summary>
    public static class AiReorderTools
    {
        public const string ToolName = "get_reorder_suggestions";
        public const string Title = "اقتراحات إعادة الطلب";
        private static readonly JsonSerializerOptions Json = new() { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

        public static AiToolDef Def() => new()
        {
            Name = ToolName,
            Description = "اقتراحات إعادة الطلب الذكية (آخر تشغيل أسبوعى): لكل صنف ومخزن الرصيد والمبيعات الأسبوعية المتوقعة وتاريخ النفاد المتوقع والكمية المقترحة بالوحدة الكبرى " +
                          "والمورد الأخير والتكلفة التقديرية. للقراءة فقط؛ لفتح الشاشة open_screen بالمسار " + AiReorderConst.Route + ".",
            Schema = new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    ["top"] = new JsonObject { ["type"] = "integer", ["description"] = "عدد الأصناف (1–30، الافتراضى 15)" },
                    ["store_id"] = new JsonObject { ["type"] = "integer", ["description"] = "مخزن واحد (اختيارى)" },
                    ["within_days"] = new JsonObject { ["type"] = "integer", ["description"] = "ينفد خلال عدد أيام (اختيارى)" },
                },
            },
        };

        public static bool Allowed(AiToolContext c) => c.HasRoute(AiReorderConst.Route);

        public static AiToolResult Run(JsonObject a, AiToolContext c, AiReorderService svc)
        {
            if (!svc.Settings().Enabled) return new AiToolResult { Content = "{\"message\":\"إعادة الطلب الذكية موقوفة من إعدادات المساعد.\"}", Title = Title };
            int top = a["top"] is JsonValue tv && tv.TryGetValue<int>(out var t) && t > 0 ? Math.Min(30, t) : 15;
            long store = a["store_id"] is JsonValue sv && sv.TryGetValue<long>(out var s) ? s : 0;
            int within = a["within_days"] is JsonValue wv && wv.TryGetValue<int>(out var w) ? Math.Max(0, w) : 0;
            var run = svc.Dal.LatestRun(true);
            if (run == null) return new AiToolResult { Content = "{\"message\":\"لم يُشغَّل حساب إعادة الطلب بعد.\"}", Title = Title };
            var branches = c.User.IsAdmin ? null
                : new AccountModelsLib.BusinessLibShared.GeneralLedger.FinalAccountsService().GetBranches().Select(b => b.Id).Distinct().ToList();
            IEnumerable<AiReorderRow> q = svc.Dal.ListRun(run.Id, null).Where(r => r.Status != AiReorderConst.StatusDismissed && (branches == null || branches.Contains(r.Branch_Id)));
            if (store > 0) q = q.Where(r => r.Store_Id == store);
            if (within > 0) q = q.Where(r => r.StockoutDate.HasValue && r.StockoutDate.Value <= DateTime.Today.AddDays(within));
            var list = q.Take(top).ToList();
            var inv = CultureInfo.InvariantCulture;
            decimal F(AiReorderRow r) => r.Factor <= 0 ? 1 : r.Factor;
            var payload = new JsonObject
            {
                ["computed_at"] = run.CreatedAt?.ToString("yyyy-MM-dd HH:mm", inv), ["screen"] = AiReorderConst.Route,
                ["note"] = "توقع إحصائى؛ الكميات بالوحدة الكبرى. حد الطلب اليدوى لا يتغير.",
                ["items"] = new JsonArray(list.Select(r => (JsonNode)new JsonObject
                {
                    ["item"] = r.ItemName, ["code"] = r.ItemCode, ["store"] = r.StoreName, ["on_hand_large"] = Math.Round(r.OnHand / F(r), 2),
                    ["weekly_forecast_large"] = Math.Round(r.AvgWeekly / F(r), 2), ["stockout_date"] = r.StockoutDate?.ToString("yyyy-MM-dd", inv),
                    ["suggested_large"] = r.SuggestQtyLarge, ["unit"] = r.LargeUnit, ["manual_reorder_level"] = r.ManualReorderLarge,
                    ["last_supplier"] = r.LastSupplierName, ["estimated_cost"] = r.EstCost, ["confidence"] = r.Confidence, ["method"] = r.Method,
                }).ToArray()),
            };
            var ds = new AiDataset
            {
                Columns = { "item_name", "store_name", "stockout_date", "suggested_large", "unit", "last_supplier", "estimated_cost" },
                Types = { "text", "text", "date", "number", "text", "text", "number" },
            };
            foreach (var r in list) ds.Rows.Add(new object?[] { r.ItemName, r.StoreName, r.StockoutDate, r.SuggestQtyLarge, r.LargeUnit, r.LastSupplierName, r.EstCost });
            return new AiToolResult { Content = payload.ToJsonString(Json), Title = Title, RowCount = list.Count, Dataset = ds };
        }
    }
}
