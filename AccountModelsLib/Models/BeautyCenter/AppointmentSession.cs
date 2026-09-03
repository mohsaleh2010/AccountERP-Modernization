using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{
    [Table("AppointmentSessions")]
    public class AppointmentSession
    {

        public long Id { get; set; }
        public long? AppointmentId { get; set; }
        public long? AppointmentServiceId { get; set; }
        public long? ServiceItem_Id { get; set; }
        public long? EmployeeId { get; set; }
        public int? DurationinMinutes { get; set; }
        public DateTime? SessionDate { get; set; }
        public int Status { get; set; }
        public string StatusStr { get; set; }
        public string EmployeeNames { get; set; }
        public string Notes { get; set; }
        public string BeforeImagePath { get; set; }
        public string AfterImagePath { get; set; }
        public int? Rating { get; set; }
        public string Feedback { get; set; }
        public int? IsCompletePayed { get; set; } = 0;
        public long? User_Id { get; set; }
        public long? Branch_Id { get; set; }

        // FK to package service if session is part of a package
        public long? AppointmentCustomerPackageServiceId { get; set; }
        public long? AppointmentCustomerPackageId { get; set; }
        
        [ForeignKey("AppointmentId")]
        public virtual Appointment Appointment { get; set; }

        [ForeignKey("AppointmentServiceId")]
        public virtual AppointmentService AppointmentService { get; set; }

        [ForeignKey("ServiceItem_Id")]
        public virtual Items Items { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; }

        [ForeignKey("AppointmentCustomerPackageServiceId")]
        public virtual AppointmentCustomerPackageService CustomerPackageService { get; set; }

        [ForeignKey("AppointmentCustomerPackageId")]
        public virtual AppointmentCustomerPackage CustomerPackage { get; set; }


    }
}
