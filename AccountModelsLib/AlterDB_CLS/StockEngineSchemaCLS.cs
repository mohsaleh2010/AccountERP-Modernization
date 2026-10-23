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
             AND OBJECT_ID(N'dbo.Store_Account_Determination', N'U') IS NOT NULL
             AND COL_LENGTH(N'dbo.Supplier', N'IS_ShipmentCompany') IS NOT NULL
             AND COL_LENGTH(N'dbo.Order_Purshing', N'Posted_By') IS NOT NULL
             AND COL_LENGTH(N'dbo.Detailes_Purshing_Import_Export', N'Engine_Posted') IS NOT NULL
             AND COL_LENGTH(N'dbo.Item_First_Period_Balance', N'Source') IS NOT NULL
             AND COL_LENGTH(N'dbo.Order_Purshing', N'Transfer_Status') IS NOT NULL
             AND COL_LENGTH(N'dbo.Detailes_Purshing', N'Source_Detail_Id') IS NOT NULL
             AND COL_LENGTH(N'dbo.Item_Category', N'Item_Profile') IS NOT NULL
             AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Price_Type_Sell_Values') AND name = N'PriceUnit' AND system_type_id <> 106)
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
                Step(opp, StockEngine);
                Step(opp, OrderDocCycle);
                Step(opp, StockLedger);
                Step(opp, ItemOpeningBalance);
                Step(opp, StoreOrderTypeSettings);
                Step(opp, TransferReceipts);
                Step(opp, ItemProfiles);
                Step(opp, PriceUnitDecimal);
                Step(opp, StoreAccountDetermination);
                Step(opp, ShipmentCompanySupplierFlag);
            }
            // مستقلة عن فحص المخطط: القواعد المرقّاة سابقاً تحتاج الربط أيضاً، وتقرأ فقط العناصر غير المربوطة.
            Step(opp, LinkStoreOrderMenus);
            Step(opp, TransferSettings);
            Step(opp, StoreAccountMenu);
            Step(opp, TextileItemsMenu);
        }

        // ---------------- حسابات المخازن: الحساب المقابل لكل حركة مخزنية (StoreAccountDeterminationCLS) ----------------
        // صف واحد لكل (فرع × نوع مخزن × مخزن × دور)؛ النوع 0 والمخزن 0 = افتراضى الفرع.
        private static void StoreAccountDetermination(operations opp)
        {
            Run(opp, @"
IF OBJECT_ID(N'dbo.Store_Account_Determination', N'U') IS NULL
CREATE TABLE [dbo].[Store_Account_Determination](
    [Id] [bigint] NOT NULL,
    [Account_Branch_Id] [bigint] NOT NULL,
    [StoreUsed_Type] [int] NOT NULL DEFAULT ((0)),
    [Store_Id] [bigint] NOT NULL DEFAULT ((0)),
    [Account_Role] [int] NOT NULL,
    [SubAccount_Id] [bigint] NOT NULL,
    [OperationDate] [datetime] NULL,
    [User_Id] [bigint] NULL DEFAULT ((0)),
    [Branch_Id] [bigint] NULL,
    [Insert_ServerDateTime] [datetime] NULL DEFAULT (getdate()),
    [Update_ServerDateTime] [datetime] NULL,
    [EditOperationDate] [datetime] NULL,
    [Edit_User_Id] [bigint] NULL,
    CONSTRAINT [PK_Store_Account_Determination] PRIMARY KEY CLUSTERED ([Id] ASC)
);");
            Run(opp, @"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_Store_Account_Determination_Key')
CREATE UNIQUE NONCLUSTERED INDEX [UX_Store_Account_Determination_Key]
    ON [dbo].[Store_Account_Determination]([Account_Branch_Id] ASC, [StoreUsed_Type] ASC, [Store_Id] ASC, [Account_Role] ASC);");
        }

        // ---------------- شركات الشحن موردو خدمات فى جدول Supplier (قرار المستخدم 2026-10-23) ----------------
        // عمود جديد يقبل NULL فلا يُلمس أى صف قائم؛ NULL/0 = مورد عادى، 1 = شركة شحن (حسابها من النوع 65).
        private static void ShipmentCompanySupplierFlag(operations opp)
        {
            Run(opp, @"
IF COL_LENGTH(N'dbo.Supplier', N'IS_ShipmentCompany') IS NULL ALTER TABLE [dbo].[Supplier] ADD [IS_ShipmentCompany] [int] NULL;");
        }

        /// <summary>
        /// عنصر «ربط المخازن بالحسابات» تحت «ربط الحسابات» (بجوار ربط العملاء/الموردين/الصندوق/البنك) يفتح /store/account-settings،
        /// وصلاحيته للمديرين فقط (AdminFlage = 1) والباقون من شاشة الصلاحيات - نفس نمط SalesRepSchemaCLS.Menus. لا يغيّر أى صف قائم.
        /// </summary>
        private static void StoreAccountMenu(operations opp)
        {
            const string key = "menuItemRelatedStoreAccounts";
            const string desc = "ربط المخازن بالحسابات";
            var menus = opp.db.Database.SqlQueryRaw<StoreTransferPolicy.KeyValueRow>("SELECT Id, KeysData, Value FROM SysConfigMenus").ToList();
            string Key(StoreTransferPolicy.KeyValueRow r) => r.KeysData == null ? "" : Common.DecryptMe(r.KeysData).Trim();
            var parent = menus.FirstOrDefault(m => Key(m) == "menuItemRelatedAccounts");
            if (parent == null) return;
            string E(string s) => Common.EncryptMe(s);
            long id = menus.FirstOrDefault(m => Key(m) == key)?.Id ?? 0;
            if (id != 0) return;
            id = menus.Max(m => m.Id) + 1;
            opp.db.Database.ExecuteSqlInterpolated($@"
INSERT INTO SysConfigMenus (Id, Parent_Id, KeysData, Description, PROJECT_ID, Value, DefaultValue, DefaultDescription,
                            POS, Market, Factory, Tourism, AlFawzan, Client_A, Client_B, UserSells, IsShowToUser, RazorPath)
VALUES ({id}, {parent.Id}, {E(key)}, {E(desc)}, 2, {E("1")}, {E("1")}, {E(desc)},
        {E("1")}, {E("1")}, {E("1")}, {E("1")}, {E("1")}, {E("1")}, {E("1")}, {E("1")}, 1, {E("/store/account-settings")})");
            var admins = opp.db.Database.SqlQueryRaw<long>("SELECT Id AS Value FROM USER_DETAILES WHERE ISNULL(AdminFlage, 0) = 1").ToList();
            foreach (long user in admins)
                opp.db.Database.ExecuteSqlInterpolated($@"
INSERT INTO SysConfigMenusPERMATIONS (Id, Menu_ID, Parent_Id, USER_ID, PERMATION, PROJECT_ID, IsShowToUser)
VALUES ({CallSnowflakeIdGenerate.GenerateNewId()}, {id}, {parent.Id}, {user}, {E("1")}, 2, 1)");
        }

        /// <summary>
        /// خطة 05 بند 22: «أصناف النسيج» تحت «الأصناف» (عنصر الديسك توب menuItemItemsAllNesseg بلا صف فى SysConfigMenus) وتحته
        /// «قائمة أصناف النسيج» (/items/p/textile) و«بيانات إنتاج الأصناف» (/items/production-data). صلاحيتها للمديرين فقط
        /// والباقون من شاشة الصلاحيات - نفس نمط StoreAccountMenu. إدراج فقط، لا يغيّر أى صف قائم، ولا يتكرر.
        /// </summary>
        private static void TextileItemsMenu(operations opp)
        {
            var menus = opp.db.Database.SqlQueryRaw<StoreTransferPolicy.KeyValueRow>("SELECT Id, KeysData, Value FROM SysConfigMenus").ToList();
            string Key(StoreTransferPolicy.KeyValueRow r) => r.KeysData == null ? "" : Common.DecryptMe(r.KeysData).Trim();
            var items = menus.FirstOrDefault(m => Key(m) == "toolStripMenuItemItemsMain");
            if (items == null) return;
            string E(string s) => Common.EncryptMe(s);
            long next = menus.Max(m => m.Id) + 1;
            var admins = opp.db.Database.SqlQueryRaw<long>("SELECT Id AS Value FROM USER_DETAILES WHERE ISNULL(AdminFlage, 0) = 1").ToList();
            long Ensure(string key, string desc, long parentId, string path)
            {
                long id = menus.FirstOrDefault(m => Key(m) == key)?.Id ?? 0;
                if (id != 0) return id;
                id = next++;
                string p = path == null ? null : E(path);
                opp.db.Database.ExecuteSqlInterpolated($@"
INSERT INTO SysConfigMenus (Id, Parent_Id, KeysData, Description, PROJECT_ID, Value, DefaultValue, DefaultDescription,
                            POS, Market, Factory, Tourism, AlFawzan, Client_A, Client_B, UserSells, IsShowToUser, RazorPath)
VALUES ({id}, {parentId}, {E(key)}, {E(desc)}, 2, {E("1")}, {E("1")}, {E(desc)},
        {E("1")}, {E("1")}, {E("1")}, {E("1")}, {E("1")}, {E("1")}, {E("1")}, {E("1")}, 1, {p})");
                foreach (long user in admins)
                    opp.db.Database.ExecuteSqlInterpolated($@"
INSERT INTO SysConfigMenusPERMATIONS (Id, Menu_ID, Parent_Id, USER_ID, PERMATION, PROJECT_ID, IsShowToUser)
VALUES ({CallSnowflakeIdGenerate.GenerateNewId()}, {id}, {parentId}, {user}, {E("1")}, 2, 1)");
                return id;
            }
            long textile = Ensure("menuItemItemsAllNesseg", "أصناف النسيج", items.Id, null);
            Ensure("menuItemItemsListTextile", "قائمة أصناف النسيج", textile, "/items/p/textile");
            Ensure("menuItemItemsProductionData", "بيانات إنتاج الأصناف", textile, "/items/production-data");
        }

        /// <summary>
        /// خطة 01 المرحلة 7 (§2.7): إعداد «طريقة التحويل بين المخازن» (Store_Transfer_Mode) والصلاحية المتقدمة
        /// «تحويل مباشر بين المخازن بدون تأكيد استلام» تحت 321. يُنشآن مرة واحدة فقط (لا يغيّران قيمة عدّلها العميل).
        /// قيمة الإعداد الأولى = الإعداد القديم (1 مباشر ← 1، 0 ← 0 بتأكيد) فلا يتغير سلوك أى عميل؛ النصوص مشفَّرة كباقى الجدولين.
        /// </summary>
        private static void TransferSettings(operations opp)
        {
            var cfg = opp.db.Database.SqlQueryRaw<StoreTransferPolicy.KeyValueRow>("SELECT Id, KeysData, Value FROM SysConfig WHERE KeysData IS NOT NULL").ToList();
            string Key(StoreTransferPolicy.KeyValueRow r) => Common.DecryptMe(r.KeysData).Trim();
            if (!cfg.Any(r => Key(r) == StoreTransferPolicy.ModeKey))
            {
                var legacy = cfg.FirstOrDefault(r => Key(r) == StoreTransferPolicy.LegacyKey);
                string value = legacy != null && Common.DecryptMe(legacy.Value ?? "").Trim() == "1" ? "1" : "0";
                long id = cfg.Any(r => r.Id == 405) ? cfg.Max(r => r.Id) + 1 : 405;
                string desc = "طريقة التحويل بين المخازن: 0 بتأكيد استلام، 1 مباشر، 2 حسب صلاحية المستخدم";
                string E(string s) => Common.EncryptMe(s);
                opp.db.Database.ExecuteSqlInterpolated($@"
INSERT INTO SysConfig (Id, KeysData, DefaultValue, Value, DefaultDescription, Description, Client_B, Client_A, POS, Market, Factory, AlFawzan, Tourism)
VALUES ({id}, {E(StoreTransferPolicy.ModeKey)}, {E("0")}, {E(value)}, {E(desc)}, {E(desc)}, {E("0")}, {E("0")}, {E("0")}, {E("0")}, {E("0")}, {E("0")}, {E("0")})");
            }

            var menus = opp.db.Database.SqlQueryRaw<StoreTransferPolicy.KeyValueRow>("SELECT Id, KeysData, Value FROM SysConfigMenus").ToList();
            if (!menus.Any(m => m.KeysData != null && Common.DecryptMe(m.KeysData).Trim() == StoreTransferPolicy.DirectPermissionKey))
            {
                long id = menus.Any(m => m.Id == 984) ? menus.Max(m => m.Id) + 1 : 984;
                string desc = "تحويل مباشر بين المخازن بدون تأكيد استلام";
                string E(string s) => Common.EncryptMe(s);
                opp.db.Database.ExecuteSqlInterpolated($@"
INSERT INTO SysConfigMenus (Id, Parent_Id, KeysData, Description, PROJECT_ID, Value, DefaultValue, DefaultDescription,
                            POS, Market, Factory, Tourism, AlFawzan, Client_A, Client_B, UserSells, IsShowToUser)
VALUES ({id}, 321, {E(StoreTransferPolicy.DirectPermissionKey)}, {E(desc)}, 2, {E("1")}, {E("1")}, {E(desc)},
        {E("1")}, {E("1")}, {E("1")}, {E("1")}, {E("1")}, {E("1")}, {E("1")}, {E("1")}, 1)");
            }
            StoreTransferPolicy.Invalidate();
        }

        /// <summary>
        /// خطة 03 المرحلتان 2 و4 (م-03-11): يربط عناصر قائمة «أذونات الصرف والاستلام» للأنواع البسيطة بصفحة Razor
        /// /store/order/{type}، وعناصر «تقارير الأذونات» بقائمة الأذون /store/orders?type=، عبر SysConfigMenus.RazorPath (مشفَّر كالباقى). يملأ الفارغ فقط فلا يغيّر ربطاً عدّله العميل،
        /// والمطابقة باسم عنصر القائمة (KeysData بعد فك التشفير) لا بالمعرّف. شاشات الويندوز لا تقرأ RazorPath فلا تتأثر.
        /// التحويلات (1001/1006/1016/1017) غير مربوطة حتى المرحلة 3.
        /// </summary>
        private static readonly (string Key, string Path)[] StoreOrderMenus =
        {
            // شاشة الإذن لكل نوع بسيط (المرحلة 2)
            ("menuItemOrderSarfCustomer", "/store/order/1002"), ("menuItemOrderSarfMortagahCustomer", "/store/order/1003"),
            ("menuItemOrderEstlamSupplier", "/store/order/1004"), ("menuItemOrderEstlamMortaga3Supplier", "/store/order/1005"),
            ("menuItemOrderTasweaZeada", "/store/order/1007"), ("menuItemOrderTasweaNakis", "/store/order/1008"),
            ("menuItemOrderTasweaEntag", "/store/order/1009"), ("menuItemOrderSarfToEntag", "/store/order/1010"),
            ("menuItemOrderSarfToKeta3", "/store/order/1012"), ("menuItemOrderSarfToHalek", "/store/order/1013"),
            ("menuItemOrderEstlamFromOuterStore", "/store/order/1014"), ("menuItemOrderEstlamFromOuterStore2", "/store/order/1014"),
            ("menuItemOrderSarfFromOuterStore2", "/store/order/1015"),
            ("menuItemOrderSarfWarsha", "/store/order/1018"), ("menuItemOrderEstlamFromWarsha", "/store/order/1019"),
            ("menuItemOrderSarfEmployee", "/store/order/1020"), ("menuItemOrderEstlamFromEmployee", "/store/order/1021"),
            ("menuItemOrderSarfMachine", "/store/order/1022"), ("menuItemOrderEstlamFromMachine", "/store/order/1023"),
            // التحويل (المرحلة 3): الشحنة 1001/1006، و«إذن إستلام من المخزن» يفتح الشحنات بانتظار الاستلام (الاستلام يتم من الشحنة).
            // «إذن صرف إلى المخزن» (851، النوع 1017) يبقى بلا ربط: الشحنة 1001 تغطيه.
            ("menuItemOrderSarf", "/store/order/1001"), ("menuItemOrderMortaga3Sarf", "/store/order/1006"),
            ("menuItemOrderEstlamFromStore", "/store/orders?pending=1"),
            // القسم مخزن فرعى برصيد (م-03-9): الصرف لقسم = تحويل لقسم، والاستلام من قسم = مرتجع تحويل من القسم (كانا بلا شاشة فى القديم).
            ("menuItemOrderSarfDepartment", "/store/order/1001?to=20"), ("menuItemOrderEstlamFromDepartment", "/store/order/1006?from=20"),
            // أرصدة الأصناف والتقييم (المرحلة 5): كمية، متوسط تكلفة (التقييم المعتمد)، سعر المورد/آخر شراء، سعر البيع، وبتاريخ سابق.
            ("menuItemBalance", "/store/balances"), ("menuItemItemBalanceOnly", "/store/balances"),
            ("menuItemItemBalance", "/store/balances?price=1"), ("menuItemItemBalanceWithCost", "/store/balances?price=1"),
            ("menuItemBalanceSupplierPrice", "/store/balances?price=2"), ("menuItemItemBalanceSellBuyPriceTotal", "/store/balances?price=2"),
            ("menuItemBalanceSellPrice", "/store/balances?price=3"),
            ("aToolStripMenuItem", "/store/balances?asof=1"), ("menuItemItemsListBeforBalance", "/store/balances?asof=1"),
            // مطابقة الأرصدة (خطة 01 م3) بدل التعديل المباشر للأرصدة: الفرق يُعتمد بتصحيح الدفتر أو يُسوّى بإذن تسوية.
            ("menuItemEditStoreBalance", "/store/reconciliation"), ("menuItemItemBalanceOnlyEdit", "/store/reconciliation"),
            // كارت الصنف الموحَّد (خطة 01 م4) على دفتر الحركة.
            ("menuItemCardsStore", "/store/item-card"), ("ToolStripMenuItem_Transaction_Details", "/store/item-card"),
            ("ToolStripMenuItem_Transaction_DetailsWithCost", "/store/item-card?value=1"), ("menuItemCardsDept", "/store/item-card?type=20"),
            // تقارير الصرف والوارد (خطة 03 م6): ملخص حركة الأذون لكل صنف، بنوع الإذن لكل تقرير قديم.
            ("menuItem153", "/store/reports/movements"), ("ToolStripMenuItem_Sarf_FromStore", "/store/reports/movements"),
            ("Item_Sarf_FromStoreDetailes_Purshing_Import", "/store/reports/movements"), ("Item_Sarf_FromStoreDetailes_Purshing_ExportWithValues", "/store/reports/movements"),
            ("Item_Sarf_FromStoreDetailes_Purshing_ImportWithValues", "/store/reports/movements"),
            ("ToolStripMenuItemItem_Sarf_FromToStoreDetailes_Purshing_Import_Export", "/store/reports/movements"),
            ("menuItem79", "/store/reports/movements?type=1010"), ("menuItem81", "/store/reports/movements?type=1012"), ("menuItem80", "/store/reports/movements?type=1013"),
            ("menuItemTasweatZeada", "/store/reports/movements?type=1007"), ("menuItemTasweatNakis", "/store/reports/movements?type=1008"),
            ("menuItem76", "/store/reports/movements?type=1009"), ("menuItem77", "/store/reports/movements?type=1009"),
            ("toolStripMenuItemSarfMachineReport", "/store/reports/movements?type=1022"), ("toolStripMenuItemDetailsSarfEstlamMachineReport", "/store/reports/movements?type=1022"),
            ("toolStripMenuItemEstlamMachineReport", "/store/reports/movements?type=1023"), ("menuItemEstlamFromSupplier", "/store/reports/movements?type=1004"),
            // الاحتياجات (حد الطلب).
            ("menuItem86", "/store/needs"), ("menuItem87", "/store/needs"), ("menuItem88", "/store/needs"), ("menuItem89", "/store/needs"),
            ("toolStripMenuItemUponRequest", "/store/needs"), ("ToolStripMenuItemInvoiceUponRequest", "/store/needs"),
            ("menuItemAddItemsToStore", "/store/copy-items"),
            // خطة 04 م1: القوائم المرجعية للأصناف فى صفحة واحدة.
            ("menuItemItem_Types", "/items/lookups/types"), ("menuItemBodies_Item", "/items/lookups/bodies"), ("menuItem9Item_Color", "/items/lookups/colors"),
            ("ToolStripMenuitemsTypesDescription", "/items/lookups/descriptions"), ("ToolStripMenuItems_TypesDescriptionNameOther", "/items/lookups/other-names"),
            ("ToolStripMenuItems_TypesFactory_Name", "/items/lookups/factories"), ("ToolStripMenuItems_TypesSize_Name", "/items/lookups/sizes"),
            ("ToolStripMenuItems_TypesDegreeFarz_Name", "/items/lookups/degrees"), ("ToolStripMenuItemItems_Nature", "/items/lookups/natures"),
            ("ToolStripMenuItemItems_Groups", "/items/lookups/groups"),
            // خطة 04 م1: الوحدات والفئات (مفتاح 494 فيه حرف تشكيل عربى U+064D كما هو مخزَّن)
            ("menuUnits", "/items/units"), ("menuItemItemMainCategory", "/items/categories"), ("menuItemItem\u064dSubCategory", "/items/categories"),
            // خطة 04 م2: قائمة الأصناف وشاشة الصنف الموحَّدة
            ("menuItemItemsListGrid", "/items"), ("menuItemItemsListView", "/items"), ("menuItemItemsListNoPrice", "/items/new"), ("menuItemItemsListNoBalance", "/items?kind=services"),
            // خطة 04 م3: ملف الأدوية (القائمة، الإضافة، أصناف التجزئة)
            ("menuItemItemsListSpecAdwia", "/items/p/pharmacy"), ("toolStripMenuItemMitems_List_PH_R", "/items/p/pharmacy/new"),
            ("toolStripMenuItemItemsListAdwiaRetail", "/items/p/pharmacy-retail"),
            // خطة 04 م4: ملفات السيارات والسيراميك (عنصر الزجاج menuItemItemsListSpecGlass بلا صف فى SysConfigMenus)
            ("menuItemItemsListSpecCars", "/items/p/cars"), ("ToolStripMenuItemItemsForCars", "/items/p/cars"),
            ("menuItemItemsListSpecSeramek", "/items/p/ceramics"),
            // خطة 05 بند 21: «بيانات الأصناف» — أصناف مخازن نوع (منتجات المصنع 1 / المشتريات 2) بدل Mitems_List و Item*_ListFRM،
            // وإضافة الصنف العام والزجاج (Mitems_ListNew / Mitems_List_WH)
            ("menuItemProductionItelList", "/items/stores/1"), ("menuProducts", "/items"), ("menuItemSellItems", "/items/stores/1"),
            ("menuItemItemPurchase", "/items/stores/2"), ("menuItemPurchaseList", "/items/stores/2"),
            ("menuItemItemsList", "/items/new"), ("menuItemItemsListSpecGlass", "/items/p/glass/new"),
            // خطة 05 بند 19: أنواع السيارات وموديلاتها تحت قائمة السيارات
            ("lookupCarTypeToolStripMenuItem", "/cars/types"), ("lookupCarTypeModelToolStripMenuItem", "/cars/models"),
            // خطة 05 بند 23: لقطات الأرصدة بالتاريخ والوقت (بدل balanceSelect_Date/balanceWithDate) وأرصدة الصلاحية (بدل ItemBalanceWithExpireFrm)
            ("menuItemItemsListSavedBalance", "/store/balances/snapshots"), ("ToolStripMenuItem_Save_ItemBalanceNow", "/store/balances/snapshots"),
            ("menuItemItemBalanceWithExpire", "/store/balances/expiry"),
            // خطة 04 م5: الأسعار (طباعة الأسعار menuItemItemPrices لم تُبنَ بعد)
            ("menuPrice_Type_List", "/items/prices/lists"), ("menuPriceCategory", "/items/prices/lists"), ("menuItemSearchItemPrices_List_Values", "/items/prices/lists"),
            ("menuItemEditPrices", "/items/prices/bulk"), ("MenuCustomer_PriceCLS", "/items/prices/customers"),
            ("menuItemPricePurchaseType", "/items/prices/suppliers"), ("menuItemPurchaedItemPrice", "/items/prices/suppliers"),
            // قائمة وبحث الأذون (المرحلة 4) بدل Order_detailes لكل نوع، وقائمة كل الأذون.
            // ملاحظة: القائمة القديمة تفتح «مرتجع صرف قطع الغيار» (879) على 1022 وعنصر «صرف إلى موظف» (881) بلا شاشة؛ هنا كلٌّ لنوعه الصحيح.
            ("menuItemOrdersList", "/store/orders"),
            ("menuItem99", "/store/orders?type=1001"), ("menuItem101", "/store/orders?type=1004"), ("menuItem102", "/store/orders?type=1005"),
            ("menuItem118", "/store/orders?type=1002"), ("menuItem104", "/store/orders?type=1003"), ("menuItem122", "/store/orders?type=1007"),
            ("menuItem123", "/store/orders?type=1008"), ("menuItem124", "/store/orders?type=1009"), ("menuItem125", "/store/orders?type=1010"),
            ("menuItem126", "/store/orders?type=1012"),
            ("SarfMachineToolStripMenuItem", "/store/orders?type=1022"), ("EstlamMachineToolStripMenuItem", "/store/orders?type=1023"),
            ("EstlamFromEmployeeToolStripMenuItem", "/store/orders?type=1021"), ("SarfToFromEmployeeToolStripMenuItem", "/store/orders?type=1020"),
        };

        private static void LinkStoreOrderMenus(operations opp)
        {
            var map = StoreOrderMenus.ToDictionary(m => m.Key, m => m.Path, StringComparer.OrdinalIgnoreCase);
            var rows = opp.db.Database.SqlQueryRaw<MenuKeyRow>(
                "SELECT Id, KeysData FROM SysConfigMenus WHERE ISNULL(RazorPath, N'') = N'' AND KeysData IS NOT NULL").ToList();
            foreach (var r in rows)
            {
                if (!map.TryGetValue(Common.DecryptMe(r.KeysData).Trim(), out string target)) continue;
                string path = Common.EncryptMe(target);
                opp.db.Database.ExecuteSqlInterpolated(
                    $"UPDATE SysConfigMenus SET RazorPath = {path} WHERE Id = {r.Id} AND ISNULL(RazorPath, N'') = N''");
            }
        }

        /// <summary>صف قراءة (Id + اسم العنصر المشفَّر) — عام لأن EF يملؤه من SQL خام.</summary>
        public class MenuKeyRow
        {
            public long Id { get; set; }
            public string KeysData { get; set; }
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

        /// <summary>
        /// خطة 03 المرحلة 3: التحويل بخطوتين — حالة الشحنة (1 فى الطريق، 2 استُلم جزئياً، 3 مكتمل/مغلق)، سبب إغلاق العجز،
        /// وربط إذن الاستلام (1016) وأسطره بالشحنة (1001/1006) وأسطرها. أعمدة جديدة فقط تقبل NULL، فلا يتأثر أى إذن قائم.
        /// </summary>
        private static void TransferReceipts(operations opp)
        {
            Run(opp, @"
IF COL_LENGTH(N'dbo.Order_Purshing', N'Transfer_Status') IS NULL ALTER TABLE [dbo].[Order_Purshing] ADD [Transfer_Status] [int] NULL;
IF COL_LENGTH(N'dbo.Order_Purshing', N'Source_Order_Id') IS NULL ALTER TABLE [dbo].[Order_Purshing] ADD [Source_Order_Id] [bigint] NULL;
IF COL_LENGTH(N'dbo.Order_Purshing', N'Transfer_Close_Reason') IS NULL ALTER TABLE [dbo].[Order_Purshing] ADD [Transfer_Close_Reason] [nvarchar](500) NULL;
IF COL_LENGTH(N'dbo.Detailes_Purshing', N'Source_Detail_Id') IS NULL ALTER TABLE [dbo].[Detailes_Purshing] ADD [Source_Detail_Id] [bigint] NULL;");
            Run(opp, @"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Order_Purshing_Source_Order')
    CREATE INDEX [IX_Order_Purshing_Source_Order] ON [dbo].[Order_Purshing] ([Source_Order_Id]) WHERE [Source_Order_Id] IS NOT NULL;");
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

        // ---------------- خطة 04: ملف نشاط الفئة (عام/أدوية/...) يحدد أقسام شاشة الصنف ----------------
        private static void ItemProfiles(operations opp)
        {
            Run(opp, @"
IF COL_LENGTH(N'dbo.Item_Category', N'Item_Profile') IS NULL ALTER TABLE [dbo].[Item_Category] ADD [Item_Profile] [nvarchar](30) NULL;");
        }

        // ---------------- خطة 04 م5: سعر قائمة الأسعار عشرى (كان bigint فلا يقبل الكسور) — قرار المستخدم 2026-10-10 ----------------
        // يوسّع النوع فقط (القيم الصحيحة القائمة تبقى كما هى)، ولا فهارس أو قيود على العمود.
        private static void PriceUnitDecimal(operations opp)
        {
            Run(opp, @"
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Price_Type_Sell_Values') AND name = N'PriceUnit' AND system_type_id <> 106)
    ALTER TABLE [dbo].[Price_Type_Sell_Values] ALTER COLUMN [PriceUnit] [decimal](18, 4) NULL;");
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
