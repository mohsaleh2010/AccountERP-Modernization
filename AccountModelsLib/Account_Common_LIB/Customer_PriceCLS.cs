using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using Microsoft.Data.SqlClient;
using System.Data.OleDb;
using Connection4It; using AccountModelsLib; using AccountModelsLib.Models;



namespace AccountModelsLib
{
	/// <summary>
	/// Summary description for job_List.
	/// </summary>
    public class Customer_PriceCLS
	{
		public DataSet G_DS;
        public Customer_PriceCLS()
		{
			G_DS=new DataSet();
			//
			// TODO: Add constructor logic here
			//
		}

        public void Add_Item(ref int Excp, int Customer_Id, int Item_Id, decimal PriceUnit, int Store_id)
		{
		 
			try
			{
                AccountModelsLib.operations opp=new AccountModelsLib.operations();
                DataSet dss = opp.return_dataset("Select * from Customer_Price where Customer_Id=" + Customer_Id + " and Item_Id=" + Item_Id + "");
                if (dss.Tables[0].Rows.Count > 0)
                {
                    Excp = 5;
                    return;
                }

                string mysql = " INSERT INTO Customer_Price (Customer_Id,Item_Id,PriceUnit,Store_id) ";
                mysql = mysql + " VALUES (" + Customer_Id + "," + Item_Id + "," + PriceUnit + "," + Store_id + ")";
				
				
				 
				opp.insert_me(mysql,ref Excp);
				opp=null;
                Excp = 0;
				 
			}
			catch(Exception e)
			{
				throw(e);
			}
			 
		}


        public void Edit_job_List(ref int Excp, int Id, int Customer_Id, int Item_Id, decimal PriceUnit, int Store_id)
		{
			try
			{

                string mysql = " UPDATE Customer_Price SET Customer_Id=" + Customer_Id + ",Item_Id=" + Item_Id + ",PriceUnit=" + PriceUnit + ",Store_id=" + Store_id + " WHERE (id =" + Id + ")";
				AccountModelsLib.operations opp=new AccountModelsLib.operations();
				 
				opp.update_me(mysql,ref Excp);
				opp=null;
				Excp=1;
				 
			}
			catch(Exception e)
			{
				Excp=-1;
				throw(e);
			}
			 
		}

		 public DataSet Get_job_List(ref int Excp,int Ch)
		{
			G_DS.Clear();
			try
			{
				string mysql;

                mysql = " SELECT  Customer_Price.*,Customer.Name, Item.Item_Name ";
                mysql= mysql + " FROM  Customer_Price INNER JOIN Customer ON Customer_Price.Customer_Id = Customer.Id INNER JOIN  Unit_item ON Customer_Price.Item_Id = Unit_item.id INNER JOIN  Item ON Unit_item.Item_id = Item.id ";
                if (Ch == 1)
                {
                    mysql = mysql + "   ORDER BY Customer.Name";
                }
                else
                {
                    mysql = mysql + "   ORDER BY Item.Item_Name";
                }
				 
				AccountModelsLib.operations opp=new AccountModelsLib.operations();
                G_DS = opp.return_dataset(mysql, "Customer_Price");
				Excp=1;
				opp=null;
				return G_DS;
			}
			catch
			{
				Excp=-1;
				return G_DS;
			}
		}

        public DataSet Get_job_List(ref int Excp, int Ch,int Customer_ID)
        {
            G_DS.Clear();
            try
            {
                string mysql;

                mysql = " SELECT  Customer_Price.*,Customer.Name, Item.Item_Name ";
                mysql = mysql + " FROM  Customer_Price INNER JOIN Customer ON Customer_Price.Customer_Id = Customer.Id INNER JOIN  Unit_item ON Customer_Price.Item_Id = Unit_item.id INNER JOIN  Item ON Unit_item.Item_id = Item.id ";
                mysql = mysql + " Where Customer_Price.Customer_Id= " + Customer_ID + " ";
                if (Ch == 1)
                {
                    mysql = mysql + "   ORDER BY Customer.Name";
                }
                else
                {
                    mysql = mysql + "   ORDER BY Item.Item_Name";
                }

                AccountModelsLib.operations opp = new AccountModelsLib.operations();
                G_DS = opp.return_dataset(mysql, "Customer_Price");
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

		public void Delete_job_List( ref int Excp ,int ID)
		{
			try
			{

                string mysql = " Delete from Customer_Price where id=" + ID;
				AccountModelsLib.operations opp=new AccountModelsLib.operations();
				 
				opp.delete_me(mysql);
				opp=null;
				Excp=1;
				 
			}
			catch(Exception e)
			{
				Excp=-1;
				throw(e);
			}
		}
		 
		 

	}
}
