using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib
{
    public  class Account_BranchesimagesLogoCLS
    {
        public Account_BranchesimagesLogoCLS()
        {
            
        }

        public static  DataTable  GetImageLogo()
        {
            DataTable dtimage = new DataTable();
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            string mysql = "Select * from Account_BranchesimagesLogo ";
            dtimage = opp.return_DataTable(mysql);

            return dtimage;
        }
    }
}
