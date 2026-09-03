using AccountModelsLib.Models;
 
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using Microsoft.Data.SqlClient;
using System.Drawing;
using System.Reflection;
 



namespace AccountModelsLib
{
    /// <summary>
    /// Summary description for CustomersAccount.
    /// </summary>
    public class CustomersAccountCLS
    {
        ArrayList arr;
        DataSet G_DS;
        public long ProcessNo = 0;
        public CustomersAccountCLS()
        {
            //
            // TODO: Add constructor logic here
            //
            arr = new ArrayList();
            G_DS = new DataSet();
        }

        public void Add_Invoice_Total_To_Account(ref int Excp, long Invoice_id, int Customer_Id, decimal Invoice_Net_Total, decimal Invoice_Total)
        {
            /*try
			{	
				string mysql="";
				  
			 arr.Clear();
				mysql="UPDATE Unit_item SET Reserved_Balance_Larg_Qnty = Reserved_Balance_Larg_Qnty - "+Diff_Larg_Qnty+" , Reserved_Balance_Small_Qnty = Reserved_Balance_Small_Qnty - "+Diff_Small_Qnty+"  WHERE (Full_qua = "+New_Larg_Balance_+") AND (D_Qua = "+New_Small_Balance_+")   AND (Reserved_Balance_Larg_Qnty="+Reserved_Balance_Larg_Qnty+") AND (Reserved_Balance_Small_Qnty="+Reserved_Balance_Small_Qnty+") AND  (id = "+Unit_Item_Table_Id+")";
				arr.Add(mysql);
				

				mysql=" UPDATE Invoices_Details SET Larg_Qnty ="+Larg_Qnty+", Small_Qnty ="+Small_Qnty+", Larg_Unit_Price ="+Larg_Unit_Price+", Small_Unit_Price ="+Small_Unit_Price+", Total ="+New_Total_Value+", ATS05 ="+ATS05+", TaxSales10 ="+TaxSales10+", Notes ='"+Notes+"', Qnty_Another ="+Qnty_Another+" ";
				mysql=mysql + " WHERE (Id ="+Invoices_DetailsTable_Id+") ";
				arr.Add(mysql);
				mysql="UPDATE Invoices_List SET Total =Total + "+Diff_Totals+" , Net_Total =Total + "+Diff_Totals+" WHERE ( Id = "+Invoices_list_table_Id+")";
				arr.Add(mysql);
				
				operations opp=new operations();
				opp.insert_me_transaction(ref Excp,1,arr);
				 
				opp=null;
				 
			}
			catch(Exception e)
			{
				Excp=1;
				throw(e);
			}
			*/
        }
        /*
        public void Set_Customer_Balance(ref int Excp, int Customer_Id, long New_Invoice_ListTable_id, decimal Net_Total)
        {

            try
            {
                operations opp = new operations();
                Net_Total = Common.RoundMe(Net_Total);
                string mysql = "";
                ArrayList arr = new ArrayList();
                arr.Clear();
                mysql = "  UPDATE Customer SET Balance = Balance + " + Net_Total + " ,Balance_Actual=Balance_Actual +  " + Net_Total + " WHERE (Id = " + Customer_Id + ")";
                arr.Add(mysql);

                UpdateProcessNo(ref Excp, ref opp, "Invoices_List", New_Invoice_ListTable_id);

                mysql = " INSERT INTO Customers_Accounts (ProcessNo,Customer_Id, Reference_Id,Reference_Type_Table_Id, Export, Balance, Transaction_Type, Transaction_Date,Reference_Type) ";
                mysql = mysql + " SELECT Invoices_List.ProcessNo,Invoices_List.Customer_Id, Invoices_List.Invoice_Id,Invoices_List.id, Invoices_List.Net_Total, Customer.Balance, 2 AS Transaction_Type, Invoices_List.Invoice_Date ,1 as Reference_Type";
                mysql = mysql + " FROM Customer INNER JOIN  Invoices_List ON Customer.Id = Invoices_List.Customer_Id ";
                mysql = mysql + " WHERE (Customer.Id = " + Customer_Id + ") AND (Invoices_List.id = " + New_Invoice_ListTable_id + ") ";
                arr.Add(mysql);

                mysql = "UPDATE Invoices_List SET Customer_BalanceSet = 1,Editable=0 WHERE (id = " + New_Invoice_ListTable_id + ") ";
                arr.Add(mysql);

                opp.insert_me_transaction(ref Excp, 1, arr);
                opp = null;
                Excp = 0;

            }
            catch (Exception e)
            {
                Excp = 1;
                throw (e);
            }


        }
        */
        /*
         public void Set_Customer_Balance(ref int Excp, ref operations opp, int Customer_Id, long New_Invoice_ListTable_id, decimal Net_Total, string Notes)
        {


            Net_Total = Common.RoundMe(Net_Total);
            string mysql = "";


            mysql = "  UPDATE Customer SET Balance = Balance + " + Net_Total + " ,Balance_Actual=Balance_Actual +  " + Net_Total + " WHERE (Id = " + Customer_Id + ")";
            opp.insert_me(mysql);

            UpdateProcessNo(ref Excp, ref opp, "Invoices_List", New_Invoice_ListTable_id);

            mysql = " INSERT INTO Customers_Accounts (ProcessNo,Notes,Customer_Id, Reference_Id,Reference_Type_Table_Id, Export, Balance, Transaction_Type, Transaction_Date,Reference_Type) ";
            mysql = mysql + " SELECT Invoices_List.ProcessNo,N'" + Notes + "' ,Invoices_List.Customer_Id, Invoices_List.Invoice_Id,Invoices_List.id, Invoices_List.Net_Total, Customer.Balance, 2 AS Transaction_Type, Invoices_List.Invoice_Date ,1 as Reference_Type";
            mysql = mysql + " FROM Customer INNER JOIN  Invoices_List ON Customer.Id = Invoices_List.Customer_Id ";
            mysql = mysql + " WHERE (Customer.Id = " + Customer_Id + ") AND (Invoices_List.id = " + New_Invoice_ListTable_id + ") ";
            opp.insert_me(mysql);

            mysql = "UPDATE Invoices_List SET Customer_BalanceSet = 1,Editable=0 WHERE (id = " + New_Invoice_ListTable_id + ") ";
            opp.insert_me(mysql);
             
        }

        */
        public void Set_Customer_BalanceInvoiceReverse(ref int Excp, ArrayList arr, int Customer_Id, long New_Invoice_ListTable_id, decimal Net_Total)
        {

            try
            {
                Net_Total = Common.RoundMe(Net_Total);
                string mysql = "";
                operations opp = new operations();
                mysql = "  UPDATE Customer SET Balance = Balance - " + Net_Total + " ,Balance_Actual=Balance_Actual -  " + Net_Total + " WHERE (Id = " + Customer_Id + ")";
                arr.Add(mysql);

               // UpdateProcessNo(ref Excp, ref opp, "Invoices_List_Reverse", New_Invoice_ListTable_id);


                mysql = " INSERT INTO Customers_Accounts (ProcessNo,Customer_Id, Reference_Id,Reference_Type_Table_Id, Import, Balance, Transaction_Type, Transaction_Date,Reference_Type) ";
                mysql = mysql + " SELECT Invoices_List_Reverse.ProcessNo,Invoices_List_Reverse.Customer_Id, Invoices_List_Reverse.Invoice_Id,Invoices_List_Reverse.id, Invoices_List_Reverse.Net_Total, Customer.Balance, 2 AS Transaction_Type, Invoices_List_Reverse.Invoice_Date ,7 as Reference_Type";
                mysql = mysql + " FROM Customer INNER JOIN  Invoices_List_Reverse ON Customer.Id = Invoices_List_Reverse.Customer_Id ";
                mysql = mysql + " WHERE (Customer.Id = " + Customer_Id + ") AND (Invoices_List_Reverse.id = " + New_Invoice_ListTable_id + ") ";
                arr.Add(mysql);

                mysql = "UPDATE Invoices_List_Reverse SET Customer_BalanceSet = 1,Editable=0 WHERE (id = " + New_Invoice_ListTable_id + ") ";
                arr.Add(mysql);

                int Status = opp.insert_me_transaction(arr);
                opp = null;
                if (Status == 1)
                {
                    Excp = 0;

                }
                else
                {
                    Excp = 1;
                }

            }
            catch (Exception e)
            {
                Excp = 1;
                throw (e);
            }


        }

        public void Set_Customer_BalanceInvoiceReverse(ref int Excp, ref operations opp, int Customer_Id, long New_Invoice_ListTable_id, decimal Net_Total, string Notes)
        {
            // CarsId,CarNumber,CustomerName, CarsWorkOrdersId, WorkOrderNumber,TaxInternalId,
            //Invoices_List_Reverse.CarsId,Invoices_List_Reverse.CarNumber,Invoices_List_Reverse.CustomerName, Invoices_List_Reverse.CarsWorkOrdersId, Invoices_List_Reverse.WorkOrderNumber, Invoices_List_Reverse.TaxInternalId,

            if (Common.Is_Set_Customer_Balance_With_Sum_Items == 1)
            {
                Set_Customer_Balance_With_Sum_Items_Invoices_List_Reverse(ref Excp, ref opp, Customer_Id, New_Invoice_ListTable_id, Net_Total, Notes);
            }

            else if (Common.Is_Set_Customer_Balance_With_Items == 1)
            {
                this.Set_Customer_BalanceInvoiceReverseWithItem(ref Excp, ref opp, Customer_Id, New_Invoice_ListTable_id, Net_Total, Notes);

            }
            else
            {
                this.Set_Customer_BalanceInvoiceReverse_(ref Excp, ref opp, Customer_Id, New_Invoice_ListTable_id, Net_Total, Notes);
            }
        }


        public void Set_Customer_BalanceInvoiceReverse_(ref int Excp, ref operations opp, int Customer_Id, long New_Invoice_ListTable_id, decimal Net_Total, string Notes)
        {
            long Customer_Id_RelatedToAccount = GetCustomer_Id_RelatedToAccount(ref opp, Customer_Id);
            string str = "";
            str = "  Delete from  Customers_Accounts Where Reference_Type_Table_Id=" + New_Invoice_ListTable_id.ToString() + " and Reference_Type=" + Common.InvoiceReverse_Type_Id.ToString();
            opp.insert_me(str);


            Net_Total = Common.RoundMe(Net_Total);

            string[] textArray1 = new string[] { "  UPDATE Customer SET Balance = Balance - ", Net_Total.ToString(), " ,Balance_Actual=Balance_Actual -  ", Net_Total.ToString(), " WHERE (Id = ", Customer_Id.ToString(), ")" };
            str = string.Concat(textArray1);
            opp.insert_me(str);

            //UpdateProcessNo(ref Excp, ref opp, "Invoices_List_Reverse", New_Invoice_ListTable_id);





            str = " INSERT INTO Customers_Accounts (CarsId,CarNumber,CustomerName, CarsWorkOrdersId, WorkOrderNumber,TaxInternalId,ProcessNo,Notes,Customer_Id,Customer_Id_RelatedToAccount, Reference_Id,Reference_Type_Table_Id, Import, Balance, Transaction_Type, Transaction_Date,Reference_Type) ";
            str = str + " SELECT Invoices_List_Reverse.CarsId,Invoices_List_Reverse.CarNumber,Invoices_List_Reverse.CustomerName, Invoices_List_Reverse.CarsWorkOrdersId, Invoices_List_Reverse.WorkOrderNumber, Invoices_List_Reverse.TaxInternalId,Invoices_List_Reverse.ProcessNo,N'" + Notes + "'," + Customer_Id_RelatedToAccount + ",Invoices_List_Reverse.Customer_Id, Invoices_List_Reverse.Invoice_Id,Invoices_List_Reverse.id, Invoices_List_Reverse.Net_Total, Customer.Balance, 2 AS Transaction_Type, Invoices_List_Reverse.Invoice_Date ,7 as Reference_Type ";
            str = str + "  FROM Customer INNER JOIN  Invoices_List_Reverse ON Customer.Id = Invoices_List_Reverse.Customer_Id ";
            str = str + "  WHERE (Customer.Id = " + Customer_Id.ToString() + ") AND (Invoices_List_Reverse.id = " + New_Invoice_ListTable_id.ToString() + ") ";

            opp.insert_me(str);
            str = "UPDATE Invoices_List_Reverse SET Customer_BalanceSet = 1,Editable=0 WHERE (id = " + New_Invoice_ListTable_id.ToString() + ") ";
            opp.insert_me(str);
        }

       
        private void Set_Customer_BalanceInvoiceReverseWithItem(ref int Excp, ref operations opp, int Customer_Id, long New_Invoice_ListTable_id, decimal Net_Total, string Notes)
        {
            long Customer_Id_RelatedToAccount = GetCustomer_Id_RelatedToAccount(ref opp, Customer_Id);
            bool Is_SumItemQnty = false;
            string str = "";
            str = "  Delete from  Customers_Accounts Where Reference_Type_Table_Id=" + New_Invoice_ListTable_id.ToString() + " and Reference_Type=" + Common.InvoiceReverse_Type_Id.ToString();
            opp.insert_me(str);
            Net_Total = Common.RoundMe(Net_Total);
            string[] textArray1 = new string[] { "  UPDATE Customer SET Balance = Balance + ", Net_Total.ToString(), " ,Balance_Actual=Balance_Actual +  ", Net_Total.ToString(), " WHERE (Id = ", Customer_Id.ToString(), ")" };
            str = string.Concat(textArray1);
            opp.insert_me(str);
            //Larg_Unit_Price,Item_Name,
            //Invoices_Details_Reverse.Larg_Unit_Price,Item.Item_Name,



            //UpdateProcessNo(ref Excp, ref opp, "Invoices_List_Reverse", New_Invoice_ListTable_id);




            str = @" INSERT INTO Customers_Accounts (CarsId,CarNumber,CustomerName, CarsWorkOrdersId, WorkOrderNumber,TaxInternalId,ProcessNo,Larg_Unit_Price,Item_Id,Item_Name,Notes, Customer_Id, Customer_Id_RelatedToAccount,Reference_Id, Reference_Type_Table_Id, Import, Balance, Transaction_Type, Transaction_Date, Reference_Type, Larg_Qnty,Small_Qnty, Item_No, Item_Size, Qnty_Size,Qnty_value,Height_Width_Item) 
                 SELECT  Invoices_List_Reverse.CarsId,Invoices_List_Reverse.CarNumber,Invoices_List_Reverse.CustomerName, Invoices_List_Reverse.CarsWorkOrdersId, Invoices_List_Reverse.WorkOrderNumber, Invoices_List_Reverse.TaxInternalId,Invoices_List_Reverse.ProcessNo,Invoices_Details_Reverse.Larg_Unit_Price,Invoices_Details_Reverse.Item_Id,Item.Item_Name,Invoices_Details_Reverse.NameSpecification, " + Customer_Id_RelatedToAccount + ",Invoices_List_Reverse.Customer_Id, Invoices_List_Reverse.Invoice_Id, Invoices_List_Reverse.ID,Invoices_Details_Reverse.Qnty_value, Customer.Balance, 1 AS Transaction_Type, Invoices_List_Reverse.Invoice_Date,   7 AS Reference_Type,Invoices_Details_Reverse.Larg_Qnty,Invoices_Details_Reverse.Small_Qnty,Invoices_Details_Reverse.Qnty_Another, Item.Height_Width_Str, CONVERT(nvarchar(100),Invoices_Details_Reverse.QntyWidth)  + N'\' + CONVERT(nvarchar(100),Invoices_Details_Reverse.QntyLenght) AS Qnty_Size, Invoices_Details_Reverse.Qnty_value, CONVERT(nvarchar(100), Item.Width_) + N'/' + CONVERT(nvarchar(100), Item.Height_) AS Height_Width_Item  ";
            str = str + "  FROM Customer INNER JOIN  Invoices_List_Reverse ON Customer.Id = Invoices_List_Reverse.Customer_Id INNER JOIN  Invoices_Details_Reverse  ON Invoices_List_Reverse.ID =Invoices_Details_Reverse.Invoice_Id INNER JOIN  Item ON Invoices_Details_Reverse.Item_Id = Item.id ";
            str = str + " WHERE (Customer.Id = " + Customer_Id.ToString() + ") AND (Invoices_List_Reverse.id = " + New_Invoice_ListTable_id.ToString() + ") ";
            //}
            opp.insert_me(str);
            str = "UPDATE Invoices_List_Reverse SET Customer_BalanceSet = 1,Editable=0 WHERE (id = " + New_Invoice_ListTable_id.ToString() + ") ";
            opp.insert_me(str);

            SetCustomers_Accounts_With_Invoices_ListReverseData(ref opp, New_Invoice_ListTable_id);
        }

        private void Set_Customer_Balance_With_Sum_Items_Invoices_List_Reverse(ref int Excp, ref operations opp, int Customer_Id, long New_Invoice_ListTable_id, decimal Net_Total, string Notes)
        {
            long Customer_Id_RelatedToAccount = GetCustomer_Id_RelatedToAccount(ref opp, Customer_Id);
            string str = "";
            str = "  Delete from  Customers_Accounts Where Reference_Type_Table_Id=" + New_Invoice_ListTable_id.ToString() + " and Reference_Type=" + Common.InvoiceReverse_Type_Id.ToString();
            opp.insert_me(str);
            Net_Total = Common.RoundMe(Net_Total);
            string[] textArray1 = new string[] { "  UPDATE Customer SET Balance = Balance + ", Net_Total.ToString(), " ,Balance_Actual=Balance_Actual +  ", Net_Total.ToString(), " WHERE (Id = ", Customer_Id.ToString(), ")" };
            str = string.Concat(textArray1);
            opp.insert_me(str);
            //Larg_Unit_Price,Item_Name,
            //Invoices_Details.Larg_Unit_Price,Item.Item_Name,

            //UpdateProcessNo(ref Excp, ref opp, "Invoices_List_Reverse", New_Invoice_ListTable_id);


            str = @" INSERT INTO Customers_Accounts (CarsId,CarNumber,CustomerName, CarsWorkOrdersId, WorkOrderNumber,TaxInternalId,ProcessNo,Larg_Unit_Price,Item_Id,Item_Name,Notes, Customer_Id,Customer_Id_RelatedToAccount, Reference_Id, Reference_Type_Table_Id, Import, Balance, Transaction_Type, Transaction_Date, Reference_Type, Larg_Qnty,Small_Qnty, Item_No, Qnty_value) 
                SELECT Invoices_List_Reverse.CarsId,Invoices_List_Reverse.CarNumber,Invoices_List_Reverse.CustomerName, Invoices_List_Reverse.CarsWorkOrdersId, Invoices_List_Reverse.WorkOrderNumber, Invoices_List_Reverse.TaxInternalId, Invoices_List_Reverse.ProcessNo,Invoices_Details_Reverse.Larg_Unit_Price, Invoices_Details_Reverse.Item_Id, Item.Item_Name, Invoices_Details_Reverse.NameSpecification  ," + Customer_Id_RelatedToAccount + ", Invoices_List_Reverse.Customer_Id, Invoices_List_Reverse.Invoice_Id, Invoices_List_Reverse.ID, SUM(Invoices_Details_Reverse.Qnty_value) AS Total ,  AVG(Customer.Balance) AS Expr3, 1 AS Transaction_Type, Invoices_List_Reverse.Invoice_Date, 7 AS Reference_Type, SUM(Invoices_Details_Reverse.Larg_Qnty) AS Larg_Qnty, SUM(Invoices_Details_Reverse.Small_Qnty) AS Small_Qnty,  SUM(Invoices_Details_Reverse.Qnty_Another) AS Qnty_Another, SUM(Invoices_Details_Reverse.Qnty_value) AS Qnty_value FROM Customer INNER JOIN Invoices_List_Reverse ON Customer.Id = Invoices_List_Reverse.Customer_Id INNER JOIN Invoices_Details_Reverse  ON Invoices_List_Reverse.ID = Invoices_Details_Reverse.Invoice_Id INNER JOIN Item ON Invoices_Details_Reverse.Item_Id = Item.id ";
            str = str + " WHERE (Customer.Id = " + Customer_Id.ToString() + ") AND (Invoices_List_Reverse.id = " + New_Invoice_ListTable_id.ToString() + ") ";
            //str = str + " GROUP BY Invoices_List_Reverse.ProcessNo,Invoices_Details_Reverse.Larg_Unit_Price, Invoices_Details_Reverse.Item_Id, Item.Item_Name, Invoices_List_Reverse.Customer_Id, Invoices_List_Reverse.Invoice_Id, Invoices_List_Reverse.ID, Invoices_List_Reverse.Invoice_Date ,Invoices_Details_Reverse.NameSpecification ";
            str = str + " GROUP BY Invoices_List_Reverse.ProcessNo, Invoices_Details_Reverse.Larg_Unit_Price, Invoices_Details_Reverse.Item_Id, Item.Item_Name, Invoices_List_Reverse.Customer_Id, Invoices_List_Reverse.Invoice_Id, \r\n                         Invoices_List_Reverse.ID, Invoices_List_Reverse.Invoice_Date, Invoices_Details_Reverse.NameSpecification, Invoices_List_Reverse.CarsId, Invoices_List_Reverse.CarNumber, Invoices_List_Reverse.CustomerName, \r\n                         Invoices_List_Reverse.CarsWorkOrdersId, Invoices_List_Reverse.WorkOrderNumber, Invoices_List_Reverse.TaxInternalId ";
            opp.insert_me(str);
            str = "UPDATE Invoices_List_Reverse SET Customer_BalanceSet = 1,Editable=0 WHERE (id = " + New_Invoice_ListTable_id.ToString() + ") ";
            opp.insert_me(str);


            SetCustomers_Accounts_With_Invoices_ListReverseData(ref opp, New_Invoice_ListTable_id);
        }
        void SetCustomers_Accounts_With_Invoices_ListData(ref operations opp, long New_Invoice_ListTable_id)
        {
            int AccountType = 0;
            int Transaction_Type = 0;
            string Notes = " ";
            string ValueImport = "0";
            string ValueExport = "0";
            int Excp = 0;
            string Msg = "";
            Account_Entry_Invoice_CLS ac = new Account_Entry_Invoice_CLS();
            DataTable dt = ac.Get_EntryForInvoices(ref Excp, ref Msg, ref opp, New_Invoice_ListTable_id, DateTime.Now.Date, DateTime.Now.Date, 0);
            if (dt.Rows.Count == 0)
            {
                return;
            }
            decimal ATS05_Value = 0;// decimal.Parse(dt.Rows[0]["ATS05_Value"].ToString());
            decimal Net_Total_CommissionValue = 0;// decimal.Parse(dt.Rows[0]["Net_Total_CommissionValue"].ToString());
            decimal TaxSales10_Value = 0;//  decimal.Parse(dt.Rows[0]["TaxSales10_Value"].ToString());
            decimal DAMGHAT = 0;// decimal.Parse(dt.Rows[0]["DAMGHAT"].ToString());
            decimal AnotherPayment = 0;// decimal.Parse(dt.Rows[0]["AnotherPayment"].ToString());

            try
            {
                ATS05_Value = decimal.Parse(dt.Rows[0]["ATS05_Value"].ToString());
            }
            catch (Exception ee) { }

            try
            {
                Net_Total_CommissionValue = decimal.Parse(dt.Rows[0]["Net_Total_CommissionValue"].ToString());

            }
            catch (Exception ee) { }
            try
            {
                TaxSales10_Value = decimal.Parse(dt.Rows[0]["TaxSales10_Value"].ToString());

            }
            catch (Exception ee) { }
            try
            {
                DAMGHAT = decimal.Parse(dt.Rows[0]["DAMGHAT"].ToString());

            }
            catch (Exception ee) { }
            try
            {
                AnotherPayment = decimal.Parse(dt.Rows[0]["AnotherPayment"].ToString());

            }
            catch (Exception ee) { }



            if (TaxSales10_Value > 0)
            {
                ////÷—«∆» «·„»Ì⁄« 
                Transaction_Type = 2;
                AccountType = Common.TaxSales10_Account_Type_ForSales;


                Notes = "÷—«∆» «·„»Ì⁄« ";
                ValueImport = "0";
                ValueExport = "Invoices_List.TaxSales10_Value";
                SetCustomers_Accounts_With_Invoices_ListData(ref opp, New_Invoice_ListTable_id, Notes, ValueImport, ValueExport, Transaction_Type, AccountType);

            }
            if (DAMGHAT > 0)
            {
                ////«·œ„€…
                Transaction_Type = 2;
                AccountType = Common.DAMGHAT_Account_Type;

                //EntryValue = EntryValue * Factor;
                Notes = "«·œ„€…  ";
                ValueImport = "0";
                ValueExport = "Invoices_List.DAMGHAT";
                SetCustomers_Accounts_With_Invoices_ListData(ref opp, New_Invoice_ListTable_id, Notes, ValueImport, ValueExport, Transaction_Type, AccountType);
            }
            if (AnotherPayment > 0)
            {

                /////„’—Ê›«  «Œ—Ï ⁄·Ï ›« Ê—… «·„»Ì⁄« 
                Transaction_Type = 2;
                AccountType = Common.Masrofat_Other_Selles_Account_Type;
                // EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["Load_Payment"].ToString());


                Notes = "„’—Ê›«  √Œ—Ï";
                ValueImport = "0";
                ValueExport = "Invoices_List.AnotherPayment";
                SetCustomers_Accounts_With_Invoices_ListData(ref opp, New_Invoice_ListTable_id, Notes, ValueImport, ValueExport, Transaction_Type, AccountType);

            }
            if (ATS05_Value > 0)
            {
                ////Õ”«» √—»«Õ  Ã«—Ì… Ê’‰«⁄Ì…
                Transaction_Type = 1;
                AccountType = Common.ATS05_Value_Account_Type_ForSales;


                Notes = "√—»«Õ  Ã«—Ì… Ê’‰«⁄Ì…";
                ValueImport = "Invoices_List.ATS05_Value";
                ValueExport = "0";
                SetCustomers_Accounts_With_Invoices_ListData(ref opp, New_Invoice_ListTable_id, Notes, ValueImport, ValueExport, Transaction_Type, AccountType);
            }
            if (Net_Total_CommissionValue > 0)
            {

                ////«·Œ’„ «·„”„ÊÕ Ì…
                Transaction_Type = 1;
                AccountType = Common.CommissionValue_Account_Type;

                //EntryValue = EntryValue * Factor;

                Notes = "«·Œ’„ «·„”„ÊÕ Ì…";
                ValueImport = "Invoices_List.Net_Total_CommissionValue";
                ValueExport = "0";
                SetCustomers_Accounts_With_Invoices_ListData(ref opp, New_Invoice_ListTable_id, Notes, ValueImport, ValueExport, Transaction_Type, AccountType);


            }

        }

        void SetCustomers_Accounts_With_Invoices_ListData(ref operations opp, long New_Invoice_ListTable_id, string Notes, string ValueImport, string ValueExport, int Transaction_Type, int AccountType)
        {

            string str = "";
            str = @" INSERT INTO Customers_Accounts (ProcessNo, Notes, Customer_Id, Reference_Id, Reference_Type_Table_Id,Import, Export, Balance, Transaction_Type, Transaction_Date, Reference_Type)  ";
            str = str + @" SELECT Invoices_List.ProcessNo, '" + Notes + "' AS Notes, Invoices_List.Customer_Id, Invoices_List.Invoice_Id, Invoices_List.ID," + ValueImport + ",  " + ValueExport + ", Customer.Balance,  " + Transaction_Type + " AS Transaction_Type, Invoices_List.Invoice_Date, " + Common.Invoice_Type_Id.ToString() + " AS Reference_Type ";
            str = str + @" FROM Customer INNER JOIN Invoices_List ON Customer.Id = Invoices_List.Customer_Id ";
            str = str + @" WHERE  (Invoices_List.Id = " + New_Invoice_ListTable_id.ToString() + ") ";

            opp.insert_me(str);
        }

        void SetCustomers_Accounts_With_Invoices_ListReverseData(ref operations opp, long New_Invoice_ListTable_id)
        {
            int AccountType = 0;
            int Transaction_Type = 0;
            decimal EntryValue = 0;
            string Notes = " ";
            string ValueImport = "0";
            string ValueExport = "0";
            Account_Entry_Invoice_CLS ac = new Account_Entry_Invoice_CLS();
            int Excp = 0;
            string Msg = "";
            DataTable dt = ac.Get_EntryForInvoices_Reverse(ref Excp, ref Msg, ref opp, New_Invoice_ListTable_id, DateTime.Now.Date, DateTime.Now.Date, 0);
            if (dt.Rows.Count == 0)
            {
                return;
            }
            decimal ATS05_Value = 0;// decimal.Parse(dt.Rows[0]["ATS05_Value"].ToString());
            decimal Net_Total_CommissionValue = 0;// decimal.Parse(dt.Rows[0]["Net_Total_CommissionValue"].ToString());
            decimal TaxSales10_Value = 0;//decimal.Parse(dt.Rows[0]["TaxSales10_Value"].ToString());
            decimal DAMGHAT = 0;//decimal.Parse(dt.Rows[0]["DAMGHAT"].ToString());
            decimal AnotherPayment = 0;// decimal.Parse(dt.Rows[0]["AnotherPayment"].ToString());



            try
            {
                ATS05_Value = decimal.Parse(dt.Rows[0]["ATS05_Value"].ToString());
            }
            catch (Exception ee)
            {
            }
            try
            {
                Net_Total_CommissionValue = decimal.Parse(dt.Rows[0]["Net_Total_CommissionValue"].ToString());
            }
            catch (Exception ee)
            {
            }
            try
            {
                TaxSales10_Value = decimal.Parse(dt.Rows[0]["TaxSales10_Value"].ToString());
            }
            catch (Exception ee)
            {
            }
            try
            {
                DAMGHAT = decimal.Parse(dt.Rows[0]["DAMGHAT"].ToString());
            }
            catch (Exception ee)
            {
            }
            try
            {
                AnotherPayment = decimal.Parse(dt.Rows[0]["AnotherPayment"].ToString());
            }
            catch (Exception ee)
            {
            }


            if (TaxSales10_Value > 0)
            {

                ////÷—«∆» «·„»Ì⁄« 
                Transaction_Type = 1;
                AccountType = Common.TaxSales10_Account_Type_ForSales;


                Notes = "÷—«∆» «·„»Ì⁄« ";
                ValueImport = "Invoices_List_Reverse.TaxSales10_Value";
                ValueExport = "0";
                Set_Customers_Accounts_With_Invoices_List_ReverseData(ref opp, New_Invoice_ListTable_id, Notes, ValueImport, ValueExport, Transaction_Type, AccountType);

            }
            if (DAMGHAT > 0)
            {
                ////«·œ„€…
                Transaction_Type = 1;
                AccountType = Common.DAMGHAT_Account_Type;

                //EntryValue = EntryValue * Factor;
                Notes = "«·œ„€…  ";
                ValueImport = "Invoices_List_Reverse.DAMGHAT";
                ValueExport = "0";
                Set_Customers_Accounts_With_Invoices_List_ReverseData(ref opp, New_Invoice_ListTable_id, Notes, ValueImport, ValueExport, Transaction_Type, AccountType);

            }
            if (AnotherPayment > 0)
            {
                /////„’—Ê›«  «Œ—Ï ⁄·Ï ›« Ê—… «·„»Ì⁄« 
                Transaction_Type = 1;
                AccountType = Common.Masrofat_Other_Selles_Account_Type;
                // EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["AnotherPayment"].ToString());


                Notes = "„’—Ê›«  √Œ—Ï";
                ValueImport = "Invoices_List_Reverse.AnotherPayment";
                ValueExport = "0";
                Set_Customers_Accounts_With_Invoices_List_ReverseData(ref opp, New_Invoice_ListTable_id, Notes, ValueImport, ValueExport, Transaction_Type, AccountType);
            }
            if (ATS05_Value > 0)
            {
                ////Õ”«» √—»«Õ  Ã«—Ì… Ê’‰«⁄Ì…
                Transaction_Type = 2;
                AccountType = Common.ATS05_Value_Account_Type_ForSales;


                Notes = "√—»«Õ  Ã«—Ì… Ê’‰«⁄Ì…";
                ValueImport = "0";
                ValueExport = "Invoices_List_Reverse.ATS05_Value";
                Set_Customers_Accounts_With_Invoices_List_ReverseData(ref opp, New_Invoice_ListTable_id, Notes, ValueImport, ValueExport, Transaction_Type, AccountType);

            }
            if (Net_Total_CommissionValue > 0)
            {
                ////«·Œ’„ «·„”„ÊÕ Ì…
                Transaction_Type = 2;
                AccountType = Common.CommissionValue_Account_Type;

                //EntryValue = EntryValue * Factor;

                Notes = "«·Œ’„ «·„”„ÊÕ Ì…";
                ValueImport = "0";
                ValueExport = "Invoices_List_Reverse.Net_Total_CommissionValue";
                Set_Customers_Accounts_With_Invoices_List_ReverseData(ref opp, New_Invoice_ListTable_id, Notes, ValueImport, ValueExport, Transaction_Type, AccountType);

            }

        }

        void Set_Customers_Accounts_With_Invoices_List_ReverseData(ref operations opp, long New_Bill_ListTable_id, string Notes, string ValueImport, string ValueExport, int Transaction_Type, int AccountType)
        {

            string str = "";
            str = @" INSERT INTO Customers_Accounts (ProcessNo, Notes, Customer_Id, Reference_Id, Reference_Type_Table_Id,Import, Export, Balance, Transaction_Type, Transaction_Date, Reference_Type)  ";
            str = str + @" SELECT Invoices_List_Reverse.ProcessNo, '" + Notes + "' AS Notes, Invoices_List_Reverse.Customer_Id, Invoices_List_Reverse.Invoice_Id, Invoices_List_Reverse.ID," + ValueImport + ",  " + ValueExport + ", Customer.Balance,  " + Transaction_Type + " AS Transaction_Type, Invoices_List_Reverse.Invoice_Date, " + Common.InvoiceReverse_Type_Id.ToString() + " AS Reference_Type ";
            str = str + @" FROM Customer INNER JOIN Invoices_List_Reverse ON Customer.Id = Invoices_List_Reverse.Customer_Id ";
            str = str + @" WHERE  (Invoices_List_Reverse.Id = " + New_Bill_ListTable_id.ToString() + ") ";

            opp.insert_me(str);
        }
        public void Set_Customer_SalesMan(ref int Excp, int SalesMan_id, int Customer_Id)
        {

            try
            {
                string mysql = "";
                operations opp = new operations();
                mysql = " update Customer Set SalesMan_Id=" + SalesMan_id + " where ID=" + Customer_Id;
                opp.update_me(mysql);
                Excp = 0;
            }
            catch (Exception e)
            {
                Excp = 1;
                throw (e);
            }


        }

        public void NotSet_Customer_SalesMan(ref int Excp, int Customer_Id)
        {

            try
            {
                string mysql = "";
                operations opp = new operations();
                mysql = " update Customer Set SalesMan_Id=-1 where ID=" + Customer_Id;
                opp.update_me(mysql);
                Excp = 0;
            }
            catch (Exception e)
            {
                Excp = 1;
                throw (e);
            }


        }
        public DataTable Get_Customer_List(int SalesMan_Id)
        {

            DataTable dtCustomer = new DataTable();
            try
            {
                string mysql = "SELECT  * FROM Customer WHERE (Active = 1) and IsSalesMan=0 and SalesMan_Id=" + SalesMan_Id + " ORDER BY Name ";
                operations opp = new operations();
                dtCustomer = opp.return_DataTable(mysql);
                // Excp = 1;

                return dtCustomer;
            }
            catch
            {
                //Excp =  1;
                return dtCustomer;
            }
        }
        decimal MyRound(decimal V)
        {
            return Common.RoundMePositive(V);
            /*
           double d = double.Parse(V.ToString());
           d = Common.RoundMe(d);
           decimal x = (decimal)d;
           return x;
             * */
        }
        public void Set_CustomersAccount_Balance(ref int Excp, DateTime Transaction_Date_, long Customer_Id, decimal Net_Total, long Process_No, int Reference_Type)
        {
            try
            {

                operations opp = new operations();
                Set_CustomersAccount_Balance(ref Excp, ref opp, Transaction_Date_, Customer_Id, Net_Total, Process_No, Reference_Type);
            }
            catch
            {
                Excp = 1;
            }
        }

        public void Set_CustomersAccount_Balance(ref int Excp, ref operations opp, DateTime Transaction_Date_, long Customer_Id, decimal Net_Total, long Process_No, int Reference_Type)
        {

            Net_Total = MyRound(Net_Total);
            string mysql = "";
            long Reference_Id = 0;
            long Reference_Type_Table_Id = Process_No;
            decimal Export = Net_Total;
            int Transaction_Type = 2;
            DateTime Transaction_Date = Transaction_Date_;
            // int Reference_Type = -1;  /// ·· ÿ—Ì“
            decimal Balance = 0;

            mysql = " Update Customer set Balance=Balance - " + Net_Total + " ,Balance_Actual=Balance_Actual -  " + Net_Total + " where id=" + Customer_Id;
            opp.insert_me(mysql);
            DataSet dss = opp.return_dataset("Select * from Customer where id=" + Customer_Id);
            try
            {
                Balance = decimal.Parse(dss.Tables[0].Rows[0]["Balance"].ToString());
            }
            catch
            {
                Balance = 0;
                Excp = 1;
                return;
            }

            mysql = " INSERT INTO Customers_Accounts (Customer_Id, Reference_Id,Reference_Type_Table_Id, Export, Balance, Transaction_Type, Transaction_Date,Reference_Type) ";
            mysql = mysql + " Values(" + Customer_Id + "," + Reference_Id + "," + Reference_Type_Table_Id + "," + Export + "," + Balance + "," + Transaction_Type + ",'" + operations.dateFormat(Transaction_Date) + "'," + Reference_Type + ")";
            opp.insert_me(mysql);
            Excp = 0;

        }
        public void Set_CustomersAccount_Balance(ref int Excp, DateTime Transaction_Date_, int Customer_Id, decimal Net_Total, long Process_No, int Reference_Type, int MadinOrDa2n)
        {
            try
            {
                Net_Total = MyRound(Net_Total);
                string mysql = "";
                long Reference_Id = 0;
                long Reference_Type_Table_Id = Process_No;
                decimal Import = Net_Total;
                int Transaction_Type = 1;
                DateTime Transaction_Date = Transaction_Date_;
                // int Reference_Type = -1;  /// ·· ÿ—Ì“
                decimal Balance = 0;
                operations opp = new operations();
                mysql = " Update Customer set Balance=Balance - " + Net_Total + " ,Balance_Actual=Balance_Actual -  " + Net_Total + " where id=" + Customer_Id;
                opp.update_me(mysql);
                DataSet dss = opp.return_dataset("Select * from Customer where id=" + Customer_Id);
                try
                {
                    Balance = decimal.Parse(dss.Tables[0].Rows[0]["Balance"].ToString());
                }
                catch
                {
                    Balance = 0;
                    Excp = 1;
                    return;
                }
                if (Reference_Type == -9) // „»Ì⁄«  œ‘  ·„Ê—œ
                {
                    Reference_Id = Process_No;
                }

                if (MadinOrDa2n == 2)
                {
                    Import = Import * -1;
                    mysql = " INSERT INTO Customers_Accounts (Supplier_Id, Reference_Id,Reference_Type_Table_Id, export , Balance, Transaction_Type, Transaction_Date,Reference_Type) ";
                    mysql = mysql + " Values(" + Customer_Id + "," + Reference_Id + "," + Reference_Type_Table_Id + "," + Import + "," + Balance + "," + Transaction_Type + ",'" + operations.dateFormat(Transaction_Date) + "'," + Reference_Type + ")";
                }
                else if (MadinOrDa2n == 1)
                {

                    mysql = " INSERT INTO Customers_Accounts (Supplier_Id, Reference_Id,Reference_Type_Table_Id, Import, Balance, Transaction_Type, Transaction_Date,Reference_Type) ";
                    mysql = mysql + " Values(" + Customer_Id + "," + Reference_Id + "," + Reference_Type_Table_Id + "," + Import + "," + Balance + "," + Transaction_Type + ",'" + operations.dateFormat(Transaction_Date) + "'," + Reference_Type + ")";

                }

                opp.insert_me(mysql);
                Excp = 0;
            }
            catch
            {
                Excp = 1;
            }
        }

        public DataSet Get_SalesMAn_List()
        {

            DataSet dsCustomer = new DataSet();
            try
            {
                string mysql = "SELECT  * FROM Customer WHERE (Active = 1) and IsSalesMan=1 ORDER BY Name ";
                operations opp = new operations();
                dsCustomer = opp.return_dataset(mysql, "Customer");
                // Excp = 1;
                opp = null;
                return dsCustomer;
            }
            catch
            {
                //Excp =  1;
                return dsCustomer;
            }
        }
        public DataTable GetCustomerFirstBalance(long Customer_Id)
        {

            DataTable dt = new DataTable();

            string mysql = "SELECT  * FROM Customers_Accounts WHERE (Customer_Id = " + Customer_Id + ")  And Reference_Type = -7";
            operations opp = new operations();
            dt = opp.return_DataTable(mysql);
            return dt;

        }

        public DataSet Get_Customer_Account(int Customer_Id, bool _GetDateFrom, DateTime GetDateFrom, bool _GetDateTo, DateTime GetDateTo, bool DESC)
        {

            G_DS.Clear();
            string mysql = "";
            decimal BalanceBefor = 0;
            try
            {
                mysql = " SELECT Customers_Accounts.*,'' As BalanceImport,'' As BalanceExport FROM Customers_Accounts WHERE Flage_View=1  ";
                if (_GetDateFrom == true)
                {
                    mysql = mysql + " AND (Transaction_Date >= '" + operations.dateFormat(GetDateFrom.Date) + "')  ";
                }
                if (_GetDateTo == true)
                {
                    mysql = mysql + "   AND (Transaction_Date <='" + operations.dateFormat(GetDateTo.Date) + "' )  ";
                }
                mysql = mysql + "  AND (Customer_Id = " + Customer_Id + ") ";
                if (DESC)
                {
                    mysql = mysql + "  ORDER BY ID  DESC";
                }
                else
                {
                    mysql = mysql + "  ORDER BY ID  ASC";
                }
                operations opp = new operations();
                G_DS = opp.return_dataset(mysql, "Customers_Accounts");

                if (_GetDateFrom == true)
                {
                    mysql = @"SELECT   SUM(Balance) AS Balance FROM Customers_Accounts ";
                    mysql = mysql + " Where (Transaction_Date < '" + operations.dateFormat(GetDateFrom.Date) + "')  AND (Customer_Id = " + Customer_Id + ") ";
                    DataTable dt = opp.return_DataTable(mysql);
                    if (dt.Rows.Count > 0)
                    {
                        try { BalanceBefor = decimal.Parse(dt.Rows[0]["Balance"].ToString()); }
                        catch { BalanceBefor = 0; }
                    }
                }

                int Count = G_DS.Tables[0].Rows.Count;
                //if (DESC)
                //{
                //   DataRow dr= G_DS.Tables[0].NewRow();
                //    dr["Balance"] = BalanceBefor;
                //    dr["Reference_Type_View"] = "—’Ìœ ”«»ﬁ";
                //    G_DS.Tables[0].Rows.InsertAt(dr, Count++);

                //}
                //else
                //{
                //    DataRow dr = G_DS.Tables[0].NewRow();
                //    dr["Balance"] = BalanceBefor;
                //    dr["Reference_Type_View"] = "—’Ìœ ”«»ﬁ";
                //    G_DS.Tables[0].Rows.InsertAt(dr,0);
                //}

                //  Excp = 1;
                opp = null;
                return G_DS;
            }
            catch
            {
                // Excp = 1;
                return G_DS;
            }
        }
        public DataTable Get_Customer_AccountComputeBalance_(int Customer_Id, bool _GetDateFrom, DateTime GetDateFrom, bool _GetDateTo, DateTime GetDateTo, bool DESC)
        {
            DateTime dtBefor = GetDateFrom;

            TimeSpan ts = new TimeSpan(1, 0, 0, 0);

            dtBefor = dtBefor.Subtract(ts);

            DataTable dtAccount = new DataTable();
            operations opp = new operations();

            string mysql = "";
            decimal BalanceBefor = 0;
            try
            {
                bool IsFountBalanceBefor = false;
                int FirstRowToCompute = 0;
                if (_GetDateFrom == true)
                {
                    mysql = @"SELECT   (SUM(Export) - SUM(Import)) AS Balance FROM Customers_Accounts ";
                    mysql = mysql + " GROUP BY Customer_Id, Transaction_Date ";

                    mysql = mysql + " HAVING (Transaction_Date < '" + operations.dateFormat(GetDateFrom.Date) + "')  AND (Customer_Id = " + Customer_Id + ") ";
                    DataTable dt = opp.return_DataTable(mysql);
                    if (dt.Rows.Count > 0)
                    {
                        IsFountBalanceBefor = true;
                        try { BalanceBefor = decimal.Parse(dt.Rows[0]["Balance"].ToString()); }
                        catch { BalanceBefor = 0; }
                    }
                }


                mysql = " SELECT Customers_Accounts.*,0 AS Serial_,'' As BalanceImport,'' As BalanceExport FROM Customers_Accounts WHERE Flage_View=1      ";
                if (_GetDateFrom == true)
                {
                    mysql = mysql + " AND (Transaction_Date >= '" + operations.dateFormat(GetDateFrom.Date) + "')  ";
                }
                if (_GetDateTo == true)
                {
                    mysql = mysql + "   AND (Transaction_Date <='" + operations.dateFormat(GetDateTo.Date) + "' )  ";
                }
                mysql = mysql + "  AND (Customer_Id = " + Customer_Id + ") ";

                mysql = mysql + "  ORDER BY    Transaction_Date";


                dtAccount = opp.return_DataTable(mysql);



                int Count = dtAccount.Rows.Count;
                if (IsFountBalanceBefor == true)
                {
                    DataRow dr2 = dtAccount.NewRow();
                    dr2["Balance"] = BalanceBefor;
                    dr2["Import"] = 0;
                    dr2["Export"] = 0;
                    dr2["Transaction_Date"] = dtBefor;
                    dr2["Reference_Type"] = 0;

                    dr2["Reference_Type_View"] = "—’Ìœ ”«»ﬁ";
                    dtAccount.Rows.InsertAt(dr2, 0);
                    FirstRowToCompute = 1;
                }
                ///////////////////////////////////////////////////////////
                decimal Balance = BalanceBefor;
                decimal ImportTotal = 0;
                decimal ExportTotal = 0;
                for (int x = FirstRowToCompute; x < dtAccount.Rows.Count; x++)
                {
                    decimal Import = decimal.Parse(dtAccount.Rows[x]["Import"].ToString());
                    decimal Export = decimal.Parse(dtAccount.Rows[x]["Export"].ToString());

                    string Reference_Type = dtAccount.Rows[x]["Reference_Type"].ToString();
                    if (Reference_Type == "-7")
                    {


                        dtAccount.Rows[x]["Import"] = 0;
                        dtAccount.Rows[x]["Export"] = 0;
                        Balance = Export - Import;
                        dtAccount.Rows[x]["Balance"] = Balance;


                    }
                    else
                    {
                        ImportTotal = ImportTotal + Import;

                        ExportTotal = ExportTotal + Export;
                        Balance = Balance + Export - Import;
                        dtAccount.Rows[x]["Balance"] = Balance;
                    }
                }

                DataRow dr = dtAccount.NewRow();
                dr["Balance"] = 0;
                dr["Import"] = ImportTotal;
                dr["Export"] = ExportTotal;
                //dr["Reference_Type"] = "-";

                dr["Reference_Type_View"] = "-";
                dr["Notes"] = "≈Ã„«·Ï «·Õ—ﬂ« ";
                dtAccount.Rows.Add(dr);
                decimal FinalBalance = BalanceBefor + ExportTotal - ImportTotal;

                DataRow dr1 = dtAccount.NewRow();
                dr1["Balance"] = FinalBalance;
                dr1["Import"] = 0;
                dr1["Export"] = 0;
                //dr1["Reference_Type"] = "-";

                dr1["Reference_Type_View"] = "-";
                dr1["Notes"] = "’«›Ï «·—’Ìœ  ";
                dtAccount.Rows.Add(dr1);


                //  Excp = 1;

                return dtAccount;
            }
            catch (Exception ee)
            {
                // Excp = 1;

            }
            return dtAccount;
        }
        public DataTable Get_Customer_AccountComputeBalance(int Customer_Id, bool _GetDateFrom, DateTime GetDateFrom, bool _GetDateTo, DateTime GetDateTo, bool DESC)
        {
            DateTime dtBefor = GetDateFrom;

            TimeSpan ts = new TimeSpan(1, 0, 0, 0);

            dtBefor = dtBefor.Subtract(ts);

            DataTable dtAccount = new DataTable();
            operations opp = new operations();

            string mysql = "";
            decimal BalanceBefor = 0;
            decimal ImportBefor = 0;
            decimal ExportBefor = 0;
            try
            {
                bool IsFountBalanceBefor = false;
                //int FirstRowToCompute = 0;
                if (_GetDateFrom == true)
                {
                    mysql = @"SELECT  SUM(Import)   AS Import, SUM(Export)   AS Export,  (SUM(Export) - SUM(Import)) AS Balance FROM Customers_Accounts ";


                    mysql = mysql + " WHere  (Transaction_Date < '" + operations.dateFormat(GetDateFrom.Date) + "')  AND (Customer_Id = " + Customer_Id + ") ";
                    DataTable dt = opp.return_DataTable(mysql);
                    if (dt.Rows.Count > 0)
                    {
                        IsFountBalanceBefor = true;
                        try
                        {
                            BalanceBefor = decimal.Parse(dt.Rows[0]["Balance"].ToString());
                            ImportBefor = decimal.Parse(dt.Rows[0]["Import"].ToString());
                            ExportBefor = decimal.Parse(dt.Rows[0]["Export"].ToString());
                        }
                        catch { BalanceBefor = 0; }
                    }
                }


                mysql = " SELECT Customers_Accounts.*,0 AS Serial_,0.0 As BalanceImport,0.0 As BalanceExport FROM Customers_Accounts WHERE Flage_View=1      ";
                if (_GetDateFrom == true)
                {
                    mysql = mysql + " AND (Transaction_Date >= '" + operations.dateFormat(GetDateFrom.Date) + "')  ";
                }
                if (_GetDateTo == true)
                {
                    mysql = mysql + "   AND (Transaction_Date <='" + operations.dateFormat(GetDateTo.Date) + "' )  ";
                }
                mysql = mysql + "  AND (Customer_Id = " + Customer_Id + ") ";

                mysql = mysql + "  ORDER BY    Transaction_Date ,Id";


                dtAccount = opp.return_DataTable(mysql);



                ///////////////«·Õ’Ê· ⁄·Ï «Ã„«·Ï «·Õ—ﬂ«  «·”«»ﬁÂ Ê«÷«› Â« ⁄·Ï —’Ìœ «Ê· «·„œÂ
                decimal Final_First_Balance_Madin = 0;
                decimal Final_First_Balance_Da2n = 0;
                decimal First_Balance = BalanceBefor;


                //Õ”«» —’Ìœ √Ê· «·„œÂ
                //decimal First_Balance_Madin = 0;
                //decimal First_Balance_Da2n = 0;

                //Get_FirstBalance_Period(Year_Financial_Period_Id, Branch_Id, Submain_Account_ID, ref   First_Balance_Madin, ref   First_Balance_Da2n, ref Equivalent_First_Balance_Madin, ref Equivalent_First_Balance_Da2n);

                //≈÷«›… —’Ìœ «Ê· «·„œÂ ⁄·Ï «·—’Ìœ «·„‰ﬁÊ·
                //decimal First_Balance = (First_Balance_Madin - First_Balance_Da2n) + (Prev_Import_Value - Prev_Export_Value);
                if (First_Balance == 0)
                {
                    Final_First_Balance_Madin = 0;
                    Final_First_Balance_Da2n = 0;



                }
                else if (First_Balance > 0)
                {
                    Final_First_Balance_Madin = First_Balance;
                    Final_First_Balance_Da2n = 0;


                }
                else if (First_Balance < 0)
                {

                    Final_First_Balance_Madin = 0;
                    Final_First_Balance_Da2n = First_Balance * -1;


                }

                /////////////Õ”«» —’Ìœ «·Õ—ﬂ« 
                Set_ComputeBalanceNew(ref dtAccount, First_Balance);
                if (IsFountBalanceBefor == true)
                {
                    /////////////// ≈÷«›… —’Ìœ √Ê· «·„œÂ «Ê «·—’Ìœ «·„‰ﬁÊ·
                    DataRow dr11 = dtAccount.NewRow();
                    dr11["BalanceImport"] = Final_First_Balance_Madin;
                    dr11["BalanceExport"] = Final_First_Balance_Da2n;
                    dr11["Balance"] = First_Balance;
                    //dr11["Notes"] = " "—’Ìœ √Ê· «·„œÂ"";

                    dr11["Import"] = ImportBefor;
                    dr11["Export"] = ExportBefor;
                    dr11["Reference_Type"] = 0;

                    dr11["Reference_Type_View"] = "—’Ìœ ”«»ﬁ";
                    dtAccount.Rows.InsertAt(dr11, 0);

                }


                Common.Make_TableColumnSerialME(ref dtAccount);

                //////////≈÷«›… «·«Ã„«·Ï
                decimal Import = 0;
                decimal Export = 0;

                decimal Balance_Import_Value = 0;
                decimal Balance_Export_Value = 0;


                for (int x = 0; x < dtAccount.Rows.Count; x++)
                {

                    try
                    {
                        Import = Import + decimal.Parse(dtAccount.Rows[x]["Import"].ToString());
                        Import = Common.RoundMe(Import);
                    }
                    catch { }

                    try
                    {
                        Export = Export + decimal.Parse(dtAccount.Rows[x]["Export"].ToString());
                        Export = Common.RoundMe(Export);
                    }
                    catch { }



                }

                DataRow dr = dtAccount.NewRow();

                dr["Import"] = Import;
                dr["Export"] = Export;
                dr["Balance"] = 0;
                dr["Notes"] = "≈Ã„«·Ï «·Õ—ﬂÂ";

                dtAccount.Rows.Add(dr);

                //////////////////Õ”«» «·—’Ìœ «·„‰ﬁÊ·
                decimal Balance_Mankool = Export - Import;

                //Balance_Mankool = Balance_Mankool + First_Balance;
                Balance_Mankool = Common.RoundMe(Balance_Mankool);





                if (Balance_Mankool == 0)
                {
                    Final_First_Balance_Madin = 0;
                    Final_First_Balance_Da2n = 0;
                }
                else if (Balance_Mankool > 0)
                {
                    Final_First_Balance_Madin = Balance_Mankool;
                    Final_First_Balance_Da2n = 0;
                }
                else if (Balance_Mankool < 0)
                {

                    Final_First_Balance_Madin = 0;
                    Final_First_Balance_Da2n = Balance_Mankool * -1;
                }



                /////////////////////
                DataRow dr2 = dtAccount.NewRow();
                dr2["Balance"] = Balance_Mankool;
                dr2["BalanceImport"] = Final_First_Balance_Madin;
                dr2["BalanceExport"] = Final_First_Balance_Da2n;


                dr2["Notes"] = "—’Ìœ „‰ﬁÊ·";

                dtAccount.Rows.Add(dr2);



                return dtAccount;
            }
            catch (Exception ee)
            {
                // Excp = 1;

            }
            return dtAccount;
        }
        public DataTable Get_Customer_AccountComputeBalanceMokhtalat__(long Customer_Id, long Supplier_Id, bool _GetDateFrom, DateTime GetDateFrom, bool _GetDateTo, DateTime GetDateTo, bool DESC)
        {


            DateTime dtBefor = GetDateFrom;

            TimeSpan ts = new TimeSpan(1, 0, 0, 0);

            dtBefor = dtBefor.Subtract(ts);

            DataTable dtAccount = new DataTable();
            operations opp = new operations();

            string mysql = "  truncate table Customers_Accounts_Mokhtalat";
            opp.insert_me(mysql);


            mysql = @"  INSERT INTO Customers_Accounts_Mokhtalat
                         (Customers_AccountsId,ProcessNo,Insert_ServerDateTime ,Larg_Unit_Price,Item_Name,Larg_Qnty,Small_Qnty,Item_No, Customer_Id, Import, Export, Balance, Balance_Actual, Transaction_Type, Transaction_Date, Reference_Id, Reference_Type, Reference_Type_Table_Id, Payment_Type, Notes, Day_Registration_flage, DayReg_List_Id, 
                         Flage_View, BalanceImport, BalanceExport, Reference_Type_View)
            SELECT    Customers_Accounts.Id, Customers_Accounts.ProcessNo, Customers_Accounts.Insert_ServerDateTime, Customers_Accounts.Larg_Unit_Price,Customers_Accounts.Item_Name,Customers_Accounts.Larg_Qnty,Customers_Accounts.Small_Qnty,Customers_Accounts.Item_No,  Customers_Accounts.Customer_Id, Customers_Accounts.Import, Customers_Accounts.Export, Customers_Accounts.Balance, Customers_Accounts.Balance_Actual, 
                                     Customers_Accounts.Transaction_Type, Customers_Accounts.Transaction_Date, Customers_Accounts.Reference_Id, Customers_Accounts.Reference_Type, Customers_Accounts.Reference_Type_Table_Id, 
                                     Customers_Accounts.Payment_Type, Customers_Accounts.Notes, Customers_Accounts.Day_Registration_flage, Customers_Accounts.DayReg_List_Id, Customers_Accounts.Flage_View, 
                                     Customers_Accounts.BalanceImport, Customers_Accounts.BalanceExport, Customers_Accounts.Reference_Type_View
            FROM            Customers_Accounts INNER JOIN
                                     Customer ON Customers_Accounts.Customer_Id = Customer.Id ";
            mysql = mysql + " WHERE      (Customers_Accounts.Customer_Id = " + Customer_Id + ") AND  (Customers_Accounts.Reference_Type <> 25)   ";
            mysql = mysql + "  union ";
            mysql = mysql + @" SELECT   Suppliers_Accounts.Id,Suppliers_Accounts.ProcessNo, Suppliers_Accounts.Insert_ServerDateTime, Suppliers_Accounts.Larg_Unit_Price,Suppliers_Accounts.Item_Name, Suppliers_Accounts.Larg_Qnty,Suppliers_Accounts.Small_Qnty,Suppliers_Accounts.Item_No,   Supplier.Ref_Cust_Supplier_Id, Suppliers_Accounts.Import, Suppliers_Accounts.Export, Suppliers_Accounts.Balance, Suppliers_Accounts.Balance_Actual, 
                                     Suppliers_Accounts.Transaction_Type, Suppliers_Accounts.Transaction_Date, Suppliers_Accounts.Reference_Id, Suppliers_Accounts.Reference_Type, Suppliers_Accounts.Reference_Type_Table_Id, 
                                     Suppliers_Accounts.Payment_Type, Suppliers_Accounts.Notes, Suppliers_Accounts.Day_Registration_flage, Suppliers_Accounts.DayReg_List_Id, Suppliers_Accounts.Flage_View, 
                                     Suppliers_Accounts.BalanceImport, Suppliers_Accounts.BalanceExport, Suppliers_Accounts.Reference_Type_View
            FROM            Suppliers_Accounts INNER JOIN
                                     Supplier ON Suppliers_Accounts.Supplier_Id = Supplier.Id ";
            mysql = mysql + "  WHERE       (Suppliers_Accounts.Supplier_Id = " + Supplier_Id + ")  AND  (Suppliers_Accounts.Reference_Type <> 25) AND (Supplier.Ref_Cust_Supplier_Id > 0)    ";



            opp.insert_me(mysql);
            decimal BalanceBefor = 0;
            decimal ImportBefor = 0;
            decimal ExportBefor = 0;
            try
            {
                bool IsFountBalanceBefor = false;
                int FirstRowToCompute = 0;
                if (_GetDateFrom == true)
                {
                    mysql = @"SELECT  SUM(Import)   AS Import, SUM(Export)   AS Export,  (SUM(Export) - SUM(Import)) AS Balance FROM Customers_Accounts_Mokhtalat ";


                    //mysql = mysql + " WHere  (Transaction_Date < '" + operations.dateFormat(GetDateFrom.Date) + "')    ";
                    mysql = mysql + " WHere (CONVERT(DATE, Transaction_Date)  < '" + operations.dateFormat(GetDateFrom.Date) + "') ";

                    DataTable dt = opp.return_DataTable(mysql);
                    if (dt.Rows.Count > 0)
                    {
                        IsFountBalanceBefor = true;
                        try
                        {
                            BalanceBefor = decimal.Parse(dt.Rows[0]["Balance"].ToString());
                            ImportBefor = decimal.Parse(dt.Rows[0]["Import"].ToString());
                            ExportBefor = decimal.Parse(dt.Rows[0]["Export"].ToString());
                        }
                        catch { BalanceBefor = 0; }
                    }
                }


                mysql = " SELECT Customers_Accounts_Mokhtalat.*,0 AS Serial_  FROM Customers_Accounts_Mokhtalat  WHERE Flage_View=1      ";
                if (_GetDateFrom == true)
                {
                    //mysql = mysql + " AND (Transaction_Date >= '" + operations.dateFormat(GetDateFrom.Date) + "')  ";
                    mysql = mysql + " AND (CONVERT(DATE, Transaction_Date)  >= '" + operations.dateFormat(GetDateFrom.Date) + "') ";

                }
                if (_GetDateTo == true)
                {
                    //mysql = mysql + "   AND (Transaction_Date <='" + operations.dateFormat(GetDateTo) + "' )  ";
                    mysql = mysql + " AND (CONVERT(DATE, Transaction_Date)  <= '" + operations.dateFormat(GetDateTo.Date) + "') ";
                }


                //mysql = mysql + "  ORDER BY  Transaction_Date ,ProcessNo ";//  Transaction_Date ,Id";
                mysql = mysql + " ORDER BY ProcessNo, Transaction_Date ";//  Transaction_Date ,Id";


                dtAccount = opp.return_DataTable(mysql);


                /////////////////////////////////////////////////////////////
                ///

                //////////////   ≈Ã„«·Ï «·Õ—ﬂ… Œ·«· › —… «·»ÕÀ
                decimal Import_Date = 0;
                decimal Export_Date = 0;
                for (int x = 0; x < dtAccount.Rows.Count; x++)
                {

                    try
                    {
                        Import_Date = Import_Date + decimal.Parse(dtAccount.Rows[x]["Import"].ToString());
                        Import_Date = Common.RoundMe(Import_Date);
                    }
                    catch { }

                    try
                    {
                        Export_Date = Export_Date + decimal.Parse(dtAccount.Rows[x]["Export"].ToString());
                        Export_Date = Common.RoundMe(Export_Date);
                    }
                    catch { }



                }


                ///////////////«·Õ’Ê· ⁄·Ï «Ã„«·Ï «·Õ—ﬂ«  «·”«»ﬁÂ Ê«÷«› Â« ⁄·Ï —’Ìœ «Ê· «·„œÂ
                decimal Final_First_Balance_Madin = 0;
                decimal Final_First_Balance_Da2n = 0;
                decimal First_Balance = BalanceBefor;


                //Õ”«» —’Ìœ √Ê· «·„œÂ
                decimal First_Balance_Madin = 0;
                decimal First_Balance_Da2n = 0;

                //Get_FirstBalance_Period(Year_Financial_Period_Id, Branch_Id, Submain_Account_ID, ref   First_Balance_Madin, ref   First_Balance_Da2n, ref Equivalent_First_Balance_Madin, ref Equivalent_First_Balance_Da2n);

                //≈÷«›… —’Ìœ «Ê· «·„œÂ ⁄·Ï «·—’Ìœ «·„‰ﬁÊ·
                //decimal First_Balance = (First_Balance_Madin - First_Balance_Da2n) + (Prev_Import_Value - Prev_Export_Value);
                if (First_Balance == 0)
                {
                    Final_First_Balance_Madin = 0;
                    Final_First_Balance_Da2n = 0;



                }
                else if (First_Balance > 0)
                {
                    Final_First_Balance_Madin = First_Balance;
                    Final_First_Balance_Da2n = 0;


                }
                else if (First_Balance < 0)
                {

                    Final_First_Balance_Madin = 0;
                    Final_First_Balance_Da2n = First_Balance * -1;


                }

                /////////////Õ”«» —’Ìœ «·Õ—ﬂ« 
                Set_ComputeBalanceNew(ref dtAccount, First_Balance);
                if (IsFountBalanceBefor == true)
                {
                    /////////////// ≈÷«›… —’Ìœ √Ê· «·„œÂ «Ê «·—’Ìœ «·„‰ﬁÊ·
                    DataRow dr11 = dtAccount.NewRow();
                    Final_First_Balance_Madin = Common.RoundMe(Final_First_Balance_Madin);
                    Final_First_Balance_Da2n = Common.RoundMe(Final_First_Balance_Da2n);
                    dr11["BalanceImport"] = Final_First_Balance_Madin;
                    dr11["BalanceExport"] = Final_First_Balance_Da2n;
                    decimal BalanceDiff1 = Final_First_Balance_Madin - Final_First_Balance_Da2n;

                    SetBalanceDiff(ref BalanceDiff1, ref dr11);



                    dr11["Balance"] = First_Balance;
                    //dr11["Notes"] = " "—’Ìœ √Ê· «·„œÂ"";

                    dr11["Import"] = ImportBefor;
                    dr11["Export"] = ExportBefor;
                    dr11["Reference_Type"] = 0;

                    dr11["Reference_Type_View"] = "—’Ìœ ”«»ﬁ";
                    dtAccount.Rows.InsertAt(dr11, 0);

                }


                Common.Make_TableColumnSerialME(ref dtAccount);

                //////////≈÷«›… «·«Ã„«·Ï
                decimal Import = 0;
                decimal Export = 0;

                decimal Balance_Import_Value = 0;
                decimal Balance_Export_Value = 0;

                //////////////  ≈Ã„«·Ï «·Õ—ﬂ…
                for (int x = 0; x < dtAccount.Rows.Count; x++)
                {

                    try
                    {
                        Import = Import + decimal.Parse(dtAccount.Rows[x]["Import"].ToString());
                        Import = Common.RoundMe(Import);
                    }
                    catch { }

                    try
                    {
                        Export = Export + decimal.Parse(dtAccount.Rows[x]["Export"].ToString());
                        Export = Common.RoundMe(Export);
                    }
                    catch { }



                }
                /////////////////////////////////////
                /// «÷«›… «Ã„«·Ï «·Õ—ﬂ… Œ·«· › —… «·»ÕÀ
                //   Import_Date = 0;
                //Export_Date = 0;
                //dr2["Notes"] = "—’Ìœ „‰ﬁÊ·";

                if (Common.IsShowTotalTransactionInSearchPeriod == 1)
                {
                    DataRow dr_ = dtAccount.NewRow();

                    dr_["Import"] = Import_Date;
                    dr_["Export"] = Export_Date;
                    dr_["Balance"] = 0;
                    dr_["Notes"] = "≈Ã„«·Ï «·Õ—ﬂ… ·› —… «·»ÕÀ";

                    dtAccount.Rows.Add(dr_);
                }
                ////////////////////////////////
                ///«÷«›… «Ã„«·Ï «·Õ—ﬂ…
                ///
                if (Common.IsShowTotalTransaction == 1)
                {
                    DataRow dr = dtAccount.NewRow();

                    dr["Import"] = Import;
                    dr["Export"] = Export;
                    dr["Balance"] = 0;
                    dr["Notes"] = "≈Ã„«·Ï «·Õ—ﬂÂ";

                    dtAccount.Rows.Add(dr);
                }
                //////////////////Õ”«» «·—’Ìœ «·„‰ﬁÊ·
                decimal Balance_Mankool = Export - Import;

                //Balance_Mankool = Balance_Mankool + First_Balance;
                Balance_Mankool = Common.RoundMe(Balance_Mankool);

                if (Balance_Mankool == 0)
                {
                    Final_First_Balance_Madin = 0;
                    Final_First_Balance_Da2n = 0;
                }
                else if (Balance_Mankool > 0)
                {
                    Final_First_Balance_Madin = Balance_Mankool;
                    Final_First_Balance_Da2n = 0;
                }
                else if (Balance_Mankool < 0)
                {

                    Final_First_Balance_Madin = 0;
                    Final_First_Balance_Da2n = Balance_Mankool * -1;
                }



                /////////////////////
                DataRow dr2 = dtAccount.NewRow();
                dr2["Balance"] = Balance_Mankool;
                dr2["BalanceImport"] = Final_First_Balance_Madin;
                dr2["BalanceExport"] = Final_First_Balance_Da2n;

                decimal BalanceDiff = Final_First_Balance_Madin - Final_First_Balance_Da2n;
                SetBalanceDiff(ref BalanceDiff, ref dr2);
                //string BalanceDiff_ = "";
                //if (BalanceDiff < 0)
                //{
                //    BalanceDiff = BalanceDiff * -1;
                //    BalanceDiff_ = "( " + BalanceDiff_.ToString() + " )";
                //}
                //dr2["BalanceDiff"] = BalanceDiff_;

                //dr2["Notes"] = "—’Ìœ „‰ﬁÊ·";
                dr2["Notes"] = Common.NotesBalanceLabelMankol;
                dtAccount.Rows.Add(dr2);
                return dtAccount;
            }
            catch (Exception ee)
            {
                // Excp = 1;

            }
            return dtAccount;
        }
        public DataTable Get_Customer_AccountComputeBalanceMokhtalat(long Customer_Id, long Supplier_Id, bool _GetDateFrom, DateTime GetDateFrom, bool _GetDateTo, DateTime GetDateTo, bool DESC)
        {


            DateTime dtBefor = GetDateFrom;

            TimeSpan ts = new TimeSpan(1, 0, 0, 0);

            dtBefor = dtBefor.Subtract(ts);
            string mysql = "";
            DataTable dtAccount = new DataTable();
            operations opp = new operations();

            bool IsFinancialPeriod = false ;
            GetCustomers_Accounts_Mokhtalat(ref opp, IsFinancialPeriod, Customer_Id, Supplier_Id);
             
            decimal BalanceBefor = 0;
            decimal ImportBefor = 0;
            decimal ExportBefor = 0;
            try
            {
                bool IsFountBalanceBefor = false;
                int FirstRowToCompute = 0;
                if (_GetDateFrom == true)
                {
                    mysql = @"SELECT  SUM(Import)   AS Import, SUM(Export)   AS Export,  (SUM(Export) - SUM(Import)) AS Balance FROM Customers_Accounts_Mokhtalat ";


                    //mysql = mysql + " WHere  (Transaction_Date < '" + operations.dateFormat(GetDateFrom.Date) + "')    ";
                    mysql = mysql + " WHere (CONVERT(DATE, Transaction_Date)  < '" + operations.dateFormat(GetDateFrom.Date) + "') ";

                    DataTable dt = opp.return_DataTable(mysql);
                    if (dt.Rows.Count > 0)
                    {
                        IsFountBalanceBefor = true;
                        try
                        {
                            BalanceBefor = decimal.Parse(dt.Rows[0]["Balance"].ToString());
                            ImportBefor = decimal.Parse(dt.Rows[0]["Import"].ToString());
                            ExportBefor = decimal.Parse(dt.Rows[0]["Export"].ToString());
                        }
                        catch { BalanceBefor = 0; }
                    }
                }


                mysql = " SELECT Customers_Accounts_Mokhtalat.*,0 AS Serial_  FROM Customers_Accounts_Mokhtalat  WHERE Flage_View=1      ";
                if (_GetDateFrom == true)
                {
                    //mysql = mysql + " AND (Transaction_Date >= '" + operations.dateFormat(GetDateFrom.Date) + "')  ";
                    mysql = mysql + " AND (CONVERT(DATE, Transaction_Date)  >= '" + operations.dateFormat(GetDateFrom.Date) + "') ";

                }
                if (_GetDateTo == true)
                {
                    //mysql = mysql + "   AND (Transaction_Date <='" + operations.dateFormat(GetDateTo) + "' )  ";
                    mysql = mysql + " AND (CONVERT(DATE, Transaction_Date)  <= '" + operations.dateFormat(GetDateTo.Date) + "') ";
                }


                //mysql = mysql + "  ORDER BY  Transaction_Date ,ProcessNo ";//  Transaction_Date ,Id";
                mysql = mysql + " ORDER BY   Transaction_Date ,ProcessNo ";//  Transaction_Date ,Id";


                dtAccount = opp.return_DataTable(mysql);


                /////////////////////////////////////////////////////////////
                ///

                //////////////   ≈Ã„«·Ï «·Õ—ﬂ… Œ·«· › —… «·»ÕÀ
                decimal Import_Date = 0;
                decimal Export_Date = 0;
                for (int x = 0; x < dtAccount.Rows.Count; x++)
                {

                    try
                    {
                        Import_Date = Import_Date + decimal.Parse(dtAccount.Rows[x]["Import"].ToString());
                        Import_Date = Common.RoundMe(Import_Date);
                    }
                    catch { }

                    try
                    {
                        Export_Date = Export_Date + decimal.Parse(dtAccount.Rows[x]["Export"].ToString());
                        Export_Date = Common.RoundMe(Export_Date);
                    }
                    catch { }



                }


                ///////////////«·Õ’Ê· ⁄·Ï «Ã„«·Ï «·Õ—ﬂ«  «·”«»ﬁÂ Ê«÷«› Â« ⁄·Ï —’Ìœ «Ê· «·„œÂ
                decimal Final_First_Balance_Madin = 0;
                decimal Final_First_Balance_Da2n = 0;
                decimal First_Balance = BalanceBefor;


                //Õ”«» —’Ìœ √Ê· «·„œÂ
                decimal First_Balance_Madin = 0;
                decimal First_Balance_Da2n = 0;

                //Get_FirstBalance_Period(Year_Financial_Period_Id, Branch_Id, Submain_Account_ID, ref   First_Balance_Madin, ref   First_Balance_Da2n, ref Equivalent_First_Balance_Madin, ref Equivalent_First_Balance_Da2n);

                //≈÷«›… —’Ìœ «Ê· «·„œÂ ⁄·Ï «·—’Ìœ «·„‰ﬁÊ·
                //decimal First_Balance = (First_Balance_Madin - First_Balance_Da2n) + (Prev_Import_Value - Prev_Export_Value);
                if (First_Balance == 0)
                {
                    Final_First_Balance_Madin = 0;
                    Final_First_Balance_Da2n = 0;



                }
                else if (First_Balance > 0)
                {
                    Final_First_Balance_Madin = First_Balance;
                    Final_First_Balance_Da2n = 0;


                }
                else if (First_Balance < 0)
                {

                    Final_First_Balance_Madin = 0;
                    Final_First_Balance_Da2n = First_Balance * -1;


                }

                /////////////Õ”«» —’Ìœ «·Õ—ﬂ« 
                Set_ComputeBalanceNew(ref dtAccount, First_Balance);
                if (IsFountBalanceBefor == true)
                {
                    /////////////// ≈÷«›… —’Ìœ √Ê· «·„œÂ «Ê «·—’Ìœ «·„‰ﬁÊ·
                    DataRow dr11 = dtAccount.NewRow();
                    Final_First_Balance_Madin = Common.RoundMe(Final_First_Balance_Madin);
                    Final_First_Balance_Da2n = Common.RoundMe(Final_First_Balance_Da2n);
                    dr11["BalanceImport"] = Final_First_Balance_Madin;
                    dr11["BalanceExport"] = Final_First_Balance_Da2n;
                    decimal BalanceDiff1 = Final_First_Balance_Madin - Final_First_Balance_Da2n;

                    SetBalanceDiff(ref BalanceDiff1, ref dr11);



                    dr11["Balance"] = First_Balance;
                    //dr11["Notes"] = " "—’Ìœ √Ê· «·„œÂ"";

                    dr11["Import"] = ImportBefor;
                    dr11["Export"] = ExportBefor;
                    dr11["Reference_Type"] = 0;

                    dr11["Reference_Type_View"] = "—’Ìœ ”«»ﬁ";
                    dtAccount.Rows.InsertAt(dr11, 0);

                }


                Common.Make_TableColumnSerialME(ref dtAccount);

                //////////≈÷«›… «·«Ã„«·Ï
                decimal Import = 0;
                decimal Export = 0;

                decimal Balance_Import_Value = 0;
                decimal Balance_Export_Value = 0;

                //////////////  ≈Ã„«·Ï «·Õ—ﬂ…
                for (int x = 0; x < dtAccount.Rows.Count; x++)
                {

                    try
                    {
                        Import = Import + decimal.Parse(dtAccount.Rows[x]["Import"].ToString());
                        Import = Common.RoundMe(Import);
                    }
                    catch { }

                    try
                    {
                        Export = Export + decimal.Parse(dtAccount.Rows[x]["Export"].ToString());
                        Export = Common.RoundMe(Export);
                    }
                    catch { }



                }
                /////////////////////////////////////
                /// «÷«›… «Ã„«·Ï «·Õ—ﬂ… Œ·«· › —… «·»ÕÀ
                //   Import_Date = 0;
                //Export_Date = 0;
                //dr2["Notes"] = "—’Ìœ „‰ﬁÊ·";

                if (Common.IsShowTotalTransactionInSearchPeriod == 1)
                {
                    DataRow dr_ = dtAccount.NewRow();

                    dr_["Import"] = Import_Date;
                    dr_["Export"] = Export_Date;
                    dr_["Balance"] = 0;
                    dr_["Notes"] = "≈Ã„«·Ï «·Õ—ﬂ… ·› —… «·»ÕÀ";

                    dtAccount.Rows.Add(dr_);
                }
                ////////////////////////////////
                ///«÷«›… «Ã„«·Ï «·Õ—ﬂ…
                ///
                if (Common.IsShowTotalTransaction == 1)
                {
                    DataRow dr = dtAccount.NewRow();

                    dr["Import"] = Import;
                    dr["Export"] = Export;
                    dr["Balance"] = 0;
                    dr["Notes"] = "≈Ã„«·Ï «·Õ—ﬂÂ";

                    dtAccount.Rows.Add(dr);
                }
                //////////////////Õ”«» «·—’Ìœ «·„‰ﬁÊ·
                decimal Balance_Mankool = Export - Import;

                //Balance_Mankool = Balance_Mankool + First_Balance;
                Balance_Mankool = Common.RoundMe(Balance_Mankool);

                if (Balance_Mankool == 0)
                {
                    Final_First_Balance_Madin = 0;
                    Final_First_Balance_Da2n = 0;
                }
                else if (Balance_Mankool > 0)
                {
                    Final_First_Balance_Madin = Balance_Mankool;
                    Final_First_Balance_Da2n = 0;
                }
                else if (Balance_Mankool < 0)
                {

                    Final_First_Balance_Madin = 0;
                    Final_First_Balance_Da2n = Balance_Mankool * -1;
                }



                /////////////////////
                DataRow dr2 = dtAccount.NewRow();
                dr2["Balance"] = Balance_Mankool;
                dr2["BalanceImport"] = Final_First_Balance_Madin;
                dr2["BalanceExport"] = Final_First_Balance_Da2n;

                dr2["Export"] = Final_First_Balance_Madin;
                dr2["Import"] = Final_First_Balance_Da2n;


                decimal BalanceDiff = Final_First_Balance_Madin - Final_First_Balance_Da2n;
                SetBalanceDiff(ref BalanceDiff, ref dr2);
                //string BalanceDiff_ = "";
                //if (BalanceDiff < 0)
                //{
                //    BalanceDiff = BalanceDiff * -1;
                //    BalanceDiff_ = "( " + BalanceDiff_.ToString() + " )";
                //}
                //dr2["BalanceDiff"] = BalanceDiff_;

                //dr2["Notes"] = "—’Ìœ „‰ﬁÊ·";
                dr2["Notes"] = Common.NotesBalanceLabelMankol;
                dtAccount.Rows.Add(dr2);
                return dtAccount;
            }
            catch (Exception ee)
            {
                // Excp = 1;

            }
            return dtAccount;
        }
        public DataTable Get_Customer_AccountComputeBalanceMokhtalatPeriod(long Customer_Id, long Supplier_Id, long Year_Financial_Period_Id, DateTime Financial_Period_dtFom, bool _GetDateFrom, DateTime GetDateFrom, bool _GetDateTo, DateTime GetDateTo, bool DESC)
        {

            DateTime dtBefor = GetDateFrom;

            TimeSpan ts = new TimeSpan(1, 0, 0, 0);

            dtBefor = dtBefor.Subtract(ts);
            string mysql = "";
            DataTable dtAccount = new DataTable();
            operations opp = new operations();
            bool IsFinancialPeriod = true;
            GetCustomers_Accounts_Mokhtalat(ref opp, IsFinancialPeriod, Customer_Id, Supplier_Id);

            decimal Prev_BalanceBefor = 0;
            decimal Prev_Import_Value = 0;
            decimal Prev_Export_Value = 0;

            decimal Final_First_Balance_Madin = 0;
            decimal Final_First_Balance_Da2n = 0;
            decimal First_Balance = 0;
            string Raseed_Awel_Moda = "—’Ìœ √Ê· «·„œÂ";
            try
            {
                bool IsFountBalanceBefor = false ;
                int FirstRowToCompute = 0;
                if (_GetDateFrom == true)
                {
                    //decimal Prev_Import_Value = 0;
                    //decimal Prev_Export_Value = 0;
                    //decimal Prev_BalanceBefor = 0;

                    if (GetDateFrom.Date > Financial_Period_dtFom)
                    {
                        //////////////Õ”«» «·› —… „‰  «—ÌŒ »œ«ÌÂ «·› —… «·„«·ÌÂ Õ Ï  »œ«Ì…  «—ÌŒ «·»ÕÀ  
                        mysql = @"SELECT  SUM(Import)   AS Import, SUM(Export)   AS Export,  (SUM(Export) - SUM(Import)) AS Balance FROM Customers_Accounts_Mokhtalat ";
                        mysql = mysql + " WHere (CONVERT(DATE, Transaction_Date)  >= '" + operations.dateFormat(Financial_Period_dtFom.Date) + "') AND  (CONVERT(DATE, Transaction_Date)  <  '" + operations.dateFormat(GetDateFrom.Date) + "') ";
                        mysql = mysql + " AND (Customer_Id = " + Customer_Id + ") And  Reference_Type <> -7 ";
                        

                        DataTable dt_Prev = opp.return_DataTable(mysql);

                      
                        try
                        {
                            if (dt_Prev.Rows.Count > 0)
                            {
                                Prev_BalanceBefor = decimal.Parse(dt_Prev.Rows[0]["Balance"].ToString());
                                Prev_Import_Value = decimal.Parse(dt_Prev.Rows[0]["Import"].ToString());
                                Prev_Export_Value = decimal.Parse(dt_Prev.Rows[0]["Export"].ToString());
                                IsFountBalanceBefor = true ;
                                Raseed_Awel_Moda = "—’Ìœ ”«»ﬁ";
                            }
                        }
                        catch { Prev_BalanceBefor = 0; }

                    }
                    long Branch_Id = Common.Branch_Id;
                    CustomerCLS customerCLS = new CustomerCLS(ref opp);
                    customerCLS.GetCustomerNoTracking(Customer_Id);
                    long Submain_Account_ID = customerCLS.cs.SubAccount_Id;
                    AccountEntry_CLS accountEntry_CLS= new AccountEntry_CLS();
                    decimal First_Balance_Madin = 0;
                    decimal First_Balance_Da2n =0;
                    decimal Equivalent_First_Balance_Madin_ = 0;
                    decimal Equivalent_First_Balance_Da2n_ =0;

 
                    accountEntry_CLS.Get_FirstBalance_Period(  Year_Financial_Period_Id,   Branch_Id,   Submain_Account_ID, ref   First_Balance_Madin, ref   First_Balance_Da2n, ref   Equivalent_First_Balance_Madin_, ref   Equivalent_First_Balance_Da2n_);

                    //≈÷«›… —’Ìœ «Ê· «·„œÂ ⁄·Ï «·—’Ìœ «·„‰ﬁÊ·
                      First_Balance = (First_Balance_Madin - First_Balance_Da2n) + (Prev_Import_Value - Prev_Export_Value);
                    if (First_Balance == 0)
                    {
                        Final_First_Balance_Madin = 0;
                        Final_First_Balance_Da2n = 0;



                    }
                    else if (First_Balance > 0)
                    {
                        Final_First_Balance_Madin = First_Balance;
                        Final_First_Balance_Da2n = 0;


                    }
                    else if (First_Balance < 0)
                    {

                        Final_First_Balance_Madin = 0;
                        Final_First_Balance_Da2n = First_Balance * -1;


                    }


                    //mysql = @"SELECT  SUM(Import)   AS Import, SUM(Export)   AS Export,  (SUM(Export) - SUM(Import)) AS Balance FROM Customers_Accounts_Mokhtalat ";
                    //mysql = mysql + " WHere (CONVERT(DATE, Transaction_Date)  < '" + operations.dateFormat(GetDateFrom.Date) + "') ";

                    //DataTable dt = opp.return_DataTable(mysql);
                    //if (dt.Rows.Count > 0)
                    //{
                    //    IsFountBalanceBefor = true;
                    //    try
                    //    {
                    //        BalanceBefor = decimal.Parse(dt.Rows[0]["Balance"].ToString());
                    //        ImportBefor = decimal.Parse(dt.Rows[0]["Import"].ToString());
                    //        ExportBefor = decimal.Parse(dt.Rows[0]["Export"].ToString());
                    //    }
                    //    catch { BalanceBefor = 0; }
                    //}
                }


                mysql = " SELECT Customers_Accounts_Mokhtalat.*,0 AS Serial_  FROM Customers_Accounts_Mokhtalat  WHERE Flage_View=1      ";
                if (_GetDateFrom == true)
                {
                    //mysql = mysql + " AND (Transaction_Date >= '" + operations.dateFormat(GetDateFrom.Date) + "')  ";
                    mysql = mysql + " AND (CONVERT(DATE, Transaction_Date)  >= '" + operations.dateFormat(GetDateFrom.Date) + "') ";

                }
                if (_GetDateTo == true)
                {
                    //mysql = mysql + "   AND (Transaction_Date <='" + operations.dateFormat(GetDateTo) + "' )  ";
                    mysql = mysql + " AND (CONVERT(DATE, Transaction_Date)  <= '" + operations.dateFormat(GetDateTo.Date) + "') ";
                }


                //mysql = mysql + "  ORDER BY  Transaction_Date ,ProcessNo ";//  Transaction_Date ,Id";
                mysql = mysql + " ORDER BY   Transaction_Date ,ProcessNo ";//  Transaction_Date ,Id";


                dtAccount = opp.return_DataTable(mysql);


                /////////////////////////////////////////////////////////////
                ///

                //////////////   ≈Ã„«·Ï «·Õ—ﬂ… Œ·«· › —… «·»ÕÀ
                decimal Import_Data = 0;
                decimal Export_Data = 0;
                for (int x = 0; x < dtAccount.Rows.Count; x++)
                {

                    try
                    {
                        Import_Data = Import_Data + decimal.Parse(dtAccount.Rows[x]["Import"].ToString());
                        Import_Data = Common.RoundMe(Import_Data);
                    }
                    catch { }

                    try
                    {
                        Export_Data = Export_Data + decimal.Parse(dtAccount.Rows[x]["Export"].ToString());
                        Export_Data = Common.RoundMe(Export_Data);
                    }
                    catch { }



                }
                 

                /////////////Õ”«» —’Ìœ «·Õ—ﬂ« 
                Set_ComputeBalanceNew(ref dtAccount, First_Balance);
                
                    /////////////// ≈÷«›… —’Ìœ √Ê· «·„œÂ «Ê «·—’Ìœ «·„‰ﬁÊ·
                    DataRow dr11 = dtAccount.NewRow();
                    Final_First_Balance_Madin = Common.RoundMe(Final_First_Balance_Madin);
                    Final_First_Balance_Da2n = Common.RoundMe(Final_First_Balance_Da2n);
                    dr11["BalanceImport"] = Final_First_Balance_Madin;
                    dr11["BalanceExport"] = Final_First_Balance_Da2n;
                    decimal BalanceDiff1 = Final_First_Balance_Madin - Final_First_Balance_Da2n;

                    SetBalanceDiff(ref BalanceDiff1, ref dr11); 

                    dr11["Balance"] = First_Balance;
                    //dr11["Notes"] = " "—’Ìœ √Ê· «·„œÂ"";

                    dr11["Import"] = Prev_Import_Value;
                    dr11["Export"] = Prev_Export_Value;
                    dr11["Reference_Type"] = 0;

                    dr11["Reference_Type_View"] = Raseed_Awel_Moda;
                    dtAccount.Rows.InsertAt(dr11, 0);

               


                Common.Make_TableColumnSerialME(ref dtAccount);

                //////////≈÷«›… «·«Ã„«·Ï
                decimal Import = 0;
                decimal Export = 0;

                decimal Balance_Import_Value = 0;
                decimal Balance_Export_Value = 0;

                //////////////  ≈Ã„«·Ï «·Õ—ﬂ…
                for (int x = 0; x < dtAccount.Rows.Count; x++)
                {

                    try
                    {
                        Import = Import + decimal.Parse(dtAccount.Rows[x]["Import"].ToString());
                        Import = Common.RoundMe(Import);
                    }
                    catch { }

                    try
                    {
                        Export = Export + decimal.Parse(dtAccount.Rows[x]["Export"].ToString());
                        Export = Common.RoundMe(Export);
                    }
                    catch { }



                }
                /////////////////////////////////////
                /// «÷«›… «Ã„«·Ï «·Õ—ﬂ… Œ·«· › —… «·»ÕÀ
                //   Import_Date = 0;
                //Export_Date = 0;
                //dr2["Notes"] = "—’Ìœ „‰ﬁÊ·";

                if (Common.IsShowTotalTransactionInSearchPeriod == 1)
                {
                    DataRow dr_ = dtAccount.NewRow();

                    dr_["Import"] = Import_Data;
                    dr_["Export"] = Export_Data;
                    dr_["Balance"] = 0;
                    dr_["Notes"] = "≈Ã„«·Ï «·Õ—ﬂ… ·› —… «·»ÕÀ";

                    dtAccount.Rows.Add(dr_);
                }
                ////////////////////////////////
                ///«÷«›… «Ã„«·Ï «·Õ—ﬂ…
                ///
                if (Common.IsShowTotalTransaction == 1)
                {
                    DataRow dr = dtAccount.NewRow();

                    dr["Import"] = Import;
                    dr["Export"] = Export;
                    dr["Balance"] = 0;
                    dr["Notes"] = "≈Ã„«·Ï «·Õ—ﬂÂ";

                    dtAccount.Rows.Add(dr);
                }
                //////////////////Õ”«» «·—’Ìœ «·„‰ﬁÊ·
                decimal Balance_Mankool = Export - Import;

                //Balance_Mankool = Balance_Mankool + First_Balance;
                Balance_Mankool = Common.RoundMe(Balance_Mankool);

                if (Balance_Mankool == 0)
                {
                    Final_First_Balance_Madin = 0;
                    Final_First_Balance_Da2n = 0;
                }
                else if (Balance_Mankool > 0)
                {
                    Final_First_Balance_Madin = Balance_Mankool;
                    Final_First_Balance_Da2n = 0;
                }
                else if (Balance_Mankool < 0)
                {

                    Final_First_Balance_Madin = 0;
                    Final_First_Balance_Da2n = Balance_Mankool * -1;
                }



                /////////////////////
                DataRow dr2 = dtAccount.NewRow();
                dr2["Balance"] = Balance_Mankool;
                dr2["BalanceImport"] = Final_First_Balance_Madin;
                dr2["BalanceExport"] = Final_First_Balance_Da2n;

                dr2["Export"] = Final_First_Balance_Madin;
                dr2["Import"] = Final_First_Balance_Da2n;


                decimal BalanceDiff = Final_First_Balance_Madin - Final_First_Balance_Da2n;
                SetBalanceDiff(ref BalanceDiff, ref dr2);
                //string BalanceDiff_ = "";
                //if (BalanceDiff < 0)
                //{
                //    BalanceDiff = BalanceDiff * -1;
                //    BalanceDiff_ = "( " + BalanceDiff_.ToString() + " )";
                //}
                //dr2["BalanceDiff"] = BalanceDiff_;

                //dr2["Notes"] = "—’Ìœ „‰ﬁÊ·";
                dr2["Notes"] = Common.NotesBalanceLabelMankol;
                dtAccount.Rows.Add(dr2);
                return dtAccount;
            }
            catch (Exception ee)
            {
                // Excp = 1;

            }
            return dtAccount;
        }

        public void  GetCustomers_Accounts_Mokhtalat(ref operations opp,bool IsFinancialPeriod,long Customer_Id,long Supplier_Id)
        {
            string mysql = "  truncate table Customers_Accounts_Mokhtalat";
            opp.insert_me(mysql);
            //CarsId,CarNumber,
            //Invoices_List.CarsId,Invoices_List.CarNumber, 

            mysql = @"  INSERT INTO Customers_Accounts_Mokhtalat
                         (Customers_AccountsId,ProcessNo,Insert_ServerDateTime ,Larg_Unit_Price,Item_Name,Larg_Qnty,Small_Qnty,Item_No, Customer_Id, Import, Export, Balance, Balance_Actual, Transaction_Type, Transaction_Date, Reference_Id, Reference_Type, Reference_Type_Table_Id, Payment_Type, Notes, Day_Registration_flage, DayReg_List_Id, 
                         Flage_View, BalanceImport, BalanceExport, Reference_Type_View,WorkOrderNumber, CustomerName  ,CarsWorkOrdersId, TaxInternalId,CarsId,CarNumber )
            SELECT    Customers_Accounts.Id, Customers_Accounts.ProcessNo, Customers_Accounts.Insert_ServerDateTime, Customers_Accounts.Larg_Unit_Price,Customers_Accounts.Item_Name,Customers_Accounts.Larg_Qnty,Customers_Accounts.Small_Qnty,Customers_Accounts.Item_No,  Customers_Accounts.Customer_Id, Customers_Accounts.Import, Customers_Accounts.Export, Customers_Accounts.Balance, Customers_Accounts.Balance_Actual, 
                                     Customers_Accounts.Transaction_Type, Customers_Accounts.Transaction_Date, Customers_Accounts.Reference_Id, Customers_Accounts.Reference_Type, Customers_Accounts.Reference_Type_Table_Id, 
                                     Customers_Accounts.Payment_Type, Customers_Accounts.Notes, Customers_Accounts.Day_Registration_flage, Customers_Accounts.DayReg_List_Id, Customers_Accounts.Flage_View, 
                                     Customers_Accounts.BalanceImport, Customers_Accounts.BalanceExport, Customers_Accounts.Reference_Type_View
                                    ,Customers_Accounts.WorkOrderNumber, Customers_Accounts.CustomerName  ,Customers_Accounts.CarsWorkOrdersId  ,Customers_Accounts.TaxInternalId,Customers_Accounts.CarsId,Customers_Accounts.CarNumber
            FROM            Customers_Accounts INNER JOIN
                                     Customer ON Customers_Accounts.Customer_Id = Customer.Id ";
            mysql = mysql + " WHERE      (Customers_Accounts.Customer_Id = " + Customer_Id + ") AND  (Customers_Accounts.Reference_Type <> 25)   ";
            if (IsFinancialPeriod == true)
            {
                mysql = mysql + " And  Reference_Type <> -7 ";
            }
            mysql = mysql + "  union ";
            mysql = mysql + @" SELECT   Suppliers_Accounts.Id,Suppliers_Accounts.ProcessNo, Suppliers_Accounts.Insert_ServerDateTime, Suppliers_Accounts.Larg_Unit_Price,Suppliers_Accounts.Item_Name, Suppliers_Accounts.Larg_Qnty,Suppliers_Accounts.Small_Qnty,Suppliers_Accounts.Item_No,   Supplier.Ref_Cust_Supplier_Id, Suppliers_Accounts.Import, Suppliers_Accounts.Export, Suppliers_Accounts.Balance, Suppliers_Accounts.Balance_Actual, 
                                     Suppliers_Accounts.Transaction_Type, Suppliers_Accounts.Transaction_Date, Suppliers_Accounts.Reference_Id, Suppliers_Accounts.Reference_Type, Suppliers_Accounts.Reference_Type_Table_Id, 
                                     Suppliers_Accounts.Payment_Type, Suppliers_Accounts.Notes, Suppliers_Accounts.Day_Registration_flage, Suppliers_Accounts.DayReg_List_Id, Suppliers_Accounts.Flage_View, 
                                     Suppliers_Accounts.BalanceImport, Suppliers_Accounts.BalanceExport, Suppliers_Accounts.Reference_Type_View
                                       ,Suppliers_Accounts.WorkOrderNumber, Suppliers_Accounts.SupplierName  ,Suppliers_Accounts.CarsWorkOrdersId ,Suppliers_Accounts.TaxInternalId,Suppliers_Accounts.CarsId,Suppliers_Accounts.CarNumber
            FROM            Suppliers_Accounts INNER JOIN
                                     Supplier ON Suppliers_Accounts.Supplier_Id = Supplier.Id ";
            mysql = mysql + "  WHERE       (Suppliers_Accounts.Supplier_Id = " + Supplier_Id + ")  AND  (Suppliers_Accounts.Reference_Type <> 25) AND (Supplier.Ref_Cust_Supplier_Id > 0)    ";
            if (IsFinancialPeriod == true)
            {
                mysql = mysql + " And  Reference_Type <> -7 ";
            }


            opp.insert_me(mysql);
        }
        void Set_ComputeBalanceNew(ref DataTable dt, decimal First_Balance)
        {
            decimal Final_First_Balance_Madin = 0;
            decimal Final_First_Balance_Da2n = 0;


            decimal Import = 0;
            decimal Export = 0;

            decimal Balance = First_Balance;

            for (int x = 0; x < dt.Rows.Count; x++)
            {
                Import = 0;
                Export = 0;

                try
                {
                    Import = Import + decimal.Parse(dt.Rows[x]["Import"].ToString());
                    Import = Common.RoundMe(Import);
                }
                catch { }
                try
                {
                    Export = Export + decimal.Parse(dt.Rows[x]["Export"].ToString());
                    Export = Common.RoundMe(Export);
                }
                catch { }




                Balance = Balance + (Export - Import);
                Balance = Common.RoundMe(Balance);
                dt.Rows[x]["Balance"] = Balance;

                if (Balance == 0)
                {
                    Final_First_Balance_Madin = 0;
                    Final_First_Balance_Da2n = 0;
                }
                else if (Balance > 0)
                {
                    Final_First_Balance_Madin = Balance;
                    Final_First_Balance_Da2n = 0;
                }
                else if (Balance < 0)
                {

                    Final_First_Balance_Madin = 0;
                    Final_First_Balance_Da2n = Balance * -1;
                }


                dt.Rows[x]["BalanceImport"] = Common.GetValuesAfterSetArabic(Final_First_Balance_Madin);
                dt.Rows[x]["BalanceExport"] = Common.GetValuesAfterSetArabic(Final_First_Balance_Da2n);
                decimal BalanceDiff = Final_First_Balance_Madin - Final_First_Balance_Da2n;
                SetBalanceDiff(ref BalanceDiff, ref dt, x);


            }
        }
        void SetBalanceDiff(ref decimal BalanceDiff, ref DataTable dt, int x)
        {
            dt.Rows[x]["BalanceDiff"] = BalanceDiff;
            //string BalanceDiff_ = "";
            //if (BalanceDiff < 0)
            //{
            //    BalanceDiff = BalanceDiff * -1;
            //    BalanceDiff_ = "( " + BalanceDiff_.ToString() + " )";
            //}
            //if (BalanceDiff_.Trim() == "")
            //{
            //    dt.Rows[x]["BalanceDiff"] = "";
            //}
            //else
            //{
            //    dt.Rows[x]["BalanceDiff"] = Common.GetValuesAfterSetArabic(BalanceDiff_);
            //}
        }

        void SetBalanceDiff(ref decimal BalanceDiff, ref DataRow dr)
        {
            dr["BalanceDiff"] = BalanceDiff;
            //dt.Rows[x]["BalanceDiff"] = BalanceDiff;
            //string BalanceDiff_1 = "";
            //if (BalanceDiff1 < 0)
            //{
            //    BalanceDiff1 = BalanceDiff1 * -1;
            //    BalanceDiff_1 = "( " + BalanceDiff_1.ToString() + " )";
            //}
            //dr11["BalanceDiff"] = BalanceDiff_1;
        }
        string SetBalanceDiffValue(decimal BalanceDiff)
        {


            string BalanceDiff_ = "";
            if (BalanceDiff < 0)
            {
                BalanceDiff = BalanceDiff * -1;
                BalanceDiff_ = "( " + BalanceDiff.ToString() + " )";
            }
            return BalanceDiff_;
        }
        public void Set_Customer_BalanceTotal(ref int Excp, int Customer_Id, long New_Invoice_ListTable_id, decimal Net_Total)
        {
            try
            {
                operations opp = new operations();
                Net_Total = Common.RoundMe(Net_Total);
                string str = "";
                ArrayList list = new ArrayList();
                list.Clear();
                string[] textArray1 = new string[] { "  UPDATE Customer SET Balance = Balance + ", Net_Total.ToString(), " ,Balance_Actual=Balance_Actual +  ", Net_Total.ToString(), " WHERE (Id = ", Customer_Id.ToString(), ")" };
                list.Add(string.Concat(textArray1));

                //UpdateProcessNo(ref Excp, ref opp, "Invoices_List", New_Invoice_ListTable_id);

                str = " INSERT INTO Customers_Accounts (ProcessNo,Customer_Id, Reference_Id,Reference_Type_Table_Id, Export, Balance, Transaction_Type, Transaction_Date,Reference_Type) " + " SELECT Invoices_List.ProcessNo,Invoices_List.Customer_Id, Invoices_List.Invoice_Id,Invoices_List.id, Invoices_List.Net_Total, Customer.Balance, 2 AS Transaction_Type, Invoices_List.Invoice_Date ,1 as Reference_Type" + " FROM Customer INNER JOIN  Invoices_List ON Customer.Id = Invoices_List.Customer_Id ";
                string[] textArray2 = new string[] { str, " WHERE (Customer.Id = ", Customer_Id.ToString(), ") AND (Invoices_List.id = ", New_Invoice_ListTable_id.ToString(), ") " };
                list.Add(string.Concat(textArray2));
                list.Add("UPDATE Invoices_List SET Customer_BalanceSet = 1,Editable=0 WHERE (id = " + New_Invoice_ListTable_id.ToString() + ") ");
                opp.insert_me_transaction(ref Excp, 1, list);
                Excp = 0;
            }
            catch (Exception exception)
            {
                Excp = 1;
                throw exception;
            }
        }
        public void Set_Customer_Balance_Invoice(ref int Excp, ref operations opp, int Customer_Id, long New_Invoice_ListTable_id, decimal Net_Total, string Notes)
        {
            //CarsId,CarNumber,
            //Invoices_List.CarsId,Invoices_List.CarNumber, 
            //if (Common.IsHavePermissionSysMenu(Common.User_Id, 342, Common.Project_ID, Common.Is_Force_Admin_Permission) | (Common.Is_Set_Customer_Balance_With_Items == 1))
            try
            {
                if (Common.Is_Set_Customer_Balance_With_Sum_Items == 1)
                {
                    this.Set_Customer_Balance_With_Sum_Items_Invoices_List(ref Excp, ref opp, Customer_Id, New_Invoice_ListTable_id, Net_Total, Notes);
                }

                else if (Common.Is_Set_Customer_Balance_With_Items == 1)
                {
                    this.Set_Customer_Balance_WithItems_Invoices_List(ref Excp, ref opp, Customer_Id, New_Invoice_ListTable_id, Net_Total, Notes);
                }

                else
                {
                    this.Set_Customer_Balance_(ref Excp, ref opp, Customer_Id, New_Invoice_ListTable_id, Net_Total, Notes);
                }
            }
            catch (Exception ee)
            {
                Excp = 1;

                throw;
            }
        }
        public void Set_Customer_Balance_Invoice(ref int Excp, ref operations opp, int Customer_Id, long Customer_Id_RelatedToAccount, long New_Invoice_ListTable_id, decimal Net_Total, string Notes)
        {
            if (Customer_Id_RelatedToAccount == 0)
            {
                Customer_Id_RelatedToAccount = Customer_Id;
            }

            //CarsId,CarNumber,
            //Invoices_List.CarsId,Invoices_List.CarNumber, 
            //if (Common.IsHavePermissionSysMenu(Common.User_Id, 342, Common.Project_ID, Common.Is_Force_Admin_Permission) | (Common.Is_Set_Customer_Balance_With_Items == 1))
            try
            {
                if (Common.Is_Set_Customer_Balance_With_Sum_Items == 1)
                {
                    this.Set_Customer_Balance_With_Sum_Items_Invoices_List_RelatedTo(ref Excp, ref opp, Customer_Id, Customer_Id_RelatedToAccount, New_Invoice_ListTable_id, Net_Total, Notes);
                }

                else if (Common.Is_Set_Customer_Balance_With_Items == 1)
                {
                    this.Set_Customer_Balance_WithItems_Invoices_List_RelatedTo(ref Excp, ref opp, Customer_Id, Customer_Id_RelatedToAccount, New_Invoice_ListTable_id, Net_Total, Notes);
                }

                else
                {
                    this.Set_Customer_Balance_RelatedTo(ref Excp, ref opp, Customer_Id, Customer_Id_RelatedToAccount, New_Invoice_ListTable_id, Net_Total, Notes);
                }
            }
            catch (Exception ee)
            {
                Excp = 1;

                throw;
            }
        }

        private void Set_Customer_Balance_WithItems_Invoices_List(ref int Excp, ref operations opp, int Customer_Id, long New_Invoice_ListTable_id, decimal Net_Total, string Notes)
        {
            long Customer_Id_RelatedToAccount = GetCustomer_Id_RelatedToAccount(ref opp, Customer_Id);
            bool Is_SumItemQnty = false;
            string str = "";
            str = "  Delete from  Customers_Accounts Where Reference_Type_Table_Id=" + New_Invoice_ListTable_id.ToString() + " and Reference_Type=" + Common.Invoice_Type_Id.ToString();
            opp.insert_me(str);
            Net_Total = Common.RoundMe(Net_Total);
            string[] textArray1 = new string[] { "  UPDATE Customer SET Balance = Balance + ", Net_Total.ToString(), " ,Balance_Actual=Balance_Actual +  ", Net_Total.ToString(), " WHERE (Id = ", Customer_Id.ToString(), ")" };
            str = string.Concat(textArray1);
            opp.insert_me(str);
            //Larg_Unit_Price,Item_Name,
            //Invoices_Details.Larg_Unit_Price,Item.Item_Name,



            //UpdateProcessNo(ref Excp, ref opp, "Invoices_List", New_Invoice_ListTable_id);



            str = @" INSERT INTO Customers_Accounts ( CarsId,CarNumber,CustomerName, CarsWorkOrdersId, WorkOrderNumber,TaxInternalId,ProcessNo,Larg_Unit_Price,Item_Id,Item_Name,Notes, Customer_Id, Customer_Id_RelatedToAccount,Reference_Id, Reference_Type_Table_Id, Export, Balance, Transaction_Type, Transaction_Date, Reference_Type, Larg_Qnty,Small_Qnty, Item_No, Item_Size, Qnty_Size,Qnty_value,Height_Width_Item) 
                 SELECT Invoices_List.CarsId,Invoices_List.CarNumber, Invoices_List.CustomerName, Invoices_List.CarsWorkOrdersId, Invoices_List.WorkOrderNumber,Invoices_List.TaxInternalId, Invoices_List.ProcessNo, Invoices_Details.Larg_Unit_Price,Invoices_Details.Item_Id,Item.Item_Name, Invoices_Details.NameSpecification," + Customer_Id_RelatedToAccount + ", Invoices_List.Customer_Id, Invoices_List.Invoice_Id, Invoices_List.ID, Invoices_Details.Qnty_value, Customer.Balance, 2 AS Transaction_Type, Invoices_List.Invoice_Date,   1 AS Reference_Type, Invoices_Details.Larg_Qnty,Invoices_Details.Small_Qnty, Invoices_Details.Qnty_Another, Item.Height_Width_Str, CONVERT(nvarchar(100), Invoices_Details.QntyWidth)  + N'\' + CONVERT(nvarchar(100), Invoices_Details.QntyLenght) AS Qnty_Size,  Invoices_Details.Qnty_value, CONVERT(nvarchar(100), Item.Width_) + N'/' + CONVERT(nvarchar(100), Item.Height_) AS Height_Width_Item   FROM Customer INNER JOIN  Invoices_List ON Customer.Id = Invoices_List.Customer_Id INNER JOIN  Invoices_Details ON Invoices_List.ID = Invoices_Details.Invoice_Id INNER JOIN  Item ON Invoices_Details.Item_Id = Item.id ";
            str = str + " WHERE (Customer.Id = " + Customer_Id.ToString() + ") AND (Invoices_List.id = " + New_Invoice_ListTable_id.ToString() + ") ";
            //}
            int C = opp.insert_me(str);
            if (C > 0)
            {
                str = "UPDATE Invoices_List SET Customer_BalanceSet = 1,Editable=0 WHERE (id = " + New_Invoice_ListTable_id.ToString() + ") ";
                opp.insert_me(str);

                SetCustomers_Accounts_With_Invoices_ListData(ref opp, New_Invoice_ListTable_id);
            }
        }
        private void Set_Customer_Balance_With_Sum_Items_Invoices_List(ref int Excp, ref operations opp, int Customer_Id, long New_Invoice_ListTable_id, decimal Net_Total, string Notes)
        {
            try
            {

                long Customer_Id_RelatedToAccount = GetCustomer_Id_RelatedToAccount(ref opp, Customer_Id);
                string str = "";
                str = "  Delete from  Customers_Accounts Where Reference_Type_Table_Id=" + New_Invoice_ListTable_id.ToString() + " and Reference_Type=" + Common.Invoice_Type_Id.ToString();
                opp.insert_me(str);
                Net_Total = Common.RoundMe(Net_Total);
                string[] textArray1 = new string[] { "  UPDATE Customer SET Balance = Balance + ", Net_Total.ToString(), " ,Balance_Actual=Balance_Actual +  ", Net_Total.ToString(), " WHERE (Id = ", Customer_Id.ToString(), ")" };
                str = string.Concat(textArray1);
                opp.insert_me(str);
                //Larg_Unit_Price,Item_Name,
                //Invoices_Details.Larg_Unit_Price,Item.Item_Name,

              //UpdateProcessNo(ref Excp, ref opp, "Invoices_List", New_Invoice_ListTable_id);


                str = @" INSERT INTO Customers_Accounts (  CarsId,CarNumber,CustomerName, CarsWorkOrdersId, WorkOrderNumber,TaxInternalId,ProcessNo,Larg_Unit_Price,Item_Id,Item_Name,Notes, Customer_Id,Customer_Id_RelatedToAccount, Reference_Id, Reference_Type_Table_Id, Export, Balance, Transaction_Type, Transaction_Date, Reference_Type, Larg_Qnty,Small_Qnty, Item_No, Qnty_value) ";
                str = str + " SELECT Invoices_List.CarsId,Invoices_List.CarNumber, Invoices_List.CustomerName,  Invoices_List.CarsWorkOrdersId, Invoices_List.WorkOrderNumber, Invoices_List.TaxInternalId,Invoices_List.ProcessNo,Invoices_Details.Larg_Unit_Price, Invoices_Details.Item_Id, Item.Item_Name, Invoices_Details.NameSpecification  , Invoices_List.Customer_Id," + Customer_Id_RelatedToAccount + ", Invoices_List.Invoice_Id, Invoices_List.ID, SUM(Invoices_Details.Qnty_value) AS Total ,  AVG(Customer.Balance) AS Expr3, 2 AS Transaction_Type, Invoices_List.Invoice_Date, 1 AS Reference_Type, SUM(Invoices_Details.Larg_Qnty) AS Larg_Qnty, SUM(Invoices_Details.Small_Qnty) AS Small_Qnty,  SUM(Invoices_Details.Qnty_Another) AS Qnty_Another, SUM(Invoices_Details.Qnty_value) AS Qnty_value ";
                str = str + " FROM Customer INNER JOIN Invoices_List ON Customer.Id = Invoices_List.Customer_Id INNER JOIN Invoices_Details ON Invoices_List.ID = Invoices_Details.Invoice_Id INNER JOIN Item ON Invoices_Details.Item_Id = Item.id ";
                str = str + " WHERE (Customer.Id = " + Customer_Id.ToString() + ") AND (Invoices_List.id = " + New_Invoice_ListTable_id.ToString() + ") ";
                str = str + " GROUP BY Invoices_List.CarsWorkOrdersId, Invoices_List.WorkOrderNumber, Invoices_List.ProcessNo, Invoices_Details.Larg_Unit_Price, Invoices_Details.Item_Id, Item.Item_Name,  Invoices_List.Customer_Id_RelatedToAccount , Invoices_List.Customer_Id, Invoices_List.Invoice_Id,  Invoices_List.ID, Invoices_List.Invoice_Date, Invoices_Details.NameSpecification, Invoices_List.CarsId, Invoices_List.CarNumber, Invoices_List.CustomerName, Invoices_List.TaxInternalId ";
                int C = opp.insert_me(str);
                if (C > 0)
                {
                    str = "UPDATE Invoices_List SET Customer_BalanceSet = 1,Editable=0 WHERE (id = " + New_Invoice_ListTable_id.ToString() + ") ";
                    opp.insert_me(str);

                    SetCustomers_Accounts_With_Invoices_ListData(ref opp, New_Invoice_ListTable_id);
                }
            }
            catch (Exception)
            {


            }

        }
        private void Set_Customer_Balance_(ref int Excp, ref operations opp, int Customer_Id, long New_Invoice_ListTable_id, decimal Net_Total, string Notes)
        {
            long Customer_Id_RelatedToAccount = GetCustomer_Id_RelatedToAccount(ref opp, Customer_Id);
            string str = "";
            str = "  Delete from  Customers_Accounts Where Reference_Type_Table_Id=" + New_Invoice_ListTable_id.ToString() + " and Reference_Type=" + Common.Invoice_Type_Id.ToString();
            opp.insert_me(str);
            Net_Total = Common.RoundMe(Net_Total);

            string ccc = Net_Total.GetType().FullName;

           ccc=
    "Value      = " + Net_Total.ToString() + Environment.NewLine +
    "Invariant  = " + Net_Total.ToString(System.Globalization.CultureInfo.InvariantCulture) + Environment.NewLine +
    "SQL String = " + str ;


    //        var dec = (System.Globalization.NumberFormatInfo)
    //System.Globalization.CultureInfo.CurrentCulture.NumberFormat.Clone();

    //        ccc=
    //            "Culture = " + System.Globalization.CultureInfo.CurrentCulture.DisplayName
    //            + "\nDecimalSeparator = [" + dec.NumberDecimalSeparator + "]"
    //            + "\nGroupSeparator = [" + dec.NumberGroupSeparator + "]";

    //        ccc = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
      

            str = "  UPDATE Customer SET Balance = Balance + " + Net_Total + " ,Balance_Actual=Balance_Actual +  " + Net_Total  + " WHERE (Id = " + Customer_Id + ")";
            opp.insert_me(str);

            //UpdateProcessNo(ref Excp, ref opp, "Invoices_List", New_Invoice_ListTable_id);

            //CarsWorkOrdersId, WorkOrderNumber,
            //    Invoices_List.CarsWorkOrdersId, Invoices_List.WorkOrderNumber,



            str = " INSERT INTO Customers_Accounts ( CarsId,CarNumber,CustomerName, CarsWorkOrdersId, WorkOrderNumber,TaxInternalId,ProcessNo,Notes,Customer_Id,Customer_Id_RelatedToAccount, Reference_Id,Reference_Type_Table_Id, Export, Balance, Transaction_Type, Transaction_Date,Reference_Type) ";
            str = str + " SELECT  Invoices_List.CarsId,Invoices_List.CarNumber,Invoices_List.CustomerName, Invoices_List.CarsWorkOrdersId, Invoices_List.WorkOrderNumber, Invoices_List.TaxInternalId,Invoices_List.ProcessNo,N'" + Notes + "' ," + Customer_Id_RelatedToAccount + ",Invoices_List.Customer_Id, Invoices_List.Invoice_Id,Invoices_List.id, Invoices_List.Net_Total, Customer.Balance, 2 AS Transaction_Type, Invoices_List.Invoice_Date ,1 as Reference_Type   FROM Customer INNER JOIN  Invoices_List ON Customer.Id = Invoices_List.Customer_Id ";
            str = str + " WHERE (Customer.Id = " + Customer_Id.ToString() + ") AND (Invoices_List.id = " + New_Invoice_ListTable_id.ToString() + ") ";

            int C = opp.insert_me(str);
            if (C > 0)
            {
                str = "UPDATE Invoices_List SET Customer_BalanceSet = 1,Editable=0 WHERE (id = " + New_Invoice_ListTable_id.ToString() + ") ";
                opp.insert_me(str);
            }
        }
        private void Set_Customer_Balance_RelatedTo(ref int Excp, ref operations opp, long Customer_Id, long Customer_Id_RelatedToAccount, long New_Invoice_ListTable_id, decimal Net_Total, string Notes)
        {
            string str = "";
            str = "  Delete from  Customers_Accounts Where Reference_Type_Table_Id=" + New_Invoice_ListTable_id.ToString() + " and Reference_Type=" + Common.Invoice_Type_Id.ToString();
            opp.insert_me(str);
            Net_Total = Common.RoundMe(Net_Total);

            //string[] textArray1 = new string[] { "  UPDATE Customer SET Balance = Balance + ", Net_Total.ToString(), " ,Balance_Actual=Balance_Actual +  ", Net_Total.ToString(), " WHERE (Id = ", Customer_Id.ToString(), ")" };
            //str = string.Concat(textArray1);
            //opp.insert_me(str);

            //UpdateProcessNo(ref Excp, ref opp, "Invoices_List", New_Invoice_ListTable_id);

            //CarsWorkOrdersId, WorkOrderNumber,
            //    Invoices_List.CarsWorkOrdersId, Invoices_List.WorkOrderNumber,



            str = " INSERT INTO Customers_Accounts ( CarsId,CarNumber,CustomerName, CarsWorkOrdersId, WorkOrderNumber,TaxInternalId,ProcessNo,Notes,Customer_Id, Customer_Id_RelatedToAccount,Reference_Id,Reference_Type_Table_Id, Export, Balance, Transaction_Type, Transaction_Date,Reference_Type) ";
            str = str + " SELECT  Invoices_List.CarsId,Invoices_List.CarNumber,Invoices_List.CustomerName, Invoices_List.CarsWorkOrdersId, Invoices_List.WorkOrderNumber, Invoices_List.TaxInternalId,Invoices_List.ProcessNo,N'" + Notes + "' ," + Customer_Id_RelatedToAccount + ",Invoices_List.Customer_Id, Invoices_List.Invoice_Id,Invoices_List.id, Invoices_List.Net_Total, Customer.Balance, 2 AS Transaction_Type, Invoices_List.Invoice_Date ,1 as Reference_Type   FROM Customer INNER JOIN  Invoices_List ON Customer.Id = Invoices_List.Customer_Id ";
            str = str + " WHERE (Customer.Id = " + Customer_Id.ToString() + ") AND (Invoices_List.id = " + New_Invoice_ListTable_id.ToString() + ") ";

            int C = opp.insert_me(str);
            if (C > 0)
            {
                str = "UPDATE Invoices_List SET Customer_BalanceSet = 1,Editable=0 WHERE (id = " + New_Invoice_ListTable_id.ToString() + ") ";
                opp.insert_me(str);
            }
        }
        private void Set_Customer_Balance_WithItems_Invoices_List_RelatedTo(ref int Excp, ref operations opp, int Customer_Id, long Customer_Id_RelatedToAccount, long New_Invoice_ListTable_id, decimal Net_Total, string Notes)
        {
            bool Is_SumItemQnty = false;
            string str = "";
            str = "  Delete from  Customers_Accounts Where Reference_Type_Table_Id=" + New_Invoice_ListTable_id.ToString() + " and Reference_Type=" + Common.Invoice_Type_Id.ToString();
            opp.insert_me(str);
            Net_Total = Common.RoundMe(Net_Total);
            //string[] textArray1 = new string[] { "  UPDATE Customer SET Balance = Balance + ", Net_Total.ToString(), " ,Balance_Actual=Balance_Actual +  ", Net_Total.ToString(), " WHERE (Id = ", Customer_Id.ToString(), ")" };
            //str = string.Concat(textArray1);
            //opp.insert_me(str);
            //Larg_Unit_Price,Item_Name,
            //Invoices_Details.Larg_Unit_Price,Item.Item_Name,



            //UpdateProcessNo(ref Excp, ref opp, "Invoices_List", New_Invoice_ListTable_id);



            str = @" INSERT INTO Customers_Accounts ( CarsId,CarNumber,CustomerName, CarsWorkOrdersId, WorkOrderNumber,TaxInternalId,ProcessNo,Larg_Unit_Price,Item_Id,Item_Name,Notes, Customer_Id,Customer_Id_RelatedToAccount, Reference_Id, Reference_Type_Table_Id, Export, Balance, Transaction_Type, Transaction_Date, Reference_Type, Larg_Qnty,Small_Qnty, Item_No, Item_Size, Qnty_Size,Qnty_value,Height_Width_Item) 
                 SELECT Invoices_List.CarsId,Invoices_List.CarNumber, Invoices_List.CustomerName, Invoices_List.CarsWorkOrdersId, Invoices_List.WorkOrderNumber,Invoices_List.TaxInternalId, Invoices_List.ProcessNo, Invoices_Details.Larg_Unit_Price,Invoices_Details.Item_Id,Item.Item_Name, Invoices_Details.NameSpecification,  " + Customer_Id_RelatedToAccount + ",Invoices_List.Customer_Id, Invoices_List.Invoice_Id, Invoices_List.ID, Invoices_Details.Qnty_value, Customer.Balance, 2 AS Transaction_Type, Invoices_List.Invoice_Date,   1 AS Reference_Type, Invoices_Details.Larg_Qnty,Invoices_Details.Small_Qnty, Invoices_Details.Qnty_Another, Item.Height_Width_Str, CONVERT(nvarchar(100), Invoices_Details.QntyWidth)  + N'\' + CONVERT(nvarchar(100), Invoices_Details.QntyLenght) AS Qnty_Size,  Invoices_Details.Qnty_value, CONVERT(nvarchar(100), Item.Width_) + N'/' + CONVERT(nvarchar(100), Item.Height_) AS Height_Width_Item   FROM Customer INNER JOIN  Invoices_List ON Customer.Id = Invoices_List.Customer_Id INNER JOIN  Invoices_Details ON Invoices_List.ID = Invoices_Details.Invoice_Id INNER JOIN  Item ON Invoices_Details.Item_Id = Item.id ";
            str = str + " WHERE (Customer.Id = " + Customer_Id.ToString() + ") AND (Invoices_List.id = " + New_Invoice_ListTable_id.ToString() + ") ";
            //}
            int C = opp.insert_me(str);
            if (C > 0)
            {
                str = "UPDATE Invoices_List SET Customer_BalanceSet = 1,Editable=0 WHERE (id = " + New_Invoice_ListTable_id.ToString() + ") ";
                opp.insert_me(str);

                SetCustomers_Accounts_With_Invoices_ListData(ref opp, New_Invoice_ListTable_id);
            }
        }

        private void Set_Customer_Balance_With_Sum_Items_Invoices_List_RelatedTo(ref int Excp, ref operations opp, int Customer_Id, long Customer_Id_RelatedToAccount, long New_Invoice_ListTable_id, decimal Net_Total, string Notes)
        {


            string str = "";
            str = "  Delete from  Customers_Accounts Where Reference_Type_Table_Id=" + New_Invoice_ListTable_id.ToString() + " and Reference_Type=" + Common.Invoice_Type_Id.ToString();
            opp.insert_me(str);
            Net_Total = Common.RoundMe(Net_Total);
            string[] textArray1 = new string[] { "  UPDATE Customer SET Balance = Balance + ", Net_Total.ToString(), " ,Balance_Actual=Balance_Actual +  ", Net_Total.ToString(), " WHERE (Id = ", Customer_Id.ToString(), ")" };
            str = string.Concat(textArray1);
            opp.insert_me(str);
            //Larg_Unit_Price,Item_Name,
            //Invoices_Details.Larg_Unit_Price,Item.Item_Name,

            //UpdateProcessNo(ref Excp, ref opp, "Invoices_List", New_Invoice_ListTable_id);


            str = @" INSERT INTO Customers_Accounts (  CarsId,CarNumber,CustomerName, CarsWorkOrdersId, WorkOrderNumber,TaxInternalId,ProcessNo,Larg_Unit_Price,Item_Id,Item_Name,Notes, Customer_Id,Customer_Id_RelatedToAccount, Reference_Id, Reference_Type_Table_Id, Export, Balance, Transaction_Type, Transaction_Date, Reference_Type, Larg_Qnty,Small_Qnty, Item_No, Qnty_value) 
                SELECT Invoices_List.CarsId,Invoices_List.CarNumber, Invoices_List.CustomerName,  Invoices_List.CarsWorkOrdersId, Invoices_List.WorkOrderNumber, Invoices_List.TaxInternalId,Invoices_List.ProcessNo,Invoices_Details.Larg_Unit_Price, Invoices_Details.Item_Id, Item.Item_Name, Invoices_Details.NameSpecification  , Invoices_List.Customer_Id, " + Customer_Id_RelatedToAccount + ",Invoices_List.Invoice_Id, Invoices_List.ID, SUM(Invoices_Details.Qnty_value) AS Total ,  AVG(Customer.Balance) AS Expr3, 2 AS Transaction_Type, Invoices_List.Invoice_Date, 1 AS Reference_Type, SUM(Invoices_Details.Larg_Qnty) AS Larg_Qnty, SUM(Invoices_Details.Small_Qnty) AS Small_Qnty,  SUM(Invoices_Details.Qnty_Another) AS Qnty_Another, SUM(Invoices_Details.Qnty_value) AS Qnty_value FROM Customer INNER JOIN Invoices_List ON Customer.Id = Invoices_List.Customer_Id INNER JOIN Invoices_Details ON Invoices_List.ID = Invoices_Details.Invoice_Id INNER JOIN Item ON Invoices_Details.Item_Id = Item.id ";
            str = str + " WHERE (Customer.Id = " + Customer_Id.ToString() + ") AND (Invoices_List.id = " + New_Invoice_ListTable_id.ToString() + ") ";
            str = str + " GROUP BY Invoices_List.CarsWorkOrdersId, Invoices_List.WorkOrderNumber, Invoices_List.ProcessNo, Invoices_Details.Larg_Unit_Price, Invoices_Details.Item_Id, Item.Item_Name, Invoices_List.Customer_Id, Invoices_List.Invoice_Id,  Invoices_List.ID, Invoices_List.Invoice_Date, Invoices_Details.NameSpecification, Invoices_List.CarsId, Invoices_List.CarNumber, Invoices_List.CustomerName, Invoices_List.TaxInternalId ";
            int C = opp.insert_me(str);
            if (C > 0)
            {
                str = "UPDATE Invoices_List SET Customer_BalanceSet = 1,Editable=0 WHERE (id = " + New_Invoice_ListTable_id.ToString() + ") ";
                opp.insert_me(str);

                SetCustomers_Accounts_With_Invoices_ListData(ref opp, New_Invoice_ListTable_id);
            }

        }

        public void Updatet_Customer_BalanceWithEditInvoice(ref int Excp, ref operations opp, DateTime Invoice_Date, int Customer_Id, long New_Invoice_ListTable_id, decimal Net_Total, decimal Invoice_NetTotal_Diff, string Notes)
        {
            Set_Customer_Balance_Invoice(ref Excp, ref opp, Customer_Id, New_Invoice_ListTable_id, Net_Total, Notes);

            return;
            //string str = "";

            //if (Common.Is_Set_Customer_Balance_With_Sum_Items == 1)
            //{

            //    this.Set_Customer_Balance_With_Sum_Items_Invoices_List(ref Excp, ref opp, Customer_Id, New_Invoice_ListTable_id, Net_Total, Notes);
            //}

            //else if (Common.Is_Set_Customer_Balance_With_Items == 1)
            //{

            //    this.Set_Customer_Balance_WithItems_Invoices_List(ref Excp, ref opp, Customer_Id, New_Invoice_ListTable_id, Net_Total, Notes);
            //}
            //else
            //{
            //    long num = 0L;
            //    str = " SELECT  Customers_Accounts.* FROM    Customers_Accounts    Where Reference_Type=1 And Reference_Type_Table_Id=" + New_Invoice_ListTable_id.ToString();
            //    DataTable table = opp.return_DataTable(str);
            //    if (table.Rows.Count > 0)
            //    {
            //        num = long.Parse(table.Rows[0]["Id"].ToString());
            //    }
            //    str = "Select * from Invoices_Details Where Invoice_Id=" + New_Invoice_ListTable_id.ToString();
            //    if (opp.return_DataTable(str).Rows.Count == 0)
            //    {
            //        str = " Delete from Customers_Accounts  WHERE (id = " + num.ToString() + ")";
            //        opp.insert_me(str);
            //    }
            //    string[] textArray1 = new string[] { "UPDATE Customer SET balance =balance - ", Invoice_NetTotal_Diff.ToString(), " WHERE (id = ", Customer_Id.ToString(), ")" };
            //    str = string.Concat(textArray1);
            //    opp.insert_me(str);
            //    string[] textArray2 = new string[9];
            //    textArray2[0] = "UPDATE Customers_Accounts SET Transaction_Date='";
            //    textArray2[1] = operations.dateFormat(Invoice_Date);
            //    textArray2[2] = "', Notes=N'";
            //    textArray2[3] = Notes;
            //    textArray2[4] = "',Export=";
            //    textArray2[5] = Net_Total.ToString();
            //    textArray2[6] = "   WHERE (id = ";
            //    textArray2[7] = num.ToString();
            //    textArray2[8] = ")";
            //    str = string.Concat(textArray2);
            //    opp.insert_me(str);
            //    string[] textArray3 = new string[] { "UPDATE Customers_Accounts SET balance=balance - ", Invoice_NetTotal_Diff.ToString(), "   WHERE (id >= ", num.ToString(), ") AND (Customer_Id=", Customer_Id.ToString(), ")" };
            //    str = string.Concat(textArray3);
            //    opp.insert_me(str);
            //}
        }

        public void Updatet_Customer_BalanceWithEditInvoiceReverse(ref int Excp, ref operations opp, DateTime Invoice_Date, int Customer_Id, long Invoice_List_Table_Id, decimal Net_Total, decimal Invoice_NetTotal_Diff, string Notes)
        {
            string str = "";
            if (Common.IsHavePermissionSysMenu(Common.User_Id, 342, Common.Project_ID, Common.Is_Force_Admin_Permission) | (Common.Is_Set_Customer_Balance_With_Items == 1))
            {
                str = "  Delete from  Customers_Accounts Where Reference_Type_Table_Id=" + Invoice_List_Table_Id.ToString() + " and Reference_Type=7";
                opp.insert_me(str);
                this.Set_Customer_BalanceInvoiceReverseWithItem(ref Excp, ref opp, Customer_Id, Invoice_List_Table_Id, Net_Total, Notes);
            }
            else
            {
                long num = 0L;
                str = " SELECT  Customers_Accounts.* FROM    Customers_Accounts    Where Reference_Type=" + Common.InvoiceReverse_Type_Id.ToString() + " And Reference_Type_Table_Id=" + Invoice_List_Table_Id.ToString();
                DataTable table = opp.return_DataTable(str);
                if (table.Rows.Count > 0)
                {
                    num = long.Parse(table.Rows[0]["Id"].ToString());
                }
                str = "Select * from Invoices_Details_Reverse Where Invoice_Id=" + Invoice_List_Table_Id.ToString();
                if (opp.return_DataTable(str).Rows.Count == 0)
                {
                    str = " Delete from Customers_Accounts  WHERE (id = " + num.ToString() + ")";
                    opp.insert_me(str);
                }
                string[] textArray1 = new string[] { "UPDATE Customer SET balance =balance + ", Invoice_NetTotal_Diff.ToString(), ",Balance_Actual=Balance_Actual + ", Invoice_NetTotal_Diff.ToString(), " WHERE (id = ", Customer_Id.ToString(), ")" };
                str = string.Concat(textArray1);
                opp.insert_me(str);
                string[] textArray2 = new string[] { "UPDATE Customers_Accounts SET Notes='", Notes, "',Import=", Net_Total.ToString(), "   WHERE (id = ", num.ToString(), ")" };
                str = string.Concat(textArray2);
                opp.insert_me(str);
                string[] textArray3 = new string[] { "UPDATE Customers_Accounts SET balance=balance + ", Invoice_NetTotal_Diff.ToString(), "   WHERE (id >= ", num.ToString(), ") AND (Customer_Id=", Customer_Id.ToString(), ")" };
                str = string.Concat(textArray3);
                opp.insert_me(str);
            }
        }

        public void Set_Customer_Balance_WithMoney_Transaction(ref int Excp, ref operations opp, long Customer_Id, int Transaction_Type, long Order_Mony_Id, long Money_Transaction_BoxId, decimal Cash_Value, string Notes)
        {
            if (Transaction_Type == 1)
            {
                Set_Customer_Balance_WithMoney_TransactionEstlam(ref Excp, ref opp, Customer_Id, Transaction_Type, Order_Mony_Id, Money_Transaction_BoxId, Cash_Value, Notes);

            }
            if (Transaction_Type == 2)
            {
                Set_Customer_Balance_WithMoney_TransactionSarf(ref Excp, ref opp, Customer_Id, Transaction_Type, Order_Mony_Id, Money_Transaction_BoxId, Cash_Value, Notes);
            }
        }

        long GetCustomer_Id_RelatedToAccount(ref operations opp, long Customer_Id)
        {
            long Customer_Id_RelatedToAccount = 0;
            Customer customer = new Customer();
            CustomerCLS customerCLS = new CustomerCLS();
            customerCLS.GetCustomerByIdAsNoTracking(ref opp, Customer_Id);
            customer = customerCLS.cs;

            if (customer != null)
            {
                Customer_Id_RelatedToAccount = customer.Customer_Id_RelatedToAccount ?? 0;
            }

            if (Customer_Id_RelatedToAccount == 0)
            {
                Customer_Id_RelatedToAccount = Customer_Id;
            }
            return Customer_Id_RelatedToAccount;
        }

        public void Set_Customer_Balance_WithMoney_TransactionEstlam(ref int Excp, ref operations opp, long Customer_Id, int Transaction_Type, long Order_Mony_Id, long Money_Transaction_BoxId, decimal Cash_Value, string Notes)
        {
            int Reference_Type = 3;
            long Reference_Type_Table_Id = Money_Transaction_BoxId;
            string mysql = "";
            long Customer_Id_RelatedToAccount = GetCustomer_Id_RelatedToAccount(ref opp, Customer_Id);
            mysql = "  Delete from  Customers_Accounts Where Reference_Type_Table_Id=" + Reference_Type_Table_Id.ToString() + " and Reference_Type=" + Reference_Type;
            opp.insert_me(mysql);
            //if (Common.Balance_BeginingPeriod == 0)
            //{
            mysql = "  UPDATE Customer SET Balance = Balance -  " + Cash_Value + " , Balance_Actual = Balance_Actual - " + Cash_Value + "  WHERE (Id = " + Customer_Id + ")";

            opp.insert_me(mysql);
            //}
            //UpdateProcessNo(ref Excp, ref opp, "Money_Transaction_Box", Money_Transaction_BoxId);

              mysql = " INSERT INTO Customers_Accounts (CarsId, CarNumber, CustomerName, CarsWorkOrdersId, WorkOrderNumber, TaxInternalId,ProcessNo,Notes,Customer_Id,Customer_Id_RelatedToAccount, Reference_Id, Reference_Type_Table_Id, Import , Balance, Reference_Type, Transaction_Type, Transaction_Date,Payment_Type) ";
            mysql = mysql + " SELECT  dbo.Money_Transaction_Box.CarsId, dbo.Money_Transaction_Box.CarNumber, dbo.Money_Transaction_Box.Account_Name, dbo.Money_Transaction_Box.CarsWorkOrdersId, dbo.Money_Transaction_Box.WorkOrderNumber, dbo.Money_Transaction_Box.TaxInternalId, dbo.Money_Transaction_Box.ProcessNo, N'" + Notes + "'," + Customer_Id_RelatedToAccount + ",dbo.Money_Transaction_Box.Customer_Id, dbo.Order_Money.Order_No, dbo.Money_Transaction_Box.Id, dbo.Money_Transaction_Box.Transaction_Value,  dbo.Customer.Balance," + Reference_Type + " as  Reference_Type," + Transaction_Type + " AS Transaction_Type, dbo.Money_Transaction_Box.Transaction_Date ,1 AS Payment_Type";
            mysql = mysql + @"  FROM            dbo.Money_Transaction_Box   WITH (NOLOCK)  
                    INNER JOIN     dbo.Customer WITH (NOLOCK) ON dbo.Money_Transaction_Box.Customer_Id = dbo.Customer.Id INNER JOIN
                         dbo.Order_Money WITH (NOLOCK) ON dbo.Money_Transaction_Box.Order_Mony_Id = dbo.Order_Money.Id ";
            mysql = mysql + " WHERE (dbo.Money_Transaction_Box.Order_Mony_Id = " + Order_Mony_Id + ") AND (dbo.Money_Transaction_Box.Customer_Id = " + Customer_Id + ") AND (dbo.Money_Transaction_Box.id=" + Money_Transaction_BoxId + ")";
            opp.insert_me(mysql);


        
        }
        public void Set_Customer_Balance_WithMoney_TransactionSarf(ref int Excp, ref operations opp, long Customer_Id, int Transaction_Type, long Order_Mony_Id, long Money_Transaction_BoxId, decimal Cash_Value, string Notes)
        {
            int Reference_Type = 4;
            long Reference_Type_Table_Id = Money_Transaction_BoxId;
            string mysql = "";
            long Customer_Id_RelatedToAccount = GetCustomer_Id_RelatedToAccount(ref opp, Customer_Id);
            mysql = "  Delete from  Customers_Accounts Where Reference_Type_Table_Id=" + Reference_Type_Table_Id.ToString() + " and Reference_Type=" + Reference_Type;
            opp.insert_me(mysql);
            //if (Common.Balance_BeginingPeriod == 0)
            //{
            mysql = "  UPDATE Customer SET Balance = Balance +  " + Cash_Value + " , Balance_Actual = Balance_Actual - " + Cash_Value + "  WHERE (Id = " + Customer_Id + ")";
            // arr.Add(mysql);
            opp.insert_me(mysql);
            //}

            //UpdateProcessNo(ref Excp, ref opp, "Money_Transaction_Box", Money_Transaction_BoxId);

            mysql = " INSERT INTO Customers_Accounts (CarsId, CarNumber, CustomerName, CarsWorkOrdersId, WorkOrderNumber, TaxInternalId,ProcessNo,Notes,Customer_Id,Customer_Id_RelatedToAccount, Reference_Id, Reference_Type_Table_Id, Export , Balance, Reference_Type, Transaction_Type, Transaction_Date,Payment_Type) ";
            mysql = mysql + " SELECT  Money_Transaction_Box.CarsId, Money_Transaction_Box.CarNumber, Account_Name, Money_Transaction_Box.CarsWorkOrdersId, Money_Transaction_Box.WorkOrderNumber, Money_Transaction_Box.TaxInternalId, Money_Transaction_Box.ProcessNo,N'" + Notes + "'," + Customer_Id_RelatedToAccount + ", Money_Transaction_Box.Customer_Id, Order_Money.Order_No, Money_Transaction_Box.Id, Money_Transaction_Box.Transaction_Value,  Customer.Balance," + Reference_Type + " as  Reference_Type," + Transaction_Type + " AS Transaction_Type, Money_Transaction_Box.Transaction_Date ,1 AS Payment_Type";
            mysql = mysql + "  FROM Money_Transaction_Box INNER JOIN  Customer ON Money_Transaction_Box.Customer_Id = Customer.Id ";
            mysql = mysql + "  INNER JOIN Order_Money ON Money_Transaction_Box.Order_Mony_Id = Order_Money.Id ";
            mysql = mysql + " WHERE (Money_Transaction_Box.Order_Mony_Id = " + Order_Mony_Id + ") AND (Money_Transaction_Box.Customer_Id = " + Customer_Id + ") AND (Money_Transaction_Box.id=" + Money_Transaction_BoxId + ")";
            // arr.Add(mysql);
            opp.insert_me(mysql);



        }


        public void Set_Customer_Balance_With_CheckMoney(ref int Excp, ref operations opp, long Customer_Id, int Transaction_Type, long Order_Mony_Id, long Checks_MemoryId, decimal Cash_Value, string Notes, DateTime ProcessDate, bool IsDeletePrevOperation = false)
        {
            if (Transaction_Type == 1)
            {
                Set_Customer_Balance_With_CheckMoney_Estlam(ref Excp, ref opp, Customer_Id, Transaction_Type, Order_Mony_Id, Checks_MemoryId, Cash_Value, Notes, ProcessDate, IsDeletePrevOperation);

            }
            if (Transaction_Type == 2)
            {
                Set_Customer_Balance_With_CheckMoney_Sarf(ref Excp, ref opp, Customer_Id, Transaction_Type, Order_Mony_Id, Checks_MemoryId, Cash_Value, Notes, ProcessDate, IsDeletePrevOperation);
            }
        }

        void UpdateProcessNo(ref int Excp, ref operations opp, string TableName, long TableId)
        {
            string mysql = "";
             

            if (ProcessNo == 0)
            {
                ProcessNo = Common.Make_AccountsProcessNo(ref Excp, ref opp);
            }


            try
            {
                mysql = "Update " + TableName + " WITH (ROWLOCK)  Set ProcessNo=" + ProcessNo + " Where Id=" + TableId;
                opp.insert_me(mysql);
            }
            catch (Exception ee)
            {

            }
        }
        void Set_Customer_Balance_With_CheckMoney_Estlam(ref int Excp, ref operations opp, long Customer_Id, int Transaction_Type, long Order_Mony_Id, long Checks_MemoryId, decimal Check_Value, string Notes, DateTime ProcessDate, bool IsDeletePrevOperation = false)
        {

            int Reference_Type = 5;
            long Reference_Type_Table_Id = Checks_MemoryId;
            string mysql = "";

            if (IsDeletePrevOperation == true)
            {
                mysql = "  Delete from  Customers_Accounts Where Reference_Type_Table_Id=" + Reference_Type_Table_Id.ToString() + " and Reference_Type=" + Reference_Type;
                opp.insert_me(mysql);
            }
            //if (Common.Balance_BeginingPeriod == 0)
            //{
            mysql = "  UPDATE Customer SET Balance = Balance -  " + Check_Value + " ,Balance_Actual = Balance_Actual - " + Check_Value + "  WHERE (Id = " + Customer_Id + ")";
            opp.insert_me(mysql);
            //}

            ///Set ProcessNo In Table Processed
            //UpdateProcessNo(ref Excp, ref opp, "Checks_Memory", Checks_MemoryId);

            mysql = " INSERT INTO Customers_Accounts (ProcessNo,Customer_Id, Reference_Id, Reference_Type_Table_Id, Import, Balance, Reference_Type, Transaction_Type, Transaction_Date,Payment_Type,Flage_View,Notes) ";
            mysql = mysql + @" SELECT Checks_Memory.ProcessNo, " + Customer_Id + ", Order_Money_Check.Order_No, Checks_Memory.id, Checks_Memory.Check_Value, Customer.Balance, 5 AS Reference_Type, 2 AS Transaction_Type, '" + operations.dateFormat(ProcessDate) + "',  2 AS Payment_Type, 1 AS Flage_View ,N'" + Notes + "'";
            mysql = mysql + @" FROM Order_Money_Check INNER JOIN
                         Checks_Memory ON Order_Money_Check.Id = Checks_Memory.Order_Mony_Id INNER JOIN
                         Customer ON Checks_Memory.SubAccount_Id_RelatedTo = Customer.SubAccount_Id ";
            mysql = mysql + " WHERE (Checks_Memory.Order_Mony_Id = " + Order_Mony_Id + ")  AND (Checks_Memory.id=" + Checks_MemoryId + ") AND (Customer.Id = " + Customer_Id + ")";
            //AND (Customer.Id = " + Customer_Id + ") 
            opp.insert_me(mysql);



        }
        void Set_Customer_Balance_With_CheckMoney_Sarf(ref int Excp, ref operations opp, long Customer_Id, int Transaction_Type, long Order_Mony_Id, long Checks_MemoryId, decimal Check_Value, string Notes, DateTime ProcessDate, bool IsDeletePrevOperation = false)
        {
            int Reference_Type = 6;
            long Reference_Type_Table_Id = Checks_MemoryId;
            string mysql = "";

            if (IsDeletePrevOperation == true)
            {
                mysql = "  Delete from  Customers_Accounts Where Reference_Type_Table_Id=" + Reference_Type_Table_Id.ToString() + " and Reference_Type=" + Reference_Type;
                opp.insert_me(mysql);
            }
            //if (Common.Balance_BeginingPeriod == 0)
            //{
            if (Common.Balance_BeginingPeriod == 0)
            {
                mysql = "  UPDATE Customer SET Balance = Balance +  " + Check_Value + " ,Balance_Actual = Balance_Actual - " + Check_Value + "  WHERE (Id = " + Customer_Id + ")";
                arr.Add(mysql);
            }

            ///Set ProcessNo In Table Processed
            //UpdateProcessNo(ref Excp, ref opp, "Checks_Memory", Checks_MemoryId);

            mysql = " INSERT INTO Customers_Accounts (ProcessNo,Customer_Id, Reference_Id, Reference_Type_Table_Id, Export, Balance, Reference_Type, Transaction_Type, Transaction_Date,Payment_Type,Flage_View,Notes) ";
            mysql = mysql + " SELECT Checks_Memory.ProcessNo, " + Customer_Id + ".Id, Order_Money_Check.Order_No, Checks_Memory.id, Checks_Memory.Check_Value, Customer.Balance,  " + Reference_Type + " AS Reference_Type , 2 AS Transaction_Type, '" + operations.dateFormat(ProcessDate) + "',2 AS Payment_Type ," + Common.Flage_AccountDetails_View + " AS Flage_View , N'" + Notes + "'";
            mysql = mysql + @" FROM Order_Money_Check INNER JOIN
                         Checks_Memory ON Order_Money_Check.Id = Checks_Memory.Order_Mony_Id INNER JOIN
                         Customer ON Checks_Memory.SubAccount_Id_RelatedFrom = Customer.SubAccount_Id  ";

            mysql = mysql + " WHERE (Checks_Memory.Order_Mony_Id = " + Order_Mony_Id + ")  AND (Checks_Memory.id=" + Checks_MemoryId + ")  AND (Customer.Id = " + Customer_Id + ")";
            opp.insert_me(mysql);
            //AND (Checks_Memory.Customer_Id = " + Customer_Id + ") 


        }

        public void Set_Customer_Balance_With_CarsExaminations(ref int Excp, ref operations opp, long Customer_Id, int Transaction_Type, long CarsExaminationsId, decimal CarsExaminationValue, string Notes, DateTime ProcessDate, bool IsDeletePrevOperation = false)
        {
            int Reference_Type = Common.CarsExaminations_Reference_Type;
            long Reference_Type_Table_Id = CarsExaminationsId;
            string mysql = "";

            if (IsDeletePrevOperation == true)
            {
                mysql = "  Delete from  Customers_Accounts Where Reference_Type_Table_Id=" + Reference_Type_Table_Id.ToString() + " and Reference_Type=" + Reference_Type;
                opp.insert_me(mysql);
            }


            ///Set ProcessNo In Table Processed
            //UpdateProcessNo(ref Excp, ref opp, "CarsExaminations", CarsExaminationsId);

            mysql = " INSERT INTO Customers_Accounts (ProcessNo,Customer_Id, Reference_Id, Reference_Type_Table_Id, Export, Balance, Reference_Type, Transaction_Type, Transaction_Date,Payment_Type,Flage_View,Notes) ";
            mysql = mysql + " SELECT CarsExaminations.ProcessNo, " + Customer_Id + ".Id, CarsExaminations.ExaminationNumber, CarsExaminations.Id, CarsExaminations.CarsExaminationValue, Customer.Balance,  " + Reference_Type + " AS Reference_Type , 2 AS Transaction_Type, '" + operations.dateFormat(ProcessDate) + "',2 AS Payment_Type ," + Common.Flage_AccountDetails_View + " AS Flage_View , N'" + Notes + "'";
            mysql = mysql + @" FROM CarsExaminations INNER JOIN
                          
                         Customer ON CarsExaminations.SubAccount_Id  = Customer.SubAccount_Id  ";

            mysql = mysql + " WHERE   (CarsExaminations.id=" + CarsExaminationsId + ")  AND (Customer.Id = " + Customer_Id + ")";
            opp.insert_me(mysql);
            //AND (CarsExaminations.Customer_Id = " + Customer_Id + ") 


        }
        public void Set_Customer_Balance_WithEntry_List(ref int Excp, ref operations opp, long SubAccount_Id, long Account_Entry_List_ID, long Account_Entry_Details_Id)
        {

            string str = "";
            str = "  Delete from  Customers_Accounts Where Reference_Type_Table_Id=" + Account_Entry_List_ID.ToString() + " and Reference_Type_TableDetails_Id=" + Account_Entry_Details_Id + " AND Reference_Type=" + Common.Entry_Type_Id.ToString();
            int c = opp.insert_me(str);

            //Larg_Unit_Price,Item_Name,
            //Invoices_Details.Larg_Unit_Price,Item.Item_Name,
            ///Set ProcessNo In Table Processed
             //UpdateProcessNo(ref Excp, ref opp, "Account_Entry_List", Account_Entry_List_ID);

            str = @"INSERT INTO Customers_Accounts (ProcessNo,Larg_Unit_Price, Item_Id, Item_Name, Notes, Customer_Id, Reference_Id, Reference_Type_Table_Id, Reference_Type_TableDetails_Id,Export,Import,  Balance, Transaction_Type, Transaction_Date, Reference_Type, Larg_Qnty, Small_Qnty, Item_No, Item_Size, Qnty_Size, Qnty_value, Height_Width_Item)";
            str = str + " SELECT      Account_Entry_List.ProcessNo,  0 AS Larg_Unit_Price, 0 AS Item_Id, 0 AS Item_Name, Account_Entry_Details.Notes_Details, Customer.Id AS Customer_Id, Account_Entry_List.Entry_No, Account_Entry_Details.Account_Entry_List_ID, Account_Entry_Details.Id,  Account_Entry_Details.Equivalent_Import_Value, Account_Entry_Details.Equivalent_Export_Value, Customer.Balance, Account_Entry_Details.Transaction_Type, Account_Entry_List.Entry_Date, " + Common.Entry_Type_Id.ToString() + " AS Reference_Type,    '-' AS Larg_Qnty, '-' AS Small_Qnty, '-' AS Item_No, '-' AS Item_Size, '-' AS Qnty_Size, '-' AS Qnty_value, '-' AS Height_Width_Item ";

            str = str + " FROM Customer INNER JOIN Account_Entry_Details ON Customer.SubAccount_Id = Account_Entry_Details.Submain_Account_ID INNER JOIN Account_Entry_List ON Account_Entry_Details.Account_Entry_List_ID = Account_Entry_List.Id  ";
            str = str + " WHERE (Customer.SubAccount_Id = " + SubAccount_Id.ToString() + ") AND (Account_Entry_List.id = " + Account_Entry_List_ID.ToString() + ") AND (Account_Entry_Details.id = " + Account_Entry_Details_Id.ToString() + ")";

            c = opp.insert_me(str);

        }
        public void Delete_Customer_Balance_WithEntry_List(ref int Excp, ref operations opp, long Account_Entry_List_ID)
        {

            string str = "";
            str = "  Delete from  Customers_Accounts Where Reference_Type_Table_Id=" + Account_Entry_List_ID.ToString() + "   AND Reference_Type=" + Common.Entry_Type_Id.ToString();
            int c = opp.insert_me(str);

        }
        public void Delete_Customer_Balance_With_Entry_Details(ref int Excp, ref operations opp, long Account_Entry_List_ID, long Account_Entry_DetailsId)
        {

            string str = "";
            str = "  Delete from  Customers_Accounts Where Reference_Type_TableDetails_Id=" + Account_Entry_DetailsId + " AND Reference_Type_Table_Id=" + Account_Entry_List_ID.ToString() + "   AND Reference_Type=" + Common.Entry_Type_Id.ToString();
            int c = opp.insert_me(str);

        }
        public void Delete_Customer_Balance(ref int Excp, ref operations opp, long Reference_Type_Table_Id, int Reference_Type)
        {

            string str = "";
            str = "  Delete from  Customers_Accounts Where Reference_Type_Table_Id=" + Reference_Type_Table_Id + " and Reference_Type=" + Reference_Type + "";
            opp.insert_me(str);

        }
        public int Delete_Customer_Accounts_OneRow(ref int Excp, ref string Msg, long Customer_Id, DateTime Transaction_Date, string Reference_Type, string Reference_Type_Table_Id, string Transaction_Type)
        {
            int c = 0;
            try
            {

                operations opp = new operations();

                CustomersAccountCLS ac = new CustomersAccountCLS();
                string mysql = "Delete from Customers_Accounts Where Customer_Id= " + Customer_Id + " AND Transaction_Date='" + operations.dateFormat(Transaction_Date) + "' AND   Reference_Type=" + Reference_Type + " And Reference_Type_Table_Id=" + Reference_Type_Table_Id + " AND Transaction_Type=" + Transaction_Type + "";
                c = opp.insert_me(mysql);
                Excp = 0;
            }
            catch (Exception ee)
            {

                Msg = ee.Message;
                Excp = 1;

            }
            return c;

        }
        public int Delete_Customer_Accounts_OneRow(ref int Excp, ref string Msg, long Customers_AccountsId)
        {
            int c = 0;
            try
            {

                operations opp = new operations();

                CustomersAccountCLS ac = new CustomersAccountCLS();
                string mysql = "Delete from Customers_Accounts Where  Id= " + Customers_AccountsId + "  ";
                c = opp.insert_me(mysql);
                Excp = 0;
            }
            catch (Exception ee)
            {

                Msg = ee.Message;
                Excp = 1;

            }
            return c;

        }
        public int Delete_Customer_Accounts_ProcessNo(ref int Excp, ref string Msg, long ProcessNo)
        {
            operations opp = new operations();
            return Delete_Customer_Accounts_ProcessNo(ref Excp, ref Msg, ref opp, ProcessNo);
        }
        public int Delete_Customer_Accounts_ProcessNo(ref int Excp, ref string Msg, ref operations opp, long ProcessNo)
        {
            int c = 0;

            CustomersAccountCLS ac = new CustomersAccountCLS();
            string mysql = "Delete from Customers_Accounts Where ProcessNo= " + ProcessNo + "  ";
            c = opp.insert_me(mysql);
            Excp = 0;

            return c;

        }
    }
}


//else if (Reference_Type == "3" | Reference_Type == "27")
//           {
//               // Reference_Type_View = "≈” ·«„ ‰ﬁœÌ…";
//               View_CachData();
//           }
//           else if (Reference_Type == "4" | Reference_Type == "28")
//           {
//               //Reference_Type_View = "’—› ‰ﬁœÌ…";
//               View_CachData();
//           }
//           else if (Reference_Type == "5" | Reference_Type == "35")
//           {
//               //  Reference_Type_View = "≈” ·«„ ‘Ìﬂ";
//               Read_Selected_Check_Data();
//           }
//           else if (Reference_Type == "6" | Reference_Type == "36")
//           {
//               // Reference_Type_View = "’—› ‘Ìﬂ";
//               Read_Selected_Check_Data();
//           }