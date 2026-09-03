namespace AccountModelsLib
{
     
    using System;
    using System.Data;

    public class ItemDataPhCLS
    {
        public DataTable Get_Items(string Item_Name,long Unit_item_Id,string Item_Code)
        {
            int excp = 0;
            string mysql = @" SELECT Unit_item.Code, Unit_item.id AS Unit_item_Id, Unit_item.Reserved_Balance_Larg_Qnty, Unit_item.Reserved_Balance_Small_Qnty, Item.S_id, unit.id AS unit_id, Item.S_id AS Store_id, unit.u_name, unit.B_Unit AS Larg_Unit,                           unit.S_unit AS Small_unit, Unit_item.Upon_Request, Unit_item.unit_id AS Expr1, Unit_item.Specification, Unit_item.Big_unit AS Larg_unit_Value, Unit_item.Small_unit AS Small_unit_Value, Unit_item.P_price AS Purchase_Price,                           Unit_item.S_price AS Sale_price, Unit_item.Item_id, Unit_item.Currency, Item.code AS Item_code, Item.Item_Name, Item.AnotherName, Unit_item.Full_qua, Unit_item.D_Qua, Store.Store_Name, Item.No3eboa, Store.Store_Code,                           Item.Width_, Item.Height_, Item.Somk, Item.Somk_Unit, Item.Items_Types_Name, Item.Items_Types_Name_Id,Unit_item.Code, Unit_item.S_price_LargeUnit, Unit_item.S_price_SmallUnit, Unit_item.P_price_LargeUnit, Unit_item.P_price_SmallUnit, Unit_item.Unit_Used_For_S_price_Default, Unit_item.Unit_Used_For_P_price_Default, 
                         Unit_item.S_price_Item_No, Unit_item.P_price_Item_No ";
            mysql = mysql + " FROM Unit_item INNER JOIN  unit ON Unit_item.unit_id = unit.id INNER JOIN Item ON Unit_item.Item_id = Item.id INNER JOIN                          Store ON Unit_item.Store_id = Store.id " + "";
            mysql = mysql + " WHERE  (Item.Id > 0) ";
            if (Item_Name.Trim()!="")
            {
                mysql=mysql+ " AND ((Item.Item_Name like N'%" + Item_Name.Trim() +  "%') ";
                mysql = mysql + " OR  (Item.AnotherName like N'%" + Item_Name.Trim() + "%') )";
            }
            if (Unit_item_Id>0)
            {
                mysql = mysql + " And (Unit_item.id  = " + Unit_item_Id + " ) ";
            }
            if (Item_Code.Trim() != "")
            {
                mysql = mysql + " And (Item.Code like N'% " + Item_Code + "%') ";
            }
                            return this.Get_Requests_Items_Data(ref excp, mysql);
        }
        public DataTable Get_Items(  string Item_Code)
        {
            int excp = 0;
            string mysql = @" SELECT DISTINCT 
                         Unit_item.Code, Unit_item.id AS Unit_item_Id, Unit_item.Reserved_Balance_Larg_Qnty, Unit_item.Reserved_Balance_Small_Qnty, Item.S_id, unit.id AS unit_id, Item.S_id AS Store_id, unit.u_name, unit.B_Unit AS Larg_Unit, 
                         unit.S_unit AS Small_unit, Unit_item.Upon_Request, Unit_item.unit_id AS Expr1, Unit_item.Specification, Unit_item.Big_unit AS Larg_unit_Value, Unit_item.Small_unit AS Small_unit_Value, Unit_item.P_price AS Purchas_Price, 
                         Unit_item.S_price AS Sale_price, Unit_item.Item_id, Unit_item.Currency, Item.code AS Item_code, Item.Item_Name, Item.AnotherName, Unit_item.Full_qua, Unit_item.D_Qua, Store.Store_Name, Item.No3eboa, Store.Store_Code, 
                         Item.Width_, Item.Height_, Item.Somk, Item.Somk_Unit, Item.Items_Types_Name, Item.Items_Types_Name_Id, Unit_item.Code AS Expr2, Unit_item.S_price_LargeUnit, Unit_item.S_price_SmallUnit, Unit_item.P_price_LargeUnit, 
                         Unit_item.P_price_SmallUnit, Unit_item.Unit_Used_For_S_price_Default, Unit_item.Unit_Used_For_P_price_Default, Unit_item.S_price_Item_No, Unit_item.P_price_Item_No
FROM            Unit_item INNER JOIN
                         unit ON Unit_item.unit_id = unit.id INNER JOIN
                         Item ON Unit_item.Item_id = Item.id INNER JOIN
                         Store ON Unit_item.Store_id = Store.id LEFT OUTER JOIN
                         Unit_Item_ExpireDate ON Unit_item.id = Unit_Item_ExpireDate.Unit_Item_Table_Id ";

            mysql = mysql + " Where (Item.Code =  '" + Item_Code + "')  ";
            mysql = mysql + " OR (Item.BarCode2 =  '" + Item_Code + "')   ";
            mysql = mysql + " OR (Item.BarCode3 =  '" + Item_Code + "')   ";
            mysql = mysql + " OR (Item.BarCode4 =  '" + Item_Code + "')   ";
            mysql = mysql + " OR (Item.BarCode4 =  '" + Item_Code + "')   ";
            //OR Unit_Item_ExpireDate.Code=  '" + Item_Code + "'
            //if (Item_Code.Trim() != "")
            //{
            //    mysql = mysql + " Where (Item.Code like N'% " + Item_Code + "%') ";
            //}
            return this.Get_Requests_Items_Data(ref excp, mysql);
        }

        public DataTable Get_ItemsExpire(long Unit_Item_Id)
        {
            int excp = 0;
            string mysql = "SELECT * FROM Unit_Item_ExpireDate ";
            mysql = mysql + " WHERE  Unit_Item_Table_Id = " + Unit_Item_Id.ToString() + " AND ( (Larg_Balance > 0) OR(Small_Balance > 0) OR(Item_No > 0) )";
            mysql = mysql + " ORDER BY Item_Expire_Date Asc";
            return this.Get_Requests_Items_Data(ref excp, mysql);
        }

        private DataTable Get_Requests_Items_Data(ref int Excp, string mysql)
        {
            DataTable table = new DataTable();
            try
            {
                table = new operations().return_DataTable(mysql);
                Excp = 1;
                int c = table.Rows.Count;
            }
            catch (Exception)
            {
                Excp = 1;
            }
            return table;
        }
    }
}

