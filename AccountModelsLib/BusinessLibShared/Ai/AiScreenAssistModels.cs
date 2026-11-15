using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AccountModelsLib
{
    // ───────────── P8.9: المساعد السياقى داخل الشاشات (.claude/docs/ai-assistant-p89-plan.md) ─────────────
    // أسلوب net48 / C# 7.3: بلا record ولا init ولا required ولا nullable annotations.

    /// <summary>أنواع الشاشات المشمولة (ص4).</summary>
    public static class AiScreenKinds
    {
        public const string TrialBalance = "trial_balance";
        public const string CustomerStatement = "customer_statement";
        public const string SalesInvoice = "sales_invoice";
        public const string SalesReturn = "sales_return";

        public static readonly string[] All = { TrialBalance, CustomerStatement, SalesInvoice, SalesReturn };

        public static bool IsKnown(string kind) => All.Contains(kind ?? "", StringComparer.Ordinal);
    }

    /// <summary>الأسئلة لكل نوع.</summary>
    public static class AiScreenQuestions
    {
        public const string WhyUnbalanced = "why_unbalanced";
        public const string ExplainAccount = "explain_account";
        public const string ExplainBalance = "explain_balance";
        public const string ExplainInvoice = "explain_invoice";

        public static string[] For(string kind)
        {
            switch (kind)
            {
                case AiScreenKinds.TrialBalance: return new[] { WhyUnbalanced, ExplainAccount };
                case AiScreenKinds.CustomerStatement: return new[] { ExplainBalance };
                case AiScreenKinds.SalesInvoice:
                case AiScreenKinds.SalesReturn: return new[] { ExplainInvoice };
                default: return new string[0];
            }
        }
    }

    public static class AiScreenAssistConst
    {
        public const string FactsAuditTool = "screen_facts";
        public const string ExplainAuditTool = "screen_explain";
        public const string BlockType = "screen_facts";
        public const int MaxFactsChars = 6000;
        public const int ExplainMaxTokens = 500;
        public const int MaxTextChars = 2500;
        public const int TopEntries = 10;
        public const int TopRows = 5;
        public const int MaxMonths = 12;
        public const int FactsCacheMinutes = 5;
        public const int NarrativeCacheMinutes = 15;
        public const int CacheMaxEntries = 200;
        public const string CustomerToken = "[C1]";
        public const string MenuKey = "menuItemAiAssistant";
        public const string RTrialBalance = "/accounts/final/trial-balance";
        public const string RStatementNormal = "/accounts/customer-account-statement/normal";
        public const string RStatementPeriod = "/accounts/customer-account-statement/period";
        public const string RSalesList = "/accounts/invoices/list";
        public const string RInvoice = "/accounts/invoices/sales-invoice";
        public const string RReturn = "/accounts/invoices/sales-return";
        public const string RLedger = "/accounts/final/ledger";
        public const string RIntegrity = "/accounts/entries-integrity";
        public const string RJournalEntries = "/accounts/journal-entries";
        public const string REntryAnomalies = "/accounts/entries-anomalies";
        public const string RCollections = "/accounts/collections-priority";
        public const string RCustomerBalances = "/accounts/customer-balance-summary";
        public const string SettingsRoute = "/settings/ai";
        public const decimal Tolerance = 0.005m;
        public const decimal MatchTolerance = 0.01m;
    }

    public static class AiScreenSeverity
    {
        public const string Info = "info";
        public const string Warn = "warn";
        public const string High = "high";
    }

    /// <summary>
    /// محددات الشاشة فقط (ص2): لا أرقام ولا نص حر. الخادم يعيد الحساب بمحرك الشاشة وصلاحيات المستخدم.
    /// Attach = true ⇒ تُبنى الحقائق وتُرفق برسالة المحادثة (ص20)؛ false ⇒ سطر سياق محيط فقط (ص22).
    /// </summary>
    public class AiScreenPayload
    {
        public string Kind { get; set; } = "";
        public string Question { get; set; } = "";
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public long BranchId { get; set; }
        public long CurrencyId { get; set; }
        public string Filter { get; set; } = "";
        public string Mode { get; set; } = "";
        public long EntityId { get; set; }
        public bool Attach { get; set; }

        public AiScreenPayload Clone() => (AiScreenPayload)MemberwiseClone();

        /// <summary>بصمة الحمولة (مفتاح ذاكرة الحقائق). Attach لا يدخل فيها.</summary>
        public string Fingerprint()
        {
            var c = CultureInfo.InvariantCulture;
            string raw = string.Join("|", Kind ?? "", Question ?? "", From.HasValue ? From.Value.ToString("yyyy-MM-dd", c) : "-",
                To.HasValue ? To.Value.ToString("yyyy-MM-dd", c) : "-", BranchId.ToString(c), CurrencyId.ToString(c), Filter ?? "", Mode ?? "", EntityId.ToString(c));
            using (var sha = SHA256.Create())
                return string.Concat(sha.ComputeHash(Encoding.UTF8.GetBytes(raw)).Select(b => b.ToString("x2"))).Substring(0, 32);
        }
    }

    /// <summary>مؤشر يُعرض فى بطاقة (التسمية AIH_V_{Key}). Side = debit/credit أو فارغ.</summary>
    public class AiScreenValue
    {
        public string Key { get; set; } = "";
        public decimal? Number { get; set; }
        public string Text { get; set; }
        public DateTime? Date { get; set; }
        public string Side { get; set; }
        /// <summary>number (افتراضى) / int / pct / code (Text كود يُترجم).</summary>
        public string Format { get; set; } = "number";
    }

    /// <summary>نتيجة بقاعدة ثابتة (النص AIH_F_{Code} بالمتغيرات المسماة). القيم نصوص بثقافة ثابتة.</summary>
    public class AiScreenFinding
    {
        public string Code { get; set; } = "";
        public string Severity { get; set; } = AiScreenSeverity.Info;
        public Dictionary<string, string> Values { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>جدول معروض فى الحوار. Types: text/number/date/code/int (code = يُترجم فى الواجهة).</summary>
    public class AiScreenTable
    {
        public string Key { get; set; } = "";
        public string[] Columns { get; set; } = new string[0];
        public string[] Types { get; set; } = new string[0];
        public List<string[]> Rows { get; set; } = new List<string[]>();
    }

    public class AiScreenLink
    {
        public string Key { get; set; } = "";
        public string Url { get; set; } = "";
    }

    /// <summary>الحقائق المحسوبة على الخادم. RawJson (الشكل الكامل §2 بالأسماء) لا يغادر الخادم إلى الواجهة.</summary>
    public class AiScreenFacts
    {
        public string Kind { get; set; } = "";
        public string Question { get; set; } = "";
        public string Subject { get; set; } = "";
        public string SubjectCode { get; set; } = "";
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public long BranchId { get; set; }
        public string BranchName { get; set; } = "";
        public List<AiScreenValue> Values { get; set; } = new List<AiScreenValue>();
        public List<AiScreenFinding> Findings { get; set; } = new List<AiScreenFinding>();
        public List<AiScreenTable> Tables { get; set; } = new List<AiScreenTable>();
        public List<AiScreenLink> Links { get; set; } = new List<AiScreenLink>();
        public bool DetailHidden { get; set; }
        public bool TaxRestricted { get; set; }
        public bool StatementNote { get; set; }
        public DateTime ComputedAt { get; set; }
        public string Hash { get; set; } = "";
        public string RawJson { get; set; } = "";
        /// <summary>حمولة النموذج: العميل رمز [C1] دائماً، والحسابات والأصناف بأسمائها (ص19 بلا تقليل).</summary>
        public string PromptJson { get; set; } = "";
        /// <summary>حمولة النموذج مع تقليل البيانات: كل الأسماء رموز واليومية محذوفة.</summary>
        public string PromptJsonMin { get; set; } = "";
        /// <summary>كتلة المحادثة (ص21): بلا اسم العميل (customer_id بدله).</summary>
        public string ChatJson { get; set; } = "";
        /// <summary>أسماء تُستبدل برموز قبل النموذج (المفتاح = الرمز، القيمة = الاسم الحقيقى). لا تغادر الخادم.</summary>
        public Dictionary<string, string> Names { get; set; } = new Dictionary<string, string>();
        /// <summary>معرّف العميل (لكتلة المحادثة بدل الاسم — ص21).</summary>
        public long CustomerId { get; set; }

        /// <summary>نسخة للواجهة بلا الحقول الداخلية.</summary>
        public AiScreenFacts ForClient()
        {
            var c = (AiScreenFacts)MemberwiseClone();
            c.RawJson = ""; c.PromptJson = ""; c.PromptJsonMin = ""; c.ChatJson = "";
            c.Names = new Dictionary<string, string>();
            return c;
        }
    }

    public class AiScreenNarrative
    {
        public string Text { get; set; } = "";
        public string Source { get; set; } = AiBriefNarrativeSource.Off;
        public string ModelName { get; set; } = "";
        public string FactsHash { get; set; } = "";
        /// <summary>حين أُعيد بناء الحقائق (انتهت ذاكرتها): الحقائق الجديدة لتحدّث الواجهة.</summary>
        public AiScreenFacts Facts { get; set; }
    }

    /// <summary>حالة الميزة لمستخدم (للأزرار): مفعّلة ولها أنواع يملك مساراتها.</summary>
    public class AiScreenAssistState
    {
        public bool Enabled { get; set; }
        public bool Narrative { get; set; }
        public bool Ambient { get; set; }
        public List<string> Kinds { get; set; } = new List<string>();
    }

    public class AiScreenAssistSettings
    {
        public bool Enabled { get; set; } = true;
        public bool Narrative { get; set; } = true;
        public int MaxPerDay { get; set; } = 200;
        public bool Ambient { get; set; } = true;
        /// <summary>للعرض فقط: شروحات النموذج الناجحة اليوم.</summary>
        public int UsedToday { get; set; }

        public static AiScreenAssistSettings From(IReadOnlyDictionary<string, string> s)
        {
            string V(string k) { string v; return s != null && s.TryGetValue(k, out v) ? (v ?? "").Trim() : ""; }
            var r = new AiScreenAssistSettings();
            r.Enabled = V(AiSettingKeys.ScreenAssist) != "0";
            r.Narrative = V(AiSettingKeys.ScreenAssistNarrative) != "0";
            int m;
            r.MaxPerDay = int.TryParse(V(AiSettingKeys.ScreenAssistMaxPerDay), NumberStyles.Integer, CultureInfo.InvariantCulture, out m) && m >= 0 && m <= 5000 ? m : 200;
            r.Ambient = V(AiSettingKeys.ScreenContextAmbient) != "0";
            return r;
        }

        public List<string> Validate()
        {
            var e = new List<string>();
            if (MaxPerDay < 0 || MaxPerDay > 5000) e.Add(AiScreenAssistErrors.Max);
            return e;
        }

        public List<KeyValuePair<string, string>> ToPairs()
        {
            var c = CultureInfo.InvariantCulture;
            return new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(AiSettingKeys.ScreenAssist, Enabled ? "1" : "0"),
                new KeyValuePair<string, string>(AiSettingKeys.ScreenAssistNarrative, Narrative ? "1" : "0"),
                new KeyValuePair<string, string>(AiSettingKeys.ScreenAssistMaxPerDay, MaxPerDay.ToString(c)),
                new KeyValuePair<string, string>(AiSettingKeys.ScreenContextAmbient, Ambient ? "1" : "0"),
            };
        }
    }

    public class AiScreenAssistException : InvalidOperationException
    {
        public string Code { get; }
        public AiScreenAssistException(string code, string message) : base(message) { Code = code ?? ""; }
    }

    public static class AiScreenAssistErrors
    {
        public const string NoPermission = "AIH_Err_NoPermission";
        public const string Off = "AIH_Err_Off";
        public const string BadRequest = "AIH_Err_BadRequest";
        public const string Branch = "AIH_Err_Branch";
        public const string Period = "AIH_Err_Period";
        public const string NotVisible = "AIH_Err_NotVisible";
        public const string NotFound = "AIH_Err_NotFound";
        public const string Failed = "AIH_Err_Failed";
        public const string NotAdmin = "AIH_Set_ErrNotAdmin";
        public const string Max = "AIH_Set_ErrMax";
    }

    /// <summary>
    /// بوابة المساعد داخل الشاشات (ينفّذها AccountERP.AI ويسجّلها مضيف الويب فقط — ص27). قراءة فقط؛ الكتابة الوحيدة صفوف التدقيق.
    /// </summary>
    public interface IAiScreenAssistGateway
    {
        AiScreenAssistState State(AiUserContext user);
        AiScreenFacts Build(AiScreenPayload payload, AiUserContext user);
        Task<AiScreenNarrative> ExplainAsync(AiScreenPayload payload, string factsHash, string language, AiUserContext user, CancellationToken ct);
        AiScreenAssistSettings GetSettings(AiUserContext admin);
        void SaveSettings(AiScreenAssistSettings settings, AiUserContext admin);
    }
}
