using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib
{
    /// <summary>
    /// ترقية قاعدة العميل لمحرك المخزون (خطط 01/02/03): نفس السكربتات المطبَّقة على ClientNew
    /// (stock-engine-phase1-schema.sql، item-opening-phase1-cleanup.sql بلا حذف الأيتام، store-orders-phase1-settings.sql).
    /// كل خطوة آمنة للتكرار (تضيف الناقص فقط) وتُنفَّذ بـ ExecuteSqlRaw مباشرة (لا تمر على محلل insert_me).
    /// تُستدعى عند بدء البرنامج (فحص سريع ثم خروج لو القاعدة محدَّثة) ومن «تحديث قاعدة البيانات» (AlterDB).
    /// لا تحذف ولا تعيد تسمية أى جدول أو عمود قائم.
    /// </summary>
    public static class StockEngineSchemaCLS
    {
        /// <summary>هل كل كائنات المحرك موجودة؟ استعلام واحد خفيف لبدء البرنامج.</summary>
        public static bool IsUpToDate(operations opp)
        {
            return opp.db.Database.SqlQueryRaw<int>(@"
SELECT CASE WHEN OBJECT_ID(N'dbo.Item_Branch_Cost', N'U') IS NOT NULL
             AND OBJECT_ID(N'dbo.Store_Order_Type_Settings', N'U') IS NOT NULL
             AND COL_LENGTH(N'dbo.Order_Purshing', N'Posted_By') IS NOT NULL
             AND COL_LENGTH(N'dbo.Detailes_Purshing_Import_Export', N'Engine_Posted') IS NOT NULL
             AND COL_LENGTH(N'dbo.Item_First_Period_Balance', N'Source') IS NOT NULL
             AND EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_Item_Branch_Cost_Item_Branch')
             AND EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_DPIE_UnitItem_Date')
             AND EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_Item_First_Period_Balance_Period_Line')
        THEN 1 ELSE 0 END AS Value").AsEnumerable().First() == 1
                // أنواع الأذون الـ 22 بيانات لازمة للخدمة: نوع محذوف يُعاد (الإيقاف يكون بـ Is_Active = 0 لا بالحذف).
                && opp.db.Database.SqlQueryRaw<int>(@"
SELECT COUNT(*) AS Value FROM Store_Order_Type_Settings
WHERE Order_Type IN (1001,1002,1003,1004,1005,1006,1007,1008,1009,1010,1012,1013,1014,1015,1016,1017,1018,1019,1020,1021,1022,1023)").AsEnumerable().First() == 22;
        }

        /// <summary>يضيف الناقص من كائنات المحرك. أى خطوة تفشل تُسجَّل وتكمل الباقى.</summary>
        public static void EnsureSchema()
        {
            LastErrors.Clear();
            var opp = new operations();
            try
            {
                if (IsUpToDate(opp)) return;
            }
            catch (Exception ee)
            {
                operations.DataDictionaryLogerException(ee);
                return;
            }
            Step(opp, StockEngine);
            Step(opp, OrderDocCycle);
            Step(opp, StockLedger);
            Step(opp, ItemOpeningBalance);
            Step(opp, StoreOrderTypeSettings);
        }

        /// <summary>أخطاء آخر تشغيل (اسم الخطوة: الرسالة) لعرضها من «تحديث قاعدة البيانات».</summary>
        public static readonly System.Collections.Generic.List<string> LastErrors = new System.Collections.Generic.List<string>();

        private static void Step(operations opp, Action<operations> step)
        {
            try { step(opp); }
            catch (Exception ee)
            {
                LastErrors.Add(step.Method.Name + ": " + ee.Message);
                operations.DataDictionaryLogerException(ee);
            }
        }

        private static void Run(operations opp, string sql) => opp.db.Database.ExecuteSqlRaw(sql);

        private static bool ColumnMissing(operations opp, string table, string column) =>
            opp.db.Database.SqlQueryRaw<int>($"SELECT CASE WHEN COL_LENGTH(N'dbo.{table}', N'{column}') IS NULL THEN 1 ELSE 0 END AS Value").AsEnumerable().First() == 1;

        // ---------------- خطة 01: متوسط التكلفة لكل صنف × فرع ----------------
        private static void StockEngine(operations opp)
        {
            Run(opp, @"
IF OBJECT_ID(N'dbo.Item_Branch_Cost', N'U') IS NULL
CREATE TABLE [dbo].[Item_Branch_Cost](
    [Id] [bigint] NOT NULL,
    [Item_Id] [bigint] NOT NULL,
    [Branch_Id] [bigint] NOT NULL,
    [Avg_Cost] [decimal](18, 6) NOT NULL DEFAULT ((0)),
    [Qty_Small] [decimal](18, 4) NOT NULL DEFAULT ((0)),
    [Last_Purchase_Price] [decimal](18, 6) NULL,
    [Last_Movement_Date] [datetime] NULL,
    [OperationDate] [datetime] NULL,
    [User_Id] [bigint] NULL DEFAULT ((0)),
    [Insert_ServerDateTime] [datetime] NULL DEFAULT (getdate()),
    [Update_ServerDateTime] [datetime] NULL,
    [EditOperationDate] [datetime] NULL,
    [Edit_User_Id] [bigint] NULL,
    [ProcessNo] [bigint] NULL,
    CONSTRAINT [PK_Item_Branch_Cost] PRIMARY KEY CLUSTERED ([Id] ASC)
);");
            Run(opp, @"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_Item_Branch_Cost_Item_Branch')
CREATE UNIQUE NONCLUSTERED INDEX [UX_Item_Branch_Cost_Item_Branch] ON [dbo].[Item_Branch_Cost]([Item_Id] ASC, [Branch_Id] ASC);");
        }

        // ---------------- خطة 01: دورة المستند مسودة ← مرحَّل ← معكوس ----------------
        private static void OrderDocCycle(operations opp)
        {
            bool addingStatus = ColumnMissing(opp, "Order_Purshing", "Doc_Status");
            Run(opp, @"
IF COL_LENGTH(N'dbo.Order_Purshing', N'Doc_Status') IS NULL ALTER TABLE [dbo].[Order_Purshing] ADD [Doc_Status] [int] NULL;
IF COL_LENGTH(N'dbo.Order_Purshing', N'Reversal_Of_Id') IS NULL ALTER TABLE [dbo].[Order_Purshing] ADD [Reversal_Of_Id] [bigint] NULL;
IF COL_LENGTH(N'dbo.Order_Purshing', N'Reversed_By_Id') IS NULL ALTER TABLE [dbo].[Order_Purshing] ADD [Reversed_By_Id] [bigint] NULL;
IF COL_LENGTH(N'dbo.Order_Purshing', N'Reverse_Reason') IS NULL ALTER TABLE [dbo].[Order_Purshing] ADD [Reverse_Reason] [nvarchar](500) NULL;
IF COL_LENGTH(N'dbo.Order_Purshing', N'Posted_Date') IS NULL ALTER TABLE [dbo].[Order_Purshing] ADD [Posted_Date] [datetime] NULL;
IF COL_LENGTH(N'dbo.Order_Purshing', N'Posted_By') IS NULL ALTER TABLE [dbo].[Order_Purshing] ADD [Posted_By] [bigint] NULL;");
            // الأذون القديمة = مرحَّلة؛ مرة واحدة فقط عند إضافة العمود (الدفعة منفصلة لأن العمود جديد).
            if (addingStatus)
                Run(opp, "UPDATE [dbo].[Order_Purshing] SET [Doc_Status] = 1 WHERE [Doc_Status] IS NULL;");
        }

        // ---------------- خطة 01: دفتر الحركة DPIE ----------------
        private static void StockLedger(operations opp)
        {
            Run(opp, @"
IF COL_LENGTH(N'dbo.Detailes_Purshing_Import_Export', N'Unit_Cost') IS NULL ALTER TABLE [dbo].[Detailes_Purshing_Import_Export] ADD [Unit_Cost] [decimal](18, 6) NULL;
IF COL_LENGTH(N'dbo.Detailes_Purshing_Import_Export', N'Qty_Small') IS NULL ALTER TABLE [dbo].[Detailes_Purshing_Import_Export] ADD [Qty_Small] [decimal](18, 4) NULL;
IF COL_LENGTH(N'dbo.Detailes_Purshing_Import_Export', N'Avg_Cost_Before') IS NULL ALTER TABLE [dbo].[Detailes_Purshing_Import_Export] ADD [Avg_Cost_Before] [decimal](18, 6) NULL;
IF COL_LENGTH(N'dbo.Detailes_Purshing_Import_Export', N'Avg_Cost_After') IS NULL ALTER TABLE [dbo].[Detailes_Purshing_Import_Export] ADD [Avg_Cost_After] [decimal](18, 6) NULL;
IF COL_LENGTH(N'dbo.Detailes_Purshing_Import_Export', N'Reversal_Of_Id') IS NULL ALTER TABLE [dbo].[Detailes_Purshing_Import_Export] ADD [Reversal_Of_Id] [bigint] NULL;
IF COL_LENGTH(N'dbo.Detailes_Purshing_Import_Export', N'Engine_Posted') IS NULL ALTER TABLE [dbo].[Detailes_Purshing_Import_Export] ADD [Engine_Posted] [bit] NULL;");
            Run(opp, @"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_DPIE_UnitItem_Date')
CREATE NONCLUSTERED INDEX [IX_DPIE_UnitItem_Date] ON [dbo].[Detailes_Purshing_Import_Export]([Unit_item_Table_Id] ASC, [Process_Date] ASC)
    INCLUDE ([Transaction_Type], [qua], [Part_qua], [Item_No], [Qty_Small]);");
        }

        // ---------------- خطة 02: رصيد أول المدة بالفترة (بلا حذف الأيتام) ----------------
        private static void ItemOpeningBalance(operations opp)
        {
            bool addingPeriod = ColumnMissing(opp, "Item_First_Period_Balance", "Year_Financial_Period_Id");
            if (addingPeriod)
                Run(opp, @"
IF OBJECT_ID(N'dbo.Item_First_Period_Balance_Backup_Engine', N'U') IS NULL
    SELECT * INTO [dbo].[Item_First_Period_Balance_Backup_Engine] FROM [dbo].[Item_First_Period_Balance];");
            Run(opp, @"
IF COL_LENGTH(N'dbo.Item_First_Period_Balance', N'Year_Financial_Period_Id') IS NULL ALTER TABLE [dbo].[Item_First_Period_Balance] ADD [Year_Financial_Period_Id] [bigint] NULL;
IF COL_LENGTH(N'dbo.Item_First_Period_Balance', N'Store_Id') IS NULL ALTER TABLE [dbo].[Item_First_Period_Balance] ADD [Store_Id] [bigint] NULL;
IF COL_LENGTH(N'dbo.Item_First_Period_Balance', N'Unit_Cost') IS NULL ALTER TABLE [dbo].[Item_First_Period_Balance] ADD [Unit_Cost] [decimal](18, 6) NULL;
IF COL_LENGTH(N'dbo.Item_First_Period_Balance', N'Source') IS NULL ALTER TABLE [dbo].[Item_First_Period_Balance] ADD [Source] [int] NULL;");
            if (addingPeriod)
                Run(opp, @"
UPDATE b SET
    b.Year_Financial_Period_Id = (SELECT TOP 1 p.Id FROM [dbo].[Financial_Period] p
                                  WHERE b.Balance_Date >= p.DateFrom AND b.Balance_Date < DATEADD(day, 1, p.DateTo)),
    b.Store_Id  = u.Store_id,
    b.Unit_Cost = ISNULL(b.P_priceAvgCost, 0) / CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END,
    b.Source    = 0
FROM [dbo].[Item_First_Period_Balance] b
INNER JOIN [dbo].[Unit_item] u ON u.id = b.Unit_Item_Id
WHERE b.Year_Financial_Period_Id IS NULL;");
            // فهرس فريد مُفلتر: يُنشأ فقط لو لا تكرار (فترة × سطر)؛ وإلا يُسجَّل للمراجعة اليدوية ولا يوقف الباقى.
            Run(opp, @"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_Item_First_Period_Balance_Period_Line')
CREATE UNIQUE NONCLUSTERED INDEX [UX_Item_First_Period_Balance_Period_Line]
    ON [dbo].[Item_First_Period_Balance]([Year_Financial_Period_Id] ASC, [Unit_Item_Id] ASC)
    WHERE [Year_Financial_Period_Id] IS NOT NULL;");
        }

        // ---------------- خطة 03: إعداد أنواع الأذون ----------------
        private static void StoreOrderTypeSettings(operations opp)
        {
            Run(opp, @"
IF OBJECT_ID(N'dbo.Store_Order_Type_Settings', N'U') IS NULL
CREATE TABLE [dbo].[Store_Order_Type_Settings](
    [Order_Type] [int] NOT NULL,
    [Name_Ar] [nvarchar](200) NOT NULL,
    [Direction] [int] NOT NULL,
    [Store_Side] [int] NOT NULL,
    [From_Party_Types] [nvarchar](50) NOT NULL,
    [Default_From_Party] [int] NOT NULL,
    [To_Party_Types] [nvarchar](50) NOT NULL,
    [Default_To_Party] [int] NOT NULL,
    [Is_Virtual_Outer_From] [bit] NOT NULL DEFAULT ((0)),
    [Is_Virtual_Outer_To] [bit] NOT NULL DEFAULT ((0)),
    [Price_Source] [int] NOT NULL DEFAULT ((1)),
    [Updates_Avg_Cost] [bit] NOT NULL DEFAULT ((0)),
    [Is_Purchase] [bit] NOT NULL DEFAULT ((0)),
    [Is_Transfer] [bit] NOT NULL DEFAULT ((0)),
    [Is_Active] [bit] NOT NULL DEFAULT ((1)),
    [OperationDate] [datetime] NULL,
    [User_Id] [bigint] NULL DEFAULT ((0)),
    [Insert_ServerDateTime] [datetime] NULL DEFAULT (getdate()),
    [Update_ServerDateTime] [datetime] NULL,
    [EditOperationDate] [datetime] NULL,
    [Edit_User_Id] [bigint] NULL,
    CONSTRAINT [PK_Store_Order_Type_Settings] PRIMARY KEY CLUSTERED ([Order_Type] ASC)
);");
            // يُدرج الأنواع الناقصة فقط؛ الصفوف القائمة (وتعديلات العميل عليها) لا تُلمس.
            Run(opp, @"
;WITH s AS (
SELECT * FROM (VALUES
 (1001, N'إذن صرف / تحويل لمخزن أو قسم',                      3, 1, N'4,20', 4,  N'4,20', 4,  0, 0, 1, 0, 0, 1),
 (1002, N'إذن صرف لعميل',                                     2, 1, N'4,20', 4,  N'2',    2,  0, 0, 1, 0, 0, 0),
 (1003, N'إذن مرتجع صرف من عميل',                             1, 2, N'2',    2,  N'4,20', 4,  0, 0, 1, 0, 0, 0),
 (1004, N'إذن إستلام من مورد',                                1, 2, N'1',    1,  N'4,20', 4,  0, 0, 3, 1, 1, 0),
 (1005, N'إذن مرتجع لمورد',                                   2, 1, N'4,20', 4,  N'1',    1,  0, 0, 1, 0, 0, 0),
 (1006, N'إذن مرتجع صرف / تحويل لمخزن أو قسم',                3, 1, N'4,20', 4,  N'4,20', 4,  0, 0, 1, 0, 0, 1),
 (1007, N'إذن تسوية بالزيادة',                                1, 2, N'7',    7,  N'4,20', 4,  0, 0, 1, 0, 0, 0),
 (1008, N'إذن تسوية بالنقص',                                  2, 1, N'4,20', 4,  N'8',    8,  0, 0, 1, 0, 0, 0),
 (1009, N'إذن إستلام من الإنتاج',                             1, 2, N'9',    9,  N'4,20', 4,  0, 0, 1, 0, 0, 0),
 (1010, N'إذن صرف إلى الإنتاج / التشغيل',                     2, 1, N'4,20', 4,  N'10',   10, 0, 0, 1, 0, 0, 0),
 (1012, N'إذن صرف قطع غيار',                                  2, 1, N'4,20', 4,  N'12',   12, 0, 0, 1, 0, 0, 0),
 (1013, N'صرف للهالك',                                        2, 1, N'4,20', 4,  N'3',    3,  0, 0, 1, 0, 0, 0),
 (1014, N'إذن إستلام من مخزن خارجى',                          1, 2, N'4,20', 4,  N'4,20', 4,  1, 0, 1, 0, 0, 0),
 (1015, N'إذن صرف إلى مخزن خارجى',                            2, 1, N'4,20', 4,  N'4,20', 4,  0, 1, 1, 0, 0, 0),
 (1016, N'إذن إستلام من المخزن',                              1, 1, N'4,20', 4,  N'4,20', 4,  0, 0, 1, 0, 0, 1),
 (1017, N'إذن صرف إلى المخزن',                                2, 1, N'4,20', 4,  N'4,20', 4,  0, 0, 1, 0, 0, 1),
 (1018, N'إذن صرف للورشة',                                    2, 1, N'4,20', 4,  N'18',   18, 0, 0, 1, 0, 0, 0),
 (1019, N'إذن مرتجع صرف من الورشة',                           1, 2, N'19',   19, N'4,20', 4,  0, 0, 1, 0, 0, 0),
 (1020, N'إذن صرف لموظف',                                     2, 1, N'4,20', 4,  N'40',   40, 0, 0, 1, 0, 0, 0),
 (1021, N'إذن إستلام من موظف',                                1, 2, N'40',   40, N'4,20', 4,  0, 0, 1, 0, 0, 0),
 (1022, N'إذن صرف قطع الغيار للماكينات وخطوط الإنتاج',        2, 1, N'4,20', 4,  N'23',   23, 0, 0, 1, 0, 0, 0),
 (1023, N'إذن مرتجع صرف قطع الغيار للماكينات وخطوط الإنتاج',  1, 2, N'23',   23, N'4,20', 4,  0, 0, 1, 0, 0, 0)
) v(Order_Type, Name_Ar, Direction, Store_Side, From_Party_Types, Default_From_Party, To_Party_Types, Default_To_Party,
    Is_Virtual_Outer_From, Is_Virtual_Outer_To, Price_Source, Updates_Avg_Cost, Is_Purchase, Is_Transfer))
INSERT INTO [dbo].[Store_Order_Type_Settings]
    (Order_Type, Name_Ar, Direction, Store_Side, From_Party_Types, Default_From_Party, To_Party_Types, Default_To_Party,
     Is_Virtual_Outer_From, Is_Virtual_Outer_To, Price_Source, Updates_Avg_Cost, Is_Purchase, Is_Transfer, OperationDate)
SELECT s.Order_Type, s.Name_Ar, s.Direction, s.Store_Side, s.From_Party_Types, s.Default_From_Party, s.To_Party_Types, s.Default_To_Party,
       s.Is_Virtual_Outer_From, s.Is_Virtual_Outer_To, s.Price_Source, s.Updates_Avg_Cost, s.Is_Purchase, s.Is_Transfer, CAST(GETDATE() AS date)
FROM s WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Store_Order_Type_Settings] t WHERE t.Order_Type = s.Order_Type);");
        }
    }
}
