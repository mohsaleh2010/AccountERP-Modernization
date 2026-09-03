using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
   
using System.Data;
using AccountModelsLib.Models;
namespace AccountModelsLib.Models
{
    public class InvoicesOperationListCLSOld
    {
        public InvoicesOperationListCLSOld()
        {

        }
        CustomerCLS cs = new CustomerCLS();
        Invoices_List invL = new Invoices_List();
        Invoices_Details invD = new Invoices_Details();

        #region Properties
        public decimal AnotherPayment = 0;
        public decimal Total_Qnty_Value { get; set; } = 0;
        public decimal Total_PriceWithoutTax = 0;
        public decimal Net_Total_Price { get; set; } = 0;


        public decimal Total_Item_Net_Total_Price = 0;

        public decimal TaxSales10 = 0;
        public decimal ATS05 = 0;

        public decimal TaxSales10_Value = 0;
        public decimal ATS05_Value = 0;

        public decimal Total_Item_TaxSales10_Value = 0;
        public decimal Total_Item_ATS05_Value = 0;
        public decimal Total_Item_No = 0;

        public decimal Total_Item_CommissionValue = 0;

        public decimal List_CommissionValue = 0;
        public decimal List_CommissionRate = 0;
        public decimal List_Final_CommissionValue = 0;

        public decimal Net_Total_CommissionValue = 0;


        public long Invoices_List_Id = 0;
        public long Account_Entry_List_ID = 0;
        public int User_Id = 0;
        public int Branch_Id = 0;
        public int Invoice_Type_Id = 0;
        public int Salesman_Id = 0;
        public int IS_Make_Sarf_Item_BalanceFromInvoice = 0;
        public int Is_InvoiceKta3y = 0;
        public int Is_Tax_Per_Item = 0;

        public long Invoice_No = 0;
        public long SubAccount_Id = 0;
        public string SubAccount_Name = "";
        //public long Invoice_Id = 0;
        public long Customer_Id = 0;
        public string Customer_Name = "";
        public long Price_Type_List_ID = 0;
        //public string Store_Name = "";

        public DateTime Invoice_Date;
        DateTime Invoice_DateTime;



        public int Make_Order = 0;
        public string Currency = "";
        public int Currency_id = 0;
        public int Invoice_Type = 0;
        public decimal Customer_BalanceSet = 0;
        public string Notes = "";
        public string PolicaNo = "";
        public string TroodContainerNo = "";
        public decimal CustomerAccountBefor = 0;
        public decimal CustomerAccountAfter = 0;
        public decimal Equivalent_Net_Total_Price = 0;
        public decimal Equivalent_Net_Total_CommissionValue = 0;
        public DateTime OperationDate;

        DataTable dt_Invoices_List = new DataTable();
        public decimal Invoice_Net_Total_Old = 0;
        public decimal Invoice_Total_Old = 0;
        public decimal Invoice_Total_PriceWithoutTax_Old = 0;
        public long processNo = 0;  
        public long CarsWorkOrdersId = 0; 

        public string Customer_Notes = "";
        public int Is_Invoice_Agile = 1;

        public long Emp_Id { get; set; } = 0;
        //public long Emp_Id = 0;
        public string Emp_Name = "";

        public long ShipmentCompany_Id = 0;
        public string ShipmentCompany_Name = "";
        public decimal Shipment_Value = 0;
        public int IsInvoiceTaxInfo = 0;
        public int IsMaintainceInvoice = 0;
        //IsInvoiceTaxInfo,IsMaintainceInvoice
        #endregion

      
     
         

        public void Get_Invoices_List(ref operations opp, ref int Excp, long Invoice_List_Table_Id)
        {
            Invoices_List_Id = Invoice_List_Table_Id;
            try
            {

                opp.cmd.Parameters.Clear();
                string mysql = "";

                mysql = " Select * from  Invoices_List  ";
                mysql = mysql + "   Where Id= " + Invoice_List_Table_Id;
                dt_Invoices_List = new DataTable();
                dt_Invoices_List = opp.return_DataTable(mysql);
                ReadDataTable(ref opp);

                Excp = 0;

            }
            catch (Exception ee)
            {

                Excp = 1;

            }
        }
        void ReadDataTable(ref operations opp)
        {

            Customer_Id = long.Parse(dt_Invoices_List.Rows[0]["Customer_Id"].ToString());
            cs.Get_CustomerByID(ref opp, Customer_Id);

            Customer_Name = cs.cs.Name;
            Price_Type_List_ID = (long)cs.cs.Price_Type_List_ID;

            Invoice_No = long.Parse(dt_Invoices_List.Rows[0]["Invoice_Id"].ToString());

            Invoice_Date = DateTime.Parse(dt_Invoices_List.Rows[0]["Invoice_Date"].ToString());
            Invoice_DateTime = DateTime.Parse(dt_Invoices_List.Rows[0]["Invoice_DateTime"].ToString());
            Total_Qnty_Value = decimal.Parse(dt_Invoices_List.Rows[0]["Total"].ToString());
            TaxSales10 = decimal.Parse(dt_Invoices_List.Rows[0]["TaxSales10"].ToString());
            ATS05 = decimal.Parse(dt_Invoices_List.Rows[0]["ATS05"].ToString());

            TaxSales10_Value = decimal.Parse(dt_Invoices_List.Rows[0]["TaxSales10_Value"].ToString());
            ATS05_Value = decimal.Parse(dt_Invoices_List.Rows[0]["ATS05_Value"].ToString());

            Currency = dt_Invoices_List.Rows[0]["Currency"].ToString();
            Currency_id = int.Parse(dt_Invoices_List.Rows[0]["Currency_id"].ToString());
            Invoice_Type = int.Parse(dt_Invoices_List.Rows[0]["Invoice_Type"].ToString());

            Customer_BalanceSet = decimal.Parse(dt_Invoices_List.Rows[0]["Customer_BalanceSet"].ToString());
            Notes = dt_Invoices_List.Rows[0]["Notes"].ToString();

            PolicaNo = dt_Invoices_List.Rows[0]["PolicaNo"].ToString();
            TroodContainerNo = dt_Invoices_List.Rows[0]["TroodContainerNo"].ToString();

            CustomerAccountBefor = decimal.Parse(dt_Invoices_List.Rows[0]["CustomerAccountBefor"].ToString());
            CustomerAccountAfter = decimal.Parse(dt_Invoices_List.Rows[0]["CustomerAccountAfter"].ToString());


            SubAccount_Id = long.Parse(dt_Invoices_List.Rows[0]["SubAccount_Id"].ToString());
            SubAccount_Name = dt_Invoices_List.Rows[0]["SubAccount_Name"].ToString();



            try
            {
                OperationDate = DateTime.Parse(dt_Invoices_List.Rows[0]["OperationDate"].ToString());
            }
            catch (Exception e)
            {
                 
            }
            AnotherPayment = decimal.Parse(dt_Invoices_List.Rows[0]["AnotherPayment"].ToString());
            Salesman_Id = int.Parse(dt_Invoices_List.Rows[0]["Salesman_Id"].ToString());

            IS_Make_Sarf_Item_BalanceFromInvoice = int.Parse(dt_Invoices_List.Rows[0]["IS_Make_Sarf_Item_BalanceFromInvoice"].ToString());
            Is_InvoiceKta3y = int.Parse(dt_Invoices_List.Rows[0]["Is_InvoiceKta3y"].ToString());


            Total_Item_CommissionValue = decimal.Parse(dt_Invoices_List.Rows[0]["Total_Item_CommissionValue"].ToString());
            List_CommissionValue = decimal.Parse(dt_Invoices_List.Rows[0]["List_CommissionValue"].ToString());

            List_CommissionRate = decimal.Parse(dt_Invoices_List.Rows[0]["List_CommissionRate"].ToString());
            List_Final_CommissionValue = decimal.Parse(dt_Invoices_List.Rows[0]["List_Final_CommissionValue"].ToString());
            Net_Total_CommissionValue = decimal.Parse(dt_Invoices_List.Rows[0]["Net_Total_CommissionValue"].ToString());
            Is_Tax_Per_Item = int.Parse(dt_Invoices_List.Rows[0]["Is_Tax_Per_Item"].ToString());
            Equivalent_Net_Total_CommissionValue = decimal.Parse(dt_Invoices_List.Rows[0]["Equivalent_Net_Total_CommissionValue"].ToString());


            Equivalent_Net_Total_Price = decimal.Parse(dt_Invoices_List.Rows[0]["Equivalent_Net_Total_Price"].ToString());
            try
            {

                Account_Entry_List_ID = long.Parse(dt_Invoices_List.Rows[0]["Account_Entry_List_ID"].ToString());

            }
            catch (Exception e)
            {
                Account_Entry_List_ID = 0;
                               
            }
            Invoices_List_Id = long.Parse(dt_Invoices_List.Rows[0]["Id"].ToString());
            Total_PriceWithoutTax = decimal.Parse(dt_Invoices_List.Rows[0]["Total_PriceWithoutTax"].ToString());
            Net_Total_Price = decimal.Parse(dt_Invoices_List.Rows[0]["Net_Total"].ToString());


            Invoice_Net_Total_Old = decimal.Parse(dt_Invoices_List.Rows[0]["Net_Total"].ToString());
            Invoice_Total_Old = decimal.Parse(dt_Invoices_List.Rows[0]["Total"].ToString());
            Invoice_Total_PriceWithoutTax_Old = decimal.Parse(dt_Invoices_List.Rows[0]["Total_PriceWithoutTax"].ToString());

            try
            {
                Is_Invoice_Agile = int.Parse(dt_Invoices_List.Rows[0]["Is_Invoice_Agile"].ToString());
            }
            catch (Exception ee)
            {

                Is_Invoice_Agile = 1;
            }
            try
            {
                Emp_Id = int.Parse(dt_Invoices_List.Rows[0]["Emp_Id"].ToString());
            }
            catch (Exception ee)
            {

                Emp_Id = 0;
            }
            Emp_Name = dt_Invoices_List.Rows[0]["Emp_Name"].ToString();

            try
            {
                ShipmentCompany_Id = long.Parse(dt_Invoices_List.Rows[0]["ShipmentCompany_Id"].ToString());
            }
            catch (Exception)
            {
                ShipmentCompany_Id = 0;
            }
            try
            {
                Shipment_Value = decimal.Parse(dt_Invoices_List.Rows[0]["Shipment_Value"].ToString());
            }
            catch (Exception)
            {
                Shipment_Value = 0;
            }

            ShipmentCompany_Name = dt_Invoices_List.Rows[0]["ShipmentCompany_Name"].ToString();



            try
            {
                Customer_Notes = dt_Invoices_List.Rows[0]["Customer_Notes"].ToString();
            }
            catch (Exception ee)
            {

            }
            try
            {
                processNo = long.Parse(dt_Invoices_List.Rows[0]["processNo"].ToString());
            }
            catch (Exception ee)
            {

                processNo = 0;
            }
            try
            {
                CarsWorkOrdersId = long.Parse(dt_Invoices_List.Rows[0]["CarsWorkOrdersId"].ToString());
            }
            catch (Exception ee)
            {

                CarsWorkOrdersId = 0;
            }
            

        }



       
      
        public void Get_Invoices_List_Reverse(long Invoice_List_Table_Id)
        {
            int Excp = 0;
            operations opp = new operations();
            Get_Invoices_List_Reverse(ref opp, ref Excp, Invoice_List_Table_Id);

        }

        public void Get_Invoices_List_Reverse(ref operations opp, ref int Excp, long Invoice_List_Table_Id)
        {
            Invoices_List_Id = Invoice_List_Table_Id;
            try
            {

                opp.cmd.Parameters.Clear();
                string mysql = "";

                mysql = " Select * from  Invoices_List_Reverse  ";
                mysql = mysql + "   Where Id= " + Invoice_List_Table_Id;
                dt_Invoices_List = new DataTable();
                dt_Invoices_List = opp.return_DataTable(mysql);
                ReadDataTable(ref opp);

                Excp = 0;

            }
            catch (Exception ee)
            {

                Excp = 1;

            }
        }
    }
}
