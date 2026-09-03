using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models 
{
    [NotMapped]
    public class ServiceViewModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string ItemServiceCategory { get; set; }
        public decimal S_Price { get; set; }
        public int ServiceDurationInMinutes { get; set; }
        public int IsCommissionEmployeeBased_True { get; set; }
        public int IsCommissionEmployeeBased { get; set; }
        public decimal CommissionEmployeeValue { get; set; }
    }

}
