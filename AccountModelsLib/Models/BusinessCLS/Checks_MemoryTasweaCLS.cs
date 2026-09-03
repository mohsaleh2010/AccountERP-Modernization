using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{
    public class Checks_MemoryTasweaCLS
    {
        public Checks_MemoryTasweaCLS()
        {

        }
        public int Receipt_Taswea { get; set; }
        public int Receipt_New { get; set; }
        public long SubAccount_Id_From_Taswea { get; set; }
        public long SubAccount_Id_To_Taswea { get; set; }

        public long SubAccount_Id_From_New { get; set; }
        public long SubAccount_Id_To_New { get; set; }

        public string SubAccount_Name_From_Taswea { get; set; }
        public string SubAccount_Name_To_Taswea { get; set; }

        public string SubAccount_Name_From_New { get; set; }
        public string SubAccount_Name_To_New { get; set; }


        public int Account_Customer_Type_Value_From_Taswea { get; set; }
        public int Account_Customer_Type_Value_To_Taswea { get; set; }

        public int Account_Customer_Type_Value_From_New { get; set; }
        public int Account_Customer_Type_Value_To_New { get; set; }


        public long Account_Customer_Type_ID_From_Taswea { get; set; }
        public long Account_Customer_Type_ID_To_Taswea { get; set; }

        public long Account_Customer_Type_ID_From_New { get; set; }
        public long Account_Customer_Type_ID_To_New { get; set; }
       
            

    }
}
