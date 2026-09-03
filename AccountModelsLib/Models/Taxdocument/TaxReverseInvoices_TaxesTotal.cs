using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models 
{
    [Table("TaxReverseInvoices_TaxesTotal")]
    public class TaxReverseInvoices_TaxesTotal
    {
        public long Id { get; set; }
        public long TaxInvoices_ListId { get; set; }
        
        public string taxType { get; set; }
        public decimal  amount { get; set; }
    }

}
