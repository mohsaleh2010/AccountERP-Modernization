using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountModelsLib.SalesRep
{
    public enum SalesRepSalesMode
    {
        Van = 0,        // بيع فورى (سيارة)
        PreSales = 1,   // طلبيات مسبقة
        Mixed = 2       // نمط مختلط (فورى + طلبيات)
    }

    public enum SalesRepShiftStatus
    {
        Open = 0,       // مفتوحة
        Closing = 1,    // قيد الإقفال
        Closed = 2,     // مقفلة
        Reopened = 3    // معاد فتحها
    }

    public enum SalesRepVisitResult
    {
        Sale = 0,       // بيع فورى
        Order = 1,      // طلبية
        Collection = 2, // تحصيل
        NoSale = 3      // زيارة بلا بيع
    }

    public enum SalesRepApprovalType
    {
        Credit = 0,         // تجاوز حد الائتمان
        Discount = 1,       // تجاوز سقف الخصم
        BlindReturn = 2,    // مرتجع بلا فاتورة
        Geo = 3,            // خارج النطاق الجغرافى
        ReturnWindow = 4,   // خارج نافذة الإرجاع
        ReopenShift = 5     // إعادة فتح وردية مقفلة
    }

    public enum SalesRepApprovalStatus
    {
        Pending = 0,    // معلق
        Approved = 1,   // معتمد
        Rejected = 2    // مرفوض
    }

    public enum SalesRepDocType
    {
        Invoice = 1,        // فاتورة مبيعات
        Return = 2,         // مرتجع مبيعات
        CashVoucher = 3,    // سند قبض نقدية
        StoreOrder = 4,     // إذن مخزن (تحميل / تفريغ / تسوية)
        CustomerOrder = 5   // طلبية عميل
    }

    /// <summary>ملف المندوب الميدانى (امتداد لسجل الموظف Employee).</summary>
    [Table("SalesRep_Reps")]
    public class SalesRep_Rep
    {
        [Key]
        public long Id { get; set; }
        public long Employee_Id { get; set; }
        public long Van_Store_Id { get; set; }
        public long Damaged_Store_Id { get; set; }
        public long Cash_Box_Id { get; set; }
        public int Sales_Mode { get; set; } = (int)SalesRepSalesMode.Van;
        public long? Price_Type_List_ID { get; set; }
        public decimal? Max_Discount_Rate { get; set; }
        public decimal? Max_Cash_On_Hand { get; set; }
        public bool Allow_Credit_Sales { get; set; } = false;
        public bool Allow_Blind_Return { get; set; } = false;
        public long? Supervisor_Employee_Id { get; set; }
        public bool Active { get; set; } = true;
        public int? User_Id { get; set; }
        public DateTime? OperationDate { get; set; }
        public DateTime? Insert_ServerDateTime { get; set; }
        public int? Edit_User_Id { get; set; }
        public DateTime? EditOperationDate { get; set; }
        public int? Branch_Id { get; set; }
        public long? ProcessNo { get; set; }
    }

    /// <summary>أجهزة المندوبين وسلاسل الترقيم للأوفلاين.</summary>
    [Table("SalesRep_Devices")]
    public class SalesRep_Device
    {
        [Key]
        public long Id { get; set; }
        public long Rep_Id { get; set; }
        public string Device_Uid { get; set; } = "";
        public string Platform { get; set; }
        public DateTime? Last_Sync_At { get; set; }
        public bool Is_Blocked { get; set; } = false;
        public long? Number_Range_From { get; set; }
        public long? Number_Range_To { get; set; }
        public long? Number_Range_Next { get; set; }
        public int? User_Id { get; set; }
        public DateTime? OperationDate { get; set; }
        public DateTime? Insert_ServerDateTime { get; set; }
        public int? Edit_User_Id { get; set; }
        public DateTime? EditOperationDate { get; set; }
        public int? Branch_Id { get; set; }
        public long? ProcessNo { get; set; }
    }

    /// <summary>وردية المندوب اليومية.</summary>
    [Table("SalesRep_Shifts")]
    public class SalesRep_Shift
    {
        [Key]
        public long Id { get; set; }
        public long Rep_Id { get; set; }
        public DateTime Shift_Date { get; set; }
        public int Status { get; set; } = (int)SalesRepShiftStatus.Open;
        public long? Load_Order_Id { get; set; }
        public long? Unload_Order_Id { get; set; }
        public DateTime? Opened_At { get; set; }
        public DateTime? Closed_At { get; set; }
        public long? Closed_By { get; set; }
        public decimal? Cash_Expected { get; set; }
        public decimal? Cash_Counted { get; set; }
        public decimal? Cash_Diff { get; set; }
        public decimal? Stock_Diff_Value { get; set; }
        public int? User_Id { get; set; }
        public DateTime? OperationDate { get; set; }
        public DateTime? Insert_ServerDateTime { get; set; }
        public int? Edit_User_Id { get; set; }
        public DateTime? EditOperationDate { get; set; }
        public int? Branch_Id { get; set; }
        public long? ProcessNo { get; set; }
    }

    /// <summary>لقطة مطابقة مخزون السيارة لكل صنف فى الوردية.</summary>
    [Table("SalesRep_Shift_Stock")]
    public class SalesRep_Shift_Stock
    {
        [Key]
        public long Id { get; set; }
        public long Shift_Id { get; set; }
        public long Unit_Item_Table_Id { get; set; }
        public decimal Opening { get; set; } = 0;
        public decimal Loaded { get; set; } = 0;
        public decimal Sold { get; set; } = 0;
        public decimal Delivered { get; set; } = 0;
        public decimal Returned_Good { get; set; } = 0;
        public decimal Returned_Damaged { get; set; } = 0;
        public decimal Unloaded { get; set; } = 0;
        public decimal Expected_Closing { get; set; } = 0;
        public decimal Counted_Closing { get; set; } = 0;
        public decimal Diff { get; set; } = 0;
        public long? Diff_Order_Id { get; set; }
        public int? User_Id { get; set; }
        public DateTime? OperationDate { get; set; }
        public DateTime? Insert_ServerDateTime { get; set; }
        public int? Edit_User_Id { get; set; }
        public DateTime? EditOperationDate { get; set; }
        public int? Branch_Id { get; set; }
        public long? ProcessNo { get; set; }
    }

    /// <summary>مطابقة نقدية الوردية حسب طريقة الدفع.</summary>
    [Table("SalesRep_Shift_Cash")]
    public class SalesRep_Shift_Cash
    {
        [Key]
        public long Id { get; set; }
        public long Shift_Id { get; set; }
        public int Payment_Method_Id { get; set; }
        public decimal Expected { get; set; } = 0;
        public decimal Counted { get; set; } = 0;
        public decimal Diff { get; set; } = 0;
        public long? Remittance_Voucher_Id { get; set; }
        public int? User_Id { get; set; }
        public DateTime? OperationDate { get; set; }
        public DateTime? Insert_ServerDateTime { get; set; }
        public int? Edit_User_Id { get; set; }
        public DateTime? EditOperationDate { get; set; }
        public int? Branch_Id { get; set; }
        public long? ProcessNo { get; set; }
    }

    /// <summary>ربط مستندات النظام بالوردية والزيارة ومعرف التزامن.</summary>
    [Table("SalesRep_Document_Links")]
    public class SalesRep_Document_Link
    {
        [Key]
        public long Id { get; set; }
        public int Doc_Type { get; set; }
        public long Doc_Id { get; set; }
        public long Shift_Id { get; set; }
        public long? Visit_Id { get; set; }
        public Guid? Doc_Guid { get; set; }
        public int? User_Id { get; set; }
        public DateTime? OperationDate { get; set; }
        public DateTime? Insert_ServerDateTime { get; set; }
        public int? Edit_User_Id { get; set; }
        public DateTime? EditOperationDate { get; set; }
        public int? Branch_Id { get; set; }
        public long? ProcessNo { get; set; }
    }

    /// <summary>زيارات العملاء (المخططة والفعلية).</summary>
    [Table("SalesRep_Visits")]
    public class SalesRep_Visit
    {
        [Key]
        public long Id { get; set; }
        public long Shift_Id { get; set; }
        public long Customer_Id { get; set; }
        public bool Planned { get; set; } = true;
        public DateTime? Check_In_At { get; set; }
        public decimal? Check_In_Lat { get; set; }
        public decimal? Check_In_Lng { get; set; }
        public DateTime? Check_Out_At { get; set; }
        public int? Distance_From_Customer_M { get; set; }
        public bool? Inside_Geofence { get; set; }
        public int Result { get; set; } = (int)SalesRepVisitResult.Sale;
        public long? No_Sale_Reason_Id { get; set; }
        public string Notes { get; set; }
        public long? Photo_Id { get; set; }
        public int? User_Id { get; set; }
        public DateTime? OperationDate { get; set; }
        public DateTime? Insert_ServerDateTime { get; set; }
        public int? Edit_User_Id { get; set; }
        public DateTime? EditOperationDate { get; set; }
        public int? Branch_Id { get; set; }
        public long? ProcessNo { get; set; }
    }

    /// <summary>أسباب عدم البيع.</summary>
    [Table("Lookup_SalesRep_NoSaleReasons")]
    public class Lookup_SalesRep_NoSaleReason
    {
        [Key]
        public long Id { get; set; }
        public string Name { get; set; } = "";
        public bool Requires_Photo { get; set; } = false;
        public int? User_Id { get; set; }
        public DateTime? OperationDate { get; set; }
        public DateTime? Insert_ServerDateTime { get; set; }
        public int? Edit_User_Id { get; set; }
        public DateTime? EditOperationDate { get; set; }
        public int? Branch_Id { get; set; }
        public long? ProcessNo { get; set; }
    }

    /// <summary>أسباب الإرجاع للمبيعات.</summary>
    [Table("Lookup_SalesRep_ReturnReasons")]
    public class Lookup_SalesRep_ReturnReason
    {
        [Key]
        public long Id { get; set; }
        public string Name { get; set; } = "";
        public bool Is_Damaged { get; set; } = false;
        public bool Requires_Photo { get; set; } = false;
        public bool Requires_Batch { get; set; } = false;
        public int? User_Id { get; set; }
        public DateTime? OperationDate { get; set; }
        public DateTime? Insert_ServerDateTime { get; set; }
        public int? Edit_User_Id { get; set; }
        public DateTime? EditOperationDate { get; set; }
        public int? Branch_Id { get; set; }
        public long? ProcessNo { get; set; }
    }

    /// <summary>طلبات الموافقة على التجاوزات.</summary>
    [Table("SalesRep_Approvals")]
    public class SalesRep_Approval
    {
        [Key]
        public long Id { get; set; }
        public int Type { get; set; }
        public Guid Ref_Document_Guid { get; set; }
        public long? Requested_By { get; set; }
        public long? Approver_Id { get; set; }
        public int Status { get; set; } = (int)SalesRepApprovalStatus.Pending;
        public string Otp_Hash { get; set; }
        public string Reason { get; set; }
        public DateTime? Decided_At { get; set; }
        public int? User_Id { get; set; }
        public DateTime? OperationDate { get; set; }
        public DateTime? Insert_ServerDateTime { get; set; }
        public int? Edit_User_Id { get; set; }
        public DateTime? EditOperationDate { get; set; }
        public int? Branch_Id { get; set; }
        public long? ProcessNo { get; set; }
    }

    /// <summary>مرفقات وصور الزيارات والمرتجعات على السيرفر.</summary>
    [Table("SalesRep_Attachments")]
    public class SalesRep_Attachment
    {
        [Key]
        public long Id { get; set; }
        public int Ref_Type { get; set; } // 0: Visit, 1: Return, 2: NoSale
        public long Ref_Id { get; set; }
        public string Blob_Path { get; set; } = "";
        public DateTime? Taken_At { get; set; }
        public decimal? Lat { get; set; }
        public decimal? Lng { get; set; }
        public int? User_Id { get; set; }
        public DateTime? OperationDate { get; set; }
        public DateTime? Insert_ServerDateTime { get; set; }
        public int? Edit_User_Id { get; set; }
        public DateTime? EditOperationDate { get; set; }
        public int? Branch_Id { get; set; }
        public long? ProcessNo { get; set; }
    }

    /// <summary>سياسة وسقف الائتمان للعملاء.</summary>
    [Table("SalesRep_Credit_Policy")]
    public class SalesRep_Credit_Policy
    {
        [Key]
        public long Id { get; set; }
        public long? Customer_Id { get; set; }
        public long? Group_Id { get; set; }
        public decimal Credit_Limit { get; set; } = 0;
        public int Max_Overdue_Days { get; set; } = 0;
        public bool Block_On_Overdue { get; set; } = true;
        public int? User_Id { get; set; }
        public DateTime? OperationDate { get; set; }
        public DateTime? Insert_ServerDateTime { get; set; }
        public int? Edit_User_Id { get; set; }
        public DateTime? EditOperationDate { get; set; }
        public int? Branch_Id { get; set; }
        public long? ProcessNo { get; set; }
    }

    /// <summary>سياسة وسقوف الخصم المسموح به.</summary>
    [Table("SalesRep_Discount_Policy")]
    public class SalesRep_Discount_Policy
    {
        [Key]
        public long Id { get; set; }
        public int Scope { get; set; } // 0: Rep, 1: Segment, 2: Item, 3: Group
        public long Scope_Id { get; set; }
        public decimal Max_Discount_Rate { get; set; } = 0;
        public decimal? Requires_Approval_Above { get; set; }
        public int? User_Id { get; set; }
        public DateTime? OperationDate { get; set; }
        public DateTime? Insert_ServerDateTime { get; set; }
        public int? Edit_User_Id { get; set; }
        public DateTime? EditOperationDate { get; set; }
        public int? Branch_Id { get; set; }
        public long? ProcessNo { get; set; }
    }
}
