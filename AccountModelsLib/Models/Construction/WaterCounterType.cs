using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
namespace AccountModelsLib.Models
{
    public class WaterCounterType
    {
        
        public long Id { get; set; } = 0;
        public string Name { get; set; }
       
        public DataTable Get_WaterCounterType()
        {
            operations opp = new operations();
            return Get_WaterCounterType(ref opp);
        }
        public DataTable GetDataFill_List()
        {
            operations opp = new operations();
            return Get_WaterCounterType(ref opp);
        }
        
        public DataTable Get_WaterCounterType(ref operations opp, long Id =0)
        {
            string mysql = " Select * from ConsWaterCounterType  ";
            if (Id > 0)
            {
                mysql = mysql + " Where Id=" + Id;
            }
            DataTable dt = opp.return_DataTable(mysql);

            return dt;
        }
        public void GetWaterCounterTypeById(ref operations opp, long Id  )
        {

            DataTable dt = Get_WaterCounterType(ref opp, Id);

            ReadData(dt, 0);
        }

        public void ReadData(DataTable dt, int index)
        {
            Id = long.Parse(dt.Rows[index]["Id"].ToString());

            Name = dt.Rows[index]["Name"].ToString();

        }
        public DataTable Get_WaterCounterTypeByName(ref operations opp)
        {
            string mysql = " Select * from ConsWaterCounterType Where  Name='" + Name + "'  ";
            DataTable dt = opp.return_DataTable(mysql);

            return dt;
        }
        public void InsertWaterCounterType(ref operations opp)
        {

            string mysql = @"INSERT INTO ConsWaterCounterType (Name ) ";
            mysql = mysql + " VALUES ('" + Name + "'  )";

            opp.insert_me(mysql);

        }
    }
}
