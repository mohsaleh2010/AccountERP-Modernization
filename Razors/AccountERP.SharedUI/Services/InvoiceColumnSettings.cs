using AccountModelsLib;

namespace AccountERP.SharedUI.Services
{
    // 🟢 يقابل NewFrmInvoive.SetConfigDataonMonitor وItemData.SetConfigDataonMonitor فى الأصل: أعمدة جدول أصناف فاتورة المبيعات
    // (وحقولها فى شاشة إضافة الصنف) تُخفى بإعدادات الشركة Show_*_OnInvoiceMonitor. الإعداد الرئيسى Is_ActiveOrNot_Show_ColumnInvoiceMonitor
    // إذا كان 1 تُطبَّق الإعدادات الفرعية (0 = مخفى)؛ وإذا كان غير 1 نُبقى سلوك Razor السابق (لا أعمدة إضافية) بدل إغراق الشاشة.
    // كل الإعدادات تُقرأ من AccountModelsLib.Common (تُحمَّل من SysConfig عند بدء الخادم أو POST /Dev/ReloadSettings).
    public static class InvoiceColumnSettings
    {
        private static bool Master => Common.Is_ActiveOrNot_Show_ColumnInvoiceMonitor == 1;
        private static bool On(int flag) => Master && flag == 1;

        // عمولة الصنف (الأصل: مجموعة Show_Commision_Value ثم كل عمود بإعداده)
        public static bool CommissionGroup => On(Common.Show_Commision_Value_OnInvoiceMonitor);
        public static bool ItemCommissionRate => CommissionGroup && Common.Show_Item_CommissionRate_OnInvoiceMonitor == 1;
        public static bool ItemCommissionValue => CommissionGroup && Common.Show_Item_CommissionValue_OnInvoiceMonitor == 1;
        public static bool UnitCommissionValue => CommissionGroup && Common.Show_Unit_CommissionValue_OnInvoiceMonitor == 1;
        public static bool ItemCommissionTotal => CommissionGroup && Common.Show_ItemCommissionTotal_OnInvoiceMonitor == 1;

        // العمولة البديلة: فى الأصل تُصفَّر دائماً عند إضافة الصنف (ItemData سطر 1558-1564) فتُعرض قيمتها فقط.
        public static bool AlternateGroup => On(Common.Show_Alternate_Commission_OnInvoiceMonitor);
        public static bool AlternateItemCommissionRate => AlternateGroup && Common.Show_Alternate_Item_CommissionRate_OnInvoiceMonitor == 1;
        public static bool AlternateUnitCommissionValue => AlternateGroup && Common.Show_Alternate_Unit_CommissionValue_OnInvoiceMonitor == 1;
        public static bool AlternateItemCommissionValue => AlternateGroup && Common.Show_Alternate_Item_CommissionValue_OnInvoiceMonitor == 1;
        public static bool AlternateItemCommissionTotal => AlternateGroup && Common.Show_Alternate_ItemCommissionTotal_OnInvoiceMonitor == 1;
        public static bool NetItemCommission => CommissionGroup || AlternateGroup;

        public static bool ThreeMola => On(Common.Show_3Mola_Value_OnInvoiceMonitor);
        public static bool ItemsCode => On(Common.Show_ItemsCode_OnInvoiceMonitor);
        public static bool StoreName => On(Common.Show_StoreName_OnInvoiceMonitor);
        public static bool ItemNo => On(Common.Show_ItemNo_OnInvoiceMonitor);
        public static bool ItemNoEditable => Common.Enable_ItemNoOnInvoice == 1;

        public static bool TaxSalesGroup => On(Common.Show_Item_TaxSale10_OnInvoiceMonitor);
        public static bool TaxSalesRate => TaxSalesGroup && Common.Show_Item_TaxSale10_Rate_OnInvoiceMonitor == 1;
        public static bool TaxSalesValue => TaxSalesGroup && Common.Show_Item_TaxSale10_Value_OnInvoiceMonitor == 1;
        public static bool ATS05Group => On(Common.Show_Item_ATS05_OnInvoiceMonitor);
        public static bool ATS05Rate => ATS05Group && Common.Show_Item_ATS05_Rate_OnInvoiceMonitor == 1;
        public static bool ATS05Value => ATS05Group && Common.Show_Item_ATS05_Value_OnInvoiceMonitor == 1;

        public static bool StatementNo => On(Common.Show_StatementNo_Value_OnInvoiceMonitor);
        public static bool OperationNo => On(Common.Show_OperstionNo_Value_OnInvoiceMonitor);
        public static bool MerchantName => On(Common.Show_Merchant_Name_OnInvoiceMonitor);
        public static bool Tash3ela => On(Common.Show_Tash3ela_OnInvoiceMonitor);
        public static bool FreeQuantity => On(Common.Show_ItemsQnty_Free_OnInvoiceMonitor);
        public static bool FreeItemNo => On(Common.Show_Item_No_Free_OnInvoiceMonitor);
        public static bool ExpireDate => On(Common.Show_Item_Expire_Date_OnInvoiceMonitor);
        public static bool TotalNoTax => On(Common.Show_columnTotalNoTax_OnInvoiceMonitor);
        public static bool QntyValue => On(Common.Show_Qnty_Value_OnInvoiceMonitor);

        // أسماء أعمدة تخصصها الشركة (Statement_Name وTager_Kontra_Name) وإلا يُستعمل الاسم الافتراضى فى الترجمة.
        // القيمة "0" فى الإعداد تعنى «بلا اسم مخصص» (تظهر هكذا فى قاعدة الشركة) فيُستعمل الاسم الافتراضى.
        private static string CustomOr(string? custom, string fallback) => string.IsNullOrWhiteSpace(custom) || custom.Trim() == "0" ? fallback : custom;
        public static string StatementLabel(string fallback) => CustomOr(Common.Statement_Name, fallback);
        public static string MerchantLabel(string fallback) => CustomOr(Common.Tager_Kontra_Name, fallback);

        // أى حقل إضافى مفعّل => يظهر قسم «بيانات إضافية للصنف» فى الحوار
        public static bool AnyDialogField =>
            ItemCommissionRate || ItemCommissionValue || UnitCommissionValue || (ItemNo && ItemNoEditable) || TaxSalesRate || ATS05Rate
            || StatementNo || OperationNo || MerchantName || Tash3ela || FreeQuantity || FreeItemNo || ExpireDate;
    }
}
