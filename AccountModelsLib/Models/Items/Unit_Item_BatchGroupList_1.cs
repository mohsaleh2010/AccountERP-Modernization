using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib.Models
{
    [NotMapped]
        //[Table("Unit_Item_BatchList")]
        public class Unit_Item_BatchGroupListOld
        {
            [Key]
            public long Id { get; set; }

            public long? Supplier_Id { get; set; }

            public DateTime ReceiptDate { get; set; }

            [ForeignKey("Supplier_Id")]
            public virtual Supplier Supplier { get; set; }

            public virtual ICollection<Unit_Item_ExpireDate> Unit_Item_ExpireDates { get; set; }
        }

}
