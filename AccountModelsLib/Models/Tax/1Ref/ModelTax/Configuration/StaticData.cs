using System.Net;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations;
namespace AccountModelsLib.Models
{
    public class StaticData
    {
        public static CustomerConfiguration Configuration { get; set; }
        public static string ClientId { get; set; } = "40af1955-0cd3-49ff-9c6d-7476385491c8";


        public static string ClientSecret { get; set; } = "b1b178db-d0de-46e1-87c4-8b7b0ebdeafb";

        public static string TokenUrl { get; set; }

        public static string Token { get; set; }

        public static string TokenPos { get; set; }

        private static string PreProduct { get; set; } = "https://api.preprod.invoicing.eta.gov.eg";

        private static string Production { get; set; } = "https://api.invoicing.eta.gov.eg";

        public static string Host { get; set; }

        public static EnvironmentPortal EnvironmentPortal
        {
            get => EnvironmentPortal.PerProduction;
            set
            {
                switch (value)
                {
                    case EnvironmentPortal.PerProduction:
                        StaticData.Host = StaticData.PreProduct;
                        StaticData.TokenUrl = "https://id.preprod.eta.gov.eg/connect/token";
                        break;
                    case EnvironmentPortal.Production:
                        StaticData.Host = StaticData.Production;
                        StaticData.TokenUrl = "https://id.eta.gov.eg/connect/token";
                         //StaticData.Configuration.DocumentTypeVersion = "1.0";
                        break;
                }
            }
        }
    }
}
