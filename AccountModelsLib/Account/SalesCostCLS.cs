using System;
using System.Collections.Generic;
using System.Text;
 using AccountModelsLib;
using AccountModelsLib.Models;
using System.Data;
using System.Collections;
 

namespace AccountModelsLib
{
    public class SalesCostCLS
    {
        public DateTime DateFrom;
        public DateTime DateTo;

        public decimal FirstStore = 0;                  // ãÎÒæä Çæá ÇáãÏÉ
        public decimal LastStore = 0;                      // ãÎÒæä ÇÎÑ ÇáãÏÉ
        public decimal PurchaseTotal = 0;         // ÇáãÔÊÑíÇÊ
        public decimal PurchaseMoneySpent = 0;         // ãÕÑæÝÇÊ ÇáãÔÊÑíÇÊ
        public decimal PurchaseReverseTotal = 0;    // ãÑÏæÏ ÇáãÔÊÑíÇÊ
        public decimal WinCommission = 0;         // ÇáÎÕã ÇáãßÊÓÈ
        public decimal CostForItemSale = 0;      //  ÊßáÝÉ ÇáÈÖÇÚÉ ÇáãÈÇÚÉ

        public decimal SalesTotal = 0;      // ÅÌãÇáì ãÈíÚÇÊ
        public decimal SalesReverseTotal = 0;      // ÅÌãÇáì ãÑÏæÏ  ãÈíÚÇÊ
        public decimal AllowCommission = 0;         // ÇáÎÕã ãÓãæÍ ÈÉ

        public decimal Import_ValueTotal = 0;      // ÅÌãÇáì æÇÑÏÇÊ
        public decimal Export_ValueTotal = 0;      // ÅÌãÇáì ãÕÑæÝÇÊ
        public SalesCostCLS()
        {

        }


        public void GetSalesTotal()
        {
            int IsInvoiceTaxInfo = Common.PermissionToShowTaxInvoiceOrNotORAll();

            int Excp = 0;
            string mysql = " SELECT SUM(Total_Item_CommissionValue / 100 * Total) AS commission, SUM(Net_Total) AS Net_Total FROM Invoices_List ";
            mysql = mysql + " Where  (Invoice_Date >= '" + operations.dateFormat(DateFrom) + "') AND  (Invoice_Date <= '" + operations.dateFormat(DateTo) + "') ";
            if (IsInvoiceTaxInfo > -1)
            {
                mysql = mysql + "  AND (Invoices_List.IsInvoiceTaxInfo=" + IsInvoiceTaxInfo + ") ";
            }
            DataSet ds = Get_Data(ref Excp, mysql);

            if (ds.Tables[0].Rows.Count > 0 & ds.Tables[0].Rows[0]["Net_Total"].ToString() != "")
            {
                SalesTotal = decimal.Parse(ds.Tables[0].Rows[0]["Net_Total"].ToString());
                AllowCommission = decimal.Parse(ds.Tables[0].Rows[0]["commission"].ToString());

                SalesTotal = Common.RoundMe(SalesTotal);
                AllowCommission = Common.RoundMe(AllowCommission);
            }

        }
        public void GetMoney_Transaction_BoxTotal()
        {
            int Excp = 0;
            string mysql = " SELECT SUM(Export_Value) AS Export_Value ,SUM(Import_Value) AS Import_Value FROM Money_Transaction_Box ";
            mysql = mysql + " Where  (Transaction_Date >= '" + operations.dateFormat(DateFrom) + "') AND  (Transaction_Date <= '" + operations.dateFormat(DateTo) + "') ";

            DataSet ds = Get_Data(ref Excp, mysql);

            if (ds.Tables[0].Rows.Count > 0)
            {
                try
                {
                    Export_ValueTotal = decimal.Parse(ds.Tables[0].Rows[0]["Export_Value"].ToString());
                }
                catch { }
                try
                {
                    Import_ValueTotal = decimal.Parse(ds.Tables[0].Rows[0]["Import_Value"].ToString());
                }
                catch { }


            }

        }
        public void GetReverseSalesTotal()
        {
            int Excp = 0;
            string mysql = " SELECT SUM(Total_Item_CommissionValue / 100 * Total) AS commission, SUM(Net_Total) AS Net_Total FROM Invoices_List_Reverse ";
            mysql = mysql + " Where  (Invoice_Date >= '" + operations.dateFormat(DateFrom) + "') AND  (Invoice_Date <= '" + operations.dateFormat(DateTo) + "') ";

            DataSet ds = Get_Data(ref Excp, mysql);

            if (ds.Tables[0].Rows.Count > 0 & ds.Tables[0].Rows[0]["Net_Total"].ToString() != "")
            {
                SalesReverseTotal = decimal.Parse(ds.Tables[0].Rows[0]["Net_Total"].ToString());
                SalesReverseTotal = Common.RoundMe(SalesReverseTotal);
            }

        }


        public void GetPurchaseTotal()
        {
            int Excp = 0;
            string mysql = " SELECT SUM(Total_Item_CommissionValue / 100 * Total) AS commission, SUM(Net_Total) AS Net_Total,SUM(Load_Payment) AS Load_Payment FROM Bills_List ";
            mysql = mysql + " Where  (Invoice_Date >= '" + operations.dateFormat(DateFrom) + "') AND  (Invoice_Date <= '" + operations.dateFormat(DateTo) + "') ";

            DataSet ds = Get_Data(ref Excp, mysql);

            if (ds.Tables[0].Rows.Count > 0 & ds.Tables[0].Rows[0]["Net_Total"].ToString() != "")
            {
                PurchaseTotal = decimal.Parse(ds.Tables[0].Rows[0]["Net_Total"].ToString());
                
                PurchaseMoneySpent = decimal.Parse(ds.Tables[0].Rows[0]["Load_Payment"].ToString());
                try
                {
                    WinCommission = decimal.Parse(ds.Tables[0].Rows[0]["commission"].ToString());
                }
                catch (Exception ee)
                {
                    WinCommission = 0;
                }
                PurchaseTotal = Common.RoundMe(PurchaseTotal);
                WinCommission = Common.RoundMe(WinCommission);
                PurchaseMoneySpent = Common.RoundMe(PurchaseMoneySpent);
            }

        }

        public void GetReversePurchaseTotal()
        {
            int Excp = 0;
            string mysql = " SELECT SUM(Total_Item_CommissionValue / 100 * Total) AS commission, SUM(Net_Total) AS Net_Total FROM Bills_List_Reverse ";
            mysql = mysql + " Where  (Invoice_Date >= '" + operations.dateFormat(DateFrom) + "') AND  (Invoice_Date <= '" + operations.dateFormat(DateTo) + "') ";

            DataSet ds = Get_Data(ref Excp, mysql);

            if (ds.Tables[0].Rows.Count > 0 & ds.Tables[0].Rows[0]["Net_Total"].ToString() != "")
            {
                PurchaseReverseTotal = decimal.Parse(ds.Tables[0].Rows[0]["Net_Total"].ToString());
                PurchaseReverseTotal = Common.RoundMe(PurchaseReverseTotal);


            }


        }

        public DataTable GetSalesTotal(long Item_Id, long Item_Category_ID, bool _dtFrom, DateTime dtFrom, bool _dtTo, DateTime dtTo)
        {
            operations opp = new operations();
            string mysql = @" SELECT  0.0 AS LostValue,       '' AS Serial_, '' AS QntyStrColumn, unit.B_Unit, unit.S_unit, unit.S_unit_Value, Item.Item_Name, ISNULL(SUM(Invoices_Details.Qnty_value), 0) AS Qnty_value_Sell, ISNULL(SUM(Bills_Details.Qnty_value), 0) 
                         AS Qnty_value_Purchase, SUM(Invoices_Details.Larg_Qnty) AS Larg_Qnty_Sell, SUM(Invoices_Details.Small_Qnty) AS Small_Qnty_Sell, SUM(Invoices_Details.Qnty_Another) AS Qnty_Another_Sell, 
                         SUM(Bills_Details.Larg_Qnty) AS Larg_Qnty_Purchase, SUM(Bills_Details.Small_Qnty) AS Small_Qnty_Purchase, SUM(Bills_Details.Qnty_Another) AS Qnty_Another_Purchase, 
                         ISNULL(SUM(Invoices_Details.Qnty_value), 0) - ISNULL(SUM(Bills_Details.Qnty_value), 0) AS Profit
FROM            Invoices_List INNER JOIN
                         Invoices_Details ON Invoices_List.ID = Invoices_Details.Invoice_Id INNER JOIN
                         Item INNER JOIN
                         unit ON Item.Unit_Id = unit.id ON Invoices_Details.Item_Id = Item.id LEFT OUTER JOIN
                         Bills_Details ON Item.id = Bills_Details.Item_Id ";



            mysql = mysql + @" WHERE     (Item.id > 0) ";



            if (Item_Category_ID > 0)
            {
                mysql = mysql + @" AND (Item.Item_Category_ID = " + Item_Category_ID + ") ";
            }
            if (Item_Id > 0)
            {
                mysql = mysql + @" AND (Invoices_Details.Item_Id = " + Item_Id + ") ";
            }




            if (_dtFrom == true)
            {

                mysql = mysql + " And (Invoices_List.Invoice_Date  >='" + operations.dateFormat(dtFrom) + "')  ";
            }
            if (_dtTo == true)
            {

                mysql = mysql + " And (Invoices_List.Invoice_Date  <='" + operations.dateFormat(dtTo) + "')  ";
            }

            mysql = mysql + @" GROUP BY unit.B_Unit, unit.S_unit, unit.S_unit_Value, Item.Item_Name";

            DataTable dt = opp.return_DataTableWithBuilder(mysql);
            return dt;
        }
        public DataTable GetPurchaseTotal(long Item_Id, long Item_Category_ID, bool _dtFrom, DateTime dtFrom, bool _dtTo, DateTime dtTo)
        {
            operations opp = new operations();
            string mysql = @" SELECT        '' AS Serial_, '' AS QntyStrColumn, unit.B_Unit, unit.S_unit, unit.S_unit_Value, Item.Item_Name, ISNULL(SUM(Bills_Details.Qnty_value), 0) AS Qnty_value, SUM(Bills_Details.Larg_Qnty) AS Larg_Qnty, 
                         SUM(Bills_Details.Small_Qnty) AS Small_Qnty, SUM(Bills_Details.Qnty_Another) AS Qnty_Another
FROM            Bills_Details INNER JOIN
                         Item INNER JOIN
                         unit ON Item.Unit_Id = unit.id ON Bills_Details.Item_Id = Item.id INNER JOIN
                         Bills_List ON Bills_Details.Invoice_Id = Bills_List.ID ";



            mysql = mysql + @" WHERE     (Item.id > 0) ";



            if (Item_Category_ID > 0)
            {
                mysql = mysql + @" AND (Item.Item_Category_ID = " + Item_Category_ID + ") ";
            }
            if (Item_Id > 0)
            {
                mysql = mysql + @" AND (Bills_Details.Item_Id = " + Item_Id + ") ";
            }




            if (_dtFrom == true)
            {

                mysql = mysql + " And (Bills_List.Invoice_Date  >='" + operations.dateFormat(dtFrom) + "')  ";
            }
            if (_dtTo == true)
            {

                mysql = mysql + " And (Bills_List.Invoice_Date  <='" + operations.dateFormat(dtTo) + "')  ";
            }

            mysql = mysql + @" GROUP BY unit.B_Unit, unit.S_unit, unit.S_unit_Value, Item.Item_Name ";

            DataTable dt = opp.return_DataTableWithBuilder(mysql);
            return dt;
        }


        private DataSet Get_Data(ref int Excp, string mysql)
        {
            DataSet G_DS = new DataSet();

            try
            {
                operations opp = new operations();
                G_DS = opp.return_dataset(mysql, "Unit_item");
                Excp = 0;
                opp = null;
                return G_DS;
            }
            catch (Exception ee)
            {

                Excp = -1;
                //MessageBox.Show(ee.Message);
                return G_DS;
            }
        }

        public DataTable GetRepotTotal()
        {
            DataTable dt = new DataTable();
            DataColumn id = new DataColumn("id", Type.GetType("System.String"));
            dt.Columns.Add(id);





            return dt;
        }

    }
}
