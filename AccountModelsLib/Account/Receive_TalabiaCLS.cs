using System;
using System.Collections.Generic;
using System.Text;
 
 using AccountModelsLib; using AccountModelsLib.Models;
using System.Data;
namespace AccountModelsLib
{
    public class Receive_TalabiaCLS
    {
        public void save(ref int Excp, ref string Msg, int Cust_Id,string Desc,string Dt_Date)
        {
            string Mysql = "INSERT INTO Customer_Receive (customer_id, descreption, dt_date) VALUES('" + Cust_Id + "','" + Desc + "','" + Dt_Date + "')";
            operations opp = new operations();
            opp.insert_me(Mysql);
            Excp = 0; 
            Msg=             "ÊãÊ ÚãáíÉ ÇáÍÝÙ ÈäÌÇÍ " ;
        }
        public void delete(ref int Excp, ref string Msg, int ID)
        {
            string MySql = "delete from Customer_Receive where id=" + ID;
            operations opp = new operations();
            opp.delete_me(MySql);
            Excp = 0;
                Msg="ÊãÊ ÚãáíÉ ÇáÍÐÝ ÈäÌÇÍ " ;

        }

        public DataSet  Get_Talabia(int Customer_Id, DateTime dtFrom, DateTime dtTo)
        {

            //string MySql = "SELECT * FROM Customer_Receive where dt_date >='" + operations.dateFormat(dtFrom) + "'and dt_date <= '" + operations.dateFormat(dtTo) + "'";
            string MySql = " SELECT Customer.Name, Customer_Receive.descreption, Customer_Receive.dt_date, Customer_Receive.id, Customer_Receive.customer_id FROM Customer_Receive LEFT OUTER JOIN Customer ON Customer_Receive.customer_id = Customer.Id where dt_date >='" + operations.dateFormat(dtFrom) + "'and dt_date <= '" + operations.dateFormat(dtTo) + "' and customer_id=" + Customer_Id;
            DataSet cust_dss = new DataSet();
            operations opp = new operations();

            cust_dss = opp.return_dataset(MySql);
            return cust_dss;
        }
    }
}
