using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 
using System.Xml.Linq;

namespace AccountModelsLib.Models
{


    public class TaxReverseInvoices_ListCLS
    {
        string documentType = "c";
        public operations opp;
        public TaxReverseInvoices_List invItm = new TaxReverseInvoices_List();
        public List<TaxReverseInvoices_List> invItmList = new List<TaxReverseInvoices_List>();

        public TaxReverseInvoices_ListCLS()
        {
            opp = new operations();
            //activeFrom = new DateTime();
            //codeName = "";
        }
        public TaxReverseInvoices_ListCLS(ref operations opp_)
        {
            opp = opp_;

        }
        public List<TaxReverseInvoices_List> GetTaxReverseInvoices_List()
        {

            invItmList = opp.db.TaxReverseInvoices_List.ToList();


            return invItmList;
        }
        public TaxReverseInvoices_List GetTaxReverseInvoices_List(long Id)
        {
            invItm = new TaxReverseInvoices_List();
            invItm = opp.db.TaxReverseInvoices_List.Where(c => c.Id == Id).FirstOrDefault();
            return invItm;

        }
        public void SaveGetTaxReverseInvoices_List(ref int Excp, ref string Msg)
        {

            try
            {
                if (invItm.Id == 0)
                {

                    opp.db.TaxReverseInvoices_List.Add(invItm);
                }
                else
                {
                    opp.db.Entry(invItm).State = EntityState.Modified;
                }
                opp.db.SaveChanges();


                Excp = 0;
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
            }
        }
        public void AddNewTaxReverseInvoices_List(ref int Excp, ref string Msg, ref operations opp, TaxReverseInvoices_List invItm)
        {


            if (invItm.Id == 0)
            {

                try
                {
                    opp.db.TaxReverseInvoices_List.Add(invItm);
                    opp.db.SaveChanges();
                    Excp = 0;
                }
                catch (Exception ee)
                {
                    Excp = 1;
                    Msg = ee.Message;
                }
            }

        }

        public void SaveGetTaxReverseInvoices_List(ref int Excp, ref string Msg, ref operations opp, TaxReverseInvoices_List invItm)
        {


            if (invItm.Id == 0)
            {

                try
                {
                    opp.db.TaxReverseInvoices_List.Add(invItm);
                    opp.db.SaveChanges();
                    Excp = 0;
                }
                catch (Exception ee)
                {
                    Excp = 1;
                    Msg = ee.Message;
                }
            }
            else
            {
                try
                {
                    opp.db.Entry(invItm).State = EntityState.Modified;
                    opp.db.SaveChanges();
                    Excp = 0;
                }
                catch (Exception ee)
                {
                    Excp = 2;
                    Msg = ee.Message;
                }

            }



        }

        public void DeleteTaxReverseInvoices_List(ref int Excp, ref string Msg, ref int DeletedRowCount, long Id)
        {
            try
            {
                TaxReverseInvoices_List cur = opp.db.TaxReverseInvoices_List.Where(c => c.Id == Id).FirstOrDefault();
                DeleteTaxReverseInvoices_List(ref Excp, ref Msg, ref DeletedRowCount, cur);
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
            }

        }
        public void DeleteTaxReverseInvoices_List(ref int Excp, ref string Msg, ref int DeletedRowCount, TaxReverseInvoices_List invItm_)
        {
            DeletedRowCount = 0;
            try
            {
                if (invItm_ != null)
                {
                    opp.db.TaxReverseInvoices_List.Remove(invItm_);
                    DeletedRowCount = opp.db.SaveChanges();
                    Excp = 0;
                }
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
            }

        }


        public string GetPortalInternalId(ref operations opp, ref long TaxInvoiceNo, Invoices_List_Reverse invL)
        {
            string internalID = "";

             

            
                if (invL.TaxInvoiceNo == 0 | invL.TaxInvoiceNo == null)
                {
                    //TaxReverseInvoices_ListCLS taxReverseInvoices_ListCLS = new TaxReverseInvoices_ListCLS(ref opp);
                    internalID = GetNewinternalID(ref TaxInvoiceNo);
                }
                else
                {
                    internalID = invL.TaxInternalId;
                    TaxInvoiceNo = invL.TaxInvoiceNo ?? 1;
                }

           
            

             
            return internalID;
        }
     public    string GetNewinternalID(ref long TaxInvoiceNo)
        {
            TaxInvoiceNo = GetNewTaxInvoiceNo(ref TaxInvoiceNo);


            string internalID = Common.TaxinternalID_Prevx + TaxInvoiceNo.ToString();

            return internalID;

        }
        long GetNewTaxInvoiceNoFrom_TaxReverseInvoices_List()
        {
            long TaxInvoiceNo = 1;
            var TaxInNo_ = opp.db.TaxReverseInvoices_List.OrderByDescending(t => t.TaxInvoiceNo).FirstOrDefault();


            if (TaxInNo_ != null)
            {
                TaxInvoiceNo = TaxInNo_.TaxInvoiceNo.Value + 1;

            }
            else
            {
                TaxInvoiceNo = Common.TaxinternalID_StartFromNumber;
            }
            return TaxInvoiceNo;
        }
        long GetNewTaxInvoiceNoFrom_ReverseInvoices_List()
        {
            long TaxInvoiceNo = 1;
            var TaxInNo_ = opp.db.Invoices_List_Reverse.OrderByDescending(t => t.TaxInvoiceNo).FirstOrDefault();


            if (TaxInNo_ != null)
            {
                TaxInNo_.TaxInvoiceNo = TaxInNo_.TaxInvoiceNo ?? 0;
                TaxInvoiceNo = TaxInNo_.TaxInvoiceNo.Value + 1;

            }
            else
            {
                TaxInvoiceNo = 1;// Common.TaxinternalID_StartFromNumber;
            }
            return TaxInvoiceNo;
        }
        long GetNewTaxInvoiceNoFrom_CarsWorkOrders()
        {
            long TaxInvoiceNo = 1;
            var TaxInNo_ = opp.db.CarsWorkOrders.OrderByDescending(t => t.TaxInvoiceNo).FirstOrDefault();


            if (TaxInNo_ != null)
            {
                TaxInvoiceNo = TaxInNo_.TaxInvoiceNo.Value + 1;

            }
            else
            {
                TaxInvoiceNo = Common.TaxinternalID_StartFromNumber;
            }
            return TaxInvoiceNo;
        }
        long GetNewTaxInvoiceNo(ref long TaxInvoiceNo)
        {

            TaxInvoiceNo = 1;
            
                TaxInvoiceNo = GetNewTaxInvoiceNoFrom_ReverseInvoices_List();

            


            return TaxInvoiceNo;
        }
        public bool CheckIsSetUpInvoiceToPortal(long ReverseInvoices_ListId, ref long TaxReverseInvoices_List)
        {
            bool IsSetUpInvoiceToPortal = false;
            TaxReverseInvoices_List = 0;
            TaxReverseInvoices_List txInv = opp.db.TaxReverseInvoices_List.Where(t => t.Invoices_ListId == ReverseInvoices_ListId).FirstOrDefault();
            if (txInv == null)
            {
                IsSetUpInvoiceToPortal = false;
            }
            else
            {
                TaxReverseInvoices_List = txInv.Id;
                IsSetUpInvoiceToPortal = true;
            }
            return IsSetUpInvoiceToPortal;
        }

        public void ReSetTaxReverseInvoices_List_From_ERPWithInvoice(ref int Excp, ref string Msg, long ReverseInvoices_ListId)
        {




            DeleteSetUpInvoiceToPortal(ref Excp, ref Msg, ReverseInvoices_ListId);
            if (Excp == 0)
            {
                try
                {
                    MakeSetTaxReverseInvoices_List_From_ERP(ref Excp, ref Msg, ReverseInvoices_ListId);

                }
                catch (Exception ee)
                {
                    Excp = 1;
                    Msg = ee.Message;

                }
            }

        }

        public void ReSetTaxReverseInvoices_List_From_ERP(ref int Excp, ref string Msg, long ReverseInvoices_ListId)
        {
            Excp = 0;

            opp.BeginTransaction(2);

            try
            {
                ReSetTaxReverseInvoices_List_From_ERPWithInvoice(ref Excp, ref Msg, ReverseInvoices_ListId);
                if (Excp == 0)
                {
                    opp.Commit();
                }
                else
                {
                    opp.Rollback();
                }
            }
            catch (Exception ee)
            {
                Msg = ee.Message;
                Excp = 1;
                opp.Rollback();
            }



        }
        public void ReSetTaxReverseInvoices_List_From_ERP_(ref int Excp, ref string Msg, long ReverseInvoices_ListId)
        {
            Excp = 0;
            Msg = "";
            opp.BeginTransaction(2);

            DeleteSetUpInvoiceToPortal(ref Excp, ref Msg, ReverseInvoices_ListId);
            if (Excp == 0)
            {
                try
                {
                    MakeSetTaxReverseInvoices_List_From_ERP(ref Excp, ref Msg, ReverseInvoices_ListId);


                    if (Excp == 0)
                    {
                        opp.Commit();
                    }
                    else
                    {
                        opp.Rollback();
                    }
                }
                catch (Exception ee)
                {
                    Excp = 1;
                    Msg = ee.Message;
                    opp.Rollback();
                }
            }
            else
            {
                Excp = 1;

                opp.Rollback();
            }
        }

        private void DeleteSetUpInvoiceToPortal(ref int Excp, ref string Msg, long Invoices_List_ReverseId)
        {
            Excp = 0;

            try
            {
                long taxInvoices_ListId = 0;
                ////////////
                ///
                Invoices_List_Reverse Inv = opp.db.Invoices_List_Reverse.Where(t => t.Id == Invoices_List_ReverseId).FirstOrDefault();
                if (Inv != null)
                {
                    taxInvoices_ListId = Inv.TaxInvoices_ListId ?? 0;

                    Inv.TaxInvoices_ListId = 0;
                    opp.db.Entry(Inv).State = EntityState.Modified;
                    opp.db.SaveChanges();
                }

                if (taxInvoices_ListId == 0)
                {

                    return;
                }

                TaxReverseInvoices_List txInv = opp.db.TaxReverseInvoices_List.Where(t => t.Id == taxInvoices_ListId).FirstOrDefault();
                List<TaxReverseInvoices_ItemsTaxes> txItemsTaxes = opp.db.TaxReverseInvoices_ItemsTax.Where(t => t.TaxInvoices_ListId == taxInvoices_ListId).ToList();
                List<TaxReverseInvoices_LineItems> txLineItems = opp.db.TaxReverseInvoices_LineItems.Where(t => t.TaxInvoices_ListId == taxInvoices_ListId).ToList();
                List<TaxReverseInvoices_TaxesTotal> txTaxesTotal = opp.db.TaxReverseInvoices_TaxesTotal.Where(t => t.TaxInvoices_ListId == taxInvoices_ListId).ToList();

                if (txInv != null)
                {
                    opp.db.TaxReverseInvoices_List.Remove(txInv);
                    opp.db.SaveChanges();
                }
                if (txItemsTaxes != null)
                {
                    opp.db.TaxReverseInvoices_ItemsTax.RemoveRange(txItemsTaxes);
                    opp.db.SaveChanges();
                }
                if (txLineItems != null)
                {
                    opp.db.TaxReverseInvoices_LineItems.RemoveRange(txLineItems);
                    opp.db.SaveChanges();
                }
                if (txTaxesTotal != null)
                {
                    opp.db.TaxReverseInvoices_TaxesTotal.RemoveRange(txTaxesTotal);
                    opp.db.SaveChanges();
                }



            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;

            }
        }
        public void SetTaxReverseInvoices_List_From_ERP(ref int Excp, ref string Msg, long ReverseInvoices_ListId)
        {
            Excp = 0;
            Msg = "";
            opp.BeginTransaction(2);

            try
            {
                MakeSetTaxReverseInvoices_List_From_ERP(ref Excp, ref Msg, ReverseInvoices_ListId);


                if (Excp == 0)
                {
                    opp.Commit();
                }
                else
                {
                    opp.Rollback();
                }
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
                opp.Rollback();
            }

        }

        void MakeSetTaxReverseInvoices_List_From_ERP(ref int Excp, ref string Msg, long Invoices_List_ReverseId)
        {
            Excp = 0;
            Msg = "";

            ////Get  Invoice List
             Invoices_List_Reverse inv = new  Invoices_List_Reverse ();
            TaxReverseInvoices_List txInv = new TaxReverseInvoices_List();
            try
            {
                txInv = SetTaxReverseInvoices_ListFromERP(ref Excp, ref Msg, ref inv, Invoices_List_ReverseId);

            }
            catch (Exception ee)
            {
                Msg = ee.Message;
                Excp = 1;
            }

            if (Excp == 0)
            {
                AddNewTaxReverseInvoices_List(ref Excp, ref Msg, ref opp, txInv);
            }
            if (Excp == 0)
            {
                ///Set Invoice Items Details Line
                /// Item detailes
                /// //Items Tax
                /// //Tax Total
                long TaxReverseInvoices_ListId = txInv.Id;

                TaxReverseInvoices_LineItemsCLS taxReverseInvoices_LineItemsCLS = new TaxReverseInvoices_LineItemsCLS(ref opp);

                taxReverseInvoices_LineItemsCLS.SetInvoiceLinesFromERP(ref Excp, ref Msg, inv, Invoices_List_ReverseId, TaxReverseInvoices_ListId);
                if (Excp == 0)
                {
                    inv.TaxInvoices_ListId = TaxReverseInvoices_ListId;
                    opp.db.Entry(inv).State = EntityState.Modified;
                    opp.db.SaveChanges();

                }

            }


        }
        public TaxReverseInvoices_List SetTaxReverseInvoices_ListFromERP(ref int Excp, ref string Msg, ref  Invoices_List_Reverse inv, long Invoices_List_ReverseId)
        {
            TaxReverseInvoices_List taxReverseInvoices_List = new TaxReverseInvoices_List();

            try
            {

                inv = opp.db.Invoices_List_Reverse.Where(i => i.Id == Invoices_List_ReverseId).FirstOrDefault();
                long carsWorkOrderId = inv.CarsWorkOrdersId ?? 0;

                CarsWorkOrder carsWorkOrder = opp.db.CarsWorkOrders.Where(c => c.Id == carsWorkOrderId).Include(c => c.Car).FirstOrDefault();

                long Customer_Id = inv.Customer_Id.Value;
                //CustomerCLS csCls = new CustomerCLS();
                //Customer cs = csCls.GetCustomer(Customer_Id);

                taxReverseInvoices_List.Invoices_ListId = Invoices_List_ReverseId;
                taxReverseInvoices_List.Customer_Id = inv.Customer_Id.Value;
                taxReverseInvoices_List.Tax_BranchesId = Common.CommonTaxConfig.Tax_BranchesId;
                //long TaxInvoiceNo = 0;
                //taxReverseInvoices_List.internalID = GetNewinternalID(ref TaxInvoiceNo);
                //taxReverseInvoices_List.TaxInvoiceNo = TaxInvoiceNo;

                taxReverseInvoices_List.internalID = inv.TaxInternalId;//   GetNewinternalID(ref TaxInvoiceNo);
                taxReverseInvoices_List.TaxInvoiceNo = inv.TaxInvoiceNo;// ;
                //Receiver_type = cs.LookUp_PersonCompany_Type,
                //Receiver_id = cs.Tax_No,
                //Receiver_name = cs.Name,



                taxReverseInvoices_List.dateTimeIssued = inv.Invoice_Date.Value;
                //taxpayerActivityCode = Common.CommonTaxConfig.ActivityCode,

                taxReverseInvoices_List.totalDiscountAmount = inv.Net_Total_CommissionValue;
                taxReverseInvoices_List.totalSalesAmount = inv.Total;
                taxReverseInvoices_List.netAmount = inv.Total_PriceWithoutTax;
                taxReverseInvoices_List.taxTotals = inv.TaxSales10_Value;
                taxReverseInvoices_List.totalAmount = inv.Net_Total;
                taxReverseInvoices_List.extraDiscountAmount = 0;
                taxReverseInvoices_List.totalItemsDiscountAmount = inv.Net_Total_CommissionValue;

                if (carsWorkOrder.Car != null)
                {
                    taxReverseInvoices_List.purchaseOrderReference = carsWorkOrder.Car.CarNumber;
                }
                else
                {
                    taxReverseInvoices_List.purchaseOrderReference = "";
                }
                taxReverseInvoices_List.purchaseOrderDescription = "";
                taxReverseInvoices_List.salesOrderReference = carsWorkOrder.CustomerOrderNumberPO;
                taxReverseInvoices_List.salesOrderDescription = "";
                taxReverseInvoices_List.proformaInvoiceNumber = carsWorkOrder.CustomerOrderNumberGR;




                SetReceiverData(Customer_Id, documentType, ref taxReverseInvoices_List);

                Excp = 0;

            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;

            }
            return taxReverseInvoices_List;

        }

        public void SetReceiverData(long Customer_Id, string documentType, ref TaxReverseInvoices_List taxReverseInvoices_List)
        {
            CustomerCLS csCls = new CustomerCLS();
            Customer cs = csCls.GetCustomer(Customer_Id);
            if (cs != null)
            {
                SetReceiverData(cs, documentType, ref taxReverseInvoices_List);
            }
        }
        public void SetReceiverData(Customer cs, string documentType, ref TaxReverseInvoices_List taxReverseInvoices_List)
        {
            if (cs.General_Customer_Flage == 1)
            {
                taxReverseInvoices_List.Receiver_type = "P";
            }
            else
            {
                taxReverseInvoices_List.Receiver_type = "B";
            }
            //taxReverseInvoices_List.Receiver_type = cs.LookUp_PersonCompany_Type;
            taxReverseInvoices_List.Receiver_id = cs.Tax_No;
            taxReverseInvoices_List.Receiver_name = cs.Name;


            taxReverseInvoices_List.issuer_type = Common.CommonTaxConfig.IssuerType;
            taxReverseInvoices_List.issuer_id = Common.CommonTaxConfig.IssuerId;
            taxReverseInvoices_List.issuer_name = Common.CommonTaxConfig.IssuerName;

            taxReverseInvoices_List.Tax_BranchesId = Common.CommonTaxConfig.Tax_BranchesId;
            taxReverseInvoices_List.documentTypeVersion = Common.CommonTaxConfig.documentTypeVersion;
            taxReverseInvoices_List.taxpayerActivityCode = Common.CommonTaxConfig.ActivityCode;
            taxReverseInvoices_List.documentType = documentType; //InvoiceType

        }

        public bool CheckTaxInvoiceNoFound(long taxInvoiceNo, string internalID)
        {
            bool IsFound = false;
            var TaxInNo = opp.db.TaxReverseInvoices_List.Where(t => t.TaxInvoiceNo == taxInvoiceNo | t.internalID == internalID).FirstOrDefault();
            if (TaxInNo != null)
            {
                IsFound = true;
            }


            return IsFound;
        }
    }
}
