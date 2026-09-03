using System.Net;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations;
namespace AccountModelsLib.Models
{
    public class StaticData1
    {
        public static CustomerConfiguration Configuration { get; set; }

        public static string TokenUrl { get; set; }

        public static string Token { get; set; }

        public static string TokenPos { get; set; }

        private static string PreProduct { get; set; } = "https://api.preprod.invoicing.eta.gov.eg";

        private static string Production { get; set; } = "https://api.invoicing.eta.gov.eg";

        public static string Host { get; set; }

        public static EnvironmentPortal EnvironmentPortal1
        {
            get => EnvironmentPortal.PerProduction;
            set
            {
                switch (value)
                {
                    case EnvironmentPortal.PerProduction:
                        StaticData.Host = StaticData1.PreProduct;
                        StaticData.TokenUrl = "https://id.preprod.eta.gov.eg/connect/token";
                        break;
                    case EnvironmentPortal.Production:
                        StaticData.Host = StaticData1.Production;
                        StaticData.TokenUrl = "https://id.eta.gov.eg/connect/token";
                         //StaticData.Configuration.DocumentTypeVersion = "1.0";
                        break;
                }
            }
        }
    }
}
