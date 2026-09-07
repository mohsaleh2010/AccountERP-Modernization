// يُستخدم لتحديث اتجاه واللغة على مستوى عنصر <html> فور تبديل اللغة في بيئات
// لا تُعيد رسم الصفحة بالكامل عند تغيير الثقافة (الديسك توب/الموبايل عبر MAUI Blazor Hybrid، والـ WebAssembly).
// في بيئة الويب (Blazor Server) تتم إعادة التحميل الكاملة عبر /Culture/SetCulture أصلاً، فهذا النداء غير ضروري هناك لكنه غير ضار.
window.accountErpInterop = {
    setDirection: function (dir, lang) {
        var html = document.documentElement;
        if (html) {
            html.setAttribute('dir', dir);
            html.setAttribute('lang', lang);
        }
    },

    // 🟢 يحفظ كوكي الثقافة عبر طلب خلفي (fetch) بدون أي Navigation في المتصفح.
    // سبب وجود هذه الدالة: استدعاء NavigationManager.NavigateTo(forceLoad:true) كان يعمل إعادة تحميل
    // كاملة للصفحة، وهو ما يُنهي دائرة Blazor Server الحالية (SignalR circuit) ويُعيد بناء كل الخدمات
    // الـ Scoped من جديد - بما فيها MenuStateService.IsAuthenticated - فيبدو المستخدم وكأنه "خرج"
    // فور تغيير اللغة رغم أنه لم يضغط تسجيل خروج. الكوكي نفسه لا يحتاج Navigation ليُحفَظ: أي استجابة
    // Set-Cookie من نفس الأصل (حتى عبر fetch) يخزّنها المتصفح تلقائياً.
    persistCultureCookie: function (culture) {
        try {
            var url = '/Culture/SetCulture?culture=' + encodeURIComponent(culture) + '&redirectUri=' + encodeURIComponent('/');
            fetch(url, { method: 'GET', credentials: 'same-origin' }).catch(function () { /* غير حرج: تجاهل فشل الحفظ الخلفي */ });
        } catch (e) { /* تجاهل بأمان */ }
    },

    // 🟢 يستبدل رمزاً صادراً لمرة واحدة فقط (LoginTokenStore) بكوكي دخول موقّع (Data Protection) عبر
    // "/Auth/Persist" - راجع Login.razor وAccountERP.Web\Program.cs لتفاصيل السبب المعماري الكامل.
    persistLoginCookie: function (token) {
        try {
            var url = '/Auth/Persist?token=' + encodeURIComponent(token);
            fetch(url, { method: 'POST', credentials: 'same-origin' }).catch(function () { /* غير حرج */ });
        } catch (e) { /* تجاهل بأمان */ }
    },

    // 🟢 يُستدعى عند تسجيل الخروج الفعلي لحذف كوكي الدخول - وإلا ستعيد الدائرة التالية استعادة الجلسة تلقائياً
    clearLoginCookie: function () {
        try {
            fetch('/Auth/Clear', { method: 'POST', credentials: 'same-origin' }).catch(function () { /* غير حرج */ });
        } catch (e) { /* تجاهل بأمان */ }
    },

    // 🟢 يحفظ اختيار الثيم (وضع داكن/فاتح + لون أساسي) عبر طلب خلفي بدون أي Navigation - بنفس فكرة
    // persistCultureCookie تماماً. يُستدعى من ThemesMenu.razor بعد كل تغيير فعلي للثيم حتى يُقرأ الاختيار
    // نفسه عند أي تحميل كامل لاحق للصفحة (فتح تبويب جديد، تحديث المتصفح F5، أو إعادة فتح النظام لاحقاً).
    persistThemeCookie: function (isDark, primaryColor) {
        try {
            var url = '/Theme/Set?isDark=' + (isDark ? 'true' : 'false') + '&primaryColor=' + encodeURIComponent(primaryColor);
            fetch(url, { method: 'POST', credentials: 'same-origin' }).catch(function () { /* غير حرج: تجاهل فشل الحفظ الخلفي */ });
        } catch (e) { /* تجاهل بأمان */ }
    },

    // 🟢 يُستخدَم فى AccountTreePicker.razor (زرّ "العودة لأعلى الشجرة" الطائر عند توسيع شجرة الحسابات) -
    // تمرير خاصية ElementReference من Blazor يصل هنا كعنصر DOM فعلي مباشرة، فلا حاجة لأي بحث بالمعرّف.
    scrollElementToTop: function (el) {
        try {
            if (el) el.scrollTo({ top: 0, behavior: 'smooth' });
        } catch (e) { /* تجاهل بأمان */ }
    }
};
