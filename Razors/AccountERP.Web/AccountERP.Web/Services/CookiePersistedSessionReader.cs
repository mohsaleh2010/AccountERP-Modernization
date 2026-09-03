using AccountERP.SharedUI.Services;
using AccountModelsLib.Models;
using Microsoft.AspNetCore.DataProtection;

namespace AccountERP.Web.Services
{
    // 🟢 التنفيذ الوحيد لـ IPersistedSessionReader - آمن هنا لأن AccountERP.Web مضيف ويب ASP.NET Core
    // خالص (وليس مكتبة مشتركة متعددة الأهداف)، فلا خطر على أهداف MAUI/Android/iOS من استخدام
    // IHttpContextAccessor/IDataProtectionProvider مباشرة. راجع "/Auth/Persist" فى Program.cs للجهة
    // التى تكتب نفس هذا الكوكي أصلاً.
    public class CookiePersistedSessionReader : IPersistedSessionReader
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDataProtectionProvider _dataProtectionProvider;

        public CookiePersistedSessionReader(IHttpContextAccessor httpContextAccessor, IDataProtectionProvider dataProtectionProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _dataProtectionProvider = dataProtectionProvider;
        }

        public int? TryReadUserId()
        {
            var cookieValue = _httpContextAccessor.HttpContext?.Request.Cookies[AuthCookieConstants.CookieName];
            if (string.IsNullOrEmpty(cookieValue)) return null;

            try
            {
                var protector = _dataProtectionProvider.CreateProtector(AuthCookieConstants.ProtectorPurpose);
                var userIdText = protector.Unprotect(cookieValue);
                return int.TryParse(userIdText, out var userId) ? userId : null;
            }
            catch
            {
                // كوكي تالف، أو موقّع بمفتاح Data Protection قديم/مختلف - تجاهل بأمان بدلاً من تعطل الصفحة
                return null;
            }
        }
    }
}
