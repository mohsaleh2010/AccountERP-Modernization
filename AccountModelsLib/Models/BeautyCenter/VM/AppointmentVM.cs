using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models 
{
    [NotMapped]
    public  class AppointmentVM: Appointment
    {
        public string  AppointmentInfo { get; set; }
    }
}
