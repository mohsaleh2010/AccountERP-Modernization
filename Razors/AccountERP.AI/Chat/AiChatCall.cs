namespace AccountERP.AI.Chat
{
    /// <summary>دور فى المحادثة كما يُرسل للمزوّد: user / assistant (النظام منفصل فى AiChatCall.System).</summary>
    public sealed record AiChatTurn(string Role, string Text);

    /// <summary>طلب محادثة موحّد لكل المزوّدين (P1: نص فقط، بلا أدوات).</summary>
    public sealed class AiChatCall
    {
        public string Model { get; init; } = "";
        public string System { get; init; } = "";
        public IReadOnlyList<AiChatTurn> Messages { get; init; } = Array.Empty<AiChatTurn>();
        public int MaxTokens { get; init; } = 8000;
        /// <summary>low / medium / high / xhigh / max؛ فارغ = افتراضى النموذج. يُرسل لـ Claude فقط.</summary>
        public string? Effort { get; init; }
        /// <summary>يُرسل فقط إن كان النموذج يدعمه (AI_Models.SupportsTemperature) — نماذج Claude الحالية ترفضه.</summary>
        public decimal? Temperature { get; init; }
    }

    /// <summary>ناتج البث بعد انتهائه. Refused = رفضه النموذج لأسباب السلامة (stop_reason refusal).</summary>
    public sealed class AiChatOutcome
    {
        public string Text { get; set; } = "";
        public int? InputTokens { get; set; }
        public int? OutputTokens { get; set; }
        public int? CacheReadTokens { get; set; }
        public string StopReason { get; set; } = "";
        public bool Refused { get; set; }
        /// <summary>الرد قُطع عند حد التوكنز.</summary>
        public bool Truncated { get; set; }
    }
}
