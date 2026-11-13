using AccountModelsLib;

namespace AccountERP.AI.Providers
{
    /// <summary>بيانات الاتصال بعد فك التشفير؛ تعيش فى الذاكرة أثناء الطلب فقط ولا تُسجَّل.</summary>
    public sealed class ProviderConnection
    {
        public long ProviderId { get; init; }
        public string ProviderType { get; init; } = "";
        /// <summary>البروتوكول الفعلى: openai / anthropic / gemini / azure / ollama.</summary>
        public string Protocol { get; init; } = AiApiProtocols.OpenAI;
        public Uri BaseUrl { get; init; } = default!;
        public string? ApiKey { get; init; }
        /// <summary>اسم Header المفتاح؛ فارغ = الافتراضى للبروتوكول (Bearer / x-api-key / x-goog-api-key / api-key).</summary>
        public string? AuthHeaderName { get; init; }
        public IReadOnlyDictionary<string, string> ExtraHeaders { get; init; } = new Dictionary<string, string>();
        public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(60);
        public string? AzureDeployment { get; init; }
        public string AzureApiVersion { get; init; } = "2024-10-21";

        /// <summary>يحذف المفتاح من أى نص قبل عرضه أو حفظه (احتياط لو أعاده المزوّد فى رسالة الخطأ).</summary>
        public string Scrub(string? text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            if (!string.IsNullOrEmpty(ApiKey) && ApiKey.Length >= 6) text = text.Replace(ApiKey, "••••");
            foreach (var v in ExtraHeaders.Values)
                if (!string.IsNullOrEmpty(v) && v.Length >= 6) text = text.Replace(v, "••••");
            return text.Length > 400 ? text[..400] + "…" : text;
        }
    }

    /// <summary>واجهة كل مزوّد: اختبار الاتصال وجلب النماذج (P0) والمحادثة بالبث (P1).</summary>
    public interface IAiProviderAdapter
    {
        /// <summary>يعيد عدد النماذج المتاحة كدليل على صحة العنوان والمفتاح. يرمى AiProviderException برسالة عربية.</summary>
        Task<List<AiRemoteModel>> ListModelsAsync(ProviderConnection c, CancellationToken ct);

        /// <summary>
        /// يبثّ الرد: onDelta لكل جزء نصى. يرمى AiProviderException برسالة عربية عند الرفض أو الخطأ،
        /// وOperationCanceledException عند الإيقاف (ct)؛ المستدعى يجمع النص المبثوث من onDelta فيحفظ الجزء المكتوب.
        /// </summary>
        Task<Chat.AiChatOutcome> StreamChatAsync(ProviderConnection c, Chat.AiChatCall call, Action<string> onDelta, CancellationToken ct);
    }

    /// <summary>خطأ من المزوّد برسالة عربية جاهزة للعرض (بلا مفاتيح).</summary>
    public sealed class AiProviderException(string message) : Exception(message);
}
