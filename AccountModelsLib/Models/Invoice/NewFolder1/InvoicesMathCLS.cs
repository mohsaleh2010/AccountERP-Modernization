using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
   
namespace AccountModelsLib.Models
{
   public class InvoicesMathCLS
    {
        public InvoicesMathCLS()
        {

        }
        public static  bool CheckComputeQuntity(decimal Height_, decimal Width_, decimal Item_No,ref decimal LargItemsQnty)
        {
            bool flag = false;
            
            decimal num2 = Common.ComputeQuantity(Height_,   Width_,   Item_No);
            if (LargItemsQnty == num2)
            {
                flag = true;
            }
            return flag;
        }
    }
}
