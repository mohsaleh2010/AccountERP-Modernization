using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
namespace AccountModelsLib.Models
{
    public class Service
    {
         
        public long Id { get; set; } = 0;
        public long PeriodServiceTypeId { get; set; } = 0;
        public string Name { get; set; }
        public decimal Price { get; set; } = 0;
       
        public DataTable GetServices()
        {
            operations opp = new operations();
            return GetServices(ref opp);
        }
        public DataTable GetServices(ref operations opp)
        {

            string mysql = "Select * from ConsService Order By  Name ";

            DataTable dt = opp.return_DataTableWithBuilder(mysql);
            return dt;
        }
        public DataTable GetServicesByName(ref operations opp)
        {
            opp = new operations();
            string mysql = " Select * from ConsService Where Name='" + Name + "'     ";

            DataTable dt = opp.return_DataTable(mysql);
            return dt;
        }
        public decimal GetPrice(long Id)
        {
            decimal Price = 0;
            operations opp = new operations();
            string mysql = "Select * from ConsService Where Id=" + Id;

            DataTable dt = opp.return_DataTableWithBuilder(mysql);
            if (dt.Rows.Count > 0)
            {
                try {
                    Price = decimal.Parse(dt.Rows[0]["Price"].ToString());
                }
                catch
                {
                    Price = 0;
                }

            }
            return Price;
        }
        public void InsertService(ref operations opp)
        {
            string mysql = @"INSERT INTO ConsService (Name,Price ,PeriodServiceTypeId) ";
            mysql = mysql + " VALUES ('" + Name + "'," + Price + " ,"+ PeriodServiceTypeId + ")";

            opp.insert_me(mysql);


        }
        public void UpdateService(ref int Excp, ref string Msg ,ref operations opp)
        {
            string mysql = " Select * from ConsService Where Name='" +  Name + "' And  Id<> " +  Id;
           
            DataTable dt = opp.return_DataTable(mysql);
            if (dt.Rows.Count > 0)
            {
                Msg = "الإسم مستخدم من قبل";
                Excp = 1;
                return;
            }

            mysql = @"Update ConsService  Set Price= " + Price + " ,Name ='" +  Name + "' ,PeriodServiceTypeId="+ PeriodServiceTypeId + "";
            mysql = mysql + " Id =" +  Id;

            opp.insert_me(mysql);


        }
        public void DeleteService(ref operations opp)
        {
            string mysql = @" Delete From ConsService Where ID=" + Id + "";

            opp.insert_me(mysql);
        }
    }



}
