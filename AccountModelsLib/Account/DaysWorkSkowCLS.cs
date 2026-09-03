using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
  using AccountModelsLib; using AccountModelsLib.Models;
using System.Collections;

namespace AccountModelsLib
{
    public class DaysWorkSkowCLS
    {
        DataSet ds;
        operations opp = new operations();
        DateTime dtFrom;
        DateTime dtTo;
        public DaysWorkSkowCLS(DateTime dtFrom_)
        {
            ds = new DataSet();
            dtFrom = dtFrom_;
        }
        public DaysWorkSkowCLS(DateTime dtFrom_, DateTime dtTo_)
        {
            ds = new DataSet();
            dtFrom = dtFrom_;
            dtTo = dtTo_;
        }
        public int User_Id = 0;
        public DataSet GetExport_Items()
        {

            GetInvoices_List();
            GetExportMoney_Transaction();
            GetExport_Check_Money();
            return ds;
        }
        public DataSet GetImport_Items()
        {

            GetBills_List();
            GetImportMoney_Transaction();
            GetImport_Check_Money();
            return ds;
        }


        #region Export Items

        public DataTable GetUsers_List()
        {

            string mysql = "SELECT  * FROM USER_DETAILES ";

            DataTable dt = opp.return_DataTable(mysql);
            return dt;
        }
        public DataSet GetInvoices_List()
        {
            // 

            string mysql = " select Invoices_List.* ,Customer.Name AS Customer_Name ";
            mysql = mysql + " FROM Invoices_List INNER JOIN  Customer ON Invoices_List.Customer_Id = Customer.Id ";

            mysql = mysql + " where    CONVERT(date, Invoices_List.Invoice_Date)   = '" + operations.dateFormat(dtFrom) + "'  ";
            if (User_Id > 0)
            {
                mysql = mysql + " AND Invoices_List.User_Id=" + User_Id + "";
            }
            mysql = mysql + " Order By Invoices_List.Id ";
            ds = opp.return_dataset(mysql, "Invoices_List");
            return ds;


        }
        public DataSet GetInvoices_List(DateTime dtFrom,DateTime dtTo)
        {
            // 

            string mysql = " select Invoices_List.* ,Customer.Name AS Customer_Name ";
            mysql = mysql + " FROM Invoices_List INNER JOIN  Customer ON Invoices_List.Customer_Id = Customer.Id ";

            mysql = mysql + " where  Invoice_Date > = @dtFrom   And  Invoice_Date <= @dtTo ";
            if (User_Id > 0)
            {
                mysql = mysql + " AND Invoices_List.User_Id=@User_Id";
            }
            mysql = mysql + " Order By Invoices_List.Id ";
            
            opp.cmd.Parameters.AddWithValue("@dtFrom", dtFrom);
            opp.cmd.Parameters.AddWithValue("@dtTo", dtTo);
            opp.cmd.Parameters.AddWithValue("@User_Id", User_Id);

            ds = opp.return_dataset(mysql, "Invoices_List");
            int C = ds.Tables[0].Rows.Count;
            opp.cmd.Parameters.Clear();
            return ds;


        }
        public DataSet GetInvoices_Reverse_List(DateTime dtFrom, DateTime dtTo)
        {
            // 

            string mysql = " select Invoices_List_Reverse.* ,Customer.Name AS Customer_Name ";
            mysql = mysql + " FROM Invoices_List_Reverse INNER JOIN  Customer ON Invoices_List_Reverse.Customer_Id = Customer.Id ";

            mysql = mysql + " where  Invoice_Date  > = @dtFrom ";// And  Invoice_Date <= @dtTo ";
            if (User_Id > 0)
            {
                mysql = mysql + " AND Invoices_List_Reverse.User_Id=@User_Id";
            }
            mysql = mysql + " Order By Invoices_List_Reverse.Id ";

            opp.cmd.Parameters.AddWithValue("@dtFrom", dtFrom);
            opp.cmd.Parameters.AddWithValue("@dtTo", dtTo);
            opp.cmd.Parameters.AddWithValue("@User_Id", User_Id);

            ds = opp.return_dataset(mysql, "Invoices_List_Reverse");
            opp.cmd.Parameters.Clear();

            return ds;


        }
        public DataSet GetWork_Out_Details()
        {
            // 

            string mysql = "  SELECT DISTINCT Supplier.Name, Work_Out_Details.Supplier_Id ";
            mysql = mysql + " FROM Work_Out_Details INNER JOIN Supplier ON Work_Out_Details.Supplier_Id = Supplier.Id ";

            mysql = mysql + " where  Work_Out_Details.Request_Date = '" + operations.dateFormat(dtFrom) + "'  ";
            ds = opp.return_dataset(mysql, "Work_Out_Details");
            return ds;


        }



        public DataSet GetExportMoney_Transaction()
        {

            return Get_Money_Transaction(2);

        }
        public DataSet GetExport_Check_Money()
        {
            return Get_Check_Money(2);

        }

        #endregion


        #region Import Items


        //public DataSet GetBills_List()
        //{

        //    string mysql = "SELECT Supplier.Name AS Supplier_Name, Bills_List.* ";
        //    mysql = mysql + " FROM Bills_List INNER JOIN Supplier ON Bills_List.Supplier_Id = Supplier.Id ";
        //      mysql =mysql + " where Invoice_Date = '" + operations.dateFormat(dtFrom) + "'  ";
        //      mysql = mysql + " Order By Bills_List.Id ";
        //      ds = opp.return_dataset(mysql, "Bills_List");
        //    return ds;
        //}

        public DataSet GetBills_List()
        {

            string mysql = "SELECT Supplier.Name AS Supplier_Name, Bills_List.* ";
            mysql = mysql + " FROM Bills_List INNER JOIN Supplier ON Bills_List.Supplier_Id = Supplier.Id ";
            mysql = mysql + " where CONVERT(date, Bills_List.Invoice_Date) = '" + operations.dateFormat(dtFrom) + "'  ";
            if (User_Id > 0)
            {
                mysql = mysql + " AND Bills_List.User_Id=" + User_Id + "";
            }

            mysql = mysql + " Order By Bills_List.Id ";
            ds = opp.return_dataset(mysql, "Bills_List");
            return ds;
        }
        public DataSet Get_NazalatTotal()
        {
            string mysql = @" SELECT        SUM(Total_of_Nazlla.total) AS total, Total_of_Nazlla.Supplier_ID, Supplier.Name AS Supplier_Name, Total_of_Nazlla.date
FROM            Total_of_Nazlla INNER JOIN
                         Supplier ON Total_of_Nazlla.Supplier_ID = Supplier.Id ";

            mysql = mysql + " Where   Total_of_Nazlla.date = '" + operations.dateFormat(dtFrom) + "'  ";
            mysql = mysql + " GROUP BY Total_of_Nazlla.Supplier_ID, Supplier.Name, Total_of_Nazlla.date ";
            ds = opp.return_dataset(mysql);
            return ds;

        }
        public DataSet GetInvoices_Reverse_List()
        {
            // 

            string mysql = " select Invoices_List_Reverse.* ,Customer.Name AS Customer_Name ";
            mysql = mysql + " FROM Invoices_List_Reverse INNER JOIN  Customer ON Invoices_List_Reverse.Customer_Id = Customer.Id ";

            mysql = mysql + " where  Invoice_Date = '" + operations.dateFormat(dtFrom) + "'  ";
            if (User_Id > 0)
            {
                mysql = mysql + " AND Invoices_List_Reverse.User_Id=" + User_Id + "";
            }
            mysql = mysql + " Order By Invoices_List_Reverse.Id ";
            ds = opp.return_dataset(mysql, "Invoices_List_Reverse");
            return ds;


        }
        public DataSet GetBills_List_Reverse()
        {

            string mysql = "SELECT Supplier.Name AS Supplier_Name, Bills_List_Reverse.* ";
            mysql = mysql + " FROM Bills_List_Reverse INNER JOIN Supplier ON Bills_List_Reverse.Supplier_Id = Supplier.Id ";
            mysql = mysql + " where Invoice_Date = '" + operations.dateFormat(dtFrom) + "'  ";
            if (User_Id > 0)
            {
                mysql = mysql + " AND Bills_List_Reverse.User_Id=" + User_Id + "";
            }
            mysql = mysql + " Order By Bills_List_Reverse.Id ";
            ds = opp.return_dataset(mysql, "Bills_List_Reverse");
            return ds;
        }


        public DataSet GetWork_Out_Details_Received()
        {
            string mysql = " SELECT SUM(Work_Out_Details_Received.Total_Price) AS Total_Price, Supplier.Name AS Supplier_Name, Work_Out_Details_Received.WorkDate,  Work_Out_Details_Received.Process_No, Work_Out_Details_Received.Supplier_Id ";
            mysql = mysql + " FROM Work_Out_Details_Received INNER JOIN  Supplier ON Work_Out_Details_Received.Supplier_Id = Supplier.Id ";
            mysql = mysql + " WHERE (Work_Out_Details_Received.WorkDate =  '" + operations.dateFormat(dtFrom) + "' ) ";
            mysql = mysql + " GROUP BY Supplier.Name, Work_Out_Details_Received.WorkDate , Work_Out_Details_Received.Supplier_Id , Work_Out_Details_Received.Process_No";
            ds = opp.return_dataset(mysql, "Work_Out_Details_Received");
            return ds;
        }
        public DataSet GetImportMoney_Transaction()
        {
            return Get_Money_Transaction(1);

        }

        public DataSet GetImport_Check_Money()
        {
            return Get_Check_Money(1);

        }

        #endregion



        private DataSet Get_Money_Transaction(int Transaction_Type)
        {

            //Submain_Account_1.SubAccount_Name AS MainAccount_Name,

            string mysql = @" SELECT        Submain_Account.SubAccount_Name, Money_Transaction_Box.Id, Money_Transaction_Box.Order_Mony_Id, Money_Transaction_Box.Transaction_Date, Money_Transaction_Box.Transaction_Type, 
                         Money_Transaction_Box.Import_Value, Money_Transaction_Box.Export_Value, Money_Transaction_Box.Transaction_Value, Money_Transaction_Box.Transaction_Value_, Money_Transaction_Box.Bank_Id, 
                         Money_Transaction_Box.Bank_Balance, Money_Transaction_Box.Supplier_Id, Money_Transaction_Box.Customer_Id, Money_Transaction_Box.Account_Id, Money_Transaction_Box.Notes, 
                         Money_Transaction_Box.Balance_Before, Money_Transaction_Box.Balance_After, Money_Transaction_Box.DestructionSales, Money_Transaction_Box.BankAccount_Id, Money_Transaction_Box.Order_mony_id_transform, 
                         Order_Money.Man_Name, Order_Money.Invoice_Id, Submain_Account.Head_Main_Account
 FROM            Money_Transaction_Box INNER JOIN
                         Order_Money ON Money_Transaction_Box.Order_Mony_Id = Order_Money.Id INNER JOIN
                         Submain_Account ON Money_Transaction_Box.SubAccount_Id = Submain_Account.Id ";


 

          

            mysql = mysql + "  WHERE (CONVERT(date, Money_Transaction_Box.Transaction_Date)   = '" + operations.dateFormat(dtFrom) + "') AND (Money_Transaction_Box.Transaction_Type = " + Transaction_Type + ") ";
            if (User_Id > 0)
            {
                mysql = mysql + "   AND  (Order_Money.User_Id=" + User_Id + ")";
            }

            ds = opp.return_dataset(mysql, "Money_Transaction_Box");
            return ds;

        }


        private DataSet Get_Check_Money(int Transaction_Type)
        {

            string mysql = @" SELECT        Submain_Account.SubAccount_Name, Checks_Memory.id, Checks_Memory.Order_Mony_Id, Checks_Memory.Check_No, Checks_Memory.Day_Date, Checks_Memory.Due_Date, Checks_Memory.Bank_id, 
                         Checks_Memory.Check_Value, Checks_Memory.Check_Value_, Checks_Memory.Import_Value, Checks_Memory.Export_Value, Checks_Memory.Due_Check, Checks_Memory.Value_Receipt_Date, Checks_Memory.Receipt, 
                         Checks_Memory.Notes, Checks_Memory.Transaction_Type, Checks_Memory.Exchanger_Name, Checks_Memory.First_Drawer_Name, Checks_Memory.Bank_Balance, Checks_Memory.Last_Drawer_Name, 
                         Checks_Memory.Balance_Before, Checks_Memory.Balance_After, Checks_Memory.Bank_Name, Checks_Memory.SpentTo_Bank_Id, Checks_Memory.SpentTo_Bank_Type, Checks_Memory.Spent_Date, 
                         Checks_Memory.Bank_Id_Under_Ta7seel, Checks_Memory.Transfer_Check_Id, Checks_Memory.Balance_BeginingPeriod, Checks_Memory.Check_Notes, Checks_Memory.OperationDate, Checks_Memory.User_Id, 
                         Checks_Memory.Branch_Id, Checks_Memory.Insert_ServerDateTime, Checks_Memory.Update_ServerDateTime, Checks_Memory.EditOperationDate, Checks_Memory.Edit_User_Id, Checks_Memory.Customer_Id, 
                         Checks_Memory.Supplier_Id, Checks_Memory.Account_Id, Checks_Memory.SubAccount_Id, Checks_Memory.SubAccount_NO, Checks_Memory.SubAccount_Name AS Expr1, Checks_Memory.MainSubAccount_ID, 
                         Checks_Memory.IS_Posted_Flage, Checks_Memory.Currency_Id, Checks_Memory.Factor, Checks_Memory.SubAccount_Name_RelatedTo, Checks_Memory.Account_Entry_List_ID, Checks_Memory.SubAccount_Id_RelatedTo, 
                         Checks_Memory.SubAccount_Name_RelatedToBefor, Checks_Memory.SubAccount_Id_RelatedToBefor, Checks_Memory.ProcessNo, Checks_Memory.IsAllowEdit, Checks_Memory.IsAllowDelete, 
                         Checks_Memory.SubAccount_Name_RelatedFrom, Checks_Memory.SubAccount_Id_RelatedFrom, Order_Money.Order_No, Order_Money.Total_Value, Order_Money.Currency_Id AS Expr2, Order_Money.Bank_Type, 
                         Order_Money.Man_Name, Order_Money.Order_Date, Order_Money.Man_Name AS Expr3
FROM            Checks_Memory INNER JOIN
                         Submain_Account ON Checks_Memory.Account_Id = Submain_Account.Id INNER JOIN
                         Order_Money ON Checks_Memory.Order_Mony_Id = Order_Money.Id ";
            mysql = mysql + "  WHERE (CONVERT(date, Checks_Memory.Day_Date )= '" + operations.dateFormat(dtFrom) + "') AND (Checks_Memory.Transaction_Type = " + Transaction_Type + ") ";
            if (User_Id > 0)
            {
                mysql = mysql + "   AND  (Order_Money.User_Id=" + User_Id + ")";
            }

            ds = opp.return_dataset(mysql, "Checks_Memory");
            return ds;

        }


        /// ///////////////////////////////////////////////////////




        public DataTable Get_TotalReport(decimal Fixed_Asset_Value, decimal Balance_Variant, decimal Credet_Variant, decimal LostCustomerBalance, decimal PrevTotalBalance,decimal FirstPeriod_Check_Total)
        {

            #region Table
            DataTable dt = new DataTable();
            DataColumn Serial_ = new DataColumn("Serial", Type.GetType("System.String"));
            dt.Columns.Add(Serial_);

            DataColumn Bill_Total_ = new DataColumn("Bill_Total", Type.GetType("System.String"));
            dt.Columns.Add(Bill_Total_);

            DataColumn Bill_Revers_Total_ = new DataColumn("Bill_Revers_Total", Type.GetType("System.String"));
            dt.Columns.Add(Bill_Revers_Total_);

            DataColumn Invoice_Total_ = new DataColumn("Invoice_Total", Type.GetType("System.String"));
            dt.Columns.Add(Invoice_Total_);

            DataColumn Invoice_Reverse_Total_ = new DataColumn("Invoice_Reverse_Total", Type.GetType("System.String"));
            dt.Columns.Add(Invoice_Reverse_Total_);


            DataColumn FirstPeriod_Money_Total_ = new DataColumn("FirstPeriod_Money_Total", Type.GetType("System.String"));
            dt.Columns.Add(FirstPeriod_Money_Total_);


            DataColumn Import_Money_Total_ = new DataColumn("Import_Money_Total", Type.GetType("System.String"));
            dt.Columns.Add(Import_Money_Total_);

            DataColumn Export_Money_Total_ = new DataColumn("Export_Money_Total", Type.GetType("System.String"));
            dt.Columns.Add(Export_Money_Total_);

            DataColumn Money_BalanceNow_ = new DataColumn("Money_BalanceNow", Type.GetType("System.String"));
            dt.Columns.Add(Money_BalanceNow_);


            DataColumn FirstPeriod_Check_Total_ = new DataColumn("FirstPeriod_Check_Total", Type.GetType("System.String"));
            dt.Columns.Add(FirstPeriod_Check_Total_);

            DataColumn Import_Check_Total_ = new DataColumn("Import_Check_Total", Type.GetType("System.String"));
            dt.Columns.Add(Import_Check_Total_);

            DataColumn Export_Check_Total_ = new DataColumn("Export_Check_Total", Type.GetType("System.String"));
            dt.Columns.Add(Export_Check_Total_);

            DataColumn Check_BalanceNowNew_ = new DataColumn("Check_BalanceNowNew", Type.GetType("System.String"));
            dt.Columns.Add(Check_BalanceNowNew_);



            DataColumn Check_BalanceNow_ = new DataColumn("Check_BalanceNow", Type.GetType("System.String"));
            dt.Columns.Add(Check_BalanceNow_);

            DataColumn Check_Money_Now_ = new DataColumn("Check_Money_Now", Type.GetType("System.String"));
            dt.Columns.Add(Check_Money_Now_);



            DataColumn CusSupImportTotal_ = new DataColumn("CusSupImportTotal", Type.GetType("System.String"));
            dt.Columns.Add(CusSupImportTotal_);

            DataColumn CusSupExportTotal_ = new DataColumn("CusSupExportTotal", Type.GetType("System.String"));
            dt.Columns.Add(CusSupExportTotal_);


            DataColumn CusSupDiff_ = new DataColumn("CusSupDiff", Type.GetType("System.String"));
            dt.Columns.Add(CusSupDiff_);



            DataColumn Fixed_Asset_Value_ = new DataColumn("Fixed_Asset_Value", Type.GetType("System.String"));
            dt.Columns.Add(Fixed_Asset_Value_);

            DataColumn Balance_Variant_ = new DataColumn("Balance_Variant", Type.GetType("System.String"));
            dt.Columns.Add(Balance_Variant_);


            DataColumn Credet_Variant_ = new DataColumn("Credet_Variant", Type.GetType("System.String"));
            dt.Columns.Add(Credet_Variant_);

            DataColumn Final_Balance_As_Bill_ = new DataColumn("Final_Balance_As_Bill", Type.GetType("System.String"));
            dt.Columns.Add(Final_Balance_As_Bill_);

            DataColumn Final_Balance_As_Invoice_ = new DataColumn("Final_Balance_As_Invoice", Type.GetType("System.String"));
            dt.Columns.Add(Final_Balance_As_Invoice_);

            DataColumn NetFinal_Balance_As_Bill_ = new DataColumn("NetFinal_Balance_As_Bill", Type.GetType("System.String"));
            dt.Columns.Add(NetFinal_Balance_As_Bill_);

            DataColumn NetFinal_Balance_As_Invoice_ = new DataColumn("NetFinal_Balance_As_Invoice", Type.GetType("System.String"));
            dt.Columns.Add(NetFinal_Balance_As_Invoice_);


            DataColumn LostCustomerBalance_ = new DataColumn("LostCustomerBalance", Type.GetType("System.String"));
            dt.Columns.Add(LostCustomerBalance_);

            DataColumn PrevTotalBalance_ = new DataColumn("PrevTotalBalance", Type.GetType("System.String"));
            dt.Columns.Add(PrevTotalBalance_);

            #endregion

            //string mysql = "Create Table AAAAAATT (";
            //for (int x = 0; x < dt.Columns.Count; x++)
            //{
            //    string C = dt.Columns[x].ColumnName.ToString();
            //    mysql = mysql + C + " decimal(18, 2) ,";
            //}
            //mysql = mysql + ")";


            decimal Bill_Total = 0;// Get_Bills_List_Total();
            decimal Bill_Revers_Total = 0;// Get_Bills_List_Reverse_Total();
            decimal Invoice_Total = 0;// Get_Invoices_List_Total();
            decimal Invoice_Reverse_Total = 0;// Get_Invoices_List_Reverse_Total();
            Get_ItemsValueAS_Sell_Purchase(ref Invoice_Total, ref Bill_Total);

            //decimal   FirstPeriod_Money_Total = 0;
            //  decimal Import_Money_Total = Get_Money_Transaction_Box(  1);
            //  decimal Export_Money_Total = Get_Money_Transaction_Box(2);
            //  decimal Money_BalanceNow = Import_Money_Total - Export_Money_Total;

            decimal FirstPeriod_Money_Total = 0;
            decimal Import_Money_Total = 0;
            decimal Export_Money_Total = 0;
            decimal Money_BalanceNow = 0;
            Get_Money_Transaction_Box(ref FirstPeriod_Money_Total, ref Export_Money_Total, ref Import_Money_Total, ref Money_BalanceNow);



            decimal Import_Check_Total = Get_Checks_Memory(1);
            decimal Export_Check_Total = Get_Checks_Memory(2);
            decimal Check_BalanceNow = Import_Check_Total - Export_Check_Total;

            //decimal FirstPeriod_Check_Total = Get_Checks_MemoryFromStartTo_dtFrom(1, dtFrom);
            decimal Check_BalanceNowNew = Get_Checks_MemoryOnWalletNow(1);// Get_Checks_MemoryFromStartTo_dtFrom(1, dtTo);

            //decimal Check_Money_Now = Money_BalanceNow + Check_BalanceNow;
            decimal Check_Money_Now = Money_BalanceNow + Check_BalanceNowNew;


           
            decimal CusSupImportTotal = 0;
            decimal CusSupExportTotal = 0;
            decimal CusSupDiff = 0;
            Customer_MokhtalatDataByGroupNew(ref CusSupImportTotal, ref CusSupExportTotal, ref CusSupDiff);
            //Credet_Variant LostCustomerBalance, decimal PrevTotalBalance
            decimal Final_Balance_As_Bill = Fixed_Asset_Value + Balance_Variant + Bill_Total + Check_Money_Now + CusSupDiff - Credet_Variant - LostCustomerBalance;
            decimal Final_Balance_As_Invoice = Fixed_Asset_Value + Balance_Variant + Invoice_Total + Check_Money_Now + CusSupDiff - Credet_Variant - LostCustomerBalance;

            decimal NetFinal_Balance_As_Bill = Final_Balance_As_Bill - PrevTotalBalance;
            decimal NetFinal_Balance_As_Invoice = Final_Balance_As_Invoice - PrevTotalBalance;


            DataRow dr = dt.NewRow();
            dr["Bill_Total"] = Bill_Total;
            dr["Invoice_Total"] = Invoice_Total;

            dr["FirstPeriod_Money_Total"] = FirstPeriod_Money_Total;
            dr["Import_Money_Total"] = Import_Money_Total;
            dr["Export_Money_Total"] = Export_Money_Total;
            dr["Money_BalanceNow"] = Money_BalanceNow;



            dr["FirstPeriod_Check_Total"] = FirstPeriod_Check_Total;
            dr["Check_BalanceNowNew"] = Check_BalanceNowNew;

            dr["Import_Check_Total"] = Import_Check_Total;
            dr["Export_Check_Total"] = Export_Check_Total;
            dr["Check_BalanceNow"] = Check_BalanceNow;
            dr["Check_Money_Now"] = Check_Money_Now;
            dr["CusSupImportTotal"] = CusSupImportTotal;
            dr["CusSupExportTotal"] = CusSupExportTotal;
            dr["CusSupDiff"] = CusSupDiff;
            dr["Final_Balance_As_Bill"] = Final_Balance_As_Bill;
            dr["Final_Balance_As_Invoice"] = Final_Balance_As_Invoice;
            dr["NetFinal_Balance_As_Bill"] = NetFinal_Balance_As_Bill;
            dr["NetFinal_Balance_As_Invoice"] = NetFinal_Balance_As_Invoice;

            dr["Fixed_Asset_Value"] = Fixed_Asset_Value;
            dr["Balance_Variant"] = Balance_Variant;
            dr["Credet_Variant"] = Credet_Variant;
            dr["LostCustomerBalance"] = LostCustomerBalance;
            dr["PrevTotalBalance"] = PrevTotalBalance;

            


            dt.Rows.Add(dr);
            return dt;
        }

        private decimal Get_Bills_List_Total()
        {

            decimal Bill_Total = 0;
            string mysql = @" SELECT  SUM(Net_Total) AS Net_Total  FROM Bills_List ";
            mysql = mysql + "  WHERE (Bills_List.Invoice_Date >= '" + operations.dateFormat(dtFrom) + "') AND (Bills_List.Invoice_Date <= '" + operations.dateFormat(dtTo) + "') ";

            DataTable dt = opp.return_DataTable(mysql);
            if (dt.Rows.Count > 0)
            {
                Bill_Total = decimal.Parse(dt.Rows[0]["Net_Total"].ToString());
            }
            Bill_Total = Common.RoundMe(Bill_Total );

            Bill_Total = Common.GetValuesAfterSetArabic(Bill_Total);
            return Bill_Total;

        }
        private decimal Get_Bills_List_Reverse_Total()
        {

            decimal Total = 0;
            string mysql = @" SELECT  SUM(Net_Total) AS Net_Total  FROM  Bills_List_Reverse ";
            mysql = mysql + "  WHERE ( Invoice_Date >= '" + operations.dateFormat(dtFrom) + "') AND (Invoice_Date <= '" + operations.dateFormat(dtTo) + "') ";

            DataTable dt = opp.return_DataTable(mysql);
            if (dt.Rows.Count > 0)
            {
                Total = decimal.Parse(dt.Rows[0]["Net_Total"].ToString());
            }


            Total = Common.GetValuesAfterSetArabic(Total);
            return Total;

        }
        private decimal Get_Invoices_List_Total()
        {

            decimal Total = 0;
            string mysql = @" SELECT   SUM(Net_Total) AS Net_Total  FROM    Invoices_List ";
            mysql = mysql + "  WHERE ( Invoice_Date >= '" + operations.dateFormat(dtFrom) + "') AND (Invoice_Date <= '" + operations.dateFormat(dtTo) + "') ";

            DataTable dt = opp.return_DataTable(mysql);
            if (dt.Rows.Count > 0)
            {
                Total = decimal.Parse(dt.Rows[0]["Net_Total"].ToString());
            }


            Total = Common.GetValuesAfterSetArabic(Total);
            return Total;

        }
        private decimal Get_Invoices_List_Reverse_Total()
        {

            decimal Total = 0;
            string mysql = @" SELECT Invoice_Date,  SUM(Net_Total) AS Net_Total  FROM      Invoices_List_Reverse ";
            mysql = mysql + "  WHERE ( Invoice_Date >= '" + operations.dateFormat(dtFrom) + "') AND (Invoice_Date <= '" + operations.dateFormat(dtTo) + "') ";

            DataTable dt = opp.return_DataTable(mysql);
            if (dt.Rows.Count > 0)
            {
                Total = decimal.Parse(dt.Rows[0]["Net_Total"].ToString());
            }


            Total = Common.GetValuesAfterSetArabic(Total);
            return Total;

        }




        decimal Get_Money_Transaction_Box(int Transaction_Type)
        {


            decimal Total = 0;
            string mysql = @" SELECT   SUM( Transaction_Value) AS Net_Total  FROM       Money_Transaction_Box ";
            mysql = mysql + "  WHERE ( Transaction_Date >= '" + operations.dateFormat(dtFrom) + "') AND (Transaction_Date <= '" + operations.dateFormat(dtTo) + "') ";
            mysql = mysql + " AND  (Transaction_Type = " + Transaction_Type + ") ";
            DataTable dt = opp.return_DataTable(mysql);
            if (dt.Rows.Count > 0)
            {
                Total = decimal.Parse(dt.Rows[0]["Net_Total"].ToString());
            }


            Total = Common.GetValuesAfterSetArabic(Total);
            return Total;
        }
        void Get_Money_Transaction_Box(ref decimal T_FirstBalance_All, ref decimal T_Export_Value_Total_All, ref decimal T_Import_Value_Total_All, ref decimal T_FinalBalance_All)
        {
           
            string msg = "";
            int Excp = 0;
            DataTable dt = new DataTable();
            int bank_Id = 1;

            DateTime DateFrom = dtFrom;
            DateTime DateTo = dtTo;

            string mysql = "Select * from Banks Where Is_Bank=0";
            DataTable dtt = opp.return_DataTable(mysql);

            for (int x = 0; x < dtt.Rows.Count; x++)
            {
                    Money_Transaction_BoxOrderReportCLS mt = new Money_Transaction_BoxOrderReportCLS();

                bank_Id = int.Parse(dtt.Rows[x]["Id"].ToString());
                mt.Get_Money_Box_Transaction_PeriodTotalPerDay(ref Excp, ref msg, ref dt, bank_Id, DateFrom, DateTo);

                //T_FirstBalance_All = T_FirstBalance_All + mt.T_FirstBalance_All;
                T_FirstBalance_All = T_FirstBalance_All + mt.FirstBalance_FirstValue;

                T_Export_Value_Total_All = T_Export_Value_Total_All + mt.T_Export_Value_Total_All;
                T_Import_Value_Total_All = T_Import_Value_Total_All + mt.T_Import_Value_Total_All;
                //T_FinalBalance_All = T_FinalBalance_All + mt.T_FinalBalance_All;
            }
            T_FinalBalance_All = T_FirstBalance_All + T_Import_Value_Total_All - T_Export_Value_Total_All;


        }
        decimal Get_Checks_MemoryOnWalletNow(int Transaction_Type)
        {
            decimal Total = 0;
            string mysql = @" SELECT   SUM( Check_Value) AS Net_Total  
                FROM            Checks_Memory INNER JOIN
                         Submain_Account ON Checks_Memory.Account_Id = Submain_Account.Id ";
            mysql = mysql + "  WHERE  Receipt =0 ";
            mysql = mysql + " AND  (Transaction_Type = " + Transaction_Type + ") ";
            DataTable dt = opp.return_DataTable(mysql);
            try
            {
                if (dt.Rows.Count > 0)
                {
                    Total = decimal.Parse(dt.Rows[0]["Net_Total"].ToString());
                }
            }
            catch { }

            Total = Common.GetValuesAfterSetArabic(Total);
            return Total;
        }
        decimal Get_Checks_Memory(int Transaction_Type)
        {
            decimal Total = 0;
            string mysql = @" SELECT   SUM( Check_Value) AS Net_Total  
                FROM            Checks_Memory INNER JOIN
                         Submain_Account ON Checks_Memory.Account_Id = Submain_Account.Id ";
            mysql = mysql + "  WHERE ( Day_Date >= '" + operations.dateFormat(dtFrom) + "') AND (Day_Date <= '" + operations.dateFormat(dtTo) + "') ";
            mysql = mysql + " AND  (Transaction_Type = " + Transaction_Type + ") ";
            DataTable dt = opp.return_DataTable(mysql);
            try
            {
                if (dt.Rows.Count > 0)
                {
                    Total = decimal.Parse(dt.Rows[0]["Net_Total"].ToString());
                }
            }
            catch { }

            Total = Common.GetValuesAfterSetArabic(Total);
            return Total;
        }

        decimal Get_Checks_MemoryFromStartTo_dtFrom(int Transaction_Type, DateTime dtFrom_)
        {
            decimal Total = 0;
            string mysql = @" SELECT   SUM( Check_Value) AS Net_Total  
                FROM            Checks_Memory INNER JOIN
                         Submain_Account ON Checks_Memory.Account_Id = Submain_Account.Id ";
            mysql = mysql + "  WHERE ( Day_Date <= '" + operations.dateFormat(dtFrom_) + "') ";
            mysql = mysql + " AND  (Transaction_Type = " + Transaction_Type + ") ";
            DataTable dt = opp.return_DataTable(mysql);
            try
            {
                if (dt.Rows.Count > 0)
                {
                    Total = decimal.Parse(dt.Rows[0]["Net_Total"].ToString());
                }
            }
            catch { }

            Total = Common.GetValuesAfterSetArabic(Total);
            return Total;
        }

        public void Customer_MokhtalatDataByGroupNew(ref decimal Import, ref decimal Export, ref decimal Diff)
        {
            DataTable dtAccount = new DataTable();
            //decimal ImportC = 0;
            //decimal ExportC = 0;

            //decimal ImportS = 0;
            //decimal ExportS = 0;

            //decimal ImportM = 0;
            //decimal ExportM = 0;



            //Get_CustomerBalance(ref Import, ref Export, ref Diff);

            Get_SupplierBalance(ref Import, ref Export, ref Diff);


            Get_MokhtalatCustomerSupplierBalance(ref Export, ref Import, ref Diff);
            Diff = Export - Import;

            //Import= Import1 + ,   Export

        }
        void Get_CustomerBalance(ref decimal Import, ref decimal Export, ref decimal Diff)
        {


            DataTable dtAccount = new DataTable();
            operations opp = new operations();
            CustomerCLS customerCLS=new  CustomerCLS();
          //  Connection4It.Customer_List1 cc = new Customer_List1();

            string mysql = customerCLS.Get_CustomerBalance_SQL(0, 0, 0, false);
            decimal Balance = 0;

            DataTable dt = opp.return_DataTable(mysql);

            for (int x = 0; x < dt.Rows.Count; x++)
            {
                string Name = dt.Rows[x]["Name"].ToString();
                Balance = decimal.Parse(dt.Rows[x]["Balance"].ToString());
                //decimal  MyImport = decimal.Parse(dt.Rows[0]["MyImport"].ToString());
                // decimal MyExport = decimal.Parse(dt.Rows[0]["MyExport"].ToString());
                // Balance = MyExport - MyImport;// decimal.Parse(dt.Rows[0]["Balance"].ToString());
                if (Balance > 0)
                {
                    Export = Export + Balance;
                }
                else
                {
                    Balance = Balance * -1;
                    Import = Import + Balance;
                }



            }

            Export = Common.GetValuesAfterSetArabic(Export);
            Import = Common.GetValuesAfterSetArabic(Import);
            Diff = Export - Import;
            Diff = Common.GetValuesAfterSetArabic(Diff);
        }
        void Get_SupplierBalance(ref decimal Import, ref decimal Export, ref decimal Diff)
        {


            DataTable dtAccount = new DataTable();
            operations opp = new operations();
            SupplierCLS  supplierCLS = new SupplierCLS();
           // Connection4It.Supplier_List ss = new Supplier_List();
            int CustomerBalanceStatus = -1;
            string mysql = supplierCLS.Get_Supplier_DataByGroupNew_SQL(0, 0, 0, false, CustomerBalanceStatus);
            decimal Balance = 0;

            DataTable dt = opp.return_DataTable(mysql);

            for (int x = 0; x < dt.Rows.Count; x++)
            {
                Balance = decimal.Parse(dt.Rows[x]["Balance"].ToString());
                if (Balance > 0)
                {
                    Import = Import + Balance;
                }
                else
                {
                    Balance = Balance * -1;
                    Export = Export + Balance;
                }



            }

            Export = Common.GetValuesAfterSetArabic(Export);
            Import = Common.GetValuesAfterSetArabic(Import);
            Diff = Export - Import;
            Diff = Common.GetValuesAfterSetArabic(Diff);
        }
        void Get_MokhtalatCustomerSupplierBalance(ref decimal Import, ref decimal Export, ref decimal Diff)
        {


            DataTable dtAccount = new DataTable();

            CustomerCLS ca = new CustomerCLS();
            //Connection4It.Customer_List1 cc = new Customer_List1();
            int Ref_Cust_Supplier_Id = -1;
            int CustomerBalanceStatus = -1;
            int General_Customer_Flage = -1;
            string mysql = ca.MakeCustomer_MokhtalatDataByGroupNew(General_Customer_Flage, 0, 0, Ref_Cust_Supplier_Id, CustomerBalanceStatus);

            decimal Balance = 0;
            operations opp = new operations();
            DataTable dt = opp.return_DataTable(mysql);

            for (int x = 0; x < dt.Rows.Count; x++)
            {
                Balance = decimal.Parse(dt.Rows[x]["Balance"].ToString());
                if (Balance > 0)
                {
                    Import = Import + Balance;
                }
                else
                {
                    Balance = Balance * -1;
                    Export = Export + Balance;
                }



            }

            Export = Common.GetValuesAfterSetArabic(Export);
            Import = Common.GetValuesAfterSetArabic(Import);
            Diff = Export - Import;
            Diff = Common.GetValuesAfterSetArabic(Diff);
        }
        public void Get_ItemsValueAS_Sell_Purchase(ref decimal Total_Sell_price, ref decimal Total_Purchase_Price)
        {
            DataTable dtAccount = new DataTable();
            operations opp = new operations();





            string mysql = @"SELECT        SUM(Unit_item.S_price * (Unit_item.D_Qua / Unit_item.Small_unit + Unit_item.Full_qua)) AS Total_Sell_price, SUM(Unit_item.P_price * (Unit_item.D_Qua / Unit_item.Small_unit + Unit_item.Full_qua)) 
                         AS Total_Purchase_Price
FROM            Unit_item INNER JOIN
                         Item ON Unit_item.Item_id = Item.id
WHERE        (Unit_item.active_flage = 1) AND (Item.Active_flage = 1)  ";



            DataTable dt = opp.return_DataTable(mysql);

            if (dt.Rows.Count > 0)
            {
                Total_Sell_price = decimal.Parse(dt.Rows[0]["Total_Sell_price"].ToString());
                Total_Purchase_Price = decimal.Parse(dt.Rows[0]["Total_Purchase_Price"].ToString());

            }

            Total_Sell_price = Common.GetValuesAfterSetArabic(Total_Sell_price);
            Total_Purchase_Price = Common.GetValuesAfterSetArabic(Total_Purchase_Price);


        }



    }
}
