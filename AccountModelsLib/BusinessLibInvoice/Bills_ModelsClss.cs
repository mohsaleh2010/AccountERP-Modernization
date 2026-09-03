using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using Microsoft.Data.SqlClient;
using System.Data.OleDb;
using Microsoft.EntityFrameworkCore;
using AccountModelsLib;
using AccountModelsLib.Models; 
using System.Linq;
using System.Security.Cryptography.X509Certificates;
 
using Newtonsoft.Json;
using System.Collections.Generic;
namespace AccountModelsLib
{
    /// <summary>
    /// Summary description for InvoicesClss.
    /// </summary>
    public class Bills_ModelsClss
    {
        //long New_Invoice_Id;
        public Int64 New_Invoices_No;   // —ﬁ„ «·›« Ê—Â «·ÃœÌœ…
        public long New_Bills_List_Id;
        public int Invoice_Type;  ///Invoice_Type=0 if ﬁÿ«⁄ ⁄«„   or =1 if    ﬁÿ«⁄ Œ«’ 
        public ArrayList arr_Order_Purshing_Id_Automatic; //·Õ›Ÿ —ﬁ„ «·«·Ï ·«’œ«— «·«–‰ ÊÌ” Œœ„ ··ÿ»«⁄…
        public Bills_ModelsClss()
        {
            New_Invoices_No = 0;
            Invoice_Type = 0;
            //
            // TODO: Add constructor logic here
            //
        }

        public Bills_ModelsClss(int Invoice_Types)
        {
            New_Invoices_No = 0;
            Invoice_Type = Invoice_Types;
            //
            // TODO: Add constructor logic here
            //
        }

        public long Make_New_Invoice_No(int Invoice_Type)
        {

            int Max_Invoice_Id = 0;
            string mysql = "SELECT MAX(Invoice_Id) AS Max_Invoice_Id FROM  Bills_List where Invoice_Type=" + Invoice_Type;
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            DataSet ds = opp.return_dataset(mysql, "Bills_List");
            try
            {
                if (ds.Tables["Bills_List"].Rows[0]["Max_Invoice_Id"].ToString() != "")
                {
                    Max_Invoice_Id = int.Parse(ds.Tables["Bills_List"].Rows[0]["Max_Invoice_Id"].ToString()) + 1;
                }
                else
                {
                    Max_Invoice_Id = 1;
                }
            }
            catch
            {
                Max_Invoice_Id = 1;
            }
            opp = null;
            ds = null;
            return Max_Invoice_Id;
        }

        public long Make_New_Invoice_No_Reverse(int Invoice_Type)
        {
            //where Invoice_Type=" + Invoice_Type;
            int Max_Invoice_Id = 0;
            string mysql = "SELECT MAX(Invoice_Id) AS Max_Invoice_Id FROM  Bills_List_Reverse where Invoice_Type=" + Invoice_Type;
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            DataSet ds = opp.return_dataset(mysql, "Bills_List");
            try
            {
                Max_Invoice_Id = int.Parse(ds.Tables["Bills_List"].Rows[0]["Max_Invoice_Id"].ToString()) + 1;
            }
            catch
            {
                Max_Invoice_Id = 1;
            }
            opp = null;
            ds = null;
            return Max_Invoice_Id;
        }


        /// <summary>
        /// Return True if New Invoice number found else return false
        /// </summary>
        /// <param name="Invoice_No">New_Invoices_No</param>
        /// <returns>bool</returns>
        public bool Check_InvoiceNo_Found(ref int Excp, long Invoice_No)
        {
            try
            {
                string mysql = "SELECT  Bills_List.* FROM  Bills_List WHERE (Invoice_Id = " + Invoice_No + ") ";
                AccountModelsLib.operations opp = new AccountModelsLib.operations();
                DataSet ds = opp.return_dataset(mysql, "Bills_List");
                if (ds.Tables["Bills_List"].Rows.Count > 0)
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
            catch (Exception e)
            {
                Excp = 1;
                throw (e);
            }
        }
        public void Make_Bill_Update_SupplierAccount(ref int Excp, ref string Msg, DataSet Invoice_ds, BillOperationListCLS invL)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations(1);
            try
            {
                //checked item_report to SendKeys to store
                DataView dvToStore = new DataView();
                dvToStore.Table = Invoice_ds.Tables[0];
                //Requests.ItemToStore it = new Requests.ItemToStore(dvToStore);
                //if (Common.Project_Market == 0)
                //{
                //    it.ShowDialog();
                //}
                //else
                //{
                //    it.ClickBtnOK();
                //}
                ArrayList mArrMake_Order = new ArrayList();
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
                //decimal Total = 0;



                //  Make_New_Bill_List_Id(ref Excp, ref opp, ref New_Invoice_Id, Invoice_No, Supplier_Id, Invoice_Date, Invoice_Type, TotalSecond, Commission, CommissionValue, TaxSales10, ATS05, TaxSales10_Value, ATS05_Value, Net_Total, "", Currency_id, 1, Notes, PolicaNo, TroodContainerNo, LoadPayment, TotalSecond, Damaghat, Load_Payment_OnTax, SupplierAccountBefor, SupplierAccountAfter, SubAccount_Id, SubAccount_Name, IS_Make_Sarf_Item_BalanceFromInvoice, Is_InvoiceKta3y);
                New_Bills_List_Id = invL.Insert_Bill_List(ref opp, ref Excp);
                if (Excp == 1)
                {
                    opp.Rollback();
                    return;
                }



                ///////////// add invoice Items and update request qnty


                //long Bills_List_Id_ = New_Bill_List_Id;

                string Supplier_Name = "";

                try
                {

                    Supplier_Name = Common.GetSupplierName(ref opp, invL.Supplier_Id);
                }
                catch { }

                InvoiceDataSetCLS ii = new InvoiceDataSetCLS();
                //////////// add item in invoice details
                for (int x = 0; x < Invoice_ds.Tables[0].Rows.Count; x++)
                {

                    BillOperationCLS invd = new BillOperationCLS();
                    invd.Bills_List_Id = New_Bills_List_Id;

                    invd.Get_Item_Details(Invoice_ds, x);


                    invd.Bills_Details_Id = invd.Insert_Bills_Details(ref opp, ref Excp);
                    if (Excp == 1)
                    {
                        opp.Rollback();
                        return;
                    }

                    Invoice_ds.Tables[0].Rows[x]["Invoices_Details_Id"] = invd.Bills_Details_Id;
                    long Unit_Item_ExpireDate_Id = 0;
                    int Transaction_Type = 1;
                    int Invoice_Type_Id = Common.Bill_Type_Id;// 3;


                    if (Common.Project_Market == 1 & invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                    {


                        try
                        {
                            invd.Store_Name = Common.GetStoreName(ref opp, invd.Store_Id);
                        }
                        catch { }

                        //int Type_From = Common.Supplier_StoreTransactionCode;
                        //int Type_To = Common.Store_StoreTransactionCode;
                        //long Id_From = invL.Supplier_Id;
                        //long Id_To = invd.Store_Id;
                        //long StoreId = invd.Store_Id;
                        //string StoreId_Name = invd.Store_Name;
                        // , Type_From,  Type_To,  Id_From,  Id_To,  StoreId,  StoreId_Name


                        //ii.Set_ItemTransactionFromInvoices(ref opp, invd.Unit_Item_Table_Id, invd.Item_Net_Total_Price, invd.Larg_Qnty, invd.Small_Qnty, invd.Qnty_Another, 0, Transaction_Type, invL.Invoice_Date, invL.Bills_List_Id, invd.Bills_Details_Id, invL.Invoice_Type_Id, "›« Ê—… „‘ —Ì« ", invL.Supplier_Name, invd.Store_Name, invL.Supplier_Name, invL.Invoice_No, Type_From, Type_To, Id_From, Id_To, StoreId, StoreId_Name);
                        ii.Set_ItemTransactionFromBill(ref opp, invd, invL, 0, Transaction_Type, Common.Bill_Type_Id, "›« Ê—… „‘ —Ì« ");

                    }
                    if (Common.Project_Market == 1 & invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                    {
                        mysql = " UPDATE Unit_item SET Full_qua =Full_qua + " + invd.Larg_Qnty + ", D_Qua =D_Qua + " + invd.Small_Qnty + ", Item_No =Item_No + " + invd.Qnty_Another + " WHERE (id = " + invd.Unit_Item_Table_Id + ") ";
                        opp.insert_me(mysql);

                        //// «·»Ê‰’
                        mysql = " UPDATE Unit_item SET Full_qua =Full_qua + " + invd.LargItemsQnty_Free + ", D_Qua =D_Qua + " + invd.SmallItemsQnty_Free + ", Item_No =Item_No + " + invd.Item_No_Free + " WHERE (id = " + invd.Unit_Item_Table_Id + ") ";
                        opp.insert_me(mysql);

                        decimal OLD_Larg_Qnty = 0;
                        decimal OLD_Small_Qnty = 0;
                        decimal OLD_Item_NoQnty = 0;
                        CommonAccounts.SetBalanceWithExpireDate_Tash3ela_Bill(ref opp, 1, invd, 0, 0, 0);


                        //mysql = "  UPDATE Unit_item SET Full_qua = FLOOR((Small_unit * Full_qua + D_Qua) / Small_unit), ";
                        //mysql = mysql + " D_Qua =   (( Small_unit * Full_qua + D_Qua) / Small_unit - FLOOR(( Small_unit * Full_qua + D_Qua) /  Small_unit)) *  Small_unit  ";
                        //mysql = mysql + " WHERE (id = " + Unit_Item_Table_Id + ")  ";

                        mysql = "  UPDATE Unit_item SET Full_qua = FLOOR((Small_unit * Full_qua + D_Qua) / Small_unit), ";
                        mysql = mysql + " D_Qua =    (Small_unit * Full_qua + D_Qua) % Small_unit  ";
                        mysql = mysql + " WHERE (id = " + invd.Unit_Item_Table_Id + ")  ";

                        opp.insert_me(mysql);
                    }
                    ////////////// ÕœÌÀ «Œ— ”⁄— «·‘—«¡
                    ///

                    UpdatePurchasePrice(ref opp, invd);
                    Invoice_ds.Tables[0].Rows[x]["Item_ProcessedFlage"] = 100;
                    Invoice_ds.Tables[0].Rows[x]["Item_DeletedFlage"] = 100;

                }
                ////////// order estlam automatic
                if (Common.Make_Store_Order_Sarf_Estlam_Automatic_From_Invoice == 1 & invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                {
                    Order_Details_AutomaticCls oo = new Order_Details_AutomaticCls();
                    oo.Make_Order_From_Bill_Details(ref Excp, ref opp, Invoice_ds.Tables[0], invL.Bills_List_Id, invL.Supplier_Id, invL.Invoice_Date, "›« Ê—… „‘ —Ì« ");
                    Msg = " „ ≈’œ«— ≈–‰ «·≈” ·«„ „‰ «·„Ê—œ ≈·Ï «·„Œ“‰";
                }
                ////////// set the work on database
                if (Common.IsSetSuppliers_AccountsForBills == 1)
                {
                    Suppliers_AccountsCls ca = new Suppliers_AccountsCls();
                    //ca.Set_SuppliersAccount_Balance(ref Excp, Supplier_Id, New_Invoice_Id, Net_Total, Notes);
                    ca.Set_Supplier_Balance_MakeBill(ref Excp, ref opp, invL.Supplier_Id, invL.Bills_List_Id, invL.Net_Total_Price, invL.Notes);


                    //«‰‘«¡ «·ﬁÌœ
                    Account_Entry_Invoice_CLS aa = new Account_Entry_Invoice_CLS();
                    aa.IsNotDeletePrevEntry = true;
                    aa.Make_EntryFor_Bills(ref Excp, ref Msg, ref opp, invL.Bills_List_Id, invL.Invoice_Date, invL.Invoice_Date, -1);


                    if (invL.Is_Invoice_Agile == 0)
                    {
                        if (Common.MakeOrderMoneyFromBill_AutomaticWithoutForm == 1)
                        {

                            decimal PaidValueFromMarket = invL.Net_Total_Price;
                            decimal RestValue = 0;
                            int Box_Id = 0;
                            Money_Transaction_BoxOrderReportCLS mo = new Money_Transaction_BoxOrderReportCLS();
                            //, long CarsWorkOrdersId, long Reference_Type_Table_Id, long DataDictionaryProcessesListsId

                            long CarsWorkOrdersId = 0;
                            long Reference_Type_Table_Id = invL.Bills_List_Id;
                            long DataDictionaryProcessesListsId = 2;

                            //   ,   CarsWorkOrdersId,   Reference_Type_Table_Id,   DataDictionaryProcessesListsId


                            long Entry_No = 0;


                            mo.MakeMoney_TransactionFromBill(ref Excp, ref Msg, ref opp, invL.Invoice_Date, invL.Invoice_No, New_Bills_List_Id, Box_Id, invL.Currency_id, invL.Supplier_Id, PaidValueFromMarket, RestValue, 0, Entry_No, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);
                        }
                    }
                }




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
        public void Make_Bill_Update_SupplierAccount(ref int Excp, ref string Msg, BillOperationCLS invD_O, BillOperationListCLS invL_O)
        {

            AccountModelsLib.operations opp = new AccountModelsLib.operations(1);
            try
            {
                //checked item_report to SendKeys to store
                DataView dvToStore = new DataView();
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
                ArrayList mArrMake_Order = new ArrayList();
                //mArrMake_Order = it.ArrItemToStore;
                //if (!it.BtnOkClick)
                //{
                //    Excp = 10;
                //    return;
                //}
                ////***********************************************************************
                ////////// Make_New_Invoice_Id

                DataTable dt = new DataTable();
                //dt = Invoice_ds.Tables[0];

                string mysql = "";
                //decimal Total = 0;
                //New_Bills_List_Id = invL.Insert_Bill_List(ref opp, ref Excp);
                New_Bills_List_Id = invL_O.Insert_Bill_List(ref opp, ref Excp, (Bills_List)invL_O.invL);

                if (Excp == 1)
                {
                    opp.Rollback();
                    return;
                }



                ///////////// add invoice Items and update request qnty


                //long Bills_List_Id_ = New_Bill_List_Id;

                string Supplier_Name = "";

                try
                {

                    Supplier_Name = Common.GetSupplierName(ref opp, invL_O.invL.Supplier_Id.Value);
                }
                catch { }

                InvoiceDataSetCLS ii = new InvoiceDataSetCLS();
                //////////// add item in invoice details
                for (int x = 0; x < invD_O.invDlst.Count; x++)
                {

                    //BillOperationCLS invd = new BillOperationCLS();
                    try
                    {
                        invD_O.invDlst[x].Store_Name = Common.GetStoreName(ref opp, invD_O.invDlst[x].Store_Id ?? 0);
                    }
                    catch { }

                    invD_O.invDlst[x].Invoice_Id = New_Bills_List_Id;
                    invD_O.invDlst[x].Item_ProcessedFlage = 100;
                    invD_O.invDlst[x].Item_DeletedFlage = 100;

                    //try
                    //{
                    var serializedParent = JsonConvert.SerializeObject(invD_O.invDlst[x]);
                    Bills_Details bills_Details = JsonConvert.DeserializeObject<Bills_Details>(serializedParent);


                    //Invoices_Details invoices_Details = (Invoices_Details)invD_O.invDlst[x];
                    invD_O.Invoices_Details_Id = invD_O.Insert_Invoices_Details(ref opp, ref Excp, bills_Details);
                    invD_O.invDlst[x].Id = bills_Details.Id;

                    //invd.Get_Item_Details(Invoice_ds, x);


                    //invd.Bills_Details_Id = invd.Insert_Bills_Details(ref opp, ref Excp);
                    if (Excp == 1)
                    {
                        opp.Rollback();
                        return;
                    }

                    //Invoice_ds.Tables[0].Rows[x]["Invoices_Details_Id"] = invd.Bills_Details_Id;
                    //long Unit_Item_ExpireDate_Id = 0;
                    int Transaction_Type = 1;
                    int Invoice_Type_Id = Common.Bill_Type_Id;// 3;


                    if (Common.Project_Market == 1 & invL_O.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                    {


                        //try
                        //{
                        //    invD_O.Store_Name = Common.GetStoreName(ref opp, bills_Details.Store_Id??0);
                        //}
                        //catch { }

                        //int Type_From = Common.Supplier_StoreTransactionCode;
                        //int Type_To = Common.Store_StoreTransactionCode;
                        //long Id_From = invL.Supplier_Id;
                        //long Id_To = invd.Store_Id;
                        //long StoreId = invd.Store_Id;
                        //string StoreId_Name = invd.Store_Name;
                        // , Type_From,  Type_To,  Id_From,  Id_To,  StoreId,  StoreId_Name


                        //ii.Set_ItemTransactionFromInvoices(ref opp, invd.Unit_Item_Table_Id, invd.Item_Net_Total_Price, invd.Larg_Qnty, invd.Small_Qnty, invd.Qnty_Another, 0, Transaction_Type, invL.Invoice_Date, invL.Bills_List_Id, invd.Bills_Details_Id, invL.Invoice_Type_Id, "›« Ê—… „‘ —Ì« ", invL.Supplier_Name, invd.Store_Name, invL.Supplier_Name, invL.Invoice_No, Type_From, Type_To, Id_From, Id_To, StoreId, StoreId_Name);
                        ii.Set_ItemTransactionFromBill(ref opp, bills_Details, invL_O, 0, Transaction_Type, Common.Bill_Type_Id, "›« Ê—… „‘ —Ì« ");

                    }
                    if (Common.Project_Market == 1 & invL_O.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                    {
                        mysql = " UPDATE Unit_item SET Full_qua =Full_qua + " + bills_Details.Larg_Qnty + ", D_Qua =D_Qua + " + bills_Details.Small_Qnty + ", Item_No =Item_No + " + bills_Details.Qnty_Another + " WHERE (id = " + bills_Details.Unit_Item_Table_Id + ") ";
                        opp.insert_me(mysql);

                        //// «·»Ê‰’
                        mysql = " UPDATE Unit_item SET Full_qua =Full_qua + " + bills_Details.LargItemsQnty_Free + ", D_Qua =D_Qua + " + bills_Details.SmallItemsQnty_Free + ", Item_No =Item_No + " + bills_Details.Item_No_Free + " WHERE (id = " + bills_Details.Unit_Item_Table_Id + ") ";
                        opp.insert_me(mysql);

                        decimal OLD_Larg_Qnty = 0;
                        decimal OLD_Small_Qnty = 0;
                        decimal OLD_Item_NoQnty = 0;
                        CommonAccounts.SetBalanceWithExpireDate_Tash3ela_Bill(ref opp, 1, bills_Details, 0, 0, 0);


                        //mysql = "  UPDATE Unit_item SET Full_qua = FLOOR((Small_unit * Full_qua + D_Qua) / Small_unit), ";
                        //mysql = mysql + " D_Qua =   (( Small_unit * Full_qua + D_Qua) / Small_unit - FLOOR(( Small_unit * Full_qua + D_Qua) /  Small_unit)) *  Small_unit  ";
                        //mysql = mysql + " WHERE (id = " + Unit_Item_Table_Id + ")  ";

                        mysql = "  UPDATE Unit_item SET Full_qua = FLOOR((Small_unit * Full_qua + D_Qua) / Small_unit), ";
                        mysql = mysql + " D_Qua =    (Small_unit * Full_qua + D_Qua) % Small_unit  ";
                        mysql = mysql + " WHERE (id = " + bills_Details.Unit_Item_Table_Id + ")  ";

                        opp.insert_me(mysql);
                    }
                    ////////////// ÕœÌÀ «Œ— ”⁄— «·‘—«¡
                    ///

                    UpdatePurchasePrice(ref opp, bills_Details);


                }

                ////////// set the work on database
                if (Common.IsSetSuppliers_AccountsForBills == 1)
                {
                    Suppliers_AccountsCls ca = new Suppliers_AccountsCls();
                    //ca.Set_SuppliersAccount_Balance(ref Excp, Supplier_Id, New_Invoice_Id, Net_Total, Notes);
                    ca.Set_Supplier_Balance_MakeBill(ref Excp, ref opp, invL_O.invL.Supplier_Id ?? 0, invL_O.invL.Id, invL_O.invL.Net_Total ?? 0, invL_O.invL.Notes);
                    ////////// order estlam automatic
                    if (Common.Make_Store_Order_Sarf_Estlam_Automatic_From_Invoice == 1 & invL_O.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                    {
                        Order_Details_AutomaticCls oo = new Order_Details_AutomaticCls();
                        oo.Make_Order_From_Bill_Details(ref Excp, ref opp, invD_O.invDlst, invL_O.invL.Id, invL_O.invL.Supplier_Id ?? 0, invL_O.invL.Invoice_Date.Value, "›« Ê—… „‘ —Ì« ");
                        Msg = " „ ≈’œ«— ≈–‰ «·≈” ·«„ „‰ «·„Ê—œ ≈·Ï «·„Œ“‰";
                    }

                    //«‰‘«¡ «·ﬁÌœ
                    Account_Entry_Invoice_CLS aa = new Account_Entry_Invoice_CLS();
                    aa.IsNotDeletePrevEntry = true;
                    aa.Make_EntryFor_Bills(ref Excp, ref Msg, ref opp, invL_O.invL.Id, invL_O.invL.Invoice_Date.Value, invL_O.invL.Invoice_Date.Value, -1);


                    if (invL_O.invL.Is_Invoice_Agile == 0)
                    {
                        if (Common.MakeOrderMoneyFromBill_AutomaticWithoutForm == 1)
                        {

                            decimal PaidValueFromMarket = invL_O.invL.Net_Total ?? 0;
                            decimal RestValue = 0;
                            int Box_Id = 0;
                            Money_Transaction_BoxOrderReportCLS mo = new Money_Transaction_BoxOrderReportCLS();
                            //, long CarsWorkOrdersId, long Reference_Type_Table_Id, long DataDictionaryProcessesListsId

                            long CarsWorkOrdersId = 0;
                            long Reference_Type_Table_Id = invL_O.invL.Id;
                            long DataDictionaryProcessesListsId = 2;

                            //   ,   CarsWorkOrdersId,   Reference_Type_Table_Id,   DataDictionaryProcessesListsId


                            long Entry_No = 0;


                            mo.MakeMoney_TransactionFromBill(ref Excp, ref Msg, ref opp, invL_O.invL.Invoice_Date.Value, invL_O.invL.Invoice_Id ?? 0, New_Bills_List_Id, Box_Id, invL_O.invL.Currency_id ?? 0, invL_O.invL.Supplier_Id ?? 0, PaidValueFromMarket, RestValue, 0, Entry_No, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);
                        }
                    }
                }

                try
                {
                    foreach (var s in invL_O.salesList)
                    {
                        try
                        {
                            var existingEntity = opp.db.Unit_Item_SalesDates.Where(x => x.Id == s.Id).FirstOrDefault();

                            if (existingEntity != null)
                            {

                                s.Bills_ListId = invL_O.invL.Id;
                                s.IsProcessedAsBill = 1;
                                opp.db.Entry(existingEntity).CurrentValues.SetValues(s);
                                opp.db.SaveChanges();

                            }

                            //opp.db.Entry(s).CurrentValues.SetValues(updatedSale);


                            //opp.db.Entry(s).State = EntityState.Modified;
                        }
                        catch (Exception exx)
                        {

                        }
                    }
                }
                catch (Exception eee)
                {

                }


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
        public void Make_Bill_Update_SupplierAccount(ref int Excp, ref string Msg, BillOperationCLS_New invD_O, BillOperationListCLS_New invL_O)
        {

            AccountModelsLib.operations opp = new AccountModelsLib.operations(1);
            try
            {
                //checked item_report to SendKeys to store
                DataView dvToStore = new DataView();
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
                ArrayList mArrMake_Order = new ArrayList();
                //mArrMake_Order = it.ArrItemToStore;
                //if (!it.BtnOkClick)
                //{
                //    Excp = 10;
                //    return;
                //}
                ////***********************************************************************
                ////////// Make_New_Invoice_Id

                DataTable dt = new DataTable();
                //dt = Invoice_ds.Tables[0];

                string mysql = "";
                //decimal Total = 0;
                //New_Bills_List_Id = invL.Insert_Bill_List(ref opp, ref Excp);
                New_Bills_List_Id = invL_O.Insert_Bill_List(ref opp, ref Excp, (Bills_List)invL_O.invL);

                if (Excp == 1)
                {
                    opp.Rollback();
                    return;
                }



                ///////////// add invoice Items and update request qnty


                //long Bills_List_Id_ = New_Bill_List_Id;

                //string Supplier_Name = "";

                //try
                //{

                //    Supplier_Name = Common.GetSupplierName(ref opp, invL_O.invL.Supplier_Id.Value);
                //}
                //catch { }

                InvoiceDataSetCLS ii = new InvoiceDataSetCLS();
                //////////// add item in invoice details
                for (int x = 0; x < invD_O.invDlst.Count; x++)
                {

                    //BillOperationCLS invd = new BillOperationCLS();
                    try
                    {
                        invD_O.invDlst[x].Store_Name = Common.GetStoreName(ref opp, invD_O.invDlst[x].Store_Id ?? 0);
                    }
                    catch { }

                    invD_O.invDlst[x].Invoice_Id = New_Bills_List_Id;
                    invD_O.invDlst[x].Item_ProcessedFlage = 100;
                    invD_O.invDlst[x].Item_DeletedFlage = 100;

                    //try
                    //{
                    var serializedParent = JsonConvert.SerializeObject(invD_O.invDlst[x]);
                    Bills_Details bills_Details = JsonConvert.DeserializeObject<Bills_Details>(serializedParent);


                    //Invoices_Details invoices_Details = (Invoices_Details)invD_O.invDlst[x];
                    invD_O.Invoices_Details_Id = invD_O.Insert_Invoices_Details(ref opp, ref Excp, bills_Details);
                    invD_O.invDlst[x].Id = bills_Details.Id;

                    //invd.Get_Item_Details(Invoice_ds, x);


                    //invd.Bills_Details_Id = invd.Insert_Bills_Details(ref opp, ref Excp);
                    if (Excp == 1)
                    {
                        opp.Rollback();
                        return;
                    }

                    //Invoice_ds.Tables[0].Rows[x]["Invoices_Details_Id"] = invd.Bills_Details_Id;
                    //long Unit_Item_ExpireDate_Id = 0;
                    int Transaction_Type = 1;
                    int Invoice_Type_Id = Common.Bill_Type_Id;// 3;


                    if (Common.Project_Market == 1 & invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                    {


                        //try
                        //{
                        //    invD_O.Store_Name = Common.GetStoreName(ref opp, bills_Details.Store_Id??0);
                        //}
                        //catch { }

                        //int Type_From = Common.Supplier_StoreTransactionCode;
                        //int Type_To = Common.Store_StoreTransactionCode;
                        //long Id_From = invL.Supplier_Id;
                        //long Id_To = invd.Store_Id;
                        //long StoreId = invd.Store_Id;
                        //string StoreId_Name = invd.Store_Name;
                        // , Type_From,  Type_To,  Id_From,  Id_To,  StoreId,  StoreId_Name


                        //ii.Set_ItemTransactionFromInvoices(ref opp, invd.Unit_Item_Table_Id, invd.Item_Net_Total_Price, invd.Larg_Qnty, invd.Small_Qnty, invd.Qnty_Another, 0, Transaction_Type, invL.Invoice_Date, invL.Bills_List_Id, invd.Bills_Details_Id, invL.Invoice_Type_Id, "›« Ê—… „‘ —Ì« ", invL.Supplier_Name, invd.Store_Name, invL.Supplier_Name, invL.Invoice_No, Type_From, Type_To, Id_From, Id_To, StoreId, StoreId_Name);
                        ii.Set_ItemTransactionFromBill(ref opp, bills_Details, invL_O, 0, Transaction_Type, Common.Bill_Type_Id, "›« Ê—… „‘ —Ì« ");

                    }
                    if (Common.Project_Market == 1 & invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                    {
                        mysql = " UPDATE Unit_item SET Full_qua =Full_qua + " + bills_Details.Larg_Qnty + ", D_Qua =D_Qua + " + bills_Details.Small_Qnty + ", Item_No =Item_No + " + bills_Details.Qnty_Another + " WHERE (id = " + bills_Details.Unit_Item_Table_Id + ") ";
                        opp.insert_me(mysql);

                        //// «·»Ê‰’
                        mysql = " UPDATE Unit_item SET Full_qua =Full_qua + " + bills_Details.LargItemsQnty_Free + ", D_Qua =D_Qua + " + bills_Details.SmallItemsQnty_Free + ", Item_No =Item_No + " + bills_Details.Item_No_Free + " WHERE (id = " + bills_Details.Unit_Item_Table_Id + ") ";
                        opp.insert_me(mysql);

                        decimal OLD_Larg_Qnty = 0;
                        decimal OLD_Small_Qnty = 0;
                        decimal OLD_Item_NoQnty = 0;
                        CommonAccounts.SetBalanceWithExpireDate_Tash3ela_Bill(ref opp, 1, bills_Details, 0, 0, 0);


                        //mysql = "  UPDATE Unit_item SET Full_qua = FLOOR((Small_unit * Full_qua + D_Qua) / Small_unit), ";
                        //mysql = mysql + " D_Qua =   (( Small_unit * Full_qua + D_Qua) / Small_unit - FLOOR(( Small_unit * Full_qua + D_Qua) /  Small_unit)) *  Small_unit  ";
                        //mysql = mysql + " WHERE (id = " + Unit_Item_Table_Id + ")  ";

                        mysql = "  UPDATE Unit_item SET Full_qua = FLOOR((Small_unit * Full_qua + D_Qua) / Small_unit), ";
                        mysql = mysql + " D_Qua =    (Small_unit * Full_qua + D_Qua) % Small_unit  ";
                        mysql = mysql + " WHERE (id = " + bills_Details.Unit_Item_Table_Id + ")  ";

                        opp.insert_me(mysql);
                    }
                    ////////////// ÕœÌÀ «Œ— ”⁄— «·‘—«¡
                    ///

                    UpdatePurchasePrice(ref opp, bills_Details);


                }

                ////////// set the work on database
                if (Common.IsSetSuppliers_AccountsForBills == 1)
                {
                    Suppliers_AccountsCls ca = new Suppliers_AccountsCls();
                    //ca.Set_SuppliersAccount_Balance(ref Excp, Supplier_Id, New_Invoice_Id, Net_Total, Notes);
                    ca.Set_Supplier_Balance_MakeBill(ref Excp, ref opp, invL_O.invL.Supplier_Id ?? 0, invL_O.invL.Id, invL_O.invL.Net_Total ?? 0, invL_O.invL.Notes);
                    ////////// order estlam automatic
                    if (Common.Make_Store_Order_Sarf_Estlam_Automatic_From_Invoice == 1 & invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                    {
                        Order_Details_AutomaticCls oo = new Order_Details_AutomaticCls();
                        oo.Make_Order_From_Bill_Details(ref Excp, ref opp, invD_O.invDlst, invL_O.invL.Id, invL_O.invL.Supplier_Id ?? 0, invL_O.invL.Invoice_Date.Value, "›« Ê—… „‘ —Ì« ");
                        Msg = " „ ≈’œ«— ≈–‰ «·≈” ·«„ „‰ «·„Ê—œ ≈·Ï «·„Œ“‰";
                    }

                    //«‰‘«¡ «·ﬁÌœ
                    Account_Entry_Invoice_CLS aa = new Account_Entry_Invoice_CLS();
                    aa.IsNotDeletePrevEntry = true;
                    aa.Make_EntryFor_Bills(ref Excp, ref Msg, ref opp, invL_O.invL.Id, invL_O.invL.Invoice_Date.Value, invL_O.invL.Invoice_Date.Value, -1);


                    if (invL_O.invL.Is_Invoice_Agile == 0)
                    {
                        if (Common.MakeOrderMoneyFromBill_AutomaticWithoutForm == 1)
                        {

                            decimal PaidValueFromMarket = invL_O.invL.Net_Total ?? 0;
                            decimal RestValue = 0;
                            int Box_Id = 0;
                            Money_Transaction_BoxOrderReportCLS mo = new Money_Transaction_BoxOrderReportCLS();
                            //, long CarsWorkOrdersId, long Reference_Type_Table_Id, long DataDictionaryProcessesListsId

                            long CarsWorkOrdersId = 0;
                            long Reference_Type_Table_Id = invL_O.invL.Id;
                            long DataDictionaryProcessesListsId = Common.Bill_Type_Id;

                            //   ,   CarsWorkOrdersId,   Reference_Type_Table_Id,   DataDictionaryProcessesListsId


                            long Entry_No = 0;


                            mo.MakeMoney_TransactionFromBill(ref Excp, ref Msg, ref opp, invL_O.invL.Invoice_Date.Value, invL_O.invL.Invoice_Id ?? 0, New_Bills_List_Id, Box_Id, invL_O.invL.Currency_id ?? 0, invL_O.invL.Supplier_Id ?? 0, PaidValueFromMarket, RestValue, 0, Entry_No, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);
                        }
                    }
                }

                try
                {
                    foreach (var s in invL_O.salesList)
                    {
                        try
                        {
                            var existingEntity = opp.db.Unit_Item_SalesDates.Where(x => x.Id == s.Id).FirstOrDefault();

                            if (existingEntity != null)
                            {

                                s.Bills_ListId = invL_O.invL.Id;
                                s.IsProcessedAsBill = 1;
                                opp.db.Entry(existingEntity).CurrentValues.SetValues(s);
                                opp.db.SaveChanges();

                            }

                            //opp.db.Entry(s).CurrentValues.SetValues(updatedSale);


                            //opp.db.Entry(s).State = EntityState.Modified;
                        }
                        catch (Exception exx)
                        {

                        }
                    }
                }
                catch (Exception eee)
                {

                }


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
        public void UpdatePurchasePrice(ref AccountModelsLib.operations opp, BillOperationCLS invd)
        {
            Items itm = opp.db.Item.Where(i => i.id == invd.Item_Id).FirstOrDefault();
            if (itm != null)
            {
                if (itm.S_Price > invd.Audience_price)
                {
                    return;
                }
            }
            string mysql = "";
            string UpDate_Audience_price = "";
            // invd.Audience_price
            if (Common.is_pharmacy == 1)
            {
                UpDate_Audience_price = "   S_Price =" + invd.Audience_price + " , S_price_LargeUnit= " + invd.Audience_price + " ,";
            }
            if (Common.Is_Update_Price_On_Bill == 1)
            {
                mysql = " UPDATE  Item SET " + UpDate_Audience_price + " P_Price =  " + invd.Larg_Unit_Price + " WHERE (id = " + invd.Item_Id + ") ";
                opp.insert_me(mysql);
                mysql = " UPDATE  Unit_Item  SET " + UpDate_Audience_price + " P_Price =  " + invd.Larg_Unit_Price + " WHERE (Item_Id = " + invd.Item_Id + ") ";
                opp.insert_me(mysql);

                if (invd.Unit_Used_for_Price == 1)
                {
                    mysql = " UPDATE  Item SET  " + UpDate_Audience_price + "P_price_LargeUnit =  " + invd.Larg_Unit_Price + ",Unit_Used_For_P_price_Default=" + invd.Unit_Used_for_Price + " WHERE (id = " + invd.Item_Id + ") ";
                    opp.insert_me(mysql);
                    mysql = " UPDATE  Unit_Item SET  " + UpDate_Audience_price + "P_price_LargeUnit =  " + invd.Larg_Unit_Price + " ,Unit_Used_For_P_price_Default=" + invd.Unit_Used_for_Price + "  WHERE (Item_Id = " + invd.Item_Id + ") ";
                    opp.insert_me(mysql);
                }
                if (invd.Unit_Used_for_Price == 2)
                {

                    mysql = " UPDATE  Item SET  " + UpDate_Audience_price + "P_price_SmallUnit =  " + invd.Larg_Unit_Price + ",Unit_Used_For_P_price_Default=" + invd.Unit_Used_for_Price + " WHERE (id = " + invd.Item_Id + ") ";
                    opp.insert_me(mysql);
                    mysql = " UPDATE  Unit_Item SET  " + UpDate_Audience_price + "P_price_SmallUnit =  " + invd.Larg_Unit_Price + " ,Unit_Used_For_P_price_Default=" + invd.Unit_Used_for_Price + "  WHERE (Item_Id = " + invd.Item_Id + ") ";
                    opp.insert_me(mysql);
                }
                if (invd.Unit_Used_for_Price == 3)
                {
                    mysql = " UPDATE  Item SET  " + UpDate_Audience_price + "P_price_Item_No =  " + invd.Larg_Unit_Price + ",Unit_Used_For_P_price_Default=" + invd.Unit_Used_for_Price + " WHERE (id = " + invd.Item_Id + ") ";
                    opp.insert_me(mysql);
                    mysql = " UPDATE  Unit_Item SET  " + UpDate_Audience_price + "P_price_Item_No =  " + invd.Larg_Unit_Price + " ,Unit_Used_For_P_price_Default=" + invd.Unit_Used_for_Price + "  WHERE (Item_Id = " + invd.Item_Id + ") ";
                    opp.insert_me(mysql);
                }
            }
        }
        void UpdatePurchasePrice(ref AccountModelsLib.operations opp, IBills_Details invd)
        {
            Items itm = opp.db.Item.Where(i => i.id == invd.Item_Id).FirstOrDefault();
            if (itm != null)
            {
                if (itm.S_Price > invd.Audience_price)
                {
                    return;
                }
            }
            string mysql = "";
            string UpDate_Audience_price = "";
            // invd.Audience_price
            if (Common.is_pharmacy == 1)
            {
                UpDate_Audience_price = "   S_Price =" + invd.Audience_price + " , S_price_LargeUnit= " + invd.Audience_price + " ,";
            }
            if (Common.Is_Update_Price_On_Bill == 1)
            {
                mysql = " UPDATE  Item SET " + UpDate_Audience_price + " P_Price =  " + invd.Larg_Unit_Price + " WHERE (id = " + invd.Item_Id + ") ";
                opp.insert_me(mysql);
                mysql = " UPDATE  Unit_Item  SET " + UpDate_Audience_price + " P_Price =  " + invd.Larg_Unit_Price + " WHERE (Item_Id = " + invd.Item_Id + ") ";
                opp.insert_me(mysql);

                if (invd.Unit_Used_for_Price == 1)
                {
                    mysql = " UPDATE  Item SET  " + UpDate_Audience_price + "P_price_LargeUnit =  " + invd.Larg_Unit_Price + ",Unit_Used_For_P_price_Default=" + invd.Unit_Used_for_Price + " WHERE (id = " + invd.Item_Id + ") ";
                    opp.insert_me(mysql);
                    mysql = " UPDATE  Unit_Item SET  " + UpDate_Audience_price + "P_price_LargeUnit =  " + invd.Larg_Unit_Price + " ,Unit_Used_For_P_price_Default=" + invd.Unit_Used_for_Price + "  WHERE (Item_Id = " + invd.Item_Id + ") ";
                    opp.insert_me(mysql);
                }
                if (invd.Unit_Used_for_Price == 2)
                {

                    mysql = " UPDATE  Item SET  " + UpDate_Audience_price + "P_price_SmallUnit =  " + invd.Larg_Unit_Price + ",Unit_Used_For_P_price_Default=" + invd.Unit_Used_for_Price + " WHERE (id = " + invd.Item_Id + ") ";
                    opp.insert_me(mysql);
                    mysql = " UPDATE  Unit_Item SET  " + UpDate_Audience_price + "P_price_SmallUnit =  " + invd.Larg_Unit_Price + " ,Unit_Used_For_P_price_Default=" + invd.Unit_Used_for_Price + "  WHERE (Item_Id = " + invd.Item_Id + ") ";
                    opp.insert_me(mysql);
                }
                if (invd.Unit_Used_for_Price == 3)
                {
                    mysql = " UPDATE  Item SET  " + UpDate_Audience_price + "P_price_Item_No =  " + invd.Larg_Unit_Price + ",Unit_Used_For_P_price_Default=" + invd.Unit_Used_for_Price + " WHERE (id = " + invd.Item_Id + ") ";
                    opp.insert_me(mysql);
                    mysql = " UPDATE  Unit_Item SET  " + UpDate_Audience_price + "P_price_Item_No =  " + invd.Larg_Unit_Price + " ,Unit_Used_For_P_price_Default=" + invd.Unit_Used_for_Price + "  WHERE (Item_Id = " + invd.Item_Id + ") ";
                    opp.insert_me(mysql);
                }
            }
        }
        public void Make_Bills_List_Reverse_Update_SupplierAccount(ref int Excp,ref string Msg, BillOperationCLS_New invD_O, BillOperationListCLS_New invL_O)
        {
            InvoiceDataSetCLS ii = new InvoiceDataSetCLS();

            DataSet New_Items_ds_Add = new DataSet();
            DataSet New_Items_ds_Minus = new DataSet();
            New_Items_ds_Add = ii.MakeInvoiceDataSet();
            New_Items_ds_Minus = ii.MakeInvoiceDataSet();


            decimal OLD_Larg_Qnty = 0;
            decimal OLD_Small_Qnty = 0;
            decimal OLD_Item_No = 0;
            AccountModelsLib.operations opp = new AccountModelsLib.operations(1);
            try
            {
                //checked item_report to SendKeys to store
                DataView dvToStore = new DataView();
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
                ArrayList mArrMake_Order = new ArrayList();
                //mArrMake_Order = it.ArrItemToStore;
                //if (!it.BtnOkClick)
                //{
                //    Excp = 10;
                //    return;
                //}
                ////***********************************************************************
                ////////// Make_New_Invoice_Id

                DataTable dt = new DataTable();
                //dt = Invoice_ds.Tables[0];

                string mysql = "";
                //decimal Total = 0;




                InvoicesClss inv = new InvoicesClss();

                // Make_New_Bills_List_Reverse_Id(ref Excp, ref opp, ref New_Invoice_Id, Invoice_No, Supplier_Id, Invoice_Date, Invoice_Type, TotalSecond, IsCommission_Value, Commission, CommissionValue, TaxSales10, ATS05, TaxSales10_Value, ATS05_Value, Net_Total, "", Currency_id, 1, Notes, PolicaNo, TroodContainerNo, LoadPayment, TotalSecond, Damaghat, Load_Payment_OnTax, SupplierAccountBefor, SupplierAccountAfter, SubAccount_Id, SubAccount_Name, IS_Make_Sarf_Item_BalanceFromInvoice, Is_InvoiceKta3y);
                New_Bills_List_Id = invL_O.Insert_Bill_List_Reverse(ref opp, ref Excp, (Bills_List_Reverse)invL_O.invL);
                if (Excp == 1)
                {
                    opp.Rollback();
                    return;
                }

                //decimal Total_CommissionValue = Total_Item_CommissionValue + CommissionValue;

                ///////////////////Set All Commision

                //mysql = " UPDATE Bills_List_Reverse SET Total_Item_CommissionValue=" + Total_Item_CommissionValue + ",Total_CommissionValue=    " + Total_CommissionValue + " WHERE (id = " + New_Bill_List_Id + ") ";
                //opp.insert_me(mysql);
                ///////////////////////////////////////////////////////////////////////////////



                ///////////// add invoice Items and update request qnty

                ArrayList arr = new ArrayList();
                //long Bills_List_Id_ = New_Bills_List_Id;
                string Supplier_Name = "";

                try
                {
                    Supplier_Name = Common.GetSupplierName(ref opp, invL_O.invL.Supplier_Id ?? 0);

                }
                catch { }


                //////////// add item in invoice details

                foreach (var item in invD_O.invDlst)
                {


                    //BillOperationCLS invD = new BillOperationCLS();
                    //invD.Bills_List_Id = New_Bills_List_Id;

                    //invD.Get_Item_Details(Invoice_ds, x);


                    //Invoice_ds.Tables[0].Rows[x]["Item_ProcessedFlage"] = "100";
                    item.Invoice_Id = New_Bills_List_Id;
                    item.Item_ProcessedFlage = 100;
                    item.Item_DeletedFlage = 100;
                    try
                    {
                        item.Store_Name = Common.GetStoreName(ref opp, item.Store_Id ?? 0);
                    }
                    catch { }

                    item.Invoice_Id = invL_O.invL.Id;
                    var serializedParent = JsonConvert.SerializeObject(item);
                    Bills_Details_Reverse bills_Details = JsonConvert.DeserializeObject<Bills_Details_Reverse>(serializedParent);
                     

                    item.Id = invD_O.Insert_Bills_Details_Reverse(ref opp, ref Excp, bills_Details);


                    int Transaction_Type = 2;
                    int Invoice_Type_Id = Common.BilleReverse_Type_Id;
                    if (Common.Project_Market == 1 & invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                    {

                        long Invoices_Details_Id_ = item.Id;
                        //InvoiceDataSetCLS ii = new InvoiceDataSetCLS();

                        //ii.Set_ItemTransactionFromInvoices(ref opp, invd.Unit_Item_Table_Id, invd.Item_Net_Total_Price, invd.Larg_Qnty, invd.Small_Qnty, invd.Qnty_Another, 0, Transaction_Type, invL.Invoice_Date, Bills_List_Id_, invd.Bills_Details_Id, invL.Invoice_Type_Id, "„— Ã⁄ „‘ —Ì« ", invL.Supplier_Name, invd.Store_Name, invL.Supplier_Name, invL.Invoice_No);
                        ii.Set_ItemTransactionFromBill(ref opp, item, invL_O, 0, Transaction_Type, Common.BilleReverse_Type_Id, "„— Ã⁄ „‘ —Ì« ");

                    }
                    if (Common.Project_Market == 1 & invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                    {
                        mysql = " UPDATE Unit_item SET Full_qua =Full_qua - " + item.Larg_Qnty + ", D_Qua =D_Qua - " + item.Small_Qnty + ", Item_No =Item_No - " + item.Qnty_Another + " WHERE (id = " + item.Unit_Item_Table_Id + ") ";
                        opp.insert_me(mysql);
                        //arr.Add(mysql);

                        //// «·»Ê‰’
                        mysql = " UPDATE Unit_item SET Full_qua =Full_qua - " + item.LargItemsQnty_Free + ", D_Qua =D_Qua - " + item.SmallItemsQnty_Free + ", Item_No =Item_No - " + item.Item_No_Free + " WHERE (id = " + item.Unit_Item_Table_Id + ") ";
                        opp.insert_me(mysql);

                        CommonAccounts.SetBalanceWithExpireDate_Tash3ela_Bill(ref opp, 2, invD_O.invD, 0, 0, 0, true);

                        //mysql = "  UPDATE Unit_item SET Full_qua = FLOOR((Small_unit * Full_qua + D_Qua) / Small_unit), ";
                        //mysql = mysql + " D_Qua =   (( Small_unit * Full_qua + D_Qua) / Small_unit - FLOOR(( Small_unit * Full_qua + D_Qua) /  Small_unit)) *  Small_unit  ";
                        //mysql = mysql + " WHERE (id = " + Unit_Item_Table_Id + ")  ";

                        mysql = "  UPDATE Unit_item SET Full_qua = FLOOR((Small_unit * Full_qua + D_Qua) / Small_unit), ";
                        mysql = mysql + " D_Qua =    (Small_unit * Full_qua + D_Qua) % Small_unit  ";
                        mysql = mysql + " WHERE (id = " + item.Unit_Item_Table_Id + ")  ";

                        opp.insert_me(mysql);
                        //arr.Add(mysql);
                    }

                    //Invoice_ds.Tables[0].Rows[x]["Supplier_Id"] = invL.Supplier_Id;

                    #region order sarf automatic
                    //if (Common.Make_Store_Order_Sarf_Estlam_Automatic_From_Invoice == 1)
                    //{



                    //    if (item.Item_DeletedFlage==1)
                    //    {
                    //        //Õ–›
                    //        Fill_Ds(ref opp, ref New_Items_ds_Minus, item);



                    //    }
                    //    else if (item.Item_DeletedFlage == -1)
                    //    {//«÷«›…

                    //        Fill_Ds(ref opp, ref New_Items_ds_Add, item);
                    //    }
                    //    else
                    //    {

                    //        Larg_Qnty = Larg_Qnty - OLD_Larg_Qnty;
                    //        Small_Qnty = Small_Qnty - Small_Qnty;
                    //        Qnty_Another = Qnty_Another - OLD_Item_No;

                    //        decimal Larg_Qnty__ = decimal.Parse(Larg_Qnty.ToString());
                    //        decimal Small_Qnty__ = decimal.Parse(Small_Qnty.ToString());
                    //        decimal Small_Unit_Price__ = decimal.Parse(Small_Unit_Price.ToString());
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
                    //            Fill_Ds(ref opp, ref New_Items_ds_Add, item, Larg_Qnty__, Small_Qnty__);
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
                    //            Fill_Ds(ref opp, ref New_Items_ds_Minus, item, Larg_Qnty__, Small_Qnty__);
                    //        }

                    //    }
                    //}




                    #endregion
                }

                ////////// set the work on database


                ////////// order estlam automatic
                if (Common.Make_Store_Order_Sarf_Estlam_Automatic_From_Invoice == 1)
                {
                    Order_Details_AutomaticCls oo = new Order_Details_AutomaticCls();
                    oo.Make_Order_From_Bills_Details_Reverse(ref Excp, ref opp, invD_O.invDlst, New_Bills_List_Id, invL_O.invL.Supplier_Id ?? 0, invL_O.invL.Invoice_Date.Value, "›« Ê—… „— Ã⁄ „‘ —Ì« ");
                    Msg = " „ ≈’œ«— ≈–‰ «·’—› („— Ã⁄ ≈” ·«„) ≈·Ï «·„Ê—œ";
                }

                if (Common.IsSetSuppliers_AccountsForBills == 1)
                {
                    Suppliers_AccountsCls ca = new Suppliers_AccountsCls();
                    ca.Set_Supplier_Balance_MakeBill_Reverse(ref Excp, ref opp, invL_O.invL.Supplier_Id ?? 0, invL_O.invL.Id, invL_O.invL.Net_Total ?? 0, invL_O.invL.Notes);

                    //«‰‘«¡ «·ﬁÌœ
                   
                    Account_Entry_Invoice_CLS aa = new Account_Entry_Invoice_CLS();
                    aa.IsNotDeletePrevEntry = true;
                    aa.Make_EntryFor_Bills_Reverse(ref Excp, ref Msg, ref opp, invL_O.invL.Id, invL_O.invL.Invoice_Date.Value, invL_O.invL.Invoice_Date.Value, -1);


                    if (invL_O.invL.Is_Invoice_Agile == 0)
                    {
                        if (Common.MakeOrderMoneyFromBills_Reverse_AutomaticWithoutForm == 1)
                        {

                            decimal PaidValueFromMarket = invL_O.invL.Net_Total ?? 0;
                            decimal RestValue = 0;
                            int Box_Id = 0;
                            Money_Transaction_BoxOrderReportCLS mo = new Money_Transaction_BoxOrderReportCLS();


                            //, long CarsWorkOrdersId, long Reference_Type_Table_Id, long DataDictionaryProcessesListsId

                            long CarsWorkOrdersId = 0;
                            long Reference_Type_Table_Id = invL_O.invL.Id;
                            long DataDictionaryProcessesListsId = Common.BilleReverse_Type_Id;

                            //   ,   CarsWorkOrdersId,   Reference_Type_Table_Id,   DataDictionaryProcessesListsId
                            long Entry_No = 0;

                            //mo.MakeMoney_TransactionFromBill(ref Excp, ref Msg, ref opp, invL.Invoice_Date, invL.Invoice_No, New_Bills_List_Id, Box_Id, invL.Currency_id, invL.Supplier_Id, PaidValueFromMarket, RestValue, 1, Entry_No, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);
                            mo.MakeMoney_TransactionFromBill_Reverse(ref Excp, ref Msg, ref opp, invL_O.invL.Invoice_Date.Value, invL_O.invL.Invoice_Id ?? 0, New_Bills_List_Id, Box_Id, invL_O.invL.Currency_id ?? 1, invL_O.invL.Supplier_Id ?? 0, PaidValueFromMarket, RestValue, 1, Entry_No, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);
                        }
                    }
                }
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
                //if (mysql_Arr.Count > 0)
                //{
                //    AccountModelsLib.operations opp2 = new AccountModelsLib.operations(1);
                //    try
                //    {
                //        mysql = "";
                //        for (int y = 0; y < mysql_Arr.Count; y++)
                //        {
                //            mysql = mysql + " " + mysql_Arr[y].ToString();

                //        }
                //        opp2.insert_me(mysql);
                //        opp2.Commit();
                //    }
                //    catch (Exception ee) { opp2.Rollback(); }
                //}
            }
            catch (Exception ee)
            {
                Excp = 1;
                opp.Rollback();
            }
        }
        public void Edit_Bill_Update_Supplier_Account(ref int Excp,ref string Msg, BillOperationCLS_New invD_O, BillOperationListCLS_New invL_O)
        {
            InvoiceDataSetCLS ids = new InvoiceDataSetCLS();
            DataSet New_Items_ds_Add = new DataSet();
            DataSet New_Items_ds_Minus = new DataSet();
            New_Items_ds_Add = ids.MakeInvoiceDataSet();
            New_Items_ds_Minus = ids.MakeInvoiceDataSet();

            decimal OLD_Larg_Qnty = 0;
            decimal OLD_Small_Qnty = 0;
            decimal OLD_Item_No = 0;
            AccountModelsLib.operations opp = new AccountModelsLib.operations(1);
            try
            {


                string mysql = "";

                try
                {

                    //invL_O.invL.Supplier_Name = Common.GetSupplierName(ref opp, invL_O.invL.Supplier_Id ?? 0);

                }
                catch { }


                InvoiceDataSetCLS ii = new InvoiceDataSetCLS();

                foreach (var item in invD_O.invDlst)
                {

                    //invD_O.Bills_List_Id = invL_O.invL.Id;
                    //invD_O.Get_Item_Details(Invoice_ds, x);

                    ////«·—’Ìœ «·”«»ﬁ
                    DataSet dsN = opp.return_dataset("Select * from Bills_Details Where Id=" + item.Id + "");
                    if (dsN.Tables[0].Rows.Count > 0)
                    {
                        OLD_Larg_Qnty = decimal.Parse(dsN.Tables[0].Rows[0]["Larg_Qnty"].ToString());
                        OLD_Small_Qnty = decimal.Parse(dsN.Tables[0].Rows[0]["Small_Qnty"].ToString());
                        OLD_Item_No = decimal.Parse(dsN.Tables[0].Rows[0]["Qnty_Another"].ToString());

                        // Item_No_Free, LargItemsQnty_Free, SmallItemsQnty_Free
                        decimal OLD_LargItemsQnty_Free = decimal.Parse(dsN.Tables[0].Rows[0]["LargItemsQnty_Free"].ToString());
                        decimal OLD_SmallItemsQnty_Free = decimal.Parse(dsN.Tables[0].Rows[0]["SmallItemsQnty_Free"].ToString());
                        decimal OLD_Item_No_Free = decimal.Parse(dsN.Tables[0].Rows[0]["Item_No_Free"].ToString());

                        OLD_Larg_Qnty = OLD_Larg_Qnty + OLD_LargItemsQnty_Free;
                        OLD_Small_Qnty = OLD_Small_Qnty + OLD_SmallItemsQnty_Free;
                        OLD_Item_No = OLD_Item_No + OLD_Item_No_Free;

                    }

                    int Transaction_Type = 1;
                    //string Item_ProcessedFlage = Invoice_ds.Tables[0].Rows[x]["Item_ProcessedFlage"].ToString();
                    //if (item.Item_ProcessedFlage == 1)
                    //{
                    //    mysql = "Delete From Bills_Details  where id=" + item.Id;
                    //    opp.insert_me(mysql);

                    //    ii.Delete_ItemTransactionFromInvoices(ref opp, item.Unit_Item_Table_Id ?? 0, invL_O.invL.Id, item.Id, 0, Common.Bill_Type_Id);
                    //    if (Common.Project_Market == 1 & invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                    //    {

                    //        //Õ–›
                    //        mysql = " UPDATE Unit_item SET Full_qua =Full_qua - " + item.Larg_Qnty + ", D_Qua =D_Qua - " + item.Small_Qnty + ", Item_No =Item_No - " + item.Qnty_Another + " WHERE (id = " + item.Unit_Item_Table_Id + ") ";
                    //        opp.insert_me(mysql);

                    //        //«·»Ê‰’
                    //        mysql = " UPDATE Unit_item SET Full_qua =Full_qua - " + item.LargItemsQnty_Free + ", D_Qua =D_Qua - " + item.SmallItemsQnty_Free + ", Item_No =Item_No - " + item.Item_No_Free + " WHERE (id = " + item.Unit_Item_Table_Id + ") ";
                    //        opp.insert_me(mysql);
                    //        CommonAccounts.SetBalanceWithExpireDate_Tash3ela_Bill(ref opp, 2, item, OLD_Larg_Qnty, OLD_Small_Qnty, OLD_Item_No);

                    //    }
                    //}
                    if (item.Item_ProcessedFlage == 0)
                    {
                        item.Invoice_Id = invL_O.invL.Id;
                        item.Item_ProcessedFlage = 100;
                        item.Item_DeletedFlage = 100;

                        var serializedParent = JsonConvert.SerializeObject(item);
                        Bills_Details bills_Details = JsonConvert.DeserializeObject<Bills_Details>(serializedParent);

                      
                        item.Id= invD_O.Insert_Bills_Details(ref opp, ref Excp, bills_Details);

                        
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
                                item.Store_Name = Common.GetStoreName(ref opp, item.Store_Id ?? 0);
                            }
                            catch { }

                            //ii.Set_ItemTransactionFromInvoices(ref opp, invd.Unit_Item_Table_Id, invd.Item_Net_Total_Price, invd.Larg_Qnty, invd.Small_Qnty, invd.Qnty_Another, 0, Transaction_Type, invL.Invoice_Date, invL.Bills_List_Id, invd.Bills_List_Id, invL.Invoice_Type_Id, "›« Ê—… „‘ —Ì« ", invd.Store_Name, invL.Supplier_Name, invL.Supplier_Name, invL.Invoice_No);
                            ii.Set_ItemTransactionFromBill(ref opp, item, invL_O, 0, Transaction_Type, Common.Bill_Type_Id, "›« Ê—… „‘ —Ì« ");


                            //«÷«›…
                            mysql = " UPDATE Unit_item SET Full_qua =Full_qua + " + item.Larg_Qnty + ", D_Qua =D_Qua + " + item.Small_Qnty + ", Item_No =Item_No + " + item.Qnty_Another + " WHERE (id = " + item.Unit_Item_Table_Id + ") ";
                            opp.insert_me(mysql);
                            //«·»Ê‰’
                            mysql = " UPDATE Unit_item SET Full_qua =Full_qua + " + item.LargItemsQnty_Free + ", D_Qua =D_Qua + " + item.SmallItemsQnty_Free + ", Item_No =Item_No + " + item.Item_No_Free + " WHERE (id = " + item.Unit_Item_Table_Id + ") ";
                            opp.insert_me(mysql);

                            UpdatePurchasePrice(ref opp, item);
                            CommonAccounts.SetBalanceWithExpireDate_Tash3ela_Bill(ref opp, 1, item, OLD_Larg_Qnty, OLD_Small_Qnty, OLD_Item_No);


                        }

                        #region order sarf automatic
                        if (Common.Make_Store_Order_Sarf_Estlam_Automatic_From_Invoice == 1 & invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                        {

                            //«÷«›…

                                Fill_Ds(ref opp, ref New_Items_ds_Add, item);
                             
                            

                        }
                        #endregion
                    }
                    if (item.Item_ProcessedFlage == -1)
                    {
                        item.Item_DeletedFlage = 100;
                        item.Item_ProcessedFlage = 100;
                        invD_O.Update_Bills_Details(ref opp, ref Excp, (Bills_Details)item);
                        if (Excp == 1)
                        {
                            opp.Rollback();
                            return;
                        }

                        if (Common.Project_Market == 1 & invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                        {
                            try
                            {
                                item.Store_Name = Common.GetStoreName(ref opp, item.Store_Id ?? 0);
                            }
                            catch { }

                            //ii.Set_ItemTransactionFromInvoices(ref opp, invd.Unit_Item_Table_Id, invd.Item_Net_Total_Price, invd.Larg_Qnty, invd.Small_Qnty, invd.Qnty_Another, 0, Transaction_Type, invL.Invoice_Date, invL.Bills_List_Id, invd.Bills_Details_Id, invL.Invoice_Type_Id, "›« Ê—… „‘ —Ì« ", invd.Store_Name, invL.Supplier_Name, invL.Supplier_Name, invL.Invoice_No);
                            ii.Set_ItemTransactionFromBill(ref opp, item, invL_O, 0, Transaction_Type, Common.Bill_Type_Id, "›« Ê—… „‘ —Ì« ");


                            decimal Larg_Qnty = item.Larg_Qnty ?? 0 - OLD_Larg_Qnty;
                            decimal Small_Qnty = item.Small_Qnty ?? 0 - OLD_Small_Qnty;
                            decimal Qnty_Another = item.Qnty_Another ?? 0 - OLD_Item_No;

                            mysql = " UPDATE Unit_item SET Full_qua =Full_qua + " + Larg_Qnty + ", D_Qua =D_Qua + " + Small_Qnty + ", Item_No =Item_No + " + Qnty_Another + " WHERE (id = " + item.Unit_Item_Table_Id + ") ";
                            opp.insert_me(mysql);


                            CommonAccounts.SetBalanceWithExpireDate_Tash3ela_Bill(ref opp, 0, item, Larg_Qnty, Small_Qnty, Qnty_Another);

                        }
                        #region order sarf automatic
                    if (Common.Make_Store_Order_Sarf_Estlam_Automatic_From_Invoice == 1 & invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                        {

                            

                                decimal Larg_Qnty = item.Larg_Qnty ?? 0 - OLD_Larg_Qnty;
                                decimal Small_Qnty = item.Small_Qnty ?? 0 - OLD_Small_Qnty;
                                decimal Qnty_Another = item.Qnty_Another ?? 0 - OLD_Item_No;

                                decimal Larg_Qnty__ = decimal.Parse(Larg_Qnty.ToString());
                                decimal Small_Qnty__ = decimal.Parse(Small_Qnty.ToString());
                                decimal Small_Unit_Price__ = decimal.Parse(item.Small_Unit_Price.ToString());
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
                                    Fill_Ds(ref opp, ref New_Items_ds_Add, item, Larg_Qnty__, Small_Qnty__);
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
                                    Fill_Ds(ref opp, ref New_Items_ds_Minus, item, Larg_Qnty__, Small_Qnty__);
                                } 
                        }
#endregion


                    }
                    
                    //Invoice_ds.Tables[0].Rows[x]["Item_ProcessedFlage"] = "100";

                    mysql = "  UPDATE Unit_item SET Full_qua = FLOOR((Small_unit * Full_qua + D_Qua) / Small_unit), ";
                    mysql = mysql + " D_Qua =    (Small_unit * Full_qua + D_Qua) % Small_unit  ";
                    mysql = mysql + " WHERE (id = " + item.Unit_Item_Table_Id + ")  ";

                    opp.insert_me(mysql);


                    UpdatePurchasePrice(ref opp, item);

                  
                }
                foreach (var item in invD_O.invDlst_Deleted)
                {



                    int Transaction_Type = 1;
                    //string Item_ProcessedFlage = Invoice_ds.Tables[0].Rows[x]["Item_ProcessedFlage"].ToString();
                    if (item.Item_ProcessedFlage == 1)
                    {
                        mysql = "Delete From Bills_Details  where id=" + item.Id;
                        opp.insert_me(mysql);

                        ii.Delete_ItemTransactionFromInvoices(ref opp, item.Unit_Item_Table_Id ?? 0, invL_O.invL.Id, item.Id, 0, Common.Bill_Type_Id);
                        if (Common.Project_Market == 1 & invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                        {

                            //Õ–›
                            mysql = " UPDATE Unit_item SET Full_qua =Full_qua - " + item.Larg_Qnty + ", D_Qua =D_Qua - " + item.Small_Qnty + ", Item_No =Item_No - " + item.Qnty_Another + " WHERE (id = " + item.Unit_Item_Table_Id + ") ";
                            opp.insert_me(mysql);

                            //«·»Ê‰’
                            mysql = " UPDATE Unit_item SET Full_qua =Full_qua - " + item.LargItemsQnty_Free + ", D_Qua =D_Qua - " + item.SmallItemsQnty_Free + ", Item_No =Item_No - " + item.Item_No_Free + " WHERE (id = " + item.Unit_Item_Table_Id + ") ";
                            opp.insert_me(mysql);
                            CommonAccounts.SetBalanceWithExpireDate_Tash3ela_Bill(ref opp, 2, item, OLD_Larg_Qnty, OLD_Small_Qnty, OLD_Item_No);

                        }
                    }

                    
                    //Invoice_ds.Tables[0].Rows[x]["Item_ProcessedFlage"] = "100";

                    mysql = "  UPDATE Unit_item SET Full_qua = FLOOR((Small_unit * Full_qua + D_Qua) / Small_unit), ";
                    mysql = mysql + " D_Qua =    (Small_unit * Full_qua + D_Qua) % Small_unit  ";
                    mysql = mysql + " WHERE (id = " + item.Unit_Item_Table_Id + ")  ";

                    opp.insert_me(mysql);




                    #region order sarf automatic
                    if (Common.Make_Store_Order_Sarf_Estlam_Automatic_From_Invoice == 1 & invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                    {

                        if (item.Item_DeletedFlage == 1)
                        {
                            //Õ–›
                            Fill_Ds(ref opp, ref New_Items_ds_Minus, item);

                        }

                    }
                    #endregion
                }




                
                ArrayList mysql_Arr = new ArrayList();
                if (Common.Make_Store_Order_Sarf_Estlam_Automatic_From_Invoice == 1 & invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                {


                    Order_Details_AutomaticCls oo = new Order_Details_AutomaticCls();
                    bool MakeOrderOk = false;
                    if (New_Items_ds_Add.Tables[0].Rows.Count > 0)
                    {
                        oo.Make_Order_From_Edit_Bill_Details(ref Excp, ref mysql_Arr, ref opp, ref arr_Order_Purshing_Id_Automatic, ref MakeOrderOk, New_Items_ds_Add.Tables[0], invL_O.invL.Id, invL_O.invL.Supplier_Id ?? 0, invL_O.invL.Invoice_Date.Value, "›« Ê—… „‘ —Ì« ");
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
                        oo.Make_Order_From_Edit_Bill_Details_Reverse(ref Excp, ref opp, ref mysql_Arr, New_Items_ds_Minus.Tables[0], invL_O.invL.Id, invL_O.invL.Supplier_Id ?? 0, invL_O.invL.Invoice_Date.Value, "›« Ê—… „‘ —Ì« ");
                        if (Excp > 0)
                        {
                            opp.Rollback();
                            return;
                        }
                    }

                }
                ///to delete  the invoice
                int CountToDelete = 0;
                //for (int x = 0; x < Invoice_ds.Tables[0].Rows.Count; x++)
                //{
                //    if (Invoice_ds.Tables[0].Rows[x]["Item_DeletedFlage"].ToString() == "1")
                //    {
                //        CountToDelete++;
                //    }
                //}
                bool IsDeleteBills_List = false;
                List<Bills_Details> bills_DetailsLst = opp.db.Bills_Details.AsNoTracking().Where(i => i.Invoice_Id == invL_O.invL.Id).ToList();

                if (bills_DetailsLst.Count == 0)
                {
                    IsDeleteBills_List = true;
                    mysql = "Delete From Bills_List  where id=" + invL_O.invL.Id;
                    opp.insert_me(mysql);
                    Money_Transaction_BoxOrderReportCLS mo = new Money_Transaction_BoxOrderReportCLS();
                    long Entry_No = 0;
                    invL_O.invL.Order_MoneyId = invL_O.invL.Order_MoneyId ?? 0;
                    if (invL_O.invL.Order_MoneyId > 0)
                    {
                        mo.Delete_Transaction_BanksBox_NewSarf_New(ref Excp, ref Msg, ref opp, invL_O.invL.Order_MoneyId ?? 0, ref Entry_No);
                    }
                }
                else
                {
                    if (Common.IsSetSuppliers_AccountsForBills == 0)
                    {
                        invL_O.invL.Supplier_BalanceSet = 1;
                    }
                    invL_O.Update_Bill_List(ref opp, ref Excp, invL_O.invL);

                }
                //foreach (var item in invD_O.invDlst)
                //{
                //    if (item.Item_DeletedFlage == 1)
                //    {
                //        CountToDelete++;
                //    }

                //}
                ////**********************************
                //foreach (var item in invD_O.invDlst_Deleted)
                //{
                //    if (item.Item_DeletedFlage == 1)
                //    {
                //        CountToDelete++;
                //    }

                //}

                //if (CountToDelete == invD_O.invDlst.Count)
                //{
                //    //IsDeleteBills_List = true ;
                //    //mysql = "Delete From Bills_List  where id=" + invL_O.invL.Id;
                //    //opp.insert_me(mysql);
                //    //Money_Transaction_BoxOrderReportCLS mo = new Money_Transaction_BoxOrderReportCLS();
                //    //long Entry_No = 0;
                //    //mo.Delete_Transaction_BanksBox_NewSarf_New(ref Excp, ref Msg, ref opp, invL_O.invL.Order_MoneyId ?? 0, ref Entry_No);
                //}
                //else
                //{
                //    //if (Common.IsSetSuppliers_AccountsForBills == 0)
                //    //{
                //    //    invL_O.invL.Supplier_BalanceSet = 1;
                //    //}
                //    //invL_O.Update_Bill_List(ref opp, ref Excp, invL_O.invL);
                //}
                //***********************************
                //Bills_List  Bills_Details



                if (Excp == 1)
                {
                    opp.Rollback();
                    return;
                }

                ///////// check customer balance

                //decimal Invoice_NetTotal_Diff = invL_O.invL.Invoice_Net_Total_Old - invL_O.invL.Net_Total;
                if (Common.IsSetSuppliers_AccountsForBills == 1)
                {

                    Suppliers_AccountsCls ca = new Suppliers_AccountsCls();
                    ca.ProcessNo = invL_O.invL.ProcessNo ?? 0;
                    ca.Update_Supplier_BalanceWithEditBill(ref Excp, ref opp, invL_O.invL.Invoice_Date.Value, invL_O.invL.Supplier_Id ?? 0, invL_O.invL.Id, invL_O.invL.Net_Total ?? 0, invL_O.invL.Notes);

                    //«‰‘«¡ «·ﬁÌœ
                    Account_Entry_Invoice_CLS aa = new Account_Entry_Invoice_CLS();
                    aa.IsNotDeletePrevEntry = false;
                    aa.Make_EntryFor_Bills(ref Excp, ref Msg, ref opp, invL_O.invL.Id, invL_O.invL.Invoice_Date.Value, invL_O.invL.Invoice_Date.Value, -1);


                    if (invL_O.invL.Is_Invoice_Agile == 0 && IsDeleteBills_List == false)
                    {
                        if (Common.MakeOrderMoneyFromBill_AutomaticWithoutForm == 1)
                        {

                            decimal PaidValueFromMarket = invL_O.invL.Net_Total ?? 0;
                            decimal RestValue = 0;
                            int Box_Id = 0;
                            Money_Transaction_BoxOrderReportCLS mo = new Money_Transaction_BoxOrderReportCLS();
                            //, long CarsWorkOrdersId, long Reference_Type_Table_Id, long DataDictionaryProcessesListsId

                            long CarsWorkOrdersId = 0;
                            long Reference_Type_Table_Id = invL_O.invL.Id;
                            long DataDictionaryProcessesListsId = 2;

                            //   ,   CarsWorkOrdersId,   Reference_Type_Table_Id,   DataDictionaryProcessesListsId

                            long Entry_No = 0;
                            mo.Delete_Transaction_BanksBox_NewSarf_New(ref Excp, ref Msg, ref opp, invL_O.invL.Order_MoneyId ?? 0, ref Entry_No);
                            mo.MakeMoney_TransactionFromBill(ref Excp, ref Msg, ref opp, invL_O.invL.Invoice_Date.Value, invL_O.invL.Id, invL_O.invL.Id, Box_Id, invL_O.invL.Currency_id ?? 0, invL_O.invL.Supplier_Id ?? 0, PaidValueFromMarket, RestValue, 0, Entry_No, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);
                        }
                    }
                }
                if (Excp == 0)
                {
                    opp.Commit();
                }
                else
                {
                    Excp = 1;
                    opp.Rollback();
                }


                if (mysql_Arr.Count > 0)
                {
                    AccountModelsLib.operations opp2 = new AccountModelsLib.operations(1);
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
                Excp = 0;
            }
            catch (Exception e)
            {
                opp.Rollback();
                Excp = 1;
                Msg = e.Message;
                //throw (e);
            }

        }

        public void Edit_Bill_List_reverse_Update_Supplier_Account(ref int Excp,ref string Msg, BillOperationCLS_New invD_O, BillOperationListCLS_New invL_O)
        {
            InvoiceDataSetCLS ids = new InvoiceDataSetCLS();
            DataSet New_Items_ds_Add = new DataSet();
            DataSet New_Items_ds_Minus = new DataSet();
            New_Items_ds_Add = ids.MakeInvoiceDataSet();
            New_Items_ds_Minus = ids.MakeInvoiceDataSet();


            decimal OLD_Larg_Qnty = 0;
            decimal OLD_Small_Qnty = 0;
            decimal OLD_Item_No = 0;
            AccountModelsLib.operations opp = new AccountModelsLib.operations(1);
            try
            {
                string mysql = "";
                //ArrayList arr = new ArrayList();


                try
                {
                    string Supplier_Name = Common.GetSupplierName(ref opp, invL_O.invL.Supplier_Id ?? 0);

                }
                catch { }

                /////////////////Set All Commision
                //decimal Total_Item_CommissionValue = 0;
                //for (int x = 0; x < dss.Tables[0].Rows.Count; x++)
                //{
                //    Total_Item_CommissionValue = Total_Item_CommissionValue + decimal.Parse(dss.Tables[0].Rows[x]["Item_CommissionValue"].ToString());
                //}
                //decimal Total_CommissionValue = Total_Item_CommissionValue + Commision;
                //mysql = " UPDATE Bills_List_Reverse SET  Edit_User_Id=" + Common.User_Id + ", EditOperationDate=" + AccountModelsLib.operations.Get_DateTime_NowToInsert() + ", Total_Item_CommissionValue=" + Total_Item_CommissionValue + ", Total_CommissionValue=    " + Total_CommissionValue + " WHERE (id = " + New_Bill_List_Id + ") ";
                //opp.insert_me(mysql);
                ///////////////////////////////////////////////////////////////////////////////



                InvoiceDataSetCLS ii = new InvoiceDataSetCLS();


                foreach (var item in invD_O.invDlst)
                {

                     

                    
                    


                    int Transaction_Type = 2;
                    int Invoice_Type_Id = Common.BilleReverse_Type_Id;
            


                      if (item.Item_DeletedFlage == 0)
                    {
                        //invD_O.Bills_List_Id = invL.Bills_List_Id;
                        item.Invoice_Id = invL_O.invL.Id;
                        item.Item_ProcessedFlage = 100;
                        item.Item_DeletedFlage = 100;

                        var serializedParent = JsonConvert.SerializeObject(item);
                        Bills_Details_Reverse bills_Details_Reverse = JsonConvert.DeserializeObject<Bills_Details_Reverse>(serializedParent);


                        invD_O.Insert_Bills_Details_Reverse(ref opp, ref Excp, bills_Details_Reverse);
                        item.Id = bills_Details_Reverse.Id;
                        if (Excp == 1)
                        {
                            opp.Rollback();
                            return;
                        }

                        if (Common.Project_Market == 1 & invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                        {

                            try
                            {
                                item.Store_Name = Common.GetStoreName(ref opp, item.Store_Id ?? 0);
                            }
                            catch { }
                            //ii.Set_ItemTransactionFromInvoices(ref opp, invd.Unit_Item_Table_Id, invd.Item_Net_Total_Price, invd.Larg_Qnty, invd.Small_Qnty, invd.Qnty_Another, 0, Transaction_Type, invL.Invoice_Date, invL.Bills_List_Id, invd.Bills_Details_Id, invL.Invoice_Type_Id, "„— Ã⁄ „‘ —Ì« ", invd.Store_Name, invL.Supplier_Name, invL.Supplier_Name, invL.Invoice_No);
                            ii.Set_ItemTransactionFromBill(ref opp, item, invL_O, 0, Transaction_Type, Common.BilleReverse_Type_Id, "„— Ã⁄ „‘ —Ì« ");

                        }
                    }
                    else if (item.Item_DeletedFlage ==-1)
                    {
                        DataSet dsN = opp.return_dataset("Select * from Bills_Details_Reverse Where Id=" + item.Id + "");
                        if (dsN.Tables[0].Rows.Count > 0)
                        {
                            OLD_Larg_Qnty = decimal.Parse(dsN.Tables[0].Rows[0]["Larg_Qnty"].ToString());
                            OLD_Small_Qnty = decimal.Parse(dsN.Tables[0].Rows[0]["Small_Qnty"].ToString());
                            OLD_Item_No = decimal.Parse(dsN.Tables[0].Rows[0]["Qnty_Another"].ToString());

                            // Item_No_Free, LargItemsQnty_Free, SmallItemsQnty_Free
                            decimal OLD_LargItemsQnty_Free = decimal.Parse(dsN.Tables[0].Rows[0]["LargItemsQnty_Free"].ToString());
                            decimal OLD_SmallItemsQnty_Free = decimal.Parse(dsN.Tables[0].Rows[0]["SmallItemsQnty_Free"].ToString());
                            decimal OLD_Item_No_Free = decimal.Parse(dsN.Tables[0].Rows[0]["Item_No_Free"].ToString());

                            OLD_Larg_Qnty = OLD_Larg_Qnty + OLD_LargItemsQnty_Free;
                            OLD_Small_Qnty = OLD_Small_Qnty + OLD_SmallItemsQnty_Free;
                            OLD_Item_No = OLD_Item_No + OLD_Item_No_Free;
                        }
                        item.Item_DeletedFlage =  100;
                        item.Item_ProcessedFlage= 100;
                        invD_O.Update_Bills_Details_Reverse(ref opp, ref Excp, (Bills_Details_Reverse)item);
                        if (Excp == 1)
                        {
                            opp.Rollback();
                            return;
                        }
                        if (Common.Project_Market == 1 & invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                        {
                            //InvoiceDataSetCLS ii = new InvoiceDataSetCLS();
                            try
                            {
                                item.Store_Name = Common.GetStoreName(ref opp, item.Store_Id ?? 0);
                            }
                            catch { }
                            //ii.Set_ItemTransactionFromInvoices(ref opp, invd.Unit_Item_Table_Id, invd.Item_Net_Total_Price, invd.Larg_Qnty, invd.Small_Qnty, invd.Qnty_Another, 0, Transaction_Type, invL.Invoice_Date, invL.Bills_List_Id, invd.Bills_Details_Id, invL.Invoice_Type_Id, "„— Ã⁄ „‘ —Ì« ", invd.Store_Name, invL.Supplier_Name, invL.Supplier_Name, invL.Invoice_No);
                            ii.Set_ItemTransactionFromBill(ref opp, item, invL_O, 0, Transaction_Type, Common.BilleReverse_Type_Id, "„— Ã⁄ „‘ —Ì« ");

                        }
                    }
                    #region Form Project_Market=1
                    if (Common.Project_Market == 1 & invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                    {

                        //if (item.Item_DeletedFlage == 1)
                        //{
                        //    //Õ–›
                        //    mysql = " UPDATE Unit_item SET Full_qua =Full_qua + " + item.Larg_Qnty + ", D_Qua =D_Qua + " + item.Small_Qnty + ", Item_No =Item_No + " + item.Qnty_Another + " WHERE (id = " + item.Unit_Item_Table_Id + ") ";
                        //    opp.insert_me(mysql);

                        //    //«·»Ê‰’
                        //    mysql = " UPDATE Unit_item SET Full_qua =Full_qua + " + item.LargItemsQnty_Free + ", D_Qua =D_Qua + " + item.SmallItemsQnty_Free + ", Item_No =Item_No + " + item.Item_No_Free + " WHERE (id = " + item.Unit_Item_Table_Id + ") ";
                        //    opp.insert_me(mysql);

                        //    // Common.SetBalanceWithExpireDate_Tash3ela(ref opp, 1, Item_Id, Unit_Item_Table_Id, Item_Expire_Date, Tash3ela, Larg_Qnty, Small_Qnty, Qnty_Another, LargItemsQnty_Free, SmallItemsQnty_Free, Item_No_Free);
                        //    CommonAccounts.SetBalanceWithExpireDate_Tash3ela_Bill(ref opp, 1, item, OLD_Larg_Qnty, OLD_Small_Qnty, OLD_Item_No, false);
                        //}
                          if (item.Item_DeletedFlage == 0)
                        {//«÷«›…
                            mysql = " UPDATE Unit_item SET Full_qua =Full_qua - " + item.Larg_Qnty + ", D_Qua =D_Qua - " + item.Small_Qnty + ", Item_No =Item_No - " + item.Qnty_Another + " WHERE (id = " + item.Unit_Item_Table_Id + ") ";
                            opp.insert_me(mysql);

                            //«·»Ê‰’
                            mysql = " UPDATE Unit_item SET Full_qua =Full_qua - " + item.LargItemsQnty_Free + ", D_Qua =D_Qua - " + item.SmallItemsQnty_Free + ", Item_No =Item_No - " + item.Item_No_Free + " WHERE (id = " + item.Unit_Item_Table_Id + ") ";
                            opp.insert_me(mysql);



                            //Common.SetBalanceWithExpireDate_Tash3ela(ref opp, 2, Item_Id, Unit_Item_Table_Id, Item_Expire_Date, Tash3ela, Larg_Qnty, Small_Qnty, Qnty_Another, LargItemsQnty_Free, SmallItemsQnty_Free, Item_No_Free);

                            CommonAccounts.SetBalanceWithExpireDate_Tash3ela_Bill(ref opp, 2, item, OLD_Larg_Qnty, OLD_Small_Qnty, OLD_Item_No, true);
                            // Common.SetBalanceWithExpireDate_Tash3ela_BillReverse(ref opp, 2, num46, num47, (long)num8, (long)num22, mydate, str12, qua, num10, num11, num31, num32, num33, num, num2, num3);
                            // SetBalanceWithExpireDate_Tash3ela_BillReverse(ref AccountModelsLib.operations opp, int Transaction, long Unit_Item_ExpireDate_Id, long Invoices_Details_Id, long Item_Id, long Unit_Item_Table_Id, DateTime Item_Expire_Date, string Tash3ela, decimal Larg_Balance, decimal Small_Balance, decimal Item_No, decimal LargItemsQnty_Free, decimal SmallItemsQnty_Free, decimal Item_No_Free, decimal OLD_Larg_Qnty, decimal OLD_Small_Qnty, decimal OLD_Item_NoQnty)

                        }
                        else if (item.Item_DeletedFlage == -1)
                        {


                            decimal Larg_Qnty = item.Larg_Qnty ?? 0 - OLD_Larg_Qnty;
                            decimal Small_Qnty = item.Small_Qnty ?? 0 - OLD_Small_Qnty;
                            decimal Qnty_Another = item.Qnty_Another ?? 0 - OLD_Item_No;

                            mysql = " UPDATE Unit_item SET Full_qua =Full_qua - " + Larg_Qnty + ", D_Qua =D_Qua - " + Small_Qnty + ", Item_No =Item_No - " + Qnty_Another + " WHERE (id = " + item.Unit_Item_Table_Id + ") ";
                            opp.insert_me(mysql);

                            // Common.SetBalanceWithExpireDate_Tash3ela(ref opp, 1, Item_Id, Unit_Item_Table_Id, Item_Expire_Date, Tash3ela, Larg_Qnty, Small_Qnty, Qnty_Another, LargItemsQnty_Free, SmallItemsQnty_Free, Item_No_Free);
                            CommonAccounts.SetBalanceWithExpireDate_Tash3ela_Bill(ref opp, 1, item, OLD_Larg_Qnty, OLD_Small_Qnty, OLD_Item_No, false);
                        }

                        //mysql = "  UPDATE Unit_item SET Full_qua = FLOOR((Small_unit * Full_qua + D_Qua) / Small_unit), ";
                        //mysql = mysql + " D_Qua =   (( Small_unit * Full_qua + D_Qua) / Small_unit - FLOOR(( Small_unit * Full_qua + D_Qua) /  Small_unit)) *  Small_unit  ";
                        //mysql = mysql + " WHERE (id = " + Unit_Item_Table_Id + ")  ";

                        mysql = "  UPDATE Unit_item SET Full_qua = FLOOR((Small_unit * Full_qua + D_Qua) / Small_unit), ";
                        mysql = mysql + " D_Qua =    (Small_unit * Full_qua + D_Qua) % Small_unit  ";
                        mysql = mysql + " WHERE (id = " + item.Unit_Item_Table_Id + ")  ";
                        opp.insert_me(mysql);
                    }

                    #endregion

                    #region order sarf automatic
                    if (Common.Make_Store_Order_Sarf_Estlam_Automatic_From_Invoice == 1 & invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                    {
                        if (item.Item_DeletedFlage == 1)
                        {
                            //Õ–›
                            Fill_Ds(ref opp, ref New_Items_ds_Minus, item);

                        }
                        else if (item.Item_DeletedFlage == 0)
                        {//«÷«›…

                            Fill_Ds(ref opp, ref New_Items_ds_Add, item);
                        }
                        else if (item.Item_DeletedFlage == -1)
                        {

                            decimal Larg_Qnty = item.Larg_Qnty ?? 0 - OLD_Larg_Qnty;
                            decimal Small_Qnty = item.Small_Qnty ?? 0 - OLD_Small_Qnty;
                            decimal Qnty_Another = item.Qnty_Another ?? 0 - OLD_Item_No;

                            decimal Larg_Qnty__ = decimal.Parse(Larg_Qnty.ToString());
                            decimal Small_Qnty__ = decimal.Parse(Small_Qnty.ToString());
                            decimal Small_Unit_Price__ = decimal.Parse(item.Small_Unit_Price.ToString());
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
                                Fill_Ds(ref opp, ref New_Items_ds_Add, item, Larg_Qnty__, Small_Qnty__);
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
                                Fill_Ds(ref opp, ref New_Items_ds_Minus, item, Larg_Qnty__, Small_Qnty__);
                            }

                        }
                    }


                    #endregion
                }


                foreach (var item in invD_O.invDlst_Deleted)
                {




                    int Transaction_Type = 2;
                    //int Invoice_Type_Id = Common.BilleReverse_Type_Id;
                    if (item.Item_DeletedFlage == 1)
                    {
                        mysql = "Delete From Bills_Details_Reverse  where id=" + item.Id;
                        opp.insert_me(mysql);

                        ii.Delete_ItemTransactionFromInvoices(ref opp, item.Unit_Item_Table_Id ?? 0, invL_O.invL.Id, invD_O.invD.Id, 0, Common.BilleReverse_Type_Id);

                    }


                    #region Form Project_Market=1
                    if (Common.Project_Market == 1 & invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                    {

                        if (item.Item_DeletedFlage == 1)
                        {
                            //Õ–›
                            mysql = " UPDATE Unit_item SET Full_qua =Full_qua + " + item.Larg_Qnty + ", D_Qua =D_Qua + " + item.Small_Qnty + ", Item_No =Item_No + " + item.Qnty_Another + " WHERE (id = " + item.Unit_Item_Table_Id + ") ";
                            opp.insert_me(mysql);

                            //«·»Ê‰’
                            mysql = " UPDATE Unit_item SET Full_qua =Full_qua + " + item.LargItemsQnty_Free + ", D_Qua =D_Qua + " + item.SmallItemsQnty_Free + ", Item_No =Item_No + " + item.Item_No_Free + " WHERE (id = " + item.Unit_Item_Table_Id + ") ";
                            opp.insert_me(mysql);

                            // Common.SetBalanceWithExpireDate_Tash3ela(ref opp, 1, Item_Id, Unit_Item_Table_Id, Item_Expire_Date, Tash3ela, Larg_Qnty, Small_Qnty, Qnty_Another, LargItemsQnty_Free, SmallItemsQnty_Free, Item_No_Free);
                            CommonAccounts.SetBalanceWithExpireDate_Tash3ela_Bill(ref opp, 1, item, OLD_Larg_Qnty, OLD_Small_Qnty, OLD_Item_No, false);
                        }
                       

                        //mysql = "  UPDATE Unit_item SET Full_qua = FLOOR((Small_unit * Full_qua + D_Qua) / Small_unit), ";
                        //mysql = mysql + " D_Qua =   (( Small_unit * Full_qua + D_Qua) / Small_unit - FLOOR(( Small_unit * Full_qua + D_Qua) /  Small_unit)) *  Small_unit  ";
                        //mysql = mysql + " WHERE (id = " + Unit_Item_Table_Id + ")  ";

                        mysql = "  UPDATE Unit_item SET Full_qua = FLOOR((Small_unit * Full_qua + D_Qua) / Small_unit), ";
                        mysql = mysql + " D_Qua =    (Small_unit * Full_qua + D_Qua) % Small_unit  ";
                        mysql = mysql + " WHERE (id = " + item.Unit_Item_Table_Id + ")  ";
                        opp.insert_me(mysql);
                    }

                    #endregion

                    #region order sarf automatic
                    if (Common.Make_Store_Order_Sarf_Estlam_Automatic_From_Invoice == 1 & invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                    {
                        if (item.Item_DeletedFlage == 1)
                        {
                            //Õ–›
                            Fill_Ds(ref opp, ref New_Items_ds_Minus, item);

                        }

                    }


                    #endregion
                }
                //DateTime Invoice_Date = DateTime.Parse(dss.Tables[0].Rows[0]["Invoice_Date"].ToString());
               
                ArrayList mysql_Arr = new ArrayList();
                if (Common.Make_Store_Order_Sarf_Estlam_Automatic_From_Invoice == 1 & invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                {

                    //for (int x = 0; x < mArrMake_Order.Count; x++)
                    //{
                    //    Invoice_ds.Tables[0].Rows[x]["Make_Order"] = mArrMake_Order[x];
                    //}

                    Order_Details_AutomaticCls oo = new Order_Details_AutomaticCls();
                    bool MakeOrderOk = false;
                    if (New_Items_ds_Add.Tables[0].Rows.Count > 0)
                    {
                        oo.Make_Order_From_Edit_Bill_Details_Reverse(ref Excp, ref opp, ref mysql_Arr, New_Items_ds_Add.Tables[0], invL_O.invL.Id, invL_O.invL.Supplier_Id ?? 0, invL_O.invL.Invoice_Date.Value, "›« Ê—… „— Ã⁄ „‘ —Ì« ");

                        if (Excp > 0)
                        {
                            opp.Rollback();
                            return;
                        }
                    }

                    if (MakeOrderOk)
                    {
                        Msg = Msg + "\n";
                        Msg = Msg + " „ ≈’œ«— ≈–‰ «·’—› ··„Ê—œ „‰ «·„Œ“‰";
                    }


                    //////////////////„— Ã⁄ ’—›
                    if (New_Items_ds_Minus.Tables[0].Rows.Count > 0)
                    {

                        oo.Make_Order_From_Edit_Bill_Details(ref Excp, ref mysql_Arr, ref opp, ref arr_Order_Purshing_Id_Automatic, ref MakeOrderOk, New_Items_ds_Minus.Tables[0], invL_O.invL.Id, invL_O.invL.Supplier_Id ?? 0, invL_O.invL.Invoice_Date.Value, "›« Ê—… „— Ã⁄ „‘ —Ì« ");

                        if (Excp > 0)
                        {
                            opp.Rollback();
                            return;
                        }
                    }

                }
                ///to delete  the invoice
                int CountToDelete = 0;
                //for (int x = 0; x < Invoice_ds.Tables[0].Rows.Count; x++)
                //{
                //    if (Invoice_ds.Tables[0].Rows[x]["Item_DeletedFlage"].ToString() == "1")
                //    {
                //        CountToDelete++;
                //    }
                //}


                bool IsDeleteBills_List = false;
                List<Bills_Details_Reverse> bills_DetailsLst = opp.db.Bills_Details_Reverse.AsNoTracking().Where(i => i.Invoice_Id == invL_O.invL.Id).ToList();

                if (bills_DetailsLst.Count == 0)
                {
                    IsDeleteBills_List = true;


                    mysql = "Delete From Bills_List_Reverse  where id=" + invL_O.invL.Id;
                    opp.insert_me(mysql);

                    Money_Transaction_BoxOrderReportCLS mo = new Money_Transaction_BoxOrderReportCLS();
                    long Entry_No = 0;
                    try
                    {
                        mo.Delete_Transaction_BanksBox_NewSarf_New(ref Excp, ref Msg, ref opp, invL_O.invL.Order_MoneyId ?? 0, ref Entry_No);
                        if(Excp>0)
                        {
                            Excp = 0;
                        }
                    }
                    catch (Exception eee)
                    {
                         
                    }


                }
                else
                {

                    invL_O.Update_Bill_List_Reverse(ref opp, ref Excp, (Bills_List_Reverse)invL_O.invL);

                }

                //**********************************

                //string mm = " SELECT ID, Supplier_Id, Import, Export, Balance, Balance_Actual, Transaction_Type, Transaction_Date, Reference_Id, Reference_Type,  Reference_Type_Table_Id, Payment_Type, Notes FROM Suppliers_Accounts WHERE     (Reference_Type_Table_Id = " + Invoice_List_Table_Id + ") AND (Reference_Type = 8)";
                //DataSet dsss = opp.return_dataset(mm);
                //long Supplier_Accounts_Id = 0;
                //Supplier_Accounts_Id = long.Parse(dsss.Tables[0].Rows[0]["Id"].ToString());

                //***********************************
                //Bills_List  Bills_Details

                //mysql = "UPDATE Bills_List_Reverse SET Invoice_Date = '" + AccountModelsLib.operations.dateFormat(Invoice_Date) + "', Notes=N'" + Notes + "',Load_Payment_OnTax=" + Load_Payment_OnTax + " ,Total =" + TotalSecond + ",  Net_Total=" + NetInvoiceTotal + " ,commission=" + Commision + ",ATS05_Value=" + ATS05_Value + " ,ATS05=" + ATS05 + ",TaxSales10_Value=" + TaxSales10_Value + " ,TaxSales10=" + TaxSales10 + " ,SupplierAccountAfter=" + SupplierAccountAfter + " WHERE (id = " + Invoice_List_Table_Id + ")";
                //opp.insert_me(mysql);

                if (Excp == 1)
                {
                    opp.Rollback();
                    return;
                }
                ///////// check customer balance

                //decimal Invoice_NetTotal_Diff = invL.Invoice_Net_Total_Old - invL.Net_Total_Price;
                if (Common.IsSetSuppliers_AccountsForBills == 1)
                {
                    invL_O.invL.Supplier_BalanceSet = 1;

                    Suppliers_AccountsCls ca = new Suppliers_AccountsCls();

                    ca.ProcessNo = invL_O.invL.ProcessNo ?? 0;
                    ca.Update_Supplier_BalanceWithEditBill_Reverse(ref Excp, ref opp, invL_O.invL.Invoice_Date.Value, invL_O.invL.Supplier_Id ?? 0, invL_O.invL.Id, invL_O.invL.Net_Total ?? 0, invL_O.invL.Notes);

                    //«‰‘«¡ «·ﬁÌœ

                    Account_Entry_Invoice_CLS aa = new Account_Entry_Invoice_CLS();
                    aa.IsNotDeletePrevEntry = false;
                    aa.Make_EntryFor_Bills_Reverse(ref Excp, ref Msg, ref opp, invL_O.invL.Id, invL_O.invL.Invoice_Date.Value, invL_O.invL.Invoice_Date.Value, -1);


                    if (invL_O.invL.Is_Invoice_Agile == 0 && IsDeleteBills_List == false)
                    {
                        if (Common.MakeOrderMoneyFromBills_Reverse_AutomaticWithoutForm == 1)
                        {

                            decimal PaidValueFromMarket = invL_O.invL.Net_Total ?? 0;
                            decimal RestValue = 0;
                            int Box_Id = 0;
                            Money_Transaction_BoxOrderReportCLS mo = new Money_Transaction_BoxOrderReportCLS();


                            //, long CarsWorkOrdersId, long Reference_Type_Table_Id, long DataDictionaryProcessesListsId

                            long CarsWorkOrdersId = 0;
                            long Reference_Type_Table_Id = invL_O.invL.Id;
                            long DataDictionaryProcessesListsId =Common.BilleReverse_Type_Id;

                            //   ,   CarsWorkOrdersId,   Reference_Type_Table_Id,   DataDictionaryProcessesListsId
                            long Entry_No = 0;
                            mo.Delete_Transaction_BanksBox_NewSarf_New(ref Excp, ref Msg, ref opp, invL_O.invL.Order_MoneyId ?? 0, ref Entry_No);
                            mo.MakeMoney_TransactionFromBill_Reverse(ref Excp, ref Msg, ref opp, invL_O.invL.Invoice_Date.Value, invL_O.invL.Invoice_Id ?? 0, invL_O.invL.Id, Box_Id, invL_O.invL.Currency_id ?? 0, invL_O.invL.Supplier_Id ?? 0, PaidValueFromMarket, RestValue, 1, Entry_No, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);
                        }
                    }
                }
                else
                {
                    invL_O.invL.Supplier_BalanceSet = 1;
                }
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

                if (mysql_Arr.Count > 0)
                {
                    AccountModelsLib.operations opp2 = new AccountModelsLib.operations(1);
                    try
                    {
                        mysql = "";
                        for (int y = 0; y < mysql_Arr.Count; y++)
                        {
                            mysql = mysql + " " + mysql_Arr[y].ToString();

                        }
                        opp2.insert_me(mysql);
                        opp2.Commit();
                    }
                    catch (Exception ee) { opp2.Rollback(); }
                }
                Excp = 0;
            }
            catch (Exception e)
            {
                opp.Rollback();
                Excp = 1;
                Msg = e.Message;
                //throw (e);
            }

        }
        void Fill_Ds(ref AccountModelsLib.operations opp, ref DataSet New_Items_ds_Add, IBills_Details invd)
        {
            DataRow dr;
            dr = New_Items_ds_Add.Tables[0].NewRow();
            ItemCLS itm = new ItemCLS(ref opp);
            Items item = itm.GetItemByIdNoTracking(invd.Item_Id ?? 0);

            try
            {

                dr["Make_Order"] = 0;
                dr["Item_Name"] = "";
                dr["ATS05"] = 0;
                dr["ATS05_Value"] = 0;
                dr["TaxSales10"] = 0;
                dr["TaxSales10_Value"] = 0;
                dr["Unit_price"] = item.unit.S_unit_Value ?? 0;
                dr["S_Unit"] = item.unit.S_unit;
                dr["B_Unit"] = item.unit.B_Unit;
                dr["Net_Total_Price"] = 0;
                dr["Larg_Qnty"] = invd.Larg_Qnty ?? 0;
                dr["Small_Qnty"] = invd.Small_Qnty ?? 0;
                dr["Qnty_Another"] = invd.Qnty_Another ?? 0;
                dr["Total_Price"] = 0;
                dr["PriceUnitIndex"] = invd.Unit_Used_for_Price ?? 0;
                //dr["Currency_id"] =
                dr["Item_id"] = invd.Item_Id ?? 0;
                dr["unit_id"] = invd.Unit_Id ?? 0;
                dr["Unit_Item_Table_Id"] = invd.Unit_Item_Table_Id ?? 0;
                dr["Code"] = invd.code;
                dr["Available_Larg_Balance"] = 0;
                dr["Available_Small_Balance"] = 0;
                dr["id"] = "0";

                dr["Specification"] = invd.Specification;
                dr["Store_Id"] = invd.Store_Id ?? 0;

                New_Items_ds_Add.Tables[0].Rows.Add(dr);

            }
            catch (Exception ee)
            { }
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
                dr["TaxSales10"] = 0;
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
        void Fill_Ds(ref AccountModelsLib.operations opp, ref DataSet New_Items_ds_Add, IBills_Details invd, decimal Will_Export_Qnty, decimal Will_Export_QntySmall)
        {
            DataRow dr;
            dr = New_Items_ds_Add.Tables[0].NewRow();
            ItemCLS itm = new ItemCLS(ref opp);
            Items item = itm.GetItemByIdNoTracking(invd.Item_Id ?? 0);
            try
            {

                dr["Make_Order"] = 0;
                dr["Item_Name"] = "";
                dr["ATS05"] = 0;
                dr["ATS05_Value"] = 0;
                dr["TaxSales10"] = 0;
                dr["TaxSales10_Value"] = 0;
                dr["Unit_price"] = item.unit.S_unit_Value ?? 0;
                dr["S_Unit"] = item.unit.S_unit;
                dr["B_Unit"] = item.unit.B_Unit;

                dr["Net_Total_Price"] = 0;
                dr["Larg_Qnty"] = Will_Export_Qnty;
                dr["Small_Qnty"] = Will_Export_QntySmall;
                dr["Qnty_Another"] = invd.Qnty_Another ?? 0;
                dr["Total_Price"] = 0;
                dr["PriceUnitIndex"] = invd.Unit_Used_for_Price ?? 0;
                //dr["Currency_id"] =
                dr["Item_id"] = invd.Item_Id ?? 0;
                dr["unit_id"] = invd.Unit_Id ?? 0;
                dr["Unit_Item_Table_Id"] = invd.Unit_Item_Table_Id ?? 0;
                dr["Code"] = invd.code;
                dr["Available_Larg_Balance"] = 0;
                dr["Available_Small_Balance"] = 0;
                dr["id"] = "0";

                dr["Specification"] = invd.Specification;
                dr["Store_Id"] = invd.Store_Id ?? 0;

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
                dr["TaxSales10"] = 0;
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

    }
    public struct Invoice_Items_Data_Struct2
    {
        public decimal gLarg_Balance;
        public decimal Small_Balance;
        public decimal Larg_Qnty;
        public decimal Small_Qnty;
        public int Unit_Item_Table_Id;
        public int Bills_Details_Id;
        public int Unit_id;
        public int Item_id;
    }
}
