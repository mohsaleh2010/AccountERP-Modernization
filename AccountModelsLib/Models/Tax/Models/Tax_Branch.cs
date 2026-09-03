using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AccountModelsLib.Models 
{
    [Table("Tax_Branches")]
    public class Tax_Branch
    {
        
        [Key]
        public long Id { get; set; }
        public long? Account_BranchesId { get; set; }
        
        public string? Name { get; set; }
        public string?  Code { get; set; }
        public string? TaxVatNumber { get; set; }//رقم التسجيل فى القيمة المضافة
        public string? TaxRegistrationNumber { get; set; }//رقم التسجيل الضريبى
        public string? Commercial_Register { get; set; }  //السجل التجارى
        
        public string? Address { get; set; }
        public string? BranchID { get; set; }
        public string? Country { get; set; }
        
        public string? Governate { get; set; }
        public string? RegionCity { get; set; }
        public string? Street { get; set; }
        public string? Phone1 { get; set; }
        public string? Phone2 { get; set; }
        public string? Phone3 { get; set; }
        public string? Fax { get; set; }
        public string? Email { get; set; }
        public string? BankName { get; set; }
        public string? AccountNo { get; set; }
        public string? IBANNumber { get; set; }
        public string? BankAddress { get; set; }
        public int? Active { get; set; }
        public int? IsDefault { get; set; }

        
        //public byte[] Logo { get; set; }
        
        public long? User_Id { get; set; }
        public long? Branch_Id { get; set; }
        public string? buildingNumber { get; set; }
        public string? postalCode { get; set; }
        public string? room { get; set; }
        public string? floor { get; set; }
        public string? lastuuid { get; set; }
        public string? landmark { get; set; }
        public string? additionalInformation { get; set; }


        public virtual Tax_BranchConfig Tax_BranchConfig { get; set; }
    }
}
