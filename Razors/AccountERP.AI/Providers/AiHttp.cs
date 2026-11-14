using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace AccountERP.AI.Providers
{
    /// <summary>
    /// اتصال HTTP مشترك للمزوّدين غير Anthropic. التحويل (Redirect) معطّل حتى لا يُرسل المفتاح لأى عنوان غير المسجّل،
    /// والأخطاء تتحول لرسائل عربية واضحة (عنوان خاطئ، مفتاح مرفوض، نموذج غير موجود، مهلة).
    /// </summary>
    internal static class AiHttp
    {
        private static readonly SocketsHttpHandler Handler = new()
        {
            AllowAutoRedirect = false,
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
        };

        /// <summary>HttpClient جديد فوق المعالج المشترك؛ يُطبَّق عليه Headers الإضافية (لـ SDK Anthropic أيضاً).</summary>
        public static HttpClient CreateClient(ProviderConnection c)
        {
            var http = new HttpClient(Handler, disposeHandler: false) { Timeout = Timeout.InfiniteTimeSpan };
            foreach (var (name, value) in c.ExtraHeaders)
                http.DefaultRequestHeaders.TryAddWithoutValidation(name, value);
            return http;
        }

        /// <summary>يرسل GET ويعيد JSON. defaultAuthHeader: null = Bearer.</summary>
        public static Task<JsonDocument> GetJsonAsync(ProviderConnection c, string relativeOrAbsolute, string? defaultAuthHeader, CancellationToken ct) =>
            SendJsonAsync(c, HttpMethod.Get, relativeOrAbsolute, null, defaultAuthHeader, c.Timeout, ct);

        /// <summary>P7.2: POST JSON بمفتاح المزوّد ويعيد JSON (التضمين). timeout = مهلة هذا الطلب.</summary>
        public static Task<JsonDocument> PostJsonAsync(ProviderConnection c, string relativeOrAbsolute, string jsonBody, string? defaultAuthHeader, TimeSpan timeout, CancellationToken ct) =>
            SendJsonAsync(c, HttpMethod.Post, relativeOrAbsolute, jsonBody, defaultAuthHeader, timeout, ct);

        private static async Task<JsonDocument> SendJsonAsync(ProviderConnection c, HttpMethod method, string relativeOrAbsolute, string? jsonBody, string? defaultAuthHeader, TimeSpan timeout, CancellationToken ct)
        {
            var url = relativeOrAbsolute.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                ? new Uri(relativeOrAbsolute)
                : new Uri(c.BaseUrl.AbsoluteUri.TrimEnd('/') + "/" + relativeOrAbsolute.TrimStart('/'));
            using var http = CreateClient(c);
            using var req = new HttpRequestMessage(method, url);
            if (jsonBody != null) req.Content = new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json");
            ApplyAuth(req, c, defaultAuthHeader);
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(timeout);
            HttpResponseMessage resp;
            try
            {
                resp = await http.SendAsync(req, HttpCompletionOption.ResponseContentRead, cts.Token);
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                throw new AiProviderException($"انتهت مهلة الاتصال ({Math.Max(1, (int)timeout.TotalSeconds)} ثانية). تأكد أن الخدمة تعمل وأن العنوان صحيح.");
            }
            catch (HttpRequestException ex)
            {
                throw new AiProviderException("تعذّر الوصول للعنوان " + c.BaseUrl.Host + ": " + c.Scrub(ex.InnerException?.Message ?? ex.Message));
            }
            using (resp)
            {
                string body = await resp.Content.ReadAsStringAsync(ct);
                if (!resp.IsSuccessStatusCode)
                    throw new AiProviderException(Describe(resp.StatusCode, c.Scrub(ExtractError(body))));
                try { return JsonDocument.Parse(body); }
                catch (JsonException)
                {
                    throw new AiProviderException("الخدمة ردّت بمحتوى ليس JSON؛ غالباً العنوان لا يشير لواجهة API (راجع الجزء الأخير مثل /v1).");
                }
            }
        }

        /// <summary>POST JSON بلا مفتاح (تبديل كود تسجيل الدخول). secrets تُحذف من أى رسالة خطأ.</summary>
        public static async Task<JsonDocument> PostJsonAsync(Uri url, string jsonBody, TimeSpan timeout, CancellationToken ct, IEnumerable<string>? secrets = null)
        {
            string Scrub(string text)
            {
                foreach (var s in secrets ?? Array.Empty<string>())
                    if (!string.IsNullOrEmpty(s) && s.Length >= 6) text = text.Replace(s, "••••");
                return text.Length > 300 ? text[..300] + "…" : text;
            }
            using var http = new HttpClient(Handler, disposeHandler: false) { Timeout = Timeout.InfiniteTimeSpan };
            using var req = new HttpRequestMessage(HttpMethod.Post, url) { Content = new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json") };
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(timeout);
            HttpResponseMessage resp;
            try { resp = await http.SendAsync(req, HttpCompletionOption.ResponseContentRead, cts.Token); }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested) { throw new AiProviderException("انتهت مهلة الاتصال بالمزوّد؛ أعد المحاولة."); }
            catch (HttpRequestException ex) { throw new AiProviderException("تعذّر الوصول إلى " + url.Host + ": " + Scrub(ex.InnerException?.Message ?? ex.Message)); }
            using (resp)
            {
                string body = await resp.Content.ReadAsStringAsync(ct);
                if (!resp.IsSuccessStatusCode)
                {
                    string head = (int)resp.StatusCode switch
                    {
                        400 or 403 => "رفض المزوّد الكود: غير صحيح أو منتهى (صلاحيته 10 دقائق) أو استُخدم من قبل. أعد تسجيل الدخول.",
                        _ => Describe(resp.StatusCode, ""),
                    };
                    string detail = Scrub(ExtractError(body));
                    throw new AiProviderException(string.IsNullOrWhiteSpace(detail) ? head : head + " — " + detail);
                }
                try { return JsonDocument.Parse(body); }
                catch (JsonException) { throw new AiProviderException("ردّ غير متوقع من المزوّد (ليس JSON)."); }
            }
        }

        /// <summary>
        /// P1: POST JSON لبثّ الرد (SSE أو NDJSON). المهلة تُطبَّق حتى وصول الرؤوس فقط؛ بعدها يستمر البث حتى ينتهى أو يُلغى (زر الإيقاف).
        /// عند رد غير ناجح يقرأ الجسم ويرمى AiProviderException برسالة عربية. المستدعى يتخلص من الرد.
        /// </summary>
        public static async Task<HttpResponseMessage> PostStreamAsync(ProviderConnection c, Uri url, string jsonBody, string? defaultAuthHeader, CancellationToken ct)
        {
            var http = CreateClient(c);
            var req = new HttpRequestMessage(HttpMethod.Post, url) { Content = new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json") };
            req.Headers.Accept.ParseAdd("text/event-stream");
            req.Headers.Accept.ParseAdd("application/json");
            ApplyAuth(req, c, defaultAuthHeader);
            using var headersCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            headersCts.CancelAfter(c.Timeout);
            HttpResponseMessage resp;
            try
            {
                resp = await http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, headersCts.Token);
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                http.Dispose();
                throw new AiProviderException($"انتهت مهلة الاتصال ({(int)c.Timeout.TotalSeconds} ثانية) قبل أن يبدأ المزوّد الرد.");
            }
            catch (HttpRequestException ex)
            {
                http.Dispose();
                throw new AiProviderException("تعذّر الوصول للعنوان " + c.BaseUrl.Host + ": " + c.Scrub(ex.InnerException?.Message ?? ex.Message));
            }
            if (!resp.IsSuccessStatusCode)
            {
                string body;
                try { body = await resp.Content.ReadAsStringAsync(ct); }
                finally { resp.Dispose(); http.Dispose(); }
                throw new AiProviderException(Describe(resp.StatusCode, c.Scrub(ExtractError(body))));
            }
            return resp;   // HttpClient فوق المعالج المشترك (disposeHandler:false) فلا يلزم التخلص منه بعد انتهاء البث
        }

        /// <summary>يقرأ أسطر الرد واحداً تلو الآخر (SSE: أسطر data:، أو NDJSON).</summary>
        public static async IAsyncEnumerable<string> ReadLinesAsync(HttpResponseMessage resp, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
        {
            await using var stream = await resp.Content.ReadAsStreamAsync(ct);
            using var reader = new StreamReader(stream, System.Text.Encoding.UTF8);
            while (true)
            {
                string? line = await reader.ReadLineAsync(ct);
                if (line == null) yield break;
                yield return line;
            }
        }

        /// <summary>يستخرج حمولة سطر SSE («data: ...»)؛ null لغير ذلك (تعليقات، event:، أسطر فارغة).</summary>
        public static string? SseData(string line)
        {
            if (!line.StartsWith("data:", StringComparison.Ordinal)) return null;
            return line.Length > 5 && line[5] == ' ' ? line[6..] : line[5..];
        }

        /// <summary>رسالة الخطأ داخل حدث مبثوث ({error:{message}}) إن وجدت.</summary>
        public static string? StreamError(JsonElement root, ProviderConnection c)
        {
            if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty("error", out var e)) return null;
            string msg = e.ValueKind == JsonValueKind.String ? e.GetString() ?? ""
                : e.ValueKind == JsonValueKind.Object && e.TryGetProperty("message", out var m) ? m.GetString() ?? "" : e.ToString();
            return "أوقف المزوّد الرد بخطأ: " + c.Scrub(msg);
        }

        private static void ApplyAuth(HttpRequestMessage req, ProviderConnection c, string? defaultAuthHeader)
        {
            if (string.IsNullOrEmpty(c.ApiKey)) return;
            string? header = string.IsNullOrWhiteSpace(c.AuthHeaderName) ? defaultAuthHeader : c.AuthHeaderName.Trim();
            if (string.IsNullOrEmpty(header) || header.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", c.ApiKey);
            else
                req.Headers.TryAddWithoutValidation(header, c.ApiKey);
        }

        public static string Describe(HttpStatusCode code, string detail)
        {
            string head = (int)code switch
            {
                401 => "المفتاح مرفوض (401). تأكد من صحة مفتاح الـ API.",
                402 => "لا يوجد رصيد كافٍ فى حسابك لدى المزوّد (402). اشحن الرصيد من صفحة الحساب لدى المزوّد، أو اختر نموذجاً مجانياً (فى OpenRouter أسماؤها تنتهى بـ :free) أو نموذجاً محلياً.",
                403 => "الوصول مرفوض (403): المفتاح لا يملك صلاحية لهذه الخدمة أو المنطقة.",
                404 => "العنوان أو النموذج غير موجود (404). راجع الـ Base URL واسم النموذج.",
                >= 300 and < 400 => $"الخدمة حوّلت الطلب لعنوان آخر ({(int)code}) ولم يُتبع حمايةً للمفتاح. أدخل العنوان النهائى مباشرة (غالباً https أو بإضافة /v1).",
                429 =>"تجاوزت حد الطلبات أو الرصيد (429). حاول لاحقاً أو راجع الرصيد.",
                >= 500 => $"خطأ لدى المزوّد ({(int)code}). حاول لاحقاً.",
                _ => $"رفض المزوّد الطلب ({(int)code}).",
            };
            return string.IsNullOrWhiteSpace(detail) ? head : head + " — " + detail;
        }

        /// <summary>يستخرج رسالة الخطأ من أشكال JSON الشائعة ({error:{message}} / {error:"..."} / {message}).</summary>
        private static string ExtractError(string body)
        {
            if (string.IsNullOrWhiteSpace(body)) return "";
            try
            {
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;
                if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0) root = root[0];
                if (root.ValueKind == JsonValueKind.Object)
                {
                    if (root.TryGetProperty("error", out var e))
                    {
                        if (e.ValueKind == JsonValueKind.String) return e.GetString() ?? "";
                        if (e.ValueKind == JsonValueKind.Object && e.TryGetProperty("message", out var m)) return m.GetString() ?? "";
                    }
                    if (root.TryGetProperty("message", out var msg) && msg.ValueKind == JsonValueKind.String) return msg.GetString() ?? "";
                }
            }
            catch (JsonException) { }
            return body.Length > 200 ? body[..200] : body;
        }
    }
}
