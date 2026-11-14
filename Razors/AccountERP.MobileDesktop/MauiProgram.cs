using AccountERP.SharedUI;
using AccountModelsLib;
using AccountModelsLib.Models;
#if ANDROID
using Android.Telephony.Data;
#endif
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Storage;
using MudBlazor.Services;
using System;
using System.Linq;
using System.Net.Http;
namespace AccountERP.MobileDesktop
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            // 1. تفعيل الـ Blazor WebView الأساسي لمشروع الماوي
            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            // تفعيل أدوات المطورين لرؤية الأخطاء والـ CSS داخل الديسك توب
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            // 2. تسجيل الـ HttpClient الافتراضي للديسك توب
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7199/") });

            // 3. تسجيل خدمات الترجمة واللغة
            // 🔥 ResourcesPath="Resources" ضروري هنا بالضبط كما فى AccountERP.Web\Program.cs: ملفات
            // SharedUIAssemblyMarker.*.resx موجودة فعلياً تحت مجلد Resources\ فى مشروع SharedUI، فاسم
            // المورد المُضمَّن (Manifest Resource Name) الفعلي يتضمن جزء "Resources." (مثال:
            // AccountERP.SharedUI.Resources.SharedUIAssemblyMarker). بدون تحديد هذا المسار هنا، يبحث
            // IStringLocalizer<SharedUIAssemblyMarker> عن اسم مختلف تماماً (بدون "Resources.") فلا يجده فى
            // أي لغة إطلاقاً - رغم وجود ملفات الـ satellite assemblies الصحيحة فعلياً فى مجلدات الإخراج
            // (ar\, en\, fr\, de\) - فيعيد اسم المفتاح نفسه كنص افتراضي لكل شاشة وكل لغة (مثل "LoginTitle"،
            // "SystemError" التى ظهرت فعلياً فى شاشة الدخول). راجع MODERNIZATION_NOTES.md.
            builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
            builder.Services.AddScoped<LanguageService>();

            // 4. 🔴 تسجيل الـ Repository الأساسي أولاً (تأكد من تسجيل سياق قاعدة البيانات DBContext الخاص به إذا لزم الأمر)
            builder.Services.AddScoped<MenuRepository>();

            // 5. 🔐 MenuStateService تُنشأ فارغة دائماً هنا أيضاً (نفس مبدأ مشروع الويب): لا تحميل لأي قوائم
            //    قبل نجاح تسجيل دخول حقيقي عبر AuthService.LoginAsync، الذي يضبط IsAuthenticated بنفسه.
            builder.Services.AddScoped<MenuStateService>();

            // 🗂️ مساحة عمل التبويبات (بديل نوافذ MDI فى الويندوز فورمز) - نفس تسجيل AccountERP.Web\Program.cs
            // بالضبط (راجع "mirror-web-changes-to-mobiledesktop" فى قواعد الجلسة الثابتة).
            builder.Services.AddScoped<AccountERP.SharedUI.Services.TabWorkspaceService>();

            // ⚙️ نفس تسجيل AccountERP.Web\Program.cs بالضبط (راجع "mirror-web-changes-to-mobiledesktop").
            builder.Services.AddScoped<AccountERP.SharedUI.Services.UserPreferencesService>();
            // 🔎 P8.2.2: لوحة البحث السريع Ctrl+K (مجموعة الشاشات فقط على MAUI - بلا بوابة بحث البيانات ولا المساعد).
            builder.Services.AddScoped<AccountERP.SharedUI.Services.CommandPaletteService>();

            // 1. نص الاتصال يُقرأ الآن من appsettings.json مُضمَّن كملف MauiAsset (Resources\Raw\appsettings.json)
            // - نفس بنية appsettings.json فى AccountERP.Web بالضبط (ConnectionStrings:DefaultConnection) -
            // بدلاً من نص مكتوب مباشرة فى الكود هنا وآخر مختلف تماماً فى AccountERP.Web\appsettings.json (كان
            // هذا سبب خطأ "FOREIGN KEY... may cause cycles" عند تسجيل الدخول من الديسك توب: كان يشير لقاعدة
            // بيانات فارغة تماماً "Connection_DataElEmanNew" فحاول My_DbContext.EnsureCreated() إنشاء كل
            // الجداول من الصفر فصطدم بتعارض Cascade حقيقى فى الموديل، بينما AccountERP.Web يشير لقاعدة
            // بيانات "ClientNew" الحقيقية التى تحتوي على المخطط والبيانات فعلياً فلا يحدث فيها هذا الاستدعاء
            // أصلاً). لتغيير قاعدة البيانات المستهدفة لهذا المشروع مستقبلاً، عدّل فقط
            // Resources\Raw\appsettings.json (يحتاج إعادة بناء لأنه Asset مُضمَّن فى حزمة التطبيق، بعكس
            // appsettings.json فى الويب الذى يُقرأ من القرص مباشرة وقت التشغيل) - راجع MODERNIZATION_NOTES.md.
            using var configStream = FileSystem.Current.OpenAppPackageFileAsync("appsettings.json").GetAwaiter().GetResult();
            var appConfig = new ConfigurationBuilder().AddJsonStream(configStream).Build();
            string connectionString = appConfig.GetConnectionString("DefaultConnection") ?? string.Empty;

            // 🔥 نفس الإصلاح المطبَّق فى AccountERP.Web\Program.cs بالضبط - وهو ضروري هنا أيضاً، وليس مجرد
            // اتساق: كل كلاسات *CLS المشتركة (SupplierCLS، GroupsCLS، ...إلخ) تُنشئ new operations()/
            // new operations(true) بنفسها داخلياً (وليس عبر الحقن أعلاه)، وoperations.MyConn() تلجأ لقيمة
            // ReturnConnStr() المكتوبة يدوياً (قاعدة بيانات مختلفة تماماً) كلما كان operations.conn_str_
            // الثابت (Static) فارغاً - وهو فارغ دائماً هنا لأن هذا الملف لم يكن يضبطه إطلاقاً من قبل. بدون
            // هذا السطر، كل منطق العمل المشترك (غير المُحقَن مباشرة عبر DI) كان يتصل فعلياً بقاعدة بيانات
            // خاطئة على الديسك توب/الأندرويد، رغم أن الاتصال المُحقَن عبر DI أدناه صحيح. راجع
            // MODERNIZATION_NOTES.md ("AccountERP.MobileDesktop parity").
            AccountModelsLib.operations.conn_str_ = connectionString;
            AccountModelsLib.Common.IS_ReadConStrFromXMLInfo = 1;
            try { AccountModelsLib.SystemSettingsLoader.LoadTenantSettings(); } catch { }

            // 2. تسجيل الـ DbContext والـ operations بنفس كود الويب تمامًا
            builder.Services.AddScoped<My_DbContext>(provider => new My_DbContext(connectionString));

            builder.Services.AddScoped<operations>(provider =>
            {
                var oppState = new operations(connectionString);
                if (oppState.ConectionStaus == false)
                {
                    // هنا يمكنك معالجة فشل الاتصال بالسيرفر في الديسك توب
                }
                return oppState;
            });

            // 3. 🔥 المفاجأة: هنا سنحقن الـ WebAuthService نفسها!
            // لأنها تحتوي على كود الـ EF الحقيقي الذي يقرأ من الداتا بيز مباشرة، وبما أننا سجلنا الـ DbContext فوق، ستعمل فورًا!
            builder.Services.AddScoped<IAuthService, WebAuthService>();
            // داخل كود الـ CreateMauiApp / أو الـ Web Program.cs
            builder.Services.AddSingleton(new AppSettings { IsWinForms = false, IsLoadMainLayout = false, IsWeb = false });

            // 6. 🔥 تسجيل خدمة المصادقة الخاصة بالديسك توب والموبايل (تمهيداً للـ API)
            // هنا نحقن كلاس الـ DesktopAuthService الذي سيكلم الـ API في خطوتنا القادمة
            //builder.Services.AddScoped<IAuthService, DesktopAuthService>();
            // خدمة الثيم (فاتح/داكن) - نفس الخدمة المستخدمة في مشروع الويب
            builder.Services.AddScoped<AccountERP.SharedUI.Services.ThemeService>();
            // سجلّ الإشعارات (جرس الإشعارات فى الشريط العلوي) - نفس الخدمة المستخدمة في مشروع الويب
            builder.Services.AddScoped<AccountERP.SharedUI.Services.NotificationCenterService>();
            builder.Services.AddMudServices();
            return builder.Build();
        }
    }
}