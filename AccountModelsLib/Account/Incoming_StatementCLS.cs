
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
    public class Incoming_StatementCLS
    {
        public Incoming_StatementCLS()
        {
        }
        public decimal Get_ProfitFrom_Incoming_Statement(long Branch_Id, long Year_Financial_Period_Id, int Show_AccountWithBalance, long Entry_Currency_Id)
        {
            DataTable dt = new DataTable();
            decimal Profit_value = Get_Incoming_Statement(ref dt, Branch_Id, Year_Financial_Period_Id, Show_AccountWithBalance, Entry_Currency_Id);
            return Profit_value;
        }
        public decimal Get_Incoming_Statement(ref DataTable dt,long Branch_Id, long Year_Financial_Period_Id, int Show_AccountWithBalance, long Entry_Currency_Id)
        {
            DataTable dtPrint = new DataTable();
            return Get_Incoming_Statement(ref dt,ref   dtPrint, Branch_Id, Year_Financial_Period_Id, Show_AccountWithBalance, Entry_Currency_Id);


        }
        public decimal Get_Incoming_Statement(ref DataTable dt,ref DataTable dtPrint, long Branch_Id, long Year_Financial_Period_Id, int Show_AccountWithBalance, long Entry_Currency_Id)
        {
            string mysql = "";


            mysql = @"SELECT  * FROM  Account_Entry_Master_Balance_MezanTemp ";

            mysql = mysql + @" WHERE     (Account_Follows=1 OR Account_Follows=2 OR Account_Follows=3) AND   (Year_Financial_Period_Id = " + Year_Financial_Period_Id + ") AND (Equivalent_Account_Currency_Id =" + Entry_Currency_Id + ") ";

            if (Branch_Id > 0)
            {

                mysql = mysql + @"  AND  (Branch_Id = " + Branch_Id + ")";
            }
            //if (Show_AccountWithBalance == 1)   
            //{

            //    mysql = mysql + @" And    (First_Balance + Final_Export_Value + Final_Import_Value <> 0)";


            //}
            if (Show_AccountWithBalance == 1)    /// لة رصيد
            {
                mysql = mysql + @" And    (Final_Last_Balance_Madin + Final_Last_Balance_Da2n   <> 0)";

            }
            if (Show_AccountWithBalance == 2)    //لة حركات بدون رصيد
            {
                mysql = mysql + @" And    (Final_Export_Value + Final_Import_Value <> 0)";

            }
            //mysql = mysql + @" ORDER BY MainCode, Code ";
            //mysql = mysql + @"  ORDER BY    AccountCode1,AccountCode2,AccountCode3,AccountCode4,AccountCode5,AccountCode6  ";
            mysql = mysql + @"  ORDER BY Head_Main_Account,Main_Account_Id,  AccountCode1,AccountCode2,AccountCode3,AccountCode4,AccountCode5,AccountCode6  ";




            ////////////////////////////////////


            dt = new DataTable();
            operations opp = new operations();
            dt = opp.return_DataTable(mysql);

            dtPrint = dt.Copy();
            decimal Final_Last_Balance_Madin = 0;
            decimal Final_Last_Balance_Da2n = 0;

            SetParentTotal(ref dt, ref Final_Last_Balance_Madin, ref Final_Last_Balance_Da2n);
            //for (int x = 0; x < dt.Rows.Count; x++)
            //{
            //    try
            //    {
            //        decimal Balance_Madin = decimal.Parse(dt.Rows[x]["Final_Last_Balance_Madin"].ToString());
            //        Balance_Madin = Common.RoundMeFormat(Balance_Madin);
            //        dt.Rows[x]["Final_Last_Balance_Madin"] = Balance_Madin.ToString();

            //        Final_Last_Balance_Madin = Final_Last_Balance_Madin + Balance_Madin;
            //    }
            //    catch { }

            //    try
            //    {
            //        decimal Balance_Da2n = decimal.Parse(dt.Rows[x]["Final_Last_Balance_Da2n"].ToString());
            //        Balance_Da2n = Common.RoundMeFormat(Balance_Da2n);
            //        dt.Rows[x]["Final_Last_Balance_Da2n"] = Balance_Da2n.ToString();

            //        Final_Last_Balance_Da2n = Final_Last_Balance_Da2n + Balance_Da2n;
            //    }
            //    catch { }
            //}
            /////حساب صافى الرح
            /////طرح اجمالى الدائن الايرادات ناقص اجمالى المدين المصروفات
            ///اقيمه بالسالب تعنى خساره
            decimal Final_Profit = Final_Last_Balance_Da2n - Final_Last_Balance_Madin;


            DataRow dr = dt.NewRow();
            dt.Rows.Add(dr);

            ///////////////////////إجمالى الإيرادات
            dr = dt.NewRow();
            dr["Account_Name"] = "إجمالى الإيرادات";
            dr["Final_Last_Balance_Da2n"] = Final_Last_Balance_Da2n;
            dr["Submain_Account_ID"] = -10;
            dt.Rows.Add(dr);

            ///////////////////////إجمالى المصروفات
            dr = dt.NewRow();
            dr["Account_Name"] = "إجمالى المصروفات";
            dr["Final_Last_Balance_Madin"] = Final_Last_Balance_Madin;
            dr["Submain_Account_ID"] = -10;
            dt.Rows.Add(dr);


            ///////////////////////صافى الربح

            dr = dt.NewRow();
            string Final_Profit_Str = "";
            decimal Final_Totals = 0;
            if (Final_Profit > 0)
            {
                Final_Profit_Str = "صافى الربح";
                Final_Totals = Final_Profit;
                dr["Final_Last_Balance_Madin"] = Final_Totals;
            }
            else
            {
                Final_Profit_Str = "صافى الخساره";
                Final_Totals = Final_Profit * -1;
                dr["Final_Last_Balance_Da2n"] = Final_Totals;
            }

            dr["Account_Name"] = Final_Profit_Str;
            dr["Submain_Account_ID"] = -10;
            dt.Rows.Add(dr);

            return Final_Profit;
        }

        public decimal Get_Incoming_StatementBackup(ref DataTable dt, long Branch_Id, long Year_Financial_Period_Id)
        {
            string mysql = "";
            mysql = @"SELECT        Account_Entry_Master_Balance.Submain_Account_ID, Account_Entry_Master_Balance.SubAccount_Name, Account_Entry_Master_Balance.Account_Currency_Id, 
                         Account_Entry_Master_Balance.Equivalent_Account_Currency_Id, Account_Entry_Master_Balance.Balance_Madin, Account_Entry_Master_Balance.Balance_Da2n, 
                         Account_Entry_Master_Balance.Balance_Madin * Currency_Factor.Factor AS Final_Last_Balance_Madin, 
                         Account_Entry_Master_Balance.Balance_Da2n * Currency_Factor.Factor AS Final_Last_Balance_Da2n, Account_Entry_Master_Balance.Year_Financial_Period_Id, 
                         Account_Entry_Master_Balance.Branch_Id, '' AS Totals, Submain_Account.Account_Code

FROM            Account_Entry_Master_Balance INNER JOIN
                         Currency_Factor ON Account_Entry_Master_Balance.Account_Currency_Id = Currency_Factor.Currency_ID_Primary AND 
                         Account_Entry_Master_Balance.Equivalent_Account_Currency_Id = Currency_Factor.Currency_ID_Secondary INNER JOIN
                         Submain_Account ON Account_Entry_Master_Balance.Submain_Account_ID = Submain_Account.Id
WHERE       ( (Submain_Account.Head_Main_Account = 4) OR (Submain_Account.Head_Main_Account = 5))

";


            if (Branch_Id > 0)
            {
                mysql = mysql + " AND    (Account_Entry_Master_Balance.Branch_Id  = " + Branch_Id + ") ";
            }

            mysql = mysql + " AND (Account_Entry_Master_Balance.Year_Financial_Period_Id = " + Year_Financial_Period_Id + ") ";

            mysql = mysql + "ORDER BY Account_Entry_Master_Balance.Balance_Madin ";

            dt = new DataTable();
            operations opp = new operations();
            dt = opp.return_DataTable(mysql);

            decimal Final_Last_Balance_Madin = 0;
            decimal Final_Last_Balance_Da2n = 0;
            for (int x = 0; x < dt.Rows.Count; x++)
            {
                try
                {
                    decimal Balance_Madin = decimal.Parse(dt.Rows[x]["Final_Last_Balance_Madin"].ToString());
                    Balance_Madin = Common.RoundMe(Balance_Madin);
                    dt.Rows[x]["Final_Last_Balance_Madin"] = Balance_Madin.ToString();

                    Final_Last_Balance_Madin = Final_Last_Balance_Madin + Balance_Madin;
                }
                catch { }

                try
                {
                    decimal Balance_Da2n = decimal.Parse(dt.Rows[x]["Final_Last_Balance_Da2n"].ToString());
                    Balance_Da2n = Common.RoundMe(Balance_Da2n);
                    dt.Rows[x]["Final_Last_Balance_Da2n"] = Balance_Da2n.ToString();

                    Final_Last_Balance_Da2n = Final_Last_Balance_Da2n + Balance_Da2n;
                }
                catch { }
            }
            /////حساب صافى الرح
            /////طرح اجمالى الدائن الايرادات ناقص اجمالى المدين المصروفات
            ///اقيمه بالسالب تعنى خساره
            decimal Final_Profit = Final_Last_Balance_Da2n - Final_Last_Balance_Madin;


            DataRow dr = dt.NewRow();
            dt.Rows.Add(dr);

            ///////////////////////إجمالى الإيرادات
            dr = dt.NewRow();
            dr["SubAccount_Name"] = "إجمالى الإيرادات";
            dr["Totals"] = Final_Last_Balance_Da2n;

            dt.Rows.Add(dr);

            ///////////////////////إجمالى المصروفات
            dr = dt.NewRow();
            dr["SubAccount_Name"] = "إجمالى المصروفات";
            dr["Totals"] = Final_Last_Balance_Madin;

            dt.Rows.Add(dr);


            ///////////////////////صافى الربح

            dr = dt.NewRow();
            string Final_Profit_Str = "";
            decimal Final_Totals = 0;
            if (Final_Profit > 0)
            {
                Final_Profit_Str = "صافى الربح";
                Final_Totals = Final_Profit;

            }
            else
            {
                Final_Profit_Str = "صافى الخساره";
                Final_Totals = Final_Profit * -1;
            }
            dr["Totals"] = Final_Totals;
            dr["SubAccount_Name"] = Final_Profit_Str;
            dt.Rows.Add(dr);

            return Final_Profit;
        }
        public void Get_Final_Accounts(ref DataTable dt, int Accounts_Follow, long Branch_Id, long Year_Financial_Period_Id, long    Entry_Currency_Id, int Show_AccountWithBalance)
        {
            string mysql = "";

            mysql = @"SELECT  * FROM  Account_Entry_Master_Balance_MezanTemp ";

            mysql = mysql + @" WHERE        (Year_Financial_Period_Id = " + Year_Financial_Period_Id + ") AND (Equivalent_Account_Currency_Id =" + Entry_Currency_Id + ") ";
            mysql = mysql + " AND (Account_Follows= " + Accounts_Follow + ") ";
            if (Branch_Id > 0)
            {

                mysql = mysql + @"  AND  (Branch_Id = " + Branch_Id + ")";
            }
            //if (Show_AccountWithBalance == 1)   
            //{

            //    mysql = mysql + @" And    (First_Balance + Final_Export_Value + Final_Import_Value <> 0)";


            //}
            if (Show_AccountWithBalance == 1)    /// لة رصيد
            {
                mysql = mysql + @" And    (Final_Last_Balance_Madin + Final_Last_Balance_Da2n   <> 0)";

            }
            if (Show_AccountWithBalance == 2)    //لة حركات بدون رصيد
            {
                mysql = mysql + @" And    (Final_Export_Value + Final_Import_Value <> 0)";

            }
            //mysql = mysql + @" ORDER BY MainCode, Code ";
            mysql = mysql + @"  ORDER BY  AccountCode1,AccountCode2,AccountCode3,AccountCode4,AccountCode5,AccountCode6  ";


            /////////////////////////////////////////////


            dt = new DataTable();
            operations opp = new operations();
            dt = opp.return_DataTable(mysql);
            //Compute_Mora7l_Tash3heelValues(ref   dt, Accounts_Follow, ref   Mora7l_Madin, ref   Mora7l_Da7n);




        }

        public void Get_Final_AccountsBackup(ref DataTable dt, int Accounts_Follow, long Branch_Id, long Year_Financial_Period_Id)
        {
            string mysql = "";
            mysql = @"SELECT  Account_Entry_Master_Balance.Submain_Account_ID, Account_Entry_Master_Balance.SubAccount_Name, Account_Entry_Master_Balance.Account_Currency_Id, 
                         Account_Entry_Master_Balance.Equivalent_Account_Currency_Id, Account_Entry_Master_Balance.Balance_Madin, Account_Entry_Master_Balance.Balance_Da2n, 
                         Account_Entry_Master_Balance.Balance_Madin * Currency_Factor.Factor AS Final_Last_Balance_Madin, 
                         Account_Entry_Master_Balance.Balance_Da2n * Currency_Factor.Factor AS Final_Last_Balance_Da2n, Account_Entry_Master_Balance.Year_Financial_Period_Id, 
                         Account_Entry_Master_Balance.Branch_Id, '' AS Totals, Submain_Account.Account_Code

FROM            Account_Entry_Master_Balance INNER JOIN
                         Currency_Factor ON Account_Entry_Master_Balance.Account_Currency_Id = Currency_Factor.Currency_ID_Primary AND 
                         Account_Entry_Master_Balance.Equivalent_Account_Currency_Id = Currency_Factor.Currency_ID_Secondary INNER JOIN
                         Submain_Account ON Account_Entry_Master_Balance.Submain_Account_ID = Submain_Account.Id
WHERE       ( (Submain_Account.Head_Main_Account = 4) OR (Submain_Account.Head_Main_Account = 5))

";


            if (Branch_Id > 0)
            {
                mysql = mysql + " AND    (Account_Entry_Master_Balance.Branch_Id  = " + Branch_Id + ") ";
            }
            mysql = mysql + " AND (Submain_Account.Account_Follows= " + Accounts_Follow + ") ";
            mysql = mysql + " AND (Account_Entry_Master_Balance.Year_Financial_Period_Id = " + Year_Financial_Period_Id + ") ";

            mysql = mysql + "ORDER BY Account_Entry_Master_Balance.Balance_Madin ";

            dt = new DataTable();
            operations opp = new operations();
            dt = opp.return_DataTable(mysql);
            //Compute_Mora7l_Tash3heelValues(ref   dt, Accounts_Follow, ref   Mora7l_Madin, ref   Mora7l_Da7n);




        }

        void SetParentTotal(ref DataTable dt, ref decimal Total_Final_Last_Balance_Madin, ref decimal Total_Final_Last_Balance_Da2n)
        {
            decimal Total_First_Balance_Madin = 0;
            decimal Total_First_Balance_Da2n = 0;

            decimal Total_Final_Import_Value = 0;
            decimal Total_Final_Export_Value = 0;
            Total_Final_Last_Balance_Madin = 0;
            Total_Final_Last_Balance_Da2n = 0;

            decimal Total_Final_Transaction_Madin = 0;
            decimal Total_Final_Transaction_Da2n = 0;
            int Count = 0;


            //////////////////////////////////////////
            decimal SubFinal_Fist_Balance_Madin = 0;
            decimal SubFinal_Fist_Balance_Da2n = 0;
            decimal SubFinal_Import_Value = 0;
            decimal SubFinal_Export_Value = 0;
            decimal SubFinal_Last_Balance_Madin = 0;
            decimal SubFinal_Last_Balance_Da2n = 0;
            decimal SubFinal_Transaction_Madin = 0;
            decimal SubFinal_Transaction_Da2n = 0;

            string MainCode = "MainCode";
            string MainCode_1 = "";
            bool IsFirstRead = true;



            string Astrix = "";
            string FirstMainAccount_Name_1 = "";
            string FirstMainCode_1 = "";

            string TTMainAccount_Name_1 = "";
            string MainAccount_Name = "";
            string Account_Code = "";

            decimal Diff_Final_Fist_Balance_Madin = 0;
            decimal Diff_SubFinal_Import_Value = 0;
            decimal Diff_SubFinal_Last_Balance_Madin = 0;
            decimal Diff_SubFinal_Transaction_Madin = 0;


            for (int x = 0; x < dt.Rows.Count; x++)
            {

                Total_First_Balance_Madin = Total_First_Balance_Madin + decimal.Parse(dt.Rows[x]["Final_Fist_Balance_Madin"].ToString());

                Total_First_Balance_Da2n = Total_First_Balance_Da2n + decimal.Parse(dt.Rows[x]["Final_Fist_Balance_Da2n"].ToString());


                Total_Final_Import_Value = Total_Final_Import_Value + decimal.Parse(dt.Rows[x]["Final_Import_Value"].ToString());
                Total_Final_Export_Value = Total_Final_Export_Value + decimal.Parse(dt.Rows[x]["Final_Export_Value"].ToString());

                Total_Final_Last_Balance_Madin = Total_Final_Last_Balance_Madin + decimal.Parse(dt.Rows[x]["Final_Last_Balance_Madin"].ToString());
                Total_Final_Last_Balance_Da2n = Total_Final_Last_Balance_Da2n + decimal.Parse(dt.Rows[x]["Final_Last_Balance_Da2n"].ToString());

                Total_Final_Transaction_Madin = Total_Final_Transaction_Madin + decimal.Parse(dt.Rows[x]["Final_Transaction_Madin"].ToString());
                Total_Final_Transaction_Da2n = Total_Final_Transaction_Da2n + decimal.Parse(dt.Rows[x]["Final_Transaction_Da2n"].ToString());

                ///  10103010001
                MainCode = dt.Rows[x]["MainCode"].ToString();
                if (MainCode == "10103010000")
                {
                    string fff = "";
                }
                if (x == 0)
                {
                    MainCode = dt.Rows[0]["MainCode"].ToString();
                    MainCode_1 = dt.Rows[0]["MainCode"].ToString();
                    TTMainAccount_Name_1 = dt.Rows[0]["MainAccount_Name"].ToString();

                    FirstMainAccount_Name_1 = TTMainAccount_Name_1;
                    FirstMainCode_1 = MainCode_1;

                    DataRow dr11 = dt.NewRow();

                    dr11["Account_Name"] = Astrix + FirstMainAccount_Name_1 + Astrix;
                    dr11["Account_Code"] = MainCode;
                    dr11["MainCode"] = "0";
                    dr11["Submain_Account_ID"] = -20;
                    dt.Rows.InsertAt(dr11, x);
                    x++;
                }
                //else if (IsFirstRead == true)
                //{
                //    MainCode = dt.Rows[x]["MainCode"].ToString();
                //    MainCode_1 = dt.Rows[x]["MainCode"].ToString();
                //    TTMainAccount_Name_1 = dt.Rows[x]["MainAccount_Name"].ToString();
                //}
                else
                {
                    MainCode = dt.Rows[x]["MainCode"].ToString();
                    MainCode_1 = dt.Rows[x - 1]["MainCode"].ToString();
                    TTMainAccount_Name_1 = dt.Rows[x - 1]["MainAccount_Name"].ToString();

                }
                MainAccount_Name = dt.Rows[x]["MainAccount_Name"].ToString();
                Account_Code = dt.Rows[x]["Account_Code"].ToString();
                if (MainCode == MainCode_1)
                {
                    SubFinal_Fist_Balance_Madin = SubFinal_Fist_Balance_Madin + decimal.Parse(dt.Rows[x]["Final_Fist_Balance_Madin"].ToString());
                    SubFinal_Fist_Balance_Da2n = SubFinal_Fist_Balance_Da2n + decimal.Parse(dt.Rows[x]["Final_Fist_Balance_Da2n"].ToString());
                    SubFinal_Import_Value = SubFinal_Import_Value + decimal.Parse(dt.Rows[x]["Final_Import_Value"].ToString());
                    SubFinal_Export_Value = SubFinal_Export_Value + decimal.Parse(dt.Rows[x]["Final_Export_Value"].ToString());
                    SubFinal_Last_Balance_Madin = SubFinal_Last_Balance_Madin + decimal.Parse(dt.Rows[x]["Final_Last_Balance_Madin"].ToString());
                    SubFinal_Last_Balance_Da2n = SubFinal_Last_Balance_Da2n + decimal.Parse(dt.Rows[x]["Final_Last_Balance_Da2n"].ToString());

                    SubFinal_Transaction_Madin = SubFinal_Transaction_Madin + decimal.Parse(dt.Rows[x]["Final_Transaction_Madin"].ToString());
                    SubFinal_Transaction_Da2n = SubFinal_Transaction_Da2n + decimal.Parse(dt.Rows[x]["Final_Transaction_Da2n"].ToString());

                    IsFirstRead = false;
                }
                else
                {


                    Diff_Final_Fist_Balance_Madin = SubFinal_Fist_Balance_Madin - SubFinal_Fist_Balance_Da2n;
                    Diff_SubFinal_Import_Value = SubFinal_Import_Value - SubFinal_Export_Value;
                    Diff_SubFinal_Last_Balance_Madin = SubFinal_Last_Balance_Madin - SubFinal_Last_Balance_Da2n;
                    Diff_SubFinal_Transaction_Madin = SubFinal_Transaction_Madin - SubFinal_Transaction_Da2n;
                    try
                    {
                        SubFinal_Fist_Balance_Madin = decimal.Parse(dt.Rows[x]["Final_Fist_Balance_Madin"].ToString());
                    }
                    catch { SubFinal_Fist_Balance_Madin = 0; }
                    try
                    {
                        SubFinal_Fist_Balance_Da2n = decimal.Parse(dt.Rows[x]["Final_Fist_Balance_Da2n"].ToString());
                    }
                    catch { SubFinal_Fist_Balance_Da2n = 0; }
                    try
                    {
                        SubFinal_Import_Value = decimal.Parse(dt.Rows[x]["Final_Import_Value"].ToString());
                    }
                    catch
                    {
                        SubFinal_Import_Value = 0;
                    }
                    try
                    {
                        SubFinal_Export_Value = decimal.Parse(dt.Rows[x]["Final_Export_Value"].ToString());
                    }
                    catch
                    {
                        SubFinal_Export_Value = 0;
                    }
                    try
                    {
                        SubFinal_Last_Balance_Madin = decimal.Parse(dt.Rows[x]["Final_Last_Balance_Madin"].ToString());
                    }
                    catch
                    {
                        SubFinal_Last_Balance_Madin = 0;
                    }
                    try
                    {
                        SubFinal_Last_Balance_Da2n = decimal.Parse(dt.Rows[x]["Final_Last_Balance_Da2n"].ToString());
                    }
                    catch
                    {
                        SubFinal_Last_Balance_Da2n = 0;
                    }
                    //////////
                    try
                    {
                        SubFinal_Transaction_Madin = decimal.Parse(dt.Rows[x]["Final_Transaction_Madin"].ToString());
                    }
                    catch
                    {
                        SubFinal_Transaction_Madin = 0;
                    }
                    try
                    {
                        SubFinal_Transaction_Da2n = decimal.Parse(dt.Rows[x]["Final_Transaction_Da2n"].ToString());
                    }
                    catch
                    {
                        SubFinal_Transaction_Da2n = 0;
                    }
                    DataRow dr2 = dt.NewRow();
                    dr2["MainAccount_Name"] = " إجمالى " + dt.Rows[x]["MainCode"].ToString();
                    dr2["Account_Name"] = " إجمالى " + TTMainAccount_Name_1;
                    dr2["Submain_Account_ID"] = -10;
                    if (Diff_Final_Fist_Balance_Madin > 0)
                    {
                        dr2["Final_Fist_Balance_Madin"] = Diff_Final_Fist_Balance_Madin.ToString();
                    }
                    else
                    {
                        Diff_Final_Fist_Balance_Madin = Diff_Final_Fist_Balance_Madin * -1;
                        dr2["Final_Fist_Balance_Da2n"] = Diff_Final_Fist_Balance_Madin.ToString();
                    }

                    ////////////////////////////////////
                    if (Diff_SubFinal_Import_Value > 0)
                    {
                        dr2["Final_Import_Value"] = Diff_SubFinal_Import_Value.ToString();
                    }
                    else
                    {
                        Diff_SubFinal_Import_Value = Diff_SubFinal_Import_Value * -1;
                        dr2["Final_Export_Value"] = Diff_SubFinal_Import_Value.ToString();
                    }

                    ////////////////////////////////////

                    if (Diff_SubFinal_Last_Balance_Madin > 0)
                    {
                        dr2["Final_Last_Balance_Madin"] = Diff_SubFinal_Last_Balance_Madin.ToString();
                    }
                    else
                    {
                        Diff_SubFinal_Last_Balance_Madin = Diff_SubFinal_Last_Balance_Madin * -1;
                        dr2["Final_Last_Balance_Da2n"] = Diff_SubFinal_Last_Balance_Madin.ToString();
                    }

                    if (Diff_SubFinal_Transaction_Madin > 0)
                    {
                        dr2["Final_Transaction_Madin"] = Diff_SubFinal_Transaction_Madin.ToString();
                    }
                    else
                    {
                        Diff_SubFinal_Transaction_Madin = Diff_SubFinal_Transaction_Madin * -1;
                        dr2["Final_Transaction_Da2n"] = Diff_SubFinal_Transaction_Madin.ToString();
                    }

                    dt.Rows.InsertAt(dr2, x);
                    IsFirstRead = true;


                    x++;

                    DataRow dr22 = dt.NewRow();

                    dr22["Account_Name"] = Astrix + MainAccount_Name + Astrix;
                    dr22["Account_Code"] = MainCode;
                    dr22["MainCode"] = "0";
                    dr22["Submain_Account_ID"] = -20;
                    dt.Rows.InsertAt(dr22, x);

                    x++;


                }

            }



        }
        public void Get_Final_AccountsTash3heel(ref DataTable dt_Tash3heel, int Accounts_Follow, ref decimal Mora7l_Madin, ref decimal Mora7l_Da7n, long Branch_Id, long Year_Financial_Period_Id, long    Entry_Currency_Id, int Show_AccountWithBalance)
        {
            Get_Final_Accounts(ref dt_Tash3heel, Accounts_Follow, Branch_Id, Year_Financial_Period_Id, Entry_Currency_Id, Show_AccountWithBalance);
            Compute_Mora7l_Tash3heelValues(ref dt_Tash3heel, Accounts_Follow, ref Mora7l_Madin, ref Mora7l_Da7n);
            //SetTotal(ref   dt_Tash3heel);
        }
        public void Get_Final_AccountsMotagra(ref DataTable dt_Motagra, int Accounts_Follow, ref decimal Mora7l_Madin, ref decimal Mora7l_Da7n, long Branch_Id, long Year_Financial_Period_Id, long    Entry_Currency_Id, int Show_AccountWithBalance)
        {
            ////Tash3heel
            decimal Mora7l_Madin_FromTash3heel = 0;
            decimal Mora7l_Da7n_FromTash3heel = 0;
            DataTable dt_Tash3heel = new DataTable();
            Get_Final_Accounts(ref dt_Tash3heel, 1, Branch_Id, Year_Financial_Period_Id, Entry_Currency_Id, Show_AccountWithBalance);
            Compute_Mora7l_Tash3heelValues(ref dt_Tash3heel, 1, ref Mora7l_Madin_FromTash3heel, ref Mora7l_Da7n_FromTash3heel);

            //////////////Motagra
            dt_Motagra = new DataTable();
            Get_Final_Accounts(ref dt_Motagra, Accounts_Follow, Branch_Id, Year_Financial_Period_Id, Entry_Currency_Id, Show_AccountWithBalance);
            Compute_Mora7l_MotagraValues(ref dt_Motagra, Accounts_Follow, ref Mora7l_Madin, ref Mora7l_Da7n, Mora7l_Madin_FromTash3heel, Mora7l_Da7n_FromTash3heel);

        }
        public void Get_Final_AccountsArba7(ref DataTable dt_Tash3heel, ref DataTable dt_Motagra, ref DataTable dt_Arba7, ref decimal Mora7l_Madin, ref decimal Mora7l_Da7n, long Branch_Id, long Year_Financial_Period_Id, long    Entry_Currency_Id, int Show_AccountWithBalance)
        {
            //
            ////Tash3heel
            decimal Mora7l_Madin_FromTash3heel = 0;
            decimal Mora7l_Da7n_FromTash3heel = 0;
            int Accounts_Follow = 1;
            Get_Final_Accounts(ref dt_Tash3heel, Accounts_Follow, Branch_Id, Year_Financial_Period_Id, Entry_Currency_Id, Show_AccountWithBalance);
            Compute_Mora7l_Tash3heelValues(ref dt_Tash3heel, Accounts_Follow, ref Mora7l_Madin_FromTash3heel, ref Mora7l_Da7n_FromTash3heel);

            //////////////Motagra
            decimal Mora7l_Madin_FromMotagra = 0;
            decimal Mora7l_Da7n_FromMotagra = 0;
            Accounts_Follow = 2;
            Get_Final_Accounts(ref dt_Motagra, Accounts_Follow, Branch_Id, Year_Financial_Period_Id, Entry_Currency_Id, Show_AccountWithBalance);
            Compute_Mora7l_MotagraValues(ref dt_Motagra, Accounts_Follow, ref Mora7l_Madin_FromMotagra, ref Mora7l_Da7n_FromMotagra, Mora7l_Madin_FromTash3heel, Mora7l_Da7n_FromTash3heel);

            //////////////Arba7
            dt_Arba7 = new DataTable();
            Accounts_Follow = 3;
            Get_Final_Accounts(ref dt_Arba7, Accounts_Follow, Branch_Id, Year_Financial_Period_Id, Entry_Currency_Id, Show_AccountWithBalance);
            Compute_Mora7l_MotagraValues(ref dt_Arba7, Accounts_Follow, ref Mora7l_Madin, ref Mora7l_Da7n, Mora7l_Madin_FromMotagra, Mora7l_Da7n_FromMotagra);

        }
        void Compute_Mora7l_Tash3heelValues(ref DataTable dt, int Accounts_Follow, ref decimal Mora7l_Madin, ref decimal Mora7l_Da7n)
        {
            decimal Final_Last_Balance_Madin = 0;
            decimal Final_Last_Balance_Da2n = 0;

            decimal Total_Final_Import_Value = 0;
            decimal Total_Final_Export_Value = 0;
            decimal Total_Final_Last_Balance_Madin = 0;
            decimal Total_Final_Last_Balance_Da2n = 0;


            for (int x = 0; x < dt.Rows.Count; x++)
            {
                try
                {
                    Total_Final_Import_Value = Total_Final_Import_Value + decimal.Parse(dt.Rows[x]["Final_Import_Value"].ToString());
                }
                catch (Exception ee)
                {


                }
                try
                {
                    Total_Final_Export_Value = Total_Final_Export_Value + decimal.Parse(dt.Rows[x]["Final_Export_Value"].ToString());
                }
                catch (Exception r)
                {


                }

                //Total_Final_Last_Balance_Madin = Total_Final_Last_Balance_Madin + decimal.Parse(dt.Rows[x]["Final_Last_Balance_Madin"].ToString());
                //Total_Final_Last_Balance_Da2n = Total_Final_Last_Balance_Da2n + decimal.Parse(dt.Rows[x]["Final_Last_Balance_Da2n"].ToString());



                try
                {
                    decimal Balance_Madin = decimal.Parse(dt.Rows[x]["Final_Last_Balance_Madin"].ToString());
                    //Balance_Madin = Common.RoundMeFormat(Balance_Madin);
                    //dt.Rows[x]["Final_Last_Balance_Madin"] = Balance_Madin.ToString();

                    Final_Last_Balance_Madin = Final_Last_Balance_Madin + Balance_Madin;
                }
                catch { }

                try
                {
                    decimal Balance_Da2n = decimal.Parse(dt.Rows[x]["Final_Last_Balance_Da2n"].ToString());
                    //Balance_Da2n = Common.RoundMeFormat(Balance_Da2n);
                    //dt.Rows[x]["Final_Last_Balance_Da2n"] = Balance_Da2n.ToString();

                    Final_Last_Balance_Da2n = Final_Last_Balance_Da2n + Balance_Da2n;
                }
                catch { }
            }

            SetTotalForImportExport(ref dt, Total_Final_Import_Value, Total_Final_Export_Value);
            try
            {

                /////حساب صافى القيمه المرحله
                decimal Final_Profit = 0;
                if (Total_Final_Import_Value == Total_Final_Export_Value)
                {
                    Final_Profit = 0;
                }
                else if (Total_Final_Import_Value > Total_Final_Export_Value)
                {
                    decimal diff = Total_Final_Import_Value - Total_Final_Export_Value;
                    Final_Profit = diff;

                    //Total_Final_Export_Value = Total_Final_Export_Value + diff;

                }
                else if (Total_Final_Import_Value < Total_Final_Export_Value)
                {
                    decimal diff = Total_Final_Import_Value - Total_Final_Export_Value;
                    diff = diff * -1;
                    Final_Profit = diff;





                }


                if (Final_Profit > 0)
                {
                    Mora7l_Madin = 0;
                    Mora7l_Da7n = Final_Profit;
                }
                if (Final_Profit < 0)
                {
                    Final_Profit = Final_Profit * -1;
                    Mora7l_Madin = Final_Profit;
                    Mora7l_Da7n = 0;
                }
                if (Final_Profit == 0)
                {
                    Mora7l_Madin = 0;
                    Mora7l_Da7n = 0;
                }
                DataRow dr = dt.NewRow();
                //dt.Rows.Add(dr);

                ///////////////////////المرحل  
                dr = dt.NewRow();
                dr["Account_Name"] = "  تكلفة التشغيل مرحل الى حساب المتاجرة  ";
                dr["Final_Last_Balance_Madin"] = 0;
                dr["Final_Last_Balance_Da2n"] = 0;

                dr["Final_Fist_Balance_Madin"] = 0;
                dr["Final_Fist_Balance_Da2n"] = 0;
                dr["Final_Import_Value"] = Mora7l_Madin;
                dr["Final_Export_Value"] = Mora7l_Da7n;
                dr["Final_Transaction_Madin"] = 0;
                dr["Final_Transaction_Da2n"] = 0;

                dt.Rows.Add(dr);
                decimal Final_Last_Balance_Madin_ = 0;
                decimal Final_Last_Balance_Da2n_ = 0;
                SetParentTotal(ref dt, ref Final_Last_Balance_Madin_, ref Final_Last_Balance_Madin_);



            }
            catch (Exception ee)
            {


            }


            ///////////////////////الإجمالى  
            //dr = dt.NewRow();
            //dr["Account_Name"] = "  الإجمالى  ";
            //dr["Final_Last_Balance_Madin"] = Final_Last_Balance_Madin;
            //dr["Final_Last_Balance_Da2n"] = Final_Last_Balance_Da2n;


            //dt.Rows.Add(dr); 

        }
        void Compute_Mora7l_MotagraValues(ref DataTable dt, int Accounts_Follow, ref decimal Mora7l_Madin, ref decimal Mora7l_Da7n, decimal Mora7l_Madin_FromTash3heel, decimal Mora7l_Da7n_FromTash3heel)
        {

            decimal Final_Last_Balance_Madin = 0;
            decimal Final_Last_Balance_Da2n = 0;

            ////يتم وضع القيمه المرحله من حساب التشغيل  
            ////// القيمه الدائنه تكون فى الجانب المدين والعكس

            decimal Updated_Mora7l_Madin_FromTash3heel = Mora7l_Da7n_FromTash3heel;
            decimal Updated_Mora7l_Da7n_FromTash3heel = Mora7l_Madin_FromTash3heel;
            decimal Total_Final_Import_Value = 0;
            decimal Total_Final_Export_Value = 0;
            for (int x = 0; x < dt.Rows.Count; x++)
            {

                try
                {
                    Total_Final_Import_Value = Total_Final_Import_Value + decimal.Parse(dt.Rows[x]["Final_Import_Value"].ToString());
                }
                catch { }
                try
                {
                    Total_Final_Export_Value = Total_Final_Export_Value + decimal.Parse(dt.Rows[x]["Final_Export_Value"].ToString());
                }
                catch { }
                try
                {
                    decimal Balance_Madin = decimal.Parse(dt.Rows[x]["Final_Last_Balance_Madin"].ToString());
                    Balance_Madin = Common.RoundMe(Balance_Madin);
                    dt.Rows[x]["Final_Last_Balance_Madin"] = Balance_Madin.ToString();

                    Final_Last_Balance_Madin = Final_Last_Balance_Madin + Balance_Madin;
                }
                catch { }

                try
                {
                    decimal Balance_Da2n = decimal.Parse(dt.Rows[x]["Final_Last_Balance_Da2n"].ToString());
                    Balance_Da2n = Common.RoundMe(Balance_Da2n);
                    dt.Rows[x]["Final_Last_Balance_Da2n"] = Balance_Da2n.ToString();

                    Final_Last_Balance_Da2n = Final_Last_Balance_Da2n + Balance_Da2n;
                }
                catch { }
            }

            SetTotalForImportExport(ref dt, Total_Final_Import_Value, Total_Final_Export_Value);

            ////////////////////////////////////////////////////////////////////////

            ////////////////////////إضافة القيمه المرحله من التشغيل الى الجدول للعرض
            ////يتم وضع القيمه المرحله من حساب التشغيل  

            DataRow dr = dt.NewRow();
            //dt.Rows.Add(dr); 
            dr = dt.NewRow();
            if (Accounts_Follow == 2)
            {
                dr["Account_Name"] = "  تكلفة التشغيل  - مرحل من حساب التشغيل  ";
            }
            if (Accounts_Follow == 3)
            {
                dr["Account_Name"] = "    مرحل من حساب المتاجره  ";
            }
            dr["Final_Last_Balance_Madin"] = Updated_Mora7l_Madin_FromTash3heel;
            dr["Final_Last_Balance_Da2n"] = Updated_Mora7l_Da7n_FromTash3heel;
            dr["Final_Fist_Balance_Madin"] = 0;
            dr["Final_Fist_Balance_Da2n"] = 0;
            dr["Final_Import_Value"] = 0;
            dr["Final_Export_Value"] = 0;
            dr["Final_Transaction_Madin"] = 0;
            dr["Final_Transaction_Da2n"] = 0;

            dt.Rows.Add(dr);


            ////////////////////////////////
            /////////إضافة القيمه المدينه المعدله المرحله من التشغيل الى القيمه المدينه لاجمالى والعكس
            ///

            Total_Final_Import_Value = Total_Final_Import_Value + Updated_Mora7l_Madin_FromTash3heel;
            Total_Final_Export_Value = Total_Final_Export_Value + Updated_Mora7l_Da7n_FromTash3heel;


            /////حساب صافى القيمه المرحله
            decimal Final_Profit = 0;

            if (Total_Final_Import_Value == Total_Final_Export_Value)
            {
                Final_Profit = Total_Final_Import_Value - Total_Final_Export_Value;
            }
            else if (Total_Final_Import_Value > Total_Final_Export_Value)
            {
                decimal diff = Total_Final_Import_Value - Total_Final_Export_Value;
                Final_Profit = diff;

                //Total_Final_Export_Value = Total_Final_Export_Value + diff;
                Total_Final_Export_Value = diff;

            }
            else if (Total_Final_Import_Value < Total_Final_Export_Value)
            {
                decimal diff = Total_Final_Import_Value - Total_Final_Export_Value;
                diff = diff * -1;

                Final_Profit = diff;


                //Total_Final_Import_Value = Total_Final_Import_Value + diff;
                Total_Final_Import_Value = diff;

            }
            /*
            Final_Last_Balance_Madin = Final_Last_Balance_Madin + Updated_Mora7l_Madin_FromTash3heel;
            Final_Last_Balance_Da2n = Final_Last_Balance_Da2n + Updated_Mora7l_Da7n_FromTash3heel;


            /////حساب صافى القيمه المرحله
            decimal Final_Profit = 0;

            if (Final_Last_Balance_Madin == Final_Last_Balance_Da2n)
            {
                Final_Profit = Final_Last_Balance_Madin - Final_Last_Balance_Da2n;
            }
            else if (Final_Last_Balance_Madin > Final_Last_Balance_Da2n)
            {
                decimal diff = Final_Last_Balance_Madin - Final_Last_Balance_Da2n;
                Final_Profit = diff;

                //Final_Last_Balance_Da2n = Final_Last_Balance_Da2n + diff;
                Final_Last_Balance_Da2n =  diff;

            }
            else if (Final_Last_Balance_Madin < Final_Last_Balance_Da2n)
            {
                decimal diff = Final_Last_Balance_Madin - Final_Last_Balance_Da2n;
                diff = diff * -1;

                Final_Profit = diff;

                
                //Final_Last_Balance_Madin = Final_Last_Balance_Madin + diff;
                Final_Last_Balance_Madin =  diff;

            }
            */


            dr = dt.NewRow();
            //dt.Rows.Add(dr);
            ///////////////////////المرحل  
            dr = dt.NewRow();
            if (Accounts_Follow == 2)
            {
                if (Final_Profit > 0)
                {

                    Mora7l_Madin = 0;
                    Mora7l_Da7n = Final_Profit;
                    dr["Account_Name"] = "  مجمل الخسارة مرحل الى حساب أرباح وخسائر  ";
                }
                if (Final_Profit < 0)
                {
                    Final_Profit = Final_Profit * -1;
                    Mora7l_Madin = Final_Profit;
                    Mora7l_Da7n = 0;

                    dr["Account_Name"] = "  مجمل الربح مرحل الى حساب أرباح وخسائر  ";
                }
                if (Final_Profit == 0)
                {
                    Mora7l_Madin = 0;
                    Mora7l_Da7n = 0;
                    dr["Account_Name"] = "    مرحل الى حساب أرباح وخسائر  ";
                }
            }
            if (Accounts_Follow == 3)
            {
                if (Final_Profit > 0)
                {

                    Mora7l_Madin = 0;
                    Mora7l_Da7n = Final_Profit;
                    dr["Account_Name"] = "صافى خسارة الفتره مرحل الى الميزانيه";
                }
                if (Final_Profit < 0)
                {
                    Final_Profit = Final_Profit * -1;
                    Mora7l_Madin = Final_Profit;
                    Mora7l_Da7n = 0;

                    dr["Account_Name"] = "صافى ربح الفتره مرحل الى الميزانيه";
                }
                if (Final_Profit == 0)
                {
                    Mora7l_Madin = 0;
                    Mora7l_Da7n = 0;
                    dr["Account_Name"] = " مرحل الى الميزانيه";
                }
            }


            dr["Final_Last_Balance_Madin"] = Mora7l_Madin;
            dr["Final_Last_Balance_Da2n"] = Mora7l_Da7n;
            dr["Final_Fist_Balance_Madin"] = 0;
            dr["Final_Fist_Balance_Da2n"] = 0;
            dr["Final_Import_Value"] = 0;
            dr["Final_Export_Value"] = 0;
            dr["Final_Transaction_Madin"] = 0;
            dr["Final_Transaction_Da2n"] = 0;

            dt.Rows.Add(dr);

            decimal Final_Last_Balance_Madin_ = 0;
            decimal Final_Last_Balance_Da2n_ = 0;
            SetParentTotal(ref dt, ref Final_Last_Balance_Madin_, ref Final_Last_Balance_Madin_);


        }

        void SetTotalForImportExport(ref DataTable dt, decimal Total_Final_Import_Value, decimal Total_Final_Export_Value)
        {
            ////////الاجماليات
            ///

            DataRow drT = dt.NewRow();
            decimal Diff = Total_Final_Import_Value - Total_Final_Export_Value;
            drT["Final_Fist_Balance_Madin"] = 0;
            drT["Final_Fist_Balance_Da2n"] = 0;
            drT["Final_Transaction_Madin"] = 0;
            drT["Final_Transaction_Da2n"] = 0;
            drT["Final_Last_Balance_Madin"] = 0;
            drT["Final_Last_Balance_Da2n"] = 0;
            drT["Final_Import_Value"] = Total_Final_Import_Value;
            drT["Final_Export_Value"] = Total_Final_Export_Value;

            //drT["Final_Import_Value"] = Total_Final_Import_Value;
            //drT["Final_Export_Value"] = Total_Final_Export_Value;
            if (Diff > 0)
            {
                drT["Final_Last_Balance_Madin"] = Diff;
                drT["Final_Last_Balance_Da2n"] = 0;

            }
            if (Diff < 0)
            {
                Diff = Diff * -1;
                drT["Final_Last_Balance_Madin"] = 0;
                drT["Final_Last_Balance_Da2n"] = Diff;

            }
            if (Diff == 0)
            {
                drT["Final_Last_Balance_Madin"] = 0;
                drT["Final_Last_Balance_Da2n"] = 0;

            }

            drT["Submain_Account_ID"] = -10;
            drT["Account_Name"] = "****الإجمالى***";
            dt.Rows.Add(drT);

            ////////////////////////////////////////////////////////////////////////
        }
    }
}
