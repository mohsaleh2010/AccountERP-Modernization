using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
   
 //using AccountModelsLib.DBModels;
namespace AccountModelsLib.Models
{
    public class TagerKontraCLS
    {
        public TagerKontraCLS()
        {

        }
        public LookUp_TagerKontra ModelTagerKontra = new LookUp_TagerKontra();
        public DataTable Get_TagerKontra()
        {
            operations opp = new operations();
            return Get_TagerKontra(ref opp);
        }
        public DataTable Get_TagerKontra(ref operations opp, long Id = 0)
        {
            string mysql = " Select * from LookUp_TagerKontra  ";
            if (Id > 0)
            {
                mysql = mysql + " Where Id=" + Id;
            }
            DataTable dt = opp.return_DataTableWithBuilder(mysql);

            return dt;
        }

        public void GetTagerKontraById(ref operations opp, long Id)
        {
            //if (Id > 0)
            //{
            //    mysql = mysql + " Where Id=" + Id;
            //}

            DataTable dt = Get_TagerKontra(ref opp, Id);
            ReadData(dt, 0);


        }
        public void ReadData(DataTable dt, int index)
        {
            ModelTagerKontra.Id = long.Parse(dt.Rows[index]["Id"].ToString());

            ModelTagerKontra.Name = dt.Rows[index]["Name"].ToString();

        }
        public DataTable Get_TagerKontraByName(ref operations opp)
        {
            string mysql = " Select * from LookUp_TagerKontra Where  Name='" + ModelTagerKontra.Name + "'  ";
            DataTable dt = opp.return_DataTable(mysql);

            return dt;
        }
        public void InsertUnitType(ref int Excp, ref string Msg)
        {
            try
            {
                operations opp = new operations();
                InsertUnitType(ref Excp, ref opp);
                Excp = 0;
            }
            catch (Exception e)
            {

                Excp = 1;
                Msg = e.Message;
            }
        }
        public void InsertUnitType(ref int Excp, ref operations opp)
        {

            string mysql = @"INSERT INTO LookUp_TagerKontra (Name ) ";
            mysql = mysql + " VALUES ('" + ModelTagerKontra.Name + "'  )";

            opp.insert_me(mysql);
            
        }
        public void UpdateUnitType(ref operations opp)
        {

            string mysql = @"Update LookUp_TagerKontra  Set Name ='" + ModelTagerKontra.Name + "'  ";
            mysql = mysql + " Where Id=" + ModelTagerKontra.Id;

            opp.insert_me(mysql);

        }
        public void DeleteTagerKontra(ref int Excp, ref string  Msg, string Id)
        {
            operations opp = new operations();
            string mysql = @" Delete From LookUp_TagerKontra Where ID=" + Id + "";

            opp.insert_me(mysql);
            Excp = 0;

        }
    }
}
