using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib
{
    public class Account_Entry_Process
    {
        public Account_Entry_Process()
        {

        }
        long Account_Entry_List_ID = 0;
        long  User_ID_Login = 0;
        string User_Id_Login_Name = "";
        long  Branch_ID_Login = 0;
        long Entry_No = 0;
        DateTime Entry_Date = new DateTime();
        int journal_Code = 0;
        long    Currency_Id = 0;
        string Entry_Notes = "";
        int Transform_Type = 0;

        double Entry_Total = 0;
        long   Branch_Id_Entry = 0;
        int IS_Make_AutomaticFromTar7eel = 0;

        string Doc_No = "";
        long Entry_Currency_ID = 0;
        string Entry_Currency_Name = "";
        decimal Factor = 0;
        long Order_Money_Id = 0;

        //long Year_Financial_Period_Id = 0;
        //string Journal_Name = "";

        int Reference_Type = 0;
        long Reference_Type_Table_Id = 0;
    }
}
