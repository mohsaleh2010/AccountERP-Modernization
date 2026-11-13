using System.Text.Json;
using AccountERP.AI.Providers;

namespace AccountERP.AI.SignIn
{
    /// <summary>
    /// OpenRouter OAuth PKCE (https://openrouter.ai/docs/guides/overview/auth/oauth): بلا تسجيل تطبيق ولا Client ID.
    /// رابط التفويض https://openrouter.ai/auth ثم POST https://openrouter.ai/api/v1/auth/keys {code, code_verifier, code_challenge_method} ← {key}.
    /// وضع «الكود» (بلا callback_url): يعرض OpenRouter كوداً يلصقه المستخدم — يُستخدم حين يكون التطبيق على http غير localhost.
    /// العنوانان ثابتان هنا ولا يُقرآن من BaseUrl الصف، فلا يُرسل الكود والـ verifier لعنوان آخر أبداً.
    /// </summary>
    internal static class OpenRouterSignIn
    {
        private const string AuthorizeEndpoint = "https://openrouter.ai/auth";
        private static readonly Uri ExchangeEndpoint = new("https://openrouter.ai/api/v1/auth/keys");
        public const string CallbackPath = "/ai/signin/callback";

        public static string BuildAuthorizeUrl(string verifier, string state, string? callbackUrl)
        {
            var q = new List<string>
            {
                "code_challenge=" + Uri.EscapeDataString(AiSignInStore.Challenge(verifier)),
                "code_challenge_method=S256",
                "key_label=" + Uri.EscapeDataString("AccountERP"),
            };
            if (callbackUrl != null)
            {
                q.Insert(0, "callback_url=" + Uri.EscapeDataString(callbackUrl));
                q.Add("state=" + Uri.EscapeDataString(state));
            }
            return AuthorizeEndpoint + "?" + string.Join("&", q);
        }

        /// <summary>يبدّل الكود بمفتاح API. يرمى AiProviderException برسالة عربية (بلا الكود أو المفتاح).</summary>
        public static async Task<string> ExchangeAsync(string code, string verifier, CancellationToken ct)
        {
            string body = JsonSerializer.Serialize(new { code, code_verifier = verifier, code_challenge_method = "S256" });
            using var doc = await AiHttp.PostJsonAsync(ExchangeEndpoint, body, TimeSpan.FromSeconds(30), ct, secrets: new[] { code, verifier });
            if (doc.RootElement.ValueKind == JsonValueKind.Object && doc.RootElement.TryGetProperty("key", out var k)
                && k.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(k.GetString()))
                return k.GetString()!.Trim();
            throw new AiProviderException("لم يُرجع OpenRouter مفتاحاً؛ أعد المحاولة.");
        }
    }
}
