using AccountModelsLib.Models;

namespace AccountModelsLib
{
    // 🟢 يجمع خطوات تحميل إعدادات SysConfig "الخاصة بكل المنشأة" (وليست خاصة بمستخدم بعينه) فى مكان واحد
    // بدل تكرارها فى أكثر من نقطة دخول (AccountERP.Web\Program.cs، وAccounts.LoadingFrm فى الويندوز
    // فورمز) - نفس تسلسل الاستدعاءات بالضبط، فمصدر الحقيقة الوحيد لهذا التسلسل هنا فقط. لا يحتوي على
    // AccountCommonDataLIB.XmlDataSet.ReadXmlDataSetConnStr() (خطوة قراءة ملف ConnInfo.xml المشفر، خاصة
    // بالويندوز فورمز فقط) ولا Common.Get_Is_Force_Admin_Permission() (يعتمد على Common.User_Id الخاص
    // بمستخدم بعينه بعد الدخول فعلياً، وليس على إعدادات المنشأة العامة) - راجع MODERNIZATION_NOTES.md.
    public static class SystemSettingsLoader
    {
        public static void LoadTenantSettings()
        {
            try { Common.SetSystemParametersDB(); } catch { }
            Common.SetMy_DbContextConnStr();
            Common.CommonTaxConfig = new CommonConfiguration(Common.Tax_BranchesId);
            Common.Is_EncryptedDataSetting();
            Common.UpdateQntySetZero();
        }
    }
}
