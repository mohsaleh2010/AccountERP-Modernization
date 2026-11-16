// AccountERP.Models/Menus/SysConfigMenus.cs

using AccountModelsLib;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{


    public class AuthService : IAuthService
    {
        public DataTable dt_Branch { get; set; } = new DataTable();  
        public List<Account_BranchesVM> account_BranchesLst { get; set; } = new List<Account_BranchesVM>();
        private readonly My_DbContext _dbContext; // سياق قاعدة البيانات الخاص بك
        private readonly MenuRepository _menuRepo;
        private readonly MenuStateService _menuState;
        private EncryptProcess en ;
        private readonly operations _opp; // سياق قاعدة البيانات الخاص بك
        public AuthService(My_DbContext dbContext, operations opp, MenuRepository menuRepo, MenuStateService menuState)
        {
            _dbContext = dbContext;
            _menuRepo = menuRepo;
            _menuState = menuState;
            _opp = opp;
            en = new EncryptProcess();
        }

        public async Task<bool> LoginAsync(LoginViewModel loginModel)
        {
            
            if (AdministrationLogin(loginModel) == true)
            {
                // 🔐 دخول الإدارة الخاص لا يمر بجدول القوائم (SysConfigMenus)، لكنه يظل دخولاً ناجحاً
                // ويجب أن يفتح بوابة الحماية (IsAuthenticated) حتى لا تتم إعادة توجيهه إلى شاشة الدخول باستمرار
                _menuState.SetAuthenticated(true);
                return true;
            }
            // 🔍 1. التحقق من المستخدم في قاعدة البيانات (يفضل تشفير الباسورد مستقبلاً)
            var user = await _dbContext.Set<USER_DETAILE>()
                .FirstOrDefaultAsync(u => u.USER_NAME == loginModel.Username && u.USER_PASSWORD == loginModel.Password && u.Active == true);

            var _user = await _opp.db.USER_DETAILES.AsNoTracking().Where(u => u.USER_NAME == loginModel.Username && u.USER_PASSWORD == loginModel.Password && u.Active == true).FirstOrDefaultAsync();
            if (user == null) return false; // مستخدم غير موجود أو غير نشط

            await PopulateSessionForUserAsync(user);
            Is_SavedPW(loginModel);
            return true;
        }

        // 🟢 يُستدعى دخول عادي ناجح (LoginAsync، بعد التحقق من كلمة السر) وأيضاً استعادة جلسة سابقة من
        // كوكي دخول موقّع (RestoreSessionAsync، بدون كلمة سر - الكوكي نفسه هو إثبات الدخول السابق).
        // كل ما يُشحن هنا (Common.*، القوائم المسموح بها، الفروع، IsAuthenticated) مطابق تماماً فى الحالتين
        // حتى لا تفترق جلسة "دخول جديد" عن جلسة "دائرة جديدة بعد كوكي دخول قديم" فى أي شيء.
        private async Task PopulateSessionForUserAsync(USER_DETAILE user)
        {
            // 🟢 ضبط حالة المستخدم الثابتة (Common.*) أولاً - قبل أي استعلام يعتمد عليها.
            // GetUserAllowedMenus يستدعي داخلياً Common.SetSystemParameters_Menus_DB() التي تقرأ
            // Common.User_Id مباشرة (وليس عبر باراميتر)؛ ضبطها بعد الاستعلام كان يجعل الاستعلام
            // ينفَّذ بقيمة User_Id قديمة/صفرية، فتعود قائمة القوائم فارغة دائماً ويظهر السايد بار بلا عناصر.
            Common.Is_User_Admin = user.AdminFlage ?? 0;
            Common.User_Id = (long)user.Id;
            Common.User_Name = user.USER_NAME;
            Common.FullUserName = user.Name;
            Common.Branch_Id = user.Branch_Id ?? 1;

            // 🟢 مكافئ استدعاء Common.Get_Is_Force_Admin_Permission() داخل Accounts.LoadingFrm فى تطبيق
            // الويندوز فورمز (يعمل هناك بعد الدخول مباشرة وقبل الشاشة الرئيسية) - يعتمد على Common.User_Id
            // المضبوط للتو أعلاه، فلا يصح تشغيله قبله. راجع MODERNIZATION_NOTES.md لتفاصيل الخطة الكاملة.
            try { Common.Get_Is_Force_Admin_Permission(); } catch { }

            // جلب وتجهيز الصلاحيات والقوائم ديناميكياً لهذا المستخدم فوراً
            // نمرر الـ USER_ID الخاص به، والمشروع رقم 2 (Accounting) كما حددنا سابقاً
            var allowedMenus = _menuRepo.GetUserAllowedMenus(  projectId: 2,  user.Id);

            // شحن الـ MenuStateService بالبيانات الحقيقية للمستخدم لتظهر في السايد بار فوراً
            _menuState.UserAllowedMenus = allowedMenus;
            _menuState.SetAuthenticated(true);

            // 🟢 عمداً بدون تفعيل/توسيع أي وحدة تلقائياً هنا: الشاشة الرئيسية بعد الدخول يجب أن تعرض كل
            // وحدات السايد بار مطوية (Collapsed) والقائمة النشطة الوحيدة هي "الرئيسية" نفسها - راجع
            // MODERNIZATION_NOTES.md. كانت هذه السطور تستدعي SelectMainMenu لأول وحدة فتفتحها تلقائياً
            // بمجرد الدخول، وهو عكس السلوك المطلوب فعلياً؛ يبقى SelectedMainMenuId فارغاً حتى يضغط
            // المستخدم على وحدة بنفسه من القائمة الجانبية.

            dt_Branch = new DataTable();
            try
            {
                Account_BrancheCLS account_BrancheCLS = new Account_BrancheCLS(_opp);

                dt_Branch = account_BrancheCLS.Get_Data(false);

                dt_Branch = account_BrancheCLS.Get_DataUSER_DETAILES_Branches_Permission(false, Common.User_Id);
                account_BranchesLst = account_BrancheCLS.Getaccount_BranchesLst();
            }
            catch (Exception ee)
            {
            }
        }

        // 🟢 يُستدعى فقط من جلسة ويب جديدة (دائرة/Circuit جديدة بعد إعادة تحميل كاملة للصفحة) تحمل كوكي
        // دخول موقّع صالحاً من دخول سابق ناجح فعلياً - راجع LoginTokenStore و"/Auth/Persist" فى
        // AccountERP.Web\Program.cs. لا يتحقق من كلمة سر هنا عمداً: الكوكي نفسه (موقَّع بواسطة
        // Data Protection، غير قابل للتزوير من طرف العميل) هو إثبات الدخول، تماماً كما تعمل أي كوكي جلسة.
        public async Task<bool> RestoreSessionAsync(long userId)
        {
            var user = await _dbContext.Set<USER_DETAILE>()
                .FirstOrDefaultAsync(u => u.Id == userId && u.Active == true);

            if (user == null) return false; // المستخدم حُذف/عُطِّل منذ إصدار الكوكي

            await PopulateSessionForUserAsync(user);
            return true;
        }

        public async Task Is_SavedPW(LoginViewModel loginModel)
        { }
        // 🔐 دخول الإدارة الخاص فى الويب مقفول افتراضياً. الثوابت المشفّرة فى Common.InvoiceSettingUser1/2
        // مفتاح فكّها فى نفس الكود، فلا تصلح كسر لدخول ويب مكشوف على الشبكة. يتفعّل فقط لو المسؤول ضبط
        // متغيرى البيئة ACCOUNTERP_ADMIN_USER و ACCOUNTERP_ADMIN_PASSWORD على السيرفر (قيم خارج الكود).
        // الديسكتوب (LoginPro / Alter_DataBase) لم يتغيّر.
        internal const string AdminUserEnvVar = "ACCOUNTERP_ADMIN_USER";
        internal const string AdminPasswordEnvVar = "ACCOUNTERP_ADMIN_PASSWORD";

        private static bool FixedTimeEquals(string a, string b)
        {
            if (a == null || b == null) return false;
            int diff = a.Length ^ b.Length;
            for (int i = 0; i < a.Length && i < b.Length; i++)
                diff |= a[i] ^ b[i];
            return diff == 0;
        }

        private bool AdministrationLogin(LoginViewModel loginModel)
        {
            Common.IsAdministration_AlterDBLogin = 0;
            bool LoginAdministrationValid = false;
            string adminUser = Environment.GetEnvironmentVariable(AdminUserEnvVar);
            string adminPassword = Environment.GetEnvironmentVariable(AdminPasswordEnvVar);
            if (string.IsNullOrWhiteSpace(adminUser) || string.IsNullOrEmpty(adminPassword))
                return false;
            if (FixedTimeEquals(loginModel.Username, adminUser) & FixedTimeEquals(loginModel.Password, adminPassword))
            {
                Common.IsAdministration_AlterDBLogin = 1;
                //this.Is_SaveUserNameLogin = false;
                //this.Is_SaveUserPasswordLogin = false;
                
                Common.Is_User_Admin = 1;
                
                Common.User_Id = 0;
               
                Common.User_Name = "Force Admin";
                Common.FullUserName = "Force Admin";
                Common.Branch_Id = 1;
                LoginAdministrationValid = true;
            }
            return LoginAdministrationValid;
        }
        //private void Login_Click(object sender, EventArgs e)
        //{
        //    Common.IsAdministration_AlterDBLogin = 0;
        //    this.AdministrationLogin();
        //    if (Common.IsAdministration_AlterDBLogin == 1)
        //    {
        //        return;
        //    }
        //    else
        //    {
        //        this.MakeLogin(  loginModel);
        //        //this.Is_SaveUserNameLogin = Common.IsHavePermissionSysMenu(Common.User_Id, 0x157, Common.Project_ID, Common.Branch_Id);
        //        //this.Is_SaveUserPasswordLogin = Common.IsHavePermissionSysMenu(Common.User_Id, 0x158, Common.Project_ID, Common.Branch_Id);

        //        this.Is_SaveUserNameLogin = Common.IsHavePermissionSysMenu(Common.User_Id, 343, 0, Common.Branch_Id);
        //        this.Is_SaveUserPasswordLogin = Common.IsHavePermissionSysMenu(Common.User_Id, 344, 0, Common.Branch_Id);

        //        this.Is_SavedPW();
        //    }
        //}
        //private void MakeLogin(LoginViewModel loginModel)
        //{

        //   // this.Counter++;
        //    this.username = this.txtUsername.Text.Trim();
        //    this.password = this.txtpassword.Text.Trim();
        //    DataSet set = new DataSet();
        //    string[] textArray1 = new string[] { " SELECT  * FROM USER_DETAILES WHERE  (USER_NAME = '", this.username, "') AND (USER_PASSWORD = '", this.password, "') AND (Active = 1 )" };
        //    set = new AccountModelsLib.operations().return_dataset(string.Concat(textArray1));
        //    try
        //    {
        //        if (set.Tables[0].Rows.Count != 1)
        //        {
        //            MessageBox.Show("يوجد خطأ فى أسم المستخدم أو كلمة المرور ", "تأكد من كلمة المرور", MessageBoxButtons.OK, MessageBoxIcon.Hand);
        //            this.txtUsername.Focus();
        //            mylogin.flage_Valid = 0;
        //            mylogin.User_Id = 0;
        //        }
        //        else
        //        {
        //            mylogin.flage_Valid = 1;
        //            Common.Is_User_Admin = int.Parse(set.Tables[0].Rows[0]["AdminFlage"].ToString());
        //            mylogin.User_Id = long.Parse(set.Tables[0].Rows[0][ID"].ToString());
        //            Common.User_Id = mylogin.User_Id;
        //            mylogin.username = set.Tables[0].Rows[0]["USER_NAME"].ToString();
        //            Common.User_Name = mylogin.username;
        //            Common.FullUserName = set.Tables[0].Rows[0]["NAME"].ToString();
        //            try
        //            {
        //                Common.Branch_Id = int.Parse(set.Tables[0].Rows[0]["Branch_Id"].ToString());
        //            }
        //            catch
        //            {
        //                Common.Branch_Id = -1;
        //            }
        //            this.progressBar2.Visible = true;
        //            this.BtnLogin.Enabled = false;
        //            this.gbLogin.Visible = true;
        //            this.timer1.Enabled = true;
        //            try
        //            {
        //                if (mylogin.flage_Valid == 1)
        //                {
        //                    this.branch_List1.Get_Data(false);
        //                }
        //                else
        //                {
        //                    this.branch_List1.Get_DataUSER_DETAILES_Branches_Permission(false, Common.User_Id);
        //                }
        //            }
        //            catch
        //            {
        //            }
        //        }
        //    }
        //    catch
        //    {
        //        MessageBox.Show("خطا فى الإتصال بقاعدة البيانات ", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Hand);
        //        mylogin.flage_Valid = 0;
        //        mylogin.User_Id = 0;
        //    }
        //    this.Cursor = Cursors.Default;
        //    this.btnclose.Enabled = true;
        //    this.BtnSetting.Enabled = true;
        //}
        public Task LogoutAsync()
        {
            _menuState.UserAllowedMenus = new List<MenuViewModel>(); // تفريغ القوائم
            _menuState.SetAuthenticated(false);
            return Task.CompletedTask;
        }

        // 🟢 نفس تسلسل الاستعلامات الثلاثة الموجود فعلياً فى LoginPro.ChangePassword.Login_Click (WinForms):
        // (1) تحقق أن كلمة المرور القديمة صحيحة لنفس المستخدم الحالي (USER_ID)، (2) ارفض كلمة المرور
        // الجديدة لو كانت مستخدمة بالفعل لأي مستخدم آخر فى النظام، (3) حدّث. منقولة هنا (بدل استدعاء
        // LoginPro مباشرة) لأن LoginPro مشروع ويندوز فورمز غير مُشار إليه من مشاريع Blazor إطلاقاً - راجع
        // MODERNIZATION_NOTES.md. بدون أي Hashing عمداً: نفس سلوك النظام الأصلي تماماً (عمود
        // USER_DETAILES.USER_PASSWORD نص صريح)، هذه الدالة لا تُدخِل تشفيراً جديداً على البيانات القائمة.
        public async Task<ChangePasswordResult> ChangePasswordAsync(string oldPassword, string newPassword)
        {
            var user = await _dbContext.Set<USER_DETAILE>()
                .FirstOrDefaultAsync(u => u.Id == Common.User_Id && u.USER_NAME == Common.User_Name && u.USER_PASSWORD == oldPassword);

            if (user == null) return ChangePasswordResult.InvalidOldPassword;

            // 🟢 بدون استثناء المستخدم الحالي عمداً - نفس استعلام LoginPro.ChangePassword الأصلي بالضبط
            // (select * from USER_DETAILES where USER_PASSWORD=...، بلا شرط USER_ID)، فيرفض أيضاً محاولة
            // "تغيير" كلمة المرور لنفس قيمتها الحالية (لأن صف المستخدم نفسه لا يزال يحمل القيمة القديمة
            // فى هذه اللحظة) - سلوك مقصود فى النظام الأصلي وليس خطأً يجب تصحيحه هنا.
            var passwordAlreadyUsed = await _dbContext.Set<USER_DETAILE>()
                .AnyAsync(u => u.USER_PASSWORD == newPassword);

            if (passwordAlreadyUsed) return ChangePasswordResult.PasswordAlreadyUsed;

            user.USER_PASSWORD = newPassword;
            await _dbContext.SaveChangesAsync();

            return ChangePasswordResult.Success;
        }
    }
}