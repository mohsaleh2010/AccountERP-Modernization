using System.Security.Cryptography;
using System.Text;

namespace AccountERP.SalesRepApp.Core
{
    /// <summary>
    /// جلسة الجهاز: التفعيل برمز المكتب، الدخول بالـ PIN (على السيرفر، أو بلا اتصال حتى 7 أيام من آخر دخول على السيرفر — م7)،
    /// وتوكن الوصول القصير يُجدَّد تلقائياً بتوكن التجديد (يُحفظ الجديد فوراً لأن السيرفر يبدّله مع كل استعمال).
    /// </summary>
    public sealed class RepSession
    {
        public static readonly TimeSpan OfflineLoginWindow = TimeSpan.FromDays(7);
        public static readonly TimeSpan OfflineSaleWindow = TimeSpan.FromHours(72);
        private const string RefreshKey = "salesrep.refresh_token";
        private const int PinIterations = 100_000;

        private readonly IRepApi _api;
        private readonly ISecretStore _secrets;
        private readonly LocalStore _store;
        private readonly Func<DateTime> _now;
        private string? _access;
        private DateTime _accessUntil;

        public RepSession(IRepApi api, ISecretStore secrets, LocalStore store, Func<DateTime>? now = null)
        {
            _api = api; _secrets = secrets; _store = store; _now = now ?? (() => DateTime.Now);
        }

        public bool IsActivated => _store.Get("device_id") != null;
        public bool LoggedIn { get; private set; }
        /// <summary>دخل بلا اتصال: البيع مسموح داخل نافذة الـ 72 ساعة، والمزامنة تُجرَّب لاحقاً.</summary>
        public bool OfflineLogin { get; private set; }
        public long RepId => long.TryParse(_store.Get("rep_id"), out long v) ? v : 0;
        public string RepName => _store.Get("rep_name") ?? "";
        public DateTime? LastSyncOk => _store.GetTime("last_sync_ok");

        /// <summary>معرّف ثابت للجهاز يُنشأ مرة (يبقى بعد إعادة التفعيل فيستمر الصادر لنفس الجهاز على السيرفر).</summary>
        public string DeviceUid
        {
            get
            {
                string? uid = _store.Get("device_uid");
                if (uid == null) { uid = "rep-" + Guid.NewGuid().ToString("N"); _store.Set("device_uid", uid); }
                return uid;
            }
        }

        public static void ValidatePin(string pin)
        {
            if (string.IsNullOrEmpty(pin) || pin.Length < 4 || pin.Length > 8 || !pin.All(char.IsAsciiDigit))
                throw new RepApiException(System.Net.HttpStatusCode.BadRequest, "weak_pin", "الـ PIN من 4 إلى 8 أرقام.");
        }

        public async Task ActivateAsync(string code, string pin, string platform, string deviceName, string appVersion)
        {
            ValidatePin(pin);
            var g = await _api.ActivateAsync(code.Trim(), DeviceUid, platform, deviceName, appVersion, pin);
            await AcceptGrant(g);
            _store.Set("device_id", g.Device_Id.ToString());
            _store.Set("rep_id", g.Rep_Id.ToString());
            _store.Set("rep_name", g.Rep_Name);
            _store.Set("pin_hash", HashPin(pin));
            _store.SetTime("last_online_login", _now());
            LoggedIn = true; OfflineLogin = false;
        }

        /// <summary>الدخول: على السيرفر أولاً؛ بلا شبكة يُقبل الـ PIN المحلى داخل نافذة الأيام السبعة. فقد الصلاحية يلغى التفعيل.</summary>
        public async Task LoginAsync(string pin, string appVersion)
        {
            if (!IsActivated) throw new InvalidOperationException("الجهاز غير مفعّل.");
            string? refresh = await _secrets.GetAsync(RefreshKey);
            try
            {
                if (refresh == null) throw new RepApiException(System.Net.HttpStatusCode.Unauthorized, "invalid_token", "فعّل الجهاز برمز جديد من المكتب.");
                var g = await _api.LoginAsync(DeviceUid, refresh, pin, appVersion);
                await AcceptGrant(g);
                _store.Set("pin_hash", HashPin(pin));
                _store.Set("rep_name", g.Rep_Name);
                _store.SetTime("last_online_login", _now());
                LoggedIn = true; OfflineLogin = false;
            }
            catch (RepOfflineException)
            {
                var last = _store.GetTime("last_online_login");
                if (last == null || _now() - last.Value > OfflineLoginWindow)
                    throw new InvalidOperationException("مرّ أكثر من 7 أيام على آخر دخول بالاتصال؛ اتصل بالإنترنت للدخول.");
                if (!VerifyPin(pin, _store.Get("pin_hash")))
                    throw new RepApiException(System.Net.HttpStatusCode.Unauthorized, "invalid_pin", "الـ PIN غير صحيح.");
                LoggedIn = true; OfflineLogin = true;
            }
            catch (RepApiException e) when (e.SessionLost)
            {
                await DeactivateAsync();
                throw;
            }
        }

        public void Logout() { LoggedIn = false; _access = null; }

        /// <summary>توكن وصول صالح (يجدّده قبل انتهائه بدقيقة).</summary>
        public async Task<string> AccessTokenAsync()
        {
            if (_access != null && _now() < _accessUntil) return _access;
            string refresh = await _secrets.GetAsync(RefreshKey) ?? throw new RepApiException(System.Net.HttpStatusCode.Unauthorized, "invalid_token", "فعّل الجهاز برمز جديد من المكتب.");
            try
            {
                var g = await _api.RefreshAsync(DeviceUid, refresh);
                await AcceptGrant(g);
                OfflineLogin = false;
                return _access!;
            }
            catch (RepApiException e) when (e.SessionLost)
            {
                await DeactivateAsync();
                throw;
            }
        }

        /// <summary>يمسح بيانات الدخول فقط؛ الصادر والبيانات المنزَّلة تبقى (تُرفع بعد التفعيل من جديد لنفس الجهاز).</summary>
        public async Task DeactivateAsync()
        {
            await _secrets.RemoveAsync(RefreshKey);
            _store.Set("device_id", null);
            _store.Set("pin_hash", null);
            _access = null;
            LoggedIn = false;
        }

        /// <summary>م7: البيع بلا اتصال مسموح 72 ساعة من آخر مزامنة ناجحة.</summary>
        public (bool Ok, string? Reason) CanSellOffline()
        {
            var last = LastSyncOk;
            if (last == null) return (false, "لم تكتمل أول مزامنة بعد.");
            if (_now() - last.Value > OfflineSaleWindow) return (false, "مرّ أكثر من 72 ساعة على آخر مزامنة؛ اتصل بالإنترنت وزامِن أولاً.");
            return (true, null);
        }

        internal void MarkSynced() => _store.SetTime("last_sync_ok", _now());

        private async Task AcceptGrant(SessionGrant g)
        {
            await _secrets.SetAsync(RefreshKey, g.Refresh_Token);   // قبل أى شىء: السيرفر أبطل السابق
            _access = g.Access_Token;
            _accessUntil = _now().AddSeconds(Math.Max(60, g.Expires_In) - 60);
        }

        public static string HashPin(string pin)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(pin), salt, PinIterations, HashAlgorithmName.SHA256, 32);
            return $"{PinIterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        public static bool VerifyPin(string pin, string? stored)
        {
            if (string.IsNullOrEmpty(stored)) return false;
            var p = stored.Split('.');
            if (p.Length != 3 || !int.TryParse(p[0], out int it)) return false;
            byte[] actual = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(pin ?? ""), Convert.FromBase64String(p[1]), it, HashAlgorithmName.SHA256, 32);
            return CryptographicOperations.FixedTimeEquals(actual, Convert.FromBase64String(p[2]));
        }
    }
}
