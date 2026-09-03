using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.ComponentModel.DataAnnotations.Schema;
namespace AccountModelsLib.Models
{




    [Table("Checks_MemoryMainDataList")]
    public class Checks_MemoryMainDataList
    {
        public long Id { get; set; }
        public string  Check_No { get; set; }
        public DateTime? Day_Date { get; set; }
        public DateTime? Due_Date { get; set; }
        public long? Bank_id { get; set; }
        public decimal? Check_Value { get; set; }
        public string  Check_Value_ { get; set; }
        public string  Due_Check { get; set; }
        
        
        public string  Exchanger_Name { get; set; }
        public string  First_Drawer_Name { get; set; }
        public string  Last_Drawer_Name { get; set; }
        public string  Bank_Name { get; set; }
        public string  Check_Notes { get; set; }
    }
}
