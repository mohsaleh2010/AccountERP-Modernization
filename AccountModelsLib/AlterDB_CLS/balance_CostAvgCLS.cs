using System;
 
using System.Collections.Generic;
using System.Text;
 
using System.Data;
using System.Linq;

namespace AccountModelsLib
{
    public class balance_CostAvgCLS
    {
        public balance_CostAvgCLS()
        {

        }
        public int AllBalance_NoZero = 1;
        public DataTable Items_dt;
        public DataTable dt_Items;

        public int Flag = 0;
        public decimal Item_Height_ = -1;
        public decimal Item_Width_ = -1;

        public string Somk_Unit;
        public decimal Item_Somk = -1;

        public string Items_Types_Name;
        public int Items_Types_Name_Id = 0;

        public int Items_TypesDescription_NameId = 0;
        public int Items_TypesDescriptionNameOtherId = 0;
        public int Items_TypesFactory_NameId = 0;
        public int Items_TypesSize_NameId = 0;
        public int Items_TypesColor_NameId = 0;
        public int Items_TypesDegreeFarz_NameId = 0;
        string mysql = @"";

        public DataTable CreateTempTableAddLastBalance_PriceFor_ItemQntyGetAVGCostForItemBalance(long Store_Id, bool All, int Item_Category_ID)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            Items_dt = CreateTempTableAddLastBalance_PriceFor_ItemQntyGetAVGCostForItemBalance(ref opp, Store_Id, All, Item_Category_ID);

            return Items_dt;
        }

        void CreateDBTable(ref AccountModelsLib.operations opp)
        {



            try
            {
                ////Create table To Insert the Purchase price for last item quntity 
                mysql = @"Drop Table ItemCost ";
                opp.insert_me(mysql);
            }
            catch (Exception ee)
            {

            }
            try
            { ////Create table To Insert the average of  the Purchase price for last item quntity 
                mysql = @"Drop Table ItemCostAVG ";
                opp.insert_me(mysql);
            }
            catch (Exception ee)
            {

            }
            try
            {
                mysql = @"Create Table ItemCost(

 [Id] [numeric](18, 0) IDENTITY(1,1)  NOT NULL,
Bills_DetailsId bigint  NOT NULL default(0) ,
      Invoice_Date [datetime] NULL,  
Unit_Item_Table_Id bigint Null default(0) ,
totalFull decimal(18,2) NULL  default(0) ,  
AVGPrice decimal(18,2) NULL default(0) ,  
[ROW NUMBER] int  NULL default(0),    
FirstValues decimal(18,2) NULL default(0),
FirstValuesAdded decimal(18,2) NULL default(0),
)";
                opp.insert_me(mysql);
            }
            catch (Exception ee)
            {

            }
            try
            {
                mysql = @"Create Table ItemCostAVG(

 [Id] [numeric](18, 0) IDENTITY(1,1)  NOT NULL,
 
Unit_Item_Table_Id bigint Null,
 
AVGPrice decimal(18,2) NULL ,  
 
)";
                opp.insert_me(mysql);
            }
            catch (Exception ee)
            {

            }




        }

        void AddAvgPriceToTableItemCosts(ref AccountModelsLib.operations opp, long Store_Id)
        {



            try
            {
                // Insert the average of  the Purchase price for last item quntity
                mysql = @"
INSERT INTO ItemCostAVG
                  (  Unit_Item_Table_Id , AVGPrice )

SELECT Unit_Item_Table_Id, AVG(AVGPrice) AS AVGPrice
FROM     ItemCost
GROUP BY Unit_Item_Table_Id
	  
";
                opp.insert_me(mysql);
                //DataTable dt = opp.return_DataTable(mysql);
            }
            catch (Exception ee)
            {
            }




            try
            {  // if the average prurchase price not found add the last price from unit item

                mysql = @" INSERT INTO ItemCostAVG
                  (Unit_Item_Table_Id, AVGPrice)
SELECT id, P_price
FROM     Unit_item
WHERE  (id NOT IN 
                      (SELECT Unit_Item_Table_Id
                       FROM      ItemCostAVG AS ItemCostAVG_1))   ";
                if (Store_Id > 0)
                {
                    mysql = mysql + " AND (Store_id = " + Store_Id + ")";
                }
                opp.insert_me(mysql);
                //DataTable dt = opp.return_DataTable(mysql);
            }
            catch (Exception ee)
            {
            }

        }
        public DataTable CreateTempTableAddLastBalance_PriceFor_ItemQntyGetAVGCostForItemBalance(ref AccountModelsLib.operations opp, long Store_Id, bool All, int Item_Category_ID)
        {

            CreateDBTable(ref opp);


            // Insert the Purchase price for last item quntity


            try
            {
                mysql = @"
INSERT INTO ItemCost
                  ( Bills_DetailsId, Invoice_Date, Unit_Item_Table_Id, totalFull, AVGPrice, [ROW NUMBER], FirstValues)
	  
	 SELECT   
	Bills_DetailsId,  Invoice_Date, Unit_Item_Table_Id,    totalFull  ,AVGPrice,[ROW NUMBER],first_value(AVGPrice) over(PARTITION BY Unit_Item_Table_Id ORDER BY Unit_Item_Table_Id,Invoice_Date DESC  rows between unbounded preceding and unbounded following) 
	 AS FirstValues
	 FROM (			  
   	SELECT    Bills_Details.Id as Bills_DetailsId,Bills_Details.Larg_Unit_Price, Bills_Details.Unit_Item_Table_Id,Invoice_Date,Larg_Qnty
	
	,  SUM(Bills_Details.Larg_Qnty) OVER (PARTITION BY Bills_Details.Unit_Item_Table_Id ORDER BY Bills_Details.Unit_Item_Table_Id,Invoice_Date DESC ) AS totalFull 
	,  AVg(Bills_Details.Larg_Unit_Price) OVER (PARTITION BY Bills_Details.Unit_Item_Table_Id ORDER BY Bills_Details.Unit_Item_Table_Id,Invoice_Date DESC ) AS AVGPrice 
	  
	 ,  ROW_NUMBER() OVER (PARTITION BY Bills_Details.Unit_Item_Table_Id ORDER BY Bills_Details.Unit_Item_Table_Id,Invoice_Date DESC 
         	   ) AS [ROW NUMBER]
FROM     Bills_Details INNER JOIN
                  Bills_List ON Bills_Details.Invoice_Id = Bills_List.ID INNER JOIN
                  Unit_item ON Bills_Details.Unit_Item_Table_Id = Unit_item.id
                 
  )t INNER JOIN
                  Unit_item ON t.Unit_Item_Table_Id = Unit_item.ID
    WHERE Unit_item.Full_qua >= t.totalFull  
	 

     ";
                opp.insert_me(mysql);
                //DataTable dt = opp.return_DataTable(mysql);
            }
            catch (Exception ee)
            {
            }

            AddAvgPriceToTableItemCosts(ref opp, Store_Id);

            return GetAVGCostForItemBalance(ref opp, Store_Id, All, Item_Category_ID);
        }

        public DataTable GetAVGCostForItemBalance(ref AccountModelsLib.operations opp, long Store_Id, bool All, int Item_Category_ID)
        {


            string mysql = @" SELECT '' AS Serial_, '' AS QntyStrColumn, Store.Store_Name, Item.Item_Name, Unit_item.id, Unit_item.Item_id, Unit_item.Store_id, Unit_item.Store_Type, Unit_item.Code, Unit_item.unit_id, Unit_item.Full_qua, Unit_item.D_Qua, Unit_item.Item_No, 
                  Unit_item.Reserved_Balance_Larg_Qnty, Unit_item.Reserved_Balance_Small_Qnty, Unit_item.Big_unit, Unit_item.Small_unit, Unit_item.P_price, Unit_item.S_price, Unit_item.Specification, Unit_item.Upon_Request, Unit_item.Currency, 
                  Unit_item.active_flage, Unit_item.TaxSales10_Value, unit.u_name, unit.B_Unit, unit.S_unit, unit.B_Unit_Value, unit.S_unit_Value, Unit_item.P_price * (Unit_item.D_Qua / Unit_item.Small_unit + Unit_item.Full_qua) AS price, 
                  Unit_item.P_price AS P_Price_, Unit_item.S_price * (Unit_item.D_Qua / Unit_item.Small_unit + Unit_item.Full_qua) AS Total_Sell_price, Unit_item.D_Qua / Unit_item.Small_unit + Unit_item.Full_qua AS TotalSmallQnty, Item.No3eboa, 
                  Item_Category.Category_Name, Item_Main_Category.Main_Category_Name, ItemCostAVG.AVGPrice * (Unit_item.D_Qua / Unit_item.Small_unit + Unit_item.Full_qua) AS Total_Purchase_Price, Item.Width_, Item.Height_, Item.Somk, 
                  Item.Somk_Unit, Item.Items_Types_Name, Item.Items_Types_Name_Id, ItemCostAVG.AVGPrice AS AVG_P_Price
FROM     Item INNER JOIN
                  Unit_item ON Item.id = Unit_item.Item_id INNER JOIN
                  unit ON Unit_item.unit_id = unit.id INNER JOIN
                  Item_Category ON Item.Item_Category_ID = Item_Category.ID INNER JOIN
                  Item_Main_Category ON Item_Category.Main_Item_Category_Id = Item_Main_Category.ID INNER JOIN
                  Store ON Unit_item.Store_id = Store.id INNER JOIN
                  ItemCostAVG AS ItemCostAVG ON Unit_item.id = ItemCostAVG.Unit_Item_Table_Id";
            mysql = mysql + " WHERE (Unit_item.active_flage =1)  AND (Item.Active_flage = 1)  ";
            if (Store_Id > 0)
            {
                mysql = mysql + " and (Unit_item.Store_id =" + Store_Id + ")  ";
            }
            if (Item_Category_ID != -1)
            {
                mysql = mysql + " AND (Item.Item_Category_ID =" + Item_Category_ID + ") ";
            }

            if (AllBalance_NoZero == 1)
            {
                mysql = mysql + " AND (Unit_item.Full_qua>0 OR  Unit_item.D_Qua>0) ";
            }
            else if (AllBalance_NoZero == 0)
            {
                mysql = mysql + " AND  Unit_item.Full_qua +  Unit_item.D_Qua = 0 ";
            }
            else if (AllBalance_NoZero == -1)
            {

            }

            //////////////////////
            if (Items_Types_Name_Id > 0)
            {
                mysql = mysql + " AND (Item.Items_Types_Name_Id= " + Items_Types_Name_Id + " ) ";
            }
            if (Items_TypesDescription_NameId > 0)
            {
                mysql = mysql + " AND (Item.Items_TypesDescription_NameId= " + Items_TypesDescription_NameId + " ) ";
            }
            if (Items_TypesDescriptionNameOtherId > 0)
            {
                mysql = mysql + " AND (Item.Items_TypesDescriptionNameOtherId= " + Items_TypesDescriptionNameOtherId + " ) ";
            }
            if (Items_TypesFactory_NameId > 0)
            {
                mysql = mysql + " AND (Item.Items_TypesFactory_NameId= " + Items_TypesFactory_NameId + " ) ";
            }
            if (Items_TypesSize_NameId > 0)
            {
                mysql = mysql + " AND (Item.Items_TypesSize_NameId= " + Items_TypesSize_NameId + " ) ";
            }
            if (Items_TypesColor_NameId > 0)
            {
                mysql = mysql + " AND (Item.Items_TypesColor_NameId= " + Items_TypesColor_NameId + " ) ";
            }
            if (Items_TypesDegreeFarz_NameId > 0)
            {
                mysql = mysql + " AND (Item.Items_TypesDegreeFarz_NameId= " + Items_TypesDegreeFarz_NameId + " ) ";
            }



            if (Item_Somk > -1)
            {
                mysql = mysql + " AND (Item.Somk= " + Item_Somk + " ) ";
            }
            if (Item_Height_ > -1)
            {
                mysql = mysql + " AND (Item.Height_= " + Item_Height_ + " ) ";
            }
            if (Item_Width_ > -1)
            {
                mysql = mysql + " AND (Item.Width_= " + Item_Width_ + " ) ";
            }

            mysql = mysql + "  ORDER BY Item_Category.Main_Item_Category_Id,Item_Category.ID,  Item.Item_Name ";
            try
            {

                Items_dt = new DataTable();

                Items_dt = opp.return_DataTable(mysql);



            }
            catch
            { }


            return Items_dt;
        }


        public DataTable CreateTempTableAddLast_DatePurchase_PriceFor_ItemQntyGetAVGCostForItemBalance(long Store_Id, int DaysAdded, bool All, int Item_Category_ID)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            Items_dt = CreateTempTableAddLast_DatePurchase_PriceFor_ItemQntyGetAVGCostForItemBalance(ref opp, DaysAdded,Store_Id,  All, Item_Category_ID);

            return Items_dt;
        }
        public DataTable CreateTempTableAddLast_DatePurchase_PriceFor_ItemQntyGetAVGCostForItemBalance(ref AccountModelsLib.operations opp, int DaysAdded, long Store_Id, bool All, int Item_Category_ID)
        {

            CreateDBTable(ref opp);


            // Insert the Purchase price for last item quntity


            try
            {
                mysql = @"
INSERT INTO ItemCost
                  ( Bills_DetailsId, Invoice_Date, Unit_Item_Table_Id, totalFull, AVGPrice, [ROW NUMBER], FirstValues,FirstValuesAdded)
	 SELECT  Bills_DetailsId,Invoice_Date,Larg_Unit_Price,Unit_Item_Table_Id,       AVGPrice  ,FirstValues,FirstValuesAdded
FROM (			  
   	SELECT Bills_Details.Id as Bills_DetailsId,Bills_Details.Larg_Unit_Price, Bills_Details.Unit_Item_Table_Id,Invoice_Date,Larg_Qnty
	
	 
	,  AVg(Bills_Details.Larg_Unit_Price) OVER (PARTITION BY Bills_Details.Unit_Item_Table_Id ORDER BY Bills_Details.Unit_Item_Table_Id,Invoice_Date DESC ) AS AVGPrice 
	  
	  ,first_value(Invoice_Date)
over(PARTITION BY Unit_Item_Table_Id ORDER BY Invoice_Date asc 
 )  as FirstValues ";

                mysql = mysql + " , DATEADD(day," + DaysAdded + ",  first_value(Invoice_Date) over(PARTITION BY Unit_Item_Table_Id ORDER BY Invoice_Date asc   ) )as FirstValuesAdded ";
                mysql = mysql + @"
FROM     Bills_Details INNER JOIN
                  Bills_List ON Bills_Details.Invoice_Id = Bills_List.ID    
GROUP BY Bills_Details.Id,Bills_Details.Unit_Item_Table_Id, Bills_Details.Larg_Unit_Price ,Invoice_Date,Larg_Qnty
 )t INNER JOIN
                  Unit_item ON t.Unit_Item_Table_Id = Unit_item.ID   

				  where FirstValuesAdded>=Invoice_Date 

     ";
                opp.insert_me(mysql);
                //DataTable dt = opp.return_DataTable(mysql);
            }
            catch (Exception ee)
            {
            }

            AddAvgPriceToTableItemCosts(ref opp, Store_Id);

            return GetAVGCostForItemBalance(ref opp, Store_Id, All, Item_Category_ID);
        }



        public DataTable CreateTempTableRangeOf_DatePurchase_PriceFor_ItemQntyGetAVGCostForItemBalance(  long Store_Id, bool IsDateFrom, DateTime dtFrom, bool IsDateTo, DateTime dtTo, bool All, int Item_Category_ID)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            Items_dt = CreateTempTableRangeOf_DatePurchase_PriceFor_ItemQntyGetAVGCostForItemBalance(ref   opp,   Store_Id,   IsDateFrom,   dtFrom,   IsDateTo,   dtTo,   All,   Item_Category_ID);

            return Items_dt;
        }
        DataTable CreateTempTableRangeOf_DatePurchase_PriceFor_ItemQntyGetAVGCostForItemBalance(ref AccountModelsLib.operations opp, long Store_Id, bool IsDateFrom, DateTime dtFrom, bool IsDateTo, DateTime dtTo, bool All, int Item_Category_ID)
        {

            CreateDBTable(ref opp);


            // Insert the Purchase price for last item quntity


            try
            {
                mysql = @"
INSERT INTO ItemCost
                  ( Bills_DetailsId, Invoice_Date, Unit_Item_Table_Id, totalFull, AVGPrice, [ROW NUMBER], FirstValues,FirstValuesAdded)
	 SELECT  Bills_DetailsId,Invoice_Date,Larg_Unit_Price,Unit_Item_Table_Id,       AVGPrice  ,FirstValues,FirstValuesAdded
FROM (			  
   	SELECT Bills_Details.Id as Bills_DetailsId,Bills_Details.Larg_Unit_Price, Bills_Details.Unit_Item_Table_Id,Invoice_Date,Larg_Qnty
	
	 
	,  AVg(Bills_Details.Larg_Unit_Price) OVER (PARTITION BY Bills_Details.Unit_Item_Table_Id ORDER BY Bills_Details.Unit_Item_Table_Id,Invoice_Date DESC ) AS AVGPrice 
	  
	  ,first_value(Invoice_Date)
over(PARTITION BY Unit_Item_Table_Id ORDER BY Invoice_Date asc 
 )  as FirstValues 

                 , DATEADD(day, 2,  first_value(Invoice_Date) over(PARTITION BY Unit_Item_Table_Id ORDER BY Invoice_Date asc   ) )as FirstValuesAdded 
        
FROM     Bills_Details INNER JOIN
                  Bills_List ON Bills_Details.Invoice_Id = Bills_List.ID    
GROUP BY Bills_Details.Id,Bills_Details.Unit_Item_Table_Id, Bills_Details.Larg_Unit_Price ,Invoice_Date,Larg_Qnty
 )t INNER JOIN
                  Unit_item ON t.Unit_Item_Table_Id = Unit_item.ID   ";


                if (IsDateFrom == true)
                {
                    mysql = mysql + " where    CONVERT(date, Invoice_Date)   >= '" + AccountModelsLib.operations.dateFormat(dtFrom) + "'  ";
                }
                if (IsDateTo == true)
                {
                    mysql = mysql + " where    CONVERT(date, Invoice_Date)   <= '" + AccountModelsLib.operations.dateFormat(dtTo) + "'  ";
                }
                opp.insert_me(mysql);
                //DataTable dt = opp.return_DataTable(mysql);
            }
            catch (Exception ee)
            {
            }

            AddAvgPriceToTableItemCosts(ref opp, Store_Id);

            return GetAVGCostForItemBalance(ref opp, Store_Id, All, Item_Category_ID);
        }





    }
}
