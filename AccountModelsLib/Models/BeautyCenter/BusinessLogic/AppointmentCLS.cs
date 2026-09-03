using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
 
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 

namespace AccountModelsLib.Models
{
    public class AppointmentCLS
    {
        AccountModelsLib.operations opp;
        public AppointmentCLS()
        {
            opp = new AccountModelsLib.operations();

        }
        public AppointmentCLS(ref AccountModelsLib.operations opp_)
        {
            opp = opp_;
        }
        //public long Id { get; set; }
        //public long CustomerId { get; set; }
        //public DateTime AppointmentDate { get; set; }
        //public List<AppointmentService> AppointmentServices { get; set; }

        public long GetNextAppointmentNumberForCurrentYear()
        {
            var currentYear = DateTime.Now.Year;

            // احصل على أكبر رقم حجز للعام الحالي
            var maxNumber = opp.db.Appointments
                .Where(a => a.AppointmentDate.Value.Year == currentYear)
                .Select(a => a.AppointmentNumber)
                .DefaultIfEmpty(0)
                .Max();

            return maxNumber + 1;
        }


        public void LoadAppointmentDetails(ref int Excp, ref string Msg, long appointmentId, ref Appointment appointment, ref List<AppointmentService> gridData)
        {

            appointment = opp.db.Appointments
                             .Include(a => a.appointmentService).FirstOrDefault();


            if (appointment == null)
            {
                Excp = 1;
                  Msg="الحجز غير موجود." ;
                return;
            }

            var appointmentService = opp.db.AppointmentServices.Include(e => e.appointmentServiceEmployee).Where(a => a.AppointmentId == appointmentId).ToList();
            // جهّز قائمة للربط بالجريد
            gridData = new List<AppointmentService>();

            //foreach (var service in appointmentService)
            //{
            //    var row = new AppointmentServiceVM
            //    {
            //        ServiceItem_Id = service.ServiceItem_Id,
            //        ServiceItem_Name = service.Items.Item_Name,
            //        S_Price = service.S_Price ?? 0,
            //        Qnty = service.Qnty ?? 0,
            //        DurationMinutes = service.DurationMinutes ?? 0,
            //        AppointmentServiceNotes = service.AppointmentServiceNotes,
            //        appointmentServiceEmployee = service.appointmentServiceEmployee,
            //        //EmployeeNames = service.em,
            //        Employees = service.appointmentServiceEmployee.Select(e => e.Employee).ToList(),
            //    };

            //    gridData.Add(row);
            //}

            gridData = appointmentService;


        }


        public void AddOrUpdateAppointment(ref int Excp, ref string Msg, Appointment appointment)
        {
            opp = new AccountModelsLib.operations();
            opp.BeginTransaction();
            Excp = 0;
            Msg = "";
            try
            {
                Appointment existingAppointment = null;

                if (appointment.Id == 0)
                {
                    // إضافة جديدة
                    opp.db.Appointments.Add(appointment);
                }
                else
                {
                    // تعديل
                    existingAppointment = opp.db.Appointments
                        .Include(a => a.appointmentService.Select(s => s.appointmentServiceEmployee))
                        .FirstOrDefault(a => a.Id == appointment.Id);

                    if (existingAppointment == null)
                        throw new Exception("لم يتم العثور على الحجز المطلوب تعديله.");

                    // تحديث الخصائص العامة للحجز
                    opp.db.Entry(existingAppointment).CurrentValues.SetValues(appointment);
                }

                // تحديث أو إضافة الخدمات
                foreach (var newService in appointment.appointmentService)
                {
                    AppointmentService existingService = null;

                    if (newService.Id > 0)
                    {
                        existingService = opp.db.AppointmentServices
                            .Include(s => s.appointmentServiceEmployee)
                            .FirstOrDefault(s => s.Id == newService.Id && s.AppointmentId == appointment.Id);

                        if (existingService != null)
                        {
                            // تعديل بيانات الخدمة
                            opp.db.Entry(existingService).CurrentValues.SetValues(newService);
                        }
                    }

                    if (existingService == null)
                    {
                        // خدمة جديدة
                        newService.AppointmentId = appointment.Id;
                        opp.db.AppointmentServices.Add(newService);
                        existingService = newService;
                    }
                    // إضافة جلسة بتاريخ الحز فى حالة وجود جلسة واحدة للخدمة
                    if (newService != null)
                    {
                        var session = opp.db.AppointmentSessions
                           .FirstOrDefault(s => s.AppointmentServiceId == newService.Id && s.AppointmentId == appointment.Id);
                        if (session == null)
                        {
                            var newSession = new AppointmentSession
                            {
                                AppointmentId = appointment.Id,
                                AppointmentServiceId = newService.Id,
                                SessionDate = appointment.AppointmentDate, // أو التاريخ المطلوب
                                DurationinMinutes = newService.DurationMinutes ?? 0,
                                IsCompletePayed = 0, // أو القيمة المطلوبة
                                Status = 0, // أو الحالة المطلوبة   
                                StatusStr = "مفتوحة", // أو الحالة النصية المطلوبة  
                                EmployeeId = newService.appointmentServiceEmployee.FirstOrDefault()?.EmployeeId, // تعيين أول موظف مرتبط بالخدمة    
                                ServiceItem_Id = newService.ServiceItem_Id,
                                BeforeImagePath = null, // أو مسار الصورة المطلوبة  
                                AfterImagePath = null, // أو مسار الصورة المطلوبة   
                                Rating = 0, // أو التقييم المطلوب    
                                User_Id = Common.User_Id,
                                Branch_Id = Common.Branch_Id,
                                AppointmentCustomerPackageServiceId = 0, // أو معرف خدمة الحزمة المطلوبة
                                AppointmentCustomerPackageId = 0, // أو معرف حزمة العميل المطلوبة   


                                Notes = "جلسة أولى", // أو أي ملاحظات أخرى
                            };
                            opp.db.AppointmentSessions.Add(newSession);
                        }
                    }
                    // تحديث أو إضافة الموظفين المرتبطين بالخدمة
                    foreach (var newEmp in newService.appointmentServiceEmployee)
                    {
                        var exists = existingService.appointmentServiceEmployee
                            .FirstOrDefault(e => e.EmployeeId == newEmp.EmployeeId);

                        if (exists == null)
                        {
                            newEmp.AppointmentServiceId = existingService.Id;
                            opp.db.AppointmentServiceEmployees.Add(newEmp);
                        }
                        else
                        {
                            opp.db.Entry(exists).CurrentValues.SetValues(newEmp);
                        }
                    }

                    // حذف الموظفين الذين تم إزالتهم من الخدمة
                    var newEmpIds = newService.appointmentServiceEmployee.Select(e => e.EmployeeId).ToList();
                    var toRemove = existingService.appointmentServiceEmployee
                        .Where(e => !newEmpIds.Contains(e.EmployeeId)).ToList();

                    foreach (var emp in toRemove)
                    {
                        opp.db.AppointmentServiceEmployees.Remove(emp);
                    }
                }

                opp.db.SaveChanges();
                opp.Commit();
            }
            catch (Exception ex)
            {
                opp.Rollback();
                Excp = 1;
                Msg = "حدث خطأ أثناء الحفظ: " + ex.Message;
            }
        }
        public void AddOrUpdateAppointmentService(ref int Excp, ref string Msg, Appointment appointment, AppointmentService appointmentService)
        {

            Excp = 0;
            Msg = "";
            try
            {

                // تحديث أو إضافة الخدمات

                AppointmentService existingService = null;

                if (appointmentService.Id > 0)
                {
                    existingService = opp.db.AppointmentServices
                        .FirstOrDefault(s => s.Id == appointmentService.Id);

                    if (existingService != null)
                    {
                        // تعديل بيانات الخدمة
                        opp.db.Entry(existingService).CurrentValues.SetValues(appointmentService);
                    }
                }

                if (existingService == null)
                {
                    // خدمة جديدة
                    appointmentService.AppointmentId = appointment.Id;
                    opp.db.AppointmentServices.Add(appointmentService);
                    existingService = appointmentService;
                }




                opp.db.SaveChanges();

            }
            catch (Exception ex)
            {
                Excp = 1;
                Msg = "حدث خطأ أثناء الحفظ: " + ex.Message;
            }
        }
        public void DeleteAppointmentService(ref int Excp, ref string Msg, long AppointmentServiceId, ICollection<AppointmentServiceEmployee> appointmentServiceEmployee)
        {
            try
            {
                AppointmentService AppointmentService_ = opp.db.AppointmentServices.Find(AppointmentServiceId);
                if (AppointmentService_ == null)
                {
                    Msg = "الخدمة غير موجودة.";
                    Excp = 1;
                    return;
                }
                else if (AppointmentService_.IsProcessedAsInvoice == 1)
                {
                    Msg = "لا يمكن حذف هذه الخدمة لأنها تمت معالجتها كفاتورة.";
                    Excp = 1;
                    return;
                }


                opp.db.AppointmentServices.Remove(opp.db.AppointmentServices.Find(AppointmentServiceId));
                opp.db.AppointmentServiceEmployees.RemoveRange(appointmentServiceEmployee);
                opp.db.SaveChanges();
                Excp = 0;
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = "حدث خطأ أثناء الحذف: " + ee.Message;
            }
        }


        public void DeleteAllAppointmentService(ref int Excp, ref string Msg, BindingList<AppointmentService> servicesList)
        {
            StringBuilder messageBuilder = new StringBuilder();
            Excp = 0;
            string innerMsg = "";
            try
            {
                var servicesToDelete = servicesList.ToList(); // إنشاء نسخة للتكرار الآمن

                foreach (var service in servicesToDelete)
                {
                    if (service.Id != 0 && service.IsProcessedAsInvoice == 0)
                    {
                        DeleteAppointmentService(ref Excp, ref innerMsg, service.Id, service.appointmentServiceEmployee);
                        if (Excp == 0)
                        {
                            servicesList.Remove(service);
                            messageBuilder.AppendLine($"تم حذف الخدمة برقم {service.ServiceItem_Name} بنجاح.");
                        }
                        else
                        {
                            messageBuilder.AppendLine($"خطأ في حذف الخدمة برقم {service.ServiceItem_Name}: {innerMsg}");
                        }
                    }
                    else
                    {
                        messageBuilder.AppendLine($"لا يمكن حذف الخدمة برقم {service.ServiceItem_Name} لأنها تمت معالجتها كفاتورة.");
                    }

                    if (service.Id == 0)
                    {
                        servicesList.Remove(service);
                    }
                }

                Msg = messageBuilder.ToString();
            }
            catch (Exception ex)
            {
                Excp = 1;
                Msg = "حدث خطأ أثناء الحذف: " + ex.Message;
            }
        }

        public void DeleteAllAppointmentService2(ref int Excp, ref string Msg, BindingList<AppointmentService> servicesList)
        {
            try
            {
                foreach (var service in servicesList)
                {
                    if (service.Id != 0 | service.IsProcessedAsInvoice == 0)
                    {
                        DeleteAppointmentService(ref Excp, ref Msg, service.Id, service.appointmentServiceEmployee);
                    }
                    else
                    {
                        Msg = Msg + "لا يمكن حذف هذه الخدمة لأنها تمت معالجتها كفاتورة.";
                    }
                    servicesList.Remove(service);
                }

                Excp = 0;
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = "حدث خطأ أثناء الحذف: " + ee.Message;
            }
        }
        public void DeleteAppointment(ref int Excp, ref string Msg, long AppointmentId)
        {
            try
            {
                Excp = 0;
                Msg = "";

                // البحث عن الحجز
                var appointment = opp.db.Appointments
                    .Include(a => a.appointmentService)

                    .FirstOrDefault(a => a.Id == AppointmentId);

                if (appointment == null)
                {
                    Msg = "الحجز غير موجود.";
                    Excp = 1;
                    return;
                }

                var AppointmentServicesLst = opp.db.AppointmentServices.Include(a => a.appointmentServiceEmployee).Where(a => a.AppointmentId == AppointmentId).ToList();
                // التحقق من الخدمات والموظفين
                foreach (var service in AppointmentServicesLst)
                {
                    if (service.IsProcessedAsInvoice == 1)
                    {
                        Msg = "لا يمكن حذف الحجز لأن بعض الخدمات تم إصدار فاتورة لها.";
                        Excp = 1;
                        return;
                    }
                }

                // حذف الموظفين المرتبطين بالخدمات
                foreach (var service in AppointmentServicesLst)
                {
                    if (service.appointmentServiceEmployee != null && service.appointmentServiceEmployee.Any())
                    {
                        opp.db.AppointmentServiceEmployees.RemoveRange(service.appointmentServiceEmployee);
                    }
                }

                // حذف الخدمات المرتبطة
                if (AppointmentServicesLst != null && AppointmentServicesLst.Any())
                {
                    opp.db.AppointmentServices.RemoveRange(AppointmentServicesLst);
                }

                // حذف الحجز نفسه
                opp.db.Appointments.Remove(appointment);

                opp.db.SaveChanges();
                Msg = "تم حذف الحجز بنجاح.";
            }
            catch (Exception ex)
            {
                Excp = 1;
                Msg = "حدث خطأ أثناء حذف الحجز: " + ex.Message;
            }
        }


        public List<Appointment> GetAppointmentsByDate(DateTime selectedDate)
        {
            var appointments = opp.db.Appointments
    .Include("Customer")
    .Include("appointmentService.items")
    .Where(a =>  a.AppointmentDate.Value.Date == selectedDate.Date)
    .ToList();
            return appointments;
        }

        public List<Appointment> GetUpcomingAppointments()
        {
            return opp.db.Appointments
                .Include(a => a.appointmentService.Select(s => s.Items))
                .Include(a => a.Customer)
                .Where(a => a.AppointmentDate.Value.Date >= DateTime.Today.Date)
                .OrderBy(a => a.AppointmentDate)
                .ToList();
        }
    }
}