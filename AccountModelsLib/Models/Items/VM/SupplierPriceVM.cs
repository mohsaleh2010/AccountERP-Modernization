using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{
    [NotMapped]
    public  class SupplierPriceVM: SupplierPrice
    {
        public string  Item_Name { get; set; }
        public string Supplier_Name { get; set; }
    }
}
