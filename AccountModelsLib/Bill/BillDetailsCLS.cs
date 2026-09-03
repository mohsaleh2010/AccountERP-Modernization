using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib
{
    public class BillDetailsCLS
    {
        public BillDetailsCLS()
        {

        }

        public long Bills_List_Id = 0;
        public long Bills_Details_Id = 0;
        public int Item_Id;
        public int Unit_Item_Table_Id;
        public int Unit_Id;
        public long Unit_Item_ExpireDate_Id;

        


        public int Store_Id;
        public string Invoice_Item_Name = "";
        public decimal Larg_Qnty;
        public decimal Small_Qnty;
        public decimal Qnty_Another;
        public string QntyStr = "";

        public decimal TotalSmall_Qnty;
        public string TotalSmall_QntyStr = "";
        public string TotalSmall_QntyStrColumn = "";
       



        public decimal LargItemsQnty_Free;
        public decimal SmallItemsQnty_Free;
        public decimal Item_No_Free;

        public decimal Larg_Unit_Price = 0;
        public decimal Small_Unit_Price = 0;
        public decimal Qnty_value;
        public decimal Item_Total;
        public decimal Item_Total_WithoutTax;
        public decimal Item_Net_Total_Price = 0;
        public decimal PriceUnit;


        public decimal Item_ATS05;
        public decimal Item_ATS05_Value;
        public decimal Item_TaxSales10;
        public decimal Item_TaxSales10_Value;
        public int Unit_Used_for_Price = 0;

        public int Unit_Used_For_P_price_Default = 0;
        public int Unit_Used_For_S_price_Default = 0;
        public decimal P_price_SmallUnit = 0;
        public decimal P_price_LargeUnit = 0;
        public decimal S_price_SmallUnit = 0;
        public decimal S_price_LargeUnit = 0;
        public int Is_ItemWithoutBalance = 0;
        public decimal TaxSales10_Value = 0;

        public decimal S_price_Item_No = 0;
        public decimal P_price_Item_No = 0;


        public string StatementNo;
        public string OperationNo;
        public string Merchant_Name;
        public decimal QntyWidth = 0;
        public decimal QntyLenght = 0;
        public decimal QntyItemNo = 0;
        public string Specification = "";
        public string NameSpecification = "";
        public string Item_Code;
        public string Item_Notes = "";
        public int Balance_Checked = 0;
        public int Make_Order = 0;
        public int Is_InvoiceItem_Kta3y = 0;
        public decimal Audience_price = 0;
        public string Tash3ela;
        public DateTime Item_Expire_Date = new DateTime(DateTime.Now.Date.Year, DateTime.Now.Date.Month, DateTime.Now.Date.Day);
        public int Is_Item_CommissionValue = 0;

        public decimal Unit_CommissionValue;
        public decimal Item_CommissionValue;
        public decimal Item_CommissionRate;
        public decimal ItemCommissionTotal;




        public decimal Alternate_Unit_CommissionValue;
        public decimal Alternate_Item_CommissionValue;
        public decimal Alternate_Item_CommissionRate;
        public decimal Alternate_ItemCommissionTotal;

        public decimal Net_Total_Item_Commission;


        public int Is_Alternate_Item_CommissionValue = 0;


    }
}
