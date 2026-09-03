using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models 
{
    [NotMapped]
    public  class Unit_Item_ExpireDateVM: Unit_Item_ExpireDate
    {
        
        public string Item_Name { get; set; }
        public string  ShowFullName { get; set; }
        public string SupplierName { get; set; }
        public string GroupName { get; set; }
    }
}
