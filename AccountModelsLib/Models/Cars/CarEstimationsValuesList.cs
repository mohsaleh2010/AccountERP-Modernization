using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models 
{
    public class CarEstimationsValuesList
    {
        public long Id { get; set; }
        public long StoreId { get; set; }
        public long CarsId { get; set; }
        public long CustomerId { get; set; }
        public long? ReceptionEngineerId { get; set; }

        public string ReceptionEngineerName { get; set; }


        public long EstimationsNumber { get; set; }
        public DateTime? EstimationsDate { get; set; }
        public string Notes { get; set; }
        public decimal? Total_ListPriceWithoutTax_Workmanship { get; set; }
        public decimal? Total_ListPriceWithoutTax_SpareParts { get; set; }
        public decimal? Total_ListPriceWithoutTax_Oils { get; set; }
        public decimal? Total_ListPriceWithoutTax { get; set; }
        public decimal? NetInvoiceTotal_Workmanship { get; set; }
        public decimal? NetInvoiceTotal_SpareParts { get; set; }
        public decimal? NetInvoiceTotal_Oils { get; set; }
        public decimal? NetInvoiceTotal { get; set; }
        public decimal? TaxSalesListWorkmanship_Rate { get; set; }
        public decimal? TaxSalesListSpareParts_Rate { get; set; }
        public decimal? TaxSalesListOils_Rate { get; set; }
        public decimal? TaxSalesListWorkmanship_Value { get; set; }
        public decimal? TaxSalesListSpareParts_Value { get; set; }
        public decimal? TaxSalesListOils_Value { get; set; }
        public decimal? TotalListTaxSales10_Value { get; set; }
        public string TafkeetNet_Total { get; set; }

        public decimal? Qnty_ValueSpareParts { get;   set; }
        public decimal? Item_Net_TotalSpareParts { get;   set; }
        public decimal? Qnty_ValueWorkmanship { get;   set; }
        public decimal? Item_Net_TotalOils { get;   set; }
        public decimal? Item_Net_TotalWorkmanship { get;   set; }
        public decimal? Total_Item_CommissionValue { get;   set; }
        public decimal? Net_Total { get;   set; }
        public decimal? Qnty_ValueOils { get;   set; }
        public decimal? Total_ItemPriceWithoutTax { get;   set; }
        public decimal? Total { get;   set; }
        public decimal? Total_PriceWithoutTax { get;   set; }
        public decimal? TaxSales10_Value { get;   set; }
        public decimal? Equivalent_Net_Total_Price { get;   set; }

        public long? Currency_id { get;   set; }
        public long ? User_Id { get; set; }
        public long ? Branch_Id { get; set; }

      
        ////////////////////////////////////
        ///

        
        public decimal? ATS05ListWorkmanship_Rate { get; set; }
        public decimal? ATS05ListSpareParts_Rate { get; set; }
        public decimal? ATS05ListOils_Rate { get; set; }
        public decimal? ATS05ListWorkmanship_Value { get; set; }
        public decimal? ATS05ListSpareParts_Value { get; set; }
        public decimal? ATS05ListOils_Value { get; set; }
        public decimal? TotalListATS05_Value { get; set; }
        

        public decimal? CommissionListWorkmanship_Rate { get; set; }
        public decimal? CommissionListSpareParts_Rate { get; set; }
        public decimal? CommissionListOils_Rate { get; set; }
        public decimal? CommissionListWorkmanship_Value { get; set; }
        public decimal? CommissionListSpareParts_Value { get; set; }
        public decimal? CommissionListOils_Value { get; set; }
        public decimal? Total_CommissionListWorkmanship { get; set; }
        public decimal? Total_CommissionListSpareParts { get; set; }
        public decimal? Total_CommissionListOils { get; set; }
        public decimal? Total_CommissionList { get; set; }
        public decimal?    List_CommissionRate { get; set; }
        public decimal? List_CommissionValue { get; set; }
        
        public decimal? List_Final_CommissionValue { get; set; }
        public decimal? Net_Total_CommissionValue { get; set; }
        public decimal? ATS05_Value { get;   set; }
        public decimal? Equivalent_Net_Total_CommissionValue { get;   set; }



        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }

        [ForeignKey("CarsId")]
        public Car Car { get; set; }

        [ForeignKey("StoreId")]
        public Store Store { get; set; }

    }

}
