using System;
using System.Collections.Generic;
using System.Text;
 using AccountModelsLib; using AccountModelsLib.Models;
using System.Data;

namespace AccountModelsLib
{


    public class Price_Type_ListCLS
    {
        operations opp;
        DataSet machineds;
        public Price_Type_ListCLS()
        {
            opp = new operations();
            machineds = new DataSet();
        }
        public DataTable GetPrice_Name_list(int ShowByActive)
        {
            //Where IS_Active=1
            string mysql = "SELECT * FROM Price_Type_List Where  IS_Active=" + ShowByActive;

            mysql = mysql + "  Order By Price_Type_List_Master_Id";
            
             
            return  opp.return_DataTable(mysql);
             
        }

        public DataTable GetPrice_Purchase_Name_list(int ShowByActive)
        {
            //Where IS_Active=1
            string mysql = "SELECT * FROM Price_Purhase_Type_List Where  IS_Active=" + ShowByActive;

            mysql = mysql + "  Order By  Id";


            return opp.return_DataTable(mysql);

        }
        public bool ISPrice_ForMain( )
        {
            bool FoundMain = false;
            //Where IS_Active=1
            string mysql = "SELECT * FROM Price_Type_List  Where Price_Type_List_Master_Id=-1";
             
            DataTable dt= opp.return_DataTable(mysql);
            if(dt.Rows.Count>0)
            {
                FoundMain = true;
            }
            else
            {
                FoundMain = false;
            }
            return FoundMain;

        }
        /// ////////////////////////////////////

        public void Add_Price_Name(string Price_Name,string Price_Name_Master,int Price_Type_List_Master_Id,string  Plus_Minus,decimal  Added_Value,int IS_Rate)
        {
            string sql;
            sql = "INSERT INTO Price_Type_List(Price_Name,Price_Name_Master,Price_Type_List_Master_Id,Plus_Minus,Added_Value,IS_Rate)";
            sql += "VALUES('" + Price_Name + " ','" + Price_Name_Master + " ', " + Price_Type_List_Master_Id + ",'" + Plus_Minus + " ', " + Added_Value + " ," + IS_Rate + "  )";
            opp.insert_me(sql);

        }
        public void Add_PricePurhase_Name(string Price_Name )
        {
            string sql;
            sql = "INSERT INTO Price_Purhase_Type_List(Price_Name )";
            sql += "VALUES('" + Price_Name + " '  )";
            opp.insert_me(sql);

        }
        public void update_Price_Name(int id, string Price_Name, string Price_Name_Master, int Price_Type_List_Master_Id, string Plus_Minus, decimal Added_Value, int IS_Rate)
        {

            string mysql = "UPDATE Price_Type_List SET Price_Name = '" + Price_Name + "'";
            mysql = mysql + " , Price_Name_Master = '" + Price_Name_Master + "'";
            mysql = mysql + " , Price_Type_List_Master_Id =  " + Price_Type_List_Master_Id + " ";
            mysql = mysql + " ,Plus_Minus = '" + Plus_Minus + "' ";
            mysql = mysql + " ,Added_Value =  " + Added_Value + "  ";
            mysql = mysql + " , IS_Rate =  " + IS_Rate + " ";


            mysql = mysql + " Where id=" + id;
            opp.update_me(mysql);
        }
        public void update_Price_Purchase_Name(int id, string Price_Name )
        {

            string mysql = "UPDATE Price_Purhase_Type_List SET Price_Name = '" + Price_Name + "'";
           


            mysql = mysql + " Where id=" + id;
            opp.update_me(mysql);
        }
        public void Delete_Price_Name(int id, int Value)
        {

            string mysql = "UPDATE Price_Type_List SET IS_Active=" + Value + " Where id=" + id;
            opp.update_me(mysql);
        }

        public void Delete_Price_Purhase_Name(int id, int Value)
        {

            string mysql = "UPDATE Price_Purhase_Type_List SET IS_Active=" + Value + " Where id=" + id;
            opp.update_me(mysql);
        }

    }

}
