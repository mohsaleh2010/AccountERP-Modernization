using System.Collections.Concurrent;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AccountERP.AI.Tools;
using AccountModelsLib;

namespace AccountERP.AI.Collections
{
    /// <summary>نطاق حساب القائمة لمستخدم: فروعه (null = بلا قيد) وصلاحيته الضريبية.</summary>
    public sealed class AiCollectScope
    {
        public long UserId { get; set; }
        public bool IsAdmin { get; set; }
        /// <summary>null = كل الفروع (المدير). تقيّد الفواتير فقط؛ الدفتر بلا قيد فرع (فحص 3).</summary>
        public List<long>? BranchIds { get; set; }
        public int TaxFilter { get; set; } = -1;

        public string Key(AiCollectSettings s, DateTime today)
        {
            var c = CultureInfo.InvariantCulture;
            bool boxPerm = AccountModelsLib.Common.IS_Used_SandoukWithPermission == 1;
            string raw = string.Join("|", BranchIds == null ? "*" : string.Join(",", BranchIds.Distinct().OrderBy(x => x).Select(x => x.ToString(c))),
                TaxFilter.ToString(c), boxPerm ? "u" + UserId.ToString(c) : "-", s.Fingerprint(), today.ToString("yyyy-MM-dd", c));
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));
        }
    }

    public sealed class AiCollectResult
    {
        public List<AiCollectRow> Rows { get; set; } = new();
        public bool Truncated { get; set; }
        public bool TaxRestricted { get; set; }
        public DateTime ComputedAt { get; set; }
        public int DurationMs { get; set; }
        public bool FromCache { get; set; }
    }

    /// <summary>
    /// P8.5.2 — حاسب أولويات التحصيل (Singleton؛ ai-assistant-p85-plan.md §1، ت3–ت16، ت19). يقرأ ai.v_customer_ledger/v_customers/v_customer_credit/
    /// v_sales/v_cash_vouchers عبر ReadOnlySqlExecutor بجلسة المستخدم الطالب (صلاحية الفواتير الضريبية والصناديق)، ثم الدرجة والحالة بـ AiCollectRules.
    /// حىّ عند الطلب مع ذاكرة قصيرة لكل نطاق (ت5). لا يكتب شيئاً إلا التدقيق collect_query.
    /// </summary>
    public sealed class AiCollectionsScorer
    {
        private static readonly HashSet<string> Allowed = new(new[] { "v_customer_ledger", "v_customers", "v_customer_credit", "v_sales", "v_cash_vouchers" }, StringComparer.OrdinalIgnoreCase);
        private static readonly JsonSerializerOptions Json = new() { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

        private readonly ReadOnlySqlExecutor _sql;
        private readonly AiCollectCLS _dal;
        private readonly AiToolAuditCLS _audit;
        private readonly ConcurrentDictionary<string, (DateTime At, AiCollectResult Result)> _cache = new();

        public Func<string, IReadOnlyDictionary<string, object>, AiQueryResult>? QuerySource { get; set; }
        public Func<DateTime> Today { get; set; } = () => DateTime.Today;
        public Func<IList<long>, Dictionary<long, List<AiCollectActionRow>>>? StateSource { get; set; }
        public List<string> LastSql { get; } = new();
        public bool AuditEnabled { get; set; } = true;
        /// <summary>عدد الاستعلامات المنفّذة (للاختبار: الذاكرة).</summary>
        public int QueryCount;

        public AiCollectionsScorer(ReadOnlySqlExecutor sql, AiCollectCLS? dal = null, AiToolAuditCLS? audit = null)
        {
            _sql = sql;
            _dal = dal ?? new AiCollectCLS();
            _audit = audit ?? new AiToolAuditCLS();
        }

        public AiCollectCLS Dal => _dal;

        // ───────────── الحساب ─────────────

        public AiCollectResult Compute(AiCollectScope scope, AiCollectSettings s, bool refresh, CancellationToken ct)
        {
            var today = Today().Date;
            string key = scope.Key(s, today);
            if (!refresh && s.CacheMinutes > 0 && _cache.TryGetValue(key, out var hit) && hit.At > DateTime.Now.AddMinutes(-s.CacheMinutes))
                return Clone(hit.Result, fromCache: true);

            var sw = System.Diagnostics.Stopwatch.StartNew();
            string sql = BuildSql(scope, s, today, out var p);
            var r = Q("main", sql, p, scope);
            // مبيعات نافذة DSO وآخر تحصيل باستعلامين تجميعيين منفصلين: ربطهما داخل الاستعلام الرئيسى يجعل المحسِّن يدفع الربط داخل
            // View الإذون (700 ms مقابل 25 ms). القيمة من الاستعلام الجانبى تغلب الموضع الاحتياطى فى الصف الرئيسى.
            var ids = r.Rows.Take(s.MaxCustomers).Select(x => L(x[0])).ToHashSet();
            var creditSales = SideMap(SideSql(scope, s, today, out var p2), p2, scope, ids);
            var receipts = SideMap(ReceiptSql(), new Dictionary<string, object>(), scope, ids);
            var pairs = new List<(AiCollectRow Row, AiCollectFacts F)>();
            bool truncated = r.Truncated || r.Rows.Count > s.MaxCustomers;
            foreach (var x in r.Rows.Take(s.MaxCustomers))
            {
                var f = new AiCollectFacts
                {
                    Balance = M(x[5]), Allocated = M(x[6]), OpenInvoices = I(x[7]), NotDue = M(x[8]), D1_30 = M(x[9]), D31_60 = M(x[10]), D61_90 = M(x[11]),
                    DOver90 = M(x[12]), OldestDays = I(x[13]), CreditSales = creditSales.TryGetValue(L(x[0]), out var csv) ? M(csv) : M(x[14]),
                    LastReceiptDate = receipts.TryGetValue(L(x[0]), out var lrv) ? D(lrv) : D(x[15]),
                    CreditLimit = x[16] == null ? null : M(x[16]),
                };
                AiCollectRules.Derive(f, s, today);
                pairs.Add((new AiCollectRow
                {
                    CustomerId = L(x[0]), Code = Str(x[1]), Name = Str(x[2]), GroupId = NL(x[3]), CurrencyId = NL(x[4]),
                    Balance = f.Balance, Overdue = f.Overdue, NotDue = f.NotDue, D1_30 = f.D1_30, D31_60 = f.D31_60, D61_90 = f.D61_90, DOver90 = f.DOver90,
                    Unallocated = f.Unallocated, OldestDays = f.OldestDays, OpenInvoices = f.OpenInvoices, CreditSales = f.CreditSales, Dso = f.Dso,
                    LastReceiptDate = f.LastReceiptDate, IdleDays = f.IdleDays, CreditLimit = f.CreditLimit, LimitSource = Str(x[17]),
                }, f));
            }
            // الدرجة بعد دمج الحالات (الوعد غير الموفى يضيف نقاطاً).
            ApplyStates(pairs, s, today);
            var rows = pairs.Select(x => x.Row).OrderByDescending(x => x.Score).ThenByDescending(x => x.Overdue).ThenBy(x => x.CustomerId).ToList();
            var result = new AiCollectResult
            {
                Rows = rows, Truncated = truncated, TaxRestricted = scope.TaxFilter != -1, ComputedAt = DateTime.Now, DurationMs = (int)sw.ElapsedMilliseconds,
            };
            if (s.CacheMinutes > 0)
            {
                _cache[key] = (DateTime.Now, result);
                if (_cache.Count > AiCollectConst.CacheMaxEntries)
                    foreach (var old in _cache.OrderBy(kv => kv.Value.At).Take(_cache.Count - AiCollectConst.CacheMaxEntries).ToList())
                        _cache.TryRemove(old.Key, out _);
            }
            return Clone(result, fromCache: false);
        }

        private void ApplyStates(List<(AiCollectRow Row, AiCollectFacts F)> pairs, AiCollectSettings s, DateTime today)
        {
            var ids = pairs.Select(r => r.Row.CustomerId).ToList();
            var states = ids.Count == 0 ? new Dictionary<long, List<AiCollectActionRow>>() : (StateSource?.Invoke(ids) ?? _dal.OpenStates(ids));
            foreach (var (r, f) in pairs)
            {
                states.TryGetValue(r.CustomerId, out var acts);
                acts ??= new List<AiCollectActionRow>();
                r.State = AiCollectRules.State(acts, today, r.LastReceiptDate, s, out bool broken);
                var promise = acts.Where(a => a.ActionType == AiCollectActionTypes.Promise && a.Status == AiCollectActionStatus.Open)
                                  .OrderByDescending(a => a.CreatedAt ?? DateTime.MinValue).FirstOrDefault();
                r.PromiseDate = promise?.PromiseDate;
                r.PromiseAmount = promise?.PromiseAmount;
                r.SnoozeUntil = acts.Where(a => a.ActionType == AiCollectActionTypes.Snooze && a.Status == AiCollectActionStatus.Open).Select(a => a.NextActionDate).Max();
                r.LastReminderAt = acts.Where(a => a.ActionType == AiCollectActionTypes.ReminderSent).Select(a => a.CreatedAt).Max();
                var last = acts.Where(a => a.Status != AiCollectActionStatus.Cancelled).OrderByDescending(a => a.CreatedAt ?? DateTime.MinValue).FirstOrDefault();
                r.LastActionAt = last?.CreatedAt;
                r.LastActionType = last?.ActionType;
                var sc = AiCollectRules.Score(f, s, today, broken);
                r.Score = sc.Score; r.Priority = sc.Priority; r.Level = sc.Level; r.Parts = sc.Parts; r.BrokenBonus = sc.BrokenBonus;
            }
        }

        /// <summary>بعد أى إجراء يُفرَّغ كل الذاكرة (الحالة تخص العميل لا النطاق، والعميل يظهر فى نطاقات عدة).</summary>
        public void Invalidate() => _cache.Clear();

        /// <summary>
        /// الاستعلام الرئيسى (§1). الدفتر بلا قيد فرع (فحص 3)، والفواتير بفروع النطاق. كل قيمة معامل.
        /// بلا «IN (SELECT … FROM bal)» داخل CTE الفواتير والتحصيل: كانت تعيد تجميع View الدفتر لكل صف (1.2 ث مقابل ~60 ms)؛ الربط بـ bal لاحقاً يكفى.
        /// </summary>
        public static string BuildSql(AiCollectScope scope, AiCollectSettings s, DateTime today, out Dictionary<string, object> p)
        {
            p = new Dictionary<string, object>
            {
                ["min_balance"] = s.MinBalance, ["terms"] = s.TermsDays, ["inv_from"] = today.AddMonths(-s.InvoiceMonths), ["today"] = today,
                ["top"] = s.MaxCustomers + 1, ["ctype"] = AccountModelsLib.Common.Customer_Type,
            };
            string bS = BranchFilter("s.branch_id", "s", scope.BranchIds, p);
            return $@"WITH bal AS (
SELECT g.customer_id, SUM(g.debit - g.credit) AS balance
FROM ai.v_customer_ledger g
GROUP BY g.customer_id
HAVING SUM(g.debit - g.credit) >= @min_balance
), inv AS (
SELECT s.customer_id, s.net_total, ISNULL(s.due_date, DATEADD(day, @terms, s.invoice_date)) AS due,
       ISNULL(SUM(s.net_total) OVER (PARTITION BY s.customer_id ORDER BY s.invoice_date DESC, s.sale_id DESC
                                     ROWS BETWEEN UNBOUNDED PRECEDING AND 1 PRECEDING), 0) AS before_amt
FROM ai.v_sales s
WHERE s.is_credit = 1 AND s.net_total > 0 AND s.invoice_date >= @inv_from AND s.invoice_date <= @today
{bS}
), opn AS (
SELECT i.customer_id, i.due,
       CASE WHEN i.before_amt + i.net_total <= b.balance THEN i.net_total ELSE b.balance - i.before_amt END AS open_amt
FROM inv i INNER JOIN bal b ON b.customer_id = i.customer_id
WHERE i.before_amt < b.balance
), age AS (
SELECT customer_id, SUM(open_amt) AS allocated, COUNT(*) AS open_invoices,
       SUM(CASE WHEN due >= @today THEN open_amt ELSE 0 END) AS not_due,
       SUM(CASE WHEN DATEDIFF(day, due, @today) BETWEEN 1 AND 30 THEN open_amt ELSE 0 END) AS d1_30,
       SUM(CASE WHEN DATEDIFF(day, due, @today) BETWEEN 31 AND 60 THEN open_amt ELSE 0 END) AS d31_60,
       SUM(CASE WHEN DATEDIFF(day, due, @today) BETWEEN 61 AND 90 THEN open_amt ELSE 0 END) AS d61_90,
       SUM(CASE WHEN DATEDIFF(day, due, @today) > 90 THEN open_amt ELSE 0 END) AS d_over_90,
       MAX(CASE WHEN due < @today THEN DATEDIFF(day, due, @today) ELSE 0 END) AS oldest_days
FROM opn GROUP BY customer_id
)
SELECT TOP (@top) b.customer_id, c.customer_code, c.customer_name, c.group_id, c.currency_id, b.balance,
       ISNULL(a.allocated, 0) AS allocated, ISNULL(a.open_invoices, 0) AS open_invoices, ISNULL(a.not_due, 0) AS not_due,
       ISNULL(a.d1_30, 0) AS d1_30, ISNULL(a.d31_60, 0) AS d31_60, ISNULL(a.d61_90, 0) AS d61_90,
       ISNULL(a.d_over_90, 0) AS d_over_90, ISNULL(a.oldest_days, 0) AS oldest_days,
       CAST(0 AS decimal(18,3)) AS credit_sales, CAST(NULL AS date) AS last_receipt, k.credit_limit, k.limit_source
FROM bal b
INNER JOIN ai.v_customers c ON c.customer_id = b.customer_id
LEFT JOIN age a ON a.customer_id = b.customer_id
LEFT JOIN ai.v_customer_credit k ON k.customer_id = b.customer_id
WHERE c.active_flag = 1 AND c.customer_type = @ctype AND ISNULL(c.is_general_customer, 0) = 0
ORDER BY b.balance DESC";
        }

        /// <summary>مبيعات آجلة فى نافذة DSO لكل عميل (فروع النطاق).</summary>
        public static string SideSql(AiCollectScope scope, AiCollectSettings s, DateTime today, out Dictionary<string, object> p)
        {
            p = new Dictionary<string, object> { ["dso_from"] = today.AddDays(-s.DsoWindowDays), ["today"] = today };
            string b = BranchFilter("branch_id", "t", scope.BranchIds, p);
            return "SELECT customer_id, SUM(net_total) AS credit_sales FROM ai.v_sales WHERE is_credit = 1 AND invoice_date >= @dso_from AND invoice_date <= @today" + b + " GROUP BY customer_id";
        }

        /// <summary>آخر إذن استلام لكل عميل (صلاحية الصناديق من الجلسة).</summary>
        public static string ReceiptSql() =>
            "SELECT party_id AS customer_id, MAX(voucher_date) AS last_receipt FROM ai.v_cash_vouchers WHERE is_receipt = 1 AND party_type = 'customer' GROUP BY party_id";

        private Dictionary<long, object?> SideMap(string sql, Dictionary<string, object> p, AiCollectScope scope, HashSet<long> ids)
        {
            var map = new Dictionary<long, object?>();
            if (ids.Count == 0) return map;
            var r = Q(sql.Contains("v_cash_vouchers") ? "receipts" : "credit_sales", sql, p, scope);
            foreach (var x in r.Rows)
            {
                if (x.Length < 2 || x[0] == null || x[0] is DBNull) continue;
                long id;
                try { id = Convert.ToInt64(x[0], CultureInfo.InvariantCulture); } catch (FormatException) { continue; } catch (InvalidCastException) { continue; }
                if (ids.Contains(id)) map[id] = x[1];
            }
            return map;
        }

        private static DateTime? D(object? o) => o == null || o is DBNull ? null : Convert.ToDateTime(o, CultureInfo.InvariantCulture);

        private static string BranchFilter(string col, string prefix, List<long>? ids, Dictionary<string, object> p)
        {
            if (ids == null) return "";
            if (ids.Count == 0) return " AND 1 = 0";
            var names = new List<string>();
            int i = 0;
            foreach (var id in ids.Distinct()) { string n = prefix + (i++).ToString(CultureInfo.InvariantCulture); p[n] = id; names.Add("@" + n); }
            return " AND " + col + " IN (" + string.Join(", ", names) + ")";
        }

        /// <summary>الفواتير المفتوحة لعميل (FIFO فى C# — نفس توزيع SQL)، الأقدم استحقاقاً أولاً، حتى 200.</summary>
        public List<AiCollectInvoice> Invoices(long customerId, decimal balance, AiCollectScope scope, AiCollectSettings s, CancellationToken ct)
        {
            var today = Today().Date;
            var p = new Dictionary<string, object> { ["c"] = customerId, ["terms"] = s.TermsDays, ["inv_from"] = today.AddMonths(-s.InvoiceMonths), ["today"] = today };
            string b = BranchFilter("branch_id", "s", scope.BranchIds, p);
            var r = Q("invoices", $@"SELECT TOP (2000) sale_id, invoice_no, invoice_date, ISNULL(due_date, DATEADD(day, @terms, invoice_date)) AS due, net_total
FROM ai.v_sales WHERE customer_id = @c AND is_credit = 1 AND net_total > 0 AND invoice_date >= @inv_from AND invoice_date <= @today{b}
ORDER BY invoice_date DESC, sale_id DESC", p, scope);
            var list = r.Rows.Select(x => new AiCollectInvoice
            {
                SaleId = L(x[0]), InvoiceNo = L(x[1]), InvoiceDate = Convert.ToDateTime(x[2], CultureInfo.InvariantCulture),
                Due = Convert.ToDateTime(x[3], CultureInfo.InvariantCulture), NetTotal = M(x[4]),
            }).ToList();
            return AiCollectRules.Fifo(balance, list, today).OrderBy(x => x.Due).ThenBy(x => x.SaleId).Take(AiCollectConst.DetailMaxInvoices).ToList();
        }

        /// <summary>أعداد تنبيه الموجز من نفس الحساب (ت31).</summary>
        public AiCollectBriefCounts BriefCounts(AiCollectScope scope, AiCollectSettings s, CancellationToken ct)
        {
            var res = Compute(scope, s, false, ct);
            var today = Today().Date;
            var high = res.Rows.Where(r => r.Priority == AiCollectPriority.High && r.State != AiCollectState.Snoozed && r.State != AiCollectState.Disputed).ToList();
            return new AiCollectBriefCounts
            {
                HighCount = high.Count, HighAmount = high.Sum(r => r.Overdue),
                OverdueCustomers = res.Rows.Count(r => r.Overdue > AiCollectRules.Tolerance), OverdueAmount = res.Rows.Sum(r => r.Overdue),
                PromisesDueToday = res.Rows.Count(r => r.State == AiCollectState.Promised && r.PromiseDate.HasValue && r.PromiseDate.Value.Date == today),
                BrokenPromises = res.Rows.Count(r => r.State == AiCollectState.Broken),
                Top = high.Take(Math.Max(1, s.BriefTop)).Select(r => new AiCollectTopItem { CustomerId = r.CustomerId, Name = r.Name, Overdue = r.Overdue, Score = r.Score }).ToList(),
            };
        }

        // ───────────── مساعدات ─────────────

        private AiQueryResult Q(string name, string sql, Dictionary<string, object> p, AiCollectScope scope)
        {
            LastSql.Add(sql);
            Interlocked.Increment(ref QueryCount);
            var args = new { query = name, branches = scope.BranchIds == null ? "all" : string.Join(",", scope.BranchIds), tax = scope.TaxFilter };
            try
            {
                AiQueryResult r;
                if (QuerySource != null)
                {
                    ReadOnlySqlExecutor.Validate(sql, Allowed, new HashSet<string>(p.Keys, StringComparer.OrdinalIgnoreCase));
                    r = QuerySource(sql, p);
                }
                else r = _sql.Execute(sql, Allowed, ReadOnlySqlExecutor.SessionFor(scope.TaxFilter, scope.UserId), p, CancellationToken.None);
                Audit(scope.UserId, sql, args, r.Rows.Count, r.DurationMs, AiToolOutcome.Ok, null);
                return r;
            }
            catch (Exception ex)
            {
                Audit(scope.UserId, sql, args, null, null, AiToolOutcome.Error, ex.Message);
                throw;
            }
        }

        private void Audit(long userId, string sql, object args, int? rows, long? ms, string outcome, string? error)
        {
            if (!AuditEnabled) return;
            try
            {
                _audit.Insert(null, userId, AiCollectConst.QueryAuditTool, JsonSerializer.Serialize(args, Json), sql, rows,
                              ms == null ? null : (int)Math.Min(int.MaxValue, ms.Value), outcome, error);
            }
            catch (Exception ex) { try { operations.DataDictionaryLogerException(ex); } catch { } }
        }

        private static AiCollectResult Clone(AiCollectResult r, bool fromCache) => new()
        {
            Rows = r.Rows.ToList(), Truncated = r.Truncated, TaxRestricted = r.TaxRestricted, ComputedAt = r.ComputedAt, DurationMs = r.DurationMs, FromCache = fromCache,
        };

        private static long L(object? o) => o == null || o is DBNull ? 0 : Convert.ToInt64(o, CultureInfo.InvariantCulture);
        private static long? NL(object? o) => o == null || o is DBNull ? null : Convert.ToInt64(o, CultureInfo.InvariantCulture);
        private static int I(object? o) => o == null || o is DBNull ? 0 : Convert.ToInt32(o, CultureInfo.InvariantCulture);
        private static decimal M(object? o) => o == null || o is DBNull ? 0 : Convert.ToDecimal(o, CultureInfo.InvariantCulture);
        private static string? Str(object? o) => o == null || o is DBNull ? null : Convert.ToString(o, CultureInfo.InvariantCulture);
    }
}
