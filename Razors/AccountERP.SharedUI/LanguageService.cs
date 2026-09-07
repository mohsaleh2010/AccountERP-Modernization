

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

// 🟢 استخدام شرطي: مكتبات الويب تُستدعى فقط عندما يتم بناء مشروع الويب
#if !ANDROID && !IOS && !MACCATALYST && !WINDOWS
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
#endif

namespace AccountERP.SharedUI
{
    // 🟢 وصف كل لغة مدعومة: الكود الثقافي، اسمها بلغتها الأصلية، علمها، واتجاهها
    // إضافة لغة جديدة مستقبلاً (مثل الإيطالية أو التركية) تتم فقط بإضافة سطر هنا،
    // بدون الحاجة لتعديل أي شاشة تستخدم LanguageSwitcher أو LangService.SupportedCultures
    public record LanguageInfo(string CultureCode, string NativeName, string EnglishName, string Flag, bool IsRtl, bool IsFullyTranslated);

    public class LanguageService
    {
        // 🌍 اللغات المدعومة رسمياً في النظام. العربية والإنجليزية مترجمتان بالكامل الآن،
        // والفرنسية والألمانية مُجهزتان بنياً (Culture + Resx placeholder) تمهيداً للترجمة الكاملة لاحقاً
        public static readonly IReadOnlyList<LanguageInfo> SupportedLanguages = new List<LanguageInfo>
        {
            new("ar-EG", "العربية",  "Arabic",  "🇪🇬", IsRtl: true,  IsFullyTranslated: true),
            new("en-US", "English",  "English", "🇺🇸", IsRtl: false, IsFullyTranslated: true),
            new("fr-FR", "Français", "French",  "🇫🇷", IsRtl: false, IsFullyTranslated: false),
            new("de-DE", "Deutsch",  "German",  "🇩🇪", IsRtl: false, IsFullyTranslated: false),
        };

        // اللغة الافتراضية للنظام هي العربية (أهم لغة في صفحة البداية والشاشات الرئيسية)
        private const string DefaultCultureCode = "ar-EG";

        public LanguageService(AppSettings appSettings)
        {
            // 🟢 على الويب: RequestLocalizationMiddleware يكون قد ضبط CultureInfo.CurrentUICulture بالفعل
            // بشكل صحيح لهذا الطلب/الدائرة من الكوكي قبل هذه اللحظة - نبدأ الحقل _currentCultureCode منه
            // مباشرة بدلاً من تركه عالقاً على الافتراضي "ar-EG" (قيمة تعريف الحقل بالأعلى). بدون هذا كانت
            // نصوص Lang[...] (المعتمدة مباشرة على CultureInfo) تظهر صحيحة بلغة الكوكي من أول رندرة، بينما
            // أي قيمة مبنية على LangService.CurrentLanguage نفسها (مثل GetLocalizedDescription لأسماء
            // القوائم متعددة اللغات) تبقى عالقة على العربية حتى يستخدم المستخدم LanguageSwitcher يدوياً
            // فى نفس الدائرة - فيظهر السايد بار/الشاشة الرئيسية بلغة مختلفة عن باقي الشاشة عند أي دخول جديد
            // بلغة غير العربية. على الديسك توب/الموبايل (MAUI) لا يوجد Middleware إطلاقاً، فنفرض الافتراضي صراحة.
            if (appSettings.IsWeb)
            {
                var requestCultureCode = CultureInfo.CurrentUICulture.Name;
                var matched = SupportedLanguages.FirstOrDefault(
                    l => l.CultureCode.Equals(requestCultureCode, StringComparison.OrdinalIgnoreCase));
                _currentCultureCode = matched?.CultureCode ?? DefaultCultureCode;
            }
            else
            {
                SetCultureDefault();
            }
        }

        // 🔥 حدث هام جداً لتحديث شاشات الماوي والديسك توب فور سماع تغيير اللغة
        public event Action? OnLanguageChanged;

        // 🟢 المصدر الحقيقي الوحيد للغة الحالية لهذه الجلسة/الدائرة (Scoped) هو هذا الحقل، وليس
        // CultureInfo.CurrentCulture الطموح (Ambient/Thread-static). فى Blazor Server قد تُنفَّذ أحداث
        // ورندرات نفس الدائرة (Circuit) الواحدة على خيوط (Threads) فعلية مختلفة من الـ ThreadPool المشترك
        // بين كل المستخدمين - و CultureInfo.CurrentCulture تُقرأ من الخيط الفعلي الحالي، فقد يظهر أن تبديل
        // اللغة "لا يستجيب" (dir/RTL يبقى كما هو) لأن الرندرة التالية نفذت على خيط مختلف لم تُحدَّث ثقافته بعد.
        // بتخزين الاختيار فى حقل على نسخة الخدمة نفسها (واحدة لكل دائرة عبر AddScoped) نضمن قراءة صحيحة
        // 100% بغض النظر عن الخيط الذى ينفّذ الرندرة.
        private string _currentCultureCode = DefaultCultureCode;

        public string CurrentLanguage => _currentCultureCode;

        // 🟢 لاستخدامها مع IStringLocalizer.WithCulture(...) فى أي مكوّن يحتاج ترجمة نصوصه الخاصة بشكل
        // مضمون 100% بغض النظر عن ثقافة الخيط الفعلي الذى نُفِّذت عليه رندرته - راجع MODERNIZATION_NOTES.md
        // (حالة NavMenuMud/MenuNode/Home.razor حيث بقيت نصوص Lang[...] بلغتها القديمة بعد تبديل اللغة رغم
        // أن قيماً أخرى محسوبة من CurrentLanguage مباشرة، مثل GetLocalizedDescription، تحدّثت بشكل صحيح).
        public CultureInfo CurrentCultureInfo => new CultureInfo(_currentCultureCode);

        public LanguageInfo CurrentLanguageInfo =>
            SupportedLanguages.FirstOrDefault(l => l.CultureCode.Equals(CurrentLanguage, StringComparison.OrdinalIgnoreCase))
            ?? SupportedLanguages.First(l => l.CultureCode == DefaultCultureCode);

        // خاصية للتوافق الخلفي مع الشاشات القديمة التي تفحص العربية تحديداً
        public bool IsArabic => CurrentLanguage.StartsWith("ar", StringComparison.OrdinalIgnoreCase);

        // جعل خاصية الـ RTL مقروءة ديناميكياً من وصف اللغة الحالية لتسهيل قلب الشاشات dir="rtl"
        // (تعمل تلقائياً لأي لغة يمين-لشمال تُضاف مستقبلاً، وليست مرتبطة بالعربية فقط)
        public bool IsRtl => CurrentLanguageInfo.IsRtl;

        public string[] GetSupportedCultures() => SupportedCultures;
        public static string[] SupportedCultures => SupportedLanguages.Select(l => l.CultureCode).ToArray();

        // 🌐 دوال الـ Web يتم تجميعها فقط في مشروع الويب لتبتعد عن مشاكل الماوي
#if !ANDROID && !IOS && !MACCATALYST && !WINDOWS
        public static RequestLocalizationOptions GetLocalizationOptions()
        {
            var options = new RequestLocalizationOptions()
                .SetDefaultCulture(DefaultCultureCode)
                .AddSupportedCultures(SupportedCultures)
                .AddSupportedUICultures(SupportedCultures);

            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(DefaultCultureCode);
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(DefaultCultureCode);

            options.RequestCultureProviders.Clear();
            options.RequestCultureProviders.Add(new CookieRequestCultureProvider());
            options.RequestCultureProviders.Add(new QueryStringRequestCultureProvider());

            return options;
        }
#endif

        public string SetCultureDefault()
        {
            SetCulture(DefaultCultureCode);
            return DefaultCultureCode;
        }

        public void SetCulture(string cultureName)
        {
            if (string.IsNullOrEmpty(cultureName)) cultureName = DefaultCultureCode;

            // 🟢 مصدر الحقيقة لهذه الدائرة/الجلسة - يُقرأ من هنا فى CurrentLanguage/IsArabic/IsRtl،
            // وليس من CultureInfo.CurrentCulture الطموح (راجع التعليق أعلى الحقل).
            _currentCultureCode = cultureName;

            ApplyToCurrentThread();

            // 🔥 إطلاق التنبيه لتحديث الواجهات فوراً في الويب والديسك توب والموبايل (يقلب dir ويعيد رسم كل المكونات المشتركة)
            OnLanguageChanged?.Invoke();
        }

        // 🟢 تُستدعى مجدداً قبل كل رندرة (راجع MainLayoutMud.OnParametersSet) لضبط CultureInfo.CurrentCulture/
        // CurrentUICulture الخاصين بالخيط الفعلي الذى ستُنفَّذ عليه هذه الرندرة تحديداً - وهو ما يحتاجه
        // IStringLocalizer (خاصية Lang[...]) ليختار ملف الـ .resx الصحيح، لأنه يقرأ الثقافة من الخيط الحالي
        // وقت الاستدعاء، وليس من حقل _currentCultureCode مباشرة.
        // 🐛 CurrentCulture (بعكس CurrentUICulture) لم يعد يُضبَط على ثقافة الواجهة (مثل ar-EG) هنا عمداً منذ
        // اكتشاف خطأ حقيقي بسببه: عمليات DAL فى AccountModelsLib تبني SQL خام بتجميع نصوص (raw string
        // concatenation) لأرقام (مثال: "Where Id=" + SubAccount_ID) عبر Task.Run - وTask.Run يرث CurrentCulture
        // (وليس فقط CurrentUICulture) من الخيط المستدعي وقتها. تنسيق الأرقام السالبة بثقافة ar-EG فى .NET يُدرج
        // علامة اتجاه Unicode غير مرئية (Arabic Letter Mark، U+061C) حول علامة السالب - فيتحول "-1" فعلياً إلى
        // "‎-1" (بحرف غير مرئي إضافي) عند إدراجه فى نص SQL، فيفشل أي WHERE Id=-1 بصمت (يُطابق صفراً من الصفوف
        // أو تحدث استثناءات تُبتلَع فى catch عام). أول ظهور فعلي: AccountModelsLib.Sub_Account.
        // Check_SubAccount_ID_Found يعيد "الحساب موجود من قبل" خطأً لكل محاولة إضافة حساب جديد من شجرة
        // الحسابات (Submain_Account_Id=-1 دائماً لحساب جديد) - راجع MODERNIZATION_NOTES.md لتفاصيل التشخيص
        // الكامل. CurrentUICulture يبقى مضبوطاً كالمعتاد (هو وحده ما يحتاجه IStringLocalizer/Lang[...] لاختيار
        // ملف .resx الصحيح) - فقط CurrentCulture/DefaultThreadCurrentCulture (المسؤولان عن تنسيق الأرقام/
        // التواريخ) يبقيان على الثقافة الغازية (Invariant) بغض النظر عن لغة الواجهة المختارة.
        public void ApplyToCurrentThread()
        {
            var culture = new CultureInfo(_currentCultureCode);
            CultureInfo.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            // 🐛 لا يكفي "عدم ضبط" CurrentCulture هنا - شيء آخر (على الأرجح UseRequestLocalization فى
            // Program.cs، المُطبَّق وقت طلب HTTP الذى يُنشئ دائرة Blazor Server نفسها) يضبطها فعلياً = ar-EG
            // وتبقى (تتدفّق عبر ExecutionContext إلى أي Task.Run لاحق طوال عمر الدائرة كلها) - تأكَّدنا من هذا
            // بالتتبع الفعلي (raw SQL يحمل "؜-1" بدل "-1" حتى بعد إزالة الضبط من هنا). الإصلاح الفعلي: ضبطها
            // بقوة لكل من CurrentCulture وDefaultThreadCurrentCulture = InvariantCulture صراحة فى كل مرة، بدل
            // مجرد الامتناع عن تغييرها - راجع MODERNIZATION_NOTES.md.
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        }
    }
}
