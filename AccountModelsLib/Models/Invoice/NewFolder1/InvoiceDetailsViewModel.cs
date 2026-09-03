using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{
    public class InvoiceDetailsViewModel
    {
        public InvoiceDetailsViewModel() { }

        public long Id { get; set; }
        public long? Invoice_Id { get; set; }
        public long? Item_Id { get; set; }
        public long? Unit_Item_Table_Id { get; set; }


        public int? Item_DeletedFlage { get; set; }
        public int Serial { get; set; }
        public string Item_Name { get; set; }
        public string Invoice_Item_Name { get; set; }
    public string Specification { get; set; }

        public string Items_Types_Name { get; set; }
        public string Code { get; set; }
        public string Store_Name { get; set; }

       
        public decimal? Item_Net_Total_Price { get; set; }
        public decimal? Item_TaxSales10_Value { get; set; }
        public decimal? Item_TaxSales10 { get; set; }
        public decimal? Item_ATS05_Value { get; set; }
        public decimal? Item_ATS05 { get; set; }
        public decimal? Item_Total_WithoutTax { get; set; }



        public decimal? Net_Total_Item_Commission { get; set; }

        public decimal? Alternate_ItemCommissionTotal { get; set; }
        public decimal? Alternate_Item_CommissionValue { get; set; }
        public decimal? Alternate_Unit_CommissionValue { get; set; }
        public decimal? Alternate_Item_CommissionRate { get; set; }


        public decimal? ItemCommissionTotal { get; set; }
        public decimal? Item_CommissionValue { get; set; }
        public decimal? Unit_CommissionValue { get; set; }
        public decimal? Item_CommissionRate { get; set; }

        public decimal? Qnty_Value { get; set; }

        public decimal? Larg_Unit_Price { get; set; }

        public string OperationNo { get; set; }
        public string Merchant_Name { get; set; }

        public string StatementNo { get; set; }

        public decimal? Larg_Qnty { get; set; }

        public decimal? Small_Qnty { get; set; }
        public decimal? Qnty_Another { get; set; }

       
       
        public decimal? Qnty { get; set; }
        public string QntyStr { get; set; }
        public string TotalSmall_QntyStr { get; set; }
        //public decimal? Audience_price,
        public decimal? LargItemsQnty_Free { get; set; }
        public decimal? SmallItemsQnty_Free { get; set; }
        public decimal? Item_No_Free { get; set; }
        public decimal? Qnty_Free { get; set; }
        public string Qnty_FreeStr { get; set; }
        public DateTime?  Item_Expire_Date { get; set; }
        public string Tash3ela { get; set; }  
        public decimal? Somk { get; set; }
        public string Somk_Unit { get; set; }
        public decimal? QntyLenght { get; set; }
        public decimal? QntyWidth { get; set; }
        public decimal? QntyItemNo { get; set; }
        public int? Is_InvoiceKta3y { get; set; }

      


    }
}
