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

        // 🟢 أيقونة كل وحدة رئيسية (المستوى الأعلى، ParentId==0) بنمط Filled - كانت نسخة خاصة داخل
        // NavMenuMud.razor فقط؛ نُقلت هنا لتُستخدَم أيضاً من بطاقات الشاشة الرئيسية (Home.razor) حتى تكون
        // نفس أيقونة كل وحدة سواء ظهرت كبطاقة أو كعنصر بالقائمة الجانبية، بدل أيقونة Folder/Apps العامة
        // الموحَّدة لكل البطاقات التى لا تميّز الوحدات عن بعضها بصرياً.
        private static readonly (string[] Keywords, string Icon)[] ModuleIconMap =
        {
            (new[] { "حساب", "قيود", "استاذ", "ميزان", "account", "ledger", "journal", "trial" }, Icons.Material.Filled.AccountBalance),
            (new[] { "مخزن", "صنف", "اصناف", "مخزون", "store", "inventory", "item", "warehouse" }, Icons.Material.Filled.Inventory2),
            (new[] { "مبيع", "فاتورة بيع", "sale", "sales" }, Icons.Material.Filled.PointOfSale),
            (new[] { "مشتري", "شراء", "purchase", "buying" }, Icons.Material.Filled.ShoppingCart),
            (new[] { "عميل", "عملاء", "customer", "client" }, Icons.Material.Filled.People),
            (new[] { "مورد", "موردين", "supplier", "vendor" }, Icons.Material.Filled.Store),
            (new[] { "تقرير", "report" }, Icons.Material.Filled.Assessment),
            (new[] { "ضريب", "فاتورة الكترونية", "tax", "einvoice", "e-invoice" }, Icons.Material.Filled.Receipt),
            (new[] { "بنك", "شيك", "خزينة", "صندوق", "bank", "check", "cash", "treasury" }, Icons.Material.Filled.AccountBalanceWallet),
            (new[] { "سيار", "صيانة", "ورشة", "car", "workshop", "maintenance" }, Icons.Material.Filled.CarRepair),
            (new[] { "حجز", "سياح", "فندق", "طيران", "reservation", "tourism", "hotel", "flight" }, Icons.Material.Filled.FlightTakeoff),
            (new[] { "موارد بشرية", "موظف", "حضور", "انصراف", "hr", "employee", "attendance" }, Icons.Material.Filled.Badge),
            (new[] { "انتاج", "تصنيع", "نسيج", "production", "manufacturing" }, Icons.Material.Filled.PrecisionManufacturing),
            (new[] { "مقاول", "عقار", "مشروع", "construction", "real estate", "project" }, Icons.Material.Filled.Apartment),
            (new[] { "اعداد", "صلاحي", "مستخدم", "setting", "permission", "user", "admin" }, Icons.Material.Filled.Settings),
            (new[] { "باركود", "bar code", "barcode" }, Icons.Material.Filled.QrCode2),
        };

        // 🟢 أيقونة كل مجموعة فرعية (ليست مستوى أعلى، لكن لها أبناء) بنمط Outlined لتمييزها بصرياً عن
        // الوحدات الرئيسية (Filled) - كانت نسخة خاصة داخل MenuNode.razor فقط؛ نُقلت هنا لنفس سبب ModuleIconMap.
        private static readonly (string[] Keywords, string Icon)[] GroupIconMap =
        {
            (new[] { "حساب", "قيود", "استاذ", "ميزان", "account", "ledger", "journal", "trial" }, Icons.Material.Outlined.AccountBalance),
            (new[] { "مخزن", "صنف", "اصناف", "مخزون", "store", "inventory", "item", "warehouse" }, Icons.Material.Outlined.Inventory2),
            (new[] { "مبيع", "فاتورة بيع", "sale", "sales" }, Icons.Material.Outlined.PointOfSale),
            (new[] { "مشتري", "شراء", "purchase", "buying" }, Icons.Material.Outlined.ShoppingCart),
            (new[] { "عميل", "عملاء", "customer", "client" }, Icons.Material.Outlined.People),
            (new[] { "مورد", "موردين", "supplier", "vendor" }, Icons.Material.Outlined.Store),
            (new[] { "تقرير", "report" }, Icons.Material.Outlined.Assessment),
            (new[] { "ضريب", "فاتورة الكترونية", "tax", "einvoice", "e-invoice" }, Icons.Material.Outlined.Receipt),
            (new[] { "بنك", "شيك", "خزينة", "صندوق", "bank", "check", "cash", "treasury" }, Icons.Material.Outlined.AccountBalanceWallet),
            (new[] { "سيار", "صيانة", "ورشة", "car", "workshop", "maintenance" }, Icons.Material.Outlined.CarRepair),
            (new[] { "حجز", "سياح", "فندق", "طيران", "reservation", "tourism", "hotel", "flight" }, Icons.Material.Outlined.FlightTakeoff),
            (new[] { "موارد بشرية", "موظف", "حضور", "انصراف", "hr", "employee", "attendance" }, Icons.Material.Outlined.Badge),
            (new[] { "انتاج", "تصنيع", "نسيج", "production", "manufacturing" }, Icons.Material.Outlined.PrecisionManufacturing),
            (new[] { "مقاول", "عقار", "مشروع", "construction", "real estate", "project" }, Icons.Material.Outlined.Apartment),
            (new[] { "اعداد", "صلاحي", "مستخدم", "setting", "permission", "user", "admin" }, Icons.Material.Outlined.Settings),
            (new[] { "باركود", "bar code", "barcode" }, Icons.Material.Outlined.QrCode2),
        };

        public static string GetLeafIcon(string? description) => MatchIcon(description, LeafIconMap, Icons.Material.Outlined.Description);

        public static string GetModuleIcon(string? description) => MatchIcon(description, ModuleIconMap, Icons.Material.Filled.Apps);

        public static string GetGroupIcon(string? description) => MatchIcon(description, GroupIconMap, Icons.Material.Outlined.FolderOpen);

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
