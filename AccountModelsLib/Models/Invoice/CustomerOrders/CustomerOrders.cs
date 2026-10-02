namespace AccountModelsLib.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public enum CustomerOrderStatus
    {
        Draft = 0,
        Confirmed = 1,
        PartiallyFulfilled = 2,
        Fulfilled = 3,
        Cancelled = 4,
        Closed = 5
    }

    public enum CustomerOrderLineStatus
    {
        Open = 0,
        Partial = 1,
        Fulfilled = 2,
        Cancelled = 3
    }

    [Table("Customer_Orders")]
    public class CustomerOrder
    {
        [Key]
        public long Id { get; set; }
        public long? Order_No { get; set; }
        public DateTime? Order_Date { get; set; }
        public DateTime? Expected_Delivery_Date { get; set; }
        public long? Customer_Id { get; set; }
        public long? Store_Id { get; set; }
        public long? Currency_Id { get; set; }
        public long? Sales_Person_Id { get; set; }
        public int? Status { get; set; }
        public decimal? Gross_Total { get; set; }
        public decimal? Discount_Total { get; set; }
        public decimal? Tax_Total { get; set; }
        public decimal? Net_Total { get; set; }
        public string? Notes { get; set; }
        public string? Cancel_Reason { get; set; }
        public long? Cancelled_By { get; set; }
        public DateTime? Cancelled_Date { get; set; }
        public DateTime? OperationDate { get; set; }
        public long? User_Id { get; set; }
        public long? Branch_Id { get; set; }
        public DateTime? Insert_ServerDateTime { get; set; }
        public DateTime? Update_ServerDateTime { get; set; }
        public DateTime? EditOperationDate { get; set; }
        public long? Edit_User_Id { get; set; }
        public long? ProcessNo { get; set; }
    }

    [Table("Customer_Order_Lines")]
    public class CustomerOrderLine
    {
        [Key]
        public long Id { get; set; }
        public long Order_Id { get; set; }
        public int? Line_No { get; set; }
        public long? Unit_Item_Table_Id { get; set; }
        public decimal? Qty_Larg { get; set; }
        public decimal? Qty_Small { get; set; }
        public decimal? Unit_Price { get; set; }
        public decimal? Discount_Percent { get; set; }
        public decimal? Discount_Value { get; set; }
        public decimal? Tax_Value { get; set; }
        public decimal? Line_Total { get; set; }
        public decimal? Fulfilled_Qty_Larg { get; set; }
        public decimal? Fulfilled_Qty_Small { get; set; }
        public decimal? Cancelled_Qty_Larg { get; set; }
        public decimal? Cancelled_Qty_Small { get; set; }
        public int? Line_Status { get; set; }
        public string? Notes { get; set; }
        public DateTime? OperationDate { get; set; }
        public long? User_Id { get; set; }
        public long? Branch_Id { get; set; }
        public DateTime? Insert_ServerDateTime { get; set; }
        public DateTime? Update_ServerDateTime { get; set; }
        public DateTime? EditOperationDate { get; set; }
        public long? Edit_User_Id { get; set; }
        public long? ProcessNo { get; set; }
    }

    [Table("Customer_Order_Deposits")]
    public class CustomerOrderDeposit
    {
        [Key]
        public long Id { get; set; }
        public long Order_Id { get; set; }
        public long? Money_Transaction_Box_Id { get; set; }
        // معرّف رأس الإذن Order_Money.Id (الفريد). Money_Transaction_Box.Id غير فريد فى القاعدة فلا يصلح وحده للربط.
        public long? Order_Money_Id { get; set; }
        public decimal? Amount { get; set; }
        public decimal? Consumed_Amount { get; set; }
        public DateTime? Deposit_Date { get; set; }
        public string? Notes { get; set; }
        public DateTime? OperationDate { get; set; }
        public long? User_Id { get; set; }
        public long? Branch_Id { get; set; }
        public DateTime? Insert_ServerDateTime { get; set; }
        public DateTime? Update_ServerDateTime { get; set; }
        public DateTime? EditOperationDate { get; set; }
        public long? Edit_User_Id { get; set; }
        public long? ProcessNo { get; set; }
    }

    [Table("Customer_Order_Fulfillments")]
    public class CustomerOrderFulfillment
    {
        [Key]
        public long Id { get; set; }
        public long Order_Line_Id { get; set; }
        public long? Invoices_List_Id { get; set; }
        public long? Invoices_Details_Id { get; set; }
        public decimal? Qty_Larg { get; set; }
        public decimal? Qty_Small { get; set; }
        public DateTime? Fulfill_Date { get; set; }
        public DateTime? OperationDate { get; set; }
        public long? User_Id { get; set; }
        public long? Branch_Id { get; set; }
        public DateTime? Insert_ServerDateTime { get; set; }
        public DateTime? Update_ServerDateTime { get; set; }
        public DateTime? EditOperationDate { get; set; }
        public long? Edit_User_Id { get; set; }
        public long? ProcessNo { get; set; }
    }
}
