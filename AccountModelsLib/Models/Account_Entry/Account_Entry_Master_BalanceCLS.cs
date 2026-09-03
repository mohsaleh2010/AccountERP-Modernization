using IdGen;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
 
using System.Drawing;
   
using System.IO;
using System.Linq;
using System.Text;

namespace AccountModelsLib.Models
{
    public class Account_Entry_Master_BalanceCLS
    {
        public Account_Entry_Master_Balance aemb = new Account_Entry_Master_Balance();
        public Account_Entry_Master_BalanceCLS()
        { }
        

        public void Get_Account_Entry_Master_Balance(ref operations opp, long Id)
        {
            aemb = opp.db.Account_Entry_Master_Balance.AsNoTracking().Where(f => f.Id == Id).FirstOrDefault();
            //string mysql = " Select * from Account_Entry_Master_Balance Where Id=" + Id;
            //DataTable dt = opp.return_DataTable(mysql);

            //if (dt.Rows.Count > 0)
            //{
            //    ReadData(dt, 0);
            //}
        }
        public DataTable Get_Account_Entry_Master_Balance(long SubAccount_Id)
        {
            DataTable dt = new DataTable();
            try
            {
                string mysql;

                mysql = @" SELECT DISTINCT Account_Entry_Master_Balance.*, Submain_Account.*
FROM            Submain_Account INNER JOIN
                         Account_Entry_Master_Balance ON Submain_Account.Id = Account_Entry_Master_Balance.SubAccount_Id
WHERE         (Submain_Account.Id =  " + SubAccount_Id + ")  ";


                operations opp = new operations();
                dt = opp.return_DataTable(mysql);

                return dt;
            }
            catch (Exception ee)
            {



            }
            return dt;
        }


        public DataTable Get_Account_Entry_Master_Balance_Submain_Account(long SubAccount_Id)
        {
            DataTable dt = new DataTable();
            try
            {
                string mysql;

                mysql = @" SELECT    *
FROM            Submain_Account  
WHERE        ( SubAccount_Id =  " + SubAccount_Id + ")  ";


                operations opp = new operations();
                dt = opp.return_DataTable(mysql);

                return dt;
            }
            catch (Exception ee)
            {



            }
            return dt;
        }


        public DataTable GetAccount_Entry_Master_Balance(ref operations opp, long SubAccount_Id)
        {
            string mysql = "SELECT * FROM Account_Entry_Master_Balance where SubAccount_Id =" + SubAccount_Id;
            DataTable dt = opp.return_DataTable(mysql);

            return dt;
        }
         
        
       
        public void InsertAccount_Entry_Master_Balance(ref operations opp)
        {
            opp.db.Account_Entry_Master_Balance.Add(aemb);
            opp.db.SaveChanges();

            

        }

      
      
        public long  UpdateAccount_Entry_Master_Balance(ref operations opp)
        {
            opp.db.Entry(aemb).State = EntityState.Deleted;
            opp.db.SaveChanges();

            return aemb.Id;

        }

        public void Delete_Account_Entry_Master_Balance_ImagesStore(string ImageId)
        {
            operations opp = new operations();
            //Set insert query
            string mysql = "Delete From   Account_Entry_Master_Balance_ImagesStore Where ImageId=" + ImageId;//
            opp.insert_me(mysql);

        }

        public void Delete_Account_Entry_Master_Balance_ForSubAccounts(ref operations opp, long SubAccount_Id)
        {
           
            //Set insert query
            string mysql = "Delete From   Account_Entry_Master_Balance  Where Submain_Account_ID=" + SubAccount_Id;//
            opp.insert_me(mysql);

        }


    }
}
