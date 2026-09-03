using AccountModelsLib;

namespace AccountERP.SharedUI.Services
{
    // 🟢 "Description_Language" كما وصفه المستخدم ليس عموداً فعلياً فى قاعدة البيانات، بل قيمة تُحسَب هنا
    // وقت العرض: تختار تلقائياً بين Description/Description_En/Description_Fr/Description_DE (الأعمدة
    // الحقيقية على SysConfigMenus - راجع SysConfigMenusCLS.cs وMODERNIZATION_NOTES.md) حسب لغة الواجهة
    // الحالية (LangService.CurrentLanguage). العربية تبقى دائماً fallback آمن لأي قائمة لم تُترجَم بعد
    // (خصوصاً الفرنسية/الألمانية، المُعلَّمتان IsFullyTranslated=false فى LanguageService).
    public static class MenuLocalization
    {
        public static string GetLocalizedDescription(this MenuViewModel menu, string? cultureCode)
        {
            if (menu == null) return string.Empty;

            var candidate = (cultureCode ?? string.Empty).ToLowerInvariant() switch
            {
                var c when c.StartsWith("en") => menu.Description_En,
                var c when c.StartsWith("fr") => menu.Description_Fr,
                var c when c.StartsWith("de") => menu.Description_DE,
                _ => menu.Description,
            };

            return string.IsNullOrWhiteSpace(candidate) ? menu.Description : candidate;
        }
    }
}
