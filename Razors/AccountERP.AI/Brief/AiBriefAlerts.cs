using System.Globalization;
using AccountERP.AI.Tools;
using AccountModelsLib;

namespace AccountERP.AI.Brief
{
    /// <summary>
    /// P8.1.2 (ق20): قواعد التنبيه الثابتة على حقائق الفترة الحالية والحالة الراهنة فقط — دالة نقية بلا قاعدة بيانات.
    /// تنبيه بلا شاشته المانحة لا يظهر (hasRoute)، وتنبيه بلا حقائقه (القسم لم يُحسب أو فشل) لا يُقيَّم.
    /// الترتيب: الخطورة ثم ترتيب الأكواد فى AiBriefAlertCodes.All.
    /// </summary>
    public static class AiBriefAlerts
    {
        public const int MaxItems = 5;

        public static List<AiBriefAlert> Evaluate(AiBriefFacts facts, AiBriefSettings settings, Func<string, bool> hasRoute)
        {
            var list = new List<AiBriefAlert>();
            if (facts == null) return list;
            settings ??= new AiBriefSettings();
            var period = new Dictionary<string, string> { ["from"] = Day(facts.PeriodFrom), ["to"] = Day(facts.PeriodTo) };

            var s = facts.Sales;
            if (s != null && hasRoute(AiSchemaCatalog.RSales))
            {
                // sales_drop: أقل من متوسط نفس يوم الأسبوع بنسبة ≥ العتبة.
                if (s.WeekdayBaseline is decimal baseline && baseline > 0
                    && s.NetSales < baseline * (100 - settings.SalesDropPct) / 100m)
                    list.Add(New(AiBriefAlertCodes.SalesDrop, AiBriefSeverity.High, AiSchemaCatalog.RSales, period,
                        ("net", R2(s.NetSales)), ("baseline", R2(baseline)), ("drop_pct", R1((baseline - s.NetSales) / baseline * 100)), ("threshold_pct", settings.SalesDropPct)));

                // no_sales: يوم عمل (BriefWeekDays) فى الفترة بلا أى فاتورة.
                var workDays = settings.WeekDays ?? new List<int>();
                var empty = (s.Days ?? new List<AiBriefDaySales>()).Where(d => d.Invoices == 0 && workDays.Contains((int)d.Date.DayOfWeek)).Select(d => d.Date).ToList();
                if (empty.Count > 0)
                {
                    var a = New(AiBriefAlertCodes.NoSales, AiBriefSeverity.High, AiSchemaCatalog.RSales, period, ("days", empty.Count));
                    a.Items.AddRange(empty.Select(Day));
                    list.Add(a);
                }

                // returns_high: المرتجعات ÷ المبيعات ≥ العتبة.
                if (s.NetSales > 0 && s.Returns / s.NetSales * 100 >= settings.ReturnsPct)
                    list.Add(New(AiBriefAlertCodes.ReturnsHigh, AiBriefSeverity.Medium, AiSchemaCatalog.RSales, period,
                        ("returns", R2(s.Returns)), ("net", R2(s.NetSales)), ("pct", R1(s.Returns / s.NetSales * 100)), ("threshold_pct", settings.ReturnsPct)));

                // below_cost: سطور بيع بأقل من متوسط التكلفة وقت البيع.
                if (s.BelowCostLines > 0)
                {
                    var a = New(AiBriefAlertCodes.BelowCost, AiBriefSeverity.Medium, AiSchemaCatalog.RSales, period,
                        ("lines", s.BelowCostLines), ("loss", R2(s.BelowCostLoss)));
                    a.Items.AddRange((s.BelowCostTop ?? new List<AiBriefNamedValue>()).Take(MaxItems).Select(x => x.Name));
                    list.Add(a);
                }
            }

            var liq = facts.Liquidity;
            string? liqRoute = hasRoute(AiSchemaCatalog.RCashTotals) ? AiSchemaCatalog.RCashTotals
                             : hasRoute(AiSchemaCatalog.RCashBoxStatement) ? AiSchemaCatalog.RCashBoxStatement : null;
            if (liq != null && liqRoute != null)
            {
                // cash_low: الرصيد الآن أقل من الحد، أو رصيد آخر الفترة سالب (BoxBalance يقصّ السالب إلى صفر فلا يكشفه وحده).
                var low = (liq.Boxes ?? new List<AiBriefBoxBalance>()).Where(b => b.Balance < settings.MinCashBalance || b.PeriodEnd < 0).ToList();
                if (low.Count > 0)
                {
                    var a = New(AiBriefAlertCodes.CashLow, AiBriefSeverity.High, liqRoute, period,
                        ("boxes", low.Count), ("min_balance", settings.MinCashBalance), ("negative", low.Count(b => b.PeriodEnd < 0)));
                    a.Items.AddRange(low.OrderBy(b => Math.Min(b.Balance, b.PeriodEnd)).Take(MaxItems).Select(b => b.BoxName));
                    list.Add(a);
                }
            }

            var rec = facts.Receivables;
            if (rec != null && hasRoute(AiSchemaCatalog.RCustomerBalances))
            {
                // overdue_receivables: آجل تجاوز الاستحقاق بأكثر من BriefOverdueDays وباقيه > 0.
                var over = (rec.Overdue ?? new List<AiBriefOverdueBucket>()).Where(b => b.DaysOverdue > settings.OverdueDays).ToList();
                int n = over.Sum(b => b.Count);
                if (n > 0)
                    list.Add(New(AiBriefAlertCodes.OverdueReceivables, AiBriefSeverity.Medium, AiSchemaCatalog.RCustomerBalances,
                        new Dictionary<string, string>(),
                        ("count", n), ("amount", R2(over.Sum(b => b.Amount))), ("max_days", over.Max(b => b.DaysOverdue)), ("threshold_days", settings.OverdueDays)));
            }

            var st = facts.Stock;
            if (st != null && st.NegativeCount > 0 && hasRoute(AiSchemaCatalog.RStock))
            {
                var a = New(AiBriefAlertCodes.NegativeStock, AiBriefSeverity.Info, AiSchemaCatalog.RStock, new Dictionary<string, string>(), ("count", st.NegativeCount));
                a.Items.AddRange((st.NegativeTop ?? new List<AiBriefNegativeItem>()).Take(MaxItems)
                    .Select(x => string.IsNullOrWhiteSpace(x.StoreName) ? x.ItemName : x.ItemName + " — " + x.StoreName));
                list.Add(a);
            }

            var col = facts.Collections;
            string? colRoute = hasRoute(AiSchemaCatalog.RCashVouchers) ? AiSchemaCatalog.RCashVouchers
                             : hasRoute(AiSchemaCatalog.RCashTotals) ? AiSchemaCatalog.RCashTotals : null;
            if (col != null && colRoute != null)
            {
                // large_payment: متوسط مدفوعات الفترة اليومى > المعامل × متوسط آخر 30 يوماً (لكل عملة، فلا خلط بين العملات).
                int days = facts.PeriodDays;
                foreach (var c in col.ByCurrency ?? new List<AiBriefCurrencyCash>())
                {
                    decimal perDay = c.Payments / days;
                    if (c.PaymentsAvgPerDay30 > 0 && perDay > settings.LargePaymentFactor * c.PaymentsAvgPerDay30)
                    {
                        var a = New(AiBriefAlertCodes.LargePayment, AiBriefSeverity.Medium, colRoute, period,
                            ("payments", R2(c.Payments)), ("per_day", R2(perDay)), ("avg_30", R2(c.PaymentsAvgPerDay30)),
                            ("factor", settings.LargePaymentFactor), ("currency_id", c.CurrencyId));
                        a.Items.Add(c.CurrencyName);
                        list.Add(a);
                    }
                }
            }

            // P8.4 (ش27): شذوذ قيود جديد غير مراجَع منذ بداية الفترة (المسح التاريخى لا يدخل).
            var an = facts.Anomalies;
            if (an != null && an.NewCount > 0 && hasRoute(AiSchemaCatalog.REntryAnomalies))
            {
                var a = New(AiBriefAlertCodes.AnomaliesNew, an.HighCount > 0 ? AiBriefSeverity.High : AiBriefSeverity.Medium, AiSchemaCatalog.REntryAnomalies,
                    new Dictionary<string, string>(), ("count", an.NewCount), ("high", an.HighCount), ("open_total", an.OpenTotal));
                a.Items.AddRange((an.Top ?? new List<string>()).Take(MaxItems));
                list.Add(a);
            }

            return list.OrderBy(a => AiBriefSeverity.Rank(a.Severity)).ThenBy(a => Array.IndexOf(AiBriefAlertCodes.All, a.Code)).ToList();
        }

        private static AiBriefAlert New(string code, string severity, string route, Dictionary<string, string> prms, params (string Key, decimal Value)[] values)
        {
            var a = new AiBriefAlert { Code = code, Severity = severity, Route = route, Params = new Dictionary<string, string>(prms) };
            foreach (var (k, v) in values) a.Values[k] = v;
            return a;
        }

        private static string Day(DateTime d) => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        private static decimal R2(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);
        private static decimal R1(decimal v) => Math.Round(v, 1, MidpointRounding.AwayFromZero);
    }
}
