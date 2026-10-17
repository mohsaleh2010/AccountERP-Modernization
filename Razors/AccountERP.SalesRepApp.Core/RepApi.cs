using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AccountERP.SalesRepApp.Core
{
    /// <summary>خطأ من السيرفر برمزه (invalid_token، blocked، pin_locked…) ورسالته العربية.</summary>
    public class RepApiException : Exception
    {
        public string Code { get; }
        public HttpStatusCode Status { get; }
        public RepApiException(HttpStatusCode status, string code, string message) : base(message) { Status = status; Code = code; }
        /// <summary>الجهاز فقد صلاحيته (محظور/توكن ملغى/مندوب موقوف): يلزم تفعيل جديد.</summary>
        public bool SessionLost => Code is "blocked" or "rep_inactive" or "invalid_token" or "invalid_code";
    }

    /// <summary>تعذر الوصول للسيرفر (لا شبكة/مهلة): العمل يستمر بلا اتصال.</summary>
    public class RepOfflineException : Exception
    {
        public RepOfflineException(Exception inner) : base("لا اتصال بالسيرفر.", inner) { }
    }

    /// <summary>واجهة /api/v1 (قابلة للاستبدال فى الاختبار).</summary>
    public interface IRepApi
    {
        Task<SessionGrant> ActivateAsync(string code, string deviceUid, string platform, string deviceName, string appVersion, string pin);
        Task<SessionGrant> LoginAsync(string deviceUid, string refreshToken, string pin, string appVersion);
        Task<SessionGrant> RefreshAsync(string deviceUid, string refreshToken);
        Task<PullResult> PullAsync(string accessToken, string dataset, long? since);
        Task<List<PushResult>> PushAsync(string accessToken, IReadOnlyList<PushDoc> docs);
        Task<PushResult?> StatusAsync(string accessToken, Guid docGuid);
    }

    public sealed class RepApi : IRepApi
    {
        private readonly HttpClient _http;

        /// <param name="baseUrl">عنوان السيرفر (مثال https://erp.example.com/) — يضبطه المكتب عند التفعيل.</param>
        public RepApi(HttpClient http, string baseUrl)
        {
            _http = http;
            _http.BaseAddress = new Uri(baseUrl.EndsWith('/') ? baseUrl : baseUrl + "/");
            _http.Timeout = TimeSpan.FromSeconds(60);
        }

        public Task<SessionGrant> ActivateAsync(string code, string deviceUid, string platform, string deviceName, string appVersion, string pin) =>
            Send<SessionGrant>(HttpMethod.Post, "api/v1/devices/activate", null,
                new { Code = code, Device_Uid = deviceUid, Platform = platform, Device_Name = deviceName, App_Version = appVersion, Pin = pin });

        public Task<SessionGrant> LoginAsync(string deviceUid, string refreshToken, string pin, string appVersion) =>
            Send<SessionGrant>(HttpMethod.Post, "api/v1/auth/login", null, new { Device_Uid = deviceUid, Refresh_Token = refreshToken, Pin = pin, App_Version = appVersion });

        public Task<SessionGrant> RefreshAsync(string deviceUid, string refreshToken) =>
            Send<SessionGrant>(HttpMethod.Post, "api/v1/auth/refresh", null, new { Device_Uid = deviceUid, Refresh_Token = refreshToken });

        public Task<PullResult> PullAsync(string accessToken, string dataset, long? since) =>
            Send<PullResult>(HttpMethod.Get, $"api/v1/sync/pull?dataset={Uri.EscapeDataString(dataset)}" + (since == null ? "" : "&since=" + since), accessToken, null);

        public async Task<List<PushResult>> PushAsync(string accessToken, IReadOnlyList<PushDoc> docs) =>
            (await Send<PushResponse>(HttpMethod.Post, "api/v1/sync/push", accessToken, new { Docs = docs })).Results;

        public async Task<PushResult?> StatusAsync(string accessToken, Guid docGuid)
        {
            try { return await Send<PushResult>(HttpMethod.Get, $"api/v1/sync/status/{docGuid}", accessToken, null); }
            catch (RepApiException e) when (e.Status == HttpStatusCode.NotFound) { return null; }
        }

        private class PushResponse { public List<PushResult> Results { get; set; } = new(); }

        private async Task<T> Send<T>(HttpMethod method, string path, string? accessToken, object? body)
        {
            using var req = new HttpRequestMessage(method, path);
            if (accessToken != null) req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            if (body != null) req.Content = new StringContent(Json.Serialize(body), Encoding.UTF8, "application/json");
            HttpResponseMessage res;
            try { res = await _http.SendAsync(req); }
            catch (Exception e) when (e is HttpRequestException or TaskCanceledException) { throw new RepOfflineException(e); }
            using (res)
            {
                string text = await res.Content.ReadAsStringAsync();
                if (res.IsSuccessStatusCode) return Json.Deserialize<T>(text) ?? throw new RepApiException(res.StatusCode, "empty", "رد فارغ من السيرفر.");
                if ((int)res.StatusCode == 429) throw new RepApiException(res.StatusCode, "rate_limited", "محاولات كثيرة؛ انتظر دقيقة ثم أعد المحاولة.");
                ApiError? err = null;
                try { err = Json.Deserialize<ApiError>(text); } catch (JsonException) { }
                if ((int)res.StatusCode >= 500 || err == null || string.IsNullOrEmpty(err.Error_Code))
                    throw new RepOfflineException(new HttpRequestException($"HTTP {(int)res.StatusCode}"));
                throw new RepApiException(res.StatusCode, err.Error_Code, err.Message);
            }
        }
    }
}
