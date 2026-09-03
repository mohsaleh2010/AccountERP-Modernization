namespace AccountERP.SharedUI.Services
{
    // 🟢 تجريد بسيط بلا أي اعتماد على أنواع ASP.NET Core Hosting (IHttpContextAccessor/IDataProtectionProvider) -
    // آمن للتجميع على كل أهداف هذه المكتبة المشتركة بما فيها net10.0-android/ios/windows (MAUI). التنفيذ
    // الفعلي (قراءة الكوكي وفك تشفيره) موجود فقط فى AccountERP.Web حيث تتوفر هذه الأنواع بأمان، ويُسجَّل هناك
    // فقط. على الديسك توب/الموبايل تبقى هذه الخدمة غير مسجَّلة عمداً، فيعيد ServiceProvider.GetService null
    // ويتجاهلها MainLayoutMud بأمان دون أي محاولة استعادة جلسة (لا يوجد كوكي/متصفح هناك أصلاً).
    public interface IPersistedSessionReader
    {
        // يعيد الـ UserId المخزَّن فى كوكي دخول موقّع سابق لهذا الطلب الحالي، أو null إن لم يوجد/تالف/منتهي الصلاحية.
        int? TryReadUserId();
    }
}
