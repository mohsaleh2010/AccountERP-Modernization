 
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{
    public class Submain_Account
    {
        /// <summary>
        /// المعرف الرئيسي الفريد للحساب (Primary Key)
        /// </summary>
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// اسم الحساب الفرعي / التكليفي
        /// </summary>
        public string SubAccount_Name { get; set; }

        /// <summary>
        /// الرقم التعريفي للحساب الرئيسي الأب المباشر (Foreign Key)
        /// </summary>
        public long Main_Account_Id { get; set; }

        /// <summary>
        /// آخر قيمة للزيادة التلقائية في الترقيم الخاص بالجزء الأخير من رقم الحساب
        /// </summary>
        public long lastPartIncrease { get; set; }

        /// <summary>
        /// نوع العميل/المورد المرتبط بالحساب كقيمة رقمية (Enum / ID)
        /// </summary>
        public int Account_Customer_Type_Value { get; set; }

        /// <summary>
        /// المعرف الخاص بنوع العميل/المورد المرتبط بالكيان
        /// </summary>
        public long? Account_Customer_Type_ID { get; set; } = 0;
        /// <summary>
        /// القيمة الخاصة بتصنيف الحساب ضمن حسابات العملاء العموميين
        /// </summary>
        public int? Account_Type_ValueGeneralCustomer { get; set; } = 0;
        /// <summary>
        /// مستوى الحساب داخل الشجرة المحاسبية (Level 1, Level 2, ...)
        /// </summary>
        public int Account_Level { get; set; }

        /// <summary>
        /// كود الحساب الرقمي (تسلسلي أو داخلي)
        /// </summary>
        public long? Code { get; set; }

        /// <summary>
        /// الجزء الأول أو بادئة كود الحساب (Prefix)
        /// </summary>
        public string FirstSubCode { get; set; }

        /// <summary>
        /// رقم الحساب الفرعي المنسق
        /// </summary>
        public string SubAccount_NO { get; set; } = "";

        /// <summary>
        /// كود الحساب المحاسبي الكامل (مثل: 1101001)
        /// </summary>
        public string? Account_Code { get; set; }

        /// <summary>
        /// يحدد الحساب الرئيسي الأكبر أو الحساب المالي المباشر للميزانية/الأرباح والخسائر
        /// </summary>
        public int Head_Main_Account { get; set; }

        /// <summary>
        /// الرقم التعريفي للعملة المربوطة بالحساب
        /// </summary>
        public long? Currency_Id { get; set; }

        /// <summary>
        /// علم يحدد إمكانية حذف الحساب (1 = قابل للحذف، 0 = غير قابل للحذف)
        /// </summary>
        public int CanDelete { get; set; }

        /// <summary>
        /// يحدد التوجيه المالي للحساب على الحسابات الختامية والقوائم المالية:
        /// (1 = حساب التشغيل،
        /// 2 = حساب المتاجرة،
        /// 3 = أرباح وخسائر،
        /// 4 = ميزانية عمومية)
        /// 
        /// </summary>
        public int Account_Follows { get; set; }

        /// <summary>
        /// تصنيف الحساب في قائمة التدفقات النقدية (تشغيلي، استثماري، تمويلي)
        /// </summary>
        public int CashFlowType { get; set; }

        /// <summary>
        /// ملاحظات أو وصف إضافي للحساب
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// علم يحدد هل يقبل الحساب تسجيل قيود مباشرة عليه (1 = يقبل، 0 = لا يقبل)
        /// </summary>
        public int Accept_EntryNew_Flage { get; set; }

        /// <summary>
        /// علم يحدد هل يلزم توجيه هذا الحساب لمراكز تكلفة (1 = نعم، 0 = لا)
        /// </summary>
        public int Accept_Cost_Center_Flage { get; set; }

        /// <summary>
        /// الرقم التعريفي لمركز التكلفة الافتراضي أو الشجرة التابعة له
        /// </summary>
        public long? Cost_Center_Tree_Id { get; set; }

        /// <summary>
        /// طبيعة الحساب المحاسبية (مثل: 1 = مدين، 2 = دائن)
        /// </summary>
        public int Account_Nature_Type { get; set; }

        /// <summary>
        /// علم يحدد هل هذا الحساب في المستوى الأخير من الشجرة (1 = نهائي/تحليلي، 0 = تجميعي/رئيسي)
        /// </summary>
        public int IS_Last_Level { get; set; } = 1;

        /// <summary>
        /// علم يحدد ما إذا كان الحساب ينتمي لنوع حساب فرعي معين
        /// </summary>
        public int IS_SubAccount_AccountType { get; set; }

        /// <summary>
        /// علم يحدد ما إذا كان هذا الحساب يعتبر حساب متابعة فرعي
        /// </summary>
        public int IS_SubAccount_FollowingAccount { get; set; }

        /// <summary>
        /// المعرف الخاص بالفرع التابع له الحساب
        /// </summary>
        public long Branch_Id { get; set; }

        /// <summary>
        /// معرف المستخدم الذي قام بإنشاء أو تعديل الحساب
        /// </summary>
        public long User_Id { get; set; }

        /// <summary>
        /// علم يحدد هل يؤثر الحساب على حسابات المصروفات أو تقارير المصاريف
        /// </summary>
        public int Is_MasrofatAffect { get; set; }

        /// <summary>كود الحساب في المستوى الأول من الهيكل</summary>
        public long? AccountCode1 { get; set; }

        /// <summary>كود الحساب في المستوى الثاني من الهيكل</summary>
        public long? AccountCode2 { get; set; }

        /// <summary>كود الحساب في المستوى الثالث من الهيكل</summary>
        public long? AccountCode3 { get; set; }

        /// <summary>كود الحساب في المستوى الرابع من الهيكل</summary>
        public long? AccountCode4 { get; set; }

        /// <summary>كود الحساب في المستوى الخامس من الهيكل</summary>
        public long? AccountCode5 { get; set; }

        /// <summary>كود الحساب في المستوى السادس من الهيكل</summary>
        public long? AccountCode6 { get; set; }

        /// <summary>
        /// معرف الشركة في نظام الأستاذ العام (General Ledger) للربط بين الشركات Multi-Company
        /// </summary>
        public long? Company_ID_For_GL { get; set; }

        /// <summary>
        /// اسم الشركة في نظام الأستاذ العام
        /// </summary>
        public string? Company_Name_For_GL { get; set; }

        /// <summary>
        /// معرف الفرع المسموح له استخدام أو التعامل على هذا الحساب
        /// </summary>
        public long? Branch_ID_for_UseAcconts { get; set; }

        /// <summary>
        /// علم يحدد حالة الربط مع نظام الأستاذ العام (1 = مرتبط، 0 = غير مرتبط)
        /// </summary>
        public int? ConnectWith_Gl_Flage { get; set; }

        /// <summary>
        /// نوع التحويل أو الترحيل المحاسبي الخاص بالحساب
        /// </summary>
        public int? Transform_Type { get; set; }

        
    }

    /*
   public  class Submain_Account 
    {

          

        [Key]
        public long Id { get; set; }
        public string SubAccount_Name { get; set; }
        public long Main_Account_Id { get; set; }
        public long lastPartIncrease { get; set; }
        public int Account_Customer_Type_Value { get; set; }
        public long? Account_Customer_Type_ID { get; set; } = 0;
        public int Account_Level { get; set; }
        public long? Code { get; set; }
        public string FirstSubCode { get; set; }
        public string SubAccount_NO { get; set; } = "";
        public string? Account_Code { get; set; }
        public int Head_Main_Account { get; set; }
        public long    Currency_Id { get; set; }
        public int CanDelete { get; set; }
        public int Account_Follows { get; set; }
        public int CashFlowType { get; set; }
        
        public string Notes { get; set; }

        public int Accept_EntryNew_Flage { get; set; }
        public int Accept_Cost_Center_Flage { get; set; }

         public long? Cost_Center_Tree_Id { get; set; }
        public int Account_Nature_Type { get; set; }
 
        public int IS_Last_Level { get; set; } = 1;
        public int IS_SubAccount_AccountType { get; set; }
        public int IS_SubAccount_FollowingAccount { get; set; }
        public long Branch_Id { get; set; }
        public long User_Id { get; set; }
        public int Is_MasrofatAffect { get; set; }
        public long? AccountCode1 { get; set; }

        public long? AccountCode2 { get; set; }
        public long? AccountCode3 { get; set; }
        public long? AccountCode4 { get; set; }
        public long? AccountCode5 { get; set; }
        public long? AccountCode6 { get; set; }

         
 
        public long? Company_ID_For_GL { get; set; }
        public string? Company_Name_For_GL { get; set; }

       
        public long? Branch_ID_for_UseAcconts { get; set; }
         
        public int? ConnectWith_Gl_Flage { get; set; }
        public int? Transform_Type { get; set; }

 
        public int? Account_Type_ValueGeneralCustomer { get; set; } = 0;
        
    }
    */
}
