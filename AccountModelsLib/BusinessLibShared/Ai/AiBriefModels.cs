using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AccountModelsLib
{
    // ───────────── P8.1: الموجز اليومى للمدير (.claude/docs/ai-assistant-p8-plan.md) ─────────────

    public static class AiBriefStatus
    {
        public const string Running = "Running";
        public const string Ready = "Ready";
        public const string Partial = "Partial";
        public const string Failed = "Failed";
    }

    /// <summary>أقسام الموجز (SectionKey). كل قسم يُحسب فقط إن ملك المستلم شاشته المانحة (ق11).</summary>
    public static class AiBriefSections
    {
        public const string Sales = "sales";
        public const string Collections = "collections";
        public const string Liquidity = "liquidity";
        public const string Receivables = "receivables";
        public const string Purchases = "purchases";
        public const string Alerts = "alerts";

        public static readonly string[] All = { Sales, Collections, Liquidity, Receivables, Purchases, Alerts };
        /// <summary>الافتراضى لاشتراك بلا اختيار: الآجل اختيارى والمشتريات موقوفة افتراضياً (§1).</summary>
        public static readonly string[] Default = { Sales, Collections, Liquidity, Alerts };

        public static bool IsKnown(string key) => All.Contains(key, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>مصدر نص السرد: model = سرد النموذج المقبول؛ غيره ⇒ الواجهة ترسم القالب من الحقائق.</summary>
    public static class AiBriefNarrativeSource
    {
        public const string Model = "model";
        public const string Rejected = "rejected";
        public const string Error = "error";
        public const string Capped = "capped";
        public const string Off = "off";
    }

    public static class AiBriefTrigger
    {
        public const string Schedule = "schedule";
        public const string Manual = "manual";
        public const string Test = "test";
    }

    public static class AiBriefConst
    {
        public const string Route = "/assistant/brief";
        public const string MenuKey = "menuItemAiDailyBrief";
        /// <summary>إصدار جامع الحقائق: يدخل فى ScopeKey، فرفعه بعد تغيير الحساب يُنتج صفوفاً جديدة بدل خلط القديم بالجديد.</summary>
        public const int CollectorVersion = 1;
        /// <summary>أقصى أيام تغطيها فترة موجز واحد بعد انقطاع (ق14).</summary>
        public const int MaxGapDays = 7;
        public const int HistoryDays = 60;
        public static readonly string[] Languages = { "ar-EG", "en-US", "de-DE", "fr-FR" };

        /// <summary>لغة معروفة أو العربية.</summary>
        public static string NormalizeLanguage(string lang)
        {
            if (string.IsNullOrWhiteSpace(lang)) return Languages[0];
            var hit = Languages.FirstOrDefault(l => l.Equals(lang.Trim(), StringComparison.OrdinalIgnoreCase))
                      ?? Languages.FirstOrDefault(l => l.StartsWith(lang.Trim().Split('-')[0] + "-", StringComparison.OrdinalIgnoreCase));
            return hit ?? Languages[0];
        }
    }

    public class AiBriefRow
    {
        public long Id { get; set; }
        public DateTime BriefDate { get; set; }
        public DateTime PeriodFrom { get; set; }
        public DateTime PeriodTo { get; set; }
        public string ScopeKey { get; set; } = "";
        public string BranchIdsCsv { get; set; }
        public string SectionsCsv { get; set; }
        public int TaxFilter { get; set; } = -1;
        public string FactsJson { get; set; }
        public string AlertsJson { get; set; }
        public int AlertCount { get; set; }
        public string ErrorsJson { get; set; }
        public string Status { get; set; } = AiBriefStatus.Running;
        public int CollectorVersion { get; set; } = AiBriefConst.CollectorVersion;
        public int? DurationMs { get; set; }
        public string TriggeredBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class AiBriefNarrativeRow
    {
        public long Id { get; set; }
        public long Brief_Id { get; set; }
        public string Language { get; set; } = "ar-EG";
        public string NarrativeText { get; set; }
        public string QuestionsJson { get; set; }
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

    /// <summary>تسليم موجز لمستلم، مع ملخص الموجز (للسجل والإشعار).</summary>
    public class AiBriefDeliveryRow
    {
        public long Id { get; set; }
        public long Brief_Id { get; set; }
        public long User_Id { get; set; }
        public string Language { get; set; } = "ar-EG";
        public DateTime? ReadAt { get; set; }
        public DateTime? HomeDismissedAt { get; set; }
        public long? Conversation_Id { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime BriefDate { get; set; }
        public DateTime PeriodFrom { get; set; }
        public DateTime PeriodTo { get; set; }
        public int AlertCount { get; set; }
        public string BriefStatus { get; set; } = "";
    }

    public class AiBriefSubscriptionRow
    {
        public long Id { get; set; }
        public long User_Id { get; set; }
        public string UserName { get; set; } = "";
        public bool UserActive { get; set; } = true;
        public bool IsEnabled { get; set; } = true;
        public string BranchIdsCsv { get; set; }
        public string SectionsCsv { get; set; }
        public string Language { get; set; } = "ar-EG";
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public List<long> BranchIds => AiBriefCsv.Longs(BranchIdsCsv);
        public List<string> Sections => AiBriefCsv.Strings(SectionsCsv).Where(AiBriefSections.IsKnown).ToList();
    }

    /// <summary>سطر فى «سجل آخر التشغيلات» (تبويب الإعدادات).</summary>
    public class AiBriefRunView
    {
        public long BriefId { get; set; }
        public DateTime BriefDate { get; set; }
        public DateTime PeriodFrom { get; set; }
        public DateTime PeriodTo { get; set; }
        public string Status { get; set; } = "";
        public string TriggeredBy { get; set; } = "";
        public int AlertCount { get; set; }
        public int? DurationMs { get; set; }
        public int Recipients { get; set; }
        public string NarrativeSources { get; set; } = "";
        public decimal NarrativeCost { get; set; }
        public string ErrorsJson { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    /// <summary>موجز كما تعرضه الصفحة لمستخدم: الصف + سرد لغته (إن وُجد) + تسليمه.</summary>
    public class AiBriefDetail
    {
        public AiBriefRow Brief { get; set; }
        public AiBriefNarrativeRow Narrative { get; set; }
        public AiBriefDeliveryRow Delivery { get; set; }
    }

    /// <summary>إعدادات الموجز من AI_Settings (القيم الفاسدة تعود للافتراضى).</summary>
    public class AiBriefSettings
    {
        public bool Enabled { get; set; }
        public TimeSpan Time { get; set; } = new TimeSpan(8, 0, 0);
        public string TimeZone { get; set; } = "";
        public List<int> WeekDays { get; set; } = new List<int> { 0, 1, 2, 3, 4, 6 };
        public bool Narrative { get; set; } = true;
        public decimal MaxCostPerMonth { get; set; }
        public int SalesDropPct { get; set; } = 30;
        public int ReturnsPct { get; set; } = 10;
        public decimal MinCashBalance { get; set; }
        public int OverdueDays { get; set; }
        public decimal LargePaymentFactor { get; set; } = 2;

        public static AiBriefSettings From(IReadOnlyDictionary<string, string> s)
        {
            string V(string k) => s != null && s.TryGetValue(k, out var v) ? (v ?? "").Trim() : "";
            var r = new AiBriefSettings();
            r.Enabled = V(AiSettingKeys.BriefEnabled) == "1";
            if (TryParseTime(V(AiSettingKeys.BriefTime), out var t)) r.Time = t;
            r.TimeZone = V(AiSettingKeys.BriefTimeZone);
            string days = V(AiSettingKeys.BriefWeekDays);
            if (days.Length > 0 || s?.ContainsKey(AiSettingKeys.BriefWeekDays) == true)
                r.WeekDays = AiBriefCsv.Longs(days).Where(d => d >= 0 && d <= 6).Select(d => (int)d).Distinct().OrderBy(d => d).ToList();
            r.Narrative = V(AiSettingKeys.BriefNarrative) != "0";
            r.MaxCostPerMonth = Dec(V(AiSettingKeys.BriefMaxCostPerMonth), 0, 0, 1_000_000);
            r.SalesDropPct = (int)Dec(V(AiSettingKeys.BriefSalesDropPct), 30, 1, 100);
            r.ReturnsPct = (int)Dec(V(AiSettingKeys.BriefReturnsPct), 10, 1, 100);
            r.MinCashBalance = Dec(V(AiSettingKeys.BriefMinCashBalance), 0, decimal.MinValue, decimal.MaxValue);
            r.OverdueDays = (int)Dec(V(AiSettingKeys.BriefOverdueDays), 0, 0, 3650);
            r.LargePaymentFactor = Dec(V(AiSettingKeys.BriefLargePaymentFactor), 2, 1, 100);
            return r;
        }

        /// <summary>القيم بصيغة التخزين (للحفظ بعد التحقق الخادمى).</summary>
        public IEnumerable<(string Key, string Value)> ToPairs()
        {
            var c = CultureInfo.InvariantCulture;
            yield return (AiSettingKeys.BriefEnabled, Enabled ? "1" : "0");
            yield return (AiSettingKeys.BriefTime, Time.ToString(@"hh\:mm", c));
            yield return (AiSettingKeys.BriefTimeZone, TimeZone ?? "");
            yield return (AiSettingKeys.BriefWeekDays, string.Join(",", (WeekDays ?? new List<int>()).Distinct().OrderBy(d => d)));
            yield return (AiSettingKeys.BriefNarrative, Narrative ? "1" : "0");
            yield return (AiSettingKeys.BriefMaxCostPerMonth, MaxCostPerMonth.ToString(c));
            yield return (AiSettingKeys.BriefSalesDropPct, SalesDropPct.ToString(c));
            yield return (AiSettingKeys.BriefReturnsPct, ReturnsPct.ToString(c));
            yield return (AiSettingKeys.BriefMinCashBalance, MinCashBalance.ToString(c));
            yield return (AiSettingKeys.BriefOverdueDays, OverdueDays.ToString(c));
            yield return (AiSettingKeys.BriefLargePaymentFactor, LargePaymentFactor.ToString(c));
        }

        /// <summary>HH:mm من 00:00 إلى 23:59.</summary>
        public static bool TryParseTime(string s, out TimeSpan t)
        {
            t = default;
            if (string.IsNullOrWhiteSpace(s)) return false;
            if (!TimeSpan.TryParseExact(s.Trim(), new[] { @"h\:mm", @"hh\:mm" }, CultureInfo.InvariantCulture, out var v)) return false;
            if (v < TimeSpan.Zero || v >= TimeSpan.FromDays(1)) return false;
            t = v;
            return true;
        }

        private static decimal Dec(string s, decimal fallback, decimal min, decimal max) =>
            decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out var d) && d >= min && d <= max ? d : fallback;
    }

    public static class AiBriefCsv
    {
        public static List<long> Longs(string csv) =>
            (csv ?? "").Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => long.TryParse(x.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out long v) ? (long?)v : null)
                .Where(v => v.HasValue).Select(v => v.Value).Distinct().ToList();

        public static List<string> Strings(string csv) =>
            (csv ?? "").Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim().ToLowerInvariant()).Where(x => x.Length > 0).Distinct().ToList();

        public static string Join(IEnumerable<long> ids) =>
            ids == null ? null : string.Join(",", ids.Distinct().OrderBy(x => x).Select(x => x.ToString(CultureInfo.InvariantCulture)));

        public static string Join(IEnumerable<string> keys) =>
            keys == null ? null : string.Join(",", keys.Where(k => !string.IsNullOrWhiteSpace(k)).Select(k => k.Trim().ToLowerInvariant()).Distinct().OrderBy(k => k, StringComparer.Ordinal));
    }

    /// <summary>
    /// بوابة الموجز (ينفّذها AccountERP.AI فى P8.1.4 ويسجّلها مضيف الويب فقط؛ بدونها لا تظهر الصفحة ولا البطاقة — ق28).
    /// كل قراءة لموجز تتحقق أن للمستخدم صف تسليم له. دوال المدير ترفض غير المدير.
    /// </summary>
    public interface IAiBriefGateway
    {
        AiBriefDetail GetLatest(AiUserContext user, string language);
        AiBriefDetail GetById(long briefId, AiUserContext user, string language);
        List<AiBriefDeliveryRow> ListHistory(AiUserContext user, int days = AiBriefConst.HistoryDays);
        int UnreadCount(long userId);
        void MarkRead(long briefId, long userId);
        void DismissHome(long briefId, long userId);
        AiBriefSubscriptionRow GetSubscription(AiUserContext user);
        void SaveSubscription(AiBriefSubscriptionRow subscription, AiUserContext user);
        Task<AiBriefDetail> RefreshNowAsync(AiUserContext user, string language, CancellationToken ct);
        long StartConversation(long briefId, AiUserContext user);

        AiBriefSettings GetSettings(AiUserContext admin);
        void SaveSettings(AiBriefSettings settings, AiUserContext admin);
        List<AiBriefSubscriptionRow> ListSubscribers(AiUserContext admin);
        void SetSubscriber(long userId, bool enabled, AiUserContext admin);
        Task<AiBriefDetail> RunNowForMeAsync(AiUserContext admin, string language, CancellationToken ct);
        List<AiBriefRunView> ListRuns(AiUserContext admin, int top = 30);
    }

    /// <summary>
    /// إشارة داخل العملية (ق2): يطلقها المشغّل بعد إدراج صف التسليم، وتشترك فيها خدمة الإشعار لكل دائرة متصلة.
    /// خطأ مشترك لا يمنع الباقين ولا يُسقط المشغّل. غير المتصل يرى الموجز عند أول دخول من الجدول.
    /// </summary>
    public static class AiBriefSignal
    {
        public static event Action<long, long> Raised;

        /// <summary>userId، deliveryId.</summary>
        public static void Raise(long userId, long deliveryId)
        {
            var handlers = Raised;
            if (handlers == null) return;
            foreach (Action<long, long> h in handlers.GetInvocationList())
            {
                try { h(userId, deliveryId); }
                catch (Exception ee) { try { operations.DataDictionaryLogerException(ee); } catch { } }
            }
        }
    }
}
