
using AccountModelsLib.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace AccountModelsLib.Models
{
    public class AppointmentPackageServicesCLS
    {
        public AccountModelsLib.operations opp;
        public AppointmentPackageServicesCLS()
        {
            opp = new AccountModelsLib.operations();
        }
        public AppointmentPackageServicesCLS(AccountModelsLib.operations _opp)
        {
            opp = _opp;
        }



        public List<AppointmentPackageService> GetByPackageId(int packageId)
        {
            return opp.db.AppointmentPackageServices
                .Where(s => s.AppointmentPackageId == packageId)
                .ToList();
        }

        public void Add(AppointmentPackageService service)
        {
            opp.db.AppointmentPackageServices.Add(service);
            opp.db.SaveChanges();
        }

        public void DeleteByPackageId(long packageId)
        {
            var services = opp.db.AppointmentPackageServices
                .Where(s => s.AppointmentPackageId == packageId)
                .ToList();
            opp.db.AppointmentPackageServices.RemoveRange(services);
            opp.db.SaveChanges();
        }
    }
}
