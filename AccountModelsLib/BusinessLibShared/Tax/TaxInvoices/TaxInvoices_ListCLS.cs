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


    public class TaxInvoices_ListCLS
    {
        string documentType = "i";
        public operations opp;
        public TaxInvoices_List invItm = new TaxInvoices_List();
        public List<TaxInvoices_List> invItmList = new List<TaxInvoices_List>();

        public TaxInvoices_ListCLS()
        {
            opp = new operations();
            //activeFrom = new DateTime();
            //codeName = "";
        }
        public TaxInvoices_ListCLS(ref operations opp_)
        {
            opp = opp_;

        }
        public List<TaxInvoices_List> GetTaxInvoices_List()
        {

            invItmList = opp.db.TaxInvoices_List.ToList();


            return invItmList;
        }
        public TaxInvoices_List GetTaxInvoices_List(long Id)
        {
            invItm = new TaxInvoices_List();
            invItm = opp.db.TaxInvoices_List.Where(c => c.Id == Id).FirstOrDefault();
            return invItm;

        }
        public void SaveGetTaxInvoices_List(ref int Excp, ref string Msg)
        {

            try
            {
                if (invItm.Id == 0)
                {

                    opp.db.TaxInvoices_List.Add(invItm);
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
        public void AddNewTaxInvoices_List(ref int Excp, ref string Msg, ref operations opp, TaxInvoices_List invItm)
        {


            if (invItm.Id == 0)
            {

                try
                {
                    opp.db.TaxInvoices_List.Add(invItm);
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

        public void SaveGetTaxInvoices_List(ref int Excp, ref string Msg, ref operations opp, TaxInvoices_List invItm)
        {


            if (invItm.Id == 0)
            {

                try
                {
                    opp.db.TaxInvoices_List.Add(invItm);
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

        public void DeleteTaxInvoices_List(ref int Excp, ref string Msg, ref int DeletedRowCount, long Id)
        {
            try
            {
                TaxInvoices_List cur = opp.db.TaxInvoices_List.Where(c => c.Id == Id).FirstOrDefault();
                DeleteTaxInvoices_List(ref Excp, ref Msg, ref DeletedRowCount, cur);
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
            }

        }
        public void DeleteTaxInvoices_List(ref int Excp, ref string Msg, ref int DeletedRowCount, TaxInvoices_List invItm_)
        {
            DeletedRowCount = 0;
            try
            {
                if (invItm_ != null)
                {
                    opp.db.TaxInvoices_List.Remove(invItm_);
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


        public string GetPortalInternalId(ref operations opp, ref long TaxInvoiceNo, Invoices_List invL)
        {
            string internalID = "";

            if (Common.FromWhereTableNameGet_TaxInvoiceNo == "TaxInvoices_List")
            {
                TaxInvoices_List taxInvoices_List = opp.db.TaxInvoices_List.Where(t => t.Id == invL.TaxInvoices_ListId).FirstOrDefault();
                if (taxInvoices_List != null)
                {
                    if (taxInvoices_List.TaxInvoiceNo == 0 | taxInvoices_List.TaxInvoiceNo == null)
                    {
                        //TaxInvoices_ListCLS taxInvoices_ListCLS = new TaxInvoices_ListCLS(ref opp);
                        internalID = GetNewinternalID(ref TaxInvoiceNo);
                    }
                    else
                    {
                        internalID = taxInvoices_List.internalID;
                        TaxInvoiceNo = taxInvoices_List.TaxInvoiceNo ?? 0;
                    }
                }
                else
                {
                    internalID = GetNewinternalID(ref TaxInvoiceNo);
                }

            }

            else if (Common.FromWhereTableNameGet_TaxInvoiceNo == "Invoices_List")
            {
                if (invL.TaxInvoiceNo == 0 | invL.TaxInvoiceNo == null)
                {
                    //TaxInvoices_ListCLS taxInvoices_ListCLS = new TaxInvoices_ListCLS(ref opp);
                    internalID = GetNewinternalID(ref TaxInvoiceNo);
                }
                else
                {
                    internalID = invL.TaxInternalId;
                    TaxInvoiceNo = invL.TaxInvoiceNo ?? 0;
                }

            }
            else if (Common.FromWhereTableNameGet_TaxInvoiceNo == "CarsWorkOrders")
            {

                CarsWorkOrder carsWorkOrder = opp.db.CarsWorkOrders.Where(t => t.Id == invL.CarsWorkOrdersId).FirstOrDefault();
                if (carsWorkOrder != null)
                {
                    if (carsWorkOrder.TaxInvoiceNo == 0 | carsWorkOrder.TaxInvoiceNo == null)
                    {
                        //TaxInvoices_ListCLS taxInvoices_ListCLS = new TaxInvoices_ListCLS(ref opp);
                        internalID = GetNewinternalID(ref TaxInvoiceNo);
                    }
                    else
                    {
                        internalID = carsWorkOrder.TaxInternalId;
                        TaxInvoiceNo = carsWorkOrder.TaxInvoiceNo ?? 0;
                    }
                }
                else
                {
                    internalID = GetNewinternalID(ref TaxInvoiceNo);
                }


            }
            else
            {
                TaxInvoiceNo = GetNewTaxInvoiceNoFrom_TaxInvoices_List();
            }


            return internalID;
        }
        public string GetNewinternalID(ref long TaxInvoiceNo)
        {
            TaxInvoiceNo = GetNewTaxInvoiceNo(ref TaxInvoiceNo);


            string internalID = Common.TaxinternalID_Prevx + TaxInvoiceNo.ToString();
            internalID = internalID.Trim();
            return internalID;

        }
        long GetNewTaxInvoiceNoFrom_TaxInvoices_List()
        {
            long TaxInvoiceNo = 1;
            var TaxInNo_ = opp.db.TaxInvoices_List.OrderByDescending(t => t.TaxInvoiceNo).FirstOrDefault();


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
        long GetNewTaxInvoiceNoFrom_Invoices_List()
        {
            long TaxInvoiceNo = 1;
            var TaxInNo_ = opp.db.Invoices_List.OrderByDescending(t => t.TaxInvoiceNo).FirstOrDefault();


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
            if (Common.FromWhereTableNameGet_TaxInvoiceNo == "TaxInvoices_List")
            {
                TaxInvoiceNo = GetNewTaxInvoiceNoFrom_TaxInvoices_List();

            }
            else if (Common.FromWhereTableNameGet_TaxInvoiceNo == "Invoices_List")
            {
                TaxInvoiceNo = GetNewTaxInvoiceNoFrom_Invoices_List();

            }
            else if (Common.FromWhereTableNameGet_TaxInvoiceNo == "CarsWorkOrders")
            {
                TaxInvoiceNo = GetNewTaxInvoiceNoFrom_CarsWorkOrders();

            }
            else
            {
                TaxInvoiceNo = GetNewTaxInvoiceNoFrom_TaxInvoices_List();
            }


            return TaxInvoiceNo;
        }
        public bool CheckIsSetUpInvoiceToPortal(long Invoices_ListId, ref long TaxInvoices_List)
        {
            bool IsSetUpInvoiceToPortal = false;
            TaxInvoices_List = 0;
            TaxInvoices_List txInv = opp.db.TaxInvoices_List.Where(t => t.Invoices_ListId == Invoices_ListId).FirstOrDefault();
            if (txInv == null)
            {
                IsSetUpInvoiceToPortal = false;
            }
            else
            {
                TaxInvoices_List = txInv.Id;
                IsSetUpInvoiceToPortal = true;
            }
            return IsSetUpInvoiceToPortal;
        }

        public void ReSetTaxInvoices_List_From_ERPWithInvoice(ref int Excp, ref string Msg, long Invoices_ListId)
        {




            DeleteSetUpInvoiceToPortal(ref Excp, ref Msg, Invoices_ListId);
            if (Excp == 0)
            {
                try
                {
                    MakeSetTaxInvoices_List_From_ERP(ref Excp, ref Msg, Invoices_ListId);

                }
                catch (Exception ee)
                {
                    Excp = 1;
                    Msg = ee.Message;

                }
            }

        }

        public void ReSetTaxInvoices_List_From_ERP(ref int Excp, ref string Msg, long Invoices_ListId)
        {
            Excp = 0;

            opp.BeginTransaction(2);

            try
            {
                ReSetTaxInvoices_List_From_ERPWithInvoice(ref Excp, ref Msg, Invoices_ListId);
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
        public void ReSetTaxInvoices_List_From_ERP_(ref int Excp, ref string Msg, long Invoices_ListId)
        {
            Excp = 0;
            Msg = "";
            opp.BeginTransaction(2);

            DeleteSetUpInvoiceToPortal(ref Excp, ref Msg, Invoices_ListId);
            if (Excp == 0)
            {
                try
                {
                    MakeSetTaxInvoices_List_From_ERP(ref Excp, ref Msg, Invoices_ListId);


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

        private void DeleteSetUpInvoiceToPortal(ref int Excp, ref string Msg, long Invoices_ListId)
        {
            Excp = 0;

            try
            {
                long taxInvoices_ListId = 0;
                ////////////
                ///
                Invoices_List Inv = opp.db.Invoices_List.Where(t => t.Id == Invoices_ListId).FirstOrDefault();
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

                TaxInvoices_List txInv = opp.db.TaxInvoices_List.Where(t => t.Id == taxInvoices_ListId).FirstOrDefault();
                List<TaxInvoices_ItemsTaxes> txItemsTaxes = opp.db.TaxInvoices_ItemsTax.Where(t => t.TaxInvoices_ListId == taxInvoices_ListId).ToList();
                List<TaxInvoices_LineItems> txLineItems = opp.db.TaxInvoices_LineItems.Where(t => t.TaxInvoices_ListId == taxInvoices_ListId).ToList();
                List<TaxInvoices_TaxesTotal> txTaxesTotal = opp.db.TaxInvoices_TaxesTotal.Where(t => t.TaxInvoices_ListId == taxInvoices_ListId).ToList();

                if (txInv != null)
                {
                    opp.db.TaxInvoices_List.Remove(txInv);
                    opp.db.SaveChanges();
                }
                if (txItemsTaxes != null)
                {
                    opp.db.TaxInvoices_ItemsTax.RemoveRange(txItemsTaxes);
                    opp.db.SaveChanges();
                }
                if (txLineItems != null)
                {
                    opp.db.TaxInvoices_LineItems.RemoveRange(txLineItems);
                    opp.db.SaveChanges();
                }
                if (txTaxesTotal != null)
                {
                    opp.db.TaxInvoices_TaxesTotal.RemoveRange(txTaxesTotal);
                    opp.db.SaveChanges();
                }



            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;

            }
        }
        public void SetTaxInvoices_List_From_ERP(ref int Excp, ref string Msg, long Invoices_ListId)
        {
            Excp = 0;
            Msg = "";
            opp.BeginTransaction(2);

            try
            {
                MakeSetTaxInvoices_List_From_ERP(ref Excp, ref Msg, Invoices_ListId);


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

        void MakeSetTaxInvoices_List_From_ERP(ref int Excp, ref string Msg, long Invoices_ListId)
        {
            Excp = 0;
            Msg = "";

            ////Get  Invoice List
            Invoices_List inv = new Invoices_List();
            TaxInvoices_List txInv = new TaxInvoices_List();
            try
            {
                txInv = SetTaxInvoices_ListFromERP(ref Excp, ref Msg, ref inv, Invoices_ListId);

            }
            catch (Exception ee)
            {
                Msg = ee.Message;
                Excp = 1;
            }

            if (Excp == 0)
            {
                AddNewTaxInvoices_List(ref Excp, ref Msg, ref opp, txInv);
            }
            if (Excp == 0)
            {
                ///Set Invoice Items Details Line
                /// Item detailes
                /// //Items Tax
                /// //Tax Total
                long TaxInvoices_ListId = txInv.Id;

                TaxInvoices_LineItemsCLS taxInvoices_LineItemsCLS = new TaxInvoices_LineItemsCLS(ref opp);

                taxInvoices_LineItemsCLS.SetInvoiceLinesFromERP(ref Excp, ref Msg, inv, Invoices_ListId, TaxInvoices_ListId);
                if (Excp == 0)
                {
                    inv.TaxInvoices_ListId = TaxInvoices_ListId;
                    opp.db.Entry(inv).State = EntityState.Modified;
                    opp.db.SaveChanges();

                }

            }


        }
        public TaxInvoices_List SetTaxInvoices_ListFromERP(ref int Excp, ref string Msg, ref Invoices_List inv, long Invoices_ListId)
        {
            TaxInvoices_List taxInvoices_List = new TaxInvoices_List();

            try
            {

                inv = opp.db.Invoices_List.Where(i => i.Id == Invoices_ListId).FirstOrDefault();
                long carsWorkOrderId = inv.CarsWorkOrdersId ?? 0;

                CarsWorkOrder carsWorkOrder = opp.db.CarsWorkOrders.Where(c => c.Id == carsWorkOrderId).Include(c => c.Car).FirstOrDefault();

                long Customer_Id = inv.Customer_Id.Value;
                //CustomerCLS csCls = new CustomerCLS();
                //Customer cs = csCls.GetCustomer(Customer_Id);

                taxInvoices_List.Invoices_ListId = Invoices_ListId;
                taxInvoices_List.CustomerId = inv.Customer_Id.Value;
                taxInvoices_List.Tax_BranchesId = Common.CommonTaxConfig.Tax_BranchesId;
                //long TaxInvoiceNo = 0;
                //taxInvoices_List.internalID = GetNewinternalID(ref TaxInvoiceNo);
                //taxInvoices_List.TaxInvoiceNo = TaxInvoiceNo;

                taxInvoices_List.internalID = inv.TaxInternalId;//   GetNewinternalID(ref TaxInvoiceNo);
                taxInvoices_List.TaxInvoiceNo = inv.TaxInvoiceNo;// ;
                //Receiver_type = cs.LookUp_PersonCompany_Type,
                //Receiver_id = cs.Tax_No,
                //Receiver_name = cs.Name,



                taxInvoices_List.dateTimeIssued = inv.Invoice_Date.Value;
                //taxpayerActivityCode = Common.CommonTaxConfig.ActivityCode,

                taxInvoices_List.totalDiscountAmount = inv.Net_Total_CommissionValue;
                taxInvoices_List.totalSalesAmount = inv.Total;
                taxInvoices_List.netAmount = inv.Total_PriceWithoutTax;
                taxInvoices_List.taxTotals = inv.TaxSales10_Value;
                taxInvoices_List.totalAmount = inv.Net_Total;
                taxInvoices_List.extraDiscountAmount = 0;
                taxInvoices_List.totalItemsDiscountAmount = inv.Net_Total_CommissionValue;
                if (carsWorkOrder != null)
                {
                    if (carsWorkOrder.Car != null)
                    {
                        taxInvoices_List.purchaseOrderReference = carsWorkOrder.Car.CarNumber.Trim();
                    }
                    else
                    {
                        taxInvoices_List.purchaseOrderReference = "";
                    }
                    taxInvoices_List.salesOrderReference = carsWorkOrder.CustomerOrderNumberPO?? " ";
                    taxInvoices_List.proformaInvoiceNumber = carsWorkOrder.CustomerOrderNumberGR?? " ";
                }
                else
                {
                    taxInvoices_List.purchaseOrderReference = "";
                    taxInvoices_List.salesOrderReference = "";
                    taxInvoices_List.proformaInvoiceNumber = "";
                }
                 
                taxInvoices_List.purchaseOrderDescription = "";
                taxInvoices_List.salesOrderDescription = "";
                




                SetReceiverData(Customer_Id, documentType, ref taxInvoices_List);

                Excp = 0;

            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;

            }
            return taxInvoices_List;

        }

        public void SetReceiverData(long Customer_Id, string documentType, ref TaxInvoices_List taxInvoices_List)
        {
            CustomerCLS csCls = new CustomerCLS();
            Customer cs = csCls.GetCustomer(Customer_Id);
            if (cs != null)
            {
                SetReceiverData(cs, documentType, ref taxInvoices_List);
            }
        }
        public void SetReceiverData(Customer cs, string documentType, ref TaxInvoices_List taxInvoices_List)
        {
            if (cs.General_Customer_Flage == 1)
            {
                taxInvoices_List.Receiver_type = "P";
            }
            else
            {
                taxInvoices_List.Receiver_type = "B";
            }
            //taxInvoices_List.Receiver_type = cs.LookUp_PersonCompany_Type;
            taxInvoices_List.Receiver_id = cs.Tax_No;
            taxInvoices_List.Receiver_name = cs.Name;


            taxInvoices_List.issuer_type = Common.CommonTaxConfig.IssuerType;
            taxInvoices_List.issuer_id = Common.CommonTaxConfig.IssuerId;
            taxInvoices_List.issuer_name = Common.CommonTaxConfig.IssuerName;

            taxInvoices_List.Tax_BranchesId = Common.CommonTaxConfig.Tax_BranchesId;
            taxInvoices_List.documentTypeVersion = Common.CommonTaxConfig.documentTypeVersion;
            taxInvoices_List.taxpayerActivityCode = Common.CommonTaxConfig.ActivityCode;
            taxInvoices_List.documentType = documentType; //InvoiceType

        }

        public bool CheckTaxInvoiceNoFound(long taxInvoiceNo, string internalID)
        {
            bool IsFound = false;
            var TaxInNo = opp.db.TaxInvoices_List.Where(t => t.TaxInvoiceNo == taxInvoiceNo | t.internalID == internalID).FirstOrDefault();
            if (TaxInNo != null)
            {
                IsFound = true;
            }


            return IsFound;
        }
    }
}
