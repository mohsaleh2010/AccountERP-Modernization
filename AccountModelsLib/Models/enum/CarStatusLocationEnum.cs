 
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace AccountModelsLib.Models
{

    
    public enum  CarStatusLocationEnum: long
    {
        [Description("سيارات فى الإستقبال"), Display(Name = "سيارات فى الإستقبال")] InReception = 1,
        [Description("سيارات فى الورشة"), Display(Name = "سيارات فى الورشة")] InWarsha = 2,
        [Description("سيارات فى المغسلة"), Display(Name = "سيارات فى المغسلة")] InWashing = 3,
        [Description("سيارات تنتظر المحاسبة"), Display(Name = "سيارات تنتظر المحاسبة ")] InWaitingAcconting = 4,
        //[Description("سيارات فى المحاسبة"), Display(Name = "سيارات فى المحاسبة ")] InStillAcconting = 5,
        //[Description("سيارات تمت محاسبتها"), Display(Name = "سيارات تمت محاسبتها")] InFinishedAcconting = 6 
 

    }
    public static class Helpers
    {
        public static string GetEnumDescription(this Enum input)
        {
            FieldInfo fi = input.GetType().GetField(input.ToString());

            if (fi.GetCustomAttributes(typeof(DescriptionAttribute), false) is DescriptionAttribute[] attributes &&
                attributes.Any())
            {
                return attributes.First().Description;
            }

            return input.ToString();
        }

        public static IList ToList(this Type type)
        {
            ArrayList list = new ArrayList();
            Array enumValues = Enum.GetValues(type);
            foreach (Enum value in enumValues)
            {
                list.Add(new KeyValuePair<Enum,string>(value, value.GetEnumDescription()));
            }

            return list;
        }
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