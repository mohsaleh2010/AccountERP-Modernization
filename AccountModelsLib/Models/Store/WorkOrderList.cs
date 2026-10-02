
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{
    [Table("WorkOrderList")]
    public class WorkOrderList
    {
        [Key]
        public long Id { get; set; }

        // رقم أمر التشغيل
        public long WorkOrder_No { get; set; }

        // تاريخ إنشاء الأمر
        public DateTime? WorkOrderDate { get; set; }

        

         

        // حالة أمر التشغيل (مفتوح / قيد التنفيذ / مكتمل / مغلق)
        public int? Status { get; set; }
        public string  StatusStr { get; set; }
        public int? WorkOrderType { get; set; }
        public string  WorkOrderTypeStr { get; set; }
        // ارتباط بالمخزن الرئيسي
        public long? StoreId { get; set; }
        public string StoreName { get; set; }

        // العميل (لو Make To Order)
        public long? CustomerId { get; set; }
        public string CustomerName { get; set; }

         

        public long? MachineId { get; set; }
        public string MachineName { get; set; }

        // بيانات الموظف المسئول
        public long? EmployeeId { get; set; }
        public string EmployeeName { get; set; }

        // ملاحظات
        public string NotesAll { get; set; }

        // تتبع العمليات
        public DateTime? OperationDate { get; set; }
        public DateTime? Insert_ServerDateTime { get; set; }
        public DateTime? Update_ServerDateTime { get; set; }
        public DateTime? EditOperationDate { get; set; }
        public long? Edit_User_Id { get; set; }

        // Navigation
        public ICollection<WorkOrderDetail> WorkOrderDetails { get; set; }
       
    }


}
