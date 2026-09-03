using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models 
{
   
using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
  
    [Table("LookUp_ItemServiceCategory")]
    public class LookUp_ItemServiceCategory
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string ServiceCategoryName { get; set; }

        public string Notes { get; set; }

        public int IsActive { get; set; } = 1;

        public int? OrderIndex { get; set; }
        public int? GenderTypeId { get; set; }
        public string  GenderTypeDescription { get; set; }
        
        // الربط مع الخدمات
        public virtual ICollection<Items> Items { get; set; }
    }


}
