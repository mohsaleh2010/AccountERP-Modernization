
namespace AccountModelsLib.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;


    [Table("DataDictionaryLogerExceptions")]
    public class DataDictionaryLogerException
    {
        public long Id { get; set; }
        public string Exception { get; set; }
        public string Message { get; set; }
        public string InnerException { get; set; }
        public string Source { get; set; }
        public string StackTrace { get; set; }
        public string MethodName { get; set; }
        public DateTime ExcpDateTime { get; set; }
    }
}
