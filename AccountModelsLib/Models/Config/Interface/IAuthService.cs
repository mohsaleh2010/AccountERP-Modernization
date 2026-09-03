// AccountERP.Models/Menus/SysConfigMenus.cs
 
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{


    // 🟢 نتيجة ChangePasswordAsync - نفس التحققات الثلاثة الموجودة فعلياً فى LoginPro.ChangePassword
    // (شاشة الويندوز فورمز الأصلية لتغيير كلمة المرور)، منقولة هنا لأن LoginPro مشروع ويندوز فورمز
    // غير متاح من Blazor - راجع MODERNIZATION_NOTES.md.
    public enum ChangePasswordResult
    {
        Success,
        InvalidOldPassword,
        PasswordAlreadyUsed,
    }

    public interface IAuthService
    {
        DataTable dt_Branch { get; set; }
        List<Account_BranchesVM> account_BranchesLst { get; set; }
        Task<bool> LoginAsync(LoginViewModel loginModel);

        // 🟢 يستعيد جلسة دخول سابقة من كوكي دخول موقّع (بدون كلمة سر) - راجع AuthService.RestoreSessionAsync
        // وLoginTokenStore و"/Auth/Persist" فى AccountERP.Web\Program.cs لسبب وجودها.
        Task<bool> RestoreSessionAsync(int userId);

        Task LogoutAsync();

        // 🟢 تغيير كلمة المرور للمستخدم المسجّل دخوله حالياً - نفس منطق LoginPro.ChangePassword
        // (Login_Click) بالضبط: تحقق من كلمة المرور القديمة لنفس المستخدم (USER_ID الحالي)، ثم تحقق أن
        // كلمة المرور الجديدة غير مستخدمة لأي مستخدم آخر فى النظام (نفس القاعدة الموجودة فى الأصل رغم
        // أنها تبدو غريبة)، ثم التحديث. بدون أي تشفير/Hashing - النظام الأصلي يخزن كلمات المرور نصاً
        // صريحاً فى عمود USER_DETAILES.USER_PASSWORD ولا تغيّر هذه الدالة تلك القاعدة.
        Task<ChangePasswordResult> ChangePasswordAsync(string oldPassword, string newPassword);
    }
}