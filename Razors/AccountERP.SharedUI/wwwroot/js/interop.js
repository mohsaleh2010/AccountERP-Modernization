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
    },

    // 🟢 يطبع عنصراً واحداً فقط داخل iframe مخفى بأنماطه الخاصة بدل window.print للصفحة كلها.
    // سبب وجودها: إخفاء باقى الصفحة بـ visibility:hidden داخل تخطيط MudBlazor كان يترك مساحة القائمة الجانبية
    // وشريط التطبيق محجوزة، ومنطقة المحتوى نفسها تمرير داخلى - فتخرج الطباعة مقصوصة على صفحة واحدة أو مزاحة
    // أو بصفحات بيضاء زائدة. هنا يُنسخ العنصر وحده (مع تحويل أى canvas لصورة) ومعه كل أنماط الصفحة.
    downloadBase64: function (fileName, contentType, base64) {
        var bin = atob(base64), bytes = new Uint8Array(bin.length);
        for (var i = 0; i < bin.length; i++) bytes[i] = bin.charCodeAt(i);
        var url = URL.createObjectURL(new Blob([bytes], { type: contentType }));
        var a = document.createElement('a');
        a.href = url; a.download = fileName; document.body.appendChild(a); a.click();
        document.body.removeChild(a); setTimeout(function () { URL.revokeObjectURL(url); }, 1000);
    },

    printElement: function (selector, title, dir, pageCss) {
        var src = typeof selector === 'string' ? document.querySelector(selector) : selector;
        if (!src) { window.print(); return; }

        var clone = src.cloneNode(true);
        var srcCanvases = src.querySelectorAll('canvas');
        var cloneCanvases = clone.querySelectorAll('canvas');
        for (var i = 0; i < srcCanvases.length; i++) {
            try {
                var img = document.createElement('img');
                img.src = srcCanvases[i].toDataURL('image/png');
                img.style.cssText = srcCanvases[i].style.cssText;
                img.width = srcCanvases[i].width;
                img.height = srcCanvases[i].height;
                cloneCanvases[i].parentNode.replaceChild(img, cloneCanvases[i]);
            } catch (e) { /* canvas ملوث - يبقى كما هو */ }
        }

        var styles = '';
        document.querySelectorAll('link[rel="stylesheet"], style').forEach(function (n) { styles += n.outerHTML; });
        var esc = function (s) { return String(s || '').replace(/[&<>"]/g, function (c) { return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c]; }); };
        dir = dir || document.documentElement.getAttribute('dir') || 'rtl';
        var lang = document.documentElement.getAttribute('lang') || (dir === 'rtl' ? 'ar' : 'en');

        var iframe = document.createElement('iframe');
        iframe.setAttribute('aria-hidden', 'true');
        // بمقاس حقيقى خارج الشاشة (لا 0×0) حتى تُحسَب أبعاد الصور والنسب المئوية داخله صحيحة
        iframe.style.cssText = 'position:fixed;left:-10000px;top:0;width:800px;height:1100px;border:0;';
        document.body.appendChild(iframe);

        var doc = iframe.contentWindow.document;
        doc.open();
        doc.write('<!doctype html><html dir="' + dir + '" lang="' + lang + '"><head><meta charset="utf-8">' +
            '<base href="' + esc(document.baseURI) + '"><title>' + esc(title) + '</title>' + styles +
            '<style>' +
            '@page { size: A4; margin: 12mm; }' +
            'html, body { background: #fff !important; color: #000 !important; margin: 0 !important; padding: 0 !important; height: auto !important; overflow: visible !important; }' +
            'body { direction: ' + dir + '; -webkit-print-color-adjust: exact; print-color-adjust: exact; }' +
            'body * { visibility: visible !important; }' +
            '.print-area { position: static !important; width: 100% !important; }' +
            '.print-header, .print-only { display: block !important; }' +
            '.no-print { display: none !important; }' +
            'thead { display: table-header-group; } tfoot { display: table-footer-group; }' +
            'tr, img, svg { page-break-inside: avoid; break-inside: avoid; }' +
            '.print-table th, .print-table td { border-color: #888 !important; }' +
            (pageCss || '') +
            '</style></head><body></body></html>');
        doc.close();
        doc.body.appendChild(doc.importNode(clone, true));

        var printed = false;
        var go = function () {
            if (printed) return;
            printed = true;
            try {
                iframe.contentWindow.focus();
                iframe.contentWindow.print();
            } finally {
                setTimeout(function () { iframe.remove(); }, 1500);
            }
        };
        // ننتظر تحميل الأنماط والصور (الباركود) قبل فتح مربع الطباعة، مع حد أقصى احتياطى
        var pending = Array.prototype.slice.call(doc.querySelectorAll('img, link[rel="stylesheet"]')).filter(function (n) {
            return n.tagName === 'IMG' ? !n.complete : !n.sheet;
        });
        if (pending.length === 0) { setTimeout(go, 50); return; }
        var left = pending.length;
        var one = function () { if (--left <= 0) setTimeout(go, 50); };
        pending.forEach(function (n) { n.addEventListener('load', one); n.addEventListener('error', one); });
        setTimeout(go, 3000);
    }
};
