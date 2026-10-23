using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib
{
    /// <summary>
    /// أرصدة الأصناف بتاريخ الصلاحية (خطة 05 بند 23) لصفحة /store/balances/expiry: بديل ItemBalanceWithExpireFrm.
    /// سطر لكل تشغيلة فى Unit_Item_ExpireDate (رصيدها Larg_Balance/Small_Balance كما تحدّثه الفواتير) مع حالتها:
    /// 1 منتهية، 2 قريبة الانتهاء (خلال NearDays يوماً)، 3 سليمة، 4 بلا تاريخ. القيمة بسعر شراء التشغيلة واحتياطياً سعر شراء السطر.
    /// التشغيلات اليتيمة بلا صنف لا تُعرض، والمعطَّلة (IsActive = 0) كذلك.
    /// </summary>
    public class ItemExpiryBalancesService
    {
        private readonly Func<operations> _newOpp;

        public ItemExpiryBalancesService(Func<operations> newOpp = null) => _newOpp = newOpp ?? (() => new operations());

        public enum ExpiryStatus { All = 0, Expired = 1, Near = 2, Ok = 3, NoDate = 4 }

        public class ExpiryFilter
        {
            public long StoreId { get; set; }
            public long CategoryId { get; set; }
            /// <summary>0 = كل الأصناف؛ وإلا تشغيلات صنف واحد (رابط «التشغيلات» من أرصدة الأدوية).</summary>
            public long ItemId { get; set; }
            public string Text { get; set; }
            public ExpiryStatus Status { get; set; }
            public int NearDays { get; set; } = 90;
            public bool IncludeZero { get; set; }
            public int Skip { get; set; }
            public int Take { get; set; } = 50;
        }

        public class ExpiryRow
        {
            public long Id { get; set; }
            public long Item_Id { get; set; }
            public string Item_Name { get; set; }
            public string Code { get; set; }
            public string Category_Name { get; set; }
            public string Store_Name { get; set; }
            public string Batch { get; set; }
            public DateTime? Expire_Date { get; set; }
            public int? Days_Left { get; set; }
            public int Status { get; set; }
            public string B_Unit { get; set; }
            public string S_Unit { get; set; }
            public decimal Factor { get; set; }
            public decimal Qty_Small { get; set; }
            public decimal? Price_Large { get; set; }
            public decimal? Value { get; set; }
            public int Total_Rows { get; set; }
            public int Expired_Rows { get; set; }
            public int Near_Rows { get; set; }
            public int Ok_Rows { get; set; }
            public int NoDate_Rows { get; set; }
            public decimal Expired_Value { get; set; }
            public decimal Near_Value { get; set; }
        }

        /// <summary>
        /// التشغيلات مرتبة بالأقرب انتهاءً (بلا تاريخ فى الآخر). عدّادات الحالات وقيمها محسوبة على كل النتيجة قبل تصفية الحالة
        /// (لتظهر على أزرار الحالة)، وTotal_Rows بعد تصفيتها (للترقيم).
        /// </summary>
        public List<ExpiryRow> GetBatches(ExpiryFilter f)
        {
            string text = (f.Text ?? "").Trim();
            string like = "%" + text + "%";
            DateTime today = DateTime.Today;
            DateTime nearUntil = today.AddDays(Math.Clamp(f.NearDays, 1, 3650) + 1);
            int status = (int)f.Status, includeZero = f.IncludeZero ? 1 : 0;
            int skip = Math.Max(0, f.Skip), take = Math.Clamp(f.Take, 1, 500);
            return Run(opp => opp.db.Database.SqlQuery<ExpiryRow>($@"
WITH l AS (
    SELECT e.Id, i.id AS Item_Id, ISNULL(i.Item_Name, N'') AS Item_Name, ISNULL(ISNULL(u.Code, i.code), N'') AS Code,
           ISNULL(c.Category_Name, N'') AS Category_Name, ISNULL(s.Store_Name, N'') AS Store_Name,
           COALESCE(NULLIF(LTRIM(RTRIM(e.Tash3ela)), N''), CASE WHEN e.BatchNo > 0 THEN CAST(e.BatchNo AS nvarchar(30)) END, N'') AS Batch,
           e.Item_Expire_Date AS Expire_Date,
           CASE WHEN e.Item_Expire_Date IS NULL THEN NULL ELSE DATEDIFF(DAY, {today}, e.Item_Expire_Date) END AS Days_Left,
           CASE WHEN e.Item_Expire_Date IS NULL THEN 4 WHEN e.Item_Expire_Date < {today} THEN 1 WHEN e.Item_Expire_Date < {nearUntil} THEN 2 ELSE 3 END AS Status,
           ISNULL(un.B_Unit, N'') AS B_Unit, ISNULL(un.S_unit, N'') AS S_Unit, f.F AS Factor,
           CAST(ISNULL(e.Larg_Balance, 0) * f.F + ISNULL(e.Small_Balance, 0) AS decimal(18,4)) AS Qty_Small,
           CAST(COALESCE(NULLIF(e.P_Price, 0), NULLIF(u.P_price, 0)) AS decimal(18,6)) AS Price_Large
    FROM Unit_Item_ExpireDate e
    INNER JOIN Item i ON i.id = e.Item_id
    LEFT JOIN Unit_item u ON u.id = e.Unit_Item_Table_Id
    LEFT JOIN Store s ON s.Id = u.Store_id
    LEFT JOIN Item_Category c ON c.ID = i.Item_Category_ID
    LEFT JOIN unit un ON un.id = ISNULL(u.unit_id, i.Unit_Id)
    CROSS APPLY (SELECT CAST(COALESCE(NULLIF(u.Small_unit, 0), NULLIF(un.S_unit_Value, 0), 1) AS decimal(18,6)) AS F) f
    WHERE ISNULL(e.IsActive, 1) = 1
      AND ({f.StoreId} = 0 OR u.Store_id = {f.StoreId})
      AND ({f.CategoryId} = 0 OR i.Item_Category_ID = {f.CategoryId})
      AND ({f.ItemId} = 0 OR e.Item_id = {f.ItemId})
      AND ({text} = N'' OR i.Item_Name LIKE {like} OR u.Code = {text} OR i.code = {text} OR e.Tash3ela = {text})
), v AS (
    SELECT l.*, CAST(CASE WHEN l.Price_Large IS NULL THEN NULL ELSE l.Qty_Small / l.Factor * l.Price_Large END AS decimal(18,2)) AS Value
    FROM l WHERE {includeZero} = 1 OR l.Qty_Small <> 0
), w AS (
    SELECT v.*,
           SUM(CASE WHEN v.Status = 1 THEN 1 ELSE 0 END) OVER () AS Expired_Rows,
           SUM(CASE WHEN v.Status = 2 THEN 1 ELSE 0 END) OVER () AS Near_Rows,
           SUM(CASE WHEN v.Status = 3 THEN 1 ELSE 0 END) OVER () AS Ok_Rows,
           SUM(CASE WHEN v.Status = 4 THEN 1 ELSE 0 END) OVER () AS NoDate_Rows,
           CAST(ISNULL(SUM(CASE WHEN v.Status = 1 THEN v.Value END) OVER (), 0) AS decimal(18,2)) AS Expired_Value,
           CAST(ISNULL(SUM(CASE WHEN v.Status = 2 THEN v.Value END) OVER (), 0) AS decimal(18,2)) AS Near_Value
    FROM v
)
SELECT w.*, COUNT(*) OVER () AS Total_Rows
FROM w
WHERE {status} = 0 OR w.Status = {status}
ORDER BY CASE WHEN w.Expire_Date IS NULL THEN 1 ELSE 0 END, w.Expire_Date, w.Item_Name, w.Store_Name
OFFSET {skip} ROWS FETCH NEXT {take} ROWS ONLY").ToList());
        }

        private T Run<T>(Func<operations, T> f) => f(_newOpp());
    }
}
