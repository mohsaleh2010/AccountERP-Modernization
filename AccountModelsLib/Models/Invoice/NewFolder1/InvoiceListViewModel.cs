using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models 
{
    public  class InvoiceListViewModel
    {
        public InvoiceListViewModel() { }

        public long Id { get; set; }
        public string CustomerName { get; set; }
        public long CustomerId { get; set; }
        public Nullable<decimal> Total_Qnty_Value { get; set; }
        public Nullable<decimal> Total_Item_CommissionValue { get; set; }
        public Nullable<decimal> Total_Item_TaxSales10_Value { get; set; }
        public Nullable<decimal> Total_Item_ATS05_Value { get; set; }
        public Nullable<decimal> Total_PriceWithoutTax { get; set; }
        public Nullable<decimal> Total_Item_Net_Total_Price { get; set; }
        public Nullable<decimal> Total_Item_No { get; set; }

         
    }
}
