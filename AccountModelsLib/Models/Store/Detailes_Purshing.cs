
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{
    [Table("Detailes_Purshing")]
    public class Detailes_Purshing
    {
        public long Id { get; set; }
        public long OrderTable_id { get; set; }
        public long Id_From { get; set; }
        public long Id_To { get; set; }

        //public long Id_From { get; set; }

        //public long Id_To { get; set; }


        public int? type_From { get; set; }
        public int? Type_To { get; set; }

        public string Type_From_Name { get; set; }

        public string Type_To_Name { get; set; }
        public string Id_FromStr { get; set; }
        public string Id_ToStr { get; set; }
        public long? StoreId { get; set; }

        public string StoreId_Name { get; set; }
        public string Item_Name { get; set; }
        public long Item_Id { get; set; }

        public long Unit_item_Table_Id { get; set; }
        public decimal? qua { get; set; }
        public decimal? Part_qua { get; set; }
        public decimal? Item_No { get; set; }
        public decimal? Import_Qua { get; set; }
        public decimal? Import_Part_Qua { get; set; }
        public decimal? Import_Item_No { get; set; }
        public decimal? Export_Qua { get; set; }
        public decimal? Export_Part_Qua { get; set; }
        public decimal? Export_Item_No { get; set; }

        public decimal? Balance_Id_From_Larg_Qnty { get; set; }
        public decimal? Balance_Id_From_Small_Qnty { get; set; }
        public decimal? Balance_Id_From_Item_No { get; set; }
        public decimal? Balance_Id_To_Larg_Qnty { get; set; }
        public decimal? Balance_Id_To_Small_Qnty { get; set; }
        public decimal? Balance_Id_To_Item_No { get; set; }
        public long To_Store_Unit_Item_Table_Id { get; set; }
        public string Notes_From { get; set; }
        public string Notes_To { get; set; }
        public string Notes { get; set; }
         public long? Machine_Id { get; set; }
        public long Related_Detailes_Purshing_Id { get; set; }
        public int? Transaction_Type { get; set; }
        public decimal? Qua_Befor { get; set; }
        public decimal? Part_qua_Befor { get; set; }
        public decimal? Item_No_Befor { get; set; }
        public decimal? Qua_After { get; set; }
        public decimal? Part_qua_After { get; set; }
        public decimal? Item_No_After { get; set; }
        public decimal? Qnty { get; set; }

        public string QntyStr { get; set; }
        public string Item_NoStr { get; set; }

        public long? Unit_Id { get; set; }

        public string UnitStr { get; set; }
        public string Code { get; set; }

        public string QntyStr_Befor { get; set; }
        public string Item_NoStr_Befor { get; set; }


        public string QntyStr_After { get; set; }
        public string Item_NoStr_After { get; set; }

        public string Notes_Item { get; set; }
        public DateTime? OperationDate { get; set; }
        public long? User_Id { get; set; }
        public long? Branch_Id { get; set; }
        public DateTime? Insert_ServerDateTime { get; set; }
        public DateTime? Update_ServerDateTime { get; set; }
        public DateTime? EditOperationDate { get; set; }
        public long? Edit_User_Id { get; set; }
        public long ProcessNo { get; set; }
        public decimal? S_Price { get; set; }
        public decimal? P_Price { get; set; }

        public Nullable<decimal> P_priceAvgCost { get; set; }
        public Nullable<decimal> S_priceAvgCost { get; set; }

        public decimal? PriceUnit { get; set; }
        public decimal? Qnty_value { get; set; }
        public decimal? Item_Total_WithoutTax { get; set; }

        public decimal? Item_ATS05 { get; set; }
        public decimal? Item_TaxSales10 { get; set; }
        public decimal? Item_ATS05_Value { get; set; }

        public decimal? Item_TaxSales10_Value { get; set; }
        public decimal? Total { get; set; }
        public decimal? Net_Total_Item_Commission { get; set; }
        public decimal? Item_Net_Total_Price { get; set; }
        public decimal? Item_CommissionValue { get; set; }
        public decimal? Unit_CommissionValue { get; set; }
        public decimal? Item_CommissionRate { get; set; }
        public int? Unit_Used_for_Price { get; set; }
        public decimal? Larg_Unit_Price { get; set; }
        public decimal? Small_Unit_Price { get; set; }
        public decimal? ItemCommissionTotal { get; set; }
        public int? Item_DeletedFlage { get; set; }
        /// <summary>
        /// Item_ProcessedFlage = 1  mean AddNew
        /// Item_ProcessedFlage = 2  mean Edit
        /// Item_ProcessedFlage = 3  mean Delete
        /// 
        /// </summary>
        public int? Item_ProcessedFlage { get; set; }

        public long? CarsWorkOrdersId { get; set; }
        public string WorkOrderNumber { get; set; }

        public string CarNumber { get; set; }
        public long? CarsId { get; set; }
        public long? CarsWorkOrdersRequestedDetailsId { get; set; }
        public long? CarsWorkOrdersDeliverDetailsId { get; set; }
        public long? CarsWorkOrdersItemProcessedDetailsId { get; set; }
        public long? Item_Category_Id { get; set; }
        public string Item_Category_Name { get; set; }
        public int? ProcessNameId { get; set; }
        public string ProcessName { get; set; }
        public string ProcessNameNumber { get; set; }
        
        public long? EmployeeId { get; set; }
        public string EmployeeName { get; set; }

        [ForeignKey("OrderTable_id")]
        public Order_Purshing Order_Purshing { get; set; }
        [ForeignKey("Unit_item_Table_Id")]
        public Unit_item Unit_item { get; set; }

        [ForeignKey("Item_Id")]
        public Items Items { get; set; }
        [ForeignKey("To_Store_Unit_Item_Table_Id")]
        public Store Unit_ItemTo_Store { get; set; }



        [ForeignKey("Id_From")]
        public Store storeFrom { get; set; }

        [ForeignKey("Id_To")]
        public Store storeTo { get; set; }
        
    }
}
