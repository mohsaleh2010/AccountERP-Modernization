using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib
{
    /// <summary>
    /// كارت الصنف الموحَّد (خطة 01 — المرحلة 4، §2.5) لسطر مخزون واحد (صنف × مخزن/قسم) على دفتر الحركة DPIE:
    /// رصيد افتتاحى فى بداية الفترة (بقاعدة المحرك: آخر أول مدة + الحركات)، ثم كل حركة بالتاريخ والمستند ومن/إلى والوارد والمنصرف
    /// والرصيد الجارى والتكلفة. صفوف المحرك تُقرأ بـ Qty_Small، والصفوف القديمة بالكميتين واتجاه Transaction_Type.
    /// يحل محل ItemsCards وItemsCardsStore وItem_Transaction_Details و«كارت الصنف كمية وقيمة».
    /// </summary>
    public class StoreItemCardService
    {
        private readonly Func<operations> _newOpp;

        public StoreItemCardService(Func<operations> newOpp = null) => _newOpp = newOpp ?? (() => new operations());

        public class CardHeader
        {
            public long Unit_Item_Id { get; set; }
            public string Item_Name { get; set; }
            public string Code { get; set; }
            public string Store_Name { get; set; }
            public string B_Unit { get; set; }
            public string S_Unit { get; set; }
            public decimal Factor { get; set; }
            public decimal Stored_Small { get; set; }
            public decimal Avg_Cost_Large { get; set; }
        }

        public class CardRow
        {
            public long Id { get; set; }
            public DateTime? Process_Date { get; set; }
            public string Doc_Name { get; set; }
            public long Order_Id { get; set; }
            public int Order_Type { get; set; }
            public long Order_No { get; set; }
            public long Invoice_Id { get; set; }
            public int Type_From { get; set; }
            public int Type_To { get; set; }
            public decimal In_Small { get; set; }
            public decimal Out_Small { get; set; }
            public decimal Running_Small { get; set; }
            public decimal? Unit_Cost_Small { get; set; }
            public bool Is_Reversal { get; set; }
            public bool Engine_Posted { get; set; }
            public string Notes { get; set; }
        }

        public CardHeader GetHeader(long unitItemId) => Run(opp => opp.db.Database.SqlQuery<CardHeader>($@"
SELECT u.id AS Unit_Item_Id, ISNULL(i.Item_Name, N'') AS Item_Name, ISNULL(ISNULL(u.Code, i.code), N'') AS Code, ISNULL(s.Store_Name, N'') AS Store_Name,
       ISNULL(un.B_Unit, N'') AS B_Unit, ISNULL(un.S_unit, N'') AS S_Unit,
       CAST(CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END AS decimal(18,6)) AS Factor,
       CAST(ISNULL(u.Full_qua, 0) * CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END + ISNULL(u.D_Qua, 0) AS decimal(18,4)) AS Stored_Small,
       CAST(ISNULL(u.P_priceAvgCost, 0) AS decimal(18,6)) AS Avg_Cost_Large
FROM Unit_item u
LEFT JOIN Item i ON i.id = u.Item_id
LEFT JOIN Store s ON s.Id = u.Store_id
LEFT JOIN unit un ON un.id = u.unit_id
WHERE u.id = {unitItemId}").AsEnumerable().FirstOrDefault());

        /// <summary>الرصيد الافتتاحى بالوحدة الصغرى فى بداية يوم from (= المحسوب حتى نهاية اليوم السابق).</summary>
        public decimal GetOpening(long unitItemId, DateTime from) =>
            Run(opp => new StockMovementService(opp).GetComputedBalanceSmall(unitItemId, from.Date.AddDays(-1)));

        /// <summary>
        /// حركات الفترة [from, to] مرتبة بالتاريخ ثم المعرّف، والرصيد الجارى = الافتتاحى + المجموع التراكمى.
        /// الحركات قبل آخر «أول مدة» داخل الفترة تبقى ظاهرة للمرجع؛ الرصيد الجارى يتبع قاعدة الدفتر نفسها.
        /// </summary>
        public List<CardRow> GetRows(long unitItemId, DateTime from, DateTime to)
        {
            decimal opening = GetOpening(unitItemId, from);
            DateTime start = from.Date, end = to.Date.AddDays(1);
            return Run(opp => opp.db.Database.SqlQuery<CardRow>($@"
WITH m AS (
    SELECT x.Id, x.Process_Date,
           COALESCE(NULLIF(x.ProcessName, N''), pl.ProcessName, NULLIF(x.Invoice_Type_Str, N''),
                    CASE WHEN x.ProcessNameId = {StockMovementService.LedgerCorrectionProcessId} THEN N'تصحيح الدفتر بالمطابقة'
                         WHEN x.ProcessNameId = {StockMovementService.OpeningBalanceProcessId} THEN N'رصيد أول المدة' END, N'') AS Doc_Name,
           ISNULL(x.OrderTable_id, 0) AS Order_Id,
           CASE WHEN p.Order_Type BETWEEN 1 AND 23 THEN p.Order_Type + 1000 ELSE ISNULL(p.Order_Type, 0) END AS Order_Type,
           CAST(ISNULL(p.Order_Purshing_No, 0) AS bigint) AS Order_No,
           ISNULL(x.Invoice_Id, 0) AS Invoice_Id, ISNULL(x.Type_From, 0) AS Type_From, ISNULL(x.Type_To, 0) AS Type_To,
           CAST(CASE WHEN x.Engine_Posted = 1 THEN ISNULL(x.Qty_Small, 0)
                     WHEN x.Transaction_Type = 1 THEN  (ISNULL(x.qua, 0) * k.F + ISNULL(x.Part_qua, 0))
                     WHEN x.Transaction_Type = 2 THEN -(ISNULL(x.qua, 0) * k.F + ISNULL(x.Part_qua, 0))
                     ELSE 0 END AS decimal(18,4)) AS Signed,
           CAST(x.Unit_Cost AS decimal(18,6)) AS Unit_Cost_Small,
           CAST(CASE WHEN x.Reversal_Of_Id IS NOT NULL THEN 1 ELSE 0 END AS bit) AS Is_Reversal,
           CAST(CASE WHEN x.Engine_Posted = 1 THEN 1 ELSE 0 END AS bit) AS Engine_Posted,
           x.Notes
    FROM Detailes_Purshing_Import_Export x
    INNER JOIN Unit_item u ON u.id = x.Unit_item_Table_Id
    CROSS APPLY (SELECT CAST(CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END AS decimal(18,6)) AS F) k
    LEFT JOIN Order_Purshing p ON p.Id = x.OrderTable_id AND x.OrderTable_id > 0
    LEFT JOIN DataDictionaryProcessesLists pl ON pl.Id = NULLIF(x.ProcessNameId, 0)
    WHERE x.Unit_item_Table_Id = {unitItemId}
      AND x.Process_Date >= {start} AND x.Process_Date < {end}
)
SELECT m.Id, m.Process_Date, m.Doc_Name, m.Order_Id, m.Order_Type, m.Order_No, m.Invoice_Id, m.Type_From, m.Type_To,
       CAST(CASE WHEN m.Signed > 0 THEN m.Signed ELSE 0 END AS decimal(18,4)) AS In_Small,
       CAST(CASE WHEN m.Signed < 0 THEN -m.Signed ELSE 0 END AS decimal(18,4)) AS Out_Small,
       CAST({opening} + SUM(m.Signed) OVER (ORDER BY m.Process_Date, m.Id ROWS UNBOUNDED PRECEDING) AS decimal(18,4)) AS Running_Small,
       m.Unit_Cost_Small, m.Is_Reversal, m.Engine_Posted, m.Notes
FROM m
ORDER BY m.Process_Date, m.Id").ToList());
        }

        private T Run<T>(Func<operations, T> f) => f(_newOpp());
    }
}
