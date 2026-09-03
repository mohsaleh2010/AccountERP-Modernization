 
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations;
namespace AccountModelsLib.Models
{
      

    public enum PortalType
    {
        [Display(Name = "تجريبي")] Test,
        [Display(Name = "فعلي")] Live,
    }

    public enum EnvironmentPortal
    {
        [Display(Name = "التجريبية")]
        PerProduction,
        [Display(Name = "الفعلية")]
        Production
    }
 

}