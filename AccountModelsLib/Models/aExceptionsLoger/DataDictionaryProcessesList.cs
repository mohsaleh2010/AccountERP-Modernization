
 

namespace AccountModelsLib.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;


    [Table("DataDictionaryProcessesLists")]
    public class DataDictionaryProcessesList
    {
        public long Id { get; set; }
       
        public string ProcessName { get; set; }
        

        
    }
}
 