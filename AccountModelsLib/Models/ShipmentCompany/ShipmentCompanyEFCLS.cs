using AccountModelsLib;
using AccountModelsLib.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
namespace AccountModelsLib.Services
{
    /// <summary>
    /// شركات الشحن = موردو خدمات فى جدول Supplier بعلامة IS_ShipmentCompany = 1 (قرار المستخدم 2026-10-23، مثل Odoo/SAP)،
    /// فتستفيد من كشف حساب المورد ومدفوعاته وأرصدته. حساب كل شركة من النوع 65 (Account_Type_ForShippmentCompanyAsServicesSupplier)
    /// تحت الحساب الأب «شركات الشحن» (يُنشأ تحت حساب الموردين عند أول حفظ لو غير موجود).
    /// كل إضافة/تعديل (شاشة الويب، الإضافة المتعددة، شبكة الويندوز فورمز) تمر على Save هنا.
    /// جدول ShipmentCompany القديم لا يُكتب فيه ولا يُحذف منه؛ صفوفه تُستورد بزر صريح (ImportLegacy).
    /// </summary>
    public class ShipmentCompanyEFCLS
    {
        public const string ParentAccountName = "شركات الشحن";

        private operations _opp;

        public ShipmentCompanyEFCLS(operations opp)
        {
            _opp = opp;
        }
        public ShipmentCompanyEFCLS()
        {
            _opp = new operations();
        }

        private static int AccountType => Common.Account_Type_ForShippmentCompanyAsServicesSupplier;

        /// <summary>شركات الشحن النشطة (بنفس شكل ShipmentCompany الذى تستخدمه الشاشات).</summary>
        public List<ShipmentCompany> GetAll()
        {
            return new operations().db.Suppliers.AsNoTracking()
                .Where(s => s.IS_ShipmentCompany == 1 && s.Active == 1)
                .OrderBy(s => s.Name)
                .Select(s => new ShipmentCompany { Id = s.Id, Name = s.Name, SubAccount_Id = s.SubAccount_Id ?? 0 })
                .ToList();
        }

        public Task<List<ShipmentCompany>> GetAllAsync() => Task.FromResult(GetAll());

        private static bool NameExists(operations opp, string name, long exceptId)
        {
            // الاسم فريد فى جدول Supplier كله (نفس قاعدة SupplierCLS.IsFoundName) لأن الشركة مورد أيضاً.
            var names = opp.db.Suppliers.AsNoTracking().Where(s => s.Id != exceptId && s.Active == 1).Select(s => s.Name).ToList();
            return names.Any(n => string.Equals((n ?? "").Trim(), name, StringComparison.CurrentCultureIgnoreCase));
        }

        /// <summary>
        /// إضافة (Id = 0) أو تعديل اسم شركة شحن. Excp: 0 نجاح، 2 اسم فارغ، 3 اسم مكرر، 4 لا يوجد حساب أب للموردين، غير ذلك خطأ (Msg).
        /// الإضافة: مورد بعلامة شركة شحن + حساب من النوع 65 فى معاملة واحدة. التعديل: اسم المورد واسم حسابه معاً.
        /// </summary>
        public long Save(ref int Excp, ref string Msg, long Id, string Name)
        {
            Excp = 0;
            Msg = "";
            Name = (Name ?? "").Trim();
            if (Name == "")
            {
                Excp = 2;
                Msg = "اسم شركة الشحن مطلوب";
                return 0;
            }
            var check = new operations();
            if (NameExists(check, Name, Id))
            {
                Excp = 3;
                Msg = "الاسم مستخدم من قبل لمورد أو شركة شحن";
                return 0;
            }
            return Id == 0 ? Add(ref Excp, ref Msg, Name) : Rename(ref Excp, ref Msg, Id, Name);
        }

        private long Add(ref int Excp, ref string Msg, string Name)
        {
            long branchId = Common.Branch_Id;
            if (EnsureParentAccount(ref Excp, ref Msg, branchId) == 0) return 0;

            var opp = new operations(2);
            try
            {
                var sp = new Supplier
                {
                    Name = Name,
                    Code = "",
                    IS_ShipmentCompany = 1,
                    Supplier_Type = Common.Customer_Type,
                    Currency_Id = 1,
                    Group_Id = 0,
                    Ref_Cust_Supplier_Id = 0,
                    Balance = 0,
                    Balance_Actual = 0,
                    Active = 1,
                    OperationDate = DateTime.Now,
                    Insert_ServerDateTime = DateTime.Now,
                    User_Id = Common.User_Id,
                    Branch_Id = branchId,
                };
                var acc = new Sub_Account();
                acc.spCls.sp = sp;
                long newAccountId = -1;
                string accountCode = "";
                acc.Add_AccountSupplierCustomerOther(ref Excp, ref Msg, ref opp, ref newAccountId, ref accountCode, AccountType, Name, branchId, 1);
                if (Excp == 0)
                    acc.UpdateSubmain_SubAccount_Id(ref Excp, ref opp, ref Msg);
                if (Excp == 0)
                {
                    opp.Commit();
                    return sp.Id;
                }
                opp.Rollback();
                return 0;
            }
            catch (Exception ee)
            {
                try { opp.Rollback(); } catch { }
                Excp = 1;
                Msg = ee.InnerException != null ? ee.InnerException.Message : ee.Message;
                return 0;
            }
        }

        private long Rename(ref int Excp, ref string Msg, long Id, string Name)
        {
            try
            {
                var opp = new operations();
                var sp = opp.db.Suppliers.FirstOrDefault(s => s.Id == Id && s.IS_ShipmentCompany == 1);
                if (sp == null)
                {
                    Excp = 1;
                    Msg = "السجل غير موجود أو تم حذفه.";
                    return 0;
                }
                if (sp.Name == Name) return Id;
                sp.Name = Name;
                sp.Update_ServerDateTime = DateTime.Now;
                sp.EditOperationDate = DateTime.Now;
                sp.Edit_User_Id = Common.User_Id;
                var account = (sp.SubAccount_Id ?? 0) > 0 ? opp.db.Submain_Account.FirstOrDefault(a => a.Id == sp.SubAccount_Id) : null;
                if (account != null) account.SubAccount_Name = Name;
                opp.db.SaveChanges();   // المورد وحسابه فى SaveChanges واحد (معاملة واحدة)
                return Id;
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.InnerException != null ? ee.InnerException.Message : ee.Message;
                return 0;
            }
        }

        /// <summary>
        /// حذف شركة شحن: يُرفض لو مستخدمة فى فواتير أو على حسابها قيود (نفس فحص المورد)، وإلا يُحذف المورد وحسابه
        /// بمسار حذف المورد نفسه (حذف فعلى أو إيقاف حسب Update_DeleteSupplier).
        /// </summary>
        public void Delete(ref int Excp, ref string Msg, long Id)
        {
            Excp = 0;
            Msg = "";
            try
            {
                var opp = new operations();
                var sp = opp.db.Suppliers.AsNoTracking().FirstOrDefault(s => s.Id == Id && s.IS_ShipmentCompany == 1);
                if (sp == null)
                {
                    Excp = 1;
                    Msg = "السجل غير موجود أو تم حذفه.";
                    return;
                }
                int used = opp.db.Database.SqlQuery<int>($@"
SELECT (SELECT COUNT(*) FROM Invoices_List WHERE ShipmentCompany_Id = {Id})
     + (SELECT COUNT(*) FROM Invoices_List_Reverse WHERE ShipmentCompany_Id = {Id}) AS Value").AsEnumerable().First();
                if (used > 0)
                {
                    Excp = 10;
                    Msg = "لا يمكن حذف شركة الشحن لاستخدامها فى " + used + " فاتورة";
                    return;
                }
                new SupplierCLS().DeleteSupplier(ref Excp, ref Msg, Id);
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.InnerException != null ? ee.InnerException.Message : ee.Message;
            }
        }

        /// <summary>
        /// الحساب الأب لشركات الشحن فى الفرع (صف Account_Type_Main_Account_Id للنوع 65). لو غير معرَّف يُنشأ حساب «شركات الشحن»
        /// تحت حساب الموردين (النوع 5) ويُسجَّل صراحةً أباً للنوع. يرجع 0 مع Excp = 4 لو لا يوجد حساب أب للموردين أصلاً.
        /// يُقرأ صف الربط مباشرة (لا البحث الاحتياطى فى Get_Account_Type_Code الذى يقبل أى حساب من النوع 65 بلا مورد).
        /// </summary>
        public long EnsureParentAccount(ref int Excp, ref string Msg, long branchId)
        {
            var opp = new operations();
            long parentId = MappedParent(opp, branchId);
            if (parentId > 0) return parentId;
            int costCenter = 0;

            long suppliersParent = 0;
            Common.Get_Account_Type_Code(ref opp, Common.Supplier_Account_Type, 0, ref suppliersParent, ref costCenter, branchId);
            var parent = suppliersParent > 0 ? opp.db.Submain_Account.AsNoTracking().FirstOrDefault(a => a.Id == suppliersParent) : null;
            if (parent == null)
            {
                Excp = 4;
                Msg = "حدد أولاً الحساب الأب للموردين من «ربط الحسابات الرئيسية» ليُنشأ تحته حساب شركات الشحن";
                return 0;
            }

            long lastPart = 0, code = 0;
            string accountCode = "";
            int level = 0;
            Common.MakeAutomatic_AccontCode(ref Excp, ref Msg, ref opp, parent.Id, ref lastPart, ref accountCode, ref code, ref level);
            var sa = new Submain_AccountCLS();
            sa.sb.Id = -1;
            sa.sb.Main_Account_Id = parent.Id;
            sa.sb.SubAccount_Name = ParentAccountName;
            sa.sb.SubAccount_NO = accountCode;
            sa.sb.FirstSubCode = parent.FirstSubCode;
            sa.sb.Code = code;
            sa.sb.Account_Level = level;
            sa.sb.lastPartIncrease = lastPart;
            sa.sb.Head_Main_Account = parent.Head_Main_Account;
            sa.sb.Currency_Id = parent.Currency_Id;
            sa.sb.CanDelete = 1;
            sa.sb.Account_Follows = parent.Account_Follows;
            sa.sb.CashFlowType = parent.CashFlowType;
            sa.sb.Notes = "";
            sa.sb.Accept_EntryNew_Flage = 0;
            sa.sb.Accept_Cost_Center_Flage = 0;
            sa.sb.Cost_Center_Tree_Id = 0;
            sa.sb.Account_Nature_Type = parent.Account_Nature_Type;
            // InsertSubmain_Account ينسخ نوع الحساب الجديد على أبيه (إلا «حسابات أخرى»)؛ نُنشئ الحساب التجميعى بنوع أبيه نفسه
            // فلا يتغير حساب الموردين، ثم نجعل نوعه 65 بعد الإنشاء (صف جديد خاص بنا).
            sa.sb.Account_Customer_Type_Value = parent.Account_Customer_Type_Value;
            sa.sb.IS_SubAccount_AccountType = 1;
            sa.sb.IS_SubAccount_FollowingAccount = 0;
            sa.sb.Branch_Id = -1;
            sa.sb.Is_MasrofatAffect = 0;
            sa.sb.Account_Type_ValueGeneralCustomer = 0;
            long newId = -1;
            long branch = -1;
            string newNo = accountCode;
            Excp = 0;
            // IS_Have_Child_Account = false: حساب تجميعى فلا يُنشأ له مورد مرتبط.
            new Add_Account_CodesCLS().AddNewAccount(ref Excp, ref Msg, sa, ref newId, ref branch, ref newNo, false);
            if (Excp != 0) return 0;

            opp = new operations();
            var header = newId > 0 ? opp.db.Submain_Account.FirstOrDefault(a => a.Id == newId && a.Main_Account_Id == parent.Id) : null;
            if (header == null)
            {
                Excp = 4;
                Msg = "تعذر إنشاء الحساب الأب لشركات الشحن";
                return 0;
            }
            header.Account_Customer_Type_Value = AccountType;
            opp.db.SaveChanges();
            string typeName = ParentAccountName;
            // Id صريح (Snowflake) حين لا يكون عموداً تلقائياً فى القاعدة - نفس قاعدة Common.Use_Auto_Identity_Column فى باقى الإدراجات.
            if (Common.Use_Auto_Identity_Column == 0)
                opp.db.Database.ExecuteSqlInterpolated($@"
INSERT INTO Account_Type_Main_Account_Id (Id, Account_Type_Value, Account_Type_Name, Account_Branch_Id, Main_Account_Id, Account_Name, Account_Type_ValueGeneralCustomer, OperationDate, User_Id, Branch_Id, Insert_ServerDateTime)
VALUES ({CallSnowflakeIdGenerate.GenerateNewId()}, {AccountType}, {typeName}, {branchId}, {header.Id}, {typeName}, 0, {DateTime.Now}, {Common.User_Id}, {branchId}, {DateTime.Now})");
            else
                opp.db.Database.ExecuteSqlInterpolated($@"
INSERT INTO Account_Type_Main_Account_Id (Account_Type_Value, Account_Type_Name, Account_Branch_Id, Main_Account_Id, Account_Name, Account_Type_ValueGeneralCustomer, OperationDate, User_Id, Branch_Id, Insert_ServerDateTime)
VALUES ({AccountType}, {typeName}, {branchId}, {header.Id}, {typeName}, 0, {DateTime.Now}, {Common.User_Id}, {branchId}, {DateTime.Now})");
            return header.Id;
        }

        private static long MappedParent(operations opp, long branchId)
        {
            int type = AccountType;
            return opp.db.account_Type_Main_Account_Id.AsNoTracking()
                .Where(m => m.Account_Type_Value == type && m.Account_Branch_Id == branchId)
                .Select(m => m.Main_Account_Id ?? 0).FirstOrDefault();
        }

        /// <summary>أسماء صفوف جدول ShipmentCompany القديم التى لم تُستورد بعد (لا توجد شركة شحن بنفس الاسم).</summary>
        public List<string> GetLegacyNotImported()
        {
            var opp = new operations();
            var current = GetAll().Select(c => (c.Name ?? "").Trim()).ToHashSet(StringComparer.CurrentCultureIgnoreCase);
            return opp.db.ShipmentCompany.AsNoTracking().Select(c => c.Name).ToList()
                .Select(n => (n ?? "").Trim()).Where(n => n != "" && !current.Contains(n))
                .Distinct(StringComparer.CurrentCultureIgnoreCase).ToList();
        }

        /// <summary>
        /// استيراد صفوف الجدول القديم كشركات شحن جديدة (كل صف بحسابه، بنفس مسار Save). الجدول القديم لا يتغير، والفواتير القديمة
        /// تحتفظ باسم الشركة المخزَّن فيها (ShipmentCompany_Name). يرجع عدد المستورَد، والأسماء المرفوضة فى Msg.
        /// </summary>
        public int ImportLegacy(ref string Msg)
        {
            int n = 0;
            var failed = new List<string>();
            foreach (var name in GetLegacyNotImported())
            {
                int ex = 0;
                string m = "";
                Save(ref ex, ref m, 0, name);
                if (ex == 0) n++;
                else failed.Add(name + ": " + m);
            }
            Msg = string.Join("\n", failed);
            return n;
        }

        /// <summary>شبكة الويندوز فورمز: كل صف جديد يُضاف وكل اسم معدَّل يُحفظ بنفس Save؛ أى رفض يُجمع فى استثناء واحد.</summary>
        public Task SaveRangeAsync(List<ShipmentCompany> companies)
        {
            var errors = new List<string>();
            foreach (var item in companies)
            {
                int ex = 0;
                string msg = "";
                long id = Save(ref ex, ref msg, item.Id, item.Name);
                if (ex != 0) errors.Add((item.Name ?? "") + ": " + msg);
                else item.Id = id;
            }
            if (errors.Count > 0) throw new InvalidOperationException(string.Join("\n", errors));
            return Task.CompletedTask;
        }

        public Task<bool> DeleteAsync(long id)
        {
            int ex = 0;
            string msg = "";
            Delete(ref ex, ref msg, id);
            if (ex != 0) throw new InvalidOperationException(msg);
            return Task.FromResult(true);
        }

        /// <summary>مسار قديم كان يربط حساب النوع 65 بجدول ShipmentCompany؛ النوع 65 صار يمر على مسار المورد فى Sub_Account.</summary>
        [Obsolete("Shipment companies are Supplier rows now; use Save.")]
        public long AddNewCompany(ref int excp, ref string msg, ref operations opp, ShipmentCompany company, Submain_Account sa_)
        {
            if (Common.Use_Auto_Identity_Column == 0)
            {
                company.Id = sa_.Id;
            }
            company.Name = sa_.SubAccount_Name;
            company.SubAccount_Id = sa_.Id;
            opp.db.ShipmentCompany.Add(company);
            opp.db.SaveChanges();
            return company.Id;
        }
    }
}
