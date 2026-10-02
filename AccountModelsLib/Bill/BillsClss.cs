using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using Microsoft.Data.SqlClient;
using System.Data.OleDb;
 
 
using AccountModelsLib.Models;
 
 
 
using System.Linq;
 
namespace AccountModelsLib
{
    /// <summary>
    /// Summary description for InvoicesClss.
    /// </summary>
    public class BillsClss: Bills_ModelsClss
    {
        //long New_Invoice_Id;
        public Int64 New_Invoices_No;   // —ﬁ„ «·›« Ê—Â «·ÃœÌœ…
        public long New_Bills_List_Id;
        public int Invoice_Type;  ///Invoice_Type=0 if ﬁÿ«⁄ ⁄«„   or =1 if    ﬁÿ«⁄ Œ«’ 
        public ArrayList arr_Order_Purshing_Id_Automatic; //·Õ›Ÿ —ﬁ„ «·«·Ï ·«’œ«— «·«–‰ ÊÌ” Œœ„ ··ÿ»«⁄…
        public BillsClss()
        {
            New_Invoices_No = 0;
            Invoice_Type = 0;
            //
            // TODO: Add constructor logic here
            //
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Invoice_Types">///Invoice_Type=0 if ﬁÿ«⁄ ⁄«„   or =1 if    ﬁÿ«⁄ Œ«’ </param>
        public BillsClss(int Invoice_Types)
        {
            New_Invoices_No = 0;
            Invoice_Type = Invoice_Types;
            //
            // TODO: Add constructor logic here
            //
        }
         
        public void Make_Bills_List_Reverse_Update_SupplierAccount_POS(ref int Excp,ref string Msg, DataSet Invoice_ds, long Invoice_No, DateTime Invoice_Date, long Supplier_Id, long Currency_Id, decimal Net_Total, decimal Commission, int Invoice_Type, string PolicaNo, string TroodContainerNo, string Notes, decimal LoadPayment, decimal TotalSecond, decimal ATS05, decimal ATS05_Value, decimal TaxSales10, decimal TaxSales10_Value, decimal Damaghat, int Load_Payment_OnTax, ArrayList mArrMake_Order)
        {
            long New_Invoice_Id = 0;
            //checked item_report to SendKeys to store
            //DataView dvToStore = new DataView();
            //dvToStore.Table = Invoice_ds.Tables[0];
            //Requests.ItemToStore it = new Requests.ItemToStore(dvToStore);
            //it.ShowDialog();
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




            for (int x = 0; x < Invoice_ds.Tables[0].Rows.Count; x++)
            {
                Total = Total + decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Total_Price"].ToString());
                //TaxSales10_Value = TaxSales10_Value + decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Total_Price"].ToString());
                // ATS05_Value = ATS05_Value + decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Total_Price"].ToString());
            }

            InvoicesClss inv = new InvoicesClss();

            Make_New_Bill_List_Id(ref Excp, ref New_Invoice_Id, Invoice_No, Supplier_Id, Invoice_Date, Invoice_Type, TotalSecond, Commission, 0, TaxSales10, ATS05, TaxSales10_Value, ATS05_Value, Net_Total, "", Currency_Id, 1, Notes, PolicaNo, TroodContainerNo, LoadPayment, TotalSecond, Damaghat, Load_Payment_OnTax, 0, 0, 0, "");

            // Make_New_Bills_List_Reverse_Id(ref Excp, ref  New_Invoice_Id, Invoice_No, Supplier_Id, Invoice_Date, Invoice_Type, TotalSecond, Commission, TaxSales10, ATS05, TaxSales10_Value, ATS05_Value, Net_Total, "", Currency_Id, 1, Notes, PolicaNo, TroodContainerNo, LoadPayment, TotalSecond, Damaghat, Load_Payment_OnTax);
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


            ///// Requests_Details
            decimal Export_Qnty_Larg;
            decimal Export_Qnty_Small;
            decimal Export_Item_No;
            int Requests_Details_id;

            //////////// add item in invoice details
            for (int x = 0; x < Invoice_ds.Tables[0].Rows.Count; x++)
            {
                TaxSales10_Value = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["TaxSales10_Value"].ToString());
                ATS05_Value = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["ATS05_Value"].ToString());

                Item_Id = long.Parse(Invoice_ds.Tables[0].Rows[x]["Item_id"].ToString());
                Larg_Qnty = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Larg_Qnty"].ToString());
                Small_Qnty = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Small_Qnty"].ToString());
                Qnty_Another = Invoice_ds.Tables[0].Rows[x]["Qnty_Another"].ToString();
                Larg_Unit_Price = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Unit_price"].ToString());
                Small_Unit_Price = 0;
                Qnty_value = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Total_Price"].ToString());
                Total_Value = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Net_Total_Price"].ToString());
                ATS05_ = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["ATS05"].ToString());
                TaxSales10_ = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["TaxSales"].ToString());
                Unit_Used_for_Price = 0;
                unit_id = long.Parse(Invoice_ds.Tables[0].Rows[x]["unit_id"].ToString());
                Unit_Item_Table_Id = long.Parse(Invoice_ds.Tables[0].Rows[x]["Unit_Item_Table_Id"].ToString());
                code = Invoice_ds.Tables[0].Rows[x]["Code"].ToString();
                Notes_ = "";
                Balance_Checked = 1;




                string NameSpecification = Invoice_ds.Tables[0].Rows[x]["NameSpecification"].ToString();

                decimal Item_Net_Total_Price = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Item_Net_Total_Price"].ToString());
                decimal Item_Total_WithoutTax = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Item_Total_WithoutTax"].ToString());
                string QntyStr = Invoice_ds.Tables[0].Rows[x]["QntyStr"].ToString();
                string QntyStrColumn = Invoice_ds.Tables[0].Rows[x]["QntyStrColumn"].ToString();
                decimal Item_TaxSales10_Value = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Item_TaxSales10_Value"].ToString());
                long Store_Id = long.Parse(Invoice_ds.Tables[0].Rows[x]["Store_Id"].ToString());



                //≈÷«›… ’‰› ›Ï ›« Ê—…
                mysql = " INSERT INTO Bills_Details (Invoice_Id, Item_Id, Larg_Qnty, Small_Qnty,Qnty_Another, Larg_Unit_Price, Small_Unit_Price, Qnty_value,Total, ATS05, TaxSales10,TaxSales10_Value,ATS05_Value,Unit_Used_for_Price,unit_id,Unit_Item_Table_Id,code, Notes,Make_Order,NameSpecification,  Item_Net_Total_Price, Item_Total_WithoutTax, QntyStr,     QntyStrColumn,    Item_TaxSales10_Value,Store_Id) ";
                mysql = mysql + " VALUES (" + Invoice_Id + "," + Item_Id + "," + Larg_Qnty + "," + Small_Qnty + ",'" + Qnty_Another + "'," + Larg_Unit_Price + "," + Small_Unit_Price + "," + Qnty_value + "," + Total_Value + "," + ATS05_ + "," + TaxSales10_ + "," + TaxSales10_Value + "," + ATS05_Value + "," + Unit_Used_for_Price + "," + unit_id + "," + Unit_Item_Table_Id + ",'" + code + "','" + Notes_ + "'," + mArrMake_Order[x] + ", '" + NameSpecification + "', " + Item_Net_Total_Price + "," + Item_Total_WithoutTax + ", '" + QntyStr + "',  '" + QntyStrColumn + "',    " + Item_TaxSales10_Value + "," + Store_Id + ") ";
                arr.Add(mysql);
            }

            ////////// set the work on database
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            int Status = opp.insert_me_transaction(arr);
            // CustomersAccount ca = new CustomersAccount();
            // ca.Set_Customer_Balance(ref Excp, Customer_Id, Invoice_Id, Net_Total);

            //SuppliersAccount ca = new SuppliersAccount();

            //ca.Set_SuppliersAccount_Balance(ref Excp, ref opp, Supplier_Id, New_Invoice_Id, Net_Total);

            //if (Status == 1)
            //{
            //    Excp = 0;

            //}
            //else
            //{
            //    Excp = 1;
            //}

            ////////// set the work on database

            Suppliers_AccountsCls ca = new Suppliers_AccountsCls();
            ca.Set_Supplier_Balance_MakeBill_Reverse(ref Excp, ref opp, Supplier_Id, New_Invoice_Id, Net_Total, Notes);

            ////////// order estlam automatic
            if (Common.Make_Store_Order_Sarf_Estlam_Automatic_From_Invoice == 1)
            {
                Order_Details_AutomaticCls oo = new Order_Details_AutomaticCls();
                oo.Make_Order_From_Bills_Details_Reverse(ref Excp, ref opp, Invoice_ds.Tables[0], New_Invoice_Id, Supplier_Id, Invoice_Date, "›« Ê—… „— Ã⁄ „‘ —Ì« ");
                Msg = " „ ≈’œ«— ≈–‰ «·’—› („— Ã⁄ ≈” ·«„) ≈·Ï «·„Ê—œ";
            }

            //«‰‘«¡ «·ﬁÌœ
            
            Account_Entry_Invoice_CLS aa = new Account_Entry_Invoice_CLS();
            aa.IsNotDeletePrevEntry = true;
            aa.Make_EntryFor_Bills_Reverse(ref Excp, ref Msg, ref opp, New_Invoice_Id, Invoice_Date, Invoice_Date, -1);

            if (Excp == 0)
            {
                opp.Commit();
            }
            else
            {
                Excp = 1;
                opp.Rollback();
            }
            Excp = 0;

        }
         
        public void Make_Bill_Update_SupplierAccount_POS(ref int Excp,ref string Msg, DataSet Invoice_ds, long Invoice_No, DateTime Invoice_Date, long Supplier_Id, long Currency_Id, decimal Net_Total, decimal Commission, decimal CommissionValue, int Invoice_Type, string PolicaNo, string TroodContainerNo, string Notes, decimal LoadPayment, decimal TotalSecond, decimal ATS05, decimal ATS05_Value, decimal TaxSales10, decimal TaxSales10_Value, decimal Damaghat, int Load_Payment_OnTax, decimal SupplierAccountBefor, decimal SupplierAccountAfter, long SubAccount_Id, string SubAccount_Name, int IS_Make_Sarf_Item_BalanceFromInvoice, int Is_InvoiceKta3y, ArrayList mArrMake_Order)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations(1);
            try
            {
                //checked item_report to SendKeys to store
                //DataView dvToStore = new DataView();
                //dvToStore.Table = Invoice_ds.Tables[0];
                //Requests.ItemToStore it = new Requests.ItemToStore(dvToStore);
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




                //for (int x = 0; x < Invoice_ds.Tables[0].Rows.Count; x++)
                //{
                //    Total = Total + decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Total_Price"].ToString());
                // }
                //Total = Common.RoundMe(Total);
                InvoicesClss inv = new InvoicesClss();

                Make_New_Bill_List_Id(ref Excp, ref opp, ref New_Bills_List_Id, Invoice_No, Supplier_Id, Invoice_Date, Invoice_Type, TotalSecond, Commission, CommissionValue, TaxSales10, ATS05, TaxSales10_Value, ATS05_Value, Net_Total, "", Currency_Id, 1, Notes, PolicaNo, TroodContainerNo, LoadPayment, TotalSecond, Damaghat, Load_Payment_OnTax, SupplierAccountBefor, SupplierAccountAfter, SubAccount_Id, SubAccount_Name, IS_Make_Sarf_Item_BalanceFromInvoice, Is_InvoiceKta3y);



                /////////////////Set All Commision
                decimal Total_Item_CommissionValue = 0;
                try
                {

                    for (int x = 0; x < Invoice_ds.Tables[0].Rows.Count; x++)
                    {
                        Total_Item_CommissionValue = Total_Item_CommissionValue + decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Item_CommissionValue"].ToString());
                    }

                }
                catch (Exception ee)
                {
                    Total_Item_CommissionValue = 0;


                }
                decimal Total_CommissionValue = Total_Item_CommissionValue + CommissionValue;
                mysql = " UPDATE Bills_List SET Total_Item_CommissionValue=" + Total_Item_CommissionValue + ",Total_CommissionValue=    " + Total_CommissionValue + " WHERE (id = " + New_Bills_List_Id + ") ";
                opp.insert_me(mysql);
                ///////////////////////////////////////////////////////////////////////////////



                ///////////// add invoice Items and update request qnty


                long Invoice_Id = New_Bills_List_Id;
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
                long Unit_Item_Table_Id;
                string code;
                string Notes_;
                int Balance_Checked;
                long Store_Id = 0;

                ///// Requests_Details
                decimal Export_Qnty_Larg;
                decimal Export_Qnty_Small;
                decimal Export_Item_No;
                int Requests_Details_id;
                string Store_Name = "";
                string Supplier_Name = "";

                try
                {

                    Supplier_Name = Common.GetSupplierName(ref opp, Supplier_Id);
                }
                catch { }

                InvoiceDataSetCLS ii = new InvoiceDataSetCLS();
                //////////// add item in invoice details
                for (int x = 0; x < Invoice_ds.Tables[0].Rows.Count; x++)
                {
                    decimal LargItemsQnty_Free = 0;
                    decimal SmallItemsQnty_Free = 0;
                    DateTime Item_Expire_Date = new DateTime(2023, 1, 1);
                    decimal Item_No_Free = 0;
                    decimal Audience_price = 0;

                    int Is_Item_CommissionValue = 0;
                    decimal Item_CommissionValue = 0;
                    decimal Unit_CommissionValue = 0;
                    decimal Item_CommissionRate = 0;
                    decimal Unit_CommissionRate = 0;
                    int Is_Alternate_Item_CommissionValue = 0;
                    decimal Alternate_Item_CommissionValue = 0;
                    decimal Alternate_Unit_CommissionValue = 0;
                    decimal Alternate_Item_CommissionRate = 0;
                    decimal Alternate_Unit_CommissionRate = 0;



                    TaxSales10_Value = 0;
                    ATS05_Value = 0;
                    Store_Id = long.Parse(Invoice_ds.Tables[0].Rows[x]["Store_Id"].ToString());
                    Item_Id = long.Parse(Invoice_ds.Tables[0].Rows[x]["Item_id"].ToString());
                    Larg_Qnty = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Larg_Qnty"].ToString());
                    Small_Qnty = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Small_Qnty"].ToString());
                    Qnty_Another = Invoice_ds.Tables[0].Rows[x]["Qnty_Another"].ToString();
                    Larg_Unit_Price = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Larg_Unit_Price"].ToString());
                    Small_Unit_Price = 0;
                    Qnty_value = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Qnty_value"].ToString());
                    Total_Value = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Item_Net_Total_Price"].ToString());
                    ATS05_ = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["ATS05"].ToString());
                    TaxSales10_ = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["TaxSales10"].ToString());
                    Unit_Used_for_Price = 0;
                    unit_id = long.Parse(Invoice_ds.Tables[0].Rows[x]["unit_id"].ToString());
                    Unit_Item_Table_Id = long.Parse(Invoice_ds.Tables[0].Rows[x]["Unit_Item_Table_Id"].ToString());
                    code = Invoice_ds.Tables[0].Rows[x]["Code"].ToString();
                    Notes_ = "";
                    Balance_Checked = 1;
                    TaxSales10_Value = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["TaxSales10_Value"].ToString());
                    ATS05_Value = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["ATS05_Value"].ToString());


                    //StatementNo OperationNo Merchant_Name Tash3ela  LargItemsQnty_Free SmallItemsQnty_Free Item_Expire_Date
                    string StatementNo = Invoice_ds.Tables[0].Rows[x]["StatementNo"].ToString();
                    string OperationNo = Invoice_ds.Tables[0].Rows[x]["OperationNo"].ToString();
                    string Merchant_Name = Invoice_ds.Tables[0].Rows[x]["Merchant_Name"].ToString();
                    string Tash3ela = Invoice_ds.Tables[0].Rows[x]["Tash3ela"].ToString();
                    try
                    {
                        LargItemsQnty_Free = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["LargItemsQnty_Free"].ToString());
                    }
                    catch
                    {

                    }
                    try
                    {
                        SmallItemsQnty_Free = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["SmallItemsQnty_Free"].ToString());
                    }
                    catch
                    {
                    }
                    try
                    {
                        Item_Expire_Date = DateTime.Parse(Invoice_ds.Tables[0].Rows[x]["Item_Expire_Date"].ToString());
                    }
                    catch
                    {
                    }

                    try
                    {
                        Item_No_Free = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Item_No_Free"].ToString());
                    }
                    catch
                    {
                    }
                    try
                    {
                        Audience_price = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Audience_price"].ToString());
                        Is_Item_CommissionValue = int.Parse(Invoice_ds.Tables[0].Rows[x]["Is_Item_CommissionValue"].ToString());
                        Item_CommissionValue = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Item_CommissionValue"].ToString());
                        Unit_CommissionValue = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Unit_CommissionValue"].ToString());
                        Item_CommissionRate = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Item_CommissionRate"].ToString());
                        Unit_CommissionRate = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Unit_CommissionRate"].ToString());
                        Is_Alternate_Item_CommissionValue = int.Parse(Invoice_ds.Tables[0].Rows[x]["Is_Alternate_Item_CommissionValue"].ToString());
                        Alternate_Item_CommissionValue = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Alternate_Item_CommissionValue"].ToString());
                        Alternate_Unit_CommissionValue = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Alternate_Unit_CommissionValue"].ToString());
                        Alternate_Item_CommissionRate = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Alternate_Item_CommissionRate"].ToString());
                        Alternate_Unit_CommissionRate = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Alternate_Unit_CommissionRate"].ToString());

                    }
                    catch (Exception ee)
                    {
                    }


                    string NameSpecification = Invoice_ds.Tables[0].Rows[x]["Item_Name"].ToString() + "  " + Invoice_ds.Tables[0].Rows[x]["Specification"].ToString();
                    string Invoice_Item_Name = Invoice_ds.Tables[0].Rows[x]["Item_Name"].ToString();
                    string Specification = Invoice_ds.Tables[0].Rows[x]["Specification"].ToString();
                    decimal Item_Net_Total_Price = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Item_Net_Total_Price"].ToString());
                    decimal Item_Total_WithoutTax = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Item_Total_WithoutTax"].ToString());
                    string QntyStr = Invoice_ds.Tables[0].Rows[x]["QntyStr"].ToString();
                    string QntyStrColumn = Invoice_ds.Tables[0].Rows[x]["QntyStrColumn"].ToString();
                    decimal Item_TaxSales10_Value = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Item_TaxSales10_Value"].ToString());

                    //≈÷«›… ’‰› ›Ï ›« Ê—…
                    mysql = " INSERT INTO Bills_Details ( Audience_price,    Item_No_Free,StatementNo, OperationNo, Merchant_Name ,Tash3ela , LargItemsQnty_Free ,SmallItemsQnty_Free, Item_Expire_Date,Store_Id,Invoice_Item_Name,Specification, NameSpecification,Invoice_Id, Item_Id, Larg_Qnty, Small_Qnty,Qnty_Another,Larg_Qnty_Balance,Small_Qnty_Balance,Qnty_Another_Balance ,Larg_Unit_Price, Small_Unit_Price, Qnty_value,Total, ATS05, TaxSales10,TaxSales10_Value,ATS05_Value,Unit_Used_for_Price,unit_id,Unit_Item_Table_Id,code, Notes,Make_Order,  Is_Item_CommissionValue,Item_CommissionValue,Unit_CommissionValue,Item_CommissionRate,Unit_CommissionRate,Is_Alternate_Item_CommissionValue, Alternate_Item_CommissionValue, Alternate_Unit_CommissionValue, Alternate_Item_CommissionRate, Alternate_Unit_CommissionRate, Item_Net_Total_Price, Item_Total_WithoutTax, QntyStr,     QntyStrColumn,    Item_TaxSales10_Value) ";
                    mysql = mysql + " VALUES ('" + Audience_price + "' ,  " + Item_No_Free + ",'" + StatementNo + "','" + OperationNo + "','" + Merchant_Name + "','" + Tash3ela + "', " + LargItemsQnty_Free + " , " + SmallItemsQnty_Free + " ,'" + AccountModelsLib.operations.dateFormat(Item_Expire_Date) + "' , " + Store_Id + ",N'" + Invoice_Item_Name + "',N'" + Specification + "',N'" + NameSpecification + "'," + Invoice_Id + "," + Item_Id + "," + Larg_Qnty + "," + Small_Qnty + ",'" + Qnty_Another + "', " + Larg_Qnty + "," + Small_Qnty + ",'" + Qnty_Another + "'," + Larg_Unit_Price + "," + Small_Unit_Price + "," + Qnty_value + "," + Total_Value + "," + ATS05_ + "," + TaxSales10_ + "," + TaxSales10_Value + "," + ATS05_Value + "," + Unit_Used_for_Price + "," + unit_id + "," + Unit_Item_Table_Id + ",'" + code + "','" + Notes_ + "'," + mArrMake_Order[x] + ",    " + Is_Item_CommissionValue + "," + Item_CommissionValue + "," + Unit_CommissionValue + "," + Item_CommissionRate + "," + Unit_CommissionRate + "," + Is_Alternate_Item_CommissionValue + "," + Alternate_Item_CommissionValue + "," + Alternate_Unit_CommissionValue + "," + Alternate_Item_CommissionRate + "," + Alternate_Unit_CommissionRate + "," + Item_Net_Total_Price + "," + Item_Total_WithoutTax + ", '" + QntyStr + "',  '" + QntyStrColumn + "',    " + Item_TaxSales10_Value + ") ";

                    long Invoices_Details_Id = opp.Insert_me_Return_Last_Identify(mysql);
                    Invoice_ds.Tables[0].Rows[x]["Invoices_Details_Id"] = Invoices_Details_Id;
                    long Unit_Item_ExpireDate_Id = 0;
                    int Transaction_Type = 1;
                    int Invoice_Type_Id = Common.Bill_Type_Id;// 3;





                    if (Common.Project_Market == 1 & IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                    {

                        long Invoices_Details_Id_ = Invoices_Details_Id;
                        //InvoiceDataSetCLS ii = new InvoiceDataSetCLS();
                        try
                        {
                            Store_Name = Common.GetStoreName(ref opp, Store_Id);
                        }
                        catch { }

                        int Type_From = Common.Supplier_StoreTransactionCode;
                        int Type_To = Common.Store_StoreTransactionCode;
                        long Id_From = Supplier_Id;
                        long Id_To = Store_Id;
                        long StoreId = Store_Id;
                        string StoreId_Name = Store_Name;
                        // , Type_From,  Type_To,  Id_From,  Id_To,  StoreId,  StoreId_Name
                        ii.Set_ItemTransactionFromInvoices(ref opp, Unit_Item_Table_Id, Larg_Unit_Price, Total_Value, Larg_Qnty, Small_Qnty, decimal.Parse(Qnty_Another), 0, Transaction_Type, Invoice_Date, Invoice_Id, Invoices_Details_Id_, Invoice_Type_Id, "›« Ê—… „‘ —Ì« ", Supplier_Name, Store_Name, Supplier_Name, Invoice_No, Type_From, Type_To, Id_From, Id_To, StoreId, StoreId_Name);

                    }
                    if (Common.Project_Market == 1 & IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                    {
                        mysql = " UPDATE Unit_item SET Full_qua =Full_qua + " + Larg_Qnty + ", D_Qua =D_Qua + " + Small_Qnty + ", Item_No =Item_No + " + Qnty_Another + " WHERE (id = " + Unit_Item_Table_Id + ") ";
                        opp.insert_me(mysql);

                        //// «·»Ê‰’
                        mysql = " UPDATE Unit_item SET Full_qua =Full_qua + " + LargItemsQnty_Free + ", D_Qua =D_Qua + " + SmallItemsQnty_Free + ", Item_No =Item_No + " + Item_No_Free + " WHERE (id = " + Unit_Item_Table_Id + ") ";
                        opp.insert_me(mysql);

                        decimal OLD_Larg_Qnty = 0;
                        decimal OLD_Small_Qnty = 0;
                        decimal OLD_Item_NoQnty = 0;

                        BillOperationCLS invd = new BillOperationCLS();
                        invd.Unit_Item_ExpireDate_Id = Unit_Item_ExpireDate_Id;
                        invd.Bills_Details_Id = Invoices_Details_Id;
                        invd.Item_Id = Item_Id;
                        invd.Unit_Item_Table_Id = (int)Unit_Item_Table_Id;
                        invd.Item_Expire_Date = Item_Expire_Date;
                        invd.Tash3ela = Tash3ela;
                        invd.Larg_Qnty = Larg_Qnty;
                        invd.Small_Qnty = Small_Qnty;
                        invd.Qnty_Another = decimal.Parse(Qnty_Another);
                        invd.LargItemsQnty_Free = LargItemsQnty_Free;
                        invd.SmallItemsQnty_Free = SmallItemsQnty_Free;
                        invd.Item_No_Free = Item_No_Free;

                        CommonAccounts.SetBalanceWithExpireDate_Tash3ela_Bill(ref opp, 1, invd, OLD_Larg_Qnty, OLD_Small_Qnty, OLD_Item_NoQnty);


                        //mysql = "  UPDATE Unit_item SET Full_qua = FLOOR((Small_unit * Full_qua + D_Qua) / Small_unit), ";
                        //mysql = mysql + " D_Qua =   (( Small_unit * Full_qua + D_Qua) / Small_unit - FLOOR(( Small_unit * Full_qua + D_Qua) /  Small_unit)) *  Small_unit  ";
                        //mysql = mysql + " WHERE (id = " + Unit_Item_Table_Id + ")  ";

                        mysql = "  UPDATE Unit_item SET Full_qua = FLOOR((Small_unit * Full_qua + D_Qua) / Small_unit), ";
                        mysql = mysql + " D_Qua =    (Small_unit * Full_qua + D_Qua) % Small_unit  ";
                        mysql = mysql + " WHERE (id = " + Unit_Item_Table_Id + ")  ";

                        opp.insert_me(mysql);
                    }
                    ////////////// ÕœÌÀ «Œ— ”⁄— «·‘—«¡
                    ///

                    if (Common.Is_Update_Price_On_Bill == 1)
                    {
                        mysql = " UPDATE  Item SET P_Price =  " + Larg_Unit_Price + " WHERE (id = " + Item_Id + ") ";
                        opp.insert_me(mysql);
                        mysql = " UPDATE  Unit_Item SET P_Price =  " + Larg_Unit_Price + " WHERE (Item_Id = " + Item_Id + ") ";
                        opp.insert_me(mysql);

                        if (Unit_Used_for_Price == 1)
                        {
                            mysql = " UPDATE  Item SET P_price_LargeUnit =  " + Larg_Unit_Price + ",Unit_Used_For_P_price_Default=" + Unit_Used_for_Price + " WHERE (id = " + Item_Id + ") ";
                            opp.insert_me(mysql);
                            mysql = " UPDATE  Unit_Item SET P_price_LargeUnit =  " + Larg_Unit_Price + " ,Unit_Used_For_P_price_Default=" + Unit_Used_for_Price + "  WHERE (Item_Id = " + Item_Id + ") ";
                            opp.insert_me(mysql);
                        }
                        if (Unit_Used_for_Price == 2)
                        {

                            mysql = " UPDATE  Item SET P_price_SmallUnit =  " + Larg_Unit_Price + ",Unit_Used_For_P_price_Default=" + Unit_Used_for_Price + " WHERE (id = " + Item_Id + ") ";
                            opp.insert_me(mysql);
                            mysql = " UPDATE  Unit_Item SET P_price_SmallUnit =  " + Larg_Unit_Price + " ,Unit_Used_For_P_price_Default=" + Unit_Used_for_Price + "  WHERE (Item_Id = " + Item_Id + ") ";
                            opp.insert_me(mysql);
                        }
                        if (Unit_Used_for_Price == 3)
                        {
                            mysql = " UPDATE  Item SET P_price_Item_No =  " + Larg_Unit_Price + ",Unit_Used_For_P_price_Default=" + Unit_Used_for_Price + " WHERE (id = " + Item_Id + ") ";
                            opp.insert_me(mysql);
                            mysql = " UPDATE  Unit_Item SET P_price_Item_No =  " + Larg_Unit_Price + " ,Unit_Used_For_P_price_Default=" + Unit_Used_for_Price + "  WHERE (Item_Id = " + Item_Id + ") ";
                            opp.insert_me(mysql);
                        }
                    }
                }

                ////////// set the work on database

                //SuppliersAccount ca = new SuppliersAccount();
                ////ca.Set_SuppliersAccount_Balance(ref Excp, Supplier_Id, New_Invoice_Id, Net_Total, Notes);
                //ca.Set_Supplier_Balance_MakeBill(ref Excp, ref opp, Supplier_Id, New_Bills_List_Id, Net_Total, Notes);
                //////////// order estlam automatic
                //if (Common.Make_Store_Order_Sarf_Estlam_Automatic_From_Invoice == 1 & IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                //{
                //    Store_LIB.Order_Details_AutomaticCls oo = new Order_Details_AutomaticCls();
                //    oo.Make_Order_From_Bill_Details(ref Excp, ref opp, Invoice_ds.Tables[0], Invoice_Id, Supplier_Id, Invoice_Date, "›« Ê—… „‘ —Ì« ");
                
                //}
                //Excp = 0;
                //opp.Commit();

                ////////// set the work on database

                Suppliers_AccountsCls ca = new Suppliers_AccountsCls();
                //ca.Set_SuppliersAccount_Balance(ref Excp, Supplier_Id, New_Invoice_Id, Net_Total, Notes);

                ca.Set_Supplier_Balance_MakeBill(ref Excp, ref opp, Supplier_Id, New_Bills_List_Id, Net_Total, Notes);
                ////////// order estlam automatic
                if (Common.Make_Store_Order_Sarf_Estlam_Automatic_From_Invoice == 1 & IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                {
                    Order_Details_AutomaticCls oo = new Order_Details_AutomaticCls();
                    oo.Make_Order_From_Bill_Details(ref Excp, ref opp, Invoice_ds.Tables[0], New_Bills_List_Id, Supplier_Id, Invoice_Date, "›« Ê—… „‘ —Ì« ");
                    Msg = " „ ≈’œ«— ≈–‰ «·≈” ·«„ „‰ «·„Ê—œ ≈·Ï «·„Œ“‰" ;
                }
                
                //«‰‘«¡ «·ﬁÌœ
                Account_Entry_Invoice_CLS aa = new Account_Entry_Invoice_CLS();
                aa.IsNotDeletePrevEntry = true;
                aa.Make_EntryFor_Bills(ref Excp, ref Msg, ref opp, New_Bills_List_Id, Invoice_Date, Invoice_Date, -1);

                if (Excp == 0)
                {
                    opp.Commit();
                }
                else
                {
                    Excp = 1;
                    opp.Rollback();
                }
            }
            catch (Exception ee)
            {
                Excp = 1;
                opp.Rollback();
            }

        }
         
        public void Make_New_Bill_List_Id(ref int Excp, ref AccountModelsLib.operations opp, ref long ID, long Invoice_Id, long Supplier_Id, DateTime Invoice_Date, int Invoice_Type, decimal Total, decimal commission, decimal CommissionValue, decimal TaxSales10, decimal ATS05, decimal TaxSales10_Value, decimal ATS05_Value, decimal Net_Total, string Currency, long Currency_Id, int Customer_BalanceSet, string Notes, string PolicaNo, string TroodContainerNo, decimal LoadPayment, decimal TotalSecond, decimal Damaghat, int Load_Payment_OnTax, decimal SupplierAccountBefor, decimal SupplierAccountAfter, long SubAccount_Id, string SubAccount_Name, int IS_Make_Sarf_Item_BalanceFromInvoice = 0, int Is_InvoiceKta3y = 0)
        {

            string mysql = " INSERT INTO Bills_List (Invoice_Id, Supplier_Id, Invoice_Date, Total, commission,CommissionValue, TaxSales10, ATS05,TaxSales10_Value,ATS05_Value , Net_Total , Currency,Currency_id,Invoice_Type,Supplier_BalanceSet,Notes,PolicaNo, TroodContainerNo,Load_Payment,Second_Total,Damaghat,Load_Payment_OnTax,SupplierAccountBefor, SupplierAccountAfter,User_Id,Branch_Id,    SubAccount_Id,   SubAccount_Name,    IS_Make_Sarf_Item_BalanceFromInvoice,   Is_InvoiceKta3y,OperationDate) ";
            mysql = mysql + " VALUES (" + Invoice_Id + "," + Supplier_Id + ",'" + AccountModelsLib.operations.dateFormat(Invoice_Date) + "'," + Total + "," + commission + "," + CommissionValue + "," + TaxSales10 + "," + ATS05 + "," + TaxSales10_Value + "," + ATS05_Value + "," + Net_Total + ",'" + Currency + "'," + Currency_Id + "," + Invoice_Type + "," + Customer_BalanceSet + ",'" + Notes + "','" + PolicaNo + "',  '" + TroodContainerNo + "'," + LoadPayment + "," + TotalSecond + "," + Damaghat + "," + Load_Payment_OnTax + "," + SupplierAccountBefor + "," + SupplierAccountAfter + "," + Common.User_Id + "," + Common.Branch_Id + "," + SubAccount_Id + ",'" + SubAccount_Name + "', " + IS_Make_Sarf_Item_BalanceFromInvoice + "," + Is_InvoiceKta3y + "," + AccountModelsLib.operations.Get_DateTimeToInsert(DateTime.Now) + ")";

            ID = opp.Insert_me_Return_Last_Identify(mysql, ref Excp);


        }


        public void Make_New_Bill_List_Id(ref int Excp, ref long ID, long Invoice_Id, long Supplier_Id, DateTime Invoice_Date, int Invoice_Type, decimal Total, decimal commission, decimal CommissionValue, decimal TaxSales10, decimal ATS05, decimal TaxSales10_Value, decimal ATS05_Value, decimal Net_Total, string Currency, long Currency_Id, int Customer_BalanceSet, string Notes, string PolicaNo, string TroodContainerNo, decimal LoadPayment, decimal TotalSecond, decimal Damaghat, int Load_Payment_OnTax, decimal SupplierAccountBefor, decimal SupplierAccountAfter, long SubAccount_Id, string SubAccount_Name)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            Make_New_Bill_List_Id(ref Excp, ref opp, ref ID, Invoice_Id, Supplier_Id, Invoice_Date, Invoice_Type, Total, commission, CommissionValue, TaxSales10, ATS05, TaxSales10_Value, ATS05_Value, Net_Total, Currency, Currency_Id, Customer_BalanceSet, Notes, PolicaNo, TroodContainerNo, LoadPayment, TotalSecond, Damaghat, Load_Payment_OnTax, SupplierAccountBefor, SupplierAccountAfter, SubAccount_Id, SubAccount_Name);

        }
         
        public DataSet Get_Bill_List_By_Table_Id(long Invoice_List_table_Id)
        {


            string mysql;

            mysql = @"  SELECT DISTINCT 
                         '' AS Serial_, '' AS Size_, '0' AS Make_Order, '100' AS Item_DeletedFlage, '100' AS Item_ProcessedFlage, '' AS QntyStrColumn, Bills_List.IS_Make_Sarf_Item_BalanceFromInvoice, Bills_List.Is_InvoiceKta3y, 
                         Bills_List.Notes AS Notes_List, Bills_List.commissionValue, Bills_List.IsCommission_Value, Bills_List.supplieraccountafter, Bills_List.supplieraccountbefor, Bills_List.Load_Payment_OnTax, Bills_List.Damaghat, 
                         Bills_List.Load_Payment, Bills_List.Second_Total, Item.Item_Name, unit.u_name, unit.B_Unit, unit.S_unit, unit.B_Unit_Value, unit.S_unit_Value, Supplier.Name, Bills_Details.code, Bills_Details.Unit_Id, Bills_List.ATS05_Value, 
                         Bills_List.TaxSales10,Bills_List.TaxSales10_Value, Bills_List.ATS05, Bills_Details.Qnty_value, Bills_Details.Unit_Used_for_Price, Bills_Details.Small_Unit_Price, Bills_Details.Larg_Unit_Price, Bills_Details.Qnty_Another, Bills_Details.Small_Qnty, 
                         Bills_Details.Larg_Qnty, Bills_Details.Unit_Item_Table_Id, Bills_Details.Item_Id, Bills_Details.Specification, Bills_List.PolicaNo, Bills_List.TroodContainerNo, Bills_List.Currency_id, Bills_List.Net_Total, Bills_List.commission, 
                         Bills_List.Invoice_Date, Bills_List.Supplier_Id, Bills_List.Invoice_Id, Bills_List.ID, Bills_List.Total, Bills_Details.Item_ATS05, Bills_Details.Item_TaxSales10, Bills_List.Invoice_Type, Supplier.Address, 
                         Bills_Details.Is_Item_CommissionValue, Bills_Details.Item_CommissionValue, Bills_Details.Unit_CommissionValue, unit.u_name AS Expr1, unit.B_Unit AS Expr2, unit.S_unit AS Expr3, unit.B_Unit_Value AS Expr4, 
                         unit.S_unit_Value AS Expr5, unit.S_unit_Value AS Small_Unit_Value, Unit_item.Full_qua AS Available_Larg_Balance, Unit_item.D_Qua AS Available_Small_Balance, Unit_item.Item_No, Bills_Details.Id AS Invoices_Details_Id, 
                         Supplier.Balance AS Customer_Balance, Supplier.Id AS Supplier_Id, Bills_Details.Invoice_Item_Name, Bills_Details.Store_Id, Item.Somk_Unit, Item.Somk, Item.Height_, Item.Width_, Item.Items_Types_Name, 
                         Item.Items_Types_Name_Id, Bills_Details.OperationNo, Bills_Details.Merchant_Name, Bills_Details.StatementNo, Bills_Details.Audience_price, Bills_Details.Tash3ela, Bills_Details.Item_No_Free, 
                         Bills_Details.LargItemsQnty_Free, Bills_Details.SmallItemsQnty_Free, Bills_Details.Item_Expire_Date, Bills_Details.Is_Alternate_Item_CommissionValue, Bills_Details.Alternate_Item_CommissionValue, 
                         Bills_Details.Alternate_Item_CommissionRate, Bills_Details.Alternate_ItemCommissionTotal, Bills_Details.Net_Total_Item_Commission, Bills_Details.QntyStr, Bills_Details.ItemCommissionTotal, 
                         Bills_Details.Item_CommissionRate, Bills_Details.Unit_CommissionRate, Bills_Details.Alternate_Unit_CommissionRate, Bills_Details.Alternate_Unit_CommissionValue, Bills_Details.Item_Net_Total_Price, 
                         Bills_Details.Item_TaxSales10_Value, Bills_Details.Item_ATS05_Value, Bills_Details.Item_Total_WithoutTax, Store.Store_Name, Store.Store_Code, Store.IS_Default_Store, Bills_Details.QntyWidth, Bills_Details.QntyLenght, 
                         Bills_Details.QntyItemNo, Bills_Details.Total As Item_Total, Bills_Details.Item_Notes, Bills_Details.Balance_Checked, Bills_Details.Unit_Item_ExpireDate_Id


                        ,Bills_List.Supplier_Notes,Bills_Details.TotalSmall_QntyStrColumn 
                        ,Bills_Details.TotalSmall_QntyStr,Bills_Details.TotalSmall_Qnty,Bills_List.Total_List3Mola,Bills_List.Value_3Mola,Bills_List.Total3MolaRate,Bills_List.NawlounValue,Bills_List.WahbaValue
,Bills_List.Total_PriceWithoutTax
FROM            Bills_List INNER JOIN
                         Supplier ON Bills_List.Supplier_Id = Supplier.Id INNER JOIN
                         Bills_Details ON Bills_List.ID = Bills_Details.Invoice_Id INNER JOIN
                         Item INNER JOIN
                         Unit_item ON Item.id = Unit_item.Item_id INNER JOIN
                         unit ON Unit_item.unit_id = unit.id ON Bills_Details.Unit_Item_Table_Id = Unit_item.id INNER JOIN
                         Store ON Unit_item.Store_id = Store.id
";
            mysql = mysql + " WHERE (Bills_List.ID = " + Invoice_List_table_Id + ")";

        int Excp = 0;
            DataSet G_DS = new DataSet();
            try
            {
                AccountModelsLib.operations opp = new AccountModelsLib.operations();
                G_DS = opp.return_dataset(mysql, "Invoices_List");
                Excp = 1;
                opp = null;
                return G_DS;
            }
            catch
            {
                Excp = -1;
                return G_DS;
            }
        }

        public DataSet Get_Bills_List_Reverse_By_Table_Id(long Invoice_List_table_Id)
        {
            string mysql;



            mysql = @"     SELECT DISTINCT 
                         '' AS Serial_, '' AS Size_, '0' AS Make_Order, '100' AS Item_DeletedFlage, '100' AS Item_ProcessedFlage, '' AS QntyStrColumn, Bills_List_Reverse.IS_Make_Sarf_Item_BalanceFromInvoice, Bills_List_Reverse.Is_InvoiceKta3y, 
                         Bills_List_Reverse.Notes AS Notes_List, Bills_List_Reverse.commissionValue, Bills_List_Reverse.IsCommission_Value, Bills_List_Reverse.supplieraccountafter, Bills_List_Reverse.supplieraccountbefor, Bills_List_Reverse.Load_Payment_OnTax, Bills_List_Reverse.Damaghat, 
                         Bills_List_Reverse.Load_Payment, Bills_List_Reverse.Second_Total, Item.Item_Name, unit.u_name, unit.B_Unit, unit.S_unit, unit.B_Unit_Value, unit.S_unit_Value, Supplier.Name, Bills_Details_Reverse.code, Bills_Details_Reverse.Unit_Id, Bills_List_Reverse.ATS05_Value, 
                         Bills_List_Reverse.TaxSales10, Bills_List_Reverse.ATS05, Bills_Details_Reverse.Qnty_value, Bills_Details_Reverse.Unit_Used_for_Price, Bills_Details_Reverse.Small_Unit_Price, Bills_Details_Reverse.Larg_Unit_Price, Bills_Details_Reverse.Qnty_Another, Bills_Details_Reverse.Small_Qnty, 
                         Bills_Details_Reverse.Larg_Qnty, Bills_Details_Reverse.Unit_Item_Table_Id, Bills_Details_Reverse.Item_Id, Bills_Details_Reverse.Specification, Bills_List_Reverse.PolicaNo, Bills_List_Reverse.TroodContainerNo, Bills_List_Reverse.Currency_id, Bills_List_Reverse.Net_Total, Bills_List_Reverse.commission, 
                         Bills_List_Reverse.Invoice_Date, Bills_List_Reverse.Supplier_Id, Bills_List_Reverse.Invoice_Id, Bills_List_Reverse.ID, Bills_List_Reverse.Total, Bills_Details_Reverse.Item_ATS05, Bills_Details_Reverse.Item_TaxSales10, Bills_List_Reverse.Invoice_Type, Supplier.Address, 
                         Bills_Details_Reverse.Is_Item_CommissionValue, Bills_Details_Reverse.Item_CommissionValue, Bills_Details_Reverse.Unit_CommissionValue, unit.u_name AS Expr1, unit.B_Unit AS Expr2, unit.S_unit AS Expr3, unit.B_Unit_Value AS Expr4, 
                         unit.S_unit_Value AS Expr5, unit.S_unit_Value AS Small_Unit_Value, Unit_item.Full_qua AS Available_Larg_Balance, Unit_item.D_Qua AS Available_Small_Balance, Unit_item.Item_No, Bills_Details_Reverse.Id AS Invoices_Details_Id, 
                         Supplier.Balance AS Customer_Balance, Supplier.Id AS Supplier_Id, Bills_Details_Reverse.Invoice_Item_Name, Bills_Details_Reverse.Store_Id, Item.Somk_Unit, Item.Somk, Item.Height_, Item.Width_, Item.Items_Types_Name, 
                         Item.Items_Types_Name_Id, Bills_Details_Reverse.OperationNo, Bills_Details_Reverse.Merchant_Name, Bills_Details_Reverse.StatementNo, Bills_Details_Reverse.Audience_price, Bills_Details_Reverse.Tash3ela, Bills_Details_Reverse.Item_No_Free, 
                         Bills_Details_Reverse.LargItemsQnty_Free, Bills_Details_Reverse.SmallItemsQnty_Free, Bills_Details_Reverse.Item_Expire_Date, Bills_Details_Reverse.Is_Alternate_Item_CommissionValue, Bills_Details_Reverse.Alternate_Item_CommissionValue, 
                         Bills_Details_Reverse.Alternate_Item_CommissionRate, Bills_Details_Reverse.Alternate_ItemCommissionTotal, Bills_Details_Reverse.Net_Total_Item_Commission, Bills_Details_Reverse.QntyStr, Bills_Details_Reverse.ItemCommissionTotal, 
                         Bills_Details_Reverse.Item_CommissionRate, Bills_Details_Reverse.Unit_CommissionRate, Bills_Details_Reverse.Alternate_Unit_CommissionRate, Bills_Details_Reverse.Alternate_Unit_CommissionValue, Bills_Details_Reverse.Item_Net_Total_Price, 
                         Bills_Details_Reverse.Item_TaxSales10_Value, Bills_Details_Reverse.Item_ATS05_Value, Bills_Details_Reverse.Item_Total_WithoutTax, Store.Store_Name, Store.Store_Code, Store.IS_Default_Store, Bills_Details_Reverse.QntyWidth, Bills_Details_Reverse.QntyLenght, 
                         Bills_Details_Reverse.QntyItemNo, Bills_Details_Reverse.Total As Item_Total, Bills_Details_Reverse.Item_Notes, Bills_Details_Reverse.Balance_Checked, Bills_Details_Reverse.Unit_Item_ExpireDate_Id

                        ,Bills_List_Reverse.Supplier_Notes,Bills_Details_Reverse.TotalSmall_QntyStrColumn 
                        ,Bills_Details_Reverse.TotalSmall_QntyStr,Bills_Details_Reverse.TotalSmall_Qnty,Bills_List_Reverse.Total_List3Mola
 ,Bills_List_Reverse.Value_3Mola,Bills_List_Reverse.Total3MolaRate,Bills_List_Reverse.NawlounValue,Bills_List_Reverse.WahbaValue
,Bills_List_Reverse.Total_PriceWithoutTax
FROM            Bills_List_Reverse INNER JOIN
                         Supplier ON Bills_List_Reverse.Supplier_Id = Supplier.Id INNER JOIN
                         Bills_Details_Reverse ON Bills_List_Reverse.ID = Bills_Details_Reverse.Invoice_Id INNER JOIN
                         Item INNER JOIN
                         Unit_item ON Item.id = Unit_item.Item_id INNER JOIN
                         unit ON Unit_item.unit_id = unit.id ON Bills_Details_Reverse.Unit_Item_Table_Id = Unit_item.id INNER JOIN
                         Store ON Unit_item.Store_id = Store.id
  

";
            mysql = mysql + " WHERE (Bills_List_Reverse.ID = " + Invoice_List_table_Id + ")";

            int Excp = 0;
            DataSet G_DS = new DataSet();
            try
            {
                AccountModelsLib.operations opp = new AccountModelsLib.operations();
                G_DS = opp.return_dataset(mysql, "Invoices_List");
                Excp = 1;
                opp = null;
                return G_DS;
            }
            catch
            {
                Excp = -1;
                return G_DS;
            }
        }


        public DataSet Get_Suppliers_Invoices(ref int Excp, decimal nuInvoiceNo, long Supplier_Id, bool _DateFrom, DateTime DateFrom, bool _DateTo, DateTime DateTo, bool NewInvoice, int Invoice_Type_, int Is_Invoice_Agile, bool OrderBySupplier)
        {
            DataSet ds = null;
            try
            {

                string mysql = " SELECT DISTINCT  Bills_List.*, Supplier.Id AS Supplier_Id , Supplier.Name,  Supplier.Address, Supplier.Tel, Supplier.Tel2, Supplier.Mobile, Supplier.E_Mail, Supplier.Fax, Supplier.Web_Site, Supplier.Balance,  Supplier.Supplier_Type, Supplier.Active, Supplier.SubAccount_Id ";
                mysql = mysql + " ,  ISNULL ((SELECT SUM(Export_Value) - SUM(Import_Value) AS Expr1  FROM Money_Transaction_Box WHERE (DataDictionaryProcessesListsId = 2) AND (Reference_Type_Table_Id = Bills_List.ID)), 0) AS TransactionValue ";

                mysql = mysql + " FROM Supplier INNER JOIN Bills_List ON Supplier.Id = Bills_List.Supplier_Id ";
                mysql = mysql + " WHERE ( Bills_List.Invoice_Type = " + Invoice_Type_ + ") ";
                if (_DateFrom)
                {
                    mysql = mysql + " AND (CONVERT(DATE, Bills_List.Invoice_Date)  >= '" + AccountModelsLib.operations.dateFormat(DateFrom.Date) + "') ";

                    //mysql = mysql + " AND  ( Bills_List.Invoice_Date >='" + AccountModelsLib.operations.dateFormat(DateFrom) + "')   ";
                }
                if (_DateTo)
                {
                    mysql = mysql + " AND (CONVERT(DATE, Bills_List.Invoice_Date)  <= '" + AccountModelsLib.operations.dateFormat(DateTo.Date) + "') ";
                    //mysql = mysql + "  AND ( Bills_List.Invoice_Date <='" + AccountModelsLib.operations.dateFormat(DateTo) + "') ";
                }
                if (Supplier_Id > 0)
                {
                    mysql = mysql + "  AND (Supplier.Id = " + Supplier_Id + ")";
                }
                if (NewInvoice)
                {
                    mysql = mysql + "  AND ( Bills_List.Supplier_BalanceSet =0) ";
                }
                else
                {
                    mysql = mysql + "  AND ( Bills_List.Supplier_BalanceSet = 1) ";
                }
                if (nuInvoiceNo > 0)
                {
                    mysql = mysql + "  AND (Bills_List.Invoice_Id=" + nuInvoiceNo + ") ";
                }
                if (Is_Invoice_Agile > -1)
                {
                    mysql = mysql + "  AND (Bills_List.Is_Invoice_Agile=" + Is_Invoice_Agile + ") ";
                }

                if (OrderBySupplier == true)
                {
                    mysql = mysql + " ORDER BY Bills_List.Supplier_Id ";
                }
                else
                {
                    mysql = mysql + "  Order By Bills_List.Invoice_Date ";
                }
                AccountModelsLib.operations opp = new AccountModelsLib.operations();
                ds = opp.return_dataset(ref Excp, mysql, "Bills_List");
                opp = null;
                Excp = 0;
                return ds;
            }
            catch (Exception e)
            {
                Excp = 1;
                throw (e);
            }

        }

        public DataSet Get_Suppliers_InvoicesList(ref int Excp, decimal nuInvoiceNo, long Supplier_Id, bool _DateFrom, DateTime DateFrom, bool _DateTo, DateTime DateTo, bool NewInvoice, int Invoice_Type_, int Is_Invoice_Agile, bool OrderBySupplier)
        {
            DataSet ds = null;
            try
            {

                string mysql = " SELECT DISTINCT  Bills_List.*, Supplier.Id AS Supplier_Id , Supplier.Name,  Supplier.Address, Supplier.Tel, Supplier.Tel2, Supplier.Mobile, Supplier.E_Mail, Supplier.Fax, Supplier.Web_Site, Supplier.Balance,  Supplier.Supplier_Type, Supplier.Active, Supplier.SubAccount_Id ";
                //mysql = mysql + " ,  ISNULL ((SELECT SUM(Export_Value) - SUM(Import_Value) AS Expr1  FROM Money_Transaction_Box WHERE (DataDictionaryProcessesListsId = 2) AND (Reference_Type_Table_Id = Bills_List.ID)), 0) AS TransactionValue ";

                mysql = mysql + " FROM Supplier INNER JOIN Bills_List ON Supplier.Id = Bills_List.Supplier_Id ";
                mysql = mysql + " WHERE ( Bills_List.Invoice_Type = " + Invoice_Type_ + ") ";
                if (_DateFrom)
                {
                    mysql = mysql + " AND (CONVERT(DATE, Bills_List.Invoice_Date)  >= '" + AccountModelsLib.operations.dateFormat(DateFrom.Date) + "') ";

                    //mysql = mysql + " AND  ( Bills_List.Invoice_Date >='" + AccountModelsLib.operations.dateFormat(DateFrom) + "')   ";
                }
                if (_DateTo)
                {
                    mysql = mysql + " AND (CONVERT(DATE, Bills_List.Invoice_Date)  <= '" + AccountModelsLib.operations.dateFormat(DateTo.Date) + "') ";
                    //mysql = mysql + "  AND ( Bills_List.Invoice_Date <='" + AccountModelsLib.operations.dateFormat(DateTo) + "') ";
                }
                if (Supplier_Id > 0)
                {
                    mysql = mysql + "  AND (Supplier.Id = " + Supplier_Id + ")";
                }
                if (NewInvoice)
                {
                    mysql = mysql + "  AND ( Bills_List.Supplier_BalanceSet =0) ";
                }
                else
                {
                    mysql = mysql + "  AND ( Bills_List.Supplier_BalanceSet = 1) ";
                }
                if (nuInvoiceNo > 0)
                {
                    mysql = mysql + "  AND (Bills_List.Invoice_Id=" + nuInvoiceNo + ") ";
                }
                if (Is_Invoice_Agile > -1)
                {
                    mysql = mysql + "  AND (Bills_List.Is_Invoice_Agile=" + Is_Invoice_Agile + ") ";
                }

                if (OrderBySupplier == true)
                {
                    mysql = mysql + " ORDER BY Bills_List.Supplier_Id ";
                }
                else
                {
                    mysql = mysql + "  Order By Bills_List.Invoice_Date ";
                }
                AccountModelsLib.operations opp = new AccountModelsLib.operations();
                ds = opp.return_dataset(ref Excp, mysql, "Bills_List");
                opp = null;
                Excp = 0;
                return ds;
            }
            catch (Exception e)
            {
                Excp = 1;
                throw (e);
            }

        }
        public DataSet Get_Suppliers_Bills_List_Reverse(ref int Excp, decimal nuInvoiceNo, long Supplier_Id, bool _DateFrom, DateTime DateFrom, bool _DateTo, DateTime DateTo, bool NewInvoice, int Invoice_Type_)
        {
            DataSet ds = null;
            try
            {

                string mysql = " SELECT DISTINCT Bills_List_Reverse.*, Supplier.Id AS Supplier_Id , Supplier.Name,  Supplier.Address, Supplier.Tel, Supplier.Tel2, Supplier.Mobile, Supplier.E_Mail, Supplier.Fax, Supplier.Web_Site, Supplier.Balance,  Supplier.Supplier_Type, Supplier.Active, Supplier.SubAccount_Id FROM Supplier INNER JOIN Bills_List_Reverse ON Supplier.Id = Bills_List_Reverse.Supplier_Id ";
                //mysql = mysql + " WHERE ( Bills_List_Reverse.Invoice_Type = " + Invoice_Type_ + ") ";
                mysql = mysql + " WHERE ( Bills_List_Reverse.Id > 0 ) ";
                 

                if (_DateFrom == true)
                {
                    //mysql = mysql + " AND  (Invoices_List.Invoice_Date >='" + AccountModelsLib.operations.dateFormat(DateFrom) + "')   ";
                    mysql = mysql + " AND (CONVERT(DATE, Bills_List_Reverse.Invoice_Date)  >= '" + AccountModelsLib.operations.dateFormat(DateFrom.Date) + "') ";

                }
                if (_DateTo == true)
                {
                    //mysql = mysql + "   AND (Invoices_List.Invoice_Date <='" + AccountModelsLib.operations.dateFormat(DateTo) + "') ";
                    mysql = mysql + " AND (CONVERT(DATE, Bills_List_Reverse.Invoice_Date)  <= '" + AccountModelsLib.operations.dateFormat(DateTo.Date) + "') ";

                }

                if (Supplier_Id > 0)
                {
                    mysql = mysql + "  AND (Supplier.Id = " + Supplier_Id + ")";
                }
                if (NewInvoice)
                {
                    mysql = mysql + "  AND ( Bills_List_Reverse.Supplier_BalanceSet =0) ";
                }
                else
                {
                    mysql = mysql + "  AND ( Bills_List_Reverse.Supplier_BalanceSet = 1) ";
                }
                if (nuInvoiceNo > 0)
                {
                    mysql = mysql + "  AND (Bills_List_Reverse.Invoice_Id=" + nuInvoiceNo + ") ";
                }
                AccountModelsLib.operations opp = new AccountModelsLib.operations();
                ds = opp.return_dataset(ref Excp, mysql, "Bills_List_Reverse");
                opp = null;
                Excp = 0;
                return ds;
            }
            catch (Exception e)
            {
                Excp = 1;
                throw (e);
            }

        }
       
        

         }
 
}
