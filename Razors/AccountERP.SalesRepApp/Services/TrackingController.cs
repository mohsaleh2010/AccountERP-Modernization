namespace AccountERP.SalesRepApp.Services
{
    /// <summary>
    /// يشغّل خدمة تتبع المسار أو يوقفها حسب الحال (دخول + وردية مفتوحة + التتبع مفعّل من المكتب). يُستدعى بعد كل مزامنة وعند الخروج.
    /// أندرويد فقط؛ رفض إذن الموقع يعنى لا تتبع (والمكتب يرى غياب النقاط).
    /// </summary>
    public static class TrackingController
    {
        public static bool Running { get; private set; }

        public static async Task UpdateAsync(AppState state)
        {
#if ANDROID
            bool want = state.Ready && state.Tracker.ShouldRun;
            if (want && !Running)
            {
                if (await Permissions.RequestAsync<Permissions.LocationWhenInUse>() != PermissionStatus.Granted) return;
                if (OperatingSystem.IsAndroidVersionAtLeast(33)) await Permissions.RequestAsync<Permissions.PostNotifications>();
                Start(state.Tracker.IntervalSec);
            }
            else if (!want && Running) Stop();
#else
            await Task.CompletedTask;
#endif
        }

        public static void Stop()
        {
#if ANDROID
            var ctx = Android.App.Application.Context;
            ctx.StopService(new Android.Content.Intent(ctx, typeof(TrackingService)));
#endif
            Running = false;
        }

#if ANDROID
        private static void Start(int intervalSec)
        {
            var ctx = Android.App.Application.Context;
            var intent = new Android.Content.Intent(ctx, typeof(TrackingService)).PutExtra(TrackingService.ExtraInterval, intervalSec);
            try
            {
                if (OperatingSystem.IsAndroidVersionAtLeast(26)) ctx.StartForegroundService(intent); else ctx.StartService(intent);
                Running = true;
            }
            catch (Java.Lang.Exception) { Running = false; }   // التطبيق فى الخلفية: يبدأ عند العودة إليه
        }
#endif
    }
}
