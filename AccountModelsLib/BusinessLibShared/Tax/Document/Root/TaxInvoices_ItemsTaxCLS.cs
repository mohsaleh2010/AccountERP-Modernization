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


    public class TaxInvoices_ItemsTaxCLS
    {
        public operations opp;
        public TaxInvoices_ItemsTaxes invItm = new TaxInvoices_ItemsTaxes();
        public List<TaxInvoices_ItemsTaxes> invItmList = new List<TaxInvoices_ItemsTaxes>();

        public TaxInvoices_ItemsTaxCLS()
        {
            opp = new operations();
            //activeFrom = new DateTime();
            //codeName = "";
        }
        public TaxInvoices_ItemsTaxCLS(ref operations opp_)
        {
            opp = opp_;

        }
        public List<TaxInvoices_ItemsTaxes> GetTaxInvoices_ItemsTax()
        {
            invItmList = opp.db.TaxInvoices_ItemsTax.ToList();
            return invItmList;
        }
        public void GetTaxInvoices_ItemsTax(long Id)
        {
            invItm = opp.db.TaxInvoices_ItemsTax.Where(c => c.Id == Id).FirstOrDefault();
        }

        public List<TaxableItem> getTaxableItems(long taxInvoices_LineItemsId)
        {
            List<TaxableItem> taxableItems = new List<TaxableItem>();
            invItmList = opp.db.TaxInvoices_ItemsTax.Where(c => c.TaxInvoices_LineItemsId == taxInvoices_LineItemsId).ToList();
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


        public void SaveGetTaxInvoices_ItemsTax(ref int Excp, ref string Msg)
        {
            try
            {
                if (invItm.Id == 0)
                {
                    opp.db.TaxInvoices_ItemsTax.Add(invItm);
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

        public void SaveRangeTaxInvoices_ItemsTax(ref int Excp, ref string Msg, List<TaxInvoices_ItemsTaxes> ItemsTaxesLst, long TaxInvoices_LineItemsId)
        {
            string mysql = "Delete From TaxInvoices_ItemsTaxes Where TaxInvoices_LineItemsId=" + TaxInvoices_LineItemsId;
            opp.insert_me(mysql);
            try
            {
                foreach (TaxInvoices_ItemsTaxes item in ItemsTaxesLst)
                {
                    if (item.Id == 0)
                    {
                        opp.db.TaxInvoices_ItemsTax.Add(item);
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

        public void DeleteTaxInvoices_ItemsTax(ref int Excp, ref string Msg, long TaxInvoices_LineItemsId)
        {
            //db.People.RemoveRange(db.People.Where(x => x.State == "CA"));
            try
            {
                opp.db.TaxInvoices_ItemsTax.RemoveRange(opp.db.TaxInvoices_ItemsTax.Where(t => t.TaxInvoices_LineItemsId == TaxInvoices_LineItemsId));
                opp.db.SaveChanges();
                Excp = 0;

            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
            }

        }

        public List<TaxInvoices_ItemsTaxes> GetInvoices_ItemsTaxList(TaxInvoices_LineItems lineItem)
        {
            invItmList = opp.db.TaxInvoices_ItemsTax.Where(l => l.TaxInvoices_LineItemsId == lineItem.Id).ToList();
            return invItmList;
        }
        public List<TaxInvoices_ItemsTaxes> GetInvoices_ItemsTaxListByInvoiceId(long TaxInvoices_ListId)
        {
            invItmList = opp.db.TaxInvoices_ItemsTax.Where(l => l.TaxInvoices_ListId == TaxInvoices_ListId).ToList();
            return invItmList;
        }
        
    }
}
