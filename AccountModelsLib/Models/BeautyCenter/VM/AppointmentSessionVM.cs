using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 

namespace AccountModelsLib.Models 
{
    [NotMapped]
    public  class AppointmentSessionVM: AppointmentSession
    {
        
        public long AppointmentSessionId { get; set; }

        public bool isFromAppointmentPrev { get; set; }
        

        public string StatusName { get; set; }
        public string ServiceItem_Name { get; set; }
        public long CustomerId { get; set; }
        public bool IsFromPackage { get; set; }
        //public long? AppointmentCustomerPackageId { get; set; }
        //public long ServiceItem_Id { get; set; }
    }
}
