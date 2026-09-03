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
    [Table("Order_Money")]
    public class Order_Money
    {
      

        public long  Id { get; set; }
        public long? MainOrder_MoneyId { get; set; }
        
        //public long Serial { get; set; }
        public long  Order_No { get; set; }
        public DateTime Order_Date { get; set; }
        public int  Transaction_type { get; set; }
        public decimal  Total_Value { get; set; }
        public int  Currency_Id { get; set; }
        public int? Bank_Type { get; set; }
        public long  Bank_Id { get; set; } = 0;
        public string? Man_Name { get; set; }
        public int? Is_Balances_Set { get; set; }
        public int? Day_Registration_flage { get; set; }
        public long? DayReg_List_Id { get; set; }
        public int? Order_mony_id_transform { get; set; }
        public long? Invoice_Id { get; set; }
        public long? User_Id { get; set; }
        public long? Branch_Id { get; set; }
        public long? Year_Financial_Period_Id { get; set; }
        public long? Ticket_Supplier_Id { get; set; }
        public string? Order_Tafkeet_Value { get; set; }
        public string? Order_Notes { get; set; }
        public string? Tafkeet_Value { get; set; }
        public long? Sandouk_Cost_Center_Tree_Id { get; set; }
        public int? IS_Posted_Flage { get; set; }
        public string? Entry_Currency_Name { get; set; }
        public decimal? Factor { get; set; }
        public int? Entry_Currency_ID { get; set; }
        public long? Account_Entry_List_ID { get; set; } = 0;
        public int? Invoice_Type { get; set; }
        public DateTime? OperationDate { get; set; }
        public DateTime? Insert_ServerDateTime { get; set; }
        public DateTime? Update_ServerDateTime { get; set; }
        public DateTime? EditOperationDate { get; set; }
        public long? Edit_User_Id { get; set; } =0;
        public long? ProcessNo { get; set; }
        public long? CarsWorkOrdersId { get; set; }
        public int? Reverse_Order { get; set; }
        public int? Account_Type { get; set; }
        public long? Account_Id { get; set; }
        public decimal? RequirementValue { get; set; }
        public decimal? StillValue { get; set; }
        public int? IsMoreTheOneBoxShow { get; set; } = 0;
        public long? Reference_Type_Table_Id { get; set; } = 0;
        public long? DataDictionaryProcessesListsId { get; set; }

        public decimal? paidTotal;

        [ForeignKey("Bank_Id")]
        public virtual Bank Bank { get; set; }


        

    }
}
