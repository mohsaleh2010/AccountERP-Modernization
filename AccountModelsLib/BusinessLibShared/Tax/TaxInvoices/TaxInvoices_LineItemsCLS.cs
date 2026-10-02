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


    public class TaxInvoices_LineItemsCLS
    {
        public operations opp;
        public TaxInvoices_LineItems invItm = new TaxInvoices_LineItems();
        public List<TaxInvoices_LineItems> invItmList = new List<TaxInvoices_LineItems>();
        string taxType_ATS05 = "T4";
        string subType_ATS05_Items = "W002"; //توريدات
        string subType_ATS05_Service = "W004";//خدمات

        string taxType_TaxSale = "T1";
        string subType_TaxSale = "V009"; // سلع عامة
        public TaxInvoices_LineItemsCLS()
        {
            opp = new operations();
            //activeFrom = new DateTime();
            //codeName = "";
        }
        public TaxInvoices_LineItemsCLS(ref operations opp_)
        {
            opp = opp_;

        }
        public List<TaxInvoices_LineItems> GetTaxInvoices_LineItems()
        {

            invItmList = opp.db.TaxInvoices_LineItems.ToList();


            return invItmList;
        }
        public List<TaxInvoices_LineItems> GetTaxInvoices_LineItems(long TaxInvoices_ListId)
        {

            invItmList = opp.db.TaxInvoices_LineItems.Where(c => c.TaxInvoices_ListId == TaxInvoices_ListId).ToList();
            return invItmList;

        }
        public void SaveGetTaxInvoices_LineItems(ref int Excp, ref string Msg)
        {

            try
            {
                if (invItm.Id == 0)
                {

                    opp.db.TaxInvoices_LineItems.Add(invItm);
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
        public void DeleteTaxInvoices_LineItems(ref int Excp, ref string Msg, long Id)
        {
            try
            {
                TaxInvoices_LineItems cur = opp.db.TaxInvoices_LineItems.Where(c => c.Id == Id).FirstOrDefault();
                if (cur != null)
                {
                    opp.db.TaxInvoices_LineItems.Remove(cur);

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

        public void SetInvoiceLinesFromERP(ref int Excp, ref string Msg, Invoices_List inv, long Invoices_ListId, long TaxInvoices_ListId)
        {
            try
            {

                //Invoices_List inv = opp.db.Invoices_List.Where(i => i.Id == Invoices_ListId).FirstOrDefault();
                Currency cur = GetCurrency(inv.Currency_id.Value);

                ///Get Invoice Details
                //List<Invoices_Details> invoices_Details = opp.db.Invoices_Details.Where(d => d.Invoice_Id == Invoices_ListId).ToList();
                //List<Invoices_Details> invoices_Details = opp.db.Invoices_Details.AsNoTracking().Where(d => d.Invoice_Id == Invoices_ListId).Include(i=>i.Items).Where(i=>i.Items.Item_Category_ID !=(long)ItemCategoryEnum.RawMaterials).ToList();
                IQueryable<Invoices_Details> q = opp.db.Invoices_Details.AsNoTracking().Include(i => i.Items).Where(d => d.Invoice_Id == Invoices_ListId).Where(i => i.Items.Item_Category_ID != (long)ItemCategoryEnum.RawMaterials) ;

                List<Invoices_Details> invoices_Details = q.ToList();
               List <TaxInvoices_LineItems> LineItemsList = new List<TaxInvoices_LineItems>();

                //////////////////
                ///

                List<TaxInvoices_ItemsTaxes> TaxItemsList = new List<TaxInvoices_ItemsTaxes>();


                foreach (var item in invoices_Details)
                {
                    //item.Item_Id=0
                    TaxInvoices_LineItems LineItems = new TaxInvoices_LineItems();

                    //ItemCodeTaxAccepted itmAcc = opp.db.ItemCodeTaxAccepted.Where(i => i.Item_Id == item.Item_Id).FirstOrDefault();
                    Items itm = opp.db.Item.Where(i => i.Id == item.Item_Id).FirstOrDefault();
                    LineItems.TaxInvoices_ListId = TaxInvoices_ListId;
                    //LineItems.ItemCodeTaxAcceptedId = itmAcc.Id;

                    LineItems.Item_Id = item.Item_Id;
                    LineItems.Unit_Item_Id = item.Unit_Item_Table_Id;


                    LineItems.CurrencyId = cur.Id;

                    LineItems.description =     itm.ItemName_OnInvoice+ " - "+ item.Specification;// itmAcc.CodeNameAr; //CodeNameAr
                    LineItems.description = LineItems.description.Trim();
                    LineItems.itemType = itm.ItemCodeType;// itmAcc.ItemCodeType;//ItemCodeType

                    LineItems.itemCode = itm.ItemCode;// itmAcc.ItemCode;//ItemCode
                    LineItems.internalCode = itm.code;// itmAcc.internalCode;  //internalCode

                    LineItems.unitType = itm.UnitCodeTax;// itmAcc.UnitCode;//UnitCode


                    LineItems.quantity = item.Qnty;   //invoice_Details.Qnty

                    LineItems.salesTotal = item.Qnty_value;  //  invoice_Details.Qnty_value

                    //LineItems.total = item.Item_Total_WithoutTax; //  invoice_Details.Item_Total_WithoutTax
                    LineItems.total = item.Item_Net_Total_Price; //  invoice_Details.Item_Total_WithoutTax

                    LineItems.valueDifference = 0; ///==0

                    LineItems.totalTaxableFees = 0;  /// ===0

                    LineItems.netTotal = item.Item_Total_WithoutTax;//  invoice_Details.Item_Net_Total_Price

                    LineItems.itemsDiscount = item.Net_Total_Item_Commission;   //  invoice_Details.Net_Total_Item_Commission

                    //invoiceLine.unitValue = new UnitValue
                    LineItems.amountEGP = item.Larg_Unit_Price;   //  invoice_Details.Larg_Unit_Price
                    LineItems.currencySold = cur.code;// "EGP";//item.unitType,//UnitCode
                    LineItems.currencyExchangeRate = 0;
                    LineItems.amountSold = item.Larg_Unit_Price;   //  invoice_Details.Larg_Unit_Price

                    //invoiceLine.discount = new Discount
                    LineItems.discount_rate = item.Unit_CommissionRate;
                    LineItems.discount_amount = item.Net_Total_Item_Commission;


                    ///
                    opp.db.TaxInvoices_LineItems.Add(LineItems);
                    opp.db.SaveChanges();

                    //invoiceItm.taxableItems = GetTaxableItem(item.Id);
                    //TaxSale 10
                    TaxInvoices_ItemsTaxes taxInvoices_ItemsTax_TaxSale = new TaxInvoices_ItemsTaxes
                    {
                        TaxInvoices_ListId = TaxInvoices_ListId,
                        TaxInvoices_LineItemsId = LineItems.Id,
                        taxType = taxType_TaxSale,
                        amount = item.Item_TaxSales10_Value ?? 0,
                        subType = subType_TaxSale,
                        rate = item.Item_TaxSales10,
                    };
                    if (taxInvoices_ItemsTax_TaxSale.amount > 0)
                    {
                        opp.db.TaxInvoices_ItemsTax.Add(taxInvoices_ItemsTax_TaxSale);
                        opp.db.SaveChanges();
                    }

                    //ATS 05
                    TaxInvoices_ItemsTaxes taxInvoices_ItemsTax_ATS05 = new TaxInvoices_ItemsTaxes
                    {
                        TaxInvoices_ListId = TaxInvoices_ListId,
                        TaxInvoices_LineItemsId = LineItems.Id,
                        taxType = taxType_ATS05,
                        amount = item.Item_ATS05_Value ?? 0,
                        
                        rate = item.Item_ATS05,
                    };
                    if (taxInvoices_ItemsTax_ATS05.amount > 0)
                    {
                        if (itm.Is_ItemOrService == (int)ItemOrServiceEnum.Item)
                        {
                            taxInvoices_ItemsTax_ATS05.subType = subType_ATS05_Items;
                        }
                        if (itm.Is_ItemOrService == (int)ItemOrServiceEnum.Service)
                        {
                            taxInvoices_ItemsTax_ATS05.subType = subType_ATS05_Service;
                        }

                        opp.db.TaxInvoices_ItemsTax.Add(taxInvoices_ItemsTax_ATS05);
                        opp.db.SaveChanges();
                    }
                }

                //opp.db.TaxInvoices_LineItems.AddRange(LineItemsList);
                //opp.db.SaveChanges();

                ////////////Get Tax Total for invoice


                var taxList = opp.db.TaxInvoices_ItemsTax
                                .Where(i => i.TaxInvoices_ListId == TaxInvoices_ListId)
                                .GroupBy(x => x.taxType)
                                .Select(g => new
                                {
                                    taxType = g.Key,
                                    amount = g.Sum(x => x.amount)
                                });
                List<TaxInvoices_TaxesTotal> invItmList = new List<TaxInvoices_TaxesTotal>();
                foreach (var item in taxList)
                {
                    TaxInvoices_TaxesTotal invItm = new TaxInvoices_TaxesTotal();
                    invItm.TaxInvoices_ListId = TaxInvoices_ListId;
                    invItm.taxType = item.taxType;
                    if (item.amount.Value == 0)
                    {
                        continue;
                    }
                    invItm.amount = item.amount.Value;
                    invItmList.Add(invItm);
                }
                ////Get Tax From Invoice List
                ///
                ////_TaxSale10
                //TaxInvoices_TaxesTotal invItmFromInvLst_TaxSale = new TaxInvoices_TaxesTotal();
                //invItmFromInvLst_TaxSale.TaxInvoices_ListId = TaxInvoices_ListId;
                //invItmFromInvLst_TaxSale.taxType = taxType_TaxSale;
                //invItmFromInvLst_TaxSale.amount = inv.TaxSales10_Value ?? 0;// item.amount.Value;
                //invItmList.Add(invItmFromInvLst_TaxSale);
                ////taxType_ATS05
                //TaxInvoices_TaxesTotal invItmFromInvLst_ATS05 = new TaxInvoices_TaxesTotal();
                //invItmFromInvLst_ATS05.TaxInvoices_ListId = TaxInvoices_ListId;
                //invItmFromInvLst_ATS05.taxType = taxType_ATS05;
                //invItmFromInvLst_ATS05.amount = inv.ATS05_Value ?? 0;// item.amount.Value;
                //invItmList.Add(invItmFromInvLst_ATS05);



                opp.db.TaxInvoices_TaxesTotal.AddRange(invItmList);
                opp.db.SaveChanges();



                Excp = 0;
            }
            catch (Exception ee)
            {

                Excp = 1;
                Msg = ee.Message;
            }
        }

        Currency GetCurrency(long CurrencyId)
        {
            CurrencyCLS cu = new CurrencyCLS(ref opp);
            Currency cur = cu.GetCurrency(CurrencyId);
            return cur;
        }
        public void AddLineItem(ref int Excp, ref string Msg, ref operations opp, TaxInvoices_LineItems lineItem)
        {


            if (invItm.Id == 0)
            {
                try
                {
                    opp.db.TaxInvoices_LineItems.Add(lineItem);
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
                    opp.db.Entry(lineItem).State = EntityState.Modified;
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


    }
}
