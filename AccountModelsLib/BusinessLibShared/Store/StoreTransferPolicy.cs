using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib
{
    /// <summary>
    /// طريقة التحويل بين المخازن (خطة 01 §2.7، قرار م-01-7): إعداد عام فى SysConfig بثلاث قيم + صلاحية متقدمة لمستخدمين بعينهم.
    /// 0 = دائماً بتأكيد استلام (شحن ← فى الطريق ← استلام)، 1 = دائماً مباشر، 2 = حسب صلاحية المستخدم المُصدِر.
    /// يُقرأ الإعداد مباشرة من SysConfig (مفكوك التشفير كباقى الإعدادات) ويُخزَّن دقيقة، فلا يلزم تعديل محمّلات Common الثلاثة.
    /// الإعداد والصلاحية يُنشآن عند ترقية القاعدة (StockEngineSchemaCLS.TransferSettings) بقيمة الإعداد القديم.
    /// </summary>
    public static class StoreTransferPolicy
    {
        public const string ModeKey = "Store_Transfer_Mode";
        public const string LegacyKey = "Make_Store_Order_Sarf_Estlam_Ta7weel_Automatic_Between_Stores";
        /// <summary>اسم عنصر الصلاحية المتقدمة «تحويل مباشر بين المخازن بدون تأكيد استلام» تحت 321.</summary>
        public const string DirectPermissionKey = "StoreTransferDirectWithoutReceipt";

        public enum Mode { WithReceipt = 0, Direct = 1, ByPermission = 2 }

        private static Mode? _mode;
        private static long? _permissionId;
        private static DateTime _readAt;

        /// <summary>الإعداد العام (الافتراضى بتأكيد استلام لو غير موجود).</summary>
        public static Mode GetMode(operations opp)
        {
            Refresh(opp);
            return _mode ?? Mode.WithReceipt;
        }

        /// <summary>هل يملك المستخدم صلاحية التحويل المباشر؟</summary>
        public static bool UserCanTransferDirect(operations opp, long userId)
        {
            Refresh(opp);
            return _permissionId is long id && Common.IsHavePermissionSysMenu(userId, (int)id, 2, -1);
        }

        /// <summary>
        /// الطريقة الفعلية للمستخدم: 0 ← بتأكيد دائماً، 1 ← مباشر دائماً، 2 ← مباشر فقط لمن يملك الصلاحية وطلبه (الافتراضى بتأكيد).
        /// </summary>
        public static StoreOrderService.TransferMode Effective(operations opp, long userId, bool directRequested)
        {
            return GetMode(opp) switch
            {
                Mode.Direct => StoreOrderService.TransferMode.Direct,
                Mode.ByPermission when directRequested && UserCanTransferDirect(opp, userId) => StoreOrderService.TransferMode.Direct,
                _ => StoreOrderService.TransferMode.WithReceiptConfirmation,
            };
        }

        /// <summary>هل تُعرض للمستخدم خانة «تحويل مباشر» فى الصفحة؟ (الإعداد 2 + الصلاحية)</summary>
        public static bool ShowDirectChoice(operations opp, long userId) =>
            GetMode(opp) == Mode.ByPermission && UserCanTransferDirect(opp, userId);

        private static void Refresh(operations opp)
        {
            if (_mode != null && (DateTime.UtcNow - _readAt).TotalSeconds < 60) return;
            var cfg = opp.db.Database.SqlQueryRaw<KeyValueRow>("SELECT Id, KeysData, Value FROM SysConfig WHERE KeysData IS NOT NULL").ToList();
            var row = cfg.FirstOrDefault(r => Common.DecryptMe(r.KeysData).Trim() == ModeKey);
            _mode = row != null && int.TryParse(Common.DecryptMe(row.Value ?? "").Trim(), out int v) && Enum.IsDefined(typeof(Mode), v) ? (Mode)v : Mode.WithReceipt;
            var menus = opp.db.Database.SqlQueryRaw<KeyValueRow>("SELECT Id, KeysData, Value FROM SysConfigMenus WHERE Parent_Id = 321 AND KeysData IS NOT NULL").ToList();
            _permissionId = menus.FirstOrDefault(m => Common.DecryptMe(m.KeysData).Trim() == DirectPermissionKey)?.Id;
            _readAt = DateTime.UtcNow;
        }

        /// <summary>يُفرغ الذاكرة المؤقتة (بعد تعديل الإعداد من شاشة الإعدادات).</summary>
        public static void Invalidate() => _mode = null;

        /// <summary>صف قراءة (عام لأن EF يملؤه من SQL خام).</summary>
        public class KeyValueRow
        {
            public long Id { get; set; }
            public string KeysData { get; set; }
            public string Value { get; set; }
        }
    }
}
