using System;
using System.Collections.Generic;
using System.Text;

using AccountModelsLib;
using AccountModelsLib.Models;
using System.Data;

namespace AccountModelsLib
{


    public class Machin_list_class
    {
        operations opp;
        DataSet machineds;
        public Machin_list_class()
        {
            opp = new operations();
            machineds = new DataSet();
        }
        DataTable dataTable=new DataTable();
        public DataTable GetData()
        {
            dataTable=new DataTable();
            string mysql = "Select * from Machin_list   Where Active=1";
            dataTable = opp.return_DataTableWithBuilder(mysql);

            return dataTable;

        }
        public void UpdateDataTable()
        {
             foreach(DataRow row in dataTable.Rows)
            {
                 string  Id = row["Id"].ToString(); 
                 string active = row["active"].ToString();
                row["active"] = 1;
            }
            opp.ada.Update(dataTable);

            //string mysql = " Update Machin_list Set Active = @Active   WHERE (active IS NULL) ";
            //opp.cmd.Parameters.AddWithValue("Active", 1);
            //opp.insert_me(mysql);
        }
        public void DeleteMachin_list(long Id)
        {
            string mysql = $" Update Machin_list Set Active = 0   WHERE (Id ={Id}) ";
            opp.insert_me(mysql);

            //string mysql = $" Delete From Machin_list   WHERE (Id ={Id}) ";
            //opp.insert_me(mysql);
        }

        public void add_machin(string MACHINE, decimal price_of_nazlla)
        {
            string sql;
            sql = "INSERT INTO Machin_list(Machin_Name, price_of_nazlla)";
            sql += "VALUES('" + MACHINE + " ' , " + price_of_nazlla + ")";
            opp.insert_me(sql);

        }
        public void add_machin(string MACHINE, decimal price_of_nazlla, string Code, decimal Speed, int Machine_Width, string Description, decimal Abstract_Hadafat)
        {
            string sql;
            sql = "INSERT INTO Machin_list(Machin_Name, price_of_nazlla, Code,  Speed,  Machine_Width,  Description,Abstract_Hadafat)";
            sql += "VALUES('" + MACHINE + " ' , " + price_of_nazlla + ",'" + Code + "'," + Speed + "," + Machine_Width + ",'" + Description + "'," + Abstract_Hadafat + ")";
            opp.insert_me(sql);

        }
        public void update_machine(int id, string MACHINE, decimal price_of_nazlla)
        {
            string mysql = "UPDATE Machin_list SET Machin_Name = '" + MACHINE + "', price_of_nazlla = " + price_of_nazlla + " WHERE(Id = " + id + ")";
            opp.update_me(mysql);
        }

        public void update_machine(int id, string MACHINE, decimal price_of_nazlla, string Code, decimal Speed, int Machine_Width, string Description, decimal Abstract_Hadafat)
        {
            string mysql = "UPDATE Machin_list SET Machin_Name = '" + MACHINE + "', price_of_nazlla = " + price_of_nazlla + " , Code='" + Code + "',   Speed=" + Speed + ", Machine_Width=" + Machine_Width + ", Description='" + Description + "' ,Abstract_Hadafat=" + Abstract_Hadafat + "";
            mysql = mysql + " WHERE(Id = " + id + ")";
            opp.update_me(mysql);
        }


        public DataSet Get_machine_list()
        {
            string mysql = "SELECT Machin_list.* FROM Machin_list";
            machineds = opp.return_dataset(mysql);
            return machineds;
        }
        public void Add_Nazallat_of_Day(DateTime date, decimal number_of_nazlla, int mashineID, decimal nazlla_price, decimal total, long Supplier_ID)
        {
            int Excp = 0;
            string mysql = "INSERT INTO Total_of_Nazlla(date, number_of_nazlla, mashineID, nazlla_price, total,Supplier_ID)";
            mysql += "VALUES('" + operations.dateFormat(date) + "'," + number_of_nazlla + " ," + mashineID + ", " + nazlla_price + ", " + total + "," + Supplier_ID + ")";
            long Process_No = opp.Insert_me_Return_Last_Identify(mysql, ref Excp);

            Suppliers_AccountsCls ca = new Suppliers_AccountsCls();
            ca.Set_SuppliersAccount_Balance(ref Excp, date, Supplier_ID, total, Process_No, -2);

        }

        public void Delete_Nazallat_of_Day(ref int Excp, long Nazlla_Id, long Supplier_ID, decimal total)
        {


            string mysql = " Delete from Total_of_Nazlla where ID=" + Nazlla_Id;
            opp.delete_me(mysql, ref Excp);
            if (Excp != 0)
            {
                return;
            }
            total = total * -1;
            Suppliers_AccountsCls ca = new Suppliers_AccountsCls();
            ca.Set_SuppliersAccount_Balance(ref Excp, DateTime.Now.Date, Supplier_ID, total, Nazlla_Id, -5, 1);

        }

        public DataSet Get_Nazalat(DateTime date_from, DateTime date_To, long Machine_Id, long Supplier_Id)
        {
            string mysql = @" SELECT Machin_list.Id , Machin_list.Machin_Name , Machin_list.price_of_nazlla , Total_of_Nazlla.ID AS Nazlla_ID, Total_of_Nazlla.date,Total_of_Nazlla.number_of_nazlla, Total_of_Nazlla.mashineID, Total_of_Nazlla.total, Total_of_Nazlla.nazlla_price,Total_of_Nazlla.Supplier_ID 
             FROM Machin_list INNER JOIN Total_of_Nazlla ON Machin_list.Id = Total_of_Nazlla.mashineID  ";
            mysql += " WHERE Total_of_Nazlla.date >= '" + operations.dateFormat(date_from) + "' and Total_of_Nazlla.date <='" + operations.dateFormat(date_To) + "' and Total_of_Nazlla.mashineID=" + Machine_Id + " AND Supplier_id=" + Supplier_Id + "";
            mysql += " Order by Total_of_Nazlla.date ";
            return opp.return_dataset(mysql);

        }


    }

}
