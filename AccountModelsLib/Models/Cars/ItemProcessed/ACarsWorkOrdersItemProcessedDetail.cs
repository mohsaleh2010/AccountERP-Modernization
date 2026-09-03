using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{
    [NotMapped]
    public abstract class ACarsWorkOrdersItemProcessedDetail
    {
       
        public long Id { get; set; }
        public long? CarsWorkOrdersId { get; set; }
        public long Item_Id { get; set; }
        public long Unit_ItemsId { get; set; }
        public long? Detailes_PurshingId { get; set; }
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
        public int? User_Id { get; set; }
        public int? Branch_Id { get; set; }
        public DateTime? Insert_ServerDateTime { get; set; }
        public DateTime? Update_ServerDateTime { get; set; }
        public DateTime? EditOperationDate { get; set; }
        public int? Edit_User_Id { get; set; }
        public long ProcessNo { get; set; }
        public DateTime? OperationDate { get; set; }
        public long? Item_Category_Id { get; set; }
        public string Item_Category_Name { get; set; }

        public int? IsMasna3iatKhamat { get; set; } = 0;

          }
}
