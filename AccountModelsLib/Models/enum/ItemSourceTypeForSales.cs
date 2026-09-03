using System.Net;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
// 
namespace AccountModelsLib.Models
{
    public enum ItemSourceTypeForSales
    {
        [Description("من باتش وارد")]
        FromBatch = 0,

        [Description("من قائمة الأصناف")]
        FromItemList = 1
    }
    public static class EnumHelper1
    {
        //public static object GetEnumDescription(Enum value)
        //{
        //    FieldInfo fi = value.GetType().GetField(value.ToString());

        //    DescriptionAttribute[] attributes =
        //        (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

        //    return attributes.Length > 0 ? attributes[0].Description : value.ToString();
        //}

        //public static void LoadEnumToComboBox<TEnum>(ComboBox comboBox, bool addDefault = false)
        //{
        //    var values = Enum.GetValues(typeof(TEnum)).Cast<TEnum>();

        //    var items = values.Select(v => new
        //    {
        //        Value = v,
        //        Text = GetEnumDescription(v as Enum)
        //    }).ToList();

        //    comboBox.DisplayMember = "Text";
        //    comboBox.ValueMember = "Value";
        //    comboBox.DataSource = items;
        //}
    }
}
