using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{
    [Table("EmployeeRelatedItems")]
    public class EmployeeRelatedItem
    {
        public long Id { get; set; }
        public long? EmployeesId { get; set; }
        public long? jobsListId { get; set; }
        public long? Item_Id { get; set; }

        [ForeignKey("Item_Id")]
        public virtual Items Item { get; set; }

        [ForeignKey("EmployeesId")]
        public virtual Employee Employee { get; set; }

        [ForeignKey("jobsListId")]
        public virtual jobsList jobslist { get; set; }

    }
}
