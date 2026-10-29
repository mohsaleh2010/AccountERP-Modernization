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

        public class CurrentOpening
        {
            public decimal TotalSmall { get; set; }
            public decimal ItemNo { get; set; }
            public decimal CostLarge { get; set; }
            public decimal Factor { get; set; }
            public bool Has { get; set; }
        }

        public record SaveError(long UnitItemId, string Message);

        public class SaveReport
        {
            public int Saved { get; set; }
            public int Unchanged { get; set; }
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

        /// <summary>كل أسطر الفلتر (بلا ترقيم) حتى حد أقصى؛ لتصدير قالب Excel.</summary>
        public List<OpeningRow> GetAllRows(OpeningFilter f, int max = 20000)
        {
            var all = new List<OpeningRow>();
            for (int skip = 0; all.Count < max; skip += 500)
            {
                var page = GetRows(new OpeningFilter { PeriodId = f.PeriodId, StoreId = f.StoreId, CategoryId = f.CategoryId, Text = f.Text, OnlyWithOpening = f.OnlyWithOpening, Skip = skip, Take = 500 });
                all.AddRange(page);
                if (page.Count < 500) break;
            }
            return all;
        }

        /// <summary>يحفظ الأسطر (كل سطر بمعاملته). السطر المرفوض يُسجَّل فى التقرير ولا يمنع غيره.</summary>
        public SaveReport Save(long periodId, IEnumerable<SaveLine> lines, ItemOpeningBalanceService.OpeningSource source)
        {
            var report = new SaveReport();
            foreach (var l in lines)
            {
                var opp = _newOpp();
                int carried = opp.db.Database.SqlQuery<int>($"SELECT COUNT(*) AS Value FROM Item_First_Period_Balance WHERE Year_Financial_Period_Id = {periodId} AND Unit_Item_Id = {l.UnitItemId} AND Source = {(int)ItemOpeningBalanceService.OpeningSource.CarriedForward}").AsEnumerable().First();
                if (carried > 0)
                {
                    report.Errors.Add(new SaveError(l.UnitItemId, "هذا السطر منقول من الفترة السابقة كلقطة؛ لا يُعدَّل هنا (رصيده فى الدفتر مستمر)."));
                    continue;
                }
                // سطر قيمه مطابقة لما هو محفوظ (أو صفر بلا صف) لا يُرحَّل: إعادة رفع قالب كما هو لا تُنتج عكساً وترحيلاً بلا فرق.
                var cur = opp.db.Database.SqlQuery<CurrentOpening>($@"
SELECT CAST(ISNULL(b.Larg_Qnty, 0) * k.F + ISNULL(b.Small_Qnty, 0) AS decimal(18,6)) AS TotalSmall, CAST(ISNULL(b.Item_No, 0) AS decimal(18,6)) AS ItemNo,
       CAST(ISNULL(b.Unit_Cost, 0) * k.F AS decimal(18,6)) AS CostLarge, CAST(k.F AS decimal(18,6)) AS Factor, CAST(CASE WHEN b.Id IS NULL THEN 0 ELSE 1 END AS bit) AS Has
FROM Unit_item u
CROSS APPLY (SELECT CAST(CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END AS decimal(18,6)) AS F) k
LEFT JOIN Item_First_Period_Balance b ON b.Unit_Item_Id = u.id AND b.Year_Financial_Period_Id = {periodId}
WHERE u.id = {l.UnitItemId}").AsEnumerable().FirstOrDefault();
                if (cur != null)
                {
                    decimal newTotal = l.QtyLarge * cur.Factor + l.QtySmall;
                    bool same = cur.Has
                        ? cur.TotalSmall == newTotal && cur.ItemNo == l.ItemNo && Math.Abs(cur.CostLarge - l.CostLarge) < 0.005m
                        : newTotal == 0;
                    if (same) { report.Unchanged++; continue; }
                }
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

        public class CarryRow
        {
            public long Unit_Item_Id { get; set; }
            public long Item_Id { get; set; }
            public long Store_Id { get; set; }
            public long Branch_Id { get; set; }
            public decimal Factor { get; set; }
            public decimal Bal { get; set; }
            public decimal Cost_Small { get; set; }
        }

        public record CarryReport(int Carried, int SkippedExisting, int SkippedNotPositive, string FromPeriodText);

        /// <summary>
        /// «نقل من الفترة السابقة» (قرار 2026-10-29: لقطة فقط بلا قيد دفتر). الدفتر يحمل الرصيد متصلاً أصلاً، فلا يُرحَّل قيد جديد (كان سيضاعف المخزَّن)؛
        /// يُكتب صف Item_First_Period_Balance بالمصدر «منقول» بقيمة رصيد نهاية الفترة السابقة المحسوب من الدفتر وبمتوسط التكلفة الحالى.
        /// لا يمس سطراً له صف فى الفترة، ولا يحمل رصيداً صفرياً أو سالباً. معاملة واحدة.
        /// </summary>
        public CarryReport CarryForward(long toPeriodId, long storeId, operations callerOpp = null)
        {
            // callerOpp: معاملة المستدعى (للاختبار الملغى)؛ بدونها تفتح الدالة معاملتها وتعتمدها.
            bool ownTransaction = callerOpp == null;
            var opp = callerOpp ?? _newOpp();
            var periods = opp.db.Database.SqlQuery<PeriodOption>($"SELECT Id, DateFrom, DateTo, IS_Finished_Posted FROM Financial_Period").ToList();
            var to = periods.FirstOrDefault(p => p.Id == toPeriodId);
            if (to?.DateFrom == null) throw new InvalidOperationException("الفترة المالية غير موجودة.");
            if (to.IS_Finished_Posted == 1) throw new InvalidOperationException("الفترة المالية مقفلة؛ لا يُنقل إليها رصيد.");
            var from = periods.Where(p => p.DateFrom != null && p.DateFrom < to.DateFrom).OrderByDescending(p => p.DateFrom).FirstOrDefault();
            if (from == null) throw new InvalidOperationException("لا توجد فترة سابقة لنقل رصيدها.");

            DateTime openingDate = to.DateFrom.Value.Date;
            DateTime until = openingDate;                       // أقل من بداية الفترة الجديدة = حتى نهاية السابقة
            int opening = StockMovementService.OpeningBalanceProcessId;
            long store = storeId;
            string fromText = $"{from.DateFrom:yyyy/MM/dd} — {from.DateTo:yyyy/MM/dd}";

            var rows = opp.db.Database.SqlQuery<CarryRow>($@"
SELECT u.id AS Unit_Item_Id, u.Item_id AS Item_Id, u.Store_id AS Store_Id, ISNULL(s.Branch_Id, 0) AS Branch_Id, k.F AS Factor,
       CAST(ISNULL(bal.Bal, 0) AS decimal(18,6)) AS Bal,
       CAST(COALESCE(bc.Avg_Cost, u.P_priceAvgCost / k.F, 0) AS decimal(18,6)) AS Cost_Small
FROM Unit_item u
INNER JOIN Store s ON s.Id = u.Store_id
INNER JOIN Item i ON i.id = u.Item_id
CROSS APPLY (SELECT CAST(CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END AS decimal(18,6)) AS F) k
OUTER APPLY (SELECT TOP 1 b.Avg_Cost FROM Item_Branch_Cost b WHERE b.Item_Id = u.Item_id AND b.Branch_Id = s.Branch_Id) bc
OUTER APPLY (
    SELECT SUM(CASE WHEN x.Engine_Posted = 1 THEN ISNULL(x.Qty_Small, 0)
                    WHEN x.Transaction_Type = 1 THEN  (ISNULL(x.qua, 0) * kk.F + ISNULL(x.Part_qua, 0))
                    WHEN x.Transaction_Type = 2 THEN -(ISNULL(x.qua, 0) * kk.F + ISNULL(x.Part_qua, 0))
                    ELSE 0 END) AS Bal
    FROM Detailes_Purshing_Import_Export x
    -- عامل الوحدة كعمود داخلى: SQL Server لا يقبل تجميع تعبير يخلط عموداً خارجياً بأعمدة الاستعلام الداخلى
    CROSS APPLY (SELECT CAST(CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END AS decimal(18,6)) AS F) kk
    OUTER APPLY (SELECT MAX(d.Process_Date) AS OpeningFrom FROM Detailes_Purshing_Import_Export d
                 WHERE d.Unit_item_Table_Id = u.id AND d.ProcessNameId = {opening} AND d.Engine_Posted = 1 AND d.Reversal_Of_Id IS NULL
                   AND d.Process_Date < {until}
                   AND NOT EXISTS (SELECT 1 FROM Detailes_Purshing_Import_Export r WHERE r.Reversal_Of_Id = d.Id)) o
    WHERE x.Unit_item_Table_Id = u.id AND (x.Process_Date IS NULL OR x.Process_Date < {until})
      AND (o.OpeningFrom IS NULL OR x.Process_Date >= o.OpeningFrom)) bal
WHERE ISNULL(i.Is_ItemOrService, 1) <> 2 AND ISNULL(i.Is_ItemWithoutBalance, 0) <> 1 AND ({store} = 0 OR u.Store_id = {store})
  AND NOT EXISTS (SELECT 1 FROM Item_First_Period_Balance e WHERE e.Unit_Item_Id = u.id AND e.Year_Financial_Period_Id = {toPeriodId})").ToList();

            int existing = opp.db.Database.SqlQuery<int>($@"
SELECT COUNT(*) AS Value FROM Item_First_Period_Balance e INNER JOIN Unit_item u ON u.id = e.Unit_Item_Id
WHERE e.Year_Financial_Period_Id = {toPeriodId} AND ({store} = 0 OR u.Store_id = {store})").AsEnumerable().First();

            var positive = rows.Where(r => r.Bal > 0).ToList();
            long userId = Common.User_Id;
            DateTime now = DateTime.Now;
            if (ownTransaction) opp.BeginTransaction();
            try
            {
                foreach (var r in positive)
                {
                    decimal larg = Math.Floor(r.Bal / r.Factor), small = r.Bal - larg * r.Factor;
                    decimal costLarge = Math.Round(r.Cost_Small * r.Factor, 2);
                    long id = CallSnowflakeIdGenerate.GenerateNewId();
                    int src = (int)ItemOpeningBalanceService.OpeningSource.CarriedForward;
                    opp.db.Database.ExecuteSqlInterpolated($@"
INSERT INTO Item_First_Period_Balance (Id, Item_Id, Unit_Item_Id, Larg_Qnty, Small_Qnty, Item_No, Balance_Date, Active,
    Year_Financial_Period_Id, Store_Id, Unit_Cost, Source, P_priceAvgCost, OperationDate, User_Id, Branch_Id, Insert_ServerDateTime)
VALUES ({id}, {r.Item_Id}, {r.Unit_Item_Id}, {larg}, {small}, {0m}, {openingDate}, 1,
    {toPeriodId}, {r.Store_Id}, {Dec6(r.Cost_Small)}, {src}, {costLarge}, {now.Date}, {userId}, {r.Branch_Id}, {now})");
                }
                if (ownTransaction) opp.Commit();
            }
            catch
            {
                if (ownTransaction) opp.Rollback();
                throw;
            }
            return new CarryReport(positive.Count, existing, rows.Count - positive.Count, fromText);
        }

        private static int _paramNo;

        private static Microsoft.Data.SqlClient.SqlParameter Dec6(decimal value) =>
            new Microsoft.Data.SqlClient.SqlParameter("@clDec" + System.Threading.Interlocked.Increment(ref _paramNo), System.Data.SqlDbType.Decimal)
            { Precision = 18, Scale = 6, Value = Math.Round(value, 6) };
    }
}
