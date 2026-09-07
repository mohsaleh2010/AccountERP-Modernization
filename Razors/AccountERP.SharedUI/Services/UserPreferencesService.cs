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

        public bool IsLoaded { get; private set; }
        public bool ShowBreadcrumb { get; private set; } = true;
        public bool ShowTabs { get; private set; } = true;

        public event Action? OnChanged;

        public void Load()
        {
            try
            {
                var settings = _cls.GetUserSettings(Common.User_Id);
                ShowBreadcrumb = !settings.TryGetValue(UserSettingKeys.ShowBreadcrumb, out var showBreadcrumb) || showBreadcrumb != "false";
                ShowTabs = !settings.TryGetValue(UserSettingKeys.ShowTabs, out var showTabs) || showTabs != "false";
            }
            catch
            {
                // غير حرج: تبقى القيم الافتراضية (الإظهار مفعّل للجميع) سارية لهذه الجلسة حتى لو تعذّرت القراءة
                ShowBreadcrumb = true;
                ShowTabs = true;
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

        private void TrySave(string key, bool value)
        {
            try
            {
                _cls.SaveUserSetting(Common.User_Id, key, value ? "true" : "false");
            }
            catch
            {
                // غير حرج: يبقى التبديل مُطبَّقاً فعلياً لهذه الجلسة حتى لو فشل الحفظ فى قاعدة البيانات
            }
        }
    }
}
