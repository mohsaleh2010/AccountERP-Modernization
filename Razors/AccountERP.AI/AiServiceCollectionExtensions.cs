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
            services.AddSingleton(sp => new Tools.ReadOnlySqlExecutor(config));   // P2: مسار القراءة الوحيد
            services.AddSingleton<Tools.AiDatasetCache>();   // P3: بيانات التقارير على الخادم
            services.AddSingleton<Files.AiAttachmentStore>();   // P6: مرفقات المساعد (AI:AttachmentsPath اختيارى)
            services.AddSingleton<Files.AiDocumentExtractor>();   // P6: extract_document بنموذج الرؤية
            services.AddSingleton<Knowledge.AiEmbeddingService>();   // P7.2: نموذج التضمين (مهمة «تضمين») + قاطع
            services.AddSingleton<Knowledge.AiKnowledgeIndex>();   // P7.2: متجهات المعرفة فى الذاكرة
            services.AddSingleton(sp => new Knowledge.AiKnowledgeService(null, sp.GetRequiredService<Knowledge.AiEmbeddingService>(), sp.GetRequiredService<Knowledge.AiKnowledgeIndex>()));   // P7: معرفة المساعد من AI_Knowledge
            services.AddSingleton<Tools.AiToolRegistry>();
            services.AddScoped<IAiChatGateway, Chat.AiChatGateway>();   // P1: المحادثة، P2: مع أدوات القراءة
            services.AddSingleton<Eval.AiEvalRunner>();   // P7.4: تشغيل التقييم فى الخلفية (واحد فى المرة)
            services.AddScoped<IAiEvalGateway, Eval.AiEvalGateway>();   // P7.4: محرك التقييم (تبويب «التقييم» P7.5)
            services.AddScoped<IAiFeedbackGateway, Feedback.AiFeedbackGateway>();   // P7.6: مراجعة ملاحظات 👍/👎 (تبويب «الملاحظات»)
            // P8.4.2 (ش28): مهمة الشذوذ تُسجَّل قبل الموجز فيمسح الخادم المطفأ ليلاً عند الإقلاع قبل موجز نفس النبضة.
            services.AddSingleton(sp => new Anomalies.AiAnomalyJob(sp.GetRequiredService<Tools.ReadOnlySqlExecutor>()));
            services.AddSingleton<Scheduling.IAiScheduledJob>(sp => sp.GetRequiredService<Anomalies.AiAnomalyJob>());
            services.AddScoped<IAiAnomalyGateway>(sp => new Anomalies.AiAnomalyGateway(sp.GetRequiredService<Anomalies.AiAnomalyJob>(), sp.GetRequiredService<ProviderRegistry>()));   // P8.4.3: صفحة الشذوذ وتبويب الإعدادات
            services.AddSingleton(sp => new Brief.AiBriefJob(sp.GetRequiredService<Tools.ReadOnlySqlExecutor>(), sp.GetRequiredService<ProviderRegistry>()));   // P8.1.4: مشغّل الموجز اليومى
            services.AddSingleton<Scheduling.IAiScheduledJob>(sp => sp.GetRequiredService<Brief.AiBriefJob>());   // ق17: مهام AiSchedulerWorker
            services.AddScoped<IAiBriefGateway>(sp => new Brief.AiBriefGateway(sp.GetRequiredService<Brief.AiBriefJob>()));   // P8.1.4: صفحة الموجز والإعدادات (P8.1.5/P8.1.6)
            services.AddScoped<IAiSearchGateway>(sp => new Search.AiSearchGateway(sp.GetRequiredService<Tools.ReadOnlySqlExecutor>()));   // P8.2.3: بحث البيانات فى لوحة Ctrl+K (بلا نموذج)
            services.AddScoped<IAiEInvoiceGateway>(sp => new EInvoice.AiEInvoiceGateway(sp.GetRequiredService<Tools.ReadOnlySqlExecutor>(), sp.GetRequiredService<Knowledge.AiKnowledgeService>()));   // P8.3: شرح خطأ الفاتورة الإلكترونية (بلا نموذج)
            return services;
        }
    }
}
