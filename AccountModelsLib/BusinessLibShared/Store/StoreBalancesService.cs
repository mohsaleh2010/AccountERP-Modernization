using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib
{
    /// <summary>
    /// أرصدة الأصناف والتقييم (خطة 03 — المرحلة 5) لصفحة /store/balances: بديل balance وItemQntyFrm وItemPurchaseQntyFrm
    /// وBalanceWith_* وItem_Balance_Date. الرصيد الحالى من Unit_item، والرصيد «بتاريخ» يُحسب من دفتر الحركة DPIE
    /// بنفس قاعدة StockMovementService.GetComputedBalanceSmall (آخر أول مدة قبل التاريخ + الحركات حتى التاريخ) لكن لكل الأسطر معاً.
    /// التقييم بمتوسط تكلفة الفرع (Item_Branch_Cost، واحتياطياً Unit_item.P_priceAvgCost) أو آخر سعر شراء أو سعر البيع،
    /// أو FIFO (أحدث فواتير الشراء التى تغطى الرصيد — بديل balance_CostAvgLastBalance، خطة 05 بند 26)،
    /// أو سعر المورد بنوع سعر شراء (Supplier_Price — بديل BalanceWith_Purchase_Price).
    /// </summary>
    public class StoreBalancesService
    {
        private readonly Func<operations> _newOpp;

        public StoreBalancesService(Func<operations> newOpp = null) => _newOpp = newOpp ?? (() => new operations());

        /// <summary>أساس التقييم: 0 كمية فقط، 1 متوسط التكلفة، 2 آخر سعر شراء، 3 سعر البيع، 4 FIFO أحدث المشتريات، 5 سعر المورد بنوع سعر شراء.</summary>
        public enum PriceBasis { QuantityOnly = 0, AverageCost = 1, LastPurchase = 2, SalePrice = 3, Fifo = 4, SupplierPrice = 5 }

        public class BalanceFilter
        {
            /// <summary>4 مخزن، 20 قسم.</summary>
            public int StoreType { get; set; } = 4;
            /// <summary>0 = كل مخازن/أقسام النوع.</summary>
            public long StoreId { get; set; }
            public long CategoryId { get; set; }
            public string Text { get; set; }
            /// <summary>null = الرصيد الحالى المخزَّن؛ تاريخ = الرصيد المحسوب من الدفتر فى نهاية ذلك اليوم.</summary>
            public DateTime? AsOf { get; set; }
            public bool IncludeZero { get; set; }
            public PriceBasis Price { get; set; }
            /// <summary>لأساس SupplierPrice: نوع سعر الشراء (Price_Purhase_Type_List، 0 = بلا نوع) والمورد (0 = أحدث سعر لأى مورد).</summary>
            public long PurchaseTypeId { get; set; }
            public long SupplierId { get; set; }
            public int Skip { get; set; }
            public int Take { get; set; } = 50;
        }

        public class BalanceRow
        {
            public long Unit_Item_Id { get; set; }
            public string Store_Name { get; set; }
            public string Item_Name { get; set; }
            public string Code { get; set; }
            public string Category_Name { get; set; }
            public string B_Unit { get; set; }
            public string S_Unit { get; set; }
            public decimal Factor { get; set; }
            public decimal Balance_Small { get; set; }
            public decimal? Price_Large { get; set; }
            public decimal? Value { get; set; }
            public int Total_Rows { get; set; }
            public decimal Total_Value { get; set; }
            public int Negative_Rows { get; set; }
        }

        public class Option
        {
            public long Id { get; set; }
            public string Name { get; set; }
        }

        public List<Option> GetStores(int storeType) => Run(opp => opp.db.Database.SqlQuery<Option>($@"
SELECT Id, Store_Name AS Name FROM Store
WHERE Active = 1 AND ISNULL(LookUp_StoreOrderTypesId, 4) = {storeType} AND ISNULL(IS_ServicesVirtual_Item_NoBalance, 0) = 0
ORDER BY IS_Default_Store DESC, Store_Name").ToList());

        public List<Option> GetCategories() => Run(opp => opp.db.Database.SqlQuery<Option>($@"
SELECT CAST(ID AS bigint) AS Id, Category_Name AS Name FROM Item_Category WHERE ISNULL(IS_Active, 1) = 1 ORDER BY Category_Name").ToList());

        /// <summary>أسطر الأرصدة مرتبة بالمخزن ثم الصنف، مع إجمالى القيمة وعدد الأسطر السالبة لكل النتيجة (لا للصفحة فقط).</summary>
        public List<BalanceRow> GetBalances(BalanceFilter f)
        {
            string text = (f.Text ?? "").Trim();
            string like = "%" + text + "%";
            bool asOf = f.AsOf.HasValue;
            DateTime until = (f.AsOf ?? DateTime.Today).Date.AddDays(1);
            int price = (int)f.Price;
            int skip = Math.Max(0, f.Skip), take = Math.Clamp(f.Take, 1, 500);
            int includeZero = f.IncludeZero ? 1 : 0, useAsOf = asOf ? 1 : 0;
            int opening = StockMovementService.OpeningBalanceProcessId;
            long ptype = f.PurchaseTypeId, supplier = f.SupplierId;
            return Run(opp => opp.db.Database.SqlQuery<BalanceRow>($@"
WITH l AS (
    SELECT u.id AS Unit_Item_Id, s.Store_Name, ISNULL(i.Item_Name, N'') AS Item_Name, ISNULL(ISNULL(u.Code, i.code), N'') AS Code,
           c.Category_Name, ISNULL(un.B_Unit, N'') AS B_Unit, ISNULL(un.S_unit, N'') AS S_Unit,
           CAST(CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END AS decimal(18,6)) AS Factor,
           bs.Small AS Balance_Small,
           CAST(CASE {price}
                     WHEN 1 THEN ISNULL(bc.Avg_Cost * CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END, u.P_priceAvgCost)
                     WHEN 2 THEN ISNULL(bc.Last_Purchase_Price * CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END, u.P_price)
                     WHEN 3 THEN u.S_price
                     -- FIFO: قيمة ما تغطيه أحدث المشتريات من الرصيد + الباقى (أقدم من الفواتير المسجَّلة) بمتوسط التكلفة ÷ الرصيد
                     WHEN 4 THEN CASE WHEN bs.Small <= 0 OR ISNULL(fifo.Covered, 0) <= 0
                                      THEN COALESCE(fifo.Last_Price, bc.Avg_Cost * CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END, u.P_priceAvgCost, u.P_price)
                                      ELSE (fifo.Covered_Value + (bs.Small / CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END - fifo.Covered)
                                            * COALESCE(bc.Avg_Cost * CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END, u.P_priceAvgCost, u.P_price, 0))
                                           / (bs.Small / CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END) END
                     WHEN 5 THEN spp.PriceUnit
                     ELSE NULL END AS decimal(18,6)) AS Price_Large
    FROM Unit_item u
    INNER JOIN Store s ON s.Id = u.Store_id
    LEFT JOIN Item i ON i.id = u.Item_id
    LEFT JOIN Item_Category c ON c.ID = i.Item_Category_ID
    LEFT JOIN unit un ON un.id = u.unit_id
    OUTER APPLY (SELECT TOP 1 b.Avg_Cost, b.Last_Purchase_Price FROM Item_Branch_Cost b WHERE b.Item_Id = u.Item_id AND b.Branch_Id = s.Branch_Id) bc
    OUTER APPLY (
        SELECT SUM(CASE WHEN x.Engine_Posted = 1 THEN ISNULL(x.Qty_Small, 0)
                        WHEN x.Transaction_Type = 1 THEN  (ISNULL(x.qua, 0) * k.F + ISNULL(x.Part_qua, 0))
                        WHEN x.Transaction_Type = 2 THEN -(ISNULL(x.qua, 0) * k.F + ISNULL(x.Part_qua, 0))
                        ELSE 0 END) AS Bal
        FROM Detailes_Purshing_Import_Export x
        -- عامل الوحدة كعمود داخلى: SQL Server لا يقبل تجميع تعبير يخلط عموداً خارجياً بأعمدة الاستعلام الداخلى
        CROSS APPLY (SELECT CAST(CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END AS decimal(18,6)) AS F) k
        OUTER APPLY (SELECT MAX(d.Process_Date) AS OpeningFrom FROM Detailes_Purshing_Import_Export d
                     WHERE d.Unit_item_Table_Id = u.id AND d.ProcessNameId = {opening} AND d.Engine_Posted = 1 AND d.Reversal_Of_Id IS NULL
                       AND d.Process_Date < {until}
                       AND NOT EXISTS (SELECT 1 FROM Detailes_Purshing_Import_Export r WHERE r.Reversal_Of_Id = d.Id)) o
        WHERE {useAsOf} = 1 AND x.Unit_item_Table_Id = u.id
          AND (x.Process_Date IS NULL OR x.Process_Date < {until})
          AND (o.OpeningFrom IS NULL OR x.Process_Date >= o.OpeningFrom)) comp
    CROSS APPLY (SELECT CAST(CASE WHEN {useAsOf} = 0
                              THEN ISNULL(u.Full_qua, 0) * CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END + ISNULL(u.D_Qua, 0)
                              ELSE ISNULL(comp.Bal, 0) END AS decimal(18,4)) AS Small) bs
    -- FIFO (بند 26): فواتير الشراء الأحدث أولاً حتى تغطية الرصيد (بالوحدة الكبرى)؛ قيم السطر الخارجى تُدخَل كأعمدة داخلية (CROSS APPLY)
    OUTER APPLY (
        SELECT SUM(t.Took * t.Pr) AS Covered_Value, SUM(t.Took) AS Covered, MAX(CASE WHEN t.Rn = 1 THEN t.Pr END) AS Last_Price
        FROM (
            SELECT p.Pr, p.Rn, CASE WHEN p.Cum <= b.B THEN p.Q WHEN p.Cum - p.Q < b.B THEN b.B - (p.Cum - p.Q) ELSE 0 END AS Took
            FROM (
                SELECT CAST(ISNULL(bd.Larg_Qnty, 0) + ISNULL(bd.Small_Qnty, 0) / k3.F AS decimal(18,6)) AS Q,
                       CAST(ISNULL(bd.Larg_Unit_Price, 0) AS decimal(18,6)) AS Pr,
                       ROW_NUMBER() OVER (ORDER BY bl.Invoice_Date DESC, bd.Id DESC) AS Rn,
                       SUM(CAST(ISNULL(bd.Larg_Qnty, 0) + ISNULL(bd.Small_Qnty, 0) / k3.F AS decimal(18,6)))
                           OVER (ORDER BY bl.Invoice_Date DESC, bd.Id DESC ROWS UNBOUNDED PRECEDING) AS Cum
                FROM Bills_Details bd
                INNER JOIN Bills_List bl ON bl.ID = bd.Invoice_Id
                CROSS APPLY (SELECT CAST(CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END AS decimal(18,6)) AS F) k3
                WHERE {price} = 4 AND bd.Unit_Item_Table_Id = u.id
                  AND (ISNULL(bd.Larg_Qnty, 0) > 0 OR ISNULL(bd.Small_Qnty, 0) > 0)
                  AND ({useAsOf} = 0 OR bl.Invoice_Date < {until})) p
            CROSS APPLY (SELECT CAST(bs.Small / CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END AS decimal(18,6)) AS B) b) t) fifo
    -- سعر المورد بنوع سعر الشراء (بند 26): سعر مخزن السطر أولاً ثم العام، والأحدث
    OUTER APPLY (SELECT TOP 1 sp.PriceUnit FROM Supplier_Price sp
                 WHERE {price} = 5 AND sp.Item_Id = u.Item_id AND ISNULL(sp.Price_PurchaseType, 0) = {ptype}
                   AND ({supplier} = 0 OR sp.Supplier_Id = {supplier})
                   AND (ISNULL(sp.Store_id, 0) = 0 OR sp.Store_id = u.Store_id)
                 ORDER BY CASE WHEN sp.Store_id = u.Store_id THEN 0 ELSE 1 END, sp.Id DESC) spp
    WHERE ISNULL(u.Store_Type, 4) = {f.StoreType}
      AND ISNULL(s.LookUp_StoreOrderTypesId, 4) = {f.StoreType}
      AND ({f.StoreId} = 0 OR u.Store_id = {f.StoreId})
      AND ({f.CategoryId} = 0 OR i.Item_Category_ID = {f.CategoryId})
      AND i.id IS NOT NULL   -- أسطر يتيمة بلا صنف لا تُعرض
      AND ISNULL(i.Active_flage, 1) = 1 AND ISNULL(u.active_flage, 1) = 1 AND ISNULL(i.Is_ItemWithoutBalance, 0) = 0
      AND ({text} = N'' OR i.Item_Name LIKE {like} OR u.Code = {text} OR i.code = {text})
), v AS (
    SELECT l.*, CAST(CASE WHEN l.Price_Large IS NULL THEN NULL ELSE l.Balance_Small / l.Factor * l.Price_Large END AS decimal(18,2)) AS Value
    FROM l WHERE {includeZero} = 1 OR l.Balance_Small <> 0
)
SELECT v.*, COUNT(*) OVER () AS Total_Rows,
       CAST(ISNULL(SUM(v.Value) OVER (), 0) AS decimal(18,2)) AS Total_Value,
       SUM(CASE WHEN v.Balance_Small < 0 THEN 1 ELSE 0 END) OVER () AS Negative_Rows
FROM v
ORDER BY v.Store_Name, v.Item_Name
OFFSET {skip} ROWS FETCH NEXT {take} ROWS ONLY").ToList());
        }

        // ============================== مطابقة الأرصدة (خطة 01 — المرحلة 3) ==============================

        public class ReconFilter
        {
            public int StoreType { get; set; } = 4;
            public long StoreId { get; set; }
            public long CategoryId { get; set; }
            public string Text { get; set; }
            public bool OnlyDifferences { get; set; } = true;
            public int Skip { get; set; }
            public int Take { get; set; } = 50;
        }

        public class ReconRow
        {
            public long Unit_Item_Id { get; set; }
            public string Store_Name { get; set; }
            public string Item_Name { get; set; }
            public string Code { get; set; }
            public string B_Unit { get; set; }
            public string S_Unit { get; set; }
            public decimal Factor { get; set; }
            public decimal Stored_Small { get; set; }
            public decimal Computed_Small { get; set; }
            public decimal Diff_Small { get; set; }
            public DateTime? Last_Movement { get; set; }
            public int Total_Rows { get; set; }
            public int Diff_Rows { get; set; }
        }

        /// <summary>
        /// لكل سطر مخزون: الرصيد المخزَّن (Unit_item) مقابل المحسوب من الدفتر (آخر أول مدة + الحركات)، والفرق وآخر حركة.
        /// Diff_Rows = عدد الأسطر ذات الفرق فى كل النتيجة (لا الصفحة فقط).
        /// </summary>
        public List<ReconRow> GetReconciliation(ReconFilter f)
        {
            string text = (f.Text ?? "").Trim();
            string like = "%" + text + "%";
            DateTime until = DateTime.Today.AddDays(1);
            int onlyDiff = f.OnlyDifferences ? 1 : 0;
            int skip = Math.Max(0, f.Skip), take = Math.Clamp(f.Take, 1, 500);
            int opening = StockMovementService.OpeningBalanceProcessId;
            return Run(opp => opp.db.Database.SqlQuery<ReconRow>($@"
WITH l AS (
    SELECT u.id AS Unit_Item_Id, s.Store_Name, ISNULL(i.Item_Name, N'') AS Item_Name, ISNULL(ISNULL(u.Code, i.code), N'') AS Code,
           ISNULL(un.B_Unit, N'') AS B_Unit, ISNULL(un.S_unit, N'') AS S_Unit,
           CAST(CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END AS decimal(18,6)) AS Factor,
           CAST(ISNULL(u.Full_qua, 0) * CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END + ISNULL(u.D_Qua, 0) AS decimal(18,4)) AS Stored_Small,
           CAST(ISNULL(comp.Bal, 0) AS decimal(18,4)) AS Computed_Small,
           comp.LastDate AS Last_Movement
    FROM Unit_item u
    INNER JOIN Store s ON s.Id = u.Store_id
    INNER JOIN Item i ON i.id = u.Item_id
    LEFT JOIN unit un ON un.id = u.unit_id
    OUTER APPLY (
        SELECT SUM(CASE WHEN x.Engine_Posted = 1 THEN ISNULL(x.Qty_Small, 0)
                        WHEN x.Transaction_Type = 1 THEN  (ISNULL(x.qua, 0) * k.F + ISNULL(x.Part_qua, 0))
                        WHEN x.Transaction_Type = 2 THEN -(ISNULL(x.qua, 0) * k.F + ISNULL(x.Part_qua, 0))
                        ELSE 0 END) AS Bal,
               MAX(x.Process_Date) AS LastDate
        FROM Detailes_Purshing_Import_Export x
        CROSS APPLY (SELECT CAST(CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END AS decimal(18,6)) AS F) k
        OUTER APPLY (SELECT MAX(d.Process_Date) AS OpeningFrom FROM Detailes_Purshing_Import_Export d
                     WHERE d.Unit_item_Table_Id = u.id AND d.ProcessNameId = {opening} AND d.Engine_Posted = 1 AND d.Reversal_Of_Id IS NULL
                       AND d.Process_Date < {until}
                       AND NOT EXISTS (SELECT 1 FROM Detailes_Purshing_Import_Export r WHERE r.Reversal_Of_Id = d.Id)) o
        WHERE x.Unit_item_Table_Id = u.id
          AND (x.Process_Date IS NULL OR x.Process_Date < {until})
          AND (o.OpeningFrom IS NULL OR x.Process_Date >= o.OpeningFrom)) comp
    WHERE ISNULL(u.Store_Type, 4) = {f.StoreType}
      AND ISNULL(s.LookUp_StoreOrderTypesId, 4) = {f.StoreType}
      AND ({f.StoreId} = 0 OR u.Store_id = {f.StoreId})
      AND ({f.CategoryId} = 0 OR i.Item_Category_ID = {f.CategoryId})
      AND ISNULL(u.active_flage, 1) = 1 AND ISNULL(i.Is_ItemWithoutBalance, 0) = 0
      AND ({text} = N'' OR i.Item_Name LIKE {like} OR u.Code = {text} OR i.code = {text})
), v AS (
    SELECT l.*, CAST(l.Stored_Small - l.Computed_Small AS decimal(18,4)) AS Diff_Small FROM l
)
SELECT v.*, COUNT(*) OVER () AS Total_Rows, SUM(CASE WHEN v.Diff_Small <> 0 THEN 1 ELSE 0 END) OVER () AS Diff_Rows
FROM v
WHERE {onlyDiff} = 0 OR v.Diff_Small <> 0
ORDER BY ABS(v.Diff_Small) DESC, v.Store_Name, v.Item_Name
OFFSET {skip} ROWS FETCH NEXT {take} ROWS ONLY").ToList());
        }

        /// <summary>يعتمد الرصيد المخزَّن للأسطر المختارة بصف تصحيح فى الدفتر لكل سطر (معاملة واحدة). يرجع عدد الأسطر المصحَّحة.</summary>
        public int AcceptStoredBalances(IReadOnlyCollection<long> unitItemIds, string reason)
        {
            if (unitItemIds == null || unitItemIds.Count == 0) return 0;
            var opp = _newOpp();
            opp.BeginTransaction();
            try
            {
                var engine = new StockMovementService(opp);
                int n = unitItemIds.Count(id => engine.PostLedgerCorrection(id, reason) > 0);
                opp.Commit();
                return n;
            }
            catch
            {
                opp.Rollback();
                throw;
            }
        }

        private T Run<T>(Func<operations, T> f) => f(_newOpp());
    }
}
