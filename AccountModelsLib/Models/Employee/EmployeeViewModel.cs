using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models 
{
    [NotMapped]
    public  class EmployeeViewModel : Employee 
    {
        //public long Id { get; set; }
        //public string name { get; set; }
        public string JobName { get; set; }
        public bool  IsSelect { get; set; }
        public long CarsWorkOrdersItemProcessedDetailsId { get; set; }
        public long? CarsWorkOrdersId { get; set; }
        public long? CarsId { get; set; }
        public long? EmployeeRelatedItemsId { get; set; }
        
        public int? IsRatio { get; set; } = 0;
        public decimal? Ratio { get; set; } = 0;
        public decimal? Value { get; set; } = 0;
        public string Notes { get; set; }

        //public Employee Employee { get; set; }
    }
}
