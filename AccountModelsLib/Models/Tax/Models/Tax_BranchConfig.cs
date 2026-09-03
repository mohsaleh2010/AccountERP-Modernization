  
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{
    [Table("Tax_BranchesConfig")]
    public class Tax_BranchConfig
    {
        [Key]
        [ForeignKey("Tax_Branch")]
        public long Tax_BranchesId { get; set; }
        //public long Id { get; set; }
        public long? Account_BranchesId { get; set; }


        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string ActivityCode { get; set; }
        public string TokenPin { get; set; }
        public string IssuerId { get; set; }
        public string IssuerName { get; set; }
        public string IssuerType { get; set; }

        public string documentTypeVersion { get; set; }

        public string portalType { get; set; }

        public string dataBaseType { get; set; }
        public string ConnectionString { get; set; }
        public int? IsSign { get; set; }


        public int? RowStatus { get; set; }
        public int? RegistrationTaxFlag { get; set; }

        public string databasename { get; set; }

        public string ClientIdPOS { get; set; }
        public string ClientSecretPOS { get; set; }
        public string SerialDevice { get; set; }
        public string SystemVersion { get; set; }
        public string documentTypeVersionPOS { get; set; }
        public string PortalUrl { get; set; }
        public string TokenUrl { get; set; }
        public string SubmitUrl { get; set; }
        public string CancelUrl { get; set; }

        public string Host { get; set; }


        //[ForeignKey("Tax_BranchesId")]
        
        public   Tax_Branch Tax_Branch { get; set; }
    }
}
