using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
  using AccountModelsLib; using AccountModelsLib.Models;
using System.Collections;
 
namespace AccountModelsLib
{
    public class InvoicesTotalReverseCls
    {
        public static Int64 New_Invoices_No;   // ÑÞã ÇáÝÇÊæÑå ÇáÌÏíÏÉ
        DataSet ds = new DataSet();
        operations opp = new operations();
        //updateed in 25 / 1/ 07
        	
        public void insertinto(long Currency_Id, DateTime dt, decimal total, decimal commisition, decimal nuTaxSales, decimal nuATS05, decimal taxsal, decimal dtsos, decimal nettotal, long custid, long invoiceid, string notes, int Invoice_Type, decimal DAMGHAT)
        {
            int Excp = 0;
            string dt_ = operations.dateFormat(dt);

            long ProcessNo = 0;
            if (ProcessNo == 0)
            {
                ProcessNo = Common.Make_AccountsProcessNo(ref Excp, ref opp, "Invoices_List_Reverse");
                // ProcessNo,
                // "+ ProcessNo + ",
            }

            string mysql = " insert into Invoices_List_Reverse(ProcessNo,Currency_id,Invoice_Date,total,commission,TaxSales10_Value,ATS05_Value,TaxSales10,ATS05,net_total,Customer_Id,Invoice_Id,Notes,Invoice_Type,DAMGHAT) ";
            mysql = mysql + " values ("+ ProcessNo + "," + Currency_Id + ",'" + operations.dateFormat(dt) + "'," + nettotal + "," + commisition + "," + taxsal + "," + dtsos + "," + nuTaxSales + "," + nuATS05 + "," + total + "," + custid + "," + invoiceid + ",'" + notes + "'," + Invoice_Type + "," + DAMGHAT + ")";
            long InvoiceListTableId = opp.Insert_me_Return_Last_Identify(mysql, ref Excp);
            ArrayList arr=new ArrayList();
            CustomersAccountCLS ca = new CustomersAccountCLS();
            ca.Set_Customer_BalanceInvoiceReverse(ref Excp, arr,custid , InvoiceListTableId, (decimal)total);


        }

        public bool Check_InvoiceNo_Found(ref int Excp, long Invoice_No)
        {
            try
            {
                string mysql = "SELECT Invoices_List_Reverse.* FROM Invoices_List_Reverse WHERE (Invoice_Id = " + Invoice_No + ") ";
                operations opp = new operations();
                DataSet ds = opp.return_dataset(mysql, "Invoices_List_Reverse");
                if (ds.Tables["Invoices_List_Reverse"].Rows.Count > 0)
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
            catch { return false; }
        }
        public void updateinvoice(ref int Excp, decimal Invoice_Net_Total_Old, long Currency_Id, DateTime dt, decimal total, decimal commisition, decimal taxsal, decimal dtsos, decimal nettotal, long custid, long invoiceid, string notes, long Customer_id, decimal? TaxSales10 = null, decimal? ATS05 = null, decimal? DAMGHAT = null)
        {
            try
            {
                ArrayList arr = new ArrayList();
                string mysql = "";

                // áÇ íõÓãÍ ÈÊÛííÑ ÇáÚãíá ÚäÏ ÇáÊÚÏíá: ÝÑÞ ÇáÑÕíÏ íõØÈóøÞ Úáì Úãíá æÇÍÏ ÝÞØ
                DataTable dtOld = opp.return_DataTable("SELECT Customer_Id FROM Invoices_List_Reverse WHERE id=" + invoiceid);
                if (dtOld.Rows.Count == 0 || dtOld.Rows[0]["Customer_Id"].ToString() != custid.ToString() || Customer_id != custid)
                {
                    Excp = 1;
                    return;
                }

                // TaxSales10/ATS05/DAMGHAT ÇÎÊíÇÑíÉ: ÊõÍÝÙ ÝÞØ Åä ãõÑöøÑÊ (ÇáÏíÓß ÊæÈ íãÑÑåÇ¡ Razor íÍÝÙåÇ ÈäÝÓå)
                string extraCols = "";
                if (TaxSales10.HasValue) extraCols += ",TaxSales10=" + TaxSales10.Value;
                if (ATS05.HasValue) extraCols += ",ATS05=" + ATS05.Value;
                if (DAMGHAT.HasValue) extraCols += ",DAMGHAT=" + DAMGHAT.Value;

                mysql = "update Invoices_List_Reverse set Currency_id= " + Currency_Id + ",Invoice_Date='" + operations.dateFormat(dt) + "',total=" + total + " ,commission=" + commisition + " ,TaxSales10_Value=" + taxsal + ",ATS05_Value=" + dtsos + " ,net_total=" + nettotal + ",Customer_Id=" + custid + ",Notes='" + notes + "'" + extraCols + ", Edit_User_Id=" + Common.User_Id + ", EditOperationDate=GETDATE() where id=" + invoiceid;
                arr.Add(mysql);

                decimal Invoice_NetTotal_Diff = Invoice_Net_Total_Old - nettotal;

                mysql = "UPDATE Customer SET balance =balance + " + Invoice_NetTotal_Diff + " WHERE (id = " + Customer_id + ")";
                arr.Add(mysql);

                DataSet dsss = opp.return_dataset(" SELECT * FROM Customers_Accounts  Where Reference_Type=7 And Reference_Type_Table_Id=" + invoiceid + "");
                long Customers_Accounts_Id = 0;


                Customers_Accounts_Id = long.Parse(dsss.Tables[0].Rows[0]["Id"].ToString());


                mysql = "UPDATE Customers_Accounts SET Import=" + nettotal + "   WHERE (id = " + Customers_Accounts_Id + ")";
                arr.Add(mysql);

                mysql = "UPDATE Customers_Accounts SET balance=balance + " + Invoice_NetTotal_Diff + "   WHERE (id >= " + Customers_Accounts_Id + ") AND (Customer_Id=" + Customer_id + ")";
                arr.Add(mysql);
                 
                
                int Status = opp.insert_me_transaction(ref Excp, 1, arr);
                if (Status == 1)
                {
                    Excp = 0;
                }
                else
                {
                    Excp = 1;
                }
            }
            catch
            {
                Excp = 1;
            }

        }
        public void deleteinvoice(ref int Excp, decimal Invoice_Net_Total_Old, string invoiceTableid, string Invoices_ID, string Customer_Id, string txtNetTotal)
        {
            try
            {
                ArrayList arr = new ArrayList();
                string mysql = " delete from  Invoices_List_Reverse where id=" + invoiceTableid;
                arr.Add(mysql);

                mysql = " update Customer set Balance=Balance + " + txtNetTotal + " ,Balance_Actual=Balance_Actual +" + txtNetTotal + " where id=" + Customer_Id;
                arr.Add(mysql);

                DataSet dsss = opp.return_dataset(" SELECT Invoices_List_Reverse.Net_Total, Customers_Accounts.* FROM Invoices_List_Reverse INNER JOIN  Customers_Accounts ON Invoices_List_Reverse.ID = Customers_Accounts.Reference_Type_Table_Id  Where Reference_Type=7 And Reference_Type_Table_Id=" + invoiceTableid + "");
                long Customers_Accounts_Id = 0;

                try
                {
                    Customers_Accounts_Id = long.Parse(dsss.Tables[0].Rows[0]["Id"].ToString());

                }
                catch
                {
                    Customers_Accounts_Id = 0;

                }

                // mysql = "UPDATE Customers_Accounts SET Export=" + NetInvoiceTotal + "   WHERE (id = " + Customers_Accounts_Id + ")";
                // arr.Add(mysql);

                mysql = " DELETE FROM Customers_Accounts WHERE (Customer_Id = " + Customer_Id + ") AND (Reference_Id = " + Invoices_ID + ") AND (Reference_Type_Table_Id = " + invoiceTableid + ") AND (Reference_Type = 7)";
                arr.Add(mysql);

                mysql = "UPDATE Customers_Accounts SET balance=balance + " + txtNetTotal + "   WHERE (id >= " + Customers_Accounts_Id + ") AND (Customer_Id=" + Customer_Id + ")";
                arr.Add(mysql);

                int Status = opp.insert_me_transaction(ref Excp, 1, arr);
                if (Status == 1)
                {
                    Excp = 0;
                }
                else
                {
                    Excp = 1;
                }
            }
            catch
            {
                Excp = 1;
            }

        }
        public DataSet GetInvoices_List(int Invoice_Type)
        {
            string mysql = ""; ;
            if (Invoice_Type == 3)
            {
                mysql = "select * from Invoices_List ORDER BY ID DESC";
            }
            else
            {
                mysql = "select * from Invoices_List where Invoice_Type = " + Invoice_Type + " ORDER BY ID DESC";
            }

            DataSet ds = opp.return_dataset(mysql);
            return ds;

        }

        public DataSet GetInvoices_List_Reverse(int Invoice_Type)
        {
            string mysql = ""; ;
            if (Invoice_Type == 3)
            {
                mysql = "select * from Invoices_List_Reverse where NOT EXISTS (SELECT 1 FROM Invoices_Details_Reverse d WHERE d.Invoice_Id = Invoices_List_Reverse.Id) ORDER BY ID DESC";
            }
            else
            {
                mysql = "select * from Invoices_List_Reverse where NOT EXISTS (SELECT 1 FROM Invoices_Details_Reverse d WHERE d.Invoice_Id = Invoices_List_Reverse.Id) and Invoice_Type = " + Invoice_Type + " ORDER BY ID DESC";
            }

            DataSet ds = opp.return_dataset(mysql);
            return ds;

        }
    }

}
