 
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;
using System;
 
using System.Security.Policy;

namespace AccountModelsLib.Models
{

    public class CommonConfiguration
    {
        public CommonConfiguration(int Tax_BranchesId = 0)
        {
            if (Tax_BranchesId > 0)
            {
                SetCommonTaxConfig(Tax_BranchesId);
            }
        }

        public long Tax_BranchesId { get; set; }
        public string TaxVatNumber { get; set; }
        public string TaxRegistrationNumber { get; set; }
        public string Commercial_Register { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string ActivityCode { get; set; }
        public string IssuerId { get; set; }
        public string IssuerName { get; set; }
        public string IssuerType { get; set; }

        public string TokenPin { get; set; }
        public InvoiceStatus invoiceStatus { get; set; }
        public PortalType PortalType { get; set; }
        public DataBaseType dataBaseType { get; set; }
        public string ConnectionString { get; set; }
        public int IsSign { get; set; }
        public DateTime StartDate { get; set; }
        public string PortalUrl { get; set; }
        public string TokenUrl { get; set; }
        public string SubmitUrl { get; set; }
        public string CancelUrl { get; set; }
        private string HostPreProduct { get; set; }
        private string HostProduct { get; set; }
        public string Host { get; set; }
        public string documentTypeVersion { get; set; }
        public string itemType { get; set; }

        public string ClientIdPOS { get; set; }
        public string ClientSecretPOS { get; set; }
        public string SerialDevice { get; set; }
        public string SystemVersion { get; set; }
        public string documentTypeVersionPOS { get; set; }
        public string databasename { get; set; }
        public string branchID { get; set; }
        public string country { get; set; }
        public string governate { get; set; }
        public string regionCity { get; set; }
        public string street { get; set; }
        public string buildingNumber { get; set; }
        public string postalCode { get; set; }
        public string floor { get; set; }
        public string room { get; set; }
        public string landmark { get; set; }
        public string additionalInformation { get; set; }
        public string lastuuid { get; set; }
        public double Trial { get; set; }


        public string serial20;
        public string DBServerName { get; set; }
        public string DataBaseName { get; set; }
        public string DB_UserName { get; set; }
        public string DB_Password { get; set; }
        public string DB_FilePath { get; set; }


        void SetPortalType()
        {
            HostPreProduct = "https://api.preprod.invoicing.eta.gov.eg";

            HostProduct = "https://api.invoicing.eta.gov.eg";

            Host = "";

             

            //HostPreProduct = "https://api.preprod.invoicing.eta.gov.eg/api/v1.0";
            //    HostProduct = "https://api.invoicing.eta.gov.eg/api/v1.0";
            //    Host = "https://api.preprod.invoicing.eta.gov.eg";

            if (PortalType == PortalType.Test)
            {
                Host = HostPreProduct;
                PortalUrl = "https://api.preprod.invoicing.eta.gov.eg/api/v1/documentsubmissions";
                TokenUrl = "https://id.preprod.eta.gov.eg/connect/token";
                SubmitUrl = "https://api.preprod.invoicing.eta.gov.eg/api/v1.0/documentSubmissions/";
            }
            if (PortalType == PortalType.Live)
            {
                Host = HostProduct;
                PortalUrl = "https://api.invoicing.eta.gov.eg/api/v1/documentsubmissions";
                TokenUrl = "https://id.eta.gov.eg/connect/token";
                SubmitUrl = "https://api.invoicing.eta.gov.eg/api/v1.0/documentSubmissions/";
                CancelUrl = "https://api.sit.invoicing.eta.gov.eg/api/v1.0/documents/";
            }


        }

        void SetCommonTaxConfig(long Tax_BranchesId_)
        {
            try
            {

                Tax_BranchCLS tbCLS = new Tax_BranchCLS();
                tbCLS.GetAllBranchDataForConfig(Tax_BranchesId_);
                if (tbCLS.tb == null)
                { return; }
                Tax_BranchesId = tbCLS.tb.Id;
                TaxRegistrationNumber = tbCLS.tb.TaxRegistrationNumber ?? "0";
                TaxVatNumber = tbCLS.tb.TaxVatNumber ?? "0";
                Commercial_Register = tbCLS.tb.Commercial_Register ?? "0";


                ClientId = tbCLS.tbConfig.ClientId ?? "";
                ClientSecret = tbCLS.tbConfig.ClientSecret ?? "";
                ActivityCode = tbCLS.tbConfig.ActivityCode ?? "";
                IssuerId = tbCLS.tbConfig.IssuerId ?? "";
                IssuerName = tbCLS.tbConfig.IssuerName ?? "";
                IssuerType = tbCLS.tbConfig.IssuerType ?? "";
                TokenPin = tbCLS.tbConfig.TokenPin ?? "";
                //invoiceStatus = tbCLS.tbConfig.invoiceStatus;
                IsSign = tbCLS.tbConfig.IsSign ?? 0;
                StartDate = DateTime.Now.Date;
                SubmitUrl = tbCLS.tbConfig.SubmitUrl ?? "";
                CancelUrl = tbCLS.tbConfig.CancelUrl ?? "";
                documentTypeVersion = tbCLS.tbConfig.documentTypeVersion ?? "";
                itemType = "EGS";

                try
                {
                    if ( tbCLS.tbConfig.dataBaseType != null)
                    {
                        dataBaseType = (DataBaseType)Enum.Parse(typeof(DataBaseType), tbCLS.tbConfig.dataBaseType);
                    }
                }
                catch (Exception ee)
                {
                    dataBaseType = DataBaseType.SqlServer;
                }
                ConnectionString = My_DbContext.ConnStr;// tbCLS.tbConfig.ConnectionString;

                try
                {
                    if (tbCLS.tbConfig.portalType != null)
                    {
                        PortalType = (PortalType)Enum.Parse(typeof(PortalType), tbCLS.tbConfig.portalType);
                    }
                }
                catch (Exception)
                {
                    PortalType = PortalType.Test;
                }

                ClientIdPOS = tbCLS.tbConfig.ClientIdPOS ?? "";
                ClientSecretPOS = tbCLS.tbConfig.ClientSecretPOS ?? "";
                SerialDevice = tbCLS.tbConfig.SerialDevice ?? "";
                SystemVersion = tbCLS.tbConfig.SystemVersion ?? "";

                documentTypeVersionPOS = tbCLS.tbConfig.documentTypeVersionPOS ?? "";

                databasename = tbCLS.tbConfig.databasename ?? "";

                branchID = tbCLS.tb.BranchID; // must be: 0

                country = tbCLS.tb.Country ?? "EG";// "EG";
                governate = tbCLS.tb.Governate ?? "Cairo";

                regionCity = tbCLS.tb.RegionCity ?? "Cairo";
                street = tbCLS.tb.Street ?? "1";
                buildingNumber = tbCLS.tb.buildingNumber ?? "1";
                postalCode = tbCLS.tb.postalCode ?? "1";
                floor = tbCLS.tb.floor ?? "1";
                room = tbCLS.tb.room ?? "1";
                landmark = tbCLS.tb.landmark ?? "";
                additionalInformation = tbCLS.tb.additionalInformation ?? "";
                lastuuid = tbCLS.tb.lastuuid ?? "";

                SetPortalType();
            }
            catch (Exception ee)
            {
                 
            }
        }

        public void Save()
        {

        }

        public string GetTaxCode(string text)
        {
            string RegistrationNumber = Common.CommonTaxConfig.TaxRegistrationNumber ?? "";
            string Code = "EG-" + RegistrationNumber + "-" + text;
            return Code;

        }
    }
}
