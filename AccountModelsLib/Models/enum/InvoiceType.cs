 
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
namespace AccountModelsLib.Models
{
    

    public enum InvoiceType
    {
        [Description("فاتورة بيع"), Display(Name = "I")] Invoice=1 ,
        [Description("اشعار خصم"), Display(Name = "C")] CreditNote=2,
        [Description("اشعار اضافة"), Display(Name = "D")] DebitNote=3,
        [Description("ايصال بيع"), Display(Name = "S")] Receipt=4,
        [Description("ايصال مرتجع بيع"), Display(Name = "R")] ReturnReceipt=5,
    }
    
    public static class EnumHelper2
    {
        public static string GetEnumDescription(Enum value)
        {
            FieldInfo field = value.GetType().GetField(value.ToString());
            DescriptionAttribute attribute = field.GetCustomAttribute<DescriptionAttribute>();
            return attribute == null ? value.ToString() : attribute.Description;
        }

        public static string GetEnumDisplay2(Enum value)
        {
            FieldInfo field = value.GetType().GetField(value.ToString());
            DisplayAttribute attribute = field.GetCustomAttribute<DisplayAttribute>();
            return attribute == null ? value.ToString() : attribute.Name;
        }
    }
}