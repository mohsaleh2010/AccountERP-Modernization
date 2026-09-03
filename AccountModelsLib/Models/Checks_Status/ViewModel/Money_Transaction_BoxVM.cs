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
    [NotMapped]
    public class Money_Transaction_BoxVM     
    {
        public long Order_Mony_Id { get; set; } = 0;
        public long Money_Transaction_Box_ID { get; set; } = 0;
        public int Transaction_Type { get; set; } = 0;
        public string Order_No { get; set; } = "";
        public long Box_Id { get; set; } = 0;
        public decimal Box_Balance { get; set; } = 0;
        public decimal Cach_Value { get; set; } = 0;
        public DateTime Order_Date { get; set; } = DateTime.Now;
        public string Man_Name { get; set; } = "";
        public int Currency_Id { get; set; } = 0;
        public long Customer_ID_Supplier { get; set; } = 0;
        public int Reverse_Order { get; set; } = 0;
        public long Entry_No { get; set; } = 0;
        public long CarsWorkOrdersId { get; set; } = 0;
        public long Reference_Type_Table_Id { get; set; } = 0;
        public long DataDictionaryProcessesListsId { get; set; } = 0;
        public long SubAccount_Id { get; set; } = 0;
        public long Account_Customer_Type_ID { get; set; } = 0;
        public long Account_Customer_Type_Value { get; set; } = 0;
        public long Invoice_No { get; set; } = 0;
        public long Invoice_Id { get; set; } = 0;
        //public decimal PaidValueFromMarket { get; set; } = 0;
        public decimal RestValue { get; set; } = 0;
       
         
        public List<Invoices_List> InvoicesLst { get; set; } = new List<Invoices_List>(); 
    }
}