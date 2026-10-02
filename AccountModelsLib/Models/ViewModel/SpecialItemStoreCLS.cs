using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models 
{
    public  class SpecialItemStoreCLS
    {
        public decimal? S_priceAvgCost { get; set; }
        public decimal? P_priceAvgCost { get; set; }

        public long Item_Id { get; set; }
        public long Unit_Item_Table_Id { get; set; }
        
        public decimal Larg_Qnty { get; set; }
        public decimal Small_Qnty { get; set; }
        public decimal Item_No_Qnty { get; set; }
        public decimal Full_qua { get; set; }
        public decimal D_Qua { get; set; }
        public decimal Item_No_Balance { get; set; }
        public decimal S_unit_Value { get; set; }
        public decimal No3eboa { get; set; }
         public decimal? P_Price { get; set; }
         public decimal? S_Price { get; set; }
        public string B_Unit { get; set; }
        public string S_unit { get; set; }

        public long? Machine_Id { get; set; }
        public string Item_Name { get; set; }
        public string Notes_Item { get; set; }
        public string Notes_From { get; set; }
        public string Notes_To { get; set; }
        public long? Unit_Id { get; set; }
        //public string UnitStr { get; set; }
        public string Code { get; set; }
    }
}
