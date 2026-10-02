using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountModelsLib.Models
{
    // 🟢 كل الأعمدة "الرقمية" هنا nvarchar فعلياً فى قاعدة البيانات (وليست رقمية) - راجع sys.columns مباشرة
    // بتاريخ 2026-09-29 قبل إنشاء هذا الموديل: الكود الأصلى (Store_LIB.PrintItemsBarCodeSettingFor_User)
    // يكتب كل القيم كنصوص دائماً، فالحفاظ على النوع نفسه هنا إلزامى (لا يجوز "تصحيحه" لأرقام).
    [Table("Item_BarCodeSetting")]
    public class Item_BarCodeSetting
    {
        [Key]
        public long Id { get; set; }
        public string Item_Id { get; set; }
        public string FontHeader { get; set; }
        public string FontFooter { get; set; }
        public string Medium { get; set; }
        public string Center { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
        public string BarCodeHeight { get; set; }
        public string Space_X_Item { get; set; }
        public string Space_Y_Item { get; set; }
        public string TopMargin { get; set; }
        public string LeftMargin { get; set; }
        public string paperWidth { get; set; }
        public string paperHeight { get; set; }
        public string SetPaperSize { get; set; }
        public string PaperTopMargin { get; set; }
        public string PaperLeftMargin { get; set; }
        public string PaperRightMargin { get; set; }
        public string PaperBottomMargin { get; set; }
        public DateTime? OperationDate { get; set; }
        public long? User_Id { get; set; }
        public long? Branch_Id { get; set; }
        public DateTime? Insert_ServerDateTime { get; set; }
        public DateTime? Update_ServerDateTime { get; set; }
        public DateTime? EditOperationDate { get; set; }
        public long? Edit_User_Id { get; set; }
        public long? ProcessNo { get; set; }
        public string TextUnderCode { get; set; }
        public string IsPrintPrice { get; set; }
    }
}
