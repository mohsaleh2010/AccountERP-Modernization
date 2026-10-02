// AccountERP.Models/Menus/SysConfigMenus.cs
 
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountModelsLib.Models
{
    
    [Table("SysConfigMenus")]
    public class SysConfigMenus
    {
        [Key] 
        public long Id { get; set; } 
        public long Parent_Id { get; set; } 
        public string   KeysData { get; set; } 
        public string  Description { get; set; } 
        public int PROJECT_ID { get; set; }
        public int? IsShowToUser { get; set; }   = 1;

        public string  Value { get; set; }

       
        public string  DefaultValue { get; set; }

       
        public string  DefaultDescription { get; set; }

        // ── أعلام الصلاحيات لكل نوع مشروع ──

      
        public string  POS { get; set; }

        
        public string  Market { get; set; }

       
        public string  Factory { get; set; }

       
        public string  Tourism { get; set; }

      
        public string  AlFawzan { get; set; }

       
        public string  Client_A { get; set; }

        
        public string  Client_B { get; set; }

       
        public string  UserSells { get; set; }

        // 🟢 الترتيب الافتراضى لظهور القائمة (يضبطه الأدمن) - يُطبَّق على كل المستخدمين الذين لم يخصّصوا
        // ترتيباً شخصياً لهذا المستوى بعد. NULL يعنى "لم يُرتَّب بعد" فيُستخدَم Id كترتيب احتياطى.
        public int? Sort_Order { get; set; }

        public List<SysConfigMenusPERMATIONS> sysConfigMenuPermissions { get; set; }
    }
   
    [Table("SysConfigMenusPERMATIONS")]
    public class SysConfigMenusPERMATIONS
    {
        public long Id { get; set; }

        public long  MenuId { get; set; }

        public long? ParentId { get; set; }

        public long? UserId { get; set; }

        public string Permission { get; set; }

        public int? ProjectId { get; set; }

        public DateTime? OperationDate { get; set; }

        public int? BranchId { get; set; }

        public DateTime? InsertServerDateTime { get; set; }

        public DateTime? UpdateServerDateTime { get; set; }

        public DateTime? EditOperationDate { get; set; }

        public int? EditUserId { get; set; }

        public long? ProcessNo { get; set; }

        // 🟢 الترتيب الشخصى الذى يضبطه هذا المستخدم بنفسه لهذه القائمة (يتغلّب على Sort_Order الافتراضى
        // فى SysConfigMenus لهذا المستخدم فقط). NULL يعنى "لم يُخصِّص بعد" - راجع MenuRepository.GetUserAllowedMenus
        // لمنطق التفضيل الكامل (بما فى ذلك: أى صلاحية جديدة تُمنح للمستخدم تظهر تلقائياً فى آخر ترتيبه الشخصى).
        public int? Sort_Order { get; set; }

        [ForeignKey("MenuId")]
        public SysConfigMenus sysConfigMenus { get; set; }
    }

    [Table("SysConfig")]
    public class SysConfig
    {
        [Key]
        
        public long Id { get; set; }

        [StringLength(100)]
        public string? KeysData { get; set; }

        [StringLength(500)]
        public string? DefaultValue { get; set; }

        [StringLength(500)]
        public string? Value { get; set; }

        [StringLength(500)]
        public string? DefaultDescription { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(500)]
        public string? Client_B { get; set; }

        [StringLength(500)]
        public string? Client_A { get; set; }

        [StringLength(500)]
        public string? POS { get; set; }

        [StringLength(500)]
        public string? Market { get; set; }

        [StringLength(500)]
        public string? Factory { get; set; }

        [StringLength(500)]
        public string? AlFawzan { get; set; }

        [StringLength(500)]
        public string? Tourism { get; set; }

        public long? ProcessNo { get; set; }
    }

}