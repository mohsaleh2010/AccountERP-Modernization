using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{
    [Table("AppointmentCustomerPackages")]

    public  class AppointmentCustomerPackage
    {
        public long Id { get; set; }
        public long CustomerId { get; set; }
        public long AppointmentPackageId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? IsActive { get; set; } = true ;
        public int? RemainingSessions { get; set; }
        public long? Invoices_ListId { get; set; }
        public string  Notes { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string  StatusName { get; set; }
        public int Status { get; set; }
        
        public long? User_Id { get; set; }
        public long? Branch_Id { get; set; }




        // Navigation properties (optional)
        public virtual Customer Customer { get; set; }
        public virtual AppointmentPackage AppointmentPackage { get; set; }
    }
}
