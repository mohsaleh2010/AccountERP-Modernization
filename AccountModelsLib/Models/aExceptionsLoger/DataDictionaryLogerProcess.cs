
 

namespace AccountModelsLib.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;


    [Table("DataDictionaryLogerProcesses")]
    public class DataDictionaryLogerProcess
    {
        internal string ProcessNotes;

        public long Id { get; set; }
        public long ProcessId { get; set; }
        public string ProcessName { get; set; }
        public long SubProcessId { get; set; }

        public string SubProcessName { get; set; } 
        public long ProcessDocNo { get; set; }
        public long ProcessTableId { get; set; }
        public long User_Id { get; set; }
        public long Branch_Id { get; set; }
        public string UserName { get; set; }

        public DateTime ProcessDateTime { get; set; }
        public string Item_Name { get; internal set; }
        public string Qnty { get; internal set; }
    }
}
 