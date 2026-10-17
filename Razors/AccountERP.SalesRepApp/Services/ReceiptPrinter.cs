using AccountERP.SalesRepApp.Core;

namespace AccountERP.SalesRepApp.Services
{
    public record PrinterDevice(string Name, string Address);

    /// <summary>
    /// طباعة المستند على طابعة حرارية بلوتوث (ESC/POS عبر SPP): يُرسم المستند صورةً بمحرك نصوص أندرويد (تشكيل العربى واتجاهه سليمان
    /// على أى طابعة، بلا صفحات ترميز)، ثم يُرسل نقطياً (GS v 0). الإعداد على الجهاز: الطابعة المقترنة، عرض الورق، الطباعة التلقائية بعد الحفظ.
    /// ويندوز: معاينة فقط.
    /// </summary>
    public static class ReceiptPrinter
    {
        private const string AddressKey = "printer.address", NameKey = "printer.name", WidthKey = "printer.width", AutoKey = "printer.auto";

        public static string? Address { get => Preferences.Default.Get<string?>(AddressKey, null); set => Preferences.Default.Set(AddressKey, value); }
        public static string? Name { get => Preferences.Default.Get<string?>(NameKey, null); set => Preferences.Default.Set(NameKey, value); }
        /// <summary>عرض الورق بالنقاط: 384 (58 مم) أو 576 (80 مم).</summary>
        public static int Width { get => Preferences.Default.Get(WidthKey, EscPos.Width58); set => Preferences.Default.Set(WidthKey, value); }
        public static bool Auto { get => Preferences.Default.Get(AutoKey, true); set => Preferences.Default.Set(AutoKey, value); }

        public static bool Supported => DeviceInfo.Platform == DevicePlatform.Android;
        public static bool Ready => Supported && !string.IsNullOrEmpty(Address);

        /// <summary>الطابعات المقترنة بالجهاز (تُقرن مرة من إعدادات بلوتوث أندرويد).</summary>
        public static async Task<List<PrinterDevice>> PairedAsync()
        {
#if ANDROID
            await EnsurePermissionAsync();
            var adapter = Adapter();
            if (adapter == null) throw new InvalidOperationException("الجهاز بلا بلوتوث.");
            if (!adapter.IsEnabled) throw new InvalidOperationException("شغّل البلوتوث أولاً.");
            return (adapter.BondedDevices ?? new List<Android.Bluetooth.BluetoothDevice>())
                .Select(d => new PrinterDevice(d.Name ?? d.Address ?? "", d.Address ?? "")).Where(d => d.Address != "").OrderBy(d => d.Name).ToList();
#else
            await Task.CompletedTask;
            return new List<PrinterDevice>();
#endif
        }

        public static async Task PrintAsync(Receipt receipt)
        {
            if (!Supported) throw new InvalidOperationException("الطباعة بالبلوتوث من تطبيق أندرويد فقط.");
            if (string.IsNullOrEmpty(Address)) throw new InvalidOperationException("اختر الطابعة أولاً من «الطابعة».");
#if ANDROID
            await EnsurePermissionAsync();
            int width = Width;
            byte[] data = await Task.Run(() =>
            {
                var (dots, height) = Render(receipt, width);
                return EscPos.Raster(dots, width, height);
            });
            await SendAsync(Address!, data);
#else
            await Task.CompletedTask;
#endif
        }

#if ANDROID
        private static readonly Java.Util.UUID Spp = Java.Util.UUID.FromString("00001101-0000-1000-8000-00805F9B34FB")!;

        private static Android.Bluetooth.BluetoothAdapter? Adapter() =>
            (Android.App.Application.Context.GetSystemService(Android.Content.Context.BluetoothService) as Android.Bluetooth.BluetoothManager)?.Adapter;

        private static async Task EnsurePermissionAsync()
        {
            var status = await Permissions.RequestAsync<Permissions.Bluetooth>();
            if (status != PermissionStatus.Granted) throw new InvalidOperationException("اسمح للتطبيق باستخدام البلوتوث للطباعة.");
        }

        private static async Task SendAsync(string address, byte[] data)
        {
            var adapter = Adapter() ?? throw new InvalidOperationException("الجهاز بلا بلوتوث.");
            if (!adapter.IsEnabled) throw new InvalidOperationException("شغّل البلوتوث أولاً.");
            await Task.Run(async () =>
            {
                var device = adapter.GetRemoteDevice(address) ?? throw new InvalidOperationException("الطابعة غير موجودة.");
                using var socket = device.CreateRfcommSocketToServiceRecord(Spp) ?? throw new InvalidOperationException("تعذر الاتصال بالطابعة.");
                try { socket.Connect(); }
                catch (Java.IO.IOException) { throw new InvalidOperationException("تعذر الاتصال بالطابعة؛ تأكد أنها مشغلة وقريبة."); }
                var os = socket.OutputStream ?? throw new InvalidOperationException("تعذر الاتصال بالطابعة.");
                for (int i = 0; i < data.Length; i += 2048) os.Write(data, i, Math.Min(2048, data.Length - i));
                os.Flush();
                // الطابعة تستقبل أسرع مما تطبع؛ غلق الاتصال مبكراً يقطع آخر المستند.
                await Task.Delay(Math.Min(6000, 800 + data.Length / 25));
                socket.Close();
            });
        }

        /// <summary>يرسم المستند بعرض الورق ويرجع النقاط (true = أسود) والارتفاع.</summary>
        internal static (bool[] Dots, int Height) Render(Receipt r, int width)
        {
            float scale = width / (float)EscPos.Width58;
            int pad = (int)(6 * scale), inner = width - 2 * pad, gap = (int)(4 * scale);
            float normal = 22 * scale, large = 30 * scale;

            Android.Text.TextPaint Paint(bool bold, bool big)
            {
                var p = new Android.Text.TextPaint(Android.Graphics.PaintFlags.AntiAlias) { Color = Android.Graphics.Color.Black, TextSize = big ? large : normal };
                p.SetTypeface(bold ? Android.Graphics.Typeface.DefaultBold : Android.Graphics.Typeface.Default);
                return p;
            }

            Android.Text.StaticLayout Layout(string text, Android.Text.TextPaint paint, int w, bool center) =>
                Android.Text.StaticLayout.Builder.Obtain(text, 0, text.Length, paint, Math.Max(1, w))!
                    .SetAlignment(center ? Android.Text.Layout.Alignment.AlignCenter! : Android.Text.Layout.Alignment.AlignNormal!)!
                    .SetTextDirection(Android.Text.TextDirectionHeuristics.FirststrongRtl!)!
                    .Build()!;

            var ops = new List<(int Height, Action<Android.Graphics.Canvas, int> Draw)>();
            var all = r.Copy ? new[] { new ReceiptRow { Text = "— نسخة —", Bold = true, Center = true } }.Concat(r.Rows) : r.Rows;
            foreach (var row in all)
            {
                if (row.Rule)
                {
                    int h = (int)(14 * scale);
                    ops.Add((h, (c, y) =>
                    {
                        using var p = new Android.Graphics.Paint { Color = Android.Graphics.Color.Black, StrokeWidth = Math.Max(1, 2 * scale) };
                        p.SetPathEffect(new Android.Graphics.DashPathEffect(new[] { 6 * scale, 4 * scale }, 0));
                        c.DrawLine(pad, y + h / 2f, width - pad, y + h / 2f, p);
                    }));
                    continue;
                }
                var paint = Paint(row.Bold, row.Large);
                if (row.Value == null)
                {
                    var layout = Layout(row.Text, paint, inner, row.Center);
                    ops.Add((layout.Height + gap, (c, y) => { c.Save(); c.Translate(pad, y); layout.Draw(c); c.Restore(); }));
                }
                else
                {
                    // القيمة على اليسار، والوصف يملأ الباقى على اليمين (اتجاه عربى).
                    var valueLayout = Layout(row.Value, paint, inner / 2, false);
                    float valueW = 0;
                    for (int i = 0; i < valueLayout.LineCount; i++) valueW = Math.Max(valueW, valueLayout.GetLineWidth(i));
                    int vw = (int)Math.Ceiling(valueW);
                    var valueBox = Layout(row.Value, paint, vw, false);
                    var label = Layout(row.Text, Paint(false, false), inner - vw - gap * 2, false);
                    int h = Math.Max(label.Height, valueBox.Height) + gap;
                    ops.Add((h, (c, y) =>
                    {
                        c.Save(); c.Translate(pad, y); valueBox.Draw(c); c.Restore();
                        c.Save(); c.Translate(pad + vw + gap * 2, y); label.Draw(c); c.Restore();
                    }));
                }
            }

            int height = ops.Sum(o => o.Height) + pad * 2;
            using var bmp = Android.Graphics.Bitmap.CreateBitmap(width, height, Android.Graphics.Bitmap.Config.Argb8888!)!;
            using (var canvas = new Android.Graphics.Canvas(bmp))
            {
                canvas.DrawColor(Android.Graphics.Color.White);
                int y = pad;
                foreach (var (h, draw) in ops) { draw(canvas, y); y += h; }
            }
            var px = new int[width * height];
            bmp.GetPixels(px, 0, width, 0, 0, width, height);
            var dots = new bool[px.Length];
            for (int i = 0; i < px.Length; i++)
            {
                int c = px[i];
                int lum = (((c >> 16) & 0xFF) * 299 + ((c >> 8) & 0xFF) * 587 + (c & 0xFF) * 114) / 1000;
                dots[i] = lum < 140;
            }
            return (dots, height);
        }
#endif
    }
}
