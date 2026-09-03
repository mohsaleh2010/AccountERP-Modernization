using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models 
{
    public  class TotalsInvoicePreview
    {
       
        public decimal? Qnty_Value { get; set; }
        //public decimal? commission { get; set; }
        public decimal? TaxSales10 { get; set; }
        public decimal? ATS05 { get; set; }
        public decimal? TaxSales10_Value { get; set; }
        public decimal? ATS05_Value { get; set; }
        

        public decimal? NetInvoiceTotal { get; set; }
    }
}
