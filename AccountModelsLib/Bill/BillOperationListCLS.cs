using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AccountModelsLib;
using AccountModelsLib.Models;
using System.Data;

namespace AccountModelsLib
{
    public class BillOperationListCLS
    {
         
        SupplierCLS sp;
        public AccountModelsLib.operations opp;
        public Supplier supplier = new Supplier();
        public List<Unit_Item_SalesDate> salesList = new List<Unit_Item_SalesDate>();
        //public List<IBills_Details> DLst =new List<IBills_Details>();
        public IBills_List invL = new IBills_List();
        public BillOperationListCLS()
        {
            opp = new AccountModelsLib.operations();
            invL = new  Bills_List (); 
        }
       
        
        public BillOperationListCLS(int isReverse)
        {
            opp = new AccountModelsLib.operations();
            if (isReverse == 1)
            {
                invL = new Bills_List_Reverse();
            }
            else
            {
                invL = new Bills_List();
            }
        }
        public BillOperationListCLS(ref AccountModelsLib.operations opp_, int isReverse)
        {
            opp = opp_;
            if (isReverse == 1)
            {
                invL = new Bills_List_Reverse();
            }
            else
            {
                invL = new Bills_List();
            }
        }

        #region Properties

        public int Is_Invoice_Agile { get; set; } = 0;
        public long Bills_List_Id = 0;
        public int IS_Make_Sarf_Item_BalanceFromInvoice = 0;

        //public decimal AnotherPayment = 0;
        public decimal Total_Qnty_Value = 0;
        public decimal Total_PriceWithoutTax = 0;
        public decimal Net_Total_Price = 0;


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


        public long Account_Entry_List_ID { get; set; } = 0;
        public long Order_MoneyId = 0;


        public long   User_Id = 0;
        public long Branch_Id = 0;
        public int Invoice_Type_Id = 0;
        public long Salesman_Id = 0;
       
        public int Is_InvoiceKta3y = 0;
        public int Is_Tax_Per_Item = 0;

        public long Invoice_No = 0;
        public long SubAccount_Id = 0;
        public string SubAccount_Name = "";
        //public long Invoice_Id = 0;
        public long Supplier_Id = 0;
        public string Supplier_Name = "";
        //public string Store_Name = "";

        public DateTime Invoice_Date;
        DateTime Invoice_DateTime;
        //public decimal Total = 0;


        public int Make_Order = 0;
        public string Currency = "";
        public long    Currency_Id = 0;
        public int Invoice_Type = 0;
        public decimal Supplier_BalanceSet = 0;
        public string Notes = "";
        public string PolicaNo = "";
        public string TroodContainerNo = "";
        public decimal SupplierAccountBefor = 0;
        public decimal SupplierAccountAfter = 0;
        public decimal Equivalent_Net_Total_Price = 0;
        public decimal Equivalent_Net_Total_CommissionValue = 0;
        public DateTime OperationDate;

        DataTable dt_Invoices_List = new DataTable();
        public decimal Invoice_Net_Total_Old = 0;
        public decimal Invoice_Total_Old = 0;
        public decimal Invoice_Total_PriceWithoutTax_Old = 0;



        public decimal Load_Payment = 0;
        public decimal Damaghat = 0;
        public int Is_Load_Payment_OnTax = 0;

        public decimal Total_PriceWithoutTax_Load_Payment = 0;

        public decimal PriceByTenns = 0;
        public long ProcessNo = 0;
        public string Supplier_Notes = "";

        
        public string Is_Invoice_AgileStr { get; set; } = "";
        #endregion

        public long Insert_Bill_List(ref AccountModelsLib.operations opp, ref int Excp)
        {
            return Insert_Bill_List(ref opp, ref Excp, "Bills_List");

        }

        
        public long Insert_Bill_List(ref AccountModelsLib.operations opp, ref int Excp, Bills_List invL)
        {
            opp.db.Bills_List.Add(invL);
            opp.db.SaveChanges();

            return invL.Id;
        }


        public void Update_Bill_List(ref AccountModelsLib.operations opp, ref int Excp)
        {
            Update_Bill_List(ref opp, ref Excp, "Bills_List");

        }
        public void Get_Invoices_List(long Invoice_List_Table_Id)
        {
            int Excp = 0;
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            Get_Invoices_List(ref opp, ref Excp, Invoice_List_Table_Id);

        }

        public void Get_Invoices_List(ref AccountModelsLib.operations opp, ref int Excp, long Invoice_List_Table_Id)
        {

            try
            {
                Bills_List_Id = Invoice_List_Table_Id;
                opp.cmd.Parameters.Clear();
                string mysql = "";

                mysql = " Select * from  Bills_List  ";
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
        void ReadDataTable(ref AccountModelsLib.operations opp)
        {
            Supplier_Id = long.Parse(dt_Invoices_List.Rows[0]["Supplier_Id"].ToString());
            sp = new SupplierCLS();
            sp.Get_SupplierById(ref opp, Supplier_Id);
            Supplier_Name = sp.sp.Name;



            Invoice_No = long.Parse(dt_Invoices_List.Rows[0]["Invoice_Id"].ToString());

            Invoice_Date = DateTime.Parse(dt_Invoices_List.Rows[0]["Invoice_Date"].ToString());
            Invoice_DateTime = DateTime.Parse(dt_Invoices_List.Rows[0]["Invoice_DateTime"].ToString());

            TaxSales10 = decimal.Parse(dt_Invoices_List.Rows[0]["TaxSales10"].ToString());
            ATS05 = decimal.Parse(dt_Invoices_List.Rows[0]["ATS05"].ToString());

            TaxSales10_Value = decimal.Parse(dt_Invoices_List.Rows[0]["TaxSales10_Value"].ToString());
            ATS05_Value = decimal.Parse(dt_Invoices_List.Rows[0]["ATS05_Value"].ToString());
            Net_Total_Price = decimal.Parse(dt_Invoices_List.Rows[0]["Net_Total"].ToString());
            Currency = dt_Invoices_List.Rows[0]["Currency"].ToString();
            Currency_Id = long.Parse(dt_Invoices_List.Rows[0]["Currency_id"].ToString());
            Invoice_Type = int.Parse(dt_Invoices_List.Rows[0]["Invoice_Type"].ToString());

            Supplier_BalanceSet = decimal.Parse(dt_Invoices_List.Rows[0]["Supplier_BalanceSet"].ToString());
            Notes = dt_Invoices_List.Rows[0]["Notes"].ToString();

            PolicaNo = dt_Invoices_List.Rows[0]["PolicaNo"].ToString();
            TroodContainerNo = dt_Invoices_List.Rows[0]["TroodContainerNo"].ToString();

            SupplierAccountBefor = decimal.Parse(dt_Invoices_List.Rows[0]["SupplierAccountBefor"].ToString());
            SupplierAccountAfter = decimal.Parse(dt_Invoices_List.Rows[0]["SupplierAccountAfter"].ToString());


            SubAccount_Id = long.Parse(dt_Invoices_List.Rows[0]["SubAccount_Id"].ToString());
            SubAccount_Name = dt_Invoices_List.Rows[0]["SubAccount_Name"].ToString();



            OperationDate = DateTime.Parse(dt_Invoices_List.Rows[0]["OperationDate"].ToString());
            //AnotherPayment = decimal.Parse(dt_Invoices_List.Rows[0]["AnotherPayment"].ToString());
            Salesman_Id = long.Parse(dt_Invoices_List.Rows[0]["Salesman_Id"].ToString());

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
            catch (Exception ee)
            {
                Account_Entry_List_ID = 0;
            }
            try
            {
                Order_MoneyId = long.Parse(dt_Invoices_List.Rows[0]["Order_MoneyId"].ToString());
            }
            catch (Exception ee)
            {
                Order_MoneyId = 0;
            }
            
            Total_Qnty_Value = decimal.Parse(dt_Invoices_List.Rows[0]["Total"].ToString());

            Load_Payment = decimal.Parse(dt_Invoices_List.Rows[0]["Load_Payment"].ToString());
            Damaghat = decimal.Parse(dt_Invoices_List.Rows[0]["Damaghat"].ToString());
            Is_Load_Payment_OnTax = int.Parse(dt_Invoices_List.Rows[0]["Is_Load_Payment_OnTax"].ToString());
            PriceByTenns = decimal.Parse(dt_Invoices_List.Rows[0]["PriceByTenns"].ToString());
            Total_PriceWithoutTax_Load_Payment = decimal.Parse(dt_Invoices_List.Rows[0]["Total_PriceWithoutTax_Load_Payment"].ToString());
            Total_PriceWithoutTax = decimal.Parse(dt_Invoices_List.Rows[0]["Total_PriceWithoutTax"].ToString());

            Bills_List_Id = long.Parse(dt_Invoices_List.Rows[0]["Id"].ToString());

            Invoice_Net_Total_Old = decimal.Parse(dt_Invoices_List.Rows[0]["Net_Total"].ToString());
            Invoice_Total_Old = decimal.Parse(dt_Invoices_List.Rows[0]["Total"].ToString());
            Invoice_Total_PriceWithoutTax_Old = decimal.Parse(dt_Invoices_List.Rows[0]["Total_PriceWithoutTax"].ToString());

            try
            {
                Supplier_Notes = dt_Invoices_List.Rows[0]["Supplier_Notes"].ToString();
            }
            catch (Exception ee)
            {

            }
            try
            {
                ProcessNo = long.Parse(dt_Invoices_List.Rows[0]["ProcessNo"].ToString());
            }
            catch (Exception ee)
            {
            }
            try
            {
                Is_Invoice_Agile = int.Parse(dt_Invoices_List.Rows[0]["Is_Invoice_Agile"].ToString());
            }
            catch (Exception ee)
            {
            }
        }



        public long Insert_Bill_List_Reverse(ref AccountModelsLib.operations opp, ref int Excp)
        {
            return Insert_Bill_List(ref opp, ref Excp, "Bills_List_Reverse");
        }

        public void Update_Bill_List_Reverse(ref AccountModelsLib.operations opp, ref int Excp)
        {
            Update_Bill_List(ref opp, ref Excp, "Bills_List_Reverse");
        }
        public void Get_Invoices_List_Reverse(long Invoice_List_Table_Id)
        {
            int Excp = 0;
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            Get_Invoices_List_Reverse(ref opp, ref Excp, Invoice_List_Table_Id);

        }

        public void Get_Invoices_List_Reverse(ref AccountModelsLib.operations opp, ref int Excp, long Invoice_List_Table_Id)
        {

            try
            {
                Bills_List_Id = Invoice_List_Table_Id;

                opp.cmd.Parameters.Clear();
                string mysql = "";

                mysql = " Select * from  Bills_List_Reverse  ";
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



        long Insert_Bill_List(ref AccountModelsLib.operations opp, ref int Excp, string TableName)
        {
            Bills_List_Id = 0;
            try
            {
                Invoice_DateTime = Invoice_Date;
                Invoice_Date = Invoice_Date.Date;
                Equivalent_Net_Total_Price = Net_Total_Price;
                Equivalent_Net_Total_CommissionValue = Net_Total_CommissionValue;
                opp.cmd.Parameters.Clear();
                string mysql = "";
                //  Invoice_DateTime   Load_Payment,Second_Total,Damaghat,Load_Payment_OnTax  commission,CommissionValue
                mysql = " INSERT INTO " + TableName + " (Invoice_Id, Supplier_Id, Invoice_Date,Invoice_DateTime, Total, TaxSales10, ATS05,TaxSales10_Value,ATS05_Value , Net_Total , Currency,Currency_id,Invoice_Type,Supplier_BalanceSet,Notes,PolicaNo, TroodContainerNo, SupplierAccountBefor,  SupplierAccountAfter,SubAccount_Id,SubAccount_Name,User_Id,Branch_Id,Salesman_Id,   IS_Make_Sarf_Item_BalanceFromInvoice,  Is_InvoiceKta3y,OperationDate,Total_PriceWithoutTax,Total_Item_CommissionValue, List_CommissionValue,List_CommissionRate,List_Final_CommissionValue,Net_Total_CommissionValue,Equivalent_Net_Total_CommissionValue,Equivalent_Net_Total_Price,Is_Tax_Per_Item,Load_Payment,Damaghat,Is_Load_Payment_OnTax,Total_PriceWithoutTax_Load_Payment,PriceByTenns ,Equivalent_Total,ProcessNo,Supplier_Notes,Is_Invoice_Agile,Is_Invoice_AgileStr) ";
                mysql = mysql + " VALUES(@Invoice_Id ,@Supplier_Id ,@Invoice_Date,@Invoice_DateTime ,@Total ,@TaxSales10 ,@ATS05 ,@TaxSales10_Value ,@ATS05_Value ,@Net_Total ,@Currency  ,@Currency_Id ,@Invoice_Type ,@Customer_BalanceSet ,@Notes  ,@PolicaNo  ,@TroodContainerNo  ,@SupplierAccountBefor ,@SupplierAccountAfter ,@SubAccount_Id ,@SubAccount_Name  ,@User_Id ,@Branch_Id  ,@Salesman_Id ,@IS_Make_Sarf_Item_BalanceFromInvoice ,@Is_InvoiceKta3y  ,@OperationDate ,@Total_PriceWithoutTax ,@Total_Item_CommissionValue,@List_CommissionValue,@List_CommissionRate,@List_Final_CommissionValue,@Net_Total_CommissionValue ,@Equivalent_Net_Total_CommissionValue,@Equivalent_Net_Total_Price,@Is_Tax_Per_Item,@Load_Payment,@Damaghat,@Is_Load_Payment_OnTax,@Total_PriceWithoutTax_Load_Payment,@PriceByTenns,@Equivalent_Total,@ProcessNo,@Supplier_Notes,@Is_Invoice_Agile,@Is_Invoice_AgileStr)";

                ////Load_Payment,Second_Total,Damaghat,Load_Payment_OnTax
                //string mysql = " INSERT INTO Bills_List (Invoice_Id, Supplier_Id, Invoice_Date, Total, , TaxSales10, ATS05,TaxSales10_Value,ATS05_Value , Net_Total , Currency,Currency_id,Invoice_Type,Supplier_BalanceSet,Notes,PolicaNo, TroodContainerNo,Load_Payment,Second_Total,Damaghat,Load_Payment_OnTax,SupplierAccountBefor, SupplierAccountAfter,User_Id,Branch_Id,    SubAccount_Id,   SubAccount_Name,    IS_Make_Sarf_Item_BalanceFromInvoice,   Is_InvoiceKta3y,OperationDate) ";
                //mysql = mysql + " VALUES (" + Invoice_Id + "," + Supplier_Id + ",'" + AccountModelsLib.operations.dateFormat(Invoice_Date) + "'," + Total + "," + commission + "," + CommissionValue + "," + TaxSales10 + "," + ATS05 + "," + TaxSales10_Value + "," + ATS05_Value + "," + Net_Total + ",'" + Currency + "'," + Currency_Id + "," + Invoice_Type + "," + Customer_BalanceSet + ",'" + Notes + "','" + PolicaNo + "',  '" + TroodContainerNo + "'," + LoadPayment + "," + TotalSecond + "," + Damaghat + "," + Load_Payment_OnTax + "," + SupplierAccountBefor + "," + SupplierAccountAfter + "," + Common.User_Id + "," + Common.Branch_Id + "," + SubAccount_Id + ",'" + SubAccount_Name + "', " + IS_Make_Sarf_Item_BalanceFromInvoice + "," + Is_InvoiceKta3y + "," + AccountModelsLib.operations.Get_DateTimeToInsert(DateTime.Now) + ")";

                opp.cmd.Parameters.AddWithValue("@Invoice_Id", Invoice_No);
                opp.cmd.Parameters.AddWithValue("@Supplier_Id", Supplier_Id);
                opp.cmd.Parameters.AddWithValue("@Invoice_Date", Invoice_Date);
                opp.cmd.Parameters.AddWithValue("@Invoice_DateTime", Invoice_DateTime);

                opp.cmd.Parameters.AddWithValue("@TaxSales10", TaxSales10);
                opp.cmd.Parameters.AddWithValue("@ATS05", ATS05);

                opp.cmd.Parameters.AddWithValue("@TaxSales10_Value", TaxSales10_Value);
                opp.cmd.Parameters.AddWithValue("@ATS05_Value", ATS05_Value);

                opp.cmd.Parameters.AddWithValue("@Currency", Currency);
                opp.cmd.Parameters.AddWithValue("@Currency_id", Currency_Id);
                opp.cmd.Parameters.AddWithValue("@Invoice_Type", Invoice_Type);

                opp.cmd.Parameters.AddWithValue("@Customer_BalanceSet", Supplier_BalanceSet);
                opp.cmd.Parameters.AddWithValue("@Notes", Notes);

                opp.cmd.Parameters.AddWithValue("@PolicaNo", PolicaNo);
                opp.cmd.Parameters.AddWithValue("@TroodContainerNo", TroodContainerNo);

                opp.cmd.Parameters.AddWithValue("@SupplierAccountBefor", SupplierAccountBefor);
                opp.cmd.Parameters.AddWithValue("@SupplierAccountAfter", SupplierAccountAfter);


                opp.cmd.Parameters.AddWithValue("@SubAccount_Id", SubAccount_Id);
                opp.cmd.Parameters.AddWithValue("@SubAccount_Name", SubAccount_Name);


                opp.cmd.Parameters.AddWithValue("@User_Id", Common.User_Id);
                opp.cmd.Parameters.AddWithValue("@Branch_Id", Common.Branch_Id);
                opp.cmd.Parameters.AddWithValue("@OperationDate", DateTime.Now);
                //opp.cmd.Parameters.AddWithValue("@AnotherPayment", AnotherPayment);
                opp.cmd.Parameters.AddWithValue("@Salesman_Id", Salesman_Id);

                opp.cmd.Parameters.AddWithValue("@IS_Make_Sarf_Item_BalanceFromInvoice", IS_Make_Sarf_Item_BalanceFromInvoice);
                opp.cmd.Parameters.AddWithValue("@Is_InvoiceKta3y", Is_InvoiceKta3y);

                opp.cmd.Parameters.AddWithValue("@Total_Item_CommissionValue", Total_Item_CommissionValue);
                opp.cmd.Parameters.AddWithValue("@List_CommissionValue", List_CommissionValue);

                opp.cmd.Parameters.AddWithValue("@List_CommissionRate", List_CommissionRate);
                opp.cmd.Parameters.AddWithValue("@List_Final_CommissionValue", List_Final_CommissionValue);
                opp.cmd.Parameters.AddWithValue("@Net_Total_CommissionValue", Net_Total_CommissionValue);
                opp.cmd.Parameters.AddWithValue("@Is_Tax_Per_Item", Is_Tax_Per_Item);
                opp.cmd.Parameters.AddWithValue("@Equivalent_Net_Total_CommissionValue", Equivalent_Net_Total_CommissionValue);



                opp.cmd.Parameters.AddWithValue("@Load_Payment", Load_Payment);

                opp.cmd.Parameters.AddWithValue("@Damaghat", Damaghat);
                opp.cmd.Parameters.AddWithValue("@Is_Load_Payment_OnTax", Is_Load_Payment_OnTax);
                opp.cmd.Parameters.AddWithValue("@PriceByTenns", PriceByTenns);
                opp.cmd.Parameters.AddWithValue("@Total_PriceWithoutTax_Load_Payment", Total_PriceWithoutTax_Load_Payment);
                opp.cmd.Parameters.AddWithValue("@Total_PriceWithoutTax", Total_PriceWithoutTax);
                opp.cmd.Parameters.AddWithValue("@Total", Total_Qnty_Value);
                opp.cmd.Parameters.AddWithValue("@Net_Total", Net_Total_Price);

                opp.cmd.Parameters.AddWithValue("@Equivalent_Total", Total_Qnty_Value);
                opp.cmd.Parameters.AddWithValue("@Equivalent_Net_Total_Price", Equivalent_Net_Total_Price);
                opp.cmd.Parameters.AddWithValue("@ProcessNo", ProcessNo);
                opp.cmd.Parameters.AddWithValue("@Supplier_Notes", Supplier_Notes);
                opp.cmd.Parameters.AddWithValue("@Is_Invoice_Agile", Is_Invoice_Agile);
                opp.cmd.Parameters.AddWithValue("@Is_Invoice_AgileStr", Is_Invoice_AgileStr);


                Bills_List_Id = opp.Insert_me_Return_Last_Identify(mysql);
                Excp = opp.Excp == 0 ? 0 : 1;

                return Bills_List_Id;
            }
            catch (Exception ee)
            {

                Excp = 1;
                return Bills_List_Id;
            }
        }
        void Update_Bill_List(ref AccountModelsLib.operations opp, ref int Excp, string TableName)
        {

            try
            {
                Invoice_DateTime = Invoice_Date;
                Invoice_Date = Invoice_Date.Date;
                Equivalent_Net_Total_Price = Net_Total_Price;
                Equivalent_Net_Total_CommissionValue = Net_Total_CommissionValue;

                opp.cmd.Parameters.Clear();
                string mysql = "";

                mysql = " Update " + TableName + "  Set Invoice_Id=@Invoice_Id, Supplier_Id=@Supplier_Id, Invoice_Date=@Invoice_Date," +
                    "Invoice_DateTime=@Invoice_DateTime  , Total=@Total  , TaxSales10=@TaxSales10 , ATS05=@ATS05" +
                    " ,TaxSales10_Value=@TaxSales10_Value ,ATS05_Value =@ATS05_Value , Net_Total =@Net_Total , Currency=@Currency ," +
                    "Currency_id=@Currency_Id ,Invoice_Type=@Invoice_Type ,Supplier_BalanceSet= @Supplier_BalanceSet ,Notes=@Notes ,PolicaNo=@PolicaNo ," +
                    " TroodContainerNo=@TroodContainerNo ,SupplierAccountBefor=@SupplierAccountBefor , SupplierAccountAfter=@SupplierAccountAfter ,SubAccount_Id=@SubAccount_Id ," +
                    "SubAccount_Name=@SubAccount_Name   ,Salesman_Id=@Salesman_Id ,   IS_Make_Sarf_Item_BalanceFromInvoice=@IS_Make_Sarf_Item_BalanceFromInvoice ," +
                    "  Is_InvoiceKta3y=@Is_InvoiceKta3y ,OperationDate=@OperationDate ,Total_PriceWithoutTax=@Total_PriceWithoutTax ," +
                    "Total_Item_CommissionValue=@Total_Item_CommissionValue , List_CommissionValue=@List_CommissionValue ," +
                    "List_CommissionRate=@List_CommissionRate ,List_Final_CommissionValue=@List_Final_CommissionValue ,Net_Total_CommissionValue=@Net_Total_CommissionValue ," +
                    "Equivalent_Net_Total_CommissionValue=@Equivalent_Net_Total_CommissionValue ,Equivalent_Net_Total_Price=@Equivalent_Net_Total_Price ," +
                    "Is_Tax_Per_Item =@Is_Tax_Per_Item,Edit_User_Id=@Edit_User_Id,EditOperationDate=@EditOperationDate," +
                    " Load_Payment=@Load_Payment, Damaghat=@Damaghat,   Is_Load_Payment_OnTax=@Is_Load_Payment_OnTax,Total_PriceWithoutTax_Load_Payment=@Total_PriceWithoutTax_Load_Payment,PriceByTenns=@PriceByTenns , " +
                    " Equivalent_Total=@Equivalent_Total,ProcessNo=@ProcessNo,Supplier_Notes=@Supplier_Notes";



                mysql = mysql + "   Where Id=@Bill_List_Table_Id";



                opp.cmd.Parameters.AddWithValue("@Bill_List_Table_Id", Bills_List_Id);
                opp.cmd.Parameters.AddWithValue("@Invoice_Id", Invoice_No);
                opp.cmd.Parameters.AddWithValue("@Supplier_Id", Supplier_Id);
                opp.cmd.Parameters.AddWithValue("@Invoice_Date", Invoice_Date);
                opp.cmd.Parameters.AddWithValue("@Invoice_DateTime", Invoice_DateTime);

                opp.cmd.Parameters.AddWithValue("@TaxSales10", TaxSales10);
                opp.cmd.Parameters.AddWithValue("@ATS05", ATS05);

                opp.cmd.Parameters.AddWithValue("@TaxSales10_Value", TaxSales10_Value);
                opp.cmd.Parameters.AddWithValue("@ATS05_Value", ATS05_Value);

                opp.cmd.Parameters.AddWithValue("@Currency", Currency);
                opp.cmd.Parameters.AddWithValue("@Currency_id", Currency_Id);
                opp.cmd.Parameters.AddWithValue("@Invoice_Type", Invoice_Type);

                opp.cmd.Parameters.AddWithValue("@Supplier_BalanceSet", Supplier_BalanceSet);
                opp.cmd.Parameters.AddWithValue("@Notes", Notes);

                opp.cmd.Parameters.AddWithValue("@PolicaNo", PolicaNo);
                opp.cmd.Parameters.AddWithValue("@TroodContainerNo", TroodContainerNo);

                opp.cmd.Parameters.AddWithValue("@SupplierAccountBefor", SupplierAccountBefor);
                opp.cmd.Parameters.AddWithValue("@SupplierAccountAfter", SupplierAccountAfter);


                opp.cmd.Parameters.AddWithValue("@SubAccount_Id", SubAccount_Id);
                opp.cmd.Parameters.AddWithValue("@SubAccount_Name", SubAccount_Name);



                opp.cmd.Parameters.AddWithValue("@OperationDate", DateTime.Now);
                //opp.cmd.Parameters.AddWithValue("@AnotherPayment", AnotherPayment);
                opp.cmd.Parameters.AddWithValue("@Salesman_Id", Salesman_Id);

                opp.cmd.Parameters.AddWithValue("@IS_Make_Sarf_Item_BalanceFromInvoice", IS_Make_Sarf_Item_BalanceFromInvoice);
                opp.cmd.Parameters.AddWithValue("@Is_InvoiceKta3y", Is_InvoiceKta3y);

                opp.cmd.Parameters.AddWithValue("@Total_Item_CommissionValue", Total_Item_CommissionValue);
                opp.cmd.Parameters.AddWithValue("@List_CommissionValue", List_CommissionValue);

                opp.cmd.Parameters.AddWithValue("@List_CommissionRate", List_CommissionRate);
                opp.cmd.Parameters.AddWithValue("@List_Final_CommissionValue", List_Final_CommissionValue);
                opp.cmd.Parameters.AddWithValue("@Net_Total_CommissionValue", Net_Total_CommissionValue);
                opp.cmd.Parameters.AddWithValue("@Is_Tax_Per_Item", Is_Tax_Per_Item);
                opp.cmd.Parameters.AddWithValue("@Equivalent_Net_Total_CommissionValue", Equivalent_Net_Total_CommissionValue);



                opp.cmd.Parameters.AddWithValue("@Total_PriceWithoutTax", Total_PriceWithoutTax);
                opp.cmd.Parameters.AddWithValue("@Total", Total_Qnty_Value);
                opp.cmd.Parameters.AddWithValue("@Net_Total", Net_Total_Price);

                opp.cmd.Parameters.AddWithValue("@Equivalent_Net_Total_Price", Equivalent_Net_Total_Price);
                opp.cmd.Parameters.AddWithValue("@Equivalent_Total", Total_Qnty_Value);

                // Supplier_Id   SupplierAccountBefor SupplierAccountAfter
                //  Load_Payment Damaghat   Load_Payment_OnTax Second_Total
                opp.cmd.Parameters.AddWithValue("@Load_Payment", Load_Payment);
                opp.cmd.Parameters.AddWithValue("@Damaghat", Damaghat);
                opp.cmd.Parameters.AddWithValue("@Is_Load_Payment_OnTax", Is_Load_Payment_OnTax);
                opp.cmd.Parameters.AddWithValue("@PriceByTenns", PriceByTenns);
                opp.cmd.Parameters.AddWithValue("@Total_PriceWithoutTax_Load_Payment", Total_PriceWithoutTax_Load_Payment);
                // Is_Load_Payment_OnTax,Total_PriceWithoutTax_Load_Payment,PriceByTenns
                opp.cmd.Parameters.AddWithValue("@Edit_User_Id", Common.User_Id);
                opp.cmd.Parameters.AddWithValue("@EditOperationDate", DateTime.Now);
                opp.cmd.Parameters.AddWithValue("@ProcessNo", ProcessNo);
                opp.cmd.Parameters.AddWithValue("@Supplier_Notes", Supplier_Notes);



                opp.insert_me(mysql);
                Excp = 0;

            }
            catch (Exception ee)
            {

                Excp = 1;

            }
        }
    }
}
