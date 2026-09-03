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


    public class TaxdocumentReverseCLS
    {
        string documentReverseType = "i";
        public operations opp;
        public TaxdocumentReverse invItm = new TaxdocumentReverse();
        public List<TaxdocumentReverse> invItmList = new List<TaxdocumentReverse>();

        public TaxdocumentReverseCLS()
        {
            opp = new operations();
            //activeFrom = new DateTime();
            //codeName = "";
        }
        public TaxdocumentReverseCLS(ref operations opp_)
        {
            opp = opp_;

        }
        public List<TaxdocumentReverse> GetTaxdocumentReverse()
        {

            invItmList = opp.db.TaxdocumentsReverse.ToList();


            return invItmList;
        }
        public TaxdocumentReverse GetTaxdocumentReverse(long Id)
        {
            invItm = new TaxdocumentReverse();
            invItm = opp.db.TaxdocumentsReverse.Where(c => c.Id == Id).FirstOrDefault();
            return invItm;

        }
        public void SaveGetTaxdocumentReverse(ref int Excp, ref string Msg)
        {

            try
            {
                if (invItm.Id == 0)
                {

                    opp.db.TaxdocumentsReverse.Add(invItm);
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
        public void AddNewTaxdocumentReverse(ref int Excp, ref string Msg, ref operations opp, TaxdocumentReverse invItm)
        {


            if (invItm.Id == 0)
            {

                try
                {
                    opp.db.TaxdocumentsReverse.Add(invItm);
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

        public void SaveGetTaxdocumentReverse(ref int Excp, ref string Msg, ref operations opp, TaxdocumentReverse invItm)
        {


            if (invItm.Id == 0)
            {

                try
                {
                    opp.db.TaxdocumentsReverse.Add(invItm);
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

        public void DeleteTaxdocumentReverse(ref int Excp, ref string Msg, ref int DeletedRowCount, long Id)
        {
            try
            {
                TaxdocumentReverse cur = opp.db.TaxdocumentsReverse.Where(c => c.Id == Id).FirstOrDefault();
                DeleteTaxdocumentReverse(ref Excp, ref Msg, ref DeletedRowCount, cur);
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
            }

        }
        public void DeleteTaxdocumentReverse(ref int Excp, ref string Msg, ref int DeletedRowCount, TaxdocumentReverse invItm_)
        {
            DeletedRowCount = 0;
            try
            {
                if (invItm_ != null)
                {
                    opp.db.TaxdocumentsReverse.Remove(invItm_);
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

        public void SaveTaxdocumentReverseLst(ref int Excp, ref string Msg, ref operations opp, List<TaxdocumentReverse> txDocLst)
        {
            try
            {
                foreach (var item in txDocLst)
                {
                    opp.db.TaxdocumentsReverse.Add(item);
                    opp.db.SaveChanges();


                    Invoices_List inv = opp.db.Invoices_List.Where(i => i.Id == item.invoices_ListId).FirstOrDefault();
                    if (inv != null)
                    {
                        inv.TaxInternalId = item.internalId;
                        inv.IsUploadToPortal = 1;

                        opp.db.Entry(inv).State = EntityState.Modified;
                        opp.db.SaveChanges();

                        long carsWorkOrderId = inv.CarsWorkOrdersId ?? 0;

                        CarsWorkOrder carsWorkOrder = opp.db.CarsWorkOrders.Where(o => o.Id == carsWorkOrderId).FirstOrDefault();
                        if (carsWorkOrder != null)
                        {
                            carsWorkOrder.TaxInternalId = item.internalId;
                            carsWorkOrder.IsUploadToPortal = 1;
                            
                            opp.db.Entry(carsWorkOrder).State = EntityState.Modified;
                            opp.db.SaveChanges();
                        }
                    }



                }
                Excp = 0;
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
            }
        }

        public TaxdocumentReverse GetTaxdocumentReverseByInvoices_ListSuccesded(long invoices_ListId)
        {
            return opp.db.TaxdocumentsReverse.Where(i => i.invoices_ListId == invoices_ListId & i.IsSuccessded == 1).FirstOrDefault();
        }
    }
}
