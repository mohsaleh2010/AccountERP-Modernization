 
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace AccountModelsLib.Models
{
    public enum StorePriceTypeEnum : long
    {
        [Description("متوسط التكلفة"), Display(Name = "متوسط التكلفة شراء")] P_PriceAvgCost = 1,
        [Description("أخر سعر شراء"), Display(Name = "أخر سعر شراء")] LastP_Price = 2,
        [Description("متوسط التكلفة"), Display(Name = "متوسط التكلفة بيع")] S_PriceAvgCost = 3,
        [Description("أخر سعر شراء"), Display(Name = "أخر سعر بيع")] LastS_Price = 4,
    }
   
    public class StorePriceTypeEnumClS
    {
        public static ArrayList GetStoreP_PriceTypeEnum(bool All)
        {
             
            ArrayList Emp = new ArrayList();
            if (All == true)
            {
                Emp.Add(new Employees("الكل", "0"));
            }
            Emp.Add(new Employees("متوسط التكلفة شراء", "1"));
            Emp.Add(new Employees("أخر سعر شراء", "2")); 
            return Emp;

        }
        public static ArrayList GetStoreS_PriceTypeEnum(bool All)
        {

            ArrayList Emp = new ArrayList();
            if (All == true)
            {
                Emp.Add(new Employees("الكل", "0"));
            }
            Emp.Add(new Employees("متوسط التكلفة بيع", "3"));
            Emp.Add(new Employees("أخر سعر بيع", "4"));
            return Emp;

        }

    }
   

}