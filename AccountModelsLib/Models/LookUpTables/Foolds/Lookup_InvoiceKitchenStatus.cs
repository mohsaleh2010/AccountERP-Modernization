using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountModelsLib.Models
{

    /// <summary>
    /// موديل جدول الحالات الموحدة للفاتورة والتجهيز داخل المطبخ
    /// </summary>
    [Table("Lookup_InvoiceKitchenStatus")]
    public partial class Lookup_InvoiceKitchenStatus
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        
        public string  Name  { get; set; }

    
      
        public string  Name_En { get; set; }
    }

    /// <summary>
    /// موديل جدول فترات العمل والورديات
    /// </summary>
    [Table("Lookup_Shifts")]
    public partial class Lookup_Shift
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        
        public string Name { get; set; }

        
       
        public string Name_En { get; set; }

         
        public TimeSpan Start_Time { get; set; } // يطابق النوع TIME(7) في SQL Server

      
        public TimeSpan End_Time { get; set; }
    }

    

    /// <summary>
    /// موديل جدول أنواع الطلبات (صالة، سفري، توصيل) والرسوم الافتراضية الخاصة بها
    /// </summary>
    [Table("Lookup_OrderDeliveryTypes")]
    public partial class Lookup_OrderDeliveryTypes
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
       
        public string Name{ get; set; }

        
       
        public string Name_En { get; set; }

        
        
        public decimal Default_Service_Rate { get; set; } = 0.00m; // نسبة الخدمة المئوية للصالة (مثل 10.00)

         
       
        public decimal Default_Delivery_Fee { get; set; } = 0.00m;  // رسوم التوصيل الثابتة (مثل 15.00)
    }


    /// <summary>
    /// 4. جدول أنواع الخصومات (Discount Types)
    /// </summary>
    [Table("Lookup_DiscountTypes")]
    public partial class Lookup_DiscountTypes
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

         
        public string Name { get; set; } // مثل: خصم موظفين، ضيافة، عرض خاص

         
        public string Name_En { get; set; }
    }

    /// <summary>
    /// 5. جدول طاولات الصالة (Tables)
    /// </summary>
    [Table("Lookup_Tables")]
    public partial class Lookup_Table
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        
        public string Name { get; set; } // مثل: طاولة 1، طاولة 2، عائلات 1

        
        public string  Name_En { get; set; }

        public bool Is_Active { get; set; } = true; // حالة الطاولة (متاحة للعمل أم مغلقة للصيانة)
    }

}