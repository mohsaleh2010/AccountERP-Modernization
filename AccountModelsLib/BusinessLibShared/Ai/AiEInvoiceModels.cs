using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AccountModelsLib
{
    // ───────────── P8.3: شرح أخطاء الفاتورة الإلكترونية المصرية (.claude/docs/ai-assistant-p83-plan.md) ─────────────

    public static class AiEInvoiceConst
    {
        public const int MaxErrorChars = 8000;
        public const int MaxMatches = 8;
        public const int MaxApprox = 3;
        public const int MaxUnmatched = 5;
        public const int MaxFragment = 300;
        public const int MaxFragments = 20;
        public const string AuditTool = "einvoice_explain";
        public const string ToolName = "explain_einvoice_error";
        public const string DocInvoice = "invoice";
        public const string DocReturn = "return";
        /// <summary>ف8: المقاطع تظهر لمن يملك أياً من شاشتى المبيعات.</summary>
        public const string RoutesCsv = "/accounts/invoices/list,/accounts/invoices/sales-invoice";
        public static readonly string[] Routes = { "/accounts/invoices/list", "/accounts/invoices/sales-invoice" };
    }

    /// <summary>حالات الرفع المشتقة فى ai.v_einvoice_status (§1).</summary>
    public static class AiEInvoiceStates
    {
        public const string NotSent = "not_sent";
        public const string Sent = "sent";
        public const string Superseded = "superseded";
        public const string FlagOnly = "flag_only";
    }

    public static class AiEtaMatchTypes
    {
        public const string Code = "code";
        public const string Path = "path";
        public const string Step = "step";
        public const string Phrase = "phrase";
        public const string Approx = "approx";

        /// <summary>أسبقية ف4 (الأصغر أدق).</summary>
        public static int Rank(string t)
        {
            switch (t)
            {
                case Code: return 0;
                case Path: return 1;
                case Step: return 2;
                case Phrase: return 3;
                default: return 4;
            }
        }
    }

    /// <summary>ما استُخرج من نص الخطأ (AiEtaErrorMatcher.Parse).</summary>
    public class AiEtaSignals
    {
        public List<string> Codes { get; set; } = new List<string>();
        public List<string> Paths { get; set; } = new List<string>();
        public List<string> Steps { get; set; } = new List<string>();
        public string NormalizedText { get; set; } = "";
        public List<string> Fragments { get; set; } = new List<string>();
        public bool IsJson { get; set; }
        /// <summary>رسالة JSON ⇒ كودها ومسارها (لمعرفة ما فسّرته المطابقة).</summary>
        public Dictionary<string, List<string>> FragmentKeys { get; set; } = new Dictionary<string, List<string>>();
        public bool IsEmpty => Codes.Count == 0 && Paths.Count == 0 && Steps.Count == 0 && NormalizedText.Length == 0;
    }

    /// <summary>مفتاح مطابقة واحد من Keywords (ف19).</summary>
    public class AiEtaKey
    {
        public string Type { get; set; } = "";
        public string Value { get; set; } = "";
    }

    public class AiEtaMatch
    {
        public long RowId { get; set; }
        public string Key { get; set; } = "";
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public string MatchType { get; set; } = "";
        public string MatchedOn { get; set; } = "";
    }

    public class AiEInvoiceStatus
    {
        public bool Found { get; set; }
        public string DocKind { get; set; } = AiEInvoiceConst.DocInvoice;
        public long SaleId { get; set; }
        public long? InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string CustomerName { get; set; }
        public decimal? NetTotal { get; set; }
        public string UploadState { get; set; }
        public DateTime? SentDate { get; set; }
        public string Uuid { get; set; }
        public int Attempts { get; set; }
        public string ErrorsText { get; set; }
        public string PortalStatus { get; set; }
    }

    public class AiEInvoiceExplainResult
    {
        public AiEInvoiceStatus Status { get; set; }
        public List<AiEtaMatch> Matches { get; set; } = new List<AiEtaMatch>();
        public List<string> Unmatched { get; set; } = new List<string>();
        /// <summary>الشرح من نص الخطأ المحفوظ مع آخر إرسال (لا ملصوق).</summary>
        public bool UsedStoredError { get; set; }
        /// <summary>لا نص ملصوق ولا محفوظ.</summary>
        public bool NoErrorText { get; set; }
        /// <summary>النص المستعمل فعلاً (بعد القص).</summary>
        public string ErrorText { get; set; } = "";
        public string Error { get; set; }
    }

    /// <summary>
    /// P8.3: بوابة شرح الخطأ (ينفّذها AccountERP.AI ويسجّلها مضيف الويب فقط). المطابقة ثابتة بلا نموذج (ف3)، قراءة فقط،
    /// لا إرسال ولا تعديل (ف24). CanUse = ف13.
    /// </summary>
    public interface IAiEInvoiceGateway
    {
        bool CanUse(AiUserContext user);
        AiEInvoiceStatus GetStatus(long saleId, string docKind, AiUserContext user);
        Task<AiEInvoiceExplainResult> ExplainAsync(string errorText, long? saleId, string docKind, AiUserContext user, CancellationToken ct);
    }
}
