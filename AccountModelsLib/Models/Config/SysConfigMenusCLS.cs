// AccountERP.Models/Menus/SysConfigMenus.cs

using AccountModelsLib.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;

namespace AccountModelsLib
{

    public class SysConfigMenusCLS
    {
        public SysConfigMenusCLS()
        {

        }
        
    }
    public class SysConfigMenusPERMATIONSCLS
    {
        public SysConfigMenusPERMATIONSCLS()
        {

        }
        public   DataTable SetSystemParameters_Menus_DB(int PROJECT_ID = 0, long USER_ID = 0)
        {

            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            // 🟢 Description_En/Description_Fr/Description_DE أُضيفت للجدول لدعم القوائم متعددة اللغات
            // (راجع MODERNIZATION_NOTES.md) - يجب تحديدها صراحة هنا وإلا فشلت قراءتها لاحقاً فى
            // MenuRepository.GetUserAllowedMenus بخطأ "Column ... does not belong to table" رغم وجود
            // الأعمدة فعلياً فى قاعدة البيانات، لأن DataTable المُعاد لا يحتوي إلا على الأعمدة المُحدَّدة هنا.
            string mysql = @" SELECT SysConfigMenus.KeysData, SysConfigMenus.Description, SysConfigMenus.Description_En,
                                SysConfigMenus.Description_Fr, SysConfigMenus.Description_DE, SysConfigMenusPERMATIONS.*
                                FROM SysConfigMenusPERMATIONS INNER JOIN
                         SysConfigMenus ON SysConfigMenusPERMATIONS.Menu_ID = SysConfigMenus.Id ";
            mysql = mysql + " WHERE (SysConfigMenusPERMATIONS.USER_ID = " + USER_ID + ") ";
            if (PROJECT_ID > 0)
            {
                mysql = mysql + " AND (SysConfigMenusPERMATIONS.PROJECT_ID = " + PROJECT_ID + ")";
            }
            // 🟢 IsShowToUser على الجدولين معاً (راجع AlterDB_CLS.AlterDBThisYear): تعريف القائمة نفسها
            // (SysConfigMenus) قد يُخفيها من الجميع، وصف صلاحية المستخدم (SysConfigMenusPERMATIONS) قد
            // يُخفيها لهذا المستخدم تحديداً دون غيره - يجب أن يكون كلاهما = 1 (أو NULL، لصفوف قديمة قبل
            // إضافة العمود) لتظهر القائمة فعلياً فى السايد بار.
            mysql = mysql + " AND (ISNULL(SysConfigMenus.IsShowToUser, 1) = 1) AND (ISNULL(SysConfigMenusPERMATIONS.IsShowToUser, 1) = 1) ";
            mysql = mysql + " ORDER BY SysConfigMenusPERMATIONS.Parent_Id ";
            DataTable dt = opp.return_DataTable(mysql);
            return dt;


        }
    }
    public class MenuRepository
    {
        public List<MenuViewModel> GetUserAllowedMenus(  int projectId, long userId)
        {
            // 🟢 يعتمد الاستعلام داخل Common.SetSystemParameters_Menus_DB على Common.User_Id الثابتة
            // (يجب أن يكون AuthService قد ضبطها = userId قبل استدعاء هذه الدالة) وعلى projectId الممرر
            // هنا فعلياً - كان يُستدعى بدون باراميتر فيتجاهل تصفية PROJECT_ID ويُعيد قوائم كل المشاريع.
            SysConfigMenusPERMATIONSCLS menuPerm= new SysConfigMenusPERMATIONSCLS();
            DataTable dtConfig = menuPerm.SetSystemParameters_Menus_DB(projectId, userId);
            List<MenuViewModel> menuViewModels = new List<MenuViewModel>();
            Common.Is_Ecrypted_Config = 1;
                for (int x=0;x<dtConfig.Rows.Count ;x++)
            {
                MenuViewModel menuViewModel = new MenuViewModel();
                menuViewModel.Id = int.Parse(dtConfig.Rows[x]["Id"].ToString());
                menuViewModel.Menu_ID = int.Parse(dtConfig.Rows[x]["Menu_ID"].ToString());
                menuViewModel.ParentId = int.Parse(dtConfig.Rows[x]["Parent_Id"].ToString());
                menuViewModel.KeysData = Common.DecryptMe(dtConfig.Rows[x]["KeysData"].ToString());
                menuViewModel.Description = Common.DecryptMe(dtConfig.Rows[x]["Description"].ToString());
                menuViewModel.Description_En = Common.DecryptMe(dtConfig.Rows[x]["Description_En"].ToString());
                menuViewModel.Description_Fr = Common.DecryptMe(dtConfig.Rows[x]["Description_Fr"].ToString());
                menuViewModel.Description_DE = Common.DecryptMe(dtConfig.Rows[x]["Description_DE"].ToString());
                menuViewModel.ProjectId = int.Parse(dtConfig.Rows[x]["PROJECT_ID"].ToString());
 


        menuViewModels.Add(menuViewModel);
            }
            // في حال عدم تطابق المستخدم، نعيد قائمة فارغة (بدون صلاحيات)
            return menuViewModels;
        }
    }
    public class MenuViewModel
    {
        public long  Id { get; set; }
        public long? ParentId { get; set; }
        public long Menu_ID { get; set; }
        public string KeysData { get; set; } // اسم القائمة أو الرابط (مثلاً "Sales", "Customers")
        public string Description { get; set; }  
        public string Description_En { get; set; }  
        public string Description_Fr { get; set; }  
        public string Description_DE { get; set; }  
         public int ProjectId { get; set; }
    }
    public class MenuStateService
    {
        public List<MenuViewModel> UserAllowedMenus { get; set; } = new();

        // 🟢 بوابة حماية: تمنع إعادة الجلب إذا أصبحت true
        public bool IsLoaded => UserAllowedMenus != null && UserAllowedMenus.Any();

        // 🔐 مصدر الحقيقة الوحيد لحالة "هل المستخدم مسجّل دخوله فعلاً؟" في هذه الجلسة (Scoped per user/circuit).
        // لا تعتمد على IsLoaded لهذا الغرض: مستخدم مسجّل دخوله بدون أي صلاحيات قوائم يظل IsLoaded=false رغم كونه مسجلاً.
        // تُضبط صراحة بواسطة AuthService عند نجاح الدخول (بما فى ذلك دخول الإدارة الخاص)، وتُعاد لـ false عند الخروج.
        public bool IsAuthenticated { get; set; }
        public event Action OnAuthChanged;

        public void SetAuthenticated(bool isAuthenticated)
        {
            IsAuthenticated = isAuthenticated;
            OnAuthChanged?.Invoke();
        }

        public long? SelectedMainMenuId { get; private set; }
        public event Action OnMainMenuChanged;

        public void SelectMainMenu(long mainMenuId)
        {
            SelectedMainMenuId = mainMenuId;
            OnMainMenuChanged?.Invoke();
        }

        // 🟢 حالة الطي/التوسيع لكل مجموعة قائمة (رئيسية عبر NavMenuMud أو فرعية عبر MenuNode المتكرر
        // ذاتياً لأي عمق) مركزية هنا بدلاً من متفرقة بين "isActive" المرتبط بـ SelectedMainMenuId فى
        // NavMenuMud و"_expanded" المحلية لكل عقدة فى MenuNode - يلزم مصدر واحد للحقيقة حتى يعمل زر
        // "توسيع/طي الكل" فى رأس القائمة الجانبية على كامل الشجرة (كل الأعماق) بضغطة واحدة.
        public HashSet<long> ExpandedMenuIds { get; } = new();
        public event Action OnExpandCollapseAll;

        public bool IsMenuExpanded(long menuId) => ExpandedMenuIds.Contains(menuId);

        public void SetMenuExpanded(long menuId, bool expanded)
        {
            if (expanded) ExpandedMenuIds.Add(menuId);
            else ExpandedMenuIds.Remove(menuId);
        }

        public void ExpandAllMenus()
        {
            ExpandedMenuIds.Clear();
            // فقط القوائم التى لديها أبناء فعلياً قابلة للطي/التوسيع أصلاً (MudNavGroup) - القوائم
            // النهائية (MudNavLink) ليس لها حالة توسيع لتُضاف هنا.
            foreach (var parentId in UserAllowedMenus.Where(m => m != null).Select(m => m.Menu_ID).Distinct())
            {
                if (UserAllowedMenus.Any(c => c.ParentId == parentId))
                {
                    ExpandedMenuIds.Add(parentId);
                }
            }
            OnExpandCollapseAll?.Invoke();
        }

        public void CollapseAllMenus()
        {
            ExpandedMenuIds.Clear();
            OnExpandCollapseAll?.Invoke();
        }
    }
}