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
            theme.Typography.Body1.FontSize = ".925rem";
            theme.Typography.Body2.FontFamily = arabicFonts;
            theme.Typography.Body2.FontSize = ".875rem";
            theme.Typography.Button.FontFamily = arabicFonts;

            return theme;
        }
    }
}
