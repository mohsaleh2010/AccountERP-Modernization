  
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models 
{
    [Table("TaxReverseInvoices_LineItems")]
    public class TaxReverseInvoices_LineItems
    {
        
        [Key]
        public long Id { get; set; }
        public long TaxInvoices_ListId { get; set; }
        public long? ItemCodeTaxAcceptedId { get; set; }
        public long? Item_Id { get; set; }
        public long? Unit_Item_Id { get; set; }

        public long? CurrencyId { get; set; }
        public string description { get; set; }
        public string itemType { get; set; }
        public string itemCode { get; set; }
        public string unitType { get; set; }
        public decimal?  quantity { get; set; }
        public string weightUnitType { get; set; }
        public decimal weightQuantity { get; set; }
        public string unitValue_currencySold { get; set; }
        public decimal unitValue_amountSold { get; set; }
        public decimal unitValue_amountEGP { get; set; }
        public decimal unitValue_currencyExchangeRate { get; set; }
        public string factoryUnitValue { get; set; }
        public decimal? salesTotal { get; set; }
        public decimal? discount_rate { get; set; }
        public decimal? discount_amount { get; set; }
        public string internalCode { get; set; }
       
        public decimal? itemsDiscount { get; set; }
        public decimal? netTotal { get; set; }
        public decimal? totalTaxableFees { get; set; }
        public decimal? valueDifference { get; set; }
        public decimal? total { get; set; }
        public decimal? amountSold { get; set; }
        public decimal? amountEGP { get; set; }
        public decimal currencyExchangeRate { get; set; }
        public string currencySold { get; set; }


        public List<TaxReverseInvoices_ItemsTaxes> taxReverseInvoices_ItemsTaxes { get; set; }


    }
}
