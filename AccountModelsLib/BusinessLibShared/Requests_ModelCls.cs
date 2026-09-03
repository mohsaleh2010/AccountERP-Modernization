using System;
using System.Collections.Generic;
using System.Text;
//using Connection4It;  using AccountModelsLib; using AccountModelsLib; using AccountModelsLib.Models;
using System.Data;
using System.Collections;
 
//using Store_LIB;
//using Account_Common_LIB;
using AccountModelsLib.Models;
namespace AccountModelsLib
{
    public partial class Requests_ModelCls
    {
        DataSet G_DS;
        public long New_Invoice_Id;
        public Requests_ModelCls()
        {
            G_DS = new DataSet();
        }
        public int CommissionISValue = 1;



        public int Make_Invoice_Agile { get; set; } = 1;
        public decimal PaidValueFromMarket = 0;
        public decimal RestValue = 0;

        public bool BtnOkPressMakeOrderMoney = false;
        public ArrayList arr_Order_Purshing_Id_Automatic; //·Õ›Ÿ —ﬁ„ «·«·Ï ·«’œ«— «·«–‰ ÊÌ” Œœ„ ··ÿ»«⁄…

       
        public void Make_Invoice_ForCars(ref int Excp, ref string Msg, ref operations opp, CarsWorkOrdersCLS carsWorkOrderCls, Invoices_List invL)
        {
            List<Invoices_Details> invdList = carsWorkOrderCls.GetInvoices_DetailsList(carsWorkOrderCls.ItemProcessedList);
            opp.BeginTransaction(2);


            TaxInvoices_ListCLS ts = new TaxInvoices_ListCLS(ref opp);
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
                Invoices_ListCls invCls = new Invoices_ListCls();
                invCls.AddInvoices_List(ref Excp, ref Msg, ref opp, invL);
                New_Invoice_Id = invL.Id;
                if (Excp == 1)
                {
                    New_Invoice_Id = 0;
                    invL.Id = 0;
                    opp.Rollback();
                    return;
                }

                Make_Invoice_Details(ref Excp, ref Msg, ref opp, New_Invoice_Id, invdList, invCls, carsWorkOrderCls);
                if (Excp == 1)
                {

                    New_Invoice_Id = 0;
                    invL.Id = 0;
                    opp.Rollback();
                    return;
                }

                //CarsWorkOrdersCLS cc = new CarsWorkOrdersCLS(ref opp);
                carsWorkOrderCls.carWorkOrder.Last_Invoice_ListId = invL.Id;
                carsWorkOrderCls.carWorkOrder.TaxInternalId = invL.TaxInternalId;
                carsWorkOrderCls.carWorkOrder.TaxInvoiceNo = invL.TaxInvoiceNo;
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
                ts.ReSetTaxInvoices_List_From_ERPWithInvoice(ref Excp, ref Msg, invL.Id);

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
                    SetCustomerAccountsForInvoice(ref Excp, ref Msg, ref opp, invL, carsWorkOrderCls.carWorkOrder);
                }
               

            }
            catch (Exception ee)
            {
                opp.Rollback();
                Excp = 1;
            }
        }

        //private void SaveInvoiceInTaxInvoices_List(ref int Excp, ref string Msg, ref operations opp, long id)
        //{
        //    Excp = 0;
        //    Msg = "";
        //    TaxInvoices_ListCLS ts = new TaxInvoices_ListCLS(ref opp);
        //    ts.ReSetTaxInvoices_List_From_ERPWithInvoice(ref Excp, ref Msg, id);

        //}

        public void Edit_Invoice_ForCars(ref int Excp, ref string Msg, ref operations opp, CarsWorkOrdersCLS carsWorkOrderCls, Invoices_List invL, List<Invoices_Details> invdList)
        {
            //// ﬂÊÌ‰  ›«’Ì· «·›« Ê—… „‰ ÃœÌœ
            List<Invoices_Details> invdListNew = carsWorkOrderCls.GetInvoices_DetailsList(carsWorkOrderCls.ItemProcessedList);

            opp.BeginTransaction(2);

            try
            {
                CarsWorkOrdersItemProcessedDetailCLS itmProcess = new CarsWorkOrdersItemProcessedDetailCLS();
                Invoices_ListCls invCls = new Invoices_ListCls();

                TaxInvoices_ListCLS ts = new TaxInvoices_ListCLS(ref opp);
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
                invCls.EditInvoices_List(ref Excp, ref Msg, ref opp, invL);
                New_Invoice_Id = invL.Id;
                if (Excp == 1)
                {

                    opp.Rollback();
                    return;
                }

                //// Õ–› «’‰«› «·›« Ê—… «·ﬁœÌ„
                ///

                Delete_InvoiceDetails(ref Excp, ref Msg, ref opp, invdList, itmProcess, invCls);
                if (Excp == 1)
                {

                    opp.Rollback();
                    return;
                }


                ///«‰‘«¡  ›«’Ì· «·›« Ê—… „‰ ÃœÌœ
                ///
                Make_Invoice_Details(ref Excp, ref Msg, ref opp, New_Invoice_Id, invdListNew, invCls, carsWorkOrderCls);
                if (Excp == 1)
                {

                    New_Invoice_Id = 0;
                    invL.Id = 0;
                    opp.Rollback();
                    return;
                }

                /////////////////////
                //carsWorkOrderCls.carWorkOrder.Last_Invoice_ListId = invL.Id;
                carsWorkOrderCls.carWorkOrder.TaxInternalId = invL.TaxInternalId;
                carsWorkOrderCls.carWorkOrder.TaxInvoiceNo = invL.TaxInvoiceNo;
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
                ts.ReSetTaxInvoices_List_From_ERPWithInvoice(ref Excp, ref Msg, invL.Id);
               
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
                    SetCustomerAccountsForInvoice(ref Excp, ref Msg, ref opp, invL, carsWorkOrderCls.carWorkOrder);
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

        public void Delete_Invoice_ForCars(ref int Excp, ref string Msg, ref operations opp, long Invoices_List_Id)
        {
            New_Invoice_Id = Invoices_List_Id;
            opp.BeginTransaction(2);

            CarsWorkOrdersItemProcessedDetailCLS itmProcess = new CarsWorkOrdersItemProcessedDetailCLS();

            Invoices_ListCls invCls = new Invoices_ListCls();
            Invoices_List invL = invCls.GetInvoices_ListById(ref opp, Invoices_List_Id);
            List<Invoices_Details> invdList = invCls.GetInvoices_Details(ref opp, Invoices_List_Id);

            CarsWorkOrdersCLS cw = new CarsWorkOrdersCLS();
            long carsWorkOrdersId = invL.CarsWorkOrdersId ?? 0;
            CarsWorkOrder carworkOrder= cw.SetWorkOrderOpenAndSendToWarsha(ref Excp, ref Msg, ref opp, carsWorkOrdersId);


            try
            {
                //EditInvoices_List(ref int Excp, ref string Msg, ref operations opp, Invoices_List invList)
                invL.Is_DeletedProcessed = 1;
                //invL.Customer_Id = 0;
                invCls.EditInvoices_List(ref Excp, ref Msg, ref opp, invL);
                //invCls.DeleteInvoices_List(ref Excp, ref Msg, ref opp, invL);

                if (Excp == 1)
                {

                    opp.Rollback();
                    return;
                }


                Delete_InvoiceDetails(ref Excp, ref Msg, ref opp, invdList, itmProcess, invCls);
                if (Excp == 1)
                {

                    opp.Rollback();
                    return;
                }
                if (Excp == 0)
                {
                    SetCustomerAccountsForInvoice(ref Excp, ref Msg, ref opp, invL, carworkOrder);
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

        private void SetCustomerAccountsForInvoice(ref int Excp, ref string Msg, ref operations opp, Invoices_List invL,CarsWorkOrder cworkOrder)
        {
            try
            {
                long WorkOrderNumber = cworkOrder.WorkOrderNumber??0;
                invL.TaxInternalId = invL.TaxInternalId ?? "";
                CustomersAccountCLS ca = new CustomersAccountCLS();
                //if (invL.Notes.Trim() == "")
                //{
                //string Notes= " ›« Ê—… „»Ì⁄«   —ﬁ„    " + invL.Invoice_Id;

                string Notes = " √„— «·‘€· —ﬁ„ " + WorkOrderNumber.ToString();
                    if (invL.TaxInternalId.Trim() != "")
                    {
                    Notes = Notes + " Ê „ —›⁄Â« «·ﬂ —Ê‰Ì« »—ﬁ„ " + invL.TaxInternalId.ToString();
                        
                    }
                invL.Notes = invL.Notes + " "+ Notes;
                //}
                ca.Set_Customer_Balance_Invoice(ref Excp, ref opp, (int)invL.Customer_Id, invL.Customer_Id_RelatedToAccount ?? 0, invL.Id,  invL.Net_Total ?? 0, invL.Notes);

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
                
                //«‰‘«¡ «·ﬁÌœ
                Account_Entry_Invoice_CLS aa = new Account_Entry_Invoice_CLS();
                aa.IsNotDeletePrevEntry = false;
                aa.Make_EntryForInvoices(ref Excp, ref Msg, ref opp, invL.Id, invL.Invoice_Date.Value, invL.Invoice_Date.Value, -1);
                //aa.Make_EntryForInvoices_Reverse(ref Excp, ref Msg, ref opp, invL.Id, invL.Invoice_Date.Value, invL.Invoice_Date.Value, -1);

                //////// Delete Old Entry
                //AccountEntry_CLS aaCLS = new AccountEntry_CLS();


                ///////Delete Old Entry
                //aaCLS.Reference_Type = Common.Invoice_Type_Id;
                //aaCLS.Reference_Type_Table_Id = Invoices_List_Id;
                //AccountEntry_CLS ac = new AccountEntry_CLS();
                //aaCLS.Delete_EntryFromAutomatic(ref opp, ref Excp, ref Msg, aaCLS.Reference_Type_Table_Id, aaCLS.Reference_Type, IsNotDeletePrevEntry, Account_Entry_List_ID);



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

        private void Make_Invoice_Details(ref int excp, ref string msg, ref operations opp, long New_Invoice_Id, List<Invoices_Details> invdList, Invoices_ListCls invCls, CarsWorkOrdersCLS carsWorkOrderCls)
        {
            CarsWorkOrdersItemProcessedDetailCLS itmProcess = new CarsWorkOrdersItemProcessedDetailCLS();
            foreach (var itm in carsWorkOrderCls.ItemProcessedList)
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
                    invCls.AddInvoices_Details(ref excp, ref msg, ref opp, item);

                    
                    itmCls.UpdateItemPriceFromInvoice(ref excp, ref msg, item);
                }
            }
            catch (Exception ee)
            {

                opp.Rollback();
                excp = 1;
                msg = ee.Message;
            }
        }
       
        private void Delete_InvoiceDetails(ref int Excp, ref string Msg, ref operations opp, List<Invoices_Details> invdList, CarsWorkOrdersItemProcessedDetailCLS itmProcess, Invoices_ListCls invCls)
        {
            try
            {
                int Is_AddToInvoice = 0;
                foreach (var item in invdList)
                {
                    //item.Invoice_Id = invL.Id;
                    //item.Item_ProcessedFlage = 100;
                    long CarsWorkOrdersItemProcessedDetailsId = item.CarsWorkOrdersItemProcessedDetailsId ?? 0;
                    itmProcess.EditItemProcess(ref opp, CarsWorkOrdersItemProcessedDetailsId, Is_AddToInvoice);
                    invCls.DeleteInvoices_Details(ref Excp, ref Msg, ref opp, item);

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
    }
}
