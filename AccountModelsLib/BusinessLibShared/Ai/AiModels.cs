using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AccountModelsLib
{
    /// <summary>أنواع المزوّدين المدعومة فى AI_Providers.ProviderType.</summary>
    public static class AiProviderTypes
    {
        public const string Anthropic = "anthropic";
        public const string OpenAI = "openai";
        public const string Gemini = "gemini";
        public const string Azure = "azure";
        public const string Ollama = "ollama";
        public const string LmStudio = "lmstudio";
        public const string OpenRouter = "openrouter";
        public const string Custom = "custom";

        public static readonly string[] BuiltIn = { Anthropic, OpenRouter, OpenAI, Gemini, Azure, Ollama, LmStudio };

        /// <summary>P0.1: المزوّدون الذين يتيحون ربطاً حقيقياً بتسجيل الدخول (OAuth PKCE بلا تسجيل تطبيق).</summary>
        public static bool SupportsSignIn(string type) => type == OpenRouter;

        /// <summary>صفحة إنشاء المفاتيح لدى المزوّد (الربط المُساعَد): يفتحها المستخدم ويسجّل دخوله وينشئ مفتاحاً ويلصقه.</summary>
        public static string KeysPageUrl(string type) => type switch
        {
            Anthropic => "https://console.anthropic.com/settings/keys",
            OpenAI => "https://platform.openai.com/api-keys",
            Gemini => "https://aistudio.google.com/app/apikey",
            Azure => "https://portal.azure.com/",
            OpenRouter => "https://openrouter.ai/settings/keys",
            _ => "",
        };
    }

    /// <summary>طريقة ربط المزوّد (AI_Providers.AuthMethod؛ NULL = مفتاح كالقديم).</summary>
    public static class AiAuthMethods
    {
        public const string ApiKey = "apikey";
        public const string SignIn = "signin";
    }

    /// <summary>بروتوكول المزوّد المخصص (AI_Providers.ApiProtocol).</summary>
    public static class AiApiProtocols
    {
        public const string OpenAI = "openai";
        public const string Anthropic = "anthropic";
        public const string Gemini = "gemini";
    }

    /// <summary>مفاتيح جدول AI_TaskRouting.TaskKey.</summary>
    public static class AiTaskKeys
    {
        public const string Chat = "chat";
        public const string Reasoning = "reasoning";
        public const string Vision = "vision";
        public const string Fast = "fast";
        public const string Embedding = "embedding";

        public static readonly string[] All = { Chat, Reasoning, Vision, Fast, Embedding };
    }

    /// <summary>مفاتيح جدول AI_Settings.</summary>
    public static class AiSettingKeys
    {
        public const string LocalOnly = "LocalOnly";
        public const string DataMinimization = "DataMinimization";
        public const string ChatRetentionDays = "ChatRetentionDays";
        public const string AttachmentRetentionDays = "AttachmentRetentionDays";
        public const string MaxToolRoundsPerMessage = "MaxToolRoundsPerMessage";
        public const string MaxUploadMb = "MaxUploadMb";
        /// <summary>P7.4: أقصى عدد حالات فى تشغيل تقييم واحد (لكل نموذج).</summary>
        public const string EvalMaxCasesPerRun = "EvalMaxCasesPerRun";
        /// <summary>P7.4: أقصى تكلفة تقديرية لتشغيل واحد (0 = بلا حد)؛ عند بلوغها يتوقف التشغيل بحالة CostCap.</summary>
        public const string EvalMaxCostPerRun = "EvalMaxCostPerRun";
        /// <summary>P7.7: عدد الأمثلة المعتمدة المشابهة التى تُلحق بسؤال المستخدم (للنموذج فقط). 0 = موقوفة (الافتراضى) حتى يثبت التقييم فائدتها.</summary>
        public const string KnowledgeAutoExamples = "KnowledgeAutoExamples";
        public const int MaxAutoExamples = 5;

        // ───── P8.1: الموجز اليومى (ai-assistant-p8-plan.md §4) ─────
        /// <summary>1 = الميزة مفعّلة (الافتراضى 0 حتى يفعّلها المدير).</summary>
        public const string BriefEnabled = "BriefEnabled";
        /// <summary>موعد التوليد HH:mm بتوقيت BriefTimeZone.</summary>
        public const string BriefTime = "BriefTime";
        /// <summary>معرّف TimeZoneInfo؛ فارغ = توقيت الخادم.</summary>
        public const string BriefTimeZone = "BriefTimeZone";
        /// <summary>أيام التوليد بترقيم DayOfWeek مفصولة بفاصلة (الافتراضى السبت–الخميس: 0,1,2,3,4,6).</summary>
        public const string BriefWeekDays = "BriefWeekDays";
        /// <summary>1 = سرد بالنموذج، 0 = القالب فقط.</summary>
        public const string BriefNarrative = "BriefNarrative";
        /// <summary>حد تكلفة السرد الشهرى (0 = بلا حد)؛ عند بلوغه يُستخدم القالب.</summary>
        public const string BriefMaxCostPerMonth = "BriefMaxCostPerMonth";
        public const string BriefSalesDropPct = "BriefSalesDropPct";
        public const string BriefReturnsPct = "BriefReturnsPct";
        public const string BriefMinCashBalance = "BriefMinCashBalance";
        public const string BriefOverdueDays = "BriefOverdueDays";
        public const string BriefLargePaymentFactor = "BriefLargePaymentFactor";

        // ───── P8.2: لوحة البحث السريع (ai-assistant-p82-plan.md س33) ─────
        /// <summary>1 = بحث البيانات (العملاء/الموردون/الأصناف/المستندات) فى لوحة Ctrl+K مفعّل (الافتراضى)؛ 0 = الشاشات فقط.</summary>
        public const string PaletteDataSearch = "PaletteDataSearch";

        // ───── P8.4: شذوذ القيود (ai-assistant-p84-plan.md §4) ─────
        public const string AnomalyEnabled = "AnomalyEnabled";
        /// <summary>لحظة آخر تفعيل (yyyy-MM-dd HH:mm:ss بتوقيت الخادم). المسح المجدول لا يفحص قيداً أُدخل قبلها (ش3، ش6).</summary>
        public const string AnomalyActivatedAt = "AnomalyActivatedAt";
        public const string AnomalyScanTime = "AnomalyScanTime";
        public const string AnomalyRules = "AnomalyRules";
        public const string AnomalyManualOnly = "AnomalyManualOnly";
        public const string AnomalyBaselineHistory = "AnomalyBaselineHistory";
        public const string AnomalyBaselineDays = "AnomalyBaselineDays";
        public const string AnomalyZThreshold = "AnomalyZThreshold";
        public const string AnomalyMinSample = "AnomalyMinSample";
        public const string AnomalyMinAmount = "AnomalyMinAmount";
        public const string AnomalyRoundUnit = "AnomalyRoundUnit";
        public const string AnomalyRoundMinAmount = "AnomalyRoundMinAmount";
        public const string AnomalyWorkStart = "AnomalyWorkStart";
        public const string AnomalyWorkEnd = "AnomalyWorkEnd";
        public const string AnomalyWorkDays = "AnomalyWorkDays";
        public const string AnomalyHolidays = "AnomalyHolidays";
        public const string AnomalyRareMaxUses = "AnomalyRareMaxUses";
        public const string AnomalyDupDays = "AnomalyDupDays";
        public const string AnomalyMaxPerRun = "AnomalyMaxPerRun";
        public const string AnomalyExplain = "AnomalyExplain";
        public const string AnomalyExplainMaxPerDay = "AnomalyExplainMaxPerDay";

        // ───── P8.5: أولويات التحصيل (ai-assistant-p85-plan.md §4) ─────
        public const string CollectTermsDays = "CollectTermsDays";
        public const string CollectMinBalance = "CollectMinBalance";
        public const string CollectInvoiceMonths = "CollectInvoiceMonths";
        public const string CollectMaxCustomers = "CollectMaxCustomers";
        public const string CollectWeights = "CollectWeights";
        public const string CollectAgeCapDays = "CollectAgeCapDays";
        public const string CollectAmountCap = "CollectAmountCap";
        public const string CollectDsoWindowDays = "CollectDsoWindowDays";
        public const string CollectDsoCapDays = "CollectDsoCapDays";
        public const string CollectIdleStartDays = "CollectIdleStartDays";
        public const string CollectIdleCapDays = "CollectIdleCapDays";
        public const string CollectHighScore = "CollectHighScore";
        public const string CollectMediumScore = "CollectMediumScore";
        public const string CollectLevel2Days = "CollectLevel2Days";
        public const string CollectLevel3Days = "CollectLevel3Days";
        public const string CollectBrokenPromiseBonus = "CollectBrokenPromiseBonus";
        public const string CollectBrokenPromiseDays = "CollectBrokenPromiseDays";
        public const string CollectCacheMinutes = "CollectCacheMinutes";
        public const string CollectDraft = "CollectDraft";
        public const string CollectDraftMaxPerDay = "CollectDraftMaxPerDay";
        public const string CollectSendEnabled = "CollectSendEnabled";
        public const string CollectReminderMinDays = "CollectReminderMinDays";
        public const string CollectBriefAlert = "CollectBriefAlert";
        public const string CollectBriefTop = "CollectBriefTop";
        // P8.9: المساعد داخل الشاشات (ص26).
        public const string ScreenAssist = "ScreenAssist";
        public const string ScreenAssistNarrative = "ScreenAssistNarrative";
        public const string ScreenAssistMaxPerDay = "ScreenAssistMaxPerDay";
        public const string ScreenContextAmbient = "ScreenContextAmbient";
    }

    /// <summary>صف AI_Providers كما يُخزَّن (المفتاح مشفّر). لا يصل للواجهة؛ الواجهة ترى AiProviderView.</summary>
    public class AiProviderRow
    {
        public long Id { get; set; }
        public string ProviderType { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string BaseUrl { get; set; }
        public string ApiKeyEnc { get; set; }
        public string ApiKeyHint { get; set; }
        public string ExtraJson { get; set; }
        public string ApiProtocol { get; set; }
        public string AuthHeaderName { get; set; }
        public string ExtraHeadersEnc { get; set; }
        public int TimeoutSeconds { get; set; } = 60;
        public bool IsCustom { get; set; }
        public bool IsEnabled { get; set; } = true;
        public bool IsLocal { get; set; }
        public DateTime? LastTestAt { get; set; }
        public bool? LastTestOk { get; set; }
        public string LastTestMessage { get; set; }
        public string AuthMethod { get; set; }
        public DateTime? SignInAt { get; set; }
        public long? SignInBy { get; set; }
    }

    /// <summary>المزوّد كما تراه الشاشة: المفتاح مقنّع فقط (••••1234) ولا تُعاد Headers الإضافية.</summary>
    public class AiProviderView
    {
        public long Id { get; set; }
        public string ProviderType { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string BaseUrl { get; set; } = "";
        public bool HasKey { get; set; }
        public string MaskedKey { get; set; } = "";
        public bool HasExtraHeaders { get; set; }
        public string ApiProtocol { get; set; } = AiApiProtocols.OpenAI;
        public string AuthHeaderName { get; set; } = "";
        public string AzureDeployment { get; set; } = "";
        public string AzureApiVersion { get; set; } = "";
        public int TimeoutSeconds { get; set; } = 60;
        public bool IsCustom { get; set; }
        public bool IsEnabled { get; set; } = true;
        public bool IsLocal { get; set; }
        public DateTime? LastTestAt { get; set; }
        public bool? LastTestOk { get; set; }
        public string LastTestMessage { get; set; } = "";
        public int ModelCount { get; set; }
        /// <summary>apikey / signin (فارغ = مفتاح).</summary>
        public string AuthMethod { get; set; } = "";
        public DateTime? SignInAt { get; set; }
    }

    /// <summary>طلب حفظ مزوّد. ApiKey/ExtraHeaders = null تعنى «بلا تغيير»، وClearKey تمسح المفتاح.</summary>
    public class AiProviderSaveRequest
    {
        public long Id { get; set; }
        public string ProviderType { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string BaseUrl { get; set; } = "";
        public string ApiKey { get; set; }
        public bool ClearKey { get; set; }
        public string ApiProtocol { get; set; } = AiApiProtocols.OpenAI;
        public string AuthHeaderName { get; set; } = "";
        /// <summary>سطر لكل Header بالشكل «الاسم: القيمة».</summary>
        public string ExtraHeaders { get; set; }
        public bool ClearExtraHeaders { get; set; }
        public string AzureDeployment { get; set; } = "";
        public string AzureApiVersion { get; set; } = "";
        public int TimeoutSeconds { get; set; } = 60;
        public bool IsEnabled { get; set; } = true;
        public bool IsLocal { get; set; }
    }

    public class AiModelRow
    {
        public long Id { get; set; }
        public long Provider_Id { get; set; }
        public string ModelId { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public bool SupportsTools { get; set; } = true;
        public bool SupportsVision { get; set; }
        public bool SupportsTemperature { get; set; }
        public bool SupportsEmbeddings { get; set; }
        public int? ContextTokens { get; set; }
        public decimal? InputPricePerM { get; set; }
        public decimal? OutputPricePerM { get; set; }
        public bool IsEnabled { get; set; } = true;
        /// <summary>للعرض فقط (اسم المزوّد).</summary>
        public string ProviderName { get; set; } = "";
    }

    public class AiTaskRoutingRow
    {
        public long Id { get; set; }
        public string TaskKey { get; set; } = AiTaskKeys.Chat;
        public long Model_Id { get; set; }
        public int Priority { get; set; } = 1;
        public string Effort { get; set; } = "";
        public int? MaxTokens { get; set; }
        public decimal? Temperature { get; set; }
    }

    /// <summary>نموذج أعاده المزوّد من endpoint النماذج (قبل حفظه فى AI_Models).</summary>
    public class AiRemoteModel
    {
        public string ModelId { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public int? ContextTokens { get; set; }
        public bool? SupportsVision { get; set; }
        public bool? SupportsEmbeddings { get; set; }
    }

    public class AiTestResult
    {
        public bool Ok { get; set; }
        public string Message { get; set; } = "";
        public long ElapsedMs { get; set; }
        public int ModelCount { get; set; }
    }

    /// <summary>بداية ربط بتسجيل الدخول: الرابط يُفتح فى تبويب جديد. Mode = callback (يرجع المزوّد تلقائياً) أو code (يلصق المستخدم الكود).</summary>
    public class AiSignInStart
    {
        public string Handle { get; set; } = "";
        public string AuthorizeUrl { get; set; } = "";
        public string Mode { get; set; } = "code";
        public DateTime ExpiresAtUtc { get; set; }
    }

    /// <summary>حالة الربط: pending / done / failed / expired.</summary>
    public class AiSignInStatus
    {
        public string State { get; set; } = "pending";
        public string Message { get; set; } = "";
        public long ProviderId { get; set; }
    }

    /// <summary>
    /// بوابة إدارة المزوّدين لشاشة /settings/ai. تُنفَّذ فى AccountERP.AI (تشفير المفاتيح والاتصال بالمزوّدين) ويسجّلها مضيف الويب فقط؛
    /// على مضيف بلا تسجيل (MAUI) تعرض الشاشة «غير متاح». كل الدوال ترمى استثناء برسالة عربية واضحة عند الرفض.
    /// </summary>
    public interface IAiAdminGateway
    {
        List<AiProviderView> GetProviders();
        long SaveProvider(AiProviderSaveRequest request, long userId);
        void DeleteProvider(long providerId);
        Task<AiTestResult> TestConnectionAsync(long providerId, CancellationToken ct = default);
        Task<List<AiRemoteModel>> FetchModelsAsync(long providerId, CancellationToken ct = default);

        List<AiModelRow> GetModels();
        long SaveModel(AiModelRow model, long userId);
        void DeleteModel(long modelId);

        List<AiTaskRoutingRow> GetRouting();
        void SaveRouting(List<AiTaskRoutingRow> rows, long userId);

        Dictionary<string, string> GetSettings();

        // P0.1: الربط بتسجيل الدخول لدى المزوّد
        /// <summary>origin = عنوان التطبيق كما يراه المتصفح (NavigationManager.BaseUri). يرفض من لا يملك صلاحية الشاشة.</summary>
        AiSignInStart StartSignIn(long providerId, string origin, long userId);
        AiSignInStatus GetSignInStatus(string handle, long userId);
        Task<AiSignInStatus> CompleteSignInWithCodeAsync(string handle, string code, long userId, CancellationToken ct = default);
        /// <summary>يمسح المفتاح المحفوظ محلياً (المفتاح البعيد يبقى فى حساب المزوّد).</summary>
        void Disconnect(long providerId, long userId);

        // P2: سجل تدقيق الأدوات وحالة اتصال القراءة
        List<AiToolAuditView> GetToolAudit(string outcome, string search, int top = 300);
        AiReadOnlyStatus GetReadOnlyStatus();

        // P7.2: فهرس البحث الدلالى للمعرفة
        AiKnowledgeIndexStatus GetKnowledgeIndexStatus();
        /// <summary>all=false: غير المفهرس بالنموذج الحالى فقط. يرمى InvalidOperationException برسالة عربية.</summary>
        Task<AiKnowledgeRebuildResult> RebuildKnowledgeIndexAsync(bool all, CancellationToken ct = default);

        // P7.3: تبويب «المعرفة»
        /// <summary>كل المقاطع (المفعّلة وغيرها) بلا المتجهات.</summary>
        List<AiKnowledgeRow> GetKnowledge();
        /// <summary>إضافة/تعديل مسرد أو قاعدة أو مثال (الـ Views للقراءة فقط). المثال يُتحقق منه كاستعلام قراءة. يعيد Id.</summary>
        long SaveKnowledge(AiKnowledgeSaveRequest request, long userId);
        void SetKnowledgeActive(long id, bool active, long userId);
        /// <summary>يعيد مقطعاً مزروعاً عدّله المدير إلى نصّه الأصلى فى الكود.</summary>
        void RestoreKnowledgeSeed(long id, long userId);
        /// <summary>«تجربة البحث» بصلاحية كاملة (كل الـ Views والشاشات).</summary>
        Task<AiKnowledgeSearchTest> TestKnowledgeSearchAsync(string query, CancellationToken ct = default);
    }

    /// <summary>P7.3: طلب حفظ مقطع معرفة من تبويب «المعرفة».</summary>
    public class AiKnowledgeSaveRequest
    {
        public long Id { get; set; }
        public string Kind { get; set; } = AiKnowledgeKinds.Glossary;
        public string TitleAr { get; set; } = "";
        public string Content { get; set; } = "";
        public string Keywords { get; set; } = "";
        /// <summary>P7.6: manual (الافتراضى) أو feedback (اعتماد رد من قائمة الملاحظات) — للإضافة فقط.</summary>
        public string Source { get; set; } = "";
        public long? Source_Ref_Id { get; set; }
    }

    /// <summary>P7.3: نتيجة «تجربة البحث»: Mode = keyword / hybrid / fallback.</summary>
    public class AiKnowledgeSearchTest
    {
        public string Mode { get; set; } = "keyword";
        public List<AiKnowledgeRow> Rows { get; set; } = new();
        /// <summary>نص الناتج كما يراه النموذج.</summary>
        public string Text { get; set; } = "";
        public long DurationMs { get; set; }
    }

    // ───────────── P1: المحادثة ─────────────

    /// <summary>حالة رسالة المساعد فى AI_Messages.Status.</summary>
    public static class AiMessageStatus
    {
        public const string Ok = "ok";
        public const string Stopped = "stopped";
        public const string Error = "error";
        /// <summary>P4: المساعد توقف على سؤال للمستخدم (ask_user) وينتظر الإجابة.</summary>
        public const string Awaiting = "awaiting";
    }

    /// <summary>P4: سؤال المساعد للمستخدم (بطاقة خيارات تحت الرد).</summary>
    public class AiAskView
    {
        public string Question { get; set; } = "";
        public List<string> Options { get; set; } = new List<string>();
        public bool AllowFreeText { get; set; } = true;
        public bool Multi { get; set; }
        /// <summary>ما زال ينتظر الإجابة (آخر رسالة وحالتها awaiting).</summary>
        public bool Pending { get; set; }
        /// <summary>الإجابة كما اختارها/كتبها المستخدم (بعد الإجابة).</summary>
        public string Answer { get; set; } = "";
        /// <summary>الخيارات التى اختارها (لتمييزها فى البطاقة بعد الإجابة).</summary>
        public List<string> Selected { get; set; } = new List<string>();
        /// <summary>تجاوزه المستخدم بإرسال رسالة جديدة دون إجابة.</summary>
        public bool Skipped { get; set; }
    }

    /// <summary>P4: إجابة المستخدم: خيارات مختارة (من خيارات السؤال فقط) و/أو نص حر.</summary>
    public class AiAskAnswer
    {
        public List<string> Selected { get; set; } = new List<string>();
        public string FreeText { get; set; } = "";
    }

    public class AiConversationView
    {
        public long Id { get; set; }
        public string Title { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public DateTime? LastMessageAt { get; set; }
        /// <summary>مؤرشفة بعد مدة الاحتفاظ أو يدوياً: للقراءة فقط حتى تُستعاد.</summary>
        public bool IsArchived { get; set; }
    }

    /// <summary>رسالة كما تعرضها شاشة المحادثة (النص فقط فى P1).</summary>
    public class AiChatMessage
    {
        public long Id { get; set; }
        public int Seq { get; set; }
        /// <summary>user / assistant.</summary>
        public string Role { get; set; } = "user";
        public string Text { get; set; } = "";
        public string Status { get; set; } = AiMessageStatus.Ok;
        public string ErrorText { get; set; } = "";
        public string ModelName { get; set; } = "";
        public int? InputTokens { get; set; }
        public int? OutputTokens { get; set; }
        public decimal? CostEstimate { get; set; }
        public DateTime CreatedAt { get; set; }
        /// <summary>P2: الأدوات التى نفّذها المساعد لإعداد هذا الرد (للعرض تحت الرسالة: ماذا قرأ وبأى استعلام).</summary>
        public List<AiToolCallView> Tools { get; set; } = new List<AiToolCallView>();
        /// <summary>P3: التقارير التى رسمها المساعد فى هذا الرد (بطاقة «عرض التقرير»).</summary>
        public List<AiReportCard> Reports { get; set; } = new List<AiReportCard>();
        /// <summary>P3: الشاشات التى اقترح فتحها (بطاقة «افتح الشاشة»).</summary>
        public List<AiScreenLinkView> Screens { get; set; } = new List<AiScreenLinkView>();
        /// <summary>P4: سؤال المساعد للمستخدم إن وُجد.</summary>
        public AiAskView Ask { get; set; }
        /// <summary>P5: المسودات التى أعدّها المساعد (بطاقة «مسودة جاهزة»).</summary>
        public List<AiDraftCard> Drafts { get; set; } = new List<AiDraftCard>();
        /// <summary>P6: ملفات رسالة المستخدم.</summary>
        public List<AiAttachmentView> Attachments { get; set; } = new List<AiAttachmentView>();
        /// <summary>P6: البيانات المستخرجة من المرفقات فى هذا الرد (بطاقة بالحقول والثقة).</summary>
        public List<AiExtractionCard> Extractions { get; set; } = new List<AiExtractionCard>();
        /// <summary>P7.6: 👍/👎 المستخدم على هذا الرد (null = لم يقيّمه).</summary>
        public AiFeedbackView Feedback { get; set; }
        /// <summary>P8.1.5: كتلة «موجز يومى» (بطاقة «افتح الموجز»).</summary>
        public List<AiBriefLinkView> Briefs { get; set; } = new List<AiBriefLinkView>();
    }

    // ───────────── P6: المرفقات والاستخراج ─────────────

    /// <summary>ملف مرفق كما يظهر فى المحادثة. Kind = image / pdf / sheet.</summary>
    public class AiAttachmentView
    {
        public long Id { get; set; }
        public string FileName { get; set; } = "";
        public string MimeType { get; set; } = "";
        public long SizeBytes { get; set; }
        public string Kind { get; set; } = "image";
        /// <summary>انتهت مدة الاحتفاظ وحُذف الملف (يبقى الاسم فقط).</summary>
        public bool Expired { get; set; }
    }

    /// <summary>الملف نفسه للمعاينة (لصاحبه فقط).</summary>
    public class AiAttachmentFile
    {
        public string FileName { get; set; } = "";
        public string MimeType { get; set; } = "";
        public byte[] Data { get; set; } = Array.Empty<byte>();
    }

    /// <summary>حدود الرفع للواجهة (accept وحجم أقصى وعدد ملفات الرسالة).</summary>
    public class AiUploadLimits
    {
        public int MaxMb { get; set; } = 10;
        public int MaxFiles { get; set; } = 5;
        public string Accept { get; set; } = "";
    }

    /// <summary>حقل رأس مستخرج بثقته (0..1). Low = يحتاج مراجعة (ثقة ضعيفة أو لا يتسق حسابياً).</summary>
    public class AiExtractField
    {
        public string Key { get; set; } = "";
        public string Label { get; set; } = "";
        public string Value { get; set; } = "";
        public double Confidence { get; set; }
        public bool Low { get; set; }
    }

    public class AiExtractLine
    {
        public int No { get; set; }
        public string Description { get; set; } = "";
        public string Code { get; set; } = "";
        public decimal? Qty { get; set; }
        public string Unit { get; set; } = "";
        public decimal? UnitPrice { get; set; }
        public decimal? Discount { get; set; }
        public decimal? Tax { get; set; }
        public decimal? LineTotal { get; set; }
        public double Confidence { get; set; }
        /// <summary>أسماء الحقول ضعيفة الثقة فى السطر (qty، unit_price، line_total...).</summary>
        public List<string> LowFields { get; set; } = new List<string>();
    }

    /// <summary>ناتج extract_document بعد التطبيع والتحقق على الخادم.</summary>
    public class AiExtraction
    {
        public string DocType { get; set; } = "";
        public string Currency { get; set; } = "";
        public List<AiExtractField> Fields { get; set; } = new List<AiExtractField>();
        public List<AiExtractLine> Lines { get; set; } = new List<AiExtractLine>();
        public List<string> Warnings { get; set; } = new List<string>();
        /// <summary>doc_type = other: نص المستند/ملخصه.</summary>
        public string Text { get; set; } = "";
        public string Field(string key) => Fields.FirstOrDefault(f => f.Key == key)?.Value ?? "";
    }

    public class AiExtractionCard
    {
        public long AttachmentId { get; set; }
        public string FileName { get; set; } = "";
        public AiExtraction Extraction { get; set; } = new AiExtraction();
    }

    // ───────────── P2: الأدوات (قراءة فقط) ─────────────

    public static class AiToolOutcome
    {
        public const string Ok = "ok";
        public const string Denied = "denied";
        public const string Error = "error";
    }

    /// <summary>استدعاء أداة واحد كما يظهر تحت رد المساعد.</summary>
    public class AiToolCallView
    {
        public string ToolName { get; set; } = "";
        /// <summary>وصف عربى قصير لما فعلته الأداة («استعلام: مبيعات الشهر»).</summary>
        public string Title { get; set; } = "";
        public string Outcome { get; set; } = AiToolOutcome.Ok;
        public string Sql { get; set; } = "";
        public int? RowCount { get; set; }
        public int? DurationMs { get; set; }
        public string ErrorText { get; set; } = "";
        /// <summary>P3: معرّف البيانات كما أُعطى للنموذج وأعمدتها (لرسائل المتابعة؛ لا يُعرض).</summary>
        public string DatasetId { get; set; } = "";
        public string DatasetColumns { get; set; } = "";
    }

    /// <summary>
    /// من يسأل، وماذا يحق له أن يقرأ: AllowedRoutes = مسارات الشاشات المسموحة له (RazorPath من نفس قائمة الويب MenuStateService)،
    /// فكل أداة/View تُتاح فقط إن كان يملك الشاشة المقابلة (§2.3). الأدوات غير المسموحة لا تُرسل للنموذج أصلاً.
    /// </summary>
    public class AiUserContext
    {
        public long UserId { get; set; }
        public long BranchId { get; set; }
        public bool IsAdmin { get; set; }
        public List<string> AllowedRoutes { get; set; } = new List<string>();
        /// <summary>P3: نفس الشاشات بعناوينها (لأداة find_screen).</summary>
        public List<AiScreenRef> AllowedScreens { get; set; } = new List<AiScreenRef>();
    }

    /// <summary>صف من سجل تدقيق الأدوات (تبويب «سجل التدقيق» فى /settings/ai).</summary>
    public class AiToolAuditView
    {
        public long Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; } = "";
        public long? ConversationId { get; set; }
        public string ToolName { get; set; } = "";
        public string ArgsJson { get; set; } = "";
        public string SqlText { get; set; } = "";
        public int? RowCount { get; set; }
        public int? DurationMs { get; set; }
        public string Outcome { get; set; } = "";
        public string ErrorText { get; set; } = "";
    }

    /// <summary>حالة اتصال القراءة: هل ضُبط حساب قاعدة بيانات للقراءة فقط (ConnectionStrings:AiReadOnly) أم يُستخدم الاتصال الرئيسى مع الحماية البرمجية.</summary>
    public class AiReadOnlyStatus
    {
        public bool DedicatedLogin { get; set; }
        public string Message { get; set; } = "";
    }

    /// <summary>سياق الشاشة الحالية (المسار وعنوانها) ليفهم المساعد «هذه الفاتورة» أو «هذا العميل».</summary>
    public class AiScreenContext
    {
        public string Route { get; set; } = "";
        public string Title { get; set; } = "";
        public string Language { get; set; } = "ar";
        /// <summary>P8.9 (ص3): محددات الشاشة (اختيارى). غيابه = السلوك السابق حرفياً.</summary>
        public AiScreenPayload Payload { get; set; }
    }

    /// <summary>جاهزية المحادثة: هل يوجد نموذج مربوط بمهمة «محادثة»؟ Message رسالة عربية عند عدم الجاهزية.</summary>
    public class AiChatAvailability
    {
        public bool Ready { get; set; }
        public string ModelName { get; set; } = "";
        public string Message { get; set; } = "";
        /// <summary>مدة الاحتفاظ بالمحادثات بالأيام (0 = بلا أرشفة تلقائية) — للعرض فى قائمة «المؤرشفة».</summary>
        public int ChatRetentionDays { get; set; } = 365;
    }

    public class AiChatTurnResult
    {
        public long ConversationId { get; set; }
        public AiChatMessage UserMessage { get; set; }
        public AiChatMessage AssistantMessage { get; set; }
    }

    /// <summary>
    /// بوابة المحادثة (P1) لصفحة /assistant والنافذة العائمة. تُنفَّذ فى AccountERP.AI ويسجّلها مضيف الويب فقط؛ على مضيف بلا تسجيل
    /// يختفى المساعد. كل دالة تتحقق أن المحادثة تخص userId نفسه. الأخطاء رسائل عربية (InvalidOperationException).
    /// </summary>
    public interface IAiChatGateway
    {
        AiChatAvailability GetAvailability();
        /// <summary>archived = true: المؤرشفة (بعد مدة الاحتفاظ ChatRetentionDays أو يدوياً). يطبّق الأرشفة التلقائية مرة كل 6 ساعات على الأكثر.</summary>
        List<AiConversationView> GetConversations(long userId, string search, bool archived = false);
        /// <summary>أرشفة محادثة أو استعادتها (صاحبها فقط).</summary>
        void SetConversationArchived(long conversationId, bool archived, long userId);
        List<AiChatMessage> GetMessages(long conversationId, long userId);
        void RenameConversation(long conversationId, string title, long userId);
        void DeleteConversation(long conversationId, long userId);

        /// <summary>
        /// يرسل رسالة المستخدم ويبثّ الرد: onDelta يُستدعى لكل جزء نصى (من خيط خلفى). conversationId = 0 ينشئ محادثة جديدة.
        /// الإلغاء (زر الإيقاف) يحفظ الجزء المكتوب بحالة stopped ولا يرمى استثناء.
        /// </summary>
        /// <param name="onStatus">P2: نص حالة قصير أثناء تنفيذ الأدوات («أبحث فى البيانات…»)؛ null عند انتهاء الأداة.</param>
        /// <param name="attachmentIds">P6: مرفقات رفعها المستخدم (UploadAttachmentAsync) ولم تُرسل بعد؛ النص قد يكون فارغاً معها.</param>
        Task<AiChatTurnResult> SendAsync(long conversationId, string text, IReadOnlyList<long> attachmentIds, AiScreenContext context, AiUserContext user,
                                         Action<string> onDelta, Action<string> onStatus, CancellationToken ct = default);

        // ───── P6: المرفقات ─────
        AiUploadLimits GetUploadLimits();
        /// <summary>يتحقق من النوع (بالمحتوى لا الامتداد فقط) والحجم ثم يحفظ الملف؛ يرمى InvalidOperationException برسالة عربية عند الرفض.</summary>
        Task<AiAttachmentView> UploadAttachmentAsync(long userId, string fileName, string contentType, System.IO.Stream content, CancellationToken ct = default);
        /// <summary>إزالة مرفق قبل إرساله.</summary>
        void RemoveAttachment(long attachmentId, long userId);
        /// <summary>الملف للمعاينة (لصاحبه فقط)؛ null إن انتهت مدة الاحتفاظ أو لم يوجد.</summary>
        AiAttachmentFile GetAttachmentFile(long attachmentId, long userId);

        /// <summary>
        /// P4: إجابة سؤال المساعد المعلّق (رسالة messageId بحالة awaiting) ثم استئناف الحلقة وبثّ الرد. الإجابة تُحفظ رسالة مستخدم،
        /// وتعود للنموذج كنتيجة الأداة ask_user (أو كنص إن تغيّر النموذج). يرفض إن لم يعد السؤال معلّقاً.
        /// </summary>
        Task<AiChatTurnResult> AnswerAsync(long conversationId, long messageId, AiAskAnswer answer, AiScreenContext context, AiUserContext user,
                                           Action<string> onDelta, Action<string> onStatus, CancellationToken ct = default);

        // ───── P3: التقارير ─────
        /// <summary>التقرير جاهزاً للعرض (لصاحبه فقط)؛ null إن لم يوجد.</summary>
        AiReportDoc GetReport(long reportId, long userId);
        /// <summary>تقاريرى المحفوظة (المثبّتة أولاً).</summary>
        List<AiSavedReportView> GetSavedReports(long userId);
        /// <summary>حفظ التقرير فى «تقاريرى» (بعنوان جديد اختيارياً) أو تثبيته/إلغاء تثبيته.</summary>
        void SaveReport(long reportId, string title, bool pinned, long userId);
        /// <summary>إزالته من «تقاريرى» (يبقى فى محادثته إن وُجدت).</summary>
        void UnsaveReport(long reportId, long userId);
        /// <summary>إعادة تشغيل مصادر التقرير ببيانات اليوم، وبفترة جديدة إن أُعطيت؛ الصلاحيات تُفحص من جديد.</summary>
        Task<AiReportDoc> RerunReportAsync(long reportId, DateTime? from, DateTime? to, AiUserContext user, CancellationToken ct = default);

        // ───── P7.6: الملاحظات ─────
        /// <summary>
        /// 👍 (1) / 👎 (-1) على رد مساعد مكتمل فى محادثة يملكها المستخدم؛ rating = 0 يسحب الملاحظة. التعديل والسحب مسموحان ما لم يراجعها المدير.
        /// يعيد الحالة الجديدة (null بعد السحب).
        /// </summary>
        AiFeedbackView SetFeedback(long messageId, int rating, string reason, string comment, long userId);
    }
}
