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


    public class TaxReverseInvoices_LineItemsCLS
    {
        public operations opp;
        public TaxReverseInvoices_LineItems invItm = new TaxReverseInvoices_LineItems();
        public List<TaxReverseInvoices_LineItems> invItmList = new List<TaxReverseInvoices_LineItems>();
        string taxType_ATS05 = "T4";
        string subType_ATS05_Items = "W002"; //توريدات
        string subType_ATS05_Service = "W004";//خدمات

        string taxType_TaxSale = "T1";
        string subType_TaxSale = "V009"; // سلع عامة
        public TaxReverseInvoices_LineItemsCLS()
        {
            opp = new operations();
            //activeFrom = new DateTime();
            //codeName = "";
        }
        public TaxReverseInvoices_LineItemsCLS(ref operations opp_)
        {
            opp = opp_;

        }
        public List<TaxReverseInvoices_LineItems> GetTaxReverseInvoices_LineItems()
        {

            invItmList = opp.db.TaxReverseInvoices_LineItems.ToList();


            return invItmList;
        }
        public List<TaxReverseInvoices_LineItems> GetTaxReverseInvoices_LineItems(long TaxReverseInvoices_ListId)
        {

            invItmList = opp.db.TaxReverseInvoices_LineItems.Where(c => c.TaxInvoices_ListId == TaxReverseInvoices_ListId).ToList();
            return invItmList;

        }
        public void SaveGetTaxReverseInvoices_LineItems(ref int Excp, ref string Msg)
        {

            try
            {
                if (invItm.Id == 0)
                {

                    opp.db.TaxReverseInvoices_LineItems.Add(invItm);
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
        public void DeleteTaxReverseInvoices_LineItems(ref int Excp, ref string Msg, long Id)
        {
            try
            {
                TaxReverseInvoices_LineItems cur = opp.db.TaxReverseInvoices_LineItems.Where(c => c.Id == Id).FirstOrDefault();
                if (cur != null)
                {
                    opp.db.TaxReverseInvoices_LineItems.Remove(cur);

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

        public void SetInvoiceLinesFromERP(ref int Excp, ref string Msg, Invoices_List_Reverse inv, long Invoices_List_ReverseId, long TaxReverseInvoices_ListId)
        {
            try
            {

                //ReverseInvoices_List inv = opp.db.ReverseInvoices_List.Where(i => i.Id == ReverseInvoices_ListId).FirstOrDefault();
                Currency cur = GetCurrency(inv.Currency_id.Value);

                ///Get Invoice Details
                List<Invoices_Details_Reverse> ReverseInvoices_Details = opp.db.Invoices_Details_Reverse.Where(d => d.Invoice_Id == Invoices_List_ReverseId).ToList();

                List<TaxReverseInvoices_LineItems> LineItemsList = new List<TaxReverseInvoices_LineItems>();

                //////////////////
                ///

                List<TaxReverseInvoices_ItemsTaxes> TaxItemsList = new List<TaxReverseInvoices_ItemsTaxes>();


                foreach (var item in ReverseInvoices_Details)
                {

                    TaxReverseInvoices_LineItems LineItems = new TaxReverseInvoices_LineItems();

                    //ItemCodeTaxAccepted itmAcc = opp.db.ItemCodeTaxAccepted.Where(i => i.Item_Id == item.Item_Id).FirstOrDefault();
                    Items itm = opp.db.Item.Where(i => i.id == item.Item_Id).FirstOrDefault();
                    LineItems.TaxInvoices_ListId = TaxReverseInvoices_ListId;
                    //LineItems.ItemCodeTaxAcceptedId = itmAcc.Id;

                    LineItems.Item_Id = item.Item_Id;
                    LineItems.Unit_Item_Id = item.Unit_Item_Table_Id;


                    LineItems.CurrencyId = cur.Id;

                    LineItems.description = itm.ItemName_OnInvoice;// itmAcc.CodeNameAr; //CodeNameAr

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
                    opp.db.TaxReverseInvoices_LineItems.Add(LineItems);
                    opp.db.SaveChanges();

                    //invoiceItm.taxableItems = GetTaxableItem(item.Id);
                    //TaxSale 10
                    TaxReverseInvoices_ItemsTaxes taxReverseInvoices_ItemsTax_TaxSale = new TaxReverseInvoices_ItemsTaxes
                    {
                        TaxInvoices_ListId = TaxReverseInvoices_ListId,
                        TaxReverseInvoices_LineItemsId = LineItems.Id,
                        taxType = taxType_TaxSale,
                        amount = item.Item_TaxSales10_Value ?? 0,
                        subType = subType_TaxSale,
                        rate = item.Item_TaxSales10,
                    };
                    if (taxReverseInvoices_ItemsTax_TaxSale.amount > 0)
                    {
                        opp.db.TaxReverseInvoices_ItemsTax.Add(taxReverseInvoices_ItemsTax_TaxSale);
                        opp.db.SaveChanges();
                    }

                    //ATS 05
                    TaxReverseInvoices_ItemsTaxes taxReverseInvoices_ItemsTax_ATS05 = new TaxReverseInvoices_ItemsTaxes
                    {
                        TaxInvoices_ListId = TaxReverseInvoices_ListId,
                        TaxReverseInvoices_LineItemsId = LineItems.Id,
                        taxType = taxType_ATS05,
                        amount = item.Item_ATS05_Value ?? 0,
                        
                        rate = item.Item_ATS05,
                    };
                    if (taxReverseInvoices_ItemsTax_ATS05.amount > 0)
                    {
                        if (itm.Is_ItemOrService == (int)ItemOrServiceEnum.Item)
                        {
                            taxReverseInvoices_ItemsTax_ATS05.subType = subType_ATS05_Items;
                        }
                        if (itm.Is_ItemOrService == (int)ItemOrServiceEnum.Service)
                        {
                            taxReverseInvoices_ItemsTax_ATS05.subType = subType_ATS05_Service;
                        }

                        opp.db.TaxReverseInvoices_ItemsTax.Add(taxReverseInvoices_ItemsTax_ATS05);
                        opp.db.SaveChanges();
                    }
                }

                //opp.db.TaxReverseInvoices_LineItems.AddRange(LineItemsList);
                //opp.db.SaveChanges();

                ////////////Get Tax Total for invoice


                var taxList = opp.db.TaxReverseInvoices_ItemsTax
                                .Where(i => i.TaxInvoices_ListId == TaxReverseInvoices_ListId)
                                .GroupBy(x => x.taxType)
                                .Select(g => new
                                {
                                    taxType = g.Key,
                                    amount = g.Sum(x => x.amount)
                                });
                List<TaxReverseInvoices_TaxesTotal> invItmList = new List<TaxReverseInvoices_TaxesTotal>();
                foreach (var item in taxList)
                {
                    TaxReverseInvoices_TaxesTotal invItm = new TaxReverseInvoices_TaxesTotal();
                    invItm.TaxInvoices_ListId = TaxReverseInvoices_ListId;
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
                //TaxReverseInvoices_TaxesTotal invItmFromInvLst_TaxSale = new TaxReverseInvoices_TaxesTotal();
                //invItmFromInvLst_TaxSale.TaxReverseInvoices_ListId = TaxReverseInvoices_ListId;
                //invItmFromInvLst_TaxSale.taxType = taxType_TaxSale;
                //invItmFromInvLst_TaxSale.amount = inv.TaxSales10_Value ?? 0;// item.amount.Value;
                //invItmList.Add(invItmFromInvLst_TaxSale);
                ////taxType_ATS05
                //TaxReverseInvoices_TaxesTotal invItmFromInvLst_ATS05 = new TaxReverseInvoices_TaxesTotal();
                //invItmFromInvLst_ATS05.TaxReverseInvoices_ListId = TaxReverseInvoices_ListId;
                //invItmFromInvLst_ATS05.taxType = taxType_ATS05;
                //invItmFromInvLst_ATS05.amount = inv.ATS05_Value ?? 0;// item.amount.Value;
                //invItmList.Add(invItmFromInvLst_ATS05);



                opp.db.TaxReverseInvoices_TaxesTotal.AddRange(invItmList);
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
        public void AddLineItem(ref int Excp, ref string Msg, ref operations opp, TaxReverseInvoices_LineItems lineItem)
        {


            if (invItm.Id == 0)
            {
                try
                {
                    opp.db.TaxReverseInvoices_LineItems.Add(lineItem);
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
