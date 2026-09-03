using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AccountModelsLib.Models
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Document
    {
        [JsonProperty("issuer")]
        public Issuer issuer { get; set; }

        [JsonProperty("receiver")]
        public Receiver receiver { get; set; }

        [JsonProperty("documentType")]
        public string documentType { get; set; }

        [JsonProperty("documentTypeVersion")]
        public string documentTypeVersion { get; set; }

        [JsonProperty("dateTimeIssued")]
        public string  dateTimeIssued { get; set; }

        [JsonProperty("serviceDeliveryDate")]
        public string serviceDeliveryDate { get; set; }

        [JsonProperty("taxpayerActivityCode")]
        

        public string taxpayerActivityCode { get; set; }

        [JsonProperty("internalID")]
        public string internalID { get; set; }

        [JsonProperty("purchaseOrderReference")]
        public string purchaseOrderReference { get; set; }

        [JsonProperty("purchaseOrderDescription")]
        public string purchaseOrderDescription { get; set; }

        [JsonProperty("salesOrderReference")]
        public string salesOrderReference { get; set; }

        [JsonProperty("salesOrderDescription")]
        public string salesOrderDescription { get; set; }

        [JsonProperty("proformaInvoiceNumber")]
        public string proformaInvoiceNumber { get; set; }

        [JsonProperty("payment")]
        public Payment payment { get; set; }

        [JsonProperty("delivery")]
        public Delivery delivery { get; set; }

        [JsonProperty("invoiceLines")]
        public List<InvoiceLine> invoiceLines { get; set; }

        [JsonProperty("totalDiscountAmount")]
        public decimal? totalDiscountAmount { get; set; }

        [JsonProperty("totalSalesAmount")]
        public decimal? totalSalesAmount { get; set; }

        [JsonProperty("netAmount")]
        public decimal? netAmount { get; set; }

        [JsonProperty("taxTotals")]
        public List<TaxTotal> taxTotals { get; set; }

        [JsonProperty("totalAmount")]
        public decimal? totalAmount { get; set; }

        [JsonProperty("extraDiscountAmount")]
        public decimal? extraDiscountAmount { get; set; }

        [JsonProperty("totalItemsDiscountAmount")]
        public decimal? totalItemsDiscountAmount { get; set; }
        //public List<Signature> signatures { get; set; }
    }
    
    public class Address
    {
        [JsonProperty("branchID")]
        public string branchID { get; set; }

        [JsonProperty("country")]
        public string country { get; set; }

        [JsonProperty("governate")]
        public string governate { get; set; }

        [JsonProperty("regionCity")]
        public string regionCity { get; set; }

        [JsonProperty("street")]
        public string street { get; set; }

        [JsonProperty("buildingNumber")]
        public string buildingNumber { get; set; }

        [JsonProperty("postalCode")]
        public string postalCode { get; set; }

        [JsonProperty("floor")]
        public string floor { get; set; }

        [JsonProperty("room")]
        public string room { get; set; }

        [JsonProperty("landmark")]
        public string landmark { get; set; }

        [JsonProperty("additionalInformation")]
        public string additionalInformation { get; set; }
    }
    public class Address_Receiver
    {
        

        [JsonProperty("country")]
        public string country { get; set; }

        [JsonProperty("governate")]
        public string governate { get; set; }

        [JsonProperty("regionCity")]
        public string regionCity { get; set; }

        [JsonProperty("street")]
        public string street { get; set; }

        [JsonProperty("buildingNumber")]
        public string buildingNumber { get; set; }

        [JsonProperty("postalCode")]
        public string postalCode { get; set; }

        [JsonProperty("floor")]
        public string floor { get; set; }

        [JsonProperty("room")]
        public string room { get; set; }

        [JsonProperty("landmark")]
        public string landmark { get; set; }

        [JsonProperty("additionalInformation")]
        public string additionalInformation { get; set; }
    }
    public class Delivery
    {
        [JsonProperty("approach")]
        public string approach { get; set; }

        [JsonProperty("packaging")]
        public string packaging { get; set; }

        [JsonProperty("dateValidity")]
        public string dateValidity { get; set; }

        [JsonProperty("exportPort")]
        public string exportPort { get; set; }

        [JsonProperty("countryOfOrigin")]
        public string countryOfOrigin { get; set; }

        [JsonProperty("grossWeight")]
        public decimal? grossWeight { get; set; }

        [JsonProperty("netWeight")]
        public decimal? netWeight { get; set; }

        [JsonProperty("terms")]
        public string terms { get; set; }
    }

    public class Discount
    {
        [JsonProperty("rate")]
        public decimal ? rate { get; set; }

        [JsonProperty("amount")]
        public decimal? amount { get; set; }
    }

    public class InvoiceLine
    {
        [JsonProperty("description")]
        public string description { get; set; }

        [JsonProperty("itemType")]
        public string itemType { get; set; }

        [JsonProperty("itemCode")]
        public string itemCode { get; set; }

        [JsonProperty("unitType")]
        public string unitType { get; set; }

        [JsonProperty("quantity")]
        public decimal? quantity { get; set; }

        [JsonProperty("internalCode")]
        public string internalCode { get; set; }

        [JsonProperty("salesTotal")]
        public decimal? salesTotal { get; set; }

        [JsonProperty("total")]
        public decimal? total { get; set; }

        [JsonProperty("valueDifference")]
        public decimal? valueDifference { get; set; }

        [JsonProperty("totalTaxableFees")]
        public decimal? totalTaxableFees { get; set; }

        [JsonProperty("netTotal")]
        public decimal? netTotal { get; set; }

        [JsonProperty("itemsDiscount")]
        public decimal? itemsDiscount { get; set; }

        [JsonProperty("unitValue")]
        public UnitValue unitValue { get; set; }

        [JsonProperty("discount")]
        public Discount discount { get; set; }

        [JsonProperty("taxableItems")]
        public List<TaxableItem> taxableItems { get; set; }
    }

    public class Issuer
    {
        [JsonProperty("address")]
        public Address address { get; set; }

        [JsonProperty("type")]
        public string type { get; set; }

        [JsonProperty("id")]
        public string id { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }
    }

    public class Payment
    {
        [JsonProperty("bankName")]
        public string bankName { get; set; } = "";

        [JsonProperty("bankAddress")]
        public string bankAddress { get; set; } = "";

        [JsonProperty("bankAccountNo")]
        public string bankAccountNo { get; set; } = "";

        [JsonProperty("bankAccountIBAN")]
        public string bankAccountIBAN { get; set; } = "";

        [JsonProperty("swiftCode")]
        public string swiftCode { get; set; } = "";

        [JsonProperty("terms")]
        public string terms { get; set; } = "";
    }

    public class Receiver
    {
        [JsonProperty("address")]
        public Address_Receiver address { get; set; }

        [JsonProperty("type")]
        public string type { get; set; }

        [JsonProperty("id")]
        public string id { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }
    }

    

    public class TaxableItem
    {
        [JsonProperty("taxType")]
        public string taxType { get; set; }

        [JsonProperty("amount")]
        public decimal? amount { get; set; }

        [JsonProperty("subType")]
        public string subType { get; set; }

        [JsonProperty("rate")]
        public decimal? rate { get; set; }
    }

    public class TaxTotal
    {
        [JsonProperty("taxType")]
        public string taxType { get; set; }

        [JsonProperty("amount")]
        public decimal? amount { get; set; }
    }

    public class UnitValue
    {
        [JsonProperty("currencySold")]
        public string currencySold { get; set; }

        [JsonProperty("amountEGP")]
        public decimal? amountEGP { get; set; }

        [JsonProperty("amountSold")]
        public decimal? amountSold { get; set; }

        [JsonProperty("currencyExchangeRate")]
        public decimal? currencyExchangeRate { get; set; }
    }

    public class Signature
    {
        public string signatureType { get; set; } = "I";

        public string value { get; set; } = "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";
    }
}
