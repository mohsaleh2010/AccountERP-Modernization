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


    public class TaxdocumentCLS
    {
        string documentType = "i";
        public operations opp;
        public Taxdocument invItm = new Taxdocument();
        public List<Taxdocument> invItmList = new List<Taxdocument>();

        public TaxdocumentCLS()
        {
            opp = new operations();
            //activeFrom = new DateTime();
            //codeName = "";
        }
        public TaxdocumentCLS(ref operations opp_)
        {
            opp = opp_;

        }
        public List<Taxdocument> GetTaxdocument()
        {

            invItmList = opp.db.Taxdocuments.ToList();


            return invItmList;
        }
        public Taxdocument GetTaxdocument(long Id)
        {
            invItm = new Taxdocument();
            invItm = opp.db.Taxdocuments.Where(c => c.Id == Id).FirstOrDefault();
            return invItm;

        }
        public void SaveGetTaxdocument(ref int Excp, ref string Msg)
        {

            try
            {
                if (invItm.Id == 0)
                {

                    opp.db.Taxdocuments.Add(invItm);
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
        public void AddNewTaxdocument(ref int Excp, ref string Msg, ref operations opp, Taxdocument invItm)
        {


            if (invItm.Id == 0)
            {

                try
                {
                    opp.db.Taxdocuments.Add(invItm);
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

        public void SaveGetTaxdocument(ref int Excp, ref string Msg, ref operations opp, Taxdocument invItm)
        {


            if (invItm.Id == 0)
            {

                try
                {
                    opp.db.Taxdocuments.Add(invItm);
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

        public void DeleteTaxdocument(ref int Excp, ref string Msg, ref int DeletedRowCount, long Id)
        {
            try
            {
                Taxdocument cur = opp.db.Taxdocuments.Where(c => c.Id == Id).FirstOrDefault();
                DeleteTaxdocument(ref Excp, ref Msg, ref DeletedRowCount, cur);
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
            }

        }
        public void DeleteTaxdocument(ref int Excp, ref string Msg, ref int DeletedRowCount, Taxdocument invItm_)
        {
            DeletedRowCount = 0;
            try
            {
                if (invItm_ != null)
                {
                    opp.db.Taxdocuments.Remove(invItm_);
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

        public void SaveTaxdocumentLst(ref int Excp, ref string Msg, ref operations opp, List<Taxdocument> txDocLst)
        {
            try
            {
                foreach (var item in txDocLst)
                {
                    opp.db.Taxdocuments.Add(item);
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

        public Taxdocument GetTaxdocumentsByInvoices_ListSuccesded(long invoices_ListId)
        {
            return opp.db.Taxdocuments.Where(i => i.invoices_ListId == invoices_ListId & i.IsSuccessded == 1).FirstOrDefault();
        }
    }
}
