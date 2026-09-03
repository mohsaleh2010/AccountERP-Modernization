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


    public class TaxReverseInvoices_ItemsTaxCLS
    {
        public operations opp;
        public TaxReverseInvoices_ItemsTaxes invItm = new TaxReverseInvoices_ItemsTaxes();
        public List<TaxReverseInvoices_ItemsTaxes> invItmList = new List<TaxReverseInvoices_ItemsTaxes>();

        public TaxReverseInvoices_ItemsTaxCLS()
        {
            opp = new operations();
            //activeFrom = new DateTime();
            //codeName = "";
        }
        public TaxReverseInvoices_ItemsTaxCLS(ref operations opp_)
        {
            opp = opp_;

        }
        public List<TaxReverseInvoices_ItemsTaxes> GetTaxReverseInvoices_ItemsTax()
        {
            invItmList = opp.db.TaxReverseInvoices_ItemsTax.ToList();
            return invItmList;
        }
        public void GetTaxReverseInvoices_ItemsTax(long Id)
        {
            invItm = opp.db.TaxReverseInvoices_ItemsTax.Where(c => c.Id == Id).FirstOrDefault();
        }

        public List<TaxableItem> getTaxableItems(long taxReverseInvoices_LineItemsId)
        {
            List<TaxableItem> taxableItems = new List<TaxableItem>();
            invItmList = opp.db.TaxReverseInvoices_ItemsTax.Where(c => c.TaxReverseInvoices_LineItemsId == taxReverseInvoices_LineItemsId).ToList();
            foreach (var item in invItmList)
            {
                TaxableItem taxableItm = new TaxableItem();
                taxableItm.amount = item.amount;
                taxableItm.rate = item.rate;
                taxableItm.taxType = item.taxType;
                taxableItm.subType = item.subType;
                taxableItems.Add(taxableItm);
            }
            return taxableItems;
        }


        public void SaveGetTaxReverseInvoices_ItemsTax(ref int Excp, ref string Msg)
        {
            try
            {
                if (invItm.Id == 0)
                {
                    opp.db.TaxReverseInvoices_ItemsTax.Add(invItm);
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

        public void SaveRangeTaxReverseInvoices_ItemsTax(ref int Excp, ref string Msg, List<TaxReverseInvoices_ItemsTaxes> ItemsTaxesLst, long TaxReverseInvoices_LineItemsId)
        {
            string mysql = "Delete From TaxReverseInvoices_ItemsTaxes Where TaxReverseInvoices_LineItemsId=" + TaxReverseInvoices_LineItemsId;
            opp.insert_me(mysql);
            try
            {
                foreach (TaxReverseInvoices_ItemsTaxes item in ItemsTaxesLst)
                {
                    if (item.Id == 0)
                    {
                        opp.db.TaxReverseInvoices_ItemsTax.Add(item);
                    }
                    else
                    {
                        opp.db.Entry(item).State = EntityState.Modified;
                    }
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

        public void DeleteTaxReverseInvoices_ItemsTax(ref int Excp, ref string Msg, long TaxReverseInvoices_LineItemsId)
        {
            //db.People.RemoveRange(db.People.Where(x => x.State == "CA"));
            try
            {
                opp.db.TaxReverseInvoices_ItemsTax.RemoveRange(opp.db.TaxReverseInvoices_ItemsTax.Where(t => t.TaxReverseInvoices_LineItemsId == TaxReverseInvoices_LineItemsId));
                opp.db.SaveChanges();
                Excp = 0;

            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
            }

        }

        public List<TaxReverseInvoices_ItemsTaxes> GetReverseInvoices_ItemsTaxList(TaxReverseInvoices_LineItems lineItem)
        {
            invItmList = opp.db.TaxReverseInvoices_ItemsTax.Where(l => l.TaxReverseInvoices_LineItemsId == lineItem.Id).ToList();
            return invItmList;
        }
        public List<TaxReverseInvoices_ItemsTaxes> GetReverseInvoices_ItemsTaxListByInvoiceId(long TaxReverseInvoices_ListId)
        {
            invItmList = opp.db.TaxReverseInvoices_ItemsTax.Where(l => l.TaxInvoices_ListId == TaxReverseInvoices_ListId).ToList();
            return invItmList;
        }
        
    }
}
