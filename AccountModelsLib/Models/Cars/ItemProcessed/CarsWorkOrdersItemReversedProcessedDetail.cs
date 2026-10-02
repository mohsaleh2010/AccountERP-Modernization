using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{
    [Table("CarsWorkOrdersItemReversedProcessedDetails")]
    public class CarsWorkOrdersItemReversedProcessedDetail : AICarsWorkOrdersItemProcessedDetail
    {
        public CarsWorkOrdersItemReversedProcessedDetail()
        {

        }
        public CarsWorkOrdersItemReversedProcessedDetail(int IsMasna3iatKhamat_)
        {
            IsMasna3iatKhamat = IsMasna3iatKhamat_;
        } 

        [ForeignKey("CarsWorkOrdersId")]
        public   CarsWorkOrder CarsWorkOrder { get; set; }

        [ForeignKey("StoreId")]
        public   Store Store { get; set; }

        [ForeignKey("Item_Id")]
        public   Items Items { get; set; }

        [ForeignKey("Unit_ItemsId")]
        public   Unit_item Unit_item { get; set; }



        [ForeignKey("Unit_Id")]
        public   unit Unit { get; set; }

        public List<CarsWorkOrdersItemProcessedEmployee> CarsWorkOrdersItemProcessedEmployee { get; set; }


        public long Id { get; set; }
        public long? CarsWorkOrdersId { get; set; }
        public long Item_Id { get; set; }
        public long Unit_ItemsId { get; set; }
        public long? Detailes_PurshingId { get; set; }
        public long? Order_Purshing_No { get; set; }
        public DateTime? Order_Purshing_Date { get; set; }
        public long? CarsWorkOrdersRequestedDetailsId { get; set; }
        public long? CarsWorkOrdersDeliverItemBalancesId { get; set; }
        public long? CarsWorkOrdersDeliverDetailsId { get; set; }
        public decimal? Qnty { get; set; }
        public string Invoice_Item_Name { get; set; }
        public string Item_Name { get; set; }
        public long? Store_Id { get; set; }
        public long? Unit_Id { get; set; }
        public string UnitStr { get; set; }
        public string ItemCode { get; set; }
        public string Specification { get; set; }
        public decimal? Larg_Qnty { get; set; }
        public decimal? Small_Qnty { get; set; }
        public decimal? Item_No { get; set; }
        public decimal? PriceUnit { get; set; }
        public decimal? Larg_Unit_Price { get; set; }
        public decimal? Small_Unit_Price { get; set; }
        public int? Unit_Used_for_Price { get; set; }
        public decimal? S_price_LargeUnit { get; set; }
        public decimal? S_price_SmallUnit { get; set; }
        public decimal? Qnty_value { get; set; }
        public decimal? Item_Total_WithoutTax { get; set; }
        public decimal? Total { get; set; }
        public decimal? Item_Net_Total_Price { get; set; }
        public decimal? Item_ATS05 { get; set; }
        public decimal? Item_ATS05_Value { get; set; }
        public decimal? Item_TaxSales10 { get; set; }
        public decimal? Item_TaxSales10_Value { get; set; }
        public int? Is_Item_CommissionValue { get; set; }
        public decimal? Item_CommissionValue { get; set; }
        public decimal? Unit_CommissionValue { get; set; }
        public decimal? Item_CommissionRate { get; set; }
        public decimal? Unit_CommissionRate { get; set; }
        public decimal? Net_Total_Item_Commission { get; set; }
        public decimal? ItemCommissionTotal { get; set; }
        public string QntyStr { get; set; }
        public string QntyStrColumn { get; set; }
        public string Item_Notes { get; set; }
        public long? Order_PurshingId { get; set; }
        public long? StoreId { get; set; }
        public int? Is_NotShowOnInvoice { get; set; }
        public int? Is_SarfFromStore { get; set; }
        public int? Is_AddToInvoice { get; set; }
        public int? Item_DeletedFlage { get; set; }
        public int? Item_ProcessedFlage { get; set; }
        public long? User_Id { get; set; }
        public long? Branch_Id { get; set; }
        public DateTime? Insert_ServerDateTime { get; set; }
        public DateTime? Update_ServerDateTime { get; set; }
        public DateTime? EditOperationDate { get; set; }
        public long? Edit_User_Id { get; set; }
        public long ProcessNo { get; set; }
        public DateTime? OperationDate { get; set; }
        public long? Item_Category_Id { get; set; }
        public string Item_Category_Name { get; set; }
        public int? IsMasna3iatKhamat { get; set; }
        public decimal ? P_Price { get; set; }
        public decimal? P_priceAvgCost { get; set; }
        public decimal? Qnty_Value_P_Price { get; set; }
        public decimal? Profits { get; set; }
    }
}
