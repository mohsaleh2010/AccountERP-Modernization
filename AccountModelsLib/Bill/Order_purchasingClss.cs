using System;
using System.Collections.Generic;
using System.Text;
 
 
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using Microsoft.Data.SqlClient;
using System.Data.OleDb;
  using AccountModelsLib; using AccountModelsLib.Models;
namespace AccountModelsLib
{
   public class Order_purchasingClss
    {

       public Order_purchasingClss()
       {

       }

       public bool Order_Purchas_No_Found(ref int Excp, string Order_Purchas_No)
       {
           try
           {
               string mysql = " SELECT  * from Order_Receiving where id=" + Order_Purchas_No;
               operations opp = new operations();

               DataSet ds=opp.return_dataset(mysql);
               opp = null;
               Excp = 0;
               if (ds.Tables[0].Rows.Count > 0)
               {

                   return true;
               }
               else
               {
                   return false;
               }
                   
              

           }
           catch (Exception e)
           {
               Excp = 1;
               throw (e);
           }
       }
        
       public void Make_Order_Purchas(ref int Excp, DateTime Order_Date, DataSet Items_Ds,ArrayList SelectedItemsIndex, long Invoice_Id, long Order_Purshing_No, long Store_Id)
       {
           try
           {
               long Item_id ;
               long unit_id ;
               decimal qua ;
               decimal Part_qua ;
               string mysql = "";
               long Invoice_List_Id;
               ArrayList arr = new ArrayList();
               arr.Clear();
               mysql = " " ;
               mysql = " INSERT INTO Order_Purshing (Id,Order_Purshing_No, p_date, type, Invoice, Deptno) ";
               mysql = mysql + " VALUES (" + Order_Purshing_No + "," + Order_Purshing_No + ",'" + operations.dateFormat(Order_Date) + "', '1','" + Invoice_Id + "', " + Store_Id + ") ";
               
               arr.Add(mysql);

               for (int x = 0; x < Items_Ds.Tables[0].Rows.Count; x++)
               {
                   if (long.Parse(Items_Ds.Tables[0].Rows[x]["Store_id"].ToString()) == Store_Id)
                   {
                       foreach (int Sindex in SelectedItemsIndex)
                           if (Sindex == x)
                           {
                               Item_id = long.Parse(Items_Ds.Tables[0].Rows[x]["Item_id"].ToString());
                               unit_id = long.Parse(Items_Ds.Tables[0].Rows[x]["unit_id"].ToString());
                               qua = decimal.Parse(Items_Ds.Tables[0].Rows[x]["Larg_Qnty"].ToString());
                               Part_qua = decimal.Parse(Items_Ds.Tables[0].Rows[x]["Small_Qnty"].ToString());
                               Invoice_List_Id = long.Parse(Items_Ds.Tables[0].Rows[x]["Invoices_Details_Id"].ToString());
                               mysql = " INSERT INTO Detailes_Purshing (P_id, Item_id, unit_id, qua, Part_qua) ";
                               mysql = mysql + " VALUES (" + Order_Purshing_No + "," + Item_id + "," + unit_id + "," + qua + "," + Part_qua + ") ";
                               arr.Add(mysql);

                               mysql = " UPDATE Invoices_Details SET Make_Order = 1 WHERE (Id = " + Invoice_List_Id + ") ";
                               arr.Add(mysql);
                           }

                   }
               }
               operations opp = new operations();
               int Status=opp.insert_me_transaction(ref Excp ,1, arr);
               if (Status == 0)
               {
                   Excp = 1000;
               }
               opp = null;

           }
           catch (Exception e)
           {
                
               Excp = 1000;
               throw (e);
           }
       }

    }
}
