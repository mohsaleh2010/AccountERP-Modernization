using System.Text.Json.Nodes;

namespace AccountERP.AI.Chat
{
    /// <summary>
    /// كتلة محتوى محايدة بين المزوّدين (P2): نص، تفكير (Claude: يُعاد كما هو مع التوقيع داخل نفس الدور)، طلب أداة، أو نتيجة أداة.
    /// </summary>
    public sealed class AiBlock
    {
        public const string KText = "text", KThinking = "thinking", KToolUse = "tool_use", KToolResult = "tool_result";
        /// <summary>P6: ملف للنموذج (صورة أو PDF) بـ base64 — يُستخدم فى طلب الاستخراج فقط ولا يُحفظ فى التاريخ.</summary>
        public const string KMedia = "media";

        /// <summary>P6: image/png، image/jpeg، image/webp، application/pdf.</summary>
        public string MediaType { get; init; } = "";
        public string Base64 { get; init; } = "";
        public string FileName { get; init; } = "";
        public bool IsPdf => MediaType == "application/pdf";

        public static AiBlock Media(string mediaType, byte[] data, string fileName) =>
            new() { Kind = KMedia, MediaType = mediaType, Base64 = Convert.ToBase64String(data), FileName = fileName };

        public string Kind { get; init; } = KText;
        public string Text { get; set; } = "";
        /// <summary>توقيع كتلة التفكير لدى Claude (يجب إعادته دون تغيير).</summary>
        public string? Signature { get; set; }
        public string ToolId { get; init; } = "";
        public string ToolName { get; init; } = "";
        /// <summary>مدخلات الأداة كنص JSON (كائن).</summary>
        public string InputJson { get; set; } = "{}";
        public bool IsError { get; init; }
        /// <summary>Gemini: توقيع التفكير المصاحب لاستدعاء الدالة (thoughtSignature) يُعاد معه كما هو.</summary>
        public string? ProviderSignature { get; set; }

        public static AiBlock TextBlock(string t) => new() { Kind = KText, Text = t };
        public static AiBlock ToolResult(string id, string name, string content, bool isError) =>
            new() { Kind = KToolResult, ToolId = id, ToolName = name, Text = content, IsError = isError };
    }

    /// <summary>
    /// دور فى المحادثة: user / assistant (النظام منفصل فى AiChatCall.System). Blocks = null ⇒ نص فقط (التاريخ القديم)،
    /// وإلا فالكتل (دور المساعد: نص/تفكير/طلبات أدوات؛ دور المستخدم: نتائج الأدوات) — تُستخدم داخل دورة الأدوات للرسالة الحالية.
    /// </summary>
    public sealed record AiChatTurn(string Role, string Text)
    {
        public IReadOnlyList<AiBlock>? Blocks { get; init; }
    }

    /// <summary>تعريف أداة للنموذج: Schema = JSON Schema لكائن المدخلات (type object + properties + required).</summary>
    public sealed class AiToolDef
    {
        public string Name { get; init; } = "";
        public string Description { get; init; } = "";
        public JsonObject Schema { get; init; } = new();
    }

    /// <summary>طلب أداة أعاده النموذج.</summary>
    public sealed class AiToolUse
    {
        public string Id { get; init; } = "";
        public string Name { get; init; } = "";
        public string InputJson { get; set; } = "{}";
    }

    /// <summary>طلب محادثة موحّد لكل المزوّدين.</summary>
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
        /// <summary>P2: الأدوات المتاحة لهذا المستخدم (فارغة = محادثة نصية فقط).</summary>
        public IReadOnlyList<AiToolDef> Tools { get; init; } = Array.Empty<AiToolDef>();
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
        /// <summary>P2: الأدوات التى طلبها النموذج فى هذا الرد (فارغة = رد نهائى).</summary>
        public List<AiToolUse> ToolCalls { get; } = new();
        /// <summary>P2: كتل رد المساعد بترتيبها (نص/تفكير/طلبات أدوات) لإعادتها فى الدورة التالية.</summary>
        public List<AiBlock> Blocks { get; } = new();
    }
}
