using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models 
{
    [Table("Appointments")]
    public class Appointment
    {

        public long Id { get; set; }
        public long AppointmentNumber { get; set; }
        public DateTime? AppointmentDate { get; set; }
        public DateTime? AppointmentDateTime { get; set; }
        public DateTime? AppointmentTime { get; set; }
        public long? Customer_Id { get; set; }
        public string CustomerName { get; set; }
       
        public string CustomerPhone { get; set; }
        public string AppointmentNotes { get; set; }
         
       

        
        public int IsCompleteStatus { get; set; } = 0;

        [ForeignKey("Customer_Id")]
        public virtual Customer Customer { get; set; }

        public virtual ICollection<AppointmentService> appointmentService { get; set; }
    }

    [Table("AppointmentServices")]
    public class AppointmentService
    {
       
        public long Id { get; set; }
      
        
        // ربط الخدمة بالحجز
        public long AppointmentId { get; set; }

        public string ServiceItem_Name { get; set; }

        public long ServiceItem_Id { get; set; }
        public long Unit_Item_Table_Id { get; set; } = 0;
        

        // الكمية (عدد مرات الخدمة في الحجز الحالي، مثل عدد الجلسات)
        public decimal? Qnty { get; set; } = 1;

        // سعر الخدمة في هذا الحجز (يمكن أن يختلف عن السعر الأصلي)
        public decimal? S_Price { get; set; } = 0;
        public decimal? QntyValue { get; set; } = 0;

        // المدة التقديرية لأداء هذه الخدمة (اختياري)
        public int? DurationMinutes { get; set; }
        public int? TotalSessions { get; set; }
        
        public string AppointmentServiceNotes { get; set; }
        public string EmployeeNames { get; set; }
        public int IsProcessedAsInvoice { get; set; } = 0;
        public int Is_ItemOrService { get; set; } = 0;
        public string   ItemOrServiceStr { get; set; }  
        
        public long? Invoices_List_Id { get; set; }
        public long? Invoices_Details_Id { get; set; }


        // العلاقة مع كلاس الحجز
        [ForeignKey("AppointmentId")]
        public virtual Appointment Appointment { get; set; }

        // العلاقة مع كلاس الصنف الذي يمثل الخدمة
        [ForeignKey("ServiceItem_Id")]
        public virtual Items Items { get; set; }

        // الموظفون المشاركون في تنفيذ هذه الخدمة
        public virtual ICollection<AppointmentServiceEmployee> appointmentServiceEmployee { get; set; }
    }

    [Table("AppointmentServiceEmployees")]
    public class AppointmentServiceEmployee
    {
        public long Id { get; set; }

        //// ربط الخدمة بالحجز
        //public long AppointmentId { get; set; }

        // ربط بالسجل الخاص بالخدمة ضمن الحجز
        public long AppointmentServiceId { get; set; }

        // ربط بالموظف
        public long EmployeeId { get; set; }

        // النسبة التي يساهم بها الموظف (اختياري - مثال: 50%)
        public int? IsContributionPercentage { get; set; } = 0;
        public decimal? ContributionPercentage { get; set; } = 0;
        public decimal? ContributionValue { get; set; } = 0;

        // الملاحظات أو وصف الدور (مثلاً: رئيسي، مساعد، ...)
        public string AppointmentServiceEmployeeNotes { get; set; }
        //

        public int IsCompletePayed { get; set; } = 0;


        // العلاقة مع كلاس AppointmentService
        [ForeignKey("AppointmentServiceId")]
        public virtual AppointmentService AppointmentService { get; set; }

        // العلاقة مع كلاس Employee (سنقوم بتعريفه لاحقًا أو ربطه بالجدول الموجود لديك)
        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; }
    }


}
