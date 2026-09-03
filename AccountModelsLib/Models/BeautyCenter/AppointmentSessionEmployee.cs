using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{
    [Table("AppointmentSessionEmployees")]
    public class AppointmentSessionEmployee
    {
        public long Id { get; set; }

        //// ربط الخدمة بالحجز
        //public long AppointmentId { get; set; }

        // ربط بالسجل الخاص بالخدمة ضمن الحجز
        public long? AppointmentId { get; set; }
        public long? AppointmentServiceId { get; set; }
        public long AppointmentSessionId { get; set; }
        public long ServiceItem_Id { get; set; }
      

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