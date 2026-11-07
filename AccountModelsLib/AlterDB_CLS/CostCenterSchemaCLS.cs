using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib
{
    /// <summary>
    /// ترقية قاعدة العميل لخطة مراكز التكلفة (cost-centers-razor-plan.md §4، المرحلة 0): إضافات فقط، آمنة للتكرار، بلا UPDATE/DELETE لأى صف قائم.
    /// نفس السكربت فى cost-centers-phase0-schema.sql (مع rollback). تُستدعى من StockEngineSchemaCLS.EnsureSchema فتصل لكل عميل مع ترقية القاعدة.
    /// </summary>
    public static class CostCenterSchemaCLS
    {
        public const string ValidationModeKey = "CostCenter_Validation_Mode";

        public static void Ensure(operations opp, Action<operations, Action<operations>> step)
        {
            step(opp, PolicyColumn);
            step(opp, ProjectTable);
            step(opp, StateTable);
            step(opp, ValidationModeSetting);
            step(opp, InvoiceHeaderColumns);
            step(opp, AssetColumn);
            step(opp, DepreciationTables);
        }

        /// <summary>مركز تكلفة الأصل الثابت (م3 دفعة 5): عمود NULL على FixedAssetData؛ NULL = بلا مركز.</summary>
        private static void AssetColumn(operations opp)
        {
            opp.db.Database.ExecuteSqlRaw(@"
IF OBJECT_ID(N'dbo.FixedAssetData', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.FixedAssetData', N'Cost_Center_Id') IS NULL
    ALTER TABLE [dbo].[FixedAssetData] ADD [Cost_Center_Id] [bigint] NULL;");
        }

        /// <summary>إعداد قيد الإهلاك لكل فرع (مغلق افتراضياً) وربط سجل الإهلاك بقيده.</summary>
        private static void DepreciationTables(operations opp)
        {
            opp.db.Database.ExecuteSqlRaw(@"
IF OBJECT_ID(N'dbo.Fixed_Asset_Depreciation_Setting', N'U') IS NULL
CREATE TABLE [dbo].[Fixed_Asset_Depreciation_Setting](
    [Account_Branch_Id] [bigint] NOT NULL,
    [Post_Entries] [bit] NOT NULL DEFAULT ((0)),
    [Expense_Account_Id] [bigint] NULL,
    [Accumulated_Account_Id] [bigint] NULL,
    [User_Id] [bigint] NULL,
    [Update_ServerDateTime] [datetime] NULL DEFAULT (getdate()),
    CONSTRAINT [PK_Fixed_Asset_Depreciation_Setting] PRIMARY KEY CLUSTERED ([Account_Branch_Id] ASC)
);
IF OBJECT_ID(N'dbo.Fixed_Asset_Depreciation_Entry', N'U') IS NULL
CREATE TABLE [dbo].[Fixed_Asset_Depreciation_Entry](
    [Ehlak_Id] [bigint] NOT NULL,
    [Entry_Id] [bigint] NOT NULL,
    [Insert_ServerDateTime] [datetime] NULL DEFAULT (getdate()),
    CONSTRAINT [PK_Fixed_Asset_Depreciation_Entry] PRIMARY KEY CLUSTERED ([Ehlak_Id] ASC)
);");
        }

        /// <summary>مركز رأس المستند (ق9): عمود NULL على رؤوس فواتير/مرتجعات المبيعات والمشتريات وأذون المخازن (Order_Purshing)؛ NULL = بلا مركز، فلا يتغير أى صف قديم.</summary>
        private static void InvoiceHeaderColumns(operations opp)
        {
            foreach (var t in new[] { "Invoices_List", "Invoices_List_Reverse", "Bills_List", "Bills_List_Reverse", "Order_Purshing" })
            {
                opp.db.Database.ExecuteSqlRaw($@"
IF OBJECT_ID(N'dbo.{t}', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.{t}', N'Cost_Center_Id') IS NULL
    ALTER TABLE [dbo].[{t}] ADD [Cost_Center_Id] [bigint] NULL;");
            }
        }

        /// <summary>Submain_Account.Cost_Center_Policy: NULL = اقرأ القديم (Accept_Cost_Center_Flage)، 0 ممنوع، 1 اختيارى، 2 إلزامى (ق4).</summary>
        private static void PolicyColumn(operations opp)
        {
            opp.db.Database.ExecuteSqlRaw(@"
IF COL_LENGTH(N'dbo.Submain_Account', N'Cost_Center_Policy') IS NULL
    ALTER TABLE [dbo].[Submain_Account] ADD [Cost_Center_Policy] [tinyint] NULL;");
        }

        /// <summary>خصائص المشروع لعقدة Cost_Type=1 (ق6): عقد واحد لكل مركز.</summary>
        private static void ProjectTable(operations opp)
        {
            opp.db.Database.ExecuteSqlRaw(@"
IF OBJECT_ID(N'dbo.Cost_Center_Project', N'U') IS NULL
CREATE TABLE [dbo].[Cost_Center_Project](
    [Cost_Center_Id] [bigint] NOT NULL,
    [Customer_Id] [bigint] NULL,
    [Contract_Value] [decimal](18, 4) NOT NULL DEFAULT ((0)),
    [Start_Date] [datetime] NULL,
    [End_Date] [datetime] NULL,
    [Status] [tinyint] NOT NULL DEFAULT ((1)),
    [Manager_User_Id] [bigint] NULL,
    [Retention_Percent] [decimal](9, 4) NOT NULL DEFAULT ((0)),
    [Currency_Id] [int] NULL,
    [Notes] [nvarchar](1024) NULL,
    [Insert_ServerDateTime] [datetime] NULL DEFAULT (getdate()),
    [User_Id] [bigint] NULL,
    [EditOperationDate] [datetime] NULL,
    [Edit_User_Id] [bigint] NULL,
    CONSTRAINT [PK_Cost_Center_Project] PRIMARY KEY CLUSTERED ([Cost_Center_Id] ASC)
);");
        }

        /// <summary>حالة أى عقدة (ق7): لا صف = نشط. Status: 1 نشط، 2 موقوف (لا ترحيل جديد)، 3 مغلق (لا ترحيل جديد).</summary>
        private static void StateTable(operations opp)
        {
            opp.db.Database.ExecuteSqlRaw(@"
IF OBJECT_ID(N'dbo.Cost_Center_State', N'U') IS NULL
CREATE TABLE [dbo].[Cost_Center_State](
    [Cost_Center_Id] [bigint] NOT NULL,
    [Status] [tinyint] NOT NULL DEFAULT ((1)),
    [Changed_Date] [datetime] NULL DEFAULT (getdate()),
    [Changed_User_Id] [bigint] NULL,
    CONSTRAINT [PK_Cost_Center_State] PRIMARY KEY CLUSTERED ([Cost_Center_Id] ASC)
);");
        }

        /// <summary>إعداد CostCenter_Validation_Mode (0 تحذير، 1 رفض) بالقيمة 0؛ يُنشأ مرة واحدة ولا يغيّر قيمة عدّلها العميل. النصوص مشفَّرة كباقى SysConfig.</summary>
        private static void ValidationModeSetting(operations opp)
        {
            var cfg = opp.db.Database.SqlQueryRaw<StoreTransferPolicy.KeyValueRow>("SELECT Id, KeysData, Value FROM SysConfig WHERE KeysData IS NOT NULL").ToList();
            if (cfg.Any(r => Common.DecryptMe(r.KeysData).Trim() == ValidationModeKey)) return;
            long id = cfg.Count == 0 ? 1 : cfg.Max(r => r.Id) + 1;
            string desc = "التحقق من مراكز التكلفة فى القيود (إلزامى/غير مربوط/غير ورقى/مغلق): 0 تحذير فقط، 1 رفض الحفظ";
            string E(string s) => Common.EncryptMe(s);
            opp.db.Database.ExecuteSqlInterpolated($@"
INSERT INTO SysConfig (Id, KeysData, DefaultValue, Value, DefaultDescription, Description, Client_B, Client_A, POS, Market, Factory, AlFawzan, Tourism)
VALUES ({id}, {E(ValidationModeKey)}, {E("0")}, {E("0")}, {E(desc)}, {E(desc)}, {E("0")}, {E("0")}, {E("0")}, {E("0")}, {E("0")}, {E("0")}, {E("0")})");
        }
    }
}
