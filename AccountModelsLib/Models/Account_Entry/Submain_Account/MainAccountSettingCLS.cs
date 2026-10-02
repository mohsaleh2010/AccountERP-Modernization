using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using Microsoft.Data.SqlClient;
  using AccountModelsLib;
using AccountModelsLib.Models;
using System.Linq;
 
namespace ConstructionLib
{
    public class MainAccountSettingCLS
    {
        public MainAccountSettingCLS()
        {
        }



        public DataTable Get_Account_Type_Main_Account(ref operations opp)
        {
            string mysql = @" SELECT        Submain_Account.SubAccount_Name, Account_Type_Main_Account_Id.*
FROM            Account_Type_Main_Account_Id INNER JOIN
                         Submain_Account ON Account_Type_Main_Account_Id.Main_Account_Id = Submain_Account.Id ";
            mysql = mysql + @"  ";

            DataTable dt = opp.return_DataTableWithBuilder(mysql);

            return dt;

        }
        public DataTable Get_Account_Type_Customer_Account(ref operations opp)
        {
            CustomerCLS cs = new CustomerCLS();

            string mysql = "";
              mysql = @" SELECT        Customer.Id, Customer.Name, Customer.SubAccount_Id, Submain_Account.SubAccount_Name, Submain_Account.Account_Customer_Type_Value, Submain_Account.Id AS Submain_Account_Id, 
                         Submain_Account.Account_Customer_Type_ID
FROM            Submain_Account FULL OUTER JOIN
                         Customer ON Submain_Account.Id = Customer.SubAccount_Id AND Submain_Account.Account_Customer_Type_ID = Customer.Id
WHERE        (Submain_Account.Account_Customer_Type_Value = 2) ";

 

            mysql = mysql + @"  ";

            DataTable dt = opp.return_DataTableWithBuilder(mysql);

            return dt;

        }
        public DataTable Get_Account_Type_Customer_Account(ref operations opp, string NameSearch, int Active, int General_Customer_Flage)
        {
            //CustomerCLS cs = new CustomerCLS();

            string mysql = "";
            mysql = @"   SELECT DISTINCT 
                         Submain_Account.Id AS Submain_Account_Id, Submain_Account.SubAccount_Name, Submain_Account.Account_Customer_Type_ID, Submain_Account.Account_Customer_Type_Value, Customer.Id, Customer.Name, 
                         Customer.SubAccount_Id AS CustomerSubAccount_Id, Submain_Account.SubAccount_NO, Customer.Active
FROM            Submain_Account INNER JOIN
                         Submain_Account AS Main_Account ON Submain_Account.Main_Account_Id = Main_Account.Id LEFT OUTER JOIN
                         Customer ON Submain_Account.Id = Customer.SubAccount_Id
WHERE        (Submain_Account.Account_Customer_Type_Value = 2) AND  (Submain_Account.Id NOT IN
                             (SELECT DISTINCT Main_Account_Id
                                FROM            Submain_Account AS Submain_Account_1)) ";



            

            if (NameSearch.Trim() != "")
            {
                mysql = mysql + " AND (Customer.Name like N'%" + NameSearch + "%' OR Submain_Account.SubAccount_Name like N'%" + NameSearch + "%') ";
                
            }
            if (Active > -1)
            {
                mysql = mysql + " AND Customer.Active=" + Active;
            }
            if (General_Customer_Flage > -1)
            {
                mysql = mysql + " AND Customer.General_Customer_Flage=" + General_Customer_Flage;
            }

            mysql = mysql + " ORDER BY Submain_Account.SubAccount_Name ";


            DataTable dt = opp.return_DataTableWithBuilder(mysql);

            Common.Make_TableColumnSerial(ref dt, "Serial");
            return dt;

        }

        public DataTable Get_Account_Type_Supplier_Account(ref operations opp, string NameSearch, int Active )
        {
             
            //CustomerCLS cs = new CustomerCLS();

            string mysql = "";
            mysql = @"   SELECT DISTINCT 
                         Submain_Account.Id AS Submain_Account_Id, Submain_Account.SubAccount_Name, Submain_Account.Account_Customer_Type_ID, Submain_Account.Account_Customer_Type_Value, Supplier.Id, Supplier.Name, 
                         Supplier.SubAccount_Id AS SupplierSubAccount_Id, Submain_Account.SubAccount_NO, Supplier.Active
FROM            Submain_Account INNER JOIN
                         Submain_Account AS Main_Account ON Submain_Account.Main_Account_Id = Main_Account.Id LEFT OUTER JOIN
                         Supplier ON Submain_Account.Id = Supplier.SubAccount_Id
WHERE        (Submain_Account.Account_Customer_Type_Value = 5) AND  (Submain_Account.Id NOT IN
                             (SELECT DISTINCT Main_Account_Id
                                FROM            Submain_Account AS Submain_Account_1)) ";





            if (NameSearch.Trim() != "")
            {
                mysql = mysql + " AND (Supplier.Name like N'%" + NameSearch + "%' OR Submain_Account.SubAccount_Name like N'%" + NameSearch + "%') ";
                 
            }
            if (Active > -1)
            {
                mysql = mysql + " AND Supplier.Active=" + Active;
            }
            

            mysql = mysql + " ORDER BY Submain_Account.SubAccount_Name ";


            DataTable dt = opp.return_DataTableWithBuilder(mysql);

            Common.Make_TableColumnSerial(ref dt, "Serial");
            return dt;

        }
        public DataTable GetGet_Account_Type_Main_Account(ref operations opp, int Account_Type_Value, long Main_Account_Id, long Account_Branch_Id)
        {
            string mysql = @" SELECT        Submain_Account.SubAccount_Name, Account_Type_Main_Account_Id.*
FROM            Account_Type_Main_Account_Id INNER JOIN
                         Submain_Account ON Account_Type_Main_Account_Id.Main_Account_Id = Submain_Account.Id ";
            mysql = mysql + @" Where Account_Type_Main_Account_Id.Account_Type_Value=" + Account_Type_Value + " And   Account_Type_Main_Account_Id.Main_Account_Id=" + Main_Account_Id + " And  Account_Type_Main_Account_Id.Account_Branch_Id=" + Account_Branch_Id + " ";

            DataTable dt = opp.return_DataTableWithBuilder(mysql);
            return dt;

        }
        public void Add_Account_Type_Main_Account(ref int Excp, ref string Msg, int Account_Type_Value, string Account_Type_Name, long Main_Account_Id,string Account_Name, long Account_Branch_Id)
        {

            operations opp = new operations();

            DataTable dt = GetGet_Account_Type_Main_Account(ref opp, Account_Type_Value, Main_Account_Id, Account_Branch_Id);
            string mysql = "";
            if (dt.Rows.Count == 0)
            {
                  mysql = " INSERT INTO Account_Type_Main_Account_Id(Account_Type_Value,Account_Type_Name, Main_Account_Id,Account_Name, Account_Branch_Id, OperationDate, User_Id, Branch_Id) ";
                mysql = mysql + "VALUES(@Account_Type_Value,@Account_Type_Name, @Main_Account_Id,@Account_Name, @Account_Branch_Id, @OperationDate, @User_Id, @Branch_Id) ";

                opp.cmd.Parameters.AddWithValue("@Account_Type_Value", Account_Type_Value);

                opp.cmd.Parameters.AddWithValue("@Account_Type_Name", Account_Type_Name);
                opp.cmd.Parameters.AddWithValue("@Main_Account_Id", Main_Account_Id);
                
                     opp.cmd.Parameters.AddWithValue("@Account_Name", Account_Name);
                opp.cmd.Parameters.AddWithValue("@Account_Branch_Id", Account_Branch_Id);
                opp.cmd.Parameters.AddWithValue("@OperationDate", DateTime.Now.Date);
                opp.cmd.Parameters.AddWithValue("@User_Id", Common.User_Id);
                opp.cmd.Parameters.AddWithValue("@Branch_Id", Common.Branch_Id);
                opp.insert_me(mysql);
                opp.cmd.Parameters.Clear();
                Excp = 0;
            }
            else
            {
                long Id = long.Parse(dt.Rows[0]["Id"].ToString());

                Edit_Account_Type_Main_Account(ref Excp, ref Msg,ref opp ,Id, Account_Type_Value, Account_Type_Name, Main_Account_Id, Account_Name, Account_Branch_Id);
            }

            mysql = "Update Submain_Account Set Account_Customer_Type_Value ="+ Account_Type_Value + " Where  Id="+ Main_Account_Id;
            opp.insert_me(mysql);
        }
        public void UpdateCustomerAccount_Type_(ref int Excp, ref string Msg,long Id, long SubAccount_Id,bool IsNotRelated)
        {

            operations opp = new operations();
            string mysql = "";
            if (IsNotRelated == true) //الغاء الربط
            {
                  mysql = "Update Submain_Account Set Account_Customer_Type_ID =0 Where  Id=" + SubAccount_Id;
                 

                mysql = mysql + " Update Customer Set SubAccount_Id =0 Where Id=" + Id;
            }
            if (IsNotRelated == false )
            {
                mysql = "Update Submain_Account Set Account_Customer_Type_ID =" + Id + " Where  Id=" + SubAccount_Id;
                //opp.insert_me(mysql);

                mysql = mysql + " Update Customer Set SubAccount_Id =" + SubAccount_Id + " Where Id=" + Id;
            }
            opp.insert_me(mysql);
        }
        public void UpdateSupplierAccount_Type_(ref int Excp, ref string Msg, long Id, long SubAccount_Id, bool IsNotRelated)
        {

            operations opp = new operations();
            string mysql = "";
            if (IsNotRelated == true) //الغاء الربط
            {
                mysql = "Update Submain_Account Set Account_Customer_Type_ID =0 Where  Id=" + SubAccount_Id;


                mysql = mysql + " Update Supplier Set SubAccount_Id =0 Where Id=" + Id;
            }
            if (IsNotRelated == false)
            {
                mysql = "Update Submain_Account Set Account_Customer_Type_ID =" + Id + " Where  Id=" + SubAccount_Id;
                //opp.insert_me(mysql);

                mysql = mysql + " Update Supplier Set SubAccount_Id =" + SubAccount_Id + " Where Id=" + Id;
            }
            opp.insert_me(mysql);
        }

        public void Edit_Account_Type_Main_Account(ref int Excp,  ref string Msg, ref operations opp, long Id, int Account_Type_Value,string Account_Type_Name, long Main_Account_Id,string Account_Name, long Account_Branch_Id)
        {
            string mysql = "";
           
            mysql = @"Update Account_Type_Main_Account_Id Set Account_Type_Value = " + Account_Type_Value + " ,Account_Type_Name='"+ Account_Type_Name + "',Main_Account_Id = " + Main_Account_Id + " ,Account_Name='"+ Account_Name + "',Account_Branch_Id = " + Account_Branch_Id + "  ";
            mysql = mysql + "  Where Id= " + Id;

            opp.insert_me(mysql);
            Excp = 0;

        }

        public void Delete_Account_Type_Main_Account(ref int Excp, ref string Msg, long Id)
        {

            string mysql = "";
            operations opp = new operations();
            mysql = @"Delete From  Account_Type_Main_Account_Id   ";
            mysql = mysql + "  Where Id= " + Id;

            opp.insert_me(mysql);
            Excp = 0;

        }
    }
}
