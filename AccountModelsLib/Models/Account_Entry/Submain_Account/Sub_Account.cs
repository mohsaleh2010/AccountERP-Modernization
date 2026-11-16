using AccountModelsLib.Models;
using AccountModelsLib.Services;
using Microsoft.Data.SqlClient;
using Net.Pkcs11Interop.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.OleDb;
using System.Drawing;


namespace AccountModelsLib
{
    /// <summary>
    /// Summary description for BanksClss.
    /// </summary>
    [NotMapped]
    public class Sub_Account
    {
        public string SubAccount_Name_En = "";
        public int Is_MasrofatAffect = 0;
        public long New_Account_Id = 0;
        public CustomerCLS cuCls { get; set; } = new CustomerCLS(true);
        public SupplierCLS spCls = new SupplierCLS(true);
        public Bank ba = new Bank();
        public Customer cu = new Customer();
        public Supplier sp = new Supplier();
        public FixedAssetCLS fixedAssetCLS = new FixedAssetCLS();
        public Employee emp = new Employee();
        public Store store = new Store();
        public ShipmentCompany shipmentCompany = new ShipmentCompany();
        public int Accept_Cost_Center_Flage = 1;
        public Submain_AccountCLS sa = new Submain_AccountCLS();
        public bool IsUse_SubAccount_Id_Select = false;



        DataSet G_DS;
        public Sub_Account()
        {
            G_DS = new DataSet();
            // TODO: Add constructor logic here
            //
        }

        public bool Check_Account_No_Found_OnSubAccount(ref int Excp, long Main_Account_Id)
        {
            G_DS.Clear();
            operations opp = new operations();

            try
            {
                string mysql;
                mysql = "Select * from Submain_Account  where Main_Account_Id=" + Main_Account_Id;
                G_DS = opp.return_dataset(mysql, "Submain_Account");
                if (G_DS.Tables[0].Rows.Count > 0)
                {
                    Excp = 0;
                    opp = null;
                    return true;
                }
                else
                {
                    Excp = 0;
                    opp = null;
                    return false;
                }
            }
            catch
            {
                Excp = 0;
                opp = null;
                return true;
            }

        }

        public bool Check_Account_No_WithEntry_Used(ref int Excp, long Submain_Account_ID)
        {

            operations opp = new operations();

            try
            {
                string mysql;
                mysql = "Select * from Account_Entry_Details  where Submain_Account_ID=" + Submain_Account_ID;
                DataTable dt = opp.return_DataTable(mysql);
                if (dt.Rows.Count > 0)
                {
                    Excp = 0;
                    opp = null;
                    return true;
                }
                else
                {
                    Excp = 0;
                    opp = null;
                    return false;
                }
            }
            catch
            {
                Excp = 0;
                opp = null;
                return true;
            }

        }
        public bool Check_Account_No_Found(ref int Excp, ref operations opp, string Account_No, long Account_Branch_Id)
        {
            DataTable dt = new DataTable();
            try
            {
                string mysql;
                mysql = "Select * from Submain_Account where SubAccount_NO= '" + Account_No + "' And  Branch_Id=" + Account_Branch_Id + "";

                dt = opp.return_DataTable(mysql);
                if (dt.Rows.Count > 0)
                {
                    Excp = 0;

                    return true;
                }
                else
                {
                    Excp = 0;

                    return false;
                }


            }
            catch (Exception ee)
            {
                Excp = 1;
                return true;
            }
        }
        public bool Check_Account_No_FoundToEdit(ref int Excp, string Account_No, long SubAccount_Id)
        {
            operations opp = new operations();
            return Check_Account_No_FoundToEdit(ref Excp, ref opp, Account_No, SubAccount_Id);
        }
        public bool Check_Account_No_FoundToEdit(ref int Excp, ref operations opp, string Account_No, long SubAccount_Id)
        {

            try
            {
                string mysql;
                mysql = "Select * from Submain_Account where SubAccount_NO= '" + Account_No + "' And  Id <> " + SubAccount_Id + "";

                DataTable dt = opp.return_DataTable(mysql);
                if (dt.Rows.Count > 0)
                {
                    Excp = 0;

                    return true;
                }
                else
                {
                    Excp = 0;

                    return false;
                }


            }
            catch
            {
                Excp = 1;
                return true;
            }
        }
        public bool Check_SubAccount_ID_Found(ref int Excp, ref operations opp, long SubAccount_ID, ref string Account_Name, long Account_Branch_Id)
        {
            G_DS.Clear();
            try
            {
                string mysql;
                mysql = "Select * from Submain_Account Where Id=  " + SubAccount_ID + " And  Branch_Id=" + Account_Branch_Id + "";

                G_DS = opp.return_dataset(mysql);
                if (G_DS.Tables[0].Rows.Count > 0)
                {
                    Excp = 0;
                    Account_Name = G_DS.Tables[0].Rows[0]["SubAccount_Name"].ToString();


                    return true;
                }
                else
                {
                    Excp = 0;

                    return false;
                }


            }
            catch
            {
                Excp = 1;
                return true;
            }
        }

        public void Add_Main_Account(ref int Excp, ref string Msg, string FirstSubCode, long Code, string Account_Name_, int Head_Main_Account, decimal Primary_Balance, long Currency_Id, int Account_Follows, string Notes, long Branch_Id, int Account_Customer_Type_Value, int Account_Nature_Type)
        {
            //long Submain_Account_Id,
            operations opp = new operations(1);
            try
            {
                string mysql = "";

                //mysql = " select * from Submain_Account where (Main_Account_Id IS NULL) ";
                //DataSet ds = new DataSet();
                //   ds= opp.return_dataset(mysql);
                //int Main_Account = ds.Tables[0].Rows.Count;
                //Main_Account++;


                mysql = " select * from Submain_Account where (Head_Main_Account= " + Head_Main_Account + ") And (Branch_Id= " + Branch_Id + ") ";
                DataTable dt = opp.return_DataTable(mysql);
                if (dt.Rows.Count > 0)
                {
                    Msg = "هذا الحساب موجود فى شجرة الحسابات هل تريد الإستمرار و إضافة الحساب";
                    Msg = "هذا الحساب موجود فى شجرة الحسابات  ";
                    Excp = 10;
                    // MessageBox.Show("هذا الحساب موجود فى شجرة الحسابات");
                    opp.Rollback();
                    return;

                    //if (!AccountCommonDataLIB.Common.MsgBoxYesNo("هذا الحساب موجود فى شجرة الحسابات هل تريد الإستمرار و إضافة الحساب", ""))
                    //{
                    //    Excp = 10;
                    //    // MessageBox.Show("هذا الحساب موجود فى شجرة الحسابات");
                    //    opp.Rollback();
                    //    return;
                    //}
                }
                int Account_Level = 1;
                int Accept_Cost_Center_Flage = 0;

                // Common.GETAutoNumber(ref opp, ref Submain_Account_Id);


                ////////////////////////رقم حساب الفرع
                long Main_Account_Id = 0;
                mysql = " select * from Submain_Account where (Head_Main_Account= 0) And (Branch_Id= " + Branch_Id + ") ";
                DataTable dt2 = opp.return_DataTable(mysql);
                if (dt2.Rows.Count > 0)
                {
                    Main_Account_Id = long.Parse(dt2.Rows[0]["SubAccount_Id"].ToString());
                }

                //mysql = " INSERT INTO Submain_Account (FirstSubCode,Code,SubAccount_Id,Main_Account_Id,lastPartIncrease,SubAccount_NO, Account_Code, SubAccount_Name, Head_Main_Account, Submain_Account_Balance, Currency_Id,Account_Follows, Notes,Branch_ID,Account_Customer_Type_Value,Account_Level,Account_Nature_Type,Accept_Cost_Center_Flage) ";
                //mysql = mysql + " VALUES ('" + FirstSubCode + "'," + Head_Main_Account + "," + Submain_Account_Id + "," + Main_Account_Id + "," + Head_Main_Account + "," + Head_Main_Account + "," + Head_Main_Account + ", '" + Account_Name_ + "', " + Head_Main_Account + "," + Primary_Balance + "," + Currency_Id + "," + Account_Follows + ",'" + global::AccountModelsLib.SqlText.Q(Notes) + "' ," + Branch_Id + "," + Account_Customer_Type_Value + "," + Account_Level + "," + Account_Nature_Type + "," + Accept_Cost_Center_Flage + ")";

                //opp.insert_me(mysql);
                Submain_Account newSubAccount = new Submain_Account
                {

                    // String Values
                    FirstSubCode = FirstSubCode,
                    SubAccount_Name = Account_Name_,
                    Notes = Notes,
                    SubAccount_NO = Head_Main_Account.ToString(),
                    Account_Code = Head_Main_Account.ToString(),

                    // Primary & Foreign Key Identifiers
                    Main_Account_Id = Main_Account_Id,
                    Branch_Id = Branch_Id,

                    Currency_Id = Currency_Id,

                    // Numeric Values
                    Code = Head_Main_Account,
                    lastPartIncrease = Head_Main_Account,
                    Head_Main_Account = Convert.ToInt32(Head_Main_Account),

                    // Account Classifications & Types
                    Account_Follows = Account_Follows,
                    Account_Customer_Type_Value = Account_Customer_Type_Value,
                    Account_Level = Account_Level,
                    Account_Nature_Type = Account_Nature_Type,
                    Accept_Cost_Center_Flage = Accept_Cost_Center_Flage,

                    // Note on Primary Key:
                    // If 'Id' is marked as an Identity/Auto-Increment column in EF, leave this line omitted.
                    // If you need to manually assign the PK value, uncomment the line below:
                    // Id = Submain_Account_Id
                };
                Submain_AccountCLS submain_AccountCLS = new Submain_AccountCLS();
                submain_AccountCLS.sb = newSubAccount;
                submain_AccountCLS.InsertSubmain_Account(ref opp);
                //Submain_Account_Id = submain_AccountCLS.sb.Id;

                Excp = 0;
                opp.Commit();

            }
            catch (Exception e)
            {
                Msg = e.Message;
                Excp = 1;
                opp.Rollback();
            }

        }
        public int Account_Follows = 4;//////////////////////
        public long Main_Account_Id = 0;
        public long SaveNewAccount(ref string Msg, int Account_Type_Value, ref operations opp, string PrefixName, string Name_Ar, string Name_En)
        {

            string Account_Code = "";
            Sub_Account acc = new Sub_Account();
            //int Account_Type_Value = 2;
            long New_Submain_Account_Id = -1;
            long  Account_Branch_Id = Common.Branch_Id;

            int Excp = 0;
            acc.SubAccount_Name_En = Name_En;
            acc.Accept_Cost_Center_Flage = Accept_Cost_Center_Flage;
            acc.Add_AccountSupplierCustomerOther(ref Excp, ref Msg, ref opp, ref New_Submain_Account_Id, ref Account_Code, Account_Type_Value, Name_Ar, Account_Branch_Id);
            return New_Submain_Account_Id;
        }

        public void Add_AccountSupplierCustomerOther(ref int Excp, ref string Msg, ref operations opp, ref long New_Submain_Account_Id, ref string Account_Code, int Account_Type_Value, string Account_Name, long Account_Branch_Id, long    Currency_Id = 1)
        {

            Excp = 0;
            bool Add_Account_Customer_List = true;

            int Account_Type_ValueGeneralCustomer = cuCls.cs?.General_Customer_Flage ?? 0;
            try
            {
                //if (New_Submain_Account_Id > 0)
                //{
                sa = new Submain_AccountCLS();
                sa = SetUpSubmain_AccountCLS(ref Excp, ref Msg, ref opp, ref New_Submain_Account_Id, ref Account_Code, Account_Type_Value, Account_Name, Account_Branch_Id, Currency_Id, Account_Type_ValueGeneralCustomer);
                //Add_Account(ref Excp, ref Msg, ref opp, ref New_Submain_Account_Id, sa);


                //Add_Account_(ref Excp, ref Msg, ref opp, ref New_Submain_Account_Id, sa.sb, Add_Account_Customer_List);
                if (sa.sb.Id <= 0)
                {
                    if (IsUse_SubAccount_Id_Select == false)
                    {
                        /////اضافة فى جدول اسماء الحسابات
                        Add_Submain_Account(ref Excp, ref Msg, ref opp, ref New_Submain_Account_Id, sa.sb, Add_Account_Customer_List);
                    }
                }
                //}
                //else
                //{
                //    sa.sb = sa.Get_Submain_Account(ref   opp, New_Submain_Account_Id);
                //}
                //اضافة عملاء موردين وخزينة وبنك وموظف
                AddAnother_Submain_Account(ref Excp, ref Msg, ref opp, ref New_Submain_Account_Id, sa.sb, Add_Account_Customer_List);
                UpdateSubmain_SubAccount_Id(ref Excp, ref opp, ref Msg);

                //Excp = 0;
            }
            catch (Exception ee)
            {
                Excp = 1;
                string detailedError = $"Main Error: {ee.Message}\n\n";

                if (ee.InnerException != null)
                {
                    detailedError += $"Detailed Cause: {ee.InnerException.Message}\n\n";

                    if (ee.InnerException.InnerException != null)
                    {
                        detailedError += $"Column Details: {ee.InnerException.InnerException.Message}";
                    }
                }

                string eeee = detailedError + "Database Mapping Error";

                throw;
            }

        }

        public void Update_AccountSupplierCustomerOther(ref int Excp, ref string Msg, ref operations opp, ref long New_Submain_Account_Id, ref string Account_Code, int Account_Type_Value, string Account_Name, long Account_Branch_Id, long    Currency_Id = 1)
        {

            Excp = 0;
            bool Add_Account_Customer_List = true;

            int Account_Type_ValueGeneralCustomer = cuCls.cs?.General_Customer_Flage ?? 0;
            try
            {
                //if (New_Submain_Account_Id > 0)
                //{
                sa = new Submain_AccountCLS();

                //Add_Account(ref Excp, ref Msg, ref opp, ref New_Submain_Account_Id, sa);


                //Add_Account_(ref Excp, ref Msg, ref opp, ref New_Submain_Account_Id, sa.sb, Add_Account_Customer_List);
                if (New_Submain_Account_Id == 0)
                {
                    if (IsUse_SubAccount_Id_Select == false)
                    {
                        sa = SetUpSubmain_AccountCLS(ref Excp, ref Msg, ref opp, ref New_Submain_Account_Id, ref Account_Code, Account_Type_Value, Account_Name, Account_Branch_Id, Currency_Id, Account_Type_ValueGeneralCustomer);
                        /////اضافة فى جدول اسماء الحسابات
                        Add_Submain_Account(ref Excp, ref Msg, ref opp, ref New_Submain_Account_Id, sa.sb, Add_Account_Customer_List);
                    }
                }
                else
                {
                    sa.Get_Submain_Account(New_Submain_Account_Id);
                    string sbname = sa.sb.SubAccount_Name;
                    if (Account_Type_Value != sa.sb.Account_Customer_Type_Value)
                    {
                        if (IsUse_SubAccount_Id_Select == false)
                        {
                            New_Submain_Account_Id = -1;
                            sa = SetUpSubmain_AccountCLS(ref Excp, ref Msg, ref opp, ref New_Submain_Account_Id, ref Account_Code, Account_Type_Value, Account_Name, Account_Branch_Id, Currency_Id, Account_Type_ValueGeneralCustomer);
                            /////اضافة فى جدول اسماء الحسابات
                            Add_Submain_Account(ref Excp, ref Msg, ref opp, ref New_Submain_Account_Id, sa.sb, Add_Account_Customer_List);
                        }
                    }
                }

                //اضافة عملاء موردين وخزينة وبنك وموظف
                UpdateAnother_Submain_Account(ref Excp, ref Msg, ref opp, ref New_Submain_Account_Id, sa.sb, Add_Account_Customer_List);


            }
            catch (Exception ee)
            {
                Excp = 1;
                //throw;
            }

        }
        public void Add_AccountSupplierCustomerOnlyAccount(ref int Excp, ref string Msg, ref operations opp, ref long New_Submain_Account_Id, ref string Account_Code, int Account_Type_Value, string Account_Name, long Account_Branch_Id, long    Currency_Id = 1)
        {

            Excp = 0;
            try
            {
                sa = new Submain_AccountCLS();
                sa = SetUpSubmain_AccountCLS(ref Excp, ref Msg, ref opp, ref New_Submain_Account_Id, ref Account_Code, Account_Type_Value, Account_Name, Account_Branch_Id);
                //Add_Account(ref Excp, ref Msg, ref opp, ref New_Submain_Account_Id, sa);

                bool Add_Account_Customer_List = true;
                //Add_Account_(ref Excp, ref Msg, ref opp, ref New_Submain_Account_Id, sa, Add_Account_Customer_List);
                if (Account_Type_Value == Common.Supplier_Account_Type || Account_Type_Value == Common.Account_Type_ForShippmentCompanyAsServicesSupplier)
                {
                    sa.sb.Account_Customer_Type_ID = spCls.sp.Id;
                }
                if (IsUse_SubAccount_Id_Select == false)
                {
                    /////اضافة فى جدول اسماء الحسابات
                    Add_Submain_Account(ref Excp, ref Msg, ref opp, ref New_Submain_Account_Id, sa.sb, Add_Account_Customer_List);
                }
                Excp = 0;
            }
            catch (Exception ee)
            {
                Excp = 1;
                throw;
            }

        }
        public void Add_AccountSupplierCustomer(ref int Excp, ref string Msg, ref operations opp, ref long New_Submain_Account_Id, ref string Account_Code, int Account_Type_Value, string Account_Name, long Account_Branch_Id, long    Currency_Id = 1)
        {

            Excp = 0;
            try
            {
                sa = new Submain_AccountCLS();
                sa = SetUpSubmain_AccountCLS(ref Excp, ref Msg, ref opp, ref New_Submain_Account_Id, ref Account_Code, Account_Type_Value, Account_Name, Account_Branch_Id, 1, cuCls.cs.General_Customer_Flage ?? 0);
                //Add_Account(ref Excp, ref Msg, ref opp, ref New_Submain_Account_Id, sa);

                bool Add_Account_Customer_List = true;

                if (IsUse_SubAccount_Id_Select == false)
                {
                    /////اضافة فى جدول اسماء الحسابات
                    //Add_Submain_Account(ref Excp, ref Msg, ref opp, ref Submain_Account_Id, sa_, Add_Account_Customer_List);
                    if (sa.sb.Account_Customer_Type_Value == Common.Customer_Account_Type)
                    {
                        sa.sb.Account_Customer_Type_ID = cuCls.cs.Id;
                    }
                    Add_Submain_Account(ref Excp, ref Msg, ref opp, ref New_Submain_Account_Id, sa.sb, Add_Account_Customer_List);
                    if (sa.sb.Account_Customer_Type_Value == Common.Customer_Account_Type)
                    {
                        //sa.Account_Customer_Type_ID = cs.cs.Id;
                    }
                }
                Excp = 0;
            }
            catch (Exception ee)
            {
                Excp = 1;
                throw;
            }

        }
        public void Add_Account(ref int Excp, ref string Msg, ref operations opp, ref long Submain_Account_Id, Models.Submain_AccountCLS sa)
        {
            bool Add_Account_Customer_List = true;
            Add_Account_(ref Excp, ref Msg, ref opp, ref Submain_Account_Id, sa.sb, Add_Account_Customer_List);
        }

        Submain_AccountCLS SetUpSubmain_AccountCLS(ref int Excp, ref string Msg, ref operations opp, ref long New_Submain_Account_Id, ref string Account_Code, int Account_Type_Value, string Account_Name, long Account_Branch_Id, long    Currency_Id = 1, int Account_Type_ValueGeneralCustomer = 0)
        {

            int Head_Main_Account = 1;//////////////////////////
            int Account_Nature_Type = 1;       /////////////////////
            long lastPartIncrease = 0;
            long Code = 0;
            Account_Code = "";

            string FirstSubCode = "01";
            int Cost_Center_Tree_ID = 0;
            /*long    Currency_Id = 1*/
            ;
            int Account_Level = 0;

            if (Main_Account_Id == 0)
            {
                Common.Get_Account_Type_Code(ref opp, Account_Type_Value, Account_Type_ValueGeneralCustomer, ref Main_Account_Id, ref Cost_Center_Tree_ID, Account_Branch_Id);
            }
            Excp = 0;
            Msg = "";
            //   ref   Excp, ref   Msg,


            //فى حالة عملاء متنوعون
            if (Account_Type_ValueGeneralCustomer == 1 & Main_Account_Id > 0)
            {
                sa.sb = new Submain_Account();
                sa.Get_Submain_Account(ref opp, Main_Account_Id);

                return sa;
            }

            Common.MakeAutomatic_AccontCode(ref Excp, ref Msg, ref opp, Main_Account_Id, ref lastPartIncrease, ref Account_Code, ref Code, ref Account_Level);


            Add_In_Account_Type_Main_Account_Id = false;

            //  string FirstSubCode, long Code, int Account_Level, long lastPartIncrease, long Main_Account_Id, string SubAccount_NO, string SubAccount_Name, int Head_Main_Account, decimal SubMain_Account_Balance, long Currency_Id, int CanDelete, int Account_Follows, string Notes, int Accept_EntryNew_Flage, int Accept_Cost_Center_Flage, long Cost_Center_Tree_ID, int Account_Nature_Type, int Account_Customer_Type_Value, int IS_SubAccount_AccountType, int IS_SubAccount_FollowingAccount, long Account_Branch_Id
            sa.sb.Id = New_Submain_Account_Id;
            sa.sb.FirstSubCode = FirstSubCode;
            sa.sb.Code = Code;
            sa.sb.SubAccount_NO = Account_Code;
            sa.sb.Account_Level = Account_Level;
            sa.sb.lastPartIncrease = lastPartIncrease;
            sa.sb.Main_Account_Id = Main_Account_Id;
            sa.sb.Account_Code = Account_Code;
            sa.sb.SubAccount_Name = Account_Name;
            sa.sb.Head_Main_Account = Head_Main_Account;

            sa.sb.Currency_Id = Currency_Id;
            sa.sb.CanDelete = 1;
            sa.sb.Account_Follows = Account_Follows;
            sa.sb.Notes = "";
            sa.sb.Accept_EntryNew_Flage = 1;
            sa.sb.Accept_Cost_Center_Flage = Accept_Cost_Center_Flage;
            sa.sb.Cost_Center_Tree_Id = Cost_Center_Tree_ID;
            sa.sb.Account_Nature_Type = Account_Nature_Type;
            sa.sb.Account_Customer_Type_Value = Account_Type_Value;
            sa.sb.IS_SubAccount_AccountType = 0;
            sa.sb.IS_SubAccount_FollowingAccount = 0;
            sa.sb.Branch_Id = Account_Branch_Id;

            return sa;
        }
        /*
        long GetMain_Account_Id(ref operations opp, int Account_Customer_Type_Value, int Head_Main_Account)
        {
            string mysql = "";
            mysql = "Select * from Submain_Account Where Main_Account_Id = 0 And Head_Main_Account =" + Head_Main_Account + "";
            DataTable dtSubmain_Account = opp.return_DataTable(mysql);
            long Main_Account_Id = 0;
            if (dtSubmain_Account.Rows.Count > 0)
            {
                Main_Account_Id = long.Parse(dtSubmain_Account.Rows[0]["SubAccount_Id"].ToString());
            }

            return Main_Account_Id;
        }
       
        public void AddSbuAccountFrom_Account_TypeAutomatic(ref operations opp, int Account_Customer_Type_Value)
        {
            string mysql = "";

            mysql = "Select * from Account_Type Where Value=" + Account_Customer_Type_Value;
            DataTable dt = opp.return_DataTable(mysql);

            if (dt.Rows.Count == 0)
            {
                return;
            }

            int Account_Follows = 4;
            int Head_Main_Account = 1;
            string Account_Type_Name = dt.Rows[0]["Name"].ToString();
            Account_Type_Name = Account_Type_Name + " (إضافة اّلية) ";
            try
            {
                Account_Follows = int.Parse(dt.Rows[0]["Account_Follows"].ToString());
            }
            catch { }
            try
            {
                Head_Main_Account = int.Parse(dt.Rows[0]["Head_Main_Account"].ToString());
            }
            catch { }

            Main_Account_Id = GetMain_Account_Id(ref opp, Account_Customer_Type_Value, Head_Main_Account);
            Submain_AccountCLS sa = new Submain_AccountCLS();
            sa.sb.Is_MasrofatAffect = 0;


            sa.sb.FirstSubCode = "01";
            sa.sb.Code = 0;
            sa.sb.Account_Level = 2;
            sa.sb.lastPartIncrease = 0;
            sa.sb.Main_Account_Id = Main_Account_Id;



            sa.sb.Main_Account_Id = Main_Account_Id;
            sa.sb.SubAccount_NO = "";
            sa.sb.SubAccount_Name = Account_Type_Name;
            sa.sb.Head_Main_Account = 1;

            sa.sb.Currency_Id = 1;
            sa.sb.CanDelete = 0;
            sa.sb.Account_Follows = Account_Follows;
            sa.sb.Notes = "";
            sa.sb.Accept_EntryNew_Flage = 0;
            sa.sb.Accept_Cost_Center_Flage = 0;
            sa.sb.Cost_Center_Tree_Id = 0;
            sa.sb.Account_Nature_Type = 1;
            sa.sb.Account_Customer_Type_Value = Account_Customer_Type_Value;

            sa.sb.IS_SubAccount_AccountType = 0;
            sa.sb.IS_SubAccount_FollowingAccount = 0;
            sa.sb.Branch_Id = Common.Branch_Id;

            sa.InsertSubmain_Account(ref opp);
        }

        */
        //string FirstSubCode, long Code, int Account_Level, long lastPartIncrease, long Main_Account_Id, string SubAccount_NO, string SubAccount_Name, int Head_Main_Account, decimal SubMain_Account_Balance, long Currency_Id, int CanDelete, int Account_Follows, string Notes, int Accept_EntryNew_Flage, int Accept_Cost_Center_Flage, long Cost_Center_Tree_ID, int Account_Nature_Type, int Account_Customer_Type_Value, int IS_SubAccount_AccountType, int IS_SubAccount_FollowingAccount, long Account_Branch_Id

        public bool Add_In_Account_Type_Main_Account_Id = true;

        void GetAccountCodeParts(string SubAccount_NO, ref int Code1, ref int Code2, ref int Code3, ref int Code4, ref int Code5, ref int Code6)
        {

            string[] CodeArray = SubAccount_NO.Split('-');
            try
            {
                Code1 = int.Parse(CodeArray[0].ToString());
            }
            catch { }
            try
            {
                Code2 = int.Parse(CodeArray[1].ToString());
            }
            catch { }
            try
            {
                Code3 = int.Parse(CodeArray[2].ToString());
            }
            catch { }
            try
            {
                Code4 = int.Parse(CodeArray[3].ToString());
            }
            catch { }
            try
            {
                Code5 = int.Parse(CodeArray[4].ToString());
            }
            catch { }

            try
            {
                Code6 = int.Parse(CodeArray[5].ToString());
            }
            catch { }
        }

        //public Submain_AccountCLS sa = new Submain_AccountCLS();
        void Add_Account_(ref int Excp, ref string Msg, ref operations opp, ref long Submain_Account_Id, Submain_Account sa_, bool Add_Account_Customer_List)
        {
            if (IsUse_SubAccount_Id_Select == false)
            {
                /////اضافة فى جدول اسماء الحسابات
                Add_Submain_Account(ref Excp, ref Msg, ref opp, ref Submain_Account_Id, sa_, Add_Account_Customer_List);
            }
            //اضافة عملاء موردين وخزينة وبنك وموظف
            AddAnother_Submain_Account(ref Excp, ref Msg, ref opp, ref Submain_Account_Id, sa_, Add_Account_Customer_List);
        }


        void Add_Submain_Account(ref int Excp, ref string Msg, ref operations opp, ref long Submain_Account_Id, Submain_Account sa_, bool Add_Account_Customer_List)
        {
            ////////////add new my account
            sa.sb = sa_;
            string mysql = "";

            sa.InsertSubmain_Account(ref opp);
            Submain_Account_Id = sa_.Id;

        }
        void AddAnother_Submain_Account(ref int Excp, ref string Msg, ref operations opp, ref long Submain_Account_Id, Submain_Account sa_, bool Add_Account_Customer_List)
        {
            //////////////اضافة رقم الحساب الرئيسى ونوع فى  جدول انواع حسابات الفروع
            //Account_Customer_Type_Value
            string mysql = "";
            if (Add_Account_Customer_List) /////تضاف فى حاله الاضافة من شجرة الحسابات
            {
                if (sa_.Account_Customer_Type_Value == Common.Customer_Account_Type) ///حساب عميل
                {
                    New_Account_Id = cuCls.InsertCustomer(ref Excp, ref Msg, ref opp, sa_);
                }
                else if (sa_.Account_Customer_Type_Value == Common.Supplier_Account_Type || sa_.Account_Customer_Type_Value == Common.Account_Type_ForShippmentCompanyAsServicesSupplier) ///حساب مورد
                {
                    New_Account_Id = spCls.InsertSupplier(ref Excp, ref Msg, ref opp, sa_);
                }
                else if (sa_.Account_Customer_Type_Value == Common.Sandouk_Account_Type) ///الصندوق
                {
                    BanksClss bcls = new BanksClss();

                    ba.Is_Bank = 0;
                    New_Account_Id = bcls.InsertBank(ref Excp, ref Msg, ref opp, ba, sa_);
                }
                else if (sa_.Account_Customer_Type_Value == Common.Bank_Account_Type) ///البنك
                {
                    BanksClss bcls = new BanksClss();

                    ba.Is_Bank = 1;

                    New_Account_Id = bcls.InsertBank(ref Excp, ref Msg, ref opp, ba, sa_);

                }
                else if (sa_.Account_Customer_Type_Value == Common.Employee_Account_Type) ///موظف
                {
                    EmployeeListCLS employeeListCLS = new EmployeeListCLS();
                    New_Account_Id = employeeListCLS.AddUpdteEmployee(ref Excp, ref Msg, ref opp, sa_);

                }
                else if (sa_.Account_Customer_Type_Value == Common.Assoul_Sabta_Account_Type) ///اصول ثابته
                {
                    New_Account_Id = fixedAssetCLS.InsertFixedAsset(ref Excp, ref Msg, ref opp, sa_);
                }
                else if (sa_.Account_Customer_Type_Value == Common.Account_Type_ForStoreEntagTam) ////مخزن انتاج تام
                {
                    StoreCLS storeCLS = new StoreCLS();
                    storeCLS.store = store;
                    New_Account_Id = storeCLS.AddNewServiceStore(ref Excp, ref Msg, ref opp, sa_);
                }

                else if (sa_.Account_Customer_Type_Value == Common.Account_Type_ForStoreKhamat)  // حساب مخزن خامات
                {
                    StoreCLS storeCLS = new StoreCLS();
                    storeCLS.store = store;
                    New_Account_Id = storeCLS.AddNewServiceStore(ref Excp, ref Msg, ref opp, sa_);
                }
                else if (sa_.Account_Customer_Type_Value == Common.Account_Type_ForStoreNotComplete || sa_.Account_Customer_Type_Value == Common.Account_Type_ForWIP) //حساب مخزن إنتاج غير مكتمل
                {
                    StoreCLS storeCLS = new StoreCLS();
                    storeCLS.store = store;
                    New_Account_Id = storeCLS.AddNewServiceStore(ref Excp, ref Msg, ref opp, sa_);
                }
                else if (sa_.Account_Customer_Type_Value == Common.Account_Type_ForShippmentCompanyAsServicesSupplier) //حساب شركات الشحن موردى خدمات
                {
                    ShipmentCompanyEFCLS companyCls = new  ShipmentCompanyEFCLS ();
                    //storeCLS.store = store;
                    New_Account_Id = companyCls.AddNewCompany(ref Excp, ref Msg, ref opp, shipmentCompany, sa_);
                }
                

            }


        }
        void UpdateAnother_Submain_Account(ref int Excp, ref string Msg, ref operations opp, ref long Submain_Account_Id, Submain_Account sa_, bool Add_Account_Customer_List)
        {
            //////////////اضافة رقم الحساب الرئيسى ونوع فى  جدول انواع حسابات الفروع
            //Account_Customer_Type_Value
            string SubAccount_Name = "";
            long SubAccount_Id = 0;
            long Account_Customer_Type_ID = 0;
            string mysql = "";
            if (Add_Account_Customer_List) /////تضاف فى حاله الاضافة من شجرة الحسابات
            {
                if (sa_.Account_Customer_Type_Value == Common.Customer_Account_Type) ///حساب عميل
                {
                    cuCls.cs.SubAccount_Id = sa_.Id;
                    New_Account_Id = cuCls.UpdateCustomerOnly(ref Excp, ref Msg, ref opp);
                    SubAccount_Name = cuCls.cs.Name;
                    SubAccount_Id = sa_.Id;
                    Account_Customer_Type_ID = cuCls.cs.Id;

                }
                else if (sa_.Account_Customer_Type_Value == Common.Supplier_Account_Type || sa_.Account_Customer_Type_Value == Common.Account_Type_ForShippmentCompanyAsServicesSupplier) ///حساب مورد
                {
                    spCls.sp.SubAccount_Id = sa_.Id;
                    New_Account_Id = spCls.UpdateSupplierOnly(ref Excp, ref Msg, ref opp);
                    Account_Customer_Type_ID = spCls.sp.Id;
                    SubAccount_Name = spCls.sp.Name;
                    SubAccount_Id = sa_.Id;


                }
                else if (sa_.Account_Customer_Type_Value == Common.Sandouk_Account_Type) ///الصندوق
                {
                    ba.SubAccount_Id = sa_.Id;
                    BanksClss bcls = new BanksClss();

                    ba.Is_Bank = 0;
                    New_Account_Id = bcls.UpdateBank(ref opp, ba);
                    Account_Customer_Type_ID = ba.Id;
                    SubAccount_Name = ba.Branch_Name;
                    SubAccount_Id = sa_.Id;

                }
                else if (sa_.Account_Customer_Type_Value == Common.Bank_Account_Type) ///البنك
                {
                    ba.SubAccount_Id = sa_.Id;
                    BanksClss bcls = new BanksClss();

                    ba.Is_Bank = 1;

                    New_Account_Id = bcls.UpdateBank(ref opp, ba);
                    Account_Customer_Type_ID = ba.Id;
                    SubAccount_Name = ba.Branch_Name;
                    SubAccount_Id = sa_.Id;
                }
                else if (sa_.Account_Customer_Type_Value == Common.Employee_Account_Type) ///موظف
                {

                    EmployeeListCLS employeeListCLS = new EmployeeListCLS();
                    employeeListCLS.emp.SubAccount_Id = sa_.Id;
                    New_Account_Id = employeeListCLS.UpdateEmployee(ref Excp, ref Msg, ref opp, sa_);
                    Account_Customer_Type_ID = employeeListCLS.emp.Id;
                    SubAccount_Name = employeeListCLS.emp.name;
                    SubAccount_Id = sa_.Id;
                }
                else if (sa_.Account_Customer_Type_Value == Common.Assoul_Sabta_Account_Type) ///اصول ثابته
                {
                    fixedAssetCLS.fxd.SubAccount_Id = sa_.Id;
                    New_Account_Id = fixedAssetCLS.UpdateFixedAsset(ref opp);
                    Account_Customer_Type_ID = fixedAssetCLS.fxd.Id;
                    SubAccount_Name = fixedAssetCLS.fxd.Name_Ar;
                    SubAccount_Id = sa_.Id;

                }
                else if (sa_.Account_Customer_Type_Value == Common.Account_Type_ForStoreEntagTam) ////مخزن انتاج تام
                {
                    StoreCLS storeCLS = new StoreCLS();
                    storeCLS.store = store;
                    storeCLS.store.SubAccount_Id = sa_.Id;
                    store.SubAccount_Id = sa_.Id;
                    New_Account_Id = storeCLS.UpdateStore(ref Excp, ref Msg, ref opp, storeCLS.store);
                    Account_Customer_Type_ID = storeCLS.store.Id;
                    SubAccount_Name = storeCLS.store.Store_Name;
                    SubAccount_Id = sa_.Id;

                }

                else if (sa_.Account_Customer_Type_Value == Common.Account_Type_ForStoreKhamat)  // حساب مخزن خامات
                {
                    StoreCLS storeCLS = new StoreCLS();
                    storeCLS.store = store;
                    storeCLS.store.SubAccount_Id = sa_.Id;
                    store.SubAccount_Id = sa_.Id;
                    New_Account_Id = storeCLS.UpdateStore(ref Excp, ref Msg, ref opp, storeCLS.store);
                    Account_Customer_Type_ID = storeCLS.store.Id;
                    SubAccount_Name = storeCLS.store.Store_Name;
                    SubAccount_Id = sa_.Id;
                }
                else if (sa_.Account_Customer_Type_Value == Common.Account_Type_ForStoreNotComplete || sa_.Account_Customer_Type_Value == Common.Account_Type_ForWIP) //حساب مخزن إنتاج غير مكتمل
                {
                    StoreCLS storeCLS = new StoreCLS();
                    storeCLS.store = store;
                    storeCLS.store.SubAccount_Id = sa_.Id;
                    store.SubAccount_Id = sa_.Id;
                    New_Account_Id = storeCLS.UpdateStore(ref Excp, ref Msg, ref opp, storeCLS.store);
                    Account_Customer_Type_ID = storeCLS.store.Id;
                    SubAccount_Name = storeCLS.store.Store_Name;
                    SubAccount_Id = sa_.Id;
                }
            }




            mysql = " Update Submain_Account Set SubAccount_Name ='" + SubAccount_Name + "' , Account_Customer_Type_ID=" + Account_Customer_Type_ID + "  Where  Id=" + SubAccount_Id + "  ";
            int C = opp.insert_me(mysql);

        }
        public void Add_Account_FromAccountTree(ref int Excp, ref string Msg, ref operations opp, ref long Submain_Account_Id, Submain_AccountCLS sa, bool Add_Account_Customer_List)
        {
             Add_Account_(ref Excp, ref Msg, ref opp, ref Submain_Account_Id, sa.sb, Add_Account_Customer_List);

        }

        public void UpdateSubmain_SubAccount_Id(ref int Excp, ref string Msg)
        {
            operations opp = new operations();
            try
            {

                UpdateSubmain_SubAccount_Id(ref Excp, ref opp, ref Msg);
                Excp = 0;
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;

            }

        }
        public void UpdateSubmain_SubAccount_Id(ref int Excp, ref operations opp, ref string Msg)
        {
            try
            {

                string mysql = " Update Submain_Account Set Account_Customer_Type_ID =" + New_Account_Id + " Where  Id=" + sa.sb.Id;
                opp.insert_me(mysql);
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
            }


        }
        public void UpdateSubmain_SubAccount_Name(ref int Excp, ref string Msg, ref operations opp, string SubAccount_Name, long Account_Customer_Type_ID, int Account_Customer_Type_Value, ref long SubAccount_Id)
        {
            if (SubAccount_Id == 0)
            {

            }
            int C = sa.UpdateSubmain_Account(ref opp, SubAccount_Name, SubAccount_Id, Account_Customer_Type_ID, Account_Customer_Type_Value);
            if (C == 0)
            {
                return;
                //Excp = 0;
                //Msg = "";
                //string Account_Code = "";
                //Sub_Account acc = new Sub_Account();
                //int Account_Type_Value = Account_Customer_Type_Value;
                //long    Currency_Id = 1;
                //long  Account_Branch_Id = Common.Branch_Id;
                //SubAccount_Id = -1;


                //Submain_AccountCLS sa_ = new Submain_AccountCLS();
                //sa = SetUpSubmain_AccountCLS(ref Excp, ref Msg, ref opp, ref SubAccount_Id, ref Account_Code, Account_Type_Value, SubAccount_Name, Account_Branch_Id);
                //sa.sb.Account_Customer_Type_ID = Account_Customer_Type_ID;
                //sa.sb.Account_Customer_Type_Value = Account_Customer_Type_Value;
                ///////اضافة فى جدول اسماء الحسابات
                Add_Submain_Account(ref Excp, ref Msg, ref opp, ref SubAccount_Id, sa.sb, false);

            }

        }

        public void Edit_Account(ref int Excp, ref string Msg, ref operations opp, Submain_AccountCLS sa)
        {
            string mysql = "";
            //operations opp = new operations(1);
            try
            {


                long SubAccount_Id = sa.sb.Id;
                string SubAccount_Name = sa.sb.SubAccount_Name;
                int Account_Customer_Type_Value = sa.sb.Account_Customer_Type_Value;
                long    Currency_Id = sa.sb.Currency_Id??1;
                string SubAccount_NO = sa.sb.SubAccount_NO;
                if (sa.sb.Account_Customer_Type_Value == Common.Customer_Account_Type) ///2حساب عميل
                {

                    //cs.Id = sa.Account_Customer_Type_ID;
                    //cs.Name = sa.SubAccount_Name;
                    //cs.Balance = 0;
                    //cs.Balance_Actual = 0;
                    //cs.Currency_Id = sa.Currency_Id;
                    //cs.Account_Code = sa.SubAccount_NO;
                    //cs.SubAccount_Id = sa.SubAccount_Id;
                    //cs.Customer_Type = Common.Customer_Type;
                    //cs.Id = sa.SubAccount_Id;
                    cuCls.UpdateCustomer(ref Excp, ref Msg, ref opp, sa.sb);

                    sa.sb.Account_Customer_Type_ID = cuCls.cs.Id;



                }
                if (Account_Customer_Type_Value == Common.Supplier_Account_Type || Account_Customer_Type_Value == Common.Account_Type_ForShippmentCompanyAsServicesSupplier) ///5حساب مورد
                {
                    spCls.UpdateSupplier(ref Excp, ref Msg, ref opp, sa.sb);
                    sa.sb.Account_Customer_Type_ID = cuCls.cs.Id;

                }
                if (Account_Customer_Type_Value == Common.Sandouk_Account_Type) ///6الصندوق
                {
                    BanksClss bankCLS = new BanksClss();
                    ba.Is_Bank = 0;
                    bankCLS.UpdateBank(ref Excp, ref opp, sa.sb);
                    sa.sb.Account_Customer_Type_ID = ba.Id;



                }
                if (Account_Customer_Type_Value == Common.Bank_Account_Type) ///7البنك
                {
                    BanksClss bankCLS = new BanksClss();
                    ba.Is_Bank = 1;
                    bankCLS.UpdateBank(ref Excp, ref opp, sa.sb);
                    sa.sb.Account_Customer_Type_ID = ba.Id;

                }
                if (Account_Customer_Type_Value == Common.Employee_Account_Type) //4/موظف
                {
                    EmployeeListCLS employeeListCLS = new EmployeeListCLS(true);

                    employeeListCLS.AddUpdteEmployee(ref opp, SubAccount_Id, SubAccount_NO, SubAccount_Name);


                }
                if (sa.sb.Account_Customer_Type_Value == Common.Assoul_Sabta_Account_Type) ///اصول ثابته
                {
                    fixedAssetCLS.UpdateFixedAsset(ref Excp, ref opp, sa.sb);
                    sa.sb.Account_Customer_Type_ID = ba.Id;

                }

                sa.UpdateSubmain_Account(ref opp);
                Excp = 0;
                opp.Commit();

            }
            catch (Exception e)
            {
                Excp = 1;
                opp.Rollback();
                Msg = e.Message;
            }

        }

        public void Edit_AccountsPosition(ref int Excp, long SubAccount_Id, long NewMain_Account_Id)
        {
            Submain_AccountCLS submain_AccountCLS = new Submain_AccountCLS();
            submain_AccountCLS.Edit_AccountsPosition(ref Excp, SubAccount_Id, NewMain_Account_Id);


        }

        // 🟢 يقابل زر "موضع جديد" فى الويندوز فورمز (تغيير أب الحساب من داخل نافذة شجرة منبثقة) - لكن هذه
        // دالة جديدة، وليست استدعاءً لِـ Submain_AccountCLS.Edit_AccountsPosition أعلاه: تلك الدالة معطوبة فعلاً
        // فى شفرة المصدر نفسها (تستخدم اسم عمود "SubAccount_Id" غير الموجود فى الجدول - المفتاح الفعلي هو
        // "Id" - وتستخدم قيمة sb.Id الثابتة على الكلاس بدل معامل SubAccount_Id المُمرَّر، وتبتلع أي استثناء
        // بصمت بلا رسالة خطأ) فاستدعاؤها كما هى كان سيُسلّم شاشة لا تعمل فعلياً. الدالة الجديدة هنا تنفّذ نفس
        // النيّة المقصودة (تغيير Main_Account_Id) بشكل صحيح، وتضيف ما كان ناقصاً بالضرورة لصحة البيانات:
        // 1) منع جعل الحساب أباً لنفسه أو نقله داخل أحد فروعه (دورة فى الشجرة). 2) تحديث Head_Main_Account
        // ليطابق تصنيف الأب الجديد (أصول/خصوم/حقوق ملكية/مصروفات/إيرادات) - وإلا بقي الحساب مصنَّفاً تحت فئته
        // القديمة فى القوائم المالية رغم انتقاله شجرياً، وهذا فعلياً نفس نوع الخطر الذى رصدناه فى تدقيق اكتمال
        // بيانات الحسابات. 3) إعادة تكويد الحساب نفسه تحت أبيه الجديد عبر نفس Common.MakeAutomatic_AccontCode
        // المستخدَم بالضبط لإضافة حساب جديد (AddAccountDialog.razor)، ثم إعادة تكويد كل شجرته الفرعية بالكامل
        // عبر Submain_AccountCLS.MakeSubNewCode_ALLCHILDE الموجودة أصلاً (نفس زر "إعادة ترقيم الكل" فى الشاشة) -
        // وإلا بقيت أكواد أبنائه وأحفاده تحمل بادئة الكود القديم وهى الآن منتقلة تحت أب مختلف تماماً.
        public void Move_Account(ref int Excp, ref string Msg, long SubAccount_Id, long NewMain_Account_Id)
        {
            Excp = 0;
            Msg = "";
            try
            {
                if (SubAccount_Id == NewMain_Account_Id)
                {
                    Excp = 1;
                    Msg = "لا يمكن أن يكون الحساب أباً لنفسه";
                    return;
                }

                operations opp = new operations();

                DataTable allDt = opp.return_DataTable("SELECT Id, Main_Account_Id FROM Submain_Account");
                var parentOf = new Dictionary<long, long>();
                foreach (DataRow r in allDt.Rows)
                {
                    parentOf[Convert.ToInt64(r["Id"])] = Convert.ToInt64(r["Main_Account_Id"]);
                }

                if (!parentOf.ContainsKey(NewMain_Account_Id))
                {
                    Excp = 1;
                    Msg = "الحساب الأب الجديد غير موجود";
                    return;
                }

                long? walk = NewMain_Account_Id;
                while (walk.HasValue && walk.Value != 0)
                {
                    if (walk.Value == SubAccount_Id)
                    {
                        Excp = 1;
                        Msg = "لا يمكن نقل الحساب إلى أحد فروعه";
                        return;
                    }
                    walk = parentOf.TryGetValue(walk.Value, out var p) ? p : (long?)null;
                }

                DataTable newParentDt = opp.return_DataTable("SELECT Head_Main_Account FROM Submain_Account WHERE Id = " + NewMain_Account_Id);
                int newHead = newParentDt.Rows.Count > 0 ? Convert.ToInt32(newParentDt.Rows[0]["Head_Main_Account"]) : 0;

                long lastPartIncrease = 0;
                string Account_Code = "";
                long Code = 0;
                int Account_Level = 0;
                string codeMsg = "";
                Common.MakeAutomatic_AccontCode(ref Excp, ref codeMsg, NewMain_Account_Id, ref lastPartIncrease, ref Account_Code, ref Code, ref Account_Level);

                string mysql = "UPDATE Submain_Account SET Main_Account_Id = " + NewMain_Account_Id +
                    (newHead > 0 ? ", Head_Main_Account = " + newHead : "") +
                    ", Code = " + Code +
                    ", Account_Code = N'" + Account_Code + "'" +
                    ", SubAccount_NO = N'" + Account_Code + "'" +
                    ", lastPartIncrease = " + lastPartIncrease +
                    ", Account_Level = " + Account_Level +
                    " WHERE Id = " + SubAccount_Id;
                opp.insert_me(mysql);

                new Submain_AccountCLS().MakeSubNewCode_ALLCHILDE(ref Excp, SubAccount_Id);

                Excp = 0;
            }
            catch (Exception e)
            {
                Excp = 1;
                Msg = e.Message;
            }
        }

        public void Edit_AccountsName(ref int Excp, ref operations opp, long SubAccount_Id, string SubAccount_Name)
        {
            Submain_AccountCLS submain_AccountCLS = new Submain_AccountCLS();
            submain_AccountCLS.Edit_AccountsName(ref Excp, ref opp, SubAccount_Id, SubAccount_Name);



        }
        public void MakeSubNewCode_ALLCHILDE(ref int Excp, long main_account_Id)
        {
            Submain_AccountCLS submain_AccountCLS = new Submain_AccountCLS();
            submain_AccountCLS.MakeSubNewCode_ALLCHILDE(ref Excp, main_account_Id);


        }




        public DataSet Get_ListSub_Account(ref int Excp, long Main_Account_Id)
        {
            Submain_AccountCLS submain_AccountCLS = new Submain_AccountCLS();
            G_DS = submain_AccountCLS.Get_ListSub_Account(ref Excp, Main_Account_Id);

            return G_DS;

        }






        //public void Delete_AllSubAccount(ref int Excp, ref string Msg, Submain_Account sa)
        //{
        //    Submain_AccountCLS submain_AccountCLS = new Submain_AccountCLS();
        //    submain_AccountCLS.Delete_AllSubAccount(ref Excp, ref Msg, sa);


        //    }
        public void Delete_AllSubAccount(ref int Excp, ref string Msg, Submain_Account sa)
        {

            try
            {
                operations opp = new operations();
                Delete_AllSubAccount(ref Excp, ref Msg, ref opp, sa);
            }
            catch (Exception ee)
            {


            }
        }
        public void Delete_AllSubAccount(ref int Excp, ref string Msg, ref operations opp, Submain_Account submain_Account)
        {
            // Get_Submain_Account(ref operations opp, long SubAccount_Id)

            string mysql = "Select * from Submain_Account Where Main_Account_Id=" + submain_Account.Id;
            DataTable dt = opp.return_DataTable(mysql);

            if (dt.Rows.Count > 0)
            {
                for (int x = 0; x < dt.Rows.Count; x++)
                {
                     
                    long SubAccount_Id = long.Parse(dt.Rows[x]["Id"].ToString());
                    //Submain_AccountCLS sa1 = new Submain_AccountCLS();
                    //sa1.Get_Submain_Account(ref opp, SubAccount_Id);
                    sa.Get_Submain_Account(ref opp, SubAccount_Id);
                    //Delete_AllSubAccount(ref Excp, ref Msg, ref opp, sa.sb);
                    //Delete_Account(ref Excp, ref Msg, sa.sb.Id);

                    int Account_Customer_Type_Value = sa.sb.Account_Customer_Type_Value;
                    long Account_Customer_Type_ID = sa.sb.Account_Customer_Type_ID ?? 0;

                     
                    DeleteCustomerSupplierOther(ref Excp, ref Msg, ref opp, Account_Customer_Type_Value, Account_Customer_Type_ID, SubAccount_Id);

                }

            }
            else
            {
                mysql = "Select * from Submain_Account Where Id=" + submain_Account.Id;
                DataTable dt_ = opp.return_DataTable(mysql);

                if (dt_.Rows.Count > 0)
                {
                    long SubAccount_Id = long.Parse(dt_.Rows[0]["SubAccount_Id"].ToString());
                    string SubAccount_Name = dt_.Rows[0]["SubAccount_Name"].ToString();
                    //Submain_AccountCLS sa11 = new Submain_AccountCLS();
                    //sa11.Get_Submain_Account(ref opp, SubAccount_Id);
                    sa.Get_Submain_Account(ref opp, SubAccount_Id);

                    //Delete_Account(ref Excp, ref Msg, sa.sb.Id);
                    int Account_Customer_Type_Value = sa.sb.Account_Customer_Type_Value;
                    long Account_Customer_Type_ID = sa.sb.Account_Customer_Type_ID ?? 0;

                    
                    DeleteCustomerSupplierOther(ref Excp, ref Msg, ref opp, Account_Customer_Type_Value, Account_Customer_Type_ID, SubAccount_Id);
                }
            }
        }

        //public void Delete_Account(ref int Excp, ref string Msg, Submain_Account sa)
        //{
        //    Submain_AccountCLS submain_AccountCLS = new Submain_AccountCLS();
        //    submain_AccountCLS.Delete_Account(ref Excp, ref Msg, sa);


        //}

        /*
                void DeleteCustomerSupplierOther(ref int Excp, ref string Msg, ref operations opp, int Account_Customer_Type_Value, long Account_Customer_Type_ID, long SubAccount_Id)
                {

                    CustomerCLS cs = new CustomerCLS();
                    SupplierCLS sp = new SupplierCLS();
                    Bank ba = new Bank();
                    FixedAssetCLS fixedAssetCLS = new FixedAssetCLS();
                    BanksClss bankCLS = new BanksClss();
                    EmployeeListCLS emp = new EmployeeListCLS();
                    string mysql = "";

                    if (Account_Customer_Type_Value == Common.Customer_Account_Type) ///2حساب عميل
                    {

                        cs.MakeDeleteCustomer(ref opp, ref Excp, ref Msg, Account_Customer_Type_ID, SubAccount_Id);

                    }

                    if (Account_Customer_Type_Value == Common.Supplier_Account_Type || Account_Customer_Type_Value == Common.Account_Type_ForShippmentCompanyAsServicesSupplier) //5/حساب مورد
                    {

                        sp.MakeDeleteSupplier(ref opp, ref Excp, ref Msg, Account_Customer_Type_ID, SubAccount_Id);

                    }
                    if (Account_Customer_Type_Value == Common.Sandouk_Account_Type) //6/الصندوق
                    {

                        bankCLS.MakeDeleteBanks(ref opp, ref Excp, ref Msg, Account_Customer_Type_ID, SubAccount_Id);


                    }
                    if (Account_Customer_Type_Value == Common.Bank_Account_Type) //7/البنك
                    {

                        bankCLS.MakeDeleteBanks(ref opp, ref Excp, ref Msg, Account_Customer_Type_ID, SubAccount_Id);
                    }
                    if (Account_Customer_Type_Value == Common.Employee_Account_Type) //4/موظف
                    {

                        emp.MakeDeleteEmployee(ref opp, ref Excp, ref Msg, Account_Customer_Type_ID, SubAccount_Id);
                    }
                    if (Account_Customer_Type_Value == Common.Assoul_Sabta_Account_Type) //8/اصول ثابته
                    {

                        fixedAssetCLS.MakeDeleteFixedAssetData(ref opp, ref Excp, ref Msg, Account_Customer_Type_ID, SubAccount_Id);
                    }
                    if (Account_Customer_Type_Value == Common.Account_Type_ForStoreEntagTam) ////مخزن انتاج تام
                    {
                        StoreCLS storeCLS = new StoreCLS();
                        storeCLS.store = store;
                        //New_Account_Id = storeCLS.AddNewServiceStore(ref Excp, ref Msg, ref opp, sa_);
                    }

                    if (Account_Customer_Type_Value == Common.Account_Type_ForStoreKhamat)  // حساب مخزن خامات
                    {
                        StoreCLS storeCLS = new StoreCLS();
                        storeCLS.store = store;
                        //New_Account_Id = storeCLS.AddNewServiceStore(ref Excp, ref Msg, ref opp, sa_);
                    }
                    if (Account_Customer_Type_Value == Common.Account_Type_ForStoreNotComplete) //حساب مخزن إنتاج غير مكتمل
                    {
                        StoreCLS storeCLS = new StoreCLS();
                        storeCLS.store = store;
                        //New_Account_Id = storeCLS.AddNewServiceStore(ref Excp, ref Msg, ref opp, sa_);
                    }
                    if (Excp == 0)
                    {
                        //Submain_AccountCLS sa = new Submain_AccountCLS();
                        MakeDeleteSubmain_Account(ref opp, ref Excp, ref Msg, 0, SubAccount_Id);

                    }

                }
                */
        void DeleteCustomerSupplierOther(ref int Excp, ref string Msg, ref operations opp, int Account_Customer_Type_Value, long Account_Customer_Type_ID, long SubAccount_Id)
        {
        
            sa.MakeDeleteSubmain_Account(ref opp, ref Excp, ref Msg,   SubAccount_Id);
            if(Excp>0)
            {
                return;
            }
            string mysql = "";

            if (Account_Customer_Type_Value == Common.Customer_Account_Type) ///2حساب عميل
            {
                cuCls.MakeDeleteCustomer(ref opp, ref Excp, ref Msg, Account_Customer_Type_ID, SubAccount_Id);

            }

            if (Account_Customer_Type_Value == Common.Supplier_Account_Type || Account_Customer_Type_Value == Common.Account_Type_ForShippmentCompanyAsServicesSupplier) //5/حساب مورد
            {
                spCls.MakeDeleteSupplier(ref opp, ref Excp, ref Msg, Account_Customer_Type_ID, SubAccount_Id);

            }
            if (Account_Customer_Type_Value == Common.Sandouk_Account_Type) //6/الصندوق
            {
                BanksClss bankCLS = new BanksClss();
                bankCLS.MakeDeleteBanks(ref opp, ref Excp, ref Msg, Account_Customer_Type_ID, SubAccount_Id);


            }
            if (Account_Customer_Type_Value == Common.Bank_Account_Type) //7/البنك
            {
                BanksClss bankCLS = new BanksClss();
                bankCLS.MakeDeleteBanks(ref opp, ref Excp, ref Msg, Account_Customer_Type_ID, SubAccount_Id);
            }
            if (Account_Customer_Type_Value == Common.Employee_Account_Type) //4/موظف
            {
                EmployeeListCLS emp = new EmployeeListCLS();
                emp.MakeDeleteEmployee(ref opp, ref Excp, ref Msg, Account_Customer_Type_ID, SubAccount_Id);
            }
            if (Account_Customer_Type_Value == Common.Assoul_Sabta_Account_Type) //8/اصول ثابته
            {
                fixedAssetCLS.MakeDeleteFixedAssetData(ref opp, ref Excp, ref Msg, Account_Customer_Type_ID, SubAccount_Id);
            }
            else if (Account_Customer_Type_Value == Common.Account_Type_ForStoreEntagTam) ////مخزن انتاج تام
            {
                StoreCLS storeCLS = new StoreCLS();
                //storeCLS.store = store;

                storeCLS.MakeDeleteStore(ref opp, ref Excp, ref Msg, Account_Customer_Type_ID, SubAccount_Id);
            }

            if (Account_Customer_Type_Value == Common.Account_Type_ForStoreKhamat)  // حساب مخزن خامات
            {
                StoreCLS storeCLS = new StoreCLS();
                //storeCLS.store = store;

                storeCLS.MakeDeleteStore(ref opp, ref Excp, ref Msg, Account_Customer_Type_ID, SubAccount_Id);
            }
            if (Account_Customer_Type_Value == Common.Account_Type_ForStoreNotComplete || Account_Customer_Type_Value == Common.Account_Type_ForWIP) //حساب مخزن إنتاج غير مكتمل
            {
                StoreCLS storeCLS = new StoreCLS();
                //storeCLS.store = store;

                storeCLS.MakeDeleteStore(ref opp, ref Excp, ref Msg, Account_Customer_Type_ID, SubAccount_Id);
            }
            

        }

        public void Delete_AccountTree(ref int Excp, ref string Msg, long SubAccount_Id)
        {
        
        
        }
        public void Delete_Account(ref int Excp, ref string Msg, long SubAccount_Id)
        {
            operations opp = new operations(1);
            try
            {
                



                sa.sb = sa.GetSubmain_AccountEFAsNoTracking(ref opp, SubAccount_Id);
                int Account_Customer_Type_Value = sa.sb.Account_Customer_Type_Value;
                long Account_Customer_Type_ID = sa.sb.Account_Customer_Type_ID ?? 0;

                //Delete_Account(ref Excp, ref Msg, sa);
                DeleteCustomerSupplierOther(ref Excp, ref Msg, ref opp, Account_Customer_Type_Value, Account_Customer_Type_ID, SubAccount_Id);
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

        public void Delete_Account(ref int Excp, ref string Msg, ref operations opp, int Account_Customer_Type_Value, long Account_Customer_Type_ID, long SubAccount_Id)
        {

            DeleteCustomerSupplierOther(ref Excp, ref Msg, ref opp, Account_Customer_Type_Value, Account_Customer_Type_ID, SubAccount_Id);

        }
        public void Delete_Account(ref int Excp, ref string Msg, int Account_Customer_Type_Value, long Account_Customer_Type_ID, long SubAccount_Id)
        {
            operations opp = new operations();
            DeleteCustomerSupplierOther(ref Excp, ref Msg, ref opp, Account_Customer_Type_Value, Account_Customer_Type_ID, SubAccount_Id);
        }

        //public void Delete_Account(ref int Excp, ref string Msg, ref operations opp, long Account_Customer_Type_ID, long SubAccount_Id)
        //{
        //    int Account_Customer_Type_Value = 0;
        //    Submain_AccountCLS submain_AccountCLS = new Submain_AccountCLS();
        //    submain_AccountCLS.Delete_Account(ref Excp, ref Msg, ref opp, Account_Customer_Type_Value, Account_Customer_Type_ID, SubAccount_Id); ;



        //}



    }
}
