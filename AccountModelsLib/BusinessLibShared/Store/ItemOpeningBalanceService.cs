using System;
using System.Collections.Generic;
using System.Linq;
using AccountModelsLib.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib
{
    /// <summary>
    /// رصيد أول المدة للأصناف (خطة 02 — المرحلة 3، الخدمة فقط). أول المدة = صف افتتاحى فى الدفتر عبر StockMovementService
    /// (ProcessNameId = 900، مؤرخ ببداية الفترة) + صف فى Item_First_Period_Balance بالفترة والمخزن وتكلفة الوحدة.
    /// التصحيح = عكس الصف الافتتاحى السابق ثم ترحيل الجديد (دورة م-01-8)، فيتغير الرصيد الجارى بالفرق فقط.
    /// تعمل داخل معاملة المستدعى (operations بعد BeginTransaction).
    /// </summary>
    public class ItemOpeningBalanceService
    {
        /// <summary>مصدر الرصيد فى Item_First_Period_Balance.Source.</summary>
        public enum OpeningSource { Legacy = 0, Manual = 1, Import = 2, CarriedForward = 3 }

        public record OpeningResult(long UnitItemId, long PeriodId, decimal QtySmall, decimal UnitCostSmall, decimal BalanceSmallAfter, bool Corrected);

        private readonly operations opp;
        private readonly StockMovementService engine;

        public ItemOpeningBalanceService(operations opp_)
        {
            opp = opp_ ?? throw new ArgumentNullException(nameof(opp_));
            engine = new StockMovementService(opp);
        }

        /// <summary>
        /// يرحّل (أو يصحّح) رصيد أول مدة لسطر مخزون فى فترة. الكمية بالوحدتين، والتكلفة للوحدة الكبرى.
        /// يرفض: فترة غير موجودة أو مقفلة، أو سطر مخزون له رصيد قديم غير مسجل فى الدفتر (المخزَّن ≠ المحسوب) لأن الترحيل عندها يضاعف الرصيد
        /// — يُعالج أولاً بالمطابقة (خطة 01 §2.4).
        /// </summary>
        public OpeningResult Post(long periodId, long unitItemId, decimal qtyLarge, decimal qtySmall, decimal itemNo, decimal unitCostLarge, OpeningSource source)
        {
            if (qtyLarge < 0 || qtySmall < 0 || itemNo < 0 || unitCostLarge < 0)
                throw new InvalidOperationException("الكميات والتكلفة لا تقبل قيماً سالبة.");

            var period = opp.db.Database.SqlQuery<PeriodRow>($"SELECT Id, DateFrom, IS_Finished_Posted FROM Financial_Period WHERE Id = {periodId}").AsEnumerable().FirstOrDefault();
            if (period == null || period.DateFrom == null) throw new InvalidOperationException("الفترة المالية غير موجودة.");
            if (period.IS_Finished_Posted == 1) throw new InvalidOperationException("الفترة المالية مقفلة؛ لا يُعدَّل رصيد أول المدة فيها.");
            DateTime openingDate = period.DateFrom.Value.Date;

            var um = opp.db.Unit_item.AsNoTracking().Where(u => u.id == unitItemId)
                .Select(u => new { u.id, u.Item_Id, u.Store_id, u.Small_unit, u.Branch_Id }).FirstOrDefault();
            if (um == null) throw new InvalidOperationException("سطر المخزون غير موجود.");

            decimal stored = engine.GetStoredBalanceSmall(unitItemId);
            decimal computed = engine.GetComputedBalanceSmall(unitItemId);
            if (stored != computed)
                throw new InvalidOperationException($"لسطر المخزون رصيد غير مسجل فى الدفتر (المخزَّن {stored} والمحسوب {computed} بالوحدة الصغرى). يُعالج بالمطابقة أولاً.");

            var existing = opp.db.Database.SqlQuery<StockMovementService.DpieRow>($@"
SELECT d.Id, d.Unit_item_Table_Id, d.Qty_Small, d.Unit_Cost, d.Item_No, d.Detailes_Purshing_Id
FROM Detailes_Purshing_Import_Export d
WHERE d.Unit_item_Table_Id = {unitItemId} AND d.ProcessNameId = {StockMovementService.OpeningBalanceProcessId}
  AND d.Engine_Posted = 1 AND d.Reversal_Of_Id IS NULL AND d.Process_Date = {openingDate}
  AND NOT EXISTS (SELECT 1 FROM Detailes_Purshing_Import_Export r WHERE r.Reversal_Of_Id = d.Id)").ToList();

            var doc = new StockMovementService.MovementDocument(0, StockMovementService.OpeningBalanceProcessId, openingDate);
            bool corrected = existing.Count > 0;

            decimal factor = (um.Small_unit ?? 0) <= 0 ? 1 : um.Small_unit.Value;
            decimal totalSmall = qtyLarge * factor + qtySmall;
            decimal unitCostSmall = unitCostLarge / factor;

            // الترحيل الجديد أولاً ثم عكس القديم: لو سبق العكسُ والكميةُ بيعت جزئياً لنزل الرصيد مؤقتاً تحت الصفر ورُفض تصحيح صحيح.
            if (totalSmall > 0)
            {
                engine.Post(doc, new[]
                {
                    new StockMovementService.MovementLine(unitItemId, +1, qtyLarge, qtySmall, itemNo, unitCostLarge, UpdatesAvgCost: true, Notes: "رصيد أول المدة")
                });
            }
            if (corrected)
                engine.ReverseRows(existing, doc, "تصحيح رصيد أول المدة");
            decimal balanceAfter = engine.GetStoredBalanceSmall(unitItemId);

            UpsertOpeningRow(periodId, um.id, um.Item_Id ?? 0, um.Store_id ?? 0, um.Branch_Id ?? Common.Branch_Id, openingDate, totalSmall, factor, itemNo, unitCostSmall, source);
            return new OpeningResult(unitItemId, periodId, totalSmall, unitCostSmall, balanceAfter, corrected);
        }

        /// <summary>صف Item_First_Period_Balance للفترة وسطر المخزون (إنشاء أو تحديث). الأعمدة الجديدة غير موجودة فى الموديل القديم فتُكتب بـ SQL مُمرَّر بالمعاملات.</summary>
        private void UpsertOpeningRow(long periodId, long unitItemId, long itemId, long storeId, long branchId, DateTime openingDate,
                                      decimal totalSmall, decimal factor, decimal itemNo, decimal unitCostSmall, OpeningSource source)
        {
            decimal larg = Math.Floor(totalSmall / factor);
            decimal small = totalSmall - larg * factor;
            decimal costLarge = Math.Round(unitCostSmall * factor, 2);
            long userId = Common.User_Id;
            DateTime now = DateTime.Now;
            int src = (int)source;

            int updated = opp.db.Database.ExecuteSqlInterpolated($@"
UPDATE Item_First_Period_Balance SET Larg_Qnty = {larg}, Small_Qnty = {small}, Item_No = {itemNo}, Balance_Date = {openingDate}, Active = 1,
    Store_Id = {storeId}, Unit_Cost = {Dec6(unitCostSmall)}, Source = {src}, P_priceAvgCost = {costLarge},
    Update_ServerDateTime = {now}, EditOperationDate = {now}, Edit_User_Id = {userId}
WHERE Year_Financial_Period_Id = {periodId} AND Unit_Item_Id = {unitItemId}");
            if (updated > 0) return;

            long id = CallSnowflakeIdGenerate.GenerateNewId();
            opp.db.Database.ExecuteSqlInterpolated($@"
INSERT INTO Item_First_Period_Balance (Id, Item_Id, Unit_Item_Id, Larg_Qnty, Small_Qnty, Item_No, Balance_Date, Active,
    Year_Financial_Period_Id, Store_Id, Unit_Cost, Source, P_priceAvgCost, OperationDate, User_Id, Branch_Id, Insert_ServerDateTime)
VALUES ({id}, {itemId}, {unitItemId}, {larg}, {small}, {itemNo}, {openingDate}, 1,
    {periodId}, {storeId}, {Dec6(unitCostSmall)}, {src}, {costLarge}, {now.Date}, {userId}, {branchId}, {now})");
        }

        private static int _paramNo;

        private static Microsoft.Data.SqlClient.SqlParameter Dec6(decimal value) =>
            new Microsoft.Data.SqlClient.SqlParameter("@iobDec" + System.Threading.Interlocked.Increment(ref _paramNo), System.Data.SqlDbType.Decimal)
            { Precision = 18, Scale = 6, Value = Math.Round(value, 6) };

        /// <summary>صف فترة مالية (عام لأن EF يملؤه من SQL خام).</summary>
        public class PeriodRow
        {
            public long Id { get; set; }
            public DateTime? DateFrom { get; set; }
            public int? IS_Finished_Posted { get; set; }
        }
    }
}
