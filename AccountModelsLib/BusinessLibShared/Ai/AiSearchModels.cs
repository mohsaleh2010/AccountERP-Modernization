using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AccountModelsLib
{
    // ───────────── P8.2: لوحة البحث السريع Ctrl+K (.claude/docs/ai-assistant-p82-plan.md) ─────────────

    /// <summary>أنواع نتائج اللوحة (Kind). المجموعات تُعرض بهذا الترتيب (س2).</summary>
    public static class AiSearchKinds
    {
        public const string Screen = "screen";
        public const string Customer = "customer";
        public const string Supplier = "supplier";
        public const string Item = "item";
        public const string Sale = "sale";
        public const string SaleReturn = "sale_return";
        public const string Purchase = "purchase";
        public const string PurchaseReturn = "purchase_return";
        public const string Ask = "ask";

        /// <summary>نطاق «مستندات» الناتج عن بادئة فاتورة/مرتجع/# (ليس نوع نتيجة).</summary>
        public const string DocScope = "doc";

        public static readonly string[] Entities = { Customer, Supplier, Item };
        public static readonly string[] Documents = { Sale, SaleReturn, Purchase, PurchaseReturn };

        public static bool IsEntity(string kind) => Array.IndexOf(Entities, kind) >= 0;
        public static bool IsDocument(string kind) => Array.IndexOf(Documents, kind) >= 0;
        /// <summary>الأنواع التى تُحفظ فى «الأخيرة» (كل شىء عدا «اسأل المساعد»).</summary>
        public static bool IsRecentKind(string kind) => kind == Screen || IsEntity(kind) || IsDocument(kind);
    }

    public static class AiSearchConst
    {
        public const int MinChars = 2;
        public const int MaxQueryChars = 100;
        public const int MaxTokens = 4;
        public const int DebounceMs = 250;
        public const int DataTimeoutMs = 2500;
        public const int PerGroup = 5;
        public const int ScreensMax = 8;
        public const int RecentMax = 8;
        public const int RecentLoad = 50;
        public const int MaxDocDigits = 12;
        /// <summary>أطول رقم تُبنى له نطاقات بادئة (أطول منه ⇒ تطابق تام فقط).</summary>
        public const int MaxPrefixDigits = 9;
        public const string AuditTool = "palette_search";
    }

    /// <summary>نتيجة AiSearchText.Parse: ما كتبه المستخدم بعد التطبيع وتحديد النطاق.</summary>
    public class AiSearchQuery
    {
        /// <summary>النص الخام بعد القص (MaxQueryChars) وإزالة المسافات الطرفية.</summary>
        public string Raw { get; set; } = "";
        /// <summary>النص المطبّع كاملاً بلا علامة السؤال — تُبحث به الشاشات دائماً (س31).</summary>
        public string Normalized { get; set; } = "";
        /// <summary>النص الذى يُبحث به فى البيانات (بعد حذف كلمة النطاق).</summary>
        public string DataText { get; set; } = "";
        public List<string> Tokens { get; set; } = new List<string>();
        /// <summary>null = كل الأنواع؛ customer / supplier / item؛ أو doc (المستندات فقط).</summary>
        public string Scope { get; set; }
        public long? DocNumber { get; set; }
        /// <summary>؟ فى أول النص أو آخره ⇒ صف «اسأل المساعد» فقط.</summary>
        public bool AskOnly { get; set; }
        public bool HasDataText => !string.IsNullOrEmpty(DataText);
    }

    public class AiSearchHit
    {
        public string Kind { get; set; } = "";
        public long Id { get; set; }
        /// <summary>اسم الكيان أو رقم المستند.</summary>
        public string Title { get; set; } = "";
        public string Code { get; set; }
        public DateTime? Date { get; set; }
        public string Party { get; set; }
        public string Branch { get; set; }
        public decimal? Amount { get; set; }
        /// <summary>رابط داخلى يبنيه الخادم (س17).</summary>
        public string Url { get; set; } = "";
        public string RefKey { get; set; } = "";
    }

    public class AiSearchResult
    {
        public List<AiSearchHit> Hits { get; set; } = new List<AiSearchHit>();
        public bool TimedOut { get; set; }
        /// <summary>بحث البيانات موقوف من الإعداد PaletteDataSearch (س33).</summary>
        public bool Disabled { get; set; }
        /// <summary>أنواع حُذفت من البحث مع أن المستخدم يملكها (مثلاً بلا فروع مسموحة).</summary>
        public List<string> SkippedKinds { get; set; } = new List<string>();
        public long DurationMs { get; set; }
        public string Error { get; set; }
    }

    /// <summary>صف AI_SearchRecent (س19–س21).</summary>
    public class AiSearchRecentRow
    {
        public long Id { get; set; }
        public long User_Id { get; set; }
        public string Kind { get; set; } = "";
        public string RefKey { get; set; } = "";
        public string Title { get; set; } = "";
        public string Subtitle { get; set; }
        public string Url { get; set; } = "";
        public int HitCount { get; set; }
        public DateTime LastUsedAt { get; set; }
        public DateTime? HiddenAt { get; set; }
    }

    /// <summary>
    /// بوابة بحث البيانات فى اللوحة (ينفّذها AccountERP.AI فى P8.2.3 ويسجّلها مضيف الويب فقط؛ بدونها تعمل اللوحة بالشاشات فقط — س32).
    /// لا استدعاء نموذج أبداً (س1). Touch/ClearRecent تفرضان user.UserId.
    /// </summary>
    public interface IAiSearchGateway
    {
        List<string> AllowedKinds(AiUserContext user);
        Task<AiSearchResult> SearchAsync(string text, AiUserContext user, CancellationToken ct);
        List<AiSearchRecentRow> GetRecent(AiUserContext user, int top);
        void Touch(string kind, string refKey, string title, string subtitle, string url, AiUserContext user);
        void ClearRecent(long userId);
    }
}
