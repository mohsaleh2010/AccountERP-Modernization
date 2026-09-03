using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{
    public enum InvoiceMaintainenceType : int
    {
        [System.ComponentModel.Description("برنامج فواتير المصانع")]
        factory = 0,



        [System.ComponentModel.Description("برنامج صيانة السيارات")]
        maintenance = 1,

        [System.ComponentModel.Description("برنامج  وكالة الخضار والفاكهة")]
        agencyWekalaKhodar = 2,

        [System.ComponentModel.Description("برنامج مراكز التجميل")]
        beautyCenters = 3,
        [System.ComponentModel.Description("برنامج كوافير  ")]
        beautySalons = 4,

        [System.ComponentModel.Description("برنامج نقاط البيع  ")]
        pos = 5,
    }

}
