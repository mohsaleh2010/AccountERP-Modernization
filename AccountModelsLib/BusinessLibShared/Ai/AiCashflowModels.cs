using System;
using System.Collections.Generic;
using System.Globalization;

namespace AccountModelsLib
{
    /// <summary>P8.7 (ai-assistant-p87-plan.md): ثوابت التوقع.</summary>
    public static class AiCashflowConst
    {
        public const int Weeks = 13;
        public const string Route = "/accounts/cashflow-forecast";
        public const string MenuKey = "menuItemAiCashflow";
        public const string ScenarioBase = "base";
        public const string ScenarioConservative = "conservative";

        // أنواع المصادر (و16)
        public const string SrcOpening = "opening";
        public const string SrcCustomer = "customer";
        public const string SrcChequeIn = "cheque_in";
        public const string SrcCashSales = "cash_sales";
        public const string SrcManualIn = "manual_in";
        public const string SrcSupplier = "supplier";
        public const string SrcChequeOut = "cheque_out";
        public const string SrcRecurring = "recurring";
        public const string SrcManualOut = "manual_out";

        public static bool IsInflow(string src) => src == SrcCustomer || src == SrcChequeIn || src == SrcCashSales || src == SrcManualIn;
    }

    public class AiCashflowSettings
    {
        public bool Enabled { get; set; }
        public int CustomerTermsDays { get; set; } = 30;
        public int SupplierTermsDays { get; set; } = 30;
        public decimal MinBalance { get; set; }
        public bool IncludeCashSales { get; set; } = true;
        public int RecurringMinMonths { get; set; } = 3;
        public int BaseDelayDays { get; set; } = 7;
        public int ConservativeDelayDays { get; set; } = 14;
        public int ConservativeCollectPct { get; set; } = 80;

        public static AiCashflowSettings From(IDictionary<string, string> s)
        {
            var r = new AiCashflowSettings();
            if (s == null) return r;
            r.Enabled = Get(s, AiSettingKeys.CashflowEnabled, "0") == "1";
            r.IncludeCashSales = Get(s, AiSettingKeys.CashflowIncludeCashSales, "1") == "1";
            r.CustomerTermsDays = Int(s, AiSettingKeys.CollectTermsDays, 30, 0, 365);
            r.SupplierTermsDays = Int(s, AiSettingKeys.CashflowSupplierTermsDays, 30, 0, 365);
            r.RecurringMinMonths = Int(s, AiSettingKeys.CashflowRecurringMinMonths, 3, 2, 4);
            r.BaseDelayDays = Int(s, AiSettingKeys.CashflowBaseDelayDays, 7, 0, 120);
            r.ConservativeDelayDays = Int(s, AiSettingKeys.CashflowConservativeDelayDays, 14, 0, 180);
            r.ConservativeCollectPct = Int(s, AiSettingKeys.CashflowConservativeCollectPct, 80, 0, 100);
            decimal d;
            if (decimal.TryParse(Get(s, AiSettingKeys.CashflowMinBalance, "0"), NumberStyles.Number, CultureInfo.InvariantCulture, out d)) r.MinBalance = Math.Max(0, d);
            return r;
        }

        private static int Int(IDictionary<string, string> s, string k, int def, int min, int max)
        {
            int v;
            return int.TryParse(Get(s, k, def.ToString(CultureInfo.InvariantCulture)), NumberStyles.Integer, CultureInfo.InvariantCulture, out v) ? Math.Max(min, Math.Min(max, v)) : def;
        }

        private static string Get(IDictionary<string, string> s, string k, string def)
        {
            string v;
            return s.TryGetValue(k, out v) && !string.IsNullOrWhiteSpace(v) ? v.Trim() : def;
        }
    }

    /// <summary>بند متوقع واحد (فاتورة، شيك، مصروف متكرر، تعديل…) قبل التوزيع على الأسابيع.</summary>
    public class AiCashflowItem
    {
        public string Source { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Label { get; set; }
        public long RefId { get; set; }
        /// <summary>رابط داخلى للشاشة الأصلية (و16).</summary>
        public string Url { get; set; }
        /// <summary>تاريخ مقدّر (لا استحقاق فعلى مسجّل).</summary>
        public bool Estimated { get; set; }
    }

    public class AiCashflowWeek
    {
        public int Index { get; set; }
        public DateTime Start { get; set; }
        public decimal Opening { get; set; }
        public decimal Customers { get; set; }
        public decimal ChequesIn { get; set; }
        public decimal CashSales { get; set; }
        public decimal ManualIn { get; set; }
        public decimal Suppliers { get; set; }
        public decimal ChequesOut { get; set; }
        public decimal Recurring { get; set; }
        public decimal ManualOut { get; set; }
        public decimal Inflows => Customers + ChequesIn + CashSales + ManualIn;
        public decimal Outflows => Suppliers + ChequesOut + Recurring + ManualOut;
        public decimal Net => Inflows - Outflows;
        public decimal Closing { get; set; }
        public int OverdueItems { get; set; }
    }

    public class AiCashflowFacts
    {
        public DateTime AsOf { get; set; }
        public string Scenario { get; set; }
        public decimal Opening { get; set; }
        public List<AiCashflowWeek> Weeks { get; set; } = new List<AiCashflowWeek>();
        /// <summary>أول أسبوع يقل رصيده الختامى عن الحد الأدنى (-1 = لا يوجد).</summary>
        public int LowWeek { get; set; } = -1;
        public decimal MinBalance { get; set; }
        public decimal LowestClosing { get; set; }
        public int LowestWeek { get; set; }
        /// <summary>نسبة بنود العملاء/الموردين ذات الاستحقاق المقدّر (0..1).</summary>
        public double EstimatedShare { get; set; }
        public bool NegativeBoxes { get; set; }
        public List<AiCashflowItem> Items { get; set; } = new List<AiCashflowItem>();
        public List<string> Warnings { get; set; } = new List<string>();
    }

    /// <summary>مصدر خام للمحرك (من SQL أو الاختبار).</summary>
    public class AiCashflowSources
    {
        public decimal Opening { get; set; }
        public bool NegativeBoxes { get; set; }
        public List<AiCashflowItem> Receivables { get; set; } = new List<AiCashflowItem>();
        public List<AiCashflowItem> Payables { get; set; } = new List<AiCashflowItem>();
        public List<AiCashflowItem> ChequesIn { get; set; } = new List<AiCashflowItem>();
        public List<AiCashflowItem> ChequesOut { get; set; } = new List<AiCashflowItem>();
        public List<AiCashflowItem> Recurring { get; set; } = new List<AiCashflowItem>();
        /// <summary>متوسط المبيعات النقدية الأسبوعى.</summary>
        public decimal CashSalesWeekly { get; set; }
        public List<AiCashflowItem> Manual { get; set; } = new List<AiCashflowItem>();
    }

    public class AiCashflowAdjustmentRow
    {
        public long Id { get; set; }
        public long User_Id { get; set; }
        public long Branch_Id { get; set; }
        public DateTime WeekStart { get; set; }
        public string Direction { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public string UserName { get; set; }
    }

    public class AiCashflowRunRow
    {
        public long Id { get; set; }
        public long User_Id { get; set; }
        public string BranchIds { get; set; }
        public DateTime AsOf { get; set; }
        public string Scenario { get; set; }
        public string Source { get; set; }
        public string FactsJson { get; set; }
        public string Narrative { get; set; }
        public string Language { get; set; }
        public string Status { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    /// <summary>دقة الأسبوع الماضى (و11): توقع لقطة سابقة مقابل الفعلى من الخزائن.</summary>
    public class AiCashflowAccuracy
    {
        public DateTime WeekStart { get; set; }
        public decimal ForecastIn { get; set; }
        public decimal ActualIn { get; set; }
        public decimal ForecastOut { get; set; }
        public decimal ActualOut { get; set; }
        public double ErrorPct { get; set; }
    }

    public class AiCashflowRecurringRow
    {
        public long SubAccountId { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public int Day { get; set; }
        public bool Excluded { get; set; }
    }

    public class AiCashflowView
    {
        public AiCashflowFacts Base { get; set; }
        public AiCashflowFacts Conservative { get; set; }
        public long RunId { get; set; }
        public DateTime? ComputedAt { get; set; }
        public string Narrative { get; set; }
        public AiCashflowAccuracy Accuracy { get; set; }
        public List<AiCashflowAdjustmentRow> Adjustments { get; set; } = new List<AiCashflowAdjustmentRow>();
    }

    public interface IAiCashflowGateway
    {
        bool CanView(AiUserContext user);
        System.Threading.Tasks.Task<AiCashflowView> GetAsync(long branchId, bool recompute, string language, AiUserContext user, System.Threading.CancellationToken ct);
        long AddAdjustment(AiCashflowAdjustmentRow row, AiUserContext user);
        bool CancelAdjustment(long id, AiUserContext user);
        List<AiCashflowRecurringRow> Recurring(AiUserContext user);
        void SetExcluded(long subAccountId, bool excluded, AiUserContext user);
        AiCashflowSettings GetSettings(AiUserContext user);
        void SaveSettings(AiCashflowSettings s, AiUserContext user);
    }

    public class AiCashflowException : InvalidOperationException
    {
        public string Code { get; }
        public AiCashflowException(string code, string message) : base(message) { Code = code ?? ""; }
    }
}
