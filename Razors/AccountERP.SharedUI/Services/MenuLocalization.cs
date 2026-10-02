using System;
using System.Collections.Generic;
using System.Linq;
using AccountModelsLib;

namespace AccountERP.SharedUI.Services
{
    // 🟢 "Description_Language" كما وصفه المستخدم ليس عموداً فعلياً فى قاعدة البيانات، بل قيمة تُحسَب هنا
    // وقت العرض: تختار تلقائياً بين Description/Description_En/Description_Fr/Description_DE (الأعمدة
    // الحقيقية على SysConfigMenus - راجع SysConfigMenusCLS.cs وMODERNIZATION_NOTES.md) حسب لغة الواجهة
    // الحالية (LangService.CurrentLanguage). العربية تبقى دائماً fallback آمن لأي قائمة لم تُترجَم بعد
    // (خصوصاً الفرنسية/الألمانية، المُعلَّمتان IsFullyTranslated=false فى LanguageService).
    public static class MenuLocalization
    {
        public static string GetLocalizedDescription(this MenuViewModel menu, string? cultureCode)
        {
            if (menu == null) return string.Empty;

            var candidate = (cultureCode ?? string.Empty).ToLowerInvariant() switch
            {
                var c when c.StartsWith("en") => menu.Description_En,
                var c when c.StartsWith("fr") => menu.Description_Fr,
                var c when c.StartsWith("de") => menu.Description_DE,
                _ => menu.Description,
            };

            return string.IsNullOrWhiteSpace(candidate) ? menu.Description : candidate;
        }

        // 🟢 "الطريق الفعلي" لأي عنصر قائمة نهائي (Leaf): RazorPath (عمود SysConfigMenus.RazorPath، يُملأ
        // يدوياً بمسار Blazor الحقيقي لكل شاشة - راجع MODERNIZATION_NOTES.md) له الأولوية دائماً متى كانت له
        // قيمة فعلية، وإلا يُستخدَم KeysData كما كان الحال قبل إضافة العمود (أغلب قيم KeysData الحالية هى
        // مفاتيح شاشات الويندوز فورمز القديمة وليست مساراً حقيقياً - راجع KnownRoutes.cs). كل استخدامات
        // Href/NavigateTo/IsImplemented الثلاثة (NavMenuMud.razor وMenuNode.razor وHome.razor) يجب أن تمر من
        // هنا بدلاً من قراءة menu.KeysData مباشرة، حتى تتفق كلها على نفس القرار لكل عنصر قائمة.
        public static string GetEffectiveRoute(this MenuViewModel menu)
        {
            if (menu == null) return string.Empty;
            return string.IsNullOrWhiteSpace(menu.RazorPath) ? menu.KeysData : menu.RazorPath;
        }

        // 🟢 يجيب "أي عنصر قائمة يقابل المسار الحالي فعلياً؟" - مصدر واحد يُستخدَم من كل من MenuBreadcrumb.razor
        // (لبناء مسار التنقّل الهرمي أعلى الشاشة) وMainLayoutMud.razor (لتحديد عنوان/أيقونة التبويب الجديد
        // عند فتح شاشة من القائمة). المطابقة بـ "بادئة المسار" وليس تطابقاً حرفياً لأن مسارات الشاشات ذات
        // الباراميتر (مثل "/customers/info/{CustomerId:long}") مُخزَّنة فى RazorPath بدون جزء الباراميتر
        // (فقط "/customers/info") - وعند وجود أكثر من تطابق (نادر) يُفضَّل الأطول (الأكثر تحديداً).
        public static MenuViewModel? FindMenuForRoute(this IEnumerable<MenuViewModel> menus, string relativePath)
        {
            if (menus == null) return null;

            var pathParts = (relativePath ?? string.Empty).Split('?', 2);
            var normalizedPath = "/" + pathParts[0].Trim('/');
            var currentQuery = ParseQuery(pathParts.Length > 1 ? pathParts[1] : string.Empty);
            MenuViewModel? best = null;
            var bestScore = -1;

            foreach (var menu in menus)
            {
                var route = menu?.GetEffectiveRoute();
                if (string.IsNullOrWhiteSpace(route) || !route.StartsWith("/")) continue;

                // 🟢 RazorPath قد يحمل query string (sales-invoice?agile=1) لتمييز عناصر قائمة تشترك فى شاشة واحدة:
                // المسار يُطابَق بدونه، ولا يُقبَل العنصر إلا إذا كانت كل أزواج query الخاصة به موجودة بنفس القيم فى العنوان الحالى،
                // وعند التعدد يفوز الأطول مساراً ثم الأكثر تطابقاً فى query (الأكثر تحديداً).
                var routeParts = route.Split('?', 2);
                var normalizedRoute = "/" + routeParts[0].Trim('/');
                var isMatch = normalizedPath.Equals(normalizedRoute, StringComparison.OrdinalIgnoreCase)
                    || normalizedPath.StartsWith(normalizedRoute + "/", StringComparison.OrdinalIgnoreCase);
                if (!isMatch) continue;

                var routeQuery = ParseQuery(routeParts.Length > 1 ? routeParts[1] : string.Empty);
                if (routeQuery.Any(p => !currentQuery.TryGetValue(p.Key, out var v) || !string.Equals(v, p.Value, StringComparison.OrdinalIgnoreCase)))
                    continue;

                var score = normalizedRoute.Length * 100 + routeQuery.Count;
                if (score > bestScore)
                {
                    best = menu;
                    bestScore = score;
                }
            }

            return best;
        }

        private static Dictionary<string, string> ParseQuery(string query)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var pair in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var kv = pair.Split('=', 2);
                result[Uri.UnescapeDataString(kv[0])] = kv.Length > 1 ? Uri.UnescapeDataString(kv[1]) : string.Empty;
            }
            return result;
        }

        // 🟢 يبني سلسلة الأجداد من الجذر حتى leaf نفسه (leaf هو آخر عنصر) بالمشي عبر ParentId - تُستخدَم
        // لعرض مسار التنقّل الهرمي (Breadcrumb) أعلى كل شاشة، ولإعادة بناء نفس مستوى الدخول فى بطاقات
        // الرئيسية (Home.razor) عند الضغط على أحد عناصر هذا المسار.
        public static List<MenuViewModel> BuildAncestorPath(this IReadOnlyCollection<MenuViewModel> menus, MenuViewModel leaf)
        {
            var chain = new List<MenuViewModel>();
            if (menus == null || leaf == null) return chain;

            var current = leaf;
            var guard = 0;
            while (current != null && guard++ < 50)
            {
                chain.Add(current);
                var parentId = current.ParentId ?? 0;
                if (parentId == 0) break;
                current = menus.FirstOrDefault(m => m.Menu_ID == parentId);
            }

            chain.Reverse();
            return chain;
        }
    }
}
