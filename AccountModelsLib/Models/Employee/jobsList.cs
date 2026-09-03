using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{
    [Table("jobsList")]
    public class jobsList
    {
        public long id { get; set; }
        public string jobname { get; set; }
        public DateTime? OperationDate { get; set; }
        public int? User_Id { get; set; }
        public int? Branch_Id { get; set; }
        public DateTime? Insert_ServerDateTime { get; set; }
        public DateTime? Update_ServerDateTime { get; set; }
        public DateTime? EditOperationDate { get; set; }
        public int? Edit_User_Id { get; set; }
        public long ProcessNo { get; set; }
    }
}
