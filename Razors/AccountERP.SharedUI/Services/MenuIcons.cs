using System;
using System.Linq;
using MudBlazor;

namespace AccountERP.SharedUI.Services
{
    // 🟢 أيقونة كل شاشة نهائية (Leaf) حسب كلمات مفتاحية فى اسمها - كانت مكرَّرة كنسخة خاصة داخل
    // MenuNode.razor فقط؛ نُقلت هنا لتُستخدَم أيضاً من شريط التبويبات (TabHost.razor/MainLayoutMud.razor)
    // بدل تكرارها نسخة ثالثة، حتى تبقى نفس الأيقونة لأي شاشة سواء ظهرت فى القائمة الجانبية أو كتبويب مفتوح.
    public static class MenuIcons
    {
        private static readonly (string[] Keywords, string Icon)[] LeafIconMap =
        {
            (new[] { "شجرة", "tree" }, Icons.Material.Outlined.AccountTree),
            (new[] { "قائمة", "list", "عرض", "استعلام", "inquiry" }, Icons.Material.Outlined.ViewList),
            (new[] { "اضافة", "إضافة", "جديد", "add", "new" }, Icons.Material.Outlined.AddCircleOutline),
            (new[] { "تعديل", "edit" }, Icons.Material.Outlined.EditNote),
            (new[] { "تقرير", "report" }, Icons.Material.Outlined.Assessment),
            (new[] { "طباعة", "print" }, Icons.Material.Outlined.Print),
            (new[] { "قيد", "journal", "entry" }, Icons.Material.Outlined.MenuBook),
            (new[] { "ميزان", "trial", "balance" }, Icons.Material.Outlined.Balance),
            (new[] { "اصل", "أصل", "asset" }, Icons.Material.Outlined.Domain),
            (new[] { "اعداد", "صلاحي", "setting", "permission" }, Icons.Material.Outlined.Tune),
        };

        public static string GetLeafIcon(string? description) => MatchIcon(description, LeafIconMap, Icons.Material.Outlined.Description);

        private static string MatchIcon(string? description, (string[] Keywords, string Icon)[] map, string fallback)
        {
            if (string.IsNullOrWhiteSpace(description)) return fallback;

            foreach (var (keywords, icon) in map)
            {
                if (keywords.Any(k => description.Contains(k, StringComparison.OrdinalIgnoreCase)))
                {
                    return icon;
                }
            }

            return fallback;
        }
    }
}
