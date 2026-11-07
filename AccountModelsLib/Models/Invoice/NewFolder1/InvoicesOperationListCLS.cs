using AccountModelsLib.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
namespace AccountModelsLib.Models
{
    public class InvoicesOperationListCLS
    {
        public operations opp;
        public InvoicesOperationListCLS()
        {
            opp = new operations();
            invL = new Invoices_List();
        }
       
        public InvoicesOperationListCLS(int isReverse)
        {
            opp = new operations();
            if (isReverse == 1)
            {
                invL = new Invoices_List_Reverse();
            }
            else
            {
                invL = new Invoices_List();
            }
        }
        public InvoicesOperationListCLS(Invoices_ListTemp invLtmp)
        {
            opp = new operations();
            invL = new IInvoices_List();
        }
        public InvoicesOperationListCLS(int isReverse, Invoices_ListTemp invLtmp)
        {
            opp = new operations();
            //if (isReverse == 1)
            //{
            //    invL = new Invoices_List_Reverse();
            //}
            //else
            //{
            //    invL = new Invoices_List();
            //}
        }
        public InvoicesOperationListCLS(ref operations opp_, int isReverse)
        {
            opp = opp_;
            if (isReverse == 1)
            {
                invL = new Invoices_List_Reverse();
            }
            else
            {
                invL = new Invoices_List();
            }
        }
        public CustomerCLS csCls = new CustomerCLS();
        public Customer cs = new Customer();
        public IInvoices_List invL = new IInvoices_List();
        
        //public Invoices_List_Reverse invL = new Invoices_List_Reverse();

        //public Invoices_List invL = new Invoices_List();
        //public Invoices_List_Reverse invL_R = new Invoices_List_Reverse();
        Invoices_Details invD = new Invoices_Details();
        Invoices_Details_Reverse invD_R = new Invoices_Details_Reverse();
        //public List<Unit_Item_SalesDate> salesList = new List<Unit_Item_SalesDate>()  ;
        public List<Unit_Item_SalesDate> salesList   = new List<Unit_Item_SalesDate>();
        public List<AppointmentService > appServiceLst   = new  List<AppointmentService>() ;

        public long Invoices_List_Id = 0;
        public long Price_Type_List_ID { get; set; } = 0;
        
        public int Invoice_Type_Id { get; set; } = 0;

        public decimal Total_Item_Net_Total_Price { get; set; } = 0;

        public decimal Total_Item_CommissionValue { get; set; } = 0;
        public decimal Total_Item_TaxSales10_Value { get; set; } = 0;
        public decimal Total_Item_ATS05_Value { get; set; } = 0;
        public decimal Total_PriceWithoutTax { get; set; } = 0;
         public decimal Total_Item_No{ get; set; }  = 0;

        //public decimal Invoice_Net_Total_Old = 0;
        //public decimal Invoice_Total_Old = 0;
        //public decimal Invoice_Total_PriceWithoutTax_Old = 0;




        DataTable dt_Invoices_List = new DataTable();
        public int Make_Order = 0;

        /*
        #region Properties
        public decimal AnotherPayment = 0;
        public decimal Total { get; set; } = 0;
        public decimal Total_PriceWithoutTax = 0;
        public decimal Net_Total { get; set; } = 0;


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


  
        public long Account_Entry_List_ID = 0;
        public long   User_Id = 0;
        public long Branch_Id = 0;
        
        public long Salesman_Id = 0;
        public int IS_Make_Sarf_Item_BalanceFromInvoice = 0;
        public int Is_InvoiceKta3y = 0;
        public int Is_Tax_Per_Item = 0;

        public long Invoice_Id = 0;
        public long SubAccount_Id = 0;
        public string SubAccount_Name = "";
        //public long Invoice_Id = 0;
        public long Customer_Id = 0;
        public string Customer_Name = "";
        public long Price_Type_List_ID = 0;
        //public string Store_Name = "";

        public DateTime Invoice_Date;
        DateTime Invoice_DateTime;



        
        public string Currency = "";
        public long    Currency_Id = 0;
        public int Invoice_Type = 0;
        public decimal Customer_BalanceSet = 0;
        public string Notes = "";
        public string PolicaNo = "";
        public string troodContainerNo = "";
        public decimal CustomerAccountBefor = 0;
        public decimal CustomerAccountAfter = 0;
        public decimal Equivalent_Net_Total_Price = 0;
        public decimal Equivalent_Net_Total_CommissionValue = 0;
        public DateTime OperationDate;

        
        public decimal Invoice_Net_Total_Old = 0;
        public decimal Invoice_Total_Old = 0;
        public decimal Invoice_Total_PriceWithoutTax_Old = 0;
        public long ProcessNo = 0;  
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
       
        #endregion
          
        public long Insert_Invoices_List(ref operations opp, ref int Excp)
        {
            Invoices_List_Id = 0;
            try
            {
                invL.Invoice_DateTime = invL.Invoice_Date;
                invL.Invoice_Date = invL.Invoice_Date.Value.Date;
                invL.Equivalent_Net_Total_Price = invL.Net_Total;
                invL.Equivalent_Net_Total_CommissionValue = invL.Net_Total_CommissionValue;
                opp.cmd.Parameters.Clear();
                string mysql = "";
                long ProcessNo = Common.Make_AccountsProcessNo(ref Excp, ref opp, "Invoices_List");
                // ProcessNo,
                // "+ ProcessNo + ",
                mysql = " INSERT INTO Invoices_List (ProcessNo,Invoice_Id, Customer_Id, Invoice_Date,Invoice_DateTime, Total, TaxSales10, ATS05,TaxSales10_Value,ATS05_Value , Net_Total , Currency,Currency_id,Invoice_Type,Customer_BalanceSet,Notes,PolicaNo, TroodContainerNo,CustomerAccountBefor, CustomerAccountAfter,SubAccount_Id,SubAccount_Name,User_Id,Branch_Id,AnotherPayment,Salesman_Id,   IS_Make_Sarf_Item_BalanceFromInvoice,  Is_InvoiceKta3y,OperationDate,Total_PriceWithoutTax,Total_Item_CommissionValue, List_CommissionValue,List_CommissionRate,List_Final_CommissionValue,Net_Total_CommissionValue,Equivalent_Net_Total_CommissionValue,Equivalent_Net_Total_Price,Is_Tax_Per_Item,processNo,Customer_Notes,Is_Invoice_Agile,Emp_Id,Emp_Name,ShipmentCompany_Id,ShipmentCompany_Name,Shipment_Value,IsInvoiceTaxInfo  ,IsMaintainceInvoice) ";
                mysql = mysql + " VALUES(@ProcessNo,@Invoice_Id ,@Customer_Id ,@Invoice_Date,@Invoice_DateTime ,@Total ,@TaxSales10 ,@ATS05 ,@TaxSales10_Value ,@ATS05_Value ,@Net_Total ,@Currency  ,@Currency_id ,@Invoice_Type ,@Customer_BalanceSet ,@Notes  ,@PolicaNo  ,@TroodContainerNo  ,@CustomerAccountBefor ,@CustomerAccountAfter ,@SubAccount_Id ,@SubAccount_Name  ,@User_Id ,@Branch_Id ,@AnotherPayment ,@Salesman_Id ,@IS_Make_Sarf_Item_BalanceFromInvoice ,@Is_InvoiceKta3y  ,@OperationDate ,@Total_PriceWithoutTax ,@Total_Item_CommissionValue,@List_CommissionValue,@List_CommissionRate,@List_Final_CommissionValue,@Net_Total_CommissionValue ,@Equivalent_Net_Total_CommissionValue,@Equivalent_Net_Total_Price,@Is_Tax_Per_Item,@processNo,@Customer_Notes,@Is_Invoice_Agile,@Emp_Id,@Emp_Name,@ShipmentCompany_Id,@ShipmentCompany_Name,@Shipment_Value,@IsInvoiceTaxInfo  ,@IsMaintainceInvoice)";


                opp.cmd.Parameters.AddWithValue("@Invoice_Id", invL.Invoice_Id);
                opp.cmd.Parameters.AddWithValue("@ProcessNo", invL.ProcessNo);
                opp.cmd.Parameters.AddWithValue("@Customer_Id", invL.Customer_Id);
                opp.cmd.Parameters.AddWithValue("@Invoice_Date", invL.Invoice_Date);
                opp.cmd.Parameters.AddWithValue("@Invoice_DateTime", invL.Invoice_DateTime);
                opp.cmd.Parameters.AddWithValue("@Total", invL.Total);
                opp.cmd.Parameters.AddWithValue("@TaxSales10", invL.TaxSales10);
                opp.cmd.Parameters.AddWithValue("@ATS05", invL.ATS05);

                opp.cmd.Parameters.AddWithValue("@TaxSales10_Value", invL.TaxSales10_Value);
                opp.cmd.Parameters.AddWithValue("@ATS05_Value", invL.ATS05_Value);
                opp.cmd.Parameters.AddWithValue("@Net_Total", invL.Net_Total);
                opp.cmd.Parameters.AddWithValue("@Currency", invL.Currency);
                opp.cmd.Parameters.AddWithValue("@Currency_id", invL.Currency_id);
                opp.cmd.Parameters.AddWithValue("@Invoice_Type", invL.Invoice_Type);

                opp.cmd.Parameters.AddWithValue("@Customer_BalanceSet", invL.Customer_BalanceSet);
                opp.cmd.Parameters.AddWithValue("@Notes", invL.Notes);

                opp.cmd.Parameters.AddWithValue("@PolicaNo", invL.PolicaNo);
                opp.cmd.Parameters.AddWithValue("@TroodContainerNo", invL.troodContainerNo);

                opp.cmd.Parameters.AddWithValue("@CustomerAccountBefor", invL.CustomerAccountBefor);
                opp.cmd.Parameters.AddWithValue("@CustomerAccountAfter", invL.CustomerAccountAfter);


                opp.cmd.Parameters.AddWithValue("@SubAccount_Id", invL.SubAccount_Id);
                opp.cmd.Parameters.AddWithValue("@SubAccount_Name", invL.SubAccount_Name);


                opp.cmd.Parameters.AddWithValue("@User_Id", Common.User_Id);
                opp.cmd.Parameters.AddWithValue("@Branch_Id", Common.Branch_Id);
                opp.cmd.Parameters.AddWithValue("@OperationDate", DateTime.Now);
                opp.cmd.Parameters.AddWithValue("@AnotherPayment", invL.AnotherPayment);
                opp.cmd.Parameters.AddWithValue("@Salesman_Id", invL.Salesman_Id);

                opp.cmd.Parameters.AddWithValue("@IS_Make_Sarf_Item_BalanceFromInvoice", invL.IS_Make_Sarf_Item_BalanceFromInvoice);
                opp.cmd.Parameters.AddWithValue("@Is_InvoiceKta3y", invL.Is_InvoiceKta3y);
                opp.cmd.Parameters.AddWithValue("@Total_PriceWithoutTax", invL.Total_PriceWithoutTax);

                opp.cmd.Parameters.AddWithValue("@Total_Item_CommissionValue", invL.Total_Item_CommissionValue);
                opp.cmd.Parameters.AddWithValue("@List_CommissionValue", invL.List_CommissionValue);

                opp.cmd.Parameters.AddWithValue("@List_CommissionRate", invL.List_CommissionRate);
                opp.cmd.Parameters.AddWithValue("@List_Final_CommissionValue", invL.List_Final_CommissionValue);
                opp.cmd.Parameters.AddWithValue("@Net_Total_CommissionValue", invL.Net_Total_CommissionValue);
                opp.cmd.Parameters.AddWithValue("@Is_Tax_Per_Item", invL.Is_Tax_Per_Item);
                opp.cmd.Parameters.AddWithValue("@Equivalent_Net_Total_CommissionValue", invL.Equivalent_Net_Total_CommissionValue);


                opp.cmd.Parameters.AddWithValue("@Equivalent_Net_Total_Price", invL.Equivalent_Net_Total_Price);

                opp.cmd.Parameters.AddWithValue("@processNo", invL.ProcessNo);
                opp.cmd.Parameters.AddWithValue("@Customer_Notes", invL.Customer_Notes);
                opp.cmd.Parameters.AddWithValue("@Is_Invoice_Agile", invL.Is_Invoice_Agile);
                opp.cmd.Parameters.AddWithValue("@Emp_Id", invL.Emp_Id);
                opp.cmd.Parameters.AddWithValue("@Emp_Name", invL.Emp_Name);


                //ShipmentCompany_Id,ShipmentCompany_Name,Shipment_Value
                opp.cmd.Parameters.AddWithValue("@ShipmentCompany_Id", invL.ShipmentCompany_Id);
                opp.cmd.Parameters.AddWithValue("@ShipmentCompany_Name", invL.ShipmentCompany_Name);
                opp.cmd.Parameters.AddWithValue("@Shipment_Value", invL.Shipment_Value);
                opp.cmd.Parameters.AddWithValue("@IsInvoiceTaxInfo", invL.IsInvoiceTaxInfo);
                opp.cmd.Parameters.AddWithValue("@IsMaintainceInvoice", invL.IsMaintainceInvoice);


                invL.Id = opp.Insert_me_Return_Last_Identify(mysql);
                Invoices_List_Id = invL.Id;
                return Invoices_List_Id;
            }
            catch (Exception ee)
            {

                Excp = 1;
                return Invoices_List_Id;
            }
        }
        */
        public long Insert_Invoices_List(ref operations opp, ref int Excp, Invoices_List invL)
        {
            
            string Msg = "";
            Invoices_ListCls invoices_ListCls = new Invoices_ListCls();
            invoices_ListCls.AddInvoices_List(ref Excp, ref Msg, ref opp, invL);   
            //opp.db.Invoices_List.Add(invL);
            //opp.db.SaveChanges();

            return invL.Id;
        }
        public long Insert_Invoices_List(ref operations opp, ref int Excp, IInvoices_List invL)
        {
            //var serializedParent = JsonConvert.SerializeObject(invL);
            //Invoices_List invoices_List = JsonConvert.DeserializeObject<Invoices_List>(serializedParent);
            Invoices_List invoices_List = MappingHelper.MapScalarProperties<Invoices_List, IInvoices_List>(invL);
            string Msg = "";
            Invoices_ListCls invoices_ListCls = new Invoices_ListCls();
            invoices_ListCls.AddInvoices_List(ref Excp, ref Msg, ref opp, invoices_List);
            invL.Id = invoices_List.Id;
            //opp.db.Invoices_List.Add(invL);
            //opp.db.SaveChanges();

            return invL.Id;
        }
        public long Insert_Invoices_ListTemp(ref operations opp, ref int Excp, Invoices_ListTemp invL)
        {

            string Msg = "";
            Invoices_ListTempCls invoices_ListCls = new Invoices_ListTempCls();
            invoices_ListCls.AddInvoices_List(ref Excp, ref Msg, ref opp, invL);
        

            return invL.Id;
        }
        public long Insert_Invoices_ListTemp(ref operations opp, ref int Excp, IInvoices_List invL)
        {
            //var serializedParent = JsonConvert.SerializeObject( invL);
            //Invoices_ListTemp invoices_ListTemp = JsonConvert.DeserializeObject<Invoices_ListTemp>(serializedParent);

            Invoices_ListTemp invoices_ListTemp = MappingHelper.MapScalarProperties<Invoices_ListTemp, IInvoices_List>(invL);

            string Msg = "";
            Invoices_ListTempCls invoices_ListCls = new Invoices_ListTempCls();
            invoices_ListCls.AddInvoices_List(ref Excp, ref Msg, ref opp, invoices_ListTemp);
            invL.Id= invoices_ListTemp.Id;
            return invL.Id;
        }
        public void Update_Invoices_List(ref operations opp, ref int Excp)
        {

            try
            {
                invL.Invoice_DateTime = invL.Invoice_Date;
                invL.Invoice_Date = invL.Invoice_Date.Value.Date;
                invL.Equivalent_Net_Total_Price = invL.Net_Total;
                invL.Equivalent_Net_Total_CommissionValue = invL.Net_Total_CommissionValue;

                opp.cmd.Parameters.Clear();
                string mysql = "";

                mysql = " Update Invoices_List  Set Invoice_Id=@Invoice_Id, Customer_Id=@Customer_Id, Invoice_Date=@Invoice_Date," +
                    "Invoice_DateTime=@Invoice_DateTime  , Total=@Total  , TaxSales10=@TaxSales10 , ATS05=@ATS05" +
                    " ,TaxSales10_Value=@TaxSales10_Value ,ATS05_Value =@ATS05_Value , Net_Total =@Net_Total , Currency=@Currency ," +
                    "Currency_id=@Currency_id ,Invoice_Type=@Invoice_Type ,Customer_BalanceSet=@Customer_BalanceSet ,Notes=@Notes ,PolicaNo=@PolicaNo ," +
                    " TroodContainerNo=@TroodContainerNo ,CustomerAccountBefor=@CustomerAccountBefor , CustomerAccountAfter=@CustomerAccountAfter ,SubAccount_Id=@SubAccount_Id ," +
                    "SubAccount_Name=@SubAccount_Name  ,AnotherPayment=@AnotherPayment ,Salesman_Id=@Salesman_Id ,   IS_Make_Sarf_Item_BalanceFromInvoice=@IS_Make_Sarf_Item_BalanceFromInvoice ," +
                    "  Is_InvoiceKta3y=@Is_InvoiceKta3y ,OperationDate=@OperationDate ,Total_PriceWithoutTax=@Total_PriceWithoutTax ," +
                    "Total_Item_CommissionValue=@Total_Item_CommissionValue , List_CommissionValue=@List_CommissionValue ," +
                    "List_CommissionRate=@List_CommissionRate ,List_Final_CommissionValue=@List_Final_CommissionValue ,Net_Total_CommissionValue=@Net_Total_CommissionValue ," +
                    "Equivalent_Net_Total_CommissionValue=@Equivalent_Net_Total_CommissionValue ,Equivalent_Net_Total_Price=@Equivalent_Net_Total_Price ," +
                    "Is_Tax_Per_Item =@Is_Tax_Per_Item,Edit_User_Id=@Edit_User_Id,EditOperationDate=@EditOperationDate,processNo=@processNo,Customer_Notes=@Customer_Notes,Is_Invoice_Agile=@Is_Invoice_Agile,Emp_Id=@Emp_Id,Emp_Name=@Emp_Name ," +
                    "ShipmentCompany_Id=@ShipmentCompany_Id,ShipmentCompany_Name=@ShipmentCompany_Name,Shipment_Value=@Shipment_Value";


                mysql = mysql + "   Where Id=@Invoices_List_Id";




                opp.cmd.Parameters.AddWithValue("@Invoice_Id", invL.Invoice_Id);
                opp.cmd.Parameters.AddWithValue("@Customer_Id", invL.Customer_Id);
                opp.cmd.Parameters.AddWithValue("@Invoice_Date", invL.Invoice_Date);
                opp.cmd.Parameters.AddWithValue("@Invoice_DateTime", invL.Invoice_DateTime);
                opp.cmd.Parameters.AddWithValue("@Total", invL.Total);
                opp.cmd.Parameters.AddWithValue("@TaxSales10", invL.TaxSales10);
                opp.cmd.Parameters.AddWithValue("@ATS05", invL.ATS05);

                opp.cmd.Parameters.AddWithValue("@TaxSales10_Value", invL.TaxSales10_Value);
                opp.cmd.Parameters.AddWithValue("@ATS05_Value", invL.ATS05_Value);
                opp.cmd.Parameters.AddWithValue("@Net_Total", invL.Net_Total);
                opp.cmd.Parameters.AddWithValue("@Currency", invL.Currency);
                opp.cmd.Parameters.AddWithValue("@Currency_id", invL.Currency_id);
                opp.cmd.Parameters.AddWithValue("@Invoice_Type", invL.Invoice_Type);

                opp.cmd.Parameters.AddWithValue("@Customer_BalanceSet", invL.Customer_BalanceSet);
                opp.cmd.Parameters.AddWithValue("@Notes", invL.Notes);

                opp.cmd.Parameters.AddWithValue("@PolicaNo", invL.PolicaNo);
                opp.cmd.Parameters.AddWithValue("@TroodContainerNo", invL.troodContainerNo);

                opp.cmd.Parameters.AddWithValue("@CustomerAccountBefor", invL.CustomerAccountBefor);
                opp.cmd.Parameters.AddWithValue("@CustomerAccountAfter", invL.CustomerAccountAfter);


                opp.cmd.Parameters.AddWithValue("@SubAccount_Id", invL.SubAccount_Id);
                opp.cmd.Parameters.AddWithValue("@SubAccount_Name", invL.SubAccount_Name);



                opp.cmd.Parameters.AddWithValue("@OperationDate", DateTime.Now);
                opp.cmd.Parameters.AddWithValue("@AnotherPayment", invL.AnotherPayment);
                opp.cmd.Parameters.AddWithValue("@Salesman_Id", invL.Salesman_Id);

                opp.cmd.Parameters.AddWithValue("@IS_Make_Sarf_Item_BalanceFromInvoice", invL.IS_Make_Sarf_Item_BalanceFromInvoice);
                opp.cmd.Parameters.AddWithValue("@Is_InvoiceKta3y", invL.Is_InvoiceKta3y);
                opp.cmd.Parameters.AddWithValue("@Total_PriceWithoutTax", invL.Total_PriceWithoutTax);

                opp.cmd.Parameters.AddWithValue("@Total_Item_CommissionValue", invL.Total_Item_CommissionValue);
                opp.cmd.Parameters.AddWithValue("@List_CommissionValue", invL.List_CommissionValue);

                opp.cmd.Parameters.AddWithValue("@List_CommissionRate", invL.List_CommissionRate);
                opp.cmd.Parameters.AddWithValue("@List_Final_CommissionValue", invL.List_Final_CommissionValue);
                opp.cmd.Parameters.AddWithValue("@Net_Total_CommissionValue", invL.Net_Total_CommissionValue);
                opp.cmd.Parameters.AddWithValue("@Is_Tax_Per_Item", invL.Is_Tax_Per_Item);
                opp.cmd.Parameters.AddWithValue("@Equivalent_Net_Total_CommissionValue", invL.Equivalent_Net_Total_CommissionValue);


                opp.cmd.Parameters.AddWithValue("@Equivalent_Net_Total_Price", invL.Equivalent_Net_Total_Price);
                opp.cmd.Parameters.AddWithValue("@Invoices_List_Id", invL.Id);

                opp.cmd.Parameters.AddWithValue("@Edit_User_Id", Common.User_Id);
                opp.cmd.Parameters.AddWithValue("@EditOperationDate", DateTime.Now);
                opp.cmd.Parameters.AddWithValue("@processNo", invL.ProcessNo);
                opp.cmd.Parameters.AddWithValue("@Customer_Notes", invL.Customer_Notes);
                opp.cmd.Parameters.AddWithValue("@Is_Invoice_Agile", invL.Is_Invoice_Agile);
                opp.cmd.Parameters.AddWithValue("@Emp_Id", invL.Emp_Id);
                opp.cmd.Parameters.AddWithValue("@Emp_Name", invL.Emp_Name);

                //ShipmentCompany_Id,ShipmentCompany_Name,Shipment_Value
                opp.cmd.Parameters.AddWithValue("@ShipmentCompany_Id", invL.ShipmentCompany_Id);
                opp.cmd.Parameters.AddWithValue("@ShipmentCompany_Name", invL.ShipmentCompany_Name);
                opp.cmd.Parameters.AddWithValue("@Shipment_Value", invL.Shipment_Value);



                opp.insert_me(mysql);
                Excp = 0;

            }
            catch (Exception ee)
            {

                Excp = 1;

            }
        }
        public void Update_Invoices_List(ref operations opp, ref int Excp, Invoices_List invL)
        {

            try
            {
                invL.Invoice_DateTime = invL.Invoice_Date;
                invL.Invoice_Date = invL.Invoice_Date.Value.Date;
                invL.Equivalent_Net_Total_Price = invL.Net_Total;
                invL.Equivalent_Net_Total_CommissionValue = invL.Net_Total_CommissionValue;

                opp.cmd.Parameters.Clear();

                opp.db.Entry(invL).State = EntityState.Modified;
                opp.db.SaveChanges();
                Excp = 0;

            }
            catch (Exception ee)
            {

                Excp = 1;

            }
        }
        public void Update_Invoices_List(ref operations opp, ref int Excp, IInvoices_List invL)
        {

            try
            {
                //var existingInvoice = opp.db.Invoices_List.Find(invL.Id); // Replace 'Invoices_Lists' with your actual DbSet name
                var existingInvoice = opp.db.Invoices_List.Where(x => x.Id == invL.Id).FirstOrDefault();
                MappingHelper.CopyScalarProperties(invL, existingInvoice);
                //Invoices_List invoices_List = MappingHelper.MapScalarProperties<Invoices_List, IInvoices_List>(invL, true);


                Invoices_ListCls invoices_ListCls = new Invoices_ListCls();
                invoices_ListCls.Update_Invoices_List(ref Excp, ref opp, existingInvoice);

                //Invoices_List invoices_List = MappingHelper.MapScalarProperties<Invoices_List, IInvoices_List>(invL,true);


                //Invoices_ListCls invoices_ListCls = new Invoices_ListCls();
                //invoices_ListCls.Update_Invoices_List(ref Excp,   ref opp, invoices_List);

            }
            catch (Exception ee)
            {

                Excp = 1;

            }
        }

        public void Update_Invoices_ListTemp(ref operations opp, ref int Excp, Invoices_ListTemp invL)
        {

            try
            {
                invL.Invoice_DateTime = invL.Invoice_Date;
                invL.Invoice_Date = invL.Invoice_Date.Value.Date;
                invL.Equivalent_Net_Total_Price = invL.Net_Total;
                invL.Equivalent_Net_Total_CommissionValue = invL.Net_Total_CommissionValue;

                opp.cmd.Parameters.Clear();

                opp.db.Entry(invL).State = EntityState.Modified;
                opp.db.SaveChanges();
                Excp = 0;

            }
            catch (Exception ee)
            {

                Excp = 1;

            }
        }
        public void Update_Invoices_ListTemp(ref operations opp, ref int Excp, IInvoices_List invL)
        {

            try
            {
                Invoices_ListTemp invoices_ListTemp = MappingHelper.MapScalarProperties<Invoices_ListTemp, IInvoices_List>(invL, true);

                Update_Invoices_ListTemp(ref opp, ref Excp, invoices_ListTemp);


            }
            catch (Exception ee)
            {

                Excp = 1;

            }
        }
        public void Get_Invoices_List(long Invoice_List_Table_Id)
        {
            int Excp = 0;
            //operations opp = new operations();
            Get_Invoices_List(ref opp, ref Excp, Invoice_List_Table_Id);

        }
        public Invoices_List Get_Invoices_List(ref operations opp, long Invoice_List_Table_Id)
        {
            Invoices_ListCls invoices_ListCls = new Invoices_ListCls();
            return invoices_ListCls.Get_Invoices_List(ref opp, Invoice_List_Table_Id);
        }
        public Invoices_ListTemp Get_Invoices_ListTemp(ref operations opp,  long Invoice_List_Table_Id)
        {
            Invoices_ListTempCls invoices_ListCls = new Invoices_ListTempCls();

            return invoices_ListCls.Get_Invoices_ListTemp(ref opp, Invoice_List_Table_Id);

 
        }
        public void Get_IInvoices_ListTemp(  long Invoice_List_Table_Id)
        {
            Invoices_ListTempCls invoices_ListCls = new Invoices_ListTempCls();

            invL= invoices_ListCls.Get_IInvoices_ListTemp(ref opp, Invoice_List_Table_Id);


        }
        public long Get_IInvoices_ListIdTemp(long selectedCustomer_Id, long selectedTableId)
        {
            Invoices_ListTempCls invoices_ListCls = new Invoices_ListTempCls();
           
            long Invoice_List_Table_Id = invoices_ListCls.Get_IInvoices_ListIDTemp(ref opp, selectedCustomer_Id, selectedTableId);
            return Invoice_List_Table_Id;

        }
        public void Get_Invoices_List(ref operations opp, ref int Excp, long Invoice_List_Table_Id)
        {
            Invoices_List_Id = Invoice_List_Table_Id;
            try
            {

                opp.cmd.Parameters.Clear();
                //string mysql = "";

                //mysql = " Select * from  Invoices_List  ";
                //mysql = mysql + "   Where Id= " + Invoice_List_Table_Id;
                //dt_Invoices_List = new DataTable();
                //dt_Invoices_List = opp.return_DataTable(mysql);
                //ReadDataTable(ref opp);

                invL = opp.db.Invoices_List.AsNoTracking().Where(i => i.Id == Invoice_List_Table_Id).FirstOrDefault();

                cs = opp.db.Customer.Where(i => i.Id == invL.Customer_Id).FirstOrDefault();

                Excp = 0;

            }
            catch (Exception ee)
            {

                Excp = 1;

            }
        }
        void ReadDataTable(ref operations opp)
        {

            //Customer_Id = long.Parse(dt_Invoices_List.Rows[0]["Customer_Id"].ToString());
            //cs.Get_CustomerByID(ref opp, Customer_Id);

            //Customer_Name = cs.cs.Name;
            //Price_Type_List_ID = (long)cs.cs.Price_Type_List_ID;

            //Invoice_Id = long.Parse(dt_Invoices_List.Rows[0]["Invoice_Id"].ToString());

            //Invoice_Date = DateTime.Parse(dt_Invoices_List.Rows[0]["Invoice_Date"].ToString());
            //Invoice_DateTime = DateTime.Parse(dt_Invoices_List.Rows[0]["Invoice_DateTime"].ToString());
            //Total = decimal.Parse(dt_Invoices_List.Rows[0]["Total"].ToString());
            //TaxSales10 = decimal.Parse(dt_Invoices_List.Rows[0]["TaxSales10"].ToString());
            //ATS05 = decimal.Parse(dt_Invoices_List.Rows[0]["ATS05"].ToString());

            //TaxSales10_Value = decimal.Parse(dt_Invoices_List.Rows[0]["TaxSales10_Value"].ToString());
            //ATS05_Value = decimal.Parse(dt_Invoices_List.Rows[0]["ATS05_Value"].ToString());

            //Currency = dt_Invoices_List.Rows[0]["Currency"].ToString();
            //Currency_id = int.Parse(dt_Invoices_List.Rows[0]["Currency_id"].ToString());
            //Invoice_Type = int.Parse(dt_Invoices_List.Rows[0]["Invoice_Type"].ToString());

            //Customer_BalanceSet = decimal.Parse(dt_Invoices_List.Rows[0]["Customer_BalanceSet"].ToString());
            //Notes = dt_Invoices_List.Rows[0]["Notes"].ToString();

            //PolicaNo = dt_Invoices_List.Rows[0]["PolicaNo"].ToString();
            //troodContainerNo = dt_Invoices_List.Rows[0]["TroodContainerNo"].ToString();

            //CustomerAccountBefor = decimal.Parse(dt_Invoices_List.Rows[0]["CustomerAccountBefor"].ToString());
            //CustomerAccountAfter = decimal.Parse(dt_Invoices_List.Rows[0]["CustomerAccountAfter"].ToString());


            //SubAccount_Id = long.Parse(dt_Invoices_List.Rows[0]["SubAccount_Id"].ToString());
            //SubAccount_Name = dt_Invoices_List.Rows[0]["SubAccount_Name"].ToString();



            //try
            //{
            //    OperationDate = DateTime.Parse(dt_Invoices_List.Rows[0]["OperationDate"].ToString());
            //}
            //catch (Exception e)
            //{

            //}
            //AnotherPayment = decimal.Parse(dt_Invoices_List.Rows[0]["AnotherPayment"].ToString());
            //Salesman_Id = long.Parse(dt_Invoices_List.Rows[0]["Salesman_Id"].ToString());

            //IS_Make_Sarf_Item_BalanceFromInvoice = int.Parse(dt_Invoices_List.Rows[0]["IS_Make_Sarf_Item_BalanceFromInvoice"].ToString());
            //Is_InvoiceKta3y = int.Parse(dt_Invoices_List.Rows[0]["Is_InvoiceKta3y"].ToString());


            //Total_Item_CommissionValue = decimal.Parse(dt_Invoices_List.Rows[0]["Total_Item_CommissionValue"].ToString());
            //List_CommissionValue = decimal.Parse(dt_Invoices_List.Rows[0]["List_CommissionValue"].ToString());

            //List_CommissionRate = decimal.Parse(dt_Invoices_List.Rows[0]["List_CommissionRate"].ToString());
            //List_Final_CommissionValue = decimal.Parse(dt_Invoices_List.Rows[0]["List_Final_CommissionValue"].ToString());
            //Net_Total_CommissionValue = decimal.Parse(dt_Invoices_List.Rows[0]["Net_Total_CommissionValue"].ToString());
            //Is_Tax_Per_Item = int.Parse(dt_Invoices_List.Rows[0]["Is_Tax_Per_Item"].ToString());
            //Equivalent_Net_Total_CommissionValue = decimal.Parse(dt_Invoices_List.Rows[0]["Equivalent_Net_Total_CommissionValue"].ToString());


            //Equivalent_Net_Total_Price = decimal.Parse(dt_Invoices_List.Rows[0]["Equivalent_Net_Total_Price"].ToString());
            //try
            //{

            //    Account_Entry_List_ID = long.Parse(dt_Invoices_List.Rows[0]["Account_Entry_List_ID"].ToString());

            //}
            //catch (Exception e)
            //{
            //    Account_Entry_List_ID = 0;

            //}
            //Invoices_List_Id = long.Parse(dt_Invoices_List.Rows[0]["Id"].ToString());
            //Total_PriceWithoutTax = decimal.Parse(dt_Invoices_List.Rows[0]["Total_PriceWithoutTax"].ToString());
            //Net_Total = decimal.Parse(dt_Invoices_List.Rows[0]["Net_Total"].ToString());


            //Invoice_Net_Total_Old = decimal.Parse(dt_Invoices_List.Rows[0]["Net_Total"].ToString());
            //Invoice_Total_Old = decimal.Parse(dt_Invoices_List.Rows[0]["Total"].ToString());
            //Invoice_Total_PriceWithoutTax_Old = decimal.Parse(dt_Invoices_List.Rows[0]["Total_PriceWithoutTax"].ToString());

            //try
            //{
            //    Is_Invoice_Agile = int.Parse(dt_Invoices_List.Rows[0]["Is_Invoice_Agile"].ToString());
            //}
            //catch (Exception ee)
            //{

            //    Is_Invoice_Agile = 1;
            //}
            //try
            //{
            //    Emp_Id = int.Parse(dt_Invoices_List.Rows[0]["Emp_Id"].ToString());
            //}
            //catch (Exception ee)
            //{

            //    Emp_Id = 0;
            //}
            //Emp_Name = dt_Invoices_List.Rows[0]["Emp_Name"].ToString();

            //try
            //{
            //    ShipmentCompany_Id = long.Parse(dt_Invoices_List.Rows[0]["ShipmentCompany_Id"].ToString());
            //}
            //catch (Exception)
            //{
            //    ShipmentCompany_Id = 0;
            //}
            //try
            //{
            //    Shipment_Value = decimal.Parse(dt_Invoices_List.Rows[0]["Shipment_Value"].ToString());
            //}
            //catch (Exception)
            //{
            //    Shipment_Value = 0;
            //}

            //ShipmentCompany_Name = dt_Invoices_List.Rows[0]["ShipmentCompany_Name"].ToString();



            //try
            //{
            //    Customer_Notes = dt_Invoices_List.Rows[0]["Customer_Notes"].ToString();
            //}
            //catch (Exception ee)
            //{

            //}
            //try
            //{
            //    ProcessNo = long.Parse(dt_Invoices_List.Rows[0]["processNo"].ToString());
            //}
            //catch (Exception ee)
            //{

            //    ProcessNo = 0;
            //}
            //try
            //{
            //    CarsWorkOrdersId = long.Parse(dt_Invoices_List.Rows[0]["CarsWorkOrdersId"].ToString());
            //}
            //catch (Exception ee)
            //{

            //    CarsWorkOrdersId = 0;
            //}


        }

       
        public long Insert_Invoices_List_Reverse(ref int Excp, ref operations opp,  Invoices_List_Reverse invL_R)
        {
           
            string Msg = "";

            Invoices_List_ReverseCls invoices_List_ReverseCls = new Invoices_List_ReverseCls();
            invoices_List_ReverseCls.AddInvoices_List_Reverse(ref Excp, ref Msg, ref opp, invL_R);

            

            return invL_R.Id;
        }
        public long Insert_Invoices_List_Reverse(ref int Excp, ref operations opp, IInvoices_List invL_R)
        {
            //GetProcessNo(invL_R);
            string Msg = "";
            Invoices_List_Reverse invoices_List = MappingHelper.MapScalarProperties<Invoices_List_Reverse, IInvoices_List>(invL_R);
             
        
            invL_R.Id = Insert_Invoices_List_Reverse(ref   Excp, ref opp, invoices_List);
            // كـ Insert_Invoices_List: رقم الرأس لصفوف دفتر الصنف (Set_ItemTransactionFromInvoiceR يكتب Invoice_Id منه) — كان يبقى 0.
            Invoices_List_Id = invL_R.Id;
            return invL_R.Id;
        }
        /*
        public long Insert_Invoices_List_Reverse(ref operations opp, ref int Excp)
        {
            Invoices_List_Id = 0;
            try
            {
                invL.Invoice_DateTime = invL.Invoice_Date;
                invL.Invoice_Date = invL.Invoice_Date.Value.Date;
                invL.Equivalent_Net_Total_Price = invL.Net_Total;
                invL.Equivalent_Net_Total_CommissionValue = invL.Net_Total_CommissionValue;
                opp.cmd.Parameters.Clear();
                string mysql = "";

                long ProcessNo = Common.Make_AccountsProcessNo(ref Excp, ref opp, "Invoices_List_Reverse");
                // ProcessNo,
                // "+ ProcessNo + ",

                mysql = " INSERT INTO Invoices_List_Reverse (ProcessNo,Invoice_Id, Customer_Id, Invoice_Date,Invoice_DateTime, Total, TaxSales10, ATS05,TaxSales10_Value,ATS05_Value , Net_Total , Currency,Currency_id,Invoice_Type,Customer_BalanceSet,Notes,PolicaNo, TroodContainerNo,CustomerAccountBefor, CustomerAccountAfter,SubAccount_Id,SubAccount_Name,User_Id,Branch_Id,AnotherPayment,Salesman_Id,   IS_Make_Sarf_Item_BalanceFromInvoice,  Is_InvoiceKta3y,OperationDate,Total_PriceWithoutTax,Total_Item_CommissionValue, List_CommissionValue,List_CommissionRate,List_Final_CommissionValue,Net_Total_CommissionValue,Equivalent_Net_Total_CommissionValue,Equivalent_Net_Total_Price,Is_Tax_Per_Item,processNo,Customer_Notes,Is_Invoice_Agile,Emp_Id,Emp_Name,ShipmentCompany_Id,ShipmentCompany_Name,Shipment_Value) ";
                mysql = mysql + " VALUES(@ProcessNo,@Invoice_Id ,@Customer_Id ,@Invoice_Date,@Invoice_DateTime ,@Total ,@TaxSales10 ,@ATS05 ,@TaxSales10_Value ,@ATS05_Value ,@Net_Total ,@Currency  ,@Currency_id ,@Invoice_Type ,@Customer_BalanceSet ,@Notes  ,@PolicaNo  ,@TroodContainerNo  ,@CustomerAccountBefor ,@CustomerAccountAfter ,@SubAccount_Id ,@SubAccount_Name  ,@User_Id ,@Branch_Id ,@AnotherPayment ,@Salesman_Id ,@IS_Make_Sarf_Item_BalanceFromInvoice ,@Is_InvoiceKta3y  ,@OperationDate ,@Total_PriceWithoutTax ,@Total_Item_CommissionValue,@List_CommissionValue,@List_CommissionRate,@List_Final_CommissionValue,@Net_Total_CommissionValue ,@Equivalent_Net_Total_CommissionValue,@Equivalent_Net_Total_Price,@Is_Tax_Per_Item,@processNo,@Customer_Notes,@Is_Invoice_Agile,@Emp_Id,@Emp_Name,@ShipmentCompany_Id,@ShipmentCompany_Name,@Shipment_Value)";


                opp.cmd.Parameters.AddWithValue("@ProcessNo", ProcessNo);
                opp.cmd.Parameters.AddWithValue("@Invoice_Id", invL.Invoice_Id);
                opp.cmd.Parameters.AddWithValue("@Customer_Id", invL.Customer_Id);
                opp.cmd.Parameters.AddWithValue("@Invoice_Date", invL.Invoice_Date);
                opp.cmd.Parameters.AddWithValue("@Invoice_DateTime", invL.Invoice_DateTime);
                opp.cmd.Parameters.AddWithValue("@Total", invL.Total);
                opp.cmd.Parameters.AddWithValue("@TaxSales10", invL.TaxSales10);
                opp.cmd.Parameters.AddWithValue("@ATS05", invL.ATS05);

                opp.cmd.Parameters.AddWithValue("@TaxSales10_Value", invL.TaxSales10_Value);
                opp.cmd.Parameters.AddWithValue("@ATS05_Value", invL.ATS05_Value);
                opp.cmd.Parameters.AddWithValue("@Net_Total", invL.Net_Total);
                opp.cmd.Parameters.AddWithValue("@Currency", invL.Currency);
                opp.cmd.Parameters.AddWithValue("@Currency_id", invL.Currency_id);
                opp.cmd.Parameters.AddWithValue("@Invoice_Type", invL.Invoice_Type);

                opp.cmd.Parameters.AddWithValue("@Customer_BalanceSet", invL.Customer_BalanceSet);
                opp.cmd.Parameters.AddWithValue("@Notes", invL.Notes);

                opp.cmd.Parameters.AddWithValue("@PolicaNo", invL.PolicaNo);
                opp.cmd.Parameters.AddWithValue("@TroodContainerNo", invL.troodContainerNo);

                opp.cmd.Parameters.AddWithValue("@CustomerAccountBefor", invL.CustomerAccountBefor);
                opp.cmd.Parameters.AddWithValue("@CustomerAccountAfter", invL.CustomerAccountAfter);


                opp.cmd.Parameters.AddWithValue("@SubAccount_Id", invL.SubAccount_Id);
                opp.cmd.Parameters.AddWithValue("@SubAccount_Name", invL.SubAccount_Name);


                opp.cmd.Parameters.AddWithValue("@User_Id", Common.User_Id);
                opp.cmd.Parameters.AddWithValue("@Branch_Id", Common.Branch_Id);
                opp.cmd.Parameters.AddWithValue("@OperationDate", DateTime.Now);
                opp.cmd.Parameters.AddWithValue("@AnotherPayment", invL.AnotherPayment);
                opp.cmd.Parameters.AddWithValue("@Salesman_Id", invL.Salesman_Id);

                opp.cmd.Parameters.AddWithValue("@IS_Make_Sarf_Item_BalanceFromInvoice", invL.IS_Make_Sarf_Item_BalanceFromInvoice);
                opp.cmd.Parameters.AddWithValue("@Is_InvoiceKta3y", invL.Is_InvoiceKta3y);
                opp.cmd.Parameters.AddWithValue("@Total_PriceWithoutTax", invL.Total_PriceWithoutTax);

                opp.cmd.Parameters.AddWithValue("@Total_Item_CommissionValue", invL.Total_Item_CommissionValue);
                opp.cmd.Parameters.AddWithValue("@List_CommissionValue", invL.List_CommissionValue);

                opp.cmd.Parameters.AddWithValue("@List_CommissionRate", invL.List_CommissionRate);
                opp.cmd.Parameters.AddWithValue("@List_Final_CommissionValue", invL.List_Final_CommissionValue);
                opp.cmd.Parameters.AddWithValue("@Net_Total_CommissionValue", invL.Net_Total_CommissionValue);
                opp.cmd.Parameters.AddWithValue("@Is_Tax_Per_Item", invL.Is_Tax_Per_Item);
                opp.cmd.Parameters.AddWithValue("@Equivalent_Net_Total_CommissionValue", invL.Equivalent_Net_Total_CommissionValue);


                opp.cmd.Parameters.AddWithValue("@Equivalent_Net_Total_Price", invL.Equivalent_Net_Total_Price);

                opp.cmd.Parameters.AddWithValue("@processNo", invL.ProcessNo);
                opp.cmd.Parameters.AddWithValue("@Customer_Notes", invL.Customer_Notes);
                opp.cmd.Parameters.AddWithValue("@Is_Invoice_Agile", invL.Is_Invoice_Agile);
                opp.cmd.Parameters.AddWithValue("@Emp_Id", invL.Emp_Id);
                opp.cmd.Parameters.AddWithValue("@Emp_Name", invL.Emp_Name);

                //ShipmentCompany_Id,ShipmentCompany_Name,Shipment_Value
                opp.cmd.Parameters.AddWithValue("@ShipmentCompany_Id", invL.ShipmentCompany_Id);
                opp.cmd.Parameters.AddWithValue("@ShipmentCompany_Name", invL.ShipmentCompany_Name);
                opp.cmd.Parameters.AddWithValue("@Shipment_Value", invL.Shipment_Value);


                Invoices_List_Id = opp.Insert_me_Return_Last_Identify(mysql);
                Excp = 0;
                return Invoices_List_Id;
            }
            catch (Exception ee)
            {

                Excp = 1;
                return Invoices_List_Id;
            }
        }
        public void Update_Invoices_List_Reverse(ref operations opp, ref int Excp)
        {

            try
            {
                invL.Invoice_DateTime = invL.Invoice_Date;
                invL.Invoice_Date = invL.Invoice_Date.Value.Date;
                invL.Equivalent_Net_Total_Price = invL.Net_Total;
                invL.Equivalent_Net_Total_CommissionValue = invL.Net_Total_CommissionValue;

                opp.cmd.Parameters.Clear();
                string mysql = "";

                mysql = " Update Invoices_List_Reverse  Set Invoice_Id=@Invoice_Id, Customer_Id=@Customer_Id, Invoice_Date=@Invoice_Date," +
                    "Invoice_DateTime=@Invoice_DateTime  , Total=@Total  , TaxSales10=@TaxSales10 , ATS05=@ATS05" +
                    " ,TaxSales10_Value=@TaxSales10_Value ,ATS05_Value =@ATS05_Value , Net_Total =@Net_Total , Currency=@Currency ," +
                    "Currency_id=@Currency_id ,Invoice_Type=@Invoice_Type ,Customer_BalanceSet=@Customer_BalanceSet ,Notes=@Notes ,PolicaNo=@PolicaNo ," +
                    " TroodContainerNo=@TroodContainerNo ,CustomerAccountBefor=@CustomerAccountBefor , CustomerAccountAfter=@CustomerAccountAfter ,SubAccount_Id=@SubAccount_Id ," +
                    "SubAccount_Name=@SubAccount_Name  ,AnotherPayment=@AnotherPayment ,Salesman_Id=@Salesman_Id ,   IS_Make_Sarf_Item_BalanceFromInvoice=@IS_Make_Sarf_Item_BalanceFromInvoice ," +
                    "  Is_InvoiceKta3y=@Is_InvoiceKta3y ,OperationDate=@OperationDate ,Total_PriceWithoutTax=@Total_PriceWithoutTax ," +
                    "Total_Item_CommissionValue=@Total_Item_CommissionValue , List_CommissionValue=@List_CommissionValue ," +
                    "List_CommissionRate=@List_CommissionRate ,List_Final_CommissionValue=@List_Final_CommissionValue ,Net_Total_CommissionValue=@Net_Total_CommissionValue ," +
                    "Equivalent_Net_Total_CommissionValue=@Equivalent_Net_Total_CommissionValue ,Equivalent_Net_Total_Price=@Equivalent_Net_Total_Price ," +
                    "Is_Tax_Per_Item =@Is_Tax_Per_Item,Edit_User_Id=@Edit_User_Id,EditOperationDate=@EditOperationDate,processNo = @processNo,Customer_Notes = @Customer_Notes,Is_Invoice_Agile=@Is_Invoice_Agile,Emp_Id=@Emp_Id ,Emp_Name=@Emp_Name ," +
                    "ShipmentCompany_Id=@ShipmentCompany_Id,ShipmentCompany_Name=@ShipmentCompany_Name,Shipment_Value=@Shipment_Value";

                mysql = mysql + "   Where Id=@Invoices_List_Id";




                opp.cmd.Parameters.AddWithValue("@Invoice_Id", invL.Invoice_Id);
                opp.cmd.Parameters.AddWithValue("@Customer_Id", invL.Customer_Id);
                opp.cmd.Parameters.AddWithValue("@Invoice_Date", invL.Invoice_Date);
                opp.cmd.Parameters.AddWithValue("@Invoice_DateTime", invL.Invoice_DateTime);
                opp.cmd.Parameters.AddWithValue("@Total", invL.Total);
                opp.cmd.Parameters.AddWithValue("@TaxSales10", invL.TaxSales10);
                opp.cmd.Parameters.AddWithValue("@ATS05", invL.ATS05);

                opp.cmd.Parameters.AddWithValue("@TaxSales10_Value", invL.TaxSales10_Value);
                opp.cmd.Parameters.AddWithValue("@ATS05_Value", invL.ATS05_Value);
                opp.cmd.Parameters.AddWithValue("@Net_Total", invL.Net_Total);
                opp.cmd.Parameters.AddWithValue("@Currency", invL.Currency);
                opp.cmd.Parameters.AddWithValue("@Currency_id", invL.Currency_id);
                opp.cmd.Parameters.AddWithValue("@Invoice_Type", invL.Invoice_Type);

                opp.cmd.Parameters.AddWithValue("@Customer_BalanceSet", invL.Customer_BalanceSet);
                opp.cmd.Parameters.AddWithValue("@Notes", invL.Notes);

                opp.cmd.Parameters.AddWithValue("@PolicaNo", invL.PolicaNo);
                opp.cmd.Parameters.AddWithValue("@TroodContainerNo", invL.troodContainerNo);

                opp.cmd.Parameters.AddWithValue("@CustomerAccountBefor", invL.CustomerAccountBefor);
                opp.cmd.Parameters.AddWithValue("@CustomerAccountAfter", invL.CustomerAccountAfter);


                opp.cmd.Parameters.AddWithValue("@SubAccount_Id", invL.SubAccount_Id);
                opp.cmd.Parameters.AddWithValue("@SubAccount_Name", invL.SubAccount_Name);



                opp.cmd.Parameters.AddWithValue("@OperationDate", DateTime.Now);
                opp.cmd.Parameters.AddWithValue("@AnotherPayment", invL.AnotherPayment);
                opp.cmd.Parameters.AddWithValue("@Salesman_Id", invL.Salesman_Id);

                opp.cmd.Parameters.AddWithValue("@IS_Make_Sarf_Item_BalanceFromInvoice", invL.IS_Make_Sarf_Item_BalanceFromInvoice);
                opp.cmd.Parameters.AddWithValue("@Is_InvoiceKta3y", invL.Is_InvoiceKta3y);
                opp.cmd.Parameters.AddWithValue("@Total_PriceWithoutTax", invL.Total_PriceWithoutTax);

                opp.cmd.Parameters.AddWithValue("@Total_Item_CommissionValue", invL.Total_Item_CommissionValue);
                opp.cmd.Parameters.AddWithValue("@List_CommissionValue", invL.List_CommissionValue);

                opp.cmd.Parameters.AddWithValue("@List_CommissionRate", invL.List_CommissionRate);
                opp.cmd.Parameters.AddWithValue("@List_Final_CommissionValue", invL.List_Final_CommissionValue);
                opp.cmd.Parameters.AddWithValue("@Net_Total_CommissionValue", invL.Net_Total_CommissionValue);
                opp.cmd.Parameters.AddWithValue("@Is_Tax_Per_Item", invL.Is_Tax_Per_Item);
                opp.cmd.Parameters.AddWithValue("@Equivalent_Net_Total_CommissionValue", invL.Equivalent_Net_Total_CommissionValue);


                opp.cmd.Parameters.AddWithValue("@Equivalent_Net_Total_Price", invL.Equivalent_Net_Total_Price);
                opp.cmd.Parameters.AddWithValue("@Invoices_List_Id", invL.Id);

                opp.cmd.Parameters.AddWithValue("@Edit_User_Id", Common.User_Id);
                opp.cmd.Parameters.AddWithValue("@EditOperationDate", DateTime.Now);

                opp.cmd.Parameters.AddWithValue("@processNo", invL.ProcessNo);
                opp.cmd.Parameters.AddWithValue("@Customer_Notes", invL.@Customer_Notes);
                opp.cmd.Parameters.AddWithValue("@Is_Invoice_Agile", invL.Is_Invoice_Agile);
                opp.cmd.Parameters.AddWithValue("@Emp_Id", invL.Emp_Id);
                opp.cmd.Parameters.AddWithValue("@Emp_Name", invL.Emp_Name);

                //ShipmentCompany_Id,ShipmentCompany_Name,Shipment_Value
                opp.cmd.Parameters.AddWithValue("@ShipmentCompany_Id", invL.ShipmentCompany_Id);
                opp.cmd.Parameters.AddWithValue("@ShipmentCompany_Name", invL.ShipmentCompany_Name);
                opp.cmd.Parameters.AddWithValue("@Shipment_Value", invL.Shipment_Value);


                opp.insert_me(mysql);
                Excp = 0;

            }
            catch (Exception ee)
            {

                Excp = 1;

            }
        }
       */
        public void Update_Invoices_List_Reverse(ref int Excp, ref operations opp, Invoices_List_Reverse invL_R)
        {
            

            Invoices_List_ReverseCls invoices_List_ReverseCls = new Invoices_List_ReverseCls();
            invoices_List_ReverseCls.Update_Invoices_List_Reverse(ref Excp,   ref opp, invL_R);
        }
        public void Update_Invoices_List_Reverse(ref int Excp, ref operations opp,  IInvoices_List invL)
        {

            try
            {
                Invoices_List_Reverse invoices_List = MappingHelper.MapScalarProperties<Invoices_List_Reverse, IInvoices_List>(invL, true);

                Update_Invoices_List_Reverse(ref Excp, ref opp,  invoices_List);


            }
            catch (Exception ee)
            {

                Excp = 1;

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
                //string mysql = "";

                //mysql = " Select * from  Invoices_List_Reverse  ";
                //mysql = mysql + "   Where Id= " + Invoice_List_Table_Id;
                //dt_Invoices_List = new DataTable();
                //dt_Invoices_List = opp.return_DataTable(mysql);
                //ReadDataTable(ref opp);


                invL = opp.db.Invoices_List_Reverse.Where(i => i.Id == Invoice_List_Table_Id).FirstOrDefault(); ;

                cs = opp.db.Customer.Where(i => i.Id == invL.Customer_Id).FirstOrDefault();
                Excp = 0;

            }
            catch (Exception ee)
            {

                Excp = 1;

            }
        }
    }
}
