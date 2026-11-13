using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using AccountERP.AI.Chat;
using AccountModelsLib;
using AccountModelsLib.BusinessLibShared.GeneralLedger;

namespace AccountERP.AI.Tools
{
    /// <summary>سياق تنفيذ الأدوات لرسالة واحدة: من يسأل، ما يحق له، وقيمة فلتر الفواتير الضريبية.</summary>
    public sealed class AiToolContext
    {
        public required AiUserContext User { get; init; }
        /// <summary>Views المسموحة (بلا schema).</summary>
        public required HashSet<string> AllowedViews { get; init; }
        /// <summary>-1 الكل، 1 الضريبية فقط، 0 غير الضريبية فقط (Common.PermissionToShowTaxInvoiceOrNotORAll كالشاشات).</summary>
        public int TaxFilter { get; init; } = -1;
        public CancellationToken Ct { get; init; }

        public bool HasRoute(string route)
        {
            if (User.IsAdmin) return true;
            foreach (var r in User.AllowedRoutes)
            {
                string path = (r ?? "").Split('?')[0].Trim().TrimEnd('/');
                if (path.Length > 0 && string.Equals(path, route.TrimEnd('/'), StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        public static HashSet<string> ViewsFor(AiUserContext user, Func<string, bool> hasRoute) =>
            new(AiSchemaCatalog.Views.Where(v => AiSchemaCatalog.Allowed(v, hasRoute, user.IsAdmin)).Select(v => v.Name), StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>ناتج أداة: Content نص JSON للنموذج؛ الباقى للتدقيق والعرض.</summary>
    public sealed class AiToolResult
    {
        public string Content { get; init; } = "";
        public bool IsError { get; init; }
        public string Outcome { get; init; } = AiToolOutcome.Ok;
        public string? Sql { get; init; }
        public int? RowCount { get; init; }
        public string Title { get; init; } = "";
    }

    /// <summary>
    /// كتالوج الأدوات (ai-assistant-plan.md §4، P2: قراءة فقط). كل أداة تعلن الشاشات التى تمنحها (PermissionGate §2.3)،
    /// والأداة غير المسموحة لا تُرسل للنموذج أصلاً، ولو طلبها رغم ذلك تُرفض وتُسجَّل. المدخلات تُتحقق هنا قبل أى تنفيذ.
    /// الأدوات المالية تستدعى FinalAccountsService نفسه فتتطابق أرقامها مع الشاشات.
    /// </summary>
    public sealed class AiToolRegistry
    {
        public const int SampleRows = 50;
        private const int MaxContentChars = 40000;

        private readonly ReadOnlySqlExecutor _sql;
        public AiToolRegistry(ReadOnlySqlExecutor sql) => _sql = sql;

        public ReadOnlySqlExecutor Sql => _sql;

        private sealed record ToolSpec(AiToolDef Def, Func<AiToolContext, bool> Allowed, Func<JsonObject, AiToolContext, AiToolResult> Run);

        private static JsonObject Str(string desc) => new() { ["type"] = "string", ["description"] = desc };
        private static JsonObject Enum(string desc, params string[] values) =>
            new() { ["type"] = "string", ["description"] = desc, ["enum"] = new JsonArray(values.Select(v => (JsonNode)JsonValue.Create(v)!).ToArray()) };
        private static JsonObject Int(string desc) => new() { ["type"] = "integer", ["description"] = desc };
        private static JsonObject Schema(JsonObject props, params string[] required) =>
            new() { ["type"] = "object", ["properties"] = props, ["required"] = new JsonArray(required.Select(r => (JsonNode)JsonValue.Create(r)!).ToArray()) };

        private IEnumerable<ToolSpec> Specs()
        {
            yield return new ToolSpec(new AiToolDef
            {
                Name = "search_schema",
                Description = "ابحث فى قاموس بيانات النظام: الـ Views وأعمدتها، المسرد العربى للمصطلحات، قواعد العمل (الضريبة، صافى المبيعات...)، وأمثلة استعلامات موثّقة. استخدمها قبل كتابة استعلام عند الشك.",
                Schema = Schema(new JsonObject { ["query"] = Str("كلمات البحث بالعربية أو الإنجليزية") }, "query"),
            }, _ => true, SearchSchema);

            yield return new ToolSpec(new AiToolDef
            {
                Name = "run_readonly_query",
                Description = "نفّذ استعلام SELECT واحداً (T-SQL، SQL Server) على Views المساعد ai.v_... فقط. ممنوع أى تعديل. يعيد الأعمدة وعدد الصفوف وأول 50 صفاً ومجاميع الأعمدة الرقمية. استخدم التجميع (SUM/COUNT/GROUP BY) بدل جلب صفوف كثيرة، والتواريخ بصيغة 'yyyy-MM-dd'.",
                Schema = Schema(new JsonObject { ["sql"] = Str("استعلام SELECT واحد"), ["purpose"] = Str("ماذا يحسب هذا الاستعلام بالعربية (يظهر للمستخدم)") }, "sql", "purpose"),
            }, c => c.AllowedViews.Count > 0, RunQuery);

            yield return new ToolSpec(new AiToolDef
            {
                Name = "find_entity",
                Description = "ابحث عن عميل أو مورد أو صنف أو مخزن أو فرع بالاسم أو الكود لتحصل على معرّفه (أفضل 8 مطابقات). استخدمها قبل الاستعلام عن كيان ذكره المستخدم بالاسم.",
                Schema = Schema(new JsonObject
                {
                    ["type"] = Enum("نوع الكيان", "customer", "supplier", "item", "store", "branch"),
                    ["text"] = Str("جزء من الاسم أو الكود"),
                }, "type", "text"),
            }, c => c.AllowedViews.Count > 0, FindEntity);

            yield return new ToolSpec(new AiToolDef
            {
                Name = "get_periods_and_branches",
                Description = "يعيد تاريخ اليوم، والفروع المسموحة للمستخدم (المعرّف والاسم)، والفترات المالية (من/إلى). استخدمها قبل الأدوات المالية لتحديد الفرع والتواريخ.",
                Schema = Schema(new JsonObject()),
            }, c => HasAnyFinancial(c), PeriodsAndBranches);

            yield return new ToolSpec(new AiToolDef
            {
                Name = "get_trial_balance",
                Description = "ميزان المراجعة كما فى شاشة النظام (نفس المحرك): افتتاحى وحركة وختامى لكل حساب لفرع ومدى تاريخ داخل فترة مالية واحدة، بالعملة المعادلة.",
                Schema = Schema(new JsonObject
                {
                    ["from"] = Str("من تاريخ yyyy-MM-dd"),
                    ["to"] = Str("إلى تاريخ yyyy-MM-dd"),
                    ["branch_id"] = Int("الفرع (اختيارى؛ الافتراضى فرع المستخدم)"),
                    ["filter"] = Enum("all = كل الحسابات، with_balance = لها رصيد، movement_only = لها حركة", "all", "with_balance", "movement_only"),
                }, "from", "to"),
            }, c => c.HasRoute(AiSchemaCatalog.RTrialBalance), TrialBalance);

            yield return new ToolSpec(new AiToolDef
            {
                Name = "get_financial_statement",
                Description = "القوائم الختامية كما فى شاشات النظام: income = قائمة الدخل، profit_loss = التشغيل والمتاجرة والأرباح والخسائر، balance_sheet = المركز المالى (الفترة كاملة حتى نهايتها).",
                Schema = Schema(new JsonObject
                {
                    ["kind"] = Enum("نوع القائمة", "income", "profit_loss", "balance_sheet"),
                    ["from"] = Str("من تاريخ yyyy-MM-dd (للميزانية: أى تاريخ داخل الفترة)"),
                    ["to"] = Str("إلى تاريخ yyyy-MM-dd"),
                    ["branch_id"] = Int("الفرع (اختيارى؛ الافتراضى فرع المستخدم)"),
                }, "kind", "from", "to"),
            }, c => c.HasRoute(AiSchemaCatalog.RIncome) || c.HasRoute(AiSchemaCatalog.RProfitLoss) || c.HasRoute(AiSchemaCatalog.RBalanceSheet), FinancialStatement);
        }

        private static bool HasAnyFinancial(AiToolContext c) =>
            c.HasRoute(AiSchemaCatalog.RTrialBalance) || c.HasRoute(AiSchemaCatalog.RIncome) || c.HasRoute(AiSchemaCatalog.RProfitLoss) || c.HasRoute(AiSchemaCatalog.RBalanceSheet);

        /// <summary>تعريفات الأدوات المسموحة لهذا المستخدم فقط.</summary>
        public List<AiToolDef> ToolsFor(AiToolContext ctx) => Specs().Where(s => s.Allowed(ctx)).Select(s => s.Def).ToList();

        public static string TitleOf(string toolName) => toolName switch
        {
            "search_schema" => "بحث فى قاموس البيانات",
            "run_readonly_query" => "استعلام قراءة",
            "find_entity" => "بحث عن كيان",
            "get_periods_and_branches" => "الفترات والفروع",
            "get_trial_balance" => "ميزان المراجعة",
            "get_financial_statement" => "قائمة مالية",
            _ => toolName,
        };

        /// <summary>ينفّذ أداة بعد التحقق من الصلاحية والمدخلات. لا يرمى إلا OperationCanceledException.</summary>
        public AiToolResult Execute(string name, string inputJson, AiToolContext ctx)
        {
            var spec = Specs().FirstOrDefault(s => s.Def.Name == name);
            if (spec == null) return Fail("الأداة «" + name + "» غير موجودة.", AiToolOutcome.Error, name);
            if (!spec.Allowed(ctx)) return Fail("لا تملك صلاحية استخدام هذه الأداة (الشاشة المقابلة غير مسموحة لك).", AiToolOutcome.Denied, name);
            JsonObject args;
            try { args = JsonNode.Parse(string.IsNullOrWhiteSpace(inputJson) ? "{}" : inputJson) as JsonObject ?? throw new JsonException(); }
            catch (JsonException) { return Fail("مدخلات الأداة ليست كائن JSON صحيحاً.", AiToolOutcome.Error, name); }
            try
            {
                return spec.Run(args, ctx);
            }
            catch (OperationCanceledException) { throw; }
            catch (AiSqlRejectedException ex) { return Fail(ex.Message, AiToolOutcome.Denied, name, Arg(args, "sql")); }
            catch (ArgumentException ex) { return Fail(ArgMessage(ex.Message), AiToolOutcome.Error, name); }
            catch (Exception ex)
            {
                operations.DataDictionaryLogerException(ex);
                return Fail("خطأ أثناء تنفيذ الأداة: " + (ex.InnerException?.Message ?? ex.Message), AiToolOutcome.Error, name);
            }
        }

        private static AiToolResult Fail(string msg, string outcome, string name, string? sql = null) =>
            new() { Content = JsonSerializer.Serialize(new { error = msg }, Json), IsError = true, Outcome = outcome, Sql = sql, Title = TitleOf(name) };

        private static string ArgMessage(string code) => code switch
        {
            "PeriodRequired" => "لا توجد فترة مالية تحتوى التاريخين معاً. استدعِ get_periods_and_branches واختر مدى داخل فترة واحدة.",
            "FromOutsidePeriod" or "ToOutsidePeriod" => "التاريخ خارج الفترة المالية. اختر مدى داخل فترة واحدة.",
            "ToBeforeFrom" => "تاريخ النهاية قبل البداية.",
            "BranchRequired" => "حدّد الفرع.",
            _ => code,
        };

        private static string Arg(JsonObject a, string key) => a[key] is JsonValue v && v.TryGetValue<string>(out var s) ? s.Trim() : "";

        // ───────────── التنفيذ ─────────────

        private AiToolResult SearchSchema(JsonObject a, AiToolContext c)
        {
            string q = Arg(a, "query");
            if (q.Length == 0) throw new ArgumentException("أدخل كلمات البحث.");
            var views = AiSchemaCatalog.Views.Where(v => c.AllowedViews.Contains(v.Name));
            return new AiToolResult { Content = AiSchemaCatalog.Search(q, views), Title = "بحث فى القاموس: " + Short(q, 60) };
        }

        private Dictionary<string, object> Session(AiToolContext c) => new() { ["ai_tax"] = c.TaxFilter, ["ai_user"] = c.User.UserId };

        private AiToolResult RunQuery(JsonObject a, AiToolContext c)
        {
            string sql = Arg(a, "sql"), purpose = Arg(a, "purpose");
            var r = _sql.Execute(sql, c.AllowedViews, Session(c), null, c.Ct);
            return new AiToolResult
            {
                Content = Cap(FormatResult(r)),
                Sql = sql,
                RowCount = r.Rows.Count,
                Title = purpose.Length > 0 ? Short(purpose, 120) : "استعلام قراءة",
            };
        }

        private AiToolResult FindEntity(JsonObject a, AiToolContext c)
        {
            string type = Arg(a, "type"), text = Arg(a, "text");
            if (text.Length == 0) throw new ArgumentException("أدخل جزءاً من الاسم أو الكود.");
            (string view, string id, string name, string code) = type switch
            {
                "customer" => ("v_customers", "customer_id", "customer_name", "customer_code"),
                "supplier" => ("v_suppliers", "supplier_id", "supplier_name", "supplier_code"),
                "item" => ("v_items", "item_id", "item_name", "item_code"),
                "store" => ("v_stores", "store_id", "store_name", "store_code"),
                "branch" => ("v_branches", "branch_id", "branch_name", "branch_code"),
                _ => throw new ArgumentException("نوع غير معروف: " + type),
            };
            if (!c.AllowedViews.Contains(view))
                return Fail("لا تملك صلاحية البحث فى هذا النوع.", AiToolOutcome.Denied, "find_entity");
            // القيمة معامل (@q) لا نص مدمج؛ نفس مسار القراءة والتدقيق.
            string sql = $"SELECT TOP (8) {id} AS id, {name} AS name, CAST({code} AS nvarchar(100)) AS code FROM ai.{view} " +
                         $"WHERE {name} LIKE @q OR CAST({code} AS nvarchar(100)) LIKE @q " +
                         $"ORDER BY CASE WHEN CAST({code} AS nvarchar(100)) = @exact OR {name} = @exact THEN 0 WHEN {name} LIKE @starts THEN 1 ELSE 2 END, {name}";
            string esc = text.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]");
            var p = new Dictionary<string, object> { ["@q"] = "%" + esc + "%", ["@starts"] = esc + "%", ["@exact"] = text };
            var r = _sql.Execute(sql, c.AllowedViews, Session(c), p, c.Ct);
            return new AiToolResult { Content = FormatResult(r), Sql = sql + "  -- @q = " + text, RowCount = r.Rows.Count, Title = "بحث عن «" + Short(text, 40) + "»" };
        }

        private static AiToolResult PeriodsAndBranches(JsonObject a, AiToolContext c)
        {
            var svc = new FinalAccountsService();
            var branches = svc.GetBranches();
            var periods = svc.GetPeriods();
            var o = new
            {
                today = DateTime.Today.ToString("yyyy-MM-dd"),
                user_branch_id = c.User.BranchId,
                branches = branches.Select(b => new { id = b.Id, name = b.Name }),
                periods = periods.Select(p => new { id = p.Id, name = p.Name, from = p.DateFrom.ToString("yyyy-MM-dd"), to = p.DateTo.ToString("yyyy-MM-dd"), active = p.IsActive }),
            };
            return new AiToolResult { Content = JsonSerializer.Serialize(o, Json), RowCount = branches.Count + periods.Count, Title = "الفترات المالية والفروع" };
        }

        /// <summary>الفترة التى تحتوى التاريخين، والفرع مسموح للمستخدم (نفس قائمة فروع الشاشات).</summary>
        private static (FinalAccountsService Svc, FinalAccountsService.PeriodRow Period, long Branch, DateTime From, DateTime To) Range(JsonObject a, AiToolContext c)
        {
            if (!DateTime.TryParseExact(Arg(a, "from"), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var from) ||
                !DateTime.TryParseExact(Arg(a, "to"), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var to))
                throw new ArgumentException("التاريخ يجب أن يكون بصيغة yyyy-MM-dd.");
            var svc = new FinalAccountsService();
            var period = svc.GetPeriods().FirstOrDefault(p => from.Date >= p.DateFrom.Date && to.Date <= p.DateTo.Date)
                         ?? throw new ArgumentException("PeriodRequired");
            long branch = a["branch_id"] is JsonValue bv && bv.TryGetValue<long>(out long b) && b > 0 ? b : c.User.BranchId;
            if (!svc.GetBranches().Any(x => x.Id == branch))
                throw new ArgumentException("الفرع " + branch + " غير مسموح لك أو غير موجود. استدعِ get_periods_and_branches.");
            return (svc, period, branch, from, to);
        }

        private static AiToolResult TrialBalance(JsonObject a, AiToolContext c)
        {
            var (svc, period, branch, from, to) = Range(a, c);
            var filter = Arg(a, "filter") switch
            {
                "with_balance" => FinalAccountsService.BalanceFilter.WithBalance,
                "movement_only" => FinalAccountsService.BalanceFilter.MovementOnly,
                _ => FinalAccountsService.BalanceFilter.All,
            };
            var r = svc.GetTrialBalance(period, branch, FinalAccountsService.EquivalentCurrencyId, from, to, filter);
            object Row(FinalAccountsService.TrialBalanceRow x) => new object[] { x.Code, x.AccountName, x.FirstMadin, x.FirstDa2n, x.Import, x.Export, x.LastMadin, x.LastDa2n };
            var rows = r.Groups.SelectMany(g => g.Rows).ToList();
            var o = new
            {
                period = period.Name, branch_id = branch, from = from.ToString("yyyy-MM-dd"), to = to.ToString("yyyy-MM-dd"),
                columns = new[] { "code", "account", "opening_debit", "opening_credit", "movement_debit", "movement_credit", "closing_debit", "closing_credit" },
                total = Row(r.Total),
                is_balanced = r.IsBalanced,
                groups = r.Groups.Select(g => new { main_code = g.MainCode, main_account = g.MainAccountName, total = Row(g.Total) }),
                account_count = rows.Count,
                accounts = rows.Take(200).Select(Row),
                accounts_truncated = rows.Count > 200,
            };
            return new AiToolResult { Content = Cap(JsonSerializer.Serialize(o, Json)), RowCount = rows.Count, Title = $"ميزان المراجعة {from:yyyy-MM-dd} → {to:yyyy-MM-dd}" };
        }

        private static AiToolResult FinancialStatement(JsonObject a, AiToolContext c)
        {
            string kind = Arg(a, "kind");
            string route = kind switch
            {
                "income" => AiSchemaCatalog.RIncome,
                "profit_loss" => AiSchemaCatalog.RProfitLoss,
                "balance_sheet" => AiSchemaCatalog.RBalanceSheet,
                _ => throw new ArgumentException("نوع القائمة غير معروف: " + kind),
            };
            if (!c.HasRoute(route)) return Fail("لا تملك صلاحية شاشة هذه القائمة.", AiToolOutcome.Denied, "get_financial_statement");
            var (svc, period, branch, from, to) = Range(a, c);
            object Rows(IEnumerable<FinalAccountsService.StatementRow> rows) => rows.Take(250).Select(x => new object[]
                { x.Kind.ToString(), x.Code, string.IsNullOrEmpty(x.Name) ? x.LabelKey : x.Name, x.LastMadin, x.LastDa2n });
            object o;
            int count;
            string title;
            switch (kind)
            {
                case "income":
                    var inc = svc.GetIncomeStatement(period, branch, FinalAccountsService.EquivalentCurrencyId, from, to);
                    o = new { period = period.Name, branch_id = branch, from = from.ToString("yyyy-MM-dd"), to = to.ToString("yyyy-MM-dd"),
                              revenue = inc.Revenue, expenses = inc.Expenses, net_profit = inc.NetProfit,
                              columns = new[] { "kind", "code", "name", "debit", "credit" }, rows = Rows(inc.Rows) };
                    count = inc.Rows.Count; title = "قائمة الدخل";
                    break;
                case "profit_loss":
                    var pl = svc.GetProfitLoss(period, branch, FinalAccountsService.EquivalentCurrencyId, from, to);
                    o = new { period = period.Name, branch_id = branch, from = from.ToString("yyyy-MM-dd"), to = to.ToString("yyyy-MM-dd"), net_profit = pl.NetProfit,
                              columns = new[] { "kind", "code", "name", "debit", "credit" },
                              operating = Rows(pl.Operating), trading = Rows(pl.Trading), profit_loss = Rows(pl.ProfitLoss) };
                    count = pl.Operating.Count + pl.Trading.Count + pl.ProfitLoss.Count; title = "التشغيل والمتاجرة والأرباح والخسائر";
                    break;
                default:
                    var bs = svc.GetBalanceSheet(period, branch);
                    object Sec(FinalAccountsService.BalanceSheetSection s) => new
                    {
                        total = s.Total,
                        lines = s.Roots.SelectMany(r => new[] { r }.Concat(r.Children)).Take(150)
                                       .Select(n => new object[] { n.Depth, n.Code, string.IsNullOrEmpty(n.Name) ? n.LabelKey : n.Name, n.Amount }),
                    };
                    o = new { period = period.Name, branch_id = branch, as_of = bs.AsOf.ToString("yyyy-MM-dd"), columns = new[] { "depth", "code", "name", "amount" },
                              assets = Sec(bs.Assets), liabilities = Sec(bs.Liabilities), equity = Sec(bs.Equity),
                              net_profit = bs.NetProfit, is_balanced = bs.IsBalanced, difference = bs.Difference };
                    count = bs.AccountCount; title = "المركز المالى";
                    break;
            }
            return new AiToolResult { Content = Cap(JsonSerializer.Serialize(o, Json)), RowCount = count, Title = $"{title} {from:yyyy-MM-dd} → {to:yyyy-MM-dd}" };
        }

        // ───────────── التنسيق ─────────────

        private static readonly JsonSerializerOptions Json = new() { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

        /// <summary>أول 50 صفاً + مجاميع الأعمدة الرقمية على كل الصفوف المقروءة (الأرقام الكاملة تبقى على الخادم).</summary>
        public static string FormatResult(AiQueryResult r)
        {
            var o = new JsonObject
            {
                ["columns"] = new JsonArray(r.Columns.Select(cn => (JsonNode)JsonValue.Create(cn)!).ToArray()),
                ["row_count"] = r.Rows.Count,
                ["truncated"] = r.Truncated,
            };
            var rows = new JsonArray();
            foreach (var row in r.Rows.Take(SampleRows))
                rows.Add(new JsonArray(row.Select(Cell).ToArray()));
            o["rows"] = rows;
            if (r.Rows.Count > SampleRows) o["note"] = $"عُرض أول {SampleRows} صفاً فقط من {r.Rows.Count}؛ استخدم التجميع للحصول على إجماليات دقيقة.";
            if (r.Rows.Count > 1)
            {
                var totals = new JsonObject();
                for (int i = 0; i < r.Columns.Count; i++)
                {
                    string cn = r.Columns[i];
                    // المعرّفات والأرقام المرجعية والأكواد لا معنى لجمعها.
                    if (cn.Equals("id", StringComparison.OrdinalIgnoreCase) || cn.EndsWith("_id", StringComparison.OrdinalIgnoreCase) ||
                        cn.EndsWith("_no", StringComparison.OrdinalIgnoreCase) || cn.EndsWith("code", StringComparison.OrdinalIgnoreCase)) continue;
                    var nums = r.Rows.Select(x => x[i]).Where(v => v is decimal or double or float or int or long or short).ToList();
                    if (nums.Count > 0 && nums.Count == r.Rows.Count(x => x[i] != null))
                        totals[r.Columns[i]] = Math.Round(nums.Sum(v => Convert.ToDecimal(v, CultureInfo.InvariantCulture)), 4);
                }
                if (totals.Count > 0) o["column_sums"] = totals;
            }
            return o.ToJsonString(Json);
        }

        private static JsonNode? Cell(object? v) => v switch
        {
            null => null,
            DateTime d => JsonValue.Create(d.TimeOfDay == TimeSpan.Zero ? d.ToString("yyyy-MM-dd") : d.ToString("yyyy-MM-dd HH:mm")),
            DateTimeOffset d => JsonValue.Create(d.ToString("yyyy-MM-dd HH:mm")),
            decimal m => JsonValue.Create(m),
            double f => JsonValue.Create(f),
            float f => JsonValue.Create(f),
            long l => JsonValue.Create(l),
            int n => JsonValue.Create(n),
            short n => JsonValue.Create(n),
            byte n => JsonValue.Create(n),
            bool b => JsonValue.Create(b),
            byte[] => JsonValue.Create("(binary)"),
            _ => JsonValue.Create(Convert.ToString(v, CultureInfo.InvariantCulture)),
        };

        private static string Cap(string s) => s.Length <= MaxContentChars ? s : s[..MaxContentChars] + " …(قُطع الناتج؛ ضيّق الطلب)";
        private static string Short(string s, int max) => s.Length > max ? s[..max] + "…" : s;
    }
}
