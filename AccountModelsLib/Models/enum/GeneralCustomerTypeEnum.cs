 
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
    public enum GeneralCustomerTypeEnum : long
    {
        [Description("عملاء متنوعون"), Display(Name = "عملاء متنوعون")] IsGeneral_Customer_Flage = 1,
        [Description("عملاء دائمون"), Display(Name = "سعملاء دائمون")] IsNotGeneral_Customer_Flage = 0,
         
    }
   
    public class GeneralCustomerTypeClS
    {
        public static ArrayList GetGeneralCustomerType(bool All)
        {
             
            ArrayList Emp = new ArrayList();
            if (All == true)
            {
                Emp.Add(new Employees("الكل", "-1"));
            }
            Emp.Add(new Employees("عملاء متنوعون", "1"));
            Emp.Add(new Employees("عملاء دائمون", "0")); 
            return Emp;

        }

         
    }
   

}