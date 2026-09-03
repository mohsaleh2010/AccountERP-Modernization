using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{

    public interface AICarsWorkOrdersItemProcessedDetail
    {
        [ForeignKey("CarsWorkOrdersId")]
        CarsWorkOrder CarsWorkOrder { get; set; }

        [ForeignKey("StoreId")]
        Store Store { get; set; }

        [ForeignKey("Item_Id")]
        Items Items { get; set; }

        [ForeignKey("Unit_ItemsId")]
        Unit_item Unit_item { get; set; }



        [ForeignKey("Unit_Id")]
        unit Unit { get; set; }

        List<CarsWorkOrdersItemProcessedEmployee> CarsWorkOrdersItemProcessedEmployee { get; set; }



        long Id { get; set; }
        long? CarsWorkOrdersId { get; set; }
        long Item_Id { get; set; }
        long Unit_ItemsId { get; set; }
        long? Detailes_PurshingId { get; set; }
        long? Order_PurshingId { get; set; }
        long? Order_Purshing_No { get; set; }
        DateTime? Order_Purshing_Date { get; set; }
        //Order_PurshingId Order_Purshing_No  Order_Purshing_Date



        long? CarsWorkOrdersRequestedDetailsId { get; set; }
        long? CarsWorkOrdersDeliverItemBalancesId { get; set; }
        long? CarsWorkOrdersDeliverDetailsId { get; set; }



        decimal? Qnty { get; set; }
        string Invoice_Item_Name { get; set; }
        string Item_Name { get; set; }
        long? Store_Id { get; set; }
        long? Unit_Id { get; set; }
        string UnitStr { get; set; }
        string ItemCode { get; set; }
        string Specification { get; set; }
        decimal? Larg_Qnty { get; set; }
        decimal? Small_Qnty { get; set; }
        decimal? Item_No { get; set; }
        decimal? PriceUnit { get; set; }
        decimal? Larg_Unit_Price { get; set; }
        decimal? Small_Unit_Price { get; set; }
        int? Unit_Used_for_Price { get; set; }
        decimal? S_price_LargeUnit { get; set; }
        decimal? S_price_SmallUnit { get; set; }
        decimal? Qnty_value { get; set; }
        decimal? P_priceAvgCost { get; set; }
        decimal? Item_Total_WithoutTax { get; set; }
        decimal? Total { get; set; }
        decimal? Item_Net_Total_Price { get; set; }
        decimal? Item_ATS05 { get; set; }
        decimal? Item_ATS05_Value { get; set; }
        decimal? Item_TaxSales10 { get; set; }
        decimal? Item_TaxSales10_Value { get; set; }

        int? Is_Item_CommissionValue { get; set; }
        decimal? Item_CommissionValue { get; set; }
        decimal? Unit_CommissionValue { get; set; }
        decimal? Item_CommissionRate { get; set; }
        decimal? Unit_CommissionRate { get; set; }
        decimal? Net_Total_Item_Commission { get; set; }
        decimal? ItemCommissionTotal { get; set; }

        string QntyStr { get; set; }
        string QntyStrColumn { get; set; }
        string Item_Notes { get; set; }
         
        long? StoreId { get; set; }
        int? Is_NotShowOnInvoice { get; set; }
        int? Is_SarfFromStore { get; set; }
        int? Is_AddToInvoice { get; set; }
        int? Item_DeletedFlage { get; set; }
        int? Item_ProcessedFlage { get; set; }
        int? User_Id { get; set; }
        int? Branch_Id { get; set; }
        DateTime? Insert_ServerDateTime { get; set; }
        DateTime? Update_ServerDateTime { get; set; }
        DateTime? EditOperationDate { get; set; }
        int? Edit_User_Id { get; set; }
        long ProcessNo { get; set; }
        DateTime? OperationDate { get; set; }
        long? Item_Category_Id { get; set; }
        string Item_Category_Name { get; set; }

        int? IsMasna3iatKhamat { get; set; }
        decimal? P_Price { get; set; }
        decimal? Qnty_Value_P_Price { get; set; }
        decimal? Profits { get; set; }
        
    }
}
