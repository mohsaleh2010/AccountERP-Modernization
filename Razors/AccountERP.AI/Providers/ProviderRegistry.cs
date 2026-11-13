using System.Text.Json;
using AccountERP.AI.Security;
using AccountModelsLib;

namespace AccountERP.AI.Providers
{
    /// <summary>يختار الـ Adapter حسب نوع المزوّد/بروتوكوله، ويبنى ProviderConnection من صف AI_Providers بعد فك التشفير.</summary>
    public sealed class ProviderRegistry
    {
        private readonly AiKeyProtector _keys;
        private static readonly IAiProviderAdapter Anthropic = new AnthropicAdapter();
        private static readonly IAiProviderAdapter OpenAi = new OpenAiCompatibleAdapter();
        private static readonly IAiProviderAdapter Gemini = new GeminiAdapter();
        private static readonly IAiProviderAdapter Azure = new AzureOpenAiAdapter();
        private static readonly IAiProviderAdapter Ollama = new OllamaAdapter();

        public ProviderRegistry(AiKeyProtector keys) => _keys = keys;

        /// <summary>العنوان الافتراضى لكل نوع مدمج (فارغ = يجب إدخاله).</summary>
        public static string DefaultBaseUrl(string providerType) => providerType switch
        {
            AiProviderTypes.Anthropic => AnthropicAdapter.DefaultBaseUrl,
            AiProviderTypes.OpenAI => "https://api.openai.com/v1",
            AiProviderTypes.Gemini => "https://generativelanguage.googleapis.com/v1beta",
            AiProviderTypes.Ollama => "http://localhost:11434",
            AiProviderTypes.LmStudio => "http://localhost:1234/v1",
            AiProviderTypes.OpenRouter => "https://openrouter.ai/api/v1",
            _ => "",
        };

        public static string ProtocolOf(AiProviderRow p) => p.ProviderType switch
        {
            AiProviderTypes.Anthropic => AiApiProtocols.Anthropic,
            AiProviderTypes.Gemini => AiApiProtocols.Gemini,
            AiProviderTypes.Azure => "azure",
            AiProviderTypes.Ollama => "ollama",
            AiProviderTypes.Custom => string.IsNullOrWhiteSpace(p.ApiProtocol) ? AiApiProtocols.OpenAI : p.ApiProtocol,
            _ => AiApiProtocols.OpenAI,   // openai, lmstudio
        };

        public IAiProviderAdapter AdapterFor(string protocol) => protocol switch
        {
            AiApiProtocols.Anthropic => Anthropic,
            AiApiProtocols.Gemini => Gemini,
            "azure" => Azure,
            "ollama" => Ollama,
            _ => OpenAi,
        };

        public ProviderConnection Connect(AiProviderRow p)
        {
            var url = AiUrlPolicy.Validate(string.IsNullOrWhiteSpace(p.BaseUrl) ? DefaultBaseUrl(p.ProviderType) : p.BaseUrl);
            var extra = ReadExtra(p.ExtraJson);
            return new ProviderConnection
            {
                ProviderId = p.Id,
                ProviderType = p.ProviderType,
                Protocol = ProtocolOf(p),
                BaseUrl = url,
                ApiKey = _keys.Unprotect(p.ApiKeyEnc),
                AuthHeaderName = p.AuthHeaderName,
                ExtraHeaders = ParseHeaders(_keys.Unprotect(p.ExtraHeadersEnc)),
                Timeout = TimeSpan.FromSeconds(Math.Clamp(p.TimeoutSeconds, 5, 600)),
                AzureDeployment = extra.Deployment,
                AzureApiVersion = string.IsNullOrWhiteSpace(extra.ApiVersion) ? "2024-10-21" : extra.ApiVersion!,
            };
        }

        public sealed record ExtraSettings(string? Deployment, string? ApiVersion);

        public static ExtraSettings ReadExtra(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new ExtraSettings(null, null);
            try
            {
                using var doc = JsonDocument.Parse(json);
                string? Get(string k) => doc.RootElement.TryGetProperty(k, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;
                return new ExtraSettings(Get("deployment"), Get("apiVersion"));
            }
            catch (JsonException) { return new ExtraSettings(null, null); }
        }

        public static string? WriteExtra(string? deployment, string? apiVersion)
        {
            if (string.IsNullOrWhiteSpace(deployment) && string.IsNullOrWhiteSpace(apiVersion)) return null;
            return JsonSerializer.Serialize(new Dictionary<string, string?>
            {
                ["deployment"] = string.IsNullOrWhiteSpace(deployment) ? null : deployment.Trim(),
                ["apiVersion"] = string.IsNullOrWhiteSpace(apiVersion) ? null : apiVersion.Trim(),
            });
        }

        private static readonly HashSet<string> ForbiddenHeaders = new(StringComparer.OrdinalIgnoreCase)
        { "Host", "Content-Length", "Content-Type", "Transfer-Encoding", "Connection", "Cookie" };

        /// <summary>سطر لكل Header «الاسم: القيمة». يرمى رسالة عربية عند اسم غير صالح.</summary>
        public static Dictionary<string, string> ParseHeaders(string? text)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(text)) return dict;
            foreach (var raw in text.Split('\n'))
            {
                string line = raw.Trim();
                if (line.Length == 0) continue;
                int colon = line.IndexOf(':');
                if (colon <= 0) throw new InvalidOperationException($"سطر Header غير صالح: «{line.Split(':')[0]}». الشكل المطلوب: الاسم: القيمة");
                string name = line[..colon].Trim(), value = line[(colon + 1)..].Trim();
                ValidateHeaderName(name);
                if (ForbiddenHeaders.Contains(name)) throw new InvalidOperationException($"الـ Header «{name}» غير مسموح بتعديله.");
                dict[name] = value;
            }
            return dict;
        }

        public static void ValidateHeaderName(string name)
        {
            if (name.Length == 0 || name.Length > 100 || !name.All(ch => char.IsAsciiLetterOrDigit(ch) || "-_.!#$%&'*+^`|~".Contains(ch)))
                throw new InvalidOperationException($"اسم الـ Header «{name}» غير صالح (حروف إنجليزية وأرقام و - فقط).");
        }
    }
}
