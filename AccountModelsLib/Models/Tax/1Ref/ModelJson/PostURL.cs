using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{
  public static   class PostURL
    { 
        public static  string ItemCodeTax { get; set; } = "/api/v1.0/codetypes/requests/codes";
    }
    public static class GetURL
    { 
       public static string ItemCodeTax { get; set; } = "/api/v1.0/codetypes/requests/my";
    }
}
