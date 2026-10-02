  
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{
    [Table("Detailes_Purshing_Import_Export")]
    public class Detailes_Purshing_Import_Export
    {
        public long Id { get; set; }
        public long Detailes_Purshing_Id { get; set; }
        public long OrderTable_id { get; set; }
        public long Invoice_Id { get; set; }
        public long Invoice_Type_Id { get; set; }
        public long Detail_Invoice_Id { get; set; }
        public long Invoice_No { get; set; }
        public string Invoice_Type_Str { get; set; }
        public long Unit_item_Table_Id { get; set; }
        public decimal? qua { get; set; }
        public decimal? Part_qua { get; set; }
        public decimal? Item_No { get; set; }
        public decimal? Balance_Larg_Qnty { get; set; }
        public decimal? Balance_Small_Qnty { get; set; }
        public decimal? Balance_Item_No { get; set; }
        public string Notes_From_To { get; set; }
        public string Notes { get; set; }
        public long? Machine_Id { get; set; }
        public int? Transaction_Type { get; set; }
        public DateTime? Process_Date { get; set; }
      
        
        public int? IS_To_Converted { get; set; }
        public decimal? qua_Import { get; set; }
        public decimal? Part_Qua_Import { get; set; }
        public decimal? Item_No_Import { get; set; }
        public decimal? Quantity_Import { get; set; }
        public decimal? qua_Export { get; set; }
        public decimal? Part_Qua_Export { get; set; }
        public decimal? Item_No_Export { get; set; }
        public decimal? Quantity_Export { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? Quantity_Balance { get; set; }
   

        public string Notes_Item { get; set; }
        public DateTime? OperationDate { get; set; }
        public long? User_Id { get; set; } 
        public long? Branch_Id { get; set; }
        public DateTime? Insert_ServerDateTime { get; set; }
        public DateTime? Update_ServerDateTime { get; set; }
        public DateTime? EditOperationDate { get; set; }
        public long? Edit_User_Id { get; set; }
        public long? ProcessNo { get; set; }
        public long? CarsWorkOrdersId { get; set; }
        
        public string WorkOrderNumber { get; set; }
        public string CarNumber { get; set; }
        public long? CarsId { get; set; }
        public long? CarsWorkOrdersDetailsId { get; set; }
        public string Notes_To { get; set; }
        public string Notes_From { get; set; }
        public string ProcessName { get; set; }
        public int? ProcessNameId { get; set; }
        public string ProcessNameNumber { get; set; }
        public string Id_FromStr { get; set; }
        public string Id_ToStr { get; set; }

        public int? Type_From { get; set; }

        public int? Type_To { get; set; }
        public long? Id_From { get; set; }
        public long? Id_To { get; set; }

        public long? StoreId { get; set; }

        public string StoreId_Name { get; set; }
        public string Type_From_Name { get;   set; }
        public string Type_To_Name { get;   set; }

        public decimal? PriceUnitImport { get; set; }
        public decimal? Item_Net_Total_PriceImport { get; set; }

        public decimal? PriceUnitExport { get; set; }
        public decimal? Item_Net_Total_PriceExport { get; set; }

        public decimal? S_Price { get; set; }
        public decimal? P_Price { get; set; }
        public decimal? PriceUnit { get; set; }
        public decimal? Qnty_value { get; set; }
        public decimal? Item_Net_Total_Price { get; set; }
        public Nullable<decimal> P_priceAvgCost { get; set; }
        public Nullable<decimal> S_priceAvgCost { get; set; }

        public long? EmployeeId { get; set; }
        public string  EmployeeName { get; set; }

        [ForeignKey("Unit_item_Table_Id")]
        public Unit_item Unit_item { get; set; }

        
        
    }
}
