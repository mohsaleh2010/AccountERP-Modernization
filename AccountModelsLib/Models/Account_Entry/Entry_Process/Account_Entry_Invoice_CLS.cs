using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.OleDb;
using AccountModelsLib.Models;
using System.Collections;
 

namespace AccountModelsLib
{
    public class Account_Entry_Invoice_CLS : Accounts_Entry_POST_CLS
    {
        //public  int Reference_Type = 0;
        // public long Reference_Type_Table_Id = 0;
        //public bool IsNotDeletePrevEntry = false;
        //public long Account_Entry_List_ID = 0;
        //public long Entry_No = 0;
        public Account_Entry_Invoice_CLS()
        {
        }
        public DataTable Get_EntryForInvoices(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, long Invoices_List_Id, DateTime dtFrom, DateTime dtTo, int IS_Posted_Flage)
        {
            string mysql = "";
            //              mysql = @" SELECT 1 AS Factor,Invoices_List.Notes, 0 AS Id_Test, 0 AS CostCenter_Id, 'لا يوجد' AS Cost_Center_Name,  
            //                         Invoices_List.Currency_id, Invoices_List.ID, Invoices_List.SubAccount_Id, Invoices_List.SubAccount_Name, Invoices_List.Invoice_Id, Invoices_List.ATS05_Value, Invoices_List.TaxSales10_Value, Invoices_List.DAMGHAT, Invoices_List.Net_Total_CommissionValue, 
            //                         Invoices_List.AnotherPayment ,  Submain_Account.Branch_ID, Invoices_List.Invoice_Date , Invoices_List.Net_Total
            //, Submain_Account.Account_Customer_Type_Value, Submain_Account.Main_Account_Id, Submain_Account.Code ,  Submain_Account.Currency_Id AS Account_Currency_ID
            //,Invoices_List.Total   ,  Invoices_List.AnotherPayment

            //                         FROM Invoices_List INNER JOIN
            //                         Submain_Account ON Invoices_List.SubAccount_Id = Submain_Account.Id";

            mysql = @" SELECT        1 AS Factor, 0 AS Id_Test, 0 AS CostCenter_Id, 'لا يوجد' AS Cost_Center_Name, Submain_Account.Account_Customer_Type_Value, Submain_Account.Main_Account_Id, Submain_Account.code, 
                         Submain_Account.Currency_Id AS Account_Currency_ID, Invoices_List.*
FROM            Invoices_List INNER JOIN
                         Submain_Account ON Invoices_List.SubAccount_Id = Submain_Account.Id ";
            mysql = mysql + " WHERE (Invoices_List.Is_DeletedProcessed <> 1) AND (Invoices_List.Branch_Id = " + Common.Branch_Id + ")  ";
            //if (IS_Posted_Flage == 0 | IS_Posted_Flage == 1)
            //{
            //    mysql = mysql + "   AND  Invoices_List.IS_Posted_Flage =" + IS_Posted_Flage;
            //}
            if (Invoices_List_Id > 0)
            {
                mysql = mysql + " And Invoices_List.Id=" + Invoices_List_Id;
            }
            else
            {
                mysql = mysql + " And     (Invoices_List.Invoice_Date  >= " + AccountModelsLib.operations.Get_DateToInsert(dtFrom) + ")AND (Invoices_List.Invoice_Date  <= " + AccountModelsLib.operations.Get_DateToInsert(dtTo) + ") ";


            }


            DataTable dt_ProcessName = opp.return_DataTable(mysql);
            return dt_ProcessName;
        }



        public void Make_EntryForInvoices(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, long Invoices_List_Id, DateTime dtFrom, DateTime dtTo, int IS_Posted_Flage)
        {
            if (Common.IS_Make_AutomaticFromTar7eel_For_Invoice == 0)
            {
                return;
            }
            //////// Delete Old Entry
            AccountEntry_CLS aa = new AccountEntry_CLS();
            DataTable dt_Entry_Details = new DataTable();

            ///////Delete Old Entry
            aa.Reference_Type = Common.Invoice_Type_Id;
            aa.Reference_Type_Table_Id = Invoices_List_Id;
            AccountEntry_CLS ac = new AccountEntry_CLS();
            ac.Delete_EntryFromAutomatic(ref opp, ref Excp, ref Msg, aa.Reference_Type_Table_Id, aa.Reference_Type, IsNotDeletePrevEntry, Account_Entry_List_ID);

            try
            {

                long Entry_Currency_ID = 0;
                //long Account_Entry_List_ID = 0;
                decimal EntryValue = 0;
                double Entry_Total = 0;

                //long Entry_Currency_ID = 0;
                decimal Transaction_Value = 0;
                string Entry_Notes = "";

                long User_ID_Login = Common.User_Id;
                long Branch_ID_Login = Common.Branch_Id;
                string User_Id_Login_Name = Common.User_Name;
                int Transaction_Type = 0;

                long Branch_Id_Entry = 0;
                DateTime dtprocess_Date = new DateTime();
                DataTable dt_ProcessName = new DataTable();
                int journal_Code = 0;
                string mysql = "";
                dt_Entry_Details = new DataTable();
                aa.Get_Empty_dt_Details(ref opp, ref dt_Entry_Details);



                dt_ProcessName = Get_EntryForInvoices(ref Excp, ref Msg, ref opp, Invoices_List_Id, dtFrom, dtTo, IS_Posted_Flage);



                for (int x = 0; x < dt_ProcessName.Rows.Count; x++)
                {
                    Entry_Currency_ID = long.Parse(dt_ProcessName.Rows[0]["Currency_ID"].ToString());
                    Invoices_List_Id = long.Parse(dt_ProcessName.Rows[0]["Id"].ToString());
                    string SubAccount_Name = dt_ProcessName.Rows[x]["SubAccount_Name"].ToString();
                    string Invoices_No = dt_ProcessName.Rows[0]["Invoice_Id"].ToString();

                    CurrencyFactorsCLS_ cc = new CurrencyFactorsCLS_();
                    decimal Factor = cc.GetFactors(ref opp, Common.Entry_Currency_Id, Entry_Currency_ID);
                    Factor = Common.RoundMe(Factor);
                    string Entry_Currency_Name = cc.Get_CurrencyName(ref opp, Entry_Currency_ID);

                    /////حساب العميل

                    Transaction_Type = 1;// int.Parse(dt_ProcessName.Rows[x]["Transaction_type"].ToString());
                    Branch_Id_Entry = long.Parse(dt_ProcessName.Rows[x]["Branch_Id"].ToString());

                    dtprocess_Date = DateTime.Parse(dt_ProcessName.Rows[x]["Invoice_Date"].ToString());
                    EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["Net_Total"].ToString());
                    EntryValue = Common.RoundMe(EntryValue);
                    EntryValue = EntryValue * Factor;
                    Entry_Total = (double)EntryValue;

                    //Factor = decimal.Parse(dt_ProcessName.Rows[x]["Factor"].ToString());

                    long Account_Currency_ID = long.Parse(dt_ProcessName.Rows[x]["Currency_ID"].ToString());


                    ///Notes    ملاحظات حساب العمل او المورد
                    string Notes = dt_ProcessName.Rows[x]["Notes"].ToString();
                    string CustomerNotesAccount = Common.GetValuesAfterSetArabic(Invoices_No) + " فاتورة مبيعات رقم ";
                    if (Notes.Trim() != "")
                    {
                        CustomerNotesAccount = CustomerNotesAccount + " - " + Notes;
                    }
                    dt_ProcessName.Rows[x]["Notes"] = CustomerNotesAccount;

                    ////Notes  ملاحظات باقى حساب الفاتورة
                    Entry_Notes = " فاتورة مبيعات رقم " + Invoices_No + " لحساب " + SubAccount_Name + "  بتاريخ " + dtprocess_Date.ToString("yyyy/MM/dd");



                    try
                    {
                        aa.CarsWorkOrdersId = 0;
                        aa.CarsId = 0;
                        aa.WorkOrderNumber = 0;
                        aa.CarNumber = "";
                         //long CarsWorkOrdersId, long CarsId, long WorkOrderNumber,string CarNumber
                            long.TryParse(dt_ProcessName.Rows[x]["CarsWorkOrdersId"].ToString(),out long CarsWorkOrdersId);
                        aa.CarsWorkOrdersId = CarsWorkOrdersId;

                        long.TryParse(dt_ProcessName.Rows[x]["CarsId"].ToString(), out long CarsId);

                        aa.CarsId = CarsId;
                        long.TryParse(dt_ProcessName.Rows[x]["WorkOrderNumber"].ToString(), out long WorkOrderNumber);

                        aa.WorkOrderNumber = WorkOrderNumber;
                        aa.CarNumber = dt_ProcessName.Rows[x]["CarNumber"].ToString();

                        if (aa.CarsWorkOrdersId > 0)
                        {
                            CustomerNotesAccount = " أمر الشغل رقم " + aa.WorkOrderNumber.ToString();
                            CustomerNotesAccount = CustomerNotesAccount + " رقم السيارة " + aa.CarNumber.ToString();
                            CustomerNotesAccount = CustomerNotesAccount + " " + " فاتورة مبيعات رقم " + Common.GetValuesAfterSetArabic(Invoices_No);
                            if (Notes.Trim() != "")
                            {
                                CustomerNotesAccount = CustomerNotesAccount + " - " + Notes;
                            }
                            dt_ProcessName.Rows[x]["Notes"] = CustomerNotesAccount;





                            string TaxInternalId = "";
                            //Entry_Notes = " فاتورة مبيعات رقم " + Invoices_No + " لحساب " + SubAccount_Name + "  بتاريخ " + dtprocess_Date.ToString("yyyy/MM/dd");

                            Entry_Notes = " أمر الشغل رقم " + aa.WorkOrderNumber.ToString() + " لحساب " + SubAccount_Name + "  بتاريخ " + dtprocess_Date.ToString("yyyy/MM/dd");
                            if (TaxInternalId.Trim() != "")
                            {
                                Entry_Notes = Entry_Notes + " وتم رفعها الكترونيا برقم " + TaxInternalId.ToString();

                            }

                        }
                    }
                    catch { }

                    Add_Row_Account_Details_FromAdd(ref aa, ref dt_Entry_Details, ref dt_ProcessName, x, Transaction_Type, Account_Currency_ID, Branch_Id_Entry, EntryValue, Factor);


                    ////حساب المبيعات
                    Transaction_Type = 2;
                    int AccountType = Common.Account_Type_ForSales;
                    EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["Total"].ToString());
                    Set_Invoice_RelatedAccount(ref aa, ref dt_Entry_Details, ref opp, AccountType, Transaction_Type, Entry_Currency_ID, EntryValue, Branch_Id_Entry, Factor, Entry_Notes);



                    ////حساب أرباح تجارية وصناعية
                    Transaction_Type = 1;
                    AccountType = Common.ATS05_Value_Account_Type_ForSales;
                    EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["ATS05_Value"].ToString());
                    Set_Invoice_RelatedAccount(ref aa, ref dt_Entry_Details, ref opp, AccountType, Transaction_Type, Entry_Currency_ID, EntryValue, Branch_Id_Entry, Factor, Entry_Notes);

                    ////ضرائب المبيعات
                    Transaction_Type = 2;
                    AccountType = Common.TaxSales10_Account_Type_ForSales;
                    EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["TaxSales10_Value"].ToString());
                    //EntryValue = EntryValue * Factor;

                    Set_Invoice_RelatedAccount(ref aa, ref dt_Entry_Details, ref opp, AccountType, Transaction_Type, Entry_Currency_ID, EntryValue, Branch_Id_Entry, Factor, Entry_Notes);


                    ////الدمغة
                    Transaction_Type = 2;
                    AccountType = Common.DAMGHAT_Account_Type;
                    //EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["DAMGHAT"].ToString());
                    try
                    {
                        EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["DAMGHAT"].ToString());
                    }
                    catch (Exception ee)
                    {

                        EntryValue = 0;
                    }
                    //EntryValue = EntryValue * Factor;
                    if (EntryValue > 0)
                    {
                        Set_Invoice_RelatedAccount(ref aa, ref dt_Entry_Details, ref opp, AccountType, Transaction_Type, Entry_Currency_ID, EntryValue, Branch_Id_Entry, Factor, Entry_Notes);
                    }
                    ////الخصم المسموح ية
                    Transaction_Type = 1;
                    AccountType = Common.CommissionValue_Account_Type;
                    EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["Net_Total_CommissionValue"].ToString());
                    //EntryValue = EntryValue * Factor;
                    Set_Invoice_RelatedAccount(ref aa, ref dt_Entry_Details, ref opp, AccountType, Transaction_Type, Entry_Currency_ID, EntryValue, Branch_Id_Entry, Factor, Entry_Notes);


                    ////مصروفات اخرى على فاتورة المبيعات
                    Transaction_Type = 2;
                    AccountType = Common.Masrofat_Other_Selles_Account_Type;
                    EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["AnotherPayment"].ToString());
                    //EntryValue = EntryValue * Factor;
                    Set_Invoice_RelatedAccount(ref aa, ref dt_Entry_Details, ref opp, AccountType, Transaction_Type, Entry_Currency_ID, EntryValue, Branch_Id_Entry, Factor, Entry_Notes);



                    ///////////////انشاء راس القيد
                    long Journal_Code = 1;// Journal_CodesList.Get_Journal_Code_ForSandouk(1);
                    int IS_Make_AutomaticFromTar7eel = 1;
                    long Entry_No = AccountEntry_CLS.NewEntry_No(ref opp, dtprocess_Date, journal_Code, Account_Entry_List_ID);
                    int Transform_Type = Common.Transform_Type;

                    ///////Delete Old Entry
                    aa.Reference_Type = Common.Invoice_Type_Id;
                    aa.Reference_Type_Table_Id = Invoices_List_Id;

                    ac.Delete_EntryFromAutomatic(ref opp, ref Excp, ref Msg, aa.Reference_Type_Table_Id, aa.Reference_Type, IsNotDeletePrevEntry, Account_Entry_List_ID);

                    ////////////make New Entry
                    aa.Make_AccountEntry(ref Excp, ref Msg, ref Account_Entry_List_ID, ref opp, User_ID_Login, User_Id_Login_Name, Branch_ID_Login, Entry_No, dtprocess_Date, Journal_Code, Entry_Currency_ID, Entry_Notes, Transform_Type, dt_Entry_Details, Entry_Total, Branch_Id_Entry, IS_Make_AutomaticFromTar7eel, Invoices_No, Entry_Currency_ID, Entry_Currency_Name, Factor, Invoices_List_Id);
                    mysql = "UPDATE Invoices_List SET  IS_Posted_Flage =1,Account_Entry_List_ID=" + Account_Entry_List_ID + " Where Id = " + Invoices_List_Id;
                    opp.insert_me(mysql);
                }
            }
            catch (Exception ee)
            {

                Excp = 1;
            }

        }
        public long SubAccount_Id_RelatedTo { get; set; }
        public string SubAccount_Name_RelatedTo { get; set; }

        public long SubAccount_Id_RelatedFrom { get; set; }
        public string SubAccount_Name_RelatedFrom { get; set; }


        void Set_Invoice_RelatedAccount(ref AccountEntry_CLS aa, ref DataTable dt_Entry_Details, ref AccountModelsLib.operations opp, int AccountType, int Transaction_Type, long    Entry_Currency_ID, decimal EntryValue, long  Branch_Id_Entry, decimal Factor, string Entry_Notes)
        {

            if (EntryValue < 0 | EntryValue == 0)
            { return; }
            EntryValue = EntryValue * Factor;
            DataTable dt = new DataTable();

            dt = GetSubmain_Account(ref opp, AccountType);
            //string mysql = "  SELECT * FROM Submain_Account WHERE (Account_Customer_Type_Value = " + AccountType + ")";
            //dt = opp.return_DataTable(mysql);

            //if (dt.Rows.Count == 0)
            //{
            //    Sub_Account sb = new Sub_Account();
            //    try
            //    {
            //        sb.AddSbuAccountFrom_Account_TypeAutomatic(ref opp, AccountType);
            //    }
            //    catch (Exception ee) { }
            //    dt = opp.return_DataTable(mysql);
            //    if (dt.Rows.Count == 0)
            //    {
            //        return;
            //    }
            //}
            Set_Entry_RelatedAccount(ref aa, ref dt_Entry_Details, dt, ref opp, Transaction_Type, Entry_Currency_ID, EntryValue, Branch_Id_Entry, Factor, Entry_Notes);

        }
        public DataTable GetSubmain_Account(long SubAccount_Id)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            return GetSubmain_Account(ref opp, SubAccount_Id);


        }
        public DataTable GetSubmain_Account(ref AccountModelsLib.operations opp, long SubAccount_Id)
        {
            DataTable dt = new DataTable();
            string mysql = "  SELECT * FROM Submain_Account WHERE (SubAccount_Id = " + SubAccount_Id + ")";
            dt = opp.return_DataTable(mysql);

            //if (dt.Rows.Count == 0)
            //{
            //    Sub_Account sb = new Sub_Account();
            //    try
            //    {
            //        sb.AddSbuAccountFrom_Account_TypeAutomatic(ref opp, AccountType);
            //    }
            //    catch (Exception ee) { }
            //    dt = new DataTable();
            //    dt = opp.return_DataTable(mysql);

            //}
            return dt;

        }
        public DataTable GetSubmain_Account(ref AccountModelsLib.operations opp, int AccountType)
        {
            Submain_AccountCLS submain_AccountCLS = new Submain_AccountCLS();
            DataTable dt = submain_AccountCLS.GetSubmain_Account(ref  opp,   AccountType);
             
            return dt;

        }
        public long GetSubmain_Account_Id(int AccountType)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            return GetSubmain_Account_Id(ref opp, AccountType);
        }
        public long GetSubmain_Account_Id(ref AccountModelsLib.operations opp, int AccountType)
        {
            long SubAccount_Id = 0;
            DataTable dt = new DataTable();
            dt = GetSubmain_Account(ref opp, AccountType);

            if (dt.Rows.Count == 1)
            {
                SubAccount_Id = long.Parse(dt.Rows[0]["Id"].ToString());

            }
            return SubAccount_Id;

        }
        void Set_Invoice_RelatedAccount(ref AccountEntry_CLS aa, ref DataTable dt_Entry_Details, ref AccountModelsLib.operations opp, long SubAccount_Id, int Transaction_Type, long    Entry_Currency_ID, decimal EntryValue, long  Branch_Id_Entry, decimal Factor, string Entry_Notes)
        {

            if (EntryValue < 0 | EntryValue == 0)
            { return; }
            EntryValue = EntryValue * Factor;

            Submain_AccountCLS submain_AccountCLS = new Submain_AccountCLS();
            DataTable dt = submain_AccountCLS.GetSubmain_Account(ref opp, SubAccount_Id);



            //DataTable dt = new DataTable();
            //string mysql = "  SELECT * FROM Submain_Account WHERE (SubAccount_Id = " + SubAccount_Id + ")";
            //dt = opp.return_DataTable(mysql);

            if (dt.Rows.Count == 0)
            {
                return;
                
            }
            Set_Entry_RelatedAccount(ref aa, ref dt_Entry_Details, dt, ref opp, Transaction_Type, Entry_Currency_ID, EntryValue, Branch_Id_Entry, Factor, Entry_Notes);

        }
        void Set_Entry_RelatedAccount(ref AccountEntry_CLS aa, ref DataTable dt_Entry_Details, DataTable dtSubmain_Account, ref AccountModelsLib.operations opp, int Transaction_Type, long    Entry_Currency_ID, decimal EntryValue, long  Branch_Id_Entry, decimal Factor, string Entry_Notes)
        {
            int x = 0;

            long Id_Test = 0;
            long CostCenter_Id = 0;
            string CostCenter_Name = "لا يوجد";

            long Submain_Account_ID = long.Parse(dtSubmain_Account.Rows[x]["Id"].ToString());

            int Account_Type = int.Parse(dtSubmain_Account.Rows[x]["Account_Customer_Type_Value"].ToString());
            long MainAccount_Id = long.Parse(dtSubmain_Account.Rows[x]["Main_Account_Id"].ToString());
            string SubAccount_Name = dtSubmain_Account.Rows[x]["SubAccount_Name"].ToString();
            string MainAccount_Name = dtSubmain_Account.Rows[x]["SubAccount_Name"].ToString();
            string Account_Code = dtSubmain_Account.Rows[x]["Code"].ToString();
            long Account_Currency_ID = long.Parse(dtSubmain_Account.Rows[x]["Currency_Id"].ToString());

            string Notes_Details = Entry_Notes;// "فاتورة مبيعات";
            SubAccount_Id_RelatedTo = Submain_Account_ID;
            SubAccount_Name_RelatedTo = SubAccount_Name;

            SubAccount_Id_RelatedFrom = Submain_Account_ID;
            SubAccount_Name_RelatedFrom = SubAccount_Name;



            // Add_Row_Account_Details_FromAdd(ref aa, ref dt_Entry_Details, Transaction_Type, Entry_Currency_ID, Branch_Id_Entry, EntryValue, Submain_Account_ID, Id_Test, CostCenter_Id, CostCenter_Name, Account_Type, MainAccount_Id, SubAccount_Name, MainAccount_Name, Account_Code, Account_Currency_ID, Factor, Notes_Details);
            int Excp = 0;
            aa.AddRow_WhenBuild(ref Excp, ref dt_Entry_Details, Id_Test, Account_Code, Account_Type, MainAccount_Id, MainAccount_Name, Submain_Account_ID, SubAccount_Name, Transaction_Type, EntryValue, Notes_Details, Factor, Account_Currency_ID, Entry_Currency_ID, CostCenter_Id, CostCenter_Name, Branch_Id_Entry, Factor);

        }
        void Set_Invoice_RelatedAccountBySubAccount_Id(ref AccountEntry_CLS aa, ref DataTable dt_Entry_Details, ref AccountModelsLib.operations opp, long SubAccount_Id, int Transaction_Type, long    Entry_Currency_ID, decimal EntryValue, long  Branch_Id_Entry, decimal Factor, string Entry_Notes)
        {
            if (EntryValue < 0 | EntryValue == 0)
            { return; }
            EntryValue = EntryValue * Factor;

            string mysql = "  SELECT * FROM Submain_Account WHERE (SubAccount_Id = " + SubAccount_Id + ")";
            DataTable dt = opp.return_DataTable(mysql);

            if (dt.Rows.Count == 0)
            {
                return;
            }
            int x = 0;

            long Id_Test = 0;
            long CostCenter_Id = 0;
            string CostCenter_Name = "لا يوجد";

            long Submain_Account_ID = long.Parse(dt.Rows[x]["SubAccount_Id"].ToString());

            int Account_Type = int.Parse(dt.Rows[x]["Account_Customer_Type_Value"].ToString());
            long MainAccount_Id = long.Parse(dt.Rows[x]["Main_Account_Id"].ToString());
            string SubAccount_Name = dt.Rows[x]["SubAccount_Name"].ToString();
            string MainAccount_Name = dt.Rows[x]["SubAccount_Name"].ToString();
            string Account_Code = dt.Rows[x]["Code"].ToString();
            long Account_Currency_ID = long.Parse(dt.Rows[x]["Currency_Id"].ToString());

            string Notes_Details = Entry_Notes;// "فاتورة مبيعات";


            // Add_Row_Account_Details_FromAdd(ref aa, ref dt_Entry_Details, Transaction_Type, Entry_Currency_ID, Branch_Id_Entry, EntryValue, Submain_Account_ID, Id_Test, CostCenter_Id, CostCenter_Name, Account_Type, MainAccount_Id, SubAccount_Name, MainAccount_Name, Account_Code, Account_Currency_ID, Factor, Notes_Details);
            int Excp = 0;
            aa.AddRow_WhenBuild(ref Excp, ref dt_Entry_Details, Id_Test, Account_Code, Account_Type, MainAccount_Id, MainAccount_Name, Submain_Account_ID, SubAccount_Name, Transaction_Type, EntryValue, Notes_Details, Factor, Account_Currency_ID, Entry_Currency_ID, CostCenter_Id, CostCenter_Name, Branch_Id_Entry, Factor);

        }

        string GetSQLForCustomer(string Notes, long SubAccount_Id)
        {
            string mysql = @" SELECT    '" + Notes + "' AS Notes, ";
            mysql = mysql + @"  1 AS Factor, 0 AS Id_Test, 0 AS CostCenter_Id, 'لا يوجد' AS Cost_Center_Name, Main_Account_Id, SubAccount_NO, code, FirstSubCode, lastPartIncrease, Is_Last_Level, Head_Main_Account, 
                         CanDelete, Account_Follows, Account_Nature_Type, Accept_Cost_Center_Flage, Cost_Center_Tree_ID, Account_Level, Account_Code, Company_ID_For_GL, Company_Name_For_GL, 
                         Account_Customer_Type_Value, Account_Customer_Type_ID, Transform_Type,  Branch_ID, ConnectWith_Gl_Flage, Currency_Id AS Account_Currency_ID, Currency_Id, SubAccount_Name, 
                         SubAccount_Id FROM Submain_Account ";

            mysql = mysql + "  WHERE (Submain_Account.Id = " + SubAccount_Id + ")";
            return mysql;
        }

        public DataTable Get_EntryForInvoices_Reverse(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, long Invoices_List_Id, DateTime dtFrom, DateTime dtTo, int IS_Posted_Flage)
        {
            //                mysql = @"SELECT 1 AS Factor, CAST(Invoices_List.Invoice_Id AS nvarchar) + N' فاتورة مبيعات رقم ' AS Notes ,  0 AS Id_Test,0 AS CostCenter_Id, 'لا يوجد' AS Cost_Center_Name, Submain_Account.Main_Account_Id, Submain_Account.SubAccount_NO, Submain_Account.code, Submain_Account.FirstSubCode, 
            //                         Submain_Account.lastPartIncrease, Submain_Account.Is_Last_Level, Submain_Account.Head_Main_Account, Submain_Account.Submain_Account_Balance, Submain_Account.Currency_Id AS Expr1, 
            //                         Submain_Account.CanDelete, Submain_Account.Account_Follows, Submain_Account.destruction_date, Submain_Account.destruction, Submain_Account.Account_Nature_Type, 
            //                         Submain_Account.Accept_Cost_Center_Flage, Submain_Account.Cost_Center_Tree_ID, Submain_Account.Account_Level, Submain_Account.Account_Code, Submain_Account.Company_ID_For_GL, 
            //                         Submain_Account.Company_Name_For_GL, Submain_Account.Account_Customer_Type_Value, Submain_Account.Account_Customer_Type_ID, Submain_Account.Transform_Type, 
            //                          Submain_Account.Branch_ID AS Expr6, Submain_Account.ConnectWith_Gl_Flage, Submain_Account.Currency_Id AS Account_Currency_ID, Invoices_List.*
            //FROM            Invoices_List INNER JOIN
            //                         Submain_Account ON Invoices_List.SubAccount_Id = Submain_Account.Id ";

            string mysql = @"  SELECT 1 AS Factor, Invoices_List_Reverse.Notes, 0 AS Id_Test, 0 AS CostCenter_Id, 'لا يوجد' AS Cost_Center_Name,  
                         Invoices_List_Reverse.Currency_id, Invoices_List_Reverse.ID, Invoices_List_Reverse.SubAccount_Id, Invoices_List_Reverse.SubAccount_Name, Invoices_List_Reverse.Invoice_Id, Invoices_List_Reverse.ATS05_Value, Invoices_List_Reverse.TaxSales10_Value, Invoices_List_Reverse.DAMGHAT, Invoices_List_Reverse.Net_Total_CommissionValue, 
                         Invoices_List_Reverse.AnotherPayment ,  Submain_Account.Branch_ID, Invoices_List_Reverse.Invoice_Date , Invoices_List_Reverse.Net_Total
, Submain_Account.Account_Customer_Type_Value, Submain_Account.Main_Account_Id, Submain_Account.Code ,  Submain_Account.Currency_Id AS Account_Currency_ID
,Invoices_List_Reverse.Total ,    Invoices_List_Reverse.AnotherPayment

                         FROM Invoices_List_Reverse INNER JOIN
                         Submain_Account ON Invoices_List_Reverse.SubAccount_Id = Submain_Account.Id";
            mysql = mysql + " WHERE (Invoices_List_Reverse.Branch_Id = " + Common.Branch_Id + ")  ";
            if (IS_Posted_Flage == 0 | IS_Posted_Flage == 1)
            {
                mysql = mysql + "   AND  ISNULL(Invoices_List_Reverse.IS_Posted_Flage, 0) =" + IS_Posted_Flage;
            }
            if (Invoices_List_Id > 0)
            {
                mysql = mysql + " And Invoices_List_Reverse.Id=" + Invoices_List_Id;
            }
            else
            {
                mysql = mysql + " And     (Invoices_List_Reverse.Invoice_Date  >= " + AccountModelsLib.operations.Get_DateToInsert(dtFrom) + ")AND (Invoices_List_Reverse.Invoice_Date  <= " + AccountModelsLib.operations.Get_DateToInsert(dtTo) + ") ";


            }


            DataTable dt_ProcessName = opp.return_DataTable(mysql);

            return dt_ProcessName;

        }

        public void Make_EntryForInvoices_Reverse(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, long Invoices_List_Id, DateTime dtFrom, DateTime dtTo, int IS_Posted_Flage)
        {
            if (Common.IS_Make_AutomaticFromTar7eel_For_Invoice == 0)
            {
                return;
            }
            //////// Delete Old Entry
            AccountEntry_CLS aa = new AccountEntry_CLS();
            DataTable dt_Entry_Details = new DataTable();


            try
            {

                long Entry_Currency_ID = 0;
                //long Account_Entry_List_ID = 0;
                decimal EntryValue = 0;
                double Entry_Total = 0;

                //long Entry_Currency_ID = 0;
                decimal Transaction_Value = 0;
                string Entry_Notes = "";

                long  User_ID_Login = Common.User_Id;
                long  Branch_ID_Login = Common.Branch_Id;
                string User_Id_Login_Name = Common.User_Name;
                int Transaction_Type = 0;

                long   Branch_Id_Entry = 0;
                DateTime dtprocess_Date = new DateTime();
                DataTable dt_ProcessName = new DataTable();
                int journal_Code = 0;
                string mysql = "";
                dt_Entry_Details = new DataTable();
                aa.Get_Empty_dt_Details(ref opp, ref dt_Entry_Details);



                dt_ProcessName = Get_EntryForInvoices_Reverse(ref Excp, ref Msg, ref opp, Invoices_List_Id, dtFrom, dtTo, IS_Posted_Flage);



                for (int x = 0; x < dt_ProcessName.Rows.Count; x++)
                {
                    Entry_Currency_ID = long.Parse(dt_ProcessName.Rows[0]["Currency_ID"].ToString());
                    Invoices_List_Id = long.Parse(dt_ProcessName.Rows[0]["Id"].ToString());
                    string SubAccount_Name = dt_ProcessName.Rows[x]["SubAccount_Name"].ToString();
                    string Invoices_No = dt_ProcessName.Rows[0]["Invoice_Id"].ToString();

                    CurrencyFactorsCLS_ cc = new CurrencyFactorsCLS_();
                    decimal Factor = cc.GetFactors(ref opp, Common.Entry_Currency_Id, Entry_Currency_ID);
                    Factor = Common.RoundMe(Factor);
                    string Entry_Currency_Name = cc.Get_CurrencyName(ref opp, Entry_Currency_ID);

                    /////حساب العميل

                    Transaction_Type = 2;
                    Branch_Id_Entry = long.Parse(dt_ProcessName.Rows[x]["Branch_Id"].ToString());

                    dtprocess_Date = DateTime.Parse(dt_ProcessName.Rows[x]["Invoice_Date"].ToString());
                    EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["Net_Total"].ToString());
                    EntryValue = Common.RoundMe(EntryValue);
                    EntryValue = EntryValue * Factor;
                    Entry_Total = (double)EntryValue;

                    //Factor = decimal.Parse(dt_ProcessName.Rows[x]["Factor"].ToString());

                    long Account_Currency_ID = long.Parse(dt_ProcessName.Rows[x]["Currency_ID"].ToString());


                    ///Notes    ملاحظات حساب العمل او المورد
                    string CustomerNotesAccount = Common.GetValuesAfterSetArabic(Invoices_No) + " فاتورة مرتجع مبيعات رقم ";
                    CustomerNotesAccount = CustomerNotesAccount + " - " + dt_ProcessName.Rows[x]["Notes"].ToString();
                    dt_ProcessName.Rows[x]["Notes"] = CustomerNotesAccount;

                    ////Notes  ملاحظات باقى حساب الفاتورة
                    Entry_Notes = " فاتورة مرتجع مبيعات رقم " + Invoices_No + " لحساب " + SubAccount_Name + "  بتاريخ " + dtprocess_Date.ToString("yyyy/MM/dd");


                    Add_Row_Account_Details_FromAdd(ref aa, ref dt_Entry_Details, ref dt_ProcessName, x, Transaction_Type, Account_Currency_ID, Branch_Id_Entry, EntryValue, Factor);

                    ////حساب مرتجع المبيعات
                    Transaction_Type = 1;
                    int AccountType = Common.Account_Type_ForReverseSales;
                    EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["Total"].ToString());
                    Set_Invoice_RelatedAccount(ref aa, ref dt_Entry_Details, ref opp, AccountType, Transaction_Type, Entry_Currency_ID, EntryValue, Branch_Id_Entry, Factor, Entry_Notes);



                    ////حساب أرباح تجارية وصناعية
                    Transaction_Type = 2;
                    AccountType = Common.ATS05_Value_Account_Type_ForSales;
                    EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["ATS05_Value"].ToString());
                    Set_Invoice_RelatedAccount(ref aa, ref dt_Entry_Details, ref opp, AccountType, Transaction_Type, Entry_Currency_ID, EntryValue, Branch_Id_Entry, Factor, Entry_Notes);

                    ////ضرائب المبيعات
                    Transaction_Type = 1;
                    AccountType = Common.TaxSales10_Account_Type_ForSales;
                    EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["TaxSales10_Value"].ToString());
                    //EntryValue = EntryValue * Factor;

                    Set_Invoice_RelatedAccount(ref aa, ref dt_Entry_Details, ref opp, AccountType, Transaction_Type, Entry_Currency_ID, EntryValue, Branch_Id_Entry, Factor, Entry_Notes);


                    ////الدمغة
                    Transaction_Type = 1;
                    AccountType = Common.DAMGHAT_Account_Type;
                    EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["DAMGHAT"].ToString());
                    //EntryValue = EntryValue * Factor;
                    Set_Invoice_RelatedAccount(ref aa, ref dt_Entry_Details, ref opp, AccountType, Transaction_Type, Entry_Currency_ID, EntryValue, Branch_Id_Entry, Factor, Entry_Notes);

                    ////الخصم المسموح ية
                    Transaction_Type = 2;
                    AccountType = Common.CommissionValue_Account_Type;
                    EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["Net_Total_CommissionValue"].ToString());
                    //EntryValue = EntryValue * Factor;
                    Set_Invoice_RelatedAccount(ref aa, ref dt_Entry_Details, ref opp, AccountType, Transaction_Type, Entry_Currency_ID, EntryValue, Branch_Id_Entry, Factor, Entry_Notes);


                    ////مصروفات اخرى على فاتورة المبيعات
                    Transaction_Type = 1;
                    AccountType = Common.Masrofat_Other_Selles_Account_Type;
                    EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["AnotherPayment"].ToString());
                    //EntryValue = EntryValue * Factor;
                    Set_Invoice_RelatedAccount(ref aa, ref dt_Entry_Details, ref opp, AccountType, Transaction_Type, Entry_Currency_ID, EntryValue, Branch_Id_Entry, Factor, Entry_Notes);



                    ///////////////انشاء راس القيد
                    long Journal_Code = 1;// Journal_CodesList.Get_Journal_Code_ForSandouk(1);
                    int IS_Make_AutomaticFromTar7eel = 1;
                    long Entry_No = AccountEntry_CLS.NewEntry_No(ref opp, dtprocess_Date, journal_Code, Account_Entry_List_ID);
                    int Transform_Type = Common.Transform_Type;


                    ////////
                    /// ///////Delete Old Entry
                    aa.Reference_Type = Common.InvoiceReverse_Type_Id;
                    aa.Reference_Type_Table_Id = Invoices_List_Id;
                    AccountEntry_CLS ac = new AccountEntry_CLS();
                    ac.Delete_EntryFromAutomatic(ref opp, ref Excp, ref Msg, aa.Reference_Type_Table_Id, aa.Reference_Type, IsNotDeletePrevEntry, Account_Entry_List_ID);


                    /////////////////
                    aa.Make_AccountEntry(ref Excp, ref Msg, ref Account_Entry_List_ID, ref opp, User_ID_Login, User_Id_Login_Name, Branch_ID_Login, Entry_No, dtprocess_Date, Journal_Code, Entry_Currency_ID, Entry_Notes, Transform_Type, dt_Entry_Details, Entry_Total, Branch_Id_Entry, IS_Make_AutomaticFromTar7eel, Invoices_No, Entry_Currency_ID, Entry_Currency_Name, Factor, Invoices_List_Id);
                    mysql = "UPDATE Invoices_List_Reverse SET  IS_Posted_Flage =1,Account_Entry_List_ID=" + Account_Entry_List_ID + " Where Id = " + Invoices_List_Id;
                    opp.insert_me(mysql);
                }
            }
            catch (Exception ee)
            {

                Excp = 1;
            }

        }


        public void Make_EntryForHospitalsServices(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, long BillNo, ArrayList arr_BillDetail, long    Entry_Currency_ID, int IS_Posted_Flage)
        {
            for (int y = 0; y < arr_BillDetail.Count; y++)
            {
                System.Threading.Thread.Sleep(1000);
                try
                {
                    long BillDetail_Id = 0;
                    //long Account_Entry_List_ID = 0;
                    decimal EntryValue = 0;
                    double Entry_Total = 0;
                    string Invoice_Id = "0";
                    //long Entry_Currency_ID = 0;
                    decimal Transaction_Value = 0;
                    string Entry_Notes = "";
                    AccountEntry_CLS aa = new AccountEntry_CLS();
                    DataTable dt_Entry_Details = new DataTable();
                    long  User_ID_Login = Common.User_Id;
                    long  Branch_ID_Login = Common.Branch_Id;
                    string User_Id_Login_Name = Common.User_Name;
                    int Transaction_Type = 0;

                    long   Branch_Id_Entry = 0;
                    DateTime dtprocess_Date = new DateTime();
                    DataTable dt_ProcessName = new DataTable();
                    int journal_Code = 0;

                    dt_Entry_Details = new DataTable();
                    aa.Get_Empty_dt_Details(ref opp, ref dt_Entry_Details);
                    /////قائمة الخدمات
                    string mysql = @" SELECT        1 AS Factor, 0 AS Id_Test, 0 AS CostCenter_Id, 'لا يوجد' AS Cost_Center_Name, Submain_Account.Main_Account_Id, Submain_Account.SubAccount_NO, Submain_Account.code, Submain_Account.FirstSubCode, 
                         Submain_Account.lastPartIncrease, Submain_Account.Is_Last_Level, Submain_Account.Head_Main_Account, Submain_Account.CanDelete, Submain_Account.Account_Follows, 
                         Submain_Account.Account_Nature_Type, Submain_Account.Accept_Cost_Center_Flage, Submain_Account.Cost_Center_Tree_ID, 
                         Submain_Account.Account_Level, Submain_Account.Account_Code, Submain_Account.Company_ID_For_GL, Submain_Account.Company_Name_For_GL, Submain_Account.Account_Customer_Type_Value, 
                         Submain_Account.Account_Customer_Type_ID, Submain_Account.Transform_Type,   Submain_Account.Branch_ID, Submain_Account.ConnectWith_Gl_Flage, tblIPBillDetail.Id, 
                         tblIPBillDetail.BillID, tblIPBillDetail.Total, tblIPBill.BillNo, Submain_Account.Currency_Id AS Account_Currency_ID, Submain_Account.Currency_Id, Submain_Account.SubAccount_Name, tblIPBill.BillingDate, 
                         Submain_Account.Id, tblIPBillDetail.Notes, tblIPBillDetail.Account_IdMasrofat, tblIPBillDetail.Account_IdEradat, tblIPBillDetail.For_Account_Id
FROM            Submain_Account INNER JOIN
                         tblIPBillDetail ON Submain_Account.Id = tblIPBillDetail.Account_IdEradat INNER JOIN
                         tblIPBill ON tblIPBillDetail.BillID = tblIPBill.BillNo ";

                    mysql = mysql + " WHERE  tblIPBillDetail.Id=" + arr_BillDetail[y] + " AND  tblIPBillDetail.IS_Posted_Flage =" + IS_Posted_Flage;



                    dt_ProcessName = opp.return_DataTable(mysql);



                    for (int x = 0; x < dt_ProcessName.Rows.Count; x++)
                    {
                        ///// الخدمة على حساب من؟
                        long SubAccount_Id = long.Parse(dt_ProcessName.Rows[x]["For_Account_Id"].ToString());


                        /////حساب العميل
                        Invoice_Id = dt_ProcessName.Rows[x]["Id"].ToString();

                        Branch_Id_Entry = 1;// int.Parse(dt_ProcessName.Rows[x]["Branch_Id"].ToString());
                        Entry_Currency_ID = long.Parse(dt_ProcessName.Rows[x]["Currency_Id"].ToString());
                        dtprocess_Date = DateTime.Parse(dt_ProcessName.Rows[x]["BillingDate"].ToString());
                        EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["Total"].ToString());
                        EntryValue = Common.RoundMe(EntryValue);

                        Entry_Total = (double)EntryValue;
                        CurrencyFactorsCLS_ cc = new CurrencyFactorsCLS_();
                        decimal Factor = cc.GetFactors(ref opp, Common.Entry_Currency_Id, Entry_Currency_ID);
                        Factor = Common.RoundMe(Factor);
                        string Entry_Currency_Name = cc.Get_CurrencyName(ref opp, Entry_Currency_ID);
                        Entry_Currency_ID = long.Parse(dt_ProcessName.Rows[0]["Currency_Id"].ToString());
                        BillDetail_Id = int.Parse(dt_ProcessName.Rows[0]["Id"].ToString());
                        string SubAccount_Name = dt_ProcessName.Rows[x]["SubAccount_Name"].ToString();
                        string Invoices_No = dt_ProcessName.Rows[0]["Id"].ToString();
                        long Account_Currency_ID = long.Parse(dt_ProcessName.Rows[x]["Currency_ID"].ToString());
                        string Notes = " قيمة     " + SubAccount_Name + "  بتاريخ " + dtprocess_Date.ToString("yyyy/MM/dd");
                        Entry_Notes = Notes + " " + dt_ProcessName.Rows[x]["Notes"].ToString();

                        /////حساب العميل
                        mysql = GetSQLForCustomer(Notes, SubAccount_Id);
                        DataTable dtCustomer = opp.return_DataTable(mysql);
                        Transaction_Type = 1;
                        dtCustomer.Rows[x]["Notes"] = Notes;
                        string Customer_Account_Name = dtCustomer.Rows[x]["SubAccount_Name"].ToString();
                        Add_Row_Account_Details_FromAdd(ref aa, ref dt_Entry_Details, ref dtCustomer, x, Transaction_Type, Entry_Currency_ID, Branch_Id_Entry, EntryValue, EntryValue);


                        /////////////////طرف الخدمه فى القيد
                        Transaction_Type = 2;
                        Notes = "  الخدمة المقدمة لحساب " + Customer_Account_Name + "  بتاريخ " + dtprocess_Date.ToString("yyyy/MM/dd");
                        Notes = Notes + " " + dt_ProcessName.Rows[x]["Notes"].ToString();
                        dt_ProcessName.Rows[x]["Notes"] = Notes;
                        Add_Row_Account_Details_FromAdd(ref aa, ref dt_Entry_Details, ref dt_ProcessName, x, Transaction_Type, Account_Currency_ID, Branch_Id_Entry, EntryValue, Factor);



                        ///////////////انشاء راس القيد
                        long Journal_Code = 1;// Journal_CodesList.Get_Journal_Code_ForSandouk(1);
                        int IS_Make_AutomaticFromTar7eel = 1;
                        long Entry_No = AccountEntry_CLS.NewEntry_No(ref opp, dtprocess_Date, journal_Code, Account_Entry_List_ID);
                        int Transform_Type = Common.Transform_Type;
                        aa.Make_AccountEntry(ref Excp, ref Msg, ref Account_Entry_List_ID, ref opp, User_ID_Login, User_Id_Login_Name, Branch_ID_Login, Entry_No, dtprocess_Date, Journal_Code, Entry_Currency_ID, Entry_Notes, Transform_Type, dt_Entry_Details, Entry_Total, Branch_Id_Entry, IS_Make_AutomaticFromTar7eel, Invoice_Id, Entry_Currency_ID, Entry_Currency_Name, Factor, long.Parse(Invoice_Id));
                        mysql = "UPDATE tblIPBillDetail SET  IS_Posted_Flage =1,Account_Entry_List_ID=" + Account_Entry_List_ID + " Where Id = " + BillDetail_Id;
                        opp.insert_me(mysql);

                        //if (Excp == 0)
                        //{
                        //    opp.Commit();
                        //}
                        //else
                        //{
                        //    opp.Rollback();
                        //}

                    }
                }
                catch (Exception ee)
                {

                    Excp = 1;
                }
            }
        }

        /*
        public void Make_EntryForHospitalsServices_(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, int IS_Tar7eel_Sandouk_Automatic, long Branch_Id, DateTime dtFrom, DateTime dtTo, long    Entry_Currency_ID, string Entry_Currency_Name, decimal Factor, long Order_Money_Id, long Entry_No)
        {
            try
            {
                long Account_Entry_List_ID = 0;
                decimal EntryValue = 0;
                double Entry_Total = 0;
                decimal Equivalent_Value = 0;
                //long Entry_Currency_ID = 0;
                decimal Transaction_Value = 0;
                string Entry_Notes = "";
                AccountEntry_CLS aa = new AccountEntry_CLS();
                DataTable dt_Entry_Details = new DataTable();
                long  User_ID_Login = Common.User_Id;
                long  Branch_ID_Login = Common.Branch_Id;
                string User_Id_Login_Name = Common.User_Name;
                string mysql = @" SELECT        0 AS CostCenter_Id, 'لا يوجد' AS Cost_Center_Name, Order_Money.Id, Order_Money.Order_No, Order_Money.Order_Date, Order_Money.Transaction_type, 
                         Order_Money.Total_Value, Order_Money.Currency_Id, Order_Money.Bank_Type, Order_Money.Bank_ID, Order_Money.Man_Name, Order_Money.Is_Balances_Set, 
                         Order_Money.Day_Registration_flage, Order_Money.DayReg_List_Id, Order_Money.Order_mony_id_transform, Order_Money.Invoice_Id, Order_Money.User_Id, 
                         Order_Money.Branch_Id, Order_Money.Year_Financial_Period_Id, Order_Money.Ticket_Supplier_Id, Order_Money.Order_Tafkeet_Value, Order_Money.Order_Notes as Notes, 
                         Order_Money.Tafkeet_Value, Order_Money.Sandouk_Cost_Center_Tree_Id, Order_Money.IS_Posted_Flage, Submain_Account.Id, 
                         Submain_Account.Main_Account_Id, Submain_Account.SubAccount_NO, Submain_Account.Code, Submain_Account.FirstSubCode, Submain_Account.lastPartIncrease, 
                         Submain_Account.SubAccount_Name, Submain_Account.IS_Last_Level, Submain_Account.Head_Main_Account, 
                         Submain_Account.Currency_Id AS Expr1,   Submain_Account.CanDelete, Submain_Account.Account_Follows, 
                         Submain_Account.Account_Nature_Type, Submain_Account.Accept_Cost_Center_Flage, 
                         Submain_Account.Cost_Center_Tree_ID, Submain_Account.Account_Level, Submain_Account.Account_Code, Submain_Account.Company_ID_For_GL, 
                         Submain_Account.Company_Name_For_GL, Submain_Account.Account_Customer_Type_Value, Submain_Account.Account_Customer_Type_ID, 
                         Submain_Account.Transform_Type,   Submain_Account.Branch_ID AS Expr6, 
                         Submain_Account.ConnectWith_Gl_Flage, Banks.id AS Expr2, Banks.Bank_Name, Banks.Primary_balance, Banks.Last_Balance, Banks.Account_No, 
                         Banks.SubAccount_Id AS Expr3, Banks.Is_Bank, Banks.Currency_Id AS Expr4, Banks.firstBalanceDate, Banks.Supplier_Bank, Banks.Default_Bank, 
                         Banks.Account_Code AS Expr5, Banks.Static_Flage, Order_Money.Sandouk_Cost_Center_Tree_Id AS Cost_Center_Id, 
                         Submain_Account.Currency_Id AS Account_Currency_ID, Order_Money.Factor, Order_Money.Entry_Currency_ID
FROM            Submain_Account INNER JOIN
                         Banks ON Submain_Account.Id = Banks.SubAccount_Id INNER JOIN
                         Order_Money ON Banks.id = Order_Money.Bank_ID
";
                if (Order_Money_Id > 0)
                {
                    mysql = mysql + " WHERE      Order_Money.Id=" + Order_Money_Id;
                }
                else
                {
                    mysql = mysql + " WHERE     (Order_Money.Order_Date  >= " + AccountModelsLib.operations.Get_DateToInsert(dtFrom) + ")AND (Order_Money.Order_Date  <= " + AccountModelsLib.operations.Get_DateToInsert(dtTo) + ") ";
                    mysql = mysql + " AND  IS_Posted_Flage =0 ";

                }
                DataTable dt_Order_Money = opp.return_DataTable(mysql);
                int Transaction_Type_For_Sandouk = 0;
                int Transaction_Type_ForAccount = 0;
                long   Branch_Id_Entry = 0;
                DateTime dtprocess_Date = new DateTime();

                dt_Entry_Details = new DataTable();
                aa.Get_Empty_dt_Details(ref opp, ref dt_Entry_Details);
                int journal_Code = 0;
                for (int x = 0; x < dt_Order_Money.Rows.Count; x++)
                {
                    Order_Money_Id = long.Parse(dt_Order_Money.Rows[x]["Id"].ToString());
                    if (IS_Tar7eel_Sandouk_Automatic == 1)
                    {
                        Transaction_Type_For_Sandouk = int.Parse(dt_Order_Money.Rows[x]["Transaction_type"].ToString());
                        Branch_Id_Entry = long.Parse(dt_Order_Money.Rows[x]["Branch_Id"].ToString());
                        Entry_Currency_ID = long.Parse(dt_Order_Money.Rows[x]["Currency_ID"].ToString());
                        dtprocess_Date = DateTime.Parse(dt_Order_Money.Rows[x]["Order_Date"].ToString());
                        EntryValue = decimal.Parse(dt_Order_Money.Rows[x]["Total_Value"].ToString());
                        Entry_Total = double.Parse(dt_Order_Money.Rows[x]["Total_Value"].ToString());
                        Equivalent_Value = decimal.Parse(dt_Order_Money.Rows[x]["Factor"].ToString());
                        Entry_Notes = dt_Order_Money.Rows[x]["Notes"].ToString();
                        /////اضافه بيانات الصندوق
                        Add_Row_Account_Details_FromAdd(ref aa, ref dt_Entry_Details, ref dt_Order_Money, x, Transaction_Type_For_Sandouk, Entry_Currency_ID, Branch_Id_Entry, EntryValue, Equivalent_Value);



                        if (Transaction_Type_For_Sandouk == 1)  //////// إذن استلام نقديه
                        {
                            Transaction_Type_ForAccount = 2;
                            journal_Code = 2;
                        }
                        if (Transaction_Type_For_Sandouk == 2)  //////// إذن صرف نقديه
                        {
                            Transaction_Type_ForAccount = 1;
                            journal_Code = 3;
                        }

                        mysql = @" SELECT        Submain_Account.Id, Submain_Account.Main_Account_Id, Submain_Account.SubAccount_NO, Submain_Account.Code, Submain_Account.FirstSubCode, 
                         Submain_Account.lastPartIncrease, Submain_Account.SubAccount_Name, Submain_Account.IS_Last_Level, Submain_Account.Head_Main_Account, 
                         Submain_Account.Currency_Id, Submain_Account.CanDelete, Submain_Account.Account_Follows, 
                         Submain_Account.Account_Nature_Type, Submain_Account.Accept_Cost_Center_Flage, 
                         Submain_Account.Account_Level, Submain_Account.Account_Code, Submain_Account.Company_ID_For_GL, Submain_Account.Company_Name_For_GL, 
                         Submain_Account.Account_Customer_Type_Value, Submain_Account.Account_Customer_Type_ID, Submain_Account.Transform_Type, 
                          Submain_Account.Branch_ID, Submain_Account.ConnectWith_Gl_Flage, 
                         Submain_Account.Currency_Id AS Account_Currency_ID, Money_Transaction_Box.Id, Money_Transaction_Box.Order_Mony_Id, 
                         Money_Transaction_Box.Transaction_Date, Money_Transaction_Box.Transaction_Type, Money_Transaction_Box.Import_Value AS Expr1, 
                         Money_Transaction_Box.Export_Value AS Expr2, Money_Transaction_Box.Transaction_Value, Money_Transaction_Box.Transaction_Value_, 
                         Money_Transaction_Box.Equivalent_Value, Money_Transaction_Box.Bank_Id, Money_Transaction_Box.Bank_Balance, Money_Transaction_Box.Supplier_Id, 
                         Money_Transaction_Box.Customer_Id, Money_Transaction_Box.Account_Id, Money_Transaction_Box.SubAccount_Id AS Expr3, 
                         Money_Transaction_Box.Balance_Before, Money_Transaction_Box.Balance_After, Money_Transaction_Box.BankAccount_Id, 
                         Money_Transaction_Box.DestructionSales, Money_Transaction_Box.Order_mony_id_transform, Money_Transaction_Box.User_Id, 
                         Money_Transaction_Box.Branch_Id AS Expr4, Money_Transaction_Box.Tafkeet_Value, Money_Transaction_Box.TotalValueWillPaid, 
                         Money_Transaction_Box.Rest_Value, Money_Transaction_Box.Account_Name, Money_Transaction_Box.Account_Code AS Expr5, 
                         Money_Transaction_Box.Account_Type, Money_Transaction_Box.Currency_Id AS Expr6, Submain_Account.Cost_Center_Tree_ID AS Cost_Center_ID, 
                         Order_Money.Order_No, Money_Transaction_Box.Account_Cost_Center_Tree_Id AS CostCenter_Id, Money_Transaction_Box.Cost_Center, 
                         Cost_Center_Tree.Name AS Cost_Center_Name, Money_Transaction_Box.Notes, Money_Transaction_Box.Factor, Money_Transaction_Box.Entry_Currency_ID
FROM            Money_Transaction_Box INNER JOIN
                         Submain_Account ON Money_Transaction_Box.SubAccount_Id = Submain_Account.Id INNER JOIN
                         Order_Money ON Money_Transaction_Box.Order_Mony_Id = Order_Money.Id LEFT OUTER JOIN
                         Cost_Center_Tree ON Money_Transaction_Box.Account_Cost_Center_Tree_Id = Cost_Center_Tree.Id
                         WHERE        (Money_Transaction_Box.Order_Mony_Id = " + Order_Money_Id + ")";


                        DataTable dt_Money_Transaction_Box = opp.return_DataTable(mysql);

                        string Order_No = "";
                        for (int y = 0; y < dt_Money_Transaction_Box.Rows.Count; y++)
                        {
                            Order_No = dt_Money_Transaction_Box.Rows[y]["Order_No"].ToString();
                            EntryValue = decimal.Parse(dt_Money_Transaction_Box.Rows[y]["Transaction_Value"].ToString());
                            try
                            {
                                Equivalent_Value = decimal.Parse(dt_Money_Transaction_Box.Rows[y]["Equivalent_Value"].ToString());
                            }
                            catch { Equivalent_Value = 1; }
                            try
                            {
                                EntryValue = EntryValue * Equivalent_Value;
                                Equivalent_Value = 1 / Equivalent_Value;
                            }
                            catch { }
                            long Account_Currency_ID = long.Parse(dt_Order_Money.Rows[x]["Currency_ID"].ToString());
                            ////إضافة الحساب الاخر
                            Add_Row_Account_Details_FromAdd(ref aa, ref dt_Entry_Details, ref dt_Money_Transaction_Box, y, Transaction_Type_ForAccount, Account_Currency_ID, Branch_Id_Entry, EntryValue, Equivalent_Value);
                        }



                        /////انشاء القيد
                        if (Entry_No == 0)
                        {
                            Entry_No = AccountEntry_CLS.NewEntry_No(ref opp, dtprocess_Date, journal_Code);
                        }
                        long Journal_Code = Journal_CodesList.Get_Journal_Code_ForSandouk(Transaction_Type_For_Sandouk);
                        int IS_Make_AutomaticFromTar7eel = 1;

                        aa.Make_AccountEntry(ref Excp, ref Msg, ref Account_Entry_List_ID, ref opp, User_ID_Login, User_Id_Login_Name, Branch_ID_Login, Entry_No, dtprocess_Date, Journal_Code, Entry_Currency_ID, Entry_Notes, Transaction_Type_ForAccount, dt_Entry_Details, Entry_Total, Branch_Id_Entry, IS_Make_AutomaticFromTar7eel, Order_No, Entry_Currency_ID, Entry_Currency_Name, Factor, Order_Money_Id);
                    }
                    mysql = "UPDATE Order_Money SET  IS_Posted_Flage =1,Account_Entry_List_ID=" + Account_Entry_List_ID + " Where Id = " + Order_Money_Id;
                    opp.insert_me(mysql);
                    dt_Entry_Details.Clear();
                }
                //opp.Commit();
                Excp = 0;

            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
                //opp.Rollback();
            }

        }
        */
        public DataTable Get_EntryFor_Bills(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, long Bills_List_Id, DateTime dtFrom, DateTime dtTo, int IS_Posted_Flage)
        { //                mysql = @"SELECT 1 AS Factor, CAST(Invoices_List.Invoice_Id AS nvarchar) + N' فاتورة مبيعات رقم ' AS Notes ,  0 AS Id_Test,0 AS CostCenter_Id, 'لا يوجد' AS Cost_Center_Name, Submain_Account.Main_Account_Id, Submain_Account.SubAccount_NO, Submain_Account.code, Submain_Account.FirstSubCode, 
          //                         Submain_Account.lastPartIncrease, Submain_Account.Is_Last_Level, Submain_Account.Head_Main_Account, Submain_Account.Submain_Account_Balance, Submain_Account.Currency_Id AS Expr1, 
          //                         Submain_Account.CanDelete, Submain_Account.Account_Follows, Submain_Account.destruction_date, Submain_Account.destruction, Submain_Account.Account_Nature_Type, 
          //                         Submain_Account.Accept_Cost_Center_Flage, Submain_Account.Cost_Center_Tree_ID, Submain_Account.Account_Level, Submain_Account.Account_Code, Submain_Account.Company_ID_For_GL, 
          //                         Submain_Account.Company_Name_For_GL, Submain_Account.Account_Customer_Type_Value, Submain_Account.Account_Customer_Type_ID, Submain_Account.Transform_Type, 
          //                           Submain_Account.Branch_ID AS Expr6, Submain_Account.ConnectWith_Gl_Flage, Submain_Account.Currency_Id AS Account_Currency_ID, Invoices_List.*
          //FROM            Invoices_List INNER JOIN
          //                         Submain_Account ON Invoices_List.SubAccount_Id = Submain_Account.Id ";

            string mysql = @" SELECT 1 AS Factor,  Bills_List.Notes, 0 AS Id_Test, 0 AS CostCenter_Id, 'لا يوجد' AS Cost_Center_Name,  
                         Bills_List.Currency_id, Bills_List.ID, Bills_List.SubAccount_Id, Bills_List.SubAccount_Name, Bills_List.Invoice_Id, Bills_List.ATS05_Value, Bills_List.TaxSales10_Value, Bills_List.Damaghat, Bills_List.Net_Total_CommissionValue, 
                         Bills_List.Load_Payment ,  Submain_Account.Branch_ID, Bills_List.Invoice_Date , Bills_List.Net_Total
, Submain_Account.Account_Customer_Type_Value, Submain_Account.Main_Account_Id, Submain_Account.Code ,  Submain_Account.Currency_Id AS Account_Currency_ID
,Bills_List.Total ,Bills_List.Total_List3Mola

                         FROM Bills_List INNER JOIN
                         Submain_Account ON Bills_List.SubAccount_Id = Submain_Account.Id";
            mysql = mysql + " WHERE (Bills_List.Branch_Id = " + Common.Branch_Id + ")  ";
            if (IS_Posted_Flage == 0 | IS_Posted_Flage == 1)
            {
                mysql = mysql + "   AND  ISNULL(Bills_List.IS_Posted_Flage, 0) =" + IS_Posted_Flage;
            }
            if (Bills_List_Id > 0)
            {
                mysql = mysql + " And Bills_List.Id=" + Bills_List_Id;
            }
            else
            {
                mysql = mysql + " And     (Bills_List.Invoice_Date  >= " + AccountModelsLib.operations.Get_DateToInsert(dtFrom) + ")AND (Bills_List.Invoice_Date  <= " + AccountModelsLib.operations.Get_DateToInsert(dtTo) + ") ";


            }



            return opp.return_DataTable(mysql);
        }
        public void Make_EntryFor_Bills(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, long Bills_List_Id, DateTime dtFrom, DateTime dtTo, int IS_Posted_Flage)
        {
            if (Common.IS_Make_AutomaticFromTar7eel_For_Invoice == 0)
            {
                return;
            }
            //////// Delete Old Entry
            AccountEntry_CLS aa = new AccountEntry_CLS();
            DataTable dt_Entry_Details = new DataTable();

            aa.Reference_Type = Common.Bill_Type_Id;
            aa.Reference_Type_Table_Id = Bills_List_Id;
            AccountEntry_CLS ac = new AccountEntry_CLS();
            ac.Delete_EntryFromAutomatic(ref opp, ref Excp, ref Msg, aa.Reference_Type_Table_Id, aa.Reference_Type, IsNotDeletePrevEntry);
            try
            {

                long Entry_Currency_ID = 0;
                //long Account_Entry_List_ID = 0;
                decimal EntryValue = 0;
                double Entry_Total = 0;

                //long Entry_Currency_ID = 0;
                decimal Transaction_Value = 0;
                string Entry_Notes = "";

                long  User_ID_Login = Common.User_Id;
                long  Branch_ID_Login = Common.Branch_Id;
                string User_Id_Login_Name = Common.User_Name;
                int Transaction_Type = 0;

                long   Branch_Id_Entry = 0;
                DateTime dtprocess_Date = new DateTime();
                DataTable dt_ProcessName = new DataTable();
                int journal_Code = 0;
                string mysql = "";
                dt_Entry_Details = new DataTable();
                aa.Get_Empty_dt_Details(ref opp, ref dt_Entry_Details);


                dt_ProcessName = Get_EntryFor_Bills(ref Excp, ref Msg, ref opp, Bills_List_Id, dtFrom, dtTo, IS_Posted_Flage);


                for (int x = 0; x < dt_ProcessName.Rows.Count; x++)
                {
                    Entry_Currency_ID = long.Parse(dt_ProcessName.Rows[0]["Currency_ID"].ToString());
                    Bills_List_Id = long.Parse(dt_ProcessName.Rows[0]["Id"].ToString());
                    string SubAccount_Name = dt_ProcessName.Rows[x]["SubAccount_Name"].ToString();
                    string Invoices_No = dt_ProcessName.Rows[0]["Invoice_Id"].ToString();

                    CurrencyFactorsCLS_ cc = new CurrencyFactorsCLS_();
                    decimal Factor = cc.GetFactors(ref opp, Common.Entry_Currency_Id, Entry_Currency_ID);
                    Factor = Common.RoundMe(Factor);
                    string Entry_Currency_Name = cc.Get_CurrencyName(ref opp, Entry_Currency_ID);

                    /////حساب المورد

                    Transaction_Type = 2;// int.Parse(dt_ProcessName.Rows[x]["Transaction_type"].ToString());
                    Branch_Id_Entry = long.Parse(dt_ProcessName.Rows[x]["Branch_Id"].ToString());

                    dtprocess_Date = DateTime.Parse(dt_ProcessName.Rows[x]["Invoice_Date"].ToString());
                    EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["Net_Total"].ToString());
                    EntryValue = Common.RoundMe(EntryValue);
                    EntryValue = EntryValue * Factor;
                    Entry_Total = (double)EntryValue;

                    //Factor = decimal.Parse(dt_ProcessName.Rows[x]["Factor"].ToString());

                    long Account_Currency_ID = long.Parse(dt_ProcessName.Rows[x]["Currency_ID"].ToString());

                    ///Notes    ملاحظات حساب العمل او المورد
                    string CustomerNotesAccount = Common.GetValuesAfterSetArabic(Invoices_No) + " فاتورة مشتريات رقم ";
                    CustomerNotesAccount = CustomerNotesAccount + " - " + dt_ProcessName.Rows[x]["Notes"].ToString();
                    dt_ProcessName.Rows[x]["Notes"] = CustomerNotesAccount;

                    ////Notes  ملاحظات باقى حساب الفاتورة
                    Entry_Notes = " فاتورة مشتريات رقم " + Invoices_No + " لحساب " + SubAccount_Name + "  بتاريخ " + dtprocess_Date.ToString("yyyy/MM/dd");

                    Add_Row_Account_Details_FromAdd(ref aa, ref dt_Entry_Details, ref dt_ProcessName, x, Transaction_Type, Account_Currency_ID, Branch_Id_Entry, EntryValue, Factor);

                    ////حساب المشتريات
                    Transaction_Type = 1;
                    int AccountType = Common.Account_Type_ForPurchase;
                    EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["Total"].ToString());
                    Set_Invoice_RelatedAccount(ref aa, ref dt_Entry_Details, ref opp, AccountType, Transaction_Type, Entry_Currency_ID, EntryValue, Branch_Id_Entry, Factor, Entry_Notes);



                    ////حساب أرباح تجارية وصناعية
                    Transaction_Type = 2;
                    AccountType = Common.ATS05_Value_Account_Type_ForSales;
                    EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["ATS05_Value"].ToString());
                    Set_Invoice_RelatedAccount(ref aa, ref dt_Entry_Details, ref opp, AccountType, Transaction_Type, Entry_Currency_ID, EntryValue, Branch_Id_Entry, Factor, Entry_Notes);

                    ////ضرائب المبيعات
                    Transaction_Type = 1;
                    AccountType = Common.TaxSales10_Account_Type_ForSales;
                    EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["TaxSales10_Value"].ToString());
                    //EntryValue = EntryValue * Factor;

                    Set_Invoice_RelatedAccount(ref aa, ref dt_Entry_Details, ref opp, AccountType, Transaction_Type, Entry_Currency_ID, EntryValue, Branch_Id_Entry, Factor, Entry_Notes);

                    ////عمولة بيع المشتريات  
                    Transaction_Type = 2;
                    AccountType = Common.Total_List3Mola_Account_Type_ForSales;
                    EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["Total_List3Mola"].ToString());
                    //EntryValue = EntryValue * Factor;
                    Set_Invoice_RelatedAccount(ref aa, ref dt_Entry_Details, ref opp, AccountType, Transaction_Type, Entry_Currency_ID, EntryValue, Branch_Id_Entry, Factor, Entry_Notes);

                    
                    ////الدمغة
                    Transaction_Type = 1;
                    AccountType = Common.DAMGHAT_Account_Type;
                    EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["Damaghat"].ToString());
                    //EntryValue = EntryValue * Factor;
                    Set_Invoice_RelatedAccount(ref aa, ref dt_Entry_Details, ref opp, AccountType, Transaction_Type, Entry_Currency_ID, EntryValue, Branch_Id_Entry, Factor, Entry_Notes);

                    ////الخصم   المكتسب
                    Transaction_Type = 2;
                    AccountType = Common.CommissionMoktasab_Account_Type;
                    EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["Net_Total_CommissionValue"].ToString());
                    //EntryValue = EntryValue * Factor;
                    Set_Invoice_RelatedAccount(ref aa, ref dt_Entry_Details, ref opp, AccountType, Transaction_Type, Entry_Currency_ID, EntryValue, Branch_Id_Entry, Factor, Entry_Notes);


                    ////مصروفات التحميل على فاتورة المشتريات
                    Transaction_Type = 1;
                    AccountType = Common.Masrofat_Load_Payment_Purchases_Account_Type;
                    EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["Load_Payment"].ToString());
                    //EntryValue = EntryValue * Factor;
                    Set_Invoice_RelatedAccount(ref aa, ref dt_Entry_Details, ref opp, AccountType, Transaction_Type, Entry_Currency_ID, EntryValue, Branch_Id_Entry, Factor, Entry_Notes);



                    ///////////////انشاء راس القيد
                    long Journal_Code = 1;// Journal_CodesList.Get_Journal_Code_ForSandouk(1);
                    int IS_Make_AutomaticFromTar7eel = 1;
                    long Entry_No = AccountEntry_CLS.NewEntry_No(ref opp, dtprocess_Date, journal_Code, Account_Entry_List_ID);
                    int Transform_Type = Common.Transform_Type;

                    aa.Make_AccountEntry(ref Excp, ref Msg, ref Account_Entry_List_ID, ref opp, User_ID_Login, User_Id_Login_Name, Branch_ID_Login, Entry_No, dtprocess_Date, Journal_Code, Entry_Currency_ID, Entry_Notes, Transform_Type, dt_Entry_Details, Entry_Total, Branch_Id_Entry, IS_Make_AutomaticFromTar7eel, Invoices_No, Entry_Currency_ID, Entry_Currency_Name, Factor, Bills_List_Id);
                    mysql = "UPDATE Invoices_List SET  IS_Posted_Flage =1,Account_Entry_List_ID=" + Account_Entry_List_ID + " Where Id = " + Bills_List_Id;
                    opp.insert_me(mysql);
                }
            }
            catch (Exception ee)
            {

                Excp = 1;
            }

        }
        public DataTable Get_EntryFor_Bills_Reverse(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, long Bills_List_Id, DateTime dtFrom, DateTime dtTo, int IS_Posted_Flage)
        {
            DataTable dt_ProcessName = new DataTable();
            string mysql = @" SELECT 1 AS Factor,  Bills_List_Reverse.Notes, 0 AS Id_Test, 0 AS CostCenter_Id, 'لا يوجد' AS Cost_Center_Name,  
                         Bills_List_Reverse.Currency_id, Bills_List_Reverse.ID, Bills_List_Reverse.SubAccount_Id, Bills_List_Reverse.SubAccount_Name, Bills_List_Reverse.Invoice_Id, Bills_List_Reverse.ATS05_Value, Bills_List_Reverse.TaxSales10_Value, Bills_List_Reverse.Damaghat, Bills_List_Reverse.Net_Total_CommissionValue, 
                         Bills_List_Reverse.Load_Payment ,  Submain_Account.Branch_ID, Bills_List_Reverse.Invoice_Date , Bills_List_Reverse.Net_Total
, Submain_Account.Account_Customer_Type_Value, Submain_Account.Main_Account_Id, Submain_Account.Code ,  Submain_Account.Currency_Id AS Account_Currency_ID
,Bills_List_Reverse.Total 

                         FROM Bills_List_Reverse INNER JOIN
                         Submain_Account ON Bills_List_Reverse.SubAccount_Id = Submain_Account.Id";
            mysql = mysql + " WHERE (Bills_List_Reverse.Branch_Id = " + Common.Branch_Id + ")  ";
            if (IS_Posted_Flage == 0 | IS_Posted_Flage == 1)
            {
                mysql = mysql + "   AND  ISNULL(Bills_List_Reverse.IS_Posted_Flage, 0) =" + IS_Posted_Flage;
            }
            if (Bills_List_Id > 0)
            {
                mysql = mysql + " And Bills_List_Reverse.Id=" + Bills_List_Id;
            }
            else
            {
                mysql = mysql + " And     (Bills_List_Reverse.Invoice_Date  >= " + AccountModelsLib.operations.Get_DateToInsert(dtFrom) + ")AND (Bills_List_Reverse.Invoice_Date  <= " + AccountModelsLib.operations.Get_DateToInsert(dtTo) + ") ";


            }
            dt_ProcessName = opp.return_DataTable(mysql);
            return dt_ProcessName;
        }
        public void Make_EntryFor_Bills_Reverse(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, long Bills_List_Id, DateTime dtFrom, DateTime dtTo, int IS_Posted_Flage)
        {
            if (Common.IS_Make_AutomaticFromTar7eel_For_Invoice == 0)
            {
                return;
            }
            //////// Delete Old Entry
            AccountEntry_CLS aa = new AccountEntry_CLS();
            DataTable dt_Entry_Details = new DataTable();

            aa.Reference_Type = Common.BilleReverse_Type_Id;
            aa.Reference_Type_Table_Id = Bills_List_Id;

            aa.Delete_EntryFromAutomatic(ref opp, ref Excp, ref Msg, aa.Reference_Type_Table_Id, aa.Reference_Type, IsNotDeletePrevEntry);
            try
            {

                long Entry_Currency_ID = 0;
                //long Account_Entry_List_ID = 0;
                decimal EntryValue = 0;
                double Entry_Total = 0;

                //long Entry_Currency_ID = 0;
                decimal Transaction_Value = 0;
                string Entry_Notes = "";

                long  User_ID_Login = Common.User_Id;
                long  Branch_ID_Login = Common.Branch_Id;
                string User_Id_Login_Name = Common.User_Name;
                int Transaction_Type = 0;

                long   Branch_Id_Entry = 0;
                DateTime dtprocess_Date = new DateTime();
                DataTable dt_ProcessName = new DataTable();
                int journal_Code = 0;
                string mysql = "";
                dt_Entry_Details = new DataTable();
                aa.Get_Empty_dt_Details(ref opp, ref dt_Entry_Details);






                dt_ProcessName = Get_EntryFor_Bills_Reverse(ref Excp, ref Msg, ref opp, Bills_List_Id, dtFrom, dtTo, IS_Posted_Flage);



                for (int x = 0; x < dt_ProcessName.Rows.Count; x++)
                {
                    Entry_Currency_ID = long.Parse(dt_ProcessName.Rows[0]["Currency_ID"].ToString());
                    Bills_List_Id = long.Parse(dt_ProcessName.Rows[0]["Id"].ToString());
                    string SubAccount_Name = dt_ProcessName.Rows[x]["SubAccount_Name"].ToString();
                    string Invoices_No = dt_ProcessName.Rows[0]["Invoice_Id"].ToString();

                    CurrencyFactorsCLS_ cc = new CurrencyFactorsCLS_();
                    decimal Factor = cc.GetFactors(ref opp, Common.Entry_Currency_Id, Entry_Currency_ID);
                    Factor = Common.RoundMe(Factor);
                    string Entry_Currency_Name = cc.Get_CurrencyName(ref opp, Entry_Currency_ID);

                    /////حساب المورد

                    Transaction_Type = 1;// int.Parse(dt_ProcessName.Rows[x]["Transaction_type"].ToString());
                    Branch_Id_Entry = long.Parse(dt_ProcessName.Rows[x]["Branch_Id"].ToString());

                    dtprocess_Date = DateTime.Parse(dt_ProcessName.Rows[x]["Invoice_Date"].ToString());
                    EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["Net_Total"].ToString());
                    EntryValue = Common.RoundMe(EntryValue);
                    EntryValue = EntryValue * Factor;
                    Entry_Total = (double)EntryValue;

                    //Factor = decimal.Parse(dt_ProcessName.Rows[x]["Factor"].ToString());

                    long Account_Currency_ID = long.Parse(dt_ProcessName.Rows[x]["Currency_ID"].ToString());

                    ///Notes    ملاحظات حساب العمل او المورد
                    string CustomerNotesAccount = Common.GetValuesAfterSetArabic(Invoices_No) + " فاتورة مرتجع مشتريات رقم ";
                    CustomerNotesAccount = CustomerNotesAccount + " - " + dt_ProcessName.Rows[x]["Notes"].ToString();
                    dt_ProcessName.Rows[x]["Notes"] = CustomerNotesAccount;

                    ////Notes  ملاحظات باقى حساب الفاتورة
                    Entry_Notes = " فاتورة مرتجع مشتريات رقم " + Invoices_No + " لحساب " + SubAccount_Name + "  بتاريخ " + dtprocess_Date.ToString("yyyy/MM/dd");

                    Add_Row_Account_Details_FromAdd(ref aa, ref dt_Entry_Details, ref dt_ProcessName, x, Transaction_Type, Account_Currency_ID, Branch_Id_Entry, EntryValue, Factor);

                    ////حساب المشتريات
                    Transaction_Type = 2;
                    int AccountType = Common.Account_Type_ForPurchase;
                    EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["Total"].ToString());
                    Set_Invoice_RelatedAccount(ref aa, ref dt_Entry_Details, ref opp, AccountType, Transaction_Type, Entry_Currency_ID, EntryValue, Branch_Id_Entry, Factor, Entry_Notes);



                    ////حساب أرباح تجارية وصناعية
                    Transaction_Type = 1;
                    AccountType = Common.ATS05_Value_Account_Type_ForSales;
                    EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["ATS05_Value"].ToString());
                    Set_Invoice_RelatedAccount(ref aa, ref dt_Entry_Details, ref opp, AccountType, Transaction_Type, Entry_Currency_ID, EntryValue, Branch_Id_Entry, Factor, Entry_Notes);

                    ////ضرائب المبيعات
                    Transaction_Type = 2;
                    AccountType = Common.TaxSales10_Account_Type_ForSales;
                    EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["TaxSales10_Value"].ToString());
                    //EntryValue = EntryValue * Factor;

                    Set_Invoice_RelatedAccount(ref aa, ref dt_Entry_Details, ref opp, AccountType, Transaction_Type, Entry_Currency_ID, EntryValue, Branch_Id_Entry, Factor, Entry_Notes);


                    ////الدمغة
                    Transaction_Type = 2;
                    AccountType = Common.DAMGHAT_Account_Type;
                    EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["Damaghat"].ToString());
                    //EntryValue = EntryValue * Factor;
                    Set_Invoice_RelatedAccount(ref aa, ref dt_Entry_Details, ref opp, AccountType, Transaction_Type, Entry_Currency_ID, EntryValue, Branch_Id_Entry, Factor, Entry_Notes);

                    ////الخصم المسموح ية
                    Transaction_Type = 1;
                    AccountType = Common.CommissionValue_Account_Type;
                    EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["Net_Total_CommissionValue"].ToString());
                    //EntryValue = EntryValue * Factor;
                    Set_Invoice_RelatedAccount(ref aa, ref dt_Entry_Details, ref opp, AccountType, Transaction_Type, Entry_Currency_ID, EntryValue, Branch_Id_Entry, Factor, Entry_Notes);


                    ////مصروفات التحميل على فاتورة المشتريات
                    Transaction_Type = 2;
                    AccountType = Common.Masrofat_Load_Payment_Purchases_Account_Type;
                    EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["Load_Payment"].ToString());
                    //EntryValue = EntryValue * Factor;
                    Set_Invoice_RelatedAccount(ref aa, ref dt_Entry_Details, ref opp, AccountType, Transaction_Type, Entry_Currency_ID, EntryValue, Branch_Id_Entry, Factor, Entry_Notes);



                    ///////////////انشاء راس القيد
                    long Journal_Code = 1;// Journal_CodesList.Get_Journal_Code_ForSandouk(1);
                    int IS_Make_AutomaticFromTar7eel = 1;
                    long Entry_No = AccountEntry_CLS.NewEntry_No(ref opp, dtprocess_Date, journal_Code, Account_Entry_List_ID);
                    int Transform_Type = Common.Transform_Type;

                    aa.Make_AccountEntry(ref Excp, ref Msg, ref Account_Entry_List_ID, ref opp, User_ID_Login, User_Id_Login_Name, Branch_ID_Login, Entry_No, dtprocess_Date, Journal_Code, Entry_Currency_ID, Entry_Notes, Transform_Type, dt_Entry_Details, Entry_Total, Branch_Id_Entry, IS_Make_AutomaticFromTar7eel, Invoices_No, Entry_Currency_ID, Entry_Currency_Name, Factor, Bills_List_Id);
                    mysql = "UPDATE Invoices_List SET  IS_Posted_Flage =1,Account_Entry_List_ID=" + Account_Entry_List_ID + " Where Id = " + Bills_List_Id;
                    opp.insert_me(mysql);
                }
            }
            catch (Exception ee)
            {

                Excp = 1;
            }

        }
        public void Make_EntryForMoney_Financial_Notice(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, long Money_Financial_Notice_Id, DateTime dtFrom, DateTime dtTo, int IS_Posted_Flage)
        {
            if (Common.IS_Make_AutomaticFromTar7eel_For_Invoice == 0)
            {
                return;
            }
            //////// Delete Old Entry
            AccountEntry_CLS aa = new AccountEntry_CLS();
            DataTable dt_Entry_Details = new DataTable();

            aa.Reference_Type = Common.Account_Type_Taswea_Malia;
            aa.Reference_Type_Table_Id = Money_Financial_Notice_Id;

            aa.Delete_EntryFromAutomatic(ref opp, ref Excp, ref Msg, aa.Reference_Type_Table_Id, aa.Reference_Type, IsNotDeletePrevEntry);
            try
            {

                long Entry_Currency_ID = 0;
                //long Account_Entry_List_ID = 0;
                decimal EntryValue = 0;
                double Entry_Total = 0;

                //long Entry_Currency_ID = 0;
                decimal Transaction_Value = 0;
                string Entry_Notes = "";

                long  User_ID_Login = Common.User_Id;
                long  Branch_ID_Login = Common.Branch_Id;
                string User_Id_Login_Name = Common.User_Name;
                int Transaction_TypeForAccount_Taswea = 0;
                int Transaction_TypeCustomerSupplier = 0;
                long   Branch_Id_Entry = 0;
                DateTime dtprocess_Date = new DateTime();
                DataTable dt_ProcessName = new DataTable();
                int journal_Code = 0;
                string mysql = "";
                dt_Entry_Details = new DataTable();
                aa.Get_Empty_dt_Details(ref opp, ref dt_Entry_Details);

                //                mysql = @"SELECT 1 AS Factor, CAST(Invoices_List.Invoice_Id AS nvarchar) + N' فاتورة مبيعات رقم ' AS Notes ,  0 AS Id_Test,0 AS CostCenter_Id, 'لا يوجد' AS Cost_Center_Name, Submain_Account.Main_Account_Id, Submain_Account.SubAccount_NO, Submain_Account.code, Submain_Account.FirstSubCode, 
                //                         Submain_Account.lastPartIncrease, Submain_Account.Is_Last_Level, Submain_Account.Head_Main_Account, Submain_Account.Submain_Account_Balance, Submain_Account.Currency_Id AS Expr1, 
                //                         Submain_Account.CanDelete, Submain_Account.Account_Follows, Submain_Account.destruction_date, Submain_Account.destruction, Submain_Account.Account_Nature_Type, 
                //                         Submain_Account.Accept_Cost_Center_Flage, Submain_Account.Cost_Center_Tree_ID, Submain_Account.Account_Level, Submain_Account.Account_Code, Submain_Account.Company_ID_For_GL, 
                //                         Submain_Account.Company_Name_For_GL, Submain_Account.Account_Customer_Type_Value, Submain_Account.Account_Customer_Type_ID, Submain_Account.Transform_Type, 
                //                          Submain_Account.Branch_ID AS Expr6, Submain_Account.ConnectWith_Gl_Flage, Submain_Account.Currency_Id AS Account_Currency_ID, Invoices_List.*
                //FROM            Invoices_List INNER JOIN
                //                         Submain_Account ON Invoices_List.SubAccount_Id = Submain_Account.Id ";

                mysql = @" SELECT        1 AS Factor, Money_Financial_Notice_Items.Notes, 0 AS Id_Test, 0 AS CostCenter_Id, 'لا يوجد' AS Cost_Center_Name, Money_Financial_Notice_Items.Currency_Id, Money_Financial_Notice_Items.Id, Money_Financial_Notice_Items.SubAccount_Id, 
                         Submain_Account.SubAccount_Name, Money_Financial_Notice_Items.OrderNo, Money_Financial_Notice_Items.Order_Value, Money_Financial_Notice_Items.OrderDate, Submain_Account.Account_Customer_Type_Value, 
                         Submain_Account.Main_Account_Id, Submain_Account.Code, Submain_Account.Currency_Id AS Account_Currency_ID, Money_Financial_Notice_Items.Transaction_Type, Money_Financial_Notice_Items.Customer_Id, 
                         Money_Financial_Notice_Items.Supplier_Id, Money_Financial_Notice_Items.User_Id, Submain_Account.Branch_Id
FROM            Submain_Account INNER JOIN
                         Money_Financial_Notice_Items ON Submain_Account.Id = Money_Financial_Notice_Items.SubAccount_Id ";
                mysql = mysql + " WHERE (Submain_Account.Branch_Id = " + Common.Branch_Id + ")  ";
                if (IS_Posted_Flage == 0 | IS_Posted_Flage == 1)
                {
                    mysql = mysql + "   AND  Money_Financial_Notice_Items.IS_Posted_Flage =" + IS_Posted_Flage;
                }
                if (Money_Financial_Notice_Id > 0)
                {
                    mysql = mysql + " And Money_Financial_Notice_Items.Id=" + Money_Financial_Notice_Id;
                }
                else
                {
                    mysql = mysql + " And     (Money_Financial_Notice_Items.OrderDate  >= " + AccountModelsLib.operations.Get_DateToInsert(dtFrom) + ")AND (Money_Financial_Notice_Items.OrderDate  <= " + AccountModelsLib.operations.Get_DateToInsert(dtTo) + ") ";


                }


                dt_ProcessName = opp.return_DataTable(mysql);



                for (int x = 0; x < dt_ProcessName.Rows.Count; x++)
                {
                    Entry_Currency_ID = long.Parse(dt_ProcessName.Rows[0]["Currency_ID"].ToString());
                    Money_Financial_Notice_Id = int.Parse(dt_ProcessName.Rows[0]["Id"].ToString());
                    string SubAccount_Name = dt_ProcessName.Rows[x]["SubAccount_Name"].ToString();
                    string Invoices_No = dt_ProcessName.Rows[0]["OrderNo"].ToString();

                    CurrencyFactorsCLS_ cc = new CurrencyFactorsCLS_();
                    decimal Factor = cc.GetFactors(ref opp, Common.Entry_Currency_Id, Entry_Currency_ID);
                    Factor = Common.RoundMe(Factor);
                    string Entry_Currency_Name = cc.Get_CurrencyName(ref opp, Entry_Currency_ID);

                    Branch_Id_Entry = long.Parse(dt_ProcessName.Rows[x]["Branch_Id"].ToString());

                    dtprocess_Date = DateTime.Parse(dt_ProcessName.Rows[x]["OrderDate"].ToString());
                    EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["Order_Value"].ToString());
                    EntryValue = Common.RoundMe(EntryValue);
                    EntryValue = EntryValue * Factor;
                    Entry_Total = (double)EntryValue;

                    //Factor = decimal.Parse(dt_ProcessName.Rows[x]["Factor"].ToString());

                    long Account_Currency_ID = long.Parse(dt_ProcessName.Rows[x]["Currency_ID"].ToString());


                    ///Notes    ملاحظات حساب العمل او المورد
                    string CustomerNotesAccount = Common.GetValuesAfterSetArabic(Invoices_No) + " إشعار تسوية رقم ";
                    CustomerNotesAccount = CustomerNotesAccount + " - " + dt_ProcessName.Rows[x]["Notes"].ToString();
                    dt_ProcessName.Rows[x]["Notes"] = CustomerNotesAccount;

                    ////Notes  ملاحظات باقى حساب الفاتورة
                    Entry_Notes = " إشعار تسوية رقم " + Invoices_No + " لحساب " + SubAccount_Name + "  بتاريخ " + dtprocess_Date.ToString("yyyy/MM/dd");

                    Transaction_TypeCustomerSupplier = int.Parse(dt_ProcessName.Rows[x]["Transaction_type"].ToString());
                    ///
                    if (Transaction_TypeCustomerSupplier == 1)
                    {
                        Transaction_TypeForAccount_Taswea = 2;
                    }
                    if (Transaction_TypeCustomerSupplier == 2)
                    {
                        Transaction_TypeForAccount_Taswea = 1;
                    }
                    Transaction_TypeForAccount_Taswea = Transaction_TypeCustomerSupplier == 1 ? 2 : 1;

                    /////حساب  اشعار التسوية 
                    Add_Row_Account_Details_FromAdd(ref aa, ref dt_Entry_Details, ref dt_ProcessName, x, Transaction_TypeForAccount_Taswea, Account_Currency_ID, Branch_Id_Entry, EntryValue, Factor);


                    /////////حساب الاساسى عميل او مورد 


                    int AccountType = Common.Account_Type_Taswea_Malia;
                    EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["Order_Value"].ToString());
                    Set_Invoice_RelatedAccount(ref aa, ref dt_Entry_Details, ref opp, AccountType, Transaction_TypeCustomerSupplier, Entry_Currency_ID, EntryValue, Branch_Id_Entry, Factor, Entry_Notes);





                    ///////////////انشاء راس القيد
                    long Journal_Code = 1;// Journal_CodesList.Get_Journal_Code_ForSandouk(1);
                    int IS_Make_AutomaticFromTar7eel = 1;
                    long Entry_No = AccountEntry_CLS.NewEntry_No(ref opp, dtprocess_Date, journal_Code, Account_Entry_List_ID);
                    int Transform_Type = Common.Transform_Type;

                    aa.Make_AccountEntry(ref Excp, ref Msg, ref Account_Entry_List_ID, ref opp, User_ID_Login, User_Id_Login_Name, Branch_ID_Login, Entry_No, dtprocess_Date, Journal_Code, Entry_Currency_ID, Entry_Notes, Transform_Type, dt_Entry_Details, Entry_Total, Branch_Id_Entry, IS_Make_AutomaticFromTar7eel, Invoices_No, Entry_Currency_ID, Entry_Currency_Name, Factor, Money_Financial_Notice_Id);
                    mysql = "UPDATE Money_Financial_Notice_Items SET  IS_Posted_Flage =1,Account_Entry_List_ID=" + Account_Entry_List_ID + " Where Id = " + Money_Financial_Notice_Id;
                    opp.insert_me(mysql);
                }
            }
            catch (Exception ee)
            {

                Excp = 1;
            }

        }

        public void Make_EntryForCarsExaminations(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, long Reference_Type_Table_Id, DateTime dtFrom, DateTime dtTo, int IS_Posted_Flage)
        {
            if (Common.IS_Make_AutomaticFromTar7eel_For_Invoice == 0)
            {
                return;
            }
            //////// Delete Old Entry
            AccountEntry_CLS aa = new AccountEntry_CLS();
            DataTable dt_Entry_Details = new DataTable();

            aa.Reference_Type = Common.Account_Type_ForCarsExaminations;
            aa.Reference_Type_Table_Id = Reference_Type_Table_Id;

            aa.Delete_EntryFromAutomatic(ref opp, ref Excp, ref Msg, aa.Reference_Type_Table_Id, aa.Reference_Type, IsNotDeletePrevEntry);
            try
            {

                long Entry_Currency_ID = 0;
                //long Account_Entry_List_ID = 0;
                decimal EntryValue = 0;
                double Entry_Total = 0;

                //long Entry_Currency_ID = 0;
                decimal Transaction_Value = 0;
                string Entry_Notes = "";

                long  User_ID_Login = Common.User_Id;
                long  Branch_ID_Login = Common.Branch_Id;
                string User_Id_Login_Name = Common.User_Name;
                int Transaction_TypeForAccount_Taswea = 0;
                int Transaction_TypeCustomerSupplier = 0;
                long   Branch_Id_Entry = 0;
                DateTime dtprocess_Date = new DateTime();
                DataTable dt_ProcessName = new DataTable();
                int journal_Code = 0;
                string mysql = "";
                dt_Entry_Details = new DataTable();
                aa.Get_Empty_dt_Details(ref opp, ref dt_Entry_Details);

                
                mysql = @"  SELECT        1 AS Factor, 0 AS Id_Test, 0 AS CostCenter_Id, 'لا يوجد' AS Cost_Center_Name, Submain_Account.Account_Customer_Type_Value, Submain_Account.Main_Account_Id, Submain_Account.code, 
                         Submain_Account.Currency_Id AS Account_Currency_ID, Submain_Account.Branch_ID, CarsExaminations.Id, CarsExaminations.ExaminationDate, CarsExaminations.ExaminationNumber, CarsExaminations.User_Id AS Expr1, 
                         CarsExaminations.Currency_Id, CarsExaminations.SubAccount_Id, CarsExaminations.Notes, CarsExaminations.CarsExaminationValue, CarsExaminations.Transaction_Type, Submain_Account.SubAccount_Name
FROM            Submain_Account INNER JOIN
                         CarsExaminations ON Submain_Account.Id = CarsExaminations.SubAccount_Id ";
                mysql = mysql + " WHERE (Submain_Account.Branch_Id = " + Common.Branch_Id + ")  ";
                if (IS_Posted_Flage == 0 | IS_Posted_Flage == 1)
                {
                    mysql = mysql + "   AND  CarsExaminations.IS_Posted_Flage =" + IS_Posted_Flage;
                }
                if (Reference_Type_Table_Id > 0)
                {
                    mysql = mysql + " And CarsExaminations.Id=" + Reference_Type_Table_Id;
                }
                else
                {
                    mysql = mysql + " And     (CarsExaminations.ExaminationDate  >= " + AccountModelsLib.operations.Get_DateToInsert(dtFrom) + ")AND (CarsExaminations.ExaminationDate  <= " + AccountModelsLib.operations.Get_DateToInsert(dtTo) + ") ";


                }


                dt_ProcessName = opp.return_DataTable(mysql);



                for (int x = 0; x < dt_ProcessName.Rows.Count; x++)
                {
                    Entry_Currency_ID = long.Parse(dt_ProcessName.Rows[0]["Currency_ID"].ToString());
                    Reference_Type_Table_Id = int.Parse(dt_ProcessName.Rows[0]["Id"].ToString());
                    string SubAccount_Name = dt_ProcessName.Rows[x]["SubAccount_Name"].ToString();
                    string Invoices_No = dt_ProcessName.Rows[0]["ExaminationNumber"].ToString();

                    CurrencyFactorsCLS_ cc = new CurrencyFactorsCLS_();
                    decimal Factor = cc.GetFactors(ref opp, Common.Entry_Currency_Id, Entry_Currency_ID);
                    Factor = Common.RoundMe(Factor);
                    string Entry_Currency_Name = cc.Get_CurrencyName(ref opp, Entry_Currency_ID);

                    Branch_Id_Entry = long.Parse(dt_ProcessName.Rows[x]["Branch_Id"].ToString());

                    dtprocess_Date = DateTime.Parse(dt_ProcessName.Rows[x]["ExaminationDate"].ToString());
                    EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["CarsExaminationValue"].ToString());
                    EntryValue = Common.RoundMe(EntryValue);
                    EntryValue = EntryValue * Factor;
                    Entry_Total = (double)EntryValue;

                    //Factor = decimal.Parse(dt_ProcessName.Rows[x]["Factor"].ToString());

                    long Account_Currency_ID = long.Parse(dt_ProcessName.Rows[x]["Currency_ID"].ToString());


                    ///Notes    ملاحظات حساب العمل او المورد
                    string CustomerNotesAccount = Common.GetValuesAfterSetArabic(Invoices_No) + " تقرير فحص  رقم ";
                    CustomerNotesAccount = CustomerNotesAccount + " - " + dt_ProcessName.Rows[x]["Notes"].ToString();
                    dt_ProcessName.Rows[x]["Notes"] = CustomerNotesAccount;

                    ////Notes  ملاحظات باقى حساب الفاتورة
                    Entry_Notes = " تقرير فحص رقم " + Invoices_No + " لحساب " + SubAccount_Name + "  بتاريخ " + dtprocess_Date.ToString("yyyy/MM/dd");

                    Transaction_TypeCustomerSupplier = int.Parse(dt_ProcessName.Rows[x]["Transaction_type"].ToString());
                    ///
                    if (Transaction_TypeCustomerSupplier == 1)
                    {
                        Transaction_TypeForAccount_Taswea = 2;
                    }
                    if (Transaction_TypeCustomerSupplier == 2)
                    {
                        Transaction_TypeForAccount_Taswea = 1;
                    }
                    Transaction_TypeForAccount_Taswea = Transaction_TypeCustomerSupplier == 1 ? 2 : 1;

                    /////حساب  اشعار التسوية 
                    Add_Row_Account_Details_FromAdd(ref aa, ref dt_Entry_Details, ref dt_ProcessName, x, Transaction_TypeForAccount_Taswea, Account_Currency_ID, Branch_Id_Entry, EntryValue, Factor);


                    /////////حساب الاساسى عميل او مورد 


                    int AccountType = Common.Account_Type_ForCarsExaminations;
                    EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["CarsExaminationValue"].ToString());
                    Set_Invoice_RelatedAccount(ref aa, ref dt_Entry_Details, ref opp, AccountType, Transaction_TypeCustomerSupplier, Entry_Currency_ID, EntryValue, Branch_Id_Entry, Factor, Entry_Notes);
                     

                    ///////////////انشاء راس القيد
                    long Journal_Code = 1;// Journal_CodesList.Get_Journal_Code_ForSandouk(1);
                    int IS_Make_AutomaticFromTar7eel = 1;
                    long Entry_No = AccountEntry_CLS.NewEntry_No(ref opp, dtprocess_Date, journal_Code, Account_Entry_List_ID);
                    int Transform_Type = Common.Transform_Type;

                    aa.Make_AccountEntry(ref Excp, ref Msg, ref Account_Entry_List_ID, ref opp, User_ID_Login, User_Id_Login_Name, Branch_ID_Login, Entry_No, dtprocess_Date, Journal_Code, Entry_Currency_ID, Entry_Notes, Transform_Type, dt_Entry_Details, Entry_Total, Branch_Id_Entry, IS_Make_AutomaticFromTar7eel, Invoices_No, Entry_Currency_ID, Entry_Currency_Name, Factor, Reference_Type_Table_Id);
                    mysql = "UPDATE CarsExaminations SET  IS_Posted_Flage =1,Account_Entry_List_ID=" + Account_Entry_List_ID + " Where Id = " + Reference_Type_Table_Id;
                    opp.insert_me(mysql);
                }
            }
            catch (Exception ee)
            {
                Msg = ee.Message;
                Excp = 1;
            }

        }

        public void Make_EntryForMoney_DuePeriod(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, long ConsCustomerDebitList, DateTime dtFrom, DateTime dtTo, int IS_Posted_Flage)
        {
            if (Common.IS_Make_AutomaticFromTar7eel_For_Invoice == 0)
            {
                return;
            }
            //////// Delete Old Entry
            AccountEntry_CLS aa = new AccountEntry_CLS();
            DataTable dt_Entry_Details = new DataTable();

            aa.Reference_Type = Common.Account_Type_Taswea_Malia;
            aa.Reference_Type_Table_Id = ConsCustomerDebitList;

            aa.Delete_EntryFromAutomatic(ref opp, ref Excp, ref Msg, aa.Reference_Type_Table_Id, aa.Reference_Type, IsNotDeletePrevEntry);
            try
            {

                long Entry_Currency_ID = 0;
                //long Account_Entry_List_ID = 0;
                decimal EntryValue = 0;
                double Entry_Total = 0;

                //long Entry_Currency_ID = 0;
                decimal Transaction_Value = 0;
                string Entry_Notes = "";

                long  User_ID_Login = Common.User_Id;
                long  Branch_ID_Login = Common.Branch_Id;
                string User_Id_Login_Name = Common.User_Name;

                long   Branch_Id_Entry = 0;
                DateTime dtprocess_Date = new DateTime();
                DataTable dt_ProcessName = new DataTable();
                int journal_Code = 0;
                string mysql = "";
                dt_Entry_Details = new DataTable();
                aa.Get_Empty_dt_Details(ref opp, ref dt_Entry_Details);

                mysql = @"SELECT DISTINCT 
                         1 AS Factor, ConsCustomerDebitList.Notes, 0 AS Id_Test, 0 AS CostCenter_Id, 'لا يوجد' AS Cost_Center_Name, Submain_Account.Currency_Id, ConsCustomerDebitList.Id, Submain_Account.Id, 
                         Submain_Account.SubAccount_Name, Submain_Account.Account_Customer_Type_Value, Submain_Account.Main_Account_Id, Submain_Account.Code, Submain_Account.Currency_Id AS Account_Currency_ID, 
                         Submain_Account.Branch_ID, ConsCustomerDebitList.DebitDate, ConsCustomerDebitList.ListDebitTotal, ConsService.Name AS ServiceName, ConsCustomerDebitList.ProcessNo
FROM            Submain_Account INNER JOIN
                         ConsProjectServices ON Submain_Account.Id = ConsProjectServices.SubAccount_Id INNER JOIN
                         ConsService ON ConsProjectServices.ServiceId = ConsService.Id INNER JOIN
                         ConsCustomerDebitList ON ConsProjectServices.ServiceId = ConsCustomerDebitList.ServicesId   ";
                mysql = mysql + " WHERE (Submain_Account.Branch_Id = " + Common.Branch_Id + ")  ";
                if (IS_Posted_Flage == 0 | IS_Posted_Flage == 1)
                {
                    mysql = mysql + "   AND  ConsCustomerDebitList.IS_Posted_Flage =" + IS_Posted_Flage;
                }
                if (ConsCustomerDebitList > 0)
                {
                    mysql = mysql + " And ConsCustomerDebitList.Id=" + ConsCustomerDebitList;
                }
                else
                {
                    mysql = mysql + " And     (ConsCustomerDebitList.OrderDate  >= " + AccountModelsLib.operations.Get_DateToInsert(dtFrom) + ")AND (ConsCustomerDebitList.OrderDate  <= " + AccountModelsLib.operations.Get_DateToInsert(dtTo) + ") ";


                }


                dt_ProcessName = opp.return_DataTable(mysql);



                for (int x = 0; x < dt_ProcessName.Rows.Count; x++)
                {
                    Entry_Currency_ID = long.Parse(dt_ProcessName.Rows[0]["Currency_ID"].ToString());
                    ConsCustomerDebitList = int.Parse(dt_ProcessName.Rows[0]["Id"].ToString());
                    string SubAccount_Name = dt_ProcessName.Rows[x]["SubAccount_Name"].ToString();
                    string Invoices_No = dt_ProcessName.Rows[0]["ProcessNo"].ToString();
                    string ServiceName = dt_ProcessName.Rows[0]["ServiceName"].ToString();
                    CurrencyFactorsCLS_ cc = new CurrencyFactorsCLS_();
                    decimal Factor = cc.GetFactors(ref opp, Common.Entry_Currency_Id, Entry_Currency_ID);
                    Factor = Common.RoundMe(Factor);
                    string Entry_Currency_Name = cc.Get_CurrencyName(ref opp, Entry_Currency_ID);

                    Branch_Id_Entry = long.Parse(dt_ProcessName.Rows[x]["Branch_Id"].ToString());

                    dtprocess_Date = DateTime.Parse(dt_ProcessName.Rows[x]["DebitDate"].ToString());
                    EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["ListDebitTotal"].ToString());
                    EntryValue = Common.RoundMe(EntryValue);
                    EntryValue = EntryValue * Factor;
                    Entry_Total = (double)EntryValue;

                    //Factor = decimal.Parse(dt_ProcessName.Rows[x]["Factor"].ToString());

                    long Account_Currency_ID = long.Parse(dt_ProcessName.Rows[x]["Currency_ID"].ToString());


                    ///Notes    ملاحظات حساب العمل او المورد
                    string CustomerNotesAccount = Common.GetValuesAfterSetArabic(Invoices_No) + "  رقم " + ServiceName + " إشعار بقيمة الخدمة  ";
                    CustomerNotesAccount = CustomerNotesAccount + " - " + dt_ProcessName.Rows[x]["Notes"].ToString();
                    dt_ProcessName.Rows[x]["Notes"] = CustomerNotesAccount;

                    ////Notes  ملاحظات باقى حساب الفاتورة
                    Entry_Notes = " إشعار بقيمة الخدمة رقم " + Invoices_No + " لحساب " + SubAccount_Name + "  بتاريخ " + dtprocess_Date.ToString("yyyy/MM/dd");


                    ///
                    int Transaction_Type = 2;

                    /////حساب  اشعار التسوية 
                    Add_Row_Account_Details_FromAdd(ref aa, ref dt_Entry_Details, ref dt_ProcessName, x, Transaction_Type, Account_Currency_ID, Branch_Id_Entry, EntryValue, Factor);


                    /////////حساب الاساسى عميل او مورد 

                    Transaction_Type = 1;

                    mysql = @" SELECT ConsCustomerDebitDetails.*, Customer.SubAccount_Id
                       FROM            ConsCustomerDebitDetails INNER JOIN
                         Customer ON ConsCustomerDebitDetails.SubAccount_Id_Customer = Customer.SubAccount_Id ";
                    mysql = mysql + " WHERE        (ConsCustomerDebitDetails.CustomerDebitListId =" + ConsCustomerDebitList + ")";
                    int AccountType = Common.Account_Type_KhadamatProjectBuilding;
                    DataTable dt = opp.return_DataTable(mysql);
                    for (int y = 0; y < dt.Rows.Count; y++)
                    {
                        EntryValue = decimal.Parse(dt.Rows[y]["TotalDebit"].ToString());
                        long SubAccount_Id = long.Parse(dt.Rows[y]["SubAccount_Id"].ToString());
                        Set_Invoice_RelatedAccountBySubAccount_Id(ref aa, ref dt_Entry_Details, ref opp, SubAccount_Id, Transaction_Type, Entry_Currency_ID, EntryValue, Branch_Id_Entry, Factor, Entry_Notes);
                    }




                    ///////////////انشاء راس القيد
                    long Journal_Code = 1;// Journal_CodesList.Get_Journal_Code_ForSandouk(1);
                    int IS_Make_AutomaticFromTar7eel = 1;
                    long Entry_No = AccountEntry_CLS.NewEntry_No(ref opp, dtprocess_Date, journal_Code, Account_Entry_List_ID);
                    int Transform_Type = Common.Transform_Type;

                    aa.Make_AccountEntry(ref Excp, ref Msg, ref Account_Entry_List_ID, ref opp, User_ID_Login, User_Id_Login_Name, Branch_ID_Login, Entry_No, dtprocess_Date, Journal_Code, Entry_Currency_ID, Entry_Notes, Transform_Type, dt_Entry_Details, Entry_Total, Branch_Id_Entry, IS_Make_AutomaticFromTar7eel, Invoices_No, Entry_Currency_ID, Entry_Currency_Name, Factor, ConsCustomerDebitList);
                    mysql = "UPDATE ConsCustomerDebitList SET  IS_Posted_Flage =1,Account_Entry_List_ID=" + Account_Entry_List_ID + " Where Id = " + ConsCustomerDebitList;
                    opp.insert_me(mysql);
                }
            }
            catch (Exception ee)
            {

                Excp = 1;
            }

        }

        //public long Account_Entry_List_ID = 0;
        /*
        public void Make_EntryForMoney_Checks_Memory(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, long Checks_Memory_Id, DateTime dtFrom, DateTime dtTo, int Transaction_TypeForCheck, int IS_Posted_Flage, bool IsNotDeletePrevEntry)
        {
            if (Common.IS_Make_AutomaticFromTar7eel_For_Invoice == 0)
            {
                return;
            }
            //////// Delete Old Entry
            AccountEntry_CLS aa = new AccountEntry_CLS();
            DataTable dt_Entry_Details = new DataTable();
            aa.Reference_Type = Common.Checks_Memory_Reference_Type;//
            aa.Reference_Type_Table_Id = Checks_Memory_Id;
            aa.IsNotDeletePrevEntry = IsNotDeletePrevEntry;

            //Delete_EntryFromAutomatic(ref opp, ref Excp, ref Msg, aa.Reference_Type_Table_Id, aa.Reference_Type);
            try
            {

                long Entry_Currency_ID = 0;

                decimal EntryValue = 0;
                double Entry_Total = 0;

                //long Entry_Currency_ID = 0;
                decimal Transaction_Value = 0;
                string Entry_Notes = "";

                long  User_ID_Login = Common.User_Id;
                long  Branch_ID_Login = Common.Branch_Id;
                string User_Id_Login_Name = Common.User_Name;

                int Transaction_TypeAccount = 0;
                long   Branch_Id_Entry = 0;
                DateTime dtprocess_Date = new DateTime();
                DataTable dt_ProcessName = new DataTable();
                int journal_Code = 0;
                string mysql = "";
                dt_Entry_Details = new DataTable();
                aa.Get_Empty_dt_Details(ref opp, ref dt_Entry_Details);


                mysql = @" SELECT        Checks_Memory.Factor, Checks_Memory.Notes, 0 AS Id_Test, 0 AS CostCenter_Id, 'لا يوجد' AS Cost_Center_Name, Checks_Memory.Currency_Id, Checks_Memory.id, Checks_Memory.SubAccount_Id, 
                         Checks_Memory.SubAccount_Name, Submain_Account.Account_Customer_Type_Value, Submain_Account.Main_Account_Id, Submain_Account.code, Submain_Account.Currency_Id AS Account_Currency_ID, 
                         Checks_Memory.User_Id, Submain_Account.Branch_ID, Order_Money_Check.Order_Date, Order_Money_Check.Order_No, Checks_Memory.Transaction_Type, Checks_Memory.Check_Value, Checks_Memory.Check_No, 
                         Checks_Memory.SubAccount_Name_RelatedTo, Checks_Memory.SubAccount_Id_RelatedTo, Checks_Memory.SubAccount_Name_RelatedFrom, Checks_Memory.SubAccount_Id_RelatedFrom
FROM            Order_Money_Check INNER JOIN
                         Checks_Memory ON Order_Money_Check.Id = Checks_Memory.Order_Mony_Id INNER JOIN
                         Submain_Account ON Checks_Memory.SubAccount_Id = Submain_Account.Id    ";
                mysql = mysql + " WHERE (Submain_Account.Branch_Id = " + Common.Branch_Id + ")  ";
                if (IS_Posted_Flage == 0 | IS_Posted_Flage == 1)
                {
                    mysql = mysql + "   AND  Checks_Memory.IS_Posted_Flage =" + IS_Posted_Flage;
                }
                if (Checks_Memory_Id > 0)
                {
                    mysql = mysql + " And Checks_Memory.Id=" + Checks_Memory_Id;
                }
                else
                {
                    mysql = mysql + " And     (Order_Money_Check.Order_Date  >= " + AccountModelsLib.operations.Get_DateToInsert(dtFrom) + ")AND (Order_Money_Check.Order_Date <= " + AccountModelsLib.operations.Get_DateToInsert(dtTo) + ") ";


                }


                dt_ProcessName = opp.return_DataTable(mysql);


                string SubAccount_Name = "";
                long SubAccount_Id = 0;
                for (int x = 0; x < dt_ProcessName.Rows.Count; x++)
                {
                    Entry_Currency_ID = long.Parse(dt_ProcessName.Rows[0]["Currency_ID"].ToString());
                    Checks_Memory_Id = int.Parse(dt_ProcessName.Rows[0]["Id"].ToString());
                    //SubAccount_Id = long.Parse(dt_ProcessName.Rows[0]["SubAccount_Id"].ToString());
                    //SubAccount_Name = dt_ProcessName.Rows[x]["SubAccount_Name"].ToString();
                    SubAccount_Id = long.Parse(dt_ProcessName.Rows[0]["SubAccount_Id_RelatedTo"].ToString());
                    SubAccount_Name = dt_ProcessName.Rows[x]["SubAccount_Name_RelatedTo"].ToString();
                    string Check_No = dt_ProcessName.Rows[0]["Check_No"].ToString();

                    CurrencyFactorsCLS_ cc = new CurrencyFactorsCLS_();
                    decimal Factor = cc.GetFactors(ref opp, Common.Entry_Currency_Id, Entry_Currency_ID);
                    Factor = Common.RoundMe(Factor);
                    string Entry_Currency_Name = cc.Get_CurrencyName(ref opp, Entry_Currency_ID);

                    Branch_Id_Entry = long.Parse(dt_ProcessName.Rows[x]["Branch_Id"].ToString());

                    dtprocess_Date = DateTime.Parse(dt_ProcessName.Rows[x]["Order_Date"].ToString());
                    EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["Check_Value"].ToString());
                    EntryValue = Common.RoundMe(EntryValue);
                    EntryValue = EntryValue * Factor;
                    Entry_Total = (double)EntryValue;

                    //Factor = decimal.Parse(dt_ProcessName.Rows[x]["Factor"].ToString());

                    long Account_Currency_ID = long.Parse(dt_ProcessName.Rows[x]["Currency_ID"].ToString());


                    ///Notes    ملاحظات حساب العمل او المورد
                    string CustomerNotesAccount = Common.GetValuesAfterSetArabic(Check_No) + " شيك رقم ";
                    CustomerNotesAccount = CustomerNotesAccount + " - " + dt_ProcessName.Rows[x]["Notes"].ToString();
                    dt_ProcessName.Rows[x]["Notes"] = CustomerNotesAccount;

                    ////Notes  ملاحظات باقى حساب الفاتورة
                    Entry_Notes = " ِشيك رقم " + Check_No + " لحساب " + SubAccount_Name + "  بتاريخ " + dtprocess_Date.ToString("yyyy/MM/dd");

                    Transaction_TypeAccount = int.Parse(dt_ProcessName.Rows[x]["Transaction_type"].ToString());
                    ///
                    int Transaction_Type = Transaction_TypeForCheck;
                    if (Transaction_Type == 1)
                    {
                        Transaction_TypeForCheck = 2;
                        Transaction_TypeAccount = 1;
                    }
                    if (Transaction_Type == 2)
                    {
                        Transaction_TypeForCheck = 1;
                        Transaction_TypeAccount = 2;
                    }


                    /////حساب  الشيك   
                    Add_Row_Account_Details_FromAdd(ref aa, ref dt_Entry_Details, ref dt_ProcessName, x, Transaction_TypeForCheck, Account_Currency_ID, Branch_Id_Entry, EntryValue, Factor);


                    ///////// حساب الاساسى عميل او مورد او حساب اخر
                    int AccountType = 0;
                    if (Transaction_Type == 1)
                    {
                        AccountType = Common.Account_Type_Checks_Memory_Ward_Hafza;

                    }
                    else
                    {
                        AccountType = Common.Account_Type_Checks_Memory_Sadra_Hafza;
                    }
                    EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["Check_Value"].ToString());
                    Set_Invoice_RelatedAccount(ref aa, ref dt_Entry_Details, ref opp, AccountType, Transaction_TypeAccount, Entry_Currency_ID, EntryValue, Branch_Id_Entry, Factor, Entry_Notes);

                    if (Transaction_Type == 1)
                    {

                        //AccountType = Common.Account_Type_Checks_Memory_Ward_Hafza;
                        SubAccount_Id_RelatedTo = SubAccount_Id;
                        SubAccount_Name_RelatedTo = SubAccount_Name;
                        SubAccount_Id_RelatedFrom = SubAccount_Id_RelatedFrom;
                        SubAccount_Name_RelatedFrom = SubAccount_Name_RelatedFrom;
                    }
                    else
                    {
                        //AccountType = Common.Account_Type_Checks_Memory_Sadra_Hafza;
                        SubAccount_Id_RelatedTo = SubAccount_Id_RelatedTo;
                        SubAccount_Name_RelatedTo = SubAccount_Name_RelatedTo;
                        SubAccount_Id_RelatedFrom = SubAccount_Id;
                        SubAccount_Name_RelatedFrom = SubAccount_Name;
                    }



                    ///////////////انشاء راس القيد
                    long Journal_Code = 1;// Journal_CodesList.Get_Journal_Code_ForSandouk(1);
                    int IS_Make_AutomaticFromTar7eel = 1;
                    long Entry_No = AccountEntry_CLS.NewEntry_No(ref opp, dtprocess_Date, journal_Code);
                    int Transform_Type = Common.Transform_Type;

                    aa.Make_AccountEntry(ref Excp, ref Msg, ref Account_Entry_List_ID, ref opp, User_ID_Login, User_Id_Login_Name, Branch_ID_Login, Entry_No, dtprocess_Date, Journal_Code, Entry_Currency_ID, Entry_Notes, Transform_Type, dt_Entry_Details, Entry_Total, Branch_Id_Entry, IS_Make_AutomaticFromTar7eel, Check_No, Entry_Currency_ID, Entry_Currency_Name, Factor, Checks_Memory_Id);
                    UpdatChecks_Memory(ref opp, Checks_Memory_Id, Account_Entry_List_ID);
                }
            }
            catch (Exception ee)
            {

                Excp = 1;
            }

        }
       */
        public void Make_EntryForMoney_Checks_Memory(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, long SubAccount_Id_TypeAccount, long Checks_Memory_Id, DateTime dtFrom, DateTime dtTo, int Transaction_TypeForCheck, int IS_Posted_Flage, bool IsNotDeletePrevEntry)
        {
            if (Common.IS_Make_AutomaticFromTar7eel_For_Invoice == 0)
            {
                return;
            }
            //////// Delete Old Entry
            AccountEntry_CLS aa = new AccountEntry_CLS();
            DataTable dt_Entry_Details = new DataTable();
            aa.Reference_Type = Common.Checks_Memory_Reference_Type;//
            aa.Reference_Type_Table_Id = Checks_Memory_Id;
            aa.IsNotDeletePrevEntry = IsNotDeletePrevEntry;
            aa.Delete_EntryFromAutomatic(ref opp, ref Excp, ref Msg, aa.Reference_Type_Table_Id, aa.Reference_Type, IsNotDeletePrevEntry, Account_Entry_List_ID);



            try
            {
                return;
                long Entry_Currency_ID = 0;

                decimal EntryValue = 0;
                double Entry_Total = 0;

                //long Entry_Currency_ID = 0;
                decimal Transaction_Value = 0;
                string Entry_Notes = "";

                long  User_ID_Login = Common.User_Id;
                long  Branch_ID_Login = Common.Branch_Id;
                string User_Id_Login_Name = Common.User_Name;

                int Transaction_TypeAccount = 0;
                long   Branch_Id_Entry = 0;
                DateTime dtprocess_Date = new DateTime();
                DataTable dt_ProcessName = new DataTable();
                int journal_Code = 0;
                string mysql = "";
                dt_Entry_Details = new DataTable();
                aa.Get_Empty_dt_Details(ref opp, ref dt_Entry_Details);


                mysql = @" SELECT        Checks_Memory.Factor, Checks_Memory.Notes, 0 AS Id_Test, 0 AS CostCenter_Id, 'لا يوجد' AS Cost_Center_Name, Checks_Memory.Currency_Id, Checks_Memory.id, Checks_Memory.SubAccount_Id, 
                         Checks_Memory.SubAccount_Name, Submain_Account.Account_Customer_Type_Value, Submain_Account.Main_Account_Id, Submain_Account.code, Submain_Account.Currency_Id AS Account_Currency_ID, 
                         Checks_Memory.User_Id, Submain_Account.Branch_ID, Order_Money_Check.Order_Date, Order_Money_Check.Order_No, Checks_Memory.Transaction_Type, Checks_Memory.Check_Value, Checks_Memory.Check_No, 
                         Checks_Memory.SubAccount_Name_RelatedTo, Checks_Memory.SubAccount_Id_RelatedTo, Checks_Memory.SubAccount_Name_RelatedFrom, Checks_Memory.SubAccount_Id_RelatedFrom
FROM            Order_Money_Check INNER JOIN
                         Checks_Memory ON Order_Money_Check.Id = Checks_Memory.Order_Mony_Id INNER JOIN
                         Submain_Account ON Checks_Memory.SubAccount_Id = Submain_Account.Id    ";
                mysql = mysql + " WHERE (Submain_Account.Branch_Id = " + Common.Branch_Id + ")  ";
                if (IS_Posted_Flage == 0 | IS_Posted_Flage == 1)
                {
                    mysql = mysql + "   AND  Checks_Memory.IS_Posted_Flage =" + IS_Posted_Flage;
                }
                if (Checks_Memory_Id > 0)
                {
                    mysql = mysql + " And Checks_Memory.Id=" + Checks_Memory_Id;
                }
                else
                {
                    mysql = mysql + " And     (Order_Money_Check.Order_Date  >= " + AccountModelsLib.operations.Get_DateToInsert(dtFrom) + ")AND (Order_Money_Check.Order_Date <= " + AccountModelsLib.operations.Get_DateToInsert(dtTo) + ") ";


                }


                dt_ProcessName = opp.return_DataTable(mysql);


                string SubAccount_Name = "";
                long SubAccount_Id = 0;
                for (int x = 0; x < dt_ProcessName.Rows.Count; x++)
                {
                    Entry_Currency_ID = long.Parse(dt_ProcessName.Rows[0]["Currency_ID"].ToString());
                    Checks_Memory_Id = int.Parse(dt_ProcessName.Rows[0]["Id"].ToString());
                    //SubAccount_Id = long.Parse(dt_ProcessName.Rows[0]["SubAccount_Id"].ToString());
                    //SubAccount_Name = dt_ProcessName.Rows[x]["SubAccount_Name"].ToString();
                    SubAccount_Id = long.Parse(dt_ProcessName.Rows[0]["SubAccount_Id_RelatedTo"].ToString());
                    SubAccount_Name = dt_ProcessName.Rows[x]["SubAccount_Name_RelatedTo"].ToString();
                    string Check_No = dt_ProcessName.Rows[0]["Check_No"].ToString();

                    CurrencyFactorsCLS_ cc = new CurrencyFactorsCLS_();
                    decimal Factor = cc.GetFactors(ref opp, Common.Entry_Currency_Id, Entry_Currency_ID);
                    Factor = Common.RoundMe(Factor);
                    string Entry_Currency_Name = cc.Get_CurrencyName(ref opp, Entry_Currency_ID);

                    Branch_Id_Entry = long.Parse(dt_ProcessName.Rows[x]["Branch_Id"].ToString());

                    dtprocess_Date = DateTime.Parse(dt_ProcessName.Rows[x]["Order_Date"].ToString());
                    EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["Check_Value"].ToString());
                    EntryValue = Common.RoundMe(EntryValue);
                    EntryValue = EntryValue * Factor;
                    Entry_Total = (double)EntryValue;

                    //Factor = decimal.Parse(dt_ProcessName.Rows[x]["Factor"].ToString());

                    long Account_Currency_ID = long.Parse(dt_ProcessName.Rows[x]["Currency_ID"].ToString());


                    ///Notes    ملاحظات حساب العمل او المورد
                    string CustomerNotesAccount = Common.GetValuesAfterSetArabic(Check_No) + " شيك رقم ";
                    CustomerNotesAccount = CustomerNotesAccount + " - " + dt_ProcessName.Rows[x]["Notes"].ToString();
                    dt_ProcessName.Rows[x]["Notes"] = CustomerNotesAccount;

                    ////Notes  ملاحظات باقى حساب الفاتورة
                    Entry_Notes = " ِشيك رقم " + Check_No + " لحساب " + SubAccount_Name + "  بتاريخ " + dtprocess_Date.ToString("yyyy/MM/dd");

                    Transaction_TypeAccount = int.Parse(dt_ProcessName.Rows[x]["Transaction_type"].ToString());
                    ///
                    int Transaction_Type = Transaction_TypeForCheck;
                    if (Transaction_Type == 1)
                    {
                        Transaction_TypeForCheck = 2;
                        Transaction_TypeAccount = 1;
                    }
                    if (Transaction_Type == 2)
                    {
                        Transaction_TypeForCheck = 1;
                        Transaction_TypeAccount = 2;
                    }


                    /////حساب  الشيك   
                    Add_Row_Account_Details_FromAdd(ref aa, ref dt_Entry_Details, ref dt_ProcessName, x, Transaction_TypeForCheck, Account_Currency_ID, Branch_Id_Entry, EntryValue, Factor);


                    ///////// حساب الاساسى عميل او مورد او حساب اخر
                    int AccountType = 0;
                    if (Transaction_Type == 1)
                    {
                        AccountType = Common.Account_Type_Checks_Memory_Ward_Hafza;

                    }
                    else
                    {
                        AccountType = Common.Account_Type_Checks_Memory_Sadra_Hafza;
                    }
                    EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["Check_Value"].ToString());
                    if (SubAccount_Id_TypeAccount == 0)
                    {
                        Set_Invoice_RelatedAccount(ref aa, ref dt_Entry_Details, ref opp, AccountType, Transaction_TypeAccount, Entry_Currency_ID, EntryValue, Branch_Id_Entry, Factor, Entry_Notes);
                    }
                    else
                    {
                        Set_Invoice_RelatedAccount(ref aa, ref dt_Entry_Details, ref opp, SubAccount_Id_TypeAccount, Transaction_TypeAccount, Entry_Currency_ID, EntryValue, Branch_Id_Entry, Factor, Entry_Notes);
                    }
                    if (Transaction_Type == 1)
                    {

                        //AccountType = Common.Account_Type_Checks_Memory_Ward_Hafza;
                        SubAccount_Id_RelatedTo = SubAccount_Id;
                        SubAccount_Name_RelatedTo = SubAccount_Name;
                        SubAccount_Id_RelatedFrom = SubAccount_Id_RelatedFrom;
                        SubAccount_Name_RelatedFrom = SubAccount_Name_RelatedFrom;
                    }
                    else
                    {
                        //AccountType = Common.Account_Type_Checks_Memory_Sadra_Hafza;
                        SubAccount_Id_RelatedTo = SubAccount_Id_RelatedTo;
                        SubAccount_Name_RelatedTo = SubAccount_Name_RelatedTo;
                        SubAccount_Id_RelatedFrom = SubAccount_Id;
                        SubAccount_Name_RelatedFrom = SubAccount_Name;
                    }



                    ///////////////انشاء راس القيد
                    long Journal_Code = 1;// Journal_CodesList.Get_Journal_Code_ForSandouk(1);
                    int IS_Make_AutomaticFromTar7eel = 1;
                    long Entry_No = AccountEntry_CLS.NewEntry_No(ref opp, dtprocess_Date, journal_Code, Account_Entry_List_ID);
                    int Transform_Type = Common.Transform_Type;

                    aa.Make_AccountEntry(ref Excp, ref Msg, ref Account_Entry_List_ID, ref opp, User_ID_Login, User_Id_Login_Name, Branch_ID_Login, Entry_No, dtprocess_Date, Journal_Code, Entry_Currency_ID, Entry_Notes, Transform_Type, dt_Entry_Details, Entry_Total, Branch_Id_Entry, IS_Make_AutomaticFromTar7eel, Check_No, Entry_Currency_ID, Entry_Currency_Name, Factor, Checks_Memory_Id);
                    UpdatChecks_Memory(ref opp, Checks_Memory_Id, Account_Entry_List_ID);
                }
            }
            catch (Exception ee)
            {

                Excp = 1;
            }

        }
        /*
        public void Make_EntryForMoney_Checks_Memory__(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, long Checks_Memory_Id, DateTime dtFrom, DateTime dtTo, long SubAccount_Id_From, long SubAccount_Id_To)
        {
            if (Common.IS_Make_AutomaticFromTar7eel_For_Invoice == 0)
            {
                return;
            }

            int IS_Posted_Flage = 0;
            bool IsNotDeletePrevEntry = false;
            int Transaction_TypeForCheck = 1;
            //////// Delete Old Entry
            AccountEntry_CLS aa = new AccountEntry_CLS();
            DataTable dt_Entry_Details = new DataTable();

            aa.Reference_Type = Common.Checks_Memory_Reference_Type;//
            aa.Reference_Type_Table_Id = Checks_Memory_Id;
            aa.IsNotDeletePrevEntry = IsNotDeletePrevEntry;
            //Delete_EntryFromAutomatic(ref opp, ref Excp, ref Msg, aa.Reference_Type_Table_Id, aa.Reference_Type);
            try
            {

                long Entry_Currency_ID = 0;

                decimal EntryValue = 0;
                double Entry_Total = 0;

                //long Entry_Currency_ID = 0;
                decimal Transaction_Value = 0;
                string Entry_Notes = "";

                long  User_ID_Login = Common.User_Id;
                long  Branch_ID_Login = Common.Branch_Id;
                string User_Id_Login_Name = Common.User_Name;

                int Transaction_TypeAccount = 0;
                long   Branch_Id_Entry = 0;
                DateTime dtprocess_Date = new DateTime();
                DataTable dt_ProcessName = new DataTable();
                int journal_Code = 0;
                string mysql = "";
                dt_Entry_Details = new DataTable();
                aa.Get_Empty_dt_Details(ref opp, ref dt_Entry_Details);


                mysql = @"   SELECT        Checks_Memory.Factor, Checks_Memory.Notes, 0 AS Id_Test, 0 AS CostCenter_Id, 'لا يوجد' AS Cost_Center_Name, Checks_Memory.Currency_Id, Checks_Memory.id, Checks_Memory.SubAccount_Id, 
                         Checks_Memory.SubAccount_Name, Submain_Account.Account_Customer_Type_Value, Submain_Account.Main_Account_Id, Submain_Account.code, Submain_Account.Currency_Id AS Account_Currency_ID, 
                         Checks_Memory.User_Id, Submain_Account.Branch_ID, Order_Money_Check.Order_Date, Order_Money_Check.Order_No, Checks_Memory.Transaction_Type, Checks_Memory.Check_Value, Checks_Memory.Check_No
                       FROM Order_Money_Check INNER JOIN
                         Checks_Memory ON Order_Money_Check.Id = Checks_Memory.Order_Mony_Id INNER JOIN
                         Submain_Account ON Checks_Memory.SubAccount_Id = Submain_Account.Id ";
                mysql = mysql + " WHERE (Submain_Account.Branch_Id = " + Common.Branch_Id + ")  ";
                if (IS_Posted_Flage == 0 | IS_Posted_Flage == 1)
                {
                    mysql = mysql + "   AND  Checks_Memory.IS_Posted_Flage =" + IS_Posted_Flage;
                }
                if (Checks_Memory_Id > 0)
                {
                    mysql = mysql + " And Checks_Memory.Id=" + Checks_Memory_Id;
                }
                else
                {
                    mysql = mysql + " And     (Order_Money_Check.Order_Date  >= " + AccountModelsLib.operations.Get_DateToInsert(dtFrom) + ")AND (Order_Money_Check.Order_Date <= " + AccountModelsLib.operations.Get_DateToInsert(dtTo) + ") ";


                }


                dt_ProcessName = opp.return_DataTable(mysql);


                string SubAccount_Name = "";
                long SubAccount_Id = 0;
                for (int x = 0; x < dt_ProcessName.Rows.Count; x++)
                {
                    Entry_Currency_ID = long.Parse(dt_ProcessName.Rows[0]["Currency_ID"].ToString());
                    Checks_Memory_Id = int.Parse(dt_ProcessName.Rows[0]["Id"].ToString());
                    SubAccount_Id = long.Parse(dt_ProcessName.Rows[0]["SubAccount_Id"].ToString());
                    SubAccount_Name = dt_ProcessName.Rows[x]["SubAccount_Name"].ToString();
                    string Check_No = dt_ProcessName.Rows[0]["Check_No"].ToString();

                    CurrencyFactorsCLS_ cc = new CurrencyFactorsCLS_();
                    decimal Factor = cc.GetFactors(ref opp, Common.Entry_Currency_Id, Entry_Currency_ID);
                    Factor = Common.RoundMe(Factor);
                    string Entry_Currency_Name = cc.Get_CurrencyName(ref opp, Entry_Currency_ID);

                    Branch_Id_Entry = long.Parse(dt_ProcessName.Rows[x]["Branch_Id"].ToString());

                    dtprocess_Date = DateTime.Parse(dt_ProcessName.Rows[x]["Order_Date"].ToString());
                    EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["Check_Value"].ToString());
                    EntryValue = Common.RoundMe(EntryValue);
                    EntryValue = EntryValue * Factor;
                    Entry_Total = (double)EntryValue;

                    //Factor = decimal.Parse(dt_ProcessName.Rows[x]["Factor"].ToString());

                    long Account_Currency_ID = long.Parse(dt_ProcessName.Rows[x]["Currency_ID"].ToString());


                    ///Notes    ملاحظات حساب العمل او المورد
                    string CustomerNotesAccount = Common.GetValuesAfterSetArabic(Check_No) + " شيك رقم ";
                    CustomerNotesAccount = CustomerNotesAccount + " - " + dt_ProcessName.Rows[x]["Notes"].ToString();
                    dt_ProcessName.Rows[x]["Notes"] = CustomerNotesAccount;

                    ////Notes  ملاحظات باقى حساب الفاتورة
                    Entry_Notes = " ِشيك رقم " + Check_No + " لحساب " + SubAccount_Name + "  بتاريخ " + dtprocess_Date.ToString("yyyy/MM/dd");

                    Transaction_TypeAccount = int.Parse(dt_ProcessName.Rows[x]["Transaction_type"].ToString());
                    ///
                    int Transaction_Type = Transaction_TypeForCheck;
                    if (Transaction_Type == 1)
                    {
                        Transaction_TypeForCheck = 2;
                        Transaction_TypeAccount = 1;
                    }
                    if (Transaction_Type == 2)
                    {
                        Transaction_TypeForCheck = 1;
                        Transaction_TypeAccount = 2;
                    }


                    /////حساب  الشيك   
                    Add_Row_Account_Details_FromAdd(ref aa, ref dt_Entry_Details, ref dt_ProcessName, x, Transaction_TypeForCheck, Account_Currency_ID, Branch_Id_Entry, EntryValue, Factor);


                    ///////// حساب الاساسى عميل او مورد او حساب اخر
                    int AccountType = 0;
                    if (Transaction_Type == 1)
                    {
                        AccountType = Common.Account_Type_Checks_Memory_Ward_Hafza;

                    }
                    else
                    {
                        AccountType = Common.Account_Type_Checks_Memory_Sadra_Hafza;
                    }
                    EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["Check_Value"].ToString());
                    Set_Invoice_RelatedAccount(ref aa, ref dt_Entry_Details, ref opp, AccountType, Transaction_TypeAccount, Entry_Currency_ID, EntryValue, Branch_Id_Entry, Factor, Entry_Notes);

                    if (Transaction_Type == 1)
                    {

                        //AccountType = Common.Account_Type_Checks_Memory_Ward_Hafza;
                        SubAccount_Id_RelatedTo = SubAccount_Id;
                        SubAccount_Name_RelatedTo = SubAccount_Name;
                        SubAccount_Id_RelatedFrom = SubAccount_Id_RelatedFrom;
                        SubAccount_Name_RelatedFrom = SubAccount_Name_RelatedFrom;
                    }
                    else
                    {
                        //AccountType = Common.Account_Type_Checks_Memory_Sadra_Hafza;
                        SubAccount_Id_RelatedTo = SubAccount_Id_RelatedTo;
                        SubAccount_Name_RelatedTo = SubAccount_Name_RelatedTo;
                        SubAccount_Id_RelatedFrom = SubAccount_Id;
                        SubAccount_Name_RelatedFrom = SubAccount_Name;
                    }



                    ///////////////انشاء راس القيد
                    long Journal_Code = 1;// Journal_CodesList.Get_Journal_Code_ForSandouk(1);
                    int IS_Make_AutomaticFromTar7eel = 1;
                    long Entry_No = AccountEntry_CLS.NewEntry_No(ref opp, dtprocess_Date, journal_Code);
                    int Transform_Type = Common.Transform_Type;

                    aa.Make_AccountEntry(ref Excp, ref Msg, ref Account_Entry_List_ID, ref opp, User_ID_Login, User_Id_Login_Name, Branch_ID_Login, Entry_No, dtprocess_Date, Journal_Code, Entry_Currency_ID, Entry_Notes, Transform_Type, dt_Entry_Details, Entry_Total, Branch_Id_Entry, IS_Make_AutomaticFromTar7eel, Check_No, Entry_Currency_ID, Entry_Currency_Name, Factor, Checks_Memory_Id);
                    UpdatChecks_Memory(ref opp, Checks_Memory_Id, Account_Entry_List_ID);
                }
            }
            catch (Exception ee)
            {

                Excp = 1;
            }

        }
        */
        public void UpdatChecks_Memory(ref AccountModelsLib.operations opp, long Checks_Memory_Id, long Account_Entry_List_ID)
        {
            string mysql = "UPDATE Checks_Memory SET  IS_Posted_Flage =1, Account_Entry_List_ID=" + Account_Entry_List_ID + ",SubAccount_Id_RelatedTo=" + SubAccount_Id_RelatedTo + ", SubAccount_Name_RelatedTo=N'" + SubAccount_Name_RelatedTo + "',SubAccount_Id_RelatedFrom=" + SubAccount_Id_RelatedFrom + ",SubAccount_Name_RelatedFrom=N'" + SubAccount_Name_RelatedFrom + "' Where Id = " + Checks_Memory_Id;
            opp.insert_me(mysql);
        }

        public void Make_EntryForChangeStatus_Checks_Memory(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, DateTime EntryDate, long Checks_Memory_Id, int Transaction_TypeAccountRelated, long SubAccount_Id_RelatedFromNew, string SubAccount_Name_RelatedFromNew, long SubAccount_Id_RelatedToNew, string SubAccount_Name_RelatedToNew, string Notes, bool IsNotDeletePrevEntry)
        {
            if (Common.IS_Make_AutomaticFromTar7eel_For_Invoice == 0)
            {
                return;
            }
            //////// Delete Old Entry
            AccountEntry_CLS aa = new AccountEntry_CLS();
            DataTable dt_Entry_Details = new DataTable();

            aa.Reference_Type = Common.Checks_Memory_Reference_Type;//
            aa.Reference_Type_Table_Id = Checks_Memory_Id;
            aa.IsNotDeletePrevEntry = IsNotDeletePrevEntry;

            //Delete_EntryFromAutomatic(ref opp, ref Excp, ref Msg, aa.Reference_Type_Table_Id, aa.Reference_Type);
            try
            {

                long Entry_Currency_ID = 0;
                //long Account_Entry_List_ID = 0;
                decimal EntryValue = 0;
                double Entry_Total = 0;

                //long Entry_Currency_ID = 0;
                decimal Transaction_Value = 0;
                string Entry_Notes = "";

                long  User_ID_Login = Common.User_Id;
                long  Branch_ID_Login = Common.Branch_Id;
                string User_Id_Login_Name = Common.User_Name;


                long   Branch_Id_Entry = 0;
                DateTime dtprocess_Date = new DateTime();
                DataTable dt_ProcessName = new DataTable();
                int journal_Code = 0;
                string mysql = "";
                dt_Entry_Details = new DataTable();
                aa.Get_Empty_dt_Details(ref opp, ref dt_Entry_Details);


                mysql = @"  SELECT        Checks_Memory.Factor, Checks_Memory.Notes, 0 AS Id_Test, 0 AS CostCenter_Id, 'لا يوجد' AS Cost_Center_Name, Checks_Memory.Currency_Id, Checks_Memory.id, Submain_Account.Account_Customer_Type_Value, 
                         Submain_Account.Main_Account_Id, Submain_Account.code, Submain_Account.Currency_Id AS Account_Currency_ID, Checks_Memory.User_Id, Submain_Account.Branch_ID, Order_Money_Check.Order_Date, 
                         Order_Money_Check.Order_No, Checks_Memory.Transaction_Type, Checks_Memory.Check_Value, Checks_Memory.Check_No, Submain_Account.Id, Submain_Account.SubAccount_Name
                         FROM Order_Money_Check INNER JOIN
                         Checks_Memory ON Order_Money_Check.Id = Checks_Memory.Order_Mony_Id INNER JOIN
                         Submain_Account ON Checks_Memory.SubAccount_Id_RelatedFrom = Submain_Account.Id ";
                mysql = mysql + " WHERE (Submain_Account.Branch_Id = " + Common.Branch_Id + ")  ";

                if (Checks_Memory_Id > 0)
                {
                    mysql = mysql + " And Checks_Memory.Id=" + Checks_Memory_Id;
                }

                dt_ProcessName = opp.return_DataTable(mysql);

                string SubAccount_Name = "";
                long SubAccount_Id = 0;
                for (int x = 0; x < dt_ProcessName.Rows.Count; x++)
                {
                    Entry_Currency_ID = long.Parse(dt_ProcessName.Rows[0]["Currency_ID"].ToString());
                    Checks_Memory_Id = int.Parse(dt_ProcessName.Rows[0]["Id"].ToString());
                    //SubAccount_Id = long.Parse(dt_ProcessName.Rows[0]["SubAccount_Id"].ToString());
                    //SubAccount_Name = dt_ProcessName.Rows[x]["SubAccount_Name"].ToString();
                    string Check_No = dt_ProcessName.Rows[0]["Check_No"].ToString();

                    CurrencyFactorsCLS_ cc = new CurrencyFactorsCLS_();
                    decimal Factor = cc.GetFactors(ref opp, Common.Entry_Currency_Id, Entry_Currency_ID);
                    Factor = Common.RoundMe(Factor);
                    string Entry_Currency_Name = cc.Get_CurrencyName(ref opp, Entry_Currency_ID);

                    Branch_Id_Entry = long.Parse(dt_ProcessName.Rows[x]["Branch_Id"].ToString());

                    dtprocess_Date = EntryDate;// DateTime.Parse(dt_ProcessName.Rows[x]["Order_Date"].ToString());
                    EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["Check_Value"].ToString());
                    EntryValue = Common.RoundMe(EntryValue);
                    EntryValue = EntryValue * Factor;
                    Entry_Total = (double)EntryValue;

                    //Factor = decimal.Parse(dt_ProcessName.Rows[x]["Factor"].ToString());

                    long Account_Currency_ID = long.Parse(dt_ProcessName.Rows[x]["Currency_ID"].ToString());


                    ///Notes    ملاحظات حساب العمل او المورد
                    string CustomerNotesAccount = Common.GetValuesAfterSetArabic(Check_No) + " شيك رقم ";
                    CustomerNotesAccount = CustomerNotesAccount + " - " + Notes;
                    dt_ProcessName.Rows[x]["Notes"] = CustomerNotesAccount;

                    ////Notes  ملاحظات باقى حساب الفاتورة
                    Entry_Notes = " ِشيك رقم " + Check_No + " لحساب " + SubAccount_Name + "  بتاريخ " + dtprocess_Date.ToString("yyyy/MM/dd");
                    Entry_Notes = Entry_Notes + " - " + Notes;
                    // Transaction_TypeAccount = int.Parse(dt_ProcessName.Rows[x]["Transaction_type"].ToString());

                    ///// 
                    ///
                    int Transaction_TypeFrom = 2;
                    //Add_Row_Account_Details_FromAdd(ref aa, ref dt_Entry_Details, ref dt_ProcessName, x, Transaction_TypeFrom, Account_Currency_ID, Branch_Id_Entry, EntryValue, Factor);

                    EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["Check_Value"].ToString());
                    Set_Invoice_RelatedAccount(ref aa, ref dt_Entry_Details, ref opp, SubAccount_Id_RelatedToNew, Transaction_TypeFrom, Entry_Currency_ID, EntryValue, Branch_Id_Entry, Factor, Entry_Notes);

                    ///////// حساب الاساسى عميل او مورد او حساب اخر


                    // int AccountType = Common.Account_Type_Checks_Memory_Hafza;
                    int Transaction_TypeFromNew = 1;
                    EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["Check_Value"].ToString());
                    Set_Invoice_RelatedAccount(ref aa, ref dt_Entry_Details, ref opp, SubAccount_Id_RelatedFromNew, Transaction_TypeFromNew, Entry_Currency_ID, EntryValue, Branch_Id_Entry, Factor, Entry_Notes);





                    ///////////////انشاء راس القيد
                    long Journal_Code = 1;// Journal_CodesList.Get_Journal_Code_ForSandouk(1);
                    int IS_Make_AutomaticFromTar7eel = 1;
                    long Entry_No = AccountEntry_CLS.NewEntry_No(ref opp, dtprocess_Date, journal_Code, Account_Entry_List_ID);
                    int Transform_Type = Common.Transform_Type;

                    aa.Make_AccountEntry(ref Excp, ref Msg, ref Account_Entry_List_ID, ref opp, User_ID_Login, User_Id_Login_Name, Branch_ID_Login, Entry_No, dtprocess_Date, Journal_Code, Entry_Currency_ID, Entry_Notes, Transform_Type, dt_Entry_Details, Entry_Total, Branch_Id_Entry, IS_Make_AutomaticFromTar7eel, Check_No, Entry_Currency_ID, Entry_Currency_Name, Factor, Checks_Memory_Id);


                    //AccountType = Common.Account_Type_Checks_Memory_Ward_Hafza;
                    SubAccount_Id_RelatedTo = SubAccount_Id_RelatedToNew;
                    SubAccount_Name_RelatedTo = SubAccount_Name_RelatedToNew;
                    SubAccount_Id_RelatedFrom = SubAccount_Id_RelatedFromNew;
                    SubAccount_Name_RelatedFrom = SubAccount_Name_RelatedFromNew;


                    UpdatChecks_Memory(ref opp, Checks_Memory_Id, Account_Entry_List_ID);
                }
            }
            catch (Exception ee)
            {

                Excp = 1;
            }

        }

        public void Make_EntryForChangeStatus_Checks_Memory0(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, DateTime EntryDate, long Checks_Memory_Id, int Transaction_TypeAccountRelated, long SubAccount_Id_RelatedFromNew, string SubAccount_Name_RelatedFromNew, string Notes, bool IsNotDeletePrevEntry)
        {
            if (Common.IS_Make_AutomaticFromTar7eel_For_Invoice == 0)
            {
                return;
            }
            //////// Delete Old Entry
            AccountEntry_CLS aa = new AccountEntry_CLS();
            DataTable dt_Entry_Details = new DataTable();

            aa.Reference_Type = Common.Checks_Memory_Reference_Type;//
            aa.Reference_Type_Table_Id = Checks_Memory_Id;
            aa.IsNotDeletePrevEntry = IsNotDeletePrevEntry;

            //Delete_EntryFromAutomatic(ref opp, ref Excp, ref Msg, aa.Reference_Type_Table_Id, aa.Reference_Type);
            try
            {

                long Entry_Currency_ID = 0;
                //long Account_Entry_List_ID = 0;
                decimal EntryValue = 0;
                double Entry_Total = 0;

                //long Entry_Currency_ID = 0;
                decimal Transaction_Value = 0;
                string Entry_Notes = "";

                long  User_ID_Login = Common.User_Id;
                long  Branch_ID_Login = Common.Branch_Id;
                string User_Id_Login_Name = Common.User_Name;


                long   Branch_Id_Entry = 0;
                DateTime dtprocess_Date = new DateTime();
                DataTable dt_ProcessName = new DataTable();
                int journal_Code = 0;
                string mysql = "";
                dt_Entry_Details = new DataTable();
                aa.Get_Empty_dt_Details(ref opp, ref dt_Entry_Details);


                mysql = @"  SELECT        Checks_Memory.Factor, Checks_Memory.Notes, 0 AS Id_Test, 0 AS CostCenter_Id, 'لا يوجد' AS Cost_Center_Name, Checks_Memory.Currency_Id, Checks_Memory.id, Submain_Account.Account_Customer_Type_Value, 
                         Submain_Account.Main_Account_Id, Submain_Account.code, Submain_Account.Currency_Id AS Account_Currency_ID, Checks_Memory.User_Id, Submain_Account.Branch_ID, Order_Money_Check.Order_Date, 
                         Order_Money_Check.Order_No, Checks_Memory.Transaction_Type, Checks_Memory.Check_Value, Checks_Memory.Check_No, Submain_Account.Id, Submain_Account.SubAccount_Name
                         FROM Order_Money_Check INNER JOIN
                         Checks_Memory ON Order_Money_Check.Id = Checks_Memory.Order_Mony_Id INNER JOIN
                         Submain_Account ON Checks_Memory.SubAccount_Id_RelatedFrom = Submain_Account.Id ";
                mysql = mysql + " WHERE (Submain_Account.Branch_Id = " + Common.Branch_Id + ")  ";

                if (Checks_Memory_Id > 0)
                {
                    mysql = mysql + " And Checks_Memory.Id=" + Checks_Memory_Id;
                }

                dt_ProcessName = opp.return_DataTable(mysql);

                string SubAccount_Name = "";
                long SubAccount_Id = 0;
                for (int x = 0; x < dt_ProcessName.Rows.Count; x++)
                {
                    Entry_Currency_ID = long.Parse(dt_ProcessName.Rows[0]["Currency_ID"].ToString());
                    Checks_Memory_Id = int.Parse(dt_ProcessName.Rows[0]["Id"].ToString());
                    SubAccount_Id = long.Parse(dt_ProcessName.Rows[0]["SubAccount_Id"].ToString());
                    SubAccount_Name = dt_ProcessName.Rows[x]["SubAccount_Name"].ToString();
                    string Check_No = dt_ProcessName.Rows[0]["Check_No"].ToString();

                    CurrencyFactorsCLS_ cc = new CurrencyFactorsCLS_();
                    decimal Factor = cc.GetFactors(ref opp, Common.Entry_Currency_Id, Entry_Currency_ID);
                    Factor = Common.RoundMe(Factor);
                    string Entry_Currency_Name = cc.Get_CurrencyName(ref opp, Entry_Currency_ID);

                    Branch_Id_Entry = long.Parse(dt_ProcessName.Rows[x]["Branch_Id"].ToString());

                    dtprocess_Date = EntryDate;// DateTime.Parse(dt_ProcessName.Rows[x]["Order_Date"].ToString());
                    EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["Check_Value"].ToString());
                    EntryValue = Common.RoundMe(EntryValue);
                    EntryValue = EntryValue * Factor;
                    Entry_Total = (double)EntryValue;

                    //Factor = decimal.Parse(dt_ProcessName.Rows[x]["Factor"].ToString());

                    long Account_Currency_ID = long.Parse(dt_ProcessName.Rows[x]["Currency_ID"].ToString());


                    ///Notes    ملاحظات حساب العمل او المورد
                    string CustomerNotesAccount = Common.GetValuesAfterSetArabic(Check_No) + " شيك رقم ";
                    CustomerNotesAccount = CustomerNotesAccount + " - " + Notes;
                    dt_ProcessName.Rows[x]["Notes"] = CustomerNotesAccount;

                    ////Notes  ملاحظات باقى حساب الفاتورة
                    Entry_Notes = " ِشيك رقم " + Check_No + " لحساب " + SubAccount_Name + "  بتاريخ " + dtprocess_Date.ToString("yyyy/MM/dd");
                    Entry_Notes = Entry_Notes + " - " + Notes;
                    // Transaction_TypeAccount = int.Parse(dt_ProcessName.Rows[x]["Transaction_type"].ToString());

                    ///// 
                    ///
                    int Transaction_TypeFrom = 2;
                    Add_Row_Account_Details_FromAdd(ref aa, ref dt_Entry_Details, ref dt_ProcessName, x, Transaction_TypeFrom, Account_Currency_ID, Branch_Id_Entry, EntryValue, Factor);


                    ///////// حساب الاساسى عميل او مورد او حساب اخر


                    // int AccountType = Common.Account_Type_Checks_Memory_Hafza;
                    int Transaction_TypeFromNew = 1;
                    EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["Check_Value"].ToString());
                    Set_Invoice_RelatedAccount(ref aa, ref dt_Entry_Details, ref opp, SubAccount_Id_RelatedFromNew, Transaction_TypeFromNew, Entry_Currency_ID, EntryValue, Branch_Id_Entry, Factor, Entry_Notes);





                    ///////////////انشاء راس القيد
                    long Journal_Code = 1;// Journal_CodesList.Get_Journal_Code_ForSandouk(1);
                    int IS_Make_AutomaticFromTar7eel = 1;
                    long Entry_No = AccountEntry_CLS.NewEntry_No(ref opp, dtprocess_Date, journal_Code, Account_Entry_List_ID);
                    int Transform_Type = Common.Transform_Type;

                    aa.Make_AccountEntry(ref Excp, ref Msg, ref Account_Entry_List_ID, ref opp, User_ID_Login, User_Id_Login_Name, Branch_ID_Login, Entry_No, dtprocess_Date, Journal_Code, Entry_Currency_ID, Entry_Notes, Transform_Type, dt_Entry_Details, Entry_Total, Branch_Id_Entry, IS_Make_AutomaticFromTar7eel, Check_No, Entry_Currency_ID, Entry_Currency_Name, Factor, Checks_Memory_Id);


                    //AccountType = Common.Account_Type_Checks_Memory_Ward_Hafza;
                    SubAccount_Id_RelatedTo = SubAccount_Id;
                    SubAccount_Name_RelatedTo = SubAccount_Name;
                    SubAccount_Id_RelatedFrom = SubAccount_Id_RelatedFromNew;
                    SubAccount_Name_RelatedFrom = SubAccount_Name_RelatedFromNew;


                    UpdatChecks_Memory(ref opp, Checks_Memory_Id, Account_Entry_List_ID);
                }
            }
            catch (Exception ee)
            {

                Excp = 1;
            }

        }
        /*
         public void Make_EntryFor_Checks_Memory(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, DateTime EntryDate, long Checks_Memory_Id, Checks_MemoryTasweaCLS cts, string Notes, bool IsNotDeletePrevEntry)
         {
             bool IsTaswea = true;
             Make_EntryChecks_Memory(ref Excp, ref Msg, ref opp, EntryDate, Checks_Memory_Id, cts, IsTaswea, Notes, IsNotDeletePrevEntry);
             if (Excp == 0)
             {
                 IsTaswea = false;
                 Make_EntryChecks_Memory(ref Excp, ref Msg, ref opp, EntryDate, Checks_Memory_Id, cts, IsTaswea, Notes, IsNotDeletePrevEntry);
             }
         }
       */
        public void Make_EntryChecks_Memory(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, DateTime EntryDate, long Checks_Memory_Id, Checks_MemoryTasweaCLS cts, bool IsTaswea, string Notes, bool IsNotDeletePrevEntry)
        {
            long SubAccount_Id_From = 0;
            long SubAccount_Id_To = 0;
            if (IsTaswea == true)
            {
                SubAccount_Id_From = cts.SubAccount_Id_From_Taswea;
                SubAccount_Id_To = cts.SubAccount_Id_To_Taswea;


                SubAccount_Id_RelatedTo = cts.SubAccount_Id_To_Taswea;
                SubAccount_Name_RelatedTo = cts.SubAccount_Name_To_Taswea;
                SubAccount_Id_RelatedFrom = cts.SubAccount_Id_From_Taswea;
                SubAccount_Name_RelatedFrom = cts.SubAccount_Name_From_Taswea;

            }
            if (IsTaswea == false)
            {
                SubAccount_Id_From = cts.SubAccount_Id_From_New;
                SubAccount_Id_To = cts.SubAccount_Id_To_New;

                SubAccount_Id_RelatedTo = cts.SubAccount_Id_To_New;
                SubAccount_Name_RelatedTo = cts.SubAccount_Name_To_New;
                SubAccount_Id_RelatedFrom = cts.SubAccount_Id_From_New;
                SubAccount_Name_RelatedFrom = cts.SubAccount_Name_From_New;
            }
            if (Common.IS_Make_AutomaticFromTar7eel_For_Invoice == 0)
            {
                return;
            }
            //////// Delete Old Entry
            AccountEntry_CLS aa = new AccountEntry_CLS();
            DataTable dt_Entry_Details = new DataTable();

            aa.Reference_Type = Common.Checks_Memory_Reference_Type;//
            aa.Reference_Type_Table_Id = Checks_Memory_Id;
            aa.IsNotDeletePrevEntry = IsNotDeletePrevEntry;

            //Delete_EntryFromAutomatic(ref opp, ref Excp, ref Msg, aa.Reference_Type_Table_Id, aa.Reference_Type);
            try
            {

                long Entry_Currency_ID = 0;
                //long Account_Entry_List_ID = 0;
                decimal EntryValue = 0;
                double Entry_Total = 0;

                //long Entry_Currency_ID = 0;
                decimal Transaction_Value = 0;
                string Entry_Notes = "";

                long  User_ID_Login = Common.User_Id;
                long  Branch_ID_Login = Common.Branch_Id;
                string User_Id_Login_Name = Common.User_Name;


                long   Branch_Id_Entry = 0;
                DateTime dtprocess_Date = new DateTime();
                DataTable dt_ProcessName = new DataTable();
                int journal_Code = 0;
                string mysql = "";
                dt_Entry_Details = new DataTable();
                aa.Get_Empty_dt_Details(ref opp, ref dt_Entry_Details);


                mysql = @"  SELECT        Checks_Memory.Factor, Checks_Memory.Notes, 0 AS Id_Test, 0 AS CostCenter_Id, 'لا يوجد' AS Cost_Center_Name, Checks_Memory.Currency_Id, Checks_Memory.id, Submain_Account.Account_Customer_Type_Value, 
                         Submain_Account.Main_Account_Id, Submain_Account.code, Submain_Account.Currency_Id AS Account_Currency_ID, Checks_Memory.User_Id, Submain_Account.Branch_ID, Order_Money_Check.Order_Date, 
                         Order_Money_Check.Order_No, Checks_Memory.Transaction_Type, Checks_Memory.Check_Value, Checks_Memory.Check_No, Submain_Account.Id, Submain_Account.SubAccount_Name
                         FROM Order_Money_Check INNER JOIN
                         Checks_Memory ON Order_Money_Check.Id = Checks_Memory.Order_Mony_Id INNER JOIN
                         Submain_Account ON Checks_Memory.SubAccount_Id_RelatedFrom = Submain_Account.Id ";
                mysql = mysql + " WHERE (Submain_Account.Branch_Id = " + Common.Branch_Id + ")  ";

                if (Checks_Memory_Id > 0)
                {
                    mysql = mysql + " And Checks_Memory.Id=" + Checks_Memory_Id;
                }

                dt_ProcessName = opp.return_DataTable(mysql);

                //string SubAccount_Name = "";
                //long SubAccount_Id = 0;
                for (int x = 0; x < dt_ProcessName.Rows.Count; x++)
                {
                    Entry_Currency_ID = long.Parse(dt_ProcessName.Rows[0]["Currency_ID"].ToString());
                    Checks_Memory_Id = int.Parse(dt_ProcessName.Rows[0]["Id"].ToString());
                    //SubAccount_Id = long.Parse(dt_ProcessName.Rows[0]["SubAccount_Id"].ToString());
                    string SubAccount_Name = dt_ProcessName.Rows[x]["SubAccount_Name"].ToString();
                    string Check_No = dt_ProcessName.Rows[0]["Check_No"].ToString();

                    CurrencyFactorsCLS_ cc = new CurrencyFactorsCLS_();
                    decimal Factor = cc.GetFactors(ref opp, Common.Entry_Currency_Id, Entry_Currency_ID);
                    Factor = Common.RoundMe(Factor);
                    string Entry_Currency_Name = cc.Get_CurrencyName(ref opp, Entry_Currency_ID);

                    Branch_Id_Entry = long.Parse(dt_ProcessName.Rows[x]["Branch_Id"].ToString());

                    dtprocess_Date = EntryDate;// DateTime.Parse(dt_ProcessName.Rows[x]["Order_Date"].ToString());
                    EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["Check_Value"].ToString());
                    EntryValue = Common.RoundMe(EntryValue);
                    EntryValue = EntryValue * Factor;
                    Entry_Total = (double)EntryValue;

                    //Factor = decimal.Parse(dt_ProcessName.Rows[x]["Factor"].ToString());

                    long Account_Currency_ID = long.Parse(dt_ProcessName.Rows[x]["Currency_ID"].ToString());


                    ///Notes    ملاحظات حساب العمل او المورد
                    string CustomerNotesAccount = Common.GetValuesAfterSetArabic(Check_No) + " شيك رقم ";
                    CustomerNotesAccount = CustomerNotesAccount + " - " + Notes + "  قيد تسوية ";
                    dt_ProcessName.Rows[x]["Notes"] = CustomerNotesAccount;

                    ////Notes  ملاحظات باقى حساب الفاتورة
                    Entry_Notes = " ِشيك رقم " + Check_No + " لحساب " + SubAccount_Name + "  بتاريخ " + dtprocess_Date.ToString("yyyy/MM/dd");
                    Entry_Notes = Entry_Notes + " - " + Notes;
                    // Transaction_TypeAccount = int.Parse(dt_ProcessName.Rows[x]["Transaction_type"].ToString());

                    ///// 
                    ///
                    int Transaction_TypeFrom = 1;

                    EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["Check_Value"].ToString());
                    Set_Invoice_RelatedAccount(ref aa, ref dt_Entry_Details, ref opp, SubAccount_Id_From, Transaction_TypeFrom, Entry_Currency_ID, EntryValue, Branch_Id_Entry, Factor, Entry_Notes);


                    ///////// حساب الاساسى عميل او مورد او حساب اخر


                    // int AccountType = Common.Account_Type_Checks_Memory_Hafza;
                    int Transaction_TypeTo = 2;

                    EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["Check_Value"].ToString());
                    Set_Invoice_RelatedAccount(ref aa, ref dt_Entry_Details, ref opp, SubAccount_Id_To, Transaction_TypeTo, Entry_Currency_ID, EntryValue, Branch_Id_Entry, Factor, Entry_Notes);





                    ///////////////انشاء راس القيد
                    long Journal_Code = 1;// Journal_CodesList.Get_Journal_Code_ForSandouk(1);
                    int IS_Make_AutomaticFromTar7eel = 1;
                    long Entry_No = AccountEntry_CLS.NewEntry_No(ref opp, dtprocess_Date, journal_Code, Account_Entry_List_ID);
                    int Transform_Type = Common.Transform_Type;

                    aa.Make_AccountEntry(ref Excp, ref Msg, ref Account_Entry_List_ID, ref opp, User_ID_Login, User_Id_Login_Name, Branch_ID_Login, Entry_No, dtprocess_Date, Journal_Code, Entry_Currency_ID, Entry_Notes, Transform_Type, dt_Entry_Details, Entry_Total, Branch_Id_Entry, IS_Make_AutomaticFromTar7eel, Check_No, Entry_Currency_ID, Entry_Currency_Name, Factor, Checks_Memory_Id);




                    if (IsTaswea == true)
                    {
                        SubAccount_Id_RelatedTo = cts.SubAccount_Id_To_Taswea;
                        SubAccount_Name_RelatedTo = cts.SubAccount_Name_To_Taswea;
                        SubAccount_Id_RelatedFrom = cts.SubAccount_Id_From_Taswea;
                        SubAccount_Name_RelatedFrom = cts.SubAccount_Name_From_Taswea;

                    }
                    if (IsTaswea == false)
                    {
                        SubAccount_Id_RelatedTo = cts.SubAccount_Id_To_New;
                        SubAccount_Name_RelatedTo = cts.SubAccount_Name_To_New;
                        SubAccount_Id_RelatedFrom = cts.SubAccount_Id_From_New;
                        SubAccount_Name_RelatedFrom = cts.SubAccount_Name_From_New;
                    }

                    UpdatChecks_Memory(ref opp, Checks_Memory_Id, Account_Entry_List_ID);
                }
            }
            catch (Exception ee)
            {

                Excp = 1;
            }

        }
        public void Make_EntryChecks_Memory(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, DateTime EntryDate, long Checks_Memory_Id, Checks_MemoryTasweaCLS cts, string Notes, bool IsNotDeletePrevEntry)
        {
            long SubAccount_Id_From = 0;
            long SubAccount_Id_To = 0;


            SubAccount_Id_From = cts.SubAccount_Id_From_New;
            SubAccount_Id_To = cts.SubAccount_Id_To_New;

            SubAccount_Id_RelatedTo = cts.SubAccount_Id_To_New;
            SubAccount_Name_RelatedTo = cts.SubAccount_Name_To_New;
            SubAccount_Id_RelatedFrom = cts.SubAccount_Id_From_New;
            SubAccount_Name_RelatedFrom = cts.SubAccount_Name_From_New;

            if (Common.IS_Make_AutomaticFromTar7eel_For_Invoice == 0)
            {
                return;
            }
            //////// Delete Old Entry
            AccountEntry_CLS aa = new AccountEntry_CLS();
            DataTable dt_Entry_Details = new DataTable();

            aa.Reference_Type = Common.Checks_Memory_Reference_Type;//
            aa.Reference_Type_Table_Id = Checks_Memory_Id;
            aa.IsNotDeletePrevEntry = IsNotDeletePrevEntry;

            //Delete_EntryFromAutomatic(ref opp, ref Excp, ref Msg, aa.Reference_Type_Table_Id, aa.Reference_Type);
            try
            {

                long Entry_Currency_ID = 0;
                //long Account_Entry_List_ID = 0;
                decimal EntryValue = 0;
                double Entry_Total = 0;

                //long Entry_Currency_ID = 0;
                decimal Transaction_Value = 0;
                string Entry_Notes = "";

                long  User_ID_Login = Common.User_Id;
                long  Branch_ID_Login = Common.Branch_Id;
                string User_Id_Login_Name = Common.User_Name;


                long   Branch_Id_Entry = 0;
                DateTime dtprocess_Date = new DateTime();
                DataTable dt_ProcessName = new DataTable();
                int journal_Code = 0;
                string mysql = "";
                dt_Entry_Details = new DataTable();
                aa.Get_Empty_dt_Details(ref opp, ref dt_Entry_Details);


                mysql = @"  SELECT        Checks_Memory.Factor, Checks_Memory.Notes, 0 AS Id_Test, 0 AS CostCenter_Id, 'لا يوجد' AS Cost_Center_Name, Checks_Memory.Currency_Id, Checks_Memory.id, Submain_Account.Account_Customer_Type_Value, 
                         Submain_Account.Main_Account_Id, Submain_Account.code, Submain_Account.Currency_Id AS Account_Currency_ID, Checks_Memory.User_Id, Submain_Account.Branch_ID, Order_Money_Check.Order_Date, 
                         Order_Money_Check.Order_No, Checks_Memory.Transaction_Type, Checks_Memory.Check_Value, Checks_Memory.Check_No, Submain_Account.Id, Submain_Account.SubAccount_Name
                         FROM Order_Money_Check INNER JOIN
                         Checks_Memory ON Order_Money_Check.Id = Checks_Memory.Order_Mony_Id INNER JOIN
                         Submain_Account ON Checks_Memory.SubAccount_Id_RelatedFrom = Submain_Account.Id ";
                // حساب الحافظة قد يكون لكل الفروع (-1) كما فى شجرة الحسابات؛ بدون ذلك كان القيد يُتخطّى بصمت.
                mysql = mysql + " WHERE ((Submain_Account.Branch_Id = " + Common.Branch_Id + ") OR (Submain_Account.Branch_Id = -1))  ";

                if (Checks_Memory_Id > 0)
                {
                    mysql = mysql + " And Checks_Memory.Id=" + Checks_Memory_Id;
                }

                dt_ProcessName = opp.return_DataTable(mysql);

                //string SubAccount_Name = "";
                //long SubAccount_Id = 0;
                for (int x = 0; x < dt_ProcessName.Rows.Count; x++)
                {
                    Entry_Currency_ID = long.Parse(dt_ProcessName.Rows[0]["Currency_ID"].ToString());
                    Checks_Memory_Id = long.Parse(dt_ProcessName.Rows[0]["Id"].ToString()); // معرّف Snowflake أكبر من int
                    //SubAccount_Id = long.Parse(dt_ProcessName.Rows[0]["SubAccount_Id"].ToString());
                    string SubAccount_Name = dt_ProcessName.Rows[x]["SubAccount_Name"].ToString();
                    string Check_No = dt_ProcessName.Rows[0]["Check_No"].ToString();

                    CurrencyFactorsCLS_ cc = new CurrencyFactorsCLS_();
                    decimal Factor = cc.GetFactors(ref opp, Common.Entry_Currency_Id, Entry_Currency_ID);
                    Factor = Common.RoundMe(Factor);
                    string Entry_Currency_Name = cc.Get_CurrencyName(ref opp, Entry_Currency_ID);

                    Branch_Id_Entry = long.Parse(dt_ProcessName.Rows[x]["Branch_Id"].ToString());
                    if (Branch_Id_Entry <= 0) Branch_Id_Entry = Common.Branch_Id; // حساب لكل الفروع: القيد على الفرع الحالى

                    dtprocess_Date = EntryDate;// DateTime.Parse(dt_ProcessName.Rows[x]["Order_Date"].ToString());
                    EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["Check_Value"].ToString());
                    EntryValue = Common.RoundMe(EntryValue);
                    EntryValue = EntryValue * Factor;
                    Entry_Total = (double)EntryValue;

                    //Factor = decimal.Parse(dt_ProcessName.Rows[x]["Factor"].ToString());

                    long Account_Currency_ID = long.Parse(dt_ProcessName.Rows[x]["Currency_ID"].ToString());


                    ///Notes    ملاحظات حساب العمل او المورد
                    ///Notes    ملاحظات حساب العمل او المورد
                    string CustomerNotesAccount = Common.GetValuesAfterSetArabic(Check_No) + " شيك رقم ";
                    CustomerNotesAccount = CustomerNotesAccount + " - " + dt_ProcessName.Rows[x]["Notes"].ToString();
                    dt_ProcessName.Rows[x]["Notes"] = CustomerNotesAccount;

                    ////Notes  ملاحظات باقى حساب الفاتورة
                    Entry_Notes = " ِشيك رقم " + Check_No + " لحساب " + SubAccount_Name + "  بتاريخ " + dtprocess_Date.ToString("yyyy/MM/dd");

                    // Transaction_TypeAccount = int.Parse(dt_ProcessName.Rows[x]["Transaction_type"].ToString());

                    ///// 
                    ///
                    int Transaction_TypeFrom = 1;

                    EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["Check_Value"].ToString());
                    Set_Invoice_RelatedAccount(ref aa, ref dt_Entry_Details, ref opp, SubAccount_Id_From, Transaction_TypeFrom, Entry_Currency_ID, EntryValue, Branch_Id_Entry, Factor, Entry_Notes);


                    ///////// حساب الاساسى عميل او مورد او حساب اخر


                    // int AccountType = Common.Account_Type_Checks_Memory_Hafza;
                    int Transaction_TypeTo = 2;

                    EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["Check_Value"].ToString());
                    Set_Invoice_RelatedAccount(ref aa, ref dt_Entry_Details, ref opp, SubAccount_Id_To, Transaction_TypeTo, Entry_Currency_ID, EntryValue, Branch_Id_Entry, Factor, Entry_Notes);





                    ///////////////انشاء راس القيد
                    long Journal_Code = 1;// Journal_CodesList.Get_Journal_Code_ForSandouk(1);
                    int IS_Make_AutomaticFromTar7eel = 1;
                    long Entry_No = AccountEntry_CLS.NewEntry_No(ref opp, dtprocess_Date, journal_Code, Account_Entry_List_ID);
                    int Transform_Type = Common.Transform_Type;

                    aa.Make_AccountEntry(ref Excp, ref Msg, ref Account_Entry_List_ID, ref opp, User_ID_Login, User_Id_Login_Name, Branch_ID_Login, Entry_No, dtprocess_Date, Journal_Code, Entry_Currency_ID, Entry_Notes, Transform_Type, dt_Entry_Details, Entry_Total, Branch_Id_Entry, IS_Make_AutomaticFromTar7eel, Check_No, Entry_Currency_ID, Entry_Currency_Name, Factor, Checks_Memory_Id);





                    SubAccount_Id_RelatedTo = cts.SubAccount_Id_To_New;
                    SubAccount_Name_RelatedTo = cts.SubAccount_Name_To_New;
                    SubAccount_Id_RelatedFrom = cts.SubAccount_Id_From_New;
                    SubAccount_Name_RelatedFrom = cts.SubAccount_Name_From_New;


                    UpdatChecks_Memory(ref opp, Checks_Memory_Id, Account_Entry_List_ID);
                }
            }
            catch (Exception ee)
            {
                Msg = ee.Message;
                Excp = 1;
            }

        }
        /*
        public void Make_EntryForChangeStatus_Checks_MemoryToTazheer(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, DateTime EntryDate, int Transaction_TypeAccountRelated,   Models.Checks_Memory cmTz, string Notes, bool IsNotDeletePrevEntry)
        {
            if (Common.IS_Make_AutomaticFromTar7eel_For_Invoice == 0)
            {
                return;
            }

            long SubAccount_Id_RelatedFromNew = (long)cmTz.SubAccount_Id_RelatedFrom;
            string SubAccount_Name_RelatedFromNew = cmTz.SubAccount_Name_RelatedFrom;
            long Checks_Memory_Id = cmTz.Id;
            //////// Delete Old Entry
            AccountEntry_CLS aa = new AccountEntry_CLS();
            DataTable dt_Entry_Details = new DataTable();

            aa.Reference_Type = Common.Checks_Memory_Reference_Type;//
            aa.Reference_Type_Table_Id = Checks_Memory_Id;
            aa.IsNotDeletePrevEntry = IsNotDeletePrevEntry;
            //Delete_EntryFromAutomatic(ref opp, ref Excp, ref Msg, aa.Reference_Type_Table_Id, aa.Reference_Type);
            try
            {

                long Entry_Currency_ID = 0;
                long Account_Entry_List_ID = 0;
                decimal EntryValue = 0;
                double Entry_Total = 0;

                //long Entry_Currency_ID = 0;
                decimal Transaction_Value = 0;
                string Entry_Notes = "";

                long  User_ID_Login = Common.User_Id;
                long  Branch_ID_Login = Common.Branch_Id;
                string User_Id_Login_Name = Common.User_Name;


                long   Branch_Id_Entry = 0;
                DateTime dtprocess_Date = new DateTime();
                DataTable dt_ProcessName = new DataTable();
                int journal_Code = 0;
                string mysql = "";
                dt_Entry_Details = new DataTable();
                aa.Get_Empty_dt_Details(ref opp, ref dt_Entry_Details);


                mysql = @"  SELECT        Checks_Memory.Factor, Checks_Memory.Notes, 0 AS Id_Test, 0 AS CostCenter_Id, 'لا يوجد' AS Cost_Center_Name, Checks_Memory.Currency_Id, Checks_Memory.id, Submain_Account.Account_Customer_Type_Value, 
                         Submain_Account.Main_Account_Id, Submain_Account.code, Submain_Account.Currency_Id AS Account_Currency_ID, Checks_Memory.User_Id, Submain_Account.Branch_ID, Order_Money_Check.Order_Date, 
                         Order_Money_Check.Order_No, Checks_Memory.Transaction_Type, Checks_Memory.Check_Value, Checks_Memory.Check_No, Submain_Account.Id, Submain_Account.SubAccount_Name
                         FROM Order_Money_Check INNER JOIN
                         Checks_Memory ON Order_Money_Check.Id = Checks_Memory.Order_Mony_Id INNER JOIN
                         Submain_Account ON Checks_Memory.SubAccount_Id_RelatedFrom = Submain_Account.Id ";
                mysql = mysql + " WHERE (Submain_Account.Branch_Id = " + Common.Branch_Id + ")  ";

                if (Checks_Memory_Id > 0)
                {
                    mysql = mysql + " And Checks_Memory.Id=" + Checks_Memory_Id;
                }

                dt_ProcessName = opp.return_DataTable(mysql);

                //string SubAccount_Name_RelatedFrom = "";
                //long SubAccount_Id_RelatedFrom = 0;
                for (int x = 0; x < dt_ProcessName.Rows.Count; x++)
                {
                    Entry_Currency_ID = long.Parse(dt_ProcessName.Rows[0]["Currency_ID"].ToString());
                    Checks_Memory_Id = int.Parse(dt_ProcessName.Rows[0]["Id"].ToString());
                    //SubAccount_Id_RelatedFrom = long.Parse(dt_ProcessName.Rows[0]["SubAccount_Id_RelatedFrom"].ToString());
                    //SubAccount_Name_RelatedFrom = dt_ProcessName.Rows[x]["SubAccount_Name_RelatedFrom"].ToString();


                    string Check_No = dt_ProcessName.Rows[0]["Check_No"].ToString();

                    CurrencyFactorsCLS_ cc = new CurrencyFactorsCLS_();
                    decimal Factor = cc.GetFactors(ref opp, Common.Entry_Currency_Id, Entry_Currency_ID);
                    Factor = Common.RoundMe(Factor);
                    string Entry_Currency_Name = cc.Get_CurrencyName(ref opp, Entry_Currency_ID);

                    Branch_Id_Entry = long.Parse(dt_ProcessName.Rows[x]["Branch_Id"].ToString());

                    dtprocess_Date = EntryDate;// DateTime.Parse(dt_ProcessName.Rows[x]["Order_Date"].ToString());
                    EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["Check_Value"].ToString());
                    EntryValue = Common.RoundMe(EntryValue);
                    EntryValue = EntryValue * Factor;
                    Entry_Total = (double)EntryValue;

                    //Factor = decimal.Parse(dt_ProcessName.Rows[x]["Factor"].ToString());

                    long Account_Currency_ID = long.Parse(dt_ProcessName.Rows[x]["Currency_ID"].ToString());


                    ///Notes    ملاحظات حساب العمل او المورد
                    string CustomerNotesAccount = Common.GetValuesAfterSetArabic(Check_No) + " شيك رقم ";
                    CustomerNotesAccount = CustomerNotesAccount + " - " + Notes;
                    dt_ProcessName.Rows[x]["Notes"] = CustomerNotesAccount;

                    ////Notes  ملاحظات باقى حساب الفاتورة
                    Entry_Notes = " ِشيك رقم " + Check_No + "  تم تظهيرة بتاريخ " + dtprocess_Date.ToString("yyyy/MM/dd");
                    Entry_Notes = Entry_Notes + " - " + Notes;
                    // Transaction_TypeAccount = int.Parse(dt_ProcessName.Rows[x]["Transaction_type"].ToString());

                    ///// 
                    ///
                    int Transaction_TypeFrom = 2;
                    Add_Row_Account_Details_FromAdd(ref aa, ref dt_Entry_Details, ref dt_ProcessName, x, Transaction_TypeFrom, Account_Currency_ID, Branch_Id_Entry, EntryValue, Factor);


                    ///////// حساب الاساسى عميل او مورد او حساب اخر


                    // int AccountType = Common.Account_Type_Checks_Memory_Hafza;
                    //int Transaction_TypeFromNew = 1;
                    //EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["Check_Value"].ToString());
                    //Set_Invoice_RelatedAccount(ref aa, ref dt_Entry_Details, ref opp, SubAccount_Id_RelatedFromNew, Transaction_TypeFromNew, Entry_Currency_ID, EntryValue, Branch_Id_Entry, Factor, Entry_Notes);




                    int AccountType = Common.Account_Type_Checks_Memory_Sadra_Hafza;
                    int Transaction_TypeFromNew = 1;
                    EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["Check_Value"].ToString());
                    Set_Invoice_RelatedAccount(ref aa, ref dt_Entry_Details, ref opp, AccountType, Transaction_TypeFromNew, Entry_Currency_ID, EntryValue, Branch_Id_Entry, Factor, Entry_Notes);



                    ///////////////انشاء راس القيد
                    long Journal_Code = 1;// Journal_CodesList.Get_Journal_Code_ForSandouk(1);
                    int IS_Make_AutomaticFromTar7eel = 1;
                    long Entry_No = AccountEntry_CLS.NewEntry_No(ref opp, dtprocess_Date, journal_Code);
                    int Transform_Type = Common.Transform_Type;

                    aa.Make_AccountEntry(ref Excp, ref Msg, ref Account_Entry_List_ID, ref opp, User_ID_Login, User_Id_Login_Name, Branch_ID_Login, Entry_No, dtprocess_Date, Journal_Code, Entry_Currency_ID, Entry_Notes, Transform_Type, dt_Entry_Details, Entry_Total, Branch_Id_Entry, IS_Make_AutomaticFromTar7eel, Check_No, Entry_Currency_ID, Entry_Currency_Name, Factor, Checks_Memory_Id);



                    //SubAccount_Id_RelatedTo = SubAccount_Id;
                    //SubAccount_Name_RelatedTo = SubAccount_Name;
                    //SubAccount_Id_RelatedFrom = SubAccount_Id_RelatedFromNew;
                    //SubAccount_Name_RelatedFrom = SubAccount_Name_RelatedFromNew;


                    //UpdatChecks_Memory(ref opp, Checks_Memory_Id, Account_Entry_List_ID);
                }
            }
            catch (Exception ee)
            {

                Excp = 1;
            }

        }
        public void Make_EntryForChangeStatus_Checks_MemoryToEstlamFromSader(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, DateTime EntryDate, int Transaction_TypeAccountRelated,   Models.Checks_Memory cmTz, string Notes, bool IsNotDeletePrevEntry)
        {
            if (Common.IS_Make_AutomaticFromTar7eel_For_Invoice == 0)
            {
                return;
            }

            long SubAccount_Id_RelatedFromNew = (long)cmTz.SubAccount_Id_RelatedFrom;
            string SubAccount_Name_RelatedFromNew = cmTz.SubAccount_Name_RelatedFrom;
            long Checks_Memory_Id = cmTz.Id;
            //////// Delete Old Entry
            AccountEntry_CLS aa = new AccountEntry_CLS();
            DataTable dt_Entry_Details = new DataTable();

            aa.Reference_Type = Common.Checks_Memory_Reference_Type;//
            aa.Reference_Type_Table_Id = Checks_Memory_Id;
            aa.IsNotDeletePrevEntry = IsNotDeletePrevEntry;
            //Delete_EntryFromAutomatic(ref opp, ref Excp, ref Msg, aa.Reference_Type_Table_Id, aa.Reference_Type);
            try
            {

                long Entry_Currency_ID = 0;
                long Account_Entry_List_ID = 0;
                decimal EntryValue = 0;
                double Entry_Total = 0;

                //long Entry_Currency_ID = 0;
                decimal Transaction_Value = 0;
                string Entry_Notes = "";

                long  User_ID_Login = Common.User_Id;
                long  Branch_ID_Login = Common.Branch_Id;
                string User_Id_Login_Name = Common.User_Name;


                long   Branch_Id_Entry = 0;
                DateTime dtprocess_Date = new DateTime();
                DataTable dt_ProcessName = new DataTable();
                int journal_Code = 0;
                string mysql = "";
                dt_Entry_Details = new DataTable();
                aa.Get_Empty_dt_Details(ref opp, ref dt_Entry_Details);


                mysql = @"  SELECT        Checks_Memory.Factor, Checks_Memory.Notes, 0 AS Id_Test, 0 AS CostCenter_Id, 'لا يوجد' AS Cost_Center_Name, Checks_Memory.Currency_Id, Checks_Memory.id, Submain_Account.Account_Customer_Type_Value, 
                         Submain_Account.Main_Account_Id, Submain_Account.code, Submain_Account.Currency_Id AS Account_Currency_ID, Checks_Memory.User_Id, Submain_Account.Branch_ID, Order_Money_Check.Order_Date, 
                         Order_Money_Check.Order_No, Checks_Memory.Transaction_Type, Checks_Memory.Check_Value, Checks_Memory.Check_No, Submain_Account.Id, Submain_Account.SubAccount_Name
                         FROM Order_Money_Check INNER JOIN
                         Checks_Memory ON Order_Money_Check.Id = Checks_Memory.Order_Mony_Id INNER JOIN
                         Submain_Account ON Checks_Memory.SubAccount_Id_RelatedFrom = Submain_Account.Id ";
                mysql = mysql + " WHERE (Submain_Account.Branch_Id = " + Common.Branch_Id + ")  ";

                if (Checks_Memory_Id > 0)
                {
                    mysql = mysql + " And Checks_Memory.Id=" + Checks_Memory_Id;
                }

                dt_ProcessName = opp.return_DataTable(mysql);

                //string SubAccount_Name_RelatedFrom = "";
                //long SubAccount_Id_RelatedFrom = 0;
                for (int x = 0; x < dt_ProcessName.Rows.Count; x++)
                {
                    Entry_Currency_ID = long.Parse(dt_ProcessName.Rows[0]["Currency_ID"].ToString());
                    Checks_Memory_Id = int.Parse(dt_ProcessName.Rows[0]["Id"].ToString());
                    //SubAccount_Id_RelatedFrom = long.Parse(dt_ProcessName.Rows[0]["SubAccount_Id_RelatedFrom"].ToString());
                    //SubAccount_Name_RelatedFrom = dt_ProcessName.Rows[x]["SubAccount_Name_RelatedFrom"].ToString();


                    string Check_No = dt_ProcessName.Rows[0]["Check_No"].ToString();

                    CurrencyFactorsCLS_ cc = new CurrencyFactorsCLS_();
                    decimal Factor = cc.GetFactors(ref opp, Common.Entry_Currency_Id, Entry_Currency_ID);
                    Factor = Common.RoundMe(Factor);
                    string Entry_Currency_Name = cc.Get_CurrencyName(ref opp, Entry_Currency_ID);

                    Branch_Id_Entry = long.Parse(dt_ProcessName.Rows[x]["Branch_Id"].ToString());

                    dtprocess_Date = EntryDate;// DateTime.Parse(dt_ProcessName.Rows[x]["Order_Date"].ToString());
                    EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["Check_Value"].ToString());
                    EntryValue = Common.RoundMe(EntryValue);
                    EntryValue = EntryValue * Factor;
                    Entry_Total = (double)EntryValue;

                    //Factor = decimal.Parse(dt_ProcessName.Rows[x]["Factor"].ToString());

                    long Account_Currency_ID = long.Parse(dt_ProcessName.Rows[x]["Currency_ID"].ToString());


                    ///Notes    ملاحظات حساب العمل او المورد
                    string CustomerNotesAccount = Common.GetValuesAfterSetArabic(Check_No) + " شيك رقم ";
                    CustomerNotesAccount = CustomerNotesAccount + " - " + Notes;
                    dt_ProcessName.Rows[x]["Notes"] = CustomerNotesAccount;

                    ////Notes  ملاحظات باقى حساب الفاتورة
                    Entry_Notes = " ِشيك صادر رقم " + Check_No + "  تم استلامة بتاريخ " + dtprocess_Date.ToString("yyyy/MM/dd");
                    Entry_Notes = Entry_Notes + " - " + Notes;
                    // Transaction_TypeAccount = int.Parse(dt_ProcessName.Rows[x]["Transaction_type"].ToString());

                    ///// 
                    ///
                    int Transaction_TypeFrom = 2;
                    Add_Row_Account_Details_FromAdd(ref aa, ref dt_Entry_Details, ref dt_ProcessName, x, Transaction_TypeFrom, Account_Currency_ID, Branch_Id_Entry, EntryValue, Factor);


                    ///////// حساب الاساسى عميل او مورد او حساب اخر


                    // int AccountType = Common.Account_Type_Checks_Memory_Hafza;
                    //int Transaction_TypeFromNew = 1;
                    //EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["Check_Value"].ToString());
                    //Set_Invoice_RelatedAccount(ref aa, ref dt_Entry_Details, ref opp, SubAccount_Id_RelatedFromNew, Transaction_TypeFromNew, Entry_Currency_ID, EntryValue, Branch_Id_Entry, Factor, Entry_Notes);




                    int AccountType = Common.Account_Type_Checks_Memory_Sadra_Hafza;
                    int Transaction_TypeFromNew = 1;
                    EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["Check_Value"].ToString());
                    Set_Invoice_RelatedAccount(ref aa, ref dt_Entry_Details, ref opp, AccountType, Transaction_TypeFromNew, Entry_Currency_ID, EntryValue, Branch_Id_Entry, Factor, Entry_Notes);



                    ///////////////انشاء راس القيد
                    long Journal_Code = 1;// Journal_CodesList.Get_Journal_Code_ForSandouk(1);
                    int IS_Make_AutomaticFromTar7eel = 1;
                    long Entry_No = AccountEntry_CLS.NewEntry_No(ref opp, dtprocess_Date, journal_Code);
                    int Transform_Type = Common.Transform_Type;

                    aa.Make_AccountEntry(ref Excp, ref Msg, ref Account_Entry_List_ID, ref opp, User_ID_Login, User_Id_Login_Name, Branch_ID_Login, Entry_No, dtprocess_Date, Journal_Code, Entry_Currency_ID, Entry_Notes, Transform_Type, dt_Entry_Details, Entry_Total, Branch_Id_Entry, IS_Make_AutomaticFromTar7eel, Check_No, Entry_Currency_ID, Entry_Currency_Name, Factor, Checks_Memory_Id);



                    //SubAccount_Id_RelatedTo = SubAccount_Id;
                    //SubAccount_Name_RelatedTo = SubAccount_Name;
                    //SubAccount_Id_RelatedFrom = SubAccount_Id_RelatedFromNew;
                    //SubAccount_Name_RelatedFrom = SubAccount_Name_RelatedFromNew;


                    //UpdatChecks_Memory(ref opp, Checks_Memory_Id, Account_Entry_List_ID);
                }
            }
            catch (Exception ee)
            {

                Excp = 1;
            }

        }
        */
        public void Make_EntryFor_Suppliers_Destruction(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, DateTime EntryDate, long Suppliers_Destruction_Id, string Notes)
        {
            if (Common.IS_Make_AutomaticFromTar7eel_For_Invoice == 0)
            {
                return;
            }
            //////// Delete Old Entry
            AccountEntry_CLS aa = new AccountEntry_CLS();
            DataTable dt_Entry_Details = new DataTable();

            aa.Reference_Type = Common.Bill_Suppliers_Destruction_Id;
            aa.Reference_Type_Table_Id = Suppliers_Destruction_Id;
            //aa.IsNotDeletePrevEntry = IsNotDeletePrevEntry;
            aa.Delete_EntryFromAutomatic(ref opp, ref Excp, ref Msg, aa.Reference_Type_Table_Id, aa.Reference_Type, IsNotDeletePrevEntry);
            try
            {

                long Entry_Currency_ID = 0;
                //long Account_Entry_List_ID = 0;
                decimal EntryValue = 0;
                double Entry_Total = 0;

                //long Entry_Currency_ID = 0;
                decimal Transaction_Value = 0;
                string Entry_Notes = "";

                long  User_ID_Login = Common.User_Id;
                long  Branch_ID_Login = Common.Branch_Id;
                string User_Id_Login_Name = Common.User_Name;

                int Transaction_TypeAccount = 0;
                long   Branch_Id_Entry = 0;
                DateTime dtprocess_Date = new DateTime();
                DataTable dt_ProcessName = new DataTable();
                int journal_Code = 0;
                string mysql = "";
                dt_Entry_Details = new DataTable();
                aa.Get_Empty_dt_Details(ref opp, ref dt_Entry_Details);


                mysql = @"  SELECT        0 AS Id_Test, 0 AS CostCenter_Id, 'لا يوجد' AS Cost_Center_Name, Submain_Account.Account_Customer_Type_Value, Submain_Account.Main_Account_Id, Submain_Account.code, 
                         Submain_Account.Currency_Id,Submain_Account.Currency_Id AS Account_Currency_ID, Submain_Account.Branch_ID, Submain_Account.Id, Submain_Account.SubAccount_Name, Suppliers_Destruction.id AS Expr1, Suppliers_Destruction.dt_date, 
                         Suppliers_Destruction.Notes AS Expr2, Suppliers_Destruction.total, Suppliers_Destruction.Notes
FROM            Suppliers_Destruction INNER JOIN
                         Submain_Account ON Suppliers_Destruction.SubAccount_Id = Submain_Account.Id ";
                mysql = mysql + " WHERE (Submain_Account.Branch_Id = " + Common.Branch_Id + ")  ";

                if (Suppliers_Destruction_Id > 0)
                {
                    mysql = mysql + " And Suppliers_Destruction.Id=" + Suppliers_Destruction_Id;
                }



                dt_ProcessName = opp.return_DataTable(mysql);



                for (int x = 0; x < dt_ProcessName.Rows.Count; x++)
                {
                    Entry_Currency_ID = long.Parse(dt_ProcessName.Rows[0]["Currency_Id"].ToString());
                    //Suppliers_Destruction_Id = int.Parse(dt_ProcessName.Rows[0]["Id"].ToString());
                    string SubAccount_Name = dt_ProcessName.Rows[x]["SubAccount_Name"].ToString();


                    CurrencyFactorsCLS_ cc = new CurrencyFactorsCLS_();
                    decimal Factor = cc.GetFactors(ref opp, Common.Entry_Currency_Id, Entry_Currency_ID);
                    Factor = Common.RoundMe(Factor);
                    string Entry_Currency_Name = cc.Get_CurrencyName(ref opp, Entry_Currency_ID);

                    Branch_Id_Entry = long.Parse(dt_ProcessName.Rows[x]["Branch_Id"].ToString());

                    dtprocess_Date = DateTime.Parse(dt_ProcessName.Rows[x]["dt_date"].ToString());
                    EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["total"].ToString());
                    EntryValue = Common.RoundMe(EntryValue);
                    EntryValue = EntryValue * Factor;
                    Entry_Total = (double)EntryValue;

                    //Factor = decimal.Parse(dt_ProcessName.Rows[x]["Factor"].ToString());

                    long Account_Currency_ID = long.Parse(dt_ProcessName.Rows[x]["Currency_ID"].ToString());


                    ///Notes    ملاحظات حساب العمل او المورد
                    string CustomerNotesAccount = " مبيعات دشت";
                    CustomerNotesAccount = CustomerNotesAccount + " - " + Notes;
                    dt_ProcessName.Rows[x]["Notes"] = CustomerNotesAccount;

                    ////Notes  ملاحظات باقى حساب الفاتورة
                    Entry_Notes = " ِ مبيعات دشت " + " لحساب " + SubAccount_Name + "  بتاريخ " + dtprocess_Date.ToString("yyyy/MM/dd");
                    Entry_Notes = Entry_Notes + " - " + Notes;
                    Transaction_TypeAccount = 1;
                    /// 
                     ///////// حساب الاساسى عميل او مورد او حساب اخر 
                    Add_Row_Account_Details_FromAdd(ref aa, ref dt_Entry_Details, ref dt_ProcessName, x, Transaction_TypeAccount, Account_Currency_ID, Branch_Id_Entry, EntryValue, Factor);




                    Transaction_TypeAccount = 2;
                    int AccountType = Common.Account_Type_OtherEradat;
                    //EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["total"].ToString());
                    Set_Invoice_RelatedAccount(ref aa, ref dt_Entry_Details, ref opp, AccountType, Transaction_TypeAccount, Entry_Currency_ID, EntryValue, Branch_Id_Entry, Factor, Entry_Notes);





                    ///////////////انشاء راس القيد
                    long Journal_Code = 1;// Journal_CodesList.Get_Journal_Code_ForSandouk(1);
                    int IS_Make_AutomaticFromTar7eel = 1;
                    long Entry_No = AccountEntry_CLS.NewEntry_No(ref opp, dtprocess_Date, journal_Code, Account_Entry_List_ID);
                    int Transform_Type = Common.Transform_Type;

                    aa.Make_AccountEntry(ref Excp, ref Msg, ref Account_Entry_List_ID, ref opp, User_ID_Login, User_Id_Login_Name, Branch_ID_Login, Entry_No, dtprocess_Date, Journal_Code, Entry_Currency_ID, Entry_Notes, Transform_Type, dt_Entry_Details, Entry_Total, Branch_Id_Entry, IS_Make_AutomaticFromTar7eel, Suppliers_Destruction_Id.ToString(), Entry_Currency_ID, Entry_Currency_Name, Factor, Suppliers_Destruction_Id);
                    mysql = "UPDATE Suppliers_Destruction SET  IS_Posted_Flage =1,Account_Entry_List_ID=" + Account_Entry_List_ID + "   Where Id = " + Suppliers_Destruction_Id;
                    opp.insert_me(mysql);
                }
            }
            catch (Exception ee)
            {

                Excp = 1;
            }

        }


    }
}
