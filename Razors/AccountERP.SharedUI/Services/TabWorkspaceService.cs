using System;
using System.Collections.Generic;
using System.Linq;

namespace AccountERP.SharedUI.Services
{
    // 🟢 تبويب واحد مفتوح فى مساحة العمل: يحمل نوع الصفحة الفعلي (لرسمه عبر <DynamicComponent> فى
    // TabHost.razor) وقيم باراميترات مساره (مثل CustomerId) بدل الاعتماد على <RouteView> القياسي - راجع
    // RoutePageRegistry.cs لسبب ذلك.
    public class OpenTab
    {
        public string Key { get; init; } = string.Empty; // المسار الكامل المُطبَّع، مثال: "/customers/info/5"
        public string Title { get; set; } = string.Empty;
        // 🟢 عنوان تضبطه الصفحة نفسها بعد تحميل بياناتها (مثل "فاتورة مبيعات رقم 1539")؛ له الأولوية على عنوان القائمة/الرابط
        // ويبقى عند إعادة تفعيل التبويب (OpenOrActivate لا يستبدله).
        public string? CustomTitle { get; set; }
        public string Icon { get; set; } = string.Empty;
        public Type PageType { get; init; } = null!;
        public Dictionary<string, object> Parameters { get; init; } = new();
    }

    // 🟢 يطابق تماماً فكرة "نوافذ MDI" فى الويندوز فورمز التى يستبدلها هذا المشروع: كل شاشة تُفتح من القائمة
    // تبقى تبويباً حياً بمحتواه وحالته (بيانات غير محفوظة بعد، تبويب فرعي مفتوح...) حتى يُغلَق صراحة، بدل أن
    // يُعاد بناؤه من الصفر عند كل تبديل بين الشاشات المفتوحة - راجع TabHost.razor (KeepPanelsAlive) وMainLayoutMud.razor.
    // Scoped: مساحة عمل مستقلة لكل مستخدم/دائرة (Circuit)، تُصفَّر عند تسجيل الخروج (راجع MainLayoutMud.LogoutClick).
    public class TabWorkspaceService
    {
        // 🟢 حد أقصى للتبويبات المفتوحة معاً: كل تبويب مفتوح يبقى مكوّناً حياً بالكامل فى ذاكرة الخادم طوال
        // مدة الدائرة (Blazor Server) - بلا حد أقصى يمكن أن ينمو استهلاك الذاكرة بلا حدود لمستخدم يفتح عشرات
        // الشاشات ولا يغلق أياً منها. عند الوصول للحد، يُغلَق أقدم تبويب غير نشط تلقائياً (وليس التبويب الحالي).
        private const int MaxOpenTabs = 12;

        public List<OpenTab> Tabs { get; } = new();
        public string? ActiveKey { get; private set; }

        public event Action? OnChanged;

        public OpenTab OpenOrActivate(string key, Type pageType, Dictionary<string, object> parameters, string title, string icon)
        {
            var existing = Tabs.FirstOrDefault(t => t.Key == key);
            if (existing != null)
            {
                // 🟢 تحديث العنوان/الأيقونة عند كل تفعيل (وليس فقط أول فتح): يلتقط تبديل لغة الواجهة لاحقاً
                existing.Title = existing.CustomTitle ?? title;
                existing.Icon = icon;
                ActiveKey = key;
                OnChanged?.Invoke();
                return existing;
            }

            if (Tabs.Count >= MaxOpenTabs)
            {
                var oldest = Tabs.FirstOrDefault(t => t.Key != ActiveKey);
                if (oldest != null) Tabs.Remove(oldest);
            }

            var tab = new OpenTab
            {
                Key = key,
                Title = title,
                Icon = icon,
                PageType = pageType,
                Parameters = parameters
            };

            Tabs.Add(tab);
            ActiveKey = key;
            OnChanged?.Invoke();
            return tab;
        }

        // 🟢 تضبطه الصفحة (بعد تحميل الفاتورة مثلاً) ليظهر رقم الفاتورة الفعلى فى عنوان التبويب بدل معرّف السجل.
        public void SetCustomTitle(string key, string title)
        {
            var tab = Tabs.FirstOrDefault(t => t.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
            if (tab == null) return;
            tab.CustomTitle = title;
            tab.Title = title;
            OnChanged?.Invoke();
        }

        public void Activate(string key)
        {
            if (Tabs.Any(t => t.Key == key) && ActiveKey != key)
            {
                ActiveKey = key;
                OnChanged?.Invoke();
            }
        }

        // يعيد مفتاح التبويب الذى يجب تفعيله بعد الإغلاق (الأقرب فى الموضع نفسه، أو null لو لم يتبقَّ شيء)
        public string? Close(string key)
        {
            var index = Tabs.FindIndex(t => t.Key == key);
            if (index < 0) return ActiveKey;

            Tabs.RemoveAt(index);

            if (ActiveKey == key)
            {
                ActiveKey = Tabs.Count == 0 ? null : Tabs[Math.Min(index, Tabs.Count - 1)].Key;
            }

            OnChanged?.Invoke();
            return ActiveKey;
        }

        public string? CloseOthers(string key)
        {
            Tabs.RemoveAll(t => t.Key != key);
            ActiveKey = Tabs.Count > 0 ? key : null;
            OnChanged?.Invoke();
            return ActiveKey;
        }

        public string? CloseAll()
        {
            Tabs.Clear();
            ActiveKey = null;
            OnChanged?.Invoke();
            return null;
        }

        // تُستدعى عند تسجيل الخروج فقط - راجع MainLayoutMud.LogoutClick
        public void Clear()
        {
            Tabs.Clear();
            ActiveKey = null;
            OnChanged?.Invoke();
        }
    }
}
