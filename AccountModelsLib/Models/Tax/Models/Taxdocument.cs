using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{
    [Table("Taxdocuments")]
    public class Taxdocument
    {
        public long Id { get; set; }
        public long TaxInvoices_ListId { get; set; }
        public long invoices_ListId { get; set; }
        public long? TaxInvoiceNo { get; set; }
        
        public string internalId { get; set; }
        public string submissionId { get; set; }
        public string uuid { get; set; }
        public string hashKey { get; set; }
        public string longId { get; set; }
        public string Errors { get; set; }
        public int IsSuccessded { get; set; }
        public int? Staus { get; set; }
        public string StausStr { get; set; }

        public DateTime? DateOfSent { get; set; }

        
    }
}
