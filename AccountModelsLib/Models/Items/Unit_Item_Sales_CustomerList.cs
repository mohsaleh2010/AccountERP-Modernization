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


    [Table("Unit_Item_Sales_CustomerLists")]
    public class Unit_Item_Sales_CustomerList
    {
        [Key]
        public long Id { get; set; }

        public long? Customer_Id { get; set; }

        public DateTime SalesDate { get; set; }

        [ForeignKey("Customer_Id")]
        public virtual Customer customer { get; set; }

        public virtual ICollection<Unit_Item_SalesDate> Unit_Item_SalesDate { get; set; }
    }

    

}
