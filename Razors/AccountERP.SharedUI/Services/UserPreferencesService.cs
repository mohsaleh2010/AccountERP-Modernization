using System;
using AccountModelsLib;

namespace AccountERP.SharedUI.Services
{
    // 🟢 تفضيلات واجهة خاصة بكل مستخدم (إظهار/إخفاء مسار التنقّل، تفعيل/إلغاء نظام التبويبات...)، محفوظة
    // فى جدول USER_DETAILES_Setting الجديد (راجع UserSettingsCLS.cs فى AccountModelsLib) بدل كوكي/
    // localStorage - بطلب المستخدم صراحة أن تُحفَظ لحسابه فى قاعدة البيانات، لا لهذا المتصفح/الجهاز فقط.
    // Scoped: نسخة مستقلة لكل مستخدم/دائرة، تُحمَّل بعد نجاح الدخول وتُصفَّر عند الخروج - راجع MainLayoutMud.razor
    // (تشترك فى نفس حدث MenuStateService.OnAuthChanged الذى يُحمِّل/يُفرِّغ القوائم بالضبط، بدل أي تعديل على
    // AuthService.cs نفسه فى AccountModelsLib).
    public class UserPreferencesService
    {
        private readonly UserSettingsCLS _cls = new();

        // 🟢 نفس نمط dd/MM/yyyy الآن هو المعيار القياسى لعرض التواريخ فى شاشات Blazor الجديدة/المُعدَّلة
        // (راجع "📐 معيار قياسى - تنسيق التواريخ" فى MODERNIZATION_NOTES.md) - كل شاشة تحتاج تنسيق تاريخ
        // تقرأه من هنا بدل نص تنسيق ثابت مكتوب مباشرة فى كودها.
        private const string DefaultDateFormat = "dd/MM/yyyy";

        public bool IsLoaded { get; private set; }
        public bool ShowBreadcrumb { get; private set; } = true;
        public bool ShowTabs { get; private set; } = true;
        public string DateFormat { get; private set; } = DefaultDateFormat;

        // 🟢 null = لا يوجد تفضيل محفوظ بعد لهذا المستخدم (أول دخول، أو لم يُغيِّر اللغة يدوياً من قبل) -
        // القارئ (MainLayoutMud.OnAuthChanged/OnInitializedAsync) لا يستدعى LanguageService.SetCulture إلا
        // لو كانت هذه القيمة غير فارغة، فيبقى الافتراضى الحالى (كوكي الويب أو افتراضى اللغة العربية على
        // الديسك توب/الموبايل) سارياً بدون أى تغيير غير مقصود.
        public string? Language { get; private set; }

        public event Action? OnChanged;

        public void Load()
        {
            try
            {
                var settings = _cls.GetUserSettings(Common.User_Id);
                ShowBreadcrumb = !settings.TryGetValue(UserSettingKeys.ShowBreadcrumb, out var showBreadcrumb) || showBreadcrumb != "false";
                ShowTabs = !settings.TryGetValue(UserSettingKeys.ShowTabs, out var showTabs) || showTabs != "false";
                DateFormat = settings.TryGetValue(UserSettingKeys.DateFormat, out var dateFormat) && !string.IsNullOrWhiteSpace(dateFormat)
                    ? dateFormat
                    : DefaultDateFormat;
                Language = settings.TryGetValue(UserSettingKeys.Language, out var language) && !string.IsNullOrWhiteSpace(language)
                    ? language
                    : null;
            }
            catch
            {
                // غير حرج: تبقى القيم الافتراضية (الإظهار مفعّل للجميع) سارية لهذه الجلسة حتى لو تعذّرت القراءة
                ShowBreadcrumb = true;
                ShowTabs = true;
                DateFormat = DefaultDateFormat;
                Language = null;
            }

            IsLoaded = true;
            OnChanged?.Invoke();
        }

        // تُستدعى عند تسجيل الخروج - لا تُحفَظ للقرص، فقط تعيد الحالة الافتراضية فى الذاكرة حتى لا يرث
        // دخول تالٍ على نفس الدائرة (نادر، لكن ممكن على الديسك توب/الموبايل) تفضيلات مستخدم سابق
        public void ResetToDefaults()
        {
            IsLoaded = false;
            ShowBreadcrumb = true;
            ShowTabs = true;
            DateFormat = DefaultDateFormat;
            Language = null;
            OnChanged?.Invoke();
        }

        public void SetShowBreadcrumb(bool value)
        {
            if (ShowBreadcrumb == value) return;
            ShowBreadcrumb = value;
            TrySave(UserSettingKeys.ShowBreadcrumb, value);
            OnChanged?.Invoke();
        }

        public void SetShowTabs(bool value)
        {
            if (ShowTabs == value) return;
            ShowTabs = value;
            TrySave(UserSettingKeys.ShowTabs, value);
            OnChanged?.Invoke();
        }

        public void SetDateFormat(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || DateFormat == value) return;
            DateFormat = value;
            TrySave(UserSettingKeys.DateFormat, value);
            OnChanged?.Invoke();
        }

        // 🟢 تُستدعى من LanguageSwitcher.ChangeLanguage عند تبديل اللغة يدوياً (فقط لو كان المستخدم مسجِّلاً
        // دخوله بالفعل - راجع الفحص هناك) - راجع MainLayoutMud.razor لتطبيق القيمة المحفوظة تلقائياً عند
        // نجاح الدخول لاحقاً على أى جهاز/متصفح آخر.
        public void SetLanguage(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || string.Equals(Language, value, StringComparison.OrdinalIgnoreCase)) return;
            Language = value;
            TrySave(UserSettingKeys.Language, value);
            OnChanged?.Invoke();
        }

        private void TrySave(string key, bool value) => TrySave(key, value ? "true" : "false");

        private void TrySave(string key, string value)
        {
            try
            {
                _cls.SaveUserSetting(Common.User_Id, key, value);
            }
            catch
            {
                // غير حرج: يبقى التبديل مُطبَّقاً فعلياً لهذه الجلسة حتى لو فشل الحفظ فى قاعدة البيانات
            }
        }
    }
}
