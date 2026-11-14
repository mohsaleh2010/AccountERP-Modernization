using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib
{
    /// <summary>مستخدم مبنىّ على الخادم بلا دائرة (P8.1 ق11): نفس ما يبنيه الدخول ثم AssistantSessionService.BuildUser().</summary>
    public sealed class AiServerUser
    {
        public long UserId { get; set; }
        public bool Found { get; set; }
        public bool Active { get; set; }
        public string UserName { get; set; } = "";
        /// <summary>جلسة المستخدم لتشغيل كود المحركات القديمة (Common.User_Id …) داخل UserSessionContext.Use.</summary>
        public UserSession Session { get; set; }
        public AiUserContext User { get; set; }
        /// <summary>مفاتيح القوائم المسموحة (KeysData) — للتحقق بمفتاح القائمة كما يفعل HasMenu فى الواجهة.</summary>
        public List<string> AllowedMenuKeys { get; set; } = new List<string>();
        /// <summary>صلاحية الفواتير الضريبية/غير الضريبية: -1 الكل، 1، 0 (Common.PermissionToShowTaxInvoiceOrNotORAll داخل جلسته).</summary>
        public int TaxFilter { get; set; } = -1;

        /// <summary>موجود ومفعّل.</summary>
        public bool Usable => Found && Active && User != null;

        public bool HasRoute(string route) =>
            User != null && (User.IsAdmin || User.AllowedRoutes.Any(r => string.Equals(r, route, StringComparison.OrdinalIgnoreCase)));

        public bool HasMenuKey(string key) => AllowedMenuKeys.Any(k => string.Equals(k, key, StringComparison.OrdinalIgnoreCase));

        /// <summary>يشغّل كوداً بجلسة هذا المستخدم ثم يعيد الجلسة السابقة (Common.User_Id خارجها لا يتغير).</summary>
        public T Run<T>(Func<T> body)
        {
            using (UserSessionContext.Use(Session)) return body();
        }
    }

    /// <summary>
    /// P8.1 (ai-assistant-p8-plan.md §3): يبنى AiUserContext لمستخدم غير متصل (العامل المجدول، «تشغيل الآن»، P8.4/P8.10).
    /// يقرأ USER_DETAILES (قراءة فقط) ويبنى UserSession كما يفعل AuthService.PopulateSessionForUserAsync، ثم داخل UserSessionContext.Use:
    /// Common.Get_Is_Force_Admin_Permission() ثم MenuRepository.GetUserAllowedMenus(2, userId) ثم صلاحية الفواتير الضريبية.
    /// الفحص المفتوح 5: كلها تأخذ المستخدم من الجلسة أو من المعامل؛ الأثر الثابت الوحيد Common.Is_Ecrypted_Config = 1 (نفس القيمة لكل المستخدمين).
    /// </summary>
    public static class AiUserContextFactory
    {
        public const int ProjectId = 2;

        public static AiServerUser ForUser(long userId)
        {
            var result = new AiServerUser { UserId = userId };
            if (userId <= 0) return result;

            var row = ReadUser(userId);
            if (row == null) return result;
            result.Found = true;
            result.Active = row.Value.Active;
            result.UserName = row.Value.UserName;

            var session = new UserSession
            {
                Is_User_Admin = row.Value.AdminFlage,
                User_Id = userId,
                Branch_Id = row.Value.BranchId ?? 1,
                User_Name = row.Value.UserName,
                FullUserName = row.Value.FullName,
                IsAdministration_AlterDBLogin = 0,
            };
            result.Session = session;
            if (!result.Active) return result;

            using (UserSessionContext.Use(session))
            {
                try { Common.Get_Is_Force_Admin_Permission(); } catch (Exception ee) { operations.DataDictionaryLogerException(ee); }
                var menus = new MenuRepository().GetUserAllowedMenus(ProjectId, userId) ?? new List<MenuViewModel>();
                result.User = Build(userId, session, menus);
                result.AllowedMenuKeys = menus.Select(m => (m.KeysData ?? "").Trim()).Where(k => k.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                try { result.TaxFilter = Common.PermissionToShowTaxInvoiceOrNotORAll(); }
                catch (Exception ee) { operations.DataDictionaryLogerException(ee); result.TaxFilter = 0; }
            }
            return result;
        }

        /// <summary>نفس شكل AssistantSessionService.BuildUser() حرفياً (المسارات والشاشات من القوائم ذات RazorPath).</summary>
        public static AiUserContext Build(long userId, UserSession session, List<MenuViewModel> allowedMenus)
        {
            var menus = (allowedMenus ?? new List<MenuViewModel>()).Where(m => !string.IsNullOrWhiteSpace(m.RazorPath)).ToList();
            return new AiUserContext
            {
                UserId = userId,
                BranchId = session?.Branch_Id ?? 0,
                IsAdmin = (session?.Is_User_Admin ?? 0) == 1,
                AllowedRoutes = menus.Select(m => m.RazorPath.Trim()).Distinct().ToList(),
                AllowedScreens = menus.Select(m => new AiScreenRef { Path = m.RazorPath.Trim(), Title = (m.Description ?? "").Trim() })
                                      .GroupBy(x => x.Path, StringComparer.OrdinalIgnoreCase).Select(g => g.First()).ToList(),
            };
        }

        private static (string UserName, string FullName, long? BranchId, int AdminFlage, bool Active)? ReadUser(long userId)
        {
            (string, string, long?, int, bool)? found = null;
            var db = new operations().db;
            var conn = db.Database.GetDbConnection();
            bool wasClosed = conn.State != ConnectionState.Open;
            if (wasClosed) conn.Open();
            try
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT USER_NAME, Name, Branch_Id, ISNULL(AdminFlage, 0), CAST(ISNULL(Active, 0) AS bit) FROM USER_DETAILES WHERE Id = @u";
                    var p = cmd.CreateParameter();
                    p.ParameterName = "@u";
                    p.Value = userId;
                    cmd.Parameters.Add(p);
                    using (DbDataReader r = cmd.ExecuteReader())
                        if (r.Read())
                            found = (r.IsDBNull(0) ? "" : Convert.ToString(r.GetValue(0)),
                                     r.IsDBNull(1) ? "" : Convert.ToString(r.GetValue(1)),
                                     r.IsDBNull(2) ? (long?)null : Convert.ToInt64(r.GetValue(2)),
                                     Convert.ToInt32(r.GetValue(3)),
                                     Convert.ToBoolean(r.GetValue(4)));
                }
            }
            finally { if (wasClosed) conn.Close(); }
            return found;
        }
    }
}
