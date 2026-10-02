using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib.Models
{
    /// <summary>صف قائمة مندوبى المبيعات (يقابل حقول Connection4It.SalesMan_Info).</summary>
    public class SalesManRow
    {
        public long Id { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string Address { get; set; } = "";
        public string Tel { get; set; } = "";
        public string Tel2 { get; set; } = "";
        public string Mobile { get; set; } = "";
        public string Fax { get; set; } = "";
        public string E_Mail { get; set; } = "";
        public long SubAccount_Id { get; set; }
    }

    public class SalesManCustomerRow
    {
        public long Id { get; set; }
        public string Name { get; set; } = "";
    }

    /// <summary>
    /// 🟢 كود أعمال بيانات مندوبى المبيعات لواجهة Razor (بطلب المستخدم: فى AccountModelsLib). المندوب = موظف (Employee) وظيفته Common.job_list_Id_SalesMan.
    /// يقابل Connection4It.SalesMan_Info لكن بلا SQL مدموج ومع إصلاح ثلاثة مواضع فى الأصل: (1) الأصل يُنشئ الحساب الفرعى (وهو يُنشئ سجل موظف بنفسه) ثم يُدرج سجل موظف
    /// ثانياً فيتكرر المندوب؛ هنا يُستكمل السجل المنشأ مع الحساب. (2) الأصل يضع فى SubAccount_Id قيمة Common.Employee_Account_Type؛ هنا رقم الحساب الفرعى الصحيح.
    /// (3) الأصل يحذف الموظف حذفاً صلباً بـint.Parse(Id) (لا يعمل مع الأرقام الكبيرة)؛ هنا Sub_Account.Delete_Account كبقية البيانات الأساسية.
    /// </summary>
    public class SalesMenCLS
    {
        public List<SalesManRow> GetSalesMen()
        {
            var opp = new operations();
            long job = Common.job_list_Id_SalesMan;
            return opp.db.Employees.AsNoTracking()
                .Where(e => e.jobsListId == job && e.active == 1)
                .OrderBy(e => e.name)
                .ToList()
                .Select(e => new SalesManRow
                {
                    Id = e.Id,
                    Code = e.code ?? "",
                    Name = e.name ?? "",
                    Address = e.address ?? "",
                    Tel = e.Tel ?? "",
                    Tel2 = e.Tel2 ?? "",
                    Mobile = e.Mobile ?? "",
                    Fax = e.Fax ?? "",
                    E_Mail = e.E_Mail ?? "",
                    SubAccount_Id = e.SubAccount_Id ?? 0,
                }).ToList();
        }

        /// <summary>إضافة مندوب (يقابل BtnAdd_Click). Excp: 0 نجاح، 3 اسم مكرر بين الموظفين، غير ذلك خطأ (Msg).</summary>
        public void AddSalesMan(ref int Excp, ref string Msg, SalesManRow row)
        {
            Excp = 0;
            Msg = "";
            var opp = new operations(2);
            try
            {
                if (opp.db.Employees.AsNoTracking().Any(e => e.name == row.Name))
                {
                    opp.Rollback();
                    Excp = 3;
                    return;
                }

                long newSubAccountId = -1;
                string accountCode = "";
                new Sub_Account().Add_AccountSupplierCustomerOther(ref Excp, ref Msg, ref opp, ref newSubAccountId, ref accountCode,
                    Common.Employee_Account_Type, row.Name, Common.Branch_Id, 1);
                if (Excp != 0)
                {
                    opp.Rollback();
                    return;
                }

                var emp = opp.db.Employees.Where(e => e.SubAccount_Id == newSubAccountId).FirstOrDefault()
                          ?? opp.db.Employees.Where(e => e.name == row.Name).FirstOrDefault();
                if (emp == null)
                {
                    throw new Exception("لم يتم إنشاء سجل المندوب مع حسابه");
                }
                Apply(emp, row);
                emp.jobsListId = Common.job_list_Id_SalesMan;
                emp.active = 1;
                emp.Branch_Id = Common.Branch_Id;
                emp.User_Id = Common.User_Id;
                opp.db.SaveChanges();
                opp.Commit();
                Excp = 0;
            }
            catch (Exception ee)
            {
                try { opp.Rollback(); } catch { }
                Excp = 1;
                Msg = ee.InnerException != null ? ee.InnerException.Message : ee.Message;
            }
        }

        /// <summary>تعديل بيانات مندوب (يقابل BtnEdit_Click) - بلا مساس بحسابه.</summary>
        public void UpdateSalesMan(ref int Excp, ref string Msg, SalesManRow row)
        {
            Excp = 0;
            Msg = "";
            try
            {
                var opp = new operations();
                var emp = opp.db.Employees.Where(e => e.Id == row.Id).FirstOrDefault();
                if (emp == null)
                {
                    Excp = 1;
                    Msg = "السجل غير موجود أو تم حذفه.";
                    return;
                }
                if (opp.db.Employees.AsNoTracking().Any(e => e.name == row.Name && e.Id != row.Id))
                {
                    Excp = 3;
                    return;
                }
                Apply(emp, row);
                emp.Edit_User_Id = Common.User_Id;
                emp.EditOperationDate = DateTime.Now;
                opp.db.SaveChanges();
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.InnerException != null ? ee.InnerException.Message : ee.Message;
            }
        }

        /// <summary>حذف مندوب (يقابل BtnDelete_Click): Sub_Account.Delete_Account لنوع حساب الموظفين. Excp=10 ← لا يمكن الحذف (Msg).</summary>
        public void DeleteSalesMan(ref int Excp, ref string Msg, long Id, long SubAccount_Id)
        {
            Excp = 0;
            Msg = "";
            try
            {
                new Sub_Account().Delete_Account(ref Excp, ref Msg, Common.Employee_Account_Type, Id, SubAccount_Id);
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.InnerException != null ? ee.InnerException.Message : ee.Message;
            }
        }

        // ---------------- عملاء المندوب (Accounts.SalesManProcess.CustomerSalesMan) ----------------
        // دوال long بدل CustomersAccountCLS.Set_Customer_SalesMan/Get_Customer_List التى تأخذ رقم المندوب int (لا يتسع لأرقام Snowflake).

        /// <summary>عملاء مندوب بعينه (Customer.SalesMan_Id) - الفعالون وغير المندوبين.</summary>
        public List<SalesManCustomerRow> GetCustomersOf(long salesManId)
        {
            var opp = new operations();
            var dt = opp.return_DataTable("SELECT Id, Name FROM Customer WHERE Active = 1 AND IsSalesMan = 0 AND SalesMan_Id = " + salesManId + " ORDER BY Name");
            return dt.Rows.Cast<DataRow>().Select(r => new SalesManCustomerRow { Id = Convert.ToInt64(r["Id"]), Name = r["Name"]?.ToString() ?? "" }).ToList();
        }

        /// <summary>العملاء الذين لا مندوب لهم (Customer_DataNotHaveSalesMan).</summary>
        public List<SalesManCustomerRow> GetCustomersWithoutSalesMan()
        {
            var opp = new operations();
            var dt = opp.return_DataTable("SELECT Id, Name FROM Customer WHERE Active = 1 AND IsSalesMan = 0 AND (SalesMan_Id = -1 OR SalesMan_Id IS NULL) ORDER BY Name");
            return dt.Rows.Cast<DataRow>().Select(r => new SalesManCustomerRow { Id = Convert.ToInt64(r["Id"]), Name = r["Name"]?.ToString() ?? "" }).ToList();
        }

        /// <summary>ربط عميل بمندوب (Set_Customer_SalesMan).</summary>
        public void SetCustomerSalesMan(ref int Excp, ref string Msg, long salesManId, long customerId)
        {
            try
            {
                new operations().update_me(" update Customer Set SalesMan_Id=" + salesManId + " where ID=" + customerId);
                Excp = 0;
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.InnerException != null ? ee.InnerException.Message : ee.Message;
            }
        }

        /// <summary>إلغاء ربط عميل بمندوبه (NotSet_Customer_SalesMan: SalesMan_Id = -1).</summary>
        public void UnsetCustomerSalesMan(ref int Excp, ref string Msg, long customerId)
        {
            try
            {
                new operations().update_me(" update Customer Set SalesMan_Id=-1 where ID=" + customerId);
                Excp = 0;
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.InnerException != null ? ee.InnerException.Message : ee.Message;
            }
        }

        private static void Apply(Employee emp, SalesManRow row)
        {
            emp.name = row.Name;
            emp.code = row.Code;
            emp.address = row.Address;
            emp.Tel = row.Tel;
            emp.Tel2 = row.Tel2;
            emp.Mobile = row.Mobile;
            emp.Fax = row.Fax;
            emp.E_Mail = row.E_Mail;
        }
    }
}
