using Microsoft.AspNetCore.DataProtection;

namespace AccountERP.AI.Security
{
    /// <summary>
    /// تشفير مفاتيح المزوّدين وHeaders الإضافية بـ Data Protection (purpose: AI.ProviderKeys). النص الصريح لا يُخزَّن ولا يُسجَّل ولا يعود للواجهة؛
    /// الواجهة ترى المقنّع فقط (••••1234). تنبيه نشر: مسار مفاتيح Data Protection يجب أن يكون ثابتاً (DataProtection:KeysPath فى appsettings)
    /// وإلا تعذّر فك المفاتيح بعد إعادة النشر فيلزم إدخالها من جديد.
    /// </summary>
    public sealed class AiKeyProtector
    {
        private readonly IDataProtector _protector;

        public AiKeyProtector(IDataProtectionProvider provider)
        {
            _protector = provider.CreateProtector("AI.ProviderKeys");
        }

        public string? Protect(string? plain) => string.IsNullOrEmpty(plain) ? null : _protector.Protect(plain);

        /// <summary>null إن لم يوجد مفتاح. يرمى رسالة واضحة إن تعذّر الفك (تغيّر مفاتيح Data Protection).</summary>
        public string? Unprotect(string? protectedValue)
        {
            if (string.IsNullOrEmpty(protectedValue)) return null;
            try { return _protector.Unprotect(protectedValue); }
            catch (System.Security.Cryptography.CryptographicException)
            {
                throw new InvalidOperationException("تعذّر فك تشفير المفتاح المحفوظ (تغيّرت مفاتيح التشفير على الخادم). أدخل المفتاح من جديد واحفظ.");
            }
        }

        /// <summary>آخر 4 أحرف فقط للعرض المقنّع.</summary>
        public static string? Hint(string? plain) => string.IsNullOrEmpty(plain) ? null : (plain.Length <= 4 ? plain[^1..] : plain[^4..]);

        public static string Mask(string? hint) => string.IsNullOrEmpty(hint) ? "" : "••••" + hint;
    }
}
