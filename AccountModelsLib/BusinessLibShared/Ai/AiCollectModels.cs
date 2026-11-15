using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AccountModelsLib
{
    // ───────────── P8.5: أولويات التحصيل (.claude/docs/ai-assistant-p85-plan.md) ─────────────
    // أسلوب net48 / C# 7.3: بلا record ولا init ولا required ولا nullable annotations.

    public static class AiCollectActionTypes
    {
        public const string Call = "call";
        public const string Promise = "promise";
        public const string Note = "note";
        public const string Snooze = "snooze";
        public const string Dispute = "dispute";
        public const string ReminderCopied = "reminder_copied";
        public const string ReminderSent = "reminder_sent";

        public static readonly string[] All = { Call, Promise, Note, Snooze, Dispute, ReminderCopied, ReminderSent };
        /// <summary>الأنواع التى يسجّلها المستخدم بيده (حوار الإجراء).</summary>
        public static readonly string[] UserTypes = { Call, Promise, Note, Snooze, Dispute };

        public static bool IsUserType(string t) => UserTypes.Contains(t ?? "", StringComparer.Ordinal);
    }

    public static class AiCollectActionStatus
    {
        public const string Open = "Open";
        public const string Closed = "Closed";
        public const string Cancelled = "Cancelled";
    }

    public static class AiCollectPriority
    {
        public const string High = "high";
        public const string Medium = "medium";
        public const string Low = "low";

        public static int Rank(string p) => p == High ? 3 : p == Medium ? 2 : p == Low ? 1 : 0;
    }

    /// <summary>حالة المتابعة المشتقة من السجل (ت19).</summary>
    public static class AiCollectState
    {
        public const string Due = "due";
        public const string Promised = "promised";
        public const string Broken = "broken";
        public const string Snoozed = "snoozed";
        public const string Disputed = "disputed";

        public static readonly string[] All = { Due, Promised, Broken, Snoozed, Disputed };
    }

    public static class AiCollectDraftSource
    {
        public const string Template = "template";
        public const string Model = "model";
        public const string Rejected = "rejected";
        public const string Error = "error";
        public const string Capped = "capped";
        public const string Off = "off";
    }

    public static class AiCollectConst
    {
        public const string Route = "/accounts/collections-priority";
        public const string MenuKey = "menuItemCollectionsPriority";
        public const string SiblingMenuKey = "menuItemCustomerAccountBalance";
        public const string ToolName = "get_collections_priority";
        /// <summary>Event_Type فى SalesRep_Notify_Outbox — خارج enum SalesRepNotifyEvent عمداً (ت27).</summary>
        public const int OutboxEventType = 20;
        public const int PageSize = 50;
        public const int DraftMaxTokens = 400;
        public const int MessageMaxChars = 1000;
        public const int NoteMaxChars = 1000;
        public const int DetailMaxInvoices = 200;
        public const int UnallocatedMinDays = 91;
        public const int CacheMaxEntries = 50;
        public const int MaxSnoozeDays = 90;
        public const string DraftAuditTool = "collect_draft";
        public const string SendAuditTool = "collect_send";
        public const string QueryAuditTool = "collect_query";
        public const string CustomerToken = "[C1]";
        public const string CustomersRoute = "/customers/list";
        public const string BalancesRoute = "/accounts/customer-balance-summary";
        public const string SalesRoute = "/accounts/invoices/list";
    }

    /// <summary>إعدادات أولويات التحصيل من AI_Settings. From يعيد الفاسد للافتراضى؛ Validate يحكم على ما ترسله الواجهة.</summary>
    public class AiCollectSettings
    {
        public int TermsDays { get; set; } = 30;
        public decimal MinBalance { get; set; } = 100;
        public int InvoiceMonths { get; set; } = 24;
        public int MaxCustomers { get; set; } = 2000;
        /// <summary>عمر، مبلغ، سلوك، حد، آخر تحصيل.</summary>
        public decimal[] Weights { get; set; } = { 35, 25, 15, 15, 10 };
        public int AgeCapDays { get; set; } = 120;
        public decimal AmountCap { get; set; } = 100000;
        public int DsoWindowDays { get; set; } = 180;
        public int DsoCapDays { get; set; } = 90;
        public int IdleStartDays { get; set; } = 30;
        public int IdleCapDays { get; set; } = 90;
        public int HighScore { get; set; } = 70;
        public int MediumScore { get; set; } = 40;
        public int Level2Days { get; set; } = 30;
        public int Level3Days { get; set; } = 60;
        public int BrokenPromiseBonus { get; set; } = 10;
        public int BrokenPromiseDays { get; set; } = 30;
        public int CacheMinutes { get; set; } = 5;
        public bool Draft { get; set; } = true;
        public int DraftMaxPerDay { get; set; } = 50;
        public bool SendEnabled { get; set; }
        public int ReminderMinDays { get; set; } = 3;
        public bool BriefAlert { get; set; } = true;
        public int BriefTop { get; set; } = 5;

        public static AiCollectSettings From(IReadOnlyDictionary<string, string> s)
        {
            string V(string k) => s != null && s.TryGetValue(k, out var v) ? (v ?? "").Trim() : "";
            int I(string k, int def, int min, int max) => (int)Dec(V(k), def, min, max);
            var r = new AiCollectSettings();
            r.TermsDays = I(AiSettingKeys.CollectTermsDays, 30, 0, 365);
            r.MinBalance = Dec(V(AiSettingKeys.CollectMinBalance), 100, 0, 1000000000m);
            r.InvoiceMonths = I(AiSettingKeys.CollectInvoiceMonths, 24, 1, 120);
            r.MaxCustomers = I(AiSettingKeys.CollectMaxCustomers, 2000, 10, 4999);
            var w = AiCollectRules.ParseWeights(V(AiSettingKeys.CollectWeights));
            if (w != null) r.Weights = w;
            r.AgeCapDays = I(AiSettingKeys.CollectAgeCapDays, 120, 1, 3650);
            r.AmountCap = Dec(V(AiSettingKeys.CollectAmountCap), 100000, 1, 1000000000000m);
            r.DsoWindowDays = I(AiSettingKeys.CollectDsoWindowDays, 180, 30, 730);
            r.DsoCapDays = I(AiSettingKeys.CollectDsoCapDays, 90, 1, 3650);
            r.IdleStartDays = I(AiSettingKeys.CollectIdleStartDays, 30, 0, 3650);
            r.IdleCapDays = I(AiSettingKeys.CollectIdleCapDays, 90, 1, 3650);
            r.HighScore = I(AiSettingKeys.CollectHighScore, 70, 1, 100);
            r.MediumScore = I(AiSettingKeys.CollectMediumScore, 40, 1, 100);
            if (r.MediumScore >= r.HighScore) { r.HighScore = 70; r.MediumScore = 40; }
            r.Level2Days = I(AiSettingKeys.CollectLevel2Days, 30, 2, 3650);
            r.Level3Days = I(AiSettingKeys.CollectLevel3Days, 60, 3, 3650);
            if (r.Level3Days <= r.Level2Days) { r.Level2Days = 30; r.Level3Days = 60; }
            r.BrokenPromiseBonus = I(AiSettingKeys.CollectBrokenPromiseBonus, 10, 0, 100);
            r.BrokenPromiseDays = I(AiSettingKeys.CollectBrokenPromiseDays, 30, 1, 365);
            r.CacheMinutes = I(AiSettingKeys.CollectCacheMinutes, 5, 0, 120);
            r.Draft = V(AiSettingKeys.CollectDraft) != "0";
            r.DraftMaxPerDay = I(AiSettingKeys.CollectDraftMaxPerDay, 50, 0, 1000);
            r.SendEnabled = V(AiSettingKeys.CollectSendEnabled) == "1";
            r.ReminderMinDays = I(AiSettingKeys.CollectReminderMinDays, 3, 0, 365);
            r.BriefAlert = V(AiSettingKeys.CollectBriefAlert) != "0";
            r.BriefTop = I(AiSettingKeys.CollectBriefTop, 5, 1, 20);
            return r;
        }

        /// <summary>مفاتيح أخطاء AIL_Set_Err* (فارغة = صالح).</summary>
        public List<string> Validate()
        {
            var e = new List<string>();
            void Add(string k) { if (!e.Contains(k)) e.Add(k); }
            if (Weights == null || Weights.Length != 5 || Weights.Any(x => x < 0 || x > 100)) Add(AiCollectErrors.Weights);
            else if (Weights.Sum() <= 0) Add(AiCollectErrors.WeightsZero);
            if (MediumScore < 1 || HighScore > 100 || MediumScore >= HighScore) Add(AiCollectErrors.Scores);
            if (Level2Days < 2 || Level3Days <= Level2Days) Add(AiCollectErrors.Levels);
            if (MinBalance < 0 || AmountCap <= 0) Add(AiCollectErrors.Amount);
            if (TermsDays < 0 || TermsDays > 365 || InvoiceMonths < 1 || InvoiceMonths > 120 || MaxCustomers < 10 || MaxCustomers > 4999
                || AgeCapDays < 1 || DsoWindowDays < 30 || DsoWindowDays > 730 || DsoCapDays < 1 || IdleStartDays < 0 || IdleCapDays < 1
                || BrokenPromiseBonus < 0 || BrokenPromiseBonus > 100 || BrokenPromiseDays < 1 || BrokenPromiseDays > 365
                || CacheMinutes < 0 || CacheMinutes > 120 || DraftMaxPerDay < 0 || DraftMaxPerDay > 1000 || ReminderMinDays < 0 || ReminderMinDays > 365
                || BriefTop < 1 || BriefTop > 20)
                Add(AiCollectErrors.Range);
            return e;
        }

        public List<KeyValuePair<string, string>> ToPairs()
        {
            var c = CultureInfo.InvariantCulture;
            var l = new List<KeyValuePair<string, string>>();
            void P(string k, string v) => l.Add(new KeyValuePair<string, string>(k, v));
            P(AiSettingKeys.CollectTermsDays, TermsDays.ToString(c));
            P(AiSettingKeys.CollectMinBalance, MinBalance.ToString(c));
            P(AiSettingKeys.CollectInvoiceMonths, InvoiceMonths.ToString(c));
            P(AiSettingKeys.CollectMaxCustomers, MaxCustomers.ToString(c));
            P(AiSettingKeys.CollectWeights, string.Join(",", (Weights ?? new decimal[0]).Select(w => w.ToString(c))));
            P(AiSettingKeys.CollectAgeCapDays, AgeCapDays.ToString(c));
            P(AiSettingKeys.CollectAmountCap, AmountCap.ToString(c));
            P(AiSettingKeys.CollectDsoWindowDays, DsoWindowDays.ToString(c));
            P(AiSettingKeys.CollectDsoCapDays, DsoCapDays.ToString(c));
            P(AiSettingKeys.CollectIdleStartDays, IdleStartDays.ToString(c));
            P(AiSettingKeys.CollectIdleCapDays, IdleCapDays.ToString(c));
            P(AiSettingKeys.CollectHighScore, HighScore.ToString(c));
            P(AiSettingKeys.CollectMediumScore, MediumScore.ToString(c));
            P(AiSettingKeys.CollectLevel2Days, Level2Days.ToString(c));
            P(AiSettingKeys.CollectLevel3Days, Level3Days.ToString(c));
            P(AiSettingKeys.CollectBrokenPromiseBonus, BrokenPromiseBonus.ToString(c));
            P(AiSettingKeys.CollectBrokenPromiseDays, BrokenPromiseDays.ToString(c));
            P(AiSettingKeys.CollectCacheMinutes, CacheMinutes.ToString(c));
            P(AiSettingKeys.CollectDraft, Draft ? "1" : "0");
            P(AiSettingKeys.CollectDraftMaxPerDay, DraftMaxPerDay.ToString(c));
            P(AiSettingKeys.CollectSendEnabled, SendEnabled ? "1" : "0");
            P(AiSettingKeys.CollectReminderMinDays, ReminderMinDays.ToString(c));
            P(AiSettingKeys.CollectBriefAlert, BriefAlert ? "1" : "0");
            P(AiSettingKeys.CollectBriefTop, BriefTop.ToString(c));
            return l;
        }

        /// <summary>بصمة ما يغيّر الأرقام (مفتاح الذاكرة).</summary>
        public string Fingerprint() =>
            string.Join("|", ToPairs().Where(p => p.Key != AiSettingKeys.CollectSendEnabled && p.Key != AiSettingKeys.CollectDraft
                && p.Key != AiSettingKeys.CollectDraftMaxPerDay && p.Key != AiSettingKeys.CollectReminderMinDays
                && p.Key != AiSettingKeys.CollectBriefAlert && p.Key != AiSettingKeys.CollectBriefTop).Select(p => p.Value));

        public AiCollectSettings Clone()
        {
            var c = (AiCollectSettings)MemberwiseClone();
            c.Weights = (decimal[])(Weights ?? new decimal[0]).Clone();
            return c;
        }

        private static decimal Dec(string s, decimal fallback, decimal min, decimal max)
        {
            decimal d;
            return decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out d) && d >= min && d <= max ? d : fallback;
        }
    }

    /// <summary>مدخلات الدرجة لعميل (§1).</summary>
    public class AiCollectFacts
    {
        public decimal Balance { get; set; }
        public decimal NotDue { get; set; }
        public decimal D1_30 { get; set; }
        public decimal D31_60 { get; set; }
        public decimal D61_90 { get; set; }
        public decimal DOver90 { get; set; }
        /// <summary>المفتوح الموزَّع على فواتير النافذة (قبل Derive).</summary>
        public decimal Allocated { get; set; }
        public decimal Unallocated { get; set; }
        public decimal Overdue { get; set; }
        public int OldestDays { get; set; }
        public int OpenInvoices { get; set; }
        public decimal CreditSales { get; set; }
        public int? Dso { get; set; }
        public DateTime? LastReceiptDate { get; set; }
        public int? IdleDays { get; set; }
        public decimal? CreditLimit { get; set; }
    }

    public class AiCollectScore
    {
        public int Score { get; set; }
        public string Priority { get; set; } = AiCollectPriority.Low;
        public int Level { get; set; }
        /// <summary>نقاط كل عامل من 100 (عمر، مبلغ، سلوك، حد، آخر تحصيل).</summary>
        public decimal[] Parts { get; set; } = new decimal[5];
        public bool BrokenBonus { get; set; }
    }

    /// <summary>صف عميل فى القائمة.</summary>
    public class AiCollectRow
    {
        public long CustomerId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public long? GroupId { get; set; }
        public long? CurrencyId { get; set; }
        public decimal Balance { get; set; }
        public decimal Overdue { get; set; }
        public decimal NotDue { get; set; }
        public decimal D1_30 { get; set; }
        public decimal D31_60 { get; set; }
        public decimal D61_90 { get; set; }
        public decimal DOver90 { get; set; }
        public decimal Unallocated { get; set; }
        public int OldestDays { get; set; }
        public int OpenInvoices { get; set; }
        public decimal CreditSales { get; set; }
        public int? Dso { get; set; }
        public DateTime? LastReceiptDate { get; set; }
        public int? IdleDays { get; set; }
        public decimal? CreditLimit { get; set; }
        public string LimitSource { get; set; }
        public int Score { get; set; }
        public string Priority { get; set; } = AiCollectPriority.Low;
        public int Level { get; set; }
        public decimal[] Parts { get; set; } = new decimal[5];
        public bool BrokenBonus { get; set; }
        public string State { get; set; } = AiCollectState.Due;
        public DateTime? PromiseDate { get; set; }
        public decimal? PromiseAmount { get; set; }
        public DateTime? SnoozeUntil { get; set; }
        public DateTime? LastReminderAt { get; set; }
        public DateTime? LastActionAt { get; set; }
        public string LastActionType { get; set; }

        public bool OverLimit => CreditLimit.HasValue && Balance > CreditLimit.Value;
    }

    public class AiCollectInvoice
    {
        public long SaleId { get; set; }
        public long InvoiceNo { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime Due { get; set; }
        public decimal NetTotal { get; set; }
        public decimal OpenAmount { get; set; }
        public int DaysOverdue { get; set; }
    }

    public class AiCollectActionRow
    {
        public long Id { get; set; }
        public long Customer_Id { get; set; }
        public string ActionType { get; set; } = "";
        public string Status { get; set; } = AiCollectActionStatus.Open;
        public string Note { get; set; }
        public DateTime? PromiseDate { get; set; }
        public decimal? PromiseAmount { get; set; }
        public DateTime? NextActionDate { get; set; }
        public int? Channel { get; set; }
        public string MessageText { get; set; }
        public string MessageSource { get; set; }
        public string Language { get; set; }
        public int? DunningLevel { get; set; }
        public long? Outbox_Id { get; set; }
        public decimal? BalanceAt { get; set; }
        public decimal? OverdueAt { get; set; }
        public int? OldestDaysAt { get; set; }
        public int? ScoreAt { get; set; }
        public long Branch_Id { get; set; }
        public long? ClosedBy { get; set; }
        public DateTime? ClosedAt { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string UserName { get; set; }
        public int? OutboxStatus { get; set; }
        public string OutboxError { get; set; }
    }

    public class AiCollectDraftRow
    {
        public long Id { get; set; }
        public long Customer_Id { get; set; }
        public string Language { get; set; } = "ar-EG";
        public int DunningLevel { get; set; }
        public string FactsHash { get; set; } = "";
        public string DraftText { get; set; }
        public string Source { get; set; } = AiCollectDraftSource.Off;
        public long? Model_Id { get; set; }
        public string ModelName { get; set; }
        public int? InputTokens { get; set; }
        public int? OutputTokens { get; set; }
        public decimal? CostEstimate { get; set; }
        public string RejectReason { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class AiCollectDetail
    {
        public AiCollectRow Customer { get; set; }
        public List<AiCollectInvoice> Invoices { get; set; } = new List<AiCollectInvoice>();
        public bool InvoicesHidden { get; set; }
        public List<AiCollectActionRow> Actions { get; set; } = new List<AiCollectActionRow>();
    }

    public class AiCollectFilter
    {
        /// <summary>فارغ = الكل.</summary>
        public string Priority { get; set; }
        /// <summary>due (الافتراضى: تحتاج متابعة) / promised / broken / snoozed / disputed / all.</summary>
        public string State { get; set; } = AiCollectState.Due;
        public long BranchId { get; set; }
        public string Search { get; set; }
        public bool OverdueOnly { get; set; } = true;
        public decimal MinOverdue { get; set; }
        public int Page { get; set; }
    }

    public class AiCollectPage
    {
        public List<AiCollectRow> Items { get; set; } = new List<AiCollectRow>();
        public int Total { get; set; }
        public bool Truncated { get; set; }
        public bool TaxRestricted { get; set; }
        public DateTime ComputedAt { get; set; }
    }

    public class AiCollectSummary
    {
        public decimal OverdueTotal { get; set; }
        public int OverdueCustomers { get; set; }
        public int HighCount { get; set; }
        public decimal HighAmount { get; set; }
        public int PromisesDueToday { get; set; }
        public int BrokenPromises { get; set; }
        /// <summary>غير مستحق، 1–30، 31–60، 61–90، +90، غير مرتبط.</summary>
        public decimal[] Buckets { get; set; } = new decimal[6];
        public bool MixedCurrency { get; set; }
        public bool SendAvailable { get; set; }
        public List<int> SendChannels { get; set; } = new List<int>();
        public bool DraftAvailable { get; set; }
        public bool TaxRestricted { get; set; }
        public bool Truncated { get; set; }
        public DateTime ComputedAt { get; set; }
    }

    public class AiCollectActionInput
    {
        public long CustomerId { get; set; }
        public string ActionType { get; set; }
        public string Note { get; set; }
        public DateTime? PromiseDate { get; set; }
        public decimal? PromiseAmount { get; set; }
        public DateTime? NextActionDate { get; set; }
    }

    public class AiCollectDraft
    {
        public string Text { get; set; }
        public string Source { get; set; } = AiCollectDraftSource.Template;
        public int Level { get; set; }
        public string Language { get; set; }
        public string ModelName { get; set; }
        public string RejectReason { get; set; }
    }

    public class AiCollectSendPreview
    {
        public string CustomerName { get; set; }
        public int Channel { get; set; }
        public string PhoneShown { get; set; }
        public bool CanSend { get; set; }
        public string BlockCode { get; set; }
    }

    public class AiCollectSendResult
    {
        public long ActionId { get; set; }
        public long OutboxId { get; set; }
    }

    public class AiCollectBriefCounts
    {
        public int HighCount { get; set; }
        public decimal HighAmount { get; set; }
        public int OverdueCustomers { get; set; }
        public decimal OverdueAmount { get; set; }
        public int PromisesDueToday { get; set; }
        public int BrokenPromises { get; set; }
        public List<AiCollectTopItem> Top { get; set; } = new List<AiCollectTopItem>();
    }

    public class AiCollectTopItem
    {
        public long CustomerId { get; set; }
        public string Name { get; set; }
        public decimal Overdue { get; set; }
        public int Score { get; set; }
    }

    public class AiCollectException : InvalidOperationException
    {
        public string Code { get; }
        public AiCollectException(string code, string message) : base(message) { Code = code ?? ""; }
    }

    public static class AiCollectErrors
    {
        public const string NotAdmin = "AIL_Set_ErrNotAdmin";
        public const string NoPermission = "AIL_ErrNoPermission";
        public const string NotFound = "AIL_ErrNotFound";
        public const string Weights = "AIL_Set_ErrWeights";
        public const string WeightsZero = "AIL_Set_ErrWeightsZero";
        public const string Scores = "AIL_Set_ErrScores";
        public const string Levels = "AIL_Set_ErrLevels";
        public const string Amount = "AIL_Set_ErrAmount";
        public const string Range = "AIL_Set_ErrRange";
        public const string ActionType = "AIL_Err_ActionType";
        public const string PromiseDate = "AIL_Err_PromiseDate";
        public const string PromiseAmount = "AIL_Err_PromiseAmount";
        public const string SnoozeDate = "AIL_Err_SnoozeDate";
        public const string NoteLength = "AIL_Err_NoteLength";
        public const string CancelNotAllowed = "AIL_Err_CancelNotAllowed";
        public const string SendOff = "AIL_Err_SendOff";
        public const string NotConfirmed = "AIL_Err_NotConfirmed";
        public const string MessageEmpty = "AIL_Err_MessageEmpty";
        public const string MessageLength = "AIL_Err_MessageLength";
        public const string Disputed = "AIL_Err_Disputed";
        public const string TooSoon = "AIL_Err_TooSoon";
        public const string NoPhone = "AIL_Err_NoPhone";
        public const string OptOut = "AIL_Err_OptOut";
        public const string Channel = "AIL_Err_Channel";
    }

    /// <summary>
    /// بوابة أولويات التحصيل (ينفّذها AccountERP.AI ويسجّلها مضيف الويب فقط — ت33). العرض بصلاحية الشاشة وفروع المستخدم وصلاحيته الضريبية؛
    /// الإعدادات للمدير. لا تعديل على أى رصيد أو فاتورة؛ الكتابة على AI_Collection* فقط وإدراج صف طابور عند الإرسال المؤكَّد.
    /// </summary>
    public interface IAiCollectionsGateway
    {
        AiCollectSummary Summary(AiUserContext user);
        AiCollectPage List(AiCollectFilter filter, bool refresh, AiUserContext user);
        AiCollectDetail GetCustomer(long customerId, AiUserContext user);
        long AddAction(AiCollectActionInput input, AiUserContext user);
        void CloseAction(long actionId, bool cancel, AiUserContext user);
        Task<AiCollectDraft> DraftAsync(long customerId, string language, bool useModel, bool regenerate, AiUserContext user, CancellationToken ct);
        long LogCopied(long customerId, string text, string source, string language, AiUserContext user);
        AiCollectSendPreview PreviewSend(long customerId, int channel, AiUserContext user);
        AiCollectSendResult Send(long customerId, int channel, string text, string source, string language, bool confirmed, AiUserContext user);
        AiCollectSettings GetSettings(AiUserContext admin);
        void SaveSettings(AiCollectSettings settings, AiUserContext admin);
    }
}
