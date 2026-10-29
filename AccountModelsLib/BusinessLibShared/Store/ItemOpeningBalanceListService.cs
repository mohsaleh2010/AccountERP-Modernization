using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib
{
    /// <summary>
    /// واجهة شاشة «رصيد أول المدة للأصناف» (خطة 02 — المرحلة 3، الشق الشاشى): قراءة الأسطر مع أول مدة الفترة والرصيد الجارى، وحفظ أسطر مُعدَّلة.
    /// الحفظ كله عبر ItemOpeningBalanceService.Post (يرفض الفترة المقفلة والرصيد غير المسجل فى الدفتر)، كل سطر فى معاملة مستقلة
    /// حتى لا يُسقط سطرٌ مرفوض بقية الأسطر.
    /// </summary>
    public class ItemOpeningBalanceListService
    {
        private readonly Func<operations> _newOpp;

        public ItemOpeningBalanceListService(Func<operations> newOpp = null) => _newOpp = newOpp ?? (() => new operations());

        public class PeriodOption
        {
            public long Id { get; set; }
            public DateTime? DateFrom { get; set; }
            public DateTime? DateTo { get; set; }
            public int? IS_Finished_Posted { get; set; }
        }

        public class OpeningFilter
        {
            public long PeriodId { get; set; }
            public long StoreId { get; set; }
            public long CategoryId { get; set; }
            public string Text { get; set; }
            public bool OnlyWithOpening { get; set; }
            public int Skip { get; set; }
            public int Take { get; set; } = 50;
        }

        public class OpeningRow
        {
            public long Unit_Item_Id { get; set; }
            public string Store_Name { get; set; }
            public string Item_Name { get; set; }
            public string Code { get; set; }
            public string Category_Name { get; set; }
            public string B_Unit { get; set; }
            public string S_Unit { get; set; }
            public decimal Factor { get; set; }
            public decimal Open_Large { get; set; }
            public decimal Open_Small { get; set; }
            public decimal Open_No { get; set; }
            public decimal Cost_Large { get; set; }
            public int Source { get; set; }
            public bool Has_Opening { get; set; }
            public decimal Current_Small { get; set; }
            public int Total_Rows { get; set; }
        }

        public class SaveLine
        {
            public long UnitItemId { get; set; }
            public decimal QtyLarge { get; set; }
            public decimal QtySmall { get; set; }
            public decimal ItemNo { get; set; }
            public decimal CostLarge { get; set; }
        }

        public record SaveError(long UnitItemId, string Message);

        public class SaveReport
        {
            public int Saved { get; set; }
            public List<SaveError> Errors { get; } = new();
        }

        public List<PeriodOption> GetPeriods() =>
            _newOpp().db.Database.SqlQuery<PeriodOption>($"SELECT Id, DateFrom, DateTo, IS_Finished_Posted FROM Financial_Period ORDER BY DateFrom DESC").ToList();

        public List<OpeningRow> GetRows(OpeningFilter f)
        {
            string like = "%" + (f.Text ?? "").Trim() + "%";
            int skip = Math.Max(0, f.Skip), take = Math.Clamp(f.Take, 1, 500);
            long period = f.PeriodId, store = f.StoreId, cat = f.CategoryId;
            int onlyOpening = f.OnlyWithOpening ? 1 : 0;
            return _newOpp().db.Database.SqlQuery<OpeningRow>($@"
SELECT u.id AS Unit_Item_Id, s.Store_Name, ISNULL(i.Item_Name, N'') AS Item_Name, ISNULL(ISNULL(u.Code, i.code), N'') AS Code,
       ISNULL(c.Category_Name, N'') AS Category_Name, ISNULL(un.B_Unit, N'') AS B_Unit, ISNULL(un.S_unit, N'') AS S_Unit,
       CAST(CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END AS decimal(18,6)) AS Factor,
       CAST(ISNULL(b.Larg_Qnty, 0) AS decimal(18,6)) AS Open_Large, CAST(ISNULL(b.Small_Qnty, 0) AS decimal(18,6)) AS Open_Small,
       CAST(ISNULL(b.Item_No, 0) AS decimal(18,6)) AS Open_No,
       CAST(ISNULL(b.Unit_Cost, 0) * CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END AS decimal(18,6)) AS Cost_Large,
       CAST(ISNULL(b.Source, 0) AS int) AS Source, CAST(CASE WHEN b.Id IS NULL THEN 0 ELSE 1 END AS bit) AS Has_Opening,
       CAST(ISNULL(u.Full_qua, 0) * CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END + ISNULL(u.D_Qua, 0) AS decimal(18,6)) AS Current_Small,
       COUNT(*) OVER () AS Total_Rows
FROM Unit_item u
INNER JOIN Store s ON s.Id = u.Store_id
INNER JOIN Item i ON i.id = u.Item_id
LEFT JOIN Item_Category c ON c.ID = i.Item_Category_ID
LEFT JOIN unit un ON un.id = u.unit_id
LEFT JOIN Item_First_Period_Balance b ON b.Unit_Item_Id = u.id AND b.Year_Financial_Period_Id = {period}
WHERE ISNULL(i.Is_ItemOrService, 1) <> 2 AND ISNULL(i.Is_ItemWithoutBalance, 0) <> 1
  AND ({store} = 0 OR u.Store_id = {store}) AND ({cat} = 0 OR i.Item_Category_ID = {cat})
  AND ({onlyOpening} = 0 OR b.Id IS NOT NULL)
  AND (i.Item_Name LIKE {like} OR u.Code LIKE {like} OR i.code LIKE {like})
ORDER BY s.Store_Name, i.Item_Name, u.id
OFFSET {skip} ROWS FETCH NEXT {take} ROWS ONLY").ToList();
        }

        /// <summary>يحفظ الأسطر (كل سطر بمعاملته). السطر المرفوض يُسجَّل فى التقرير ولا يمنع غيره.</summary>
        public SaveReport Save(long periodId, IEnumerable<SaveLine> lines, ItemOpeningBalanceService.OpeningSource source)
        {
            var report = new SaveReport();
            foreach (var l in lines)
            {
                var opp = _newOpp();
                opp.BeginTransaction();
                try
                {
                    new ItemOpeningBalanceService(opp).Post(periodId, l.UnitItemId, l.QtyLarge, l.QtySmall, l.ItemNo, l.CostLarge, source);
                    opp.Commit();
                    report.Saved++;
                }
                catch (Exception ex)
                {
                    opp.Rollback();
                    while (ex.InnerException != null) ex = ex.InnerException;
                    report.Errors.Add(new SaveError(l.UnitItemId, ex.Message));
                }
            }
            return report;
        }
    }
}
