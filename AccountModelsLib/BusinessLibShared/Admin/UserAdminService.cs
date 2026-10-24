using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using AccountModelsLib.Models;

namespace AccountModelsLib
{
    /// <summary>صف مستخدم كما تعرضه شاشة المستخدمين - بلا كلمة المرور.</summary>
    public class UserAdminRow
    {
        public long Id { get; set; }
        public string Name { get; set; } = "";
        public string USER_NAME { get; set; } = "";
        public bool IsAdmin { get; set; }
        public bool Active { get; set; }
        public long Branch_Id { get; set; }
        public List<long> Branches { get; set; } = new();
    }

    /// <summary>بيانات الحفظ: Password فارغة عند التعديل = تبقى كما هى.</summary>
    public class UserAdminInput
    {
        public long Id { get; set; }
        public string Name { get; set; } = "";
        public string USER_NAME { get; set; } = "";
        public string Password { get; set; } = "";
        public bool IsAdmin { get; set; }
        public bool Active { get; set; } = true;
        public long Branch_Id { get; set; }
        public List<long> Branches { get; set; } = new();
        public long CopyPermissionsFrom { get; set; }   // للمستخدم الجديد فقط: 0 = الافتراضى من SysConfigMenus.Value
    }

    /// <summary>
    /// المستخدمون وفروعهم المسموحة (خطة 05 - المرحلة 7) بدل UsersList / ADDNEWUSER / Branch_UserPermission.
    /// الإضافة والتعديل عبر UserDetailsCLS (Id بـ Snowflake). كلمة المرور تبقى نصاً صريحاً لأن دخول الديسك توب والويب يقارنها كذلك
    /// (docs/known-constraints.md)، ولا تُعرض أبداً؛ ولا تُقبل كلمة مرور يستخدمها مستخدم آخر (نفس شرط AuthService.ChangePassword).
    /// لا حذف للمستخدمين - إيقاف فقط (له قيود وحركات باسمه)، ولا يبقى النظام بلا مدير نشط ولا يوقف المستخدم نفسه.
    /// </summary>
    public class UserAdminService
    {
        private readonly Func<operations> _newOpp;

        public UserAdminService(Func<operations> newOpp = null)
        {
            _newOpp = newOpp ?? (() => new operations());
        }

        public List<UserAdminRow> GetUsers()
        {
            var db = _newOpp().db;
            var branches = db.USER_DETAILES_Branches_Permission.AsNoTracking()
                .Select(p => new { p.USER_DETAILES_Id, p.Branch_Id }).ToList()
                .GroupBy(p => p.USER_DETAILES_Id ?? 0).ToDictionary(g => g.Key, g => g.Select(x => x.Branch_Id ?? 0).Distinct().ToList());
            return db.USER_DETAILES.AsNoTracking().ToList().Select(u => new UserAdminRow
            {
                Id = u.Id,
                Name = u.Name ?? "",
                USER_NAME = u.USER_NAME ?? "",
                IsAdmin = (u.AdminFlage ?? 0) == 1,
                Active = u.Active ?? false,
                Branch_Id = u.Branch_Id ?? 0,
                Branches = branches.TryGetValue(u.Id, out var b) ? b : new List<long>(),
            }).OrderBy(u => u.Active ? 0 : 1).ThenBy(u => u.Name).ToList();
        }

        public List<(long Id, string Name)> GetBranches() =>
            _newOpp().db.Account_Branches.AsNoTracking().Where(b => b.Id != null).ToList()
                .Select(b => (b.Id.Value, b.Branch_Name ?? "")).OrderBy(b => b.Item1).ToList();

        /// <summary>إضافة أو تعديل. Excp: 0 نجاح، 2 مرفوض (Msg)، 1 خطأ. يرجع Id المستخدم.</summary>
        public long Save(ref int Excp, ref string Msg, UserAdminInput input, long actingUserId)
        {
            Excp = 0;
            Msg = "";
            try
            {
                var opp = _newOpp();
                var db = opp.db;
                input.USER_NAME = (input.USER_NAME ?? "").Trim();
                input.Name = (input.Name ?? "").Trim();
                input.Password ??= "";
                var branches = (input.Branches ?? new()).Where(b => b > 0).Distinct().ToList();
                if (input.Branch_Id > 0 && !branches.Contains(input.Branch_Id)) branches.Add(input.Branch_Id);

                if (input.USER_NAME == "") return Reject(ref Excp, ref Msg, "اكتب اسم الدخول");
                if (input.Branch_Id <= 0) return Reject(ref Excp, ref Msg, "اختر الفرع الافتراضى");
                bool isNew = input.Id == 0;
                if (isNew && input.Password == "") return Reject(ref Excp, ref Msg, "اكتب كلمة المرور");
                if (input.Password != "" && input.Password.Length < 4) return Reject(ref Excp, ref Msg, "كلمة المرور 4 أحرف على الأقل");
                if (db.USER_DETAILES.AsNoTracking().Any(u => u.USER_NAME == input.USER_NAME && u.Id != input.Id))
                    return Reject(ref Excp, ref Msg, "اسم الدخول مستخدم من قبل");
                if (input.Password != "" && db.USER_DETAILES.AsNoTracking().Any(u => u.USER_PASSWORD == input.Password && u.Id != input.Id))
                    return Reject(ref Excp, ref Msg, "اختر كلمة مرور أخرى");

                USER_DETAILE existing = null;
                if (!isNew)
                {
                    existing = db.USER_DETAILES.AsNoTracking().FirstOrDefault(u => u.Id == input.Id);
                    if (existing == null) return Reject(ref Excp, ref Msg, "المستخدم غير موجود");
                    if (input.Id == actingUserId && (!input.Active || (existing.AdminFlage == 1 && !input.IsAdmin)))
                        return Reject(ref Excp, ref Msg, "لا يمكنك إيقاف حسابك أو سحب صلاحية المدير منه");
                    bool wasActiveAdmin = existing.AdminFlage == 1 && existing.Active == true;
                    if (wasActiveAdmin && (!input.IsAdmin || !input.Active)
                        && !db.USER_DETAILES.AsNoTracking().Any(u => u.Id != input.Id && u.AdminFlage == 1 && u.Active == true))
                        return Reject(ref Excp, ref Msg, "يجب أن يبقى مدير نشط واحد على الأقل");
                }

                var user = new USER_DETAILE
                {
                    Id = input.Id,
                    Name = input.Name == "" ? input.USER_NAME : input.Name,
                    USER_NAME = input.USER_NAME,
                    USER_PASSWORD = input.Password != "" ? input.Password : existing?.USER_PASSWORD,
                    Branch_Id = input.Branch_Id,
                    AdminFlage = input.IsAdmin ? 1 : 0,
                    Active = input.Active,
                    Account_Proj = existing?.Account_Proj ?? 1,
                    HR_Proj = existing?.HR_Proj ?? 0,
                    Store_Proj = existing?.Store_Proj ?? 1,
                    Production_Proj = existing?.Production_Proj ?? 0,
                    Gawda_Proj = existing?.Gawda_Proj ?? 0,
                };
                var cls = new UserDetailsCLS(opp);
                if (isNew) cls.AddUser(ref Excp, ref Msg, user, actingUserId);
                else cls.UpdateUser(ref Excp, ref Msg, user, actingUserId);
                if (Excp != 0) return 0;

                SetBranches(opp, user.Id, branches, actingUserId);
                if (isNew) new MenuPermissionService(_newOpp).SeedNewUser(user.Id, input.CopyPermissionsFrom, actingUserId);
                return user.Id;
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.InnerException != null ? ee.InnerException.Message : ee.Message;
                return 0;
            }
        }

        private static long Reject(ref int Excp, ref string Msg, string message)
        {
            Excp = 2;
            Msg = message;
            return 0;
        }

        // الفروع المسموحة: يُضاف الناقص ويُحذف ما أُلغى فقط (لا حذف ثم إعادة إدراج كالديسك توب).
        private static void SetBranches(operations opp, long userId, List<long> branches, long actingUserId)
        {
            var current = opp.db.USER_DETAILES_Branches_Permission.Where(p => p.USER_DETAILES_Id == userId).ToList();
            foreach (var p in current.Where(p => !branches.Contains(p.Branch_Id ?? 0)))
                opp.db.USER_DETAILES_Branches_Permission.Remove(p);
            var now = DateTime.Now;
            foreach (long b in branches.Where(b => !current.Any(p => p.Branch_Id == b)))
                opp.db.USER_DETAILES_Branches_Permission.Add(new USER_DETAILES_Branches_Permission
                {
                    ID = CallSnowflakeIdGenerate.GenerateNewId(),
                    USER_DETAILES_Id = userId,
                    Branch_Id = b,
                    User_Id = actingUserId,
                    OperationDate = now,
                    Insert_ServerDateTime = now,
                });
            opp.db.SaveChanges();
        }
    }
}
