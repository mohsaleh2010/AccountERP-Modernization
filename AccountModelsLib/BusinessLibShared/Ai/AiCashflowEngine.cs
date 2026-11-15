using System;
using System.Collections.Generic;
using System.Linq;

namespace AccountModelsLib
{
    /// <summary>
    /// P8.7.1 (ai-assistant-p87-plan.md و1–و9): محرك التوزيع على 13 أسبوعاً (pure، بلا قاعدة بيانات). الأسبوع يبدأ السبت.
    /// الأساسى: المقبوضات بعد تأخير BaseDelayDays. المتحفظ: تأخير ConservativeDelayDays ونسبة تحصيل ConservativeCollectPct للعملاء والمبيعات النقدية؛
    /// المدفوعات فى موعدها فى السيناريوهين. المتأخر (قبل بداية الأسبوع الأول) ⇒ الأسبوع الأول بعلامة متأخر. بعد الأفق ⇒ يُسقط.
    /// </summary>
    public static class AiCashflowEngine
    {
        public static DateTime WeekStart(DateTime d)
        {
            d = d.Date;
            int offset = ((int)d.DayOfWeek + 1) % 7;   // السبت = 0
            return d.AddDays(-offset);
        }

        public static AiCashflowFacts Build(AiCashflowSources src, string scenario, AiCashflowSettings s, DateTime asOf)
        {
            src = src ?? new AiCashflowSources();
            s = s ?? new AiCashflowSettings();
            bool cons = scenario == AiCashflowConst.ScenarioConservative;
            var start = WeekStart(asOf);
            var f = new AiCashflowFacts
            {
                AsOf = asOf.Date, Scenario = cons ? AiCashflowConst.ScenarioConservative : AiCashflowConst.ScenarioBase,
                Opening = Math.Round(src.Opening, 2), MinBalance = s.MinBalance, NegativeBoxes = src.NegativeBoxes,
            };
            for (int i = 0; i < AiCashflowConst.Weeks; i++) f.Weeks.Add(new AiCashflowWeek { Index = i, Start = start.AddDays(7 * i) });
            var end = start.AddDays(7 * AiCashflowConst.Weeks);
            decimal pct = cons ? s.ConservativeCollectPct / 100m : 1m;

            void Place(AiCashflowItem it, string source, DateTime date, decimal amount)
            {
                if (amount == 0) return;
                bool overdue = date < start;
                if (overdue) date = start;
                if (date >= end) return;
                int w = (int)((date - start).TotalDays / 7);
                var wk = f.Weeks[w];
                amount = Math.Round(amount, 2);
                switch (source)
                {
                    case AiCashflowConst.SrcCustomer: wk.Customers += amount; break;
                    case AiCashflowConst.SrcChequeIn: wk.ChequesIn += amount; break;
                    case AiCashflowConst.SrcCashSales: wk.CashSales += amount; break;
                    case AiCashflowConst.SrcManualIn: wk.ManualIn += amount; break;
                    case AiCashflowConst.SrcSupplier: wk.Suppliers += amount; break;
                    case AiCashflowConst.SrcChequeOut: wk.ChequesOut += amount; break;
                    case AiCashflowConst.SrcRecurring: wk.Recurring += amount; break;
                    case AiCashflowConst.SrcManualOut: wk.ManualOut += amount; break;
                    default: return;
                }
                if (overdue) wk.OverdueItems++;
                f.Items.Add(new AiCashflowItem
                {
                    Source = source, Date = date, Amount = amount, Label = it?.Label, RefId = it?.RefId ?? 0, Url = it?.Url, Estimated = it?.Estimated ?? false,
                });
            }

            int delay = cons ? s.ConservativeDelayDays : s.BaseDelayDays;
            int est = 0, all = 0;
            foreach (var it in src.Receivables)
            {
                all++; if (it.Estimated) est++;
                Place(it, AiCashflowConst.SrcCustomer, it.Date.Date.AddDays(delay), it.Amount * pct);
            }
            foreach (var it in src.Payables)
            {
                all++; if (it.Estimated) est++;
                Place(it, AiCashflowConst.SrcSupplier, it.Date.Date, it.Amount);
            }
            foreach (var it in src.ChequesIn) Place(it, AiCashflowConst.SrcChequeIn, it.Date.Date.AddDays(cons ? 3 : 0), it.Amount);
            foreach (var it in src.ChequesOut) Place(it, AiCashflowConst.SrcChequeOut, it.Date.Date, it.Amount);
            foreach (var it in src.Recurring) Place(it, AiCashflowConst.SrcRecurring, it.Date.Date, it.Amount);
            foreach (var it in src.Manual)
                Place(it, it.Source == AiCashflowConst.SrcManualOut ? AiCashflowConst.SrcManualOut : AiCashflowConst.SrcManualIn, it.Date.Date, it.Amount);
            if (s.IncludeCashSales && src.CashSalesWeekly > 0)
            {
                // الأسبوع الأول جزئى: من اليوم حتى نهايته فقط
                for (int i = 0; i < AiCashflowConst.Weeks; i++)
                {
                    decimal amt = src.CashSalesWeekly * pct;
                    if (i == 0) amt = amt * (decimal)Math.Max(0, (start.AddDays(7) - asOf.Date).TotalDays) / 7m;
                    Place(new AiCashflowItem { Label = "cash_sales" }, AiCashflowConst.SrcCashSales, f.Weeks[i].Start.AddDays(i == 0 ? Math.Max(0, (asOf.Date - start).Days) : 0), amt);
                }
            }

            decimal bal = f.Opening;
            f.LowestClosing = decimal.MaxValue;
            foreach (var wk in f.Weeks)
            {
                wk.Opening = bal;
                bal = Math.Round(bal + wk.Net, 2);
                wk.Closing = bal;
                if (wk.Closing < f.LowestClosing) { f.LowestClosing = wk.Closing; f.LowestWeek = wk.Index; }
                if (f.LowWeek < 0 && s.MinBalance > 0 && wk.Closing < s.MinBalance) f.LowWeek = wk.Index;
            }
            if (f.LowestClosing == decimal.MaxValue) f.LowestClosing = f.Opening;
            f.EstimatedShare = all == 0 ? 0 : Math.Round((double)est / all, 3);
            if (f.EstimatedShare > 0.5) f.Warnings.Add("estimated_due");
            if (f.NegativeBoxes) f.Warnings.Add("negative_boxes");
            return f;
        }

        /// <summary>
        /// و6: مصروف متكرر من مدفوعات حساب عام لآخر 4 أشهر كاملة: ظهر فى ≥ minMonths شهراً وتباين المبلغ الشهرى ≤ 25%.
        /// الإدخال: (حساب، اسم، تاريخ، مبلغ). الإخراج: بند لكل شهر داخل الأفق فى يوم الوسيط بمبلغ الوسيط.
        /// </summary>
        public static List<AiCashflowRecurringRow> DetectRecurring(IEnumerable<Tuple<long, string, DateTime, decimal>> payments, DateTime asOf, int minMonths)
        {
            var monthStart = new DateTime(asOf.Year, asOf.Month, 1);
            var from = monthStart.AddMonths(-4);
            var rows = new List<AiCashflowRecurringRow>();
            foreach (var g in (payments ?? Enumerable.Empty<Tuple<long, string, DateTime, decimal>>())
                         .Where(p => p.Item3 >= from && p.Item3 < monthStart && p.Item4 > 0).GroupBy(p => p.Item1))
            {
                var months = g.GroupBy(p => new DateTime(p.Item3.Year, p.Item3.Month, 1))
                              .Select(m => new { Total = m.Sum(x => x.Item4), Day = Median(m.Select(x => (decimal)x.Item3.Day).ToList()) }).ToList();
                if (months.Count < minMonths) continue;
                var totals = months.Select(m => m.Total).ToList();
                decimal mean = totals.Average();
                if (mean <= 0) continue;
                double sd = Math.Sqrt(totals.Select(t => Math.Pow((double)(t - mean), 2)).Average());
                if (sd / (double)mean > 0.25) continue;
                rows.Add(new AiCashflowRecurringRow
                {
                    SubAccountId = g.Key, Name = g.Select(x => x.Item2).FirstOrDefault(n => !string.IsNullOrWhiteSpace(n)) ?? "",
                    Amount = Math.Round(Median(totals), 2), Day = (int)Math.Round(Median(months.Select(m => m.Day).ToList())),
                });
            }
            return rows.OrderByDescending(r => r.Amount).ToList();
        }

        /// <summary>يحوّل المتكرر إلى بنود شهرية داخل الأفق (الشهر الحالى إن لم يمر يومه بعد).</summary>
        public static List<AiCashflowItem> RecurringItems(IEnumerable<AiCashflowRecurringRow> rows, DateTime asOf)
        {
            var list = new List<AiCashflowItem>();
            var start = WeekStart(asOf);
            var end = start.AddDays(7 * AiCashflowConst.Weeks);
            foreach (var r in rows.Where(x => !x.Excluded))
                for (int m = 0; m <= 4; m++)
                {
                    var month = new DateTime(asOf.Year, asOf.Month, 1).AddMonths(m);
                    int day = Math.Max(1, Math.Min(r.Day, DateTime.DaysInMonth(month.Year, month.Month)));
                    var d = new DateTime(month.Year, month.Month, day);
                    if (d < asOf.Date || d >= end) continue;
                    list.Add(new AiCashflowItem { Date = d, Amount = r.Amount, Label = r.Name, RefId = r.SubAccountId, Estimated = true });
                }
            return list;
        }

        /// <summary>
        /// FIFO: الرصيد المفتوح يغطى أحدث الفواتير أولاً (المدفوع يسدد الأقدم). invoices: (id, date, due, amount). يعيد (id, due, open).
        /// </summary>
        public static List<Tuple<long, DateTime, decimal>> OpenByFifo(IEnumerable<Tuple<long, DateTime, DateTime, decimal>> invoices, decimal balance)
        {
            var result = new List<Tuple<long, DateTime, decimal>>();
            if (balance <= 0) return result;
            decimal left = balance;
            foreach (var inv in (invoices ?? Enumerable.Empty<Tuple<long, DateTime, DateTime, decimal>>()).OrderByDescending(x => x.Item2).ThenByDescending(x => x.Item1))
            {
                if (left <= 0) break;
                if (inv.Item4 <= 0) continue;
                decimal open = Math.Min(left, inv.Item4);
                result.Add(Tuple.Create(inv.Item1, inv.Item3, open));
                left -= open;
            }
            // رصيد أكبر من الفواتير المتاحة (أرصدة قديمة) ⇒ بند بلا فاتورة مستحق اليوم (يُعامل كمتأخر)
            if (left > 0.005m) result.Add(Tuple.Create(0L, DateTime.MinValue, left));
            return result;
        }

        public static decimal Median(List<decimal> v)
        {
            if (v == null || v.Count == 0) return 0;
            var s = v.OrderBy(x => x).ToList();
            int n = s.Count;
            return n % 2 == 1 ? s[n / 2] : (s[n / 2 - 1] + s[n / 2]) / 2m;
        }
    }
}
