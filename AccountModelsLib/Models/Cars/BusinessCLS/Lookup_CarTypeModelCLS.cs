using AccountModelsLib.Models;
 
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
 
namespace AccountModelsLib
{
    public class Lookup_CarTypeModelCLS
    {
        public operations opp;
        public Lookup_CarTypeModelCLS()
        {
            opp = new operations();
        }
        public Lookup_CarTypeModelCLS(ref operations opp_)
        {
            opp = opp_;
        }



        public string Get_CarTypeModel(ref operations opp, string Code)
        {
            //opp = new operations();
            string mysql = "Select * from Lookup_CarTypeModel    Wher  Code= '" + Code + "'";
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
            string mysql = "Select * from Lookup_CarTypeModel Order By Name ";

            DataTable dt = opp.return_DataTableWithBuilder(mysql);
            return dt;
        }
        public long Add_CarType(ref int Excp, string Name, string CarType, long Lookup_CarTypeId)
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
 
            mysql = @"INSERT INTO Lookup_CarTypeModel (Name, CarType, Lookup_CarTypeId) ";
            mysql = mysql + " Values ('" + Name + "','" + CarType + "', " + Lookup_CarTypeId + " )";

            long Id = opp.Insert_me_Return_Last_Identify(mysql);
            Excp = 0;
            return Id;


        }
        public DataTable GetCarTypeList()
        {
            string mysql = " Select * from Lookup_CarTypeModel Order By Lookup_CarTypeId";
            operations opp = new operations();
            DataTable dt = opp.return_DataTable(mysql);
            return dt;

        }
        public List<Lookup_CarTypeModel> Get_Lookup_CarTypeModelList()
        {
            List<Lookup_CarTypeModel> lookup_CarTypes = new List<Lookup_CarTypeModel>();
            lookup_CarTypes = opp.db.Lookup_CarTypeModel.ToList();
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
                mysql = @" Delete From Lookup_CarTypeModel Where Id=" + Id + "";

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
