using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountModelsLib.Models
{
    [Table("Money_Transaction_BoxForInvoices")]
    public class Money_Transaction_BoxForInvoice
    {
        
        [Key]
        public long  Id { get; set; } 
        public long Money_Transaction_BoxId { get; set; }
        public long? Order_Mony_Id { get; set; }
        public long? Order_No { get; set; }
         
        public int? Invoice_TypeId { get; set; }
         
        public long? Bank_Id { get; set; }
         
        public long? Invoices_ListId { get; set; }
        public long? Bills_ListId { get; set; }

        public long? Reference_Type_Table_Id { get; set; }
        public long? DataDictionaryProcessesListsId { get; set; }
        public decimal Transaction_Value { get; set; }
   
        public DateTime? Transaction_Date { get;   set; }
        public decimal? Import_Value { get;   set; }
        public decimal? Export_Value { get;   set; }
        public long Customer_Id { get; set; }
        public long? Supplier_Id { get;   set; }
        public long? SubAccount_Id { get;   set; }
        public int? Transaction_Type { get;   set; }
        public decimal? Order_Total_Value { get; set; }

        // Navigation Properties
        [ForeignKey("Money_Transaction_BoxId")]
        public virtual Money_Transaction_Box money_Transaction_Box { get; set; }

        [ForeignKey("Invoices_ListId")]
        public virtual Invoices_List invoices_List { get; set; }

        
        [ForeignKey("Bills_ListId")]
        public virtual Bills_List bills_List { get; set; }
       
    }
}
