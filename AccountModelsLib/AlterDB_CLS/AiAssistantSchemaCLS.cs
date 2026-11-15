using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib
{
    /// <summary>
    /// ترقية القاعدة للمساعد الذكى (.claude/docs/ai-assistant-plan.md §8، المرحلتان P0/P1): جداول المزوّدين والنماذج وربط المهام والإعدادات والمحادثات،
    /// وعنصر قائمة «إعدادات الذكاء الاصطناعى» (/settings/ai). كل الجداول جديدة ببادئة AI_ ومعرّفها bigint بـ Snowflake، ولا مفاتيح أجنبية
    /// على جداول النظام القائمة. إدراج فقط (IF NOT EXISTS)، لا يغيّر أى صف قائم، وآمنة للتكرار. جداول المراحل التالية تُضاف فى مراحلها.
    /// </summary>
    public static class AiAssistantSchemaCLS
    {
        private const string Audit = @"
    [CreatedBy] [bigint] NULL,
    [CreatedAt] [datetime] NULL DEFAULT (getdate()),
    [UpdatedBy] [bigint] NULL,
    [UpdatedAt] [datetime] NULL";

        /// <summary>(اسم الجدول، الأعمدة بعد Id، فهارس).</summary>
        private static readonly (string Table, string Columns, string Indexes)[] Tables =
        {
            // المفتاح وHeaders الإضافية مشفّرة بـ IDataProtector فى طبقة AccountERP.AI؛ هذا الجدول لا يرى نصاً صريحاً أبداً.
            ("AI_Providers", @"
    [ProviderType] [nvarchar](30) NOT NULL,
    [DisplayName] [nvarchar](200) NOT NULL,
    [BaseUrl] [nvarchar](500) NULL,
    [ApiKeyEnc] [nvarchar](max) NULL,
    [ApiKeyHint] [nvarchar](10) NULL,
    [ExtraJson] [nvarchar](max) NULL,
    [ApiProtocol] [nvarchar](20) NULL,
    [AuthHeaderName] [nvarchar](100) NULL,
    [ExtraHeadersEnc] [nvarchar](max) NULL,
    [TimeoutSeconds] [int] NOT NULL DEFAULT ((60)),
    [IsCustom] [bit] NOT NULL DEFAULT ((0)),
    [IsEnabled] [bit] NOT NULL DEFAULT ((1)),
    [IsLocal] [bit] NOT NULL DEFAULT ((0)),
    [LastTestAt] [datetime] NULL,
    [LastTestOk] [bit] NULL,
    [LastTestMessage] [nvarchar](1000) NULL,", ""),

            ("AI_Models", @"
    [Provider_Id] [bigint] NOT NULL,
    [ModelId] [nvarchar](200) NOT NULL,
    [DisplayName] [nvarchar](200) NULL,
    [SupportsTools] [bit] NOT NULL DEFAULT ((1)),
    [SupportsVision] [bit] NOT NULL DEFAULT ((0)),
    [SupportsTemperature] [bit] NOT NULL DEFAULT ((0)),
    [SupportsEmbeddings] [bit] NOT NULL DEFAULT ((0)),
    [ContextTokens] [int] NULL,
    [InputPricePerM] [decimal](18, 4) NULL,
    [OutputPricePerM] [decimal](18, 4) NULL,
    [IsEnabled] [bit] NOT NULL DEFAULT ((1)),",
             @"CREATE UNIQUE NONCLUSTERED INDEX [UX_AI_Models_Provider_Model] ON [dbo].[AI_Models]([Provider_Id], [ModelId]);"),

            // TaskKey: chat / reasoning / vision / fast / embedding. Priority 1 = الأساسى، والأكبر = بدائل (fallback).
            ("AI_TaskRouting", @"
    [TaskKey] [nvarchar](30) NOT NULL,
    [Model_Id] [bigint] NOT NULL,
    [Priority] [int] NOT NULL DEFAULT ((1)),
    [Effort] [nvarchar](20) NULL,
    [MaxTokens] [int] NULL,
    [Temperature] [decimal](4, 2) NULL,",
             @"CREATE UNIQUE NONCLUSTERED INDEX [UX_AI_TaskRouting_Task_Priority] ON [dbo].[AI_TaskRouting]([TaskKey], [Priority]);"),

            ("AI_Settings", @"
    [SettingKey] [nvarchar](100) NOT NULL,
    [SettingValue] [nvarchar](max) NULL,",
             @"CREATE UNIQUE NONCLUSTERED INDEX [UX_AI_Settings_Key] ON [dbo].[AI_Settings]([SettingKey]);"),

            // P1: المحادثات ورسائلها. User_Id مرجع منطقى فقط (لا مفتاح أجنبى على USER_DETAILES). Status: Active / Archived.
            ("AI_Conversations", @"
    [User_Id] [bigint] NOT NULL,
    [Branch_Id] [bigint] NULL,
    [Title] [nvarchar](200) NULL,
    [Status] [nvarchar](20) NOT NULL DEFAULT (N'Active'),
    [LastMessageAt] [datetime] NULL,",
             @"CREATE NONCLUSTERED INDEX [IX_AI_Conversations_User] ON [dbo].[AI_Conversations]([User_Id], [Status], [LastMessageAt] DESC);"),

            // ContentJson = كتل المحتوى ([{""type"":""text"",""text"":...}]) لتتسع للأدوات فى P2. Status: ok / stopped / error.
            ("AI_Messages", @"
    [Conversation_Id] [bigint] NOT NULL,
    [Seq] [int] NOT NULL,
    [Role] [nvarchar](20) NOT NULL,
    [ContentJson] [nvarchar](max) NULL,
    [Model_Id] [bigint] NULL,
    [ModelName] [nvarchar](200) NULL,
    [InputTokens] [int] NULL,
    [OutputTokens] [int] NULL,
    [CacheReadTokens] [int] NULL,
    [CostEstimate] [decimal](18, 6) NULL,
    [Status] [nvarchar](20) NULL,
    [ErrorText] [nvarchar](1000) NULL,
    [DurationMs] [int] NULL,",
             @"CREATE UNIQUE NONCLUSTERED INDEX [UX_AI_Messages_Conv_Seq] ON [dbo].[AI_Messages]([Conversation_Id], [Seq]);"),

            // P2: سجل تدقيق كل استدعاء أداة (§2.1 طبقة 6): يُدرج لحظة التنفيذ (Message_Id يُربط بعد حفظ رد المساعد)، فالمرفوض يُسجَّل حتى لو انقطع الرد.
            // Outcome: ok / denied / error. SqlText = الاستعلام كما نُفّذ (أو كما رُفض).
            ("AI_ToolCalls_Audit", @"
    [Conversation_Id] [bigint] NULL,
    [Message_Id] [bigint] NULL,
    [User_Id] [bigint] NOT NULL,
    [ToolName] [nvarchar](100) NOT NULL,
    [ArgsJson] [nvarchar](max) NULL,
    [SqlText] [nvarchar](max) NULL,
    [RowCount] [int] NULL,
    [DurationMs] [int] NULL,
    [Outcome] [nvarchar](20) NOT NULL,
    [ErrorText] [nvarchar](1000) NULL,",
             @"CREATE NONCLUSTERED INDEX [IX_AI_ToolCalls_Audit_CreatedAt] ON [dbo].[AI_ToolCalls_Audit]([CreatedAt] DESC);"),

            // P3: كل تقرير يرسمه المساعد (render_report) صف هنا: SpecJson = الشكل بعد التحقق، SourcesJson = الأداة ومدخلاتها لكل مصدر بيانات
            // (لإعادة التشغيل بفترة جديدة)، DataJson = لقطة البيانات وقت الرسم. IsSaved = فى «تقاريرى»؛ غير المحفوظ يُحذف مع محادثته.
            ("AI_SavedReports", @"
    [User_Id] [bigint] NOT NULL,
    [Conversation_Id] [bigint] NULL,
    [Message_Id] [bigint] NULL,
    [Title] [nvarchar](200) NOT NULL,
    [SpecJson] [nvarchar](max) NOT NULL,
    [SourcesJson] [nvarchar](max) NULL,
    [DataJson] [nvarchar](max) NULL,
    [PeriodFrom] [date] NULL,
    [PeriodTo] [date] NULL,
    [IsSaved] [bit] NOT NULL DEFAULT (0),
    [IsPinned] [bit] NOT NULL DEFAULT (0),
    [IsShared] [bit] NOT NULL DEFAULT (0),
    [LastRunAt] [datetime] NULL,",
             @"CREATE NONCLUSTERED INDEX [IX_AI_SavedReports_User] ON [dbo].[AI_SavedReports]([User_Id], [IsSaved], [IsPinned]);"),

            // P5: مسودات المساعد (فاتورة مبيعات / مشتريات / إذن مخزن). لا يُكتب شىء فى جداول المستندات قبل ضغط المستخدم «حفظ» فى الشاشة الحقيقية؛
            // الشاشة تفتح ?aiDraft={Id} وتعبّئ نفسها، وبعد الحفظ: Status = Consumed + معرّف/رقم المستند الناتج (للتدقيق).
            // DraftType: sales / purchase / store_order. Status: Pending / Consumed / Discarded.
            ("AI_Drafts", @"
    [User_Id] [bigint] NOT NULL,
    [Conversation_Id] [bigint] NULL,
    [Message_Id] [bigint] NULL,
    [DraftType] [nvarchar](30) NOT NULL,
    [Title] [nvarchar](200) NULL,
    [PayloadJson] [nvarchar](max) NOT NULL,
    [Status] [nvarchar](20) NOT NULL DEFAULT (N'Pending'),
    [Target_Doc_Id] [bigint] NULL,
    [Target_Doc_No] [nvarchar](50) NULL,
    [ConsumedAt] [datetime] NULL,",
             @"CREATE NONCLUSTERED INDEX [IX_AI_Drafts_User] ON [dbo].[AI_Drafts]([User_Id], [Status], [CreatedAt] DESC);"),

            // P6: الملفات التى يرفعها المستخدم للمساعد (صور، PDF، Excel/CSV). الملف نفسه على القرص (StoragePath نسبى لمجلد المرفقات) لا فى القاعدة.
            // Sha256 لكشف رفع نفس الملف مرة أخرى. ExtractJson = آخر استخراج منظّم (يُعاد استخدامه بلا استدعاء نموذج لنفس النوع).
            // Status: Active / Expired (بعد مدة الاحتفاظ يُحذف الملف من القرص ويبقى الصف للتدقيق).
            ("AI_Attachments", @"
    [User_Id] [bigint] NOT NULL,
    [Conversation_Id] [bigint] NULL,
    [Message_Id] [bigint] NULL,
    [FileName] [nvarchar](260) NOT NULL,
    [MimeType] [nvarchar](100) NOT NULL,
    [SizeBytes] [bigint] NOT NULL,
    [Sha256] [char](64) NOT NULL,
    [StoragePath] [nvarchar](400) NOT NULL,
    [Status] [nvarchar](20) NOT NULL DEFAULT (N'Active'),
    [ExtractDocType] [nvarchar](30) NULL,
    [ExtractJson] [nvarchar](max) NULL,
    [ExtractModel_Id] [bigint] NULL,
    [ExtractInputTokens] [int] NULL,
    [ExtractOutputTokens] [int] NULL,
    [ExtractCost] [decimal](18, 6) NULL,
    [ExtractedAt] [datetime] NULL,
    [Doc_No] [nvarchar](60) NULL,
    [ExpiredAt] [datetime] NULL,",
             @"CREATE NONCLUSTERED INDEX [IX_AI_Attachments_User] ON [dbo].[AI_Attachments]([User_Id], [CreatedAt] DESC);
CREATE NONCLUSTERED INDEX [IX_AI_Attachments_Sha] ON [dbo].[AI_Attachments]([Sha256]);"),

            // P7: معرفة المساعد (قاموس الـ Views، المسرد، القواعد، الأمثلة). Kind: view / glossary / rule / example / screen.
            // صفوف view تُنسخ من الكود (AiSchemaCatalog) عند كل مزامنة؛ الباقى يُزرع مرة ثم الجدول هو المرجع (IsEdited = عدّله المدير فلا يُمس).
            // ViewsCsv = الـ Views اللازمة كلها لرؤية المقطع، RoutesCsv = شاشات تمنحه. Embedding = float32 مطبّع (P7.2) لنموذج EmbeddingModel_Id.
            ("AI_Knowledge", @"
    [Kind] [nvarchar](20) NOT NULL,
    [KeyName] [nvarchar](150) NOT NULL,
    [TitleAr] [nvarchar](300) NOT NULL,
    [Content] [nvarchar](max) NOT NULL,
    [Keywords] [nvarchar](1000) NULL,
    [ViewsCsv] [nvarchar](500) NULL,
    [RoutesCsv] [nvarchar](1000) NULL,
    [Source] [nvarchar](30) NOT NULL DEFAULT (N'seed'),
    [Source_Ref_Id] [bigint] NULL,
    [Version] [int] NOT NULL DEFAULT ((1)),
    [IsEdited] [bit] NOT NULL DEFAULT ((0)),
    [IsActive] [bit] NOT NULL DEFAULT ((1)),
    [ContentHash] [char](64) NULL,
    [Embedding] [varbinary](max) NULL,
    [EmbeddingModel_Id] [bigint] NULL,
    [EmbeddingDim] [int] NULL,
    [EmbeddedAt] [datetime] NULL,",
             @"CREATE UNIQUE NONCLUSTERED INDEX [UX_AI_Knowledge_Kind_Key] ON [dbo].[AI_Knowledge]([Kind], [KeyName]);"),

            // P7.4: مجموعة أسئلة التقييم. CheckType: result / tool / financial / ask / refuse. ExpectedTools/ForbiddenTools = أسماء أدوات مفصولة بفاصلة.
            // ReferenceSql = استعلام قراءة مرجعى يُنفَّذ حياً وقت التقييم (result)، ReferenceArgsJson = مدخلات الأداة المالية المرجعية (financial)،
            // ExpectedText = عبارات بديلة مفصولة بـ | يجب أن تظهر إحداها فى الرد (اختيارى). الحالات المزروعة تُدرج مرة (CaseKey) ثم الجدول هو المرجع.
            ("AI_EvalCases", @"
    [CaseKey] [nvarchar](100) NOT NULL,
    [Question] [nvarchar](2000) NOT NULL,
    [Category] [nvarchar](30) NOT NULL,
    [Dialect] [nvarchar](10) NOT NULL DEFAULT (N'msa'),
    [CheckType] [nvarchar](20) NOT NULL,
    [ExpectedTools] [nvarchar](300) NULL,
    [ForbiddenTools] [nvarchar](300) NULL,
    [ReferenceSql] [nvarchar](max) NULL,
    [ReferenceArgsJson] [nvarchar](2000) NULL,
    [ExpectedText] [nvarchar](1000) NULL,
    [Tags] [nvarchar](300) NULL,
    [Notes] [nvarchar](1000) NULL,
    [Source] [nvarchar](30) NOT NULL DEFAULT (N'seed'),
    [Source_Ref_Id] [bigint] NULL,
    [Version] [int] NOT NULL DEFAULT ((1)),
    [IsEdited] [bit] NOT NULL DEFAULT ((0)),
    [IsActive] [bit] NOT NULL DEFAULT ((1)),",
             @"CREATE UNIQUE NONCLUSTERED INDEX [UX_AI_EvalCases_Key] ON [dbo].[AI_EvalCases]([CaseKey]);"),

            // P7.4: تشغيل تقييم لنموذج واحد. Group_Id يجمع نموذجين أُطلقا معاً على نفس الحالات (للمقارنة). Status: Running / Done / Cancelled / Interrupted / CostCap / Failed.
            ("AI_EvalRuns", @"
    [Group_Id] [bigint] NOT NULL,
    [User_Id] [bigint] NOT NULL,
    [Model_Id] [bigint] NOT NULL,
    [ModelName] [nvarchar](200) NULL,
    [ProviderName] [nvarchar](200) NULL,
    [Effort] [nvarchar](20) NULL,
    [MaxTokens] [int] NULL,
    [Status] [nvarchar](20) NOT NULL,
    [FilterJson] [nvarchar](2000) NULL,
    [ConfigJson] [nvarchar](max) NULL,
    [PromptHash] [char](64) NULL,
    [TotalCases] [int] NOT NULL DEFAULT ((0)),
    [DoneCases] [int] NOT NULL DEFAULT ((0)),
    [PassedCases] [int] NOT NULL DEFAULT ((0)),
    [FailedCases] [int] NOT NULL DEFAULT ((0)),
    [ErrorCases] [int] NOT NULL DEFAULT ((0)),
    [ToolOkCases] [int] NOT NULL DEFAULT ((0)),
    [InputTokens] [bigint] NULL,
    [OutputTokens] [bigint] NULL,
    [CostEstimate] [decimal](18, 6) NULL,
    [DurationMs] [bigint] NULL,
    [StartedAt] [datetime] NULL,
    [FinishedAt] [datetime] NULL,
    [ErrorText] [nvarchar](1000) NULL,",
             @"CREATE NONCLUSTERED INDEX [IX_AI_EvalRuns_Group] ON [dbo].[AI_EvalRuns]([Group_Id]);
CREATE NONCLUSTERED INDEX [IX_AI_EvalRuns_Started] ON [dbo].[AI_EvalRuns]([StartedAt] DESC);"),

            // P7.4: نتيجة كل حالة فى كل تشغيل. Outcome: pass / fail / error / skipped. ToolsJson = الأدوات المنفّذة (الاسم، المدخلات، النتيجة، الاستعلام، الصفوف).
            ("AI_EvalResults", @"
    [Run_Id] [bigint] NOT NULL,
    [Case_Id] [bigint] NOT NULL,
    [CaseKey] [nvarchar](100) NOT NULL,
    [CaseVersion] [int] NOT NULL,
    [Outcome] [nvarchar](20) NOT NULL,
    [ToolOk] [bit] NOT NULL DEFAULT ((0)),
    [FailReason] [nvarchar](1000) NULL,
    [AnswerText] [nvarchar](max) NULL,
    [ToolsJson] [nvarchar](max) NULL,
    [Rounds] [int] NULL,
    [InputTokens] [int] NULL,
    [OutputTokens] [int] NULL,
    [CostEstimate] [decimal](18, 6) NULL,
    [DurationMs] [int] NULL,",
             @"CREATE UNIQUE NONCLUSTERED INDEX [UX_AI_EvalResults_Run_Case] ON [dbo].[AI_EvalResults]([Run_Id], [Case_Id]);"),

            // P7.6: 👍/👎 على ردود المساعد. Rating: 1 / -1. لقطة من السؤال والرد والاستعلام والأدوات وقت التقييم (تبقى للمراجعة لو حُذفت المحادثة).
            // ReviewStatus: New / Accepted / Rejected. Knowledge_Id = اعتُمد مثالاً، EvalCase_Id = صار حالة تقييم.
            ("AI_Feedback", @"
    [Message_Id] [bigint] NOT NULL,
    [Conversation_Id] [bigint] NOT NULL,
    [User_Id] [bigint] NOT NULL,
    [Rating] [smallint] NOT NULL,
    [Reason] [nvarchar](30) NULL,
    [Comment] [nvarchar](1000) NULL,
    [QuestionText] [nvarchar](max) NULL,
    [AnswerText] [nvarchar](max) NULL,
    [SqlText] [nvarchar](max) NULL,
    [ToolsJson] [nvarchar](max) NULL,
    [Model_Id] [bigint] NULL,
    [ModelName] [nvarchar](200) NULL,
    [ReviewStatus] [nvarchar](20) NOT NULL DEFAULT (N'New'),
    [ReviewedBy] [bigint] NULL,
    [ReviewedAt] [datetime] NULL,
    [ReviewNote] [nvarchar](1000) NULL,
    [Knowledge_Id] [bigint] NULL,
    [EvalCase_Id] [bigint] NULL,",
             @"CREATE UNIQUE NONCLUSTERED INDEX [UX_AI_Feedback_Message_User] ON [dbo].[AI_Feedback]([Message_Id], [User_Id]);
CREATE NONCLUSTERED INDEX [IX_AI_Feedback_Status] ON [dbo].[AI_Feedback]([ReviewStatus], [CreatedAt] DESC);"),

            // P8.1 (ai-assistant-p8-plan.md §4): الموجز اليومى. صف لكل (يوم، نطاق): ScopeKey = بصمة كل ما يغيّر الأرقام (الفروع، الأقسام، TaxFilter، الصناديق،
            // إصدار الجامع)، فالمستخدمون المتطابقون يتشاركون صفاً واحداً. الحجز بإدراج صف Running تحت الفهرس الفريد (آمن مع التزامن وإعادة التشغيل).
            // FactsJson/AlertsJson محايدة لغوياً (أكواد + أرقام + أسماء كيانات). Status: Running / Ready / Partial / Failed. TriggeredBy: schedule / manual / test.
            ("AI_Briefs", @"
    [BriefDate] [date] NOT NULL,
    [PeriodFrom] [date] NOT NULL,
    [PeriodTo] [date] NOT NULL,
    [ScopeKey] [char](64) NOT NULL,
    [BranchIdsCsv] [nvarchar](500) NULL,
    [SectionsCsv] [nvarchar](200) NULL,
    [TaxFilter] [int] NOT NULL DEFAULT ((-1)),
    [FactsJson] [nvarchar](max) NULL,
    [AlertsJson] [nvarchar](max) NULL,
    [AlertCount] [int] NOT NULL DEFAULT ((0)),
    [ErrorsJson] [nvarchar](max) NULL,
    [Status] [nvarchar](20) NOT NULL DEFAULT (N'Running'),
    [CollectorVersion] [int] NOT NULL DEFAULT ((1)),
    [DurationMs] [int] NULL,
    [TriggeredBy] [nvarchar](20) NULL,",
             @"CREATE UNIQUE NONCLUSTERED INDEX [UX_AI_Briefs_Date_Scope] ON [dbo].[AI_Briefs]([BriefDate], [ScopeKey]);"),

            // P8.1: سرد النموذج (أو القالب) لكل موجز ولغة. Source: model / rejected / error / capped / off (غير model ⇒ الواجهة ترسم القالب من الحقائق).
            ("AI_BriefNarratives", @"
    [Brief_Id] [bigint] NOT NULL,
    [Language] [nvarchar](5) NOT NULL,
    [NarrativeText] [nvarchar](max) NULL,
    [QuestionsJson] [nvarchar](1000) NULL,
    [Source] [nvarchar](20) NOT NULL,
    [Model_Id] [bigint] NULL,
    [ModelName] [nvarchar](150) NULL,
    [InputTokens] [int] NULL,
    [OutputTokens] [int] NULL,
    [CostEstimate] [decimal](18, 6) NULL,
    [RejectReason] [nvarchar](300) NULL,",
             @"CREATE UNIQUE NONCLUSTERED INDEX [UX_AI_BriefNarratives_Brief_Lang] ON [dbo].[AI_BriefNarratives]([Brief_Id], [Language]);"),

            // P8.1: تسليم الموجز لكل مستلم (هو مصدر الإشعار وصلاحية فتح الموجز). Conversation_Id = محادثة «اسأل المساعد» المفتوحة منه.
            ("AI_BriefDeliveries", @"
    [Brief_Id] [bigint] NOT NULL,
    [User_Id] [bigint] NOT NULL,
    [Language] [nvarchar](5) NOT NULL,
    [ReadAt] [datetime] NULL,
    [HomeDismissedAt] [datetime] NULL,
    [Conversation_Id] [bigint] NULL,",
             @"CREATE UNIQUE NONCLUSTERED INDEX [UX_AI_BriefDeliveries_Brief_User] ON [dbo].[AI_BriefDeliveries]([Brief_Id], [User_Id]);
CREATE NONCLUSTERED INDEX [IX_AI_BriefDeliveries_User_Read] ON [dbo].[AI_BriefDeliveries]([User_Id], [ReadAt]);"),

            // P8.1: الاشتراك اختيارى لكل مستخدم. BranchIdsCsv/SectionsCsv فارغة = كل المسموح له.
            ("AI_BriefSubscriptions", @"
    [User_Id] [bigint] NOT NULL,
    [IsEnabled] [bit] NOT NULL DEFAULT ((1)),
    [BranchIdsCsv] [nvarchar](500) NULL,
    [SectionsCsv] [nvarchar](200) NULL,
    [Language] [nvarchar](5) NOT NULL DEFAULT (N'ar-EG'),",
             @"CREATE UNIQUE NONCLUSTERED INDEX [UX_AI_BriefSubscriptions_User] ON [dbo].[AI_BriefSubscriptions]([User_Id]);"),

            // P8.2 (ai-assistant-p82-plan.md س19–س21): آخر ما فتحه المستخدم من لوحة البحث السريع. صف لكل (مستخدم، نوع، مفتاح).
            // RefKey: للشاشة = المسار بأحرف صغيرة، للبيانات = المعرّف. HiddenAt = «مسح الأخيرة» بلا حذف.
            ("AI_SearchRecent", @"
    [User_Id] [bigint] NOT NULL,
    [Kind] [nvarchar](20) NOT NULL,
    [RefKey] [nvarchar](300) NOT NULL,
    [Title] [nvarchar](300) NOT NULL,
    [Subtitle] [nvarchar](300) NULL,
    [Url] [nvarchar](400) NOT NULL,
    [HitCount] [int] NOT NULL DEFAULT ((1)),
    [LastUsedAt] [datetime] NOT NULL DEFAULT (getdate()),
    [HiddenAt] [datetime] NULL,",
             @"CREATE UNIQUE NONCLUSTERED INDEX [UX_AI_SearchRecent_User_Kind_Key] ON [dbo].[AI_SearchRecent]([User_Id], [Kind], [RefKey]);
CREATE NONCLUSTERED INDEX [IX_AI_SearchRecent_User_Last] ON [dbo].[AI_SearchRecent]([User_Id], [LastUsedAt] DESC);"),

            // P8.4 (ai-assistant-p84-plan.md ش17–ش19): شذوذ القيود. صف لكل (قيد، قاعدة) = لقطة وقت الكشف؛ القيد نفسه لا يُمس.
            // Status: New / Reviewed / Dismissed (على مستوى القيد). IsBackfill = نتيجة مسح تاريخى طلبه المدير.
            ("AI_Anomalies", @"
    [Entry_Id] [bigint] NOT NULL,
    [RuleCode] [nvarchar](30) NOT NULL,
    [Line_Id] [bigint] NULL,
    [Severity] [nvarchar](10) NOT NULL,
    [Score] [decimal](18, 4) NULL,
    [Amount] [decimal](18, 3) NULL,
    [EntryTotal] [decimal](18, 3) NULL,
    [Account_Id] [bigint] NULL,
    [AccountName] [nvarchar](500) NULL,
    [Branch_Id] [bigint] NOT NULL DEFAULT ((0)),
    [EntryNo] [bigint] NULL,
    [EntryDate] [date] NULL,
    [EntryInsertedAt] [datetime] NULL,
    [JournalCode] [int] NULL,
    [ReferenceType] [int] NULL,
    [EntryUser_Id] [bigint] NULL,
    [DetailsJson] [nvarchar](max) NULL,
    [Run_Id] [bigint] NOT NULL,
    [IsBackfill] [bit] NOT NULL DEFAULT ((0)),
    [DetectedAt] [datetime] NOT NULL DEFAULT (getdate()),
    [Status] [nvarchar](20) NOT NULL DEFAULT (N'New'),
    [ReviewedBy] [bigint] NULL,
    [ReviewedAt] [datetime] NULL,
    [ReviewNote] [nvarchar](500) NULL,",
             @"CREATE UNIQUE NONCLUSTERED INDEX [UX_AI_Anomalies_Entry_Rule] ON [dbo].[AI_Anomalies]([Entry_Id], [RuleCode]);
CREATE NONCLUSTERED INDEX [IX_AI_Anomalies_Status_Detected] ON [dbo].[AI_Anomalies]([Status], [DetectedAt] DESC);
CREATE NONCLUSTERED INDEX [IX_AI_Anomalies_Branch_Date] ON [dbo].[AI_Anomalies]([Branch_Id], [EntryDate]);"),

            // P8.4 (ش16): تشغيلات المسح = الحجز + العلامة المائية (ToTs لآخر Done) + السجل. RunKey: s:yyyy-MM-dd للمجدول، m:/b: + معرّف لليدوى والتاريخى.
            // Status: Queued / Running / Done / Failed. TriggeredBy: schedule / manual / backfill / test.
            ("AI_AnomalyRuns", @"
    [RunKey] [nvarchar](40) NOT NULL,
    [TriggeredBy] [nvarchar](20) NOT NULL,
    [FromTs] [datetime] NULL,
    [ToTs] [datetime] NULL,
    [Status] [nvarchar](20) NOT NULL DEFAULT (N'Running'),
    [RulesCsv] [nvarchar](200) NULL,
    [EntriesScanned] [int] NOT NULL DEFAULT ((0)),
    [Found] [int] NOT NULL DEFAULT ((0)),
    [Inserted] [int] NOT NULL DEFAULT ((0)),
    [Truncated] [bit] NOT NULL DEFAULT ((0)),
    [DurationMs] [int] NULL,
    [ErrorText] [nvarchar](1000) NULL,
    [RequestedBy] [bigint] NULL,",
             @"CREATE UNIQUE NONCLUSTERED INDEX [UX_AI_AnomalyRuns_Key] ON [dbo].[AI_AnomalyRuns]([RunKey]);
CREATE NONCLUSTERED INDEX [IX_AI_AnomalyRuns_Status] ON [dbo].[AI_AnomalyRuns]([Status], [CreatedAt] DESC);"),

            // P8.4 (ش24): شرح النموذج لكل (قيد، لغة). Source: model / rejected / error / capped / off (غير model ⇒ الواجهة تعرض القالب وحده).
            ("AI_AnomalyExplains", @"
    [Entry_Id] [bigint] NOT NULL,
    [Language] [nvarchar](5) NOT NULL,
    [ExplainText] [nvarchar](max) NULL,
    [Source] [nvarchar](20) NOT NULL,
    [Model_Id] [bigint] NULL,
    [ModelName] [nvarchar](150) NULL,
    [InputTokens] [int] NULL,
    [OutputTokens] [int] NULL,
    [CostEstimate] [decimal](18, 6) NULL,
    [RejectReason] [nvarchar](300) NULL,",
             @"CREATE UNIQUE NONCLUSTERED INDEX [UX_AI_AnomalyExplains_Entry_Lang] ON [dbo].[AI_AnomalyExplains]([Entry_Id], [Language]);"),

            // P8.5 (ai-assistant-p85-plan.md ت18، ت28): سجل متابعة التحصيل لكل عميل. لا حذف؛ الإلغاء Status = Cancelled.
            // ActionType: call / promise / note / snooze / dispute / reminder_copied / reminder_sent. Status: Open / Closed / Cancelled.
            // اللقطة (BalanceAt…ScoreAt) وقت الإجراء. Outbox_Id = صف SalesRep_Notify_Outbox عند reminder_sent (يُقرأ فقط).
            ("AI_CollectionActions", @"
    [Customer_Id] [bigint] NOT NULL,
    [ActionType] [nvarchar](20) NOT NULL,
    [Status] [nvarchar](20) NOT NULL DEFAULT (N'Open'),
    [Note] [nvarchar](1000) NULL,
    [PromiseDate] [date] NULL,
    [PromiseAmount] [decimal](18, 3) NULL,
    [NextActionDate] [date] NULL,
    [Channel] [int] NULL,
    [MessageText] [nvarchar](1000) NULL,
    [MessageSource] [nvarchar](20) NULL,
    [Language] [nvarchar](5) NULL,
    [DunningLevel] [int] NULL,
    [Outbox_Id] [bigint] NULL,
    [BalanceAt] [decimal](18, 3) NULL,
    [OverdueAt] [decimal](18, 3) NULL,
    [OldestDaysAt] [int] NULL,
    [ScoreAt] [int] NULL,
    [Branch_Id] [bigint] NOT NULL DEFAULT ((0)),
    [ClosedBy] [bigint] NULL,
    [ClosedAt] [datetime] NULL,",
             @"CREATE NONCLUSTERED INDEX [IX_AI_CollectionActions_Customer] ON [dbo].[AI_CollectionActions]([Customer_Id], [CreatedAt] DESC);
CREATE NONCLUSTERED INDEX [IX_AI_CollectionActions_Type_Status] ON [dbo].[AI_CollectionActions]([ActionType], [Status], [NextActionDate]);"),

            // P8.5 (ت21، ت22): مسودة النموذج لكل (عميل، لغة). FactsHash = بصمة الحقائق والمستوى؛ اختلافها ⇒ إعادة التوليد.
            // Source: model / rejected / error (غير model ⇒ الواجهة تعرض القالب الثابت).
            ("AI_CollectionDrafts", @"
    [Customer_Id] [bigint] NOT NULL,
    [Language] [nvarchar](5) NOT NULL,
    [DunningLevel] [int] NOT NULL,
    [FactsHash] [nvarchar](64) NOT NULL,
    [DraftText] [nvarchar](max) NULL,
    [Source] [nvarchar](20) NOT NULL,
    [Model_Id] [bigint] NULL,
    [ModelName] [nvarchar](150) NULL,
    [InputTokens] [int] NULL,
    [OutputTokens] [int] NULL,
    [CostEstimate] [decimal](18, 6) NULL,
    [RejectReason] [nvarchar](300) NULL,",
             @"CREATE UNIQUE NONCLUSTERED INDEX [UX_AI_CollectionDrafts_Customer_Lang] ON [dbo].[AI_CollectionDrafts]([Customer_Id], [Language]);"),

            // P8.8 (ai-assistant-p88-plan.md ص13): سجل الاقتراحات (لا مصدر حقيقة). Kind: expense / item. Source: exact / neighbors / model.
            // Outcome: shown / applied / dismissed / overridden. FinalJson = ما حفظه المستخدم فعلاً.
            ("AI_Suggestions", @"
    [Kind] [nvarchar](20) NOT NULL,
    [User_Id] [bigint] NOT NULL,
    [Branch_Id] [bigint] NOT NULL DEFAULT ((0)),
    [InputText] [nvarchar](400) NULL,
    [InputHash] [nvarchar](64) NULL,
    [SuggestedJson] [nvarchar](max) NULL,
    [Source] [nvarchar](20) NULL,
    [Confidence] [decimal](9, 4) NULL,
    [Outcome] [nvarchar](20) NOT NULL DEFAULT (N'shown'),
    [FinalJson] [nvarchar](max) NULL,
    [Screen] [nvarchar](200) NULL,
    [DecidedAt] [datetime] NULL,",
             @"CREATE NONCLUSTERED INDEX [IX_AI_Suggestions_Kind_CreatedAt] ON [dbo].[AI_Suggestions]([Kind], [CreatedAt] DESC);"),

            // P8.8 (ص8): متجهات النصوص المميزة للسوابق (float32 LE مطبّعة) لكل نموذج تضمين.
            ("AI_Suggestions_Vectors", @"
    [Kind] [nvarchar](20) NOT NULL,
    [TextHash] [nvarchar](64) NOT NULL,
    [Text] [nvarchar](400) NOT NULL,
    [Embedding] [varbinary](max) NULL,
    [EmbeddingModel_Id] [bigint] NOT NULL,
    [EmbeddingDim] [int] NULL,",
             @"CREATE UNIQUE NONCLUSTERED INDEX [UX_AI_SuggVec_Kind_Hash_Model] ON [dbo].[AI_Suggestions_Vectors]([Kind], [TextHash], [EmbeddingModel_Id]);"),
        };

        /// <summary>القيم الافتراضية (قرارات المستخدم 2026-11-13): سحابى مسموح مع تقليل البيانات، احتفاظ 12 شهراً للمحادثات و90 يوماً للمرفقات.</summary>
        public static readonly (string Key, string Value)[] DefaultSettings =
        {
            (AiSettingKeys.LocalOnly, "0"),
            (AiSettingKeys.DataMinimization, "1"),
            (AiSettingKeys.ChatRetentionDays, "365"),
            (AiSettingKeys.AttachmentRetentionDays, "90"),
            (AiSettingKeys.MaxToolRoundsPerMessage, "12"),
            (AiSettingKeys.MaxUploadMb, "10"),
            (AiSettingKeys.EvalMaxCasesPerRun, "100"),
            (AiSettingKeys.EvalMaxCostPerRun, "0"),   // 0 = بلا حد (التشغيل يتطلب موافقة المدير على التكلفة التقديرية دائماً)
            (AiSettingKeys.KnowledgeAutoExamples, "0"),   // P7.7: موقوفة حتى يُظهر التقييم تحسّناً
            // P8.1 الموجز اليومى: موقوف حتى يفعّله المدير (ق10). الأيام بترقيم DayOfWeek (0 = الأحد … 6 = السبت): السبت–الخميس.
            (AiSettingKeys.BriefEnabled, "0"),
            (AiSettingKeys.BriefTime, "08:00"),
            (AiSettingKeys.BriefTimeZone, ""),
            (AiSettingKeys.BriefWeekDays, "0,1,2,3,4,6"),
            (AiSettingKeys.BriefNarrative, "1"),
            (AiSettingKeys.BriefMaxCostPerMonth, "0"),
            (AiSettingKeys.BriefSalesDropPct, "30"),
            (AiSettingKeys.BriefReturnsPct, "10"),
            (AiSettingKeys.BriefMinCashBalance, "0"),
            (AiSettingKeys.BriefOverdueDays, "0"),
            (AiSettingKeys.BriefLargePaymentFactor, "2"),
            // P8.2 (س33): بحث البيانات فى لوحة Ctrl+K مفعّل؛ 0 يوقفه للجميع وتبقى الشاشات.
            (AiSettingKeys.PaletteDataSearch, "1"),
            // P8.4 شذوذ القيود: موقوف حتى يفعّله المدير (ش30). AnomalyActivatedAt يُكتب عند التفعيل فقط (ش6). الأيام بترقيم DayOfWeek.
            (AiSettingKeys.AnomalyEnabled, "0"),
            (AiSettingKeys.AnomalyActivatedAt, ""),
            (AiSettingKeys.AnomalyScanTime, "02:00"),
            (AiSettingKeys.AnomalyRules, "amount_outlier,duplicate,off_hours,rare_account,round_large"),
            (AiSettingKeys.AnomalyManualOnly, "1"),
            (AiSettingKeys.AnomalyBaselineHistory, "1"),
            (AiSettingKeys.AnomalyBaselineDays, "365"),
            (AiSettingKeys.AnomalyZThreshold, "3"),
            (AiSettingKeys.AnomalyMinSample, "30"),
            (AiSettingKeys.AnomalyMinAmount, "1000"),
            (AiSettingKeys.AnomalyRoundUnit, "1000"),
            (AiSettingKeys.AnomalyRoundMinAmount, "50000"),
            (AiSettingKeys.AnomalyWorkStart, "08:00"),
            (AiSettingKeys.AnomalyWorkEnd, "22:00"),
            (AiSettingKeys.AnomalyWorkDays, "0,1,2,3,4,6"),
            (AiSettingKeys.AnomalyHolidays, ""),
            (AiSettingKeys.AnomalyRareMaxUses, "2"),
            (AiSettingKeys.AnomalyDupDays, "7"),
            (AiSettingKeys.AnomalyMaxPerRun, "500"),
            (AiSettingKeys.AnomalyExplain, "1"),
            (AiSettingKeys.AnomalyExplainMaxPerDay, "50"),
            // P8.5 أولويات التحصيل (ت5–ت31). الإرسال موقوف حتى يفعّله المدير (ت24).
            (AiSettingKeys.CollectTermsDays, "30"),
            (AiSettingKeys.CollectMinBalance, "100"),
            (AiSettingKeys.CollectInvoiceMonths, "24"),
            (AiSettingKeys.CollectMaxCustomers, "2000"),
            (AiSettingKeys.CollectWeights, "35,25,15,15,10"),
            (AiSettingKeys.CollectAgeCapDays, "120"),
            (AiSettingKeys.CollectAmountCap, "100000"),
            (AiSettingKeys.CollectDsoWindowDays, "180"),
            (AiSettingKeys.CollectDsoCapDays, "90"),
            (AiSettingKeys.CollectIdleStartDays, "30"),
            (AiSettingKeys.CollectIdleCapDays, "90"),
            (AiSettingKeys.CollectHighScore, "70"),
            (AiSettingKeys.CollectMediumScore, "40"),
            (AiSettingKeys.CollectLevel2Days, "30"),
            (AiSettingKeys.CollectLevel3Days, "60"),
            (AiSettingKeys.CollectBrokenPromiseBonus, "10"),
            (AiSettingKeys.CollectBrokenPromiseDays, "30"),
            (AiSettingKeys.CollectCacheMinutes, "5"),
            (AiSettingKeys.CollectDraft, "1"),
            (AiSettingKeys.CollectDraftMaxPerDay, "50"),
            (AiSettingKeys.CollectSendEnabled, "0"),
            (AiSettingKeys.CollectReminderMinDays, "3"),
            (AiSettingKeys.CollectBriefAlert, "1"),
            (AiSettingKeys.CollectBriefTop, "5"),
            // P8.9 (ص26)
            (AiSettingKeys.ScreenAssist, "1"),
            (AiSettingKeys.ScreenAssistNarrative, "1"),
            (AiSettingKeys.ScreenAssistMaxPerDay, "200"),
            (AiSettingKeys.ScreenContextAmbient, "1"),
            // P8.8 (ص17): موقوف افتراضياً
            (AiSettingKeys.SuggestEnabled, "0"),
            (AiSettingKeys.SuggestUseModel, "1"),
            (AiSettingKeys.SuggestMinConfidence, "0.35"),
            (AiSettingKeys.SuggestHistoryMonths, "24"),
            (AiSettingKeys.SuggestMaxRows, "50000"),
            (AiSettingKeys.SuggestForAllUsers, "0"),
        };

        public static readonly List<string> LastErrors = new List<string>();

        public static void EnsureSchema()
        {
            LastErrors.Clear();
            var opp = new operations();
            foreach (var t in Tables)
                Step(t.Table, () => CreateTable(opp, t.Table, t.Columns, t.Indexes));
            Step("AiAddedColumns", () => AddedColumns(opp));
            Step("AiSettingsSeed", () => SeedSettings(opp));
            Step("AiSettingsMenu", () => SettingsMenu(opp));
            Step("AiAssistantMenu", () => AssistantMenu(opp));
            Step("AiBriefMenu", () => BriefMenu(opp));   // P8.1
            Step("AiAnomaliesMenu", () => AnomaliesMenu(opp));   // P8.4.4
            Step("AiCollectionsMenu", () => CollectionsMenu(opp));   // P8.5.3
            Step("AiReadViews", () => AiReadViewsCLS.Ensure(opp));   // P2: schema ai وViews القراءة
        }

        /// <summary>
        /// P8.1 (ق9): عنصر «الموجز اليومى» تحت «ملف» بجوار «المساعد الذكى» يفتح /assistant/brief، بأسماء اللغات الأربع.
        /// الصلاحية للمديرين فقط افتراضياً والباقون من شاشة الصلاحيات. إدراج فقط، لا يتكرر.
        /// </summary>
        private static void BriefMenu(operations opp)
        {
            const string key = "menuItemAiDailyBrief";
            const string desc = "الموجز اليومى";
            var menus = opp.db.Database.SqlQueryRaw<StoreTransferPolicy.KeyValueRow>("SELECT Id, KeysData, Value FROM SysConfigMenus").ToList();
            string Key(StoreTransferPolicy.KeyValueRow r) => r.KeysData == null ? "" : Common.DecryptMe(r.KeysData).Trim();
            if (menus.Any(m => Key(m) == key)) return;
            var parent = menus.FirstOrDefault(m => Key(m) == "menuItemFile");
            if (parent == null) return;
            string E(string s) => Common.EncryptMe(s);
            long id = menus.Max(m => m.Id) + 1;
            opp.db.Database.ExecuteSqlInterpolated($@"
INSERT INTO SysConfigMenus (Id, Parent_Id, KeysData, Description, Description_En, Description_Fr, Description_DE, PROJECT_ID, Value, DefaultValue, DefaultDescription,
                            POS, Market, Factory, Tourism, AlFawzan, Client_A, Client_B, UserSells, IsShowToUser, RazorPath)
VALUES ({id}, {parent.Id}, {E(key)}, {E(desc)}, {E("Daily brief")}, {E("Synthèse quotidienne")}, {E("Tagesbriefing")}, 2, {E("1")}, {E("1")}, {E(desc)},
        {E("1")}, {E("1")}, {E("1")}, {E("1")}, {E("1")}, {E("1")}, {E("1")}, {E("1")}, 1, {E("/assistant/brief")})");
            var admins = opp.db.Database.SqlQueryRaw<long>("SELECT Id AS Value FROM USER_DETAILES WHERE ISNULL(AdminFlage, 0) = 1").ToList();
            foreach (long user in admins)
                opp.db.Database.ExecuteSqlInterpolated($@"
INSERT INTO SysConfigMenusPERMATIONS (Id, Menu_ID, Parent_Id, USER_ID, PERMATION, PROJECT_ID, IsShowToUser)
VALUES ({CallSnowflakeIdGenerate.GenerateNewId()}, {id}, {parent.Id}, {user}, {E("1")}, 2, 1)");
        }

        /// <summary>
        /// P8.5.3 (ai-assistant-p85-plan.md ت17): «أولويات التحصيل» بجوار «أرصدة العملاء» (نفس الأب) يفتح /accounts/collections-priority بأسماء اللغات
        /// الأربع، للمديرين فقط افتراضياً. «أرصدة العملاء» غائب ⇒ تحت «ملف» (فحص 9). إدراج فقط، لا يتكرر.
        /// </summary>
        private static void CollectionsMenu(operations opp)
        {
            const string key = AiCollectConst.MenuKey;
            const string desc = "أولويات التحصيل";
            var menus = opp.db.Database.SqlQueryRaw<StoreTransferPolicy.KeyValueRow>("SELECT Id, KeysData, Value FROM SysConfigMenus").ToList();
            string Key(StoreTransferPolicy.KeyValueRow r) => r.KeysData == null ? "" : Common.DecryptMe(r.KeysData).Trim();
            if (menus.Any(m => Key(m) == key)) return;
            var sibling = menus.FirstOrDefault(m => Key(m) == AiCollectConst.SiblingMenuKey);
            long parentId;
            if (sibling != null)
                parentId = opp.db.Database.SqlQueryRaw<long>("SELECT ISNULL(Parent_Id, 0) AS Value FROM SysConfigMenus WHERE Id = {0}", sibling.Id).AsEnumerable().First();
            else
            {
                var file = menus.FirstOrDefault(m => Key(m) == "menuItemFile");
                if (file == null) return;
                parentId = file.Id;
            }
            if (parentId <= 0) return;
            string E(string s) => Common.EncryptMe(s);
            long id = menus.Max(m => m.Id) + 1;
            opp.db.Database.ExecuteSqlInterpolated($@"
INSERT INTO SysConfigMenus (Id, Parent_Id, KeysData, Description, Description_En, Description_Fr, Description_DE, PROJECT_ID, Value, DefaultValue, DefaultDescription,
                            POS, Market, Factory, Tourism, AlFawzan, Client_A, Client_B, UserSells, IsShowToUser, RazorPath)
VALUES ({id}, {parentId}, {E(key)}, {E(desc)}, {E("Collections priority")}, {E("Priorités de recouvrement")}, {E("Inkassoprioritäten")}, 2, {E("1")}, {E("1")}, {E(desc)},
        {E("1")}, {E("1")}, {E("1")}, {E("1")}, {E("1")}, {E("1")}, {E("1")}, {E("1")}, 1, {E(AiCollectConst.Route)})");
            var admins = opp.db.Database.SqlQueryRaw<long>("SELECT Id AS Value FROM USER_DETAILES WHERE ISNULL(AdminFlage, 0) = 1").ToList();
            foreach (long user in admins)
                opp.db.Database.ExecuteSqlInterpolated($@"
INSERT INTO SysConfigMenusPERMATIONS (Id, Menu_ID, Parent_Id, USER_ID, PERMATION, PROJECT_ID, IsShowToUser)
VALUES ({CallSnowflakeIdGenerate.GenerateNewId()}, {id}, {parentId}, {user}, {E("1")}, 2, 1)");
        }

        /// <summary>
        /// P8.4.4 (ai-assistant-p84-plan.md ش20): «شذوذ القيود» تحت «القيود» (ToolStripMenuAccountEntry) بجوار «فحص سلامة القيود»، يفتح
        /// /accounts/entries-anomalies بأسماء اللغات الأربع. للمديرين فقط افتراضياً والباقون من شاشة الصلاحيات. إدراج فقط، لا يتكرر؛
        /// قاعدة بلا أب «القيود» ⇒ لا إدراج (نفس EntriesIntegrityMenu).
        /// </summary>
        private static void AnomaliesMenu(operations opp)
        {
            const string key = AiAnomalyConst.MenuKey;
            const string desc = "شذوذ القيود";
            var menus = opp.db.Database.SqlQueryRaw<StoreTransferPolicy.KeyValueRow>("SELECT Id, KeysData, Value FROM SysConfigMenus").ToList();
            string Key(StoreTransferPolicy.KeyValueRow r) => r.KeysData == null ? "" : Common.DecryptMe(r.KeysData).Trim();
            if (menus.Any(m => Key(m) == key)) return;
            var parent = menus.FirstOrDefault(m => Key(m) == "ToolStripMenuAccountEntry");
            if (parent == null) return;
            string E(string s) => Common.EncryptMe(s);
            long id = menus.Max(m => m.Id) + 1;
            opp.db.Database.ExecuteSqlInterpolated($@"
INSERT INTO SysConfigMenus (Id, Parent_Id, KeysData, Description, Description_En, Description_Fr, Description_DE, PROJECT_ID, Value, DefaultValue, DefaultDescription,
                            POS, Market, Factory, Tourism, AlFawzan, Client_A, Client_B, UserSells, IsShowToUser, RazorPath)
VALUES ({id}, {parent.Id}, {E(key)}, {E(desc)}, {E("Entry anomalies")}, {E("Anomalies des écritures")}, {E("Buchungsauffälligkeiten")}, 2, {E("1")}, {E("1")}, {E(desc)},
        {E("1")}, {E("1")}, {E("1")}, {E("1")}, {E("1")}, {E("1")}, {E("1")}, {E("1")}, 1, {E(AiAnomalyConst.Route)})");
            var admins = opp.db.Database.SqlQueryRaw<long>("SELECT Id AS Value FROM USER_DETAILES WHERE ISNULL(AdminFlage, 0) = 1").ToList();
            foreach (long user in admins)
                opp.db.Database.ExecuteSqlInterpolated($@"
INSERT INTO SysConfigMenusPERMATIONS (Id, Menu_ID, Parent_Id, USER_ID, PERMATION, PROJECT_ID, IsShowToUser)
VALUES ({CallSnowflakeIdGenerate.GenerateNewId()}, {id}, {parent.Id}, {user}, {E("1")}, 2, 1)");
        }

        /// <summary>
        /// P1: عنصر «المساعد الذكى» تحت «ملف» (menuItemFile) يفتح /assistant - نفس نمط «دليل المحاسب» (/help).
        /// الصلاحية للمديرين فقط افتراضياً (المحادثة تستهلك رصيد المزوّد) والباقون من شاشة الصلاحيات. إدراج فقط، لا يتكرر.
        /// </summary>
        private static void AssistantMenu(operations opp)
        {
            const string key = "menuItemAiAssistant";
            const string desc = "المساعد الذكى";
            var menus = opp.db.Database.SqlQueryRaw<StoreTransferPolicy.KeyValueRow>("SELECT Id, KeysData, Value FROM SysConfigMenus").ToList();
            string Key(StoreTransferPolicy.KeyValueRow r) => r.KeysData == null ? "" : Common.DecryptMe(r.KeysData).Trim();
            if (menus.Any(m => Key(m) == key)) return;
            var parent = menus.FirstOrDefault(m => Key(m) == "menuItemFile");
            if (parent == null) return;
            string E(string s) => Common.EncryptMe(s);
            long id = menus.Max(m => m.Id) + 1;
            opp.db.Database.ExecuteSqlInterpolated($@"
INSERT INTO SysConfigMenus (Id, Parent_Id, KeysData, Description, PROJECT_ID, Value, DefaultValue, DefaultDescription,
                            POS, Market, Factory, Tourism, AlFawzan, Client_A, Client_B, UserSells, IsShowToUser, RazorPath)
VALUES ({id}, {parent.Id}, {E(key)}, {E(desc)}, 2, {E("1")}, {E("1")}, {E(desc)},
        {E("1")}, {E("1")}, {E("1")}, {E("1")}, {E("1")}, {E("1")}, {E("1")}, {E("1")}, 1, {E("/assistant")})");
            var admins = opp.db.Database.SqlQueryRaw<long>("SELECT Id AS Value FROM USER_DETAILES WHERE ISNULL(AdminFlage, 0) = 1").ToList();
            foreach (long user in admins)
                opp.db.Database.ExecuteSqlInterpolated($@"
INSERT INTO SysConfigMenusPERMATIONS (Id, Menu_ID, Parent_Id, USER_ID, PERMATION, PROJECT_ID, IsShowToUser)
VALUES ({CallSnowflakeIdGenerate.GenerateNewId()}, {id}, {parent.Id}, {user}, {E("1")}, 2, 1)");
        }

        private static void CreateTable(operations opp, string table, string columns, string indexes)
        {
            if (TableExists(opp, table)) return;
            opp.db.Database.ExecuteSqlRaw($@"
CREATE TABLE [dbo].[{table}](
    [Id] [bigint] NOT NULL,{columns}{Audit},
    CONSTRAINT [PK_{table}] PRIMARY KEY CLUSTERED ([Id] ASC)
);");
            if (!string.IsNullOrWhiteSpace(indexes))
                opp.db.Database.ExecuteSqlRaw(indexes);
        }

        /// <summary>أعمدة أُضيفت بعد إنشاء الجداول (P0.1: الربط بتسجيل الدخول لدى المزوّد). إضافة فقط إن لم توجد، كلها NULL فلا تمس صفاً قائماً.</summary>
        private static readonly (string Table, string Column, string Type)[] Columns =
        {
            ("AI_Providers", "AuthMethod", "[nvarchar](20) NULL"),
            ("AI_Providers", "SignInAt", "[datetime] NULL"),
            ("AI_Providers", "SignInBy", "[bigint] NULL"),
            // P4 (ask_user): حالة حلقة الأدوات المتوقفة على سؤال للمستخدم (الأدوار الجارية + نتائج الدورة) لاستئنافها بعد الإجابة؛ تُمسح عند الإجابة.
            ("AI_Messages", "PendingJson", "[nvarchar](max) NULL"),
            // P6 (جدول المساعد نفسه من P5): المورد/العميل ورقم فاتورة المورد ومصدر المسودة (المرفق) لكشف التكرار.
            ("AI_Drafts", "Party_Id", "[bigint] NULL"),
            ("AI_Drafts", "Ref_No", "[nvarchar](60) NULL"),
            ("AI_Drafts", "Source_Attachment_Id", "[bigint] NULL"),
            // P7.4: استدعاءات أدوات أثناء تشغيل تقييم (بلا محادثة).
            ("AI_ToolCalls_Audit", "EvalRun_Id", "[bigint] NULL"),
        };

        private static void AddedColumns(operations opp)
        {
            foreach (var (table, column, type) in Columns)
            {
                bool missing = opp.db.Database.SqlQueryRaw<int>($"SELECT CASE WHEN COL_LENGTH(N'dbo.{table}', N'{column}') IS NULL THEN 1 ELSE 0 END AS Value").AsEnumerable().First() == 1;
                if (missing) opp.db.Database.ExecuteSqlRaw($"ALTER TABLE [dbo].[{table}] ADD [{column}] {type}");
            }
        }

        /// <summary>يضيف المفاتيح الناقصة فقط؛ قيمة غيّرها المدير لا تُمس.</summary>
        private static void SeedSettings(operations opp)
        {
            foreach (var (key, value) in DefaultSettings)
            {
                int has = opp.db.Database.SqlQuery<int>($"SELECT COUNT(*) AS Value FROM AI_Settings WHERE SettingKey = {key}").AsEnumerable().First();
                if (has > 0) continue;
                opp.db.Database.ExecuteSqlInterpolated($"INSERT INTO AI_Settings (Id, SettingKey, SettingValue) VALUES ({CallSnowflakeIdGenerate.GenerateNewId()}, {key}, {value})");
            }
        }

        /// <summary>
        /// عنصر «إعدادات الذكاء الاصطناعى» بجوار «بيانات المعدلات الضريبية» (TaxInfotoolStripMenuItem) يفتح /settings/ai،
        /// وصلاحيته للمديرين فقط والباقون من شاشة الصلاحيات - نفس نمط InventoryValueMenu. إدراج فقط، لا يتكرر.
        /// </summary>
        private static void SettingsMenu(operations opp)
        {
            const string key = "menuItemAiSettings";
            const string desc = "إعدادات الذكاء الاصطناعى";
            var menus = opp.db.Database.SqlQueryRaw<StoreTransferPolicy.KeyValueRow>("SELECT Id, KeysData, Value FROM SysConfigMenus").ToList();
            string Key(StoreTransferPolicy.KeyValueRow r) => r.KeysData == null ? "" : Common.DecryptMe(r.KeysData).Trim();
            if (menus.Any(m => Key(m) == key)) return;
            var sibling = menus.FirstOrDefault(m => Key(m) == "TaxInfotoolStripMenuItem");
            if (sibling == null) return;
            long parentId = opp.db.Database.SqlQuery<long>($"SELECT CAST(Parent_Id AS bigint) AS Value FROM SysConfigMenus WHERE Id = {sibling.Id}").FirstOrDefault();
            if (parentId == 0) return;
            string E(string s) => Common.EncryptMe(s);
            long id = menus.Max(m => m.Id) + 1;
            opp.db.Database.ExecuteSqlInterpolated($@"
INSERT INTO SysConfigMenus (Id, Parent_Id, KeysData, Description, PROJECT_ID, Value, DefaultValue, DefaultDescription,
                            POS, Market, Factory, Tourism, AlFawzan, Client_A, Client_B, UserSells, IsShowToUser, RazorPath)
VALUES ({id}, {parentId}, {E(key)}, {E(desc)}, 2, {E("1")}, {E("1")}, {E(desc)},
        {E("1")}, {E("1")}, {E("1")}, {E("1")}, {E("1")}, {E("1")}, {E("1")}, {E("1")}, 1, {E("/settings/ai")})");
            var admins = opp.db.Database.SqlQueryRaw<long>("SELECT Id AS Value FROM USER_DETAILES WHERE ISNULL(AdminFlage, 0) = 1").ToList();
            foreach (long user in admins)
                opp.db.Database.ExecuteSqlInterpolated($@"
INSERT INTO SysConfigMenusPERMATIONS (Id, Menu_ID, Parent_Id, USER_ID, PERMATION, PROJECT_ID, IsShowToUser)
VALUES ({CallSnowflakeIdGenerate.GenerateNewId()}, {id}, {parentId}, {user}, {E("1")}, 2, 1)");
        }

        private static bool TableExists(operations opp, string table) =>
            opp.db.Database.SqlQueryRaw<int>($"SELECT CASE WHEN OBJECT_ID(N'dbo.{table}', N'U') IS NULL THEN 0 ELSE 1 END AS Value").AsEnumerable().First() == 1;

        private static void Step(string name, Action step)
        {
            try { step(); }
            catch (Exception ee)
            {
                LastErrors.Add(name + ": " + ee.Message);
                operations.DataDictionaryLogerException(ee);
            }
        }
    }
}
