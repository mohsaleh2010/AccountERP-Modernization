using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{
    [Table("AppointmentPackages")]

    public class AppointmentPackage
    {
        public long Id { get; set; }
        public string Name_Package { get; set; }
        //public long? ItemServiceId { get; set; }
        //public string ItemService_Name { get; set; }
        //public int TotalSessions { get; set; }
        public int? DurationInDays { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public long? User_Id { get; set; }
        public long? Branch_Id { get; set; }
        // Navigation Property (اختياري إذا عندك جدول Services)

        //[ForeignKey("ItemServiceId")]
        //public virtual Items Items { get; set; }

        public virtual ICollection<AppointmentPackageService> AppointmentPackageServices { get; set; }
    }

    [Table("AppointmentPackageServices")]
    public class AppointmentPackageService
    {
        public long Id { get; set; }
        public long AppointmentPackageId { get; set; }
        public long ItemServiceId { get; set; }
        public string ItemService_Name { get; set; } = null;
        public int TotalSessions { get; set; }

        public bool IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }

        public string Description { get; set; } = null;

        [ForeignKey("AppointmentPackageId")]
        public virtual AppointmentPackage AppointmentPackage { get; set; }

        [ForeignKey("ItemServiceId")]
        public virtual Items ItemService { get; set; }
    }
}