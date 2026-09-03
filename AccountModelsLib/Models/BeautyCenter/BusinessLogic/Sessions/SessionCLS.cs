using AccountModelsLib.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
 

using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 


namespace AccountModelsLib.Models
{
    /// <summary>
    /// الطبقة الوسطى لإدارة جلسات المواعيد (من باقة أو منفردة)
    /// - تتحقق من القيود (Validation)
    /// - تتعامل مباشرة مع opp.db
    /// </summary>
    public class SessionCLS
    {
        private   operations opp;
        public SessionCLS()
        {
            this.opp = new operations();
        }
        public SessionCLS(operations opp)
        {
            this.opp = opp ?? throw new ArgumentNullException(nameof(opp));
        }

        #region Helpers: Validation
        public void EnsureEmployeeSelected(long? employeeId)
        {
            if (!employeeId.HasValue || employeeId.Value <= 0)
                throw new InvalidOperationException("يجب اختيار موظف لتنفيذ الجلسة.");
        }

        public void EnsureNotPastDate(DateTime sessionDate, bool isEditMode)
        {
            var now = DateTime.Now;
            // يسمح بالتاريخ الماضي فقط عند تعديل جلسة محفوظة مسبقاً
            if (!isEditMode && sessionDate < now.AddMinutes(-5))
                throw new InvalidOperationException("لا يمكن تحديد تاريخ جلسة في الماضي.");
        }

        public void EnsureServiceSelected(long? serviceItemId)
        {
            if (!serviceItemId.HasValue || serviceItemId.Value <= 0)
                throw new InvalidOperationException("يجب اختيار الخدمة.");
        }
        #endregion

        #region Queries
        public AppointmentSession GetById(long id)
        {
            return opp.db.AppointmentSessions
                .Include(s => s.Appointment)
                .Include(s => s.Employee)
                .FirstOrDefault(s => s.Id == id);
        }

        public object GetActiveCustomerPackages(long customerId)
        {
            return opp.db.AppointmentCustomerPackages
                .Where(p => p.CustomerId == customerId && p.IsActive == true)
                .Select(p => new { p.Id, p.AppointmentPackage.Name_Package })
                .OrderBy(p => p.Name_Package)
                .ToList();
        }
        public List<AppointmentCustomerPackage> GetActiveCustomerPackagesAll(long customerId)
        {
            List<AppointmentCustomerPackage> AppointmentCustomerPackages = new List<AppointmentCustomerPackage>();
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
        public object GetPackageServices(long appointmentCustomerPackageId)
        {
            return opp.db.AppointmentCustomerPackageServices
                .Where(s => s.AppointmentCustomerPackageId == appointmentCustomerPackageId)
                .Select(s => new
                {
                    s.Id,
                    ServiceName = opp.db.Item.Where(i => i.id == s.ItemService_Id)
                                              .Select(i => i.Item_Name).FirstOrDefault(),
                    s.TotalSessions,
                    s.UsedSessions,
                    s.RemainingSessions
                })
                .OrderBy(s => s.ServiceName)
                .ToList();
        }

        public List<AppointmentVM> GetCustomerAppointments(long customerId)
        {
            var raw = opp.db.Appointments
                .Where(a => a.Customer_Id == customerId)
                .OrderByDescending(a => a.AppointmentDate)
                .Select(a => new AppointmentVM { Id = a.Id, AppointmentDate = a.AppointmentDate })
                .ToList();

            return raw.Select(a => new AppointmentVM
            {
                Id = a.Id,
                AppointmentDate = a.AppointmentDate,
                AppointmentInfo = $"{a.AppointmentDate:yyyy-MM-dd HH:mm}"
            }).ToList();
        }

        public object GetEmployees()
        {
            return opp.db.Employees
                .Select(e => new { e.Id, e.name })
                .OrderBy(e => e.name)
                .ToList();
        }
        #endregion

        #region Package Balance
        public bool HasRemainingSessions(long packageServiceId)
        { int RemainingSessions = 0;
            var svc = opp.db.AppointmentCustomerPackageServices.FirstOrDefault(x => x.Id == packageServiceId);
            if(svc!=null)
            {
                  RemainingSessions = svc.RemainingSessions;
            }

            return svc != null && svc.RemainingSessions > 0;
        }

        private void DecrementPackageOneSession(long packageServiceId)
        {
            var svc = opp.db.AppointmentCustomerPackageServices.FirstOrDefault(x => x.Id == packageServiceId);
            if (svc == null) throw new InvalidOperationException("خدمة الباقة غير موجودة.");
            if (svc.RemainingSessions <= 0) throw new InvalidOperationException("لا توجد جلسات متبقية لهذه الخدمة.");

            svc.UsedSessions += 1;
            svc.RemainingSessions -= 1;
            svc.UpdatedAt = DateTime.Now;
        }
        #endregion

        #region Image Handling
        /// <summary>
        /// ينسخ الصورة إلى مجلد ثابت ضمن المشروع: Images/Sessions/{customerId}/
        /// ويعيد المسار النهائي المحفوظ في قاعدة البيانات.
        /// </summary>
        public string SaveImageForCustomer(long customerId, string sourcePath)
        {
            if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
                return null;

            var baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "Sessions", customerId.ToString());
            if (!Directory.Exists(baseDir)) Directory.CreateDirectory(baseDir);

            var ext = Path.GetExtension(sourcePath);
            var fileName = $"{DateTime.Now:yyyyMMdd_HHmmssfff}{ext}";
            var destPath = Path.Combine(baseDir, fileName);

            File.Copy(sourcePath, destPath, overwrite: false);
            return destPath;
        }
        #endregion

        #region Commands
        /// <summary>
        /// إضافة جلسة من باقة (مع خصم جلسة)
        /// </summary>
        public long AddFromPackage(
            long packageServiceId,
            long customerId,
            long? appointmentId,
            DateTime sessionDate,
            long? employeeId,
            string notes,
            string beforeImagePath,
            string afterImagePath)
        {
            EnsureEmployeeSelected(employeeId);
            EnsureNotPastDate(sessionDate, isEditMode: false);

            var pkgSvc = opp.db.AppointmentCustomerPackageServices.FirstOrDefault(s => s.Id == packageServiceId);
            if (pkgSvc == null) throw new InvalidOperationException("الخدمة غير موجودة ضمن باقة العميل.");
            if (pkgSvc.Customer_Id != customerId) throw new InvalidOperationException("الخدمة المختارة لا تخص هذا العميل.");
            if (pkgSvc.RemainingSessions <= 0) throw new InvalidOperationException("لا توجد جلسات متبقية لهذه الخدمة.");

            // حفظ الصور (إن وجدت)
            var beforePath = SaveImageForCustomer(customerId, beforeImagePath);
            var afterPath = SaveImageForCustomer(customerId, afterImagePath);

            var entity = new AppointmentSession
            {
                AppointmentId = appointmentId,          // قد يكون null (تسجيل مباشر)
                AppointmentServiceId = null,
                ServiceItem_Id = pkgSvc.ItemService_Id, // الخدمة نفسها من الباقة
             
                DurationinMinutes = null,
                SessionDate = sessionDate,
               
                
                Notes = notes,
                BeforeImagePath = beforePath,
                AfterImagePath = afterPath,
                Status = 1,
                StatusStr = "مسجّلة",
                EmployeeNames = null,
                EmployeeId = employeeId,
                Rating = null,
                Feedback = null,
                IsCompletePayed = 1,                    // مدفوعة ضمن الباقة
                User_Id = Common.User_Id,
                Branch_Id = Common.Branch_Id,
            };

            opp.db.AppointmentSessions.Add(entity);

            // خصم جلسة واحدة من رصيد الباقة
            DecrementPackageOneSession(packageServiceId);

            opp.db.SaveChanges();
            return entity.Id;
        }

        /// <summary>
        /// إضافة جلسة منفردة (غير باقة)
        /// </summary>
        public long AddStandalone(
            long customerId,
            long? appointmentId,
            long serviceItemId,
            DateTime sessionDate,
            long? employeeId,
            string notes,
            string beforeImagePath,
            string afterImagePath,
            bool markPaid)
        {
            EnsureEmployeeSelected(employeeId);
            EnsureNotPastDate(sessionDate, isEditMode: false);
            EnsureServiceSelected(serviceItemId);

            // حفظ الصور (إن وجدت)
            var beforePath = SaveImageForCustomer(customerId, beforeImagePath);
            var afterPath = SaveImageForCustomer(customerId, afterImagePath);

            var entity = new AppointmentSession
            {
                AppointmentId = appointmentId,   // يفضل وجود موعد للجلسات المنفردة
                AppointmentServiceId = null,
                ServiceItem_Id = serviceItemId,
                EmployeeId = employeeId,
                DurationinMinutes = null,
                SessionDate = sessionDate,
                Status = 1,
                StatusStr = "مسجّلة",
                EmployeeNames = null,
                Notes = notes,
                BeforeImagePath = beforePath,
                AfterImagePath = afterPath,
                Rating = null,
                Feedback = null,
                IsCompletePayed = markPaid ? 1 : 0,
                User_Id = Common.User_Id,
                Branch_Id = Common.Branch_Id,
            };

            opp.db.AppointmentSessions.Add(entity);
            opp.db.SaveChanges();

            return entity.Id;
        }

        /// <summary>
        /// تحديث تفاصيل الجلسة (لا يغيّر أرصدة الباقات)
        /// </summary>
        public void UpdateSession(
            long sessionId,
            DateTime sessionDate,
            long? employeeId,
            string notes,
            string beforeImagePath,
            string afterImagePath,
            bool isFromPackage,
            long customerId)
        {
            EnsureEmployeeSelected(employeeId);
            EnsureNotPastDate(sessionDate, isEditMode: true);

            var dbEntity = opp.db.AppointmentSessions.FirstOrDefault(s => s.Id == sessionId);
            if (dbEntity == null) throw new InvalidOperationException("الجلسة غير موجودة.");

            // تحديث الصور إذا أُدخلت مسارات جديدة
            var beforePath = string.IsNullOrWhiteSpace(beforeImagePath) ? dbEntity.BeforeImagePath
                                                                        : SaveImageForCustomer(customerId, beforeImagePath);
            var afterPath = string.IsNullOrWhiteSpace(afterImagePath) ? dbEntity.AfterImagePath
                                                                        : SaveImageForCustomer(customerId, afterImagePath);

            dbEntity.SessionDate = sessionDate;
            dbEntity.EmployeeId = employeeId;
            dbEntity.Notes = notes;
            dbEntity.BeforeImagePath = beforePath;
            dbEntity.AfterImagePath = afterPath;

            // لا تغيّر IsCompletePayed هنا، ولا الأرصدة
            opp.db.SaveChanges();
        }

        /// <summary>
        /// إنشاء فاتورة لجلسة منفردة وتعليمها كمدفوعة
        /// </summary>
        public long CreateInvoiceForStandalone(long sessionId, decimal amount)
        {
            var session = opp.db.AppointmentSessions
                               .Include(s => s.Appointment)
                               .FirstOrDefault(s => s.Id == sessionId);
            if (session == null) throw new InvalidOperationException("الجلسة غير موجودة.");
            if (session.IsCompletePayed == 1) throw new InvalidOperationException("الجلسة مسددة بالفعل.");
            if (session.Appointment == null) throw new InvalidOperationException("لا يمكن إنشاء فاتورة دون موعد يحدد العميل.");

            var invoice = new Invoices_List
            {
                Customer_Id = session.Appointment.Customer_Id,
                Net_Total = amount,
                //CreatedAt = DateTime.Now,
                //UpdatedAt = null,
                //Status = 1
            };

            opp.db.Invoices_List.Add(invoice);
            session.IsCompletePayed = 1;

            opp.db.SaveChanges();
            return invoice.Id;
        }
        #endregion

        public List<Customer> GetAllCustomers()
        {
            return opp.db.Customer
                .OrderBy(c => c.Name)
                .ToList();
        }
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


        public void AddNewSession(ref int Excp, ref string Msg, AppointmentSessionVM model)
        {

            try
            {
                opp.BeginTransaction();

                // 1. Create Appointment
                var customer = opp.db.Customer.Find(model.CustomerId);
                if (customer == null)
                    throw new Exception("Customer not found.");

                AppointmentCLS appointmentCLS = new AppointmentCLS(ref opp);
                Appointment appointment  =null;
                if (model.AppointmentId > 0)
                {
                      appointment = opp.db.Appointments.Find(model.AppointmentId);
                     
                }

                if (appointment == null | model.AppointmentId==0)
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
                AppointmentService appointmentServices = opp.db.AppointmentServices.Where(i => i.ServiceItem_Id == model.ServiceItem_Id & i.AppointmentId == appointment.Id).FirstOrDefault();
                if (appointmentServices == null)
                {

                    appointmentServices = new AppointmentService
                    {
                        AppointmentId = appointment.Id,
                        ServiceItem_Id = model.ServiceItem_Id ?? 0,
                        ServiceItem_Name = model.ServiceItem_Name,
                        Qnty = 1,
                        S_Price = 0
                    };
                    opp.db.AppointmentServices.Add(appointmentServices);
                    opp.db.SaveChanges();
                }

               
                // 3. Create Appointment Session
                var session = new AppointmentSession
                {
                    AppointmentId = appointment.Id,
                    AppointmentServiceId = appointmentServices.Id,
                    ServiceItem_Id = model.ServiceItem_Id,
                    EmployeeId = model.EmployeeId,
                    SessionDate = model.SessionDate,
                    Notes = model.Notes ?? "",
                    BeforeImagePath = model.BeforeImagePath,
                    AfterImagePath = model.AfterImagePath,
                    DurationinMinutes = model.DurationinMinutes,
                    Status = 1,
                    StatusStr = "مسجّلة",
                    EmployeeNames = null,
                    //EmployeeId = employeeId,
                    Rating = null,
                    Feedback = null,
                    IsCompletePayed = model.IsCompletePayed,                     // مدفوعة ضمن الباقة
                    User_Id = Common.User_Id,
                    Branch_Id = Common.Branch_Id,
                };
                if (model.AppointmentSessionId > 0)
                {
                    AppointmentSession appointmentSession = opp.db.AppointmentSessions.Where(i => i.Id == model.AppointmentSessionId).FirstOrDefault();
                     
                    // تحديث الصور إذا أُدخلت مسارات جديدة
                    session.BeforeImagePath  = string.IsNullOrWhiteSpace(model.BeforeImagePath) ? appointmentSession.BeforeImagePath
                                                                        : SaveImageForCustomer(model.CustomerId , model.BeforeImagePath);
                    session.AfterImagePath = string.IsNullOrWhiteSpace(model.AfterImagePath) ? appointmentSession.AfterImagePath
                                                                        : SaveImageForCustomer(model.CustomerId, model.AfterImagePath);

            
                    opp.db.Entry(appointmentSession).CurrentValues.SetValues(session);
                }
                else
                {
                    // تحديث الصور إذا أُدخلت مسارات جديدة
                    session.BeforeImagePath = string.IsNullOrWhiteSpace(model.BeforeImagePath) ? model.BeforeImagePath
                                                                        : SaveImageForCustomer(model.CustomerId, model.BeforeImagePath);
                    session.AfterImagePath = string.IsNullOrWhiteSpace(model.AfterImagePath) ? model.AfterImagePath
                                                                        : SaveImageForCustomer(model.CustomerId, model.AfterImagePath);

                    opp.db.AppointmentSessions.Add(session);
                    opp.db.SaveChanges();
                }
                // 4. Update package AppointmentSessionEmployees
                Employee employee = opp.db.Employees.Find(model.EmployeeId);
                AppointmentSessionEmployee ase = new AppointmentSessionEmployee
                {
                    //public long AppointmentServiceId { get; set; }

                    EmployeeId = model.EmployeeId ?? 0,
                    ContributionPercentage = 0,
                    IsContributionPercentage = 0,
                    ContributionValue = 0,
                    AppointmentServiceEmployeeNotes = "أساسي",
                    AppointmentId=appointment.Id,
                    AppointmentServiceId=appointmentServices.Id,
                    AppointmentSessionId= session.Id,

                };
                AppointmentSessionEmployee appointmentServiceEmployee = opp.db.AppointmentSessionEmployees.Where(a => a.AppointmentSessionId == session.Id & a.EmployeeId == model.EmployeeId).FirstOrDefault();
                if (appointmentServiceEmployee == null)
                {
                    opp.db.AppointmentSessionEmployees.Add(ase);
                }
                // 5. Update package sessions if from package
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
                Excp = 0;
                opp.Commit();
            }
            catch (Exception ex)
            {
                Excp = 1;
                Msg = ex.Message;
                opp.Rollback();
            }

        }
        #endregion


        #region Load Data


        public List<Employee> GetAllEmployees()
        {
            return opp.db.Employees
                .OrderBy(e => e.name)
                .ToList();
        }



        public List<Items> GetAllServices()
        {
            return opp.db.Item.Where(a => a.Is_ItemOrService == 2 & a.Active_flage == 1)
                .OrderBy(s => s.Item_Name)
                .ToList();
        }
        #endregion
    }
}
/*

namespace AccountModelsLib.Models
{
    public class PackageSessionService
    {
        private readonly operations _opp;

        public PackageSessionService(operations opp)
        {
            _opp = opp;
        }

        public List<AppointmentCustomerPackage> GetCustomerPackages(long customerId)
        {
            return _opp.db.AppointmentCustomerPackages
                .Where(p => p.CustomerId == customerId && p.Status == 1)
                .ToList();
        }

        public List<AppointmentCustomerPackageService> GetPackageServices(long packageId)
        {
            return _opp.db.AppointmentCustomerPackageServices
                .Where(s => s.AppointmentCustomerPackageId == packageId)
                .ToList();
        }

        public bool AddSession(long serviceId, long? appointmentId, DateTime? sessionDate, long? employeeId, string notes = null)
        {
            var service = _opp.db.AppointmentCustomerPackageServices
                .FirstOrDefault(s => s.Id == serviceId);

            if (service == null)
                throw new Exception("الخدمة غير موجودة.");

            if (service.RemainingSessions <= 0)
                throw new Exception("لا توجد جلسات متبقية لهذه الخدمة.");

            var session = new AppointmentSession
            {
                ServiceItem_Id = service.ItemService_Id,
                AppointmentId = appointmentId,
                SessionDate = sessionDate ?? DateTime.Now,
                EmployeeId = employeeId,
                Status = 1,
                Notes = notes,
                User_Id = Common.User_Id,
                Branch_Id = Common.Branch_Id,
            };

            _opp.db.AppointmentSessions.Add(session);

            service.UsedSessions += 1;
            service.RemainingSessions -= 1;
            service.UpdatedAt = DateTime.Now;

            _opp.db.SaveChanges();

            return true;
        }
        public bool AddSession(
         long serviceId,
         long? appointmentId,
         DateTime? sessionDate,
         long? employeeId,
         string notes = null,
         string beforeImagePath = null,
         string afterImagePath = null)
        {
            var service = _opp.db.AppointmentCustomerPackageServices
                .FirstOrDefault(s => s.Id == serviceId);

            if (service == null)
                throw new Exception("الخدمة غير موجودة.");

            if (service.RemainingSessions <= 0)
                throw new Exception("لا توجد جلسات متبقية لهذه الخدمة.");

            var session = new AppointmentSession
            {
                ServiceItem_Id = service.ItemService_Id,
                AppointmentId = appointmentId,
                SessionDate = sessionDate ?? DateTime.Now,
                EmployeeId = employeeId,
                Status = 1,
                Notes = notes,
                BeforeImagePath = beforeImagePath,
                AfterImagePath = afterImagePath,
                User_Id = Common.User_Id,
                Branch_Id = Common.Branch_Id,
            };

            _opp.db.AppointmentSessions.Add(session);

            service.UsedSessions += 1;
            service.RemainingSessions -= 1;
            service.UpdatedAt = DateTime.Now;

            _opp.db.SaveChanges();

            return true;
        }
    }
}
*/
