using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib
{
    /// <summary>
    /// حماية الشاشات القديمة للأذون (خطة 03 م-03-6): الإذن المسودة أو الملغى أو المعكوس أو المرحَّل بالمحرك
    /// لا يُعدَّل ولا يُحذف من الشاشات القديمة لأنها تعدّل الرصيد مباشرة بلا دفتر؛ يُعكس من الشاشة الجديدة.
    /// لو لم تُضَف أعمدة المحرك بعد على القاعدة (عميل لم يُرقَّ) فلا أذون جديدة أصلاً، فيُسمح بالتعديل كما كان.
    /// </summary>
    public static class StoreOrderLegacyGuard
    {
        public const string LockedMessage = "هذا الإذن من شاشة الأذون الجديدة (مسودة أو مرحَّل بالمحرك أو معكوس)؛ لا يُعدَّل ولا يُحذف من هذه الشاشة، ويُعكس من الشاشة الجديدة.";

        /// <summary>هل الإذن مقفل أمام الشاشات القديمة؟ opp = null يفتح اتصالاً مستقلاً.</summary>
        public static bool IsLocked(operations opp, long orderId)
        {
            if (orderId <= 0) return false;
            try
            {
                var o = opp ?? new operations();
                return o.db.Database.SqlQuery<int>($@"
SELECT CASE WHEN EXISTS (
    SELECT 1 FROM Order_Purshing p
    WHERE p.Id = {orderId}
      AND (ISNULL(p.Doc_Status, 1) <> 1 OR p.Reversal_Of_Id IS NOT NULL
           OR EXISTS (SELECT 1 FROM Detailes_Purshing_Import_Export d WHERE d.OrderTable_id = p.Id AND d.Engine_Posted = 1)))
THEN 1 ELSE 0 END AS Value").AsEnumerable().First() == 1;
            }
            catch (Microsoft.Data.SqlClient.SqlException)
            {
                return false;   // أعمدة المحرك غير موجودة على هذه القاعدة
            }
        }

        public const string EngineLineMessage = "لهذا الصنف فى هذا المخزن حركات مرحَّلة بالمحرك الجديد؛ يُعدَّل رصيد أول المدة من شاشة رصيد أول المدة الجديدة.";

        /// <summary>
        /// هل لسطر المخزون حركات مرحَّلة بالمحرك؟ أداة «تعديل أول المدة» القديمة تعيد كتابة الأرصدة فى أسطر الأذون و Unit_item مباشرة،
        /// فتُرفض لهذا السطر (خطة 02/03).
        /// </summary>
        public static bool HasEngineMovements(operations opp, long unitItemId)
        {
            if (unitItemId <= 0) return false;
            try
            {
                var o = opp ?? new operations();
                return o.db.Database.SqlQuery<int>($@"
SELECT CASE WHEN EXISTS (SELECT 1 FROM Detailes_Purshing_Import_Export WHERE Unit_item_Table_Id = {unitItemId} AND Engine_Posted = 1)
THEN 1 ELSE 0 END AS Value").AsEnumerable().First() == 1;
            }
            catch (Microsoft.Data.SqlClient.SqlException)
            {
                return false;   // أعمدة المحرك غير موجودة على هذه القاعدة
            }
        }

        /// <summary>نفس الفحص بمعرّف سطر الإذن.</summary>
        public static bool IsLockedByDetail(operations opp, long detailId)
        {
            if (detailId <= 0) return false;
            var o = opp ?? new operations();
            long orderId = o.db.Detailes_Purshings.AsNoTracking().Where(d => d.Id == detailId).Select(d => d.OrderTable_id).FirstOrDefault();
            return IsLocked(o, orderId);
        }
    }
}
