using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models 
{
    [Table("Supplier_Price")]
    public class SupplierPrice
    {
        [Key]
        public long Id { get; set; }

        public long Supplier_Id { get; set; }

        public long Item_Id { get; set; }

        
        public decimal? PriceUnit { get; set; }

        public int? Store_Id { get; set; }

        public DateTime? OperationDate { get; set; }

        public long? User_Id { get; set; }

        public long? Branch_Id { get; set; }

        public DateTime? Insert_ServerDateTime { get; set; }

       
        public DateTime Update_ServerDateTime { get; set; }

        public DateTime? EditOperationDate { get; set; }

        public long? Edit_User_Id { get; set; }

        public long? ProcessNo { get; set; }

        public int? Price_PurchaseType { get; set; }


        [ForeignKey("Item_Id")]
        public virtual Items Item { get; set; }

         


        [ForeignKey("Supplier_Id")]
        public virtual Supplier Supplier { get; set; }
    }
}
