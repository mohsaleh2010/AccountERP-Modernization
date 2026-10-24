using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace AccountModelsLib.Models
{
    /// <summary>
    /// دور الحساب المقابل لحساب المخزن فى قيد الحركة المخزنية (مثل جدول تحديد الحسابات فى SAP وحسابات تقييم المخزون فى Odoo).
    /// حساب المخزن نفسه هو حسابه فى الشجرة (Store.SubAccount_Id)؛ هذا الدور يحدد الطرف الآخر من القيد.
    /// القيم مخزَّنة فى Store_Account_Determination.Account_Role فلا يُعاد ترقيمها.
    /// </summary>
    public enum StoreAccountRole
    {
        GRIR = 1,              // استلام من مورد ومرتجعه (1004/1005): بضاعة مستلمة غير مفوترة
        COGS = 2,              // صرف لعميل ومرتجعه وفواتير البيع (1002/1003): تكلفة البضاعة المباعة
        AdjustmentGain = 3,    // تسوية بالزيادة (1007)
        AdjustmentLoss = 4,    // تسوية بالنقص (1008)
        WIP = 5,               // صرف للإنتاج واستلام منه (1010/1009): إنتاج تحت التشغيل
        Scrap = 6,             // صرف للهالك (1013)
        Transit = 7,           // التحويل بين المخازن والمخازن الخارجية (1001/1006/1014-1017): بضاعة بالطريق
        Consumption = 8,       // صرف قطع غيار وللورشة والماكينات والموظفين ومرتجعاتها (1012/1018-1023)
        PurchasePriceVariance = 9, // فرق سعر الشراء عن سعر الاستلام
        OpeningBalance = 10,   // رصيد أول المدة للأصناف
    }

    [Table("Store_Account_Determination")]
    public class Store_Account_Determination
    {
        public long Id { get; set; }
        public long Account_Branch_Id { get; set; }
        public int StoreUsed_Type { get; set; }   // 0 = كل أنواع المخازن (افتراضى الفرع)
        public long Store_Id { get; set; }        // 0 = ليس تخصيصاً لمخزن بعينه
        public int Account_Role { get; set; }
        public long SubAccount_Id { get; set; }
        public DateTime? OperationDate { get; set; }
        public long? User_Id { get; set; }
        public long? Branch_Id { get; set; }
        public DateTime? Insert_ServerDateTime { get; set; }
        public DateTime? Update_ServerDateTime { get; set; }
        public DateTime? EditOperationDate { get; set; }
        public long? Edit_User_Id { get; set; }
    }

    /// <summary>وصف دور: الاسم، نوع الحساب المقترح فى الشجرة (0 = بلا نوع)، والأذون التى تستخدمه.</summary>
    public class StoreAccountRoleInfo
    {
        public StoreAccountRole Role { get; set; }
        public string Name { get; set; } = "";
        public int SuggestedAccountType { get; set; }
        public string OrderTypes { get; set; } = "";
    }

    /// <summary>صف إعداد كما تعرضه الشاشة: المستوى (فرع/نوع مخزن/مخزن) + الدور + الحساب.</summary>
    public class StoreAccountSetting
    {
        public long Id { get; set; }
        public int StoreUsed_Type { get; set; }
        public long Store_Id { get; set; }
        public int Account_Role { get; set; }
        public long SubAccount_Id { get; set; }
        public string SubAccount_Name { get; set; } = "";
        public string SubAccount_NO { get; set; } = "";
    }

    /// <summary>
    /// تحديد الحسابات المقابلة للحركات المخزنية. الحساب لكل دور يُبحث عنه بالترتيب: تخصيص المخزن نفسه، ثم نوع المخزن، ثم افتراضى الفرع.
    /// القيود الفعلية (الأذون والفواتير) لا تقرأ هذا الجدول بعد - ربطها مرحلة لاحقة منفصلة.
    /// </summary>
    public class StoreAccountDeterminationCLS
    {
        public static readonly IReadOnlyList<StoreAccountRoleInfo> Roles = new List<StoreAccountRoleInfo>
        {
            new StoreAccountRoleInfo { Role = StoreAccountRole.GRIR, Name = "بضاعة مستلمة غير مفوترة", SuggestedAccountType = Common.Account_Type_ForGRIR, OrderTypes = "1004,1005" },
            new StoreAccountRoleInfo { Role = StoreAccountRole.COGS, Name = "تكلفة البضاعة المباعة", SuggestedAccountType = Common.Account_Type_ForCOGS, OrderTypes = "1002,1003" },
            new StoreAccountRoleInfo { Role = StoreAccountRole.AdjustmentGain, Name = "تسوية الجرد بالزيادة", SuggestedAccountType = Common.Account_Type_ForInventoryAdjustment, OrderTypes = "1007" },
            new StoreAccountRoleInfo { Role = StoreAccountRole.AdjustmentLoss, Name = "تسوية الجرد بالنقص", SuggestedAccountType = Common.Account_Type_ForInventoryAdjustment, OrderTypes = "1008" },
            new StoreAccountRoleInfo { Role = StoreAccountRole.WIP, Name = "إنتاج تحت التشغيل", SuggestedAccountType = Common.Account_Type_ForWIP, OrderTypes = "1009,1010" },
            new StoreAccountRoleInfo { Role = StoreAccountRole.Scrap, Name = "الهالك", SuggestedAccountType = 0, OrderTypes = "1013" },
            new StoreAccountRoleInfo { Role = StoreAccountRole.Transit, Name = "بضاعة بالطريق (تحويلات)", SuggestedAccountType = 0, OrderTypes = "1001,1006,1014,1015,1016,1017" },
            new StoreAccountRoleInfo { Role = StoreAccountRole.Consumption, Name = "مصروف الاستهلاك (قطع غيار/ورشة/ماكينات/موظفين)", SuggestedAccountType = 0, OrderTypes = "1012,1018,1019,1020,1021,1022,1023" },
            new StoreAccountRoleInfo { Role = StoreAccountRole.PurchasePriceVariance, Name = "فروق أسعار الشراء", SuggestedAccountType = Common.Account_Type_ForPurchasePriceVariance, OrderTypes = "" },
            new StoreAccountRoleInfo { Role = StoreAccountRole.OpeningBalance, Name = "أرصدة أول المدة", SuggestedAccountType = Common.Account_Type_ForOpeningBalanceEquity, OrderTypes = "" },
        };

        /// <summary>كل إعدادات الفرع (كل المستويات) مع اسم الحساب وكوده.</summary>
        public List<StoreAccountSetting> GetSettings(long branchId)
        {
            var db = new operations().db;
            return (from d in db.Store_Account_Determination.AsNoTracking()
                    where d.Account_Branch_Id == branchId
                    join a in db.Submain_Account.AsNoTracking() on d.SubAccount_Id equals a.Id into aj
                    from a in aj.DefaultIfEmpty()
                    select new StoreAccountSetting
                    {
                        Id = d.Id,
                        StoreUsed_Type = d.StoreUsed_Type,
                        Store_Id = d.Store_Id,
                        Account_Role = d.Account_Role,
                        SubAccount_Id = d.SubAccount_Id,
                        SubAccount_Name = a != null ? a.SubAccount_Name : "",
                        SubAccount_NO = a != null ? a.SubAccount_NO : "",
                    }).ToList();
        }

        /// <summary>الحساب المقابل لحركة مخزن: تخصيص المخزن، ثم نوعه، ثم افتراضى الفرع. 0 = غير محدد.</summary>
        public long ResolveAccount(long branchId, long storeId, int storeUsedType, StoreAccountRole role, operations opp = null)
        {
            // opp: معاملة المستدعى (الترحيل)؛ بدونه اتصال مستقل
            var rows = (opp ?? new operations()).db.Store_Account_Determination.AsNoTracking()
                .Where(d => d.Account_Branch_Id == branchId && d.Account_Role == (int)role
                         && ((d.Store_Id == storeId && storeId != 0) || (d.Store_Id == 0 && (d.StoreUsed_Type == storeUsedType || d.StoreUsed_Type == 0))))
                .ToList();
            var hit = rows.FirstOrDefault(d => d.Store_Id != 0)
                   ?? rows.FirstOrDefault(d => d.StoreUsed_Type != 0)
                   ?? rows.FirstOrDefault();
            return hit?.SubAccount_Id ?? 0;
        }

        /// <summary>
        /// حفظ حساب دور فى مستوى واحد (الفرع: النوع 0 والمخزن 0، أو نوع مخزن، أو مخزن بعينه). SubAccount_Id = 0 يلغى الإعداد فيعود للمستوى الأعم.
        /// Excp: 0 نجاح، 2 حساب غير صالح (غير موجود أو له حسابات فرعية)، غير ذلك خطأ (Msg).
        /// </summary>
        public void Save(ref int Excp, ref string Msg, long branchId, int storeUsedType, long storeId, StoreAccountRole role, long subAccountId)
        {
            Excp = 0;
            Msg = "";
            try
            {
                var opp = new operations();
                if (storeId != 0) storeUsedType = 0;   // تخصيص المخزن لا يتبع نوعاً
                if (subAccountId != 0)
                {
                    if (!opp.db.Submain_Account.AsNoTracking().Any(a => a.Id == subAccountId))
                    {
                        Excp = 2;
                        Msg = "الحساب غير موجود";
                        return;
                    }
                    if (opp.db.Submain_Account.AsNoTracking().Any(a => a.Main_Account_Id == subAccountId))
                    {
                        Excp = 2;
                        Msg = "اختر حساباً فرعياً (بلا حسابات تحته) ليُرحَّل عليه القيد";
                        return;
                    }
                }
                var existing = opp.db.Store_Account_Determination.FirstOrDefault(d => d.Account_Branch_Id == branchId && d.StoreUsed_Type == storeUsedType
                                                                                   && d.Store_Id == storeId && d.Account_Role == (int)role);
                if (subAccountId == 0)
                {
                    if (existing != null)
                    {
                        opp.db.Store_Account_Determination.Remove(existing);
                        opp.db.SaveChanges();
                    }
                    return;
                }
                if (existing == null)
                {
                    opp.db.Store_Account_Determination.Add(new Store_Account_Determination
                    {
                        Id = CallSnowflakeIdGenerate.GenerateNewId(),
                        Account_Branch_Id = branchId,
                        StoreUsed_Type = storeUsedType,
                        Store_Id = storeId,
                        Account_Role = (int)role,
                        SubAccount_Id = subAccountId,
                        OperationDate = DateTime.Now,
                        User_Id = Common.User_Id,
                        Branch_Id = Common.Branch_Id,
                        Insert_ServerDateTime = DateTime.Now,
                    });
                }
                else
                {
                    existing.SubAccount_Id = subAccountId;
                    existing.Update_ServerDateTime = DateTime.Now;
                    existing.EditOperationDate = DateTime.Now;
                    existing.Edit_User_Id = Common.User_Id;
                }
                opp.db.SaveChanges();
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.InnerException != null ? ee.InnerException.Message : ee.Message;
            }
        }

        /// <summary>
        /// اقتراح حساب الدور من نوع الحساب فى الشجرة (Account_Customer_Type_Value): يُقترح فقط لو وُجد حساب فرعى واحد بالضبط من هذا النوع فى الفرع.
        /// 0 = لا اقتراح (لا نوع للدور، أو لا حساب، أو أكثر من حساب فيختار المستخدم).
        /// </summary>
        public long SuggestAccount(long branchId, StoreAccountRole role)
        {
            int type = Roles.FirstOrDefault(r => r.Role == role)?.SuggestedAccountType ?? 0;
            if (type == 0) return 0;
            var db = new operations().db;
            var ids = db.Submain_Account.AsNoTracking()
                .Where(a => a.Account_Customer_Type_Value == type && a.Branch_Id == branchId
                         && !db.Submain_Account.Any(c => c.Main_Account_Id == a.Id))
                .Select(a => a.Id).Take(2).ToList();
            return ids.Count == 1 ? ids[0] : 0;
        }
    }
}
