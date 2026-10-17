namespace AccountERP.SalesRepApp.Core
{
    // بقية مستندات المندوب على الجهاز ومجموعاتها (= مدخلات SalesRepReturnService/OrderService/ChequeService/VisitService على السيرفر).

    public class ReasonRow
    {
        public long Id { get; set; }
        public string Name { get; set; } = "";
        public bool Is_Damaged { get; set; }
        public bool Requires_Photo { get; set; }
        public bool Requires_Batch { get; set; }
    }

    public class BankRow
    {
        public long Id { get; set; }
        public string Name { get; set; } = "";
    }

    /// <summary>فاتورة للمندوب قابلة للإرجاع (مجموعة invoices).</summary>
    public class InvoiceView
    {
        public long Id { get; set; }
        public long Invoice_No { get; set; }
        public DateTime? Invoice_Date { get; set; }
        public long Customer_Id { get; set; }
        public string Customer_Name { get; set; } = "";
        public decimal Net_Total { get; set; }
        public bool Is_Credit { get; set; }
        public int Age_Days { get; set; }
        public int Window_Days { get; set; }
        public List<ReturnableLine> Lines { get; set; } = new();
    }

    public class ReturnableLine
    {
        public long Original_Detail_Id { get; set; }
        public long Unit_Item_Id { get; set; }
        public long Item_Id { get; set; }
        public string Item_Name { get; set; } = "";
        public string Code { get; set; } = "";
        public string B_Unit { get; set; } = "";
        public string S_Unit { get; set; } = "";
        public decimal Factor { get; set; } = 1;
        public decimal Sold_Small { get; set; }
        public decimal Returned_Small { get; set; }
        public decimal Net_Unit_Price { get; set; }
        public bool Has_Batch { get; set; }
        public decimal Returnable_Small => Math.Max(0, Sold_Small - Returned_Small);
    }

    /// <summary>طلبية مسندة للتسليم (مجموعة orders).</summary>
    public class OrderSync
    {
        public OrderInfo Order { get; set; } = new();
        public List<DeliveryLine> Lines { get; set; } = new();
    }

    public class OrderInfo
    {
        public long Id { get; set; }
        public long Order_No { get; set; }
        public DateTime? Order_Date { get; set; }
        public DateTime? Expected_Delivery_Date { get; set; }
        public long Customer_Id { get; set; }
        public string Customer_Name { get; set; } = "";
        public decimal Net_Total { get; set; }
        public decimal Deposit_Remaining { get; set; }
        public decimal Remaining_Value { get; set; }
    }

    public class DeliveryLine
    {
        public long Order_Line_Id { get; set; }
        public long Unit_Item_Id { get; set; }
        public string Item_Name { get; set; } = "";
        public string B_Unit { get; set; } = "";
        public string S_Unit { get; set; } = "";
        public decimal Factor { get; set; } = 1;
        public decimal Remaining_Large { get; set; }
        public decimal Remaining_Small { get; set; }
        public decimal Net_Unit_Price { get; set; }
        public decimal Remaining_Qty_Small => Remaining_Large * (Factor <= 0 ? 1 : Factor) + Remaining_Small;
    }

    /// <summary>صنف من المخزن المصدر لأخذ طلبية (مجموعة order_items).</summary>
    public class OrderItemRow
    {
        public long Id { get; set; }
        public long Store_Id { get; set; }
        public string Store_Name { get; set; } = "";
        public long Item_Id { get; set; }
        public string Item_Name { get; set; } = "";
        public string Code { get; set; } = "";
        public string B_Unit { get; set; } = "";
        public string S_Unit { get; set; } = "";
        public decimal Factor { get; set; } = 1;
        public decimal Price_Large { get; set; }
        public decimal Tax_Rate { get; set; }
        public decimal Available_Small { get; set; }
    }

    public class ReturnLineInput
    {
        public long? Original_Detail_Id { get; set; }
        public long? Unit_Item_Id { get; set; }
        public decimal Qty_Large { get; set; }
        public decimal Qty_Small { get; set; }
        public long Reason_Id { get; set; }
        /// <summary>0 سليم (لمخزن السيارة)، 1 تالف (لمخزن التالف).</summary>
        public int Condition { get; set; }
        public long? Batch_Id { get; set; }
        public byte[]? Photo { get; set; }
        public string? Photo_Content_Type { get; set; }
    }

    public class ReturnInput
    {
        public long Customer_Id { get; set; }
        public long? Original_Invoice_Id { get; set; }
        public bool Refund_Cash { get; set; }
        public string? Notes { get; set; }
        public List<ReturnLineInput> Lines { get; set; } = new();
    }

    public class TakeOrderLine
    {
        public long Unit_Item_Id { get; set; }
        public decimal Qty_Large { get; set; }
        public decimal Qty_Small { get; set; }
        public decimal Unit_Price { get; set; }
        public decimal Gross { get; set; }
        public decimal Discount_Value { get; set; }
        public decimal Tax_Value { get; set; }
        public decimal Line_Total { get; set; }
        public string? Notes { get; set; }
    }

    public class TakeOrderInput
    {
        public long Customer_Id { get; set; }
        public long Source_Store_Id { get; set; }
        public long? Currency_Id { get; set; }
        public DateTime? Expected_Delivery_Date { get; set; }
        public string? Notes { get; set; }
        public decimal Deposit { get; set; }
        public long Deposit_Payment_Method_Id { get; set; }
        public List<TakeOrderLine> Lines { get; set; } = new();
    }

    public class DeliveryLineInput
    {
        public long Order_Line_Id { get; set; }
        public decimal Qty_Large { get; set; }
        public decimal Qty_Small { get; set; }
    }

    public class DeliveryInput
    {
        public long Order_Id { get; set; }
        public List<DeliveryLineInput> Lines { get; set; } = new();
        public bool Collect_Cash { get; set; }
        public long Payment_Method_Id { get; set; }
        /// <summary>للمتبقى غير المسلَّم: 0 إعادة المحاولة، 1 إغلاق المتبقى.</summary>
        public int? Remaining_Action { get; set; }
        public string? Reason { get; set; }
        public string? Notes { get; set; }
    }

    public class ChequeInput
    {
        public long Customer_Id { get; set; }
        public string Check_No { get; set; } = "";
        public long Bank_Id { get; set; }
        public DateTime Due_Date { get; set; }
        public decimal Amount { get; set; }
        public string? Drawer_Name { get; set; }
        public string? Notes { get; set; }
    }

    public class NoSaleInput
    {
        public long Customer_Id { get; set; }
        public long Reason_Id { get; set; }
        public string? Notes { get; set; }
        public byte[]? Photo { get; set; }
        public string? Photo_Content_Type { get; set; }
        public decimal? Lat { get; set; }
        public decimal? Lng { get; set; }
    }
}
