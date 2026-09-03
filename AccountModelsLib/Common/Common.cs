
using AccountModelsLib.Models;
using Newtonsoft.Json.Linq;
 
using System;
using System.Collections;
//using AccountModelsLib.DBModels;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
 
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
 
using static System.Net.Mime.MediaTypeNames;
namespace AccountModelsLib
{
    /// <summary>
    /// Summary description for Common.
    /// </summary>
    public class Common
    {
        public static int IsInDesignAndDevelopmentTime { get; set; } = 1;
        //public static string DesignedBy = "Designed By:Egy Linux,+201557668366,mohsaleh2010@yahoo.com";
        public static string DesignedBy = "EgySIT Co. - Designed By:Eng.Mohammed Saleh ,+201557668366,moh_saleh2010@yahoo.com";
        public static string CompanyPhone = "01208800915-01115556074";
        public static int Project_ID = 0;
        public static int Is_Force_Admin_Permission = 0;
        public static DataTable dtUnit = new DataTable();
        public static int Project_GeneralAccounts = 0;
        public static int Make_Login = 1;
        public static int Make_Login_Auto = 0;
        public static string Project_Str = "Advanced ERP System";
        public static string Project_StrAr = "منصة الحسابات - منظومة الحسابات الشاملة";

        public static string Project_Module_Str = "General Ledger Module";
        public static string Sunisoft_IrisSkin_SerialNumber = "isBx30VBCC0GTnJuOKmQ0jK7I8NqeyOjDigXGIG5v1dN7aw4qc3Ogw==";
        public static int Balance_BeginingPeriod;
        public static int Flage_AccountDetails_View = 1;

        public static int Language_ID = 0;//arabic language
        public static string DataBaseType = "SqlServer";
        public static int Is_User_Admin = 1;
        public static int User_Id = 1;
        public static int Branch_Id = 1;
        public static int Tax_BranchesId = 1;
        public static string User_Name = "Admin";
        public static string FullUserName = "Admin";
        public static string Branch_Name { get; set; } = "Company Name";
        public static string CompanyTitle_Client07 = "مؤسسة المهدى الأعمى";


        public static string CompanyTitle_Client02 = "أندلس بـاك";
        //public static string CompanyTitle_Client02 = "www.e-computersystem.com";
        public static string CompanyTitle_Client09 = " فوكس";
        public static string CompanyTitle_Client10 = " حــلايب قتيلو ";
        public static string CompanyTitle_Client13 = "www.e-computersystem.com";
        //public static string CompanyTitle_Client13 = "إسم المصنع";
        //public static string CompanyTitle_Client01 = "الجوكر للملابس الجاهزة";
        public static string CompanyTitle_Client06 = " ";

        //public static string CompanyTitle_Client06 = "عيد - للأدوات الكهربائية";
        public static string CompanyTitle_Client11 = "HAM Trade";

        //public static string CompanyTitle_Client15 = "شركة جراند بلو  ";
        public static string CompanyTitle_Client15 = "شركة ينبع للسياحة  "; //500025
        //public static string CompanyTitle_Client05 ="المصطفى للإلكترونيات";// "دواجن الوطنية";
        //public static string CompanyTitle_Client05 = "مخزن - المصطفى للإلكترونيات";
        //public static string CompanyTitle_Client05 = " جاد للكمبيوتر ";
        //public static string CompanyTitle_Client05 = " العشرى للتجارة ";
        ///public static string CompanyTitle_Client05 = " العشرى للتجارة ";
        //public static string CompanyTitle_Client05 = "مشويات وكشرى زياده";
        //public static string CompanyTitle_Client05 = "DARMAGHA";
        public static string CompanyTitle_Client05 = "مطعم لغاويص";
        //public static string CompanyTitle_Client04 = "الجمال للسياحه";

        public static string CompanyTitle_Client_A = "رزق السروجى";
        public static string CompanyTitle_Client_B = "راضى محسن السروجى وشركاة";
        public static string CompanyTitle_Client_C = "رزق السروجى";
        public static string CompanyTitle_Client03 = "نيوسكاى - فرع طلعت حرب";// " 500000       ";//"الفوزان للنقل الجوى والبرى";
        public static string CompanyTitle_Client08 = "مصنع الطارق";
        //public static string CompanyTitle_Client12 = " البيك / أحمد الخولى ";//"الفوزان للنقل الجوى والبرى";
        //public static string CompanyTitle_Client12 = "مصنع الطارق";//"الفوزان للنقل الجوى والبرى";
        //public static string CompanyTitle_Client12 = "وان تاتش";
        //public static string CompanyTitle_Client12 = "بنتـى حفصة";
        //public static string CompanyTitle_Client12 = "الأسود لصناعة الجنز";

        public static string CompanyTitle_Client12 = "مصنع أبوسمرة للمفروشات";

        //public static string CompanyTitle_Client08 = "مصنع الطارق";
        public static string UnionGroup = "  د.محمود النجريدى ";//"الفوزان للنقل الجوى والبرى";

        //public static string CompanyTitle_Client01 = CompanyTitle_Client11;
        //public static string CompanyTitle_Client01 = "Egy Linux";
        //public static string CompanyTitle_Client01 = "عيد - للأدوات الكهربائية"; //  -500024-
        //public static string CompanyTitle_Client01 = "الشركة السورية";
        //public static string CompanyTitle_Client01 = "شركة الطمبارى واخواته لتجارة الورق والدوبلكس ومستلزمات مصانع الكرتون";
        //public static string CompanyTitle_Client01 = " لاشين للإستيراد والتصدير ";  //500022
        //public static string CompanyTitle_Client01 = "  حامد ابوالعزم ";  //500023
        //public static string CompanyTitle_Client01 = "  مصنع الامبراطور للزجاج الهياتم";  //50002600
        //public static string CompanyTitle_Client01 = "  مصنع الامبراطور للزجاج الورشة ";  //50002601

        //public static string CompanyTitle_Client01 = "  أولاد الديب للتجاره ";  //500027
        //public static string CompanyTitle_Client01 = "  معرض نور للأجهزة الكهربائية ";  //500028
        //public static string CompanyTitle_Client01 = "  معرض سى عمر      ";  //500029
        //public static string CompanyTitle_Client01 = "مكة - للأدوات الكهربائية";  500030 
        //  public static string CompanyTitle_Client01 = " مصنع المجد نكسـت ";  //500031
        //public static string CompanyTitle_Client01 = " للمفروشات Zero مصنع ";  //500032
        // public static string CompanyTitle_Client01 = " الأسود لصناعة الجنز ";  //500033
        // public static string CompanyTitle_Client01 = "  كشمير";  //500034
        //public static string CompanyTitle_Client01 = "  مصنع هابى هاوس";  //500035

        //public static string CompanyTitle_Client01 = " مصنع سريهات للمفروشات ";  //500037
        //public static string CompanyTitle_Client01 = " شركة شابون للنسيج ";  //500038
        //public static string CompanyTitle_Client01 = "مصنع الفضالى للمفروشات";//500039
        // public static string CompanyTitle_Client01 = "مصنع الإسراء للملابس";//500040

        //public static string CompanyTitle_Client01 = "للمفروشات pupilla مصنع";//500041
        //public static string CompanyTitle_Client01 = "مصنع عماد النجار";//500042

        // public static string CompanyTitle_Client01 = "مصنع الغدير للمفروشات";//500043
        //public static string CompanyTitle_Client01 = "مصنع مس رغد للملابس";//500044
        // public static string CompanyTitle_Client01 = "للملابس الجاهزة STM مصنع";//500045
        public static string CompanyTitle_Client01 = "شركة السعيد شرف للورق";//500046
        /*
           01/03/2020
          public static string CompanyTitle_Client01 = "مصنع مودست ملابس الرياضية ";//500047
          public static string CompanyTitle_Client01 = "مصنع كريم ملابس اطفال وحريمى ";//500047
          public static string CompanyTitle_Client01 = "مصنع زهرة المدينة للمفروشات ";//500048


             public static string CompanyTitle_Client01 = "مصنع cotton House ";//500049
              public static string CompanyTitle_Client01 = "معرض السويقه مصر ";//500050
                public static string CompanyTitle_Client01 = "ماركة خالد باشا ";//500051
                public static string CompanyTitle_Client01 = "مصنع نانو تكس للملابس الجاهزه ";//500052
                  
             */

        public static string CompanyTitle_Client14 = "إسم المنشأة";

        public static string ClientCompanyTitle = "";//  CompanyTitle_Client01;//  ;

        public static string InstituteName_ = CompanyTitle_Client01;


        public static string InstituteNameFromInfo = CompanyTitle_Client01;

        public static string CompanyTitle { get; set; } = InstituteName;

        public static string InstituteName
        {
            get
            {
                return InstituteName_;
            }

            set
            {
                //#3
                InstituteName_ = value;
                //CompanyTitle = InstituteName_;
            }
        }
        //public static string maskCarNumber = "?-?-?      ########";
        public static string maskCarNumber = "? ? ? ########";
        //public static string EmptyStrmaskCarNumber = " - -       ";
        //public static string EmptyStrmaskCarNumber = " - -       ";
        public static string EmptyStrmaskCarNumber = "      ";
        public static string ItemNameForMasna3iatKhamat { get; set; } = "مصنعية خامات";
        public static string TaxinternalID_Prevx { get; set; } = "Einv-";
        public static int IsItemSellPriceContainTaxValue = 1;// سعر البيع للصنف شامل الضريبة فى بيانات النف
        public static int TaxinternalID_StartFromNumber { get; set; } = 6620;
        public static string RenewalDateForTaxPortalCloud { get; set; } = "29/8/2024";
        public static int IsNeedToRenewalForTaxPortalCloud { get; set; } = 0;
        public static int IsCheckRenewalForTaxPortalCloud { get; set; } = 0;
        public static int IsBtnAddItemsDirectTocarWorkOrder { get; set; } = 0;
        public static int storePriceType_CostPriceOrLastPrice = 2; // 1= Cost Price , 2= Last Price
        public static int IsEnableStore_CostPriceOrLastPrice = 0; // 1= قابل للتعديل

        public static int TaxinternalID_StartFromNumber_AS_InvoiceList { get; set; } = 0;
        public static int IsUseStaticDataForCustomerReceiver { get; set; } = 1;
        public static string FromWhereTableNameGet_TaxInvoiceNo { get; set; } = "Invoices_List";
        public static string User_Agent_ToSendToTaxPortal { get; set; } = "Byte_ERP";

        public static CommonConfiguration CommonTaxConfig;//=new CommonConfiguration();
        public static string Is_Paid_Str = "Yes";
        public static string Is_Not_Paid_Str = "No";
        //public static string CompanyName_Tel1 = "الكابتن / طارق الغضبان 01064752909";
        //public static string CompanyName_Tel2 = "أ/ محمد الغضبان 01206923223";

        //public static string CompanyName_Tel1 = "01140423701 ";

        //public static string CompanyName_Tel2 = " ";
        public static string CompanyName_Tel1 { get; set; } = " ";
        public static string CompanyName_Tel2 { get; set; } = "  ";
        public static string Project_TypeStr = "Tourism"; //Tourism , POS , Market  , Factory , AlFawzan , Client
        public static int Is_Ecrypted_Config = -1;
        public static int Is_Ecrypted_Menu = 0;
        public static int Is_Save_DataDictionary = 0; // Save The DML To DataDictionary table 0 not save, 1 save
        public static int Is_Save_DataDictionaryLogerException = 0; // Save The DML To DataDictionary table 0 not save, 1 save
        
        public static int Is_Save_DataDictionaryDbSyncLogs = 0; // Save The DML To DataDictionary table 0 not save, 1 save
        public static int Is_ShowPrintDialog = 0;
        public static int Is_ShowPrintDialog_POS = 0;
        public static string Backgroundimage_Name = " ";
        public static string PrinterNameForReportsA4 = "";  // إسم طابعة التفارير
        public static string PrinterNameForPOSInvoice = ""; //اسم طابعةفواتير POS
        public static string PrinterNameForKitchenPrinter = ""; //اسم طابعةفواتير POS
        public static string PrinterNameForBarCodePrinter = ""; //اسم طابعةفواتير POS
        
        public static int IsSentToPrinterNameForKitchenPrinter = 0;
        public static int Is_Print_Branch_Name = 1;
        public static int Is_Print_CompanyName = 1;
        public static int IS_ReadConStrFromXMLInfo = 0;
        public static int Project_Market = 1;
        public static int PointOfSale = 1;
        public static int Is_InvoiceNoBy_Branch_Id = 0;
        public static int Is_InvoiceNoBy_FinancialYear = 0;
        public static int Is_InvoiceNoBy_Invoice_Type = 0;

        public static int Is_SaveOnDB_When_AddItemDirectly_On_invoice = 0;
        public static int Is_SaveOnDB_When_AddItemDirectly_On_Bill = 0;
        public static int UseAutomaticItemCode = 1;
        public static int Is_ShowItemSpecificationinItemDataList = 0;
        public static int Is_SearchItemBy_BarCode = 0;
        public static int Show_SearchItemBy_BarCode_On_Invoice = 0;
        public static int Store_Id_UsedBefore = -1;
        public static int Store_Index_UsedBefore = -1;
        public static string TextToShowUnderCodeWhenPrintBarCode = "صيدلية د/ عاء حسنى";
        public static int IsSalesMan = 0;  // IsSalesMan =0 is customer , IsSalesMan=1 if sales man
        public static int Customer_Type = 1;
        public static int Customer_Id_ForInvoiceNakdy { get; set; } = -1;  // اختيار العميل للفاتورة النقدى
        public static int RemoveCustomer_Id_ForInvoiceNakdy = -1;  // حذف العميل للفاتورة النقدى


        public static int Customer_Id_ForInvoiceNakdyThenDisableCombo = -1;  // اختيار العميل افتراضى لا يمكن التغير
        public static int Customer_Id_DefaultSelect = -1;  // اختيار العميل افتراضى
        public static int Supplier_Id_DefaultSelect = -1;  // اختيار المورد افتراضى
        public static int Sandouk_Id_DefaultSelect = -1;   //اختيار الصندوق افتراضى
        public static int Currency_Id_DefaultSelect = -1;  // اختيار العملة افتراضى 
        public static int IsDisableEditEntryWhenCreateAutomatic = 1; // عدم تعديل القيد لو تم انشاءة اليا

        public static int IS_Make_Automatic_Queeod = 0;
        public static int VersionRun_On_OnePC_Using_HardwareInfo = 0;  //نسخة تعمل على جهاز واحد POS
        public static string HardWareInfo_Serial = "BFEBFBFF000006FD-WD-WXE808LF5638";  // 
        public static int Make_Sarf_Item_BalanceFromInvoice = 1; // السماح صرف من رصيد الصنف اثناء فاتورة البيع
        public static int Make_Store_Order_Sarf_Estlam_Automatic_From_Invoice = 0; //انشاء الاذن للمخزن عند انشاء الفاتورة
        public static int Make_Store_Order_Sarf_Estlam_Ta7weel_Automatic_Between_Stores = 0; // إنشاء اذن التحويل اوتوماتيك بين المخازن عند اجراء تحويل
        public static int Make_Store_Order_Sarf_Estlam_Automatic_From_Invoice_Enabled = 0; //انشاء الاذن للمخزن عند انشاء الفاتورة

        public static int ItemDeleteOrMakeNotActiveInDataBase = 1;  //  Delete==1 // Edit=0;
        public static int ItemCategory_Type = 1;  //  ----غير محدد 0     //   1 مبيعات مشتريات
        public static int Show_BtnOrderSarfStoreFromInvoice = 1; // / زرار استلام نقدية فى فاتورة مبيعات مرئى
        public static int ShowSalesMan = 1;  // أستخدام مندوبى المبيعات
        public static int MakeOrderMoneyFromInvoiceAutomaticWithoutForm = 0;
        public static int MakeOrderMoneyFromInvoice_Reverse_AutomaticWithoutForm = 0;
        public static int MakeOrderMoneyFromBill_AutomaticWithoutForm = 0;
        public static int MakeOrderMoneyFromBills_Reverse_AutomaticWithoutForm = 0;


        public static int MakeOrderMoneyFromInvoice = 1;   ///انشاء استلام نقدية من فاتورة مبيعات
                                                           /// <summary>
                                                           /// 1 mean  =زيادة الرصيد وتفعيل الاذن اوتوماتيك اى انشاء الاذن كأنة اذن صرف من واذن استلام الى
                                                           /// </summary>
        public static int Compute_ItemBalance_With_Fraction = 0; // حساب رصيد الصنف بالوحده الكبرى والصغرى بدون كسور= 0   
        public static int Make_Store_Order_Sarf_Estlam_Automatic = 1;
        public static int Compute_QntyFrom_ItemNo = 0;//حساب الكمية من العدد واطوال الصف الطول والعرض

        public static int Compute_QntyFrom_ItemNo_CheckBox = 0;//حساب الكمية من العدد واطوال الصف الطول والعرض
        public static int Compute_QntyFrom_Only_ItemNo = 0;// حساب الكمية من العدد - الكيلو = 9  عدد  
        public static int Compute_QntyFrom_Only_ItemNoIsOne = 0;//حساب العدد من الكمية العدد = 9 كيلوا  
        public static int VisibleCompute_QntyFrom_ItemNo_CheckBox = 0; // 'عرض حساب الكمية من العدد  '
        public static int VisibleSpecification_QntyFrom_ItemNo = 0; //عرض زرار مواصفات الصنف عند البيع
        public static int Is_Show_Quantity_Lenghts_For_ItemsOnInvoice = 0;// 0=Qnty , 1= lenghts , 2= All إضافة الكميات والاطوال
        public static int Is_ShowTotalsItems_in_Invoice { get; set; } = 0;
        public static int Is_ShowTotalsItems_in_Bill { get; set; } = 0;


        public static int Used_Item_NoAS_3ebowaInReport_Balance = 0;//حساب العبوة فى حالة  التقارير
        public static int Is_Takseet_Invoice = 0;//   تشغيل برنامج تقسيط الفاتوره
        public static int IsBoxBalanceAllowSarfAny = 0; //  الصرف من الصندوق بدون رصيد
        public static int Compute_3eboaFrom_EznSarf_Estlam = 1;  //حساب العبوة فى حالة ان الصرف والاستلام
                                                                 //public static int Compute_3eboaFrom_InReport = 0;  //حساب العبوة فى حالة  التقارير
        public static int Update_DeleteSupplier = 1;//  0 mean update , 1 mean delete
        public static int Is_ShowUnitBeside_Qnty = 1;//  0 show Qnty only , 1 show Qnty Beside Unit
        public static string Employee_Tables_IS_USER_DETAILES_Or_Employee = "Employee";//إستخدام جداول الموظفينUSER_DETAILES_==0 _Employee==1

        public static int Transform_Type = 1;
        public static int Year_Financial_Period_Id = 1;  //رقم السنة المالية
        public static DateTime Year_Financial_Period_DateFrom;  //رقم السنة المالية
        public static DateTime Year_Financial_Period_DateTo;  //رقم السنة المالية

        public static int Dived_ByStore_Type_In_Invoice_Type = 1;  //تقسيم المخازن فى فاتورة المبيعات والمشتريات بمعنى عرض مخازن المنتج التام فى فاتورة البيع ومخازن المشتريات فى فاتورة المشتريات
        public static int Use_Default_Store_In_Invoice_Bill = 1;//انشاء فاتورة مشتريات من المخزن الافتراضى للصنف
        public static int IsReadPreviousAudience_priceOnBill = 0;
        public static int Can_ChangePrice_On_invoice = 0;  //تغيير السعر فى انشاء فاتورة
        public static int IS_Use_Price_Type_List = 0;  //استخدام قائمة اسعار للاصناف
        public static int Can_Chang_Automatic_SarfFromInvoice_ByItems = 0;// مبيعات تغيير اختيار الاصناف عند صرف اوتوماتيك من الفاتورة
        public static int Separate_Items_Sell_PurchaseByStoreType = 1;//شاشة الاصناف والمنتجات تفصل مخازن المبيعات عن المشتريات
        public static int IsUsePriceWithStoreOrder = 0; //استخدام سعر الصنف فى اذن صرف المخزن
        public static int Image_Item_TypeFor_Items = 0;  // للاصناف
        public static int Image_Item_TypeFor_CheckMoney = 1; // للشيكات
        public static int Image_Item_TypeFor_Entry = 2;       // لمستندات القيود
        public static int Image_Item_TypeFor_Customer = 3;       // صورة العميل  
        public static int Image_Item_TypeFor_Medical = 4;       // مستندات فحص طبى 
        public static int IS_Used_SandoukSystem = 1;//هل يتم استخدام برنامج الصندوق
        public static int IS_Used_SandoukWithPermission = 0;// وضع صلاحيات استخدام الخزينه  0 معناها الكل 1 معناها مع الصلاحية
        public static int Ehlak_Kest_Sabt = 1;//طريقه القسط الثابت
        public static int Ehlak_Kest_Motanakes = 2;//طريقه القسط المتناقص
        public static Char Account_Seperator_Char = '-';
        public static int Add_AccountEntryByTree = 0;//عند إضافة حساب فى القيد امكانية الاختيار من الشجره

        public static int Currency_Count { get; set; } = 7;
        public static int Is_PrintCompanyNameOnInvoice = 1; // طباعة اسم المصنع فى فاتورة البيع
        public static int Is_PrintCompanyNameOnBill = 1; // طباعة اسم المصنع فى فاتورة المشتريات
        public static string Invoice_Title = " فاتورة رقم "; // عنوان الفاتوره /  فاتورة رقم  / إذن صرف رقم

        public static string Customer_Title = "العميل";
        public static string Supplier_Title = "المورد";
        public static string Invoice_Title_Date = " تاريخ الفاتورة "; // عنوان الفاتوره /    تاريخ الفاتورة    /   تاريخ الإذن  

        public static string Invoice_MainTitle_Invoice = " فاتورة مبيعات "; // فاتورة مرتجع مشتريات /  فاتورة مشتريات  / فاتورة مبيعات/ فاتورة مرتجع مبيعات
        public static string Invoice_MainTitle_Bill = " فاتورة مشتريات "; // فاتورة مرتجع مشتريات /  فاتورة مشتريات  / فاتورة مبيعات/ فاتورة مرتجع مبيعات
        public static string Invoice_MainTitle_Invoice_Reverse = " فاتورة مبيعات "; // فاتورة مرتجع مشتريات /  فاتورة مشتريات  / فاتورة مبيعات/ فاتورة مرتجع مبيعات
        public static string Invoice_MainTitle_Bill_Reverse = " فاتورة مبيعات "; // فاتورة مرتجع مشتريات /  فاتورة مشتريات  / فاتورة مبيعات/ فاتورة مرتجع مبيعات

        public static int IsAddAllItemInAllStores = 0;
        public static int IsAddAllItemForAllCars = 0;

        public static string NotesBalanceLabelMankol = "رصيد منقول";
        public static int IsShowTotalTransactionInSearchPeriod = 1;
        public static int IsShowTotalTransaction = 1;

        /// <summary>
        /// Order Purchase
        /// </summary>
        public static int Show_Prices_Value_OnOrderPurchaseMonitor = 0;
        public static int Show_Taxes_Value_OnOrderPurchaseMonitor = 0;
        public static int IsShowCarWorkOrderDataOnFormStore = 0;


        public static int Is_ActiveOrNot_Show_ColumnInvoiceMonitor = 0;
        public static int Show_ItemNo_OnInvoiceMonitor = 0;
        public static int Enabled_ItemNo_OnItemData = 0;
        public static int Show_Commision_Value_OnInvoiceMonitor = 0;
        public static int Show_3Mola_Value_OnInvoiceMonitor = 0;
        public static int IsCompute_3Mola_Value_OnInvoiceMonitor = 0;
        public static decimal _3Mola_Value = 3;

        //Total3Mola Unit3Mola
        public static string Save_Backup_Path = @"E:\\BackUpMyDB";
        public static string Rasamat_Title = "رسمات";
        public static string Tager_Kontra_Name = "أنواع الكونترا";

        public static string Statement_Name = "رقم البيان";
        public static decimal Is_Div_100Or_1_Rate = 1; // هل يت القسمة على مائة ام على واحد
        public static decimal TaxSales_Rate = 0;
        public static decimal ATS05_Sales_Rate = 0;
        public static string InstituteNameEN_ = " ";
        public static int decimalValueRoundOrTruncate = 1; // Rounded = 1 . truncate = 2
        public static int Is_Unit_As_decimal = 0;
        public static int DecimalPlaces = 2;
        public static int DecimalPlaces_Must = 2;
        public static int DecimalPlaces_NoFractionIfZero = 1;

        public static int Is_Tax_Per_Item_Bill_ = 1;

        public static int Show_StatementNo_Value_OnInvoiceMonitor = 0;

        public static int Show_StoreName_On_Bill_Monitor = 0;
        public static int Show_StoreName_OnInvoiceMonitor = 0;


        public static int Show_OperstionNo_Value_OnInvoiceMonitor = 0;
        public static int Show_Merchant_Name_OnInvoiceMonitor = 0;
        public static int Show_Tash3ela_OnInvoiceMonitor = 0;
        public static int Show_ItemsQnty_Free_OnInvoiceMonitor = 0;
        public static int Show_Item_No_Free_OnInvoiceMonitor = 0;

        public static int Show_Qnty_Value_OnInvoiceMonitor = 0;
        public static int Show_columnTotalNoTax_OnInvoiceMonitor = 0;
        public static int Show_Alternate_Commission_OnInvoiceMonitor = 0;




        /// ////////////////////
        /// 


        public static int Show_ItemCommissionTotal_OnInvoiceMonitor = 0;
        public static int Show_Unit_CommissionValue_OnInvoiceMonitor = 0;
        public static int Show_Item_CommissionValue_OnInvoiceMonitor = 0;

        public static int Show_Item_CommissionRate_OnInvoiceMonitor { get; set; }
        //public static int Show_Item_CommissionRate_OnInvoiceMonitor = 0;
        public static int Show_Alternate_Item_CommissionRate_OnInvoiceMonitor = 0;
        public static int Show_Alternate_Unit_CommissionValue_OnInvoiceMonitor = 0;
        public static int Show_Alternate_Item_CommissionValue_OnInvoiceMonitor = 0;
        public static int Show_Alternate_ItemCommissionTotal_OnInvoiceMonitor = 0;
        public static int Show_ItemsCode_OnInvoiceMonitor = 0;



        public static int Show_ItemsCode_On_Bill_Monitor = 0;

        public static int Show_Alternate_Item_CommissionRate_On_Bill_Monitor = 0;
        public static int Show_Alternate_Unit_CommissionValue_On_Bill_Monitor = 0;
        public static int Show_Alternate_Item_CommissionValue_On_Bill_Monitor = 0;

        public static int Show_Alternate_ItemCommissionTotal_On_Bill_Monitor = 0;

        public static int Show_ItemCommissionTotal_On_Bill_Monitor = 0;

        public static int Show_Unit_CommissionValue_On_Bill_Monitor = 0;

        public static int Show_Item_CommissionValue_On_Bill_Monitor = 0;

        public static int Show_Item_CommissionRate_On_Bill_Monitor = 0;

        public static int Show_Item_TaxSale10_Rate_On_Bill_Monitor = 0;
        public static int Show_Item_TaxSale10_Value_On_Bill_Monitor = 0;
        public static int Show_Item_ATS05_Rate_On_Bill_Monitor = 0;
        public static int Show_Item_ATS05_Value_On_Bill_Monitor = 0;

        public static int Show_Item_TaxSale10_Rate_OnInvoiceMonitor = 0;
        //public static int Show_Item_TaxSale10_Value_OnInvoiceMonitor = 0;

        public static int Show_Item_TaxSale10_Value_OnInvoiceMonitor { get; set; }
        public static int Show_Item_ATS05_Rate_OnInvoiceMonitor = 0;
        public static int Show_Item_ATS05_Value_OnInvoiceMonitor = 0;

        //public static int Show_Item_TaxSale10_OnInvoiceMonitor = 0;
        public static int Show_Item_ATS05_OnInvoiceMonitor = 0;

        public static int Show_Item_TaxSale10_OnInvoiceMonitor { get; set; }

        public static int Enabel_cbIs_TaxPer_Item_OnInvoiceMonitor { get; set; }
        public static int Use_cbIs_TaxPer_Item_OnInvoiceMonitor { get; set; }
        // /////////////////////////

        public static int Show_Qnty_Value_On_Bill_Monitor = 0;
        public static int Show_columnTotalNoTax_On_Bill_Monitor = 0;
        public static int Show_Alternate_Commission_On_Bill_Monitor = 0;
        public static int Is_PriceByTenns_On_Bill_Monitor = 0;   // حساب الوزن بالطن فى فاتورة المشتريات
        public static int Show_Item_Expire_Date_OnInvoiceMonitor = 0;
        public static int Is_Item_Expire_Date_WithBalance = 0;
        public static int Show_Commision_Value_On_Bill_Monitor = 0;
        public static int Show_ItemNo_On_Bill_Monitor = 1;
        public static int Show_Tash3ela_On_Bill_Monitor = 0;
        public static int Show_ItemsQnty_Free_On_Bill_Monitor = 0;
        public static int Show_Audience_price_On_Bill_Monitor = 0;
        public static int Show_Item_No_Free_On_Bill_Monitor = 0;
        public static int Used_Item_NoAS_3ebowaInReport_Balance_InBill = 0;
        public static int Is_ActiveOrNot_Show_Column_Bill_Monitor = 0;
        public static int Show_Item_Expire_Date_On_Bill_Monitor = 0;
        public static string ColumnItemsName__Bill_ = "";
        public static string ColumnUnitPriceName_Bill = "";
        public static int Show_Item_TaxSale_On_Bill_Monitor = 0;
        public static int Show_Item_ATS05_On_Bill_Monitor = 0;
        public static int Show_StatementNo_Value_On_Bill_Monitor = 0;
        public static int Show_OperstionNo_Value_On_Bill_Monitor = 0;
        public static int Show_Merchant_Name_On_Bill_Monitor = 0;
        public static int IS_Make_Sarf_Item_BalanceFromInvoice = 1;// البيع فى فاتورة البيع من رصيد الصنف  1 = و 0 تعنى برصيد او بدون
        public static int Is_Tash3ela_WithBalance = 0;
        public static int Is_Bills_DetailsId_WithUnit_Item_ExpireDate = 0;
        public static int Is_P_Price_WithUnit_Item_ExpireDate = 0;
        //public static int Is_Tax_Per_Item = 1;
        public static string ColumnItemsName_Invoice = "";
        public static int Show_Qnty_With_Unit = 1;
        public static int ColumnQnty_width = 220;
        public static int ColumnItemsName_width = 220;
        public static int ColumnStoreName_width = 100;
        public static int ColumnSmallBalalance_Width_ItemData_OnInvoiceMonitor = 100;
        public static int ColumnBalance_Width_ItemData_OnInvoiceMonitor = 220;
        public static int ColumnItemName_Width_ItemData_OnInvoiceMonitor = 280;
        public static int Show_Customer_Notes_OnInvoiceMonitor = 0;
        public static int Show_Supplier_Notes_OnInvoiceMonitor = 0;
        public static int Show_EmployeeSeller_OnInvoiceMonitor = 0;
        public static int Show_Shipment_DataInvoiceMonitor = 0;
        public static int Show_Shipment_DataInvoiceOnPrint = 0;
        public static int ColumnQnty_width_Bill = 220;
        public static int ColumnItemsName_width_Bill = 220;
        public static int ColumnStoreName_width_Bill = 220;
        public static int ColumnEmpty_width_Bill = 0;
        public static int ColumnSmall_QntyStr_width_Invoice = 0;
        public static int ColumnSmall_QntyStr_width_Bill = 0;

        public static string ColumnSmall_QntyStr_Name_Invoice = "الصغرى";
        public static string ColumnSmall_QntyStr_Name_Bill = "الصغرى";
        public static string ColumnQnty_Name_Invoice = "الكمية المطلوبة";
        public static string ColumnQnty_Name_Bill = "الكمية المطلوبة";

        public static string ColumnLarg_Qnty_Name_Invoice = "عبوة";
        public static string columnSmall_Qnty_Name_Invoice = "جزء";

        public static int ColumnLarg_Qnty_Width_Invoice = 0;
        public static int columnSmall_Qnty_Width_Invoice = 0;


        public static int Show_AddNotes_On_Invoice = 1;
        public static int Show_AddNotes_On_Bill = 1;

        public static int Show_Load_Payment_OnBillPrint = 0; //"مصاريف التحميل  "
        public static int Show_ItemSecondTotalPrice_OnBillPrint = 0;   //"إجمالى"; + "مصاريف التحميل  "
        public static int Show_TaxSale_Value_OnBillPrint = 0;// "ضريبة المبيعات";

        public static int Show_Damaghat_OnBillPrint = 0; // "دمغة";
        public static int Show_Commision_OnBillPrint = 0;  // "الخصم";
        public static int Show_ATS05_Value_OnBillPrint = 0;//"أرباح تجارية وصناعية";
        public static int Show_SupplierAccount_AfterBeforInBillPrint = 0;//"أرباح تجارية وصناعية";
        public static int Show_NotesInBillPrint = 0;

        public static int Show_Total_List3Mola_OnBillPrint = 0; //"عمولة البيع";

        public static int Show_TotalBeforTax_OnInvoicePrint = 0;
        public static int Show_TotalQnty_OnInvoicePrint = 0;

        public static int Show_InvoicePaymentValuesOnInvoicePrint = 0;

        public static int Show_InvoicePaymentValues_StillsOnInvoicePrint = 0;

        public static int Show_TotalAfterTax_OnInvoicePrint = 0;
        public static int Show_CustomerAccount_Befor_OnlyInInvoice = 0;
        public static int Show_CustomerAccount_After_OnlyInInvoice = 0;
        public static int Show_CustomerAccount_FinalyNowInInvoicePrint = 0;
        public static string CompanyTitleEN = InstituteNameEN;
        public static string Company_Tax_No = "  ";
        public static string Company_Commercial_Register = "  ";
        public static string Company_Memberance_Register = "  ";
        public static string Company_Address1 = "  ";
        public static string Company_Address2 = "  ";
        public static string Company_Fax = "  ";
        public static string Company_PostOffice = "  ";
        public static string Company_footerAddressData = " ";
        public static int Is_Set_Customer_Balance_With_Items = 0;
        public static int Is_Set_Supplier_Balance_With_Items = 0;


        public static int Is_Set_Customer_Balance_With_Sum_Items = 0;
        public static int Is_Set_Supplier_Balance_With_Sum_Items = 0;

        public static int Is_Invoice_Qnty_Needed = 1;
        public static int Is_ToIndicDigitsArabic = 1;
        public static int Is_SaveUserNameLogin = 1;
        public static int Is_SaveUserPasswordLogin = 1;
        public static int Is_Invoice_ItemNo_Needed = 0;
        public static int Is_Invoice_Price_Needed = 0;



        public static int IsOpenItemDataToAddNew = 1;
        public static int Is_Update_Price_On_invoice = 1;
        public static int Is_Update_Price_On_Bill { set; get; } = 1;
        public static int Is_Check_Expiration_validation = 1;
        public static string Company_Tax_Invoice_URL_1 = "  ";
        public static string Company_Tax_Invoice_URL_2 = "  ";
        public static string Company_Tax_Invoice_URL_UserName = "  ";
        public static string Company_Tax_Invoice_URL_Password = "  ";
        public static int Show_NetInvoiceTotal_OnInvoiceMonitor = 0;
        public static int Show_Qnty_With_Unit_Bill_Show_Qnty_With_Unit = 1;
        public static string Is_ConnXML_Encrypted = "潗潟潝潟";
        public static string UsedXML_Conn = "1";
        public static string DTAdmin = "Stop"; //Run mean Run  - Stop mean Stop, for alter database program execute only month


        public static string DTAdmin_Run = "Run";
        public static string DTAdmin_Stop = "Stop";
        public static int is_pharmacy = 0;
        public static int IsSetCustomers_AccountsForInvoice = 1;
        public static int IsSetSuppliers_AccountsForBills = 1;
        public static int is_CarsMaintainceSystem = 0;
        public static string StartupForm_Name = "";



        //public static int Show_CustomerAccount_AfterBeforInInvoice = 0;
        //public static int Show_CustomerAccount_Befor_OnlyInInvoice = 0;
        //public static int Show_CustomerAccount_After_OnlyInInvoice = 0;
        //public static int Show_SupplierAccount_AfterBeforInInvoice = 0;
        //public static int Show_NotesInInvoicePrint = 0;
        //public static int Show_SpecificationInBillPrint = 0;
        //public static int Show_SpecificationInInvoicePrint = 0;






        public static int DueDateNearestDayes = 7;  // موعد اقتراب استحقاق الشيك بعدد ايام
        public static int IS_Make_AutomaticFromTar7eel_For_Invoice = 1;
        public static int IS_Make_AutomaticFromTar7eel_For_InvoiceReverse = 1;
        public static int IS_Make_AutomaticFromTar7eel_For_Bill = 1;
        public static int IS_Make_AutomaticFromTar7eel_For_BillReverse = 1;
        public static int POS_Is_StartInvoiceWith_All_MainCategoy = 0;
        public static int SelectFirstPriceListOnInvoice = 0;
        public static int POSInvoivePrinterCopiesNumber = 1;
        public static int UseInvoiceTypeKta3am_Khas = 0;
        public static int UseInvoiceNoByDay_POS = 0;
        public static int UseInvoice_POS = 0;
        public static int IsVisibleServerDataBackup = 1;
        public static string SQLDataSource = "";
        public static string SQLUserName = "";
        public static string SQLUserPassword = "";
        public static string SQLDataBaseName = "Connection_Data";

        public static string SQl_ServerExpress = "1";
        public static string SQl_ServerWindows_Authentication = "1";
        public static string SQl_Server_SQl_Authentication = "1";
        public static string SQl_NativeClientSQl_Server2005 = "1";
        public static string SQl_ServerExpressWithDataDirectory = "1";
        public static string SQl_LocalDB = "1";


        public static string Store_Text_Label = "المـخـزن";
        public static int Is_CheckSandokBalnce_Sarf = 1; // التأكد من رصيد الخزنه عند صرف النقدية
        public static int Use_AutoNumeric_Customer_Id__With_Submain_Account_Id = 0; // استخدام رقم اوتوماتيك للعميل او يستخدم رقم حسابة ك رقمه         ;
        public static int Use_AutoNumeric_Supplier_Id_With_Submain_Account_Id = 0; // استخدام رقم اوتوماتيك للعميل او يستخدم رقم حسابة ك رقمه
        public static int Use_AutoNumeric_Bank_Id_With_Submain_Account_Id = 1;
        public static int Use_AutoNumeric_Submain_Account_Id { get; set; } = 0;
        public static int Use_Auto_Identity_Column = 1;
      
        public static int Use_AutoNumeric_Employee_Id_With_Submain_Account_Id = 0;
        public static int ShowCustomerVisaData = 0;//عرض بيانات الفيزا للعميل
        public static int ShowCustomerImage = 0;//عرض زرار صورة العميل
        public static int Show_CustomerAccount_AfterBeforInInvoice = 0;   //طباعة رصيد العميل قبل وبعد المبيعات فى الفاتورة
        public static int Show_SupplierAccount_AfterBeforInInvoice = 0;//طباعة رصيد المورد  قبل وبعد المبيعات فى الفاتورة
        public static int Show_NotesInInvoicePrint = 0;//طباعة البيان فى الفاتورة
        public static int Show_SpecificationInBillPrint = 0;//طباعة ملاحظات الصنف فى الفاتورة المشتريات
        public static int Show_SpecificationInInvoicePrint = 0;//طباعة ملاحظات الصنف فى الفاتورة المبيعات


        public static int Divid_ItemCatygoryByType_Id = 0;  // تقسم فئة الصنف الى مبيعات ومشتريات
        public static int Set_ItemBalanceZeroWhenNegative = 1;  // تحويل ارصدة الاصناف بصفر فى حالة اقل من الصفر
        public static int bubbleBar_Visibility = 1;
        public static int UseMoreImageForItem = 0; // استخدام اكثر من صورة للصنف
        public static int EditItemBalanceFromReport = 0;   //تعديل الرصيد من تقرير رصيد الصنف
        public static int Show_TaxSale_Value_OnInvoicePrint = 0; //"ضريبة المبيعات";
        public static int Show_ATS05_Value_OnInvoicePrint = 0;  // "أرباح تجارية وصناعية";
        public static int Show_Commision_Value_OnInvoicePrint = 0;  //"الخصم";
        public static int Show_NetInvoiceTotal_OnInvoicePrint = 0; //"صافى الفاتورة";
        public static int Show_InvoiceAnotherPayment = 0; //"مصروفات اخرى  ";
        public static int VisiblecolumnBalanceAfterInOderMoney = 0; //فى اذن النقدية عرض او اخفاء عمود الرصيد التالى
        public static int Show_Bank_Name = 1; //"عرض اسم الخزينة فى الا1ن للفوزان  ";
        public static string Static_NotesInInvoicePrint = " ";
        public static string Static_NotesInInvoicePrintNotShowCustomerAccount = " ";
        public static string CharToSeparateQntyLeft = "[";
        public static string CharToSeparateQntyRight = "]";
        public static string PrintInvoice_Size = "A4"; // A4 / A4Logo / A4LogoClient_A/شكل الطباعة
        public static string PrintBill_Size = "BillInvoicePrint"; // A4 / A4Logo / A4LogoClient_A/شكل الطباعة
        public static string OrderDetailsPrint = "OrderDetails";
        public static string OrderMoneyPrint = "Order_MoneyRPT"; // شكل اذن النقدية
        public static string CommissionTextOnInvoice = "الخصم";
        public static string DefaultEntryLanguah = "eg-ar";
        public static int ItemNoVisibility = 1;
        public static int Enable_ItemNoOnInvoice = 1;  //عرض العدد فى مدخلات الفاتوره
        public static string InvoiceSettingUser1 = "CHANGE_ME";
        public static string InvoiceSettingUser2 = "CHANGE_ME";
        public static int UseSarfEstlamFromFromReservation = 0;
        public static string SystemInfo = "5000";          // Data From Xml file that equal setting name :InvoiceNo_System
        public static string InvoiceNo_System = "5000";   // Data from DB SysConfig
        public static int Demo_Verion = 0;
        public static int Demo_VerionInvoiceMaxCount = 100;
        public static bool ISVersionWillExpire_FromCode = false;
        public static int VersionWillExpire = 0;
        public static int RenewExpireCount = 3; // maximum Three Times
                                                //public static DateTime dtExpire;// = new DateTime(DateTime.Now.Date.Year, DateTime.Now.Date.Minute, DateTime.Now.Date.Day - 1);
        public static DateTime dtExpire { get; set; } = new DateTime(2026, 7, 1);
        public static int IsAdministration_AlterDBLogin = 0;
        public static int IsMake_AlterDBLogin = 1;

        public Common()
        {
            //dtExpire  = new DateTime(DateTime.Now.Date.Year, DateTime.Now.Date.Minute, DateTime.Now.Date.Day - 1);

            //
            // TODO: Add constructor logic here
            // 
        }


        public static string NameClient1 = "admin2030";
        public static string NameClient2 = "saudianew2030..";
        public static int FormHeighDiff = 130;
        public static int Using_FormHeighDiff = 1;
        //public static int FormWidthDiff { get; set; }
        public static int FormWidthDiff = 130;
        public static int Using_FormWidthDiff = 1;
       

        public static int Added_Alf_Hadfa_Value = 1;//إضافى الالف حدفة
        public static int Discount_Alf_Hadfa_Value = 2;//خصم الالف حدفه


        public static int Invoice_Type_Id = 1;
        public static int Bill_Type_Id = 2;
        public static int InvoiceReverse_Type_Id = 7;
        public static int BilleReverse_Type_Id = 8;
        public static int Bill_Suppliers_Destruction_Id = 53;
        public static int Entry_Type_Id = 100;
        public static int Taswea_Malia_Reference_Type = 20;
        public static int Taswea_Malia_By_Item_Reference_Type = 29;
        public static int Checks_Memory_Reference_Type = 5;
        public static int CarsExaminations_Reference_Type = 54;


        /// <summary>
        /// /////////////////////////////////// ثوابت الحسابات
        /// </summary>
        public static int Entry_Currency_Id = 1;
        public static string Currency_Name = "جنيه مصرى";
        public static int Supplier_Account_Type = 5;  //حساب موردين
        public static int Employee_Account_Type = 4;  //حساب موظفين
        public static int Customer_Account_Type = 2;  //حساب العملاء
        public static int Sandouk_Account_Type = 6;  //حساب الصندوق
        public static int Bank_Account_Type = 7;  //حساب البنك
        public static int ServiceProider_Doctor_Account_Type = 9;  //حساب مقدمى الخدمات - الاطباء
        public static int Customer_Patient_Indoor_Account_Type = 23;  //حساب المرضى الداخلى
        public static int Customer_Patient_Outdoor_Account_Type = 24;  //حساب المرضى الكشف الخارجى
        public static int Customer_Company_Contract = 25;  //حساب العميل كشركات متعاقد معها
        public static int Assoul_Sabta_Account_Type = 8;  ////الأصول الثايتة
        public static int ATS05_Value_Account_Type_ForSales = 18;  // حساب ارباح تجارية وصناعية مبيعات
        public static int TaxSales10_Account_Type_ForSales = 19;    //حساب ضرايب المبيعات القيمة المضافة مبيعات
        public static int Total_List3Mola_Account_Type_ForSales = 55;    //عمولة البيع - المشتريات وكالة الخضار


        public static int ATS05_Value_Account_Type_ForPurchase = 33;  // حساب ارباح تجارية وصناعية مشتريات
        public static int TaxSales10_Account_Type_ForPurchase = 34;    //حساب ضرايب المبيعات القيمة المضافة مشتريات

        public static int DAMGHAT_Account_Type = 20;       //  حساب الدمغه
        public static int CommissionValue_Account_Type = 21;  // حساب الخصم المسموح بة
        public static int CommissionMoktasab_Account_Type = 27;  // حساب الخصم المكتسب
        public static int Masrofat_Load_Payment_Purchases_Account_Type = 22;  //  حساب مصروفات التحميل مشتريات
        public static int Masrofat_Other_Selles_Account_Type = 25;  //  حساب مصروفات أخرى مبيعات
        public static int Account_Type_Taswea_Malia = 26;  //  حساب التسوية المالية للعملاء والموردين
        public static int Account_Type_KhadamatProjectBuilding = 28;  //  حساب التسوية المالية للعملاء  لخدمات المبانى
        public static int Account_Type_CurrencyDiff = 29;  //  حساب فرق العملة
                                                           //public static int Account_Type_Checks_Memory_Ward_Hafza = 30;  //  حافظة الشيكات الواردة
        public static int Account_Type_Checks_Memory_Sadra_Hafza = 31;  //  حافظة الشيكات الصادرة
        public static int Account_Type_OtherEradat = 32;  //  إيرادات أخرى
        public static int Account_Type_Checks_Memory_Ward_Hafza { get; set; } = 30;  //  حافظة الشيكات الواردة
        public static int Masrofat = 16;    //مصروفات
        public static int Eradat = 17;       //ايرادات

        public static int Account_Type_ForSales = 11;  ////إيراد المبيعات
        public static int Account_Type_ForReverseSales = 12;  ////مرتجع المبيعات
        public static int Account_Type_ForPurchase = 13;  ///مشتريات
        public static int Account_Type_ForReversePurchase = 14;  ///مشتريات  مرتجع
        public static int Account_Type_ForOther = 10;  ////Account_Type   // حسابات اخرى
        public static int Account_Type_ForCarsExaminations = 54;  ////Account_Type   // حسابات تقارير الفحص
        public static int Account_Type_ForStoreEntagTam = 56;  ////Account_Type   //  مخزن انتاج تام
        public static int Account_Type_ForStoreKhamat = 57;  ////Account_Type   //  حساب مخزن خامات
        public static int Account_Type_ForStoreNotComplete = 58;  ////Account_Type   //  حساب مخزن إنتاج غير مكتمل
                 // =========================================================
                   // الحسابات الخاصة بجدول مجموعات الأصناف (Item Groups Accounts)
                     // =========================================================

        public static int Account_Type_ForWIP = 59;                     // حساب الإنتاج تحت التشغيل (WIP)
        public static int Account_Type_ForGRIR = 60;                    // حساب بضاعة مستلمة غير مفوترة (GRIR)
        public static int Account_Type_ForCOGS = 61;                    // حساب تكلفة البضاعة المباعة (COGS)
        public static int Account_Type_ForInventoryAdjustment = 62;     // حساب تسويات وتسفية العجز والزيادة بالمخزون
        public static int Account_Type_ForPurchasePriceVariance = 63;   // حساب فروق أسعار الشراء (PPV)
        public static int Account_Type_ForOpeningBalanceEquity = 64; //(أو حساب حقوق الملكية الافتتاحي) حساب الأرصدة الافتتاحية (Opening Balance Equity)

        public static int Journal_Code_ForSandouk_Sarf = 3;
        public static int Journal_Code_ForSandouk_Estlam = 2;
        public static int Journal_Code_ForSandouk = 2;
        public static int job_list_Id_SalesMan = 100;
        public static int job_list_Id_DeliveryMan = 101;
        public static int job_list_Id_WaiterMan = 102;

        

        public static int Supplier_StoreTransactionCode { get; set; } = 1;  // فى حركة المخازن نوع موردين
        public static int Customer_StoreTransactionCode = 2;  //// فى حركة المخازن نوع   العملاء
        public static int Store_StoreTransactionCode = 4;  //// فى حركة المخازن نوع   المخازن
        
        
        public static DataTable dtConfig = new DataTable();
        //public static void SetSystemParametersDBOLD()
        //{
        //    AccountModelsLib.operations.ReadXmlDataSetConnStr();
        //    AccountModelsLib.operations.ReturnConnStr();

        //    try
        //    {

        //        AccountModelsLib.operations opp = new AccountModelsLib.operations(true);
        //        string mysql = "Select * from SysConfig ";
        //        dtConfig = opp.return_DataTable(mysql);
        //        string KeysData = "";

        //        Is_Ecrypted_Config = Is_EncryptedDataSetting(dtConfig);


        //        for (int x = 0; x < dtConfig.Rows.Count; x++)
        //        {
        //            try
        //            {
        //                KeysData = DecryptMe(dtConfig.Rows[x]["KeysData"].ToString());
        //                dtConfig.Rows[x]["Value"] = DecryptMe(dtConfig.Rows[x]["Value"].ToString());

        //                string ID = dtConfig.Rows[x]["Id"].ToString();
        //                if (ID == "113")
        //                {
        //                    int vv = 0;
        //                }
        //                if (KeysData == "InvoiceNo_System")
        //                {

        //                }
        //                SearchSystemParametersDB(KeysData, x);
        //            }
        //            catch (Exception ee)
        //            {

        //            }
        //        }
        //    }
        //    catch (Exception ee)
        //    {

        //    }



        //}

        public static void GetLoadParameter()
        {

            Common.InvoiceNo_System = Common.Get_SystemParametersDB("InvoiceNo_System");
            Common.InstituteNameEN = Common.Get_SystemParametersDB("IN_EN");
            Common.InstituteName = Common.Get_SystemParametersDB("IN");
            int Value = 0;
            int.TryParse(Common.Get_SystemParametersDB("Make_Login"), out Value);
            Common.Make_Login = Value;



        }
        public static string DecryptMe(string mData)
        {
            EncryptProcess en = new EncryptProcess();
            if (Is_Ecrypted_Config == 1)
            {
                mData = en.decrypt(mData);
            }
            return mData;

        }

        public static string EncryptMe(string mData)
        {
            EncryptProcess en = new EncryptProcess();
            if (Is_Ecrypted_Config == 1)
            {
                mData = en.encrypt(mData);
            }
            return mData;

        }
        public static void TestDBConn()
        {
            //if(DataBaseType=="5")
            //{
            //    DBConfigCLS dd = new DBConfigCLS();
            //    dd.AutoRebuildSave();
            //}

            AccountModelsLib.operations opp = new AccountModelsLib.operations(true, 0);
        }
        public static void Get_Is_Force_Admin_Permission()
        {
            bool IsHavePermission = Common.IsHavePermissionSysMenu(Common.User_Id, 336, 2, 0);
            if (IsHavePermission == true)
            {
                Is_Force_Admin_Permission = 1;
            }
            else { Is_Force_Admin_Permission = 0; }
        }
        public static string GetSystemParametersDB(string KeysData)
        {
            string Value = "";

            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            string mysql = "Select * from SysConfig Where KeysData='" + KeysData + "'";


            DataTable dt = opp.return_DataTable(mysql);
            if (dt.Rows.Count == 0)
            {
                ////tble encrypted
                Is_Ecrypted_Config = 1;
                KeysData = EncryptMe(KeysData);

                mysql = "Select * from SysConfig Where KeysData='" + KeysData + "'";
                dt = opp.return_DataTable(mysql);
                if (dt.Rows.Count > 0)
                {
                    Value = dt.Rows[0]["Value"].ToString();
                    KeysData = DecryptMe(KeysData);
                }


            }
            else
            {
                Value = dt.Rows[0]["Value"].ToString();
            }


            return Value;

        }
        public static int Is_EncryptedDataSetting(DataTable dtConfig)
        {
            int value = 0;
            if (Is_Ecrypted_Config > -1)
            { value = Is_Ecrypted_Config; }
            else
            {
                DataRow[] dr = dtConfig.Select("KeysData = 'Is_Ecrypted'");
                if (dr.Length == 0)
                {
                    value = 1;
                    Is_Ecrypted_Config = 1;
                }
                else
                {
                    value = 0;
                    Is_Ecrypted_Config = 0;
                }
            }
            return value;
        }
        public static int Is_EncryptedDataSetting(ref AccountModelsLib.operations opp)
        {
            int value = 0;
            if (Is_Ecrypted_Config > -1)
            { value = Is_Ecrypted_Config; }

            else
            {
                string mysql = "Select * from SysConfig Where KeysData='Is_Ecrypted'";


                DataTable dt = opp.return_DataTable(mysql);

                value = Is_EncryptedDataSetting(dt);
            }
            return value;
        }
        public static string Get_SystemParametersDB(string KeysData)
        {
            string Id = "0";
            string Value_ = "";
            AccountModelsLib.operations opp = new AccountModelsLib.operations(true);
            Is_Ecrypted_Config = Common.Is_EncryptedDataSetting(ref opp);

            string mysql = "";

            if (Is_Ecrypted_Config == 1)
            {
                KeysData = Common.EncryptMe(KeysData);
                mysql = "Select * from SysConfig Where KeysData=N'" + KeysData + "'";
                DataTable dt = opp.return_DataTable(mysql);
                if (dt.Rows.Count > 0)
                {
                    Value_ = dt.Rows[0]["Value"].ToString();
                    Value_ = Common.DecryptMe(Value_);
                }




                //mysql = "Select * from SysConfig";

                //DataTable dt = opp.return_DataTable(mysql);
                //for (int x = 0; x < dt.Rows.Count; x++)
                //{
                //    string K = dt.Rows[x]["KeysData"].ToString();

                //    K = Common.DecryptMe(K);
                //    Id = dt.Rows[x]["Id"].ToString();


                //    if (K == KeysData)
                //    {
                //        Value_ = dt.Rows[x]["Value"].ToString();
                //        Value_ = Common.DecryptMe(Value_);
                //        break;
                //    }
                //}
            }
            else
            {
                mysql = "Select * from SysConfig Where KeysData='" + KeysData + "'";
                DataTable dt = opp.return_DataTable(mysql);
                if (dt.Rows.Count > 0)
                {
                    Value_ = dt.Rows[0]["Value"].ToString();
                }
            }

            return Value_;
        }
        public static void Save_SystemParametersDB(string KeysData, string NewValue)
        {
            string Id = "0";
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            Is_Ecrypted_Config = Common.Is_EncryptedDataSetting(ref opp);

            string mysql = "";

            if (Is_Ecrypted_Config == 1)
            {
                mysql = "Select * from SysConfig";

                DataTable dt = opp.return_DataTable(mysql);
                for (int x = 0; x < dt.Rows.Count; x++)
                {
                    string K = dt.Rows[x]["KeysData"].ToString();
                    K = Common.DecryptMe(K);
                    if (K == KeysData)
                    {
                        Id = dt.Rows[x]["Id"].ToString();
                        NewValue = Common.EncryptMe(NewValue);
                        break;
                    }
                }
            }
            else
            {
                mysql = "Select * from SysConfig Where KeysData='" + KeysData + "'";
                DataTable dt = opp.return_DataTable(mysql);
                if (dt.Rows.Count > 0)
                {
                    Id = dt.Rows[0]["Id"].ToString();
                }
            }

            mysql = "Update SysConfig Set Value=N'" + NewValue + "' Where Id=" + Id;
            opp.insert_me(mysql);

        }
        public static bool Is_Found_SystemParametersDB(string KeysData, string NewValue)
        {
            bool found = false;
            string Id = "0";
            AccountModelsLib.operations opp = new AccountModelsLib.operations(true);
            Is_Ecrypted_Config = Common.Is_EncryptedDataSetting(ref opp);
            string mysql = "";

            if (Is_Ecrypted_Config == 1)
            {
                mysql = "Select * from SysConfig";

                DataTable dt = opp.return_DataTable(mysql);
                for (int x = 0; x < dt.Rows.Count; x++)
                {
                    string K = dt.Rows[x]["KeysData"].ToString();
                    K = Common.DecryptMe(K);
                    if (K == KeysData)
                    {
                        Id = dt.Rows[x]["Id"].ToString();
                        string Value = dt.Rows[x]["Value"].ToString();

                        string NewValue__ = Common.DecryptMe(Value);
                        if (NewValue__ == NewValue)
                        {
                            found = true;
                        }
                        break;
                    }
                }
            }
            else
            {
                mysql = "Select * from SysConfig Where KeysData='" + KeysData + "' and Value=N'" + NewValue + "'";
                DataTable dt = opp.return_DataTable(mysql);
                if (dt.Rows.Count > 0)
                {
                    found = true;
                    Id = dt.Rows[0]["Id"].ToString();
                }
            }
            return found;
            //mysql = "Update SysConfig Set Value=N'" + NewValue + "' Where Id=" + Id;
            //opp.insert_me(mysql);

        }
       

        public static void SetMy_DbContextConnStr()
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            My_DbContext.ConnStr = opp.str_sql_conn;
        }


       
        //public static void ExpirationVersion_()
        //{

        //    string Msg_ = "";
        //    string Msg = "";
        //    bool IsNotValid = false;
        //    DateTime datNow = DateTime.Now.Date;

        //    /////فى حالة عدم تطابق كود الظام فى ملف  xml  
        //    ///والقيمة فى جدول الاعدادات
        //    if (InvoiceNo_System != SystemInfo)
        //    {

        //        Msg_ = "\n";

        //        Msg_ = "\n" + "تم العبث ببيانات النظام";
        //        IsNotValid = true;
        //    }
        //    if (Common.InstituteName != Common.InstituteNameFromInfo)
        //    {
        //        Msg_ = "\n";

        //        Msg_ = "\n" + "تم العبث ببيانات النظام";
        //        IsNotValid = true;
        //    }
        //    if (Common.VersionWillExpire == 1)
        //    {
        //        if (datNow > Common.dtExpire)
        //        {
        //            IsNotValid = true;


        //        }
        //    }
        //    Msg = "\n" + "نسخة البرنامج منتهية الصلاحية";
        //    Msg = Msg + "\n" + "من فضلك يجب الإتصال بالمهندس محمد صالح";
        //    Msg = Msg + "\n" + "E-mail:Moh_saleh2010@yahoo.com";
        //    Msg = Msg + "\n" + "Mob.:+201557668366";


        //    Msg = Msg_ + Msg;

        //    if (IsNotValid == true)
        //    {
        //        MessageBox.Show (Msg);
        //        Common.CloseTheProcess_ApplicationExit();
        //    }



        //    //if (InvoiceNo_System == "500026") //الامبراطور
        //    //{

        //    //    if (datNow > new DateTime(2020,5,15))
        //    //    {
        //    //        string mysql = "ALTER LOGIN sa WITH PASSWORD = 'dddddddd';";
        //    //        AccountModelsLib.operations opp = new AccountModelsLib.operations();
        //    //        opp.insert_me("");


        //    //    }
        //    //}
        //}


       

        public static ArrayList ProjectList()
        {
            ArrayList Emp = new ArrayList();

            Emp.Add(new Employees("شئون العاملين", "1"));
            Emp.Add(new Employees("الحسابات", "2"));
            Emp.Add(new Employees("المخازن", "3"));

            Emp.Add(new Employees("الحسابات العامه", "9"));
            Emp.Add(new Employees("HIS", "10"));
            return Emp;

        }
        public static ArrayList ProjectType()
        {
            ArrayList Emp = new ArrayList();
            Emp.Add(new Employees("HIS", "10"));
            Emp.Add(new Employees("POS", "1"));
            Emp.Add(new Employees("Market", "2"));
            Emp.Add(new Employees("Factory", "3"));
            Emp.Add(new Employees("Tourism", "4"));
            Emp.Add(new Employees("AlFawzan", "5"));
            Emp.Add(new Employees("Client_A", "6"));
            Emp.Add(new Employees("Client_B", "7"));
            return Emp;

        }

        public static ArrayList OrderType()
        {
            ArrayList Emp = new ArrayList();


            Emp.Add(new Employees("إستلام من الإنتاج", "9"));
            Emp.Add(new Employees(" تحويل أصناف من مخزن لأخر", "1"));
            Emp.Add(new Employees("صرف إلى الإنتاج", "10"));
            Emp.Add(new Employees("الهالك", "2"));

            return Emp;

        }
        //public static void Customer_TypeChanged()
        //{
        //    //if (Project_Market == 1 & InstituteName == CompanyTitle_Client01)
        //    //{
        //    //    //Customer_Type = 10;
        //    //}
        //}
       


        /// <summary>
        /// if (Click No)return false 
        /// if (Click Yes)return true;
        /// </summary>
        /// <param name="message"></param>
        /// <param name="caption"></param>
        /// <returns></returns>
       
        public static bool PermissionToSaveEntryDateOutThisYear(ref string Msg, DateTime dtEntry)
        {
            //صلاحية إدخال القيود عن سنوات مالية اخري
            bool IsHavePermission = Common.IsHavePermission(Common.User_Id, 915, Common.Project_ID, Common.Branch_Id);
            if (IsHavePermission == false)
            {
                if (dtEntry.Year == DateTime.Now.Date.Year)
                {
                    return true;
                }
                else
                {
                    Msg = "تاريخ القيد غير مطابق لتاريخ العام الحالى";
                    return false;
                }
            }
            else
            {
                return true;
            }

        }

        public static int PermissionToShowTaxInvoiceOrNotORAll()
        {
            int IsInvoiceTaxInfo = -1;
            //عرض فواتير المبيعات الضريبية
            bool IsHavePermissionTax = Common.IsHavePermission(Common.User_Id, 920, Common.Project_ID, Common.Branch_Id);


            //عرض فواتير المبيعات اللغير الضريبية
            bool IsHavePermissionNotTax = Common.IsHavePermission(Common.User_Id, 921, Common.Project_ID, Common.Branch_Id);

            if (IsHavePermissionTax == true & IsHavePermissionNotTax == true)
            {
                IsInvoiceTaxInfo = -1;
            }
            if (IsHavePermissionTax == false & IsHavePermissionNotTax == false)
            {
                IsInvoiceTaxInfo = -1;
            }
            if (IsHavePermissionTax == true & IsHavePermissionNotTax == false)
            {
                IsInvoiceTaxInfo = 1;
            }
            if (IsHavePermissionTax == false & IsHavePermissionNotTax == true)
            {
                IsInvoiceTaxInfo = 0;
            }

            return IsInvoiceTaxInfo;


        }
        public static bool IsHavePermission(int User_ID, int Permission_ID, int PROJECT_ID, int Branch_Id)
        {


            if (Common.Is_User_Admin == 1)
            {
                return true;
            }

            if (PROJECT_ID == 9)
            {
                string mysql = " Select * from   PERMATIONS Where USER_ID=" + User_ID + " AND  FORM_ID=" + Permission_ID + "  And Branch_Id=" + Branch_Id + " ";
                //And PROJECT_ID=" + PROJECT_ID + "
                AccountModelsLib.operations opp = new AccountModelsLib.operations();
                DataTable dt = opp.return_DataTable(mysql);
                //DataTable dt = us.GetDataByPermissionID(User_ID, Permission_ID);

                if (dt.Rows.Count == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return IsHavePermissionSysMenu(User_ID, Permission_ID, PROJECT_ID);
            }

        }
        public static int Is_EditItemBalanceItem_Permission()
        {
            int Ok = 0;
            //صلاحية تعديل رصيد الصنف
            bool IsHavePermission = Common.IsHavePermissionSysMenu(Common.User_Id, 822, 2, -1);
            if (IsHavePermission == false)
            {
                Ok = 0;
            }
            else
            {
                Ok = 1;
            }
            return Ok;
            //صلاحية تعديل رصيد الصنف
            //bool IsHavePermission = Common.IsHavePermission(Common.User_Id, 822, 9, Common.Branch_Id);

        }
        public static bool IsHavePermissionSysMenu(int User_ID, int Menu_ID, int PROJECT_ID, int Force_Admin = 1)
        {
            //Check IF Ticket ID from My branch by User ID in this branch
            //UserPermissionDataSetTableAdapters.User_PermissionTableAdapter us = new BusinessLIB.UserPermissionDataSetTableAdapters.User_PermissionTableAdapter();
            if ((Common.Is_User_Admin == 1 & Force_Admin == 1) | Is_Force_Admin_Permission == 1)
            {
                return true;
            }
            string mysql = " Select * from   SysConfigMenusPERMATIONS Where USER_ID=" + User_ID + " AND  Menu_ID=" + Menu_ID + " And PERMATION=N'" + Common.EncryptMe("1") + "' ";
            if (PROJECT_ID > 0)
            {
                mysql = mysql + " And PROJECT_ID = " + PROJECT_ID + "";
            }
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            DataTable dt = opp.return_DataTable(mysql);
            //DataTable dt = us.GetDataByPermissionID(User_ID, Permission_ID);

            if (dt.Rows.Count == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public static string GetPermissionValue(int User_ID, int Menu_ID, int PROJECT_ID)
        {
            //Check IF Ticket ID from My branch by User ID in this branch
            //UserPermissionDataSetTableAdapters.User_PermissionTableAdapter us = new BusinessLIB.UserPermissionDataSetTableAdapters.User_PermissionTableAdapter();

            string mysql = " Select * from   SysConfigMenusPERMATIONS Where USER_ID=" + User_ID + " AND  Menu_ID=" + Menu_ID + " And PERMATION=N'" + Common.EncryptMe("1") + "' ";
            if (PROJECT_ID > 0)
            {
                mysql = mysql + " And PROJECT_ID = " + PROJECT_ID + "";
            }
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            DataTable dt = opp.return_DataTable(mysql);
            //DataTable dt = us.GetDataByPermissionID(User_ID, Permission_ID);

            if (dt.Rows.Count == 0)
            {
                return "";
            }
            else
            {

                return dt.Rows[0]["PERMATION"].ToString();
            }
        }

        public static void UpdateQntySetZero()
        {
            if (Common.Set_ItemBalanceZeroWhenNegative == 1)
            {


                AccountModelsLib.operations opp = new AccountModelsLib.operations(1);
                try
                {
                    string mysql = " ";
                    mysql = mysql + " update  Unit_item set Full_qua=0 where Full_qua<0 ";
                    //opp.insert_me(mysql);
                    mysql = mysql + " update  Unit_item set D_Qua=0 where D_Qua<0";
                    //opp.insert_me(mysql);
                    mysql = mysql + " update  Unit_item set Item_No=0 where Item_No<0 ";
                    //opp.insert_me(mysql);


                    mysql = mysql + " Update Item Set No3eboa=1 Where No3eboa=0";
                    //AccountModelsLib.operations opp = new AccountModelsLib.operations();
                    opp.insert_me(mysql);


                    opp.Commit();
                }
                catch
                {
                    opp.Rollback();
                }
            }
        }
        public static string Get_ColmnData(ref int Excp,ref string Msg, int i, DataTable dt_item, string ColumnName)
        {
            string Category_Name = "";
            try
            {
                Category_Name = dt_item.Rows[i]["" + ColumnName + ""].ToString();
                if (dt_item.Rows.Count > i)
                {
                    if (i > 0)
                    {
                        string Value_Now = dt_item.Rows[i]["" + ColumnName + ""].ToString();
                        string Value_Prev = dt_item.Rows[i - 1]["" + ColumnName + ""].ToString();

                        if (Value_Now == Value_Prev)
                        {
                            Category_Name = "";
                        }
                    }
                }
                else
                {
                    Excp = 110;
                    Msg = "Error Code CC_110";
                }
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message + "  " + "Error Code CC_110" ;
            }
            return Category_Name;
        }

        public static long GETAutoNumber(ref AccountModelsLib.operations opp, string Table_Name, string ColumnNameID, int Branch_Id)
        {
            long Submain_Account_Id = 1;
            string mysql = " Select Max(" + ColumnNameID + ")  AS SubAccount_Id from " + Table_Name + "";
            DataSet ds = opp.return_dataset(mysql);
            if (ds.Tables[0].Rows.Count > 0)
            {
                if (ds.Tables[0].Rows[0]["SubAccount_Id"].ToString() == "")
                {
                    Submain_Account_Id = 1;
                }

                else
                {
                    Submain_Account_Id = long.Parse(ds.Tables[0].Rows[0]["SubAccount_Id"].ToString()) + 1;
                }
            }
            try
            {
                string Table_Id = Branch_Id.ToString() + Submain_Account_Id.ToString();
                Submain_Account_Id = long.Parse(Table_Id);
            }
            catch { }
            return Submain_Account_Id;
        }
        public static long GETAutoNumber(ref AccountModelsLib.operations opp, string Table_Name, string ColumnNameID, string Branch_Id)
        {
            long Submain_Account_Id = 1;
            if (Common.Use_Auto_Identity_Column == 0)
            {
                Submain_Account_Id = CallSnowflakeIdGenerate.GenerateNewId();
            }
            else
            {
                string mysql = " Select Max(" + ColumnNameID + ")  AS SubAccount_Id from " + Table_Name + "";
                DataSet ds = opp.return_dataset(mysql);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    if (ds.Tables[0].Rows[0]["SubAccount_Id"].ToString() == "")
                    {
                        Submain_Account_Id = 1;
                    }

                    else
                    {
                        Submain_Account_Id = long.Parse(ds.Tables[0].Rows[0]["SubAccount_Id"].ToString()) + 1;
                    }
                }
                try
                {
                    string Table_Id = Branch_Id.ToString() + Submain_Account_Id.ToString();
                    Submain_Account_Id = long.Parse(Table_Id);
                }
                catch { }
            }
            return Submain_Account_Id;
        }
        public static void GETAutoNumber(ref AccountModelsLib.operations opp, ref long Submain_Account_Id)
        {
            if (Common.Use_Auto_Identity_Column == 0)
            {
                Submain_Account_Id = CallSnowflakeIdGenerate.GenerateNewId();
            }
            else
            {
                string mysql = " Select Max( Id)  AS Id from Submain_Account";
                DataSet ds = opp.return_dataset(mysql);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    if (ds.Tables[0].Rows[0]["Id"].ToString() == "")
                    {
                        Submain_Account_Id = 1;
                    }

                    else
                    {
                        Submain_Account_Id = long.Parse(ds.Tables[0].Rows[0]["Id"].ToString()) + 1;
                    }
                }
            }
            Submain_Account_Id = CallSnowflakeIdGenerate.GenerateNewId();
        }

         
        public static void Get_Account_Type_Code(ref AccountModelsLib.operations opp, int Account_Type_Id, int Account_Type_ValueGeneralCustomer, ref long Main_Account_Id, ref int Cost_Center_Tree_ID, long  Branch_Id)
        {
            Main_Account_Id = 0;
            Cost_Center_Tree_ID = 0;
            //string mysql = "Select * from Account_Type Where Value=" + Account_Type_Id;
            string mysql = "Select * from Account_Type_Main_Account_Id Where Account_Type_Value=" + Account_Type_Id + " And Account_Branch_Id=" + Branch_Id + "";
            if (Account_Type_ValueGeneralCustomer > 0)
            {
                mysql = mysql + " AND Account_Type_ValueGeneralCustomer = 1";
            }
            DataTable dt = opp.return_DataTable(mysql);
            if (dt.Rows.Count > 0)
            {
                Main_Account_Id = long.Parse(dt.Rows[0]["Main_Account_Id"].ToString());
                Cost_Center_Tree_ID = 0;// int.Parse(dt.Rows[0]["Cost_Center_Tree_ID"].ToString());
            }
            else
            {
                //mysql = "Select * from Submain_Account Where   Branch_ID=" + Branch_Id + " AND Main_Account_Id IS NULL Order by SubAccount_Id";
                //DataTable dt2 = opp.return_DataTable(mysql);
                //if (dt2.Rows.Count > 0)
                //{
                //    Main_Account_Id = long.Parse(dt2.Rows[0]["SubAccount_Id"].ToString());
                //}
                //else
                //{
                //    Main_Account_Id = 0;
                //}

                mysql = "SELECT Account_Customer_Type_Value, Account_Customer_Type_ID, SubAccount_Id FROM Submain_Account";
                mysql = mysql + " Where Account_Customer_Type_ID=0 AND Account_Customer_Type_Value= " + Account_Type_Id;
                if (Account_Type_ValueGeneralCustomer > 0)
                {
                    mysql = mysql + " AND Account_Type_ValueGeneralCustomer = 1";
                }
                else
                {
                    Account_Type_ValueGeneralCustomer = 0;
                }
                DataTable dt2 = opp.return_DataTable(mysql);
                if (dt2.Rows.Count > 0)
                {

                    long SubAccount_Id = long.Parse(dt2.Rows[0]["SubAccount_Id"].ToString());
                    mysql = "Insert Into Account_Type_Main_Account_Id(Account_Type_Value,Account_Branch_Id,Main_Account_Id,Account_Type_ValueGeneralCustomer) Values(" + Account_Type_Id + "," + Branch_Id + "," + SubAccount_Id + "," + Account_Type_ValueGeneralCustomer + " )";
                    opp.insert_me(mysql);
                    Main_Account_Id = SubAccount_Id;
                }
                else
                {
                    mysql = "SELECT Account_Customer_Type_Value, Account_Customer_Type_ID, SubAccount_Id FROM Submain_Account";
                    mysql = mysql + " Where Account_Customer_Type_ID=0 AND Account_Customer_Type_Value= " + Account_Type_Id;
                    DataTable dt22 = opp.return_DataTable(mysql);
                    if (dt22.Rows.Count > 0)
                    {

                        long SubAccount_Id = long.Parse(dt22.Rows[0]["SubAccount_Id"].ToString());
                        mysql = "Insert Into Account_Type_Main_Account_Id(Account_Type_Value,Account_Branch_Id,Main_Account_Id,Account_Type_ValueGeneralCustomer) Values(" + Account_Type_Id + "," + Branch_Id + "," + SubAccount_Id + "," + Account_Type_ValueGeneralCustomer + " )";
                        opp.insert_me(mysql);
                        Main_Account_Id = SubAccount_Id;
                    }
                }

            }

        }
        public static void Get_PositiveNumber(ref decimal Number_)
        {
            if (Number_ < 0)
            {
                Number_ = Number_ * -1;
            }
            Number_ = RoundMePositive(Number_);
        }
        public static decimal GetZeroIfNegative(decimal Number_)
        {
            decimal R = 0;
            if (Number_ < 0)
            {
                R = Number_ * -1;
                R = RoundMe(R);
            }
            else
            {
                R = 0;
            }
            return R;
        }
        public static decimal? Get_PositiveNumberDecimal(decimal? Number_)
        {
            if (Number_ != null)
            {
                if (Number_ < 0)
                {
                    Number_ = Number_ * -1;
                }
                Number_ = RoundMePositive(Number_.Value);
            }
            else
            {
                Number_ = 0;
            }
            return Number_;
        }
        public static void MakeAutomatic_AccontCode(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, long Submain_account_Id, ref long lastPartIncrease, ref string Account_Code, ref long Code, ref int Account_Level)
        { ///تكويد أوتوماتيكى

            try
            {
                  Excp = 0;
                Char Account_Seperator = Account_Seperator_Char;

                Account_Code = "";
                Code = 0;
                lastPartIncrease = 0;

                DataSet ds;

                long SubAccount_Count = 0;


                DataTable dt = Get_Submain_Account(ref Excp, ref opp, Submain_account_Id, ref SubAccount_Count, 2);

                if (dt.Rows.Count > 0)
                {
                    string FirstSubCode = dt.Rows[0]["FirstSubCode"].ToString();// AccountTreeViewUSCTRL.FirstSubCode;
                    string Old_Submain_account_No = dt.Rows[0]["SubAccount_NO"].ToString();// AccountTreeViewUSCTRL.Submain_account_No.ToString();
                    Account_Level = int.Parse(dt.Rows[0]["Account_Level"].ToString());
                    Account_Level = Account_Level + 1;

                    lastPartIncrease = SubAccount_Count + 1;

                    int FirstSubCodeLengh = FirstSubCode.Length;
                    int lastPartIncreaseLengh = lastPartIncrease.ToString().Length;

                    int DiffLengh = FirstSubCodeLengh - lastPartIncreaseLengh;
                    Account_Code = Old_Submain_account_No.Trim() + Account_Seperator;

                    for (int x = 0; x < DiffLengh; x++)
                    {
                        Account_Code = Account_Code + "0";
                    }

                    Account_Code = Account_Code + lastPartIncrease.ToString();

                    //if (FirstSubCode == "1")
                    //{
                    //    Account_Code = Old_Submain_account_No + Account_Seperator + lastPartIncrease.ToString();
                    //}
                    //else   ///if (FirstSubCode == "01")
                    //{
                    //    if (lastPartIncrease < 10)
                    //    {
                    //        Account_Code = Old_Submain_account_No + Account_Seperator + "0" + lastPartIncrease.ToString();
                    //    }
                    //    else
                    //    {
                    //        Account_Code = Old_Submain_account_No + Account_Seperator + lastPartIncrease.ToString();
                    //    }
                    //}


                    try
                    {
                        string Account_Code__ = Account_Code.Replace(Account_Seperator.ToString(), "");
                        Code = long.Parse(Account_Code__);
                    }
                    catch
                    { }

                }
            }
            catch (Exception ee) {
                Msg = ee.Message;  
            
            }
        }

        public static void MakeAutomatic_AccontCode(ref int Excp, ref string Msg, long Submain_account_Id, ref long lastPartIncrease, ref string Account_Code, ref long Code, ref int Account_Level)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            MakeAutomatic_AccontCode(ref   Excp, ref   Msg, ref opp, Submain_account_Id, ref lastPartIncrease, ref Account_Code, ref Code, ref Account_Level);
        }
        public static void MakeAutomatic_CostCenterCode(ref int Excp, ref string  Msg, long CostCenter_Id, ref long lastPartIncrease, ref string CostCenter_Code, ref long Code, ref int Cost_Level)
        {
            
            ///تكويد أوتوماتيكى
            ///
            try
            {
                  Excp = 0;
                Char Account_Seperator = Account_Seperator_Char;

                CostCenter_Code = "";
                Code = 0;
                lastPartIncrease = 0;

                DataSet ds;

                long SubAccount_Count = 0;

                DataTable dt = Get_CostCenter(ref Excp, CostCenter_Id, ref SubAccount_Count, 2);

                if (dt.Rows.Count > 0)
                {
                    string FirstSubCode = dt.Rows[0]["FirstSubCode"].ToString().Trim();// AccountTreeViewUSCTRL.FirstSubCode;
                    string CostCode = dt.Rows[0]["Code_Str"].ToString();// AccountTreeViewUSCTRL.Submain_account_No.ToString();
                    Cost_Level = int.Parse(dt.Rows[0]["Cost_Level"].ToString());
                    Cost_Level = Cost_Level + 1;

                    lastPartIncrease = SubAccount_Count + 1;
                    if (CostCode == "0")
                    {
                        if (FirstSubCode == "1")
                        {
                            CostCenter_Code = lastPartIncrease.ToString();
                        }
                        else   ///if (FirstSubCode == "01")
                        {



                            if (lastPartIncrease < 10)
                            {
                                CostCenter_Code = "0" + lastPartIncrease.ToString();
                            }
                            else
                            {
                                CostCenter_Code = lastPartIncrease.ToString();
                            }
                        }

                    }
                    else
                    {
                        if (FirstSubCode == "1")
                        {
                            CostCenter_Code = CostCode + Account_Seperator + lastPartIncrease.ToString();
                        }
                        else   ///if (FirstSubCode == "01")
                        {
                            if (lastPartIncrease < 10)
                            {
                                CostCenter_Code = CostCode + Account_Seperator + "0" + lastPartIncrease.ToString();
                            }
                            else
                            {
                                CostCenter_Code = CostCode + Account_Seperator + lastPartIncrease.ToString();
                            }
                        }
                    }



                    try
                    {
                        string Account_Code__ = CostCenter_Code.Replace(Account_Seperator.ToString(), "");
                        Code = long.Parse(Account_Code__);
                    }
                    catch
                    { }

                }
            }
            catch (Exception ee) { Msg = ee.Message; }
        }

        public static string GetUserName(int USER_ID, int Branch_Id)
        {
            string mysql = " Select * from USER_DETAILES where USER_ID=" + USER_ID + "";
            if (Branch_Id > 0)
            {
                mysql = mysql + " And Branch_Id=" + Branch_Id + "";
            }
            string USER_NAME = "";

            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            DataTable dt = opp.return_DataTable(mysql);
            if (dt.Rows.Count > 0)
            {
                USER_NAME = dt.Rows[0]["USER_NAME"].ToString();
            }
            return USER_NAME;
        }





        static DataTable Get_Submain_Account(ref int Excp, long SubAccount_Id, ref long SubAccount_Count, int Use_Count_Max)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            DataTable dt = Get_Submain_Account(ref Excp, ref opp, SubAccount_Id, ref SubAccount_Count, Use_Count_Max);
            return dt;

        }
        static DataTable Get_Submain_Account(ref int Excp, ref AccountModelsLib.operations opp, long SubAccount_Id, ref long SubAccount_Count, int Use_Count_Max)
        {
            DataTable dt = new DataTable();  
            try
            {
                string mysql;
                mysql = " SELECT Submain_Account.* FROM Submain_Account ";
                mysql = mysql + " WHERE (Id = " + SubAccount_Id + ")    ";


                dt = opp.return_DataTable(mysql);

                if (Use_Count_Max == 1)
                {
                    mysql = " SELECT Count(Submain_Account.Id) AS SubAccount_Id FROM Submain_Account ";
                    mysql = mysql + " WHERE (Main_Account_Id = " + SubAccount_Id + ")    ";

                    DataTable dtC = opp.return_DataTable(mysql);
                    try
                    {
                        SubAccount_Count = long.Parse(dtC.Rows[0]["SubAccount_Id"].ToString());

                    }
                    catch { }

                }
                else
                {
                    mysql = " SELECT  MAX(lastPartIncrease) AS lastPartIncrease FROM Submain_Account ";
                    mysql = mysql + " WHERE (Main_Account_Id = " + SubAccount_Id + ")    ";

                    DataTable dtC = opp.return_DataTable(mysql);
                    try
                    {
                          long.TryParse(dtC.Rows[0]["lastPartIncrease"].ToString(),out SubAccount_Count);

                    }
                    catch { SubAccount_Count = 0; }
                }
                Excp = 0;

                return dt;
            }
            catch
            {
                Excp = 1;
                return dt;
            }
        }
        static DataTable Get_CostCenter(ref int Excp, long CostCenter_Id, ref long CostCenter_Count, int Use_Count_Max)
        {
            DataTable dt = new DataTable(); ;
            try
            {
                string mysql;
                mysql = " SELECT Cost_Center_Tree.* FROM Cost_Center_Tree WHERE (Id = " + CostCenter_Id + ")    ";

                AccountModelsLib.operations opp = new AccountModelsLib.operations();
                dt = opp.return_DataTable(mysql);

                if (Use_Count_Max == 1)
                {
                    mysql = " SELECT Count(Cost_Center_Tree.CostCenter_Id) AS CostCenter_Id FROM Cost_Center_Tree ";
                    mysql = mysql + " WHERE (Parent_Id = " + CostCenter_Id + ")    ";

                    DataTable dtC = opp.return_DataTable(mysql);
                    try
                    {
                        CostCenter_Count = long.Parse(dtC.Rows[0]["CostCenter_Id"].ToString());

                    }
                    catch { }

                }
                else
                {
                    mysql = " SELECT  MAX(lastPartIncrease) AS lastPartIncrease FROM Cost_Center_Tree ";
                    mysql = mysql + " WHERE (Parent_Id = " + CostCenter_Id + ")    ";

                    DataTable dtC = opp.return_DataTable(mysql);
                    try
                    {
                        CostCenter_Count = long.Parse(dtC.Rows[0]["lastPartIncrease"].ToString());

                    }
                    catch { CostCenter_Count = 0; }
                }
                Excp = 0;
                opp = null;
                return dt;
            }
            catch
            {
                Excp = 1;
                return dt;
            }
        }


        public static decimal Get_Item_No(int i, DataTable dt_item, string Large_Qnty, string Small_Qnty, string Item_No)
        {
            int Excp = 0;
            string Msg = "";
            ///العبوة بالوحدة الكبرى

            decimal Item_NO = 0;
            try
            {
                decimal No3eboa = decimal.Parse(dt_item.Rows[i]["No3eboa"].ToString());
                if (No3eboa == 0)
                {
                    dt_item.Rows[i]["No3eboa"] = 1;
                }
            }
            catch
            {
                  Msg = "  خطأ فى قراءة العبوة من فضلك تأكد من عبوة الصنف";
                Msg = Msg + "\r\n";
                Msg = Msg + " اتصل بالدعم الفنى  ";
                dt_item.Rows[i]["No3eboa"] = 1;
                
            }
            try
            {

                decimal S_unit_Value_ = decimal.Parse(dt_item.Rows[i]["S_unit_Value"].ToString());
                if (S_unit_Value_ == 0)
                {
                    dt_item.Rows[i]["S_unit_Value"] = 1;
                }

            }
            catch
            {
                Msg = "  AAA_خطأ رقم 101";
                dt_item.Rows[i]["S_unit_Value"] = 1;
            }
            try
            {
                if (Common.Used_Item_NoAS_3ebowaInReport_Balance == 1)
                {
                    // string Qnty = Common.GetQntyString(ds_item.Tables[0].Rows[i]["Full_qua"].ToString(), ds_item.Tables[0].Rows[i]["B_Unit"].ToString(), ds_item.Tables[0].Rows[i]["D_Qua"].ToString(), ds_item.Tables[0].Rows[i]["S_unit"].ToString());

                    decimal No = decimal.Parse(dt_item.Rows[i]["" + Item_No + ""].ToString());
                    decimal D_Qua = decimal.Parse(dt_item.Rows[i]["" + Small_Qnty + ""].ToString());
                    decimal Full_qua = decimal.Parse(dt_item.Rows[i]["" + Large_Qnty + ""].ToString());
                    decimal S_unit_Value_ = decimal.Parse(dt_item.Rows[i]["S_unit_Value"].ToString());

                    decimal TotalSmallQnty = D_Qua + (Full_qua * S_unit_Value_);
                    decimal No3eboa = decimal.Parse(dt_item.Rows[i]["No3eboa"].ToString());
                    Item_NO = (TotalSmallQnty / No3eboa) / S_unit_Value_;
                    Item_NO = Common.RoundMe(Item_NO);

                }
                else
                {
                    Item_NO = decimal.Parse(dt_item.Rows[i]["" + Item_No + ""].ToString());
                    Item_NO = Common.RoundMe(Item_NO);
                }
            }
            catch { Item_NO = 0; }
            return Item_NO;
        }
        public static decimal Get_Item_No(DataRow dr, string Large_Qnty, string Small_Qnty, string Item_No)
        {

            ///العبوة بالوحدة الكبرى

            decimal Item_NO = 0;

            try
            {

                decimal No3eboa = decimal.Parse(dr["No3eboa"].ToString());
                if (No3eboa == 0)
                {
                    dr["No3eboa"] = 1;
                }
            }
            catch
            {
                dr["No3eboa"] = 1;
                //string Msg = "  خطأ فى قراءة العبوة من فضلك تأكد من عبوة الصنف";
                //Msg = Msg + "\r\n";
                //Msg = Msg + " اتصل بالدعم الفنى  ";
                //dr["No3eboa"] = 1;
                 
            }
            try
            {

                decimal S_unit_Value_ = decimal.Parse(dr["S_unit_Value"].ToString());
                if (S_unit_Value_ == 0)
                {
                    dr["S_unit_Value"] = 1;
                }
            }
            catch
            {
                dr["S_unit_Value"] = 1;
                
                //dr["S_unit_Value"] = 1;
            }
            try
            {
                if (Common.Used_Item_NoAS_3ebowaInReport_Balance == 1)
                {
                    // string Qnty = Common.GetQntyString(ds_item.Tables[0].Rows[i]["Full_qua"].ToString(), ds_item.Tables[0].Rows[i]["B_Unit"].ToString(), ds_item.Tables[0].Rows[i]["D_Qua"].ToString(), ds_item.Tables[0].Rows[i]["S_unit"].ToString());

                    decimal No = decimal.Parse(dr["" + Item_No + ""].ToString());
                    decimal D_Qua = decimal.Parse(dr["" + Small_Qnty + ""].ToString());
                    decimal Full_qua = decimal.Parse(dr["" + Large_Qnty + ""].ToString());
                    decimal S_unit_Value_ = decimal.Parse(dr["S_unit_Value"].ToString());

                    decimal TotalSmallQnty = D_Qua + (Full_qua * S_unit_Value_);
                    decimal No3eboa = decimal.Parse(dr["No3eboa"].ToString());
                    Item_NO = (TotalSmallQnty / No3eboa) / S_unit_Value_;
                    Item_NO = Common.RoundMe(Item_NO);

                }
                else
                {
                    Item_NO = decimal.Parse(dr["" + Item_No + ""].ToString());
                    Item_NO = Common.RoundMe(Item_NO);
                }
            }
            catch (Exception) { Item_NO = 0; }
            return Item_NO;
        }
        public static decimal Get_Item_No(decimal Large_Qnty, decimal Small_Qnty, decimal Item_No, decimal S_unit_Value, decimal No3eboa)
        {

            ///العبوة بالوحدة الكبرى

            decimal Item_NO = 0;

            try
            {
                if (Common.Used_Item_NoAS_3ebowaInReport_Balance == 1)
                {

                    decimal No = Item_No;
                    decimal D_Qua = Small_Qnty;
                    decimal Full_qua = Large_Qnty;
                    decimal S_unit_Value_ = S_unit_Value;

                    decimal TotalSmallQnty = D_Qua + (Full_qua * S_unit_Value_);

                    Item_NO = (TotalSmallQnty / No3eboa) / S_unit_Value_;
                    Item_NO = Common.RoundMe(Item_NO);

                }
                else
                {
                    Item_NO = Item_No;
                    Item_NO = Common.RoundMe(Item_NO);
                }
            }
            catch (Exception) { Item_NO = 0; }
            return Item_NO;
        }
        public static decimal Get_Item_Quantity(decimal Larg_Qnty, decimal Small_Qnty, decimal S_unit_Value)
        {
            if (Larg_Qnty < 0)
            { Larg_Qnty = Larg_Qnty * -1; }

            if (Small_Qnty < 0)
            { Small_Qnty = Small_Qnty * -1; }


            Small_Qnty = (Larg_Qnty * S_unit_Value) + Small_Qnty;
            decimal Quantity = Small_Qnty / S_unit_Value;

            Quantity = Common.RoundMe(Quantity);

            return Quantity;


        }
        public static decimal Get_Item_Quantity_(decimal Larg_Qnty, decimal Small_Qnty, decimal S_unit_Value)
        {
            //if (Larg_Qnty < 0)
            //{ Larg_Qnty = Larg_Qnty * -1; }

            //if (Small_Qnty < 0)
            //{ Small_Qnty = Small_Qnty * -1; }


            Small_Qnty = (Larg_Qnty * S_unit_Value) + Small_Qnty;
            decimal Quantity = Small_Qnty / S_unit_Value;

            Quantity = Common.RoundMe(Quantity);

            return Quantity;


        }

       
       
        public static string GetQntyString_(string LargQnty, string LargUnit, string SmallQnty, string SmallUnit)
        {
            //  string Qnty = lv_dv[x]["Larg_Qnty"].ToString() + " " + lv_dv[x]["B_Unit"].ToString() + " __ " + lv_dv[x]["Small_Qnty"].ToString() + " " + lv_dv[x]["S_unit"].ToString();
            //CultureInfo culture = new CultureInfo("ar-EG");
            //CultureInfo culture = new CultureInfo("en-US");

            string FirstCHAR = "(";
            string LastCHAR = ")";

            string Qnty = "";
            decimal LargQnty_ = 0;
            decimal SmallQnty_ = 0;
            try
            {
                LargQnty_ = decimal.Parse(LargQnty);
                LargQnty_ = Common.RoundMe(LargQnty_);

                decimal LargQnty_INT = Math.Truncate(LargQnty_);
                if (LargQnty_INT == LargQnty_)
                {
                    LargQnty_ = LargQnty_INT;
                }
            }
            catch { LargQnty_ = 0; }

            try
            {
                SmallQnty_ = decimal.Parse(SmallQnty);
                SmallQnty_ = Common.RoundMe(SmallQnty_);

                decimal SmallQnty_INT = Math.Truncate(SmallQnty_);
                if (SmallQnty_INT == SmallQnty_)
                {
                    SmallQnty_ = SmallQnty_INT;
                }

            }
            catch
            { SmallQnty_ = 0; }

            if (LargQnty_ > 0)
            {
                //Qnty = LargQnty.ToString() + "  " + LargUnit;
                string XX = ToIndicDigits(LargQnty_.ToString());
                Qnty = " " + XX + "  " + LargUnit;

                //Qnty = string.Format(culture, "{0}  {1}", LargQnty_.ToString(), LargUnit);


            }

            if (LargQnty_ > 0 & SmallQnty_ > 0)
            {
                Qnty = Qnty + " __ ";
                //Qnty = string.Format(culture, "{0} ـ ", Qnty);
            }
            if (SmallQnty_ > 0)
            {
                Qnty = Qnty + SmallQnty.ToString() + "  " + SmallUnit;
                //Qnty = string.Format(culture, "{0}  {1}  {2}", Qnty, SmallQnty_.ToString(), SmallUnit);

            }

            //////////////لا يوجد رصيد
            if (LargQnty_ == 0 & SmallQnty_ == 0)
            {
                Qnty = "ــ";
            }

            //Qnty = Qnty.ToString(culture);

            //Qnty= String.Format("{0: Keta   0.0 , Dasta 0.0 }", 11,8);   // "my number is 12.3"
            Qnty = ToIndicDigits(Qnty);
            return Qnty;
        }
        public static string GetQntyString1(string LargQnty, string LargUnit, string SmallQnty, string SmallUnit)
        {
            //  string Qnty = lv_dv[x]["Larg_Qnty"].ToString() + " " + lv_dv[x]["B_Unit"].ToString() + " __ " + lv_dv[x]["Small_Qnty"].ToString() + " " + lv_dv[x]["S_unit"].ToString();
            CultureInfo culture = new CultureInfo("ar-EG");
            //CultureInfo culture = new CultureInfo("en-US");

            string FirstCHAR = "(";
            string LastCHAR = ")";

            string Qnty = "";
            decimal LargQnty_ = 0;
            decimal SmallQnty_ = 0;
            try
            {
                LargQnty_ = decimal.Parse(LargQnty);
                LargQnty_ = Common.RoundMe(LargQnty_);

                decimal LargQnty_INT = Math.Truncate(LargQnty_);
                if (LargQnty_INT == LargQnty_)
                {
                    LargQnty_ = LargQnty_INT;
                }
            }
            catch { LargQnty_ = 0; }

            try
            {
                SmallQnty_ = decimal.Parse(SmallQnty);
                SmallQnty_ = Common.RoundMe(SmallQnty_);

                decimal SmallQnty_INT = Math.Truncate(SmallQnty_);
                if (SmallQnty_INT == SmallQnty_)
                {
                    SmallQnty_ = SmallQnty_INT;
                }

            }
            catch
            { SmallQnty_ = 0; }

            if (LargQnty_ > 0)
            {
                // Qnty = LargQnty.ToString() + "  " + LargUnit;
                Qnty = string.Format(culture, "{0}  {1}", LargQnty_.ToString(), LargUnit);


            }

            if (LargQnty_ > 0 & SmallQnty_ > 0)
            {
                // Qnty = Qnty + " __ ";
                Qnty = string.Format(culture, "{0} ـ ", Qnty);
            }
            if (SmallQnty_ > 0)
            {
                //   Qnty = Qnty + SmallQnty.ToString() + "  " + SmallUnit;
                Qnty = string.Format(culture, "{0}  {1}  {2}", Qnty, SmallQnty_.ToString(), SmallUnit);

            }

            //////////////لا يوجد رصيد
            if (LargQnty_ == 0 & SmallQnty_ == 0)
            {
                Qnty = "ــ";
            }

            Qnty = Qnty.ToString(culture);

            //Qnty= String.Format("{0: Keta   0.0 , Dasta 0.0 }", 11,8);   // "my number is 12.3"
            Qnty = ToIndicDigits(Qnty);
            return Qnty;
        }
        static void Fill_dtUnit()
        {
            //string mysql = "Select * from Unit Where B_Unit ='"+ LargUnit + "' S_unit ='" + SmallUnit + "'";
            string mysql = "Select * from Unit ";
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            dtUnit = opp.return_DataTable(mysql);
        }

        static decimal Get_S_Unit_ValueByUnit(ref int Excp, string LargUnit, string SmallUnit)
        {
            Excp = 0;
            decimal S_Unit_Value = 1;
            try
            {

                //string mysql = "Select * from Unit Where B_Unit ='"+ LargUnit + "' S_unit ='" + SmallUnit + "'";
                //string mysql = "Select * from Unit Where B_Unit ='" + LargUnit + "' S_unit ='" + SmallUnit + "'";
                //AccountModelsLib.operations opp = new AccountModelsLib.operations();
                //dtUnit = opp.return_DataTable(mysql);
                if (dtUnit.Rows.Count == 0 | dtUnit == null)
                {
                    Fill_dtUnit();
                }
                string Expression = "B_Unit = '" + LargUnit + "' AND  S_unit = '" + SmallUnit + "'";
                DataRow[] dr = dtUnit.Select(Expression);

                if (dr.Length > 0)
                {
                    try
                    {
                        S_Unit_Value = decimal.Parse(dr[0]["S_unit_Value"].ToString());
                    }
                    catch (Exception ee)
                    {
                        Excp = 1;
                    }
                }
            }
            catch (Exception eee)
            {
                Excp = 1;
            }

            return S_Unit_Value;
        }

        public static string GetQntyLenghts_Str(string Qnty, string WidthQnty, string LenghtQnty, string ItemNoQnty)
        {
            string Quantity = "";

            WidthQnty = Common.RoundMeStr(WidthQnty);
            LenghtQnty = Common.RoundMeStr(LenghtQnty);
            ItemNoQnty = Common.RoundMeStr(ItemNoQnty);



            //0=Qnty , 1= lenghts , 2= All إضافة الكميات والاطوال
            if (Common.Is_Show_Quantity_Lenghts_For_ItemsOnInvoice == 0)
            {
                Quantity = Qnty;
            }
            else if (Common.Is_Show_Quantity_Lenghts_For_ItemsOnInvoice == 1)
            {
                WidthQnty = WidthQnty + " * ";
                LenghtQnty = LenghtQnty + " * ";


                string LenghtsQnty = WidthQnty + LenghtQnty + ItemNoQnty;
                Quantity = LenghtsQnty;
            }
            else
            {
                Quantity = Qnty;

                if (WidthQnty == "0")
                {
                    WidthQnty = "";
                }
                else
                {
                    Quantity = Quantity + "  " + WidthQnty;
                }


                if (LenghtQnty == "0")
                {
                    LenghtQnty = "";
                }
                else
                {
                    Quantity = Quantity + " * " + LenghtQnty;
                }


                if (ItemNoQnty == "0")
                {
                    ItemNoQnty = "";
                }
                else
                {
                    Quantity = Quantity + " * " + ItemNoQnty;
                }

            }



            return Quantity;
        }
        public static decimal ComputeQuantity(decimal Height_, decimal Width_, decimal Item_No)
        {

            Width_ = Width_ / 100;
            Height_ = Height_ / 100;

            decimal Qnty = Height_ * Width_ * Item_No;

            Qnty = Common.RoundMePositive(Qnty);

            return Qnty;

        }
        static decimal S_Unit_Value = -1;
        public static string GetQntyString(decimal? Small_Unit_Value, decimal? LargQnty, string LargUnit, decimal? SmallQnty, string SmallUnit)
        {
            S_Unit_Value = Small_Unit_Value ?? 1;
            return GetQntyString(LargQnty.Value.ToString(), LargUnit, SmallQnty.Value.ToString(), SmallUnit);
        }
        public static string GetQntyString(decimal Small_Unit_Value, string LargQnty, string LargUnit, string SmallQnty, string SmallUnit)
        {
            S_Unit_Value = Small_Unit_Value;
            return GetQntyString(LargQnty, LargUnit, SmallQnty, SmallUnit);
        }
        public static string GetQntyString(string LargQnty, string LargUnit, string SmallQnty, string SmallUnit)
        {
            ///////////////////////3333
            string Qnty = "";
            int Excp = 0;


            if (Is_Unit_As_decimal == 1)
            {
                if (S_Unit_Value == -1)
                {
                    S_Unit_Value = Get_S_Unit_ValueByUnit(ref Excp, LargUnit, SmallUnit);
                }

                if (Common.Is_ShowUnitBeside_Qnty == 0)
                {
                    LargUnit = "";
                    SmallUnit = "";
                }
                if (Common.PointOfSale == 0)
                {
                    if (Excp == 0)
                    {
                        Qnty = GetQntyStringDecimalNew(S_Unit_Value, LargQnty, LargUnit, SmallQnty, SmallUnit);
                    }
                    else
                    {
                        Qnty = GetQntyStringNew(LargQnty, LargUnit, SmallQnty, SmallUnit);
                    }
                }
                else
                {
                    if (Excp == 0)
                    {
                        Qnty = GetQntyStringDecimalNewPOS(S_Unit_Value, LargQnty, LargUnit, SmallQnty, SmallUnit);
                    }
                    else
                    {
                        Qnty = GetQntyStringNewPOS(LargQnty, LargUnit, SmallQnty, SmallUnit);
                    }
                }
            }
            else
            {
                if (Common.Is_ShowUnitBeside_Qnty == 0)
                {
                    LargUnit = "";
                    SmallUnit = "";
                }
                Qnty = GetQntyStringNew(LargQnty, LargUnit, SmallQnty, SmallUnit);
            }
            S_Unit_Value = -1;
            return Qnty;
        }
        public static string GetQntyStringNew(string LargQnty, string LargUnit, string SmallQnty, string SmallUnit)
        {
            //CultureInfo culture = new CultureInfo("ar-EG");
            CultureInfo culture = new CultureInfo("en-US");
            culture = CultureInfo.CurrentCulture;
            //culture.NumberFormat.DigitSubstitution = DigitShapes.NativeNational; // Always use native characters

            string FirstCHAR = "(";
            string LastCHAR = ")";

            string Qnty = "";
            decimal LargQnty_ = 0;
            decimal SmallQnty_ = 0;
            try
            {
                LargQnty_ = decimal.Parse(LargQnty);
                LargQnty_ = Common.RoundMe(LargQnty_);

                decimal LargQnty_INT = Math.Truncate(LargQnty_);
                if (LargQnty_INT == LargQnty_)
                {
                    LargQnty_ = LargQnty_INT;
                }
            }
            catch { LargQnty_ = 0; }

            try
            {
                SmallQnty_ = decimal.Parse(SmallQnty);
                SmallQnty_ = Common.RoundMe(SmallQnty_);

                decimal SmallQnty_INT = Math.Truncate(SmallQnty_);
                if (SmallQnty_INT == SmallQnty_)
                {
                    SmallQnty_ = SmallQnty_INT;
                }

            }
            catch
            { SmallQnty_ = 0; }

            if (LargQnty_ > 0)
            {
                //var culture1 = new CultureInfo("ar-SA");
                //culture1.NumberFormat = new NumberFormatInfo();
                //Thread.CurrentThread.CurrentCulture = culture1;

                //Qnty = string.Format(culture , "({0:d} {1:d})", LargQnty_.ToString(), LargUnit);
                // Qnty = string.Format(culture, "( {0} )  {1}", LargQnty_.ToString(), LargUnit);
                //Qnty = string.Format(culture, "[ {0} ] {1}", LargQnty_.ToString(), LargUnit);
                Qnty = string.Format(culture, "{0} {1} {2} {3}", Common.CharToSeparateQntyLeft, LargQnty_.ToString(), Common.CharToSeparateQntyRight, LargUnit);
                Qnty = string.Format(culture, "{0} {1} {2} {3}", Common.CharToSeparateQntyLeft, LargUnit.ToString(), Common.CharToSeparateQntyRight, LargQnty_);

            }

            if (LargQnty_ > 0 & SmallQnty_ > 0)
            {

                Qnty = string.Format(culture, "{0:d}", Qnty);
            }
            if (SmallQnty_ > 0)
            {
                //Qnty = string.Format(culture, "{0:d} ، ({1:d} {2:d})", Qnty, SmallUnit, SmallQnty_.ToString());
                //Qnty = string.Format(culture, "{0}،[ {1} ] {2}", Qnty, SmallQnty_.ToString(), SmallUnit);
                //Qnty = string.Format(culture, "{0} {1} {2} {3}", Common.CharToSeparateQntyLeft, LargQnty_.ToString(), Common.CharToSeparateQntyRight, LargUnit);
                Qnty = string.Format(culture, "{0} {1} {2} {3} {4}", Qnty, Common.CharToSeparateQntyLeft, SmallQnty_.ToString(), Common.CharToSeparateQntyRight, SmallUnit);
            }

            //////////////لا يوجد رصيد
            if (LargQnty_ == 0 & SmallQnty_ == 0)
            {
                Qnty = "ــ";
            }

            Qnty = Qnty.ToString(culture);

            //Qnty= String.Format("{0: Keta   0.0 , Dasta 0.0 }", 11,8);   // "my number is 12.3"
            Qnty = ToIndicDigits(Qnty);
            return Qnty;
        }

        static string GetQntyStringNewPOS(string LargQnty, string LargUnit, string SmallQnty, string SmallUnit)
        {
            //CultureInfo culture = new CultureInfo("ar-EG");
            //CultureInfo culture = new CultureInfo("en-US");
            var culture = CultureInfo.CurrentCulture;
            //culture.NumberFormat.DigitSubstitution = DigitShapes.NativeNational; // Always use native characters

            string FirstCHAR = "(";
            string LastCHAR = ")";

            string Qnty = "";
            decimal LargQnty_ = 0;
            decimal SmallQnty_ = 0;
            try
            {
                LargQnty_ = decimal.Parse(LargQnty);
                LargQnty_ = Common.RoundMe(LargQnty_);

                decimal LargQnty_INT = Math.Truncate(LargQnty_);
                if (LargQnty_INT == LargQnty_)
                {
                    LargQnty_ = LargQnty_INT;
                }
            }
            catch { LargQnty_ = 0; }

            try
            {
                SmallQnty_ = decimal.Parse(SmallQnty);
                SmallQnty_ = Common.RoundMe(SmallQnty_);

                decimal SmallQnty_INT = Math.Truncate(SmallQnty_);
                if (SmallQnty_INT == SmallQnty_)
                {
                    SmallQnty_ = SmallQnty_INT;
                }

            }
            catch
            { SmallQnty_ = 0; }

            if (LargQnty_ > 0)
            {
                //var culture1 = new CultureInfo("ar-SA");
                //culture1.NumberFormat = new NumberFormatInfo();
                //Thread.CurrentThread.CurrentCulture = culture1;

                //Qnty = string.Format(culture , "({0:d} {1:d})", LargQnty_.ToString(), LargUnit);
                // Qnty = string.Format(culture, "( {0} )  {1}", LargQnty_.ToString(), LargUnit);
                //Qnty = string.Format(culture, "[ {0} ] {1}", LargQnty_.ToString(), LargUnit);
                Qnty = string.Format(culture, "{0} {1} {2} {3}", Common.CharToSeparateQntyLeft, LargQnty_.ToString(), Common.CharToSeparateQntyRight, LargUnit);

            }

            if (LargQnty_ > 0 & SmallQnty_ > 0)
            {

                Qnty = string.Format(culture, "{0:d}", Qnty);
            }
            if (SmallQnty_ > 0)
            {
                //Qnty = string.Format(culture, "{0:d} ، ({1:d} {2:d})", Qnty, SmallUnit, SmallQnty_.ToString());
                //Qnty = string.Format(culture, "{0}،[ {1} ] {2}", Qnty, SmallQnty_.ToString(), SmallUnit);
                Qnty = string.Format(culture, "{0}،{1} {2} {3} {4}", Qnty, Common.CharToSeparateQntyLeft, SmallQnty_.ToString(), Common.CharToSeparateQntyRight, SmallUnit);
            }

            //////////////لا يوجد رصيد
            if (LargQnty_ == 0 & SmallQnty_ == 0)
            {
                Qnty = "ــ";
            }

            Qnty = Qnty.ToString(culture);

            //Qnty= String.Format("{0: Keta   0.0 , Dasta 0.0 }", 11,8);   // "my number is 12.3"
            Qnty = ToIndicDigits(Qnty);
            return Qnty;
        }

        static string GetQntyStringDecimalNew(decimal S_Unit_Value, string LargQnty, string LargUnit, string SmallQnty, string SmallUnit)
        {
            var culture = CultureInfo.CurrentCulture;
            string Qnty = "";
            decimal LargQnty_ = 0;
            decimal SmallQnty_ = 0;
            try
            {
                LargQnty_ = decimal.Parse(LargQnty);

            }
            catch { LargQnty_ = 0; }

            try
            {
                SmallQnty_ = decimal.Parse(SmallQnty);

            }
            catch
            { SmallQnty_ = 0; }


            decimal MyValue = 0;
            decimal All = LargQnty_ + (SmallQnty_ / S_Unit_Value);
            All = Common.RoundMe(All);
            ////remove decimal part
            int All_ = (int)(All);
            if (All > All_)
            {
                All = Common.RoundMe(All);
            }
            else
            {
                All = Common.RoundMe(All, 0);
            }

            //Qnty = All.ToString() + " " + LargUnit;
            if (Common.Is_ShowUnitBeside_Qnty == 1)
            //if (Common.PointOfSale == 0)
            {
                //Qnty = string.Format(culture, "[ {0} ] {1}", All.ToString(), LargUnit);
                Qnty = string.Format(culture, "{0} {1} {2} {3}", Common.CharToSeparateQntyLeft, All.ToString(), Common.CharToSeparateQntyRight, LargUnit);

            }
            else
            {
                Qnty = All.ToString();
            }
            if (All == 0)
            {
                Qnty = "-";
            }
            Qnty = ToIndicDigits(Qnty);
            return Qnty;
        }
        static string GetQntyStringDecimalNewPOS(decimal S_Unit_Value, string LargQnty, string LargUnit, string SmallQnty, string SmallUnit)
        {
            string Qnty = "";
            decimal LargQnty_ = 0;
            decimal SmallQnty_ = 0;
            try
            {
                LargQnty_ = decimal.Parse(LargQnty);

            }
            catch { LargQnty_ = 0; }

            try
            {
                SmallQnty_ = decimal.Parse(SmallQnty);

            }
            catch
            { SmallQnty_ = 0; }


            decimal MyValue = 0;
            decimal All = LargQnty_ + (SmallQnty_ / S_Unit_Value);
            All = Common.RoundMe(All);
            ////remove decimal part
            int All_ = (int)(All);
            if (All > All_)
            {
                All = Common.RoundMe(All);
            }
            else
            {
                All = Common.RoundMe(All, 0);
            }

            //Qnty = All.ToString() + " " + LargUnit;


            Qnty = All.ToString();

            Qnty = ToIndicDigits(Qnty);
            return Qnty;
        }

        public static string ToIndicDigits(string input)
        {
            if (Is_ToIndicDigitsArabic == 0)
            {
                return input;
            }
            string R = input.Replace('0', '\u0660')
                    .Replace('1', '\u0661')
                    .Replace('2', '\u0662')
                    .Replace('3', '\u0663')
                    .Replace('4', '\u0664')
                    .Replace('5', '\u0665')
                    .Replace('6', '\u0666')
                    .Replace('7', '\u0667')
                    .Replace('8', '\u0668')
                    .Replace('9', '\u0669');
            return R;

        }
        public static decimal GetValuesAfterSetArabic(decimal ValueM)
        {
            decimal MyValues = ValueM;
            MyValues = RoundMe(MyValues);
            try
            {
                //MyValues = decimal.Parse(MyValues);
                string M = ToIndicDigits(ValueM.ToString());



            }
            catch { }
            return MyValues;
        }
        public static decimal GetValuesAfterSetArabic(string ValueM)
        {

            decimal MyValues = decimal.Parse(ValueM);
            MyValues = RoundMe(MyValues);
            try
            {
                string M = ToIndicDigits(MyValues.ToString());
                MyValues = decimal.Parse(M);


            }
            catch { }
            return MyValues;
        }
        public static string SetBackUpName()
        {
            return "BackUp_" + DateTime.Now.ToString("dd-MM-yyyy  HH-mm-ss");
        }
        public static void Update3ebowa()
        {

            try
            {
                string mysql = "Update Item Set No3eboa=1 Where No3eboa=0";
                AccountModelsLib.operations opp = new AccountModelsLib.operations();
                opp.insert_me(mysql);

            }
            catch (Exception ee) { }

        }
        public static decimal GetQnty3ebowa(decimal No3eboa, decimal Item_No)
        {
            decimal LargItemsQnty = 0;



            if (No3eboa > 0)
            {
                try
                {
                    LargItemsQnty = Item_No / No3eboa;
                }
                catch
                {
                    LargItemsQnty = 0;
                }
            }

            return LargItemsQnty;
        }
        public static decimal GetQnty3ebowaItemNoIsOne(decimal No3eboa, decimal Item_No)
        {
            decimal LargItemsQnty = 0;



            if (No3eboa > 0)
            {
                try
                {
                    LargItemsQnty = Item_No * No3eboa;
                    LargItemsQnty = Common.RoundMePositive(LargItemsQnty);
                }
                catch
                {
                    LargItemsQnty = 0;
                }
            }

            return LargItemsQnty;
        }
        public static decimal GetItemNoFromQnty(decimal No3eboa, decimal LargItemsQnty)
        {
            decimal Item_No = 0;



            if (No3eboa > 0)
            {
                try
                {
                    Item_No = LargItemsQnty * No3eboa;
                    Item_No = Common.RoundMePositive(Item_No);
                }
                catch
                {
                    Item_No = 0;
                }
            }

            return Item_No;
        }
        public static decimal GetItemNoFromQntyIsOne(decimal No3eboa, decimal LargItemsQnty)
        {
            decimal Item_No = 0;



            if (No3eboa > 0)
            {
                try
                {
                    Item_No = LargItemsQnty / No3eboa;
                    Item_No = Common.RoundMePositive(Item_No);
                }
                catch
                {
                    Item_No = 0;
                }
            }

            return Item_No;
        }

        public static decimal Get3ebowa(decimal LargItemsQnty, long Item_Id)
        {
            decimal No3eboa_ = 0;
            try
            {
                string mysql = "Select * from Item Where Id=" + Item_Id;
                AccountModelsLib.operations opp = new AccountModelsLib.operations();
                DataTable dt = opp.return_DataTable(mysql);
                if (dt.Rows.Count > 0)
                {
                    decimal No3eboa = 1;
                    try
                    {
                        No3eboa = decimal.Parse(dt.Rows[0]["No3eboa"].ToString());
                    }
                    catch (Exception ee)
                    {
                        mysql = "Update Item Set No3eboa=1 Where  Id=" + Item_Id;
                        opp.insert_me(mysql);

                    }
                    No3eboa_ = 1;// Get3ebowa(No3eboa, LargItemsQnty);
                }
            }
            catch (Exception ee) { }
            return No3eboa_;
        }
        public static decimal Get3ebowa(decimal No3eboa, decimal LargItemsQnty)
        {
            decimal Item_No = 0;
            decimal No3eboa_ = 0;
            if (Common.Used_Item_NoAS_3ebowaInReport_Balance == 1)
            {

                if (No3eboa > 0 & LargItemsQnty > 0)
                {
                    try
                    {

                        Item_No = LargItemsQnty / No3eboa;
                        Item_No = Common.RoundMe(Item_No, 1);
                        //double Reminder = Math.IEEERemainder(LargItemsQnty, No3eboa);
                        No3eboa_ = Item_No;
                        No3eboa_ = Common.RoundMe(No3eboa_);
                        //if (Reminder == 0)
                        //{0
                        //    txtQntyAnother.Text = Item_No.ToString();
                        //}
                        //else
                        //{
                        //    txtQntyAnother.Text = "0";
                        //}
                    }
                    catch
                    {
                        No3eboa_ = 0;
                    }
                }
            }
            return No3eboa_;
        }
        public static decimal Get3ebowa(decimal No3eboa, decimal LargItemsQnty, decimal SmallItemsQnty)
        {
            decimal Item_No = 0;
            decimal No3eboa_ = 0;
            if (Common.Used_Item_NoAS_3ebowaInReport_Balance == 1)
            {

                if (No3eboa > 0 & LargItemsQnty > 0)
                {
                    try
                    {

                        Item_No = LargItemsQnty / No3eboa;
                        Item_No = Common.RoundMe(Item_No, 1);
                        //double Reminder = Math.IEEERemainder(LargItemsQnty, No3eboa);
                        No3eboa_ = Item_No;
                        No3eboa_ = Common.RoundMe(No3eboa_);
                        //if (Reminder == 0)
                        //{0
                        //    txtQntyAnother.Text = Item_No.ToString();
                        //}
                        //else
                        //{
                        //    txtQntyAnother.Text = "0";
                        //}
                    }
                    catch
                    {
                        No3eboa_ = 0;
                    }
                }
            }
            return No3eboa_;
        }

        public static void Update_UnitItem()
        {
            if (Common.Compute_QntyFrom_ItemNo == 1)
            {
                AccountModelsLib.operations opp = new AccountModelsLib.operations();
                string mysql = @"	UPDATE Unit_item SET Full_qua = Unit_item.Item_No * Item.Width_ * Item.Height_ / 10000, D_Qua = 0
                    FROM Unit_item INNER JOIN
                         Item ON Unit_item.Item_id = Item.id";
                opp.insert_me(mysql);
            }
        }
        public static void Set_UnitItem_BalanceDateWhenSystemClosed(ref int Excp, DateTime dtDay)
        {
            string mysql = "";
            Common.Update_UnitItem();
            AccountModelsLib.operations opp = new AccountModelsLib.operations(1);

            try
            {

                //mysql = "Delete from Unit_Item_Balance_Date_Details Where Balance_Date=" + AccountModelsLib.operations.Get_DateTimeToInsert(dtDay) + "";
                //opp.insert_me(mysql);
                //mysql = @"INSERT INTO Unit_Item_Balance_Date_Details(Unit_Item_Id, Item_Id, Balance_Date, Full_qua, D_Qua, Item_No, P_price, S_price,Store_Id,Unit_Id) ";
                //mysql = mysql + " SELECT id, Item_id," + AccountModelsLib.operations.Get_DateTimeToInsert(dtDay) + " AS Balance_Date, Full_qua, D_Qua, Item_No, P_price, S_price ,Store_Id,Unit_Id FROM  Unit_item ";
                //opp.insert_me(mysql);


                opp.insert_me("Delete from Unit_Item_Balance_Date_Details Where Balance_Date=" + AccountModelsLib.operations.Get_DateTimeToInsert(dtDay));
                opp.insert_me("INSERT INTO Unit_Item_Balance_Date_Details(Unit_Item_Id, Item_Id, Balance_Date, Large_Balance, Small_Balance, Item_No, P_price, S_price,Store_Id,Unit_Id) " + " SELECT id, Item_id," + AccountModelsLib.operations.Get_DateTimeToInsert(dtDay) + " AS Balance_Date, Full_qua, D_Qua, Item_No, P_price, S_price ,Store_Id,Unit_Id FROM  Unit_item ");



                opp.Commit();
                Excp = 0;
            }
            catch (Exception ee)
            {
                Excp = 1;
                opp.Rollback();

            }

        }
        public void Set_Item_No(ref AccountModelsLib.operations opp)
        {


            //if (Common.Used_Item_NoAS_3ebowaInReport_Balance == 1 | Common.Compute_QntyFrom_Only_ItemNo == 1 | Common.Compute_QntyFrom_Only_ItemNoIsOne == 1)
            //{
            //    string mysql = "Select * from Unit_Item";
            //    DataTable dt = opp.return_DataTable(mysql);

            //    decimal No3eboa = decimal.Parse(ds_item.Tables[0].Rows[i]["No3eboa"].ToString());
            //    decimal Full_qua = decimal.Parse(ds_item.Tables[0].Rows[i]["Full_qua"].ToString());
            //    decimal D_Qua = decimal.Parse(ds_item.Tables[0].Rows[i]["D_Qua"].ToString());
            //    decimal Small_unit = decimal.Parse(ds_item.Tables[0].Rows[i]["Small_unit"].ToString());

            //    Full_qua = Full_qua + (D_Qua / Small_unit);
            //    //Full_qua = Full_qua / Small_unit;
            //    No = 0;
            //    try
            //    {
            //        if (Common.Used_Item_NoAS_3ebowaInReport_Balance == 1)
            //        {
            //            No = Full_qua / No3eboa;
            //        }
            //        else if (Common.Compute_QntyFrom_Only_ItemNo == 1)
            //        {
            //            No = Full_qua * No3eboa;
            //        }
            //        else if (Common.Compute_QntyFrom_Only_ItemNoIsOne == 1)
            //        {
            //            No = Full_qua / No3eboa;
            //        }
            //        else
            //        {
            //            No = Full_qua / No3eboa;
            //        }

            //        No = Common.RoundMe(No);
            //    }
            //    catch { }

            //}

        }
        public static string GetCustomerName(long Customer_Id)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            string CustomerName = GetCustomerName(ref opp, Customer_Id);

            return CustomerName;
        }
        public static string GetCustomerName(ref AccountModelsLib.operations opp, long Customer_Id)
        {
            string CustomerName = "";

            try
            {
                string mysql = "Select * from Customer Where Id=" + Customer_Id;

                DataTable dt = opp.return_DataTable(mysql);
                if (dt.Rows.Count > 0)
                {
                    decimal No3eboa = 1;
                    try
                    {
                        CustomerName = dt.Rows[0]["Name"].ToString();
                    }
                    catch (Exception ee)
                    {


                    }

                }
            }
            catch (Exception ee) { }
            return CustomerName;
        }
        public static string GetSupplierName(long Supplier_Id)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            string SupplierName = GetSupplierName(ref opp, Supplier_Id);

            return SupplierName;
        }
        public static string GetSupplierName(ref AccountModelsLib.operations opp, long Supplier_Id)
        {
            string SupplierName = "";
            try
            {
                string mysql = "Select * from Supplier Where Id=" + Supplier_Id;

                DataTable dt = opp.return_DataTable(mysql);
                if (dt.Rows.Count > 0)
                {
                    decimal No3eboa = 1;
                    try
                    {
                        SupplierName = dt.Rows[0]["Name"].ToString();
                    }
                    catch (Exception ee)
                    {


                    }

                }
            }
            catch (Exception ee) { }
            return SupplierName;
        }

        public static string GetStoreName(long Store_Id)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            string StoreName = GetStoreName(ref opp, Store_Id);

            return StoreName;
        }
        public static string GetStoreName(ref AccountModelsLib.operations opp, long Store_Id)
        {
            string StoreName = "";
            try
            {
                string mysql = "Select * from Store  Where Id=" + Store_Id;

                DataTable dt = opp.return_DataTable(mysql);
                if (dt.Rows.Count > 0)
                {
                    decimal No3eboa = 1;
                    try
                    {
                        StoreName = dt.Rows[0]["Store_Name"].ToString();
                    }
                    catch (Exception ee)
                    {


                    }

                }
            }
            catch (Exception ee) { }
            return StoreName;
        }
        public static long GetTable_Id(string Table_Name, string column_AutoNumeric)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            DataSet ds = null;
            long ID_Value = 0;
            long AutoNumericValue = 0;
            string New_Id_ = "";
            string mysql = "";
            if (Common.Branch_Id >= 0)
            {
                mysql = " SELECT MAX(" + column_AutoNumeric + ") AS New_Id FROM " + Table_Name + " Where Branch_Id=" + Common.Branch_Id;
            }
            else
            {
                mysql = " SELECT MAX(" + column_AutoNumeric + ") AS New_Id FROM " + Table_Name;
            }
            int Excp = 0;
            ds = opp.return_dataset(mysql, Table_Name);
            if (ds.Tables[0].Rows.Count > 0)
            {
                if (ds.Tables[0].Rows[0]["New_Id"].ToString() == "")
                {
                    AutoNumericValue = 1;
                }
                else
                {
                    AutoNumericValue = long.Parse(ds.Tables[0].Rows[0]["New_Id"].ToString()) + 1;
                }
            }
            else
            {
                AutoNumericValue = 1;
            }

            Excp = 0;
            New_Id_ = Common.Branch_Id.ToString() + AutoNumericValue.ToString();
            ID_Value = long.Parse(New_Id_.ToString());
            return ID_Value;
        }
        public static long GetTable_Id(ref AccountModelsLib.operations opp, string Table_Name, string column_AutoNumeric)
        {

            DataSet ds = null;
            long ID_Value = 0;
            long AutoNumericValue = 0;
            string New_Id_ = "";
            string mysql = "";
            if (Common.Branch_Id >= 0)
            {
                mysql = " SELECT MAX(" + column_AutoNumeric + ") AS New_Id FROM " + Table_Name + " Where Branch_Id=" + Common.Branch_Id;
            }
            else
            {
                mysql = " SELECT MAX(" + column_AutoNumeric + ") AS New_Id FROM " + Table_Name;
            }
            int Excp = 0;
            ds = opp.return_dataset(mysql, Table_Name);
            if (ds.Tables[0].Rows.Count > 0)
            {
                if (ds.Tables[0].Rows[0]["New_Id"].ToString() == "")
                {
                    AutoNumericValue = 1;
                }
                else
                {
                    AutoNumericValue = long.Parse(ds.Tables[0].Rows[0]["New_Id"].ToString()) + 1;
                }
            }
            else
            {
                AutoNumericValue = 1;
            }

            Excp = 0;
            New_Id_ = Common.Branch_Id.ToString() + AutoNumericValue.ToString();
            ID_Value = long.Parse(New_Id_.ToString());
            return ID_Value;
        }
        public static long GetTable_Id_NoBranch(string Table_Name, string column_AutoNumeric)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            DataSet ds = null;

            long AutoNumericValue = 0;

            string mysql = "";

            mysql = " SELECT MAX(" + column_AutoNumeric + ") AS New_Id FROM " + Table_Name;

            int Excp = 0;
            ds = opp.return_dataset(mysql, Table_Name);
            if (ds.Tables[0].Rows.Count > 0)
            {
                if (ds.Tables[0].Rows[0]["New_Id"].ToString() == "")
                {
                    AutoNumericValue = 1;
                }
                else
                {
                    AutoNumericValue = long.Parse(ds.Tables[0].Rows[0]["New_Id"].ToString()) + 1;
                }
            }
            else
            {
                AutoNumericValue = 1;
            }

            Excp = 0;

            return AutoNumericValue;
        }
        public static bool IsNumeric(string s)
        {
            bool result = false;
            try
            {

                long i = 0;
                result = long.TryParse(s, out i);

            }
            catch (Exception)
            {

            }
            return result;
        }
        public static decimal GetColumnValueDecimal(DataTable dt, string ColumnName, int index)
        {
            decimal VV = 0;

            try
            {
                VV = decimal.Parse(dt.Rows[index][ColumnName].ToString());
            }
            catch
            {

            }
            return VV;

        }
        public static decimal GetColumnValueDecimalRounded(string value)
        {
            decimal VV = 0;

            try
            {
                VV = decimal.Parse(value);
            }
            catch
            {

            }
            VV = RoundMe(VV);
            return VV;

        }
        public static decimal GetColumnValueDecimal(DataRow dr, string ColumnName)
        {
            decimal VV = 0;

            try
            {

                VV = decimal.Parse(dr[ColumnName].ToString());
            }
            catch
            {

            }
            return VV;

        }
        public static long GetColumnValueLong(DataRow dr, string ColumnName)
        {
            long VV = 0;

            try
            {

                VV = long.Parse(dr[ColumnName].ToString());
            }
            catch
            {

            }
            return VV;

        }
        public static long GetColumnValueLong(DataTable dt, string ColumnName, int index)
        {
            long VV = 0;

            try
            {
                VV = long.Parse(dt.Rows[index][ColumnName].ToString());
            }
            catch
            {

            }
            return VV;

        }
        public static DateTime GetColumnValueDateTime(DataTable dt, string ColumnName, int index)
        {
            DateTime VV = new DateTime();

            try
            {
                VV = DateTime.Parse(dt.Rows[index][ColumnName].ToString());
            }
            catch
            {

            }
            return VV;

        }
        public static DateTime? GetColumnValueDateTimeNullable(DataTable dt, string ColumnName, int index)
        {
            object sqlDateTime = dt.Rows[index][ColumnName];
            DateTime? dat = (sqlDateTime == System.DBNull.Value)
                ? (DateTime?)null
                : Convert.ToDateTime(sqlDateTime);



            return dat;

        }

        public static DateTime? GetColumnValueDateTimeNullable(object sqlDateTime)
        {
            // object sqlDateTime = dt.Rows[index][ColumnName].ToString();
            DateTime? dat = (sqlDateTime == System.DBNull.Value | sqlDateTime == null)
                ? (DateTime?)null
                : Convert.ToDateTime(sqlDateTime);



            return dat;

        }
        public static long Make_Suppliers_AccountsProcessNo(ref int Excp, ref AccountModelsLib.operations opp)
        {
            DataTable dt = null;
            try
            {
                long ProcessNo;
                string mysql = "  SELECT MAX(ProcessNo) AS ProcessNo FROM Suppliers_Accounts  ";

                dt = opp.return_DataTable(mysql);
                if (dt.Rows.Count > 0)
                {
                    if (dt.Rows[0]["ProcessNo"] == null)
                    {

                        ProcessNo = 1;
                    }
                    else
                    {
                        ProcessNo = long.Parse(dt.Rows[0]["ProcessNo"].ToString());
                        ProcessNo = ProcessNo + 1;
                    }

                }
                else
                {
                    ProcessNo = 1;
                }

                Excp = 0;
                return ProcessNo;

            }
            catch (Exception e)
            {
                Excp = 1;
                throw (e);

            }
        }
        public static long Make_Customers_AccountsProcessNo(ref int Excp, ref AccountModelsLib.operations opp)
        {
            DataTable dt = null;
            try
            {
                long ProcessNo;
                string mysql = "  SELECT MAX(ProcessNo) AS ProcessNo FROM Customers_Accounts  ";

                dt = opp.return_DataTable(mysql);
                if (dt.Rows.Count > 0)
                {
                    if (dt.Rows[0]["ProcessNo"] == null)
                    {

                        ProcessNo = 1;
                    }
                    else
                    {
                        ProcessNo = long.Parse(dt.Rows[0]["ProcessNo"].ToString());
                        ProcessNo = ProcessNo + 1;
                    }

                }
                else
                {
                    ProcessNo = 1;
                }

                Excp = 0;
                return ProcessNo;

            }
            catch (Exception e)
            {
                Excp = 1;
                throw (e);

            }
        }
        public static long Make_AccountsProcessNo(ref int Excp, ref AccountModelsLib.operations opp)
        {
            DataTable dt = null;
            try
            {
                long ProcessNo = 0;
                long ProcessNo1 = 0;
                long ProcessNo2 = 0;
                string mysql = "";
                mysql = @"  SELECT MAX(ProcessNo) AS ProcessNo1 FROM Suppliers_Accounts ";
                dt = opp.return_DataTable(mysql);
                if (dt.Rows.Count > 0)
                {
                    try
                    {
                        if (dt.Rows[0]["ProcessNo1"].ToString() != "")
                        {
                            ProcessNo1 = long.Parse(dt.Rows[0]["ProcessNo1"].ToString());
                        }else
                        {
                            ProcessNo1 = 0;
                        }
                    }
                    catch (Exception)
                    {
                        ProcessNo1 = 0;
                    }



                }

                mysql = @"  SELECT MAX(ProcessNo) AS ProcessNo2 FROM Customers_Accounts ";
                dt = opp.return_DataTable(mysql);
                if (dt.Rows.Count > 0)
                {

                    try
                    {
                        ProcessNo2 = long.Parse(dt.Rows[0]["ProcessNo2"].ToString());
                    }
                    catch (Exception)
                    {
                        ProcessNo2 = 0;
                    }
                }

                ProcessNo = ProcessNo1 > ProcessNo2 ? ProcessNo1 : ProcessNo2;

                ProcessNo = ProcessNo + 1;
                Excp = 0;
                return ProcessNo;

            }
            catch (Exception e)
            {
                Excp = 1;
                throw (e);

            }
        }

        public static long Make_AccountsProcessNo(ref int Excp, ref AccountModelsLib.operations opp, string TableName)
        {
            DataTable dt = null;
            try
            {
                long ProcessNo = 0;
                long ProcessNo1 = 0;
                long ProcessNo2 = 0;
                long ProcessNo3 = 0;

                string mysql = "";

                mysql = @"  SELECT MAX(ProcessNo) AS ProcessNo1 FROM DataDictionaryProcessesNo  WITH (NOLOCK) ";
                dt = opp.return_DataTable(mysql);
                if (dt.Rows.Count > 0)
                {
                    try
                    {
                        ProcessNo3 = long.Parse(dt.Rows[0]["ProcessNo1"].ToString());
                    }
                    catch (Exception)
                    {
                        ProcessNo3 = 1;
                    }


                    mysql = @"  INSERT INTO DataDictionaryProcessesNo (ProcessNo, TableName) VALUES(" + ProcessNo3 + ",N'" + TableName.Trim() + "')";
                    opp.insert_me(mysql);
                }


                mysql = @"  SELECT MAX(ProcessNo) AS ProcessNo1 FROM Suppliers_Accounts ";
                dt = opp.return_DataTable(mysql);
                if (dt.Rows.Count > 0)
                {
                    try
                    {
                        if (dt.Rows[0]["ProcessNo1"].ToString() != "")
                        {
                            ProcessNo1 = long.Parse(dt.Rows[0]["ProcessNo1"].ToString());
                        }
                        else
                        {
                            ProcessNo1 = 0;
                        }
                    }
                    catch (Exception)
                    {
                        ProcessNo1 = 0;
                    }



                }


                mysql = @"  SELECT MAX(ProcessNo) AS ProcessNo2 FROM Customers_Accounts ";
                dt = opp.return_DataTable(mysql);
                if (dt.Rows.Count > 0)
                {

                    try
                    {
                        ProcessNo2 = long.Parse(dt.Rows[0]["ProcessNo2"].ToString());
                    }
                    catch (Exception)
                    {
                        ProcessNo2 = 0;
                    }
                }
                ProcessNo = Math.Max(ProcessNo1, Math.Max(ProcessNo2, ProcessNo3));

                //ProcessNo = ProcessNo1 > ProcessNo2 ? ProcessNo1 : ProcessNo2;

                ProcessNo = ProcessNo + 1;
                Excp = 0;
                return ProcessNo;

            }
            catch (Exception e)
            {
                Excp = 1;
                throw (e);

            }
        }

        public static void Make_TableColumnSerialME(ref DataTable dt, string Serial_ColmnName)
        {
            try
            {

                int S = 0;
                for (int x = 0; x < dt.Rows.Count; x++)
                {
                    S = x + 1;
                    dt.Rows[x]["Serial_"] = S.ToString();
                }
            }
            catch (Exception ee)
            {
            }
        }
        public static void Make_TableColumnSerialME(ref DataTable dt)
        {
            try
            {
                if(!dt.Columns.Contains("Serial_"))
                {
                    dt.Columns.Add("Serial_");
                }
                int S = 0;
                for (int x = 0; x < dt.Rows.Count; x++)
                {
                    S = x + 1;
                    dt.Rows[x]["Serial_"] = S.ToString();
                }
            }
            catch (Exception ee)
            {
            }
        }
        public static void Make_TableColumnSerial(ref DataTable dt, string Serial_ColmnName)
        {
            try
            {

                DataColumn Serial_ = new DataColumn(Serial_ColmnName);
                Serial_.DataType = System.Type.GetType("System.Int32");
                if (!dt.Columns.Contains(Serial_ColmnName))
                {
                    dt.Columns.Add(Serial_);
                }
            }
            catch (Exception ee)
            {
            }
            try
            {

                for (int x = 0; x < dt.Rows.Count; x++)
                {
                    dt.Rows[x][Serial_ColmnName] = x + 1;
                }

            }
            catch (Exception ee)
            {
            }


        }
        public static void Make_TableColumnSerial(ref DataSet ds, string Serial_ColmnName)
        {
            try
            {

                DataColumn Serial_ = new DataColumn(Serial_ColmnName);
                Serial_.DataType = System.Type.GetType("System.Int32");
                if (!ds.Tables[0].Columns.Contains(Serial_ColmnName))
                {  
                ds.Tables[0].Columns.Add(Serial_);
            }


            }
            catch (Exception ee)
            {
            }

            try
            {
                for (int x = 0; x < ds.Tables[0].Rows.Count; x++)
                {
                    ds.Tables[0].Rows[x]["Serial_"] = x + 1;
                }
            }
            catch (Exception)
            {


            }
        }

        public static void Insert_FORMS_DETAILES(ref long FORM_ID, long Parent_Id, string FORM_NAME, int PROJECT_ID, string Form_Tag, int IS_MainMenu, string Form_NameID)
        {

            try
            {
                string mysql = "INSERT INTO FORMS_DETAILES(Parent_Id, FORM_NAME, PROJECT_ID, Form_Tag, IS_MainMenu,Form_NameID) ";
                mysql = mysql + " VALUES ( " + Parent_Id + ",'" + FORM_NAME + "'," + PROJECT_ID + ",'" + Form_Tag + "'," + IS_MainMenu + ",'" + Form_NameID + "')";
                AccountModelsLib.operations opp = new AccountModelsLib.operations();
                FORM_ID = opp.Insert_me_Return_Last_Identify(mysql);
            }
            catch { }
        }
        public static void GetTable_Id(ref AccountModelsLib.operations opp, string Table_Name, string Column_AutoNumeric, ref long AutoNumericValue, ref long Serial)
        {



            DataTable dt;
            string New_Id_ = "";
            string mysql = "";
            if (Common.Branch_Id >= 0)
            {
                mysql = " SELECT MAX(" + Column_AutoNumeric + ") AS New_Id FROM " + Table_Name + " GROUP BY Branch_Id HAVING Branch_Id=" + Common.Branch_Id;
            }
            else
            {
                mysql = " SELECT MAX(" + Column_AutoNumeric + ") AS New_Id FROM " + Table_Name + " ";
            }
            int Excp = 0;
            dt = opp.return_DataTable(mysql);
            if (dt.Rows.Count > 0)
            {
                if (dt.Rows[0]["New_Id"] == null)
                {
                    AutoNumericValue = 1;
                }
                else
                {
                    try
                    {
                        AutoNumericValue = long.Parse(dt.Rows[0]["New_Id"].ToString()) + 1;
                    }
                    catch { AutoNumericValue = 1; }
                }
            }
            else
            {
                AutoNumericValue = 1;
            }

            Excp = 0;
            New_Id_ = Common.Branch_Id.ToString() + AutoNumericValue.ToString();
            Serial = long.Parse(New_Id_.ToString());

        }
        public static DataTable Get_TotalBalanceByTotal_(ref DataTable dt, string CoumnLarg_Qnty, string CoumnSmall_Qnty, string CoumnItem_No, string CoumnBig_Unit, string CoumnSmall_Unit, string CoumnS_unit_Value)
        {
            string balanceStr = "";

            DataView view = new DataView(dt);
            DataTable dt_Stores = view.ToTable(true, CoumnBig_Unit);


            DataTable dtT = new DataTable();
            DataColumn Larg_Qnty_ = new DataColumn("Larg_Qnty", Type.GetType("System.String"));
            dtT.Columns.Add(Larg_Qnty_);

            DataColumn Small_Qnty_ = new DataColumn("Small_Qnty", Type.GetType("System.String"));
            dtT.Columns.Add(Small_Qnty_);
            DataColumn Item_No = new DataColumn("Item_No", Type.GetType("System.String"));
            dtT.Columns.Add(Item_No);

            DataColumn Big_Unit_ = new DataColumn("Big_Unit", Type.GetType("System.String"));
            dtT.Columns.Add(Big_Unit_);

            DataColumn Small_Unit_ = new DataColumn("Small_Unit", Type.GetType("System.String"));
            dtT.Columns.Add(Small_Unit_);
            DataColumn balanceStr_ = new DataColumn("balanceStr", Type.GetType("System.String"));
            dtT.Columns.Add(balanceStr_);


            //////////////////////////
            DataColumn balanceStr_1 = new DataColumn("balanceStr", Type.GetType("System.String"));
            dt.Columns.Add(balanceStr_1);
            DataColumn Item_No_ = new DataColumn("Item_No_", Type.GetType("System.String"));
            dt.Columns.Add(Item_No_);

            for (int x = 0; x < dt_Stores.Rows.Count; x++)
            {
                string Big_Unit = dt_Stores.Rows[x][CoumnBig_Unit].ToString();

                decimal T_Larg_Qnty = 0;
                decimal T_Small_Qnty = 0;
                decimal T_Item_No = 0;
                decimal S_unit_Value = 1;
                //string Big_Unit = "";
                string Small_Unit = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    string S_Big_Unit = dt.Rows[i][CoumnBig_Unit].ToString();

                    if (S_Big_Unit != Big_Unit)
                    {
                        continue;
                    }

                    S_unit_Value = decimal.Parse(dt.Rows[i][CoumnS_unit_Value].ToString());

                    Small_Unit = dt.Rows[i][CoumnSmall_Unit].ToString();
                    try
                    {
                        T_Larg_Qnty = T_Larg_Qnty + decimal.Parse(dt.Rows[i][CoumnLarg_Qnty].ToString());
                    }
                    catch { }
                    try
                    {
                        T_Small_Qnty = T_Small_Qnty + decimal.Parse(dt.Rows[i][CoumnSmall_Qnty].ToString());
                    }
                    catch { }
                    try
                    {
                        T_Item_No = T_Item_No + decimal.Parse(dt.Rows[i][CoumnItem_No].ToString());
                    }
                    catch { }
                    try
                    {
                        balanceStr = Common.GetQntyString(dt.Rows[i][CoumnLarg_Qnty].ToString(), Big_Unit, dt.Rows[i][CoumnSmall_Qnty].ToString(), Small_Unit);
                        //Qnty = Common.GetQntyLenghts_Str(Qnty, dt.Rows[x]["QntyWidth"].ToString(), dt.Rows[x]["QntyLenght"].ToString(), dt.Rows[x]["QntyItemNo"].ToString());

                        dt.Rows[x]["balanceStr"] = balanceStr;
                    }
                    catch { }
                    try
                    {
                        dt.Rows[x]["Item_No_"] = dt.Rows[i][CoumnItem_No].ToString();
                    }
                    catch { }

                    //Big_Unit = dt_Stores.Rows[x][CoumnBig_Unit].ToString();

                }
                Cls_Conversion_Unit.ConVert_By_S_UnitValue(ref T_Larg_Qnty, ref T_Small_Qnty, S_unit_Value);
                balanceStr = Common.GetQntyString(T_Larg_Qnty.ToString(), Big_Unit, T_Small_Qnty.ToString(), Small_Unit);
                DataRow dr = dtT.NewRow();
                dr["Larg_Qnty"] = T_Larg_Qnty.ToString();
                dr["Small_Qnty"] = T_Small_Qnty.ToString();
                dr["Item_No"] = T_Item_No.ToString();
                dr["Big_Unit"] = Big_Unit;
                dr["Small_Unit"] = Small_Unit;
                dr["balanceStr"] = balanceStr;
                dtT.Rows.Add(dr);
            }
            return dtT;
        }
        static ArrayList ArrExcludeIndex = new ArrayList();
        public static DataTable Get_TotalBalanceByTotal(DataTable dt, ArrayList ArrExcludeIndex_, string CoumnLarg_Qnty, string CoumnSmall_Qnty, string CoumnItem_No, string CoumnBig_Unit, string CoumnSmall_Unit, string CoumnS_unit_Value)
        {
            ArrExcludeIndex = new ArrayList();
            ArrExcludeIndex = ArrExcludeIndex_;
            return Get_TotalBalanceByTotal(dt, CoumnLarg_Qnty, CoumnSmall_Qnty, CoumnItem_No, CoumnBig_Unit, CoumnSmall_Unit, CoumnS_unit_Value);
        }
        public static void Get_Tax_Rate()
        {
            TaxListCLS tcls = new TaxListCLS();
            TaxSales_Rate = tcls.Get_TaxInfo(1);
            ATS05_Sales_Rate = tcls.Get_TaxInfo(2);
        }
        public static bool Get_TrueOrFalse(int? value)
        {
            bool IsTrue = false;
            if (value != null)
            {
                if (value == 1)
                {
                    IsTrue = true;
                }
            }
            return IsTrue;
        }
        public static bool Get_TrueOrFalse(int value)
        {
            bool IsTrue = false;
            if (value != null)
            {
                if (value == 1)
                {
                    IsTrue = true;
                }
            }
            return IsTrue;
        }
        public static int Get_TrueOrFalseValue(bool value)
        {
            int IsTrue = 0;
            if (value != null)
            {
                if (value == true)
                {
                    IsTrue = 1;
                }
            }
            return IsTrue;
        }
        public static int Get_TrueOrFalseValueRevere(bool value)
        {
            int IsTrue = 1;
            if (value != null)
            {
                if (value == true)
                {
                    IsTrue = 0;
                }
            }
            return IsTrue;
        }
        public static int Get_TrueOrFalseValue(bool? value)
        {
            int IsTrue = 0;
            if (value != null)
            {
                if (value == true)
                {
                    IsTrue = 1;
                }
            }
            return IsTrue;
        }
        public static int Get_TrueOrFalseValueMinusOne(bool value)
        {
            int IsTrue = -1;

            if (value == true)
            {
                IsTrue = 1;
            }

            return IsTrue;
        }
        public static int Get_TrueOrFalseValueRevereMinusOne(bool value)
        {
            int IsTrue = 1;

            if (value == true)
            {
                IsTrue = -1;
            }

            return IsTrue;
        }
        //tttt4444
        public static DataTable Get_TotalBalanceByTotal(DataTable dt, string CoumnLarg_Qnty, string CoumnSmall_Qnty, string CoumnItem_No, string CoumnBig_Unit, string CoumnSmall_Unit, string CoumnS_unit_Value)
        {
            string balanceStr = "";
            DataTable dtT = new DataTable();
            if (dt is null)
            {
                return dtT;
            }
            if (dt.Rows.Count == 0)
            {
                return dtT;
            }
            DataView view = new DataView(dt);
            DataTable dt_Stores = view.ToTable(true, CoumnBig_Unit);



            DataColumn Larg_Qnty_ = new DataColumn("Larg_Qnty", Type.GetType("System.String"));
            dtT.Columns.Add(Larg_Qnty_);

            DataColumn Small_Qnty_ = new DataColumn("Small_Qnty", Type.GetType("System.String"));
            dtT.Columns.Add(Small_Qnty_);
            DataColumn Item_No = new DataColumn("Item_No", Type.GetType("System.String"));
            dtT.Columns.Add(Item_No);

            DataColumn Qnty_Another = new DataColumn("Qnty_Another", Type.GetType("System.String"));
            dtT.Columns.Add(Qnty_Another);

            DataColumn Big_Unit_ = new DataColumn("Big_Unit", Type.GetType("System.String"));
            dtT.Columns.Add(Big_Unit_);

            DataColumn Small_Unit_ = new DataColumn("Small_Unit", Type.GetType("System.String"));
            dtT.Columns.Add(Small_Unit_);
            DataColumn balanceStr_ = new DataColumn("balanceStr", Type.GetType("System.String"));
            dtT.Columns.Add(balanceStr_);

            //DataColumn Item_No_ = new DataColumn("Item_No_", Type.GetType("System.String"));
            //dtT.Columns.Add(Item_No_);


            for (int x = 0; x < dt_Stores.Rows.Count; x++)
            {
                //New_Items_ds.Tables[0].Rows[index]["Item_DeletedFlage"] = 1;
                //New_Items_ds.Tables[0].Rows[index]["Item_ProcessedFlage"] = 1;


                string Big_Unit = dt_Stores.Rows[x][CoumnBig_Unit].ToString();

                decimal T_Larg_Qnty = 0;
                decimal T_Small_Qnty = 0;
                decimal T_Item_No = 0;
                decimal S_unit_Value = 1;
                //string Big_Unit = "";
                string Small_Unit = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    try
                    {
                        if (dt.Rows[i]["Item_DeletedFlage"].ToString() == "1")
                        {
                            continue;
                        }
                    }
                    catch (Exception ee)
                    {
                    }

                    try
                    {
                        int index = int.Parse(ArrExcludeIndex[i].ToString());
                        if (index == -1)
                        {

                        }
                        else
                        {
                            continue;
                        }
                    }
                    catch { }

                    string S_Big_Unit = dt.Rows[i][CoumnBig_Unit].ToString();

                    if (S_Big_Unit != Big_Unit)
                    {
                        continue;
                    }

                    S_unit_Value = decimal.Parse(dt.Rows[i][CoumnS_unit_Value].ToString());

                    Small_Unit = dt.Rows[i][CoumnSmall_Unit].ToString();
                    try
                    {
                        T_Larg_Qnty = T_Larg_Qnty + decimal.Parse(dt.Rows[i][CoumnLarg_Qnty].ToString());
                    }
                    catch { }
                    try
                    {
                        T_Small_Qnty = T_Small_Qnty + decimal.Parse(dt.Rows[i][CoumnSmall_Qnty].ToString());
                    }
                    catch { }
                    try
                    {
                        T_Item_No = T_Item_No + decimal.Parse(dt.Rows[i][CoumnItem_No].ToString());
                    }
                    catch { }
                    //try
                    //{
                    //    balanceStr = Common.GetQntyString(dt.Rows[i][CoumnLarg_Qnty].ToString(), Big_Unit, dt.Rows[i][CoumnSmall_Qnty].ToString(), Small_Unit);
                    //    dt.Rows[x]["balanceStr"] = balanceStr;
                    //    dt.Rows[x]["Item_No_"] = dt.Rows[i][CoumnItem_No].ToString();

                    //}
                    //catch { }
                    //Big_Unit = dt_Stores.Rows[x][CoumnBig_Unit].ToString();

                }
                Cls_Conversion_Unit.ConVert_By_S_UnitValue(ref T_Larg_Qnty, ref T_Small_Qnty, S_unit_Value);
                balanceStr = Common.GetQntyString(T_Larg_Qnty.ToString(), Big_Unit, T_Small_Qnty.ToString(), Small_Unit);
                //balanceStr = Common.GetQntyLenghts_Str(balanceStr, lv_dv[x]["QntyWidth"].ToString(), lv_dv[x]["QntyLenght"].ToString(), lv_dv[x]["QntyItemNo"].ToString());

                DataRow dr = dtT.NewRow();
                dr["Larg_Qnty"] = T_Larg_Qnty.ToString();
                dr["Small_Qnty"] = T_Small_Qnty.ToString();
                dr["Item_No"] = T_Item_No.ToString();
                dr["Qnty_Another"] = T_Item_No.ToString();

                dr["Big_Unit"] = Big_Unit;
                dr["Small_Unit"] = Small_Unit;
                dr["balanceStr"] = balanceStr;
                dtT.Rows.Add(dr);
            }
            return dtT;
        }
        public static DataTable Get_TotalBalanceByTotal(List<IInvoices_Details> invDs, ArrayList ArrExcludeIndex_, string CoumnLarg_Qnty, string CoumnSmall_Qnty, string CoumnItem_No)
        {
            //, string CoumnBig_Unit, string CoumnSmall_Unit, string CoumnS_unit_Value
            ArrExcludeIndex = new ArrayList();
            ArrExcludeIndex = ArrExcludeIndex_;
            return Get_TotalBalanceByTotal(invDs, CoumnLarg_Qnty, CoumnSmall_Qnty, CoumnItem_No);
            //, CoumnBig_Unit, CoumnSmall_Unit, CoumnS_unit_Value
        }
        public static DataTable Get_TotalBalanceByTotal(List<IInvoices_Details> invDs, string CoumnLarg_Qnty, string CoumnSmall_Qnty, string CoumnItem_No)
        {
            //, string CoumnBig_Unit, string CoumnSmall_Unit, string CoumnS_unit_Value
            ItemCLS itm = new ItemCLS();
            string balanceStr = "";
            DataTable dtT = new DataTable();
            if (invDs is null)
            {
                return dtT;
            }
            if (invDs.Count == 0)
            {
                return dtT;
            }



            DataTable dt = Common.ToDataTable(invDs);
            DataView view = new DataView(dt);
            string CoumnBig_Unit = "UnitStr";
            DataTable dt_Stores = view.ToTable(true, CoumnBig_Unit);

            DataColumn Larg_Qnty_ = new DataColumn("Larg_Qnty", Type.GetType("System.String"));
            dtT.Columns.Add(Larg_Qnty_);

            DataColumn Small_Qnty_ = new DataColumn("Small_Qnty", Type.GetType("System.String"));
            dtT.Columns.Add(Small_Qnty_);
            DataColumn Item_No = new DataColumn("Item_No", Type.GetType("System.String"));
            dtT.Columns.Add(Item_No);

            DataColumn Qnty_Another = new DataColumn("Qnty_Another", Type.GetType("System.String"));
            dtT.Columns.Add(Qnty_Another);

            DataColumn Big_Unit_ = new DataColumn("Big_Unit", Type.GetType("System.String"));
            dtT.Columns.Add(Big_Unit_);

            DataColumn Small_Unit_ = new DataColumn("Small_Unit", Type.GetType("System.String"));
            dtT.Columns.Add(Small_Unit_);
            DataColumn balanceStr_ = new DataColumn("balanceStr", Type.GetType("System.String"));
            dtT.Columns.Add(balanceStr_);

            //DataColumn Item_No_ = new DataColumn("Item_No_", Type.GetType("System.String"));
            //dtT.Columns.Add(Item_No_);
            unitCLS unitCLS = new unitCLS();

            for (int x = 0; x < dt_Stores.Rows.Count; x++)
            {

                //New_Items_ds.Tables[0].Rows[index]["Item_DeletedFlage"] = 1;
                //New_Items_ds.Tables[0].Rows[index]["Item_ProcessedFlage"] = 1;
                //long Unit_Id= long.Parse(dt.Rows[x]["Unit_Id"].ToString());
                //unit un = unitCLS.GetUnitByIdAsNoTracking();
                //string Big_Unit = dt_Stores.Rows[x][CoumnBig_Unit].ToString();

                string Big_Unit = "";
                decimal T_Larg_Qnty = 0;
                decimal T_Small_Qnty = 0;
                decimal T_Item_No = 0;
                decimal S_unit_Value = 1;
                //string Big_Unit = "";
                string Small_Unit = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    long Item_Id = 0;
                    long.TryParse(dt.Rows[x]["Item_Id"].ToString(), out Item_Id);
                    Items items = itm.GetItemByIdNoTracking(Item_Id);
                    Big_Unit = items.unit.B_Unit;
                    try
                    {
                        if (dt.Rows[i]["Item_DeletedFlage"].ToString() == "1")
                        {
                            continue;
                        }
                    }
                    catch (Exception ee)
                    {
                    }

                    try
                    {
                        int index = int.Parse(ArrExcludeIndex[i].ToString());
                        if (index == -1)
                        {

                        }
                        else
                        {
                            continue;
                        }
                    }
                    catch { }

                    //string S_Big_Unit = dt.Rows[i][CoumnBig_Unit].ToString();
                    string S_Big_Unit = items.unit.B_Unit;

                    if (S_Big_Unit != Big_Unit)
                    {
                        continue;
                    }

                    //S_unit_Value = decimal.Parse(dt.Rows[i][CoumnS_unit_Value].ToString());
                    S_unit_Value = items.unit.S_unit_Value ?? 1;
                    //Small_Unit = dt.Rows[i][CoumnSmall_Unit].ToString();
                    Small_Unit = items.unit.S_unit;
                    try
                    {
                        T_Larg_Qnty = T_Larg_Qnty + decimal.Parse(dt.Rows[i][CoumnLarg_Qnty].ToString());
                    }
                    catch { }
                    try
                    {
                        T_Small_Qnty = T_Small_Qnty + decimal.Parse(dt.Rows[i][CoumnSmall_Qnty].ToString());
                    }
                    catch { }
                    try
                    {
                        T_Item_No = T_Item_No + decimal.Parse(dt.Rows[i][CoumnItem_No].ToString());
                    }
                    catch { }
                    //try
                    //{
                    //    balanceStr = Common.GetQntyString(dt.Rows[i][CoumnLarg_Qnty].ToString(), Big_Unit, dt.Rows[i][CoumnSmall_Qnty].ToString(), Small_Unit);
                    //    dt.Rows[x]["balanceStr"] = balanceStr;
                    //    dt.Rows[x]["Item_No_"] = dt.Rows[i][CoumnItem_No].ToString();

                    //}
                    //catch { }
                    //Big_Unit = dt_Stores.Rows[x][CoumnBig_Unit].ToString();

                }
                Cls_Conversion_Unit.ConVert_By_S_UnitValue(ref T_Larg_Qnty, ref T_Small_Qnty, S_unit_Value);
                balanceStr = Common.GetQntyString(T_Larg_Qnty.ToString(), Big_Unit, T_Small_Qnty.ToString(), Small_Unit);
                //balanceStr = Common.GetQntyLenghts_Str(balanceStr, lv_dv[x]["QntyWidth"].ToString(), lv_dv[x]["QntyLenght"].ToString(), lv_dv[x]["QntyItemNo"].ToString());

                DataRow dr = dtT.NewRow();
                dr["Larg_Qnty"] = T_Larg_Qnty.ToString();
                dr["Small_Qnty"] = T_Small_Qnty.ToString();
                dr["Item_No"] = T_Item_No.ToString();
                dr["Qnty_Another"] = T_Item_No.ToString();

                dr["Big_Unit"] = Big_Unit;
                dr["Small_Unit"] = Small_Unit;
                dr["balanceStr"] = balanceStr;
                dtT.Rows.Add(dr);
            }
            return dtT;
        }


        public static string InstituteNameEN
        {
            get =>
                InstituteNameEN_;
            set
            {
                InstituteNameEN_ = value;
                CompanyTitleEN = InstituteNameEN_;
            }
        }



        public static void GetValue(DataTable dt, int index, ref int Column)
        {
            try
            {
                Column = int.Parse(dt.Rows[index]["" + Column.ToString() + ""].ToString());
            }
            catch (Exception ee)
            {

                Column = 0;
            }
        }
        public static int GetValueInt(DataTable dt, int index, string Column)
        {
            int Value = 0;
            try
            {
                Value = int.Parse(dt.Rows[index]["" + Column.ToString() + ""].ToString());
            }
            catch (Exception ee)
            {

                Value = 0;
            }
            return Value;
        }
        public static int GetValueInt(string Column)
        {
            int Value = 0;
            try
            {
                //Value = int.Parse(Column);

                // Try parsing as a decimal using InvariantCulture (handles dots as decimal separators)
                if (decimal.TryParse(Column, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal decimalValue))
                {
                    return (int)decimalValue; // Truncates decimal parts: "26.00" -> 26, "26.85" -> 26
                                              // Or use: return (int)Math.Round(decimalValue); // If you prefer rounding up/down
                }

                return 0; // Default fallback if parsing fails or string is empty/null
            }
            catch (Exception ee)
            {

                Value = 0;
            }

            return Value; 
        }

        public static  float ConvertValuefloat(string V)
        {
            // TryParse يتعامل مع النصوص الفارغة "" و null دون أي خطأ ويرجع false
            if (float.TryParse(V, NumberStyles.Any, CultureInfo.InvariantCulture, out float result))
            {
                return result;
            }

            return 0f; // القيمة الافتراضية عند الفشل أو عندما يكون النص فارغاً
        }

        public static long GetValueLong(DataTable dt, int index, string Column)
        {
            long Value = 0;
            try
            {
                string V = dt.Rows[index]["" + Column.ToString() + ""].ToString();
                if (long.TryParse(V, NumberStyles.Any, CultureInfo.InvariantCulture, out long result))
                {
                    return result;
                }

                //Value = long.Parse(dt.Rows[index]["" + Column.ToString() + ""].ToString());
            }
            catch (Exception ee)
            {

                Value = 0;
            }
            return Value;
        }
        public static long GetValueLong(string Column)
        {
            
            try
            {
                if (long.TryParse(Column, NumberStyles.Any, CultureInfo.InvariantCulture, out long result))
                {
                    return result;
                }

                return 0 ;
                //Value = long.Parse(Column);
            }
            catch (Exception ee)
            {

                return 0;
            }

            
        }
        public static decimal GetValueDecimal(string Column)
        {
            decimal Value = 0;
            try
            {
                decimal.TryParse(Column, NumberStyles.Any, CultureInfo.InvariantCulture, out Value);

            }
            catch (Exception ee)
            {

                Value = 0;
            }
            Value = RoundMe(Value);
            return Value;
        }
        public static decimal GetValueDecimal(DataTable dt, int index, string Column)
        {
            decimal Value = 0;
            try
            {
                string V = dt.Rows[index]["" + Column.ToString() + ""].ToString();
                decimal.TryParse(V, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result);
               // Value = decimal.Parse(dt.Rows[index]["" + Column.ToString() + ""].ToString());
            }
            catch (Exception ee)
            {

                Value = 0;
            }
            Value = RoundMe(Value);
            return Value;
        }
        public static string GetValueString(DataTable dt, int index, string Column)
        {
            string Value = "";
            try
            {
                Value = dt.Rows[index]["" + Column.ToString() + ""].ToString();
            }
            catch (Exception ee)
            {

                Value = "";
            }
            return Value;
        }
        

        

        public static void GetValue(DataTable dt, int index, ref string Column)
        {
            try
            {
                Column = dt.Rows[index]["" + Column.ToString() + ""].ToString();
            }
            catch (Exception ee)
            {

                Column = "";
            }
        }

        public static void GetValue(DataTable dt, int index, ref DateTime Column)
        {
            try
            {
                Column = DateTime.Parse(dt.Rows[index]["" + Column.ToString() + ""].ToString());
            }
            catch (Exception ee)
            {

                Column = new DateTime(2000, 1, 1);
            }
        }

        public static void GetValue(DataTable dt, int index, ref DateTime? Column)
        {
            try
            {
                Column = DateTime.Parse(dt.Rows[index]["" + Column.ToString() + ""].ToString());
            }
            catch (Exception ee)
            {

                Column = null;
            }
        }
        public static DateTime? GetValueNullDateTime(DataTable dt, int index, string Column)
        {
            DateTime? Column_;
            try
            {
                Column_ = DateTime.Parse(dt.Rows[index]["" + Column.ToString() + ""].ToString());
            }
            catch (Exception ee)
            {

                Column_ = null;
            }

            return Column_;
        }

        public static void SearchSystemParametersDB(string KeysData, int index)
        {

            DataRow row = dtConfig.Rows[index];
            if (KeysData == "IN")
            {
                InstituteName = row["Value"].ToString();
            }
            if (KeysData == "IN_EN")
            {
                InstituteNameEN = row["Value"].ToString();
            }
            else if (KeysData == "Is_Show_Quantity_Lenghts_For_ItemsOnInvoice")
            {
                Is_Show_Quantity_Lenghts_For_ItemsOnInvoice = int.Parse(row["Value"].ToString());
            }

            else if (KeysData == "Is_ShowTotalsItems_in_Invoice")
            {
                Is_ShowTotalsItems_in_Invoice = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Is_ShowTotalsItems_in_Bill")
            {
                Is_ShowTotalsItems_in_Bill = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_InvoiceAnotherPayment")
            {
                Show_InvoiceAnotherPayment = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "VisiblecolumnBalanceAfterInOderMoney")
            {
                VisiblecolumnBalanceAfterInOderMoney = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Project_Market")
            {
                Project_Market = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Make_Login")
            {
                Make_Login = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Make_Login_Auto")
            {
                Make_Login_Auto = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Store_Id_UsedBefore")
            {
                Store_Id_UsedBefore = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Store_Index_UsedBefore")
            {
                Store_Index_UsedBefore = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Is_ToIndicDigitsArabic")
            {
                Is_ToIndicDigitsArabic = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Is_SaveUserNameLogin")
            {
                Is_SaveUserNameLogin = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Is_SaveUserPasswordLogin")
            {
                Is_SaveUserPasswordLogin = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "IsSalesMan")
            {
                IsSalesMan = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Customer_Type")
            {
                Customer_Type = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Make_Store_Order_Sarf_Estlam_Automatic_From_Invoice")
            {
                Make_Store_Order_Sarf_Estlam_Automatic_From_Invoice = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Make_Store_Order_Sarf_Estlam_Automatic_From_Invoice_Enabled")
            {
                Make_Store_Order_Sarf_Estlam_Automatic_From_Invoice_Enabled = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "ItemCategory_Type")
            {
                ItemCategory_Type = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "ItemDeleteOrMakeNotActiveInDataBase")
            {
                ItemDeleteOrMakeNotActiveInDataBase = int.Parse(row["Value"].ToString());
            }

            else if (KeysData == "MakeOrderMoneyFromInvoice")
            {
                MakeOrderMoneyFromInvoice = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "MakeOrderMoneyFromInvoiceAutomaticWithoutForm")
            {
                MakeOrderMoneyFromInvoiceAutomaticWithoutForm = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "MakeOrderMoneyFromInvoice_Reverse_AutomaticWithoutForm")
            {
                MakeOrderMoneyFromInvoice_Reverse_AutomaticWithoutForm = int.Parse(row["Value"].ToString());
            }


            else if (KeysData == "MakeOrderMoneyFromBill_AutomaticWithoutForm")
            {
                MakeOrderMoneyFromBill_AutomaticWithoutForm = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "MakeOrderMoneyFromBills_Reverse_AutomaticWithoutForm")
            {
                MakeOrderMoneyFromBills_Reverse_AutomaticWithoutForm = int.Parse(row["Value"].ToString());
            }

            else if (KeysData == "Customer_Title")
            {
                Customer_Title = row["Value"].ToString();
            }
            else if (KeysData == "Supplier_Title")
            {
                Supplier_Title = row["Value"].ToString();
            }
            else if (KeysData == "Show_BtnOrderSarfStoreFromInvoice")
            {
                Show_BtnOrderSarfStoreFromInvoice = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Enabel_cbIs_TaxPer_Item_OnInvoiceMonitor")
            {
                Enabel_cbIs_TaxPer_Item_OnInvoiceMonitor = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Use_cbIs_TaxPer_Item_OnInvoiceMonitor")
            {
                Use_cbIs_TaxPer_Item_OnInvoiceMonitor = int.Parse(row["Value"].ToString());
            }
            //else if (KeysData == "Is_Tax_Per_Item")
            //{
            //    Is_Tax_Per_Item = int.Parse(row["Value"].ToString());
            //}
            else if (KeysData == "ShowSalesMan")
            {
                ShowSalesMan = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Make_Store_Order_Sarf_Estlam_Automatic")
            {
                Make_Store_Order_Sarf_Estlam_Automatic = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Used_Item_NoAS_3ebowaInReport_Balance")
            {
                Used_Item_NoAS_3ebowaInReport_Balance = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Used_Item_NoAS_3ebowaInReport_Balance_InBill")
            {
                Used_Item_NoAS_3ebowaInReport_Balance_InBill = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Compute_QntyFrom_ItemNo")
            {
                Compute_QntyFrom_ItemNo = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "VisibleCompute_QntyFrom_ItemNo_CheckBox")
            {
                VisibleCompute_QntyFrom_ItemNo_CheckBox = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "DesignedBy")
            {
                DesignedBy = row["Value"].ToString();
            }
            else if (KeysData == "Compute_QntyFrom_Only_ItemNo")
            {
                Compute_QntyFrom_Only_ItemNo = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Compute_QntyFrom_Only_ItemNoIsOne")
            {
                Compute_QntyFrom_Only_ItemNoIsOne = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "VisibleSpecification_QntyFrom_ItemNo")
            {
                VisibleSpecification_QntyFrom_ItemNo = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Compute_ItemBalance_With_Fraction")
            {
                Compute_ItemBalance_With_Fraction = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Is_Takseet_Invoice")
            {
                Is_Takseet_Invoice = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Compute_QntyFrom_ItemNo_CheckBox")
            {
                Compute_QntyFrom_ItemNo_CheckBox = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Update_DeleteSupplier")
            {
                Update_DeleteSupplier = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Is_ShowUnitBeside_Qnty")
            {
                Is_ShowUnitBeside_Qnty = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Employee_Tables_IS_USER_DETAILES_Or_Employee")
            {
                Employee_Tables_IS_USER_DETAILES_Or_Employee = row["Value"].ToString();
            }
            else if (KeysData == "Transform_Type")
            {
                Transform_Type = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Year_Financial_Period_Id")
            {
                Year_Financial_Period_Id = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Compute_3eboaFrom_EznSarf_Estlam")
            {
                Compute_3eboaFrom_EznSarf_Estlam = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Dived_ByStore_Type_In_Invoice_Type")
            {
                Dived_ByStore_Type_In_Invoice_Type = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Use_Default_Store_In_Invoice_Bill")
            {
                Use_Default_Store_In_Invoice_Bill = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "IsReadPreviousAudience_priceOnBill")
            {
                IsReadPreviousAudience_priceOnBill = int.Parse(row["Value"].ToString());
            }

            else if (KeysData == "Can_ChangePrice_On_invoice")
            {
                Can_ChangePrice_On_invoice = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "IS_Use_Price_Type_List")
            {
                IS_Use_Price_Type_List = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Is_Invoice_Qnty_Needed")
            {
                Is_Invoice_Qnty_Needed = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Is_Invoice_ItemNo_Needed")
            {
                Is_Invoice_ItemNo_Needed = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Is_Invoice_Price_Needed")
            {
                Is_Invoice_Price_Needed = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Is_Update_Price_On_Bill")
            {
                Is_Update_Price_On_Bill = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Is_Update_Price_On_invoice")
            {
                Is_Update_Price_On_invoice = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "IsOpenItemDataToAddNew")
            {
                IsOpenItemDataToAddNew = int.Parse(row["Value"].ToString());
            }


            else if (KeysData == "Can_Chang_Automatic_SarfFromInvoice_ByItems")
            {
                Can_Chang_Automatic_SarfFromInvoice_ByItems = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Separate_Items_Sell_PurchaseByStoreType")
            {
                Separate_Items_Sell_PurchaseByStoreType = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "IsUsePriceWithStoreOrder")
            {
                IsUsePriceWithStoreOrder = int.Parse(row["Value"].ToString());
            }



            else if (KeysData == "Add_AccountEntryByTree")
            {
                Add_AccountEntryByTree = int.Parse(row["Value"].ToString());
            }
             
            else if ((KeysData == "dtExpire"))
            {
                try
                {
                    dtExpire = DateTime.Parse(row["Value"].ToString());
                }
                catch (Exception ee)
                {

                }
            }
            if (KeysData == "PointOfSale")
            {
                PointOfSale = int.Parse(row["Value"].ToString());
            }
            if (KeysData == "Is_InvoiceNoBy_Branch_Id")
            {
                Is_InvoiceNoBy_Branch_Id = int.Parse(row["Value"].ToString());
            }
            if (KeysData == "Is_InvoiceNoBy_FinancialYear")
            {
                Is_InvoiceNoBy_FinancialYear = int.Parse(row["Value"].ToString());
            }
            if (KeysData == "Is_InvoiceNoBy_Invoice_Type")
            {
                Is_InvoiceNoBy_Invoice_Type = int.Parse(row["Value"].ToString());
            }

            else if (KeysData == "Show_Qnty_With_Unit")
            {
                Show_Qnty_With_Unit = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "ColumnQnty_width")
            {
                ColumnQnty_width = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "ColumnItemsName_width")
            {
                ColumnItemsName_width = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "ColumnStoreName_width")
            {
                ColumnStoreName_width = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "ColumnSmallBalalance_Width_ItemData_OnInvoiceMonitor")
            {
                ColumnSmallBalalance_Width_ItemData_OnInvoiceMonitor = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "ColumnBalance_Width_ItemData_OnInvoiceMonitor")
            {
                ColumnBalance_Width_ItemData_OnInvoiceMonitor = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "ColumnItemName_Width_ItemData_OnInvoiceMonitor")
            {
                ColumnItemName_Width_ItemData_OnInvoiceMonitor = int.Parse(row["Value"].ToString());
            }


            else if (KeysData == "ColumnQnty_width_Bill")
            {
                ColumnQnty_width_Bill = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "ColumnItemsName_width_Bill")
            {
                ColumnItemsName_width_Bill = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "ColumnStoreName_width_Bill")
            {
                ColumnStoreName_width_Bill = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "ColumnEmpty_width_Bill")
            {
                ColumnEmpty_width_Bill = int.Parse(row["Value"].ToString());
            }

            else if (KeysData == "ColumnSmall_QntyStr_width_Invoice")
            {
                ColumnSmall_QntyStr_width_Invoice = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "ColumnSmall_QntyStr_width_Bill")
            {
                ColumnSmall_QntyStr_width_Bill = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_Customer_Notes_OnInvoiceMonitor")
            {
                Show_Customer_Notes_OnInvoiceMonitor = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_Supplier_Notes_OnInvoiceMonitor")
            {
                Show_Supplier_Notes_OnInvoiceMonitor = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_EmployeeSeller_OnInvoiceMonitor")
            {
                Show_EmployeeSeller_OnInvoiceMonitor = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_Shipment_DataInvoiceMonitor")
            {
                Show_Shipment_DataInvoiceMonitor = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_Shipment_DataInvoiceOnPrint")
            {
                Show_Shipment_DataInvoiceOnPrint = int.Parse(row["Value"].ToString());
            }


            else if (KeysData == "ColumnSmall_QntyStr_Name_Invoice")
            {
                ColumnSmall_QntyStr_Name_Invoice = row["Value"].ToString();
            }
            else if (KeysData == "ColumnSmall_QntyStr_Name_Bill")
            {
                ColumnSmall_QntyStr_Name_Bill = row["Value"].ToString();
            }
            else if (KeysData == "ColumnQnty_Name_Invoice")
            {
                ColumnQnty_Name_Invoice = row["Value"].ToString();
            }
            else if (KeysData == "ColumnQnty_Name_Bill")
            {
                ColumnQnty_Name_Bill = row["Value"].ToString();
            }

            else if (KeysData == "ColumnLarg_Qnty_Name_Invoice")
            {
                ColumnLarg_Qnty_Name_Invoice = row["Value"].ToString();
            }
            else if (KeysData == "columnSmall_Qnty_Name_Invoice")
            {
                columnSmall_Qnty_Name_Invoice = row["Value"].ToString();
            }

            else if (KeysData == "ColumnLarg_Qnty_Width_Invoice")
            {
                ColumnLarg_Qnty_Width_Invoice = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "columnSmall_Qnty_Width_Invoice")
            {
                columnSmall_Qnty_Width_Invoice = int.Parse(row["Value"].ToString());
            }



            else if (KeysData == "UseAutomaticItemCode")
            {
                UseAutomaticItemCode = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Is_ShowItemSpecificationinItemDataList")
            {
                Is_ShowItemSpecificationinItemDataList = int.Parse(row["Value"].ToString());
            }


            else if (KeysData == "Is_Print_CompanyName")
            {
                Is_Print_CompanyName = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Is_Print_Branch_Name")
            {
                Is_Print_Branch_Name = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Is_ShowPrintDialog")
            {
                Is_ShowPrintDialog = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Is_ShowPrintDialog_POS")
            {
                Is_ShowPrintDialog_POS = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Is_Save_DataDictionary")
            {
                Is_Save_DataDictionary = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Is_Save_DataDictionaryDbSyncLogs")
            {
                Is_Save_DataDictionaryDbSyncLogs = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Is_Save_DataDictionaryLogerException")
            {
                Is_Save_DataDictionaryLogerException = int.Parse(row["Value"].ToString());
            }
            
            else if (KeysData == "Is_SearchItemBy_BarCode")
            {
                Is_SearchItemBy_BarCode = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_SearchItemBy_BarCode_On_Invoice")
            {
                Show_SearchItemBy_BarCode_On_Invoice = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "TextToShowUnderCodeWhenPrintBarCode")
            {
                TextToShowUnderCodeWhenPrintBarCode = row["Value"].ToString();

            }

            else if (KeysData == "Is_SaveOnDB_When_AddItemDirectly_On_invoice")
            {
                Is_SaveOnDB_When_AddItemDirectly_On_invoice = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Is_SaveOnDB_When_AddItemDirectly_On_Bill")
            {
                Is_SaveOnDB_When_AddItemDirectly_On_Bill = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_AddNotes_On_Invoice")
            {
                Show_AddNotes_On_Invoice = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_AddNotes_On_Bill")
            {
                Show_AddNotes_On_Bill = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_Load_Payment_OnBillPrint")
            {
                Show_Load_Payment_OnBillPrint = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_TaxSale_Value_OnBillPrint")
            {
                Show_TaxSale_Value_OnBillPrint = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_ItemSecondTotalPrice_OnBillPrint")
            {
                Show_ItemSecondTotalPrice_OnBillPrint = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_Damaghat_OnBillPrint")
            {
                Show_Damaghat_OnBillPrint = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_Commision_OnBillPrint")
            {
                Show_Commision_OnBillPrint = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_ATS05_Value_OnBillPrint")
            {
                Show_ATS05_Value_OnBillPrint = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_SupplierAccount_AfterBeforInBillPrint")
            {
                Show_SupplierAccount_AfterBeforInBillPrint = int.Parse(row["Value"].ToString());
            }

            else if (KeysData == "Show_NotesInBillPrint")
            {
                Show_NotesInBillPrint = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_Total_List3Mola_OnBillPrint")
            {
                Show_Total_List3Mola_OnBillPrint = int.Parse(row["Value"].ToString());
            }
            //"عمولة البيع";
            else if (KeysData == "RenewalDateForTaxPortalCloud")
            {
                RenewalDateForTaxPortalCloud = row["Value"].ToString();
                RenewalDateForTaxPortalCloud = RenewalDateForTaxPortalCloud.Trim();
            }
            else if (KeysData == "IsItemSellPriceContainTaxValue")
            {
                IsItemSellPriceContainTaxValue = int.Parse(row["Value"].ToString());

            }

            else if (KeysData == "IsBtnAddItemsDirectTocarWorkOrder")
            {
                IsBtnAddItemsDirectTocarWorkOrder = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "storePriceType_CostPriceOrLastPrice")
            {
                storePriceType_CostPriceOrLastPrice = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "IsEnableStore_CostPriceOrLastPrice")
            {
                IsEnableStore_CostPriceOrLastPrice = int.Parse(row["Value"].ToString());
            }

            else if (KeysData == "IsCheckRenewalForTaxPortalCloud")
            {
                IsCheckRenewalForTaxPortalCloud = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "IsNeedToRenewalForTaxPortalCloud")
            {
                IsNeedToRenewalForTaxPortalCloud = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "TaxinternalID_Prevx")
            {
                TaxinternalID_Prevx = row["Value"].ToString();
                TaxinternalID_Prevx = TaxinternalID_Prevx.Trim();
            }
            else if (KeysData == "TaxinternalID_StartFromNumber")
            {
                TaxinternalID_StartFromNumber = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "TaxinternalID_StartFromNumber_AS_InvoiceList")
            {
                TaxinternalID_StartFromNumber_AS_InvoiceList = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "IsItemSellPriceContainTaxValue")
            {
                IsItemSellPriceContainTaxValue = int.Parse(row["Value"].ToString());
            }


            else if (KeysData == "ItemNameForMasna3iatKhamat")
            {
                ItemNameForMasna3iatKhamat = row["Value"].ToString();
            }

            else if (KeysData == "VersionRun_On_OnePC_Using_HardwareInfo")
            {
                VersionRun_On_OnePC_Using_HardwareInfo = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "HardWareInfo_Serial")
            {
                HardWareInfo_Serial = row["Value"].ToString();
            }
            else if (KeysData == "IS_Used_SandoukSystem")
            {
                IS_Used_SandoukSystem = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "IS_Used_SandoukWithPermission")
            {
                IS_Used_SandoukWithPermission = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Ehlak_Kest_Sabt")
            {
                Ehlak_Kest_Sabt = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Ehlak_Kest_Motanakes")
            {
                Ehlak_Kest_Motanakes = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Account_Seperator_Char")
            {
                Account_Seperator_Char = char.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Currency_Count")
            {
                Currency_Count = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Project_GeneralAccounts")
            {
                Project_GeneralAccounts = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Sunisoft_IrisSkin_SerialNumber")
            {
                Sunisoft_IrisSkin_SerialNumber = row["Value"].ToString();
            }
            else if (KeysData == "Balance_BeginingPeriod")
            {
                Balance_BeginingPeriod = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Flage_AccountDetails_View")
            {
                Flage_AccountDetails_View = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Language_ID")
            {
                Language_ID = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "DataBaseType")
            {
                DataBaseType = row["Value"].ToString();
            }
            else if (KeysData == "IS_Make_Automatic_Queeod")
            {
                IS_Make_Automatic_Queeod = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Customer_Id_DefaultSelect")
            {
                Customer_Id_DefaultSelect = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Customer_Id_ForInvoiceNakdy")
            {
                Customer_Id_ForInvoiceNakdy = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Customer_Id_ForInvoiceNakdyThenDisableCombo")
            {
                Customer_Id_ForInvoiceNakdyThenDisableCombo = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "RemoveCustomer_Id_ForInvoiceNakdy")
            {
                RemoveCustomer_Id_ForInvoiceNakdy = int.Parse(row["Value"].ToString());
            }

            else if (KeysData == "IsDisableEditEntryWhenCreateAutomatic")
            {
                IsDisableEditEntryWhenCreateAutomatic = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Supplier_Id_DefaultSelect")
            {
                Supplier_Id_DefaultSelect = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Currency_Id_DefaultSelect")
            {
                Currency_Id_DefaultSelect = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Sandouk_Id_DefaultSelect")
            {
                Sandouk_Id_DefaultSelect = int.Parse(row["Value"].ToString());
            }

            else if (KeysData == "Is_Set_Customer_Balance_With_Items")
            {
                Is_Set_Customer_Balance_With_Items = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Is_Set_Supplier_Balance_With_Items")
            {
                Is_Set_Supplier_Balance_With_Items = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Is_Set_Customer_Balance_With_Sum_Items")
            {
                Is_Set_Customer_Balance_With_Sum_Items = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Is_Set_Supplier_Balance_With_Sum_Items")
            {
                Is_Set_Supplier_Balance_With_Sum_Items = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Make_Sarf_Item_BalanceFromInvoice")
            {
                Make_Sarf_Item_BalanceFromInvoice = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Make_Store_Order_Sarf_Estlam_Ta7weel_Automatic_Between_Stores")
            {
                Make_Store_Order_Sarf_Estlam_Ta7weel_Automatic_Between_Stores = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Year_Financial_Period_DateFrom")
            {
                Year_Financial_Period_DateFrom = DateTime.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Year_Financial_Period_DateTo")
            {
                Year_Financial_Period_DateTo = DateTime.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Image_Item_TypeFor_Items")
            {
                Image_Item_TypeFor_Items = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Image_Item_TypeFor_CheckMoney")
            {
                Image_Item_TypeFor_CheckMoney = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Image_Item_TypeFor_Entry")
            {
                Image_Item_TypeFor_Entry = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "DueDateNearestDayes")
            {
                DueDateNearestDayes = int.Parse(row["Value"].ToString());
            }

            else if (KeysData == "IS_Make_AutomaticFromTar7eel_For_Invoice")
            {
                IS_Make_AutomaticFromTar7eel_For_Invoice = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "IS_Make_AutomaticFromTar7eel_For_InvoiceReverse")
            {
                IS_Make_AutomaticFromTar7eel_For_InvoiceReverse = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "IS_Make_AutomaticFromTar7eel_For_Bill")
            {
                IS_Make_AutomaticFromTar7eel_For_Bill = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "IS_Make_AutomaticFromTar7eel_For_BillReverse")
            {
                IS_Make_AutomaticFromTar7eel_For_BillReverse = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "POS_Is_StartInvoiceWith_All_MainCategoy")
            {
                POS_Is_StartInvoiceWith_All_MainCategoy = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "SelectFirstPriceListOnInvoice")
            {
                SelectFirstPriceListOnInvoice = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "UseInvoiceTypeKta3am_Khas")
            {
                UseInvoiceTypeKta3am_Khas = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "UseInvoiceNoByDay_POS")
            {
                UseInvoiceNoByDay_POS = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "UseInvoice_POS")
            {
                UseInvoice_POS = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "IsVisibleServerDataBackup")
            {
                IsVisibleServerDataBackup = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "SQLDataSource")
            {
                SQLDataSource = row["Value"].ToString();
            }
            else if (KeysData == "SQLUserName")
            {
                SQLUserName = row["Value"].ToString();
            }
            else if (KeysData == "SQLUserPassword")
            {
                SQLUserPassword = row["Value"].ToString();
            }
            else if (KeysData == "SQLDataBaseName")
            {
                SQLDataBaseName = row["Value"].ToString();
            }
            else if (KeysData == "SQl_ServerExpress")
            {
                SQl_ServerExpress = row["Value"].ToString();
            }
            else if (KeysData == "Is_PrintCompanyNameOnInvoice")
            {
                Is_PrintCompanyNameOnInvoice = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "IsShowTotalTransactionInSearchPeriod")
            {
                IsShowTotalTransactionInSearchPeriod = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "IsShowTotalTransaction")
            {
                IsShowTotalTransaction = int.Parse(row["Value"].ToString());
            }

            else if (KeysData == "NotesBalanceLabelMankol")
            {
                NotesBalanceLabelMankol = row["Value"].ToString();
            }

            else if (KeysData == "Save_Backup_Path")
            {
                Save_Backup_Path = row["Value"].ToString();
            }
            else if (KeysData == "Tager_Kontra_Name")
            {
                Tager_Kontra_Name = row["Value"].ToString();
            }
            else if (KeysData == "Statement_Name")
            {
                Statement_Name = row["Value"].ToString();
            }
            else if (KeysData == "Rasamat_Title")
            {
                Rasamat_Title = row["Value"].ToString();
            }


            else if (KeysData == "Invoice_Title")
            {
                Invoice_Title = row["Value"].ToString();
            }
            else if (KeysData == "Invoice_MainTitle_Invoice")
            {
                Invoice_MainTitle_Invoice = row["Value"].ToString();
            }
            else if (KeysData == "Invoice_MainTitle_Bill")
            {
                Invoice_MainTitle_Bill = row["Value"].ToString();
            }
            else if (KeysData == "Invoice_MainTitle_Invoice_Reverse")
            {
                Invoice_MainTitle_Invoice_Reverse = row["Value"].ToString();
            }
            else if (KeysData == "Invoice_MainTitle_Bill_Reverse")
            {
                Invoice_MainTitle_Bill_Reverse = row["Value"].ToString();
            }
            else if (KeysData == "Invoice_Title_Date")
            {
                Invoice_Title_Date = row["Value"].ToString();
            }
            else if (KeysData == "PrinterNameForReportsA4")
            {
                PrinterNameForReportsA4 = row["Value"].ToString();
            }
            else if (KeysData == "PrinterNameForPOSInvoice")
            {
                PrinterNameForPOSInvoice = row["Value"].ToString();
            }
            else if (KeysData == "PrinterNameForKitchenPrinter")
            {
                PrinterNameForKitchenPrinter = row["Value"].ToString();
            }
            else if (KeysData == "PrinterNameForBarCodePrinter")
            {
                PrinterNameForBarCodePrinter = row["Value"].ToString();
            }
            


            else if (KeysData == "IsSentToPrinterNameForKitchenPrinter")
            {
                IsSentToPrinterNameForKitchenPrinter = int.Parse(row["Value"].ToString());
            }

            else if (KeysData == "Is_PrintCompanyNameOnBill")
            {
                Is_PrintCompanyNameOnBill = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "SQl_ServerWindows_Authentication")
            {
                SQl_ServerWindows_Authentication = row["Value"].ToString();
            }
            else if (KeysData == "SQl_Server_SQl_Authentication")
            {
                SQl_Server_SQl_Authentication = row["Value"].ToString();
            }
            else if (KeysData == "SQl_NativeClientSQl_Server2005")
            {
                SQl_NativeClientSQl_Server2005 = row["Value"].ToString();
            }
            else if (KeysData == "SQl_ServerExpressWithDataDirectory")
            {
                SQl_ServerExpressWithDataDirectory = row["Value"].ToString();
            }
            else if (KeysData == "SQl_LocalDB")
            {
                SQl_LocalDB = row["Value"].ToString();
            }
            else if (KeysData == "POSInvoivePrinterCopiesNumber")
            {
                POSInvoivePrinterCopiesNumber = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Use_AutoNumeric_Customer_Id__With_Submain_Account_Id")
            {
                Use_AutoNumeric_Customer_Id__With_Submain_Account_Id = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Is_CheckSandokBalnce_Sarf")
            {
                Is_CheckSandokBalnce_Sarf = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Use_AutoNumeric_Supplier_Id_With_Submain_Account_Id")
            {
                Use_AutoNumeric_Supplier_Id_With_Submain_Account_Id = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Use_AutoNumeric_Bank_Id_With_Submain_Account_Id")
            {
                Use_AutoNumeric_Bank_Id_With_Submain_Account_Id = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Use_AutoNumeric_Employee_Id_With_Submain_Account_Id")
            {
                Use_AutoNumeric_Employee_Id_With_Submain_Account_Id = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Use_AutoNumeric_Submain_Account_Id")
            {
                Use_AutoNumeric_Submain_Account_Id = int.Parse(row["Value"].ToString());
            }
            
            else if (KeysData == "UseMoreImageForItem")
            {
                UseMoreImageForItem = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_CustomerAccount_AfterBeforInInvoice")
            {
                Show_CustomerAccount_AfterBeforInInvoice = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_CustomerAccount_Befor_OnlyInInvoice")
            {
                Show_CustomerAccount_Befor_OnlyInInvoice = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_CustomerAccount_After_OnlyInInvoice")
            {
                Show_CustomerAccount_After_OnlyInInvoice = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_SupplierAccount_AfterBeforInInvoice")
            {
                Show_SupplierAccount_AfterBeforInInvoice = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_CustomerAccount_FinalyNowInInvoicePrint")
            {
                Show_CustomerAccount_FinalyNowInInvoicePrint = int.Parse(row["Value"].ToString());
            }


            else if (KeysData == "Show_NotesInInvoicePrint")
            {
                Show_NotesInInvoicePrint = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_SpecificationInBillPrint")
            {
                Show_SpecificationInBillPrint = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_SpecificationInInvoicePrint")
            {
                Show_SpecificationInInvoicePrint = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "ShowCustomerImage")
            {
                ShowCustomerImage = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "ShowCustomerVisaData")
            {
                ShowCustomerVisaData = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Is_Unit_As_decimal")
            {
                Is_Unit_As_decimal = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "is_pharmacy")
            {
                is_pharmacy = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "IsSetCustomers_AccountsForInvoice")
            {
                IsSetCustomers_AccountsForInvoice = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "IsSetSuppliers_AccountsForBills")
            {
                IsSetSuppliers_AccountsForBills = int.Parse(row["Value"].ToString());
            }



            else if (KeysData == "is_CarsMaintainceSystem")
            {
                is_CarsMaintainceSystem = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "StartupForm_Name")
            {
                StartupForm_Name = row["Value"].ToString();
            }
            else if (KeysData == "FromWhereTableNameGet_TaxInvoiceNo")
            {
                FromWhereTableNameGet_TaxInvoiceNo = row["Value"].ToString();
            }
            else if (KeysData == "User_Agent_ToSendToTaxPortal")
            {
                User_Agent_ToSendToTaxPortal = row["Value"].ToString();
            }

            else if (KeysData == "DecimalPlaces")
            {
                DecimalPlaces = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "DecimalPlaces_NoFractionIfZero")
            {
                DecimalPlaces_NoFractionIfZero = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "decimalValueRoundOrTruncate")
            {
                decimalValueRoundOrTruncate = int.Parse(row["Value"].ToString());
            }


            else if (KeysData == "Divid_ItemCatygoryByType_Id")
            {
                Divid_ItemCatygoryByType_Id = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Set_ItemBalanceZeroWhenNegative")
            {
                Set_ItemBalanceZeroWhenNegative = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Is_Check_Expiration_validation")
            {
                Is_Check_Expiration_validation = int.Parse(row["Value"].ToString());
            }

            if (KeysData == "FormHeighDiff")
            {
                FormHeighDiff = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Using_FormHeighDiff")
            {
                Using_FormHeighDiff = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Use_Auto_Identity_Column")
            {
                Use_Auto_Identity_Column = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Added_Alf_Hadfa_Value")
            {
                Added_Alf_Hadfa_Value = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Discount_Alf_Hadfa_Value")
            {
                Discount_Alf_Hadfa_Value = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Entry_Currency_Id")
            {
                Entry_Currency_Id = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Currency_Name")
            {
                Currency_Name = row["Value"].ToString();
            }
            else if (KeysData == "bubbleBar_Visibility")
            {
                bubbleBar_Visibility = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "EditItemBalanceFromReport")
            {
                EditItemBalanceFromReport = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Project_TypeStr")
            {
                Project_TypeStr = row["Value"].ToString();
            }
            else if (KeysData == "Store_Text_Label")
            {
                Store_Text_Label = row["Value"].ToString();
            }


            else if (KeysData == "CompanyName_Tel1")
            {
                CompanyName_Tel1 = row["Value"].ToString();
                if (CompanyName_Tel1.Trim() == "")
                {
                    CompanyName_Tel1 = " ";
                }
            }
            else if (KeysData == "CompanyName_Tel2")
            {
                CompanyName_Tel2 = row["Value"].ToString();
                if (CompanyName_Tel2.Trim() == "")
                {
                    CompanyName_Tel2 = " ";
                }
            }
            else if (KeysData.Trim() == "CompanyTitle")
            {
                CompanyTitle = row["Value"].ToString();
                if (CompanyTitle.Trim() == "")
                {
                    CompanyTitle = " ";
                }
            }

            else if (KeysData == "Company_Tax_No")
            {
                Company_Tax_No = row["Value"].ToString();
                if (Company_Tax_No.Trim() == "")
                {
                    Company_Tax_No = " ";
                }
            }
            else if (KeysData == "Company_Commercial_Register")
            {
                Company_Commercial_Register = row["Value"].ToString();
                if (Company_Commercial_Register.Trim() == "")
                {
                    Company_Commercial_Register = " ";
                }
            }
            else if (KeysData == "Company_Memberance_Register")
            {
                Company_Memberance_Register = row["Value"].ToString();
                if (Company_Memberance_Register.Trim() == "")
                {
                    Company_Memberance_Register = " ";
                }
            }
            else if (KeysData == "Company_Address1")
            {
                Company_Address1 = row["Value"].ToString();
                if (Company_Address1.Trim() == "")
                {
                    Company_Address1 = " ";
                }
            }
            else if (KeysData == "Company_Address2")
            {
                Company_Address2 = row["Value"].ToString();
                if (Company_Address2.Trim() == "")
                {
                    Company_Address2 = " ";
                }
            }
            else if (KeysData == "Company_Fax")
            {
                Company_Fax = row["Value"].ToString();
                if (Company_Fax.Trim() == "")
                {
                    Company_Fax = " ";
                }
            }
            else if (KeysData == "Company_PostOffice")
            {
                Company_PostOffice = row["Value"].ToString();
                if (Company_PostOffice.Trim() == "")
                {
                    Company_PostOffice = " ";
                }
            }
            else if (KeysData == "Company_footerAddressData")
            {
                Company_footerAddressData = row["Value"].ToString();
                if (Company_footerAddressData.Trim() == "")
                {
                    Company_footerAddressData = " ";
                }
            }
            else if (KeysData == "Company_Tax_Invoice_URL_1")
            {
                Company_Tax_Invoice_URL_1 = row["Value"].ToString();
            }
            else if (KeysData == "Company_Tax_Invoice_URL_2")
            {
                Company_Tax_Invoice_URL_2 = row["Value"].ToString();
            }
            else if (KeysData == "Company_Tax_Invoice_URL_UserName")
            {
                Company_Tax_Invoice_URL_UserName = row["Value"].ToString();
            }
            else if (KeysData == "Company_Tax_Invoice_URL_Password")
            {
                Company_Tax_Invoice_URL_Password = row["Value"].ToString();
            }
            else if (KeysData == "InvoiceNo_System")
            {
                InvoiceNo_System = row["Value"].ToString();
            }
            else if (KeysData == "Backgroundimage_Name")
            {
                Backgroundimage_Name = row["Value"].ToString();
            }
            else if (KeysData == "Is_Ecrypted")
            {
                Is_Ecrypted_Config = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Is_Ecrypted_Config")
            {
                Is_Ecrypted_Config = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Is_Ecrypted_Menu")
            {
                Is_Ecrypted_Menu = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_ATS05_Value_OnInvoicePrint")
            {
                Show_ATS05_Value_OnInvoicePrint = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_TaxSale_Value_OnInvoicePrint")
            {
                Show_TaxSale_Value_OnInvoicePrint = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_Commision_Value_OnInvoicePrint")
            {
                Show_Commision_Value_OnInvoicePrint = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_NetInvoiceTotal_OnInvoicePrint")
            {
                Show_NetInvoiceTotal_OnInvoicePrint = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_TotalQnty_OnInvoicePrint")
            {
                Show_TotalQnty_OnInvoicePrint = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_TotalAfterTax_OnInvoicePrint")
            {
                Show_TotalAfterTax_OnInvoicePrint = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_InvoicePaymentValuesOnInvoicePrint")
            {
                Show_InvoicePaymentValuesOnInvoicePrint = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_InvoicePaymentValues_StillsOnInvoicePrint")
            {
                Show_InvoicePaymentValues_StillsOnInvoicePrint = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_TotalBeforTax_OnInvoicePrint")
            {
                Show_TotalBeforTax_OnInvoicePrint = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_Prices_Value_OnOrderPurchaseMonitor")
            {
                Show_Prices_Value_OnOrderPurchaseMonitor = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_Taxes_Value_OnOrderPurchaseMonitor")
            {
                Show_Taxes_Value_OnOrderPurchaseMonitor = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "IsShowCarWorkOrderDataOnFormStore")
            {
                IsShowCarWorkOrderDataOnFormStore = int.Parse(row["Value"].ToString());
            }


            else if (KeysData == "Is_ActiveOrNot_Show_ColumnInvoiceMonitor")
            {
                Is_ActiveOrNot_Show_ColumnInvoiceMonitor = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_Item_ATS05_OnInvoiceMonitor")
            {
                Show_Item_ATS05_OnInvoiceMonitor = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_StatementNo_Value_OnInvoiceMonitor")
            {
                Show_StatementNo_Value_OnInvoiceMonitor = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_StoreName_OnInvoiceMonitor")
            {
                Show_StoreName_OnInvoiceMonitor = int.Parse(row["Value"].ToString());
            }


            else if (KeysData == "Show_OperstionNo_Value_OnInvoiceMonitor")
            {
                Show_OperstionNo_Value_OnInvoiceMonitor = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_Merchant_Name_OnInvoiceMonitor")
            {
                Show_Merchant_Name_OnInvoiceMonitor = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_Tash3ela_OnInvoiceMonitor")
            {
                Show_Tash3ela_OnInvoiceMonitor = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_ItemsQnty_Free_OnInvoiceMonitor")
            {
                Show_ItemsQnty_Free_OnInvoiceMonitor = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_Item_No_Free_OnInvoiceMonitor")
            {
                Show_Item_No_Free_OnInvoiceMonitor = int.Parse(row["Value"].ToString());
            }

            else if (KeysData == "Show_Qnty_Value_OnInvoiceMonitor")
            {
                Show_Qnty_Value_OnInvoiceMonitor = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_columnTotalNoTax_OnInvoiceMonitor")
            {
                Show_columnTotalNoTax_OnInvoiceMonitor = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_Alternate_Commission_OnInvoiceMonitor")
            {
                Show_Alternate_Commission_OnInvoiceMonitor = int.Parse(row["Value"].ToString());
            }

            else if (KeysData == "Show_StoreName_On_Bill_Monitor")
            {
                Show_StoreName_On_Bill_Monitor = int.Parse(row["Value"].ToString());
            }

            else if (KeysData == "Show_Qnty_Value_On_Bill_Monitor")
            {
                Show_Qnty_Value_On_Bill_Monitor = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_columnTotalNoTax_On_Bill_Monitor")
            {
                Show_columnTotalNoTax_On_Bill_Monitor = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_Alternate_Commission_On_Bill_Monitor")
            {
                Show_Alternate_Commission_On_Bill_Monitor = int.Parse(row["Value"].ToString());
            }

            else if (KeysData == "Is_PriceByTenns_On_Bill_Monitor")
            {
                Is_PriceByTenns_On_Bill_Monitor = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_Item_Expire_Date_OnInvoiceMonitor")
            {
                Show_Item_Expire_Date_OnInvoiceMonitor = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Is_Item_Expire_Date_WithBalance")
            {
                Is_Item_Expire_Date_WithBalance = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Is_Tash3ela_WithBalance")
            {
                Is_Tash3ela_WithBalance = int.Parse(row["Value"].ToString());
            }

            else if (KeysData == "Is_Bills_DetailsId_WithUnit_Item_ExpireDate")
            {
                Is_Bills_DetailsId_WithUnit_Item_ExpireDate = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Is_P_Price_WithUnit_Item_ExpireDate")
            {
                Is_P_Price_WithUnit_Item_ExpireDate = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "ColumnItemsName_Invoice")
            {
                ColumnItemsName_Invoice = row["Value"].ToString();
            }
            else if (KeysData == "Show_Commision_Value_OnInvoiceMonitor")
            {
                Show_Commision_Value_OnInvoiceMonitor = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_3Mola_Value_OnInvoiceMonitor")
            {
                Show_3Mola_Value_OnInvoiceMonitor = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "IsCompute_3Mola_Value_OnInvoiceMonitor")
            {
                IsCompute_3Mola_Value_OnInvoiceMonitor = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Is_Div_100Or_1_Rate")
            {
                Is_Div_100Or_1_Rate = int.Parse(row["Value"].ToString());
            }

            else if (KeysData == "_3Mola_Value")
            {
                _3Mola_Value = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "IsAddAllItemInAllStores")
            {
                IsAddAllItemInAllStores = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "IsAddAllItemForAllCars")
            {
                IsAddAllItemForAllCars = int.Parse(row["Value"].ToString());
            }

            else if (KeysData == "Show_NetInvoiceTotal_OnInvoiceMonitor")
            {
                Show_NetInvoiceTotal_OnInvoiceMonitor = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_ItemNo_OnInvoiceMonitor")
            {
                Show_ItemNo_OnInvoiceMonitor = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Enabled_ItemNo_OnItemData")
            {
                Enabled_ItemNo_OnItemData = int.Parse(row["Value"].ToString());
            }


            else if (KeysData == "Is_ActiveOrNot_Show_Column_Bill_Monitor")
            {
                Is_ActiveOrNot_Show_Column_Bill_Monitor = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_Commision_Value_On_Bill_Monitor")
            {
                Show_Commision_Value_On_Bill_Monitor = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_ItemNo_On_Bill_Monitor")
            {
                Show_ItemNo_On_Bill_Monitor = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_Item_TaxSale_On_Bill_Monitor")
            {
                Show_Item_TaxSale_On_Bill_Monitor = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_Item_ATS05_On_Bill_Monitor")
            {
                Show_Item_ATS05_On_Bill_Monitor = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Is_Tax_Per_Item_Bill_")
            {
                Is_Tax_Per_Item_Bill_ = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_StatementNo_Value_On_Bill_Monitor")
            {
                Show_StatementNo_Value_On_Bill_Monitor = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_OperstionNo_Value_On_Bill_Monitor")
            {
                Show_OperstionNo_Value_On_Bill_Monitor = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_Merchant_Name_On_Bill_Monitor")
            {
                Show_Merchant_Name_On_Bill_Monitor = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_Qnty_With_Unit_Bill_Show_Qnty_With_Unit")
            {
                Show_Qnty_With_Unit_Bill_Show_Qnty_With_Unit = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_Item_Expire_Date_On_Bill_Monitor")
            {
                Show_Item_Expire_Date_On_Bill_Monitor = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_Tash3ela_On_Bill_Monitor")
            {
                Show_Tash3ela_On_Bill_Monitor = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_ItemsQnty_Free_On_Bill_Monitor")
            {
                Show_ItemsQnty_Free_On_Bill_Monitor = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_Item_No_Free_On_Bill_Monitor")
            {
                Show_Item_No_Free_On_Bill_Monitor = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_Audience_price_On_Bill_Monitor")
            {
                Show_Audience_price_On_Bill_Monitor = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "ColumnItemsName__Bill_")
            {
                ColumnItemsName__Bill_ = row["Value"].ToString();
            }
            else if (KeysData == "ColumnUnitPriceName_Bill")
            {
                ColumnUnitPriceName_Bill = row["Value"].ToString();
            }
            else if (KeysData == "Static_NotesInInvoicePrint")
            {
                Static_NotesInInvoicePrint = row["Value"].ToString();
            }
            else if (KeysData == "Static_NotesInInvoicePrintNotShowCustomerAccount")
            {
                Static_NotesInInvoicePrintNotShowCustomerAccount = row["Value"].ToString();
            }
            else if (KeysData == "CharToSeparateQntyLeft")
            {
                CharToSeparateQntyLeft = row["Value"].ToString();
            }
            else if (KeysData == "CharToSeparateQntyRight")
            {
                CharToSeparateQntyRight = row["Value"].ToString();
            }
            else if (KeysData == "Show_Bank_Name")
            {
                Show_Bank_Name = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "PrintInvoice_Size")
            {
                PrintInvoice_Size = row["Value"].ToString();
            }
            else if (KeysData == "PrintBill_Size")
            {
                PrintBill_Size = row["Value"].ToString();
            }

            else if (KeysData == "CommissionTextOnInvoice")
            {
                CommissionTextOnInvoice = row["Value"].ToString();
            }

            else if (KeysData == "OrderDetailsPrint")
            {
                OrderDetailsPrint = row["Value"].ToString();
            }

            else if (KeysData == "OrderMoneyPrint")
            {
                OrderMoneyPrint = row["Value"].ToString();
            }
            else if (KeysData == "ItemNoVisibility")
            {
                ItemNoVisibility = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Demo_Verion")
            {
                Demo_Verion = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Demo_VerionInvoiceMaxCount")
            {
                Demo_VerionInvoiceMaxCount = int.Parse(row["Value"].ToString());
            }

            else if (KeysData == "Enable_ItemNoOnInvoice")
            {
                Enable_ItemNoOnInvoice = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "DefaultEntryLanguah")
            {
                DefaultEntryLanguah = row["Value"].ToString();
            }
            else if (KeysData == "IS_Make_Sarf_Item_BalanceFromInvoice")
            {
                IS_Make_Sarf_Item_BalanceFromInvoice = int.Parse(row["Value"].ToString());
            }

            else if (KeysData == "Show_ItemCommissionTotal_OnInvoiceMonitor")
            {
                Show_ItemCommissionTotal_OnInvoiceMonitor = int.Parse(row["Value"].ToString());
            }



            else if (KeysData == "Show_Unit_CommissionValue_OnInvoiceMonitor")
            {
                Show_Unit_CommissionValue_OnInvoiceMonitor = int.Parse(row["Value"].ToString());
            }

            else if (KeysData == "Show_Item_CommissionValue_OnInvoiceMonitor")
            {
                Show_Item_CommissionValue_OnInvoiceMonitor = int.Parse(row["Value"].ToString());
            }

            else if (KeysData == "Show_Item_CommissionRate_OnInvoiceMonitor")
            {
                Show_Item_CommissionRate_OnInvoiceMonitor = int.Parse(row["Value"].ToString());
            }

            else if (KeysData == "Show_Alternate_Item_CommissionRate_OnInvoiceMonitor")
            {
                Show_Alternate_Item_CommissionRate_OnInvoiceMonitor = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_Alternate_Unit_CommissionValue_OnInvoiceMonitor")
            {
                Show_Alternate_Unit_CommissionValue_OnInvoiceMonitor = int.Parse(row["Value"].ToString());
            }

            else if (KeysData == "Show_Alternate_Item_CommissionValue_OnInvoiceMonitor")
            {
                Show_Alternate_Item_CommissionValue_OnInvoiceMonitor = int.Parse(row["Value"].ToString());
            }

            else if (KeysData == "Show_Alternate_ItemCommissionTotal_OnInvoiceMonitor")
            {
                Show_Alternate_ItemCommissionTotal_OnInvoiceMonitor = int.Parse(row["Value"].ToString());
            }





            else if (KeysData == "Show_ItemsCode_On_Bill_Monitor")
            {
                Show_ItemsCode_On_Bill_Monitor = int.Parse(row["Value"].ToString());
            }


            else if (KeysData == "Show_ItemsCode_OnInvoiceMonitor")
            {
                Show_ItemsCode_OnInvoiceMonitor = int.Parse(row["Value"].ToString());
            }



            else if (KeysData == "Show_Alternate_Item_CommissionRate_On_Bill_Monitor")
            {
                Show_Alternate_Item_CommissionRate_On_Bill_Monitor = int.Parse(row["Value"].ToString());
            }


            else if (KeysData == "Show_Alternate_Unit_CommissionValue_On_Bill_Monitor")
            {
                Show_Alternate_Unit_CommissionValue_On_Bill_Monitor = int.Parse(row["Value"].ToString());
            }

            else if (KeysData == "Show_Alternate_Item_CommissionValue_On_Bill_Monitor")
            {
                Show_Alternate_Item_CommissionValue_On_Bill_Monitor = int.Parse(row["Value"].ToString());
            }




            else if (KeysData == "Show_Alternate_ItemCommissionTotal_On_Bill_Monitor")
            {
                Show_Alternate_ItemCommissionTotal_On_Bill_Monitor = int.Parse(row["Value"].ToString());
            }


            else if (KeysData == "Show_ItemCommissionTotal_On_Bill_Monitor")
            {
                Show_ItemCommissionTotal_On_Bill_Monitor = int.Parse(row["Value"].ToString());
            }


            else if (KeysData == "Show_Unit_CommissionValue_On_Bill_Monitor")
            {
                Show_Unit_CommissionValue_On_Bill_Monitor = int.Parse(row["Value"].ToString());
            }




            else if (KeysData == "Show_Item_CommissionValue_On_Bill_Monitor")
            {
                Show_Item_CommissionValue_On_Bill_Monitor = int.Parse(row["Value"].ToString());
            }


            else if (KeysData == "Show_Item_CommissionRate_On_Bill_Monitor")
            {
                Show_Item_CommissionRate_On_Bill_Monitor = int.Parse(row["Value"].ToString());
            }
             
            else if (KeysData == "Show_Item_TaxSale10_Rate_On_Bill_Monitor")
            {
                Show_Item_TaxSale10_Rate_On_Bill_Monitor = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_Item_TaxSale10_Value_On_Bill_Monitor")
            {
                Show_Item_TaxSale10_Value_On_Bill_Monitor = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_Item_ATS05_Rate_On_Bill_Monitor")
            {
                Show_Item_ATS05_Rate_On_Bill_Monitor = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_Item_ATS05_Value_On_Bill_Monitor")
            {
                Show_Item_ATS05_Value_On_Bill_Monitor = int.Parse(row["Value"].ToString());
            }

            else if (KeysData == "Show_Item_TaxSale10_Rate_OnInvoiceMonitor")
            {
                Show_Item_TaxSale10_Rate_OnInvoiceMonitor = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_Item_TaxSale10_Value_OnInvoiceMonitor")
            {
                Show_Item_TaxSale10_Value_OnInvoiceMonitor = int.Parse(row["Value"].ToString());
            }

            else if (KeysData == "Show_Item_TaxSale10_OnInvoiceMonitor")
            {
                Show_Item_TaxSale10_OnInvoiceMonitor = int.Parse(row["Value"].ToString());
            }

            else if (KeysData == "Show_Item_ATS05_Rate_OnInvoiceMonitor")
            {
                Show_Item_ATS05_Rate_OnInvoiceMonitor = int.Parse(row["Value"].ToString());
            }
            else if (KeysData == "Show_Item_ATS05_Value_OnInvoiceMonitor")
            {
                Show_Item_ATS05_Value_OnInvoiceMonitor = int.Parse(row["Value"].ToString());
            }

            else if (KeysData == "UseSarfEstlamFromFromReservation")
            {
                try
                {
                    UseSarfEstlamFromFromReservation = int.Parse(row["Value"].ToString());
                }
                catch
                {
                }
            }

        }
        public static decimal RoundMe(decimal? D, int DPlaces = -1)
        {
            decimal V = D ?? 0;
            return RoundMe(V, DPlaces);
        }
       
        /*
        public static decimal RoundMe(decimal D, int DPlaces = -1)
        {
            int decimalPlaces = DecimalPlaces;
            if (DPlaces > -1)
            {
                decimalPlaces = DPlaces;
            }
            else
            {
                decimalPlaces = DecimalPlaces;
            }


            try
            {

                if (DPlaces == -1)
                {
                    if (Common.DecimalPlaces_NoFractionIfZero == 0)
                    {
                        D = RoundDecimal(D, DPlaces);
                        decimal x = Math.Floor(D);
                        decimal DD = Math.Floor(D);
                        if (DD > x)
                        {

                             DD = Math.Round(DD, decimalPlaces);
                        }
                        else
                        {

                             DD = Math.Round(DD, 0);
                        }
                    }
                    else
                    {
                         D = Math.Round(D, decimalPlaces );

                    }
                }
                else
                {
                    decimal x = Math.Floor(D);
                    decimal DD = D;



                    if (DD > x)
                    {

                        DD = Math.Round(DD, decimalPlaces);
                    }
                    else
                    {

                         DD = Math.Round(DD, 0);
                    }

                }
            }
            catch { }
            return D;
        }
       */

        public static decimal RoundMe(decimal D, int DPlaces = -1)
        {
            decimal d = 0;
            int decimalPlaces = DecimalPlaces;
            if (DPlaces > -1)
            {
                decimalPlaces = DPlaces;
            }
            else
            {
                decimalPlaces = DecimalPlaces;
            }


            try
            {


                if (Common.DecimalPlaces_NoFractionIfZero == 0)
                {

                    if (D % 1 == 0)
                    {
                        d = ((int)D); // No fraction, return as integer string
                    }
                    else
                    {
                        d = Math.Round(D, decimalPlaces); // Round to 2 decimal places
                                                          //d = RoundOrTruncate(D, decimalPlaces);

                    }
                }
                else
                {
                    d = Math.Round(D, decimalPlaces);
                    //d = RoundOrTruncate(D, decimalPlaces);

                }


            }
            catch { }
            return d;
        }
        static decimal RoundOrTruncate(decimal value, int decimalPlaces = -1)
        {
            decimal d = 0;
            int decimalPlacesPlusOne = decimalPlaces + 1;
            d = Math.Round(value, decimalPlacesPlusOne);
            //d = TruncateDecimal(value, decimalPlaces);


            //if (Common.decimalValueRoundOrTruncate == 1)
            //{
            //    d = Math.Round(value, decimalPlaces);
            //}
            //else
            //{
            //    d = TruncateDecimal(value, decimalPlaces);
            //}
            return d;
        }
        static decimal TruncateDecimal(decimal value, int precision)
        {
            decimal d = 0;
            decimal factor = (decimal)Math.Pow(10, precision);
            d = Math.Truncate(value * factor) / factor;

            string C = d.ToString($"F{precision}");

            return d;

        }

        public static string RoundMeStr(string D)
        {
            int Dplaces = DecimalPlaces;
            if(D=="")
            { D = "0"; }
            decimal x = 0;
            try
            {
                x = decimal.Parse(D);
                decimal fraction = x - Math.Truncate(x);
                if (fraction == 0)
                {
                    Dplaces = 0;
                }

                x = RoundMe(x, Dplaces);
                if (x < 0)
                {
                    x = x * -1;
                }
            }
            catch { x = 0; }
            return x.ToString();
        }



        public static double RoundMe(double D, int DPlaces = -1)
        {
            Convert.ToDecimal(D);

            return Convert.ToDouble(RoundMe(Convert.ToDecimal(D), DPlaces));
        }
        public static decimal RoundMePositive(decimal D, int DPlaces = -1)
        {
            decimal x = D;
            try
            {

                if (x < 0)
                {
                    x = x * -1;
                }
                x = RoundMe(x, DPlaces);
            }
            catch { x = 0; }
            return x;
        }
        public static decimal? RoundMePositive(decimal? D, int DPlaces = -1)
        {
            D = D ?? 0;
            
            decimal? x = D;
            try
            {

                if (x < 0)
                {
                    x = x * -1;
                }
                x = RoundMe(x.Value, DPlaces);
            }
            catch { x = 0; }
            return x;
        }
        //public static decimal RoundMe(decimal D)
        //{
        //    decimal x = Math.Floor(D);


        //    try
        //    {
        //        if (D > x)
        //        {
        //            D = RoundMe(D, DecimalPlaces);
        //        }
        //        else
        //        {
        //            D = RoundMe(D, 0);
        //        }

        //    }
        //    catch { D = 0; }
        //    return D;
        //}

        //public static float RoundMe(float V)
        //{
        //    double d = double.Parse(V.ToString());
        //    d = Math.Round(d, DecimalPlaces);
        //    float x = (float)d;
        //    return x;


        //}


        public static DataTable dtBanksPOS;


        public static void GetBanksPOS()
        {
            dtBanksPOS = new DataTable();
            BanksClss bb = new BanksClss();
            dtBanksPOS = bb.Get_AllBanks(0);
        }

        public static void SetItemPrices(ref Items im, ref Unit_item um)
        {
            //////////////
            //um.S_price_LargeUnit = um.S_price;// txtS_price_BigUnit.Value;
            //um.S_price_SmallUnit = um.S_price;// txtS_price_SmallUnit.Value;
            //um.P_price_LargeUnit = um.P_price;// txtP_price_BigUnit.Value;
            //um.P_price_SmallUnit = um.P_price;//  txtP_price_SmallUnit.Value;
            //um.S_price_Item_No = 0;// txtS_price_ItemNo.Value;
            //um.P_price_Item_No = 0;// txtP_price_ItemNo.Value;



            if (um.Small_unit == 1)
            {

                im.S_price_SmallUnit = im.S_price_LargeUnit;
                im.P_price_SmallUnit = im.P_price_LargeUnit;

                um.S_price_SmallUnit = um.S_price_LargeUnit;
                um.P_price_SmallUnit = um.P_price_LargeUnit;

            }
            if (um.Unit_Used_For_P_price_Default == 1)
            {
                um.P_price = um.P_price_LargeUnit;
                im.P_Price = um.P_price_LargeUnit;
            }
            else if (um.Unit_Used_For_P_price_Default == 2)
            {
                um.P_price = um.P_price_SmallUnit;
                im.P_Price = um.P_price_SmallUnit;
            }
            else //(im.um.Unit_Used_For_P_price_Default == 3)
            {
                um.P_price = um.P_price_Item_No;
                im.P_Price = um.P_price_Item_No;
            }

            if (um.Unit_Used_For_S_price_Default == 1)
            {
                um.S_price = um.S_price_LargeUnit;
                im.S_Price = um.S_price_LargeUnit;
            }
            else if (um.Unit_Used_For_S_price_Default == 2)
            {
                um.S_price = um.S_price_SmallUnit;
                im.S_Price = um.S_price_SmallUnit;
            }
            else //(im.um.Unit_Used_For_S_price_Default == 3)
            {
                um.S_price = um.S_price_Item_No;
                im.S_Price = um.S_price_Item_No;
            }


        }
        public static void GetCustomerLabelAccount(ref string Label, ref decimal BalanceValue, ref int mySign)
        {
            if (BalanceValue >= 0)
            {
                mySign = 1;
                Label = "مدين";

            }
            else
            {
                mySign = -1;
                Label = "دائن";
                BalanceValue *= -1;

            }
        }
        public static void GetSupplierLabelAccount(ref string Label, ref decimal BalanceValue, ref int mySign)
        {
            if (BalanceValue < 0)
            {
                mySign = -1;
                Label = "مدين";
                BalanceValue = BalanceValue * -1;


            }
            else
            {
                mySign = 1;
                Label = "دائن";


            }
        }
        public static DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);
            //Get all the properties by using reflection   
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names  
                dataTable.Columns.Add(prop.Name);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {

                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }

            return dataTable;
        }

        public static DateTime? CheckValidDate(DateTime value)
        {
            DateTime dt = new DateTime(0001, 01, 01);
            if (value == dt)
            {
                dt = DateTime.Now.Date;

            }
            return dt;
        }

        public static bool CheckCloudDataForPortal()
        {


            bool IsValid = true;
            if (Common.IsCheckRenewalForTaxPortalCloud == 1)
            {
                if (Common.IsNeedToRenewalForTaxPortalCloud == 0)
                {
                    try
                    {
                        DateTime RenewalDate = DateTime.Parse(Common.RenewalDateForTaxPortalCloud);
                        DateTime dat = DateTime.Now.Date;
                        if (dat > RenewalDate)
                        {
                            string KeysData = "IsNeedToRenewalForTaxPortalCloud";
                            string NewValue = "1";
                            Common.Save_SystemParametersDB(KeysData, NewValue);

                            IsValid = false;
                        }
                        else
                        {
                            string KeysData = "IsNeedToRenewalForTaxPortalCloud";
                            string NewValue = "0";
                            Common.Save_SystemParametersDB(KeysData, NewValue);

                            IsValid = false;
                        }
                    }
                    catch
                    {

                    }
                }
                else if (Common.IsNeedToRenewalForTaxPortalCloud < 100)
                {
                    Common.IsNeedToRenewalForTaxPortalCloud = Common.IsNeedToRenewalForTaxPortalCloud + 1;
                    string KeysData = "IsNeedToRenewalForTaxPortalCloud";
                    string NewValue = Common.IsNeedToRenewalForTaxPortalCloud.ToString();
                    Common.Save_SystemParametersDB(KeysData, NewValue);

                    IsValid = false;
                }
                else
                {

                }
            }
            else
            {
                Common.IsNeedToRenewalForTaxPortalCloud = 0;
            }
            return IsValid;
        }
        public static DataTable GetImageLogo()
        {
            DataTable dtimage = new DataTable();
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            string mysql = "Select * from Account_BranchesimagesLogo ";
            dtimage = opp.return_DataTable(mysql);

            return dtimage;
        }
        public static string GetLogo(string CompanyLogoType)
        {
            string mimage = "";
            //MemoryStream ms = new MemoryStream();
            //if (CompanyLogoType == "1") // new sky
            //{
            //    Bitmap bmp = new Bitmap(global::  Properties.Resources.NewSky);
            //    bmp.Save(ms, ImageFormat.Bmp);
            //}
            //if (CompanyLogoType == "2") // fawzan
            //{
            //    Bitmap bmp = new Bitmap(global::  Properties.Resources.fawzan);
            //    bmp.Save(ms, ImageFormat.Bmp);
            //}
            //if (CompanyLogoType == "0") //  
            //{
            //    Bitmap bmp = new Bitmap(global::  Properties.Resources.Empty);
            //    bmp.Save(ms, ImageFormat.Bmp);
            //}

            //mimage = Convert.ToBase64String(ms.ToArray());
            return mimage;
        }

        public static bool IsMay_ShowPricesOnOrderPurchase(int order_Type)
        {
            bool IsMay_ShowPrices = false;
            //if (order_Type < 2 | order_Type > 5)
            //{
            //    IsMay_ShowPrices = false;
            //}
            //else
            //{
            //    IsMay_ShowPrices = true;
            //}

            if (order_Type == (int)StoreOrderType.EstlamFromSupplier_4 | order_Type == (int)StoreOrderType.MortagaToSupplier_5)
            {
                IsMay_ShowPrices = true;
            }
            else if (order_Type == (int)StoreOrderType.SarfToWarsha_18 | order_Type == (int)StoreOrderType.MortagaSarfFromWarsha_19)
            {
                IsMay_ShowPrices = true;
            }
            else
            {
                IsMay_ShowPrices = false;
            }
            return IsMay_ShowPrices;
        }
        public static string GetInvoiceTypeAgile(int Is_Invoice_Agile)
        {
            string Is_Invoice_AgileStr = "";
            if (Is_Invoice_Agile == 1)
            {
                Is_Invoice_AgileStr = "فاتورة أجل";
            }
            else
            {
                Is_Invoice_AgileStr = "فاتورة نقدي";
            }

            return Is_Invoice_AgileStr;
        }
        public static void Compute_3Mola_Value(ref Invoices_Details invD)
        {
            invD.Unit3Mola = invD.Unit3Mola ?? 0;
            invD.Qnty_Another = invD.Qnty_Another ?? 0;
            if (Common.IsCompute_3Mola_Value_OnInvoiceMonitor == 1)
            {
                if (invD.Unit3Mola == 0)
                {
                    invD.Unit3Mola = Common._3Mola_Value;
                }


                invD.Total3Mola = invD.Qnty_Another * invD.Unit3Mola;
            }
            else
            {
                //Common._3Mola_Value;
                invD.Unit3Mola = 0;
                invD.Total3Mola = 0;

            }
        }
        public static void Compute_3Mola_Value(ref IInvoices_Details invD)
        {
            if (Common.IsCompute_3Mola_Value_OnInvoiceMonitor == 1)
            {
                invD.Unit3Mola = Common._3Mola_Value;
                invD.Qnty_Another = invD.Qnty_Another ?? 0;
                invD.Unit3Mola = invD.Unit3Mola ?? 0;

                invD.Total3Mola = invD.Qnty_Another * invD.Unit3Mola;
            }
            else
            {
                //Common._3Mola_Value;
                invD.Unit3Mola = 0;
                invD.Total3Mola = 0;

            }
        }
        public static BatchSelection GetBatchSelection(Unit_Item_ExpireDateVM unit_Item_ExpireDateVM)
        {
            BatchSelection batchSelection = new BatchSelection();

            batchSelection.Supplier_Id = unit_Item_ExpireDateVM.Supplier_Id ?? 0;
            batchSelection.Id = unit_Item_ExpireDateVM.Id;
            batchSelection.Item_Id = unit_Item_ExpireDateVM.Id;
            //batchSelection.Item_Name = $"{unit_Item_ExpireDateVM.OperationDate.Value.ToString("yyyy/MM/dd")} - {unit_Item_ExpireDateVM.Item_Name} - {unit_Item_ExpireDateVM.SupplierName}";
            batchSelection.Item_Name = unit_Item_ExpireDateVM.ShowFullName;
            batchSelection.Larg_Balance = unit_Item_ExpireDateVM.Larg_Balance;
            batchSelection.Item_No = unit_Item_ExpireDateVM.Item_No;
            batchSelection.P_Price = unit_Item_ExpireDateVM.P_Price;
            batchSelection.QntyValue = unit_Item_ExpireDateVM.QntyValue;


            return batchSelection;
        }
        public static BatchSelection GetBatchSelection(Unit_Item_BatchGroupListVM unit_Item_BatchGroupList)
        {
            BatchSelection batchSelection = new BatchSelection();

            batchSelection.Supplier_Id = unit_Item_BatchGroupList.Supplier_Id ?? 0;
            batchSelection.Id = unit_Item_BatchGroupList.Id;
            batchSelection.Item_Id = unit_Item_BatchGroupList.Id;
            //batchSelection.Item_Name = $"{unit_Item_BatchGroupList.OperationDate.Value.ToString("yyyy/MM/dd")} - {unit_Item_BatchGroupList.Item_Name} - {unit_Item_BatchGroupList.SupplierName}";
            batchSelection.Item_Name = unit_Item_BatchGroupList.GroupName;



            return batchSelection;
        }
        public static void SetValuesDecimals(ref IInvoices_List invL)
        {
            invL.Item_Net_TotalWorkmanship = Common.RoundMePositive(invL.Item_Net_TotalWorkmanship);
            invL.CommissionListWorkmanship_Rate = Common.RoundMePositive(invL.CommissionListWorkmanship_Rate);
            invL.Item_Net_TotalSpareParts = Common.RoundMePositive(invL.Item_Net_TotalSpareParts);
            invL.Item_Net_TotalOils = Common.RoundMePositive(invL.Item_Net_TotalOils);
            invL.Item_Net_TotalSparePartsOils = Common.RoundMePositive(invL.Item_Net_TotalSparePartsOils);
            invL.CommissionListSpareParts_Rate = Common.RoundMePositive(invL.CommissionListSpareParts_Rate);
            invL.CommissionListOils_Rate = Common.RoundMePositive(invL.CommissionListOils_Rate);
            ////+
            invL.CommissionListSparePartsOils_Rate = Common.RoundMePositive(invL.CommissionListSparePartsOils_Rate);
            invL.CommissionListWorkmanship_Value = Common.RoundMePositive(invL.CommissionListWorkmanship_Value);
            invL.CommissionListSpareParts_Value = Common.RoundMePositive(invL.CommissionListSpareParts_Value);
            invL.CommissionListOils_Value = Common.RoundMePositive(invL.CommissionListOils_Value);
            invL.CommissionListSparePartsOils_Value = Common.RoundMePositive(invL.CommissionListSparePartsOils_Value);

            invL.Total = Common.RoundMePositive(invL.Total);


            invL.Net_Total = Common.RoundMePositive(invL.Net_Total);



            invL.List_CommissionRate = Common.RoundMePositive(invL.List_CommissionRate);

            invL.Total_CommissionListWorkmanship = Common.RoundMePositive(invL.Total_CommissionListWorkmanship);
            invL.Total_CommissionListSpareParts = Common.RoundMePositive(invL.Total_CommissionListSpareParts);
            invL.Total_CommissionListOils = Common.RoundMePositive(invL.Total_CommissionListOils);
            invL.Total_CommissionListSparePartsOils = Common.RoundMePositive(invL.Total_CommissionListSparePartsOils);
            invL.Total_CommissionList = Common.RoundMePositive(invL.Total_CommissionList);

            invL.List_CommissionValue = Common.RoundMePositive(invL.List_CommissionValue);
            invL.List_Final_CommissionValue = Common.RoundMePositive(invL.List_Final_CommissionValue);

            ////////////
            invL.Net_Total_CommissionValue = Common.RoundMePositive(invL.Net_Total_CommissionValue);
            invL.Total_ListPriceWithoutTax_Workmanship = Common.RoundMePositive(invL.Total_ListPriceWithoutTax_Workmanship);
            invL.Total_ListPriceWithoutTax_SpareParts = Common.RoundMePositive(invL.Total_ListPriceWithoutTax_SpareParts);
            invL.Total_ListPriceWithoutTax_Oils = Common.RoundMePositive(invL.Total_ListPriceWithoutTax_Oils);
            invL.Total_ListPriceWithoutTax_SparePartsOils = Common.RoundMePositive(invL.Total_ListPriceWithoutTax_SparePartsOils);
            invL.Total_ListPriceWithoutTax = Common.RoundMePositive(invL.Total_ListPriceWithoutTax);
            invL.Total_PriceWithoutTax = Common.RoundMePositive(invL.Total_PriceWithoutTax);



            invL.ATS05ListWorkmanship_Value = Common.RoundMePositive(invL.ATS05ListWorkmanship_Value);
            invL.ATS05ListSpareParts_Value = Common.RoundMePositive(invL.ATS05ListSpareParts_Value);
            invL.ATS05ListOils_Value = Common.RoundMePositive(invL.ATS05ListOils_Value);
            ////+
            invL.ATS05ListSparePartsOils_Value = Common.RoundMePositive(invL.ATS05ListSparePartsOils_Value);

            invL.TotalListATS05_Value = Common.RoundMePositive(invL.TotalListATS05_Value);

            invL.ATS05_Value = Common.RoundMePositive(invL.ATS05_Value);
            invL.TaxSales10 = Common.RoundMePositive(invL.TaxSales10);
            invL.ATS05 = Common.RoundMePositive(invL.ATS05);


            invL.TaxSalesListWorkmanship_Value = Common.RoundMePositive(invL.TaxSalesListWorkmanship_Value);
            invL.TaxSalesListSpareParts_Value = Common.RoundMePositive(invL.TaxSalesListSpareParts_Value);
            invL.TaxSalesListOils_Value = Common.RoundMePositive(invL.TaxSalesListOils_Value);
            invL.TaxSalesListSparePartsOils_Value = Common.RoundMePositive(invL.TaxSalesListSparePartsOils_Value);
            invL.TotalListTaxSales10_Value = Common.RoundMePositive(invL.TotalListTaxSales10_Value);
            invL.TaxSales10_Value = Common.RoundMePositive(invL.TaxSales10_Value);

            ////////
            invL.NetInvoiceTotal_Workmanship = Common.RoundMePositive(invL.NetInvoiceTotal_Workmanship);
            invL.NetInvoiceTotal_SpareParts = Common.RoundMePositive(invL.NetInvoiceTotal_SpareParts);
            invL.NetInvoiceTotal_Oils = Common.RoundMePositive(invL.NetInvoiceTotal_Oils);
            invL.NetInvoiceTotal_SparePartsOils = Common.RoundMePositive(invL.NetInvoiceTotal_SparePartsOils);
            invL.NetInvoiceTotal = Common.RoundMePositive(invL.NetInvoiceTotal);

            invL.Equivalent_Net_Total_Price = Common.RoundMePositive(invL.Equivalent_Net_Total_Price);
            invL.Equivalent_Net_Total_CommissionValue = Common.RoundMePositive(invL.Equivalent_Net_Total_CommissionValue);


        }
       
        public static bool IsShowInvoices_List_WithDueDate()
        {
            
            int Force_Admin = 0;
            //return  Common.IsHavePermissionSysMenu(Common.User_Id, 957, Common.Project_ID, Common.Branch_Id,0);
            return Common.IsHavePermissionSysMenu(
                                                    User_ID: Common.User_Id,
                                                    Menu_ID: 957,
                                                    PROJECT_ID: Common.Project_ID,
                                                    Force_Admin: 0);

        }

        public static DataTable SetSystemParameters_Menus_DB(int PROJECT_ID = 0)
        {
             
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            string mysql = @" SELECT SysConfigMenus.KeysData, SysConfigMenus.Description, SysConfigMenusPERMATIONS.*
                                FROM SysConfigMenusPERMATIONS INNER JOIN
                         SysConfigMenus ON SysConfigMenusPERMATIONS.Menu_ID = SysConfigMenus.Id ";
            mysql = mysql + " WHERE (SysConfigMenusPERMATIONS.USER_ID = " + Common.User_Id + ") ";
            if (PROJECT_ID > 0)
            {
                mysql = mysql + " AND (SysConfigMenusPERMATIONS.PROJECT_ID = " + PROJECT_ID + ")";
            }
            // 🟢 IsShowToUser على الجدولين معاً (راجع AlterDB_CLS.AlterDBThisYear): تعريف القائمة نفسها
            // (SysConfigMenus) قد يُخفيها من الجميع، وصف صلاحية المستخدم (SysConfigMenusPERMATIONS) قد
            // يُخفيها لهذا المستخدم تحديداً دون غيره - يجب أن يكون كلاهما = 1 (أو NULL، لصفوف قديمة قبل
            // إضافة العمود) لتظهر القائمة فعلياً فى السايد بار.
            mysql = mysql + " AND (ISNULL(SysConfigMenus.IsShowToUser, 1) = 1) AND (ISNULL(SysConfigMenusPERMATIONS.IsShowToUser, 1) = 1) ";
            mysql = mysql + " ORDER BY SysConfigMenusPERMATIONS.Parent_Id ";
            DataTable dt = opp.return_DataTable(mysql);
            return dt;


        }
        //public static DataTable SetSystemParameters_Menus_DB(int PROJECT_ID = 0)
        //{
        //    AccountModelsLib.operations opp = new AccountModelsLib.operations();
        //    string mysql = @" SELECT SysConfigMenus.KeysData, SysConfigMenus.Description, SysConfigMenusPERMATIONS.*
        //                        FROM SysConfigMenusPERMATIONS INNER JOIN
        //                 SysConfigMenus ON SysConfigMenusPERMATIONS.Menu_ID = SysConfigMenus.Id ";
        //    mysql = mysql + " WHERE (SysConfigMenusPERMATIONS.USER_ID = " + Common.User_Id + ") ";
        //    if (PROJECT_ID > 0)
        //    {
        //        mysql = mysql + " AND (SysConfigMenusPERMATIONS.PROJECT_ID = " + PROJECT_ID + ")";
        //    }
        //    mysql = mysql + " ORDER BY SysConfigMenusPERMATIONS.Parent_Id ";
        //    DataTable dt = opp.return_DataTable(mysql);
        //    return dt;


        //}
    }

}

