using System;
 
using System.Collections.Generic;
using System.Text;
   
using System.Data;
namespace AccountModelsLib.Models
{
    public class OrderChoiceCLS
    {
        DataSet G_DS;
        public OrderChoiceCLS()
        { }
        public DataSet Get_Store(ref int Excp,int LookUp_StoreOrderTypesId=4,int IS_Virtual_OuterStore=0)
        {
            string mysql;
            mysql = "  Select   id, Store_Name as Name , Store_location, Emp_id, Job_id From Store ";
            mysql = mysql + "   Where Active=1  ";
            
                mysql = mysql + "   And  IS_Virtual_OuterStore= "+ IS_Virtual_OuterStore;
                mysql = mysql + "   And  LookUp_StoreOrderTypesId= " + LookUp_StoreOrderTypesId;
                mysql = mysql + "   And  IS_ServicesVirtual_Item_NoBalance= 0 ";
            
            mysql = mysql + "   ORDER BY IS_Default_Store DESC, Name, StoreUsed_Type ";

            return Get_Items_Data(ref Excp, mysql);

        }
        public DataSet Get_Department(ref int Excp)
        {
            string mysql;
            mysql = " Select Id, dname AS Name , location  from Department  ";// Where Active=1 ";
            return Get_Items_Data(ref Excp, mysql);

        }

        public DataSet Get_Supplier(ref int Excp)
        {
            string mysql;
            mysql = " Select * from Supplier where active=1 ";
            return Get_Items_Data(ref Excp, mysql);

        }
        public DataSet Get_Customer(ref int Excp)
        {
            string mysql;
            mysql = " Select * from Customer where active=1 ";
            return Get_Items_Data(ref Excp, mysql);

        }
        public DataSet Get_Employee(ref int Excp)
        {
            string mysql;
            mysql = " Select * from Employee where active=1 ";
            return Get_Items_Data(ref Excp, mysql);

        }
        public DataSet Get_Machine(ref int Excp)
        {
            string mysql;
            mysql = " SELECT Id, Machin_Name AS Name FROM Machin_list WHERE        (active = 1) ";
            return Get_Items_Data(ref Excp, mysql);

        }
        
        public DataSet Get_Item_List(ref int Excp, int Store_Type, long Store_id)
        {
            string mysql;
            mysql = @"SELECT  unit.u_name, unit.B_Unit, unit.S_unit, unit.B_Unit_Value, unit.S_unit_Value, Unit_item.id, Unit_item.Item_id, Unit_item.Store_id, Unit_item.Store_Type, 
                        Item.Code,  Unit_item.Code as Code1, Unit_item.unit_id, Unit_item.Full_qua, Unit_item.D_Qua, Unit_item.Item_No, Unit_item.Reserved_Balance_Larg_Qnty, 
                         Unit_item.Reserved_Balance_Small_Qnty, Unit_item.Big_unit, Unit_item.Small_unit, Unit_item.P_price, Unit_item.S_price, Unit_item.Specification, 
                         Unit_item.Upon_Request, Unit_item.Currency, Unit_item.active_flage, Unit_item.TaxSales10_Value, Item.Item_Name, Unit_item.id AS Unit_itemTable_id, Item.No3eboa, 
                         Item.Active_flage AS Expr1,CAST(Unit_item.Full_qua AS nvarchar) + N' : ' + N'' + unit.B_Unit AS Full_qua_B, CAST(Unit_item.D_Qua AS nvarchar) + N' : ' + N'' + unit.S_unit AS D_Qua_S
FROM            Item INNER JOIN
                         Unit_item ON Item.id = Unit_item.Item_id INNER JOIN
                         unit ON Unit_item.unit_id = unit.id";
            mysql = mysql + " WHERE (Unit_item.Store_Type = " + Store_Type + ") AND (Unit_item.Store_id = " + Store_id + ") AND (Item.Active_flage=1) AND (Unit_item.Active_flage=1) order by Item.id";
            DataSet ds = Get_Items_Data(ref Excp, mysql);

            //for(int x=0;x<ds.Tables[0].Rows.Count;x++)
            //{

            //    if(ds.Tables[0].Rows[x]["S_unit_Value"].ToString()== ds.Tables[0].Rows[x]["B_Unit_Value"].ToString())
            //    {
            //        ds.Tables[0].Rows[x]["D_Qua_S"]  = "";
            //        }
            //}
            return ds;

        }
        public long GetNewOrderNo(ref operations opp, int Order_Type)
        {
            string mysql;
            int Excp = 0;
            long Order_Purshing_No = 1;
            /* if (Order_Type == 1)
             {
                 Order_Type = 2;
             }
             * */
            mysql = " SELECT MAX(Order_Purshing_No) AS Order_Purshing_No FROM Order_Purshing WHERE (Order_Type = " + Order_Type + ") ";
            DataTable dt = Get_Items_Data(ref Excp, ref opp, mysql);

            if (dt.Rows.Count > 0)
            {
                if (dt.Rows[0]["Order_Purshing_No"].ToString() == "")
                {
                    Order_Purshing_No = 1;
                    return Order_Purshing_No;
                }
                else
                {
                    Order_Purshing_No = long.Parse(dt.Rows[0]["Order_Purshing_No"].ToString());
                    if (Order_Purshing_No < 0)
                    {
                        Order_Purshing_No = 1;// Order_Purshing_No + 100;
                    }
                    else
                    {
                        Order_Purshing_No = Order_Purshing_No + 1;
                    }
                    return Order_Purshing_No;
                }

            }
            else
            {
                Order_Purshing_No = 1;
                return Order_Purshing_No;
            }
        }
        public long GetNewOrderNo(int Order_Type)
        {
            string mysql;
            int Excp = 0;
            long Order_Purshing_No = 1;
            /* if (Order_Type == 1)
             {
                 Order_Type = 2;
             }
             * */
            mysql = " SELECT MAX(Order_Purshing_No) AS Order_Purshing_No FROM Order_Purshing WHERE (Order_Type = " + Order_Type + ") ";
            DataSet dss = Get_Items_Data(ref Excp, mysql);

            if (dss.Tables[0].Rows.Count > 0)
            {
                if (dss.Tables[0].Rows[0]["Order_Purshing_No"].ToString() == "")
                {
                    Order_Purshing_No = 1;
                    return Order_Purshing_No;
                }
                else
                {
                    Order_Purshing_No = long.Parse(dss.Tables[0].Rows[0]["Order_Purshing_No"].ToString());
                    if (Order_Purshing_No < 0)
                    {
                        Order_Purshing_No =1;
                    }
                    else
                    {
                        Order_Purshing_No = Order_Purshing_No + 1;
                    }
                    return Order_Purshing_No;
                }

            }
            else
            {
                Order_Purshing_No = 1;
                return Order_Purshing_No;
            }
        }

        public long GetNewItem_Raw_MaterialsFor_Entage_List()
        {
            string mysql;
            int Excp = 0;
            long Order_Purshing_No = 1;
            /* if (Order_Type == 1)
             {
                 Order_Type = 2;
             }
             * */
            mysql = " SELECT MAX(Process_No) AS Process_No FROM Item_Raw_MaterialsFor_Entage_List  ";
            DataSet dss = Get_Items_Data(ref Excp, mysql);

            if (dss.Tables[0].Rows.Count > 0)
            {
                if (dss.Tables[0].Rows[0]["Process_No"].ToString() == "")
                {
                    Order_Purshing_No = 150;

                }
                else
                {
                    Order_Purshing_No = long.Parse(dss.Tables[0].Rows[0]["Process_No"].ToString());

                    Order_Purshing_No = Order_Purshing_No + 1;


                }

            }
            else
            {
                Order_Purshing_No = 1;

            }
            return Order_Purshing_No;
        }

        private DataSet Get_Items_Data(ref int Excp, string mysql)
        {
            G_DS = new DataSet();
            try
            {
                operations opp = new operations();
                G_DS = opp.return_dataset(mysql);
                Excp = 1;
                opp = null;
                return G_DS;
            }
            catch (Exception ee)
            {

                Excp = -1;
                //MessageBox.Show(ee.Message);
                return G_DS;
            }
        }
        private DataTable Get_Items_Data(ref int Excp, ref operations opp, string mysql)
        {
            DataTable dt = new DataTable();
            try
            {

                dt = opp.return_DataTable(mysql);
                Excp = 0;

                return dt;
            }
            catch (Exception ee)
            {

                Excp = 1;
                //MessageBox.Show(ee.Message);
                return dt;
            }
        }
    }
}
