using System;
using System.Collections.Generic;
using System.Text;
 
using System.Data;
  using AccountModelsLib; using AccountModelsLib.Models;
using System.Collections;
namespace AccountModelsLib
{
    public class Stores_Balance
    {
        public Stores_Balance()
        {

        }

        /// <summary>
        /// „Œ“‰ „Ê«œ √Œ— «·„œ…
        /// </summary>
        /// <returns></returns>
        public decimal  GetRawMaterialLast()
        {
            int Excp = 0;
            string mysql = " SELECT SUM((Unit_item.Full_qua + Unit_item.D_Qua / unit.S_unit_Value) * Unit_item.P_price) AS Price_RawMaterial ";
            mysql = mysql + " FROM Unit_item INNER JOIN unit ON Unit_item.unit_id = unit.id INNER JOIN Store ON Unit_item.Store_id = Store.id ";
            mysql = mysql + " WHERE     (Unit_item.Store_Type = 4) AND (Store.StoreUsed_Type = 2) ";


            return  Get_Price_Items_Data(ref Excp, mysql,"Price_RawMaterial");
           
        }

         /// <summary>
        /// «‰ «Ã  «„  √Œ— «·„œ…
        /// </summary>
        /// <returns></returns>
        public decimal GetFinalProductLast()
        {
            int Excp = 0;
            string mysql = " SELECT SUM((Unit_item.Full_qua + Unit_item.D_Qua / unit.S_unit_Value) * Unit_item.P_price) AS Price_RawMaterial ";
            mysql = mysql + " FROM Unit_item INNER JOIN unit ON Unit_item.unit_id = unit.id INNER JOIN Store ON Unit_item.Store_id = Store.id ";
            mysql = mysql + " WHERE     (Unit_item.Store_Type = 4) AND (Store.StoreUsed_Type = 1) ";


            return Get_Price_Items_Data(ref Excp, mysql, "Price_RawMaterial");
           
           
        }

        /// <summary>
        /// √‰ «Ã  Õ  «· ‘€Ì· √Œ— «·„œ…
        /// </summary>
        /// <returns></returns>
        public decimal  GetUnderWorkingMaterialLast()
        {
            int Excp = 0;
            string mysql = " SELECT SUM((Unit_item.Full_qua + Unit_item.D_Qua / unit.S_unit_Value) * Unit_item.P_price) AS Price_RawMaterial  ";
            mysql = mysql + " FROM Unit_item INNER JOIN   unit ON Unit_item.unit_id = unit.id INNER JOIN  Department ON Unit_item.Store_id = Department.Id  ";
            mysql = mysql + "  WHERE (Unit_item.Store_Type = 0) ";


            return Get_Price_Items_Data(ref Excp, mysql, "Price_RawMaterial");
        }
       

        /// <summary>
        /// „Œ“‰ „Ê«œ √Ê· «·„œ…
        /// «‰ «Ã  Õ  «· ‘€Ì· «Ê· «·„œ…
        /// «‰ «Ã  «„ «Ê· «·„œ…
        /// </summary>
        /// <param name="Excp"></param>
        /// <param name="mysql"></param>
        /// <param name="ColName"></param>
        /// <returns></returns>
        /// 
        public void Get_First_Stoes_Data(DateTime DateFrom, DateTime DateTo, ref decimal RawMaterialFirst, ref decimal UnderWorkingMaterialFirst, ref decimal FinalProductFirst)
        {
        string mysql = " SELECT *  ";
              mysql = mysql + " FROM Financial_Period_Stores_Balance " ;
              mysql = mysql + " WHERE (DateFrom < '" + operations.dateFormat(DateTo) + "') ";
              mysql = mysql + " ORDER BY ID DESC ";
            int Excp=0;
            int counter=0;
            DataSet ds=Get_Requests_Items_Data(ref Excp,mysql);
            if(ds.Tables[0].Rows.Count>0)
            {
                RawMaterialFirst = decimal.Parse(ds.Tables[0].Rows[0]["RawMaterialLast"].ToString());
                UnderWorkingMaterialFirst = decimal.Parse(ds.Tables[0].Rows[0]["UnderWorkingMaterialLast"].ToString());
                FinalProductFirst = decimal.Parse(ds.Tables[0].Rows[0]["FinalProductLast"].ToString());
            }
        }

        public void Get_All_Financial_Period_Stores_Balance(DateTime DateFrom, DateTime DateTo, ref decimal RawMaterialFirst, ref decimal UnderWorkingMaterialFirst, ref decimal FinalProductFirst, ref decimal RawMaterialLast, ref decimal UnderWorkingMaterialLast, ref decimal FinalProductLast)
        {
            string mysql = " SELECT *  ";
            mysql = mysql + " FROM Financial_Period_Stores_Balance ";
            mysql = mysql + " WHERE (DateFrom = '" + operations.dateFormat(DateFrom) + "') AND (DateTo = '" + operations.dateFormat(DateTo) + "') ";
            mysql = mysql + " ORDER BY ID DESC ";
            int Excp = 0;
            int counter = 0;
            DataSet ds = Get_Requests_Items_Data(ref Excp, mysql);
            if (ds.Tables[0].Rows.Count > 0)
            {
                RawMaterialFirst = decimal.Parse(ds.Tables[0].Rows[0]["RawMaterialFirst"].ToString());
                UnderWorkingMaterialFirst = decimal.Parse(ds.Tables[0].Rows[0]["UnderWorkingMaterialFirst"].ToString());
                FinalProductFirst = decimal.Parse(ds.Tables[0].Rows[0]["FinalProductFirst"].ToString());
                RawMaterialLast = decimal.Parse(ds.Tables[0].Rows[0]["RawMaterialLast"].ToString());
                UnderWorkingMaterialLast = decimal.Parse(ds.Tables[0].Rows[0]["UnderWorkingMaterialLast"].ToString());
                FinalProductLast = decimal.Parse(ds.Tables[0].Rows[0]["FinalProductLast"].ToString());
            }
        }
        public DataSet  Get_All_Financial_Period_Stores_Balance()
        {
            string mysql = " SELECT *  ";
            mysql = mysql + " FROM Financial_Period_Stores_Balance ";
             mysql = mysql + " ORDER BY ID DESC ";
             int Excp = 0;
           return  Get_Requests_Items_Data(ref Excp, mysql);
             
        }
        private decimal  Get_Price_Items_Data(ref int Excp, string mysql,string ColName)
        {
            DataSet ds = new DataSet();
            decimal Price_Material=0;
            try
            {
                operations opp = new operations();
                ds = opp.return_dataset(mysql);
                Excp = 1;
                opp = null;
               
                 if (ds.Tables[0].Rows.Count > 0)
            {
                if (ds.Tables[0].Rows[0][ColName].ToString() == "")
                {
                    Price_Material = 0;
                }
                else
                {
                    Price_Material = decimal.Parse(ds.Tables[0].Rows[0][ColName].ToString());
                }
            }
            else
            {
                Price_Material = 0;
            }

            return Common.RoundMe(Price_Material);
            }
            catch (Exception ee)
            {

                Excp = -1;
                //MessageBox.Show(ee.Message);
                 Price_Material = 0;
                 return Common.RoundMe(Price_Material);
            }
        }


        public void Add_New_StoreBalancePeriod(ref int Excp, DateTime DateFrom, DateTime DateTo, decimal RawMaterialFirst, decimal RawMaterialLast, decimal UnderWorkingMaterialFirst, decimal UnderWorkingMaterialLast, decimal FinalProductFirst, decimal FinalProductLast)
            
        {
            try
            { 
                string mysql = "";
                 mysql = " INSERT INTO Financial_Period_Stores_Balance (DateFrom, DateTo, RawMaterialFirst, RawMaterialLast, UnderWorkingMaterialFirst, UnderWorkingMaterialLast, FinalProductFirst, FinalProductLast) ";
                 mysql = mysql + " VALUES ('" + operations.dateFormat(DateFrom) + "','" + operations.dateFormat(DateTo) + "'," + RawMaterialFirst + "," + RawMaterialLast + "," + UnderWorkingMaterialFirst + "," + UnderWorkingMaterialLast + "," + FinalProductFirst + "," + FinalProductLast + ") ";
                 
                operations opp = new operations();
                opp.insert_me(mysql);
                
               Excp = 0;  
                
            }
            catch (Exception ee)
            { 
               // MessageBox.Show(ee.Message);
                Excp = 1; 
            }
        }

        public void Delete_StoreBalancePeriod(ref int Excp, int index)
        {
            try
            {
                string mysql = "";
                mysql = " Delete from Financial_Period_Stores_Balance  where Id=" + index;
                operations opp = new operations();
                opp.delete_me(mysql);

                Excp = 0;

            }
            catch (Exception ee)
            { 
                //MessageBox.Show(ee.Message);
                Excp = 1; 
            }
        }

        private DataSet Get_Requests_Items_Data(ref int Excp, string mysql)
        {
            DataSet G_DS = new DataSet();
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
    }
}
