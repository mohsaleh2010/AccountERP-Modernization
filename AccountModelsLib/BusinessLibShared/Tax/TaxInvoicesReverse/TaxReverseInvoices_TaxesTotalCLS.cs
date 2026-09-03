using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{


    public class TaxReverseInvoices_TaxesTotalCLS
    {
        public operations opp;
        public TaxReverseInvoices_TaxesTotal invItm = new TaxReverseInvoices_TaxesTotal();
        public List<TaxReverseInvoices_TaxesTotal> invItmList = new List<TaxReverseInvoices_TaxesTotal>();

        public TaxReverseInvoices_TaxesTotalCLS()
        {
            opp = new operations();
            //activeFrom = new DateTime();
            //codeName = "";
        }
        public TaxReverseInvoices_TaxesTotalCLS(ref operations opp_)
        {
            opp = opp_;

        }
        public List<TaxReverseInvoices_TaxesTotal> GetTaxReverseInvoices_TaxesTotal()
        {

            invItmList = opp.db.TaxReverseInvoices_TaxesTotal.ToList();


            return invItmList;
        }
        public void GetTaxReverseInvoices_TaxesTotal(long Id)
        {

            invItm = opp.db.TaxReverseInvoices_TaxesTotal.Where(c => c.Id == Id).FirstOrDefault();

        }
        public List<TaxReverseInvoices_TaxesTotal> GetTaxReverseInvoices_TaxesTotalByInvoiceId(long TaxReverseInvoices_ListId)
        {
            invItmList = opp.db.TaxReverseInvoices_TaxesTotal.Where(t => t.TaxInvoices_ListId == TaxReverseInvoices_ListId).ToList();
            return invItmList;
        }
        public void SaveGetTaxReverseInvoices_TaxesTotal(ref int Excp, ref string Msg)
        {

            try
            {
                if (invItm.Id == 0)
                {

                    opp.db.TaxReverseInvoices_TaxesTotal.Add(invItm);
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
        public void DeleteTaxReverseInvoices_TaxesTotal(ref int Excp, ref string Msg, long Id)
        {
            try
            {
                TaxReverseInvoices_TaxesTotal cur = opp.db.TaxReverseInvoices_TaxesTotal.Where(c => c.Id == Id).FirstOrDefault();
                if (cur != null)
                {
                    opp.db.TaxReverseInvoices_TaxesTotal.Remove(invItm);

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

        public  List<TaxTotal> GetTaxTotals_Reverse(long taxReverseInvoices_ListId)
        {
            List<TaxTotal> taxTotalList = new List<TaxTotal>();
            var TaxReverseInvoices_TaxesTotal_List = opp.db.TaxReverseInvoices_TaxesTotal.Where(i => i.TaxInvoices_ListId == taxReverseInvoices_ListId);
            foreach (var item in TaxReverseInvoices_TaxesTotal_List)
            {
                TaxTotal taxTotal = new TaxTotal();
                taxTotal.taxType=item.taxType;
                taxTotal.amount=item.amount;
                taxTotalList.Add(taxTotal);
            }

            return taxTotalList;
        }

        public  void SaveAddTaxReverseInvoices_TaxesTotal(ref int Excp, ref string Msg,long TaxReverseInvoices_ListId, List<TaxReverseInvoices_TaxesTotal> taxtotals)
        {
            string mysql = "Delete From TaxReverseInvoices_TaxesTotal Where TaxReverseInvoices_ListId=" + TaxReverseInvoices_ListId;
            opp.insert_me(mysql);


            foreach (var tax in taxtotals)
            {
                try
                {
                    if (tax.Id == 0)
                    {

                        opp.db.TaxReverseInvoices_TaxesTotal.Add(tax);
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
