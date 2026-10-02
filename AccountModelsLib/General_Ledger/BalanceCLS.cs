using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using Microsoft.Data.SqlClient;
using System.Data.OleDb;
  using AccountModelsLib;  using AccountModelsLib.Models;
 
namespace AccountModelsLib
{
    public class BalanceCLS
    {
        public BalanceCLS()
        {

        }
        //        Insert Into  AccountTempBalance(SubAccount_Name,SubAccount_Id  ,  Main_Account_Id  , IS_Last_Level  , Account_Level   ,  Balance_Madin  ,  Balance_Da2n  ,Equivalent_Balance_Madin , Equivalent_Balance_Da2n,Final_madin  , Final_da2n ,Final_Equivalent_madin , Final_Equivalent_da2n,Account_Currency_Id,Equivalent_Account_Currency_Id )
        //SELECT   Submain_Account.SubAccount_Name,Submain_Account.Id, Submain_Account.Main_Account_Id, Submain_Account.IS_Last_Level, Submain_Account.Account_Level,ISNull( Account_Entry_Master_Balance.Balance_Madin,0), ISNull(Account_Entry_Master_Balance.Balance_Da2n,0),  ISNull(Account_Entry_Master_Balance.Equivalent_Balance_Madin,0), ISNull(Account_Entry_Master_Balance.Equivalent_Balance_Da2n,0),0 AS madin,0 AS da2n,0 AS Equivalent_madin,0 as Equivalent_da2n,
        //Submain_Account.Currency_Id, " + Common.Entry_Currency_Id + "  AS Equivalent_Account_Currency_Id ";
        //                mysql = mysql + @" FROM Submain_Account LEFT OUTER JOIN Account_Entry_Master_Balance ON Submain_Account.Id = Account_Entry_Master_Balance.Submain_Account_ID
        //               ";
        //                mysql = mysql + @" Where   (Submain_Account.Head_Main_Account = 1 OR Submain_Account.Head_Main_Account = 2 OR Submain_Account.Head_Main_Account = 3 )  AND (Account_Entry_Master_Balance.Year_Financial_Period_Id = " + Year_Financial_Period_Id + ") AND (Account_Entry_Master_Balance.Branch_Id = " + Branch_Id + ") ";
        //  mysql = mysql + @" ORDER BY Submain_Account.Account_Level DESC ";

        public DataTable GetBalanceData_WithLevel_(long Branch_Id, long Year_Financial_Period_Id, int Level_Required, long Entry_Currency_Id)
        {
            DataTable dt_AccountTempBalance = new DataTable();
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            string mysql = "";

            try
            {
               
                mysql = @" Truncate Table AccountTempBalance";
                opp.insert_me(mysql);

                ////////////////////حفظ كل ارصدة الحسابات ى الجدول الموؤقت
                mysql = @" INSERT INTO AccountTempBalance
                         (SubAccount_Name, SubAccount_Id, Main_Account_Id, IS_Last_Level, Account_Level, Balance_Madin, Balance_Da2n, Equivalent_Balance_Madin, 
                         Equivalent_Balance_Da2n, Final_madin, Final_da2n, Final_Equivalent_madin, Final_Equivalent_da2n, Account_Currency_Id, Equivalent_Account_Currency_Id)
                    
                        SELECT   Submain_Account.SubAccount_Name, Submain_Account.Id, Submain_Account.Main_Account_Id, Submain_Account.IS_Last_Level, 
                         Submain_Account.Account_Level, 0 AS Balance_Madin, 0 AS Balance_Da2n, 0 AS Equivalent_Balance_Madin, 0 AS Equivalent_Balance_Da2n, 0 AS madin, 
                         0 AS da2n, 0 AS Equivalent_madin, 0 AS Equivalent_da2n, Submain_Account.Currency_Id,  " + Common.Entry_Currency_Id + " AS Equivalent_Account_Currency_Id ";
                mysql = mysql + @" FROM            Submain_Account
                    WHERE  (Submain_Account.Head_Main_Account = 1) OR
                         (Submain_Account.Head_Main_Account = 2) OR
                         (Submain_Account.Head_Main_Account = 3)
                            ORDER BY Submain_Account.Account_Level DESC ";

                opp.insert_me(mysql);


                /////////////////////////////////////////
                ///udate with euqivalent currency now
                /// حساب مكافىء الحسابات التى بأرصده مختله مثل الدولار بالقيمه الحاليه للعمله
                //                 mysql = @" UPDATE       AccountTempBalance
                //SET                Balance_Madin = Account_Entry_Master_Balance.Balance_Madin * Currency_Factor.Factor, 
                //                         Balance_Da2n = Account_Entry_Master_Balance.Balance_Da2n * Currency_Factor.Factor, 
                //                         Equivalent_Balance_Madin = Account_Entry_Master_Balance.Equivalent_Balance_Madin, 
                //                         Equivalent_Balance_Da2n = Account_Entry_Master_Balance.Equivalent_Balance_Da2n
                //FROM            AccountTempBalance INNER JOIN
                //                         Currency_Factor ON AccountTempBalance.Account_Currency_Id = Currency_Factor.Currency_ID_Primary AND 
                //                         AccountTempBalance.Equivalent_Account_Currency_Id = Currency_Factor.Currency_ID_Secondary INNER JOIN
                //                         Account_Entry_Master_Balance ON AccountTempBalance.SubAccount_Id = Account_Entry_Master_Balance.Submain_Account_ID ";
                //                 mysql = mysql + @" WHERE   (Account_Entry_Master_Balance.Year_Financial_Period_Id = " + Year_Financial_Period_Id + ") AND (Account_Entry_Master_Balance.Branch_Id = " + Branch_Id + ") ";

                //                opp.insert_me(mysql);

                ////////////////////////حفظ السنه الماليه والفرع فى الجدول المؤقت
                mysql = @" UPDATE AccountTempBalance Set Year_Financial_Period_Id = " + Year_Financial_Period_Id + ",Branch_Id = " + Branch_Id + " ";
                opp.insert_me(mysql);
                /////////////////////////////////
                ///وضع عمود النهائى يساوى الرصيد لحين جمع الحسسابات الفرعيه ووضعها فى النهائى

                mysql = " Update AccountTempBalance Set Final_madin = Balance_Madin,Final_da2n = Balance_Da2n ,   Final_Equivalent_madin =  Equivalent_Balance_Madin , Final_Equivalent_da2n =Equivalent_Balance_Da2n ";
                opp.insert_me(mysql);

                /////////////////////////////////////////////////////////////////
                /////وضع حساب حقوق الملكيه متفرع من الخصوم  
                ///ووضع مستوى حقوق الملكيه مستوى ثان

                mysql = "Update AccountTempBalance Set Main_Account_Id=2 , Account_Level=2 Where SubAccount_Id =3";
                opp.insert_me(mysql);

                /////  اضافة حساب الربح والخساره متفرع من حقوق الملكيه
                Incoming_StatementCLS cc = new Incoming_StatementCLS();

                decimal Profit_Value = cc.Get_ProfitFrom_Incoming_Statement(Branch_Id, Year_Financial_Period_Id, 1, Entry_Currency_Id);
                int Profit_Account_Level = 5;
                string Profit_Account_Name = "صافى الربح";

                mysql = "INSERT INTO AccountTempBalance(SubAccount_Id, Main_Account_Id, SubAccount_Name, IS_Last_Level, Account_Level, Balance_Madin, Balance_Da2n, Equivalent_Balance_Madin, Equivalent_Balance_Da2n, Final_madin, Final_da2n, Final_Equivalent_madin, Final_Equivalent_da2n, Account_Currency_Id, Equivalent_Account_Currency_Id)";
                mysql = mysql + " VALUES  (0, 3, N'" + Profit_Account_Name + " ', 1," + Profit_Account_Level + " , 0, " + Profit_Value + ", 0, " + Profit_Value + ", 0,  " + Profit_Value + ", 0,  " + Profit_Value + ", " + Common.Entry_Currency_Id + ",  " + Common.Entry_Currency_Id + ")";
                opp.insert_me(mysql);

                //////////////////////////////////////////////////////
                ///Select to sum main account
                dt_AccountTempBalance = new DataTable();
                mysql = " Select * from  AccountTempBalance Order By Account_Level DESC ";
                dt_AccountTempBalance = opp.return_DataTable(mysql);

                //////////إضافة اعمده للجدول بعدد اكبر مستوى فى شجرة الحسابات
                AlterTableAll(ref opp, ref dt_AccountTempBalance);

                /////////////////////////////////////////////////
                ////////////جمع اجمالى الحسابات الفرعيه من  
                ////Final_madin
                ////ووضعها فى
                ///Final_madin
                ///للحساب الاب
                for (int x = 0; x < dt_AccountTempBalance.Rows.Count; x++)
                {
                    string SubAccount_Id = dt_AccountTempBalance.Rows[x]["SubAccount_Id"].ToString();
                    string SubAccount_Name = dt_AccountTempBalance.Rows[x]["SubAccount_Name"].ToString();

                    if (SubAccount_Id != "")
                    {
                        UpdateData_WithLevel(long.Parse(SubAccount_Id), ref opp, ref dt_AccountTempBalance, x);
                    }
                }

                /////////////////////////اعادة التجميع

                //for (int x = 0; x < dt_AccountTempBalance.Rows.Count; x++)
                //{
                //    string SubAccount_Id = dt_AccountTempBalance.Rows[x]["SubAccount_Id"].ToString();
                //    string SubAccount_Name = dt_AccountTempBalance.Rows[x]["SubAccount_Name"].ToString();

                //    if (SubAccount_Id != "")
                //    {
                //        UpdateData_WithLevel(int.Parse(SubAccount_Id), ref opp, ref dt_AccountTempBalance, x);
                //    }
                //}

                mysql = " Select * from  AccountTempBalance Where Account_Level <= " + Level_Required + " Order By Account_Level DESC ";
                dt_AccountTempBalance = opp.return_DataTable(mysql);
                return dt_AccountTempBalance;
                opp.Commit();
            }
            catch (Exception ee)
            {
                opp.Rollback();
                string ms = ee.Message;
                //MessageBox.Show(ms);
                return dt_AccountTempBalance;
            }

        }
        public DataTable GetBalanceData_WithLevel__(long Branch_Id, long Year_Financial_Period_Id, int Level_Required, long Entry_Currency_Id)
        {
            DataTable dt_AccountTempBalance = new DataTable();
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            string mysql = "";

            try
            {

                mysql = @" Truncate Table AccountTempBalance";
                opp.insert_me(mysql);

                ////////////////////حفظ كل ارصدة الحسابات ى الجدول الموؤقت
                mysql = @" INSERT INTO AccountTempBalance
                         (SubAccount_Name, SubAccount_Id, Main_Account_Id, IS_Last_Level, Account_Level, Balance_Madin, Balance_Da2n, Equivalent_Balance_Madin, Equivalent_Balance_Da2n, Account_Currency_Id, 
                         Equivalent_Account_Currency_Id, Year_Financial_Period_Id, Branch_Id, SLevel)
                    ";
                mysql = mysql + @" 
SELECT DISTINCT 
                         Submain_Account.SubAccount_Name, Submain_Account.Id, Submain_Account.Main_Account_Id, Submain_Account.IS_Last_Level, Submain_Account.Account_Level, 
                         ISNULL(Account_Entry_Master_Balance.Balance_Madin, 0) AS Balance_Madin, ISNULL(Account_Entry_Master_Balance.Balance_Da2n, 0) AS Balance_Da2n, 
                         ISNULL(Account_Entry_Master_Balance.Equivalent_Balance_Madin, 0) AS Equivalent_Balance_Madin, ISNULL(Account_Entry_Master_Balance.Equivalent_Balance_Da2n, 0) AS Equivalent_Balance_Da2n, 
                         Submain_Account.Currency_Id, Account_Entry_Master_Balance.Equivalent_Account_Currency_Id, Account_Entry_Master_Balance.Year_Financial_Period_Id, Account_Entry_Master_Balance.Branch_Id, 0 AS SLevel
FROM            Submain_Account INNER JOIN
                         Account_Entry_Master_Balance ON Submain_Account.Id = Account_Entry_Master_Balance.Submain_Account_ID ";
                mysql = mysql + @" WHERE  (Account_Entry_Master_Balance.Year_Financial_Period_Id=" + Year_Financial_Period_Id + ")  And (Account_Entry_Master_Balance.Branch_Id=" + Branch_Id + ") AND (Submain_Account.Account_Follows = 4)   ";
                if (Entry_Currency_Id > -1)
                {
                    mysql = mysql + "AND Account_Entry_Master_Balance.Equivalent_Account_Currency_Id=" + Entry_Currency_Id + "";
                }
                int C = opp.insert_me(mysql);



                mysql = "Select Distinct  Equivalent_Account_Currency_Id From AccountTempBalance";
                DataTable dtt = opp.return_DataTable(mysql);

                for (int x = 0; x < dtt.Rows.Count; x++)
                {
                    long Equivalent_Account_Currency_Id = long.Parse(dtt.Rows[x]["Equivalent_Account_Currency_Id"].ToString());

                    for (int y = 0; y < 100; y++)
                    {
                        int SLevel = y;
                        int SSLevel = SLevel + 1;
                        mysql = @" INSERT INTO AccountTempBalance
                         (SubAccount_Name, SubAccount_Id, Main_Account_Id, IS_Last_Level, Account_Level, Balance_Madin, Balance_Da2n, Equivalent_Balance_Madin, Equivalent_Balance_Da2n, Account_Currency_Id, 
                         Equivalent_Account_Currency_Id, Year_Financial_Period_Id, Branch_Id, SLevel)

SELECT DISTINCT 
                         Submain_Account.SubAccount_Name, Submain_Account.Id, Submain_Account.Main_Account_Id, Submain_Account.IS_Last_Level, Submain_Account.Account_Level, SUM(AccountTempBalance_1.Balance_Madin) 
                         AS Balance_Madin, SUM(AccountTempBalance_1.Balance_Da2n) AS Balance_Da2n, SUM(AccountTempBalance_1.Equivalent_Balance_Madin) AS Equivalent_Balance_Madin, 
                         SUM(AccountTempBalance_1.Equivalent_Balance_Da2n) AS Equivalent_Balance_Da2n, Submain_Account.Currency_Id, AccountTempBalance_1.Equivalent_Account_Currency_Id, 
                         AccountTempBalance_1.Year_Financial_Period_Id, AccountTempBalance_1.Branch_Id, " + SSLevel + " AS SLevel ";
                        mysql = mysql + @" FROM            Submain_Account INNER JOIN
                         AccountTempBalance AS AccountTempBalance_1 ON Submain_Account.Id = AccountTempBalance_1.Main_Account_Id
GROUP BY Submain_Account.SubAccount_Name, Submain_Account.Id, Submain_Account.Main_Account_Id, Submain_Account.IS_Last_Level, Submain_Account.Account_Level, Submain_Account.Currency_Id, 
                         AccountTempBalance_1.Equivalent_Account_Currency_Id, AccountTempBalance_1.SLevel, AccountTempBalance_1.Year_Financial_Period_Id, AccountTempBalance_1.Branch_Id
 ";

                        mysql = mysql + @" HAVING (AccountTempBalance_1.Year_Financial_Period_Id = " + Year_Financial_Period_Id + ") AND(AccountTempBalance_1.Branch_Id = " + Branch_Id + ") AND(AccountTempBalance_1.Equivalent_Account_Currency_Id =" + Equivalent_Account_Currency_Id + ")";
                        mysql = mysql + @" And (AccountTempBalance_1.SLevel = " + SLevel + ") ";

                        int Count = opp.insert_me(mysql);
                        if (Count == 0)
                        {
                            break;
                        }
                    }
                }



                /////////////////////////////////////////
                ///udate with euqivalent currency now
                /// حساب مكافىء الحسابات التى بأرصده مختله مثل الدولار بالقيمه الحاليه للعمله
                //                 mysql = @" UPDATE       AccountTempBalance
                //SET                Balance_Madin = Account_Entry_Master_Balance.Balance_Madin * Currency_Factor.Factor, 
                //                         Balance_Da2n = Account_Entry_Master_Balance.Balance_Da2n * Currency_Factor.Factor, 
                //                         Equivalent_Balance_Madin = Account_Entry_Master_Balance.Equivalent_Balance_Madin, 
                //                         Equivalent_Balance_Da2n = Account_Entry_Master_Balance.Equivalent_Balance_Da2n
                //FROM            AccountTempBalance INNER JOIN
                //                         Currency_Factor ON AccountTempBalance.Account_Currency_Id = Currency_Factor.Currency_ID_Primary AND 
                //                         AccountTempBalance.Equivalent_Account_Currency_Id = Currency_Factor.Currency_ID_Secondary INNER JOIN
                //                         Account_Entry_Master_Balance ON AccountTempBalance.SubAccount_Id = Account_Entry_Master_Balance.Submain_Account_ID ";
                //                 mysql = mysql + @" WHERE   (Account_Entry_Master_Balance.Year_Financial_Period_Id = " + Year_Financial_Period_Id + ") AND (Account_Entry_Master_Balance.Branch_Id = " + Branch_Id + ") ";

                //                opp.insert_me(mysql);

                ////////////////////////حفظ السنه الماليه والفرع فى الجدول المؤقت
                //mysql = @" UPDATE AccountTempBalance Set Year_Financial_Period_Id = " + Year_Financial_Period_Id + ",Branch_Id = " + Branch_Id + " ";
                //opp.insert_me(mysql);
                /////////////////////////////////
                ///وضع عمود النهائى يساوى الرصيد لحين جمع الحسسابات الفرعيه ووضعها فى النهائى

                mysql = " Update AccountTempBalance Set Final_madin = Balance_Madin,Final_da2n = Balance_Da2n ,   Final_Equivalent_madin =  Equivalent_Balance_Madin , Final_Equivalent_da2n =Equivalent_Balance_Da2n ";
                opp.insert_me(mysql);

                /////////////////////////////////////////////////////////////////
                /////وضع حساب حقوق الملكيه متفرع من الخصوم  
                ///ووضع مستوى حقوق الملكيه مستوى ثان

                mysql = "Update AccountTempBalance Set Main_Account_Id=2 , Account_Level=2 Where SubAccount_Id =3";
                opp.insert_me(mysql);

                /////  اضافة حساب الربح والخساره متفرع من حقوق الملكيه
                Incoming_StatementCLS cc = new Incoming_StatementCLS();

                decimal Profit_Value = cc.Get_ProfitFrom_Incoming_Statement(Branch_Id, Year_Financial_Period_Id, 1, Entry_Currency_Id);
                int Profit_Account_Level = 5;
                string Profit_Account_Name = "صافى الربح";

                mysql = "INSERT INTO AccountTempBalance(SubAccount_Id, Main_Account_Id, SubAccount_Name, IS_Last_Level, Account_Level, Balance_Madin, Balance_Da2n, Equivalent_Balance_Madin, Equivalent_Balance_Da2n, Final_madin, Final_da2n, Final_Equivalent_madin, Final_Equivalent_da2n, Account_Currency_Id, Equivalent_Account_Currency_Id)";
                mysql = mysql + " VALUES  (0, 3, N'" + Profit_Account_Name + " ', 1," + Profit_Account_Level + " , 0, " + Profit_Value + ", 0, " + Profit_Value + ", 0,  " + Profit_Value + ", 0,  " + Profit_Value + ", " + Common.Entry_Currency_Id + ",  " + Common.Entry_Currency_Id + ")";
                opp.insert_me(mysql);

                //////////////////////////////////////////////////////
                ///Select to sum main account
                dt_AccountTempBalance = new DataTable();
                mysql = " Select * from  AccountTempBalance Order By Account_Level DESC ";
                dt_AccountTempBalance = opp.return_DataTable(mysql);

                //////////إضافة اعمده للجدول بعدد اكبر مستوى فى شجرة الحسابات
                AlterTableAll(ref opp, ref dt_AccountTempBalance);

                /////////////////////////////////////////////////
                ////////////جمع اجمالى الحسابات الفرعيه من  
                ////Final_madin
                ////ووضعها فى
                ///Final_madin
                ///للحساب الاب


                UpdateData_WithLevelNew(ref opp);



                /////////////////////////اعادة التجميع

                //for (int x = 0; x < dt_AccountTempBalance.Rows.Count; x++)
                //{
                //    string SubAccount_Id = dt_AccountTempBalance.Rows[x]["SubAccount_Id"].ToString();
                //    string SubAccount_Name = dt_AccountTempBalance.Rows[x]["SubAccount_Name"].ToString();

                //    if (SubAccount_Id != "")
                //    {
                //        UpdateData_WithLevel(int.Parse(SubAccount_Id), ref opp, ref dt_AccountTempBalance, x);
                //    }
                //}

                mysql = " Select * from  AccountTempBalance Where Account_Level <= " + Level_Required + " Order By Account_Level DESC ";
                dt_AccountTempBalance = opp.return_DataTable(mysql);
                return dt_AccountTempBalance;
                opp.Commit();
            }
            catch (Exception ee)
            {
                opp.Rollback();
                string ms = ee.Message;
                //MessageBox.Show(ms);
                return dt_AccountTempBalance;
            }

        }
        public DataTable GetBalanceData_WithLevel(long Branch_Id, long Year_Financial_Period_Id, int Level_Required, long Entry_Currency_Id)
        {
            DataTable dt_AccountTempBalance = new DataTable();
            AccountModelsLib.operations opp = new AccountModelsLib.operations( );
            string mysql = "";

            try
            {

                mysql = @" Truncate Table AccountTempBalance";
                opp.insert_me(mysql);

                ////////////////////حفظ كل ارصدة الحسابات ى الجدول الموؤقت
                mysql = @" INSERT INTO AccountTempBalance
                         (SubAccount_Name, SubAccount_Id, Main_Account_Id, IS_Last_Level, Account_Level, Balance_Madin, Balance_Da2n, Account_Currency_Id, Equivalent_Account_Currency_Id, Year_Financial_Period_Id, Branch_Id, SLevel)
                    ";
                mysql = mysql + @" 
SELECT DISTINCT 
                         Submain_Account.SubAccount_Name, Submain_Account.Id, Submain_Account.Main_Account_Id, Submain_Account.IS_Last_Level, Submain_Account.Account_Level, 
                         Account_Entry_Master_Balance_MezanTemp.Final_Last_Balance_Madin, Account_Entry_Master_Balance_MezanTemp.Final_Last_Balance_Da2n, Submain_Account.Currency_Id, 
                         Account_Entry_Master_Balance_MezanTemp.Equivalent_Account_Currency_Id, Account_Entry_Master_Balance_MezanTemp.Year_Financial_Period_Id, Account_Entry_Master_Balance_MezanTemp.Branch_Id, 
                         0 AS SLevel
FROM            Submain_Account INNER JOIN
                         Account_Entry_Master_Balance_MezanTemp ON Submain_Account.Id = Account_Entry_Master_Balance_MezanTemp.Submain_Account_ID

 ";
                mysql = mysql + @" WHERE  (Account_Entry_Master_Balance_MezanTemp.Year_Financial_Period_Id=" + Year_Financial_Period_Id + ")  And (Account_Entry_Master_Balance_MezanTemp.Branch_Id=" + Branch_Id + ") AND (Submain_Account.Account_Follows = 4)   ";
                if (Entry_Currency_Id > -1)
                {
                    mysql = mysql + "AND Account_Entry_Master_Balance_MezanTemp.Equivalent_Account_Currency_Id=" + Entry_Currency_Id + "";
                }
                int C = opp.insert_me(mysql);



                mysql = "Select Distinct  Equivalent_Account_Currency_Id From AccountTempBalance";
                DataTable dtt = opp.return_DataTable(mysql);

                for (int x = 0; x < dtt.Rows.Count; x++)
                {
                    long Equivalent_Account_Currency_Id = long.Parse(dtt.Rows[x]["Equivalent_Account_Currency_Id"].ToString());

                    for (int y = 0; y < 100; y++)
                    {
                        int SLevel = y;
                        int SSLevel = SLevel + 1;
                        mysql = @" INSERT INTO AccountTempBalance
                         (SubAccount_Name, SubAccount_Id, Main_Account_Id, IS_Last_Level, Account_Level, Balance_Madin, Balance_Da2n, Equivalent_Balance_Madin, Equivalent_Balance_Da2n, Account_Currency_Id, 
                         Equivalent_Account_Currency_Id, Year_Financial_Period_Id, Branch_Id, SLevel)

SELECT DISTINCT 
                         Submain_Account.SubAccount_Name, Submain_Account.Id, Submain_Account.Main_Account_Id, Submain_Account.IS_Last_Level, Submain_Account.Account_Level, SUM(AccountTempBalance_1.Balance_Madin) 
                         AS Balance_Madin, SUM(AccountTempBalance_1.Balance_Da2n) AS Balance_Da2n, SUM(AccountTempBalance_1.Equivalent_Balance_Madin) AS Equivalent_Balance_Madin, 
                         SUM(AccountTempBalance_1.Equivalent_Balance_Da2n) AS Equivalent_Balance_Da2n, Submain_Account.Currency_Id, AccountTempBalance_1.Equivalent_Account_Currency_Id, 
                         AccountTempBalance_1.Year_Financial_Period_Id, AccountTempBalance_1.Branch_Id, " + SSLevel + " AS SLevel ";
                        mysql = mysql + @" FROM            Submain_Account INNER JOIN
                         AccountTempBalance AS AccountTempBalance_1 ON Submain_Account.Id = AccountTempBalance_1.Main_Account_Id
GROUP BY Submain_Account.SubAccount_Name, Submain_Account.Id, Submain_Account.Main_Account_Id, Submain_Account.IS_Last_Level, Submain_Account.Account_Level, Submain_Account.Currency_Id, 
                         AccountTempBalance_1.Equivalent_Account_Currency_Id, AccountTempBalance_1.SLevel, AccountTempBalance_1.Year_Financial_Period_Id, AccountTempBalance_1.Branch_Id
 ";

                        mysql = mysql + @" HAVING (AccountTempBalance_1.Year_Financial_Period_Id = " + Year_Financial_Period_Id + ") AND(AccountTempBalance_1.Branch_Id = " + Branch_Id + ") AND(AccountTempBalance_1.Equivalent_Account_Currency_Id =" + Equivalent_Account_Currency_Id + ")";
                        mysql = mysql + @" And (AccountTempBalance_1.SLevel = " + SLevel + ") ";

                        int Count = opp.insert_me(mysql);
                        if (Count == 0)
                        {
                            break;
                        }
                    }
                }

                 
                /////////////////////////////////
                ///وضع عمود النهائى يساوى الرصيد لحين جمع الحسسابات الفرعيه ووضعها فى النهائى

                mysql = " Update AccountTempBalance Set Final_madin = Balance_Madin,Final_da2n = Balance_Da2n ,   Final_Equivalent_madin =  Equivalent_Balance_Madin , Final_Equivalent_da2n =Equivalent_Balance_Da2n ";
                opp.insert_me(mysql);

                /////////////////////////////////////////////////////////////////
                /////وضع حساب حقوق الملكيه متفرع من الخصوم  
                ///ووضع مستوى حقوق الملكيه مستوى ثان

                mysql = "Update AccountTempBalance Set Main_Account_Id=2 , Account_Level=2 Where SubAccount_Id =3";
                opp.insert_me(mysql);

                /////  اضافة حساب الربح والخساره متفرع من حقوق الملكيه
                Incoming_StatementCLS cc = new Incoming_StatementCLS();

                decimal Profit_Value = cc.Get_ProfitFrom_Incoming_Statement(Branch_Id, Year_Financial_Period_Id, 1, Entry_Currency_Id);
                int Profit_Account_Level = 5;
                string Profit_Account_Name = "صافى الربح";

                mysql = "INSERT INTO AccountTempBalance(SubAccount_Id, Main_Account_Id, SubAccount_Name, IS_Last_Level, Account_Level, Balance_Madin, Balance_Da2n, Equivalent_Balance_Madin, Equivalent_Balance_Da2n, Final_madin, Final_da2n, Final_Equivalent_madin, Final_Equivalent_da2n, Account_Currency_Id, Equivalent_Account_Currency_Id)";
                mysql = mysql + " VALUES  (0, 3, N'" + Profit_Account_Name + " ', 1," + Profit_Account_Level + " , 0, " + Profit_Value + ", 0, " + Profit_Value + ", 0,  " + Profit_Value + ", 0,  " + Profit_Value + ", " + Common.Entry_Currency_Id + ",  " + Common.Entry_Currency_Id + ")";
                opp.insert_me(mysql);

                //////////////////////////////////////////////////////
                ///Select to sum main account
                dt_AccountTempBalance = new DataTable();
                mysql = " Select * from  AccountTempBalance Order By Account_Level DESC ";
                dt_AccountTempBalance = opp.return_DataTable(mysql);

                //////////إضافة اعمده للجدول بعدد اكبر مستوى فى شجرة الحسابات
                AlterTableAll(ref opp, ref dt_AccountTempBalance);

                /////////////////////////////////////////////////
                ////////////جمع اجمالى الحسابات الفرعيه من  
                ////Final_madin
                ////ووضعها فى
                ///Final_madin
                ///للحساب الاب


                UpdateData_WithLevelNew(ref opp);



                /////////////////////////اعادة التجميع

                //for (int x = 0; x < dt_AccountTempBalance.Rows.Count; x++)
                //{
                //    string SubAccount_Id = dt_AccountTempBalance.Rows[x]["SubAccount_Id"].ToString();
                //    string SubAccount_Name = dt_AccountTempBalance.Rows[x]["SubAccount_Name"].ToString();

                //    if (SubAccount_Id != "")
                //    {
                //        UpdateData_WithLevel(int.Parse(SubAccount_Id), ref opp, ref dt_AccountTempBalance, x);
                //    }
                //}

                mysql = " Select * from  AccountTempBalance Where Account_Level <= " + Level_Required + " Order By Account_Level DESC ";
                dt_AccountTempBalance = opp.return_DataTable(mysql);
                return dt_AccountTempBalance;
                opp.Commit();
            }
            catch (Exception ee)
            {
                opp.Rollback();
                string ms = ee.Message;
                //MessageBox.Show(ms);
                return dt_AccountTempBalance;
            }

        }

        void AlterTableAll(ref AccountModelsLib.operations opp, ref DataTable dt_AccountTempBalance)
        {
            string mysql = "";

            ///////////////////////////////  البحث عن اخر مستوى فى شجرة الحسابات
            ///


            int Max_Account_Level = 0;
            mysql = " SELECT   MAX(Account_Level) AS Account_Level FROM    Submain_Account ";
            DataTable dt_MAX = opp.return_DataTable(mysql);
            if (dt_MAX.Rows.Count > 0)
            {
                try
                {
                    Max_Account_Level = int.Parse(dt_MAX.Rows[0]["Account_Level"].ToString());


                }
                catch (Exception exc) { }
            }


            string ColumnName = "";

            ColumnName = "Final_Balance_Value";

            mysql = " Alter Table AccountTempBalance Add " + ColumnName + " decimal(18, 2) ";
            try
            {
                opp.insert_me(mysql);
            }
            catch { }


            for (int z = 1; z <= Max_Account_Level; z++)
            {


                ColumnName = "Final_madin_" + z;


                mysql = " Alter Table AccountTempBalance Add " + ColumnName + " decimal(18, 2) ";
                try
                {
                    opp.insert_me(mysql);
                }
                catch { }

                ///////////////////////////////////////////////
                ColumnName = "Final_da2n_" + z;


                mysql = " Alter Table AccountTempBalance Add " + ColumnName + " decimal(18, 2) ";
                try
                {
                    opp.insert_me(mysql);
                }
                catch { }
                ///////////////////////////////////////////////
                ColumnName = "Final_Equivalent_madin_" + z;


                mysql = " Alter Table AccountTempBalance Add " + ColumnName + " decimal(18, 2) ";

                try
                {
                    opp.insert_me(mysql);
                }
                catch { }

                ///////////////////////////////////////////////
                ColumnName = "Final_Equivalent_da2n_" + z;


                mysql = " Alter Table AccountTempBalance Add " + ColumnName + " decimal(18, 2) ";
                try
                {
                    opp.insert_me(mysql);
                }
                catch { }
                ColumnName = "SubAccount_Name_" + z;

                mysql = " Alter Table AccountTempBalance Add " + ColumnName + " nvarchar(90)";
                try
                {
                    opp.insert_me(mysql);
                }
                catch { }

            }



        }

        void UpdateData_WithLevel(long SubAccount_Id, ref AccountModelsLib.operations opp, ref DataTable dt_AccountTempBalance, int Index)
        {
            int Account_Level = int.Parse(dt_AccountTempBalance.Rows[Index]["Account_Level"].ToString());
            string SubAccount_Name = dt_AccountTempBalance.Rows[Index]["SubAccount_Name"].ToString();
            string mysql = @"  SELECT        SUM(Final_madin) AS Final_madin, SUM(Final_da2n) AS Final_da2n, SUM(Final_Equivalent_madin) AS Final_Equivalent_madin, 
                         SUM(Final_Equivalent_da2n) AS Final_Equivalent_da2n,  COUNT(SubAccount_Id) AS Sub_Count

FROM            AccountTempBalance ";

            mysql = mysql + "  WHERE           (Main_Account_Id = " + SubAccount_Id + ")";
            mysql = mysql + "   AND     (Balance_Madin IS NOT NULL) AND (Balance_Da2n IS NOT NULL) ";


            DataTable dt = opp.return_DataTable(mysql);



            decimal Balance_Madin = 0;
            decimal Balance_Da2n = 0;
            decimal Equivalent_Balance_Madin = 0;
            decimal Equivalent_Balance_Da2n = 0;
            long Sub_Count = 0;
            if (dt.Rows.Count > 0)
            {
                try
                {
                    Sub_Count = long.Parse(dt.Rows[0]["Sub_Count"].ToString());
                }
                catch { }
                if (Sub_Count > 0)
                {

                    try
                    {
                        //Balance_Madin = decimal.Parse(dt.Rows[0]["Balance_Madin"].ToString());
                        Balance_Madin = decimal.Parse(dt.Rows[0]["Final_madin"].ToString());
                    }
                    catch { }
                    try
                    {
                        //Balance_Da2n = decimal.Parse(dt.Rows[0]["Balance_Da2n"].ToString());
                        Balance_Da2n = decimal.Parse(dt.Rows[0]["Final_da2n"].ToString());
                    }
                    catch { }
                    try
                    {
                        //Equivalent_Balance_Madin = decimal.Parse(dt.Rows[0]["Equivalent_Balance_Madin"].ToString());
                        Equivalent_Balance_Madin = decimal.Parse(dt.Rows[0]["Final_Equivalent_madin"].ToString());
                    }
                    catch { }
                    try
                    {
                        //Equivalent_Balance_Da2n = decimal.Parse(dt.Rows[0]["Equivalent_Balance_Da2n"].ToString());
                        Equivalent_Balance_Da2n = decimal.Parse(dt.Rows[0]["Final_Equivalent_da2n"].ToString());
                    }
                    catch { }

                    decimal Final_Balance_Value = Balance_Madin + Balance_Da2n;

                    dt_AccountTempBalance.Rows[Index]["Final_madin"] = Balance_Madin;
                    dt_AccountTempBalance.Rows[Index]["Final_da2n"] = Balance_Da2n;
                    dt_AccountTempBalance.Rows[Index]["Final_Equivalent_madin"] = Equivalent_Balance_Madin;
                    dt_AccountTempBalance.Rows[Index]["Final_Equivalent_da2n"] = Equivalent_Balance_Da2n;
                    dt_AccountTempBalance.Rows[Index]["Final_Balance_Value"] = Final_Balance_Value;

                    try
                    {
                        dt_AccountTempBalance.Rows[Index]["SubAccount_Name" + "_" + Account_Level] = SubAccount_Name;


                        dt_AccountTempBalance.Rows[Index]["Final_madin" + "_" + Account_Level] = Balance_Madin;
                        dt_AccountTempBalance.Rows[Index]["Final_da2n" + "_" + Account_Level] = Balance_Da2n;
                        dt_AccountTempBalance.Rows[Index]["Final_Equivalent_madin" + "_" + Account_Level] = Equivalent_Balance_Madin;
                        dt_AccountTempBalance.Rows[Index]["Final_Equivalent_da2n" + "_" + Account_Level] = Equivalent_Balance_Da2n;


                    }
                    catch (Exception ee) { }

                    if (SubAccount_Id == 1 | SubAccount_Id == 133 | SubAccount_Id == 10 | SubAccount_Id == 2059 | SubAccount_Id == 2064 | SubAccount_Id == 40)
                    {
                        int x = 0;
                    }
                    if (SubAccount_Id == 3038 | SubAccount_Id == 2044 | SubAccount_Id == 93 | SubAccount_Id == 2)
                    {
                        int x = 0;
                    }
                    if (SubAccount_Id == 11 | SubAccount_Id == 2048 | SubAccount_Id == 2017 | SubAccount_Id == 2 | SubAccount_Id == 2022)
                    {
                        int x = 0;
                    }


                    try
                    {
                        mysql = " Update AccountTempBalance Set ";
                        mysql = mysql + "  SubAccount_Name" + "_" + Account_Level + "='" + SubAccount_Name + "'";
                        mysql = mysql + " , Final_madin" + "_" + Account_Level + "=" + Balance_Madin;
                        mysql = mysql + " , Final_da2n" + "_" + Account_Level + "=" + Balance_Da2n;
                        mysql = mysql + " , Final_Equivalent_madin" + "_" + Account_Level + "=" + Equivalent_Balance_Madin;
                        mysql = mysql + " , Final_Equivalent_da2n" + "_" + Account_Level + "=" + Equivalent_Balance_Da2n;

                        mysql = mysql + " , Final_madin =" + Balance_Madin;
                        mysql = mysql + " , Final_da2n =" + Balance_Da2n;
                        mysql = mysql + " , Final_Equivalent_madin =" + Equivalent_Balance_Madin;
                        mysql = mysql + " , Final_Equivalent_da2n =" + Equivalent_Balance_Da2n;
                        mysql = mysql + " , Final_Balance_Value =" + Final_Balance_Value;

                        mysql = mysql + " Where SubAccount_Id=" + SubAccount_Id;

                        opp.insert_me(mysql);

                    }
                    catch (Exception ee) { }

                }
            }
        }

        void UpdateData_WithLevelNew(ref AccountModelsLib.operations opp)
        {

            string mysql = "";
            mysql = "Select Distinct  Account_Level From AccountTempBalance";
            DataTable dtt = opp.return_DataTable(mysql);

            for (int x = 0; x < dtt.Rows.Count; x++)
            {

                int Account_Level = int.Parse(dtt.Rows[x]["Account_Level"].ToString());
                try
                {
                    //mysql = " Update AccountTempBalance Set ";
                    //mysql = mysql + "  SubAccount_Name" + "_" + Account_Level + "=  SubAccount_Name  ";
                    //mysql = mysql + " , Final_madin" + "_" + Account_Level + "=Final_madin ";
                    //mysql = mysql + " , Final_da2n" + "_" + Account_Level + "=Final_da2n ";
                    //mysql = mysql + " , Final_Equivalent_madin" + "_" + Account_Level + "=Final_Equivalent_madin";
                    //mysql = mysql + " , Final_Equivalent_da2n" + "_" + Account_Level + "=Final_Equivalent_da2n";

                    //mysql = mysql + " , Final_Balance_Value =Final_madin +Final_da2n ";



                    ////////////////
                    ///

                    mysql = " Update AccountTempBalance Set ";
                    mysql = mysql + "  SubAccount_Name" + "_" + Account_Level + "=SubAccount_Name";
                    mysql = mysql + " , Final_madin" + "_" + Account_Level + "=Balance_Madin";
                    mysql = mysql + " , Final_da2n" + "_" + Account_Level + "=Balance_Da2n";
                    mysql = mysql + " , Final_Equivalent_madin" + "_" + Account_Level + "=  Equivalent_Balance_Madin";
                    mysql = mysql + " , Final_Equivalent_da2n" + "_" + Account_Level + "=   Equivalent_Balance_Da2n";

                    mysql = mysql + " , Final_Balance_Value =Balance_Madin +Balance_Da2n ";

                    opp.insert_me(mysql);

                    mysql = " Update AccountTempBalance Set ";
                   
                    mysql = mysql + "   Final_Balance_Value =Final_Balance_Value * -1 Where  Final_Balance_Value < 0";

                    opp.insert_me(mysql);


                }
                catch (Exception ee) { }

            }

        }

    }
}
