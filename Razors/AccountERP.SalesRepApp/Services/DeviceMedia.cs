namespace AccountERP.SalesRepApp.Services
{
    /// <summary>كاميرا وموقع الجهاز لإثبات المرتجع وعدم البيع. الفشل (رفض الإذن/لا كاميرا) يرجع null والمندوب يكمل بدونه إن سمح السبب.</summary>
    public static class DeviceMedia
    {
        public const int MaxPhotoBytes = 2_000_000;

        /// <summary>صورة بالكاميرا (أو من المعرض حيث لا كاميرا، كويندوز)، مضغوطة بعرض 1280 تقريباً.</summary>
        public static async Task<(byte[] Data, string ContentType)?> TakePhotoAsync()
        {
            FileResult? file;
            var options = new MediaPickerOptions { Title = "صورة إثبات", MaximumWidth = 1280, MaximumHeight = 1280, CompressionQuality = 70 };
            if (MediaPicker.Default.IsCaptureSupported) file = await MediaPicker.Default.CapturePhotoAsync(options);
            else file = (await MediaPicker.Default.PickPhotosAsync(options)).FirstOrDefault();
            if (file == null) return null;
            await using var stream = await file.OpenReadAsync();
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            if (ms.Length > MaxPhotoBytes) throw new InvalidOperationException("الصورة أكبر من 2 ميجا؛ أعد التصوير بدقة أقل.");
            return (ms.ToArray(), string.IsNullOrEmpty(file.ContentType) ? "image/jpeg" : file.ContentType);
        }

        /// <summary>الموقع الحالى بدقة متوسطة خلال 10 ثوانٍ، وإلا آخر موقع معروف، وإلا null.</summary>
        public static async Task<(decimal Lat, decimal Lng)?> LocationAsync()
        {
            try
            {
                var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted) return null;
                var loc = await Geolocation.Default.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10)))
                          ?? await Geolocation.Default.GetLastKnownLocationAsync();
                return loc == null ? null : (Math.Round((decimal)loc.Latitude, 6), Math.Round((decimal)loc.Longitude, 6));
            }
            catch (Exception) { return null; }
        }
    }
}
