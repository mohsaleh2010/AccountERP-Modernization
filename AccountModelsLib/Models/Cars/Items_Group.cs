using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{
    [Table("Items_Group")]
    public class Items_Group
    {
        
       public long Id { get; set; }
        public string Name { get; set; }
        public string ItemsGroup_Name { get; set; }
        
    } 
}