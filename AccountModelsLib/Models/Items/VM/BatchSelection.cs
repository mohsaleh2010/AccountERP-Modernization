using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{
    public class BatchSelection
    {
        public long Id { get; set; }
        public string BatchNo { get; set; }
        public string Item_Name { get; set; }
        public long Item_Id { get; set; }
        public string SupplierName { get; set; }
        public long Supplier_Id { get; set; }
        public decimal? Larg_Balance { get; set; }
        public decimal? Item_No { get; set; }
        public decimal? P_Price { get; set; }
        public decimal? QntyValue { get; set; }
        

    }

}