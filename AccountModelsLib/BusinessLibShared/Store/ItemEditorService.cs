using System;
using System.Collections.Generic;
using System.Linq;
using AccountModelsLib.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib
{
    /// <summary>
    /// شاشة الصنف الموحَّدة (خطة 04 — المرحلة 2): إضافة/تعديل صنف بملفى «عام» و«خدمات» بدل Mitems_ListNew وأخواتها.
    /// قواعد (قرارات م-04-3 وخطة 01، وحسب الأنظمة الحديثة):
    /// - الإضافة تُنشئ Item + سطر Unit_item بكمية صفر للمخزن المختار (ولكل المخازن الفعالة من نفس النوع إذا IsAddAllItemInAllStores = 1).
    /// - الرصيد الابتدائى (اختيارى، عند الإضافة فقط) يُرحَّل عبر ItemOpeningBalanceService كأول مدة للفترة الحالية؛ لا صف أول مدة بالصفر.
    /// - التعديل لا يغيّر Full_qua/D_Qua إطلاقاً؛ الوحدة تتغير فقط لصنف بلا رصيد وبلا حركات (لأن الأرصدة محسوبة بمعاملها).
    /// - الأسعار بيانات الصنف: تُحفظ على Item وعلى كل أسطر Unit_item للصنف.
    /// - الكود: تلقائى = أكبر كود رقمى + 1 (من 1000) عند UseAutomaticItemCode = 1 أو كود فارغ/0، ولا يتكرر (الكود «0» القديم مستثنى).
    /// - الصلاحيات 916 (إضافة) و917 (تعديل) كما فى ItemCLS.
    /// </summary>
    public class ItemEditorService
    {
        public const int PermissionAdd = 916, PermissionEdit = 917;

        private readonly Func<operations> _newOpp;
        private readonly Func<int, bool> _hasPermission;

        public ItemEditorService(Func<operations> newOpp = null, Func<int, bool> hasPermission = null)
        {
            _newOpp = newOpp ?? (() => new operations());
            _hasPermission = hasPermission ?? (p => Common.IsHavePermission(Common.User_Id, p, Common.Project_ID, Common.Branch_Id));
        }

        // ===================== النماذج =====================

        public class ItemForm
        {
            public long Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string Name_On_Invoice { get; set; }
            public string Other_Name { get; set; }
            public string Barcode2 { get; set; }
            public string Barcode3 { get; set; }
            public long Category_Id { get; set; }
            public long Group_Id { get; set; }
            public long Nature_Id { get; set; }
            /// <summary>1 صنف، 2 خدمة (ItemOrServiceEnum).</summary>
            public int Item_Or_Service { get; set; } = 1;
            public bool Without_Balance { get; set; }
            public bool Is_Active { get; set; } = true;
            public long Unit_Id { get; set; }
            public decimal Reorder_Level { get; set; }
            public decimal Sale_Price_Large { get; set; }
            public decimal Sale_Price_Small { get; set; }
            public decimal Purchase_Price_Large { get; set; }
            public decimal Purchase_Price_Small { get; set; }
            /// <summary>1 الكبرى، 2 الصغرى.</summary>
            public int Default_Sale_Unit { get; set; } = 1;
            public int Default_Purchase_Unit { get; set; } = 1;
            public string Description { get; set; }
            /// <summary>عند الإضافة: المخزن الأساسى.</summary>
            public long Store_Id { get; set; }
            // الرصيد الابتدائى (الإضافة فقط)
            public decimal Opening_Large { get; set; }
            public decimal Opening_Small { get; set; }
            public decimal Opening_Cost_Large { get; set; }
        }

        public class StoreLine
        {
            public long Unit_Item_Id { get; set; }
            public long Store_Id { get; set; }
            public string Store_Name { get; set; }
            public decimal Full_Qua { get; set; }
            public decimal D_Qua { get; set; }
            public decimal Avg_Cost { get; set; }
            public int Movements { get; set; }
        }

        public class ItemDetails
        {
            public ItemForm Form { get; set; }
            public List<StoreLine> Stores { get; set; } = new();
            public decimal Factor { get; set; } = 1;
            public bool Unit_Locked { get; set; }
        }

        public class Option
        {
            public long Id { get; set; }
            public string Name { get; set; }
        }

        public class UnitOption
        {
            public long Id { get; set; }
            public string Name { get; set; }
            public string Big_Unit { get; set; }
            public string Small_Unit { get; set; }
            public decimal Factor { get; set; }
            public bool Is_Default { get; set; }
        }

        public class Lookups
        {
            public List<Option> Categories { get; set; }
            public List<Option> Groups { get; set; }
            public List<Option> Natures { get; set; }
            public List<UnitOption> Units { get; set; }
            public List<Option> Stores { get; set; }
            public long Default_Store_Id { get; set; }
            public long Default_Category_Item { get; set; }
            public long Default_Category_Service { get; set; }
            public bool Auto_Code { get; set; }
            public bool All_Stores { get; set; }
            public string Period_Name { get; set; }
        }

        public class ListRow
        {
            public long Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string Category { get; set; }
            public string Unit { get; set; }
            public decimal Factor { get; set; }
            public decimal Sale_Price { get; set; }
            public decimal Purchase_Price { get; set; }
            public decimal Balance_Small { get; set; }
            public int Item_Or_Service { get; set; }
            public bool Without_Balance { get; set; }
            public bool Is_Active { get; set; }
        }

        public class ListFilter
        {
            public string Text { get; set; }
            public long Category_Id { get; set; }
            /// <summary>0 الكل، 1 أصناف، 2 خدمات.</summary>
            public int Kind { get; set; }
            /// <summary>1 النشطة، 0 الموقوفة، -1 الكل.</summary>
            public int Active { get; set; } = 1;
            public int Top { get; set; } = 500;
        }

        // ===================== القراءة =====================

        public Lookups GetLookups() => Run(opp =>
        {
            var l = new Lookups
            {
                Categories = opp.db.Database.SqlQuery<Option>($"SELECT ID AS Id, ISNULL(Category_Name, N'') AS Name FROM Item_Category WHERE ISNULL(IS_Active, 1) = 1 ORDER BY Category_Name").ToList(),
                Groups = opp.db.Database.SqlQuery<Option>($"SELECT CAST(Id AS bigint) AS Id, ISNULL(Name, N'') AS Name FROM Items_Group ORDER BY Name").ToList(),
                Natures = opp.db.Database.SqlQuery<Option>($"SELECT CAST(Id AS bigint) AS Id, ISNULL(ItemsNature_Name, N'') AS Name FROM Items_Nature WHERE ISNULL(IsActive, 1) = 1 ORDER BY ItemsNature_Name").ToList(),
                Units = opp.db.Database.SqlQuery<UnitOption>($@"SELECT id AS Id, u_name AS Name, B_Unit AS Big_Unit, S_unit AS Small_Unit, CAST(ISNULL(S_unit_Value, 1) AS decimal(18,4)) AS Factor,
       CAST(CASE WHEN IS_Default = 1 THEN 1 ELSE 0 END AS bit) AS Is_Default FROM unit ORDER BY u_name").ToList(),
                Stores = opp.db.Database.SqlQuery<Option>($@"SELECT Id, ISNULL(Store_Name, N'') AS Name FROM Store
WHERE Active = 1 AND ISNULL(IS_ItemSecond_degree, 0) = 0 ORDER BY IS_Default_Store DESC, Store_Name").ToList(),
                Auto_Code = Common.UseAutomaticItemCode == 1,
                All_Stores = Common.IsAddAllItemInAllStores == 1,
            };
            l.Default_Store_Id = l.Stores.FirstOrDefault()?.Id ?? 0;
            l.Default_Category_Item = opp.db.Database.SqlQuery<long>($"SELECT TOP 1 ID AS Value FROM Item_Category WHERE Is_Default = 1 AND Is_ItemOrService = 1 AND ISNULL(IS_Active, 1) = 1").AsEnumerable().FirstOrDefault();
            l.Default_Category_Service = opp.db.Database.SqlQuery<long>($"SELECT TOP 1 ID AS Value FROM Item_Category WHERE Is_Default = 1 AND Is_ItemOrService = 2 AND ISNULL(IS_Active, 1) = 1").AsEnumerable().FirstOrDefault();
            l.Period_Name = GetCurrentPeriod(opp)?.Label;
            return l;
        });

        public List<ListRow> List(ListFilter f)
        {
            string t = (f.Text ?? "").Trim();
            string like = "%" + t + "%";
            int top = Math.Clamp(f.Top, 1, 5000);
            return Run(opp => opp.db.Database.SqlQuery<ListRow>($@"
SELECT TOP ({top}) i.id AS Id, ISNULL(i.code, N'') AS Code, ISNULL(i.Item_Name, N'') AS Name, ISNULL(c.Category_Name, N'') AS Category,
       ISNULL(u.B_Unit, N'') AS Unit, CAST(ISNULL(u.S_unit_Value, 1) AS decimal(18,4)) AS Factor,
       CAST(ISNULL(i.S_price_LargeUnit, 0) AS decimal(18,4)) AS Sale_Price, CAST(ISNULL(i.P_price_LargeUnit, 0) AS decimal(18,4)) AS Purchase_Price,
       CAST(ISNULL((SELECT SUM(ISNULL(ui.Full_qua, 0) * ISNULL(ui.Small_unit, 1) + ISNULL(ui.D_Qua, 0)) FROM Unit_item ui WHERE ui.Item_id = i.id), 0) AS decimal(18,4)) AS Balance_Small,
       ISNULL(i.Is_ItemOrService, 1) AS Item_Or_Service, CAST(CASE WHEN i.Is_ItemWithoutBalance = 1 THEN 1 ELSE 0 END AS bit) AS Without_Balance,
       CAST(CASE WHEN ISNULL(i.Active_flage, 1) = -1 OR i.Active_flage = 0 THEN 0 ELSE 1 END AS bit) AS Is_Active
FROM Item i
LEFT JOIN Item_Category c ON c.ID = i.Item_Category_ID
LEFT JOIN unit u ON u.id = i.Unit_Id
WHERE ({t} = N'' OR i.Item_Name LIKE {like} OR i.code = {t} OR i.AnotherName LIKE {like} OR i.BarCode2 = {t} OR i.BarCode3 = {t}
       OR EXISTS (SELECT 1 FROM Unit_item b WHERE b.Item_id = i.id AND b.Code = {t}))
  AND ({f.Category_Id} = 0 OR i.Item_Category_ID = {f.Category_Id})
  AND ({f.Kind} = 0 OR ({f.Kind} = 2 AND (i.Is_ItemOrService = 2 OR i.Is_ItemWithoutBalance = 1)) OR ({f.Kind} = 1 AND ISNULL(i.Is_ItemOrService, 1) <> 2 AND ISNULL(i.Is_ItemWithoutBalance, 0) = 0))
  AND ({f.Active} = -1 OR ({f.Active} = 1 AND ISNULL(i.Active_flage, 1) NOT IN (0, -1)) OR ({f.Active} = 0 AND i.Active_flage IN (0, -1)))
ORDER BY i.Item_Name").ToList());
        }

        public ItemDetails Get(long id) => Run(opp =>
        {
            var i = opp.db.Item.AsNoTracking().FirstOrDefault(x => x.Id == id) ?? throw new InvalidOperationException("الصنف غير موجود.");
            var d = new ItemDetails
            {
                Form = new ItemForm
                {
                    Id = i.Id, Code = i.code, Name = i.Item_Name, Name_On_Invoice = i.ItemName_OnInvoice, Other_Name = i.AnotherName,
                    Barcode2 = i.BarCode2, Barcode3 = i.BarCode3, Category_Id = i.Item_Category_ID ?? 0, Group_Id = i.Items_GroupsId ?? 0,
                    Nature_Id = i.Items_NaturesId ?? 0, Item_Or_Service = i.Is_ItemOrService == 2 ? 2 : 1, Without_Balance = i.Is_ItemWithoutBalance == 1,
                    Is_Active = i.Active_flage is not (0 or -1), Unit_Id = i.Unit_Id ?? 0, Reorder_Level = i.Upon_Request ?? 0,
                    Sale_Price_Large = i.S_price_LargeUnit ?? 0, Sale_Price_Small = i.S_price_SmallUnit ?? 0,
                    Purchase_Price_Large = i.P_price_LargeUnit ?? 0, Purchase_Price_Small = i.P_price_SmallUnit ?? 0,
                    Default_Sale_Unit = i.Unit_Used_For_S_price_Default == 2 ? 2 : 1, Default_Purchase_Unit = i.Unit_Used_For_P_price_Default == 2 ? 2 : 1,
                    Description = i.description, Store_Id = i.S_id,
                },
            };
            d.Stores = StoreLines(opp, id);
            d.Factor = opp.db.Database.SqlQuery<decimal>($"SELECT CAST(ISNULL(S_unit_Value, 1) AS decimal(18,4)) AS Value FROM unit WHERE id = {d.Form.Unit_Id}").AsEnumerable().FirstOrDefault();
            if (d.Factor <= 0) d.Factor = 1;
            d.Unit_Locked = d.Stores.Any(s => s.Full_Qua != 0 || s.D_Qua != 0 || s.Movements > 0);
            return d;
        });

        private static List<StoreLine> StoreLines(operations opp, long itemId) => opp.db.Database.SqlQuery<StoreLine>($@"
SELECT ui.id AS Unit_Item_Id, CAST(ISNULL(ui.Store_id, 0) AS bigint) AS Store_Id, ISNULL(s.Store_Name, N'') AS Store_Name,
       CAST(ISNULL(ui.Full_qua, 0) AS decimal(18,4)) AS Full_Qua, CAST(ISNULL(ui.D_Qua, 0) AS decimal(18,4)) AS D_Qua,
       CAST(ISNULL(ui.P_priceAvgCost, 0) AS decimal(18,4)) AS Avg_Cost,
       (SELECT COUNT(*) FROM Detailes_Purshing_Import_Export d WHERE d.Unit_item_Table_Id = ui.id) AS Movements
FROM Unit_item ui LEFT JOIN Store s ON s.id = ui.Store_id
WHERE ui.Item_id = {itemId} ORDER BY s.Store_Name").ToList();

        // ===================== الحفظ =====================

        /// <summary>إضافة صنف (Id = 0) أو تعديله. يرجع المعرّف.</summary>
        public long Save(ItemForm f)
        {
            bool isNew = f.Id == 0;
            if (!_hasPermission(isNew ? PermissionAdd : PermissionEdit))
                throw new InvalidOperationException(isNew ? "ليس لديك صلاحية إضافة صنف جديد." : "ليس لديك صلاحية تعديل بيانات الصنف.");
            Validate(f);
            return RunTx(opp => isNew ? Insert(opp, f) : Update(opp, f));
        }

        private static void Validate(ItemForm f)
        {
            f.Name = (f.Name ?? "").Trim();
            if (f.Name.Length == 0) throw new InvalidOperationException("اسم الصنف مطلوب.");
            if (f.Name.Length > 500) throw new InvalidOperationException("اسم الصنف طويل جداً.");
            if (f.Item_Or_Service is not (1 or 2)) throw new InvalidOperationException("نوع الصنف غير صحيح.");
            if (f.Item_Or_Service == 2) f.Without_Balance = true;   // الخدمة لا رصيد لها (StockMovementService يتجاهلها)
            if (f.Unit_Id == 0) throw new InvalidOperationException("اختر وحدة الصنف.");
            if (f.Category_Id == 0) throw new InvalidOperationException("اختر فئة الصنف.");
            if (new[] { f.Sale_Price_Large, f.Sale_Price_Small, f.Purchase_Price_Large, f.Purchase_Price_Small, f.Reorder_Level }.Any(v => v < 0))
                throw new InvalidOperationException("الأسعار وحد الطلب لا تقبل قيماً سالبة.");
            if (f.Default_Sale_Unit is not (1 or 2) || f.Default_Purchase_Unit is not (1 or 2)) throw new InvalidOperationException("الوحدة الافتراضية غير صحيحة.");
            f.Code = string.IsNullOrWhiteSpace(f.Code) ? null : f.Code.Trim();
        }

        private long Insert(operations opp, ItemForm f)
        {
            var unit = UnitOf(opp, f.Unit_Id);
            if (f.Store_Id == 0) throw new InvalidOperationException("اختر المخزن.");
            int stores = opp.db.Database.SqlQuery<int>($"SELECT COUNT(*) AS Value FROM Store WHERE id = {f.Store_Id} AND Active = 1").AsEnumerable().First();
            if (stores == 0) throw new InvalidOperationException("المخزن غير موجود أو موقوف.");
            CheckCategory(opp, f.Category_Id);
            bool opening = f.Opening_Large > 0 || f.Opening_Small > 0;
            if (opening && f.Without_Balance) throw new InvalidOperationException("الصنف بلا رصيد (خدمة) لا يقبل رصيداً ابتدائياً.");
            if (f.Opening_Large < 0 || f.Opening_Small < 0 || f.Opening_Cost_Large < 0) throw new InvalidOperationException("الرصيد الابتدائى لا يقبل قيماً سالبة.");

            string code = f.Code;
            if (Common.UseAutomaticItemCode == 1 || code == null || code == "0")
                code = opp.db.Database.SqlQuery<long>($@"SELECT CAST(ISNULL(MAX(TRY_CAST(code AS bigint)), 999) + 1 AS bigint) AS Value FROM Item WITH (UPDLOCK, HOLDLOCK)
WHERE TRY_CAST(code AS bigint) >= 1000").AsEnumerable().First().ToString();
            EnsureUniqueCode(opp, code, 0);
            EnsureUniqueBarcodes(opp, f, 0);

            DateTime now = DateTime.Now;
            var im = new Items();
            Apply(im, f, unit);
            im.code = code;
            im.InternalCode = long.TryParse(code, out var ic) ? ic : 0;
            im.S_id = f.Store_Id;
            im.StoreUsed_Type = 1;
            im.User_Id = Common.User_Id;
            im.Branch_Id = Common.Branch_Id;
            opp.db.Item.Add(im);
            opp.db.SaveChanges();

            var um = NewUnitItem(im, f, unit, f.Store_Id, code);
            opp.db.Unit_item.Add(um);
            opp.db.SaveChanges();

            if (Common.IsAddAllItemInAllStores == 1)
            {
                var others = opp.db.Database.SqlQuery<long>($@"SELECT id AS Value FROM Store WHERE id <> {f.Store_Id} AND Active = 1 AND StoreUsed_Type = 1 AND ISNULL(IS_ItemSecond_degree, 0) = 0").ToList();
                foreach (long s in others) opp.db.Unit_item.Add(NewUnitItem(im, f, unit, s, code));
                if (others.Count > 0) opp.db.SaveChanges();
            }
            if (Common.IsAddAllItemForAllCars == 1)
                new ItemsForCarsCLS(ref opp).AddItemsForCarWhenAddItems(im);

            if (opening)
            {
                var period = GetCurrentPeriod(opp) ?? throw new InvalidOperationException("لا توجد فترة مالية للتاريخ الحالى لترحيل الرصيد الابتدائى.");
                new ItemOpeningBalanceService(opp).Post(period.Id, um.id, f.Opening_Large, f.Opening_Small, 0, f.Opening_Cost_Large, ItemOpeningBalanceService.OpeningSource.Manual);
            }
            return im.Id;
        }

        private long Update(operations opp, ItemForm f)
        {
            var im = opp.db.Item.FirstOrDefault(x => x.Id == f.Id) ?? throw new InvalidOperationException("الصنف غير موجود.");
            if ((f.Category_Id != (im.Item_Category_ID ?? 0))) CheckCategory(opp, f.Category_Id);
            var unit = UnitOf(opp, f.Unit_Id);
            bool unitChanged = (im.Unit_Id ?? 0) != f.Unit_Id;
            if (unitChanged)
            {
                var lines = StoreLines(opp, f.Id);
                if (lines.Any(s => s.Full_Qua != 0 || s.D_Qua != 0 || s.Movements > 0))
                    throw new InvalidOperationException("لا تتغير وحدة صنف له رصيد أو حركات، لأن الأرصدة محسوبة بمعامل الوحدة.");
            }
            bool wasWithoutBalance = im.Is_ItemWithoutBalance == 1;
            if (wasWithoutBalance != f.Without_Balance)
            {
                var lines = StoreLines(opp, f.Id);
                if (lines.Any(s => s.Full_Qua != 0 || s.D_Qua != 0 || s.Movements > 0))
                    throw new InvalidOperationException("لا يتغير «بلا رصيد / خدمة» لصنف له رصيد أو حركات مخزن.");
            }
            string code = f.Code ?? im.code;
            if (code != im.code) EnsureUniqueCode(opp, code, f.Id);
            EnsureUniqueBarcodes(opp, f, f.Id);

            Apply(im, f, unit);
            im.code = code;
            if (long.TryParse(code, out var ic)) im.InternalCode = ic;
            im.Edit_User_Id = Common.User_Id;
            opp.db.SaveChanges();

            // أسعار ووحدة كل أسطر الصنف — بلا مساس بالكميات
            DateTime now = DateTime.Now;
            int sDef = f.Default_Sale_Unit, pDef = f.Default_Purchase_Unit, nb = f.Without_Balance ? 1 : 0;
            opp.db.Database.ExecuteSql($@"UPDATE Unit_item SET
    S_price = {f.Sale_Price_Large}, P_price = {f.Purchase_Price_Large},
    S_price_LargeUnit = {f.Sale_Price_Large}, S_price_SmallUnit = {f.Sale_Price_Small},
    P_price_LargeUnit = {f.Purchase_Price_Large}, P_price_SmallUnit = {f.Purchase_Price_Small},
    Unit_Used_For_S_price_Default = {sDef}, Unit_Used_For_P_price_Default = {pDef}, Upon_Request = {f.Reorder_Level},
    Is_ItemWithoutBalance = {nb}, active_flage = {(f.Is_Active ? 1 : -1)}, Code = CASE WHEN Code IS NULL OR Code = N'0' OR Code = {im.code} THEN {code} ELSE Code END,
    Update_ServerDateTime = {now}, EditOperationDate = {now}, Edit_User_Id = {Common.User_Id}
WHERE Item_id = {f.Id}");
            if (unitChanged)
                opp.db.Database.ExecuteSql($"UPDATE Unit_item SET unit_id = {f.Unit_Id}, Big_unit = 1, Small_unit = {unit.Factor} WHERE Item_id = {f.Id}");
            return f.Id;
        }

        /// <summary>إضافة الصنف لمخزن آخر برصيد صفر (يمكن إدخال رصيده بعد ذلك بإذن أو أول مدة).</summary>
        public long AddToStore(long itemId, long storeId)
        {
            if (!_hasPermission(PermissionEdit)) throw new InvalidOperationException("ليس لديك صلاحية تعديل بيانات الصنف.");
            return RunTx(opp =>
            {
                var im = opp.db.Item.AsNoTracking().FirstOrDefault(x => x.Id == itemId) ?? throw new InvalidOperationException("الصنف غير موجود.");
                int exists = opp.db.Database.SqlQuery<int>($"SELECT COUNT(*) AS Value FROM Unit_item WHERE Item_id = {itemId} AND Store_id = {storeId}").AsEnumerable().First();
                if (exists > 0) throw new InvalidOperationException("الصنف موجود فى هذا المخزن بالفعل.");
                int st = opp.db.Database.SqlQuery<int>($"SELECT COUNT(*) AS Value FROM Store WHERE id = {storeId} AND Active = 1").AsEnumerable().First();
                if (st == 0) throw new InvalidOperationException("المخزن غير موجود أو موقوف.");
                var unit = UnitOf(opp, im.Unit_Id ?? 0);
                var f = new ItemForm
                {
                    Sale_Price_Large = im.S_price_LargeUnit ?? 0, Sale_Price_Small = im.S_price_SmallUnit ?? 0,
                    Purchase_Price_Large = im.P_price_LargeUnit ?? 0, Purchase_Price_Small = im.P_price_SmallUnit ?? 0,
                    Default_Sale_Unit = im.Unit_Used_For_S_price_Default == 2 ? 2 : 1, Default_Purchase_Unit = im.Unit_Used_For_P_price_Default == 2 ? 2 : 1,
                    Without_Balance = im.Is_ItemWithoutBalance == 1, Reorder_Level = im.Upon_Request ?? 0, Is_Active = true,
                };
                var um = NewUnitItem(im, f, unit, storeId, im.code);
                opp.db.Unit_item.Add(um);
                opp.db.SaveChanges();
                return um.id;
            });
        }

        // ===================== مساعدات =====================

        private static void Apply(Items im, ItemForm f, UnitOption unit)
        {
            im.Item_Name = f.Name;
            im.ItemName_OnInvoice = string.IsNullOrWhiteSpace(f.Name_On_Invoice) ? f.Name : f.Name_On_Invoice.Trim();
            im.AnotherName = f.Other_Name?.Trim();
            im.BarCode2 = string.IsNullOrWhiteSpace(f.Barcode2) ? null : f.Barcode2.Trim();
            im.BarCode3 = string.IsNullOrWhiteSpace(f.Barcode3) ? null : f.Barcode3.Trim();
            im.Item_Category_ID = f.Category_Id;
            im.Items_GroupsId = f.Group_Id;
            im.Items_NaturesId = f.Nature_Id;
            im.Is_ItemOrService = f.Item_Or_Service;
            im.Is_ItemWithoutBalance = f.Without_Balance ? 1 : 0;
            im.Active_flage = f.Is_Active ? 1 : -1;
            im.Unit_Id = f.Unit_Id;
            im.UnitStr = unit.Big_Unit;
            im.No3eboa = unit.Factor;
            im.Upon_Request = f.Reorder_Level;
            im.S_Price = f.Sale_Price_Large;
            im.P_Price = f.Purchase_Price_Large;
            im.S_price_LargeUnit = f.Sale_Price_Large;
            im.S_price_SmallUnit = f.Sale_Price_Small;
            im.P_price_LargeUnit = f.Purchase_Price_Large;
            im.P_price_SmallUnit = f.Purchase_Price_Small;
            im.Unit_Used_for_Price = 1;
            im.Unit_Used_For_S_price_Default = f.Default_Sale_Unit;
            im.Unit_Used_For_P_price_Default = f.Default_Purchase_Unit;
            im.description = string.IsNullOrWhiteSpace(f.Description) ? null : f.Description.Trim();
        }

        private static Unit_item NewUnitItem(Items im, ItemForm f, UnitOption unit, long storeId, string code) => new Unit_item
        {
            Item_Id = im.Id, Store_id = storeId, Store_Type = 4, Code = code, unit_id = unit.Id,
            Full_qua = 0, D_Qua = 0, Item_No = 0, Reserved_Balance_Larg_Qnty = 0, Reserved_Balance_Small_Qnty = 0,
            Big_unit = 1, Small_unit = unit.Factor,
            S_price = f.Sale_Price_Large, P_price = f.Purchase_Price_Large,
            S_price_LargeUnit = f.Sale_Price_Large, S_price_SmallUnit = f.Sale_Price_Small,
            P_price_LargeUnit = f.Purchase_Price_Large, P_price_SmallUnit = f.Purchase_Price_Small,
            Unit_Used_For_S_price_Default = f.Default_Sale_Unit, Unit_Used_For_P_price_Default = f.Default_Purchase_Unit,
            S_price_Item_No = 0, P_price_Item_No = 0, TaxSales10_Value = 0, Upon_Request = f.Reorder_Level,
            Is_ItemWithoutBalance = f.Without_Balance ? 1 : 0, active_flage = f.Is_Active ? 1 : -1,
            User_Id = Common.User_Id, Branch_Id = Common.Branch_Id,
        };

        private static UnitOption UnitOf(operations opp, long unitId) =>
            opp.db.Database.SqlQuery<UnitOption>($@"SELECT id AS Id, u_name AS Name, B_Unit AS Big_Unit, S_unit AS Small_Unit, CAST(ISNULL(S_unit_Value, 1) AS decimal(18,4)) AS Factor,
       CAST(0 AS bit) AS Is_Default FROM unit WHERE id = {unitId}").AsEnumerable().FirstOrDefault()
            ?? throw new InvalidOperationException("الوحدة غير موجودة.");

        private static void CheckCategory(operations opp, long categoryId)
        {
            int ok = opp.db.Database.SqlQuery<int>($"SELECT COUNT(*) AS Value FROM Item_Category WHERE ID = {categoryId} AND ISNULL(IS_Active, 1) = 1").AsEnumerable().First();
            if (ok == 0) throw new InvalidOperationException("الفئة غير موجودة أو موقوفة.");
        }

        private static void EnsureUniqueCode(operations opp, string code, long exceptId)
        {
            if (code == null || code == "0") return;
            int dup = opp.db.Database.SqlQuery<int>($"SELECT COUNT(*) AS Value FROM Item WHERE code = {code} AND id <> {exceptId}").AsEnumerable().First();
            if (dup > 0) throw new InvalidOperationException($"كود الصنف {code} مستعمل لصنف آخر.");
        }

        private static void EnsureUniqueBarcodes(operations opp, ItemForm f, long exceptId)
        {
            foreach (string b in new[] { f.Barcode2, f.Barcode3 }.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()))
            {
                int dup = opp.db.Database.SqlQuery<int>($@"SELECT COUNT(*) AS Value FROM Item WHERE id <> {exceptId}
  AND (code = {b} OR BarCode2 = {b} OR BarCode3 = {b} OR BarCode4 = {b} OR BarCode5 = {b} OR EXISTS (SELECT 1 FROM Unit_item u WHERE u.Item_id = Item.id AND u.Code = {b}))").AsEnumerable().First();
                if (dup > 0) throw new InvalidOperationException($"الباركود {b} مستعمل لصنف آخر.");
            }
            if (!string.IsNullOrWhiteSpace(f.Barcode2) && f.Barcode2.Trim() == f.Barcode3?.Trim())
                throw new InvalidOperationException("الباركود الثانى والثالث متطابقان.");
        }

        private class PeriodRow
        {
            public long Id { get; set; }
            public DateTime DateFrom { get; set; }
            public DateTime DateTo { get; set; }
            public string Label => $"{DateFrom:yyyy-MM-dd} — {DateTo:yyyy-MM-dd}";
        }

        private static PeriodRow GetCurrentPeriod(operations opp)
        {
            DateTime today = DateTime.Today;
            return opp.db.Database.SqlQuery<PeriodRow>($@"SELECT TOP 1 Id, DateFrom, DateTo FROM Financial_Period
WHERE DateFrom <= {today} AND DateTo >= {today} AND ISNULL(IS_Finished_Posted, 0) = 0 ORDER BY DateFrom").AsEnumerable().FirstOrDefault();
        }

        private T Run<T>(Func<operations, T> f) => f(_newOpp());

        private T RunTx<T>(Func<operations, T> f)
        {
            var opp = _newOpp();
            bool own = opp.tns?.Connection == null;
            if (own) opp.BeginTransaction();
            try { T r = f(opp); if (own) opp.Commit(); return r; }
            catch { if (own) opp.Rollback(); throw; }
        }
    }
}
