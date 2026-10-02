using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models 
{
    
        [Table("Currency")]
        public class Currency
        {
            [Key]
            
            public long Id { get; set; } 
            public string? Currency_Name { get; set; }

           
            public string? Currency_Name_EN { get; set; }

            public int Value { get; set; }

            public int Active { get; set; } = 1;

            public int? IS_GL_Currency_Flage { get; set; } = 0;

            public DateTime? OperationDate { get; set; } = DateTime.Now;

            public long? User_Id { get; set; } = 0;

            public long? Branch_Id { get; set; } = 1;

            public DateTime? Insert_ServerDateTime { get; set; } = DateTime.Now;

            public DateTime? Update_ServerDateTime { get; set; } = DateTime.Now;

            public DateTime? EditOperationDate { get; set; }

            public long? Edit_User_Id { get; set; } = 0;

            public long? ProcessNo { get; set; } = 0;

            [StringLength(500)]
            public string? code { get; set; }

            public decimal? rate { get; set; } = 1.00m;

            [StringLength(500)]
            public string? Desc_en { get; set; }

            [StringLength(500)]
            public string? Desc_ar { get; set; }
        }
    
}
