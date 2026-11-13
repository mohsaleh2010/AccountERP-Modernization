using AccountERP.AI.Providers;
using AccountERP.AI.Security;
using AccountModelsLib;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AccountERP.AI
{
    public static class AiServiceCollectionExtensions
    {
        /// <summary>
        /// تسجيل خدمات المساعد الذكى (P0). DataProtection:KeysPath (اختيارى فى appsettings) يثبّت مسار مفاتيح التشفير على الخادم المنشور
        /// حتى تبقى مفاتيح المزوّدين قابلة للفك بعد إعادة النشر؛ بدونه يبقى المسار الافتراضى كما هو اليوم (لا يتأثر كوكى الدخول الحالى).
        /// </summary>
        public static IServiceCollection AddAccountErpAi(this IServiceCollection services, IConfiguration config)
        {
            string? keysPath = config["DataProtection:KeysPath"];
            if (!string.IsNullOrWhiteSpace(keysPath))
                services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(keysPath));

            services.AddSingleton<AiKeyProtector>();
            services.AddSingleton<ProviderRegistry>();
            services.AddSingleton<SignIn.AiSignInStore>();
            services.AddSingleton<SignIn.AiSignInService>();
            services.AddScoped<IAiAdminGateway, AiAdminGateway>();
            services.AddScoped<IAiChatGateway, Chat.AiChatGateway>();   // P1: المحادثة
            return services;
        }
    }
}
