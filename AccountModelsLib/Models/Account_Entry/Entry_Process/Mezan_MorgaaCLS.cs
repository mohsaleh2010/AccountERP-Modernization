using AccountModelsLib.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Numerics;





namespace AccountModelsLib
{
    public class Mezan_MorgaaCLS
    {
        public Mezan_MorgaaCLS()
        {
        }
        public long Account_Customer_Type_ID { get; set; } = -1;
        public int Account_Customer_Type_Value { get; set; } = -1;
        public int IsShowTrueAccountOnly { get; set; } = 0; // عرض الحسابات الحقيقية فقط

        public static DataTable get_Tree(long Branch_Id, int Account_Level)
        {
            //  Equivalent_First_Balance_Madin,Equivalent_First_Balance_Da2n
            string mysql = @" SELECT        Submain_Account.*, MainSubmain_Account.SubAccount_Name AS MainAccount_Name, MainSubmain_Account.SubAccount_NO AS MainSubAccount_NO, 
                         MainSubmain_Account.SubAccount_NO AS MainCode
FROM            Submain_Account INNER JOIN
                         Submain_Account AS MainSubmain_Account ON Submain_Account.Main_Account_Id = MainSubmain_Account.Id Where Submain_Account.Branch_Id=" + Branch_Id + " AND Submain_Account.Account_Level <= " + Account_Level + " Order By Submain_Account.Code";


            DataTable dt = new DataTable();
            operations opp = new operations();
            dt = opp.return_DataTable(mysql);
            return dt;
        }
        public void UpdateFirstBalance(ref int Excp, ref string Msg, long Entry_Currency_ID, long Branch_Id, DateTime Financial_dtFrom, DateTime Financial_dtTo, long Year_Financial_Period_Id_ToUpdate, int Currency_Count)
        {



            //long Entry_Currency_Id = 0;
            operations opp = new operations();
            Mezan_MorgaaCLS mm = new Mezan_MorgaaCLS();


            try
            {

                //int index = financial_Period1.cmb_Financial_Period.SelectedIndex;

                if (Financial_dtFrom.Year <= 2014)
                {

                    //Excp = 1;
                    //Msg = "لا يوجد بيانات قبل هذا التاريخ";
                    //return;
                }



                long Year_Financial_Period_Id_Prev = 0;
                DataTable dtPrev = mm.Get_Year_Financial_Period_IdPrev(ref opp, Financial_dtFrom, ref Year_Financial_Period_Id_Prev);

                if (Year_Financial_Period_Id_Prev == 0)
                {

                    Excp = 1;
                    Msg = "لا يوجد فترة مالية مسجله من قبل لتحديث  الأرصدة فى الفترة الحالية";
                    return;

                }
                else
                {
                    Financial_dtFrom = DateTime.Parse(dtPrev.Rows[0]["DateFrom"].ToString());
                    Financial_dtTo = DateTime.Parse(dtPrev.Rows[0]["DateTo"].ToString());
                }
                string mysql = "";
                mysql = "Delete from Account_Entry_Master_Balance Where Year_Financial_Period_Id=" + Year_Financial_Period_Id_ToUpdate + " And  Branch_Id=" + Branch_Id + "";
                if (Entry_Currency_ID != -1)
                {
                    mysql = mysql + "AND Equivalent_Account_Currency_Id=" + Entry_Currency_ID + "";
                }
                opp.insert_me(mysql);

                //int Currency_Count = 0;
                //if (Entry_Currency_Id == -1)
                //{
                //    Currency_Count = currency_List_21.cmbCurrencyName.Items.Count;
                //}
                //else
                //{
                //    Currency_Count = 1;
                //}
                bool AllCurrency = false;
                if (Entry_Currency_ID == -1)
                {
                    AllCurrency = true;
                }
                for (int i = 0; i < Currency_Count; i++)
                {
                    if (AllCurrency == true)
                    {
                        Entry_Currency_ID = i;
                    }
                    DataTable dt7 = new DataTable();
                    mm.SetTemp_Mezan_Morgaa_ByPeiodNew(ref opp, false, Branch_Id, Financial_dtFrom, Financial_dtTo, 0, Entry_Currency_ID, Year_Financial_Period_Id_Prev, true, false, Financial_dtFrom);

                    mysql = @"INSERT INTO Account_Entry_Master_Balance(Submain_Account_ID, SubAccount_Name, First_Balance_Madin, First_Balance_Da2n, First_Balance, Year_Financial_Period_Id, Branch_Id, Equivalent_Account_Currency_Id, Account_Currency_Id)

            SELECT        Submain_Account_ID, Account_Name, Final_Last_Balance_Madin, Final_Last_Balance_Da2n, Final_Last_Balance_Madin+ Final_Last_Balance_Da2n, " + Year_Financial_Period_Id_ToUpdate + ", Branch_Id, Equivalent_Account_Currency_Id, Account_Currency_Id ";

                    mysql = mysql + " FROM   Account_Entry_Master_Balance_MezanTemp";

                    opp.insert_me(mysql);


                }
                ///رصيد اول المده للحسابات الوهمية بصفر
                mysql = @"UPDATE       Account_Entry_Master_Balance
SET                Equivalent_First_Balance = 0, First_Balance_Madin = 0, First_Balance = 0, First_Balance_Da2n = 0, Equivalent_First_Balance_Madin = 0, 
                         Equivalent_First_Balance_Da2n = 0
FROM            Submain_Account INNER JOIN
                         Account_Entry_Master_Balance ON Submain_Account.Id = Account_Entry_Master_Balance.Submain_Account_ID
WHERE        (Submain_Account.Account_Follows = 1) OR
                         (Submain_Account.Account_Follows = 2) OR
                         (Submain_Account.Account_Follows = 3)";

                opp.insert_me(mysql);

                Excp = 0;
                Msg = "تم تحديث الأرصدة بنجاح";
            }

            catch (Exception ee)
            {

                Excp = 1;
                Msg = ee.Message;

            }
        }

        public void UpdateFirstBalanceCostCenter(ref int Excp, ref string Msg, long Entry_Currency_Id, long Branch_Id, DateTime Financial_dtFrom, DateTime Financial_dtTo, long Year_Financial_Period_Id_ToUpdate, int Currency_Count)
        {



            //long Entry_Currency_Id = 0;
            operations opp = new operations();
            Mezan_MorgaaCLS mm = new Mezan_MorgaaCLS();


            try
            {

                //int index = financial_Period1.cmb_Financial_Period.SelectedIndex;

                //if (Financial_dtFrom.Year <= 2014)
                //{

                //    Excp = 1;
                //    Msg = "لا يوجد بيانات قبل هذا التاريخ";
                //    return;
                //}



                long Year_Financial_Period_Id_Prev = 0;
                DataTable dtPrev = mm.Get_Year_Financial_Period_IdPrev(ref opp, Financial_dtFrom, ref Year_Financial_Period_Id_Prev);

                if (Year_Financial_Period_Id_Prev == 0)
                {

                    Excp = 1;
                    Msg = "لا يوجد فترة مالية مسجله من قبل  لتحديث الأرصدة منها";
                    return;

                }
                else
                {
                    Financial_dtFrom = DateTime.Parse(dtPrev.Rows[0]["DateFrom"].ToString());
                    Financial_dtTo = DateTime.Parse(dtPrev.Rows[0]["DateTo"].ToString());
                }
                string mysql = "";
                mysql = "Delete from Account_Entry_Master_Balance_CostCenter Where Year_Financial_Period_Id=" + Year_Financial_Period_Id_ToUpdate + " And  Branch_Id=" + Branch_Id + "";
                if (Entry_Currency_Id != -1)
                {
                    mysql = mysql + "AND Equivalent_Account_Currency_Id=" + Entry_Currency_Id + "";
                }
                opp.insert_me(mysql);

                //int Currency_Count = 0;
                //if (Entry_Currency_Id == -1)
                //{
                //    Currency_Count = currency_List_21.cmbCurrencyName.Items.Count;
                //}
                //else
                //{
                //    Currency_Count = 1;
                //}
                bool AllCurrency = false;
                if (Entry_Currency_Id == -1)
                {
                    AllCurrency = true;
                }
                for (int i = 0; i < Currency_Count; i++)
                {
                    if (AllCurrency == true)
                    {
                        Entry_Currency_Id = i;
                    }
                    DataTable dt7 = new DataTable();
                    mm.SetTemp_Mezan_Morgaa_ByPeiodNew_CostCenter(ref opp, false, Branch_Id, Financial_dtFrom, Financial_dtTo, 0, Entry_Currency_Id, Year_Financial_Period_Id_Prev, true, false, Financial_dtFrom);

                    mysql = @"INSERT INTO Account_Entry_Master_Balance_CostCenter( Cost_Center_Id, Cost_Center_Name,Submain_Account_ID, SubAccount_Name, First_Balance_Madin, First_Balance_Da2n, First_Balance, Year_Financial_Period_Id, Branch_Id, Equivalent_Account_Currency_Id, Account_Currency_Id)
            SELECT        Cost_Center_Id, Cost_Center_Name, Submain_Account_ID, Account_Name, Final_Last_Balance_Madin, Final_Last_Balance_Da2n, Final_Last_Balance_Madin+ Final_Last_Balance_Da2n, " + Year_Financial_Period_Id_ToUpdate + ", Branch_Id, Equivalent_Account_Currency_Id, Account_Currency_Id FROM   Account_Entry_Master_Balance_CostCenterTemp";

                    opp.insert_me(mysql);


                }
                ///رصيد اول المده للحسابات الوهمية بصفر
                //                mysql = @"UPDATE       Account_Entry_Master_Balance_CostCenter
                //SET                Equivalent_First_Balance = 0, First_Balance_Madin = 0, First_Balance = 0, First_Balance_Da2n = 0, Equivalent_First_Balance_Madin = 0, 
                //                         Equivalent_First_Balance_Da2n = 0
                //FROM            Submain_Account INNER JOIN
                //                         Account_Entry_Master_Balance ON Submain_Account.Id = Account_Entry_Master_Balance.Submain_Account_ID
                //WHERE        (Submain_Account.Account_Follows = 1) OR
                //                         (Submain_Account.Account_Follows = 2) OR
                //                         (Submain_Account.Account_Follows = 3)";

                //                opp.insert_me(mysql);

                Excp = 0;
                Msg = "تم تحديث الأرصدة بنجاح";
            }

            catch (Exception ee)
            {

                Excp = 1;
                Msg = ee.Message;

            }
        }
        public void Get_FirstBalance_Period(DataTable dt, int x, bool Is_Equivalent, bool Is_EquivalentFactor, ref decimal First_Balance_Madin, ref decimal First_Balance_Da2n)
        {
            //  Equivalent_First_Balance_Madin,Equivalent_First_Balance_Da2n



            First_Balance_Madin = 0;
            First_Balance_Da2n = 0;

            if (Is_Equivalent == true)
            {

                if (Is_EquivalentFactor == true)
                {
                    try
                    {
                        First_Balance_Madin = First_Balance_Madin + decimal.Parse(dt.Rows[x]["First_Balance_Madin_Factor"].ToString());
                    }
                    catch { }

                    try
                    {
                        First_Balance_Da2n = First_Balance_Da2n + decimal.Parse(dt.Rows[x]["First_Balance_Da2n_Factor"].ToString());
                    }
                    catch { }
                }
                else
                {

                    try
                    {
                        First_Balance_Madin = First_Balance_Madin + decimal.Parse(dt.Rows[x]["Equivalent_First_Balance_Madin"].ToString());
                    }
                    catch { }

                    try
                    {
                        First_Balance_Da2n = First_Balance_Da2n + decimal.Parse(dt.Rows[x]["Equivalent_First_Balance_Da2n"].ToString());
                    }
                    catch { }
                }
            }
            else
            {
                try
                {
                    First_Balance_Madin = First_Balance_Madin + decimal.Parse(dt.Rows[x]["First_Balance_Madin"].ToString());
                }
                catch { }

                try
                {
                    First_Balance_Da2n = First_Balance_Da2n + decimal.Parse(dt.Rows[x]["First_Balance_Da2n"].ToString());
                }
                catch { }
            }



        }

        public void Get_FirstBalance_PeriodNew(DataTable dt, int x, ref decimal First_Balance_Madin, ref decimal First_Balance_Da2n)
        {
            //  Equivalent_First_Balance_Madin,Equivalent_First_Balance_Da2n



            First_Balance_Madin = 0;
            First_Balance_Da2n = 0;


            try
            {
                First_Balance_Madin = First_Balance_Madin + decimal.Parse(dt.Rows[x]["First_Balance_Madin"].ToString());
            }
            catch { }

            try
            {
                First_Balance_Da2n = First_Balance_Da2n + decimal.Parse(dt.Rows[x]["First_Balance_Da2n"].ToString());
            }
            catch { }




        }

        public void Get_FirstBalance_Period(bool Is_Equivalent, bool Is_EquivalentFactor, long Year_Financial_Period_Id, long Branch_Id, long Submain_Account_ID, ref decimal First_Balance_Madin, ref decimal First_Balance_Da2n)
        {
            //  Equivalent_First_Balance_Madin,Equivalent_First_Balance_Da2n
            string mysql = "";
            mysql = @" SELECT        Account_Entry_Master_Balance.First_Balance_Madin * Currency_Factor.Factor AS First_Balance_Madin_Factor, 
                         Account_Entry_Master_Balance.First_Balance_Da2n * Currency_Factor.Factor AS First_Balance_Da2n_Factor, 
                         Account_Entry_Master_Balance.Equivalent_First_Balance_Madin, Account_Entry_Master_Balance.Equivalent_First_Balance_Da2n, 
                         Account_Entry_Master_Balance.First_Balance_Madin, Account_Entry_Master_Balance.First_Balance_Da2n
FROM            Account_Entry_Master_Balance INNER JOIN
                         Currency_Factor ON Account_Entry_Master_Balance.Account_Currency_Id = Currency_Factor.Currency_ID_Primary AND 
                         Account_Entry_Master_Balance.Equivalent_Account_Currency_Id = Currency_Factor.Currency_ID_Secondary
";

            //mysql = "SELECT        Account_Entry_Master_Balance.*  From Account_Entry_Master_Balance";
            mysql = mysql + " WHERE        (Account_Entry_Master_Balance.Year_Financial_Period_Id = " + Year_Financial_Period_Id + ") ";

            if (Branch_Id > 0)
            {
                mysql = mysql + " AND (Account_Entry_Master_Balance.Branch_Id = " + Branch_Id + ") ";
            }
            if (Submain_Account_ID > 0)
            {
                mysql = mysql + " AND (Account_Entry_Master_Balance.Submain_Account_ID = " + Submain_Account_ID + ")";
            }
            DataTable dt = new DataTable();
            operations opp = new operations();
            dt = opp.return_DataTable(mysql);
            First_Balance_Madin = 0;
            First_Balance_Da2n = 0;
            for (int x = 0; x < dt.Rows.Count; x++)
            {
                if (Is_Equivalent == true)
                {

                    if (Is_EquivalentFactor == true)
                    {
                        try
                        {
                            First_Balance_Madin = First_Balance_Madin + decimal.Parse(dt.Rows[x]["First_Balance_Madin_Factor"].ToString());
                        }
                        catch { }

                        try
                        {
                            First_Balance_Da2n = First_Balance_Da2n + decimal.Parse(dt.Rows[x]["First_Balance_Da2n_Factor"].ToString());
                        }
                        catch { }
                    }
                    else
                    {

                        try
                        {
                            First_Balance_Madin = First_Balance_Madin + decimal.Parse(dt.Rows[x]["Equivalent_First_Balance_Madin"].ToString());
                        }
                        catch { }

                        try
                        {
                            First_Balance_Da2n = First_Balance_Da2n + decimal.Parse(dt.Rows[x]["Equivalent_First_Balance_Da2n"].ToString());
                        }
                        catch { }
                    }
                }
                else
                {
                    try
                    {
                        First_Balance_Madin = First_Balance_Madin + decimal.Parse(dt.Rows[x]["First_Balance_Madin"].ToString());
                    }
                    catch { }

                    try
                    {
                        First_Balance_Da2n = First_Balance_Da2n + decimal.Parse(dt.Rows[x]["First_Balance_Da2n"].ToString());
                    }
                    catch { }
                }
            }


        }
        public DataTable Get_Mezan_Morgaa_ByPeiod(long Branch_Id, DateTime dtFom, DateTime dtTo, long Submain_Account_ID, long Account_Currency_Id, long Year_Financial_Period_Id)
        {
            string mysql = "";
            operations opp = new operations();


            mysql = @"  SELECT        1 AS Last_Balance_Zero, Account_Entry_Details.Submain_Account_ID, Account_Entry_Details.Account_Name, SUM(Account_Entry_Details.Import_Value) 
                         AS Import_Value, SUM(Account_Entry_Details.Export_Value) AS Export_Value, SUM(Account_Entry_Details.Equivalent_Import_Value) AS Final_Import_Value, 
                         SUM(Account_Entry_Details.Equivalent_Export_Value) AS Final_Export_Value, 0.0 AS Final_Fist_Balance_Madin, 0.0 AS Final_Fist_Balance_Da2n, 
                         0.0 AS Final_Last_Balance_Madin, 0.0 AS Final_Last_Balance_Da2n, Submain_Account.SubAccount_NO, Submain_Account.Code, Submain_Account.Account_Code, 
                         MainSubmain_Account.SubAccount_Name AS MainAccount_Name, MainSubmain_Account.SubAccount_NO AS MainAccount_NO, Submain_Account.Main_Account_Id, 
                         MainSubmain_Account.SubAccount_NO AS MainCode, Account_Entry_Master_Balance.First_Balance_Madin * Currency_Factor.Factor AS First_Balance_Madin_Factor, 
                         Account_Entry_Master_Balance.First_Balance_Da2n * Currency_Factor.Factor AS First_Balance_Da2n_Factor, 
                         Account_Entry_Master_Balance.Equivalent_First_Balance_Madin, Account_Entry_Master_Balance.Equivalent_First_Balance_Da2n, 
                         Account_Entry_Master_Balance.First_Balance_Madin, Account_Entry_Master_Balance.First_Balance_Da2n, Currency_Factor.Currency_Secondary_Name, 
                         Currency_Factor.Currency_Primary_Name
FROM            Account_Entry_Details INNER JOIN
                         Account_Entry_List ON Account_Entry_Details.Account_Entry_List_ID = Account_Entry_List.Id INNER JOIN
                         Submain_Account ON Account_Entry_Details.Submain_Account_ID = Submain_Account.Id INNER JOIN
                         Submain_Account AS MainSubmain_Account ON Submain_Account.Main_Account_Id = MainSubmain_Account.Id INNER JOIN
                         Account_Entry_Master_Balance ON Account_Entry_Details.Submain_Account_ID = Account_Entry_Master_Balance.Submain_Account_ID AND 
                         Account_Entry_List.Branch_Id_Entry = Account_Entry_Master_Balance.Branch_Id INNER JOIN
                         Currency_Factor ON Account_Entry_Master_Balance.Account_Currency_Id = Currency_Factor.Currency_ID_Primary AND 
                         Account_Entry_Master_Balance.Equivalent_Account_Currency_Id = Currency_Factor.Currency_ID_Secondary ";


            mysql = mysql + " WHERE     (Account_Entry_List.Entry_No IS NOT  NULL) AND  (Account_Entry_List.Entry_Date >= " + operations.Get_DateToInsert(dtFom) + ") AND (Account_Entry_List.Entry_Date <= " + operations.Get_DateToInsert(dtTo) + ") AND     (Account_Entry_Master_Balance.Year_Financial_Period_Id = " + Year_Financial_Period_Id + ")";
            if (Submain_Account_ID > 0)
            {
                mysql = mysql + " AND    (Account_Entry_Details.Submain_Account_ID  = " + Submain_Account_ID + ") ";
            }
            if (Branch_Id > 0)
            {
                mysql = mysql + " AND    (Account_Entry_List.Branch_Id_Entry  = " + Branch_Id + ") ";
            }
            if (Account_Currency_Id > 0)
            {
                mysql = mysql + " AND (Submain_Account.Currency_Id = " + Account_Currency_Id + ") ";
            }
            //mysql = mysql + " GROUP BY Account_Entry_List.Branch_Id_Entry,Account_Entry_Details.Submain_Account_ID, Account_Entry_Details.Account_Name, Currency_Factor.Currency_Primary_Name, Currency_Factor.Currency_Secondary_Name, Currency_Factor.Factor, Submain_Account.SubAccount_NO, Submain_Account.Code, Submain_Account.Account_Code,  Submain_Account.Currency_Id";
            mysql = mysql + @" GROUP BY Account_Entry_List.Branch_Id_Entry, Account_Entry_Details.Submain_Account_ID, Account_Entry_Details.Account_Name, Submain_Account.SubAccount_NO, 
                         Submain_Account.Code, Submain_Account.Account_Code, Submain_Account.Currency_Id, MainSubmain_Account.SubAccount_Name, 
                         MainSubmain_Account.SubAccount_NO, Submain_Account.Main_Account_Id, MainSubmain_Account.Code, Account_Entry_Master_Balance.First_Balance_Da2n, 
                         Account_Entry_Master_Balance.First_Balance_Madin, Account_Entry_Master_Balance.Equivalent_First_Balance_Da2n, 
                         Account_Entry_Master_Balance.Equivalent_First_Balance_Madin, Account_Entry_Master_Balance.First_Balance_Da2n * Currency_Factor.Factor, 
                         Account_Entry_Master_Balance.First_Balance_Madin * Currency_Factor.Factor, Account_Entry_Master_Balance.Year_Financial_Period_Id, 
                         Currency_Factor.Currency_Secondary_Name, Currency_Factor.Currency_Primary_Name ";

            mysql = mysql + @" ORDER BY MainSubmain_Account.Code   ";
            //, Submain_Account.Code
            DataTable dt = new DataTable();

            dt = opp.return_DataTable(mysql);



            //////////////////////////يجب حذف السطور بين المنطقه  التاليه بعد ضبط البرنامج  بعد برنامج الفنار
            ///  الحسابات التى لها رصيد اول المده وليس لها حركات للفتره المحدده
            #region  Region Must delete after  fanar GL

            mysql = @" SELECT      Submain_Account.Code,  Submain_Account_1.Code AS MainCode, Submain_Account.Id, Submain_Account.SubAccount_Name  , Submain_Account.SubAccount_NO, 
                         Submain_Account.Account_Code, Submain_Account_1.SubAccount_Name AS MainAccount_Name, Submain_Account_1.SubAccount_NO AS MainAccount_NO, 
                         Account_Entry_Master_Balance.First_Balance, Account_Entry_Master_Balance.Equivalent_First_Balance, Account_Entry_Master_Balance.First_Balance_Madin, 
                         Account_Entry_Master_Balance.First_Balance_Da2n, Account_Entry_Master_Balance.Equivalent_First_Balance_Madin, 
                         Account_Entry_Master_Balance.Equivalent_First_Balance_Da2n
FROM            Account_Entry_Master_Balance INNER JOIN
                         Submain_Account ON Account_Entry_Master_Balance.Submain_Account_ID = Submain_Account.Id INNER JOIN
                         Submain_Account AS Submain_Account_1 ON Submain_Account.Main_Account_Id = Submain_Account_1.SubAccount_Id
WHERE        (Account_Entry_Master_Balance.Submain_Account_ID NOT IN
                             (SELECT DISTINCT Account_Entry_Details.Submain_Account_ID
                                FROM            Account_Entry_Details INNER JOIN
                                                         Account_Entry_List ON Account_Entry_Details.Account_Entry_List_ID = Account_Entry_List.Id ";

            mysql = mysql + " AND     (Account_Entry_List.Entry_No IS NOT  NULL) AND  (Account_Entry_List.Entry_Date >= " + operations.Get_DateToInsert(dtFom) + ") AND (Account_Entry_List.Entry_Date <= " + operations.Get_DateToInsert(dtTo) + ") AND     (Account_Entry_Master_Balance.Year_Financial_Period_Id = " + Year_Financial_Period_Id + ")))";
            mysql = mysql + "  AND (Account_Entry_Master_Balance.First_Balance <> 0)";
            //WHERE        (Account_Entry_List.Entry_Date >= CONVERT(DATETIME, '01/01/2014', 102)) AND (Account_Entry_List.Entry_No IS NOT NULL) AND 
            //                                            (Account_Entry_List.Entry_Date <= CONVERT(DATETIME, '12/31/2014', 102))))";


            DataTable dt3 = opp.return_DataTable(mysql);
            for (int x = 0; x < dt3.Rows.Count; x++)
            {
                if (dt3.Rows[x]["Code"].ToString() == "30101010001")
                {
                    int bbb = 0;
                }
                DataRow dr = dt.NewRow();
                dr["First_Balance_Madin"] = dt3.Rows[x]["First_Balance_Madin"].ToString();
                dr["First_Balance_Da2n"] = dt3.Rows[x]["First_Balance_Da2n"].ToString();
                dr["Equivalent_First_Balance_Madin"] = dt3.Rows[x]["Equivalent_First_Balance_Madin"].ToString();
                dr["Equivalent_First_Balance_Da2n"] = dt3.Rows[x]["Equivalent_First_Balance_Da2n"].ToString();

                dr["Submain_Account_ID"] = dt3.Rows[x]["SubAccount_Id"].ToString();
                dr["Account_Name"] = dt3.Rows[x]["SubAccount_Name"].ToString();
                //dr["Currency_Primary_Name"] = "";
                //dr["Currency_Secondary_Name"] = "";
                dr["Import_Value"] = 0;
                dr["Export_Value"] = 0;
                //dr["Factor"] = 0;
                dr["Final_Import_Value"] = 0;
                dr["Final_Export_Value"] = 0;
                dr["Final_Fist_Balance_Madin"] = 0;
                dr["Final_Fist_Balance_Da2n"] = 0;
                dr["Final_Last_Balance_Madin"] = 0;
                dr["Final_Last_Balance_Da2n"] = 0;
                dr["SubAccount_NO"] = dt3.Rows[x]["SubAccount_NO"].ToString();
                dr["Code"] = dt3.Rows[x]["Code"].ToString();
                dr["Account_Code"] = dt3.Rows[x]["Account_Code"].ToString();
                dr["MainAccount_Name"] = dt3.Rows[x]["MainAccount_Name"].ToString();
                dr["MainAccount_NO"] = dt3.Rows[x]["MainAccount_NO"].ToString();
                dr["Main_Account_Id"] = 0; ;
                dr["MainCode"] = dt3.Rows[x]["MainCode"].ToString();

                dt.Rows.Add(dr);
            }


            #endregion

            return dt;
        }

        public string Get_Mezan_Morgaa_ByPeiodNewUpdate(ref operations opp, bool IS_Excute_All, long Branch_Id, DateTime dtFom, DateTime dtTo, long Submain_Account_ID, long Entry_Currency_Id, long Year_Financial_Period_Id, DateTime Financial_Period_dtFom)
        {
            //bool ShowBalanceZero,
            string mysql = "";
            //operations opp = new operations();

            //mysql = "Delete From Account_Entry_Master_Balance_MezanTemp";
            //opp.insert_me(mysql);
            DateTime dtFomPrev = Financial_Period_dtFom;
            TimeSpan ts = new TimeSpan(1, 0, 0, 0);
            DateTime dtToPrev = dtFom.Subtract(ts);


            mysql = mysql + " truncate Table  Account_Entry_Master_TransactionTemp";
            if (IS_Excute_All == false)
            {
                opp.insert_me(mysql);
                mysql = "";
            }




            ////////////////////////////////////////////////
            /////إضافة الحسابات التى لها حركات  فى جدول   الحركات المؤقت
            mysql = mysql + @" INSERT INTO Account_Entry_Master_TransactionTemp
                         (  Last_Balance_Zero, Submain_Account_ID, Account_Name, Final_Fist_Balance_Madin, Final_Fist_Balance_Da2n, Final_Last_Balance_Madin, 
                         Final_Last_Balance_Da2n, Final_Transaction_Madin, Final_Transaction_Da2n, SubAccount_NO, Code, Account_Code, MainAccount_Name, MainAccount_NO, 
                         Main_Account_Id, MainCode,   
                          Prev_Import_Value, Prev_Export_Value,Equivalent_Account_Currency_Id)";

            mysql = mysql + @"  SELECT        1 AS Last_Balance_Zero, Account_Entry_Details.Submain_Account_ID, Account_Entry_Details.Account_Name, 0.0 AS Final_Fist_Balance_Madin, 0.0 AS Final_Fist_Balance_Da2n, 
                         0.0 AS Final_Last_Balance_Madin, 0.0 AS Final_Last_Balance_Da2n,
  0.0 AS Final_Transaction_Madin, 0.0 AS Final_Transaction_Da2n,

Submain_Account.SubAccount_NO, Submain_Account.Code, Submain_Account.Account_Code, 
                         MainSubmain_Account.SubAccount_Name AS MainAccount_Name, MainSubmain_Account.SubAccount_NO AS MainAccount_NO, Submain_Account.Main_Account_Id, 
                         MainSubmain_Account.SubAccount_NO AS MainCode,  
                          
                             ";
            if (Entry_Currency_Id > 0)
            {
                mysql = mysql + @" 
SUM(Account_Entry_Details.Import_Value) AS Final_Import_Value,
SUM(Account_Entry_Details.Export_Value) AS Final_Export_Value , ";
            }
            if (Entry_Currency_Id == 0)
            {
                mysql = mysql + @"  
SUM(Account_Entry_Details.Equivalent_Import_Value) AS Final_Import_Value, 
SUM(Account_Entry_Details.Equivalent_Export_Value) AS Final_Export_Value , ";
            }
            mysql = mysql + " " + Common.Entry_Currency_Id + " AS Equivalent_Account_Currency_Id ";

            mysql = mysql + @" FROM            Account_Entry_Details INNER JOIN
                         Account_Entry_List ON Account_Entry_Details.Account_Entry_List_ID = Account_Entry_List.Id INNER JOIN
                         Submain_Account ON Account_Entry_Details.Submain_Account_ID = Submain_Account.Id INNER JOIN
                         Submain_Account AS MainSubmain_Account ON Submain_Account.Main_Account_Id = MainSubmain_Account.Id ";

            mysql = mysql + " WHERE  (Account_Entry_List.Entry_No IS NOT  NULL) AND  (Account_Entry_List.Entry_Date BETWEEN " + operations.Get_DateToInsert(dtFomPrev) + "  AND  " + operations.Get_DateToInsert(dtToPrev) + ")";

            //  mysql = mysql + " WHERE     (Account_Entry_List.Entry_No IS NOT  NULL) AND  (Account_Entry_List.Entry_Date >= " + operations.Get_DateToInsert(dtFom) + ") AND (Account_Entry_List.Entry_Date <= " + operations.Get_DateToInsert(dtTo) + ")  ";
            //  mysql = mysql + " AND (Account_Entry_Master_Balance.Equivalent_Account_Currency_Id = " + Entry_Currency_Id + ") ";

            if (Submain_Account_ID > 0)
            {
                mysql = mysql + " AND    (Account_Entry_Details.Submain_Account_ID  = " + Submain_Account_ID + ") ";
            }
            if (Branch_Id > 0)
            {
                mysql = mysql + " AND    (Account_Entry_List.Branch_Id_Entry  = " + Branch_Id + ") ";
            }
            if (Entry_Currency_Id > 0)
            {
                mysql = mysql + " AND (Account_Entry_List.Entry_Currency_Id = " + Entry_Currency_Id + ") ";
            }




            //mysql = mysql + " GROUP BY Account_Entry_List.Branch_Id_Entry,Account_Entry_Details.Submain_Account_ID, Account_Entry_Details.Account_Name, Currency_Factor.Currency_Primary_Name, Currency_Factor.Currency_Secondary_Name, Currency_Factor.Factor, Submain_Account.SubAccount_NO, Submain_Account.Code, Submain_Account.Account_Code,  Submain_Account.Currency_Id";
            mysql = mysql + @" GROUP BY Account_Entry_List.Branch_Id_Entry, Account_Entry_Details.Submain_Account_ID, Account_Entry_Details.Account_Name, Submain_Account.SubAccount_NO, 
                         Submain_Account.Code, Submain_Account.Account_Code, Submain_Account.Currency_Id, MainSubmain_Account.SubAccount_Name, 
                         MainSubmain_Account.SubAccount_NO, Submain_Account.Main_Account_Id, MainSubmain_Account.Code 
                         ";

            mysql = mysql + @" ORDER BY MainSubmain_Account.Code   ";


            if (IS_Excute_All == false)
            {
                opp.insert_me(mysql);
                mysql = "";
            }




            ///تعديل حركات الحساب من جدول الارصدة المؤقت المؤقت الى جدول
            //////الحركات المؤقت
            mysql = mysql + @" UPDATE       Account_Entry_Master_Balance_MezanTemp
SET                Prev_Import_Value = Account_Entry_Master_TransactionTemp.Prev_Import_Value, 
                         Prev_Export_Value = Account_Entry_Master_TransactionTemp.Prev_Export_Value
FROM            Account_Entry_Master_Balance_MezanTemp INNER JOIN
                         Account_Entry_Master_TransactionTemp ON 
                         Account_Entry_Master_Balance_MezanTemp.Submain_Account_ID = Account_Entry_Master_TransactionTemp.Submain_Account_ID";
            if (IS_Excute_All == false)
            {
                opp.insert_me(mysql);
                mysql = "";
            }

            return mysql;
        }

        public string Get_Mezan_Morgaa_ByPeiodNewCostCenterUpdate(ref operations opp, bool IS_Excute_All, long Branch_Id, DateTime dtFom, DateTime dtTo, long CostCenter_Id, long Entry_Currency_Id, long Year_Financial_Period_Id, DateTime Financial_Period_dtFom)
        {
            //bool ShowBalanceZero,
            string mysql = "";
            //operations opp = new operations();

            //mysql = "Delete From Account_Entry_Master_Balance_MezanTemp";
            //opp.insert_me(mysql);
            DateTime dtFomPrev = Financial_Period_dtFom;
            TimeSpan ts = new TimeSpan(1, 0, 0, 0);
            DateTime dtToPrev = dtFom.Subtract(ts);


            mysql = mysql + " truncate Table  Account_Entry_Master_TransactionCostCenterTemp ";
            if (IS_Excute_All == false)
            {
                opp.insert_me(mysql);
                mysql = "";
            }




            ////////////////////////////////////////////////
            /////إضافة الحسابات التى لها حركات  فى جدول   الحركات المؤقت
            mysql = mysql + @" INSERT INTO Account_Entry_Master_TransactionCostCenterTemp
                         (Last_Balance_Zero, Submain_Account_ID, Account_Name, Final_Fist_Balance_Madin, Final_Fist_Balance_Da2n, Final_Last_Balance_Madin, Final_Last_Balance_Da2n, Final_Transaction_Madin, 
                         Final_Transaction_Da2n, SubAccount_NO, Code, Account_Code, MainAccount_Name, MainAccount_NO, Main_Account_Id, MainCode, Prev_Import_Value, Prev_Export_Value) ";

            mysql = mysql + @"  SELECT        1 AS Last_Balance_Zero, Account_Entry_Details.Submain_Account_ID, Account_Entry_Details.Account_Name, 0.0 AS Final_Fist_Balance_Madin, 0.0 AS Final_Fist_Balance_Da2n, 
                         0.0 AS Final_Last_Balance_Madin, 0.0 AS Final_Last_Balance_Da2n, 0.0 AS Final_Transaction_Madin, 0.0 AS Final_Transaction_Da2n, Cost_Center_Tree.Code_Str, Cost_Center_Tree.Code, 
                         Cost_Center_Tree.Code_Str AS Expr1, Cost_Center_Tree_Main.Name AS MainAccount_Name, Cost_Center_Tree_Main.Code_Str AS MainAccount_NO, Cost_Center_Tree.Parent_Id, 
                         Cost_Center_Tree_Main.Code AS MainCode,
                          
                             ";
            if (Entry_Currency_Id > 0)
            {
                mysql = mysql + @" 
SUM(Account_Entry_Details.Import_Value) AS Final_Import_Value,
SUM(Account_Entry_Details.Export_Value) AS Final_Export_Value ";
            }
            if (Entry_Currency_Id == 0)
            {
                mysql = mysql + @"  
SUM(Account_Entry_Details.Equivalent_Import_Value) AS Final_Import_Value, 
SUM(Account_Entry_Details.Equivalent_Export_Value) AS Final_Export_Value 
  ";
            }
            mysql = mysql + @" FROM            Account_Entry_Details INNER JOIN
                         Account_Entry_List ON Account_Entry_Details.Account_Entry_List_ID = Account_Entry_List.Id INNER JOIN
                         Cost_Center_Tree ON Account_Entry_Details.CostCenter_Id = Cost_Center_Tree.Id INNER JOIN
                         Cost_Center_Tree AS Cost_Center_Tree_Main ON Cost_Center_Tree.Parent_Id = Cost_Center_Tree_Main.Id ";

            mysql = mysql + " WHERE  (Account_Entry_List.Entry_No IS NOT  NULL) AND  (Account_Entry_List.Entry_Date BETWEEN " + operations.Get_DateToInsert(dtFomPrev) + "  AND  " + operations.Get_DateToInsert(dtToPrev) + ")";

            //  mysql = mysql + " WHERE     (Account_Entry_List.Entry_No IS NOT  NULL) AND  (Account_Entry_List.Entry_Date >= " + operations.Get_DateToInsert(dtFom) + ") AND (Account_Entry_List.Entry_Date <= " + operations.Get_DateToInsert(dtTo) + ")  ";
            //  mysql = mysql + " AND (Account_Entry_Master_Balance.Equivalent_Account_Currency_Id = " + Entry_Currency_Id + ") ";

            if (CostCenter_Id > 0)
            {
                mysql = mysql + " AND    (Account_Entry_Details.CostCenter_Id  = " + CostCenter_Id + ") ";
            }
            if (Branch_Id > 0)
            {
                mysql = mysql + " AND    (Account_Entry_List.Branch_Id_Entry  = " + Branch_Id + ") ";
            }
            if (Entry_Currency_Id > 0)
            {
                mysql = mysql + " AND (Account_Entry_List.Entry_Currency_Id = " + Entry_Currency_Id + ") ";
            }




            //mysql = mysql + " GROUP BY Account_Entry_List.Branch_Id_Entry,Account_Entry_Details.Submain_Account_ID, Account_Entry_Details.Account_Name, Currency_Factor.Currency_Primary_Name, Currency_Factor.Currency_Secondary_Name, Currency_Factor.Factor, Submain_Account.SubAccount_NO, Submain_Account.Code, Submain_Account.Account_Code,  Submain_Account.Currency_Id";
            mysql = mysql + @" GROUP BY Account_Entry_List.Branch_Id_Entry, Account_Entry_Details.Submain_Account_ID, Account_Entry_Details.Account_Name, Submain_Account.SubAccount_NO, 
                         Submain_Account.Code, Submain_Account.Account_Code, Submain_Account.Currency_Id, MainSubmain_Account.SubAccount_Name, 
                         MainSubmain_Account.SubAccount_NO, Submain_Account.Main_Account_Id, MainSubmain_Account.Code 
                         ";

            mysql = mysql + @" ORDER BY MainSubmain_Account.Code   ";


            if (IS_Excute_All == false)
            {
                opp.insert_me(mysql);
                mysql = "";
            }




            ///تعديل حركات الحساب من جدول الارصدة المؤقت المؤقت الى جدول
            //////الحركات المؤقت
            mysql = mysql + @"  UPDATE       Account_Entry_Master_Balance_CostCenterTemp
SET                Prev_Import_Value = Account_Entry_Master_TransactionCostCenterTemp.Prev_Import_Value, Prev_Export_Value = Account_Entry_Master_TransactionCostCenterTemp.Prev_Export_Value
FROM            Account_Entry_Master_Balance_CostCenterTemp INNER JOIN
                         Account_Entry_Master_TransactionCostCenterTemp ON Account_Entry_Master_Balance_CostCenterTemp.Submain_Account_ID = Account_Entry_Master_TransactionCostCenterTemp.Submain_Account_ID
";
            if (IS_Excute_All == false)
            {
                opp.insert_me(mysql);
                mysql = "";
            }

            return mysql;
        }

        public void Get_Mezan_Morgaa_ByPeiodNewNewPrev(bool ShowBalanceZero, long Branch_Id, DateTime dtFom, DateTime dtTo, long Submain_Account_ID, long Entry_Currency_Id, long Year_Financial_Period_Id)
        {
            string mysql = "";
            operations opp = new operations();

            mysql = @" UPDATE       Account_Entry_Master_Balance_MezanTemp SET    Prev_Import_Value = ";
            mysql = mysql + @"  (SELECT   ";
            if (Entry_Currency_Id > 0)
            {
                mysql = mysql + @" 
SUM(Account_Entry_Details.Import_Value) AS Final_Import_Value";

            }
            if (Entry_Currency_Id == 0)
            {
                mysql = mysql + @"  
SUM(Account_Entry_Details.Equivalent_Import_Value) AS Final_Import_Value 
  ";
            }
            mysql = mysql + @"  FROM  Account_Entry_Details INNER JOIN Account_Entry_List ON Account_Entry_Details.Account_Entry_List_ID = Account_Entry_List.Id ";
            mysql = mysql + " WHERE     (Account_Entry_List.Entry_No IS NOT  NULL) AND  (Account_Entry_List.Entry_Date >= " + operations.Get_DateToInsert(dtFom) + ") AND (Account_Entry_List.Entry_Date <= " + operations.Get_DateToInsert(dtTo) + ") AND     (Account_Entry_Master_Balance.Year_Financial_Period_Id = " + Year_Financial_Period_Id + ")";
            mysql = mysql + " AND    (Account_Entry_Details.Submain_Account_ID  = " + Submain_Account_ID + ") ";

            if (Branch_Id > 0)
            {
                mysql = mysql + " AND    (Account_Entry_List.Branch_Id_Entry  = " + Branch_Id + ") ";
            }
            if (Entry_Currency_Id > 0)
            {
                mysql = mysql + " AND (Account_Entry_List.Entry_Currency_Id = " + Entry_Currency_Id + ") ";
            }

            mysql = mysql + "   AND (Account_Entry_Details.Submain_Account_ID = Account_Entry_Master_Balance_MezanTemp.Submain_Account_ID) )";
            opp.insert_me(mysql);

            //////////////////////////

            mysql = @" UPDATE       Account_Entry_Master_Balance_MezanTemp SET    Prev_Export_Value = ";
            mysql = mysql + @"  (SELECT   ";
            if (Entry_Currency_Id > 0)
            {
                mysql = mysql + @" 
 
SUM(Account_Entry_Details.Export_Value) AS Final_Export_Value ";
            }
            if (Entry_Currency_Id == 0)
            {
                mysql = mysql + @"  
 
SUM(Account_Entry_Details.Equivalent_Export_Value) AS Final_Export_Value 
  ";
            }
            mysql = mysql + @"  FROM  Account_Entry_Details INNER JOIN Account_Entry_List ON Account_Entry_Details.Account_Entry_List_ID = Account_Entry_List.Id ";
            mysql = mysql + " WHERE     (Account_Entry_List.Entry_No IS NOT  NULL) AND  (Account_Entry_List.Entry_Date >= " + operations.Get_DateToInsert(dtFom) + ") AND (Account_Entry_List.Entry_Date <= " + operations.Get_DateToInsert(dtTo) + ") AND     (Account_Entry_Master_Balance.Year_Financial_Period_Id = " + Year_Financial_Period_Id + ")";
            mysql = mysql + " AND    (Account_Entry_Details.Submain_Account_ID  = " + Submain_Account_ID + ") ";

            if (Branch_Id > 0)
            {
                mysql = mysql + " AND    (Account_Entry_List.Branch_Id_Entry  = " + Branch_Id + ") ";
            }
            if (Entry_Currency_Id > 0)
            {
                mysql = mysql + " AND (Account_Entry_List.Entry_Currency_Id = " + Entry_Currency_Id + ") ";
            }

            mysql = mysql + "   AND (Account_Entry_Details.Submain_Account_ID = Account_Entry_Master_Balance_MezanTemp.Submain_Account_ID) )";
            opp.insert_me(mysql);

        }
        public void Get_Mezan_Morgaa_ByPeiodNewNew_(bool ShowBalanceZero, long Branch_Id, DateTime dtFom, DateTime dtTo, long Submain_Account_ID, long Entry_Currency_Id, long Year_Financial_Period_Id)
        {
            string mysql = "";
            operations opp = new operations();

            mysql = @"INSERT INTO Account_Entry_Master_Balance_MezanTemp
                         (  Last_Balance_Zero, Submain_Account_ID, Account_Name, Final_Fist_Balance_Madin, Final_Fist_Balance_Da2n, Final_Last_Balance_Madin, 
                         Final_Last_Balance_Da2n, Final_Transaction_Madin, Final_Transaction_Da2n, SubAccount_NO, Code, Account_Code, MainAccount_Name, MainAccount_NO, 
                         Main_Account_Id, MainCode, Equivalent_First_Balance_Madin, Equivalent_First_Balance_Da2n, First_Balance_Madin, First_Balance_Da2n, 
                         Equivalent_Account_Currency_Id,Head_Main_Account, Final_Import_Value, Final_Export_Value)";

            mysql = mysql + @"  SELECT        1 AS Last_Balance_Zero, Account_Entry_Details.Submain_Account_ID, Account_Entry_Details.Account_Name, 0.0 AS Final_Fist_Balance_Madin, 0.0 AS Final_Fist_Balance_Da2n, 
                         0.0 AS Final_Last_Balance_Madin, 0.0 AS Final_Last_Balance_Da2n,
  0.0 AS Final_Transaction_Madin, 0.0 AS Final_Transaction_Da2n,

Submain_Account.SubAccount_NO, Submain_Account.Code, Submain_Account.Account_Code, 
                         MainSubmain_Account.SubAccount_Name AS MainAccount_Name, MainSubmain_Account.SubAccount_NO AS MainAccount_NO, Submain_Account.Main_Account_Id, 
                         MainSubmain_Account.SubAccount_NO AS MainCode, Account_Entry_Master_Balance.Equivalent_First_Balance_Madin, 
                         Account_Entry_Master_Balance.Equivalent_First_Balance_Da2n, Account_Entry_Master_Balance.First_Balance_Madin, 
                         Account_Entry_Master_Balance.First_Balance_Da2n, Account_Entry_Master_Balance.Equivalent_Account_Currency_Id , Submain_Account.Head_Main_Account, ";
            if (Entry_Currency_Id > 0)
            {
                mysql = mysql + @" 
SUM(Account_Entry_Details.Import_Value) AS Final_Import_Value,
SUM(Account_Entry_Details.Export_Value) AS Final_Export_Value ";
            }
            if (Entry_Currency_Id == 0)
            {
                mysql = mysql + @"  
SUM(Account_Entry_Details.Equivalent_Import_Value) AS Final_Import_Value, 
SUM(Account_Entry_Details.Equivalent_Export_Value) AS Final_Export_Value 
  ";
            }
            mysql = mysql + @" FROM            Account_Entry_Details INNER JOIN
                         Account_Entry_List ON Account_Entry_Details.Account_Entry_List_ID = Account_Entry_List.Id INNER JOIN
                         Submain_Account ON Account_Entry_Details.Submain_Account_ID = Submain_Account.Id INNER JOIN
                         Submain_Account AS MainSubmain_Account ON Submain_Account.Main_Account_Id = MainSubmain_Account.Id INNER JOIN
                         Account_Entry_Master_Balance ON Account_Entry_Details.Submain_Account_ID = Account_Entry_Master_Balance.Submain_Account_ID AND 
                         Account_Entry_List.Branch_Id_Entry = Account_Entry_Master_Balance.Branch_Id ";


            mysql = mysql + " WHERE     (Account_Entry_List.Entry_No IS NOT  NULL) AND  (Account_Entry_List.Entry_Date >= " + operations.Get_DateToInsert(dtFom) + ") AND (Account_Entry_List.Entry_Date <= " + operations.Get_DateToInsert(dtTo) + ") AND     (Account_Entry_Master_Balance.Year_Financial_Period_Id = " + Year_Financial_Period_Id + ")";
            mysql = mysql + " AND (Account_Entry_Master_Balance.Equivalent_Account_Currency_Id = " + Entry_Currency_Id + ") ";

            if (Submain_Account_ID > 0)
            {
                mysql = mysql + " AND    (Account_Entry_Details.Submain_Account_ID  = " + Submain_Account_ID + ") ";
            }
            if (Branch_Id > 0)
            {
                mysql = mysql + " AND    (Account_Entry_List.Branch_Id_Entry  = " + Branch_Id + ") ";
            }
            if (Entry_Currency_Id > 0)
            {
                mysql = mysql + " AND (Account_Entry_List.Entry_Currency_Id = " + Entry_Currency_Id + ") ";
            }




            //mysql = mysql + " GROUP BY Account_Entry_List.Branch_Id_Entry,Account_Entry_Details.Submain_Account_ID, Account_Entry_Details.Account_Name, Currency_Factor.Currency_Primary_Name, Currency_Factor.Currency_Secondary_Name, Currency_Factor.Factor, Submain_Account.SubAccount_NO, Submain_Account.Code, Submain_Account.Account_Code,  Submain_Account.Currency_Id";
            mysql = mysql + @" GROUP BY Account_Entry_List.Branch_Id_Entry, Account_Entry_Details.Submain_Account_ID, Account_Entry_Details.Account_Name, Submain_Account.SubAccount_NO, 
                         Submain_Account.Code, Submain_Account.Account_Code, Submain_Account.Currency_Id, MainSubmain_Account.SubAccount_Name, 
                         MainSubmain_Account.SubAccount_NO, Submain_Account.Main_Account_Id, MainSubmain_Account.Code, Account_Entry_Master_Balance.First_Balance_Da2n, 
                         Account_Entry_Master_Balance.First_Balance_Madin, Account_Entry_Master_Balance.Equivalent_First_Balance_Da2n, 
                         Account_Entry_Master_Balance.Equivalent_First_Balance_Madin, Account_Entry_Master_Balance.Year_Financial_Period_Id, 
                         Account_Entry_Master_Balance.Equivalent_Account_Currency_Id  ";

            mysql = mysql + @" ORDER BY MainSubmain_Account.Code   ";
            //, Submain_Account.Code


            opp.insert_me(mysql);




            //////////////////////////يجب حذف السطور بين المنطقه  التاليه بعد ضبط البرنامج  بعد برنامج الفنار
            ///  الحسابات التى لها رصيد اول المده وليس لها حركات للفتره المحدده
            #region  Region Must delete after  fanar GL

            //VALUES        (,,,,,,,,,,,,, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,0)";
            //
            //    Submain_Account.Account_Code, 
            //  Account_Entry_Master_Balance.First_Balance, Account_Entry_Master_Balance.Equivalent_First_Balance
            //, Last_Balance_Zero, Final_Fist_Balance_Madin, 
            //             Final_Fist_Balance_Da2n, Final_Last_Balance_Madin, Final_Last_Balance_Da2n, Final_Transaction_Madin, Final_Transaction_Da2n, 
            //             Equivalent_Account_Currency_Id, Final_Import_Value, Final_Export_Value, Main_Account_Id
            mysql = @"INSERT INTO Account_Entry_Master_Balance_MezanTemp
(Equivalent_First_Balance_Madin,
Equivalent_First_Balance_Da2n,
First_Balance_Madin,
First_Balance_Da2n,
Submain_Account_ID,
Account_Name, 
SubAccount_NO,
Code, 
Account_Code,
Main_Account_Id,
MainAccount_Name,
MainAccount_NO,
MainCode,
Head_Main_Account,Equivalent_Account_Currency_Id) ";



            mysql = mysql + @" SELECT     
Account_Entry_Master_Balance.Equivalent_First_Balance_Madin, 
 Account_Entry_Master_Balance.Equivalent_First_Balance_Da2n,
 Account_Entry_Master_Balance.First_Balance_Madin, 
 Account_Entry_Master_Balance.First_Balance_Da2n ,
Submain_Account.Id,
Submain_Account.SubAccount_Name  ,
Submain_Account.SubAccount_NO, 
Submain_Account.Code,
 Submain_Account.Account_Code,
Submain_Account_1.SubAccount_Id As Main_Account_Id,
 Submain_Account_1.SubAccount_Name AS MainAccount_Name,
Submain_Account_1.SubAccount_NO AS MainAccount_NO,
Submain_Account_1.SubAccount_NO AS MainCode ,
Submain_Account.Head_Main_Account,
Account_Entry_Master_Balance.Equivalent_Account_Currency_Id


 
FROM            Account_Entry_Master_Balance INNER JOIN
                         Submain_Account ON Account_Entry_Master_Balance.Submain_Account_ID = Submain_Account.Id INNER JOIN
                         Submain_Account AS Submain_Account_1 ON Submain_Account.Main_Account_Id = Submain_Account_1.SubAccount_Id ";

            mysql = mysql + @" WHERE      (Account_Entry_Master_Balance.Year_Financial_Period_Id = " + Year_Financial_Period_Id + ") AND (Account_Entry_Master_Balance.Equivalent_Account_Currency_Id = " + Entry_Currency_Id + ") ";
            if (ShowBalanceZero == false)
            {
                mysql = mysql + @" And (Account_Entry_Master_Balance.First_Balance <> 0)      ";
            }
            mysql = mysql + @" AND   (Account_Entry_Master_Balance.Submain_Account_ID NOT IN
                             (SELECT DISTINCT Account_Entry_Details.Submain_Account_ID
                                FROM            Account_Entry_Details INNER JOIN
                                                         Account_Entry_List ON Account_Entry_Details.Account_Entry_List_ID = Account_Entry_List.Id ";

            mysql = mysql + " Where     ";
            if (Entry_Currency_Id > 0)
            {
                mysql = mysql + "   (Account_Entry_List.Entry_Currency_Id = " + Entry_Currency_Id + ") AND ";
            }

            mysql = mysql + @"     (Account_Entry_List.Entry_No IS NOT  NULL) AND  (Account_Entry_List.Entry_Date >= " + operations.Get_DateToInsert(dtFom) + ") AND (Account_Entry_List.Entry_Date <= " + operations.Get_DateToInsert(dtTo) + ")))";

            //WHERE        (Account_Entry_List.Entry_Date >= CONVERT(DATETIME, '01/01/2014', 102)) AND (Account_Entry_List.Entry_No IS NOT NULL) AND 
            //                                            (Account_Entry_List.Entry_Date <= CONVERT(DATETIME, '12/31/2014', 102))))";

            opp.insert_me(mysql);
            //DataTable dt3 = opp.return_DataTable(mysql);


            #endregion


        }


        void Set_NewAccountInFirstBalanceZero(ref operations opp, long Branch_ID, long Year_Financial_Period_Id)
        {
            // التأكد من كل الحسابات فى شجرة الحسابات لها حساب فى جدول ارصدة الحسابات المؤقته
            string mysql = " truncate Table  Account_Entry_Master_BalanceFirstAccount_Temp ";

            mysql = " DROP TABLE IF EXISTS Account_Entry_Master_BalanceFirstAccount_Temp;";
            mysql = mysql + " CREATE TABLE [dbo].[Account_Entry_Master_BalanceFirstAccount_Temp]( [Id] [bigint] IDENTITY(1,1) NOT NULL, [Submain_Account_ID] [bigint] NULL, [SubAccount_Name] [nvarchar](500) NULL, [Account_Currency_Id] [bigint] NULL, [Cost_Center_Id] [bigint] NULL, [Cost_Center_Name] [nvarchar](500) NULL, [OperationDate] [datetime] NULL, [User_Id] [bigint] NULL, [Branch_Id] [bigint] NULL, [Insert_ServerDateTime] [datetime] NULL, [Update_ServerDateTime] [datetime] NULL, [EditOperationDate] [datetime] NULL, [Edit_User_Id] [bigint] NULL, [ProcessNo] [bigint] NULL )  ";
            opp.insert_me(mysql);

            mysql = mysql + @" INSERT INTO Account_Entry_Master_BalanceFirstAccount_Temp (Submain_Account_ID, SubAccount_Name, Account_Currency_Id)";
            mysql = mysql + @"SELECT DISTINCT Id, SubAccount_Name, Currency_Id ";
            mysql = mysql + @" FROM            Submain_Account ";
            mysql = mysql + @" WHERE        (Id NOT IN
                             (SELECT        MainSubmain_Account.Id AS MAinAccount_Id
                                FROM            Submain_Account AS Submain_Account_1 INNER JOIN
                                                         Submain_Account AS MainSubmain_Account ON Submain_Account_1.Main_Account_Id = MainSubmain_Account.Id)) AND 
                         (Id NOT IN
                             (SELECT        Submain_Account_ID
                                FROM            Account_Entry_Master_Balance
                                WHERE        (Year_Financial_Period_Id = " + Year_Financial_Period_Id + ") AND (Branch_Id = " + Branch_ID + "))) AND (Branch_ID = " + Branch_ID + ") OR  (Branch_ID = - 1)";

            //DataTable dt = opp.return_DataTable(mysql);
            opp.insert_me(mysql);

            mysql = "";
            CurrencyFactorsCLS_ cc = new CurrencyFactorsCLS_();

            // 1. جلب البيانات المتميزة من الجدول المؤقت مرة واحدة فقط خارج اللوب (لتجنب تكرار الـ Queries على الداتابيز)
            var tempItems = opp.db.Account_Entry_Master_BalanceFirstAccount_Temp
                .Select(x => new
                {
                    x.Submain_Account_ID,
                    x.SubAccount_Name,
                    x.Account_Currency_Id
                })
                .Distinct()
                .ToList();// إن لم تكن البيانات موجودة لن ندخل في اللوب أصلاً
            if (tempItems.Count > 0)
            {
                var newEntries = new List<Account_Entry_Master_Balance>();
                bool generateIdManually = Common.Use_Auto_Identity_Column == 0;

                // 1. Fetch all active currencies (using their Value column)
                List<int> activeCurrencyValues = opp.db.Currency
                    .AsNoTracking()
                    .Where(c => c.Active == 1)
                    .OrderBy(c => c.Value)
                    .Select(c => c.Value)
                    .ToList();

                // 2. معالجة البيانات حسب العملات المتعددة في الـ Memory
                //for (int equivalentCurrencyId = 0; equivalentCurrencyId < Common.Currency_Count; equivalentCurrencyId++)
                //{
                foreach (int equivalentCurrencyId in activeCurrencyValues)
                {
                    decimal factor = Common.RoundMe(cc.GetFactors(Common.Entry_Currency_Id, equivalentCurrencyId));

                    foreach (var item in tempItems)
                    {
                        var entry = new Account_Entry_Master_Balance
                        {
                            Submain_Account_ID = item.Submain_Account_ID,
                            SubAccount_Name = item.SubAccount_Name,
                            Account_Currency_Id = item.Account_Currency_Id,
                            Year_Financial_Period_Id = Year_Financial_Period_Id,
                            Branch_Id = Branch_ID,
                            Equivalent_Account_Currency_Id = equivalentCurrencyId,
                            Factor = factor
                        };

                        // إسناد الـ ID يدويًا فقط في حال عدم استخدام Auto Identity
                        if (generateIdManually)
                        {
                            entry.Id = CallSnowflakeIdGenerate.GenerateNewId();
                        }

                        newEntries.Add(entry);
                    }
                }

                // 3. إدراج جميع السجلات والحفظ دفعة واحدة (Bulk Insert) في استعلام واحد للـ Database
                opp.db.Account_Entry_Master_Balance.AddRange(newEntries);
                opp.db.SaveChanges();
            }

            //for (long Equivalent_Account_Currency_Id = 0; Equivalent_Account_Currency_Id < Common.Currency_Count; Equivalent_Account_Currency_Id++)
            //{
            //    decimal Factor = cc.GetFactors(Common.Entry_Currency_Id, Equivalent_Account_Currency_Id);
            //    Factor = Common.RoundMe(Factor);
            //    mysql = @" INSERT INTO Account_Entry_Master_Balance (Submain_Account_ID, SubAccount_Name, Account_Currency_Id, Year_Financial_Period_Id, Branch_Id, Equivalent_Account_Currency_Id,Factor)";
            //    mysql = mysql + @"SELECT DISTINCT  Submain_Account_ID, SubAccount_Name, Account_Currency_Id ," + Year_Financial_Period_Id + " AS  Year_Financial_Period_Id  ," + Branch_ID + " AS Branch_ID," + Equivalent_Account_Currency_Id + "  AS Equivalent_Account_Currency_Id, " + Factor + " AS Factor";
            //    mysql = mysql + @" FROM            Account_Entry_Master_BalanceFirstAccount_Temp ";
            //    opp.insert_me(mysql);

            //}

        }

        public void Check_NewAccountInFirstBalanceZero(long Branch_ID, long Year_Financial_Period_Id)
        {
            operations opp = new operations();
            //
            string mysql = " truncate Table  Account_Entry_Master_BalanceFirstAccount_Temp ";

            mysql = " DROP TABLE IF EXISTS Account_Entry_Master_BalanceFirstAccount_Temp;";
            mysql = mysql + " CREATE TABLE [dbo].[Account_Entry_Master_BalanceFirstAccount_Temp]( [Id] [bigint] IDENTITY(1,1) NOT NULL, [Submain_Account_ID] [bigint] NULL, [SubAccount_Name] [nvarchar](500) NULL, [Account_Currency_Id] [bigint] NULL, [Cost_Center_Id] [bigint] NULL, [Cost_Center_Name] [nvarchar](500) NULL, [OperationDate] [datetime] NULL, [User_Id] [bigint] NULL, [Branch_Id] [bigint] NULL, [Insert_ServerDateTime] [datetime] NULL, [Update_ServerDateTime] [datetime] NULL, [EditOperationDate] [datetime] NULL, [Edit_User_Id] [bigint] NULL, [ProcessNo] [bigint] NULL )  ";
            opp.insert_me(mysql);

            mysql = mysql + @" INSERT INTO Account_Entry_Master_BalanceFirstAccount_Temp (Submain_Account_ID, SubAccount_Name, Account_Currency_Id)";
            mysql = mysql + @"SELECT DISTINCT Id, SubAccount_Name, Currency_Id ";
            mysql = mysql + @" FROM            Submain_Account ";
            mysql = mysql + @" WHERE        (Id NOT IN
                             (SELECT        MainSubmain_Account.Id AS MAinAccount_Id
                                FROM            Submain_Account AS Submain_Account_1 INNER JOIN
                                                         Submain_Account AS MainSubmain_Account ON Submain_Account_1.Main_Account_Id = MainSubmain_Account.Id)) ";

            //AND 
            //             (Id NOT IN
            //                 (SELECT        Submain_Account_ID
            //                    FROM            Account_Entry_Master_Balance
            //                    WHERE        (Year_Financial_Period_Id = " + Year_Financial_Period_Id + ") AND (Branch_Id = " + Branch_ID + "))) AND (Branch_ID = " + Branch_ID + ") OR  (Branch_ID = - 1)";

            //DataTable dt = opp.return_DataTable(mysql);
            opp.insert_me(mysql);

            mysql = "";
            CurrencyFactorsCLS_ cc = new CurrencyFactorsCLS_();

            // 1. جلب البيانات المتميزة من الجدول المؤقت مرة واحدة فقط خارج اللوب (لتجنب تكرار الـ Queries على الداتابيز)
            var tempItems = opp.db.Account_Entry_Master_BalanceFirstAccount_Temp
                .Select(x => new
                {
                    x.Submain_Account_ID,
                    x.SubAccount_Name,
                    x.Account_Currency_Id

                })
                .Distinct()
                .ToList();// إن لم تكن البيانات موجودة لن ندخل في اللوب أصلاً
            if (tempItems.Count > 0)
            {
                var newEntries = new List<Account_Entry_Master_Balance>();
                bool generateIdManually = Common.Use_Auto_Identity_Column == 0;

                // 1. Fetch all active currencies (using their Value column)
                List<int> activeCurrencyValues = opp.db.Currency
                    .AsNoTracking()
                    .Where(c => c.Active == 1)
                    .OrderBy(c => c.Value)
                    .Select(c => c.Value)
                    .ToList();

                // 2. معالجة البيانات حسب العملات المتعددة في الـ Memory
                //for (int equivalentCurrencyId = 0; equivalentCurrencyId < Common.Currency_Count; equivalentCurrencyId++)
                //{
                foreach (int equivalentCurrencyId in activeCurrencyValues)
                {
                    decimal factor = Common.RoundMe(cc.GetFactors(Common.Entry_Currency_Id, equivalentCurrencyId));

                    foreach (var item in tempItems)
                    {
                        var aemb = opp.db.Account_Entry_Master_Balance.AsNoTracking().Where(a => a.Submain_Account_ID == item.Submain_Account_ID & a.Equivalent_Account_Currency_Id == equivalentCurrencyId & a.Branch_Id == Branch_ID & a.Year_Financial_Period_Id == Year_Financial_Period_Id).FirstOrDefault();
                        if (aemb != null)
                        {
                            continue;
                        }
                        var entry = new Account_Entry_Master_Balance
                        {
                            Submain_Account_ID = item.Submain_Account_ID,
                            SubAccount_Name = item.SubAccount_Name,
                            Account_Currency_Id = item.Account_Currency_Id,
                            Year_Financial_Period_Id = Year_Financial_Period_Id,
                            Branch_Id = Branch_ID,
                            Equivalent_Account_Currency_Id = equivalentCurrencyId,
                            Factor = factor,

                            // Decimal Fields Initialized to 0
                            First_Balance = 0m,
                            Equivalent_First_Balance = 0m,
                            First_Balance_Madin = 0m,
                            First_Balance_Da2n = 0m,
                            Madin_Value = 0m,
                            Da2n_Value = 0m,
                            Balance = 0m,
                            Balance_Madin = 0m,
                            Balance_Da2n = 0m,
                            Equivalent_First_Balance_Madin = 0m,
                            Equivalent_First_Balance_Da2n = 0m,
                            Equivalent_Madin_Value = 0m,
                            Equivalent_Da2n_Value = 0m,
                            Equivalent_Balance_Madin = 0m,
                            Equivalent_Balance_Da2n = 0m

                        };

                        // إسناد الـ ID يدويًا فقط في حال عدم استخدام Auto Identity
                        if (generateIdManually)
                        {
                            entry.Id = CallSnowflakeIdGenerate.GenerateNewId();
                        }

                        newEntries.Add(entry);
                    }
                }

                // 3. إدراج جميع السجلات والحفظ دفعة واحدة (Bulk Insert) في استعلام واحد للـ Database
                opp.db.Account_Entry_Master_Balance.AddRange(newEntries);
                opp.db.SaveChanges();
            }

            //for (long Equivalent_Account_Currency_Id = 0; Equivalent_Account_Currency_Id < Common.Currency_Count; Equivalent_Account_Currency_Id++)
            //{
            //    decimal Factor = cc.GetFactors(Common.Entry_Currency_Id, Equivalent_Account_Currency_Id);
            //    Factor = Common.RoundMe(Factor);
            //    mysql = @" INSERT INTO Account_Entry_Master_Balance (Submain_Account_ID, SubAccount_Name, Account_Currency_Id, Year_Financial_Period_Id, Branch_Id, Equivalent_Account_Currency_Id,Factor)";
            //    mysql = mysql + @"SELECT DISTINCT  Submain_Account_ID, SubAccount_Name, Account_Currency_Id ," + Year_Financial_Period_Id + " AS  Year_Financial_Period_Id  ," + Branch_ID + " AS Branch_ID," + Equivalent_Account_Currency_Id + "  AS Equivalent_Account_Currency_Id, " + Factor + " AS Factor";
            //    mysql = mysql + @" FROM            Account_Entry_Master_BalanceFirstAccount_Temp ";
            //    opp.insert_me(mysql);

            //}

        }

        void Set_NewAccountInFirstBalanceZeroCostCenter(ref operations opp, long Branch_Id, long Year_Financial_Period_Id)
        {
            //
            string mysql = " truncate Table  Account_Entry_Master_BalanceFirstAccount_Temp ";
            opp.insert_me(mysql);

            mysql = mysql + @" INSERT INTO Account_Entry_Master_BalanceFirstAccount_Temp (Cost_Center_Id,Cost_Center_Name,Submain_Account_ID, SubAccount_Name, Account_Currency_Id)";
            //mysql = mysql + @"SELECT DISTINCT Id, Name, 0 AS Currency_Id ";
            //mysql = mysql + @" FROM            Cost_Center_Tree "
            mysql = mysql + @" SELECT DISTINCT Cost_Center_Tree.Id, Cost_Center_Tree.Name, Account_Entry_Details.Submain_Account_ID, Submain_Account.SubAccount_Name, 0 AS Currency_Id
FROM            Cost_Center_Tree INNER JOIN
                         Account_Entry_Details ON Cost_Center_Tree.Id = Account_Entry_Details.CostCenter_Id INNER JOIN
                         Submain_Account ON Account_Entry_Details.Submain_Account_ID = Submain_Account.Id
WHERE        (Cost_Center_Tree.Id NOT IN
                             (SELECT        Cost_Center_Tree_Main.Id AS MAinAccount_Id
                                FROM            Cost_Center_Tree AS Cost_Center_Tree_1 INNER JOIN
                                                         Cost_Center_Tree AS Cost_Center_Tree_Main ON Cost_Center_Tree_1.Parent_Id = Cost_Center_Tree_Main.Id)) AND (Cost_Center_Tree.Id NOT IN
                             (SELECT        Submain_Account_ID
                                FROM            Account_Entry_Master_Balance_CostCenter
                                WHERE        (Year_Financial_Period_Id = " + Year_Financial_Period_Id + "))) ";

            //AND (Branch_ID = " + Branch_ID + ") OR  (Branch_ID = - 1)   AND (Branch_Id = " + Branch_ID + ")
            //
            //DataTable dt = opp.return_DataTable(mysql);
            opp.insert_me(mysql);

            mysql = "";
            // 1. Fetch all active currencies (using their Value column)
            List<int> activeCurrencyValues = opp.db.Currency
                .AsNoTracking()
                .Where(c => c.Active == 1)
                .OrderBy(c => c.Value)
                .Select(c => c.Value)
                .ToList();

            // 2. معالجة البيانات حسب العملات المتعددة في الـ Memory
            //for (int equivalentCurrencyId = 0; equivalentCurrencyId < Common.Currency_Count; equivalentCurrencyId++)
            //{
            foreach (long Equivalent_Account_Currency_Id in activeCurrencyValues)
            {
                //for (int i = 0; i < Common.Currency_Count; i++)
                //{
                //    for (long Equivalent_Account_Currency_Id = 0; Equivalent_Account_Currency_Id < Common.Currency_Count; Equivalent_Account_Currency_Id++)
                //{
                mysql = @" INSERT INTO Account_Entry_Master_Balance_CostCenter (Cost_Center_Id,Cost_Center_Name,Submain_Account_ID, SubAccount_Name, Account_Currency_Id, Year_Financial_Period_Id, Branch_Id, Equivalent_Account_Currency_Id)";
                mysql = mysql + @"SELECT DISTINCT  Cost_Center_Id,Cost_Center_Name,Submain_Account_ID, SubAccount_Name, Account_Currency_Id ," + Year_Financial_Period_Id + " AS  Year_Financial_Period_Id  ," + Branch_Id + " AS Branch_ID," + Equivalent_Account_Currency_Id + "  AS Equivalent_Account_Currency_Id";
                mysql = mysql + @" FROM            Account_Entry_Master_BalanceFirstAccount_Temp ";

                opp.insert_me(mysql);
                //Equivalent_Account_Currency_Id++;
            }

        }
        void Set_NewAccountInFirstBalanceZero(long Branch_Id, long Year_Financial_Period_Id)
        {
            operations opp = new operations();
            string mysql = " ";

            mysql = @"SELECT DISTINCT SubAccount_Id, SubAccount_Name, Currency_Id  ";
            mysql = mysql + @" FROM            Submain_Account ";
            mysql = mysql + @" WHERE        (SubAccount_Id NOT IN
                             (SELECT        MainSubmain_Account.Id AS MAinAccount_Id
                                FROM            Submain_Account AS Submain_Account_1 INNER JOIN
                                                         Submain_Account AS MainSubmain_Account ON Submain_Account_1.Main_Account_Id = MainSubmain_Account.Id)) AND 
                         (SubAccount_Id NOT IN
                             (SELECT        Submain_Account_ID
                                FROM            Account_Entry_Master_Balance
                                WHERE        (Year_Financial_Period_Id = " + Year_Financial_Period_Id + ") AND (Branch_Id = " + Branch_Id + "))) AND (Branch_ID = " + Branch_Id + ") OR  (Branch_ID = - 1)";


            DataTable dt = opp.return_DataTable(mysql);
            mysql = "";

            for (int x = 0; x < dt.Rows.Count; x++)
            {
                long Submain_Account_ID = long.Parse(dt.Rows[x]["SubAccount_Id"].ToString());

                string SubAccount_Name = dt.Rows[x]["SubAccount_Name"].ToString();
                long Account_Currency_Id = long.Parse(dt.Rows[x]["Currency_Id"].ToString()); ;

                for (long Equivalent_Account_Currency_Id = 0; Equivalent_Account_Currency_Id < 5; Equivalent_Account_Currency_Id++)
                {

                    mysql = mysql + @" INSERT INTO Account_Entry_Master_Balance (Submain_Account_ID, SubAccount_Name, Account_Currency_Id, Year_Financial_Period_Id, Branch_Id, Equivalent_Account_Currency_Id)";
                    mysql = mysql + @"  Values (" + Submain_Account_ID + ", '" + SubAccount_Name + "'," + Account_Currency_Id + "," + Year_Financial_Period_Id + "," + Branch_Id + "," + Equivalent_Account_Currency_Id + ") ";
                }
            }
            if (dt.Rows.Count > 0)
            {
                opp.insert_me(mysql);
            }
        }
        public DataTable Get_Mezan_Morgaa_ByPeiod(bool ShowBalanceZero, bool AnyDatePeriod, long Year_Financial_Period_Id, long Branch_Id, DateTime dtFom, DateTime dtTo, DateTime Financial_Period_dtFom, long Account_Currency_Id)
        {
            bool Is_Equivalent = true;
            bool Is_EquivalentFactor = false;

            long Submain_Account_ID = 0;
            Set_NewAccountInFirstBalanceZero(Branch_Id, Year_Financial_Period_Id);
            ////حساب اجمالى حركات الحسابات عن فترة
            DataTable dt = Get_Mezan_Morgaa_ByPeiod(Branch_Id, dtFom, dtTo, Submain_Account_ID, Account_Currency_Id, Year_Financial_Period_Id);


            for (int x = 0; x < dt.Rows.Count; x++)
            {
                decimal First_Balance_Madin = 0;
                decimal First_Balance_Da2n = 0;
                decimal Final_First_Balance_Madin = 0;
                decimal Final_First_Balance_Da2n = 0;
                decimal Final_Last_Balance_Madin = 0;
                decimal Final_Last_Balance_Da2n = 0;

                Submain_Account_ID = long.Parse(dt.Rows[x]["Submain_Account_ID"].ToString());
                string Account_Name = dt.Rows[x]["Account_Name"].ToString();
                ////قيم الحركات
                decimal Final_Import_Value = decimal.Parse(dt.Rows[x]["Final_Import_Value"].ToString());
                decimal Final_Export_Value = decimal.Parse(dt.Rows[x]["Final_Export_Value"].ToString());
                ////////////////////////////////////////////////////////////// 
                ////////حساب قيمة الحركات السابقه
                if (Submain_Account_ID == 30101010001)
                {

                    int vvv = 0;
                }
                decimal Prev_Import_Value = 0;
                decimal Prev_Export_Value = 0;
                if (AnyDatePeriod == false)
                {
                    if (dtFom > Financial_Period_dtFom)
                    {
                        //////////////حساب الفترة من تاريخ بدايه الفترة الماليه حتى تاريخ البحث الى
                        TimeSpan ts = new TimeSpan(1, 0, 0, 0);
                        DateTime New_dtTo = dtFom.Subtract(ts);
                        DataTable dt_Acc = Get_Mezan_Morgaa_ByPeiod(Branch_Id, Financial_Period_dtFom, New_dtTo, Submain_Account_ID, Account_Currency_Id, Year_Financial_Period_Id);

                        if (dt_Acc.Rows.Count > 0)
                        {
                            Prev_Import_Value = decimal.Parse(dt.Rows[0]["Final_Import_Value"].ToString());
                            Prev_Export_Value = decimal.Parse(dt.Rows[0]["Final_Export_Value"].ToString());

                        }
                    }
                    //////////////////////////////////////////////////////////////

                    //// الحصول على ارصده اول المده
                    // Get_FirstBalance_Period(Is_Equivalent, Is_EquivalentFactor, Year_Financial_Period_Id, Branch_Id, Submain_Account_ID, ref   First_Balance_Madin, ref   First_Balance_Da2n);
                    Get_FirstBalance_Period(dt, x, Is_Equivalent, Is_EquivalentFactor, ref First_Balance_Madin, ref First_Balance_Da2n);
                }
                ///////////////////////////////////////////
                decimal First_Balance = (First_Balance_Madin - First_Balance_Da2n) + (Prev_Import_Value - Prev_Export_Value);
                decimal Last_Balance = First_Balance + (Final_Import_Value - Final_Export_Value);

                if (First_Balance == 0)
                {
                    Final_First_Balance_Madin = 0;
                    Final_First_Balance_Da2n = 0;
                }
                else if (First_Balance > 0)
                {
                    Final_First_Balance_Madin = First_Balance;
                    Final_First_Balance_Da2n = 0;
                }
                else if (First_Balance < 0)
                {
                    First_Balance = First_Balance * -1;
                    Final_First_Balance_Madin = 0;
                    Final_First_Balance_Da2n = First_Balance;
                }

                int Last_Balance_Zero = 1;
                if (Last_Balance == 0)
                {
                    Final_Last_Balance_Madin = 0;
                    Final_Last_Balance_Da2n = 0;
                    Last_Balance_Zero = 0;
                }
                else if (Last_Balance > 0)
                {
                    Final_Last_Balance_Madin = Last_Balance;
                    Final_Last_Balance_Da2n = 0;
                }
                else if (Last_Balance < 0)
                {
                    Last_Balance = Last_Balance * -1;
                    Final_Last_Balance_Madin = 0;
                    Final_Last_Balance_Da2n = Last_Balance;
                }

                //////////ضبط رصيد اول المده
                dt.Rows[x]["Final_Fist_Balance_Madin"] = Final_First_Balance_Madin;
                dt.Rows[x]["Final_Fist_Balance_Da2n"] = Final_First_Balance_Da2n;


                ///////////////ضبط الرصيد النهائى للحساب  
                dt.Rows[x]["Final_Last_Balance_Madin"] = Final_Last_Balance_Madin;
                dt.Rows[x]["Final_Last_Balance_Da2n"] = Final_Last_Balance_Da2n;
                dt.Rows[x]["Last_Balance_Zero"] = Last_Balance_Zero;

            }
            DataView dv = dt.DefaultView;
            if (ShowBalanceZero == false)
            {

                dv.RowFilter = "Last_Balance_Zero = 1";
                //   Sort data

            }
            dv.Sort = "MainCode ASC";
            //   Convert back your sorted DataView to DataTable
            dt = dv.ToTable();
            return dt;
        }
        public void Get_Mezan_Morgaa_ByPeiodNewBackup2702205(bool IS_Set_NewAccountInFirstBalanceZero, bool ShowBalanceZero, bool AnyDatePeriod, long Year_Financial_Period_Id, long Branch_Id, DateTime dtFom, DateTime dtTo, DateTime Financial_Period_dtFom, long Account_Currency_Id)
        {
            //bool Is_Equivalent = true;
            //bool Is_EquivalentFactor = false;

            //long Submain_Account_ID = 0;
            //if (IS_Set_NewAccountInFirstBalanceZero == true)
            //{
            //    Set_NewAccountInFirstBalanceZero(Branch_Id, Year_Financial_Period_Id);
            //}

            //bool Is_To_Show_Prev = false;
            //if (AnyDatePeriod == false)
            //{
            //    if (dtFom > Financial_Period_dtFom)
            //    {
            //        Is_To_Show_Prev = true;
            //        //////////////حساب الفترة من تاريخ بدايه الفترة الماليه حتى تاريخ البحث الى

            //    }
            //}
            //    ////حساب اجمالى حركات الحسابات عن فترة
            //    ////////////DataTable dt = Get_Mezan_Morgaa_ByPeiodNew(Is_To_Show_Prev, ShowBalanceZero, Branch_Id, dtFom, dtTo, Submain_Account_ID, Account_Currency_Id, Year_Financial_Period_Id);

            //    //object TAllMadin = dt.Compute("Sum(Final_Import_Value)", "");
            //    //object TAllDa2n = dt.Compute("Sum(Final_Export_Value)", "");
            //    //decimal dif4 = (decimal)TAllMadin - (decimal)TAllDa2n;

            //    //for (int x = 0; x < dt.Rows.Count; x++)
            //    //{
            //    //    decimal First_Balance_Madin = 0;
            //    //    decimal First_Balance_Da2n = 0;
            //    //    decimal Final_First_Balance_Madin = 0;
            //    //    decimal Final_First_Balance_Da2n = 0;
            //    //    decimal Final_Last_Balance_Madin = 0;
            //    //    decimal Final_Last_Balance_Da2n = 0;

            //    //    decimal Final_Transaction_Madin = 0;
            //    //    decimal Final_Transaction_Da2n = 0;

            //    //    Submain_Account_ID = long.Parse(dt.Rows[x]["Submain_Account_ID"].ToString());
            //    //    if (Submain_Account_ID == 10105020005)
            //    //    {
            //    //        int ccc = 0;
            //    //    }
            //    //    string Account_Name = dt.Rows[x]["Account_Name"].ToString();
            //    //    ////قيم الحركات
            //    //    decimal Final_Import_Value = decimal.Parse(dt.Rows[x]["Final_Import_Value"].ToString());
            //    //    decimal Final_Export_Value = decimal.Parse(dt.Rows[x]["Final_Export_Value"].ToString());

            //    //    decimal Diff_Transaction = Final_Import_Value - Final_Export_Value;
            //    //    if (Diff_Transaction == 0)
            //    //    {
            //    //        Final_Transaction_Madin = 0;
            //    //        Final_Transaction_Da2n = 0;
            //    //    }
            //    //    else if (Diff_Transaction > 0)
            //    //    {

            //    //        Final_Transaction_Madin = Diff_Transaction;
            //    //        Final_Transaction_Da2n = 0;
            //    //    }
            //    //    else if (Diff_Transaction < 0)
            //    //    {
            //    //        Final_Transaction_Madin = 0;
            //    //        Final_Transaction_Da2n = Diff_Transaction * -1;
            //    //    }
            //    //    ////////////////////////////////////////////////////////////// 
            //    //    ////////حساب قيمة الحركات السابقه
            //    //    if (Submain_Account_ID == 10101010001)
            //    //    {

            //    //        int vvv = 0;
            //    //    }
            //    //    decimal Prev_Import_Value = 0;
            //    //    decimal Prev_Export_Value = 0;
            //    //    //if (AnyDatePeriod == false)
            //    //    //{
            //    //    //    if (dtFom > Financial_Period_dtFom)
            //    //    //    {
            //    //    //        ////////////////حساب الفترة من تاريخ بدايه الفترة الماليه حتى تاريخ البحث الى
            //    //    //        //TimeSpan ts = new TimeSpan(1, 0, 0, 0);
            //    //    //        //DateTime New_dtTo = dtFom.Subtract(ts);
            //    //    //        //DataTable dt_Acc = Get_Mezan_Morgaa_ByPeiodNew(ShowBalanceZero, Branch_Id, Financial_Period_dtFom, New_dtTo, Submain_Account_ID, Account_Currency_Id, Year_Financial_Period_Id);

            //    //    //        //if (dt_Acc.Rows.Count > 0)
            //    //    //        //{
            //    //    //        //    Prev_Import_Value = decimal.Parse(dt.Rows[0]["Final_Import_Value"].ToString());
            //    //    //        //    Prev_Export_Value = decimal.Parse(dt.Rows[0]["Final_Export_Value"].ToString());

            //    //    //        //}
            //    //    //    }
            //    //    //    //////////////////////////////////////////////////////////////

            //    //    //    //// الحصول على ارصده اول المده
            //    //    //    // Get_FirstBalance_Period(Is_Equivalent, Is_EquivalentFactor, Year_Financial_Period_Id, Branch_Id, Submain_Account_ID, ref   First_Balance_Madin, ref   First_Balance_Da2n);

            //    //    //}



            //    //    try
            //    //    {
            //    //        First_Balance_Madin = decimal.Parse(dt.Rows[x]["First_Balance_Madin"].ToString());
            //    //    }
            //    //    catch { }

            //    //    try
            //    //    {
            //    //        First_Balance_Da2n = decimal.Parse(dt.Rows[x]["First_Balance_Da2n"].ToString());
            //    //    }
            //    //    catch { }
            //    //    ///////////////////////////////////////////
            //    //    decimal First_Balance = (First_Balance_Madin - First_Balance_Da2n) + (Prev_Import_Value - Prev_Export_Value);
            //    //    decimal Last_Balance = First_Balance + (Final_Import_Value - Final_Export_Value);

            //    //    if (First_Balance == 0)
            //    //    {
            //    //        Final_First_Balance_Madin = 0;
            //    //        Final_First_Balance_Da2n = 0;
            //    //    }
            //    //    else if (First_Balance > 0)
            //    //    {
            //    //        Final_First_Balance_Madin = First_Balance;
            //    //        Final_First_Balance_Da2n = 0;
            //    //    }
            //    //    else if (First_Balance < 0)
            //    //    {
            //    //        First_Balance = First_Balance * -1;
            //    //        Final_First_Balance_Madin = 0;
            //    //        Final_First_Balance_Da2n = First_Balance;
            //    //    }
            //    //    ////////////////////////////////////////////
            //    //    int Last_Balance_Zero = 1;
            //    //    if (Last_Balance == 0)
            //    //    {
            //    //        Final_Last_Balance_Madin = 0;
            //    //        Final_Last_Balance_Da2n = 0;
            //    //        Last_Balance_Zero = 0;
            //    //    }
            //    //    else if (Last_Balance > 0)
            //    //    {
            //    //        Final_Last_Balance_Madin = Last_Balance;
            //    //        Final_Last_Balance_Da2n = 0;
            //    //    }
            //    //    else if (Last_Balance < 0)
            //    //    {
            //    //        Last_Balance = Last_Balance * -1;
            //    //        Final_Last_Balance_Madin = 0;
            //    //        Final_Last_Balance_Da2n = Last_Balance;
            //    //    }

            //    //    //////////ضبط رصيد اول المده
            //    //    dt.Rows[x]["Final_Fist_Balance_Madin"] = Final_First_Balance_Madin;
            //    //    dt.Rows[x]["Final_Fist_Balance_Da2n"] = Final_First_Balance_Da2n;

            //    //    //////ضبط رصيد الخركة

            //    //    dt.Rows[x]["Final_Transaction_Madin"] = Final_Transaction_Madin;
            //    //    dt.Rows[x]["Final_Transaction_Da2n"] = Final_Transaction_Da2n;
            //    //    ///////////////ضبط الرصيد النهائى للحساب  
            //    //    dt.Rows[x]["Final_Last_Balance_Madin"] = Final_Last_Balance_Madin;
            //    //    dt.Rows[x]["Final_Last_Balance_Da2n"] = Final_Last_Balance_Da2n;
            //    //    dt.Rows[x]["Last_Balance_Zero"] = Last_Balance_Zero;

            //    //}
            //    //TAllMadin = dt.Compute("Sum(Final_Import_Value)", "");
            //    //TAllDa2n = dt.Compute("Sum(Final_Export_Value)", "");
            //    //dif4 = (decimal)TAllMadin - (decimal)TAllDa2n;

            //    DataView dv = dt.DefaultView;
            //    //if (ShowBalanceZero == false)
            //    //{

            //    //    dv.RowFilter = "Last_Balance_Zero = 1";
            //    //    //   Sort data

            //    //}
            //    dv.Sort = "MainCode ASC";
            //    //   Convert back your sorted DataView to DataTable
            //    dt = dv.ToTable();
            //    //TAllMadin = dt.Compute("Sum(Final_Import_Value)", "");
            //    //  TAllDa2n = dt.Compute("Sum(Final_Export_Value)", "");
            //    //  dif4 = (decimal)TAllMadin - (decimal)TAllDa2n;
            //    return dt;




        }
         
        public DataTable ShowFirstBalance_Mezan_Morgaa_ByFirstPeiod(ref int Excp, ref string Msg, bool Is_To_Show_Prev, int Show_AccountWithBalance, long Branch_Id, long Submain_Account_ID, long Entry_Currency_Id, long Year_Financial_Period_Id, bool IS_Set_NewAccountInFirstBalanceZero, bool AnyDatePeriod, DateTime Financial_Period_dtFom)
        {
            FirstBalance_CLS firstBalance_CLS = new FirstBalance_CLS();
            operations opp = new operations();
            firstBalance_CLS.BalanceOpeningBalanceEquity(ref opp, Year_Financial_Period_Id, Branch_Id, Entry_Currency_Id);
            return Get_FirstBalance_By_FirstPeiod(ref Excp, ref Msg, ref opp, Is_To_Show_Prev, Show_AccountWithBalance, Branch_Id, Submain_Account_ID, Entry_Currency_Id, Year_Financial_Period_Id, IS_Set_NewAccountInFirstBalanceZero, AnyDatePeriod, Financial_Period_dtFom);
        }
        public DataTable Get_FirstBalance_By_FirstPeiod(ref int Excp, ref string Msg, ref operations opp, bool Is_To_Show_Prev, int Show_AccountWithBalance, long Branch_Id, long Submain_Account_ID, long Entry_Currency_Id, long Year_Financial_Period_Id, bool IS_Set_NewAccountInFirstBalanceZero, bool AnyDatePeriod, DateTime Financial_Period_dtFom)
        {
            //bool ShowBalanceZero,
            string mysql = "";

            DataTable dt = new DataTable();
            try
            {
                Set_NewAccountInFirstBalanceZero(ref opp, Branch_Id, Year_Financial_Period_Id);
                //SetOptionGet_Mezan_Morgaa_ByPeiodNew(ref opp, Is_To_Show_Prev, IS_Set_NewAccountInFirstBalanceZero, AnyDatePeriod, Year_Financial_Period_Id, Branch_Id, dtFom, Financial_Period_dtFom);

                // SetTemp_Mezan_Morgaa_ByPeiodNew(ref opp, Is_To_Show_Prev, Branch_Id, dtFom, dtTo, Submain_Account_ID, Entry_Currency_Id, Year_Financial_Period_Id, IS_Set_NewAccountInFirstBalanceZero, AnyDatePeriod, Financial_Period_dtFom);

                mysql = @"SELECT   mezan.First_Balance_Madin AS Final_Fist_Balance_Madin, mezan.First_Balance_Da2n AS Final_Fist_Balance_Da2n, mezan.Madin_Value AS Final_Transaction_Madin, mezan.Da2n_Value AS Final_Transaction_Da2n
,mezan.* ,  Submain_Account.Account_Customer_Type_ID, Submain_Account.Account_Customer_Type_Value  
, MainSubmain_Account.SubAccount_NO AS MainCode, 
                         MainSubmain_Account.SubAccount_Name AS MainAccount_Name, Submain_Account.SubAccount_Name AS Account_Name, Submain_Account.Account_Code
,  
    -- معالجة اسم العملة لكود 0 (إجمالي المكافئ)
    CASE 
        WHEN mezan.Equivalent_Account_Currency_Id = 0 THEN N'إجمالي المكافئ  '
        ELSE Currency.Currency_Name 
    END AS Currency_Name,
    Currency.Currency_Name_EN

 
                                                   
FROM            Account_Entry_Master_Balance AS mezan INNER JOIN
                         Submain_Account ON mezan.Submain_Account_ID = Submain_Account.Id INNER JOIN
                         Submain_Account AS MainSubmain_Account ON Submain_Account.Main_Account_Id = MainSubmain_Account.Id LEFT OUTER JOIN
                         Currency ON mezan.Equivalent_Account_Currency_Id = Currency.[Value] ";
                mysql = mysql + @" WHERE        (Year_Financial_Period_Id = " + Year_Financial_Period_Id + ")  ";
                mysql = mysql + @"  AND  (mezan.Branch_Id = " + Branch_Id + ")";
                //if (Show_AccountWithBalance == 1)   
                //{

                //    mysql = mysql + @" And    (First_Balance + Final_Export_Value + Final_Import_Value <> 0)";


                //}
                if (Entry_Currency_Id > -1)
                {
                    mysql = mysql + @" AND (Equivalent_Account_Currency_Id =" + Entry_Currency_Id + ")";

                }
                if (Show_AccountWithBalance == 0)    ///صفر لة رصيد
                {
                    mysql = mysql + @" And    (mezan.First_Balance_Madin + mezan.First_Balance_Da2n   = 0)";

                }
                if (Show_AccountWithBalance == 1)    //لة   رصيد
                {
                    mysql = mysql + @" And    (mezan.First_Balance_Madin + mezan.First_Balance_Da2n <> 0)";

                }

                if (Account_Customer_Type_ID > -1)    //لة حركات بدون رصيد
                {
                    mysql = mysql + @" And    (Submain_Account.Account_Customer_Type_ID  =" + Account_Customer_Type_ID + ")";

                }
                if (Account_Customer_Type_Value > -1)    //لة حركات بدون رصيد
                {
                    mysql = mysql + @" And    (Submain_Account.Account_Customer_Type_Value  =" + Account_Customer_Type_Value + ")";

                }


                if (IsShowTrueAccountOnly == 1)    // عرض الحسابات الحقيقية فقط
                {
                    //mysql = mysql + @" And      (Submain_Account.Head_Main_Account = 1 OR Submain_Account.Head_Main_Account = 2 OR Submain_Account.Head_Main_Account = 3 )";
                    mysql = mysql + @"       AND (Submain_Account.Head_Main_Account IN (1, 2, 3))";

                }


                //mysql = mysql + @"  ORDER BY mezan.Head_Main_Account,Main_Account_Id,mezan.AccountCode1, mezan.AccountCode2, mezan.AccountCode3, mezan.AccountCode4, mezan.AccountCode5, mezan.AccountCode6 ";
                mysql = mysql + @"  ORDER BY Submain_Account.Head_Main_Account, MainSubmain_Account.Head_Main_Account, Submain_Account.Main_Account_Id, Submain_Account.Id, Submain_Account.Account_Customer_Type_Value, Currency.[Value] DESC ";


                dt = opp.return_DataTable(mysql);

            }
            catch (Exception ee)
            {
                Msg = ee.Message;

                //opp.Rollback();
            }
            return dt;
        }

        public DataTable ShowFirstBalance_Mezan_Morgaa_ByPeiodNew(ref int Excp, ref string Msg, bool Is_To_Show_Prev, int Show_AccountWithBalance, long Branch_Id, DateTime dtFom, DateTime dtTo, long Submain_Account_ID, long Entry_Currency_Id, long Year_Financial_Period_Id, bool IS_Set_NewAccountInFirstBalanceZero, bool AnyDatePeriod, DateTime Financial_Period_dtFom)
        {
            FirstBalance_CLS firstBalance_CLS = new FirstBalance_CLS();
            operations opp = new operations();
            firstBalance_CLS.BalanceOpeningBalanceEquity(ref opp, Year_Financial_Period_Id, Branch_Id, Entry_Currency_Id);
            return Get_Mezan_Morgaa_ByPeiodNew(ref Excp, ref Msg, ref opp, Is_To_Show_Prev, Show_AccountWithBalance, Branch_Id, dtFom, dtTo, Submain_Account_ID, Entry_Currency_Id, Year_Financial_Period_Id, IS_Set_NewAccountInFirstBalanceZero, AnyDatePeriod, Financial_Period_dtFom);
        }
        public DataTable Get_Mezan_Morgaa_ByPeiodNew(ref int Excp, ref string Msg, bool Is_To_Show_Prev, int Show_AccountWithBalance, long Branch_Id, DateTime dtFom, DateTime dtTo, long Submain_Account_ID, long Entry_Currency_Id, long Year_Financial_Period_Id, bool IS_Set_NewAccountInFirstBalanceZero, bool AnyDatePeriod, DateTime Financial_Period_dtFom)
        {
            operations opp = new operations();
            return Get_Mezan_Morgaa_ByPeiodNew(ref Excp, ref Msg, ref opp, Is_To_Show_Prev, Show_AccountWithBalance, Branch_Id, dtFom, dtTo, Submain_Account_ID, Entry_Currency_Id, Year_Financial_Period_Id, IS_Set_NewAccountInFirstBalanceZero, AnyDatePeriod, Financial_Period_dtFom);

        }
        public DataTable Get_Mezan_Morgaa_ByPeiodNew(ref int Excp, ref string Msg, ref operations opp, bool Is_To_Show_Prev, int Show_AccountWithBalance, long Branch_Id, DateTime dtFom, DateTime dtTo, long Submain_Account_ID, long Entry_Currency_Id, long Year_Financial_Period_Id, bool IS_Set_NewAccountInFirstBalanceZero, bool AnyDatePeriod, DateTime Financial_Period_dtFom)
        {
            //bool ShowBalanceZero,
            string mysql = "";

            DataTable dt = new DataTable();
            try
            {
                SetTemp_Mezan_Morgaa_ByPeiodNew(ref opp, Is_To_Show_Prev, Branch_Id, dtFom, dtTo, Submain_Account_ID, Entry_Currency_Id, Year_Financial_Period_Id, IS_Set_NewAccountInFirstBalanceZero, AnyDatePeriod, Financial_Period_dtFom);

                mysql = @"SELECT  mezan.* ,  Submain_Account.Account_Customer_Type_ID, Submain_Account.Account_Customer_Type_Value
                         FROM            Account_Entry_Master_Balance_MezanTemp AS mezan INNER JOIN
                         Submain_Account ON mezan.Submain_Account_ID = Submain_Account.Id ";
                mysql = mysql + @" WHERE        (Year_Financial_Period_Id = " + Year_Financial_Period_Id + ") AND (Equivalent_Account_Currency_Id =" + Entry_Currency_Id + ") ";
                mysql = mysql + @"  AND  (mezan.Branch_Id = " + Branch_Id + ")";
                //if (Show_AccountWithBalance == 1)   
                //{

                //    mysql = mysql + @" And    (First_Balance + Final_Export_Value + Final_Import_Value <> 0)";


                //}
                if (Show_AccountWithBalance == 1)    /// لة رصيد
                {
                    mysql = mysql + @" And    (mezan.Final_Last_Balance_Madin + mezan.Final_Last_Balance_Da2n   <> 0)";

                }
                if (Show_AccountWithBalance == 2)    //لة حركات بدون رصيد
                {
                    mysql = mysql + @" And    (mezan.Final_Export_Value + mezan.Final_Import_Value <> 0)";

                }

                if (Account_Customer_Type_ID > -1)    //لة حركات بدون رصيد
                {
                    mysql = mysql + @" And    (Submain_Account.Account_Customer_Type_ID  =" + Account_Customer_Type_ID + ")";

                }
                if (Account_Customer_Type_Value > -1)    //لة حركات بدون رصيد
                {
                    mysql = mysql + @" And    (Submain_Account.Account_Customer_Type_Value  =" + Account_Customer_Type_Value + ")";

                }


                if (IsShowTrueAccountOnly == 1)    // عرض الحسابات الحقيقية فقط
                {
                    mysql = mysql + @" And      (Submain_Account.Head_Main_Account = 1 OR Submain_Account.Head_Main_Account = 2 OR Submain_Account.Head_Main_Account = 3 )";

                }


                //mysql = mysql + @" ORDER BY MainCode, Code ";
                //mysql = mysql + @"  ORDER BY Code, AccountCode1,AccountCode2,AccountCode3,AccountCode4,AccountCode5,AccountCode6  ";
                //mysql = mysql + @"  ORDER BY mezan.AccountCode1, mezan.AccountCode2, mezan.AccountCode3, mezan.AccountCode4, mezan.AccountCode5, mezan.AccountCode6 ";
                mysql = mysql + @"  ORDER BY mezan.Head_Main_Account,Main_Account_Id,mezan.AccountCode1, mezan.AccountCode2, mezan.AccountCode3, mezan.AccountCode4, mezan.AccountCode5, mezan.AccountCode6 ";


                dt = opp.return_DataTable(mysql);

            }
            catch (Exception ee)
            {
                Msg = ee.Message;

                //opp.Rollback();
            }
            return dt;
        }


        public void SetTemp_Mezan_Morgaa_ByPeiodNew(ref operations opp, bool Is_To_Show_Prev, long Branch_Id, DateTime dtFom, DateTime dtTo, long Submain_Account_ID, long Entry_Currency_Id, long Year_Financial_Period_Id, bool IS_Set_NewAccountInFirstBalanceZero, bool AnyDatePeriod, DateTime Financial_Period_dtFom)
        {
            //bool ShowBalanceZero,
            ///////////////
            bool IS_Excute_All = false;
            SetOptionGet_Mezan_Morgaa_ByPeiodNew(ref opp, Is_To_Show_Prev, IS_Set_NewAccountInFirstBalanceZero, AnyDatePeriod, Year_Financial_Period_Id, Branch_Id, dtFom, Financial_Period_dtFom);


            string mysql = "";

            mysql = " truncate Table  Account_Entry_Master_Balance_MezanTemp ";
            mysql = mysql + " truncate Table  Account_Entry_Master_TransactionTemp ";
            if (IS_Excute_All == false)
            {
                opp.insert_me(mysql);
                mysql = "";
            }
            ////   إضافة كل ارصدة اول المدة للحسابات الى الجدول الارصدة المؤقت والتى لها حركات او بدون 
            ////من جدول اول المدة


            mysql = mysql + @" INSERT INTO Account_Entry_Master_Balance_MezanTemp
                         ( Last_Balance_Zero, SubAccount_NO, Code, Account_Code, MainAccount_Name, MainAccount_NO, Main_Account_Id, MainCode, Submain_Account_ID, 
                         Account_Name, First_Balance_Madin, First_Balance_Da2n, Equivalent_First_Balance_Madin, Equivalent_First_Balance_Da2n, Equivalent_Account_Currency_Id, 
                         Branch_Id, Year_Financial_Period_Id, First_Balance,Account_Currency_Id,AccountCode1,AccountCode2,AccountCode3,AccountCode4,AccountCode5,AccountCode6,Account_Follows,Head_Main_Account)

                        SELECT         1 AS Last_Balance_Zero, Submain_Account.SubAccount_NO, Submain_Account.Code, Submain_Account.Account_Code, 
                         MainSubmain_Account.SubAccount_Name AS MainAccount_Name, MainSubmain_Account.SubAccount_NO AS MainAccount_NO, Submain_Account.Main_Account_Id, 
                         MainSubmain_Account.SubAccount_NO AS MainCode, Account_Entry_Master_Balance.Submain_Account_ID, Submain_Account.SubAccount_Name, 
                         Account_Entry_Master_Balance.First_Balance_Madin, Account_Entry_Master_Balance.First_Balance_Da2n, 
                         Account_Entry_Master_Balance.Equivalent_First_Balance_Madin, Account_Entry_Master_Balance.Equivalent_First_Balance_Da2n, 
                         Account_Entry_Master_Balance.Equivalent_Account_Currency_Id, Account_Entry_Master_Balance.Branch_Id, 
                         Account_Entry_Master_Balance.Year_Financial_Period_Id, Account_Entry_Master_Balance.First_Balance,Account_Entry_Master_Balance.Account_Currency_Id 
                        ,Submain_Account.AccountCode1,Submain_Account.AccountCode2,Submain_Account.AccountCode3,Submain_Account.AccountCode4,Submain_Account.AccountCode5,Submain_Account.AccountCode6 
                        ,Submain_Account.Account_Follows ,Submain_Account.Head_Main_Account
                        FROM            Submain_Account INNER JOIN
                         Submain_Account AS MainSubmain_Account ON Submain_Account.Main_Account_Id = MainSubmain_Account.Id  INNER JOIN
                         Account_Entry_Master_Balance ON Submain_Account.Id = Account_Entry_Master_Balance.Submain_Account_ID ";

            mysql = mysql + @" WHERE        (Account_Entry_Master_Balance.Year_Financial_Period_Id = " + Year_Financial_Period_Id + ") AND (Account_Entry_Master_Balance.Equivalent_Account_Currency_Id =" + Entry_Currency_Id + ") ";
            mysql = mysql + @"  AND  (Account_Entry_Master_Balance.Branch_Id = " + Branch_Id + ")";


            if (IS_Excute_All == false)
            {
                try
                {
                    // DataTable dt = opp.return_DataTable(mysqlS);
                    opp.insert_me(mysql);
                }
                catch (Exception ee)
                {

                    throw;
                }
                //DataTable dt = opp.return_DataTable("SELECT        1 AS Last_Balance_Zero, Submain_Account.SubAccount_NO, Submain_Account.Code, Submain_Account.Account_Code, \r\n                         MainSubmain_Account.SubAccount_Name AS MainAccount_Name, MainSubmain_Account.SubAccount_NO AS MainAccount_NO, Submain_Account.Main_Account_Id, \r\n                         MainSubmain_Account.SubAccount_NO AS MainCode, Account_Entry_Master_Balance.Submain_Account_ID, Submain_Account.SubAccount_Name, \r\n                         Account_Entry_Master_Balance.First_Balance_Madin, Account_Entry_Master_Balance.First_Balance_Da2n, \r\n                         Account_Entry_Master_Balance.Equivalent_First_Balance_Madin, Account_Entry_Master_Balance.Equivalent_First_Balance_Da2n, \r\n                         Account_Entry_Master_Balance.Equivalent_Account_Currency_Id, Account_Entry_Master_Balance.Branch_Id, \r\n                         Account_Entry_Master_Balance.Year_Financial_Period_Id, Account_Entry_Master_Balance.First_Balance,Account_Currency_Id \r\n                        ,Submain_Account.AccountCode1,Submain_Account.AccountCode2,Submain_Account.AccountCode3,Submain_Account.AccountCode4,Submain_Account.AccountCode5,Submain_Account.AccountCode6 \r\n                        ,Submain_Account.Account_Follows ,Submain_Account.Head_Main_Account\r\n                        FROM            Submain_Account INNER JOIN\r\n                         Submain_Account AS MainSubmain_Account ON Submain_Account.Main_Account_Id = MainSubmain_Account.Id  INNER JOIN\r\n                         Account_Entry_Master_Balance ON Submain_Account.Id = Account_Entry_Master_Balance.Submain_Account_ID ");
                mysql = "";
            }
            ////////////////////////////////////////////////
            /////إضافة الحسابات التى لها حركات  فى جدول   الحركات المؤقت
            mysql = mysql + @" INSERT INTO Account_Entry_Master_TransactionTemp
                         (  Last_Balance_Zero, Submain_Account_ID, Account_Name, Final_Fist_Balance_Madin, Final_Fist_Balance_Da2n, Final_Last_Balance_Madin, 
                         Final_Last_Balance_Da2n, Final_Transaction_Madin, Final_Transaction_Da2n, SubAccount_NO, Code, Account_Code, MainAccount_Name, MainAccount_NO, 
                         Main_Account_Id, MainCode,   
                          Final_Import_Value, Final_Export_Value)";

            mysql = mysql + @"  SELECT        1 AS Last_Balance_Zero, Account_Entry_Details.Submain_Account_ID, Account_Entry_Details.Account_Name, 0.0 AS Final_Fist_Balance_Madin, 0.0 AS Final_Fist_Balance_Da2n, 
                         0.0 AS Final_Last_Balance_Madin, 0.0 AS Final_Last_Balance_Da2n,
  0.0 AS Final_Transaction_Madin, 0.0 AS Final_Transaction_Da2n,

Submain_Account.SubAccount_NO, Submain_Account.Code, Submain_Account.Account_Code, 
                         MainSubmain_Account.SubAccount_Name AS MainAccount_Name, MainSubmain_Account.SubAccount_NO AS MainAccount_NO, Submain_Account.Main_Account_Id, 
                         MainSubmain_Account.SubAccount_NO AS MainCode,  
                          
                             ";
            if (Entry_Currency_Id > 0)
            {
                mysql = mysql + @" 
SUM(Account_Entry_Details.Import_Value) AS Final_Import_Value,
SUM(Account_Entry_Details.Export_Value) AS Final_Export_Value ";
            }
            if (Entry_Currency_Id == 0)
            {
                mysql = mysql + @"  
SUM(Account_Entry_Details.Equivalent_Import_Value) AS Final_Import_Value, 
SUM(Account_Entry_Details.Equivalent_Export_Value) AS Final_Export_Value 
  ";
            }
            mysql = mysql + @" FROM            Account_Entry_Details INNER JOIN
                         Account_Entry_List ON Account_Entry_Details.Account_Entry_List_ID = Account_Entry_List.Id INNER JOIN
                         Submain_Account ON Account_Entry_Details.Submain_Account_ID = Submain_Account.Id INNER JOIN
                         Submain_Account AS MainSubmain_Account ON Submain_Account.Main_Account_Id = MainSubmain_Account.Id ";

            mysql = mysql + " WHERE  (Account_Entry_List.Entry_No IS NOT  NULL) AND  (Account_Entry_List.Entry_Date BETWEEN " + operations.Get_DateToInsert(dtFom) + "  AND  " + operations.Get_DateToInsert(dtTo) + ")";

            if (Submain_Account_ID > 0)
            {
                mysql = mysql + " AND    (Account_Entry_Details.Submain_Account_ID  = " + Submain_Account_ID + ") ";
            }
            if (Branch_Id > 0)
            {
                mysql = mysql + " AND    (Account_Entry_List.Branch_Id_Entry  = " + Branch_Id + ") ";
            }
            if (Entry_Currency_Id > 0)
            {
                mysql = mysql + " AND (Account_Entry_List.Entry_Currency_Id = " + Entry_Currency_Id + ") ";
            }


            mysql = mysql + @" GROUP BY Account_Entry_List.Branch_Id_Entry, Account_Entry_Details.Submain_Account_ID, Account_Entry_Details.Account_Name, Submain_Account.SubAccount_NO, 
                         Submain_Account.Code, Submain_Account.Account_Code, Submain_Account.Currency_Id, MainSubmain_Account.SubAccount_Name, 
                         MainSubmain_Account.SubAccount_NO, Submain_Account.Main_Account_Id, MainSubmain_Account.Code 
                         ";

            mysql = mysql + @" ORDER BY MainSubmain_Account.Code   ";



            if (IS_Excute_All == false)
            {
                opp.insert_me(mysql);
                mysql = "";
            }


            ///تعديل حركات الحساب من جدول الارصدة المؤقت المؤقت الى جدول
            //////الحركات المؤقت
            ///نقل وحفظ اجمالى حركات الحساب من جدول الحركات المؤقت الى جدول الارصدة المؤقت
            mysql = mysql + @" UPDATE       Account_Entry_Master_Balance_MezanTemp
SET                Final_Import_Value = Account_Entry_Master_TransactionTemp.Final_Import_Value, 
                         Final_Export_Value = Account_Entry_Master_TransactionTemp.Final_Export_Value
FROM            Account_Entry_Master_Balance_MezanTemp INNER JOIN
                         Account_Entry_Master_TransactionTemp ON 
                         Account_Entry_Master_Balance_MezanTemp.Submain_Account_ID = Account_Entry_Master_TransactionTemp.Submain_Account_ID";
            if (IS_Excute_All == false)
            {
                opp.insert_me(mysql);
                mysql = "";
            }
            /////////////////////
            ///التاريخ ليس من اول الفترة المالية
            ///Is_To_Show
            ///

            if (Is_To_Show_Prev == true)
            {
                //TimeSpan ts = new TimeSpan(1, 0, 0, 0);
                //DateTime New_dtTo = dtFom.Subtract(ts);

                mysql = mysql + Get_Mezan_Morgaa_ByPeiodNewUpdate(ref opp, IS_Excute_All, Branch_Id, dtFom, dtTo, Submain_Account_ID, Entry_Currency_Id, Year_Financial_Period_Id, Financial_Period_dtFom);
            }



            mysql = mysql + @" UPDATE       Account_Entry_Master_Balance_MezanTemp
SET                Final_Fist_Balance_Madin = (First_Balance_Madin - First_Balance_Da2n) + (Prev_Import_Value - Prev_Export_Value), 
                         Final_Fist_Balance_Da2n = (First_Balance_Madin - First_Balance_Da2n) + (Prev_Import_Value - Prev_Export_Value), 
                         Final_Last_Balance_Madin = ((First_Balance_Madin - First_Balance_Da2n) + (Prev_Import_Value - Prev_Export_Value)) 
                         + (Final_Import_Value - Final_Export_Value), Final_Last_Balance_Da2n = ((First_Balance_Madin - First_Balance_Da2n) 
                         + (Prev_Import_Value - Prev_Export_Value)) + (Final_Import_Value - Final_Export_Value), Final_Transaction_Madin = Final_Import_Value - Final_Export_Value, 
                         Final_Transaction_Da2n = Final_Import_Value - Final_Export_Value ";

            if (IS_Excute_All == false)
            {
                opp.insert_me(mysql);
                mysql = "";
            }

            mysql = mysql + @" UPDATE       Account_Entry_Master_Balance_MezanTemp
SET                                     Final_Fist_Balance_Da2n =0
where  Final_Fist_Balance_Madin>0 ";

            if (IS_Excute_All == false)
            {
                opp.insert_me(mysql);
                mysql = "";
            }

            mysql = mysql + @" UPDATE       Account_Entry_Master_Balance_MezanTemp
SET                                   Final_Fist_Balance_Madin=0, Final_Fist_Balance_Da2n= Final_Fist_Balance_Da2n*-1

where  Final_Fist_Balance_Madin<0 ";



            if (IS_Excute_All == false)
            {
                opp.insert_me(mysql);
                mysql = "";
            }

            mysql = mysql + @" UPDATE       Account_Entry_Master_Balance_MezanTemp
SET Final_Last_Balance_Da2n =0
where  Final_Last_Balance_Madin>0 ";

            if (IS_Excute_All == false)
            {
                opp.insert_me(mysql);
                mysql = "";
            }

            mysql = mysql + @" UPDATE       Account_Entry_Master_Balance_MezanTemp
SET Final_Last_Balance_Madin=0, Final_Last_Balance_Da2n= Final_Last_Balance_Da2n*-1

where  Final_Last_Balance_Madin<0 ";


            if (IS_Excute_All == false)
            {
                opp.insert_me(mysql);
                mysql = "";
            }

            mysql = mysql + @" UPDATE       Account_Entry_Master_Balance_MezanTemp
SET Final_Transaction_Da2n =0
where  Final_Transaction_Madin>0 ";

            if (IS_Excute_All == false)
            {
                opp.insert_me(mysql);
                mysql = "";
            }

            mysql = mysql + @" UPDATE       Account_Entry_Master_Balance_MezanTemp
SET Final_Transaction_Madin=0,  Final_Transaction_Da2n=  Final_Transaction_Da2n * -1

where  Final_Transaction_Madin<0  ";


            opp.insert_me(mysql);
            //mysql = "";



        }


        /// <summary>
        /// //////////////////////Update CostCenter first Balance
        /// </summary>
        /// <param name="opp"></param>
        /// <param name="Is_To_Show_Prev"></param>
        /// <param name="Branch_Id"></param>
        /// <param name="dtFom"></param>
        /// <param name="dtTo"></param>
        /// <param name="CostCenter_Id"></param>
        /// <param name="Entry_Currency_Id"></param>
        /// <param name="Year_Financial_Period_Id"></param>
        /// <param name="IS_Set_NewAccountInFirstBalanceZero"></param>
        /// <param name="AnyDatePeriod"></param>
        /// <param name="Financial_Period_dtFom"></param>
        public void SetTemp_Mezan_Morgaa_ByPeiodNew_CostCenter(ref operations opp, bool Is_To_Show_Prev, long Branch_Id, DateTime dtFom, DateTime dtTo, long CostCenter_Id, long Entry_Currency_Id, long Year_Financial_Period_Id, bool IS_Set_NewAccountInFirstBalanceZero, bool AnyDatePeriod, DateTime Financial_Period_dtFom)
        {
            ////////////CCCCCCCC
            //bool ShowBalanceZero,
            bool IS_Excute_All = false;
            SetOptionGet_Mezan_Morgaa_ByPeiodNewCostCenter(ref opp, Is_To_Show_Prev, IS_Set_NewAccountInFirstBalanceZero, AnyDatePeriod, Year_Financial_Period_Id, Branch_Id, dtFom, Financial_Period_dtFom);


            string mysql = "";

            mysql = " truncate Table  Account_Entry_Master_Balance_CostCenterTemp ";
            mysql = mysql + " truncate Table  Account_Entry_Master_TransactionCostCenterTemp ";
            if (IS_Excute_All == false)
            {
                opp.insert_me(mysql);
                mysql = "";
            }
            ////   إضافة كل الحسابات الى الجدول الارصدة المؤقت والتى لها حركات او بدون 
            ////من جدول اول المدة
            //Cost_Center_Id,Cost_Center_Name
            mysql = mysql + @" INSERT INTO Account_Entry_Master_Balance_CostCenterTemp
                         (Last_Balance_Zero, SubAccount_NO, Code, Account_Code, MainAccount_Name, MainAccount_NO, Main_Account_Id, MainCode, Submain_Account_ID, Account_Name, First_Balance_Madin, First_Balance_Da2n, 
                         Equivalent_First_Balance_Madin, Equivalent_First_Balance_Da2n, Equivalent_Account_Currency_Id, Branch_Id, Year_Financial_Period_Id, First_Balance, Account_Currency_Id, Cost_Center_Id, 
                         Cost_Center_Name)
SELECT        1 AS Last_Balance_Zero, Cost_Center_Tree.Code_Str, Cost_Center_Tree.Code AS Expr1, Cost_Center_Tree.Code_Str AS Expr2, Cost_Center_Tree_Main.Name AS MainAccount_Name, 
                         Cost_Center_Tree_Main.Code_Str AS MainAccount_NO, Cost_Center_Tree.Parent_Id, Cost_Center_Tree_Main.Code AS MainCode, Account_Entry_Master_Balance_CostCenter.Submain_Account_ID, 
                         Cost_Center_Tree.Name, Account_Entry_Master_Balance_CostCenter.First_Balance_Madin, Account_Entry_Master_Balance_CostCenter.First_Balance_Da2n, 
                         Account_Entry_Master_Balance_CostCenter.Equivalent_First_Balance_Madin, Account_Entry_Master_Balance_CostCenter.Equivalent_First_Balance_Da2n, 
                         Account_Entry_Master_Balance_CostCenter.Equivalent_Account_Currency_Id, Account_Entry_Master_Balance_CostCenter.Branch_Id, Account_Entry_Master_Balance_CostCenter.Year_Financial_Period_Id, 
                         Account_Entry_Master_Balance_CostCenter.First_Balance, Account_Entry_Master_Balance_CostCenter.Account_Currency_Id, Account_Entry_Master_Balance_CostCenter.Cost_Center_Id, 
                         Account_Entry_Master_Balance_CostCenter.Cost_Center_Name
FROM            Cost_Center_Tree INNER JOIN
                         Cost_Center_Tree AS Cost_Center_Tree_Main ON Cost_Center_Tree.Parent_Id = Cost_Center_Tree_Main.Id INNER JOIN
                         Account_Entry_Master_Balance_CostCenter ON Cost_Center_Tree.Id = Account_Entry_Master_Balance_CostCenter.Cost_Center_Id
 
 ";



            mysql = mysql + @" WHERE        (Account_Entry_Master_Balance_CostCenter.Year_Financial_Period_Id = " + Year_Financial_Period_Id + ") AND (Account_Entry_Master_Balance_CostCenter.Equivalent_Account_Currency_Id =" + Entry_Currency_Id + ") ";
            mysql = mysql + @"  AND  (Account_Entry_Master_Balance_CostCenter.Branch_Id = " + Branch_Id + ")";
            mysql = mysql + @"  ORDER BY MainCode";

            if (IS_Excute_All == false)
            {
                opp.insert_me(mysql);
                mysql = "";
            }
            //////////////////////////////////////////////// Cost_Center_Id,Cost_Center_Name
            /////إضافة الحسابات التى لها حركات  فى جدول   الحركات المؤقت
            mysql = mysql + @" INSERT INTO Account_Entry_Master_TransactionCostCenterTemp
                         (Account_Name, Submain_Account_ID, Cost_Center_Id, Cost_Center_Name, Final_Fist_Balance_Madin, Final_Fist_Balance_Da2n, Final_Last_Balance_Madin, Final_Last_Balance_Da2n, 
                         Final_Transaction_Madin, Final_Transaction_Da2n, SubAccount_NO, Code, Account_Code, MainAccount_Name, MainAccount_NO, Main_Account_Id, MainCode, Final_Import_Value, Final_Export_Value) ";

            mysql = mysql + @"  SELECT        Account_Entry_Details.Account_Name, Account_Entry_Details.Submain_Account_ID, Account_Entry_Details.CostCenter_Id, Cost_Center_Tree.Name, 0.0 AS Final_Fist_Balance_Madin, 0.0 AS Final_Fist_Balance_Da2n, 0.0 AS Final_Last_Balance_Madin, 0.0 AS Final_Last_Balance_Da2n, 
                         0.0 AS Final_Transaction_Madin, 0.0 AS Final_Transaction_Da2n, Cost_Center_Tree.Code_Str, Cost_Center_Tree.Code_Str AS Expr1, Cost_Center_Tree.Code_Str AS Expr2, 
                         Cost_Center_Tree_Main.Name AS MainAccount_Name, Cost_Center_Tree_Main.Code_Str AS MainAccount_NO, Cost_Center_Tree.Parent_Id, Cost_Center_Tree_Main.Code AS MainCode, 
                          
                             ";
            if (Entry_Currency_Id > 0)
            {
                mysql = mysql + @" 
SUM(Account_Entry_Details.Import_Value) AS Final_Import_Value,
SUM(Account_Entry_Details.Export_Value) AS Final_Export_Value ";
            }
            if (Entry_Currency_Id == 0)
            {
                mysql = mysql + @"  
SUM(Account_Entry_Details.Equivalent_Import_Value) AS Final_Import_Value, 
SUM(Account_Entry_Details.Equivalent_Export_Value) AS Final_Export_Value 
  ";
            }
            mysql = mysql + @" FROM            Account_Entry_Details INNER JOIN
                         Account_Entry_List ON Account_Entry_Details.Account_Entry_List_ID = Account_Entry_List.Id INNER JOIN
                         Cost_Center_Tree ON Account_Entry_Details.CostCenter_Id = Cost_Center_Tree.Id INNER JOIN
                         Cost_Center_Tree AS Cost_Center_Tree_Main ON Cost_Center_Tree.Parent_Id = Cost_Center_Tree_Main.Id ";

            mysql = mysql + " WHERE   (Account_Entry_Details.CostCenter_Id <> 0)AND  (Account_Entry_List.Entry_No IS NOT  NULL) AND  (Account_Entry_List.Entry_Date BETWEEN " + operations.Get_DateToInsert(dtFom) + "  AND  " + operations.Get_DateToInsert(dtTo) + ")";

            if (CostCenter_Id > 0)
            {
                mysql = mysql + " AND    (Account_Entry_Details.CostCenter_Id  = " + CostCenter_Id + ") ";
            }
            if (Branch_Id > 0)
            {
                mysql = mysql + " AND    (Account_Entry_List.Branch_Id_Entry  = " + Branch_Id + ") ";
            }
            if (Entry_Currency_Id > 0)
            {
                mysql = mysql + " AND (Account_Entry_List.Entry_Currency_Id = " + Entry_Currency_Id + ") ";
            }


            mysql = mysql + @" GROUP BY Account_Entry_List.Branch_Id_Entry, Account_Entry_Details.CostCenter_Id, Cost_Center_Tree.Name, Cost_Center_Tree_Main.Code, Cost_Center_Tree_Main.Code_Str, Cost_Center_Tree.Code_Str, 
                         Cost_Center_Tree_Main.Name, Cost_Center_Tree.Parent_Id , Account_Entry_Details.Submain_Account_ID, Account_Entry_Details.Account_Name

                         ";

            mysql = mysql + @" ORDER BY Cost_Center_Tree_Main.Code   ";



            if (IS_Excute_All == false)
            {
                opp.insert_me(mysql);
                mysql = "";
            }


            ///تعديل حركات الحساب من جدول الارصدة المؤقت المؤقت الى جدول
            //////الحركات المؤقت
            mysql = mysql + @" UPDATE       Account_Entry_Master_Balance_CostCenterTemp
SET                Final_Import_Value = Account_Entry_Master_TransactionCostCenterTemp.Final_Import_Value, Final_Export_Value = Account_Entry_Master_TransactionCostCenterTemp.Final_Export_Value
FROM            Account_Entry_Master_Balance_CostCenterTemp INNER JOIN
                         Account_Entry_Master_TransactionCostCenterTemp ON Account_Entry_Master_Balance_CostCenterTemp.Submain_Account_ID = Account_Entry_Master_TransactionCostCenterTemp.Submain_Account_ID AND 
                         Account_Entry_Master_Balance_CostCenterTemp.Cost_Center_Id = Account_Entry_Master_TransactionCostCenterTemp.Cost_Center_Id ";
            if (IS_Excute_All == false)
            {
                opp.insert_me(mysql);
                mysql = "";
            }
            /////////////////////
            ///التاريخ ليس من اول الفترة المالية
            ///Is_To_Show
            ///

            if (Is_To_Show_Prev == true)
            {


                //     mysql = mysql + Get_Mezan_Morgaa_ByPeiodNewCostCenterUpdate(ref opp, IS_Excute_All, Branch_Id, dtFom, dtTo, CostCenter_Id, Entry_Currency_Id, Year_Financial_Period_Id, Financial_Period_dtFom);
            }





            mysql = mysql + @" UPDATE       Account_Entry_Master_Balance_CostCenterTemp
SET                Final_Fist_Balance_Madin = (First_Balance_Madin - First_Balance_Da2n) + (Prev_Import_Value - Prev_Export_Value), 
                         Final_Fist_Balance_Da2n = (First_Balance_Madin - First_Balance_Da2n) + (Prev_Import_Value - Prev_Export_Value), 
                         Final_Last_Balance_Madin = ((First_Balance_Madin - First_Balance_Da2n) + (Prev_Import_Value - Prev_Export_Value)) 
                         + (Final_Import_Value - Final_Export_Value), Final_Last_Balance_Da2n = ((First_Balance_Madin - First_Balance_Da2n) 
                         + (Prev_Import_Value - Prev_Export_Value)) + (Final_Import_Value - Final_Export_Value), Final_Transaction_Madin = Final_Import_Value - Final_Export_Value, 
                         Final_Transaction_Da2n = Final_Import_Value - Final_Export_Value ";

            if (IS_Excute_All == false)
            {
                opp.insert_me(mysql);
                mysql = "";
            }

            mysql = mysql + @" UPDATE       Account_Entry_Master_Balance_CostCenterTemp
SET                                     Final_Fist_Balance_Da2n =0
where  Final_Fist_Balance_Madin>0 ";

            if (IS_Excute_All == false)
            {
                opp.insert_me(mysql);
                mysql = "";
            }

            mysql = mysql + @" UPDATE       Account_Entry_Master_Balance_CostCenterTemp
SET                                   Final_Fist_Balance_Madin=0, Final_Fist_Balance_Da2n= Final_Fist_Balance_Da2n*-1

where  Final_Fist_Balance_Madin<0 ";



            if (IS_Excute_All == false)
            {
                opp.insert_me(mysql);
                mysql = "";
            }

            mysql = mysql + @" UPDATE       Account_Entry_Master_Balance_CostCenterTemp
SET Final_Last_Balance_Da2n =0
where  Final_Last_Balance_Madin>0 ";

            if (IS_Excute_All == false)
            {
                opp.insert_me(mysql);
                mysql = "";
            }

            mysql = mysql + @" UPDATE       Account_Entry_Master_Balance_CostCenterTemp
SET Final_Last_Balance_Madin=0, Final_Last_Balance_Da2n= Final_Last_Balance_Da2n*-1

where  Final_Last_Balance_Madin<0 ";


            if (IS_Excute_All == false)
            {
                opp.insert_me(mysql);
                mysql = "";
            }

            mysql = mysql + @" UPDATE       Account_Entry_Master_Balance_CostCenterTemp
SET Final_Transaction_Da2n =0
where  Final_Transaction_Madin>0 ";

            if (IS_Excute_All == false)
            {
                opp.insert_me(mysql);
                mysql = "";
            }

            mysql = mysql + @" UPDATE       Account_Entry_Master_Balance_CostCenterTemp
SET Final_Transaction_Madin=0,  Final_Transaction_Da2n=  Final_Transaction_Da2n * -1

where  Final_Transaction_Madin<0  ";


            opp.insert_me(mysql);
            //mysql = "";



        }
        public void SetOptionGet_Mezan_Morgaa_ByPeiodNew(ref operations opp, bool Is_To_Show_Prev, bool IS_Set_NewAccountInFirstBalanceZero, bool AnyDatePeriod, long Year_Financial_Period_Id, long Branch_Id, DateTime dtFom, DateTime Financial_Period_dtFom)
        {



            if (IS_Set_NewAccountInFirstBalanceZero == true)
            {
                // التأكد من كل الحسابات فى شجرة الحسابات لها حساب فى جدول ارصدة الحسابات المؤقته
                Set_NewAccountInFirstBalanceZero(ref opp, Branch_Id, Year_Financial_Period_Id);
            }

            Is_To_Show_Prev = false;
            if (AnyDatePeriod == false)
            {
                if (dtFom > Financial_Period_dtFom)
                {
                    Is_To_Show_Prev = true;
                    //////////////حساب الفترة من تاريخ بدايه الفترة الماليه حتى تاريخ البحث الى

                }

            }
        }
        ///
        ///
        public void SetOptionGet_Mezan_Morgaa_ByPeiodNewCostCenter(ref operations opp, bool Is_To_Show_Prev, bool IS_Set_NewAccountInFirstBalanceZero, bool AnyDatePeriod, long Year_Financial_Period_Id, long Branch_Id, DateTime dtFom, DateTime Financial_Period_dtFom)
        {



            if (IS_Set_NewAccountInFirstBalanceZero == true)
            {
                Set_NewAccountInFirstBalanceZeroCostCenter(ref opp, Branch_Id, Year_Financial_Period_Id);
            }

            Is_To_Show_Prev = false;
            if (AnyDatePeriod == false)
            {
                if (dtFom > Financial_Period_dtFom)
                {
                    Is_To_Show_Prev = true;
                    //////////////حساب الفترة من تاريخ بدايه الفترة الماليه حتى تاريخ البحث الى

                }

            }
        }
        public DataTable Get_Mezan_Morgaa_ByPeiodNewNew(bool IS_Set_NewAccountInFirstBalanceZero, bool ShowBalanceZero, bool AnyDatePeriod, long Year_Financial_Period_Id, long Branch_Id, DateTime dtFom, DateTime dtTo, DateTime Financial_Period_dtFom, long Account_Currency_Id)
        {
            operations opp = new operations();
            string mysql = "truncate table Account_Entry_Master_Balance_MezanTemp";
            opp.insert_me(mysql);
            bool Is_Equivalent = true;
            bool Is_EquivalentFactor = false;

            long Submain_Account_ID = 0;
            if (IS_Set_NewAccountInFirstBalanceZero == true)
            {
                Set_NewAccountInFirstBalanceZero(Branch_Id, Year_Financial_Period_Id);
            }
            ////حساب اجمالى حركات الحسابات عن فترة
            Get_Mezan_Morgaa_ByPeiodNewNew_(ShowBalanceZero, Branch_Id, dtFom, dtTo, Submain_Account_ID, Account_Currency_Id, Year_Financial_Period_Id);

            if (AnyDatePeriod == false)
            {
                if (dtFom > Financial_Period_dtFom)
                {
                    //////////////حساب الفترة من تاريخ بدايه الفترة الماليه حتى تاريخ البحث الى
                    TimeSpan ts = new TimeSpan(1, 0, 0, 0);
                    DateTime New_dtTo = dtFom.Subtract(ts);
                    Get_Mezan_Morgaa_ByPeiodNewNewPrev(ShowBalanceZero, Branch_Id, Financial_Period_dtFom, New_dtTo, Submain_Account_ID, Account_Currency_Id, Year_Financial_Period_Id);


                }

            }


            mysql = @"UPDATE       Account_Entry_Master_Balance_MezanTemp
SET                Final_Fist_Balance_Madin = First_Balance_Madin - First_Balance_Da2n + Prev_Import_Value - Prev_Export_Value, 
                         Final_Fist_Balance_Da2n = First_Balance_Madin - First_Balance_Da2n + Prev_Import_Value - Prev_Export_Value, 
                         Final_Last_Balance_Madin = First_Balance_Madin - First_Balance_Da2n + Prev_Import_Value - Prev_Export_Value + Final_Import_Value - Final_Export_Value, 
                         Final_Last_Balance_Da2n = First_Balance_Madin - First_Balance_Da2n + Prev_Import_Value - Prev_Export_Value + Final_Import_Value - Final_Export_Value";

            opp.insert_me(mysql);


            mysql = @"UPDATE       Account_Entry_Master_Balance_MezanTemp
SET                Final_Fist_Balance_Da2n = 0
WHERE        (Final_Fist_Balance_Madin > 0)";
            opp.insert_me(mysql);
            mysql = @"UPDATE       Account_Entry_Master_Balance_MezanTemp
SET                Final_Fist_Balance_Da2n = Final_Fist_Balance_Da2n * - 1, Final_Fist_Balance_Madin = 0
WHERE        (Final_Fist_Balance_Madin < 0)";
            opp.insert_me(mysql);

            mysql = @"UPDATE       Account_Entry_Master_Balance_MezanTemp
SET                Final_Last_Balance_Da2n = 0
WHERE        (Final_Last_Balance_Madin > 0)";
            opp.insert_me(mysql);
            mysql = @"UPDATE       Account_Entry_Master_Balance_MezanTemp
SET                Final_Last_Balance_Da2n = Final_Last_Balance_Da2n * - 1,Final_Last_Balance_Madin = 0
WHERE        (Final_Last_Balance_Madin < 0)";
            opp.insert_me(mysql);


            mysql = @"UPDATE       Account_Entry_Master_Balance_MezanTemp
SET               Last_Balance_Zero=1";

            opp.insert_me(mysql);

            mysql = @"UPDATE       Account_Entry_Master_Balance_MezanTemp
SET               Last_Balance_Zero=0 
WHERE        (Final_Last_Balance_Madin = 0) AND (Final_Last_Balance_Da2n = 0)";
            opp.insert_me(mysql);


            mysql = "Select * from Account_Entry_Master_Balance_MezanTemp Order by MainCode ASC ";
            DataTable dt = opp.return_DataTable(mysql);
            return dt;



        }
        public long Get_Year_Financial_Period_Id_Next(ref operations opp, DateTime dateto)
        {
            Financial_PeriodCLS ff = new Financial_PeriodCLS();
            return ff.Get_Year_Financial_Period_Id_Next(ref opp, dateto);

        }

        public DataTable Get_Year_Financial_Period_IdPrev(ref operations opp, DateTime dateFrom, ref long Year_Financial_Period_Id_Prev)
        {
            Financial_PeriodCLS ff = new Financial_PeriodCLS();
            return ff.Get_Year_Financial_Period_IdPrev(ref opp, dateFrom, ref Year_Financial_Period_Id_Prev);


        }
        public DataTable Get_Mezan_Morgaa_ByPeiod_(bool AnyDatePeriod, long Year_Financial_Period_Id, long Branch_Id, DateTime dtFom, DateTime dtTo, DateTime Financial_Period_dtFom, long Account_Currency_Id)
        {
            bool Is_Equivalent = true;
            bool Is_EquivalentFactor = false;

            long Submain_Account_ID = 0;
            Set_NewAccountInFirstBalanceZero(Branch_Id, Year_Financial_Period_Id);
            ////حساب اجمالى حركات الحسابات عن فترة
            DataTable dt = Get_Mezan_Morgaa_ByPeiod(Branch_Id, dtFom, dtTo, Submain_Account_ID, Account_Currency_Id, Year_Financial_Period_Id);


            for (int x = 0; x < dt.Rows.Count; x++)
            {
                decimal First_Balance_Madin = 0;
                decimal First_Balance_Da2n = 0;
                decimal Final_First_Balance_Madin = 0;
                decimal Final_First_Balance_Da2n = 0;
                decimal Final_Last_Balance_Madin = 0;
                decimal Final_Last_Balance_Da2n = 0;

                Submain_Account_ID = long.Parse(dt.Rows[x]["Submain_Account_ID"].ToString());
                string Account_Name = dt.Rows[x]["Account_Name"].ToString();
                ////قيم الحركات
                decimal Final_Import_Value = decimal.Parse(dt.Rows[x]["Final_Import_Value"].ToString());
                decimal Final_Export_Value = decimal.Parse(dt.Rows[x]["Final_Export_Value"].ToString());
                ////////////////////////////////////////////////////////////// 
                ////////حساب قيمة الحركات السابقه

                decimal Prev_Import_Value = 0;
                decimal Prev_Export_Value = 0;
                if (AnyDatePeriod == false)
                {
                    if (dtFom > Financial_Period_dtFom)
                    {
                        //////////////حساب الفترة من تاريخ بدايه الفترة الماليه حتى تاريخ البحث الى
                        TimeSpan ts = new TimeSpan(1, 0, 0, 0);
                        DateTime New_dtTo = dtFom.Subtract(ts);
                        DataTable dt_Acc = Get_Mezan_Morgaa_ByPeiod(Branch_Id, Financial_Period_dtFom, New_dtTo, Submain_Account_ID, Account_Currency_Id, Year_Financial_Period_Id);

                        if (dt_Acc.Rows.Count > 0)
                        {
                            Prev_Import_Value = decimal.Parse(dt.Rows[0]["Final_Import_Value"].ToString());
                            Prev_Export_Value = decimal.Parse(dt.Rows[0]["Final_Export_Value"].ToString());

                        }
                    }
                    //////////////////////////////////////////////////////////////

                    //// الحصول على ارصده اول المده
                    Get_FirstBalance_Period(Is_Equivalent, Is_EquivalentFactor, Year_Financial_Period_Id, Branch_Id, Submain_Account_ID, ref First_Balance_Madin, ref First_Balance_Da2n);
                }
                ///////////////////////////////////////////
                decimal First_Balance = (First_Balance_Madin - First_Balance_Da2n) + (Prev_Import_Value - Prev_Export_Value);
                decimal Last_Balance = First_Balance + (Final_Import_Value - Final_Export_Value);

                if (First_Balance == 0)
                {
                    Final_First_Balance_Madin = 0;
                    Final_First_Balance_Da2n = 0;
                }
                else if (First_Balance > 0)
                {
                    Final_First_Balance_Madin = First_Balance;
                    Final_First_Balance_Da2n = 0;
                }
                else if (First_Balance < 0)
                {
                    First_Balance = First_Balance * -1;
                    Final_First_Balance_Madin = 0;
                    Final_First_Balance_Da2n = First_Balance;
                }


                if (Last_Balance == 0)
                {
                    Final_Last_Balance_Madin = 0;
                    Final_Last_Balance_Da2n = 0;
                }
                else if (Last_Balance > 0)
                {
                    Final_Last_Balance_Madin = Last_Balance;
                    Final_Last_Balance_Da2n = 0;
                }
                else if (Last_Balance < 0)
                {
                    Last_Balance = Last_Balance * -1;
                    Final_Last_Balance_Madin = 0;
                    Final_Last_Balance_Da2n = Last_Balance;
                }

                //////////ضبط رصيد اول المده
                dt.Rows[x]["Final_Fist_Balance_Madin"] = Final_First_Balance_Madin;
                dt.Rows[x]["Final_Fist_Balance_Da2n"] = Final_First_Balance_Da2n;


                ///////////////ضبط الرصيد النهائى للحساب  
                dt.Rows[x]["Final_Last_Balance_Madin"] = Final_Last_Balance_Madin;
                dt.Rows[x]["Final_Last_Balance_Da2n"] = Final_Last_Balance_Da2n;

            }



            return dt;
        }

    }
}
