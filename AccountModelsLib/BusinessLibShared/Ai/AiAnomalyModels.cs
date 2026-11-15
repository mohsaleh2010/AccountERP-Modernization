using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AccountModelsLib
{
    // ───────────── P8.4: كشف الشذوذ فى القيود (.claude/docs/ai-assistant-p84-plan.md) ─────────────
    // أسلوب net48 / C# 7.3: بلا record ولا init ولا required ولا nullable annotations.

    public static class AiAnomalyRuleCodes
    {
        public const string AmountOutlier = "amount_outlier";
        public const string Duplicate = "duplicate";
        public const string OffHours = "off_hours";
        public const string RareAccount = "rare_account";
        public const string RoundLarge = "round_large";

        public static readonly string[] All = { AmountOutlier, Duplicate, OffHours, RareAccount, RoundLarge };
        /// <summary>القواعد المقصورة على القيود اليدوية ما دام AnomalyManualOnly = 1 (ش8).</summary>
        public static readonly string[] ManualOnly = { Duplicate, OffHours, RareAccount, RoundLarge };

        public static bool IsKnown(string code) => All.Contains(code ?? "", StringComparer.Ordinal);
    }

    public static class AiAnomalySeverity
    {
        public const string High = "high";
        public const string Medium = "medium";
        public const string Info = "info";

        public static int Rank(string s) => s == High ? 3 : s == Medium ? 2 : s == Info ? 1 : 0;
        public static string FromRank(int r) => r >= 3 ? High : r == 2 ? Medium : Info;
    }

    public static class AiAnomalyStatus
    {
        public const string New = "New";
        public const string Reviewed = "Reviewed";
        public const string Dismissed = "Dismissed";

        public static readonly string[] All = { New, Reviewed, Dismissed };
        public static bool IsKnown(string s) => All.Contains(s ?? "", StringComparer.Ordinal);
    }

    public static class AiAnomalyRunStatus
    {
        public const string Queued = "Queued";
        public const string Running = "Running";
        public const string Done = "Done";
        public const string Failed = "Failed";
    }

    public static class AiAnomalyTrigger
    {
        public const string Schedule = "schedule";
        public const string Manual = "manual";
        public const string Backfill = "backfill";
        public const string Test = "test";
    }

    /// <summary>سبب قاعدة off_hours (يُخزَّن فى DetailsJson).</summary>
    public static class AiAnomalyOffReason
    {
        public const string Night = "night";
        public const string Weekend = "weekend";
        public const string Holiday = "holiday";
    }

    public static class AiAnomalyConst
    {
        public const string Route = "/accounts/entries-anomalies";
        public const string MenuKey = "menuItemEntriesAnomalies";
        public const string EntryRoute = "/accounts/journal-entry/madin-da2n";
        public const string EntriesListRoute = "/accounts/journal-entries";
        /// <summary>نافذة المسح تنتهى عند الآن ناقص هذه الدقائق (قيد يُدرج رأسه ثم سطوره — ش29).</summary>
        public const int SettleMinutes = 10;
        public const int StaleMinutes = 30;
        public const int FailedRetryMinutes = 15;
        public const int RunTimeoutMinutes = 10;
        public const int MaxBackfillDays = 92;
        public const int PageSize = 50;
        public const int ExplainMaxTokens = 500;
        public const string ExplainAuditTool = "anomaly_explain";
        public const string ScanAuditTool = "anomaly_scan";
        /// <summary>محفوظ للتوافق: لا عمود لتاريخ إنشاء الحساب فى Submain_Account (فحص 1)، فالماسح يمرّر null دائماً.</summary>
        public const int AccountMinAgeDays = 90;
        /// <summary>تسامح مطابقة المبالغ فى قاعدة التكرار.</summary>
        public const decimal LineTolerance = 0.005m;
        public const string ActivatedAtFormat = "yyyy-MM-dd HH:mm:ss";
    }

    /// <summary>إعدادات الكشف من AI_Settings. From يعيد القيم الفاسدة للافتراضى؛ Validate يحكم على ما ترسله الواجهة.</summary>
    public class AiAnomalySettings
    {
        public bool Enabled { get; set; }
        public DateTime? ActivatedAt { get; set; }
        public TimeSpan ScanTime { get; set; } = new TimeSpan(2, 0, 0);
        public List<string> Rules { get; set; } = new List<string>(AiAnomalyRuleCodes.All);
        public bool ManualOnly { get; set; } = true;
        public bool BaselineHistory { get; set; } = true;
        public int BaselineDays { get; set; } = 365;
        public decimal ZThreshold { get; set; } = 3;
        public int MinSample { get; set; } = 30;
        public decimal MinAmount { get; set; } = 1000;
        public decimal RoundUnit { get; set; } = 1000;
        public decimal RoundMinAmount { get; set; } = 50000;
        public TimeSpan WorkStart { get; set; } = new TimeSpan(8, 0, 0);
        public TimeSpan WorkEnd { get; set; } = new TimeSpan(22, 0, 0);
        /// <summary>ترقيم DayOfWeek (0 = الأحد … 6 = السبت). الافتراضى السبت–الخميس.</summary>
        public List<int> WorkDays { get; set; } = new List<int> { 0, 1, 2, 3, 4, 6 };
        public List<DateTime> Holidays { get; set; } = new List<DateTime>();
        public int RareMaxUses { get; set; } = 2;
        public int DupDays { get; set; } = 7;
        public int MaxPerRun { get; set; } = 500;
        public bool Explain { get; set; } = true;
        public int ExplainMaxPerDay { get; set; } = 50;

        public bool RuleOn(string code) => Rules != null && Rules.Contains(code ?? "", StringComparer.Ordinal);

        /// <summary>هل القاعدة مقصورة على اليدوى فى هذه الإعدادات.</summary>
        public bool ManualScope(string code) => ManualOnly && AiAnomalyRuleCodes.ManualOnly.Contains(code ?? "", StringComparer.Ordinal);

        public static AiAnomalySettings From(IReadOnlyDictionary<string, string> s)
        {
            string V(string k) => s != null && s.TryGetValue(k, out var v) ? (v ?? "").Trim() : "";
            bool Has(string k) => s != null && s.ContainsKey(k);
            var r = new AiAnomalySettings();
            r.Enabled = V(AiSettingKeys.AnomalyEnabled) == "1";
            r.ActivatedAt = ParseActivatedAt(V(AiSettingKeys.AnomalyActivatedAt));
            if (AiBriefSettings.TryParseTime(V(AiSettingKeys.AnomalyScanTime), out var scan)) r.ScanTime = scan;
            if (Has(AiSettingKeys.AnomalyRules))
                r.Rules = AiBriefCsv.Strings(V(AiSettingKeys.AnomalyRules)).Where(AiAnomalyRuleCodes.IsKnown).ToList();
            r.ManualOnly = V(AiSettingKeys.AnomalyManualOnly) != "0";
            r.BaselineHistory = V(AiSettingKeys.AnomalyBaselineHistory) != "0";
            r.BaselineDays = (int)Dec(V(AiSettingKeys.AnomalyBaselineDays), 365, 30, 1095);
            r.ZThreshold = Dec(V(AiSettingKeys.AnomalyZThreshold), 3, 2, 10);
            r.MinSample = (int)Dec(V(AiSettingKeys.AnomalyMinSample), 30, 10, 1000);
            r.MinAmount = Dec(V(AiSettingKeys.AnomalyMinAmount), 1000, 0, 1000000000000m);
            r.RoundUnit = Dec(V(AiSettingKeys.AnomalyRoundUnit), 1000, 1, 1000000000m);
            r.RoundMinAmount = Dec(V(AiSettingKeys.AnomalyRoundMinAmount), 50000, 0, 1000000000000m);
            if (AiBriefSettings.TryParseTime(V(AiSettingKeys.AnomalyWorkStart), out var ws)) r.WorkStart = ws;
            if (AiBriefSettings.TryParseTime(V(AiSettingKeys.AnomalyWorkEnd), out var we)) r.WorkEnd = we;
            if (r.WorkStart == r.WorkEnd) { r.WorkStart = new TimeSpan(8, 0, 0); r.WorkEnd = new TimeSpan(22, 0, 0); }
            string days = V(AiSettingKeys.AnomalyWorkDays);
            if (Has(AiSettingKeys.AnomalyWorkDays))
            {
                var parsed = AiBriefCsv.Longs(days).Where(d => d >= 0 && d <= 6).Select(d => (int)d).Distinct().OrderBy(d => d).ToList();
                if (parsed.Count > 0) r.WorkDays = parsed;
            }
            r.Holidays = AiAnomalyRules.ParseHolidays(V(AiSettingKeys.AnomalyHolidays));
            r.RareMaxUses = (int)Dec(V(AiSettingKeys.AnomalyRareMaxUses), 2, 0, 100);
            r.DupDays = (int)Dec(V(AiSettingKeys.AnomalyDupDays), 7, 1, 60);
            r.MaxPerRun = (int)Dec(V(AiSettingKeys.AnomalyMaxPerRun), 500, 1, 5000);
            r.Explain = V(AiSettingKeys.AnomalyExplain) != "0";
            r.ExplainMaxPerDay = (int)Dec(V(AiSettingKeys.AnomalyExplainMaxPerDay), 50, 0, 1000);
            return r;
        }

        /// <summary>مفاتيح أخطاء AIJ_Set_Err* (فارغة = صالح). لا يحكم على ActivatedAt (لا يُقبل من الواجهة أصلاً).</summary>
        public List<string> Validate()
        {
            var errors = new List<string>();
            void Add(string key) { if (!errors.Contains(key)) errors.Add(key); }
            bool DayTime(TimeSpan t) => t >= TimeSpan.Zero && t < TimeSpan.FromDays(1);
            if (!DayTime(ScanTime) || !DayTime(WorkStart) || !DayTime(WorkEnd)) Add(AiAnomalyErrors.Time);
            if (WorkStart == WorkEnd) Add(AiAnomalyErrors.Work);
            if (WorkDays == null || !WorkDays.Any(d => d >= 0 && d <= 6)) Add(AiAnomalyErrors.Days);
            if (ZThreshold < 2 || ZThreshold > 10) Add(AiAnomalyErrors.Z);
            if (MinSample < 10 || MinSample > 1000) Add(AiAnomalyErrors.Sample);
            if (MinAmount < 0 || RoundMinAmount < 0) Add(AiAnomalyErrors.Amount);
            if (RoundUnit <= 0 || BaselineDays < 30 || BaselineDays > 1095 || RareMaxUses < 0 || RareMaxUses > 100
                || DupDays < 1 || DupDays > 60 || MaxPerRun < 1 || MaxPerRun > 5000 || ExplainMaxPerDay < 0 || ExplainMaxPerDay > 1000)
                Add(AiAnomalyErrors.Range);
            if (Rules != null && Rules.Any(r => !AiAnomalyRuleCodes.IsKnown(r))) Add(AiAnomalyErrors.Range);
            return errors;
        }

        /// <summary>القيم بصيغة التخزين (بما فيها ActivatedAt كما هى فى الكائن؛ البوابة تقرر قيمته).</summary>
        public IEnumerable<KeyValuePair<string, string>> ToPairs()
        {
            var c = CultureInfo.InvariantCulture;
            string T(TimeSpan t) => t.ToString(@"hh\:mm", c);
            yield return Pair(AiSettingKeys.AnomalyEnabled, Enabled ? "1" : "0");
            yield return Pair(AiSettingKeys.AnomalyActivatedAt, ActivatedAt.HasValue ? ActivatedAt.Value.ToString(AiAnomalyConst.ActivatedAtFormat, c) : "");
            yield return Pair(AiSettingKeys.AnomalyScanTime, T(ScanTime));
            yield return Pair(AiSettingKeys.AnomalyRules, string.Join(",", AiAnomalyRuleCodes.All.Where(RuleOn)));
            yield return Pair(AiSettingKeys.AnomalyManualOnly, ManualOnly ? "1" : "0");
            yield return Pair(AiSettingKeys.AnomalyBaselineHistory, BaselineHistory ? "1" : "0");
            yield return Pair(AiSettingKeys.AnomalyBaselineDays, BaselineDays.ToString(c));
            yield return Pair(AiSettingKeys.AnomalyZThreshold, ZThreshold.ToString(c));
            yield return Pair(AiSettingKeys.AnomalyMinSample, MinSample.ToString(c));
            yield return Pair(AiSettingKeys.AnomalyMinAmount, MinAmount.ToString(c));
            yield return Pair(AiSettingKeys.AnomalyRoundUnit, RoundUnit.ToString(c));
            yield return Pair(AiSettingKeys.AnomalyRoundMinAmount, RoundMinAmount.ToString(c));
            yield return Pair(AiSettingKeys.AnomalyWorkStart, T(WorkStart));
            yield return Pair(AiSettingKeys.AnomalyWorkEnd, T(WorkEnd));
            yield return Pair(AiSettingKeys.AnomalyWorkDays, string.Join(",", (WorkDays ?? new List<int>()).Distinct().OrderBy(d => d)));
            yield return Pair(AiSettingKeys.AnomalyHolidays, string.Join(",", (Holidays ?? new List<DateTime>()).Select(d => d.Date).Distinct().OrderBy(d => d).Select(d => d.ToString("yyyy-MM-dd", c))));
            yield return Pair(AiSettingKeys.AnomalyRareMaxUses, RareMaxUses.ToString(c));
            yield return Pair(AiSettingKeys.AnomalyDupDays, DupDays.ToString(c));
            yield return Pair(AiSettingKeys.AnomalyMaxPerRun, MaxPerRun.ToString(c));
            yield return Pair(AiSettingKeys.AnomalyExplain, Explain ? "1" : "0");
            yield return Pair(AiSettingKeys.AnomalyExplainMaxPerDay, ExplainMaxPerDay.ToString(c));
        }

        public AiAnomalySettings Clone()
        {
            var c = (AiAnomalySettings)MemberwiseClone();
            c.Rules = new List<string>(Rules ?? new List<string>());
            c.WorkDays = new List<int>(WorkDays ?? new List<int>());
            c.Holidays = new List<DateTime>(Holidays ?? new List<DateTime>());
            return c;
        }

        public static DateTime? ParseActivatedAt(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            DateTime d;
            if (DateTime.TryParseExact(s.Trim(), new[] { AiAnomalyConst.ActivatedAtFormat, "yyyy-MM-dd HH:mm", "yyyy-MM-ddTHH:mm:ss" },
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out d)) return d;
            return null;
        }

        private static KeyValuePair<string, string> Pair(string k, string v) => new KeyValuePair<string, string>(k, v);

        private static decimal Dec(string s, decimal fallback, decimal min, decimal max)
        {
            decimal d;
            return decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out d) && d >= min && d <= max ? d : fallback;
        }
    }

    /// <summary>صف AI_Anomalies = لقطة (قيد، قاعدة) وقت الكشف.</summary>
    public class AiAnomalyRow
    {
        public long Id { get; set; }
        public long Entry_Id { get; set; }
        public string RuleCode { get; set; } = "";
        public long? Line_Id { get; set; }
        public string Severity { get; set; } = AiAnomalySeverity.Medium;
        public decimal? Score { get; set; }
        public decimal? Amount { get; set; }
        public decimal? EntryTotal { get; set; }
        public long? Account_Id { get; set; }
        public string AccountName { get; set; }
        public long Branch_Id { get; set; }
        public long? EntryNo { get; set; }
        public DateTime? EntryDate { get; set; }
        public DateTime? EntryInsertedAt { get; set; }
        public int? JournalCode { get; set; }
        public int? ReferenceType { get; set; }
        public long? EntryUser_Id { get; set; }
        /// <summary>أرقام القاعدة محايدة لغوياً (§1).</summary>
        public string DetailsJson { get; set; }
        public long Run_Id { get; set; }
        public bool IsBackfill { get; set; }
        public DateTime? DetectedAt { get; set; }
        public string Status { get; set; } = AiAnomalyStatus.New;
        public long? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string ReviewNote { get; set; }
    }

    public class AiAnomalyRunRow
    {
        public long Id { get; set; }
        public string RunKey { get; set; } = "";
        public string TriggeredBy { get; set; } = AiAnomalyTrigger.Schedule;
        public DateTime? FromTs { get; set; }
        public DateTime? ToTs { get; set; }
        public string Status { get; set; } = AiAnomalyRunStatus.Running;
        public string RulesCsv { get; set; }
        public int EntriesScanned { get; set; }
        public int Found { get; set; }
        public int Inserted { get; set; }
        public bool Truncated { get; set; }
        public int? DurationMs { get; set; }
        public string ErrorText { get; set; }
        public long? RequestedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class AiAnomalyExplainRow
    {
        public long Id { get; set; }
        public long Entry_Id { get; set; }
        public string Language { get; set; } = "ar-EG";
        public string ExplainText { get; set; }
        /// <summary>model / rejected / error / capped / off (AiBriefNarrativeSource).</summary>
        public string Source { get; set; } = AiBriefNarrativeSource.Off;
        public long? Model_Id { get; set; }
        public string ModelName { get; set; }
        public int? InputTokens { get; set; }
        public int? OutputTokens { get; set; }
        public decimal? CostEstimate { get; set; }
        public string RejectReason { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>مفتاح سطر لمطابقة التكرار: الحساب والمدين والدائن بالمكافئ.</summary>
    public class AiAnomalyLineKey
    {
        public long AccountId { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }

        public AiAnomalyLineKey() { }
        public AiAnomalyLineKey(long accountId, decimal debit, decimal credit) { AccountId = accountId; Debit = debit; Credit = credit; }
    }

    /// <summary>قيد شاذ كما تعرضه الصفحة: صف لكل قيد بقواعده.</summary>
    public class AiAnomalyEntryView
    {
        public long EntryId { get; set; }
        public long? EntryNo { get; set; }
        public DateTime? EntryDate { get; set; }
        public DateTime? InsertedAt { get; set; }
        public int JournalCode { get; set; }
        public string JournalName { get; set; }
        public int ReferenceType { get; set; }
        public long BranchId { get; set; }
        public string BranchName { get; set; }
        public decimal EntryTotal { get; set; }
        public string Severity { get; set; } = AiAnomalySeverity.Info;
        public string Status { get; set; } = AiAnomalyStatus.New;
        public bool IsBackfill { get; set; }
        public DateTime DetectedAt { get; set; }
        public string ReviewNote { get; set; }
        public long? ReviewedBy { get; set; }
        public string ReviewedByName { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public List<AiAnomalyRow> Rules { get; set; } = new List<AiAnomalyRow>();
        public decimal? LiveTotal { get; set; }
        public bool Missing { get; set; }
        public bool Changed { get; set; }
        public bool CanOpen { get; set; }
    }

    public class AiAnomalyLineView
    {
        public long LineId { get; set; }
        public int? LineNo { get; set; }
        public long? AccountId { get; set; }
        public string AccountNo { get; set; }
        public string AccountName { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal DebitEquivalent { get; set; }
        public decimal CreditEquivalent { get; set; }
        public string Notes { get; set; }
        /// <summary>السطر المصاب بقاعدة على مستوى السطر.</summary>
        public bool Flagged { get; set; }
    }

    public class AiAnomalyEntryDetail
    {
        public AiAnomalyEntryView Entry { get; set; }
        public List<AiAnomalyLineView> Lines { get; set; } = new List<AiAnomalyLineView>();
        public string EntryNotes { get; set; }
        public AiAnomalyExplainRow Explain { get; set; }
    }

    public class AiAnomalyFilter
    {
        /// <summary>فارغ = الكل.</summary>
        public string Status { get; set; } = AiAnomalyStatus.New;
        public string RuleCode { get; set; }
        public string Severity { get; set; }
        /// <summary>0 = كل الفروع المسموحة.</summary>
        public long BranchId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public long? EntryNo { get; set; }
        public bool IncludeBackfill { get; set; }
        /// <summary>يبدأ من 0.</summary>
        public int Page { get; set; }
    }

    public class AiAnomalyPage
    {
        public List<AiAnomalyEntryView> Items { get; set; } = new List<AiAnomalyEntryView>();
        public int Total { get; set; }
    }

    public class AiAnomalySummary
    {
        public int NewCount { get; set; }
        public int HighCount { get; set; }
        public int ReviewedThisMonth { get; set; }
        public DateTime? LastRunAt { get; set; }
        public string LastRunStatus { get; set; }
        public bool Enabled { get; set; }
    }

    /// <summary>أعداد مجموعة anomalies فى الموجز (ش27).</summary>
    public class AiAnomalyBriefCounts
    {
        public int NewCount { get; set; }
        public int HighCount { get; set; }
        public int OpenTotal { get; set; }
        /// <summary>أعلى 5: رقم القيد واسم الحساب.</summary>
        public List<AiAnomalyTopItem> Top { get; set; } = new List<AiAnomalyTopItem>();
    }

    public class AiAnomalyTopItem
    {
        public long EntryId { get; set; }
        public long? EntryNo { get; set; }
        public string AccountName { get; set; }
        public string Severity { get; set; }
    }

    /// <summary>خطأ تحقق أو صلاحية من بوابة الشذوذ. Code = مفتاح resx (AIJ_*).</summary>
    public class AiAnomalyException : InvalidOperationException
    {
        public string Code { get; }
        public AiAnomalyException(string code, string message) : base(message) { Code = code ?? ""; }
    }

    public static class AiAnomalyErrors
    {
        public const string NotAdmin = "AIJ_Set_ErrNotAdmin";
        public const string NoPermission = "AIJ_ErrNoPermission";
        public const string NotFound = "AIJ_ErrNotFound";
        public const string Busy = "AIJ_ErrBusy";
        public const string Disabled = "AIJ_EmptyDisabled";
        public const string Time = "AIJ_Set_ErrTime";
        public const string Work = "AIJ_Set_ErrWork";
        public const string Days = "AIJ_Set_ErrDays";
        public const string Z = "AIJ_Set_ErrZ";
        public const string Sample = "AIJ_Set_ErrSample";
        public const string Amount = "AIJ_Set_ErrAmount";
        public const string Range = "AIJ_Set_ErrRange";
        public const string BackfillRange = "AIJ_Set_ErrBackfillRange";
    }

    /// <summary>
    /// بوابة شذوذ القيود (ينفّذها AccountERP.AI ويسجّلها مضيف الويب فقط؛ بدونها تعرض الصفحة «غير متاح» — ش31).
    /// العرض بصلاحية الصفحة وفروع المستخدم (ش21)؛ دوال الإدارة للمدير أو مالك /settings/ai.
    /// </summary>
    public interface IAiAnomalyGateway
    {
        AiAnomalySummary Summary(AiUserContext user);
        AiAnomalyPage List(AiAnomalyFilter filter, AiUserContext user);
        AiAnomalyEntryDetail GetEntry(long entryId, AiUserContext user);
        void SetStatus(IList<long> entryIds, string status, string note, AiUserContext user);
        Task<AiAnomalyExplainRow> ExplainAsync(long entryId, string language, bool regenerate, AiUserContext user, CancellationToken ct);

        AiAnomalySettings GetSettings(AiUserContext admin);
        void SaveSettings(AiAnomalySettings settings, AiUserContext admin);
        Task<AiAnomalyRunRow> RunNowAsync(AiUserContext admin, CancellationToken ct);
        long QueueBackfill(DateTime from, DateTime to, AiUserContext admin);
        List<AiAnomalyRunRow> ListRuns(AiUserContext admin, int top);
    }
}
