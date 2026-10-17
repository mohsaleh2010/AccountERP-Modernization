using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Locations;
using Android.OS;
using AccountERP.SalesRepApp.Core;
using AccountERP.SalesRepApp.Services;

namespace AccountERP.SalesRepApp
{
    /// <summary>
    /// خدمة أمامية لتسجيل مسار المندوب أثناء الوردية: إشعار ظاهر طوال التشغيل (يعرف المندوب أن موقعه يُسجَّل)، وتحديثات الموقع
    /// من GPS والشبكة كل نصف فاصل التسجيل أو 25 م. النقطة تُمرَّر لـ RepTracker على الخيط الرئيسى (القاعدة المحلية خيط واحد).
    /// </summary>
    [Service(Exported = false, ForegroundServiceType = ForegroundService.TypeLocation)]
    public class TrackingService : Service, ILocationListener
    {
        public const string ActionStop = "salesrep.track.stop";
        public const string ExtraInterval = "interval";
        private const string Channel = "salesrep.track";
        private const int NotificationId = 7101;
        private LocationManager? _lm;

        public override IBinder? OnBind(Intent? intent) => null;

        public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
        {
            if (intent?.Action == ActionStop) { StopSelf(); return StartCommandResult.NotSticky; }
            int interval = Math.Clamp(intent?.GetIntExtra(ExtraInterval, 120) ?? 120, 30, 900);
            var notification = BuildNotification();
            if (OperatingSystem.IsAndroidVersionAtLeast(29)) StartForeground(NotificationId, notification, ForegroundService.TypeLocation);
            else StartForeground(NotificationId, notification);

            _lm ??= GetSystemService(LocationService) as LocationManager;
            if (_lm != null)
            {
                _lm.RemoveUpdates(this);
                long ms = interval * 1000L / 2;
                foreach (var provider in new[] { LocationManager.GpsProvider, LocationManager.NetworkProvider })
                    if (provider != null && _lm.IsProviderEnabled(provider))
                        _lm.RequestLocationUpdates(provider, ms, 25f, this, Looper.MainLooper);
            }
            return StartCommandResult.Sticky;
        }

        private Notification BuildNotification()
        {
            var nm = (NotificationManager)GetSystemService(NotificationService)!;
            if (OperatingSystem.IsAndroidVersionAtLeast(26))
                nm.CreateNotificationChannel(new NotificationChannel(Channel, "تتبع الوردية", NotificationImportance.Low) { Description = "يظهر أثناء تسجيل مسار الوردية" });
            var open = PendingIntent.GetActivity(this, 0, new Intent(this, typeof(MainActivity)).AddFlags(ActivityFlags.SingleTop),
                PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent);
            return new AndroidX.Core.App.NotificationCompat.Builder(this, Channel)
                .SetContentTitle("تتبع مسار الوردية يعمل")!
                .SetContentText("يُسجَّل موقعك أثناء الوردية المفتوحة فقط.")!
                .SetSmallIcon(Resource.Mipmap.appicon)!
                .SetOngoing(true)!
                .SetContentIntent(open)!
                .SetPriority(AndroidX.Core.App.NotificationCompat.PriorityLow)!
                .Build()!;
        }

        public void OnLocationChanged(global::Android.Locations.Location location)
        {
            var p = new TrackPoint
            {
                Recorded_At = DateTimeOffset.FromUnixTimeMilliseconds(location.Time).LocalDateTime,
                Latitude = (decimal)location.Latitude, Longitude = (decimal)location.Longitude,
                Accuracy_M = location.HasAccuracy ? (int)Math.Round(location.Accuracy) : null,
                Speed_Kmh = location.HasSpeed ? Math.Round((decimal)location.Speed * 3.6m, 2) : null,
            };
            Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
            {
                try { IPlatformApplication.Current?.Services.GetService<AppState>()?.Tracker?.Record(p); }
                catch (Exception) { /* نقطة واحدة فائتة لا تهم؛ التالية بعد دقائق */ }
            });
        }

        public void OnProviderDisabled(string provider) { }
        public void OnProviderEnabled(string provider) { }
        public void OnStatusChanged(string? provider, Availability status, Bundle? extras) { }

        public override void OnDestroy()
        {
            _lm?.RemoveUpdates(this);
            if (OperatingSystem.IsAndroidVersionAtLeast(24)) StopForeground(StopForegroundFlags.Remove);
            base.OnDestroy();
        }
    }
}
