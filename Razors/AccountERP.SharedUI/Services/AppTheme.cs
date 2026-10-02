using MudBlazor;

namespace AccountERP.SharedUI.Services
{
    // مصدر واحد لثيم MudBlazor (الخطوط + بناء الألوان) يُستخدم في كل الـ Layouts (MainLayoutMud وAuthLayout)
    // حتى تبقى شاشة تسجيل الدخول ومباقي الشاشات بنفس الهوية البصرية دون تكرار القيم في أكثر من ملف.
    // اللون الأساسي أصبح قابلاً للتغيير من المستخدم عبر ThemesMenu (راجع ThemeService.PrimaryColor)،
    // لذا لم يعد الثيم قيمة ثابتة واحدة بل يُبنى ديناميكياً بدالة Build(color) في كل مرة يتغيّر فيها اللون.
    public static class AppTheme
    {
        // اللون الافتراضي عند أول تشغيل + خلفية رمادية خفيفة مريحة للعين (نفس القيم المعتمدة سابقاً)
        public const string DefaultPrimaryColor = "#1976D2";
        public const string BackgroundColor = "#F8F9FA";

        // مجموعة الألوان المتاحة للاختيار فى قائمة الثيمات (ThemesMenu) - نفس أسلوب Swatches فى MudBlazor
        public static readonly string[] PrimaryColorSwatches =
        {
            DefaultPrimaryColor,
            Colors.Green.Default,
            Colors.Teal.Default,
            Colors.Indigo.Default,
            Colors.DeepPurple.Default,
            Colors.Pink.Default,
            Colors.DeepOrange.Default,
            Colors.Brown.Default,
            Colors.BlueGray.Default,
        };

        public static MudTheme Build(string primaryColor)
        {
            var theme = new MudTheme
            {
                PaletteLight = new PaletteLight
                {
                    Primary = primaryColor,
                    AppbarBackground = primaryColor,
                    Background = BackgroundColor
                },
                PaletteDark = new PaletteDark
                {
                    Primary = primaryColor
                }
            };

            // خطوط تدعم العربية بشكل مريح للعين وأنظمة الـ ERP، موحّدة عبر كل الشاشات
            var arabicFonts = new[] { "Segoe UI", "Tahoma", "Arial", "sans-serif" };
            theme.Typography.Body1.FontFamily = arabicFonts;
            // 🟢 أحجام مريحة للعين (تصميم احترافى موحّد): النص الأساسى 16px والثانوى 15px بدل 14.8/14، وسطر أعلى قليلاً،
            // وتسميات/تعليقات لا تقل عن 13px، وأزرار 15px بوزن متوسط - يُطبَّق تلقائياً على كل شاشات التطبيق.
            theme.Typography.Body1.FontFamily = arabicFonts;
            theme.Typography.Body1.FontSize = "1rem";
            theme.Typography.Body1.LineHeight = "1.6";
            theme.Typography.Body2.FontFamily = arabicFonts;
            theme.Typography.Body2.FontSize = ".9375rem";
            theme.Typography.Body2.LineHeight = "1.55";
            theme.Typography.Caption.FontFamily = arabicFonts;
            theme.Typography.Caption.FontSize = ".8125rem";
            theme.Typography.Subtitle1.FontFamily = arabicFonts;
            theme.Typography.Subtitle1.FontSize = "1.0625rem";
            theme.Typography.Subtitle2.FontFamily = arabicFonts;
            theme.Typography.Subtitle2.FontSize = ".9375rem";
            theme.Typography.H5.FontFamily = arabicFonts;
            theme.Typography.H5.FontSize = "1.5rem";
            theme.Typography.H6.FontFamily = arabicFonts;
            theme.Typography.H6.FontSize = "1.2rem";
            theme.Typography.Button.FontFamily = arabicFonts;
            theme.Typography.Button.FontSize = ".9375rem";
            theme.Typography.Button.FontWeight = "600";
            theme.LayoutProperties.DefaultBorderRadius = "10px";

            return theme;
        }
    }
}
