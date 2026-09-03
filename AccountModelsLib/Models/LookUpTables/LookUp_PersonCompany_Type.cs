using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models 
{
    public class LookUp_PersonCompany_Type
    {
        public long Id { get; set; }
        public string  Name { get; set; }
        
    }
}
