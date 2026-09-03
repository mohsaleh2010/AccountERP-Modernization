using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using Microsoft.Data.SqlClient;

using System.IO;
using System.ComponentModel.DataAnnotations.Schema;
namespace AccountModelsLib.Models
{
    [Table("Banks")]
    public class Bank
    {
        public long Id { get; set; }
        public string  Bank_Name { get; set; }
        public string? Bank_Name_En { get; set; }
        public decimal? Primary_balance { get; set; }
        public decimal? Last_Balance { get; set; }
        public string?  Account_No { get; set; }
        public long SubAccount_Id { get; set; }
        public int? Is_Bank { get; set; }
        public int? Currency_Id { get; set; }
        public DateTime? firstBalanceDate { get; set; }
        public int? Supplier_Bank { get; set; }
        public int? Default_Bank { get; set; }
        public string?  Account_Code { get; set; }
        public int? Static_Flage { get; set; }
        
        public int? Account_Type_Value { get; set; }
        public int? CompanyLogoType { get; set; }
        public DateTime? OperationDate { get; set; }
        public long? User_Id { get; set; }
         
        public long? Branch_Id { get; set; }
        public string? Branch_Name { get; set; }
        public DateTime? Insert_ServerDateTime { get; set; }
        public DateTime? Update_ServerDateTime { get; set; }
        public DateTime? EditOperationDate { get; set; }
        public int? Edit_User_Id { get; set; }
        public int? Active { get; set; }
        public long? ProcessNo { get; set; } = 0;
    }
}
