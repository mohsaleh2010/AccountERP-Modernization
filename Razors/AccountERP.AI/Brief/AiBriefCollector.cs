using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using AccountERP.AI.Tools;
using AccountModelsLib;

namespace AccountERP.AI.Brief
{
    /// <summary>ناتج الجمع: الحقائق + التنبيهات + أخطاء الأقسام (القسم الفاشل null فى الحقائق والباقى يصدر — ق/§3).</summary>
    public sealed class AiBriefCollectResult
    {
        public required AiBriefFacts Facts { get; init; }
        public List<AiBriefAlert> Alerts { get; init; } = new();
        /// <summary>مجموعة الحقائق ← رسالة الخطأ.</summary>
        public Dictionary<string, string> Errors { get; } = new(StringComparer.OrdinalIgnoreCase);
        public int DurationMs { get; set; }
        public int Queries { get; set; }

        /// <summary>Ready بلا أخطاء، Failed لو فشلت كل المجموعات، وإلا Partial.</summary>
        public string Status(int groupCount) =>
            Errors.Count == 0 ? AiBriefStatus.Ready : Errors.Count >= Math.Max(1, groupCount) ? AiBriefStatus.Failed : AiBriefStatus.Partial;

        public string FactsJson => JsonSerializer.Serialize(Facts, AiBriefCollector.Json);
        public string AlertsJson => JsonSerializer.Serialize(Alerts, AiBriefCollector.Json);
        public string? ErrorsJson => Errors.Count == 0 ? null : JsonSerializer.Serialize(Errors, AiBriefCollector.Json);
    }

    /// <summary>
    /// P8.1.2 — جامع حقائق الموجز بلا نموذج (ق4، ق18): استعلامات ثابتة مكتوبة بأيدينا عبر ReadOnlySqlExecutor بصلاحيات المستلم
    /// (AllowedViews + ai_tax + ai_box_perm) ومحرك شاشات الخزينة (BoxTotalsPerDay / BoxBalance) داخل جلسته.
    /// كل استعلام يُسجَّل فى AI_ToolCalls_Audit باسم brief_query (ق26). فشل مجموعة لا يُسقط الباقى.
    /// لا كتابة فى جداول النظام ولا استدعاء SetFirstBalanceForAllCustomers.
    /// </summary>
    public sealed class AiBriefCollector
    {
        public const string AuditTool = "brief_query";
        public const int TopN = 5;

        internal static readonly JsonSerializerOptions Json = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        private readonly ReadOnlySqlExecutor _sql;
        private readonly AiToolAuditCLS _audit;

        public AiBriefCollector(ReadOnlySqlExecutor sql, AiToolAuditCLS? audit = null)
        {
            _sql = sql;
            _audit = audit ?? new AiToolAuditCLS();
        }

        /// <summary>P8.4 خطافا اختبار: تفعيل الكشف وأعداد AI_Anomalies (بدل AI_Settings/AiAnomalyCLS).</summary>
        public Func<bool>? AnomalyEnabled { get; set; }
        public Func<IList<long>, DateTime, AiAnomalyBriefCounts>? AnomalySource { get; set; }
        /// <summary>P8.5: حاسب أولويات التحصيل (غائب ⇒ أصفار) وخطافا اختبار.</summary>
        public Collections.AiCollectionsScorer? Collections { get; set; }
        public Func<bool>? CollectEnabled { get; set; }
        public Func<AiBriefScope, AiCollectBriefCounts>? CollectSource { get; set; }

        public static AiBriefFacts? ParseFacts(string? json) =>
            string.IsNullOrWhiteSpace(json) ? null : JsonSerializer.Deserialize<AiBriefFacts>(json, Json);

        public static List<AiBriefAlert> ParseAlerts(string? json) =>
            string.IsNullOrWhiteSpace(json) ? new() : JsonSerializer.Deserialize<List<AiBriefAlert>>(json, Json) ?? new();

        /// <summary>
        /// يجمع حقائق [from, to] للنطاق ثم يقيّم التنبيهات. today = «اليوم» بتوقيت الموجز (الاستحقاق ورصيد الآن).
        /// auditUserId = أول مستلم فى النطاق (ق26).
        /// </summary>
        public AiBriefCollectResult Collect(AiBriefScope scope, DateTime from, DateTime to, DateTime today, AiBriefSettings settings,
                                            long auditUserId, CancellationToken ct)
        {
            var sw = Stopwatch.StartNew();
            var facts = new AiBriefFacts
            {
                PeriodFrom = from.Date, PeriodTo = to.Date, Today = today.Date,
                BranchIds = scope.BranchIds.ToList(), Sections = scope.Sections.ToList(),
            };
            var result = new AiBriefCollectResult { Facts = facts };
            var run = new Run(this, scope, from.Date, to.Date, today.Date, auditUserId, ct, result);

            var prevCulture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;   // محركات الخزينة تكتب الأرقام نصاً
            try
            {
                Group(result, AiBriefSections.Sales, scope, ct, () => facts.Sales = run.Sales());
                BoxTotals? boxes = null;
                if (scope.Computes(AiBriefSections.Collections) || scope.Computes(AiBriefSections.Liquidity))
                {
                    string g = scope.Computes(AiBriefSections.Collections) ? AiBriefSections.Collections : AiBriefSections.Liquidity;
                    Group(result, g, scope, ct, () => boxes = run.Boxes(scope.Computes(AiBriefSections.Collections)));
                    if (boxes == null && g == AiBriefSections.Collections && scope.Computes(AiBriefSections.Liquidity))
                        result.Errors[AiBriefSections.Liquidity] = result.Errors[g];
                }
                if (boxes != null)
                {
                    Group(result, AiBriefSections.Collections, scope, ct, () => facts.Collections = run.Collections(boxes));
                    Group(result, AiBriefSections.Liquidity, scope, ct, () => facts.Liquidity = Liquidity(boxes));
                }
                Group(result, AiBriefSections.Receivables, scope, ct, () => facts.Receivables = run.Receivables());
                Group(result, AiBriefSections.Purchases, scope, ct, () => facts.Purchases = run.Purchases());
                Group(result, AiBriefScopes.StockGroup, scope, ct, () => facts.Stock = run.Stock());
                Group(result, AiBriefScopes.AnomalyGroup, scope, ct, () => facts.Anomalies = run.Anomalies());   // P8.4 (ش27)
                Group(result, AiBriefScopes.CollectGroup, scope, ct, () => facts.Collect = run.Collect());   // P8.5 (ت31)
            }
            finally { CultureInfo.CurrentCulture = prevCulture; }

            if (scope.Shows(AiBriefSections.Alerts))
                result.Alerts.AddRange(AiBriefAlerts.Evaluate(facts, settings, scope.HasRoute));
            sw.Stop();
            result.DurationMs = (int)Math.Min(int.MaxValue, sw.ElapsedMilliseconds);
            return result;
        }

        private static void Group(AiBriefCollectResult result, string group, AiBriefScope scope, CancellationToken ct, Action body)
        {
            if (!scope.Computes(group) || result.Errors.ContainsKey(group)) return;
            ct.ThrowIfCancellationRequested();
            try { body(); }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                result.Errors[group] = ex is AiSqlRejectedException ? ex.Message : ex.GetType().Name + ": " + ex.Message;
                if (ex is not AiSqlRejectedException) { try { operations.DataDictionaryLogerException(ex); } catch { } }
            }
        }

        // ─────────────── الخزينة (محرك الشاشة) ───────────────

        /// <summary>مخرجات محرك «إجمالى يوميات الخزينة» لكل صندوق: الفترة، والثلاثون يوماً قبلها، والرصيد الآن.</summary>
        internal sealed class BoxTotals
        {
            public readonly List<(AiBriefBox Box, decimal First, decimal Receipts, decimal Payments, decimal Final, decimal Payments30, decimal Now)> Rows = new();
        }

        internal static AiBriefLiquidityFacts Liquidity(BoxTotals t)
        {
            var f = new AiBriefLiquidityFacts();
            foreach (var r in t.Rows)
                f.Boxes.Add(new AiBriefBoxBalance { BoxId = r.Box.Id, BoxName = r.Box.Name, BranchId = r.Box.BranchId, CurrencyId = r.Box.CurrencyId,
                                                    Balance = r.Now, PeriodStart = r.First, PeriodEnd = r.Final });
            foreach (var g in t.Rows.GroupBy(r => r.Box.CurrencyId).OrderBy(g => g.Key))
            {
                var first = g.First().Box;
                decimal bal = g.Sum(r => r.Now), start = g.Sum(r => r.First);
                f.ByCurrency.Add(new AiBriefCurrencyBalance { CurrencyId = g.Key, CurrencyName = first.CurrencyName, CurrencyNameEn = first.CurrencyNameEn,
                                                             Balance = bal, PeriodStart = start, Change = bal - start });
            }
            return f;
        }

        /// <summary>جلسة جمع واحدة (النطاق + الفترة + التدقيق).</summary>
        private sealed class Run(AiBriefCollector owner, AiBriefScope scope, DateTime from, DateTime to, DateTime today, long auditUserId,
                                 CancellationToken ct, AiBriefCollectResult result)
        {
            private Dictionary<string, object>? _session;
            private Dictionary<string, object> Session => _session ??= ReadOnlySqlExecutor.SessionFor(scope.TaxFilter, scope.UserId);

            // ─────────────── المبيعات ───────────────

            public AiBriefSalesFacts Sales()
            {
                var s = new AiBriefSalesFacts();
                var (bIn, bp) = InList("branch_id", "b", scope.BranchIds);
                DateTime baseFrom = from.AddDays(-28);

                // الأيام من بداية نافذة المقارنة حتى آخر الفترة (يوم بلا فواتير لا يظهر ⇒ صفر).
                var daily = Q(AiBriefSections.Sales, "sales_daily", $@"
SELECT invoice_date, COUNT(*) AS invoices, SUM(ISNULL(net_total_equivalent, 0)) AS net,
       SUM(CASE WHEN is_credit = 1 THEN ISNULL(net_total_equivalent, 0) ELSE 0 END) AS credit_net,
       SUM(CASE WHEN is_credit = 1 THEN 1 ELSE 0 END) AS credit_invoices
FROM ai.v_sales
WHERE invoice_date >= @base_from AND invoice_date <= @date_to AND {bIn}
GROUP BY invoice_date", With(bp, ("@base_from", baseFrom), ("@date_to", to)));
                var byDay = new Dictionary<DateTime, (int N, decimal Net, decimal Credit, int CreditN)>();
                foreach (var r in daily.Rows)
                    if (r[0] is DateTime d) byDay[d.Date] = (I(r[1]), D(r[2]), D(r[3]), I(r[4]));

                decimal baseline = 0;
                bool anyBase = byDay.Keys.Any(d => d < from);
                for (var d = from; d <= to; d = d.AddDays(1))
                {
                    var v = byDay.TryGetValue(d, out var x) ? x : default;
                    s.Days.Add(new AiBriefDaySales { Date = d, Invoices = v.N, Net = v.Net });
                    s.Invoices += v.N;
                    s.NetSales += v.Net;
                    s.CreditSales += v.Credit;
                    s.CreditInvoices += v.CreditN;
                    for (int k = 1; k <= 4; k++)
                        if (byDay.TryGetValue(d.AddDays(-7 * k), out var b)) baseline += b.Net;
                }
                s.CashSales = s.NetSales - s.CreditSales;
                s.CashInvoices = s.Invoices - s.CreditInvoices;
                s.AvgInvoice = s.Invoices > 0 ? Math.Round(s.NetSales / s.Invoices, 2) : 0;
                if (anyBase)
                {
                    s.WeekdayBaseline = Math.Round(baseline / 4m, 2);
                    s.VsWeekdayPct = s.WeekdayBaseline > 0 ? Pct(s.NetSales, s.WeekdayBaseline.Value) : null;
                }

                // ق19(ب): الشهر حتى تاريخه مقابل نفس عدد الأيام من الشهر السابق.
                s.MtdFrom = new DateTime(to.Year, to.Month, 1);
                s.MtdTo = to;
                s.PrevMtdFrom = s.MtdFrom.AddMonths(-1);
                s.PrevMtdTo = s.PrevMtdFrom.AddDays(Math.Min(to.Day, DateTime.DaysInMonth(s.PrevMtdFrom.Year, s.PrevMtdFrom.Month)) - 1);
                var mtd = Q(AiBriefSections.Sales, "sales_mtd", $@"
SELECT SUM(CASE WHEN invoice_date >= @mtd_from THEN ISNULL(net_total_equivalent, 0) ELSE 0 END) AS mtd,
       SUM(CASE WHEN invoice_date <= @prev_to THEN ISNULL(net_total_equivalent, 0) ELSE 0 END) AS prev_mtd
FROM ai.v_sales
WHERE ((invoice_date >= @mtd_from AND invoice_date <= @date_to) OR (invoice_date >= @prev_from AND invoice_date <= @prev_to)) AND {bIn}",
                    With(bp, ("@mtd_from", s.MtdFrom), ("@date_to", to), ("@prev_from", s.PrevMtdFrom), ("@prev_to", s.PrevMtdTo)));
                if (mtd.Rows.Count > 0) { s.Mtd = D(mtd.Rows[0][0]); s.PrevMtd = D(mtd.Rows[0][1]); }
                s.VsPrevMonthPct = s.PrevMtd > 0 ? Pct(s.Mtd, s.PrevMtd) : null;

                // المرتجعات وحسب الفرع.
                var period = With(bp, ("@date_from", from), ("@date_to", to));
                var ret = Q(AiBriefSections.Sales, "sales_returns", $@"
SELECT branch_id, COUNT(*) AS returns, SUM(ISNULL(net_total_equivalent, 0)) AS net
FROM ai.v_sales_returns
WHERE return_date >= @date_from AND return_date <= @date_to AND {bIn}
GROUP BY branch_id", period);
                var retByBranch = ret.Rows.ToDictionary(r => L(r[0]), r => (N: I(r[1]), Net: D(r[2])));
                s.ReturnCount = retByBranch.Values.Sum(v => v.N);
                s.Returns = retByBranch.Values.Sum(v => v.Net);
                s.NetAfterReturns = s.NetSales - s.Returns;

                if (scope.BranchIds.Count > 1)
                {
                    var bb = Q(AiBriefSections.Sales, "sales_by_branch", $@"
SELECT branch_id, MAX(branch_name) AS branch_name, COUNT(*) AS invoices, SUM(ISNULL(net_total_equivalent, 0)) AS net
FROM ai.v_sales
WHERE invoice_date >= @date_from AND invoice_date <= @date_to AND {bIn}
GROUP BY branch_id", period);
                    var rows = bb.Rows.ToDictionary(r => L(r[0]), r => (Name: Convert.ToString(r[1]) ?? "", N: I(r[2]), Net: D(r[3])));
                    foreach (var id in scope.BranchIds)
                    {
                        rows.TryGetValue(id, out var v);
                        retByBranch.TryGetValue(id, out var rv);
                        string name = scope.BranchNames.TryGetValue(id, out var bn) && bn.Length > 0 ? bn : v.Name ?? "";
                        s.ByBranch.Add(new AiBriefBranchSales { BranchId = id, BranchName = name, Invoices = v.N, Net = v.Net, Returns = rv.Net });
                    }
                }

                // أعلى العملاء والأصناف.
                var tc = Q(AiBriefSections.Sales, "sales_top_customers", $@"
SELECT TOP {TopN} customer_id, MAX(customer_name) AS customer_name, SUM(ISNULL(net_total_equivalent, 0)) AS net, COUNT(*) AS invoices
FROM ai.v_sales
WHERE invoice_date >= @date_from AND invoice_date <= @date_to AND {bIn}
GROUP BY customer_id
ORDER BY SUM(ISNULL(net_total_equivalent, 0)) DESC", period);
                s.TopCustomers = tc.Rows.Select(r => new AiBriefNamedValue { Id = L(r[0]), Name = Convert.ToString(r[1]) ?? "", Value = D(r[2]), Count = I(r[3]) }).ToList();

                var (lIn, _) = InList("l.branch_id", "b", scope.BranchIds);
                var ti = Q(AiBriefSections.Sales, "sales_top_items", $@"
SELECT TOP {TopN} item_id, MAX(item_name) AS item_name, SUM(ISNULL(line_net, 0)) AS net, COUNT(*) AS lines
FROM ai.v_sales_lines
WHERE invoice_date >= @date_from AND invoice_date <= @date_to AND {bIn}
GROUP BY item_id
ORDER BY SUM(ISNULL(line_net, 0)) DESC", period);
                s.TopItems = ti.Rows.Select(r => new AiBriefNamedValue { Id = L(r[0]), Name = Convert.ToString(r[1]) ?? "", Value = D(r[2]), Count = I(r[3]) }).ToList();

                // هامش تقريبى وسطور تحت التكلفة: avg_cost_at_sale للوحدة الكبرى (P_priceAvgCost) × الكمية بالكبرى. السطر المجانى (قيمة 0) ليس «تحت التكلفة».
                string lines = $@"
WITH x AS (
    SELECT l.item_id, l.item_name, ISNULL(l.line_total, 0) AS amt,
           ISNULL(l.avg_cost_at_sale, 0) * (ISNULL(l.large_qty, 0) + ISNULL(l.small_qty, 0) /
               CASE WHEN ISNULL(i.small_per_large, 0) <= 0 THEN 1 ELSE i.small_per_large END) AS cost
    FROM ai.v_sales_lines l LEFT JOIN ai.v_items i ON i.item_id = l.item_id
    WHERE l.invoice_date >= @date_from AND l.invoice_date <= @date_to AND {lIn})";
                var mg = Q(AiBriefSections.Sales, "sales_margin", lines + @"
SELECT COUNT(*) AS lines, SUM(CASE WHEN cost > 0 THEN 1 ELSE 0 END) AS costed,
       SUM(CASE WHEN cost > 0 THEN amt ELSE 0 END) AS costed_amount, SUM(CASE WHEN cost > 0 THEN cost ELSE 0 END) AS cost,
       SUM(CASE WHEN cost > 0 AND amt > 0 AND amt < cost THEN 1 ELSE 0 END) AS below_lines,
       SUM(CASE WHEN cost > 0 AND amt > 0 AND amt < cost THEN cost - amt ELSE 0 END) AS below_loss
FROM x", period);
                if (mg.Rows.Count > 0)
                {
                    var r = mg.Rows[0];
                    int all = I(r[0]), costed = I(r[1]);
                    decimal amt = D(r[2]), cost = D(r[3]);
                    s.CostCoveragePct = all > 0 ? Math.Round(costed * 100m / all, 1) : 0;
                    if (costed > 0)
                    {
                        s.MarginApprox = Math.Round(amt - cost, 2);
                        s.MarginPct = amt != 0 ? Math.Round((amt - cost) / amt * 100, 1) : null;
                    }
                    s.BelowCostLines = I(r[4]);
                    s.BelowCostLoss = Math.Round(D(r[5]), 2);
                }
                if (s.BelowCostLines > 0)
                {
                    var bc = Q(AiBriefSections.Sales, "sales_below_cost", lines + $@"
SELECT TOP {TopN} item_id, MAX(item_name) AS item_name, SUM(cost - amt) AS loss, COUNT(*) AS lines
FROM x
WHERE cost > 0 AND amt > 0 AND amt < cost
GROUP BY item_id
ORDER BY SUM(cost - amt) DESC", period);
                    s.BelowCostTop = bc.Rows.Select(r => new AiBriefNamedValue { Id = L(r[0]), Name = Convert.ToString(r[1]) ?? "", Value = Math.Round(D(r[2]), 2), Count = I(r[3]) }).ToList();
                }
                return s;
            }

            // ─────────────── الخزينة ───────────────

            /// <summary>لكل صندوق: BoxTotalsPerDay للفترة (+ الثلاثون يوماً السابقة إن طُلبت التحصيلات) وBoxBalance الآن — نفس محرك الشاشات.</summary>
            public BoxTotals Boxes(bool withHistory)
            {
                var t = new BoxTotals();
                var svc = new CashVouchersService();
                var sw = Stopwatch.StartNew();
                foreach (var box in scope.Boxes)
                {
                    ct.ThrowIfCancellationRequested();
                    var p = scope.Server.Run(() => svc.BoxTotalsPerDay(new[] { box.Id }, from, to, true));
                    decimal first = Col(p, "FirstBalance"), rec = Col(p, "Import_Value_Total"), pay = Col(p, "Export_Value_Total"), fin = Col(p, "FinalBalance");
                    decimal pay30 = 0;
                    if (withHistory)
                    {
                        var h = scope.Server.Run(() => svc.BoxTotalsPerDay(new[] { box.Id }, from.AddDays(-30), from.AddDays(-1), true));
                        pay30 = Col(h, "Export_Value_Total");
                    }
                    decimal now = scope.Server.Run(() => svc.BoxBalance(box.Id));
                    t.Rows.Add((box, first, rec, pay, fin, pay30, now));
                }
                sw.Stop();
                Audit("cash_box_totals", $"CashVouchersService.BoxTotalsPerDay(boxes, {Day(from)}, {Day(to)}, totalOnly: true){(withHistory ? " + 30d history" : "")} + BoxBalance",
                      new { boxes = scope.Boxes.Select(b => b.Id).ToList() }, scope.Boxes.Count, sw.ElapsedMilliseconds, AiToolOutcome.Ok, null);
                return t;
            }

            public AiBriefCollectionsFacts Collections(BoxTotals t)
            {
                var f = new AiBriefCollectionsFacts { BoxCount = t.Rows.Count };
                foreach (var g in t.Rows.GroupBy(r => r.Box.CurrencyId).OrderBy(g => g.Key))
                {
                    var first = g.First().Box;
                    decimal rec = g.Sum(r => r.Receipts), pay = g.Sum(r => r.Payments);
                    f.ByCurrency.Add(new AiBriefCurrencyCash
                    {
                        CurrencyId = g.Key, CurrencyName = first.CurrencyName, CurrencyNameEn = first.CurrencyNameEn,
                        Receipts = rec, Payments = pay, Net = rec - pay, PaymentsAvgPerDay30 = Math.Round(g.Sum(r => r.Payments30) / 30m, 2),
                    });
                }
                if (scope.Boxes.Count > 0 && scope.AllowedViews.Contains("v_cash_vouchers"))
                {
                    var (xIn, xp) = InList("box_id", "x", scope.Boxes.Select(b => b.Id).ToList());
                    var tr = Q(AiBriefSections.Collections, "cash_top_receipts", $@"
SELECT TOP {TopN} party_id, MAX(party_name) AS party_name, SUM(ISNULL(amount_equivalent, 0)) AS amount, COUNT(DISTINCT voucher_id) AS vouchers
FROM ai.v_cash_vouchers
WHERE is_receipt = 1 AND party_type = 'customer' AND voucher_date >= @date_from AND voucher_date <= @date_to AND {xIn}
GROUP BY party_id
ORDER BY SUM(ISNULL(amount_equivalent, 0)) DESC", With(xp, ("@date_from", from), ("@date_to", to)));
                    f.TopReceipts = tr.Rows.Select(r => new AiBriefNamedValue { Id = L(r[0]), Name = Convert.ToString(r[1]) ?? "", Value = D(r[2]), Count = I(r[3]) }).ToList();
                }
                return f;
            }

            // ─────────────── الآجل / المشتريات / المخزون ───────────────

            public AiBriefReceivablesFacts Receivables()
            {
                var f = new AiBriefReceivablesFacts { FromDate = today.AddMonths(-12) };
                var (bIn, bp) = InList("branch_id", "b", scope.BranchIds);
                var p = With(bp, ("@r_from", f.FromDate), ("@today", today));
                const string open = "is_credit = 1 AND rest_value > 0 AND invoice_date >= @r_from AND invoice_date <= @today";
                var o = Q(AiBriefSections.Receivables, "receivables_open", $@"
SELECT COUNT(*) AS invoices, SUM(ISNULL(rest_value, 0)) AS rest, SUM(CASE WHEN due_date IS NOT NULL THEN 1 ELSE 0 END) AS with_due
FROM ai.v_sales WHERE {open} AND {bIn}", p);
                if (o.Rows.Count > 0) { f.OpenCount = I(o.Rows[0][0]); f.OpenCredit = D(o.Rows[0][1]); f.WithDueDate = I(o.Rows[0][2]); }
                var od = Q(AiBriefSections.Receivables, "receivables_overdue", $@"
SELECT DATEDIFF(day, due_date, @today) AS days_overdue, COUNT(*) AS invoices, SUM(ISNULL(rest_value, 0)) AS rest
FROM ai.v_sales WHERE {open} AND due_date < @today AND {bIn}
GROUP BY DATEDIFF(day, due_date, @today)", p);
                f.Overdue = od.Rows.Select(r => new AiBriefOverdueBucket { DaysOverdue = I(r[0]), Count = I(r[1]), Amount = D(r[2]) }).OrderBy(b => b.DaysOverdue).ToList();
                return f;
            }

            public AiBriefPurchasesFacts Purchases()
            {
                var f = new AiBriefPurchasesFacts();
                var (bIn, bp) = InList("branch_id", "b", scope.BranchIds);
                var p = With(bp, ("@date_from", from), ("@date_to", to));
                var h = Q(AiBriefSections.Purchases, "purchases_total", $@"
SELECT COUNT(*) AS invoices, SUM(ISNULL(net_total_equivalent, 0)) AS net
FROM ai.v_purchases WHERE invoice_date >= @date_from AND invoice_date <= @date_to AND {bIn}", p);
                if (h.Rows.Count > 0) { f.Invoices = I(h.Rows[0][0]); f.NetPurchases = D(h.Rows[0][1]); }
                var r = Q(AiBriefSections.Purchases, "purchases_returns", $@"
SELECT COUNT(*) AS returns, SUM(ISNULL(net_total_equivalent, 0)) AS net
FROM ai.v_purchase_returns WHERE return_date >= @date_from AND return_date <= @date_to AND {bIn}", p);
                if (r.Rows.Count > 0) { f.ReturnCount = I(r.Rows[0][0]); f.Returns = D(r.Rows[0][1]); }
                return f;
            }

            /// <summary>P8.4: أعداد AI_Anomalies (جدولنا؛ ليس عبر الـ Views) منذ بداية الفترة. الميزة موقوفة ⇒ أصفار.</summary>
            public AiBriefAnomalyFacts Anomalies()
            {
                var f = new AiBriefAnomalyFacts();
                var sw = Stopwatch.StartNew();
                bool on = owner.AnomalyEnabled?.Invoke() ?? AiAnomalySettings.From(new AiProvidersCLS().GetSettings()).Enabled;
                if (!on) return f;
                var c = owner.AnomalySource != null ? owner.AnomalySource(scope.BranchIds, from) : new AiAnomalyCLS().BriefCounts(scope.BranchIds, from);
                f.NewCount = c.NewCount; f.HighCount = c.HighCount; f.OpenTotal = c.OpenTotal;
                f.Top = c.Top.Select(t => "#" + (t.EntryNo?.ToString(CultureInfo.InvariantCulture) ?? t.EntryId.ToString(CultureInfo.InvariantCulture))
                                          + (string.IsNullOrWhiteSpace(t.AccountName) ? "" : " — " + t.AccountName)).ToList();
                result.Queries++;
                Audit("entry_anomalies", "AiAnomalyCLS.BriefCounts(branches, " + Day(from) + ")", new { group = AiBriefScopes.AnomalyGroup, query = "entry_anomalies", from = Day(from), to = Day(to) },
                      c.OpenTotal, sw.ElapsedMilliseconds, AiToolOutcome.Ok, null);
                return f;
            }

            /// <summary>P8.5: أعداد أولويات التحصيل بنطاق المستلم (فروعه وصلاحيته الضريبية). التنبيه موقوف أو لا حاسب ⇒ أصفار.</summary>
            public AiBriefCollectFacts Collect()
            {
                var f = new AiBriefCollectFacts();
                var sw = Stopwatch.StartNew();
                AiCollectSettings? s = null;
                bool on = owner.CollectEnabled?.Invoke() ?? (s = AiCollectSettings.From(new AiProvidersCLS().GetSettings())).BriefAlert;
                if (!on) return f;
                AiCollectBriefCounts c;
                if (owner.CollectSource != null) c = owner.CollectSource(scope);
                else if (owner.Collections != null)
                {
                    s ??= AiCollectSettings.From(new AiProvidersCLS().GetSettings());
                    var cs = new Collections.AiCollectScope
                    {
                        UserId = scope.Server.UserId, IsAdmin = scope.Server.User.IsAdmin, TaxFilter = scope.TaxFilter,
                        BranchIds = scope.Server.User.IsAdmin ? null : scope.BranchIds.ToList(),
                    };
                    c = owner.Collections.BriefCounts(cs, s, ct);
                }
                else return f;
                f.HighCount = c.HighCount; f.HighAmount = c.HighAmount; f.OverdueCustomers = c.OverdueCustomers; f.OverdueAmount = c.OverdueAmount;
                f.PromisesDueToday = c.PromisesDueToday; f.BrokenPromises = c.BrokenPromises;
                f.Top = c.Top.Select(t => new AiBriefNamedValue { Name = t.Name ?? "", Value = t.Overdue }).ToList();
                result.Queries++;
                Audit("collections_priority", "AiCollectionsScorer.BriefCounts(scope)", new { group = AiBriefScopes.CollectGroup, query = "collections_priority", from = Day(from), to = Day(to) },
                      c.OverdueCustomers, sw.ElapsedMilliseconds, AiToolOutcome.Ok, null);
                return f;
            }

            public AiBriefStockFacts Stock()
            {
                var f = new AiBriefStockFacts();
                var (bIn, bp) = InList("branch_id", "b", scope.BranchIds);
                var c = Q(AiBriefScopes.StockGroup, "stock_negative_count", $"SELECT COUNT(*) AS rows_negative FROM ai.v_stock_balance WHERE total_small_qty < 0 AND {bIn}", bp);
                f.NegativeCount = c.Rows.Count > 0 ? I(c.Rows[0][0]) : 0;
                if (f.NegativeCount > 0)
                {
                    var t = Q(AiBriefScopes.StockGroup, "stock_negative_top", $@"
SELECT TOP {TopN} item_id, item_name, store_name, large_qty, small_qty
FROM ai.v_stock_balance WHERE total_small_qty < 0 AND {bIn}
ORDER BY total_small_qty ASC, item_id", bp);
                    f.NegativeTop = t.Rows.Select(r => new AiBriefNegativeItem { ItemId = L(r[0]), ItemName = Convert.ToString(r[1]) ?? "", StoreName = Convert.ToString(r[2]) ?? "",
                                                                                 LargeQty = D(r[3]), SmallQty = D(r[4]) }).ToList();
                }
                return f;
            }

            // ─────────────── التنفيذ والتدقيق ───────────────

            private AiQueryResult Q(string group, string name, string sql, Dictionary<string, object> parameters)
            {
                ct.ThrowIfCancellationRequested();
                sql = sql.Trim();
                var args = new { group, query = name, from = Day(from), to = Day(to) };
                try
                {
                    var r = owner._sql.Execute(sql, scope.AllowedViews, Session, parameters, ct);
                    result.Queries++;
                    Audit(name, sql, args, r.Rows.Count, r.DurationMs, AiToolOutcome.Ok, null);
                    return r;
                }
                catch (AiSqlRejectedException ex)
                {
                    Audit(name, sql, args, null, null, AiToolOutcome.Error, ex.Message);
                    throw;
                }
            }

            private void Audit(string name, string sql, object args, int? rows, long? ms, string outcome, string? error)
            {
                try
                {
                    owner._audit.Insert(null, auditUserId, AuditTool, JsonSerializer.Serialize(args, Json), sql, rows,
                                        ms == null ? null : (int)Math.Min(int.MaxValue, ms.Value), outcome, error);
                }
                catch (Exception ex) { try { operations.DataDictionaryLogerException(ex); } catch { } }
            }
        }

        // ─────────────── مساعدات ───────────────

        /// <summary>«col IN (@b0, @b1 …)» بمعاملات (القائمة الفارغة ⇒ شرط لا يتحقق).</summary>
        internal static (string Sql, Dictionary<string, object> Params) InList(string column, string prefix, IReadOnlyList<long> ids)
        {
            var p = new Dictionary<string, object>();
            if (ids.Count == 0) return ("1 = 0", p);
            var names = new List<string>();
            for (int i = 0; i < ids.Count; i++)
            {
                string n = "@" + prefix + i.ToString(CultureInfo.InvariantCulture);
                names.Add(n);
                p[n] = ids[i];
            }
            return ($"{column} IN ({string.Join(", ", names)})", p);
        }

        private static Dictionary<string, object> With(Dictionary<string, object> baseParams, params (string Name, object Value)[] extra)
        {
            var p = new Dictionary<string, object>(baseParams);
            foreach (var (n, v) in extra) p[n] = v;
            return p;
        }

        private static decimal Col(DataTable dt, string col)
        {
            decimal sum = 0;
            if (!dt.Columns.Contains(col)) return 0;
            foreach (DataRow r in dt.Rows)
                if (decimal.TryParse(Convert.ToString(r[col], CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out var v)) sum += v;
            return sum;
        }

        private static decimal Pct(decimal value, decimal baseValue) => Math.Round((value - baseValue) / baseValue * 100, 1);
        private static decimal D(object? v) => v == null || v is DBNull ? 0 : Convert.ToDecimal(v, CultureInfo.InvariantCulture);
        private static int I(object? v) => v == null || v is DBNull ? 0 : Convert.ToInt32(v, CultureInfo.InvariantCulture);
        private static long L(object? v) => v == null || v is DBNull ? 0 : Convert.ToInt64(v, CultureInfo.InvariantCulture);
        private static string Day(DateTime d) => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }
}
