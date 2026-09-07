using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

namespace AccountModelsLib
{
    // 🟢 مفاتيح إعدادات المستخدم المعروفة حالياً - جدول USER_DETAILES_Setting نفسه Key/Value (User_Id,
    // SettingKey, SettingValue) وليس عموداً ثابتاً لكل إعداد، فإضافة إعداد مستقبلي جديد لا يحتاج أي تعديل
    // على الجدول أو هذا الكلاس - فقط مفتاح جديد هنا وقراءة/كتابة له من الخدمة المستهلكة (راجع
    // AccountERP.SharedUI.Services.UserPreferencesService).
    public static class UserSettingKeys
    {
        public const string ShowBreadcrumb = "ShowBreadcrumb";
        public const string ShowTabs = "ShowTabs";
    }

    // 🟢 نفس نمط SysConfigMenusPERMATIONSCLS المجاور (SysConfigMenusCLS.cs): كلاس بسيط بلا حقن تبعيات
    // (new operations() داخلياً)، يُستخدَم فقط من طبقة Blazor حالياً (لا شاشة WinForms تستهلكه) لكنه يعيش هنا
    // فى AccountModelsLib مثل بقية طبقة الـ CLS المشتركة، تمهيداً لأي استخدام WinForms مستقبلي لنفس الإعدادات.
    public class UserSettingsCLS
    {
        public Dictionary<string, string> GetUserSettings(long userId)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var opp = new operations();
            var sql = "SELECT SettingKey, SettingValue FROM USER_DETAILES_Setting WHERE User_Id = " + userId;
            DataTable dt = opp.return_DataTable(sql);

            foreach (DataRow row in dt.Rows)
            {
                var key = row["SettingKey"]?.ToString();
                if (string.IsNullOrEmpty(key)) continue;
                result[key] = row["SettingValue"]?.ToString() ?? string.Empty;
            }

            return result;
        }

        // Upsert بسيط (حذف ثم إدراج): تكرار حفظ نفس المفتاح لنفس المستخدم متوقّع هنا (كل تبديل فى لوحة
        // الإعدادات يحفظ فوراً)، وحجم الجدول لكل مستخدم صغير جداً (عدد الإعدادات المعروفة فقط) فلا داعٍ
        // لتعقيد MERGE - القيد الفريد (User_Id, SettingKey) أصلاً يمنع أي تكرار حتى لو تسابقت طلبات نادراً.
        //
        // 🟢 استخدام SqlCommand بمعاملات (Parameters) مباشرة على opp.conn هنا - وليس opp.insert_me(sql) كباقي
        // هذا الملف - عمداً: عمود Id فى هذا الجدول IDENTITY(1,1) حقيقي (auto-increment من SQL Server نفسه)،
        // بينما operations.insert_me يمرّ دائماً عبر SetUpInsertCmd التى تُدرج قيمة Id صريحة تلقائياً كلما كان
        // Common.Use_Auto_Identity_Column = 0 (قيمته الفعلية فى هذه القاعدة وقت الكتابة - راجع
        // MODERNIZATION_NOTES.md) - فيفشل الإدراج دائماً بخطأ "Cannot insert explicit value for identity
        // column" (كان يفشل بصمت هنا تحديداً لأن SaveUserSetting نفسها تُستدعى من كتلة try/catch فى
        // UserPreferencesService.TrySave). المعاملات هنا إضافة أمان أيضاً (SettingValue قد يحمل نصاً حراً
        // لإعدادات مستقبلية، بعكس مفاتيح ثابتة معروفة كـ "ShowTabs" اليوم).
        public void SaveUserSetting(long userId, string settingKey, string settingValue)
        {
            var opp = new operations();
            opp.MyConn();

            if (opp.conn.State == ConnectionState.Closed)
            {
                opp.conn.Open();
            }

            try
            {
                using (var deleteCmd = new SqlCommand("DELETE FROM USER_DETAILES_Setting WHERE User_Id = @UserId AND SettingKey = @SettingKey", opp.conn))
                {
                    deleteCmd.Parameters.AddWithValue("@UserId", userId);
                    deleteCmd.Parameters.AddWithValue("@SettingKey", settingKey ?? string.Empty);
                    deleteCmd.ExecuteNonQuery();
                }

                using (var insertCmd = new SqlCommand(
                    "INSERT INTO USER_DETAILES_Setting (User_Id, SettingKey, SettingValue, InsertServerDateTime) VALUES (@UserId, @SettingKey, @SettingValue, GETDATE())",
                    opp.conn))
                {
                    insertCmd.Parameters.AddWithValue("@UserId", userId);
                    insertCmd.Parameters.AddWithValue("@SettingKey", settingKey ?? string.Empty);
                    insertCmd.Parameters.AddWithValue("@SettingValue", (object)settingValue ?? DBNull.Value);
                    insertCmd.ExecuteNonQuery();
                }
            }
            finally
            {
                opp.conn.Close();
            }
        }
    }
}
