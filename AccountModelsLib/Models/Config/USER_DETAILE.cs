// AccountERP.Models/Menus/SysConfigMenus.cs
using AccountModelsLib.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountModelsLib.Models
{
    
 

    [Table("USER_DETAILES")]
    public class USER_DETAILE
    {
        [Key]
        [Column("Id")]
        public long Id { get; set; }

        public string Name { get; set; }

        [Column("Emp_Id")]
        public long? Emp_Id { get; set; }

        [Required]
        [Column("USER_NAME")]
        [StringLength(50)]
        public string USER_NAME { get; set; }

        [Required]
        [Column("USER_PASSWORD")]
        [StringLength(500)]
        public string USER_PASSWORD { get; set; }

        [Column("Branch_Id")]
        public long? Branch_Id { get; set; }

        public int? AdminFlage { get; set; }

        public int? Account_Proj { get; set; }

        public int? HR_Proj { get; set; }

        public int? Store_Proj { get; set; }

        public int? Production_Proj { get; set; }

        public int? Gawda_Proj { get; set; }

         

        public bool? Active { get; set; }

        public DateTime? OperationDate { get; set; }

        public DateTime? Insert_ServerDateTime { get; set; }

        public DateTime? Update_ServerDateTime { get; set; }

        public DateTime? EditOperationDate { get; set; }

        [Column("Edit_User_Id")]
        public long? Edit_User_Id { get; set; }

        public long? ProcessNo { get; set; }
    }
}