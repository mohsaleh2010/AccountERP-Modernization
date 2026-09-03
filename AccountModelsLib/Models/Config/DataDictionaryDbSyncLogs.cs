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

         
        [StringLength(128)]
        public string TableName { get; set; } = string.Empty;

        public long RecordId { get; set; }

         
        [StringLength(10)]
        public string ActionType { get; set; } = string.Empty; // 'INSERT', 'UPDATE', 'DELETE'

        // العمود الجديد لتقييد الإجراء الثاني لنفس السجل
        [StringLength(10)]
        public string ActionType2 { get; set; } // 'UPDATE', 'DELETE' أو null
        public string payload { get; set; } // 'UPDATE', 'DELETE' أو null
        public string mysqlCmd { get; set; } // 'UPDATE', 'DELETE' أو null

        [StringLength(10)]
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? ProcessedAt { get; set; }

        public int RetryCount { get; set; } = 0;

        public string LastError { get; set; }
    }
}