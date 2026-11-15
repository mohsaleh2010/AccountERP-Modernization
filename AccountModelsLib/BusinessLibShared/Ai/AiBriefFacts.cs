using System;
using System.Collections.Generic;

namespace AccountModelsLib
{
    // ───────────── P8.1.2: حقائق الموجز (AI_Briefs.FactsJson / AlertsJson) ─────────────
    // محايدة لغوياً (ق7): أرقام وأكواد وأسماء كيانات فقط؛ العناوين والنصوص ترسمها الواجهة من resx.
    // كل قسم null = لم يُطلب أو لا يملك المستلم شاشته أو فشل (السبب فى ErrorsJson).

    public class AiBriefFacts
    {
        public int CollectorVersion { get; set; } = AiBriefConst.CollectorVersion;
        public DateTime PeriodFrom { get; set; }
        public DateTime PeriodTo { get; set; }
        /// <summary>«اليوم» بتوقيت الموجز (ق15): مرجع الاستحقاق ورصيد الخزن الآن.</summary>
        public DateTime Today { get; set; }
        public List<long> BranchIds { get; set; } = new List<long>();
        /// <summary>الأقسام التى حُسبت فعلاً (بعد الصلاحيات واختيار الاشتراك).</summary>
        public List<string> Sections { get; set; } = new List<string>();
        public AiBriefSalesFacts Sales { get; set; }
        public AiBriefCollectionsFacts Collections { get; set; }
        public AiBriefLiquidityFacts Liquidity { get; set; }
        public AiBriefReceivablesFacts Receivables { get; set; }
        public AiBriefPurchasesFacts Purchases { get; set; }
        /// <summary>للتنبيه negative_stock فقط (بصلاحية أرصدة المخازن)؛ ليس قسماً مستقلاً.</summary>
        public AiBriefStockFacts Stock { get; set; }
        /// <summary>P8.4 (ش27): للتنبيه anomalies_new فقط (بصلاحية صفحة شذوذ القيود)؛ ليس قسماً مستقلاً.</summary>
        public AiBriefAnomalyFacts Anomalies { get; set; }

        public int PeriodDays => Math.Max(1, (int)(PeriodTo.Date - PeriodFrom.Date).TotalDays + 1);
    }

    /// <summary>اسم + قيمة (أعلى العملاء/الأصناف/المقبوضات).</summary>
    public class AiBriefNamedValue
    {
        public long Id { get; set; }
        public string Name { get; set; } = "";
        public decimal Value { get; set; }
        public int Count { get; set; }
    }

    public class AiBriefDaySales
    {
        public DateTime Date { get; set; }
        public int Invoices { get; set; }
        public decimal Net { get; set; }
    }

    public class AiBriefBranchSales
    {
        public long BranchId { get; set; }
        public string BranchName { get; set; } = "";
        public int Invoices { get; set; }
        public decimal Net { get; set; }
        public decimal Returns { get; set; }
    }

    /// <summary>كل المبالغ بالعملة المحلية (net_total_equivalent).</summary>
    public class AiBriefSalesFacts
    {
        public decimal NetSales { get; set; }
        public int Invoices { get; set; }
        public decimal AvgInvoice { get; set; }
        public decimal CashSales { get; set; }
        public int CashInvoices { get; set; }
        public decimal CreditSales { get; set; }
        public int CreditInvoices { get; set; }
        public decimal Returns { get; set; }
        public int ReturnCount { get; set; }
        public decimal NetAfterReturns { get; set; }
        /// <summary>ق19(أ): مجموع متوسط نفس يوم الأسبوع فى آخر 4 أسابيع لكل يوم فى الفترة (null لو لا بيانات).</summary>
        public decimal? WeekdayBaseline { get; set; }
        /// <summary>نسبة التغير عن الأساس (+ زيادة، − انخفاض)؛ null لو الأساس صفر.</summary>
        public decimal? VsWeekdayPct { get; set; }
        public DateTime MtdFrom { get; set; }
        public DateTime MtdTo { get; set; }
        public decimal Mtd { get; set; }
        public DateTime PrevMtdFrom { get; set; }
        public DateTime PrevMtdTo { get; set; }
        public decimal PrevMtd { get; set; }
        public decimal? VsPrevMonthPct { get; set; }
        /// <summary>هامش تقريبى = إجمالى السطور − (متوسط التكلفة وقت البيع × الكمية) للسطور التى لها تكلفة فقط.</summary>
        public decimal? MarginApprox { get; set; }
        public decimal? MarginPct { get; set; }
        /// <summary>نسبة السطور التى لها تكلفة مسجلة (كلما قلت كان الهامش أقل دقة).</summary>
        public decimal CostCoveragePct { get; set; }
        public int BelowCostLines { get; set; }
        public decimal BelowCostLoss { get; set; }
        public List<AiBriefNamedValue> BelowCostTop { get; set; } = new List<AiBriefNamedValue>();
        public List<AiBriefNamedValue> TopCustomers { get; set; } = new List<AiBriefNamedValue>();
        public List<AiBriefNamedValue> TopItems { get; set; } = new List<AiBriefNamedValue>();
        /// <summary>أيام الفترة كلها (اليوم بلا فواتير يظهر بصفر).</summary>
        public List<AiBriefDaySales> Days { get; set; } = new List<AiBriefDaySales>();
        /// <summary>حسب الفرع — فقط حين يغطى الموجز أكثر من فرع (ق12).</summary>
        public List<AiBriefBranchSales> ByBranch { get; set; } = new List<AiBriefBranchSales>();
    }

    /// <summary>حركة الخزن لكل عملة (لا جمع بين العملات) من محرك «إجمالى يوميات الخزينة».</summary>
    public class AiBriefCurrencyCash
    {
        public long CurrencyId { get; set; }
        public string CurrencyName { get; set; } = "";
        public string CurrencyNameEn { get; set; } = "";
        public decimal Receipts { get; set; }
        public decimal Payments { get; set; }
        public decimal Net { get; set; }
        /// <summary>متوسط المدفوعات اليومى فى الثلاثين يوماً السابقة للفترة (لتنبيه large_payment).</summary>
        public decimal PaymentsAvgPerDay30 { get; set; }
    }

    public class AiBriefCollectionsFacts
    {
        public int BoxCount { get; set; }
        public List<AiBriefCurrencyCash> ByCurrency { get; set; } = new List<AiBriefCurrencyCash>();
        /// <summary>أعلى مقبوضات من عملاء (amount_equivalent بالعملة المحلية).</summary>
        public List<AiBriefNamedValue> TopReceipts { get; set; } = new List<AiBriefNamedValue>();
    }

    public class AiBriefBoxBalance
    {
        public long BoxId { get; set; }
        public string BoxName { get; set; } = "";
        public long BranchId { get; set; }
        public long CurrencyId { get; set; }
        /// <summary>الرصيد الآن (BoxBalance كالشاشة؛ يقصّ السالب إلى صفر).</summary>
        public decimal Balance { get; set; }
        /// <summary>رصيد أول الفترة ورصيد آخرها من محرك الإجماليات (غير مقصوص).</summary>
        public decimal PeriodStart { get; set; }
        public decimal PeriodEnd { get; set; }
    }

    public class AiBriefCurrencyBalance
    {
        public long CurrencyId { get; set; }
        public string CurrencyName { get; set; } = "";
        public string CurrencyNameEn { get; set; } = "";
        public decimal Balance { get; set; }
        public decimal PeriodStart { get; set; }
        public decimal Change { get; set; }
    }

    public class AiBriefLiquidityFacts
    {
        public List<AiBriefBoxBalance> Boxes { get; set; } = new List<AiBriefBoxBalance>();
        public List<AiBriefCurrencyBalance> ByCurrency { get; set; } = new List<AiBriefCurrencyBalance>();
    }

    /// <summary>عدد ومبلغ الفواتير المتأخرة بعدد أيام التأخير (يطبّق التنبيه عتبة BriefOverdueDays عليها دون إعادة استعلام).</summary>
    public class AiBriefOverdueBucket
    {
        public int DaysOverdue { get; set; }
        public int Count { get; set; }
        public decimal Amount { get; set; }
    }

    /// <summary>الآجل غير المسدد لفواتير آخر 12 شهراً (rest_value بعملة الفاتورة — تقريبى).</summary>
    public class AiBriefReceivablesFacts
    {
        public DateTime FromDate { get; set; }
        public decimal OpenCredit { get; set; }
        public int OpenCount { get; set; }
        /// <summary>منها فواتير لها تاريخ استحقاق (التاريخ قليل الاستعمال، فالتأخير جزئى).</summary>
        public int WithDueDate { get; set; }
        public List<AiBriefOverdueBucket> Overdue { get; set; } = new List<AiBriefOverdueBucket>();
    }

    public class AiBriefPurchasesFacts
    {
        public decimal NetPurchases { get; set; }
        public int Invoices { get; set; }
        public decimal Returns { get; set; }
        public int ReturnCount { get; set; }
    }

    public class AiBriefNegativeItem
    {
        public long ItemId { get; set; }
        public string ItemName { get; set; } = "";
        public string StoreName { get; set; } = "";
        public decimal LargeQty { get; set; }
        public decimal SmallQty { get; set; }
    }

    public class AiBriefStockFacts
    {
        public int NegativeCount { get; set; }
        public List<AiBriefNegativeItem> NegativeTop { get; set; } = new List<AiBriefNegativeItem>();
    }

    /// <summary>P8.4: شذوذ غير مراجَع (ليس تاريخياً) فى فروع النطاق. NewCount/HighCount = ما كُشف منذ بداية الفترة، OpenTotal = كل المفتوح.</summary>
    public class AiBriefAnomalyFacts
    {
        public int NewCount { get; set; }
        public int HighCount { get; set; }
        public int OpenTotal { get; set; }
        /// <summary>أعلى 5: «#رقم القيد — اسم الحساب».</summary>
        public List<string> Top { get; set; } = new List<string>();
    }

    public static class AiBriefAlertCodes
    {
        public const string SalesDrop = "sales_drop";
        public const string NoSales = "no_sales";
        public const string ReturnsHigh = "returns_high";
        public const string BelowCost = "below_cost";
        public const string CashLow = "cash_low";
        public const string OverdueReceivables = "overdue_receivables";
        public const string NegativeStock = "negative_stock";
        public const string LargePayment = "large_payment";
        /// <summary>P8.4 (ش27).</summary>
        public const string AnomaliesNew = "anomalies_new";

        public static readonly string[] All = { SalesDrop, NoSales, ReturnsHigh, BelowCost, CashLow, OverdueReceivables, NegativeStock, LargePayment, AnomaliesNew };
    }

    public static class AiBriefSeverity
    {
        public const string High = "high";
        public const string Medium = "medium";
        public const string Info = "info";

        public static int Rank(string s) => s == High ? 0 : s == Medium ? 1 : 2;
    }

    /// <summary>تنبيه واحد (AlertsJson): الكود والقيم فقط؛ النص من resx (AIB_Al_&lt;code&gt;_Text).</summary>
    public class AiBriefAlert
    {
        public string Code { get; set; } = "";
        public string Severity { get; set; } = AiBriefSeverity.Info;
        /// <summary>قيم رقمية بأسماء ثابتة لكل كود (تُعرض فى النص).</summary>
        public Dictionary<string, decimal> Values { get; set; } = new Dictionary<string, decimal>();
        /// <summary>أسماء/تواريخ مرافقة (أعلى 5 أصناف، أيام بلا مبيعات، صناديق …).</summary>
        public List<string> Items { get; set; } = new List<string>();
        /// <summary>الشاشة التى يفتحها زر «افتح الشاشة» ومعاملاتها (from/to yyyy-MM-dd …).</summary>
        public string Route { get; set; } = "";
        public Dictionary<string, string> Params { get; set; } = new Dictionary<string, string>();
    }
}
