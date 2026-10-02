using System;
using System.Collections.Generic;
using System.Text;
 using AccountModelsLib; using AccountModelsLib.Models;
using System.Data;
 
using System.Collections;
 
namespace AccountModelsLib
{
    public class sup_quantity
    {
        
        public DataSet Get_Slaes_Destruction_Data(ref int Excp, long Supplier_id, DateTime dtFrom, DateTime dtTo)
        {
            string mysql = " SELECT Item.Item_Name AS Items_Name, unit.B_Unit AS Unit_Larg, unit.S_unit AS Unit_Small, unit.B_Unit_Value, unit.S_unit_Value,  Suppliers_Destruction.Unit_Item_Table_Id, Suppliers_Destruction.supplier_id, Suppliers_Destruction.dt_date, Suppliers_Destruction.total,  Suppliers_Destruction.price, Suppliers_Destruction.large_quantity, Suppliers_Destruction.small_quantity, Suppliers_Destruction.supplier_name,  Suppliers_Destruction.id, Suppliers_Destruction.Account_ID ,Suppliers_Destruction.Notes";
            mysql = mysql + " FROM Item INNER JOIN  Unit_item ON Item.id = Unit_item.Item_id INNER JOIN  unit ON Unit_item.unit_id = unit.id INNER JOIN  Suppliers_Destruction ON Unit_item.id = Suppliers_Destruction.Unit_Item_Table_Id    ";
            mysql = mysql + "  Where (Suppliers_Destruction.supplier_id=" + Supplier_id + " ) ";
            if (dtFrom == dtTo)
            {
                mysql = mysql + " AND (Suppliers_Destruction.dt_date ='" + operations.dateFormat(dtFrom) + "') ";
            }
            else
            {
                mysql = mysql + " AND (Suppliers_Destruction.dt_date >='" + operations.dateFormat(dtFrom) + "') AND (Suppliers_Destruction.dt_date <='" + operations.dateFormat(dtTo) + "') ";
            }

            DataSet G_DS = new DataSet();
            G_DS.Clear();
            try
            {
                operations opp = new operations();
                G_DS = opp.return_dataset(mysql, "Suppliers_Destruction");
                Excp = 1;
                opp = null;
                return G_DS;
            }
            catch
            {
                Excp = -1;
                return G_DS;
            }
        }


        public DataSet Get_Slaes_Destruction_Data_Customer(ref int Excp, long Customer_id, DateTime dtFrom, DateTime dtTo)
        {
            string mysql = " SELECT Item.Item_Name AS Items_Name, unit.B_Unit AS Unit_Larg, unit.S_unit AS Unit_Small, unit.B_Unit_Value, unit.S_unit_Value,  Customers_Destruction.Unit_Item_Table_Id, Customers_Destruction.Customer_id, Customers_Destruction.dt_date, Customers_Destruction.total,  Customers_Destruction.price, Customers_Destruction.large_quantity, Customers_Destruction.small_quantity, Customers_Destruction.Customer_name,  Customers_Destruction.id, Customers_Destruction.Account_ID ,Customers_Destruction.Notes";
            mysql = mysql + " FROM Item INNER JOIN  Unit_item ON Item.id = Unit_item.Item_id INNER JOIN  unit ON Unit_item.unit_id = unit.id INNER JOIN  Customers_Destruction ON Unit_item.id = Customers_Destruction.Unit_Item_Table_Id    ";
            mysql = mysql + "  Where (Customers_Destruction.Customer_id=" + Customer_id + " ) ";
            if (dtFrom == dtTo)
            {
                mysql = mysql + " AND (Customers_Destruction.dt_date ='" + operations.dateFormat(dtFrom) + "') ";
            }
            else
            {
                mysql = mysql + " AND (Customers_Destruction.dt_date >='" + operations.dateFormat(dtFrom) + "') AND (Customers_Destruction.dt_date <='" + operations.dateFormat(dtTo) + "') ";
            }

            DataSet G_DS = new DataSet();
            G_DS.Clear();
            try
            {
                operations opp = new operations();
                G_DS = opp.return_dataset(mysql, "Customers_Destruction");
                Excp = 1;
                opp = null;
                return G_DS;
            }
            catch
            {
                Excp = -1;
                return G_DS;
            }
        }
        public void delete_sub(ref int Excp,ref string Msg, long id, decimal Total, long Supplier_Id)
        {
            
            operations opp = new operations(1);
            try
            {
                string mysql = "DELETE FROM suppliers_Destruction where id=" + id;
                opp.delete_me(mysql);

                Suppliers_AccountsCls ca = new Suppliers_AccountsCls();
                int Reference_Type = Common.Bill_Suppliers_Destruction_Id;
                ca.Delete_Supplier_Balance(ref Excp, ref opp, id, Reference_Type);

                //«‰‘«¡ «·ﬁÌœ

                Account_Entry_Invoice_CLS aa = new Account_Entry_Invoice_CLS();
                AccountEntry_CLS ac = new AccountEntry_CLS();
                ac.IsNotDeletePrevEntry = false;
                ac.Delete_EntryFromAutomatic(ref opp, ref Excp, ref Msg, id, Reference_Type, ac.IsNotDeletePrevEntry);


                if (Excp == 0)
                {
                    opp.Commit();
                    Msg=" „ Õ–› «·⁄„·Ì…" ;
                }
                else
                {
                    opp.Rollback();
                    Msg = "Œÿ√ Õ–› «·⁄„·Ì…";
                }
            }
            catch (Exception ee)
            {

                opp.Rollback();
                Msg = "Œÿ√ Õ–› «·⁄„·Ì…";
            }

            

        }

        public void delete_sub_Supplier(ref int Excp, ref string Msg, int id, decimal Total, long Supplier_Id, decimal S_unit_value, decimal Larg_Qnty, decimal Small_Qnty, long Unit_Item_Table_Id)
        {
            
            decimal Larg_QntyBalance = 0;
            decimal Small_QntyBalance = 0;
            decimal StillLargBalance = 0;
            decimal StillSmallBalance = 0;
            string mysql = "";
            operations opp = new operations();
            ArrayList arr = new ArrayList();

            mysql = " SELECT Unit_Item_Table_Id, Balance_Qnty_Larg, Balance_Qnty_Small, Supplier_Id FROM Work_Out_Details_Balance WHERE (Supplier_Id = " + Supplier_Id + ") AND (Unit_Item_Table_Id = " + Unit_Item_Table_Id + ") ";

            DataSet ds = opp.return_dataset(mysql);
            if (ds.Tables[0].Rows.Count > 0)
            {
                Larg_QntyBalance = decimal.Parse(ds.Tables[0].Rows[0]["Balance_Qnty_Larg"].ToString());
                Small_QntyBalance = decimal.Parse(ds.Tables[0].Rows[0]["Balance_Qnty_Small"].ToString());
            }

            Cls_Conversion_Unit cu = new Cls_Conversion_Unit();
            cu.ConVersionQuntity_ADD_ToItem(Larg_Qnty, Small_Qnty, Larg_QntyBalance, Small_QntyBalance, ref StillLargBalance, ref StillSmallBalance, S_unit_value);


            mysql = "DELETE FROM suppliers_Destruction where id=" + id;
            arr.Add(mysql);
            // opp.delete_me(mysql);
            mysql = " UPDATE Work_Out_Details_Balance SET Balance_Qnty_Larg =" + StillLargBalance + ", Balance_Qnty_Small =" + StillSmallBalance + " WHERE (Unit_Item_Table_Id = " + Unit_Item_Table_Id + ") AND (Supplier_Id = " + Supplier_Id + ") ";
            arr.Add(mysql);
            int Status = opp.insert_me_transaction(arr);
            if (Status == 1)
            {
                Suppliers_AccountsCls ca = new Suppliers_AccountsCls();
                ca.Set_SuppliersAccount_Balance(ref Excp, DateTime.Now.Date, Supplier_Id, (decimal)Total, id, -10);
                if (Excp == 0)
                {
                    Msg = " „ Õ–› «·”Ã·";
                }
            }
            else
            {
                Msg = "·„ Ì „ Õ–› «·”Ã·";
            }



            /*
              
         
                    
            */

        }

        public void delete_sub_Customer(ref int Excp, ref string Msg, int id, decimal Total, long Customer_Id, decimal S_unit_value, decimal Larg_Qnty, decimal Small_Qnty, long Unit_Item_Table_Id)
        {
            
            decimal Larg_QntyBalance = 0;
            decimal Small_QntyBalance = 0;
            decimal StillLargBalance = 0;
            decimal StillSmallBalance = 0;
            string mysql = "";
            operations opp = new operations();
            ArrayList arr = new ArrayList();

            mysql = " SELECT Unit_Item_Table_Id, Balance_Qnty_Larg, Balance_Qnty_Small, Supplier_Id FROM Work_In_Details_Balance WHERE (Customer_Id = " + Customer_Id + ") AND (Unit_Item_Table_Id = " + Unit_Item_Table_Id + ") ";

            DataSet ds = opp.return_dataset(mysql);
            if (ds.Tables[0].Rows.Count > 0)
            {
                Larg_QntyBalance = decimal.Parse(ds.Tables[0].Rows[0]["Balance_Qnty_Larg"].ToString());
                Small_QntyBalance = decimal.Parse(ds.Tables[0].Rows[0]["Balance_Qnty_Small"].ToString());
            }

            Cls_Conversion_Unit cu = new Cls_Conversion_Unit();
            cu.ConVersionQuntity_ADD_ToItem(Larg_Qnty, Small_Qnty, Larg_QntyBalance, Small_QntyBalance, ref StillLargBalance, ref StillSmallBalance, S_unit_value);


            mysql = "DELETE FROM Customers_Destruction where id=" + id;
            arr.Add(mysql);
            // opp.delete_me(mysql);
            mysql = " UPDATE Work_In_Details_Balance SET Balance_Qnty_Larg =" + StillLargBalance + ", Balance_Qnty_Small =" + StillSmallBalance + " WHERE (Unit_Item_Table_Id = " + Unit_Item_Table_Id + ") AND (Customer_Id = " + Customer_Id + ") ";
            arr.Add(mysql);
            int Status = opp.insert_me_transaction(arr);
            if (Status == 1)
            {
                CustomersAccountCLS ca = new CustomersAccountCLS();
                ca.Set_CustomersAccount_Balance(ref Excp, DateTime.Now.Date, Customer_Id, (decimal)Total, id, -14);
                if (Excp == 0)
                {
                    Msg = " „ Õ–› «·”Ã·";
                }
            }
            else
            {
                Msg = "·„ Ì „ Õ–› «·”Ã·";
            }



            /*
              
         
                    
            */

        }
        //edit 
        public void Edit_Suppliers_Destruction(ref int Excp, ref string Msg, long Suppliers_Destruction_Id, decimal kelo, decimal price, decimal total, DateTime dt_date, long Supplier_Id, string Notes)
        {
            operations opp = new operations();

            string mysql = "UPDATE suppliers_Destruction SET   quantity =" + kelo + ",   price =" + price + ", total =" + total + ", dt_date ='" + operations.dateFormat(dt_date) + "' where Id=" + Suppliers_Destruction_Id;
            opp.insert_me(mysql);


            //SuppliersAccount ca = new SuppliersAccount();
            //ca.Set_SuppliersAccount_Balance(ref Excp, dt_date, supplier_id, total * -1, suppliers_qu_Id, -9, 1);
            Suppliers_AccountsCls ca = new Suppliers_AccountsCls();
            ca.ProcessNo = -1;
            ca.Set_Supplier_Balance_Make_Destruction(ref Excp, ref opp, Supplier_Id, Suppliers_Destruction_Id, total, Notes);
            //«‰‘«¡ «·ﬁÌœ

            Account_Entry_Invoice_CLS aa = new Account_Entry_Invoice_CLS();
            aa.IsNotDeletePrevEntry = false ;
            aa.Make_EntryFor_Suppliers_Destruction(ref Excp, ref Msg, ref opp, dt_date, Suppliers_Destruction_Id, Notes);

            if (Excp == 0)
            {
                opp.Commit();
                Msg = " „  ⁄„·Ì… «· ⁄œÌ· »‰Ã«Õ";
            }
            else
            {
                opp.Rollback();
                Msg = "Œÿ√ ›Ï  «·≈÷«›… ";
            }

        }
        //save
        public void Save_Suppliers_Destruction(ref int Excp,ref string Msg, decimal kelo, decimal price, decimal total, DateTime dt_date, long Supplier_Id, long SubAccount_Id, string Notes)
        {
            operations opp = new operations(1);
            Excp = 0;
            
            try
            {
                string mysql = "INSERT INTO Suppliers_Destruction (supplier_id, SubAccount_Id, quantity,  price, total,dt_date,Notes) VALUES ";
                mysql = mysql + " (" + Supplier_Id + "," + SubAccount_Id + "," + kelo + ", " + price + "," + total + ",'" + operations.dateFormat(dt_date) + "','" + Notes + "')";

                long Suppliers_Destruction_Id = opp.Insert_me_Return_Last_Identify(mysql, ref Excp);

                //SuppliersAccount ca = new SuppliersAccount();
                //ca.Set_SuppliersAccount_Balance(ref Excp, dt_date, supplier_id, total * -1, suppliers_qu_Id, -9, 1);
                Suppliers_AccountsCls ca = new Suppliers_AccountsCls();
                ca.Set_Supplier_Balance_Make_Destruction(ref Excp, ref opp, Supplier_Id, Suppliers_Destruction_Id, total, Notes);
                //«‰‘«¡ «·ﬁÌœ

                Account_Entry_Invoice_CLS aa = new Account_Entry_Invoice_CLS();
                aa.IsNotDeletePrevEntry = true;
                aa.Make_EntryFor_Suppliers_Destruction(ref Excp, ref Msg, ref opp, dt_date, Suppliers_Destruction_Id, Notes);

                if (Excp == 0)
                {
                    opp.Commit();
                    Msg = " „  ⁄„·Ì… «·Õ›Ÿ »‰Ã«Õ";
                }
                else
                {
                    opp.Rollback();
                    Msg = "Œÿ√ ›Ï  «·≈÷«›… ";
                }
            }
            catch (Exception ee)
            {

                opp.Rollback();
                Msg = "Œÿ√ ›Ï  «·≈÷«›… ";
            }
        }

        public void save_data_supplier(ref int Excp,ref string Msg,  decimal Larg_Qnty, decimal Small_Qnty, decimal price, decimal total, DateTime dt_date, long supplier_id, long Unit_Item_Table_Id, decimal S_unit_value, decimal qntyLarg_Balance, decimal QntySmallBalance)
        {
             
            string mysql = "INSERT INTO suppliers_Destruction (supplier_id, small_quantity, large_quantity, price, total,dt_date,Account_ID,Unit_Item_Table_Id) VALUES ";
            mysql = mysql + " (" + supplier_id + "," + Small_Qnty + "," + Larg_Qnty + "," + price + "," + total + ",'" + operations.dateFormat(dt_date) + "',84," + Unit_Item_Table_Id + ")";
            operations oop = new operations();
            long suppliers_qu_Id = oop.Insert_me_Return_Last_Identify(mysql, ref Excp);

            Suppliers_AccountsCls ca = new Suppliers_AccountsCls();
            ca.Set_SuppliersAccount_Balance(ref Excp, dt_date, supplier_id, total, suppliers_qu_Id, -9, 1);

            decimal StillLargBalance = 0;
            decimal StillSmallBalance = 0;
            Cls_Conversion_Unit cu = new Cls_Conversion_Unit();

            cu.ConVersionQuntity_Subtract_ToItem(false, Larg_Qnty, Small_Qnty, qntyLarg_Balance, QntySmallBalance, ref StillLargBalance, ref StillSmallBalance, S_unit_value);

            mysql = " UPDATE Work_Out_Details_Balance SET Balance_Qnty_Larg =" + StillLargBalance + ", Balance_Qnty_Small =" + StillSmallBalance + " WHERE (Unit_Item_Table_Id = " + Unit_Item_Table_Id + ") AND (Supplier_Id = " + supplier_id + ") ";
            oop.update_me(mysql);
            if (Excp == 0)
            {
                Msg = " „  ⁄„·Ì… «·Õ›Ÿ »‰Ã«Õ";
            }
        }
        public void save_data_Customer(ref int Excp, ref string Msg, decimal Larg_Qnty, decimal Small_Qnty, decimal price, decimal total, DateTime dt_date, long Customer_id, long Unit_Item_Table_Id, decimal S_unit_value, decimal qntyLarg_Balance, decimal QntySmallBalance)
        {
           
            string mysql = "INSERT INTO Customers_Destruction (Customer_id, small_quantity, large_quantity, price, total,dt_date,Account_ID,Unit_Item_Table_Id) VALUES ";
            mysql = mysql + " (" + Customer_id + "," + Small_Qnty + "," + Larg_Qnty + "," + price + "," + total + ",'" + operations.dateFormat(dt_date) + "',84," + Unit_Item_Table_Id + ")";
            operations oop = new operations();
            long suppliers_qu_Id = oop.Insert_me_Return_Last_Identify(mysql, ref Excp);

            CustomersAccountCLS ca2 = new CustomersAccountCLS();
            ca2.Set_CustomersAccount_Balance(ref Excp, dt_date, Customer_id, total, suppliers_qu_Id, -13, 1);

            //SuppliersAccount ca = new SuppliersAccount();
            //ca.Set_SuppliersAccount_Balance(ref Excp, dt_date, Customer_id, total * -1, suppliers_qu_Id, -9, 1);

            decimal StillLargBalance = 0;
            decimal StillSmallBalance = 0;
            Cls_Conversion_Unit cu = new Cls_Conversion_Unit();

            cu.ConVersionQuntity_Subtract_ToItem(false, Larg_Qnty, Small_Qnty, qntyLarg_Balance, QntySmallBalance, ref StillLargBalance, ref StillSmallBalance, S_unit_value);

            mysql = " UPDATE Work_In_Details_Balance SET Balance_Qnty_Larg =" + StillLargBalance + ", Balance_Qnty_Small =" + StillSmallBalance + " WHERE (Unit_Item_Table_Id = " + Unit_Item_Table_Id + ") AND (Customer_id = " + Customer_id + ") ";
            oop.update_me(mysql);
            if (Excp == 0)
            {
                Msg = " „  ⁄„·Ì… «·Õ›Ÿ »‰Ã«Õ";
            }
        }

    }
}

