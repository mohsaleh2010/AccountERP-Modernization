using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models 
{
    public enum GenderType
    {
        [System.ComponentModel.Description("نساء ورجال")]
        All = 0,

        [System.ComponentModel.Description("نساء فقط")]
        Female = 1,

        [System.ComponentModel.Description("رجال فقط")]
        Male = 2
    }

}
