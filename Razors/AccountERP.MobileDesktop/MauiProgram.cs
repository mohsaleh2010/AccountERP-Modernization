using AccountERP.SharedUI;
using AccountModelsLib;
using AccountModelsLib.Models;
#if ANDROID
using Android.Telephony.Data;
#endif
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
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
            builder.Services.AddLocalization();
            builder.Services.AddScoped<LanguageService>();

            // 4. 🔴 تسجيل الـ Repository الأساسي أولاً (تأكد من تسجيل سياق قاعدة البيانات DBContext الخاص به إذا لزم الأمر)
            builder.Services.AddScoped<MenuRepository>();

            // 5. 🔐 MenuStateService تُنشأ فارغة دائماً هنا أيضاً (نفس مبدأ مشروع الويب): لا تحميل لأي قوائم
            //    قبل نجاح تسجيل دخول حقيقي عبر AuthService.LoginAsync، الذي يضبط IsAuthenticated بنفسه.
            builder.Services.AddScoped<MenuStateService>();

            // 1. تزويد نص الاتصال يدويًا (لأن جهاز العميل سيكلم السيرفر مباشرة عبر الشبكة الداخلية للشركة)
            string connectionString = "Server=***;Database=***;User Id=***;Password=;TrustServerCertificate=True;MultipleActiveResultSets=true;";

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