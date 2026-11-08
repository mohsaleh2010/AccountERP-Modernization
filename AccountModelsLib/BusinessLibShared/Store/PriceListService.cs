using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib
{
    /// <summary>
    /// أسعار الأصناف (خطة 04 — المرحلة 5) بدل Price_Type_ListFRM / Prices_List_Values / Edit_Prices_List_Values / Customer_Price / Suppliers_Price.
    /// قرارات المستخدم (2026-10-10):
    /// - Price_Type_Sell_Values.PriceUnit صار decimal(18,4) (كان bigint) — خطوة PriceUnitDecimal فى StockEngineSchemaCLS.
    /// - القوائم المشتقة كالقديم: تغيير سعر قائمة لصنف يعيد حساب كل القوائم المشتقة منها (على كل المستويات)، وتغيير قاعدة قائمة يعيد حسابها لكل الأصناف.
    /// - التقريب: المشتقة لأقرب 0.01؛ التعديل الجماعى بخطوة تقريب يختارها المستخدم.
    /// - الصلاحية 917 (تعديل بيانات الصنف) لأى تغيير فى الأسعار.
    /// - سعر العميل الخاص يغلب نوع السعر فى فاتورة المبيعات (GetCustomerPrice).
    /// السعر فى القوائم وأسعار العملاء والموردين = سعر الوحدة الكبرى (كما يقرؤه حوار صنف الفاتورة).
    /// </summary>
    public class PriceListService
    {
        public const int PermissionEdit = 917;

        private readonly Func<operations> _newOpp;
        private readonly Func<int, bool> _hasPermission;

        public PriceListService(Func<operations> newOpp = null, Func<int, bool> hasPermission = null)
        {
            _newOpp = newOpp ?? (() => new operations());
            _hasPermission = hasPermission ?? (p => Common.IsHavePermission(Common.User_Id, p, Common.Project_ID, Common.Branch_Id));
        }

        // ===================== النماذج =====================

        public class PriceType
        {
            public long Id { get; set; }
            public string Name { get; set; }
            public bool Is_Active { get; set; } = true;
            /// <summary>القائمة الأم؛ 0 = قائمة أساسية مستقلة (Price_Type_List_Master_Id = -1 كالقديم).</summary>
            public long Master_Id { get; set; }
            public string Master_Name { get; set; }
            /// <summary>true = زيادة، false = نقص.</summary>
            public bool Is_Plus { get; set; } = true;
            public decimal Added_Value { get; set; }
            /// <summary>true = نسبة مئوية، false = قيمة.</summary>
            public bool Is_Rate { get; set; } = true;
            public int Priced_Items { get; set; }
            public int Customers { get; set; }
            public int Children { get; set; }
        }

        public class ItemPrice
        {
            public long Item_Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string Category { get; set; }
            public string Unit { get; set; }
            public decimal Sale_Price { get; set; }
            /// <summary>null = لا سعر لهذا الصنف فى القائمة.</summary>
            public decimal? Price { get; set; }
            public decimal Commission_Value { get; set; }
        }

        public class PriceFilter
        {
            public string Text { get; set; }
            public long Category_Id { get; set; }
            public long Group_Id { get; set; }
            /// <summary>true = المسعَّرة فقط.</summary>
            public bool Priced_Only { get; set; }
            public int Top { get; set; } = 500;
        }

        public enum BulkTarget { Sale = 1, Purchase = 2, PriceType = 3 }

        public class BulkRequest
        {
            public BulkTarget Target { get; set; } = BulkTarget.Sale;
            public long Price_Type_Id { get; set; }
            public long Category_Id { get; set; }
            public long Group_Id { get; set; }
            public string Text { get; set; }
            public bool Is_Rate { get; set; } = true;
            public bool Is_Plus { get; set; } = true;
            public decimal Value { get; set; }
            /// <summary>خطوة التقريب: 0.01، 0.05، 0.25، 0.5، 1، 5، 10.</summary>
            public decimal Rounding { get; set; } = 0.01m;
        }

        public class BulkRow
        {
            public long Item_Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public decimal Old_Price { get; set; }
            public decimal New_Price { get; set; }
            public decimal Factor { get; set; } = 1;
        }

        public class PartyPrice
        {
            public long Id { get; set; }
            public long Party_Id { get; set; }
            public string Party_Name { get; set; }
            public long Item_Id { get; set; }
            public string Item_Code { get; set; }
            public string Item_Name { get; set; }
            public string Unit { get; set; }
            public decimal Item_Price { get; set; }
            public decimal Price { get; set; }
            /// <summary>للموردين فقط: نوع سعر الشراء (0 = بدون).</summary>
            public long Purchase_Type_Id { get; set; }
            public string Purchase_Type_Name { get; set; }
        }

        public class Option
        {
            public long Id { get; set; }
            public string Name { get; set; }
        }

        public static readonly decimal[] Roundings = { 0.01m, 0.05m, 0.25m, 0.5m, 1m, 5m, 10m };

        // ===================== أنواع الأسعار =====================

        public List<PriceType> ListTypes() => Run(opp => opp.db.Database.SqlQuery<PriceType>($@"
SELECT t.ID AS Id, ISNULL(LTRIM(RTRIM(t.Price_Name)), N'') AS Name, CAST(CASE WHEN ISNULL(t.IS_Active, 1) = 1 THEN 1 ELSE 0 END AS bit) AS Is_Active,
       CAST(CASE WHEN ISNULL(t.Price_Type_List_Master_Id, -1) <= 0 OR t.Price_Type_List_Master_Id = t.ID THEN 0 ELSE t.Price_Type_List_Master_Id END AS bigint) AS Master_Id,
       ISNULL(LTRIM(RTRIM(m.Price_Name)), N'') AS Master_Name,
       CAST(CASE WHEN LTRIM(RTRIM(ISNULL(t.Plus_Minus, N'+'))) = N'-' THEN 0 ELSE 1 END AS bit) AS Is_Plus,
       CAST(ISNULL(t.Added_Value, 0) AS decimal(18,4)) AS Added_Value, CAST(CASE WHEN t.IS_Rate = 1 THEN 1 ELSE 0 END AS bit) AS Is_Rate,
       (SELECT COUNT(*) FROM Price_Type_Sell_Values v WHERE v.Price_Type_List_ID = t.ID) AS Priced_Items,
       (SELECT COUNT(*) FROM Customer c WHERE c.Price_Type_List_ID = t.ID) AS Customers,
       (SELECT COUNT(*) FROM Price_Type_List k WHERE k.Price_Type_List_Master_Id = t.ID AND k.ID <> t.ID) AS Children
FROM Price_Type_List t
LEFT JOIN Price_Type_List m ON m.ID = t.Price_Type_List_Master_Id AND m.ID <> t.ID
ORDER BY t.ID").ToList());

        /// <summary>إضافة/تعديل نوع سعر. تغيير قاعدة نوع مشتق (الأم/الزيادة/النسبة) يعيد حساب أسعاره وأسعار ما يتفرع منه لكل الأصناف.</summary>
        public long SaveType(PriceType t)
        {
            Demand();
            string name = (t.Name ?? "").Trim();
            if (name.Length == 0) throw new InvalidOperationException("اسم نوع السعر مطلوب.");
            if (name.Length > 200) throw new InvalidOperationException("اسم نوع السعر طويل جداً.");
            if (t.Added_Value < 0) throw new InvalidOperationException("قيمة الزيادة/النقص لا تكون سالبة؛ اختر «نقص» بدلاً من ذلك.");
            if (t.Master_Id != 0 && t.Is_Rate && !t.Is_Plus && t.Added_Value >= 100) throw new InvalidOperationException("نسبة النقص يجب أن تقل عن 100%.");
            return RunTx(opp =>
            {
                var all = LoadTypes(opp);
                if (all.Any(x => x.Id != t.Id && string.Equals(x.Name.Trim(), name, StringComparison.CurrentCultureIgnoreCase)))
                    throw new InvalidOperationException("اسم نوع السعر موجود بالفعل.");
                string masterName = null;
                if (t.Master_Id != 0)
                {
                    var master = all.FirstOrDefault(x => x.Id == t.Master_Id) ?? throw new InvalidOperationException("القائمة الأم غير موجودة.");
                    if (t.Id != 0 && (t.Master_Id == t.Id || Descendants(all, t.Id).Contains(t.Master_Id)))
                        throw new InvalidOperationException("لا تُشتق القائمة من نفسها أو من قائمة مشتقة منها.");
                    masterName = master.Name;
                }
                int masterId = t.Master_Id == 0 ? -1 : checked((int)t.Master_Id);
                string pm = t.Master_Id == 0 ? null : (t.Is_Plus ? "+" : "-");
                decimal added = t.Master_Id == 0 ? 0 : t.Added_Value;
                int isRate = t.Master_Id != 0 && t.Is_Rate ? 1 : 0;
                DateTime now = DateTime.Now;
                if (t.Id == 0)
                {
                    long id = opp.db.Database.SqlQuery<long>($"SELECT CAST(ISNULL(MAX(ID), 0) + 1 AS bigint) AS Value FROM Price_Type_List WITH (UPDLOCK, HOLDLOCK)").AsEnumerable().First();
                    if (IsIdentity(opp, "Price_Type_List", "ID"))
                        id = opp.db.Database.SqlQuery<long>($@"INSERT INTO Price_Type_List (Price_Name, IS_Active, Price_Name_Master, Price_Type_List_Master_Id, Plus_Minus, Added_Value, IS_Rate, OperationDate, User_Id, Branch_Id, Insert_ServerDateTime)
OUTPUT CAST(INSERTED.ID AS bigint) AS Value VALUES ({name}, {(t.Is_Active ? 1 : 0)}, {masterName}, {masterId}, {pm}, {added}, {isRate}, {now.Date}, {Common.User_Id}, {Common.Branch_Id}, {now})").AsEnumerable().First();
                    else
                        opp.db.Database.ExecuteSql($@"INSERT INTO Price_Type_List (ID, Price_Name, IS_Active, Price_Name_Master, Price_Type_List_Master_Id, Plus_Minus, Added_Value, IS_Rate, OperationDate, User_Id, Branch_Id, Insert_ServerDateTime)
VALUES ({id}, {name}, {(t.Is_Active ? 1 : 0)}, {masterName}, {masterId}, {pm}, {added}, {isRate}, {now.Date}, {Common.User_Id}, {Common.Branch_Id}, {now})");
                    if (t.Master_Id != 0) RecomputeTypeAllItems(opp, id);
                    return id;
                }
                var old = all.FirstOrDefault(x => x.Id == t.Id) ?? throw new InvalidOperationException("نوع السعر غير موجود.");
                bool ruleChanged = old.Master_Id != t.Master_Id || (t.Master_Id != 0 && (old.Is_Plus != t.Is_Plus || old.Added_Value != added || old.Is_Rate != (isRate == 1)));
                opp.db.Database.ExecuteSql($@"UPDATE Price_Type_List SET Price_Name = {name}, IS_Active = {(t.Is_Active ? 1 : 0)}, Price_Name_Master = {masterName},
    Price_Type_List_Master_Id = {masterId}, Plus_Minus = {pm}, Added_Value = {added}, IS_Rate = {isRate},
    Update_ServerDateTime = {now}, EditOperationDate = {now}, Edit_User_Id = {Common.User_Id} WHERE ID = {t.Id}");
                if (old.Name != name)
                    opp.db.Database.ExecuteSql($"UPDATE Price_Type_List SET Price_Name_Master = {name} WHERE Price_Type_List_Master_Id = {t.Id} AND ID <> {t.Id}");
                if (ruleChanged && t.Master_Id != 0) RecomputeTypeAllItems(opp, t.Id);
                return t.Id;
            });
        }

        /// <summary>الحذف للنوع غير المستعمل فقط (بلا أسعار ولا عملاء ولا قوائم مشتقة)؛ وإلا يُوقف.</summary>
        public void DeleteType(long id)
        {
            Demand();
            RunTx(opp =>
            {
                var t = LoadTypes(opp).FirstOrDefault(x => x.Id == id) ?? throw new InvalidOperationException("نوع السعر غير موجود.");
                if (t.Children > 0) throw new InvalidOperationException($"تُشتق منه {t.Children} قائمة؛ احذفها أو غيّر أمها أولاً.");
                if (t.Customers > 0) throw new InvalidOperationException($"مرتبط بـ {t.Customers} عميل؛ أوقفه بدلاً من الحذف.");
                if (t.Priced_Items > 0) throw new InvalidOperationException($"فيه أسعار لـ {t.Priced_Items} صنف؛ أوقفه بدلاً من الحذف.");
                return opp.db.Database.ExecuteSql($"DELETE FROM Price_Type_List WHERE ID = {id}");
            });
        }

        // ===================== أسعار قائمة =====================

        public List<ItemPrice> ListPrices(long typeId, PriceFilter f)
        {
            string t = (f.Text ?? "").Trim();
            string like = "%" + t + "%";
            int top = Math.Clamp(f.Top, 1, 5000);
            int pricedOnly = f.Priced_Only ? 1 : 0;
            return Run(opp => opp.db.Database.SqlQuery<ItemPrice>($@"
SELECT TOP ({top}) i.id AS Item_Id, ISNULL(i.code, N'') AS Code, ISNULL(i.Item_Name, N'') AS Name, ISNULL(c.Category_Name, N'') AS Category,
       ISNULL(u.B_Unit, N'') AS Unit, CAST(ISNULL(i.S_price_LargeUnit, ISNULL(i.S_Price, 0)) AS decimal(18,4)) AS Sale_Price,
       CAST(v.PriceUnit AS decimal(18,4)) AS Price, CAST(ISNULL(v.CommisionValue, 0) AS decimal(18,4)) AS Commission_Value
FROM Item i
LEFT JOIN Item_Category c ON c.ID = i.Item_Category_ID
LEFT JOIN unit u ON u.id = i.Unit_Id
OUTER APPLY (SELECT TOP 1 PriceUnit, CommisionValue FROM Price_Type_Sell_Values x WHERE x.Item_ID = i.id AND x.Price_Type_List_ID = {typeId} ORDER BY x.ID) v
WHERE ISNULL(i.Active_flage, 1) NOT IN (0, -1)
  AND ({t} = N'' OR i.Item_Name LIKE {like} OR i.code = {t} OR i.BarCode2 = {t} OR i.BarCode3 = {t})
  AND ({f.Category_Id} = 0 OR i.Item_Category_ID = {f.Category_Id})
  AND ({f.Group_Id} = 0 OR i.Items_GroupsId = {f.Group_Id})
  AND ({pricedOnly} = 0 OR v.PriceUnit IS NOT NULL)
ORDER BY i.Item_Name").ToList());
        }

        public class PriceSheetRow
        {
            public long Item_Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string Category { get; set; }
            public string Unit { get; set; }
            public decimal Sale_Price { get; set; }
            /// <summary>سعر الصنف فى كل قائمة مختارة (المفتاح رقم القائمة)؛ غياب المفتاح = لا سعر للصنف فى القائمة.</summary>
            public Dictionary<long, decimal> Prices { get; set; } = new();
        }

        private class SheetPrice
        {
            public long Item_Id { get; set; }
            public long Type_Id { get; set; }
            public decimal Price { get; set; }
        }

        /// <summary>
        /// كشف أسعار للطباعة (خطة 05 بند 26، بدل ItemPriceFRM «طباعة أسعار الأصناف»): الأصناف بسعر البيع الأساسى وسعر كل قائمة مختارة.
        /// Priced_Only يقصر النتيجة على الأصناف التى لها سعر فى قائمة مختارة واحدة على الأقل.
        /// </summary>
        public List<PriceSheetRow> PriceSheet(IReadOnlyCollection<long> typeIds, PriceFilter f)
        {
            var ids = (typeIds ?? Array.Empty<long>()).Distinct().ToList();
            var rows = ListPrices(0, new PriceFilter { Text = f.Text, Category_Id = f.Category_Id, Group_Id = f.Group_Id, Top = f.Top })
                .Select(r => new PriceSheetRow { Item_Id = r.Item_Id, Code = r.Code, Name = r.Name, Category = r.Category, Unit = r.Unit, Sale_Price = r.Sale_Price })
                .ToList();
            if (ids.Count == 0 || rows.Count == 0) return f.Priced_Only && ids.Count > 0 ? new() : rows;
            // أرقام القوائم أعداد صحيحة من الواجهة فتُضمَّن نصاً فى IN بأمان
            string inList = string.Join(",", ids);
            var prices = Run(opp => opp.db.Database.SqlQueryRaw<SheetPrice>(
                // أول صف لكل صنف وقائمة (بالترتيب ID) كما تقرأه ListPrices
                "SELECT Item_Id, Type_Id, Price FROM (SELECT CAST(Item_ID AS bigint) AS Item_Id, CAST(Price_Type_List_ID AS bigint) AS Type_Id, " +
                "CAST(PriceUnit AS decimal(18,4)) AS Price, ROW_NUMBER() OVER (PARTITION BY Item_ID, Price_Type_List_ID ORDER BY ID) AS Rn " +
                "FROM Price_Type_Sell_Values WHERE Price_Type_List_ID IN (" + inList + ") AND PriceUnit IS NOT NULL) x WHERE Rn = 1").ToList());
            var byItem = prices.GroupBy(p => p.Item_Id).ToDictionary(g => g.Key, g => g.ToDictionary(x => x.Type_Id, x => x.Price));
            foreach (var r in rows)
                if (byItem.TryGetValue(r.Item_Id, out var d)) r.Prices = d;
            return f.Priced_Only ? rows.Where(r => r.Prices.Values.Any(v => v != 0)).ToList() : rows;
        }

        /// <summary>القوائم الأكثر تسعيراً للأصناف النشطة (سعر غير صفرى) لاختيار كشف الأسعار الافتراضى؛ Priced_Items يعدّ كل الصفوف.</summary>
        public List<long> MostPricedTypes(int take) => Run(opp => opp.db.Database.SqlQuery<long>($@"
SELECT TOP ({take}) CAST(v.Price_Type_List_ID AS bigint) AS Value
FROM Price_Type_Sell_Values v
INNER JOIN Item i ON i.id = v.Item_ID AND ISNULL(i.Active_flage, 1) NOT IN (0, -1)
INNER JOIN Price_Type_List t ON t.ID = v.Price_Type_List_ID AND ISNULL(t.IS_Active, 1) = 1
WHERE ISNULL(v.PriceUnit, 0) <> 0
GROUP BY v.Price_Type_List_ID
ORDER BY COUNT(DISTINCT v.Item_ID) DESC, v.Price_Type_List_ID").ToList());

        /// <summary>سعر صنف فى قائمة (null = حذف السعر من القائمة)، ثم إعادة حساب كل القوائم المشتقة منها لهذا الصنف (كالقديم).</summary>
        public void SetPrice(long typeId, long itemId, decimal? price)
        {
            Demand();
            if (price < 0) throw new InvalidOperationException("السعر لا يكون سالباً.");
            RunTx(opp =>
            {
                var all = LoadTypes(opp);
                if (all.All(x => x.Id != typeId)) throw new InvalidOperationException("نوع السعر غير موجود.");
                if (opp.db.Database.SqlQuery<int>($"SELECT COUNT(*) AS Value FROM Item WHERE id = {itemId}").AsEnumerable().First() == 0)
                    throw new InvalidOperationException("الصنف غير موجود.");
                if (price == null) opp.db.Database.ExecuteSql($"DELETE FROM Price_Type_Sell_Values WHERE Item_ID = {itemId} AND Price_Type_List_ID = {typeId}");
                else Upsert(opp, typeId, itemId, Math.Round(price.Value, 4));
                RecomputeChildren(opp, all, typeId, new[] { itemId });
                return 0;
            });
        }

        // ===================== تصنيف أسعار مبيعات الأصناف (بدل Prices_List_Edit_Values) =====================

        /// <summary>صف سعر مسجَّل فى Price_Type_Sell_Values: نوع السعر + الصنف + السعر + خصم البيع (CommisionValue قيمة لا نسبة).</summary>
        public class SellValue
        {
            public long Type_Id { get; set; }
            public string Type_Name { get; set; }
            /// <summary>true = القائمة مشتقة من أخرى (سعرها يُعاد حسابه عند تغيير الأم).</summary>
            public bool Is_Derived { get; set; }
            public long Item_Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string Category { get; set; }
            public string Unit { get; set; }
            /// <summary>سعر البيع الأساسى فى بطاقة الصنف (للمقارنة).</summary>
            public decimal Item_Sale_Price { get; set; }
            public decimal Price { get; set; }
            public decimal Discount { get; set; }
            /// <summary>false = لا سعر مسجَّل للصنف فى هذا النوع (يظهر فقط مع includeUnpriced).</summary>
            public bool Has_Price { get; set; } = true;
        }

        /// <summary>
        /// الأسعار المسجَّلة (كشاشة «قيم أسعار بيع الأصناف» القديمة): typeId = 0 كل الأنواع، categoryId = 0 كل الفئات.
        /// صف واحد لكل (صنف × نوع) — أول صف بالترتيب ID كما تقرؤه ListPrices.
        /// includeUnpriced = true: كل الأصناف النشطة × الأنواع (النوع المختار، أو كل الأنواع النشطة) سواء لها سعر أم لا؛
        /// الصف بلا سعر Has_Price = false والسعر والخصم صفر. الأسعار المسجَّلة تظهر دائماً حتى للصنف/النوع الموقوف.
        /// </summary>
        public List<SellValue> ListSellValues(long typeId, long categoryId, string text, int top = 1000, bool includeUnpriced = false)
        {
            string t = (text ?? "").Trim();
            string like = "%" + t + "%";
            int n = Math.Clamp(top, 1, 5000);
            if (includeUnpriced)
                return Run(opp => opp.db.Database.SqlQuery<SellValue>($@"
SELECT TOP ({n}) CAST(p.ID AS bigint) AS Type_Id, ISNULL(LTRIM(RTRIM(p.Price_Name)), N'') AS Type_Name,
       CAST(CASE WHEN ISNULL(p.Price_Type_List_Master_Id, -1) <= 0 OR p.Price_Type_List_Master_Id = p.ID THEN 0 ELSE 1 END AS bit) AS Is_Derived,
       CAST(i.id AS bigint) AS Item_Id, ISNULL(i.code, N'') AS Code, ISNULL(i.Item_Name, N'') AS Name, ISNULL(c.Category_Name, N'') AS Category,
       ISNULL(u.B_Unit, N'') AS Unit, CAST(ISNULL(i.S_price_LargeUnit, ISNULL(i.S_Price, 0)) AS decimal(18,4)) AS Item_Sale_Price,
       CAST(ISNULL(v.PriceUnit, 0) AS decimal(18,4)) AS Price, CAST(ISNULL(v.CommisionValue, 0) AS decimal(18,4)) AS Discount,
       CAST(CASE WHEN v.Item_ID IS NULL THEN 0 ELSE 1 END AS bit) AS Has_Price
FROM Item i
CROSS JOIN Price_Type_List p
LEFT JOIN (SELECT Item_ID, Price_Type_List_ID, PriceUnit, CommisionValue,
                  ROW_NUMBER() OVER (PARTITION BY Item_ID, Price_Type_List_ID ORDER BY ID) AS Rn
           FROM Price_Type_Sell_Values WHERE ({typeId} = 0 OR Price_Type_List_ID = {typeId})) v
       ON v.Item_ID = i.id AND v.Price_Type_List_ID = p.ID AND v.Rn = 1
LEFT JOIN Item_Category c ON c.ID = i.Item_Category_ID
LEFT JOIN unit u ON u.id = i.Unit_Id
WHERE ({typeId} = 0 OR p.ID = {typeId})
  AND (v.Item_ID IS NOT NULL OR (ISNULL(i.Active_flage, 1) NOT IN (0, -1) AND ({typeId} <> 0 OR ISNULL(p.IS_Active, 1) = 1)))
  AND ({categoryId} = 0 OR i.Item_Category_ID = {categoryId})
  AND ({t} = N'' OR i.Item_Name LIKE {like} OR i.code = {t} OR i.BarCode2 = {t} OR i.BarCode3 = {t})
ORDER BY p.Price_Name, c.Category_Name, i.Item_Name").ToList());
            return Run(opp => opp.db.Database.SqlQuery<SellValue>($@"
SELECT TOP ({n}) CAST(v.Price_Type_List_ID AS bigint) AS Type_Id, ISNULL(LTRIM(RTRIM(p.Price_Name)), N'') AS Type_Name,
       CAST(CASE WHEN ISNULL(p.Price_Type_List_Master_Id, -1) <= 0 OR p.Price_Type_List_Master_Id = p.ID THEN 0 ELSE 1 END AS bit) AS Is_Derived,
       CAST(i.id AS bigint) AS Item_Id, ISNULL(i.code, N'') AS Code, ISNULL(i.Item_Name, N'') AS Name, ISNULL(c.Category_Name, N'') AS Category,
       ISNULL(u.B_Unit, N'') AS Unit, CAST(ISNULL(i.S_price_LargeUnit, ISNULL(i.S_Price, 0)) AS decimal(18,4)) AS Item_Sale_Price,
       CAST(ISNULL(v.PriceUnit, 0) AS decimal(18,4)) AS Price, CAST(ISNULL(v.CommisionValue, 0) AS decimal(18,4)) AS Discount,
       CAST(1 AS bit) AS Has_Price
FROM (SELECT Item_ID, Price_Type_List_ID, PriceUnit, CommisionValue,
             ROW_NUMBER() OVER (PARTITION BY Item_ID, Price_Type_List_ID ORDER BY ID) AS Rn
      FROM Price_Type_Sell_Values WHERE ({typeId} = 0 OR Price_Type_List_ID = {typeId})) v
INNER JOIN Item i ON i.id = v.Item_ID
INNER JOIN Price_Type_List p ON p.ID = v.Price_Type_List_ID
LEFT JOIN Item_Category c ON c.ID = i.Item_Category_ID
LEFT JOIN unit u ON u.id = i.Unit_Id
WHERE v.Rn = 1
  AND ({categoryId} = 0 OR i.Item_Category_ID = {categoryId})
  AND ({t} = N'' OR i.Item_Name LIKE {like} OR i.code = {t} OR i.BarCode2 = {t} OR i.BarCode3 = {t})
ORDER BY p.Price_Name, c.Category_Name, i.Item_Name").ToList());
        }

        /// <summary>
        /// حفظ سعر صنف فى نوع سعر مع خصم البيع (إضافة أو تعديل). السعر يعيد حساب القوائم المشتقة كـ SetPrice؛
        /// الخصم يخص هذا النوع فقط (لا يُنسخ للمشتقة، كالقديم). السعر يجب أن يكون أكبر من صفر كالشاشة القديمة.
        /// </summary>
        public void SaveSellValue(long typeId, long itemId, decimal price, decimal discount)
        {
            Demand();
            if (typeId == 0) throw new InvalidOperationException("اختر نوع السعر.");
            if (itemId == 0) throw new InvalidOperationException("اختر الصنف.");
            if (price <= 0) throw new InvalidOperationException("أدخل السعر.");
            if (discount < 0) throw new InvalidOperationException("الخصم لا يكون سالباً.");
            if (discount > price) throw new InvalidOperationException("الخصم أكبر من السعر.");
            RunTx(opp =>
            {
                var all = LoadTypes(opp);
                if (all.All(x => x.Id != typeId)) throw new InvalidOperationException("نوع السعر غير موجود.");
                if (opp.db.Database.SqlQuery<int>($"SELECT COUNT(*) AS Value FROM Item WHERE id = {itemId}").AsEnumerable().First() == 0)
                    throw new InvalidOperationException("الصنف غير موجود.");
                Upsert(opp, typeId, itemId, Math.Round(price, 4));
                decimal d = Math.Round(discount, 2);
                opp.db.Database.ExecuteSql($"UPDATE Price_Type_Sell_Values SET CommisionValue = {d} WHERE Item_ID = {itemId} AND Price_Type_List_ID = {typeId}");
                RecomputeChildren(opp, all, typeId, new[] { itemId });
                return 0;
            });
        }

        /// <summary>سعر البيع الأساسى فى بطاقة الصنف (الوحدة الكبرى) — للمقارنة فى نافذة السعر.</summary>
        public decimal GetItemSalePrice(long itemId) => Run(opp => opp.db.Database.SqlQuery<decimal>($@"
SELECT CAST(ISNULL(S_price_LargeUnit, ISNULL(S_Price, 0)) AS decimal(18,4)) AS Value FROM Item WHERE id = {itemId}").AsEnumerable().FirstOrDefault());

        /// <summary>هل للصنف سعر مسجَّل فى هذا النوع؟ (لتنبيه «سيُستبدل السعر الحالى» عند الإضافة).</summary>
        public SellValue GetSellValue(long typeId, long itemId) =>
            typeId == 0 || itemId == 0 ? null : Run(opp => opp.db.Database.SqlQuery<SellValue>($@"
SELECT TOP 1 CAST(Price_Type_List_ID AS bigint) AS Type_Id, CAST(N'' AS nvarchar(1)) AS Type_Name, CAST(0 AS bit) AS Is_Derived,
       CAST(Item_ID AS bigint) AS Item_Id, CAST(N'' AS nvarchar(1)) AS Code, CAST(N'' AS nvarchar(1)) AS Name, CAST(N'' AS nvarchar(1)) AS Category,
       CAST(N'' AS nvarchar(1)) AS Unit, CAST(0 AS decimal(18,4)) AS Item_Sale_Price,
       CAST(ISNULL(PriceUnit, 0) AS decimal(18,4)) AS Price, CAST(ISNULL(CommisionValue, 0) AS decimal(18,4)) AS Discount,
       CAST(1 AS bit) AS Has_Price
FROM Price_Type_Sell_Values WHERE Item_ID = {itemId} AND Price_Type_List_ID = {typeId} ORDER BY ID").AsEnumerable().FirstOrDefault());

        // ===================== بحث أسعار الصنف (بدل SearchItemPrices_List_Values) =====================

        /// <summary>سعر الصنف فى نوع سعر واحد؛ Price = null يعنى لا سعر مسجَّل للصنف فى هذا النوع.</summary>
        public class ItemTypePrice
        {
            public long Type_Id { get; set; }
            public string Type_Name { get; set; }
            public bool Is_Active { get; set; }
            public bool Is_Derived { get; set; }
            public string Master_Name { get; set; }
            public decimal? Price { get; set; }
            public decimal Discount { get; set; }
        }

        /// <summary>بيانات الصنف للعرض أعلى شاشة البحث (الكود، الاسم، الفئة، الوحدة، سعر البطاقة)؛ null إن لم يوجد.</summary>
        public ItemPrice GetItem(long itemId) => itemId == 0 ? null : Run(opp => opp.db.Database.SqlQuery<ItemPrice>($@"
SELECT CAST(i.id AS bigint) AS Item_Id, ISNULL(i.code, N'') AS Code, ISNULL(i.Item_Name, N'') AS Name, ISNULL(c.Category_Name, N'') AS Category,
       ISNULL(u.B_Unit, N'') AS Unit, CAST(ISNULL(i.S_price_LargeUnit, ISNULL(i.S_Price, 0)) AS decimal(18,4)) AS Sale_Price,
       CAST(NULL AS decimal(18,4)) AS Price, CAST(0 AS decimal(18,4)) AS Commission_Value
FROM Item i LEFT JOIN Item_Category c ON c.ID = i.Item_Category_ID LEFT JOIN unit u ON u.id = i.Unit_Id
WHERE i.id = {itemId}").AsEnumerable().FirstOrDefault());

        /// <summary>
        /// كل أنواع الأسعار مع سعر الصنف وخصمه فى كل نوع (القديمة كانت تعرض الأنواع المسعَّرة فقط؛ هنا تظهر الأنواع النشطة كلها
        /// ليُضاف السعر الناقص من نفس الشاشة). النوع الموقوف يظهر فقط إن كان للصنف سعر فيه. أول صف بالترتيب ID كبقية الشاشات.
        /// </summary>
        public List<ItemTypePrice> ListItemPrices(long itemId) => Run(opp => opp.db.Database.SqlQuery<ItemTypePrice>($@"
SELECT CAST(t.ID AS bigint) AS Type_Id, ISNULL(LTRIM(RTRIM(t.Price_Name)), N'') AS Type_Name,
       CAST(CASE WHEN ISNULL(t.IS_Active, 1) = 1 THEN 1 ELSE 0 END AS bit) AS Is_Active,
       CAST(CASE WHEN ISNULL(t.Price_Type_List_Master_Id, -1) <= 0 OR t.Price_Type_List_Master_Id = t.ID THEN 0 ELSE 1 END AS bit) AS Is_Derived,
       ISNULL(LTRIM(RTRIM(m.Price_Name)), N'') AS Master_Name,
       CAST(v.PriceUnit AS decimal(18,4)) AS Price, CAST(ISNULL(v.CommisionValue, 0) AS decimal(18,4)) AS Discount
FROM Price_Type_List t
LEFT JOIN Price_Type_List m ON m.ID = t.Price_Type_List_Master_Id AND m.ID <> t.ID
OUTER APPLY (SELECT TOP 1 PriceUnit, CommisionValue FROM Price_Type_Sell_Values x WHERE x.Item_ID = {itemId} AND x.Price_Type_List_ID = t.ID ORDER BY x.ID) v
WHERE ISNULL(t.IS_Active, 1) = 1 OR v.PriceUnit IS NOT NULL
ORDER BY CASE WHEN v.PriceUnit IS NULL THEN 1 ELSE 0 END, t.Price_Name").ToList());

        /// <summary>إعادة حساب قائمة مشتقة (وما يتفرع منها) لكل الأصناف من أسعار أمها.</summary>
        public int RecomputeType(long typeId)
        {
            Demand();
            return RunTx(opp => RecomputeTypeAllItems(opp, typeId));
        }

        // ===================== تعديل أسعار مبيعات الأصناف (بدل Edit_Prices_List_Values) =====================

        /// <summary>فئة صنف للاختيار فى شاشات الأسعار مع نوعها (Category_Type_Id: 1 مبيعات، 2 مشتريات...).</summary>
        public class CategoryPick
        {
            public long Id { get; set; }
            public string Name { get; set; }
            public int Type_Id { get; set; }
        }

        /// <summary>
        /// فئات الأصناف النشطة مرتبة: فئات النوع المفضَّل أولاً (1 = مبيعات لشاشات أسعار البيع، 2 = مشتريات لأسعار الشراء)، ثم بقية الأنواع، ثم بالاسم.
        /// (طلب المستخدم 2026-11-07: الشاشات القديمة كانت تعرض فئات المبيعات فقط بـ Item_Data(1)، والجديدة تعرض الكل مرتبة بالنوع.)
        /// </summary>
        public List<CategoryPick> ListCategories(int preferType) => Run(opp => opp.db.Database.SqlQuery<CategoryPick>($@"
SELECT CAST(ID AS bigint) AS Id, ISNULL(Category_Name, N'') AS Name, CAST(ISNULL(Category_Type_Id, 0) AS int) AS Type_Id
FROM Item_Category WHERE ISNULL(IS_Active, 1) = 1
ORDER BY CASE WHEN ISNULL(Category_Type_Id, 0) = {preferType} THEN 0 ELSE 1 END, ISNULL(Category_Type_Id, 0), Category_Name").ToList());

        /// <summary>
        /// تعديل جماعى لسعر البيع (وخصم البيع اختيارياً) لأصناف محددة فى نوع سعر واحد — زر BtnEditValue القديم.
        /// السعر الجديد = السعر الحالى ± (قيمة أو نسبة) مقرَّباً بخطوة يختارها المستخدم (القديم كان يقرّب لأقرب جنيه دائماً).
        /// discount = null يُبقى الخصم الحالى لكل صنف. النوع المشتق يحتاج allowDerived (سعره يُعاد حسابه عند تغيّر الأم).
        /// يعيد حساب القوائم المشتقة من النوع للأصناف المعدَّلة. يرجع عدد الأصناف.
        /// </summary>
        public int EditSellValues(long typeId, IReadOnlyCollection<long> itemIds, bool isRate, bool isPlus, decimal value, decimal? discount, decimal rounding, bool allowDerived)
        {
            Demand();
            var ids = (itemIds ?? Array.Empty<long>()).Distinct().ToList();
            if (typeId == 0) throw new InvalidOperationException("اختر نوع السعر.");
            if (ids.Count == 0) throw new InvalidOperationException("اختر الأصناف المراد تعديل أسعارها.");
            CheckAdjust(isRate, isPlus, value, discount, rounding);
            if (value == 0 && discount == null) throw new InvalidOperationException("لا يوجد تغيير: أدخل قيمة التعديل أو خصماً جديداً.");
            return RunTx(opp =>
            {
                var all = LoadTypes(opp);
                var t = all.FirstOrDefault(x => x.Id == typeId) ?? throw new InvalidOperationException("نوع السعر غير موجود.");
                if (t.Master_Id != 0 && !allowDerived)
                    throw new InvalidOperationException("نوع السعر مشتق؛ أسعاره تُحسب من القائمة الأم. فعّل «تعديل القائمة المشتقة» للتعديل على أى حال.");
                var cur = CurrentValues(opp, typeId, ids);
                var missing = ids.Where(i => !cur.ContainsKey(i)).ToList();
                if (missing.Count > 0) throw new InvalidOperationException($"{missing.Count} صنف ليس لها سعر فى هذا النوع؛ حدّث العرض وأعد الاختيار.");
                foreach (long item in ids)
                {
                    var (price, disc) = cur[item];
                    decimal np = Adjust(price, isRate, isPlus, value, rounding);
                    decimal nd = discount ?? disc;
                    if (np < 0) throw new InvalidOperationException("بعض الأسعار ستصبح سالبة؛ قلّل النقص.");
                    if (nd > np) throw new InvalidOperationException($"الخصم ({nd:0.##}) سيصبح أكبر من السعر ({np:0.##}) لبعض الأصناف.");
                    Upsert(opp, typeId, item, np);
                    if (discount != null) opp.db.Database.ExecuteSql($"UPDATE Price_Type_Sell_Values SET CommisionValue = {Math.Round(nd, 2)} WHERE Item_ID = {item} AND Price_Type_List_ID = {typeId}");
                }
                RecomputeChildren(opp, all, typeId, ids);
                return ids.Count;
            });
        }

        /// <summary>
        /// نوع سعر = نوع سعر آخر ± (قيمة أو نسبة) — زرّا BtnEditByType (itemIds = null: كل أصناف المصدر) و BtnEditByType_Item (الأصناف المحددة فقط).
        /// الأصناف غير المسعَّرة فى الهدف تُضاف، والمسعَّرة يُستبدل سعرها. قرار حديث: الأصناف غير المحددة لا تُمس
        /// (القديم كان ينسخ سعر المصدر كما هو لكل الأصناف غير المحددة). الهدف المشتق يحتاج allowDerived (كالديسك توب الذى يحمّل كل الأنواع؛
        /// سعره يُستبدل عند تغيّر سعر الأم). discount = null: الأصناف الموجودة تحتفظ بخصمها والجديدة بلا خصم. يرجع عدد الأصناف.
        /// </summary>
        public int CopySellValues(long sourceTypeId, long targetTypeId, IReadOnlyCollection<long> itemIds, bool isRate, bool isPlus, decimal value, decimal? discount, decimal rounding, bool allowDerived = false)
        {
            Demand();
            if (sourceTypeId == 0) throw new InvalidOperationException("اختر نوع السعر المصدر.");
            if (targetTypeId == 0) throw new InvalidOperationException("اختر نوع السعر المطلوب تعديله.");
            // أصناف محددة (التبويب 3): المطلوب قد يكون المصدر نفسه = تعديله ± التعديل؛ المنع لنسخ «كل الأصناف» فقط.
            if (sourceTypeId == targetTypeId && itemIds == null) throw new InvalidOperationException("نوع السعر المطلوب هو نفس المصدر؛ استخدم تبويب «تعديل أسعار الأصناف المحددة».");
            if (itemIds != null && itemIds.Count == 0) throw new InvalidOperationException("اختر الأصناف المراد تعديل أسعارها.");
            CheckAdjust(isRate, isPlus, value, discount, rounding);
            return RunTx(opp =>
            {
                var all = LoadTypes(opp);
                if (all.All(x => x.Id != sourceTypeId)) throw new InvalidOperationException("نوع السعر المصدر غير موجود.");
                var target = all.FirstOrDefault(x => x.Id == targetTypeId) ?? throw new InvalidOperationException("نوع السعر المطلوب غير موجود.");
                if (target.Master_Id != 0 && !allowDerived)
                    throw new InvalidOperationException($"«{target.Name}» قائمة مشتقة؛ أسعارها تُحسب تلقائياً من أمها. فعّل «تعديل القائمة المشتقة» للتعديل على أى حال.");
                if (Descendants(all, targetTypeId).Contains(sourceTypeId))
                    throw new InvalidOperationException("المصدر مشتق من النوع المطلوب؛ النسخ سيدور فى حلقة.");
                var src = itemIds == null
                    ? opp.db.Database.SqlQuery<long>($"SELECT DISTINCT CAST(Item_ID AS bigint) AS Value FROM Price_Type_Sell_Values WHERE Price_Type_List_ID = {sourceTypeId}").ToList()
                    : itemIds.Distinct().ToList();
                if (src.Count == 0) throw new InvalidOperationException("لا توجد أسعار فى نوع السعر المصدر.");
                var srcVals = CurrentValues(opp, sourceTypeId, src);
                var missing = src.Where(i => !srcVals.ContainsKey(i)).ToList();
                if (missing.Count > 0) throw new InvalidOperationException($"{missing.Count} صنف ليس لها سعر فى المصدر؛ حدّث العرض وأعد الاختيار.");
                var tgtVals = CurrentValues(opp, targetTypeId, src);
                foreach (long item in src)
                {
                    decimal np = Adjust(srcVals[item].Price, isRate, isPlus, value, rounding);
                    decimal nd = discount ?? (tgtVals.TryGetValue(item, out var tv) ? tv.Discount : 0);
                    if (np < 0) throw new InvalidOperationException("بعض الأسعار ستصبح سالبة؛ قلّل النقص.");
                    if (nd > np) throw new InvalidOperationException($"الخصم ({nd:0.##}) سيصبح أكبر من السعر ({np:0.##}) لبعض الأصناف.");
                    Upsert(opp, targetTypeId, item, np);
                    if (discount != null) opp.db.Database.ExecuteSql($"UPDATE Price_Type_Sell_Values SET CommisionValue = {Math.Round(nd, 2)} WHERE Item_ID = {item} AND Price_Type_List_ID = {targetTypeId}");
                }
                RecomputeChildren(opp, all, targetTypeId, src);
                return src.Count;
            });
        }

        /// <summary>عدد الأصناف المسعَّرة فى نوع (لمعاينة «نوع = نوع آخر» قبل التنفيذ).</summary>
        public int CountPricedItems(long typeId) => typeId == 0 ? 0 : Run(opp => opp.db.Database.SqlQuery<int>(
            $"SELECT COUNT(DISTINCT Item_ID) AS Value FROM Price_Type_Sell_Values WHERE Price_Type_List_ID = {typeId}").AsEnumerable().First());

        /// <summary>السعر بعد التعديل (نفس حساب الخادم) — تستعمله الواجهة لعمود «السعر الجديد».</summary>
        public static decimal Adjust(decimal price, bool isRate, bool isPlus, decimal value, decimal rounding)
        {
            decimal delta = isRate ? price * value / 100m : value;
            return RoundTo(price + (isPlus ? delta : -delta), rounding);
        }

        private static void CheckAdjust(bool isRate, bool isPlus, decimal value, decimal? discount, decimal rounding)
        {
            if (value < 0) throw new InvalidOperationException("قيمة التعديل لا تكون سالبة؛ اختر «نقص» بدلاً من ذلك.");
            if (isRate && !isPlus && value >= 100) throw new InvalidOperationException("نسبة النقص يجب أن تقل عن 100%.");
            if (discount < 0) throw new InvalidOperationException("الخصم لا يكون سالباً.");
            if (!Roundings.Contains(rounding)) throw new InvalidOperationException("خطوة التقريب غير صحيحة.");
        }

        private class PriceDisc
        {
            public long Item_Id { get; set; }
            public decimal Price { get; set; }
            public decimal Discount { get; set; }
        }

        /// <summary>السعر والخصم الحاليان لأصناف فى نوع (أول صف لكل صنف بالترتيب ID كبقية الخدمة).</summary>
        private static Dictionary<long, (decimal Price, decimal Discount)> CurrentValues(operations opp, long typeId, List<long> itemIds)
        {
            var res = new Dictionary<long, (decimal, decimal)>();
            // أرقام الأصناف أعداد صحيحة فتُضمَّن نصاً فى IN بأمان؛ على دفعات لتجنب حد معاملات SQL
            foreach (var chunk in itemIds.Chunk(1000))
            {
                var rows = opp.db.Database.SqlQueryRaw<PriceDisc>(
                    "SELECT Item_Id, Price, Discount FROM (SELECT CAST(Item_ID AS bigint) AS Item_Id, CAST(ISNULL(PriceUnit, 0) AS decimal(18,4)) AS Price, " +
                    "CAST(ISNULL(CommisionValue, 0) AS decimal(18,4)) AS Discount, ROW_NUMBER() OVER (PARTITION BY Item_ID ORDER BY ID) AS Rn " +
                    "FROM Price_Type_Sell_Values WHERE Price_Type_List_ID = {0} AND Item_ID IN (" + string.Join(",", chunk) + ")) x WHERE Rn = 1", typeId).ToList();
                foreach (var r in rows) res[r.Item_Id] = (r.Price, r.Discount);
            }
            return res;
        }

        // ===================== التعديل الجماعى =====================

        public List<BulkRow> BulkPreview(BulkRequest r) => Run(opp => Preview(opp, Check(r)));

        /// <summary>يطبّق المعاينة نفسها داخل معاملة ويرجع عدد الأصناف المعدَّلة.</summary>
        public int BulkApply(BulkRequest r)
        {
            Demand();
            Check(r);
            return RunTx(opp =>
            {
                var rows = Preview(opp, r).Where(x => x.New_Price != x.Old_Price).ToList();
                if (rows.Any(x => x.New_Price < 0)) throw new InvalidOperationException("بعض الأسعار ستصبح سالبة؛ قلّل النقص.");
                DateTime now = DateTime.Now;
                foreach (var x in rows)
                {
                    decimal ratio = x.Old_Price != 0 ? x.New_Price / x.Old_Price : 0;
                    switch (r.Target)
                    {
                        case BulkTarget.Sale:
                            opp.db.Database.ExecuteSql($@"UPDATE Item SET S_Price = {x.New_Price}, S_price_LargeUnit = {x.New_Price},
    S_price_SmallUnit = CASE WHEN {ratio} <> 0 THEN ROUND(ISNULL(S_price_SmallUnit, 0) * {ratio}, 4) ELSE ROUND({x.New_Price} / {x.Factor}, 4) END,
    Update_ServerDateTime = {now}, Edit_User_Id = {Common.User_Id} WHERE id = {x.Item_Id}");
                            opp.db.Database.ExecuteSql($@"UPDATE Unit_item SET S_price = {x.New_Price}, S_price_LargeUnit = {x.New_Price},
    S_price_SmallUnit = (SELECT S_price_SmallUnit FROM Item WHERE id = {x.Item_Id}), Update_ServerDateTime = {now}, Edit_User_Id = {Common.User_Id} WHERE Item_id = {x.Item_Id}");
                            break;
                        case BulkTarget.Purchase:
                            opp.db.Database.ExecuteSql($@"UPDATE Item SET P_Price = {x.New_Price}, P_price_LargeUnit = {x.New_Price},
    P_price_SmallUnit = CASE WHEN {ratio} <> 0 THEN ROUND(ISNULL(P_price_SmallUnit, 0) * {ratio}, 4) ELSE ROUND({x.New_Price} / {x.Factor}, 4) END,
    Update_ServerDateTime = {now}, Edit_User_Id = {Common.User_Id} WHERE id = {x.Item_Id}");
                            opp.db.Database.ExecuteSql($@"UPDATE Unit_item SET P_price = {x.New_Price}, P_price_LargeUnit = {x.New_Price},
    P_price_SmallUnit = (SELECT P_price_SmallUnit FROM Item WHERE id = {x.Item_Id}), Update_ServerDateTime = {now}, Edit_User_Id = {Common.User_Id} WHERE Item_id = {x.Item_Id}");
                            break;
                        case BulkTarget.PriceType:
                            Upsert(opp, r.Price_Type_Id, x.Item_Id, x.New_Price);
                            break;
                    }
                }
                if (r.Target == BulkTarget.PriceType && rows.Count > 0)
                    RecomputeChildren(opp, LoadTypes(opp), r.Price_Type_Id, rows.Select(x => x.Item_Id).ToList());
                return rows.Count;
            });
        }

        private BulkRequest Check(BulkRequest r)
        {
            if (r.Value <= 0) throw new InvalidOperationException("أدخل قيمة الزيادة أو النقص.");
            if (r.Is_Rate && !r.Is_Plus && r.Value >= 100) throw new InvalidOperationException("نسبة النقص يجب أن تقل عن 100%.");
            if (!Roundings.Contains(r.Rounding)) throw new InvalidOperationException("خطوة التقريب غير صحيحة.");
            if (r.Target == BulkTarget.PriceType && r.Price_Type_Id == 0) throw new InvalidOperationException("اختر قائمة الأسعار.");
            return r;
        }

        private static List<BulkRow> Preview(operations opp, BulkRequest r)
        {
            string t = (r.Text ?? "").Trim();
            string like = "%" + t + "%";
            int target = (int)r.Target;
            var rows = opp.db.Database.SqlQuery<BulkRow>($@"
SELECT i.id AS Item_Id, ISNULL(i.code, N'') AS Code, ISNULL(i.Item_Name, N'') AS Name,
       CAST(CASE {target} WHEN 1 THEN ISNULL(i.S_price_LargeUnit, ISNULL(i.S_Price, 0)) WHEN 2 THEN ISNULL(i.P_price_LargeUnit, ISNULL(i.P_Price, 0))
            ELSE (SELECT TOP 1 PriceUnit FROM Price_Type_Sell_Values v WHERE v.Item_ID = i.id AND v.Price_Type_List_ID = {r.Price_Type_Id} ORDER BY v.ID) END AS decimal(18,4)) AS Old_Price,
       CAST(0 AS decimal(18,4)) AS New_Price,
       CAST(CASE WHEN ISNULL(u.S_unit_Value, 0) <= 0 THEN 1 ELSE u.S_unit_Value END AS decimal(18,4)) AS Factor
FROM Item i LEFT JOIN unit u ON u.id = i.Unit_Id
WHERE ISNULL(i.Active_flage, 1) NOT IN (0, -1)
  AND ({t} = N'' OR i.Item_Name LIKE {like} OR i.code = {t})
  AND ({r.Category_Id} = 0 OR i.Item_Category_ID = {r.Category_Id})
  AND ({r.Group_Id} = 0 OR i.Items_GroupsId = {r.Group_Id})
  AND ({target} <> 3 OR EXISTS (SELECT 1 FROM Price_Type_Sell_Values v WHERE v.Item_ID = i.id AND v.Price_Type_List_ID = {r.Price_Type_Id}))
ORDER BY i.Item_Name").ToList();
            foreach (var x in rows)
            {
                decimal delta = r.Is_Rate ? x.Old_Price * r.Value / 100m : r.Value;
                x.New_Price = RoundTo(x.Old_Price + (r.Is_Plus ? delta : -delta), r.Rounding);
            }
            return rows;
        }

        public static decimal RoundTo(decimal v, decimal step) =>
            step <= 0 ? v : Math.Round(Math.Round(v / step, 0, MidpointRounding.AwayFromZero) * step, 4);

        // ===================== أسعار العملاء والموردين =====================

        public List<PartyPrice> ListCustomerPrices(long customerId, string text) => ListParty(false, customerId, text);
        public List<PartyPrice> ListSupplierPrices(long supplierId, string text) => ListParty(true, supplierId, text);

        private List<PartyPrice> ListParty(bool supplier, long partyId, string text)
        {
            string t = (text ?? "").Trim();
            string like = "%" + t + "%";
            return Run(opp => supplier
                ? opp.db.Database.SqlQuery<PartyPrice>($@"
SELECT TOP (2000) p.Id, CAST(p.Supplier_Id AS bigint) AS Party_Id, ISNULL(s.Name, N'') AS Party_Name, CAST(p.Item_Id AS bigint) AS Item_Id,
       ISNULL(i.code, N'') AS Item_Code, ISNULL(i.Item_Name, N'') AS Item_Name, ISNULL(u.B_Unit, N'') AS Unit,
       CAST(ISNULL(i.P_price_LargeUnit, ISNULL(i.P_Price, 0)) AS decimal(18,4)) AS Item_Price, CAST(ISNULL(p.PriceUnit, 0) AS decimal(18,4)) AS Price,
       CAST(ISNULL(p.Price_PurchaseType, 0) AS bigint) AS Purchase_Type_Id, ISNULL(pt.Price_Name, N'') AS Purchase_Type_Name
FROM Supplier_Price p JOIN Item i ON i.id = p.Item_Id LEFT JOIN Supplier s ON s.Id = p.Supplier_Id LEFT JOIN unit u ON u.id = i.Unit_Id
LEFT JOIN Price_Purhase_Type_List pt ON pt.Id = p.Price_PurchaseType
WHERE ({partyId} = 0 OR p.Supplier_Id = {partyId}) AND ({t} = N'' OR i.Item_Name LIKE {like} OR i.code = {t} OR s.Name LIKE {like})
ORDER BY s.Name, i.Item_Name").ToList()
                : opp.db.Database.SqlQuery<PartyPrice>($@"
SELECT TOP (2000) p.Id, CAST(p.Customer_Id AS bigint) AS Party_Id, ISNULL(c.Name, N'') AS Party_Name, CAST(p.Item_Id AS bigint) AS Item_Id,
       ISNULL(i.code, N'') AS Item_Code, ISNULL(i.Item_Name, N'') AS Item_Name, ISNULL(u.B_Unit, N'') AS Unit,
       CAST(ISNULL(i.S_price_LargeUnit, ISNULL(i.S_Price, 0)) AS decimal(18,4)) AS Item_Price, CAST(ISNULL(p.PriceUnit, 0) AS decimal(18,4)) AS Price,
       CAST(0 AS bigint) AS Purchase_Type_Id, CAST(N'' AS nvarchar(10)) AS Purchase_Type_Name
FROM Customer_Price p JOIN Item i ON i.id = p.Item_Id LEFT JOIN Customer c ON c.Id = p.Customer_Id LEFT JOIN unit u ON u.id = i.Unit_Id
WHERE ({partyId} = 0 OR p.Customer_Id = {partyId}) AND ({t} = N'' OR i.Item_Name LIKE {like} OR i.code = {t} OR c.Name LIKE {like})
ORDER BY c.Name, i.Item_Name").ToList());
        }

        /// <summary>سعر خاص لعميل على صنف (صف واحد لكل عميل × صنف لكل المخازن).</summary>
        public long SaveCustomerPrice(long customerId, long itemId, decimal price) => SaveParty(false, customerId, itemId, price, 0);

        /// <summary>سعر مورد على صنف، اختيارياً بنوع سعر شراء.</summary>
        public long SaveSupplierPrice(long supplierId, long itemId, decimal price, long purchaseTypeId) => SaveParty(true, supplierId, itemId, price, purchaseTypeId);

        private long SaveParty(bool supplier, long partyId, long itemId, decimal price, long purchaseTypeId)
        {
            Demand();
            if (price < 0) throw new InvalidOperationException("السعر لا يكون سالباً.");
            if (partyId == 0) throw new InvalidOperationException(supplier ? "اختر المورد." : "اختر العميل.");
            if (itemId == 0) throw new InvalidOperationException("اختر الصنف.");
            return RunTx(opp =>
            {
                string partyTable = supplier ? "Supplier" : "Customer";
                if (opp.db.Database.SqlQuery<int>(FormattableStringFactory.Create($"SELECT COUNT(*) AS Value FROM [{partyTable}] WHERE Id = {{0}}", partyId)).AsEnumerable().First() == 0)
                    throw new InvalidOperationException(supplier ? "المورد غير موجود." : "العميل غير موجود.");
                if (opp.db.Database.SqlQuery<int>($"SELECT COUNT(*) AS Value FROM Item WHERE id = {itemId}").AsEnumerable().First() == 0)
                    throw new InvalidOperationException("الصنف غير موجود.");
                if (supplier && purchaseTypeId != 0 && opp.db.Database.SqlQuery<int>($"SELECT COUNT(*) AS Value FROM Price_Purhase_Type_List WHERE Id = {purchaseTypeId}").AsEnumerable().First() == 0)
                    throw new InvalidOperationException("نوع سعر الشراء غير موجود.");
                DateTime now = DateTime.Now;
                decimal p = Math.Round(price, 4);
                int ptype = checked((int)purchaseTypeId);
                long? existing = supplier
                    ? opp.db.Database.SqlQuery<long>($"SELECT TOP 1 Id AS Value FROM Supplier_Price WHERE Supplier_Id = {partyId} AND Item_Id = {itemId} AND ISNULL(Price_PurchaseType, 0) = {ptype} ORDER BY Id").AsEnumerable().Cast<long?>().FirstOrDefault()
                    : opp.db.Database.SqlQuery<long>($"SELECT TOP 1 Id AS Value FROM Customer_Price WHERE Customer_Id = {partyId} AND Item_Id = {itemId} ORDER BY Id").AsEnumerable().Cast<long?>().FirstOrDefault();
                if (existing is long ex)
                {
                    if (supplier) opp.db.Database.ExecuteSql($"UPDATE Supplier_Price SET PriceUnit = {p}, Update_ServerDateTime = {now}, EditOperationDate = {now}, Edit_User_Id = {Common.User_Id} WHERE Id = {ex}");
                    else opp.db.Database.ExecuteSql($"UPDATE Customer_Price SET PriceUnit = {p}, Update_ServerDateTime = {now}, EditOperationDate = {now}, Edit_User_Id = {Common.User_Id} WHERE Id = {ex}");
                    return ex;
                }
                string table = supplier ? "Supplier_Price" : "Customer_Price";
                string cols = supplier ? "Supplier_Id, Item_Id, PriceUnit, Store_id, Price_PurchaseType, OperationDate, User_Id, Branch_Id, Insert_ServerDateTime"
                                       : "Customer_Id, Item_Id, PriceUnit, Store_id, OperationDate, User_Id, Branch_Id, Insert_ServerDateTime";
                string vals = supplier ? "{1}, {2}, {3}, 0, {4}, {5}, {6}, {7}, {8}" : "{1}, {2}, {3}, 0, {5}, {6}, {7}, {8}";
                return Insert(opp, table, "Id", cols, vals, partyId, itemId, p, ptype, now.Date, Common.User_Id, Common.Branch_Id, now);
            });
        }

        public void DeleteCustomerPrice(long id) { Demand(); Run(opp => opp.db.Database.ExecuteSql($"DELETE FROM Customer_Price WHERE Id = {id}")); }
        public void DeleteSupplierPrice(long id) { Demand(); Run(opp => opp.db.Database.ExecuteSql($"DELETE FROM Supplier_Price WHERE Id = {id}")); }

        // ===================== أسعار مشتريات الأصناف (بدل Supplier_Price / Suppliers_PriceEditFRM) =====================

        /// <summary>حذف عدة أسعار موردين معاً (زر BtnDelet القديم يحذف كل الصفوف المحددة) فى معاملة واحدة. يرجع عدد المحذوف.</summary>
        public int DeleteSupplierPrices(IReadOnlyCollection<long> ids)
        {
            Demand();
            var list = (ids ?? Array.Empty<long>()).Distinct().ToList();
            if (list.Count == 0) throw new InvalidOperationException("اختر الأسعار المراد حذفها.");
            return RunTx(opp => list.Sum(id => opp.db.Database.ExecuteSql($"DELETE FROM Supplier_Price WHERE Id = {id}")));
        }

        private class ItemRowId
        {
            public long Item_Id { get; set; }
            public long Row_Id { get; set; }
        }

        public class SupplierPriceRow
        {
            public long Id { get; set; }
            public long Supplier_Id { get; set; }
            public string Supplier_Name { get; set; }
            public long Item_Id { get; set; }
            public string Item_Code { get; set; }
            public string Item_Name { get; set; }
            public long Category_Id { get; set; }
            public string Category { get; set; }
            public string Unit { get; set; }
            /// <summary>سعر الشراء فى بطاقة الصنف (الوحدة الكبرى) للمقارنة.</summary>
            public decimal Card_Price { get; set; }
            public long Type_Id { get; set; }
            public string Type_Name { get; set; }
            public decimal Price { get; set; }
        }

        /// <summary>
        /// أسعار الموردين بفلاتر الشاشة القديمة: الفئة، الصنف، المورد، نوع سعر الشراء (0 = الكل فى الأربعة؛ typeId = -1 = بدون نوع)
        /// مع بحث نصى اختيارى بالصنف أو المورد. أول {top} صف.
        /// </summary>
        public List<SupplierPriceRow> ListSupplierPriceRows(long categoryId, long itemId, long supplierId, long typeId, string text, int top = 1000)
        {
            string t = (text ?? "").Trim();
            string like = "%" + t + "%";
            int n = Math.Clamp(top, 1, 5000);
            return Run(opp => opp.db.Database.SqlQuery<SupplierPriceRow>($@"
SELECT TOP ({n}) CAST(p.Id AS bigint) AS Id, CAST(p.Supplier_Id AS bigint) AS Supplier_Id, ISNULL(s.Name, N'') AS Supplier_Name,
       CAST(p.Item_Id AS bigint) AS Item_Id, ISNULL(i.code, N'') AS Item_Code, ISNULL(i.Item_Name, N'') AS Item_Name,
       CAST(ISNULL(i.Item_Category_ID, 0) AS bigint) AS Category_Id, ISNULL(c.Category_Name, N'') AS Category, ISNULL(u.B_Unit, N'') AS Unit,
       CAST(ISNULL(i.P_price_LargeUnit, ISNULL(i.P_Price, 0)) AS decimal(18,4)) AS Card_Price,
       CAST(ISNULL(p.Price_PurchaseType, 0) AS bigint) AS Type_Id, ISNULL(LTRIM(RTRIM(pt.Price_Name)), N'') AS Type_Name,
       CAST(ISNULL(p.PriceUnit, 0) AS decimal(18,4)) AS Price
FROM Supplier_Price p
JOIN Item i ON i.id = p.Item_Id
LEFT JOIN Supplier s ON s.Id = p.Supplier_Id
LEFT JOIN Item_Category c ON c.ID = i.Item_Category_ID
LEFT JOIN unit u ON u.id = i.Unit_Id
LEFT JOIN Price_Purhase_Type_List pt ON pt.Id = p.Price_PurchaseType
WHERE ({categoryId} = 0 OR i.Item_Category_ID = {categoryId})
  AND ({itemId} = 0 OR p.Item_Id = {itemId})
  AND ({supplierId} = 0 OR p.Supplier_Id = {supplierId})
  AND ({typeId} = 0 OR ({typeId} = -1 AND ISNULL(p.Price_PurchaseType, 0) = 0) OR p.Price_PurchaseType = {typeId})
  AND ({t} = N'' OR i.Item_Name LIKE {like} OR i.code = {t} OR s.Name LIKE {like})
ORDER BY c.Category_Name, i.Item_Name, s.Name, pt.Price_Name").ToList());
        }

        /// <summary>السعر المسجَّل لنفس (المورد × الصنف × نوع السعر) إن وُجد — لتنبيه «سيُستبدل» عند الإضافة.</summary>
        public SupplierPriceRow FindSupplierPrice(long supplierId, long itemId, long typeId) =>
            supplierId == 0 || itemId == 0 ? null : Run(opp => opp.db.Database.SqlQuery<SupplierPriceRow>($@"
SELECT TOP 1 CAST(Id AS bigint) AS Id, CAST(Supplier_Id AS bigint) AS Supplier_Id, CAST(N'' AS nvarchar(1)) AS Supplier_Name, CAST(Item_Id AS bigint) AS Item_Id,
       CAST(N'' AS nvarchar(1)) AS Item_Code, CAST(N'' AS nvarchar(1)) AS Item_Name, CAST(0 AS bigint) AS Category_Id, CAST(N'' AS nvarchar(1)) AS Category,
       CAST(N'' AS nvarchar(1)) AS Unit, CAST(0 AS decimal(18,4)) AS Card_Price, CAST(ISNULL(Price_PurchaseType, 0) AS bigint) AS Type_Id,
       CAST(N'' AS nvarchar(1)) AS Type_Name, CAST(ISNULL(PriceUnit, 0) AS decimal(18,4)) AS Price
FROM Supplier_Price WHERE Supplier_Id = {supplierId} AND Item_Id = {itemId} AND ISNULL(Price_PurchaseType, 0) = {typeId} ORDER BY Id").AsEnumerable().FirstOrDefault());

        /// <summary>سعر الشراء فى بطاقة الصنف (الوحدة الكبرى) للمقارنة فى نافذة السعر.</summary>
        public decimal GetItemPurchasePrice(long itemId) => Run(opp => opp.db.Database.SqlQuery<decimal>($@"
SELECT CAST(ISNULL(P_price_LargeUnit, ISNULL(P_Price, 0)) AS decimal(18,4)) AS Value FROM Item WHERE id = {itemId}").AsEnumerable().FirstOrDefault());

        /// <summary>
        /// سعر واحد لمورد على كل أصناف فئة نشطة (اختيار «كل الأصناف» فى Suppliers_PriceEditFRM القديمة).
        /// overwrite = true يستبدل الأسعار الموجودة لنفس المورد والنوع؛ false يضيف للأصناف التى ليس لها سعر فقط. يرجع (أُضيف، عُدِّل).
        /// </summary>
        public (int Added, int Updated) SaveSupplierPriceForCategory(long supplierId, long categoryId, decimal price, long typeId, bool overwrite)
        {
            Demand();
            if (supplierId == 0) throw new InvalidOperationException("اختر المورد.");
            if (categoryId == 0) throw new InvalidOperationException("اختر فئة الصنف.");
            if (price <= 0) throw new InvalidOperationException("أدخل السعر.");
            int ptype = checked((int)typeId);
            decimal p = Math.Round(price, 4);
            // معاملة واحدة: كل أصناف الفئة تُحفظ معاً أو لا يُحفظ شىء
            return RunTx(opp =>
            {
                if (opp.db.Database.SqlQuery<int>($"SELECT COUNT(*) AS Value FROM Supplier WHERE Id = {supplierId}").AsEnumerable().First() == 0)
                    throw new InvalidOperationException("المورد غير موجود.");
                if (typeId != 0 && opp.db.Database.SqlQuery<int>($"SELECT COUNT(*) AS Value FROM Price_Purhase_Type_List WHERE Id = {typeId}").AsEnumerable().First() == 0)
                    throw new InvalidOperationException("نوع سعر الشراء غير موجود.");
                var items = opp.db.Database.SqlQuery<long>($@"SELECT CAST(id AS bigint) AS Value FROM Item
WHERE Item_Category_ID = {categoryId} AND ISNULL(Active_flage, 1) NOT IN (0, -1)").ToList();
                if (items.Count == 0) throw new InvalidOperationException("لا توجد أصناف نشطة فى هذه الفئة.");
                // أول صف لكل صنف (بالترتيب Id) لنفس المورد والنوع، كما يعدّله SaveParty
                var existing = opp.db.Database.SqlQuery<ItemRowId>($@"SELECT Item_Id, CAST(Id AS bigint) AS Row_Id FROM (
    SELECT CAST(p.Item_Id AS bigint) AS Item_Id, p.Id, ROW_NUMBER() OVER (PARTITION BY p.Item_Id ORDER BY p.Id) AS Rn
    FROM Supplier_Price p JOIN Item i ON i.id = p.Item_Id
    WHERE p.Supplier_Id = {supplierId} AND ISNULL(p.Price_PurchaseType, 0) = {ptype} AND i.Item_Category_ID = {categoryId}) x WHERE Rn = 1")
                    .ToList().ToDictionary(x => x.Item_Id, x => x.Row_Id);
                DateTime now = DateTime.Now;
                int added = 0, updated = 0;
                foreach (long item in items)
                {
                    if (existing.TryGetValue(item, out long rowId))
                    {
                        if (!overwrite) continue;
                        opp.db.Database.ExecuteSql($"UPDATE Supplier_Price SET PriceUnit = {p}, Update_ServerDateTime = {now}, EditOperationDate = {now}, Edit_User_Id = {Common.User_Id} WHERE Id = {rowId}");
                        updated++;
                    }
                    else
                    {
                        Insert(opp, "Supplier_Price", "Id", "Supplier_Id, Item_Id, PriceUnit, Store_id, Price_PurchaseType, OperationDate, User_Id, Branch_Id, Insert_ServerDateTime",
                               "{1}, {2}, {3}, 0, {4}, {5}, {6}, {7}, {8}", supplierId, item, p, ptype, now.Date, Common.User_Id, Common.Branch_Id, now);
                        added++;
                    }
                }
                return (added, updated);
            });
        }

        /// <summary>
        /// حفظ سعر مورد من نافذة الشاشة: id = 0 إضافة (أو استبدال سعر نفس المورد × الصنف × النوع كـ SaveSupplierPrice)؛
        /// id ≠ 0 تعديل الصف نفسه بما فيه المورد/الصنف/النوع (كـ Edit_Supplier_Price القديم)، ويُرفض لو صار مطابقاً لصف آخر.
        /// السعر أكبر من صفر (القديمة كانت تقبل الصفر).
        /// </summary>
        public long SaveSupplierPriceRow(long id, long supplierId, long itemId, decimal price, long typeId)
        {
            if (price <= 0) throw new InvalidOperationException("أدخل السعر.");
            if (id == 0) return SaveSupplierPrice(supplierId, itemId, price, typeId);
            Demand();
            if (supplierId == 0) throw new InvalidOperationException("اختر المورد.");
            if (itemId == 0) throw new InvalidOperationException("اختر الصنف.");
            return RunTx(opp =>
            {
                if (opp.db.Database.SqlQuery<int>($"SELECT COUNT(*) AS Value FROM Supplier_Price WHERE Id = {id}").AsEnumerable().First() == 0)
                    throw new InvalidOperationException("السعر غير موجود؛ ربما حُذف. حدّث العرض.");
                if (opp.db.Database.SqlQuery<int>($"SELECT COUNT(*) AS Value FROM Supplier WHERE Id = {supplierId}").AsEnumerable().First() == 0)
                    throw new InvalidOperationException("المورد غير موجود.");
                if (opp.db.Database.SqlQuery<int>($"SELECT COUNT(*) AS Value FROM Item WHERE id = {itemId}").AsEnumerable().First() == 0)
                    throw new InvalidOperationException("الصنف غير موجود.");
                if (typeId != 0 && opp.db.Database.SqlQuery<int>($"SELECT COUNT(*) AS Value FROM Price_Purhase_Type_List WHERE Id = {typeId}").AsEnumerable().First() == 0)
                    throw new InvalidOperationException("نوع سعر الشراء غير موجود.");
                int ptype = checked((int)typeId);
                if (opp.db.Database.SqlQuery<int>($"SELECT COUNT(*) AS Value FROM Supplier_Price WHERE Id <> {id} AND Supplier_Id = {supplierId} AND Item_Id = {itemId} AND ISNULL(Price_PurchaseType, 0) = {ptype}").AsEnumerable().First() > 0)
                    throw new InvalidOperationException("يوجد سعر آخر لنفس المورد والصنف ونوع السعر؛ عدّله هو أو احذفه أولاً.");
                DateTime now = DateTime.Now;
                decimal p = Math.Round(price, 4);
                opp.db.Database.ExecuteSql($@"UPDATE Supplier_Price SET Supplier_Id = {supplierId}, Item_Id = {itemId}, PriceUnit = {p}, Price_PurchaseType = {ptype},
    Update_ServerDateTime = {now}, EditOperationDate = {now}, Edit_User_Id = {Common.User_Id} WHERE Id = {id}");
                return id;
            });
        }

        /// <summary>
        /// سعر العميل الخاص للصنف (للفاتورة): المخزن المحدد أولاً ثم «كل المخازن» (Store_id = 0)؛ null = لا سعر خاص.
        /// الأولوية فى الفاتورة: سعر العميل ← قائمة أسعاره ← سعر الصنف.
        /// </summary>
        public decimal? GetCustomerPrice(long customerId, long itemId, long storeId = 0)
        {
            if (customerId == 0 || itemId == 0) return null;
            return Run(opp => opp.db.Database.SqlQuery<decimal?>($@"SELECT TOP 1 CAST(PriceUnit AS decimal(18,4)) AS Value FROM Customer_Price
WHERE Customer_Id = {customerId} AND Item_Id = {itemId} AND (ISNULL(Store_id, 0) = 0 OR Store_id = {storeId})
ORDER BY CASE WHEN Store_id = {storeId} AND {storeId} <> 0 THEN 0 ELSE 1 END, Id").AsEnumerable().FirstOrDefault());
        }

        // ---------- بحث للاختيار ----------

        /// <summary>بحث العملاء أو الموردين بالاسم أو الكود (أول 30).</summary>
        public List<Option> SearchParties(bool supplier, string text)
        {
            string t = (text ?? "").Trim();
            string like = "%" + t + "%";
            return Run(opp => supplier
                ? opp.db.Database.SqlQuery<Option>($"SELECT TOP (30) Id, ISNULL(Name, N'') AS Name FROM Supplier WHERE ({t} = N'' OR Name LIKE {like} OR CAST(Code AS nvarchar(50)) = {t}) ORDER BY Name").ToList()
                : opp.db.Database.SqlQuery<Option>($"SELECT TOP (30) Id, ISNULL(Name, N'') AS Name FROM Customer WHERE ({t} = N'' OR Name LIKE {like} OR CAST(Code AS nvarchar(50)) = {t}) ORDER BY Name").ToList());
        }

        /// <summary>بحث الأصناف النشطة بالاسم أو الكود أو الباركود (أول 30)؛ الاسم يُعرض «الكود - الاسم».</summary>
        public List<Option> SearchItems(string text) => SearchItems(text, 0);

        /// <summary>كـ SearchItems مع قصر النتيجة على فئة (categoryId = 0 كل الفئات).</summary>
        public List<Option> SearchItems(string text, long categoryId)
        {
            string t = (text ?? "").Trim();
            string like = "%" + t + "%";
            return Run(opp => opp.db.Database.SqlQuery<Option>($@"SELECT TOP (30) id AS Id, ISNULL(code + N' - ', N'') + ISNULL(Item_Name, N'') AS Name FROM Item
WHERE ISNULL(Active_flage, 1) NOT IN (0, -1) AND ({categoryId} = 0 OR Item_Category_ID = {categoryId})
  AND ({t} = N'' OR Item_Name LIKE {like} OR code = {t} OR BarCode2 = {t} OR BarCode3 = {t}) ORDER BY Item_Name").ToList());
        }

        // ---------- أنواع سعر الشراء (قائمة بسيطة ضمن أسعار الموردين) ----------

        public List<Option> ListPurchaseTypes() => Run(opp => opp.db.Database.SqlQuery<Option>($@"SELECT Id, ISNULL(LTRIM(RTRIM(Price_Name)), N'') AS Name FROM Price_Purhase_Type_List
WHERE ISNULL(IS_Active, 1) = 1 ORDER BY Price_Name").ToList());

        public long AddPurchaseType(string name)
        {
            Demand();
            string n = (name ?? "").Trim();
            if (n.Length == 0) throw new InvalidOperationException("اسم نوع سعر الشراء مطلوب.");
            return RunTx(opp =>
            {
                if (opp.db.Database.SqlQuery<int>($"SELECT COUNT(*) AS Value FROM Price_Purhase_Type_List WHERE LTRIM(RTRIM(Price_Name)) = {n}").AsEnumerable().First() > 0)
                    throw new InvalidOperationException("الاسم موجود بالفعل.");
                DateTime now = DateTime.Now;
                // MAX + 1: Supplier_Price.Price_PurchaseType من نوع int
                long id = opp.db.Database.SqlQuery<long>($"SELECT CAST(ISNULL(MAX(Id), 0) + 1 AS bigint) AS Value FROM Price_Purhase_Type_List WITH (UPDLOCK, HOLDLOCK)").AsEnumerable().First();
                if (IsIdentity(opp, "Price_Purhase_Type_List", "Id"))
                    return opp.db.Database.SqlQuery<long>($@"INSERT INTO Price_Purhase_Type_List (Price_Name, IS_Active, Price_Type_List_Master_Id, OperationDate, User_Id, Branch_Id, Insert_ServerDateTime)
OUTPUT CAST(INSERTED.Id AS bigint) AS Value VALUES ({n}, 1, -1, {now.Date}, {Common.User_Id}, {Common.Branch_Id}, {now})").AsEnumerable().First();
                opp.db.Database.ExecuteSql($@"INSERT INTO Price_Purhase_Type_List (Id, Price_Name, IS_Active, Price_Type_List_Master_Id, OperationDate, User_Id, Branch_Id, Insert_ServerDateTime)
VALUES ({id}, {n}, 1, -1, {now.Date}, {Common.User_Id}, {Common.Branch_Id}, {now})");
                return id;
            });
        }

        // ===================== أنواع سعر المشتريات (بدل Price_Type_List_PurchasFRM / Price_Type_Edit_PurchasFRM) =====================

        public class PurchaseType
        {
            public long Id { get; set; }
            public string Name { get; set; }
            public bool Is_Active { get; set; }
            /// <summary>عدد أسعار الموردين (Supplier_Price) المسجَّلة بهذا النوع.</summary>
            public int Prices { get; set; }
            public int Suppliers { get; set; }
        }

        /// <summary>كل أنواع سعر الشراء (النشطة والموقوفة) مع عدد الأسعار والموردين المرتبطين بكل نوع.</summary>
        public List<PurchaseType> ListPurchaseTypesAll() => Run(opp => opp.db.Database.SqlQuery<PurchaseType>($@"
SELECT CAST(t.Id AS bigint) AS Id, ISNULL(LTRIM(RTRIM(t.Price_Name)), N'') AS Name,
       CAST(CASE WHEN ISNULL(t.IS_Active, 1) = 1 THEN 1 ELSE 0 END AS bit) AS Is_Active,
       (SELECT COUNT(*) FROM Supplier_Price p WHERE p.Price_PurchaseType = t.Id) AS Prices,
       (SELECT COUNT(DISTINCT p.Supplier_Id) FROM Supplier_Price p WHERE p.Price_PurchaseType = t.Id) AS Suppliers
FROM Price_Purhase_Type_List t ORDER BY t.Price_Name").ToList());

        /// <summary>إضافة (id = 0) أو إعادة تسمية نوع سعر شراء. الاسم مطلوب، حتى 100 حرف، وغير مكرر.</summary>
        public long SavePurchaseType(long id, string name)
        {
            string n = (name ?? "").Trim();
            if (n.Length > 100) throw new InvalidOperationException("اسم نوع سعر الشراء أطول من 100 حرف.");
            if (id == 0) return AddPurchaseType(n);
            Demand();
            if (n.Length == 0) throw new InvalidOperationException("اسم نوع سعر الشراء مطلوب.");
            return RunTx(opp =>
            {
                if (opp.db.Database.SqlQuery<int>($"SELECT COUNT(*) AS Value FROM Price_Purhase_Type_List WHERE Id = {id}").AsEnumerable().First() == 0)
                    throw new InvalidOperationException("نوع سعر الشراء غير موجود.");
                if (opp.db.Database.SqlQuery<int>($"SELECT COUNT(*) AS Value FROM Price_Purhase_Type_List WHERE Id <> {id} AND LTRIM(RTRIM(Price_Name)) = {n}").AsEnumerable().First() > 0)
                    throw new InvalidOperationException("الاسم موجود بالفعل.");
                DateTime now = DateTime.Now;
                opp.db.Database.ExecuteSql($@"UPDATE Price_Purhase_Type_List SET Price_Name = {n}, Update_ServerDateTime = {now}, EditOperationDate = {now}, Edit_User_Id = {Common.User_Id} WHERE Id = {id}");
                return id;
            });
        }

        /// <summary>إيقاف/تفعيل نوع سعر شراء (زر BtnEditStatus القديم، هنا لكل صف على حدة). الموقوف لا يظهر فى اختيارات الأسعار الجديدة وأسعاره القديمة تبقى.</summary>
        public void SetPurchaseTypeActive(long id, bool active)
        {
            Demand();
            RunTx(opp =>
            {
                DateTime now = DateTime.Now;
                int n = opp.db.Database.ExecuteSql($@"UPDATE Price_Purhase_Type_List SET IS_Active = {(active ? 1 : 0)}, Update_ServerDateTime = {now}, EditOperationDate = {now}, Edit_User_Id = {Common.User_Id} WHERE Id = {id}");
                if (n == 0) throw new InvalidOperationException("نوع سعر الشراء غير موجود.");
                return n;
            });
        }

        /// <summary>حذف نوع سعر شراء غير مستعمل فقط؛ المستعمل فى أسعار الموردين يُوقف بدلاً من الحذف.
        /// (زر الحذف القديم كان يوقف نوع سعر <b>بيع</b> بنفس الرقم بالخطأ — Delete_Price_Name.)</summary>
        public void DeletePurchaseType(long id)
        {
            Demand();
            RunTx(opp =>
            {
                int used = opp.db.Database.SqlQuery<int>($"SELECT COUNT(*) AS Value FROM Supplier_Price WHERE Price_PurchaseType = {id}").AsEnumerable().First();
                if (used > 0) throw new InvalidOperationException($"مستعمل فى {used} سعر مورد؛ أوقفه بدلاً من الحذف.");
                int n = opp.db.Database.ExecuteSql($"DELETE FROM Price_Purhase_Type_List WHERE Id = {id}");
                if (n == 0) throw new InvalidOperationException("نوع سعر الشراء غير موجود.");
                return n;
            });
        }

        // ===================== الحساب =====================

        private static List<PriceType> LoadTypes(operations opp) => opp.db.Database.SqlQuery<PriceType>($@"
SELECT ID AS Id, ISNULL(LTRIM(RTRIM(Price_Name)), N'') AS Name, CAST(CASE WHEN ISNULL(IS_Active, 1) = 1 THEN 1 ELSE 0 END AS bit) AS Is_Active,
       CAST(CASE WHEN ISNULL(Price_Type_List_Master_Id, -1) <= 0 OR Price_Type_List_Master_Id = ID THEN 0 ELSE Price_Type_List_Master_Id END AS bigint) AS Master_Id,
       CAST(N'' AS nvarchar(10)) AS Master_Name,
       CAST(CASE WHEN LTRIM(RTRIM(ISNULL(Plus_Minus, N'+'))) = N'-' THEN 0 ELSE 1 END AS bit) AS Is_Plus,
       CAST(ISNULL(Added_Value, 0) AS decimal(18,4)) AS Added_Value, CAST(CASE WHEN IS_Rate = 1 THEN 1 ELSE 0 END AS bit) AS Is_Rate,
       (SELECT COUNT(*) FROM Price_Type_Sell_Values v WHERE v.Price_Type_List_ID = t.ID) AS Priced_Items,
       (SELECT COUNT(*) FROM Customer c WHERE c.Price_Type_List_ID = t.ID) AS Customers,
       (SELECT COUNT(*) FROM Price_Type_List k WHERE k.Price_Type_List_Master_Id = t.ID AND k.ID <> t.ID) AS Children
FROM Price_Type_List t").ToList();

        private static HashSet<long> Descendants(List<PriceType> all, long id)
        {
            var res = new HashSet<long>();
            var queue = new Queue<long>(new[] { id });
            while (queue.Count > 0)
            {
                long parent = queue.Dequeue();
                foreach (var c in all.Where(x => x.Master_Id == parent && x.Id != parent))
                    if (res.Add(c.Id)) queue.Enqueue(c.Id);
            }
            return res;
        }

        /// <summary>السعر المشتق = سعر الأم ± (قيمة أو نسبة)، مقرَّباً لأقرب 0.01 (قرار المستخدم)، ولا يقل عن صفر.</summary>
        public static decimal Derive(decimal masterPrice, PriceType t)
        {
            decimal delta = t.Is_Rate ? masterPrice * t.Added_Value / 100m : t.Added_Value;
            return Math.Max(0, Math.Round(masterPrice + (t.Is_Plus ? delta : -delta), 2, MidpointRounding.AwayFromZero));
        }

        /// <summary>إعادة حساب المشتقات من القائمة (على كل المستويات) لأصناف محددة؛ القوائم الموقوفة تُحسب أيضاً حتى لا تتقادم.</summary>
        private static void RecomputeChildren(operations opp, List<PriceType> all, long typeId, IReadOnlyCollection<long> itemIds)
        {
            foreach (var child in all.Where(x => x.Master_Id == typeId && x.Id != typeId))
            {
                foreach (long item in itemIds)
                {
                    decimal? master = opp.db.Database.SqlQuery<decimal?>($"SELECT TOP 1 CAST(PriceUnit AS decimal(18,4)) AS Value FROM Price_Type_Sell_Values WHERE Item_ID = {item} AND Price_Type_List_ID = {typeId} ORDER BY ID").AsEnumerable().FirstOrDefault();
                    if (master == null) opp.db.Database.ExecuteSql($"DELETE FROM Price_Type_Sell_Values WHERE Item_ID = {item} AND Price_Type_List_ID = {child.Id}");
                    else Upsert(opp, child.Id, item, Derive(master.Value, child));
                }
                RecomputeChildren(opp, all, child.Id, itemIds);
            }
        }

        /// <summary>يحسب قائمة مشتقة لكل أصناف أمها (بعملية واحدة) ثم ما يتفرع منها. يرجع عدد الأصناف.</summary>
        private static int RecomputeTypeAllItems(operations opp, long typeId)
        {
            var all = LoadTypes(opp);
            var t = all.FirstOrDefault(x => x.Id == typeId) ?? throw new InvalidOperationException("نوع السعر غير موجود.");
            if (t.Master_Id == 0) return 0;
            int sign = t.Is_Plus ? 1 : -1, rate = t.Is_Rate ? 1 : 0;
            DateTime now = DateTime.Now;
            // أصناف لم يعد لها سعر فى الأم تُحذف من المشتقة
            opp.db.Database.ExecuteSql($@"DELETE d FROM Price_Type_Sell_Values d
WHERE d.Price_Type_List_ID = {typeId} AND NOT EXISTS (SELECT 1 FROM Price_Type_Sell_Values m WHERE m.Item_ID = d.Item_ID AND m.Price_Type_List_ID = {t.Master_Id})");
            string calc = "CASE WHEN v.P < 0 THEN 0 ELSE ROUND(v.P, 2) END";
            opp.db.Database.ExecuteSql(FormattableStringFactory.Create($@"
UPDATE d SET PriceUnit = {calc}, Update_ServerDateTime = {{0}}, Edit_User_Id = {{1}}
FROM Price_Type_Sell_Values d
CROSS APPLY (SELECT TOP 1 CAST(m.PriceUnit AS decimal(18,4)) + {{2}} * CASE WHEN {{3}} = 1 THEN CAST(m.PriceUnit AS decimal(18,4)) * {{4}} / 100 ELSE {{4}} END AS P
             FROM Price_Type_Sell_Values m WHERE m.Item_ID = d.Item_ID AND m.Price_Type_List_ID = {{5}} ORDER BY m.ID) v
WHERE d.Price_Type_List_ID = {{6}}", now, Common.User_Id, sign, rate, t.Added_Value, t.Master_Id, typeId));
            var missing = opp.db.Database.SqlQuery<long>($@"SELECT DISTINCT m.Item_ID AS Value FROM Price_Type_Sell_Values m
WHERE m.Price_Type_List_ID = {t.Master_Id} AND NOT EXISTS (SELECT 1 FROM Price_Type_Sell_Values d WHERE d.Item_ID = m.Item_ID AND d.Price_Type_List_ID = {typeId})").ToList();
            foreach (long item in missing)
            {
                decimal master = opp.db.Database.SqlQuery<decimal>($"SELECT TOP 1 CAST(PriceUnit AS decimal(18,4)) AS Value FROM Price_Type_Sell_Values WHERE Item_ID = {item} AND Price_Type_List_ID = {t.Master_Id} ORDER BY ID").AsEnumerable().First();
                Upsert(opp, typeId, item, Derive(master, t));
            }
            int count = opp.db.Database.SqlQuery<int>($"SELECT COUNT(*) AS Value FROM Price_Type_Sell_Values WHERE Price_Type_List_ID = {typeId}").AsEnumerable().First();
            foreach (var child in all.Where(x => x.Master_Id == typeId && x.Id != typeId)) RecomputeTypeAllItems(opp, child.Id);
            return count;
        }

        private static void Upsert(operations opp, long typeId, long itemId, decimal price)
        {
            DateTime now = DateTime.Now;
            int n = opp.db.Database.ExecuteSql($@"UPDATE Price_Type_Sell_Values SET PriceUnit = {price}, Update_ServerDateTime = {now}, EditOperationDate = {now}, Edit_User_Id = {Common.User_Id}
WHERE Item_ID = {itemId} AND Price_Type_List_ID = {typeId}");
            if (n > 0) return;
            Insert(opp, "Price_Type_Sell_Values", "ID", "Item_ID, Store_ID, Price_Type_List_ID, PriceUnit, CommisionRat, CommisionValue, Is_CommisionRat, OperationDate, User_Id, Branch_Id, Insert_ServerDateTime",
                   "{1}, 0, {2}, {3}, 0, 0, 0, {4}, {5}, {6}, {7}", itemId, typeId, price, now.Date, Common.User_Id, Common.Branch_Id, now);
        }

        /// <summary>
        /// إدراج صف: عمود الهوية يولّده الخادم، وغيره بمعرّف Snowflake مثل EF. القيم معاملات ({1}...)، و{0} محجوز للمعرّف؛ أسماء الجدول والأعمدة من الكود فقط.
        /// </summary>
        private static long Insert(operations opp, string table, string idCol, string cols, string vals, params object[] args)
        {
            var all = new object[args.Length + 1];
            Array.Copy(args, 0, all, 1, args.Length);
            if (IsIdentity(opp, table, idCol))
            {
                all[0] = 0L;
                return opp.db.Database.SqlQuery<long>(FormattableStringFactory.Create(
                    $"INSERT INTO [{table}] ({cols}) OUTPUT CAST(INSERTED.[{idCol}] AS bigint) AS Value VALUES ({vals})", all)).AsEnumerable().First();
            }
            long id = CallSnowflakeIdGenerate.GenerateNewId();
            all[0] = id;
            opp.db.Database.ExecuteSql(FormattableStringFactory.Create($"INSERT INTO [{table}] ([{idCol}], {cols}) VALUES ({{0}}, {vals})", all));
            return id;
        }

        private static bool IsIdentity(operations opp, string table, string col) =>
            opp.db.Database.SqlQuery<int>(FormattableStringFactory.Create(
                "SELECT ISNULL(COLUMNPROPERTY(OBJECT_ID({0}), {1}, 'IsIdentity'), 0) AS Value", table, col)).AsEnumerable().First() == 1;

        private void Demand()
        {
            if (!_hasPermission(PermissionEdit)) throw new InvalidOperationException("ليس لديك صلاحية تعديل بيانات الأصناف وأسعارها.");
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
