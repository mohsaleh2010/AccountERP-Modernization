
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{
    
    [Table("Machin_list")]
    public class Machin_list
    {
        
        
        public long Id { get; set; }

        public string? Code { get; set; }
        public decimal? Speed { get; set; }
        public string Machin_Name { get; set; }

        
        public decimal? price_of_nazlla { get; set; }

        public DateTime? OperationDate { get; set; }

        public long? User_Id { get; set; }

        public long? Branch_Id { get; set; }

        public DateTime? Insert_ServerDateTime { get; set; }

        public DateTime? Update_ServerDateTime { get; set; }

        public DateTime? EditOperationDate { get; set; }

        public long? Edit_User_Id { get; set; }

        public long? ProcessNo { get; set; }

        public int? Active { get; set; }


        public int? Machine_Width { get; set; }

        public string? Description { get; set; }

        public decimal? Abstract_Hadafat { get; set; }

         
        //Invalid column name 'User_Id'.
        //Invalid column name 'Branch_Id'.

        //Invalid column name 'EditUserId'.

    }

    [Table("Machin_list_Textile")]
    public class Machin_list_Textile
    {
        public long Id { get; set; }

        public string Machin_Name { get; set; } = string.Empty;

        public decimal? price_of_nazlla { get; set; }

        public string Code { get; set; }

        public string Description { get; set; }

        public decimal Speed { get; set; }

        public int? Machine_Width { get; set; }

        public decimal? Abstract_Hadafat { get; set; }

        public int? Machin_Type { get; set; }

        public int? Machine_Id { get; set; }

        public decimal? Price_Day_Alf_Hadfa { get; set; }

        public decimal? Machine_Stitch_No { get; set; }

        public decimal? Atala_efficiency { get; set; }

        public DateTime? OperationDate { get; set; }

        public int? User_Id { get; set; }

        public int? Branch_Id { get; set; }

        public DateTime? Insert_ServerDateTime { get; set; }

        public DateTime? Update_ServerDateTime { get; set; }

        public DateTime? EditOperationDate { get; set; }

        public int? Edit_User_Id { get; set; }

        public long? ProcessNo { get; set; }
    }
}
