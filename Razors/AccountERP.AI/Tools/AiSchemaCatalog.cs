using System.Text;

namespace AccountERP.AI.Tools
{
    /// <summary>وصف View للمساعد: الشاشات التى تمنح صلاحية قراءته (أى واحدة تكفى)، والأعمدة بشرح عربى.</summary>
    public sealed record AiViewDoc(string Name, string TitleAr, string Description, string[] Routes, (string Col, string Desc)[] Columns);

    /// <summary>مقطع معرفة (مسرد / قاعدة / مثال) تبحث فيه أداة search_schema.</summary>
    /// P7: Key ثابت (مفتاح الصف فى AI_Knowledge مع Kind)، و Version يُرفع عند تعديل المحتوى هنا ليُحدَّث الصف المزروع غير المعدَّل.
    public sealed record AiKnowledgeItem(string Kind, string Title, string Content, string Keywords, string Key, int Version = 1);

    /// <summary>
    /// طبقة المعرفة الأولى (ai-assistant-plan.md §3، P2): قاموس الـ Views والمسرد العربى وقواعد العمل وأمثلة موثّقة، فى الكود.
    /// القرار (الموصى به): البداية فى الكود بلا جدول AI_Knowledge — المحتوى صغير ومرتبط بتعريف الـ Views نفسها فيتغيّر معها؛
    /// الجدول والبحث بالمتجهات فى P7. الأعمدة هنا يجب أن تطابق AiReadViewsCLS حرفياً.
    /// </summary>
    public static class AiSchemaCatalog
    {
        // مسارات الشاشات التى تمنح الصلاحية (المقارنة بالمسار بلا معاملات الاستعلام).
        public const string RSales = "/accounts/invoices/list", RPurchases = "/accounts/invoices/purchases-list", RStock = "/store/balances",
            RItems = "/items", RCustomers = "/customers/list", RSuppliers = "/suppliers/list", RCustomerBalances = "/accounts/customer-balance-summary",
            RSupplierBalances = "/accounts/supplier-balance-summary", RTrialBalance = "/accounts/final/trial-balance",
            RIncome = "/accounts/final/income-statement", RProfitLoss = "/accounts/final/profit-loss", RBalanceSheet = "/accounts/final/balance-sheet";

        /// <summary>Views مرجعية بلا بيانات مالية (الفروع والمخازن): متاحة لكل من يملك المساعد.</summary>
        public static readonly string[] Always = Array.Empty<string>();

        private static readonly (string, string)[] SaleHead =
        {
            ("total", "إجمالى الأصناف قبل الضريبة والخصومات"), ("commission", "العمولة"), ("sales_tax", "ضريبة المبيعات/القيمة المضافة (TaxSales10)"),
            ("ats05_value", "الخصم الإضافى / ضريبة الخصم والتحصيل (ATS05)"), ("stamp_duty", "الدمغة"), ("net_total", "صافى الفاتورة (هو الرقم المعتمد لإجمالى المبيعات)"),
            ("net_total_equivalent", "الصافى بالعملة المعادلة (المحلية)"), ("currency_id", "العملة"), ("is_tax_invoice", "1 = فاتورة ضريبية، 0 = غير ضريبية"),
            ("is_credit", "1 = آجل، 0 = نقدى"), ("notes", "ملاحظات"),
        };

        private static readonly (string, string)[] LineCols =
        {
            ("item_id", "معرّف الصنف"), ("item_code", "كود الصنف"), ("item_name", "اسم الصنف"), ("store_id", "المخزن"), ("store_name", "اسم المخزن"),
            ("large_qty", "الكمية بالوحدة الكبرى"), ("small_qty", "الكمية بالوحدة الصغرى"), ("qty", "الكمية المعروضة على الفاتورة"), ("unit_name", "اسم الوحدة"),
            ("unit_price", "سعر الوحدة"), ("line_total", "إجمالى السطر قبل الضريبة"), ("line_sales_tax", "ضريبة السطر"), ("line_ats05_value", "خصم/ضريبة ATS05 للسطر"),
            ("line_net", "صافى السطر"),
        };

        public static readonly AiViewDoc[] Views =
        {
            new("v_branches", "الفروع", "فروع الشركة.", Always, new[] { ("branch_id", "معرّف الفرع"), ("branch_name", "اسم الفرع"), ("branch_code", "كود الفرع") }),
            new("v_stores", "المخازن", "المخازن وفروعها.", Always, new[] { ("store_id", "معرّف المخزن"), ("store_name", "اسم المخزن"), ("store_code", "كود المخزن"), ("branch_id", "الفرع"), ("active_flag", "علم التفعيل كما فى النظام") }),
            new("v_items", "الأصناف", "بطاقة الصنف: الكود والاسم والمجموعة والوحدات والأسعار الافتراضية.", new[] { RItems, RStock, RSales, RPurchases },
                new[] { ("item_id", "معرّف الصنف"), ("item_code", "الكود"), ("item_name", "الاسم"), ("category_id", "المجموعة"), ("category_name", "اسم المجموعة"),
                        ("active_flag", "علم التفعيل كما فى النظام"), ("large_unit", "الوحدة الكبرى"), ("small_unit", "الوحدة الصغرى"), ("small_per_large", "عدد الصغرى فى الكبرى"),
                        ("sale_price_large", "سعر البيع للكبرى"), ("sale_price_small", "سعر البيع للصغرى"), ("purchase_price_large", "سعر الشراء للكبرى"),
                        ("purchase_price_small", "سعر الشراء للصغرى"), ("avg_cost", "متوسط التكلفة") }),
            new("v_customers", "العملاء", "بيانات العملاء الأساسية (بلا أرصدة ولا بيانات اتصال).", new[] { RCustomers, RSales, RCustomerBalances },
                new[] { ("customer_id", "معرّف العميل"), ("customer_code", "الكود"), ("customer_name", "الاسم"), ("active_flag", "علم التفعيل"), ("is_general_customer", "1 = عميل نقدى عام"), ("branch_id", "الفرع") }),
            new("v_suppliers", "الموردون", "بيانات الموردين الأساسية (بلا أرصدة ولا بيانات اتصال).", new[] { RSuppliers, RPurchases, RSupplierBalances },
                new[] { ("supplier_id", "معرّف المورد"), ("supplier_code", "الكود"), ("supplier_name", "الاسم"), ("active_flag", "علم التفعيل"), ("is_shipment_company", "1 = شركة شحن"), ("branch_id", "الفرع") }),
            new("v_sales", "فواتير المبيعات", "رأس فاتورة المبيعات (فاتورة واحدة لكل صف). المحذوف مستبعد، ويظهر فقط ما يحق للمستخدم من الفواتير الضريبية/غير الضريبية.", new[] { RSales },
                new (string, string)[] { ("sale_id", "معرّف الفاتورة (للربط مع السطور)"), ("invoice_no", "رقم الفاتورة"), ("invoice_date", "التاريخ (date)"), ("invoice_datetime", "التاريخ والوقت"),
                        ("customer_id", "العميل"), ("customer_name", "اسم العميل"), ("branch_id", "الفرع"), ("branch_name", "اسم الفرع") }
                    .Concat(SaleHead).Concat(new[] { ("is_tax_per_item", "1 = الضريبة على مستوى الصنف"), ("paid_value", "المدفوع"), ("rest_value", "الباقى"),
                        ("salesman_id", "المندوب"), ("is_maintenance", "1 = فاتورة صيانة سيارات"), ("is_posted", "1 = مرحّلة") }).ToArray()),
            new("v_sales_lines", "سطور المبيعات", "أصناف فواتير المبيعات (سطر لكل صنف) مع رأس الفاتورة.", new[] { RSales },
                new (string, string)[] { ("line_id", "معرّف السطر"), ("sale_id", "الفاتورة"), ("invoice_no", "رقم الفاتورة"), ("invoice_date", "التاريخ"), ("customer_id", "العميل"),
                        ("customer_name", "اسم العميل"), ("branch_id", "الفرع") }.Concat(LineCols)
                    .Concat(new[] { ("avg_cost_at_sale", "متوسط التكلفة وقت البيع"), ("is_tax_invoice", "1 = فاتورة ضريبية") }).ToArray()),
            new("v_sales_returns", "مرتجعات المبيعات", "رأس مرتجع المبيعات.", new[] { RSales },
                new (string, string)[] { ("return_id", "معرّف المرتجع"), ("return_no", "رقم المرتجع"), ("return_date", "التاريخ"), ("customer_id", "العميل"), ("customer_name", "اسم العميل"),
                        ("branch_id", "الفرع"), ("branch_name", "اسم الفرع") }.Concat(SaleHead).ToArray()),
            new("v_sales_return_lines", "سطور مرتجعات المبيعات", "أصناف مرتجعات المبيعات.", new[] { RSales },
                new (string, string)[] { ("line_id", "معرّف السطر"), ("return_id", "المرتجع"), ("return_no", "رقم المرتجع"), ("return_date", "التاريخ"), ("customer_id", "العميل"),
                        ("customer_name", "اسم العميل"), ("branch_id", "الفرع") }.Concat(LineCols).ToArray()),
            new("v_purchases", "فواتير المشتريات", "رأس فاتورة المشتريات. المحذوف مستبعد.", new[] { RPurchases },
                new (string, string)[] { ("purchase_id", "معرّف الفاتورة"), ("invoice_no", "رقم الفاتورة"), ("invoice_date", "التاريخ"), ("supplier_id", "المورد"), ("supplier_name", "اسم المورد"),
                        ("branch_id", "الفرع"), ("branch_name", "اسم الفرع") }.Concat(SaleHead)
                    .Concat(new[] { ("is_tax_per_item", "1 = الضريبة على مستوى الصنف"), ("paid_value", "المدفوع"), ("rest_value", "الباقى"), ("is_posted", "1 = مرحّلة") }).ToArray()),
            new("v_purchase_lines", "سطور المشتريات", "أصناف فواتير المشتريات.", new[] { RPurchases },
                new (string, string)[] { ("line_id", "معرّف السطر"), ("purchase_id", "الفاتورة"), ("invoice_no", "رقم الفاتورة"), ("invoice_date", "التاريخ"), ("supplier_id", "المورد"),
                        ("supplier_name", "اسم المورد"), ("branch_id", "الفرع") }.Concat(LineCols).ToArray()),
            new("v_purchase_returns", "مرتجعات المشتريات", "رأس مرتجع المشتريات.", new[] { RPurchases },
                new (string, string)[] { ("return_id", "معرّف المرتجع"), ("return_no", "رقم المرتجع"), ("return_date", "التاريخ"), ("supplier_id", "المورد"), ("supplier_name", "اسم المورد"),
                        ("branch_id", "الفرع"), ("branch_name", "اسم الفرع") }.Concat(SaleHead).ToArray()),
            new("v_purchase_return_lines", "سطور مرتجعات المشتريات", "أصناف مرتجعات المشتريات.", new[] { RPurchases },
                new (string, string)[] { ("line_id", "معرّف السطر"), ("return_id", "المرتجع"), ("return_no", "رقم المرتجع"), ("return_date", "التاريخ"), ("supplier_id", "المورد"),
                        ("supplier_name", "اسم المورد"), ("branch_id", "الفرع") }.Concat(LineCols).ToArray()),
            new("v_stock_balance", "أرصدة المخازن", "الرصيد الحالى لكل صنف فى كل مخزن (Unit_Item). الرصيد = large_qty وحدة كبرى + small_qty وحدة صغرى؛ total_small_qty = الإجمالى بالصغرى.", new[] { RStock },
                new[] { ("balance_row_id", "معرّف صف الرصيد"), ("item_id", "الصنف"), ("item_code", "كود الصنف"), ("item_name", "اسم الصنف"), ("store_id", "المخزن"), ("store_name", "اسم المخزن"),
                        ("branch_id", "فرع المخزن"), ("large_qty", "الرصيد بالوحدة الكبرى"), ("small_qty", "الرصيد بالوحدة الصغرى (الكسر)"), ("large_unit", "اسم الكبرى"),
                        ("small_unit", "اسم الصغرى"), ("small_per_large", "عدد الصغرى فى الكبرى"), ("total_small_qty", "الرصيد كله بالصغرى"),
                        ("avg_cost_large", "متوسط التكلفة"), ("purchase_price_large", "سعر الشراء للكبرى"), ("sale_price_large", "سعر البيع للكبرى") }),
        };

        public static readonly AiKnowledgeItem[] Knowledge =
        {
            new("glossary", "مدين ودائن", "مدين = Madin، دائن = Da2n فى أسماء الأعمدة القديمة. رصيد العميل المدين = مستحق لنا عليه، والدائن = مستحق له علينا.", "مدين دائن madin da2n رصيد", "debit_credit"),
            new("glossary", "المشتريات", "Purshing = مشتريات (خطأ إملائى قديم فى أسماء الجداول)، Detailes = تفاصيل. Views المساعد تُخفى هذه الأسماء.", "مشتريات purshing detailes", "purchases_names"),
            new("glossary", "آجل ونقدى", "الفاتورة الآجلة is_credit = 1، والنقدية is_credit = 0.", "آجل اجل نقدى نقدي كاش credit", "credit_cash"),
            new("glossary", "الوحدات", "لكل صنف وحدة كبرى (كرتونة مثلاً) وصغرى (قطعة)، small_per_large = عدد الصغرى فى الكبرى. الكمية الكلية بالصغرى = large_qty * small_per_large + small_qty.", "وحدة كبرى صغرى كرتونة قطعة كمية", "units"),
            new("glossary", "إذن الإضافة والصرف", "إذن إضافة مخزنى = 1007، إذن صرف = 1008 (فى شاشات الأذون). لا View لهما فى هذه المرحلة.", "اذن اضافة صرف 1007 1008 مخزن", "store_orders"),
            new("rule", "الضريبة", "is_tax_invoice = 1 فاتورة ضريبية. is_tax_per_item = 1 الضريبة محسوبة لكل صنف فلا تُحسب على الإجمالى مرة أخرى. لا تستخدم نوع الفاتورة كعلم ضريبى.", "ضريبة ضريبية قيمة مضافة tax vat", "tax"),
            new("rule", "صافى المبيعات", "إجمالى المبيعات المعتمد = SUM(net_total) من v_sales. صافى المبيعات بعد المرتجعات = مبيعات الفترة - SUM(net_total) من v_sales_returns لنفس الفترة.", "صافى مبيعات اجمالى مرتجع", "net_sales"),
            new("rule", "أرقام الحسابات", "أرصدة الحسابات والميزان وقائمة الدخل والميزانية تؤخذ من الأدوات المالية (get_trial_balance، get_financial_statement) لا من الـ Views، لتطابق شاشات النظام.", "ميزان مراجعة قائمة دخل ميزانية ارباح خسائر حسابات", "account_figures"),
            new("rule", "المحذوف والصلاحيات", "الـ Views تستبعد الفواتير المحذوفة، وتعرض للمستخدم فقط الفواتير الضريبية/غير الضريبية التى يملك صلاحية رؤيتها؛ لذلك قد تختلف الأرقام بين مستخدمين.", "محذوف صلاحية ضريبية", "deleted_permissions"),
            new("example", "مبيعات شهر", "SELECT SUM(net_total) AS sales, COUNT(*) AS invoices FROM ai.v_sales WHERE invoice_date >= '2026-10-01' AND invoice_date < '2026-11-01'", "مبيعات شهر اجمالى فواتير عدد", "monthly_sales"),
            new("example", "أكثر الأصناف مبيعاً", "SELECT TOP 10 item_name, SUM(line_net) AS value, SUM(qty) AS qty FROM ai.v_sales_lines WHERE invoice_date >= '2026-01-01' GROUP BY item_name ORDER BY value DESC", "اكثر اصناف مبيعا افضل top", "top_items"),
            new("example", "رصيد صنف فى مخزن", "SELECT store_name, large_qty, large_unit, small_qty, small_unit, total_small_qty FROM ai.v_stock_balance WHERE item_id = 123 AND store_id = 4", "رصيد صنف مخزن كمية متاح", "item_store_balance"),
            new("example", "مبيعات عميل", "SELECT invoice_no, invoice_date, net_total FROM ai.v_sales WHERE customer_id = 55 ORDER BY invoice_date DESC", "مبيعات عميل فواتير", "customer_sales"),
            new("example", "مشتريات مورد", "SELECT SUM(net_total) FROM ai.v_purchases WHERE supplier_id = 9 AND invoice_date >= '2026-01-01'", "مشتريات مورد", "supplier_purchases"),
        };

        public static AiViewDoc? Find(string name) => Views.FirstOrDefault(v => string.Equals(v.Name, name, StringComparison.OrdinalIgnoreCase));

        /// <summary>هل يحق للمستخدم قراءة الـ View: المدير دائماً، والمرجعية للجميع، والباقى بأى شاشة من شاشاته.</summary>
        public static bool Allowed(AiViewDoc v, Func<string, bool> hasRoute, bool isAdmin) =>
            isAdmin || v.Routes.Length == 0 || v.Routes.Any(hasRoute);

        /// <summary>وصف مضغوط للـ Views المسموحة يدخل فى تعليمات النظام (ثابت لنفس الصلاحيات فيستفيد من التخزين المؤقت).</summary>
        public static string Describe(IEnumerable<AiViewDoc> views)
        {
            var sb = new StringBuilder();
            foreach (var v in views)
            {
                sb.Append("- ai.").Append(v.Name).Append(" — ").Append(v.TitleAr).Append(": ").AppendLine(v.Description);
                sb.Append("  ").AppendLine(string.Join("، ", v.Columns.Select(c => c.Col + " (" + c.Desc + ")")));
            }
            return sb.ToString();
        }

        /// <summary>بحث بالكلمات فى الكود (P2). منذ P7 البحث من AI_Knowledge (AiKnowledgeService)، وهذه احتياطية إن تعذّرت قراءة الجدول.</summary>
        public static string Search(string query, IEnumerable<AiViewDoc> allowed, int top = 6)
        {
            var words = (query ?? "").ToLowerInvariant()
                .Split(new[] { ' ', '،', ',', '.', '?', '؟', '-', '_', '/' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(Normalize).Where(w => w.Length >= 2).Distinct().ToList();
            var hits = new List<(int Score, string Text)>();
            foreach (var v in allowed)
            {
                string hay = Normalize((v.Name + " " + v.TitleAr + " " + v.Description + " " + string.Join(" ", v.Columns.Select(c => c.Col + " " + c.Desc))).ToLowerInvariant());
                int s = words.Count(w => hay.Contains(w));
                if (s > 0) hits.Add((s + 1, "[view] ai." + v.Name + " — " + v.TitleAr + ": " + v.Description + "\nالأعمدة: " + string.Join("، ", v.Columns.Select(c => c.Col + " (" + c.Desc + ")"))));
            }
            foreach (var k in Knowledge)
            {
                string hay = Normalize((k.Title + " " + k.Content + " " + k.Keywords).ToLowerInvariant());
                int s = words.Count(w => hay.Contains(w));
                if (s > 0) hits.Add((s, "[" + k.Kind + "] " + k.Title + ": " + k.Content));
            }
            var best = hits.OrderByDescending(h => h.Score).Take(top).Select(h => h.Text).ToList();
            return best.Count == 0 ? "لا نتائج. الـ Views المتاحة مذكورة فى التعليمات." : string.Join("\n\n", best);
        }

        /// <summary>توحيد الهمزات والتاء المربوطة والياء حتى تتطابق الصياغات العربية.</summary>
        public static string Normalize(string s) => s
            .Replace('أ', 'ا').Replace('إ', 'ا').Replace('آ', 'ا').Replace('ة', 'ه').Replace('ى', 'ي').Replace("ـ", "");
    }
}
