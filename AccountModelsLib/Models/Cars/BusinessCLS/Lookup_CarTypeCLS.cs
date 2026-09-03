using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using Microsoft.Data.SqlClient;

 
using AccountModelsLib.Models;
 
using System.Linq;
namespace AccountModelsLib
{
    public class Lookup_CarTypeCLS
    {
        public operations opp;
        public Lookup_CarTypeCLS()
        {
            opp = new operations();
        }
        public Lookup_CarTypeCLS(ref operations opp_)
        {
            opp = opp_;
        }



        public string Get_CurrencyName(ref operations opp, string Code)
        {
            //opp = new operations();
            string mysql = "Select * from Lookup_CarType    Wher  Code= '" + Code + "'";
            string Name = "";
            DataTable dt = opp.return_DataTableWithBuilder(mysql);
            if (dt.Rows.Count > 0)
            {
                Name = dt.Rows[0]["Name"].ToString();
            }
            return Name;
        }
        public DataTable Gey_CarTypeList(ref operations opp)
        {
            opp = new operations();
            string mysql = "Select * from Lookup_CarType Order By Name ";

            DataTable dt = opp.return_DataTableWithBuilder(mysql);
            return dt;
        }
        public long Add_CarType(ref int Excp, string Name, string NameEN, string Code)
        {
            string mysql = " ";
            //  mysql = " Select * from Lookup_CarType Where Code='" + Code + "'";
            //operations opp = new operations();
            //DataTable dt = opp.return_DataTable(mysql);
            //if (dt.Rows.Count > 0)
            //{
            
            //    Excp = 1;
            //    return;
            //}

            mysql = @"INSERT INTO Lookup_CarType (Name, NameEN, Code) ";
            mysql = mysql + " Values ('" + Name + "','" + NameEN + "','" + Code + "')";

            long Id = opp.Insert_me_Return_Last_Identify(mysql);
            Excp = 0;
            return Id;


        }
        public DataTable GetCarTypeList()
        {
            string mysql = " Select * from Lookup_CarType Order By Code";
            operations opp = new operations();
            DataTable dt = opp.return_DataTable(mysql);
            return dt;

        }
        public List<Lookup_CarType> Get_CarTypeList()
        {
            List<Lookup_CarType> lookup_CarTypes = new List<Lookup_CarType>();
            lookup_CarTypes = opp.db.Lookup_CarType.ToList();
            return lookup_CarTypes;
        }
        public void Delete_CarType(ref int Excp, long Id)
        {
            //if (Code.Trim() == "1")
            //{
          
            //    Excp = 1;
            //    return;
            //}
            operations opp = new operations();
            string mysql = " ";
            try
            {
                mysql = @" Delete From Lookup_CarType Where Id=" + Id + "";

                opp.insert_me(mysql);
                Excp = 0;
            }
            catch (Exception exceptionObj)
            {
                //MessageBox.Show(exceptionObj.Message.ToString());
            }

        }
    }
}
