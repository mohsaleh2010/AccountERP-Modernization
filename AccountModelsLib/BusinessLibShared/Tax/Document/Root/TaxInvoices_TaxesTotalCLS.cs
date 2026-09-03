using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models.Tax
{


    public class TaxInvoices_TaxesTotalCLS
    {
        public operations opp;
        public TaxInvoices_TaxesTotal invItm = new TaxInvoices_TaxesTotal();
        public List<TaxInvoices_TaxesTotal> invItmList = new List<TaxInvoices_TaxesTotal>();

        public TaxInvoices_TaxesTotalCLS()
        {
            opp = new operations();
            //activeFrom = new DateTime();
            //codeName = "";
        }
        public TaxInvoices_TaxesTotalCLS(ref operations opp_)
        {
            opp = opp_;

        }
        public List<TaxInvoices_TaxesTotal> GetTaxInvoices_TaxesTotal()
        {

            invItmList = opp.db.TaxInvoices_TaxesTotal.ToList();


            return invItmList;
        }
        public void GetTaxInvoices_TaxesTotal(long Id)
        {

            invItm = opp.db.TaxInvoices_TaxesTotal.Where(c => c.Id == Id).FirstOrDefault();

        }
        public List<TaxInvoices_TaxesTotal> GetTaxInvoices_TaxesTotalByInvoiceId(long TaxInvoices_ListId)
        {
            invItmList = opp.db.TaxInvoices_TaxesTotal.Where(t => t.TaxInvoices_ListId == TaxInvoices_ListId).ToList();
            return invItmList;
        }
        public void SaveGetTaxInvoices_TaxesTotal(ref int Excp, ref string Msg)
        {

            try
            {
                if (invItm.Id == 0)
                {

                    opp.db.TaxInvoices_TaxesTotal.Add(invItm);
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
        public void DeleteTaxInvoices_TaxesTotal(ref int Excp, ref string Msg, long Id)
        {
            try
            {
                TaxInvoices_TaxesTotal cur = opp.db.TaxInvoices_TaxesTotal.Where(c => c.Id == Id).FirstOrDefault();
                if (cur != null)
                {
                    opp.db.TaxInvoices_TaxesTotal.Remove(invItm);

                    opp.db.SaveChanges();
                    Excp = 0;
                }
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
            }

        }

        public  List<Tax.TaxTotal> GetTaxTotals(long taxInvoices_ListId)
        {
            List<TaxTotal> taxTotalList = new List<TaxTotal>();
            var TaxInvoices_TaxesTotal_List = opp.db.TaxInvoices_TaxesTotal.Where(i => i.TaxInvoices_ListId == taxInvoices_ListId);
            foreach (var item in TaxInvoices_TaxesTotal_List)
            {
                TaxTotal taxTotal = new TaxTotal();
                taxTotal.taxType=item.taxType;
                taxTotal.amount=item.amount;
                taxTotalList.Add(taxTotal);
            }

            return taxTotalList;
        }

        public  void SaveAddTaxInvoices_TaxesTotal(ref int Excp, ref string Msg,long TaxInvoices_ListId, List<TaxInvoices_TaxesTotal> taxtotals)
        {
            string mysql = "Delete From TaxInvoices_TaxesTotal Where TaxInvoices_ListId=" + TaxInvoices_ListId;
            opp.insert_me(mysql);


            foreach (var tax in taxtotals)
            {
                try
                {
                    if (tax.Id == 0)
                    {

                        opp.db.TaxInvoices_TaxesTotal.Add(tax);
                    }
                    else
                    {
                        opp.db.Entry(tax).State = EntityState.Modified;
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
        }
    }
}
