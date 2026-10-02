using System;
using System.Collections.Generic;
using System.Text;
 
using AccountModelsLib;
using AccountModelsLib.Models;
using System.Data;
using System.Collections;
using Microsoft.EntityFrameworkCore;

using System.Linq;
 
 
namespace AccountModelsLib
{
    public class Requests_Cls : Requests_ModelCls
    {
        public int Make_Invoice_Agile { get; set; } = 1;
        public decimal PaidValueFromMarket = 0;
        public decimal RestValue = 0;

        public bool BtnOkPressMakeOrderMoney = false;
        public ArrayList arr_Order_Purshing_Id_Automatic; //·Õ›Ÿ —ﬁ„ «·«·Ï ·«’œ«— «·«–‰ ÊÌ” Œœ„ ··ÿ»«⁄…
        DataSet G_DS;
        public long New_Invoice_Id;
        public Requests_Cls()
        {
            G_DS = new DataSet();
        }
        public int CommissionISValue = 1;
        
        public void Add_Request(ref int Excp,ref string Msg, DateTime Request_Date, long Customer_Id, long Unit_Item_Table_Id, decimal Request_Qnty_Larg, decimal Request_Qnty_Small, decimal Request_Item_NO, long Money_Transaction_Box_Id)
        {
            try
            {
                 
                string mysql = " INSERT INTO Requests_Details (Customer_Id, Request_Date, Unit_Item_Table_Id, Request_Qnty_Larg, Request_Qnty_Small, Request_Item_NO,Money_Transaction_Box_Id) ";

                mysql = mysql + " VALUES (" + Customer_Id + ",'" + operations.dateFormat(Request_Date) + "'," + Unit_Item_Table_Id + "," + Request_Qnty_Larg + "," + Request_Qnty_Small + "," + Request_Item_NO + "," + Money_Transaction_Box_Id + ") ";
                operations opp = new operations();
                opp.insert_me(mysql, ref Excp);
                opp = null;
                Excp = 0;

            }
            catch (Exception e)
            {
                Excp = 1;
                Msg = e.Message;
                throw (e);
            }
        }
        public DataSet Get_Order_Money_Value(DateTime Order_Date, long  Customer_Id)
        {
            int Excp = 0;
            string mysql = "SELECT Money_Transaction_Box.Import_Value, Money_Transaction_Box.Export_Value, Order_Money.Order_No, Order_Money.Order_Date,  Money_Transaction_Box.Transaction_Value, Money_Transaction_Box.Order_Mony_Id, Money_Transaction_Box.Id AS Money_Transaction_Box_Id ";
            mysql = mysql + " FROM Money_Transaction_Box INNER JOIN  Order_Money ON Money_Transaction_Box.Order_Mony_Id = Order_Money.Id ";
            mysql = mysql + " WHERE (Order_Money.Order_Date = '" + operations.dateFormat(Order_Date) + "') AND (Money_Transaction_Box.Customer_Id = " + Customer_Id + ") AND (Money_Transaction_Box.Transaction_Type = 1) ";
            mysql = mysql + " ORDER BY Order_Money.Order_Date DESC";

           
            string Msg = "";
            //ref Excp,ref Msg,
            return Get_Requests_Items_Data(ref Excp, ref Msg, mysql);

        }

        public DataSet Get_Request_ListWith_Order_Money_Value(long Customer_Id, DateTime dtFrom, DateTime dtTo)
        {
            
            string mysql = " SELECT Money_Transaction_Box.Id AS Money_Transaction_Box_Id,Money_Transaction_Box.Import_Value, Money_Transaction_Box.Export_Value, Money_Transaction_Box.Transaction_Value, Requests_Details.Request_Date, Money_Transaction_Box.Order_Mony_Id, Requests_Details.Customer_Id ";
            mysql = mysql + " FROM Order_Money INNER JOIN  Money_Transaction_Box ON Order_Money.Id = Money_Transaction_Box.Order_Mony_Id INNER JOIN  Requests_Details ON Money_Transaction_Box.Id = Requests_Details.Money_Transaction_Box_Id ";
            mysql = mysql + " WHERE (Requests_Details.Money_Transaction_Box_Id <> - 1) AND (Requests_Details.Customer_Id = " + Customer_Id + ") ";
            mysql = mysql + "  AND (Requests_Details.Request_Date>='" + operations.dateFormat(dtFrom) + "') AND (Requests_Details.Request_Date>='" + operations.dateFormat(dtTo) + "' ) ";
            mysql = mysql + " ORDER BY Requests_Details.Request_Date DESC ";

            int Excp = 0;
            string Msg = "";
            //ref Excp,ref Msg,
            return Get_Requests_Items_Data(ref Excp, ref Msg, mysql);

        }
        public DataSet Get_Sum_All_Requests_Items_Balance(ref int Excp, long  Customer_Id)
        {
            string mysql;
            mysql = "  SELECT SUM(Requests_Details.Request_Qnty_Larg) AS Request_Qnty_Larg, SUM(Requests_Details.Request_Qnty_Small) AS Request_Qnty_Small,  SUM(Requests_Details.Request_Item_NO) AS Request_Item_NO, SUM(Requests_Details.Export_Qnty_Larg) AS Export_Qnty_Larg,  SUM(Requests_Details.Export_Qnty_Small) AS Export_Qnty_Small, SUM(Requests_Details.Export_Item_No) AS Export_Item_No,  Requests_Details.Unit_Item_Table_Id, Item.Item_Name , unit.u_name, unit.B_Unit, unit.S_unit, unit.B_Unit_Value, unit.S_unit_Value";
            mysql = mysql + "   FROM Requests_Details INNER JOIN  Unit_item ON Requests_Details.Unit_Item_Table_Id = Unit_item.id INNER JOIN  Item ON Unit_item.Item_id = Item.id INNER JOIN  Customer ON Requests_Details.Customer_Id = Customer.Id INNER JOIN  unit ON Unit_item.unit_id = unit.id ";
            mysql = mysql + "   WHERE (Customer_Id = " + Customer_Id + ") AND  (Item_Exported_Finish=0)  ";
            mysql = mysql + "   GROUP BY Requests_Details.Unit_Item_Table_Id, Item.Item_Name , unit.u_name, unit.B_Unit, unit.S_unit , unit.B_Unit_Value, unit.S_unit_Value";
           
            string Msg = "";
            //  ref Excp,ref Msg,
            return Get_Requests_Items_Data(ref Excp, ref Msg, mysql);

        }

        public DataSet Get_All_Requests_Items_Balance(ref int Excp, long Customer_Id, long Order_Money_Transaction_Box_Id, int Item_Exported_Finish)
        {
            string mysql;
            mysql = " SELECT Requests_Details.Id, Requests_Details.Customer_Id, Requests_Details.Request_Date, Requests_Details.Unit_Item_Table_Id,  Requests_Details.Request_Qnty_Larg, Requests_Details.Request_Item_NO, Requests_Details.Request_Qnty_Small,  Requests_Details.Export_Qnty_Larg, Requests_Details.Export_Qnty_Small, Requests_Details.Export_Item_No, Item.Item_Name,  Customer.Name AS Customer_Name, Unit_item.S_price , Unit_item.Item_id, Unit_item.Code, Unit_item.unit_id , unit.u_name, unit.B_Unit,  unit.S_unit , unit.B_Unit_Value, unit.S_unit_Value ";
            mysql = mysql + "  FROM Requests_Details INNER JOIN  Unit_item ON Requests_Details.Unit_Item_Table_Id = Unit_item.id INNER JOIN  Item ON Unit_item.Item_id = Item.id INNER JOIN  Customer ON Requests_Details.Customer_Id = Customer.Id INNER JOIN  unit ON Unit_item.unit_id = unit.id ";
            mysql = mysql + "   WHERE (Customer_Id = " + Customer_Id + ")  ";
            if (Order_Money_Transaction_Box_Id != -1)
            {
                mysql = mysql + " AND  Money_Transaction_Box_Id=" + Order_Money_Transaction_Box_Id;
            }
            if (Item_Exported_Finish >= 0)
            {
                mysql = mysql + " AND (Item_Exported_Finish=" + Item_Exported_Finish + ")  ";
            }
            
            string Msg = "";
            //  ref Excp,ref Msg,
            return Get_Requests_Items_Data(ref Excp, ref Msg, mysql);
        }

        public DataSet Get_Requests_Items(ref int Excp, DateTime dateDay, long  Customer_Id)
        {
            string mysql;
            mysql = " SELECT DISTINCT  Requests_Details.Id, Requests_Details.Customer_Id, Requests_Details.Request_Date, Requests_Details.Unit_Item_Table_Id,  Requests_Details.Request_Qnty_Larg, Requests_Details.Request_Item_NO, Requests_Details.Request_Qnty_Small,  Requests_Details.Export_Qnty_Larg, Requests_Details.Export_Qnty_Small, Requests_Details.Export_Item_No, Item.Item_Name,  Customer.Name AS Customer_Name, unit.S_unit, unit.B_Unit ";
            mysql = mysql + "  FROM Requests_Details INNER JOIN  Unit_item ON Requests_Details.Unit_Item_Table_Id = Unit_item.id INNER JOIN  Item ON Unit_item.Item_id = Item.id INNER JOIN  Customer ON Requests_Details.Customer_Id = Customer.Id INNER JOIN  unit ON Unit_item.unit_id = unit.id ";
            mysql = mysql + "   WHERE (Customer_Id = " + Customer_Id + ") AND (Request_Date = '" + operations.dateFormat(dateDay) + "')  AND (  Item_Exported_Finish = 0)";
           
            string Msg = "";
            //  ref Excp,ref Msg,
            return Get_Requests_Items_Data(ref Excp, ref Msg, mysql);

        }

        private DataSet Get_Requests_Items_Data(ref int Excp,ref string Msg, string mysql)
        {
            G_DS.Clear();
            try
            {
                operations opp = new operations();
                G_DS = opp.return_dataset(mysql, "Requests_Details");
                Excp = 1;
                opp = null;
                return G_DS;
            }
            catch (Exception ee)
            {

                Excp = -1;
                Msg = ee.Message;
                return G_DS;
            }
        }
        public int IS_Item_SpecificInvoive = 0;


        public DataSet Get_Invoice_By_Table_Id(long Invoice_List_table_Id)
        {
            string mysql = "";
            operations operations = new operations();
            operations.insert_me("UPDATE       Invoices_Details\r\nSET Invoice_Item_Name = Item.ItemName_OnInvoice\r\nFROM Invoices_Details INNER JOIN\r\n Item ON Invoices_Details.Item_Id = Item.id\r\nWHERE(Invoices_Details.Invoice_Item_Name IS NULL)");
            //

            //Invoices_List.commission,
            mysql = @"    SELECT DISTINCT 
                         '' AS Serial_, '' AS Size_, '0' AS Make_Order, '100' AS Item_DeletedFlage, '100' AS Item_ProcessedFlage, Invoices_List.IS_Make_Sarf_Item_BalanceFromInvoice, Invoices_List.Is_InvoiceKta3y, Invoices_List.Salesman_Id, 
                         Invoices_List.commissionValue, Invoices_List.IsCommission_Value, Invoices_List.CustomerAccountAfter, Invoices_List.CustomerAccountBefor, Invoices_List.PolicaNo, Invoices_List.troodContainerNo, Invoices_List.ID, 
                         Invoices_List.AnotherPayment, Invoices_List.Invoice_Id, Customer.Name, Customer.Address, Invoices_List.Invoice_Date, Invoices_List.Invoice_DateTime, Invoices_List.Total,  
                         Invoices_List.TaxSales10, Invoices_List.ATS05, Invoices_List.Net_Total, Invoices_List.Currency_id, Invoices_List.Notes, Invoices_List.Invoice_Type, Invoices_Details.Item_Id, Invoices_Details.Unit_Item_Table_Id, 
                         Invoices_Details.Larg_Qnty, Invoices_Details.Small_Qnty,Invoices_Details.Qnty_Another,  Invoices_Details.Larg_Unit_Price, Invoices_Details.Small_Unit_Price, Invoices_Details.Qnty_value, 
                         Invoices_Details.Unit_Used_for_Price, Invoices_Details.Total AS Item_Total, Invoices_Details.Item_Net_Total_Price, Invoices_Details.Notes AS Item_Notes, Invoices_Details.Unit_Id, Item.code, Invoices_Details.Specification, 
                         Item.Item_Name, Item.AnotherName,Item.ItemName_OnInvoice, unit.u_name, unit.B_Unit, unit.S_unit, unit.B_Unit_Value, unit.S_unit_Value, unit.S_unit_Value AS Small_Unit_Value, Unit_item.Full_qua AS Available_Larg_Balance, 
                         Unit_item.D_Qua AS Available_Small_Balance, Unit_item.Item_No  , Invoices_Details.Id AS Invoices_Details_Id, Customer.Balance AS Customer_Balance, Customer.Price_Type_List_ID, Invoices_Details.Store_Id, 
                         Invoices_Details.Invoice_Item_Name, Invoices_Details.Order_Purshing_Id_Automatic, Invoices_Details.Balance_Checked, Store.Store_Name, Store.Store_Code, Store.IS_Default_Store, 
                         Invoices_Details.Item_CommissionValue, Invoices_Details.Unit_CommissionValue, Invoices_Details.Is_Item_CommissionValue, Invoices_Details.QntyWidth, Invoices_Details.QntyLenght, Invoices_Details.QntyItemNo, 
                         Invoices_Details.OperationNo, Invoices_Details.Merchant_Name, Item.Somk_Unit, Item.Somk, Item.Height_, Item.Width_, Item.Items_Types_Name, Item.Items_Types_Name_Id, Item.Height_Width_Str, 
                         Item.Items_Name_Somk_Str, Item.User_Id, Item.OperationDate, Customer.Tax_No, Invoices_List.Total_PriceWithoutTax, Invoices_Details.Tash3ela, Invoices_Details.Item_No_Free, Invoices_Details.LargItemsQnty_Free, 
                         Invoices_Details.SmallItemsQnty_Free, Invoices_Details.Item_Expire_Date, Invoices_Details.Is_Alternate_Item_CommissionValue, Invoices_Details.Alternate_Item_CommissionValue, 
                         Invoices_Details.Alternate_ItemCommissionTotal, Invoices_Details.Net_Total_Item_Commission, Invoices_Details.Alternate_Item_CommissionRate, Invoices_Details.Item_CommissionRate, 
                         Invoices_Details.Alternate_Unit_CommissionValue, Invoices_Details.Unit_Item_ExpireDate_Id, Invoices_Details.ItemCommissionTotal, Invoices_Details.Audience_price, Invoices_Details.NameSpecification, 
                         Invoices_Details.StatementNo, Invoices_Details.Item_Total_WithoutTax, Invoices_List.Customer_Id, Invoices_Details.QntyStrColumn, Invoices_Details.QntyStr, Unit_item.Reserved_Balance_Larg_Qnty, 
                         Unit_item.Reserved_Balance_Small_Qnty, Invoices_List.TaxSales10_Value, Invoices_List.ATS05_Value, Invoices_Details.Item_TaxSales10, Invoices_Details.Item_TaxSales10_Value, Invoices_Details.Item_ATS05, 
                         Invoices_Details.Item_ATS05_Value, Invoices_List.List_CommissionValue, Invoices_List.List_CommissionRate, Invoices_List.List_Final_CommissionValue, Invoices_List.Net_Total_CommissionValue, 
                         Invoices_List.Equivalent_Net_Total_CommissionValue, Invoices_List.Is_Tax_Per_Item, Invoices_Details.Bills_Details_Id, Invoices_Details.Purchase_Price, Invoices_Details.Is_InvoiceItem_Kta3y, 
                         Invoices_List.Is_Buy_From_Bills_Items_Balance,Invoices_List.Order_MoneyId,Invoices_List.BanksId,
                            Invoices_List.BanksNamePaymentMethod  ,Invoices_List.PaidValue  ,Invoices_List.RestValue
                        ,Invoices_List.Customer_Notes,Invoices_Details.TotalSmall_QntyStrColumn 
                        ,Invoices_Details.TotalSmall_QntyStr,Invoices_Details.TotalSmall_Qnty
                        ,Invoices_List.Is_Invoice_Agile,Invoices_List.Emp_Id,Invoices_List.Emp_Name
                        ,Invoices_List.ShipmentCompany_Id,Invoices_List.ShipmentCompany_Name,Invoices_List.Shipment_Value
                        ,  Item.No3eboa, Item.No3eboa_BigUnit, Item.No3eboa_SmallUnit ,  Invoices_Details.Unit3Mola, Invoices_Details.Total3Mola 
                        ,Invoices_List.Total_Item_CommissionValue ,Invoices_List.Delivery_Fee,Invoices_List.Service_Amount,Invoices_List.Service_Rate,Invoices_List.IsDeliveryFeeAddedToInvoiceTotalAfterTax,Invoices_List.IsDeliveryFeeAddedToInvoiceTotal,Invoices_List.PaidValueFromCashNakdy,Invoices_List.RestValueFromCashNakdia
                        ,Invoices_List.NetInvoiceTotal , Invoices_List.Delivery_Address ,Invoices_List.CustomerPhone,Invoices_List.CustomerDeliveryName,Invoices_List.Lookup_OrderDeliveryTypesId
FROM            unit INNER JOIN
                         Invoices_List INNER JOIN
                         Invoices_Details ON Invoices_List.ID = Invoices_Details.Invoice_Id INNER JOIN
                         Item INNER JOIN
                         Unit_item ON Item.Id = Unit_item.Item_id ON Invoices_Details.Unit_Item_Table_Id = Unit_item.Id ON unit.id = Unit_item.unit_id INNER JOIN
                         Store ON Unit_item.Store_id = Store.id   INNER JOIN
                         Customer ON Invoices_List.Customer_Id = Customer.Id
                ";


            mysql = mysql + " WHERE (Invoices_List.ID = " + Invoice_List_table_Id.ToString() + ") Order by Invoices_Details.ID  ";
            this.G_DS.Clear();
            try
            {
                this.G_DS = operations.return_dataset(mysql, "Invoices_List");
                int c = G_DS.Tables[0].Rows.Count;
                operations = null;
                return this.G_DS;
            }
            catch
            {
                return this.G_DS;
            }
        }

        public DataSet Get_InvoiceTemp_By_Table_Id(long Invoice_List_table_Id)
        {
            string mysql = "";
            operations operations = new operations();

            operations.insert_me("UPDATE       Invoices_DetailsTemp SET Invoice_Item_Name = Item.ItemName_OnInvoice FROM Invoices_DetailsTemp INNER JOIN  Item ON Invoices_DetailsTemp.Item_Id = Item.id WHERE(Invoices_DetailsTemp.Invoice_Item_Name IS NULL)");
            //

            //Invoices_ListTemp.commission,
            mysql = @"    SELECT DISTINCT 
                         '' AS Serial_, '' AS Size_, '0' AS Make_Order, '100' AS Item_DeletedFlage, '100' AS Item_ProcessedFlage, Invoices_ListTemp.IS_Make_Sarf_Item_BalanceFromInvoice, Invoices_ListTemp.Is_InvoiceKta3y, Invoices_ListTemp.Salesman_Id, 
                         Invoices_ListTemp.commissionValue, Invoices_ListTemp.IsCommission_Value, Invoices_ListTemp.CustomerAccountAfter, Invoices_ListTemp.CustomerAccountBefor, Invoices_ListTemp.PolicaNo, Invoices_ListTemp.troodContainerNo, Invoices_ListTemp.ID, 
                         Invoices_ListTemp.AnotherPayment, Invoices_ListTemp.Invoice_Id, Customer.Name, Customer.Address, Invoices_ListTemp.Invoice_Date, Invoices_ListTemp.Invoice_DateTime, Invoices_ListTemp.Total,  
                         Invoices_ListTemp.TaxSales10, Invoices_ListTemp.ATS05, Invoices_ListTemp.Net_Total, Invoices_ListTemp.Currency_id, Invoices_ListTemp.Notes, Invoices_ListTemp.Invoice_Type, Invoices_DetailsTemp.Item_Id, Invoices_DetailsTemp.Unit_Item_Table_Id, 
                         Invoices_DetailsTemp.Larg_Qnty, Invoices_DetailsTemp.Small_Qnty,Invoices_DetailsTemp.Qnty_Another,  Invoices_DetailsTemp.Larg_Unit_Price, Invoices_DetailsTemp.Small_Unit_Price, Invoices_DetailsTemp.Qnty_value, 
                         Invoices_DetailsTemp.Unit_Used_for_Price, Invoices_DetailsTemp.Total AS Item_Total, Invoices_DetailsTemp.Item_Net_Total_Price, Invoices_DetailsTemp.Notes AS Item_Notes, Invoices_DetailsTemp.Unit_Id, Item.code, Invoices_DetailsTemp.Specification, 
                         Item.Item_Name, Item.AnotherName,Item.ItemName_OnInvoice, unit.u_name, unit.B_Unit, unit.S_unit, unit.B_Unit_Value, unit.S_unit_Value, unit.S_unit_Value AS Small_Unit_Value, Unit_item.Full_qua AS Available_Larg_Balance, 
                         Unit_item.D_Qua AS Available_Small_Balance, Unit_item.Item_No  , Invoices_DetailsTemp.Id AS Invoices_Details_Id, Customer.Balance AS Customer_Balance, Customer.Price_Type_List_ID, Invoices_DetailsTemp.Store_Id, 
                         Invoices_DetailsTemp.Invoice_Item_Name, Invoices_DetailsTemp.Order_Purshing_Id_Automatic, Invoices_DetailsTemp.Balance_Checked, Store.Store_Name, Store.Store_Code, Store.IS_Default_Store, 
                         Invoices_DetailsTemp.Item_CommissionValue, Invoices_DetailsTemp.Unit_CommissionValue, Invoices_DetailsTemp.Is_Item_CommissionValue, Invoices_DetailsTemp.QntyWidth, Invoices_DetailsTemp.QntyLenght, Invoices_DetailsTemp.QntyItemNo, 
                         Invoices_DetailsTemp.OperationNo, Invoices_DetailsTemp.Merchant_Name, Item.Somk_Unit, Item.Somk, Item.Height_, Item.Width_, Item.Items_Types_Name, Item.Items_Types_Name_Id, Item.Height_Width_Str, 
                         Item.Items_Name_Somk_Str, Item.User_Id, Item.OperationDate, Customer.Tax_No, Invoices_ListTemp.Total_PriceWithoutTax, Invoices_DetailsTemp.Tash3ela, Invoices_DetailsTemp.Item_No_Free, Invoices_DetailsTemp.LargItemsQnty_Free, 
                         Invoices_DetailsTemp.SmallItemsQnty_Free, Invoices_DetailsTemp.Item_Expire_Date, Invoices_DetailsTemp.Is_Alternate_Item_CommissionValue, Invoices_DetailsTemp.Alternate_Item_CommissionValue, 
                         Invoices_DetailsTemp.Alternate_ItemCommissionTotal, Invoices_DetailsTemp.Net_Total_Item_Commission, Invoices_DetailsTemp.Alternate_Item_CommissionRate, Invoices_DetailsTemp.Item_CommissionRate, 
                         Invoices_DetailsTemp.Alternate_Unit_CommissionValue, Invoices_DetailsTemp.Unit_Item_ExpireDate_Id, Invoices_DetailsTemp.ItemCommissionTotal, Invoices_DetailsTemp.Audience_price, Invoices_DetailsTemp.NameSpecification, 
                         Invoices_DetailsTemp.StatementNo, Invoices_DetailsTemp.Item_Total_WithoutTax, Invoices_ListTemp.Customer_Id, Invoices_DetailsTemp.QntyStrColumn, Invoices_DetailsTemp.QntyStr, Unit_item.Reserved_Balance_Larg_Qnty, 
                         Unit_item.Reserved_Balance_Small_Qnty, Invoices_ListTemp.TaxSales10_Value, Invoices_ListTemp.ATS05_Value, Invoices_DetailsTemp.Item_TaxSales10, Invoices_DetailsTemp.Item_TaxSales10_Value, Invoices_DetailsTemp.Item_ATS05, 
                         Invoices_DetailsTemp.Item_ATS05_Value, Invoices_ListTemp.List_CommissionValue, Invoices_ListTemp.List_CommissionRate, Invoices_ListTemp.List_Final_CommissionValue, Invoices_ListTemp.Net_Total_CommissionValue, 
                         Invoices_ListTemp.Equivalent_Net_Total_CommissionValue, Invoices_ListTemp.Is_Tax_Per_Item, Invoices_DetailsTemp.Bills_Details_Id, Invoices_DetailsTemp.Purchase_Price, Invoices_DetailsTemp.Is_InvoiceItem_Kta3y, 
                         Invoices_ListTemp.Is_Buy_From_Bills_Items_Balance,Invoices_ListTemp.Order_MoneyId,Invoices_ListTemp.BanksId,
                            Invoices_ListTemp.BanksNamePaymentMethod  ,Invoices_ListTemp.PaidValue  ,Invoices_ListTemp.RestValue
                        ,Invoices_ListTemp.Customer_Notes,Invoices_DetailsTemp.TotalSmall_QntyStrColumn 
                        ,Invoices_DetailsTemp.TotalSmall_QntyStr,Invoices_DetailsTemp.TotalSmall_Qnty
                        ,Invoices_ListTemp.Is_Invoice_Agile,Invoices_ListTemp.Emp_Id,Invoices_ListTemp.Emp_Name
                        ,Invoices_ListTemp.ShipmentCompany_Id,Invoices_ListTemp.ShipmentCompany_Name,Invoices_ListTemp.Shipment_Value
                        ,  Item.No3eboa, Item.No3eboa_BigUnit, Item.No3eboa_SmallUnit ,  Invoices_DetailsTemp.Unit3Mola, Invoices_DetailsTemp.Total3Mola 
            ,Invoices_ListTemp.Total_Item_CommissionValue ,Invoices_ListTemp.Delivery_Fee,Invoices_ListTemp.Service_Amount,Invoices_ListTemp.Service_Rate,Invoices_ListTemp.IsDeliveryFeeAddedToInvoiceTotalAfterTax,Invoices_ListTemp.IsDeliveryFeeAddedToInvoiceTotal,Invoices_ListTemp.PaidValueFromCashNakdy,Invoices_ListTemp.RestValueFromCashNakdia
                        ,Invoices_ListTemp.NetInvoiceTotal , Invoices_ListTemp.Delivery_Address ,Invoices_ListTemp.CustomerPhone,Invoices_ListTemp.CustomerDeliveryName,Invoices_List.Lookup_OrderDeliveryTypesId
       
FROM Item INNER JOIN
                         Unit_item ON Item.id = Unit_item.Item_id INNER JOIN
                         Invoices_ListTemp INNER JOIN
                         Customer ON Invoices_ListTemp.Customer_Id = Customer.Id INNER JOIN
                         Invoices_DetailsTemp ON Invoices_ListTemp.ID = Invoices_DetailsTemp.Invoice_Id ON Unit_item.id = Invoices_DetailsTemp.Unit_Item_Table_Id INNER JOIN
                         unit ON Unit_item.unit_id = unit.id INNER JOIN
                         Store ON Unit_item.Store_id = Store.id
                ";


            mysql = mysql + " WHERE (Invoices_ListTemp.ID = " + Invoice_List_table_Id.ToString() + ") Order by Invoices_DetailsTemp.ID  ";
            this.G_DS.Clear();
            try
            {
                this.G_DS = operations.return_dataset(mysql, "Invoices_List");
                operations = null;
                return this.G_DS;
            }
            catch
            {
                return this.G_DS;
            }
        }
        public DataSet Get_Invoice_By_Table_Id_Sum(long Invoice_List_table_Id)
        {
            string mysql = "";
            operations operations = new operations();
            operations.insert_me("UPDATE       Invoices_Details\r\nSET Invoice_Item_Name = Item.Item_Name\r\nFROM Invoices_Details INNER JOIN\r\n Item ON Invoices_Details.Item_Id = Item.id\r\nWHERE(Invoices_Details.Invoice_Item_Name IS NULL)");

            //

            mysql = @" SELECT DISTINCT '' AS QntyStrColumn, '' AS Serial_,
  Invoices_List.IS_Make_Sarf_Item_BalanceFromInvoice, Customer.Address, Customer.Tax_No,

                         Invoices_List.ID, Invoices_List.Is_InvoiceKta3y, Invoices_List.Salesman_Id, Invoices_List.commissionValue, Invoices_List.IsCommission_Value, Invoices_List.CustomerAccountAfter, Invoices_List.CustomerAccountBefor, 
                         Invoices_List.PolicaNo, Invoices_List.troodContainerNo, Invoices_List.Invoice_Id, Customer.Name, Invoices_List.Invoice_Date, Invoices_List.Invoice_DateTime, Invoices_List.Total, Invoices_List.commission, 
                         Invoices_List.TaxSales10, Invoices_List.ATS05, Invoices_List.Net_Total, Invoices_List.Currency_id, Invoices_List.Notes, Invoices_List.Invoice_Type, Item.code, Item.Item_Name, Item.AnotherName,Item.ItemName_OnInvoice, unit.u_name, unit.B_Unit, 
                         unit.S_unit, unit.B_Unit_Value, unit.S_unit_Value, unit.S_unit_Value AS Small_Unit_Value, Item.Somk_Unit, Item.Somk, Item.Height_, Item.Width_, Item.Items_Types_Name, Item.Items_Types_Name_Id, Item.Height_Width_Str, 
                         Item.Items_Name_Somk_Str, Item.User_Id, Item.OperationDate, Invoices_List.Total_PriceWithoutTax, Invoices_List.Customer_Id, Invoices_List.TaxSales10_Value, Invoices_List.ATS05_Value, 
                         Invoices_List.List_CommissionValue, Invoices_List.List_CommissionRate, Invoices_List.List_Final_CommissionValue, Invoices_List.Net_Total_CommissionValue, Invoices_List.Equivalent_Net_Total_CommissionValue, 
                         Invoices_List.Is_Buy_From_Bills_Items_Balance, Invoices_List.Is_Tax_Per_Item, SUM(Invoices_Details.Item_CommissionValue) AS Item_CommissionValue, SUM(Invoices_Details.Unit_CommissionValue) 
                         AS Unit_CommissionValue, SUM(Invoices_Details.Item_No_Free) AS Item_No_Free, SUM(Invoices_Details.LargItemsQnty_Free) AS LargItemsQnty_Free, SUM(Invoices_Details.SmallItemsQnty_Free) AS SmallItemsQnty_Free, 
                         SUM(Invoices_Details.Alternate_Item_CommissionValue) AS Alternate_Item_CommissionValue, SUM(Invoices_Details.Alternate_ItemCommissionTotal) AS Alternate_ItemCommissionTotal, 
                         SUM(Invoices_Details.Net_Total_Item_Commission) AS Net_Total_Item_Commission, SUM(Invoices_Details.Alternate_Item_CommissionRate) AS Alternate_Item_CommissionRate, 
                         SUM(Invoices_Details.Item_CommissionRate) AS Item_CommissionRate, SUM(Invoices_Details.Alternate_Unit_CommissionValue) AS Alternate_Unit_CommissionValue, Invoices_Details.Invoice_Item_Name, 
                         SUM(Invoices_Details.Item_TaxSales10) AS Item_TaxSales10, SUM(Invoices_Details.Item_TaxSales10_Value) AS Item_TaxSales10_Value, SUM(Invoices_Details.Item_ATS05) AS Item_ATS05, 
                         SUM(Invoices_Details.ItemCommissionTotal) AS ItemCommissionTotal, SUM(Invoices_Details.Item_ATS05_Value) AS Item_ATS05_Value, SUM(Invoices_Details.Item_Total_WithoutTax) AS Item_Total_WithoutTax, 
                         Invoices_Details.Item_Id, SUM(Invoices_Details.Larg_Qnty) AS Larg_Qnty, SUM(Invoices_Details.Small_Qnty) AS Small_Qnty, SUM(Invoices_Details.Qnty_Another) AS Qnty_Another, Invoices_Details.Larg_Unit_Price, 
                         Invoices_Details.Small_Unit_Price, SUM(Invoices_Details.Qnty_value) AS Qnty_value, SUM(Invoices_Details.Item_Net_Total_Price) AS Item_Net_Total_Price, Invoices_Details.Unit_Id, SUM(Invoices_Details.Total) 
                         AS Item_Total,  Invoices_Details.Unit3Mola, SUM(Invoices_Details.Total3Mola) AS Total3Mola
FROM            Item INNER JOIN
                         Unit_item ON Item.id = Unit_item.Item_id INNER JOIN
                         Invoices_List INNER JOIN
                         Customer ON Invoices_List.Customer_Id = Customer.Id INNER JOIN
                         Invoices_Details ON Invoices_List.ID = Invoices_Details.Invoice_Id ON Unit_item.id = Invoices_Details.Unit_Item_Table_Id INNER JOIN
                         unit ON Unit_item.unit_id = unit.id INNER JOIN
                         Store ON Unit_item.Store_id = Store.id
GROUP BY Invoices_List.Is_InvoiceKta3y, Invoices_List.Salesman_Id, Invoices_List.commissionValue, Invoices_List.IsCommission_Value, Invoices_List.CustomerAccountAfter, Invoices_List.CustomerAccountBefor, 
                         Invoices_List.PolicaNo, Invoices_List.troodContainerNo, Invoices_List.ID, Invoices_List.Invoice_Id, Customer.Name, Invoices_List.Invoice_Date, Invoices_List.Invoice_DateTime, Invoices_List.Total, Invoices_List.commission, 
                         Invoices_List.TaxSales10, Invoices_List.ATS05, Invoices_List.Net_Total, Invoices_List.Currency_id, Invoices_List.Notes, Invoices_List.Invoice_Type, Item.code, Item.Item_Name, Item.AnotherName, unit.u_name, unit.B_Unit, 
                         unit.S_unit, unit.B_Unit_Value, unit.S_unit_Value, Item.Somk_Unit, Item.Somk, Item.Height_, Item.Width_, Item.Items_Types_Name, Item.Items_Types_Name_Id, Item.Height_Width_Str, Item.Items_Name_Somk_Str, 
                         Item.User_Id, Item.OperationDate, Invoices_List.Total_PriceWithoutTax, Invoices_List.Customer_Id, Invoices_List.TaxSales10_Value, Invoices_List.ATS05_Value, Invoices_List.List_CommissionValue, 
                         Invoices_List.List_CommissionRate, Invoices_List.List_Final_CommissionValue, Invoices_List.Net_Total_CommissionValue, Invoices_List.Equivalent_Net_Total_CommissionValue, 
                         Invoices_List.Is_Buy_From_Bills_Items_Balance, Invoices_List.Is_Tax_Per_Item, Invoices_Details.Invoice_Item_Name, Invoices_Details.Item_Id, Invoices_Details.Larg_Unit_Price, Invoices_Details.Small_Unit_Price, 
                         Invoices_Details.Unit_Id,  Invoices_List.IS_Make_Sarf_Item_BalanceFromInvoice, Customer.Address, Customer.Tax_No,Invoices_Details.Unit3Mola

 ";


            mysql = mysql + " HAVING (Invoices_List.ID = " + Invoice_List_table_Id.ToString() + ") ";
            this.G_DS.Clear();
            try
            {
                this.G_DS = operations.return_dataset(mysql, "Invoices_List");
                operations = null;
                return this.G_DS;
            }
            catch
            {
                return this.G_DS;
            }
        }
        public DataSet Get_Invoice_Reverse_By_Table_Id(long Invoice_List_table_Id)
        {
            string mysql = "";
            operations operations = new operations();
            // operations.insert_me("UPDATE       Invoices_Details_Reverse\r\nSET Invoice_Item_Name = Item.Item_Name\r\nFROM Invoices_Details_Reverse INNER JOIN\r\n                         Item ON Invoices_Details_Reverse.Item_Id = Item.id\r\nWHERE(Invoices_Details_Reverse.Invoice_Item_Name IS NULL)");
            mysql = @"    SELECT DISTINCT 
                         '' AS Serial_, '' AS Size_, '0' AS Make_Order, '100' AS Item_DeletedFlage, '100' AS Item_ProcessedFlage, Invoices_List_Reverse.IS_Make_Sarf_Item_BalanceFromInvoice, Invoices_List_Reverse.Is_InvoiceKta3y, 
                         Invoices_List_Reverse.Salesman_Id, Invoices_List_Reverse.commissionValue, Invoices_List_Reverse.IsCommission_Value, Invoices_List_Reverse.CustomerAccountAfter, Invoices_List_Reverse.CustomerAccountBefor, 
                         Invoices_List_Reverse.PolicaNo, Invoices_List_Reverse.troodContainerNo, Invoices_List_Reverse.ID, Invoices_List_Reverse.AnotherPayment, Invoices_List_Reverse.Invoice_Id, Customer.Name, Customer.Address, 
                         Invoices_List_Reverse.Invoice_Date, Invoices_List_Reverse.Invoice_DateTime, Invoices_List_Reverse.Total, Invoices_List_Reverse.TaxSales10, Invoices_List_Reverse.ATS05, 
                         Invoices_List_Reverse.Net_Total, Invoices_List_Reverse.Currency_id, Invoices_List_Reverse.Notes, Invoices_List_Reverse.Invoice_Type, Invoices_Details_Reverse.Item_Id, Invoices_Details_Reverse.Unit_Item_Table_Id, 
                         Invoices_Details_Reverse.Larg_Qnty, Invoices_Details_Reverse.Small_Qnty,  Invoices_Details_Reverse.Qnty_Another, Invoices_Details_Reverse.Larg_Unit_Price, Invoices_Details_Reverse.Small_Unit_Price, 
                         Invoices_Details_Reverse.Qnty_value, Invoices_Details_Reverse.Unit_Used_for_Price, Invoices_Details_Reverse.Total AS Item_Total, Invoices_Details_Reverse.Item_Net_Total_Price, 
                         Invoices_Details_Reverse.Notes AS Item_Notes, Invoices_Details_Reverse.Unit_Id, Item.code, Invoices_Details_Reverse.Specification, Item.Item_Name, Item.AnotherName, unit.u_name, unit.B_Unit, unit.S_unit, 
                         unit.B_Unit_Value, unit.S_unit_Value, unit.S_unit_Value AS Small_Unit_Value, Unit_item.Full_qua AS Available_Larg_Balance, Unit_item.D_Qua AS Available_Small_Balance, Unit_item.Item_No  , 
                         Invoices_Details_Reverse.Id AS Invoices_Details_Reverse_Id, Customer.Balance AS Customer_Balance, Customer.Price_Type_List_ID, Invoices_Details_Reverse.Store_Id, Invoices_Details_Reverse.Invoice_Item_Name, 
                         Invoices_Details_Reverse.Order_Purshing_Id_Automatic, Invoices_Details_Reverse.Balance_Checked, Store.Store_Name, Store.Store_Code, Store.IS_Default_Store, Invoices_Details_Reverse.Item_CommissionValue, 
                         Invoices_Details_Reverse.Unit_CommissionValue, Invoices_Details_Reverse.Is_Item_CommissionValue, Invoices_Details_Reverse.QntyWidth, Invoices_Details_Reverse.QntyLenght, Invoices_Details_Reverse.QntyItemNo, 
                         Invoices_Details_Reverse.OperationNo, Invoices_Details_Reverse.Merchant_Name, Item.Somk_Unit, Item.Somk, Item.Height_, Item.Width_, Item.Items_Types_Name, Item.Items_Types_Name_Id, Item.Height_Width_Str, 
                         Item.Items_Name_Somk_Str, Item.User_Id, Item.OperationDate, Customer.Tax_No, Invoices_List_Reverse.Total_PriceWithoutTax, Invoices_Details_Reverse.Tash3ela, Invoices_Details_Reverse.Item_No_Free, 
                         Invoices_Details_Reverse.LargItemsQnty_Free, Invoices_Details_Reverse.SmallItemsQnty_Free, Invoices_Details_Reverse.Item_Expire_Date, Invoices_Details_Reverse.Is_Alternate_Item_CommissionValue, 
                         Invoices_Details_Reverse.Alternate_Item_CommissionValue, Invoices_Details_Reverse.Alternate_ItemCommissionTotal, Invoices_Details_Reverse.Net_Total_Item_Commission, 
                         Invoices_Details_Reverse.Alternate_Item_CommissionRate, Invoices_Details_Reverse.Item_CommissionRate, Invoices_Details_Reverse.Alternate_Unit_CommissionValue, Invoices_Details_Reverse.Unit_Item_ExpireDate_Id, 
                         Invoices_Details_Reverse.ItemCommissionTotal, Invoices_Details_Reverse.Audience_price, Invoices_Details_Reverse.NameSpecification, Invoices_Details_Reverse.StatementNo, 
                         Invoices_Details_Reverse.Item_Total_WithoutTax, Invoices_List_Reverse.Customer_Id, Invoices_Details_Reverse.QntyStrColumn, Invoices_Details_Reverse.QntyStr, Unit_item.Reserved_Balance_Larg_Qnty, 
                         Unit_item.Reserved_Balance_Small_Qnty, Invoices_List_Reverse.TaxSales10_Value, Invoices_List_Reverse.ATS05_Value, Invoices_Details_Reverse.Item_TaxSales10, Invoices_Details_Reverse.Item_TaxSales10_Value, 
                         Invoices_Details_Reverse.Item_ATS05, Invoices_Details_Reverse.Item_ATS05_Value, Invoices_List_Reverse.List_CommissionValue, Invoices_List_Reverse.List_CommissionRate, 
                         Invoices_List_Reverse.List_Final_CommissionValue, Invoices_List_Reverse.Net_Total_CommissionValue, Invoices_List_Reverse.Equivalent_Net_Total_CommissionValue, Invoices_List_Reverse.Is_Tax_Per_Item, 
                         Invoices_Details_Reverse.Bills_Details_Id, Invoices_Details_Reverse.Purchase_Price, Invoices_Details_Reverse.Is_InvoiceItem_Kta3y, Invoices_List_Reverse.Is_Buy_From_Bills_Items_Balance

                        ,Invoices_List_Reverse.Customer_Notes,Invoices_Details_Reverse.TotalSmall_QntyStrColumn 
                        ,Invoices_Details_Reverse.TotalSmall_QntyStr,Invoices_Details_Reverse.TotalSmall_Qnty
                        ,Invoices_List_Reverse.Is_Invoice_Agile,Invoices_List_Reverse.Emp_Id,Invoices_List_Reverse.Emp_Name
 ,Invoices_List_Reverse.ShipmentCompany_Id,Invoices_List_Reverse.ShipmentCompany_Name,Invoices_List_Reverse.Shipment_Value
 


,  Item.No3eboa, Item.No3eboa_BigUnit, Item.No3eboa_SmallUnit ,  Invoices_Details_Reverse.Unit3Mola, Invoices_Details_Reverse.Total3Mola 
                        ,Invoices_List_Reverse.Total_Item_CommissionValue ,Invoices_List_Reverse.Delivery_Fee,Invoices_List_Reverse.Service_Amount,Invoices_List_Reverse.Service_Rate,Invoices_List_Reverse.IsDeliveryFeeAddedToInvoiceTotalAfterTax,Invoices_List_Reverse.IsDeliveryFeeAddedToInvoiceTotal,Invoices_List_Reverse.PaidValueFromCashNakdy,Invoices_List_Reverse.RestValueFromCashNakdia
                        ,Invoices_List_Reverse.NetInvoiceTotal , Invoices_List_Reverse.Delivery_Address ,Invoices_List_Reverse.CustomerPhone,Invoices_List_Reverse.CustomerDeliveryName,Invoices_List_Reverse.Lookup_OrderDeliveryTypesId


FROM            Item INNER JOIN
                         Unit_item ON Item.id = Unit_item.Item_id INNER JOIN
                         Invoices_List_Reverse INNER JOIN
                         Customer ON Invoices_List_Reverse.Customer_Id = Customer.Id INNER JOIN
                         Invoices_Details_Reverse ON Invoices_List_Reverse.ID = Invoices_Details_Reverse.Invoice_Id ON Unit_item.id = Invoices_Details_Reverse.Unit_Item_Table_Id INNER JOIN
                         unit ON Unit_item.unit_id = unit.id INNER JOIN
                         Store ON Unit_item.Store_id = Store.id
                ";


            mysql = mysql + " WHERE (Invoices_List_Reverse.ID = " + Invoice_List_table_Id.ToString() + ") Order by  Invoices_Details_Reverse.ID  ";
            this.G_DS.Clear();
            try
            {
                this.G_DS = operations.return_dataset(mysql, "Invoices_List_Reverse");
                operations = null;
                return this.G_DS;
            }
            catch
            {
                return this.G_DS;
            }
        }



        public ArrayList Get_arr_Order_Purshing_Id_Automatic_From_Invoice(long Invoice_Id)
        {
            arr_Order_Purshing_Id_Automatic = new ArrayList();
            operations opp = new operations();
            string mysql = "SELECT Invoices_Details.Order_Purshing_Id_Automatic FROM Invoices_Details WHERE (Invoice_Id = " + Invoice_Id + ")";
            DataTable dt = opp.return_DataTable(mysql);
            for (int x = 0; x < dt.Rows.Count; x++)
            {
                arr_Order_Purshing_Id_Automatic.Add(dt.Rows[x]["Order_Purshing_Id_Automatic"].ToString());
            }
            return arr_Order_Purshing_Id_Automatic;

        }
      
       
        

        public void Make_Invoice_Reverse_Update_CustomerAccount(ref int Excp, InvoicesOperationListCLS invL_O, InvoicesOperationCLS invD_O)
        {
            operations opp = new operations(1);
            try
            {
                //checked item_report to SendKeys to store
                ////DataView dvToStore = new DataView();
                //// dvToStore.Table = Invoice_ds.Tables[0];
                ////ItemToStore it = new ItemToStore(dvToStore);

                ////if (Common.Project_Market == 0)
                ////{
                ////    it.ShowDialog();
                ////}
                ////else
                ////{
                ////    it.ClickBtnOK();
                ////}
                ////ArrayList mArrMake_Order = new ArrayList();
                ////mArrMake_Order = it.ArrItemToStore;
                ////if (!it.BtnOkClick)
                ////{
                ////    Excp = 10;
                ////    return;
                ////}
                ////***********************************************************************
                ////////// Make_New_Invoice_Id
                //DataTable dt = new DataTable();
                //dt = Invoice_ds.Tables[0];

                string mysql = "";
                try
                {
                    if (invL_O.invL.CustomerName.Trim() == "")
                    {
                        invL_O.invL.CustomerName = Common.GetCustomerName(ref opp, invL_O.invL.Customer_Id ?? 0);
                    }
                }
                catch { }
                
                string Msg = "";
                New_Invoice_Id= Add_UpdateInvoice_List_Reverse(ref Excp, ref Msg, ref opp, invL_O);


                InvoiceDataSetCLS ii = new InvoiceDataSetCLS();
                for (int x = 0; x < invD_O.invDlst.Count; x++)
                {
                     
                    invD_O.invDlst[x].Invoice_Id = New_Invoice_Id;

                    //invd_O.Get_Item_Details(Invoice_ds, x);
                    try
                    {
                        if (invD_O.invDlst[x].Store_Name.Trim() == "")
                        {
                            invD_O.invDlst[x].Store_Name = Common.GetStoreName(ref opp, invD_O.invDlst[x].Store_Id ?? 0);
                        }
                    }
                    catch { }
                    
                    Invoices_Details_Reverse invoices_Details_Reverse = invD_O.Insert_Invoices_Details_Reverse(ref Excp, ref opp, invD_O.invDlst[x]);
                    long Invoices_Details_Id_ = invoices_Details_Reverse.Id;
                    try
                    {
                        //Invoice_ds.Tables[0].Rows[x]["Invoices_Details_Id"] = invD_O.invDlst[x].Id.ToString();
                    }
                    catch (Exception ee)
                    { }
                    if (Excp == 1)
                    {
                        opp.Rollback();
                        return;
                    }

                    if (Common.Project_Market == 1 & invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                    {
                        int Transaction_Type = 1;


                        //InvoiceDataSetCLS ii = new InvoiceDataSetCLS();

                        //ii.Set_ItemTransactionFromInvoices(ref opp, invd.Unit_Item_Table_Id, invd.Item_Net_Total_Price, invd.Larg_Qnty, invd.Small_Qnty, invd.Qnty_Another, 0, Transaction_Type, invL.Invoice_Date, invL.Invoices_List_Id, invd.Invoices_Details_Id, invL.Invoice_Type_Id, "›« Ê—… „— Ã⁄ „»Ì⁄« ", invd.Store_Name, invL.Customer_Name, invL.Customer_Name, invL.Invoice_No);
                        ii.Set_ItemTransactionFromInvoiceR(ref opp, invD_O.invDlst[x], invL_O, 0, Transaction_Type, Common.InvoiceReverse_Type_Id, "›« Ê—… „— Ã⁄ „»Ì⁄« ");

                    }
                    if (Common.Project_Market == 1 & invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                    {
                        mysql = " UPDATE Unit_item SET Full_qua =Full_qua + " + invD_O.invDlst[x].Larg_Qnty + ", D_Qua =D_Qua + " + invD_O.invDlst[x].Small_Qnty + ", Item_No =Item_No + " + invD_O.invDlst[x].Qnty_Another + " WHERE (id = " + invD_O.invDlst[x].Unit_Item_Table_Id + ") ";
                        opp.insert_me(mysql);

                        //// «·»Ê‰’
                        mysql = " UPDATE Unit_item SET Full_qua =Full_qua + " + invD_O.invDlst[x].LargItemsQnty_Free + ", D_Qua =D_Qua + " + invD_O.invDlst[x].SmallItemsQnty_Free + ", Item_No =Item_No + " + invD_O.invDlst[x].Item_No_Free + " WHERE (id = " + invD_O.invDlst[x].Unit_Item_Table_Id + ") ";
                        opp.insert_me(mysql);

                        //// «·»Ê‰’
                        mysql = " UPDATE Bills_Details   SET Larg_Qnty_Balance = Larg_Qnty_Balance + " + invD_O.invDlst[x].LargItemsQnty_Free + ", Small_Qnty_Balance =Small_Qnty_Balance + " + invD_O.invDlst[x].SmallItemsQnty_Free + ", Qnty_Another_Balance =Qnty_Another_Balance + " + invD_O.invDlst[x].Item_No_Free + " WHERE (id = " + invD_O.invDlst[x].Bills_Details_Id + ") ";
                        opp.insert_me(mysql);
                        //mysql = "  UPDATE Unit_item SET Full_qua = FLOOR((Small_unit * Full_qua + D_Qua) / Small_unit), ";
                        //mysql = mysql + " D_Qua =   (( Small_unit * Full_qua + D_Qua) / Small_unit - FLOOR(( Small_unit * Full_qua + D_Qua) /  Small_unit)) *  Small_unit  ";
                        //mysql = mysql + " WHERE (id = " + Unit_Item_Table_Id + ")  ";


                        CommonAccounts.SetBalanceWithExpireDate_Tash3ela_Invoice(ref opp, 1, invD_O.invDlst[x], 0, 0, 0);

                        mysql = "  UPDATE Unit_item SET Full_qua = FLOOR((Small_unit * Full_qua + D_Qua) / Small_unit), ";
                        mysql = mysql + " D_Qua =    (Small_unit * Full_qua + D_Qua) % Small_unit  ";
                        mysql = mysql + " WHERE (id = " + invD_O.invDlst[x].Unit_Item_Table_Id + ")  ";

                        opp.insert_me(mysql);
                    }
                }
                ////////// order estlam automatic
                if (Common.Make_Store_Order_Sarf_Estlam_Automatic_From_Invoice == 1 & invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                {
                    //Order_Details_AutomaticCls oo = new Order_Details_AutomaticCls();
                    //oo.Make_Order_From_Invoices_Details_Reverse(ref Excp, ref opp, Invoice_ds.Tables[0], invL_O.invL_R.Id, (int)invL_O.invL_R.Customer_Id, invL_O.invL_R.Invoice_Date.Value, "›« Ê—… „— Ã⁄ „»Ì⁄« ");
                    //Get_arr_Order_Purshing_Id_Automatic_From_InvoiceReverse(New_Invoice_Id, ref opp);
                    
                }

                CustomersAccountCLS ca = new CustomersAccountCLS();
                ca.Set_Customer_BalanceInvoiceReverse(ref Excp, ref opp, (long)invL_O.invL.Customer_Id, invL_O.invL.Id, invL_O.invL.Net_Total ?? 0, invL_O.invL.Notes);
                //  ca.Set_Customer_Balance_Invoice(ref Excp, ref opp, (int)invL.Customer_Id, invL.Invoices_List_Id, invL.Net_Total_Price, invL.Notes);

                  Msg = "";
                //«‰‘«¡ «·ﬁÌœ
                Account_Entry_Invoice_CLS aa = new Account_Entry_Invoice_CLS();
                aa.IsNotDeletePrevEntry = true;
                aa.Make_EntryForInvoices_Reverse(ref Excp, ref Msg, ref opp, invL_O.invL.Id, invL_O.invL.Invoice_Date.Value, invL_O.invL.Invoice_Date.Value, -1);


                if (invL_O.invL.Is_Invoice_Agile == 0)
                {
                    if (Common.MakeOrderMoneyFromInvoiceAutomaticWithoutForm == 1)
                    {
                        long Box_Id = invL_O.invL.BanksId ?? 0;


                        //, long CarsWorkOrdersId, long Reference_Type_Table_Id, long DataDictionaryProcessesListsId

                        long CarsWorkOrdersId = 0;
                        long Reference_Type_Table_Id = invL_O.invL.Id;
                        long DataDictionaryProcessesListsId = Common.InvoiceReverse_Type_Id;

                        PaidValueFromMarket = invL_O.invL.NetInvoiceTotal ?? 0;
                        if (invL_O.invL.PaymentValueFor_Is_Invoice_Agile > PaidValueFromMarket)
                        {
                            PaidValueFromMarket = invL_O.invL.PaymentValueFor_Is_Invoice_Agile ?? 0;
                        }
                        RestValue = 0;
                        Money_Transaction_BoxOrderReportCLS mo = new Money_Transaction_BoxOrderReportCLS();
                        mo.Invoice_Type = Common.InvoiceReverse_Type_Id;
                        mo.MakeMoney_TransactionFromInvoiceReverse(ref Excp, ref Msg, ref opp, invL_O.invL.Invoice_Date.Value, invL_O.invL.Invoice_Id ?? 0, invL_O.invL.Id, Box_Id, invL_O.invL.Currency_id ?? 0, invL_O.invL.Customer_Id ?? 0, PaidValueFromMarket, RestValue, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);

                    }
                }
                else if (invL_O.invL.Is_Invoice_Agile == 1 && invL_O.invL.PaymentValueFor_Is_Invoice_Agile > 0)
                {
                    if (Common.MakeOrderMoneyFromInvoiceAutomaticWithoutForm == 1)
                    {
                        long Box_Id = 0;
                        Money_Transaction_BoxOrderReportCLS mo = new Money_Transaction_BoxOrderReportCLS();

                        //, long CarsWorkOrdersId, long Reference_Type_Table_Id, long DataDictionaryProcessesListsId

                        long CarsWorkOrdersId = 0;
                        long Reference_Type_Table_Id = invL_O.invL.Id;
                        long DataDictionaryProcessesListsId = Common.InvoiceReverse_Type_Id;

                        PaidValueFromMarket = invL_O.invL.PaymentValueFor_Is_Invoice_Agile ?? 0;
                        RestValue = 0;

                        mo.Invoice_Type = Common.InvoiceReverse_Type_Id;
                        mo.MakeMoney_TransactionFromInvoiceReverse(ref Excp, ref Msg, ref opp, invL_O.invL.Invoice_Date.Value, invL_O.invL.Invoice_Id ?? 0, invL_O.invL.Id, Box_Id, invL_O.invL.Currency_id ?? 0, invL_O.invL.Customer_Id ?? 0, PaidValueFromMarket, RestValue, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);

                    }
                }



                opp.Commit();
                Excp = 0;
            }
            catch (Exception ee)
            {
                New_Invoice_Id = 0;
                Excp = 1;
                opp.Rollback();
            }

        }


        public void Edit_InvoiceReverse_Update_CustomerAccount(ref int Excp,ref string Msg, InvoicesOperationListCLS invL_O, InvoicesOperationCLS invD_O)
        {
            // DateTime Invoice_Date, decimal InvoiveTotal, decimal NetInvoiceTotal, decimal Commision, decimal CommissionValue, string PolicaNo, string TroodContainerNo, int Make_Order, decimal Invoice_Net_Total_Old, decimal Invoice_Total_Old, decimal ATS05, decimal TaxSales10, decimal CustomerAccountAfter, string Notes, decimal AnotherPayment, long Salesman_Id, int IS_Make_Sarf_Item_BalanceFromInvoice, int Is_InvoiceKta3y
            long Invoice_List_Table_Id = invL_O.invL.Id;
            InvoiceDataSetCLS ids = new InvoiceDataSetCLS();
            DataSet New_Items_ds_Add = new DataSet();
            DataSet New_Items_ds_Minus = new DataSet();
            New_Items_ds_Add = ids.MakeInvoiceDataSet();
            New_Items_ds_Minus = ids.MakeInvoiceDataSet();
            ArrayList mysql_Arr = new ArrayList();
            operations opp = new operations(1);
            try
            {
                string mysql = "";
                //ArrayList arr = new ArrayList();

                //Invoices_List invL_ROld = opp.db.Invoices_List.AsNoTracking().Where(i => i.Id == Invoice_List_Table_Id).FirstOrDefault();
                
                New_Invoice_Id = Add_UpdateInvoice_List_Reverse(ref Excp, ref Msg, ref opp, invL_O);


               

                decimal OLD_Larg_Qnty = 0;
                decimal OLD_Small_Qnty = 0;
                decimal OLD_Item_No = 0;
                InvoiceDataSetCLS ii = new InvoiceDataSetCLS();
                  /////// check the editableitem
                for (int x = 0; x < invD_O.invDlst.Count; x++)
                {
                    InvoicesOperationCLS invd_O = new InvoicesOperationCLS();
                    //invd_O.invD.Id = invL_O.Invoices_List_Id;
                    //invd_O.Get_Item_Details(dss, x);
                    invd_O.invD = invD_O.invDlst[x];
                   



                    int Transaction_Type = 1;
                    int Invoice_Type_Id = Common.InvoiceReverse_Type_Id;
                    invL_O.Invoice_Type_Id = Common.InvoiceReverse_Type_Id;
                     
                    if (invD_O.invDlst[x].Item_DeletedFlage.ToString() == "0")
                    {
                        invD_O.invDlst[x].Invoice_Id = invL_O.invL.Id;
                        Invoices_Details_Reverse invoices_Details_Reverse = invd_O.Insert_Invoices_Details_Reverse(ref Excp, ref opp, invD_O.invDlst[x]);
                        invd_O.invD.Id = invoices_Details_Reverse.Id;
                        
                        if (Excp == 1)
                        {
                            opp.Rollback();
                            return;
                        }



                        if (Common.Project_Market == 1 & invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                        {
                            //int Transaction_Type = 2;
                            //int Invoice_Type_Id = 1;
                            try
                            {
                                invd_O.invD.Store_Name = Common.GetStoreName(ref opp, invd_O.invD.Store_Id ?? 0);
                            }
                            catch { }
                            //ii.Set_ItemTransactionFromInvoices(ref opp, invd.Unit_Item_Table_Id, invd.Item_Net_Total_Price, invd.Larg_Qnty, invd.Small_Qnty, invd.Qnty_Another, 0, Transaction_Type, invL.Invoice_Date, Invoice_List_Table_Id, invd.Invoices_Details_Id, invL.Invoice_Type_Id, "„— Ã⁄ „»Ì⁄« ", invd.Store_Name, invL.Customer_Name, invL.Customer_Name, invL.Invoice_No);
                            ii.Set_ItemTransactionFromInvoiceR(ref opp, invD_O.invDlst[x], invL_O, 0, Transaction_Type, Common.InvoiceReverse_Type_Id, "›« Ê—… „— Ã⁄ „»Ì⁄« ");
                            //InvoiceDataSetCLS ii = new InvoiceDataSetCLS();
                        }
                    }
                    else if (invD_O.invDlst[x].Item_DeletedFlage.ToString() == "-1")
                    {
                        string mysqlOld = "Select * from Invoices_Details_Reverse Where Id=" + invD_O.invDlst[x].Id + "";
                        DataSet dsN = opp.return_dataset(mysqlOld);
                        if (dsN.Tables[0].Rows.Count > 0)
                        {
                            OLD_Larg_Qnty = decimal.Parse(dsN.Tables[0].Rows[0]["Larg_Qnty"].ToString());
                            OLD_Small_Qnty = decimal.Parse(dsN.Tables[0].Rows[0]["Small_Qnty"].ToString());
                            OLD_Item_No = decimal.Parse(dsN.Tables[0].Rows[0]["Qnty_Another"].ToString());

                            //Item_No_Free, LargItemsQnty_Free, SmallItemsQnty_Free ,Item_Expire_Date
                            decimal OLD_LargItemsQnty_Free = decimal.Parse(dsN.Tables[0].Rows[0]["LargItemsQnty_Free"].ToString());
                            decimal OLD_SmallItemsQnty_Free = decimal.Parse(dsN.Tables[0].Rows[0]["SmallItemsQnty_Free"].ToString());
                            decimal OLD_Item_No_Free = decimal.Parse(dsN.Tables[0].Rows[0]["Item_No_Free"].ToString());

                            OLD_Larg_Qnty = OLD_Larg_Qnty + OLD_LargItemsQnty_Free;
                            OLD_Small_Qnty = OLD_Small_Qnty + OLD_SmallItemsQnty_Free;
                            OLD_Item_No = OLD_Item_No + OLD_Item_No_Free;
                        }
                        invd_O.Update_Invoices_Details_Reverse(ref Excp, ref opp,  invD_O.invDlst[x]);
                        if (Excp == 1)
                        {
                            opp.Rollback();
                            return;
                        }


                        if (Common.Project_Market == 1 & invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                        {
                            //int Transaction_Type = 2;
                            //int Invoice_Type_Id = 1;
                            try
                            {
                                if (invd_O.invD.Store_Name.Trim() == "")
                                {
                                    invd_O.invD.Store_Name = Common.GetStoreName(ref opp, invd_O.invD.Store_Id ?? 0);
                                }
                            }
                            catch { }
                            //ii.Set_ItemTransactionFromInvoices(ref opp, invd.Unit_Item_Table_Id, invd.Item_Net_Total_Price, invd.Larg_Qnty, invd.Small_Qnty, invd.Qnty_Another, 0, Transaction_Type, invL.Invoice_Date, Invoice_List_Table_Id, invd.Invoices_Details_Id, invL.Invoice_Type_Id, "„— Ã⁄ „»Ì⁄« ", invd.Store_Name, invL.Customer_Name, invL.Customer_Name, invL.Invoice_No);
                            ii.Set_ItemTransactionFromInvoiceR(ref opp, invD_O.invDlst[x], invL_O, 0, Transaction_Type, Common.InvoiceReverse_Type_Id, "›« Ê—… „— Ã⁄ „»Ì⁄« ");
                            //InvoiceDataSetCLS ii = new InvoiceDataSetCLS();
                        }
                    }

                    #region Form Project_Market=1
                    if (Common.Project_Market == 1 & invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                    {
                        //if (invD_O.invDlst[x].Item_DeletedFlage.ToString() == "1")
                        //{

                        //    //Õ–›
                        //    mysql = " UPDATE Unit_item SET Full_qua =Full_qua - " + invd_O.invD.Larg_Qnty + ", D_Qua =D_Qua - " + invd_O.invD.Small_Qnty + ", Item_No =Item_No - " + invd_O.invD.Qnty_Another + " WHERE (id = " + invd_O.invD.Unit_Item_Table_Id + ") ";

                        //    opp.insert_me(mysql);

                        //    //«·»Ê‰’
                        //    mysql = " UPDATE Unit_item SET Full_qua =Full_qua - " + invd_O.invD.LargItemsQnty_Free + ", D_Qua =D_Qua - " + invd_O.invD.SmallItemsQnty_Free + ", Item_No =Item_No - " + invd_O.invD.Item_No_Free + " WHERE (id = " + invd_O.invD.Unit_Item_Table_Id + ") ";
                        //    opp.insert_me(mysql);

                        //    CommonAccounts.SetBalanceWithExpireDate_Tash3ela_Invoice(ref opp, 2, invD_O.invDlst[x], OLD_Larg_Qnty, OLD_Small_Qnty, OLD_Item_No);

                        //}
                        if (invD_O.invDlst[x].Item_DeletedFlage.ToString() == "-1")
                        {//«÷«›…
                            mysql = " UPDATE Unit_item SET Full_qua =Full_qua + " + invd_O.invD.Larg_Qnty + ", D_Qua =D_Qua + " + invd_O.invD.Small_Qnty + ", Item_No =Item_No + " + invd_O.invD.Qnty_Another + " WHERE (id = " + invd_O.invD.Unit_Item_Table_Id + ") ";

                            opp.insert_me(mysql);

                            //«·»Ê‰’
                            mysql = " UPDATE Unit_item SET Full_qua =Full_qua + " + invd_O.invD.LargItemsQnty_Free + ", D_Qua =D_Qua + " + invd_O.invD.SmallItemsQnty_Free + ", Item_No =Item_No + " + invd_O.invD.Item_No_Free + " WHERE (id = " + invd_O.invD.Unit_Item_Table_Id + ") ";
                            opp.insert_me(mysql);

                            // Common.SetBalanceWithExpireDate_Tash3ela(ref opp, 1, Item_Id, Unit_Item_Table_Id, Item_Expire_Date, Tash3ela, Larg_Qnty, Small_Qnty, Qnty_Another, LargItemsQnty_Free, SmallItemsQnty_Free, Item_No_Free);
                            CommonAccounts.SetBalanceWithExpireDate_Tash3ela_Invoice(ref opp, 1, invd_O.invD, OLD_Larg_Qnty, OLD_Small_Qnty, OLD_Item_No);

                        }
                        else if (invD_O.invDlst[x].Item_DeletedFlage.ToString() == "0")
                        {
                            //dsN = opp.return_dataset("Select * from   Invoices_Details_Reverse Where Id =" + Invoices_Details_Id + "");
                            //OLD_Larg_Qnty = decimal.Parse(dsN.Tables[0].Rows[0]["Larg_Qnty"].ToString());
                            //OLD_Small_Qnty = decimal.Parse(dsN.Tables[0].Rows[0]["Small_Qnty"].ToString());
                            //OLD_Item_No = decimal.Parse(dsN.Tables[0].Rows[0]["Qnty_Another"].ToString());

                            decimal Larg_Qnty = invd_O.invD.Larg_Qnty ?? 0 - OLD_Larg_Qnty;
                            decimal Small_Qnty = invd_O.invD.Small_Qnty ?? 0 - OLD_Small_Qnty;
                            decimal Qnty_Another = invd_O.invD.Qnty_Another ?? 0 - OLD_Item_No;

                            mysql = " UPDATE Unit_item SET Full_qua =Full_qua + " + Larg_Qnty + ", D_Qua =D_Qua + " + Small_Qnty + ", Item_No =Item_No + " + Qnty_Another + " WHERE (id = " + invd_O.invD.Unit_Item_Table_Id + ") ";
                            opp.insert_me(mysql);


                            // Common.SetBalanceWithExpireDate_Tash3ela(ref opp, 1, Item_Id, Unit_Item_Table_Id, Item_Expire_Date, Tash3ela, Larg_Qnty, Small_Qnty, Qnty_Another, LargItemsQnty_Free, SmallItemsQnty_Free, Item_No_Free);
                            CommonAccounts.SetBalanceWithExpireDate_Tash3ela_Invoice(ref opp, 0, invD_O.invDlst[x], Larg_Qnty, Small_Qnty, Qnty_Another);


                        }

                        //mysql = "  UPDATE Unit_item SET Full_qua = FLOOR((Small_unit * Full_qua + D_Qua) / Small_unit), ";
                        //mysql = mysql + " D_Qua =   (( Small_unit * Full_qua + D_Qua) / Small_unit - FLOOR(( Small_unit * Full_qua + D_Qua) /  Small_unit)) *  Small_unit  ";
                        //mysql = mysql + " WHERE (id = " + Unit_Item_Table_Id + ")  ";
                        mysql = "  UPDATE Unit_item SET Full_qua = FLOOR((Small_unit * Full_qua + D_Qua) / Small_unit), ";
                        mysql = mysql + " D_Qua =    (Small_unit * Full_qua + D_Qua) % Small_unit  ";
                        mysql = mysql + " WHERE (id = " + invd_O.invD.Unit_Item_Table_Id + ")  ";
                        opp.insert_me(mysql);

                    }

                    #endregion

                    #region order sarf automatic
                    if (Common.Make_Store_Order_Sarf_Estlam_Automatic_From_Invoice == 1 & invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                    {

                        /*

                        if (dss.Tables[0].Rows[x]["Item_DeletedFlage"].ToString() == "1")
                        {
                            //Õ–›
                            Fill_Ds(ref New_Items_ds_Minus, dss, x);



                        }
                        else if (dss.Tables[0].Rows[x]["Item_DeletedFlage"].ToString() == "-1")
                        {//«÷«›…

                            Fill_Ds(ref New_Items_ds_Add, dss, x);
                        }
                        else
                        {

                            decimal Larg_Qnty = invd_O.invD.Larg_Qnty ?? 0 - OLD_Larg_Qnty;
                            decimal Small_Qnty = invd_O.invD.Small_Qnty ?? 0 - OLD_Small_Qnty;
                            decimal Qnty_Another = invd_O.invD.Qnty_Another ?? 0 - OLD_Item_No;

                            decimal Larg_Qnty__ = decimal.Parse(Larg_Qnty.ToString());
                            decimal Small_Qnty__ = decimal.Parse(Small_Qnty.ToString());
                            decimal Small_Unit_Price__ = decimal.Parse(invd_O.invD.Small_Unit_Price.ToString());
                            Cls_Conversion_Unit.ConVert_By_S_UnitValue(ref Larg_Qnty__, ref Small_Qnty__, Small_Unit_Price__);
                            int Add_ = 1;

                            if (Larg_Qnty__ < 0 | Small_Qnty__ < 0)
                            {
                                Add_ = 0;

                            }
                            else if (Larg_Qnty__ > 0 | Small_Qnty__ > 0)
                            {
                                Add_ = 1;

                            }
                            else
                            {
                                Add_ = -1;
                            }
                            if (Add_ == 1)
                            {
                                Fill_Ds(ref New_Items_ds_Add, dss, x, Larg_Qnty__, Small_Qnty__);
                            }
                            else if (Add_ == 0)
                            {
                                if (Larg_Qnty__ < 0)
                                {
                                    Larg_Qnty__ = Larg_Qnty__ * -1;
                                }

                                if (Small_Qnty__ < 0)
                                {
                                    Small_Qnty__ = Small_Qnty__ * -1;
                                }
                                Fill_Ds(ref New_Items_ds_Minus, dss, x, Larg_Qnty__, Small_Qnty__);
                            }

                        }
                        */
                    }




                    #endregion
                }

                //////// «·Õ–›
                ///
                for (int x = 0; x < invD_O.invDlst_Deleted.Count; x++)
                {
                    //InvoicesOperationCLS invd_O = new InvoicesOperationCLS();
                    //invd_O.invD.Id = invL_O.Invoices_List_Id;
                    
                    //invd_O.invD = invD_O.invDlst_Deleted[x];


                    //int Transaction_Type = 1;
                    int Invoice_Type_Id = Common.InvoiceReverse_Type_Id;
                    //invL_O.Invoice_Type_Id = Common.InvoiceReverse_Type_Id;
                    if (invD_O.invDlst_Deleted[x].Item_DeletedFlage.ToString() == "1")
                    {
                      
                        mysql = "Delete From Invoices_Details_Reverse  where id=" + invD_O.invDlst_Deleted[x].Id;
                        opp.insert_me(mysql);

                        //ii.Delete_ItemTransactionFromInvoices(ref opp, invd_O.invD.Unit_Item_Table_Id ?? 0, Invoice_List_Table_Id, invd_O.invD.Id, 0, invL_O.Invoice_Type_Id);
                        ii.Delete_ItemTransactionFromInvoices(ref opp, invD_O.invDlst_Deleted[x].Unit_Item_Table_Id ?? 0, Invoice_List_Table_Id, invD_O.invDlst_Deleted[x].Id, 0, Invoice_Type_Id);
                    }


                    #region Form Project_Market=1
                    if (Common.Project_Market == 1 & invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                    {
                        if (invD_O.invDlst_Deleted[x].Item_DeletedFlage.ToString() == "1")
                        {

                            //Õ–›
                            mysql = " UPDATE Unit_item SET Full_qua =Full_qua - " + invD_O.invDlst_Deleted[x].Larg_Qnty + ", D_Qua =D_Qua - " + invD_O.invDlst_Deleted[x].Small_Qnty + ", Item_No =Item_No - " + invD_O.invDlst_Deleted[x].Qnty_Another + " WHERE (id = " + invD_O.invDlst_Deleted[x].Unit_Item_Table_Id + ") ";

                            opp.insert_me(mysql);

                            //«·»Ê‰’
                            mysql = " UPDATE Unit_item SET Full_qua =Full_qua - " + invD_O.invDlst_Deleted[x].LargItemsQnty_Free + ", D_Qua =D_Qua - " + invD_O.invDlst_Deleted[x].SmallItemsQnty_Free + ", Item_No =Item_No - " + invD_O.invDlst_Deleted[x].Item_No_Free + " WHERE (id = " + invD_O.invDlst_Deleted[x].Unit_Item_Table_Id + ") ";
                            opp.insert_me(mysql);

                            CommonAccounts.SetBalanceWithExpireDate_Tash3ela_Invoice(ref opp, 2, invD_O.invDlst_Deleted[x], OLD_Larg_Qnty, OLD_Small_Qnty, OLD_Item_No);

                        }



                        mysql = "  UPDATE Unit_item SET Full_qua = FLOOR((Small_unit * Full_qua + D_Qua) / Small_unit), ";
                        mysql = mysql + " D_Qua =    (Small_unit * Full_qua + D_Qua) % Small_unit  ";
                        mysql = mysql + " WHERE (id = " + invD_O.invDlst_Deleted[x].Unit_Item_Table_Id + ")  ";
                        opp.insert_me(mysql);

                    }

                    #endregion

                    #region order sarf automatic
                    if (Common.Make_Store_Order_Sarf_Estlam_Automatic_From_Invoice == 1 & invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                    {

                        /*

                        if (dss.Tables[0].Rows[x]["Item_DeletedFlage"].ToString() == "1")
                        {
                            //Õ–›
                            Fill_Ds(ref New_Items_ds_Minus, dss, x);



                        }
                        else if (dss.Tables[0].Rows[x]["Item_DeletedFlage"].ToString() == "-1")
                        {//«÷«›…

                            Fill_Ds(ref New_Items_ds_Add, dss, x);
                        }
                        else
                        {

                            decimal Larg_Qnty = invd_O.invD.Larg_Qnty ?? 0 - OLD_Larg_Qnty;
                            decimal Small_Qnty = invd_O.invD.Small_Qnty ?? 0 - OLD_Small_Qnty;
                            decimal Qnty_Another = invd_O.invD.Qnty_Another ?? 0 - OLD_Item_No;

                            decimal Larg_Qnty__ = decimal.Parse(Larg_Qnty.ToString());
                            decimal Small_Qnty__ = decimal.Parse(Small_Qnty.ToString());
                            decimal Small_Unit_Price__ = decimal.Parse(invd_O.invD.Small_Unit_Price.ToString());
                            Cls_Conversion_Unit.ConVert_By_S_UnitValue(ref Larg_Qnty__, ref Small_Qnty__, Small_Unit_Price__);
                            int Add_ = 1;

                            if (Larg_Qnty__ < 0 | Small_Qnty__ < 0)
                            {
                                Add_ = 0;

                            }
                            else if (Larg_Qnty__ > 0 | Small_Qnty__ > 0)
                            {
                                Add_ = 1;

                            }
                            else
                            {
                                Add_ = -1;
                            }
                            if (Add_ == 1)
                            {
                                Fill_Ds(ref New_Items_ds_Add, dss, x, Larg_Qnty__, Small_Qnty__);
                            }
                            else if (Add_ == 0)
                            {
                                if (Larg_Qnty__ < 0)
                                {
                                    Larg_Qnty__ = Larg_Qnty__ * -1;
                                }

                                if (Small_Qnty__ < 0)
                                {
                                    Small_Qnty__ = Small_Qnty__ * -1;
                                }
                                Fill_Ds(ref New_Items_ds_Minus, dss, x, Larg_Qnty__, Small_Qnty__);
                            }

                        }
                        */
                    }




                    #endregion
                }
                //DateTime Invoice_Date = DateTime.Parse(dss.Tables[0].Rows[0]["Invoice_Date"].ToString());
                  Msg = "";


                if (Common.Make_Store_Order_Sarf_Estlam_Automatic_From_Invoice == 1 & invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                {
                    Order_Details_AutomaticCls oo = new Order_Details_AutomaticCls();
                    bool MakeOrderOk = false;
                    if (New_Items_ds_Add.Tables[0].Rows.Count > 0)
                    {
                        oo.Make_Order_From_Edit_Invoices_Details_Reverse(ref Excp, ref opp, ref mysql_Arr, New_Items_ds_Add.Tables[0], Invoice_List_Table_Id, (long)invL_O.invL.Customer_Id, invL_O.invL.Invoice_Date.Value, "›« Ê—… „— Ã⁄ „»Ì⁄« ");


                        if (Excp > 0)
                        {
                            opp.Rollback();
                            return;
                        }
                    }

                    if (MakeOrderOk)
                    {
                        Msg = Msg + "\n";
                        Msg = Msg + " „ ≈’œ«— ≈–‰ «·’—› ··⁄„Ì· „‰ «·„Œ“‰";
                    }

                    //////////////////„— Ã⁄ ’—›
                    if (New_Items_ds_Minus.Tables[0].Rows.Count > 0)
                    {

                        oo.Make_Order_From_EditInvoive_Details(ref Excp, ref mysql_Arr, ref opp, ref arr_Order_Purshing_Id_Automatic, ref MakeOrderOk, New_Items_ds_Minus.Tables[0], Invoice_List_Table_Id, (long)invL_O.invL.Customer_Id, invL_O.invL.Invoice_Date.Value, "›« Ê—… „— Ã⁄ „»Ì⁄« ");

                        if (Excp > 0)
                        {
                            opp.Rollback();
                            return;
                        }
                    }

                }

                List<Invoices_Details_Reverse> invoices_DetailsLst = opp.db.Invoices_Details_Reverse.AsNoTracking().Where(i => i.Invoice_Id == invL_O.invL.Id).ToList();

                if (invoices_DetailsLst.Count == 0)
                {
                    //IsDeleteInvoices_List = true;
                    mysql = "Delete From Invoices_List  where id=" + Invoice_List_Table_Id;
                    opp.insert_me(mysql);

                    //Invoice_List_Table_Id = 0;
                    //invL.Invoices_List_Id = 0;
                }
                else
                {
                    //invL_O.Update_Invoices_List(ref opp, ref Excp, (Invoices_List)invL_O.invL);
                }
                ///to delete  the invoice
                
                if (Excp == 1)
                {
                    opp.Rollback();
                    return;
                }
                ///////// check customer balance
 



                CustomersAccountCLS ca = new CustomersAccountCLS();
                ca.ProcessNo = invL_O.invL.ProcessNo ?? 0;
                //ca.Updatet_Customer_BalanceWithEditInvoiceReverse(ref Excp, ref opp, invL.Invoice_Date, (int)invL.Customer_Id, Invoice_List_Table_Id, invL.Net_Total_Price, Invoice_NetTotal_Diff, invL.Notes);
                ca.Set_Customer_BalanceInvoiceReverse(ref Excp, ref opp, (long)invL_O.invL.Customer_Id, invL_O.Invoices_List_Id, invL_O.invL.Net_Total ?? 0, invL_O.invL.Notes);
                //«‰‘«¡ «·ﬁÌœ
                Account_Entry_Invoice_CLS aa = new Account_Entry_Invoice_CLS();
                aa.IsNotDeletePrevEntry = false;
                aa.Make_EntryForInvoices_Reverse(ref Excp, ref Msg, ref opp, invL_O.Invoices_List_Id, invL_O.invL.Invoice_Date.Value, invL_O.invL.Invoice_Date.Value, -1);


                //opp.insert_me_transaction(ref Excp, 1, arr);
                //opp.insert_me_transaction(ref Excp, arr);
                opp.Commit();

                Excp = 0;

            }
            catch (Exception e)
            {
                Excp = 1;
                opp.Rollback();
                Msg = e.Message;
                //throw (e);
            }
            if (Excp == 0)
            {
                string mysql = "";

                if (mysql_Arr.Count > 0)
                {
                    operations opp2 = new operations(1);
                    try
                    {
                        mysql = "";
                        for (int y = 0; y < mysql_Arr.Count; y++)
                        {
                            mysql = mysql + " " + mysql_Arr[y].ToString();
                            //opp2.insert_me(mysql_Arr[y].ToString());
                        }
                        opp2.insert_me(mysql);
                        opp2.Commit();
                    }
                    catch (Exception ee) { opp2.Rollback(); }
                }

            }
        }


      
        public void Make_Invoice_Reverse_Update_CustomerAccount_POS(ref int Excp, long Box_Id, DataSet Invoice_ds, long Invoice_No, DateTime Invoice_Date, long Customer_Id, long Currency_Id, decimal Net_Total, int IsCommission_Value, decimal Commission, decimal CommissionValue, int Invoice_Type, string PolicaNo, string TroodContainerNo, string Notes, int Make_Order, decimal TaxSale10, decimal ATS05, decimal CustomerAccountBefor, decimal CustomerAccountAfter, ArrayList mArrMake_Order)
        {
            operations opp = new operations(1);
            try
            {
                //checked item_report to SendKeys to store
                //DataView dvToStore = new DataView();
                //dvToStore.Table = Invoice_ds.Tables[0];
                //ItemToStore it = new ItemToStore(dvToStore);
                 
                //if (Common.Project_Market == 0)
                //{
                //    it.ShowDialog();
                //}
                //else
                //{
                //    it.ClickBtnOK();
                //}
                //ArrayList mArrMake_Order = new ArrayList();
                //mArrMake_Order = it.ArrItemToStore;
                //if (!it.BtnOkClick)
                //{
                //    Excp = 10;
                //    return;
                //}
                ////***********************************************************************
                ////////// Make_New_Invoice_Id
                DataTable dt = new DataTable();
                dt = Invoice_ds.Tables[0];

                string mysql = "";
                decimal Total = 0;
                //  decimal TaxSales10=0; 
                //   decimal  ATS05=0;
                decimal TaxSales10_Value = 0;
                decimal ATS05_Value = 0;
                //for (int x = 0; x < Invoice_ds.Tables[0].Rows.Count; x++)
                //{
                //    Total = Total + decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Total_Price"].ToString());

                //}
                //TaxSales10_Value = Total * TaxSale10 / 100;

                //ATS05_Value = Total * ATS05 / 100;

                InvoicesClss inv = new InvoicesClss();

                inv.Make_New_Invoice_Reverse_Id_POS(ref Excp, ref opp, ref New_Invoice_Id, Invoice_No, Customer_Id, Invoice_Date, Invoice_Type, (decimal)Total, IsCommission_Value, Commission, CommissionValue, decimal.Parse(TaxSale10.ToString()), decimal.Parse(ATS05.ToString()), (decimal)TaxSales10_Value, (decimal)ATS05_Value, Net_Total, "", Currency_Id, 1, Notes, PolicaNo, TroodContainerNo, CustomerAccountBefor, CustomerAccountAfter);



                /////////////////Set All Commision
                decimal Total_Item_CommissionValue = 0;
                for (int x = 0; x < Invoice_ds.Tables[0].Rows.Count; x++)
                {
                    Total_Item_CommissionValue = Total_Item_CommissionValue + decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Item_CommissionValue"].ToString());
                }
                decimal Total_CommissionValue = Total_Item_CommissionValue + CommissionValue;
                mysql = " UPDATE Invoices_List_Reverse SET  Total_Item_CommissionValue=" + Total_Item_CommissionValue + ",Total_CommissionValue=    " + Total_CommissionValue + " WHERE (id = " + New_Invoice_Id + ") ";
                opp.insert_me(mysql);
                ///////////////////////////////////////////////////////////////////////////////


                ///////////// add invoice Items and update request qnty


                long Invoice_Id = New_Invoice_Id;
                string Invoice_Item_Name = "";
                long Item_Id;
                long Store_Id = 0;
                decimal Larg_Qnty;
                decimal Small_Qnty;
                string Qnty_Another;
                decimal Larg_Unit_Price;
                decimal Small_Unit_Price;
                decimal Qnty_value;
                decimal Total_Value;
                decimal ATS05_;
                decimal TaxSales10_;
                int Unit_Used_for_Price;
                long unit_id;
                long Unit_Item_Table_Id = 0;  
                string code;
                string Notes_;
                int Balance_Checked;
                string Specification;

                ///// Requests_Details
                decimal Export_Qnty_Larg;
                decimal Export_Qnty_Small;
                decimal Export_Item_No;
                int Requests_Details_id;
                ////
                string Customer_Name = "";
                string Store_Name = "";
                ////
                ///
                try
                {
                    Customer_Name = Common.GetCustomerName(ref opp, Customer_Id);
                }
                catch { }
                InvoiceDataSetCLS ii = new InvoiceDataSetCLS();
                //////////// add item in invoice details
                for (int x = 0; x < Invoice_ds.Tables[0].Rows.Count; x++)
                {
                    try
                    {
                        TaxSales10_Value = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["TaxSales10_Value"].ToString());
                    }
                    catch (Exception e)
                    {
                        TaxSales10_Value = 0;
                    }
                    try
                    {
                        ATS05_Value = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["ATS05_Value"].ToString());
                    }
                    catch (Exception e)
                    {
                        ATS05_Value = 0;
                    }

                    Invoice_Item_Name = Invoice_ds.Tables[0].Rows[x]["Item_Name"].ToString();
                    Specification = Invoice_ds.Tables[0].Rows[x]["Specification"].ToString();

                    string NameSpecification = Invoice_ds.Tables[0].Rows[x]["Item_Name"].ToString() + "  " + Invoice_ds.Tables[0].Rows[x]["Specification"].ToString();
                    Item_Id = long.Parse(Invoice_ds.Tables[0].Rows[x]["Item_id"].ToString());
                    Store_Id = long.Parse(Invoice_ds.Tables[0].Rows[x]["Store_Id"].ToString());

                    Larg_Qnty = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Larg_Qnty"].ToString());
                    Small_Qnty = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Small_Qnty"].ToString());
                    Qnty_Another = Invoice_ds.Tables[0].Rows[x]["Qnty_Another"].ToString();
                    Larg_Unit_Price = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Larg_Unit_price"].ToString());
                    Small_Unit_Price = 0;
                    Qnty_value = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Qnty_value"].ToString());
                    Total_Value = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Item_Net_Total_Price"].ToString());
                    ATS05_ = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["ATS05"].ToString());
                    TaxSales10_ = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["TaxSales10"].ToString());
                    Unit_Used_for_Price = 0;
                    unit_id = long.Parse(Invoice_ds.Tables[0].Rows[x]["unit_id"].ToString());
                    Unit_Item_Table_Id = long.Parse(Invoice_ds.Tables[0].Rows[x]["Unit_Item_Table_Id"].ToString());
                    code = Invoice_ds.Tables[0].Rows[x]["Code"].ToString();
                    int Is_Item_CommissionValue = int.Parse(Invoice_ds.Tables[0].Rows[x]["Is_Item_CommissionValue"].ToString());
                    decimal Item_CommissionValue = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Item_CommissionValue"].ToString());
                    decimal Unit_CommissionValue = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Unit_CommissionValue"].ToString());

                    //  , Is_Item_CommissionValue,Item_CommissionValue
                    //  ,  " + Is_Item_CommissionValue + "," + Item_CommissionValue + "

                    Notes_ = "";
                    Balance_Checked = 1;

                    // string NameSpecification = Invoice_ds.Tables[0].Rows[x]["NameSpecification"].ToString();

                    decimal Item_Net_Total_Price = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Item_Net_Total_Price"].ToString());
                    decimal Item_Total_WithoutTax = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Item_Total_WithoutTax"].ToString());
                    string QntyStr = Invoice_ds.Tables[0].Rows[x]["QntyStr"].ToString();
                    string QntyStrColumn = Invoice_ds.Tables[0].Rows[x]["QntyStrColumn"].ToString();
                    decimal Item_TaxSales10_Value = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Item_TaxSales10_Value"].ToString());
                    //long Store_Id = long.Parse(Invoice_ds.Tables[0].Rows[x]["Store_Id"].ToString());


                    mysql = " INSERT INTO Invoices_Details_Reverse (Store_Id,Invoice_Item_Name,NameSpecification,Invoice_Id, Item_Id, Larg_Qnty, Small_Qnty,Qnty_Another, Larg_Unit_Price, Small_Unit_Price, Qnty_value,Total, ATS05, TaxSales10,TaxSales10_Value,ATS05_Value,Unit_Used_for_Price,unit_id,Unit_Item_Table_Id,code, Notes,Balance_Checked,Specification,Make_Order, Is_Item_CommissionValue,Item_CommissionValue,Unit_CommissionValue,Item_Net_Total_Price, Item_Total_WithoutTax, QntyStr,     QntyStrColumn,    Item_TaxSales10_Value) ";
                    mysql = mysql + " VALUES (" + Store_Id + ",N'" + Invoice_Item_Name + "',N'" + NameSpecification + "'," + Invoice_Id + "," + Item_Id + "," + Larg_Qnty + "," + Small_Qnty + ",'" + Qnty_Another + "'," + Larg_Unit_Price + "," + Small_Unit_Price + "," + Qnty_value + "," + Total_Value + "," + ATS05_ + "," + TaxSales10_ + "," + TaxSales10_Value + "," + ATS05_Value + "," + Unit_Used_for_Price + "," + unit_id + "," + Unit_Item_Table_Id + ",'" + code + "','" + Notes_ + "'," + Balance_Checked + ",'" + Specification + "'," + mArrMake_Order[x] + ",  " + Is_Item_CommissionValue + "," + Item_CommissionValue + "," + Unit_CommissionValue + "," + Item_Net_Total_Price + "," + Item_Total_WithoutTax + ", '" + QntyStr + "',  '" + QntyStrColumn + "',    " + Item_TaxSales10_Value + ") ";
                    long Invoices_Details_Id = opp.Insert_me_Return_Last_Identify(mysql);

                    if (Common.Project_Market == 1)
                    {
                        int Transaction_Type = 1;
                        int Invoice_Type_Id = 2;
                        long Invoices_Details_Id_ = Invoices_Details_Id;
                        //InvoiceDataSetCLS ii = new InvoiceDataSetCLS();
                        try
                        {
                            Store_Name = Common.GetStoreName(ref opp, Store_Id);
                        }
                        catch { }

                        int Type_From = Common.Customer_StoreTransactionCode;
                        int Type_To = Common.Store_StoreTransactionCode;
                        long Id_From = Customer_Id;
                        long Id_To = Store_Id;
                        long StoreId = Store_Id;
                        string StoreId_Name = Store_Name;
                        // , Type_From,  Type_To,  Id_From,  Id_To,  StoreId,  StoreId_Name



                        ii.Set_ItemTransactionFromInvoices(ref opp, Unit_Item_Table_Id, Larg_Unit_Price, Total_Value, Larg_Qnty, Small_Qnty, decimal.Parse(Qnty_Another), 0, Transaction_Type, Invoice_Date, Invoice_Id, Invoices_Details_Id_, Invoice_Type_Id, "›« Ê—… „— Ã⁄ „»Ì⁄« ", Store_Name, Customer_Name, Customer_Name, Invoice_No, Type_From, Type_To, Id_From, Id_To, StoreId, StoreId_Name);

                    }
                    if (Common.Project_Market == 1)
                    {
                        mysql = " UPDATE Unit_item SET Full_qua =Full_qua + " + Larg_Qnty + ", D_Qua =D_Qua + " + Small_Qnty + ", Item_No =Item_No + " + Qnty_Another + " WHERE (id = " + Unit_Item_Table_Id + ") ";
                        opp.insert_me(mysql);


                        //mysql = "  UPDATE Unit_item SET Full_qua = FLOOR((Small_unit * Full_qua + D_Qua) / Small_unit), ";
                        //mysql = mysql + " D_Qua =   (( Small_unit * Full_qua + D_Qua) / Small_unit - FLOOR(( Small_unit * Full_qua + D_Qua) /  Small_unit)) *  Small_unit  ";
                        //mysql = mysql + " WHERE (id = " + Unit_Item_Table_Id + ")  ";

                        mysql = "  UPDATE Unit_item SET Full_qua = FLOOR((Small_unit * Full_qua + D_Qua) / Small_unit), ";
                        mysql = mysql + " D_Qua =    (Small_unit * Full_qua + D_Qua) % Small_unit  ";
                        mysql = mysql + " WHERE (id = " + Unit_Item_Table_Id + ")  ";

                        opp.insert_me(mysql);
                    }
                }

                string Msg = "";
                CustomersAccountCLS ca = new CustomersAccountCLS();
                ca.Set_Customer_BalanceInvoiceReverse(ref Excp, ref opp, Customer_Id, Invoice_Id, Net_Total, Notes);
                if (Make_Invoice_Agile == 0)
                {
                    Money_Transaction_BoxOrderReportCLS mt = new Money_Transaction_BoxOrderReportCLS();

                    Excp = 0;

                    int Transaction_Type = 2;
                    if (Box_Id == 0)
                    {
                        Box_Id = mt.GetDefaultBank_ID();// int.Parse(ot.banks_List1.cmbBankType.SelectedValue.ToString());
                    }
                    decimal Box_Balance = 0;
                    string Order_No = mt.Make_New_Order_No(ref Excp, Transaction_Type).ToString();
                    mysql = "Select * from Banks Where Id=" + Box_Id;
                    DataTable dtBanks = opp.return_DataTable(mysql);
                    if (dtBanks.Rows.Count > 0)
                    {

                        Box_Balance = decimal.Parse(dtBanks.Rows[0]["Last_Balance"].ToString());
                        decimal Cach_Value = PaidValueFromMarket;// ot.nuValue.Value;
                        DateTime Order_Date = Invoice_Date;
                        string Man_Name = " ›« Ê—… „— Ã⁄ „»Ì⁄«  —ﬁ„ " + Invoice_No;
                        int Reverse_Order = 1;
                        long Order_Mony_Id2 = 0;
                        mt.Invoice_ID = Invoice_Id;

                        //, long CarsWorkOrdersId, long Reference_Type_Table_Id, long DataDictionaryProcessesListsId

                        long CarsWorkOrdersId = 0;
                        long Reference_Type_Table_Id = 0;
                        long DataDictionaryProcessesListsId = 0;

                        //   ,   CarsWorkOrdersId,   Reference_Type_Table_Id,   DataDictionaryProcessesListsId

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
                        Msg = "·„ Ì „ ≈’œ«— «·‰ﬁœÌ… —»„« ·⁄œ„ ÊÃÊœ ’‰œÊﬁ ≈› —«÷Ï ··›Ê« Ì—";
                    }
                }
                //«‰‘«¡ «·ﬁÌœ
                if (Common.IS_Make_Automatic_Queeod == 1)

                {
                    Account_Entry_Invoice_CLS aa = new Account_Entry_Invoice_CLS();
                    aa.IsNotDeletePrevEntry = true;
                    aa.Make_EntryForInvoices_Reverse(ref Excp, ref Msg, ref opp, New_Invoice_Id, Invoice_Date, Invoice_Date, 0);
                }

                ////////// order estlam automatic
                //if (Common.Make_Store_Order_Sarf_Estlam_Automatic_From_Invoice == 1)
                //{
                //    Store_LIB.Order_Details_AutomaticCls oo = new Store_LIB.Order_Details_AutomaticCls();
                //    oo.Make_Order_From_Invoices_Details_Reverse(ref Excp, ref opp, Invoice_ds.Tables[0], Invoice_Id, Customer_Id, Invoice_Date, "›« Ê—… „— Ã⁄ „»Ì⁄« ");
                //    Get_arr_Order_Purshing_Id_Automatic_From_InvoiceReverse(New_Invoice_Id, ref opp);
                 
                //}

                opp.Commit();
                Excp = 0;
            }
            catch (Exception ee)
            {
                Excp = 1;
                opp.Rollback();
            }

        }
        public void Make_Invoice_Update_CustomerAccount_POS(ref int Excp, DataSet Invoice_ds, ref string Msg, long Box_Id, long Invoice_No, DateTime Invoice_Date, long Customer_Id, long Currency_Id, decimal Total, int IsCommission_Value, decimal Commission, decimal CommissionValue, int Invoice_Type, string PolicaNo, string TroodContainerNo, string Notes, int Make_Order, decimal TaxSale10, decimal ATS05, decimal CustomerAccountBefor, decimal CustomerAccountAfter, long SubAccount_Id, string SubAccount_Name, ArrayList mArrMake_Order)
        {
            operations opp = new operations(1);
            decimal Net_Total = Total - Commission;
            try
            {
                Net_Total = Common.RoundMePositive(Net_Total);
                Commission = Common.RoundMePositive(Commission);

                //checked item_report to SendKeys to store
                //DataView dvToStore = new DataView();
                //dvToStore.Table = Invoice_ds.Tables[0];
                //ItemToStore it = new ItemToStore(dvToStore);
                //if (Common.Project_Market == 0)
                //{
                //    it.ShowDialog();
                //}
                //else
                //{
                //    it.ClickBtnOK();
                //}
                //ArrayList mArrMake_Order = new ArrayList();
                //mArrMake_Order = it.ArrItemToStore;

                //if (!it.BtnOkClick)
                //{
                //    Excp = 10;
                //    return;
                //}

                ////////// Make_New_Invoice_Id
                DataTable dt = new DataTable();
                dt = Invoice_ds.Tables[0];

                string mysql = "";

                //  decimal TaxSales10=0; 
                //   decimal  ATS05=0;
                decimal TaxSales10_Value = 0;
                decimal ATS05_Value = 0;

                //for (int x = 0; x < Invoice_ds.Tables[0].Rows.Count; x++)
                //{
                //    Total = Total + decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Total_Price"].ToString());
                //    Total = Common.RoundMe(Total);

                //}
                TaxSales10_Value = (decimal)Total * TaxSale10 / 100;

                ATS05_Value = (decimal)Total * ATS05 / 100;
                TaxSales10_Value = Common.RoundMePositive(TaxSales10_Value);
                ATS05_Value = Common.RoundMePositive(ATS05_Value);


                InvoicesClss inv = new InvoicesClss();

                inv.Make_New_Invoice_Id_POS(ref Excp, ref opp, ref New_Invoice_Id, Invoice_No, Customer_Id, Invoice_Date, Invoice_Type, (decimal)Total, IsCommission_Value, Commission, CommissionValue, decimal.Parse(TaxSale10.ToString()), decimal.Parse(ATS05.ToString()), (decimal)TaxSales10_Value, (decimal)ATS05_Value, Net_Total, "", Currency_Id, 1, Notes, PolicaNo, TroodContainerNo, CustomerAccountBefor, CustomerAccountAfter, SubAccount_Id, SubAccount_Name);

                ///////////// add invoice Items and update request qnty

                ArrayList arr = new ArrayList();
                long Invoice_Id = New_Invoice_Id;
                long Item_Id;
                decimal Larg_Qnty;
                decimal Small_Qnty;
                string Qnty_Another;
                decimal Larg_Unit_Price;
                decimal Small_Unit_Price;
                decimal Qnty_value;
                decimal Total_Value;
                decimal ATS05_;
                decimal TaxSales10_;
                int Unit_Used_for_Price;
                long unit_id;
                long Unit_Item_Table_Id=0;
                string code;
                string Notes_;
                int Balance_Checked;
                string Specification;

                ///// Requests_Details
                decimal Export_Qnty_Larg;
                decimal Export_Qnty_Small;
                decimal Export_Item_No;
                int Requests_Details_id;
                string Invoice_Item_Name = "";
                ////



                //////////// add item in invoice details
                for (int x = 0; x < Invoice_ds.Tables[0].Rows.Count; x++)
                {
                    try
                    {
                        TaxSales10_Value = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["TaxSales10_Value"].ToString());
                    }
                    catch (Exception e)
                    {
                        TaxSales10_Value = 0;
                    }
                    try
                    {
                        ATS05_Value = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["ATS05_Value"].ToString());
                    }
                    catch (Exception e)
                    {
                        ATS05_Value = 0;
                    }
                    Invoice_Item_Name = Invoice_ds.Tables[0].Rows[x]["Item_Name"].ToString();
                    Item_Id = long.Parse(Invoice_ds.Tables[0].Rows[x]["Item_id"].ToString());
                    Specification = Invoice_ds.Tables[0].Rows[x]["Specification"].ToString();
                    Larg_Qnty = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Larg_Qnty"].ToString());
                    Small_Qnty = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Small_Qnty"].ToString());
                    Qnty_Another = Invoice_ds.Tables[0].Rows[x]["Qnty_Another"].ToString();
                    Larg_Unit_Price = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Larg_Unit_Price"].ToString());
                    Small_Unit_Price = 0;
                    Qnty_value = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Qnty_value"].ToString());
                    Total_Value = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Item_Net_Total_Price"].ToString());
                    ATS05_ = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["ATS05"].ToString());
                    TaxSales10_Value = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["TaxSales10_Value"].ToString());
                    TaxSales10_ = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["TaxSales10"].ToString());
                    Unit_Used_for_Price = 0;
                    unit_id = long.Parse(Invoice_ds.Tables[0].Rows[x]["unit_id"].ToString());
                    Unit_Item_Table_Id = long.Parse(Invoice_ds.Tables[0].Rows[x]["Unit_Item_Table_Id"].ToString());
                    code = Invoice_ds.Tables[0].Rows[x]["Code"].ToString();
                    Notes_ = "";
                    Balance_Checked = 1;
                    //  TaxSales10_Value = Qnty_value * TaxSales10_ /100;
                    // ATS05_Value = Qnty_value * ATS05_ /100 ;

                    //≈÷«›… ’‰› ›Ï ›« Ê—…


                    if (Common.Project_Market == 1)
                    {
                        mysql = " UPDATE Unit_item SET Full_qua =Full_qua - " + Larg_Qnty + ", D_Qua =D_Qua - " + Small_Qnty + ", Item_No =Item_No - " + Qnty_Another + " WHERE (id = " + Unit_Item_Table_Id + ") ";
                        //arr.Add(mysql);
                        opp.insert_me(mysql);


                        //mysql = "  UPDATE Unit_item SET Full_qua = FLOOR((Small_unit * Full_qua + D_Qua) / Small_unit), ";
                        //mysql = mysql + " D_Qua =   (( Small_unit * Full_qua + D_Qua) / Small_unit - FLOOR(( Small_unit * Full_qua + D_Qua) /  Small_unit)) *  Small_unit  ";
                        //mysql = mysql + " WHERE (id = " + Unit_Item_Table_Id + ")  ";
                        mysql = "  UPDATE Unit_item SET Full_qua = FLOOR((Small_unit * Full_qua + D_Qua) / Small_unit), ";
                        mysql = mysql + " D_Qua =    (Small_unit * Full_qua + D_Qua) % Small_unit  ";
                        mysql = mysql + " WHERE (id = " + Unit_Item_Table_Id + ")  ";

                        //arr.Add(mysql);
                        opp.insert_me(mysql);
                        Make_Order = 1;
                    }

                    string NameSpecification = Invoice_ds.Tables[0].Rows[x]["NameSpecification"].ToString();

                    decimal Item_Net_Total_Price = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Item_Net_Total_Price"].ToString());
                    decimal Item_Total_WithoutTax = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Item_Total_WithoutTax"].ToString());
                    string QntyStr = Invoice_ds.Tables[0].Rows[x]["QntyStr"].ToString();
                    string QntyStrColumn = Invoice_ds.Tables[0].Rows[x]["QntyStrColumn"].ToString();
                    decimal Item_TaxSales10_Value = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Item_TaxSales10_Value"].ToString());
                    long Store_Id = long.Parse(Invoice_ds.Tables[0].Rows[x]["Store_Id"].ToString());

                    mysql = " INSERT INTO Invoices_Details (Invoice_Item_Name,Invoice_Id, Item_Id, Larg_Qnty, Small_Qnty,Qnty_Another, Larg_Unit_Price, Small_Unit_Price, Qnty_value,Total, ATS05, TaxSales10,TaxSales10_Value,ATS05_Value,Unit_Used_for_Price,unit_id,Unit_Item_Table_Id,code, Notes,Balance_Checked,Specification,Make_Order,NameSpecification,  Item_Net_Total_Price, Item_Total_WithoutTax, QntyStr,     QntyStrColumn,    Item_TaxSales10_Value,Store_Id) ";
                    mysql = mysql + " VALUES (N'" + Invoice_Item_Name + "'," + Invoice_Id + "," + Item_Id + "," + Larg_Qnty + "," + Small_Qnty + ",'" + Qnty_Another + "'," + Larg_Unit_Price + "," + Small_Unit_Price + "," + Qnty_value + "," + Total_Value + "," + ATS05_ + "," + TaxSales10_ + "," + TaxSales10_Value + "," + ATS05_Value + "," + Unit_Used_for_Price + "," + unit_id + "," + Unit_Item_Table_Id + ",'" + code + "','" + Notes_ + "'," + Balance_Checked + ",'" + Specification + "'," + Make_Order + ", '" + NameSpecification + "', " + Item_Net_Total_Price + "," + Item_Total_WithoutTax + ", '" + QntyStr + "',  '" + QntyStrColumn + "',    " + Item_TaxSales10_Value + "," + Store_Id + ") ";
                    //arr.Add(mysql);
                    opp.insert_me(mysql);

                }

                CustomersAccountCLS ca = new CustomersAccountCLS();
                ca.Set_Customer_Balance_Invoice(ref Excp, ref opp, Customer_Id, Invoice_Id, Net_Total, Notes);



                ////make order Money
                if (Make_Invoice_Agile == 0)
                {
                    Money_Transaction_BoxOrderReportCLS mo = new Money_Transaction_BoxOrderReportCLS();
                    //, long CarsWorkOrdersId, long Reference_Type_Table_Id, long DataDictionaryProcessesListsId



                    //   ,   CarsWorkOrdersId,   Reference_Type_Table_Id,   DataDictionaryProcessesListsId


                    mo.Invoice_Type = Common.Invoice_Type_Id;

                    //mo.MakeMoney_TransactionFromInvoice(ref Excp, ref Msg, ref opp, Invoice_Date, Invoice_No, Invoice_Id, Box_Id, Currency_id, Customer_Id, PaidValueFromMarket, RestValue, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);

                    /////////
                    ///
                    InvoicesOperationListCLS invL_O = new InvoicesOperationListCLS(ref opp, 0);
                    invL_O.Get_Invoices_List(Invoice_Id);
                    long CarsWorkOrdersId = 0;
                    long Reference_Type_Table_Id = invL_O.invL.Id;
                    long DataDictionaryProcessesListsId = Common.Invoice_Type_Id;

                    mo.MakeMoney_TransactionFromInvoice(ref Excp, ref Msg, ref opp, invL_O.invL.Invoice_Date.Value, invL_O.invL.Invoice_Id ?? 0, invL_O.invL.Id, Box_Id, invL_O.invL.Currency_id ?? 0, invL_O.invL.Customer_Id ?? 0, PaidValueFromMarket, RestValue, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);

                    //try
                    //{


                    //}
                    //catch (Exception ee)
                    //{


                    //}


                }
                //«‰‘«¡ «·ﬁÌœ
                if (Common.IS_Make_Automatic_Queeod == 1)

                {
                    Account_Entry_Invoice_CLS aa = new Account_Entry_Invoice_CLS();
                    aa.IsNotDeletePrevEntry = true;
                    aa.Make_EntryForInvoices(ref Excp, ref Msg, ref opp, New_Invoice_Id, Invoice_Date, Invoice_Date, 0);
                }

                if (Excp == 1)
                {
                    opp.Rollback();
                }
                else
                {
                    opp.Commit();
                    Excp = 0;
                }

            }
            catch (Exception ee)
            {
                opp.Rollback();
                Excp = 1;
            }
        }

       
        public void Edit_Invoice_Update_CustomerAccount_POS(ref int Excp,ref string Msg, long Box_Id, DataSet dss, long Invoice_List_Table_Id, DateTime Invoice_Date, decimal InvoiveTotal, decimal NetInvoiceTotal, decimal Commision, decimal CommissionValue, string PolicaNo, string TroodContainerNo, int Make_Order, decimal Invoice_Net_Total_Old, decimal Invoice_Total_Old, decimal ATS05, decimal TaxSales10, decimal CustomerAccountAfter, long Currency_Id, string Notes, long SubAccount_Id, string SubAccount_Name)

        {
            InvoiceDataSetCLS ids = new InvoiceDataSetCLS();
            DataSet New_Items_ds_Add = new DataSet();
            DataSet New_Items_ds_Minus = new DataSet();
            New_Items_ds_Add = ids.MakeInvoiceDataSet();
            New_Items_ds_Minus = ids.MakeInvoiceDataSet();

            ////////////////////////////////////////////////////
            //bool RETURNOK = false;
            //Get_Store_WhenEditInvoice(ref dss, ref RETURNOK);
            //if (RETURNOK == false)
            //{
            //    Excp = 10;
            //    return;
            //}


            operations opp = new operations(1);
            try
            {
                string mysql = "";
                //ArrayList arr = new ArrayList();




                decimal Larg_Qnty;
                decimal Small_Qnty;
                decimal Qnty_Another;
                decimal OLD_Larg_Qnty = 0;
                decimal OLD_Small_Qnty = 0;
                decimal OLD_Item_No = 0;


                decimal Larg_Unit_Price;

                decimal Qnty_value;
                decimal Total_Value;


                decimal TaxSales10_Value = decimal.Parse(InvoiveTotal.ToString()) * TaxSales10 / 100;
                decimal ATS05_Value = decimal.Parse(InvoiveTotal.ToString()) * ATS05 / 100; ;
                decimal Unit_Used_for_Price;
                string Specification;

                long Item_Id;
                long unit_id;
                long Unit_Item_Table_Id;
                string code;
                decimal Small_Unit_Price;
                long Invoices_Details_Id;
                decimal TaxSales10_ValueTotal = 0;
                decimal ATS05_ValueTotal = 0;
                bool DeleteInvoice = false;
                long Store_Id = 0;

                decimal ATS05_ = 0;
                decimal TaxSales10_ = 0;
                /////////////////Set All Commision
                decimal Total_Item_CommissionValue = 0;
                for (int x = 0; x < dss.Tables[0].Rows.Count; x++)
                {
                    Total_Item_CommissionValue = Total_Item_CommissionValue + decimal.Parse(dss.Tables[0].Rows[x]["Item_CommissionValue"].ToString());
                }
                decimal Total_CommissionValue = Total_Item_CommissionValue + CommissionValue;
                mysql = " UPDATE Invoices_List SET  Total_Item_CommissionValue=" + Total_Item_CommissionValue + ",Total_CommissionValue=    " + Total_CommissionValue + " WHERE (id = " + New_Invoice_Id + ") ";
                opp.insert_me(mysql);
                ///////////////////////////////////////////////////////////////////////////////

                ///get the customer acount id
                DataSet dsss = opp.return_dataset(" SELECT Invoices_List.Account_Entry_List_ID,Invoices_List.Net_Total, Customers_Accounts.* FROM Invoices_List INNER JOIN  Customers_Accounts ON Invoices_List.ID = Customers_Accounts.Reference_Type_Table_Id  Where Reference_Type=1 And Reference_Type_Table_Id=" + Invoice_List_Table_Id + "");
                long Customers_Accounts_Id = 0;
                Customers_Accounts_Id = long.Parse(dsss.Tables[0].Rows[0]["Id"].ToString());
                long Account_Entry_List_ID = long.Parse(dsss.Tables[0].Rows[0]["Account_Entry_List_ID"].ToString());
                InvoiceDataSetCLS ii = new InvoiceDataSetCLS();
                /////// check the editableitem
                for (int x = 0; x < dss.Tables[0].Rows.Count; x++)
                {
                    //if (x == 12)
                    //{
                    //    int bb = 0;
                    //}
                    Store_Id = long.Parse(dss.Tables[0].Rows[x]["Store_Id"].ToString());
                    Larg_Qnty = decimal.Parse(dss.Tables[0].Rows[x]["Larg_Qnty"].ToString());
                    Small_Qnty = decimal.Parse(dss.Tables[0].Rows[x]["Small_Qnty"].ToString());
                    try
                    {
                        Qnty_Another = decimal.Parse(dss.Tables[0].Rows[x]["Qnty_Another"].ToString());
                    }
                    catch (Exception ee)
                    {

                        Qnty_Another = 0;
                    }
                    Larg_Unit_Price = decimal.Parse(dss.Tables[0].Rows[x]["Larg_Unit_Price"].ToString());

                    Qnty_value = decimal.Parse(dss.Tables[0].Rows[x]["Qnty_value"].ToString());
                    Total_Value = decimal.Parse(dss.Tables[0].Rows[x]["Item_Total"].ToString());
                    // ATS05 = decimal.Parse(dss.Tables[0].Rows[x]["ATS05"].ToString());
                    // TaxSales10 = decimal.Parse(dss.Tables[0].Rows[x]["TaxSales10"].ToString());
                    //  TaxSales10_Value = decimal.Parse(dss.Tables[0].Rows[x]["TaxSales10_Value"].ToString());
                    //  ATS05_Value = decimal.Parse(dss.Tables[0].Rows[x]["ATS05_Value"].ToString());
                    Unit_Used_for_Price = decimal.Parse(dss.Tables[0].Rows[x]["Unit_Used_for_Price"].ToString());
                    Specification = dss.Tables[0].Rows[x]["Specification"].ToString();
                    string NameSpecification = dss.Tables[0].Rows[x]["Item_Name"].ToString() + "  " + dss.Tables[0].Rows[x]["Specification"].ToString();
                    string Invoice_Item_Name = dss.Tables[0].Rows[x]["Item_Name"].ToString();



                    ATS05 = decimal.Parse(dss.Tables[0].Rows[x]["ATS05_Value"].ToString());
                    TaxSales10 = decimal.Parse(dss.Tables[0].Rows[x]["TaxSales10_Value"].ToString());

                    //   TaxSales10_ValueTotal = TaxSales10_ValueTotal + TaxSales10_Value;
                    // ATS05_ValueTotal = ATS05_ValueTotal + ATS05_Value;

                    Item_Id = long.Parse(dss.Tables[0].Rows[x]["Item_id"].ToString());
                    unit_id = long.Parse(dss.Tables[0].Rows[x]["unit_id"].ToString());
                    Unit_Item_Table_Id = long.Parse(dss.Tables[0].Rows[x]["Unit_Item_Table_Id"].ToString());
                    code = dss.Tables[0].Rows[x]["Code"].ToString();
                    Small_Unit_Price = 0;// decimal.Parse(dss.Tables[0].Rows[x]["S_unit_Value"].ToString());
                    int Is_Item_CommissionValue = int.Parse(dss.Tables[0].Rows[x]["Is_Item_CommissionValue"].ToString());
                    decimal Item_CommissionValue = decimal.Parse(dss.Tables[0].Rows[x]["Item_CommissionValue"].ToString());
                    decimal Unit_CommissionValue = decimal.Parse(dss.Tables[0].Rows[x]["Unit_CommissionValue"].ToString());
                    //  , Is_Item_CommissionValue,Item_CommissionValue
                    //  ,  " + Is_Item_CommissionValue + "," + Item_CommissionValue + "


                    try
                    {
                        Invoices_Details_Id = long.Parse(dss.Tables[0].Rows[x]["Invoices_Details_Id"].ToString());
                    }
                    catch
                    {
                        Invoices_Details_Id = 0;
                    }

                    ///get the last Qnty After Edit
                    ///

                    DataSet dsN = opp.return_dataset("Select * from Invoices_Details Where Id=" + Invoices_Details_Id + "");
                    if (dsN.Tables[0].Rows.Count > 0)
                    {
                        OLD_Larg_Qnty = decimal.Parse(dsN.Tables[0].Rows[0]["Larg_Qnty"].ToString());
                        OLD_Small_Qnty = decimal.Parse(dsN.Tables[0].Rows[0]["Small_Qnty"].ToString());
                        OLD_Item_No = decimal.Parse(dsN.Tables[0].Rows[0]["Qnty_Another"].ToString());
                    }


                    Make_Order = 0;// int.Parse(dss.Tables[0].Rows[x]["Make_Order"].ToString());
                    if (dss.Tables[0].Rows[x]["Item_DeletedFlage"].ToString() == "1")
                    {
                        mysql = "Delete From Invoices_Details  where id=" + Invoices_Details_Id;
                        opp.insert_me(mysql);

                    }
                    else if (dss.Tables[0].Rows[x]["Item_DeletedFlage"].ToString() == "-1")
                    {

                        mysql = " INSERT INTO Invoices_Details (Store_Id,Invoice_Id, Item_Id, Larg_Qnty, Small_Qnty,Qnty_Another, Larg_Unit_Price, Small_Unit_Price, Qnty_value,Total, ATS05, TaxSales10,TaxSales10_Value,ATS05_Value,Unit_Used_for_Price,unit_id,Unit_Item_Table_Id,code, Notes,Balance_Checked,Make_Order,Specification,Invoice_Item_Name,NameSpecification, Is_Item_CommissionValue,Item_CommissionValue,Unit_CommissionValue) ";
                        mysql = mysql + " VALUES (" + Store_Id + "," + Invoice_List_Table_Id + "," + Item_Id + "," + Larg_Qnty + "," + Small_Qnty + ",'" + Qnty_Another + "'," + Larg_Unit_Price + "," + Small_Unit_Price + "," + Qnty_value + "," + Total_Value + "," + ATS05 + "," + TaxSales10 + "," + TaxSales10_Value + "," + ATS05_Value + "," + Unit_Used_for_Price + "," + unit_id + "," + Unit_Item_Table_Id + ",'" + code + "',' ', 1, " + Make_Order + ",N'" + Specification + "' ,N'" + Invoice_Item_Name + "',N'" + NameSpecification + "',  " + Is_Item_CommissionValue + "," + Item_CommissionValue + "," + Unit_CommissionValue + ") ";
                        opp.insert_me(mysql);
                    }
                    else
                    {

                        mysql = " Update Invoices_Details Set  Is_Item_CommissionValue=  " + Is_Item_CommissionValue + ",Item_CommissionValue= " + Item_CommissionValue + ",Unit_CommissionValue = " + Unit_CommissionValue + ", Larg_Qnty=" + Larg_Qnty + ", Small_Qnty=" + Small_Qnty + ",Qnty_Another='" + Qnty_Another + "', Larg_Unit_Price=" + Larg_Unit_Price + ",   Qnty_value=" + Qnty_value + ",Total=" + Total_Value + ", ATS05=" + ATS05 + " ";
                        mysql = mysql + " , TaxSales10=" + TaxSales10 + ",TaxSales10_Value=" + TaxSales10_Value + ",ATS05_Value=" + ATS05_Value + ",Unit_Used_for_Price=" + Unit_Used_for_Price + " , Specification='" + Specification + "' ,Invoice_Item_Name=N'" + Invoice_Item_Name + "',NameSpecification=N'" + NameSpecification + "'";
                        mysql = mysql + " Where Id=" + Invoices_Details_Id;
                        opp.insert_me(mysql);
                    }



                    //if (Common.Project_Market == 1)
                    //{
                    //    int Transaction_Type = 2;
                    //    int Invoice_Type_Id = 1;
                    //    ii.Set_ItemTransactionFromInvoices(ref opp, Unit_Item_Table_Id, Larg_Qnty, Small_Qnty, decimal.Parse(Qnty_Another.ToString()), 0, Transaction_Type, Invoice_Date, Invoice_List_Table_Id, Invoices_Details_Id, Invoice_Type_Id, "›« Ê—… „»Ì⁄« ", Notes, "");

                    //}


                    //////////////////////////////////////////////
                    #region Form Project_Market=1
                    if (Common.Project_Market == 1)
                    {
                        if (dss.Tables[0].Rows[x]["Item_DeletedFlage"].ToString() == "1")
                        {
                            //Õ–›
                            mysql = " UPDATE Unit_item SET Full_qua =Full_qua + " + Larg_Qnty + ", D_Qua =D_Qua + " + Small_Qnty + ", Item_No =Item_No + " + Qnty_Another + " WHERE (id = " + Unit_Item_Table_Id + ") ";
                            opp.insert_me(mysql);
                        }
                        else if (dss.Tables[0].Rows[x]["Item_DeletedFlage"].ToString() == "-1")
                        {//«÷«›…
                            mysql = " UPDATE Unit_item SET Full_qua =Full_qua - " + Larg_Qnty + ", D_Qua =D_Qua - " + Small_Qnty + ", Item_No =Item_No - " + Qnty_Another + " WHERE (id = " + Unit_Item_Table_Id + ") ";
                            opp.insert_me(mysql);
                        }
                        else
                        {
                            //DataSet dsN = opp.return_dataset("Select * from Invoices_Details Where Id=" + Invoices_Details_Id + "");
                            //decimal OLD_Larg_Qnty = decimal.Parse(dsN.Tables[0].Rows[0]["Larg_Qnty"].ToString());
                            //decimal OLD_Small_Qnty = decimal.Parse(dsN.Tables[0].Rows[0]["Small_Qnty"].ToString());
                            //decimal OLD_Item_No = decimal.Parse(dsN.Tables[0].Rows[0]["Qnty_Another"].ToString());

                            Larg_Qnty = Larg_Qnty - OLD_Larg_Qnty;
                            Small_Qnty = Small_Qnty - Small_Qnty;
                            Qnty_Another = Qnty_Another - OLD_Item_No;

                            mysql = " UPDATE Unit_item SET Full_qua =Full_qua - " + Larg_Qnty + ", D_Qua =D_Qua - " + Small_Qnty + ", Item_No =Item_No - " + Qnty_Another + " WHERE (id = " + Unit_Item_Table_Id + ") ";
                            opp.insert_me(mysql);


                        }

                        //mysql = "  UPDATE Unit_item SET Full_qua = FLOOR((Small_unit * Full_qua + D_Qua) / Small_unit), ";
                        //mysql = mysql + " D_Qua =   (( Small_unit * Full_qua + D_Qua) / Small_unit - FLOOR(( Small_unit * Full_qua + D_Qua) /  Small_unit)) *  Small_unit  ";
                        //mysql = mysql + " WHERE (id = " + Unit_Item_Table_Id + ")  ";
                        mysql = "  UPDATE Unit_item SET Full_qua = FLOOR((Small_unit * Full_qua + D_Qua) / Small_unit), ";
                        mysql = mysql + " D_Qua =    (Small_unit * Full_qua + D_Qua) % Small_unit  ";
                        mysql = mysql + " WHERE (id = " + Unit_Item_Table_Id + ")  ";
                        opp.insert_me(mysql);
                    }

                    #endregion
                }
                int Customer_id = int.Parse(dss.Tables[0].Rows[0]["Customer_Id"].ToString());
                //DateTime Invoice_Date = DateTime.Parse(dss.Tables[0].Rows[0]["Invoice_Date"].ToString());
               
                ArrayList mysql_Arr = new ArrayList();


                ///to delete  the invoice
                int CountToDelete = 0;
                for (int x = 0; x < dss.Tables[0].Rows.Count; x++)
                {
                    if (dss.Tables[0].Rows[x]["Item_DeletedFlage"].ToString() == "1")
                    {
                        CountToDelete++;
                    }
                }
                if (CountToDelete == dss.Tables[0].Rows.Count)
                {
                    mysql = "Delete From Invoices_List  where id=" + Invoice_List_Table_Id;
                    opp.insert_me(mysql);

                    mysql = " Delete from Customers_Accounts  WHERE (id = " + Customers_Accounts_Id + ")";
                    opp.insert_me(mysql);
                }
                mysql = "UPDATE Invoices_List SET  Invoice_Date ='" + operations.dateFormat(Invoice_Date) + "',ATS05=" + ATS05 + ", TaxSales10= " + TaxSales10 + ",Notes =N'" + Notes + "',Total =" + InvoiveTotal + ",  Net_Total=" + NetInvoiceTotal + " ,commission=" + Commision + ",CommissionValue = " + CommissionValue + ",ATS05_Value=" + ATS05_Value + " ,TaxSales10_Value=" + TaxSales10_Value + " ,CustomerAccountAfter=" + CustomerAccountAfter + " WHERE (id = " + Invoice_List_Table_Id + ")";
                opp.insert_me(mysql);

                ///////// check customer balance
                decimal Invoice_NetTotal_Diff = Invoice_Net_Total_Old - NetInvoiceTotal;


                //mysql = "UPDATE Customer SET balance =balance - " + Invoice_NetTotal_Diff + " WHERE (id = " + Customer_id + ")";
                //opp.insert_me(mysql);

                //mysql = "UPDATE Customers_Accounts SET Notes=N'" + Notes + "',Export=" + NetInvoiceTotal + "   WHERE (id = " + Customers_Accounts_Id + ")";
                //opp.insert_me(mysql);

                //mysql = "UPDATE Customers_Accounts SET balance=balance - " + Invoice_NetTotal_Diff + "   WHERE (id >= " + Customers_Accounts_Id + ") AND (Customer_Id=" + Customer_id + ")";
                //opp.insert_me(mysql);

                CustomersAccountCLS ca = new CustomersAccountCLS();
                ca.Set_Customer_Balance_Invoice(ref Excp, ref opp, Customer_id, Invoice_List_Table_Id, NetInvoiceTotal, Notes);
                ////make order Money
                if (Make_Invoice_Agile == 0)
                {
                    Money_Transaction_BoxOrderReportCLS mt = new Money_Transaction_BoxOrderReportCLS();

                    Excp = 0;

                    int Transaction_Type = 1;
                    if (Box_Id == 0)
                    {
                        Box_Id = mt.GetDefaultBank_ID();// int.Parse(ot.banks_List1.cmbBankType.SelectedValue.ToString());
                    }
                    decimal Box_Balance = 0;
                    string Order_No = mt.Make_New_Order_No(ref Excp, Transaction_Type).ToString();
                    mysql = "Select * from Banks Where Id=" + Box_Id;
                    DataTable dtBanks = opp.return_DataTable(mysql);
                    if (dtBanks.Rows.Count > 0)
                    {

                        Box_Balance = decimal.Parse(dtBanks.Rows[0]["Last_Balance"].ToString());
                        decimal Cach_Value = PaidValueFromMarket;// ot.nuValue.Value;
                        DateTime Order_Date = Invoice_Date;
                        string Man_Name = " ›« Ê—… „»Ì⁄«  —ﬁ„ " + "Invoice_No";
                        int Reverse_Order = 0;
                        long Order_Mony_Id2 = 0;
                        mt.Invoice_ID = Invoice_List_Table_Id;

                        //, long CarsWorkOrdersId, long Reference_Type_Table_Id, long DataDictionaryProcessesListsId

                        long CarsWorkOrdersId = 0;
                        long Reference_Type_Table_Id = 0;
                        long DataDictionaryProcessesListsId = 0;
                        long Entry_No = 0;
                        //   ,   CarsWorkOrdersId,   Reference_Type_Table_Id,   DataDictionaryProcessesListsId


                        long Money_Transaction_Box_ID = 0;
                        mt.Make_Transaction_BanksBox_FromInvoices(ref Excp, ref Msg, ref opp, ref Order_Mony_Id2, ref Money_Transaction_Box_ID, Transaction_Type, Order_No, Box_Id, Box_Balance, Cach_Value, Order_Date, Man_Name, Currency_Id, Customer_id, Reverse_Order, Entry_No, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);
                        ////print Order
                        //Order_Receipt_Cash oo = new Order_Receipt_Cash();
                        //DataSet dsOrderMoney = oo.Get_Order_Cash(ref Excp, Order_Mony_Id2);
                        //Order_Receipt_CashFrm or = new Order_Receipt_CashFrm(Order_Mony_Id2, Transaction_Type, dsOrderMoney, 0);
                        //or.LoadDataOnForm();
                        //or.Set_Print(1);
                        //or = null;

                        string Bank_Name = dtBanks.Rows[0]["Bank_Name"].ToString();

                        mysql = "Update Invoices_List Set BanksNamePaymentMethod =N'" + Bank_Name + "' ,RestValue=" + RestValue + ",PaidValue=" + PaidValueFromMarket + " , Order_MoneyId=" + Order_Mony_Id2 + ", BanksId=" + Box_Id + " Where Id=" + Invoice_List_Table_Id;
                        opp.insert_me(mysql);
                    }
                    else
                    {
                        Msg = Msg + "\n";
                        Msg = "·„ Ì „ ≈’œ«— «·‰ﬁœÌ… —»„« ·⁄œ„ ÊÃÊœ ’‰œÊﬁ ≈› —«÷Ï ··›Ê« Ì—";
                    }
                }

                Excp = 0;
                ///«‰‘«¡ «·ﬁÌœ
                //«‰‘«¡ «·ﬁÌœ
                if (Common.IS_Make_Automatic_Queeod == 1)

                {
                    Account_Entry_Invoice_CLS aa = new Account_Entry_Invoice_CLS();
                    aa.IsNotDeletePrevEntry = false;
                    aa.Make_EntryForInvoices(ref Excp, ref Msg, ref opp, New_Invoice_Id, Invoice_Date, Invoice_Date, 0);
                }
                if (Excp == 1)
                {
                    opp.Rollback();
                }
                else
                {
                    opp.Commit();
                    Excp = 0;
                }


            }
            catch (Exception e)
            {
                opp.Rollback();
                Excp = 1;
                Msg = e.Message;
                //throw (e);
            }

        }

        public void Finish_Request(ref int Excp, int Request_Id)
        {
            try
            {

                string mysql = " update  Requests_Details  set Item_Exported_Finish=1 where id=" + Request_Id;
                operations opp = new operations();

                opp.delete_me(mysql);
                opp = null;
                Excp = 0;

            }
            catch (Exception e)
            {
                Excp = 1;
                throw (e);
            }
        }
        public void Delete_Request(ref int Excp, int Request_Id)
        {
            try
            {

                string mysql = " Delete from Requests_Details where id=" + Request_Id;
                operations opp = new operations();

                opp.delete_me(mysql);
                opp = null;
                Excp = 0;

            }
            catch (Exception e)
            {
                Excp = 1;
                throw (e);
            }
        }

        public DataTable GetInvoiceTotalAnalaytic(DateTime dtFrom, DateTime dtTo)
        {

            string mysql = @"SELECT        Item.Item_Name, Invoices_List.Invoice_Date, Invoices_Details.Larg_Qnty, Invoices_Details.Small_Qnty, Invoices_Details.Unit_Id, unit.B_Unit, unit.S_unit, 
                         Invoices_Details.Larg_Unit_Price, Invoices_Details.Small_Unit_Price, Invoices_Details.Total, Customer.Name
FROM            Invoices_List INNER JOIN
                         Invoices_Details ON Invoices_List.ID = Invoices_Details.Invoice_Id INNER JOIN
                         Item ON Invoices_Details.Item_Id = Item.id INNER JOIN
                         unit ON Invoices_Details.Unit_Id = unit.id INNER JOIN
                         Customer ON Invoices_List.Customer_Id = Customer.Id ";
            mysql = mysql + " WHERE(Invoices_List.Invoice_Date >= '" + operations.dateFormat(dtFrom.Date) + "') AND(Invoices_List.Invoice_Date <= '" + operations.dateFormat(dtTo.Date) + "')";

            operations opp = new operations();
            DataTable dt = opp.return_DataTable(mysql);
            return dt;

        }
        void Fill_Ds(ref DataSet New_Items_ds_Add, DataSet dss, int x)
        {
            DataRow dr;
            dr = New_Items_ds_Add.Tables[0].NewRow();

            try
            {

                dr["Make_Order"] = 0;
                dr["Item_Name"] = "";
                dr["ATS05"] = 0;
                dr["ATS05_Value"] = 0;
                dr["TaxSales"] = 0;
                dr["TaxSales10_Value"] = 0;
                dr["Unit_price"] = decimal.Parse(dss.Tables[0].Rows[x]["S_unit_Value"].ToString());

                dr["Net_Total_Price"] = 0;
                dr["Larg_Qnty"] = decimal.Parse(dss.Tables[0].Rows[x]["Larg_Qnty"].ToString());
                dr["Small_Qnty"] = decimal.Parse(dss.Tables[0].Rows[x]["Small_Qnty"].ToString());
                dr["Qnty_Another"] = decimal.Parse(dss.Tables[0].Rows[x]["Qnty_Another"].ToString());
                dr["Total_Price"] = 0;
                dr["PriceUnitIndex"] = decimal.Parse(dss.Tables[0].Rows[x]["Unit_Used_for_Price"].ToString());
                //dr["Currency_id"] =
                dr["Item_id"] = dss.Tables[0].Rows[x]["Item_id"].ToString();
                dr["unit_id"] = dss.Tables[0].Rows[x]["unit_id"].ToString();
                dr["Unit_Item_Table_Id"] = dss.Tables[0].Rows[x]["Unit_Item_Table_Id"].ToString();
                dr["Code"] = dss.Tables[0].Rows[x]["Code"].ToString();
                dr["Available_Larg_Balance"] = 0;
                dr["Available_Small_Balance"] = 0;
                dr["id"] = "0";
                dr["S_Unit"] = dss.Tables[0].Rows[x]["S_Unit"].ToString();
                dr["B_Unit"] = dss.Tables[0].Rows[x]["B_Unit"].ToString();
                dr["Specification"] = dss.Tables[0].Rows[x]["Specification"].ToString();
                dr["Store_Id"] = dss.Tables[0].Rows[x]["Store_Id"].ToString();

                New_Items_ds_Add.Tables[0].Rows.Add(dr);

            }
            catch (Exception ee)
            { }
        }
        void Fill_Ds(ref DataSet New_Items_ds_Add, DataSet dss, int x, decimal Will_Export_Qnty, decimal Will_Export_QntySmall)
        {
            DataRow dr;
            dr = New_Items_ds_Add.Tables[0].NewRow();

            try
            {

                dr["Make_Order"] = 0;
                dr["Item_Name"] = "";
                dr["ATS05"] = 0;
                dr["ATS05_Value"] = 0;
                dr["TaxSales"] = 0;
                dr["TaxSales10_Value"] = 0;
                dr["Unit_price"] = decimal.Parse(dss.Tables[0].Rows[x]["S_unit_Value"].ToString());

                dr["Net_Total_Price"] = 0;
                dr["Larg_Qnty"] = Will_Export_Qnty;
                dr["Small_Qnty"] = Will_Export_QntySmall;
                dr["Qnty_Another"] = decimal.Parse(dss.Tables[0].Rows[x]["Qnty_Another"].ToString());
                dr["Total_Price"] = 0;
                dr["PriceUnitIndex"] = decimal.Parse(dss.Tables[0].Rows[x]["Unit_Used_for_Price"].ToString());
                //dr["Currency_id"] =
                dr["Item_id"] = dss.Tables[0].Rows[x]["Item_id"].ToString();
                dr["unit_id"] = dss.Tables[0].Rows[x]["unit_id"].ToString();
                dr["Unit_Item_Table_Id"] = dss.Tables[0].Rows[x]["Unit_Item_Table_Id"].ToString();
                dr["Code"] = dss.Tables[0].Rows[x]["Code"].ToString();
                dr["Available_Larg_Balance"] = 0;
                dr["Available_Small_Balance"] = 0;
                dr["id"] = "0";
                dr["S_Unit"] = dss.Tables[0].Rows[x]["S_Unit"].ToString();
                dr["B_Unit"] = dss.Tables[0].Rows[x]["B_Unit"].ToString();
                dr["Specification"] = dss.Tables[0].Rows[x]["Specification"].ToString();
                dr["Store_Id"] = dss.Tables[0].Rows[x]["Store_Id"].ToString();

                New_Items_ds_Add.Tables[0].Rows.Add(dr);

            }
            catch (Exception ee)
            { }
        }



       


        public void Get_arr_Order_Purshing_Id_Automatic_From_InvoiceReverse(long Invoice_Id, ref operations opp)
        {
            arr_Order_Purshing_Id_Automatic = new ArrayList();

            string mysql = "SELECT Order_Purshing_Id_Automatic FROM Invoices_Details_Reverse WHERE (Invoice_Id = " + Invoice_Id + ")";
            DataTable dt = opp.return_DataTable(mysql);
            for (int x = 0; x < dt.Rows.Count; x++)
            {
                arr_Order_Purshing_Id_Automatic.Add(dt.Rows[x]["Order_Purshing_Id_Automatic"].ToString());
            }
        }

        public ArrayList Get_arr_Order_Purshing_Id_Automatic_From_InvoiceReverse(long Invoice_Id)
        {
            arr_Order_Purshing_Id_Automatic = new ArrayList();
            operations opp = new operations();
            string mysql = "SELECT Order_Purshing_Id_Automatic FROM Invoices_Details_Reverse WHERE (Invoice_Id = " + Invoice_Id + ")";
            DataTable dt = opp.return_DataTable(mysql);
            for (int x = 0; x < dt.Rows.Count; x++)
            {
                arr_Order_Purshing_Id_Automatic.Add(dt.Rows[x]["Order_Purshing_Id_Automatic"].ToString());
            }
            return arr_Order_Purshing_Id_Automatic;

        }


        public void Make_Invoice_Update_CustomerAccountOOO(ref int Excp, ref DataSet Invoice_ds, ref string Msg, InvoicesOperationListCLS invL_O)
        {
            /*
           operations opp = new operations(1);

           try
           {
               //Net_Total = Common.RoundMe(Net_Total);
               //Total_PriceWithoutTax = Common.RoundMe(Total_PriceWithoutTax);
               //Commission = Common.RoundMe(Commission);

               //checked item_report to SendKeys to store
               DataView dvToStore = new DataView();
               dvToStore.Table = Invoice_ds.Tables[0];
               ItemToStore it = new ItemToStore(dvToStore);
               if (Common.Project_Market == 0)
               {
                   it.ShowDialog();
               }
               else
               {
                   it.ClickBtnOK();
               }
               ArrayList mArrMake_Order = new ArrayList();
               mArrMake_Order = it.ArrItemToStore;

               if (!it.BtnOkClick)
               {
                   Excp = 10;
                   return;
               }
               ////////////////////////////
               /////////////////// »—‰«„Ã «· ﬁ”Ìÿ
               Make_Takseet mt = new Make_Takseet();
               if (Common.Is_Takseet_Invoice == 1)
               {

                   mt.txtTotal_Money.Value = invL_O.invL.Net_Total ?? 0;
                   mt.dt = Invoice_ds.Tables[0];
                   mt.ShowDialog();
               }
               ///



               ////***********************************************************************
               ////////// Make_New_Invoice_Id
               DataTable dt = new DataTable();
               dt = Invoice_ds.Tables[0];

               string mysql = "";
               //decimal Total = 0;


               //for (int x = 0; x < Invoice_ds.Tables[0].Rows.Count; x++)
               //{
               //    try
               //    {

               //        Total = Total + decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Item_Net_Total_Price"].ToString());

               //        Total = Common.RoundMe(Total);
               //    }
               //    catch (Exception r)
               //    {


               //    }

               //}
               //TaxSales10_Value = Total * TaxSale10 / 100;

               //ATS05_Value = Total * ATS05 / 100;

               //InvoicesClss inv = new InvoicesClss();
               //inv.CommissionISValue = CommissionISValue;

               //inv.Make_New_Invoice_Id(ref Excp, ref opp, ref New_Invoice_Id, Invoice_No, Customer_Id, Invoice_Date, Invoice_Type, (decimal)Total, IsCommission_Value, Commission, CommissionValue, decimal.Parse(TaxSale10.ToString()), decimal.Parse(ATS05.ToString()), (decimal)TaxSales10_Value, (decimal)ATS05_Value, Net_Total, "", Currency_id, 1, Notes, PolicaNo, TroodContainerNo, CustomerAccountBefor, CustomerAccountAfter, SubAccount_Id, SubAccount_Name, AnotherPayment, Total_PriceWithoutTax, Salesman_Id, IS_Make_Sarf_Item_BalanceFromInvoice, Is_InvoiceKta3y);
               New_Invoice_Id = invL_O.Insert_Invoices_List(ref opp, ref Excp);
               //invL.Invoices_List_Id = New_Invoice_Id;
               if (Excp == 1)
               {
                   New_Invoice_Id = 0;
                   invL_O.Invoices_List_Id = 0;
                   opp.Rollback();
                   return;
               }
               ///////////////////Set All Commision
               //decimal Total_Item_CommissionValue = 0;
               //for (int x = 0; x < Invoice_ds.Tables[0].Rows.Count; x++)
               //{
               //    Total_Item_CommissionValue = Total_Item_CommissionValue + decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Net_Total_Item_Commission"].ToString());
               //}
               //decimal Total_CommissionValue = Total_Item_CommissionValue + CommissionValue;
               //mysql = " UPDATE Invoices_List SET Total_Item_CommissionValue=" + Total_Item_CommissionValue + ",Total_CommissionValue=    " + Total_CommissionValue + " WHERE (id = " + New_Invoice_Id + ") ";
               //opp.insert_me(mysql);
               ///////////////////////////////////////////////////////////////////////////////



               ///////////// add invoice Items and update request qnty

               ArrayList arr = new ArrayList();

               ////
               ///
               try
               {
                   invL_O.invL.CustomerName = Common.GetCustomerName(ref opp, invL_O.invL.Customer_Id.Value);
               }
               catch { }

               InvoiceDataSetCLS ii = new InvoiceDataSetCLS();
               //////////// add item in invoice details
               for (int x = 0; x < Invoice_ds.Tables[0].Rows.Count; x++)
               {
                   InvoicesOperationCLS invD_O = new InvoicesOperationCLS();
                   invD_O.invD.Id = New_Invoice_Id;

                   invD_O.Get_Item_Details(Invoice_ds, x);


                   if (Common.Project_Market == 1 & invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                   {

                       mysql = " UPDATE Unit_item SET Full_qua =Full_qua - " + invD_O.invD.Larg_Qnty + ", D_Qua =D_Qua - " + invD_O.invD.Small_Qnty + ", Item_No =Item_No - " + invD_O.invD.Qnty_Another + " WHERE (id = " + invD_O.invD.Unit_Item_Table_Id + ") ";
                       //arr.Add(mysql);
                       opp.insert_me(mysql);
                       mysql = " UPDATE Bills_Details   SET Larg_Qnty_Balance = Larg_Qnty_Balance - " + invD_O.invD.Larg_Qnty + ", Small_Qnty_Balance =Small_Qnty_Balance - " + invD_O.invD.Small_Qnty + ", Qnty_Another_Balance =Qnty_Another_Balance - " + invD_O.invD.Qnty_Another + " WHERE (id = " + invD_O.invD.Bills_Details_Id + ") ";
                       //arr.Add(mysql);
                       opp.insert_me(mysql);


                       //// «·»Ê‰’
                       mysql = " UPDATE Unit_item SET Full_qua =Full_qua - " + invD_O.invD.LargItemsQnty_Free + ", D_Qua =D_Qua - " + invD_O.invD.SmallItemsQnty_Free + ", Item_No =Item_No - " + invD_O.invD.Item_No_Free + " WHERE (id = " + invD_O.invD.Unit_Item_Table_Id + ") ";
                       opp.insert_me(mysql);

                       //// «·»Ê‰’
                       mysql = " UPDATE Bills_Details   SET Larg_Qnty_Balance = Larg_Qnty_Balance - " + invD_O.invD.LargItemsQnty_Free + ", Small_Qnty_Balance =Small_Qnty_Balance - " + invD_O.invD.SmallItemsQnty_Free + ", Qnty_Another_Balance =Qnty_Another_Balance - " + invD_O.invD.Item_No_Free + " WHERE (id = " + invD_O.invD.Bills_Details_Id + ") ";
                       opp.insert_me(mysql);



                       //CommonAccounts.SetBalanceWithExpireDate_Tash3ela_Invoice(ref opp, 2, invd.Unit_Item_ExpireDate_Id, invd.Invoices_Details_Id, (long)invd.Item_Id, (long)invd.Unit_Item_Table_Id, invd.Item_Expire_Date, invd.Tash3ela, invd.Larg_Qnty, invd.Small_Qnty, invd.Qnty_Another, invd.LargItemsQnty_Free, invd.SmallItemsQnty_Free, invd.Item_No_Free, 0, 0, 0);
                       CommonAccounts.SetBalanceWithExpireDate_Tash3ela_Invoice(ref opp, 2, (Invoices_Details)invD_O.invD, 0, 0, 0);

                       // Common.SetBalanceWithExpireDate_Tash3ela_Invoice(ref opp, 2, num51, num50, (long)num7, (long)num21, time, str13, num9, num10, decimal.Parse(str2), num34, num35, num36, 0M, 0M, 0M);
                       // SetBalanceWithExpireDate_Tash3ela_Invoice(ref operations opp, int Transaction, long Unit_Item_ExpireDate_Id, long Invoices_Details_Id, long Item_Id, long Unit_Item_Table_Id, DateTime Item_Expire_Date, string Tash3ela, decimal Larg_Balance, decimal Small_Balance, decimal Item_No, decimal LargItemsQnty_Free, decimal SmallItemsQnty_Free, decimal Item_No_Free, decimal OLD_Larg_Qnty, decimal OLD_Small_Qnty, decimal OLD_Item_NoQnty)


                       //mysql = "  UPDATE Unit_item SET Full_qua = FLOOR((Small_unit * Full_qua + D_Qua) / Small_unit), ";
                       //mysql = mysql + " D_Qua =   (( Small_unit * Full_qua + D_Qua) / Small_unit - FLOOR(( Small_unit * Full_qua + D_Qua) /  Small_unit)) *  Small_unit  ";
                       //mysql = mysql + " WHERE (id = " + Unit_Item_Table_Id + ")  ";


                       if (Common.Compute_ItemBalance_With_Fraction == 0)
                       {
                           mysql = "  UPDATE Unit_item SET Full_qua = FLOOR((Small_unit * Full_qua + D_Qua) / Small_unit), ";
                           mysql = mysql + " D_Qua =    (Small_unit * Full_qua + D_Qua) % Small_unit  ";
                           mysql = mysql + " WHERE (id = " + invD_O.invD.Unit_Item_Table_Id + ")  ";
                           opp.insert_me(mysql);
                       }
                       //arr.Add(mysql);



                       invL_O.Make_Order = 1;
                   }
                   else
                   {
                       if (Common.Make_Store_Order_Sarf_Estlam_Automatic_From_Invoice == 1)
                       {
                           invL_O.Make_Order = 1;
                       }
                       else
                       {
                           invL_O.Make_Order = int.Parse(mArrMake_Order[x].ToString());
                       }
                   }


                   invD_O.Invoices_Details_Id = invD_O.Insert_Invoices_Details(ref opp, ref Excp);
                   try
                   {
                       Invoice_ds.Tables[0].Rows[x]["Invoices_Details_Id"] = invD_O.invD.Id.ToString();
                   }
                   catch (Exception ee)
                   { }
                   if (Excp == 1)
                   {
                       New_Invoice_Id = 0;
                       invL_O.Invoices_List_Id = 0;
                       opp.Rollback();
                       return;
                   }

                   //   mysql = " INSERT INTO Invoices_Details (OperationNo,StatementNo,Merchant_Name,Store_Id,Invoice_Item_Name,Invoice_Id, Item_Id, Larg_Qnty, Small_Qnty,Qnty_Another, Larg_Unit_Price, Small_Unit_Price, Qnty_value,Total, ATS05, TaxSales10,TaxSales10_Value,ATS05_Value,Unit_Used_for_Price,unit_id,Unit_Item_Table_Id,code, Notes,Balance_Checked,Specification,NameSpecification,Make_Order , QntyWidth,QntyLenght,QntyItemNo,Is_InvoiceItem_Kta3y,Bills_Details_Id,  Audience_price ,Tash3ela  ,  Item_No_Free, LargItemsQnty_Free, SmallItemsQnty_Free ,Item_Expire_Date,Is_Item_CommissionValue,Item_CommissionValue,Unit_CommissionValue,Item_CommissionRate,Unit_CommissionRate,Is_Alternate_Item_CommissionValue, Alternate_Item_CommissionValue, Alternate_Unit_CommissionValue, Alternate_Item_CommissionRate, Alternate_Unit_CommissionRate ) ";
                   //  mysql = mysql + " VALUES ('" + OperationNo + "','" + StatementNo + "','" + Merchant_Name + "'," + Store_Id + ",'" + Invoice_Item_Name + "'," + Invoice_Id + "," + Item_Id + "," + Larg_Qnty + "," + Small_Qnty + ",'" + Qnty_Another + "'," + Larg_Unit_Price + "," + Small_Unit_Price + "," + Qnty_value + "," + Total_Value + "," + ATS05_ + "," + TaxSales10_ + "," + TaxSales10_Value + "," + ATS05_Value + "," + Unit_Used_for_Price + "," + unit_id + "," + Unit_Item_Table_Id + ",'" + code + "','" + Notes_ + "'," + Balance_Checked + ",N'" + Specification + "',N'" + NameSpecification + "'," + Make_Order + ",   " + QntyWidth + "," + QntyLenght + "," + QntyItemNo + "," + Is_InvoiceItem_Kta3y + "," + Bills_Details_Id + "," + Audience_price + ", '" + Tash3ela + "' , " + Item_No_Free + "," + LargItemsQnty_Free + "," + SmallItemsQnty_Free + " ,'" + operations.dateFormat(Item_Expire_Date) + "'," + Is_Item_CommissionValue + "," + Item_CommissionValue + "," + Unit_CommissionValue + "," + Item_CommissionRate + "," + Unit_CommissionRate + "," + Is_Alternate_Item_CommissionValue + "," + Alternate_Item_CommissionValue + "," + Alternate_Unit_CommissionValue + "," + Alternate_Item_CommissionRate + "," + Alternate_Unit_CommissionRate + ") ";


                   //  Invoices_Details_Id = opp.Insert_me_Return_Last_Identify(mysql);
                   int Transaction_Type = 2;
                   int Invoice_Type_Id = Common.Invoice_Type_Id;
                   if (Common.Project_Market == 1 & invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                   {
                       try
                       {
                           invD_O.invD.Store_Name = Common.GetStoreName(ref opp, invD_O.invD.Store_Id ?? 0);
                       }
                       catch { }

                       //ii.Set_ItemTransactionFromInvoices(ref opp, invd.Unit_Item_Table_Id, invd.Item_Net_Total_Price, invd.Larg_Qnty, invd.Small_Qnty, invd.Qnty_Another, 0, Transaction_Type, invL.Invoice_Date, invd.Invoices_List_Id, invd.Invoices_Details_Id, Invoice_Type_Id, "›« Ê—… „»Ì⁄« ", invL.Customer_Name, invd.Store_Name, invL.Customer_Name, invL.Invoice_No);

                       ii.Set_ItemTransactionFromInvoice(ref opp, (Invoices_Details)invD_O.invD, invL_O, 0, Transaction_Type, Common.Invoice_Type_Id, "›« Ê—… „»Ì⁄« ");

                   }

                   Invoice_ds.Tables[0].Rows[x]["Item_ProcessedFlage"] = 100;
                   Invoice_ds.Tables[0].Rows[x]["Customer_Id"] = invL_O.invL.Customer_Id;
               }

               ////////// order sarf automatic
               if (Common.Make_Store_Order_Sarf_Estlam_Automatic_From_Invoice == 1 & invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
               {
                   for (int x = 0; x < mArrMake_Order.Count; x++)
                   {
                       Invoice_ds.Tables[0].Rows[x]["Make_Order"] = mArrMake_Order[x];
                   }
                   Order_Details_AutomaticCls oo = new Order_Details_AutomaticCls();
                   bool MakeOrderOk = false;
                   oo.Make_Order_From_Invoive_Details(ref Excp, ref opp, ref arr_Order_Purshing_Id_Automatic, ref MakeOrderOk, Invoice_ds.Tables[0], New_Invoice_Id, (long)invL_O.invL.Customer_Id, invL_O.invL.Invoice_Date.Value, "›« Ê—… „»Ì⁄« ");
                   if (MakeOrderOk)
                   {
                       Msg = Msg + "\n";
                       Msg = Msg + " „ ≈’œ«— ≈–‰ «·’—› ··⁄„Ì· „‰ «·„Œ“‰";
                   }

                   Get_arr_Order_Purshing_Id_Automatic_From_Invoice(New_Invoice_Id, ref opp);
                   
               }
               #region Comment
               ////make order Money
               //////if (Make_Invoice_Agile == 1)
               //////{
               //////    Accounts.Money_Transaction.Cash_Money.Money_Transaction_BoxCLS mt = new Accounts.Money_Transaction.Cash_Money.Money_Transaction_BoxCLS();

               //////    Excp = 0;

               //////    int Transaction_Type = 1;
               //////    long Box_Id = mt.GetStaticBank_ID();// int.Parse(ot.banks_List1.cmbBankType.SelectedValue.ToString());

               //////    decimal Box_Balance = 0;
               //////    string Order_No = mt.Make_New_Order_No(ref Excp, Transaction_Type).ToString();
               //////    mysql = "Select * from Banks Where Id=" + Box_Id;
               //////    DataTable dtBanks = opp.return_DataTable(mysql);
               //////    if (dtBanks.Rows.Count > 0)
               //////    {

               //////        Box_Balance = decimal.Parse(dtBanks.Rows[0]["Last_Balance"].ToString());
               //////        decimal Cach_Value = PaidValueFromMarket;// ot.nuValue.Value;
               //////        DateTime Order_Date = Invoice_Date;
               //////        string Man_Name = "";
               //////        int Reverse_Order = 0;
               //////        long Order_Mony_Id2 = 0;
               //////        mt.Make_Transaction_BanksBox(ref   Excp, ref   Msg, ref    opp, ref Order_Mony_Id2, Transaction_Type, Order_No, Box_Id, Box_Balance, Cach_Value, Order_Date, Man_Name, Currency_id, Customer_Id, Reverse_Order);
               //////        ////print Order
               //////        //Order_Receipt_Cash oo = new Order_Receipt_Cash();
               //////        //DataSet dsOrderMoney = oo.Get_Order_Cash(ref Excp, Order_Mony_Id2);
               //////        //Order_Receipt_CashFrm or = new Order_Receipt_CashFrm(Order_Mony_Id2, Transaction_Type, dsOrderMoney, 0);
               //////        //or.LoadDataOnForm();
               //////        //or.Set_Print(1);
               //////        //or = null;

               //////    }
               //////    else
               //////    {
               //////        Msg = Msg + "\n";
               //////        Msg = "·„ Ì „ ·≈’œ«— «·‰ﬁœÌ… —»„« ·⁄œ„ ÊÃÊœ ’‰œÊﬁ ≈› —«÷Ï ··›Ê« Ì—";
               //////    }
               //////}
               //////if (BtnOkPressMakeOrderMoney)
               //////{
               //////    Accounts.Money_Transaction.Cash_Money.Money_Transaction_BoxCLS mt = new Accounts.Money_Transaction.Cash_Money.Money_Transaction_BoxCLS();

               //////    Excp = 0;

               //////    int Transaction_Type = 1;
               //////    long Box_Id = int.Parse(ot.banks_List1.cmbBankType.SelectedValue.ToString());

               //////    decimal Box_Balance = 0;
               //////    string Order_No = mt.Make_New_Order_No(ref Excp, Transaction_Type).ToString();
               //////    mysql = "Select * from Banks Where Id=" + Box_Id;
               //////    DataTable dtBanks = opp.return_DataTable(mysql);
               //////    if (dtBanks.Rows.Count > 0)
               //////    {

               //////        Box_Balance = decimal.Parse(dtBanks.Rows[0]["Last_Balance"].ToString());
               //////        decimal Cach_Value = ot.nuValue.Value;
               //////        DateTime Order_Date = Invoice_Date;
               //////        string Man_Name = "";
               //////        int Reverse_Order = 0;
               //////        long Order_Mony_Id2 = 0;
               //////        mt.Make_Transaction_BanksBox(ref   Excp, ref   Msg, ref    opp, ref Order_Mony_Id2, Transaction_Type, Order_No, Box_Id, Box_Balance, Cach_Value, Order_Date, Man_Name, Currency_id, Customer_Id, Reverse_Order);
               //////        //print Order
               //////        Order_Receipt_Cash oo = new Order_Receipt_Cash();
               //////        DataSet dsOrderMoney = oo.Get_Order_Cash(ref Excp, Order_Mony_Id2);
               //////        Order_Receipt_CashFrm or = new Order_Receipt_CashFrm(Order_Mony_Id2, Transaction_Type, dsOrderMoney, 0);
               //////        or.LoadDataOnForm();
               //////        or.Set_Print(1);
               //////        or = null;

               //////    }
               //////    else
               //////    {
               //////        Msg = Msg + "\n";
               //////        Msg = "·„ Ì „ ·≈’œ«— «·‰ﬁœÌ… —»„« ·⁄œ„ ÊÃÊœ ’‰œÊﬁ ≈› —«÷Ï ··›Ê« Ì—";
               //////    }

               //////}
               ///

               #endregion

               if (Common.IsSetCustomers_AccountsForInvoice == 1)
               {
                   CustomersAccountCLS ca = new CustomersAccountCLS();
                   ca.Set_Customer_Balance_Invoice(ref Excp, ref opp, (long)invL_O.invL.Customer_Id, invL_O.Invoices_List_Id, invL_O.invL.Net_Total ?? 0, invL_O.invL.Notes);

                   //«‰‘«¡ «·ﬁÌœ
                   Account_Entry_Invoice_CLS aa = new Account_Entry_Invoice_CLS();
                   aa.IsNotDeletePrevEntry = true;
                   aa.Make_EntryForInvoices(ref Excp, ref Msg, ref opp, New_Invoice_Id, invL_O.invL.Invoice_Date.Value, invL_O.invL.Invoice_Date.Value, 0);
               }
               /////////////////// »—‰«„Ã «· ﬁ”Ìÿ

               if (Common.Is_Takseet_Invoice == 1 & mt.Is_Press_Ok == true)
               {
                   mt.MakeTakseet(ref opp, invL_O.invL.Customer_Id ?? 0, New_Invoice_Id, 0);

               }
               ///

               if (invL_O.invL.Is_Invoice_Agile == 0)
               {
                   if (Common.MakeOrderMoneyFromInvoiceAutomaticWithoutForm == 1)
                   {
                       long Box_Id = 0;
                       MakeMoney_TransactionFromInvoice(ref Excp, ref Msg, ref opp, invL_O.invL.Invoice_Date.Value, invL_O.invL.Invoice_Id ?? 0, invL_O.Invoices_List_Id, Box_Id, invL_O.invL.Currency_id ?? 0, invL_O.invL.Customer_Id ?? 0);
                   }
               }

               if (Excp == 1)
               {
                   New_Invoice_Id = 0;
                   invL_O.Invoices_List_Id = 0;
                   opp.Rollback();
               }
               else
               {
                   opp.Commit();
                   Excp = 0;
               }

           }
           catch (Exception ee)
           {
               opp.Rollback();
               Excp = 1;
           }
      */
        }


       
       
        public void Edit_InvoiceReverse_Update_CustomerAccount(ref int Excp, DataSet dss, InvoicesOperationListCLS invL_O)
        {
            /*
            // DateTime Invoice_Date, decimal InvoiveTotal, decimal NetInvoiceTotal, decimal Commision, decimal CommissionValue, string PolicaNo, string TroodContainerNo, int Make_Order, decimal Invoice_Net_Total_Old, decimal Invoice_Total_Old, decimal ATS05, decimal TaxSales10, decimal CustomerAccountAfter, string Notes, decimal AnotherPayment, long Salesman_Id, int IS_Make_Sarf_Item_BalanceFromInvoice, int Is_InvoiceKta3y
            long Invoice_List_Table_Id = invL_O.invL.Id;
            InvoiceDataSetCLS ids = new InvoiceDataSetCLS();
            DataSet New_Items_ds_Add = new DataSet();
            DataSet New_Items_ds_Minus = new DataSet();
            New_Items_ds_Add = ids.MakeInvoiceDataSet();
            New_Items_ds_Minus = ids.MakeInvoiceDataSet();
            ArrayList mysql_Arr = new ArrayList();
            operations opp = new operations(1);
            try
            {
                string mysql = "";
                //ArrayList arr = new ArrayList();



                //try
                //{
                //    invL.Customer_id = int.Parse(dss.Tables[0].Rows[0]["Customer_Id"].ToString());
                //    Customer_Name = Common.GetCustomerName(ref opp, Customer_id);
                //    //
                //    Invoice_No = int.Parse(dss.Tables[0].Rows[0]["Invoice_Id"].ToString());
                //}
                //catch { }
                /////////////////Set All Commision
                //decimal Total_Item_CommissionValue = 0;
                //for (int x = 0; x < dss.Tables[0].Rows.Count; x++)
                //{
                //    Total_Item_CommissionValue = Total_Item_CommissionValue + decimal.Parse(dss.Tables[0].Rows[x]["Item_CommissionValue"].ToString());
                //}
                //decimal Total_CommissionValue = Total_Item_CommissionValue + CommissionValue;
                //mysql = " UPDATE Invoices_List_Reverse SET  Edit_User_Id=" + Common.User_Id + ", EditOperationDate=" + operations.Get_DateTime_NowToInsert() + ", Total_Item_CommissionValue=" + Total_Item_CommissionValue + ",Total_CommissionValue=    " + Total_CommissionValue + " WHERE (id = " + New_Invoice_Id + ") ";
                //opp.insert_me(mysql);
                ///////////////////////////////////////////////////////////////////////////////


                decimal OLD_Larg_Qnty = 0;
                decimal OLD_Small_Qnty = 0;
                decimal OLD_Item_No = 0;
                InvoiceDataSetCLS ii = new InvoiceDataSetCLS();
                ///get the customer acount id
                //DataSet dsss = opp.return_dataset(" SELECT Invoices_List_Reverse.Net_Total, Customers_Accounts.* FROM Invoices_List_Reverse INNER JOIN  Customers_Accounts ON Invoices_List_Reverse.ID = Customers_Accounts.Reference_Type_Table_Id  Where Reference_Type=7 And Reference_Type_Table_Id=" + Invoice_List_Table_Id + "");
                //long Customers_Accounts_Id = 0;
                //Customers_Accounts_Id = long.Parse(dsss.Tables[0].Rows[0]["Id"].ToString());
                /////// check the editableitem
                for (int x = 0; x < dss.Tables[0].Rows.Count; x++)
                {
                    InvoicesOperationCLS invD_O = new InvoicesOperationCLS();
                    invD_O.invD.Id = invL_O.Invoices_List_Id;
                    invD_O.Get_Item_Details(dss, x);

                    string mysqlOld = "Select * from Invoices_Details_Reverse Where Id=" + invD_O.invD.Id + "";
                    DataSet dsN = opp.return_dataset(mysqlOld);
                    if (dsN.Tables[0].Rows.Count > 0)
                    {
                        OLD_Larg_Qnty = decimal.Parse(dsN.Tables[0].Rows[0]["Larg_Qnty"].ToString());
                        OLD_Small_Qnty = decimal.Parse(dsN.Tables[0].Rows[0]["Small_Qnty"].ToString());
                        OLD_Item_No = decimal.Parse(dsN.Tables[0].Rows[0]["Qnty_Another"].ToString());

                        //Item_No_Free, LargItemsQnty_Free, SmallItemsQnty_Free ,Item_Expire_Date
                        decimal OLD_LargItemsQnty_Free = decimal.Parse(dsN.Tables[0].Rows[0]["LargItemsQnty_Free"].ToString());
                        decimal OLD_SmallItemsQnty_Free = decimal.Parse(dsN.Tables[0].Rows[0]["SmallItemsQnty_Free"].ToString());
                        decimal OLD_Item_No_Free = decimal.Parse(dsN.Tables[0].Rows[0]["Item_No_Free"].ToString());

                        OLD_Larg_Qnty = OLD_Larg_Qnty + OLD_LargItemsQnty_Free;
                        OLD_Small_Qnty = OLD_Small_Qnty + OLD_SmallItemsQnty_Free;
                        OLD_Item_No = OLD_Item_No + OLD_Item_No_Free;
                    }



                    int Transaction_Type = 1;
                    int Invoice_Type_Id = Common.InvoiceReverse_Type_Id;
                    invL_O.Invoice_Type_Id = Common.InvoiceReverse_Type_Id;
                    if (dss.Tables[0].Rows[x]["Item_DeletedFlage"].ToString() == "1")
                    {
                        mysql = "Delete From Invoices_Details_Reverse  where id=" + invD_O.invD.Id;
                        opp.insert_me(mysql);

                        ii.Delete_ItemTransactionFromInvoices(ref opp, invD_O.invD.Unit_Item_Table_Id ?? 0, Invoice_List_Table_Id, invD_O.invD.Id, 0, invL_O.Invoice_Type_Id);

                    }
                    else if (dss.Tables[0].Rows[x]["Item_DeletedFlage"].ToString() == "-1")
                    {
                        invD_O.invD.Id = invD_O.Insert_Invoices_Details_Reverse(ref opp, ref Excp);
                        dss.Tables[0].Rows[x]["Invoices_Details_Id"] = invD_O.invD.Id.ToString();
                        if (Excp == 1)
                        {
                            opp.Rollback();
                            return;
                        }



                        if (Common.Project_Market == 1 & invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                        {
                            //int Transaction_Type = 2;
                            //int Invoice_Type_Id = 1;
                            try
                            {
                                invD_O.invD.Store_Name = Common.GetStoreName(ref opp, invD_O.invD.Store_Id ?? 0);
                            }
                            catch { }
                            //ii.Set_ItemTransactionFromInvoices(ref opp, invd.Unit_Item_Table_Id, invd.Item_Net_Total_Price, invd.Larg_Qnty, invd.Small_Qnty, invd.Qnty_Another, 0, Transaction_Type, invL.Invoice_Date, Invoice_List_Table_Id, invd.Invoices_Details_Id, invL.Invoice_Type_Id, "„— Ã⁄ „»Ì⁄« ", invd.Store_Name, invL.Customer_Name, invL.Customer_Name, invL.Invoice_No);
                            ii.Set_ItemTransactionFromInvoiceR(ref opp, invD_O, invL_O, 0, Transaction_Type, Common.InvoiceReverse_Type_Id, "›« Ê—… „— Ã⁄ „»Ì⁄« ");
                            //InvoiceDataSetCLS ii = new InvoiceDataSetCLS();
                        }
                    }
                    else if (dss.Tables[0].Rows[x]["Item_DeletedFlage"].ToString() == "0")
                    {
                        invD_O.Update_Invoices_Details_Reverse(ref opp, ref Excp);
                        if (Excp == 1)
                        {
                            opp.Rollback();
                            return;
                        }


                        if (Common.Project_Market == 1 & invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                        {
                            //int Transaction_Type = 2;
                            //int Invoice_Type_Id = 1;
                            try
                            {
                                invD_O.invD.Store_Name = Common.GetStoreName(ref opp, invD_O.invD.Store_Id ?? 0);
                            }
                            catch { }
                            //ii.Set_ItemTransactionFromInvoices(ref opp, invd.Unit_Item_Table_Id, invd.Item_Net_Total_Price, invd.Larg_Qnty, invd.Small_Qnty, invd.Qnty_Another, 0, Transaction_Type, invL.Invoice_Date, Invoice_List_Table_Id, invd.Invoices_Details_Id, invL.Invoice_Type_Id, "„— Ã⁄ „»Ì⁄« ", invd.Store_Name, invL.Customer_Name, invL.Customer_Name, invL.Invoice_No);
                            ii.Set_ItemTransactionFromInvoiceR(ref opp, invD_O, invL_O, 0, Transaction_Type, Common.InvoiceReverse_Type_Id, "›« Ê—… „— Ã⁄ „»Ì⁄« ");
                            //InvoiceDataSetCLS ii = new InvoiceDataSetCLS();
                        }
                    }

                    #region Form Project_Market=1
                    if (Common.Project_Market == 1 & invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                    {
                        if (dss.Tables[0].Rows[x]["Item_DeletedFlage"].ToString() == "1")
                        {

                            //Õ–›
                            mysql = " UPDATE Unit_item SET Full_qua =Full_qua - " + invD_O.invD.Larg_Qnty + ", D_Qua =D_Qua - " + invD_O.invD.Small_Qnty + ", Item_No =Item_No - " + invD_O.invD.Qnty_Another + " WHERE (id = " + invD_O.invD.Unit_Item_Table_Id + ") ";

                            opp.insert_me(mysql);

                            //«·»Ê‰’
                            mysql = " UPDATE Unit_item SET Full_qua =Full_qua - " + invD_O.invD.LargItemsQnty_Free + ", D_Qua =D_Qua - " + invD_O.invD.SmallItemsQnty_Free + ", Item_No =Item_No - " + invD_O.invD.Item_No_Free + " WHERE (id = " + invD_O.invD.Unit_Item_Table_Id + ") ";
                            opp.insert_me(mysql);

                            CommonAccounts.SetBalanceWithExpireDate_Tash3ela_Invoice(ref opp, 2, invD_O.invD, OLD_Larg_Qnty, OLD_Small_Qnty, OLD_Item_No);

                        }
                        else if (dss.Tables[0].Rows[x]["Item_DeletedFlage"].ToString() == "-1")
                        {//«÷«›…
                            mysql = " UPDATE Unit_item SET Full_qua =Full_qua + " + invD_O.invD.Larg_Qnty + ", D_Qua =D_Qua + " + invD_O.invD.Small_Qnty + ", Item_No =Item_No + " + invD_O.invD.Qnty_Another + " WHERE (id = " + invD_O.invD.Unit_Item_Table_Id + ") ";

                            opp.insert_me(mysql);

                            //«·»Ê‰’
                            mysql = " UPDATE Unit_item SET Full_qua =Full_qua + " + invD_O.invD.LargItemsQnty_Free + ", D_Qua =D_Qua + " + invD_O.invD.SmallItemsQnty_Free + ", Item_No =Item_No + " + invD_O.invD.Item_No_Free + " WHERE (id = " + invD_O.invD.Unit_Item_Table_Id + ") ";
                            opp.insert_me(mysql);

                            // Common.SetBalanceWithExpireDate_Tash3ela(ref opp, 1, Item_Id, Unit_Item_Table_Id, Item_Expire_Date, Tash3ela, Larg_Qnty, Small_Qnty, Qnty_Another, LargItemsQnty_Free, SmallItemsQnty_Free, Item_No_Free);
                            CommonAccounts.SetBalanceWithExpireDate_Tash3ela_Invoice(ref opp, 1, invD_O.invD, OLD_Larg_Qnty, OLD_Small_Qnty, OLD_Item_No);

                        }
                        else if (dss.Tables[0].Rows[x]["Item_DeletedFlage"].ToString() == "0")
                        {
                            //dsN = opp.return_dataset("Select * from   Invoices_Details_Reverse Where Id =" + Invoices_Details_Id + "");
                            //OLD_Larg_Qnty = decimal.Parse(dsN.Tables[0].Rows[0]["Larg_Qnty"].ToString());
                            //OLD_Small_Qnty = decimal.Parse(dsN.Tables[0].Rows[0]["Small_Qnty"].ToString());
                            //OLD_Item_No = decimal.Parse(dsN.Tables[0].Rows[0]["Qnty_Another"].ToString());

                            decimal Larg_Qnty = invD_O.invD.Larg_Qnty ?? 0 - OLD_Larg_Qnty;
                            decimal Small_Qnty = invD_O.invD.Small_Qnty ?? 0 - OLD_Small_Qnty;
                            decimal Qnty_Another = invD_O.invD.Qnty_Another ?? 0 - OLD_Item_No;

                            mysql = " UPDATE Unit_item SET Full_qua =Full_qua + " + Larg_Qnty + ", D_Qua =D_Qua + " + Small_Qnty + ", Item_No =Item_No + " + Qnty_Another + " WHERE (id = " + invD_O.invD.Unit_Item_Table_Id + ") ";
                            opp.insert_me(mysql);


                            // Common.SetBalanceWithExpireDate_Tash3ela(ref opp, 1, Item_Id, Unit_Item_Table_Id, Item_Expire_Date, Tash3ela, Larg_Qnty, Small_Qnty, Qnty_Another, LargItemsQnty_Free, SmallItemsQnty_Free, Item_No_Free);
                            CommonAccounts.SetBalanceWithExpireDate_Tash3ela_Invoice(ref opp, 0, invD_O.invD, Larg_Qnty, Small_Qnty, Qnty_Another);


                        }

                        //mysql = "  UPDATE Unit_item SET Full_qua = FLOOR((Small_unit * Full_qua + D_Qua) / Small_unit), ";
                        //mysql = mysql + " D_Qua =   (( Small_unit * Full_qua + D_Qua) / Small_unit - FLOOR(( Small_unit * Full_qua + D_Qua) /  Small_unit)) *  Small_unit  ";
                        //mysql = mysql + " WHERE (id = " + Unit_Item_Table_Id + ")  ";
                        mysql = "  UPDATE Unit_item SET Full_qua = FLOOR((Small_unit * Full_qua + D_Qua) / Small_unit), ";
                        mysql = mysql + " D_Qua =    (Small_unit * Full_qua + D_Qua) % Small_unit  ";
                        mysql = mysql + " WHERE (id = " + invD_O.invD.Unit_Item_Table_Id + ")  ";
                        opp.insert_me(mysql);

                    }

                    #endregion

                    #region order sarf automatic
                    if (Common.Make_Store_Order_Sarf_Estlam_Automatic_From_Invoice == 1 & invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                    {



                        if (dss.Tables[0].Rows[x]["Item_DeletedFlage"].ToString() == "1")
                        {
                            //Õ–›
                            Fill_Ds(ref New_Items_ds_Minus, dss, x);



                        }
                        else if (dss.Tables[0].Rows[x]["Item_DeletedFlage"].ToString() == "-1")
                        {//«÷«›…

                            Fill_Ds(ref New_Items_ds_Add, dss, x);
                        }
                        else
                        {

                            decimal Larg_Qnty = invD_O.invD.Larg_Qnty ?? 0 - OLD_Larg_Qnty;
                            decimal Small_Qnty = invD_O.invD.Small_Qnty ?? 0 - OLD_Small_Qnty;
                            decimal Qnty_Another = invD_O.invD.Qnty_Another ?? 0 - OLD_Item_No;

                            decimal Larg_Qnty__ = decimal.Parse(Larg_Qnty.ToString());
                            decimal Small_Qnty__ = decimal.Parse(Small_Qnty.ToString());
                            decimal Small_Unit_Price__ = decimal.Parse(invD_O.invD.Small_Unit_Price.ToString());
                            Cls_Conversion_Unit.ConVert_By_S_UnitValue(ref Larg_Qnty__, ref Small_Qnty__, Small_Unit_Price__);
                            int Add_ = 1;

                            if (Larg_Qnty__ < 0 | Small_Qnty__ < 0)
                            {
                                Add_ = 0;

                            }
                            else if (Larg_Qnty__ > 0 | Small_Qnty__ > 0)
                            {
                                Add_ = 1;

                            }
                            else
                            {
                                Add_ = -1;
                            }
                            if (Add_ == 1)
                            {
                                Fill_Ds(ref New_Items_ds_Add, dss, x, Larg_Qnty__, Small_Qnty__);
                            }
                            else if (Add_ == 0)
                            {
                                if (Larg_Qnty__ < 0)
                                {
                                    Larg_Qnty__ = Larg_Qnty__ * -1;
                                }

                                if (Small_Qnty__ < 0)
                                {
                                    Small_Qnty__ = Small_Qnty__ * -1;
                                }
                                Fill_Ds(ref New_Items_ds_Minus, dss, x, Larg_Qnty__, Small_Qnty__);
                            }

                        }
                    }




                    #endregion
                }

                //DateTime Invoice_Date = DateTime.Parse(dss.Tables[0].Rows[0]["Invoice_Date"].ToString());
                string Msg = "";


                if (Common.Make_Store_Order_Sarf_Estlam_Automatic_From_Invoice == 1 & invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                {
                    Order_Details_AutomaticCls oo = new Order_Details_AutomaticCls();
                    bool MakeOrderOk = false;
                    if (New_Items_ds_Add.Tables[0].Rows.Count > 0)
                    {
                        oo.Make_Order_From_Edit_Invoices_Details_Reverse(ref Excp, ref opp, ref mysql_Arr, New_Items_ds_Add.Tables[0], Invoice_List_Table_Id, (long)invL_O.invL.Customer_Id, invL_O.invL.Invoice_Date.Value, "›« Ê—… „— Ã⁄ „»Ì⁄« ");


                        if (Excp > 0)
                        {
                            opp.Rollback();
                            return;
                        }
                    }

                    if (MakeOrderOk)
                    {
                        Msg = Msg + "\n";
                        Msg = Msg + " „ ≈’œ«— ≈–‰ «·’—› ··⁄„Ì· „‰ «·„Œ“‰";
                    }

                    //////////////////„— Ã⁄ ’—›
                    if (New_Items_ds_Minus.Tables[0].Rows.Count > 0)
                    {

                        oo.Make_Order_From_EditInvoive_Details(ref Excp, ref mysql_Arr, ref opp, ref arr_Order_Purshing_Id_Automatic, ref MakeOrderOk, New_Items_ds_Minus.Tables[0], Invoice_List_Table_Id, (long)invL_O.invL.Customer_Id, invL_O.invL.Invoice_Date.Value, "›« Ê—… „— Ã⁄ „»Ì⁄« ");

                        if (Excp > 0)
                        {
                            opp.Rollback();
                            return;
                        }
                    }

                }

                ///to delete  the invoice
                int CountToDelete = 0;
                for (int x = 0; x < dss.Tables[0].Rows.Count; x++)
                {
                    if (dss.Tables[0].Rows[x]["Item_DeletedFlage"].ToString() == "1")
                    {
                        CountToDelete++;
                    }
                }
                if (CountToDelete == dss.Tables[0].Rows.Count)
                {
                    mysql = "Delete From Invoices_List_Reverse  where id=" + Invoice_List_Table_Id;
                    opp.insert_me(mysql);
                    //mysql = " Delete from Customers_Accounts  WHERE (id = " + Customers_Accounts_Id + ")";
                    //opp.insert_me(mysql);
                }
                invL_O.Update_Invoices_List_Reverse(ref opp, ref Excp);
                if (Excp == 1)
                {
                    opp.Rollback();
                    return;
                }
                ///////// check customer balance
                decimal Invoice_NetTotal_Diff = invL_O.Invoice_Net_Total_Old - invL_O.invL.Net_Total ?? 0;


                Invoice_NetTotal_Diff = Invoice_NetTotal_Diff;



                CustomersAccountCLS ca = new CustomersAccountCLS();
                ca.ProcessNo = invL_O.invL.ProcessNo ?? 0;
                //ca.Updatet_Customer_BalanceWithEditInvoiceReverse(ref Excp, ref opp, invL.Invoice_Date, (int)invL.Customer_Id, Invoice_List_Table_Id, invL.Net_Total_Price, Invoice_NetTotal_Diff, invL.Notes);
                ca.Set_Customer_BalanceInvoiceReverse(ref Excp, ref opp, (long)invL_O.invL.Customer_Id, invL_O.Invoices_List_Id, invL_O.invL.Net_Total ?? 0, invL_O.invL.Notes);
                //«‰‘«¡ «·ﬁÌœ
                Account_Entry_Invoice_CLS aa = new Account_Entry_Invoice_CLS();
                aa.IsNotDeletePrevEntry = false;
                aa.Make_EntryForInvoices_Reverse(ref Excp, ref Msg, ref opp, invL_O.Invoices_List_Id, invL_O.invL.Invoice_Date.Value, invL_O.invL.Invoice_Date.Value, -1);


                //opp.insert_me_transaction(ref Excp, 1, arr);
                //opp.insert_me_transaction(ref Excp, arr);
                opp.Commit();

                Excp = 0;

            }
            catch (Exception e)
            {
                Excp = 1;
                opp.Rollback();
               
                //throw (e);
            }
            if (Excp == 0)
            {
                string mysql = "";

                if (mysql_Arr.Count > 0)
                {
                    operations opp2 = new operations(1);
                    try
                    {
                        mysql = "";
                        for (int y = 0; y < mysql_Arr.Count; y++)
                        {
                            mysql = mysql + " " + mysql_Arr[y].ToString();
                            //opp2.insert_me(mysql_Arr[y].ToString());
                        }
                        opp2.insert_me(mysql);
                        opp2.Commit();
                    }
                    catch (Exception ee) { opp2.Rollback(); }
                }

            }

            */
        }

        public void Edit_InvoiceReverse_Update_CustomerAccount00(ref int Excp,ref string Msg, InvoicesOperationListCLS invL_O, InvoicesOperationCLS invD_O)
        {
            // DateTime Invoice_Date, decimal InvoiveTotal, decimal NetInvoiceTotal, decimal Commision, decimal CommissionValue, string PolicaNo, string TroodContainerNo, int Make_Order, decimal Invoice_Net_Total_Old, decimal Invoice_Total_Old, decimal ATS05, decimal TaxSales10, decimal CustomerAccountAfter, string Notes, decimal AnotherPayment, long Salesman_Id, int IS_Make_Sarf_Item_BalanceFromInvoice, int Is_InvoiceKta3y
            long Invoice_List_Table_Id = invL_O.invL.Id;
            InvoiceDataSetCLS ids = new InvoiceDataSetCLS();
            DataSet New_Items_ds_Add = new DataSet();
            DataSet New_Items_ds_Minus = new DataSet();
            New_Items_ds_Add = ids.MakeInvoiceDataSet();
            New_Items_ds_Minus = ids.MakeInvoiceDataSet();
            ArrayList mysql_Arr = new ArrayList();
            operations opp = new operations(1);
            try
            {
                string mysql = "";
                //ArrayList arr = new ArrayList();

                Invoices_List invL_ROld = opp.db.Invoices_List.AsNoTracking().Where(i => i.Id == Invoice_List_Table_Id).FirstOrDefault();


                //try
                //{
                //    invL.Customer_id = int.Parse(dss.Tables[0].Rows[0]["Customer_Id"].ToString());
                //    Customer_Name = Common.GetCustomerName(ref opp, Customer_id);
                //    //
                //    Invoice_No = int.Parse(dss.Tables[0].Rows[0]["Invoice_Id"].ToString());
                //}
                //catch { }
                /////////////////Set All Commision
                //decimal Total_Item_CommissionValue = 0;
                //for (int x = 0; x < dss.Tables[0].Rows.Count; x++)
                //{
                //    Total_Item_CommissionValue = Total_Item_CommissionValue + decimal.Parse(dss.Tables[0].Rows[x]["Item_CommissionValue"].ToString());
                //}
                //decimal Total_CommissionValue = Total_Item_CommissionValue + CommissionValue;
                //mysql = " UPDATE Invoices_List_Reverse SET  Edit_User_Id=" + Common.User_Id + ", EditOperationDate=" + operations.Get_DateTime_NowToInsert() + ", Total_Item_CommissionValue=" + Total_Item_CommissionValue + ",Total_CommissionValue=    " + Total_CommissionValue + " WHERE (id = " + New_Invoice_Id + ") ";
                //opp.insert_me(mysql);
                ///////////////////////////////////////////////////////////////////////////////


                decimal OLD_Larg_Qnty = 0;
                decimal OLD_Small_Qnty = 0;
                decimal OLD_Item_No = 0;
                InvoiceDataSetCLS ii = new InvoiceDataSetCLS();
                ///get the customer acount id
                //DataSet dsss = opp.return_dataset(" SELECT Invoices_List_Reverse.Net_Total, Customers_Accounts.* FROM Invoices_List_Reverse INNER JOIN  Customers_Accounts ON Invoices_List_Reverse.ID = Customers_Accounts.Reference_Type_Table_Id  Where Reference_Type=7 And Reference_Type_Table_Id=" + Invoice_List_Table_Id + "");
                //long Customers_Accounts_Id = 0;
                //Customers_Accounts_Id = long.Parse(dsss.Tables[0].Rows[0]["Id"].ToString());
                /////// check the editableitem
                for (int x = 0; x < invD_O.invDlst.Count; x++)
                {
                    InvoicesOperationCLS invd_O = new InvoicesOperationCLS();
                    //invd_O.invD.Id = invL_O.Invoices_List_Id;
                    //invd_O.Get_Item_Details(dss, x);
                    invd_O.invD = invD_O.invDlst[x];
                    string mysqlOld = "Select * from Invoices_Details_Reverse Where Id=" + invD_O.invDlst[x].Id + "";
                    DataSet dsN = opp.return_dataset(mysqlOld);
                    if (dsN.Tables[0].Rows.Count > 0)
                    {
                        OLD_Larg_Qnty = decimal.Parse(dsN.Tables[0].Rows[0]["Larg_Qnty"].ToString());
                        OLD_Small_Qnty = decimal.Parse(dsN.Tables[0].Rows[0]["Small_Qnty"].ToString());
                        OLD_Item_No = decimal.Parse(dsN.Tables[0].Rows[0]["Qnty_Another"].ToString());

                        //Item_No_Free, LargItemsQnty_Free, SmallItemsQnty_Free ,Item_Expire_Date
                        decimal OLD_LargItemsQnty_Free = decimal.Parse(dsN.Tables[0].Rows[0]["LargItemsQnty_Free"].ToString());
                        decimal OLD_SmallItemsQnty_Free = decimal.Parse(dsN.Tables[0].Rows[0]["SmallItemsQnty_Free"].ToString());
                        decimal OLD_Item_No_Free = decimal.Parse(dsN.Tables[0].Rows[0]["Item_No_Free"].ToString());

                        OLD_Larg_Qnty = OLD_Larg_Qnty + OLD_LargItemsQnty_Free;
                        OLD_Small_Qnty = OLD_Small_Qnty + OLD_SmallItemsQnty_Free;
                        OLD_Item_No = OLD_Item_No + OLD_Item_No_Free;
                    }



                    int Transaction_Type = 1;
                    int Invoice_Type_Id = Common.InvoiceReverse_Type_Id;
                    invL_O.Invoice_Type_Id = Common.InvoiceReverse_Type_Id;
                    //if (invD_O.invDlst[x].Item_DeletedFlage.ToString() == "1")
                    //{
                    //    mysql = "Delete From Invoices_Details_Reverse  where id=" + invd_O.invD.Id;
                    //    opp.insert_me(mysql);

                    //    ii.Delete_ItemTransactionFromInvoices(ref opp, invd_O.invD.Unit_Item_Table_Id ?? 0, Invoice_List_Table_Id, invd_O.invD.Id, 0, invL_O.Invoice_Type_Id);

                    //}
                    if (invD_O.invDlst[x].Item_DeletedFlage.ToString() == "0")
                    {
                        invD_O.invDlst[x].Invoice_Id = invL_O.invL.Id;
                        invd_O.invD.Id = invd_O.Insert_Invoices_Details_Reverse(ref Excp, ref opp, (Invoices_Details_Reverse)invD_O.invDlst[x]);
                        //dss.Tables[0].Rows[x]["Invoices_Details_Id"] = invd_O.invD.Id.ToString();
                        if (Excp == 1)
                        {
                            opp.Rollback();
                            return;
                        }



                        if (Common.Project_Market == 1 & invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                        {
                            //int Transaction_Type = 2;
                            //int Invoice_Type_Id = 1;
                            try
                            {
                                invd_O.invD.Store_Name = Common.GetStoreName(ref opp, invd_O.invD.Store_Id ?? 0);
                            }
                            catch { }
                            //ii.Set_ItemTransactionFromInvoices(ref opp, invd.Unit_Item_Table_Id, invd.Item_Net_Total_Price, invd.Larg_Qnty, invd.Small_Qnty, invd.Qnty_Another, 0, Transaction_Type, invL.Invoice_Date, Invoice_List_Table_Id, invd.Invoices_Details_Id, invL.Invoice_Type_Id, "„— Ã⁄ „»Ì⁄« ", invd.Store_Name, invL.Customer_Name, invL.Customer_Name, invL.Invoice_No);
                            ii.Set_ItemTransactionFromInvoiceR(ref opp, invD_O.invDlst[x], invL_O, 0, Transaction_Type, Common.InvoiceReverse_Type_Id, "›« Ê—… „— Ã⁄ „»Ì⁄« ");
                            //InvoiceDataSetCLS ii = new InvoiceDataSetCLS();
                        }
                    }
                    else if (invD_O.invDlst[x].Item_DeletedFlage.ToString() == "-1")
                    {
                        invd_O.Update_Invoices_Details_Reverse(ref Excp, ref opp, (Invoices_Details_Reverse)invD_O.invDlst[x]);
                        if (Excp == 1)
                        {
                            opp.Rollback();
                            return;
                        }


                        if (Common.Project_Market == 1 & invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                        {
                            //int Transaction_Type = 2;
                            //int Invoice_Type_Id = 1;
                            try
                            {
                                invd_O.invD.Store_Name = Common.GetStoreName(ref opp, invd_O.invD.Store_Id ?? 0);
                            }
                            catch { }
                            //ii.Set_ItemTransactionFromInvoices(ref opp, invd.Unit_Item_Table_Id, invd.Item_Net_Total_Price, invd.Larg_Qnty, invd.Small_Qnty, invd.Qnty_Another, 0, Transaction_Type, invL.Invoice_Date, Invoice_List_Table_Id, invd.Invoices_Details_Id, invL.Invoice_Type_Id, "„— Ã⁄ „»Ì⁄« ", invd.Store_Name, invL.Customer_Name, invL.Customer_Name, invL.Invoice_No);
                            ii.Set_ItemTransactionFromInvoiceR(ref opp, invD_O.invDlst[x], invL_O, 0, Transaction_Type, Common.InvoiceReverse_Type_Id, "›« Ê—… „— Ã⁄ „»Ì⁄« ");
                            //InvoiceDataSetCLS ii = new InvoiceDataSetCLS();
                        }
                    }

                    #region Form Project_Market=1
                    if (Common.Project_Market == 1 & invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                    {
                        //if (invD_O.invDlst[x].Item_DeletedFlage.ToString() == "1")
                        //{

                        //    //Õ–›
                        //    mysql = " UPDATE Unit_item SET Full_qua =Full_qua - " + invd_O.invD.Larg_Qnty + ", D_Qua =D_Qua - " + invd_O.invD.Small_Qnty + ", Item_No =Item_No - " + invd_O.invD.Qnty_Another + " WHERE (id = " + invd_O.invD.Unit_Item_Table_Id + ") ";

                        //    opp.insert_me(mysql);

                        //    //«·»Ê‰’
                        //    mysql = " UPDATE Unit_item SET Full_qua =Full_qua - " + invd_O.invD.LargItemsQnty_Free + ", D_Qua =D_Qua - " + invd_O.invD.SmallItemsQnty_Free + ", Item_No =Item_No - " + invd_O.invD.Item_No_Free + " WHERE (id = " + invd_O.invD.Unit_Item_Table_Id + ") ";
                        //    opp.insert_me(mysql);

                        //    CommonAccounts.SetBalanceWithExpireDate_Tash3ela_Invoice(ref opp, 2, invD_O.invDlst[x], OLD_Larg_Qnty, OLD_Small_Qnty, OLD_Item_No);

                        //}
                        if (invD_O.invDlst[x].Item_DeletedFlage.ToString() == "-1")
                        {//«÷«›…
                            mysql = " UPDATE Unit_item SET Full_qua =Full_qua + " + invd_O.invD.Larg_Qnty + ", D_Qua =D_Qua + " + invd_O.invD.Small_Qnty + ", Item_No =Item_No + " + invd_O.invD.Qnty_Another + " WHERE (id = " + invd_O.invD.Unit_Item_Table_Id + ") ";

                            opp.insert_me(mysql);

                            //«·»Ê‰’
                            mysql = " UPDATE Unit_item SET Full_qua =Full_qua + " + invd_O.invD.LargItemsQnty_Free + ", D_Qua =D_Qua + " + invd_O.invD.SmallItemsQnty_Free + ", Item_No =Item_No + " + invd_O.invD.Item_No_Free + " WHERE (id = " + invd_O.invD.Unit_Item_Table_Id + ") ";
                            opp.insert_me(mysql);

                            // Common.SetBalanceWithExpireDate_Tash3ela(ref opp, 1, Item_Id, Unit_Item_Table_Id, Item_Expire_Date, Tash3ela, Larg_Qnty, Small_Qnty, Qnty_Another, LargItemsQnty_Free, SmallItemsQnty_Free, Item_No_Free);
                            CommonAccounts.SetBalanceWithExpireDate_Tash3ela_Invoice(ref opp, 1, invd_O.invD, OLD_Larg_Qnty, OLD_Small_Qnty, OLD_Item_No);

                        }
                        else if (invD_O.invDlst[x].Item_DeletedFlage.ToString() == "0")
                        {
                            //dsN = opp.return_dataset("Select * from   Invoices_Details_Reverse Where Id =" + Invoices_Details_Id + "");
                            //OLD_Larg_Qnty = decimal.Parse(dsN.Tables[0].Rows[0]["Larg_Qnty"].ToString());
                            //OLD_Small_Qnty = decimal.Parse(dsN.Tables[0].Rows[0]["Small_Qnty"].ToString());
                            //OLD_Item_No = decimal.Parse(dsN.Tables[0].Rows[0]["Qnty_Another"].ToString());

                            decimal Larg_Qnty = invd_O.invD.Larg_Qnty ?? 0 - OLD_Larg_Qnty;
                            decimal Small_Qnty = invd_O.invD.Small_Qnty ?? 0 - OLD_Small_Qnty;
                            decimal Qnty_Another = invd_O.invD.Qnty_Another ?? 0 - OLD_Item_No;

                            mysql = " UPDATE Unit_item SET Full_qua =Full_qua + " + Larg_Qnty + ", D_Qua =D_Qua + " + Small_Qnty + ", Item_No =Item_No + " + Qnty_Another + " WHERE (id = " + invd_O.invD.Unit_Item_Table_Id + ") ";
                            opp.insert_me(mysql);


                            // Common.SetBalanceWithExpireDate_Tash3ela(ref opp, 1, Item_Id, Unit_Item_Table_Id, Item_Expire_Date, Tash3ela, Larg_Qnty, Small_Qnty, Qnty_Another, LargItemsQnty_Free, SmallItemsQnty_Free, Item_No_Free);
                            CommonAccounts.SetBalanceWithExpireDate_Tash3ela_Invoice(ref opp, 0, invD_O.invDlst[x], Larg_Qnty, Small_Qnty, Qnty_Another);


                        }

                        //mysql = "  UPDATE Unit_item SET Full_qua = FLOOR((Small_unit * Full_qua + D_Qua) / Small_unit), ";
                        //mysql = mysql + " D_Qua =   (( Small_unit * Full_qua + D_Qua) / Small_unit - FLOOR(( Small_unit * Full_qua + D_Qua) /  Small_unit)) *  Small_unit  ";
                        //mysql = mysql + " WHERE (id = " + Unit_Item_Table_Id + ")  ";
                        mysql = "  UPDATE Unit_item SET Full_qua = FLOOR((Small_unit * Full_qua + D_Qua) / Small_unit), ";
                        mysql = mysql + " D_Qua =    (Small_unit * Full_qua + D_Qua) % Small_unit  ";
                        mysql = mysql + " WHERE (id = " + invd_O.invD.Unit_Item_Table_Id + ")  ";
                        opp.insert_me(mysql);

                    }

                    #endregion

                    #region order sarf automatic
                    //if (Common.Make_Store_Order_Sarf_Estlam_Automatic_From_Invoice == 1 & invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                    //{

                    //    /*

                    //    if (dss.Tables[0].Rows[x]["Item_DeletedFlage"].ToString() == "1")
                    //    {
                    //        //Õ–›
                    //        Fill_Ds(ref New_Items_ds_Minus, dss, x);



                    //    }
                    //    else if (dss.Tables[0].Rows[x]["Item_DeletedFlage"].ToString() == "-1")
                    //    {//«÷«›…

                    //        Fill_Ds(ref New_Items_ds_Add, dss, x);
                    //    }
                    //    else
                    //    {

                    //        decimal Larg_Qnty = invd_O.invD.Larg_Qnty ?? 0 - OLD_Larg_Qnty;
                    //        decimal Small_Qnty = invd_O.invD.Small_Qnty ?? 0 - OLD_Small_Qnty;
                    //        decimal Qnty_Another = invd_O.invD.Qnty_Another ?? 0 - OLD_Item_No;

                    //        decimal Larg_Qnty__ = decimal.Parse(Larg_Qnty.ToString());
                    //        decimal Small_Qnty__ = decimal.Parse(Small_Qnty.ToString());
                    //        decimal Small_Unit_Price__ = decimal.Parse(invd_O.invD.Small_Unit_Price.ToString());
                    //        Cls_Conversion_Unit.ConVert_By_S_UnitValue(ref Larg_Qnty__, ref Small_Qnty__, Small_Unit_Price__);
                    //        int Add_ = 1;

                    //        if (Larg_Qnty__ < 0 | Small_Qnty__ < 0)
                    //        {
                    //            Add_ = 0;

                    //        }
                    //        else if (Larg_Qnty__ > 0 | Small_Qnty__ > 0)
                    //        {
                    //            Add_ = 1;

                    //        }
                    //        else
                    //        {
                    //            Add_ = -1;
                    //        }
                    //        if (Add_ == 1)
                    //        {
                    //            Fill_Ds(ref New_Items_ds_Add, dss, x, Larg_Qnty__, Small_Qnty__);
                    //        }
                    //        else if (Add_ == 0)
                    //        {
                    //            if (Larg_Qnty__ < 0)
                    //            {
                    //                Larg_Qnty__ = Larg_Qnty__ * -1;
                    //            }

                    //            if (Small_Qnty__ < 0)
                    //            {
                    //                Small_Qnty__ = Small_Qnty__ * -1;
                    //            }
                    //            Fill_Ds(ref New_Items_ds_Minus, dss, x, Larg_Qnty__, Small_Qnty__);
                    //        }

                    //    }
                    //    */
                    //}




                    #endregion
                }

                //////// «·Õ–›
                ///
                for (int x = 0; x < invD_O.invDlst_Deleted.Count; x++)
                {
                    InvoicesOperationCLS invd_O = new InvoicesOperationCLS();
                    invd_O.invD.Id = invL_O.Invoices_List_Id;
                    //invd_O.Get_Item_Details(dss, x);
                    invd_O.invD = invD_O.invDlst_Deleted[x];


                    int Transaction_Type = 1;
                    int Invoice_Type_Id = Common.InvoiceReverse_Type_Id;
                    invL_O.Invoice_Type_Id = Common.InvoiceReverse_Type_Id;
                    if (invD_O.invDlst_Deleted[x].Item_DeletedFlage.ToString() == "1")
                    {
                        mysql = "Delete From Invoices_Details_Reverse  where id=" + invd_O.invD.Id;
                        opp.insert_me(mysql);

                        ii.Delete_ItemTransactionFromInvoices(ref opp, invd_O.invD.Unit_Item_Table_Id ?? 0, Invoice_List_Table_Id, invd_O.invD.Id, 0, invL_O.Invoice_Type_Id);

                    }


                    #region Form Project_Market=1
                    if (Common.Project_Market == 1 & invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                    {
                        if (invD_O.invDlst_Deleted[x].Item_DeletedFlage.ToString() == "1")
                        {

                            //Õ–›
                            mysql = " UPDATE Unit_item SET Full_qua =Full_qua - " + invD_O.invDlst_Deleted[x].Larg_Qnty + ", D_Qua =D_Qua - " + invD_O.invDlst_Deleted[x].Small_Qnty + ", Item_No =Item_No - " + invD_O.invDlst_Deleted[x].Qnty_Another + " WHERE (id = " + invD_O.invDlst_Deleted[x].Unit_Item_Table_Id + ") ";

                            opp.insert_me(mysql);

                            //«·»Ê‰’
                            mysql = " UPDATE Unit_item SET Full_qua =Full_qua - " + invD_O.invDlst_Deleted[x].LargItemsQnty_Free + ", D_Qua =D_Qua - " + invd_O.invD.SmallItemsQnty_Free + ", Item_No =Item_No - " + invd_O.invD.Item_No_Free + " WHERE (id = " + invD_O.invDlst_Deleted[x].Unit_Item_Table_Id + ") ";
                            opp.insert_me(mysql);

                            CommonAccounts.SetBalanceWithExpireDate_Tash3ela_Invoice(ref opp, 2, invD_O.invDlst_Deleted[x], OLD_Larg_Qnty, OLD_Small_Qnty, OLD_Item_No);

                        }



                        mysql = "  UPDATE Unit_item SET Full_qua = FLOOR((Small_unit * Full_qua + D_Qua) / Small_unit), ";
                        mysql = mysql + " D_Qua =    (Small_unit * Full_qua + D_Qua) % Small_unit  ";
                        mysql = mysql + " WHERE (id = " + invD_O.invDlst_Deleted[x].Unit_Item_Table_Id + ")  ";
                        opp.insert_me(mysql);

                    }

                    #endregion

                    #region order sarf automatic
                    //if (Common.Make_Store_Order_Sarf_Estlam_Automatic_From_Invoice == 1 & invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                    //{

                    //    /*

                    //    if (dss.Tables[0].Rows[x]["Item_DeletedFlage"].ToString() == "1")
                    //    {
                    //        //Õ–›
                    //        Fill_Ds(ref New_Items_ds_Minus, dss, x);



                    //    }
                    //    else if (dss.Tables[0].Rows[x]["Item_DeletedFlage"].ToString() == "-1")
                    //    {//«÷«›…

                    //        Fill_Ds(ref New_Items_ds_Add, dss, x);
                    //    }
                    //    else
                    //    {

                    //        decimal Larg_Qnty = invd_O.invD.Larg_Qnty ?? 0 - OLD_Larg_Qnty;
                    //        decimal Small_Qnty = invd_O.invD.Small_Qnty ?? 0 - OLD_Small_Qnty;
                    //        decimal Qnty_Another = invd_O.invD.Qnty_Another ?? 0 - OLD_Item_No;

                    //        decimal Larg_Qnty__ = decimal.Parse(Larg_Qnty.ToString());
                    //        decimal Small_Qnty__ = decimal.Parse(Small_Qnty.ToString());
                    //        decimal Small_Unit_Price__ = decimal.Parse(invd_O.invD.Small_Unit_Price.ToString());
                    //        Cls_Conversion_Unit.ConVert_By_S_UnitValue(ref Larg_Qnty__, ref Small_Qnty__, Small_Unit_Price__);
                    //        int Add_ = 1;

                    //        if (Larg_Qnty__ < 0 | Small_Qnty__ < 0)
                    //        {
                    //            Add_ = 0;

                    //        }
                    //        else if (Larg_Qnty__ > 0 | Small_Qnty__ > 0)
                    //        {
                    //            Add_ = 1;

                    //        }
                    //        else
                    //        {
                    //            Add_ = -1;
                    //        }
                    //        if (Add_ == 1)
                    //        {
                    //            Fill_Ds(ref New_Items_ds_Add, dss, x, Larg_Qnty__, Small_Qnty__);
                    //        }
                    //        else if (Add_ == 0)
                    //        {
                    //            if (Larg_Qnty__ < 0)
                    //            {
                    //                Larg_Qnty__ = Larg_Qnty__ * -1;
                    //            }

                    //            if (Small_Qnty__ < 0)
                    //            {
                    //                Small_Qnty__ = Small_Qnty__ * -1;
                    //            }
                    //            Fill_Ds(ref New_Items_ds_Minus, dss, x, Larg_Qnty__, Small_Qnty__);
                    //        }

                    //    }
                    //    */
                    //}




                    #endregion
                }
                //DateTime Invoice_Date = DateTime.Parse(dss.Tables[0].Rows[0]["Invoice_Date"].ToString());
              


                if (Common.Make_Store_Order_Sarf_Estlam_Automatic_From_Invoice == 1 & invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                {
                    Order_Details_AutomaticCls oo = new Order_Details_AutomaticCls();
                    bool MakeOrderOk = false;
                    if (New_Items_ds_Add.Tables[0].Rows.Count > 0)
                    {
                        oo.Make_Order_From_Edit_Invoices_Details_Reverse(ref Excp, ref opp, ref mysql_Arr, New_Items_ds_Add.Tables[0], Invoice_List_Table_Id, (long)invL_O.invL.Customer_Id, invL_O.invL.Invoice_Date.Value, "›« Ê—… „— Ã⁄ „»Ì⁄« ");


                        if (Excp > 0)
                        {
                            opp.Rollback();
                            return;
                        }
                    }

                    if (MakeOrderOk)
                    {
                        Msg = Msg + "\n";
                        Msg = Msg + " „ ≈’œ«— ≈–‰ «·’—› ··⁄„Ì· „‰ «·„Œ“‰";
                    }

                    //////////////////„— Ã⁄ ’—›
                    if (New_Items_ds_Minus.Tables[0].Rows.Count > 0)
                    {

                        oo.Make_Order_From_EditInvoive_Details(ref Excp, ref mysql_Arr, ref opp, ref arr_Order_Purshing_Id_Automatic, ref MakeOrderOk, New_Items_ds_Minus.Tables[0], Invoice_List_Table_Id, (long)invL_O.invL.Customer_Id, invL_O.invL.Invoice_Date.Value, "›« Ê—… „— Ã⁄ „»Ì⁄« ");

                        if (Excp > 0)
                        {
                            opp.Rollback();
                            return;
                        }
                    }

                }

                ///to delete  the invoice
                int CountToDelete = 0;
                for (int x = 0; x < invD_O.invDlst.Count; x++)
                {
                    if (invD_O.invDlst[x].Item_DeletedFlage.ToString() == "1")
                    {
                        CountToDelete++;
                    }
                }
                if (CountToDelete == invD_O.invDlst.Count)
                {
                    mysql = "Delete From Invoices_List_Reverse  where id=" + Invoice_List_Table_Id;
                    opp.insert_me(mysql);
                    //mysql = " Delete from Customers_Accounts  WHERE (id = " + Customers_Accounts_Id + ")";
                    //opp.insert_me(mysql);
                }
                else
                {
                    invL_O.Update_Invoices_List_Reverse(ref Excp, ref opp, (Invoices_List_Reverse)invL_O.invL);
                }
                if (Excp == 1)
                {
                    opp.Rollback();
                    return;
                }
                ///////// check customer balance
                decimal Invoice_NetTotal_Diff = invL_ROld.Net_Total - invL_O.invL.Net_Total ?? 0;


                Invoice_NetTotal_Diff = Invoice_NetTotal_Diff;



                CustomersAccountCLS ca = new CustomersAccountCLS();
                ca.ProcessNo = invL_O.invL.ProcessNo ?? 0;
                //ca.Updatet_Customer_BalanceWithEditInvoiceReverse(ref Excp, ref opp, invL.Invoice_Date, (int)invL.Customer_Id, Invoice_List_Table_Id, invL.Net_Total_Price, Invoice_NetTotal_Diff, invL.Notes);
                ca.Set_Customer_BalanceInvoiceReverse(ref Excp, ref opp, (long)invL_O.invL.Customer_Id, invL_O.Invoices_List_Id, invL_O.invL.Net_Total ?? 0, invL_O.invL.Notes);
                //«‰‘«¡ «·ﬁÌœ
                Account_Entry_Invoice_CLS aa = new Account_Entry_Invoice_CLS();
                aa.IsNotDeletePrevEntry = false;
                aa.Make_EntryForInvoices_Reverse(ref Excp, ref Msg, ref opp, invL_O.Invoices_List_Id, invL_O.invL.Invoice_Date.Value, invL_O.invL.Invoice_Date.Value, -1);


                //opp.insert_me_transaction(ref Excp, 1, arr);
                //opp.insert_me_transaction(ref Excp, arr);
                opp.Commit();

                Excp = 0;

            }
            catch (Exception e)
            {
                Excp = 1;
                opp.Rollback();
                Msg = e.Message;
                //throw (e);
            }
            if (Excp == 0)
            {
                string mysql = "";

                if (mysql_Arr.Count > 0)
                {
                    operations opp2 = new operations(1);
                    try
                    {
                        mysql = "";
                        for (int y = 0; y < mysql_Arr.Count; y++)
                        {
                            mysql = mysql + " " + mysql_Arr[y].ToString();
                            //opp2.insert_me(mysql_Arr[y].ToString());
                        }
                        opp2.insert_me(mysql);
                        opp2.Commit();
                    }
                    catch (Exception ee) { opp2.Rollback(); }
                }

            }
        }


         
        
        
        /*
          public void Get_arr_Order_Purshing_Id_Automatic_From_Invoice(long Invoice_Id, ref operations opp)
          {
              arr_Order_Purshing_Id_Automatic = new ArrayList();

              string mysql = "SELECT Invoices_Details.Order_Purshing_Id_Automatic FROM Invoices_Details WHERE (Invoice_Id = " + Invoice_Id + ")";
              DataTable dt = opp.return_DataTable(mysql);
              for (int x = 0; x < dt.Rows.Count; x++)
              {
                  arr_Order_Purshing_Id_Automatic.Add(dt.Rows[x]["Order_Purshing_Id_Automatic"].ToString());
              }
          }
          */

        //void MakeMoney_TransactionFromInvoice(ref int Excp, ref string Msg, ref operations opp, DateTime Invoice_Date, long Invoice_No, long Invoice_Id, int Box_Id, long Currency_Id, long Customer_Id)
        //{

        //    ////make order Money

        //    Money_Transaction_BoxOrderReportCLS mt = new Money_Transaction_BoxOrderReportCLS();

        //    Excp = 0;

        //    int Transaction_Type = 1;
        //    if (Box_Id == 0)
        //    {
        //        Box_Id = mt.GetDefaultBank_ID();// int.Parse(ot.banks_List1.cmbBankType.SelectedValue.ToString());
        //    }
        //    decimal Box_Balance = 0;
        //    string Order_No = mt.Make_New_Order_No(ref Excp, Transaction_Type).ToString();
        //    string mysql = "Select * from Banks Where Id=" + Box_Id;
        //    DataTable dtBanks = opp.return_DataTable(mysql);
        //    if (dtBanks.Rows.Count > 0)
        //    {

        //        Box_Balance = decimal.Parse(dtBanks.Rows[0]["Last_Balance"].ToString());
        //        decimal Cach_Value = PaidValueFromMarket;// ot.nuValue.Value;
        //        DateTime Order_Date = Invoice_Date;
        //        string Man_Name = " ›« Ê—… „»Ì⁄«  —ﬁ„ " + Invoice_No;
        //        int Reverse_Order = 0;
        //        long Order_Mony_Id2 = 0;
        //        mt.Invoice_ID = Invoice_Id;

        //        //, long CarsWorkOrdersId, long Reference_Type_Table_Id, long DataDictionaryProcessesListsId

        //        long CarsWorkOrdersId = 0;
        //        long Reference_Type_Table_Id = 0;
        //        long DataDictionaryProcessesListsId = 0;

        //        //   ,   CarsWorkOrdersId,   Reference_Type_Table_Id,   DataDictionaryProcessesListsId



        //        mt.Make_Transaction_BanksBox_FromInvoices(ref Excp, ref Msg, ref opp, ref Order_Mony_Id2, Transaction_Type, Order_No, Box_Id, Box_Balance, Cach_Value, Order_Date, Man_Name, Currency_id, Customer_Id, Reverse_Order, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);
        //        ////print Order
        //        //Order_Receipt_Cash oo = new Order_Receipt_Cash();
        //        //DataSet dsOrderMoney = oo.Get_Order_Cash(ref Excp, Order_Mony_Id2);
        //        //Order_Receipt_CashFrm or = new Order_Receipt_CashFrm(Order_Mony_Id2, Transaction_Type, dsOrderMoney, 0);
        //        //or.LoadDataOnForm();
        //        //or.Set_Print(1);
        //        //or = null;

        //        string Bank_Name = dtBanks.Rows[0]["Bank_Name"].ToString();

        //        mysql = "Update Invoices_List Set BanksNamePaymentMethod =N'" + Bank_Name + "' ,RestValue=" + RestValue + ",PaidValue=" + PaidValueFromMarket + " , Order_MoneyId=" + Order_Mony_Id2 + ", BanksId=" + Box_Id + " Where Id=" + Invoice_Id;
        //        opp.insert_me(mysql);
        //    }
        //    else
        //    {
        //        Msg = Msg + "\n";
        //        Msg = "·„ Ì „ ≈’œ«— «·‰ﬁœÌ… —»„« ·⁄œ„ ÊÃÊœ ’‰œÊﬁ ≈› —«÷Ï ··›Ê« Ì—";
        //    }

        //}
    }
}
