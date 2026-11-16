using System;
using System.Collections.Generic;

namespace AccountModelsLib
{
    /// <summary>P7.4: نوع الحكم على حالة التقييم (حكم آلى ثابت بلا نموذج حكم).</summary>
    public static class AiEvalCheckTypes
    {
        /// <summary>أى dataset أنتجه المساعد يحتوى صفوف الاستعلام المرجعى (أو الرقم الواحد فى نص الرد).</summary>
        public const string Result = "result";
        /// <summary>نجحت أداة من ExpectedTools ولم تُستدعَ أداة من ForbiddenTools.</summary>
        public const string Tool = "tool";
        /// <summary>استُدعيت الأداة المالية المتوقعة وبياناتها = استدعاؤها المباشر بـ ReferenceArgsJson.</summary>
        public const string Financial = "financial";
        /// <summary>سأل المستخدم (ask_user)، أو لم ينفّذ استعلام بيانات وانتهى رده بسؤال.</summary>
        public const string Ask = "ask";
        /// <summary>لا أداة مسودة ولا استعلام بيانات ناجح (+ ExpectedText اختيارى).</summary>
        public const string Refuse = "refuse";

        public static readonly string[] All = { Result, Tool, Financial, Ask, Refuse };
    }

    public static class AiEvalCategories
    {
        public const string Sales = "sales", Purchases = "purchases", Stock = "stock", Accounts = "accounts", Parties = "parties",
            Screens = "screens", Ambiguous = "ambiguous", Refuse = "refuse";

        public static readonly string[] All = { Sales, Purchases, Stock, Accounts, Parties, Screens, Ambiguous, Refuse };
    }

    public static class AiEvalOutcomes
    {
        public const string Pass = "pass";
        public const string Fail = "fail";
        /// <summary>خطأ من المزوّد/النموذج (لا رد).</summary>
        public const string Error = "error";
        /// <summary>المرجع نفسه تعذّر حسابه على هذه القاعدة (لا يُحسب على النموذج).</summary>
        public const string Skipped = "skipped";
    }

    public static class AiEvalRunStatus
    {
        public const string Running = "Running";
        public const string Done = "Done";
        public const string Cancelled = "Cancelled";
        /// <summary>انقطع (أُعيد تشغيل الخادم أثناءه).</summary>
        public const string Interrupted = "Interrupted";
        /// <summary>توقف عند حد التكلفة EvalMaxCostPerRun.</summary>
        public const string CostCap = "CostCap";
        public const string Failed = "Failed";
    }

    /// <summary>صف AI_EvalCases.</summary>
    public class AiEvalCaseRow
    {
        public long Id { get; set; }
        public string CaseKey { get; set; } = "";
        public string Question { get; set; } = "";
        public string Category { get; set; } = "";
        /// <summary>msa = فصحى، eg = عامية مصرية.</summary>
        public string Dialect { get; set; } = "msa";
        public string CheckType { get; set; } = AiEvalCheckTypes.Result;
        public string ExpectedTools { get; set; }
        public string ForbiddenTools { get; set; }
        public string ReferenceSql { get; set; }
        public string ReferenceArgsJson { get; set; }
        public string ExpectedText { get; set; }
        public string Tags { get; set; }
        public string Notes { get; set; }
        public string Source { get; set; } = "seed";
        public long? Source_Ref_Id { get; set; }
        public int Version { get; set; } = 1;
        public bool IsEdited { get; set; }
        public bool IsActive { get; set; } = true;
    }

    /// <summary>صف AI_EvalRuns (UserName للعرض فقط).</summary>
    public class AiEvalRunRow
    {
        public long Id { get; set; }
        public long Group_Id { get; set; }
        public long User_Id { get; set; }
        public string UserName { get; set; } = "";
        public long Model_Id { get; set; }
        public string ModelName { get; set; } = "";
        public string ProviderName { get; set; } = "";
        public string Effort { get; set; }
        public int? MaxTokens { get; set; }
        public string Status { get; set; } = AiEvalRunStatus.Running;
        public string FilterJson { get; set; }
        public string ConfigJson { get; set; }
        public string PromptHash { get; set; }
        public int TotalCases { get; set; }
        public int DoneCases { get; set; }
        public int PassedCases { get; set; }
        public int FailedCases { get; set; }
        public int ErrorCases { get; set; }
        public int ToolOkCases { get; set; }
        public long? InputTokens { get; set; }
        public long? OutputTokens { get; set; }
        public decimal? CostEstimate { get; set; }
        public long? DurationMs { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public string ErrorText { get; set; }

        /// <summary>الحالات المتخطاة (المرجع غير متاح) لا تدخل فى النسبة.</summary>
        public int SkippedCases => Math.Max(0, DoneCases - PassedCases - FailedCases - ErrorCases);
        /// <summary>نسبة النجاح من الحالات المحكومة (بلا المتخطاة)، أو null إن لم تُحكم حالة بعد.</summary>
        public decimal? PassRate
        {
            get
            {
                int judged = PassedCases + FailedCases + ErrorCases;
                return judged == 0 ? (decimal?)null : Math.Round(PassedCases * 100m / judged, 1);
            }
        }
    }

    /// <summary>صف AI_EvalResults (Question/Category للعرض من الحالة).</summary>
    public class AiEvalResultRow
    {
        public long Id { get; set; }
        public long Run_Id { get; set; }
        public long Case_Id { get; set; }
        public string CaseKey { get; set; } = "";
        public int CaseVersion { get; set; }
        public string Outcome { get; set; } = "";
        public bool ToolOk { get; set; }
        public string FailReason { get; set; }
        public string AnswerText { get; set; }
        public string ToolsJson { get; set; }
        public int? Rounds { get; set; }
        public int? InputTokens { get; set; }
        public int? OutputTokens { get; set; }
        public decimal? CostEstimate { get; set; }
        public int? DurationMs { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string Question { get; set; } = "";
        public string Category { get; set; } = "";
        public string CheckType { get; set; } = "";
        /// <summary>بقايا ب3: متوسط درجات حكم النموذج (1–5) وسببه؛ null = لم يُحكَم.</summary>
        public decimal? JudgeScore { get; set; }
        public string JudgeReason { get; set; }
    }

    /// <summary>P7.4: أى الحالات تدخل التشغيل. الفارغ = الكل (المفعّلة فقط دائماً).</summary>
    public class AiEvalFilter
    {
        public List<string> Categories { get; set; } = new List<string>();
        public List<string> CheckTypes { get; set; } = new List<string>();
        /// <summary>msa / eg / فارغ = الاثنان.</summary>
        public string Dialect { get; set; } = "";
        public List<long> CaseIds { get; set; } = new List<long>();
        /// <summary>أول N حالة بعد الفلترة (null = الكل حتى حد EvalMaxCasesPerRun).</summary>
        public int? Limit { get; set; }
    }

    /// <summary>P7.4: طلب تشغيل تقييم لنموذج أو نموذجين على نفس الحالات.</summary>
    public class AiEvalStartRequest
    {
        public List<long> ModelIds { get; set; } = new List<long>();
        public AiEvalFilter Filter { get; set; } = new AiEvalFilter();
        /// <summary>فارغ = إعداد ربط «محادثة» لنفس النموذج إن وُجد.</summary>
        public string Effort { get; set; }
        public int? MaxTokens { get; set; }
        /// <summary>P7.7: عدد الأمثلة التلقائية لهذا التشغيل؛ null = الإعداد الحالى KnowledgeAutoExamples، 0 = بدونها.</summary>
        public int? AutoExamples { get; set; }
        /// <summary>وافق المدير على العدد والتكلفة التقديرية (مربع الموافقة). بدونه يُرفض التشغيل.</summary>
        public bool Confirmed { get; set; }
    }

    public class AiEvalModelEstimate
    {
        public long ModelId { get; set; }
        public string ModelName { get; set; } = "";
        public long InputTokens { get; set; }
        public long OutputTokens { get; set; }
        /// <summary>null = سعر النموذج غير مسجّل.</summary>
        public decimal? Cost { get; set; }
        /// <summary>history = من متوسط تشغيلات سابقة لنفس النموذج، default = تقدير ثابت.</summary>
        public string Basis { get; set; } = "default";
    }

    /// <summary>P7.4: ما يُعرض فى نافذة الموافقة قبل التشغيل.</summary>
    public class AiEvalEstimate
    {
        public int CaseCount { get; set; }
        public int MaxCases { get; set; }
        /// <summary>0 = بلا حد.</summary>
        public decimal MaxCostPerRun { get; set; }
        public List<AiEvalModelEstimate> Models { get; set; } = new List<AiEvalModelEstimate>();
        public decimal? TotalCost { get; set; }
        /// <summary>سبب منع التشغيل (فارغ = مسموح).</summary>
        public string Blocked { get; set; } = "";
        /// <summary>P7.7: عدد الأمثلة التلقائية الفعلى لهذا التشغيل.</summary>
        public int AutoExamples { get; set; }
    }

    /// <summary>
    /// P7.7: دليل التقييم على فائدة الأمثلة التلقائية: أحدث تشغيل مكتمل بها (بنفس العدد) وأحدث تشغيل مكتمل بدونها لنفس النموذج،
    /// مقارنةً على الحالات المشتركة (نفس المفتاح والإصدار). التفعيل بلا تحسّن يحتاج تأكيداً صريحاً من المدير.
    /// </summary>
    public class AiAutoExamplesStatus
    {
        /// <summary>الإعداد الحالى (0 = موقوفة).</summary>
        public int Current { get; set; }
        /// <summary>العدد الذى حُسب له الدليل.</summary>
        public int Count { get; set; }
        /// <summary>أمثلة «المعرفة» المفعّلة (مصدر الأمثلة التلقائية).</summary>
        public int ActiveExamples { get; set; }
        public bool HasEvidence { get; set; }
        public bool Improved { get; set; }
        public string ModelName { get; set; } = "";
        public long? RunOnId { get; set; }
        public long? RunOffId { get; set; }
        public DateTime? OnAt { get; set; }
        public DateTime? OffAt { get; set; }
        public int CommonCases { get; set; }
        public int PassOn { get; set; }
        public int PassOff { get; set; }
        /// <summary>أقل عدد حالات مشتركة ليُعتد بالمقارنة.</summary>
        public const int MinCommonCases = 10;
    }

    public static class AiEvalConfig
    {
        /// <summary>P7.7: knowledge_auto_examples من ConfigJson لتشغيل (0 إن غاب أو تعذّرت القراءة).</summary>
        public static int AutoExamplesOf(string configJson)
        {
            if (string.IsNullOrWhiteSpace(configJson)) return 0;
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(configJson);
                return doc.RootElement.TryGetProperty("knowledge_auto_examples", out var v) && v.ValueKind == System.Text.Json.JsonValueKind.Number && v.TryGetInt32(out int n) ? n : 0;
            }
            catch (System.Text.Json.JsonException) { return 0; }
        }
    }

    public class AiEvalStartResult
    {
        public long GroupId { get; set; }
        public List<long> RunIds { get; set; } = new List<long>();
        public int CaseCount { get; set; }
    }

    /// <summary>P7.4: حالة تشغيل جماعى (للمتابعة كل ثانيتين).</summary>
    public class AiEvalProgress
    {
        public long GroupId { get; set; }
        public bool Active { get; set; }
        public string CurrentCase { get; set; } = "";
        public List<AiEvalRunRow> Runs { get; set; } = new List<AiEvalRunRow>();
    }

    /// <summary>P7.4: إضافة/تعديل حالة تقييم يدوياً (تبويب «التقييم» فى P7.5، أو من الملاحظات فى P7.6).</summary>
    public class AiEvalCaseSaveRequest
    {
        public long Id { get; set; }
        public string Question { get; set; } = "";
        public string Category { get; set; } = AiEvalCategories.Sales;
        public string Dialect { get; set; } = "msa";
        public string CheckType { get; set; } = AiEvalCheckTypes.Result;
        public string ExpectedTools { get; set; } = "";
        public string ForbiddenTools { get; set; } = "";
        public string ReferenceSql { get; set; } = "";
        public string ReferenceArgsJson { get; set; } = "";
        public string ExpectedText { get; set; } = "";
        public string Tags { get; set; } = "";
        public string Notes { get; set; } = "";
        /// <summary>manual / feedback.</summary>
        public string Source { get; set; } = "manual";
        public long? Source_Ref_Id { get; set; }
    }

    /// <summary>
    /// P7.4: محرك التقييم (بلا واجهة فى P7.4؛ تبويب «التقييم» فى P7.5). كل الكتابة والتشغيل للمدير (صلاحية شاشة /settings/ai) فقط،
    /// والتشغيل يتطلب Confirmed. يرمى InvalidOperationException برسالة عربية عند الرفض.
    /// </summary>
    public interface IAiEvalGateway
    {
        List<AiEvalCaseRow> GetCases();
        long SaveCase(AiEvalCaseSaveRequest request, long userId);
        void SetCaseActive(long id, bool active, long userId);

        AiEvalEstimate Estimate(AiEvalStartRequest request, AiUserContext user);
        AiEvalStartResult Start(AiEvalStartRequest request, AiUserContext user);
        void Cancel(long groupId, long userId);
        /// <summary>التشغيل الجارى الآن (واحد فقط فى المرة) أو null.</summary>
        long? ActiveGroupId();
        AiEvalProgress GetProgress(long groupId);
        List<AiEvalRunRow> GetRuns(int top = 100);
        List<AiEvalResultRow> GetResults(long runId);

        // P7.7: الأمثلة التلقائية (موقوفة افتراضياً؛ تُفعَّل بدليل من التقييم)
        /// <summary>count = العدد المراد تقييمه؛ null = الإعداد الحالى (أو 3 إن كان موقوفاً).</summary>
        AiAutoExamplesStatus GetAutoExamplesStatus(int? count = null);
        /// <summary>0 = إيقاف (مسموح دائماً). التفعيل بلا دليل تحسّن يُرفض إلا مع overrideEvidence.</summary>
        void SetAutoExamples(int count, bool overrideEvidence, long userId);

        // بقايا ب2، ب3: مقاطع الشاشات فى المعرفة وحكم النموذج (موقوفان افتراضياً؛ للمدير)
        bool GetFlag(string key);
        void SetFlag(string key, bool on, long userId);
    }
}
