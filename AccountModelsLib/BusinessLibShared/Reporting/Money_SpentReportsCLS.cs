using System;
using System.Collections;
using System.Data;

namespace AccountModelsLib.BusinessLibShared.Reporting
{
    // 🟢 نسخة Blazor من Reporting_System.Money_SpentReportsCLS (مشروع WinForms فقط، UseWindowsForms=true) -
    // نفس نمط نقل ItemsCardsCLS/CurrencyCLS/Journal_DaysListCLS سابقاً. النسخة الأصلية فى Reporting_System
    // لم تُمَس ولا تزال تُستخدَم كما هى من شاشة Money_SpentReports. الاستعلامات والشروط هنا هى نفسها بالضبط
    // بدون أي تغيير فى منطق العمل، تغيير واحد فقط: استبدال MessageBox.Show (فى Get_Data الأصلية) بـ
    // ref string Msg (نفس نمط باقى الكلاسات المنقولة).
    public class Money_SpentReportsCLS
    {
        int FrmType = 2;
        public ArrayList arr = new ArrayList();

        public Money_SpentReportsCLS(int FrmType_)
        {
        }

        public string MakeINClause(ArrayList arr)
        {
            string D = "";
            for (int x = 0; x < arr.Count; x++)
            {
                D = D + arr[x].ToString() + ",";
            }
            return D;
        }

        /// <summary>
        /// Is_MasrofatAffect =  *** // 0 mean not affect , 1 mean affect , 2 mean all
        /// </summary>
        public int Is_MasrofatAffect = 2;  //0 mean not affect , 1 mean affect , 2 mean all

        public string GetMoney_SpentDetails_mysql(ref int Excp, long Bank_Id, long Account_ID, int OrderBy, bool AllDates, DateTime DateFrom, DateTime DateTo, bool Totals, int Transaction_Type)
        {
            string MakeINClause_ = MakeINClause(arr);
            MakeINClause_ = MakeINClause_ + Account_ID.ToString();
            string mysql = "";

            mysql = @" SELECT  '' AS Order_Date_,  '' AS PayType,    Submain_Account.Id, MainSubmain_Account.SubAccount_Name AS MainSubAccount_Name, Submain_Account.SubAccount_Name,
                         Money_Transaction_Box.Export_Value, Money_Transaction_Box.Account_Id, Order_Money.Order_Date, Order_Money.Transaction_type,
                         Money_Transaction_Box.Import_Value, Money_Transaction_Box.Notes, Order_Money.Order_Notes,Money_Transaction_Box.Bank_Name
FROM            Order_Money INNER JOIN
                         Money_Transaction_Box ON Order_Money.Id = Money_Transaction_Box.Order_Mony_Id INNER JOIN
                         Submain_Account INNER JOIN
                         Submain_Account AS MainSubmain_Account ON Submain_Account.Main_Account_Id = MainSubmain_Account.Id ON Money_Transaction_Box.SubAccount_Id = Submain_Account.Id ";
            mysql = mysql + "  WHERE (Order_Money.ID>=0)   ";

            if (Transaction_Type > 0)
            {
                mysql = mysql + " AND  (Order_Money.Transaction_type  =  " + Transaction_Type + " ) ";
            }
            if (Bank_Id > 0)
            {
                mysql = mysql + " AND  (Money_Transaction_Box.Bank_Id  =  " + Bank_Id + " ) ";
            }

            //AND (Submain_Account.Head_Main_Account = 3)
            if (AllDates == false)
            {
                mysql = mysql + " AND  (Order_Money.Order_Date >= '" + operations.dateFormat(DateFrom) + "') AND  (Order_Money.Order_Date <= '" + operations.dateFormat(DateTo) + "') ";
            }

            if (Is_MasrofatAffect < 2)
            {
                mysql = mysql + " AND  (Submain_Account.Is_MasrofatAffect = " + Is_MasrofatAffect + ")   ";
            }


            if (arr.Count > 0)
            {
                mysql = mysql + "AND Submain_Account.Id  IN ( " + MakeINClause_ + ")";
            }

            if (OrderBy == 0)
            {
                mysql = mysql + " ORDER BY Order_Money.Order_Date ";
            }
            else
            {
                mysql = mysql + " ORDER BY Submain_Account.Id ";
            }
            return mysql;
        }

        public DataSet GetMoney_SpentDetails(ref int Excp, ref string Msg, long Bank_Id, long Account_ID, int OrderBy, bool AllDates, DateTime DateFrom, DateTime DateTo, bool Totals, int Transaction_Type)
        {
            string mysql = GetMoney_SpentDetails_mysql(ref Excp, Bank_Id, Account_ID, OrderBy, AllDates, DateFrom, DateTo, Totals, Transaction_Type);
            return Get_Data(ref Excp, ref Msg, mysql);
        }

        public string GetCheck_SpentDetails_mysql(ref int Excp, int OrderBy, long Account_ID, bool AllDates, DateTime DateFrom, DateTime DateTo, bool Totals, int Transaction_Type)
        {
            string MakeINClause_ = MakeINClause(arr);
            MakeINClause_ = MakeINClause_ + Account_ID.ToString();

            string mysql = "";

            mysql = @" SELECT        '' AS Order_Date_, '' AS PayType, Submain_Account.Id, MainSubmain_Account.SubAccount_Name AS MainSubAccount_Name, Submain_Account.SubAccount_Name, Checks_Memory.Export_Value,
                         Checks_Memory.Account_Id, Checks_Memory.Import_Value, Order_Money_Check.Transaction_type, Order_Money_Check.Order_Date
FROM            Checks_Memory INNER JOIN
                         Submain_Account INNER JOIN
                         Submain_Account AS MainSubmain_Account ON Submain_Account.Main_Account_Id = MainSubmain_Account.Id ON Checks_Memory.SubAccount_Id = Submain_Account.Id INNER JOIN
                         Order_Money_Check ON Checks_Memory.Order_Mony_Id = Order_Money_Check.Id ";
            mysql = mysql + "  WHERE (Order_Money_Check.ID> 0)    ";

            if (Transaction_Type > 0)
            {
                mysql = mysql + " AND  (Order_Money_Check.Transaction_type  =  " + Transaction_Type + " ) ";
            }
            if (Is_MasrofatAffect > 1)
            {
                mysql = mysql + " AND  (Submain_Account.Is_MasrofatAffect = " + Is_MasrofatAffect + ")   ";
            }
            //AND (Submain_Account.Head_Main_Account = 3)
            if (AllDates == false)
            {
                mysql = mysql + " AND  (Order_Money_Check.Order_Date >= '" + operations.dateFormat(DateFrom) + "') AND  (Order_Money_Check.Order_Date <= '" + operations.dateFormat(DateTo) + "') ";
            }
            if (arr.Count > 0)
            {
                mysql = mysql + "AND Submain_Account.Id  IN ( " + MakeINClause_ + ")";
            }
            if (OrderBy == 0)
            {
                mysql = mysql + " ORDER BY Order_Money_Check.Order_Date ";
            }
            else
            {
                mysql = mysql + " ORDER BY Submain_Account.Id ";
            }
            return mysql;
        }

        public DataSet GetCheck_SpentDetails(ref int Excp, ref string Msg, int OrderBy, long Account_ID, bool AllDates, DateTime DateFrom, DateTime DateTo, bool Totals, int Transaction_Type)
        {
            string mysql = GetCheck_SpentDetails_mysql(ref Excp, OrderBy, Account_ID, AllDates, DateFrom, DateTo, Totals, Transaction_Type);
            return Get_Data(ref Excp, ref Msg, mysql);
        }

        public DataSet GetTotalMoney_SpentDetails_(ref int Excp, ref string Msg, int OrderBy, long Bank_Id, long Account_ID, bool AllDates, DateTime DateFrom, DateTime DateTo, bool Totals, int Transaction_Type)
        {
            string MakeINClause_ = MakeINClause(arr);
            MakeINClause_ = MakeINClause_ + Account_ID.ToString();
            string mysql = "";

            string Bank_NameGroupBy = "";
            string Bank_NameColumn = "";
            if (Bank_Id > 0)
            {
                Bank_NameGroupBy = " ,    Bank_Name ";
                Bank_NameColumn = ", Money_Transaction_Box.Bank_Name ";
            }
            else
            {
                Bank_NameGroupBy = "";
                Bank_NameColumn = " ,'' AS Bank_Name ";
            }


            mysql = @" SELECT  '' AS Order_Date_, '' AS PayType,  MainSubmain_Account.SubAccount_Name AS MainSubAccount_Name, Submain_Account.SubAccount_Name, SUM(Money_Transaction_Box.Export_Value) AS Export_Value,SUM(Money_Transaction_Box.Import_Value) AS Import_Value,    Order_Money.Transaction_type ";
            mysql = mysql + Bank_NameColumn;
            mysql = mysql + @" FROM Submain_Account INNER JOIN
                         Submain_Account AS MainSubmain_Account ON Submain_Account.Main_Account_Id = MainSubmain_Account.Id INNER JOIN
                         Money_Transaction_Box ON Submain_Account.Id = Money_Transaction_Box.SubAccount_Id INNER JOIN
                         Order_Money ON Money_Transaction_Box.Order_Mony_Id = Order_Money.Id  ";

            mysql = mysql + "  WHERE (Order_Money.ID >0)   ";
            if (Transaction_Type > 0)
            {
                mysql = mysql + " AND  (Order_Money.Transaction_type  =  " + Transaction_Type + " ) ";
            }
            if (Bank_Id > 0)
            {
                mysql = mysql + " AND  (Money_Transaction_Box.Bank_Id  =  " + Bank_Id + " ) ";
            }

            if (AllDates == false)
            {
                mysql = mysql + " AND  (Order_Money.Order_Date >= '" + operations.dateFormat(DateFrom) + "') AND  (Order_Money.Order_Date <= '" + operations.dateFormat(DateTo) + "') ";
            }

            if (Is_MasrofatAffect > 1)
            {
                mysql = mysql + " AND  (Submain_Account.Is_MasrofatAffect = " + Is_MasrofatAffect + ")   ";
            }

            if (arr.Count > 0)
            {
                mysql = mysql + "AND Submain_Account.Id  IN ( " + MakeINClause_ + ")";
            }
            mysql = mysql + " GROUP BY Order_Money.Transaction_type, Submain_Account.SubAccount_Name, MainSubmain_Account.SubAccount_Name, Submain_Account.Id ";
            mysql = mysql + Bank_NameGroupBy;
            mysql = mysql + " ORDER BY Submain_Account.Id ";
            return Get_Data(ref Excp, ref Msg, mysql);
        }

        public DataSet GetTotalCheck_SpentDetails_(ref int Excp, ref string Msg, int OrderBy, long Account_ID, bool AllDates, DateTime DateFrom, DateTime DateTo, bool Totals, int Transaction_Type)
        {
            string MakeINClause_ = MakeINClause(arr);
            MakeINClause_ = MakeINClause_ + Account_ID.ToString();

            string mysql = "";

            mysql = @" SELECT        '' AS Order_Date_, '' AS PayType, MainSubmain_Account.SubAccount_Name AS MainSubAccount_Name, Submain_Account.SubAccount_Name, SUM(Checks_Memory.Export_Value) AS Export_Value, SUM(Checks_Memory.Import_Value) AS Import_Value, Order_Money_Check.Transaction_type AS Order_Money_Check
                  FROM Order_Money_Check INNER JOIN Checks_Memory ON Order_Money_Check.Id = Checks_Memory.Order_Mony_Id INNER JOIN Submain_Account INNER JOIN  Submain_Account AS MainSubmain_Account ON Submain_Account.Main_Account_Id = MainSubmain_Account.Id ON Checks_Memory.SubAccount_Id = Submain_Account.Id  ";
            mysql = mysql + "  WHERE (Order_Money_Check.Id >0)   ";
            if (Transaction_Type > 0)
            {
                mysql = mysql + " AND  (Order_Money_Check.Transaction_type  =  " + Transaction_Type + " ) ";
            }
            if (Is_MasrofatAffect > 1)
            {
                mysql = mysql + " AND  (Submain_Account.Is_MasrofatAffect = " + Is_MasrofatAffect + ")   ";
            }
            if (AllDates == false)
            {
                mysql = mysql + " AND  (Order_Money_Check.Order_Date >= '" + operations.dateFormat(DateFrom) + "') AND  (Order_Money_Check.Order_Date <= '" + operations.dateFormat(DateTo) + "') ";
            }
            if (arr.Count > 0)
            {
                mysql = mysql + "AND Submain_Account.Id  IN ( " + MakeINClause_ + ")";
            }

            mysql = mysql + " GROUP BY Submain_Account.SubAccount_Name, MainSubmain_Account.SubAccount_Name, Order_Money_Check.Transaction_type ";
            mysql = mysql + " ORDER BY MainSubAccount_Name ";
            return Get_Data(ref Excp, ref Msg, mysql);
        }

        // 🟢 دالة بحث تقنية جديدة (غير موجودة فى النسخة الأصلية): تُرجع نفس مجموعة الحسابات التى تصل إليها
        // شاشة الويندوز فورمز الأصلية فى وضع "يومية مصروفات" (Show_MasrofateForDay=1) عبر تحديد شجرة الحسابات
        // بالكامل تلقائياً (Masrofat_Only=3 يُقيّد شجرة AccountsTree.Get_SubMain_Account_WithFirstBalance على
        // Head_Main_Account=3 ثم يضغط الكود "موافق" على أول عقدة) والضغط "موافق" - بديل تقنى بحت لتفاعل شجرة
        // اختيار متعددة لم تُبنَ فى Blazor بعد، بدون أى قاعدة عمل جديدة: نفس شرط Head_Main_Account=3 المستخدَم
        // فى الفورم الأصلى.
        public ArrayList Get_SubAccountIdsByHeadMainAccount(int HeadMainAccount)
        {
            ArrayList result = new ArrayList();
            operations opp = new operations();
            string mysql = "Select Id from Submain_Account Where Head_Main_Account = " + HeadMainAccount;
            DataTable dt = opp.return_DataTable(mysql);
            foreach (DataRow row in dt.Rows)
            {
                result.Add(row["Id"]);
            }
            return result;
        }

        private DataSet Get_Data(ref int Excp, ref string Msg, string mysql)
        {
            DataSet G_DS = new DataSet();

            try
            {
                operations opp = new operations();
                G_DS = opp.return_dataset(mysql);
                Excp = 0;
                opp = null;
                return G_DS;
            }
            catch (Exception ee)
            {
                Excp = -1;
                Msg = ee.Message;
                return G_DS;
            }
        }
    }
}
