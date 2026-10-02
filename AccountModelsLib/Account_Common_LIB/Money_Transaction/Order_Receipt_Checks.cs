
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using Microsoft.Data.SqlClient;
using System.Data.OleDb;
  using AccountModelsLib; using AccountModelsLib.Models;
using AccountModelsLib.Models;
 
namespace AccountModelsLib
{
    /// <summary>
    /// Summary description for Order_Receipt_Checks.
    /// </summary>
    public class Order_Receipt_Checks
    {
        public Order_Receipt_Checks()
        {
            //
            // TODO: Add constructor logic here
            //
        }
        public int Bank_Id_Under_Ta7seel = -1;
        
        
        
        public void Get_Customer_Supplier_SubAccount_No(ref int Excp, int CustomerOrSupplierOr_Other, long CustomerOrSupplier_id, ref long SubAccount_Id, ref string SubAccount_No, ref string SubAccount_Name, ref int Account_Type_Value)
        {

            string mysql = " ";
            if (CustomerOrSupplierOr_Other == 1)
            {
                mysql = " SELECT Customer.SubAccount_Id, Customer.Id, Customer.Name, Submain_Account.Main_Account_Id, Submain_Account.SubAccount_NO,  Submain_Account.SubAccount_Name, Submain_Account.Account_Customer_Type_Value ";
                mysql = mysql + " FROM Submain_Account INNER JOIN  Customer ON Submain_Account.Id = Customer.SubAccount_Id ";
                mysql = mysql + " WHERE (Customer.Id = " + CustomerOrSupplier_id + ")";
                Account_Type_Value = Common.Customer_Account_Type;
                DataSet ds = null;
                try
                {
                    operations opp = new operations();
                    ds = opp.return_dataset(ref Excp, mysql, "Submain_Account");
                    opp = null;
                    Excp = 0;
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        SubAccount_Id = long.Parse(ds.Tables[0].Rows[0]["SubAccount_Id"].ToString());
                        SubAccount_No = ds.Tables[0].Rows[0]["SubAccount_NO"].ToString();
                        SubAccount_Name = ds.Tables[0].Rows[0]["SubAccount_Name"].ToString();
                        // Account_Type_Value = int.Parse(ds.Tables[0].Rows[0]["Account_Customer_Type_Value"].ToString());

                    }

                }
                catch (Exception e)
                {
                    Excp = 1;
                    throw (e);

                }

            }
            else if (CustomerOrSupplierOr_Other == 2)
            {

                mysql = " SELECT Submain_Account.Main_Account_Id, Submain_Account.SubAccount_NO, Submain_Account.SubAccount_Name, Supplier.Id, Supplier.Name,   Supplier.SubAccount_Id , Submain_Account.Account_Customer_Type_Value";
                mysql = mysql + " FROM Submain_Account INNER JOIN  Supplier ON Submain_Account.Id = Supplier.SubAccount_Id ";
                mysql = mysql + " WHERE (Supplier.Id = " + CustomerOrSupplier_id + ") ";
                Account_Type_Value = Common.Supplier_Account_Type;

                DataSet ds = null;
                try
                {
                    operations opp = new operations();
                    ds = opp.return_dataset(ref Excp, mysql, "Submain_Account");
                    opp = null;
                    Excp = 0;
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        SubAccount_Id = long.Parse(ds.Tables[0].Rows[0]["SubAccount_Id"].ToString());
                        SubAccount_No = ds.Tables[0].Rows[0]["SubAccount_NO"].ToString();
                        SubAccount_Name = ds.Tables[0].Rows[0]["SubAccount_Name"].ToString();
                        // Account_Type_Value = int.Parse(ds.Tables[0].Rows[0]["Account_Customer_Type_Value"].ToString());

                    }

                }
                catch (Exception e)
                {
                    Excp = 1;
                    throw (e);

                }

            }
            else
            {
                mysql = " SELECT * FROM Submain_Account ";
                mysql = mysql + " WHERE (SubAccount_NO = " + SubAccount_No + ")";

                DataSet ds = null;
                try
                {
                    operations opp = new operations();
                    ds = opp.return_dataset(ref Excp, mysql, "Submain_Account");
                    opp = null;
                    Excp = 0;
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        SubAccount_Id = long.Parse(ds.Tables[0].Rows[0]["SubAccount_Id"].ToString());
                        SubAccount_No = ds.Tables[0].Rows[0]["SubAccount_NO"].ToString();
                        SubAccount_Name = ds.Tables[0].Rows[0]["SubAccount_Name"].ToString();
                        Account_Type_Value = int.Parse(ds.Tables[0].Rows[0]["Account_Customer_Type_Value"].ToString());

                    }

                }
                catch (Exception e)
                {
                    Excp = 1;
                    throw (e);

                }
            }



        }

        /// <summary>
        /// get the check in the order
        /// </summary>
        /// <param name="Excp"></param>
        /// <param name="Order_Id"></param>
        /// <returns></returns>
        
        public DataSet Get_Check_Money(long Checks_Memory_Id)
        {
            DataSet ds = null;
            try
            {
                string mysql = " SELECT Submain_Account.SubAccount_Name, Customer.Name AS Customer_Name, Supplier.Name AS Supplier_Name, Checks_Memory.* ,Order_Money.Order_No ,  Order_Money.Total_Value, Order_Money.Currency_Id, Order_Money.Bank_Type, Order_Money.Man_Name,Order_Money.Order_Date";
                mysql = mysql + " FROM Checks_Memory INNER JOIN  Submain_Account ON Checks_Memory.Account_Id = Submain_Account.Id INNER JOIN  Order_Money ON Checks_Memory.Order_Mony_Id = Order_Money.Id LEFT OUTER JOIN  Supplier ON Checks_Memory.Supplier_Id = Supplier.Id LEFT OUTER JOIN  Customer ON Checks_Memory.Customer_Id = Customer.Id ";
                mysql = mysql + "  WHERE (Checks_Memory.ID=" + Checks_Memory_Id + ") ";
                operations opp = new operations();
                ds = opp.return_dataset(mysql, "Checks_Memory");
                opp = null;

                return ds;
            }
            catch (Exception e)
            {

                throw (e);

            }

        }
        
    }

}

