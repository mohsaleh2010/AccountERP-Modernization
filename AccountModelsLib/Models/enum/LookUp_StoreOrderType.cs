 
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


    public enum LookUp_StoreOrderType : int
    {
        [Description("مخزن"), Display(Name = "مخزن")] Store = 4,
        [Description("قسم"), Display(Name = "قسم")] Department = 20,
        [Description("عميل "), Display(Name = "عميل")] Customer = 2,
        [Description("مورد"), Display(Name = "مورد")] Supplier = 1,
        [Description("ماكينة"), Display(Name = "ماكينة")] Machine = 23,

    }
    public enum  StoreOrderType : int
    {
        [Description("إذن صرف / تحويل لمخزن أو قسم"), Display(Name = "إذن صرف / تحويل لمخزن أو قسم")] SarfTahweel_1 =1001,
        [Description("إذن صرف لعميل"), Display(Name = "إذن صرف لعميل")] SarfToCustomer_2 = 1002,
        [Description("إذن مرتجع صرف من عميل"), Display(Name = "إذن مرتجع صرف من عميل")] MortagaFromCustomer_3 = 1003,
        [Description("إذن إستلام من مورد"), Display(Name = "إذن إستلام من مورد")] EstlamFromSupplier_4 = 1004,
        [Description("إذن مرتجع  لمورد"), Display(Name = "إذن مرتجع  لمورد")] MortagaToSupplier_5 = 1005,
        [Description("إذن مرتجع صرف / تحويل لمخزن أو قسم"), Display(Name = "إذن مرتجع صرف / تحويل لمخزن أو قسم")] MortagaSarfTahweel_6 = 1006,
        [Description("إذن تسوية بالزيادة"), Display(Name = "إذن تسوية بالزيادة")] TaswehaZeada_7 = 1007,
        [Description("إذن تسوية بالنقص"), Display(Name = "إذن تسوية بالنقص")] TaswehaNaks_8 = 1008,
        [Description("إذن إستلام من الإنتاج"), Display(Name = "إذن إستلام من الإنتاج")] EstlamFromEntag_9 = 1009,
        [Description("إذن صرف إلى الإنتاج / التشغيل"), Display(Name = "إذن صرف إلى الإنتاج / التشغيل")] SarfToEntag_10 = 1010,
        [Description("إذن صرف قطع غيار"), Display(Name = "إذن صرف قطع غيار")] SarfKet3Khiar_12 = 1012,
        [Description("صرف للهالك"), Display(Name = "صرف للهالك")] SarfToHalek_13 = 1013,
        [Description("إذن إستلام من مخزن خارجى"), Display(Name = "إذن إستلام من مخزن خارجى")] EstlamFromOuterStore_14 = 1014,
        [Description("إذن صرف إلى مخزن خارجى"), Display(Name = "إذن صرف إلى مخزن خارجى")] SarfToOuterStore_15 = 1015,
        [Description("إذن إستلام من المخزن"), Display(Name = "إذن إستلام من المخزن")] EstlamFromStore_16 = 1016,
        [Description("إذن صرف إلى  المخزن"), Display(Name = "إذن صرف إلى  المخزن")] SarfToStore_17 = 1017,
        [Description("إذن صرف للورشة"), Display(Name = "إذن صرف للورشة")] SarfToWarsha_18 = 1018,
        [Description("إذن مرتجع صرف من الورشة"), Display(Name = "إذن مرتجع صرف من الورشة")] MortagaSarfFromWarsha_19 = 1019,
        [Description("إذن صرف لموظف"), Display(Name = "إذن صرف لموظف")] SarfToEmployee_20 = 1020,
        [Description("إذن إستلام من موظف"), Display(Name = "إذن إستلام من موظف")] EstlamFromEmployee_21 = 1021,
        [Description("إذن صرف قطع الغيار للماكينات وخطوط الإنتاج"), Display(Name = "إذن صرف قطع الغيار للماكينات وخطوط الإنتاج")] SarfToMachine_22 = 1022,
        [Description("إذن مرتجع صرف قطع الغيار للماكينات وخطوط الإنتاج"), Display(Name = "إذن مرتجع صرف قطع الغيار للماكينات وخطوط الإنتاج")] MortagaSarfFromMachine_23 = 1023,
        
 

    }

    public enum WorkOrderStatus : int
    {
        [Description("مفتوح")]
        [Display(Name = "مفتوح")]
        Open = 0,

        [Description("تحت التشغيل")]
        [Display(Name = "تحت التشغيل")]
        InProgress = 1,

        [Description("مكتمل")]
        [Display(Name = "مكتمل")]
        Completed = 2,

        [Description("مغلق")]
        [Display(Name = "مغلق")]
        Closed = 3
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