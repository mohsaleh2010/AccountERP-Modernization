using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{
    public class MappingCLS
    {
        public MappingCLS()
        {

        }
        public Detailes_Purshing detailes_Purshing = new Detailes_Purshing();
        public CarsWorkOrdersRequestedDetail carsWorkOrdersRequestedDetail = new CarsWorkOrdersRequestedDetail();

        public CarsWorkOrdersRequestedDetail MappingFromDetailes_Purshing(ref Detailes_Purshing detailes_Purshing)
        {
            CarsWorkOrdersRequestedDetail carsWorkOrdersRequestedDetail = new CarsWorkOrdersRequestedDetail();
            //cwd.CarsWorkOrdersId  = dp.    ;
            //cwd.Order_PurshingId = dp.    ;

            carsWorkOrdersRequestedDetail.Detailes_PurshingId = detailes_Purshing.Id;
            carsWorkOrdersRequestedDetail.Order_PurshingId = detailes_Purshing.OrderTable_id;

            carsWorkOrdersRequestedDetail.CarsWorkOrdersId = detailes_Purshing.CarsWorkOrdersId ?? 0;
            carsWorkOrdersRequestedDetail.ItemCode = detailes_Purshing.Code;
            carsWorkOrdersRequestedDetail.Item_Id = detailes_Purshing.Item_Id;
            carsWorkOrdersRequestedDetail.Unit_ItemsId = detailes_Purshing.Unit_item_Table_Id;
            carsWorkOrdersRequestedDetail.Invoice_Item_Name = detailes_Purshing.Item_Name;
            carsWorkOrdersRequestedDetail.Specification = detailes_Purshing.Item_Name;

            carsWorkOrdersRequestedDetail.Item_Category_Id = detailes_Purshing.Item_Category_Id;
            carsWorkOrdersRequestedDetail.Item_Category_Name = detailes_Purshing.Item_Category_Name;
            carsWorkOrdersRequestedDetail.Larg_Qnty = detailes_Purshing.qua;
            carsWorkOrdersRequestedDetail.Small_Qnty = detailes_Purshing.Part_qua;
            carsWorkOrdersRequestedDetail.Item_No = detailes_Purshing.Item_No;
            carsWorkOrdersRequestedDetail.Qnty = detailes_Purshing.Qnty;

            carsWorkOrdersRequestedDetail.Larg_Unit_Price = detailes_Purshing.Larg_Unit_Price;
            carsWorkOrdersRequestedDetail.Small_Unit_Price = detailes_Purshing.Small_Unit_Price;
            carsWorkOrdersRequestedDetail.PriceUnit = detailes_Purshing.PriceUnit;
            carsWorkOrdersRequestedDetail.P_Price = detailes_Purshing.P_Price;
            carsWorkOrdersRequestedDetail.P_priceAvgCost = detailes_Purshing.P_priceAvgCost??0;

            carsWorkOrdersRequestedDetail.Unit_Used_for_Price = detailes_Purshing.Unit_Used_for_Price;

            carsWorkOrdersRequestedDetail.Qnty_value = detailes_Purshing.Qnty_value;
            carsWorkOrdersRequestedDetail.Item_Total_WithoutTax = detailes_Purshing.Item_Total_WithoutTax;
            carsWorkOrdersRequestedDetail.Item_ATS05 = detailes_Purshing.Item_ATS05;
            carsWorkOrdersRequestedDetail.Item_ATS05_Value = detailes_Purshing.Item_ATS05_Value;
            carsWorkOrdersRequestedDetail.Item_TaxSales10 = detailes_Purshing.Item_TaxSales10;
            carsWorkOrdersRequestedDetail.Item_TaxSales10_Value = detailes_Purshing.Item_TaxSales10_Value;
            carsWorkOrdersRequestedDetail.Total = detailes_Purshing.Total;
            //cwd.Is_Item_CommissionValue = dp.Is_Item_CommissionValue;
            carsWorkOrdersRequestedDetail.Item_CommissionValue = detailes_Purshing.Item_CommissionValue;
            carsWorkOrdersRequestedDetail.Unit_CommissionValue = detailes_Purshing.Unit_CommissionValue;
            carsWorkOrdersRequestedDetail.Item_CommissionRate = detailes_Purshing.Item_CommissionRate;
            //cwd.Unit_CommissionRate = dp.Unit_CommissionRate;
            carsWorkOrdersRequestedDetail.Net_Total_Item_Commission = detailes_Purshing.Net_Total_Item_Commission;
            carsWorkOrdersRequestedDetail.ItemCommissionTotal = detailes_Purshing.ItemCommissionTotal;
            carsWorkOrdersRequestedDetail.Item_Net_Total_Price = detailes_Purshing.Item_Net_Total_Price;
            carsWorkOrdersRequestedDetail.QntyStr = detailes_Purshing.QntyStr;
            carsWorkOrdersRequestedDetail.QntyStrColumn = detailes_Purshing.QntyStr;
            carsWorkOrdersRequestedDetail.Item_Notes = detailes_Purshing.Notes_Item;

            carsWorkOrdersRequestedDetail.StoreId = detailes_Purshing.StoreId;
            carsWorkOrdersRequestedDetail.Unit_Id = detailes_Purshing.Unit_Id;
            carsWorkOrdersRequestedDetail.UnitStr = detailes_Purshing.UnitStr;

            carsWorkOrdersRequestedDetail.Is_ItemDeliver = true;
            //cwd.Is_AddToInvoice = dp.    ;
            //cwd.Is_SarfFromStore = dp.    ;

            //cwd.Item_DeletedFlage = dp.    ;
            //cwd.Item_ProcessedFlage = dp.    ;


            //cwd.User_Id = dp.    ;
            //cwd.Branch_Id = dp.    ;
            //cwd.Insert_ServerDateTime = dp.Insert_ServerDateTime;
            //cwd.Update_ServerDateTime = dp.Update_ServerDateTime;
            //cwd.EditOperationDate = dp.    ;
            //cwd.Edit_User_Id = dp.    ;
            //cwd.ProcessNo = dp.    ;
            //cwd.OperationDate = dp.    ;

            return carsWorkOrdersRequestedDetail;
        }
        public Detailes_Purshing MappingToDetailes_Purshing(ref CarsWorkOrdersRequestedDetail carsWorkOrdersRequestedDetail)
        {
            Detailes_Purshing detailes_Purshing = new Detailes_Purshing();
            //cwd.CarsWorkOrdersId  = dp.    ;
            //cwd.Order_PurshingId = dp.    ;

            detailes_Purshing.Item_Id = carsWorkOrdersRequestedDetail.Item_Id;
            detailes_Purshing.Unit_item_Table_Id = carsWorkOrdersRequestedDetail.Unit_ItemsId;
            detailes_Purshing.Item_Name = carsWorkOrdersRequestedDetail.Invoice_Item_Name;
            //detailes_Purshing.Item_Name = carsWorkOrdersRequestedDetail.Specification;

            detailes_Purshing.Item_Category_Id = carsWorkOrdersRequestedDetail.Item_Category_Id;
            detailes_Purshing.Item_Category_Name = carsWorkOrdersRequestedDetail.Item_Category_Name;

            detailes_Purshing.qua = carsWorkOrdersRequestedDetail.Larg_Qnty;
            detailes_Purshing.Part_qua = carsWorkOrdersRequestedDetail.Small_Qnty;
            detailes_Purshing.Item_No = carsWorkOrdersRequestedDetail.Item_No;
            detailes_Purshing.Qnty = carsWorkOrdersRequestedDetail.Qnty;

            detailes_Purshing.Larg_Unit_Price = carsWorkOrdersRequestedDetail.Larg_Unit_Price;
            detailes_Purshing.Small_Unit_Price = carsWorkOrdersRequestedDetail.Small_Unit_Price;
            detailes_Purshing.PriceUnit = carsWorkOrdersRequestedDetail.PriceUnit;
            detailes_Purshing.P_Price = carsWorkOrdersRequestedDetail.P_Price;
            detailes_Purshing.P_priceAvgCost = carsWorkOrdersRequestedDetail.P_priceAvgCost??0;

            detailes_Purshing.Unit_Used_for_Price = carsWorkOrdersRequestedDetail.Unit_Used_for_Price;

            detailes_Purshing.Qnty_value = carsWorkOrdersRequestedDetail.Qnty_value;
            detailes_Purshing.Item_Total_WithoutTax = carsWorkOrdersRequestedDetail.Item_Total_WithoutTax;
            detailes_Purshing.Item_ATS05 = carsWorkOrdersRequestedDetail.Item_ATS05;
            detailes_Purshing.Item_ATS05_Value = carsWorkOrdersRequestedDetail.Item_ATS05_Value;
            detailes_Purshing.Item_TaxSales10 = carsWorkOrdersRequestedDetail.Item_TaxSales10;
            detailes_Purshing.Item_TaxSales10_Value = carsWorkOrdersRequestedDetail.Item_TaxSales10_Value;
            detailes_Purshing.Total = carsWorkOrdersRequestedDetail.Total;
            //dp.Is_Item_CommissionValue =cwd.Is_Item_CommissionValue;
            detailes_Purshing.Item_CommissionValue = carsWorkOrdersRequestedDetail.Item_CommissionValue;
            detailes_Purshing.Unit_CommissionValue = carsWorkOrdersRequestedDetail.Unit_CommissionValue;
            detailes_Purshing.Item_CommissionRate = carsWorkOrdersRequestedDetail.Item_CommissionRate;
            //dp.Unit_CommissionRate =cwd.Unit_CommissionRate;
            detailes_Purshing.Net_Total_Item_Commission = carsWorkOrdersRequestedDetail.Net_Total_Item_Commission;
            detailes_Purshing.ItemCommissionTotal = carsWorkOrdersRequestedDetail.ItemCommissionTotal;
            detailes_Purshing.Item_Net_Total_Price = carsWorkOrdersRequestedDetail.Item_Net_Total_Price;
            detailes_Purshing.QntyStr = carsWorkOrdersRequestedDetail.QntyStr;
            //dp.QntyStrColumn =cwd.QntyStr;
            detailes_Purshing.Notes_Item = carsWorkOrdersRequestedDetail.Item_Notes;

            detailes_Purshing.StoreId = carsWorkOrdersRequestedDetail.StoreId;
            if (carsWorkOrdersRequestedDetail.Store != null)
            {
                detailes_Purshing.StoreId_Name = carsWorkOrdersRequestedDetail.Store.Store_Name;
            }
            detailes_Purshing.CarsWorkOrdersId = carsWorkOrdersRequestedDetail.CarsWorkOrdersId;
            detailes_Purshing.CarsWorkOrdersRequestedDetailsId = carsWorkOrdersRequestedDetail.Id;
            detailes_Purshing.CarsWorkOrdersDeliverDetailsId = -1;


            //dp.Is_AddToInvoice =cwd.    ;
            //dp.Is_SarfFromStore =cwd.    ;

            //dp.Item_DeletedFlage =cwd.    ;
            //dp.Item_ProcessedFlage =cwd.    ;


            //dp.User_Id =cwd.    ;
            //dp.Branch_Id =cwd.    ;
            //dp.Insert_ServerDateTime =cwd.Insert_ServerDateTime;
            //dp.Update_ServerDateTime =cwd.Update_ServerDateTime;
            //dp.EditOperationDate =cwd.    ;
            //dp.Edit_User_Id =cwd.    ;
            //dp.ProcessNo =cwd.    ;
            //dp.OperationDate =cwd.    ;

            return detailes_Purshing;
        }

        public Detailes_Purshing MappingToDetailes_Purshing(Items itm)
        {
            Detailes_Purshing detailes_Purshing = new Detailes_Purshing();
            //cwd.CarsWorkOrdersId  = dp.    ;
            //cwd.Order_PurshingId = dp.    ;

            detailes_Purshing.Item_Id = itm.id;
            detailes_Purshing.Unit_item_Table_Id = 0;
            detailes_Purshing.Item_Name = itm.ItemName_OnInvoice;
            //detailes_Purshing.Item_Name = carsWorkOrdersRequestedDetail.Specification;

            detailes_Purshing.Item_Category_Id = itm.Item_Category_ID;
            detailes_Purshing.Item_Category_Name = itm.Item_Category.Category_Name;

            detailes_Purshing.qua = 1;
            detailes_Purshing.Part_qua = 0;
            detailes_Purshing.Item_No = 0;
            detailes_Purshing.Qnty = 0;

            detailes_Purshing.Larg_Unit_Price = itm.S_price_LargeUnit;
            detailes_Purshing.Small_Unit_Price = itm.S_price_SmallUnit;
            detailes_Purshing.PriceUnit = itm.S_Price;
            detailes_Purshing.P_Price = itm.P_Price;
            detailes_Purshing.P_priceAvgCost = itm.P_priceAvgCost ?? 0;

            detailes_Purshing.Unit_Used_for_Price = itm.Unit_Used_for_Price;

            detailes_Purshing.Qnty_value = 0;
            detailes_Purshing.Item_Total_WithoutTax = 0;
            detailes_Purshing.Item_ATS05 = 0;
            detailes_Purshing.Item_ATS05_Value = 0;
            detailes_Purshing.Item_TaxSales10 = 0;
            detailes_Purshing.Item_TaxSales10_Value = 0;
            detailes_Purshing.Total = 0;
            //dp.Is_Item_CommissionValue =cwd.Is_Item_CommissionValue;
            detailes_Purshing.Item_CommissionValue = 0;
            detailes_Purshing.Unit_CommissionValue = 0;
            detailes_Purshing.Item_CommissionRate = 0;
            //dp.Unit_CommissionRate =cwd.Unit_CommissionRate;
            detailes_Purshing.Net_Total_Item_Commission = 0;
            detailes_Purshing.ItemCommissionTotal = 0;
            detailes_Purshing.Item_Net_Total_Price = 0;
            detailes_Purshing.QntyStr = "";
            //dp.QntyStrColumn =cwd.QntyStr;
            detailes_Purshing.Notes_Item = "";

            detailes_Purshing.StoreId = itm.S_id;
            if (itm.Store != null)
            {
                detailes_Purshing.StoreId_Name = itm.Store.Store_Name;
            }
            detailes_Purshing.CarsWorkOrdersId = 0;
            detailes_Purshing.CarsWorkOrdersRequestedDetailsId = 0;
            detailes_Purshing.CarsWorkOrdersDeliverDetailsId = -1;




            return detailes_Purshing;
        }


        public CarsWorkOrdersDeliverItemBalance MappingGetCarsWorkOrdersDeliverItemBalance(Detailes_Purshing dp)
        {

            CarsWorkOrdersDeliverItemBalance crD = new CarsWorkOrdersDeliverItemBalance();
            crD.CarsWorkOrdersId = dp.CarsWorkOrdersId;
            crD.WarshaStoresId = dp.StoreId;
            crD.Item_Id = dp.Item_Id;
            crD.ItemCode = dp.Code;
            crD.Qnty = dp.Qnty;
            crD.Larg_Qnty = dp.qua;
            crD.Small_Qnty = dp.Part_qua;
            crD.Item_No = dp.Item_No;
            crD.Unit_Id = dp.Unit_Id;
            crD.UnitStr = dp.UnitStr;







            crD.Larg_Unit_Price = dp.Larg_Unit_Price;
            crD.Small_Unit_Price = dp.Small_Unit_Price;
            crD.PriceUnit = dp.PriceUnit;
            crD.P_Price = dp.P_Price;
            crD.P_priceAvgCost = dp.P_priceAvgCost ?? 0;

            crD.Unit_Used_for_Price = dp.Unit_Used_for_Price;



            return crD;
        }
        //public CarsWorkOrdersDeliverDetail MappingGetCarsWorkOrdersDeliverDetail(Detailes_Purshing dp)
        //{
        //    CarsWorkOrdersDeliverDetail crD = new CarsWorkOrdersDeliverDetail();
        //    return MappingGetCarsWorkOrdersDeliverDetail(ref crD, dp);
        //}
        public CarsWorkOrdersDeliverDetail MappingGetCarsWorkOrdersDeliverDetail(ref CarsWorkOrdersDeliverDetail crD, Detailes_Purshing dp,Order_Purshing order_Purshing)
        {


            crD.CarsWorkOrdersId = dp.CarsWorkOrdersId;
            crD.Detailes_PurshingId = dp.Id;
            crD.Order_PurshingId = order_Purshing.Id;
            crD.Order_Purshing_No = order_Purshing.Order_Purshing_No;
            crD.Order_Purshing_Date = order_Purshing.Order_date;

            
            crD.CarsWorkOrdersRequestedDetailsId = dp.CarsWorkOrdersRequestedDetailsId;

            crD.Item_Id = dp.Item_Id;
            crD.Unit_ItemsId = dp.Unit_item_Table_Id;
            crD.Invoice_Item_Name = dp.Item_Name;
            crD.Item_Category_Id = dp.Item_Category_Id;
            crD.Item_Category_Name = dp.Item_Category_Name;
            crD.ItemCode = dp.Code;

            crD.Store_Id = dp.StoreId;

            crD.Unit_Id = dp.Unit_Id;
            crD.UnitStr = dp.UnitStr;
            crD.Item_Name = dp.Item_Name;

            crD.Specification = dp.Item_Name;
            crD.Larg_Qnty = dp.qua;
            crD.Small_Qnty = dp.Part_qua;
            crD.Item_No = dp.Item_No;
            crD.Qnty = dp.Qnty;

            crD.Larg_Unit_Price = dp.Larg_Unit_Price;
            crD.Small_Unit_Price = dp.Small_Unit_Price;
            crD.PriceUnit = dp.PriceUnit;
            crD.P_Price = dp.P_Price;
            //crD.S_Price = dp.S_Price;
            crD.P_priceAvgCost = dp.P_priceAvgCost ?? 0;

            crD.Unit_Used_for_Price = dp.Unit_Used_for_Price;

            crD.Qnty_value = dp.Qnty_value;
            crD.Item_Total_WithoutTax = dp.Item_Total_WithoutTax;
            crD.Item_ATS05 = dp.Item_ATS05;
            crD.Item_ATS05_Value = dp.Item_ATS05_Value;
            crD.Item_TaxSales10 = dp.Item_TaxSales10;
            crD.Item_TaxSales10_Value = dp.Item_TaxSales10_Value;
            crD.Total = dp.Total;
            //crD.Is_Item_CommissionValue  =crR.Is_Item_CommissionValue;
            crD.Item_CommissionValue = dp.Item_CommissionValue;
            crD.Unit_CommissionValue = dp.Unit_CommissionValue;
            crD.Item_CommissionRate = dp.Item_CommissionRate;
            //crD.Unit_CommissionRate  =crR.CommissionRate;
            crD.Net_Total_Item_Commission = dp.Net_Total_Item_Commission;
            crD.ItemCommissionTotal = dp.ItemCommissionTotal;
            crD.Item_Net_Total_Price = dp.Item_Net_Total_Price;
            crD.QntyStr = dp.QntyStr;
            crD.QntyStrColumn = dp.QntyStr;
            crD.Item_Notes = dp.Notes_Item;
            crD.Order_PurshingId = dp.OrderTable_id;
            crD.StoreId = dp.StoreId;
            //crD.Is_AddToInvoice  =crR.Is_AddToInvoice;
            //crD.Is_SarfFromStore  =crR.Is_SarfFromStore;

            crD.Item_DeletedFlage = dp.Item_DeletedFlage;
            crD.Item_ProcessedFlage = dp.Item_ProcessedFlage;
            //crD.Item_ProcessedFlage  =crR.Item_ProcessedFlage;
            crD.User_Id = dp.User_Id;
            crD.Branch_Id = dp.Branch_Id;
            crD.Insert_ServerDateTime = dp.Insert_ServerDateTime;
            crD.Update_ServerDateTime = dp.Update_ServerDateTime;
            crD.EditOperationDate = dp.EditOperationDate;
            crD.Edit_User_Id = dp.Edit_User_Id;
            crD.ProcessNo = dp.ProcessNo;
            crD.OperationDate = dp.OperationDate;

            crD.Is_SarfFromStore = 1;
            crD.Is_AddToInvoice = 0;
            crD.Item_DeletedFlage = 0;
            crD.Item_ProcessedFlage = 0;
            return crD;
        }

        public CarsWorkOrdersItemProcessedDetail MappingGetCarsWorkOrdersItemProcessedDetailsFrom_dp(Detailes_Purshing dp, Order_Purshing order_Purshing)
        {

            CarsWorkOrdersItemProcessedDetail crD = new CarsWorkOrdersItemProcessedDetail();
            crD.CarsWorkOrdersId = dp.CarsWorkOrdersId;
            crD.Detailes_PurshingId = dp.Id;
            crD.Order_PurshingId = order_Purshing.Id;
            crD.Order_Purshing_No = order_Purshing.Order_Purshing_No;
            crD.Order_Purshing_Date = order_Purshing.Order_date;

            crD.CarsWorkOrdersRequestedDetailsId = dp.CarsWorkOrdersRequestedDetailsId;
            crD.ItemCode = dp.Code;
            crD.Item_Id = dp.Item_Id;
            crD.Unit_ItemsId = dp.Unit_item_Table_Id;
            crD.Invoice_Item_Name = dp.Item_Name;
            crD.Specification = dp.Item_Name;
            crD.Item_Category_Id = dp.Item_Category_Id;
            crD.Item_Category_Name = dp.Item_Category_Name;

            crD.Larg_Qnty = dp.qua;
            crD.Small_Qnty = dp.Part_qua;
            crD.Item_No = dp.Item_No;
            crD.Qnty = dp.Qnty;

            crD.Larg_Unit_Price = dp.Larg_Unit_Price;
            crD.Small_Unit_Price = dp.Small_Unit_Price;
            crD.PriceUnit = dp.PriceUnit;
            crD.P_Price = dp.P_Price;
            crD.P_priceAvgCost = dp.P_priceAvgCost ?? 0;

            crD.Unit_Used_for_Price = dp.Unit_Used_for_Price;

            crD.Qnty_value = dp.Qnty_value;
            crD.Item_Total_WithoutTax = dp.Item_Total_WithoutTax;
            crD.Item_ATS05 = dp.Item_ATS05;
            crD.Item_ATS05_Value = dp.Item_ATS05_Value;
            crD.Item_TaxSales10 = dp.Item_TaxSales10;
            crD.Item_TaxSales10_Value = dp.Item_TaxSales10_Value;
            crD.Total = dp.Total;
            //crD.Is_Item_CommissionValue  =crR.Is_Item_CommissionValue;
            crD.Item_CommissionValue = dp.Item_CommissionValue;
            crD.Unit_CommissionValue = dp.Unit_CommissionValue;
            crD.Item_CommissionRate = dp.Item_CommissionRate;
            //crD.Unit_CommissionRate  =crR.CommissionRate;
            crD.Net_Total_Item_Commission = dp.Net_Total_Item_Commission;
            crD.ItemCommissionTotal = dp.ItemCommissionTotal;
            crD.Item_Net_Total_Price = dp.Item_Net_Total_Price;
            crD.QntyStr = dp.QntyStr;
            crD.QntyStrColumn = dp.QntyStr;
            crD.Item_Notes = dp.Notes_Item;
           

             
        
        //Order_PurshingId Order_Purshing_No  Order_Purshing_Date


        crD.StoreId = dp.StoreId;
            //crD.Is_AddToInvoice  =crR.Is_AddToInvoice;
            //crD.Is_SarfFromStore  =crR.Is_SarfFromStore;

            crD.Item_DeletedFlage = dp.Item_DeletedFlage;
            crD.Item_ProcessedFlage = dp.Item_ProcessedFlage;
            //crD.Item_ProcessedFlage  =crR.Item_ProcessedFlage;
            crD.User_Id = dp.User_Id;
            crD.Branch_Id = dp.Branch_Id;
            crD.Insert_ServerDateTime = dp.Insert_ServerDateTime;
            crD.Update_ServerDateTime = dp.Update_ServerDateTime;
            crD.EditOperationDate = dp.EditOperationDate;
            crD.Edit_User_Id = dp.Edit_User_Id;
            crD.ProcessNo = dp.ProcessNo;
            crD.OperationDate = dp.OperationDate;
            crD.Unit_Id = dp.Unit_Id;
            crD.UnitStr = dp.UnitStr;






            crD.Is_SarfFromStore = 1;
            crD.Is_AddToInvoice = 0;
            crD.Item_DeletedFlage = 0;
            crD.Item_ProcessedFlage = 0;

            return crD;
        }
        public CarsWorkOrdersItemProcessedDetail MappingGetItemProcessed_From_Deliverd(CarsWorkOrdersDeliverDetail dp)
        {

            CarsWorkOrdersItemProcessedDetail crD = new CarsWorkOrdersItemProcessedDetail();
            crD.CarsWorkOrdersId = dp.CarsWorkOrdersId;
            crD.Detailes_PurshingId = dp.Detailes_PurshingId;
            crD.Order_PurshingId = dp.Order_PurshingId;
            crD.Order_Purshing_No = dp.Order_Purshing_No;
            crD.Order_Purshing_Date = dp.Order_Purshing_Date;

            crD.CarsWorkOrdersRequestedDetailsId = dp.CarsWorkOrdersRequestedDetailsId;
            crD.Item_Id = dp.Item_Id;
            crD.Unit_ItemsId = dp.Unit_ItemsId;
            crD.Invoice_Item_Name = dp.Invoice_Item_Name;
            crD.Specification = dp.Invoice_Item_Name;
            crD.Item_Category_Id = dp.Item_Category_Id;
            crD.Item_Category_Name = dp.Item_Category_Name;
            crD.ItemCode = dp.ItemCode;

            crD.Store_Id = dp.StoreId;


            crD.Item_Name = dp.Item_Name;

            crD.Larg_Qnty = dp.Larg_Qnty;
            crD.Small_Qnty = dp.Small_Qnty;
            crD.Item_No = dp.Item_No;
            crD.Qnty = dp.Qnty;

            crD.ItemCode = dp.ItemCode;

            crD.Store_Id = dp.StoreId;

            crD.Unit_Id = dp.Unit_Id;
            crD.UnitStr = dp.UnitStr;
            crD.Item_Name = dp.Item_Name;


            crD.Larg_Unit_Price = dp.Larg_Unit_Price;
            crD.Small_Unit_Price = dp.Small_Unit_Price;
            crD.PriceUnit = dp.PriceUnit;
            crD.P_Price = dp.P_Price;

            crD.Unit_Used_for_Price = dp.Unit_Used_for_Price;

            crD.Qnty_value = dp.Qnty_value;
            crD.Item_Total_WithoutTax = dp.Item_Total_WithoutTax;
            crD.Item_ATS05 = dp.Item_ATS05;
            crD.Item_ATS05_Value = dp.Item_ATS05_Value;
            crD.Item_TaxSales10 = dp.Item_TaxSales10;
            crD.Item_TaxSales10_Value = dp.Item_TaxSales10_Value;
            crD.Total = dp.Total;
            //crD.Is_Item_CommissionValue  =crR.Is_Item_CommissionValue;
            crD.Item_CommissionValue = dp.Item_CommissionValue;
            crD.Unit_CommissionValue = dp.Unit_CommissionValue;
            crD.Item_CommissionRate = dp.Item_CommissionRate;
            //crD.Unit_CommissionRate  =crR.CommissionRate;
            crD.Net_Total_Item_Commission = dp.Net_Total_Item_Commission;
            crD.ItemCommissionTotal = dp.ItemCommissionTotal;
            crD.Item_Net_Total_Price = dp.Item_Net_Total_Price;
            crD.QntyStr = dp.QntyStr;
            crD.QntyStrColumn = dp.QntyStr;
            crD.Item_Notes = dp.Item_Notes;
           
            crD.StoreId = dp.StoreId;
            //crD.Is_AddToInvoice  =crR.Is_AddToInvoice;
            //crD.Is_SarfFromStore  =crR.Is_SarfFromStore;

            crD.Item_DeletedFlage = dp.Item_DeletedFlage;
            //crD.Item_ProcessedFlage  =crR.Item_ProcessedFlage;
            crD.User_Id = dp.User_Id;
            crD.Branch_Id = dp.Branch_Id;
            crD.Insert_ServerDateTime = dp.Insert_ServerDateTime;
            crD.Update_ServerDateTime = dp.Update_ServerDateTime;
            crD.EditOperationDate = dp.EditOperationDate;
            crD.Edit_User_Id = dp.Edit_User_Id;
            crD.ProcessNo = dp.ProcessNo;
            crD.OperationDate = dp.OperationDate;


            //crD.Is_SarfFromStore = dp.Is_SarfFromStore;
            //crD.Is_AddToInvoice = dp.Is_AddToInvoice;
            //crD.Item_DeletedFlage = dp.Item_DeletedFlage;
            //crD.Item_ProcessedFlage = dp.Item_ProcessedFlage;

            crD.Is_SarfFromStore = dp.Is_SarfFromStore;
            crD.Is_AddToInvoice = 0;
            crD.Item_DeletedFlage = 0;
            crD.Item_ProcessedFlage = 0;

            return crD;
        }

        public CarsWorkOrdersItemReversedProcessedDetail MappingGetItemReversed_From_Processed(CarsWorkOrdersItemProcessedDetail dp)
        {

            CarsWorkOrdersItemReversedProcessedDetail crD = new CarsWorkOrdersItemReversedProcessedDetail();
            crD.CarsWorkOrdersId = dp.CarsWorkOrdersId;
            crD.Detailes_PurshingId = dp.Detailes_PurshingId;
            crD.Order_PurshingId = dp.Order_PurshingId;
            crD.Order_Purshing_No = dp.Order_Purshing_No;
            crD.Order_Purshing_Date = dp.Order_Purshing_Date;

            crD.CarsWorkOrdersRequestedDetailsId = dp.CarsWorkOrdersRequestedDetailsId;
            crD.Item_Id = dp.Item_Id;
            crD.Unit_ItemsId = dp.Unit_ItemsId;
            crD.Invoice_Item_Name = dp.Invoice_Item_Name;
            crD.Specification = dp.Invoice_Item_Name;
            crD.Item_Category_Id = dp.Item_Category_Id;
            crD.Item_Category_Name = dp.Item_Category_Name;
            crD.ItemCode = dp.ItemCode;

            crD.Store_Id = dp.StoreId;


            crD.Item_Name = dp.Item_Name;

            crD.Larg_Qnty = dp.Larg_Qnty;
            crD.Small_Qnty = dp.Small_Qnty;
            crD.Item_No = dp.Item_No;
            crD.Qnty = dp.Qnty;

            crD.ItemCode = dp.ItemCode;

            crD.Store_Id = dp.StoreId;

            crD.Unit_Id = dp.Unit_Id;
            crD.UnitStr = dp.UnitStr;
            crD.Item_Name = dp.Item_Name;


            crD.Larg_Unit_Price = dp.Larg_Unit_Price;
            crD.Small_Unit_Price = dp.Small_Unit_Price;
            crD.PriceUnit = dp.PriceUnit;

            crD.Unit_Used_for_Price = dp.Unit_Used_for_Price;

            crD.Qnty_value = dp.Qnty_value;
            crD.Item_Total_WithoutTax = dp.Item_Total_WithoutTax;
            crD.Item_ATS05 = dp.Item_ATS05;
            crD.Item_ATS05_Value = dp.Item_ATS05_Value;
            crD.Item_TaxSales10 = dp.Item_TaxSales10;
            crD.Item_TaxSales10_Value = dp.Item_TaxSales10_Value;
            crD.Total = dp.Total;
            //crD.Is_Item_CommissionValue  =crR.Is_Item_CommissionValue;
            crD.Item_CommissionValue = dp.Item_CommissionValue;
            crD.Unit_CommissionValue = dp.Unit_CommissionValue;
            crD.Item_CommissionRate = dp.Item_CommissionRate;
            //crD.Unit_CommissionRate  =crR.CommissionRate;
            crD.Net_Total_Item_Commission = dp.Net_Total_Item_Commission;
            crD.ItemCommissionTotal = dp.ItemCommissionTotal;
            crD.Item_Net_Total_Price = dp.Item_Net_Total_Price;
            crD.QntyStr = dp.QntyStr;
            crD.QntyStrColumn = dp.QntyStr;
            crD.Item_Notes = dp.Item_Notes;
           

            crD.StoreId = dp.StoreId;
            //crD.Is_AddToInvoice  =crR.Is_AddToInvoice;
            //crD.Is_SarfFromStore  =crR.Is_SarfFromStore;

            crD.Item_DeletedFlage = dp.Item_DeletedFlage;
            //crD.Item_ProcessedFlage  =crR.Item_ProcessedFlage;
            crD.User_Id = dp.User_Id;
            crD.Branch_Id = dp.Branch_Id;
            crD.Insert_ServerDateTime = dp.Insert_ServerDateTime;
            crD.Update_ServerDateTime = dp.Update_ServerDateTime;
            crD.EditOperationDate = dp.EditOperationDate;
            crD.Edit_User_Id = dp.Edit_User_Id;
            crD.ProcessNo = dp.ProcessNo;
            crD.OperationDate = dp.OperationDate;


            //crD.Is_SarfFromStore = dp.Is_SarfFromStore;
            //crD.Is_AddToInvoice = dp.Is_AddToInvoice;
            //crD.Item_DeletedFlage = dp.Item_DeletedFlage;
            //crD.Item_ProcessedFlage = dp.Item_ProcessedFlage;

            crD.Is_SarfFromStore = dp.Is_SarfFromStore;
            crD.Is_AddToInvoice = 0;
            crD.Item_DeletedFlage = 0;
            crD.Item_ProcessedFlage = 0;

            return crD;
        }

    }
}
