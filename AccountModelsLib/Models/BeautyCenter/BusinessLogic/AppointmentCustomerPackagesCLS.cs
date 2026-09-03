using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using Microsoft.EntityFrameworkCore;
 
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 

namespace AccountModelsLib.Models
{

    /// <summary>
    /// Business layer for customer packages AccountModelsLib.operations.
    /// Uses AccountModelsLib.operations opp; and opp.db (Entity Framework).
    /// </summary>
    public class AppointmentCustomerPackagesCLS
    {
        public AccountModelsLib.operations opp;
        public AppointmentCustomerPackagesCLS( )
        {
            opp = new AccountModelsLib.operations();
        }
        public AppointmentCustomerPackagesCLS(AccountModelsLib.operations _opp)
        {
            opp = _opp ?? throw new ArgumentNullException(nameof(_opp));
        }

        /// <summary>
        /// جلب باقات العميل مع ملخص الخدمات (لا يحمل تفاصيل العمليات الثقيلة).
        /// Returns list of DTOs.
        /// </summary>
        public List<CustomerPackageDTO> GetCustomerPackages(long customerId, bool includeExpired = true)
        {
            var today = DateTime.Today;

            var query = opp.db.AppointmentCustomerPackages.AsQueryable();

            if (customerId > 0)
                query = query.Where(cp => cp.CustomerId == customerId);

            //if (!includeExpired)
            //    query = query.Where(cp => cp.EndDate == null || cp.EndDate >= today);

            var list = query
                .Select(cp => new CustomerPackageDTO
                {
                    Id = cp.Id,
                    AppointmentPackageId = cp.AppointmentPackageId,
                    CustomerId= cp.CustomerId,
                    Customer_Name = cp.Customer.Name,
                    Package_Name = cp.AppointmentPackage.Name_Package,
                    StartDate = cp.StartDate.Value,
                    EndDate = cp.EndDate.Value,
                    IsActive = cp.IsActive??false,
                    // إجمالي الجلسات = مجموع جلسات خدمات الباقة (من جدول AppointmentPackageServices)
                    TotalSessions = cp.AppointmentPackage.AppointmentPackageServices.Sum(s => (int?)s.TotalSessions) ?? 0,
                    // يمكن احتساب الجلسات المستخدمة من سجل جلسات العميل إن وجد (هنا نعرض قيمة افتراضية إن لم يوجد حقل)
                    UsedSessions = cp.RemainingSessions ?? 0
                })
                .ToList();

            return (List<CustomerPackageDTO>)list;
        }

        /// <summary>
        /// جلب تفاصيل خدمات باقة مُحددة (اسم الخدمة + عدد الجلسات في الباقة).
        /// </summary>
        public List<PackageServiceDetailDTO> GetPackageServicesDetails(long appointmentPackageId)
        {
            var list = opp.db.AppointmentPackageServices
                .Where(s => s.AppointmentPackageId == appointmentPackageId)
                .Select(s => new PackageServiceDetailDTO
                {
                    ItemService_Id = s.ItemServiceId,
                    ServiceName = opp.db.Item.Where(i => i.id == s.ItemServiceId)
                                              .Select(i => i.Item_Name).FirstOrDefault(),
                    SessionsCount = s.TotalSessions
                }).ToList();

            return list;
        }

        

        /// <summary>
        /// حساب قيمة فاتورة لعدة باقات (مثال بسيط: مجموع أسعار الباقات).
        /// يرجع المجموع (يجب تعديل ليتوافق مع منطق الفواتير لديك).
        /// </summary>
        public decimal CalculateInvoiceAmountForPackages(List<long> customerPackageIds)
        {
            var total = opp.db.AppointmentCustomerPackages
                .Where(cp => customerPackageIds.Contains(cp.Id))
                .Select(cp => cp.AppointmentPackage.Price)
                .DefaultIfEmpty(0)
                .Sum();

            return total;
        }

        /// <summary>
        /// Placeholder لإنشاء فاتورة فعلية — هنا نعيد رقم الفاتورة الافتراضي إن أجريت عملية.
        /// تحتاج ربطها بوحدة الفواتير الحقيقية في مشروعك.
        /// </summary>
        public long CreateInvoiceForPackages(long customerId, List<long> customerPackageIds, decimal amount)
        {
            // **ملاحظة**: هذا مكان لإنشاء سجل الفاتورة داخل جدول الفواتير لديك
            // إذا كان لديك جدول Invoices يمكنك هنا إنشاء السجل وربط Items/Lines بالباقات.
            // سنقوم بعمل عملية بسيطة: فقط نُعيد رقم وهمي أو 0 إذا لم يتم إنشاء فاتورة.
            // تعديل مقترح: قم بإنشاء سجل في opp.db.Invoices ثم احفظ وضع id وترجع id.
            //
            // مثال توضيحي (غير مفعل لأنه يعتمد على نموذج الفاتورة لديك):
            //
            // var inv = new Invoice { CustomerId = customerId, TotalAmount = amount, CreatedAt = DateTime.Now };
            // opp.db.Invoices.Add(inv);
            // opp.db.SaveChanges();
            // int invoiceId = inv.Id;
            // // ربط InvoiceId في AppointmentCustomerPackages إن رغبت:
            // var cps = opp.db.AppointmentCustomerPackages.Where(cp => customerPackageIds.Contains(cp.Id)).ToList();
            // foreach(var cp in cps) cp.InvoiceId = invoiceId;
            // opp.db.SaveChanges();
            //
            // هنا سنعيد قيمة 0 للدلالة على أن هذه الدالة تحتاج تنفيذ فعلي بحسب نموذجك.
            return 0;
        }

        public List<object> GetCustomerPackages(long customerId)
        {
            var list = opp.db.AppointmentCustomerPackages
                .Where(x => x.CustomerId == customerId)
                .Select(x => new
                {
                    x.Id,
                    PackageName = x.AppointmentPackage.Name_Package,
                    x.StartDate,
                    x.EndDate,
                    TotalSessions = x.AppointmentPackage.AppointmentPackageServices.Sum(s => s.TotalSessions),
                    Status = x.EndDate < DateTime.Now ? "منتهية" : "فعّالة"
                })
                .ToList<object>();

            return list;
        }

       


        public List<dynamic> GetPackagesForCustomer(long customerId)
        {
            return opp.db.AppointmentCustomerPackages
                .Where(p => p.CustomerId == customerId)
                .Select(p => new
                {
                    p.Id,
                    p.AppointmentPackageId,
                    PackageName = p.AppointmentPackage.Name_Package,
                    p.StartDate,
                    p.EndDate,
                    p.IsActive
                }).ToList<dynamic>();
        }

        public List<dynamic> GetAvailablePackages()
        {
            return opp.db.AppointmentPackages
                .Select(p => new
                {
                    p.Id,
                    p.Name_Package
                }).ToList<dynamic>();
        }

        public AppointmentCustomerPackage GetById(long id)
        {
            return opp.db.AppointmentCustomerPackages.FirstOrDefault(p => p.Id == id);
        }
         

        public void Save(AppointmentCustomerPackage entity)
        {
            if (entity.Id == 0)
            {
                opp.db.AppointmentCustomerPackages.Add(entity);
            }
            else
            {
                var existing = opp.db.AppointmentCustomerPackages.Find(entity.Id);
                if (existing != null)
                {
                    existing.AppointmentPackageId = entity.AppointmentPackageId;
                    existing.StartDate = entity.StartDate;
                    existing.EndDate = entity.EndDate;
                    existing.IsActive = entity.IsActive;
                }
            }
            opp.db.SaveChanges();
        }

         
       
        // 🔹 إضافة باقة جديدة للعميل
        public void Add_(AppointmentCustomerPackage package)
        {
            if (package == null)
                throw new ArgumentNullException(nameof(package));

            opp.db.AppointmentCustomerPackages.Add(package);
            opp.db.SaveChanges();
        }
        public void Add(AppointmentCustomerPackage customerPackage)
        {
            // 1- إضافة سجل الباقة للعميل
            opp.db.AppointmentCustomerPackages.Add(customerPackage);
            opp.db.SaveChanges();

            // 2- جلب جميع الخدمات من الباقة المختارة
            var packageServices = opp.db.AppointmentPackageServices
                .Where(s => s.AppointmentPackageId == customerPackage.AppointmentPackageId)
                .ToList();

            // 3- إدراج كل خدمة في جدول AppointmentCustomerPackageServices
            foreach (var service in packageServices)
            {
                var customerPackageService = new AppointmentCustomerPackageService
                {
                    AppointmentCustomerPackageId = customerPackage.Id,
                    AppointmentPackageId = customerPackage.AppointmentPackageId,
                    ItemService_Id = service.ItemServiceId,
                    Customer_Id = customerPackage.CustomerId,
                    StartDate = customerPackage.StartDate.Value,
                    EndDate = customerPackage.EndDate,
                    UsedSessions = 0,
                    RemainingSessions = service.TotalSessions, // من جدول الباقة
                    Status = 1, // نشطة
                    CreatedAt = DateTime.Now
                };

                opp.db.AppointmentCustomerPackageServices.Add(customerPackageService);
            }

            // 4- حفظ التغييرات
            opp.db.SaveChanges();
        }
        // 🔹 تعديل باقة العميل
        public void Update_(AppointmentCustomerPackage package)
        {
            if (package == null)
                throw new ArgumentNullException(nameof(package));

            var existing = opp.db.AppointmentCustomerPackages
                                 .FirstOrDefault(p => p.Id == package.Id);

            if (existing == null)
                throw new InvalidOperationException("Package not found");

            // تحديث البيانات
            existing.CustomerId = package.CustomerId;
            existing.AppointmentPackageId = package.AppointmentPackageId;
            existing.StartDate = package.StartDate;
            existing.EndDate = package.EndDate;
            existing.IsActive = package.IsActive;

            opp.db.SaveChanges();
        }
        public void Update(AppointmentCustomerPackage customerPackage)
        {
            var existingPackage = opp.db.AppointmentCustomerPackages
                .FirstOrDefault(p => p.Id == customerPackage.Id);

            if (existingPackage == null) return;

            // تحديث بيانات الباقة
            existingPackage.AppointmentPackageId = customerPackage.AppointmentPackageId;
            existingPackage.CustomerId = customerPackage.CustomerId;
            existingPackage.StartDate = customerPackage.StartDate;
            existingPackage.EndDate = customerPackage.EndDate;
            existingPackage.Status = customerPackage.Status;
            existingPackage.UpdatedAt = DateTime.Now;
            opp.db.SaveChanges();

            // جلب خدمات الباقة الجديدة من جدول AppointmentPackageServices
            var packageServices = opp.db.AppointmentPackageServices
                .Where(s => s.AppointmentPackageId == customerPackage.AppointmentPackageId)
                .ToList();

            // جلب خدمات الباقة القديمة للعميل
            var existingServices = opp.db.AppointmentCustomerPackageServices
                .Where(s => s.AppointmentCustomerPackageId == customerPackage.Id)
                .ToList();

            // إضافة أي خدمة جديدة
            foreach (var service in packageServices)
            {
                if (!existingServices.Any(es => es.ItemService_Id == service.ItemServiceId))
                {
                    opp.db.AppointmentCustomerPackageServices.Add(new AppointmentCustomerPackageService
                    {
                        AppointmentCustomerPackageId = existingPackage.Id,
                        AppointmentPackageId = existingPackage.AppointmentPackageId,
                        ItemService_Id = service.ItemServiceId,
                        Customer_Id = existingPackage.CustomerId,
                        StartDate = existingPackage.StartDate.Value,
                        EndDate = existingPackage.EndDate,
                        UsedSessions = 0,
                        RemainingSessions = service.TotalSessions,
                        Status = 1,
                        CreatedAt = DateTime.Now
                    });
                }
            }

            // تحديث أو حذف الخدمات القديمة
            foreach (var oldService in existingServices)
            {
                var newService = packageServices.FirstOrDefault(s => s.ItemServiceId == oldService.ItemService_Id);

                if (newService == null)
                {
                    // الخدمة لم تعد موجودة في الباقة الجديدة
                    if (oldService.UsedSessions == 0)
                    {
                        opp.db.AppointmentCustomerPackageServices.Remove(oldService);
                    }
                }
                else
                {
                    // تحديث عدد الجلسات المتبقية إذا كان أكبر من المستخدم
                    int minSessions = Math.Max(oldService.UsedSessions, newService.TotalSessions);
                    oldService.RemainingSessions = minSessions - oldService.UsedSessions;
                    oldService.UpdatedAt = DateTime.Now;
                }
            }

            opp.db.SaveChanges();
        }

        // 🔹 حذف باقة العميل
        public void Delete(long id)
        {
            var existing = opp.db.AppointmentCustomerPackages
                                 .FirstOrDefault(p => p.Id == id);

            if (existing == null)
                throw new InvalidOperationException("Package not found");

            opp.db.AppointmentCustomerPackages.Remove(existing);
            opp.db.SaveChanges();
        } /// <summary>
          /// حذف باقة العميل (يجب حذف أي سجلات مرتبطة حسب الحاجة).
          /// </summary>

        public bool DeleteCustomerPackage(long customerPackageId)
        {
            // جلب الباقة
            var package = opp.db.AppointmentCustomerPackages
                .FirstOrDefault(p => p.Id == customerPackageId);

            if (package == null)
                return false;

            // جلب الخدمات التابعة للباقة
            var services = opp.db.AppointmentCustomerPackageServices
                .Where(s => s.AppointmentCustomerPackageId == customerPackageId)
                .ToList();

            // التحقق إذا كان أي خدمة بها جلسات مستخدمة
            bool hasUsedSessions = services.Any(s => s.UsedSessions > 0);
            if (hasUsedSessions)
            {
                // لا يمكن الحذف إذا هناك جلسات مستخدمة
                throw new InvalidOperationException("لا يمكن حذف الباقة لوجود جلسات مسجلة لخدماتها.");
            }

            // حذف الخدمات
            opp.db.AppointmentCustomerPackageServices.RemoveRange(services);

            // حذف الباقة
            opp.db.AppointmentCustomerPackages.Remove(package);

            // حفظ التغييرات
            opp.db.SaveChanges();

            return true;
        }

        public bool DeleteCustomerPackage_(long id)
        {
            var pkg = opp.db.AppointmentCustomerPackages.FirstOrDefault(x => x.Id == id);
            if (pkg != null)
            {
                opp.db.AppointmentCustomerPackages.Remove(pkg);
                opp.db.SaveChanges();
                return true;
            }
            return false;
        }
        public bool DeleteServiceFromPackage(long packageId, long serviceId)
        {

            var entity = opp.db.AppointmentCustomerPackageServices
                .FirstOrDefault(s => s.AppointmentCustomerPackageId == packageId && s.ItemService_Id == serviceId);

            if (entity != null)
            {
                opp.db.AppointmentCustomerPackageServices.Remove(entity);
                opp.db.SaveChanges();
                return true;
            }
            return false;
        }

        // 🔹 جلب جميع الباقات الخاصة بعميل
        public List<AppointmentCustomerPackage> GetByCustomer(long customerId)
        {
            return opp.db.AppointmentCustomerPackages
                         .Where(p => p.CustomerId == customerId)
                         .ToList();
        }

        public long GetServiceIdByName(long packageId, string serviceName)
        {
            var service = (from s in  opp.db.AppointmentCustomerPackageServices
                           join item in  opp.db.Item on s.ItemService_Id equals item.id
                           where s.AppointmentPackageId == packageId
                           && item.Item_Name == serviceName
                           select s.ItemService_Id).FirstOrDefault();

            return service;
        }

        public DataTable GetServicesByPackageId(long packageId)
        {
         
            var query = from s in  opp.db.AppointmentCustomerPackageServices
                        join item in  opp.db.Item on s.ItemService_Id equals item.id
                        where s.AppointmentCustomerPackageId == packageId
                        select new
                        {
                            ServiceName = item.Item_Name,
                            TotalSessions = s.RemainingSessions,
                            UsedSessions = s.UsedSessions,
                            RemainingSessions = s.RemainingSessions - s.UsedSessions
                        };

            return ToDataTable(query.ToList());
         
    }
        private DataTable ToDataTable<T>(System.Collections.Generic.List<T> items)
        {
            var dt = new DataTable(typeof(T).Name);
            var props = typeof(T).GetProperties();

            foreach (var prop in props)
            {
                dt.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (int i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }
                dt.Rows.Add(values);
            }
            return dt;
        }

        

        // 🔹 جلب باقة واحدة بالـ Id

    }

    #region DTOs
    public class CustomerPackageDTO
    {
        public long Id { get; set; }
        public long AppointmentPackageId { get; set; }
        public long CustomerId { get; set; }
        public string Package_Name { get; set; }
        public string Customer_Name  { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public int TotalSessions { get; set; }
        public int UsedSessions { get; set; }
    }

    public class PackageServiceDetailDTO
    {
        public long ItemService_Id { get; set; }
        public string ServiceName { get; set; }
        public int SessionsCount { get; set; }
    }
    #endregion
}