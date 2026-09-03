using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{
    public class ItemsForCarsViewModel
    {
        public long ItemsForCarsId { get; set; }
        public long Item_Id { get; set; }
        public long Lookup_CarTypeId { get; set; }
        public long Lookup_CarTypeModeLId { get; set; }
        public int IsMain { get; set; }
        
        public string Lookup_CarTypeCode { get; set; }    
        public string Item_Name { get; set; }    
        public string Lookup_CarTypeName { get; set; }    
        public string Lookup_CarTypeModeLName { get; set; }    

    }

}
