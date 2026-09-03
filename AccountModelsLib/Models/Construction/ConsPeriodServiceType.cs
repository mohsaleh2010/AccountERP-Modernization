using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
namespace AccountModelsLib.Models
{
    public class ConsPeriodServiceType
    {
         
        public long Id { get; set; } = 0;
        public string Name { get; set; }
       
        public DataTable GetServices()
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            return GetPeriodServiceType(ref opp);
        }
        public DataTable GetPeriodServiceType(ref AccountModelsLib.operations opp)
        {

            string mysql = "Select * from PeriodServiceType Order By  Name ";

            DataTable dt = opp.return_DataTableWithBuilder(mysql);
            return dt;
        }
        public DataTable GetPeriodServiceTypeName(ref AccountModelsLib.operations opp)
        {
            opp = new AccountModelsLib.operations();
            string mysql = " Select * from PeriodServiceType Where Name='" + Name + "'     ";

            DataTable dt = opp.return_DataTable(mysql);
            return dt;
        }
        
        public void InsertPeriodServiceType(ref AccountModelsLib.operations opp)
        {
            string mysql = @"INSERT INTO PeriodServiceType (Name, ) ";
            mysql = mysql + " VALUES ('" + Name + "' )";

            opp.insert_me(mysql);


        }
        public void Edit_PeriodServiceType(ref int Excp, ref string Msg, Service ss)
        {
            string mysql = " Select * from PeriodServiceType Where Name='" + ss.Name + "' And  Id<> " + ss.Id;
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            DataTable dt = opp.return_DataTable(mysql);
            if (dt.Rows.Count > 0)
            {
                Msg = "الإسم مستخدم من قبل";
                Excp = 1;
                return;
            }

            mysql = @"Update PeriodServiceType  Set Name ='" + ss.Name + "' ";
            mysql = mysql + " Id =" + ss.Id;

            opp.insert_me(mysql);


        }
        public void DeletePeriodServiceType(ref AccountModelsLib.operations opp)
        {
            string mysql = @" Delete From PeriodServiceType Where ID=" + Id + "";

            opp.insert_me(mysql);
        }
    }



}
