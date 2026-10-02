using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
namespace AccountModelsLib.Models
{
    public class UnitType
    {
        
        public long Id { get; set; } = 0;
        public string Name { get; set; }
    
        public DataTable Get_UnitType()
        {
            operations opp = new operations();
            return Get_UnitType(ref opp);
        }
        public DataTable Get_UnitType(ref operations opp, long Id = 0)
        {
            string mysql = " Select * from ConsUnitType  ";
            if (Id > 0)
            {
                mysql = mysql + " Where Id=" + Id;
            }
            DataTable dt = opp.return_DataTable(mysql);

            return dt;
        }
       
        public void GetUnitTypeById(ref operations opp, long Id)
        {
            //if (Id > 0)
            //{
            //    mysql = mysql + " Where Id=" + Id;
            //}

            DataTable dt = Get_UnitType(ref opp, Id);
            ReadData(dt, 0);


        }
        public void ReadData(DataTable dt, int index)
        {
            Id = long.Parse(dt.Rows[index]["Id"].ToString());
           
            Name = dt.Rows[index]["Name"].ToString();

        }
        public DataTable Get_UnitTypeByName(ref operations opp)
        {
            string mysql = " Select * from ConsUnitType Where  Name='" + Name + "'  ";
            DataTable dt = opp.return_DataTable(mysql);

            return dt;
        }
        public void InsertUnitType(ref operations opp)
        {

            string mysql = @"INSERT INTO ConsUnitType (Name ) ";
            mysql = mysql + " VALUES ('" + Name + "'  )";

            opp.insert_me(mysql);

        }
        public void UpdateUnitType(ref operations opp)
        {

            string mysql = @"Update ConsUnitType  Set Name ='" + Name + "'  ";
            mysql = mysql + " Where Id="+Id;

            opp.insert_me(mysql);

        }
        public void DeleteRealStateType(ref operations opp, string Id)
        { 
                string mysql = @" Delete From ConsUnitType Where ID=" + Id + "";

                opp.insert_me(mysql);
               

        }
    }
}
