using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{
    [Table("LookUp_TaxableTypes")]
    public class LookUp_TaxableType
    {
        [Key]
        public long Id { get; set; }
        public string Code { get; set; }
        public string DescriptionAr { get; set; }
        public string DescriptionEn { get; set; }
         
        public List<LookUp_TaxableSubType> lookUp_TaxableSubTypes { get; set; }



    }
}