using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using Microsoft.Data.SqlClient;
using System.Data.OleDb;
using AccountModelsLib.Models;


namespace AccountModelsLib
{
    
    public class AccountsTree
    {
        DataSet G_DS;
        public AccountsTree()
    {
        G_DS = new DataSet();
    
    }
       public  bool SmallTree = false;
        //public    int Branch_Id=Common.Branch_Id;
        public DataSet Get_SubAccount(ref int Excp,long MainAccount_Id)
        {
            G_DS.Clear();
            try
            {
                string mysql;

                mysql = @"SELECT DISTINCT 
                         Main_Account.SubAccount_Name + '  /  ' + Submain_Account.SubAccount_Name AS Name, 
                         Submain_Account.SubAccount_NO + '  ' + Submain_Account.SubAccount_Name AS Account_Name_No, Main_Account.SubAccount_Name AS MainAccount_Name, 
                         Main_Account.SubAccount_NO AS MainAccount_No , 
                         Main_Account.Currency_Id AS main_Currency_Id, Main_Account.Notes AS main_Notes, Submain_Account.*
FROM            Submain_Account LEFT OUTER JOIN
                         Submain_Account AS Main_Account ON Submain_Account.Main_Account_Id = Main_Account.Id ";
                mysql = mysql + " Where Submain_Account.Main_Account_Id=" + MainAccount_Id;
                mysql = mysql + " ORDER BY  Submain_Account.SubAccount_Name ";
                
                AccountModelsLib.operations opp = new AccountModelsLib.operations();
                G_DS = opp.return_dataset(mysql, "Submain_Account");
                Excp = 1;
                opp = null;
                return G_DS;
            }
            catch(Exception ee)
            {
                
                Excp = -1;
                return G_DS;
            }
        }
    public string Get_Submain_Account_Name(int Account_Id)
        {
             
            Submain_AccountCLS submain_AccountCLS = new Submain_AccountCLS();
            decimal Submain_Account_Balance = 0;
            string Account_Name = submain_AccountCLS.Get_Submain_Account_Name(  Account_Id, ref   Submain_Account_Balance);
         
            return Account_Name;
        }
    public DataTable  Get_Submain_Account_Data(long SubAccount_Id)
    {
        string mysql = @" SELECT DISTINCT 
                         Main_Account.SubAccount_Name + '  /  ' + Submain_Account.SubAccount_Name AS Name, 
                         Submain_Account.SubAccount_NO + '  ' + Submain_Account.SubAccount_Name AS Account_Name_No, Main_Account.SubAccount_Name AS MainAccount_Name, 
                         Main_Account.SubAccount_NO AS MainAccount_No,   
                         Main_Account.Currency_Id AS main_Currency_Id, Main_Account.Notes AS main_Notes, Submain_Account.*
FROM            Submain_Account LEFT OUTER JOIN
                         Submain_Account AS Main_Account ON Submain_Account.Main_Account_Id = Main_Account.Id ";
        mysql = mysql + " Where Submain_Account.Id=" + SubAccount_Id;
        
      
        AccountModelsLib.operations opp = new AccountModelsLib.operations();
        DataTable  dt = opp.return_DataTable(mysql);
        return dt;
        
    }

    
       
        public int Masrofat_Only = 0;
      public  static  string Get_SQL_AccountsCode()
        {
            Submain_AccountCLS submain_AccountCLS = new Submain_AccountCLS();
           return  submain_AccountCLS.Get_SQL_AccountsCode();
             
        }
        public DataTable  Get_SubMain_Account(ref int Excp, int Branch_Id)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
        return   Get_SubMain_Account(ref opp,ref   Excp,   Branch_Id);
        }


        public DataTable Get_FixedAsset_Account(ref int Excp, int Branch_Id)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations();

          return    Get_FixedAsset_Account(ref   opp, ref   Excp,   Branch_Id);


        }
        public static DataTable get_TreeToPrint(long Branch_Id, int Account_Level)
        {
            Submain_AccountCLS submain_AccountCLS = new Submain_AccountCLS();
            return submain_AccountCLS.get_TreeToPrint(  Branch_Id,   Account_Level);
             
        }
        public DataTable Get_FixedAsset_Account(ref AccountModelsLib.operations opp, ref int Excp, int Branch_Id)
        {
            Submain_AccountCLS submain_AccountCLS = new Submain_AccountCLS();
            return submain_AccountCLS.Get_FixedAsset_Account(ref   opp, ref   Excp,   Branch_Id);
             
        }
        


        public DataTable Get_SubMain_Account_AllAccounts(ref int Excp)
        {
            Submain_AccountCLS submain_AccountCLS = new Submain_AccountCLS();
            return submain_AccountCLS.Get_SubMain_Account_AllAccounts(ref   Excp);

             
        }
        public DataTable Get_SubMain_Account(ref AccountModelsLib.operations opp, ref int Excp, int Branch_Id)
        {
            DataTable dt = new DataTable();
            try
            {
                string mysql=Get_SQL_AccountsCode();

                mysql = mysql + "  WHERE   (Submain_Account.Id > 0 ) ";
                if (SmallTree)
                {
                   // mysql = mysql + "  WHERE  (Submain_Account.Id = 138) OR   (Submain_Account.Id <> 40) AND (Submain_Account.Main_Account_Id = 133) OR  (Submain_Account.Main_Account_Id = 93) AND (Submain_Account.Submain_Account_Balance = 0) OR   (Submain_Account.Id = 140) OR   (Submain_Account.Id = 141) OR   (Submain_Account.Id = 82) OR   (Submain_Account.Id = 11) OR   (Submain_Account.Id = 83) OR   (Submain_Account.Id = 84) ";

                    if (Masrofat_Only != 0)
                    {
                        mysql = mysql + "  AND  (Submain_Account.Head_Main_Account = " + Masrofat_Only + ") ";
                    }

                }
                else
                {
                    if (Masrofat_Only != 0)
                    {
                        mysql = mysql + "  And  (Submain_Account.Head_Main_Account = " + Masrofat_Only + ") ";
                    }
                }
                if (Branch_Id > 0)
                {
                  //  mysql = mysql + "  And   (Submain_Account.Branch_Id = " + Branch_Id + ") ";
                    mysql = mysql + "  AND  ((Submain_Account.Branch_Id = " + Branch_Id + ") OR  (Submain_Account.Branch_Id = -1 ))";
                }
                ///·⁄—÷ «·„’—Ê›«  ›ﬁÿ
                ///⁄—÷ ﬁ«∆„… —∆Ì”… ›ﬁÿ „À· ⁄—÷ «·«’Ê· ›ﬁÿ «Ê «·Œ’Ê„ ›ﬁÿ «Ê «·„’—Ê›«  ›ﬁÿ

                mysql = mysql + " ORDER BY Submain_Account.Id ";// " ORDER BY Submain_Account.Head_Main_Account, Submain_Account.lastPartIncrease ";

              
                dt = opp.return_DataTable(mysql);
                Excp = 1;
               
                return dt;
            }
            catch(Exception ee)
            {
                Excp = -1;
                return dt;
            }
        }

        
        public DataTable Get_SubMain_Account_WithFirstBalance(ref int Excp, int Branch_Id)
        {
            DataTable dt = new DataTable();
            try
            {
                string mysql = Get_SQL_AccountsCode();


                ///⁄—÷ «·«’Ê· Ê«Œ’Ê„ ÊÕﬁÊﬁ «·„·ﬂÌÂ
                mysql = mysql + "  Where  (Submain_Account.Branch_ID = " + Branch_Id + ") AND ( Submain_Account.Head_Main_Account = 1 OR Submain_Account.Head_Main_Account = 2 OR Submain_Account.Head_Main_Account = 3 )";
                mysql = mysql + "  OR  (Submain_Account.Main_Account_Id IS NULL) ";
                //mysql =mysql + "  (Submain_Account.Branch_ID = " + Branch_Id + ") ";
                mysql = mysql + "  AND  ((Submain_Account.Branch_Id = " + Branch_Id + ") OR  (Submain_Account.Branch_Id = -1 ))";
                mysql = mysql + " ORDER BY Submain_Account.Id ";// " ORDER BY Submain_Account.Head_Main_Account, Submain_Account.lastPartIncrease ";

                AccountModelsLib.operations opp = new AccountModelsLib.operations();
                dt= opp.return_DataTable(mysql );
                Excp = 0;
               
               
            }
            catch
            {
                Excp =  1;
                
            }
            return dt;
        }
       
        public DataTable Get_SubMain_Account_ToSetCostCenter(ref int Excp, int Branch_ID)
        {
            DataTable dt = new DataTable();
            try
            {
                string mysql = Get_SQL_AccountsCode();


                ///⁄—÷ «·«’Ê· Ê«Œ’Ê„ ÊÕﬁÊﬁ «·„·ﬂÌÂ
                mysql = mysql + "  Where  ((Submain_Account.Branch_Id = " + Branch_ID + ") OR  (Submain_Account.Branch_Id = -1 )) AND ( (Submain_Account.Head_Main_Account = 4 OR Submain_Account.Head_Main_Account = 5 ) )";
                mysql = mysql + "  OR  (Submain_Account.Main_Account_Id IS NULL) ";
               // AND (Submain_Account.Branch_ID = " +  + ") ";
                     
                mysql = mysql + "  AND  ((Submain_Account.Branch_Id = " +  Branch_ID  + ") OR  (Submain_Account.Branch_Id = -1 ))";
              
                mysql = mysql + " ORDER BY Submain_Account.Id ";// " ORDER BY Submain_Account.Head_Main_Account, Submain_Account.lastPartIncrease ";

                AccountModelsLib.operations opp = new AccountModelsLib.operations();
                dt = opp.return_DataTable(mysql);
                Excp = 0;


            }
            catch
            {
                Excp = 1;

            }
            return dt;
        }
        public DataTable Get_Sub_Account(ref int Excp, int Branch_Id)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
          return  Get_Sub_Account(ref opp,ref   Excp,   Branch_Id);
        }
        public DataTable Get_Sub_Account(ref AccountModelsLib.operations opp, ref int Excp, int Branch_Id)
        {
            DataTable dt=new DataTable();
            try
            {
               
                string mysql = Get_SQL_AccountsCode();
                mysql = mysql + @" Where   (Submain_Account.Id NOT IN
                          (SELECT     MainSubmain_Account.Id AS MAinAccount_Id
                             FROM         Submain_Account AS Submain_Account_1 INNER JOIN
                                                   Submain_Account AS MainSubmain_Account ON Submain_Account_1.Main_Account_Id = MainSubmain_Account.Id))";

                if (Masrofat_Only != 0)
                {
                    mysql = mysql + "  AND  (Submain_Account.Head_Main_Account = " + Masrofat_Only + ") ";
                }

                if (SmallTree)
                {
                    mysql = mysql + "  AND(  (Submain_Account.Id = 138) OR   (Submain_Account.Id <> 40) AND (Submain_Account.Main_Account_Id = 133) OR  (Submain_Account.Main_Account_Id = 93)   OR   (Submain_Account.Id = 140) OR   (Submain_Account.Id = 141) OR   (Submain_Account.Id = 82) OR   (Submain_Account.Id = 11) OR   (Submain_Account.Id = 83) OR   (Submain_Account.Id = 84)) ";

                  

                }
                if (Branch_Id > 0)
                {
                //    mysql = mysql + "  AND  (Submain_Account.Branch_Id = " + Branch_Id + ") ";
                    mysql = mysql + "  AND    ((Submain_Account.Branch_Id = " + Branch_Id + ") OR  (Submain_Account.Branch_Id = -1 )) ";
                }
                ///·⁄—÷ «·„’—Ê›«  ›ﬁÿ
                ///⁄—÷ ﬁ«∆„… —∆Ì”… ›ﬁÿ „À· ⁄—÷ «·«’Ê· ›ﬁÿ «Ê «·Œ’Ê„ ›ﬁÿ «Ê «·„’—Ê›«  ›ﬁÿ
             
                //mysql = mysql + " ORDER BY Submain_Account.Id, Submain_Account.lastPartIncrease ";

                mysql = mysql + "ORDER BY Submain_Account.Main_Account_Id, Submain_Account.Code";
                  dt = opp.return_DataTable(mysql);
                Excp = 1;
               
                return dt;
            }
            catch
            {
                Excp = -1;
                return dt;
            }



//            string mysql = @"SELECT Submain_Account.Id, Submain_Account.SubAccount_Name, MainSubmain_Account.Id AS MAinAccount_Id, 
//                      MainSubmain_Account.SubAccount_Name AS MainSubAccount_Name
//                      FROM Submain_Account INNER JOIN
//                      Submain_Account AS MainSubmain_Account ON Submain_Account.Main_Account_Id = MainSubmain_Account.Id
//                        
//                        WHERE  (Submain_Account.Id NOT IN
//                          (SELECT     MainSubmain_Account.Id AS MAinAccount_Id
//                             FROM         Submain_Account AS Submain_Account_1 INNER JOIN
//                                                   Submain_Account AS MainSubmain_Account ON Submain_Account_1.Main_Account_Id = MainSubmain_Account.Id))";

//            mysql = mysql + "   ORDER BY MAinAccount_Id";
        }
        public DataTable Get_Submain_Account_Code(  int Branch_Id, long Code)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            DataTable dt = new DataTable();
            

                string mysql = Get_SQL_AccountsCode();
                mysql = mysql + @" Where   (Submain_Account.Id NOT IN
                          (SELECT     MainSubmain_Account.Id AS MAinAccount_Id
                             FROM         Submain_Account AS Submain_Account_1 INNER JOIN
                                                   Submain_Account AS MainSubmain_Account ON Submain_Account_1.Main_Account_Id = MainSubmain_Account.Id))";


                mysql = mysql + "  AND  (Submain_Account.Code = " + Code + ") ";
                

                 

                 
                if (Branch_Id > 0)
                {
                    //    mysql = mysql + "  AND  (Submain_Account.Branch_Id = " + Branch_Id + ") ";
                    mysql = mysql + "  AND    ((Submain_Account.Branch_Id = " + Branch_Id + ") OR  (Submain_Account.Branch_Id = -1 )) ";
                }
                
              
                dt = opp.return_DataTable(mysql);
                

                return dt;
            
        }
        public DataTable Get_Sub_FixedAccount( ref int Excp, int Branch_Id)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
           return   Get_Sub_FixedAccount(ref   opp, ref   Excp, Branch_Id);
        }
        public DataTable Get_Sub_FixedAccount( ref AccountModelsLib.operations opp, ref int Excp, int Branch_Id)
        {
            DataTable dt = new DataTable();
           
            try
            {

                string mysql = Get_SQL_AccountsCode();
                mysql = mysql + @" Where   (Submain_Account.Id NOT IN
                          (SELECT     MainSubmain_Account.Id AS MAinAccount_Id
                             FROM         Submain_Account AS Submain_Account_1 INNER JOIN
                                                   Submain_Account AS MainSubmain_Account ON Submain_Account_1.Main_Account_Id = MainSubmain_Account.Id))";

                mysql = mysql + "  AND  (Submain_Account.Account_Customer_Type_Value = " + Common.Assoul_Sabta_Account_Type + ") ";
                if (Branch_Id > 0)
                {
                    mysql = mysql + "  AND  ((Submain_Account.Branch_Id = " + Branch_Id + ") OR  (Submain_Account.Branch_Id = -1 ))";
                }
                 
                dt = opp.return_DataTable(mysql);
                Excp = 1;

                return dt;
            }
            catch
            {
                Excp = -1;
                return dt;
            }

             
        }
        public DataTable Get_Sub_AccountWithFirstBalance(ref int Excp)
        {
            DataTable dt = new DataTable();
            try
            {

                string mysql = Get_SQL_AccountsCode();
                mysql = mysql + @" Where   (Submain_Account.Id NOT IN
                          (SELECT     MainSubmain_Account.Id AS MAinAccount_Id
                             FROM         Submain_Account AS Submain_Account_1 INNER JOIN
                                                   Submain_Account AS MainSubmain_Account ON Submain_Account_1.Main_Account_Id = MainSubmain_Account.Id))";

                ///⁄—÷ «·«’Ê· Ê«Œ’Ê„ ÊÕﬁÊﬁ «·„·ﬂÌÂ
                mysql = mysql + "  AND (   Submain_Account.Head_Main_Account = 1 OR Submain_Account.Head_Main_Account = 2 OR Submain_Account.Head_Main_Account = 3 )";
               
                
                AccountModelsLib.operations opp = new AccountModelsLib.operations();
                dt = opp.return_DataTable(mysql);
                Excp = 1;
                opp = null;
                return dt;
            }
            catch
            {
                Excp = -1;
                return dt;
            }

           
        }
        public DataTable Get_Sub_AccountToSetCostCenter(ref int Excp)
        {
            DataTable dt = new DataTable();
            try
            {

                string mysql = Get_SQL_AccountsCode();
                mysql = mysql + @" Where   (Submain_Account.Id NOT IN
                          (SELECT     MainSubmain_Account.Id AS MAinAccount_Id
                             FROM         Submain_Account AS Submain_Account_1 INNER JOIN
                                                   Submain_Account AS MainSubmain_Account ON Submain_Account_1.Main_Account_Id = MainSubmain_Account.Id))";

                ///⁄—÷ «·«’Ê· Ê«Œ’Ê„ ÊÕﬁÊﬁ «·„·ﬂÌÂ
                mysql = mysql + "  AND (   Submain_Account.Head_Main_Account = 1 OR Submain_Account.Head_Main_Account = 2 OR Submain_Account.Head_Main_Account = 3 )";


                AccountModelsLib.operations opp = new AccountModelsLib.operations();
                dt = opp.return_DataTable(mysql);
                Excp = 1;
                opp = null;
                return dt;
            }
            catch
            {
                Excp = -1;
                return dt;
            }


        }
    }
}

   
 
