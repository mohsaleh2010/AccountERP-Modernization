using System;
using System.Collections.Generic;
using System.Text;
  using AccountModelsLib; using AccountModelsLib.Models;
using System.Data;
using System.Collections;
 

namespace AccountModelsLib
{
    public class Money_SpentCustomerSuppliersCLS
    {
        string Table_Details = "";
        string Table_List = "";
        int FrmType = 2;
        public ArrayList arr = new ArrayList();

        public Money_SpentCustomerSuppliersCLS(int FrmType_)
        {

        }

        public string MakeINClause(ArrayList arr)
        {
            string D = "";
            for (int x = 0; x < arr.Count; x++)
            {
                D = D + arr[x].ToString() + ",";
            }
            return D;
        }
        public DataSet GetMoney_SpentDetails(ref int Excp, bool AllCustomer, long Customer_ID, bool AllDates, DateTime DateFrom, DateTime DateTo, bool Totals)
        {
            string mysql = "";

            mysql = " SELECT   Money_Transaction_Box.Import_Value,  Money_Transaction_Box.Export_Value, Money_Transaction_Box.Account_Id, Order_Money.Order_Date, Order_Money.Transaction_type, Customer.Name, Order_Money.Order_No, Order_Money.Id ";

            mysql = mysql + " FROM Money_Transaction_Box INNER JOIN  Order_Money ON Money_Transaction_Box.Order_Mony_Id = Order_Money.Id INNER JOIN  Customer ON Money_Transaction_Box.Customer_Id = Customer.Id ";
            mysql = mysql + "  WHERE (Order_Money.Transaction_type = 1)  ";
            if (AllDates == false)
            {
                mysql = mysql + " AND  (Order_Money.Order_Date >= '" + operations.dateFormat(DateFrom) + "') AND  (Order_Money.Order_Date <= '" + operations.dateFormat(DateTo) + "') ";
            }
            if (AllCustomer == false)
            {
                mysql = mysql + " AND  (Money_Transaction_Box.Customer_Id=" + Customer_ID + ")";
            }
            mysql = mysql + " ORDER BY Order_Money.Order_Date ";

            return Get_Data(ref Excp, mysql);

        }
        public DataSet GetCheck_SpentDetails(ref int Excp, bool AllCustomer, long Customer_ID, bool AllDates, DateTime DateFrom, DateTime DateTo, bool Totals)
        {
            string mysql = "";               

            mysql = " SELECT Checks_Memory.Export_Value, Checks_Memory.Account_Id, Order_Money.Order_Date, Order_Money.Transaction_type, Checks_Memory.Day_Date, Customer.Name, Order_Money.Order_No, Order_Money.Id, Checks_Memory.Import_Value ";
            mysql = mysql + " FROM Order_Money INNER JOIN  Checks_Memory ON Order_Money.Id = Checks_Memory.Order_Mony_Id INNER JOIN  Customer ON Checks_Memory.Customer_Id = Customer.Id ";
            mysql = mysql + "  WHERE (Order_Money.Transaction_type = 1)  ";
            if (AllDates == false)
            {
                mysql = mysql + " AND  (Order_Money.Order_Date >= '" + operations.dateFormat(DateFrom) + "') AND  (Order_Money.Order_Date <= '" + operations.dateFormat(DateTo) + "') ";
            }
            if (AllCustomer == false)
            {
                mysql = mysql + " AND  (Checks_Memory.Customer_Id=" + Customer_ID + ")";
            }
            mysql = mysql + " ORDER BY  Checks_Memory.Customer_Id";
            return Get_Data(ref Excp, mysql);

        }
        public DataSet GetTotalMoney_SpentDetails(ref int Excp, bool AllCustomer, long Customer_ID, bool AllDates, DateTime DateFrom, DateTime DateTo, bool Totals)
        {

            string mysql = "";

            mysql = " SELECT SUM(Money_Transaction_Box.Export_Value) AS Export_Value, Order_Money.Transaction_type, Customer.Name, SUM(Money_Transaction_Box.Import_Value) AS Import_Value ,  '' AS Order_No, '' AS Order_Date";
            mysql = mysql + " FROM Money_Transaction_Box INNER JOIN  Order_Money ON Money_Transaction_Box.Order_Mony_Id = Order_Money.Id INNER JOIN  Customer ON Money_Transaction_Box.Customer_Id = Customer.Id ";
            mysql = mysql + "  WHERE (Order_Money.Transaction_type = 1)  ";
            if (AllDates == false)
            {
                mysql = mysql + " AND  (Order_Money.Order_Date >= '" + operations.dateFormat(DateFrom) + "') AND  (Order_Money.Order_Date <= '" + operations.dateFormat(DateTo) + "') ";
            }
            if (AllCustomer == false)
            {
                mysql = mysql + " AND  (Money_Transaction_Box.Customer_Id=" + Customer_ID + ")";
            }

            mysql = mysql + " GROUP BY Order_Money.Transaction_type, Customer.Name ";

            return Get_Data(ref Excp, mysql);

        }

        public DataSet GetTotalCheck_SpentDetails(ref int Excp, bool AllCustomer, long Customer_ID, bool AllDates, DateTime DateFrom, DateTime DateTo, bool Totals)
        {

            string mysql = "";

            mysql = " SELECT SUM(Checks_Memory.Export_Value) AS Export_Value, Order_Money.Transaction_type, Customer.Name, '' AS Order_No, '' AS Order_Date,  SUM(Checks_Memory.Import_Value) AS Import_Value ";

            mysql = mysql + " FROM Checks_Memory INNER JOIN  Order_Money ON Checks_Memory.Order_Mony_Id = Order_Money.Id INNER JOIN  Customer ON Checks_Memory.Customer_Id = Customer.Id ";
            mysql = mysql + "  WHERE (Order_Money.Transaction_type = 1)  ";
            if (AllDates == false)
            {
                mysql = mysql + " AND  (Order_Money.Order_Date >= '" + operations.dateFormat(DateFrom) + "') AND  (Order_Money.Order_Date <= '" + operations.dateFormat(DateTo) + "') ";
            }


            if (AllCustomer == false)
            {
                mysql = mysql + " AND  (Checks_Memory.Customer_Id=" + Customer_ID + ")";
            }
            mysql = mysql + " GROUP BY Order_Money.Transaction_type, Customer.Name ";
            return Get_Data(ref Excp, mysql);

        }


        private DataSet Get_Data(ref int Excp, string mysql)
        {
            DataSet G_DS = new DataSet();

            try
            {
                operations opp = new operations();
                G_DS = opp.return_dataset(mysql, "Unit_item");
                Excp = 0;
                opp = null;
                return G_DS;
            }
            catch (Exception ee)
            {

                Excp = -1;
             //   MessageBox.Show(ee.Message);
                return G_DS;
            }
        }

    }
}
