using AccountModelsLib.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
 

namespace AccountModelsLib.Services
{
    public class EmployeeService
    {
        private readonly operations _opp;

        public EmployeeService()
        {
            _opp = new operations();
        }

        public EmployeeService(operations opp)
        {
            _opp = opp;
        }

        /// <summary>
        /// جلب جميع الموظفين النشطين (Active = 1) بدون تتبع للمساعدة في الأداء والسلامة
        /// </summary>
        public async Task<List<Employee>> GetActiveEmployeesAsync()
        {
             
            try
            {
                List<Employee>  empList = await _opp.db.Employees
                                        .AsNoTracking()
                                        .Include(e => e.jobsList)
                                        .Where(e => e.active == 1 || e.active == null)
                                        .ToListAsync();

                return empList;
            }
            catch (Exception ee)
            {
                return new List<Employee>();
            }
             
        }

        /// <summary>
        /// جلب البيانات بطريقة عادية لخدمة شاشات WinForms التقليدية
        /// </summary>
        public List<Employee> GetActiveEmployees()
        {
            return _opp.db.Employees
                          .AsNoTracking()
                          .Include(e => e.jobsList)
                          .Where(e => e.active == 1 || e.active == null)
                          .ToList();
        }

        /// <summary>
        /// جلب الموظفين بحسب المعرف بصيغة ViewModel
        /// </summary>
        public async Task<List<EmployeeViewModel>> GetEmployeeListAsync(long empId)
        {
            IQueryable<Employee> query = _opp.db.Employees.AsNoTracking().Include(j => j.jobsList);

            if (empId > 0)
            {
                query = query.Where(i => i.Id == empId);
            }

            var employees = await query.ToListAsync();
            return MapToViewModel(employees);
        }

        /// <summary>
        /// جلب الموظفين بحسب الوظيفة
        /// </summary>
        public async Task<List<EmployeeViewModel>> GetEmployeeByJobAsync(long jobId)
        {
            var employees = await _opp.db.Employees
                                        .AsNoTracking()
                                        .Include(j => j.jobsList)
                                        .Where(e => e.jobsListId == jobId)
                                        .ToListAsync();

            return MapToViewModel(employees);
        }

        /// <summary>
        /// حفظ أو تحديث مجموعة موظفين قادمة من GridView أو Razor Form
        /// </summary>
        public async Task SaveRangeAsync1(List<Employee> employees)
        {
            try
            {
                foreach (var item in employees)
                {
                    item.active = 1;

                    if (item.Id == 0)
                    {
                        // توليد ID جديد باستخدام Snowflake
                        item.Id = CallSnowflakeIdGenerate.GenerateNewId();
                        await _opp.db.Employees.AddAsync(item);
                    }
                    else
                    {
                        var local = _opp.db.Employees.Local.FirstOrDefault(e => e.Id == item.Id);
                        if (local != null)
                        {
                            _opp.db.Entry(local).State = EntityState.Detached;
                        }

                        _opp.db.Employees.Update(item);
                    }
                }

                await _opp.db.SaveChangesAsync();
            }
            catch (Exception ee)
            {
                 
            }
        }
        public async Task SaveRangeAsync(List<Employee> employees)
        {
            try
            {
                // إنشاء سياق جديد ونظيف خصيصاً لعملية الحفظ ينتهي فور الخروج من الـ using
                using (var opp = new operations()) // أو new operations().db حسب طريقة تعريف كائن الـ DbContext لديك
                {
                    foreach (var item in employees)
                    {
                        item.active = 1;

                        if (item.Id == 0)
                        {
                            // 1. إضافة الموظف الجديد
                            item.Id = CallSnowflakeIdGenerate.GenerateNewId();
                            opp.db.Employees.Add(item);
                        }
                        else
                        {
                            // 2. تحديث الموظف فقط وتغيير حالته إلى Modified
                            // استخدام Entry يعلم EF Core بتحديث جدول الموظف فقط دون مساس الكائنات المرتبطة (مثل jobsList)
                            opp.db.Entry(item).State = EntityState.Modified;
                        }
                    }

                    // حفظ جميع التغييرات دفعة واحدة
                    await opp.db.SaveChangesAsync();
                }
            }
            catch (Exception ee)
            {
                // يُفضل دائماً عدم ترك الـ catch فارغة لمعرفة الأخطاء أثناء التطوير
                throw;
            }
        }
        /// <summary>
        /// الحذف المنطقي أو النهائي بناءً على إعدادات النظام
        /// </summary>
        public async Task DeleteEmployeeAsync(long id, long subAccountId)
        {
            var employee = await _opp.db.Employees.FirstOrDefaultAsync(e => e.Id == id || (subAccountId > 0 && e.SubAccount_Id == subAccountId));

            if (employee != null)
            {
                if (Common.Update_DeleteSupplier == 0)
                {
                    employee.active = 0; // Soft delete
                    _opp.db.Employees.Update(employee);
                }
                else
                {
                    _opp.db.Employees.Remove(employee); // Hard delete
                }

                await _opp.db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// إدراج أو تحديث بيانات الموظف المربوط بالحساب الفرعي
        /// </summary>
        public async Task<long> AddOrUpdateEmployeeFromSubAccountAsync(Submain_Account sb)
        {
            var emp = await _opp.db.Employees.FirstOrDefaultAsync(e => e.name == sb.SubAccount_Name || e.SubAccount_Id == sb.Id);

            if (emp == null)
            {
                emp = new Employee
                {
                    name = sb.SubAccount_Name,
                    SubAccount_Id = sb.Id,
                    Account_Code = sb.Account_Code,
                    active = 1
                };

                if (Common.Use_AutoNumeric_Employee_Id_With_Submain_Account_Id == 0 || Common.Use_Auto_Identity_Column == 0)
                {
                    emp.Id = sb.Id;
                }
                else
                {
                    emp.Id = CallSnowflakeIdGenerate.GenerateNewId();
                }

                await _opp.db.Employees.AddAsync(emp);
            }
            else
            {
                emp.name = sb.SubAccount_Name;
                emp.SubAccount_Id = sb.Id;
                emp.Account_Code = sb.Account_Code;
                _opp.db.Employees.Update(emp);
            }

            await _opp.db.SaveChangesAsync();
            return emp.Id;
        }

        // Helper mapper to produce ViewModel
        private List<EmployeeViewModel> MapToViewModel(IEnumerable<Employee> employees)
        {
            return employees.Select(emp => new EmployeeViewModel
            {
                Id = emp.Id,
                name = emp.name,
                jobsListId = emp.jobsListId,
                JobName = emp.jobsList != null ? emp.jobsList.jobname : string.Empty
            }).ToList();
        }
    }
}