// AccountERP.Models/Menus/SysConfigMenus.cs
using AccountModelsLib.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountModelsLib.Models
{


    [Table("DataDictionaryDbSyncLogs")]
    public class DataDictionaryDbSyncLogs
    {
        [Key]
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; } 
        public string TableName { get; set; } = string.Empty;

        public long RecordId { get; set; }


        
        public string? RecordIdStr { get; set; } = string.Empty; //  Where
        public string ActionType { get; set; } = string.Empty; // 'INSERT', 'UPDATE', 'DELETE'

        // العمود الجديد لتقييد الإجراء الثاني لنفس السجل
        
        public string? ActionType2 { get; set; } // 'UPDATE', 'DELETE' أو null
        public string? payload { get; set; } // 'UPDATE', 'DELETE' أو null
        public string? mysqlCmd { get; set; } // 'UPDATE', 'DELETE' أو null

        
        public string Status { get; set; } = "Pending";
        public int IsStatus { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? ProcessedAt { get; set; }

        public int RetryCount { get; set; } = 0;

        public string? LastError { get; set; }


        public long User_Id { get; set; }
        public long Branch_Id { get; set; }
        public int Is_EF_Or_Adonet { get; set; } // 1 للـ EF Core، و 2 للـ ADO.NET
    }
}