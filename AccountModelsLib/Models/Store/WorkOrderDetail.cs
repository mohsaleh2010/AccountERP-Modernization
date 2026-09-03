
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{
    [Table("WorkOrderDetail")]
    public class WorkOrderDetail
    {
        [Key]
        public long Id { get; set; }

        // الربط بالرأس
        public long WorkOrderListId { get; set; }

        

        // الصنف المطلوب (خام / نصف مصنع)
        public long Item_Id { get; set; }
        public string Item_Name { get; set; }

        // الوحدة
        public long UnitId { get; set; }
        public string UnitName { get; set; }

        // الكمية المطلوبة
        public decimal? RequiredQuantity { get; set; }

        // الكمية المصروفة فعلاً (من Order_Purchasing مع OrderType = صرف إنتاج)
        public decimal? IssuedQuantity { get; set; }

        // الكمية المستلمة فعلاً (من Order_Purchasing مع OrderType = استلام إنتاج)
        public decimal? ReceivedQuantity { get; set; }

        // ملاحظات
        public string Notes { get; set; }

        // تتبع العمليات
        public DateTime? OperationDate { get; set; }
        public DateTime? Insert_ServerDateTime { get; set; }
        public DateTime? Update_ServerDateTime { get; set; }
        public DateTime? EditOperationDate { get; set; }
        public long? Edit_User_Id { get; set; }


        [ForeignKey("WorkOrderListId")]
        public WorkOrderList WorkOrderList { get; set; }
    }


}
