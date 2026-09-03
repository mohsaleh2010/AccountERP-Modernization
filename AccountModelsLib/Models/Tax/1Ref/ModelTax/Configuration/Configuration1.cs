 
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace AccountModelsLib.Models
{

    public class Configuration1
    {
        

        public static DateTime StartDate { get; set; } = new DateTime(2021, 10, 1);

        public static DataBaseType DataBaseType { get; set; } = DataBaseType.SqlServer;

        public static string ClientId { get; set; }

        public static string ClientSecret { get; set; }

        public static string ActivityCode { get; set; } = "6820";

        public static string IssuerId { get; set; } = "528176870";

        public static string IssuerName { get; set; } = "الشركة الاهلية للمقازلات و التوريدات الكهربائية";

        public static string TokenPin { get; internal set; } = "65186327";

        public static bool IsSign { get; set; }

        public static string PortalUrl { get; set; }

        public static string TokenUrl { get; set; }

        public static string SubmitUrl { get; set; }

        public static string CancelUrl { get; set; }

        private static string HostPreProduct { get; set; } = "https://api.preprod.invoicing.eta.gov.eg/api/v1.0";

        private static string HostProduct { get; set; } = "https://api.invoicing.eta.gov.eg/api/v1.0";

        public static string Host { get; set; }= "https://api.preprod.invoicing.eta.gov.eg";

        public static string documentTypeVersion { get; set; } = "1.0";

        public static string itemType { get; set; } = "GS1";

        private static PortalType portalType;

       
    }
}