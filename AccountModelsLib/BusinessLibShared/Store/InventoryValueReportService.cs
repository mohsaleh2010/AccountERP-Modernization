using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib
{
    /// <summary>
    /// تقرير قيمة المخزون (م-8): لكل مخزن قيمة الرصيد الحالى بآخر سعر شراء وبمتوسط التكلفة معاً، وتفاصيل أصناف المخزن.
    /// نفس مصدر الأسعار فى StoreBalancesService: متوسط تكلفة الفرع (Item_Branch_Cost) ثم Unit_item.P_priceAvgCost،
    /// وآخر سعر شراء الفرع ثم Unit_item.P_price. قراءة فقط.
    /// </summary>
    public class InventoryValueReportService
    {
        private readonly Func<operations> _newOpp;

        public InventoryValueReportService(Func<operations> newOpp = null) => _newOpp = newOpp ?? (() => new operations());

        public class StoreValueRow
        {
            public long Store_Id { get; set; }
            public string Store_Name { get; set; }
            public int Items_Count { get; set; }
            public decimal Last_Purchase_Total { get; set; }
            public decimal Avg_Cost_Total { get; set; }
        }

        public class ItemValueRow
        {
            public long Unit_Item_Id { get; set; }
            public long Store_Id { get; set; }
            public string Store_Name { get; set; }
            public string Item_Name { get; set; }
            public string Code { get; set; }
            public string B_Unit { get; set; }
            public string S_Unit { get; set; }
            public decimal Factor { get; set; }
            public decimal Balance_Small { get; set; }
            public decimal? Last_Purchase_Price { get; set; }
            public decimal? Avg_Cost { get; set; }
            public decimal Last_Purchase_Total { get; set; }
            public decimal Avg_Cost_Total { get; set; }
        }

        /// <summary>ملخص كل المخازن (النوع 4) ذات الرصيد غير الصفرى، مرتبة بالاسم.</summary>
        public List<StoreValueRow> GetStoreSummary() =>
            GetItems(0).GroupBy(r => r.Store_Id).Select(g => new StoreValueRow
            {
                Store_Id = g.Key,
                Store_Name = g.First().Store_Name,
                Items_Count = g.Count(),
                Last_Purchase_Total = g.Sum(x => x.Last_Purchase_Total),
                Avg_Cost_Total = g.Sum(x => x.Avg_Cost_Total),
            }).OrderBy(s => s.Store_Name).ToList();

        /// <summary>أصناف مخزن (أو كل المخازن لو 0) برصيد غير صفرى، مع الأسعار للوحدة الكبرى وإجمالى كل سعر.</summary>
        public List<ItemValueRow> GetItems(long storeId)
        {
            var opp = _newOpp();
            return opp.db.Database.SqlQuery<ItemValueRow>($@"
WITH l AS (
    SELECT u.id AS Unit_Item_Id, u.Store_id AS Store_Id, s.Store_Name, ISNULL(i.Item_Name, N'') AS Item_Name, ISNULL(ISNULL(u.Code, i.code), N'') AS Code,
           ISNULL(un.B_Unit, N'') AS B_Unit, ISNULL(un.S_unit, N'') AS S_Unit,
           CAST(CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END AS decimal(18,6)) AS Factor,
           CAST(ISNULL(u.Full_qua, 0) * CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END + ISNULL(u.D_Qua, 0) AS decimal(18,4)) AS Balance_Small,
           CAST(ISNULL(bc.Last_Purchase_Price * CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END, u.P_price) AS decimal(18,6)) AS Last_Purchase_Price,
           CAST(ISNULL(bc.Avg_Cost * CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END, u.P_priceAvgCost) AS decimal(18,6)) AS Avg_Cost
    FROM Unit_item u
    INNER JOIN Store s ON s.Id = u.Store_id
    INNER JOIN Item i ON i.id = u.Item_id
    LEFT JOIN unit un ON un.id = u.unit_id
    OUTER APPLY (SELECT TOP 1 b.Avg_Cost, b.Last_Purchase_Price FROM Item_Branch_Cost b WHERE b.Item_Id = u.Item_id AND b.Branch_Id = s.Branch_Id) bc
    WHERE ISNULL(u.Store_Type, 4) = 4 AND ISNULL(s.LookUp_StoreOrderTypesId, 4) = 4
      AND ({storeId} = 0 OR u.Store_id = {storeId})
      AND ISNULL(i.Active_flage, 1) = 1 AND ISNULL(u.active_flage, 1) = 1 AND ISNULL(i.Is_ItemWithoutBalance, 0) = 0
)
SELECT l.*,
       CAST(ISNULL(l.Balance_Small / l.Factor * l.Last_Purchase_Price, 0) AS decimal(18,2)) AS Last_Purchase_Total,
       CAST(ISNULL(l.Balance_Small / l.Factor * l.Avg_Cost, 0) AS decimal(18,2)) AS Avg_Cost_Total
FROM l WHERE l.Balance_Small <> 0
ORDER BY l.Store_Name, l.Item_Name").ToList();
        }
    }
}
