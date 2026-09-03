// AccountERP.Models/Menus/SysConfigMenus.cs
using AccountModelsLib.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountModelsLib.Models
{


    [Table("USER_DETAILES_Branches_Permission")]
    public class USER_DETAILES_Branches_Permission
    {
        [Key]
        public long ID { get; set; }

        public long? USER_DETAILES_Id { get; set; }

        public long? Branch_Id { get; set; }

        public DateTime? OperationDate { get; set; }

        public int? User_Id { get; set; }

       

        public DateTime? Insert_ServerDateTime { get; set; }

        public DateTime? Update_ServerDateTime { get; set; }

        public DateTime? EditOperationDate { get; set; }

        public int? Edit_User_Id { get; set; }

        public long? ProcessNo { get; set; }
    }
}