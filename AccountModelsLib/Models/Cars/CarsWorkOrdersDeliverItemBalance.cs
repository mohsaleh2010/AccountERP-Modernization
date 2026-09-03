using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{
    [Table("CarsWorkOrdersDeliverItemBalances")]
    public class CarsWorkOrdersDeliverItemBalance
    {
        public long Id { get; set; }
        public long? WarshaStoresId { get; set; }
        public long? CarsWorkOrdersId { get; set; }
        public long? Item_Id { get; set; }
        
        public decimal? Qnty { get; set; }
        public decimal? Larg_Qnty { get; set; }
        public decimal? Small_Qnty { get; set; }
        public decimal? Item_No { get; set; }
        public long? Unit_Id { get; set; }
        public string UnitStr { get; set; }
        public string ItemCode { get; set; }
        public decimal? P_Price { get;   set; }
        public Nullable<decimal> P_priceAvgCost { get; set; }
        public decimal? PriceUnit { get; set; }
        public decimal? Larg_Unit_Price { get; set; }
        public decimal? Small_Unit_Price { get; set; }
        public int? Unit_Used_for_Price { get; set; }
        
    }
}
