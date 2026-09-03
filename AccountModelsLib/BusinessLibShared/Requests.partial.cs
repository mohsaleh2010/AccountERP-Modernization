using System;
using System.Collections.Generic;
using System.Text;
//using Connection4It;  using AccountModelsLib; using AccountModelsLib; using AccountModelsLib.Models;
using System.Data;
using System.Collections;
 
//using Store_LIB;
//using Account_Common_LIB;
using AccountModelsLib.Models;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
 
using System.Linq;
namespace AccountModelsLib
{
    public partial class Requests_ModelCls
    {
        public void Make_Invoice_Update_CustomerAccount(ref int Excp, ref string Msg, InvoicesOperationListCLS invL_O, InvoicesOperationCLS invD_O)
        {
            operations opp = new operations(1);
             
            try
            {
                /*
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
               */
                ////////////////////////////
                /////////////////// »—‰«„Ã «· ﬁ”Ìÿ
                //Make_Takseet mt = new Make_Takseet();
                //if (Common.Is_Takseet_Invoice == 1)
                //{
                //    /*
                //    mt.txtTotal_Money.Value = invL_O.invL.Net_Total??0;
                //    mt.dt = Invoice_ds.Tables[0];
                //    mt.ShowDialog();
                //    */
                //}
                ///



                ////***********************************************************************
                ////////// Make_New_Invoice_Id


                string mysql = "";
                try
                {
                    if (invL_O.invL.CustomerName.ToString().Trim() == "")
                    {
                        invL_O.invL.CustomerName = Common.GetCustomerName(ref opp, invL_O.invL.Customer_Id.Value);
                    }
                }
                catch { }
                //var serializedParent = JsonConvert.SerializeObject(invL_O.invL);
                //Invoices_List invoices_List = JsonConvert.DeserializeObject<Invoices_List>(serializedParent);

                //Invoices_List invoices_List_ = MappingHelper.MapScalarProperties<Invoices_List, IInvoices_List>(ind);
                Add_UpdateInvoice_List(ref Excp, ref Msg, ref opp, invL_O);
                //if (invL_O.invL.Id == 0)
                //{
                //    //New_Invoice_Id = invL_O.Insert_Invoices_List(ref opp, ref Excp, (Invoices_List)invL_O.invL);
                //    New_Invoice_Id = invL_O.Insert_Invoices_List(ref opp, ref Excp,  invL_O.invL);

                //}
                //else
                //{

                //    Invoices_List invlst = invL_O.Get_Invoices_List(ref opp, invL_O.invL.Id);
                //    if (invlst == null)
                //    {
                //        //New_Invoice_Id = invL_O.Insert_Invoices_List(ref opp, ref Excp, (Invoices_List)invL_O.invL);
                //        New_Invoice_Id = invL_O.Insert_Invoices_List(ref opp, ref Excp, invL_O.invL);
                //    }
                //    else
                //    {
                //        invL_O.Update_Invoices_List(ref   opp, ref   Excp, invL_O.invL);

                //        //opp.db.Entry((Invoices_List)invL_O.invL).State = EntityState.Modified;
                //        New_Invoice_Id = invL_O.invL.Id;
                //    }
                //}

                //New_Invoice_Id = 1000000;
                if (Excp == 1)
                {
                    opp.db.Entry((Invoices_List)invL_O.invL).State = EntityState.Detached;
                    New_Invoice_Id = 0;
                    invL_O.Invoices_List_Id = 0;
                    opp.Rollback();
                    return;
                }


                //ArrayList arr = new ArrayList();

                ////
                ///


                InvoiceDataSetCLS ii = new InvoiceDataSetCLS();
                //////////// add item in invoice details
                for (int x = 0; x < invD_O.invDlst.Count; x++)
                {
                    //InvoicesOperationCLS invd_O = new InvoicesOperationCLS();
                    invD_O.invDlst[x].Invoice_Id = New_Invoice_Id;

                    ////invd_O.Get_Item_Details(Invoice_ds, x);
                    //try
                    //{
                    //    invD_O.invDlst[x].Store_Name = Common.GetStoreName(ref opp, invD_O.invDlst[x].Store_Id ?? 0);
                    //}
                    //catch { }
                    invD_O.invDlst[x].Item_DeletedFlage = 100;
                    invD_O.invDlst[x].Item_ProcessedFlage = 100;

                    //try
                    //{
                    //var serializedParent = JsonConvert.SerializeObject(invD_O.invDlst[x]);
                    // Invoices_Details invoices_Details = JsonConvert.DeserializeObject<Invoices_Details>(serializedParent);

                    //Invoices_Details invoices_Details = (Invoices_Details)invD_O.invDlst[x];
                    Invoices_Details invoices_Details = invD_O.Insert_Invoices_Details(ref opp, ref Excp, invD_O.invDlst[x]);
                    invD_O.Invoices_Details_Id= invoices_Details.Id;
                    //}
                    //catch (Exception eeee)
                    //{ 
                    //}
                    if (Common.Project_Market == 1 & invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                    {

                        mysql = " UPDATE Unit_item SET Full_qua =Full_qua - " + invD_O.invDlst[x].Larg_Qnty + ", D_Qua =D_Qua - " + invD_O.invDlst[x].Small_Qnty + ", Item_No =Item_No - " + invD_O.invDlst[x].Qnty_Another + " WHERE (id = " + invD_O.invDlst[x].Unit_Item_Table_Id + ") ";
                        //opp.insert_me(mysql);
                        mysql = mysql + " UPDATE Bills_Details   SET Larg_Qnty_Balance = Larg_Qnty_Balance - " + invD_O.invDlst[x].Larg_Qnty + ", Small_Qnty_Balance =Small_Qnty_Balance - " + invD_O.invDlst[x].Small_Qnty + ", Qnty_Another_Balance =Qnty_Another_Balance - " + invD_O.invDlst[x].Qnty_Another + " WHERE (id = " + invD_O.invDlst[x].Bills_Details_Id + ") ";
                        //opp.insert_me(mysql);


                        //// «·»Ê‰’
                        mysql = mysql + " UPDATE Unit_item SET Full_qua =Full_qua - " + invD_O.invDlst[x].LargItemsQnty_Free + ", D_Qua =D_Qua - " + invD_O.invDlst[x].SmallItemsQnty_Free + ", Item_No =Item_No - " + invD_O.invDlst[x].Item_No_Free + " WHERE (id = " + invD_O.invDlst[x].Unit_Item_Table_Id + ") ";
                        //opp.insert_me(mysql);

                        //// «·»Ê‰’
                        mysql = mysql + " UPDATE Bills_Details   SET Larg_Qnty_Balance = Larg_Qnty_Balance - " + invD_O.invDlst[x].LargItemsQnty_Free + ", Small_Qnty_Balance =Small_Qnty_Balance - " + invD_O.invDlst[x].SmallItemsQnty_Free + ", Qnty_Another_Balance =Qnty_Another_Balance - " + invD_O.invDlst[x].Item_No_Free + " WHERE (id = " + invD_O.invDlst[x].Bills_Details_Id + ") ";
                        opp.insert_me(mysql);



                        //CommonAccounts.SetBalanceWithExpireDate_Tash3ela_Invoice(ref opp, 2, invd.Unit_Item_ExpireDate_Id, invd.Invoices_Details_Id, (long)invd.Item_Id, (long)invd.Unit_Item_Table_Id, invd.Item_Expire_Date, invd.Tash3ela, invd.Larg_Qnty, invd.Small_Qnty, invd.Qnty_Another, invd.LargItemsQnty_Free, invd.SmallItemsQnty_Free, invd.Item_No_Free, 0, 0, 0);
                        CommonAccounts.SetBalanceWithExpireDate_Tash3ela_Invoice(ref opp, 2, invD_O.invDlst[x], 0, 0, 0);

                        // Common.SetBalanceWithExpireDate_Tash3ela_Invoice(ref opp, 2, num51, num50, (long)num7, (long)num21, time, str13, num9, num10, decimal.Parse(str2), num34, num35, num36, 0M, 0M, 0M);
                        // SetBalanceWithExpireDate_Tash3ela_Invoice(ref operations opp, int Transaction, long Unit_Item_ExpireDate_Id, long Invoices_Details_Id, long Item_Id, long Unit_Item_Table_Id, DateTime Item_Expire_Date, string Tash3ela, decimal Larg_Balance, decimal Small_Balance, decimal Item_No, decimal LargItemsQnty_Free, decimal SmallItemsQnty_Free, decimal Item_No_Free, decimal OLD_Larg_Qnty, decimal OLD_Small_Qnty, decimal OLD_Item_NoQnty)


                        //mysql = "  UPDATE Unit_item SET Full_qua = FLOOR((Small_unit * Full_qua + D_Qua) / Small_unit), ";
                        //mysql = mysql + " D_Qua =   (( Small_unit * Full_qua + D_Qua) / Small_unit - FLOOR(( Small_unit * Full_qua + D_Qua) /  Small_unit)) *  Small_unit  ";
                        //mysql = mysql + " WHERE (id = " + Unit_Item_Table_Id + ")  ";


                        if (Common.Compute_ItemBalance_With_Fraction == 0)
                        {
                            mysql = "  UPDATE Unit_item SET Full_qua = FLOOR((Small_unit * Full_qua + D_Qua) / Small_unit), ";
                            mysql = mysql + " D_Qua =    (Small_unit * Full_qua + D_Qua) % Small_unit  ";
                            mysql = mysql + " WHERE (id = " + invD_O.invDlst[x].Unit_Item_Table_Id + ")  ";
                            opp.insert_me(mysql);
                        }




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
                            //invL_O.Make_Order = int.Parse(mArrMake_Order[x].ToString());
                        }
                    }

                    try
                    {
                        if (Common.Is_Update_Price_On_invoice == 1)
                        {
                            mysql = " UPDATE  Item SET S_price =  " + invD_O.invDlst[x].Larg_Unit_Price + " WHERE (id = " + invD_O.invDlst[x].Item_Id + ") ";
                            opp.insert_me(mysql);
                            mysql = " UPDATE  Unit_Item SET S_price =  " + invD_O.invDlst[x].Larg_Unit_Price + " WHERE (Item_Id = " + invD_O.invDlst[x].Item_Id + ") ";
                            opp.insert_me(mysql);

                            if (invD_O.invDlst[x].Unit_Used_for_Price == 1)
                            {
                                mysql = " UPDATE  Item SET S_price_LargeUnit =  " + invD_O.invDlst[x].Larg_Unit_Price + ",Unit_Used_For_S_price_Default=" + invD_O.invDlst[x].Unit_Used_for_Price + " WHERE (id = " + invD_O.invDlst[x].Item_Id + ") ";
                                opp.insert_me(mysql);
                                mysql = " UPDATE  Unit_Item SET S_price_LargeUnit =  " + invD_O.invDlst[x].Larg_Unit_Price + " ,Unit_Used_For_S_price_Default=" + invD_O.invDlst[x].Unit_Used_for_Price + "  WHERE (Item_Id = " + invD_O.invDlst[x].Item_Id + ") ";
                                opp.insert_me(mysql);
                            }
                            if (invD_O.invDlst[x].Unit_Used_for_Price == 2)
                            {

                                mysql = " UPDATE  Item SET S_price_SmallUnit =  " + invD_O.invDlst[x].Larg_Unit_Price + ",Unit_Used_For_S_price_Default=" + invD_O.invDlst[x].Unit_Used_for_Price + " WHERE (id = " + invD_O.invDlst[x].Item_Id + ") ";
                                opp.insert_me(mysql);
                                mysql = " UPDATE  Unit_Item SET S_price_SmallUnit =  " + invD_O.invDlst[x].Larg_Unit_Price + " ,Unit_Used_For_S_price_Default=" + invD_O.invDlst[x].Unit_Used_for_Price + "  WHERE (Item_Id = " + invD_O.invDlst[x].Item_Id + ") ";
                                opp.insert_me(mysql);
                            }
                            if (invD_O.invDlst[x].Unit_Used_for_Price == 3)
                            {
                                mysql = " UPDATE  Item SET S_price_Item_No =  " + invD_O.invDlst[x].Larg_Unit_Price + ",Unit_Used_For_S_price_Default=" + invD_O.invDlst[x].Unit_Used_for_Price + " WHERE (id = " + invD_O.invDlst[x].Item_Id + ") ";
                                opp.insert_me(mysql);
                                mysql = " UPDATE  Unit_Item SET S_price_Item_No =  " + invD_O.invDlst[x].Larg_Unit_Price + " ,Unit_Used_For_S_price_Default=" + invD_O.invDlst[x].Unit_Used_for_Price + "  WHERE (Item_Id = " + invD_O.invDlst[x].Item_Id + ") ";
                                opp.insert_me(mysql);
                            }
                        }
                    }
                    catch (Exception ee)
                    {

                    }
                    //invd_O.Invoices_Details_Id = invd_O.Insert_Invoices_Details(ref opp, ref Excp);
                    //Invoice_ds.Tables[0].Rows[x]["Item_ProcessedFlage"] = 100;
                    //Invoice_ds.Tables[0].Rows[x]["Customer_Id"] = invL_O.invL.Customer_Id;
                    //try
                    //{
                    //    Invoice_ds.Tables[0].Rows[x]["Invoices_Details_Id"] = invD_O.invDlst[x].Id.ToString();
                    //}
                    //catch (Exception ee)
                    //{ }
                    if (Excp == 1)
                    {
                        New_Invoice_Id = 0;
                        invL_O.Invoices_List_Id = 0;
                        opp.Rollback();
                        return;
                    }


                    int Transaction_Type = 2;
                    int Invoice_Type_Id = Common.Invoice_Type_Id;
                    if (Common.Project_Market == 1 & invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                    {


                        //ii.Set_ItemTransactionFromInvoices(ref opp, invd.Unit_Item_Table_Id, invd.Item_Net_Total_Price, invd.Larg_Qnty, invd.Small_Qnty, invd.Qnty_Another, 0, Transaction_Type, invL.Invoice_Date, invd.Invoices_List_Id, invd.Invoices_Details_Id, Invoice_Type_Id, "›« Ê—… „»Ì⁄« ", invL.Customer_Name, invd.Store_Name, invL.Customer_Name, invL.Invoice_No);

                        ii.Set_ItemTransactionFromInvoice(ref opp, invoices_Details, invL_O, 0, Transaction_Type, Common.Invoice_Type_Id, "›« Ê—… „»Ì⁄« ");

                    }


                }


                ////////// order sarf automatic
                if (Common.Make_Store_Order_Sarf_Estlam_Automatic_From_Invoice == 1 & invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                {
                    //for (int x = 0; x < mArrMake_Order.Count; x++)
                    //{
                    //    Invoice_ds.Tables[0].Rows[x]["Make_Order"] = mArrMake_Order[x];
                    //}
                    //Order_Details_AutomaticCls oo = new Order_Details_AutomaticCls();
                    //bool MakeOrderOk = false;
                    //oo.Make_Order_From_Invoive_Details(ref Excp, ref opp, ref arr_Order_Purshing_Id_Automatic, ref MakeOrderOk, Invoice_ds.Tables[0], New_Invoice_Id, (int)invL_O.invL_R.Customer_Id, invL_O.invL_R.Invoice_Date.Value, "›« Ê—… „»Ì⁄« ");
                    //if (MakeOrderOk)
                    //{
                    //    Msg = Msg + "\n";
                    //    Msg = Msg + " „ ≈’œ«— ≈–‰ «·’—› ··⁄„Ì· „‰ «·„Œ“‰";
                    //}

                    //Get_arr_Order_Purshing_Id_Automatic_From_Invoice(New_Invoice_Id, ref opp);
                    
                }
                #region Comment
                ////make order Money
                //////if (Make_Invoice_Agile == 1)
                //////{
                //////    Accounts.Money_Transaction.Cash_Money.Money_Transaction_BoxCLS mt = new Accounts.Money_Transaction.Cash_Money.Money_Transaction_BoxCLS();

                //////    Excp = 0;

                //////    int Transaction_Type = 1;
                //////    int Box_Id = mt.GetStaticBank_ID();// int.Parse(ot.banks_List1.cmbBankType.SelectedValue.ToString());

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
                //////    int Box_Id = int.Parse(ot.banks_List1.cmbBankType.SelectedValue.ToString());

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
                    ca.Set_Customer_Balance_Invoice(ref Excp, ref opp, (int)invL_O.invL.Customer_Id, invL_O.invL.Id, invL_O.invL.Net_Total ?? 0, invL_O.invL.Notes);

                    //«‰‘«¡ «·ﬁÌœ
                    Account_Entry_Invoice_CLS aa = new Account_Entry_Invoice_CLS();
                    aa.IsNotDeletePrevEntry = true;
                    aa.Make_EntryForInvoices(ref Excp, ref Msg, ref opp, invL_O.invL.Id, invL_O.invL.Invoice_Date.Value, invL_O.invL.Invoice_Date.Value, 0);
                }
                /////////////////// »—‰«„Ã «· ﬁ”Ìÿ

                //if (Common.Is_Takseet_Invoice == 1 & mt.Is_Press_Ok == true)
                //{
                //    mt.MakeTakseet(ref opp, invL_O.invL.Customer_Id ?? 0, New_Invoice_Id, 0);

                //}
                ///

                if (invL_O.invL.Is_Invoice_Agile == 0)
                {
                    if (Common.MakeOrderMoneyFromInvoiceAutomaticWithoutForm == 1)
                    {
                        int Box_Id = invL_O.invL.BanksId??0;
                        

                        //, long CarsWorkOrdersId, long Reference_Type_Table_Id, long DataDictionaryProcessesListsId

                        long CarsWorkOrdersId = 0;
                        long Reference_Type_Table_Id = invL_O.invL.Id;
                        long DataDictionaryProcessesListsId = Common.Invoice_Type_Id;
                        
                        PaidValueFromMarket = invL_O.invL.NetInvoiceTotal??0;
                        if(invL_O.invL.PaymentValueFor_Is_Invoice_Agile > PaidValueFromMarket)
                        {
                            PaidValueFromMarket = invL_O.invL.PaymentValueFor_Is_Invoice_Agile ?? 0;
                        }
                        RestValue = 0;
                        Money_Transaction_BoxOrderReportCLS mo = new Money_Transaction_BoxOrderReportCLS();
                        mo.Invoice_Type = Common.Invoice_Type_Id;
                        mo.MakeMoney_TransactionFromInvoice(ref Excp, ref Msg, ref opp, invL_O.invL.Invoice_Date.Value, invL_O.invL.Invoice_Id ?? 0, invL_O.invL.Id, Box_Id, invL_O.invL.Currency_id ?? 0, invL_O.invL.Customer_Id ?? 0, PaidValueFromMarket, RestValue, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);

                    }
                }
                else if (invL_O.invL.Is_Invoice_Agile ==1 && invL_O.invL.PaymentValueFor_Is_Invoice_Agile>0)
                {
                    if (Common.MakeOrderMoneyFromInvoiceAutomaticWithoutForm == 1)
                    {
                        int Box_Id = 0;
                        Money_Transaction_BoxOrderReportCLS mo = new Money_Transaction_BoxOrderReportCLS();

                        //, long CarsWorkOrdersId, long Reference_Type_Table_Id, long DataDictionaryProcessesListsId

                        long CarsWorkOrdersId = 0;
                        long Reference_Type_Table_Id = invL_O.invL.Id;
                        long DataDictionaryProcessesListsId = Common.Invoice_Type_Id;

                        PaidValueFromMarket = invL_O.invL.PaymentValueFor_Is_Invoice_Agile??0;
                        RestValue = 0;

                        mo.Invoice_Type = Common.Invoice_Type_Id;
                        mo.MakeMoney_TransactionFromInvoice(ref Excp, ref Msg, ref opp, invL_O.invL.Invoice_Date.Value, invL_O.invL.Invoice_Id ?? 0, invL_O.invL.Id, Box_Id, invL_O.invL.Currency_id ?? 0, invL_O.invL.Customer_Id ?? 0, PaidValueFromMarket, RestValue, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);

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
                                s.Invoices_ListId = invL_O.invL.Id;
                                s.IsProcessedAsInvoice = 1;
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


                    if (invL_O.salesList.Count > 0)
                    {
                        mysql = @" UPDATE Unit_Item_SalesDate SET         
                                   Invoices_DetailsId = Invoices_Details.Unit_Item_SalesDateId 
                                    FROM Unit_Item_SalesDate INNER JOIN Invoices_Details ON Unit_Item_SalesDate.Id = Invoices_Details.Unit_Item_SalesDateId 
                                    WHERE        (Invoices_Details.Invoice_Id = " + invL_O.invL.Id + ")";
                        opp.insert_me(mysql);
                    }
                }
                catch (Exception eee)
                {
                    Excp = 1;
                    Msg = eee.Message;
                }

                try
                {
                    foreach (var s in invL_O.appServiceLst)
                    {
                        try
                        {
                            var existingEntity = opp.db.AppointmentServices.Where(x => x.Id == s.Id).FirstOrDefault();

                            if (existingEntity != null)
                            {
                                s.Invoices_List_Id = invL_O.invL.Id;
                                s.IsProcessedAsInvoice = 1;
                                opp.db.Entry(existingEntity).CurrentValues.SetValues(s);
                                opp.db.SaveChanges(); 
                            }
                             
                        }
                        catch (Exception exx)
                        { 
                        }
                    }
                    if (invL_O.salesList.Count > 0)
                    {
                        mysql = @"UPDATE AppointmentServices
                                    SET Invoices_Details_Id = Invoices_Details.Id
                                    FROM AppointmentServices INNER JOIN
                                    Invoices_Details ON AppointmentServices.Id = Invoices_Details.AppointmentServicesId
                                    WHERE        (Invoices_Details.Invoice_Id = " + invL_O.invL.Id + ")";
                        opp.insert_me(mysql);
                    }
                }
                catch (Exception eee)
                {
                    Excp = 1;
                    Msg = eee.Message;
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
                Msg=ee.Message;
            }
        }
      public   long Add_UpdateInvoice_List_Reverse(ref int Excp, ref string Msg, ref operations opp, InvoicesOperationListCLS invL_O)
        {
            if (invL_O.invL.Id == 0)
            {

                New_Invoice_Id = invL_O.Insert_Invoices_List_Reverse(ref Excp, ref opp, invL_O.invL);

            }
            else
            {


                invL_O.Update_Invoices_List_Reverse(ref Excp, ref opp,  invL_O.invL);

                
                New_Invoice_Id = invL_O.invL.Id;

            }
            return New_Invoice_Id;

            //try
            //{
            //    invL_O.invL.Invoices_ListTempId = invL_O.invL.Invoices_ListTempId ?? 0;
            //    if (invL_O.invL.Invoices_ListTempId > 0)
            //    {
            //        RequestsInvoiceTemp requestsInvoiceTemp = new RequestsInvoiceTemp();
            //        requestsInvoiceTemp.Delete_InvoiceTem(ref Excp, ref Msg, ref opp, invL_O.invL.Invoices_ListTempId ?? 0);


            //    }
            //}
            //catch (Exception ee)
            //{

            //}

        }

        long Add_UpdateInvoice_List(ref int Excp, ref string  Msg, ref operations opp, InvoicesOperationListCLS invL_O )
        {
            if (invL_O.invL.Id == 0)
            {

                New_Invoice_Id = invL_O.Insert_Invoices_List(ref opp, ref Excp, invL_O.invL);

            }
            else
            {

                 
                    invL_O.Update_Invoices_List(ref opp, ref Excp, invL_O.invL);

                    //opp.db.Entry((Invoices_List)invL_O.invL).State = EntityState.Modified;
                    New_Invoice_Id = invL_O.invL.Id;
                 
            }
            //else
            //{

            //    Invoices_List invlst = invL_O.Get_Invoices_List(ref opp, invL_O.invL.Id);
            //    if (invlst == null)
            //    {
            //        //New_Invoice_Id = invL_O.Insert_Invoices_List(ref opp, ref Excp, (Invoices_List)invL_O.invL);
            //        New_Invoice_Id = invL_O.Insert_Invoices_List(ref opp, ref Excp, invL_O.invL);
            //    }
            //    else
            //    {
            //        invL_O.Update_Invoices_List(ref opp, ref Excp, invL_O.invL);

            //        //opp.db.Entry((Invoices_List)invL_O.invL).State = EntityState.Modified;
            //        New_Invoice_Id = invL_O.invL.Id;
            //    }
            //}

            try
            {
                invL_O.invL.Invoices_ListTempId = invL_O.invL.Invoices_ListTempId ?? 0;
                if (invL_O.invL.Invoices_ListTempId > 0)
                {
                    RequestsInvoiceTemp requestsInvoiceTemp = new RequestsInvoiceTemp();
                    requestsInvoiceTemp.Delete_InvoiceTem(ref Excp, ref Msg, ref opp, invL_O.invL.Invoices_ListTempId ?? 0);
                    //requestsInvoiceTemp.ChangeStatus_InvoiceTem(ref Excp, ref Msg, ref opp, invL_O.invL.Invoices_ListTempId ?? 0,(int)InvoiceStatusEnum.Closed);



                }
            }
            catch (Exception ee)
            {

            }

            return New_Invoice_Id;

        }
        public void Edit_Invoice_Update_CustomerAccount(ref int Excp, InvoicesOperationListCLS invL_O, InvoicesOperationCLS invD_O)
        {
            //PaidValueFromMarket, RestValue

            long Invoice_List_Table_Id = invL_O.invL.Id;
            // DateTime Invoice_Date, decimal InvoiveTotal, decimal NetInvoiceTotal, decimal Commision, decimal CommissionValue, string PolicaNo, string TroodContainerNo, int Make_Order, decimal Invoice_Net_Total_Old, decimal Invoice_Total_Old, decimal Invoice_Total_PriceWithoutTax_Old, decimal ATS05, decimal TaxSales10, decimal CustomerAccountAfter, int Currency_Id, string Notes, decimal AnotherPayment, long Salesman_Id, int IS_Make_Sarf_Item_BalanceFromInvoice, int Is_InvoiceKta3y

            InvoiceDataSetCLS ids = new InvoiceDataSetCLS();
            DataSet New_Items_ds_Add = new DataSet();
            DataSet New_Items_ds_Minus = new DataSet();
            New_Items_ds_Add = ids.MakeInvoiceDataSet();
            New_Items_ds_Minus = ids.MakeInvoiceDataSet();
            string Msg = "";
            ////////////////////////////////////////////////////
            invL_O.opp.BeginTransaction(2);
            operations opp = invL_O.opp;
            //operations opp = new operations(1);
            try
            {
                string mysql = "";

                string Customer_Name = "";

                decimal OLD_Larg_Qnty = 0;
                decimal OLD_Small_Qnty = 0;
                decimal OLD_Item_No = 0;
                string Store_Name = "";
                mysql = "";
                int Invoice_Type_Id = Common.Invoice_Type_Id;
                 Invoices_List invLOld = opp.db.Invoices_List.AsNoTracking().Where(i => i.Id == Invoice_List_Table_Id).FirstOrDefault();


                Add_UpdateInvoice_List(ref Excp, ref Msg, ref opp, invL_O);
                InvoiceDataSetCLS ii = new InvoiceDataSetCLS();
                /////// check the editableitem
                for (int x = 0; x < invD_O.invDlst.Count; x++)
                {
                    
                    int Transaction_Type = 2;

                    int Item_ProcessedFlage = 100;

                    try
                    {
                        Item_ProcessedFlage = int.Parse(invD_O.invDlst[x].Item_ProcessedFlage.ToString());
                        long IDD = long.Parse(invD_O.invDlst[x].Id.ToString());
                    }
                    catch (Exception e)
                    {

                    }

                    
                    if (invD_O.invDlst[x].Item_ProcessedFlage.ToString() == "0")
                    {
                        //invoices_Details = new Invoices_Details();
                        invD_O.invDlst[x].Invoice_Id = invL_O.invL.Id;
                        invD_O.invDlst[x].Item_ProcessedFlage = 100;
                        invD_O.invDlst[x].Item_DeletedFlage = 100;

                        //var serializedParent = JsonConvert.SerializeObject(invD_O.invDlst[x]);
                        //invoices_Details = JsonConvert.DeserializeObject<Invoices_Details>(serializedParent);

                        Invoices_Details invoices_Details = MappingHelper.MapScalarProperties<Invoices_Details, IInvoices_Details>(invD_O.invDlst[x]);

                        invD_O.invDlst[x].Id = invD_O.Insert_Invoices_Details(ref opp, ref Excp, invoices_Details);
                        //dss.Tables[0].Rows[x]["Invoices_Details_Id"] = invD_O.invDlst[x].Id.ToString();
                        if (Excp == 1)
                        {
                            opp.Rollback();
                            return;
                        }

                        if (Common.Project_Market == 1 & invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                        {
                            //int Transaction_Type = 2;
                            //int Invoice_Type_Id = 1;

                        }

                        #region Form Project_Market=1
                        if (Common.Project_Market == 1 & invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                        {
                            try
                            {
                                Store_Name = Common.GetStoreName(ref opp, invD_O.invDlst[x].Store_Id ?? 0);
                            }
                            catch { }
                            ii.Set_ItemTransactionFromInvoices(ref opp, invD_O.invDlst[x].Unit_Item_Table_Id ?? 0, invD_O.invDlst[x].Larg_Unit_Price ?? 0, invD_O.invDlst[x].Item_Net_Total_Price ?? 0, invD_O.invDlst[x].Larg_Qnty ?? 0, invD_O.invDlst[x].Small_Qnty ?? 0, invD_O.invDlst[x].Qnty_Another ?? 0, 0, Transaction_Type, invL_O.invL.Invoice_Date.Value, Invoice_List_Table_Id, invD_O.invDlst[x].Id, Invoice_Type_Id, "›« Ê—… „»Ì⁄« ", Store_Name, Customer_Name, Customer_Name, invL_O.invL.Invoice_Id ?? 0);
                            //InvoiceDataSetCLS ii = new InvoiceDataSetCLS();


                            decimal Larg_Qnty = invD_O.invDlst[x].Larg_Qnty ?? 0 - OLD_Larg_Qnty;
                            decimal Small_Qnty = invD_O.invDlst[x].Small_Qnty ?? 0 - OLD_Small_Qnty;
                            decimal Qnty_Another = invD_O.invDlst[x].Qnty_Another ?? 0 - OLD_Item_No;

                            mysql = " UPDATE Unit_item SET Full_qua =Full_qua - " + Larg_Qnty + ", D_Qua =D_Qua - " + Small_Qnty + ", Item_No =Item_No - " + Qnty_Another + " WHERE (id = " + invD_O.invDlst[x].Unit_Item_Table_Id + ") ";
                            opp.insert_me(mysql);
                            // Common.SetBalanceWithExpireDate_Tash3ela(ref opp, 2, Item_Id, Unit_Item_Table_Id, Item_Expire_Date, Tash3ela, Larg_Qnty, Small_Qnty, Qnty_Another, LargItemsQnty_Free, SmallItemsQnty_Free, Item_No_Free);
                            //CommonAccounts.SetBalanceWithExpireDate_Tash3ela_Invoice(ref opp, 2, invd.Unit_Item_ExpireDate_Id, invd.Invoices_Details_Id, (long)invd.Item_Id, (long)invd.Unit_Item_Table_Id, invd.Item_Expire_Date, invd.Tash3ela, Larg_Qnty, Small_Qnty, Qnty_Another, invd.LargItemsQnty_Free, invd.SmallItemsQnty_Free, invd.Item_No_Free, OLD_Larg_Qnty, OLD_Small_Qnty, OLD_Item_No);
                            Larg_Qnty = Larg_Qnty * -1;
                            Small_Qnty = Small_Qnty * -1;
                            Qnty_Another = Qnty_Another * -1;
                            CommonAccounts.SetBalanceWithExpireDate_Tash3ela_Invoice(ref opp, 0, invoices_Details, Larg_Qnty, Small_Qnty, Qnty_Another);




                            //mysql = "  UPDATE Unit_item SET Full_qua = FLOOR((Small_unit * Full_qua + D_Qua) / Small_unit), ";
                            //mysql = mysql + " D_Qua =   (( Small_unit * Full_qua + D_Qua) / Small_unit - FLOOR(( Small_unit * Full_qua + D_Qua) /  Small_unit)) *  Small_unit  ";
                            //mysql = mysql + " WHERE (id = " + Unit_Item_Table_Id + ")  ";
                            mysql = "  UPDATE Unit_item SET Full_qua = FLOOR((Small_unit * Full_qua + D_Qua) / Small_unit), ";
                            mysql = mysql + " D_Qua =    (Small_unit * Full_qua + D_Qua) % Small_unit  ";
                            mysql = mysql + " WHERE (id = " + invD_O.invDlst[x].Unit_Item_Table_Id + ")  ";
                            opp.insert_me(mysql);


                            //dss.Tables[0].Rows[x]["Item_ProcessedFlage"] = 100;
                        }

                        #endregion
                    }
                    else if (invD_O.invDlst[x].Item_ProcessedFlage.ToString() == "-1")
                    {
                        string mysql_Old = "Select * from Invoices_Details Where Id=" + invD_O.invDlst[x].Id + "";
                        DataSet dsN = opp.return_dataset(mysql_Old);
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


                        //var serializedParent = JsonConvert.SerializeObject(invoices_Details);
                        //Invoices_Details invoices_DetailsToUpdate = JsonConvert.DeserializeObject<Invoices_Details>(serializedParent);
                        invD_O.invDlst[x].Item_ProcessedFlage = 100;
                        invD_O.invDlst[x].Item_DeletedFlage = 100;
                        Invoices_Details invoices_Details = MappingHelper.MapScalarProperties<Invoices_Details, IInvoices_Details>(invD_O.invDlst[x],true);

                        //opp.db.Invoices_Details.Attach(invoices_DetailsToUpdate);
                        invD_O.Update_Invoices_Details(ref opp, ref Excp, invoices_Details);
                        if (Excp == 1)
                        {
                            opp.Rollback();
                            return;
                        }
                        if (Common.Project_Market == 1 & invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                        {
                            //int Transaction_Type = 2;
                            //int Invoice_Type_Id = 1;

                        }
                        #region Form Project_Market=1
                        if (Common.Project_Market == 1 & invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                        {
                            try
                            {
                                Store_Name = Common.GetStoreName(ref opp, invD_O.invDlst[x].Store_Id ?? 0);
                            }
                            catch { }
                            //ii.Set_ItemTransactionFromInvoices(ref opp, invd.Unit_Item_Table_Id, invd.Item_Net_Total_Price, invd.Larg_Qnty, invd.Small_Qnty, invd.Qnty_Another, 0, Transaction_Type, invL.Invoice_Date, Invoice_List_Table_Id, invd.Invoices_Details_Id, Invoice_Type_Id, "›« Ê—… „»Ì⁄« ", Store_Name, Customer_Name, Customer_Name, invL.Invoice_No);
                            ii.Set_ItemTransactionFromInvoice(ref opp, invoices_Details, invL_O, 0, Transaction_Type, Common.Invoice_Type_Id, "›« Ê—… „»Ì⁄« ");

                            //InvoiceDataSetCLS ii = new InvoiceDataSetCLS();


                            //«÷«›…
                            mysql = " UPDATE Unit_item SET Full_qua =Full_qua - " + invD_O.invDlst[x].Larg_Qnty + ", D_Qua =D_Qua - " + invD_O.invDlst[x].Small_Qnty + ", Item_No =Item_No - " + invD_O.invDlst[x].Qnty_Another + " WHERE (id = " + invD_O.invDlst[x].Unit_Item_Table_Id + ") ";
                            opp.insert_me(mysql);


                            //«·»Ê‰’
                            mysql = " UPDATE Unit_item SET Full_qua =Full_qua - " + invD_O.invDlst[x].LargItemsQnty_Free + ", D_Qua =D_Qua - " + invD_O.invDlst[x].SmallItemsQnty_Free + ", Item_No =Item_No - " + invD_O.invDlst[x].Item_No_Free + " WHERE (id = " + invD_O.invDlst[x].Unit_Item_Table_Id + ") ";
                            opp.insert_me(mysql);

                            // Common.SetBalanceWithExpireDate_Tash3ela(ref opp, 2, Item_Id, Unit_Item_Table_Id, Item_Expire_Date, Tash3ela, Larg_Qnty, Small_Qnty, Qnty_Another, LargItemsQnty_Free, SmallItemsQnty_Free, Item_No_Free);
                            //CommonAccounts.SetBalanceWithExpireDate_Tash3ela_Invoice(ref opp, 2, invd.Unit_Item_ExpireDate_Id, invd.Invoices_Details_Id, (long)invd.Item_Id, (long)invd.Unit_Item_Table_Id, invd.Item_Expire_Date, invd.Tash3ela, invd.Larg_Qnty, invd.Small_Qnty, invd.Qnty_Another, invd.LargItemsQnty_Free, invd.SmallItemsQnty_Free, invd.Item_No_Free, OLD_Larg_Qnty, OLD_Small_Qnty, OLD_Item_No);
                            CommonAccounts.SetBalanceWithExpireDate_Tash3ela_Invoice(ref opp, 2, invoices_Details, OLD_Larg_Qnty, OLD_Small_Qnty, OLD_Item_No);


                            //mysql = "  UPDATE Unit_item SET Full_qua = FLOOR((Small_unit * Full_qua + D_Qua) / Small_unit), ";
                            //mysql = mysql + " D_Qua =   (( Small_unit * Full_qua + D_Qua) / Small_unit - FLOOR(( Small_unit * Full_qua + D_Qua) /  Small_unit)) *  Small_unit  ";
                            //mysql = mysql + " WHERE (id = " + Unit_Item_Table_Id + ")  ";
                            mysql = "  UPDATE Unit_item SET Full_qua = FLOOR((Small_unit * Full_qua + D_Qua) / Small_unit), ";
                            mysql = mysql + " D_Qua =    (Small_unit * Full_qua + D_Qua) % Small_unit  ";
                            mysql = mysql + " WHERE (id = " + invD_O.invDlst[x].Unit_Item_Table_Id + ")  ";
                            opp.insert_me(mysql);


                            //dss.Tables[0].Rows[x]["Item_ProcessedFlage"] = 100;
                        }

                        #endregion
                    }
                     

                    #region order sarf automatic
                    if (Common.Make_Store_Order_Sarf_Estlam_Automatic_From_Invoice == 1 & invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                    {

                        //if (invD_O.invDlst[x].Item_ProcessedFlage.ToString() == "1")
                        //{
                        //    //Õ–›
                        //    Fill_Ds(ref New_Items_ds_Minus, dss, x);



                        //}
                        //else if (invD_O.invDlst[x].Item_ProcessedFlage.ToString() == "-1")
                        //{//«÷«›…

                        //    Fill_Ds(ref New_Items_ds_Add, dss, x);
                        //}
                        //else
                        //{

                        //    decimal Larg_Qnty = invD_O.invDlst[x].Larg_Qnty ?? 0 - OLD_Larg_Qnty;
                        //    decimal Small_Qnty = invD_O.invDlst[x].Small_Qnty ?? 0 - OLD_Small_Qnty;
                        //    decimal Qnty_Another = invD_O.invDlst[x].Qnty_Another ?? 0 - OLD_Item_No;

                        //    decimal Larg_Qnty__ = decimal.Parse(Larg_Qnty.ToString());
                        //    decimal Small_Qnty__ = decimal.Parse(Small_Qnty.ToString());
                        //    decimal Small_Unit_Price__ = decimal.Parse(invD_O.invDlst[x].Small_Unit_Price.ToString());
                        //    Cls_Conversion_Unit.ConVert_By_S_UnitValue(ref Larg_Qnty__, ref Small_Qnty__, Small_Unit_Price__);
                        //    int Add_ = 1;

                        //    if (Larg_Qnty__ < 0 | Small_Qnty__ < 0)
                        //    {
                        //        Add_ = 0;

                        //    }
                        //    else if (Larg_Qnty__ > 0 | Small_Qnty__ > 0)
                        //    {
                        //        Add_ = 1;

                        //    }
                        //    else
                        //    {
                        //        Add_ = -1;
                        //    }
                        //    if (Add_ == 1)
                        //    {
                        //        Fill_Ds(ref New_Items_ds_Add, dss, x, Larg_Qnty__, Small_Qnty__);
                        //    }
                        //    else if (Add_ == 0)
                        //    {
                        //        if (Larg_Qnty__ < 0)
                        //        {
                        //            Larg_Qnty__ = Larg_Qnty__ * -1;
                        //        }

                        //        if (Small_Qnty__ < 0)
                        //        {
                        //            Small_Qnty__ = Small_Qnty__ * -1;
                        //        }
                        //        Fill_Ds(ref New_Items_ds_Minus, dss, x, Larg_Qnty__, Small_Qnty__);
                        //    }

                        //}
                    }




                    #endregion
                    //invD_O.invDlst[x].Item_ProcessedFlage = 100;
                     
                }

                /////////«·Õ–›
                for (int x = 0; x < invD_O.invDlst_Deleted.Count; x++)
                {
                    Invoices_Details invoices_Details = MappingHelper.MapScalarProperties<Invoices_Details, IInvoices_Details>(invD_O.invDlst_Deleted[x], true);

 

                    if (invD_O.invDlst_Deleted[x].Item_ProcessedFlage  == 1)
                    {
                        mysql = "Delete From Invoices_Details  where id=" + invD_O.invDlst_Deleted[x].Id;
                        opp.insert_me(mysql);

                        //ii.Delete_ItemTransactionFromInvoices(ref opp, Unit_Item_Table_Id, Larg_Qnty, Small_Qnty, decimal.Parse(Qnty_Another.ToString()), 0, Transaction_Type, Invoice_Date, Invoice_List_Table_Id, Invoices_Details_Id, Invoice_Type_Id, "›« Ê—… „»Ì⁄« ", Store_Name, Customer_Name, Customer_Name);

                        ii.Delete_ItemTransactionFromInvoices(ref opp, invD_O.invDlst_Deleted[x].Unit_Item_Table_Id ?? 0, Invoice_List_Table_Id, invD_O.invDlst_Deleted[x].Id, 0, Invoice_Type_Id);
                    }
                    #region Form Project_Market=1
                    if (Common.Project_Market == 1 & invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                    {
                        if (invD_O.invDlst_Deleted[x].Item_ProcessedFlage == 1)
                        {
                            //Õ–›
                            mysql = " UPDATE Unit_item SET Full_qua =Full_qua + " + invD_O.invDlst_Deleted[x].Larg_Qnty + ", D_Qua =D_Qua + " + invD_O.invDlst_Deleted[x].Small_Qnty + ", Item_No =Item_No + " + invD_O.invDlst_Deleted[x].Qnty_Another + " WHERE (id = " + invD_O.invDlst_Deleted[x].Unit_Item_Table_Id + ") ";
                            opp.insert_me(mysql);

                            //«·»Ê‰’
                            mysql = " UPDATE Unit_item SET Full_qua =Full_qua + " + invD_O.invDlst_Deleted[x].LargItemsQnty_Free + ", D_Qua =D_Qua + " + invD_O.invDlst_Deleted[x].SmallItemsQnty_Free + ", Item_No =Item_No + " + invD_O.invDlst_Deleted[x].Item_No_Free + " WHERE (id = " + invD_O.invDlst_Deleted[x].Unit_Item_Table_Id + ") ";
                            opp.insert_me(mysql);


                            CommonAccounts.SetBalanceWithExpireDate_Tash3ela_Invoice(ref opp, 1, invoices_Details, invoices_Details.Larg_Qnty ?? 0, invoices_Details.Small_Qnty ?? 0, invoices_Details.Qnty_Another ?? 0);

                        }


                        mysql = "  UPDATE Unit_item SET Full_qua = FLOOR((Small_unit * Full_qua + D_Qua) / Small_unit), ";
                        mysql = mysql + " D_Qua =    (Small_unit * Full_qua + D_Qua) % Small_unit  ";
                        mysql = mysql + " WHERE (id = " + invD_O.invDlst_Deleted[x].Unit_Item_Table_Id + ")  ";
                        opp.insert_me(mysql);


                        //dss.Tables[0].Rows[x]["Item_ProcessedFlage"] = 100;
                    }

                    #endregion

                    #region order sarf automatic
                    if (Common.Make_Store_Order_Sarf_Estlam_Automatic_From_Invoice == 1 & invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                    {


                    }




                    #endregion
                    invD_O.invDlst_Deleted[x].Item_ProcessedFlage = 100;
                }

                //DateTime Invoice_Date = DateTime.Parse(dss.Tables[0].Rows[0]["Invoice_Date"].ToString());
                  Msg = "";
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
                        oo.Make_Order_From_EditInvoive_Details(ref Excp, ref mysql_Arr, ref opp, ref arr_Order_Purshing_Id_Automatic, ref MakeOrderOk, New_Items_ds_Add.Tables[0], Invoice_List_Table_Id, (int)invL_O.invL.Customer_Id, invL_O.invL.Invoice_Date.Value, "›« Ê—… „»Ì⁄« ");
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
                        oo.Make_Order_From_Edit_Invoices_Details_Reverse(ref Excp, ref opp, ref mysql_Arr, New_Items_ds_Minus.Tables[0], Invoice_List_Table_Id, (int)invL_O.invL.Customer_Id, invL_O.invL.Invoice_Date.Value, "›« Ê—… „»Ì⁄« ");
                        if (Excp > 0)
                        {
                            opp.Rollback();
                            return;
                        }
                    }

                }

                ///to delete  the invoice
                int CountToDelete = 0;

                bool IsDeleteInvoices_List = false;
                List<Invoices_Details> invoices_DetailsLst = opp.db.Invoices_Details.AsNoTracking().Where(i => i.Invoice_Id == invL_O.invL.Id).ToList();

                if (invoices_DetailsLst.Count == 0)
                {
                    IsDeleteInvoices_List = true;
                    mysql = "Delete From Invoices_List  where id=" + Invoice_List_Table_Id;
                    opp.insert_me(mysql);

                    //Invoice_List_Table_Id = 0;
                    //invL.Invoices_List_Id = 0;
                }
                else
                {
                    //invL_O.Update_Invoices_List(ref opp, ref Excp, (Invoices_List)invL_O.invL);
                }
                //for (int x = 0; x < invD_O.invDlst.Count; x++)
                //{
                //    if (invD_O.invDlst[x].Item_ProcessedFlage.ToString() == "1")
                //    {
                //        CountToDelete++;
                //    }
                //}
                //for (int x = 0; x < invD_O.invDlst_Deleted.Count; x++)
                //{
                //    if (invD_O.invDlst_Deleted[x].Item_ProcessedFlage.ToString() == "1")
                //    {
                //        CountToDelete++;
                //    }
                //}
                //if (CountToDelete == invD_O.invDlst.Count)
                //{
                //    mysql = "Delete From Invoices_List  where id=" + Invoice_List_Table_Id;
                //    opp.insert_me(mysql);

                //    //Invoice_List_Table_Id = 0;
                //    //invL.Invoices_List_Id = 0;
                //}
                //else
                //{
                //    invL_O.Update_Invoices_List(ref opp, ref Excp, (Invoices_List)invL_O.invL);
                //}
                if (Excp == 1)
                {
                    opp.Rollback();
                    return;
                }
                ///////// check customer balance
                decimal Invoice_NetTotal_Diff = invLOld.Net_Total - invL_O.invL.Net_Total ?? 0;
                if (Common.IsSetCustomers_AccountsForInvoice == 1)
                {

                    CustomersAccountCLS ca = new CustomersAccountCLS();
                    ca.ProcessNo = invL_O.invL.ProcessNo ?? 0;
                    ca.Updatet_Customer_BalanceWithEditInvoice(ref Excp, ref opp, invL_O.invL.Invoice_Date.Value, (int)invL_O.invL.Customer_Id, Invoice_List_Table_Id, invL_O.invL.Net_Total ?? 0, Invoice_NetTotal_Diff, invL_O.invL.Notes);


                    //«‰‘«¡ «·ﬁÌœ
                    Account_Entry_Invoice_CLS aa = new Account_Entry_Invoice_CLS();
                    aa.IsNotDeletePrevEntry = false;
                    aa.Account_Entry_List_ID = invL_O.invL.Account_Entry_List_ID ?? 0;
                    aa.Make_EntryForInvoices(ref Excp, ref Msg, ref opp, invL_O.Invoices_List_Id, invL_O.invL.Invoice_Date.Value, invL_O.invL.Invoice_Date.Value, 1);

                    if (invL_O.invL.Is_Invoice_Agile == 0 && IsDeleteInvoices_List == false)
                    {

                        if (Common.MakeOrderMoneyFromInvoiceAutomaticWithoutForm == 1)
                        {

                            decimal PaidValueFromMarket = invL_O.invL.Net_Total ?? 0;
                            decimal RestValue = 0;
                            int Box_Id = 0;
                            Money_Transaction_BoxOrderReportCLS mo = new Money_Transaction_BoxOrderReportCLS();
                            //, long CarsWorkOrdersId, long Reference_Type_Table_Id, long DataDictionaryProcessesListsId

                            long CarsWorkOrdersId = 0;
                            long Reference_Type_Table_Id = invL_O.invL.Id;
                            long DataDictionaryProcessesListsId = Common.Invoice_Type_Id;

                            //   ,   CarsWorkOrdersId,   Reference_Type_Table_Id,   DataDictionaryProcessesListsId

                            long Entry_No = 0;
                            mo.Delete_Transaction_BanksBox_NewSarf_New(ref Excp, ref Msg, ref opp, invL_O.invL.Order_MoneyId ?? 0, ref Entry_No);
                            //mo.MakeMoney_TransactionFromInvoice(ref Excp, ref Msg, ref opp, invL_O.invL.Invoice_Date.Value, invL_O.invL.Id, invL_O.invL.Id, Box_Id, invL_O.invL.Currency_id ?? 0, invL_O.invL.Customer_Id ?? 0, PaidValueFromMarket, RestValue, 0, Entry_No, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);
                            mo.Invoice_Type = Common.Invoice_Type_Id;
                            mo.MakeMoney_TransactionFromInvoice(ref Excp, ref Msg, ref opp, invL_O.invL.Invoice_Date.Value, invL_O.invL.Invoice_Id ?? 0, invL_O.invL.Id, Box_Id, invL_O.invL.Currency_id ?? 0, invL_O.invL.Customer_Id ?? 0, PaidValueFromMarket, RestValue, CarsWorkOrdersId, Reference_Type_Table_Id, DataDictionaryProcessesListsId);
                        }
                    }

                }
                Excp = 0;

                opp.Commit();



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
            catch (Exception e)
            {
                opp.Rollback();
                Excp = 1;
                Msg = e.Message;
                //throw (e);
            }

        }

        public void Delete_Finaly_Invoice_CustomerAccount(ref int Excp,ref string Msg, InvoicesOperationListCLS invL_O, InvoicesOperationCLS invD_O)
        {
            invL_O.opp.BeginTransaction(2);

            
           

            Delete_Invoices_Details_CustomerAccount(ref Excp, ref Msg, ref invL_O.opp, invL_O, invD_O);

            if(Excp == 1)
            {
                invL_O.opp.Rollback();
                return;
            }

            if (invL_O.invL.Id > 0)
            {
                string mysql = "Delete From Invoices_List  where id=" + invL_O.invL.Id;
                invL_O.opp.insert_me(mysql);
            }
            else
            {
                Excp = 1;
                Msg = "·« Ì„ﬂ‰ Õ–› «·›« Ê—…";
                return;
            }

        }
       public void Delete_Invoices_Details_CustomerAccount(ref int Excp,ref string Msg, ref operations opp , InvoicesOperationListCLS invL_O, InvoicesOperationCLS invD_O)
        {
            try
            {
                long Invoice_List_Table_Id = invL_O.invL.Id;
            // DateTime Invoice_Date, decimal InvoiveTotal, decimal NetInvoiceTotal, decimal Commision, decimal CommissionValue, string PolicaNo, string TroodContainerNo, int Make_Order, decimal Invoice_Net_Total_Old, decimal Invoice_Total_Old, decimal Invoice_Total_PriceWithoutTax_Old, decimal ATS05, decimal TaxSales10, decimal CustomerAccountAfter, int Currency_Id, string Notes, decimal AnotherPayment, long Salesman_Id, int IS_Make_Sarf_Item_BalanceFromInvoice, int Is_InvoiceKta3y

            InvoiceDataSetCLS ids = new InvoiceDataSetCLS();
            DataSet New_Items_ds_Add = new DataSet();
            DataSet New_Items_ds_Minus = new DataSet();
            New_Items_ds_Add = ids.MakeInvoiceDataSet();
            New_Items_ds_Minus = ids.MakeInvoiceDataSet();

            ////////////////////////////////////////////////////
           
           
            //operations opp = new operations(1);
           
                string mysql = "";

                
                mysql = "";
                int Invoice_Type_Id = Common.Invoice_Type_Id;
                Invoices_List invLOld = opp.db.Invoices_List.AsNoTracking().Where(i => i.Id == Invoice_List_Table_Id).FirstOrDefault();

                InvoiceDataSetCLS ii = new InvoiceDataSetCLS();
                /////// check the editableitem
                

                /////////«·Õ–›
                for (int x = 0; x < invD_O.invDlst.Count; x++)
                {
                    Invoices_Details invoices_Details = (Invoices_Details)invD_O.invDlst[x];

                    
                        mysql = "Delete From Invoices_Details  where id=" + invD_O.invDlst[x].Id;
                        opp.insert_me(mysql);

                       
                        ii.Delete_ItemTransactionFromInvoices(ref opp, invD_O.invDlst[x].Unit_Item_Table_Id ?? 0, Invoice_List_Table_Id, invD_O.invDlst[x].Id, 0, Invoice_Type_Id);
                    
                    #region Form Project_Market=1
                    if (Common.Project_Market == 1 & invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                    {
                        
                            //Õ–›
                            mysql = " UPDATE Unit_item SET Full_qua =Full_qua + " + invD_O.invDlst[x].Larg_Qnty + ", D_Qua =D_Qua + " + invD_O.invDlst[x].Small_Qnty + ", Item_No =Item_No + " + invD_O.invDlst[x].Qnty_Another + " WHERE (id = " + invD_O.invDlst[x].Unit_Item_Table_Id + ") ";
                            opp.insert_me(mysql);

                            //«·»Ê‰’
                            mysql = " UPDATE Unit_item SET Full_qua =Full_qua + " + invD_O.invDlst[x].LargItemsQnty_Free + ", D_Qua =D_Qua + " + invD_O.invDlst[x].SmallItemsQnty_Free + ", Item_No =Item_No + " + invD_O.invDlst[x].Item_No_Free + " WHERE (id = " + invD_O.invDlst[x].Unit_Item_Table_Id + ") ";
                            opp.insert_me(mysql);


                            CommonAccounts.SetBalanceWithExpireDate_Tash3ela_Invoice(ref opp, 1, invoices_Details, invoices_Details.Larg_Qnty ?? 0, invoices_Details.Small_Qnty ?? 0, invoices_Details.Qnty_Another ?? 0);

                        


                        mysql = "  UPDATE Unit_item SET Full_qua = FLOOR((Small_unit * Full_qua + D_Qua) / Small_unit), ";
                        mysql = mysql + " D_Qua =    (Small_unit * Full_qua + D_Qua) % Small_unit  ";
                        mysql = mysql + " WHERE (id = " + invD_O.invDlst[x].Unit_Item_Table_Id + ")  ";
                        opp.insert_me(mysql);
                         
                    }

                    #endregion

                    #region order sarf automatic
                    if (Common.Make_Store_Order_Sarf_Estlam_Automatic_From_Invoice == 1 & invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1)
                    {


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
                        oo.Make_Order_From_EditInvoive_Details(ref Excp, ref mysql_Arr, ref opp, ref arr_Order_Purshing_Id_Automatic, ref MakeOrderOk, New_Items_ds_Add.Tables[0], Invoice_List_Table_Id, (int)invL_O.invL.Customer_Id, invL_O.invL.Invoice_Date.Value, "›« Ê—… „»Ì⁄« ");
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
                        oo.Make_Order_From_Edit_Invoices_Details_Reverse(ref Excp, ref opp, ref mysql_Arr, New_Items_ds_Minus.Tables[0], Invoice_List_Table_Id, (int)invL_O.invL.Customer_Id, invL_O.invL.Invoice_Date.Value, "›« Ê—… „»Ì⁄« ");
                        if (Excp > 0)
                        {
                            opp.Rollback();
                            return;
                        }
                    }

                }

                ///to delete  the invoice
                 

                
                if (Excp == 1)
                {
                     
                    return;
                }
                ///////// check customer balance
                 decimal Invoice_NetTotal_Diff = invLOld.Net_Total - invL_O.invL.Net_Total ?? 0;
                if (Common.IsSetCustomers_AccountsForInvoice == 1)
                {

                    CustomersAccountCLS ca = new CustomersAccountCLS();
                    ca.ProcessNo = invL_O.invL.ProcessNo ?? 0;
                    ca.Updatet_Customer_BalanceWithEditInvoice(ref Excp, ref opp, invL_O.invL.Invoice_Date.Value, (int)invL_O.invL.Customer_Id, Invoice_List_Table_Id, invL_O.invL.Net_Total ?? 0, Invoice_NetTotal_Diff, invL_O.invL.Notes);


                    //«‰‘«¡ «·ﬁÌœ
                    Account_Entry_Invoice_CLS aa = new Account_Entry_Invoice_CLS();
                    aa.IsNotDeletePrevEntry = false;
                    aa.Account_Entry_List_ID = invL_O.invL.Account_Entry_List_ID ?? 0;
                    aa.Make_EntryForInvoices(ref Excp, ref Msg, ref opp, invL_O.Invoices_List_Id, invL_O.invL.Invoice_Date.Value, invL_O.invL.Invoice_Date.Value, 1);

                    if (invL_O.invL.Is_Invoice_Agile == 0  )
                    {

                        if (Common.MakeOrderMoneyFromInvoiceAutomaticWithoutForm == 1)
                        {

                            decimal PaidValueFromMarket = invL_O.invL.Net_Total ?? 0;
                            decimal RestValue = 0;
                            int Box_Id = 0;
                            Money_Transaction_BoxOrderReportCLS mo = new Money_Transaction_BoxOrderReportCLS();
                            //, long CarsWorkOrdersId, long Reference_Type_Table_Id, long DataDictionaryProcessesListsId

                            long CarsWorkOrdersId = 0;
                            long Reference_Type_Table_Id = invL_O.invL.Id;
                            long DataDictionaryProcessesListsId = Common.Invoice_Type_Id;

                            //   ,   CarsWorkOrdersId,   Reference_Type_Table_Id,   DataDictionaryProcessesListsId

                            long Entry_No = 0;
                            invL_O.invL.Order_MoneyId = invL_O.invL.Order_MoneyId ?? 0;
                            if(invL_O.invL.Order_MoneyId > 0)
                            {
                                mo.Delete_Transaction_BanksBox_NewSarf_New(ref Excp, ref Msg, ref opp, invL_O.invL.Order_MoneyId ?? 0, ref Entry_No);
                            }
                            
                        }
                    }

                }
                Excp = 0;

                try
                {

                    //////////Unit_Item_SalesDate
                    mysql = @" UPDATE Unit_Item_SalesDate SET         
                                   Invoices_DetailsId = 0,IsProcessedAsInvoice = 0,Invoices_ListId=0
                                    FROM Unit_Item_SalesDate INNER JOIN Invoices_Details ON Unit_Item_SalesDate.Id = Invoices_Details.Unit_Item_SalesDateId 
                                    WHERE        (Invoices_Details.Invoice_Id = " + invL_O.invL.Id + ")";
                        opp.insert_me(mysql);
                    
                }
                catch (Exception eee)
                {
                    Excp = 1;
                    Msg = eee.Message;
                }

                try
                {
                    ////////////AppointmentServices
                    ///
                    mysql = @"UPDATE AppointmentServices
                                    SET Invoices_Details_Id = 0,Invoices_List_Id=0, IsProcessedAsInvoice = 0
                                    FROM AppointmentServices INNER JOIN
                                    Invoices_Details ON AppointmentServices.Id = Invoices_Details.AppointmentServicesId
                                    WHERE        (Invoices_Details.Invoice_Id = " + invL_O.invL.Id + ")";
                        opp.insert_me(mysql);
                     
                }
                catch (Exception eee)
                {
                    Excp = 1;
                    Msg = eee.Message;
                }

            }
            catch (Exception e)
            {
                 
                Excp = 1;
               Msg= e.Message;
                //throw (e);
            }

        }

        public void Make_InvoiceReverse_ForCars(ref int Excp, ref string Msg, ref operations opp, CarsWorkOrdersCLS carsWorkOrderCls, Invoices_List_Reverse invL)
        {
            List<Invoices_Details_Reverse> invdList = carsWorkOrderCls.GetInvoices_Details_ReverseList(carsWorkOrderCls.ItemReversedProcessedList);
            opp.BeginTransaction(2);


            TaxReverseInvoices_ListCLS ts = new  TaxReverseInvoices_ListCLS (ref opp);
            long TaxInvoiceNo = 0;
            string internalID = "";
            if (invL.IsInvoiceTaxInfo == 1)
            {
                ////Get Portal InternalId

                internalID = ts.GetPortalInternalId(ref opp, ref TaxInvoiceNo, invL);
            }

            try
            {

                invL.TaxInvoiceNo = TaxInvoiceNo;
                invL.TaxInternalId = internalID;
                Invoices_List_ReverseCls invCls = new  Invoices_List_ReverseCls ();
                invCls.AddInvoices_List_Reverse(ref Excp, ref Msg, ref opp, invL);
                New_Invoice_Id = invL.Id;
                if (Excp == 1)
                {
                    New_Invoice_Id = 0;
                    invL.Id = 0;
                    opp.Rollback();
                    return;
                }

                Make_Invoice_DetailsReverse(ref Excp, ref Msg, ref opp, New_Invoice_Id, invdList, invCls, carsWorkOrderCls);
                if (Excp == 1)
                {

                    New_Invoice_Id = 0;
                    invL.Id = 0;
                    opp.Rollback();
                    return;
                }
                 
                //CarsWorkOrdersCLS cc = new CarsWorkOrdersCLS(ref opp);
                carsWorkOrderCls.carWorkOrder.Last_Invoices_List_ReverseId = invL.Id;
                carsWorkOrderCls.carWorkOrder.TaxReverseInternalId = invL.TaxInternalId;
                carsWorkOrderCls.carWorkOrder.TaxReverseInvoiceNo = invL.TaxInvoiceNo;
                //cc.carWorkOrder = carsWorkOrder;
                carsWorkOrderCls.EditWorkOrder(ref Excp, ref Msg);
                if (Excp == 1)
                {

                    New_Invoice_Id = 0;
                    invL.Id = 0;
                    opp.Rollback();
                    return;
                }

                //////////// Save Invoice in  TaxInvoices_List
                // SaveInvoiceInTaxInvoices_List(ref Excp, ref Msg, ref opp, invL.Id);
                ts.ReSetTaxReverseInvoices_List_From_ERPWithInvoice(ref Excp, ref Msg, invL.Id);

                if (Excp == 1)
                {
                    Msg = "Œÿ√ ›Ï  «⁄œ«œ «·›« Ê—…";
                    New_Invoice_Id = 0;
                    invL.Id = 0;
                    opp.Rollback();
                    return;
                }

                if (Excp == 0)
                {
                    SetCustomerAccountsForInvoiceReverse(ref Excp, ref Msg, ref opp, invL, carsWorkOrderCls.carWorkOrder);
                }


            }
            catch (Exception ee)
            {
                opp.Rollback();
                Excp = 1;
            }
        }
        private void SetCustomerAccountsForInvoiceReverse(ref int Excp, ref string Msg, ref operations opp, Invoices_List_Reverse invL, CarsWorkOrder cworkOrder)
        {
            try
            {
                long WorkOrderNumber = cworkOrder.WorkOrderNumber ?? 0;

                CustomersAccountCLS ca = new CustomersAccountCLS();
                //if (invL.Notes.Trim() == "")
                //{
                //string Notes= " ›« Ê—… „»Ì⁄«   —ﬁ„    " + invL.Invoice_Id;

                string Notes = " √„— «·‘€· —ﬁ„ " + WorkOrderNumber.ToString();
                if (invL.TaxInternalId.Trim() != "")
                {
                    Notes = Notes + " Ê „ —›⁄Â« «·ﬂ —Ê‰Ì« »—ﬁ„ " + invL.TaxInternalId.ToString();

                }
                invL.Notes = invL.Notes + " " + Notes;
                //}
                //ca.Set_Customer_Balance_Invoice(ref Excp, ref opp, (int)invL.Customer_Id, invL.Id, invL.Net_Total ?? 0, invL.Notes);
                ca.Set_Customer_BalanceInvoiceReverse(ref Excp, ref opp, (int)invL.Customer_Id, invL.Id, invL.Net_Total ?? 0, invL.Notes);
                
                if (Excp == 1)
                {

                    opp.Rollback();
                    return;
                }
            }
            catch (Exception ee)
            {

                Excp = 1;
                opp.Rollback();
                return;
            }
            //«‰‘«¡ «·ﬁÌœ
            try
            {
                Account_Entry_Invoice_CLS aa = new Account_Entry_Invoice_CLS();
                aa.IsNotDeletePrevEntry = false;
                aa.Make_EntryForInvoices_Reverse(ref Excp, ref Msg, ref opp, invL.Id, invL.Invoice_Date.Value, invL.Invoice_Date.Value, 0);

                if (Excp == 1)
                {

                    opp.Rollback();
                    return;
                }

            }
            catch (Exception ee)
            {

                Excp = 1;
                opp.Rollback();
                return;
            }
        }
        private void Make_Invoice_DetailsReverse(ref int excp, ref string msg, ref operations opp, long New_Invoice_Id, List<Invoices_Details_Reverse> invdList, Invoices_List_ReverseCls invCls, CarsWorkOrdersCLS carsWorkOrderCls)
        {
            CarsWorkOrdersItemReversedProcessedDetailCLS itmProcess = new  CarsWorkOrdersItemReversedProcessedDetailCLS ();
            foreach (var itm in carsWorkOrderCls.ItemReversedProcessedList)
            {
                itm.Is_AddToInvoice = 1;
                itmProcess.EditItemProcess(ref opp, itm);

            }

            try
            {
                ItemCLS itmCls = new ItemCLS(ref opp);
                foreach (var item in invdList)
                {
                    item.Invoice_Id = New_Invoice_Id;
                    item.Item_ProcessedFlage = 100;
                    invCls.AddInvoices_Details_Reverse(ref excp, ref msg, ref opp, item);


                    //itmCls.UpdateItemPriceFromInvoice(ref excp, ref msg, item);
                }
            }
            catch (Exception ee)
            {

                opp.Rollback();
                excp = 1;
                msg = ee.Message;
            }
        }

        private void Delete_Invoices_Details_Reverse(ref int Excp, ref string Msg, ref operations opp, List<Invoices_Details_Reverse> invdList, CarsWorkOrdersItemReversedProcessedDetailCLS itmProcess, Invoices_List_ReverseCls invCls)
        {
            try
            {
                int Is_AddToInvoice = 0;
                foreach (var item in invdList)
                {
                    //item.Invoice_Id = invL.Id;
                    //item.Item_ProcessedFlage = 100;
                    long CarsWorkOrdersItemProcessedDetailsId = item.CarsWorkOrdersItemProcessedDetailsId ?? 0;
                    itmProcess.EditItemReversedProcess(ref opp, CarsWorkOrdersItemProcessedDetailsId, Is_AddToInvoice);
                    invCls.DeleteInvoices_Details_Reverse(ref Excp, ref Msg, ref opp, item);

                    if (Excp == 1)
                    {

                        opp.Rollback();
                        return;
                    }

                }
            }
            catch (Exception ee)
            {

                opp.Rollback();
                Msg = ee.Message;
            }
        }

        public void Edit_InvoiceReverse_ForCars(ref int Excp, ref string Msg, ref operations opp, CarsWorkOrdersCLS carsWorkOrderCls, Invoices_List_Reverse invL, List<Invoices_Details_Reverse> invdList)
        {
            //// ﬂÊÌ‰  ›«’Ì· «·›« Ê—… „‰ ÃœÌœ
            List<Invoices_Details_Reverse> invdListNew = carsWorkOrderCls.GetInvoices_Details_ReverseList(carsWorkOrderCls.ItemReversedProcessedList);

            opp.BeginTransaction(2);

            try
            {
                CarsWorkOrdersItemReversedProcessedDetailCLS itmProcess = new  CarsWorkOrdersItemReversedProcessedDetailCLS ();
                Invoices_List_ReverseCls invCls = new  Invoices_List_ReverseCls ();

                TaxReverseInvoices_ListCLS ts = new  TaxReverseInvoices_ListCLS (ref opp);
                ////Get Portal InternalId
                long TaxInvoiceNo = 0;
                string internalID = "";
                if (invL.IsInvoiceTaxInfo == 1)
                {
                    ////Get Portal InternalId
                    TaxInvoiceNo = 0;
                    internalID = ts.GetPortalInternalId(ref opp, ref TaxInvoiceNo, invL);
                }

                invL.TaxInvoiceNo = TaxInvoiceNo;
                invL.TaxInternalId = internalID;


                //Õ›Ÿ  ⁄œÌ·«  »Ì«‰«  «·›« Ê—…
                invCls.EditInvoices_List_Reverse(ref Excp, ref Msg, ref opp, invL);
                New_Invoice_Id = invL.Id;
                if (Excp == 1)
                {

                    opp.Rollback();
                    return;
                }

                //// Õ–› «’‰«› «·›« Ê—… «·ﬁœÌ„
                ///

                Delete_Invoices_Details_Reverse(ref Excp, ref Msg, ref opp, invdList, itmProcess, invCls);
                if (Excp == 1)
                {

                    opp.Rollback();
                    return;
                }


                ///«‰‘«¡  ›«’Ì· «·›« Ê—… „‰ ÃœÌœ
                ///
                Make_Invoice_DetailsReverse(ref Excp, ref Msg, ref opp, New_Invoice_Id, invdListNew, invCls, carsWorkOrderCls);
                if (Excp == 1)
                {

                    New_Invoice_Id = 0;
                    invL.Id = 0;
                    opp.Rollback();
                    return;
                }

                /////////////////////
                //carsWorkOrderCls.carWorkOrder.Last_Invoice_ListId = invL.Id;
                carsWorkOrderCls.carWorkOrder.TaxReverseInternalId = invL.TaxInternalId;
                carsWorkOrderCls.carWorkOrder.TaxReverseInvoiceNo = invL.TaxInvoiceNo;
                //cc.carWorkOrder = carsWorkOrder;
                carsWorkOrderCls.EditWorkOrder(ref Excp, ref Msg);
                if (Excp == 1)
                {

                    New_Invoice_Id = 0;
                    invL.Id = 0;
                    opp.Rollback();
                    return;
                }

                //////////// Save Invoice in  TaxInvoices_Lis
                //  SaveInvoiceInTaxInvoices_List(ref Excp, ref Msg, ref opp, invL.Id);
                ts.ReSetTaxReverseInvoices_List_From_ERPWithInvoice(ref Excp, ref Msg, invL.Id);

                if (Excp == 1)
                {
                    Msg = "Œÿ√ ›Ï  «⁄œ«œ «·›« Ê—…";
                    New_Invoice_Id = 0;
                    invL.Id = 0;
                    opp.Rollback();
                    return;
                }

                if (Excp == 0)
                {
                    SetCustomerAccountsForInvoiceReverse(ref Excp, ref Msg, ref opp, invL, carsWorkOrderCls.carWorkOrder);
                }
                if (Excp == 0)
                {
                    opp.Commit();
                }


            }
            catch (Exception ee)
            {
                opp.Rollback();
                Excp = 1;
            }
        }

        public void Delete_InvoiceReverse_ForCars(ref int Excp, ref string Msg, ref operations opp, long Invoices_List_ReverseId)
        {
            New_Invoice_Id = Invoices_List_ReverseId;
            opp.BeginTransaction(2);

            CarsWorkOrdersItemReversedProcessedDetailCLS itmProcess = new  CarsWorkOrdersItemReversedProcessedDetailCLS ();

            Invoices_List_ReverseCls invCls = new  Invoices_List_ReverseCls ();
            Invoices_List_Reverse invL = invCls.GetInvoices_List_ReverseById(ref opp, Invoices_List_ReverseId);
            List<Invoices_Details_Reverse> invdList = invCls.GetInvoices_Details_Reverse(ref opp, Invoices_List_ReverseId);

            CarsWorkOrdersCLS cw = new CarsWorkOrdersCLS();
            long carsWorkOrdersId = invL.CarsWorkOrdersId ?? 0;
            CarsWorkOrder carworkOrder = cw.SetWorkOrderOpenAndSendToWarsha(ref Excp, ref Msg, ref opp, carsWorkOrdersId);


            try
            {
                invCls.DeleteInvoices_List_Reverse(ref Excp, ref Msg, ref opp, invL);

                if (Excp == 1)
                {

                    opp.Rollback();
                    return;
                }


                Delete_Invoices_Details_Reverse(ref Excp, ref Msg, ref opp, invdList, itmProcess, invCls);
                if (Excp == 1)
                {

                    opp.Rollback();
                    return;
                }
                if (Excp == 0)
                {
                    SetCustomerAccountsForInvoiceReverse(ref Excp, ref Msg, ref opp, invL, carworkOrder);
                }
                if (Excp == 0)
                {
                    opp.Commit();
                }

            }
            catch (Exception ee)
            {
                opp.Rollback();
                Excp = 1;
            }
        }



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
    }
}
