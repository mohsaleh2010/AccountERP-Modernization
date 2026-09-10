namespace AccountERP.SharedUI.Services
{
    // 🟢 قائمة ثابتة (بلا استعلام قاعدة بيانات) منقولة حرفياً - بما فى ذلك تكرار نفس القيمة العددية لتسميتين
    // مختلفتين، وهى ليست خطأ نقل بل موجودة هكذا فى الأصل - من Connection4It.Account_Ehlak_Status.
    // Get_Account_Ehlak_Status() (عنصر تحكم WinForms فقط، مشروع Fixed_Assets).
    public static class FixedAssetLookups
    {
        public record LookupItem(int Value, string TextAr);

        public static readonly LookupItem[] EhlakStatuses =
        {
            new(1, "غير محدد"),
            new(1, "مشترى جديد"),
            new(2, "تحت الإهلاك"),
            new(3, "مباع"),
            new(4, "مستهلك بالكامل"),
            new(4, "خارج الخدمه"),
        };
    }
}
