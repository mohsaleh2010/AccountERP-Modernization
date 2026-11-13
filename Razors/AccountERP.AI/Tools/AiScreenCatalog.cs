using System.Globalization;
using System.Text;
using System.Text.Json.Nodes;
using AccountModelsLib;

namespace AccountERP.AI.Tools
{
    /// <summary>
    /// P3: كتالوج الشاشات التى يفتحها open_screen (§5 «متى يفتح شاشة بدل رسم تقرير»). الشاشات المعرّفة هنا تقبل باراميترات (فترة، فرع، معرّف)،
    /// وأى شاشة أخرى من قائمة المستخدم تُفتح بمسارها كما هو (find_screen). الصلاحية دائماً = الشاشة موجودة فى قائمة المستخدم.
    /// لا يُنفَّذ أى تنقّل تلقائى: الناتج بطاقة بزر «افتح الشاشة» يضغطه المستخدم.
    /// </summary>
    public static class AiScreenCatalog
    {
        /// <summary>Key للنموذج، Route = مسار الصلاحية، Params المقبولة، و Build يكوّن الرابط من المدخلات بعد التحقق.</summary>
        public sealed record Screen(string Key, string Route, string TitleAr, string ParamsHelp, string[] Params);

        public const string PFrom = "from", PTo = "to", PBranch = "branch_id", PMode = "mode", PFilter = "filter", PId = "id";

        public static readonly Screen[] Screens =
        {
            new("trial_balance", AiSchemaCatalog.RTrialBalance, "ميزان المراجعة",
                "from, to (yyyy-MM-dd داخل فترة مالية واحدة), branch_id, mode (totals = بالمجاميع | closing = بالأرصدة), filter (all | with_balance | movement_only)",
                new[] { PFrom, PTo, PBranch, PMode, PFilter }),
            new("income_statement", AiSchemaCatalog.RIncome, "قائمة الدخل", "from, to, branch_id", new[] { PFrom, PTo, PBranch }),
            new("profit_loss", AiSchemaCatalog.RProfitLoss, "التشغيل والمتاجرة والأرباح والخسائر", "from, to, branch_id", new[] { PFrom, PTo, PBranch }),
            new("balance_sheet", AiSchemaCatalog.RBalanceSheet, "المركز المالى (الميزانية)", "to (أى تاريخ داخل الفترة المالية المطلوبة), branch_id", new[] { PTo, PBranch }),
            new("sales_invoice", "/accounts/invoices/sales-invoice", "فاتورة مبيعات", "id (معرّف الفاتورة invoice_id؛ بدونه = فاتورة جديدة)", new[] { PId }),
            new("sales_return", "/accounts/invoices/sales-return", "مرتجع مبيعات", "id (معرّف المرتجع؛ بدونه = جديد)", new[] { PId }),
            new("purchase_invoice", "/accounts/invoices/purchase-invoice", "فاتورة مشتريات", "id (معرّف الفاتورة؛ بدونه = جديدة)", new[] { PId }),
            new("purchase_return", "/accounts/invoices/purchase-return", "مرتجع مشتريات", "id (معرّف المرتجع؛ بدونه = جديد)", new[] { PId }),
            new("customer_info", "/customers/info", "بيانات العميل", "id (customer_id؛ بدونه = عميل جديد)", new[] { PId }),
            new("sales_invoices_list", AiSchemaCatalog.RSales, "قائمة فواتير المبيعات", "", Array.Empty<string>()),
            new("purchase_invoices_list", AiSchemaCatalog.RPurchases, "قائمة فواتير المشتريات", "", Array.Empty<string>()),
            new("store_balances", AiSchemaCatalog.RStock, "أرصدة المخازن", "", Array.Empty<string>()),
        };

        public static Screen? Find(string key) => Screens.FirstOrDefault(s => string.Equals(s.Key, key, StringComparison.OrdinalIgnoreCase));

        /// <summary>وصف الشاشات المسموحة للنموذج (داخل وصف أداة open_screen).</summary>
        public static string Describe(Func<string, bool> hasRoute)
        {
            var sb = new StringBuilder();
            foreach (var s in Screens.Where(s => hasRoute(s.Route)))
                sb.Append("\n- ").Append(s.Key).Append(" = ").Append(s.TitleAr).Append(s.ParamsHelp.Length > 0 ? " — params: " + s.ParamsHelp : "");
            return sb.ToString();
        }

        /// <summary>
        /// يكوّن الرابط بعد التحقق من كل باراميتر (لا يُمرَّر أى نص حر للرابط). يرمى ArgumentException برسالة عربية للنموذج.
        /// Detail = وصف عربى لما سيُفتح.
        /// </summary>
        public static (string Url, string Detail) Build(Screen s, JsonObject? p)
        {
            p ??= new JsonObject();
            foreach (var k in p.Select(kv => kv.Key))
                if (!s.Params.Contains(k)) throw new ArgumentException($"الباراميتر «{k}» غير مدعوم فى شاشة {s.TitleAr}. المدعوم: {(s.Params.Length == 0 ? "لا شىء" : string.Join(", ", s.Params))}.");
            var q = new List<string>();
            var d = new List<string>();
            DateTime? Date(string key)
            {
                if (p[key] is not JsonValue v || !v.TryGetValue<string>(out var t) || string.IsNullOrWhiteSpace(t)) return null;
                if (!DateTime.TryParseExact(t.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                    throw new ArgumentException($"«{key}» يجب أن يكون تاريخاً بصيغة yyyy-MM-dd.");
                return dt;
            }
            long? Long(string key)
            {
                if (p[key] is not JsonValue v) return null;
                if (v.TryGetValue<long>(out long n) && n > 0) return n;
                if (v.TryGetValue<string>(out var t) && long.TryParse(t, NumberStyles.None, CultureInfo.InvariantCulture, out n) && n > 0) return n;
                throw new ArgumentException($"«{key}» يجب أن يكون رقماً موجباً.");
            }
            string? Choice(string key, params string[] allowed)
            {
                if (p[key] is not JsonValue v || !v.TryGetValue<string>(out var t) || string.IsNullOrWhiteSpace(t)) return null;
                t = t.Trim().ToLowerInvariant();
                if (!allowed.Contains(t)) throw new ArgumentException($"«{key}» يجب أن يكون واحداً من: {string.Join(", ", allowed)}.");
                return t;
            }

            string path = s.Route;
            if (s.Params.Contains(PId))
            {
                long? id = Long(PId);
                if (id != null) { path += "/" + id.Value.ToString(CultureInfo.InvariantCulture); d.Add("رقم " + id.Value.ToString(CultureInfo.InvariantCulture)); }
            }
            var from = s.Params.Contains(PFrom) ? Date(PFrom) : null;
            var to = s.Params.Contains(PTo) ? Date(PTo) : null;
            if (from != null && to != null && to < from) throw new ArgumentException("تاريخ النهاية قبل البداية.");
            if (from != null) { q.Add("from=" + from.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)); d.Add("من " + from.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)); }
            if (to != null) { q.Add("to=" + to.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)); d.Add((s.Key == "balance_sheet" ? "حتى " : "إلى ") + to.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)); }
            if (s.Params.Contains(PBranch) && Long(PBranch) is long b) { q.Add("branch=" + b.ToString(CultureInfo.InvariantCulture)); d.Add("فرع " + b.ToString(CultureInfo.InvariantCulture)); }
            if (s.Params.Contains(PMode) && Choice(PMode, "totals", "closing") is string m)
            {
                if (m == "closing") q.Add("mode=closing");
                d.Add(m == "closing" ? "بالأرصدة" : "بالمجاميع");
            }
            if (s.Params.Contains(PFilter) && Choice(PFilter, "all", "with_balance", "movement_only") is string f && f != "all")
            {
                q.Add("filter=" + f);
                d.Add(f == "with_balance" ? "الحسابات ذات الرصيد" : "الحسابات ذات الحركة");
            }
            // فتح الشاشة بفترة محددة يعرض النتيجة مباشرة.
            if (from != null || to != null) q.Add("run=1");
            return (q.Count == 0 ? path : path + "?" + string.Join("&", q), string.Join("، ", d));
        }

        // ───── find_screen: البحث فى قائمة المستخدم بالعنوان ─────

        /// <summary>تطبيع عربى للبحث (أ/إ/آ→ا، ة→ه، ى→ي، حذف التشكيل والتطويل).</summary>
        public static string Normalize(string s)
        {
            var sb = new StringBuilder(s.Length);
            foreach (char ch in s.ToLowerInvariant())
            {
                if (ch is >= 'ً' and <= 'ْ' or 'ـ') continue;
                sb.Append(ch switch { 'أ' or 'إ' or 'آ' => 'ا', 'ة' => 'ه', 'ى' => 'ي', _ => ch });
            }
            return sb.ToString();
        }

        public static List<AiScreenRef> Search(IEnumerable<AiScreenRef> screens, string text, int top = 10)
        {
            var words = Normalize(text).Split(new[] { ' ', '،', ',' }, StringSplitOptions.RemoveEmptyEntries).Where(w => w.Length > 1).ToList();
            if (words.Count == 0) return new();
            return screens
                .Where(s => !string.IsNullOrWhiteSpace(s.Path))
                .Select(s => (S: s, N: Normalize(s.Title + " " + s.Path)))
                .Select(x => (x.S, Score: words.Count(w => x.N.Contains(w))))
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score).ThenBy(x => x.S.Title.Length)
                .GroupBy(x => x.S.Path, StringComparer.OrdinalIgnoreCase).Select(g => g.First().S)
                .Take(top).ToList();
        }
    }
}
