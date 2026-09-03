using System.Net;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations;
namespace AccountModelsLib.Models
{
    public enum InvoiceStatusEnum
    {
        /// <summary>
        /// فاتورة معلقة / مؤقتة (مسودة لشاشة الـ POS قابلة للتعديل والحذف)
        /// </summary>
        Temporary = 0,

        /// <summary>
        /// فاتورة مغلقة / معتمدة نهائياً (رحلت للمخازن والحسابات وغير قابلة للتعديل)
        /// </summary>
        Closed = 1,

        /// <summary>
        /// فاتورة ملغاة رقابياً (حُذفت من الشاشة ولكن تظل في قاعدة البيانات لأغراض التدقيق)
        /// </summary>
        Canceled = 2
    }
}
