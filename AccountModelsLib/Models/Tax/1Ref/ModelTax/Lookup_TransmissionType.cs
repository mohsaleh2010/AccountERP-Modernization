using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models 
{
    public class Account_BranchesTaxInfo
    {
        public long Id { get; set; }
        public string testtype { get; set; }
        public string CompanyName { get; set; }
        public string RowCode { get; set; }
        public string TaxNumber { get; set; }
        public string RegistrationNumber { get; set; }
        public string Phone1 { get; set; }
        public string buildingNumber { get; set; }
        public string postalCode { get; set; }
        public string room { get; set; }
        public string Address { get; set; }
        public string floor { get; set; }
        public string BranchID { get; set; }
        public string RegionCity { get; set; }
        public string databasename { get; set; }
        public string lastuuid { get; set; }
        public string landmark { get; set; }
        public string additionalInformation { get; set; }
        public string Governate { get; set; }
        public string Street { get; set; }
        public string BankName { get; set; }
        public string AccountNo { get; set; }
        public string IBANNumber { get; set; }
        public string BankAddress { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string ActivityCode { get; set; }
        public string TokenPin { get; set; }
        public string documentTypeVersion { get; set; }
        public string ClientIdPOS { get; set; }
        public string ClientSecretPOS { get; set; }
        public string SerialDevice { get; set; }
        public string SystemVersion { get; set; }
        public string documentTypeVersionPOS { get; set; }
    }
}
