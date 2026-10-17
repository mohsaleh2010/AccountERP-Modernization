namespace AccountERP.SalesRepApp.Core
{
    /// <summary>
    /// أرقام مستندات الجهاز (م9): بادئة ثابتة لكل جهاز يحجزها السيرفر عند التفعيل (M001) + رمز النوع + تسلسل لكل نوع على الجهاز:
    /// M001-S000012. تُطبع للعميل فوراً بلا اتصال، ولا تتداخل بين جهازين، ويبحث بها المكتب فى «مستندات الأجهزة».
    /// رقم الفاتورة الرسمى فى النظام يُعطى عند الحفظ على السيرفر (Result_Ref_Id). = SalesRepSyncService.DocCode/DocNo.
    /// </summary>
    public static class DocNumbers
    {
        public const string PrefixKey = "doc_prefix";

        public static string SeqKey(DocType type) => "doc_seq." + (int)type;

        public static string? Code(DocType type) => type switch
        {
            DocType.Sale => "S", DocType.Return => "R", DocType.Collection => "C", DocType.TakeOrder => "O", DocType.Delivery => "D", DocType.Cheque => "Q", _ => null,
        };

        public static string? Format(string? prefix, DocType type, long seq) =>
            string.IsNullOrEmpty(prefix) || Code(type) == null || seq <= 0 ? null : $"{prefix}-{Code(type)}{seq:000000}";
    }
}
