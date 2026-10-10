using System;
using System.Collections.Generic;
using System.Linq;
using AccountModelsLib.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib
{
    /// <summary>
    /// محرك حركة المخزون الموحَّد (خطة 01 — المرحلة 1). كل حركة فى الشاشات الجديدة (Razor، المندوبين، الـ API) تمر به فقط.
    /// - الرصيد الجارى فى Unit_item (Full_qua/D_Qua/Item_No) والدفتر فى Detailes_Purshing_Import_Export (DPIE).
    /// - متوسط التكلفة لكل (صنف × فرع) فى Item_Branch_Cost (بالوحدة الصغرى)، ويُنسخ إلى Unit_item.P_priceAvgCost (بالوحدة الكبرى كما يقرؤه الكود القديم).
    /// - دورة المستند: مسودة ← ترحيل ← عكس (م-01-8)؛ المرحَّل لا يُعدَّل ولا يُحذف.
    /// كل الدوال تعمل داخل معاملة المستدعى (operations بعد BeginTransaction) ولا تفتح أو تُنهى معاملة بنفسها.
    /// الأعمدة الجديدة على DPIE و Order_Purshing (غير الموجودة فى الموديلات القديمة) تُكتب بـ SQL مُمرَّر بالمعاملات (ExecuteSqlInterpolated).
    /// </summary>
    public class StockMovementService
    {
        /// <summary>حالة المستند فى Order_Purshing.Doc_Status.</summary>
        public enum DocStatus { Draft = 0, Posted = 1, Reversed = 2, CancelledDraft = 3 }

        /// <summary>
        /// سطر حركة. Direction: +1 وارد، −1 منصرف. الكميات بالوحدتين كما فى الشاشات.
        /// UnitCostLarge: تكلفة الوحدة الكبرى للوارد بسعر (شراء، مرتجع بتكلفة السطر الأصلى، إنتاج، أول مدة)؛ null = بالمتوسط الحالى.
        /// UpdatesAvgCost: هل يحدّث الوارد المتوسط (من إعداد نوع الإذن). المنصرف لا يغيّر المتوسط أبداً.
        /// </summary>
        public record MovementLine(
            long UnitItemId,
            int Direction,
            decimal QtyLarge,
            decimal QtySmall,
            decimal ItemNo = 0,
            decimal? UnitCostLarge = null,
            bool UpdatesAvgCost = false,
            bool IsPurchase = false,
            long DetailId = 0,
            string Notes = null);

        /// <summary>رأس الحركة: المستند (إذن مخزن أو فاتورة) ونوعه وتاريخه.</summary>
        public record MovementDocument(
            long OrderId,
            int ProcessNameId,
            DateTime ProcessDate,
            long InvoiceId = 0,
            long InvoiceTypeId = 0,
            int TypeFrom = 0,
            long IdFrom = 0,
            int TypeTo = 0,
            long IdTo = 0);

        /// <summary>نتيجة ترحيل سطر.</summary>
        public record PostedLine(long DpieId, long UnitItemId, decimal QtySmallSigned, decimal UnitCostSmall, decimal AvgBefore, decimal AvgAfter, decimal BalanceSmallAfter);

        private readonly operations opp;

        /// <summary>يجب أن يكون opp داخل معاملة (BeginTransaction) قبل أى ترحيل.</summary>
        public StockMovementService(operations opp_)
        {
            opp = opp_ ?? throw new ArgumentNullException(nameof(opp_));
        }

        // ============================== الترحيل ==============================

        /// <summary>
        /// يرحّل أسطر مستند: يحدّث الرصيد والمتوسط ويكتب صف DPIE لكل سطر. يرمى استثناءً عند أى مخالفة (الرصيد، الخدمة، المخزن)،
        /// والمستدعى يتراجع عن المعاملة كلها.
        /// </summary>
        public List<PostedLine> Post(MovementDocument doc, IEnumerable<MovementLine> lines)
        {
            if (doc == null) throw new ArgumentNullException(nameof(doc));
            var result = new List<PostedLine>();
            foreach (var line in lines ?? Enumerable.Empty<MovementLine>())
            {
                var posted = PostLine(doc, line, reversalOfDpieId: null);
                if (posted != null) result.Add(posted);
            }
            if (doc.OrderId > 0)
            {
                long userId = Common.User_Id;
                DateTime now = DateTime.Now;
                opp.db.Database.ExecuteSqlInterpolated($@"UPDATE Order_Purshing SET Doc_Status = {(int)DocStatus.Posted}, Posted_Date = {now}, Posted_By = {userId} WHERE Id = {doc.OrderId}");
            }
            return result;
        }

        /// <summary>
        /// يعكس كل حركات المحرك لمستند أصلى بمستند عكسى مرتبط (رأسه ينشئه المستدعى: reversalDoc.OrderId).
        /// الوارد الأصلى يُعكس بإخراج نفس الكمية بنفس تكلفته ويُخرج أثره من المتوسط؛ والمنصرف الأصلى يُعكس بإدخال نفس الكمية بتكلفته الأصلية.
        /// </summary>
        public List<PostedLine> Reverse(long originalOrderId, MovementDocument reversalDoc, string reason)
        {
            if (originalOrderId <= 0) throw new ArgumentException("رقم المستند الأصلى غير صحيح.");
            if (string.IsNullOrWhiteSpace(reason)) throw new InvalidOperationException("سبب العكس مطلوب.");

            int status = opp.db.Database.SqlQuery<int>($"SELECT ISNULL(Doc_Status, 1) AS Value FROM Order_Purshing WHERE Id = {originalOrderId}").AsEnumerable().FirstOrDefault();
            if (status != (int)DocStatus.Posted)
                throw new InvalidOperationException("لا يمكن عكس مستند غير مرحَّل أو معكوس من قبل.");

            var rows = opp.db.Database.SqlQuery<DpieRow>($@"
SELECT Id, Unit_item_Table_Id, Qty_Small, Unit_Cost, Item_No, Detailes_Purshing_Id
FROM Detailes_Purshing_Import_Export
WHERE OrderTable_id = {originalOrderId} AND Engine_Posted = 1 AND Reversal_Of_Id IS NULL").ToList();
            if (rows.Count == 0)
                throw new InvalidOperationException("لا توجد حركات مرحَّلة بالمحرك لهذا المستند (المستندات القديمة تُصحَّح بإذن تسوية).");

            var result = ReverseRows(rows, reversalDoc, reason);

            long userId = Common.User_Id;
            DateTime now = DateTime.Now;
            opp.db.Database.ExecuteSqlInterpolated($@"UPDATE Order_Purshing SET Doc_Status = {(int)DocStatus.Reversed}, Reversed_By_Id = {reversalDoc.OrderId}, Reverse_Reason = {reason} WHERE Id = {originalOrderId}");
            opp.db.Database.ExecuteSqlInterpolated($@"UPDATE Order_Purshing SET Doc_Status = {(int)DocStatus.Posted}, Reversal_Of_Id = {originalOrderId}, Reverse_Reason = {reason}, Posted_Date = {now}, Posted_By = {userId} WHERE Id = {reversalDoc.OrderId}");
            return result;
        }

        /// <summary>
        /// يعكس صفوف دفتر محددة (مرحَّلة بالمحرك) بصفوف عكسية مرتبطة بها، داخل معاملة المستدعى. يستعمله عكس المستند وتصحيح رصيد أول المدة.
        /// الوارد الأصلى يُعكس بإخراج نفس الكمية بنفس تكلفته ويُخرج أثره من المتوسط؛ والمنصرف الأصلى يُعكس بإدخال نفس الكمية بتكلفته الأصلية.
        /// </summary>
        public List<PostedLine> ReverseRows(IEnumerable<DpieRow> rows, MovementDocument reversalDoc, string reason)
        {
            var result = new List<PostedLine>();
            foreach (var r in rows)
            {
                decimal signed = r.Qty_Small ?? 0;
                if (signed == 0) continue;
                var factor = GetFactor(r.Unit_item_Table_Id);
                var line = new MovementLine(
                    UnitItemId: r.Unit_item_Table_Id,
                    Direction: signed > 0 ? -1 : +1,
                    QtyLarge: 0,
                    QtySmall: Math.Abs(signed),
                    ItemNo: r.Item_No ?? 0,
                    UnitCostLarge: (r.Unit_Cost ?? 0) * factor,
                    UpdatesAvgCost: true,
                    DetailId: r.Detailes_Purshing_Id ?? 0,
                    Notes: reason);
                var posted = PostLine(reversalDoc, line, reversalOfDpieId: r.Id);
                if (posted != null) result.Add(posted);
            }
            return result;
        }

        // ============================== الرصيد المحسوب ==============================

        /// <summary>رمز عملية «رصيد أول المدة» فى DPIE.ProcessNameId (خطة 02). صفه مؤرخ ببداية الفترة.</summary>
        public const int OpeningBalanceProcessId = 900;

        /// <summary>
        /// الرصيد المحسوب من الدفتر بالوحدة الصغرى حتى تاريخ (شاملاً) = آخر رصيد أول مدة مرحَّل بالمحرك فى فترة تبدأ قبل التاريخ
        /// + كل الحركات من بداية تلك الفترة. بلا رصيد أول مدة: كل الحركات من البداية. صفوف المحرك تُقرأ من Qty_Small،
        /// والصفوف القديمة من qua/Part_qua وإشارة Transaction_Type. (أول المدة المعكوس ثم المعاد ترحيله بنفس التاريخ يُجمع صافياً.)
        /// </summary>
        public decimal GetComputedBalanceSmall(long unitItemId, DateTime? asOf = null)
        {
            var factor = GetFactor(unitItemId);
            DateTime until = (asOf ?? DateTime.MaxValue.AddDays(-1)).Date.AddDays(1);
            decimal total = opp.db.Database.SqlQuery<decimal>($@"
SELECT ISNULL(SUM(CASE
         WHEN x.Engine_Posted = 1 THEN ISNULL(x.Qty_Small, 0)
         WHEN x.Transaction_Type = 1 THEN  (ISNULL(x.qua, 0) * {factor} + ISNULL(x.Part_qua, 0))
         WHEN x.Transaction_Type = 2 THEN -(ISNULL(x.qua, 0) * {factor} + ISNULL(x.Part_qua, 0))
         ELSE 0 END), 0) AS Value
FROM Detailes_Purshing_Import_Export x
CROSS APPLY (SELECT MAX(d.Process_Date) AS OpeningFrom
             FROM Detailes_Purshing_Import_Export d
             WHERE d.Unit_item_Table_Id = {unitItemId} AND d.ProcessNameId = {OpeningBalanceProcessId} AND d.Engine_Posted = 1
               AND d.Reversal_Of_Id IS NULL AND d.Process_Date < {until}
               AND NOT EXISTS (SELECT 1 FROM Detailes_Purshing_Import_Export r WHERE r.Reversal_Of_Id = d.Id)) o
WHERE x.Unit_item_Table_Id = {unitItemId}
  AND (x.Process_Date IS NULL OR x.Process_Date < {until})
  AND (o.OpeningFrom IS NULL OR x.Process_Date >= o.OpeningFrom)").AsEnumerable().FirstOrDefault();
            return total;
        }

        /// <summary>الرصيد المخزَّن فى Unit_item بالوحدة الصغرى.</summary>
        public decimal GetStoredBalanceSmall(long unitItemId)
        {
            var um = opp.db.Unit_item.AsNoTracking().Where(u => u.id == unitItemId).Select(u => new { u.Full_qua, u.D_Qua, u.Small_unit }).FirstOrDefault();
            if (um == null) throw new InvalidOperationException("سطر المخزون غير موجود.");
            decimal f = NormalizeFactor(um.Small_unit);
            return (um.Full_qua ?? 0) * f + (um.D_Qua ?? 0);
        }

        // ============================== الداخلى ==============================

        private PostedLine PostLine(MovementDocument doc, MovementLine line, long? reversalOfDpieId)
        {
            if (line.Direction != 1 && line.Direction != -1) throw new ArgumentException("اتجاه الحركة يجب أن يكون +1 أو −1.");

            // 1) قفل سطر المخزون
            var um = opp.db.Unit_item
                .FromSqlInterpolated($"SELECT * FROM Unit_item WITH (UPDLOCK, ROWLOCK) WHERE id = {line.UnitItemId}")
                .AsNoTracking().FirstOrDefault();
            if (um == null) throw new InvalidOperationException($"سطر المخزون {line.UnitItemId} غير موجود.");

            var item = opp.db.Item.AsNoTracking().Where(i => i.Id == um.Item_Id).Select(i => new { i.Id, i.Is_ItemOrService, i.Is_ItemWithoutBalance }).FirstOrDefault();
            if (item == null) throw new InvalidOperationException("الصنف غير موجود.");
            if (item.Is_ItemOrService == 2 || item.Is_ItemWithoutBalance == 1) return null;   // خدمة / بلا رصيد: لا حركة مخزن

            var store = opp.db.Stores.AsNoTracking().Where(s => s.Id == um.Store_id).Select(s => new { s.Id, s.Branch_Id, s.Active }).FirstOrDefault();
            if (store == null) throw new InvalidOperationException("المخزن غير موجود.");
            if (store.Active == 0) throw new InvalidOperationException("المخزن غير نشط.");
            long branchId = store.Branch_Id ?? Common.Branch_Id;

            // 2) الكميات بالوحدة الصغرى
            decimal factor = NormalizeFactor(um.Small_unit);
            decimal qtySmall = line.QtyLarge * factor + line.QtySmall;
            if (qtySmall <= 0) throw new InvalidOperationException("الكمية يجب أن تكون أكبر من صفر.");
            decimal balanceBefore = (um.Full_qua ?? 0) * factor + (um.D_Qua ?? 0);
            decimal signed = qtySmall * line.Direction;
            decimal balanceAfter = balanceBefore + signed;
            if (balanceAfter < 0)   // م-01-5: الرصيد السالب ممنوع (إعداد السماح لكل مخزن يُضاف مع شاشة المخازن)
                throw new InvalidOperationException($"الرصيد غير كافٍ: المتاح {balanceBefore / factor:0.##} والمطلوب {qtySmall / factor:0.##} (بالوحدة الكبرى).");

            // 3) متوسط الفرع
            var cost = LockOrCreateBranchCost(item.Id, branchId);
            decimal avgBefore = cost.Avg_Cost;
            decimal unitCostSmall;
            decimal avgAfter = avgBefore;
            decimal branchQtyAfter = cost.Qty_Small + signed;

            if (line.Direction > 0)
            {
                unitCostSmall = line.UnitCostLarge.HasValue ? line.UnitCostLarge.Value / factor : avgBefore;
                if (line.UpdatesAvgCost && line.UnitCostLarge.HasValue)
                {
                    avgAfter = cost.Qty_Small <= 0
                        ? unitCostSmall
                        : (cost.Qty_Small * avgBefore + qtySmall * unitCostSmall) / (cost.Qty_Small + qtySmall);
                }
            }
            else
            {
                // المنصرف بالمتوسط الحالى، إلا عكس وارد: يخرج بتكلفته الأصلية ويُزال أثره من المتوسط
                bool reversingInbound = reversalOfDpieId.HasValue && line.UnitCostLarge.HasValue;
                unitCostSmall = reversingInbound ? line.UnitCostLarge.Value / factor : avgBefore;
                if (reversingInbound && branchQtyAfter > 0)
                    avgAfter = (cost.Qty_Small * avgBefore - qtySmall * unitCostSmall) / branchQtyAfter;
                if (avgAfter < 0) avgAfter = avgBefore;
            }

            // 4) تحديث سطر المخزون (توحيد الكبرى/الصغرى)
            decimal newFull = Math.Floor(balanceAfter / factor);
            decimal newD = balanceAfter - newFull * factor;
            decimal newItemNo = (um.Item_No ?? 0) + line.Direction * line.ItemNo;
            DateTime now = DateTime.Now;
            long userId = Common.User_Id;
            opp.db.Database.ExecuteSqlInterpolated($@"UPDATE Unit_item SET Full_qua = {newFull}, D_Qua = {newD}, Item_No = {newItemNo},
    Update_ServerDateTime = {now}, EditOperationDate = {now}, Edit_User_Id = {userId} WHERE id = {um.id}");

            // 5) صف الدفتر (الأعمدة القديمة عبر الموديل، والجديدة بعدها بنفس المعاملة)
            decimal largeOut = Math.Floor(qtySmall / factor);
            decimal smallOut = qtySmall - largeOut * factor;
            var dpie = new Detailes_Purshing_Import_Export
            {
                Id = CallSnowflakeIdGenerate.GenerateNewId(),
                Detailes_Purshing_Id = line.DetailId,
                OrderTable_id = doc.OrderId,
                Invoice_Id = doc.InvoiceId,
                Invoice_Type_Id = doc.InvoiceTypeId,
                Unit_item_Table_Id = um.id,
                qua = largeOut,
                Part_qua = smallOut,
                Item_No = line.ItemNo,
                Balance_Larg_Qnty = newFull,
                Balance_Small_Qnty = newD,
                Balance_Item_No = newItemNo,
                Transaction_Type = line.Direction > 0 ? 1 : 2,
                Process_Date = doc.ProcessDate,
                ProcessNameId = doc.ProcessNameId,
                Type_From = doc.TypeFrom,
                Id_From = doc.IdFrom,
                Type_To = doc.TypeTo,
                Id_To = doc.IdTo,
                StoreId = um.Store_id,
                Notes = line.Notes,
                Quantity = qtySmall / factor,
                Quantity_Balance = balanceAfter / factor,
                PriceUnit = Math.Round(unitCostSmall * factor, 2),
                P_priceAvgCost = Math.Round(avgAfter * factor, 2),
                Branch_Id = branchId,
                User_Id = userId,
                OperationDate = now.Date,
                Insert_ServerDateTime = now,
            };
            opp.db.Detailes_Purshing_Import_Exports.Add(dpie);
            opp.db.SaveChanges();
            opp.db.Entry(dpie).State = EntityState.Detached;

            decimal totalValue = Math.Round(unitCostSmall * qtySmall, 2);
            opp.db.Database.ExecuteSqlInterpolated($@"UPDATE Detailes_Purshing_Import_Export SET Unit_Cost = {Dec(unitCostSmall)}, Qty_Small = {Dec(signed, 4)}, Total_Value = {Dec(totalValue, 2)},
    Avg_Cost_Before = {Dec(avgBefore)}, Avg_Cost_After = {Dec(avgAfter)}, Reversal_Of_Id = {reversalOfDpieId}, Engine_Posted = 1 WHERE Id = {dpie.Id}");

            // 6) متوسط الفرع + نسخه لكل صفوف الصنف فى مخازن الفرع (بالوحدة الكبرى لكل صف)
            decimal? lastPurchase = line.Direction > 0 && line.IsPurchase && line.UnitCostLarge.HasValue ? unitCostSmall : cost.Last_Purchase_Price;
            opp.db.Database.ExecuteSqlInterpolated($@"UPDATE Item_Branch_Cost SET Avg_Cost = {Dec(avgAfter)}, Qty_Small = {Dec(branchQtyAfter, 4)}, Last_Purchase_Price = {Dec(lastPurchase)},
    Last_Movement_Date = {doc.ProcessDate}, Update_ServerDateTime = {now}, EditOperationDate = {now}, Edit_User_Id = {userId} WHERE Id = {cost.Id}");
            if (avgAfter != avgBefore)
            {
                opp.db.Database.ExecuteSqlInterpolated($@"UPDATE u SET u.P_priceAvgCost = ROUND({Dec(avgAfter)} * CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END, 2)
FROM Unit_item u INNER JOIN Store s ON s.id = u.Store_id
WHERE u.Item_id = {item.Id} AND s.Branch_Id = {branchId}");
            }
            else
            {
                // المتوسط لم يتغير لكن السطر نفسه قد يكون جديداً (سطر مستلم تحويل يُنشأ برصيد صفر وتكلفة صفر): يأخذ متوسط فرعه.
                opp.db.Database.ExecuteSqlInterpolated($@"UPDATE Unit_item SET P_priceAvgCost = ROUND({Dec(avgAfter)} * CASE WHEN ISNULL(Small_unit, 0) <= 0 THEN 1 ELSE Small_unit END, 2)
WHERE id = {um.id} AND ISNULL(P_priceAvgCost, 0) <> ROUND({Dec(avgAfter)} * CASE WHEN ISNULL(Small_unit, 0) <= 0 THEN 1 ELSE Small_unit END, 2)");
            }

            return new PostedLine(dpie.Id, um.id, signed, unitCostSmall, avgBefore, avgAfter, balanceAfter);
        }

        /// <summary>
        /// يقفل صف متوسط (صنف × فرع) أو يُنشئه. عند الإنشاء: الكمية = مجموع أرصدة الصنف فى مخازن الفرع، والمتوسط = المتوسط المرجَّح
        /// لـ P_priceAvgCost الحالية (بالوحدة الصغرى) — هذا هو ترحيل المتوسط القديم لكل مخزن إلى مستوى الفرع.
        /// </summary>
        private Item_Branch_Cost LockOrCreateBranchCost(long itemId, long branchId)
        {
            var cost = opp.db.Item_Branch_Costs
                .FromSqlInterpolated($"SELECT * FROM Item_Branch_Cost WITH (UPDLOCK, HOLDLOCK) WHERE Item_Id = {itemId} AND Branch_Id = {branchId}")
                .AsNoTracking().FirstOrDefault();
            if (cost != null) return cost;

            var rows = (from u in opp.db.Unit_item.AsNoTracking()
                        join s in opp.db.Stores.AsNoTracking() on u.Store_id equals s.Id
                        where u.Item_Id == itemId && s.Branch_Id == branchId
                        select new { u.Full_qua, u.D_Qua, u.Small_unit, u.P_priceAvgCost }).ToList();
            decimal qty = 0, value = 0;
            foreach (var r in rows)
            {
                decimal f = NormalizeFactor(r.Small_unit);
                decimal q = (r.Full_qua ?? 0) * f + (r.D_Qua ?? 0);
                decimal avgSmall = (r.P_priceAvgCost ?? 0) / f;
                if (q > 0) { qty += q; value += q * avgSmall; }
            }
            decimal seedAvg = qty > 0 ? value / qty : rows.Select(r => (r.P_priceAvgCost ?? 0) / NormalizeFactor(r.Small_unit)).DefaultIfEmpty(0).Max();

            DateTime now = DateTime.Now;
            cost = new Item_Branch_Cost
            {
                Id = CallSnowflakeIdGenerate.GenerateNewId(),
                Item_Id = itemId,
                Branch_Id = branchId,
                Avg_Cost = seedAvg,
                Qty_Small = qty,
                OperationDate = now.Date,
                User_Id = Common.User_Id,
                Insert_ServerDateTime = now,
            };
            opp.db.Item_Branch_Costs.Add(cost);
            opp.db.SaveChanges();
            opp.db.Entry(cost).State = EntityState.Detached;
            return cost;
        }

        private decimal GetFactor(long unitItemId)
        {
            var f = opp.db.Unit_item.AsNoTracking().Where(u => u.id == unitItemId).Select(u => u.Small_unit).FirstOrDefault();
            return NormalizeFactor(f);
        }

        private static decimal NormalizeFactor(decimal? smallUnit) => (smallUnit ?? 0) <= 0 ? 1 : smallUnit.Value;

        private static int _paramNo;

        /// <summary>
        /// معامل decimal بدقة صريحة: EF يرسل قيم SQL الخام بنوع decimal(18,2) الافتراضى فيضيع كسر المتوسط (ظهر فى الاختبار: 11.612903 خُزِّن 11.61).
        /// </summary>
        private static Microsoft.Data.SqlClient.SqlParameter Dec(decimal? value, byte scale = 6) =>
            new Microsoft.Data.SqlClient.SqlParameter("@smsDec" + System.Threading.Interlocked.Increment(ref _paramNo), System.Data.SqlDbType.Decimal)
            { Precision = 18, Scale = scale, Value = value.HasValue ? Math.Round(value.Value, scale) : DBNull.Value };

        /// <summary>صف دفتر لازم للعكس (عام لأن EF يملؤه من SQL خام).</summary>
        public class DpieRow
        {
            public long Id { get; set; }
            public long Unit_item_Table_Id { get; set; }
            public decimal? Qty_Small { get; set; }
            public decimal? Unit_Cost { get; set; }
            public decimal? Item_No { get; set; }
            public long? Detailes_Purshing_Id { get; set; }
        }
    }
}
