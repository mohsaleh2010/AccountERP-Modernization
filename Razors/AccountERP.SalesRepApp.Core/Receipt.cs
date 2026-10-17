namespace AccountERP.SalesRepApp.Core
{
    /// <summary>سطر فى المستند المطبوع: نص وحده، أو نص وقيمة (القيمة على الطرف الآخر)، أو فاصل.</summary>
    public class ReceiptRow
    {
        public string Text { get; set; } = "";
        public string? Value { get; set; }
        public bool Bold { get; set; }
        public bool Large { get; set; }
        public bool Center { get; set; }
        public bool Rule { get; set; }
    }

    /// <summary>
    /// المستند المطبوع للعميل كما كان لحظة الحفظ (لقطة ثابتة): إعادة الطباعة بعد المزامنة تطابق الأصل ولو اختفت الطلبية أو الفاتورة من بيانات الجهاز.
    /// يُرسم صورةً على الجهاز (فيصح العربى على أى طابعة حرارية) ويُعرض معاينة HTML.
    /// </summary>
    public class Receipt
    {
        public Guid Doc_Guid { get; set; }
        public string Title { get; set; } = "";
        public string? Doc_No { get; set; }
        public decimal Total { get; set; }
        public List<ReceiptRow> Rows { get; set; } = new();
        /// <summary>طُبع من قبل: يُطبع بعلامة «نسخة».</summary>
        public bool Copy { get; set; }

        internal Receipt Line(string text, bool bold = false, bool large = false, bool center = false)
        {
            if (!string.IsNullOrWhiteSpace(text)) Rows.Add(new ReceiptRow { Text = text.Trim(), Bold = bold, Large = large, Center = center });
            return this;
        }

        internal Receipt Pair(string text, string? value, bool bold = false, bool large = false)
        {
            if (!string.IsNullOrWhiteSpace(value)) Rows.Add(new ReceiptRow { Text = text, Value = value.Trim(), Bold = bold, Large = large });
            return this;
        }

        internal Receipt Rule() { Rows.Add(new ReceiptRow { Rule = true }); return this; }
    }

    /// <summary>
    /// أوامر ESC/POS للطابعات الحرارية: المستند صورة نقطية (GS v 0) على شرائح، ثم تغذية الورق.
    /// النقاط بالصفوف من أعلى، true = نقطة سوداء. العرض 384 نقطة لورق 58 مم و576 لورق 80 مم.
    /// </summary>
    public static class EscPos
    {
        public const int Width58 = 384, Width80 = 576;
        /// <summary>ارتفاع الشريحة: ذاكرة الطابعات الصغيرة لا تحتمل صورة طويلة فى أمر واحد.</summary>
        public const int Band = 128;

        public static byte[] Raster(bool[] dots, int width, int height, int feedLines = 4)
        {
            if (width <= 0 || height < 0 || dots.Length < width * height) throw new ArgumentException("أبعاد الصورة غير صحيحة.");
            int rowBytes = (width + 7) / 8;
            using var ms = new MemoryStream();
            ms.Write(new byte[] { 0x1B, 0x40 });                       // ESC @ : تهيئة
            for (int top = 0; top < height; top += Band)
            {
                int h = Math.Min(Band, height - top);
                ms.Write(new byte[] { 0x1D, 0x76, 0x30, 0x00, (byte)(rowBytes & 0xFF), (byte)(rowBytes >> 8), (byte)(h & 0xFF), (byte)(h >> 8) });
                var band = new byte[rowBytes * h];
                for (int y = 0; y < h; y++)
                    for (int x = 0; x < width; x++)
                        if (dots[(top + y) * width + x]) band[y * rowBytes + x / 8] |= (byte)(0x80 >> (x % 8));
                ms.Write(band);
            }
            ms.Write(new byte[] { 0x1B, 0x64, (byte)Math.Clamp(feedLines, 0, 255) });   // ESC d n : تغذية
            return ms.ToArray();
        }
    }
}
