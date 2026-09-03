using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TafkeetProj
{
   public  class NumberToText
   {
       List<CurrencyInfo> currencies = new List<CurrencyInfo>();

       public   NumberToText()
       {
       }
       public static string  GetText(decimal Number,int Currency_Id)
       {
            Number = Math.Round(Number, 3);
           string STR = "";
           //CurrencyInfo ee = new CurrencyInfo(CurrencyInfo.Currencies.Egypt);
           CurrencyInfo ee = new CurrencyInfo(Currency_Id);
         
           try
           {
               ToWord toWord = new ToWord(Convert.ToDecimal(Number.ToString()), ee);
               //txtEnglishWord.Text = toWord.ConvertToEnglish();
               STR = toWord.ConvertToArabic();
           }
           catch (Exception ex)
           {
               STR = "";
           }
           return STR;

       }
    }
}
