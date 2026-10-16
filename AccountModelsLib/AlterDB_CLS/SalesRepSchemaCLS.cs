using System;
using System.Linq;
using AccountModelsLib.SalesRep;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib
{
    /// <summary>
    /// ترقية قاعدة العميل لموديول المندوبين (field-sales-van-presales-plan.md §5.1، بعد توافق خطة 06).
    /// كل جداول الموديول جديدة ببادئة SalesRep_ (ق2)، معرّفها bigint بـ Snowflake، وعليها أعمدة التدقيق القياسية.
    /// لا تمس أى جدول قائم من النظام. تُستدعى عند بدء البرنامج ومن «تحديث قاعدة البيانات» (AlterDB) وهى آمنة للتكرار.
    /// تصحيح 2026-10-16: نسخة سابقة غير معتمدة من هذه الجداول (بلا Update_ServerDateTime) أُنشئت على ClientNew
    /// ناقصة الأعمدة؛ الجدول القديم الشكل يُحذف ويُعاد إنشاؤه **فقط إن كان فارغاً**، وإلا يُترك ويُسجَّل خطأ.
    /// </summary>
    public static class SalesRepSchemaCLS
    {
        /// <summary>أعمدة التدقيق القياسية (نفس أسماء جداول النظام).</summary>
        private const string Audit = @"
    [User_Id] [bigint] NULL,
    [OperationDate] [datetime] NULL,
    [Insert_ServerDateTime] [datetime] NULL DEFAULT (getdate()),
    [Update_ServerDateTime] [datetime] NULL,
    [Edit_User_Id] [bigint] NULL,
    [EditOperationDate] [datetime] NULL,
    [Branch_Id] [bigint] NULL,
    [ProcessNo] [bigint] NULL";

        /// <summary>(اسم الجدول، تعريف الأعمدة بعد Id، فهارس إضافية).</summary>
        private static readonly (string Table, string Columns, string Indexes)[] Tables =
        {
            ("SalesRep_Reps", @"
    [Employee_Id] [bigint] NOT NULL,
    [Van_Store_Id] [bigint] NOT NULL,
    [Damaged_Store_Id] [bigint] NOT NULL,
    [Cash_Box_Id] [bigint] NOT NULL,
    [Sales_Mode] [int] NOT NULL DEFAULT ((0)),
    [Price_Type_List_ID] [bigint] NULL,
    [Max_Discount_Rate] [decimal](9, 4) NULL,
    [Max_Cash_On_Hand] [decimal](18, 4) NULL,
    [Allow_Credit_Sales] [bit] NOT NULL DEFAULT ((0)),
    [Allow_Blind_Return] [bit] NOT NULL DEFAULT ((0)),
    [Return_Window_Days] [int] NULL,
    [End_Of_Day_Stock_Policy] [int] NOT NULL DEFAULT ((0)),
    [Cost_Center_Id] [bigint] NULL,
    [Commission_Plan_Id] [bigint] NULL,
    [Supervisor_Employee_Id] [bigint] NULL,
    [Active] [bit] NOT NULL DEFAULT ((1)),",
             @"CREATE UNIQUE NONCLUSTERED INDEX [UX_SalesRep_Reps_Employee] ON [dbo].[SalesRep_Reps]([Employee_Id]);
CREATE UNIQUE NONCLUSTERED INDEX [UX_SalesRep_Reps_Van_Store] ON [dbo].[SalesRep_Reps]([Van_Store_Id]);
CREATE UNIQUE NONCLUSTERED INDEX [UX_SalesRep_Reps_Damaged_Store] ON [dbo].[SalesRep_Reps]([Damaged_Store_Id]);"),

            ("SalesRep_Devices", @"
    [Rep_Id] [bigint] NOT NULL,
    [Device_Uid] [nvarchar](200) NOT NULL,
    [Platform] [nvarchar](50) NULL,
    [Last_Sync_At] [datetime] NULL,
    [Is_Blocked] [bit] NOT NULL DEFAULT ((0)),
    [Number_Range_From] [bigint] NULL,
    [Number_Range_To] [bigint] NULL,
    [Number_Range_Next] [bigint] NULL,",
             @"CREATE UNIQUE NONCLUSTERED INDEX [UX_SalesRep_Devices_Uid] ON [dbo].[SalesRep_Devices]([Device_Uid]);"),

            ("SalesRep_Routes", @"
    [Name] [nvarchar](200) NOT NULL,
    [Rep_Id] [bigint] NOT NULL,
    [Area_Id] [bigint] NULL,
    [Active] [bit] NOT NULL DEFAULT ((1)),", ""),

            ("SalesRep_Route_Customers", @"
    [Route_Id] [bigint] NOT NULL,
    [Customer_Id] [bigint] NOT NULL,
    [Visit_Day] [int] NOT NULL,
    [Frequency] [int] NOT NULL DEFAULT ((1)),
    [Week_Parity] [int] NULL,
    [Sequence] [int] NOT NULL DEFAULT ((0)),",
             @"CREATE UNIQUE NONCLUSTERED INDEX [UX_SalesRep_Route_Customers] ON [dbo].[SalesRep_Route_Customers]([Route_Id], [Customer_Id]);"),

            ("SalesRep_Customer_Geo", @"
    [Customer_Id] [bigint] NOT NULL,
    [Latitude] [decimal](9, 6) NOT NULL,
    [Longitude] [decimal](9, 6) NOT NULL,
    [Geofence_Radius_M] [int] NOT NULL DEFAULT ((100)),
    [Verified_By] [bigint] NULL,
    [Verified_At] [datetime] NULL,",
             @"CREATE UNIQUE NONCLUSTERED INDEX [UX_SalesRep_Customer_Geo] ON [dbo].[SalesRep_Customer_Geo]([Customer_Id]);"),

            // ق7: المجموعة والعميل معاً، والأخص (العميل) يغلب — صف واحد لكل منهما بالضبط.
            ("SalesRep_Credit_Policy", @"
    [Customer_Id] [bigint] NULL,
    [Group_Id] [bigint] NULL,
    [Credit_Limit] [decimal](18, 4) NOT NULL DEFAULT ((0)),
    [Max_Overdue_Days] [int] NULL,
    [Block_On_Overdue] [bit] NOT NULL DEFAULT ((1)),
    CONSTRAINT [CK_SalesRep_Credit_Policy_Scope] CHECK (([Customer_Id] IS NULL AND [Group_Id] IS NOT NULL) OR ([Customer_Id] IS NOT NULL AND [Group_Id] IS NULL)),",
             @"CREATE UNIQUE NONCLUSTERED INDEX [UX_SalesRep_Credit_Policy_Customer] ON [dbo].[SalesRep_Credit_Policy]([Customer_Id]) WHERE [Customer_Id] IS NOT NULL;
CREATE UNIQUE NONCLUSTERED INDEX [UX_SalesRep_Credit_Policy_Group] ON [dbo].[SalesRep_Credit_Policy]([Group_Id]) WHERE [Group_Id] IS NOT NULL;"),

            ("SalesRep_Discount_Policy", @"
    [Scope] [int] NOT NULL,
    [Scope_Id] [bigint] NOT NULL,
    [Max_Discount_Rate] [decimal](9, 4) NOT NULL DEFAULT ((0)),
    [Requires_Approval_Above] [decimal](9, 4) NULL,",
             @"CREATE UNIQUE NONCLUSTERED INDEX [UX_SalesRep_Discount_Policy] ON [dbo].[SalesRep_Discount_Policy]([Scope], [Scope_Id]);"),

            // وردية واحدة غير مقفلة لكل مندوب (§6.1 بند 5) يفرضها فهرس مُرشَّح.
            ("SalesRep_Shifts", @"
    [Rep_Id] [bigint] NOT NULL,
    [Shift_Date] [date] NOT NULL,
    [Status] [int] NOT NULL,
    [Load_Order_Id] [bigint] NULL,
    [Unload_Order_Id] [bigint] NULL,
    [Damaged_Transfer_Order_Id] [bigint] NULL,
    [Opened_At] [datetime] NULL,
    [Closed_At] [datetime] NULL,
    [Closed_By] [bigint] NULL,
    [Cash_Expected] [decimal](18, 4) NULL,
    [Cash_Counted] [decimal](18, 4) NULL,
    [Cash_Diff] [decimal](18, 4) NULL,
    [Stock_Diff_Value] [decimal](18, 4) NULL,
    [Notes] [nvarchar](1000) NULL,",
             @"CREATE UNIQUE NONCLUSTERED INDEX [UX_SalesRep_Shifts_One_Open] ON [dbo].[SalesRep_Shifts]([Rep_Id]) WHERE [Status] <> 3;"),

            ("SalesRep_Shift_Stock", @"
    [Shift_Id] [bigint] NOT NULL,
    [Unit_Item_Table_Id] [bigint] NOT NULL,
    [Opening] [decimal](18, 4) NOT NULL DEFAULT ((0)),
    [Loaded] [decimal](18, 4) NOT NULL DEFAULT ((0)),
    [Sold] [decimal](18, 4) NOT NULL DEFAULT ((0)),
    [Delivered] [decimal](18, 4) NOT NULL DEFAULT ((0)),
    [Returned_Good] [decimal](18, 4) NOT NULL DEFAULT ((0)),
    [Returned_Damaged] [decimal](18, 4) NOT NULL DEFAULT ((0)),
    [Unloaded] [decimal](18, 4) NOT NULL DEFAULT ((0)),
    [Other_Movement] [decimal](18, 4) NOT NULL DEFAULT ((0)),
    [Expected_Closing] [decimal](18, 4) NOT NULL DEFAULT ((0)),
    [Counted_Closing] [decimal](18, 4) NULL,
    [Diff] [decimal](18, 4) NULL,
    [Unit_Cost] [decimal](18, 6) NULL,
    [Diff_Order_Id] [bigint] NULL,",
             @"CREATE UNIQUE NONCLUSTERED INDEX [UX_SalesRep_Shift_Stock] ON [dbo].[SalesRep_Shift_Stock]([Shift_Id], [Unit_Item_Table_Id]);"),

            ("SalesRep_Shift_Cash", @"
    [Shift_Id] [bigint] NOT NULL,
    [Payment_Method_Id] [bigint] NOT NULL,
    [Expected] [decimal](18, 4) NOT NULL DEFAULT ((0)),
    [Counted] [decimal](18, 4) NULL,
    [Diff] [decimal](18, 4) NULL,
    [Remittance_Voucher_Id] [bigint] NULL,
    [Diff_Voucher_Id] [bigint] NULL,",
             @"CREATE UNIQUE NONCLUSTERED INDEX [UX_SalesRep_Shift_Cash] ON [dbo].[SalesRep_Shift_Cash]([Shift_Id], [Payment_Method_Id]);"),

            // ق11: ربط أى مستند ERP بالوردية والزيارة بدل أعمدة على Invoices_List.
            ("SalesRep_Document_Links", @"
    [Doc_Type] [int] NOT NULL,
    [Doc_Id] [bigint] NOT NULL,
    [Shift_Id] [bigint] NOT NULL,
    [Visit_Id] [bigint] NULL,
    [Doc_Guid] [uniqueidentifier] NULL,",
             @"CREATE UNIQUE NONCLUSTERED INDEX [UX_SalesRep_Document_Links_Doc] ON [dbo].[SalesRep_Document_Links]([Doc_Type], [Doc_Id]);
CREATE UNIQUE NONCLUSTERED INDEX [UX_SalesRep_Document_Links_Guid] ON [dbo].[SalesRep_Document_Links]([Doc_Guid]) WHERE [Doc_Guid] IS NOT NULL;
CREATE NONCLUSTERED INDEX [IX_SalesRep_Document_Links_Shift] ON [dbo].[SalesRep_Document_Links]([Shift_Id]);"),

            ("SalesRep_Visits", @"
    [Shift_Id] [bigint] NOT NULL,
    [Customer_Id] [bigint] NOT NULL,
    [Planned] [bit] NOT NULL DEFAULT ((1)),
    [Check_In_At] [datetime] NULL,
    [Check_In_Lat] [decimal](9, 6) NULL,
    [Check_In_Lng] [decimal](9, 6) NULL,
    [Check_Out_At] [datetime] NULL,
    [Distance_From_Customer_M] [int] NULL,
    [Inside_Geofence] [bit] NULL,
    [Result] [int] NULL,
    [No_Sale_Reason_Id] [bigint] NULL,
    [Notes] [nvarchar](1000) NULL,
    [Photo_Id] [bigint] NULL,",
             @"CREATE NONCLUSTERED INDEX [IX_SalesRep_Visits_Shift] ON [dbo].[SalesRep_Visits]([Shift_Id]);"),

            ("Lookup_SalesRep_NoSaleReasons", @"
    [Name] [nvarchar](200) NOT NULL,
    [Requires_Photo] [bit] NOT NULL DEFAULT ((0)),
    [Active] [bit] NOT NULL DEFAULT ((1)),", ""),

            ("Lookup_SalesRep_ReturnReasons", @"
    [Name] [nvarchar](200) NOT NULL,
    [Is_Damaged] [bit] NOT NULL DEFAULT ((0)),
    [Requires_Photo] [bit] NOT NULL DEFAULT ((0)),
    [Requires_Batch] [bit] NOT NULL DEFAULT ((0)),
    [Active] [bit] NOT NULL DEFAULT ((1)),", ""),

            // §6.6: المسموح بإرجاعه يُحسب من هذا الجدول؛ المرتجع بلا فاتورة (ق15) بلا سطر أصلى ويُعلَّم Is_Blind.
            ("SalesRep_Invoice_Return_Links", @"
    [Return_Detail_Id] [bigint] NOT NULL,
    [Original_Detail_Id] [bigint] NULL,
    [Original_Invoice_Id] [bigint] NULL,
    [Is_Blind] [bit] NOT NULL DEFAULT ((0)),
    [Qty_Large] [decimal](18, 4) NOT NULL DEFAULT ((0)),
    [Qty_Small] [decimal](18, 4) NOT NULL DEFAULT ((0)),
    [Qty_ItemNo] [decimal](18, 4) NOT NULL DEFAULT ((0)),
    [Net_Unit_Price] [decimal](18, 6) NOT NULL DEFAULT ((0)),
    [Unit_Cost] [decimal](18, 6) NULL,
    [Reason_Id] [bigint] NULL,
    [Condition] [int] NOT NULL DEFAULT ((0)),
    [Unit_Item_ExpireDate_Id] [bigint] NULL,
    [Photo_Id] [bigint] NULL,",
             @"CREATE NONCLUSTERED INDEX [IX_SalesRep_Return_Links_Original] ON [dbo].[SalesRep_Invoice_Return_Links]([Original_Detail_Id]);"),

            ("SalesRep_Approvals", @"
    [Type] [int] NOT NULL,
    [Ref_Document_Guid] [uniqueidentifier] NULL,
    [Ref_Doc_Type] [int] NULL,
    [Ref_Doc_Id] [bigint] NULL,
    [Requested_By] [bigint] NOT NULL,
    [Approver_Id] [bigint] NULL,
    [Status] [int] NOT NULL DEFAULT ((0)),
    [Otp_Hash] [nvarchar](200) NULL,
    [Reason] [nvarchar](1000) NULL,
    [Decision_Notes] [nvarchar](1000) NULL,
    [Decided_At] [datetime] NULL,
    [Rep_Id] [bigint] NULL,
    [Shift_Id] [bigint] NULL,
    [Customer_Id] [bigint] NULL,
    [Requested_Value] [decimal](18, 4) NULL,
    [Allowed_Value] [decimal](18, 4) NULL,
    [Approved_Value] [decimal](18, 4) NULL,",
             @"CREATE NONCLUSTERED INDEX [IX_SalesRep_Approvals_Status] ON [dbo].[SalesRep_Approvals]([Status]);
CREATE NONCLUSTERED INDEX [IX_SalesRep_Approvals_Guid] ON [dbo].[SalesRep_Approvals]([Ref_Document_Guid], [Type]);"),

            ("SalesRep_GPS_Track", @"
    [Rep_Id] [bigint] NOT NULL,
    [Shift_Id] [bigint] NULL,
    [Tracked_At] [datetime] NOT NULL,
    [Lat] [decimal](9, 6) NOT NULL,
    [Lng] [decimal](9, 6) NOT NULL,
    [Accuracy] [decimal](9, 2) NULL,
    [Battery] [int] NULL,",
             @"CREATE NONCLUSTERED INDEX [IX_SalesRep_GPS_Track_Rep] ON [dbo].[SalesRep_GPS_Track]([Rep_Id], [Tracked_At]);"),

            // مبدأ 5: Doc_Guid فريد فإعادة الإرسال لا تكرر الأثر.
            ("SalesRep_Sync_Inbox", @"
    [Device_Id] [bigint] NOT NULL,
    [Doc_Guid] [uniqueidentifier] NOT NULL,
    [Doc_Type] [int] NOT NULL,
    [Payload] [nvarchar](max) NULL,
    [Status] [int] NOT NULL DEFAULT ((0)),
    [Error] [nvarchar](max) NULL,
    [Result_Ref_Id] [bigint] NULL,
    [Received_At] [datetime] NOT NULL DEFAULT (getdate()),
    [Processed_At] [datetime] NULL,",
             @"CREATE UNIQUE NONCLUSTERED INDEX [UX_SalesRep_Sync_Inbox_Guid] ON [dbo].[SalesRep_Sync_Inbox]([Doc_Guid]);"),

            // الصورة فى القاعدة كبقية النظام (أو مسار لاحقاً).
            ("SalesRep_Attachments", @"
    [Ref_Type] [int] NOT NULL,
    [Ref_Id] [bigint] NOT NULL,
    [File_Data] [varbinary](max) NULL,
    [File_Path] [nvarchar](500) NULL,
    [Content_Type] [nvarchar](100) NULL,
    [Taken_At] [datetime] NULL,
    [Lat] [decimal](9, 6) NULL,
    [Lng] [decimal](9, 6) NULL,", ""),

            ("SalesRep_Targets", @"
    [Rep_Id] [bigint] NOT NULL,
    [Period_From] [date] NOT NULL,
    [Period_To] [date] NOT NULL,
    [Scope] [int] NOT NULL DEFAULT ((0)),
    [Scope_Id] [bigint] NULL,
    [Target_Qty] [decimal](18, 4) NULL,
    [Target_Value] [decimal](18, 4) NULL,", ""),

            ("SalesRep_Commission_Plans", @"
    [Name] [nvarchar](200) NOT NULL,
    [Deduct_Returns] [bit] NOT NULL DEFAULT ((1)),
    [Collection_Speed_Days] [int] NULL,
    [Collection_Speed_Bonus_Rate] [decimal](9, 4) NULL,
    [Active] [bit] NOT NULL DEFAULT ((1)),", ""),

            // ق13: أربعة مكوّنات (صافى المبيعات، التحصيل، الهدف الإجمالى، هدف الصنف/المجموعة) لكل منها شرائح.
            ("SalesRep_Commission_Plan_Tiers", @"
    [Plan_Id] [bigint] NOT NULL,
    [Component] [int] NOT NULL,
    [Achievement_From] [decimal](9, 4) NOT NULL DEFAULT ((0)),
    [Achievement_To] [decimal](9, 4) NULL,
    [Rate] [decimal](9, 4) NULL,
    [Fixed_Amount] [decimal](18, 4) NULL,", ""),

            ("SalesRep_Commission_Runs", @"
    [Rep_Id] [bigint] NOT NULL,
    [Plan_Id] [bigint] NOT NULL,
    [Period_From] [date] NOT NULL,
    [Period_To] [date] NOT NULL,
    [Status] [int] NOT NULL DEFAULT ((0)),
    [Total_Amount] [decimal](18, 4) NOT NULL DEFAULT ((0)),
    [Approved_By] [bigint] NULL,
    [Approved_At] [datetime] NULL,
    [Account_Entry_List_ID] [bigint] NULL,", ""),

            ("SalesRep_Commission_Run_Lines", @"
    [Run_Id] [bigint] NOT NULL,
    [Component] [int] NOT NULL,
    [Scope_Id] [bigint] NULL,
    [Base_Amount] [decimal](18, 4) NOT NULL DEFAULT ((0)),
    [Target_Value] [decimal](18, 4) NULL,
    [Achievement_Rate] [decimal](9, 4) NULL,
    [Rate] [decimal](9, 4) NULL,
    [Amount] [decimal](18, 4) NOT NULL DEFAULT ((0)),
    [Notes] [nvarchar](500) NULL,", ""),

            // المرحلة 5 (ق11 + قرار المستخدم 2026-10-16): إسناد طلبية عميل لمندوب يسلّمها من سيارته. أسطر الطلبية تنتقل لأصناف
            // مخزن السيارة فيصير الحجز عليها؛ الأصل محفوظ فى أسطر الإسناد لإلغائه قبل أى تنفيذ. Status: 0 مُسنَد، 1 مُسلَّم، 2 ملغى الإسناد، 3 أُغلق المتبقى.
            ("SalesRep_Order_Assignments", @"
    [Order_Id] [bigint] NOT NULL,
    [Rep_Id] [bigint] NOT NULL,
    [Planned_Date] [date] NULL,
    [Status] [int] NOT NULL DEFAULT ((0)),
    [Original_Store_Id] [bigint] NULL,
    [Ended_At] [datetime] NULL,
    [End_Reason] [nvarchar](500) NULL,",
             @"CREATE NONCLUSTERED INDEX [IX_SalesRep_Order_Assignments_Order] ON [dbo].[SalesRep_Order_Assignments]([Order_Id]);
CREATE NONCLUSTERED INDEX [IX_SalesRep_Order_Assignments_Rep] ON [dbo].[SalesRep_Order_Assignments]([Rep_Id], [Status]);"),

            ("SalesRep_Order_Assignment_Lines", @"
    [Assignment_Id] [bigint] NOT NULL,
    [Order_Line_Id] [bigint] NOT NULL,
    [Original_Unit_Item_Id] [bigint] NULL,
    [Van_Unit_Item_Id] [bigint] NOT NULL,",
             @"CREATE NONCLUSTERED INDEX [IX_SalesRep_Order_Assignment_Lines_A] ON [dbo].[SalesRep_Order_Assignment_Lines]([Assignment_Id]);"),

            // محاولات التسليم عند الباب: مُسلَّم كاملاً/جزئياً/مرفوض، والسبب إلزامى لأى متبقى، والإجراء: 0 إعادة المحاولة (يبقى محجوزاً)، 1 إغلاق المتبقى.
            ("SalesRep_Order_Deliveries", @"
    [Assignment_Id] [bigint] NOT NULL,
    [Order_Id] [bigint] NOT NULL,
    [Shift_Id] [bigint] NULL,
    [Invoice_Id] [bigint] NULL,
    [Result] [int] NOT NULL DEFAULT ((0)),
    [Remaining_Action] [int] NULL,
    [Reason] [nvarchar](500) NULL,
    [Deposit_Applied] [decimal](18, 4) NOT NULL DEFAULT ((0)),
    [Cash_Collected] [decimal](18, 4) NOT NULL DEFAULT ((0)),
    [Notes] [nvarchar](500) NULL,",
             @"CREATE NONCLUSTERED INDEX [IX_SalesRep_Order_Deliveries_A] ON [dbo].[SalesRep_Order_Deliveries]([Assignment_Id]);"),
        };

        /// <summary>جداول النسخة السابقة غير المعتمدة التى ليس لها مقابل بنفس الاسم.</summary>
        private static readonly string[] ObsoleteTables = { "SalesRep_Commission_Plans_Tiers" };

        /// <summary>أخطاء آخر تشغيل لعرضها من «تحديث قاعدة البيانات».</summary>
        public static readonly System.Collections.Generic.List<string> LastErrors = new System.Collections.Generic.List<string>();

        /// <summary>هل كل الجداول بالشكل المعتمد؟ (عمود Update_ServerDateTime علامة النسخة المعتمدة.)</summary>
        public static bool IsUpToDate(operations opp)
        {
            string checks = string.Join(" AND ", Tables.Select(t => $"COL_LENGTH(N'dbo.{t.Table}', N'Update_ServerDateTime') IS NOT NULL"));
            string obsolete = string.Join(" AND ", ObsoleteTables.Select(t => $"OBJECT_ID(N'dbo.{t}', N'U') IS NULL"));
            return opp.db.Database.SqlQueryRaw<int>($"SELECT CASE WHEN {checks} AND {obsolete} THEN 1 ELSE 0 END AS Value").AsEnumerable().First() == 1;
        }

        public static void EnsureSchema()
        {
            LastErrors.Clear();
            var opp = new operations();
            bool upToDate;
            try
            {
                upToDate = IsUpToDate(opp);
            }
            catch (Exception ee)
            {
                operations.DataDictionaryLogerException(ee);
                return;
            }
            if (!upToDate)
            {
                foreach (string t in ObsoleteTables)
                    Step(t, () => DropIfEmpty(opp, t));
                foreach (var t in Tables)
                    Step(t.Table, () => CreateTable(opp, t.Table, t.Columns, t.Indexes));
            }
            Step("SalesRepAddedColumns", () => AddedColumns(opp));
            Step("SalesRepReturnReasons", () => SeedReturnReasons(opp));
            // مستقلة عن فحص الجداول: القواعد المرقّاة سابقاً تحتاج عناصر القائمة أيضاً.
            Step("SalesRepMenus", () => Menus(opp));
        }

        /// <summary>
        /// أعمدة أُضيفت لجداول المندوبين بعد إنشائها (جداول هذا الموديول فقط، لا جداول النظام القديمة): ALTER ADD لو ناقصة.
        /// المرحلة 3: الموافقة تحمل المندوب والوردية والعميل والقيمة المطلوبة/المسموحة/المعتمدة لتُستعمل مرة لنفس المستند وبحدها.
        /// </summary>
        private static readonly (string Table, string Column, string Type)[] Added =
        {
            ("SalesRep_Approvals", "Rep_Id", "[bigint] NULL"),
            ("SalesRep_Approvals", "Shift_Id", "[bigint] NULL"),
            ("SalesRep_Approvals", "Customer_Id", "[bigint] NULL"),
            ("SalesRep_Approvals", "Requested_Value", "[decimal](18, 4) NULL"),
            ("SalesRep_Approvals", "Allowed_Value", "[decimal](18, 4) NULL"),
            ("SalesRep_Approvals", "Approved_Value", "[decimal](18, 4) NULL"),
            // المرحلة 4: نافذة الإرجاع لكل مندوب.
            ("SalesRep_Reps", "Return_Window_Days", "[int] NULL"),
            // المرحلة 3: تحصيل الشيكات على الوردية وتسليمها عند الإقفال.
            ("SalesRep_Reps", "Allow_Cheque_Collection", "[bit] NOT NULL DEFAULT ((0))"),
            ("SalesRep_Reps", "Max_Cheque_Due_Days", "[int] NULL"),
            ("SalesRep_Document_Links", "Handed_Over_At", "[datetime] NULL"),
            ("SalesRep_Document_Links", "Handed_Over_User_Id", "[bigint] NULL"),
        };

        /// <summary>أسباب الإرجاع الافتراضية (§6.6-6) تُضاف فقط لو جدول الأسباب فارغ؛ تُعدَّل وتُضاف من القاعدة. لا يمس أى صف قائم.</summary>
        private static readonly (string Name, bool Damaged, bool Photo, bool Batch)[] DefaultReturnReasons =
        {
            ("رغبة العميل / فائض", false, false, false),
            ("خطأ فى الطلب أو التسليم", false, false, false),
            ("تالف أو مكسور", true, true, false),
            ("منتهى أو قريب الصلاحية", true, false, true),
        };

        private static void SeedReturnReasons(operations opp)
        {
            if (!TableExists(opp, "Lookup_SalesRep_ReturnReasons")) return;
            if (opp.db.Database.SqlQueryRaw<int>("SELECT COUNT(*) AS Value FROM Lookup_SalesRep_ReturnReasons").AsEnumerable().First() > 0) return;
            foreach (var (name, damaged, photo, batch) in DefaultReturnReasons)
            {
                var r = new Lookup_SalesRep_ReturnReason { Id = CallSnowflakeIdGenerate.GenerateNewId(), Name = name, Is_Damaged = damaged, Requires_Photo = photo, Requires_Batch = batch };
                r.StampInsert(Common.Branch_Id);
                opp.db.Lookup_SalesRep_ReturnReasons.Add(r);
            }
            opp.db.SaveChanges();
        }

        private static void AddedColumns(operations opp)
        {
            foreach (var (table, column, type) in Added)
                if (TableExists(opp, table) && ColumnMissing(opp, table, column))
                    opp.db.Database.ExecuteSqlRaw($"ALTER TABLE [dbo].[{table}] ADD [{column}] {type}");
            if (TableExists(opp, "SalesRep_Approvals") && opp.db.Database.SqlQueryRaw<int>(
                    "SELECT COUNT(*) AS Value FROM sys.indexes WHERE name = N'IX_SalesRep_Approvals_Guid' AND object_id = OBJECT_ID(N'dbo.SalesRep_Approvals')").AsEnumerable().First() == 0)
                opp.db.Database.ExecuteSqlRaw("CREATE NONCLUSTERED INDEX [IX_SalesRep_Approvals_Guid] ON [dbo].[SalesRep_Approvals]([Ref_Document_Guid], [Type])");
        }

        /// <summary>عناصر القائمة تحت «تعاملات المندوبين» (menuSalesManProcess): اسم العنصر، الوصف، صفحة Razor.</summary>
        private static readonly (string Key, string Description, string Path)[] MenuItems =
        {
            ("menuSalesRepProfiles", "ملفات المندوبين الميدانيين", "/salesrep/reps"),
            ("menuSalesRepShift", "وردية المندوب", "/salesrep/shift"),
            ("menuSalesRepApprovals", "موافقات المندوبين", "/salesrep/approvals"),
            ("menuSalesRepReturns", "مرتجعات المندوبين", "/salesrep/returns"),
            ("menuSalesRepReturnReasons", "أسباب إرجاع المندوبين", "/salesrep/return-reasons"),
            ("menuSalesRepOrders", "طلبيات المندوبين", "/salesrep/orders"),
            ("menuSalesRepPolicies", "سياسات ائتمان وخصم المندوبين", "/salesrep/policies"),
        };

        /// <summary>
        /// يضيف عنصرى القائمة لو غير موجودين (بالمطابقة مع اسم العنصر بعد فك التشفير، لا بالمعرّف)، وصلاحيتهما للمستخدمين
        /// المديرين فقط (AdminFlage = 1، قرار المستخدم 2026-10-16) والباقون من شاشة الصلاحيات. لا يغيّر أى صف قائم.
        /// </summary>
        private static void Menus(operations opp)
        {
            var menus = opp.db.Database.SqlQueryRaw<StoreTransferPolicy.KeyValueRow>("SELECT Id, KeysData, Value FROM SysConfigMenus").ToList();
            string Key(StoreTransferPolicy.KeyValueRow r) => r.KeysData == null ? "" : Common.DecryptMe(r.KeysData).Trim();
            var parent = menus.FirstOrDefault(m => Key(m) == "menuSalesManProcess");
            if (parent == null) return;   // قاعدة بلا قائمة المندوبين
            string E(string s) => Common.EncryptMe(s);
            var admins = opp.db.Database.SqlQueryRaw<long>("SELECT USER_ID AS Value FROM USER_DETAILES WHERE ISNULL(AdminFlage, 0) = 1").ToList();
            foreach (var (key, desc, path) in MenuItems)
            {
                long id = menus.FirstOrDefault(m => Key(m) == key)?.Id ?? 0;
                if (id == 0)
                {
                    id = menus.Max(m => m.Id) + 1;
                    opp.db.Database.ExecuteSqlInterpolated($@"
INSERT INTO SysConfigMenus (Id, Parent_Id, KeysData, Description, PROJECT_ID, Value, DefaultValue, DefaultDescription,
                            POS, Market, Factory, Tourism, AlFawzan, Client_A, Client_B, UserSells, IsShowToUser, RazorPath)
VALUES ({id}, {parent.Id}, {E(key)}, {E(desc)}, 2, {E("1")}, {E("1")}, {E(desc)},
        {E("1")}, {E("1")}, {E("1")}, {E("1")}, {E("1")}, {E("1")}, {E("1")}, {E("1")}, 1, {E(path)})");
                    menus.Add(new StoreTransferPolicy.KeyValueRow { Id = id, KeysData = E(key) });
                }
                foreach (long user in admins)
                {
                    int has = opp.db.Database.SqlQuery<int>($"SELECT COUNT(*) AS Value FROM SysConfigMenusPERMATIONS WHERE Menu_ID = {id} AND USER_ID = {user}").AsEnumerable().First();
                    if (has > 0) continue;
                    opp.db.Database.ExecuteSqlInterpolated($@"
INSERT INTO SysConfigMenusPERMATIONS (Id, Menu_ID, Parent_Id, USER_ID, PERMATION, PROJECT_ID, IsShowToUser)
VALUES ({CallSnowflakeIdGenerate.GenerateNewId()}, {id}, {parent.Id}, {user}, {E("1")}, 2, 1)");
                }
            }
        }

        private static void CreateTable(operations opp, string table, string columns, string indexes)
        {
            if (TableExists(opp, table) && ColumnMissing(opp, table, "Update_ServerDateTime"))
                DropIfEmpty(opp, table);
            if (TableExists(opp, table)) return;
            opp.db.Database.ExecuteSqlRaw($@"
CREATE TABLE [dbo].[{table}](
    [Id] [bigint] NOT NULL,{columns}{Audit},
    CONSTRAINT [PK_{table}] PRIMARY KEY CLUSTERED ([Id] ASC)
);");
            if (!string.IsNullOrWhiteSpace(indexes))
                opp.db.Database.ExecuteSqlRaw(indexes);
        }

        /// <summary>يحذف جدول النسخة السابقة فقط إن كان بلا صفوف؛ جدول به بيانات لا يُمس أبداً.</summary>
        private static void DropIfEmpty(operations opp, string table)
        {
            if (!TableExists(opp, table)) return;
            int rows = opp.db.Database.SqlQueryRaw<int>($"SELECT COUNT(*) AS Value FROM [dbo].[{table}]").AsEnumerable().First();
            if (rows > 0)
                throw new InvalidOperationException($"الجدول {table} بالشكل القديم وبه {rows} صف؛ لم يُحذف ويحتاج ترحيلاً يدوياً.");
            opp.db.Database.ExecuteSqlRaw($"DROP TABLE [dbo].[{table}]");
        }

        private static bool TableExists(operations opp, string table) =>
            opp.db.Database.SqlQueryRaw<int>($"SELECT CASE WHEN OBJECT_ID(N'dbo.{table}', N'U') IS NULL THEN 0 ELSE 1 END AS Value").AsEnumerable().First() == 1;

        private static bool ColumnMissing(operations opp, string table, string column) =>
            opp.db.Database.SqlQueryRaw<int>($"SELECT CASE WHEN COL_LENGTH(N'dbo.{table}', N'{column}') IS NULL THEN 1 ELSE 0 END AS Value").AsEnumerable().First() == 1;

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
