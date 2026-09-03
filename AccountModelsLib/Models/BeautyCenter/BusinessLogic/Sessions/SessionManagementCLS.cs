using AccountModelsLib;
 
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using System.Linq;
namespace AccountModelsLib.Models
{
    public class SessionManagementCLS
    {
        private readonly operations opp;

        public SessionManagementCLS()
        {
            this.opp = new operations();
        }
        public SessionManagementCLS(operations opp)
        {
            this.opp = opp;
        }
        public AppointmentSession GetSessionById(long sessionId)
        {
            return opp.db.AppointmentSessions.FirstOrDefault(s => s.Id == sessionId);
        }
        // ✅ إضافة جلسة جديدة
        public void AddSession(AppointmentSession session)
        {
            opp.db.AppointmentSessions.Add(session);
            opp.db.SaveChanges();
        }

        // ✅ تعديل جلسة
        public void UpdateSession(AppointmentSession updatedSession)
        {
            var session = opp.db.AppointmentSessions.FirstOrDefault(s => s.Id == updatedSession.Id);
            if (session != null)
            {
                session.SessionDate = updatedSession.SessionDate;
                session.EmployeeNames = updatedSession.EmployeeNames;
                session.DurationinMinutes = updatedSession.DurationinMinutes;
                session.Notes = updatedSession.Notes;
                opp.db.SaveChanges();
            }
        }



        // ✅ حذف جلسة

        public int DeleteSession(long sessionId, ref string msg)
        {
            try
            {
                // جلب الجلسة المطلوبة
                var session = opp.db.AppointmentSessions.FirstOrDefault(s => s.Id == sessionId);
                if (session == null)
                {
                    msg = "لم يتم العثور على الجلسة.";
                    return -1;
                }

                // ✅ منع الحذف إذا كانت الجلسة مدفوعة بالكامل
                if (session.IsCompletePayed == 1)
                {
                    msg = "لا يمكن حذف جلسة تم دفعها بالكامل.";
                    return -1;
                }

                // ✅ تعيين IsCompletePayed = 0 قبل الحذف (احتياطيًا)
                //session.IsCompletePayed = 0;
                //opp.db.SaveChanges();

                // ✅ حذف جميع الموظفين المرتبطين بالجسلة
                var sessionEmployees = opp.db.AppointmentSessionEmployees
                                             .Where(se => se.AppointmentSessionId == sessionId)
                                             .ToList();
                if (sessionEmployees.Any())
                {
                    opp.db.AppointmentSessionEmployees.RemoveRange(sessionEmployees);
                }

                // ✅ حذف الجلسة نفسها
                opp.db.AppointmentSessions.Remove(session);
                opp.db.SaveChanges();

                msg = "تم حذف الجلسة بنجاح.";
                return 0;
            }
            catch (Exception ex)
            {
                msg = $"حدث خطأ أثناء الحذف: {ex.Message}";
                return -1;
            }
        }
        // ✅ جلب الجلسات الخاصة بحجز معين
        public List<AppointmentSession> GetSessionsByAppointment(long appointmentId)
        {
            return opp.db.AppointmentSessions
                        .Include(s => s.AppointmentService)
                        .Where(s => s.AppointmentId == appointmentId)
                        .ToList();
        }
        public AppointmentService LoadServiceSessions(long appointmentServiceId)
        {
            return opp.db.AppointmentServices
                                  .Include(s => s.Appointment)
                                  .FirstOrDefault(s => s.Id == appointmentServiceId);
        }


        public List<AppointmentSession> GetSessionsByService(long serviceId)
        {
            return opp.db.AppointmentSessions
                         .Include(s => s.AppointmentService)
                         .Where(s => s.AppointmentServiceId == serviceId)
                         .ToList();
        }
        public List<AppointmentSessionVM> GetAppointmentSessionVM(long serviceId)
        {
             

            var sessionVMList = opp.db.AppointmentSessions
    .Include(s => s.AppointmentService)
    .Include(s => s.Items).Where(s => s.AppointmentServiceId == serviceId)
    .Select(s => new AppointmentSessionVM
    {
        Id = s.Id,
        AppointmentId = s.AppointmentId,
        AppointmentServiceId = s.AppointmentServiceId,
        ServiceItem_Id = s.ServiceItem_Id,
        EmployeeId = s.EmployeeId,
        DurationinMinutes = s.DurationinMinutes,
        SessionDate = s.SessionDate,
        Status = s.Status,
        EmployeeNames = s.EmployeeNames,
        Notes = s.Notes,
        BeforeImagePath = s.BeforeImagePath,
        AfterImagePath = s.AfterImagePath,
        Rating = s.Rating,
        Feedback = s.Feedback,
        IsCompletePayed = s.IsCompletePayed,

        // ✅ الحقول الإضافية (VM)
        StatusName = (s.Status == 0) ? "منتهية" :
                     (s.Status == 1) ? "قيد التنفيذ" :
                     "غير محدد",
        ServiceItem_Name = s.AppointmentService.ServiceItem_Name != null ? s.Items.Item_Name : ""
    })
    .ToList();

            return sessionVMList;
        }
        public List<AppointmentSessionVM> GetAppointmentSessionVM_(long serviceId)
        {



            var session = opp.db.AppointmentSessions
                         .Include(s => s.AppointmentService)
                         .Where(s => s.AppointmentServiceId == serviceId)
                         .ToList();

            List<AppointmentSessionVM> sessionVMList = new List<AppointmentSessionVM>();
            foreach (var item in session)
            {
                var sessionVM = MapToVM(item);
                if (sessionVM != null)
                {
                    sessionVMList.Add(sessionVM);
                }
            }
            

            return sessionVMList;
        }

        public AppointmentSessionVM MapToVM(AppointmentSession session)
        {
            return new AppointmentSessionVM
            {
                Id = session.Id,
                AppointmentId = session.AppointmentId,
                AppointmentServiceId = session.AppointmentServiceId,
                ServiceItem_Id = session.ServiceItem_Id,
                EmployeeId = session.EmployeeId,
                DurationinMinutes = session.DurationinMinutes,
                SessionDate = session.SessionDate,
                Status = session.Status,
                EmployeeNames = session.EmployeeNames,
                Notes = session.Notes,
                BeforeImagePath = session.BeforeImagePath,
                AfterImagePath = session.AfterImagePath,
                Rating = session.Rating,
                Feedback = session.Feedback,
                IsCompletePayed = session.IsCompletePayed,

                // ✅ الحقول الإضافية
                StatusName = (session.Status == 1) ? "منتهية" :
                             (session.Status ==0) ? "قيد التنفيذ" :
                             "غير محدد",
                ServiceItem_Name = session.Items != null ? session.Items.Item_Name : ""
            };
        }

        public List<long> GetAppointmentSessionEmployeeIDs(long sessionId)
        {
            return opp.db.AppointmentSessionEmployees
                         .Where(se => se.Id == sessionId)
                         .Select(i => i.EmployeeId)
                         .ToList();
        }
        public List<Employee> GetEmployees()
        {
            return opp.db.Employees
                .Where(emp => emp.active == 1)

                .ToList();


        }// جلب الموظفين المرتبطين بجلسة
        public List<AppointmentSessionEmployee> GetAppointmentSessionEmployee_(long sessionId)
        {
            return opp.db.AppointmentSessionEmployees
                         .Where(se => se.Id == sessionId)
                         .ToList();
        }
        public List<EmployeeViewModel> GetAppointmentServiceEmployee(long appointmentServiceId, List<long> selectedEmployeeIds)
        {
            var allEmps = opp.db.AppointmentServiceEmployees
                   .Include(e => e.Employee)
                   .Where(e => e.AppointmentServiceId == appointmentServiceId)
                   .ToList();



            var employeeRows = allEmps.Select(emp =>
            {

                return new EmployeeViewModel
                {
                    IsSelect = selectedEmployeeIds.Contains(emp.Id),
                    Id = emp.Id,
                    name = emp.Employee.name,
                    JobName = "",
                    IsRatio = emp?.IsContributionPercentage ?? 0,
                    Ratio = emp?.ContributionPercentage ?? 0,
                    Value = emp?.ContributionValue ?? 0,
                    Notes = emp?.AppointmentServiceEmployeeNotes ?? ""
                };
            }).ToList();

            return employeeRows;
        }
        public List<EmployeeViewModel> GetAppointmentServiceEmployee(long appointmentServiceId)
        {
            var allEmployees = GetEmployees();

            var allEmpsFromService = opp.db.AppointmentServiceEmployees
                   .Include(e => e.Employee)
                   .Where(e => e.AppointmentServiceId == appointmentServiceId)
                   .ToList();



            var employeeRows = allEmployees.Select(emp =>
            {
                var selectedId = allEmpsFromService.FirstOrDefault(e => e.EmployeeId == emp.Id);
                if (selectedId != null)
                {
                    return new EmployeeViewModel
                    {
                        IsSelect = true,
                        Id = emp.Id,
                        name = emp.name,
                        JobName = "",
                        IsRatio = selectedId?.IsContributionPercentage ?? 0,
                        Ratio = selectedId?.ContributionPercentage ?? 0,
                        Value = selectedId?.ContributionValue ?? 0,
                        Notes = selectedId?.AppointmentServiceEmployeeNotes ?? ""
                    };
                }
                else
                {
                    return new EmployeeViewModel
                    {
                        IsSelect = false,
                        Id = emp.Id,
                        name = emp.name,
                        JobName = "",
                        IsRatio = 0,
                        Ratio = 0,
                        Value = 0,
                        Notes = ""
                    };
                }

            }).ToList();

            return employeeRows;
        }
        public List<EmployeeViewModel> GetAppointmentSessionEmployee(long sessionId)
        {
            var allEmployees = GetEmployees();

            var allEmpsFromSession = opp.db.AppointmentSessionEmployees
                   .Include(e => e.Employee)
                   .Where(e => e.AppointmentSessionId == sessionId)
                   .ToList();

            //var selectedEmployeeIds = GetAppointmentSessionEmployeeIDs(sessionId);

            var employeeRows = allEmployees.Select(emp =>
            {
                var selectedId = allEmpsFromSession.FirstOrDefault(e => e.EmployeeId == emp.Id);
                if (selectedId != null)
                {
                    return new EmployeeViewModel
                    {
                        IsSelect = true,
                        Id = emp.Id,
                        name = emp.name,
                        JobName = "",
                        IsRatio = selectedId?.IsContributionPercentage ?? 0,
                        Ratio = selectedId?.ContributionPercentage ?? 0,
                        Value = selectedId?.ContributionValue ?? 0,
                        Notes = selectedId?.AppointmentServiceEmployeeNotes ?? ""
                    };
                }
                else
                {
                    return new EmployeeViewModel
                    {
                        IsSelect = false,
                        Id = emp.Id,
                        name = emp.name,
                        JobName = "",
                        IsRatio = 0,
                        Ratio = 0,
                        Value = 0,
                        Notes = ""
                    };
                }

            }).ToList();

            return employeeRows;
        }
        public List<AppointmentServiceEmployee> GetAppointmentServiceEmployees(long appointmentServiceId)
        {
            return opp.db.AppointmentServiceEmployees
                   .Include(e => e.Employee)
                   .Where(e => e.AppointmentServiceId == appointmentServiceId)
                   .ToList();
        }
        public List<long> GetAppointmentServiceEmployeesIDs(long appointmentServiceId)
        {
            return opp.db.AppointmentServiceEmployees
                   .Include(e => e.Employee)
                   .Where(e => e.AppointmentServiceId == appointmentServiceId)
                   .Select(i => i.EmployeeId)
                   .ToList();

        }


        // ✅ إنشاء جلسة جديدة وربطها بموعد وخدمة
        public AppointmentSession CreateNewSession(long appointmentId, long serviceId, DateTime sessionDate, int duration, string notes)
        {
            var session = new AppointmentSession
            {
                AppointmentId = appointmentId,
                AppointmentServiceId = serviceId,
                SessionDate = sessionDate,
                DurationinMinutes = duration,
                Notes = notes,

            };

            opp.db.AppointmentSessions.Add(session);
            opp.db.SaveChanges();

            return session;
        }


        public void AddSessionWithEmployees(AppointmentSession session, List<long> selectedEmployeeIds)
        {
            if (session.Id == 0)
            {
                // ✅ إضافة الجلسة أولاً
                opp.db.AppointmentSessions.Add(session);
                opp.db.SaveChanges();
            }
            // ✅ إضافة الموظفين للجلسة مع منع التكرار
            foreach (var empId in selectedEmployeeIds.Distinct()) // استخدام Distinct لتجنب التكرار
            {
                bool exists = opp.db.AppointmentSessionEmployees
                    .Any(se => se.AppointmentSessionId == session.Id && se.EmployeeId == empId);

                if (!exists)
                {
                    opp.db.AppointmentSessionEmployees.Add(new AppointmentSessionEmployee
                    {
                        AppointmentSessionId = session.Id,
                        AppointmentId = session.AppointmentId,
                        AppointmentServiceId = session.AppointmentServiceId,

                        EmployeeId = empId
                    });
                }
            }

            opp.db.SaveChanges();
        }
        public void AddSessionWithEmployees(AppointmentSession session, List<EmployeeViewModel> selectedEmployees)
        {
            if (session.Id == 0)
            {
                // إضافة الجلسة
                opp.db.AppointmentSessions.Add(session);
                opp.db.SaveChanges();
            }
            // جلب الموظفين الموجودين مسبقاً للجلسة (احتياطيًا للتحقق)
            var existingEmployees = opp.db.AppointmentSessionEmployees
                .Where(se => se.AppointmentSessionId == session.Id)
                .Select(se => se.EmployeeId)
                .ToList();

            // ربط الموظفين بالجلسة مع التحقق من التكرار
            foreach (var emp in selectedEmployees)
            {
                if (!existingEmployees.Contains(emp.Id))
                {
                    opp.db.AppointmentSessionEmployees.Add(new AppointmentSessionEmployee
                    {
                        AppointmentSessionId = session.Id,

                        EmployeeId = emp.Id,

                        AppointmentId = session.AppointmentId,
                        AppointmentServiceId = session.AppointmentServiceId,
                        IsContributionPercentage = emp.IsRatio,
                        ContributionPercentage = emp.Ratio,
                        ContributionValue = emp.Value,
                        AppointmentServiceEmployeeNotes = emp.Notes,
                        IsCompletePayed = 0
                    });
                }

            }

            // ربط الموظفين بالجلسة مع التحقق من التكرار
            foreach (var emp in selectedEmployees)
            {

                if (existingEmployees.Contains(emp.Id))
                {
                    AppointmentSessionEmployee ap = new AppointmentSessionEmployee
                    {
                        AppointmentSessionId = session.Id,

                        EmployeeId = emp.Id,

                        AppointmentId = session.AppointmentId,
                        AppointmentServiceId = session.AppointmentServiceId,
                        IsContributionPercentage = emp.IsRatio,
                        ContributionPercentage = emp.Ratio,
                        ContributionValue = emp.Value,
                        AppointmentServiceEmployeeNotes = emp.Notes,
                        IsCompletePayed = 0
                    };
                }

            }
            opp.db.SaveChanges();
        }


        public void AddUpsertSessionEmployees(AppointmentSession session, List<EmployeeViewModel> selectedEmployees)
        {
            // ✅ إذا كانت الجلسة جديدة أضفها أولاً
            if (session.Id == 0)
            {
                opp.db.AppointmentSessions.Add(session);
                opp.db.SaveChanges();
            }
            else
            {
                // تحديث بيانات الجلسة إذا كانت موجودة
                var existingSession = opp.db.AppointmentSessions.FirstOrDefault(s => s.Id == session.Id);
                if (existingSession != null)
                {
                    opp.db.Entry(existingSession).CurrentValues.SetValues(session);
                    opp.db.SaveChanges();
                }
            }

            // ✅ جلب الموظفين الحاليين المرتبطين بالجلسة
            //var existingEmployees = opp.db.AppointmentSessionEmployees
            //    .Where(se => se.AppointmentSessionId == session.Id)
            //    .ToList();



            // ✅  جلب الموظفين الحاليين المرتبطين بالجلسة وحذفهم
            var existingEmployees = opp.db.AppointmentSessionEmployees
                .Where(se => se.AppointmentSessionId == session.Id)
                .ToList();

            if (existingEmployees.Any())
            {
                opp.db.AppointmentSessionEmployees.RemoveRange(existingEmployees);
                opp.db.SaveChanges();
            }

            // ✅ إضافة أو تحديث الموظفين
            foreach (var emp in selectedEmployees)
            {
                 
                    // ➕ إضافة موظف جديد
                    opp.db.AppointmentSessionEmployees.Add(new AppointmentSessionEmployee
                    {
                        AppointmentSessionId = session.Id,
                        EmployeeId = emp.Id,

                        AppointmentId = session.AppointmentId,
                        AppointmentServiceId = session.AppointmentServiceId,
                        IsContributionPercentage = emp.IsRatio,
                        ContributionPercentage = emp.Ratio,
                        ContributionValue = emp.Value,
                        AppointmentServiceEmployeeNotes = emp.Notes,
                        IsCompletePayed = 0
                        // أمثلة لحقول إضافية إن وُجدت
                        // Role = emp.Role,
                        // Commission = emp.Commission
                    });
                 
            }

            //// ✅ إزالة الموظفين غير المحددين (تم إلغاء اختيارهم)
            //var employeesToRemove = existingEmployees
            //    .Where(e => !selectedEmployees.Any(se => se.Id == e.EmployeeId))
            //    .ToList();

            //if (employeesToRemove.Any())
            //{
            //    opp.db.AppointmentSessionEmployees.RemoveRange(employeesToRemove);
            //}

            // حفظ جميع التغييرات
            opp.db.SaveChanges();
        }

        public void UpdateSessionWithEmployees(AppointmentSession session, List<long> selectedEmployeeIds)
        {

            // تحديث بيانات الجلسة
            var findSession = opp.db.AppointmentSessions.FirstOrDefault(s => s.Id == session.Id);
            if (findSession != null)
            {
                opp.db.Entry(findSession).CurrentValues.SetValues(session);
                opp.db.SaveChanges();
            }

            // جلب الموظفين الحاليين المرتبطين بالجسة
            var existingEmployees = opp.db.AppointmentSessionEmployees
                .Where(se => se.AppointmentSessionId == session.Id)
                .ToList();

            // الموظفين الحاليين (IDs)
            var existingEmployeeIds = existingEmployees.Select(e => e.EmployeeId).ToList();

            // ✅ الموظفين الذين يجب إضافتهم (غير موجودين حالياً)
            var employeesToAdd = selectedEmployeeIds.Except(existingEmployeeIds).ToList();

            foreach (var empId in employeesToAdd)
            {
                opp.db.AppointmentSessionEmployees.Add(new AppointmentSessionEmployee
                {
                    AppointmentSessionId = session.Id,
                    EmployeeId = empId
                });
            }

            // ✅ الموظفين الذين يجب إزالتهم (تم إلغاء اختيارهم)
            var employeesToRemove = existingEmployees.Where(e => !selectedEmployeeIds.Contains(e.EmployeeId)).ToList();

            if (employeesToRemove.Any())
            {
                opp.db.AppointmentSessionEmployees.RemoveRange(employeesToRemove);
            }

            opp.db.SaveChanges();
        }


        public void UpdateSessionWithEmployees(AppointmentSession session, List<EmployeeViewModel> selectedEmployees)
        {
            // تحديث بيانات الجلسة
            var findSession = opp.db.AppointmentSessions.FirstOrDefault(s => s.Id == session.Id);
            if (findSession != null)
            {
                opp.db.Entry(findSession).CurrentValues.SetValues(session);
                opp.db.SaveChanges();
            }

            // جلب الموظفين الحاليين المرتبطين بالجسة
            var existingEmployees = opp.db.AppointmentSessionEmployees
                .Where(se => se.AppointmentSessionId == session.Id)
                .ToList();

            // الموظفين الحاليين (IDs)
            var existingEmployeeIds = existingEmployees.Select(e => e.EmployeeId).ToList();

            // ✅ الموظفين الذين يجب إضافتهم (غير موجودين حاليًا)
            var employeesToAdd = selectedEmployees
                .Where(emp => !existingEmployeeIds.Contains(emp.Id)) // تجنب التكرار
                .ToList();

            foreach (var emp in employeesToAdd)
            {
                opp.db.AppointmentSessionEmployees.Add(new AppointmentSessionEmployee
                {
                    AppointmentSessionId = session.Id,
                    EmployeeId = emp.Id
                });
            }

            // ✅ الموظفين الذين يجب إزالتهم (تم إلغاء اختيارهم)
            var employeesToRemove = existingEmployees
                .Where(e => !selectedEmployees.Any(se => se.Id == e.EmployeeId))
                .ToList();

            if (employeesToRemove.Any())
            {
                opp.db.AppointmentSessionEmployees.RemoveRange(employeesToRemove);
            }

            opp.db.SaveChanges();
        }

        // ✅ تعديل الجلسة مع تحديث الموظفين (إضافة/إزالة الاختلاف فقط)
        public void UpdateSessionWithEmployees2(AppointmentSession session, List<long> newEmployeeIds)
        {
            // تحديث بيانات الجلسة
            var findSession = opp.db.AppointmentSessions.FirstOrDefault(s => s.Id == session.Id);
            if (findSession != null)
            {
                opp.db.Entry(findSession).CurrentValues.SetValues(session);
                opp.db.SaveChanges();
            }
            var currentEmployees = opp.db.AppointmentSessionEmployees
                                         .Where(se => se.AppointmentSessionId == session.Id)
                                         .ToList();

            // حذف الموظفين الذين لم يتم اختيارهم الآن
            foreach (var emp in currentEmployees)
            {
                if (!newEmployeeIds.Contains(emp.EmployeeId))
                    opp.db.AppointmentSessionEmployees.Remove(emp);
            }

            // إضافة الموظفين الجدد
            var currentEmpIds = currentEmployees.Select(e => e.EmployeeId).ToList();
            foreach (var empId in newEmployeeIds)
            {
                if (!currentEmpIds.Contains(empId))
                {
                    opp.db.AppointmentSessionEmployees.Add(new AppointmentSessionEmployee
                    {
                        AppointmentSessionId = session.Id,
                        EmployeeId = empId
                    });
                }
            }

            opp.db.SaveChanges();
        }




        // ✅ جلب الموظفين من AppointmentServiceEmployees (للإضافة)
        public List<EmployeeSelectionVM> GetEmployeesFromService(long serviceId)
        {
            return opp.db.AppointmentServiceEmployees
                         .Include(e => e.Employee)
                         .Where(e => e.AppointmentServiceId == serviceId)
                         .Select(e => new EmployeeSelectionVM
                         {
                             EmployeeId = e.EmployeeId,
                             EmployeeName = e.Employee.name,
                             IsSelected = true // لأنه تم اختياره عند الحجز
                         })
                         .ToList();
        }

        // ✅ جلب الموظفين من AppointmentSessionEmployees (للتعديل)
        public List<EmployeeSelectionVM> GetEmployeesFromSession(long sessionId)
        {
            var allEmployees = opp.db.Employees
                                     .Select(emp => new EmployeeSelectionVM
                                     {
                                         EmployeeId = emp.Id,
                                         EmployeeName = emp.name,
                                         IsSelected = false
                                     }).ToList();

            var selectedEmployees = opp.db.AppointmentSessionEmployees
                                          .Where(se => se.AppointmentSessionId == sessionId)
                                          .Select(se => se.EmployeeId)
                                          .ToList();

            // تحديد الموظفين المختارين
            foreach (var emp in allEmployees)
            {
                if (selectedEmployees.Contains(emp.EmployeeId))
                    emp.IsSelected = true;
            }

            return allEmployees;
        }

        // ✅ إضافة جلسة مع الموظفين
        public void AddSessionWithEmployees2(AppointmentSession session, List<long> employeeIds)
        {
            opp.db.AppointmentSessions.Add(session);
            opp.db.SaveChanges();

            foreach (var empId in employeeIds)
            {
                opp.db.AppointmentSessionEmployees.Add(new AppointmentSessionEmployee
                {
                    AppointmentSessionId = session.Id,
                    EmployeeId = empId
                });
            }

            opp.db.SaveChanges();
        }


        // ✅ جلب الموظفين المرتبطين بجلسة معينة
        public List<long> GetSessionEmployees(long sessionId)
        {
            return opp.db.AppointmentSessionEmployees
                         .Where(se => se.AppointmentSessionId == sessionId)
                         .Select(se => se.EmployeeId)
                         .ToList();
        }

        public AppointmentService GetAppointmentService(long serviceId)
        {
            return opp.db.AppointmentServices.Where(s => s.Id == serviceId).FirstOrDefault();
        }

        public List<EmployeeSelectionVM> GetAllEmployeesWithSelectionFromSession2(long sessionId)
        {
            // جميع الموظفين
            var allEmployees = opp.db.Employees
                .Select(e => new EmployeeSelectionVM
                {
                    EmployeeId = e.Id,
                    EmployeeName = e.name,
                    IsSelected = false
                }).ToList();

            // الموظفين المرتبطين بالجلسة
            var selectedIds = opp.db.AppointmentSessionEmployees
                .Where(a => a.AppointmentSessionId == sessionId)
                .Select(a => a.EmployeeId)
                .ToList();

            // تحديد الموظفين المختارين
            foreach (var emp in allEmployees)
            {
                emp.IsSelected = selectedIds.Contains(emp.EmployeeId);
            }

            return allEmployees;
        }

        public List<EmployeeViewModel> GetAllEmployeesWithSelectionFromSession(long sessionId)
        {
            List<EmployeeViewModel> empVm = new List<EmployeeViewModel>();
            var allEmployees = opp.db.Employees
                 .ToList();
            //EmployeeId = e.Id,
            //        EmployeeName = e.name,
            //        IsSelected = false
            var selectedIds = opp.db.AppointmentSessionEmployees.Include(e => e.Employee)
                .Where(a => a.AppointmentSessionId == sessionId)
                  .ToList();

            foreach (var emp in allEmployees)
            {

                AppointmentSessionEmployee selectedEmpSer = selectedIds.Where(a => a.EmployeeId == emp.Id).FirstOrDefault();
                EmployeeViewModel employeeVM;
                if (selectedEmpSer != null)
                {
                    employeeVM = new EmployeeViewModel
                    {
                        IsSelect = true,
                        Id = emp.Id,

                        name = emp.name,
                        JobName = "",
                        IsRatio = selectedEmpSer?.IsContributionPercentage ?? 0,
                        Ratio = selectedEmpSer?.ContributionPercentage ?? 0,
                        Value = selectedEmpSer?.ContributionValue ?? 0,
                        Notes = selectedEmpSer?.AppointmentServiceEmployeeNotes ?? ""
                    };
                }
                else
                {
                    employeeVM = new EmployeeViewModel
                    {
                        IsSelect = false,
                        Id = emp.Id,

                        name = emp.name,
                        JobName = "",
                        IsRatio = 0,
                        Ratio = 0,
                        Value = 0,
                        Notes = ""
                    };
                }

                empVm.Add(employeeVM);



            }

            return empVm;
        }

        public List<EmployeeViewModel> GetAllEmployeesWithSelectionFromService(long serviceId)
        {
            List<EmployeeViewModel> empVm = new List<EmployeeViewModel>();
           var allEmployees = opp.db.Employees
                .ToList();
            //EmployeeId = e.Id,
            //        EmployeeName = e.name,
            //        IsSelected = false
            var selectedIds = opp.db.AppointmentServiceEmployees.Include(e=>e.Employee)
                .Where(a => a.AppointmentServiceId == serviceId)
                  .ToList();

            foreach (var emp in allEmployees)
            {
                
                AppointmentServiceEmployee selectedEmpSer= selectedIds.Where(a => a.EmployeeId == emp.Id).FirstOrDefault();
                EmployeeViewModel employeeVM;
                if (selectedEmpSer != null)
                {
                    employeeVM = new EmployeeViewModel
                    {
                        IsSelect = true,
                        Id = selectedEmpSer.Id,

                        name = selectedEmpSer.Employee.name,
                        JobName = "",
                        IsRatio = selectedEmpSer?.IsContributionPercentage ?? 0,
                        Ratio = selectedEmpSer?.ContributionPercentage ?? 0,
                        Value = selectedEmpSer?.ContributionValue ?? 0,
                        Notes = selectedEmpSer?.AppointmentServiceEmployeeNotes ?? ""
                    };
                }
                else
                {
                    employeeVM = new EmployeeViewModel
                    {
                        IsSelect = false ,
                        Id = emp.Id,

                        name = emp.name,
                        JobName = "",
                        IsRatio =   0,
                        Ratio =   0,
                        Value =   0,
                        Notes =   ""
                    };
                }

                empVm.Add(employeeVM);

                 
             
            }

            return empVm;
        }
        public List<EmployeeSelectionVM> GetAllEmployeesWithSelectionFromService2(long serviceId)
        {
            var allEmployees = opp.db.Employees
                .Select(e => new EmployeeSelectionVM
                {
                    EmployeeId = e.Id,
                    EmployeeName = e.name,
                    IsSelected = false
                }).ToList();

            var selectedIds = opp.db.AppointmentServiceEmployees
                .Where(a => a.AppointmentServiceId == serviceId)
                .Select(a => a.EmployeeId)
                .ToList();

            foreach (var emp in allEmployees)
            {
                emp.IsSelected = selectedIds.Contains(emp.EmployeeId);
            }

            return allEmployees;
        }

    }
    public class EmployeeSelectionVM
    {
        public long EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public bool IsSelected { get; set; }
    }
}

