using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models 
{
    public class AppointmentServiceVM
    {
        public long Id { get; set; }
        public long ServiceItem_Id { get; set; }
        public string ServiceItem_Name { get; set; }
        public decimal Qnty { get; set; }
        public decimal S_Price { get; set; }
        public int DurationMinutes { get; set; }
        public string AppointmentServiceNotes { get; set; }
        public List<Employee> Employees { get; set; }
        public List<EmployeeViewModel> EmployeesVM { get; set; }
        public virtual ICollection<AppointmentServiceEmployee> appointmentServiceEmployee { get; set; }
        public string EmployeeNames
        {
            get
            {
                return Employees != null ? string.Join(Environment.NewLine, Employees.Select(e => e.name)) : "";
            }
            
        }


        public bool Selected { get; set; }
        

        
    }

}
