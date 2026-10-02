using Microsoft.Data.SqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Drawing;
 
using System.IO;
using System.Text;
namespace AccountModelsLib.Models
{
    /*
    public class FixedAssetData
	{
      
		public decimal Serial { get; set; }
		public long Id { get; set; }
		public string Code { get; set; }
		public string Name_Ar { get; set; }
		public string Name_En { get; set; }
		public DateTime? Purchase_Date { get; set; }
		public int Account_Ehlak_Status { get; set; }
		public string Account_Ehlak_Status_Str { get; set; }
		public long Emp_Id { get; set; }
		public DateTime? Emp_Receive_Date { get; set; }
		public string Model { get; set; }
		public string Industry_Factory { get; set; }
		public string Fixed_Serial_No { get; set; }
		public string Location { get; set; }
		public DateTime? Factory_warrantyFinal_Date { get; set; }
		public string Supplier_Name { get; set; }
		public string Factory_warranty_Details { get; set; }
		public int Account_Ehlak_Type { get; set; }
		public string Account_Ehlak_Type_Str { get; set; }
		public decimal Main_Value { get; set; }
		public decimal Accumulated_Depreciation_Value { get; set; }
		public decimal Rate_of_depreciation { get; set; }
		public decimal Book_value { get; set; }
		public DateTime? beginning_service_Date { get; set; }
		public decimal Price_scrap_Value { get; set; }
		public decimal Last_Modified_Value { get; set; }
		public decimal Default_Age { get; set; }
		public long Branch_Id { get; set; }
		public decimal SubAccount_Id { get; set; }
		public string Account_Code { get; set; }
		public DateTime? OperationDate { get; set; }
		public long User_Id { get; set; }
		public DateTime? Insert_ServerDateTime { get; set; }
		public DateTime? Update_ServerDateTime { get; set; }
		public DateTime? EditOperationDate { get; set; }
		public long Edit_User_Id { get; set; }
		public int Active { get; set; }
	}
    */

 
    [Table("FixedAssetData")]
    public class FixedAssetData
    {
        [Key]
        public long  Id { get; set; } 
        public string? Code { get; set; }

         
        public string? Name_Ar { get; set; }

         
        public string? Name_En { get; set; }

        public DateTime? Purchase_Date { get; set; }

        public int? Account_Ehlak_Status { get; set; }

         
        public string? Account_Ehlak_Status_Str { get; set; }

        public long? Emp_Id { get; set; }

        public DateTime? Emp_Receive_Date { get; set; }

         
        public string? Model { get; set; }

         
        public string? Industry_Factory { get; set; }

         
        public string? Fixed_Serial_No { get; set; }

         
        public string? Location { get; set; }

        public DateTime? Factory_warrantyFinal_Date { get; set; }

        
        public string? Supplier_Name { get; set; }

         
        public string? Factory_warranty_Details { get; set; }

        public long? Account_Ehlak_Type { get; set; }

         
        public string? Account_Ehlak_Type_Str { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Main_Value { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Accumulated_Depreciation_Value { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Rate_of_depreciation { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Book_value { get; set; }

        public DateTime? beginning_service_Date { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Price_scrap_Value { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Last_Modified_Value { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Default_Age { get; set; }

        public long? Branch_Id { get; set; }

        public long? SubAccount_Id { get; set; }

        [StringLength(500)]
        public string? Account_Code { get; set; }

        public DateTime? OperationDate { get; set; }

        public long? User_Id { get; set; }

        public DateTime? Insert_ServerDateTime { get; set; }

        public DateTime? Update_ServerDateTime { get; set; }

        public DateTime? EditOperationDate { get; set; }

        public long? Edit_User_Id { get; set; }

        public int? Active { get; set; }

        public long? ProcessNo { get; set; }
    }
}
