using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using Microsoft.Data.SqlClient;
 
 
namespace AccountModelsLib
{
   public  class CurrencyFactorsCLS_
    {
       public   CurrencyFactorsCLS_()
       {
       }

       public void AddNewCurrency_Factors(ref int Excp, ref string Msg, string Currency_Primary_Name, int Currency_ID_Primary, string Currency_Secondary_Name, int Currency_ID_Secondary, decimal Factor)
       {
           int Count = 0;
             AccountModelsLib.operations opp = new   AccountModelsLib.operations(1);

           try
           {
               ////Insert New Factors
               int IS_TheSameCurrencies = 0;
               Count = Add_TheSameCurrency(ref   opp, Currency_Primary_Name, Currency_ID_Primary, Currency_Secondary_Name, Currency_ID_Secondary, Factor, IS_TheSameCurrencies);

               ////Insert When Factors=1 the same as   1 LE=1 LE
               IS_TheSameCurrencies = 1;
               Add_TheSameCurrency(ref   opp, Currency_Primary_Name, Currency_ID_Primary, Currency_Primary_Name, Currency_ID_Primary, 1, IS_TheSameCurrencies);

               ////Insert When Factors=1 the same as   1 $=1 $
               IS_TheSameCurrencies = 1;
               Add_TheSameCurrency(ref   opp, Currency_Secondary_Name, Currency_ID_Secondary, Currency_Secondary_Name, Currency_ID_Secondary, 1, IS_TheSameCurrencies);

               opp.Commit();
               Excp = 0;
           }
           catch (Exception ee)
           {
               Excp = 1;
               Msg = ee.Message;
               opp.Rollback();
           }


       }
       int Add_TheSameCurrency(ref   AccountModelsLib.operations opp, string Currency_Primary_Name, int Currency_ID_Primary, string Currency_Secondary_Name, int Currency_ID_Secondary, decimal Factor, int IS_TheSameCurrencies)
       {
           int Count = 0;

           string mysql = " ";

           int Currency_Primary_ISOne = 1;


           mysql = "SELECT  Currency_Factor.*  FROM   Currency_Factor ";
           mysql = mysql + " WHERE  (Currency_ID_Primary = " + Currency_ID_Primary + ") AND (Currency_ID_Secondary = " + Currency_ID_Secondary + ")";

           DataTable dt = opp.return_DataTable(mysql);

           if (dt.Rows.Count > 0)
           {
               mysql = " UPDATE   Currency_Factor SET Factor = " + Factor + " ";
               mysql = mysql + " WHERE  (Currency_ID_Primary = " + Currency_ID_Primary + ") AND (Currency_ID_Secondary = " + Currency_ID_Secondary + ")";

               Count = opp.insert_me(mysql);

           }
           else
           {
               mysql = " INSERT INTO Currency_Factor(Currency_Primary_Name, Currency_Primary_ISOne, Currency_ID_Primary, Factor, Currency_ID_Secondary, Currency_Secondary_Name,IS_TheSameCurrencies) ";
               mysql = mysql + " VALUES ('" + Currency_Primary_Name + "'," + Currency_Primary_ISOne + "," + Currency_ID_Primary + "," + Factor + "," + Currency_ID_Secondary + ",'" + Currency_Secondary_Name + "'," + IS_TheSameCurrencies + ")";

               Count = opp.insert_me(mysql);

           }
           return Count;
       }
        public decimal GetFactors(int Currency_Id_Needed, int Currency_Id)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            return GetFactors(ref opp, Currency_Id_Needed, Currency_Id);
        }


        public decimal GetFactors(ref AccountModelsLib.operations opp, int Currency_Id_Needed,int Currency_Id)
       {
           decimal Factor = 1;
           try
           {
               
               string mysql = "SELECT  Currency_Factor.*  FROM   Currency_Factor ";
               mysql = mysql + " WHERE  (Currency_ID_Primary = " + Currency_Id + ") AND (Currency_ID_Secondary = " + Currency_Id_Needed + ")";

               DataTable dt = opp.return_DataTable(mysql);

               if (dt.Rows.Count > 0)
               {
                   Factor = decimal.Parse(dt.Rows[0]["Factor"].ToString());
               }
               else
               {
                   mysql = "SELECT  Currency_Factor.*  FROM   Currency_Factor ";
                   mysql = mysql + " WHERE  (Currency_ID_Primary = " + Currency_Id_Needed + ") AND (Currency_ID_Secondary = " + Currency_Id + ")";
                   dt = opp.return_DataTable(mysql);

                   if (dt.Rows.Count > 0)
                   {
                       Factor = 1 / decimal.Parse(dt.Rows[0]["Factor"].ToString());
                   }
               }
           }
           catch { }
            return Factor;
       }

        // 🟢 جديدة: تقابل استعلام Fill_Grid فى Account_Common_LIB.Currency_Factors.cs حرفياً (نفس شرط
       // IS_TheSameCurrencies = 0 لإخفاء صفوف عامل=1 التلقائية) - لم تكن موجودة كدالة مستقلة لأن شاشة
       // الويندوز فورمز بَنَت الـ SQL مباشرة فى الفورم بدلاً من الكلاس.
       public DataTable Get_Currency_Factors_List(ref AccountModelsLib.operations opp)
       {
           opp = new AccountModelsLib.operations();
           string mysql = "Select * from Currency_Factor Where IS_TheSameCurrencies = 0";
           DataTable dt = opp.return_DataTableWithBuilder(mysql);
           return dt;
       }

       // 🟢 جديدة: تكييف تقنى بحت يقابل تعديل عمود "السعر" داخل شبكة Currency_Factors.cs وحفظه بزر
       // "حفظ التعديل" (عبر opp.ada.Update دفعة واحدة) - لا مقابل له فى Blazor فاستُبدل بتحديث صريح
       // لصف واحد بنفس الأثر بالضبط، بدون أى قاعدة عمل جديدة.
       public void Update_Factor(ref int Excp, ref string Msg, long Id, decimal Factor)
       {
           AccountModelsLib.operations opp = new AccountModelsLib.operations();
           try
           {
               string mysql = " Update Currency_Factor Set Factor=" + Factor + " Where ID=" + Id + "";
               opp.insert_me(mysql);
               Excp = 0;
           }
           catch (Exception ee)
           {
               Excp = 1;
               Msg = ee.Message;
           }
       }

       public string  Get_CurrencyName(ref AccountModelsLib.operations opp,int Currency_Id)
        {
            //opp = new AccountModelsLib.operations();
            string mysql = "Select * from Currency    Wher  Value= "+ Currency_Id;
            string Currency_Name = "";
            DataTable dt = opp.return_DataTableWithBuilder(mysql);
            if (dt.Rows.Count > 0)
            {
                  Currency_Name = dt.Rows[0]["Currency_Name"].ToString();
            }
            return Currency_Name;
        }
        public DataTable Gey_CurrencyList(ref AccountModelsLib.operations opp)
       {
           opp = new AccountModelsLib.operations();
           string mysql = "Select * from Currency Order By Value ";

           DataTable dt = opp.return_DataTableWithBuilder(mysql);
           return dt;
       }
       public void Add_Currency(ref int Excp,ref string Msg, string Currency_Name,string Currency_Name_EN,long  Value)
       {
           string mysql = " Select * from Currency Where Value=" + Value + "";
           AccountModelsLib.operations opp = new AccountModelsLib.operations();
           DataTable dt = opp.return_DataTable(mysql);
           if (dt.Rows.Count > 0)
           {
                Msg = "الكود مستخدم من قبل";
               
               Excp = 1;
               return;
           }
           int IS_GL_Currency_Flage = 1;
           mysql = @"INSERT INTO Currency (Currency_Name, Currency_Name_EN, Value, IS_GL_Currency_Flage) ";
           mysql = mysql + " VALUES ('" + Currency_Name + "','" + Currency_Name_EN + "'," + Value + "," + IS_GL_Currency_Flage + ")";

           opp.insert_me(mysql);
           Excp = 0;

       }
        public DataTable GetCurrencyList( )
        {
            string mysql = " Select * from Currency Order By Value";
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            DataTable dt = opp.return_DataTable(mysql);
            return dt;

        }
        public void Delete_Currency(ref int Excp,ref string Msg, string Value)
       {
           if (Value.Trim() == "1")
           {
                Msg = "لا يمكن حذف العمله بالجنيه المصرى";
               
               Excp = 1;
               return;
           }
           string mysql = "Select * from Submain_Account Where Currency_Id=" + Value + "";
           AccountModelsLib.operations opp = new AccountModelsLib.operations();
           DataTable dt = opp.return_DataTable(mysql);
           if (dt.Rows.Count > 0)
           {
                Msg = "لا يمكن حذف العمله نظرا لإستخدامها كعمله للحسابات" ;
               
               Excp = 1;
               return;
           }
           try
           {
               mysql = @" Delete From Currency Where Value=" + Value + "";

               opp.insert_me(mysql);
               Excp = 0;
           }
           catch (Exception exceptionObj)
            {
                Excp = 1;
                Msg = exceptionObj.Message.ToString();
                 
           }

       } 
    }
}
