using System.Text;
using AccountModelsLib;
using Microsoft.Extensions.DependencyInjection;

namespace AccountERP.SharedUI.Services
{
    /// <summary>
    /// P8.3.3: شرح خطأ الفاتورة الإلكترونية فى الواجهة (Scoped، نمط AiBriefService). واجهة رقيقة فوق IAiEInvoiceGateway الذى يسجّله
    /// مضيف الويب فقط (ف23) — بدونه IsAvailable = false فلا يظهر الزر. المطابقة ثابتة بلا نموذج (ف3)، قراءة فقط (ف24)،
    /// والصلاحية تتحقق منها البوابة على الخادم (ف13). كل الاستدعاءات خارج خيط الرسم بـ Task.Run.
    /// </summary>
    public sealed class AiEInvoiceService
    {
        private readonly AssistantSessionService? _chat;
        private readonly IAiEInvoiceGateway? _gw;

        public AiEInvoiceService(IServiceProvider sp)
        {
            _chat = sp.GetService<AssistantSessionService>();
            _gw = sp.GetService<IAiEInvoiceGateway>();
        }

        /// <summary>البوابة مسجّلة + مستخدم داخل + يملك شاشة فواتير المبيعات (ف13).</summary>
        public bool IsAvailable
        {
            get
            {
                if (_gw == null || _chat == null || _chat.UserId <= 0) return false;
                try { return _gw.CanUse(_chat.BuildUser()); }
                catch { return false; }
            }
        }

        /// <summary>«شرح مفصّل من المساعد» متاح (ف17): للمستخدم صلاحية المساعد.</summary>
        public bool CanAskAssistant => _chat != null && _chat.HasAccess;

        private static Task<T> Bg<T>(Func<T> f) => Task.Run(() => { AccountCultureGuard.EnsureInvariant(); return f(); });

        private IAiEInvoiceGateway Gw => _gw ?? throw new InvalidOperationException("شرح الفاتورة الإلكترونية غير متاح.");
        private AiUserContext User() => _chat?.BuildUser() ?? throw new InvalidOperationException("شرح الفاتورة الإلكترونية غير متاح.");

        public async Task<AiEInvoiceStatus> GetStatusAsync(long saleId, string docKind)
        {
            var gw = Gw; var user = User();
            return await Bg(() => gw.GetStatus(saleId, docKind, user));
        }

        public async Task<AiEInvoiceExplainResult> ExplainAsync(string errorText, long? saleId, string docKind, CancellationToken ct = default)
        {
            var gw = Gw; var user = User();
            return await Task.Run(async () => { AccountCultureGuard.EnsureInvariant(); return await gw.ExplainAsync(errorText, saleId, docKind, user, ct); }, ct);
        }

        /// <summary>
        /// ف17: محادثة جديدة برسالة مركّبة (رقم الفاتورة ونوعها ونص الخطأ) تطلب الشرح من الكتالوج عبر الأداة. false = لا صلاحية أو رد جارٍ.
        /// </summary>
        public bool TryAskAssistant(string errorText, long saleId, long? invoiceNo, string docKind, string route, string title, string language)
        {
            if (_chat == null || !_chat.HasAccess) return false;
            var text = ComposeAsk(errorText, saleId, invoiceNo, docKind);
            return _chat.TryAskNew(text, new AiScreenContext { Route = route ?? "", Title = title ?? "", Language = language ?? "ar" });
        }

        /// <summary>نص الرسالة إلى المساعد (عام للاختبار). نص الخطأ يُقص إلى الحد (ف20).</summary>
        public static string ComposeAsk(string? errorText, long saleId, long? invoiceNo, string docKind)
        {
            var err = (errorText ?? "").Trim();
            if (err.Length > AiEInvoiceConst.MaxErrorChars) err = err.Substring(0, AiEInvoiceConst.MaxErrorChars);
            var sb = new StringBuilder();
            sb.Append("اشرح لى خطأ الفاتورة الإلكترونية ");
            sb.Append(docKind == AiEInvoiceConst.DocReturn ? "لمرتجع المبيعات" : "لفاتورة المبيعات");
            if (invoiceNo is > 0) sb.Append(" رقم ").Append(invoiceNo.Value);
            if (saleId > 0) sb.Append(" (المعرّف ").Append(saleId).Append(')');
            sb.Append(" وأين أصحّحه فى النظام، مستعملاً أداة ").Append(AiEInvoiceConst.ToolName).Append(" وكتالوج الأخطاء فقط.");
            if (err.Length > 0) sb.Append("\n\nنص الخطأ:\n").Append(err);
            else sb.Append("\n\nلا يوجد نص خطأ ملصوق؛ استعمل النص المحفوظ مع آخر إرسال إن وُجد.");
            return sb.ToString();
        }

        /// <summary>«نسخ الشرح»: نص عادى لكل المطابقات.</summary>
        public static string PlainText(AiEInvoiceExplainResult r, Func<string, string> typeLabel, string unmatchedTitle)
        {
            var sb = new StringBuilder();
            foreach (var m in r.Matches)
            {
                sb.Append("■ ").Append(m.Title).Append(" (").Append(typeLabel(m.MatchType)).Append(")\n");
                sb.Append(m.Content.Trim()).Append("\n\n");
            }
            if (r.Unmatched.Count > 0)
            {
                sb.Append(unmatchedTitle).Append(":\n");
                foreach (var u in r.Unmatched) sb.Append("- ").Append(u).Append('\n');
            }
            return sb.ToString().Trim();
        }
    }
}
