using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using Microsoft.Data.SqlClient;
using System.Data.OleDb;
using AccountModelsLib.Models;
using System.Collections.Generic;
using System.Reflection;
 

namespace AccountModelsLib
{
    /// <summary>
    /// Summary description for InvoicesClss.
    /// </summary>
    public class InvoicesClss
    {
        public static Int64 New_Invoices_No;   // —ﬁ„ «·›« Ê—Â «·ÃœÌœ…
        public static Int64 Invoices_Id;
        public int Invoice_Type { get; set; }  ///Invoice_Type=0 if ﬁÿ«⁄ ⁄«„   or =1 if    ﬁÿ«⁄ Œ«’ 
		public InvoicesClss()
        {
            New_Invoices_No = 0;
            Invoice_Type = Common.Invoice_Type_Id;
            //
            // TODO: Add constructor logic here
            //
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Invoice_Types">///Invoice_Type=0 if ﬁÿ«⁄ ⁄«„   or =1 if    ﬁÿ«⁄ Œ«’ </param>
        public InvoicesClss(int Invoice_Types)
        {
            New_Invoices_No = 0;
            Invoice_Type = Invoice_Types;
            //
            // TODO: Add constructor logic here
            //
        }

        public long Make_New_Invoice_No(int Invoice_Type, DateTime Invoice_Date)
        {

            long Max_Invoice_Id = 0;
            string mysql = "SELECT MAX(Invoice_Id) AS Max_Invoice_Id FROM Invoices_List where Id>0 ";
            if (Common.Is_InvoiceNoBy_Invoice_Type == 1)
            {
                mysql = mysql + " And Invoice_Type=" + Invoice_Type + "";
            }
            if (Common.Is_InvoiceNoBy_Branch_Id == 1)
            {
                mysql = mysql + " And Branch_Id = " + Common.Branch_Id + "";
            }
            if (Common.Is_InvoiceNoBy_FinancialYear == 1)
            {

                mysql = mysql + "  year(Invoice_Date) =  " + Invoice_Date.Year + "";
            }

            operations opp = new operations();
            DataSet ds = opp.return_dataset(mysql, "Invoices_List");
            try
            {
                Max_Invoice_Id = long.Parse(ds.Tables["Invoices_List"].Rows[0]["Max_Invoice_Id"].ToString()) + 1;
            }
            catch
            {
                Max_Invoice_Id = 1;
            }
            opp = null;
            ds = null;
            return Max_Invoice_Id;
        }
        public long Make_New_Invoice_No(int Invoice_Type)
        {

            long Max_Invoice_Id = 0;
            string mysql = "SELECT MAX(Invoice_Id) AS Max_Invoice_Id FROM Invoices_List where Id>0 ";
            if (Common.Is_InvoiceNoBy_Invoice_Type == 1)
            {
                mysql = mysql + " And Invoice_Type=" + Invoice_Type + "";
            }
            if (Common.Is_InvoiceNoBy_Branch_Id == 1)
            {
                mysql = mysql + " And Branch_Id = " + Common.Branch_Id + "";
            }
            if (Common.Is_InvoiceNoBy_FinancialYear == 1)
            {

                mysql = mysql + "  year(Invoice_Date) =  " + DateTime.Now.Year + "";
            }

            operations opp = new operations();
            DataSet ds = opp.return_dataset(mysql, "Invoices_List");
            try
            {
                Max_Invoice_Id = 0;
                  long.TryParse(ds.Tables["Invoices_List"].Rows[0]["Max_Invoice_Id"].ToString(),out Max_Invoice_Id);
                Max_Invoice_Id = Max_Invoice_Id + 1;
            }
            catch
            {
                Max_Invoice_Id = 1;
            }
            opp = null;
            ds = null;
            return Max_Invoice_Id;
        }

        public long Make_New_Invoice_No_POS(int Invoice_Type, DateTime Invoice_Date)
        {
            string mysql = "SELECT MAX(Invoice_Id) AS Max_Invoice_Id FROM Invoices_List where Invoice_Type=" + Invoice_Type;
            DateTime dtStart = new DateTime(Invoice_Date.Year, Invoice_Date.Month, Invoice_Date.Day, 0, 0, 0);
            DateTime dtEnd = new DateTime(Invoice_Date.Year, Invoice_Date.Month, Invoice_Date.Day, 23, 59, 59);




            if (Common.UseInvoiceNoByDay_POS == 1)
            {

                //mysql = mysql + " And Invoice_Date >='" + operations.Get_DateTimeFormate(dtStart) + "'  And Invoice_Date <='" + operations.Get_DateTimeFormate(dtEnd) + "'";
                mysql = mysql + " AND(YEAR(Invoice_Date) = " + Invoice_Date.Year + ") AND(MONTH(Invoice_Date) = " + Invoice_Date.Month + ") AND(DAY(Invoice_Date) = " + Invoice_Date.Day + ") ";
            }

            long Max_Invoice_Id = 0;

            operations opp = new operations();
            DataSet ds = opp.return_dataset(mysql, "Invoices_List");
            try
            {
                Max_Invoice_Id = long.Parse(ds.Tables["Invoices_List"].Rows[0]["Max_Invoice_Id"].ToString()) + 1;
            }
            catch
            {
                Max_Invoice_Id = 1;
            }
            opp = null;
            ds = null;
            return Max_Invoice_Id;
        }

        public long Make_New_Invoice_ReverseNo(int Invoice_Type, DateTime Invoice_Date)
        {

            long Max_Invoice_Id = 0;
            string mysql = "SELECT MAX(Invoice_Id) AS Max_Invoice_Id FROM Invoices_List_Reverse where Id>0 ";
            if (Common.Is_InvoiceNoBy_Invoice_Type == 1)
            {
                mysql = mysql + " And Invoice_Type=" + Invoice_Type + "";
            }
            if (Common.Is_InvoiceNoBy_Branch_Id == 1)
            {
                mysql = mysql + " And Branch_Id = " + Common.Branch_Id + "";
            }
            if (Common.Is_InvoiceNoBy_FinancialYear == 1)
            {

                mysql = mysql + "  year(Invoice_Date) =  " + Invoice_Date.Year + "";
            }

            operations opp = new operations();
            DataSet ds = opp.return_dataset(mysql, "Invoices_List");
            try
            {
                Max_Invoice_Id = long.Parse(ds.Tables["Invoices_List"].Rows[0]["Max_Invoice_Id"].ToString()) + 1;
            }
            catch
            {
                Max_Invoice_Id = 1;
            }
            opp = null;
            ds = null;
            return Max_Invoice_Id;
        }

        public long Make_New_Invoice_ReverseNo(int Invoice_Type)
        {

            long Max_Invoice_Id = 0;
            string mysql = "SELECT MAX(Invoice_Id) AS Max_Invoice_Id FROM Invoices_List_Reverse where Invoice_Type=" + Invoice_Type;
            operations opp = new operations();
            DataSet ds = opp.return_dataset(mysql, "Invoices_List");
            try
            {
                Max_Invoice_Id = long.Parse(ds.Tables["Invoices_List"].Rows[0]["Max_Invoice_Id"].ToString()) + 1;
            }
            catch
            {
                Max_Invoice_Id = 1;
            }
            opp = null;
            ds = null;
            return Max_Invoice_Id;
        }


        /// <summary>
        /// Return True if New Invoice number found else return false
        /// </summary>
        /// <param name="Invoice_No">New_Invoices_No</param>
        /// <returns>bool </returns>
        public bool Check_InvoiceNo_Found(ref int Excp, int Invoice_Type, long Invoice_No)
        {
            try
            {
                string mysql = "SELECT Invoices_List.* FROM Invoices_List WHERE (Invoice_Id = " + Invoice_No + ")   ";

                if (Common.Is_InvoiceNoBy_Invoice_Type == 1)
                {
                    mysql = mysql + " And Invoice_Type=" + Invoice_Type + "";
                }
                if (Common.Is_InvoiceNoBy_Branch_Id == 1)
                {
                    mysql = mysql + " And Branch_Id = " + Common.Branch_Id + "";
                }
                if (Common.Is_InvoiceNoBy_FinancialYear == 1)
                {

                    mysql = mysql + "  year(Invoice_Date) =  " + DateTime.Now.Year + "";
                }
                operations opp = new operations();
                DataSet ds = opp.return_dataset(mysql, "Invoices_List");
                if (ds.Tables["Invoices_List"].Rows.Count > 0)
                {
                    Excp = 0;
                    return true;
                }
                else
                {
                    Excp = 0;
                    New_Invoices_No = Invoice_No;
                    return false;
                }
            }
            catch (Exception e)
            {
                Excp = 1;
                throw (e);
            }
        }
        public bool Check_InvoiceNo_Found(ref int Excp, long Invoice_ListId, int Invoice_Type, string Invoice_No, DateTime Invoice_Date)
        {
            try
            {
                string mysql = "SELECT Invoices_List.* FROM Invoices_List WHERE (Invoice_Id = " + Invoice_No + ")   ";
                if (Invoice_ListId > 0)
                {
                    mysql = mysql + " And Id=" + Invoice_ListId + "";
                }

                if (Common.Is_InvoiceNoBy_Invoice_Type == 1)
                {
                    mysql = mysql + " And Invoice_Type=" + Invoice_Type + "";
                }
                if (Common.Is_InvoiceNoBy_Branch_Id == 1)
                {
                    mysql = mysql + " And Branch_Id = " + Common.Branch_Id + "";
                }
                if (Common.Is_InvoiceNoBy_FinancialYear == 1)
                {

                    mysql = mysql + "  year(Invoice_Date) =  " + Invoice_Date.Year + "";
                }
                operations opp = new operations();
                DataSet ds = opp.return_dataset(mysql, "Invoices_List");
                if (ds.Tables["Invoices_List"].Rows.Count > 0)
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
            catch (Exception e)
            {
                Excp = 1;
                return true;
            }
        }
        public bool Check_InvoiceNo_Found(ref int Excp, long Invoice_No)
        {
            try
            {
                string mysql = "SELECT Invoices_List.* FROM Invoices_List WHERE (Invoice_Id = " + Invoice_No + ") ";
                operations opp = new operations();
                DataSet ds = opp.return_dataset(mysql, "Invoices_List");
                if (ds.Tables["Invoices_List"].Rows.Count > 0)
                {
                    Excp = 0;
                    return true;
                }
                else
                {
                    Excp = 0;
                    New_Invoices_No = Invoice_No;
                    return false;
                }
            }
            catch (Exception e)
            {
                Excp = 1;
                throw (e);
            }
        }
        public bool Check_InvoiceReverseNo_Found(ref int Excp, long Invoice_ListId, int Invoice_Type, string Invoice_No, DateTime Invoice_Date)
        {
            try
            {
                string mysql = "SELECT Invoices_List_Reverse.* FROM Invoices_List_Reverse WHERE (Invoice_Id = " + Invoice_No + ")   ";
                if (Invoice_ListId > 0)
                {
                    mysql = mysql + " And Id=" + Invoice_ListId + "";
                }

                if (Common.Is_InvoiceNoBy_Invoice_Type == 1)
                {
                    mysql = mysql + " And Invoice_Type=" + Invoice_Type + "";
                }
                if (Common.Is_InvoiceNoBy_Branch_Id == 1)
                {
                    mysql = mysql + " And Branch_Id = " + Common.Branch_Id + "";
                }
                if (Common.Is_InvoiceNoBy_FinancialYear == 1)
                {

                    mysql = mysql + "  year(Invoice_Date) =  " + Invoice_Date.Year + "";
                }
                operations opp = new operations();
                DataSet ds = opp.return_dataset(mysql, "Invoices_List");
                if (ds.Tables["Invoices_List"].Rows.Count > 0)
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
            catch (Exception e)
            {
                Excp = 1;
                return true;
            }
        }

        /// <summary>
        /// Return True if New Invoice number found else return false
        /// </summary>
        /// <param name="Invoice_No">New_Invoices_No</param>
        /// <returns>bool</returns>
       
        public void Make_New_Invoice_Id(ref int Excp, ref operations opp, ref long ID, long Invoice_Id, long Customer_Id, DateTime Invoice_Date, int Invoice_Type, decimal Total, int IsCommission_Value, decimal commission, decimal commissionValue, decimal TaxSales10, decimal ATS05, decimal TaxSales10_Value, decimal ATS05_Value, decimal Net_Total, string Currency, long Currency_Id, int Customer_BalanceSet, string Notes, string PolicaNo, string TroodContainerNo, decimal CustomerAccountBefor, decimal CustomerAccountAfter, long SubAccount_Id, string SubAccount_Name, decimal AnotherPayment, decimal Total_PriceWithoutTax, long Salesman_Id, string Customer_Notes, int Is_Invoice_Agile, long Emp_Id, string Emp_Name, int IS_Make_Sarf_Item_BalanceFromInvoice = 0, int Is_InvoiceKta3y = 0)
        {
            string mysql = " ";
            long ProcessNo = 0;
            if (ProcessNo == 0)
            {
                ProcessNo = Common.Make_AccountsProcessNo(ref Excp, ref opp, "Invoices_List");
                // ProcessNo,
                // "+ ProcessNo + ",
            }
            mysql = " INSERT INTO Invoices_List (ProcessNo,Invoice_Id, Customer_Id, Invoice_Date,Invoice_DateTime, Total,IsCommission_Value, commission,commissionValue, TaxSales10, ATS05,TaxSales10_Value,ATS05_Value , Net_Total , Currency,Currency_id,Invoice_Type,Customer_BalanceSet,Notes,PolicaNo, TroodContainerNo,CustomerAccountBefor, CustomerAccountAfter,SubAccount_Id,   SubAccount_Name,User_Id,Branch_Id,AnotherPayment,Salesman_Id,   IS_Make_Sarf_Item_BalanceFromInvoice,  Is_InvoiceKta3y,OperationDate,Total_PriceWithoutTax,Customer_Notes,Is_Invoice_Agile,Emp_Id,Emp_Name) ";
            mysql = mysql + " VALUES (" + ProcessNo + "," + Invoice_Id + "," + Customer_Id + ", '" + operations.dateFormat(Invoice_Date) + "' , " + operations.Get_DateTimeToInsert(Invoice_Date) + "," + Total + "," + IsCommission_Value + "," + commission + "," + commissionValue + "," + TaxSales10 + "," + ATS05 + "," + TaxSales10_Value + "," + ATS05_Value + "," + Net_Total + ",'" + Currency + "'," + Currency_Id + "," + Invoice_Type + "," + Customer_BalanceSet + ",'" + Notes + "','" + PolicaNo + "',  '" + TroodContainerNo + "'," + CustomerAccountBefor + "," + CustomerAccountAfter + "," + SubAccount_Id + ", '" + SubAccount_Name + "'," + Common.User_Id + "," + Common.Branch_Id + "," + AnotherPayment + "," + Salesman_Id + "," + IS_Make_Sarf_Item_BalanceFromInvoice + "," + Is_InvoiceKta3y + "  ," + operations.Get_DateTimeToInsert(DateTime.Now) + "," + Total_PriceWithoutTax + ",'" + Customer_Notes + "'," + Is_Invoice_Agile + "," + Emp_Id + ",'" + Emp_Name + "')";

            ID = opp.Insert_me_Return_Last_Identify(mysql, ref Excp);


        }
        public void Make_New_Invoice_Id_POS(ref int Excp, ref operations opp, ref long ID, long Invoice_Id, long Customer_Id, DateTime Invoice_Date, int Invoice_Type, decimal Total, int IsCommission_Value, decimal commission, decimal commissionValue, decimal TaxSales10, decimal ATS05, decimal TaxSales10_Value, decimal ATS05_Value, decimal Net_Total, string Currency, long Currency_Id, int Customer_BalanceSet, string Notes, string PolicaNo, string TroodContainerNo, decimal CustomerAccountBefor, decimal CustomerAccountAfter, long SubAccount_Id, string SubAccount_Name)
        {
            TaxSales10 = Common.RoundMe(TaxSales10);
            TaxSales10_Value = Common.RoundMe(TaxSales10_Value);
            ATS05 = Common.RoundMe(ATS05);
            ATS05_Value = Common.RoundMe(ATS05_Value);


            string Msg = "";
            Invoices_List invl = new Invoices_List();
            long ProcessNo =DataDictionaryProcessesNoCLS.GetProcessesNo(ref Excp,ref Msg, ref opp, invl);
            // ProcessNo,
            // "+ ProcessNo + ",

            string mysql = " ";

            mysql = " INSERT INTO Invoices_List (ProcessNo,Invoice_Id, Customer_Id, Total,IsCommission_Value,commissionValue, TaxSales10, ATS05,TaxSales10_Value,ATS05_Value , Net_Total , Currency,Currency_id,Invoice_Type,Customer_BalanceSet,Notes,PolicaNo, TroodContainerNo,CustomerAccountBefor, CustomerAccountAfter,SubAccount_Id,   SubAccount_Name,User_Id,Branch_Id  ,OperationDate) ";
            mysql = mysql + " VALUES (" + ProcessNo + ", " + Invoice_Id + "," + Customer_Id + "," + Total + "," + IsCommission_Value + "," + commissionValue + "," + TaxSales10 + "," + ATS05 + "," + TaxSales10_Value + "," + ATS05_Value + "," + Net_Total + ",'" + Currency + "'," + Currency_Id + "," + Invoice_Type + "," + Customer_BalanceSet + ",'" + Notes + "','" + PolicaNo + "',  '" + TroodContainerNo + "'," + CustomerAccountBefor + "," + CustomerAccountAfter + "," + SubAccount_Id + ", '" + SubAccount_Name + "'," + Common.User_Id + "," + Common.Branch_Id + " ," + operations.Get_DateTimeToInsert(DateTime.Now) + ")";
            //'" + operations.Get_DateTimeToInsert(Invoice_Date) + "'

            ID = opp.Insert_me_Return_Last_Identify(mysql, ref Excp);

            mysql = "Update Invoices_List Set Invoice_Date=@Invoice_Date,Invoice_DateTime=@Invoice_Date  Where Id=@ID";
            opp.cmd.Parameters.AddWithValue("@Invoice_Date", Invoice_Date);
            opp.cmd.Parameters.AddWithValue("@ID", ID);

            opp.insert_me(mysql);
            opp.cmd.Parameters.Clear();
        }
         public int CommissionISValue = 1;

          public void Make_New_Invoice_Reverse_Id_POS(ref int Excp, ref operations opp, ref long ID, long Invoice_Id, long Customer_Id, DateTime Invoice_Date, int Invoice_Type, decimal Total, int IsCommission_Value, decimal commission, decimal CommissionValue, decimal TaxSales10, decimal ATS05, decimal TaxSales10_Value, decimal ATS05_Value, decimal Net_Total, string Currency, long Currency_Id, int Customer_BalanceSet, string Notes, string PolicaNo, string TroodContainerNo, decimal CustomerAccountBefor, decimal CustomerAccountAfter)
        {


            // (Invoice_Id, Customer_Id, Invoice_Date, Total, commission, TaxSales10, ATS05, Net_Total, Currency_id, Currency, Invoice_Type, 
            //  Customer_BalanceSet)
            long ProcessNo = Common.Make_AccountsProcessNo(ref Excp, ref opp, "Invoices_List_Reverse");
            // ProcessNo,
            // "+ ProcessNo + ",
            string mysql = " INSERT INTO Invoices_List_Reverse (ProcessNo,Invoice_Id, Customer_Id,   Total,IsCommission_Value, commission,CommissionValue, TaxSales10, ATS05,TaxSales10_Value,ATS05_Value , Net_Total , Currency,Currency_id,Invoice_Type,Customer_BalanceSet,Notes,PolicaNo, TroodContainerNo,CustomerAccountBefor, CustomerAccountAfter,User_Id,Branch_Id) ";
            mysql = mysql + " VALUES (" + ProcessNo + "," + Invoice_Id + "," + Customer_Id + " ," + Total + "," + IsCommission_Value + "," + commission + "," + CommissionValue + "," + TaxSales10 + "," + ATS05 + "," + TaxSales10_Value + "," + ATS05_Value + "," + Net_Total + ",'" + Currency + "'," +  Currency_Id + "," + Invoice_Type + "," + Customer_BalanceSet + ",'" + Notes + "','" + PolicaNo + "',  '" + TroodContainerNo + "'," + CustomerAccountBefor + "," + CustomerAccountAfter + "," + Common.User_Id + "," + Common.Branch_Id + ")";

            ID = opp.Insert_me_Return_Last_Identify(mysql, ref Excp);

            mysql = "Update Invoices_List_Reverse Set Invoice_Date=@Invoice_Date,Invoice_DateTime=@Invoice_Date Where Id=@ID";
            opp.cmd.Parameters.AddWithValue("@Invoice_Date", Invoice_Date);
            opp.cmd.Parameters.AddWithValue("@ID", ID);

            opp.insert_me(mysql);
            opp.cmd.Parameters.Clear();


        }
 
         public int Is_Invoice_Agile = -1;
        public int Is_Net_Total_Paid = 0;
        public long Emp_Id = 0;
        public DataTable Get_Customers_Invoices(ref int Excp, ref operations opp, long Invoices_List)
        {
            decimal nuInvoiceNo = 0;
            long Customer_Id = 0;
            bool _DateFrom = false;
            DateTime DateFrom = DateTime.Now;
            bool _DateTo = false;
            DateTime DateTo = DateTime.Now;
            bool NewInvoice = false;
            int IsInvoiceTaxInfo = -1;
            return Get_Customers_Invoices(ref Excp, ref opp, Invoices_List, nuInvoiceNo, Customer_Id, _DateFrom, DateFrom, _DateTo, DateTo, NewInvoice, IsInvoiceTaxInfo);
        }
        public DataTable Get_Customers_Invoices(ref int Excp, long Invoices_List, decimal nuInvoiceNo, long Customer_Id, bool _DateFrom, DateTime DateFrom, bool _DateTo, DateTime DateTo, bool NewInvoice, int IsInvoiceTaxInfo, int General_Customer_Flage)
        {
            operations opp = new operations();

            return Get_Customers_Invoices(ref Excp, ref opp, Invoices_List, nuInvoiceNo, Customer_Id, _DateFrom, DateFrom, _DateTo, DateTo, General_Customer_Flage, NewInvoice, IsInvoiceTaxInfo);
        }
        
        DataTable Get_Customers_Invoices(ref int Excp, ref operations opp, long Invoices_List, decimal nuInvoiceNo, long Customer_Id, bool _DateFrom, DateTime DateFrom, bool _DateTo, DateTime DateTo, bool NewInvoice, int IsInvoiceTaxInfo)
        {
            int General_Customer_Flage = -1;
            return Get_Customers_Invoices(ref Excp, ref opp, Invoices_List, nuInvoiceNo, Customer_Id, _DateFrom, DateFrom, _DateTo, DateTo, General_Customer_Flage, NewInvoice, IsInvoiceTaxInfo);
        }
        DataTable Get_Customers_Invoices(ref int Excp, ref operations opp, long Invoices_List, decimal nuInvoiceNo, long Customer_Id, bool _DateFrom, DateTime DateFrom, bool _DateTo, DateTime DateTo, int General_Customer_Flage, bool NewInvoice, int IsInvoiceTaxInfo)
        {

            IsInvoiceTaxInfo = Common.PermissionToShowTaxInvoiceOrNotORAll();
            DataTable dt = new DataTable();
            try
            {
                string mysql = @" SELECT   '' as Serial,     USER_DETAILES.USER_NAME, USER_DETAILES.Name AS FullName, Customer.Id AS Customer_Id, Customer.Name, Invoices_List.* ";
                mysql = mysql + @" , ISNULL((SELECT SUM(Import_Value) - SUM(Export_Value) AS Expr1 FROM Money_Transaction_Box WHERE   (Invoice_TypeId = 1)  AND (Invoice_Id = Invoices_List.Id)), 0) AS PaymentValue ";
                //string   StillValues =   @" ,   Invoices_List.Net_Total - ISNULL((SELECT SUM(Import_Value) - SUM(Export_Value) AS Expr1 FROM Money_Transaction_Box AS Money_Transaction_Box_1 WHERE (Invoice_Id = Invoices_List.Id)), 0) AS StillValues ";
                string StillValues = @"    Invoices_List.Net_Total - ISNULL((SELECT SUM(Import_Value) - SUM(Export_Value) AS Expr1 FROM Money_Transaction_Box AS Money_Transaction_Box_1 WHERE  (Invoice_TypeId = 1)  AND (Invoice_Id = Invoices_List.Id)), 0)   ";

                mysql = mysql + @" , IIF (" + StillValues + " > 0 , " + StillValues + ", 0 ) As StillValues ";

                mysql = mysql + @" FROM Invoices_List INNER JOIN
                         Customer ON Invoices_List.Customer_Id = Customer.Id INNER JOIN
                         USER_DETAILES ON Invoices_List.User_Id = USER_DETAILES.USER_ID ";
                //mysql = mysql + " WHERE (Invoices_List.Invoice_Type = " + Invoice_Type + ") ";
                mysql = mysql + " WHERE (Invoices_List.Is_DeletedProcessed <> 1) ";
                //(Invoices_List.Id > 0  ) AND 
                if (Invoices_List > 0)
                {
                    mysql = mysql + "  AND (Invoices_List.Id = " + Invoices_List + ")";

                }
                else
                {
                    if (_DateFrom == true)
                    {
                        //mysql = mysql + " AND  (Invoices_List.Invoice_Date >='" + operations.dateFormat(DateFrom) + "')   ";
                        mysql = mysql + " AND (CONVERT(DATE, Invoices_List.Invoice_Date)  >= '" + operations.dateFormat(DateFrom.Date) + "') ";

                    }
                    if (_DateTo == true)
                    {
                        //mysql = mysql + "   AND (Invoices_List.Invoice_Date <='" + operations.dateFormat(DateTo) + "') ";
                        mysql = mysql + " AND (CONVERT(DATE, Invoices_List.Invoice_Date)  <= '" + operations.dateFormat(DateTo.Date) + "') ";

                    }

                    if (Customer_Id > 0)
                    {
                        mysql = mysql + "  AND (Customer.Id = " + Customer_Id + ")";
                    }
                    if (NewInvoice)
                    {
                        mysql = mysql + "  AND (Invoices_List.Customer_BalanceSet =0) ";
                    }
                    else
                    {
                        //mysql = mysql + "  AND (Invoices_List.Customer_BalanceSet = 1) ";
                    }
                    if (nuInvoiceNo > 0)
                    {
                        mysql = mysql + "  AND (Invoices_List.Invoice_Id=" + nuInvoiceNo + ") ";
                    }
                    if (IsInvoiceTaxInfo > -1)
                    {
                        mysql = mysql + "  AND (Invoices_List.IsInvoiceTaxInfo=" + IsInvoiceTaxInfo + ") ";
                    }

                    if (Is_Invoice_Agile > -1)
                    {
                        mysql = mysql + "  AND (Invoices_List.Is_Invoice_Agile=" + Is_Invoice_Agile + ") ";
                    }
                    if (Emp_Id > 0)
                    {
                        mysql = mysql + "  AND (Invoices_List.Emp_Id=" + Emp_Id + ") ";
                    }
                    if (Is_Net_Total_Paid > -1)
                    {
                        mysql = mysql + @" AND (Invoices_List.Net_Total -  ISNULL((SELECT        SUM(Import_Value) - SUM(Export_Value) AS Expr1 FROM Money_Transaction_Box AS Money_Transaction_Box_1 ";
                        //„œ›Ê⁄…
                        if (Is_Net_Total_Paid == 1)
                        {

                            mysql = mysql + @" WHERE(Invoice_Id = Invoices_List.Id)), 0)  = 0)";
                        }

                        // €Ì— „œ›Ê⁄…
                        if (Is_Net_Total_Paid == 0)
                        {

                            mysql = mysql + @" WHERE(Invoice_Id = Invoices_List.Id)), 0)  <> 0)";
                        }
                    }
                }
                if (General_Customer_Flage > -1)
                {

                    mysql = mysql + "  AND (Customer.General_Customer_Flage = " + General_Customer_Flage + ")";

                }

                mysql = mysql + " Order By Invoices_List.Invoice_Id ";

                dt = opp.return_DataTable(mysql);

                Excp = 0;
                return dt;
            }
            catch (Exception e)
            {
                Excp = 1;
                throw (e);
            }

        }
         
        public DataTable Get_Customers_InvoicesList(ref int Excp, long Invoices_List, decimal nuInvoiceNo, long Customer_Id, bool _DateFrom, DateTime DateFrom, bool _DateTo, DateTime DateTo, bool NewInvoice, int IsInvoiceTaxInfo, int General_Customer_Flage)
        {
            operations opp = new operations();

            return Get_Customers_InvoicesList(ref Excp, ref opp, Invoices_List, nuInvoiceNo, Customer_Id, _DateFrom, DateFrom, _DateTo, DateTo, General_Customer_Flage, NewInvoice, IsInvoiceTaxInfo);
        }
        DataTable Get_Customers_InvoicesList(ref int Excp, ref operations opp, long Invoices_List, decimal nuInvoiceNo, long Customer_Id, bool _DateFrom, DateTime DateFrom, bool _DateTo, DateTime DateTo, int General_Customer_Flage, bool NewInvoice, int IsInvoiceTaxInfo)
        {

            IsInvoiceTaxInfo = Common.PermissionToShowTaxInvoiceOrNotORAll();
            DataTable dt = new DataTable();
            try
            {
                string mysql = @" SELECT   '' as Serial,     USER_DETAILES.USER_NAME, USER_DETAILES.Name AS FullName, Customer.Id AS Customer_Id, Customer.Name, Invoices_List.* ";
                //mysql = mysql + @" , ISNULL((SELECT SUM(Import_Value) - SUM(Export_Value) AS Expr1 FROM Money_Transaction_Box WHERE   (Invoice_TypeId = 1)  AND (Invoice_Id = Invoices_List.Id)), 0) AS PaymentValue ";
                //string   StillValues =   @" ,   Invoices_List.Net_Total - ISNULL((SELECT SUM(Import_Value) - SUM(Export_Value) AS Expr1 FROM Money_Transaction_Box AS Money_Transaction_Box_1 WHERE (Invoice_Id = Invoices_List.Id)), 0) AS StillValues ";
                //string StillValues = @"    Invoices_List.Net_Total - ISNULL((SELECT SUM(Import_Value) - SUM(Export_Value) AS Expr1 FROM Money_Transaction_Box AS Money_Transaction_Box_1 WHERE  (Invoice_TypeId = 1)  AND (Invoice_Id = Invoices_List.Id)), 0)   ";

                //mysql = mysql + @" , IIF (" + StillValues + " > 0 , " + StillValues + ", 0 ) As StillValues ";

                mysql = mysql + @" FROM Invoices_List INNER JOIN
                         Customer ON Invoices_List.Customer_Id = Customer.Id INNER JOIN
                         USER_DETAILES ON Invoices_List.User_Id = USER_DETAILES.USER_ID ";
                //mysql = mysql + " WHERE (Invoices_List.Invoice_Type = " + Invoice_Type + ") ";
                mysql = mysql + " WHERE (Invoices_List.Is_DeletedProcessed <> 1) ";
                //(Invoices_List.Id > 0  ) AND 
                if (Invoices_List > 0)
                {
                    mysql = mysql + "  AND (Invoices_List.Id = " + Invoices_List + ")";

                }
                else
                {
                    if (_DateFrom == true)
                    {
                        //mysql = mysql + " AND  (Invoices_List.Invoice_Date >='" + operations.dateFormat(DateFrom) + "')   ";
                        mysql = mysql + " AND (CONVERT(DATE, Invoices_List.Invoice_Date)  >= '" + operations.dateFormat(DateFrom.Date) + "') ";

                    }
                    if (_DateTo == true)
                    {
                        //mysql = mysql + "   AND (Invoices_List.Invoice_Date <='" + operations.dateFormat(DateTo) + "') ";
                        mysql = mysql + " AND (CONVERT(DATE, Invoices_List.Invoice_Date)  <= '" + operations.dateFormat(DateTo.Date) + "') ";

                    }

                    if (Customer_Id > 0)
                    {
                        mysql = mysql + "  AND (Customer.Id = " + Customer_Id + ")";
                    }
                    if (NewInvoice)
                    {
                        mysql = mysql + "  AND (Invoices_List.Customer_BalanceSet =0) ";
                    }
                    else
                    {
                        //mysql = mysql + "  AND (Invoices_List.Customer_BalanceSet = 1) ";
                    }
                    if (nuInvoiceNo > 0)
                    {
                        mysql = mysql + "  AND (Invoices_List.Invoice_Id=" + nuInvoiceNo + ") ";
                    }
                    if (IsInvoiceTaxInfo > -1)
                    {
                        mysql = mysql + "  AND (Invoices_List.IsInvoiceTaxInfo=" + IsInvoiceTaxInfo + ") ";
                    }

                    if (Is_Invoice_Agile > -1)
                    {
                        mysql = mysql + "  AND (Invoices_List.Is_Invoice_Agile=" + Is_Invoice_Agile + ") ";
                    }
                    if (Emp_Id > 0)
                    {
                        mysql = mysql + "  AND (Invoices_List.Emp_Id=" + Emp_Id + ") ";
                    }
                    //if (Is_Net_Total_Paid > -1)
                    //{
                    //    mysql = mysql + @" AND (Invoices_List.Net_Total -  ISNULL((SELECT        SUM(Import_Value) - SUM(Export_Value) AS Expr1 FROM Money_Transaction_Box AS Money_Transaction_Box_1 ";
                    //    //„œ›Ê⁄…
                    //    if (Is_Net_Total_Paid == 1)
                    //    {

                    //        mysql = mysql + @" WHERE(Invoice_Id = Invoices_List.Id)), 0)  = 0)";
                    //    }

                    //    // €Ì— „œ›Ê⁄…
                    //    if (Is_Net_Total_Paid == 0)
                    //    {

                    //        mysql = mysql + @" WHERE(Invoice_Id = Invoices_List.Id)), 0)  <> 0)";
                    //    }
                    //}
                }
                if (General_Customer_Flage > -1)
                {

                    mysql = mysql + "  AND (Customer.General_Customer_Flage = " + General_Customer_Flage + ")";

                }

                mysql = mysql + " Order By Invoices_List.Invoice_Id ";

                dt = opp.return_DataTable(mysql);

                Excp = 0;
                return dt;
            }
            catch (Exception e)
            {
                Excp = 1;
                throw (e);
            }

        }
        
        public DataTable Get_Customers_InvoicesForTax(ref int Excp, long Invoices_List, decimal nuInvoiceNo, long Customer_Id, bool _DateFrom, DateTime DateFrom, bool _DateTo, DateTime DateTo, bool NewInvoice, int IsInvoiceTaxInfo)
        {
            operations opp = new operations();
            int General_Customer_Flage = -1;
            return Get_Customers_InvoicesForTax(ref Excp, ref opp, Invoices_List, nuInvoiceNo, Customer_Id, _DateFrom, DateFrom, _DateTo, DateTo, General_Customer_Flage, NewInvoice, IsInvoiceTaxInfo);
        }
        DataTable Get_Customers_InvoicesForTax(ref int Excp, ref operations opp, long Invoices_List, decimal nuInvoiceNo, long Customer_Id, bool _DateFrom, DateTime DateFrom, bool _DateTo, DateTime DateTo, int General_Customer_Flage, bool NewInvoice, int IsInvoiceTaxInfo)
        {


            DataTable dt = new DataTable();
            try
            {
                string mysql = @" SELECT   '' as Serial,     USER_DETAILES.USER_NAME, USER_DETAILES.Name AS FullName, Customer.Id AS Customer_Id, Customer.Name, Invoices_List.* ";
                //mysql = mysql + @" , ISNULL((SELECT SUM(Import_Value) - SUM(Export_Value) AS Expr1 FROM Money_Transaction_Box WHERE (Invoice_Id = Invoices_List.Id)), 0) AS PaymentValue ";
                //mysql = mysql + @" ,   Invoices_List.Net_Total -
                //              ISNULL((SELECT SUM(Import_Value) - SUM(Export_Value) AS Expr1 FROM Money_Transaction_Box AS Money_Transaction_Box_1 WHERE (Invoice_Id = Invoices_List.Id)), 0) AS StillValues ";
                mysql = mysql + @" FROM Invoices_List INNER JOIN
                         Customer ON Invoices_List.Customer_Id = Customer.Id INNER JOIN
                         USER_DETAILES ON Invoices_List.User_Id = USER_DETAILES.USER_ID ";

                mysql = mysql + " WHERE (Invoices_List.Id > 0  ) ";

                if (Invoices_List > 0)
                {
                    mysql = mysql + "  AND (Invoices_List.Id = " + Invoices_List + ")";

                }
                else
                {
                    if (_DateFrom == true)
                    {
                        //mysql = mysql + " AND  (Invoices_List.Invoice_Date >='" + operations.dateFormat(DateFrom) + "')   ";
                        mysql = mysql + " AND (CONVERT(DATE, Invoices_List.Invoice_Date)  >= '" + operations.dateFormat(DateFrom.Date) + "') ";

                    }
                    if (_DateTo == true)
                    {
                        //mysql = mysql + "   AND (Invoices_List.Invoice_Date <='" + operations.dateFormat(DateTo) + "') ";
                        mysql = mysql + " AND (CONVERT(DATE, Invoices_List.Invoice_Date)  <= '" + operations.dateFormat(DateTo.Date) + "') ";

                    }

                    if (Customer_Id > 0)
                    {
                        mysql = mysql + "  AND (Customer.Id = " + Customer_Id + ")";
                    }

                    if (nuInvoiceNo > 0)
                    {
                        mysql = mysql + "  AND (Invoices_List.Invoice_Id=" + nuInvoiceNo + ") ";
                    }
                    if (IsInvoiceTaxInfo > -1)
                    {
                        mysql = mysql + "  AND (Invoices_List.IsInvoiceTaxInfo=" + IsInvoiceTaxInfo + ") ";
                    }

                    if (Is_Invoice_Agile > -1)
                    {
                        mysql = mysql + "  AND (Invoices_List.Is_Invoice_Agile=" + Is_Invoice_Agile + ") ";
                    }
                    if (Emp_Id > 0)
                    {
                        mysql = mysql + "  AND (Invoices_List.Emp_Id=" + Emp_Id + ") ";
                    }

                }
                if (General_Customer_Flage > -1)
                {

                    mysql = mysql + "  AND (Customer.General_Customer_Flage = " + General_Customer_Flage + ")";

                }

                mysql = mysql + " Order By Invoices_List.Invoice_Date ";

                dt = opp.return_DataTable(mysql);

                Excp = 0;
                return dt;
            }
            catch (Exception e)
            {
                Excp = 1;
                throw (e);
            }

        }

        public DataTable Get_Customers_InvoicesForCar(ref int Excp, string SearchTxt, long Customer_Id, bool _DateFrom, DateTime DateFrom, bool _DateTo, DateTime DateTo, int General_Customer_Flage)
        {
            int IsInvoiceTaxInfo = Common.PermissionToShowTaxInvoiceOrNotORAll();
            operations opp = new operations();

            BanksClss bncls = new BanksClss();
            List<Bank> bnks = bncls.Get_AllBanks(ref opp, 0);


            DataTable dt = new DataTable();
            try
            {
                string mysql = @" SELECT  '' AS IsPayed,'' as Serial,     USER_DETAILES.USER_NAME, USER_DETAILES.Name AS FullName, Customer.Id AS Customer_Id, Customer.Name, Invoices_List.* ";
                foreach (var bnk in bnks)
                {
                    mysql = mysql + @" , ISNULL((SELECT SUM(Import_Value) - SUM(Export_Value) AS Expr1 FROM Money_Transaction_Box WHERE (Invoice_TypeId = 1) AND (Invoice_Id = Invoices_List.Id) AND  (Bank_Id = " + bnk.Id + ")), 0) AS " + bnk.Bank_Name_En + " ";

                }

                mysql = mysql + @" , ISNULL((SELECT SUM(Import_Value) - SUM(Export_Value) AS Expr1 FROM Money_Transaction_Box WHERE  (Invoice_TypeId = 1) AND (Invoice_Id = Invoices_List.Id)), 0) AS PaymentValue ";
                mysql = mysql + @" ,  ISNULL
                             ((SELECT        SUM(ISNULL(Purchase_Price, 0) * ISNULL(Qnty, 0)) AS Expr1
                                 FROM            Invoices_Details AS Invoices_Details1
                                 WHERE        (Invoice_Id = Invoices_List.ID)), 0) AS Total_P_Price_FromInvoice ";

                /////
                ///
                mysql = mysql + @" ,  ISNULL
                             ((SELECT        SUM(ISNULL(P_Price, 0) * ISNULL(Qnty, 0)) AS Expr1
FROM            CarsWorkOrdersItemProcessedDetails AS CarsWorkOrdersItemProcessedDetails_33
WHERE         (CarsWorkOrdersId = Invoices_List.CarsWorkOrdersId)), 0) AS Total_P_Price ";


                /////

                mysql = mysql + @" ,   Invoices_List.Net_Total - ISNULL((SELECT SUM(Import_Value) - SUM(Export_Value) AS Expr1 FROM Money_Transaction_Box AS Money_Transaction_Box_1 WHERE  (Invoice_TypeId = 1) AND (Invoice_Id = Invoices_List.Id)), 0) AS StillValues ";


                /////////////////////////////////////////
                //// ﬂ·›… «·Œ«„« 
                mysql = mysql + @" , ISNULL
                             ((SELECT        SUM(ISNULL(P_Price, 0) * ISNULL(Qnty, 0) ) AS Expr1
                                 FROM            CarsWorkOrdersItemProcessedDetails
                                 WHERE      (Is_AddToInvoice = 1) And    (Item_Category_Id = " + (long)ItemCategoryEnum.RawMaterials + ") AND (CarsWorkOrdersId = Invoices_List.CarsWorkOrdersId)), 0) AS P_Price_For_RawMaterial ";

                /////////////////////////////////////////
                //// ﬂ·›… ﬁÿ⁄ «·€Ì«—
                mysql = mysql + @" , ISNULL
                             ((SELECT        SUM(ISNULL(P_Price, 0) * ISNULL(Qnty, 0)) AS Expr1
                                 FROM            CarsWorkOrdersItemProcessedDetails
                                 WHERE      (Is_AddToInvoice = 1) And   (Item_Category_Id = " + (long)ItemCategoryEnum.SpareParts + ") AND (CarsWorkOrdersId = Invoices_List.CarsWorkOrdersId)), 0) AS P_Price_For_SpareParts ";

                /////////////////////////////////////////
                //// ﬂ·›… ﬁÿ⁄ «·“ÌÊ 
                mysql = mysql + @" , ISNULL
                             ((SELECT        SUM(ISNULL(P_Price, 0) * ISNULL(Qnty, 0)) AS Expr1
                                 FROM            CarsWorkOrdersItemProcessedDetails
                                 WHERE     (Is_AddToInvoice = 1) And    (Item_Category_Id = " + (long)ItemCategoryEnum.Oils + ") AND (CarsWorkOrdersId = Invoices_List.CarsWorkOrdersId)), 0) AS P_Price_For_Oils ";

                // ﬂ·›… „’‰⁄Ì«   


                string P_Price_For_Masnaheaiat = @" ISNULL
                             ((SELECT        SUM(ISNULL(P_Price, 0) * ISNULL(Qnty, 0)) AS Expr1
                                 FROM            CarsWorkOrdersItemProcessedDetails
                                 WHERE     (Is_AddToInvoice = 1) And    (Item_Category_Id = " + (long)ItemCategoryEnum.Masnaheaiat + ") AND (CarsWorkOrdersId = Invoices_List.CarsWorkOrdersId)), 0)   ";

                mysql = mysql + @" , " + P_Price_For_Masnaheaiat + " AS P_Price_For_Masnaheaiat ";
                ////////////////////////
                ///Õ”«» «·—»ÕÌ… ·«„— «·‘€·
                // //profitability ﬁÿ⁄ «·€Ì«—

                mysql = mysql + @"";

                string profitability_For_SpareParts = @"  ISNULL(Invoices_List.Total_ListPriceWithoutTax_SpareParts - ISNULL
                             ((SELECT        SUM(ISNULL(P_Price, 0) * ISNULL(Qnty, 0)) AS Expr1
                                 FROM            CarsWorkOrdersItemProcessedDetails AS CarsWorkOrdersItemProcessedDetails_1
                                 WHERE     (Is_AddToInvoice = 1) And     (Item_Category_Id =   " + (long)ItemCategoryEnum.SpareParts + " ) AND (CarsWorkOrdersId = Invoices_List.CarsWorkOrdersId)), 0), 0) ";
                mysql = mysql + " , " + profitability_For_SpareParts + " AS profitability_For_SpareParts ";

                //////profitability “ÌÊ   
                string profitability_For_Oils = @"    ISNULL(Invoices_List.Total_ListPriceWithoutTax_Oils - ISNULL
                             ((SELECT        SUM(ISNULL(P_Price, 0) * ISNULL(Qnty, 0)) AS Expr1
                                 FROM            CarsWorkOrdersItemProcessedDetails AS CarsWorkOrdersItemProcessedDetails_1
                WHERE (Is_AddToInvoice = 1) And  (Item_Category_Id = " + (long)ItemCategoryEnum.Oils + ") AND(CarsWorkOrdersId = Invoices_List.CarsWorkOrdersId)), 0), 0) ";
                mysql = mysql + " , " + profitability_For_Oils + " AS profitability_For_Oils ";

                ////////profitability «·„’‰⁄Ì«     Total_ListPriceWithoutTax_Workmanship
                string profitability_For_Masnaheaiat = @"    ISNULL(Invoices_List.Total_ListPriceWithoutTax_Workmanship - ISNULL
                             ((SELECT        SUM(ISNULL(P_Price, 0) * ISNULL(Qnty, 0)) AS Expr1
                                 FROM            CarsWorkOrdersItemProcessedDetails AS CarsWorkOrdersItemProcessedDetails_1
                                 WHERE      (Is_AddToInvoice = 1) And    (Item_Category_Id =   " + (long)ItemCategoryEnum.RawMaterials + " ) AND (CarsWorkOrdersId = Invoices_List.CarsWorkOrdersId)), 0) - " + P_Price_For_Masnaheaiat + ", 0)  ";
                mysql = mysql + " , " + profitability_For_Masnaheaiat + " AS profitability_For_Masnaheaiat ";


                string profitability_Final = " , (" + profitability_For_SpareParts + ") + (" + profitability_For_Oils + ") + ( " + profitability_For_Masnaheaiat + " ) ";
                mysql = mysql + profitability_Final + " AS profitability_Final ";
                ///////////////////////////////////////////




                /////////////////////////////////////////
                mysql = mysql + @"";


                mysql = mysql + @" FROM Invoices_List INNER JOIN
                         Customer ON Invoices_List.Customer_Id = Customer.Id INNER JOIN
                         USER_DETAILES ON Invoices_List.User_Id = USER_DETAILES.USER_ID ";
                //mysql = mysql + " WHERE (Invoices_List.Invoice_Type = " + Invoice_Type + ") ";
                mysql = mysql + " WHERE (Invoices_List.Id > 0  ) ";
                mysql = mysql + " AND (Invoices_List.Is_DeletedProcessed = 0 ) ";



                if (_DateFrom == true)
                {
                    //mysql = mysql + " AND  (Invoices_List.Invoice_Date >='" + operations.dateFormat(DateFrom) + "')   ";
                    mysql = mysql + " AND (CONVERT(DATE, Invoices_List.Invoice_Date)  >= '" + operations.dateFormat(DateFrom.Date) + "') ";

                }
                if (_DateTo == true)
                {
                    //mysql = mysql + "   AND (Invoices_List.Invoice_Date <='" + operations.dateFormat(DateTo) + "') ";
                    mysql = mysql + " AND (CONVERT(DATE, Invoices_List.Invoice_Date)  <= '" + operations.dateFormat(DateTo.Date) + "') ";

                }

                if (Customer_Id > 0)
                {
                    mysql = mysql + "  AND (Customer.Id = " + Customer_Id + ")";
                }
                //if (NewInvoice)
                //{
                //    mysql = mysql + "  AND (Invoices_List.Customer_BalanceSet =0) ";
                //}
                //else
                //{
                //    //mysql = mysql + "  AND (Invoices_List.Customer_BalanceSet = 1) ";
                //}
                if (IsInvoiceTaxInfo > -1)
                {
                    mysql = mysql + "  AND (Invoices_List.IsInvoiceTaxInfo=" + IsInvoiceTaxInfo + ") ";
                }
                if (Is_Invoice_Agile > -1)
                {
                    mysql = mysql + "  AND (Invoices_List.Is_Invoice_Agile=" + Is_Invoice_Agile + ") ";
                }
                if (Emp_Id > 0)
                {
                    mysql = mysql + "  AND (Invoices_List.Emp_Id=" + Emp_Id + ") ";
                }
                if (Is_Net_Total_Paid > -1)
                {
                    mysql = mysql + @" AND (Invoices_List.Net_Total -  ISNULL((SELECT        SUM(Import_Value) - SUM(Export_Value) AS Expr1 FROM Money_Transaction_Box AS Money_Transaction_Box_1 ";
                    //„œ›Ê⁄…
                    if (Is_Net_Total_Paid == 1)
                    {

                        mysql = mysql + @" WHERE(Invoice_Id = Invoices_List.Id)), 0)  = 0)";
                    }

                    // €Ì— „œ›Ê⁄…
                    if (Is_Net_Total_Paid == 0)
                    {

                        mysql = mysql + @" WHERE(Invoice_Id = Invoices_List.Id)), 0)  <> 0)";
                    }
                }

                if (General_Customer_Flage > -1)
                {

                    mysql = mysql + "  AND (Customer.General_Customer_Flage = " + General_Customer_Flage + ")";

                }


                Invoices_List inv = new Invoices_List();
                if (SearchTxt.Trim() != "")
                {
                    long N = 0;
                    bool IsParse = long.TryParse(SearchTxt, out N);
                    if (IsParse == true)
                    {

                        mysql = mysql + "  AND ((Invoices_List.Invoice_Id=" + SearchTxt + ") ";
                        mysql = mysql + "  OR (Invoices_List.TaxInternalId='" + SearchTxt + "') ";
                        mysql = mysql + "  OR (Invoices_List.WorkOrderNumber=" + SearchTxt + ") ";
                        mysql = mysql + "  OR (Invoices_List.CarNumber Like'%" + SearchTxt + "%') )";
                    }
                    else
                    {


                        mysql = mysql + "  And  ((Invoices_List.TaxInternalId='" + SearchTxt + "') ";

                        mysql = mysql + "  OR (Invoices_List.CarNumber Like'%" + SearchTxt + "%') )";
                    }


                }
                //mysql = mysql + " Order By Invoices_List.Invoice_Id ";
                //mysql = mysql + " And "+ profitability_For_Oils + " < 0";

                mysql = mysql + " ORDER BY Invoices_List.Invoice_Date, Invoices_List.CarsWorkOrdersId ,Invoices_List.Invoice_Id ";

                dt = opp.return_DataTable(mysql);

                Excp = 0;
                return dt;
            }
            catch (Exception e)
            {
                Excp = 1;
                throw (e);
            }

        }
        public DataTable Get_Customers_Invoices_Reverse(ref int Excp, decimal nuInvoiceNo, long Customer_Id, bool _DateFrom, DateTime DateFrom, bool _DateTo, DateTime DateTo, bool NewInvoice)
        {
            DataTable dt = new DataTable();
            try
            {
                string mysql = @" SELECT   '' as Serial,     USER_DETAILES.USER_NAME, USER_DETAILES.Name AS FullName, Customer.Id AS Customer_Id, Customer.Name, Invoices_List_Reverse.*
FROM            Invoices_List_Reverse INNER JOIN
                         Customer ON Invoices_List_Reverse.Customer_Id = Customer.Id INNER JOIN
                         USER_DETAILES ON Invoices_List_Reverse.User_Id = USER_DETAILES.USER_ID ";


                //mysql = mysql + " WHERE (Invoices_List_Reverse.Invoice_Type = " + Invoice_Type + ") ";
                mysql = mysql + " WHERE (Invoices_List_Reverse.Id > 0) ";
                 
                if (_DateFrom == true)
                {
                    //mysql = mysql + " AND  (Invoices_List.Invoice_Date >='" + operations.dateFormat(DateFrom) + "')   ";
                    mysql = mysql + " AND (CONVERT(DATE, Invoices_List_Reverse.Invoice_Date)  >= '" + operations.dateFormat(DateFrom.Date) + "') ";

                }
                if (_DateTo == true)
                {
                    //mysql = mysql + "   AND (Invoices_List.Invoice_Date <='" + operations.dateFormat(DateTo) + "') ";
                    mysql = mysql + " AND (CONVERT(DATE, Invoices_List_Reverse.Invoice_Date)  <= '" + operations.dateFormat(DateTo.Date) + "') ";

                }
                if (Customer_Id > 0)
                {
                    mysql = mysql + "  AND (Customer.Id = " + Customer_Id + ")";
                }
                if (NewInvoice)
                {
                    mysql = mysql + "  AND (Invoices_List_Reverse.Customer_BalanceSet =0) ";
                }
                else
                {
                    //mysql = mysql + "  AND (Invoices_List_Reverse.Customer_BalanceSet = 1) ";
                }
                if (nuInvoiceNo > 0)
                {
                    mysql = mysql + "  AND (Invoices_List_Reverse.Invoice_Id=" + nuInvoiceNo + ") ";
                }
                operations opp = new operations();
                dt = opp.return_DataTable(mysql);

                Excp = 0;
                return dt;
            }
            catch (Exception e)
            {
                Excp = 1;
                throw (e);
            }

        }
        public DataTable Get_Customers_InvoicesCountForCar(ref int Excp, string SearchTxt, long Customer_Id, bool _DateFrom, DateTime DateFrom, bool _DateTo, DateTime DateTo, int General_Customer_Flage)
        {

            int IsInvoiceTaxInfo = Common.PermissionToShowTaxInvoiceOrNotORAll();
            operations opp = new operations();

            BanksClss bncls = new BanksClss();
            List<Bank> bnks = bncls.Get_AllBanks(ref opp, 0);


            DataTable dt = new DataTable();
            try
            {
                string mysql = @" SELECT        '' AS General_Customer_FlageStr, COUNT(Invoices_List.Customer_Id) AS Counts, Customer.General_Customer_Flage, SUM(Invoices_List.Net_Total) AS Net_Total, SUM(Invoices_List.Total) AS Total ";


                mysql = mysql + @" FROM Invoices_List INNER JOIN
                         Customer ON Invoices_List.Customer_Id = Customer.Id   ";
                //mysql = mysql + " WHERE (Invoices_List.Invoice_Type = " + Invoice_Type + ") ";
                mysql = mysql + " WHERE (Invoices_List.Id > 0  ) ";



                if (_DateFrom == true)
                {
                    //mysql = mysql + " AND  (Invoices_List.Invoice_Date >='" + operations.dateFormat(DateFrom) + "')   ";
                    mysql = mysql + " AND (CONVERT(DATE, Invoices_List.Invoice_Date)  >= '" + operations.dateFormat(DateFrom.Date) + "') ";

                }
                if (_DateTo == true)
                {
                    //mysql = mysql + "   AND (Invoices_List.Invoice_Date <='" + operations.dateFormat(DateTo) + "') ";
                    mysql = mysql + " AND (CONVERT(DATE, Invoices_List.Invoice_Date)  <= '" + operations.dateFormat(DateTo.Date) + "') ";

                }
                if (IsInvoiceTaxInfo > -1)
                {
                    mysql = mysql + "  AND (Invoices_List.IsInvoiceTaxInfo=" + IsInvoiceTaxInfo + ") ";
                }

                if (General_Customer_Flage > -1)
                {

                    mysql = mysql + "  AND (Customer.General_Customer_Flage = " + General_Customer_Flage + ")";

                }

                mysql = mysql + "   GROUP BY Customer.General_Customer_Flage ";
                dt = opp.return_DataTable(mysql);

                Excp = 0;
                return dt;
            }
            catch (Exception e)
            {
                Excp = 1;
                throw (e);
            }

        }
        public DataTable Get_Customers_InvoicesForDistributeMoney(ref int Excp, long Invoices_List, decimal nuInvoiceNo, long Customer_Id, bool _DateFrom, DateTime DateFrom, bool _DateTo, DateTime DateTo, long Money_Transaction_BoxId, bool NewInvoice, int IsInvoiceTaxInfo, bool IsOnlySearch)
        {
            operations opp = new operations();
            IsInvoiceTaxInfo = Common.PermissionToShowTaxInvoiceOrNotORAll();
            DataTable dt = new DataTable();
            try
            {
                //Money_Transaction_BoxForInvoices
                string mysql = @" SELECT  Distinct '' as Serial,     USER_DETAILES.USER_NAME, USER_DETAILES.Name AS FullName,Invoices_List.Invoice_Id, Customer.Id AS Customer_Id, Customer.Name, Invoices_List.* ";
                if (IsOnlySearch == false)
                {
                    mysql = mysql + @" ,  ( Select  ISNULL( Money_Transaction_BoxForInvoices.Transaction_Value,0) From Money_Transaction_BoxForInvoices Where  (Invoices_ListId = Invoices_List.Id) AND (Money_Transaction_BoxForInvoices.Money_Transaction_BoxId =" + Money_Transaction_BoxId + "))  AS ToPaymentValue    ";

                    mysql = mysql + @" ,  ( Select  ISNULL( Money_Transaction_BoxForInvoices.Id  ,0) From Money_Transaction_BoxForInvoices Where  (Invoices_ListId = Invoices_List.Id) AND (Money_Transaction_BoxForInvoices.Money_Transaction_BoxId =" + Money_Transaction_BoxId + "))  AS Money_Transaction_BoxForInvoicesId    ";
                    mysql = mysql + @" ,  ( Select  ISNULL( Money_Transaction_BoxForInvoices.Money_Transaction_BoxId  ,0) From Money_Transaction_BoxForInvoices Where  (Invoices_ListId = Invoices_List.Id) AND (Money_Transaction_BoxForInvoices.Money_Transaction_BoxId =" + Money_Transaction_BoxId + "))  AS Money_Transaction_BoxId    ";
                    mysql = mysql + @" ,  ( Select  ISNULL( Money_Transaction_BoxForInvoices.Transaction_Value  ,0) From Money_Transaction_BoxForInvoices Where  (Invoices_ListId = Invoices_List.Id) AND (Money_Transaction_BoxForInvoices.Money_Transaction_BoxId =" + Money_Transaction_BoxId + "))  AS Transaction_Value    ";
                }
                mysql = mysql + @" , ISNULL((SELECT ISNULL(SUM(Import_Value), 0) - ISNULL(SUM(Export_Value), 0) AS Expr1 FROM Money_Transaction_BoxForInvoices WHERE (Invoices_ListId = Invoices_List.Id)), 0) AS PaymentValue ";

                string StillValues = @"    Invoices_List.Net_Total - ISNULL((SELECT ISNULL(SUM(Import_Value), 0)   - ISNULL(SUM(Export_Value), 0) AS Expr1 FROM Money_Transaction_BoxForInvoices AS Money_Transaction_BoxForInvoices_1 WHERE (Invoices_ListId = Invoices_List.Id)), 0)   ";

                mysql = mysql + @" , IIF (" + StillValues + " > 0 , " + StillValues + ", 0 ) As StillValues ";

                mysql = mysql + @" FROM            Invoices_List INNER JOIN
                         Customer ON Invoices_List.Customer_Id = Customer.Id INNER JOIN
                         USER_DETAILES ON Invoices_List.User_Id = USER_DETAILES.USER_ID ";
                mysql = mysql + @" LEFT OUTER JOIN Money_Transaction_BoxForInvoices ON Invoices_List.ID = Money_Transaction_BoxForInvoices.Invoices_ListId ";
                mysql = mysql + " WHERE   (Money_Transaction_BoxForInvoices.Money_Transaction_BoxId = " + Money_Transaction_BoxId + ") ";
                mysql = mysql + " OR (Invoices_List.Is_DeletedProcessed <> 1) ";

                if (Invoices_List > 0)
                {
                    mysql = mysql + "  AND (Invoices_List.Id = " + Invoices_List + ")";

                }
                else
                {
                    if (_DateFrom == true)
                    {
                        //mysql = mysql + " AND  (Invoices_List.Invoice_Date >='" + operations.dateFormat(DateFrom) + "')   ";
                        mysql = mysql + " AND (CONVERT(DATE, Invoices_List.Invoice_Date)  >= '" + operations.dateFormat(DateFrom.Date) + "') ";

                    }
                    if (_DateTo == true)
                    {
                        //mysql = mysql + "   AND (Invoices_List.Invoice_Date <='" + operations.dateFormat(DateTo) + "') ";
                        mysql = mysql + " AND (CONVERT(DATE, Invoices_List.Invoice_Date)  <= '" + operations.dateFormat(DateTo.Date) + "') ";

                    }

                    if (Customer_Id > 0)
                    {
                        mysql = mysql + "  AND (Customer.Id = " + Customer_Id + ")";
                    }
                    if (NewInvoice)
                    {
                        mysql = mysql + "  AND (Invoices_List.Customer_BalanceSet =0) ";
                    }
                    else
                    {
                        //mysql = mysql + "  AND (Invoices_List.Customer_BalanceSet = 1) ";
                    }
                    if (nuInvoiceNo > 0)
                    {
                        mysql = mysql + "  AND (Invoices_List.Invoice_Id=" + nuInvoiceNo + ") ";
                    }
                    if (IsInvoiceTaxInfo > -1)
                    {
                        mysql = mysql + "  AND (Invoices_List.IsInvoiceTaxInfo=" + IsInvoiceTaxInfo + ") ";
                    }

                    if (Is_Invoice_Agile > -1)
                    {
                        mysql = mysql + "  AND (Invoices_List.Is_Invoice_Agile=" + Is_Invoice_Agile + ") ";
                    }
                    if (Emp_Id > 0)
                    {
                        mysql = mysql + "  AND (Invoices_List.Emp_Id=" + Emp_Id + ") ";
                    }
                    if (Is_Net_Total_Paid > -1)
                    {
                        mysql = mysql + @" AND (Invoices_List.Net_Total -  ISNULL((SELECT       ISNULL(SUM(Import_Value), 0) - ISNULL(SUM(Export_Value), 0) AS Expr1 FROM Money_Transaction_BoxForInvoices AS Money_Transaction_BoxForInvoices_1 ";
                        //„œ›Ê⁄…
                        if (Is_Net_Total_Paid == 1)
                        {

                            mysql = mysql + @" WHERE(Invoices_ListId = Invoices_List.Id)), 0)  = 0)";
                        }

                        // €Ì— „œ›Ê⁄…
                        if (Is_Net_Total_Paid == 0)
                        {

                            mysql = mysql + @" WHERE(Invoices_ListId = Invoices_List.Id)), 0)  <> 0)";
                        }
                    }
                }
                //if (General_Customer_Flage > -1)
                //{

                //    mysql = mysql + "  AND (Customer.General_Customer_Flage = " + General_Customer_Flage + ")";

                //}

                mysql = mysql + " Order By Invoices_List.Invoice_Id ";
                //ToPaymentValue  Desc,
                dt = opp.return_DataTable(mysql);

                Excp = 0;
                return dt;
            }
            catch (Exception e)
            {
                Excp = 1;
                throw (e);
            }

        }
        
        public DataSet Get_Items_In_invoice(ref int Excp, long Invoice_Id)
        {
            try
            {
                string mysql = " SELECT Unit_item.Code, Unit_item.id AS Unit_item_id, Unit_item.Reserved_Balance_Larg_Qnty, Unit_item.Reserved_Balance_Small_Qnty, Item.Item_Name,  Item.S_id AS Store_id, unit.u_name, unit.B_Unit AS Larg_Unit, unit.S_unit AS Small_unit, Unit_item.Upon_Request, Unit_item.unit_id,  Unit_item.Full_qua AS Larg_Balance, Unit_item.Specification, Unit_item.D_Qua AS Small_Balance, Unit_item.Big_unit AS Larg_unit_Value,  Unit_item.Small_unit AS Small_unit_Value, Unit_item.P_price AS Purchas_Price, Unit_item.S_price AS Sale_price, Unit_item.Item_id,  Invoices_Details.Id AS Invoices_Details_Id, Invoices_Details.Larg_Qnty, Invoices_Details.Small_Qnty, Invoices_Details.Qnty_Another,  Invoices_Details.Larg_Unit_Price, Invoices_Details.Small_Unit_Price, Invoices_Details.Unit_Used_for_Price, Invoices_Details.Total,  Invoices_Details.ATS05, Invoices_Details.TaxSales10, Invoices_Details.Notes, Invoices_Details.Qnty_value, Invoices_Details.Make_Order,  Invoices_List.Customer_Id, Invoices_List.License_Id, Invoices_List.Invoice_Date, Invoices_List.Total AS Invoices_Total, Invoices_List.commission,  Invoices_List.Net_Total, Invoices_List.Currency_id, Store.Store_Name, Store.id AS Store_id ";
                mysql = mysql + " FROM Unit_item INNER JOIN  unit ON Unit_item.unit_id = unit.id INNER JOIN  Item ON Unit_item.Item_id = Item.id INNER JOIN  Invoices_Details ON Unit_item.Item_id = Invoices_Details.Item_Id AND Unit_item.unit_id = Invoices_Details.Unit_Id AND   Unit_item.id = Invoices_Details.Unit_Item_Table_Id INNER JOIN  Invoices_List ON Invoices_Details.Invoice_Id = Invoices_List.ID INNER JOIN  Store ON Item.S_id = Store.id ";
                mysql = mysql + " WHERE (Invoices_Details.Invoice_Id = " + Invoice_Id + ")";
                operations opp = new operations();

                return opp.return_dataset(mysql);
                opp = null;
                Excp = 0;

            }
            catch (Exception e)
            {
                Excp = 1;
                throw (e);
            }

        }
        public DataSet Get_Stores_Of_Items_In_invoice(ref int Excp, ArrayList Arr)
        {
            try
            {
                string mysql = " SELECT id, Store_Name FROM Store WHERE (id IN ( ";

                foreach (int id in Arr)
                {
                    mysql = mysql + id + ",";
                }
                mysql = mysql + " 0)) ";

                operations opp = new operations();

                return opp.return_dataset(mysql);
                opp = null;
                Excp = 0;

            }
            catch (Exception e)
            {
                Excp = 1;
                throw (e);
            }
        }
       

        /// <summary>
        /// Delete Item from Invoice
        /// </summary>
        /// <param name="Excp">Excption id</param>
        /// <param name="Item_Id">Item_Id</param>
      }
    public struct Invoice_Items_Data_Struct
    {
        public decimal gLarg_Balance;
        public decimal Small_Balance;
        public decimal Larg_Qnty;
        public decimal Small_Qnty;
        public long Unit_Item_Table_Id;
        public int Invoices_Details_Id;
        public int Bills_Details_Id;
        public int Unit_id;
        public int Item_id;
    }
}
