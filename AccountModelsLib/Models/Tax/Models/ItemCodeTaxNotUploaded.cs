using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{
    
    [Table("ItemCodeTaxNotUploaded")]
    public class ItemCodeTaxNotUploaded  
    {
        public ItemCodeTaxNotUploaded()
        {
            Id = 0;
            ItemCode = "EG-674859545-123456784";
            CodeType = "EGS";
            requestReason = "for test";
            linkedCode = "for test";
            //public string PostURL { get; set; } = "/api/v1.0/codetypes/requests/codes";
            IsUploaded = false;
            //activeFrom = new DateTime();
            //codeName = "";
        }
        [Key]
        public long Id { get; set; }

        
        public long? Tax_BranchesId { get; set; }  
        public long? Item_Id { get; set; }  
        public string internalCode { get; set; }  
        public string CodeType { get; set; }  

        public string ParentCode { get; set; }

        public string ItemCode { get; set; }  

        public string CodeName { get; set; }

        public string CodeNameAr { get; set; }

        public DateTime activeFrom { get; set; }

        public DateTime? activeTo { get; set; }  

        public string description { get; set; }

        public string descriptionAr { get; set; }

        public string requestReason { get; set; }  

        public string linkedCode { get; set; }  
         
        //public string PostURL { get; set; } = "/api/v1.0/codetypes/requests/codes";


        public bool IsUploaded { get; set; }  

         
        
    }
}
