using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

using Microsoft.EntityFrameworkCore;
namespace AccountModelsLib.Models
{
    public class SupplierCLS
    {
        public Supplier sp = new Supplier();
        public operations opp;

        public SupplierCLS(operations opp_)
        {
            opp = opp_; ;
            SetDefault();
        }
        public SupplierCLS(bool No_opp_)
        {

            SetDefault();
        }
        public SupplierCLS()
        {
            opp = new operations();
            SetDefault();
        }
        void SetDefault()
        {
            sp = new Supplier()
            {
                Supplier_Type = Common.Customer_Type,
                Currency_Id = 1,
                SubAccount_Id = 0,
                Group_Id = 1,

                Balance = 0,
                Balance_Actual = 0

            };
        }
        public void Get_Supplier(long Supplier_Id)
        {


            Get_SupplierById(ref opp, Supplier_Id);

        }
        public void Get_SupplierById(ref operations opp, long Supplier_Id)
        {
            //string mysql = " Select * from Supplier where Id=" + Supplier_Id;
            //dt = opp.return_DataTable(mysql);

            sp = new Supplier();
            sp = opp.db.Suppliers.Where(s => s.Id == Supplier_Id).FirstOrDefault();
            //ReadSupplierData(dt, 0);


        }


        public List<Supplier> Get_SupplierByActive(int Active)
        {
            //string mysql = " Select * from Supplier where Id=" + Supplier_Id;
            //dt = opp.return_DataTable(mysql);

            List<Supplier> spList = new List<Supplier>();
            if (Active > -1)
            {
                spList = opp.db.Suppliers.Where(s => s.Active == Active).ToList();
            }
            else
            {
                spList = opp.db.Suppliers.ToList();
            }

            return spList;
            //ReadSupplierData(dt, 0);


        }
        public List<SupplierViewModel> Get_SupplierVMByActive(ref operations opp, int Active)
        {

            IQueryable<Supplier> query = opp.db.Suppliers;

            if (Active > -1)
            {
                query = query.Where(s => s.Active == Active);
            }
            else
            {
                query = opp.db.Suppliers;
            }
            query = query.OrderBy(s => s.Name);
            return Get_SupplierViewModelList(query);



        }

        public List<SupplierViewModel> Get_SupplierViewModelList(IQueryable<Supplier> query)
        {
            List<SupplierViewModel> ls = new List<SupplierViewModel>();
            int Serial = 0;
            foreach (var item in query)
            {
                SupplierViewModel gg = new SupplierViewModel();
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


                    gg.Date_Balance_First = item.Date_Balance_First;
                    gg.Balance_First = item.Balance_First;

                    gg.Tax_Registration_No = item.Tax_Registration_No;


                    gg.Account_Code = item.Account_Code;
                    gg.Currency_Id = item.Currency_Id;


                    gg.Ref_Cust_Supplier_Id = item.Ref_Cust_Supplier_Id;



                    gg.OperationDate = item.OperationDate;
                    gg.User_Id = item.User_Id;
                    gg.Branch_Id = item.Branch_Id;
                    gg.Insert_ServerDateTime = item.Insert_ServerDateTime;
                    gg.Update_ServerDateTime = item.Update_ServerDateTime;
                    gg.EditOperationDate = item.EditOperationDate;
                    gg.Edit_User_Id = item.Edit_User_Id;

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

                    try
                    {
                        if (item.Customer_Supplier_Groups != null)
                        {
                            gg.GroupName = item.Customer_Supplier_Groups.GroupName;
                        }
                        else
                        {
                            gg.GroupName = "";
                        }
                    }
                    catch (Exception)
                    {
                        gg.GroupName = "";
                    }

                    try
                    {
                        if (item.Gov_City_Lists != null)
                        {
                            gg.CityName = item.Gov_City_Lists.NameAr;
                        }
                        else
                        {
                            gg.CityName = "";
                        }

                    }
                    catch (Exception ee)
                    {
                        gg.CityName = "";
                    }
                    try
                    {
                        if (item.Govern_Lists != null)
                        {
                            gg.GovernName = item.Govern_Lists.Gov_Name;
                        }
                        else
                        {
                            gg.GovernName = "";

                        }

                    }
                    catch (Exception ee)
                    {
                        gg.GovernName = "";
                    }
                    try
                    {
                        if (item.Govern_Area_Lists != null)
                        {
                            gg.AreaName = item.Govern_Area_Lists.Area_Name;
                        }
                        else
                        {
                            gg.AreaName = "";
                        }

                    }
                    catch (Exception ee)
                    {
                        gg.AreaName = "";
                    }
                    try
                    {
                        if (item.Gov_Country_Lists != null)
                        {
                            gg.CountryName = item.Gov_Country_Lists.NameAr;
                        }
                        else
                        {
                            gg.CountryName = "";
                        }
                    }
                    catch (Exception ee)
                    {
                        gg.CountryName = "";
                    }
                    try
                    {
                        if (item.Submain_Account != null)
                        {
                            gg.SubAccount_Name = item.Submain_Account.SubAccount_Name;
                        }
                        else
                        {
                            gg.SubAccount_Name = "";
                        }
                    }
                    catch (Exception ee)
                    {
                        gg.SubAccount_Name = "";
                    }


                    gg.SubAccount_Id = item.SubAccount_Id;

                }
                catch (Exception ee)
                {

                }
                ls.Add(gg);
            }
            ls.OrderBy(i => i.Name);
            return ls;
        }

        public void Get_SupplierBySubAccount_Id(ref operations opp, long SubAccount_Id)
        {
            //string mysql = " Select * from Supplier where SubAccount_Id =" + SubAccount_Id;
            //dt = opp.return_DataTable(mysql);
            sp = new Supplier();
            sp = opp.db.Suppliers.Where(s => s.SubAccount_Id == SubAccount_Id).FirstOrDefault();
            //ReadSupplierData(dt, 0);


        }
        //public DataTable Get_SupplierBySubAccount_Id(ref operations opp, long SubAccount_Id)
        //{
        //    string mysql = "SELECT * FROM Supplier where SubAccount_Id =" + SubAccount_Id;
        //    DataTable dt = opp.return_DataTable(mysql);

        //    return dt;
        //}
        public DataTable GetSupplier(ref operations opp)
        {
            string mysql = " SELECT * FROM Supplier  Where active=1 Order by name";
            DataTable dt = opp.return_DataTable(mysql);

            return dt;
        }
        public void ReadSupplierData(DataTable dt, int index)
        {
            /*
            if (dt.Rows.Count > 0)
            {
                Id = long.Parse(dt.Rows[index]["Id"].ToString());
                Name = dt.Rows[index]["Name"].ToString();
                Tax_Registration_No = dt.Rows[index]["Tax_Registration_No"].ToString();
                Code = dt.Rows[index]["Code"].ToString();
                Address = dt.Rows[index]["Address"].ToString();
                Tel = dt.Rows[index]["Tel"].ToString();
                Tel2 = dt.Rows[index]["Tel2"].ToString();
                Mobile = dt.Rows[index]["Mobile"].ToString();
                E_Mail = dt.Rows[index]["E_Mail"].ToString();
                Fax = dt.Rows[index]["Fax"].ToString();
                Web_Site = dt.Rows[index]["Web_Site"].ToString();
                Balance = decimal.Parse(dt.Rows[index]["Balance"].ToString());
                Balance_Actual = decimal.Parse(dt.Rows[index]["Balance_Actual"].ToString());
                SubAccount_Id = long.Parse(dt.Rows[index]["SubAccount_Id"].ToString());
                Currency_Id = long.Parse(dt.Rows[index]["Currency_Id"].ToString());
                try
                {
                    Supplier_Type = int.Parse(dt.Rows[index]["Supplier_Type"].ToString());
                }
                catch (Exception ee)
                {

                    Supplier_Type = 0;
                }
                try
                {
                    Group_Id = int.Parse(dt.Rows[index]["Group_Id"].ToString());
                }
                catch (Exception ee)
                {

                    Group_Id = 0;
                }
                try
                {
                    Ref_Cust_Supplier_Id = long.Parse(dt.Rows[index]["Ref_Cust_Supplier_Id"].ToString());
                }
                catch (Exception ee)
                {

                    Ref_Cust_Supplier_Id = 0;
                }

                Active = int.Parse(dt.Rows[index]["Active"].ToString());

                CompanyName = dt.Rows[index]["CompanyName"].ToString();
                CompanyContactName = dt.Rows[index]["CompanyContactName"].ToString();
                CompanyContactData = dt.Rows[index]["CompanyContactData"].ToString();
                NationalId = dt.Rows[index]["NationalId"].ToString();
                PassportNo = dt.Rows[index]["PassportNo"].ToString();
                PostalNo = dt.Rows[index]["PostalNo"].ToString();
                Gov_Country_ListId = int.Parse(dt.Rows[index]["Gov_Country_ListId"].ToString());
                Govern_Area_ListId = int.Parse(dt.Rows[index]["Govern_Area_ListId"].ToString());

                Govern_ListId = int.Parse(dt.Rows[index]["Govern_ListId"].ToString());
                Gov_City_ListId = int.Parse(dt.Rows[index]["Gov_City_ListId"].ToString());
                try
                {
                    PersonCompanyType = int.Parse(dt.Rows[index]["PersonCompanyType"].ToString());
                }
                catch (Exception ee)
                {
                    PersonCompanyType = 1;
                }
                RegistrationDate = DateTime.Parse(dt.Rows[index]["RegistrationDate"].ToString());
                Commercial_Register = dt.Rows[index]["Commercial_Register"].ToString();


            }
       */
        }

        public void InsertSupplier(ref int Excp, ref operations opp)
        {
            string mysql = "";

            if (Common.Use_AutoNumeric_Supplier_Id_With_Submain_Account_Id == 1)
            {
                //opp.db.Entry(sp).State = EntityState.Added;

                opp.db.Suppliers.Add(sp);
                opp.db.SaveChanges();

                //mysql = "INSERT INTO Supplier(Currency_Id,Group_Id,Tax_Registration_No,Ref_Cust_Supplier_Id,code, Name, Address, Tel, Tel2, Mobile, E_Mail, Fax, Web_Site, Balance,Balance_Actual,  SubAccount_Id,Supplier_Type,Active,Account_Code)";
                //mysql += " VALUES(" + sp.Currency_Id + "," + sp.Group_Id + ",'" + sp.Tax_Registration_No + "'," + sp.Ref_Cust_Supplier_Id + ",'" + sp.Code + "','" + sp.Name + "','" + sp.Address + "','" + sp.Tel + "','" + sp.Tel2 + "','" + sp.Mobile + "','" + sp.E_Mail + "','" + sp.Fax + "','" + sp.Web_Site + "'," + sp.Balance + "," + sp.Balance + ", " + sp.SubAccount_Id + "," + sp.Supplier_Type + "," + sp.Active + ",'" + sp.Account_Code + "')";

                //sp.Id = opp.Insert_me_Return_Last_Identify(mysql);
            }
            else
            {
                //mysql = "INSERT INTO Supplier(Id,Currency_Id,Group_Id,Tax_Registration_No,Ref_Cust_Supplier_Id,code, Name, Address, Tel, Tel2, Mobile, E_Mail, Fax, Web_Site, Balance,Balance_Actual,  SubAccount_Id,Supplier_Type,Active,Account_Code)";
                //mysql += " VALUES(" + sp.SubAccount_Id + "," + sp.Currency_Id + "," + sp.Group_Id + ",'" + sp.Tax_Registration_No + "'," + sp.Ref_Cust_Supplier_Id + ",'" + sp.Code + "','" + sp.Name + "','" + sp.Address + "','" + sp.Tel + "','" + sp.Tel2 + "','" + sp.Mobile + "','" + sp.E_Mail + "','" + sp.Fax + "','" + sp.Web_Site + "'," + sp.Balance + "," + sp.Balance + ", " + sp.SubAccount_Id + "," + sp.Supplier_Type + "," + sp.Active + ",'" + sp.Account_Code + "')";

                //opp.insert_me(mysql);
                sp.Id = sp.SubAccount_Id.Value;

                opp.db.Suppliers.Add(sp);
                opp.db.SaveChanges();

            }

            //mysql = " Update Supplier Set  CompanyName= '" + sp.CompanyName + "'  ";
            //mysql = mysql + " , CompanyContactName= '" + sp.CompanyContactName + "'  ";
            //mysql = mysql + " , CompanyContactData= '" + sp.CompanyContactData + "'  ";
            //mysql = mysql + " , NationalId= '" + sp.NationalId + "'  ";
            //mysql = mysql + " , PassportNo= '" + sp.PassportNo + "'  ";
            //mysql = mysql + " , PostalNo= '" + sp.PostalNo + "'  ";
            //mysql = mysql + " , Commercial_Register= '" + sp.Commercial_Register + "'  ";
            //mysql = mysql + " , RegistrationDate= '" + operations.dateFormat(sp.RegistrationDate) + "'  ";
            //mysql = mysql + " , Gov_Country_ListId=  " + sp.Gov_Country_ListId + "  ";
            //mysql = mysql + " , Govern_Area_ListId=  " + sp.Govern_Area_ListId + "  ";
            //mysql = mysql + " , Govern_ListId=  " + sp.Govern_ListId + "  ";
            //mysql = mysql + " , Gov_City_ListId=  " + sp.Gov_City_ListId + "  ";
            //mysql = mysql + " Where Id=" + sp.Id + "";
            //opp.insert_me(mysql);

            string Msg = "";
            FirstBalance_CLS ff = new FirstBalance_CLS();
            long Year_Financial_Period_Id = Financial_PeriodCLS.Get_Year_Financial_Period_Id(DateTime.Now);
            ff.IsCheckLastLevel = true;
            ff.Account_Customer_Type_ID = sp.Id;
            ff.Account_Customer_Type_Value = Common.Supplier_Account_Type;

            ff.Save_FirstBalance(ref Excp, ref Msg, ref opp, sp.SubAccount_Id.Value, sp.Name, Year_Financial_Period_Id, Common.Branch_Id, sp.Currency_Id ?? 1, 0, 1, 1);

        }
        public long InsertSupplier(ref int Excp, ref string Msg, ref operations opp, Submain_Account sa_)
        {
            if (sp.Id == 0)
            {
                if (Common.Use_AutoNumeric_Supplier_Id_With_Submain_Account_Id == 0 | Common.Use_Auto_Identity_Column == 0)
                {
                    sp.Id = sa_.Id;
                }

                string mysql = "";
                mysql = "Select Id From Supplier Where Id=" + sp.Id;
                DataTable dt = opp.return_DataTable(mysql);
                int C = dt.Rows.Count;
                if (C == 0)
                {
                    //sp.Name = sa_.SubAccount_Name;
                    sp.Currency_Id = sa_.Currency_Id;
                    sp.SubAccount_Id = sa_.Id;
                    sp.Account_Code = sa_.SubAccount_NO;
                    sp.Active = 1;
                    // sp.Ref_Cust_Supplier_Id = 0;


                    //
                    opp.db.Suppliers.Add(sp);
                    opp.db.SaveChanges();

                }


                Save_FirstBalance(ref Excp, ref Msg, ref opp, sp);
                return sp.Id;
            }
            else
            {
                opp.db.Entry(sp).State = EntityState.Modified;
                opp.db.SaveChanges();
                return sp.Id;
            }
        }
        public long UpdateSupplierOnly(ref int Excp, ref string Msg, ref operations opp )
        {
             
                 
                opp.db.Entry(sp).State = EntityState.Modified;
                opp.db.SaveChanges();
                return sp.Id;
             
        }
        void Save_FirstBalance(ref int Excp, ref string Msg, ref operations opp, Supplier sp)
        {
            FirstBalance_CLS ff = new FirstBalance_CLS();
            long Year_Financial_Period_Id = Financial_PeriodCLS.Get_Year_Financial_Period_Id(DateTime.Now);
            ff.IsCheckLastLevel = true;
            ff.Account_Customer_Type_ID = sp.Id;
            ff.Account_Customer_Type_Value = Common.Supplier_Account_Type;

            ff.Save_FirstBalance(ref Excp, ref Msg, ref opp, sp.SubAccount_Id.Value, sp.Name, Year_Financial_Period_Id, Common.Branch_Id, sp.Currency_Id ?? 1, 0, 1, 1);

        }
        public void UpdateSupplier(ref int Excp, ref string Msg, ref operations opp, Submain_Account sa)
        {

            Get_SupplierBySubAccount_Id(ref opp, sa.Id);
            if (sp == null)
            {
                //sp.Id = sa.SubAccount_Id;
                sp.Name = sa.SubAccount_Name;
                sp.Currency_Id = sa.Currency_Id;
                sp.Account_Code = sa.Account_Code;
                sp.SubAccount_Id = sa.Id;
                sp.Balance = 0;
                sp.Balance_Actual = 0;
                sp.Group_Id = 1;
                InsertSupplier(ref Excp, ref opp);
            }
            else
            {
                //Id = long.Parse(dt.Rows[0]["Id"].ToString());
                //Get_Supplier(sp.Id);

                sp.Name = sa.SubAccount_Name;
                sp.Currency_Id = sa.Currency_Id;
                sp.Account_Code = sa.Account_Code;
                sp.SubAccount_Id = sa.Id;

                UpdateSupplier(ref Excp, ref Msg, ref opp);
            }
        }
        public void UpdateSupplier(ref int Excp, ref string Msg, ref operations opp)
        {
            string mysql = "";

            //sa = new Submain_AccountCLS();
            //sa = SetUpSubmain_AccountCLS(ref Excp, ref Msg, ref opp, ref New_Submain_Account_Id, ref Account_Code, Account_Type_Value, Account_Name, Account_Branch_Id, Currency_Id, Account_Type_ValueGeneralCustomer);
            ////Add_Account(ref Excp, ref Msg, ref opp, ref New_Submain_Account_Id, sa);


            ////Add_Account_(ref Excp, ref Msg, ref opp, ref New_Submain_Account_Id, sa.sb, Add_Account_Customer_List);

            //if (IsUse_SubAccount_Id_Select == false)
            //{
            //    /////اضافة فى جدول اسماء الحسابات
            //    Add_Submain_Account(ref Excp, ref Msg, ref opp, ref New_Submain_Account_Id, sa.sb, Add_Account_Customer_List);
            //}
            //opp.db.Entry(sp).State = EntityState.Modified;
            //opp.db.SaveChanges();

            //sp.SubAccount_Id = sp.SubAccount_Id ?? 0;
            //long Account_Customer_Type_ID = sp.Id;
            //int Account_Customer_Type_Value = Common.Supplier_Account_Type;
            //long SubAccount_Id_ = sp.SubAccount_Id.Value;

            //Sub_Account sa = new Sub_Account();
            //sa.UpdateSubmain_SubAccount_Name(ref Excp, ref Msg, ref opp, sp.Name, Account_Customer_Type_ID, Account_Customer_Type_Value, ref SubAccount_Id_);


            //sp.SubAccount_Id = SubAccount_Id_;
            opp.db.Entry(sp).State = EntityState.Modified;
            opp.db.SaveChanges();

            //mysql = "UPDATE Supplier SET  SubAccount_Id=" + SubAccount_Id_ + "";
            //mysql += " where Id=" + sp.Id;
            //opp.insert_me(mysql);

        }

        public void UpdateSupplier(ref int Excp, ref string Msg, Supplier sp,long New_Submain_Account_Id, bool IsUse_SubAccount_Id_Select)
        {
            //operations opp = new operations(1);
            opp.BeginTransaction(2);
            try
            {
                sp.SubAccount_Id = sp.SubAccount_Id ?? 0;
                Sub_Account acc = new Sub_Account();
                int Account_Type_Value = Common.Supplier_Account_Type;
                  
                long  Account_Branch_Id = sp.Branch_Id ?? 1;// Common.Branch_Id;
                string Account_Code = "";


                acc.IsUse_SubAccount_Id_Select = IsUse_SubAccount_Id_Select;

                if (IsUse_SubAccount_Id_Select == false )
                {
                    New_Submain_Account_Id = sp.SubAccount_Id ?? -1;

                }
                 
                acc.spCls.sp = sp;
                acc.Update_AccountSupplierCustomerOther(ref Excp, ref Msg, ref opp, ref New_Submain_Account_Id, ref Account_Code, Account_Type_Value, sp.Name, Account_Branch_Id, sp.Currency_Id ?? 1);
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
        public bool IsFoundName(ref operations opp, string Name, long Id = 0)
        {
            bool IsFound = false;
            string mysql = " Select * from Supplier where    Name = '" + Name.Trim() + "'";
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
        public void DeleteSupplier(ref int Excp, ref string Msg, long Id)
        {
            operations opp = new operations();
            DeleteSupplier(ref opp, ref Excp, ref Msg, Id);
        }

        public void DeleteSupplier(ref operations opp, ref int Excp, ref string Msg, long Id)
        {
            Excp = 0;
            Supplier sp = (Supplier)opp.db.Suppliers.Where(c => c.Id == Id).FirstOrDefault();
            if (sp != null)
            {
                long c = (long)sp.SubAccount_Id;
            }

            var saa = (Suppliers_Accounts)opp.db.Suppliers_Accounts.Where(c => c.Supplier_Id == Id).FirstOrDefault();
            if (saa != null)
            {
                if (saa.Reference_Type != -7)
                {
                    Excp = 1;
                    Msg = "لا يمكن حذف المورد نظرا لوجود عمليات محاسبية على الحساب";
                }
            }
            sp.SubAccount_Id = sp.SubAccount_Id ?? 0;
            var acd = (Account_Entry_Details)opp.db.Account_Entry_Details.Where(c => c.Submain_Account_ID == sp.SubAccount_Id).FirstOrDefault();
            if (acd != null)
            {
                Excp = 1;
                Msg = "لا يمكن حذف المورد نظرا لوجود عمليات محاسبية على الحساب";
            }

            if (Excp == 0)
            {
                //string mysql = "";
                //if (Common.Update_DeleteSupplier == 0)
                //{
                //    mysql = "Update Supplier set active=0 Where Id= " + Id;
                //}
                //if (Common.Update_DeleteSupplier == 1)
                //{
                //    mysql = "delete from Supplier where Id= " + Id;
                //}
                Excp = 0;
                Msg = "";
                //opp.insert_me(mysql);
                Sub_Account sa = new Sub_Account();
                int Account_Customer_Type_Value = 5;
                sa.Delete_Account(ref Excp, ref Msg, ref opp, Account_Customer_Type_Value, Id, sp.SubAccount_Id.Value);
            }
        }
        public void MakeDeleteSupplier(ref operations opp, ref int Excp, ref string Msg, long Id, long SubAccount_Id)
        {
            string mysql = "";
            if (Common.Update_DeleteSupplier == 0)
            {
                mysql = "Update Supplier set active=0 Where Id= " + Id;
                mysql = mysql + "Update Supplier set active=0 Where SubAccount_Id= " + SubAccount_Id;
            }
            if (Common.Update_DeleteSupplier == 1)
            {
                mysql = "delete from Supplier where Id= " + Id;
                mysql = mysql + "delete from Supplier where SubAccount_Id= " + SubAccount_Id;

                mysql = mysql + " DELETE FROM Suppliers_Accounts WHERE  Supplier_Id = " + Id;


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
        public void AddNewSupplier(ref int Excp, ref string Msg, Supplier sp, bool IsUse_SubAccount_Id_Select)
        {
            //operations opp = new operations(1);
            opp.BeginTransaction(2);
            try
            {

                Sub_Account acc = new Sub_Account();
                int Account_Type_Value = Common.Supplier_Account_Type;
                long New_Submain_Account_Id = -1;
                long  Account_Branch_Id = Common.Branch_Id;
                string Account_Code = "";
                sp.SubAccount_Id = sp.SubAccount_Id ?? 0;
                if (IsUse_SubAccount_Id_Select == true)
                {
                    New_Submain_Account_Id = sp.SubAccount_Id.Value;
                    acc.IsUse_SubAccount_Id_Select = IsUse_SubAccount_Id_Select;
                }

                acc.spCls.sp = sp;
                acc.Add_AccountSupplierCustomerOther(ref Excp, ref Msg, ref opp, ref New_Submain_Account_Id, ref Account_Code, Account_Type_Value, sp.Name, Account_Branch_Id, sp.Currency_Id ?? 1);
                acc.UpdateSubmain_SubAccount_Id(ref Excp, ref opp, ref Msg);

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

        public void AddNewSupplierAccount(ref int Excp, ref string Msg, SupplierCLS sp, bool IsUse_SubAccount_Id_Select)
        {
            //operations opp = new operations(1);
            opp.BeginTransaction(2);
            try
            {

                Sub_Account acc = new Sub_Account();
                int Account_Type_Value = Common.Supplier_Account_Type;
                long New_Submain_Account_Id = -1;
                long  Account_Branch_Id = Common.Branch_Id;
                string Account_Code = "";
                sp.sp.SubAccount_Id = sp.sp.SubAccount_Id ?? 0;
                if (IsUse_SubAccount_Id_Select == true)
                {
                    New_Submain_Account_Id = sp.sp.SubAccount_Id.Value;
                    acc.IsUse_SubAccount_Id_Select = IsUse_SubAccount_Id_Select;
                }

                acc.spCls = sp;
                acc.Add_AccountSupplierCustomerOnlyAccount(ref Excp, ref Msg, ref opp, ref New_Submain_Account_Id, ref Account_Code, Account_Type_Value, sp.sp.Name, Account_Branch_Id, sp.sp.Currency_Id ?? 1);

                Supplier spp = opp.db.Suppliers.Where(s => s.Id == sp.sp.Id).FirstOrDefault();
                spp.SubAccount_Id = New_Submain_Account_Id;
                opp.db.Entry(spp).State = EntityState.Modified;
                opp.db.SaveChanges();

                opp.Commit();
                Excp = 0;




            }
            catch (Exception ee)
            {
                opp.Rollback();
                Excp = 1;
                Msg = ee.Message;
            }
        }
        public DataTable GetSupplier_DataBalance(long Supplier_Id)
        {
            long Group_Id = 0;
            long Ref_Cust_Supplier_Id = 0;
            bool ShowMokhtalat = true;
            int CustomerBalanceStatus = 2;

            string mysql = Get_Supplier_DataByGroupNew_SQL(Group_Id, Ref_Cust_Supplier_Id, Supplier_Id, ShowMokhtalat, CustomerBalanceStatus);
            operations opp = new operations();
            DataTable dt = opp.return_DataTable(mysql);
            return dt;
        }
        // 🟢 دالة بحث جديدة (تقنية فقط): تقابل حرفياً استعلام Connection4It.GroupCustomer_List.GroupCustomer_Data
        // (عنصر تحكم WinForms فقط) عند استدعائه بـ GroupType=Common.Supplier_Account_Type - نفس الاستعلام
        // والترتيب بالضبط، بدون أى تغيير فى منطق العمل.
        public DataTable Get_SupplierGroups()
        {
            operations opp = new operations();
            string mysql = " Select * from Customer_Supplier_Group where GroupType=" + Common.Supplier_Account_Type + " Order by GroupName";
            return opp.return_DataTable(mysql);
        }
        // 🟢 دالة جديدة (تقنية فقط): تقابل حرفياً استعلام Connection4It.Supplier_List.Get_Supplier_DataByGroupNewList_SQL
        // (عنصر تحكم WinForms فقط، يبنى قائمة الموردين الأساسية لشاشتَى "كشف حساب مورد" المُدمَجتين -
        // Supplier_AccountNewFrm[Period]) - نفس الاستعلام والترتيب بالضبط، بدون أى تغيير فى منطق العمل، نُقلت هنا
        // فقط لأن Connection4It غير متاح من مشروع Blazor.
        public DataTable Get_SupplierListForStatement(long Group_Id, long Ref_Cust_Supplier_Id, long Supplier_Id, bool ShowMokhtalat)
        {
            string mysql = @"SELECT Id, Name, '' AS Serial_ FROM            Supplier ";
            mysql = mysql + " where Supplier.active=1      ";
            if (Group_Id > 0)
            {
                mysql = mysql + " And  Supplier.Group_Id=" + Group_Id + "";
            }
            if (Ref_Cust_Supplier_Id > 0)
            {
                mysql = mysql + " And  Supplier.Ref_Cust_Supplier_Id=" + Ref_Cust_Supplier_Id + "";
            }
            if (Supplier_Id > 0)
            {
                mysql = mysql + " And  Supplier.Id=" + Supplier_Id + "";
            }
            if (ShowMokhtalat == true)
            {
                ////mysql = mysql + "  AND(Customer.Ref_Cust_Supplier_Id > 0)";
            }
            else
            {
                mysql = mysql + "  AND(Supplier.Ref_Cust_Supplier_Id = 0)";
            }
            mysql = mysql + " ORDER BY  Supplier.Name ";
            operations opp4 = new operations();
            return opp4.return_DataTable(mysql);
        }
        public string Get_Supplier_DataByGroupNew_SQL(long Group_Id, long Ref_Cust_Supplier_Id, long Supplier_Id, bool ShowMokhtalat, int CustomerBalanceStatus)
        {
            string mysql = "";
            mysql = @"SELECT   SUM( Suppliers_Accounts.Import) - SUM( Suppliers_Accounts.Export) AS Balance,  Supplier.Id, Supplier.Name  , 0.0 AS Import, 0.0 AS Export, '' AS Serial_
FROM  Supplier INNER JOIN  Suppliers_Accounts ON  Supplier.Id =  Suppliers_Accounts.Supplier_Id ";
            mysql = mysql + " where Supplier.active=1      ";
            if (Group_Id > 0)
            {
                mysql = mysql + " And  Supplier.Group_Id=" + Group_Id + "";
            }
            if (Ref_Cust_Supplier_Id > 0)
            {
                mysql = mysql + " And  Supplier.Ref_Cust_Supplier_Id=" + Ref_Cust_Supplier_Id + "";
            }
            if (Supplier_Id > 0)
            {
                mysql = mysql + " And  Supplier.Id=" + Supplier_Id + "";
            }

            if (ShowMokhtalat == true)
            {
                ////mysql = mysql + "  AND(Customer.Ref_Cust_Supplier_Id > 0)";
            }
            else
            {
                mysql = mysql + "  AND(Supplier.Ref_Cust_Supplier_Id = 0)";
            }
            mysql = mysql + " GROUP BY  Supplier.Id,  Supplier.Name ";
            if (CustomerBalanceStatus == 0)
            {
                mysql = mysql + "   HAVING        (SUM(Suppliers_Accounts.Export) - SUM(Suppliers_Accounts.Import) = 0) ";
            }
            else if (CustomerBalanceStatus == 1)
            {
                mysql = mysql + "   HAVING        (SUM(Suppliers_Accounts.Export) - SUM(Suppliers_Accounts.Import) != 0) ";
            }


            mysql = mysql + " ORDER BY  Supplier.Name ";
            return mysql;
        }
        public Supplier GetSupplierNoTracking(long supplier_Id)
        {

            sp = opp.db.Suppliers.AsNoTracking().Where(c => c.Id == supplier_Id).FirstOrDefault();

            return sp;
        }
        public Supplier GetSupplierNoTracking(ref operations opp, long supplier_Id)
        {

            sp = opp.db.Suppliers.AsNoTracking().Where(c => c.Id == supplier_Id).FirstOrDefault();

            return sp;
        }

        public DataTable GetSupplierToPrint(int Active)
        {
            string mysql = " SELECT         * FROM            Supplier ";
            mysql = mysql + " WHERE         Id >0  ";


            if (Active > 0)
            {
                mysql = mysql + " And  Active=" + Active + "";
            }




            mysql = mysql + "ORDER BY Name ";
            return opp.return_DataTable(mysql);

        }
    }
}
