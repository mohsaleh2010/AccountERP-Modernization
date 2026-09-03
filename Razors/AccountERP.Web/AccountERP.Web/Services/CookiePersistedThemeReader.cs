using AccountERP.SharedUI.Services;

namespace AccountERP.Web.Services
{
    // 🟢 التنفيذ الوحيد لـ IPersistedThemeReader - آمن هنا لأن AccountERP.Web مضيف ويب ASP.NET Core خالص.
    // راجع "/Theme/Set" فى Program.cs للجهة التى تكتب نفس هذا الكوكي أصلاً، وThemesMenu.razor لمكان الاستدعاء.
    public class CookiePersistedThemeReader : IPersistedThemeReader
    {
        public const string CookieName = "AccountErpTheme";

        private readonly IHttpContextAccessor _httpContextAccessor;

        public CookiePersistedThemeReader(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public (bool? IsDarkMode, string? PrimaryColor) TryRead()
        {
            var cookieValue = _httpContextAccessor.HttpContext?.Request.Cookies[CookieName];
            if (string.IsNullOrEmpty(cookieValue)) return (null, null);

            // 🟢 قيمة بسيطة مفصولة بـ "|" (وضع|لون) بدل JSON - بنفس روح كوكيات المشروع الأخرى الخفيفة
            // (كوكي اللغة/الدخول فى Program.cs)، لا حاجة لتحليل JSON لقيمة بهذه البساطة.
            var parts = cookieValue.Split('|', 2);
            bool? isDark = parts.Length > 0 ? parts[0] == "dark" : null;
            string? color = parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[1]) ? parts[1] : null;
            return (isDark, color);
        }
    }
}
