 
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib.Models
{
    public class AppointmentSessionCLS2
    {
        private   AccountModelsLib.operations opp;

        public AppointmentSessionCLS2()
        {
            opp = new AccountModelsLib.operations();
        }
        public AppointmentSessionCLS2(AccountModelsLib.operations operationsInstance)
        {
            opp = operationsInstance;
        }

        #region Load Data
        public List<Customer> GetAllCustomers()
        {
            return opp.db.Customer
                .OrderBy(c => c.Name)
                .ToList();
        }

        public List<Employee> GetAllEmployees()
        {
            return opp.db.Employees
                .OrderBy(e => e.name)
                .ToList();
        }

        public List<AppointmentCustomerPackage> GetActiveCustomerPackages(long customerId)
        {
            List<AppointmentCustomerPackage> AppointmentCustomerPackages= new List<AppointmentCustomerPackage>();
            //AppointmentCustomerPackages= opp.db.AppointmentCustomerPackages
            //    .Include(p => p.AppointmentPackage)
            //    .Where(p => p.CustomerId == customerId & p.IsActive.Value & p.RemainingSessions > 0)
            //    .OrderBy(p => p.AppointmentPackage.Name_Package)
            //    .ToList();

            AppointmentCustomerPackages = opp.db.AppointmentCustomerPackages
        .Include(p => p.AppointmentPackage)
        .Where(p => p.CustomerId == customerId
                    && p.IsActive.Value
                    && p.RemainingSessions > 0)
        .OrderBy(p => p.AppointmentPackage.Name_Package)
        .ToList();
            return AppointmentCustomerPackages;
        }

        public List<Items> GetAllServices()
        {
            return opp.db.Item.Where(a=>a.Is_ItemOrService==2 & a.Active_flage==1)
                .OrderBy(s => s.Item_Name)
                .ToList();
        }
        #endregion

        #region Save Image
        public string SaveImage(string originalPath)
        {
            if (string.IsNullOrEmpty(originalPath) || !File.Exists(originalPath))
                return null;

            string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Uploads", "SessionsImages");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string fileName = DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_" + Guid.NewGuid() + Path.GetExtension(originalPath);
            string destPath = Path.Combine(folder, fileName);

            File.Copy(originalPath, destPath, true);

            return Path.Combine("Uploads", "SessionsImages", fileName).Replace("\\", "/");
        }
        #endregion

        #region Add New Session
       

        public void AddNewSession(AppointmentSessionVM model)
        {
           
            try
            {
                opp.BeginTransaction();

                // 1. Create Appointment
                var customer = opp.db.Customer.Find(model.CustomerId);
                if (customer == null)
                    throw new Exception("Customer not found.");

                AppointmentCLS appointmentCLS = new AppointmentCLS(ref opp);
                Appointment appointment = new Appointment();
                if (model.AppointmentId > 0)
                {
                      appointment = opp.db.Appointments
                        .Where(a => a.Id==model.AppointmentId).FirstOrDefault();
                }
               
                if (appointment == null)
                {
                    appointment = new Appointment
                    {
                        AppointmentNumber = appointmentCLS.GetNextAppointmentNumberForCurrentYear(),
                        CustomerPhone = "",
                        AppointmentDate = model.SessionDate,
                        AppointmentTime = model.SessionDate,
                        AppointmentNotes = model.Notes,
                        AppointmentDateTime = model.SessionDate,
                        Customer_Id = model.CustomerId,
                        CustomerName = customer.Name
                    };
                    opp.db.Appointments.Add(appointment);
                    opp.db.SaveChanges();
                }
                  
                // 2. Create Appointment Service
                var service = opp.db.AppointmentServices.Find(model.ServiceItem_Id);
                if (service == null)
                    throw new Exception("Service not found.");

                var appService = new AppointmentService
                {
                    AppointmentId = appointment.Id,
                    ServiceItem_Id = model.ServiceItem_Id??0,
                    ServiceItem_Name = service.ServiceItem_Name,
                    Qnty = 1,
                    S_Price = 0
                };
                opp.db.AppointmentServices.Add(appService);
                opp.db.SaveChanges();

                // 3. Create Appointment Session
                var session = new AppointmentSession
                {
                    AppointmentId = appointment.Id,
                    AppointmentServiceId = appService.Id,
                    ServiceItem_Id = model.ServiceItem_Id,
                    EmployeeId = model.EmployeeId,
                    SessionDate = model.SessionDate,
                    Notes = model.Notes ?? "",
                    BeforeImagePath = model.BeforeImagePath,
                    AfterImagePath = model.AfterImagePath,
                    DurationinMinutes = 0
                };
                opp.db.AppointmentSessions.Add(session);
                opp.db.SaveChanges();

                // 4. Update package sessions if from package
                if (model.IsFromPackage && model.AppointmentCustomerPackageId.HasValue)
                {
                    var pkgService = opp.db.AppointmentCustomerPackageServices
                        .FirstOrDefault(x =>
                            x.AppointmentCustomerPackageId == model.AppointmentCustomerPackageId.Value &&
                            x.ItemService_Id == model.ServiceItem_Id);

                    if (pkgService != null)
                    {
                        pkgService.UsedSessions += 1;
                        pkgService.RemainingSessions -= 1;
                        opp.db.SaveChanges();
                    }
                }

                opp.Commit();
            }
            catch
            {
                opp.Rollback();
                
            }
        }
        #endregion
    }
}
