using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models 
{
    

    //[Table("TaxInvoices_ItemsTax")]
    [Table("TaxInvoices_ItemsTaxes")]
    public class TaxInvoices_ItemsTaxes
    {
        public long Id { get; set; }
        public long TaxInvoices_ListId { get; set; }
        public long TaxInvoices_LineItemsId { get; set; }
        public string itemCode { get; set; }
        public string taxType { get; set; }
        public decimal?  amount { get; set; }
        public string subType { get; set; }
        public decimal?  rate { get; set; }

        [ForeignKey("TaxInvoices_LineItemsId")]
        public  TaxInvoices_LineItems  TaxInvoices_LineItems { get; set; }

        
    }
}
