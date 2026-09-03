using System.Net;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations;
namespace AccountModelsLib.Models
{
    public enum InvoiceStatus
    {
        [Display(Name = "صحية")] Valid,
        [Display(Name = "جديدة")] Submitted,
        [Display(Name = "غير صحيحة")] Invalid,
        [Display(Name = "ملغية")] Cancel,
        [Display(Name = "مرفوضة")] Reject,
        NotAsync,
        NotContent,
    }
   
   
}
