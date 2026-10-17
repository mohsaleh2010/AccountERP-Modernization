using System.Security.Cryptography;
using AccountERP.SalesRepApp.Core;

namespace AccountERP.SalesRepApp.Services
{
    /// <summary>أسرار الجهاز فى SecureStorage (Keystore على أندرويد، DPAPI على ويندوز).</summary>
    public sealed class MauiSecretStore : ISecretStore
    {
        public Task<string?> GetAsync(string key) => SecureStorage.Default.GetAsync(key);
        public Task SetAsync(string key, string value) => SecureStorage.Default.SetAsync(key, value);
        public Task RemoveAsync(string key) { SecureStorage.Default.Remove(key); return Task.CompletedTask; }
    }

    /// <summary>
    /// حالة التطبيق الواحدة: القاعدة المحلية المشفرة (مفتاح عشوائى فى SecureStorage)، عنوان السيرفر، الجلسة، المزامنة، والبيع.
    /// مزامنة خلفية كل 5 دقائق وعند عودة الشبكة (تحترم التأخير بعد الفشل)، ومزامنة يدوية فورية من الشاشات.
    /// </summary>
    public sealed class AppState : IDisposable
    {
        private const string DbKeyName = "salesrep.db_key";
        private const string ServerUrlKey = "salesrep.server_url";
        public static readonly TimeSpan BackgroundEvery = TimeSpan.FromMinutes(5);

        private readonly ISecretStore _secrets = new MauiSecretStore();
        private IDispatcherTimer? _timer;

        public LocalStore Store { get; private set; } = null!;
        public RepSession Session { get; private set; } = null!;
        public SyncEngine Sync { get; private set; } = null!;
        public SalesBook Book { get; private set; } = null!;
        public RepTracker Tracker { get; private set; } = null!;
        public bool Ready { get; private set; }
        public SyncReport? LastReport { get; private set; }
        public string AppVersion => AppInfo.Current.VersionString;
        public string ServerUrl => Preferences.Default.Get(ServerUrlKey, "");
        public bool Online => Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

        public event Action? Changed;

        public async Task InitAsync()
        {
            if (Ready) return;
            string? key = await _secrets.GetAsync(DbKeyName);
            if (key == null)
            {
                key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
                await _secrets.SetAsync(DbKeyName, key);
            }
            Store = new LocalStore(Path.Combine(FileSystem.AppDataDirectory, "salesrep.db"), key);
            Build();
            Connectivity.Current.ConnectivityChanged += OnConnectivityChanged;
            _timer = Application.Current?.Dispatcher.CreateTimer();
            if (_timer != null)
            {
                _timer.Interval = BackgroundEvery;
                _timer.Tick += async (_, _) => await BackgroundSyncAsync();
                _timer.Start();
            }
            Ready = true;
        }

        private void Build()
        {
            var api = new RepApi(new HttpClient(), string.IsNullOrEmpty(ServerUrl) ? "https://localhost/" : ServerUrl);
            Session = new RepSession(api, _secrets, Store);
            Sync = new SyncEngine(api, Store, Session);
            Book = new SalesBook(Store, Session);
            Tracker = new RepTracker(Store, Session);
        }

        /// <summary>الإصدار يشترط https؛ التطوير يقبل http للشبكة المحلية.</summary>
        public static string ValidateServerUrl(string url)
        {
            url = (url ?? "").Trim();
            if (!Uri.TryCreate(url, UriKind.Absolute, out var u)) throw new InvalidOperationException("عنوان السيرفر غير صحيح.");
#if DEBUG
            if (u.Scheme != Uri.UriSchemeHttps && u.Scheme != Uri.UriSchemeHttp) throw new InvalidOperationException("عنوان السيرفر يبدأ بـ https://");
#else
            if (u.Scheme != Uri.UriSchemeHttps) throw new InvalidOperationException("عنوان السيرفر يجب أن يبدأ بـ https:// لحماية البيانات.");
#endif
            return u.ToString();
        }

        public void SetServerUrl(string url)
        {
            Preferences.Default.Set(ServerUrlKey, ValidateServerUrl(url));
            Build();
        }

        public async Task<SyncReport> SyncNowAsync()
        {
            LastReport = await Sync.SyncAsync();
            await TrackingController.UpdateAsync(this);
            Changed?.Invoke();
            return LastReport;
        }

        private async Task BackgroundSyncAsync()
        {
            if (!Ready || !Session.LoggedIn || !Online || Sync.Running) return;
            LastReport = await Sync.SyncAsync(force: false);
            await TrackingController.UpdateAsync(this);
            Changed?.Invoke();
        }

        private async void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
        {
            if (e.NetworkAccess == NetworkAccess.Internet) await BackgroundSyncAsync();
            Changed?.Invoke();
        }

        public void NotifyChanged() => Changed?.Invoke();

        public void Dispose()
        {
            _timer?.Stop();
            Connectivity.Current.ConnectivityChanged -= OnConnectivityChanged;
            Store?.Dispose();
        }
    }
}
