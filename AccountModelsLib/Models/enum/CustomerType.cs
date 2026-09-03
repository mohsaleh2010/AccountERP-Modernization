 
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations;
using System.Collections;
 
namespace AccountModelsLib.Models
{
    public class CustomerType
    {
        public static ArrayList PersonCompany_Type(bool All)
        {
            ArrayList Emp = new ArrayList();
            if (All == true)
            {
                Emp.Add(new Employees("الكل", "0"));
            }
            Emp.Add(new Employees("أفراد", "1"));
            Emp.Add(new Employees("شركة", "2"));
            Emp.Add(new Employees("أجنبى", "2"));


            return Emp;

        }

        public static string GetCustomerTypeCode(Customer customer)
        {
            string CustomerTypeCode = "P";
            if (customer == null)
            {
                CustomerTypeCode = "P";
            }
            else
            {
                long LookUp_PersonCompany_TypeId = customer.LookUp_PersonCompany_TypeId ?? 0;
                CustomerTypeCode = GetCustomerTypeCode(LookUp_PersonCompany_TypeId);
            }
            return CustomerTypeCode;
        }
        public static string GetCustomerTypeCode(long  LookUp_PersonCompany_TypeId)
        {

            string CustomerTypeCode = "P";
             
            if (LookUp_PersonCompany_TypeId == 0)
            {
                CustomerTypeCode = "P";
            }
            if (LookUp_PersonCompany_TypeId == 1)
            {
                CustomerTypeCode = "P";
            }
            if (LookUp_PersonCompany_TypeId == 2)
            {
                CustomerTypeCode = "B";

            }
            if (LookUp_PersonCompany_TypeId == 3)
            {
                CustomerTypeCode = "F";
            }

            return CustomerTypeCode;
        }
    }
    public enum CustomerTypeTax
    {
        [Display(Name = "P")]
        Person,
        [Display(Name = "B")]
        Business,
        [Display(Name = "F")]
        Foreigner
    }

}