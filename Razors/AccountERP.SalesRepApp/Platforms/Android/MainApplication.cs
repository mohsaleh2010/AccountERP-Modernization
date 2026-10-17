using Android.App;
using Android.Runtime;

namespace AccountERP.SalesRepApp
{
#if DEBUG
    // التطوير فقط: سيرفر الشبكة المحلية بـ http. الإصدار يشترط https (AppState.ValidateServerUrl).
    [Application(UsesCleartextTraffic = true)]
#else
    [Application(UsesCleartextTraffic = false)]
#endif
    public class MainApplication : MauiApplication
    {
        public MainApplication(IntPtr handle, JniHandleOwnership ownership) : base(handle, ownership) { }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}
