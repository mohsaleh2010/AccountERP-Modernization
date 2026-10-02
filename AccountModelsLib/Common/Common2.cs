using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib 
{
    public  class Common2
    {
        public static string Journal_CodeName(ref AccountModelsLib.operations opp, long Code)
        {
            string Journal_Name = "";
            string mysql = "";

            mysql = " Select * from Journal_Codes   Where Code =" + Code + "   ";
            DataTable dt = opp.return_DataTable(mysql);
            if (dt.Rows.Count > 0)
            {
                Journal_Name = dt.Rows[0]["Journal_Name"].ToString();
            }
            return Journal_Name;
        }
        public static long Get_Journal_Code_ForSandouk(int Transaction_Type)
        {
            string mysql = "";
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            long Journal_Code_ID = 0;
            long Journal_Code_Type = 0;
            if (Transaction_Type == 1)
            {
                if (Common.Journal_Code_ForSandouk_Estlam != -1)
                {
                    Journal_Code_Type = Common.Journal_Code_ForSandouk_Estlam;
                }
                else
                {
                    Journal_Code_Type = Common.Journal_Code_ForSandouk;
                }
            }
            if (Transaction_Type == 2)
            {
                if (Common.Journal_Code_ForSandouk_Sarf != -1)
                {
                    Journal_Code_Type = Common.Journal_Code_ForSandouk_Sarf;
                }
                else
                {
                    Journal_Code_Type = Common.Journal_Code_ForSandouk;
                }
            }
            mysql = " Select * from Journal_Codes  Where Code=" + Journal_Code_Type;
            DataTable dt = opp.return_DataTable(mysql);
            if (dt.Rows.Count > 0)
            {
                Journal_Code_ID = long.Parse(dt.Rows[0]["ID"].ToString());
            }
            return Journal_Code_ID;
        }
    }
}
