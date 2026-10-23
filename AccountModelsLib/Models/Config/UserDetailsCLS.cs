using AccountModelsLib.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace AccountModelsLib
{
    /// <summary>
    /// إضافة/تعديل المستخدمين (USER_DETAILES) عبر EF.
    /// بعد تحويل العمود USER_ID إلى Id أصبح Id مفتاحاً أساسياً غير Identity، فكان INSERT القديم (بدون Id) يفشل دائماً؛
    /// هنا يُولَّد Id بـ CallSnowflakeIdGenerate مثل باقى الجداول، ويُضاف صف صلاحية الفرع فى نفس SaveChanges (عملية واحدة).
    /// المستخدم المنفِّذ (actingUserId) يُمرَّر صراحة ولا يُقرأ من Common.User_Id حتى تصلح الدوال للويب والموبايل.
    /// </summary>
    public class UserDetailsCLS
    {
        private readonly operations _opp;

        public UserDetailsCLS()
        {
            _opp = new operations();
        }

        public UserDetailsCLS(operations opp)
        {
            _opp = opp;
        }

        public USER_DETAILE GetById(long id)
        {
            return _opp.db.USER_DETAILES.AsNoTracking().FirstOrDefault(u => u.Id == id);
        }

        public bool UserNameExists(string userName, long excludeId)
        {
            userName = (userName ?? "").Trim();
            return _opp.db.USER_DETAILES.Any(u => u.USER_NAME == userName && u.Id != excludeId);
        }

        public void AddUser(ref int Excp, ref string Msg, USER_DETAILE user, long actingUserId)
        {
            Excp = 0;
            Msg = "";
            try
            {
                user.USER_NAME = (user.USER_NAME ?? "").Trim();
                if (UserNameExists(user.USER_NAME, 0))
                {
                    Excp = 1;
                    Msg = "اسم المستخدم موجود من قبل، من فضلك ادخل اسم مستخدم آخر";
                    return;
                }

                DateTime now = DateTime.Now;
                user.Id = CallSnowflakeIdGenerate.GenerateNewId();
                user.OperationDate = now;
                user.Insert_ServerDateTime = now;
                _opp.db.USER_DETAILES.Add(user);

                if (user.Branch_Id.HasValue && user.Branch_Id.Value > 0)
                {
                    _opp.db.USER_DETAILES_Branches_Permission.Add(NewBranchPermission(user.Id, user.Branch_Id.Value, actingUserId, now));
                }

                _opp.db.SaveChanges();
            }
            catch (Exception ex)
            {
                Excp = 1;
                Msg = ex.InnerException?.Message ?? ex.Message;
            }
        }

        public void UpdateUser(ref int Excp, ref string Msg, USER_DETAILE input, long actingUserId)
        {
            Excp = 0;
            Msg = "";
            try
            {
                var user = _opp.db.USER_DETAILES.FirstOrDefault(u => u.Id == input.Id);
                if (user == null)
                {
                    Excp = 1;
                    Msg = "المستخدم غير موجود";
                    return;
                }

                DateTime now = DateTime.Now;
                user.Name = input.Name;
                user.USER_PASSWORD = input.USER_PASSWORD;
                user.Branch_Id = input.Branch_Id;
                user.Active = input.Active;
                user.AdminFlage = input.AdminFlage;
                user.Account_Proj = input.Account_Proj;
                user.HR_Proj = input.HR_Proj;
                user.Store_Proj = input.Store_Proj;
                user.Production_Proj = input.Production_Proj;
                user.Gawda_Proj = input.Gawda_Proj;
                user.EditOperationDate = now;
                user.Update_ServerDateTime = now;
                user.Edit_User_Id = actingUserId;

                // الفرع الأساسى الجديد يجب أن يكون ضمن فروع المستخدم المسموحة وإلا لا يستطيع الدخول عليه
                if (user.Branch_Id.HasValue && user.Branch_Id.Value > 0)
                {
                    long branchId = user.Branch_Id.Value;
                    bool hasPermission = _opp.db.USER_DETAILES_Branches_Permission.Any(p => p.USER_DETAILES_Id == user.Id && p.Branch_Id == branchId);
                    if (!hasPermission)
                    {
                        _opp.db.USER_DETAILES_Branches_Permission.Add(NewBranchPermission(user.Id, branchId, actingUserId, now));
                    }
                }

                _opp.db.SaveChanges();
            }
            catch (Exception ex)
            {
                Excp = 1;
                Msg = ex.InnerException?.Message ?? ex.Message;
            }
        }

        private static USER_DETAILES_Branches_Permission NewBranchPermission(long userId, long branchId, long actingUserId, DateTime now)
        {
            return new USER_DETAILES_Branches_Permission
            {
                ID = CallSnowflakeIdGenerate.GenerateNewId(),
                USER_DETAILES_Id = userId,
                Branch_Id = branchId,
                User_Id = actingUserId,
                OperationDate = now,
                Insert_ServerDateTime = now
            };
        }
    }
}
