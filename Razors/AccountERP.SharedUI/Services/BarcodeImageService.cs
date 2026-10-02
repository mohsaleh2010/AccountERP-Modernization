using System;
using ZXing;
using ZXing.Common;
using ZXing.SkiaSharp;

namespace AccountERP.SharedUI.Services
{
    // 🟢 توليد صورة باركود من الخادم عبر ZXing.Net بدل إعادة كتابة خوارزمية Code 39 يدوياً (كما فى
    // Store_LIB.BarCodeCtrl الأصلية) - بموافقة المستخدم الصريحة (راجع barcode-print-settings-razor-plan.md).
    // يُنتج صورة PNG جاهزة كـ data URI مباشرة لعرضها فى <img> وطباعتها عبر window.print().
    public static class BarcodeImageService
    {
        public static string GeneratePngDataUri(string code, BarcodeFormat format, int widthPx, int heightPx)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                code = " ";
            }

            var writer = new BarcodeWriter
            {
                Format = format,
                Options = new EncodingOptions
                {
                    Width = Math.Max(widthPx, 10),
                    Height = Math.Max(heightPx, 10),
                    Margin = 0,
                    PureBarcode = true,
                }
            };

            using var bitmap = writer.Write(code);
            using var data = bitmap.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100);
            var base64 = Convert.ToBase64String(data.ToArray());
            return "data:image/png;base64," + base64;
        }
    }
}
