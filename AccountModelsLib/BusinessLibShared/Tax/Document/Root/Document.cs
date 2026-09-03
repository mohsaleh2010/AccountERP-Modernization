using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models.Tax 
{
    public class Document
    {
        public Issuer issuer { get; set; }
        public Receiver receiver { get; set; }
        public string documentType { get; set; }
        public string documentTypeVersion { get; set; }
        public string dateTimeIssued { get; set; }
        public string taxpayerActivityCode { get; set; }
        public string internalID { get; set; }
        public string purchaseOrderReference { get; set; }
        public string purchaseOrderDescription { get; set; }
        public string salesOrderReference { get; set; }
        public string salesOrderDescription { get; set; }
        public string proformaInvoiceNumber { get; set; }
        public Payment payment { get; set; }
        public Delivery delivery { get; set; }
        public List<InvoiceLine> invoiceLines { get; set; }
        public decimal?  totalDiscountAmount { get; set; }
        public decimal? totalSalesAmount { get; set; }
        public decimal? netAmount { get; set; }
        public List<TaxTotal> taxTotals { get; set; }
        public decimal? totalAmount { get; set; }
        public decimal? extraDiscountAmount { get; set; }
        public decimal? totalItemsDiscountAmount { get; set; }
    }
    public class Address
    {
        public string branchID { get; set; }
        public string country { get; set; }
        public string governate { get; set; }
        public string regionCity { get; set; }
        public string street { get; set; }
        public string buildingNumber { get; set; }
        public string postalCode { get; set; }
        public string floor { get; set; }
        public string room { get; set; }
        public string landmark { get; set; }
        public string additionalInformation { get; set; }
    }

    public class Delivery
    {
        public string approach { get; set; }
        public string packaging { get; set; }
        public DateTime dateValidity { get; set; }
        public string exportPort { get; set; }
        public string countryOfOrigin { get; set; }
        public decimal? grossWeight { get; set; }
        public decimal? netWeight { get; set; }
        public string terms { get; set; }
    }

    public class Discount
    {
        public int rate { get; set; }
        public decimal? amount { get; set; }
    }

    public class InvoiceLine
    {
        public string description { get; set; }
        public string itemType { get; set; }
        public string itemCode { get; set; }
        public string unitType { get; set; }
        public decimal?  quantity { get; set; }
        public string internalCode { get; set; }
        public decimal? salesTotal { get; set; }
        public decimal? total { get; set; }
        public decimal? valueDifference { get; set; }
        public decimal? totalTaxableFees { get; set; }
        public decimal? netTotal { get; set; }
        public decimal? itemsDiscount { get; set; }
        public UnitValue unitValue { get; set; }
        public Discount discount { get; set; }
        public List<TaxableItem> taxableItems { get; set; }
    }

    public class Issuer
    {
        public Address address { get; set; }
        public string type { get; set; }
        public string id { get; set; }
        public string name { get; set; }
    }

    public class Payment
    {
        public string bankName { get; set; }
        public string bankAddress { get; set; }
        public string bankAccountNo { get; set; }
        public string bankAccountIBAN { get; set; }
        public string swiftCode { get; set; }
        public string terms { get; set; }
    }

    public class Receiver
    {
        public Address address { get; set; }
        public string type { get; set; }
        public string id { get; set; }
        public string name { get; set; }
    }

   

    public class TaxableItem
    {
        public string taxType { get; set; }
        public decimal? amount { get; set; }
        public string subType { get; set; }
        public decimal? rate { get; set; }
    }

    public class TaxTotal
    {
        public string taxType { get; set; }
        public decimal? amount { get; set; }
    }

    public class UnitValue
    {
        public string currencySold { get; set; }
        public decimal? amountEGP { get; set; }
        public decimal? amountSold { get; set; }
        public decimal? currencyExchangeRate { get; set; }
    }


}
