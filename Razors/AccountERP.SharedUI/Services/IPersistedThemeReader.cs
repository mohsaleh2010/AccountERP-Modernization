namespace AccountERP.SharedUI.Services
{
    // 🟢 نفس فكرة IPersistedSessionReader بالضبط (راجع ذلك الملف): تجريد بلا أي اعتماد على أنواع
    // ASP.NET Core Hosting، آمن للتجميع على كل أهداف هذه المكتبة المشتركة. التنفيذ الفعلي (قراءة كوكي
    // الثيم) موجود فقط فى AccountERP.Web حيث تتوفر IHttpContextAccessor بأمان، ويُسجَّل هناك فقط - على
    // الديسك توب/الموبايل تبقى غير مسجَّلة عمداً فيعيد ServiceProvider.GetService null، فيتجاهلها
    // ThemeService بأمان ويبقى الثيم على قيمه الافتراضية كما كان قبل إضافة هذا الحفظ.
    public interface IPersistedThemeReader
    {
        // يعيد اختيار الثيم المحفوظ سابقاً (وضع داكن/فاتح ولون أساسي) من كوكي هذا الطلب الحالي،
        // أو (null, null) إن لم يوجد كوكي/تالف - عندها يبقى ThemeService على قيمه الافتراضية.
        (bool? IsDarkMode, string? PrimaryColor) TryRead();
    }
}
