using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{

    [Table("ItemCodeTaxAccepted")]
    public class ItemCodeTaxAccepted
    {
        public ItemCodeTaxAccepted()
        {
            //Id = 0;
            //ItemCode = "EG-674859545-123456784";
            //ItemCodeType = "EGS";
            //requestReason = "for test";
            //linkedCode = "for test";
            
            //IsRelatedToLocalItem = false;
           
        }
        [Key]

        public long Id { get; set; }
        
        public string Barcode { get; set; }
        public long? Item_Id { get; set; }
        public long? Tax_BranchesId { get; set; }
        public long? CurrencyId { get; set; }
        public string internalCode { get; set; }
        
        public string ParentCode { get; set; }
        public string CodeName { get; set; }
        public string CodeNameAr { get; set; }
        public string DescriptionEn { get; set; }
        public string DescriptionAr { get; set; }
        public string ItemCode { get; set; }
        public string ItemCodeType { get; set; }
        public decimal  Price { get; set; }
        public string UnitCode { get; set; }
        public string UnitDescEn { get; set; }
        public string UnitDescAr { get; set; }
        public string ParentCodeLookupValue { get; set; }

        
        
       
        public DateTime? activeFrom { get; set; }
        public DateTime? activeTo { get; set; }
        public string requestReason { get; set; }
        public string linkedCode { get; set; }
        public bool IsRelatedToLocalItem { get; set; }
        public string codeCategorization { get;  set; }
        public string codeTypeID { get;  set; }
        public string codeTypeLevelID { get;  set; }
        public string codeTypeLevelNamePrimaryLang { get;  set; }
        public string codeTypeLevelNameSecondaryLang { get;  set; }
        public string parentCodeNamePrimaryLang { get;  set; }
        public string parentCodeNameSecondaryLang { get;  set; }
        public string parentLevelName { get;  set; }
        public string codeTypeNameSecondaryLang { get;  set; }
        public string active { get;  set; }
        public int pageNo { get;  set; }
        public int ps_in_pageNo { get;   set; }
        public int Is_Repeated { get;   set; }
        public int? Is_RepeatedExcluded { get;   set; }
        public int Is_SendToERP { get;   set; }
        
    }
}
