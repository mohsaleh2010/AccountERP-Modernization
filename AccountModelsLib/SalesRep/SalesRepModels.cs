using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountModelsLib.SalesRep
{
    // جداول موديول المندوبين — مطابقة حرفياً لـ SalesRepSchemaCLS (field-sales-van-presales-plan.md §5.1).

    public enum SalesRepSalesMode { Van = 0, PreSales = 1, Mixed = 2, DeliveryOnly = 3 }

    /// <summary>ق9: المتبقى آخر اليوم — يبقى عهدة (افتتاحى الوردية التالية) أو يُفرَّغ للرئيسى.</summary>
    public enum SalesRepEndOfDayStockPolicy { KeepAsCustody = 0, Unload = 1 }

    public enum SalesRepShiftStatus { Open = 1, Closing = 2, Closed = 3, Reopened = 4 }

    public enum SalesRepDocType { SaleInvoice = 1, SaleReturn = 2, CashVoucher = 3, StoreOrder = 4, CustomerOrder = 5, JournalEntry = 6, Cheque = 7 }

    public enum SalesRepVisitResult { Sale = 1, Delivery = 2, Order = 3, Collection = 4, Return = 5, NoSale = 6 }

    public enum SalesRepApprovalType { Credit = 1, Discount = 2, BlindReturn = 3, OutsideGeofence = 4, OutsideReturnWindow = 5, ReopenShift = 6, ChequeDueDate = 7 }

    public enum SalesRepApprovalStatus { Pending = 0, Approved = 1, Rejected = 2 }

    public enum SalesRepDiscountScope { Rep = 0, CustomerGroup = 1, Item = 2, ItemGroup = 3 }

    public enum SalesRepReturnCondition { Good = 0, Damaged = 1 }

    public enum SalesRepSyncStatus { Received = 0, Processed = 1, Error = 2, PendingReview = 3 }

    public enum SalesRepTargetScope { Total = 0, Item = 1, ItemGroup = 2, NewCustomers = 3, Collection = 4 }

    public enum SalesRepCommissionComponent { NetSales = 0, Collection = 1, TotalTarget = 2, ItemTarget = 3 }

    public enum SalesRepCommissionRunStatus { Calculated = 0, Approved = 1, Posted = 2 }

    public enum SalesRepAttachmentRefType { Visit = 0, Return = 1, NoSale = 2 }

    /// <summary>أعمدة التدقيق القياسية المشتركة (غير مربوطة بجدول بذاتها).</summary>
    public abstract class SalesRepAuditEntity
    {
        public long? User_Id { get; set; }
        public DateTime? OperationDate { get; set; }
        public DateTime? Insert_ServerDateTime { get; set; }
        public DateTime? Update_ServerDateTime { get; set; }
        public long? Edit_User_Id { get; set; }
        public DateTime? EditOperationDate { get; set; }
        public long? Branch_Id { get; set; }
        public long? ProcessNo { get; set; }

        /// <summary>يملأ أعمدة الإضافة من الجلسة الحالية.</summary>
        public void StampInsert(long branchId)
        {
            DateTime now = DateTime.Now;
            User_Id = Common.User_Id;
            OperationDate = now.Date;
            Insert_ServerDateTime = now;
            Branch_Id = branchId;
        }

        /// <summary>يملأ أعمدة التعديل من الجلسة الحالية.</summary>
        public void StampEdit()
        {
            DateTime now = DateTime.Now;
            Edit_User_Id = Common.User_Id;
            EditOperationDate = now;
            Update_ServerDateTime = now;
        }
    }

    /// <summary>ملف المندوب الميدانى — امتداد للموظف (Employee).</summary>
    [Table("SalesRep_Reps")]
    public class SalesRep_Rep : SalesRepAuditEntity
    {
        [Key] public long Id { get; set; }
        public long Employee_Id { get; set; }
        public long Van_Store_Id { get; set; }
        public long Damaged_Store_Id { get; set; }
        public long Cash_Box_Id { get; set; }
        public int Sales_Mode { get; set; }
        public long? Price_Type_List_ID { get; set; }
        public decimal? Max_Discount_Rate { get; set; }
        public decimal? Max_Cash_On_Hand { get; set; }
        public bool Allow_Credit_Sales { get; set; }
        public bool Allow_Blind_Return { get; set; }
        /// <summary>نافذة الإرجاع بالأيام من تاريخ الفاتورة (فارغ = 30)؛ خارجها يحتاج موافقة المشرف (§6.6-4).</summary>
        public int? Return_Window_Days { get; set; }
        /// <summary>يحصّل شيكات من العملاء على ورديته (مغلق افتراضياً).</summary>
        public bool Allow_Cheque_Collection { get; set; }
        /// <summary>أقصى أيام بين التحصيل واستحقاق الشيك؛ ما بعده يحتاج موافقة المشرف (فارغ = بلا حد).</summary>
        public int? Max_Cheque_Due_Days { get; set; }
        public int End_Of_Day_Stock_Policy { get; set; }
        public long? Cost_Center_Id { get; set; }
        public long? Commission_Plan_Id { get; set; }
        public long? Supervisor_Employee_Id { get; set; }
        public bool Active { get; set; } = true;
    }

    [Table("SalesRep_Devices")]
    public class SalesRep_Device : SalesRepAuditEntity
    {
        [Key] public long Id { get; set; }
        public long Rep_Id { get; set; }
        public string Device_Uid { get; set; } = "";
        public string Platform { get; set; }
        public DateTime? Last_Sync_At { get; set; }
        public bool Is_Blocked { get; set; }
        public long? Number_Range_From { get; set; }
        public long? Number_Range_To { get; set; }
        public long? Number_Range_Next { get; set; }
    }

    [Table("SalesRep_Routes")]
    public class SalesRep_Route : SalesRepAuditEntity
    {
        [Key] public long Id { get; set; }
        public string Name { get; set; } = "";
        public long Rep_Id { get; set; }
        public long? Area_Id { get; set; }
        public bool Active { get; set; } = true;
    }

    [Table("SalesRep_Route_Customers")]
    public class SalesRep_Route_Customer : SalesRepAuditEntity
    {
        [Key] public long Id { get; set; }
        public long Route_Id { get; set; }
        public long Customer_Id { get; set; }
        /// <summary>يوم الأسبوع بترقيم System.DayOfWeek (0 = الأحد).</summary>
        public int Visit_Day { get; set; }
        /// <summary>الوتيرة بالأسابيع: 1 أسبوعى، 2 كل أسبوعين، 4 شهرى.</summary>
        public int Frequency { get; set; } = 1;
        public int? Week_Parity { get; set; }
        public int Sequence { get; set; }
    }

    [Table("SalesRep_Customer_Geo")]
    public class SalesRep_Customer_Geo : SalesRepAuditEntity
    {
        [Key] public long Id { get; set; }
        public long Customer_Id { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public int Geofence_Radius_M { get; set; } = 100;
        public long? Verified_By { get; set; }
        public DateTime? Verified_At { get; set; }
    }

    [Table("SalesRep_Credit_Policy")]
    public class SalesRep_Credit_Policy : SalesRepAuditEntity
    {
        [Key] public long Id { get; set; }
        public long? Customer_Id { get; set; }
        public long? Group_Id { get; set; }
        public decimal Credit_Limit { get; set; }
        public int? Max_Overdue_Days { get; set; }
        public bool Block_On_Overdue { get; set; } = true;
    }

    [Table("SalesRep_Discount_Policy")]
    public class SalesRep_Discount_Policy : SalesRepAuditEntity
    {
        [Key] public long Id { get; set; }
        public int Scope { get; set; }
        public long Scope_Id { get; set; }
        public decimal Max_Discount_Rate { get; set; }
        public decimal? Requires_Approval_Above { get; set; }
    }

    [Table("SalesRep_Shifts")]
    public class SalesRep_Shift : SalesRepAuditEntity
    {
        [Key] public long Id { get; set; }
        public long Rep_Id { get; set; }
        public DateTime Shift_Date { get; set; }
        public int Status { get; set; }
        public long? Load_Order_Id { get; set; }
        public long? Unload_Order_Id { get; set; }
        public long? Damaged_Transfer_Order_Id { get; set; }
        public DateTime? Opened_At { get; set; }
        public DateTime? Closed_At { get; set; }
        public long? Closed_By { get; set; }
        public decimal? Cash_Expected { get; set; }
        public decimal? Cash_Counted { get; set; }
        public decimal? Cash_Diff { get; set; }
        public decimal? Stock_Diff_Value { get; set; }
        public string Notes { get; set; }
    }

    /// <summary>مطابقة الصنف فى الوردية (بالوحدة الصغرى)، محسوبة من دفتر المحرك لمخزن السيارة.</summary>
    [Table("SalesRep_Shift_Stock")]
    public class SalesRep_Shift_Stock : SalesRepAuditEntity
    {
        [Key] public long Id { get; set; }
        public long Shift_Id { get; set; }
        public long Unit_Item_Table_Id { get; set; }
        public decimal Opening { get; set; }
        public decimal Loaded { get; set; }
        public decimal Sold { get; set; }
        public decimal Delivered { get; set; }
        public decimal Returned_Good { get; set; }
        public decimal Returned_Damaged { get; set; }
        public decimal Unloaded { get; set; }
        public decimal Other_Movement { get; set; }
        public decimal Expected_Closing { get; set; }
        public decimal? Counted_Closing { get; set; }
        public decimal? Diff { get; set; }
        public decimal? Unit_Cost { get; set; }
        public long? Diff_Order_Id { get; set; }
    }

    [Table("SalesRep_Shift_Cash")]
    public class SalesRep_Shift_Cash : SalesRepAuditEntity
    {
        [Key] public long Id { get; set; }
        public long Shift_Id { get; set; }
        public long Payment_Method_Id { get; set; }
        public decimal Expected { get; set; }
        public decimal? Counted { get; set; }
        public decimal? Diff { get; set; }
        public long? Remittance_Voucher_Id { get; set; }
        public long? Diff_Voucher_Id { get; set; }
    }

    [Table("SalesRep_Document_Links")]
    public class SalesRep_Document_Link : SalesRepAuditEntity
    {
        [Key] public long Id { get; set; }
        public int Doc_Type { get; set; }
        public long Doc_Id { get; set; }
        public long Shift_Id { get; set; }
        public long? Visit_Id { get; set; }
        public Guid? Doc_Guid { get; set; }
        /// <summary>شيك المندوب: وقت تسليمه الفعلى للمشرف عند إقفال الوردية ومن سلّمه (فارغ = ما زال مع المندوب).</summary>
        public DateTime? Handed_Over_At { get; set; }
        public long? Handed_Over_User_Id { get; set; }
    }

    [Table("SalesRep_Visits")]
    public class SalesRep_Visit : SalesRepAuditEntity
    {
        [Key] public long Id { get; set; }
        public long Shift_Id { get; set; }
        public long Customer_Id { get; set; }
        public bool Planned { get; set; } = true;
        public DateTime? Check_In_At { get; set; }
        public decimal? Check_In_Lat { get; set; }
        public decimal? Check_In_Lng { get; set; }
        public DateTime? Check_Out_At { get; set; }
        public int? Distance_From_Customer_M { get; set; }
        public bool? Inside_Geofence { get; set; }
        public int? Result { get; set; }
        public long? No_Sale_Reason_Id { get; set; }
        public string Notes { get; set; }
        public long? Photo_Id { get; set; }
    }

    [Table("Lookup_SalesRep_NoSaleReasons")]
    public class Lookup_SalesRep_NoSaleReason : SalesRepAuditEntity
    {
        [Key] public long Id { get; set; }
        public string Name { get; set; } = "";
        public bool Requires_Photo { get; set; }
        public bool Active { get; set; } = true;
    }

    [Table("Lookup_SalesRep_ReturnReasons")]
    public class Lookup_SalesRep_ReturnReason : SalesRepAuditEntity
    {
        [Key] public long Id { get; set; }
        public string Name { get; set; } = "";
        public bool Is_Damaged { get; set; }
        public bool Requires_Photo { get; set; }
        public bool Requires_Batch { get; set; }
        public bool Active { get; set; } = true;
    }

    public enum SalesRepAssignmentStatus { Assigned = 0, Delivered = 1, Unassigned = 2, Closed = 3 }

    public enum SalesRepDeliveryResult { Full = 0, Partial = 1, Refused = 2 }

    public enum SalesRepRemainingAction { Retry = 0, Close = 1 }

    [Table("SalesRep_Order_Assignments")]
    public class SalesRep_Order_Assignment : SalesRepAuditEntity
    {
        [Key] public long Id { get; set; }
        public long Order_Id { get; set; }
        public long Rep_Id { get; set; }
        public DateTime? Planned_Date { get; set; }
        public int Status { get; set; }
        public long? Original_Store_Id { get; set; }
        public DateTime? Ended_At { get; set; }
        public string End_Reason { get; set; }
    }

    [Table("SalesRep_Order_Assignment_Lines")]
    public class SalesRep_Order_Assignment_Line : SalesRepAuditEntity
    {
        [Key] public long Id { get; set; }
        public long Assignment_Id { get; set; }
        public long Order_Line_Id { get; set; }
        public long? Original_Unit_Item_Id { get; set; }
        public long Van_Unit_Item_Id { get; set; }
    }

    [Table("SalesRep_Order_Deliveries")]
    public class SalesRep_Order_Delivery : SalesRepAuditEntity
    {
        [Key] public long Id { get; set; }
        public long Assignment_Id { get; set; }
        public long Order_Id { get; set; }
        public long? Shift_Id { get; set; }
        public long? Invoice_Id { get; set; }
        public int Result { get; set; }
        public int? Remaining_Action { get; set; }
        public string Reason { get; set; }
        public decimal Deposit_Applied { get; set; }
        public decimal Cash_Collected { get; set; }
        public string Notes { get; set; }
    }

    [Table("SalesRep_Invoice_Return_Links")]
    public class SalesRep_Invoice_Return_Link : SalesRepAuditEntity
    {
        [Key] public long Id { get; set; }
        public long Return_Detail_Id { get; set; }
        public long? Original_Detail_Id { get; set; }
        public long? Original_Invoice_Id { get; set; }
        public bool Is_Blind { get; set; }
        public decimal Qty_Large { get; set; }
        public decimal Qty_Small { get; set; }
        public decimal Qty_ItemNo { get; set; }
        public decimal Net_Unit_Price { get; set; }
        public decimal? Unit_Cost { get; set; }
        public long? Reason_Id { get; set; }
        public int Condition { get; set; }
        public long? Unit_Item_ExpireDate_Id { get; set; }
        public long? Photo_Id { get; set; }
    }

    [Table("SalesRep_Approvals")]
    public class SalesRep_Approval : SalesRepAuditEntity
    {
        [Key] public long Id { get; set; }
        public int Type { get; set; }
        public Guid? Ref_Document_Guid { get; set; }
        public int? Ref_Doc_Type { get; set; }
        public long? Ref_Doc_Id { get; set; }
        public long Requested_By { get; set; }
        public long? Approver_Id { get; set; }
        public int Status { get; set; }
        public string Otp_Hash { get; set; }
        public string Reason { get; set; }
        public string Decision_Notes { get; set; }
        public DateTime? Decided_At { get; set; }
        public long? Rep_Id { get; set; }
        public long? Shift_Id { get; set; }
        public long? Customer_Id { get; set; }
        /// <summary>القيمة التى طلبها المندوب (نسبة الخصم، أو رصيد العميل بعد الفاتورة، أو عمر الدين بالأيام).</summary>
        public decimal? Requested_Value { get; set; }
        /// <summary>الحد المسموح وقت الطلب.</summary>
        public decimal? Allowed_Value { get; set; }
        /// <summary>الحد الذى اعتمده المشرف (افتراضياً المطلوب)؛ الموافقة لا تغطى قيمة أعلى منه.</summary>
        public decimal? Approved_Value { get; set; }
    }

    [Table("SalesRep_GPS_Track")]
    public class SalesRep_GPS_Track : SalesRepAuditEntity
    {
        [Key] public long Id { get; set; }
        public long Rep_Id { get; set; }
        public long? Shift_Id { get; set; }
        public DateTime Tracked_At { get; set; }
        public decimal Lat { get; set; }
        public decimal Lng { get; set; }
        public decimal? Accuracy { get; set; }
        public int? Battery { get; set; }
    }

    [Table("SalesRep_Sync_Inbox")]
    public class SalesRep_Sync_Inbox : SalesRepAuditEntity
    {
        [Key] public long Id { get; set; }
        public long Device_Id { get; set; }
        public Guid Doc_Guid { get; set; }
        public int Doc_Type { get; set; }
        public string Payload { get; set; }
        public int Status { get; set; }
        public string Error { get; set; }
        public long? Result_Ref_Id { get; set; }
        public DateTime Received_At { get; set; }
        public DateTime? Processed_At { get; set; }
    }

    [Table("SalesRep_Attachments")]
    public class SalesRep_Attachment : SalesRepAuditEntity
    {
        [Key] public long Id { get; set; }
        public int Ref_Type { get; set; }
        public long Ref_Id { get; set; }
        public byte[] File_Data { get; set; }
        public string File_Path { get; set; }
        public string Content_Type { get; set; }
        public DateTime? Taken_At { get; set; }
        public decimal? Lat { get; set; }
        public decimal? Lng { get; set; }
    }

    [Table("SalesRep_Targets")]
    public class SalesRep_Target : SalesRepAuditEntity
    {
        [Key] public long Id { get; set; }
        public long Rep_Id { get; set; }
        public DateTime Period_From { get; set; }
        public DateTime Period_To { get; set; }
        public int Scope { get; set; }
        public long? Scope_Id { get; set; }
        public decimal? Target_Qty { get; set; }
        public decimal? Target_Value { get; set; }
    }

    [Table("SalesRep_Commission_Plans")]
    public class SalesRep_Commission_Plan : SalesRepAuditEntity
    {
        [Key] public long Id { get; set; }
        public string Name { get; set; } = "";
        public bool Deduct_Returns { get; set; } = true;
        public int? Collection_Speed_Days { get; set; }
        public decimal? Collection_Speed_Bonus_Rate { get; set; }
        public bool Active { get; set; } = true;
    }

    [Table("SalesRep_Commission_Plan_Tiers")]
    public class SalesRep_Commission_Plan_Tier : SalesRepAuditEntity
    {
        [Key] public long Id { get; set; }
        public long Plan_Id { get; set; }
        public int Component { get; set; }
        public decimal Achievement_From { get; set; }
        public decimal? Achievement_To { get; set; }
        public decimal? Rate { get; set; }
        public decimal? Fixed_Amount { get; set; }
    }

    [Table("SalesRep_Commission_Runs")]
    public class SalesRep_Commission_Run : SalesRepAuditEntity
    {
        [Key] public long Id { get; set; }
        public long Rep_Id { get; set; }
        public long Plan_Id { get; set; }
        public DateTime Period_From { get; set; }
        public DateTime Period_To { get; set; }
        public int Status { get; set; }
        public decimal Total_Amount { get; set; }
        public long? Approved_By { get; set; }
        public DateTime? Approved_At { get; set; }
        public long? Account_Entry_List_ID { get; set; }
    }

    [Table("SalesRep_Commission_Run_Lines")]
    public class SalesRep_Commission_Run_Line : SalesRepAuditEntity
    {
        [Key] public long Id { get; set; }
        public long Run_Id { get; set; }
        public int Component { get; set; }
        public long? Scope_Id { get; set; }
        public decimal Base_Amount { get; set; }
        public decimal? Target_Value { get; set; }
        public decimal? Achievement_Rate { get; set; }
        public decimal? Rate { get; set; }
        public decimal Amount { get; set; }
        public string Notes { get; set; }
    }
}
