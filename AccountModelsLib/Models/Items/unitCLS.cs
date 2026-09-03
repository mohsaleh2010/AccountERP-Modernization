 
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
 

namespace AccountModelsLib.Models
{
    public class unitCLS
    {
        operations opp;
        public unit un = new unit();
        public unitCLS()
        {
            opp = new operations();
        }
        public unitCLS(ref operations opp_)
        {
            opp = opp_;
        }
        public void GetUnit(long Id)
        {
            un = new unit();
            un = opp.db.unit.Where(p => p.id == Id).FirstOrDefault();
        }
        public unit GetUnitByIdAsNoTracking(long Id)
        {
            un = new unit();
            un = opp.db.unit.AsNoTracking().Where(p => p.id == Id).FirstOrDefault();

            return un;
        }
        public unit GetUnitIS_Default()
        {
            un = new unit();
            un = opp.db.unit.Where(p => p.IS_Default == 1).FirstOrDefault();
            if (un == null)
            {
                unit un=SetUnitDefaultData();
                opp.db.unit.Add(un);
                opp.db.SaveChanges();
                   
            }
            return un;
        }
        unit SetUnitDefaultData()
        {
            unit un = new unit();
            un.Branch_Id = Common.Branch_Id;
            un.User_Id = Common.User_Id;
            un.B_Unit = "قطعة";
            un.B_Unit_Value = 1;
            un.S_unit = "جزء";
            un.S_unit_Value = 1;
            //un.Co = "SET";
            un.u_name = "العدد";
            un.IS_Default = 1;

            return un;
        }
        public void Edit_Unit(ref int Excp, ref string Msg)
        {

            string mysql = "";

            opp.BeginTransaction(2);



            try
            {
                if (un.IS_Default == 1)
                {
                    mysql = "update unit set IS_Default=0";
                    opp.insert_me(mysql);
                }
                opp.db.Entry(un).State = EntityState.Modified;


                opp.db.SaveChanges();


                opp.Commit();
                Excp = 0;

            }
            catch (Exception ee)
            {
                Msg = ee.Message;
                Excp = 1;
                opp.Rollback();
            }




        }

        public void Add_Unit(ref int Excp, ref string Msg)
        {

            string mysql = "";


            opp.BeginTransaction(2);

            try
            {
                if (un.IS_Default == 1)
                {
                    mysql = "update unit set IS_Default=0";
                    opp.insert_me(mysql);
                }
                opp.db.Entry(un).State = EntityState.Added;


                opp.db.SaveChanges();


                opp.Commit();
                Excp = 0;

            }
            catch (Exception ee)
            {
                Msg = ee.Message;
                Excp = 1;
                opp.Rollback();
            }



        }
    }
}
