using System;
using System.Linq;
using System.Collections.Generic;
using AccountModelsLib.SalesRep;
using AccountModelsLib.Models;

namespace AccountModelsLib.SalesRep
{
    public class SalesRepSyncService
    {
        private readonly operations _opp;

        public SalesRepSyncService(operations opp)
        {
            _opp = opp;
        }

        // دفع البيانات للمندوب (Pull)
        public object GetSyncData(long repId)
        {
            return new
            {
                Routes = _opp.db.SalesRep_Routes.Where(r => r.Rep_Id == repId).ToList(),
                Customers = _opp.db.SalesRep_Route_Customers.Where(rc => rc.Route_Id != null).ToList(),
                Prices = _opp.db.Price_Type_Sell_Values.ToList(),
                Policies = _opp.db.SalesRep_Credit_Policies.ToList()
            };
        }

        // استقبال البيانات من المندوب (Push)
        public void ProcessSyncInbox(long deviceId)
        {
            var pendingSyncs = _opp.db.SalesRep_Sync_Inboxes
                .Where(s => s.Device_Id == deviceId && s.Status == 0) // Received
                .OrderBy(s => s.Received_At)
                .ToList();

            foreach (var sync in pendingSyncs)
            {
                try
                {
                    // 1. فك تشفير البيانات (Payload)
                    // 2. توجيه الحركة للمحرك المناسب (فاتورة/تحصيل/مرتجع)
                    // 3. تحديث الحالة
                    sync.Status = 1; // Processed
                    sync.Processed_At = DateTime.Now;
                }
                catch (Exception ex)
                {
                    sync.Status = 2; // Error
                    sync.Error = ex.Message;
                }
            }
            _opp.db.SaveChanges();
        }
    }
}
