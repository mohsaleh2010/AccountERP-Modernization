using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models 
{
    public class Lookup_CarType
    {
        public long Id { get; set; }
        public string  Name { get; set; }
        public string NameEn { get; set; }
        public string Code { get; set; }
        
    }
    
}
