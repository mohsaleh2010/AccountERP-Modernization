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
        // 🟢 كل مسار مُسجَّل مُقسَّم مسبقاً لأجزائه (Segments) بدل نص واحد - يسمح بمطابقة مسارات ذات
        // باراميتر حقيقي (مثل "/accounts/customer-account-statement/{Mode}") مقابل القيمة الفعلية
        // المحفوظة فى RazorPath لكل عنصر قائمة (مثل "/accounts/customer-account-statement/normal") بدل
        // اشتراط تطابق حرفي كامل كان يفشل دائماً لأي شاشة تخدم أكثر من عنصر قائمة بفارق باراميتر فقط
        // (كشف حساب عميل عادي/بفترة، إشعار تسوية عام/بالصنف بمُعرِّف "جديد"=0...) - راجع
        // menu-main-account-razor-plan.md لأمثلة حقيقية اصطدمت بهذا القيد.
        private static readonly Lazy<string[][]> _routeSegments = new(BuildRouteSegmentSet);

        // 🟢 مبني بالـ Reflection وليس مُدرَجاً يدوياً: أي شاشة جديدة تُضاف بـ @page "/..." تصبح معروفة
        // تلقائياً بدون أي تعديل هنا أو فى القائمة الجانبية نفسها - يطابق وتيرة "شاشة واحدة كل مرة" لمشروع التطوير.
        private static string[][] BuildRouteSegmentSet()
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

            return set.Select(SplitSegments).ToArray();
        }

        private static string[] SplitSegments(string route) =>
            route.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);

        // segment مطابق حرفياً، أو segment القالب هو باراميتر "{...}" (بأي قيد نوع مثل "{Id:long}") يقبل أي قيمة فعلية
        private static bool SegmentMatches(string templateSegment, string actualSegment) =>
            templateSegment.StartsWith("{", StringComparison.Ordinal)
                ? true
                : string.Equals(templateSegment, actualSegment, StringComparison.OrdinalIgnoreCase);

        // KeysData يجب أن تبدأ بـ "/" وتطابق مساراً حقيقياً مسجَّلاً (حرفياً أو عبر باراميتر مسار حقيقي) لتُعتبر شاشة قائمة فعلياً
        public static bool IsImplemented(string? keysData)
        {
            if (string.IsNullOrWhiteSpace(keysData) || !keysData.StartsWith("/"))
            {
                return false;
            }

            var actualSegments = SplitSegments(keysData);

            foreach (var templateSegments in _routeSegments.Value)
            {
                if (templateSegments.Length != actualSegments.Length) continue;

                var allMatch = true;
                for (var i = 0; i < templateSegments.Length; i++)
                {
                    if (!SegmentMatches(templateSegments[i], actualSegments[i]))
                    {
                        allMatch = false;
                        break;
                    }
                }

                if (allMatch) return true;
            }

            return false;
        }
    }
}
