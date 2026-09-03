using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.ViewModelPrint
{
    

    public class Fist_BalanceVMPrint
    {
        public decimal First_Balance_Madin { get; set; }
        public decimal First_Balance_Da2n { get; set; }
        public decimal Equivalent_First_Balance_Madin { get; set; }
        public decimal Equivalent_First_Balance_Da2n { get; set; }
        // Aliased Columns
        public decimal Final_Fist_Balance_Madin { get; set; }
        public decimal Final_Fist_Balance_Da2n { get; set; }
        public decimal Final_Transaction_Madin { get; set; }
        public decimal Final_Transaction_Da2n { get; set; }

        // Primary & Key Fields
        public long Id { get; set; }
        public long Submain_Account_ID { get; set; }
        public string SubAccount_Name { get; set; }
        public long Year_Financial_Period_Id { get; set; }
        public long Branch_Id { get; set; }
        public int Account_Currency_Id { get; set; }
        public int Equivalent_Account_Currency_Id { get; set; }

        // Numeric & Balance Fields (Mezan)
        public decimal First_Balance { get; set; }
        public decimal Equivalent_First_Balance { get; set; }
     
        public decimal Madin_Value { get; set; }
        public decimal Da2n_Value { get; set; }
        public decimal Balance { get; set; }
        public decimal Balance_Madin { get; set; }
        public decimal Balance_Da2n { get; set; }

        // Equivalent Values
     
        public decimal Equivalent_Madin_Value { get; set; }
        public decimal Equivalent_Da2n_Value { get; set; }
        public decimal Equivalent_Balance_Madin { get; set; }
        public decimal Equivalent_Balance_Da2n { get; set; }

        // Operation & Audit Fields
        public int IS_Madin_Da2n { get; set; }
        public long? Cost_Center_Tree_ID { get; set; }
        public DateTime OperationDate { get; set; }
        public long User_Id { get; set; }
        public DateTime? Insert_ServerDateTime { get; set; }
        public DateTime? Update_ServerDateTime { get; set; }
        public DateTime? EditOperationDate { get; set; }
        public long? Edit_User_Id { get; set; }
        public decimal? Factor { get; set; }
        public long? ProcessNo { get; set; }

        // Joined Fields (Submain_Account)
        public long? Account_Customer_Type_ID { get; set; }
        public int Account_Customer_Type_Value { get; set; }
        public string Account_Name { get; set; }
        public string? Account_Code { get; set; }

        // Joined Fields (MainSubmain_Account)
        public string MainCode { get; set; }
        public string MainAccount_Name { get; set; }

        // Currency Fields (Evaluated CASE Expression)
        public string? Currency_Name { get; set; }
        public string? Currency_Name_EN { get; set; }
    }
}
