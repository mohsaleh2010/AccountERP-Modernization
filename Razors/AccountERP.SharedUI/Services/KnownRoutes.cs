using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Components;

namespace AccountERP.SharedUI.Services
{
    // 🟢 يحل مشكلة "أي كليك على القائمة يعيد تحميلها من قاعدة البيانات": أغلب قيم KeysData الحالية
    // في SysConfigMenus هي مفاتيح شاشات الويندوز فورمز القديمة (مثل "menuAccountsTree")، وليست
    // مسارات Blazor حقيقية بعد - راجع MODERNIZATION_NOTES.md. الضغط على مثل هذا الرابط كـ Href عادي
    // يفشل فى المطابقة الداخلية فيتحول Blazor لتنقّل كامل (Full/Hard Navigation)، وهو ما يفتح دائرة
    // (Circuit) جديدة من الصفر فتُعاد قراءة كل القوائم من قاعدة البيانات مجدداً عبر استعادة الجلسة من
    // الكوكي - رغم أن القوائم نفسها لم تتغيّر إطلاقاً. الحل هنا: نتحقق مسبقاً هل KeysData يطابق فعلاً
    // مساراً حقيقياً مُسجَّلاً (@page) قبل استخدامه كـ Href؛ فقط الروابط الحقيقية تتنقّل، والباقي يُعرض
    // كـ "الشاشة قيد الإنشاء" بدون أي تنقّل أو أي اتصال بقاعدة البيانات.
    public static class KnownRoutes
    {
        private static readonly Lazy<HashSet<string>> _routes = new(BuildRouteSet);

        // 🟢 مبني بالـ Reflection وليس مُدرَجاً يدوياً: أي شاشة جديدة تُضاف بـ @page "/..." تصبح معروفة
        // تلقائياً بدون أي تعديل هنا أو فى القائمة الجانبية نفسها - يطابق وتيرة "شاشة واحدة كل مرة" لمشروع التطوير.
        private static HashSet<string> BuildRouteSet()
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            void ScanAssembly(Assembly assembly)
            {
                foreach (var type in assembly.GetTypes())
                {
                    foreach (var route in type.GetCustomAttributes<RouteAttribute>(inherit: true))
                    {
                        if (!string.IsNullOrWhiteSpace(route.Template))
                        {
                            set.Add(route.Template.TrimEnd('/'));
                        }
                    }
                }
            }

            try
            {
                ScanAssembly(typeof(SharedUIAssemblyMarker).Assembly);
            }
            catch
            {
                // غير حرج: لو فشل الفحص لأي سبب، تبقى القائمة فارغة فيُعامل كل رابط كـ"قيد الإنشاء" بأمان
            }

            return set;
        }

        // KeysData يجب أن تبدأ بـ "/" وتطابق مساراً حقيقياً مسجَّلاً بالضبط لتُعتبر شاشة قائمة فعلياً
        public static bool IsImplemented(string? keysData)
        {
            if (string.IsNullOrWhiteSpace(keysData) || !keysData.StartsWith("/"))
            {
                return false;
            }

            return _routes.Value.Contains(keysData.TrimEnd('/'));
        }
    }
}
