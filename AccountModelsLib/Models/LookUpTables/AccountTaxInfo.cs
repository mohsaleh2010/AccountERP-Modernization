using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models 
{
    [Table("AccountTaxInfo")]
    public class AccountTaxInfo
    {
        public long Id { get; set; }
        public string TaxName_Ar { get; set; }
        public string TaxName_En { get; set; }
        public string MainCode { get; set; }
        public string SubCode { get; set; }
        public Nullable<decimal> TaxRate { get; set; }
        public Nullable<decimal>   MinAllowedValueToComputeTax { get; set; }


    }

}
