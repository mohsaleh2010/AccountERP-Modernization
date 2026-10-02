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

       public void AddNewCurrency_Factors(ref int Excp, ref string Msg, string Currency_Primary_Name, long Currency_ID_Primary, string Currency_Secondary_Name, long Currency_ID_Secondary, decimal Factor)
       {
           int Count = 0;
             AccountModelsLib.operations opp = new   AccountModelsLib.operations(1);

           try
           {
               ////Insert New Factors
               int IS_TheSameCurrencies = 0;
               Count = Add_TheSameCurrency(ref   opp, Currency_Primary_Name, Currency_ID_Primary, Currency_Secondary_Name, Currency_ID_Secondary, Factor, IS_TheSameCurrencies);

               // 🟢 مزامنة عكسية بطلب المستخدم صراحة (تزامن مزدوج بين شاشتَى "العملات" و"أسعار العملات") -
               // راجع تعليق SyncCurrencyRateFromFactor لتفاصيل الشرط والاتجاه.
               SyncCurrencyRateFromFactor(ref opp, Currency_ID_Primary, Currency_ID_Secondary, Factor);

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
       int Add_TheSameCurrency(ref   AccountModelsLib.operations opp, string Currency_Primary_Name, long Currency_ID_Primary, string Currency_Secondary_Name, long Currency_ID_Secondary, decimal Factor, int IS_TheSameCurrencies)
       {
           int Count = 0;

           string mysql = " ";

           int Currency_Primary_ISOne = 1;


           mysql = "SELECT  Currency_Factor.*  FROM   Currency_Factor ";
           mysql = mysql + " WHERE  (Currency_ID_Primary = " + Currency_ID_Primary + ") AND (Currency_ID_Secondary = " + Currency_ID_Secondary + ")";

           DataTable dt = opp.return_DataTable(mysql);

           // 🐛 Factor (decimal) هنا كان يُدمَج فى نص SQL عبر التسلسل الضمنى ToString() الذى يتبع ثقافة
           // الخيط السائدة وقت التنفيذ - نفس خطأ الفاصلة العشرية الموثَّق فى AccountCultureGuard/LanguageService
           // (الأرقام العشرية بثقافة ar-EG تُنسَّق بفاصل "٫" بدل "." فتُنتِج SQL غير صالح بصمت). ظهر هذا فعلياً
           // عند إضافة الربط التلقائى Currency↔Currency_Factor من CurrencyCLS.SaveCurrencyAsync (يُستدعى
           // مباشرة من دائرة Blazor بدون Task.Run/AccountCultureGuard) - قيمة السعر لم تُحدَّث فى القاعدة رغم
           // نجاح الحفظ ظاهرياً. الإصلاح: فرض InvariantCulture صراحة عند التحويل، بغض النظر عن ثقافة المستدعى.
           string FactorText = Factor.ToString(System.Globalization.CultureInfo.InvariantCulture);

           if (dt.Rows.Count > 0)
           {
               mysql = " UPDATE   Currency_Factor SET Factor = " + FactorText + " ";
               mysql = mysql + " WHERE  (Currency_ID_Primary = " + Currency_ID_Primary + ") AND (Currency_ID_Secondary = " + Currency_ID_Secondary + ")";

               Count = opp.insert_me(mysql);

           }
           else
           {
               mysql = " INSERT INTO Currency_Factor(Currency_Primary_Name, Currency_Primary_ISOne, Currency_ID_Primary, Factor, Currency_ID_Secondary, Currency_Secondary_Name,IS_TheSameCurrencies) ";
               mysql = mysql + " VALUES ('" + Currency_Primary_Name + "'," + Currency_Primary_ISOne + "," + Currency_ID_Primary + "," + FactorText + "," + Currency_ID_Secondary + ",'" + Currency_Secondary_Name + "'," + IS_TheSameCurrencies + ")";

               Count = opp.insert_me(mysql);

           }
           return Count;
       }

       // 🟢 مزامنة عكسية بطلب المستخدم صراحة: تعديل السعر من شاشة "أسعار العملات" (Currency_Factor) يُحدِّث
       // عمود Currency.rate المقابل أيضاً - نفس مبدأ CurrencyCLS.SaveCurrencyAsync (الاتجاه الآخر) لكن هنا
       // بتحديث مباشر لعمود rate فقط، بدون استدعاء SaveCurrencyAsync، لتفادى أى حلقة استدعاء دائرية بين
       // الاتجاهين. تُطبَّق فقط عندما يكون أحد طرفَى الزوج هو العملة الرئيسية (GL) - فـCurrency.rate حقل واحد
       // فقط لكل عملة يمثل سعرها *مقابل الرئيسية تحديداً*، فلا معنى لمزامنته من معامل تحويل متقاطع بين عملتين
       // أجنبيتين (مثلاً دولار↔يورو). فشل هذه الخطوة (مثلاً: لا توجد عملة رئيسية مُعرَّفة بعد) لا يُفشِل حفظ
       // معامل العملة نفسه - نفس مبدأ معالجة الأخطاء غير الحرجة المُتَّبَع فى بقية هذا الكلاس.
       void SyncCurrencyRateFromFactor(ref AccountModelsLib.operations opp, long Currency_ID_Primary, long Currency_ID_Secondary, decimal Factor)
       {
           try
           {
               DataTable glDt = opp.return_DataTable("SELECT Value FROM Currency WHERE IS_GL_Currency_Flage = 1");
               if (glDt.Rows.Count == 0) return;
               long GL_Value = Convert.ToInt64(glDt.Rows[0]["Value"]);

               long OtherCurrencyId;
               decimal Rate;
               if (Currency_ID_Secondary == GL_Value && Currency_ID_Primary != GL_Value)
               {
                   // العملة الأولية هى العملة الأجنبية، الثانوية هى الرئيسية - Factor يطابق تعريف Currency.rate حرفياً
                   OtherCurrencyId = Currency_ID_Primary;
                   Rate = Factor;
               }
               else if (Currency_ID_Primary == GL_Value && Currency_ID_Secondary != GL_Value)
               {
                   // العكس: الرئيسية أولية، فالمعامل هو "1 رئيسية = Factor أجنبية" - Currency.rate يحتاج المعكوس
                   OtherCurrencyId = Currency_ID_Secondary;
                   Rate = Factor == 0 ? 0 : 1 / Factor;
               }
               else
               {
                   return; // لا علاقة لأى طرف بالعملة الرئيسية (معامل تحويل متقاطع) - لا شىء لمزامنته
               }

               string RateText = Rate.ToString(System.Globalization.CultureInfo.InvariantCulture);
               opp.insert_me("UPDATE Currency SET rate = " + RateText + " WHERE Value = " + OtherCurrencyId);
           }
           catch
           {
               // غير حرج - راجع التعليق أعلى الدالة
           }
       }

        public decimal GetFactors(long Currency_Id_Needed, long Currency_Id)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            return GetFactors(ref opp, Currency_Id_Needed, Currency_Id);
        }


        public decimal GetFactors(ref AccountModelsLib.operations opp, long Currency_Id_Needed,long Currency_Id)
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
               // 🐛 نفس إصلاح InvariantCulture أعلاه فى Add_TheSameCurrency - راجع تعليقها لتفاصيل الخطأ.
               string mysql = " Update Currency_Factor Set Factor=" + Factor.ToString(System.Globalization.CultureInfo.InvariantCulture) + " Where ID=" + Id + "";
               opp.insert_me(mysql);

               // 🟢 مزامنة عكسية (راجع SyncCurrencyRateFromFactor) - صفوف "عامل=1" الذاتية (IS_TheSameCurrencies=1)
               // مُستبعَدة عمداً، لا معنى لمزامنتها.
               DataTable rowDt = opp.return_DataTable("SELECT Currency_ID_Primary, Currency_ID_Secondary, IS_TheSameCurrencies FROM Currency_Factor WHERE ID=" + Id);
               if (rowDt.Rows.Count > 0 && Convert.ToInt32(rowDt.Rows[0]["IS_TheSameCurrencies"]) == 0)
               {
                   int primaryId = Convert.ToInt32(rowDt.Rows[0]["Currency_ID_Primary"]);
                   int secondaryId = Convert.ToInt32(rowDt.Rows[0]["Currency_ID_Secondary"]);
                   SyncCurrencyRateFromFactor(ref opp, primaryId, secondaryId, Factor);
               }

               Excp = 0;
           }
           catch (Exception ee)
           {
               Excp = 1;
               Msg = ee.Message;
           }
       }

       public string  Get_CurrencyName(ref AccountModelsLib.operations opp,long  Currency_Id)
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
