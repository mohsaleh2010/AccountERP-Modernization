  
using Newtonsoft.Json;
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


    public class InvoiceLineCls
    {
        operations opp;
        public InvoiceLineCls()
        {
            opp = new operations();
        }
        public InvoiceLineCls(operations opp_)
        {
            opp = opp_;
        }

        public List<InvoiceLine> GetInvoiceLines(long TaxInvoices_ListId)
        {
            List<InvoiceLine> invoiceLines = new List<InvoiceLine>();
            //TaxInvoices_LineItems InvoiceLine = new TaxInvoices_LineItems();

            TaxInvoices_LineItemsCLS itmlinesCLS = new TaxInvoices_LineItemsCLS();

            List<TaxInvoices_LineItems> taxInvLines = itmlinesCLS.GetTaxInvoices_LineItems(TaxInvoices_ListId);
            foreach (var item in taxInvLines)
            {
                InvoiceLine invoiceLine = new InvoiceLine();

                //ItemCodeTaxAccepted itmAcc = new ItemCodeTaxAccepted();

                invoiceLine.description = item.description; //CodeNameAr

                invoiceLine.itemType = item.itemType;//ItemCodeType

                invoiceLine.itemCode = item.itemCode;//ItemCode
                invoiceLine.unitType = item.unitType;//UnitCode

                invoiceLine.quantity =  item.quantity;   //invoice_Details.Qnty

                invoiceLine.internalCode = item.internalCode;  //ParentCode

                invoiceLine.salesTotal = item.salesTotal;  //  invoice_Details.Qnty_value

                invoiceLine.total = item.total; //  invoice_Details.Item_Total_WithoutTax

                invoiceLine.valueDifference = item.valueDifference; ///==0

                invoiceLine.totalTaxableFees = item.totalTaxableFees;  /// ===0

                invoiceLine.netTotal = item.netTotal;//  invoice_Details.Item_Net_Total_Price

                invoiceLine.itemsDiscount = item.itemsDiscount;   //  invoice_Details.Net_Total_Item_Commission

                invoiceLine.unitValue = new UnitValue
                {
                    amountEGP = item.amountEGP,   //  invoice_Details.Larg_Unit_Price
                    currencySold = "EGP",//item.unitType,//UnitCode
                    currencyExchangeRate = 0,
                    amountSold = 0
                };
                
                invoiceLine.discount = new Discount
                {
                    rate = 0,
                    amount = 0
                };


                invoiceLine.taxableItems = GetTaxableItem(item.Id);

                invoiceLines.Add(invoiceLine);
            }
            return invoiceLines;
        }

        List<TaxableItem> GetTaxableItem(long TaxInvoices_LineItemsId)
        {

            TaxInvoices_ItemsTaxCLS taxCls = new TaxInvoices_ItemsTaxCLS();
            List<TaxableItem> taxableItems = taxCls.getTaxableItems(TaxInvoices_LineItemsId);
            return taxableItems;
        }


        public List<InvoiceLine> GetInvoiceLines_Reverse(long TaxInvoices_ListId)
        {
            List<InvoiceLine> invoiceLines = new List<InvoiceLine>();
            //TaxInvoices_LineItems InvoiceLine = new TaxInvoices_LineItems();

            TaxReverseInvoices_LineItemsCLS itmlinesCLS = new  TaxReverseInvoices_LineItemsCLS ();

            List<TaxReverseInvoices_LineItems> taxInvLines = itmlinesCLS.GetTaxReverseInvoices_LineItems(TaxInvoices_ListId);
            foreach (var item in taxInvLines)
            {
                InvoiceLine invoiceLine = new InvoiceLine();

                //ItemCodeTaxAccepted itmAcc = new ItemCodeTaxAccepted();

                invoiceLine.description = item.description; //CodeNameAr

                invoiceLine.itemType = item.itemType;//ItemCodeType

                invoiceLine.itemCode = item.itemCode;//ItemCode
                invoiceLine.unitType = item.unitType;//UnitCode

                invoiceLine.quantity = item.quantity;   //invoice_Details.Qnty

                invoiceLine.internalCode = item.internalCode;  //ParentCode

                invoiceLine.salesTotal = item.salesTotal;  //  invoice_Details.Qnty_value

                invoiceLine.total = item.total; //  invoice_Details.Item_Total_WithoutTax

                invoiceLine.valueDifference = item.valueDifference; ///==0

                invoiceLine.totalTaxableFees = item.totalTaxableFees;  /// ===0

                invoiceLine.netTotal = item.netTotal;//  invoice_Details.Item_Net_Total_Price

                invoiceLine.itemsDiscount = item.itemsDiscount;   //  invoice_Details.Net_Total_Item_Commission

                invoiceLine.unitValue = new UnitValue
                {
                    amountEGP = item.amountEGP,   //  invoice_Details.Larg_Unit_Price
                    currencySold = "EGP",//item.unitType,//UnitCode
                    currencyExchangeRate = 0,
                    amountSold = 0
                };

                invoiceLine.discount = new Discount
                {
                    rate = 0,
                    amount = 0
                };


                invoiceLine.taxableItems = GetTaxableItem_Reverse(item.Id);

                invoiceLines.Add(invoiceLine);
            }
            return invoiceLines;
        }

        List<TaxableItem> GetTaxableItem_Reverse(long TaxInvoices_LineItemsId)
        {

            TaxReverseInvoices_ItemsTaxCLS taxCls = new  TaxReverseInvoices_ItemsTaxCLS ();
            List<TaxableItem> taxableItems = taxCls.getTaxableItems(TaxInvoices_LineItemsId);
            return taxableItems;
        }

    }

}
