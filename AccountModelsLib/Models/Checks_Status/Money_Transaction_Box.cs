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
    [Table("Money_Transaction_Box")]
    public class Money_Transaction_Box
    {

        public long Id { get; set; }
        //public long Serial { get; set; }
        public long Order_Mony_Id { get; set; }
        public long? SubOrder_Money_Id { get; set; }

        public DateTime? Transaction_Date { get; set; }
        public int? Transaction_Type { get; set; }
        public decimal? Import_Value { get; set; }
        public decimal? Export_Value { get; set; }
        public decimal? Transaction_Value { get; set; }
        public string Transaction_Value_ { get; set; }
        public decimal? Equivalent_Value { get; set; }
        public long Bank_Id { get; set; }
        public decimal? Bank_Balance { get; set; }
        public long Supplier_Id { get; set; }
        public long Customer_Id { get; set; }
        public long Account_Id { get; set; }
        public long SubAccount_Id { get; set; }
        public string Notes { get; set; }
        public decimal? Balance_Before { get; set; }
        public decimal? Balance_After { get; set; }
        public long? BankAccount_Id { get; set; }
        public long? BankSubAccount_Id { get; set; }
        public int? IsDefault_Bank { get; set; }
        public long? Account_Entry_List_ID { get; set; }
         
        public int? DestructionSales { get; set; }
        public long? Order_mony_id_transform { get; set; }
        public long? Account_Cost_Center_Tree_Id { get; set; }
        public int? User_Id { get; set; }
        public int? Branch_Id { get; set; }
        public string Tafkeet_Value { get; set; }
        public decimal? TotalValueWillPaid { get; set; }
        public decimal? Rest_Value { get; set; }
        public string Account_Name { get; set; }
        public string Account_Code { get; set; }
        public int? Account_Type { get; set; }
        public string Cost_Center { get; set; }
        public int? Currency_Id { get; set; }
        public decimal? Purchase_Ticket_Value { get; set; }
        public decimal? Commission_Value { get; set; }
        public decimal? Factor { get; set; }
        public int? Entry_Currency_ID { get; set; }
        public string CostCenter_Name { get; set; }
        public DateTime? OperationDate { get; set; }
        public DateTime? Insert_ServerDateTime { get; set; }
        public DateTime? Update_ServerDateTime { get; set; }
        public DateTime? EditOperationDate { get; set; }
        public int? Edit_User_Id { get; set; }
        public long ProcessNo { get; set; }
        public int? Invoice_TypeId { get; set; }
        public long? Invoice_Id { get; set; }
        public string Balance_BeforStr { get; set; }
        public string Balance_AfterStr { get; set; }
        public long? CarsWorkOrdersId { get; set; }
        public long? Reference_Type_Table_Id { get; set; }
        public long? DataDictionaryProcessesListsId { get; set; }
        
        public long? Lookup_CashPaymentMethodId { get; set; }
        public string Lookup_CashPaymentMethodName { get; set; }
        public long? Order_No { get; set; }
        public string Bank_Name { get; set; } 
        public long? CarsId { get; set; }
        
        public string TaxInternalId { get; set; }
        public string CarNumber { get; set; }
        public long? WorkOrderNumber { get; set; }
        public int? Is_ProcessedFlage { get; set; } = 0;
    }
}
