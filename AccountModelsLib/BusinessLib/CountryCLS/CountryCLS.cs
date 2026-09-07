using System;
using System.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using Microsoft.Data.SqlClient;

using Microsoft.EntityFrameworkCore;
 

using System.Linq;

using AccountModelsLib.Models;
namespace AccountModelsLib
{
    public class CountryCLS
    {
        public CountryCLS()
        {
        }
        private DataTable dataTable = null;
        AccountModelsLib.operations opp = new AccountModelsLib.operations();
        public DataTable GetData()
        {
            opp = new AccountModelsLib.operations( );
            string mysql = "Select * from Gov_Country_List";
            dataTable = opp.return_DataTableWithBuilder(mysql);

            if(dataTable.Rows.Count==0)
            {
                AddFirstCountry();
                opp = new AccountModelsLib.operations();
                dataTable = opp.return_DataTableWithBuilder(mysql);

            }
             
            return dataTable;

        }
        void AddFirstCountry()
        {
            opp = new AccountModelsLib.operations(2);
            string mysql = "Truncate Table Gov_Country_List";
            opp.insert_me(mysql);

            Gov_Country_List gc = new Gov_Country_List();
            gc.NameAr = "مصر";
            gc.NameEn = "Egypt";

            opp.db.Gov_Country_List.Add(gc);
            opp.db.SaveChanges();
            opp.Commit();
        }
        public Gov_Country_List GetCountry(ref AccountModelsLib.operations opp, long? Id)
        {
            Gov_Country_List gc = opp.db.Gov_Country_List.Where(i => i.Id == Id).FirstOrDefault();

            return gc;
        }

        public void UpdateDataTable()
        {
            opp.ada.Update(dataTable);


        }
        public void Delete (ref int Excp, ref string Msg,  long Id)
        {
            try
            {
                opp = new AccountModelsLib.operations();
                var garea = opp.db.Govern_Area_List.Where(i => i.Gov_Country_ListId == Id);
                if (garea != null)
                {
                    Excp = 1;
                    Msg = "لا يمكن حذف الدولة تم تسجيل مناطق بها";
                    return;
                }


                var gl = opp.db.Govern_List.Where(i => i.Govern_Area_ListId == Id);
                if (gl != null)
                {
                    Excp = 1;
                    Msg = "لا يمكن حذف الدولة تم تسجيل محافظات بها";
                    return;
                }

                var cityL = opp.db.Gov_City_List.Where(i => i.Govern_Area_ListId == Id);
                if (cityL != null)
                {
                    Excp = 1;
                    Msg = "لا يمكن حذف الدولة تم تسجيل مدن بها";
                    return;
                }

                Govern_Area_List ga = opp.db.Govern_Area_List.Where(i => i.Id == Id).SingleOrDefault();
                if (ga != null)
                {
                    opp.db.Govern_Area_List.Remove(ga);
                    opp.db.SaveChanges();
                    Excp = 0;
                }
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
            }


        }
    }
}
