using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models 
{
    [NotMapped]
    public class DisplayItemService
    {
        public long Id { get; set; }
        public string Item_Name { get; set; }
        public int Is_ItemOrService { get; set; } // 1 for item, 0 for service
    }
}
