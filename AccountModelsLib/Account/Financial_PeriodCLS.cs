using System;
using System.Collections.Generic;
using System.Text;
 
using System.Data;
 
using System.Collections;
namespace AccountModelsLib
{
    public class Financial_PeriodCLS
    {
        public Financial_PeriodCLS()
        {

        }
        public DataTable Get_Financial_Period()
        {
            int Excp = 0;
            string mysql = " SELECT * from  Financial_Period";

            mysql = mysql + " ORDER BY DateFrom ASC ";


            return Get_Financial_Period_Data(ref Excp, mysql);

        }

        public static long Get_Year_Financial_Period_Id_(DateTime dt_Operation)
        {
            long Year_Financial_Period_Id = 0;
            string mysql = @"  Select * from Financial_Period   ";
            mysql = mysql + " Where ( DateFrom <= " + operations.Get_DateToInsert(dt_Operation) + ") AND ( DateTo >= " + operations.Get_DateToInsert(dt_Operation) + ")";
            mysql = mysql + " ORDER BY DateFrom ASC ";
            operations opp = new operations();
            DataTable dt = opp.return_DataTable(mysql);
            try
            {
                if (dt.Rows.Count > 0)
                {
                    Year_Financial_Period_Id = long.Parse(dt.Rows[0]["Id"].ToString());
                }
                else
                {
                    DateTime dtNow = new DateTime(DateTime.Now.Year, 1, 1);
                    Year_Financial_Period_Id = Get_Year_Financial_Period_Id_(dtNow);

                }
            }
            catch { }
            return Year_Financial_Period_Id;
        }
        public static long Get_Year_Financial_Period_Id(DateTime dt_Operation, ref DateTime dt_OperationFrom)
        {
            long Year_Financial_Period_Id = 0;
            string mysql = @"  Select * from Financial_Period   ";
            mysql = mysql + " Where ( DateFrom <= " + operations.Get_DateToInsert(dt_Operation) + ") AND ( DateTo >= " + operations.Get_DateToInsert(dt_Operation) + ")";
            mysql = mysql + " ORDER BY DateFrom ASC ";
            operations opp = new operations();
            DataTable dt = opp.return_DataTable(mysql);
            try
            {
                if (dt.Rows.Count > 0)
                {
                    Year_Financial_Period_Id = long.Parse(dt.Rows[0]["Id"].ToString());
                    dt_OperationFrom = DateTime.Parse(dt.Rows[0]["DateFrom"].ToString());
                }
                else
                {
                    Year_Financial_Period_Id = 0;

                }
            }
            catch { }
            return Year_Financial_Period_Id;
        }
        public static long Get_Year_Financial_Period_Id(DateTime dt_Operation)
        {
            long Year_Financial_Period_Id = 0;
            string mysql = @"  Select * from Financial_Period   ";
            mysql = mysql + " Where ( DateFrom <= " + operations.Get_DateToInsert(dt_Operation) + ") AND ( DateTo >= " + operations.Get_DateToInsert(dt_Operation) + ")";
            mysql = mysql + " ORDER BY DateFrom ASC ";
            operations opp = new operations();
            DataTable dt = opp.return_DataTable(mysql);
            try
            {
                if (dt.Rows.Count > 0)
                {
                    Year_Financial_Period_Id = long.Parse(dt.Rows[0]["Id"].ToString());
                }
                else
                {
                    int Year = dt_Operation.Year;
                    Financial_PeriodCLS ff = new Financial_PeriodCLS();
                    DateTime dt_From = new DateTime(Year, 1, 1);
                    DateTime dt_To = new DateTime(Year, 12, 31);
                    int Excp = 0;
                    long ID = ff.Make_Period(ref Excp, dt_From, dt_To, 0);
                    Year_Financial_Period_Id =  ID;

                }
            }
            catch { }
            return Year_Financial_Period_Id;
        }
        public DataTable Get_FirstYear_Financial_Period(DateTime dt_Operation)
        {

            string mysql = Get_Financial_Period_Mysql();


            
            operations opp = new operations();
            DataTable dt = opp.return_DataTable(mysql);
            try
            {
                if (dt.Rows.Count == 0)
                {
                    
                    int Year = dt_Operation.Year;
                    int Month = dt_Operation.Month;
                    int Day = dt_Operation.Day;
                    Financial_PeriodCLS ff = new Financial_PeriodCLS();
                    DateTime dt_From = new DateTime(Year, Month, Day);
                    DateTime dt_To = new DateTime(Year, 12, 31);
                    int Excp = 0;
                    long ID = ff.Make_Period(ref Excp, dt_From, dt_To, 1);
                    dt = opp.return_DataTable(mysql);
                   
                }
            }
            catch { }
            return dt;
        }

        public static long Get_Year_Financial_Period_Id(ref operations opp, DateTime dt_Operation)
        {
            long Year_Financial_Period_Id = 0;
            string mysql = @"  Select * from Financial_Period   ";
            mysql = mysql + " Where ( DateFrom <= " + operations.Get_DateToInsert(dt_Operation) + ") AND ( DateTo >= " + operations.Get_DateToInsert(dt_Operation) + ")";
            mysql = mysql + " ORDER BY DateFrom ASC ";

            DataTable dt = opp.return_DataTable(mysql);
            try
            {
                if (dt.Rows.Count > 0)
                {
                    Year_Financial_Period_Id = long.Parse(dt.Rows[0]["Id"].ToString());
                }
                else
                {
                    int Year = dt_Operation.Year;
                    Financial_PeriodCLS ff = new Financial_PeriodCLS();
                    DateTime dt_From = new DateTime(Year, 1, 1);
                    DateTime dt_To = new DateTime(Year, 12, 31);
                    int Excp = 0;
                    long ID = ff.Make_Period(ref Excp, dt_From, dt_To, 1);
                    Year_Financial_Period_Id = (long)ID;

                }
            }
            catch { }
            return Year_Financial_Period_Id;
        }
        public void Active_Period(ref int Excp,ref string Msg, long Financial_Period_ID, int Active)
        {
            operations opp = new operations(1);
            try
            {

                string mysql = "Select * from Financial_Period Where Id=" + Financial_Period_ID;
                DataTable dt = opp.return_DataTable(mysql);
                if (dt.Rows.Count > 0)
                {
                    int IS_Finished_Posted = int.Parse(dt.Rows[0]["IS_Finished_Posted"].ToString());
                    if (IS_Finished_Posted == 1)
                    {
                        Msg="  áã íÊã ÊäÔíØ ÇáÝÊÑÉ ÇáãÇáíå - ÇáÝÊÑÉ ãäÊåíå" ;
                        opp.Commit();
                        return;
                    }
                }
                mysql = " Update Financial_Period Set  Done= 1 ,IS_Active=0";
                opp.insert_me(mysql);
                mysql = " Update Financial_Period Set  Done=" + Active + ", IS_Active=1 where Id=" + Financial_Period_ID;
                int Count = opp.insert_me(mysql);
                Common.Year_Financial_Period_Id = Financial_Period_ID;

                Excp = 0;
                opp.Commit();

                //MessageBox.Show("Êã ÊäÔíØ ÇáÝÊÑÉ ÇáãÇáíå");

            }
            catch (Exception ee)
            {
                opp.Rollback();
                Msg=ee.Message;
                Excp = 1;
            }
        }

        public long Make_Period(ref int Excp, DateTime DateFrom, DateTime DateTo, int FirstPeriod)
        {
            long Year_Financial_Period_Id = 0;
            try
            {
                int IS_Active = 0;
                operations opp = new operations();
                string mysql = " Select * from Financial_Period Where IS_Active=1 and IS_Finished_Posted=0";
                DataTable dt = opp.return_DataTable(mysql);
                if (dt.Rows.Count == 0)
                {
                    IS_Active = 1;
                }
                long ID = Common.GetTable_Id_NoBranch("Financial_Period", "ID");
                mysql = " Insert Into Financial_Period(ID,DateFrom,DateTo,Done,FirstPeriod,IS_Active,IS_Finished_Posted) Values(" + ID + ",'" + operations.dateFormat(DateFrom) + "','" + operations.dateFormat(DateTo) + "',1," + FirstPeriod + "," + IS_Active + ",0) ";

                opp.insert_me(mysql);
                Year_Financial_Period_Id = ID;
                Excp = 0;
            }
            catch (Exception ee)
            {
                //MessageBox.Show(ee.Message);
                Excp = 1;
            }
            return Year_Financial_Period_Id;
        }

        public void Delete_Period(ref int Excp, ref string Msg, long Period_Id, DateTime DateFrom, DateTime DateTo)
        {
            string mysql = "";
            operations opp = new operations();

            mysql = "SELECT   Account_Entry_List.* FROM    Account_Entry_List ";
            mysql = mysql + " WHERE   (Entry_Date > " + operations.Get_DateToInsert(DateFrom) + ") AND (Entry_Date < " + operations.Get_DateToInsert(DateTo) + ")";
            DataTable dt = opp.return_DataTable(mysql);
            if (dt.Rows.Count > 0)
            {
                Msg = "áÇ íãßä ÍÐÝ ÇáÝÊÑå ÇáãÇáíå ÈÓÈÈ æÌæÏ ÞíæÏ ãÓÌáå Ýì Êáß ÇáÝÊÑå";
                Excp = 1;
                return;
            }

            mysql = " Delete from  Financial_Period where IS_Finished_Posted =0 AND Id=" + Period_Id;


            int Status = opp.insert_me(mysql);
            if (Status == 1)
            {
                Excp = 0;
                Msg = "Êã ÍÐÝ ÇáÝÊÑå ÇáãÇáíå";
            }
            else
            {
                Excp = 1;
                Msg = "áã íÊã ÍÐÝ ÇáÝÊÑå ÇáãÇáíå ";
            }

        }
        public string Get_ActiveFinancial_Period_Mysql()
        {
            string mysql = "";
            mysql = " SELECT        '   ãä  ' + CONVERT(nvarchar, DateFrom, 111) + '  Åáì  ' + CONVERT(nvarchar, DateTo, 111) AS Financial_Period_Name, Financial_Period.* FROM   Financial_Period";

            mysql = mysql + " Where IS_Active=1";
            mysql = mysql + " ORDER BY DateFrom ASC ";

            return mysql;
        }

        public string  Get_Financial_Period_Mysql()
        {
            string mysql = "";
            mysql = " SELECT        '   ãä  ' + CONVERT(nvarchar, DateFrom, 111) + '  Åáì  ' + CONVERT(nvarchar, DateTo, 111) AS Financial_Period_Name, Financial_Period.* FROM   Financial_Period";
            //mysql = mysql + " Where IS_Active=1";
            mysql = mysql + " ORDER BY DateFrom ASC ";

            return mysql;
        }

        public DataTable Get_Year_Financial_Period_IdPrev(ref operations opp, DateTime dateFrom, ref long Year_Financial_Period_Id_Prev)
        {
            


            Year_Financial_Period_Id_Prev = 0;
            TimeSpan ts = new TimeSpan(1, 0, 0, 0);
            dateFrom = dateFrom.Subtract(ts);
            string mysql = "Select * from Financial_Period Where DateTo= " + operations.Get_DateToInsert(dateFrom.Date) + " "; ;
            DataTable dt = opp.return_DataTable(mysql);
            if (dt.Rows.Count > 0)
            {
                Year_Financial_Period_Id_Prev = long.Parse(dt.Rows[0]["Id"].ToString());

            }
            return dt;
        }
        public long Get_Year_Financial_Period_Id_Next(ref operations opp, DateTime dateto)
        {
            long Year_Financial_Period_Id_New = 0;
            dateto = dateto.AddDays(1);
            string mysql = "Select * from Financial_Period Where DateFrom= " + operations.Get_DateToInsert(dateto.Date) + " "; ;
            DataTable dt = opp.return_DataTable(mysql);
            if (dt.Rows.Count > 0)
            {
                Year_Financial_Period_Id_New = long.Parse(dt.Rows[0]["Id"].ToString());

            }
            return Year_Financial_Period_Id_New;
        }

        public DataTable Get_Financial_Period_Data(ref int Excp, string mysql)
        {
            DataTable dt = new DataTable();
            try
            {
                operations opp = new operations();
                dt = opp.return_DataTable(mysql);
                Excp = 0;
                opp = null;
                return dt;
            }
            catch (Exception ee)
            {

                Excp = 1;
                //MessageBox.Show(ee.Message);
                return dt;
            }
        }
    }
}
