using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models 
{
    public class Lookup_CarTypeModel
    {
        public long Id { get; set; }
        public long Lookup_CarTypeId { get; set; }
        public string  Name { get; set; }
         
    }
    
}
