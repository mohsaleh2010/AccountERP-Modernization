using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{
    [NotMapped]
    public class EmployeeRelatedItemsViewModel : EmployeeRelatedItem
    {

        public string Emp_Name { get; set; }
        public string Job_Name { get; set; }
        public string Item_Name { get; set; }



    }
}
