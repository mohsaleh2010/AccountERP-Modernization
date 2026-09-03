 
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


    public enum ItemCategoryEnum : long
    {
        [Description("قطع غيار"), Display(Name = "قطع غيار")] SpareParts = 1L,
        [Description("زيوت"), Display(Name = "زيوت")] Oils = 2L,
        [Description("مصنعيات "), Display(Name = "مصنعيات")] Masnaheaiat = 3L,
        [Description("خامات"), Display(Name = "خامات")] RawMaterials = 4L,

    }

    //public static class EnumHelper
    //{
    //    public static string GetEnumDescription(Enum value)
    //    {
    //        FieldInfo field = value.GetType().GetField(value.ToString());
    //        DescriptionAttribute attribute = field.GetCustomAttribute<DescriptionAttribute>();
    //        return attribute == null ? value.ToString() : attribute.Description;
    //    }

    //    public static string GetEnumDisplay(Enum value)
    //    {
    //        FieldInfo field = value.GetType().GetField(value.ToString());
    //        DisplayAttribute attribute = field.GetCustomAttribute<DisplayAttribute>();
    //        return attribute == null ? value.ToString() : attribute.Name;
    //    }
    //}
}