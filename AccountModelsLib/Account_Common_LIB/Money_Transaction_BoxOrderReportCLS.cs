
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using AccountModelsLib;
using AccountModelsLib.Models;
using System.Collections.Generic;
using System.Linq;
 
using System.CodeDom;
namespace AccountModelsLib
{


    public class Money_Transaction_BoxOrderReportCLS
    {
        //public List<BanksListViewModel> banksListViewModels = new List<BanksListViewModel>();
        public List<Money_Transaction_Box> mbxList = new List<Money_Transaction_Box>();
        public Money_Transaction_Box mbx = new Money_Transaction_Box();
        //public Order_Money om = new Order_Money();
        public Order_Money omMain = new Order_Money();
        public long Customer_ID_Supplier = 0;
        public int Reverse_Order = 0;
        public operations opp;
        long SubAccount_Id_ { get; set; } = 0;
        string SubAccountName = "";
        public string Notes = "";
        public decimal Factor = 1;

        public decimal Balance_Before = 0;
        public decimal Balance_After = 0;
        public string Balance_BeforStr = "";
        public string Balance_AfterStr = "";

        public decimal T_FirstBalance_All { get; set; } = 0;
        public decimal T_Export_Value_Total_All = 0;
        public decimal T_Import_Value_Total_All = 0;
        public decimal T_FinalBalance_All = 0;
        public bool GetFirstBalance_FirstValue = false;
        public decimal FirstBalance_FirstValue = 0;

        public Money_Transaction_BoxOrderReportCLS()
        {
            opp = new operations();
        }
        public Money_Transaction_BoxOrderReportCLS(ref operations opp_)
        {
            opp = opp_;
        }
        public long Invoice_ID = 0;
        //public long CarsWorkOrdersId { get; set; } = 0;

        public int Invoice_Type = 0;
        public Order_Money Get_Order_MoneyById(long Order_MoneyId)
        {
            Order_MoneyCLS omCls = new Order_MoneyCLS(ref opp);
            omMain = omCls.Get_Order_MoneyById(Order_MoneyId);
            return omMain;
        }
        public List<Money_Transaction_Box> Get_Money_Transaction_BoxList(long Order_MoneyId)
        {
            Money_Transaction_BoxCLS omCls = new Money_Transaction_BoxCLS(ref opp);
            mbxList = omCls.Get_Money_Transaction_BoxList(Order_MoneyId);
            return mbxList;
        }


        public static DataTable MakeEmpty_OrderMoney()
        {
            DataTable dt;
            string mysql;

            mysql = " SELECT 0 AS Id_Test , Money_Transaction_Box.* From Money_Transaction_Box Where Id=-100";



            operations opp = new operations();
            dt = opp.return_DataTable(mysql);

            return dt;

        }
        public DataTable Get_Order_Data(long Order_Money_Id)
        {



            string mysql = @" SELECT   0 AS   Id_Test,   Money_Transaction_Box.*, Order_Money.Id AS Order_Money_Id, Order_Money.Order_No, Order_Money.Order_Date, 
                         Order_Money.Transaction_type AS Order_Money_Transaction_type, Order_Money.Total_Value, Order_Money.Currency_Id AS Order_Money_Currency_Id, 
                         Order_Money.Bank_Type, Order_Money.Bank_ID AS Order_Money_Bank_ID, Order_Money.Man_Name, Order_Money.Is_Balances_Set, 
                         Order_Money.Day_Registration_flage, Order_Money.DayReg_List_Id, Order_Money.Order_mony_id_transform AS Order_Money_Order_mony_id_transform, 
                         Order_Money.Invoice_Id, Order_Money.User_Id AS Order_Money_User_Id, Order_Money.Branch_Id AS Order_Money_Branch_Id, 
                         Order_Money.Year_Financial_Period_Id, Order_Money.Ticket_Supplier_Id, Order_Money.Order_Tafkeet_Value, Order_Money.Order_Notes, 
                         Order_Money.Tafkeet_Value AS Order_Money_Tafkeet_Value, Order_Money.Sandouk_Cost_Center_Tree_Id, Order_Money.IS_Posted_Flage ,Order_Money.Account_Entry_List_ID 
FROM            Order_Money INNER JOIN
                         Money_Transaction_Box ON Order_Money.Id = Money_Transaction_Box.Order_Mony_Id
WHERE        (Order_Money.Id = " + Order_Money_Id + ") ";

            operations opp = new operations();
            DataTable dt = opp.return_DataTable(mysql);
            return dt;


        }
        public long Make_New_Order_No(ref int Excp, int Transaction_type)
        {
            operations opp = new operations();
            return Make_New_Order_No(ref Excp, ref opp, Transaction_type);
        }

        public long Make_New_Order_No(ref int Excp, ref operations opp, int Transaction_type)
        {
            DataSet ds = null;
            try
            {
                long Order_No;
                string mysql = " SELECT MAX(Order_No) AS Order_No FROM Order_Money GROUP BY Bank_Type, Transaction_type HAVING (Bank_Type = 0) AND (Transaction_type = " + Transaction_type + ")  ";

                ds = opp.return_dataset(ref Excp, mysql, "Order_Money");
                if (ds.Tables[0].Rows.Count > 0)
                {
                    if (ds.Tables[0].Rows[0]["Order_No"] == null)
                    {
                        Order_No = 1;
                    }
                    else
                    {
                        Order_No = long.Parse(ds.Tables[0].Rows[0]["Order_No"].ToString()) + 1;
                    }
                }
                else
                {
                    Order_No = 1;
                }
                //opp = null;
                Excp = 0;

                return Order_No;

            }
            catch (Exception e)
            {
                Excp = 1;
                throw (e);

            }
        }

        public long Make_New_Order_No(ref int Excp, int Transaction_type, DateTime dt_Order, long Bank_Id)
        {
            long Order_No = 0;
            operations opp = new operations();
            Order_No = Make_New_Order_No(ref Excp, ref opp, Transaction_type, dt_Order, Bank_Id);
            return Order_No;
        }
        public long Make_New_Order_No(ref int Excp, ref operations opp, int Transaction_type, DateTime dt_Order, long Bank_Id)
        {
            long Year_Financial_Period_Id = Financial_PeriodCLS.Get_Year_Financial_Period_Id(ref opp, dt_Order);
            DataTable dt = new DataTable();
            long Order_No = 1;
            try
            {

                // AND (Bank_Id=" + Bank_Id + ")
                //string mysql = " SELECT MAX(Order_No) AS Order_No FROM Order_Money GROUP BY Bank_Id,Bank_Type, Transaction_type ,Year_Financial_Period_Id HAVING (Bank_Type = 0) AND (Transaction_type = " + Transaction_type + ")  AND (Year_Financial_Period_Id=" + Year_Financial_Period_Id + ")";
                string mysql = @" SELECT        MAX(Order_No) AS Order_No
FROM            Order_Money
WHERE       (Bank_Type = 0) AND  (Transaction_type =  " + Transaction_type + ") AND (Year_Financial_Period_Id =  " + Year_Financial_Period_Id + ") ";

                dt = opp.return_DataTable(mysql);
                if (dt.Rows.Count > 0)
                {
                    if (dt.Rows[0]["Order_No"].ToString() == "")
                    {
                        Order_No = 1;
                    }
                    else
                    {
                        Order_No = long.Parse(dt.Rows[0]["Order_No"].ToString()) + 1;
                    }
                }
                else
                {
                    Order_No = 1;
                }


            }
            catch (Exception e)
            {
                Excp = 1;

            }
            return Order_No;
        }

        public bool Check_Order_NoFound(ref int Excp, ref operations opp, ref long Order_No, int Transaction_type, DateTime dt_Order, long Bank_Id)
        {
            long Year_Financial_Period_Id = Financial_PeriodCLS.Get_Year_Financial_Period_Id(ref opp, dt_Order);
            DataTable dt = new DataTable();

            try
            {

                // AND (Bank_Id=" + Bank_Id + ")
                //string mysql = " SELECT MAX(Order_No) AS Order_No FROM Order_Money GROUP BY Bank_Id,Bank_Type, Transaction_type ,Year_Financial_Period_Id HAVING (Bank_Type = 0) AND (Transaction_type = " + Transaction_type + ")  AND (Year_Financial_Period_Id=" + Year_Financial_Period_Id + ")";
                string mysql = @" SELECT         Order_No 
FROM            Order_Money
WHERE      (Order_No=  " + Order_No + ") And   (Transaction_type =  " + Transaction_type + ") AND (Year_Financial_Period_Id =  " + Year_Financial_Period_Id + ") ";

                dt = opp.return_DataTable(mysql);
                if (dt.Rows.Count > 0)
                {

                    Order_No = Order_No + 1;
                    return Check_Order_NoFound(ref Excp, ref opp, ref Order_No, Transaction_type, dt_Order, Bank_Id);
                    //return true;
                }
                else
                {
                    return false;
                }


            }
            catch (Exception e)
            {
                Excp = 1;
                return false;
            }

        }


        public long GetDefaultBank_ID()
        {
            operations opp = new operations();
            return GetDefaultBank_ID(ref opp);
        }
        public long GetDefaultBank_ID(ref operations opp)
        {
            string mysql = "Select * from Banks Where Default_Bank=1 and Is_Bank=0";

            DataTable dt = opp.return_DataTable(mysql);
            long Bank_Id = 0;
            if (dt.Rows.Count > 0)
            {
                Bank_Id = long.Parse(dt.Rows[0]["Id"].ToString());
            }
            return Bank_Id;
        }

        public Bank GetDefaultBank(ref operations opp)
        {
            Bank bank = new Bank();
            bank = opp.db.Banks.AsNoTracking().Where(b => b.Is_Bank == 0 & b.Default_Bank == 1).FirstOrDefault();
            return bank;
            //string mysql = "Select * from Banks Where Default_Bank=1 and Is_Bank=0";

            //DataTable dt = opp.return_DataTable(mysql);
            //long  Bank_Id = 0;
            //if (dt.Rows.Count > 0)
            //{
            //    Bank_Id = long.Parse(dt.Rows[0]["Id"].ToString());
            //}
            //return Bank_Id;
        }

        public void Get_Money_Box_Transaction_PeriodTotalPerDay(ref int Excp, ref string msg, ref DataTable dtTotal, long bank_Id, DateTime DateFrom, DateTime DateTo)
        {
            T_FirstBalance_All = 0;
            T_Export_Value_Total_All = 0;
            T_Import_Value_Total_All = 0;
            T_FinalBalance_All = 0;
            GetFirstBalance_FirstValue = false;
            try
            {
                DataTable dt = new DataTable();
                dtTotal = new DataTable();
                bool Interval = true;
                decimal FirstBalance = 0;
                decimal Export_Value_Total = 0;
                decimal Import_Value_Total = 0;
                decimal FinalBalance = 0;

                decimal T_FirstBalance = 0;
                decimal T_Export_Value_Total = 0;
                decimal T_Import_Value_Total = 0;
                decimal T_FinalBalance = 0;

                DataColumn d11 = new DataColumn("DayDate", typeof(String));
                dtTotal.Columns.Add(d11);
                DataColumn d12 = new DataColumn("FirstBalance", typeof(String));
                dtTotal.Columns.Add(d12);
                DataColumn d13 = new DataColumn("Export_Value_Total", typeof(String));
                dtTotal.Columns.Add(d13);
                DataColumn d14 = new DataColumn("Import_Value_Total", typeof(String));
                dtTotal.Columns.Add(d14);
                DataColumn d15 = new DataColumn("FinalBalance", typeof(String));
                dtTotal.Columns.Add(d15);


                TimeSpan tt = new TimeSpan();

                do
                {



                    Get_Money_Box_Transaction_Period(ref Excp, ref msg, ref dt, bank_Id, Interval, DateFrom, DateFrom, ref FirstBalance, ref Export_Value_Total, ref Import_Value_Total, ref FinalBalance);



                    if (Import_Value_Total == 0 & Export_Value_Total == 0)
                    {
                        tt = new TimeSpan(1, 0, 0, 0);
                        DateFrom = DateFrom.Add(tt);
                        continue;
                    }

                    if (GetFirstBalance_FirstValue == false)
                    {
                        GetFirstBalance_FirstValue = true;
                        FirstBalance_FirstValue = FirstBalance;
                    }
                    T_FirstBalance = T_FirstBalance + FirstBalance;
                    T_Export_Value_Total = T_Export_Value_Total + Export_Value_Total;
                    T_Import_Value_Total = T_Import_Value_Total + Import_Value_Total;
                    T_FinalBalance = T_FinalBalance + FinalBalance;


                    DataRow dr = dtTotal.NewRow();
                    dr["FirstBalance"] = FirstBalance;
                    dr["Export_Value_Total"] = Export_Value_Total;
                    dr["Import_Value_Total"] = Import_Value_Total;
                    dr["FinalBalance"] = FinalBalance;
                    dr["DayDate"] = DateFrom.ToString("yyyy/MM/dd");
                    dtTotal.Rows.Add(dr);

                    tt = new TimeSpan(1, 0, 0, 0);
                    DateFrom = DateFrom.Add(tt);
                }
                while (DateFrom <= DateTo);

                Common.Make_TableColumnSerial(ref dtTotal, "Serial_");

                DataRow dr11 = dtTotal.NewRow();
                dtTotal.Rows.Add(dr11);


                DataRow dr1 = dtTotal.NewRow();
                dr1["FirstBalance"] = T_FirstBalance;
                dr1["Export_Value_Total"] = T_Export_Value_Total;
                dr1["Import_Value_Total"] = T_Import_Value_Total;
                dr1["FinalBalance"] = T_FinalBalance;
                dr1["DayDate"] = "الإجمالى";
                dtTotal.Rows.Add(dr1);


                T_FirstBalance_All = T_FirstBalance;
                T_Export_Value_Total_All = T_Export_Value_Total;
                T_Import_Value_Total_All = T_Import_Value_Total;
                T_FinalBalance_All = T_FinalBalance;


            }
            catch (Exception ee)
            {


            }

        }


        public void Get_Money_Box_Transaction_PeriodTotalPerDay_(ref int Excp, ref string msg, ref DataTable dtTotal, long bank_Id, DateTime DateFrom, DateTime DateTo)
        {
            T_FirstBalance_All = 0;
            T_Export_Value_Total_All = 0;
            T_Import_Value_Total_All = 0;
            T_FinalBalance_All = 0;
            GetFirstBalance_FirstValue = false;
            try
            {
                DataTable dt = new DataTable();
                dtTotal = new DataTable();
                bool Interval = true;
                decimal FirstBalance = 0;
                decimal Export_Value_Total = 0;
                decimal Import_Value_Total = 0;
                decimal FinalBalance = 0;

                decimal T_FirstBalance = 0;
                decimal T_Export_Value_Total = 0;
                decimal T_Import_Value_Total = 0;
                decimal T_FinalBalance = 0;

                DataColumn d11 = new DataColumn("DayDate", typeof(String));
                dtTotal.Columns.Add(d11);
                DataColumn d12 = new DataColumn("FirstBalance", typeof(String));
                dtTotal.Columns.Add(d12);
                DataColumn d13 = new DataColumn("Export_Value_Total", typeof(String));
                dtTotal.Columns.Add(d13);
                DataColumn d14 = new DataColumn("Import_Value_Total", typeof(String));
                dtTotal.Columns.Add(d14);
                DataColumn d15 = new DataColumn("FinalBalance", typeof(String));
                dtTotal.Columns.Add(d15);


                TimeSpan tt = new TimeSpan();

                do
                {



                    Get_Money_Box_Transaction_Period(ref Excp, ref msg, ref dt, bank_Id, Interval, DateFrom, DateFrom, ref FirstBalance, ref Export_Value_Total, ref Import_Value_Total, ref FinalBalance);



                    if (Import_Value_Total == 0 & Export_Value_Total == 0)
                    {
                        tt = new TimeSpan(1, 0, 0, 0);
                        DateFrom = DateFrom.Add(tt);
                        continue;
                    }

                    if (GetFirstBalance_FirstValue == false)
                    {
                        GetFirstBalance_FirstValue = true;
                        FirstBalance_FirstValue = FirstBalance;
                    }
                    T_FirstBalance = T_FirstBalance + FirstBalance;
                    T_Export_Value_Total = T_Export_Value_Total + Export_Value_Total;
                    T_Import_Value_Total = T_Import_Value_Total + Import_Value_Total;
                    T_FinalBalance = T_FinalBalance + FinalBalance;


                    DataRow dr = dtTotal.NewRow();
                    dr["FirstBalance"] = FirstBalance;
                    dr["Export_Value_Total"] = Export_Value_Total;
                    dr["Import_Value_Total"] = Import_Value_Total;
                    dr["FinalBalance"] = FinalBalance;
                    dr["DayDate"] = DateFrom.ToString("yyyy/MM/dd");
                    dtTotal.Rows.Add(dr);

                    tt = new TimeSpan(1, 0, 0, 0);
                    DateFrom = DateFrom.Add(tt);
                }
                while (DateFrom <= DateTo);

                Common.Make_TableColumnSerial(ref dtTotal, "Serial_");

                DataRow dr11 = dtTotal.NewRow();
                dtTotal.Rows.Add(dr11);


                DataRow dr1 = dtTotal.NewRow();
                dr1["FirstBalance"] = T_FirstBalance;
                dr1["Export_Value_Total"] = T_Export_Value_Total;
                dr1["Import_Value_Total"] = T_Import_Value_Total;
                dr1["FinalBalance"] = T_FinalBalance;
                dr1["DayDate"] = "الإجمالى";
                dtTotal.Rows.Add(dr1);


                T_FirstBalance_All = T_FirstBalance;
                T_Export_Value_Total_All = T_Export_Value_Total;
                T_Import_Value_Total_All = T_Import_Value_Total;
                T_FinalBalance_All = T_FinalBalance;


            }
            catch (Exception ee)
            {


            }

        }


        public void Get_Money_Box_Transaction_PeriodTotalPerDay(ref int Excp, ref string msg, ref DataTable dtTotal, ArrayList Arrbank, bool Interval, DateTime DateFrom, DateTime DateTo, int TafseleyTotaly)
        {
            T_FirstBalance_All = 0;
            T_Export_Value_Total_All = 0;
            T_Import_Value_Total_All = 0;
            T_FinalBalance_All = 0;
            GetFirstBalance_FirstValue = false;
            try
            {
                //DataTable dt = new DataTable();
                dtTotal = new DataTable();



                decimal T_FirstBalance = 0;
                decimal T_Export_Value_Total = 0;
                decimal T_Import_Value_Total = 0;
                decimal T_FinalBalance = 0;

                DataColumn d11 = new DataColumn("DayDate", typeof(String));
                dtTotal.Columns.Add(d11);

                DataColumn d16 = new DataColumn("Bank_Name", typeof(String));
                dtTotal.Columns.Add(d16);

                DataColumn d12 = new DataColumn("FirstBalance", typeof(String));
                dtTotal.Columns.Add(d12);
                DataColumn d13 = new DataColumn("Export_Value_Total", typeof(String));
                dtTotal.Columns.Add(d13);
                DataColumn d14 = new DataColumn("Import_Value_Total", typeof(String));
                dtTotal.Columns.Add(d14);
                DataColumn d15 = new DataColumn("FinalBalance", typeof(String));
                dtTotal.Columns.Add(d15);


                TimeSpan tt = new TimeSpan();
                foreach (long bank_Id in Arrbank)
                {
                    DateTime datefrom = new DateTime(DateFrom.Year, DateFrom.Month, DateFrom.Day);
                    DateTime dateto = new DateTime(DateTo.Year, DateTo.Month, DateTo.Day);




                    string Bank_Name = "";
                    Bank bank = new Bank();
                    bank = opp.db.Banks.AsNoTracking().Where(b => b.Id == bank_Id).FirstOrDefault();
                    if (bank != null)
                    {
                        Bank_Name = bank.Bank_Name;
                    }
                    else
                    {
                        Bank_Name = "";
                    }

                    //////////////////
                    ///
                    decimal FirstBalance = 0;
                    decimal Export_Value_Total = 0;
                    decimal Import_Value_Total = 0;
                    decimal FinalBalance = 0;

                    //if (datefrom.Date == dateto.Date)
                    //{
                    //    Interval = false;
                    //}
                    //if (datefrom.Date < dateto.Date)
                    //{
                    //    Interval = true ;
                    //}

                    if (TafseleyTotaly == 1)
                    {
                        Interval = true;
                    }
                    else
                    {
                        Interval = false;
                    }
                    if (TafseleyTotaly == 1)
                    {
                        FirstBalance = 0;
                        Export_Value_Total = 0;
                        Import_Value_Total = 0;
                        FinalBalance = 0;

                        GetBankTrasactionByDate(ref Excp, ref msg, ref dtTotal, bank_Id, Bank_Name, Interval, datefrom, dateto, ref FirstBalance, ref Export_Value_Total, ref Import_Value_Total, ref FinalBalance);

                        T_FirstBalance = T_FirstBalance + FirstBalance;
                        T_Export_Value_Total = T_Export_Value_Total + Export_Value_Total;
                        T_Import_Value_Total = T_Import_Value_Total + Import_Value_Total;
                        T_FinalBalance = T_FinalBalance + FinalBalance;

                    }
                    else
                    {
                        do
                        {
                            FirstBalance = 0;
                            Export_Value_Total = 0;
                            Import_Value_Total = 0;
                            FinalBalance = 0;

                            GetBankTrasactionByDate(ref Excp, ref msg, ref dtTotal, bank_Id, Bank_Name, Interval, datefrom, dateto, ref FirstBalance, ref Export_Value_Total, ref Import_Value_Total, ref FinalBalance);



                            T_FirstBalance = T_FirstBalance + FirstBalance;
                            T_Export_Value_Total = T_Export_Value_Total + Export_Value_Total;
                            T_Import_Value_Total = T_Import_Value_Total + Import_Value_Total;
                            T_FinalBalance = T_FinalBalance + FinalBalance;


                            datefrom = datefrom.AddDays(1);
                        }
                        while (datefrom <= dateto);
                    }


                }
                Common.Make_TableColumnSerial(ref dtTotal, "Serial_");

                //DataRow dr11 = dtTotal.NewRow();
                //dtTotal.Rows.Add(dr11);


                //DataRow dr1 = dtTotal.NewRow();

                //dr1["FirstBalance"] = T_FirstBalance;
                //dr1["Export_Value_Total"] = T_Export_Value_Total;
                //dr1["Import_Value_Total"] = T_Import_Value_Total;
                //T_FinalBalance = T_FirstBalance + T_Import_Value_Total - T_Export_Value_Total;
                //dr1["FinalBalance"] = T_FinalBalance;
                //dr1["Bank_Name"] = "الإجمالى";
                //dtTotal.Rows.Add(dr1);


                //T_FirstBalance_All = T_FirstBalance;
                //T_Export_Value_Total_All = T_Export_Value_Total;
                //T_Import_Value_Total_All = T_Import_Value_Total;
                //T_FinalBalance_All = T_FinalBalance;

            }
            catch (Exception ee)
            {


            }

        }

        void GetBankTrasactionByDate(ref int Excp, ref string msg, ref DataTable dtTotal, long bank_Id, string Bank_Name, bool Interval, DateTime DateFrom, DateTime DateTo, ref decimal FirstBalance, ref decimal Export_Value_Total, ref decimal Import_Value_Total, ref decimal FinalBalance)
        {
            FirstBalance = 0;
            Export_Value_Total = 0;
            Import_Value_Total = 0;
            FinalBalance = 0;
            Get_Money_Box_Transaction_Period(ref Excp, ref msg, bank_Id, Interval, DateFrom, DateTo, ref FirstBalance, ref Export_Value_Total, ref Import_Value_Total, ref FinalBalance);


            DataRow dr = dtTotal.NewRow();
            dr["Bank_Name"] = Bank_Name;
            dr["FirstBalance"] = FirstBalance;
            dr["Export_Value_Total"] = Export_Value_Total;
            dr["Import_Value_Total"] = Import_Value_Total;
            dr["FinalBalance"] = FinalBalance;
            dr["DayDate"] = DateFrom.ToString("yyyy/MM/dd");
            dtTotal.Rows.Add(dr);



        }
        public void Get_Money_Box_Transaction_Period(ref int Excp, ref string msg, ref DataTable dt, long bank_Id, bool Interval, DateTime DateFrom, DateTime DateTo, ref decimal FirstBalance, ref decimal Export_Value_Total, ref decimal Import_Value_Total, ref decimal FinalBalance)
        {

            // DataTable dtFirstBalance = new DataTable();
            //DataTable dtPrev_Period = new DataTable();
            Export_Value_Total = 0;
            Import_Value_Total = 0;
            try
            {
                Excp = 0;


                FirstBalance = Compute_Box_BalanceBeforDataDay(bank_Id, false, DateFrom);
                FirstBalance = Common.RoundMe(FirstBalance);
                dt = Money_Box_Transaction(ref Excp, bank_Id, DateFrom, DateTo, Interval);

                if (dt.Rows.Count == 0)
                {
                    msg = "لا توجد نتيجة للبحث";
                    Excp = 1;
                    return;
                }

                DateTime datFirstProcess = DateTime.Parse(dt.Rows[0]["Transaction_Date"].ToString());


                decimal Balance = FirstBalance;


                for (int x = 0; x < dt.Rows.Count; x++)
                {

                    decimal Export_Value = 0;
                    decimal Import_Value = 0;

                    try
                    {
                        Export_Value = decimal.Parse(dt.Rows[x]["Export_Value"].ToString());
                    }
                    catch { }

                    try
                    {
                        Import_Value = decimal.Parse(dt.Rows[x]["Import_Value"].ToString());
                    }
                    catch { }
                    Balance = Common.RoundMe(Balance);
                    Import_Value = Common.RoundMe(Import_Value);
                    Export_Value = Common.RoundMe(Export_Value);

                    Balance = Balance + Import_Value - Export_Value;
                    if (Balance < 0)
                    {
                        Balance = 0;
                    }
                    dt.Rows[x]["Bank_Balance"] = Balance;

                    try
                    {
                        Export_Value_Total = Export_Value_Total + decimal.Parse(dt.Rows[x]["Export_Value"].ToString());
                    }
                    catch { }

                    try
                    {
                        Import_Value_Total = Import_Value_Total + decimal.Parse(dt.Rows[x]["Import_Value"].ToString());
                    }
                    catch { }
                }

                Export_Value_Total = Common.RoundMe(Export_Value_Total);
                Import_Value_Total = Common.RoundMe(Import_Value_Total);
                FirstBalance = Common.RoundMe(FirstBalance);
                //////Add FirstBalance
                DataRow dr = dt.NewRow();
                dr["Bank_Balance"] = FirstBalance;
                dr["Notes"] = "الرصيد الإفتتاحى";
                dt.Rows.InsertAt(dr, 0);


                ////اضافة اجمالى الصادر والوارد
                //Export_Value_Total = Common.RoundMe(Export_Value_Total);
                //Import_Value_Total = Common.RoundMe(Import_Value_Total);

                DataRow dr2 = dt.NewRow();
                dr2["Import_Value"] = Import_Value_Total;
                dr2["Export_Value"] = Export_Value_Total;
                dr2["Notes"] = "إجمالى الحركة  ";
                dt.Rows.Add(dr2);
                //// اضافة الصافى
                decimal Day_Diff = FirstBalance + Import_Value_Total - Export_Value_Total;

                Day_Diff = Common.RoundMe(Day_Diff);
                FinalBalance = Day_Diff;
                DataRow dr3 = dt.NewRow();
                dr3["Bank_Balance"] = Day_Diff;

                dr3["Notes"] = "صافى الرصيد ";
                dt.Rows.Add(dr3);
            }
            catch (Exception ee)
            {
                msg = ee.Message;
                Excp = 1;


                return;

            }
        }

        public void Get_Money_Box_Transaction_Period(ref int Excp, ref string msg, long bank_Id, bool Interval, DateTime DateFrom, DateTime DateTo, ref decimal FirstBalance, ref decimal Export_Value_Total, ref decimal Import_Value_Total, ref decimal FinalBalance)
        {

            // DataTable dtFirstBalance = new DataTable();
            //DataTable dtPrev_Period = new DataTable();
            Export_Value_Total = 0;
            Import_Value_Total = 0;
            try
            {
                Excp = 0;


                FirstBalance = Compute_Box_BalanceBeforDataDay(bank_Id, false, DateFrom);
                FirstBalance = Common.RoundMe(FirstBalance);
                DataTable dt = Money_Box_Transaction(ref Excp, bank_Id, DateFrom, DateTo, Interval);



                //DateTime datFirstProcess = DateTime.Parse(dt.Rows[0]["Transaction_Date"].ToString());


                decimal Balance = FirstBalance;


                for (int x = 0; x < dt.Rows.Count; x++)
                {

                    decimal Export_Value = 0;
                    decimal Import_Value = 0;

                    try
                    {
                        Export_Value = decimal.Parse(dt.Rows[x]["Export_Value"].ToString());
                    }
                    catch { }

                    try
                    {
                        Import_Value = decimal.Parse(dt.Rows[x]["Import_Value"].ToString());
                    }
                    catch { }
                    Balance = Common.RoundMe(Balance);
                    Import_Value = Common.RoundMe(Import_Value);
                    Export_Value = Common.RoundMe(Export_Value);

                    Balance = Balance + Import_Value - Export_Value;
                    if (Balance < 0)
                    {
                        Balance = 0;
                    }
                    dt.Rows[x]["Bank_Balance"] = Balance;

                    try
                    {
                        Export_Value_Total = Export_Value_Total + decimal.Parse(dt.Rows[x]["Export_Value"].ToString());
                    }
                    catch { }

                    try
                    {
                        Import_Value_Total = Import_Value_Total + decimal.Parse(dt.Rows[x]["Import_Value"].ToString());
                    }
                    catch { }
                }

                Export_Value_Total = Common.RoundMe(Export_Value_Total);
                Import_Value_Total = Common.RoundMe(Import_Value_Total);
                FirstBalance = Common.RoundMe(FirstBalance);
                //////Add FirstBalance

                //// اضافة الصافى
                FinalBalance = FirstBalance + Import_Value_Total - Export_Value_Total;


            }
            catch (Exception ee)
            {
                msg = ee.Message;
                Excp = 1;


                return;

            }
        }
        public void Get_Money_Box_Transaction_PeriodFromInvoiceBills(ref int Excp, ref string msg, ref DataTable dt, long bank_Id, long Reference_Type_Table_Id, int DataDictionaryProcessesListsId)
        {

            // DataTable dtFirstBalance = new DataTable();
            //DataTable dtPrev_Period = new DataTable();

            try
            {
                Excp = 0;
                //Bank_id = long.Parse(banks_List1.cmbBankType.SelectedValue.ToString());

                //DateFrom = this.dtOrderDateFrom.Value;
                //DateTo = this.dtOrderDateTo.Value;

                //dtFirstBalance = GetFirstBalance(bank_Id);
                //DateTime datFirstBalance = DateTime.Parse(dtFirstBalance.Rows[0]["firstBalanceDate"].ToString());
                //FirstBalance = decimal.Parse(dtFirstBalance.Rows[0]["Primary_balance"].ToString());


                dt = Money_Box_TransactionFromBills(ref Excp, bank_Id, Reference_Type_Table_Id, DataDictionaryProcessesListsId);

                if (dt.Rows.Count == 0)
                {
                    msg = "لا توجد نتيجة للبحث";
                    Excp = 1;
                    return;
                }



            }
            catch (Exception ee)
            {
                msg = ee.Message;
                Excp = 1;


                return;

            }
        }


        public DataTable GetMoney_Box_TransactionForInvoicesDistribut(int Invoice_TypeId, long Invoice_Id, decimal InvoiceNetTotal = 0)
        {

            string mysql = @"SELECT        Order_Money.Order_No, Order_Money.Order_Date, Order_Money.Total_Value, Order_Money.Man_Name, Order_Money.Order_Notes, Money_Transaction_BoxForInvoices.Order_Mony_Id, Money_Transaction_BoxForInvoices.Import_Value, 
                         Money_Transaction_BoxForInvoices.Export_Value, Money_Transaction_BoxForInvoices.Transaction_Value, Money_Transaction_BoxForInvoices.Invoices_ListId, Money_Transaction_BoxForInvoices.Invoice_TypeId,Money_Transaction_BoxForInvoices.Order_Total_Value  ,Order_Money.Id, Banks.Bank_Name
FROM            Order_Money INNER JOIN
                         Money_Transaction_BoxForInvoices ON Order_Money.Id = Money_Transaction_BoxForInvoices.Order_Mony_Id INNER JOIN
                         Banks ON Money_Transaction_BoxForInvoices.Bank_Id = Banks.id ";
            //mysql = mysql + " WHERE(Money_Transaction_BoxForInvoices.Invoice_TypeId = " + Invoice_TypeId + ") AND (Money_Transaction_BoxForInvoices.Invoice_Id = " + Invoice_Id + ") ";
            mysql = mysql + " WHERE(Money_Transaction_BoxForInvoices.DataDictionaryProcessesListsId = " + Invoice_TypeId + ") AND (Money_Transaction_BoxForInvoices.Reference_Type_Table_Id = " + Invoice_Id + ") ";

            operations opp = new operations();
            DataTable dt = opp.return_DataTable(mysql);
            Common.Make_TableColumnSerial(ref dt, "Serial");
            decimal Import_Value = 0;
            decimal Export_Value = 0;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    Import_Value = Import_Value + decimal.Parse(dt.Rows[i]["Import_Value"].ToString());
                }
                catch (Exception e)
                {


                }
                try
                {
                    Export_Value = Export_Value + decimal.Parse(dt.Rows[i]["Export_Value"].ToString());
                }
                catch (Exception ee)
                {


                }
            }

            DataRow dr = dt.NewRow();
            dr["Import_Value"] = Import_Value;
            dr["Export_Value"] = Export_Value;
            dr["Id"] = 0;
            dr["Order_Notes"] = "الإجمالى";
            dr["Bank_Name"] = "الإجمالى";
            dt.Rows.Add(dr);
            if (InvoiceNetTotal > 0)
            {
                decimal Diff = Import_Value - Export_Value;
                dr = dt.NewRow();
                dr["Id"] = 0;
                dr["Order_Notes"] = "الصافى";
                dr["Bank_Name"] = "الصافى";
                if (Diff > 0)
                {

                    dr["Import_Value"] = Diff;
                    dr["Export_Value"] = 0;

                }
                else
                {
                    decimal Diff_ = Diff * -1;

                    dr["Import_Value"] = 0;
                    dr["Export_Value"] = Diff_;
                }
                //dt.Rows.Add(dr);


                ///////// المتبقى من قيمة الفاتورة
                decimal DiffStill = InvoiceNetTotal - Diff;
                dr = dt.NewRow();
                dr["Id"] = 0;
                dr["Order_Notes"] = "الصافى";
                dr["Bank_Name"] = "الصافى";
                if (DiffStill > 0)
                {

                    dr["Import_Value"] = 0;
                    dr["Export_Value"] = DiffStill;


                }
                else if (DiffStill > 0)
                {
                    decimal Diff_ = DiffStill * -1;

                    dr["Import_Value"] = Diff_;
                    dr["Export_Value"] = 0;


                }
                else if (DiffStill == 0)
                {


                    dr["Import_Value"] = 0;
                    dr["Export_Value"] = 0;

                }
                dt.Rows.Add(dr);

            }
            return dt;
        }


        public DataTable GetMoney_Box_TransactionInvoices(int Invoice_TypeId, long Invoice_Id, decimal InvoiceNetTotal = 0)
        {
            string mysql = @"SELECT        Order_Money.Order_No, Order_Money.Order_Date, Order_Money.Total_Value, Order_Money.Man_Name, Order_Money.Order_Notes, Money_Transaction_Box.Order_Mony_Id, Money_Transaction_Box.Import_Value, 
                         Money_Transaction_Box.Export_Value, Money_Transaction_Box.Transaction_Value, Money_Transaction_Box.Invoice_Id, Money_Transaction_Box.Invoice_TypeId, Order_Money.Id, Banks.Bank_Name
FROM            Order_Money INNER JOIN
                         Money_Transaction_Box ON Order_Money.Id = Money_Transaction_Box.Order_Mony_Id INNER JOIN
                         Banks ON Money_Transaction_Box.Bank_Id = Banks.id ";
            //mysql = mysql + " WHERE(Money_Transaction_Box.Invoice_TypeId = " + Invoice_TypeId + ") AND (Money_Transaction_Box.Invoice_Id = " + Invoice_Id + ") ";
            mysql = mysql + " WHERE(Money_Transaction_Box.DataDictionaryProcessesListsId = " + Invoice_TypeId + ") AND (Money_Transaction_Box.Reference_Type_Table_Id = " + Invoice_Id + ") ";

            operations opp = new operations();
            DataTable dt = opp.return_DataTable(mysql);
            Common.Make_TableColumnSerial(ref dt, "Serial");
            decimal Import_Value = 0;
            decimal Export_Value = 0;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    Import_Value = Import_Value + decimal.Parse(dt.Rows[i]["Import_Value"].ToString());
                }
                catch (Exception e)
                {


                }
                try
                {
                    Export_Value = Export_Value + decimal.Parse(dt.Rows[i]["Export_Value"].ToString());
                }
                catch (Exception ee)
                {


                }
            }

            DataRow dr = dt.NewRow();
            dr["Import_Value"] = Import_Value;
            dr["Export_Value"] = Export_Value;
            dr["Id"] = 0;
            dr["Order_Notes"] = "الإجمالى";
            dr["Bank_Name"] = "الإجمالى";
            dt.Rows.Add(dr);
            if (InvoiceNetTotal > 0)
            {
                decimal Diff = Import_Value - Export_Value;
                dr = dt.NewRow();
                dr["Id"] = 0;
                dr["Order_Notes"] = "الصافى";
                dr["Bank_Name"] = "الصافى";
                if (Diff > 0)
                {

                    dr["Import_Value"] = Diff;
                    dr["Export_Value"] = 0;

                }
                else
                {
                    decimal Diff_ = Diff * -1;

                    dr["Import_Value"] = 0;
                    dr["Export_Value"] = Diff_;
                }
                //dt.Rows.Add(dr);


                ///////// المتبقى من قيمة الفاتورة
                decimal DiffStill = InvoiceNetTotal - Diff;
                dr = dt.NewRow();
                dr["Id"] = 0;
                dr["Order_Notes"] = "المتبقى";
                dr["Bank_Name"] = "المتبقى";
                if (DiffStill > 0)
                {

                    dr["Import_Value"] = 0;
                    dr["Export_Value"] = DiffStill;


                }
                else if (DiffStill > 0)
                {
                    decimal Diff_ = DiffStill * -1;

                    dr["Import_Value"] = Diff_;
                    dr["Export_Value"] = 0;


                }
                else if (DiffStill == 0)
                {


                    dr["Import_Value"] = 0;
                    dr["Export_Value"] = 0;

                }
                dt.Rows.Add(dr);

            }
            return dt;
        }
        public DataSet Money_Box_Transaction(ref int Excp, long Bank_Id, DateTime Day_DateFrom, DateTime Day_DateTo, bool Interval, bool Sort)
        {
            DataSet ds = null;
            string mysql;
            try
            {
                mysql = @" SELECT        Submain_Account.SubAccount_Name, Submain_Account.Main_Account_Id, Submain_Account.SubAccount_NO, Submain_Account.Id, Money_Transaction_Box.Transaction_Type, 
                         Money_Transaction_Box.Import_Value, Money_Transaction_Box.Export_Value, Money_Transaction_Box.Transaction_Value, Money_Transaction_Box.Bank_Balance, Money_Transaction_Box.Order_Mony_Id, 
                         Money_Transaction_Box.Id, Money_Transaction_Box.Transaction_Date, Order_Money.Order_No, Money_Transaction_Box.Bank_Id, Money_Transaction_Box.Account_Id
FROM            Order_Money INNER JOIN
                         Money_Transaction_Box ON Order_Money.Id = Money_Transaction_Box.Order_Mony_Id INNER JOIN
                         Submain_Account ON Money_Transaction_Box.SubAccount_Id = Submain_Account.Id
";

                if (Interval)
                {

                    mysql = mysql + "  WHERE (CONVERT(DATE, Money_Transaction_Box.Transaction_Date) >= '" + operations.dateFormat(Day_DateFrom) + "') AND (Money_Transaction_Box.Transaction_Date <= '" + operations.dateFormat(Day_DateTo) + "') AND (Money_Transaction_Box.Bank_Id = " + Bank_Id + ")";
                }
                else
                {
                    mysql = mysql + "  WHERE (CONVERT(DATE, Money_Transaction_Box.Transaction_Date) = '" + operations.dateFormat(Day_DateFrom) + "') AND (Money_Transaction_Box.Bank_Id = " + Bank_Id + ")";
                }
                // if (Sort)
                // {
                //     mysql = mysql + " ORDER BY Money_Transaction_Box.Transaction_Date DESC ";
                // }
                //else {
                //     mysql = mysql + " ORDER BY Money_Transaction_Box.Transaction_Date ASC ";
                // }
                if (Sort)
                {
                    mysql = mysql + " ORDER BY Money_Transaction_Box.ID DESC ";
                }
                else
                {
                    mysql = mysql + " ORDER BY Money_Transaction_Box.ID ASC ";
                }

                operations opp = new operations();
                ds = opp.return_dataset(ref Excp, mysql, "Money_Transaction_Box");
                opp = null;
                Excp = 0;
                return ds;
            }
            catch (Exception e)
            {
                Excp = 1;
                throw (e);

            }
        }
        public DataTable Money_Box_Transaction(ref int Excp, long Bank_Id, DateTime Day_DateFrom, DateTime Day_DateTo, bool Interval)
        {
            DataTable dt = new DataTable();
            string mysql;
            try
            {
                mysql = @"   SELECT        Money_Transaction_Box.Transaction_Date, Order_Money.Order_Date, Submain_Account.SubAccount_Name, Submain_Account.Main_Account_Id, Submain_Account.SubAccount_NO, Submain_Account.Id, 
                         Money_Transaction_Box.Transaction_Type, Money_Transaction_Box.Import_Value, Money_Transaction_Box.Export_Value, Money_Transaction_Box.Transaction_Value, Money_Transaction_Box.Bank_Balance, 
                         Money_Transaction_Box.Order_Mony_Id, Money_Transaction_Box.Id, Order_Money.Order_No, Money_Transaction_Box.Bank_Id, Money_Transaction_Box.Account_Id, Order_Money.Order_Notes, 
                         Money_Transaction_Box.Notes AS Money_Transaction_Box_Notes, Order_Money.Order_Notes + N'  ' + Money_Transaction_Box.Notes AS Notes, Order_Money.Order_Notes AS Expr1
FROM            Order_Money INNER JOIN
                         Money_Transaction_Box ON Order_Money.Id = Money_Transaction_Box.Order_Mony_Id INNER JOIN
                         Submain_Account ON Money_Transaction_Box.SubAccount_Id = Submain_Account.Id
";
                mysql = mysql + " WHERE (Money_Transaction_Box.Bank_Id = " + Bank_Id + ")";

                //string P_mysql = mysql;
                if (Interval)
                {
                    //mysql = mysql + "  And  (Money_Transaction_Box.Transaction_Date >= '" + operations.dateFormat(Day_DateFrom) + "') AND (Money_Transaction_Box.Transaction_Date <= '" + operations.dateFormat(Day_DateTo) + "') ";

                    mysql = mysql + " AND (CONVERT(DATE, Money_Transaction_Box.Transaction_Date )  >= '" + operations.dateFormat(Day_DateFrom.Date) + "') ";
                    mysql = mysql + " AND (CONVERT(DATE, Money_Transaction_Box.Transaction_Date )  <= '" + operations.dateFormat(Day_DateTo.Date) + "') ";


                }
                else
                {
                    //mysql = mysql + "  And (Money_Transaction_Box.Transaction_Date = '" + operations.dateFormat(Day_DateFrom) + "') ";

                    mysql = mysql + " AND (CONVERT(DATE, Money_Transaction_Box.Transaction_Date )  = '" + operations.dateFormat(Day_DateFrom.Date) + "') ";


                }


                mysql = mysql + " ORDER BY Money_Transaction_Box.Transaction_Date,Money_Transaction_Box.Id   ";


                operations opp = new operations();
                dt = opp.return_DataTable(mysql);
                //Common.Make_TableColumnSerial(ref dt, "Serial_");
                //////////////////////////////
                //P_mysql = P_mysql + "  And  (Money_Transaction_Box.Transaction_Date < '" + operations.dateFormat(Day_DateFrom) + "') AND (Money_Transaction_Box.Transaction_Date >= '" + operations.dateFormat(dtFirstDate) + "') ";

                //dtPrev_Period = opp.return_DataTable(P_mysql);
                Excp = 0;
                return dt;
            }
            catch (Exception e)
            {
                Excp = 1;

                return dt;
            }
        }
        public DataTable Money_Box_TransactionFromBills(ref int Excp, long Bank_Id, long Reference_Type_Table_Id, int DataDictionaryProcessesListsId)
        {
            DataTable dt = new DataTable();
            string mysql;
            try
            {
                mysql = @"   SELECT        Money_Transaction_Box.Transaction_Date, Order_Money.Order_Date, Submain_Account.SubAccount_Name, Submain_Account.Main_Account_Id, Submain_Account.SubAccount_NO, Submain_Account.Id, 
                         Money_Transaction_Box.Transaction_Type, Money_Transaction_Box.Import_Value, Money_Transaction_Box.Export_Value, Money_Transaction_Box.Transaction_Value, Money_Transaction_Box.Bank_Balance, 
                         Money_Transaction_Box.Order_Mony_Id, Money_Transaction_Box.Id, Order_Money.Order_No, Money_Transaction_Box.Bank_Id, Money_Transaction_Box.Account_Id, Order_Money.Order_Notes, 
                         Money_Transaction_Box.Notes AS Money_Transaction_Box_Notes, Order_Money.Order_Notes + N'  ' + Money_Transaction_Box.Notes AS Notes, Order_Money.Order_Notes AS Expr1
FROM            Order_Money INNER JOIN
                         Money_Transaction_Box ON Order_Money.Id = Money_Transaction_Box.Order_Mony_Id INNER JOIN
                         Submain_Account ON Money_Transaction_Box.SubAccount_Id = Submain_Account.Id
";
                mysql = mysql + " WHERE (Money_Transaction_Box.Id > 0)";
                if (Bank_Id > 0)
                {

                    mysql = mysql + "  And  (Money_Transaction_Box.Bank_Id = " + Bank_Id + " )";
                }

                //string P_mysql = mysql;
                if (Reference_Type_Table_Id > 0)
                {
                    mysql = mysql + "  And  (Money_Transaction_Box.DataDictionaryProcessesListsId = " + DataDictionaryProcessesListsId + " )";
                    mysql = mysql + "  And  (Money_Transaction_Box.Reference_Type_Table_Id = " + Reference_Type_Table_Id + " )";
                }



                mysql = mysql + " ORDER BY Money_Transaction_Box.Transaction_Date,Money_Transaction_Box.Id   ";


                operations opp = new operations();
                dt = opp.return_DataTable(mysql);
                Common.Make_TableColumnSerial(ref dt, "Serial_");
                //////////////////////////////
                //P_mysql = P_mysql + "  And  (Money_Transaction_Box.Transaction_Date < '" + operations.dateFormat(Day_DateFrom) + "') AND (Money_Transaction_Box.Transaction_Date >= '" + operations.dateFormat(dtFirstDate) + "') ";

                //dtPrev_Period = opp.return_DataTable(P_mysql);
                Excp = 0;
                return dt;
            }
            catch (Exception e)
            {
                Excp = 1;

                return dt;
            }
        }


        public DataTable Get_Money_Box_Transaction(long Bank_Id, DateTime Day_Date, int Transaction_Type)
        {

            string mysql;
            operations opp = new operations();
            mysql = @" SELECT        Order_Money.* FROM    Order_Money ";
            mysql = mysql + " WHERE      (Transaction_type = " + Transaction_Type + ") AND   (Order_Date = " + operations.Get_DateTimeToInsert(Day_Date.Date) + ") AND (Bank_ID =" + Bank_Id + ") ";


            DataTable dt = opp.return_DataTable(mysql);
            return dt;

        }
        public DataTable Get_Money_Box_Transaction_Details(long Bank_Id, DateTime Day_Date, int Transaction_Type)
        {

            string mysql;
            operations opp = new operations();
            mysql = @" SELECT Submain_Account.SubAccount_Name, Order_Money.*, Order_Money.Order_Notes + N'  ' + Money_Transaction_Box.Notes AS AllOrder_Notes, Money_Transaction_Box.*
FROM            Order_Money INNER JOIN
                         Money_Transaction_Box ON Order_Money.Id = Money_Transaction_Box.Order_Mony_Id LEFT OUTER JOIN
                         Submain_Account ON Money_Transaction_Box.SubAccount_Id = Submain_Account.Id ";
            mysql = mysql + " WHERE      ( Order_Money.Transaction_type = " + Transaction_Type + ") ";
            mysql = mysql + " AND (CONVERT(DATE, Order_Money.Order_Date)   = '" + operations.dateFormat(Day_Date.Date) + "') ";



            mysql = mysql + "   AND   ( Money_Transaction_Box.Bank_Id =" + Bank_Id + ")";
            //(Order_Money.Bank_ID = " + Bank_Id + ") OR
            //AND(Order_Money.Bank_ID = " + Bank_Id + ")
            DataTable dt = opp.return_DataTable(mysql);
            return dt;

        }


        public decimal Compute_Box_BalanceBeforDataDay(long Bank_Id, bool Is_ALLDate, DateTime Day_Date)
        {
            operations opp = new operations();
            return Compute_Box_BalanceBeforDataDay(ref opp, Bank_Id, Is_ALLDate, Day_Date);
        }

        // 🟢 دالة بحث جديدة (تقنية فقط): نفس استعلام Connection4It.SandOuk_AccountList.GetSandoukWithout_Perission(0,0)
        // (مشروع WinForms فقط) - قائمة الصناديق/البنوك (Is_Bank=0) بدون تصفية صلاحيات المستخدم، بنفس شرط الفورم
        // الأصلى تماماً بدون أي تغيير في منطق العمل.
        public DataTable Get_BoxesList()
        {
            operations opp = new operations();
            string mysql = @" SELECT        Currency.Currency_Name, Currency.Currency_Name_EN, Banks.*
FROM            Banks INNER JOIN
                         Currency ON Banks.currency_Id = Currency.Value ";
            mysql = mysql + " Where    Is_Bank=0 ";
            return opp.return_DataTable(mysql);
        }
        public decimal Compute_Box_BalanceBeforDataDay(ref operations opp, long Bank_Id, bool Is_ALLDate, DateTime Day_Date)
        {

            decimal Import_Value_Old = 0;
            decimal Export_Value_Old = 0;
            Get_Money_Box_TransactionBeforDataDay(ref opp, Bank_Id, Is_ALLDate, Day_Date, ref Import_Value_Old, ref Export_Value_Old);
            decimal Primary_balance = Get_BankPrimary_balance(ref opp, Bank_Id);

            decimal First_BalanceAfterDay = Primary_balance + Import_Value_Old - Export_Value_Old;
            if (First_BalanceAfterDay < 0)
            {
                First_BalanceAfterDay = 0;
            }
            return First_BalanceAfterDay;
        }
        void Get_Money_Box_TransactionBeforDataDay(ref operations opp, long Bank_Id, bool Is_ALLDate, DateTime Day_Date, ref decimal Import_Value_Old, ref decimal Export_Value_Old)
        {
            Import_Value_Old = 0;
            Export_Value_Old = 0;
            string mysql;

            //            mysql = @" SELECT        SUM(Money_Transaction_Box.Import_Value) AS Import_Value, SUM(Money_Transaction_Box.Export_Value) AS Export_Value
            //FROM            Money_Transaction_Box INNER JOIN
            //                         Order_Money ON Money_Transaction_Box.Order_Mony_Id = Order_Money.Id ";

            mysql = @"  SELECT        SUM(Import_Value) AS Import_Value, SUM(Export_Value) AS Export_Value
FROM            Money_Transaction_Box  WITH (NOLOCK) ";


            mysql = mysql + " WHERE     (Money_Transaction_Box.Bank_Id =" + Bank_Id + ") ";
            if (Is_ALLDate == false)
            {
                //mysql = mysql + " And (Money_Transaction_Box.Transaction_Date < " + operations.Get_DateTimeToInsert(Day_Date.Date) + ")   ";

                mysql = mysql + " AND (CONVERT(DATE, Money_Transaction_Box.Transaction_Date )  < '" + operations.dateFormat(Day_Date.Date) + "') ";

            }

            DataTable dt = opp.return_DataTable(mysql);

            if (dt.Rows.Count > 0)
            {
                try
                {
                     decimal.TryParse(dt.Rows[0]["Import_Value"].ToString(),out Import_Value_Old);
                }
                catch { Import_Value_Old = 0; }

                try
                {
                     decimal.TryParse(dt.Rows[0]["Export_Value"].ToString(),out Export_Value_Old);
                }
                catch { Export_Value_Old = 0; }

            }


        }



        public DataTable Get_Money_Box_TransactionPrintEstlam(long Bank_Id, DateTime Day_Date, int Transaction_Type)
        {

            string mysql;
            operations opp = new operations();
            mysql = @" SELECT        Order_Money.Order_No AS Order_NoE, Order_Money.Total_Value, Order_Money.Order_Notes, Money_Transaction_Box.Transaction_Value AS Total_ValueE, Money_Transaction_Box.Notes AS Order_NotesE, 
                         Money_Transaction_Box.Account_Name  AS  [AccountNameE]  , Money_Transaction_Box.SubAccount_Id, Submain_Account.SubAccount_Name
FROM            Order_Money INNER JOIN
                         Money_Transaction_Box ON Order_Money.Id = Money_Transaction_Box.Order_Mony_Id LEFT OUTER JOIN
                         Submain_Account ON Money_Transaction_Box.SubAccount_Id = Submain_Account.Id ";
            mysql = mysql + " WHERE      (Money_Transaction_Box.Transaction_type = " + Transaction_Type + ")  AND (Money_Transaction_Box.Bank_ID =" + Bank_Id + ") ";
            //AND(Money_Transaction_Box.Transaction_Date = " + operations.Get_DateTimeToInsert(Day_Date.Date) + ")
            mysql = mysql + " AND (CONVERT(DATE,Money_Transaction_Box.Transaction_Date)   = '" + operations.dateFormat(Day_Date.Date) + "') ";

            DataTable dt = opp.return_DataTable(mysql);
            return dt;

        }
        public DataTable Get_Money_Box_TransactionPrintSarf(long Bank_Id, DateTime Day_Date, int Transaction_Type)
        {

            string mysql;
            operations opp = new operations();
            mysql = @" SELECT        Order_Money.Order_No AS Order_NoS, Order_Money.Total_Value, Order_Money.Order_Notes, Money_Transaction_Box.Transaction_Value AS Total_ValueS, Money_Transaction_Box.Notes AS Order_NotesS, 
                         Submain_Account.SubAccount_Name, Money_Transaction_Box.Account_Name AS  AccountNameS
FROM            Order_Money INNER JOIN
                         Money_Transaction_Box ON Order_Money.Id = Money_Transaction_Box.Order_Mony_Id LEFT OUTER JOIN
                         Submain_Account ON Money_Transaction_Box.SubAccount_Id = Submain_Account.Id ";
            //mysql = mysql + " WHERE      (Transaction_type = " + Transaction_Type + ") AND   (Order_Date = " + operations.Get_DateTimeToInsert(Day_Date.Date) + ") AND (Bank_ID =" + Bank_Id + ") ";
            mysql = mysql + " WHERE      (Money_Transaction_Box.Transaction_type = " + Transaction_Type + ") AND    (Money_Transaction_Box.Bank_ID =" + Bank_Id + ") ";
            //(Money_Transaction_Box.Transaction_Date = " + operations.Get_DateTimeToInsert(Day_Date.Date) + ") AND
            mysql = mysql + " AND (CONVERT(DATE,Money_Transaction_Box.Transaction_Date)   = '" + operations.dateFormat(Day_Date.Date) + "') ";


            DataTable dt = opp.return_DataTable(mysql);
            return dt;

        }
        public DataSet Money_Box_Transaction(ref int Excp, int Transaction_Type, long Bank_Id, DateTime Day_DateFrom, DateTime Day_DateTo, bool Interval, bool Sort)
        {
            DataSet ds = null;
            string mysql;
            try
            {
                mysql = " SELECT Submain_Account.SubAccount_Name, Submain_Account.Main_Account_Id, Submain_Account.SubAccount_NO, Submain_Account.Id,  Money_Transaction_Box.Transaction_Type, Money_Transaction_Box.Import_Value, Money_Transaction_Box.Export_Value,  Money_Transaction_Box.Transaction_Value, Money_Transaction_Box.Bank_Balance, Supplier.Name AS Sup_Name, Customer.Name AS CustName,  Money_Transaction_Box.Order_Mony_Id, Money_Transaction_Box.Id, Money_Transaction_Box.Transaction_Date, Order_Money.Order_No,  Money_Transaction_Box.Bank_Id ,Money_Transaction_Box.Account_Id";
                mysql = mysql + "  FROM Supplier RIGHT OUTER JOIN  Customer RIGHT OUTER JOIN  Order_Money INNER JOIN  Money_Transaction_Box ON Order_Money.Id = Money_Transaction_Box.Order_Mony_Id ON Customer.Id = Money_Transaction_Box.Customer_Id ON   Supplier.Id = Money_Transaction_Box.Supplier_Id LEFT OUTER JOIN  Submain_Account ON Money_Transaction_Box.Account_Id = Submain_Account.Id ";

                if (Interval)
                {
                    mysql = mysql + "  WHERE (Money_Transaction_Box.Transaction_Date >= '" + operations.dateFormat(Day_DateFrom) + "') AND (Money_Transaction_Box.Transaction_Date <= '" + operations.dateFormat(Day_DateTo) + "') AND (Money_Transaction_Box.Bank_Id = " + Bank_Id + ")";
                }
                else
                {
                    mysql = mysql + "  WHERE (Money_Transaction_Box.Transaction_Date = '" + operations.dateFormat(Day_DateFrom) + "') AND (Money_Transaction_Box.Bank_Id = " + Bank_Id + ")";
                }
                mysql = mysql + " And  Money_Transaction_Box.Transaction_Type=" + Transaction_Type + " ";
                // if (Sort)
                // {
                //     mysql = mysql + " ORDER BY Money_Transaction_Box.Transaction_Date DESC ";
                // }
                //else {
                //     mysql = mysql + " ORDER BY Money_Transaction_Box.Transaction_Date ASC ";
                // }
                if (Sort)
                {
                    mysql = mysql + " ORDER BY Money_Transaction_Box.ID DESC ";
                }
                else
                {
                    mysql = mysql + " ORDER BY Money_Transaction_Box.ID ASC ";
                }

                operations opp = new operations();
                ds = opp.return_dataset(ref Excp, mysql, "Money_Transaction_Box");
                opp = null;
                Excp = 0;
                return ds;
            }
            catch (Exception e)
            {
                Excp = 1;
                throw (e);

            }
        }



        void GetCustomer_Balance(string TableName, long CustomerSupplier_Id, ref operations opp, ref decimal Balance_Before, ref decimal Balance_After, decimal Cash_Value)
        {
            Balance_Before = 0;
            try
            {
                string mysql = "Select * from " + TableName + " Where id=" + CustomerSupplier_Id;
                DataTable dt = opp.return_DataTable(mysql);
                if (dt.Rows.Count > 0)
                {
                    //Balance_Before = decimal.Parse(dt.Rows[0]["Balance"].ToString());
                    //Balance_Before = Common.RoundMe(Balance_Before);

                    try
                    {
                        SubAccount_Id_ = long.Parse(dt.Rows[0]["SubAccount_Id"].ToString());
                    }
                    catch { SubAccount_Id_ = 0; }
                    SubAccountName = dt.Rows[0]["Name"].ToString();
                }



                //Balance_After = Balance_Before - Cash_Value;

            }
            catch { }


        }
        public Bank GetBank_Balance(long Bank_Id, ref operations opp, ref decimal Last_Balance)
        {

            Bank bank = new Bank();
            bank = opp.db.Banks.Where(b => b.Id == Bank_Id).FirstOrDefault();

            Last_Balance = Compute_Box_BalanceBeforDataDay(ref opp, Bank_Id, true, DateTime.Now.Date);



            return bank;
        }

        public decimal GetBank_Balance(ref operations opp, long Bank_Id, bool ISAllDates)
        {
            decimal Last_Balance = Compute_Box_BalanceBeforDataDay(ref opp, Bank_Id, ISAllDates, DateTime.Now.Date);


            return Last_Balance;
        }
        public decimal GetBank_Balance(long Bank_Id, bool ISAllDates)
        {
            operations opp = new operations();
            decimal Last_Balance = Compute_Box_BalanceBeforDataDay(ref opp, Bank_Id, ISAllDates, DateTime.Now.Date);


            return Last_Balance;
        }


        public DataTable Get_Banks(long Bank_Id)
        {

            string mysql;
            operations opp = new operations();
            mysql = @" SELECT       *  FROM Banks";
            mysql = mysql + " WHERE   ( ID =" + Bank_Id + ") ";


            DataTable dt = opp.return_DataTable(mysql);
            return dt;

        }


        public static long GetBank_SubAccount_Id(ref operations opp, long Bank_Id)
        {
            long SubAccount_Id = 0;

            try
            {
                string mysql = "Select * from Banks Where id=" + Bank_Id;
                DataTable dt = opp.return_DataTable(mysql);
                if (dt.Rows.Count > 0)
                {
                    SubAccount_Id = long.Parse(dt.Rows[0]["SubAccount_Id"].ToString());

                }
            }
            catch { }

            return SubAccount_Id;
        }

        void InsertOrderMoneyNew(ref int Excp, ref string Msg, ref operations opp, ref long Order_Mony_Id2, long Order_No, int Transaction_type, DateTime Order_Date, decimal Total_Value, long Currency_Id, int Bank_Type_, string Man_Name, long Bank_Id, int Order_Mony_Id_Transform, long Invoice_ID, int Invoice_Type, long User_Id, long Branch_Id, long Year_Financial_Period_Id, long Ticket_Supplier_Id, string Order_Tafkeet_Value, string Order_Notes, long Sandouk_Cost_Center_Tree_Id, int IS_Posted_Flage, decimal Factor, long Entry_Currency_ID, string Entry_Currency_Name, long CarsWorkOrdersId, long Reference_Type_Table_Id, long DataDictionaryProcessesListsId)
        {

            try
            {

                //long Year_Financial_Period_Id = Financial_PeriodCLS.Get_Year_Financial_Period_Id(Order_Date);

                Order_MoneyCLS om = new Order_MoneyCLS();
                Order_Money order_Money = new Order_Money();
                order_Money.Order_No = Order_No;
                order_Money.Transaction_type = Transaction_type;
                order_Money.Order_Date = Order_Date;
                order_Money.Total_Value = Total_Value;
                order_Money.Currency_Id = Currency_Id;
                order_Money.Bank_Type = Bank_Type_;
                order_Money.Man_Name = Man_Name;
                order_Money.Bank_Id = Bank_Id;
                order_Money.Order_Notes = Order_Notes;
                order_Money.User_Id = Common.User_Id;
                order_Money.Branch_Id = Common.Branch_Id;
                order_Money.OperationDate = DateTime.Now.Date;
                order_Money.Insert_ServerDateTime = DateTime.Now.Date;
                order_Money.Factor = Factor;
                order_Money.Year_Financial_Period_Id = Year_Financial_Period_Id;


                order_Money.Is_Balances_Set = 0;
                order_Money.Day_Registration_flage = 0;
                order_Money.DayReg_List_Id = 0;
                order_Money.Order_mony_id_transform = Order_Mony_Id_Transform;
                order_Money.Invoice_Id = Invoice_ID;

                order_Money.CarsWorkOrdersId = CarsWorkOrdersId;
                order_Money.Reference_Type_Table_Id = Reference_Type_Table_Id;
                order_Money.DataDictionaryProcessesListsId = DataDictionaryProcessesListsId;




                order_Money.Invoice_Type = Invoice_Type;
                order_Money.Ticket_Supplier_Id = Ticket_Supplier_Id;
                order_Money.Order_Tafkeet_Value = Order_Tafkeet_Value;
                order_Money.Sandouk_Cost_Center_Tree_Id = Sandouk_Cost_Center_Tree_Id;
                order_Money.IS_Posted_Flage = IS_Posted_Flage;
                order_Money.Entry_Currency_ID = Entry_Currency_ID;
                order_Money.Entry_Currency_Name = Entry_Currency_Name;


                om.InsertOrder_Money(ref Excp, ref Msg, ref opp, order_Money);

                Order_Mony_Id2 = om.om.Id;

            }
            catch (Exception ee)
            {

            }
        }



        public void Set_BankBalanceDay(ref operations opp, DateTime Balance_Date, long Bank_Id, decimal First_Balance, decimal Total_Value, int Transaction_Type)
        {
            string mysql = @" SELECT  Banks_Balance.* FROM    Banks_Balance ";
            mysql = mysql + " WHERE (Bank_Id = " + Bank_Id + ") AND (Balance_Date = " + operations.Get_DateTimeToInsert(Balance_Date.Date) + ")  ";
            DataTable dt = opp.return_DataTable(mysql);

            if (dt.Rows.Count > 0)//////تم  حفظظ الرصيد من قبل
            {
                long Id = long.Parse(dt.Rows[0]["Id"].ToString());

                if (Transaction_Type == 1)
                {

                    mysql = " UPDATE  Banks_Balance SET   Last_Balance = Last_Balance + " + Total_Value + "  ";
                    mysql = mysql + " WHERE  (Id = " + Id + ")";


                }
                else
                {
                    mysql = " UPDATE  Banks_Balance SET   Last_Balance = Last_Balance -  " + Total_Value + "  ";
                    mysql = mysql + " WHERE  (Id = " + Id + ")";
                }

            }
            else
            {
                ////تاريخ يستعمل لاول مره
                decimal Last_Balance = 0;

                ///البحث عن التاريخ السابق حتى ناخذ منه الرصيد النهائى
                mysql = @" SELECT Top(1) Banks_Balance.* FROM    Banks_Balance ";
                mysql = mysql + " WHERE (Bank_Id = " + Bank_Id + ") AND (Balance_Date < " + operations.Get_DateTimeToInsert(Balance_Date.Date) + ")  ";
                mysql = mysql + " Order By Balance_Date Desc";
                dt = opp.return_DataTable(mysql);
                if (dt.Rows.Count > 0)
                {
                    Last_Balance = decimal.Parse(dt.Rows[0]["Last_Balance"].ToString());
                    First_Balance = Last_Balance;
                }
                else
                {
                    ///البحث عن التاريخ التالى حتى ناخذ منه الرصيد النهائى
                    mysql = @" SELECT Top(1) Banks_Balance.* FROM    Banks_Balance ";
                    mysql = mysql + " WHERE (Bank_Id = " + Bank_Id + ") AND (Balance_Date > " + operations.Get_DateTimeToInsert(Balance_Date.Date) + ")  ";
                    mysql = mysql + " Order By Balance_Date Asc";
                    dt = opp.return_DataTable(mysql);
                    if (dt.Rows.Count > 0)
                    {
                        Last_Balance = decimal.Parse(dt.Rows[0]["Last_Balance"].ToString());
                        First_Balance = Last_Balance;
                    }
                }

                if (Transaction_Type == 1)
                {

                    Last_Balance = First_Balance + Total_Value;

                }
                else
                {
                    Last_Balance = First_Balance - Total_Value;
                }
                mysql = " INSERT INTO Banks_Balance(Bank_Id, Balance_Date, First_Balance, Last_Balance)";
                mysql = mysql + " VALUES     (" + Bank_Id + "," + operations.Get_DateTimeToInsert(Balance_Date) + "," + First_Balance + "," + Last_Balance + ")";
                //opp.insert_me(mysql);
                //mysql = " ";
            }
            /////////////////////////////
            if (Transaction_Type == 1)
            {

                mysql = mysql + " UPDATE  Banks_Balance SET  First_Balance = First_Balance + " + Total_Value + " , Last_Balance = Last_Balance + " + Total_Value + "  ";
                mysql = mysql + " WHERE   (Bank_Id = " + Bank_Id + ") AND (Balance_Date > " + operations.Get_DateTimeToInsert(Balance_Date) + ")";
            }
            else
            {
                mysql = mysql + " UPDATE  Banks_Balance SET  First_Balance = First_Balance - " + Total_Value + " , Last_Balance = Last_Balance - " + Total_Value + "  ";
                mysql = mysql + " WHERE  (Bank_Id = " + Bank_Id + ") AND  (Balance_Date > " + operations.Get_DateTimeToInsert(Balance_Date) + ")";

            }
            mysql = mysql + " UPDATE  Banks_Balance SET  First_Balance   =0";
            mysql = mysql + " WHERE  (Bank_Id = " + Bank_Id + ") AND ( First_Balance  < 0 ) AND   (Balance_Date > " + operations.Get_DateTimeToInsert(Balance_Date) + ")";

            mysql = mysql + " UPDATE  Banks_Balance SET  Last_Balance   =0";
            mysql = mysql + " WHERE  (Bank_Id = " + Bank_Id + ") AND ( Last_Balance  < 0  ) AND  (Balance_Date > " + operations.Get_DateTimeToInsert(Balance_Date) + ")";
            opp.insert_me(mysql);
        }

        public void Set_UpdateBankBalanceDay(ref operations opp, DateTime Balance_Date, long Bank_Id, decimal First_Balance, decimal Total_Value, int Transaction_Type)
        {
            string mysql = @" SELECT  Banks_Balance.* FROM    Banks_Balance ";
            mysql = mysql + " WHERE (Bank_Id = " + Bank_Id + ") AND (Balance_Date = " + operations.Get_DateTimeToInsert(Balance_Date.Date) + ")  ";
            DataTable dt = opp.return_DataTable(mysql);

            if (dt.Rows.Count > 0)
            {
                long Id = long.Parse(dt.Rows[0]["Id"].ToString());

                if (Transaction_Type == 1)
                {

                    mysql = " UPDATE  Banks_Balance SET   Last_Balance = Last_Balance + " + Total_Value + "  ";
                    mysql = mysql + " WHERE  (Id = " + Id + ")";

                    mysql = mysql + " UPDATE  Banks_Balance SET   Last_Balance = Last_Balance + " + Total_Value + "  ";
                    mysql = mysql + " WHERE (Bank_Id = " + Bank_Id + ") AND (Balance_Date > " + operations.Get_DateTimeToInsert(Balance_Date.Date) + ")  ";




                }
                else
                {
                    mysql = " UPDATE  Banks_Balance SET   Last_Balance = Last_Balance -  " + Total_Value + "  ";
                    mysql = mysql + " WHERE  (Id = " + Id + ")";

                    mysql = mysql + " UPDATE  Banks_Balance SET   Last_Balance = Last_Balance -  " + Total_Value + "  ";
                    mysql = mysql + " WHERE (Bank_Id = " + Bank_Id + ") AND (Balance_Date > " + operations.Get_DateTimeToInsert(Balance_Date.Date) + ")  ";

                }

            }
            else
            {
                decimal Last_Balance = 0;
                if (Transaction_Type == 1)
                {

                    Last_Balance = First_Balance + Total_Value;

                }
                else
                {
                    Last_Balance = First_Balance - Total_Value;
                }
                mysql = " INSERT INTO Banks_Balance(Bank_Id, Balance_Date, First_Balance, Last_Balance)";
                mysql = mysql + " VALUES     (" + Bank_Id + "," + operations.Get_DateTimeToInsert(Balance_Date) + "," + First_Balance + "," + Last_Balance + ")";
            }
            opp.insert_me(mysql);
        }
        public void Set_Delete_BankBalanceDay(ref operations opp, DateTime Balance_Date, long Bank_Id, decimal First_Balance, decimal Total_Value, int Transaction_Type)
        {
            string mysql = @" SELECT  Banks_Balance.* FROM    Banks_Balance ";
            mysql = mysql + " WHERE (Bank_Id = " + Bank_Id + ") AND (Balance_Date = " + operations.Get_DateTimeToInsert(Balance_Date.Date) + ")  ";
            DataTable dt = opp.return_DataTable(mysql);

            if (dt.Rows.Count > 0)
            {
                long Id = long.Parse(dt.Rows[0]["Id"].ToString());

                if (Transaction_Type == 2)
                {

                    mysql = " UPDATE  Banks_Balance SET   Last_Balance = Last_Balance + " + Total_Value + "  ";
                    mysql = mysql + " WHERE  (Id = " + Id + ")";

                    mysql = mysql + " UPDATE  Banks_Balance SET   Last_Balance = Last_Balance + " + Total_Value + "  ";
                    mysql = mysql + " WHERE (Bank_Id = " + Bank_Id + ") AND (Balance_Date > " + operations.Get_DateTimeToInsert(Balance_Date.Date) + ")  ";




                }
                else
                {
                    mysql = " UPDATE  Banks_Balance SET   Last_Balance = Last_Balance -  " + Total_Value + "  ";
                    mysql = mysql + " WHERE  (Id = " + Id + ")";

                    mysql = mysql + " UPDATE  Banks_Balance SET   Last_Balance = Last_Balance -  " + Total_Value + "  ";
                    mysql = mysql + " WHERE (Bank_Id = " + Bank_Id + ") AND (Balance_Date > " + operations.Get_DateTimeToInsert(Balance_Date.Date) + ")  ";

                }

            }
            else
            {
                decimal Last_Balance = 0;
                if (Transaction_Type == 1)
                {

                    Last_Balance = First_Balance + Total_Value;

                }
                else
                {
                    Last_Balance = First_Balance - Total_Value;
                }
                mysql = " INSERT INTO Banks_Balance(Bank_Id, Balance_Date, First_Balance, Last_Balance)";
                mysql = mysql + " VALUES     (" + Bank_Id + "," + operations.Get_DateTimeToInsert(Balance_Date) + "," + First_Balance + "," + Last_Balance + ")";
            }
            opp.insert_me(mysql);
        }
        public DataTable Get_BankBalanceDay(DateTime Balance_Date, long Bank_Id)
        {
            operations opp = new operations();
            string mysql = @" SELECT  Banks_Balance.* FROM    Banks_Balance ";
            mysql = mysql + " WHERE (Bank_Id = " + Bank_Id + ") AND (Balance_Date = " + operations.Get_DateTimeToInsert(Balance_Date.Date) + ")  ";
            DataTable dt = opp.return_DataTable(mysql);

            return dt;
        }
        public decimal Get_BankPrimary_balance(ref operations opp, long Bank_Id)
        {
            decimal Primary_balance = 0;

            string mysql = @" SELECT        Banks.*  FROM  Banks ";
            mysql = mysql + " WHERE (ID = " + Bank_Id + ")    ";
            DataTable dt = opp.return_DataTable(mysql);
            if (dt.Rows.Count > 0)
            {
                Primary_balance = decimal.Parse(dt.Rows[0]["Primary_balance"].ToString());
            }
            return Primary_balance;
        }
        public long Lookup_CashPaymentMethodId { get; set; } = 0;
        public string Lookup_CashPaymentMethodName { get; set; } = "";
        public void Make_Transaction_BanksBox_FromInvoices(ref int Excp, ref string Msg, ref operations opp, ref long Order_Mony_Id2, ref long Money_Transaction_Box_ID, int Transaction_Type, string Order_No, long Box_Id, decimal Box_Balance, decimal Cach_Value, DateTime Order_Date, string Man_Name, long Currency_Id, long Customer_ID_Supplier, int Reverse_Order, long Entry_No, long CarsWorkOrdersId, long Reference_Type_Table_Id, long DataDictionaryProcessesListsId)
        {



            long   User_Id = Common.User_Id;
            long Branch_Id = Common.Branch_Id;


            string mysql = "";

            decimal Cach_Value_Comput = Cach_Value;
            long Entry_Currency_ID = 1;
            string Bank_Name = "";
            //Bank bnk = GetBank_Balance(Box_Id, ref opp, ref Box_Balance);
            Bank bnk = opp.db.Banks.AsNoTracking().Where(b => b.Id == Box_Id).FirstOrDefault();
            if (bnk != null)
            {
                Entry_Currency_ID = bnk.Currency_Id ?? 1;
                Bank_Name = bnk.Bank_Name;
            }
            string Entry_Currency_Name = "جنية مصرى";
            int Ticket_Supplier_Id = 0;
            int Sandouk_Cost_Center_Tree_Id = 0;
            int IS_Posted_Flage = 0;
            string Order_Notes = Man_Name;
            Notes = Man_Name;
            string Order_Tafkeet_Value = TafkeetProj.NumberToText.GetText(Cach_Value, Entry_Currency_ID);
            int Bank_Type_ = 0;
            int Order_Mony_Id_Transform = -1;
            long Year_Financial_Period_Id = Financial_PeriodCLS.Get_Year_Financial_Period_Id(Order_Date);


            try
            {


                long B_Id = 0;

                Order_Mony_Id2 = B_Id;
                Money_Transaction_Box_ID = 0;

                //B_Idx= Insert_Order_Money(ref   opp, Order_No, Order_Date, Transaction_Type, Cach_Value, Currency_Id, 0, Box_Id, Man_Name, 0, 0, 0, 0, 0, 0, "", "", 0, 1.0, 1, "جنية مصرى");
                if (Transaction_Type == 1 & Reverse_Order == 0)  //استلام من عميل
                {

                    GetCustomer_Balance("Customer", Customer_ID_Supplier, ref opp, ref Balance_Before, ref Balance_After, Cach_Value_Comput);


                    InsertOrderMoneyNew(ref Excp, ref Msg, ref opp, ref Order_Mony_Id2, long.Parse(Order_No), Transaction_Type, Order_Date, Cach_Value, Currency_Id, Bank_Type_, Man_Name, Box_Id, Order_Mony_Id_Transform, Invoice_ID, Invoice_Type, User_Id, Branch_Id, Year_Financial_Period_Id, Ticket_Supplier_Id, Order_Tafkeet_Value, Order_Notes, Sandouk_Cost_Center_Tree_Id, IS_Posted_Flage, Factor, Entry_Currency_ID, Entry_Currency_Name, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);

                    decimal Last_Balance = Box_Balance + Cach_Value;

                    Money_Transaction_Box_ID = InsertMoney_Transaction_Box(ref Excp, ref Msg, ref opp, Order_Mony_Id2, long.Parse(Order_No), Bank_Name, Order_Date, Transaction_Type, Cach_Value, 0, Cach_Value, Cach_Value.ToString(), Box_Id, Last_Balance, 0, Customer_ID_Supplier, 0, Customer_ID_Supplier, Common.Customer_Account_Type, SubAccount_Id_, Notes, Balance_Before, Balance_After, Balance_BeforStr, Balance_AfterStr, SubAccountName, User_Id, Branch_Id, Factor, Factor, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);


                    CustomersAccountCLS cu = new CustomersAccountCLS();
                    //cu.Set_Customer_Balance_WithMoney_TransactionEstlam(ref Excp, ref opp, Customer_ID_Supplier, Transaction_Type, Order_Mony_Id2, Money_Transaction_Box_ID, Cach_Value, Notes);

                    cu.Set_Customer_Balance_WithMoney_Transaction(ref Excp, ref opp, Customer_ID_Supplier, Transaction_Type, Order_Mony_Id2, Money_Transaction_Box_ID, Cach_Value, Notes);
                }

                if (Transaction_Type == 2 & Reverse_Order == 0) // صرف لمورد
                {

                    GetCustomer_Balance("Supplier", Customer_ID_Supplier, ref opp, ref Balance_Before, ref Balance_After, Cach_Value_Comput);

                    InsertOrderMoneyNew(ref Excp, ref Msg, ref opp, ref Order_Mony_Id2, long.Parse(Order_No), Transaction_Type, Order_Date, Cach_Value, Currency_Id, Bank_Type_, Man_Name, Box_Id, Order_Mony_Id_Transform, Invoice_ID, Invoice_Type, User_Id, Branch_Id, Year_Financial_Period_Id, Ticket_Supplier_Id, Order_Tafkeet_Value, Order_Notes, Sandouk_Cost_Center_Tree_Id, IS_Posted_Flage, Factor, Entry_Currency_ID, Entry_Currency_Name, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);

                    decimal Last_Balance = Box_Balance - Cach_Value;
                    Money_Transaction_Box_ID = InsertMoney_Transaction_Box(ref Excp, ref Msg, ref opp, Order_Mony_Id2, long.Parse(Order_No), Bank_Name, Order_Date, Transaction_Type, 0, Cach_Value, Cach_Value, Cach_Value.ToString(), Box_Id, Last_Balance, Customer_ID_Supplier, 0, 0, Customer_ID_Supplier, Common.Supplier_Account_Type, SubAccount_Id_, Notes, Balance_Before, Balance_After, Balance_BeforStr, Balance_AfterStr, SubAccountName, User_Id, Branch_Id, Factor, Factor, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);


                    Suppliers_AccountsCls cu = new Suppliers_AccountsCls();
                    // cu.Set_Supplier_Balance_WithMoney_TransactionSarf(ref Excp, ref opp, Customer_ID_Supplier, Transaction_Type, Order_Mony_Id2, Money_Transaction_Box_ID, Cach_Value, Notes);
                    cu.Set_Supplier_Balance_WithMoney_Transaction(ref Excp, ref opp, Customer_ID_Supplier, Transaction_Type, Order_Mony_Id2, Money_Transaction_Box_ID, Cach_Value, Notes);
                }

                //////////////////////////***********************************
                if (Transaction_Type == 1 & Reverse_Order == 1)  // أستلام من المورد
                {
                    Cach_Value_Comput = Cach_Value_Comput * -1;
                    GetCustomer_Balance("Supplier", Customer_ID_Supplier, ref opp, ref Balance_Before, ref Balance_After, Cach_Value_Comput);
                    InsertOrderMoneyNew(ref Excp, ref Msg, ref opp, ref Order_Mony_Id2, long.Parse(Order_No), Transaction_Type, Order_Date, Cach_Value, Currency_Id, Bank_Type_, Man_Name, Box_Id, Order_Mony_Id_Transform, Invoice_ID, Invoice_Type, User_Id, Branch_Id, Year_Financial_Period_Id, Ticket_Supplier_Id, Order_Tafkeet_Value, Order_Notes, Sandouk_Cost_Center_Tree_Id, IS_Posted_Flage, Factor, Entry_Currency_ID, Entry_Currency_Name, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);

                    decimal Last_Balance = Box_Balance + Cach_Value;
                    Money_Transaction_Box_ID = InsertMoney_Transaction_Box(ref Excp, ref Msg, ref opp, Order_Mony_Id2, long.Parse(Order_No), Bank_Name, Order_Date, Transaction_Type, Cach_Value, 0, Cach_Value, Cach_Value.ToString(), Box_Id, Last_Balance, Customer_ID_Supplier, 0, 0, Customer_ID_Supplier, Common.Supplier_Account_Type, SubAccount_Id_, Notes, Balance_Before, Balance_After, Balance_BeforStr, Balance_AfterStr, SubAccountName, User_Id, Branch_Id, Factor, Factor, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);

                    Suppliers_AccountsCls cu = new Suppliers_AccountsCls();
                    //cu.Set_Supplier_Balance_WithMoney_TransactionEstlam(ref Excp, ref opp, Customer_ID_Supplier, Transaction_Type, Order_Mony_Id2, Money_Transaction_Box_ID, Cach_Value, Notes);

                    cu.Set_Supplier_Balance_WithMoney_Transaction(ref Excp, ref opp, Customer_ID_Supplier, Transaction_Type, Order_Mony_Id2, Money_Transaction_Box_ID, Cach_Value, Notes);
                }

                if (Transaction_Type == 2 & Reverse_Order == 1)   // صرف للعملاء
                {
                    Cach_Value_Comput = Cach_Value_Comput * -1;
                    GetCustomer_Balance("Customer", Customer_ID_Supplier, ref opp, ref Balance_Before, ref Balance_After, Cach_Value_Comput);
                    InsertOrderMoneyNew(ref Excp, ref Msg, ref opp, ref Order_Mony_Id2, long.Parse(Order_No), Transaction_Type, Order_Date, Cach_Value, Currency_Id, Bank_Type_, Man_Name, Box_Id, Order_Mony_Id_Transform, Invoice_ID, Invoice_Type, User_Id, Branch_Id, Year_Financial_Period_Id, Ticket_Supplier_Id, Order_Tafkeet_Value, Order_Notes, Sandouk_Cost_Center_Tree_Id, IS_Posted_Flage, Factor, Entry_Currency_ID, Entry_Currency_Name, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);

                    decimal Last_Balance = Box_Balance - Cach_Value;
                    Money_Transaction_Box_ID = InsertMoney_Transaction_Box(ref Excp, ref Msg, ref opp, Order_Mony_Id2, long.Parse(Order_No), Bank_Name, Order_Date, Transaction_Type, 0, Cach_Value, Cach_Value, Cach_Value.ToString(), Box_Id, Last_Balance, 0, Customer_ID_Supplier, 0, Customer_ID_Supplier, Common.Customer_Account_Type, SubAccount_Id_, Notes, Balance_Before, Balance_After, Balance_BeforStr, Balance_AfterStr, SubAccountName, User_Id, Branch_Id, Factor, Factor, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);



                    CustomersAccountCLS cu = new CustomersAccountCLS();
                    //cu.Set_Customer_Balance_WithMoney_TransactionSarf(ref Excp, ref opp, Customer_ID_Supplier, Transaction_Type, Order_Mony_Id2, Money_Transaction_Box_ID, Cach_Value, Notes);
                    cu.Set_Customer_Balance_WithMoney_Transaction(ref Excp, ref opp, Customer_ID_Supplier, Transaction_Type, Order_Mony_Id2, Money_Transaction_Box_ID, Cach_Value, Notes);

                }
                // Keep a failure recorded by the inserts above (they set Excp=1 and Msg); it used to be reset here so a missing order/box row was reported as success.
                if (Excp == 1 && !string.IsNullOrEmpty(Msg)) { return; }
                Excp = 0;

                decimal Box_BalanceBeforAction = GetBank_Balance(ref opp, Box_Id, true);
                // Money_Transaction_BoxOrderReportCLS mo = new Money_Transaction_BoxOrderReportCLS();
                Set_BankBalanceDay(ref opp, Order_Date.Date, Box_Id, Box_BalanceBeforAction, Cach_Value, Transaction_Type);




                Accounts_Entry_POST_CLS aa = new Accounts_Entry_POST_CLS();
                aa.Make_Entry_For_AccountsSandouk(ref Excp, ref Msg, ref opp, Currency_Id, Entry_Currency_Name, Factor, Order_Mony_Id2, Entry_No);

            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
            }



        }
        public void Make_Transaction_BanksBox_FromInvoices(ref int Excp, ref string Msg, ref operations opp, Money_Transaction_BoxVM mtVM)
        {
            long Order_Mony_Id2 = mtVM.Order_Mony_Id;
            long Money_Transaction_Box_ID = mtVM.Money_Transaction_Box_ID;
            int Transaction_Type = mtVM.Transaction_Type;
            string Order_No = mtVM.Order_No;
            long Box_Id = mtVM.Box_Id;
            decimal Box_Balance = mtVM.Box_Balance;
            decimal Cach_Value = mtVM.Cach_Value;
            DateTime Order_Date = mtVM.Order_Date;
            string Man_Name = mtVM.Man_Name;
            long    Currency_Id = mtVM.Currency_Id;
            long Customer_ID_Supplier = mtVM.Customer_ID_Supplier;
            int Reverse_Order = mtVM.Reverse_Order;
            long Entry_No = mtVM.Entry_No;
            long CarsWorkOrdersId = mtVM.CarsWorkOrdersId;
            long Reference_Type_Table_Id = mtVM.Reference_Type_Table_Id;
            long DataDictionaryProcessesListsId = mtVM.DataDictionaryProcessesListsId;


            long   User_Id = Common.User_Id;
            long Branch_Id = Common.Branch_Id;


            string mysql = "";

            decimal Cach_Value_Comput = Cach_Value;
            long Entry_Currency_ID = 1;
            string Bank_Name = "";
            //Bank bnk = GetBank_Balance(Box_Id, ref opp, ref Box_Balance);
            Bank bnk = opp.db.Banks.AsNoTracking().Where(b => b.Id == Box_Id).FirstOrDefault();
            if (bnk != null)
            {
                Entry_Currency_ID = bnk.Currency_Id ?? 1;
                Bank_Name = bnk.Bank_Name;
            }
            string Entry_Currency_Name = "جنية مصرى";
            int Ticket_Supplier_Id = 0;
            int Sandouk_Cost_Center_Tree_Id = 0;
            int IS_Posted_Flage = 0;
            string Order_Notes = Man_Name;
            Notes = Man_Name;
            string Order_Tafkeet_Value = TafkeetProj.NumberToText.GetText(Cach_Value, Entry_Currency_ID);
            int Bank_Type_ = 0;
            int Order_Mony_Id_Transform = -1;
            long Year_Financial_Period_Id = Financial_PeriodCLS.Get_Year_Financial_Period_Id(Order_Date);


            try
            {


                long B_Id = 0;

                Order_Mony_Id2 = B_Id;
                Money_Transaction_Box_ID = 0;

                //B_Idx= Insert_Order_Money(ref   opp, Order_No, Order_Date, Transaction_Type, Cach_Value, Currency_Id, 0, Box_Id, Man_Name, 0, 0, 0, 0, 0, 0, "", "", 0, 1.0, 1, "جنية مصرى");
                if (Transaction_Type == 1 & Reverse_Order == 0)  //استلام من عميل
                {

                    GetCustomer_Balance("Customer", Customer_ID_Supplier, ref opp, ref Balance_Before, ref Balance_After, Cach_Value_Comput);


                    InsertOrderMoneyNew(ref Excp, ref Msg, ref opp, ref Order_Mony_Id2, long.Parse(Order_No), Transaction_Type, Order_Date, Cach_Value, Currency_Id, Bank_Type_, Man_Name, Box_Id, Order_Mony_Id_Transform, Invoice_ID, Invoice_Type, User_Id, Branch_Id, Year_Financial_Period_Id, Ticket_Supplier_Id, Order_Tafkeet_Value, Order_Notes, Sandouk_Cost_Center_Tree_Id, IS_Posted_Flage, Factor, Entry_Currency_ID, Entry_Currency_Name, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);

                    decimal Last_Balance = Box_Balance + Cach_Value;

                    Money_Transaction_Box_ID = InsertMoney_Transaction_Box(ref Excp, ref Msg, ref opp, Order_Mony_Id2, long.Parse(Order_No), Bank_Name, Order_Date, Transaction_Type, Cach_Value, 0, Cach_Value, Cach_Value.ToString(), Box_Id, Last_Balance, 0, Customer_ID_Supplier, 0, Customer_ID_Supplier, Common.Customer_Account_Type, SubAccount_Id_, Notes, Balance_Before, Balance_After, Balance_BeforStr, Balance_AfterStr, SubAccountName, User_Id, Branch_Id, Factor, Factor, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);


                    CustomersAccountCLS cu = new CustomersAccountCLS();
                    //cu.Set_Customer_Balance_WithMoney_TransactionEstlam(ref Excp, ref opp, Customer_ID_Supplier, Transaction_Type, Order_Mony_Id2, Money_Transaction_Box_ID, Cach_Value, Notes);

                    cu.Set_Customer_Balance_WithMoney_Transaction(ref Excp, ref opp, Customer_ID_Supplier, Transaction_Type, Order_Mony_Id2, Money_Transaction_Box_ID, Cach_Value, Notes);
                }

                if (Transaction_Type == 2 & Reverse_Order == 0) // صرف لمورد
                {

                    GetCustomer_Balance("Supplier", Customer_ID_Supplier, ref opp, ref Balance_Before, ref Balance_After, Cach_Value_Comput);

                    InsertOrderMoneyNew(ref Excp, ref Msg, ref opp, ref Order_Mony_Id2, long.Parse(Order_No), Transaction_Type, Order_Date, Cach_Value, Currency_Id, Bank_Type_, Man_Name, Box_Id, Order_Mony_Id_Transform, Invoice_ID, Invoice_Type, User_Id, Branch_Id, Year_Financial_Period_Id, Ticket_Supplier_Id, Order_Tafkeet_Value, Order_Notes, Sandouk_Cost_Center_Tree_Id, IS_Posted_Flage, Factor, Entry_Currency_ID, Entry_Currency_Name, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);

                    decimal Last_Balance = Box_Balance - Cach_Value;
                    Money_Transaction_Box_ID = InsertMoney_Transaction_Box(ref Excp, ref Msg, ref opp, Order_Mony_Id2, long.Parse(Order_No), Bank_Name, Order_Date, Transaction_Type, 0, Cach_Value, Cach_Value, Cach_Value.ToString(), Box_Id, Last_Balance, Customer_ID_Supplier, 0, 0, Customer_ID_Supplier, Common.Supplier_Account_Type, SubAccount_Id_, Notes, Balance_Before, Balance_After, Balance_BeforStr, Balance_AfterStr, SubAccountName, User_Id, Branch_Id, Factor, Factor, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);


                    Suppliers_AccountsCls cu = new Suppliers_AccountsCls();
                    // cu.Set_Supplier_Balance_WithMoney_TransactionSarf(ref Excp, ref opp, Customer_ID_Supplier, Transaction_Type, Order_Mony_Id2, Money_Transaction_Box_ID, Cach_Value, Notes);
                    cu.Set_Supplier_Balance_WithMoney_Transaction(ref Excp, ref opp, Customer_ID_Supplier, Transaction_Type, Order_Mony_Id2, Money_Transaction_Box_ID, Cach_Value, Notes);
                }

                //////////////////////////***********************************
                if (Transaction_Type == 1 & Reverse_Order == 1)  // أستلام من المورد
                {
                    Cach_Value_Comput = Cach_Value_Comput * -1;
                    GetCustomer_Balance("Supplier", Customer_ID_Supplier, ref opp, ref Balance_Before, ref Balance_After, Cach_Value_Comput);
                    InsertOrderMoneyNew(ref Excp, ref Msg, ref opp, ref Order_Mony_Id2, long.Parse(Order_No), Transaction_Type, Order_Date, Cach_Value, Currency_Id, Bank_Type_, Man_Name, Box_Id, Order_Mony_Id_Transform, Invoice_ID, Invoice_Type, User_Id, Branch_Id, Year_Financial_Period_Id, Ticket_Supplier_Id, Order_Tafkeet_Value, Order_Notes, Sandouk_Cost_Center_Tree_Id, IS_Posted_Flage, Factor, Entry_Currency_ID, Entry_Currency_Name, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);

                    decimal Last_Balance = Box_Balance + Cach_Value;
                    Money_Transaction_Box_ID = InsertMoney_Transaction_Box(ref Excp, ref Msg, ref opp, Order_Mony_Id2, long.Parse(Order_No), Bank_Name, Order_Date, Transaction_Type, Cach_Value, 0, Cach_Value, Cach_Value.ToString(), Box_Id, Last_Balance, Customer_ID_Supplier, 0, 0, Customer_ID_Supplier, Common.Supplier_Account_Type, SubAccount_Id_, Notes, Balance_Before, Balance_After, Balance_BeforStr, Balance_AfterStr, SubAccountName, User_Id, Branch_Id, Factor, Factor, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);

                    Suppliers_AccountsCls cu = new Suppliers_AccountsCls();
                    //cu.Set_Supplier_Balance_WithMoney_TransactionEstlam(ref Excp, ref opp, Customer_ID_Supplier, Transaction_Type, Order_Mony_Id2, Money_Transaction_Box_ID, Cach_Value, Notes);

                    cu.Set_Supplier_Balance_WithMoney_Transaction(ref Excp, ref opp, Customer_ID_Supplier, Transaction_Type, Order_Mony_Id2, Money_Transaction_Box_ID, Cach_Value, Notes);
                }

                if (Transaction_Type == 2 & Reverse_Order == 1)   // صرف للعملاء
                {
                    Cach_Value_Comput = Cach_Value_Comput * -1;
                    GetCustomer_Balance("Customer", Customer_ID_Supplier, ref opp, ref Balance_Before, ref Balance_After, Cach_Value_Comput);
                    InsertOrderMoneyNew(ref Excp, ref Msg, ref opp, ref Order_Mony_Id2, long.Parse(Order_No), Transaction_Type, Order_Date, Cach_Value, Currency_Id, Bank_Type_, Man_Name, Box_Id, Order_Mony_Id_Transform, Invoice_ID, Invoice_Type, User_Id, Branch_Id, Year_Financial_Period_Id, Ticket_Supplier_Id, Order_Tafkeet_Value, Order_Notes, Sandouk_Cost_Center_Tree_Id, IS_Posted_Flage, Factor, Entry_Currency_ID, Entry_Currency_Name, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);

                    decimal Last_Balance = Box_Balance - Cach_Value;
                    Money_Transaction_Box_ID = InsertMoney_Transaction_Box(ref Excp, ref Msg, ref opp, Order_Mony_Id2, long.Parse(Order_No), Bank_Name, Order_Date, Transaction_Type, 0, Cach_Value, Cach_Value, Cach_Value.ToString(), Box_Id, Last_Balance, 0, Customer_ID_Supplier, 0, Customer_ID_Supplier, Common.Customer_Account_Type, SubAccount_Id_, Notes, Balance_Before, Balance_After, Balance_BeforStr, Balance_AfterStr, SubAccountName, User_Id, Branch_Id, Factor, Factor, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);



                    CustomersAccountCLS cu = new CustomersAccountCLS();
                    //cu.Set_Customer_Balance_WithMoney_TransactionSarf(ref Excp, ref opp, Customer_ID_Supplier, Transaction_Type, Order_Mony_Id2, Money_Transaction_Box_ID, Cach_Value, Notes);
                    cu.Set_Customer_Balance_WithMoney_Transaction(ref Excp, ref opp, Customer_ID_Supplier, Transaction_Type, Order_Mony_Id2, Money_Transaction_Box_ID, Cach_Value, Notes);

                }

                mtVM.Order_Mony_Id = Order_Mony_Id2;
                mtVM.Money_Transaction_Box_ID = Money_Transaction_Box_ID;
                Excp = 0;

                decimal Box_BalanceBeforAction = GetBank_Balance(ref opp, Box_Id, true);
                // Money_Transaction_BoxOrderReportCLS mo = new Money_Transaction_BoxOrderReportCLS();
                Set_BankBalanceDay(ref opp, Order_Date.Date, Box_Id, Box_BalanceBeforAction, Cach_Value, Transaction_Type);




                Accounts_Entry_POST_CLS aa = new Accounts_Entry_POST_CLS();
                aa.Make_Entry_For_AccountsSandouk(ref Excp, ref Msg, ref opp, Currency_Id, Entry_Currency_Name, Factor, Order_Mony_Id2, Entry_No);

            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
            }



        }

        public void Make_Transaction_BanksBox_FromMultiInvoices(ref int Excp, ref string Msg, ref operations opp, Money_Transaction_BoxVM mtVM)
        {
            long Order_Mony_Id2 = mtVM.Order_Mony_Id;
            long Money_Transaction_Box_ID = mtVM.Money_Transaction_Box_ID;
            int Transaction_Type = mtVM.Transaction_Type;
            string Order_No = mtVM.Order_No;
            long Box_Id = mtVM.Box_Id;
            decimal Box_Balance = mtVM.Box_Balance;
            decimal Cach_Value = mtVM.Cach_Value;
            DateTime Order_Date = mtVM.Order_Date;
            string Man_Name = mtVM.Man_Name;
            long    Currency_Id = mtVM.Currency_Id;
            long Customer_ID_Supplier = mtVM.Customer_ID_Supplier;
            int Reverse_Order = mtVM.Reverse_Order;
            long Entry_No = mtVM.Entry_No;
            long CarsWorkOrdersId = mtVM.CarsWorkOrdersId;
            long Reference_Type_Table_Id = mtVM.Reference_Type_Table_Id;
            long DataDictionaryProcessesListsId = mtVM.DataDictionaryProcessesListsId;


            long   User_Id = Common.User_Id;
            long Branch_Id = Common.Branch_Id;


            string mysql = "";

            decimal Cach_Value_Comput = Cach_Value;
            long Entry_Currency_ID = 1;
            string Bank_Name = "";
            //Bank bnk = GetBank_Balance(Box_Id, ref opp, ref Box_Balance);
            Bank bnk = opp.db.Banks.AsNoTracking().Where(b => b.Id == Box_Id).FirstOrDefault();
            if (bnk != null)
            {
                Entry_Currency_ID = bnk.Currency_Id ?? 1;
                Bank_Name = bnk.Bank_Name;
            }
            string Entry_Currency_Name = "جنية مصرى";
            long Ticket_Supplier_Id = 0;
            long Sandouk_Cost_Center_Tree_Id = 0;
            int IS_Posted_Flage = 0;
            string Order_Notes = Man_Name;
            Notes = Man_Name;
            string Order_Tafkeet_Value = TafkeetProj.NumberToText.GetText(Cach_Value, Entry_Currency_ID);
            int Bank_Type_ = 0;
            int Order_Mony_Id_Transform = -1;
            long Year_Financial_Period_Id = Financial_PeriodCLS.Get_Year_Financial_Period_Id(Order_Date);


            try
            {


                long B_Id = 0;

                Order_Mony_Id2 = B_Id;
                Money_Transaction_Box_ID = 0;

                //B_Idx= Insert_Order_Money(ref   opp, Order_No, Order_Date, Transaction_Type, Cach_Value, Currency_Id, 0, Box_Id, Man_Name, 0, 0, 0, 0, 0, 0, "", "", 0, 1.0, 1, "جنية مصرى");
                if (Transaction_Type == 1 & Reverse_Order == 0)  //استلام من عميل
                {

                    GetCustomer_Balance("Customer", Customer_ID_Supplier, ref opp, ref Balance_Before, ref Balance_After, Cach_Value_Comput);


                    InsertOrderMoneyNew(ref Excp, ref Msg, ref opp, ref Order_Mony_Id2, long.Parse(Order_No), Transaction_Type, Order_Date, Cach_Value, Currency_Id, Bank_Type_, Man_Name, Box_Id, Order_Mony_Id_Transform, Invoice_ID, Invoice_Type, User_Id, Branch_Id, Year_Financial_Period_Id, Ticket_Supplier_Id, Order_Tafkeet_Value, Order_Notes, Sandouk_Cost_Center_Tree_Id, IS_Posted_Flage, Factor, Entry_Currency_ID, Entry_Currency_Name, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);

                    decimal Last_Balance = Box_Balance + Cach_Value;

                    Money_Transaction_Box_ID = InsertMoney_Transaction_Box(ref Excp, ref Msg, ref opp, Order_Mony_Id2, long.Parse(Order_No), Bank_Name, Order_Date, Transaction_Type, Cach_Value, 0, Cach_Value, Cach_Value.ToString(), Box_Id, Last_Balance, 0, Customer_ID_Supplier, 0, Customer_ID_Supplier, Common.Customer_Account_Type, SubAccount_Id_, Notes, Balance_Before, Balance_After, Balance_BeforStr, Balance_AfterStr, SubAccountName, User_Id, Branch_Id, Factor, Factor, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);


                    CustomersAccountCLS cu = new CustomersAccountCLS();
                    //cu.Set_Customer_Balance_WithMoney_TransactionEstlam(ref Excp, ref opp, Customer_ID_Supplier, Transaction_Type, Order_Mony_Id2, Money_Transaction_Box_ID, Cach_Value, Notes);

                    cu.Set_Customer_Balance_WithMoney_Transaction(ref Excp, ref opp, Customer_ID_Supplier, Transaction_Type, Order_Mony_Id2, Money_Transaction_Box_ID, Cach_Value, Notes);
                }

                if (Transaction_Type == 2 & Reverse_Order == 0) // صرف لمورد
                {

                    GetCustomer_Balance("Supplier", Customer_ID_Supplier, ref opp, ref Balance_Before, ref Balance_After, Cach_Value_Comput);

                    InsertOrderMoneyNew(ref Excp, ref Msg, ref opp, ref Order_Mony_Id2, long.Parse(Order_No), Transaction_Type, Order_Date, Cach_Value, Currency_Id, Bank_Type_, Man_Name, Box_Id, Order_Mony_Id_Transform, Invoice_ID, Invoice_Type, User_Id, Branch_Id, Year_Financial_Period_Id, Ticket_Supplier_Id, Order_Tafkeet_Value, Order_Notes, Sandouk_Cost_Center_Tree_Id, IS_Posted_Flage, Factor, Entry_Currency_ID, Entry_Currency_Name, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);

                    decimal Last_Balance = Box_Balance - Cach_Value;
                    Money_Transaction_Box_ID = InsertMoney_Transaction_Box(ref Excp, ref Msg, ref opp, Order_Mony_Id2, long.Parse(Order_No), Bank_Name, Order_Date, Transaction_Type, 0, Cach_Value, Cach_Value, Cach_Value.ToString(), Box_Id, Last_Balance, Customer_ID_Supplier, 0, 0, Customer_ID_Supplier, Common.Supplier_Account_Type, SubAccount_Id_, Notes, Balance_Before, Balance_After, Balance_BeforStr, Balance_AfterStr, SubAccountName, User_Id, Branch_Id, Factor, Factor, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);


                    Suppliers_AccountsCls cu = new Suppliers_AccountsCls();
                    // cu.Set_Supplier_Balance_WithMoney_TransactionSarf(ref Excp, ref opp, Customer_ID_Supplier, Transaction_Type, Order_Mony_Id2, Money_Transaction_Box_ID, Cach_Value, Notes);
                    cu.Set_Supplier_Balance_WithMoney_Transaction(ref Excp, ref opp, Customer_ID_Supplier, Transaction_Type, Order_Mony_Id2, Money_Transaction_Box_ID, Cach_Value, Notes);
                }

                //////////////////////////***********************************
                if (Transaction_Type == 1 & Reverse_Order == 1)  // أستلام من المورد
                {
                    Cach_Value_Comput = Cach_Value_Comput * -1;
                    GetCustomer_Balance("Supplier", Customer_ID_Supplier, ref opp, ref Balance_Before, ref Balance_After, Cach_Value_Comput);
                    InsertOrderMoneyNew(ref Excp, ref Msg, ref opp, ref Order_Mony_Id2, long.Parse(Order_No), Transaction_Type, Order_Date, Cach_Value, Currency_Id, Bank_Type_, Man_Name, Box_Id, Order_Mony_Id_Transform, Invoice_ID, Invoice_Type, User_Id, Branch_Id, Year_Financial_Period_Id, Ticket_Supplier_Id, Order_Tafkeet_Value, Order_Notes, Sandouk_Cost_Center_Tree_Id, IS_Posted_Flage, Factor, Entry_Currency_ID, Entry_Currency_Name, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);

                    decimal Last_Balance = Box_Balance + Cach_Value;
                    Money_Transaction_Box_ID = InsertMoney_Transaction_Box(ref Excp, ref Msg, ref opp, Order_Mony_Id2, long.Parse(Order_No), Bank_Name, Order_Date, Transaction_Type, Cach_Value, 0, Cach_Value, Cach_Value.ToString(), Box_Id, Last_Balance, Customer_ID_Supplier, 0, 0, Customer_ID_Supplier, Common.Supplier_Account_Type, SubAccount_Id_, Notes, Balance_Before, Balance_After, Balance_BeforStr, Balance_AfterStr, SubAccountName, User_Id, Branch_Id, Factor, Factor, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);

                    Suppliers_AccountsCls cu = new Suppliers_AccountsCls();
                    //cu.Set_Supplier_Balance_WithMoney_TransactionEstlam(ref Excp, ref opp, Customer_ID_Supplier, Transaction_Type, Order_Mony_Id2, Money_Transaction_Box_ID, Cach_Value, Notes);

                    cu.Set_Supplier_Balance_WithMoney_Transaction(ref Excp, ref opp, Customer_ID_Supplier, Transaction_Type, Order_Mony_Id2, Money_Transaction_Box_ID, Cach_Value, Notes);
                }

                if (Transaction_Type == 2 & Reverse_Order == 1)   // صرف للعملاء
                {
                    Cach_Value_Comput = Cach_Value_Comput * -1;
                    GetCustomer_Balance("Customer", Customer_ID_Supplier, ref opp, ref Balance_Before, ref Balance_After, Cach_Value_Comput);
                    InsertOrderMoneyNew(ref Excp, ref Msg, ref opp, ref Order_Mony_Id2, long.Parse(Order_No), Transaction_Type, Order_Date, Cach_Value, Currency_Id, Bank_Type_, Man_Name, Box_Id, Order_Mony_Id_Transform, Invoice_ID, Invoice_Type, User_Id, Branch_Id, Year_Financial_Period_Id, Ticket_Supplier_Id, Order_Tafkeet_Value, Order_Notes, Sandouk_Cost_Center_Tree_Id, IS_Posted_Flage, Factor, Entry_Currency_ID, Entry_Currency_Name, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);

                    decimal Last_Balance = Box_Balance - Cach_Value;
                    Money_Transaction_Box_ID = InsertMoney_Transaction_Box(ref Excp, ref Msg, ref opp, Order_Mony_Id2, long.Parse(Order_No), Bank_Name, Order_Date, Transaction_Type, 0, Cach_Value, Cach_Value, Cach_Value.ToString(), Box_Id, Last_Balance, 0, Customer_ID_Supplier, 0, Customer_ID_Supplier, Common.Customer_Account_Type, SubAccount_Id_, Notes, Balance_Before, Balance_After, Balance_BeforStr, Balance_AfterStr, SubAccountName, User_Id, Branch_Id, Factor, Factor, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);



                    CustomersAccountCLS cu = new CustomersAccountCLS();
                    //cu.Set_Customer_Balance_WithMoney_TransactionSarf(ref Excp, ref opp, Customer_ID_Supplier, Transaction_Type, Order_Mony_Id2, Money_Transaction_Box_ID, Cach_Value, Notes);
                    cu.Set_Customer_Balance_WithMoney_Transaction(ref Excp, ref opp, Customer_ID_Supplier, Transaction_Type, Order_Mony_Id2, Money_Transaction_Box_ID, Cach_Value, Notes);

                }

                mtVM.Order_Mony_Id = Order_Mony_Id2;
                mtVM.Money_Transaction_Box_ID = Money_Transaction_Box_ID;
                Excp = 0;

                decimal Box_BalanceBeforAction = GetBank_Balance(ref opp, Box_Id, true);
                // Money_Transaction_BoxOrderReportCLS mo = new Money_Transaction_BoxOrderReportCLS();
                Set_BankBalanceDay(ref opp, Order_Date.Date, Box_Id, Box_BalanceBeforAction, Cach_Value, Transaction_Type);




                Accounts_Entry_POST_CLS aa = new Accounts_Entry_POST_CLS();
                aa.Make_Entry_For_AccountsSandouk(ref Excp, ref Msg, ref opp, Currency_Id, Entry_Currency_Name, Factor, Order_Mony_Id2, Entry_No);

            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
            }



        }


        public void Make_Transaction_BanksBox_FromInvoicesMulti(ref int Excp, ref string Msg, ref operations opp, ref long MainOrder_MoneyId, Order_Money omMain, List<Money_Transaction_Box> mbxLst_, long Customer_ID_Supplier)
        {

            long Order_Mony_Id2 = 0;


            DateTime Order_Date = omMain.Order_Date;
            int Transaction_Type = omMain.Transaction_type;
            long Account_Id = omMain.Account_Id ?? 0;
            int Account_Type = omMain.Account_Type ?? 0;

            Money_Transaction_BoxCLS mxCLS = new Money_Transaction_BoxCLS();
            //Invoices_ListCls invCls = new Invoices_ListCls();
            //Invoices_List invL = invCls.GetInvoices_ListById(ref opp, omMain.Invoice_Id ?? 0);
            //if (invL != null)
            //{
            //    Invoices_List_Id = invL.Id;
            //    Invoice_No = invL.Invoice_Id.Value.ToString();
            //}
            //CarsWorkOrdersCLS cwCls = new CarsWorkOrdersCLS(ref opp);
            //CarsWorkOrder carW = cwCls.geCarWorkOrder(omMain.CarsWorkOrdersId ?? 0);
            //if (invL != null)
            //{
            //    WorkOrderNumber = carW.WorkOrderNumber.Value.ToString();
            //}



            mbxLst_ = mbxLst_.OrderByDescending(d => d.IsDefault_Bank).ToList();
            try

            {
                foreach (var item in mbxLst_)
                {
                    decimal Cach_Value = item.Transaction_Value ?? 0;
                    if (Cach_Value == 0)
                    {
                        continue;
                    }


                    //SetOrderMoney
                    Order_MoneyCLS om = new Order_MoneyCLS();
                    long Bank_Id = item.Bank_Id;
                    decimal Box_Balance = 0;
                    long Entry_Currency_ID = 1;
                    string Entry_Currency_Name = "جنية مصرى";
                    Bank bnk = GetBank_Balance(Bank_Id, ref opp, ref Box_Balance);
                    //if (bnk != null)
                    //{
                    //    Entry_Currency_ID = bnk.Currency_Id ?? 1;
                    //    Bank_Name = bnk.Bank_Name;
                    //}
                    //else
                    //{

                    //}
                    decimal Cach_Value_Comput = item.Transaction_Value ?? 0;
                    Order_Money order_Money = Get_Order_Money(MainOrder_MoneyId, bnk, omMain, item);


                    om.InsertOrder_Money(ref Excp, ref Msg, ref opp, order_Money);

                    if (Excp == 1)
                    {
                        break;
                    }
                    Order_Mony_Id2 = order_Money.Id;
                    if (MainOrder_MoneyId == 0)
                    {
                        MainOrder_MoneyId = order_Money.Id;
                    }
                    //long Money_Transaction_Box_ID;


                    Set_Transaction_Money_Box(omMain, item, bnk, MainOrder_MoneyId);
                    long ProcessNo = 0;
                    Set_TransactionMoney_For_Customer_SupplierAccountsMulti(ref Excp, ref Msg, ref opp, MainOrder_MoneyId, item, bnk, Customer_ID_Supplier);
                    if (Excp == 1)
                    {
                        break;
                    }

                }

                if (Excp == 0)
                {
                    long Entry_No = 0;
                    Accounts_Entry_POST_CLS aa = new Accounts_Entry_POST_CLS();
                    aa.IsNotDeletePrevEntry = false;
                    aa.Account_Entry_List_ID = omMain.Account_Entry_List_ID ?? 0;
                    aa.Make_Entry_For_AccountsSandoukMulti(ref Excp, ref Msg, ref opp, omMain.Currency_Id, omMain.Entry_Currency_Name, Factor, MainOrder_MoneyId, Entry_No);
                }
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
            }
        }

        public void Make_Transaction_BanksBox_FromInvoicesMulti_(ref int Excp, ref string Msg, ref operations opp, ref long MainOrder_MoneyId, Order_Money omMain, List<Money_Transaction_Box> mbxLst_, long Customer_ID_Supplier)
        {

            long Order_Mony_Id2 = 0;


            DateTime Order_Date = omMain.Order_Date;
            int Transaction_Type = omMain.Transaction_type;
            long Account_Id = omMain.Account_Id ?? 0;
            int Account_Type = omMain.Account_Type ?? 0;

            Money_Transaction_BoxCLS mxCLS = new Money_Transaction_BoxCLS();




            mbxLst_ = mbxLst_.OrderByDescending(d => d.IsDefault_Bank).ToList();
            try

            {
                foreach (var item in mbxLst_)
                {
                    decimal Cach_Value = item.Transaction_Value ?? 0;
                    if (Cach_Value == 0)
                    {
                        continue;
                    }


                    //SetOrderMoney
                    Order_MoneyCLS om = new Order_MoneyCLS();
                    long Bank_Id = item.Bank_Id;
                    decimal Box_Balance = 0;
                    long Entry_Currency_ID = 1;
                    string Entry_Currency_Name = "جنية مصرى";
                    Bank bnk = GetBank_Balance(Bank_Id, ref opp, ref Box_Balance);


                    //if (bnk != null)
                    //{
                    //    Entry_Currency_ID = bnk.Currency_Id ?? 1;
                    //    Bank_Name = bnk.Bank_Name;
                    //}
                    //else
                    //{

                    //}
                    decimal Cach_Value_Comput = item.Transaction_Value ?? 0;
                    Order_Money order_Money = Get_Order_Money(MainOrder_MoneyId, bnk, omMain, item);


                    om.InsertOrder_Money(ref Excp, ref Msg, ref opp, order_Money);

                    if (Excp == 1)
                    {
                        break;
                    }
                    Order_Mony_Id2 = order_Money.Id;
                    if (MainOrder_MoneyId == 0)
                    {
                        MainOrder_MoneyId = order_Money.Id;
                    }
                    //long Money_Transaction_Box_ID;

                    item.Order_Mony_Id = MainOrder_MoneyId;
                    item.SubOrder_Money_Id = order_Money.Id;
                    item.Order_No = order_Money.Order_No;
                    item.Bank_Name = bnk.Bank_Name;
                    mxCLS.InsertMoney_Transaction_Box_(ref Excp, ref Msg, item, ref opp);

                    if (Excp == 0)
                    {

                        // Money_Transaction_BoxOrderReportCLS mo = new Money_Transaction_BoxOrderReportCLS();
                        Set_BankBalanceDay(ref opp, item.Transaction_Date.Value, bnk.Id, Box_Balance, item.Transaction_Value.Value, Transaction_Type);
                    }

                    //Set_Transaction_Money_Box(omMain, item, bnk, MainOrder_MoneyId);
                    if (Excp == 0)
                    {
                        long ProcessNo = 0;
                        Set_TransactionMoney_For_Customer_SupplierAccountsMulti_(ref Excp, ref Msg, ref opp, MainOrder_MoneyId, item);
                    }
                    if (Excp == 1)
                    {
                        break;
                    }



                    //aa.Make_Entry_For_AccountsSandouk(ref Excp, ref Msg, ref opp, Entry_Currency_ID, Entry_Currency_Name, Factor, Order_Mony_Id2, Entry_No);

                }

                if (Excp == 0)
                {
                    long Entry_No = 0;
                    Accounts_Entry_POST_CLS aa = new Accounts_Entry_POST_CLS();
                    aa.IsNotDeletePrevEntry = false;
                    aa.Account_Entry_List_ID = omMain.Account_Entry_List_ID ?? 0;
                    aa.Make_Entry_For_AccountsSandoukMulti(ref Excp, ref Msg, ref opp, omMain.Currency_Id, omMain.Entry_Currency_Name, Factor, MainOrder_MoneyId, Entry_No);
                }
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
            }
        }


        public void Make_Transaction_BanksBox_FromAccounts(ref int Excp, ref string Msg, ref operations opp, ref long MainOrder_MoneyId, Order_Money order_Money, Money_Transaction_Box mbx, long Customer_ID_Supplier)
        {

            long Order_Mony_Id2 = 0;


            DateTime Order_Date = order_Money.Order_Date;
            int Transaction_Type = order_Money.Transaction_type;
            long Account_Id = order_Money.Account_Id ?? 0;
            int Account_Type = order_Money.Account_Type ?? 0;

            Money_Transaction_BoxCLS mxCLS = new Money_Transaction_BoxCLS();


            try

            {

                decimal Cach_Value = mbx.Transaction_Value ?? 0;
                if (Cach_Value == 0)
                {
                    return;
                }


                //SetOrderMoney
                Order_MoneyCLS om = new Order_MoneyCLS();
                long Bank_Id = mbx.Bank_Id;
                decimal Box_Balance = 0;
                long Entry_Currency_ID = 1;
                string Entry_Currency_Name = "جنية مصرى";
                Bank bnk = GetBank_Balance(Bank_Id, ref opp, ref Box_Balance);


                decimal Cach_Value_Comput = mbx.Transaction_Value ?? 0;



                //Order_Money order_Money = Get_Order_Money(MainOrder_MoneyId, bnk, omMain, mbx);


                om.InsertOrder_Money(ref Excp, ref Msg, ref opp, order_Money);

                if (Excp == 1)
                {
                    return;
                }
                Order_Mony_Id2 = order_Money.Id;
                if (MainOrder_MoneyId == 0)
                {
                    MainOrder_MoneyId = order_Money.Id;
                }
                //long Money_Transaction_Box_ID;

                mbx.Order_Mony_Id = MainOrder_MoneyId;
                mbx.SubOrder_Money_Id = order_Money.Id;
                mbx.Order_No = order_Money.Order_No;
                mbx.Bank_Name = bnk.Bank_Name;
                mxCLS.InsertMoney_Transaction_Box_(ref Excp, ref Msg, mbx, ref opp);

                if (Excp == 0)
                {

                    // Money_Transaction_BoxOrderReportCLS mo = new Money_Transaction_BoxOrderReportCLS();
                    Set_BankBalanceDay(ref opp, mbx.Transaction_Date.Value,bnk.Id, Box_Balance, mbx.Transaction_Value.Value, Transaction_Type);
                }

                //Set_Transaction_Money_Box(omMain, item, bnk, MainOrder_MoneyId);
                if (Excp == 0)
                {
                    long ProcessNo = 0;
                    Set_TransactionMoney_For_Customer_SupplierAccountsMulti_(ref Excp, ref Msg, ref opp, MainOrder_MoneyId, mbx);
                }
                if (Excp == 1)
                {
                    return;
                }



                //aa.Make_Entry_For_AccountsSandouk(ref Excp, ref Msg, ref opp, Entry_Currency_ID, Entry_Currency_Name, Factor, Order_Mony_Id2, Entry_No);



                if (Excp == 0)
                {
                    long Entry_No = 0;
                    Accounts_Entry_POST_CLS aa = new Accounts_Entry_POST_CLS();
                    aa.IsNotDeletePrevEntry = false;
                    aa.Account_Entry_List_ID = order_Money.Account_Entry_List_ID ?? 0;
                    aa.Make_Entry_For_AccountsSandoukMulti(ref Excp, ref Msg, ref opp, order_Money.Currency_Id, order_Money.Entry_Currency_Name, Factor, MainOrder_MoneyId, Entry_No);
                }
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
            }
        }

        private void Set_Transaction_Money_Box(Order_Money omMain, Money_Transaction_Box item, Bank bnk, long MainOrder_MoneyId)
        {

            string Invoice_No = "";
            string WorkOrderNumber = "";
            long Invoices_List_Id = 0;
            long Order_Mony_Id2 = 0;


            DateTime Order_Date = omMain.Order_Date;
            int Transaction_Type = omMain.Transaction_type;
            long Account_Id = omMain.Account_Id ?? 0;
            int Account_Type = omMain.Account_Type ?? 0;

            Money_Transaction_BoxCLS mxCLS = new Money_Transaction_BoxCLS();
            Invoices_ListCls invCls = new Invoices_ListCls();
            Invoices_List invL = invCls.GetInvoices_ListById(ref opp, omMain.Invoice_Id ?? 0);
            if (invL != null)
            {
                Invoices_List_Id = invL.Id;
                Invoice_No = invL.Invoice_Id.Value.ToString();
            }
            CarsWorkOrdersCLS cwCls = new CarsWorkOrdersCLS(ref opp);
            CarsWorkOrder carW = cwCls.geCarWorkOrder(omMain.CarsWorkOrdersId ?? 0);
            if (invL != null)
            {
                WorkOrderNumber = carW.WorkOrderNumber.Value.ToString();
            }


            Notes = bnk.Bank_Name;
            if (Invoice_No != "")
            {
                Notes = Notes + "   فاتورة رقم " + Invoice_No;
            }
            if (WorkOrderNumber != "")
            {
                Notes = Notes + " أمر الشغل رقم " + WorkOrderNumber;

            }
            if (omMain.Order_Notes.Trim() != "")
            {
                Notes = Notes + " " + omMain.Order_Notes;
            }

            //item.Order_Mony_Id = MainOrder_MoneyId;
            //item.SubOrder_Money_Id = order_Money.Id;
            item.Order_No = omMain.Order_No;
            item.Bank_Name = bnk.Bank_Name;



            item.Order_Mony_Id = MainOrder_MoneyId;
            item.SubOrder_Money_Id = omMain.Id;
            item.Transaction_Date = Order_Date;
            item.Transaction_Type = Transaction_Type;


            item.Transaction_Value_ = item.Transaction_Value.ToString();
            item.Bank_Id = bnk.Id;


            item.BankAccount_Id = bnk.SubAccount_Id;
            item.BankSubAccount_Id = bnk.SubAccount_Id;

            item.Account_Id = Customer_ID_Supplier;
            item.Account_Type = Common.Customer_Account_Type;
            item.SubAccount_Id = SubAccount_Id_;


            item.Notes = Notes;
            item.Balance_Before = Balance_Before;
            item.Balance_After = Balance_After;
            item.Balance_BeforStr = Balance_BeforStr;
            item.Balance_AfterStr = Balance_AfterStr;


            item.User_Id = Common.User_Id;
            item.Branch_Id = Common.Branch_Id;
            item.Equivalent_Value = Factor;
            item.Factor = Factor;
            ///////////////////
            ///


            //item.Invoice_TypeId = Common.Invoice_Type_Id;
            //item.CarsWorkOrdersId = CarsWorkOrdersId;
            //item.Import_Value = Cach_Value;
            //item.Export_Value = 0;
            //item.Bank_Balance = Last_Balance;

            //item.CarsId = carw.CarsId;
            //item.CarNumber = carw.Car.CarNumber;
            //item.WorkOrderNumber = carw.WorkOrderNumber;
            //item.Account_Name = invList.CustomerName;
            //Invoices_List_Id;
            item.Invoice_TypeId = omMain.Invoice_Type;
            item.Invoice_Id = omMain.Invoice_Id;
            ////////////////////////////

            item.DestructionSales = 0;
            item.Order_mony_id_transform = -1;
            item.Account_Cost_Center_Tree_Id = 0;
            item.TotalValueWillPaid = 0;
            item.Rest_Value = 0;
            item.Account_Code = "";
            item.Cost_Center = "";
            item.CostCenter_Name = "";
            item.Purchase_Ticket_Value = 0;
            item.Commission_Value = 0;
            item.Entry_Currency_ID = 0;
            item.Tafkeet_Value = TafkeetProj.NumberToText.GetText(item.Transaction_Value.Value, 1);

            item.Lookup_CashPaymentMethodId = Lookup_CashPaymentMethodId;
            item.Lookup_CashPaymentMethodName = Lookup_CashPaymentMethodName;

        }

        private Order_Money Get_Order_Money(long MainOrder_MoneyId, Bank bnk, Order_Money omMain, Money_Transaction_Box item)
        {
            Order_Money order_Money = new Order_Money();
            //decimal Cach_Value = item.Transaction_Value ?? 0;
            order_Money.Factor = item.Factor ?? 1;
            order_Money.Total_Value = item.Transaction_Value ?? 0;
            //item.Factor = item.Factor ?? 1;



            //decimal Cach_Value_Comput = item.Transaction_Value ?? 0;
            //decimal Box_Balance = 0;
            //long Entry_Currency_ID = 1;
            //string Bank_Name = "";
            //long  Bank_Id = (int)item.Bank_Id;
            //Bank bnk = GetBank_Balance(Bank_Id, ref opp, ref Box_Balance);
            //if (bnk != null)
            //{
            //    Entry_Currency_ID = bnk.Currency_Id ?? 1;
            //    Bank_Name = bnk.Bank_Name;
            //}
            //string Entry_Currency_Name = "جنية مصرى";


            string Order_Tafkeet_Value = TafkeetProj.NumberToText.GetText(item.Transaction_Value ?? 0, omMain.Currency_Id);

            //long Year_Financial_Period_Id = Financial_PeriodCLS.Get_Year_Financial_Period_Id(omMain.Order_Date);
            order_Money.User_Id = omMain.User_Id;
            order_Money.Branch_Id = omMain.Branch_Id;


            order_Money.Order_No = omMain.Order_No;
            order_Money.Transaction_type = omMain.Transaction_type;
            order_Money.Order_Date = omMain.Order_Date;

            order_Money.Man_Name = omMain.Man_Name;
            order_Money.Bank_Id = omMain.Bank_Id;
            order_Money.Currency_Id = omMain.Currency_Id;
            order_Money.Order_Notes = omMain.Order_Notes;

            order_Money.CarsWorkOrdersId = omMain.CarsWorkOrdersId;
            order_Money.Invoice_Type = omMain.Invoice_Type;
            order_Money.Invoice_Id = omMain.Invoice_Id;


            order_Money.MainOrder_MoneyId = MainOrder_MoneyId;
            order_Money.Reverse_Order = omMain.Reverse_Order;
            order_Money.Account_Type = omMain.Account_Type;
            order_Money.Account_Id = omMain.Account_Id;
            order_Money.RequirementValue = omMain.RequirementValue;
            order_Money.StillValue = omMain.StillValue;
            order_Money.paidTotal = omMain.paidTotal;
            order_Money.IsMoreTheOneBoxShow = omMain.IsMoreTheOneBoxShow;
            order_Money.Day_Registration_flage = omMain.Day_Registration_flage ?? 0;

            return order_Money;
        }

        public void Edit_Transaction_BanksBox_NewSarfMulti_(ref int Excp, ref string Msg, ref operations opp, ref long Order_Mony_Id2, List<Money_Transaction_Box> arr_IdDelet)
        {
            // int Account_Type_Value, int SubAccount_Customer_Supplier_Id  decimal Cach_Value
            //int Account_Cost_Center_Tree_Id, ref long Order_Mony_Id2, int Transaction_Type
            try
            {
                //Edit_Order_Money(ref   opp,Order_Mony_Id2, long.Parse(Order_No), Order_Date, Transaction_Type, 0, Box_Currency_Id, 0, Box_Id, Man_Name, 0, 0, 0, 0, 0, 0, Order_Tafkeet_Value, Order_Notes, Sandouk_Cost_Center_Tree_Id, Factor, Entry_Currency_ID, Entry_Currency_Name);
                //Edit_Order_Money(ref opp, Order_Mony_Id2, long.Parse(Order_No), Order_Date, Transaction_Type, Man_Name, Order_Tafkeet_Value, Order_Notes, Factor);
                Order_MoneyCLS om = new Order_MoneyCLS();
                om.InsertOrder_Money(ref Excp, ref Msg, ref opp, omMain);
                long ProcessNo = omMain.ProcessNo ?? 0;
                long Money_Transaction_Box_ID = 0;
                foreach (var item in mbxList)
                {
                    Money_Transaction_Box_ID = item.Id;

                    int Id_Test = item.Is_ProcessedFlage ?? 0;// long.Parse(dt_Order.Rows[x]["Id_Test"].ToString());

                    if (Id_Test == 0) ///سطر بدون تعديل اى من القيد قبل التعديلات
                    {
                        //
                        //continue;
                    }
                    int Transaction_Type = item.Transaction_Type.Value;

                    //
                    decimal Cach_Value = item.Transaction_Value ?? 0;// decimal.Parse(dt_Order.Rows[x]["Transaction_Value"].ToString());

                    decimal Cach_Value_Comput = Cach_Value;

                    long Bank_Id = item.Bank_Id;
                    decimal Box_Balance = 0;
                    //long Entry_Currency_ID = 1;
                    //string Entry_Currency_Name = "جنية مصرى";
                    Bank bnk = GetBank_Balance(Bank_Id, ref opp, ref Box_Balance);
                    if (Id_Test == 1) //   اضافة على الاذن
                    {
                        Order_Money order_Money = Get_Order_Money(Order_Mony_Id2, bnk, omMain, item);
                        item.Order_No = omMain.Order_No;
                        item.Bank_Name = bnk.Bank_Name;

                        Money_Transaction_BoxCLS mxCLS = new Money_Transaction_BoxCLS();
                        mxCLS.InsertMoney_Transaction_Box_(ref Excp, ref Msg, item, ref opp);
                        if (Excp == 0)
                        {

                            // Money_Transaction_BoxOrderReportCLS mo = new Money_Transaction_BoxOrderReportCLS();
                            Set_BankBalanceDay(ref opp, item.Transaction_Date.Value, bnk.Id, Box_Balance, item.Transaction_Value.Value, Transaction_Type);
                        }
                    }
                    if (Id_Test == 2) //تعديل
                    {
                        Money_Transaction_Box_ID = item.Id;// long.Parse(dt_Order.Rows[x]["Id"].ToString());
                        decimal Transaction_Value_Old = 0;
                        decimal DiffValue = 0;
                        string mysql = "Select * from Money_Transaction_Box Where Id=" + Money_Transaction_Box_ID;
                        DataTable dt = opp.return_DataTable(mysql);

                        if (dt.Rows.Count > 0)
                        {
                            Transaction_Value_Old = decimal.Parse(dt.Rows[0]["Transaction_Value"].ToString());

                            try
                            {
                                ProcessNo = long.Parse(dt.Rows[0]["ProcessNo"].ToString());
                            }
                            catch (Exception ee)
                            {

                            }
                        }
                        DiffValue = Cach_Value - Transaction_Value_Old;


                        //mysql = "Delete From Money_Transaction_Box Where Id=" + Money_Transaction_Box_ID;
                        //opp.insert_me(mysql);

                        EditBankBalance(ref opp, item, bnk, DiffValue);


                        item.Order_No = omMain.Order_No;
                        item.Bank_Name = bnk.Bank_Name;

                        Money_Transaction_BoxCLS mxCLS = new Money_Transaction_BoxCLS();
                        mxCLS.InsertMoney_Transaction_Box_(ref Excp, ref Msg, item, ref opp);
                        if (Excp == 0)
                        {

                            // Money_Transaction_BoxOrderReportCLS mo = new Money_Transaction_BoxOrderReportCLS();
                            Set_BankBalanceDay(ref opp, item.Transaction_Date.Value, bnk.Id, Box_Balance, item.Transaction_Value.Value, Transaction_Type);
                        }
                    }



                    if (Excp == 0)
                    {
                        Set_TransactionMoney_For_Customer_SupplierAccountsMulti_(ref Excp, ref Msg, ref opp, Order_Mony_Id2, item);
                    }
                    if (Excp == 1)
                    { break; }
                }
                if (Excp == 0)
                {
                    Money_Transaction_Box_ID = 0;
                    foreach (var item in arr_IdDelet)
                    {
                        decimal Box_Balance = 0;
                        Bank bnk = GetBank_Balance( item.Bank_Id, ref opp, ref Box_Balance);

                        Money_Transaction_Box_ID = item.Id;// long.Parse(arr_IdDelet[y].ToString());
                        decimal Transaction_Value_Old = 0;


                        Transaction_Value_Old = item.Transaction_Value.Value;

                        Transaction_Value_Old = Transaction_Value_Old * -1;

                        EditBankBalance(ref opp, item, bnk, Transaction_Value_Old);


                        string mysql = "Delete From  Money_Transaction_Box Where Id= " + Money_Transaction_Box_ID;
                        opp.insert_me(mysql);

                        try
                        {
                            mysql = "Delete From  Money_Transaction_BoxForInvoices Where Money_Transaction_BoxId= " + Money_Transaction_Box_ID;
                            opp.insert_me(mysql);
                        }
                        catch (Exception ee)
                        {

                        }

                        Set_TransactionMoney_For_Customer_SupplierAccountsMulti_(ref Excp, ref Msg, ref opp, Order_Mony_Id2, item);

                    }

                    if (mbxList.Count == 0)
                    {
                        string mysql = " Delete From  Order_Money Where Id= " + omMain.Id + "";
                        mysql = mysql + " Delete From  Order_Money Where MainOrder_MoneyId= " + omMain.Id + "";
                        opp.insert_me(mysql);
                    }
                }
                if (Excp == 0)
                {
                    //انشاء القيد
                    Account_Entry_Invoice_CLS aa = new Account_Entry_Invoice_CLS();
                    long Entry_No = 0;
                    aa.IsNotDeletePrevEntry = false;
                    aa.Account_Entry_List_ID = omMain.Account_Entry_List_ID ?? 0;
                    aa.Make_Entry_For_AccountsSandoukMulti(ref Excp, ref Msg, ref opp, omMain.Entry_Currency_ID ?? 1, omMain.Entry_Currency_Name, Factor, omMain.Id, Entry_No);
                }
                //aa.IsNotDeletePrevEntry = false;
                //aa.Reference_Type = Common.Checks_Memory_Reference_Type;//
                //aa.Reference_Type_Table_Id = Checks_Memory_Id;
                //aa.Account_Entry_List_ID = invL.Account_Entry_List_ID;

            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
            }

        }


        public void Edit_Transaction_BanksBox_NewSarfForAccounts(ref int Excp, ref string Msg, ref operations opp, ref long Order_Mony_Id2)
        {

            try
            {
                Order_MoneyCLS om = new Order_MoneyCLS();
                om.InsertOrder_Money(ref Excp, ref Msg, ref opp, omMain);
                long ProcessNo = omMain.ProcessNo ?? 0;
                long Money_Transaction_Box_ID = 0;

                Money_Transaction_Box_ID = mbx.Id;

                mbx.Is_ProcessedFlage = mbx.Is_ProcessedFlage ?? 0;// long.Parse(dt_Order.Rows[x]["Id_Test"].ToString());


                int Transaction_Type = mbx.Transaction_Type.Value;

                //
                decimal Cach_Value = mbx.Transaction_Value ?? 0;// decimal.Parse(dt_Order.Rows[x]["Transaction_Value"].ToString());

                decimal Cach_Value_Comput = Cach_Value;

                long Bank_Id = mbx.Bank_Id;
                decimal Box_Balance = 0;
                //long Entry_Currency_ID = 1;
                //string Entry_Currency_Name = "جنية مصرى";
                Bank bnk = GetBank_Balance(Bank_Id, ref opp, ref Box_Balance);

                if (mbx.Is_ProcessedFlage == 2) //تعديل
                {
                    Money_Transaction_Box_ID = mbx.Id;// long.Parse(dt_Order.Rows[x]["Id"].ToString());
                    decimal Transaction_Value_Old = 0;
                    decimal DiffValue = 0;
                    string mysql = "Select * from Money_Transaction_Box Where Id=" + Money_Transaction_Box_ID;
                    DataTable dt = opp.return_DataTable(mysql);

                    if (dt.Rows.Count > 0)
                    {
                        Transaction_Value_Old = decimal.Parse(dt.Rows[0]["Transaction_Value"].ToString());

                        try
                        {
                            ProcessNo = long.Parse(dt.Rows[0]["ProcessNo"].ToString());
                        }
                        catch (Exception ee)
                        {

                        }
                    }
                    DiffValue = Cach_Value - Transaction_Value_Old;


                    //mysql = "Delete From Money_Transaction_Box Where Id=" + Money_Transaction_Box_ID;
                    //opp.insert_me(mysql);

                    EditBankBalance(ref opp, mbx, bnk, DiffValue);


                    mbx.Order_No = omMain.Order_No;
                    mbx.Bank_Name = bnk.Bank_Name;

                    Money_Transaction_BoxCLS mxCLS = new Money_Transaction_BoxCLS();
                    mxCLS.InsertMoney_Transaction_Box_(ref Excp, ref Msg, mbx, ref opp);
                    if (Excp == 0)
                    {

                        // Money_Transaction_BoxOrderReportCLS mo = new Money_Transaction_BoxOrderReportCLS();
                        Set_BankBalanceDay(ref opp, mbx.Transaction_Date.Value, bnk.Id, Box_Balance, mbx.Transaction_Value.Value, Transaction_Type);
                    }
                }



                if (Excp == 0)
                {
                    Set_TransactionMoney_For_Customer_SupplierAccountsMulti_(ref Excp, ref Msg, ref opp, Order_Mony_Id2, mbx);
                }
                if (Excp == 1)
                { return; }


                if (Excp == 0)
                {
                    //انشاء القيد
                    Account_Entry_Invoice_CLS aa = new Account_Entry_Invoice_CLS();
                    long Entry_No = 0;
                    aa.IsNotDeletePrevEntry = false;
                    aa.Account_Entry_List_ID = omMain.Account_Entry_List_ID ?? 0;
                    aa.Make_Entry_For_AccountsSandoukMulti(ref Excp, ref Msg, ref opp, omMain.Entry_Currency_ID ?? 1, omMain.Entry_Currency_Name, Factor, omMain.Id, Entry_No);
                }
                //aa.IsNotDeletePrevEntry = false;
                //aa.Reference_Type = Common.Checks_Memory_Reference_Type;//
                //aa.Reference_Type_Table_Id = Checks_Memory_Id;
                //aa.Account_Entry_List_ID = invL.Account_Entry_List_ID;

            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
            }

        }



        public void EditBankBalance(ref operations opp, Money_Transaction_Box item, Bank bnk, decimal DiffValue)
        {

            int Excp = 0;
            string Msg = "";
            string mysql = " ";

            string Tafkeet_Value = "";



            Tafkeet_Value = TafkeetProj.NumberToText.GetText(item.Transaction_Value.Value, item.Currency_Id ?? 1);


            if (item.Transaction_Type == 1)
            {

                mysql = "  UPDATE Banks SET Last_Balance = Last_Balance +  " + DiffValue + "   WHERE (Id = " + bnk.Id + ")";

                opp.insert_me(mysql);
            }
            else
            {

                mysql = "  UPDATE Banks SET Last_Balance = Last_Balance -  " + DiffValue + "   WHERE (Id = " + bnk.Id + ")";

                opp.insert_me(mysql);
            }


        }

        public void Set_TransactionMoney_For_Customer_SupplierAccountsMulti(ref int Excp, ref string Msg, ref operations opp, long Order_Mony_Id2, Money_Transaction_Box item, Bank bnk, long Customer_ID_Supplier)
        {

            //long Money_Transaction_Box_ID,   decimal Cach_Value, long Customer_Id, long Supplier_Id, long SubAccount_Id, int Account_Type, string Notes,
            Money_Transaction_BoxCLS mxCLS = new Money_Transaction_BoxCLS();
            string mysql = "";
            decimal Balance_Before = 0;
            decimal Balance_After = 0;
            decimal Cach_Value_Comput = item.Transaction_Value.Value;
            int Transaction_Type = item.Transaction_Type.Value;
            decimal Box_Balance = GetBank_Balance(ref opp, bnk.Id, true);// bnk.Last_Balance ?? 0;
            decimal Box_BalanceBeforAction = Box_Balance;
            //decimal Box_BalanceBeforAction = GetBank_Balance(ref opp, bnk.Id, true);
            item.Is_ProcessedFlage = 0;
            Notes = item.Notes;
            #region Customer
            if (item.Account_Type == Common.Customer_Account_Type)
            {
                CustomersAccountCLS ca = new CustomersAccountCLS();
                ca.ProcessNo = item.ProcessNo;
                if (Transaction_Type == 1)  //استلام من عميل
                {
                    GetCustomer_Balance("Customer", Customer_ID_Supplier, ref opp, ref Balance_Before, ref Balance_After, Cach_Value_Comput);

                    decimal Last_Balance = Box_Balance + item.Transaction_Value.Value;

                    item.Import_Value = item.Transaction_Value;
                    item.Export_Value = 0;
                    item.Bank_Balance = Last_Balance;
                    item.Supplier_Id = 0;
                    item.Customer_Id = Customer_ID_Supplier;
                    item.Account_Name = SubAccountName;
                    item.SubAccount_Id = SubAccount_Id_;
                    mxCLS.InsertMoney_Transaction_Box_(ref Excp, ref Msg, item, ref opp);

                    if (Excp == 0)
                    {
                        ca.Set_Customer_Balance_WithMoney_Transaction(ref Excp, ref opp, Customer_ID_Supplier, Transaction_Type, Order_Mony_Id2, item.Id, item.Transaction_Value.Value, Notes);
                    }

                }
                if (Transaction_Type == 2)   // صرف للعملاء
                {


                    Cach_Value_Comput = Cach_Value_Comput * -1;
                    GetCustomer_Balance("Customer", Customer_ID_Supplier, ref opp, ref Balance_Before, ref Balance_After, Cach_Value_Comput);
                    //InsertOrderMoneyNew(ref Excp, ref Msg, ref opp, ref Order_Mony_Id2, long.Parse(Order_No), Transaction_Type, Order_Date, Cach_Value, Currency_Id, Bank_Type_, Man_Name, Box_Id, Order_Mony_Id_Transform, Invoice_ID, Invoice_Type, User_Id, Branch_Id, Year_Financial_Period_Id, Ticket_Supplier_Id, Order_Tafkeet_Value, Order_Notes, Sandouk_Cost_Center_Tree_Id, IS_Posted_Flage, Factor, Entry_Currency_ID, Entry_Currency_Name);

                    decimal Last_Balance = Box_Balance - item.Transaction_Value.Value;
                    //Money_Transaction_Box_ID = InsertMoney_Transaction_Box(ref Excp, ref Msg, ref opp, Order_Mony_Id2, Order_Date, Transaction_Type, 0, Cach_Value, Cach_Value, Cach_Value.ToString(), Bank_Id, Last_Balance, 0, Customer_ID_Supplier, 0, Customer_ID_Supplier, Common.Customer_Account_Type, SubAccount_Id, Notes, Balance_Before, Balance_After, Balance_BeforStr, Balance_AfterStr, SubAccountName, User_Id, Branch_Id, Factor, Factor);

                    item.Import_Value = 0;
                    item.Export_Value = item.Transaction_Value;
                    item.Bank_Balance = Last_Balance;
                    item.Supplier_Id = 0;
                    item.Customer_Id = Customer_ID_Supplier;
                    item.Account_Name = SubAccountName;
                    item.SubAccount_Id = SubAccount_Id_;

                    mxCLS.InsertMoney_Transaction_Box_(ref Excp, ref Msg, item, ref opp);
                    if (Excp == 0)
                    {
                        ca.Set_Customer_Balance_WithMoney_Transaction(ref Excp, ref opp, Customer_ID_Supplier, Transaction_Type, Order_Mony_Id2, item.Id, item.Transaction_Value.Value, Notes);

                    }


                }
            }
            #endregion

            #region Supplier
            //////////////////////////***********************************
            if (item.Account_Type == Common.Supplier_Account_Type)
            {

                Suppliers_AccountsCls sp = new Suppliers_AccountsCls();
                sp.ProcessNo = item.ProcessNo;
                if (Transaction_Type == 1)  // أستلام من المورد
                {

                    Cach_Value_Comput = Cach_Value_Comput * -1;
                    GetCustomer_Balance("Supplier", Customer_ID_Supplier, ref opp, ref Balance_Before, ref Balance_After, Cach_Value_Comput);
                    //InsertOrderMoneyNew(ref Excp, ref Msg, ref opp, ref Order_Mony_Id2, long.Parse(Order_No), Transaction_Type, Order_Date, Cach_Value, Currency_Id, Bank_Type_, Man_Name, Box_Id, Order_Mony_Id_Transform, Invoice_ID, Invoice_Type, User_Id, Branch_Id, Year_Financial_Period_Id, Ticket_Supplier_Id, Order_Tafkeet_Value, Order_Notes, Sandouk_Cost_Center_Tree_Id, IS_Posted_Flage, Factor, Entry_Currency_ID, Entry_Currency_Name);

                    decimal Last_Balance = Box_Balance + item.Transaction_Value.Value;
                    item.Import_Value = item.Transaction_Value.Value;
                    item.Export_Value = 0;
                    item.Bank_Balance = Last_Balance;
                    item.Supplier_Id = Customer_ID_Supplier;
                    item.Customer_Id = 0;
                    item.Account_Name = SubAccountName;
                    item.SubAccount_Id = SubAccount_Id_;

                    if (Excp == 0)
                    {
                        mxCLS.InsertMoney_Transaction_Box_(ref Excp, ref Msg, item, ref opp);

                        sp.Set_Supplier_Balance_WithMoney_Transaction(ref Excp, ref opp, Customer_ID_Supplier, Transaction_Type, Order_Mony_Id2, item.Id, item.Transaction_Value.Value, Notes);
                    }


                }
                if (Transaction_Type == 2) // صرف لمورد
                {


                    GetCustomer_Balance("Supplier", Customer_ID_Supplier, ref opp, ref Balance_Before, ref Balance_After, Cach_Value_Comput);

                    //InsertOrderMoneyNew(ref Excp, ref Msg, ref opp, ref Order_Mony_Id2, long.Parse(Order_No), Transaction_Type, Order_Date, Cach_Value, Currency_Id, Bank_Type_, Man_Name, Box_Id, Order_Mony_Id_Transform, Invoice_ID, Invoice_Type, User_Id, Branch_Id, Year_Financial_Period_Id, Ticket_Supplier_Id, Order_Tafkeet_Value, Order_Notes, Sandouk_Cost_Center_Tree_Id, IS_Posted_Flage, Factor, Entry_Currency_ID, Entry_Currency_Name);

                    decimal Last_Balance = Box_Balance - item.Transaction_Value.Value;
                    item.Import_Value = 0;
                    item.Export_Value = item.Transaction_Value.Value;
                    item.Bank_Balance = Last_Balance;
                    item.Supplier_Id = Customer_ID_Supplier;
                    item.Customer_Id = 0;
                    item.Account_Name = SubAccountName;
                    item.SubAccount_Id = SubAccount_Id_;
                    mxCLS.InsertMoney_Transaction_Box_(ref Excp, ref Msg, item, ref opp);
                    if (Excp == 0)
                    {
                        sp.Set_Supplier_Balance_WithMoney_Transaction(ref Excp, ref opp, Customer_ID_Supplier, Transaction_Type, Order_Mony_Id2, item.Id, item.Transaction_Value.Value, Notes);
                    }
                }
            }

            #endregion
            if (Excp == 0)
            {

                // Money_Transaction_BoxOrderReportCLS mo = new Money_Transaction_BoxOrderReportCLS();
                Set_BankBalanceDay(ref opp, item.Transaction_Date.Value, bnk.Id, Box_BalanceBeforAction, item.Transaction_Value.Value, Transaction_Type);
            }

        }
        public void Set_TransactionMoney_For_Customer_SupplierAccountsMulti_(ref int Excp, ref string Msg, ref operations opp, long Order_Mony_Id2, Money_Transaction_Box item)
        {

            //long Money_Transaction_Box_ID,   decimal Cach_Value, long Customer_Id, long Supplier_Id, long SubAccount_Id, int Account_Type, string Notes,
            Money_Transaction_BoxCLS mxCLS = new Money_Transaction_BoxCLS();
            string mysql = "";
            //decimal Balance_Before = 0;
            //decimal Balance_After = 0;
            decimal Cach_Value_Comput = item.Transaction_Value.Value;
            int Transaction_Type = item.Transaction_Type.Value;

            long Customer_ID_Supplier = item.Account_Id;
            //item.Is_ProcessedFlage = 0;
            Notes = item.Notes;


            #region Customer
            if (item.Account_Type == Common.Customer_Account_Type)
            {
                CustomersAccountCLS ca = new CustomersAccountCLS();
                ca.ProcessNo = item.ProcessNo;
                if (Transaction_Type == 1)  //استلام من عميل
                {
                    GetCustomer_Balance("Customer", Customer_ID_Supplier, ref opp, ref Balance_Before, ref Balance_After, Cach_Value_Comput);




                    if (Excp == 0)
                    {
                        ca.Set_Customer_Balance_WithMoney_Transaction(ref Excp, ref opp, Customer_ID_Supplier, Transaction_Type, Order_Mony_Id2, item.Id, item.Transaction_Value.Value, Notes);
                    }

                }
                if (Transaction_Type == 2)   // صرف للعملاء
                {


                    Cach_Value_Comput = Cach_Value_Comput * -1;
                    GetCustomer_Balance("Customer", Customer_ID_Supplier, ref opp, ref Balance_Before, ref Balance_After, Cach_Value_Comput);
                    //InsertOrderMoneyNew(ref Excp, ref Msg, ref opp, ref Order_Mony_Id2, long.Parse(Order_No), Transaction_Type, Order_Date, Cach_Value, Currency_Id, Bank_Type_, Man_Name, Box_Id, Order_Mony_Id_Transform, Invoice_ID, Invoice_Type, User_Id, Branch_Id, Year_Financial_Period_Id, Ticket_Supplier_Id, Order_Tafkeet_Value, Order_Notes, Sandouk_Cost_Center_Tree_Id, IS_Posted_Flage, Factor, Entry_Currency_ID, Entry_Currency_Name);


                    if (Excp == 0)
                    {
                        ca.Set_Customer_Balance_WithMoney_Transaction(ref Excp, ref opp, Customer_ID_Supplier, Transaction_Type, Order_Mony_Id2, item.Id, item.Transaction_Value.Value, Notes);

                    }


                }
            }
            #endregion

            #region Supplier
            //////////////////////////***********************************
            if (item.Account_Type == Common.Supplier_Account_Type)
            {

                Suppliers_AccountsCls sp = new Suppliers_AccountsCls();
                sp.ProcessNo = item.ProcessNo;
                if (Transaction_Type == 1)  // أستلام من المورد
                {

                    Cach_Value_Comput = Cach_Value_Comput * -1;
                    GetCustomer_Balance("Supplier", Customer_ID_Supplier, ref opp, ref Balance_Before, ref Balance_After, Cach_Value_Comput);
                    //InsertOrderMoneyNew(ref Excp, ref Msg, ref opp, ref Order_Mony_Id2, long.Parse(Order_No), Transaction_Type, Order_Date, Cach_Value, Currency_Id, Bank_Type_, Man_Name, Box_Id, Order_Mony_Id_Transform, Invoice_ID, Invoice_Type, User_Id, Branch_Id, Year_Financial_Period_Id, Ticket_Supplier_Id, Order_Tafkeet_Value, Order_Notes, Sandouk_Cost_Center_Tree_Id, IS_Posted_Flage, Factor, Entry_Currency_ID, Entry_Currency_Name);


                    if (Excp == 0)
                    {


                        sp.Set_Supplier_Balance_WithMoney_Transaction(ref Excp, ref opp, Customer_ID_Supplier, Transaction_Type, Order_Mony_Id2, item.Id, item.Transaction_Value.Value, Notes);
                    }


                }
                if (Transaction_Type == 2) // صرف لمورد
                {


                    GetCustomer_Balance("Supplier", Customer_ID_Supplier, ref opp, ref Balance_Before, ref Balance_After, Cach_Value_Comput);



                    if (Excp == 0)
                    {
                        sp.Set_Supplier_Balance_WithMoney_Transaction(ref Excp, ref opp, Customer_ID_Supplier, Transaction_Type, Order_Mony_Id2, item.Id, item.Transaction_Value.Value, Notes);
                    }
                }
            }

            #endregion


        }
        public void Edit_Transaction_BanksBox_NewSarf(ref int Excp, ref string Msg, ref operations opp, string Bank_Name, ref long Order_Mony_Id2, ArrayList arr_IdDelet, DataTable dt_Order, int Transaction_Type, long Sandouk_Cost_Center_Tree_Id, string Order_No, long Box_Id, decimal Box_Balance, DateTime Order_Date, string Man_Name, long Box_Currency_Id, string Order_Tafkeet_Value, string Order_Notes, decimal Factor, long    Entry_Currency_ID, string Entry_Currency_Name, long CarsWorkOrdersId, long Reference_Type_Table_Id, long DataDictionaryProcessesListsId)
        {


            // int Account_Type_Value, int SubAccount_Customer_Supplier_Id  decimal Cach_Value
            //int Account_Cost_Center_Tree_Id, ref long Order_Mony_Id2, int Transaction_Type
            try
            {
                //Edit_Order_Money(ref   opp,Order_Mony_Id2, long.Parse(Order_No), Order_Date, Transaction_Type, 0, Box_Currency_Id, 0, Box_Id, Man_Name, 0, 0, 0, 0, 0, 0, Order_Tafkeet_Value, Order_Notes, Sandouk_Cost_Center_Tree_Id, Factor, Entry_Currency_ID, Entry_Currency_Name);
                Edit_Order_Money(ref opp, Order_Mony_Id2, long.Parse(Order_No), Order_Date, Transaction_Type, Man_Name, Order_Tafkeet_Value, Order_Notes, Factor);


                long Money_Transaction_Box_ID = 0;
                for (int x = 0; x < dt_Order.Rows.Count; x++)
                {
                    Money_Transaction_Box_ID = 0;

                    long Id_Test = long.Parse(dt_Order.Rows[x]["Id_Test"].ToString());

                    if (Id_Test == 0) ///سطر بدون تعديل اى من القيد قبل التعديلات
                    {
                        //
                        continue;
                    }

                    //
                    decimal Cach_Value = decimal.Parse(dt_Order.Rows[x]["Transaction_Value"].ToString());
                    string Notes_Details = dt_Order.Rows[x]["Notes"].ToString();
                    string Account_Name = dt_Order.Rows[x]["Account_Name"].ToString();
                    string CostCenter_Name = dt_Order.Rows[x]["CostCenter_Name"].ToString();
                    decimal Balance_Before = 0;
                    decimal Balance_After = 0;
                    string Balance_BeforStr = "";
                    string Balance_AfterStr = "";
                    try
                    {
                        Balance_Before = decimal.Parse(dt_Order.Rows[x]["Balance_Before"].ToString());
                    }
                    catch (Exception)
                    {
                    }
                    try
                    {
                        Balance_After = decimal.Parse(dt_Order.Rows[x]["Balance_After"].ToString());
                    }
                    catch (Exception)
                    {
                    }
                    try
                    {
                        Balance_BeforStr = dt_Order.Rows[x]["Balance_BeforStr"].ToString();
                    }
                    catch (Exception)
                    {
                    }
                    try
                    {
                        Balance_AfterStr = dt_Order.Rows[x]["Balance_AfterStr"].ToString();
                    }
                    catch (Exception)
                    {
                    }
                    decimal Cach_Value_Comput = Cach_Value;
                    long Customer_Id = 0;
                    long Supplier_Id = 0;
                    int Account_Type_Value = int.Parse(dt_Order.Rows[x]["Account_Type"].ToString());
                    long SubAccount_Id = long.Parse(dt_Order.Rows[x]["SubAccount_Id"].ToString());
                    long Account_Cost_Center_Tree_Id = long.Parse(dt_Order.Rows[x]["Account_Cost_Center_Tree_Id"].ToString());
                    long Year_Financial_Period_Id = Financial_PeriodCLS.Get_Year_Financial_Period_Id(Order_Date);// Common.Year_Financial_Period_Id;
                    long Account_Currency_Id = long.Parse(dt_Order.Rows[x]["Currency_Id"].ToString());
                    string Account_Code = dt_Order.Rows[x]["Account_Code"].ToString();
                    //if (Account_Type_Value == Common.Customer_Account_Type)
                    //{
                    //    Customer_Id = SubAccount_Customer_Supplier_Id;
                    //}
                    //if (Account_Type_Value == Common.Supplier_Account_Type)
                    //{
                    //    Supplier_Id = SubAccount_Customer_Supplier_Id;
                    //}
                    if (Account_Type_Value == Common.Customer_Account_Type)
                    {
                        CustomerCLS cu = new CustomerCLS();
                        cu.Get_CustomerBySubAccount_Id(ref opp, SubAccount_Id);
                        Customer_Id = cu.cs.Id;

                        //Customer_Id = SubAccount_Customer_Supplier_Id;
                    }
                    if (Account_Type_Value == Common.Supplier_Account_Type)
                    {
                        SupplierCLS su = new SupplierCLS();
                        su.Get_SupplierBySubAccount_Id(ref opp, SubAccount_Id);
                        Supplier_Id = su.sp.Id;
                        //Supplier_Id = SubAccount_Customer_Supplier_Id;
                    }
                    long ProcessNo = 0;
                    if (Id_Test == 1) //   اضافة على الاذن
                    {
                        Money_Transaction_Box_ID = Insert_Money_Transaction_Box(ref opp, Order_Mony_Id2, long.Parse(Order_No), Bank_Name, Order_Date, Box_Currency_Id, Account_Currency_Id, Transaction_Type, Cach_Value, Cach_Value.ToString(), Box_Id, Customer_Id, Supplier_Id, SubAccount_Id, Account_Type_Value, Notes_Details, Balance_Before, Balance_After, Balance_BeforStr, Balance_AfterStr, 0, 0, 0, 0, 0, Account_Cost_Center_Tree_Id, Account_Name, CostCenter_Name, Factor, Account_Code, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);
                        Set_TransactionMoney_For_Customer_SupplierAccounts(ref opp, Order_Mony_Id2, Money_Transaction_Box_ID, Transaction_Type, Cach_Value, Customer_Id, Supplier_Id, SubAccount_Id, Account_Type_Value, Notes_Details, ProcessNo);
                    }
                    if (Id_Test == 2) //تعديل
                    {
                        Money_Transaction_Box_ID = long.Parse(dt_Order.Rows[x]["Id"].ToString());
                        decimal Transaction_Value_Old = 0;
                        decimal DiffValue = 0;
                        string mysql = "Select * from Money_Transaction_Box Where Id=" + Money_Transaction_Box_ID;
                        DataTable dt = opp.return_DataTable(mysql);
                        if (dt.Rows.Count > 0)
                        {
                            Transaction_Value_Old = decimal.Parse(dt.Rows[0]["Transaction_Value"].ToString());

                            try
                            { ProcessNo = long.Parse(dt.Rows[0]["ProcessNo"].ToString()); }
                            catch (Exception ee)
                            { }
                        }
                        DiffValue = Cach_Value - Transaction_Value_Old;
                        if (Transaction_Type == 1)
                        { }
                        Edit_Money_Transaction_Box(ref opp, Order_Mony_Id2, long.Parse(Order_No), Bank_Name, Money_Transaction_Box_ID, Order_Date, Box_Currency_Id, Account_Currency_Id, Transaction_Type, Cach_Value, Cach_Value.ToString(), DiffValue, Box_Id, SubAccount_Id, Account_Type_Value, Notes_Details, Balance_Before, Balance_After, Balance_BeforStr, Balance_AfterStr, 0, 0, 0, 0, 0, Account_Cost_Center_Tree_Id, Account_Name, CostCenter_Name, Factor, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);
                        mysql = "Delete From Money_Transaction_Box Where Id=" + Money_Transaction_Box_ID;
                        opp.insert_me(mysql);
                        Set_TransactionMoney_For_Customer_SupplierAccounts(ref opp, Order_Mony_Id2, Money_Transaction_Box_ID, Transaction_Type, DiffValue, Customer_Id, Supplier_Id, SubAccount_Id, Account_Type_Value, Notes_Details, ProcessNo);
                    }
                }
                Money_Transaction_Box_ID = 0;
                for (int y = 0; y < arr_IdDelet.Count; y++)
                {
                    Money_Transaction_Box_ID = long.Parse(arr_IdDelet[y].ToString());
                    decimal Transaction_Value_Old = 0;
                    decimal DiffValue = 0;
                    string mysql = "Select * from Money_Transaction_Box Where Id=" + Money_Transaction_Box_ID;
                    DataTable dt = opp.return_DataTable(mysql);
                    if (dt.Rows.Count > 0)
                    {
                        Transaction_Value_Old = decimal.Parse(dt.Rows[0]["Transaction_Value"].ToString());
                    }
                    Transaction_Value_Old = Transaction_Value_Old * -1;



                    Delete_Money_Transaction_Box(ref opp, Order_Mony_Id2, Money_Transaction_Box_ID, Transaction_Type, Transaction_Value_Old, Box_Id);



                }

                Excp = 0;
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
            }

        }

        public void Edit_Transaction_BanksBox_NewSarf_(ref int Excp, ref string Msg, ref operations opp, ref long Order_Mony_Id2, string Bank_Name, ArrayList arr_IdDelet, DataTable dt_Order, int Transaction_Type, long Sandouk_Cost_Center_Tree_Id, string Order_No, long Box_Id, decimal Box_Balance, DateTime Order_Date, string Man_Name, long  Box_Currency_Id, string Order_Tafkeet_Value, string Order_Notes, decimal Factor, long    Entry_Currency_ID, string Entry_Currency_Name, long CarsWorkOrdersId, long Reference_Type_Table_Id, long DataDictionaryProcessesListsId)
        {


            // int Account_Type_Value, int SubAccount_Customer_Supplier_Id  decimal Cach_Value
            //int Account_Cost_Center_Tree_Id, ref long Order_Mony_Id2, int Transaction_Type
            try
            {
                //Edit_Order_Money(ref   opp,Order_Mony_Id2, long.Parse(Order_No), Order_Date, Transaction_Type, 0, Box_Currency_Id, 0, Box_Id, Man_Name, 0, 0, 0, 0, 0, 0, Order_Tafkeet_Value, Order_Notes, Sandouk_Cost_Center_Tree_Id, Factor, Entry_Currency_ID, Entry_Currency_Name);
                Edit_Order_Money(ref opp, Order_Mony_Id2, long.Parse(Order_No), Order_Date, Transaction_Type, Man_Name, Order_Tafkeet_Value, Order_Notes, Factor);


                long Money_Transaction_Box_ID = 0;
                for (int x = 0; x < dt_Order.Rows.Count; x++)
                {
                    Money_Transaction_Box_ID = 0;

                    long Id_Test = long.Parse(dt_Order.Rows[x]["Id_Test"].ToString());

                    if (Id_Test == 0) ///سطر بدون تعديل اى من القيد قبل التعديلات
                    {
                        //
                        continue;
                    }

                    //
                    decimal Cach_Value = decimal.Parse(dt_Order.Rows[x]["Transaction_Value"].ToString());
                    string Notes_Details = dt_Order.Rows[x]["Notes"].ToString();
                    string Account_Name = dt_Order.Rows[x]["Account_Name"].ToString();
                    string CostCenter_Name = dt_Order.Rows[x]["CostCenter_Name"].ToString();
                    decimal Balance_Before = 0;
                    decimal Balance_After = 0;
                    string Balance_BeforStr = "";
                    string Balance_AfterStr = "";
                    try
                    {
                        Balance_Before = decimal.Parse(dt_Order.Rows[x]["Balance_Before"].ToString());
                    }
                    catch (Exception)
                    {
                    }
                    try
                    {
                        Balance_After = decimal.Parse(dt_Order.Rows[x]["Balance_After"].ToString());
                    }
                    catch (Exception)
                    {
                    }
                    try
                    {
                        Balance_BeforStr = dt_Order.Rows[x]["Balance_BeforStr"].ToString();
                    }
                    catch (Exception)
                    {
                    }
                    try
                    {
                        Balance_AfterStr = dt_Order.Rows[x]["Balance_AfterStr"].ToString();
                    }
                    catch (Exception)
                    {
                    }
                    decimal Cach_Value_Comput = Cach_Value;
                    long Customer_Id = 0;
                    long Supplier_Id = 0;
                    int Account_Type_Value = int.Parse(dt_Order.Rows[x]["Account_Type"].ToString());
                    long SubAccount_Id_ = long.Parse(dt_Order.Rows[x]["SubAccount_Id"].ToString());
                    long Account_Cost_Center_Tree_Id = long.Parse(dt_Order.Rows[x]["Account_Cost_Center_Tree_Id"].ToString());
                    long Year_Financial_Period_Id = Financial_PeriodCLS.Get_Year_Financial_Period_Id(Order_Date);// Common.Year_Financial_Period_Id;
                    long Account_Currency_Id = long.Parse(dt_Order.Rows[x]["Currency_Id"].ToString());
                    string Account_Code = dt_Order.Rows[x]["Account_Code"].ToString();
                    //if (Account_Type_Value == Common.Customer_Account_Type)
                    //{
                    //    Customer_Id = SubAccount_Customer_Supplier_Id;
                    //}
                    //if (Account_Type_Value == Common.Supplier_Account_Type)
                    //{
                    //    Supplier_Id = SubAccount_Customer_Supplier_Id;
                    //}
                    if (Account_Type_Value == Common.Customer_Account_Type)
                    {
                        CustomerCLS cu = new CustomerCLS();
                        cu.Get_CustomerBySubAccount_Id(ref opp, SubAccount_Id_);
                        Customer_Id = cu.cs.Id;

                        //Customer_Id = SubAccount_Customer_Supplier_Id;
                    }
                    if (Account_Type_Value == Common.Supplier_Account_Type)
                    {
                        SupplierCLS su = new SupplierCLS();
                        su.Get_SupplierBySubAccount_Id(ref opp, SubAccount_Id_);
                        Supplier_Id = su.sp.Id;
                        //Supplier_Id = SubAccount_Customer_Supplier_Id;
                    }
                    long ProcessNo = 0;
                    if (Id_Test == 1) //   اضافة على الاذن
                    {
                        Money_Transaction_Box_ID = Insert_Money_Transaction_Box(ref opp, Order_Mony_Id2, long.Parse(Order_No), Bank_Name, Order_Date, Box_Currency_Id, Account_Currency_Id, Transaction_Type, Cach_Value, Cach_Value.ToString(), Box_Id, Customer_Id, Supplier_Id, SubAccount_Id_, Account_Type_Value, Notes_Details, Balance_Before, Balance_After, Balance_BeforStr, Balance_AfterStr, 0, 0, 0, 0, 0, Account_Cost_Center_Tree_Id, Account_Name, CostCenter_Name, Factor, Account_Code, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);
                        Set_TransactionMoney_For_Customer_SupplierAccounts(ref opp, Order_Mony_Id2, Money_Transaction_Box_ID, Transaction_Type, Cach_Value, Customer_Id, Supplier_Id, SubAccount_Id_, Account_Type_Value, Notes_Details, ProcessNo);
                    }
                    if (Id_Test == 2) //تعديل
                    {
                        Money_Transaction_Box_ID = long.Parse(dt_Order.Rows[x]["Id"].ToString());
                        decimal Transaction_Value_Old = 0;
                        decimal DiffValue = 0;
                        string mysql = "Select * from Money_Transaction_Box Where Id=" + Money_Transaction_Box_ID;
                        DataTable dt = opp.return_DataTable(mysql);

                        if (dt.Rows.Count > 0)
                        {
                            Transaction_Value_Old = decimal.Parse(dt.Rows[0]["Transaction_Value"].ToString());

                            try
                            {
                                ProcessNo = long.Parse(dt.Rows[0]["ProcessNo"].ToString());
                            }
                            catch (Exception ee)
                            {

                            }
                        }
                        DiffValue = Cach_Value - Transaction_Value_Old;

                        if (Transaction_Type == 1)
                        {

                        }

                        Edit_Money_Transaction_Box(ref opp, Order_Mony_Id2, long.Parse(Order_No), Bank_Name, Money_Transaction_Box_ID, Order_Date, Box_Currency_Id, Account_Currency_Id, Transaction_Type, Cach_Value, Cach_Value.ToString(), DiffValue, Box_Id, SubAccount_Id_, Account_Type_Value, Notes_Details, Balance_Before, Balance_After, Balance_BeforStr, Balance_AfterStr, 0, 0, 0, 0, 0, Account_Cost_Center_Tree_Id, Account_Name, CostCenter_Name, Factor, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);
                        mysql = "Delete From Money_Transaction_Box Where Id=" + Money_Transaction_Box_ID;
                        opp.insert_me(mysql);


                        Set_TransactionMoney_For_Customer_SupplierAccounts(ref opp, Order_Mony_Id2, Money_Transaction_Box_ID, Transaction_Type, DiffValue, Customer_Id, Supplier_Id, SubAccount_Id_, Account_Type_Value, Notes_Details, ProcessNo);
                    }
                }
                Money_Transaction_Box_ID = 0;
                for (int y = 0; y < arr_IdDelet.Count; y++)
                {
                    Money_Transaction_Box_ID = long.Parse(arr_IdDelet[y].ToString());
                    decimal Transaction_Value_Old = 0;
                    decimal DiffValue = 0;
                    string mysql = "Select * from Money_Transaction_Box Where Id=" + Money_Transaction_Box_ID;
                    DataTable dt = opp.return_DataTable(mysql);
                    if (dt.Rows.Count > 0)
                    {
                        Transaction_Value_Old = decimal.Parse(dt.Rows[0]["Transaction_Value"].ToString());
                    }
                    Transaction_Value_Old = Transaction_Value_Old * -1;



                    Delete_Money_Transaction_Box(ref opp, Order_Mony_Id2, Money_Transaction_Box_ID, Transaction_Type, Transaction_Value_Old, Box_Id);



                }

                Excp = 0;
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
            }

        }
        public void Set_TransactionMoney_For_Customer_SupplierAccounts(ref operations opp, long Order_Mony_Id2, long Money_Transaction_Box_ID, int Transaction_Type, decimal Cach_Value, long Customer_Id, long Supplier_Id, long SubAccount_Id, int Account_Type, string Notes, long ProcessNo)
        {


            int Excp = 0;
            string mysql = "";
            decimal Balance_Before = 0;
            decimal Balance_After = 0;
            decimal Cach_Value_Comput = Cach_Value;

            if (Account_Type == 0 & SubAccount_Id > 0)
            {
                string mysqlSub = "Select Account_Customer_Type_Value from  Submain_Account  Where  Id=" + SubAccount_Id;
                DataTable dtSub = opp.return_DataTable(mysqlSub);
                if (dtSub.Rows.Count > 0)
                {
                    int.TryParse(dtSub.Rows[0]["Account_Customer_Type_Value"].ToString(), out Account_Type);
                }
            }
            #region Customer
            if (Account_Type == Common.Customer_Account_Type)
            {
                CustomersAccountCLS ca = new CustomersAccountCLS();
                ca.ProcessNo = ProcessNo;
                if (Transaction_Type == 1)  //استلام من عميل
                {

                    GetCustomer_Balance("Customer", Customer_Id, ref opp, ref Balance_Before, ref Balance_After, Cach_Value_Comput);


                    ca.Set_Customer_Balance_WithMoney_Transaction(ref Excp, ref opp, Customer_Id, Transaction_Type, Order_Mony_Id2, Money_Transaction_Box_ID, Cach_Value, Notes);


                }
                if (Transaction_Type == 2)   // صرف للعملاء
                {
                    Cach_Value_Comput = Cach_Value_Comput * -1;
                    GetCustomer_Balance("Customer", Customer_Id, ref opp, ref Balance_Before, ref Balance_After, Cach_Value_Comput);


                    ca.Set_Customer_Balance_WithMoney_Transaction(ref Excp, ref opp, Customer_Id, Transaction_Type, Order_Mony_Id2, Money_Transaction_Box_ID, Cach_Value, Notes);




                }
            }
            #endregion

            #region Supplier
            //////////////////////////***********************************
            if (Account_Type == Common.Supplier_Account_Type)
            {

                Suppliers_AccountsCls sp = new Suppliers_AccountsCls();
                sp.ProcessNo = ProcessNo;
                if (Transaction_Type == 1)  // أستلام من المورد
                {
                    Cach_Value_Comput = Cach_Value_Comput * -1;
                    GetCustomer_Balance("Supplier", Supplier_Id, ref opp, ref Balance_Before, ref Balance_After, Cach_Value_Comput);

                    sp.Set_Supplier_Balance_WithMoney_Transaction(ref Excp, ref opp, Supplier_Id, Transaction_Type, Order_Mony_Id2, Money_Transaction_Box_ID, Cach_Value, Notes);



                }
                if (Transaction_Type == 2) // صرف لمورد
                {

                    GetCustomer_Balance("Supplier", Supplier_Id, ref opp, ref Balance_Before, ref Balance_After, Cach_Value_Comput);

                    sp.Set_Supplier_Balance_WithMoney_Transaction(ref Excp, ref opp, Supplier_Id, Transaction_Type, Order_Mony_Id2, Money_Transaction_Box_ID, Cach_Value, Notes);

                }
            }

            #endregion 

        }

        public void Make_Transaction_BanksBox_NewSarf(ref int Excp, ref string Msg, ref operations opp, ref long Order_Mony_Id2, string Bank_Name, DataTable dt_Order, int Transaction_Type, long Sandouk_Cost_Center_Tree_Id, string Order_No, long Box_Id, decimal Box_Balance, DateTime Order_Date, string Man_Name, long Box_Currency_Id, string Order_Tafkeet_Value, string Order_Notes, decimal Factor, long    Entry_Currency_ID, string Entry_Currency_Name, long CarsWorkOrdersId, long Reference_Type_Table_Id, long DataDictionaryProcessesListsId, int IsMoreTheOneBoxShow)
        {
            // int Account_Type_Value, int SubAccount_Customer_Supplier_Id  decimal Cach_Value
            //int Account_Cost_Center_Tree_Id, ref long Order_Mony_Id2, int Transaction_Type
            try
            {
                long Order_No_ = long.Parse(Order_No);
                bool OrderNoFound = Check_Order_NoFound(ref Excp, ref opp, ref Order_No_, Transaction_Type, Order_Date, Box_Id);
                Order_No = Order_No_.ToString();
                if (OrderNoFound == true | Order_No.ToString() == "0")
                {
                    Order_No = Make_New_Order_No(ref Excp, ref opp, Transaction_Type, Order_Date, Box_Id).ToString();

                }

                Order_Mony_Id2 = Insert_Order_Money(ref Excp, ref Msg, ref opp, long.Parse(Order_No), Order_Date, Transaction_Type, 0, Box_Currency_Id, 0, Box_Id, Man_Name, 0, 0, 0, 0, Invoice_ID, 0, Order_Tafkeet_Value, Order_Notes, Sandouk_Cost_Center_Tree_Id, Factor, Entry_Currency_ID, Entry_Currency_Name, CarsWorkOrdersId, IsMoreTheOneBoxShow);
                long Customer_Id = 0;
                long Supplier_Id = 0;
                for (int x = 0; x < dt_Order.Rows.Count; x++)
                {
                    decimal Cach_Value = decimal.Parse(dt_Order.Rows[x]["Transaction_Value"].ToString());
                    string Notes_Details = dt_Order.Rows[x]["Notes"].ToString();
                    string Account_Name = dt_Order.Rows[x]["Account_Name"].ToString();
                    string CostCenter_Name = dt_Order.Rows[x]["CostCenter_Name"].ToString();
                    decimal Balance_Before = 0;
                    decimal Balance_After = 0;
                    string Balance_BeforStr = "";
                    string Balance_AfterStr = "";
                    try
                    {
                        Balance_Before = decimal.Parse(dt_Order.Rows[x]["Balance_Before"].ToString());
                    }
                    catch (Exception)
                    {
                    }
                    try
                    {
                        Balance_After = decimal.Parse(dt_Order.Rows[x]["Balance_After"].ToString());
                    }
                    catch (Exception)
                    {
                    }
                    try
                    {
                        Balance_BeforStr = dt_Order.Rows[x]["Balance_BeforStr"].ToString();
                    }
                    catch (Exception)
                    {
                    }
                    try
                    {
                        Balance_AfterStr = dt_Order.Rows[x]["Balance_AfterStr"].ToString();
                    }
                    catch (Exception)
                    {
                    }


                    decimal Cach_Value_Comput = Cach_Value;
                    Customer_Id = 0;
                    Supplier_Id = 0;
                    int Account_Type_Value = int.Parse(dt_Order.Rows[x]["Account_Type"].ToString());
                    string Account_Code = dt_Order.Rows[x]["Account_Code"].ToString();
                    long SubAccount_Customer_Supplier_Id = long.Parse(dt_Order.Rows[x]["SubAccount_Id"].ToString());
                    long SubAccount_Id = long.Parse(dt_Order.Rows[x]["SubAccount_Id"].ToString());
                    long Account_Cost_Center_Tree_Id = long.Parse(dt_Order.Rows[x]["Account_Cost_Center_Tree_Id"].ToString());
                    long Year_Financial_Period_Id = Financial_PeriodCLS.Get_Year_Financial_Period_Id(ref opp, Order_Date);// Common.Year_Financial_Period_Id;
                    long Account_Currency_Id = 1;
                    try
                    {
                        Account_Currency_Id = long.Parse(dt_Order.Rows[x]["Currency_Id"].ToString());
                    }
                    catch { }

                    if (Account_Type_Value == Common.Customer_Account_Type)
                    {
                        CustomerCLS cu = new CustomerCLS();
                        cu.Get_CustomerBySubAccount_Id(ref opp, SubAccount_Id);
                        Customer_Id = cu.cs.Id;

                        //Customer_Id = SubAccount_Customer_Supplier_Id;
                    }
                    if (Account_Type_Value == Common.Supplier_Account_Type)
                    {
                        SupplierCLS su = new SupplierCLS();
                        su.Get_SupplierBySubAccount_Id(ref opp, SubAccount_Id);
                        Supplier_Id = su.sp.Id;
                        //Supplier_Id = SubAccount_Customer_Supplier_Id;
                    }

                    /*
                    Money_Transaction_Box mt = new Money_Transaction_Box();

                    ///////// 
                    ///
                    //long Money_Transaction_Box_ID = Insert_Money_Transaction_Box(ref opp, Order_Mony_Id2, Order_Date, Box_Currency_Id, Account_Currency_Id, Transaction_Type, Cach_Value, Cach_Value.ToString(), Box_Id, Customer_Id, Supplier_Id, SubAccount_Customer_Supplier_Id, Account_Type_Value, Notes_Details, Balance_Before, Balance_After, Balance_BeforStr, Balance_AfterStr, 0, 0, 0, 0, 0, Account_Cost_Center_Tree_Id, Account_Name, CostCenter_Name, Factor, Account_Code, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);

                    mt.Order_Mony_Id = Order_Mony_Id2;
                    mt.Transaction_Date = Order_Date;
                    mt.Transaction_Type = Transaction_Type;
                    mt.Import_Value = Import_Value;
                    mt.Export_Value = Export_Value;
                    mt.Transaction_Value = Transaction_Value;
                    mt.Transaction_Value_ = Transaction_Value_;
                    mt.Equivalent_Value = Equivalent_Value;

                    mt.Tafkeet_Value = Tafkeet_Value;


                    mt.Bank_Id = Box_Id;
                  
                    mt.Supplier_Id = Supplier_Id;
                    mt.Customer_Id = Customer_Id;
                    mt.Account_Id = SubAccount_Customer_Supplier_Id;
                    mt.SubAccount_Id = SubAccount_Id;
                    mt.Account_Type = Account_Type;
                    mt.Invoice_Id = Invoice_Id;
                    mt.Invoice_TypeId = Invoice_TypeId;

                    mt.CarsWorkOrdersId = CarsWorkOrdersId;
                    mt.Reference_Type_Table_Id = Reference_Type_Table_Id;
                    mt.DataDictionaryProcessesListsId = DataDictionaryProcessesListsId;

                    mt.Notes = Notes;
                    mt.Balance_Before = Balance_Before;
                    mt.Balance_After = Balance_After;

                    mt.Balance_BeforStr = Balance_BeforStr;
                    mt.Balance_AfterStr = Balance_AfterStr;

                    mt.SubAccount_Id = SubAccount_Id;
                    mt.Account_Name = Account_Name;

                    mt.Equivalent_Value = Factor;
                    mt.Factor = Factor;

                    mt.Bank_Balance = Bank_Balance;

                    mt.Lookup_CashPaymentMethodId = Lookup_CashPaymentMethodId;
                    mt.Lookup_CashPaymentMethodName = Lookup_CashPaymentMethodName;

                    mt.BankAccount_Id = BankAccount_Id;
                    mt.DestructionSales = DestructionSales;
                    mt.Order_mony_id_transform = Order_mony_id_transform;
                    mt.Account_Cost_Center_Tree_Id = Account_Cost_Center_Tree_Id;
                    mt.TotalValueWillPaid = TotalValueWillPaid;
                    mt.Rest_Value = Rest_Value;
                    mt.Account_Code = Account_Code;
                    mt.Cost_Center = Cost_Center;
                    mt.Currency_Id = Currency_Id;
                    mt.Purchase_Ticket_Value = Purchase_Ticket_Value;
                    mt.Commission_Value = Commission_Value;
                    mt.Entry_Currency_ID = Entry_Currency_ID;
                    mt.CostCenter_Name = CostCenter_Name;

                    mt.User_Id = Common.User_Id;
                    mt.Branch_Id = Common.Branch_Id;
                    */

                    ////////

                    long Money_Transaction_Box_ID = Insert_Money_Transaction_Box(ref opp, Order_Mony_Id2, long.Parse(Order_No), Bank_Name, Order_Date, Box_Currency_Id, Account_Currency_Id, Transaction_Type, Cach_Value, Cach_Value.ToString(), Box_Id, Customer_Id, Supplier_Id, SubAccount_Customer_Supplier_Id, Account_Type_Value, Notes_Details, Balance_Before, Balance_After, Balance_BeforStr, Balance_AfterStr, 0, 0, 0, 0, 0, Account_Cost_Center_Tree_Id, Account_Name, CostCenter_Name, Factor, Account_Code, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);


                    long ProcessNo = 0;
                    Set_TransactionMoney_For_Customer_SupplierAccounts(ref opp, Order_Mony_Id2, Money_Transaction_Box_ID, Transaction_Type, Cach_Value, Customer_Id, Supplier_Id, SubAccount_Customer_Supplier_Id, Account_Type_Value, Notes_Details, ProcessNo);


                }
                Excp = 0;
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
            }

        }


        public void Make_Transaction_BanksBox_NewSarf_(ref int Excp, ref string Msg, ref operations opp, ref long Order_Mony_Id2, Order_Money omMain, List<Money_Transaction_Box> mbxList)
        {
            //DataTable dt_Order, int Transaction_Type, long Sandouk_Cost_Center_Tree_Id, string Order_No, long  Box_Id, decimal Box_Balance, DateTime Order_Date, string Man_Name, long  Box_Currency_Id, string Order_Tafkeet_Value, string Order_Notes, decimal Factor, long    Entry_Currency_ID, string Entry_Currency_Name
            try
            {
                long Order_No_ = omMain.Order_No;
                bool OrderNoFound = Check_Order_NoFound(ref Excp, ref opp, ref Order_No_, omMain.Transaction_type, omMain.Order_Date, omMain.Bank_Id);

                if (OrderNoFound == true | Order_No_.ToString() == "0")
                {
                    omMain.Order_No = Make_New_Order_No(ref Excp, ref opp, omMain.Transaction_type, omMain.Order_Date, omMain.Bank_Id);

                }
                Order_MoneyCLS omCLS = new Order_MoneyCLS();
                omCLS.InsertOrder_Money(ref Excp, ref Msg, ref opp, omMain);

                //Order_Mony_Id2 = Insert_Order_Money(ref Excp, ref Msg, ref opp, long.Parse(Order_No), Order_Date, Transaction_Type, 0, Box_Currency_Id, 0, Box_Id, Man_Name, 0, 0, 0, 0, Invoice_ID, 0, Order_Tafkeet_Value, Order_Notes, Sandouk_Cost_Center_Tree_Id, Factor, Entry_Currency_ID, Entry_Currency_Name, CarsWorkOrdersId);

                Money_Transaction_BoxCLS mxCLS = new Money_Transaction_BoxCLS();
                decimal Total_Value = 0;
                foreach (var item in mbxList)
                {
                    item.Order_Mony_Id = omMain.Id;
                    item.SubOrder_Money_Id = 0;
                    decimal Cach_Value_Comput = item.Transaction_Value.Value;
                    Total_Value = Total_Value + item.Transaction_Value.Value;

                    //long Money_Transaction_Box_ID = Insert_Money_Transaction_Box(ref opp, Order_Mony_Id2, Order_Date, Box_Currency_Id, Account_Currency_Id, Transaction_Type, Cach_Value, Cach_Value.ToString(), Box_Id, Customer_Id, Supplier_Id, SubAccount_Customer_Supplier_Id, Account_Type_Value, Notes_Details, Balance_Before, Balance_After, Balance_BeforStr, Balance_AfterStr, 0, 0, 0, 0, 0, Account_Cost_Center_Tree_Id, Account_Name, CostCenter_Name, Factor, Account_Code);
                    mxCLS.InsertMoney_Transaction_Box_(ref Excp, ref Msg, item, ref opp);


                    long ProcessNo = 0;
                    //Set_TransactionMoney_For_Customer_SupplierAccounts(ref opp, Order_Mony_Id2, Money_Transaction_Box_ID, Transaction_Type, Cach_Value, Customer_Id, Supplier_Id, SubAccount_Customer_Supplier_Id, Account_Type_Value, Notes_Details, ProcessNo);
                    Set_TransactionMoney_For_Customer_SupplierAccountsMulti_(ref Excp, ref Msg, ref opp, omMain.Id, item);

                }
                if (Excp == 0)
                {
                    decimal Box_Balance = 0;

                    Bank bnk = GetBank_Balance(omMain.Bank_Id, ref opp, ref Box_Balance);
                    Set_BankBalanceDay(ref opp, omMain.Order_Date, omMain.Bank_Id, Box_Balance, Total_Value, omMain.Transaction_type);

                }

                if (Excp == 0)
                {
                    AccountModelsLib.Accounts_Entry_POST_CLS aa = new AccountModelsLib.Accounts_Entry_POST_CLS();
                    int IS_Tar7eel_Sandouk_Automatic = 1;
                    long Branch_Id = Common.Branch_Id;
                    DateTime DateFrom = new DateTime(DateTime.Now.Year, 1, 1);
                    DateTime DateTo = new DateTime(DateTime.Now.Year, 12, 31);
                    int Excp_1 = 0;
                    string Msg_1 = "";

                    //decimal Factor = 1;
                    //if (Common.MsgBoxYesNo("هل تريد ترحيل الإذن الى الحسابات العامة بإصدار القيد", "الصندوق"))
                    //{
                    aa.Make_Entry_For_AccountsSandouk(ref Excp_1, ref Msg_1, ref opp, IS_Tar7eel_Sandouk_Automatic, Branch_Id, DateFrom, DateTo, omMain.Entry_Currency_ID ?? 1, omMain.Entry_Currency_Name, Factor, omMain.Id, 0);

                    if (Excp_1 == 0)
                    {
                        Msg_1 = Msg_1 + "\n\r\n";
                        Msg_1 = Msg_1 + "تم إصدار سند النقدية وترحيلة إلى الحسابات العامه  ";
                        //opp.Commit();


                    }
                    else
                    {
                        //Msg_1 = Msg_1 + "\n\r\n";
                        Msg_1 = Msg_1 + "خطإ فى إصدار الإذن و خطأ فى ترحيل الإذن إلى الحسابات العامه يرجى مراجعة الحسابات  وإدخال القيود";
                        //MessageBox.Show(Msg_1);
                        //opp.Rollback(); 
                    }

                    //}
                    Msg = Msg + Msg_1;
                }

            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
            }

        }
        // إذن مرتبط بعربون طلبية عميل (Customer_Order_Deposits): لا يُحذف بمحرك الحذف مباشرة حتى لا يبقى العربون يتيماً
        // (الحذف والتعديل بالحذف وإعادة الإصدار فى شاشات الديسك توب). CashVouchersService.DeleteVoucher يفك الربط أولاً ثم يحذف.
        bool IsOrderMoneyLinkedToDeposit(ref operations opp, long Order_MoneyId, ref string Msg)
        {
            DataTable dtDep = opp.return_DataTable("SELECT COUNT(*) FROM Customer_Order_Deposits WHERE Order_Money_Id = " + Order_MoneyId);
            if (dtDep != null && dtDep.Rows.Count > 0 && Convert.ToInt32(dtDep.Rows[0][0]) > 0)
            {
                Msg = "لا يمكن حذف أو تعديل إذن مرتبط بعربون طلبية عميل";
                return true;
            }
            return false;
        }

        public void Delete_Transaction_BanksBox_NewSarf(ref int Excp, ref string Msg, ref operations opp, ref long Order_Mony_Id2, DataTable dt_Order, int Transaction_Type, long Box_Id)
        {

            try
            {
                if (IsOrderMoneyLinkedToDeposit(ref opp, Order_Mony_Id2, ref Msg))
                {
                    Excp = 1;
                    return;
                }
                Delete_Order_Money(ref opp, Order_Mony_Id2);


                long Money_Transaction_Box_ID = 0;
                decimal Transaction_Value_Old = 0;
                for (int x = 0; x < dt_Order.Rows.Count; x++)
                {
                    Transaction_Value_Old = 0;
                    Money_Transaction_Box_ID = long.Parse(dt_Order.Rows[x]["Id"].ToString());
                    Transaction_Value_Old = decimal.Parse(dt_Order.Rows[x]["Transaction_Value"].ToString());


                    Transaction_Value_Old = Transaction_Value_Old * -1;



                    Delete_Money_Transaction_Box(ref opp, Order_Mony_Id2, Money_Transaction_Box_ID, Transaction_Type, Transaction_Value_Old, Box_Id);


                }


                Excp = 0;
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
            }

        }

        public void Delete_Transaction_BanksBox_NewSarf_New(ref int Excp, ref string Msg, ref operations opp, long Order_MoneyId)
        {
            long Entry_No = 0;
            Delete_Transaction_BanksBox_NewSarf_New(ref Excp, ref Msg, ref opp, Order_MoneyId, ref Entry_No);
        }
        public void Delete_Transaction_BanksBox_NewSarf_New(ref int Excp, ref string Msg, ref operations opp, long Order_MoneyId, ref long Entry_No)
        {

            try
            {

                Excp = 0;
                Msg = "";
                if (IsOrderMoneyLinkedToDeposit(ref opp, Order_MoneyId, ref Msg))
                {
                    Excp = 1;
                    return;
                }
                Order_Money omMain = opp.db.Order_Money.Where(i => i.Id == Order_MoneyId).FirstOrDefault();
                if (omMain == null)
                {
                    Excp = 2;
                    Msg = "إذن النقدية غير موجود";
                    return;
                }
                List<Money_Transaction_Box> mbxlst = opp.db.Money_Transaction_Box.Where(i => i.Order_Mony_Id == Order_MoneyId).ToList();

                //Delete_Order_Money(ref opp, omMain.Id);
                opp.db.Entry(omMain).State =  EntityState.Deleted;
                opp.db.SaveChanges();



                decimal Transaction_Value_Old = 0;
                decimal Total_Value_New = 0;
                foreach (var mbx in mbxlst)
                {
                    Total_Value_New = Total_Value_New + mbx.Transaction_Value.Value;
                    Transaction_Value_Old = mbx.Transaction_Value.Value;


                    Transaction_Value_Old = Transaction_Value_Old * -1;
                    // Delete_Money_Transaction_Box_(ref opp, Transaction_Value_Old, mbx);

                    string mysql = " ";


                    if (mbx.Transaction_Type == 1)
                    {

                        mysql = "  UPDATE Banks SET Last_Balance = Last_Balance +  " + Transaction_Value_Old + "   WHERE (Id = " + mbx.Bank_Id + ")";

                        opp.insert_me(mysql);
                    }
                    else
                    {

                        mysql = "  UPDATE Banks SET Last_Balance = Last_Balance -  " + Transaction_Value_Old + "   WHERE (Id = " + mbx.Bank_Id + ")";

                        opp.insert_me(mysql);
                    }

                    ////////////تعديل اجمالى الاذن
                    mysql = "Update Order_Money Set Total_Value=Total_Value + " + Transaction_Value_Old + " Where Id= " + mbx.Order_Mony_Id;
                    opp.insert_me(mysql);



                    mysql = "Delete From  Money_Transaction_Box Where Id= " + mbx.Id;
                    opp.insert_me(mysql);
                    // EF still tracks this row after the raw DELETE; a new row that reuses the same Id (MAX+1) would otherwise fail with a tracking-key conflict.
                    opp.db.Entry(mbx).State = EntityState.Detached;

                    try
                    {
                        mysql = "Delete From  Money_Transaction_BoxForInvoices Where Money_Transaction_BoxId= " + mbx.Id;
                        opp.insert_me(mysql);
                    }
                    catch (Exception ee)
                    {

                    }
                    Set_TransactionMoney_For_Customer_SupplierAccounts(ref opp, mbx.Order_Mony_Id, mbx.Id, mbx.Transaction_Type ?? 0, mbx.Transaction_Value ?? 0, mbx.Customer_Id, mbx.Supplier_Id, mbx.SubAccount_Id, mbx.Account_Type ?? 0, mbx.Notes, 0);

                }

                decimal Box_BalanceBeforAction = GetBank_Balance(ref opp, omMain.Bank_Id, true);
                Set_Delete_BankBalanceDay(ref opp, omMain.Order_Date, omMain.Bank_Id, Box_BalanceBeforAction, Total_Value_New, omMain.Transaction_type);

                Msg = "تم حذف إذن النقدية بنجاح";
                AccountModelsLib.Accounts_Entry_POST_CLS aa = new AccountModelsLib.Accounts_Entry_POST_CLS();

                long Branch_Id = Common.Branch_Id;
                DateTime DateFrom = new DateTime(DateTime.Now.Year, 1, 1);
                DateTime DateTo = new DateTime(DateTime.Now.Year, 12, 31);
                int Excp_1 = 0;
                string Msg_1 = "";

                //decimal Factor = 1;
                //if (Common.MsgBoxYesNo("هل تريد ترحيل الإذن الى الحسابات العامة بإصدار القيد", "الصندوق"))
                //{
                string Doc_No = omMain.Order_No.ToString();

                //operations opp2 = new operations(1);

                string MsgEntry = "";
                Entry_No = 0;
                AccountEntry_CLS ac = new AccountEntry_CLS();
                ac.Delete_EntryFromAutomaticWithRef(ref opp, ref Excp_1, ref MsgEntry, omMain.Account_Entry_List_ID ?? 0, ref Entry_No);

                if (Excp_1 == 1)
                {

                    Msg_1 = Msg_1 + "\n\r\n";
                    Msg_1 = Msg_1 + MsgEntry;


                }
                else
                {


                    Msg_1 = Msg_1 + "\n\r\n";
                    Msg_1 = Msg_1 + "تم   حذف القيد فى الحسابات العامه  ";
                }

                Msg = Msg + "\n\r";
                //Msg = Msg  + "\n\r\n";
                Msg = Msg + Msg_1;

            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = Msg + "خطأ فى حذا إذن النقدية";
                Msg = Msg + "\n\r\n";
                Msg = Msg + ee.Message;

            }

        }

        public void MakeOrderBetweenTwo_Sandouk(ref int Excp, ref string Msg, ref operations opp, string Bank_Name, ref long Order_Mony_Id2, DateTime Order_Date, string Order_No_S, long Box_Id_S, long Currency_Id_S, string Tafkeet_Value_S, string Order_No_T, long Box_Id_T, long Currency_Id_T, string Tafkeet_Value_T, decimal Cach_Value, decimal Equivalent_Value, string Man_Name, string Order_Notes, string Account_Name, decimal Factor, long CarsWorkOrdersId, long Reference_Type_Table_Id, long DataDictionaryProcessesListsId, int IsMoreTheOneBoxShow)
        {

            try
            {
                decimal Balance_S = GetBank_Balance(ref opp, Box_Id_S, true);
                if (Cach_Value > Balance_S)
                {
                    Excp = 10;
                    Msg = "رصيد الصندوق لا يسمح بالصرف";
                    return;
                }

                string Account_Code = "";
                long SubAccount_Customer_Supplier_Id = 0;

                int Account_Type_Value_S = 6;
                int Account_Type_Value_T = 6;

                //إذن صرف
                SubAccount_Customer_Supplier_Id = GetBank_SubAccount_Id(ref opp, Box_Id_T);
                Equivalent_ValueBetween_Banks = Equivalent_Value;

                IS_Posted_Flage = -1;



                Make_Transaction_BanksBox_New(ref Excp, ref Msg, ref opp, Bank_Name, Account_Type_Value_T, SubAccount_Customer_Supplier_Id, ref Order_Mony_Id2, 2, Order_No_S, Box_Id_S, Cach_Value, Order_Date, Man_Name, Currency_Id_S, Currency_Id_T, Tafkeet_Value_S, Order_Notes, Account_Name, Factor, 0, "", Account_Code, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId, IsMoreTheOneBoxShow);
                IS_Posted_Flage = 0;

                if (Excp == 1)
                {
                    return;
                }
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////
                decimal Cach_Value_Equivalent = Cach_Value * Equivalent_Value;
                try
                {
                    Equivalent_ValueBetween_Banks = 1 / Equivalent_Value;
                    Equivalent_ValueBetween_Banks = Common.RoundMe(Equivalent_ValueBetween_Banks);
                }
                catch { Equivalent_ValueBetween_Banks = 1; }
                // اذن استلام
                SubAccount_Customer_Supplier_Id = GetBank_SubAccount_Id(ref opp, Box_Id_S);
                long Order_Mony_Id = 0;



                Make_Transaction_BanksBox_New(ref Excp, ref Msg, ref opp, Bank_Name, Account_Type_Value_S, SubAccount_Customer_Supplier_Id, ref Order_Mony_Id, 1, Order_No_T, Box_Id_T, Cach_Value_Equivalent, Order_Date, Man_Name, Currency_Id_T, Currency_Id_S, Tafkeet_Value_T, Order_Notes, Account_Name, Factor, 0, "", Account_Code, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId, IsMoreTheOneBoxShow);

                Equivalent_ValueBetween_Banks = 1;


                Accounts_Entry_POST_CLS aa = new Accounts_Entry_POST_CLS();
                long Entry_No = 0;
                aa.Make_Entry_For_AccountsSandouk(ref Excp, ref Msg, ref opp, Currency_Id_S, "", Factor, Order_Mony_Id, Entry_No);

            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
            }
        }

        public void Make_Transaction_BanksBox_New(ref int Excp, ref string Msg, ref operations opp, string Bank_Name, int Account_Type_Value, long SubAccount_Customer_Supplier_Id, ref long Order_Mony_Id2, int Transaction_Type, string Order_No, long Box_Id, decimal Cach_Value, DateTime Order_Date, string Man_Name, long  Box_Currency_Id, long Account_Currency_Id, string Tafkeet_Value, string Order_Notes, string Account_Name, decimal Factor, long    Entry_Currency_ID, string Entry_Currency_Name, string Account_Code, long CarsWorkOrdersId, long Reference_Type_Table_Id, long DataDictionaryProcessesListsId, int IsMoreTheOneBoxShow)
        {


            decimal Balance_Before = 0;
            decimal Balance_After = 0;
            string Balance_BeforStr = "";
            string Balance_AfterStr = "";

            decimal Cach_Value_Comput = Cach_Value;
            string CostCenter_Name = "";

            decimal Box_Balance = GetBank_Balance(ref opp, Box_Id, true);
            //long Year_Financial_Period_Id = Common.Year_Financial_Period_Id;
            long Customer_Id = 0;
            long Supplier_Id = 0;


            if (Account_Type_Value == Common.Customer_Account_Type)
            {
                CustomerCLS cu = new CustomerCLS();
                cu.Get_CustomerBySubAccount_Id(ref opp, SubAccount_Id_);
                Customer_Id = cu.cs.Id;


            }
            if (Account_Type_Value == Common.Supplier_Account_Type)
            {
                SupplierCLS su = new SupplierCLS();
                su.Get_SupplierBySubAccount_Id(ref opp, SubAccount_Id_);
                Supplier_Id = su.sp.Id;

            }
            try
            {

                //Order_Mony_Id2 = Insert_Order_Money(ref   opp, long.Parse(Order_No), Order_Date, Transaction_Type, 0, Box_Currency_Id, 0, Box_Id, Man_Name, 0, 0, 0, 0, 0, 0, "", Order_Notes, Sandouk_Cost_Center_Tree_Id, Factor, Entry_Currency_ID, Entry_Currency_Name);

                //long Money_Transaction_Box_ID = Insert_Money_Transaction_Box(ref opp, Order_Mony_Id2, Order_Date, Box_Currency_Id, Account_Currency_Id, Transaction_Type, Cach_Value, Cach_Value.ToString(), Box_Id, SubAccount_Customer_Supplier_Id, Account_Type_Value, Order_Notes, Balance_Before, Balance_After, 0, 0, 0, 0, 0, Account_Cost_Center_Tree_Id, Account_Name, CostCenter_Name,Factor,Account_Code);

                Order_Mony_Id2 = Insert_Order_Money(ref Excp, ref Msg, ref opp, long.Parse(Order_No), Order_Date, Transaction_Type, 0, Box_Currency_Id, 0, Box_Id, Man_Name, 0, 0, 0, 0, 0, 0, Tafkeet_Value, Order_Notes, 0, Factor, Entry_Currency_ID, Entry_Currency_Name, CarsWorkOrdersId, IsMoreTheOneBoxShow);


                long Money_Transaction_Box_ID = Insert_Money_Transaction_Box(ref opp, Order_Mony_Id2, long.Parse(Order_No), Bank_Name, Order_Date, Box_Currency_Id, Account_Currency_Id, Transaction_Type, Cach_Value, Cach_Value.ToString(), Box_Id, Customer_Id, Supplier_Id, SubAccount_Customer_Supplier_Id, Account_Type_Value, Order_Notes, Balance_Before, Balance_After, Balance_BeforStr, Balance_AfterStr, 0, 0, 0, 0, 0, 0, Account_Name, CostCenter_Name, Factor, Account_Code, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);

                long ProcessNo = 0;
                Set_TransactionMoney_For_Customer_SupplierAccounts(ref opp, Order_Mony_Id2, Money_Transaction_Box_ID, Transaction_Type, Cach_Value, Customer_Id, Supplier_Id, SubAccount_Customer_Supplier_Id, Account_Type_Value, Order_Notes, ProcessNo);

                //////قيد123
                Excp = 0;
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
            }



        }
        public decimal Purchase_Ticket_Value = 0;
        public decimal Commission_Value = 0;
        public void Make_Transaction_BanksBox_NewSalesTicket(ref int Excp, ref string Msg, ref operations opp, string Bank_Name, int Account_Type_Value, long Sandouk_Cost_Center_Tree_Id, int Account_Cost_Center_Tree_Id, ref long Order_Mony_Id2, int Transaction_Type, string Order_No, long Box_Id, decimal Box_Balance, decimal Cach_Value, DateTime Order_Date, string Man_Name, long  Box_Currency_Id, long Account_Currency_Id, long  Ticket_Supplier_Id, long Customer_Id, int Reverse_Order, long SubAccount_Id, string Tafkeet_Value, string Order_Notes, decimal Total_Value, decimal Rest_Value, string Account_Name, string CostCenter_Name, decimal Factor, long    Entry_Currency_ID, string Entry_Currency_Name, long CarsWorkOrdersId, long Reference_Type_Table_Id, long DataDictionaryProcessesListsId, int IsMoreTheOneBoxShow)
        {
            //long Order_No,string Bank_Name = "";
            string mysql = "";
            decimal Balance_Before = 0;
            decimal Balance_After = 0;

            string Balance_BeforStr = "";
            string Balance_AfterStr = "";

            decimal Cach_Value_Comput = Cach_Value;
            Box_Balance = GetBank_Balance(ref opp, Box_Id, true);
            //long Year_Financial_Period_Id=Common.Year_Financial_Period_Id;
            try
            {
                #region Customer



                //if (Account_Type_Value == Common.Customer_Account_Type)
                //{
                //    CustomerCLS cu = new CustomerCLS();
                //    cu.Get_CustomerBySubAccount_Id(ref opp, SubAccount_Id);
                //    Customer_Id = cu.Id;


                //}
                //if (Account_Type_Value == Common.Supplier_Account_Type)
                //{
                //    SupplierCLS su = new SupplierCLS();
                //    su.Get_SupplierBySubAccount_Id(ref opp, SubAccount_Id);
                //    Supplier_Id = su.Id;

                //}



                Order_Mony_Id2 = Insert_Order_Money(ref Excp, ref Msg, ref opp, long.Parse(Order_No), Order_Date, Transaction_Type, 0, Box_Currency_Id, 0, Box_Id, Man_Name, 0, 0, 0, 0, 0, Ticket_Supplier_Id, "", Order_Notes, Sandouk_Cost_Center_Tree_Id, Factor, Entry_Currency_ID, Entry_Currency_Name, CarsWorkOrdersId, IsMoreTheOneBoxShow);

                long Money_Transaction_Box_ID;

                Money_Transaction_Box_ID = Insert_Money_Transaction_Box(ref opp, Order_Mony_Id2, long.Parse(Order_No), Bank_Name, Order_Date, Box_Currency_Id, Account_Currency_Id, Transaction_Type, Cach_Value, Cach_Value.ToString(), Box_Id, Customer_Id, Ticket_Supplier_Id, SubAccount_Id, Account_Type_Value, Order_Notes, Balance_Before, Balance_After, Balance_BeforStr, Balance_AfterStr, 0, 0, 0, Total_Value, Rest_Value, Account_Cost_Center_Tree_Id, Account_Name, CostCenter_Name, Factor, "", CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);






                #endregion


                Excp = 0;
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
            }



        }
        int IS_Posted_Flage = 0;

        public long Insert_Order_Money(ref int Excp, ref string Msg, ref operations opp, long Order_No, DateTime Order_Date, int Transaction_type, decimal Total_Value, long Currency_Id, int Bank_Type, long Bank_ID, string Man_Name, int Is_Balances_Set, int Day_Registration_flage, long DayReg_List_Id, int Order_mony_id_Transform, long Invoice_Id, long  Ticket_Supplier_Id, string Tafkeet_Value, string Order_Notes, long Sandouk_Cost_Center_Tree_Id, decimal Factor, long    Entry_Currency_ID, string Entry_Currency_Name, long CarsWorkOrdersId, int IsMoreTheOneBoxShow)
        {
            Order_MoneyCLS om = new Order_MoneyCLS();


            try
            {

                return om.Insert_Order_Money(ref Excp, ref Msg, ref opp, Order_No, Order_Date, Transaction_type, Total_Value, Currency_Id, Bank_Type, Bank_ID, Man_Name, Is_Balances_Set, Day_Registration_flage, DayReg_List_Id, Order_mony_id_Transform, Invoice_Id, Ticket_Supplier_Id, Tafkeet_Value, Order_Notes, Sandouk_Cost_Center_Tree_Id, Factor, Entry_Currency_ID, Entry_Currency_Name, Invoice_Type, IS_Posted_Flage, CarsWorkOrdersId, IsMoreTheOneBoxShow);



            }
            catch (Exception e)
            {
                Excp = 1;
                throw (e);
            }
        }

        public void Edit_Order_Money(ref operations opp, long Order_Money_Id, long Order_No, DateTime Order_Date, int Transaction_type, string Man_Name, string Tafkeet_Value, string Order_Notes, decimal Factor)
        {

            string mysql = " ";


            long Year_Financial_Period_Id = Financial_PeriodCLS.Get_Year_Financial_Period_Id(Order_Date);

            //Id,Serial,
            mysql = " Update  Order_Money Set  Order_No=" + Order_No + ",Order_Date=" + operations.Get_DateToInsert(Order_Date) + ",  Man_Name='" + Man_Name + "',User_Id=" + Common.User_Id + ",Year_Financial_Period_Id=" + Year_Financial_Period_Id + ",Order_Tafkeet_Value='" + Tafkeet_Value + "',Order_Notes='" + Order_Notes + "',Factor=" + Factor + ", Edit_User_Id=" + Common.User_Id + ", EditOperationDate=GETDATE() ";


            mysql = mysql + " Where Id=" + Order_Money_Id;
            opp.insert_me(mysql);

        }
        public void Delete_Order_Money(ref operations opp, long Order_Money_Id)
        {

            string mysql = " ";

            //Id,Serial,
            mysql = " Delete from  Order_Money Where Id=" + Order_Money_Id;


            opp.insert_me(mysql);

        }

        decimal Equivalent_ValueBetween_Banks = 1;
        public long InsertMoney_Transaction_Box(ref int Excp, ref string Msg, ref operations opp, long Order_Mony_Id, long Order_No, string Bank_Name, DateTime Transaction_Date, int Transaction_Type, decimal Import_Value, decimal Export_Value, decimal Transaction_Value, string Transaction_Value_, long Bank_Id, decimal Bank_Balance, long Supplier_Id, long Customer_Id, long BankAccount_Id, long Account_Id, int Account_Type, long SubAccount_Id, string Notes, decimal Balance_Before, decimal Balance_After, string Balance_BeforStr, string Balance_AfterStr, string Account_Name, long User_Id, long Branch_Id, decimal Equivalent_Value, decimal Factor, long CarsWorkOrdersId, long Reference_Type_Table_Id, long DataDictionaryProcessesListsId)
        {
            Money_Transaction_BoxCLS mt = new Money_Transaction_BoxCLS(ref opp);
            long    Currency_Id = 1;

            int DestructionSales = 0;
            int Order_mony_id_transform = -1;
            long Account_Cost_Center_Tree_Id = 0;
            decimal TotalValueWillPaid = 0;
            decimal Rest_Value = 0;
            string Account_Code = "";
            string Cost_Center = "";
            string CostCenter_Name = "";
            decimal Purchase_Ticket_Value = 0;
            decimal Commission_Value = 0;
            long Entry_Currency_ID = 0;
            string Tafkeet_Value = TafkeetProj.NumberToText.GetText(Transaction_Value, 1);

            mt.Lookup_CashPaymentMethodId = Lookup_CashPaymentMethodId;
            mt.Lookup_CashPaymentMethodName = Lookup_CashPaymentMethodName;

            return mt.InsertMoney_Transaction_Box(ref Excp, ref Msg, ref opp, Order_Mony_Id, Order_No, Bank_Name, Transaction_Date, Transaction_Type, Import_Value, Export_Value, Transaction_Value, Transaction_Value_, Equivalent_Value, Bank_Id, Bank_Balance, Supplier_Id, Customer_Id, SubAccount_Id, Notes, Balance_Before, Balance_After, Balance_BeforStr, Balance_AfterStr, BankAccount_Id, DestructionSales, Order_mony_id_transform, Account_Cost_Center_Tree_Id, Tafkeet_Value, TotalValueWillPaid, Rest_Value, Account_Name, Account_Code, Account_Type, Cost_Center, Currency_Id, Purchase_Ticket_Value, Commission_Value, Factor, Entry_Currency_ID, CostCenter_Name, Invoice_ID, Invoice_Type, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);
        }



        public long Insert_Money_Transaction_Box(ref operations opp, long Order_Mony_Id, long Order_No, string Bank_Name, DateTime Order_Date, long  Box_Currency_Id, long Account_Currency_Id, int Transaction_Type, decimal Transaction_Value, string Transaction_Value_, long Bank_Id, long Customer_Id, long Supplier_Id, long SubAccount_Customer_Supplier_Id, int Account_Type_Value, string Notes, decimal Balance_Before, decimal Balance_After, string Balance_BeforStr, string Balance_AfterStr, int BankAccount_Id, int DestructionSales, int Order_mony_id_Transform, decimal Total_Value, decimal Rest_Value, long Account_Cost_Center_Tree_Id, string Account_Name, string CostCenter_Name, decimal Factor, string Account_Code, long CarsWorkOrdersId, long Reference_Type_Table_Id, long DataDictionaryProcessesListsId)
        {
            int Excp = 0;
            string Msg = "";
            string mysql = " ";
            decimal Import_Value = 0;
            decimal Export_Value = 0;
            string Tafkeet_Value = "";
            //long Customer_Id = 0;
            //long Supplier_Id = 0;
            Tafkeet_Value = TafkeetProj.NumberToText.GetText(Transaction_Value, Account_Currency_Id);
            decimal Cach_Value_Comput = Transaction_Value;



            if (Transaction_Type == 1)
            {
                Import_Value = Transaction_Value;
                Export_Value = 0;
                mysql = "  UPDATE Banks SET Last_Balance = Last_Balance +  " + Transaction_Value + "   WHERE (Id = " + Bank_Id + ")";

                opp.insert_me(mysql);
            }
            else
            {
                Import_Value = 0;
                Export_Value = Transaction_Value;
                mysql = "  UPDATE Banks SET Last_Balance = Last_Balance -  " + Transaction_Value + "   WHERE (Id = " + Bank_Id + ")";

                opp.insert_me(mysql);
            }

            ////////////تعديل اجمالى الاذن
            mysql = "Update Order_Money Set Total_Value=Total_Value + " + Transaction_Value + " Where Id= " + Order_Mony_Id;
            opp.insert_me(mysql);
            decimal Last_Balance = GetBank_Balance(ref opp, Bank_Id, true);

            long Id = 0;
            long B_Id = 0;

            //Common.GetTable_Id(ref opp, "Money_Transaction_Box", "Serial", ref Id, ref B_Id);

            long    Currency_Id = 1;
            long Entry_Currency_ID = 1;
            long Invoice_Id = 0;
            int invoiceType_Id = 0;

            Money_Transaction_BoxCLS mt = new Money_Transaction_BoxCLS();

            B_Id = mt.InsertMoney_Transaction_Box(ref Excp, ref Msg, ref opp, Order_Mony_Id, Order_No, Bank_Name, Order_Date, Transaction_Type, Import_Value, Export_Value, Transaction_Value, Transaction_Value_, (int)Equivalent_ValueBetween_Banks, Bank_Id, Last_Balance, Supplier_Id, Customer_Id, SubAccount_Customer_Supplier_Id, Notes, Balance_Before, Balance_After, Balance_BeforStr, Balance_AfterStr, BankAccount_Id, DestructionSales, Order_mony_id_Transform, Account_Cost_Center_Tree_Id, Tafkeet_Value, Total_Value, Rest_Value, Account_Name, Account_Code, Account_Type_Value, CostCenter_Name, Currency_Id, Purchase_Ticket_Value, Commission_Value, Factor, Entry_Currency_ID, CostCenter_Name, Invoice_Id, invoiceType_Id, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);


            return B_Id;

        }
        public long Edit_Money_Transaction_Box(ref operations opp, long Order_Mony_Id, long Order_No, string Bank_Name, long Money_Transaction_Box_ID, DateTime Order_Date, long  Box_Currency_Id, long Account_Currency_Id, int Transaction_Type, decimal Transaction_Value, string Transaction_Value_, decimal DiffValue, long Bank_Id, long SubAccount_Customer_Supplier_Id, int Account_Type_Value, string Notes, decimal Balance_Before, decimal Balance_After, string Balance_BeforStr, string Balance_AfterStr, int BankAccount_Id, int DestructionSales, int Order_mony_id_Transform, decimal Total_Value, decimal Rest_Value, long Account_Cost_Center_Tree_Id, string Account_Name, string CostCenter_Name, decimal Factor, long CarsWorkOrdersId, long Reference_Type_Table_Id, long DataDictionaryProcessesListsId)
        {


            int Excp = 0;
            string Msg = "";
            string mysql = " ";
            decimal Import_Value = 0;
            decimal Export_Value = 0;
            string Tafkeet_Value = "";
            long Customer_Id = 0;
            long Supplier_Id = 0;


            Tafkeet_Value = TafkeetProj.NumberToText.GetText(Transaction_Value, Account_Currency_Id);



            decimal Cach_Value_Comput = Transaction_Value;



            if (Transaction_Type == 1)
            {
                Import_Value = Transaction_Value;
                Export_Value = 0;
                mysql = "  UPDATE Banks SET Last_Balance = Last_Balance +  " + DiffValue + "   WHERE (Id = " + Bank_Id + ")";

                opp.insert_me(mysql);
            }
            else
            {
                Import_Value = 0;
                Export_Value = Transaction_Value;
                mysql = "  UPDATE Banks SET Last_Balance = Last_Balance -  " + DiffValue + "   WHERE (Id = " + Bank_Id + ")";

                opp.insert_me(mysql);
            }

            ////////////تعديل اجمالى الاذن
            mysql = "Update Order_Money Set Total_Value=Total_Value + " + DiffValue + " Where Id= " + Order_Mony_Id;
            opp.insert_me(mysql);
            decimal Last_Balance = GetBank_Balance(ref opp, Bank_Id, true);

            long Id = 0;
            long B_Id = 0;
            long Invoice_Id = 0;
            int invoiceType_Id = 0;
            //Common.GetTable_Id(ref opp, "Money_Transaction_Box", "Serial", ref Id, ref B_Id);
            long    Currency_Id = 1;
            long Entry_Currency_ID = 1;
            string Account_Code = "";
            //B_Id = InsertMoney_Transaction_Box(ref Excp, ref Msg, ref opp, Order_Mony_Id, Order_Date, Transaction_Type, Import_Value, Export_Value, Transaction_Value, Transaction_Value.ToString(), (int)Equivalent_ValueBetween_Banks, Bank_Id, Last_Balance, Supplier_Id, Customer_Id, SubAccount_Customer_Supplier_Id, SubAccount_Customer_Supplier_Id, Notes, Balance_Before, Balance_After, Balance_BeforStr, Balance_AfterStr, BankAccount_Id, DestructionSales, Order_mony_id_Transform, Account_Cost_Center_Tree_Id, Tafkeet_Value, Total_Value, Rest_Value, Account_Name, Account_Code, Account_Type_Value, CostCenter_Name, Currency_Id, Purchase_Ticket_Value, Commission_Value, Factor, Entry_Currency_ID, CostCenter_Name, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);

            Money_Transaction_BoxCLS mt = new Money_Transaction_BoxCLS();




            B_Id = mt.InsertMoney_Transaction_Box(ref Excp, ref Msg, ref opp, Order_Mony_Id, Order_No, Bank_Name, Order_Date, Transaction_Type, Import_Value, Export_Value, Transaction_Value, Transaction_Value_, (int)Equivalent_ValueBetween_Banks, Bank_Id, Last_Balance, Supplier_Id, Customer_Id, SubAccount_Customer_Supplier_Id, Notes, Balance_Before, Balance_After, Balance_BeforStr, Balance_AfterStr, BankAccount_Id, DestructionSales, Order_mony_id_Transform, Account_Cost_Center_Tree_Id, Tafkeet_Value, Total_Value, Rest_Value, Account_Name, Account_Code, Account_Type_Value, CostCenter_Name, Currency_Id, Purchase_Ticket_Value, Commission_Value, Factor, Entry_Currency_ID, CostCenter_Name, Invoice_Id, invoiceType_Id, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);

            //if (Common.Use_Auto_Identity_Column == 0)
            //{
            //    //Purchase_Ticket_Value,Commission_Value 

            //    mysql = " INSERT INTO Money_Transaction_Box(Id,Serial,Bank_Balance, Order_Mony_Id, Transaction_Date, Transaction_Type, Import_Value, Export_Value, Transaction_Value, Transaction_Value_, Bank_Id, Supplier_Id, Customer_Id, Account_Id, SubAccount_Id, Notes, Balance_Before, Balance_After, BankAccount_Id,DestructionSales, Order_mony_id_transform,User_Id,Branch_Id,Tafkeet_Value,TotalValueWillPaid,Rest_Value,Account_Cost_Center_Tree_Id,Account_Type,Equivalent_Value,Purchase_Ticket_Value,Commission_Value ,Account_Name,CostCenter_Name,Cost_Center,Factor) ";
            //    mysql = mysql + " Values(" + B_Id + "," + Id + "," + Last_Balance + ", " + Order_Mony_Id + ", " + operations.Get_DateToInsert(Order_Date) + " ," + Transaction_Type + "," + Import_Value + "," + Export_Value + "," + Transaction_Value + ",'" + Transaction_Value.ToString() + "'," + Bank_Id + "," + Supplier_Id + "," + Customer_Id + "," + SubAccount_Customer_Supplier_Id + "," + SubAccount_Customer_Supplier_Id + ",  '" + Notes + "'," + Balance_Before + "," + Balance_After + "," + BankAccount_Id + "," + DestructionSales + "," + Order_mony_id_Transform + " ," + Common.User_Id + "," + Common.Branch_Id + ",'" + Tafkeet_Value + "'," + Total_Value + "," + Rest_Value + "," + Account_Cost_Center_Tree_Id + "," + Account_Type_Value + "," + Equivalent_ValueBetween_Banks + "," + Purchase_Ticket_Value + "," + Commission_Value + ",'" + Account_Name + "','" + CostCenter_Name + "','" + CostCenter_Name + "'," + Factor + ")";

            //    opp.insert_me(mysql);


            //}
            //else
            //{

            //    mysql = " INSERT INTO Money_Transaction_Box(Serial,Bank_Balance, Order_Mony_Id, Transaction_Date, Transaction_Type, Import_Value, Export_Value, Transaction_Value, Transaction_Value_, Bank_Id, Supplier_Id, Customer_Id, Account_Id, SubAccount_Id, Notes, Balance_Before, Balance_After, BankAccount_Id,DestructionSales, Order_mony_id_transform,User_Id,Branch_Id,Tafkeet_Value,TotalValueWillPaid,Rest_Value,Account_Cost_Center_Tree_Id,Account_Type,Equivalent_Value,Purchase_Ticket_Value,Commission_Value,Account_Name,CostCenter_Name ,Cost_Center,Factor) ";
            //    mysql = mysql + " Values(  " + Id + "," + Last_Balance + "," + Order_Mony_Id + ", " + operations.Get_DateToInsert(Order_Date) + " ," + Transaction_Type + "," + Import_Value + "," + Export_Value + "," + Transaction_Value + ",'" + Transaction_Value.ToString() + "'," + Bank_Id + "," + Supplier_Id + "," + Customer_Id + "," + SubAccount_Customer_Supplier_Id + "," + SubAccount_Customer_Supplier_Id + ", '" + Notes + "'," + Balance_Before + "," + Balance_After + "," + BankAccount_Id + "," + DestructionSales + "," + Order_mony_id_Transform + " ," + Common.User_Id + "," + Common.Branch_Id + ",'" + Tafkeet_Value + "'," + Total_Value + "," + Rest_Value + "," + Account_Cost_Center_Tree_Id + "," + Account_Type_Value + "," + Equivalent_ValueBetween_Banks + "," + Purchase_Ticket_Value + "," + Commission_Value + ",'" + CostCenter_Name + "','" + CostCenter_Name + "'," + Factor + ")";


            //    B_Id = opp.Insert_me_Return_Last_Identify(mysql);
            //}
            return B_Id;

        }

        public void Delete_Money_Transaction_Box(ref operations opp, long Order_Mony_Id, long Money_Transaction_Box_ID, int Transaction_Type, decimal Transaction_Value, long Bank_Id)
        {
            string mysql = " ";


            if (Transaction_Type == 1)
            {

                mysql = "  UPDATE Banks SET Last_Balance = Last_Balance +  " + Transaction_Value + "   WHERE (Id = " + Bank_Id + ")";

                opp.insert_me(mysql);
            }
            else
            {

                mysql = "  UPDATE Banks SET Last_Balance = Last_Balance -  " + Transaction_Value + "   WHERE (Id = " + Bank_Id + ")";

                opp.insert_me(mysql);
            }

            ////////////تعديل اجمالى الاذن
            mysql = "Update Order_Money Set Total_Value=Total_Value + " + Transaction_Value + " Where Id= " + Order_Mony_Id;
            opp.insert_me(mysql);



            mysql = "Delete From  Money_Transaction_Box Where Id= " + Money_Transaction_Box_ID;
            opp.insert_me(mysql);

            try
            {
                mysql = "Delete From  Money_Transaction_BoxForInvoices Where Money_Transaction_BoxId= " + Money_Transaction_Box_ID;
                opp.insert_me(mysql);
            }
            catch (Exception ee)
            {

            }

        }
        //public void Add_New_Cash(ref int Excp, long Order_Mony_Id, DateTime Day_Date, int Transaction_Type, long  Bank_Id, decimal Bank_Balance, decimal Cash_Value, string Cash_Value_, long Customer_Id, long Supplier_Id, int Account_Id, string Notes, int Account_Selection_)
        //{
        //    operations opp = new operations(1);
        //    int Supplier_Customer = Account_Selection_;

        //    long Identity = 0;
        //    string mysql = "";
        //    decimal Balance_Before = Account_Selection.Supplier_Customer_Balance;
        //    decimal Balance_After = 0;
        //    int SubAccountRef = 0;
        //    if (Account_Id == 10)
        //    {
        //        SubAccountRef = Customer_Id;
        //        Balance_After = Balance_Before - Cash_Value;
        //    }
        //    else if (Account_Id == 11)
        //    {
        //        SubAccountRef = Supplier_Id;
        //        Balance_After = Balance_Before - Cash_Value;
        //    }
        //    else
        //    {
        //        Balance_After = 0;
        //    }


        //    try
        //    {

        //        mysql = "Select * from Banks Where id=" + Bank_id;
        //        DataSet dsBank_Balance = opp.return_dataset(mysql);
        //        if (dsBank_Balance.Tables[0].Rows.Count > 0)
        //        {
        //            Bank_Balance = decimal.Parse(dsBank_Balance.Tables[0].Rows[0]["Last_Balance"].ToString());
        //        }
        //        else
        //        {

        //        }
        //    }
        //    catch { }

        //    try
        //    {
        //        //ArrayList arr=new ArrayList();


        //        if (Check_Static_Data.Transaction_Type == 1)
        //        {

        //            Bank_Balance = Bank_Balance + Cash_Value;
        //            mysql = " INSERT INTO Money_Transaction_Box (Order_Mony_Id, Transaction_Date, Transaction_Type,Import_Value, Transaction_Value, Transaction_Value_, Bank_Id, Bank_Balance, Supplier_Id, Customer_Id,Account_Id, Notes,Balance_Before,Balance_After) ";
        //            mysql = mysql + " VALUES (" + Order_Mony_Id + ",'" + operations.dateFormat(Day_Date) + "'," + Transaction_Type + "," + Cash_Value + "," + Cash_Value + ",'" + Cash_Value_ + "'," + Bank_id + "," + Bank_Balance + "," + Supplier_Id + "," + Customer_Id + "," + Account_Id + ",'" + Notes + "'," + Balance_Before + "," + Balance_After + ")";
        //            // arr.Add(mysql);

        //            Identity = opp.Insert_me_Return_Last_Identify(mysql, ref Excp);

        //            /// if the insert  sql not executed
        //            if (Excp != 0)
        //            {
        //                opp = null;
        //                Excp = 1;
        //                return;
        //            }
        //            ///ليس رصيد أول المدة
        //            if (Common.Balance_BeginingPeriod == 0)
        //            {
        //                mysql = "  UPDATE Banks SET Last_Balance = Last_Balance +  " + Cash_Value + "   WHERE (Id = " + Bank_id + ")";
        //                opp.insert_me(mysql);
        //                // arr.Add(mysql);
        //            }
        //            /*
        //            if (Check_Static_Data.MainSubAccount_ID==93)
        //            {
        //                mysql = " UPDATE Submain_Account SET Submain_Account_Balance = Submain_Account_Balance - " + Cash_Value + " WHERE (SubAccount_Id = " + Account_Id + ") ";
        //                arr.Add(mysql);
        //            }
        //             * */
        //            Add_AnotherStaticAccountOnSubmainTable(ref opp, Account_Id, Transaction_Type, Cash_Value, SubAccountRef, Bank_id, Day_Date);
        //        }

        //        if (Check_Static_Data.Transaction_Type == 2)
        //        {
        //            // Balance_After = Balance_Before +  Cash_Value;
        //            Bank_Balance = Bank_Balance - Cash_Value;
        //            mysql = " INSERT INTO Money_Transaction_Box (Order_Mony_Id, Transaction_Date, Transaction_Type,Export_Value, Transaction_Value, Transaction_Value_, Bank_Id, Bank_Balance, Supplier_Id, Customer_Id,Account_Id, Notes, Balance_Before,Balance_After) ";
        //            mysql = mysql + " VALUES (" + Order_Mony_Id + ",'" + operations.dateFormat(Day_Date) + "'," + Transaction_Type + "," + Cash_Value + "," + Cash_Value + ",'" + Cash_Value_ + "'," + Bank_id + "," + Bank_Balance + "," + Supplier_Id + "," + Customer_Id + "," + Account_Id + ",'" + Notes + "'," + Balance_Before + "," + Balance_After + ")";
        //            //arr.Add(mysql);


        //            Identity = opp.Insert_me_Return_Last_Identify(mysql, ref Excp);

        //            /// if the insert  sql not executed
        //            if (Excp != 0)
        //            {
        //                opp = null;
        //                Excp = 1;
        //                return;
        //            }
        //            ///ليس رصيد أول المدة
        //            if (Common.Balance_BeginingPeriod == 0)
        //            {
        //                mysql = "  UPDATE Banks SET Last_Balance = Last_Balance -  " + Cash_Value + "   WHERE (Id = " + Bank_id + ")";
        //                //  arr.Add(mysql);
        //                opp.insert_me(mysql);
        //            }
        //            /*
        //            if (Check_Static_Data.MainSubAccount_ID == 93)
        //            {
        //                mysql = " UPDATE Submain_Account SET Submain_Account_Balance = Submain_Account_Balance + " + Cash_Value + " WHERE (SubAccount_Id = " + Account_Id + ") ";
        //                arr.Add(mysql);
        //            }
        //             * */
        //            //Add_AnotherStaticAccountOnSubmainTable(ref arr, Account_Id, Transaction_Type, Cash_Value);
        //            Add_AnotherStaticAccountOnSubmainTable(ref opp, Account_Id, Transaction_Type, Cash_Value, SubAccountRef, Bank_id, Day_Date);

        //        }



        //        mysql = " UPDATE Order_Money SET Total_Value = Total_Value + " + Cash_Value + " WHERE (id =" + Order_Mony_Id + ") ";
        //        //arr.Add(mysql);
        //        opp.insert_me(mysql);

        //        //(Account_Selection == 0)             حسابات أخرى
        //        if (Account_Selection_ == 2)  //عملاء
        //        {
        //            ///ليس رصيد أول المدة
        //            if (Common.Balance_BeginingPeriod == 0)
        //            {
        //                mysql = "  UPDATE Customer SET Balance = Balance -  " + Cash_Value + " , Balance_Actual = Balance_Actual - " + Cash_Value + "  WHERE (Id = " + Customer_Id + ")";
        //                // arr.Add(mysql);
        //                opp.insert_me(mysql);
        //            }

        //            mysql = " INSERT INTO Customers_Accounts (Customer_Id, Reference_Id, Reference_Type_Table_Id, Import , Balance, Reference_Type, Transaction_Type, Transaction_Date,Payment_Type) ";
        //            mysql = mysql + " SELECT Money_Transaction_Box.Customer_Id, Order_Money.Order_No, Money_Transaction_Box.Id, Money_Transaction_Box.Import_Value,  Customer.Balance,3 as  Reference_Type," + Check_Static_Data.Transaction_Type + " AS Transaction_Type, Money_Transaction_Box.Transaction_Date ,1 AS Payment_Type";
        //            mysql = mysql + "  FROM Money_Transaction_Box INNER JOIN  Customer ON Money_Transaction_Box.Customer_Id = Customer.Id ";
        //            mysql = mysql + "  INNER JOIN Order_Money ON Money_Transaction_Box.Order_Mony_Id = Order_Money.Id ";
        //            mysql = mysql + " WHERE (Money_Transaction_Box.Order_Mony_Id = " + Order_Mony_Id + ") AND (Money_Transaction_Box.Customer_Id = " + Customer_Id + ") AND (Money_Transaction_Box.id=" + Identity + ")";
        //            // arr.Add(mysql);
        //            opp.insert_me(mysql);
        //        }
        //        if (Account_Selection_ == 1)       // موردين
        //        {

        //            if (Check_Static_Data.Transaction_Type == 2)  // صرف نقدية
        //            {
        //                ///ليس رصيد أول المدة
        //                if (Common.Balance_BeginingPeriod == 0)
        //                {
        //                    mysql = " UPDATE Supplier SET Balance = Balance -  " + Cash_Value + "   WHERE (Id = " + Supplier_Id + ")";
        //                    // arr.Add(mysql);
        //                    opp.insert_me(mysql);
        //                }

        //                mysql = " INSERT INTO Suppliers_Accounts  (Reference_Id, Reference_Type_Table_Id, Balance, Supplier_Id, Export,Reference_Type, Transaction_Type, Transaction_Date,Payment_Type) ";
        //                mysql = mysql + " SELECT Order_Money.Order_No, Money_Transaction_Box.Id, Supplier.Balance, Money_Transaction_Box.Supplier_Id,  Money_Transaction_Box.Export_Value,4 AS Reference_Type , " + Check_Static_Data.Transaction_Type + " AS Transaction_Type, Money_Transaction_Box.Transaction_Date ,1 AS Payment_Type";
        //                mysql = mysql + " FROM Money_Transaction_Box INNER JOIN  Supplier ON Money_Transaction_Box.Supplier_Id = Supplier.Id ";
        //                mysql = mysql + "  INNER JOIN Order_Money ON Money_Transaction_Box.Order_Mony_Id = Order_Money.Id ";
        //                mysql = mysql + " WHERE (Money_Transaction_Box.Order_Mony_Id = " + Order_Mony_Id + ") AND (Money_Transaction_Box.Supplier_Id = " + Supplier_Id + ")  AND (Money_Transaction_Box.id=" + Identity + ")";
        //                //   arr.Add(mysql);
        //                opp.insert_me(mysql);
        //            }
        //            ////////////***********************************************************************************
        //            if (Check_Static_Data.Transaction_Type == 1)  // استلام نقدية
        //            {
        //                ///ليس رصيد أول المدة
        //                if (Common.Balance_BeginingPeriod == 0)
        //                {
        //                    mysql = " UPDATE Supplier SET Balance = Balance -  " + Cash_Value + "   WHERE (Id = " + Supplier_Id + ")";
        //                    //  arr.Add(mysql);
        //                    opp.insert_me(mysql);
        //                }
        //                mysql = " INSERT INTO Suppliers_Accounts  (Reference_Id, Reference_Type_Table_Id, Balance, Supplier_Id, Export,Reference_Type, Transaction_Type, Transaction_Date,Payment_Type) ";
        //                mysql = mysql + " SELECT Order_Money.Order_No, Money_Transaction_Box.Id, Supplier.Balance, Money_Transaction_Box.Supplier_Id,  Money_Transaction_Box.Import_Value,3 AS Reference_Type , " + Check_Static_Data.Transaction_Type + " AS Transaction_Type, Money_Transaction_Box.Transaction_Date ,1 AS Payment_Type";
        //                mysql = mysql + " FROM Money_Transaction_Box INNER JOIN  Supplier ON Money_Transaction_Box.Supplier_Id = Supplier.Id ";
        //                mysql = mysql + "  INNER JOIN Order_Money ON Money_Transaction_Box.Order_Mony_Id = Order_Money.Id ";
        //                mysql = mysql + " WHERE (Money_Transaction_Box.Order_Mony_Id = " + Order_Mony_Id + ") AND (Money_Transaction_Box.Supplier_Id = " + Supplier_Id + ")  AND (Money_Transaction_Box.id=" + Identity + ")";
        //                //  arr.Add(mysql);
        //                opp.insert_me(mysql);
        //            }


        //        }



        //        //if(opp.insert_me_transaction(arr)==1)
        //        //{
        //        //    opp=null;
        //        //    Excp=0;

        //        //}
        //        //else
        //        //{

        //        //    Excp=1;
        //        //    //mysql = " delete from Money_Transaction_Box where (id=" + Identity + ")";
        //        //    //opp.delete_me(mysql);
        //        //    opp = null;

        //        //} 
        //        opp.Commit();
        //        Excp = 0;
        //        opp = null;
        //    }
        //    catch (Exception ee)
        //    {
        //        opp.Rollback();
        //        Excp = 1;
        //        //mysql = " delete from Money_Transaction_Box where (id=" + Identity + ")";
        //        //opp.delete_me(mysql);
        //        opp = null;
        //    }

        //}


        public void MakeMoney_TransactionFromInvoice(ref int Excp, ref string Msg, ref operations opp, DateTime Invoice_Date, long Invoice_No, long Invoice_Id, long Box_Id, long Currency_Id, long Customer_Id, decimal PaidValueFromMarket, decimal RestValue, long CarsWorkOrdersId, long Reference_Type_Table_Id, long DataDictionaryProcessesListsId)
        {

            ////make order Money

            Money_Transaction_BoxOrderReportCLS mt = new Money_Transaction_BoxOrderReportCLS();
            mt.Invoice_Type = Common.Invoice_Type_Id;
            Excp = 0;

            int Transaction_Type = 1;
            if (Box_Id == 0)
            {
                Box_Id = mt.GetDefaultBank_ID(ref opp);// int.Parse(ot.banks_List1.cmbBankType.SelectedValue.ToString());
            }
            decimal Box_Balance = 0;
            string Order_No = mt.Make_New_Order_No(ref Excp,ref opp, Transaction_Type).ToString();
            string mysql = "Select * from Banks Where Id=" + Box_Id;
            DataTable dtBanks = opp.return_DataTable(mysql);
            if (dtBanks.Rows.Count > 0)
            {

                Box_Balance = decimal.Parse(dtBanks.Rows[0]["Last_Balance"].ToString());
                decimal Cach_Value = PaidValueFromMarket;// ot.nuValue.Value;
                DateTime Order_Date = Invoice_Date;
                string Man_Name = " فاتورة مبيعات رقم " + Invoice_No;
                int Reverse_Order = 0;
                long Order_Mony_Id2 = 0;
                mt.Invoice_ID = Invoice_Id;

                long Entry_No = 0;
                long Money_Transaction_Box_ID = 0;
                mt.Make_Transaction_BanksBox_FromInvoices(ref Excp, ref Msg, ref opp, ref Order_Mony_Id2, ref Money_Transaction_Box_ID, Transaction_Type, Order_No, Box_Id, Box_Balance, Cach_Value, Order_Date, Man_Name, Currency_Id, Customer_Id, Reverse_Order, Entry_No, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);
                ////print Order
                //Order_Receipt_Cash oo = new Order_Receipt_Cash();
                //DataSet dsOrderMoney = oo.Get_Order_Cash(ref Excp, Order_Mony_Id2);
                //Order_Receipt_CashFrm or = new Order_Receipt_CashFrm(Order_Mony_Id2, Transaction_Type, dsOrderMoney, 0);
                //or.LoadDataOnForm();
                //or.Set_Print(1);
                //or = null;

                string Bank_Name = dtBanks.Rows[0]["Bank_Name"].ToString();

                mysql = "Update Invoices_List Set BanksNamePaymentMethod =N'" + Bank_Name + "' ,RestValue=" + RestValue + ",PaidValue=" + PaidValueFromMarket + " , Order_MoneyId=" + Order_Mony_Id2 + ", BanksId=" + Box_Id + " Where Id=" + Invoice_Id;
                opp.insert_me(mysql);

                 
            }
            else
            {
                Msg = Msg + "\n";
                Msg = "لم يتم إصدار النقدية ربما لعدم وجود صندوق إفتراضى للفواتير";
            }

        }

        public void MakeMoney_TransactionFromInvoiceReverse(ref int Excp, ref string Msg, ref operations opp, DateTime Invoice_Date, long Invoice_No, long Invoice_Id, long Box_Id, long Currency_Id, long Customer_Id, decimal PaidValueFromMarket, decimal RestValue, long CarsWorkOrdersId, long Reference_Type_Table_Id, long DataDictionaryProcessesListsId)
        {

            ////make order Money

            Money_Transaction_BoxOrderReportCLS mt = new Money_Transaction_BoxOrderReportCLS();
            mt.Invoice_Type = Common.InvoiceReverse_Type_Id;
            Excp = 0;

            int Transaction_Type = 2;
            if (Box_Id == 0)
            {
                Box_Id = mt.GetDefaultBank_ID();// int.Parse(ot.banks_List1.cmbBankType.SelectedValue.ToString());
            }
            decimal Box_Balance = 0;
            string Order_No = mt.Make_New_Order_No(ref Excp, Transaction_Type).ToString();
            string mysql = "Select * from Banks Where Id=" + Box_Id;
            DataTable dtBanks = opp.return_DataTable(mysql);
            if (dtBanks.Rows.Count > 0)
            {

                Box_Balance = decimal.Parse(dtBanks.Rows[0]["Last_Balance"].ToString());
                decimal Cach_Value = PaidValueFromMarket;// ot.nuValue.Value;
                DateTime Order_Date = Invoice_Date;
                string Man_Name = " فاتورة مرتجع مبيعات رقم " + Invoice_No;
                int Reverse_Order = 1;
                long Order_Mony_Id2 = 0;
                mt.Invoice_ID = Invoice_Id;

                long Entry_No = 0;
                long Money_Transaction_Box_ID = 0;
                mt.Make_Transaction_BanksBox_FromInvoices(ref Excp, ref Msg, ref opp, ref Order_Mony_Id2, ref Money_Transaction_Box_ID, Transaction_Type, Order_No, Box_Id, Box_Balance, Cach_Value, Order_Date, Man_Name, Currency_Id, Customer_Id, Reverse_Order, Entry_No, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);
                ////print Order
                //Order_Receipt_Cash oo = new Order_Receipt_Cash();
                //DataSet dsOrderMoney = oo.Get_Order_Cash(ref Excp, Order_Mony_Id2);
                //Order_Receipt_CashFrm or = new Order_Receipt_CashFrm(Order_Mony_Id2, Transaction_Type, dsOrderMoney, 0);
                //or.LoadDataOnForm();
                //or.Set_Print(1);
                //or = null;

                string Bank_Name = dtBanks.Rows[0]["Bank_Name"].ToString();

                mysql = "Update Invoices_List_Reverse Set BanksNamePaymentMethod =N'" + Bank_Name + "' ,RestValue=" + RestValue + ",PaidValue=" + PaidValueFromMarket + " , Order_MoneyId=" + Order_Mony_Id2 + ", BanksId=" + Box_Id + " Where Id=" + Invoice_Id;
                opp.insert_me(mysql);
            }
            else
            {
                Msg = Msg + "\n";
                Msg = "لم يتم إصدار النقدية ربما لعدم وجود صندوق إفتراضى للفواتير";
            }

        }

        public void MakeMoney_TransactionFromMultiInvoice(ref int Excp, ref string Msg, Money_Transaction_BoxVM mtVM)
        {

            DateTime Invoice_Date = mtVM.Order_Date;
            long Invoice_No = mtVM.Invoice_No;
            long Invoice_Id = mtVM.Invoice_Id;
            long Box_Id = mtVM.Box_Id;
            long    Currency_Id = mtVM.Currency_Id;
            long Customer_Id = mtVM.Customer_ID_Supplier;
            decimal Cach_Value = mtVM.Cach_Value;
            decimal RestValue = mtVM.RestValue;
            long CarsWorkOrdersId = mtVM.CarsWorkOrdersId;
            long Reference_Type_Table_Id = mtVM.Reference_Type_Table_Id;
            long DataDictionaryProcessesListsId = mtVM.DataDictionaryProcessesListsId;
            operations opp = new operations();
            Money_Transaction_BoxOrderReportCLS mt = new Money_Transaction_BoxOrderReportCLS();
            mt.Invoice_Type = Common.Invoice_Type_Id;
            Excp = 0;

            int Transaction_Type = 1;
            if (Box_Id == 0)
            {
                Box_Id = mt.GetDefaultBank_ID();// int.Parse(ot.banks_List1.cmbBankType.SelectedValue.ToString());
            }
            decimal Box_Balance = 0;
            string Order_No = mt.Make_New_Order_No(ref Excp, Transaction_Type).ToString();
            string mysql = "Select * from Banks Where Id=" + Box_Id;
            DataTable dtBanks = opp.return_DataTable(mysql);
            if (dtBanks.Rows.Count > 0)
            {

                mtVM.Box_Balance = decimal.Parse(dtBanks.Rows[0]["Last_Balance"].ToString());

                DateTime Order_Date = Invoice_Date;
                mtVM.Man_Name = " فاتورة مبيعات رقم " + Invoice_No;
                int Reverse_Order = 0;
                long Order_Mony_Id2 = 0;
                mt.Invoice_ID = Invoice_Id;

                long Entry_No = 0;
                long Money_Transaction_Box_ID = 0;
                //ref Order_Mony_Id2, ref Money_Transaction_Box_ID, Transaction_Type, Order_No, Box_Id, Box_Balance, Cach_Value, Order_Date, Man_Name, Currency_id, Customer_Id, Reverse_Order, Entry_No, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId
                mt.Make_Transaction_BanksBox_FromMultiInvoices(ref Excp, ref Msg, ref opp, mtVM);


                string Bank_Name = dtBanks.Rows[0]["Bank_Name"].ToString();
                mysql = "";
                foreach (var inv in mtVM.InvoicesLst)
                {
                    Invoice_Id = inv.Id;

                    mysql = mysql + " Update Invoices_List Set BanksNamePaymentMethod =N'" + Bank_Name + "' ,RestValue=0 ,PaidValue=" + mtVM.Cach_Value + " , Order_MoneyId=" + mtVM.Order_Mony_Id + ",mtVM.Money_Transaction_BoxID =" + mtVM.Money_Transaction_Box_ID + ", BanksId=" + Box_Id + " Where Id=" + Invoice_Id;
                }
                opp.insert_me(mysql);


            }
            else
            {
                Msg = Msg + "\n";
                Msg = "لم يتم إصدار النقدية ربما لعدم وجود صندوق إفتراضى للفواتير";
            }

        }

        public void MakeMoney_TransactionFromBill(ref int Excp, ref string Msg, ref operations opp, DateTime Invoice_Date, long Invoice_No, long Bills_Id, long Box_Id, long Currency_Id, long Supplier_Id, decimal PaidValueFromMarket, decimal RestValue, int Reverse_Order, long Entry_No, long CarsWorkOrdersId, long Reference_Type_Table_Id, long DataDictionaryProcessesListsId)
        {

            ////make order Money
            string mysql = "";
            Money_Transaction_BoxOrderReportCLS mt = new Money_Transaction_BoxOrderReportCLS();
            mt.Invoice_Type = Common.Bill_Type_Id;
            Excp = 0;
            Bank bank;
            int Transaction_Type = 2;
            if (Box_Id == 0)
            {
                //Box_Id = mt.GetDefaultBank_ID(ref opp);
                bank = mt.GetDefaultBank(ref opp);
                Box_Id = bank.Id;
            }
            else
            {
                // كان يبقى bank = new Bank() الفارغة هنا فيُكتب BanksNamePaymentMethod فارغاً فى Bills_List
                // لكل فاتورة فيها صندوق محدَّد صراحةً (الحالة الشائعة) بدل اسم الصندوق الفعلى.
                bank = opp.db.Banks.AsNoTracking().Where(b => b.Id == Box_Id).FirstOrDefault();
            }


            decimal Box_BalanceBeforAction = GetBank_Balance(ref opp,  Box_Id, true);

            if (Transaction_Type == 2)
            {
                if (Box_BalanceBeforAction < PaidValueFromMarket)
                {
                    // كان يعود هنا بـExcp=0 فيظن المستدعى (حفظ الفاتورة) أن كل شىء نجح ويعتمد الفاتورة
                    // كاملة رغم عدم إنشاء أى سند أو حركة خزنة على الإطلاق - بند نقص رصيد الخزنة صامت.
                    Excp = 1;
                    Msg = Msg + "\n\r";
                    // النص العربى الأصلى هنا كان تالفاً بالفعل (بايتات U+FFFD غير قابلة للاسترجاع) قبل هذا التعديل؛
                    // استُبدل بنص واضح بنفس المعنى لأن الخطأ صار يظهر فعلياً للمستخدم بعد إصلاح حارس Excp أعلاه.
                    Msg = Msg + "لا يوجد رصيد كافى فى الخزنة لسداد قيمة هذه الفاتورة نقداً";
                    return;
                }

            }
            string Order_No = mt.Make_New_Order_No(ref Excp, ref opp, Transaction_Type).ToString();
            //string mysql = "Select * from Banks Where Id=" + Box_Id;
            //DataTable dtBanks = opp.return_DataTable(mysql);
            if (bank != null)
            {


                decimal Cach_Value = PaidValueFromMarket;// ot.nuValue.Value;
                DateTime Order_Date = Invoice_Date;


                string Man_Name = " فاتورة مشتريات رقم " + Invoice_No;


                long Order_Mony_Id2 = 0;
                mt.Invoice_ID = Bills_Id;
                long Money_Transaction_Box_ID = 0;
                mt.Make_Transaction_BanksBox_FromInvoices(ref Excp, ref Msg, ref opp, ref Order_Mony_Id2, ref Money_Transaction_Box_ID, Transaction_Type, Order_No,  Box_Id, Box_BalanceBeforAction, Cach_Value, Order_Date, Man_Name, Currency_Id, Supplier_Id, Reverse_Order, Entry_No, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);
                ////print Order
                //Order_Receipt_Cash oo = new Order_Receipt_Cash();
                //DataSet dsOrderMoney = oo.Get_Order_Cash(ref Excp, Order_Mony_Id2);
                //Order_Receipt_CashFrm or = new Order_Receipt_CashFrm(Order_Mony_Id2, Transaction_Type, dsOrderMoney, 0);
                //or.LoadDataOnForm();
                //or.Set_Print(1);
                //or = null;

                string Bank_Name = bank.Bank_Name;// dtBanks.Rows[0]["Bank_Name"].ToString();

                mysql = "Update Bills_List Set BanksNamePaymentMethod =N'" + Bank_Name + "' ,RestValue=" + RestValue + ",PaidValue=" + PaidValueFromMarket + " , Order_MoneyId=" + Order_Mony_Id2 + ", BanksId=" + Box_Id + " Where Id=" + Bills_Id;
                opp.insert_me(mysql);

                Msg = Msg + "\n\r";
                Msg = Msg + "  تم إصدار صرف النقدية  ";
            }
            else
            {
                // نفس حارس الإكسبشن: صندوق غير موجود (bank=null) يجب ألا يمر بصمت أيضاً.
                Excp = 1;
                Msg = Msg + "\n\r";
                Msg = Msg + "لم يتم العثور على صندوق نقدية افتراضى لتسجيل سداد هذه الفاتورة";
            }

        }

        public void MakeMoney_TransactionFromBill_Reverse(ref int Excp, ref string Msg, ref operations opp, DateTime Invoice_Date, long Invoice_No, long Bills_Id, long Box_Id, long Currency_Id, long Supplier_Id, decimal PaidValueFromMarket, decimal RestValue, int Reverse_Order, long Entry_No, long CarsWorkOrdersId, long Reference_Type_Table_Id, long DataDictionaryProcessesListsId)
        {

            ////make order Money
            string mysql = "";
            Money_Transaction_BoxOrderReportCLS mt = new Money_Transaction_BoxOrderReportCLS();
            mt.Invoice_Type = Common.BilleReverse_Type_Id;
            Excp = 0;
            Bank bank;
            int Transaction_Type = 1;
            if (Box_Id == 0)
            {
                //Box_Id = mt.GetDefaultBank_ID(ref opp);
                bank = mt.GetDefaultBank(ref opp);
                Box_Id = bank.Id;
            }
            else
            {
                // نفس إصلاح المسار الأمامى أعلاه: يجب تحميل الصندوق الفعلى لا ترك Bank فارغ.
                bank = opp.db.Banks.AsNoTracking().Where(b => b.Id == Box_Id).FirstOrDefault();
            }


            decimal Box_BalanceBeforAction = GetBank_Balance(ref opp,  Box_Id, true);

            if (Transaction_Type == 2)
            {
                if (Box_BalanceBeforAction < PaidValueFromMarket)
                {
                    Msg = Msg + "\n\r";
                    Msg = Msg + "لم يتم إصدار النقدية   لعدم وجود صندوق إفتراضى للفواتير";
                    return;

                }

            }
            string Order_No = mt.Make_New_Order_No(ref Excp, ref opp, Transaction_Type).ToString();
            //string mysql = "Select * from Banks Where Id=" + Box_Id;
            //DataTable dtBanks = opp.return_DataTable(mysql);
            if (bank != null)
            {


                decimal Cach_Value = PaidValueFromMarket;// ot.nuValue.Value;
                DateTime Order_Date = Invoice_Date;


                string Man_Name = " فاتورة مرتجع مشتريات رقم " + Invoice_No;


                long Order_Mony_Id2 = 0;
                mt.Invoice_ID = Bills_Id;
                long Money_Transaction_Box_ID = 0;
                mt.Make_Transaction_BanksBox_FromInvoices(ref Excp, ref Msg, ref opp, ref Order_Mony_Id2, ref Money_Transaction_Box_ID, Transaction_Type, Order_No, Box_Id, Box_BalanceBeforAction, Cach_Value, Order_Date, Man_Name, Currency_Id, Supplier_Id, Reverse_Order, Entry_No, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);
                ////print Order
                //Order_Receipt_Cash oo = new Order_Receipt_Cash();
                //DataSet dsOrderMoney = oo.Get_Order_Cash(ref Excp, Order_Mony_Id2);
                //Order_Receipt_CashFrm or = new Order_Receipt_CashFrm(Order_Mony_Id2, Transaction_Type, dsOrderMoney, 0);
                //or.LoadDataOnForm();
                //or.Set_Print(1);
                //or = null;

                string Bank_Name = bank.Bank_Name;// dtBanks.Rows[0]["Bank_Name"].ToString();

                mysql = "Update Bills_List_Reverse Set BanksNamePaymentMethod =N'" + Bank_Name + "' ,RestValue=" + RestValue + ",PaidValue=" + PaidValueFromMarket + " , Order_MoneyId=" + Order_Mony_Id2 + ", BanksId=" + Box_Id + " Where Id=" + Bills_Id;
                opp.insert_me(mysql);

                Msg = Msg + "\n\r";
                Msg = Msg + "  تم إصدار صرف النقدية  ";
            }
            else
            {
                // نفس حارس المسار الأمامى أعلاه.
                Excp = 1;
                Msg = Msg + "\n\r";
                Msg = Msg + "لم يتم العثور على صندوق نقدية افتراضى لتسجيل سداد هذا المرتجع";
            }

        }
        public decimal Get_PaymentInvoice(long invoice_List_Table_Id)
        {
            decimal PaymentValue = 0;
            string mysql = "   SELECT SUM(Import_Value) - SUM(Export_Value) AS PaymentValue FROM Money_Transaction_Box WHERE (Invoice_Id = " + invoice_List_Table_Id + ")   ";

            DataTable dt = opp.return_DataTable(mysql);
            if (dt.Rows.Count > 0)
            {
                if (dt.Rows[0]["PaymentValue"] != DBNull.Value)
                {
                    PaymentValue = decimal.TryParse(dt.Rows[0]["PaymentValue"].ToString(), out PaymentValue) ? PaymentValue : 0;
                }
            }
            if (PaymentValue < 0)
            {
                PaymentValue = 0;
            }
            PaymentValue = Common.RoundMe(PaymentValue);
            return PaymentValue;

            //mysql = @" SELECT   '' as Serial,     USER_DETAILES.USER_NAME, USER_DETAILES.Name AS FullName, Customer.Id AS Customer_Id, Customer.Name, Invoices_List.* ";
            //mysql = mysql + @" , ISNULL((SELECT SUM(Import_Value) - SUM(Export_Value) AS Expr1 FROM Money_Transaction_Box WHERE (Invoice_Id = Invoices_List.Id)), 0) AS PaymentValue ";
            //string StillValues = @"    Invoices_List.Net_Total - ISNULL((SELECT SUM(Import_Value) - SUM(Export_Value) AS Expr1 FROM Money_Transaction_Box AS Money_Transaction_Box_1 WHERE (Invoice_Id = Invoices_List.Id)), 0)   ";

            //mysql = mysql + @" , IIF (" + StillValues + " > 0 , " + StillValues + ", 0 ) As StillValues ";

            //mysql = mysql + @" FROM Invoices_List INNER JOIN
            //             Customer ON Invoices_List.Customer_Id = Customer.Id INNER JOIN
            //             USER_DETAILES ON Invoices_List.User_Id = USER_DETAILES.Id ";

            //mysql = mysql + " WHERE (Invoices_List.Id =" + Invoice_List_Table_Id + ") ";
        }
    }


}
