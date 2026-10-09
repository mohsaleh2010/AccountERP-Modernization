using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountModelsLib.Models
{
    /// <summary>
    /// متوسط تكلفة الصنف لكل فرع (وحدة التقييم — خطة 01 §2.3، قرار م-01-4).
    /// كل التكاليف والكميات بالوحدة الصغرى. يكتبه StockMovementService فقط، وينسخ Avg_Cost إلى Unit_item.P_priceAvgCost لمخازن الفرع.
    /// </summary>
    [Table("Item_Branch_Cost")]
    public class Item_Branch_Cost
    {
        public long Id { get; set; }
        public long Item_Id { get; set; }
        public long Branch_Id { get; set; }
        [Column(TypeName = "decimal(18, 6)")]
        public decimal Avg_Cost { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal Qty_Small { get; set; }
        [Column(TypeName = "decimal(18, 6)")]
        public decimal? Last_Purchase_Price { get; set; }
        public DateTime? Last_Movement_Date { get; set; }
        public DateTime? OperationDate { get; set; }
        public long? User_Id { get; set; }
        public DateTime? Insert_ServerDateTime { get; set; }
        public DateTime? Update_ServerDateTime { get; set; }
        public DateTime? EditOperationDate { get; set; }
        public long? Edit_User_Id { get; set; }
        public long? ProcessNo { get; set; }
    }
}
