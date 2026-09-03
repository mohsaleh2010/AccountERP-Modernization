using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{
    [Table("AppointmentCustomerPackageServices")]

    public class AppointmentCustomerPackageService
    {
        public long Id { get; set; }
        public long ItemService_Id { get; set; }
        public long AppointmentCustomerPackageId { get; set; }    
        public long AppointmentPackageId { get; set; }
        
        
        public long Customer_Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        
        public int TotalSessions { get; set; }
        public int UsedSessions { get; set; }
        public int RemainingSessions { get; set; }
        public byte Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        
        [ForeignKey("AppointmentPackageId")]
        public virtual AppointmentPackage AppointmentPackage { get; set; }
        [ForeignKey("Customer_Id")]
        public virtual Customer Customer { get; set; }
        [ForeignKey("ItemService_Id")]
        public virtual Items ItemServices { get; set; }
    }
}

