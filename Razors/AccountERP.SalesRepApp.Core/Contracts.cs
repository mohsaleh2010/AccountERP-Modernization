using System.Text.Json;
using System.Text.Json.Serialization;

namespace AccountERP.SalesRepApp.Core
{
    // عقود /api/v1 كما يرسلها السيرفر (أسماء الخصائص كما فى C# على السيرفر: Doc_Guid، Rep_Name…) — راجع
    // AccountERP.Web/Api/SalesRepMobileApi.cs وAccountModelsLib/SalesRep/SalesRepSyncService.cs.

    public static class Json
    {
        public static readonly JsonSerializerOptions Options = new()
        {
            PropertyNamingPolicy = null,
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
        };

        public static string Serialize<T>(T value) => JsonSerializer.Serialize(value, Options);
        public static T? Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, Options);
    }

    public class SessionGrant
    {
        public long Device_Id { get; set; }
        public long Rep_Id { get; set; }
        public string Rep_Name { get; set; } = "";
        public string Refresh_Token { get; set; } = "";
        public string Access_Token { get; set; } = "";
        public int Expires_In { get; set; }
        /// <summary>بادئة أرقام مستندات الجهاز (M001) وآخر تسلسل استلمه السيرفر لكل نوع.</summary>
        public string Doc_Prefix { get; set; } = "";
        public Dictionary<int, long> Doc_Seqs { get; set; } = new();
    }

    public class ApiError
    {
        public string Error_Code { get; set; } = "";
        public string Message { get; set; } = "";
    }

    public class PullResult
    {
        public string Dataset { get; set; } = "";
        public long Version { get; set; }
        public bool Full { get; set; }
        public List<JsonElement> Rows { get; set; } = new();
        public List<long> Deleted { get; set; } = new();
        public int Total { get; set; }
    }

    public class PushDoc
    {
        public Guid Doc_Guid { get; set; }
        public int Doc_Type { get; set; }
        public JsonElement Payload { get; set; }
        public DateTime? Created_At { get; set; }
        public long? Doc_Seq { get; set; }
    }

    public class PushResult
    {
        public Guid Doc_Guid { get; set; }
        public int Status { get; set; }
        public string? Error { get; set; }
        public long? Result_Ref_Id { get; set; }
        public bool Duplicate { get; set; }
    }

    /// <summary>أنواع المستندات المرفوعة (= SalesRepSyncDocType على السيرفر).</summary>
    public enum DocType { Sale = 1, Return = 2, Collection = 3, TakeOrder = 4, Delivery = 5, Cheque = 6, NoSale = 7, CheckIn = 8, CheckOut = 9 }

    /// <summary>حالة مستند الصادر: Queued محلى (لم يُقبل بعد)، والباقى = SalesRepSyncStatus على السيرفر.</summary>
    public enum OutboxStatus { Queued = -1, Received = 0, Processed = 1, Error = 2, PendingReview = 3, Rejected = 4 }

    // ===================== صفوف مجموعات البيانات =====================

    public class RepProfile
    {
        public long Id { get; set; }
        /// <summary>رأس المستند المطبوع (بيانات الفرع الضريبية وإلا اسم الفرع).</summary>
        public string Company_Name { get; set; } = "";
        public string Company_Address { get; set; } = "";
        public string Company_Phone { get; set; } = "";
        public string Tax_Number { get; set; } = "";
        /// <summary>منازل التقريب (SysConfig.DecimalPlaces) ليطابق حساب الجهاز حساب السيرفر.</summary>
        public int Decimal_Places { get; set; } = 2;
        public string Rep_Name { get; set; } = "";
        public long Branch_Id { get; set; }
        public int Sales_Mode { get; set; }
        public long Van_Store_Id { get; set; }
        public string Van_Store_Name { get; set; } = "";
        public long Cash_Box_Id { get; set; }
        public string Cash_Box_Name { get; set; } = "";
        public decimal Cash_On_Hand { get; set; }
        public long? Price_Type_List_ID { get; set; }
        public decimal? Max_Discount_Rate { get; set; }
        public decimal? Max_Cash_On_Hand { get; set; }
        public bool Allow_Credit_Sales { get; set; }
        public bool Allow_Blind_Return { get; set; }
        public bool Allow_Cheque_Collection { get; set; }
        public long? Shift_Id { get; set; }
        public int? Shift_Status { get; set; }
        /// <summary>تتبع المسار مفعّل لهذا المندوب (من المكتب) وفاصل التسجيل بالثوانى.</summary>
        public bool Track_Location { get; set; }
        public int Track_Interval_Sec { get; set; } = 120;
        /// <summary>وردية مفتوحة أو أُعيد فتحها (SalesRepShiftStatus 1/4).</summary>
        public bool ShiftOpen => Shift_Status is 1 or 4;
    }

    /// <summary>نقطة من مسار المندوب (= SalesRepTrackService.PointInput).</summary>
    public class TrackPoint
    {
        public DateTime Recorded_At { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public int? Accuracy_M { get; set; }
        public decimal? Speed_Kmh { get; set; }
    }

    public class TrackUploadResult
    {
        public int Accepted { get; set; }
        public int Skipped { get; set; }
    }

    public class PaymentMethod
    {
        public long Id { get; set; }
        public string Name { get; set; } = "";
        public bool Is_Cash { get; set; }
    }

    public class CustomerRow
    {
        public long Id { get; set; }
        public string Name { get; set; } = "";
        public string Code { get; set; } = "";
        public string Address { get; set; } = "";
        public string Mobile { get; set; } = "";
        public string Tel { get; set; } = "";
        public long Price_Type_List_ID { get; set; }
        public decimal Balance { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int? Geofence_Radius_M { get; set; }
    }

    public class ItemRow
    {
        public long Id { get; set; }
        public long Item_Id { get; set; }
        public string Item_Name { get; set; } = "";
        public string Code { get; set; } = "";
        public string BarCode2 { get; set; } = "";
        public string B_Unit { get; set; } = "";
        public string S_Unit { get; set; } = "";
        public decimal Factor { get; set; } = 1;
        public decimal Price_Large { get; set; }
        public decimal Price_Small { get; set; }
        public decimal Tax_Rate { get; set; }
    }

    public class PriceRow
    {
        public long Id { get; set; }
        public long Item_Id { get; set; }
        public long Price_Type_List_ID { get; set; }
        public decimal Price { get; set; }
    }

    public class StockRow
    {
        public long Id { get; set; }
        public decimal Full_Qua { get; set; }
        public decimal D_Qua { get; set; }
    }

    public class PlanRow
    {
        public long Customer_Id { get; set; }
        public string Customer_Name { get; set; } = "";
        public string Route_Name { get; set; } = "";
        public int Sequence { get; set; }
    }

    // ===================== مدخلات المستندات (= مدخلات SalesRepSyncService) =====================

    public class SaleLine
    {
        public long Unit_Item_Id { get; set; }
        public decimal Qty_Large { get; set; }
        public decimal Qty_Small { get; set; }
        /// <summary>سعر الوحدة الكبرى.</summary>
        public decimal Unit_Price { get; set; }
        public decimal Discount_Value { get; set; }
        public decimal Tax_Rate { get; set; }
        public string? Notes { get; set; }
    }

    public class SaleInput
    {
        public long Customer_Id { get; set; }
        public bool Is_Credit { get; set; }
        public long Payment_Method_Id { get; set; }
        public string? Notes { get; set; }
        public List<SaleLine> Lines { get; set; } = new();
    }

    public class CollectionInput
    {
        public long Customer_Id { get; set; }
        public decimal Amount { get; set; }
        public long Payment_Method_Id { get; set; }
        public string? Notes { get; set; }
    }
}
