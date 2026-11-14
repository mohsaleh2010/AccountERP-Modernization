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
        /// <summary>P3: المحادثة الحالية (يُضبط بعد إنشائها) لربط التقارير المرسومة بها.</summary>
        public long? ConversationId { get; set; }
        /// <summary>P3: التقارير التى رُسمت أثناء هذه الرسالة (تُربط برسالة المساعد بعد حفظها).</summary>
        public List<long> ReportIds { get; } = new();
        /// <summary>P5: المسودات التى أُعدّت أثناء هذه الرسالة (تُربط برسالة المساعد بعد حفظها).</summary>
        public List<long> DraftIds { get; } = new();
        /// <summary>P5: أنواع أذون المخزن المسموحة (تُحسب مرة لكل رسالة).</summary>
        internal List<AiDraftTools.OrderTypeInfo>? CachedOrderTypes { get; set; }
        /// <summary>P6: مرفقات المحادثة الحالية (لأداة extract_document؛ فارغة ⇒ الأداة لا تُعرض).</summary>
        public List<AiAttachmentView> Attachments { get; set; } = new();

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
        /// <summary>P3: البيانات الكاملة (تبقى على الخادم؛ النموذج يرى dataset_id وعيّنة).</summary>
        public AiDataset? Dataset { get; init; }
        /// <summary>P3: كتلة تُضاف لمحتوى رد المساعد (بطاقة تقرير أو «افتح الشاشة»).</summary>
        public JsonObject? Block { get; init; }
        /// <summary>P4: ask_user — الحلقة تتوقف هنا حتى يجيب المستخدم (Block = بطاقة السؤال).</summary>
        public bool PauseForUser { get; init; }
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
        private readonly AiDatasetCache _datasets;
        private readonly AiReportsCLS _reports = new();
        private readonly Files.AiDocumentExtractor? _extractor;
        private readonly Knowledge.AiKnowledgeService _knowledge;
        public AiToolRegistry(ReadOnlySqlExecutor sql, AiDatasetCache datasets, Files.AiDocumentExtractor? extractor = null,
            Knowledge.AiKnowledgeService? knowledge = null)
        {
            _sql = sql;
            _datasets = datasets;
            _extractor = extractor;
            _knowledge = knowledge ?? new Knowledge.AiKnowledgeService();
        }

        public Knowledge.AiKnowledgeService KnowledgeService => _knowledge;

        public ReadOnlySqlExecutor Sql => _sql;
        public AiDatasetCache Datasets => _datasets;

        /// <summary>RunAsync (P6) للأدوات التى تستدعى نموذجاً (extract_document)؛ Run للباقى.</summary>
        private sealed record ToolSpec(AiToolDef Def, Func<AiToolContext, bool> Allowed, Func<JsonObject, AiToolContext, AiToolResult> Run)
        {
            public Func<JsonObject, AiToolContext, Task<AiToolResult>>? RunAsync { get; init; }
        }

        private static JsonObject Str(string desc) => new() { ["type"] = "string", ["description"] = desc };
        private static JsonObject Enum(string desc, params string[] values) =>
            new() { ["type"] = "string", ["description"] = desc, ["enum"] = new JsonArray(values.Select(v => (JsonNode)JsonValue.Create(v)!).ToArray()) };
        private static JsonObject Int(string desc) => new() { ["type"] = "integer", ["description"] = desc };
        private static JsonObject Schema(JsonObject props, params string[] required) =>
            new() { ["type"] = "object", ["properties"] = props, ["required"] = new JsonArray(required.Select(r => (JsonNode)JsonValue.Create(r)!).ToArray()) };

        private IEnumerable<ToolSpec> Specs(AiToolContext ctx)
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
                Description = "نفّذ استعلام SELECT واحداً (T-SQL، SQL Server) على Views المساعد ai.v_... فقط. ممنوع أى تعديل. يعيد الأعمدة وعدد الصفوف وأول 50 صفاً ومجاميع الأعمدة الرقمية. استخدم التجميع (SUM/COUNT/GROUP BY) بدل جلب صفوف كثيرة، والتواريخ بصيغة 'yyyy-MM-dd'. " +
                              "الناتج يبدأ بـ dataset_id لاستخدامه فى render_report. إن كان الاستعلام لفترة فمرّر date_from وdate_to واستخدم داخله المعاملين @date_from و@date_to بدل كتابة التاريخين " +
                              "(مثال: WHERE invoice_date >= @date_from AND invoice_date < DATEADD(day, 1, @date_to))، فيمكن إعادة تشغيل التقرير بفترة أخرى.",
                Schema = Schema(new JsonObject
                {
                    ["sql"] = Str("استعلام SELECT واحد"),
                    ["purpose"] = Str("ماذا يحسب هذا الاستعلام بالعربية (يظهر للمستخدم)"),
                    ["date_from"] = Str("بداية الفترة yyyy-MM-dd = المعامل @date_from (اختيارى)"),
                    ["date_to"] = Str("نهاية الفترة yyyy-MM-dd = المعامل @date_to (اختيارى)"),
                }, "sql", "purpose"),
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
            }, c => c.AllowedViews.Count > 0 || AiDraftTools.Any(c), FindEntity);

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

            // ───── P3: التقارير وفتح الشاشات ─────
            yield return new ToolSpec(new AiToolDef
            {
                Name = "render_report",
                Description = "اعرض للمستخدم تقريراً منسّقاً (مؤشرات، جداول، رسوم بيانية bar/line/pie/stacked، ملاحظات) من datasets أعادتها أدوات البيانات (dataset_id). " +
                              "أنت تحدد الشكل فقط؛ الأرقام تُحسب على الخادم من البيانات الكاملة. استخدمها حين يطلب المستخدم تقريراً أو جدولاً أو رسماً، أو حين تكون النتيجة أكثر من بضعة صفوف. " +
                              "بعدها لخّص أهم النتائج فى سطرين أو ثلاثة ولا تكرر الجدول فى النص.",
                Schema = AiReportEngine.ToolSchema(),
            }, c => c.AllowedViews.Count > 0 || HasAnyFinancial(c), RenderReport);

            yield return new ToolSpec(new AiToolDef
            {
                Name = "open_screen",
                Description = "اعرض للمستخدم زر «افتح الشاشة» لشاشة من النظام (لا تُفتح تلقائياً). استخدمها حين توجد شاشة أو تقرير قائم يجيب الطلب (ميزان المراجعة، القوائم المالية، فاتورة بعينها...) بدل رسم تقرير. " +
                              "إما screen من القائمة التالية مع params، أو path من نتيجة find_screen (بلا params). الشاشات ذات الباراميترات المسموحة:" + AiScreenCatalog.Describe(ctx.HasRoute),
                Schema = Schema(new JsonObject
                {
                    ["screen"] = Str("مفتاح الشاشة من القائمة (مثل trial_balance)"),
                    ["path"] = Str("أو: مسار شاشة كما أعادته find_screen"),
                    ["params"] = new JsonObject
                    {
                        ["type"] = "object",
                        ["description"] = "باراميترات الشاشة المدعومة فقط",
                        ["properties"] = new JsonObject
                        {
                            [AiScreenCatalog.PFrom] = Str("yyyy-MM-dd"),
                            [AiScreenCatalog.PTo] = Str("yyyy-MM-dd"),
                            [AiScreenCatalog.PBranch] = Int("معرّف الفرع"),
                            [AiScreenCatalog.PMode] = Enum("ميزان المراجعة: بالمجاميع أو بالأرصدة", "totals", "closing"),
                            [AiScreenCatalog.PFilter] = Enum("ميزان المراجعة: الحسابات المعروضة", "all", "with_balance", "movement_only"),
                            [AiScreenCatalog.PId] = Int("معرّف المستند أو العميل"),
                        },
                    },
                }),
            }, c => true, OpenScreen);

            yield return new ToolSpec(new AiToolDef
            {
                Name = "find_screen",
                Description = "ابحث فى شاشات قائمة المستخدم بالاسم (مثل «كشف حساب عميل»، «تحويل بين المخازن») لتحصل على مسارها، ثم اعرضها بـ open_screen.",
                Schema = Schema(new JsonObject { ["text"] = Str("كلمات من اسم الشاشة") }, "text"),
            }, c => c.User.AllowedScreens.Count > 0, FindScreen);

            // ───── P4: قرار المستخدم ─────
            yield return new ToolSpec(new AiToolDef
            {
                Name = AskUserTool,
                Description = "اسأل المستخدم سؤالاً واحداً حين تحتاج قراره قبل أن تكمل: أكثر من مطابقة لاسم عميل/مورد/صنف، أو لا مطابقة، أو فترة/فرع/وحدة غير واضحة وتؤثر على النتيجة. " +
                              "تتوقف المحادثة حتى يجيب، وإجابته تعود نتيجةً لهذه الأداة. أعطِ من 2 إلى 6 خيارات قصيرة يميّز كل منها نفسه (مثل «شركة النور — كود 1203»). " +
                              "لا تكتب السؤال نفسه فى النص (يظهر فى البطاقة)، ولا تسأل عمّا تستطيع معرفته من الأدوات، ولا تسأل أكثر من سؤال فى المرة.",
                Schema = Schema(new JsonObject
                {
                    ["question"] = Str("السؤال بلغة المستخدم، سطر أو سطران"),
                    ["options"] = new JsonObject
                    {
                        ["type"] = "array",
                        ["description"] = "الخيارات (2 إلى 6، كل خيار حتى 120 حرفاً)",
                        ["items"] = new JsonObject { ["type"] = "string" },
                    },
                    ["allow_free_text"] = new JsonObject { ["type"] = "boolean", ["description"] = "يسمح للمستخدم بكتابة إجابة أخرى (الافتراضى true)" },
                    ["multi"] = new JsonObject { ["type"] = "boolean", ["description"] = "يسمح باختيار أكثر من خيار (الافتراضى false)" },
                }, "question", "options"),
            }, _ => true, AskUser);

            // ───── P5: المسودات (لا حفظ ولا أثر على المخزون؛ الشاشة الحقيقية تحفظ) ─────
            yield return new ToolSpec(AiDraftTools.SalesDef(), AiDraftTools.CanSales, AiDraftTools.CreateSales);
            yield return new ToolSpec(AiDraftTools.PurchaseDef(), AiDraftTools.CanPurchase, AiDraftTools.CreatePurchase);
            if (AiDraftTools.CanStoreOrder(ctx))
                yield return new ToolSpec(AiDraftTools.StoreOrderDef(ctx), AiDraftTools.CanStoreOrder, AiDraftTools.CreateStoreOrder);

            // ───── P6: قراءة المرفقات (نموذج رؤية) ─────
            if (_extractor != null && AiExtractTools.Allowed(ctx))
            {
                var extractor = _extractor;
                yield return new ToolSpec(AiExtractTools.Def(ctx), AiExtractTools.Allowed,
                    (_, _) => throw new InvalidOperationException("extract_document تُنفَّذ بـ ExecuteAsync"))
                { RunAsync = (a, c) => AiExtractTools.RunAsync(a, c, extractor) };
            }
        }

        public const string AskUserTool = "ask_user";
        public const int MaxAskOptions = 6, MaxAskOptionChars = 120, MaxAskQuestionChars = 500;

        /// <summary>P4: يتحقق من السؤال ويعيد بطاقته؛ الحلقة (AiChatGateway) تتوقف عند PauseForUser.</summary>
        private static AiToolResult AskUser(JsonObject a, AiToolContext c)
        {
            string q = Arg(a, "question");
            if (q.Length == 0) throw new ArgumentException("اكتب السؤال.");
            if (q.Length > MaxAskQuestionChars) throw new ArgumentException($"السؤال أطول من {MaxAskQuestionChars} حرف؛ اختصره.");
            var opts = new List<string>();
            if (a["options"] is JsonArray arr)
                foreach (var n in arr)
                {
                    string s = n is JsonValue v && v.TryGetValue<string>(out var t) ? string.Join(' ', t.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)) : "";
                    if (s.Length == 0) continue;
                    if (s.Length > MaxAskOptionChars) throw new ArgumentException($"الخيار «{Short(s, 40)}» أطول من {MaxAskOptionChars} حرف؛ اختصره.");
                    if (!opts.Contains(s, StringComparer.OrdinalIgnoreCase)) opts.Add(s);
                }
            if (opts.Count > MaxAskOptions) throw new ArgumentException($"الخيارات أكثر من {MaxAskOptions}؛ ضيّقها أو استخدم find_entity بكلمة أدق.");
            bool free = a["allow_free_text"] is not JsonValue fv || !fv.TryGetValue<bool>(out bool fb) || fb;
            bool multi = a["multi"] is JsonValue mv && mv.TryGetValue<bool>(out bool mb) && mb;
            if (opts.Count < 2 && !free) throw new ArgumentException("أعطِ خيارين على الأقل، أو اسمح بالإجابة الحرة.");
            if (opts.Count < 2) multi = false;
            return new AiToolResult
            {
                Content = "{}",   // النتيجة الحقيقية = إجابة المستخدم عند الاستئناف
                Title = "سؤال للمستخدم: " + Short(q, 80),
                PauseForUser = true,
                Block = new JsonObject
                {
                    ["type"] = "ask",
                    ["question"] = q,
                    ["options"] = new JsonArray(opts.Select(o => (JsonNode)JsonValue.Create(o)!).ToArray()),
                    ["allow_free_text"] = free,
                    ["multi"] = multi,
                },
            };
        }

        private static bool HasAnyFinancial(AiToolContext c) =>
            c.HasRoute(AiSchemaCatalog.RTrialBalance) || c.HasRoute(AiSchemaCatalog.RIncome) || c.HasRoute(AiSchemaCatalog.RProfitLoss) || c.HasRoute(AiSchemaCatalog.RBalanceSheet);

        /// <summary>تعريفات الأدوات المسموحة لهذا المستخدم فقط.</summary>
        public List<AiToolDef> ToolsFor(AiToolContext ctx) => Specs(ctx).Where(s => s.Allowed(ctx)).Select(s => s.Def).ToList();

        public static string TitleOf(string toolName) => toolName switch
        {
            "search_schema" => "بحث فى قاموس البيانات",
            "run_readonly_query" => "استعلام قراءة",
            "find_entity" => "بحث عن كيان",
            "get_periods_and_branches" => "الفترات والفروع",
            "get_trial_balance" => "ميزان المراجعة",
            "get_financial_statement" => "قائمة مالية",
            "render_report" => "رسم تقرير",
            "open_screen" => "فتح شاشة",
            "find_screen" => "بحث عن شاشة",
            AskUserTool => "سؤال للمستخدم",
            AiDraftTools.SalesTool => "مسودة فاتورة مبيعات",
            AiDraftTools.PurchaseTool => "مسودة فاتورة مشتريات",
            AiDraftTools.StoreOrderTool => "مسودة إذن مخزن",
            AiExtractTools.Tool => "قراءة مستند",
            _ => toolName,
        };

        /// <summary>ينفّذ أداة بعد التحقق من الصلاحية والمدخلات. لا يرمى إلا OperationCanceledException.</summary>
        public AiToolResult Execute(string name, string inputJson, AiToolContext ctx) => ExecuteAsync(name, inputJson, ctx).GetAwaiter().GetResult();

        /// <summary>P6: نفس Execute، والأدوات غير المتزامنة (extract_document) تُنتظر.</summary>
        public async Task<AiToolResult> ExecuteAsync(string name, string inputJson, AiToolContext ctx)
        {
            var spec = Specs(ctx).FirstOrDefault(s => s.Def.Name == name);
            if (spec == null) return Fail("الأداة «" + name + "» غير موجودة.", AiToolOutcome.Error, name);
            if (!spec.Allowed(ctx)) return Fail("لا تملك صلاحية استخدام هذه الأداة (الشاشة المقابلة غير مسموحة لك).", AiToolOutcome.Denied, name);
            JsonObject args;
            try { args = JsonNode.Parse(string.IsNullOrWhiteSpace(inputJson) ? "{}" : inputJson) as JsonObject ?? throw new JsonException(); }
            catch (JsonException) { return Fail("مدخلات الأداة ليست كائن JSON صحيحاً.", AiToolOutcome.Error, name); }
            try
            {
                var res = spec.RunAsync != null ? await spec.RunAsync(args, ctx) : spec.Run(args, ctx);
                if (res.Dataset == null || res.IsError) return res;
                // P3: البيانات الكاملة تبقى على الخادم، والنموذج يأخذ معرّفها أول الناتج (يبقى ظاهراً حتى لو قُطع الباقى).
                res.Dataset.Title = res.Title;
                res.Dataset.Tool = name;
                res.Dataset.Args = (JsonObject)args.DeepClone();
                res.Dataset.Sql ??= res.Sql;
                string id = _datasets.Add(ctx.User.UserId, res.Dataset);
                string body = res.Content.TrimStart();
                return new AiToolResult
                {
                    Content = body.StartsWith('{') ? "{\"dataset_id\":\"" + id + "\"," + body[1..].TrimStart() : "{\"dataset_id\":\"" + id + "\"}\n" + body,
                    Outcome = res.Outcome, Sql = res.Sql, RowCount = res.RowCount, Title = res.Title, Dataset = res.Dataset, Block = res.Block,
                };
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
            // P7.1: من AI_Knowledge مع فلترة الصلاحية (الكود احتياطى إن تعذّرت قراءة الجدول).
            return new AiToolResult { Content = _knowledge.Search(q, c.AllowedViews, c.HasRoute), Title = "بحث فى القاموس: " + Short(q, 60) };
        }

        private Dictionary<string, object> Session(AiToolContext c) => new() { ["ai_tax"] = c.TaxFilter, ["ai_user"] = c.User.UserId };

        private AiToolResult RunQuery(JsonObject a, AiToolContext c)
        {
            string sql = Arg(a, "sql"), purpose = Arg(a, "purpose");
            Dictionary<string, object>? p = null;
            string df = Arg(a, "date_from"), dt = Arg(a, "date_to");
            if (df.Length > 0 || dt.Length > 0)
            {
                if (!DateTime.TryParseExact(df, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var from) ||
                    !DateTime.TryParseExact(dt, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var to))
                    throw new ArgumentException("date_from وdate_to يُمرَّران معاً بصيغة yyyy-MM-dd.");
                if (to < from) throw new ArgumentException("ToBeforeFrom");
                p = new Dictionary<string, object> { ["@date_from"] = from, ["@date_to"] = to };
            }
            var r = _sql.Execute(sql, c.AllowedViews, Session(c), p, c.Ct);
            return new AiToolResult
            {
                Content = Cap(FormatResult(r)),
                Sql = p == null ? sql : sql + $"  -- @date_from = {df}, @date_to = {dt}",
                RowCount = r.Rows.Count,
                Title = purpose.Length > 0 ? Short(purpose, 120) : "استعلام قراءة",
                Dataset = AiDataset.FromQuery(r),
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
            // P5: من يملك شاشة مستند (فاتورة/إذن) يبحث عن كياناته ولو لم يملك شاشة القوائم.
            var views = new HashSet<string>(c.AllowedViews, StringComparer.OrdinalIgnoreCase);
            views.UnionWith(AiDraftTools.ImpliedViews(c));
            if (!views.Contains(view))
                return Fail("لا تملك صلاحية البحث فى هذا النوع.", AiToolOutcome.Denied, "find_entity");
            // القيمة معامل (@q) لا نص مدمج؛ نفس مسار القراءة والتدقيق.
            string sql = $"SELECT TOP (8) {id} AS id, {name} AS name, CAST({code} AS nvarchar(100)) AS code FROM ai.{view} " +
                         $"WHERE {name} LIKE @q OR CAST({code} AS nvarchar(100)) LIKE @q " +
                         $"ORDER BY CASE WHEN CAST({code} AS nvarchar(100)) = @exact OR {name} = @exact THEN 0 WHEN {name} LIKE @starts THEN 1 ELSE 2 END, {name}";
            string esc = text.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]");
            var p = new Dictionary<string, object> { ["@q"] = "%" + esc + "%", ["@starts"] = esc + "%", ["@exact"] = text };
            var r = _sql.Execute(sql, views, Session(c), p, c.Ct);
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
            var ds = AiDataset.FromRows(
                new[] { "code", "account", "main_account", "opening_debit", "opening_credit", "movement_debit", "movement_credit", "closing_debit", "closing_credit" },
                new[] { AiDataset.TText, AiDataset.TText, AiDataset.TText, AiDataset.TNumber, AiDataset.TNumber, AiDataset.TNumber, AiDataset.TNumber, AiDataset.TNumber, AiDataset.TNumber },
                r.Groups.SelectMany(g => g.Rows.Select(x => new object?[] { x.Code, x.AccountName, g.MainAccountName, x.FirstMadin, x.FirstDa2n, x.Import, x.Export, x.LastMadin, x.LastDa2n })));
            return new AiToolResult { Content = Cap(JsonSerializer.Serialize(o, Json)), RowCount = rows.Count, Title = $"ميزان المراجعة {from:yyyy-MM-dd} → {to:yyyy-MM-dd}", Dataset = ds };
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
            AiDataset ds;
            static object?[] SRow(string section, FinalAccountsService.StatementRow x) =>
                new object?[] { section, x.Kind.ToString(), x.Code, string.IsNullOrEmpty(x.Name) ? x.LabelKey : x.Name, x.LastMadin, x.LastDa2n };
            string[] sCols = { "section", "kind", "code", "name", "debit", "credit" };
            string[] sTypes = { AiDataset.TText, AiDataset.TText, AiDataset.TText, AiDataset.TText, AiDataset.TNumber, AiDataset.TNumber };
            switch (kind)
            {
                case "income":
                    var inc = svc.GetIncomeStatement(period, branch, FinalAccountsService.EquivalentCurrencyId, from, to);
                    o = new { period = period.Name, branch_id = branch, from = from.ToString("yyyy-MM-dd"), to = to.ToString("yyyy-MM-dd"),
                              revenue = inc.Revenue, expenses = inc.Expenses, net_profit = inc.NetProfit,
                              columns = new[] { "kind", "code", "name", "debit", "credit" }, rows = Rows(inc.Rows) };
                    count = inc.Rows.Count; title = "قائمة الدخل";
                    ds = AiDataset.FromRows(sCols, sTypes, inc.Rows.Select(x => SRow("income", x)));
                    break;
                case "profit_loss":
                    var pl = svc.GetProfitLoss(period, branch, FinalAccountsService.EquivalentCurrencyId, from, to);
                    o = new { period = period.Name, branch_id = branch, from = from.ToString("yyyy-MM-dd"), to = to.ToString("yyyy-MM-dd"), net_profit = pl.NetProfit,
                              columns = new[] { "kind", "code", "name", "debit", "credit" },
                              operating = Rows(pl.Operating), trading = Rows(pl.Trading), profit_loss = Rows(pl.ProfitLoss) };
                    count = pl.Operating.Count + pl.Trading.Count + pl.ProfitLoss.Count; title = "التشغيل والمتاجرة والأرباح والخسائر";
                    ds = AiDataset.FromRows(sCols, sTypes, pl.Operating.Select(x => SRow("operating", x)).Concat(pl.Trading.Select(x => SRow("trading", x))).Concat(pl.ProfitLoss.Select(x => SRow("profit_loss", x))));
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
                    IEnumerable<object?[]> BRows(string section, FinalAccountsService.BalanceSheetSection sec) =>
                        sec.Roots.SelectMany(r => new[] { r }.Concat(r.Children)).Select(n => new object?[] { section, n.Depth, n.Code, string.IsNullOrEmpty(n.Name) ? n.LabelKey : n.Name, n.Amount });
                    ds = AiDataset.FromRows(new[] { "section", "depth", "code", "name", "amount" },
                        new[] { AiDataset.TText, AiDataset.TNumber, AiDataset.TText, AiDataset.TText, AiDataset.TNumber },
                        BRows("assets", bs.Assets).Concat(BRows("liabilities", bs.Liabilities)).Concat(BRows("equity", bs.Equity)));
                    break;
            }
            return new AiToolResult { Content = Cap(JsonSerializer.Serialize(o, Json)), RowCount = count, Title = $"{title} {from:yyyy-MM-dd} → {to:yyyy-MM-dd}", Dataset = ds };
        }

        // ───────────── P3: التقارير وفتح الشاشات ─────────────

        private AiToolResult RenderReport(JsonObject a, AiToolContext c)
        {
            var (spec, data) = AiReportEngine.Prepare(a, id => _datasets.Get(id, c.User.UserId));
            var doc = AiReportEngine.Materialize(spec, data);   // يتحقق أن البناء ينجح قبل الحفظ
            var (from, to) = AiReportEngine.PeriodOf(spec, data);
            long id = _reports.Insert(new AiReportRow
            {
                User_Id = c.User.UserId,
                Conversation_Id = c.ConversationId,
                Title = doc.Title,
                SpecJson = spec.ToJsonString(Json),
                SourcesJson = AiReportEngine.SourcesJson(data),
                DataJson = AiReportEngine.DataJson(data),
                PeriodFrom = from,
                PeriodTo = to,
            });
            c.ReportIds.Add(id);
            return new AiToolResult
            {
                Content = JsonSerializer.Serialize(new
                {
                    ok = true,
                    report_id = id.ToString(CultureInfo.InvariantCulture),
                    shown = "عُرض التقرير للمستخدم كبطاقة فيها زر «عرض التقرير» (طباعة، Excel، حفظ، إعادة تشغيل بفترة أخرى).",
                    kpis = doc.Kpis.Select(k => new { label = k.Label, value = k.Value }),
                    next = "لخّص أهم النتائج فى سطرين أو ثلاثة، ولا تكرر الجدول ولا تكتب رابطاً.",
                }, Json),
                RowCount = data.Values.Sum(d => d.Rows.Count),
                Title = "تقرير: " + Short(doc.Title, 100),
                Block = new JsonObject
                {
                    ["type"] = "report",
                    ["report_id"] = id.ToString(CultureInfo.InvariantCulture),
                    ["title"] = doc.Title,
                    ["subtitle"] = from != null && to != null ? $"{from:yyyy-MM-dd} → {to:yyyy-MM-dd}" : doc.Subtitle,
                },
            };
        }

        private static AiToolResult OpenScreen(JsonObject a, AiToolContext c)
        {
            string key = Arg(a, "screen"), path = Arg(a, "path");
            string title, url, detail = "";
            if (key.Length > 0)
            {
                var s = AiScreenCatalog.Find(key) ?? throw new ArgumentException($"الشاشة «{key}» ليست فى القائمة. استخدم find_screen للبحث بالاسم.");
                if (!c.HasRoute(s.Route)) return Fail("لا تملك صلاحية هذه الشاشة.", AiToolOutcome.Denied, "open_screen");
                (url, detail) = AiScreenCatalog.Build(s, a["params"] as JsonObject);
                title = s.TitleAr;
            }
            else if (path.Length > 0)
            {
                // مسار من قائمة المستخدم نفسها فقط (لا روابط خارجية).
                if (!path.StartsWith('/') || path.StartsWith("//") || path.Contains('\\') || path.Contains(':'))
                    return Fail("مسار غير صالح. استخدم find_screen.", AiToolOutcome.Denied, "open_screen");
                var hit = c.User.AllowedScreens.FirstOrDefault(x => string.Equals((x.Path ?? "").Trim(), path, StringComparison.OrdinalIgnoreCase))
                          ?? c.User.AllowedRoutes.Where(r => string.Equals((r ?? "").Trim(), path, StringComparison.OrdinalIgnoreCase))
                                                 .Select(r => new AiScreenRef { Path = r.Trim(), Title = "" }).FirstOrDefault();
                if (hit == null) return Fail("هذا المسار ليس من شاشات قائمتك. استخدم find_screen.", AiToolOutcome.Denied, "open_screen");
                if (a["params"] is JsonObject po && po.Count > 0) throw new ArgumentException("الشاشات من find_screen تُفتح بلا params.");
                url = hit.Path.Trim();
                title = string.IsNullOrWhiteSpace(hit.Title) ? url : hit.Title.Trim();
            }
            else throw new ArgumentException("حدّد screen أو path.");
            return new AiToolResult
            {
                Content = JsonSerializer.Serialize(new { ok = true, shown = "ظهر للمستخدم زر «افتح الشاشة: " + title + "». لا تكتب الرابط فى النص.", title, detail }, Json),
                Title = "فتح شاشة: " + title + (detail.Length > 0 ? " (" + detail + ")" : ""),
                Block = new JsonObject { ["type"] = "screen", ["title"] = title, ["url"] = url, ["detail"] = detail },
            };
        }

        private static AiToolResult FindScreen(JsonObject a, AiToolContext c)
        {
            string text = Arg(a, "text");
            if (text.Length == 0) throw new ArgumentException("أدخل كلمات من اسم الشاشة.");
            var hits = AiScreenCatalog.Search(c.User.AllowedScreens, text);
            return new AiToolResult
            {
                Content = JsonSerializer.Serialize(new
                {
                    matches = hits.Select(h => new { path = h.Path, title = h.Title }),
                    note = hits.Count == 0 ? "لا توجد شاشة مطابقة فى قائمة المستخدم." : null,
                }, Json),
                RowCount = hits.Count,
                Title = "بحث عن شاشة «" + Short(text, 40) + "»",
            };
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
