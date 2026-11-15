using System.Data;
using System.Diagnostics;
using System.Globalization;
using AccountERP.AI.Tools;
using AccountModelsLib;

namespace AccountERP.AI.Cashflow
{
    /// <summary>
    /// P8.7.2 (ai-assistant-p87-plan.md §1): يجمع المصادر الخام للتوقع بصلاحيات المستخدم: الخزائن (محرك شاشات الخزينة BoxBalance × عامل العملة)،
    /// وقراءة ai.* عبر ReadOnlySqlExecutor (ai_tax + ai_box_perm). كل استعلام يُسجَّل فى التدقيق بأداة cashflow_query.
    /// </summary>
    public sealed class AiCashflowReader
    {
        public const string AuditTool = "cashflow_query";
        public const int InvoiceMonths = 24;
        private static readonly HashSet<string> Views = new(new[] { "v_sales", "v_customer_ledger", "v_purchases", "v_cash_vouchers", "v_cheques" }, StringComparer.OrdinalIgnoreCase);

        private readonly ReadOnlySqlExecutor _sql;
        private readonly AiToolAuditCLS _audit = new();

        /// <summary>اختبار: بديل المصادر كاملة.</summary>
        public Func<AiServerUser, IList<long>?, AiCashflowSettings, DateTime, AiCashflowSources>? Override { get; set; }

        public AiCashflowReader(ReadOnlySqlExecutor sql) => _sql = sql;

        /// <param name="branches">null = كل الفروع (المدير).</param>
        public AiCashflowSources Read(AiServerUser server, IList<long>? branches, AiCashflowSettings s, DateTime today, HashSet<long> exclusions,
                                      List<AiCashflowAdjustmentRow> adjustments, out List<AiCashflowRecurringRow> recurring, CancellationToken ct)
        {
            recurring = new List<AiCashflowRecurringRow>();
            if (Override != null) return Override(server, branches, s, today);
            var src = new AiCashflowSources();
            var session = ReadOnlySqlExecutor.SessionFor(server.TaxFilter, server.UserId);
            string bf(string col) => branches == null ? "1 = 1" : branches.Count == 0 ? "1 = 0"
                : col + " IN (" + string.Join(",", branches.Select(b => b.ToString(CultureInfo.InvariantCulture))) + ")";
            var start = AiCashflowEngine.WeekStart(today);
            var end = start.AddDays(7 * AiCashflowConst.Weeks);
            var invFrom = today.AddMonths(-InvoiceMonths);

            // (1) الافتتاحى: الخزائن المسموحة بنفس محرك الشاشات
            var svc = new CashVouchersService();
            var boxes = server.Run(() => svc.GetBoxes(server.UserId));
            decimal opening = 0;
            foreach (DataRow r in boxes.Rows)
            {
                ct.ThrowIfCancellationRequested();
                long id = Convert.ToInt64(r["Id"], CultureInfo.InvariantCulture);
                long br = r["Branch_Id"] == DBNull.Value ? 0 : Convert.ToInt64(r["Branch_Id"], CultureInfo.InvariantCulture);
                if (branches != null && br > 0 && !branches.Contains(br)) continue;
                long cur = r.Table.Columns.Contains("currency_Id") && r["currency_Id"] != DBNull.Value ? Convert.ToInt64(r["currency_Id"], CultureInfo.InvariantCulture) : 1;
                decimal bal = server.Run(() => svc.BoxBalance(id));
                decimal factor = server.Run(() => svc.GetFactor(cur));
                if (factor <= 0) factor = 1;
                if (bal < 0) src.NegativeBoxes = true;
                opening += bal * factor;
            }
            src.Opening = Math.Round(opening, 2);

            // (2) العملاء: الرصيد الموجب ⇒ أحدث الفواتير الآجلة FIFO (ت P8.5)، الاستحقاق = due_date أو التاريخ + المدة
            var bal2 = Q(server, session, "receivables_balance", $@"SELECT TOP (5000) customer_id, SUM(debit - credit) AS bal
FROM ai.v_customer_ledger GROUP BY customer_id HAVING SUM(debit - credit) > 1", null, ct);
            var balances = bal2.Rows.ToDictionary(r => L(r[0]), r => D(r[1]));
            if (balances.Count > 0)
            {
                var inv = Q(server, session, "receivables_invoices", $@"SELECT TOP (5000) sale_id, customer_id, invoice_date, due_date, net_total, invoice_no
FROM ai.v_sales WHERE is_credit = 1 AND invoice_date >= @from AND {bf("branch_id")}
ORDER BY invoice_date DESC", new Dictionary<string, object> { ["from"] = invFrom }, ct);
                var byCust = inv.Rows.GroupBy(r => L(r[1])).ToDictionary(g => g.Key, g => g.ToList());
                foreach (var kv in balances)
                {
                    if (!byCust.TryGetValue(kv.Key, out var rows))
                    {
                        // عميل بلا فواتير آجلة داخل النطاق: بلا فرع (المدير فقط) يُعد متأخراً
                        if (branches == null)
                            src.Receivables.Add(new AiCashflowItem { Date = DateTime.MinValue, Amount = kv.Value, RefId = kv.Key, Estimated = true,
                                Label = "customer#" + kv.Key.ToString(CultureInfo.InvariantCulture), Url = "/accounts/customer-account-statement/normal?customer=" + kv.Key.ToString(CultureInfo.InvariantCulture) });
                        continue;
                    }
                    var list = rows.Select(r => Tuple.Create(L(r[0]), Dt(r[2]), r[3] == null || r[3] is DBNull ? Dt(r[2]).AddDays(s.CustomerTermsDays) : Dt(r[3]), D(r[4]))).ToList();
                    var estimated = rows.Where(r => r[3] == null || r[3] is DBNull).Select(r => L(r[0])).ToHashSet();
                    var nos = rows.ToDictionary(r => L(r[0]), r => Convert.ToString(r[5], CultureInfo.InvariantCulture));
                    foreach (var o in AiCashflowEngine.OpenByFifo(list, kv.Value))
                    {
                        bool noInv = o.Item1 == 0;
                        if (noInv && branches != null) continue;
                        src.Receivables.Add(new AiCashflowItem
                        {
                            Date = o.Item2, Amount = o.Item3, RefId = noInv ? kv.Key : o.Item1, Estimated = noInv || estimated.Contains(o.Item1),
                            Label = noInv ? "customer#" + kv.Key.ToString(CultureInfo.InvariantCulture) : "invoice " + nos[o.Item1],
                            Url = noInv ? "/accounts/customer-account-statement/normal?customer=" + kv.Key.ToString(CultureInfo.InvariantCulture)
                                        : "/accounts/invoices/sales-invoice/" + o.Item1.ToString(CultureInfo.InvariantCulture),
                        });
                    }
                }
            }

            // (3) الموردون: المتبقى على فواتير الشراء الآجلة (RestValue كما فى الشاشة)، الاستحقاق = التاريخ + مدة الموردين
            var pay = Q(server, session, "payables", $@"SELECT TOP (5000) purchase_id, invoice_date, rest_value, invoice_no
FROM ai.v_purchases WHERE is_credit = 1 AND rest_value > 0 AND invoice_date >= @from AND {bf("branch_id")}
ORDER BY invoice_date", new Dictionary<string, object> { ["from"] = invFrom }, ct);
            foreach (var r in pay.Rows)
                src.Payables.Add(new AiCashflowItem
                {
                    Date = Dt(r[1]).AddDays(s.SupplierTermsDays), Amount = D(r[2]), RefId = L(r[0]), Estimated = true,
                    Label = "purchase " + Convert.ToString(r[3], CultureInfo.InvariantCulture), Url = "/accounts/invoices/purchase-invoice/" + L(r[0]).ToString(CultureInfo.InvariantCulture),
                });

            // (4) الشيكات غير المحصّلة/غير المصروفة المستحقة قبل نهاية الأفق (المتأخرة حتى سنة)
            var chq = Q(server, session, "cheques", $@"SELECT TOP (5000) cheque_id, cheque_no, due_date, amount_equivalent, transaction_type, customer_id, supplier_id
FROM ai.v_cheques WHERE receipt = 0 AND spent_date IS NULL AND due_date IS NOT NULL AND due_date >= @old AND due_date < @end AND {bf("branch_id")}",
                new Dictionary<string, object> { ["old"] = today.AddYears(-1), ["end"] = end }, ct);
            foreach (var r in chq.Rows)
            {
                // فحص 1 (مفتوح): Transaction_Type 1 = شيك وارد (استلام) كترقيم سندات الخزينة؛ غيره صادر.
                bool incoming = I(r[4]) == 1;
                var it = new AiCashflowItem { Date = Dt(r[2]), Amount = D(r[3]), RefId = L(r[0]), Label = "cheque " + Convert.ToString(r[1], CultureInfo.InvariantCulture) };
                (incoming ? src.ChequesIn : src.ChequesOut).Add(it);
            }

            // (5) المتكرر: مدفوعات حسابات عامة لآخر 4 أشهر كاملة
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var rec = Q(server, session, "recurring", $@"SELECT TOP (5000) party_id, MAX(party_name), voucher_date, SUM(amount_equivalent)
FROM ai.v_cash_vouchers WHERE party_type = N'account' AND is_receipt = 0 AND voucher_date >= @from AND voucher_date < @to AND {bf("branch_id")}
GROUP BY party_id, voucher_date", new Dictionary<string, object> { ["from"] = monthStart.AddMonths(-4), ["to"] = monthStart }, ct);
            recurring = AiCashflowEngine.DetectRecurring(rec.Rows.Select(r => Tuple.Create(L(r[0]), Convert.ToString(r[1], CultureInfo.InvariantCulture) ?? "", Dt(r[2]), D(r[3]))),
                                                         today, s.RecurringMinMonths);
            foreach (var x in recurring) x.Excluded = exclusions.Contains(x.SubAccountId);
            src.Recurring = AiCashflowEngine.RecurringItems(recurring, today);

            // (6) المبيعات النقدية: متوسط آخر 8 أسابيع
            if (s.IncludeCashSales)
            {
                var cs = Q(server, session, "cash_sales", $@"SELECT SUM(net_total) FROM ai.v_sales WHERE is_credit = 0 AND invoice_date >= @from AND invoice_date < @to AND {bf("branch_id")}",
                    new Dictionary<string, object> { ["from"] = today.AddDays(-56), ["to"] = today }, ct);
                src.CashSalesWeekly = cs.Rows.Count == 0 ? 0 : Math.Round(D(cs.Rows[0][0]) / 8m, 2);
            }

            // (7) التعديلات اليدوية
            foreach (var a in adjustments)
                src.Manual.Add(new AiCashflowItem
                {
                    Source = a.Direction == "out" ? AiCashflowConst.SrcManualOut : AiCashflowConst.SrcManualIn,
                    Date = a.WeekStart, Amount = a.Amount, RefId = a.Id, Label = a.Description,
                });
            return src;
        }

        /// <summary>و11: الفعلى من الخزائن لأسبوع (مقبوضات، مدفوعات) بالعملة المكافئة.</summary>
        public (decimal In, decimal Out) Actual(AiServerUser server, IList<long>? branches, DateTime from, DateTime to, CancellationToken ct)
        {
            string bf = branches == null ? "1 = 1" : branches.Count == 0 ? "1 = 0"
                : "branch_id IN (" + string.Join(",", branches.Select(b => b.ToString(CultureInfo.InvariantCulture))) + ")";
            var r = Q(server, ReadOnlySqlExecutor.SessionFor(server.TaxFilter, server.UserId), "actual_week",
                $@"SELECT SUM(CASE WHEN is_receipt = 1 THEN amount_equivalent ELSE 0 END), SUM(CASE WHEN is_receipt = 0 THEN amount_equivalent ELSE 0 END)
FROM ai.v_cash_vouchers WHERE voucher_date >= @from AND voucher_date < @to AND {bf}",
                new Dictionary<string, object> { ["from"] = from, ["to"] = to }, ct);
            return r.Rows.Count == 0 ? (0, 0) : (D(r.Rows[0][0]), D(r.Rows[0][1]));
        }

        private AiQueryResult Q(AiServerUser server, Dictionary<string, object> session, string name, string sql, Dictionary<string, object>? p, CancellationToken ct)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                var r = _sql.Execute(sql, Views, session, p, ct);
                try { _audit.Insert(null, server.UserId, AuditTool, "{\"q\":\"" + name + "\"}", sql, r.Rows.Count, (int)sw.ElapsedMilliseconds, AiToolOutcome.Ok, null); } catch { }
                return r;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                try { _audit.Insert(null, server.UserId, AuditTool, "{\"q\":\"" + name + "\"}", sql, 0, (int)sw.ElapsedMilliseconds, AiToolOutcome.Error, ex.Message); } catch { }
                throw;
            }
        }

        private static long L(object? o) => o == null || o is DBNull ? 0 : Convert.ToInt64(o, CultureInfo.InvariantCulture);
        private static int I(object? o) => o == null || o is DBNull ? 0 : Convert.ToInt32(o, CultureInfo.InvariantCulture);
        private static decimal D(object? o) => o == null || o is DBNull ? 0 : Convert.ToDecimal(o, CultureInfo.InvariantCulture);
        private static DateTime Dt(object? o) => o == null || o is DBNull ? DateTime.MinValue : Convert.ToDateTime(o, CultureInfo.InvariantCulture);
    }
}
