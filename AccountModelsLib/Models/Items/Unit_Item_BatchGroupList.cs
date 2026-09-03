using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{
    [Table("Unit_Item_BatchGroupLists")]
    public class Unit_Item_BatchGroupList
    {
        
        public long Id { get; set; }                // Primary Key (Identity)
        public Guid? BatchGroupId { get; set; }      // معرف GUID فريد
        public long? Supplier_Id { get; set; }
        public string GroupName { get; set; }
        public DateTime? ReceiptDate { get; set; }

        // ربط مع جدول Invoices_List
        public long? Invoices_ListId { get; set; }
        public long? User_Id { get; set; }
        public long? Branch_Id { get; set; }
        public DateTime? CreatedDate { get; set; }

        // الحقول المالية
        public decimal? WahbaValue { get; set; } = 0;
        public decimal? NawlounValue { get; set; } = 0;
        public decimal? Total3MolaRate { get; set; } = 0;
        public decimal? Total3Mola { get; set; } = 0;
        public decimal? Value_3Mola { get; set; } = 0;

        // العلاقات
        [ForeignKey("Supplier_Id")]
        public virtual Supplier Supplier { get; set; }
        [ForeignKey("Invoices_ListId")]
        public virtual Invoices_List Invoices_List { get; set; }   // علاقة بالفاتورة/قائمة الفواتير
        public virtual ICollection<Unit_Item_ExpireDate> Unit_Item_ExpireDates { get; set; }
        public int? IsActive { get;   set; } = 1;
    }
}

