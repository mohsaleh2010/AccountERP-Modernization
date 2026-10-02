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
            // 🟢 SysConfigMenus.Sort_Order (الترتيب الافتراضى الذى يضبطه الأدمن) مُستعلَم هنا باسم مستعار
            // Default_Sort_Order لتفادى تعارض الاسم مع SysConfigMenusPERMATIONS.Sort_Order (الترتيب الشخصى
            // لهذا المستخدم) القادم من SysConfigMenusPERMATIONS.* - راجع MenuRepository.GetUserAllowedMenus
            // لمنطق دمج الاثنين فى ترتيب ظهور فعلى واحد.
            string mysql = @" SELECT SysConfigMenus.KeysData, SysConfigMenus.Description, SysConfigMenus.Description_En,
                                SysConfigMenus.Description_Fr, SysConfigMenus.Description_DE, SysConfigMenus.RazorPath,
                                SysConfigMenus.Sort_Order AS Default_Sort_Order, SysConfigMenusPERMATIONS.*
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
                menuViewModel.Id = long.Parse(dtConfig.Rows[x]["Id"].ToString());
                menuViewModel.Menu_ID = int.Parse(dtConfig.Rows[x]["Menu_ID"].ToString());
                menuViewModel.ParentId = int.Parse(dtConfig.Rows[x]["Parent_Id"].ToString());
                menuViewModel.RazorPath = Common.DecryptMe(dtConfig.Rows[x]["RazorPath"].ToString());
                menuViewModel.KeysData = Common.DecryptMe(dtConfig.Rows[x]["KeysData"].ToString());
                menuViewModel.Description = Common.DecryptMe(dtConfig.Rows[x]["Description"].ToString());
                menuViewModel.Description_En = Common.DecryptMe(dtConfig.Rows[x]["Description_En"].ToString());
                menuViewModel.Description_Fr = Common.DecryptMe(dtConfig.Rows[x]["Description_Fr"].ToString());
                menuViewModel.Description_DE = Common.DecryptMe(dtConfig.Rows[x]["Description_DE"].ToString());
                menuViewModel.ProjectId = int.Parse(dtConfig.Rows[x]["PROJECT_ID"].ToString());
                menuViewModel.DefaultSortOrder = dtConfig.Rows[x]["Default_Sort_Order"] == DBNull.Value ? null : int.Parse(dtConfig.Rows[x]["Default_Sort_Order"].ToString());
                menuViewModel.UserSortOrder = dtConfig.Rows[x]["Sort_Order"] == DBNull.Value ? null : int.Parse(dtConfig.Rows[x]["Sort_Order"].ToString());


        menuViewModels.Add(menuViewModel);
            }
            ApplyMenuOrder(menuViewModels);
            // في حال عدم تطابق المستخدم، نعيد قائمة فارغة (بدون صلاحيات)
            return menuViewModels;
        }

        // 🟢 ترتيب الظهور الفعلى لكل مجموعة إخوة (نفس ParentId): الأصل ترتيب الأدمن الافتراضى
        // (DefaultSortOrder)، وأى مستخدم خصَّص ترتيباً شخصياً لبعض إخوة هذا المستوى (UserSortOrder) يتغلَّب
        // ترتيبه على الافتراضى لهذا المستوى بالذات - وأى قائمة لم يُرتِّبها هو شخصياً بعد (صلاحية جديدة
        // مُتاحة له حديثاً) تُضاف فى آخر ترتيبه الشخصى تلقائياً بدل أن تظهر فى منتصف القائمة حسب الترتيب
        // الافتراضى. بدون أى تخصيص شخصى لهذا المستوى إطلاقاً، يُستخدَم الترتيب الافتراضى مباشرة للجميع.
        public static void ApplyMenuOrder(List<MenuViewModel> menuViewModels)
        {
            foreach (var siblings in menuViewModels.GroupBy(m => m.ParentId))
            {
                var rank = 0;
                if (siblings.Any(m => m.UserSortOrder.HasValue))
                {
                    var ordered = siblings.Where(m => m.UserSortOrder.HasValue).OrderBy(m => m.UserSortOrder!.Value);
                    var unordered = siblings.Where(m => !m.UserSortOrder.HasValue)
                                             .OrderBy(m => m.DefaultSortOrder ?? int.MaxValue).ThenBy(m => m.Menu_ID);
                    foreach (var m in ordered.Concat(unordered)) m.SortRank = rank++;
                }
                else
                {
                    foreach (var m in siblings.OrderBy(m => m.DefaultSortOrder ?? int.MaxValue).ThenBy(m => m.Menu_ID)) m.SortRank = rank++;
                }
            }

            menuViewModels.Sort((a, b) =>
            {
                var byParent = a.ParentId.GetValueOrDefault().CompareTo(b.ParentId.GetValueOrDefault());
                return byParent != 0 ? byParent : a.SortRank.CompareTo(b.SortRank);
            });
        }
    }

    // 🟢 يحفظ ترتيب الظهور: SaveAdminDefaultOrder يضبط SysConfigMenus.Sort_Order (يراه كل المستخدمين
    // كافتراضى)، SaveUserOrder يضبط SysConfigMenusPERMATIONS.Sort_Order (يخص مستخدماً واحداً فقط ويتغلَّب
    // على الافتراضى له هو تحديداً) - راجع MenuRepository.ApplyMenuOrder لمنطق الدمج بينهما وقت القراءة.
    public class MenuOrderCLS
    {
        public void SaveAdminDefaultOrder(List<long> orderedMenuIds)
        {
            var opp = new operations();
            var order = 10;
            foreach (var id in orderedMenuIds)
            {
                opp.update_me($"UPDATE SysConfigMenus SET Sort_Order = {order} WHERE Id = {id}");
                order += 10;
            }
        }

        public void SaveUserOrder(long userId, List<long> orderedMenuIds)
        {
            var opp = new operations();
            var order = 10;
            foreach (var id in orderedMenuIds)
            {
                opp.update_me($"UPDATE SysConfigMenusPERMATIONS SET Sort_Order = {order} WHERE USER_ID = {userId} AND Menu_ID = {id}");
                order += 10;
            }
        }

        // 🟢 "إرجاع لترتيبى الافتراضى": يمسح تخصيص هذا المستخدم لهذا المستوى بالذات فقط (يعود يتبع ترتيب
        // الأدمن الافتراضى تلقائياً)، بدون التأثير على أى مستوى آخر خصَّصه هذا المستخدم من قبل.
        public void ResetUserOrderForLevel(long userId, IEnumerable<long> menuIdsAtThisLevel)
        {
            var opp = new operations();
            foreach (var id in menuIdsAtThisLevel)
            {
                opp.update_me($"UPDATE SysConfigMenusPERMATIONS SET Sort_Order = NULL WHERE USER_ID = {userId} AND Menu_ID = {id}");
            }
        }
    }
    public class MenuViewModel
    {
        public long  Id { get; set; }
        public long? ParentId { get; set; }
        public long Menu_ID { get; set; }
        public string KeysData { get; set; } // اسم القائمة أو الرابط (مثلاً "Sales", "Customers")
        public string Description { get; set; }  
        public string RazorPath { get; set; }  
        public string Description_En { get; set; }  
        public string Description_Fr { get; set; }  
        public string Description_DE { get; set; }
         public int ProjectId { get; set; }

        // 🟢 ترتيب الظهور - راجع MenuRepository.ApplyMenuOrder لمنطق الدمج الكامل. DefaultSortOrder/
        // UserSortOrder خام كما جاءا من القاعدة (لعرضهما/تعديلهما فى شاشة الترتيب)، SortRank هو الترتيب
        // الفعلى المحسوب المُستخدَم فعلياً للعرض فى الشاشة الرئيسية والقائمة الجانبية.
        public int? DefaultSortOrder { get; set; }
        public int? UserSortOrder { get; set; }
        public int SortRank { get; set; }
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

        // 🟢 تُستدعى بعد حفظ ترتيب جديد (شاشة ترتيب القوائم) لتحديث الشجرة فوراً فى الشاشة الرئيسية
        // والقائمة الجانبية بدون الحاجة لإعادة تسجيل الدخول - نفس قائمة الكائنات (UserAllowedMenus) تبقى
        // كما هى (فقط SortRank تغيَّرت)، وإعادة استخدام OnAuthChanged الموجودة أصلاً (كل المستمعين
        // المشتركين فيها بالفعل - NavMenuMud/MenuNode/Home - يكتفون باستدعاء StateHasChanged) توفّر ربط
        // حدث جديد مكرَّر بلا داعٍ.
        public void RefreshMenuOrder()
        {
            OnAuthChanged?.Invoke();
        }
    }
}