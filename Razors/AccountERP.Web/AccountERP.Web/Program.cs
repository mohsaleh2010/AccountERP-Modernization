using AccountERP.SharedUI;
using AccountERP.Web.Client.Pages;
using AccountERP.Web.Components;
using AccountModelsLib;
using AccountModelsLib.Models;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.DataProtection;
using MudBlazor.Services;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using MudBlazor.Services;
var builder = WebApplication.CreateBuilder(args);
 
// 1. إضافة خدمات الـ Components والـ Localization
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// تسجيل خدمة اللغات كـ Scoped لتكون مستقلة لكل مستخدم
builder.Services.AddScoped<AccountERP.SharedUI.LanguageService>();
// 🔥 تسجيل خدمة المصادقة لربط صفحة الـ Login بقاعدة البيانات الحقيقية في الويب
// 1. جلب نص الاتصال من ملف الإعدادات
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 🔥 مهم جداً: ضبط الـ connection string الثابت (Static) المستخدم من كل كود قديم (Legacy) لا يزال
// ينشئ new AccountModelsLib.operations() بدون تمرير سترنج صراحة - وعلى رأسه Common.SetSystemParameters_Menus_DB()
// (المستدعاة من MenuRepository لجلب صلاحيات القوائم من SysConfigMenus/SysConfigMenusPERMATIONS). بدون هذا
// السطر كان يعتمد على القيمة الافتراضية المكتوبة يدوياً داخل operations.ReturnConnStr() - وهي قاعدة بيانات
// مختلفة تماماً عن قاعدة البيانات المُعدّة فعلياً فى appsettings.json لهذا المشروع - فتعود قوائم فارغة دائماً
// رغم وجود صلاحيات حقيقية للمستخدم. هذا هو نفس ما تفعله Common.SetSystemParameters() فى تطبيقات الويندوز فورمز
// عند بدء التشغيل، لكن مضيف الويب هذا لا يستدعيها إطلاقاً.
AccountModelsLib.operations.conn_str_ = connectionString ?? string.Empty;

// 🔥 يجب ضبط هذا العلم = 1 دائماً هنا وقبل أي استدعاء لـ Common.SetSystemParametersDB() أدناه: تلك الدالة
// تنتهي باستدعاء operations.ReturnConnStr()، والذي - إن كان IS_ReadConStrFromXMLInfo لا يساوي 1 - يقوم
// **بالكتابة فوق** الـ connection string الصحيح أعلاه بقيمة قديمة مكتوبة يدوياً داخل تلك الدالة (لقاعدة
// بيانات مختلفة تماماً). فى تطبيقات الويندوز فورمز يُضبط هذا العلم=1 تلقائياً من ملف ConnInfo.xml المشفر عبر
// Common.SetSystemParameters() قبل أي استدعاء مشابه؛ مضيف الويب هذا لا يقرأ ملف XML إطلاقاً (يعتمد على
// appsettings.json مباشرة أعلاه)، فيجب ضبط العلم يدوياً هنا ليكون ReturnConnStr() بلا أثر (no-op) تماماً
// كحاله على الديسك توب. راجع MODERNIZATION_NOTES.md ("SetSystemParametersDB()'s last line is dangerous...").
AccountModelsLib.Common.IS_ReadConStrFromXMLInfo = 1;

// 🔥 تحميل إعدادات النظام (SysConfig بالكامل) مرة واحدة فقط عند بدء تشغيل العملية - وليس عند كل تسجيل دخول:
// هذا هو الجزء "الخاص بكل المنشأة/قاعدة البيانات" (وليس الخاص بمستخدم بعينه) من تسلسل Accounts.LoadingFrm
// الأصلي فى تطبيق الويندوز فورمز (يعمل هناك بعد الدخول مباشرة لأن كل Process يخدم مستخدماً واحداً فقط؛ هنا
// عملية واحدة تخدم عشرات المستخدمين المتزامنين، فتحميلها مرة واحدة عند الإقلاع أدق وأوفر). التسلسل نفسه
// مجمَّع فى AccountModelsLib.SystemSettingsLoader.LoadTenantSettings() - مصدر حقيقة واحد يستدعيه هذا
// الملف وAccounts.LoadingFrm فى الويندوز فورمز كلاهما، بدل تكرار نفس الأسطر فى مكانين. راجع
// MODERNIZATION_NOTES.md لتفاصيل الخطوات والتصنيف الكامل. أي فشل هنا (قاعدة بيانات غير متاحة مؤقتاً عند
// الإقلاع) لا يجب أن يمنع التطبيق من البدء - تماماً كما يفعل Program.cs الأصلي فى الويندوز فورمز.
try { AccountModelsLib.SystemSettingsLoader.LoadTenantSettings(); } catch { }

// ترقية القاعدة كما يفعل الديسك توب عند الإقلاع (AccountsERPSystem/Program.cs): الجداول والأعمدة الناقصة وربط RazorPath الفارغ فقط؛
// بدونها لا تصل ربوط القوائم الجديدة (مثل «إدخال القيود») لقاعدة لم يُشغَّل عليها الديسك توب فتظهر «قريباً». الفشل لا يمنع الإقلاع.
try { AccountModelsLib.StockEngineSchemaCLS.EnsureSchema(); } catch (Exception ee) { AccountModelsLib.operations.DataDictionaryLogerException(ee); }
try { AccountModelsLib.SalesRepSchemaCLS.EnsureSchema(); } catch (Exception ee) { AccountModelsLib.operations.DataDictionaryLogerException(ee); }
// 🤖 المساعد الذكى P0: جداول AI_* (إدراج فقط) وعنصر قائمة «إعدادات الذكاء الاصطناعى».
try { AccountModelsLib.AiAssistantSchemaCLS.EnsureSchema(); } catch (Exception ee) { AccountModelsLib.operations.DataDictionaryLogerException(ee); }

// 2. تسجيل الخدمة وتمرير الـ string داخل الـ Constructor مانيوال
builder.Services.AddScoped<My_DbContext>(provider => new My_DbContext(connectionString));
builder.Services.AddScoped<operations>(provider => {
    var oppState = new    operations(connectionString);
    if(oppState.ConectionStaus==false)
    {
       
    }
    return oppState;
    });
 
builder.Services.AddScoped<IAuthService, WebAuthService>();

// 🔐 م6 (المرحلة 7): جلسة مستخدم مستقلة لكل دائرة/طلب بدل حقول Common الثابتة المشتركة بين كل المستخدمين المتزامنين
// (كان دخول أى مستخدم يغيّر Common.User_Id/Branch_Id للجميع). راجع AccountModelsLib/Common/UserSessionContext.cs.
builder.Services.AddScoped(_ => AccountModelsLib.UserSession.FromProcessDefaults());
builder.Services.AddScoped<Microsoft.AspNetCore.Components.Server.Circuits.CircuitHandler, AccountERP.Web.Services.UserSessionCircuitHandler>();

// 📱 المرحلة 7: حد محاولات تفعيل/دخول أجهزة المندوبين (10 فى الدقيقة لكل عنوان) ضد تخمين رمز التفعيل والـ PIN.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy(AccountERP.Web.Api.SalesRepMobileApi.AuthRateLimitPolicy, httpContext =>
        System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});

// 🔐 يتيحان استعادة جلسة الدخول بعد إعادة تحميل كاملة للصفحة (دائرة/Circuit جديدة) - راجع
// LoginTokenStore ونقطتي "/Auth/Persist" و"/Auth/Clear" أسفل هذا الملف، وتفاصيل السبب فى
// MODERNIZATION_NOTES.md (بحث عن "submenu click → back to login").
builder.Services.AddSingleton<AccountModelsLib.Models.LoginTokenStore>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AccountERP.SharedUI.Services.IPersistedSessionReader, AccountERP.Web.Services.CookiePersistedSessionReader>();

// 🎨 يتيح استعادة آخر ثيم (وضع داكن/فاتح + لون أساسي) اختاره المستخدم عبر كوكي - راجع ThemeService
// (AccountERP.SharedUI) لجهة القراءة عند بدء الدائرة، و"/Theme/Set" أسفل هذا الملف وThemesMenu.razor لجهة الحفظ.
builder.Services.AddScoped<AccountERP.SharedUI.Services.IPersistedThemeReader, AccountERP.Web.Services.CookiePersistedThemeReader>();

// داخل كود الـ CreateMauiApp / أو الـ Web Program.cs
builder.Services.AddSingleton(new AppSettings { IsWinForms = false, IsLoadMainLayout = false, IsWeb = true });
 
// 1. تسجيل كلاس الجلب (يمكن تسجيله كـ Singleton لأنه مجرد محاكي حالياً)
builder.Services.AddSingleton<AccountModelsLib.MenuRepository>();

// 2. 🔐 MenuStateService تُنشأ فارغة دائماً هنا. لا يجوز تحميل قوائم أي مستخدم قبل نجاح تسجيل الدخول الفعلي
//    (كانت هذه النقطة تُحمّل قوائم المستخدم رقم 1 تلقائياً بمجرد أول طلب للخدمة، ما يعني ظهور القوائم
//    والدخول لبوابة الحماية IsAuthenticated=true دون أي تسجيل دخول حقيقي - تم تصحيح ذلك).
//    الشحن الحقيقي بالبيانات وضبط IsAuthenticated يتمّان فقط داخل AuthService.LoginAsync عند نجاح الدخول.
builder.Services.AddScoped<AccountModelsLib.MenuStateService>();
builder.Services.AddScoped<InvoiceStateService>();
// 🗂️ مساحة عمل التبويبات (بديل نوافذ MDI فى الويندوز فورمز) - Scoped لتكون مستقلة لكل مستخدم/دائرة،
// وتُصفَّر عند تسجيل الخروج (راجع MainLayoutMud.LogoutClick وTabWorkspaceService.cs).
builder.Services.AddScoped<AccountERP.SharedUI.Services.TabWorkspaceService>();
// ⚙️ تفضيلات واجهة خاصة بكل مستخدم (إظهار الـ Breadcrumb/تفعيل التبويبات...) - محفوظة فى قاعدة البيانات
// (USER_DETAILES_Setting) لا كوكي، فتتبع حساب المستخدم عبر أي جهاز/متصفح - راجع UserPreferencesService.cs.
builder.Services.AddScoped<AccountERP.SharedUI.Services.UserPreferencesService>();
// خدمة الثيم (فاتح/داكن) - Scoped لتكون مستقلة لكل مستخدم على الويب
builder.Services.AddScoped<AccountERP.SharedUI.Services.ThemeService>();
// سجلّ الإشعارات (جرس الإشعارات فى الشريط العلوي) - Scoped لتكون مستقلة لكل مستخدم على الويب
builder.Services.AddScoped<AccountERP.SharedUI.Services.NotificationCenterService>();
// 📨 المرحلة 8: مرسل إشعارات عملاء المندوبين (SMS/WhatsApp) من طابور الإرسال.
builder.Services.AddHostedService<AccountERP.Web.Services.SalesRepNotifyWorker>();
// 🤖 المساعد الذكى (Razors/AccountERP.AI): تشفير مفاتيح المزوّدين واختبار الاتصال وجلب النماذج لشاشة /settings/ai.
AccountERP.AI.AiServiceCollectionExtensions.AddAccountErpAi(builder.Services, builder.Configuration);
builder.Services.AddHostedService<AccountERP.Web.Services.AiSchedulerWorker>();   // P8 ق17: مهام المساعد الدورية (الموجز اليومى)
// 🤖 المساعد الذكى P1: حالة المحادثة لكل دائرة (تتشاركها صفحة /assistant والنافذة العائمة).
builder.Services.AddScoped<AccountERP.SharedUI.Services.AssistantSessionService>();
builder.Services.AddScoped<AccountERP.SharedUI.Services.AiBriefService>();   // P8.1.5: صفحة الموجز والبطاقة والإشعار
builder.Services.AddScoped<AccountERP.SharedUI.Services.AiAnomalyService>();   // P8.4.3: شذوذ القيود (صفحة + تبويب) — ويب فقط (ش31)
builder.Services.AddScoped<AccountERP.SharedUI.Services.AiCollectService>();   // P8.5.2: أولويات التحصيل (صفحة + تبويب) — ويب فقط (ت33)
builder.Services.AddScoped<AccountERP.SharedUI.Services.AiEInvoiceService>();   // P8.3.3: حوار شرح خطأ الفاتورة الإلكترونية
builder.Services.AddScoped<AccountERP.SharedUI.Services.AiScreenAssistService>();   // P8.9: المساعد داخل الشاشات — ويب فقط (ص27)
builder.Services.AddScoped<AccountERP.SharedUI.Services.CommandPaletteService>();   // P8.2.2: لوحة البحث السريع Ctrl+K
builder.Services.AddMudServices();
var app = builder.Build();

// 2. إعداد الـ HTTP Pipeline بالترتيب الصحيح
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/notfound", createScopeForStatusCodePages: true);

// 🔐 م6: الطلبات العادية (العرض المسبق وواجهة /api) تحمل جلسة نطاق الطلب؛ أنشطة الدائرة يضبطها UserSessionCircuitHandler.
app.Use((httpContext, next) =>
{
    AccountModelsLib.UserSessionContext.Current = httpContext.RequestServices.GetRequiredService<AccountModelsLib.UserSession>();
    // 📱 واجهة الأجهزة ترجع حالتها كما هى (401/429...) لا صفحة /notfound بالإعادة الداخلية.
    if (httpContext.Request.Path.StartsWithSegments("/api"))
    {
        var statusPages = httpContext.Features.Get<Microsoft.AspNetCore.Diagnostics.IStatusCodePagesFeature>();
        if (statusPages != null) statusPages.Enabled = false;
    }
    return next(httpContext);
});
app.UseHttpsRedirection();
app.UseAntiforgery();
app.UseRateLimiter();
app.MapStaticAssets();

// 📱 المرحلة 7: واجهة جهاز المندوب (/api/v1) - راجع Api/SalesRepMobileApi.cs.
AccountERP.Web.Api.SalesRepMobileApi.MapSalesRepMobileApi(app);

// 🤖 المساعد الذكى P0.1: رجوع المزوّد بعد «تسجيل الدخول لدى المزوّد» (OpenRouter PKCE) - راجع AccountERP.AI/SignIn/AiSignInEndpoints.cs.
AccountERP.AI.SignIn.AiSignInEndpoints.MapAiSignInEndpoints(app).RequireRateLimiting(AccountERP.Web.Api.SalesRepMobileApi.AuthRateLimitPolicy);

// 🔥 3. جلب وتفعيل إعدادات اللغات (يجب أن تكون قبل عمل Map للـ Components)
// تأكد أن كلاس LanguageService يحتوي على تفريغ الـ Providers لتعطيل لغة المتصفح
var localizationOptions = AccountERP.SharedUI.LanguageService.GetLocalizationOptions();
app.UseRequestLocalization(localizationOptions);

// 4. عمل ربط للمكونات والصفحات
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(
        typeof(AccountERP.Web.Client._Imports).Assembly,
        typeof(AccountERP.SharedUI.SharedUIAssemblyMarker).Assembly
    );

// 5. الـ Minimal API المسؤول عن تحويل اللغة وحفظ الكوكيز
app.MapGet("/Culture/SetCulture", (string culture, string redirectUri, HttpContext httpContext, AccountERP.SharedUI.LanguageService langService) =>
{
    //if (culture != null)
    //{
    //    langService.SetCulture(culture);

    //    var cookieValue = CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture));
    //    httpContext.Response.Cookies.Append(
    //        CookieRequestCultureProvider.DefaultCookieName,
    //        cookieValue,
    //        new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
    //    );
    //}
    //return Results.LocalRedirect(redirectUri);


    if (!string.IsNullOrEmpty(culture))
    {
        langService.SetCulture(culture);

        // 🟢 تأكد من صياغة الكوكي بدقة بالغة هنا:
        var cookieValue = CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture));

        httpContext.Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName, // هذا هو الاسم القياسي (.AspNetCore.Culture)
            cookieValue,
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                Path = "/", // 🔥 حاسمة جداً لكي تقرأها كل الصفحات واللينكات بالتساوي
                // 🟢 Secure=true يمنع المتصفح من حفظ الكوكي إطلاقاً عند الوصول عبر http عادي (مثل فتح
                // التطبيق من جهاز آخر على نفس الشبكة المحلية عبر http://<IP>:5256 كما هو مطلوب دائماً
                // لهذا المشروع تحديداً) - Secure حقيقية فقط عندما يكون الطلب نفسه https بالفعل.
                Secure = httpContext.Request.IsHttps,
                SameSite = SameSiteMode.Lax
            }
        );
    }
    return Results.LocalRedirect(redirectUri);
});

// 🔐 6. مسارات استمرار جلسة الدخول عبر كوكي موقّع (Data Protection) لتنجو من إعادة تحميل كاملة للصفحة
// (مثل الضغط على عنصر قائمة جانبية لشاشة لم تُبنَ بعد بـ Blazor - راجع MODERNIZATION_NOTES.md).
// لماذا مسار API منفصل وليس ضبط الكوكي مباشرة من كود صفحة الـ Login: بحلول لحظة نجاح الدخول نحن
// بالفعل داخل دائرة (Circuit) تفاعلية حية، واستجابة الـ HTTP الأصلية انتهت وأُرسلت للمتصفح منذ زمن -
// فلا يمكن لكود مكوّن Blazor Server ضبط كوكي جديد مباشرة، تماماً كما هو الحال مع "/Culture/SetCulture" أعلاه.
app.MapPost("/Auth/Persist", (string token, HttpContext httpContext,
    AccountModelsLib.Models.LoginTokenStore tokenStore, Microsoft.AspNetCore.DataProtection.IDataProtectionProvider dp) =>
{
    // 🟢 الرمز صادر فقط بعد تحقق فعلي وناجح من كلمة السر داخل AuthService.LoginAsync (راجع Login.razor)،
    // ولمرة واحدة فقط، صالح لثوانٍ معدودة - لا حاجة لإعادة التحقق من كلمة السر هنا.
    if (!tokenStore.TryRedeem(token, out var userId))
    {
        return Results.Unauthorized();
    }

    var protector = dp.CreateProtector(AccountModelsLib.Models.AuthCookieConstants.ProtectorPurpose);
    var cookieValue = protector.Protect(userId.ToString());

    httpContext.Response.Cookies.Append(AccountModelsLib.Models.AuthCookieConstants.CookieName, cookieValue, new CookieOptions
    {
        HttpOnly = true, // غير قابل للقراءة من الـ JS فى المتصفح - يحمي من سرقته عبر XSS
        Expires = DateTimeOffset.UtcNow.AddDays(7),
        Path = "/",
        Secure = httpContext.Request.IsHttps, // نفس اعتبار كوكي اللغة أعلاه: يجب أن يعمل عبر http على الشبكة المحلية أيضاً
        SameSite = SameSiteMode.Lax
    });

    return Results.Ok();
});

app.MapPost("/Auth/Clear", (HttpContext httpContext) =>
{
    httpContext.Response.Cookies.Delete(AccountModelsLib.Models.AuthCookieConstants.CookieName, new CookieOptions { Path = "/" });
    return Results.Ok();
});

// 🎨 7. حفظ اختيار الثيم (وضع داكن/فاتح + لون أساسي) فى كوكي - نفس سبب وجود "/Culture/SetCulture" كمسار API
// منفصل بدلاً من ضبط الكوكي مباشرة من كود مكوّن Blazor Server: بحلول لحظة تغيير الثيم نحن بالفعل داخل دائرة
// (Circuit) تفاعلية حية، واستجابة الـ HTTP الأصلية انتهت منذ زمن. يُستدعى من ThemesMenu.razor عبر
// interop.js:persistThemeCookie بعد كل تغيير فعلي للثيم، بدون أي Navigation (فلا يُنهي الدائرة الحالية).
app.MapPost("/Theme/Set", (bool isDark, string? primaryColor, HttpContext httpContext) =>
{
    var color = string.IsNullOrWhiteSpace(primaryColor) ? AccountERP.SharedUI.Services.AppTheme.DefaultPrimaryColor : primaryColor;
    var cookieValue = (isDark ? "dark" : "light") + "|" + color;

    httpContext.Response.Cookies.Append(AccountERP.Web.Services.CookiePersistedThemeReader.CookieName, cookieValue, new CookieOptions
    {
        Expires = DateTimeOffset.UtcNow.AddYears(1),
        Path = "/",
        Secure = httpContext.Request.IsHttps,
        SameSite = SameSiteMode.Lax
    });

    return Results.Ok();
});

// 🛠️ 8. (للتطوير فقط) إعادة تحميل إعدادات SysConfig فى الخادم الحى بعد تغييرها مباشرة فى قاعدة البيانات، بدل إغلاق الخادم وإعادة الدخول.
// يقابل عنصر قائمة الويندوز "إعادة تحميل الإعدادات" (StripMenuItemReloadSetting → Common.SetSystemParametersDB) عبر نفس تجميع
// SystemSettingsLoader.LoadTenantSettings؛ لا يستدعى Common.SetSystemParameters() الخاصة بالويندوز لأنها تقرأ ملف ConnInfo.xml وتُعيد ضبط سلسلة الاتصال.
// مقيَّد بوضع Development وبعناوين الجهاز المحلى فقط. مثال: curl -X POST http://localhost:5256/Dev/ReloadSettings
if (app.Environment.IsDevelopment())
{
    app.MapPost("/Dev/ReloadSettings", (HttpContext httpContext) =>
    {
        var remote = httpContext.Connection.RemoteIpAddress;
        if (remote == null || !System.Net.IPAddress.IsLoopback(remote))
        {
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        AccountModelsLib.SystemSettingsLoader.LoadTenantSettings();
        return Results.Ok("settings reloaded");
    });
}

// 🔐 م6: من هنا فصاعداً كل كود يعمل داخل دائرة أو طلب له جلسته؛ قراءة Common.User_Id/Branch_Id/... بلا جلسة تُسجَّل كخطأ
// (كود خارج سياق أى مستخدم)، وكتابتها بلا جلسة تُرفض لأنها كانت ستغيّر القيمة للمستخدمين المتصلين جميعاً.
// يُفعَّل بعد التهيئة عمداً: LoadTenantSettings أعلاه وما شابهها تعمل قبل وجود أى مستخدم.
AccountModelsLib.UserSessionContext.MisuseReporter = message => app.Logger.LogError("{UserSessionMisuse}", message);
AccountModelsLib.UserSessionContext.IsMultiUserHost = true;

app.Run();