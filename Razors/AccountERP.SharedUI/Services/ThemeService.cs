using System;
using Microsoft.Extensions.DependencyInjection;

namespace AccountERP.SharedUI.Services
{
    // خدمة بسيطة لإدارة وضع الثيم (فاتح/داكن) للواجهة، مسجّلة كـ Scoped
    // (نسخة مستقلة لكل مستخدم على الويب، ونسخة واحدة لكل جلسة تطبيق على الديسك توب/الموبايل)
    public class ThemeService
    {
        public bool IsDarkMode { get; private set; }

        // اللون الأساسي المختار حالياً من قائمة الثيمات (ThemesMenu) - يبدأ باللون الافتراضي للنظام
        public string PrimaryColor { get; private set; } = AppTheme.DefaultPrimaryColor;

        // 🔥 حدث لتحديث الـ MudThemeProvider وكل المكونات المشتركة فور تبديل الثيم (الوضع أو اللون)
        public event Action? OnThemeChanged;

        // 🟢 استعادة آخر اختيار محفوظ للمستخدم (وضع داكن/فاتح + لون أساسي) عند إنشاء هذه الخدمة لأول مرة
        // فى الجلسة/الدائرة، بنفس فكرة LanguageService لكن عبر قراءة كوكي صريحة (لا يوجد Middleware جاهز
        // مكافئ لـ RequestLocalizationMiddleware يخص الثيم). IPersistedThemeReader غير مسجَّلة إطلاقاً على
        // الديسك توب/الموبايل (لا كوكي/متصفح هناك أصلاً) فتعود GetService بـ null بأمان ويبقى الثيم على
        // قيمه الافتراضية كما كان دائماً هناك. الحفظ نفسه (بعد تغيير الثيم) يتم من ThemesMenu.razor.
        public ThemeService(IServiceProvider serviceProvider)
        {
            var saved = serviceProvider.GetService<IPersistedThemeReader>()?.TryRead();
            if (saved?.IsDarkMode is bool savedDark) IsDarkMode = savedDark;
            if (!string.IsNullOrWhiteSpace(saved?.PrimaryColor)) PrimaryColor = saved!.Value.PrimaryColor!;
        }

        public void SetDarkMode(bool isDarkMode)
        {
            if (IsDarkMode == isDarkMode) return;
            IsDarkMode = isDarkMode;
            OnThemeChanged?.Invoke();
        }

        public void ToggleDarkMode()
        {
            SetDarkMode(!IsDarkMode);
        }

        public void SetPrimaryColor(string color)
        {
            if (PrimaryColor == color) return;
            PrimaryColor = color;
            OnThemeChanged?.Invoke();
        }
    }
}
