using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
 
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 

namespace AccountModelsLib.Models
{
    public class AppointmentSessionCLS
    {
        private readonly AccountModelsLib.operations opp;

        public AppointmentSessionCLS()
        {
            opp = new AccountModelsLib.operations(); // إنشاء كائن AccountModelsLib.operations لإدارة DbContext
        }

        // 1️⃣ Get All Sessions (Filtered)
        public List<AppointmentSessionDTO> GetSessions(string status = "All", int? employeeId = null, DateTime? date = null)
        {
            //var query = opp.db.AppointmentSessions
            //   .Select(s => new AppointmentSessionDTO
            //   {
            //       Id = s.Id,
            //       CustomerName = s.Appointment.Customer.Name,
            //       ServiceName = s.Items.Item_Name,
            //       EmployeeName = s.Employee.name,
            //       EmployeeId = s.EmployeeId.Value,
            //       SessionDate = s.SessionDate,
            //       Status = s.Status
            //   });

            //if (!string.IsNullOrEmpty(status) && status != "All")
            //    query = query.Where(s => s.Status == status);

            //if (employeeId.HasValue)
            //    query = query.Where(s => s.EmployeeId == employeeId.Value);

            //if (date.HasValue)
            //    query = query.Where(s => s.SessionDate == date.Value);

            //query = query.OrderByDescending(s => s.SessionDate);

            //return query.ToList();
            return new List<AppointmentSessionDTO>();
        }

        // 2️⃣ Get Session Details by ID
        public AppointmentSessionDetailsDTO GetSessionDetails(long sessionId)
        {
            return opp.db.AppointmentSessions
                .Where(s => s.Id == sessionId)
                .Select(s => new AppointmentSessionDetailsDTO
                {
                    Id = s.Id,
                    CustomerName = s.Appointment.Customer.Name,
                    ServiceItem_Name = s.Items.Item_Name,
                    EmployeeName = s.Employee.name,
                    SessionDate = s.SessionDate.Value,
                    Notes = s.Notes,
                    BeforeImagePath = s.BeforeImagePath,
                    AfterImagePath = s.AfterImagePath,
                    Rating = s.Rating.Value,
                    Feedback = s.Feedback
                }).FirstOrDefault();
        }

        // 3️⃣ Update Session Details
        public bool UpdateSessionDetails(int sessionId, string notes, string beforeImg, string afterImg, int rating, string feedback)
        {
            var session = opp.db.AppointmentSessions.FirstOrDefault(s => s.Id == sessionId);
            if (session == null) return false;

            session.Notes = notes;
            session.BeforeImagePath = beforeImg;
            session.AfterImagePath = afterImg;
            session.Rating = rating;
            session.Feedback = feedback;

            opp.db.SaveChanges();
            return true;
        }

        // 4️⃣ Change Session Status
        

        // 5️⃣ Get Employees
        public List<Employee> GetEmployees()
        {
            return opp.db.Employees.OrderBy(e => e.name).ToList();
        }

        // 6️⃣ Add New AppointmentSession
      
        // 7️⃣ Delete Session
        public bool DeleteSession(int sessionId)
        {
            var session = opp.db.AppointmentSessions.FirstOrDefault(s => s.Id == sessionId);
            if (session == null) return false;

            opp.db.AppointmentSessions.Remove(session);
            opp.db.SaveChanges();
            return true;
        }

        // 8️⃣ Get Sessions Pending Invoice
       
    }

    // DTOs remain unchanged
    public class AppointmentSessionDTO
    {
        public long Id { get; set; }
        public string CustomerName { get; set; }
        public string ServiceName { get; set; }
        public string EmployeeName { get; set; }
        public long EmployeeId { get; set; }
        public DateTime? SessionDate { get; set; }
        public string Status { get; set; }
    }

    public class AppointmentSessionDetailsDTO
    {
        public long Id { get; set; }
        public string CustomerName { get; set; }
        public string ServiceItem_Name { get; set; }
        public string EmployeeName { get; set; }
        public DateTime? SessionDate { get; set; }
        public string Notes { get; set; }
        public string BeforeImagePath { get; set; }
        public string AfterImagePath { get; set; }
        public int Rating { get; set; }
        public string Feedback { get; set; }
    }
}
