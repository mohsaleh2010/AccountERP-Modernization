using AccountModelsLib;
using Anthropic;
using Anthropic.Exceptions;
using Anthropic.Models.Models;

namespace AccountERP.AI.Providers
{
    /// <summary>
    /// Anthropic (Claude) والمزوّد المخصص بالبروتوكول Anthropic-compatible، عبر حزمة NuGet الرسمية Anthropic (لا shim متوافق مع OpenAI).
    /// اختبار الاتصال = قائمة النماذج (GET /v1/models): يتحقق من المفتاح والعنوان بلا استهلاك توكنز.
    /// </summary>
    internal sealed class AnthropicAdapter : IAiProviderAdapter
    {
        public const string DefaultBaseUrl = "https://api.anthropic.com";

        public async Task<List<AiRemoteModel>> ListModelsAsync(ProviderConnection c, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(c.ApiKey))
                throw new AiProviderException("أدخل مفتاح الـ API الخاص بـ Anthropic أولاً.");
            using var http = AiHttp.CreateClient(c);
            // Header مخصص للمفتاح (مزوّد مخصص خلف بوابة): يُرسل لنفس العنوان المسجّل فقط.
            if (!string.IsNullOrWhiteSpace(c.AuthHeaderName) && !c.AuthHeaderName.Equals("x-api-key", StringComparison.OrdinalIgnoreCase))
                http.DefaultRequestHeaders.TryAddWithoutValidation(c.AuthHeaderName.Trim(), c.ApiKey);
            var client = new AnthropicClient
            {
                ApiKey = c.ApiKey,
                BaseUrl = c.BaseUrl.AbsoluteUri.TrimEnd('/'),
                HttpClient = http,
                Timeout = c.Timeout,
                MaxRetries = 0,
            };
            try
            {
                var page = await client.Models.List(new ModelListParams { Limit = 1000 }, ct);
                return page.Items.Select(m => new AiRemoteModel
                {
                    ModelId = m.ID,
                    DisplayName = string.IsNullOrWhiteSpace(m.DisplayName) ? m.ID : m.DisplayName,
                    SupportsVision = true,
                }).ToList();
            }
            catch (AnthropicUnauthorizedException) { throw new AiProviderException(AiHttp.Describe(System.Net.HttpStatusCode.Unauthorized, "")); }
            catch (AnthropicForbiddenException ex) { throw new AiProviderException(AiHttp.Describe(System.Net.HttpStatusCode.Forbidden, c.Scrub(ex.Message))); }
            catch (AnthropicNotFoundException) { throw new AiProviderException(AiHttp.Describe(System.Net.HttpStatusCode.NotFound, "")); }
            catch (AnthropicRateLimitException) { throw new AiProviderException(AiHttp.Describe((System.Net.HttpStatusCode)429, "")); }
            catch (AnthropicApiException ex) { throw new AiProviderException("رفض Anthropic الطلب: " + c.Scrub(ex.Message)); }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                throw new AiProviderException($"انتهت مهلة الاتصال ({(int)c.Timeout.TotalSeconds} ثانية).");
            }
            catch (AnthropicException ex) { throw new AiProviderException("تعذّر الاتصال بـ Anthropic: " + c.Scrub(ex.InnerException?.Message ?? ex.Message)); }
            catch (HttpRequestException ex) { throw new AiProviderException("تعذّر الوصول للعنوان " + c.BaseUrl.Host + ": " + c.Scrub(ex.Message)); }
        }
    }
}
