 
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{
    [Table("LookUp_TaxableSubTypes")]
    public class LookUp_TaxableSubType
    {

        public long Id { get; set; }
        public string Code { get; set; }
        public string DescriptionAr { get; set; }
        public string DescriptionEn { get; set; }

        public string TaxtypeReference { get; set; }


        public long LookUp_TaxableTypeId { get; set; }
        [ForeignKey("LookUp_TaxableTypeId")]
        public LookUp_TaxableType LookUp_TaxableTypes { get; set; }

         
    }
}