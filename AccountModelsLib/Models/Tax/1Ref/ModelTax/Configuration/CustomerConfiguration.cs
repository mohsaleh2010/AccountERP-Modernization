 
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace AccountModelsLib.Models
{
    public class CustomerConfiguration
    {

        public string ClientId { get; set; }


        public string ClientSecret { get; set; }


        public string ActivityCode { get; set; }


        public string IssuerId { get; set; }


        public string IssuerName { get; set; }

        public string TokenPin { get; set; }


        public static string documentTypeVersion { get; set; }


        public InvoiceStatus invoiceStatus { get; set; }


        public DataBaseType dataBaseType { get; set; }


        public string ConnectionString { get; set; }


        public bool IsSign { get; set; }

       
    }
}