using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AccountModelsLib.Models;


namespace AccountModelsLib.Models
{
    public class InvoiceNewCls
    {
        Invoices_ListCls inv_ListCls;
        public Invoices_List invList = new Invoices_List();
        public List<Invoices_Details> invDs = new List<Invoices_Details>();
       public  operations opp;
        public InvoiceNewCls()
        {
            opp = new operations();
            inv_ListCls = new Invoices_ListCls(ref opp);
        }
        public InvoiceNewCls(ref operations opp_)
        {
            opp = opp_;
            inv_ListCls = new Invoices_ListCls(ref opp);
        }
       
        public Invoices_Details invDNewEdit { get; set; } = new Invoices_Details();

        public void Get_Invoice_By_Table_Id(long Invoice_List_table_Id)
        {
            string mysql = "";
            
              mysql = " UPDATE       Invoices_Details SET Invoice_Item_Name = Item.Item_Name FROM Invoices_Details INNER JOIN Item ON Invoices_Details.Item_Id = Item.id WHERE(Invoices_Details.Invoice_Item_Name IS NULL)";
            opp.insert_me(mysql);

            invDs = new List<Invoices_Details>();
            invDs= inv_ListCls.GetInvoices_Details(ref opp, Invoice_List_table_Id);

            invList = new Invoices_List();
            invList = inv_ListCls.GetInvoices_ListById(ref opp, Invoice_List_table_Id);
            //Invoices_List.commission,
            
            
            //mysql = @"    SELECT DISTINCT 
            //             '' AS Serial_, '' AS Size_, '0' AS Make_Order, '100' AS Item_DeletedFlage, '100' AS Item_ProcessedFlage, Invoices_List.IS_Make_Sarf_Item_BalanceFromInvoice, Invoices_List.Is_InvoiceKta3y, Invoices_List.Salesman_Id, 
            //             Invoices_List.commissionValue, Invoices_List.IsCommission_Value, Invoices_List.CustomerAccountAfter, Invoices_List.CustomerAccountBefor, Invoices_List.PolicaNo, Invoices_List.troodContainerNo, Invoices_List.ID, 
            //             Invoices_List.AnotherPayment, Invoices_List.Invoice_Id, Customer.Name, Customer.Address, Invoices_List.Invoice_Date, Invoices_List.Invoice_DateTime, Invoices_List.Total,  
            //             Invoices_List.TaxSales10, Invoices_List.ATS05, Invoices_List.Net_Total, Invoices_List.Currency_id, Invoices_List.Notes, Invoices_List.Invoice_Type, Invoices_Details.Item_Id, Invoices_Details.Unit_Item_Table_Id, 
            //             Invoices_Details.Larg_Qnty, Invoices_Details.Small_Qnty,Invoices_Details.Qnty_Another,  Invoices_Details.Larg_Unit_Price, Invoices_Details.Small_Unit_Price, Invoices_Details.Qnty_value, 
            //             Invoices_Details.Unit_Used_for_Price, Invoices_Details.Total AS Item_Total, Invoices_Details.Item_Net_Total_Price, Invoices_Details.Notes AS Item_Notes, Invoices_Details.Unit_Id, Item.code, Invoices_Details.Specification, 
            //             Item.Item_Name, Item.AnotherName, unit.u_name, unit.B_Unit, unit.S_unit, unit.B_Unit_Value, unit.S_unit_Value, unit.S_unit_Value AS Small_Unit_Value, Unit_item.Full_qua AS Available_Larg_Balance, 
            //             Unit_item.D_Qua AS Available_Small_Balance, Unit_item.Item_No  , Invoices_Details.Id AS Invoices_Details_Id, Customer.Balance AS Customer_Balance, Customer.Price_Type_List_ID, Invoices_Details.Store_Id, 
            //             Invoices_Details.Invoice_Item_Name, Invoices_Details.Order_Purshing_Id_Automatic, Invoices_Details.Balance_Checked, Store.Store_Name, Store.Store_Code, Store.IS_Default_Store, 
            //             Invoices_Details.Item_CommissionValue, Invoices_Details.Unit_CommissionValue, Invoices_Details.Is_Item_CommissionValue, Invoices_Details.QntyWidth, Invoices_Details.QntyLenght, Invoices_Details.QntyItemNo, 
            //             Invoices_Details.OperationNo, Invoices_Details.Merchant_Name, Item.Somk_Unit, Item.Somk, Item.Height_, Item.Width_, Item.Items_Types_Name, Item.Items_Types_Name_Id, Item.Height_Width_Str, 
            //             Item.Items_Name_Somk_Str, Item.User_Id, Item.OperationDate, Customer.Tax_No, Invoices_List.Total_PriceWithoutTax, Invoices_Details.Tash3ela, Invoices_Details.Item_No_Free, Invoices_Details.LargItemsQnty_Free, 
            //             Invoices_Details.SmallItemsQnty_Free, Invoices_Details.Item_Expire_Date, Invoices_Details.Is_Alternate_Item_CommissionValue, Invoices_Details.Alternate_Item_CommissionValue, 
            //             Invoices_Details.Alternate_ItemCommissionTotal, Invoices_Details.Net_Total_Item_Commission, Invoices_Details.Alternate_Item_CommissionRate, Invoices_Details.Item_CommissionRate, 
            //             Invoices_Details.Alternate_Unit_CommissionValue, Invoices_Details.Unit_Item_ExpireDate_Id, Invoices_Details.ItemCommissionTotal, Invoices_Details.Audience_price, Invoices_Details.NameSpecification, 
            //             Invoices_Details.StatementNo, Invoices_Details.Item_Total_WithoutTax, Invoices_List.Customer_Id, Invoices_Details.QntyStrColumn, Invoices_Details.QntyStr, Unit_item.Reserved_Balance_Larg_Qnty, 
            //             Unit_item.Reserved_Balance_Small_Qnty, Invoices_List.TaxSales10_Value, Invoices_List.ATS05_Value, Invoices_Details.Item_TaxSales10, Invoices_Details.Item_TaxSales10_Value, Invoices_Details.Item_ATS05, 
            //             Invoices_Details.Item_ATS05_Value, Invoices_List.List_CommissionValue, Invoices_List.List_CommissionRate, Invoices_List.List_Final_CommissionValue, Invoices_List.Net_Total_CommissionValue, 
            //             Invoices_List.Equivalent_Net_Total_CommissionValue, Invoices_List.Is_Tax_Per_Item, Invoices_Details.Bills_Details_Id, Invoices_Details.Purchase_Price, Invoices_Details.Is_InvoiceItem_Kta3y, 
            //             Invoices_List.Is_Buy_From_Bills_Items_Balance,Invoices_List.Order_MoneyId,Invoices_List.BanksId,
            //                Invoices_List.BanksNamePaymentMethod  ,Invoices_List.PaidValue  ,Invoices_List.RestValue
            //            ,Invoices_List.Customer_Notes,Invoices_Details.TotalSmall_QntyStrColumn 
            //            ,Invoices_Details.TotalSmall_QntyStr,Invoices_Details.TotalSmall_Qnty
            //            ,Invoices_List.Is_Invoice_Agile,Invoices_List.Emp_Id,Invoices_List.Emp_Name
            //            ,Invoices_List.ShipmentCompany_Id,Invoices_List.ShipmentCompany_Name,Invoices_List.Shipment_Value
            //            ,  Item.No3eboa, Item.No3eboa_BigUnit, Item.No3eboa_SmallUnit
            //FROM Item INNER JOIN
            //             Unit_item ON Item.id = Unit_item.Item_id INNER JOIN
            //             Invoices_List INNER JOIN
            //             Customer ON Invoices_List.Customer_Id = Customer.Id INNER JOIN
            //             Invoices_Details ON Invoices_List.ID = Invoices_Details.Invoice_Id ON Unit_item.id = Invoices_Details.Unit_Item_Table_Id INNER JOIN
            //             unit ON Unit_item.unit_id = unit.id INNER JOIN
            //             Store ON Unit_item.Store_id = Store.id
            //    ";


            //mysql = mysql + " WHERE (Invoices_List.ID = " + Invoice_List_table_Id.ToString() + ") Order by Invoices_Details.ID  ";
            
        }
        public ArrayList Get_arr_Order_Purshing_Id_Automatic_From_Invoice(long Invoice_Id)
        {
            ArrayList arr_Order_Purshing_Id_Automatic = new ArrayList();
            operations opp = new operations();
            string mysql = "SELECT Invoices_Details.Order_Purshing_Id_Automatic FROM Invoices_Details WHERE (Invoice_Id = " + Invoice_Id + ")";
            DataTable dt = opp.return_DataTable(mysql);
            for (int x = 0; x < dt.Rows.Count; x++)
            {
                arr_Order_Purshing_Id_Automatic.Add(dt.Rows[x]["Order_Purshing_Id_Automatic"].ToString());
            }
            return arr_Order_Purshing_Id_Automatic;

        }
    }
}
