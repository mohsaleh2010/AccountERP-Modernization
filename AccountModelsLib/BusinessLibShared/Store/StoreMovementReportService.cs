using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib
{
    /// <summary>
    /// تقارير الصرف والوارد (خطة 03 — المرحلة 6): ملخص حركة أذون المخزن لكل صنف فى فترة، من دفتر الحركة DPIE المرتبط
    /// بأذون Order_Purshing المرحَّلة. بديل «تقارير صرف أصناف وخامات» (المنصرف، الخامات للإنتاج، قطع الغيار، الهالك)، التسويات،
    /// المستلم من الإنتاج، تقارير الماكينات، والاستلام من الموردين. نوع الإذن من رأس الإذن (والأرقام القصيرة 1..23 = 1001..1023).
    /// القيمة من صفوف المحرك فقط (لقطة التكلفة)؛ الصفوف القديمة بلا قيمة.
    /// </summary>
    public class StoreMovementReportService
    {
        private readonly Func<operations> _newOpp;

        public StoreMovementReportService(Func<operations> newOpp = null) => _newOpp = newOpp ?? (() => new operations());

        public class ReportFilter
        {
            /// <summary>0 = كل الأنواع.</summary>
            public int OrderType { get; set; }
            public int StoreType { get; set; } = 4;
            public long StoreId { get; set; }
            public DateTime From { get; set; }
            public DateTime To { get; set; }
            public string Text { get; set; }
            public int Skip { get; set; }
            public int Take { get; set; } = 50;
        }

        public class ReportRow
        {
            public long Item_Id { get; set; }
            public string Item_Name { get; set; }
            public string Code { get; set; }
            public string B_Unit { get; set; }
            public string S_Unit { get; set; }
            public decimal Factor { get; set; }
            public decimal In_Small { get; set; }
            public decimal Out_Small { get; set; }
            public decimal? In_Value { get; set; }
            public decimal? Out_Value { get; set; }
            public int Moves { get; set; }
            public int Total_Rows { get; set; }
            public decimal Total_In_Value { get; set; }
            public decimal Total_Out_Value { get; set; }
        }

        public List<ReportRow> GetSummary(ReportFilter f)
        {
            string text = (f.Text ?? "").Trim();
            string like = "%" + text + "%";
            DateTime from = f.From.Date, to = f.To.Date.AddDays(1);
            int skip = Math.Max(0, f.Skip), take = Math.Clamp(f.Take, 1, 500);
            return Run(opp => opp.db.Database.SqlQuery<ReportRow>($@"
WITH m AS (
    SELECT u.Item_id, i.Item_Name, ISNULL(i.code, N'') AS Code, ISNULL(un.B_Unit, N'') AS B_Unit, ISNULL(un.S_unit, N'') AS S_Unit,
           k.F AS Factor,
           CAST(CASE WHEN x.Engine_Posted = 1 THEN ISNULL(x.Qty_Small, 0)
                     WHEN x.Transaction_Type = 1 THEN  (ISNULL(x.qua, 0) * k.F + ISNULL(x.Part_qua, 0))
                     WHEN x.Transaction_Type = 2 THEN -(ISNULL(x.qua, 0) * k.F + ISNULL(x.Part_qua, 0))
                     ELSE 0 END AS decimal(18,4)) AS Signed,
           CASE WHEN x.Engine_Posted = 1 THEN CAST(ISNULL(x.Qty_Small, 0) * ISNULL(x.Unit_Cost, 0) AS decimal(18,2)) END AS SignedValue
    FROM Detailes_Purshing_Import_Export x
    INNER JOIN Order_Purshing p ON p.Id = x.OrderTable_id AND ISNULL(p.Doc_Status, 1) IN (1, 2)
    INNER JOIN Unit_item u ON u.id = x.Unit_item_Table_Id
    INNER JOIN Item i ON i.id = u.Item_id
    INNER JOIN Store s ON s.Id = u.Store_id
    LEFT JOIN unit un ON un.id = u.unit_id
    CROSS APPLY (SELECT CAST(CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END AS decimal(18,6)) AS F) k
    WHERE x.Process_Date >= {from} AND x.Process_Date < {to}
      AND ({f.OrderType} = 0 OR CASE WHEN p.Order_Type BETWEEN 1 AND 23 THEN p.Order_Type + 1000 ELSE p.Order_Type END = {f.OrderType})
      AND ISNULL(u.Store_Type, 4) = {f.StoreType} AND ({f.StoreId} = 0 OR u.Store_id = {f.StoreId})
      AND ({text} = N'' OR i.Item_Name LIKE {like} OR i.code = {text})
), g AS (
    SELECT Item_id AS Item_Id, MAX(Item_Name) AS Item_Name, MAX(Code) AS Code, MAX(B_Unit) AS B_Unit, MAX(S_Unit) AS S_Unit, MAX(Factor) AS Factor,
           SUM(CASE WHEN Signed > 0 THEN Signed ELSE 0 END) AS In_Small,
           SUM(CASE WHEN Signed < 0 THEN -Signed ELSE 0 END) AS Out_Small,
           SUM(CASE WHEN Signed > 0 THEN SignedValue END) AS In_Value,
           -SUM(CASE WHEN Signed < 0 THEN SignedValue END) AS Out_Value,
           COUNT(*) AS Moves
    FROM m GROUP BY Item_id
)
SELECT g.*, COUNT(*) OVER () AS Total_Rows,
       CAST(ISNULL(SUM(g.In_Value) OVER (), 0) AS decimal(18,2)) AS Total_In_Value,
       CAST(ISNULL(SUM(g.Out_Value) OVER (), 0) AS decimal(18,2)) AS Total_Out_Value
FROM g
ORDER BY g.Item_Name
OFFSET {skip} ROWS FETCH NEXT {take} ROWS ONLY").ToList());
        }

        // ============================== الاحتياجات (حد الطلب) ==============================

        public class NeedRow
        {
            public long Unit_Item_Id { get; set; }
            public string Store_Name { get; set; }
            public string Item_Name { get; set; }
            public string Code { get; set; }
            public string B_Unit { get; set; }
            public string S_Unit { get; set; }
            public decimal Factor { get; set; }
            public decimal Balance_Small { get; set; }
            public decimal Reorder_Large { get; set; }
            public decimal Shortage_Small { get; set; }
            public decimal? Last_Purchase_Large { get; set; }
            public int Total_Rows { get; set; }
        }

        /// <summary>
        /// الاحتياجات (بديل NeedeD_balance وNeeded_balance_Total): الأصناف التى رصيدها ≤ حد الطلب (Unit_item.Upon_Request بالوحدة الكبرى)،
        /// والكمية المقترحة = حد الطلب − الرصيد، مع آخر سعر شراء للتقدير. الأصناف بلا حد طلب (0) لا تظهر.
        /// </summary>
        public List<NeedRow> GetNeeds(int storeType, long storeId, string textIn, int skipIn, int takeIn)
        {
            string text = (textIn ?? "").Trim();
            string like = "%" + text + "%";
            int skip = Math.Max(0, skipIn), take = Math.Clamp(takeIn, 1, 500);
            return Run(opp => opp.db.Database.SqlQuery<NeedRow>($@"
WITH l AS (
    SELECT u.id AS Unit_Item_Id, s.Store_Name, i.Item_Name, ISNULL(ISNULL(u.Code, i.code), N'') AS Code,
           ISNULL(un.B_Unit, N'') AS B_Unit, ISNULL(un.S_unit, N'') AS S_Unit, k.F AS Factor,
           CAST(ISNULL(u.Full_qua, 0) * k.F + ISNULL(u.D_Qua, 0) AS decimal(18,4)) AS Balance_Small,
           CAST(ISNULL(u.Upon_Request, 0) AS decimal(18,4)) AS Reorder_Large,
           CAST(ISNULL(bc.Last_Purchase_Price * k.F, NULLIF(u.P_price, 0)) AS decimal(18,6)) AS Last_Purchase_Large
    FROM Unit_item u
    INNER JOIN Store s ON s.Id = u.Store_id
    INNER JOIN Item i ON i.id = u.Item_id
    LEFT JOIN unit un ON un.id = u.unit_id
    CROSS APPLY (SELECT CAST(CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END AS decimal(18,6)) AS F) k
    OUTER APPLY (SELECT TOP 1 b.Last_Purchase_Price FROM Item_Branch_Cost b WHERE b.Item_Id = u.Item_id AND b.Branch_Id = s.Branch_Id) bc
    WHERE ISNULL(u.Upon_Request, 0) > 0
      AND ISNULL(u.Store_Type, 4) = {storeType} AND ({storeId} = 0 OR u.Store_id = {storeId})
      AND ISNULL(i.Active_flage, 1) = 1 AND ISNULL(u.active_flage, 1) = 1 AND ISNULL(i.Is_ItemWithoutBalance, 0) = 0
      AND ({text} = N'' OR i.Item_Name LIKE {like} OR u.Code = {text} OR i.code = {text})
)
SELECT l.*, CAST(l.Reorder_Large * l.Factor - l.Balance_Small AS decimal(18,4)) AS Shortage_Small, COUNT(*) OVER () AS Total_Rows
FROM l
WHERE l.Balance_Small <= l.Reorder_Large * l.Factor
ORDER BY (l.Reorder_Large * l.Factor - l.Balance_Small) / NULLIF(l.Reorder_Large * l.Factor, 0) DESC, l.Item_Name
OFFSET {skip} ROWS FETCH NEXT {take} ROWS ONLY").ToList());
        }

        private T Run<T>(Func<operations, T> f) => f(_newOpp());
    }
}
