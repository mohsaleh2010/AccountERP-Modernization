using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using Microsoft.Data.SqlClient;
using System.Data.OleDb;
  using AccountModelsLib; using AccountModelsLib.Models;


namespace AccountModelsLib
{
    public class POS_InvoiceCLS
    {
        public POS_InvoiceCLS()
        {
        }
        public DataTable Get_Item_Category(int Id)
        {
            string mysql = " Select * from Item_Category ";
            if (Id > 0)
            {
                 mysql = mysql + " Where Category_Type_Id = " + Id;
            }
            operations opp = new operations();


            DataTable dt = opp.return_DataTable(mysql); ;
            return dt;

        }
        public DataTable GetItemsImage_ByCategory_ID(int Item_Category_ID)
        {
            operations opp = new operations();
            string mysql = @" SELECT        Item_ImagesStore.*, Item.Item_Name

FROM            Item INNER JOIN
                         Item_Category ON Item.Item_Category_ID = Item_Category.ID INNER JOIN
                         Item_ImagesStore ON Item.id = Item_ImagesStore.Item_Id
  ";
            mysql = mysql + " WHERE     (Item.Item_Category_ID = " + Item_Category_ID + ") AND (Item.Active_flage=1)";
            mysql = mysql + " Order By Item.Item_Name ";
            DataTable dt = opp.return_DataTable(mysql); ;
            return dt;

        }
        public DataTable GetAllItemsImage_ByCategory_ID(int Item_Category_ID)
        {
            operations opp = new operations();
            string mysql = @" SELECT        Item_ImagesStore.ImageId, Item_ImagesStore.Description, Item_ImagesStore.OriginalPath, Item_ImagesStore.ImageData, Item_ImagesStore.Default_Img_Flage, Item_ImagesStore.Item_Type, Item.Item_Name, 
                         Item_Category.Category_Name, Item.id AS Item_Id
FROM            Item INNER JOIN
                         Item_Category ON Item.Item_Category_ID = Item_Category.ID LEFT OUTER JOIN
                         Item_ImagesStore ON Item.id = Item_ImagesStore.Item_Id
  ";
            mysql = mysql + @" WHERE (Item.Active_flage = 1) AND (Item_ImagesStore.Default_Img_Flage = 1) OR
                         (Item_ImagesStore.Default_Img_Flage IS NULL) ";
            if(Item_Category_ID>0)
            {
                mysql = mysql + " AND (Item.Item_Category_ID = " + Item_Category_ID + ") ";
            }
            mysql = mysql + " Order By Item.Item_Name ";
            DataTable dt = opp.return_DataTable(mysql);   
            return dt;

        }
        public DataTable GetItemsList_ByCategory_ID(int Item_Category_ID)
        {

            operations opp = new operations();
            string mysql = @" SELECT        Item.Item_Name, Unit_item.Id, Unit_item.Item_id, Unit_item.Store_id, Unit_item.Store_Type, Unit_item.Code, Unit_item.unit_id, Unit_item.Full_qua, Unit_item.D_Qua, Unit_item.Item_No, Unit_item.Reserved_Balance_Larg_Qnty, 
                         Unit_item.Reserved_Balance_Small_Qnty, Unit_item.Big_unit, Unit_item.Small_unit, Unit_item.P_price, Unit_item.S_price, Unit_item.Specification, Unit_item.Upon_Request, Unit_item.Currency, Unit_item.active_flage, 
                         Unit_item.TaxSales10_Value, unit.u_name, unit.B_Unit, unit.S_unit, unit.B_Unit_Value, unit.S_unit_Value, Unit_item.P_price * (Unit_item.D_Qua / Unit_item.Small_unit + Unit_item.Full_qua) AS price, 
                         Unit_item.P_price AS P_Price_, Unit_item.S_price * (Unit_item.D_Qua / Unit_item.Small_unit + Unit_item.Full_qua) AS Total_Sell_price, Unit_item.D_Qua / Unit_item.Small_unit + Unit_item.Full_qua AS TotalSmallQnty, 
                         Item.No3eboa, Item_Category.Category_Name, Item.Item_Category_ID
FROM            Item INNER JOIN
                         Unit_item ON Item.Id = Unit_item.Item_id AND Item.S_id = Unit_item.Store_id INNER JOIN
                         unit ON Unit_item.unit_id = unit.id INNER JOIN
                         Item_Category ON Item.Item_Category_ID = Item_Category.ID ";
            mysql = mysql + " WHERE     (Item.Item_Category_ID = " + Item_Category_ID + ") AND (Item.Active_flage=1)";
            mysql = mysql + " Order By Item.Item_Name ";
            DataTable dt = opp.return_DataTable(mysql);  ;
            return dt;
        }
        public DataTable GetItemsList_ByCategory_IDWithImage(int Item_Category_ID)
        {

            operations opp = new operations();
            string mysql = @" SELECT     Item.Item_Name, Unit_item.id, Unit_item.Item_id, Unit_item.Store_id, Unit_item.Store_Type, Unit_item.Code, Unit_item.unit_id, Unit_item.Full_qua, Unit_item.D_Qua, 
                      Unit_item.Item_No, Unit_item.Reserved_Balance_Larg_Qnty, Unit_item.Reserved_Balance_Small_Qnty, Unit_item.Big_unit, Unit_item.Small_unit, Unit_item.P_price, 
                      Unit_item.S_price, Unit_item.Specification, Unit_item.Upon_Request, Unit_item.Currency, Unit_item.active_flage, Unit_item.TaxSales10_Value, unit.u_name, unit.B_Unit, 
                      unit.S_unit, unit.B_Unit_Value, unit.S_unit_Value, Unit_item.P_price * (Unit_item.D_Qua / Unit_item.Small_unit + Unit_item.Full_qua) AS price, 
                      Unit_item.P_price AS P_Price_, Unit_item.S_price * (Unit_item.D_Qua / Unit_item.Small_unit + Unit_item.Full_qua) AS Total_Sell_price, 
                      Unit_item.D_Qua / Unit_item.Small_unit + Unit_item.Full_qua AS TotalSmallQnty, Unit_item.TaxSales10_Value AS Expr1, Item.No3eboa, Item_Category.Category_Name, 
                      Item.Item_Category_ID
FROM         Item INNER JOIN
                      Unit_item ON Item.id = Unit_item.Item_id INNER JOIN
                      unit ON Unit_item.unit_id = unit.id INNER JOIN
                      Item_Category ON Item.Item_Category_ID = Item_Category.ID";
            mysql = mysql + " WHERE     (Item.Item_Category_ID = " + Item_Category_ID + ") AND (Item.Active_flage=1)";
            mysql = mysql + " Order By Item.Item_Name ";
            DataTable dt = opp.return_DataTable(mysql); ;
            return dt;
        }
        public DataTable GetItemsList_Data(string Item_Code)
        {
            operations opp = new operations();
            string mysql = @" SELECT     Item.Item_Name, Unit_item.id, Unit_item.Item_id, Unit_item.Store_id, Unit_item.Store_Type, Unit_item.Code, Unit_item.unit_id, Unit_item.Full_qua, Unit_item.D_Qua, 
                      Unit_item.Item_No, Unit_item.Reserved_Balance_Larg_Qnty, Unit_item.Reserved_Balance_Small_Qnty, Unit_item.Big_unit, Unit_item.Small_unit, Unit_item.P_price, 
                      Unit_item.S_price, Unit_item.Specification, Unit_item.Upon_Request, Unit_item.Currency, Unit_item.active_flage, Unit_item.TaxSales10_Value, unit.u_name, unit.B_Unit, 
                      unit.S_unit, unit.B_Unit_Value, unit.S_unit_Value, Unit_item.P_price * (Unit_item.D_Qua / Unit_item.Small_unit + Unit_item.Full_qua) AS price, 
                      Unit_item.P_price AS P_Price_, Unit_item.S_price * (Unit_item.D_Qua / Unit_item.Small_unit + Unit_item.Full_qua) AS Total_Sell_price, 
                      Unit_item.D_Qua / Unit_item.Small_unit + Unit_item.Full_qua AS TotalSmallQnty, Unit_item.TaxSales10_Value AS Expr1, Item.No3eboa, Item_Category.Category_Name, 
                      Item.Item_Category_ID
FROM         Item INNER JOIN
                      Unit_item ON Item.id = Unit_item.Item_id INNER JOIN
                      unit ON Unit_item.unit_id = unit.id INNER JOIN
                      Item_Category ON Item.Item_Category_ID = Item_Category.ID";
            mysql = mysql + " WHERE     (Unit_item.Code ='" + Item_Code + "') AND (Item.Active_flage=1)";

            DataTable dt = opp.return_DataTable(mysql); ;
            return dt;
        }
    }
}
