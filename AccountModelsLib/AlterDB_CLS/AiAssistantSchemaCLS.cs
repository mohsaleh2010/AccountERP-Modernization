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
            Step("AiReadViews", () => AiReadViewsCLS.Ensure(opp));   // P2: schema ai وViews القراءة
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
