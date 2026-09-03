
using AccountModelsLib.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace AccountModelsLib.Models
{
    public class AppointmentPackagesCLS
    {
        public AccountModelsLib.operations opp;
        public AppointmentPackagesCLS()
        {
            opp = new AccountModelsLib.operations();
        }
        public AppointmentPackagesCLS(AccountModelsLib.operations _opp)
        {
            opp = _opp;
        }
         
        // جلب كل الباقات
        public List<AppointmentPackage> GetAll()
        {
            return opp.db.AppointmentPackages.ToList();
        }

        // جلب باقة محددة
        public AppointmentPackage GetById(int id)
        {
            return opp.db.AppointmentPackages.FirstOrDefault(x => x.Id == id);
        }

        // إضافة باقة جديدة
        public bool Add(AppointmentPackage package)
        {
            try
            {
                opp.db.AppointmentPackages.Add(package);
                opp.db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                // يمكن إضافة لوج لعرض الأخطاء
                return false;
            }
        }

        // تعديل باقة
        public bool Update(AppointmentPackage package)
        {
            try
            {
                var existing = opp.db.AppointmentPackages.FirstOrDefault(x => x.Id == package.Id);
                if (existing == null) return false;

                opp.db.Entry(existing).CurrentValues.SetValues(package);
                existing.UpdatedAt = DateTime.Now;

                opp.db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // حذف باقة
        public bool Delete(int id)
        {
            try
            {
                var existing = opp.db.AppointmentPackages.FirstOrDefault(x => x.Id == id);
                if (existing == null) return false;

                opp.db.AppointmentPackages.Remove(existing);
                opp.db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public List<AppointmentPackageDTO> GetAllPackages(string search = "")
        {
            var query = opp.db.AppointmentPackages.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => p.Name_Package.Contains(search));

            return query
                .Select(p => new AppointmentPackageDTO
                {
                    Id = p.Id,
                    Name_Package = p.Name_Package,
                    //TotalSessions = p.TotalSessions,
                    DurationInDays = p.DurationInDays,
                    Price = p.Price,
                    IsActive = p.IsActive,
                    Services = p.AppointmentPackageServices.Select(s => new ServiceDTO
                    {
                        ServiceId = s.ItemServiceId,
                        ServiceName = s.ItemService.Item_Name,
                        SessionsCount = s.TotalSessions
                    }).ToList()
                }).ToList();
        }

        public void DeletePackage(int id)
        {
            var pkg = opp.db.AppointmentPackages.FirstOrDefault(p => p.Id == id);
            if (pkg != null)
            {
                opp.db.AppointmentPackages.Remove(pkg);
                opp.db.SaveChanges();

                DeletePackageServicesByPackageId(pkg.Id);
            }
        }
        #region AppointmentPackage CRUD
        public AppointmentPackage GetPackageById(long id)
        {
            return opp.db.AppointmentPackages.FirstOrDefault(p => p.Id == id);
        }

        public long AddPackage(AppointmentPackage package)
        {
            package.CreatedAt = DateTime.Now;
            opp.db.AppointmentPackages.Add(package);
            opp.db.SaveChanges();
            return package.Id;
        }

        public void UpdatePackage(AppointmentPackage package)
        {
            var existing = opp.db.AppointmentPackages.FirstOrDefault(p => p.Id == package.Id);
            if (existing == null) throw new InvalidOperationException("Package not found.");

            // تحديث الخصائص العامة للحجز
            opp.db.Entry(existing).CurrentValues.SetValues(package);

            // Update fields
            //existing.Name_Package = package.Name_Package;
            //existing.DurationInDays = package.DurationInDays;
            //existing.Price = package.Price;
            //existing.Description = package.Description;
            //existing.IsActive = package.IsActive;
            //existing.UpdatedAt = DateTime.Now.Date;

            opp.db.SaveChanges();
        }

#endregion


        #region AppointmentPackageServices (link table)

        public List<AppointmentPackageService> GetServicesByPackageId(long packageId)
        {
            return opp.db.AppointmentPackageServices
                      .Where(s => s.AppointmentPackageId == packageId)
                      .ToList();
        }

        public void AddPackageService(AppointmentPackageService service)
        {
            opp.db.AppointmentPackageServices.Add(service);
            opp.db.SaveChanges();
        }

        public void UpdatePackageService(AppointmentPackageService service)
        {
            var existing = opp.db.AppointmentPackageServices.FirstOrDefault(s => s.Id == service.Id);
            if (existing == null) throw new InvalidOperationException("PackageService not found.");
            existing.TotalSessions = service.TotalSessions;
            opp.db.SaveChanges();
        }

        public void DeletePackageServiceById(long id)
        {
            var existing = opp.db.AppointmentPackageServices.FirstOrDefault(s => s.Id == id);
            if (existing != null)
            {
                opp.db.AppointmentPackageServices.Remove(existing);
                opp.db.SaveChanges();
            }
        }

        public void DeletePackageServicesByPackageId(long packageId)
        {
            var list = opp.db.AppointmentPackageServices.Where(s => s.AppointmentPackageId == packageId).ToList();
            if (list.Any())
            {
                opp.db.AppointmentPackageServices.RemoveRange(list);
                opp.db.SaveChanges();
            }
        }

        /// <summary>
        /// Synchronize package services based on provided selectedItems.
        /// selectedItems: list of (ItemServiceId, SessionsCount).
        /// This method will add new, update existing, and delete removed services.
        /// </summary>
        public void SyncPackageServices(long packageId, List<(long ItemServiceId, int SessionsCount)> selectedItems)
        {
            // get existing services from DB
            var existing = opp.db.AppointmentPackageServices
                          .Where(s => s.AppointmentPackageId == packageId)
                          .ToList();

            // Add or update
            foreach (var sel in selectedItems)
            {
                var ex = existing.FirstOrDefault(e => e.ItemServiceId == sel.ItemServiceId);
                if (ex == null)
                {
                    // add new
                    var svc = new AppointmentPackageService
                    {
                        AppointmentPackageId = packageId,
                        ItemServiceId = sel.ItemServiceId,
                        TotalSessions = sel.SessionsCount
                    };
                    opp.db.AppointmentPackageServices.Add(svc);
                }
                else
                {
                    // update if different
                    if (ex.TotalSessions != sel.SessionsCount)
                        ex.TotalSessions = sel.SessionsCount;
                }
            }

            // Delete removed
            var toDelete = existing.Where(e => !selectedItems.Any(s => s.ItemServiceId == e.ItemServiceId)).ToList();
            if (toDelete.Any())
            {
                opp.db.AppointmentPackageServices.RemoveRange(toDelete);
            }

            opp.db.SaveChanges();
        }

        #endregion

        #region Items helper

        /// <summary>
        /// Return all items (services/products) — you may filter by Is_ItemOrService if needed.
        /// Each returned item has Id and DisplayName.
        /// </summary>
        public List<DisplayItemService> GetAllItemsForCombo(int Is_ItemOrService)
        {
            ItemCLS itemCLS = new ItemCLS(ref opp);
            List<DisplayItemService> itemsItemServiceLst = itemCLS.GetAllItemsForCombo(Is_ItemOrService);
            return itemsItemServiceLst;


        }

        #endregion
    }


    public class AppointmentPackageDTO
    {
        public long Id { get; set; }
        public string Name_Package { get; set; }
        public int TotalSessions { get; set; }
        public int? DurationInDays { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
        public List<ServiceDTO> Services { get; set; }
    }

    public class ServiceDTO
    {
        public long ServiceId { get; set; }
        public string ServiceName { get; set; }
        public int SessionsCount { get; set; }
    }
    public class AppointmentSubscriptionCLS
    {
        private readonly AccountModelsLib.operations opp;

        public AppointmentSubscriptionCLS(AccountModelsLib.operations _opp)
        {
            opp = _opp;
        }

        // جلب كل الاشتراكات
        //public List<AppointmentCustomerPackageService> GetAll()
        //{
        //    return opp.db.AppointmentSubscriptions.ToList();
        //}

        //// جلب اشتراك محدد
        //public AppointmentCustomerPackageService GetById(int id)
        //{
        //    return opp.db.AppointmentSubscriptions.FirstOrDefault(x => x.Id == id);
        //}

        // إضافة اشتراك جديد
        //public bool Add(AppointmentCustomerPackageService subscription)
        //{
        //    try
        //    {
        //        opp.db.AppointmentSubscriptions.Add(subscription);
        //        opp.db.SaveChanges();
        //        return true;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        // تعديل اشتراك
        //public bool Update(AppointmentCustomerPackageService subscription)
        //{
        //    try
        //    {
        //        var existing = opp.db.AppointmentSubscriptions.FirstOrDefault(x => x.Id == subscription.Id);
        //        if (existing == null) return false;

        //        opp.db.Entry(existing).CurrentValues.SetValues(subscription);
        //        opp.db.SaveChanges();
        //        return true;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        // حذف اشتراك
        //public bool Delete(int id)
        //{
        //    try
        //    {
        //        var existing = opp.db.AppointmentSubscriptions.FirstOrDefault(x => x.Id == id);
        //        if (existing == null) return false;

        //        opp.db.AppointmentSubscriptions.Remove(existing);
        //        opp.db.SaveChanges();
        //        return true;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        // خصم جلسة من الاشتراك
        //public bool DeductSession(int subscriptionId)
        //{
        //    try
        //    {
        //        var sub = opp.db.AppointmentSubscriptions.FirstOrDefault(x => x.Id == subscriptionId && x.Status == 1);
        //        if (sub == null || sub.RemainingSessions <= 0) return false;

        //        sub.UsedSessions += 1;
        //        sub.RemainingSessions -= 1;

        //        // إذا انتهت الجلسات
        //        if (sub.RemainingSessions <= 0)
        //            sub.Status = 0;

        //        opp.db.SaveChanges();
        //        return true;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}
    }
}
