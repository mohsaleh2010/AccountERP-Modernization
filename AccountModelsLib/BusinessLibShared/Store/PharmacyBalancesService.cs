using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib
{
    /// <summary>
    /// أرصدة أصناف الأدوية (خطة 05 بند 24) لصفحة /store/balances/pharmacy: بديل Item_balance_ph.
    /// سطر لكل صنف دواء فى مخزن: الرصيد بالعبوة والجزء، حد الطلب (Item.Upon_Request بالعبوة)، وأقرب صلاحية من التشغيلات ذات الرصيد.
    /// أصناف الأدوية = فئات ملفها pharmacy، أو كل الأصناف إن لم تُحدَّد فئة أدوية بعد (نفس قاعدة ItemEditorService).
    /// لا تعديل رصيد مباشر هنا (الديسك توب كان يحدّث Unit_item بلا حركة)؛ تعديل الرصيد بإذن تسوية من المخازن.
    /// </summary>
    public class PharmacyBalancesService
    {
        private readonly Func<operations> _newOpp;

        public PharmacyBalancesService(Func<operations> newOpp = null) => _newOpp = newOpp ?? (() => new operations());

        public class PharmacyFilter
        {
            public long StoreId { get; set; }
            public long CategoryId { get; set; }
            public long TypeId { get; set; }
            public long FactoryId { get; set; }
            public string Text { get; set; }
            /// <summary>الأصناف التى بلغت حد الطلب أو نزلت عنه فقط.</summary>
            public bool BelowReorder { get; set; }
            public bool IncludeZero { get; set; }
            public int NearDays { get; set; } = 90;
            public int Skip { get; set; }
            public int Take { get; set; } = 50;
        }

        public class PharmacyRow
        {
            public long Unit_Item_Id { get; set; }
            public long Item_Id { get; set; }
            public string Code { get; set; }
            public string Item_Name { get; set; }
            public string English_Name { get; set; }
            public string Type_Name { get; set; }
            public string Factory_Name { get; set; }
            public string Store_Name { get; set; }
            public string B_Unit { get; set; }
            public string S_Unit { get; set; }
            public decimal Factor { get; set; }
            public decimal Balance_Small { get; set; }
            public decimal Reorder_Large { get; set; }
            public bool Below_Reorder { get; set; }
            public DateTime? Nearest_Expiry { get; set; }
            /// <summary>0 بلا تشغيلات بتاريخ، 1 منتهية، 2 قريبة، 3 سليمة.</summary>
            public int Expiry_Status { get; set; }
            public int Batches { get; set; }
            public int Total_Rows { get; set; }
            public int Below_Rows { get; set; }
            public int Expired_Rows { get; set; }
            public int Near_Rows { get; set; }
        }

        public class Option
        {
            public long Id { get; set; }
            public string Name { get; set; }
        }

        /// <summary>فئات الأدوية (أو كل الفئات إن لم تُحدَّد فئة أدوية)، والأنواع والمصانع النشطة لقوائم التصفية.</summary>
        public (List<Option> Categories, List<Option> Types, List<Option> Factories) GetLookups() => Run(opp => (
            opp.db.Database.SqlQuery<Option>($@"
SELECT CAST(ID AS bigint) AS Id, Category_Name AS Name FROM Item_Category
WHERE ISNULL(IS_Active, 1) = 1
  AND (Item_Profile = N'pharmacy' OR NOT EXISTS (SELECT 1 FROM Item_Category pc WHERE pc.Item_Profile = N'pharmacy'))
ORDER BY Category_Name").ToList(),
            opp.db.Database.SqlQuery<Option>($"SELECT CAST(Id AS bigint) AS Id, ItemsType_Name AS Name FROM Items_Types_Name WHERE ISNULL(IsActive, 1) = 1 ORDER BY ItemsType_Name").ToList(),
            opp.db.Database.SqlQuery<Option>($"SELECT CAST(Id AS bigint) AS Id, Type_Name AS Name FROM Items_TypesFactory_Name ORDER BY Type_Name").ToList()));

        /// <summary>
        /// الأسطر مرتبة بالأقرب صلاحية ثم الاسم. عدّادات حد الطلب والصلاحية محسوبة على كل النتيجة (لا الصفحة فقط).
        /// </summary>
        public List<PharmacyRow> GetBalances(PharmacyFilter f)
        {
            string text = (f.Text ?? "").Trim();
            string like = "%" + text + "%";
            DateTime today = DateTime.Today;
            DateTime nearUntil = today.AddDays(Math.Clamp(f.NearDays, 1, 3650) + 1);
            int below = f.BelowReorder ? 1 : 0, includeZero = f.IncludeZero ? 1 : 0;
            int skip = Math.Max(0, f.Skip), take = Math.Clamp(f.Take, 1, 500);
            return Run(opp => opp.db.Database.SqlQuery<PharmacyRow>($@"
WITH l AS (
    SELECT u.id AS Unit_Item_Id, i.id AS Item_Id, ISNULL(ISNULL(u.Code, i.code), N'') AS Code, ISNULL(i.Item_Name, N'') AS Item_Name,
           ISNULL(i.AnotherName, N'') AS English_Name, ISNULL(t.ItemsType_Name, N'') AS Type_Name, ISNULL(fa.Type_Name, N'') AS Factory_Name,
           s.Store_Name, ISNULL(un.B_Unit, N'') AS B_Unit, ISNULL(un.S_unit, N'') AS S_Unit, k.F AS Factor,
           CAST(ISNULL(u.Full_qua, 0) * k.F + ISNULL(u.D_Qua, 0) AS decimal(18,4)) AS Balance_Small,
           CAST(ISNULL(i.Upon_Request, 0) AS decimal(18,4)) AS Reorder_Large,
           ex.Nearest_Expiry, ISNULL(ex.Batches, 0) AS Batches
    FROM Unit_item u
    INNER JOIN Store s ON s.Id = u.Store_id
    INNER JOIN Item i ON i.id = u.Item_id
    LEFT JOIN Item_Category c ON c.ID = i.Item_Category_ID
    LEFT JOIN Items_Types_Name t ON t.Id = i.Items_Types_Name_Id
    LEFT JOIN Items_TypesFactory_Name fa ON fa.Id = i.Items_TypesFactory_NameId
    LEFT JOIN unit un ON un.id = u.unit_id
    CROSS APPLY (SELECT CAST(CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END AS decimal(18,6)) AS F) k
    OUTER APPLY (SELECT MIN(e.Item_Expire_Date) AS Nearest_Expiry, COUNT(*) AS Batches FROM Unit_Item_ExpireDate e
                 WHERE e.Unit_Item_Table_Id = u.id AND ISNULL(e.IsActive, 1) = 1
                   AND (ISNULL(e.Larg_Balance, 0) <> 0 OR ISNULL(e.Small_Balance, 0) <> 0)) ex
    WHERE ISNULL(u.Store_Type, 4) = 4 AND ISNULL(s.LookUp_StoreOrderTypesId, 4) = 4
      AND ISNULL(i.Active_flage, 1) = 1 AND ISNULL(u.active_flage, 1) = 1 AND ISNULL(i.Is_ItemWithoutBalance, 0) = 0
      AND (c.Item_Profile = N'pharmacy' OR NOT EXISTS (SELECT 1 FROM Item_Category pc WHERE pc.Item_Profile = N'pharmacy'))
      AND ({f.StoreId} = 0 OR u.Store_id = {f.StoreId})
      AND ({f.CategoryId} = 0 OR i.Item_Category_ID = {f.CategoryId})
      AND ({f.TypeId} = 0 OR i.Items_Types_Name_Id = {f.TypeId})
      AND ({f.FactoryId} = 0 OR i.Items_TypesFactory_NameId = {f.FactoryId})
      AND ({text} = N'' OR i.Item_Name LIKE {like} OR i.AnotherName LIKE {like} OR u.Code = {text} OR i.code = {text} OR i.BarCode2 = {text} OR i.BarCode3 = {text})
), v AS (
    SELECT l.*,
           CAST(CASE WHEN l.Reorder_Large > 0 AND l.Balance_Small <= l.Reorder_Large * l.Factor THEN 1 ELSE 0 END AS bit) AS Below_Reorder,
           CASE WHEN l.Nearest_Expiry IS NULL THEN 0 WHEN l.Nearest_Expiry < {today} THEN 1 WHEN l.Nearest_Expiry < {nearUntil} THEN 2 ELSE 3 END AS Expiry_Status
    FROM l
    WHERE ({includeZero} = 1 OR l.Balance_Small <> 0 OR ({below} = 1 AND l.Reorder_Large > 0))
)
SELECT v.*, COUNT(*) OVER () AS Total_Rows,
       SUM(CASE WHEN v.Below_Reorder = 1 THEN 1 ELSE 0 END) OVER () AS Below_Rows,
       SUM(CASE WHEN v.Expiry_Status = 1 THEN 1 ELSE 0 END) OVER () AS Expired_Rows,
       SUM(CASE WHEN v.Expiry_Status = 2 THEN 1 ELSE 0 END) OVER () AS Near_Rows
FROM v
WHERE {below} = 0 OR v.Below_Reorder = 1
ORDER BY CASE WHEN v.Nearest_Expiry IS NULL THEN 1 ELSE 0 END, v.Nearest_Expiry, v.Item_Name, v.Store_Name
OFFSET {skip} ROWS FETCH NEXT {take} ROWS ONLY").ToList());
        }

        private T Run<T>(Func<operations, T> f) => f(_newOpp());
    }
}
