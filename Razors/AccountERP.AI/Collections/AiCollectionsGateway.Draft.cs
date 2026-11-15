using AccountModelsLib;

namespace AccountERP.AI.Collections
{
    /// <summary>P8.5.4: المسودة والنسخ والإرسال (تُنفَّذ فى P8.5.4).</summary>
    public sealed partial class AiCollectionsGateway
    {
        public Task<AiCollectDraft> DraftAsync(long customerId, string language, bool useModel, bool regenerate, AiUserContext user, CancellationToken ct) =>
            throw new NotSupportedException("P8.5.4");

        public long LogCopied(long customerId, string text, string source, string language, AiUserContext user) => throw new NotSupportedException("P8.5.4");

        public AiCollectSendPreview PreviewSend(long customerId, int channel, AiUserContext user) => throw new NotSupportedException("P8.5.4");

        public AiCollectSendResult Send(long customerId, int channel, string text, string source, string language, bool confirmed, AiUserContext user) =>
            throw new NotSupportedException("P8.5.4");
    }
}
