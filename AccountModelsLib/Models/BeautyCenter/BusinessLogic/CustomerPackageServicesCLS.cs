// AccountModelsLib.Models.CustomerPackageServicesCLS.cs
using System;
using System.Collections.Generic;
using System.Linq;

namespace AccountModelsLib.Models
{
    /// <summary>
    /// Business layer for customer-package-service operations.
    /// Uses operations opp; and opp.db to access EF models.
    /// Namespace: AccountModelsLib.Models
    /// </summary>
    public class CustomerPackageServicesCLS
    {
        public operations opp { get; }
        public CustomerPackageServicesCLS( )
        {
            opp = new operations();
        }
        public CustomerPackageServicesCLS(operations _opp)
        {
            opp = _opp ?? throw new ArgumentNullException(nameof(_opp));
        }

        /// <summary>
        /// Get services rows for a given AppointmentCustomerPackage (subscription).
        /// Returns DTO with service name, allocated, used, remaining, and DB id.
        /// </summary>
        public List<CustomerPackageServiceDTO> GetServicesByCustomerPackage(long appointmentCustomerPackageId)
        {
            // NOTE: adjust entity/property names if your EF model uses different names.
            var q = opp.db.AppointmentCustomerPackageServices
                .Where(x => x.AppointmentCustomerPackageId == appointmentCustomerPackageId)
                .Select(x => new
                {
                    x.Id,
                    x.AppointmentCustomerPackageId,
                    x.ItemService_Id,
                    x.UsedSessions,
                    x.RemainingSessions
                })
                .ToList()
                .Select(x =>
                {
                    // get service name from Item table
                    var svcName = opp.db.Item.Where(i => i.Id == x.ItemService_Id)
                                              .Select(i => i.Item_Name)
                                              .FirstOrDefault() ?? $"Service #{x.ItemService_Id}";
                    return new CustomerPackageServiceDTO
                    {
                        Id = x.Id,
                        AppointmentCustomerPackageId = x.AppointmentCustomerPackageId,
                        ItemService_Id = x.ItemService_Id,
                        ServiceName = svcName,
                        AllocatedSessions = x.RemainingSessions,
                        UsedSessions = x.UsedSessions,
                        RemainingSessions = (x.RemainingSessions - x.UsedSessions)
                    };
                })
                .ToList();

            return q;
        }

        /// <summary>
        /// Update the allocated / used counts for a specific record.
        /// Saves changes immediately.
        /// </summary>
        public void UpdateServiceCounts(int id, int allocatedSessions, int usedSessions)
        {
            var rec = opp.db.AppointmentCustomerPackageServices.FirstOrDefault(x => x.Id == id);
            if (rec == null) throw new InvalidOperationException("Record not found.");

            // basic validation: used must be <= allocated and non-negative
            if (allocatedSessions < 0) throw new ArgumentException("AllocatedSessions must be >= 0");
            if (usedSessions < 0) throw new ArgumentException("UsedSessions must be >= 0");
            if (usedSessions > allocatedSessions) throw new ArgumentException("UsedSessions cannot exceed AllocatedSessions");

            rec.RemainingSessions = allocatedSessions;
            rec.UsedSessions = usedSessions;
            opp.db.SaveChanges();
        }

        /// <summary>
        /// Delete a service row by its Id.
        /// </summary>
        public void DeleteService(int id)
        {
            var rec = opp.db.AppointmentCustomerPackageServices.FirstOrDefault(x => x.Id == id);
            if (rec == null) return;
            opp.db.AppointmentCustomerPackageServices.Remove(rec);
            opp.db.SaveChanges();
        }

        /// <summary>
        /// Add a new service row for a customer-package subscription.
        /// Returns inserted Id.
        /// </summary>
        public long AddService(int appointmentCustomerPackageId, int itemServiceId, int allocatedSessions, int usedSessions = 0)
        {
            if (allocatedSessions < 0) throw new ArgumentException("AllocatedSessions must be >= 0");
            if (usedSessions < 0) throw new ArgumentException("UsedSessions must be >= 0");
            if (usedSessions > allocatedSessions) throw new ArgumentException("UsedSessions cannot exceed AllocatedSessions");

            var rec = new AppointmentCustomerPackageService
            {
                AppointmentCustomerPackageId = appointmentCustomerPackageId,
                ItemService_Id = itemServiceId,
                RemainingSessions = allocatedSessions,
                UsedSessions = usedSessions
            };
            opp.db.AppointmentCustomerPackageServices.Add(rec);
            opp.db.SaveChanges();
            return rec.Id;
        }
    }

    /// <summary>
    /// DTO used by the UI.
    /// </summary>
    public class CustomerPackageServiceDTO
    {
        public long Id { get; set; }
        public long AppointmentCustomerPackageId { get; set; }
        public long ItemService_Id { get; set; }
        public string ServiceName { get; set; }
        public int AllocatedSessions { get; set; }
        public int UsedSessions { get; set; }
        public int RemainingSessions { get; set; }
    }
}
