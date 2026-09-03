using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{
   public  class Customers_AccountsCLS
    {
        public Customers_AccountsCLS()
        {

        }
        public Customers_Accounts ca = new Customers_Accounts();
        public void GetCustomers_Accounts(long Customer_Id)
        {
            try
            {
                ca = new  Customers_Accounts ();
                

                operations opp = new operations();
                My_DbContext.ConnStr = opp.str_sql_conn;
                My_DbContext db = new My_DbContext();

                // um = db.Unit_item.Find(Unit_item_Table_Id);
                ca = db.Customers_Accounts.Where(s => s.Customer_Id == Customer_Id).FirstOrDefault();
                 

            }
            catch (Exception ee)
            {


            }



        }

        public void SetCustomerFirstBalance(ref int Excp, ref operations opp, bool IsFirst, long Customer_Id, decimal Balance, int Transform_Type, DateTime datBalance, string Notes)
        {


            long Customers_Accounts_Id = 0;
            DateTime datNow = DateTime.Now.Date;
            CustomersAccountCLS cc = new CustomersAccountCLS();
            DataTable dt = cc.GetCustomerFirstBalance(Customer_Id);
            if (dt.Rows.Count > 0)
            {
                Customers_Accounts_Id = long.Parse(dt.Rows[0]["Id"].ToString());
                datNow = DateTime.Parse(dt.Rows[0]["Transaction_Date"].ToString());
            }

            //long  Year_Financial_Period_Id = Financial_PeriodCLS.Get_Year_Financial_Period_Id(datBalance);
            Excp = 0;
            try
            {

                decimal BalanceSaved = Balance;

                string mysql = "";
                decimal Import = 0;
                decimal Export = 0;
                if (Transform_Type == 2)
                {
                    Balance = Balance * -1;
                    Import = BalanceSaved;
                    Export = 0;
                }
                else
                {
                    Import = 0;
                    Export = BalanceSaved;
                }



                if (IsFirst == false)
                {
                    Excp = 10;
                    return;
                }
                if (Customers_Accounts_Id > 0)
                {
                    mysql = " Update Customers_Accounts Set Transaction_Date='" + operations.dateFormat(datBalance) + "' , Notes=N'" + Notes + "',Balance = " + Balance + " ,Export=" + Export + ",Import=" + Import + " Where Id=" + Customers_Accounts_Id;
                    opp.insert_me(mysql);
                }
                else
                {
                    mysql = " INSERT INTO Customers_Accounts (Customer_Id, Reference_Id,Reference_Type_Table_Id, Export,Import, Balance, Transaction_Type, Transaction_Date,Reference_Type,Notes) ";
                    mysql += " VALUES(" + Customer_Id + ",0," + Customer_Id + "," + Export + "," + Import + "," + Balance + ",2,'" + operations.dateFormat(datBalance) + "',-7,N'" + Notes + "')";
                    opp.insert_me(mysql);



                }
                Excp = 0;
            }
            catch (Exception ee)
            {
                Excp = 1;
            }
        }
    }
}
