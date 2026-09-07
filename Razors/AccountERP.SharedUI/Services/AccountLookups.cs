namespace AccountERP.SharedUI.Services
{
    // 🟢 القوائم الثابتة (بلا استعلام قاعدة بيانات) المستخدَمة فعلياً فى شاشة شجرة الحسابات على الويندوز
    // فورمز - منقولة حرفياً من الكنترولز الأصلية (وليس نسخة connectionit المكررة المختلفة قليلاً - راجع
    // MODERNIZATION_NOTES.md لتفاصيل الفرق):
    // - Account_Common_LIB.Account_Common.Account_Follows.cs (AccountList())
    // - Account_Common_LIB.Account_Common.Account_CashFlowType.cs (AccountList())
    // - Connection4It.FirstAccountCoding.cs (Currency_List_())
    public static class AccountLookups
    {
        public record LookupItem(string Value, string TextAr);

        public static readonly LookupItem[] AccountFollows =
        {
            new("1", "حساب التشغيل"),
            new("2", "حساب المتاجرة"),
            new("3", "حساب أرباح وخسائر"),
            new("5", "قائمة الدخل"),
            new("4", "الميزانية العمومية"),
        };

        public const string AccountFollowsDefault = "2";

        public static readonly LookupItem[] CashFlowTypes =
        {
            new("0", "غير محدد"),
            new("1", "تشغيلي"),
            new("2", "تمويلي"),
            new("3", "استثماري"),
        };

        public const string CashFlowTypeDefault = "1";

        public static readonly LookupItem[] FirstSubCodes =
        {
            new("1", "1 >> 9"),
            new("01", "01 >> 99"),
            new("001", "001 >> 999"),
            new("0001", "0001 >> 9999"),
        };

        public const string FirstSubCodeDefault = "1";

        public static string GetAccountFollowsText(int value) =>
            System.Array.Find(AccountFollows, x => x.Value == value.ToString())?.TextAr ?? value.ToString();

        public static string GetCashFlowTypeText(int value) =>
            System.Array.Find(CashFlowTypes, x => x.Value == value.ToString())?.TextAr ?? value.ToString();
    }
}
