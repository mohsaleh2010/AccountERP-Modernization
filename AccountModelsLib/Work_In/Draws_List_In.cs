using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using Microsoft.Data.SqlClient;
using System.Data.OleDb;
  using AccountModelsLib; using AccountModelsLib.Models;



namespace AccountModelsLib
{
	/// <summary>
	/// Summary description for job_List.
	/// </summary>
    public class Draws_List_In
    {
		public DataSet G_DS;
        public Draws_List_In()
		{
			G_DS=new DataSet();
			//
			// TODO: Add constructor logic here
			//
		}

        public void Add_Item(ref int Excp, string ItemName, decimal Price)
		{
		 
			try
			{

                string mysql = " INSERT INTO Draws_List (Draw_Name,Price) ";
                mysql = mysql + " VALUES ('" + ItemName + "'," + Price + ")";
				
				operations opp=new operations();
				 
				opp.insert_me(mysql,ref Excp);
				opp=null;
				 
			}
			catch(Exception e)
			{
				throw(e);
			}
			 
		}


        public void Edit_Draws_List(ref int Excp, int Id, string ItemName,  decimal Price)
		{
			try
			{
                string mysql = " UPDATE Draws_List SET Draw_Name ='" + ItemName + "',  Price=" + Price + "  WHERE (id =" + Id + ")";
				operations opp=new operations();
				 
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

        public DataSet Get_Draws_List(ref int Excp)
		{
			G_DS.Clear();
			try
			{
				string mysql;

                mysql = " Select * from Draws_List ";
				 
				operations opp=new operations();
                G_DS = opp.return_dataset(mysql, "Draws_List");
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


        public void Delete_Draws_List(ref int Excp, int ID)
		{
			try
			{

                string mysql = " Delete from Draws_List where id=" + ID;
				operations opp=new operations();
				 
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
