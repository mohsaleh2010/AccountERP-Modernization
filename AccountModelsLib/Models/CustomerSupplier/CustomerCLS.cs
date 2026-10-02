using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
namespace AccountModelsLib.Models
{
    [NotMapped]
    public class CustomerCLS
    {
        public Sub_Account acc;
        public operations opp;
        public Customer cs = new Customer();
        DataTable dt = new DataTable();
        public CustomerCLS()
        {
            opp = new operations();
            SetCustomerDefaultValue();


        }
        public CustomerCLS(bool NoDefatult_opp)
        {
            //opp = new operations();
            SetCustomerDefaultValue();


        }
        public CustomerCLS(ref operations opp_)
        {
            SetCustomerDefaultValue();
            opp = opp_;
        }

        public void SetCustomerDefaultValue()
        {
            //    cs.Id = 0;
            //    cs.Active = 1;
            //    cs.Price_Type_List_ID = 0;
            //    cs.Balance = 0;
            //    cs.Balance_Actual = 0;
            //    cs.SubAccount_Id = 0;
            //    cs.Group_Id = 1;

            //    cs.General_Customer_Flage = 0;

            //    cs.IsSalesMan = 0;
            //    cs.SalesMan_Id = 0;
            //    cs.Location_Id = 0;

            //    cs.Ref_Cust_Supplier_Id = 0;



            cs.SubAccount_Id = 0;
            cs.General_Customer_Flage = 0;
            cs.Tax_No = "";
            cs.Code = "";
            cs.Name = "";
            cs.Address = "";
            cs.Tel = "";
            cs.Tel2 = "";
            cs.Mobile = "";
            cs.E_Mail = "";
            cs.Fax = "";
            cs.Web_Site = "";
            cs.Balance = 0;
            Common.Customer_Type = 1;
            cs.Price_Type_List_ID = 0;
            cs.Group_Id = 1;
            cs.VisaNo = "";
            cs.VisaPassword = "";
            cs.Currency_Id = 1;
            cs.Account_Code = "";
            cs.Active = 1;
            cs.ContactPerson = "";
            cs.Ref_Cust_Supplier_Id = 0;


            cs.CompanyName = "";
            cs.CompanyContactName = "";
            cs.CompanyContactData = "";
            cs.NationalId = "";
            cs.PassportNo = "";
            cs.PostalNo = "";
            cs.Commercial_Register = "";
            cs.RegistrationDate = DateTime.Now.Date;
            cs.Gov_Country_ListId = 0;
            cs.Govern_Area_ListId = 0;
            cs.Govern_ListId = 0;
            cs.Gov_City_ListId = 0;
        }

        public void InsertCustomer(ref int Excp, ref string Msg, ref operations opp)
        {
            //Customer cs = new Customer();
            int C = 0;
            string mysql = "";
            mysql = "Select Id From Customer Where Id=" + cs.Id;
            DataTable dt = opp.return_DataTable(mysql);
            C = dt.Rows.Count;
            if (C == 0)
            {
                if (Common.Use_AutoNumeric_Customer_Id__With_Submain_Account_Id == 0)
                {
                    cs.Id = cs.SubAccount_Id;
                    opp.db.Customer.Add(cs);
                }
                else
                {
                    opp.db.Customer.Add(cs);
                }

                opp.db.SaveChanges();



                FirstBalance_CLS ff = new FirstBalance_CLS();
                long Year_Financial_Period_Id = Financial_PeriodCLS.Get_Year_Financial_Period_Id(DateTime.Now);
                ff.IsCheckLastLevel = true;
                ff.Account_Customer_Type_ID = cs.Id;
                ff.Account_Customer_Type_Value = Common.Customer_Account_Type;

                ff.Save_FirstBalance(ref Excp, ref Msg, ref opp, (long)cs.SubAccount_Id, cs.Name, Year_Financial_Period_Id, Common.Branch_Id, (int)cs.Currency_Id, 0, 1, 1);


            }
            else
            {
                UpdateCustomer(ref Excp, ref Msg, ref opp);
            }
        }
        public long InsertCustomer(ref int Excp, ref string Msg, ref operations opp, Submain_Account sa_)
        {

            if (sa_.Id > 0)
            {
                //فى حالة الكود اتوماتيك والعميل ليس من متنوعون
                if (cs.General_Customer_Flage == 0 & (Common.Use_AutoNumeric_Customer_Id__With_Submain_Account_Id == 0 | Common.Use_Auto_Identity_Column == 0))
                {
                    cs.Id = sa_.Id;
                }
            }
            int C = 0;
            if (cs.Id > 0)
            {
                string mysql = "";
                mysql = "Select Id From Customer Where Id=" + cs.Id;
                DataTable dt = opp.return_DataTable(mysql);
                C = dt.Rows.Count;
            }
            if (C == 0)
            {
                //cs.Name = sa_.SubAccount_Name;
                cs.Currency_Id = sa_.Currency_Id;
                cs.Account_Code = sa_.SubAccount_NO;
                cs.SubAccount_Id = sa_.Id;
                cs.Customer_Type = Common.Customer_Type;
                cs.Active = 1;
                //cs.Ref_Cust_Supplier_Id = 0;


                opp.db.Customer.Add(cs);
                opp.db.SaveChanges();
                //if (cs.Id > 0)
                //{
                //    string mysql = "";
                //    mysql = "Select Id From Customer Where Id=" + cs.Id;
                //    DataTable dt = opp.return_DataTable(mysql);
                //    C = dt.Rows.Count;
                //}
                Save_FirstBalance(ref Excp, ref Msg, ref opp, cs);

                return cs.Id;


            }
            else
            {
                UpdateCustomer(ref Excp, ref Msg, ref opp);

                return cs.Id;
            }

        }

        void Save_FirstBalance(ref int Excp, ref string Msg, ref operations opp, Customer cs)
        {
            FirstBalance_CLS ff = new FirstBalance_CLS();
            long Year_Financial_Period_Id = Financial_PeriodCLS.Get_Year_Financial_Period_Id(DateTime.Now);
            ff.IsCheckLastLevel = true;
            ff.Account_Customer_Type_ID = cs.Id;
            ff.Account_Customer_Type_Value = Common.Customer_Account_Type;

            ff.Save_FirstBalance(ref Excp, ref Msg, ref opp, cs.SubAccount_Id, cs.Name, Year_Financial_Period_Id, Common.Branch_Id, (int)cs.Currency_Id, 0, 1, 1);

        }
        public void UpdateCustomer(ref int Excp, ref string Msg, ref operations opp, Submain_Account sa)
        {
            //Customer cs = new Customer();
            Get_CustomerBySubAccount_Id(ref opp, sa.Id);

            //DataTable dt = GetCustomer(ref opp, sa.SubAccount_Id);
            //if (dt.Rows.Count == 0)
            if (cs == null)
            {
                cs.Id = sa.Id;
                cs.Name = sa.SubAccount_Name;
                cs.Currency_Id = sa.Currency_Id;
                cs.Account_Code = sa.Account_Code;
                cs.SubAccount_Id = sa.Id;
                cs.Group_Id = 1;
                cs.Balance = 0;
                cs.Balance_Actual = 0;

                InsertCustomer(ref Excp, ref Msg, ref opp);


            }
            else
            {
                //cs.Id = cs.Id;// long.Parse(dt.Rows[0]["Id"].ToString());
                //Get_CustomerByID(ref opp,cs.Id);

                cs.Name = sa.SubAccount_Name;
                cs.Currency_Id = sa.Currency_Id;
                cs.Account_Code = sa.Account_Code;
                cs.SubAccount_Id = sa.Id;

                UpdateCustomer(ref Excp, ref Msg, ref opp);
            }
        }
        public long UpdateCustomerOnly(ref int Excp, ref string Msg, ref operations opp)
        {
            EditCustomer(ref opp, cs);
            //opp.db.Entry(cs).State = EntityState.Modified;
            //opp.db.SaveChanges();
            return cs.Id;
        }

        public void UpdateCustomer(ref int Excp, ref string Msg, ref operations opp)
        {
            //Customer cs = new Customer();
            string mysql = "";
            //mysql = "UPDATE Customer SET General_Customer_Flage=" + cs.General_Customer_Flage + ",Tax_No='" + cs.Tax_No + "', code='" + cs.Code + "',Name='" + cs.Name + "', Address='" + cs.Address + "', Tel='" + cs.Tel + "'";
            //mysql = mysql + "   , Tel2= '" + cs.Tel2 + "', Mobile= '" + cs.Mobile + "', E_Mail= '" + cs.E_Mail + "', Fax= '" + cs.Fax + "' ";
            //mysql = mysql + "  , Web_Site='" + cs.Web_Site + "', Balance=" + cs.Balance + ",Balance_Actual=" + cs.Balance + ",SubAccount_Id=" + cs.SubAccount_Id + ",Customer_Type=" + Common.Customer_Type + ",Price_Type_List_ID=" + cs.Price_Type_List_ID + "";
            //mysql = mysql + " ,Ref_Cust_Supplier_Id=" + cs.Ref_Cust_Supplier_Id + " ,Group_Id=" + cs.Group_Id + ",VisaNo='" + cs.VisaNo + "',VisaPassword ='" + cs.VisaPassword + "',ContactPerson='" + cs.ContactPerson + "'";

            //mysql = mysql + " , CompanyName= '" + cs.CompanyName + "'  ";
            //mysql = mysql + " , CompanyContactName= '" + cs.CompanyContactName + "'  ";
            //mysql = mysql + " , CompanyContactData= '" + cs.CompanyContactData + "'  ";
            //mysql = mysql + " , NationalId= '" + cs.NationalId + "'  ";
            //mysql = mysql + " , PassportNo= '" + cs.PassportNo + "'  ";
            //mysql = mysql + " , PostalNo= '" + cs.PostalNo + "'  ";
            //mysql = mysql + " , Commercial_Register= '" + cs.Commercial_Register + "'  ";
            //mysql = mysql + " , RegistrationDate= '" + operations.dateFormat(cs.RegistrationDate) + "'  ";
            //mysql = mysql + " , Gov_Country_ListId=  " + cs.Gov_Country_ListId + "  ";
            //mysql = mysql + " , Govern_Area_ListId=  " + cs.Govern_Area_ListId + "  ";
            //mysql = mysql + " , Govern_ListId=  " + cs.Govern_ListId + "  ";
            //mysql = mysql + " , Gov_City_ListId=  " + cs.Gov_City_ListId + "  ";
            EditCustomer(ref opp, cs);

            //opp.db.Entry(cs).State = EntityState.Modified;
            //opp.db.SaveChanges();
            //CompanyName
            //CompanyContactName
            //CompanyContactData
            //NationalId
            //PassportNo
            //PostalNo
            //Gov_Country_ListId
            //Govern_Area_ListId

            //Govern_ListId
            //Gov_City_ListId
            //PersonCompanyType
            //RegistrationDate
            //Commercial_Register


            mysql += " where Id=" + cs.Id;
            //opp.insert_me(mysql);

            long Account_Customer_Type_ID = cs.Id;
            int Account_Customer_Type_Value = Common.Customer_Account_Type;
            long SubAccount_Id = cs.SubAccount_Id;

            Sub_Account sa = new Sub_Account();
            sa.UpdateSubmain_SubAccount_Name(ref Excp, ref Msg, ref opp, cs.Name, Account_Customer_Type_ID, Account_Customer_Type_Value, ref SubAccount_Id);

            cs.SubAccount_Id = SubAccount_Id;

            EditCustomer(ref opp, cs);

            //opp.db.Entry(cs).State = EntityState.Modified;
            //opp.db.SaveChanges();

            //mysql = "UPDATE Customer SET  SubAccount_Id=" + SubAccount_Id + "";
            //mysql += " where Id=" + cs.Id;
            //opp.insert_me(mysql);


        }
        public void UpdateCustomer(ref int Excp, ref string Msg, Customer cu, long New_Submain_Account_Id, bool IsUse_SubAccount_Id_Select)
        {
            //operations opp = new operations(1);
            opp.BeginTransaction(2);
            try
            {

                acc = new Sub_Account();
                int Account_Type_Value = Common.Customer_Account_Type;

                long Account_Branch_Id = cu.Branch_Id ?? 1;// Common.Branch_Id;
                string Account_Code = "";

                acc.IsUse_SubAccount_Id_Select = IsUse_SubAccount_Id_Select;

                if (IsUse_SubAccount_Id_Select == false)
                {
                    New_Submain_Account_Id = cu.SubAccount_Id;

                }

                acc.cuCls.cs = cu;
                acc.Update_AccountSupplierCustomerOther(ref Excp, ref Msg, ref opp, ref New_Submain_Account_Id, ref Account_Code, Account_Type_Value, cu.Name, Account_Branch_Id, cu.Currency_Id ?? 1);
                //acc.UpdateSubmain_SubAccount_Id(ref Excp, ref opp, ref Msg);

                if (Excp == 0)
                {
                    opp.Commit();
                }
                else
                {
                    opp.Rollback();

                }


            }
            catch (Exception ee)
            {
                opp.Rollback();
                Excp = 1;
                Msg = ee.Message;
            }
        }



        public void EditCustomer(ref operations opp)
        {
            EditCustomer(ref opp, cs);
            //opp.db.Entry(cs).State = EntityState.Modified;
            //opp.db.SaveChanges();
        }
        public void EditCustomer(ref operations opp, Customer customer)
        {

            opp.db.Entry(customer).State = EntityState.Modified;
            opp.db.SaveChanges();
        }

        public Customer GetCustomer(long Id)
        {
            //operations opp = new operations();
            cs = opp.db.Customer.Where(c => c.Id == Id).FirstOrDefault();

            return cs;
        }
        public Customer GetCustomerNoTracking(long Id)
        {
            //operations opp = new operations();
            cs = opp.db.Customer.AsNoTracking().Where(c => c.Id == Id).FirstOrDefault();

            return cs;
        }

        //public Customer GetCustomerById(ref operations opp,long Id)
        //{

        //    Customer customer = opp.db.Customer.Where(c => c.Id == Id).FirstOrDefault(); ;

        //    return customer;
        //}
        public List<Customer> GetCustomers(ref operations opp)
        {
            List<Customer> customers = new List<Customer>();
            try
            {
                customers = opp.db.Customer.ToList();
            }
            catch (Exception ee)
            {
                operations.DataDictionaryLogerException(ee);
            }
            return customers;
        }
        public List<Customer> GetCustomersAsNoTracking(ref operations opp)
        {
            List<Customer> customers = new List<Customer>();
            try
            {
                customers = opp.db.Customer.AsNoTracking().Where(a => a.Active == 1).OrderBy(c => c.Name).ToList();
            }
            catch (Exception ee)
            {
                operations.DataDictionaryLogerException(ee);
            }
            return customers;
        }
        public List<CustomerSelect> GetCustomersSelectAsNoTracking()
        {
            List<CustomerSelect> customers = new List<CustomerSelect>();
            try
            {
                customers = opp.db.Customer.AsNoTracking().Where(a => a.Active == 1).OrderBy(c => c.Name)
                    .Select(c => new CustomerSelect { Id = c.Id, Name = c.Name })
                    .ToList();
            }
            catch (Exception ee)
            {
                operations.DataDictionaryLogerException(ee);
            }
            return customers;
        }

        public List<Customer> GetCustomersInsurance(ref operations opp)
        {
            List<Customer> customers = new List<Customer>();
            try
            {
                customers = opp.db.Customer.Where(c => c.IsInsuranceCompany == 1).ToList();
            }
            catch (Exception ee)
            {
                operations.DataDictionaryLogerException(ee);
            }
            return customers;
        }
        public List<Customer> GetCustomers()
        {
            List<Customer> customers = GetCustomers(ref opp);

            return customers;
        }
        public List<Customer> GetCustomersAsNoTracking()
        {
            List<Customer> customers = GetCustomersAsNoTracking(ref opp);

            return customers;
        }
        public List<Customer> GetCustomersList(ref operations opp, int Active, int General_Customer_Flage)
        {
            IQueryable<Customer> query = opp.db.Customer.AsNoTracking().OrderBy(i => i.Name);
            //
            //
            if (Active > -1)
            {
                query = query.Where(i => i.Active == Active);
            }
            if (General_Customer_Flage > -1)
            {
                query = query.Where(i => i.General_Customer_Flage == General_Customer_Flage);
            }
            List<Customer> ls = new List<Customer>();

            ls = query.ToList();// GetCustomerList(query);

            return ls;

            //List<Customer> customers = new List<Customer>();
            //try
            //{
            //    customers = opp.db.Customer.ToList();
            //}
            //catch (Exception ee)
            //{

            //}
            //return customers;

        }
        public List<CustomerViewModel> GetCustomers(ref operations opp, int Active, int General_Customer_Flage)
        {


            IQueryable<Customer> query = opp.db.Customer.Include(c => c.Submain_Account).Include(c => c.Gov_City_Lists).Include(c => c.Govern_Lists).Include(c => c.Govern_Area_Lists).Include(c => c.Gov_Country_Lists).Include(c => c.Customer_Supplier_Groups).OrderBy(i => i.Name);
            //
            //
            if (Active > -1)
            {
                query = query.Where(i => i.Active == Active);
            }
            if (General_Customer_Flage > -1)
            {
                query = query.Where(i => i.General_Customer_Flage == General_Customer_Flage);
            }
            List<CustomerViewModel> ls = new List<CustomerViewModel>();

            ls = GetCustomerViewModelList(query);

            return ls;
        }
        public List<CustomerViewModel> GetCustomersForSearchCar(ref operations opp, int Active, int General_Customer_Flage)
        {


            IQueryable<Customer> query = opp.db.Customer.Include(c => c.Customer_Supplier_Groups).OrderBy(i => i.Name);
            //
            //
            if (Active > -1)
            {
                query = query.Where(i => i.Active == Active);
            }
            if (General_Customer_Flage > -1)
            {
                query = query.Where(i => i.General_Customer_Flage == General_Customer_Flage);
            }
            List<CustomerViewModel> ls = new List<CustomerViewModel>();

            ls = GetCustomerViewModelList(query);

            return ls;
        }
        public List<CustomerViewModel> GetCustomersOnlyList0(ref operations opp, string NameSearch, int Active, int General_Customer_Flage)
        {


            IQueryable<Customer> query = opp.db.Customer.AsNoTracking();
            //
            //
            if (NameSearch.Trim() != "")
            {
                query = query.Where(i => i.Name.Contains(NameSearch));
            }
            if (Active > -1)
            {
                query = query.Where(i => i.Active == Active);
            }
            if (General_Customer_Flage > -1)
            {
                query = query.Where(i => i.General_Customer_Flage == General_Customer_Flage);
            }
            List<CustomerViewModel> ls = new List<CustomerViewModel>();

            ls = GetCustomerViewModelList(query);

            return ls;
        }
        public List<CustomerViewModel> GetCustomersOnlyList(ref operations opp, string NameSearch, int Active, int General_Customer_Flage)
        {


            IQueryable<Customer> query = opp.db.Customer.AsNoTracking();
            //
            //
            // 2. تطبيق الفلاتر
            if (!string.IsNullOrWhiteSpace(NameSearch))
            {
                query = query.Where(i => i.Name.Contains(NameSearch.Trim()));
            }
            if (Active > -1)
            {
                query = query.Where(i => i.Active == Active);
            }
            if (General_Customer_Flage > -1)
            {
                query = query.Where(i => i.General_Customer_Flage == General_Customer_Flage);
            }

            // 3. الترتيب من خلال قاعدة البيانات أولاً
            query = query.OrderBy(i => i.Name);
            List<CustomerViewModel> ls = new List<CustomerViewModel>();

            ls = GetCustomerViewModelList(query);

            return ls;
        }

        // 🟢 عملاء لهم مورد مرتبط فعلاً (Customer_SupplierInfo الأصلية، فلترها الافتراضى بدون rdAllCustomer) -
        // تُستخدَم لتعبئة قائمة الاختيار فى شاشة "بيانات حساب عميل-مورد-مختلط".
        public List<CustomerViewModel> GetMixedCustomerSupplierList(ref operations opp)
        {
            IQueryable<Customer> query = opp.db.Customer.AsNoTracking()
                .Where(i => i.Active == 1 && i.Ref_Cust_Supplier_Id != null && i.Ref_Cust_Supplier_Id != 0)
                .OrderBy(i => i.Name);
            return GetCustomerViewModelList(query);
        }

        public DataTable GetCustomersOnlyListWithSubMainAccount(ref operations opp, string NameSearch, int Active, int General_Customer_Flage)
        {
            //
            string mysql = " SELECT DISTINCT Submain_Account.Id AS Submain_AccountId, Submain_Account.SubAccount_Name, Submain_Account.SubAccount_NO, Customer.*\r\nFROM            Submain_Account RIGHT OUTER JOIN\r\n                         Customer ON Submain_Account.Id = Customer.SubAccount_Id ";

            mysql = mysql + " Where  Customer.Id >0  ";

            if (NameSearch.Trim() != "")
            {
                mysql = mysql + " AND Customer.Name like N'%" + NameSearch + "%' ";
            }
            if (Active > -1)
            {
                mysql = mysql + " AND Customer.Active=" + Active;
            }
            if (General_Customer_Flage > -1)
            {
                mysql = mysql + " AND Customer.General_Customer_Flage=" + General_Customer_Flage;
            }

            mysql = mysql + " Order by General_Customer_Flage ";
            DataTable dt = opp.return_DataTable(mysql);

            return dt;
        }
        public List<Customer> GetCustomerList(IQueryable<Customer> query)
        {
            List<Customer> ls = new List<Customer>();
            int Serial = 0;
            foreach (var item in query)
            {
                Customer gg = new Customer();
                try
                {

                    gg.Id = item.Id;
                    gg.Name = item.Name;

                    gg.Code = item.Code;
                    gg.Address = item.Address;
                    gg.Tel = item.Tel;
                    gg.Tel2 = item.Tel2;
                    gg.Mobile = item.Mobile;
                    gg.E_Mail = item.E_Mail;
                    gg.Fax = item.Fax;
                    gg.Web_Site = item.Web_Site;
                    gg.Customer_Type = item.Customer_Type;
                    gg.Active = item.Active;

                    gg.IsSalesMan = item.IsSalesMan;
                    gg.SalesMan_Id = item.SalesMan_Id;
                    gg.Date_Balance_First = item.Date_Balance_First;
                    gg.Balance_First = item.Balance_First;
                    gg.Location_Id = item.Location_Id;
                    gg.Price_Type_List_ID = item.Price_Type_List_ID;
                    gg.Tax_No = item.Tax_No;


                    gg.Account_Code = item.Account_Code;
                    gg.Currency_Id = item.Currency_Id;
                    gg.Static_Flage = item.Static_Flage;
                    gg.General_Customer_Flage = item.General_Customer_Flage;
                    gg.Govern_ListName = item.Govern_ListName;

                    gg.Ref_Cust_Supplier_Id = item.Ref_Cust_Supplier_Id;
                    gg.VisaNo = item.VisaNo;
                    gg.VisaPassword = item.VisaPassword;
                    gg.CustomerNotes = item.CustomerNotes;


                    gg.OperationDate = item.OperationDate;
                    gg.User_Id = item.User_Id;
                    gg.Branch_Id = item.Branch_Id;
                    gg.Insert_ServerDateTime = item.Insert_ServerDateTime;
                    gg.Update_ServerDateTime = item.Update_ServerDateTime;
                    gg.EditOperationDate = item.EditOperationDate;
                    gg.Edit_User_Id = item.Edit_User_Id;
                    gg.ContactPerson = item.ContactPerson;
                    gg.ProcessNo = item.ProcessNo;

                    gg.CompanyName = item.CompanyName;
                    gg.CompanyContactName = item.CompanyContactName;
                    gg.CompanyContactData = item.CompanyContactData;
                    gg.Commercial_Register = item.Commercial_Register;
                    gg.NationalId = item.NationalId;
                    gg.PassportNo = item.PassportNo;
                    gg.PostalNo = item.PostalNo;


                    gg.Gov_Country_ListId = item.Gov_Country_ListId;
                    gg.Govern_Area_ListId = item.Govern_Area_ListId;
                    gg.Govern_ListId = item.Govern_ListId;
                    gg.Gov_City_ListId = item.Gov_City_ListId;

                    gg.Group_Id = item.Group_Id;



                }
                catch (Exception ee)
                {

                }
                ls.Add(gg);
            }
            ls.OrderBy(i => i.Name);
            return ls;
        }

        public List<CustomerViewModel> GetCustomerViewModelList(IQueryable<Customer> query)
        {
            List<CustomerViewModel> ls = new List<CustomerViewModel>();
            int Serial = 0;
            foreach (var item in query)
            {
                CustomerViewModel gg = new CustomerViewModel();
                try
                {
                    Serial++;
                    gg.Serial = Serial;
                    gg.Id = item.Id;
                    gg.Name = item.Name;

                    gg.Code = item.Code;
                    gg.Address = item.Address;
                    gg.Tel = item.Tel;
                    gg.Tel2 = item.Tel2;
                    gg.Mobile = item.Mobile;
                    gg.E_Mail = item.E_Mail;
                    gg.Fax = item.Fax;
                    gg.Web_Site = item.Web_Site;
                    gg.Customer_Type = item.Customer_Type;
                    gg.Active = item.Active;

                    gg.IsSalesMan = item.IsSalesMan;
                    gg.SalesMan_Id = item.SalesMan_Id;
                    gg.Date_Balance_First = item.Date_Balance_First;
                    gg.Balance_First = item.Balance_First;
                    gg.Location_Id = item.Location_Id;
                    gg.Price_Type_List_ID = item.Price_Type_List_ID;
                    gg.Tax_No = item.Tax_No;


                    gg.Account_Code = item.Account_Code;
                    gg.Currency_Id = item.Currency_Id;
                    gg.Static_Flage = item.Static_Flage;
                    gg.General_Customer_Flage = item.General_Customer_Flage;
                    gg.Govern_ListName = item.Govern_ListName;

                    gg.Ref_Cust_Supplier_Id = item.Ref_Cust_Supplier_Id;
                    gg.VisaNo = item.VisaNo;
                    gg.VisaPassword = item.VisaPassword;
                    gg.CustomerNotes = item.CustomerNotes;


                    gg.OperationDate = item.OperationDate;
                    gg.User_Id = item.User_Id;
                    gg.Branch_Id = item.Branch_Id;
                    gg.Insert_ServerDateTime = item.Insert_ServerDateTime;
                    gg.Update_ServerDateTime = item.Update_ServerDateTime;
                    gg.EditOperationDate = item.EditOperationDate;
                    gg.Edit_User_Id = item.Edit_User_Id;
                    gg.ContactPerson = item.ContactPerson;
                    gg.ProcessNo = item.ProcessNo;

                    gg.CompanyName = item.CompanyName;
                    gg.CompanyContactName = item.CompanyContactName;
                    gg.CompanyContactData = item.CompanyContactData;
                    gg.Commercial_Register = item.Commercial_Register;
                    gg.NationalId = item.NationalId;
                    gg.PassportNo = item.PassportNo;
                    gg.PostalNo = item.PostalNo;


                    gg.Gov_Country_ListId = item.Gov_Country_ListId;
                    gg.Govern_Area_ListId = item.Govern_Area_ListId;
                    gg.Govern_ListId = item.Govern_ListId;
                    gg.Gov_City_ListId = item.Gov_City_ListId;

                    gg.Group_Id = item.Group_Id;
                    gg.GroupName = item.Customer_Supplier_Groups != null ? item.Customer_Supplier_Groups.GroupName : "";
                    gg.CityName = item.Gov_City_Lists != null ? item.Gov_City_Lists.NameAr : "";
                    gg.GovernName = item.Govern_Lists != null ? item.Govern_Lists.Gov_Name : "";
                    gg.AreaName = item.Govern_Area_Lists != null ? item.Govern_Area_Lists.Area_Name : "";
                    gg.CountryName = item.Gov_Country_Lists != null ? item.Gov_Country_Lists.NameAr : "";
                    gg.SubAccount_Name = item.Submain_Account != null ? item.Submain_Account.SubAccount_Name : "";
                    gg.SubAccount_Id = item.SubAccount_Id;
                    //try
                    //{
                    //    if (item.Customer_Supplier_Groups != null)
                    //    {
                    //        gg.GroupName = item.Customer_Supplier_Groups.GroupName;
                    //    }
                    //    else
                    //    {
                    //        gg.GroupName = "";
                    //    }


                    //}
                    //catch (Exception)
                    //{
                    //    gg.GroupName = "";
                    //}

                    //try
                    //{
                    //    if (item.Gov_City_Lists != null)
                    //    {
                    //        gg.CityName = item.Gov_City_Lists.NameAr;
                    //    }
                    //    else
                    //    {
                    //        gg.CityName = "";
                    //    }

                    //}
                    //catch (Exception ee)
                    //{
                    //    gg.CityName = "";
                    //}
                    //try
                    //{
                    //    if (item.Govern_Lists != null)
                    //    {
                    //        gg.GovernName = item.Govern_Lists.Gov_Name;
                    //    }
                    //    else
                    //    {
                    //        gg.GovernName = "";

                    //    }

                    //}
                    //catch (Exception ee)
                    //{
                    //    gg.GovernName = "";
                    //}
                    //try
                    //{
                    //    if (item.Govern_Area_Lists != null)
                    //    {
                    //        gg.AreaName = item.Govern_Area_Lists.Area_Name;
                    //    }
                    //    else
                    //    {
                    //        gg.AreaName = "";
                    //    }

                    //}
                    //catch (Exception ee)
                    //{
                    //    gg.AreaName = "";
                    //}
                    //try
                    //{
                    //    if (item.Gov_Country_Lists != null)
                    //    {
                    //        gg.CountryName = item.Gov_Country_Lists.NameAr;
                    //    }
                    //    else
                    //    {
                    //        gg.CountryName = "";
                    //    }
                    //}
                    //catch (Exception ee)
                    //{
                    //    gg.CountryName = "";
                    //}
                    //try
                    //{
                    //    if (item.Submain_Account != null)
                    //    {
                    //        gg.SubAccount_Name = item.Submain_Account.SubAccount_Name;
                    //    }
                    //    else
                    //    {
                    //        gg.SubAccount_Name = "";
                    //    }
                    //}
                    //catch (Exception ee)
                    //{
                    //    gg.SubAccount_Name = "";
                    //}




                }
                catch (Exception ee)
                {

                }
                ls.Add(gg);
            }
            //ls.OrderBy(i => i.Name);
            return ls;
        }

        public void GetCustomerByIdAsNoTracking(long Id)
        {
            //Customer cs = new Customer();
            operations opp = new operations();
            GetCustomerByIdAsNoTracking(ref opp, Id);

        }
        public void GetCustomerByIdAsNoTracking(ref operations opp, long Id)
        {
            //Customer cs = new Customer();

            cs = opp.db.Customer.AsNoTracking().Where(c => c.Id == Id).FirstOrDefault();

        }
        public void GetCustomerById(ref operations opp, long Id)
        {

            cs = opp.db.Customer.Where(c => c.Id == Id).FirstOrDefault();

        }


        public DataTable GetCustomer(ref operations opp, int General_Customer_Flage_)
        {
            //
            string mysql = "SELECT * FROM Customer where Active =1  and IsSalesMan=0 And Customer_Type=" + Common.Customer_Type;
            if (General_Customer_Flage_ > -1)
            {
                mysql = mysql + " AND General_Customer_Flage=" + General_Customer_Flage_ + "";
            }
            mysql = mysql + " Order by General_Customer_Flage ";
            DataTable dt = opp.return_DataTable(mysql);

            return dt;
        }
        public DataTable GetCustomerToPrint(int Active, int Customer_Type, int General_Customer_Flage)
        {
            operations opp = new operations();

            string mysql = @" SELECT        Customer.Id, Customer.Name, Customer.Balance, Customer.Balance_Actual, Customer.Code, Customer.Address, Customer.Tel, Customer.Tel2, Customer.Mobile, Customer.E_Mail, Customer.Fax, Customer.Web_Site,                          Customer.SubAccount_Id, Customer.IsSalesMan, Customer.SalesMan_Id, Customer.Date_Balance_First, Customer.Balance_First, Customer.Location_Id, Customer.Price_Type_List_ID, Customer.Currency_Id,                          Customer.Balance_Air, Customer.Balance_Road, Customer.Tax_No, Customer.Active, Customer.Customer_Type, Price_Type_List.Price_Name, Customer.General_Customer_Flage, Customer.Account_Code  FROM            Customer LEFT OUTER JOIN                           Price_Type_List ON Customer.Price_Type_List_ID = Price_Type_List.ID ";
            mysql = mysql + $" WHERE        (Customer.Active =  {Active}) AND (Customer.Customer_Type ={Customer_Type})  ";

            if (General_Customer_Flage > -1)
            {
                mysql = mysql + " AND General_Customer_Flage=" + General_Customer_Flage + "";
            }
            mysql = mysql + " ORDER BY Customer.Name ";
            DataTable dt = opp.return_DataTable(mysql);

            return dt;
        }
        public DataTable GetCustomer(ref operations opp, long SubAccount_Id)
        {
            string mysql = "SELECT * FROM Customer where SubAccount_Id =" + SubAccount_Id;
            DataTable dt = opp.return_DataTable(mysql);

            return dt;
        }
        public void Get_Customer(long Customer_Id)
        {

            operations opp = new operations();
            Get_CustomerByID(ref opp, Customer_Id);

        }

        public bool IsFoundName(ref operations opp, string Name, long Id = 0)
        {
            bool IsFound = false;
            string mysql = " Select * from Customer where    Name = '" + Name.Trim() + "'";
            if (Id > 0)
            {
                mysql = mysql + " And Id <> " + Id;
            }
            DataTable dt = opp.return_DataTable(mysql);
            if (dt.Rows.Count > 0)
            {
                IsFound = true;
            }

            return IsFound;
        }
        public void Get_CustomerByID(ref operations opp, long Customer_Id)
        {
            //string mysql = " Select * from Customer where Id=" + Customer_Id;
            //dt = opp.return_DataTable(mysql);
            cs = new Customer();
            cs = opp.db.Customer.AsNoTracking().Where(c => c.Id == Customer_Id).FirstOrDefault();
            //ReadCustomerData(dt, 0);


        }
        public void Get_CustomerBySubAccount_Id(ref operations opp, long SubAccount_Id)
        {
            //string mysql = " Select * from Customer where SubAccount_Id=" + SubAccount_Id;
            //dt = opp.return_DataTable(mysql);
            cs = new Customer();
            cs = opp.db.Customer.AsNoTracking().Where(c => c.SubAccount_Id == SubAccount_Id).FirstOrDefault();
            //ReadCustomerData(dt, 0);


        }
        public void ReadCustomerData(DataTable dt, int index)
        {
            //Customer cs = new Customer();
            if (dt.Rows.Count > 0)
            {
                cs.Id = long.Parse(dt.Rows[index]["Id"].ToString());
                cs.Name = dt.Rows[index]["Name"].ToString();
                try
                {
                    cs.Price_Type_List_ID = int.Parse(dt.Rows[index]["Price_Type_List_ID"].ToString());
                }
                catch { cs.Price_Type_List_ID = 0; }

                cs.General_Customer_Flage = int.Parse(dt.Rows[index]["General_Customer_Flage"].ToString());
                cs.Tax_No = dt.Rows[index]["Tax_No"].ToString();
                cs.Code = dt.Rows[index]["Code"].ToString();

                cs.Address = dt.Rows[index]["Address"].ToString();
                cs.Tel = dt.Rows[index]["Tel"].ToString();
                cs.Tel2 = dt.Rows[index]["Tel2"].ToString();
                cs.Mobile = dt.Rows[index]["Mobile"].ToString();
                cs.E_Mail = dt.Rows[index]["E_Mail"].ToString();
                cs.Fax = dt.Rows[index]["Fax"].ToString();
                cs.Web_Site = dt.Rows[index]["Web_Site"].ToString();
                cs.Balance = decimal.Parse(dt.Rows[index]["Balance"].ToString());
                cs.Balance_Actual = decimal.Parse(dt.Rows[index]["Balance_Actual"].ToString());

                cs.SubAccount_Id = long.Parse(dt.Rows[index]["SubAccount_Id"].ToString());
                cs.Currency_Id = long.Parse(dt.Rows[index]["Currency_Id"].ToString());
                cs.Account_Code = dt.Rows[index]["Account_Code"].ToString();

                try
                {
                    cs.Customer_Type = int.Parse(dt.Rows[index]["Customer_Type"].ToString());
                }
                catch (Exception)
                {
                    cs.Customer_Type = 1;
                }

                cs.Group_Id = int.Parse(dt.Rows[index]["Group_Id"].ToString());
                cs.VisaNo = dt.Rows[index]["VisaNo"].ToString();
                cs.VisaPassword = dt.Rows[index]["VisaPassword"].ToString();
                cs.ContactPerson = dt.Rows[index]["ContactPerson"].ToString();

                try
                {
                    cs.Ref_Cust_Supplier_Id = (long)long.Parse(dt.Rows[index]["Ref_Cust_Supplier_Id"].ToString());
                }
                catch (Exception)
                {


                }

                cs.CompanyName = dt.Rows[index]["CompanyName"].ToString();
                cs.CompanyContactName = dt.Rows[index]["CompanyContactName"].ToString();
                cs.CompanyContactData = dt.Rows[index]["CompanyContactData"].ToString();
                cs.NationalId = dt.Rows[index]["NationalId"].ToString();
                cs.PassportNo = dt.Rows[index]["PassportNo"].ToString();
                cs.PostalNo = dt.Rows[index]["PostalNo"].ToString();
                try
                {
                    cs.LookUp_PersonCompany_TypeId = int.Parse(dt.Rows[index]["PersonCompanyType"].ToString());
                }
                catch (Exception ee)
                {
                    cs.LookUp_PersonCompany_TypeId = 1;
                }
                try
                {
                    cs.Gov_Country_ListId = int.Parse(dt.Rows[index]["Gov_Country_ListId"].ToString());
                    cs.Govern_Area_ListId = int.Parse(dt.Rows[index]["Govern_Area_ListId"].ToString());

                    cs.Govern_ListId = int.Parse(dt.Rows[index]["Govern_ListId"].ToString());
                    cs.Gov_City_ListId = int.Parse(dt.Rows[index]["Gov_City_ListId"].ToString());

                    cs.RegistrationDate = DateTime.Parse(dt.Rows[index]["RegistrationDate"].ToString());
                }
                catch (Exception ee)
                {
                }
                cs.Commercial_Register = dt.Rows[index]["Commercial_Register"].ToString();


            }
        }
        public void DeleteCustomer(ref int Excp, ref string Msg, long Id)
        {
            operations opp = new operations(2);
            DeleteCustomer(ref opp, ref Excp, ref Msg, Id);
            if (Excp == 0)
            {
                opp.Commit();
                Msg = "تم  حذف العميل بنجاح";
            }
            else
            {
                opp.Rollback();
                string Msg2 = "خطأ فى حذف العميل";
                Msg = Msg2 + "\n\r" + Msg;
            }
        }
        public void DeleteCustomer(ref operations opp, ref int Excp, ref string Msg, long Id)
        {

            string mysql = "";

            //Get_Customer(ref opp, Id);
            GetCustomerById(ref opp, Id);
            //Customer cs = (Customer)opp.db.Customer.Where(c => c.Id == 5).FirstOrDefault();
            //if (cs != null)
            //{
            //    long c = (long)cs.SubAccount_Id;
            //}

            var ca = (Customers_Accounts)opp.db.Customers_Accounts.Where(c => c.Customer_Id == Id & c.Reference_Type != -7).FirstOrDefault();
            if (ca != null)
            {
                Excp = 1;
                Msg = "لا يمكن حذف العميل نظرا لوجود عمليات محاسبية على الحساب";
            }

            var acd = (Account_Entry_Details)opp.db.Account_Entry_Details.Where(c => c.Submain_Account_ID == cs.SubAccount_Id).FirstOrDefault();
            if (acd != null)
            {
                Excp = 1;
                Msg = "لا يمكن حذف العميل نظرا لوجود عمليات محاسبية على الحساب";
            }

            if (Excp == 0)
            {
                //MakeDeleteCustomer(ref opp, ref Excp, Id);

                Excp = 0;
                Msg = "";

                int Account_Customer_Type_Value = 2;
                Sub_Account sa = new Sub_Account();
                sa.Delete_Account(ref Excp, ref Msg, ref opp, Account_Customer_Type_Value, Id, cs.SubAccount_Id);
            }
        }
        public void MakeDeleteCustomer(ref operations opp, ref int Excp, ref string Msg, long Id, long SubAccount_Id)
        {
            string mysql = "";
            if (Common.Update_DeleteSupplier == 0)
            {
                mysql = "Update Customer set active=0 Where Id= " + Id;
                mysql = mysql + "Update Customer set active=0 Where SubAccount_Id= " + SubAccount_Id;
            }
            if (Common.Update_DeleteSupplier == 1)
            {
                mysql = "delete from Customer where Id= " + Id;
                mysql = mysql + "delete from Customer where SubAccount_Id= " + SubAccount_Id;
            }
            try
            {
                opp.insert_me(mysql);
                Excp = 0;
            }
            catch (Exception ee)
            {
                Msg = ee.Message;
                Excp = 1;
            }
        }
        public void AddNewCustomerSupplier(ref int Excp, ref string Msg, CustomerCLS cs, SupplierCLS sp)
        {

            operations opp = new operations(1);
            try
            {
                AddNewCustomer(ref Excp, ref Msg, ref opp, cs);
                sp.sp.SubAccount_Id = (long)cs.cs.SubAccount_Id;
                sp.sp.Ref_Cust_Supplier_Id = cs.cs.Id;
                sp.InsertSupplier(ref Excp, ref opp);

                string mysql = " Update Customer Set Ref_Cust_Supplier_Id=" + sp.sp.Id + " Where Id=" + cs.cs.Id;
                opp.insert_me(mysql);

                opp.Commit();
                //operations opp2 = new operations();
                //string  mysql = " Update Submain_Account Set Account_Customer_Type_ID =" + cs.Id + " Where SubAccount_Id=" + cs.SubAccount_Id;
                // opp.insert_me(mysql);
                Excp = 0;
                acc.UpdateSubmain_SubAccount_Id(ref Excp, ref Msg);
            }
            catch (Exception ee)
            {
                opp.Rollback();
                Excp = 1;
                Msg = ee.Message;
            }

        }
        public void AddNewCustomer(ref int Excp, ref string Msg, ref operations opp, Customer cs, bool IsUse_SubAccount_Id_Select)
        {
            opp.BeginTransaction();
            //operations opp = new operations(2);


            try
            {
                Excp = 0;
                Msg = "";
                string Account_Code = "";
                acc = new Sub_Account();
                int Account_Type_Value = Common.Customer_Account_Type;
                long Currency_Id = 1;
                long Account_Branch_Id = Common.Branch_Id;
                long New_Submain_Account_Id = -1;
                //if(cs.General_Customer_Flage==1)
                //{
                //    acc.sa.get_TreeToPrint
                //}
                acc.cuCls.cs = cs;
                acc.Add_AccountSupplierCustomerOther(ref Excp, ref Msg, ref opp, ref New_Submain_Account_Id, ref Account_Code, Account_Type_Value, cs.Name, Account_Branch_Id, cs.Currency_Id ?? 1);

                //cs.Id = acc.New_Account_Id;
                //cs.SubAccount_Id = New_Submain_Account_Id;
                if (Excp == 0)
                {
                    opp.Commit();
                }
                else
                {
                    opp.Rollback();

                }
            }
            catch (Exception ee)
            {
                opp.Rollback();
                Excp = 1;
                Msg = ee.Message;
            }
        }
        //void AddNewCustomer(ref int Excp, ref operations opp, ref string Msg, Customer cs)
        //{

        //    try
        //    {
        //        AddNewCustomer(ref Excp, ref Msg, ref opp, cs);
        //        opp.Commit();
        //        //operations opp2 = new operations();
        //        //string  mysql = " Update Submain_Account Set Account_Customer_Type_ID =" + cs.Id + " Where SubAccount_Id=" + cs.SubAccount_Id;
        //        // opp.insert_me(mysql);
        //        Excp = 0;
        //        acc.UpdateSubmain_SubAccount_Id(ref Excp, ref Msg);
        //    }
        //    catch (Exception ee)
        //    {
        //        opp.Rollback();
        //        Excp = 1;
        //        Msg = ee.Message;
        //    }
        //}

        public void AddNewCustomer(ref int Excp, ref string Msg, ref operations opp, CustomerCLS cs)
        {
            Excp = 0;
            Msg = "";
            string Account_Code = "";
            acc = new Sub_Account();
            int Account_Type_Value = Common.Customer_Account_Type;
            long Currency_Id = 1;
            long Account_Branch_Id = Common.Branch_Id;
            long New_Submain_Account_Id = -1;
            acc.cuCls = cs;
            acc.Add_AccountSupplierCustomerOther(ref Excp, ref Msg, ref opp, ref New_Submain_Account_Id, ref Account_Code, Account_Type_Value, cs.cs.Name, Account_Branch_Id, (int)cs.cs.Currency_Id);

            cs.cs.Id = acc.New_Account_Id;
            cs.cs.SubAccount_Id = New_Submain_Account_Id;

        }
        public void AddNewCustomer(ref int Excp, ref string Msg, ref operations opp, Customer cs)
        {
            Excp = 0;
            Msg = "";
            string Account_Code = "";
            acc = new Sub_Account();
            int Account_Type_Value = Common.Customer_Account_Type;
            long Currency_Id = 1;
            long Account_Branch_Id = Common.Branch_Id;
            long New_Submain_Account_Id = -1;
            //if(cs.General_Customer_Flage==1)
            //{
            //    acc.sa.get_TreeToPrint
            //}
            acc.cuCls.cs = cs;
            acc.Add_AccountSupplierCustomerOther(ref Excp, ref Msg, ref opp, ref New_Submain_Account_Id, ref Account_Code, Account_Type_Value, cs.Name, Account_Branch_Id, (int)cs.Currency_Id);

            //cs.Id = acc.New_Account_Id;
            //cs.SubAccount_Id = New_Submain_Account_Id;

        }

        public void AddNewCustomerSubAccount(ref int Excp, ref string Msg, Customer cs)
        {
            operations opp = new operations(2);
            try
            {
                AddNewCustomerSubAccount(ref Excp, ref Msg, ref opp, cs);
                opp.Commit();
                //operations opp2 = new operations();
                //string  mysql = " Update Submain_Account Set Account_Customer_Type_ID =" + cs.Id + " Where SubAccount_Id=" + cs.SubAccount_Id;
                // opp.insert_me(mysql);
                Excp = 0;
                //acc.UpdateSubmain_SubAccount_Id(ref Excp, ref Msg);
            }
            catch (Exception ee)
            {
                opp.Rollback();
                Excp = 1;
                Msg = ee.Message;
            }
        }
        public void AddNewCustomerSubAccount(ref int Excp, ref string Msg, ref operations opp, Customer cs)
        {
            long New_Submain_Account_Id = -1;
            if (cs.General_Customer_Flage == 0)
            {
                Excp = 0;
                Msg = "";
                string Account_Code = "";
                acc = new Sub_Account();
                int Account_Type_Value = Common.Customer_Account_Type;
                long Currency_Id = cs.Currency_Id ?? 1;
                long Account_Branch_Id = Common.Branch_Id;

                acc.cuCls.cs = cs;

                acc.Add_AccountSupplierCustomer(ref Excp, ref Msg, ref opp, ref New_Submain_Account_Id, ref Account_Code, Account_Type_Value, cs.Name, Account_Branch_Id, Currency_Id);

                //cs.cs.Id = acc.New_Account_Id;
                //cs.cs.SubAccount_Id = New_Submain_Account_Id;
                Customer cs2 = opp.db.Customer.Where(c => c.Id == cs.Id).FirstOrDefault();
                cs2.SubAccount_Id = New_Submain_Account_Id;

                EditCustomer(ref opp, cs2);

                //opp.db.Entry(cs2).State = EntityState.Modified;
                //opp.db.SaveChanges();
            }
            else
            {
                int Account_Type_Value = Common.Customer_Account_Type;
                int Account_Type_ValueGeneralCustomer = 1;
                long Main_Account_Id = 0;
                int Cost_Center_Tree_ID = 0;
                long Account_Branch_Id = Common.Branch_Id;
                Common.Get_Account_Type_Code(ref opp, Account_Type_Value, Account_Type_ValueGeneralCustomer, ref Main_Account_Id, ref Cost_Center_Tree_ID, Account_Branch_Id);

                Customer cs2 = opp.db.Customer.Where(c => c.Id == cs.Id).FirstOrDefault();
                cs2.SubAccount_Id = Main_Account_Id;

                EditCustomer(ref opp, cs2);

                //opp.db.Entry(cs2).State = EntityState.Modified;
                //opp.db.SaveChanges();
            }

        }
        void AddNewCustomerSubAccount(ref int Excp, ref operations opp, ref string Msg, Customer cs)
        {


        }


        public DataTable GetCustomerBalance(long Customer_Id)
        {
            int General_Customer_Flage = -1;
            long Group_Id = 0;
            int Ref_Cust_Supplier_Id = -1;
            int CustomerBalanceStatus = -1;

            string mysql = MakeCustomer_MokhtalatDataByGroupNew(General_Customer_Flage, Group_Id, Customer_Id, Ref_Cust_Supplier_Id, CustomerBalanceStatus);
            operations opp = new operations();
            DataTable dt = opp.return_DataTable(mysql);
            return dt;
        }
        // 🟢 دالة بحث جديدة (تقنية فقط): تقابل حرفياً استعلام Connection4It.GroupCustomer_List.GroupCustomer_Data
        // (عنصر تحكم WinForms فقط) عند استدعائه بـ GroupType=Common.Customer_Account_Type - نفس الاستعلام
        // والترتيب بالضبط، بدون أى تغيير فى منطق العمل.
        public DataTable Get_CustomerGroups()
        {
            operations opp = new operations();
            string mysql = " Select * from Customer_Supplier_Group where GroupType=" + Common.Customer_Account_Type + " Order by GroupName";
            return opp.return_DataTable(mysql);
        }
        public string MakeCustomer_MokhtalatDataByGroupNew(int General_Customer_Flage, long Group_Id, long Customer_Id, int Ref_Cust_Supplier_Id, int CustomerBalanceStatus)
        {
            DataTable dtAccount = new DataTable();
            operations opp = new operations();

            //string mysql = " Delete From Customers_Accounts_Mokhtalat";
            //opp.insert_me(mysql);

            // 🟢 Customers_Accounts_Mokhtalat.ID هو IDENTITY(1,1) حقيقى من SQL Server نفسه - عكس معظم جداول
            // هذا المشروع. opp.insert_me(sql) يمرّ دائماً عبر SetUpInsertCmd التى تُدرج قيمة Id صريحة تلقائياً
            // كلما كان Common.Use_Auto_Identity_Column = 0 (قيمته الفعلية فى قاعدة التطوير)، فيفشل الإدراج هنا
            // دائماً بصمت بخطأ "Cannot insert explicit value for identity column" (نفس الخلل الموثَّق ومُصلَح
            // سابقاً بنفس الطريقة فى UserSettingsCLS.SaveUserSetting - راجع تعليقها). الإصلاح: تنفيذ TRUNCATE
            // والـ INSERT...SELECT...UNION مباشرة عبر SqlCommand على opp.conn، متجاوزَين SetUpInsertCmd تماماً.
            string mysql = " truncate table Customers_Accounts_Mokhtalat";
            opp.MyConn();
            if (opp.conn.State == ConnectionState.Closed)
            {
                opp.conn.Open();
            }
            try
            {
                using (var truncateCmd = new SqlCommand(mysql, opp.conn))
                {
                    truncateCmd.ExecuteNonQuery();
                }
            }
            catch (Exception)
            {
            }
            try
            {

                //                mysql = @"  INSERT INTO Customers_Accounts_Mokhtalat
                //SELECT        Customers_Accounts.ID, Customers_Accounts.Customer_Id, Customers_Accounts.Import, Customers_Accounts.Export, Customers_Accounts.Balance, Customers_Accounts.Balance_Actual, 
                //                         Customers_Accounts.Transaction_Type, Customers_Accounts.Transaction_Date, Customers_Accounts.Reference_Id, Customers_Accounts.Reference_Type, Customers_Accounts.Reference_Type_Table_Id, 
                //                         Customers_Accounts.Payment_Type, Customers_Accounts.Notes, Customers_Accounts.Day_Registration_flage, Customers_Accounts.DayReg_List_Id, Customers_Accounts.Flage_View, 
                //                         Customers_Accounts.BalanceImport, Customers_Accounts.BalanceExport, Customers_Accounts.Reference_Type_View
                //FROM            Customers_Accounts INNER JOIN
                //                         Customer ON Customers_Accounts.Customer_Id = Customer.Id
                //WHERE        (Customers_Accounts.Reference_Type <> 25) 
                //UNION
                //SELECT        Suppliers_Accounts.ID, Supplier.Ref_Cust_Supplier_Id, Suppliers_Accounts.Import, Suppliers_Accounts.Export, Suppliers_Accounts.Balance, Suppliers_Accounts.Balance_Actual, 
                //                         Suppliers_Accounts.Transaction_Type, Suppliers_Accounts.Transaction_Date, Suppliers_Accounts.Reference_Id, Suppliers_Accounts.Reference_Type, Suppliers_Accounts.Reference_Type_Table_Id, 
                //                         Suppliers_Accounts.Payment_Type, Suppliers_Accounts.Notes, Suppliers_Accounts.Day_Registration_flage, Suppliers_Accounts.DayReg_List_Id, Suppliers_Accounts.Flage_View, 
                //                         Suppliers_Accounts.BalanceImport, Suppliers_Accounts.BalanceExport, Suppliers_Accounts.Reference_Type_View
                //FROM            Suppliers_Accounts INNER JOIN
                //                         Supplier ON Suppliers_Accounts.Supplier_Id = Supplier.Id
                //WHERE        (Suppliers_Accounts.Reference_Type <> 25) AND (Supplier.Ref_Cust_Supplier_Id > 0) ";

                mysql = @"  INSERT INTO Customers_Accounts_Mokhtalat
                         ( Customers_AccountsId, Customer_Id, Import, Export, Balance, Balance_Actual, Transaction_Type, Transaction_Date, Reference_Id, Reference_Type, Reference_Type_Table_Id, Payment_Type, Notes, Day_Registration_flage, DayReg_List_Id, 
                         Flage_View, BalanceImport, BalanceExport, Reference_Type_View)
SELECT        Customers_Accounts.Id, Customers_Accounts.Customer_Id, Customers_Accounts.Import, Customers_Accounts.Export, Customers_Accounts.Balance, Customers_Accounts.Balance_Actual, 
                         Customers_Accounts.Transaction_Type, Customers_Accounts.Transaction_Date, Customers_Accounts.Reference_Id, Customers_Accounts.Reference_Type, Customers_Accounts.Reference_Type_Table_Id, 
                         Customers_Accounts.Payment_Type, Customers_Accounts.Notes, Customers_Accounts.Day_Registration_flage, Customers_Accounts.DayReg_List_Id, Customers_Accounts.Flage_View, 
                         Customers_Accounts.BalanceImport, Customers_Accounts.BalanceExport, Customers_Accounts.Reference_Type_View
FROM            Customers_Accounts INNER JOIN
                         Customer ON Customers_Accounts.Customer_Id = Customer.Id
WHERE        (Customers_Accounts.Reference_Type <> 25) ";

                if (General_Customer_Flage > -1)
                {
                    mysql = mysql + @" AND Customer.General_Customer_Flage =" + General_Customer_Flage + "";
                }
                mysql = mysql + @"UNION
                SELECT        Suppliers_Accounts.Id,  Supplier.Ref_Cust_Supplier_Id, Suppliers_Accounts.Import, Suppliers_Accounts.Export, Suppliers_Accounts.Balance, Suppliers_Accounts.Balance_Actual, 
                         Suppliers_Accounts.Transaction_Type, Suppliers_Accounts.Transaction_Date, Suppliers_Accounts.Reference_Id, Suppliers_Accounts.Reference_Type, Suppliers_Accounts.Reference_Type_Table_Id, 
                         Suppliers_Accounts.Payment_Type, Suppliers_Accounts.Notes, Suppliers_Accounts.Day_Registration_flage, Suppliers_Accounts.DayReg_List_Id, Suppliers_Accounts.Flage_View, 
                         Suppliers_Accounts.BalanceImport, Suppliers_Accounts.BalanceExport, Suppliers_Accounts.Reference_Type_View
FROM            Suppliers_Accounts INNER JOIN
                         Supplier ON Suppliers_Accounts.Supplier_Id = Supplier.Id
WHERE        (Suppliers_Accounts.Reference_Type <> 25) AND (Supplier.Ref_Cust_Supplier_Id > 0) ";

                //AND (Customer.Ref_Cust_Supplier_Id > 0)
                using (var insertCmd = new SqlCommand(mysql, opp.conn))
                {
                    insertCmd.ExecuteNonQuery();
                }

            }
            catch (Exception ee)
            {
                //MessageBox.Show(ee.Message);
            }
            finally
            {
                opp.conn.Close();
            }
            mysql = Get_CustomerBalanceMokhtalat_SQL(General_Customer_Flage, Group_Id, Customer_Id, Ref_Cust_Supplier_Id, CustomerBalanceStatus);

            return mysql;
        }
        public string Get_CustomerBalanceMokhtalat_SQL(int General_Customer_Flage, long Group_Id, long Customer_Id, int Ref_Cust_Supplier_Id, int CustomerBalanceStatus)
        {
            string mysql = "";
            mysql = @"SELECT        SUM(Customers_Accounts_Mokhtalat.Export) - SUM(Customers_Accounts_Mokhtalat.Import) AS Balance, Customer.Id, Customer.Name, 0.0 AS Import, 0.0 AS Export, '' AS Serial_
FROM            Customer INNER JOIN
                         Customers_Accounts_Mokhtalat ON Customer.Id = Customers_Accounts_Mokhtalat.Customer_Id ";
            mysql = mysql + " where Customer.active=1  And  Customer.Customer_Type= " + Common.Customer_Type + "  ";
            //AND Customer.General_Customer_Flage=" + General_Customer_Flage + "   
            if (Customer_Id > 0)
            {
                mysql = mysql + " And Customer.Id=" + Customer_Id + "";
            }
            if (Group_Id > 0)
            {
                mysql = mysql + " And Customer.Group_Id=" + Group_Id + "";
            }
            //if (ShowMokhtalat == true)
            //{
            //    ////mysql = mysql + "  AND(Customer.Ref_Cust_Supplier_Id > 0)";
            //}
            //else
            //{
            //    mysql = mysql + "  AND(Customer.Ref_Cust_Supplier_Id = 0)";
            //}

            if (Ref_Cust_Supplier_Id == 0)
            {
                mysql = mysql + " And Customer.Ref_Cust_Supplier_Id= 0 ";
            }
            else if (Ref_Cust_Supplier_Id > 0)
            {
                mysql = mysql + " And Customer.Ref_Cust_Supplier_Id > 0";
            }
            mysql = mysql + " GROUP BY Customer.Id, Customer.Name ";


            if (CustomerBalanceStatus == 0)
            {
                mysql = mysql + "   HAVING        (SUM(Customers_Accounts_Mokhtalat.Export) - SUM(Customers_Accounts_Mokhtalat.Import) = 0) ";
            }
            else if (CustomerBalanceStatus == 1)
            {
                mysql = mysql + "   HAVING        (SUM(Customers_Accounts_Mokhtalat.Export) - SUM(Customers_Accounts_Mokhtalat.Import) != 0) ";
            }


            mysql = mysql + " ORDER BY Customer.Name ";

            return mysql;
        }
        // 🟢 دالة جديدة (تقنية فقط): تقابل حرفياً استعلام Connection4It.Customer_List1.Customer_SupplierListData
        // (عنصر تحكم WinForms فقط، يبنى قائمة العملاء الأساسية لشاشات "كشف حساب عميل" الأربع المُدمَجة -
        // Customer_AccountMokhtalat[.,_Period,_EF,_EF_Period]) - نفس الاستعلام والترتيب بالضبط، بدون أى تغيير فى
        // منطق العمل، نُقلت هنا فقط لأن Connection4It غير متاح من مشروع Blazor.
        public DataTable Get_CustomerSupplierListForStatement(long Group_Id, long Customer_Id, int Ref_Cust_Supplier_Id, int General_Customer_Flage)
        {
            string mysql = @" SELECT        Customer.* FROM Customer ";
            mysql = mysql + " Where Customer.Id > 0 AND  (Active = 1) ";
            if (Group_Id > 0)
            {
                mysql = mysql + " And Customer.Group_Id=" + Group_Id + "";
            }
            if (Customer_Id > 0)
            {
                mysql = mysql + " And Customer.Id=" + Customer_Id + "";
            }
            if (Ref_Cust_Supplier_Id == 0)
            {
                mysql = mysql + " And Customer.Ref_Cust_Supplier_Id= 0 ";
            }
            else if (Ref_Cust_Supplier_Id > 0)
            {
                mysql = mysql + " And Customer.Ref_Cust_Supplier_Id > 0";
            }
            if (General_Customer_Flage > -1)
            {
                mysql = mysql + " And Customer.General_Customer_Flage =" + General_Customer_Flage + "";
            }
            mysql = mysql + " Order by Customer.Name";
            operations opp3 = new operations();
            return opp3.return_DataTable(mysql);
        }
        // 🟢 دالة جديدة (تقنية فقط): تقابل حرفياً استعلام Connection4It.Customer_List1.Customer_SupplierData
        // (عنصر تحكم WinForms فقط، يبنى القائمة الأساسية لشاشة "كشف أرصدة الحسابات المختلطة" -
        // Customer_SupplierAccountFrmLv) - نفس الاستعلام والترتيب بالضبط، بدون أى تغيير فى منطق العمل، نُقلت هنا فقط
        // لأن Connection4It غير متاح من مشروع Blazor.
        public DataTable Get_CustomerSupplierMokhtalatList(long Group_Id, long Customer_Id)
        {
            string mysql = @" SELECT        Customer.*,Supplier.Id AS Supplier_Id , Supplier.Balance AS SupplierBalance, Supplier.Balance_Actual AS SupplierBalance_Actual
FROM Customer INNER JOIN
                         Supplier ON Customer.Id = Supplier.Ref_Cust_Supplier_Id ";
            mysql = mysql + " Where Customer.Id > 0";
            if (Group_Id > 0)
            {
                mysql = mysql + " And Customer.Group_Id=" + Group_Id + "";
            }
            if (Customer_Id > 0)
            {
                mysql = mysql + " And Customer.Id=" + Customer_Id + "";
            }
            mysql = mysql + " ORDER BY Customer.Name ";
            operations opp2 = new operations();
            return opp2.return_DataTable(mysql);
        }
        public string Get_CustomerBalance_SQL(int General_Customer_Flage, long Group_Id, long Customer_Id, bool ShowMokhtalat)
        {
            string mysql = "";
            mysql = @"SELECT SUM(Customers_Accounts.Export)  - SUM(Customers_Accounts.Import) AS Balance,SUM(Customers_Accounts.Export) AS MyExport, SUM(Customers_Accounts.Import) AS MyImport, Customer.Id, Customer.Name ,0.0 AS Import, 0.0 AS Export, '' AS Serial_ 
FROM Customer INNER JOIN Customers_Accounts ON Customer.Id = Customers_Accounts.Customer_Id ";
            mysql = mysql + " where Customer.active=1  And  Customer.Customer_Type= " + Common.Customer_Type + " AND Customer.General_Customer_Flage=" + General_Customer_Flage + "    ";
            if (Customer_Id > 0)
            {
                mysql = mysql + " And Customer.Id=" + Customer_Id + "";
            }
            if (Group_Id > 0)
            {
                mysql = mysql + " And Customer.Group_Id=" + Group_Id + "";
            }
            if (ShowMokhtalat == true)
            {
                ////mysql = mysql + "  AND(Customer.Ref_Cust_Supplier_Id > 0)";
            }
            else
            {
                mysql = mysql + "  AND(Customer.Ref_Cust_Supplier_Id = 0)";
            }



            mysql = mysql + " GROUP BY Customer.Id, Customer.Name ";

            mysql = mysql + " ORDER BY Customer.Name ";

            return mysql;
        }


    }

}
