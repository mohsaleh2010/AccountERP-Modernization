using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models 
{
 

    [Table("Unit_Item_Sales_ItemLists")]
    public class Unit_Item_Sales_ItemList
    {
        [Key]
        public long Id { get; set; }

        public long? Item_Id { get; set; }
        public long? Unit_Item_Table_Id { get; set; }

        public DateTime SalesDate { get; set; }

        public int? ItemSourceType { get; set; }
        public long? Unit_Item_ExpireDateId { get; set; }
        public string  ItemNameSource { get; set; }
         

        [ForeignKey("Item_Id")]
        public virtual Items items { get; set; }

        public virtual ICollection<Unit_Item_SalesDate> Unit_Item_SalesDate { get; set; }
    }

   

}
