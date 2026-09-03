  
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models 
{
    [Table("TaxReverseInvoices_List")]
    public class TaxReverseInvoices_List
    {
        public long Id { get; set; }
        public long? Tax_BranchesId { get; set; }
        
        
        public long? Invoices_ListId { get; set; }
        public long? Customer_Id { get; set; }
        public long? TaxInvoiceNo { get; set; }
        public string internalID { get; set; }
        public string Receiver_type { get; set; }
        public string Receiver_id { get; set; }
        public string Receiver_name { get; set; }
        public string issuer_type { get; set; }
        public string issuer_id { get; set; }
        public string issuer_name { get; set; }
        public string documentType { get; set; }
        public string documentTypeVersion { get; set; }
        public DateTime  dateTimeIssued { get; set; }
        public string taxpayerActivityCode { get; set; }
        public string purchaseOrderReference { get; set; }
        public string purchaseOrderDescription { get; set; }
        public string salesOrderReference { get; set; }
        public string salesOrderDescription { get; set; }
        public string proformaInvoiceNumber { get; set; }
        public decimal?  totalDiscountAmount { get; set; }
        public decimal? totalSalesAmount { get; set; }
        public decimal? netAmount { get; set; }
        public decimal? taxTotals { get; set; }
        public decimal? totalAmount { get; set; }
        public decimal? extraDiscountAmount { get; set; }
        public decimal? totalItemsDiscountAmount { get; set; }
      
    }
}
