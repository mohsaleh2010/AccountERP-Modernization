using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using Microsoft.Data.SqlClient;
using System.Data.OleDb;

 
using AccountModelsLib.Models;

namespace AccountModelsLib
{
    public class Accounts_Entry_POST_CLS
    {
        public Accounts_Entry_POST_CLS()
        {
        }
        public bool IsNotDeletePrevEntry = false;
        public long Account_Entry_List_ID = 0;
        public long Entry_No = 0;


        //public bool IsNotDeletePrevEntry = false;
        //public long Account_Entry_List_ID = 0;

        decimal First_Balance = 0;
        decimal First_Balance_Madin = 0;
        decimal First_Balance_Da2n = 0;
        decimal Madin_Value = 0;
        decimal Da2n_Value = 0;
        decimal Balance = 0;
        decimal Balance_Madin = 0;
        decimal Balance_Da2n = 0;
        decimal Equivalent_First_Balance_Madin = 0;
        decimal Equivalent_First_Balance_Da2n = 0;
        decimal Equivalent_Madin_Value = 0;
        decimal Equivalent_Da2n_Value = 0;
        decimal Equivalent_Balance_Madin = 0;
        decimal Equivalent_Balance_Da2n = 0;

        public void Make_Post_WithNewTar7eel(ref int Excp, ref string Msg, int Submain_Account_ID, long Journal_Code, long Branch_Id, DateTime dtFrom, DateTime dtTo)
        {
            ////ترحيل الصندوق
            //int IS_Tar7eel_Sandouk_Automatic
            //Make_Post_For_Sandouk(IS_Tar7eel_Sandouk_Automatic,Branch_Id, dtFrom, dtTo);

            AccountModelsLib.operations opp = new AccountModelsLib.operations(1);
            try
            {

                string mysql = " ";
                string Mysql_Condition = "";

                Mysql_Condition = Mysql_Condition + " WHERE     (Account_Entry_List.Entry_Date >= " + AccountModelsLib.operations.Get_DateToInsert(dtFrom) + ")AND (Account_Entry_List.Entry_Date <= " + AccountModelsLib.operations.Get_DateToInsert(dtTo) + ") ";
                Mysql_Condition = Mysql_Condition + " AND  IS_Transfer_To_Gl_Flage=0 "; ///غير مرحل
                if (Journal_Code > 0)
                {
                    Mysql_Condition = Mysql_Condition + " AND (Account_Entry_List.journal_Code = " + Journal_Code + ") ";
                }
                if (Branch_Id > 0)
                {
                    Mysql_Condition = Mysql_Condition + " AND (Account_Entry_List.Branch_Id_Entry = " + Branch_Id + ") ";
                }
                if (Submain_Account_ID > 0)
                {
                    Mysql_Condition = Mysql_Condition + " AND (Account_Entry_Details.Submain_Account_ID = " + Submain_Account_ID + ") ";
                }


                mysql = @" SELECT     Account_Entry_List.*, Account_Entry_Details.*,Account_Entry_Details.Id AS Account_Entry_Details_Id, Submain_Account.*
FROM         Account_Entry_List INNER JOIN
                      Account_Entry_Details ON Account_Entry_List.ID = Account_Entry_Details.Account_Entry_List_ID INNER JOIN
                      Submain_Account ON Account_Entry_Details.Submain_Account_ID = Submain_Account.Id ";

                mysql = mysql + Mysql_Condition;



                mysql = mysql + " Order by Account_Entry_Details.Id ";
                DataTable dt = opp.return_DataTable(mysql);

                if (dt.Rows.Count == 0)
                {
                    opp.Rollback();
                    Msg = "لا توجد قيود لترحيلها";
                    return;

                }
                else
                {
                    Make_Post(ref Msg, dt, ref opp, Submain_Account_ID, Journal_Code, Branch_Id, dtFrom, dtTo);

                    //////////Update date
                    mysql = " UPDATE    Account_Entry_List SET IS_Transfer_To_Gl_Flage = 1, IS_Transfer_To_Gl_Flage_Str = N'مرحل', Transfer_To_Gl_DateTime = " + AccountModelsLib.operations.Get_DateTimeToInsert(DateTime.Now.Date) + " ";
                    mysql = mysql + Mysql_Condition;
                    opp.insert_me(mysql);
                }
                opp.Commit();
                Excp = 0;
                Msg = "تمت عملية ترحيل القيود بنجاح";
            }
            catch (Exception ee)
            {
                Excp = 1;

                Msg = "تمت عملية ترحيل القيود بنجاح";
                Msg = Msg + "\n\r";
                Msg = Msg + "\n\r" + ee.Message;
            }
        }

        public void Make_Post_WithChangeFirstBalance(ref string Msg, ref AccountModelsLib.operations opp, long Submain_Account_ID, long Journal_Code, long Branch_Id, DateTime dtFrom, DateTime dtTo)
        {
            DataTable dt = new DataTable();

            string mysql = " ";
            string Mysql_Condition = "";

            Mysql_Condition = Mysql_Condition + " WHERE     (Account_Entry_List.Entry_Date >= " + AccountModelsLib.operations.Get_DateToInsert(dtFrom) + ")AND (Account_Entry_List.Entry_Date <= " + AccountModelsLib.operations.Get_DateToInsert(dtTo) + ") ";
            Mysql_Condition = Mysql_Condition + " AND  IS_Transfer_To_Gl_Flage=1 "; ///  مرحل
            if (Journal_Code > 0)
            {
                Mysql_Condition = Mysql_Condition + " AND (Account_Entry_List.journal_Code = " + Journal_Code + ") ";
            }
            if (Branch_Id > 0)
            {
                Mysql_Condition = Mysql_Condition + " AND (Account_Entry_List.Branch_Id_Entry = " + Branch_Id + ") ";
            }
            if (Submain_Account_ID > 0)
            {
                Mysql_Condition = Mysql_Condition + " AND (Account_Entry_Details.Submain_Account_ID = " + Submain_Account_ID + ") ";
            }

            mysql = @" SELECT     Account_Entry_List.*, Account_Entry_Details.*,Account_Entry_Details.Id AS Account_Entry_Details_Id, Submain_Account.*
FROM         Account_Entry_List INNER JOIN
                      Account_Entry_Details ON Account_Entry_List.ID = Account_Entry_Details.Account_Entry_List_ID INNER JOIN
                      Submain_Account ON Account_Entry_Details.Submain_Account_ID = Submain_Account.Id ";

            mysql = mysql + Mysql_Condition;



            mysql = mysql + " Order by Account_Entry_Details.Id ";
            dt = opp.return_DataTable(mysql);

            if (dt.Rows.Count == 0)
            {
                opp.Rollback();
                Msg = "لا توجد قيود لترحيلها";
                return;

            }
            else
            {
                Make_Post(ref Msg, dt, ref opp, Submain_Account_ID, Journal_Code, Branch_Id, dtFrom, dtTo);

                //////////Update date
                mysql = " UPDATE       Account_Entry_List SET     IS_Transfer_To_Gl_Flage = 1, IS_Transfer_To_Gl_Flage_Str = N'مرحل', Transfer_To_Gl_DateTime = " + AccountModelsLib.operations.Get_DateTimeToInsert(DateTime.Now.Date) + "";
                mysql = mysql + " FROM    Account_Entry_List INNER JOIN Account_Entry_Details ON Account_Entry_List.ID = Account_Entry_Details.Account_Entry_List_ID ";
                mysql = mysql + Mysql_Condition;
                opp.insert_me(mysql);
            }

        }
        public void Make_Post(ref string Msg, DataTable dt, ref AccountModelsLib.operations opp, long Submain_Account_ID, long Journal_Code, long Branch_Id, DateTime dtFrom, DateTime dtTo)
        {


            string mysql = "";
            for (int x = 0; x < dt.Rows.Count; x++)
            {
                ////////////////////ترصيد الحساب فى جدول ارصدة الاستاذ  

                long Account_Entry_Details_Id = long.Parse(dt.Rows[x]["Account_Entry_Details_Id"].ToString());
                
                DateTime dt_Entry = DateTime.Parse(dt.Rows[x]["Entry_Date"].ToString());
                long Year_Financial_Period_Id = Financial_PeriodCLS.Get_Year_Financial_Period_Id(dt_Entry);
                Submain_Account_ID = long.Parse(dt.Rows[x]["Submain_Account_ID"].ToString());

                long   Branch_Id_Entry = long.Parse(dt.Rows[x]["Branch_Id_Entry"].ToString());
                //long    Currency_Id = long.Parse(dt.Rows[x]["Currency_Id"].ToString());

                long Account_Currency_ID = long.Parse(dt.Rows[x]["Account_Currency_ID"].ToString());
                long Entry_Currency_ID = long.Parse(dt.Rows[x]["Entry_Currency_ID"].ToString());

                decimal Import_Value = decimal.Parse(dt.Rows[x]["Import_Value"].ToString());
                decimal Export_Value = decimal.Parse(dt.Rows[x]["Export_Value"].ToString());

                decimal Equivalent_Import_Value = decimal.Parse(dt.Rows[x]["Equivalent_Import_Value"].ToString());
                decimal Equivalent_Export_Value = decimal.Parse(dt.Rows[x]["Equivalent_Export_Value"].ToString());
                decimal Equivalent_Transaction_Value = decimal.Parse(dt.Rows[x]["Equivalent_Transaction_Value"].ToString());

                long CostCenter_Id = long.Parse(dt.Rows[x]["CostCenter_Id"].ToString());
                string CostCenter_Name = dt.Rows[x]["CostCenter_Name"].ToString();
                int Line_No = int.Parse(dt.Rows[x]["Line_No"].ToString());
                string Notes_Details = dt.Rows[x]["Notes_Details"].ToString();

                string SubAccount_Name = dt.Rows[x]["SubAccount_Name"].ToString();
                if (Submain_Account_ID == 2045)
                {
                    int rrr = 0;
                }

                ////////////////بحث عن رقم الحساب فى جدول ارصده الاستاذ
                long Account_Entry_Master_Id_With_Account_Currency_ID = 0;

                Account_Entry_Master_Id_With_Account_Currency_ID = Search_Account_In_Master(ref opp, Year_Financial_Period_Id, Submain_Account_ID, SubAccount_Name, Branch_Id_Entry, Account_Currency_ID);
                MakeAccountPost_WithCurrency(ref opp, Account_Entry_Details_Id, Account_Entry_Master_Id_With_Account_Currency_ID, Import_Value, Export_Value, Equivalent_Import_Value, Equivalent_Export_Value);



            }

        }



        public void Cancel_Post(ref AccountModelsLib.operations opp, long Year_Financial_Period_Id, long Branch_Id)
        {
            string mysql = "";
            ///////////////تاريخ اول المده
            mysql = "Select * from Financial_Period Where Id=" + Year_Financial_Period_Id; ;
            DataTable dtF = opp.return_DataTable(mysql);
            string Entry_Date = AccountModelsLib.operations.Get_DateToInsert(DateTime.Now.Date);
            string DateFrom_ = AccountModelsLib.operations.Get_DateToInsert(DateTime.Now.Date);
            string DateTo_ = AccountModelsLib.operations.Get_DateToInsert(DateTime.Now.Date);
            DateTime DateFrom_Financial = new DateTime();
            DateTime DateTo_Financial = new DateTime();
            if (dtF.Rows.Count > 0)
            {
                Entry_Date = AccountModelsLib.operations.Get_DateToInsert(DateTime.Parse(dtF.Rows[0]["DateFrom"].ToString()));
                DateFrom_ = AccountModelsLib.operations.Get_DateToInsert(DateTime.Parse(dtF.Rows[0]["DateFrom"].ToString()));
                DateTo_ = AccountModelsLib.operations.Get_DateToInsert(DateTime.Parse(dtF.Rows[0]["DateTo"].ToString()));

                DateFrom_Financial = DateTime.Parse(dtF.Rows[0]["DateFrom"].ToString());
                DateTo_Financial = DateTime.Parse(dtF.Rows[0]["DateTo"].ToString());
            }



            mysql = " UPDATE  Account_Entry_Master_Balance SET  ";

            mysql = mysql + "   Balance =0";
            mysql = mysql + " , Balance_Madin =0";
            mysql = mysql + ", Balance_Da2n =0";

            mysql = mysql + ", Madin_Value =0";
            mysql = mysql + ", Da2n_Value =0";
            mysql = mysql + ", Equivalent_Madin_Value =0";
            mysql = mysql + ", Equivalent_Da2n_Value =0";

            mysql = mysql + " ,  Equivalent_Balance_Madin=0";
            mysql = mysql + " , Equivalent_Balance_Da2n =0";
            mysql = mysql + " ,IS_Madin_Da2n=1";
            mysql = mysql + " WHERE   (Year_Financial_Period_Id = " + Year_Financial_Period_Id + ") AND Branch_Id=" + Branch_Id + "";
            opp.insert_me(mysql);



            //////////////////////////////////////

            ////إلغاء الترصيد لحساب العميل لنفس السنه الماليه
            mysql = "  UPDATE       Account_Entry_Details SET   ";
            mysql = mysql + " Balance_Import_Value = 0, Balance_Export_Value = 0, Equivalent_Balance_Import_Value = 0, Equivalent_Balance_Export_Value = 0 ";
            mysql = mysql + " FROM   Account_Entry_Details INNER JOIN Account_Entry_List ON Account_Entry_Details.Account_Entry_List_ID = Account_Entry_List.ID ";
            mysql = mysql + " WHERE     (Account_Entry_List.Branch_Id_Entry = " + Branch_Id + ") ";
            mysql = mysql + " AND (Account_Entry_List.Entry_No IS NOT NULL) ";
            mysql = mysql + " AND (Account_Entry_List.Entry_Date >= " + DateFrom_ + ") AND (Account_Entry_List.Entry_Date <= " + DateTo_ + ") ";
            opp.insert_me(mysql);


            mysql = "UPDATE  Account_Entry_List SET    IS_Transfer_To_Gl_Flage = 0, IS_Transfer_To_Gl_Flage_Str = N'غير مرحل', Transfer_To_Gl_DateTime = NULL ";
            mysql = mysql + " Where  (Account_Entry_List.Branch_Id_Entry = " + Branch_Id + ") AND (Account_Entry_List.Entry_No IS NOT NULL) ";
            mysql = mysql + " AND (Account_Entry_List.Entry_Date >= " + DateFrom_ + ") AND (Account_Entry_List.Entry_Date <= " + DateTo_ + ") ";
            opp.insert_me(mysql);
        }






        public void Make_Entry_For_AccountsSandouk_(ref int Excp, ref string Msg, int IS_Tar7eel_Sandouk_Automatic, long Branch_Id, DateTime dtFrom, DateTime dtTo, long    Entry_Currency_ID, string Entry_Currency_Name, decimal Factor, long Order_Money_Id, long Entry_No)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations(1);
            Make_Entry_For_AccountsSandouk(ref Excp, ref Msg, ref opp, IS_Tar7eel_Sandouk_Automatic, Branch_Id, dtFrom, dtTo, Entry_Currency_ID, Entry_Currency_Name, Factor, Order_Money_Id, Entry_No);
            if (Excp == 0)
            {
                opp.Commit();
            }
            else
            {
                opp.Rollback();
            }


        }

        public void Make_Entry_For_AccountsSandouk(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, long Order_Money_Id)
        {
            string mysql = @" SELECT     * from Order_Money Where Id=" + Order_Money_Id;
            DataTable dt = opp.return_DataTable(mysql);
            int IS_Tar7eel_Sandouk_Automatic = 1;
            long Branch_Id = Common.Branch_Id;
            long Entry_Currency_ID = 0;
            string Entry_Currency_Name = "";
            decimal Factor = 0;
            DateTime Order_Date = new DateTime();
            long Entry_No = 0;

            if (dt.Rows.Count > 0)
            {
                Order_Date = DateTime.Parse(dt.Rows[0]["Order_Date"].ToString());
                Entry_Currency_ID = long.Parse(dt.Rows[0]["Entry_Currency_ID"].ToString());
                Entry_Currency_Name = dt.Rows[0]["Entry_Currency_Name"].ToString();
                Factor = decimal.Parse(dt.Rows[0]["Factor"].ToString());


            }

            Make_Entry_For_AccountsSandouk(ref Excp, ref Msg, ref opp, IS_Tar7eel_Sandouk_Automatic, Branch_Id, Order_Date, Order_Date, Entry_Currency_ID, Entry_Currency_Name, Factor, Order_Money_Id, Entry_No);
        }

        public void Make_Entry_For_AccountsSandouk(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, long Entry_Currency_ID, string Entry_Currency_Name, decimal Factor, long Order_Money_Id, long Entry_No)
        {

            int IS_Tar7eel_Sandouk_Automatic = 1;
            long Branch_Id = Common.Branch_Id;
            DateTime DateFrom = new DateTime(DateTime.Now.Year, 1, 1);
            DateTime DateTo = new DateTime(DateTime.Now.Year, 12, 31);


            //decimal Factor = 1;
            //if (AccountCommonDataLIB.Common.MsgBoxYesNo("هل تريد ترحيل الإذن الى الحسابات العامة بإصدار القيد", "الصندوق"))
            //{
            Make_Entry_For_AccountsSandouk(ref Excp, ref Msg, ref opp, IS_Tar7eel_Sandouk_Automatic, Branch_Id, DateFrom, DateTo, Entry_Currency_ID, Entry_Currency_Name, Factor, Order_Money_Id, Entry_No);


        }
        public void Make_Entry_For_AccountsSandouk(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, int IS_Tar7eel_Sandouk_Automatic, long Branch_Id, DateTime dtFrom, DateTime dtTo, long    Entry_Currency_ID, string Entry_Currency_Name, decimal Factor, long Order_Money_Id, long Entry_No)
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
                long User_ID_Login = Common.User_Id;
                long Branch_ID_Login = Common.Branch_Id;
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
                         Submain_Account.Transform_Type,  Submain_Account.Branch_ID AS Expr6, 
                         Submain_Account.ConnectWith_Gl_Flage, Banks.id AS Expr2, Banks.Bank_Name, Banks.Primary_balance, Banks.Last_Balance, Banks.Account_No, 
                         Banks.SubAccount_Id AS Expr3, Banks.Is_Bank, Banks.Currency_Id AS Expr4, Banks.firstBalanceDate, Banks.Supplier_Bank, Banks.Default_Bank, 
                         Banks.Account_Code AS Expr5, Banks.Static_Flage, Order_Money.Sandouk_Cost_Center_Tree_Id AS Cost_Center_Id, 
                         Submain_Account.Currency_Id AS Account_Currency_ID, Order_Money.Factor, Order_Money.Entry_Currency_ID, Submain_Account.Id AS SubAccount_Id
FROM            dbo.Submain_Account INNER JOIN
                         dbo.Banks ON dbo.Submain_Account.Id = dbo.Banks.SubAccount_Id INNER JOIN
                         dbo.Order_Money ON dbo.Banks.id = dbo.Order_Money.Bank_ID
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
                         Cost_Center_Tree.Name AS Cost_Center_Name, Money_Transaction_Box.Notes, Money_Transaction_Box.Factor, Money_Transaction_Box.Entry_Currency_ID, Submain_Account.Id AS SubAccount_Id
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
                            Entry_No = AccountEntry_CLS.NewEntry_No(ref opp, dtprocess_Date, journal_Code, Account_Entry_List_ID);
                        }
                        long Journal_Code = Common2.Get_Journal_Code_ForSandouk(Transaction_Type_For_Sandouk);
                        int IS_Make_AutomaticFromTar7eel = 1;
                        aa.Reference_Type = Common.Sandouk_Account_Type;
                        aa.Reference_Type_Table_Id = Order_Money_Id;

                        if (dt_Entry_Details.Rows.Count > 1)
                        {
                            Excp = 1;
                            aa.Make_AccountEntry(ref Excp, ref Msg, ref Account_Entry_List_ID, ref opp, User_ID_Login, User_Id_Login_Name, Branch_ID_Login, Entry_No, dtprocess_Date, Journal_Code, Entry_Currency_ID, Entry_Notes, Transaction_Type_ForAccount, dt_Entry_Details, Entry_Total, Branch_Id_Entry, IS_Make_AutomaticFromTar7eel, Order_No, Entry_Currency_ID, Entry_Currency_Name, Factor, Order_Money_Id);

                            // 🟢 بقرار المستخدم (خطة الفواتير §25.1، 2026-09-29): سطر الطرف (عميل/مورد/حساب آخر)
                            // فى قيد سند القبض/الصرف يجب أن يشير Reference_Type_Table_Id إلى حركته الخاصة
                            // (Money_Transaction_Box.Id) لا إلى رأس الإذن (Order_Money.Id) كباقى السطور. محرك
                            // القيود المشترك (AccountEntry_CLS.Make_AccountEntry) يكتب قيمة Reference_Type_Table_Id
                            // واحدة لكل سطور القيد دفعة واحدة (لا يوجد مفهوم "قيمة لكل سطر" فيه) فلا نعدّله؛
                            // بدلاً من ذلك نصحّح سطور الأطراف فقط بتحديث لاحق مباشر بعد إنشاء القيد، مقصور على
                            // هذه الدالة (خاصة بسندات الصندوق/النقدية) دون المساس بمحرك القيود العام أو بدوال
                            // القيود الأخرى (الفواتير/المشتريات). سطر الصندوق نفسه يبقى بمرجع الرأس Order_Money.Id
                            // لعدم وجود صف Money_Transaction_Box خاص به فى هذا المخطط (تحقَّق من بيانات حقيقية:
                            // كل صفوف Money_Transaction_Box لإذن ما تمثّل جانب "الطرف الآخر" فقط).
                            if (Excp == 0 && Account_Entry_List_ID > 0)
                            {
                                for (int y2 = 0; y2 < dt_Money_Transaction_Box.Rows.Count; y2++)
                                {
                                    long partySubAccountId = long.Parse(dt_Money_Transaction_Box.Rows[y2]["SubAccount_Id"].ToString());
                                    // 🟢 "Id1" لا "Id": الاستعلام أعلاه (بناء dt_Money_Transaction_Box) يختار كلاً من
                                    // Submain_Account.Id (بلا Alias) و Money_Transaction_Box.Id (بلا Alias أيضاً) معاً - ADO.NET
                                    // يعيد تسمية العمود الثانى تلقائياً إلى "Id1" لتفادى تعارض الاسم، فيصبح "Id" فعلياً هو
                                    // Submain_Account.Id (المطابق لقيمة SubAccount_Id نفسها) و"Id1" هو Money_Transaction_Box.Id
                                    // الحقيقى المطلوب هنا. تحقَّقتُ من هذا حياً: قبل هذا الإصلاح كانت القيمة المحفوظة تساوي
                                    // SubAccount_Id خطأً بدل معرّف صف Money_Transaction_Box.
                                    long partyBoxRowId = long.Parse(dt_Money_Transaction_Box.Rows[y2]["Id1"].ToString());
                                    string mysqlFixRef = "UPDATE Account_Entry_Details SET Reference_Type_Table_Id = " + partyBoxRowId +
                                        " WHERE Account_Entry_List_ID = " + Account_Entry_List_ID + " AND Submain_Account_ID = " + partySubAccountId;
                                    opp.insert_me(mysqlFixRef);
                                }
                            }
                        }

                    }
                    if (Excp == 0)
                    {
                        mysql = "UPDATE Order_Money SET  IS_Posted_Flage =1,Account_Entry_List_ID=" + Account_Entry_List_ID + " Where Id = " + Order_Money_Id;
                        opp.insert_me(mysql);
                    }
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
        public void Make_Entry_For_AccountsSandoukMulti(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, long    Entry_Currency_ID, string Entry_Currency_Name, decimal Factor, long Order_Money_Id, long Entry_No)
        {
            long tt = Account_Entry_List_ID;
            int IS_Tar7eel_Sandouk_Automatic = 1;
            long Branch_Id = Common.Branch_Id;
            DateTime DateFrom = new DateTime(DateTime.Now.Year, 1, 1);
            DateTime DateTo = new DateTime(DateTime.Now.Year, 12, 31);


            //decimal Factor = 1;
            //if (AccountCommonDataLIB.Common.MsgBoxYesNo("هل تريد ترحيل الإذن الى الحسابات العامة بإصدار القيد", "الصندوق"))
            //{
            Make_Entry_For_AccountsSandoukMulti(ref Excp, ref Msg, ref opp, IS_Tar7eel_Sandouk_Automatic, Branch_Id, DateFrom, DateTo, Entry_Currency_ID, Entry_Currency_Name, Factor, Order_Money_Id, Entry_No);


        }
        public void Make_Entry_For_AccountsSandoukMulti(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, int IS_Tar7eel_Sandouk_Automatic, long Branch_Id, DateTime dtFrom, DateTime dtTo, long    Entry_Currency_ID, string Entry_Currency_Name, decimal Factor, long Order_Money_Id, long Entry_No)
        {
            try
            {
                //////// Delete Old Entry
                AccountEntry_CLS aa = new AccountEntry_CLS();
                DataTable dt_Entry_Details = new DataTable();

                ///////Delete Old Entry
                aa.Reference_Type = Common.Sandouk_Account_Type;
                aa.Reference_Type_Table_Id = Order_Money_Id;
                AccountEntry_CLS ac = new AccountEntry_CLS();
                ac.Delete_EntryFromAutomatic(ref opp, ref Excp, ref Msg, aa.Reference_Type_Table_Id, aa.Reference_Type, IsNotDeletePrevEntry, this.Account_Entry_List_ID);



                long Account_Entry_List_Id_ = 0;
                decimal EntryValue = 0;
                double Entry_Total = 0;
                decimal Equivalent_Value = 0;
                //long Entry_Currency_ID = 0;
                decimal Transaction_Value = 0;
                string Entry_Notes = "";


                long User_ID_Login = Common.User_Id;
                long Branch_ID_Login = Common.Branch_Id;
                string User_Id_Login_Name = Common.User_Name;
                string mysql = @" SELECT        0 AS CostCenter_Id, 'لا يوجد' AS Cost_Center_Name, Order_Money.Id, Order_Money.Order_No, Order_Money.Order_Date, Order_Money.Transaction_type, Order_Money.Total_Value, Order_Money.Currency_Id, 
                         Order_Money.Bank_Type, Order_Money.Bank_ID, Order_Money.Man_Name, Order_Money.Is_Balances_Set, Order_Money.Day_Registration_flage, Order_Money.DayReg_List_Id, Order_Money.Order_mony_id_transform, 
                         Order_Money.Invoice_Id, Order_Money.User_Id, Order_Money.Branch_Id, Order_Money.Year_Financial_Period_Id, Order_Money.Ticket_Supplier_Id, Order_Money.Order_Tafkeet_Value, 
                         Order_Money.Tafkeet_Value, Order_Money.Sandouk_Cost_Center_Tree_Id, Order_Money.IS_Posted_Flage, Submain_Account.Id,Submain_Account.Id AS SubAccount_Id, Submain_Account.Main_Account_Id, Submain_Account.SubAccount_NO, 
                         Submain_Account.Code, Submain_Account.FirstSubCode, Submain_Account.lastPartIncrease, Submain_Account.SubAccount_Name, Submain_Account.IS_Last_Level, Submain_Account.Head_Main_Account, 
                         Submain_Account.Currency_Id AS Expr1, Submain_Account.CanDelete, Submain_Account.Account_Follows, 
                         Submain_Account.Account_Nature_Type, Submain_Account.Accept_Cost_Center_Flage, Submain_Account.Cost_Center_Tree_ID, Submain_Account.Account_Level, Submain_Account.Account_Code, 
                         Submain_Account.Company_ID_For_GL, Submain_Account.Company_Name_For_GL, Submain_Account.Account_Customer_Type_Value, Submain_Account.Account_Customer_Type_ID, Submain_Account.Transform_Type, 
                           Submain_Account.Branch_ID AS Expr6, Submain_Account.ConnectWith_Gl_Flage, Banks.id AS Expr2, Banks.Bank_Name, Banks.Primary_balance, Banks.Last_Balance, 
                         Banks.Account_No, Banks.SubAccount_Id AS Expr3, Banks.Is_Bank, Banks.Currency_Id AS Expr4, Banks.firstBalanceDate, Banks.Supplier_Bank, Banks.Default_Bank, Banks.Account_Code AS Expr5, Banks.Static_Flage, 
                         Order_Money.Sandouk_Cost_Center_Tree_Id AS Cost_Center_Id, Submain_Account.Currency_Id AS Account_Currency_ID, Order_Money.Factor, Order_Money.Entry_Currency_ID,
                         Money_Transaction_Box.Id AS Money_Transaction_BoxId,Money_Transaction_Box.Transaction_Value, Money_Transaction_Box.Notes AS Notes , Order_Money.Order_Notes AS Order_Notes

FROM            dbo.Submain_Account INNER JOIN
                         dbo.Banks ON dbo.Submain_Account.Id = dbo.Banks.SubAccount_Id INNER JOIN
                         dbo.Money_Transaction_Box ON dbo.Banks.id = dbo.Money_Transaction_Box.Bank_Id INNER JOIN
                         dbo.Order_Money ON dbo.Money_Transaction_Box.Order_Mony_Id = dbo.Order_Money.Id
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
                string Order_No = "";
                dt_Entry_Details = new DataTable();
                aa.Get_Empty_dt_Details(ref opp, ref dt_Entry_Details);
                int journal_Code = 0;
                for (int x = 0; x < dt_Order_Money.Rows.Count; x++)
                {
                    Order_Money_Id = long.Parse(dt_Order_Money.Rows[x]["Id"].ToString());
                    if (IS_Tar7eel_Sandouk_Automatic == 1)
                    {
                        long Money_Transaction_BoxId = long.Parse(dt_Order_Money.Rows[x]["Money_Transaction_BoxId"].ToString());
                        Transaction_Type_For_Sandouk = int.Parse(dt_Order_Money.Rows[x]["Transaction_type"].ToString());
                        Branch_Id_Entry = long.Parse(dt_Order_Money.Rows[x]["Branch_Id"].ToString());
                        Entry_Currency_ID = long.Parse(dt_Order_Money.Rows[x]["Currency_ID"].ToString());
                        dtprocess_Date = DateTime.Parse(dt_Order_Money.Rows[x]["Order_Date"].ToString());
                        EntryValue = decimal.Parse(dt_Order_Money.Rows[x]["Transaction_Value"].ToString());
                        Entry_Total = double.Parse(dt_Order_Money.Rows[x]["Transaction_Value"].ToString());
                        Equivalent_Value = decimal.Parse(dt_Order_Money.Rows[x]["Factor"].ToString());
                        Entry_Notes = dt_Order_Money.Rows[x]["Notes"].ToString();
                        //Entry_Notes = dt_Order_Money.Rows[x]["Money_Transaction_BoxNotes"].ToString();

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

                        mysql = @" SELECT        Submain_Account.Id, Submain_Account.Id As SubAccount_Id,Submain_Account.Main_Account_Id, Submain_Account.SubAccount_NO, Submain_Account.Code, Submain_Account.FirstSubCode, 
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
                         Cost_Center_Tree ON Money_Transaction_Box.Account_Cost_Center_Tree_Id = Cost_Center_Tree.Id ";

                        //mysql= mysql + " WHERE (Money_Transaction_Box.Order_Mony_Id = " + Order_Money_Id + ")";
                        mysql = mysql + " WHERE (Money_Transaction_Box.Id = " + Money_Transaction_BoxId + ")";


                        DataTable dt_Money_Transaction_Box = opp.return_DataTable(mysql);


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





                    }

                    //dt_Entry_Details.Clear();
                }/////انشاء القيد
                if (dt_Entry_Details.Rows.Count > 1)
                {
                    if (Entry_No == 0)
                    {
                        Entry_No = AccountEntry_CLS.NewEntry_No(ref opp, dtprocess_Date, journal_Code, Account_Entry_List_Id_);
                    }
                    long Journal_Code = Common2.Get_Journal_Code_ForSandouk(Transaction_Type_For_Sandouk);
                    int IS_Make_AutomaticFromTar7eel = 1;
                    aa.Reference_Type = Common.Sandouk_Account_Type;
                    aa.Reference_Type_Table_Id = Order_Money_Id;


                    Excp = 1;
                    aa.Make_AccountEntry(ref Excp, ref Msg, ref Account_Entry_List_Id_, ref opp, User_ID_Login, User_Id_Login_Name, Branch_ID_Login, Entry_No, dtprocess_Date,  Journal_Code, Entry_Currency_ID, Entry_Notes, Transaction_Type_ForAccount, dt_Entry_Details, Entry_Total, Branch_Id_Entry, IS_Make_AutomaticFromTar7eel, Order_No, Entry_Currency_ID, Entry_Currency_Name, Factor, Order_Money_Id);
                }
                if (Excp == 0)
                {
                    mysql = "UPDATE Order_Money SET  IS_Posted_Flage =1,Account_Entry_List_ID=" + Account_Entry_List_Id_ + " Where Id = " + Order_Money_Id;
                     opp.insert_me(mysql);

                    mysql = "UPDATE Money_Transaction_Box SET  IS_Posted_Flage =1,Account_Entry_List_ID=" + Account_Entry_List_Id_ + " Where Order_Mony_Id = " + Order_Money_Id;
                    opp.insert_me(mysql);


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
        public void Add_Row_Account_Details_FromAdd(ref AccountEntry_CLS aa, ref DataTable dt_Entry_Details, ref DataTable dt, int x, int Transaction_Type, long Entry_Currency_ID, long Branch_Id_Entry, decimal EntryValue, decimal Equivalent_Value)
        {
            //try
            //{


            //int x = 0;
            int Excp = 0;
            long Submain_Account_ID = long.Parse(dt.Rows[x]["SubAccount_Id"].ToString());
            long Id_Test = 0;
            try
            {
                Id_Test = long.Parse(dt.Rows[x]["Id_Test"].ToString());
            }
            catch { }
            int MadinDa2n = Transaction_Type;

            long CostCenter_Id = 0;
            try
            {
                CostCenter_Id = long.Parse(dt.Rows[x]["CostCenter_Id"].ToString());
            }
            catch { }
            string CostCenter_Name = "";
            if (CostCenter_Id > 0)
            {
                CostCenter_Name = dt.Rows[x]["Cost_Center_Name"].ToString();

            }
            else
            {
                CostCenter_Name = "لا يوجد";
            }
            int Account_Type = int.Parse(dt.Rows[x]["Account_Customer_Type_Value"].ToString());
            long MainAccount_Id = long.Parse(dt.Rows[x]["Main_Account_Id"].ToString());
            string SubAccount_Name = dt.Rows[x]["SubAccount_Name"].ToString();
            string MainAccount_Name = dt.Rows[x]["SubAccount_Name"].ToString();
            string Account_Code = dt.Rows[x]["Code"].ToString();
            long Account_Currency_ID = long.Parse(dt.Rows[x]["Account_Currency_ID"].ToString());
            decimal Factor = 1;

            try
            {
                Factor = decimal.Parse(dt.Rows[x]["Factor"].ToString());
            }
            catch (Exception ee)
            {


            }

            Equivalent_Value = Factor;

            //long CarsWorkOrdersId = 0;
            //long CarsId = 0;
            //long WorkOrderNumber = 0;
            //string CarNumber = "";

            //try
            //{
            //    //long CarsWorkOrdersId, long CarsId, long WorkOrderNumber,string CarNumber
            //    CarsWorkOrdersId = long.Parse(dt.Rows[x]["CarsWorkOrdersId"].ToString());
            //    CarsId = long.Parse(dt.Rows[x]["CarsId"].ToString());
            //    WorkOrderNumber = long.Parse(dt.Rows[x]["WorkOrderNumber"].ToString());
            //    CarNumber = dt.Rows[x]["CarNumber"].ToString();
            //}
            //catch { }
            string Notes_Details = dt.Rows[x]["Notes"].ToString();
            //long   Branch_Id_Entry = long.Parse(dt.Rows[x]["Account_Cost_Center_Tree_Id"].ToString());
            aa.AddRow_WhenBuild(ref Excp, ref dt_Entry_Details, Id_Test, Account_Code, Account_Type, MainAccount_Id, MainAccount_Name, Submain_Account_ID, SubAccount_Name, MadinDa2n, EntryValue, Notes_Details, Equivalent_Value, Account_Currency_ID, Entry_Currency_ID, CostCenter_Id, CostCenter_Name, Branch_Id_Entry, Factor);

            if (Excp == 1)
            { return; }



            //}
            //catch (Exception ee)
            //{
           
            //}
        }
        public void Add_Row_Account_Details_FromAdd(ref AccountEntry_CLS aa, ref DataTable dt_Entry_Details, int Transaction_Type, long    Entry_Currency_ID, long Branch_Id_Entry, decimal EntryValue, long Submain_Account_ID, long Id_Test, long CostCenter_Id, string CostCenter_Name, int Account_Type, long MainAccount_Id, string SubAccount_Name, string MainAccount_Name, string Account_Code, long Account_Currency_Id, decimal Factor, string Notes_Details)
        {
            //try
            //{

            //int x = 0;
            int Excp = 0;
            //= long.Parse(dt.Rows[x]["SubAccount_Id"].ToString());
            //long Id_Test = 0;
            //try
            //{
            //    Id_Test = long.Parse(dt.Rows[x]["Id_Test"].ToString());
            //}
            //catch { }



            aa.AddRow_WhenBuild(ref Excp, ref dt_Entry_Details, Id_Test, Account_Code, Account_Type, MainAccount_Id, MainAccount_Name, Submain_Account_ID, SubAccount_Name, Transaction_Type, EntryValue, Notes_Details, Factor, Account_Currency_Id, Entry_Currency_ID, CostCenter_Id, CostCenter_Name, Branch_Id_Entry, Factor);

            if (Excp == 1)
            { return; }



             
        }

        void MakeAccountPost_WithCurrency(ref AccountModelsLib.operations opp, long Account_Entry_Details_Id, long Account_Entry_Master_Id, decimal Import_Value, decimal Export_Value, decimal Equivalent_Import_Value, decimal Equivalent_Export_Value)
        {

            string mysql = "";

            //////////تعديل البيانات فى جدول الاستاذ والترحيل للقيم
            mysql = "UPDATE    Account_Entry_Master_Balance SET ";

            mysql = mysql + " First_Balance =First_Balance   ";
            mysql = mysql + ", First_Balance_Madin =First_Balance_Madin   ";

            mysql = mysql + ", First_Balance_Da2n =First_Balance_Da2n   ";

            mysql = mysql + ", Madin_Value =Madin_Value + " + Import_Value;
            mysql = mysql + ", Da2n_Value =Da2n_Value + " + Export_Value;

            mysql = mysql + ", Equivalent_Madin_Value =Equivalent_Madin_Value + " + Equivalent_Import_Value;
            mysql = mysql + ", Equivalent_Da2n_Value =Equivalent_Da2n_Value + " + Equivalent_Export_Value;


            decimal Total_First_Balance = First_Balance_Madin - First_Balance_Da2n;
            decimal Total_Madin_Value = Madin_Value + Import_Value;
            decimal Total_Da2n_Value = Da2n_Value + Export_Value;
            decimal New_Balance = Total_First_Balance + Total_Madin_Value - Total_Da2n_Value;


            decimal Total_Equivalent_First_Balance = Equivalent_First_Balance_Madin - Equivalent_First_Balance_Da2n;
            decimal Equivalent_Total_Madin_Value = Equivalent_Madin_Value + Equivalent_Import_Value;
            decimal Equivalent_Total_Da2n_Value = Equivalent_Da2n_Value + Equivalent_Export_Value;
            decimal New_Equivalent_Balance = Total_Equivalent_First_Balance + Equivalent_Total_Madin_Value - Equivalent_Total_Da2n_Value;


            decimal Balance_C = 0;
            decimal Balance_Madin_C = 0;
            decimal Balance_Da2n_C = 0;
            decimal Equivalent_Balance_Madin_C = 0;
            decimal Equivalent_Balance_Da2n_C = 0;



            if (New_Balance > 0)
            {


                Balance_C = New_Balance;
                Balance_Madin_C = New_Balance;
                Balance_Da2n_C = 0;
                Equivalent_Balance_Madin_C = New_Equivalent_Balance;
                Equivalent_Balance_Da2n_C = 0;

            }
            if (New_Balance < 0)
            {



                decimal New_Balance_A = New_Balance * -1;
                Balance_C = New_Balance;
                Balance_Madin_C = 0;
                Balance_Da2n_C = New_Balance_A;

                decimal New_Equivalent_Balance_A = New_Equivalent_Balance * -1;

                Equivalent_Balance_Madin_C = 0;
                Equivalent_Balance_Da2n_C = New_Equivalent_Balance_A;

            }
            if (New_Balance == 0)
            {
                Balance_C = 0;
                Balance_Madin_C = 0;
                Balance_Da2n_C = 0;
                Equivalent_Balance_Madin_C = 0;
                Equivalent_Balance_Da2n_C = 0;
            }
            mysql = mysql + ", Balance =" + Balance_C + "";
            mysql = mysql + ", Balance_Madin =" + Balance_Madin_C + "";
            mysql = mysql + ", Balance_Da2n = " + Balance_Da2n_C + " ";


            mysql = mysql + ", Equivalent_Balance_Madin =" + Equivalent_Balance_Madin_C + "";
            mysql = mysql + ", Equivalent_Balance_Da2n = " + Equivalent_Balance_Da2n_C + " ";
            mysql = mysql + " WHERE     (Id = " + Account_Entry_Master_Id + ")";

            opp.insert_me(mysql);

            ////////////////////////////Update Maset Transaction

            mysql = "  UPDATE  Account_Entry_Details SET  Balance_Import_Value =" + Balance_Madin_C + ", Balance_Export_Value =" + Balance_Da2n_C + ", Equivalent_Balance_Import_Value =" + Equivalent_Balance_Madin_C + ", Equivalent_Balance_Export_Value =" + Equivalent_Balance_Da2n_C + " ";
            mysql = mysql + " WHERE  (Id = " + Account_Entry_Details_Id + ")";
            opp.insert_me(mysql);
        }

        long Search_Account_In_Master(ref AccountModelsLib.operations opp, long Year_Financial_Period_Id, long Submain_Account_ID, string SubAccount_Name, long Branch_Id, long Account_Currency_ID)
        {
            //decimal Equivalent_Madin_Value, ref decimal Equivalent_Da2n_Value, ref decimal Equivalent_Balance_Madin, ref decimal Equivalent_Balance_Da2n
            /////////////يجب الا يتكرر وضع الحساب فى الجدول لكل من
            ///////////// Primary key :Year_Financial_Period_Id,   Submain_Account_ID,   Branch_Id
            long Account_Entry_Master_Id = 0;
            string mysql = @"SELECT  * FROM  Account_Entry_Master_Balance
WHERE     (Submain_Account_ID = " + Submain_Account_ID + ") AND (Year_Financial_Period_Id = " + Year_Financial_Period_Id + ") AND (Branch_Id = " + Branch_Id + ") AND (Account_Currency_ID = " + Account_Currency_ID + ")";

            DataTable dt = opp.return_DataTable(mysql);
            if (dt.Rows.Count == 0)
            {
                // لا تكرار لمفتاح جدول الأرصدة (الحساب، الفترة، الفرع، Equivalent_Account_Currency_Id): إن وُجد صف بعملة المكافىء التى ستُضاف يُستعمل ويُعدَّل بدل إضافة صف ثانٍ
                dt = opp.return_DataTable(@"SELECT  * FROM  Account_Entry_Master_Balance WHERE (Submain_Account_ID = " + Submain_Account_ID + ") AND (Year_Financial_Period_Id = " + Year_Financial_Period_Id + ") AND (Branch_Id = " + Branch_Id + ") AND (Equivalent_Account_Currency_Id = " + Common.Entry_Currency_Id + ")");
            }
            if (dt.Rows.Count == 0)  //الحساب غير موجود فى جدول الاستاذ ويجب اضافتة
            {
                mysql = "INSERT INTO Account_Entry_Master_Balance(Submain_Account_ID,SubAccount_Name, Year_Financial_Period_Id, Branch_Id,Account_Currency_Id,Equivalent_Account_Currency_Id, First_Balance, First_Balance_Madin, First_Balance_Da2n, Madin_Value, Da2n_Value, Balance, Balance_Madin, Balance_Da2n)";
                mysql = mysql + " VALUES     (" + Submain_Account_ID + ",'" + SubAccount_Name + "', " + Year_Financial_Period_Id + ", " + Branch_Id + ", " + Account_Currency_ID + "," + Common.Entry_Currency_Id + ", 0, 0, 0, 0, 0, 0, 0, 0)";
                Account_Entry_Master_Id = opp.Insert_me_Return_Last_Identify(mysql);


                First_Balance = 0;
                First_Balance_Madin = 0;
                First_Balance_Da2n = 0;
                Madin_Value = 0;
                Da2n_Value = 0;
                Balance = 0;
                Balance_Madin = 0;
                Balance_Da2n = 0;
                Equivalent_First_Balance_Madin = 0;
                Equivalent_First_Balance_Da2n = 0;
                Equivalent_Madin_Value = 0;
                Equivalent_Da2n_Value = 0;
                Equivalent_Balance_Madin = 0;
                Equivalent_Balance_Da2n = 0;


            }
            else
            {
                Account_Entry_Master_Id = long.Parse(dt.Rows[0]["Id"].ToString());
                First_Balance = decimal.Parse(dt.Rows[0]["First_Balance"].ToString());
                First_Balance_Madin = decimal.Parse(dt.Rows[0]["First_Balance_Madin"].ToString());
                First_Balance_Da2n = decimal.Parse(dt.Rows[0]["First_Balance_Da2n"].ToString());
                Madin_Value = decimal.Parse(dt.Rows[0]["Madin_Value"].ToString());
                Da2n_Value = decimal.Parse(dt.Rows[0]["Da2n_Value"].ToString());
                Balance = decimal.Parse(dt.Rows[0]["Balance"].ToString());
                Balance_Madin = decimal.Parse(dt.Rows[0]["Balance_Madin"].ToString());
                Balance_Da2n = decimal.Parse(dt.Rows[0]["Balance_Da2n"].ToString());

                Equivalent_Madin_Value = decimal.Parse(dt.Rows[0]["Equivalent_Madin_Value"].ToString());
                Equivalent_Da2n_Value = decimal.Parse(dt.Rows[0]["Equivalent_Da2n_Value"].ToString());

                Equivalent_Balance_Madin = decimal.Parse(dt.Rows[0]["Equivalent_Balance_Madin"].ToString());
                Equivalent_Balance_Da2n = decimal.Parse(dt.Rows[0]["Equivalent_Balance_Da2n"].ToString());


                Equivalent_First_Balance_Madin = decimal.Parse(dt.Rows[0]["Equivalent_First_Balance_Madin"].ToString());
                Equivalent_First_Balance_Da2n = decimal.Parse(dt.Rows[0]["Equivalent_First_Balance_Da2n"].ToString());


            }
            return Account_Entry_Master_Id;
        }



    }
}
