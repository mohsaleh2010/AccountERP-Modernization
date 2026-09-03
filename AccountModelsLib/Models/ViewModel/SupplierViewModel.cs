using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models 
{
    [NotMapped]
    public  class SupplierViewModel : Supplier
    {
        public int Serial { get; set; }
        
        public string SubAccount_Name { get; set; }
         
        public int? Currency_Name { get; set; }
   
        public string GroupName { get; set; }
         
        public string CityName { get; set; }
        public string GovernName { get; set; }
        public string AreaName { get; set; }
        public string CountryName { get; set; }

    }
}
