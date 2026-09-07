using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using Microsoft.Data.SqlClient;
using System.Data.OleDb;
 
 
using AccountModelsLib.Models;
namespace AccountModelsLib
{
    public class FirstBalance_CLS
    {
        public FirstBalance_CLS()
        {
        }

        public DataTable Get_FirstBalance(long Submain_Account_ID, long  Year_Financial_Period_Id, long Account_Currency_Id, long  Branch_Id, long Equivalent_Account_Currency_Id)
        {

            string mysql = @"SELECT        Account_Entry_Master_Balance.*
FROM            Account_Entry_Master_Balance
WHERE        (Submain_Account_ID = " + Submain_Account_ID + ") AND (Year_Financial_Period_Id = " + Year_Financial_Period_Id + ") ";
            if (Branch_Id > 0)
            {
                mysql = mysql + " AND (Branch_Id = " + Branch_Id + ")  ";
            }
            //if (Account_Currency_Id > 0)
            //{
            //    mysql = mysql + "   AND (Account_Currency_Id = " + Account_Currency_Id + ")";
            //}
            mysql = mysql + "   AND (Equivalent_Account_Currency_Id = " + Equivalent_Account_Currency_Id + ")";

            operations opp = new operations();
            DataTable dt = opp.return_DataTable(mysql);
            return dt;
        }
        public bool IsCheckLastLevel = false;
        public int Account_Customer_Type_Value = 0;
        public long Account_Customer_Type_ID = 0;

        public void Save_FirstBalance(ref int Excp, ref string Msg,ref operations opp, long Submain_Account_ID, string SubAccount_Name, long  Year_Financial_Period_Id, long  Branch_Id, long Account_Currency_Id, decimal First_Balance, int Transform_Type, decimal EquivalentValue)
        {
             
            string mysql = "";
            try
            {
                ///التأكد من عدم وجود رصيد سابق للحساب لتلك الفترة الماليه
                mysql = @"SELECT        Account_Entry_Master_Balance.* FROM            Account_Entry_Master_Balance ";
                 //mysql=mysql+ " WHERE        (Submain_Account_ID = " + Submain_Account_ID + ") AND (Year_Financial_Period_Id = " + Year_Financial_Period_Id + ") AND (Branch_Id = " + Branch_Id + ")  AND (Account_Currency_Id = "  + Account_Currency_Id + " ) AND (Equivalent_Account_Currency_Id = " +Common.Entry_Currency_Id + ")";
                 mysql=mysql+ " WHERE        (Submain_Account_ID = " + Submain_Account_ID + ") AND (Year_Financial_Period_Id = " + Year_Financial_Period_Id + ") AND (Branch_Id = " + Branch_Id + ")   AND (Equivalent_Account_Currency_Id = " + Account_Currency_Id + ")";

                
                DataTable dt_Master_Balance = opp.return_DataTable(mysql);

                if (dt_Master_Balance.Rows.Count == 0)
                {
                    if (IsCheckLastLevel == false)
                    {
                        Submain_AccountCLS submain_AccountCLS = new Submain_AccountCLS();
                        DataTable dtt2 = submain_AccountCLS.GetMainSubmain_Account(ref opp, Submain_Account_ID);


                        if (dtt2.Rows.Count > 0)
                        {
                            Excp = 1;
                            Msg = "يجب وضع الرصيد للحسابات الفرعيه لهذا  الحساب الرئيسى ";
                            //opp.Rollback();
                            return;
                        }
                    }
                    Make_New_FirstBalance(ref opp, ref Msg, Submain_Account_ID, SubAccount_Name, Year_Financial_Period_Id, Branch_Id, Account_Currency_Id, First_Balance, Transform_Type, EquivalentValue);
                }
                else
                {
                    decimal New_First_Balance = First_Balance;
                    //Make_Update_FirstBalance(ref   opp, dt_Master_Balance, Submain_Account_ID, Year_Financial_Period_Id, Branch_Id, Account_Currency_Id, Transform_Type, New_First_Balance, EquivalentValue);
                    Make_Update_FirstBalance_New(ref opp, dt_Master_Balance, Submain_Account_ID, Year_Financial_Period_Id, Branch_Id, Account_Currency_Id, Transform_Type, New_First_Balance, EquivalentValue);
                    //Make_Update_EquivalentValueFirstBalance(ref   opp, Submain_Account_ID, Year_Financial_Period_Id, Branch_Id, Currency_Id, Transform_Type, New_First_Balance, EquivalentValue);
                }
                UpdateEquivalentFirstValue(ref opp, Submain_Account_ID, Year_Financial_Period_Id, Branch_Id);
                SetCustomer_SupplierAccountFirstBalance(ref opp, Submain_Account_ID, First_Balance, Transform_Type, Year_Financial_Period_Id, "");
                 
                // 👈 ضف هذا الاستدعاء هنا ليتم ضبط التوازن فوراً بعد كل إضافة/تعديل
                BalanceOpeningBalanceEquity(ref opp, Year_Financial_Period_Id, Branch_Id, Account_Currency_Id );
                Msg = "تم تعديل رصيد أول المده";
                Excp = 0;
            }
            catch (Exception ee)
            {

                Msg = "خطأ فى تعديل رصيد أول المده";
                Msg = Msg + "\r\n" + ee.Message;
                Excp = 1;
            }
        }
        public void Save_FirstBalance(ref int Excp, ref string Msg, long Submain_Account_ID, string SubAccount_Name, long  Year_Financial_Period_Id, long  Branch_Id, long Account_Currency_Id, decimal First_Balance, int Transform_Type, decimal EquivalentValue)
        {
            operations opp = new operations(1);
            
                Save_FirstBalance(ref Excp, ref Msg, ref opp, Submain_Account_ID, SubAccount_Name, Year_Financial_Period_Id, Branch_Id, Account_Currency_Id, First_Balance, Transform_Type, EquivalentValue);
                if (Excp == 0)
                {
                    opp.Commit();
                }
                else
                {
                    opp.Rollback();
                }

            
            
        }
        public void BalanceOpeningBalanceEquity(ref operations opp, long financialPeriodId, long branchId, long currencyId )
        {
            long equityAccountId = 0;
            long Account_Currency_Id= 0;
            // 1. جلب أو إنشاء حساب الرصيد الافتتاحي
            DataTable dt= GetOrCreateOpeningBalanceAccount(ref opp  );
            if (dt != null && dt.Rows.Count > 0)
            {
                  equityAccountId = Convert.ToInt64(dt.Rows[0]["Id"]);
                Account_Currency_Id = Convert.ToInt64(dt.Rows[0]["Currency_Id"]);
            }
            // 2. تجميع كافة الأرصدة الافتتاحية للمدين والدائن لجميع الحسابات (مع استبعاد حساب الرصيد الافتتاحي نفسه)
            string querySum = $@"
        SELECT 
            ISNULL(SUM(First_Balance_Madin), 0) AS TotalMadin,
            ISNULL(SUM(First_Balance_Da2n), 0) AS TotalDa2n,
            ISNULL(SUM(Equivalent_First_Balance_Madin), 0) AS TotalEqMadin,
            ISNULL(SUM(Equivalent_First_Balance_Da2n), 0) AS TotalEqDa2n
        FROM Account_Entry_Master_Balance 
        WHERE Year_Financial_Period_Id = {financialPeriodId} 
          AND Branch_Id = {branchId}
        AND Equivalent_Account_Currency_Id = {currencyId}
          AND Submain_Account_ID <> {equityAccountId}";

            DataTable dtTotals = opp.return_DataTable(querySum);

            if (dtTotals == null || dtTotals.Rows.Count == 0) return;

            decimal totalMadin = Convert.ToDecimal(dtTotals.Rows[0]["TotalMadin"]);
            decimal totalDa2n = Convert.ToDecimal(dtTotals.Rows[0]["TotalDa2n"]);
            decimal totalEqMadin = Convert.ToDecimal(dtTotals.Rows[0]["TotalEqMadin"]);
            decimal totalEqDa2n = Convert.ToDecimal(dtTotals.Rows[0]["TotalEqDa2n"]);

            // 3. حساب الفارق المطلوب لوزن الكفة
            decimal diff = totalMadin - totalDa2n;
            decimal eqDiff = totalEqMadin - totalEqDa2n;

            decimal firstBalanceMadin = 0;
            decimal firstBalanceDa2n = 0;
            decimal eqFirstBalanceMadin = 0;
            decimal eqFirstBalanceDa2n = 0;
            decimal finalFirstBalance = 0;
            int transformType = 0; // 1 = مدين، 2 = دائن

            if (diff > 0)
            {
                // إجمالي المدين أكبر -> الفارق يوضع دائن لتعادلا
                firstBalanceDa2n = diff;
                eqFirstBalanceDa2n = eqDiff;
                
                transformType = 2;
            }
            else if (diff < 0)
            {
                finalFirstBalance = diff * -1; // بالماينس للإشارة للدائن
                eqFirstBalanceMadin = eqDiff * -1; // بالماينس للإشارة للدائن
                // إجمالي الدائن أكبر -> الفارق يوضع مدين لتعادلا
                firstBalanceMadin = Math.Abs(diff);
                eqFirstBalanceMadin = Math.Abs(eqDiff);
                finalFirstBalance = Math.Abs(diff);
                transformType = 1;
            }
            else
            {
                // الأرصدة متوازنة تماماً
                finalFirstBalance = 0;
            }

            // 4. التحقق إذا كان سجل حساب الرصيد الافتتاحي موجوداً في جدول الأستاذ لهذه الفترة والفرع
            string checkEquityBalance = $@"
        SELECT Id FROM Account_Entry_Master_Balance 
        WHERE Submain_Account_ID = {equityAccountId} 
          AND Year_Financial_Period_Id = {financialPeriodId} 
          AND Branch_Id = {branchId}  AND Equivalent_Account_Currency_Id = {currencyId}";

            DataTable dtEquityBal = opp.return_DataTable(checkEquityBalance);

            if (dtEquityBal != null && dtEquityBal.Rows.Count > 0)
            {
                // تحديث السجل الحالي
                long recordId = Convert.ToInt64(dtEquityBal.Rows[0]["Id"]);
                string updateSql = $@"
            UPDATE Account_Entry_Master_Balance SET 
                First_Balance = {finalFirstBalance},
                First_Balance_Madin = {firstBalanceMadin},
                First_Balance_Da2n = {firstBalanceDa2n},
                Balance = {finalFirstBalance},
                Balance_Madin = {firstBalanceMadin},
                Balance_Da2n = {firstBalanceDa2n},
                Equivalent_First_Balance = {eqDiff * (transformType == 2 ? -1 : 1)},
                Equivalent_First_Balance_Madin = {eqFirstBalanceMadin},
                Equivalent_First_Balance_Da2n = {eqFirstBalanceDa2n},
                Equivalent_Balance_Madin = {eqFirstBalanceMadin},
                Equivalent_Balance_Da2n = {eqFirstBalanceDa2n},
                IS_Madin_Da2n = {transformType},
                Update_ServerDateTime = GETDATE(),
                Edit_User_Id = {Common.User_Id}
            WHERE Id = {recordId}";

                 opp.insert_me(updateSql);
            }
            else
            {
                // إدراج سجل جديد للرصيد الافتتاحي
                var equityRecord = new Account_Entry_Master_Balance
                {
                    Submain_Account_ID = equityAccountId,
                    SubAccount_Name = "حساب الرصيد الافتتاحي",
                    Year_Financial_Period_Id = financialPeriodId,
                    Branch_Id = branchId,
                    Account_Currency_Id = Account_Currency_Id,
                    Equivalent_Account_Currency_Id = currencyId,
                    First_Balance = finalFirstBalance,
                    First_Balance_Madin = firstBalanceMadin,
                    First_Balance_Da2n = firstBalanceDa2n,
                    Balance = finalFirstBalance,
                    Balance_Madin = firstBalanceMadin,
                    Balance_Da2n = firstBalanceDa2n,
                    Equivalent_First_Balance = eqDiff * (transformType == 2 ? -1 : 1),
                    Equivalent_First_Balance_Madin = eqFirstBalanceMadin,
                    Equivalent_First_Balance_Da2n = eqFirstBalanceDa2n,
                    Equivalent_Balance_Madin = eqFirstBalanceMadin,
                    Equivalent_Balance_Da2n = eqFirstBalanceDa2n,
                    IS_Madin_Da2n = transformType,
                    OperationDate = DateTime.Now,
                    User_Id = Common.User_Id,
                    
                    Insert_ServerDateTime = DateTime.Now,
                    Factor = 1
                };

                string insertSql = $@"
            INSERT INTO Account_Entry_Master_Balance
            (Submain_Account_ID, SubAccount_Name, Year_Financial_Period_Id, Branch_Id, Account_Currency_Id, Equivalent_Account_Currency_Id, First_Balance, First_Balance_Madin, First_Balance_Da2n, Balance, Balance_Madin, Balance_Da2n, Equivalent_First_Balance, Equivalent_First_Balance_Madin, Equivalent_First_Balance_Da2n, Equivalent_Balance_Madin, Equivalent_Balance_Da2n, IS_Madin_Da2n, OperationDate, User_Id, Factor)
            VALUES
            ({equityRecord.Submain_Account_ID}, N'{equityRecord.SubAccount_Name}', {equityRecord.Year_Financial_Period_Id}, {equityRecord.Branch_Id}, {equityRecord.Account_Currency_Id}, {equityRecord.Equivalent_Account_Currency_Id}, {equityRecord.First_Balance}, {equityRecord.First_Balance_Madin}, {equityRecord.First_Balance_Da2n}, {equityRecord.Balance}, {equityRecord.Balance_Madin}, {equityRecord.Balance_Da2n}, {equityRecord.Equivalent_First_Balance}, {equityRecord.Equivalent_First_Balance_Madin}, {equityRecord.Equivalent_First_Balance_Da2n}, {equityRecord.Equivalent_Balance_Madin}, {equityRecord.Equivalent_Balance_Da2n}, {equityRecord.IS_Madin_Da2n}, GETDATE(), {Common.User_Id}, 1)";

                 opp.insert_me(insertSql);
            }
        }
         
        public DataTable GetOrCreateOpeningBalanceAccount(ref operations opp )
        {
            string accountName = "حساب الرصيد الافتتاحي";

            // 1. البحث هل الحساب موجود مسبقاً في شجرة الحسابات
            //string queryCheck = $"SELECT Id FROM Submain_Account WHERE SubAccount_Name = N'{accountName}' OR Code = 3999";
            //DataTable dt = opp.return_DataTable(queryCheck);
            Submain_AccountCLS submain_AccountCLS = new Submain_AccountCLS();
            DataTable dt = submain_AccountCLS.GetSubmain_Account(ref opp, Common.Account_Type_ForOpeningBalanceEquity);
           
            return dt;


        }
        public void Save_FirstBalanceForCheck(ref int Excp, ref string Msg, long Submain_Account_ID, string SubAccount_Name, long  Year_Financial_Period_Id, long  Branch_Id, int Account_Currency_Id, decimal First_Balance, int Transform_Type, decimal EquivalentValue)
        {
            operations opp = new operations(1);

            try
            {
                Save_FirstBalanceForCheck(ref Excp, ref Msg, ref opp, Submain_Account_ID, SubAccount_Name, Year_Financial_Period_Id, Branch_Id, Account_Currency_Id, First_Balance, Transform_Type, EquivalentValue);
                opp.Commit();
                Msg = "تم تعديل رصيد أول المده";
                Excp = 0;
            }
            catch (Exception ee)
            {
                opp.Rollback();
                Msg = "خطأ فى تعديل رصيد أول المده";
                Msg = Msg + "\r\n" + ee.Message;
                Excp = 1;
            }
        }
        public void Save_FirstBalanceForCheck(ref int Excp, ref string Msg, ref operations opp, long Submain_Account_ID, string SubAccount_Name, long  Year_Financial_Period_Id, long  Branch_Id, int Account_Currency_Id, decimal First_Balance, int Transform_Type, decimal EquivalentValue)
        {

            string mysql = "";

            ///التأكد من عدم وجود رصيد سابق للحساب لتلك الفترة الماليه
            mysql = @"SELECT        Account_Entry_Master_Balance.* FROM            Account_Entry_Master_Balance
                  WHERE        (Submain_Account_ID = " + Submain_Account_ID + ") AND (Year_Financial_Period_Id = " + Year_Financial_Period_Id + ") AND (Branch_Id = " + Branch_Id + ") AND (Equivalent_Account_Currency_Id = " + Account_Currency_Id + ")";
            DataTable dt_Master_Balance = opp.return_DataTable(mysql);

            if (dt_Master_Balance.Rows.Count == 0)
            {
                if (IsCheckLastLevel == false)
                {
                    ///التاكد من الحساب ليس له فروع
                    mysql = "Select * from Submain_Account  Where Main_Account_Id=" + Submain_Account_ID;
                    DataTable dtt2 = opp.return_DataTable(mysql);
                    if (dtt2.Rows.Count > 0)
                    {
                        Excp = 0;
                        Msg = "يجب وضع الرصيد للحسابات الفرعيه لتلك الحساب";
                        opp.Rollback();
                        return;
                    }
                }
                Make_New_FirstBalance(ref opp, ref Msg, Submain_Account_ID, SubAccount_Name, Year_Financial_Period_Id, Branch_Id, Account_Currency_Id, First_Balance, Transform_Type, EquivalentValue);
            }
            else
            {
                decimal New_First_Balance = First_Balance;
                //Make_Update_FirstBalance(ref   opp, dt_Master_Balance, Submain_Account_ID, Year_Financial_Period_Id, Branch_Id, Account_Currency_Id, Transform_Type, New_First_Balance, EquivalentValue);
                Make_Update_FirstBalance_NewForCheck(ref opp, dt_Master_Balance, Submain_Account_ID, Year_Financial_Period_Id, Branch_Id, Account_Currency_Id, Transform_Type, New_First_Balance, EquivalentValue);
                //Make_Update_EquivalentValueFirstBalance(ref   opp, Submain_Account_ID, Year_Financial_Period_Id, Branch_Id, Currency_Id, Transform_Type, New_First_Balance, EquivalentValue);
            }
            // UpdateEquivalentFirstValue(ref opp, Submain_Account_ID, Year_Financial_Period_Id, Branch_Id);
            // SetCustomer_SupplierAccountFirstBalance(ref opp, Submain_Account_ID, First_Balance, Transform_Type, Year_Financial_Period_Id, "");

        }

        void UpdateEquivalentFirstValue(ref operations opp, long Submain_Account_ID, long  Year_Financial_Period_Id, long  Branch_Id)
        {
            string mysql = @"SELECT SUM(Equivalent_First_Balance) AS Equivalent_First_Balance, SUM(Equivalent_First_Balance_Madin) AS Equivalent_First_Balance_Madin, SUM(Equivalent_First_Balance_Da2n) AS Equivalent_First_Balance_Da2n 
                                FROM Account_Entry_Master_Balance
                  WHERE        (Submain_Account_ID = " + Submain_Account_ID + ") AND (Year_Financial_Period_Id = " + Year_Financial_Period_Id + ") AND (Branch_Id = " + Branch_Id + ") AND (Equivalent_Account_Currency_Id <>0)";
            DataTable dt = opp.return_DataTable(mysql);
            decimal Equivalent_First_Balance = 0;
            decimal Equivalent_First_Balance_Madin = 0;
            decimal Equivalent_First_Balance_Da2n = 0;
            if (dt.Rows.Count > 0)
            {
                Equivalent_First_Balance = decimal.Parse(dt.Rows[0]["Equivalent_First_Balance"].ToString());
                Equivalent_First_Balance_Madin = decimal.Parse(dt.Rows[0]["Equivalent_First_Balance_Madin"].ToString());
                Equivalent_First_Balance_Da2n = decimal.Parse(dt.Rows[0]["Equivalent_First_Balance_Da2n"].ToString());

                decimal Diff = Equivalent_First_Balance_Madin - Equivalent_First_Balance_Da2n;

                if (Diff > 0)
                {
                    Equivalent_First_Balance = Diff;
                    Equivalent_First_Balance_Madin = Diff;
                    Equivalent_First_Balance_Da2n = 0;
                }
                if (Diff < 0)
                {
                    Diff = Diff * -1;
                    Equivalent_First_Balance = Diff;
                    Equivalent_First_Balance_Madin = 0;
                    Equivalent_First_Balance_Da2n = Diff;
                }
                if (Diff == 0)
                {
                    Equivalent_First_Balance = 0;
                    Equivalent_First_Balance_Madin = 0;
                    Equivalent_First_Balance_Da2n = 0;
                }
            }

            mysql = "Select * From  Account_Entry_Master_Balance ";
            mysql = mysql + "  WHERE        (Submain_Account_ID = " + Submain_Account_ID + ") AND (Year_Financial_Period_Id = " + Year_Financial_Period_Id + ") AND (Branch_Id = " + Branch_Id + ") AND (Equivalent_Account_Currency_Id  = 0)";
            DataTable dt2 = opp.return_DataTable(mysql);

            if (dt2.Rows.Count > 0)
            {
                mysql = @" Update Account_Entry_Master_Balance Set  First_Balance =" + Equivalent_First_Balance + ",  First_Balance_Madin=" + Equivalent_First_Balance_Madin + ",    First_Balance_Da2n =" + Equivalent_First_Balance_Da2n + ",  Equivalent_First_Balance =" + Equivalent_First_Balance + ", Equivalent_First_Balance_Madin=" + Equivalent_First_Balance_Madin + ",   Equivalent_First_Balance_Da2n =" + Equivalent_First_Balance_Da2n + " ";
                mysql = mysql + "  WHERE        (Submain_Account_ID = " + Submain_Account_ID + ") AND (Year_Financial_Period_Id = " + Year_Financial_Period_Id + ") AND (Branch_Id = " + Branch_Id + ") AND (Equivalent_Account_Currency_Id  = 0)";
                opp.insert_me(mysql);
            }
            else
            {
                mysql = @" Insert Into Account_Entry_Master_Balance( Submain_Account_ID,Year_Financial_Period_Id,Branch_Id,First_Balance  ,  First_Balance_Madin ,    First_Balance_Da2n  ,  Equivalent_First_Balance  , Equivalent_First_Balance_Madin ,   Equivalent_First_Balance_Da2n ,Equivalent_Account_Currency_Id ) ";
                mysql = mysql + " Values (  " + Submain_Account_ID + ", " + Year_Financial_Period_Id + "," + Branch_Id + "," + Equivalent_First_Balance + ",  " + Equivalent_First_Balance_Madin + ",    " + Equivalent_First_Balance_Da2n + ",  " + Equivalent_First_Balance + ",  " + Equivalent_First_Balance_Madin + ",   " + Equivalent_First_Balance_Da2n + ",0)";

                opp.insert_me(mysql);

            }

        }
        void Make_New_FirstBalance(ref operations opp, ref string Msg, long Submain_Account_ID, string SubAccount_Name, long  Year_Financial_Period_Id, long  Branch_Id, long Account_Currency_Id, decimal First_Balance, int Transform_Type, decimal EquivalentValue)
        {
            string mysql = "";

            decimal First_Balance_Madin = 0;
            decimal First_Balance_Da2n = 0;
            decimal First_Balance__ = 0;

            decimal Equivalent_First_Balance = EquivalentValue * First_Balance;
            decimal Equivalent_First_Balance_Madin = 0;
            decimal Equivalent_First_Balance_Da2n = 0;
            decimal Equivalent_First_Balance__ = 0;

             
            ///////////////تاريخ اول المده
            mysql = "Select * from Financial_Period Where Id=" + Year_Financial_Period_Id;  
            DataTable dtF = opp.return_DataTable(mysql);
            string Entry_Date = operations.Get_DateToInsert(DateTime.Now.Date);
            DateTime DateFrom_Financial = new DateTime();
            DateTime DateTo_Financial = new DateTime();
            if (dtF.Rows.Count > 0)
            {
                Entry_Date = operations.Get_DateToInsert(DateTime.Parse(dtF.Rows[0]["DateFrom"].ToString()));
                DateFrom_Financial = DateTime.Parse(dtF.Rows[0]["DateFrom"].ToString());
                DateTo_Financial = DateTime.Parse(dtF.Rows[0]["DateTo"].ToString());
            }
            if (Transform_Type == 1) /// madin
            {
                First_Balance_Madin = First_Balance;
                First_Balance_Da2n = 0;
                First_Balance__ = First_Balance;


                Equivalent_First_Balance_Madin = Equivalent_First_Balance;
                Equivalent_First_Balance_Da2n = 0;
                Equivalent_First_Balance__ = Equivalent_First_Balance;
            }
            else
            {
                /////////////////da2n
                First_Balance_Madin = 0;
                First_Balance_Da2n = First_Balance;
                First_Balance__ = First_Balance * -1;


                Equivalent_First_Balance_Madin = 0;
                Equivalent_First_Balance_Da2n = Equivalent_First_Balance;
                Equivalent_First_Balance__ = Equivalent_First_Balance * -1;
            }
            ///// اضافة الحساب فى جدول الارصده  

            //mysql = @"SELECT        Account_Entry_Master_Balance.* FROM            Account_Entry_Master_Balance
            //      WHERE        (Submain_Account_ID = " + Submain_Account_ID + ") AND (Year_Financial_Period_Id = " + Year_Financial_Period_Id + ") AND (Branch_Id = " + Branch_Id + ") AND (Account_Currency_Id = " + Account_Currency_Id + ")";
            //DataTable dt_Master_Balance = opp.return_DataTable(mysql);
            //if (dt_Master_Balance.Rows.Count == 0)
            //{

            //CurrencyFactorsCLS_ cc = new CurrencyFactorsCLS_();
            //decimal Factor = cc.GetFactors(Common.Entry_Currency_Id, Account_Currency_Id);
            //Factor = Common.RoundMe(Factor);

            /////عملة الحساب الرئيسية

            Submain_AccountCLS submain_AccountCLS = new Submain_AccountCLS();
            Submain_Account sb = submain_AccountCLS.Get_Submain_Account(ref opp, Submain_Account_ID);
            long Equivalent_Account_Currency_Id = Account_Currency_Id;
            int Account_Currency_Id_ = sb.Currency_Id ?? 1;
            mysql = "INSERT INTO Account_Entry_Master_Balance(SubAccount_Name,Submain_Account_ID, Year_Financial_Period_Id, Branch_Id, Account_Currency_Id, First_Balance, First_Balance_Madin, First_Balance_Da2n,Madin_Value,Da2n_Value,Balance,Balance_Madin,Balance_Da2n,IS_Madin_Da2n";
            mysql = mysql + ", Equivalent_Account_Currency_Id, Equivalent_First_Balance_Madin, Equivalent_First_Balance_Da2n, Equivalent_Madin_Value, Equivalent_Da2n_Value,  Equivalent_Balance_Madin, Equivalent_Balance_Da2n,Factor,Equivalent_First_Balance) ";
            mysql = mysql + " VALUES  ('" + SubAccount_Name + "'," + Submain_Account_ID + "," + Year_Financial_Period_Id + "," + Branch_Id + "," + Account_Currency_Id + "," + First_Balance__ + "," + First_Balance_Madin + "," + First_Balance_Da2n + ",0,0," + First_Balance__ + ", " + First_Balance_Madin + "," + First_Balance_Da2n + "," + Transform_Type + "";
            mysql = mysql + "," + Common.Entry_Currency_Id + "," + Equivalent_First_Balance_Madin + "," + Equivalent_First_Balance_Da2n + ",0,0," + Equivalent_First_Balance_Madin + "," + Equivalent_First_Balance_Da2n + " ," + EquivalentValue + "," + Equivalent_First_Balance + ")";
            //opp.insert_me(mysql);
            var newBalance = new Account_Entry_Master_Balance
            {
                // Id will be automatically generated by snakgenerateId inside GenericRepository AddAsync
                SubAccount_Name = SubAccount_Name,
                Submain_Account_ID = Submain_Account_ID,
                Year_Financial_Period_Id = Year_Financial_Period_Id,
                Branch_Id = Branch_Id,
                Account_Currency_Id = Account_Currency_Id_,

                // Primary balances
                First_Balance = First_Balance__,
                First_Balance_Madin = First_Balance_Madin,
                First_Balance_Da2n = First_Balance_Da2n,

                // Running values initialized to zero
                Madin_Value = 0,
                Da2n_Value = 0,

                // Current totals matching initial values
                Balance = First_Balance__,
                Balance_Madin = First_Balance_Madin,
                Balance_Da2n = First_Balance_Da2n,

                IS_Madin_Da2n = Transform_Type,

                // Equivalent (foreign currency / base currency exchange) fields
                Equivalent_Account_Currency_Id = Equivalent_Account_Currency_Id,
                Equivalent_First_Balance_Madin = Equivalent_First_Balance_Madin,
                Equivalent_First_Balance_Da2n = Equivalent_First_Balance_Da2n,
                Equivalent_Madin_Value = 0,
                Equivalent_Da2n_Value = 0,
                Equivalent_Balance_Madin = Equivalent_First_Balance_Madin,
                Equivalent_Balance_Da2n = Equivalent_First_Balance_Da2n,

                Factor = EquivalentValue,
                Equivalent_First_Balance = Equivalent_First_Balance,

                // Audit metadata
                OperationDate = DateTime.Now,
                Insert_ServerDateTime = DateTime.Now,
                Update_ServerDateTime = DateTime.Now
            };
            Account_Entry_Master_BalanceCLS account_Entry_Master_BalanceCLS= new Account_Entry_Master_BalanceCLS();
            account_Entry_Master_BalanceCLS.aemb = newBalance;
            account_Entry_Master_BalanceCLS.InsertAccount_Entry_Master_Balance(ref opp );
            
            //}

            Make_CancelTar7eelWithUpdateFirstBalance(ref opp, Submain_Account_ID, Branch_Id, DateFrom_Financial, DateTo_Financial);


        }
        void Make_Update_FirstBalance(ref operations opp, DataTable dt_Master_Balance, long Submain_Account_ID, long  Year_Financial_Period_Id, long  Branch_Id, long Account_Currency_Id, int Transform_Type, decimal New_First_Balance, decimal EquivalentValue)
        {
            decimal First_Balance__New = 0;
            decimal First_Balance = 0;//decimal.Parse(dt_Master_Balance.Rows[0]["Id"].ToString());
            decimal First_Balance_Madin = 0;//decimal.Parse(dt_Master_Balance.Rows[0]["First_Balance_Madin"].ToString());
            decimal First_Balance_Da2n = 0;//decimal.Parse(dt_Master_Balance.Rows[0]["First_Balance_Da2n"].ToString());
            decimal Madin_Value = decimal.Parse(dt_Master_Balance.Rows[0]["Madin_Value"].ToString());
            decimal Da2n_Value = decimal.Parse(dt_Master_Balance.Rows[0]["Da2n_Value"].ToString());

            decimal Equivalent_Madin_Value = decimal.Parse(dt_Master_Balance.Rows[0]["Equivalent_Madin_Value"].ToString());
            decimal Equivalent_Da2n_Value = decimal.Parse(dt_Master_Balance.Rows[0]["Equivalent_Da2n_Value"].ToString());

            decimal First_Balance_Madin_Old = decimal.Parse(dt_Master_Balance.Rows[0]["First_Balance_Madin"].ToString());
            decimal First_Balance_Da2n_Old = decimal.Parse(dt_Master_Balance.Rows[0]["First_Balance_Da2n"].ToString());
            decimal Equivalent_First_Balance_Madin_Old = decimal.Parse(dt_Master_Balance.Rows[0]["Equivalent_First_Balance_Madin"].ToString());
            decimal Equivalent_First_Balance_Da2n_Old = decimal.Parse(dt_Master_Balance.Rows[0]["Equivalent_First_Balance_Da2n"].ToString());


            long Account_Entry_Master_Balance_Id = long.Parse(dt_Master_Balance.Rows[0]["Id"].ToString());

            decimal Equivalent_First_Balance = EquivalentValue * New_First_Balance;
            decimal Equivalent_First_Balance_Madin = 0;
            decimal Equivalent_First_Balance_Da2n = 0;
            decimal Equivalent_First_Balance__ = 0;


            if (Transform_Type == 1) /// madin
            {
                First_Balance_Madin = New_First_Balance;
                First_Balance_Da2n = 0;
                First_Balance__New = New_First_Balance;


                Equivalent_First_Balance_Madin = Equivalent_First_Balance;
                Equivalent_First_Balance_Da2n = 0;
                Equivalent_First_Balance__ = Equivalent_First_Balance;


            }
            else
            {
                /////////////////da2n
                First_Balance_Madin = 0;
                First_Balance_Da2n = New_First_Balance;
                First_Balance__New = New_First_Balance * -1;

                Equivalent_First_Balance_Madin = 0;
                Equivalent_First_Balance_Da2n = Equivalent_First_Balance;
                Equivalent_First_Balance__ = Equivalent_First_Balance * -1;
            }


            ////////////////حساب الرصيد الجديد بعد تعديل رصيد اول المده
            decimal New_Final_Balance = (First_Balance_Madin - First_Balance_Da2n) + (Madin_Value - Da2n_Value);
            decimal New_Equivalent_Final_Balance = (Equivalent_First_Balance_Madin - Equivalent_First_Balance_Da2n) + (Equivalent_Madin_Value - Equivalent_Da2n_Value);

            decimal Balance_Madin_New = 0;
            decimal Balance_Da2n_New = 0;
            decimal Equevalent_Balance_Madin_New = 0;
            decimal Equevalent_Balance_Da2n_New = 0;
            int IS_Madin_Da2n = 0;
            if (New_Final_Balance > 0)
            {
                IS_Madin_Da2n = 1;
                Balance_Madin_New = New_Final_Balance;
                Balance_Da2n_New = 0;

                Equevalent_Balance_Madin_New = New_Equivalent_Final_Balance;
                Equevalent_Balance_Da2n_New = 0;
            }
            else
            {
                IS_Madin_Da2n = 2;
                Balance_Madin_New = 0;
                Balance_Da2n_New = New_Final_Balance * -1;

                Equevalent_Balance_Madin_New = 0;
                Equevalent_Balance_Da2n_New = New_Equivalent_Final_Balance * -1;

            }
            string mysql = "";
            mysql = " UPDATE  Account_Entry_Master_Balance SET  ";
            mysql = mysql + " First_Balance =" + First_Balance__New;
            mysql = mysql + ",First_Balance_Madin =" + First_Balance_Madin;
            mysql = mysql + ", First_Balance_Da2n =" + First_Balance_Da2n;
            mysql = mysql + " , Balance =" + New_Final_Balance;
            mysql = mysql + " , Balance_Madin =" + Balance_Madin_New;
            mysql = mysql + ", Balance_Da2n =" + Balance_Da2n_New;

            mysql = mysql + ", Equivalent_First_Balance_Madin=" + Equivalent_First_Balance_Madin;

            mysql = mysql + " , Equivalent_First_Balance_Da2n=" + Equivalent_First_Balance_Da2n;
            //mysql = mysql + " , Equivalent_Madin_Value="+Equivalent_Madin_Value_New;
            //mysql = mysql + " , Equivalent_Da2n_Value="+Equivalent_Da2n_Value_New;
            mysql = mysql + " ,  Equivalent_Balance_Madin=" + Equevalent_Balance_Madin_New;
            mysql = mysql + " , Equivalent_Balance_Da2n =" + Equevalent_Balance_Da2n_New;
            mysql = mysql + " ,IS_Madin_Da2n=" + IS_Madin_Da2n;
            mysql = mysql + " WHERE   (Id = " + Account_Entry_Master_Balance_Id + ")";
            opp.insert_me(mysql);


            string Entry_Date = operations.Get_DateToInsert(DateTime.Now.Date);


            mysql = @" SELECT   Account_Entry_List.Entry_Total, Account_Entry_List.Entry_No, Account_Entry_Details.Submain_Account_ID, Account_Entry_List.IS_FirstBalance, 
                         Account_Entry_List.Year_Financial_Period_Id, Account_Entry_List.Branch_Id_Entry, Account_Entry_List.Currency_ID, Account_Entry_Details.ID, 
                         Account_Entry_Details.Account_Entry_List_ID

FROM            Account_Entry_List INNER JOIN
                         Account_Entry_Details ON Account_Entry_List.ID = Account_Entry_Details.Account_Entry_List_ID ";
            mysql = mysql + "    WHERE        (Account_Entry_List.Entry_No IS NULL)  AND (Account_Entry_List.IS_FirstBalance = 1) ";
            mysql = mysql + " AND (Account_Entry_Details.Submain_Account_ID = " + Submain_Account_ID + ") AND (Account_Entry_List.Year_Financial_Period_Id = " + Year_Financial_Period_Id + ") AND (Account_Entry_List.Branch_Id_Entry = " + Branch_Id + ") AND (Account_Entry_List.Currency_ID = " + Account_Currency_Id + ") ";

            DataTable dt = opp.return_DataTable(mysql);
            if (dt.Rows.Count > 0)
            {
                long Account_Entry_Details_Id = long.Parse(dt.Rows[0]["Id"].ToString());
                long Account_Entry_List_ID = long.Parse(dt.Rows[0]["Account_Entry_List_ID"].ToString());

                mysql = "Update Account_Entry_List Set Entry_Total=" + New_First_Balance + " Where Id=" + Account_Entry_List_ID;
                opp.insert_me(mysql);



                mysql = " UPDATE       Account_Entry_Details SET     Balance_Import_Value =" + First_Balance_Madin + ", Balance_Export_Value =" + First_Balance_Da2n + " , Equivalent_Balance_Import_Value =" + Equivalent_First_Balance_Madin + ", Equivalent_Balance_Export_Value =" + Equivalent_First_Balance_Da2n + " ";
                mysql = mysql + "Where Id=" + Account_Entry_Details_Id;
                opp.insert_me(mysql);

            }
            else
            {
                //   Add_FirstBalanceTransaction(ref   opp, Submain_Account_ID, Branch_Id, Year_Financial_Period_Id, Account_Currency_Id, Transform_Type, EquivalentValue, First_Balance, First_Balance_Madin, First_Balance_Da2n, Equivalent_First_Balance, Equivalent_First_Balance_Madin, Equivalent_First_Balance_Da2n);

            }

            //////////////////////////////////////
            ///تعديل ارصدة الحركات للحساب
            ///تعديل رصيد الوارد اذا كان اكبر من الصفر ورصيد الصادر يساوى صفر والعكس
            ///New_First_Balance, decimal EquivalentValue
            //decimal First_Balance_Madin_Old = decimal.Parse(dt_Master_Balance.Rows[0]["First_Balance_Madin"].ToString());
            //decimal First_Balance_Da2n_Old = decimal.Parse(dt_Master_Balance.Rows[0]["First_Balance_Da2n"].ToString());
            //decimal Equivalent_First_Balance_Madin_Old = decimal.Parse(dt_Master_Balance.Rows[0]["Equivalent_First_Balance_Madin"].ToString());
            //decimal Equivalent_First_Balance_Da2n_Old = decimal.Parse(dt_Master_Balance.Rows[0]["Equivalent_First_Balance_Da2n"].ToString());
            decimal Diff_New_First_Balance = 0;
            decimal Diff_Equivalent_First_Balance = 0;
            if (First_Balance_Madin_Old > 0) /// madin
            {
                Diff_New_First_Balance = New_First_Balance - First_Balance_Madin_Old;
                Diff_Equivalent_First_Balance = (New_First_Balance * EquivalentValue) - Equivalent_First_Balance_Madin_Old;
            }
            else
            {
                Diff_New_First_Balance = New_First_Balance - First_Balance_Da2n_Old;
                Diff_Equivalent_First_Balance = (New_First_Balance * EquivalentValue) - Equivalent_First_Balance_Da2n_Old;
            }
            Update_Account_Transaction_Balance(ref opp, Submain_Account_ID, Diff_New_First_Balance, Diff_Equivalent_First_Balance, Transform_Type, Year_Financial_Period_Id);

        }
        void Make_Update_FirstBalance_New(ref operations opp, DataTable dt_Master_Balance, long Submain_Account_ID, long  Year_Financial_Period_Id, long  Branch_Id, long Account_Currency_Id, int Transform_Type, decimal New_First_Balance, decimal EquivalentValue)
        {
            string mysql = "";
            ///////////////تاريخ اول المده
            mysql = "Select * from Financial_Period Where Id=" + Year_Financial_Period_Id;
            DataTable dtF = opp.return_DataTable(mysql);
            string Entry_Date = operations.Get_DateToInsert(DateTime.Now.Date);
            string DateFrom_ = operations.Get_DateToInsert(DateTime.Now.Date);
            string DateTo_ = operations.Get_DateToInsert(DateTime.Now.Date);
            DateTime DateFrom_Financial = new DateTime();
            DateTime DateTo_Financial = new DateTime();
            if (dtF.Rows.Count > 0)
            {
                Entry_Date = operations.Get_DateToInsert(DateTime.Parse(dtF.Rows[0]["DateFrom"].ToString()));
                DateFrom_ = operations.Get_DateToInsert(DateTime.Parse(dtF.Rows[0]["DateFrom"].ToString()));
                DateTo_ = operations.Get_DateToInsert(DateTime.Parse(dtF.Rows[0]["DateTo"].ToString()));

                DateFrom_Financial = DateTime.Parse(dtF.Rows[0]["DateFrom"].ToString());
                DateTo_Financial = DateTime.Parse(dtF.Rows[0]["DateTo"].ToString());
            }

            decimal First_Balance__New = 0;
            decimal First_Balance = 0;
            decimal First_Balance_Madin = 0;
            decimal First_Balance_Da2n = 0;


            long Account_Entry_Master_Balance_Id = long.Parse(dt_Master_Balance.Rows[0]["Id"].ToString());

            decimal Equivalent_First_Balance = EquivalentValue * New_First_Balance;
            decimal Equivalent_First_Balance_Madin = 0;
            decimal Equivalent_First_Balance_Da2n = 0;
            decimal Equivalent_First_Balance__ = 0;


            if (Transform_Type == 1) /// madin
            {
                First_Balance_Madin = New_First_Balance;
                First_Balance_Da2n = 0;
                First_Balance__New = New_First_Balance;


                Equivalent_First_Balance_Madin = Equivalent_First_Balance;
                Equivalent_First_Balance_Da2n = 0;
                Equivalent_First_Balance__ = Equivalent_First_Balance;


            }
            else
            {
                /////////////////da2n
                First_Balance_Madin = 0;
                First_Balance_Da2n = New_First_Balance;
                First_Balance__New = New_First_Balance * -1;

                Equivalent_First_Balance_Madin = 0;
                Equivalent_First_Balance_Da2n = Equivalent_First_Balance;
                Equivalent_First_Balance__ = Equivalent_First_Balance * -1;
            }




            int IS_Madin_Da2n = Transform_Type;


            mysql = " UPDATE  Account_Entry_Master_Balance SET  ";
            mysql = mysql + " First_Balance =" + First_Balance__New;
            mysql = mysql + ",First_Balance_Madin =" + First_Balance_Madin;
            mysql = mysql + ", First_Balance_Da2n =" + First_Balance_Da2n;
            mysql = mysql + " , Balance =0";
            mysql = mysql + " , Balance_Madin =0";
            mysql = mysql + ", Balance_Da2n =0";

            mysql = mysql + ", Madin_Value =0";
            mysql = mysql + ", Da2n_Value =0";
            mysql = mysql + ", Equivalent_Madin_Value =0";
            mysql = mysql + ", Equivalent_Da2n_Value =0";





            mysql = mysql + ", Equivalent_First_Balance_Madin=" + Equivalent_First_Balance_Madin;

            mysql = mysql + " , Equivalent_First_Balance_Da2n=" + Equivalent_First_Balance_Da2n;
            mysql = mysql + " , Equivalent_First_Balance=" + Equivalent_First_Balance;

            mysql = mysql + " ,  Equivalent_Balance_Madin=0";
            mysql = mysql + " , Equivalent_Balance_Da2n =0";
            mysql = mysql + " ,IS_Madin_Da2n=" + IS_Madin_Da2n;
            mysql = mysql + " ,Factor=" + EquivalentValue;

            mysql = mysql + " WHERE   (Id = " + Account_Entry_Master_Balance_Id + ")";
            opp.insert_me(mysql);


            Make_CancelTar7eelWithUpdateFirstBalance(ref opp, Submain_Account_ID, Branch_Id, DateFrom_Financial, DateTo_Financial);

        }
        void Make_Update_FirstBalance_NewForCheck(ref operations opp, DataTable dt_Master_Balance, long Submain_Account_ID, long  Year_Financial_Period_Id, long  Branch_Id, long Account_Currency_Id, int Transform_Type, decimal New_First_Balance, decimal EquivalentValue)
        {
            string mysql = "";
            ///////////////تاريخ اول المده
            mysql = "Select * from Financial_Period Where Id=" + Year_Financial_Period_Id;
            DataTable dtF = opp.return_DataTable(mysql);
            string Entry_Date = operations.Get_DateToInsert(DateTime.Now.Date);
            string DateFrom_ = operations.Get_DateToInsert(DateTime.Now.Date);
            string DateTo_ = operations.Get_DateToInsert(DateTime.Now.Date);
            DateTime DateFrom_Financial = new DateTime();
            DateTime DateTo_Financial = new DateTime();
            if (dtF.Rows.Count > 0)
            {
                Entry_Date = operations.Get_DateToInsert(DateTime.Parse(dtF.Rows[0]["DateFrom"].ToString()));
                DateFrom_ = operations.Get_DateToInsert(DateTime.Parse(dtF.Rows[0]["DateFrom"].ToString()));
                DateTo_ = operations.Get_DateToInsert(DateTime.Parse(dtF.Rows[0]["DateTo"].ToString()));

                DateFrom_Financial = DateTime.Parse(dtF.Rows[0]["DateFrom"].ToString());
                DateTo_Financial = DateTime.Parse(dtF.Rows[0]["DateTo"].ToString());
            }

            decimal First_Balance__New = 0;
            decimal First_Balance = 0;
            decimal First_Balance_Madin = 0;
            decimal First_Balance_Da2n = 0;


            long Account_Entry_Master_Balance_Id = long.Parse(dt_Master_Balance.Rows[0]["Id"].ToString());

            decimal Equivalent_First_Balance = EquivalentValue * New_First_Balance;
            decimal Equivalent_First_Balance_Madin = 0;
            decimal Equivalent_First_Balance_Da2n = 0;
            decimal Equivalent_First_Balance__ = 0;


            if (Transform_Type == 1) /// madin
            {
                First_Balance_Madin = New_First_Balance;
                First_Balance_Da2n = 0;
                First_Balance__New = New_First_Balance;


                Equivalent_First_Balance_Madin = Equivalent_First_Balance;
                Equivalent_First_Balance_Da2n = 0;
                Equivalent_First_Balance__ = Equivalent_First_Balance;


            }
            else
            {
                /////////////////da2n
                First_Balance_Madin = 0;
                First_Balance_Da2n = New_First_Balance;
                First_Balance__New = New_First_Balance * -1;

                Equivalent_First_Balance_Madin = 0;
                Equivalent_First_Balance_Da2n = Equivalent_First_Balance;
                Equivalent_First_Balance__ = Equivalent_First_Balance * -1;
            }




            int IS_Madin_Da2n = Transform_Type;


            mysql = " UPDATE  Account_Entry_Master_Balance SET  ";
            mysql = mysql + " First_Balance =First_Balance + " + First_Balance__New;
            mysql = mysql + ",First_Balance_Madin =First_Balance_Madin + " + First_Balance_Madin;
            mysql = mysql + ", First_Balance_Da2n =First_Balance_Da2n + " + First_Balance_Da2n;
            mysql = mysql + " , Balance =0";
            mysql = mysql + " , Balance_Madin =0";
            mysql = mysql + ", Balance_Da2n =0";

            mysql = mysql + ", Madin_Value =0";
            mysql = mysql + ", Da2n_Value =0";
            mysql = mysql + ", Equivalent_Madin_Value =0";
            mysql = mysql + ", Equivalent_Da2n_Value =0";





            mysql = mysql + ", Equivalent_First_Balance_Madin=Equivalent_First_Balance_Madin + " + Equivalent_First_Balance_Madin;

            mysql = mysql + " , Equivalent_First_Balance_Da2n= Equivalent_First_Balance_Da2n + " + Equivalent_First_Balance_Da2n;
            mysql = mysql + " , Equivalent_First_Balance=" + Equivalent_First_Balance;

            mysql = mysql + " ,  Equivalent_Balance_Madin=0";
            mysql = mysql + " , Equivalent_Balance_Da2n =0";
            mysql = mysql + " ,IS_Madin_Da2n=" + IS_Madin_Da2n;
            mysql = mysql + " ,Factor=" + EquivalentValue;

            mysql = mysql + " WHERE   (Id = " + Account_Entry_Master_Balance_Id + ")";
            opp.insert_me(mysql);


            Make_CancelTar7eelWithUpdateFirstBalance(ref opp, Submain_Account_ID, Branch_Id, DateFrom_Financial, DateTo_Financial);

        }

        void Make_CancelTar7eelWithUpdateFirstBalance(ref operations opp, long Submain_Account_ID, long  Branch_Id, DateTime DateFrom_Financial, DateTime DateTo_Financial)
        {

            string mysql = "";
            //////////////////////////////////////

            ////إلغاء الترصيد لحساب العميل لنفس السنه الماليه
            mysql = "  UPDATE       Account_Entry_Details SET    ";
            mysql = mysql + "   Balance_Import_Value = 0, Balance_Export_Value = 0, Equivalent_Balance_Import_Value = 0, Equivalent_Balance_Export_Value = 0 ";
            mysql = mysql + " FROM   Account_Entry_Details INNER JOIN Account_Entry_List ON Account_Entry_Details.Account_Entry_List_ID = Account_Entry_List.ID ";
            mysql = mysql + " WHERE   (Account_Entry_Details.Submain_Account_ID = " + Submain_Account_ID + ") AND (Account_Entry_List.Branch_Id_Entry = " + Branch_Id + ")  ";
            mysql = mysql + " AND (Account_Entry_List.Entry_No IS NOT NULL) ";
            mysql = mysql + " AND (Account_Entry_List.Entry_Date >= " + operations.Get_DateToInsert(DateFrom_Financial) + ") AND (Account_Entry_List.Entry_Date <= " + operations.Get_DateToInsert(DateTo_Financial) + ") ";
            opp.insert_me(mysql);

            ////إلغاء الترصيد لحساب العميل لنفس السنه الماليه
            mysql = "  UPDATE       Account_Entry_List SET   IS_Transfer_To_Gl_Flage=0,IS_Transfer_To_Gl_Flage_Str='غير مرحل' ";
            mysql = mysql + " FROM   Account_Entry_Details INNER JOIN Account_Entry_List ON Account_Entry_Details.Account_Entry_List_ID = Account_Entry_List.ID ";
            mysql = mysql + " WHERE   (Account_Entry_Details.Submain_Account_ID = " + Submain_Account_ID + ") AND (Account_Entry_List.Branch_Id_Entry = " + Branch_Id + ")  ";
            mysql = mysql + " AND (Account_Entry_List.Entry_No IS NOT NULL) ";
            mysql = mysql + " AND (Account_Entry_List.Entry_Date >= " + operations.Get_DateToInsert(DateFrom_Financial) + ") AND (Account_Entry_List.Entry_Date <= " + operations.Get_DateToInsert(DateTo_Financial) + ") ";
            opp.insert_me(mysql);


            mysql = " UPDATE  Account_Entry_Master_Balance SET  ";

            mysql = mysql + "  Balance =0";
            mysql = mysql + " , Balance_Madin =0";
            mysql = mysql + ", Balance_Da2n =0";
            mysql = mysql + ", Madin_Value =0";
            mysql = mysql + ", Da2n_Value =0";
            mysql = mysql + ", Equivalent_Madin_Value =0";
            mysql = mysql + ", Equivalent_Da2n_Value =0";
            mysql = mysql + " ,  Equivalent_Balance_Madin=0";
            mysql = mysql + " , Equivalent_Balance_Da2n =0";
            opp.insert_me(mysql);

        }


         void Update_Account_Transaction_Balance(ref operations opp, long  Submain_Account_ID, decimal First_Balance, decimal Equivalent_First_Balance, int Transform_Type, long  Year_Financial_Period_Id)
        {
            string mysql = "";
            ///تعديل ارصدة الحركات للحساب
            ///تعديل رصيد الوارد اذا كان اكبر من الصفر ورصيد الصادر يساوى صفر والعكس
            decimal Update_Balance_Value = 0;
            decimal Update_Equivalent_Balance_Value = 0;
            if (Transform_Type == 1) /// madin
            {
                Update_Balance_Value = First_Balance;
                Update_Equivalent_Balance_Value = Equivalent_First_Balance;
            }
            else
            {
                Update_Balance_Value = First_Balance * -1;
                Update_Equivalent_Balance_Value = Equivalent_First_Balance * -1;
            }

            ///        Update
            ///Balance_Import_Value
            ///Balance_Export_Value
            ///Equivalent_Balance_Import_Value
            ///Equivalent_Balance_Export_Value


            mysql = " UPDATE  Account_Entry_Details SET    Balance_Import_Value =Balance_Import_Value+" + Update_Balance_Value + " ";
            mysql = mysql + " FROM Account_Entry_Details INNER JOIN Account_Entry_List ON Account_Entry_Details.Account_Entry_List_ID = Account_Entry_List.ID ";
            mysql = mysql + "  WHERE      (Submain_Account_ID = " + Submain_Account_ID + ") AND (Balance_Import_Value>0) AND (Balance_Export_Value= 0)";
            mysql = mysql + " AND (Account_Entry_List.IS_Transfer_To_Gl_Flage = 1) AND (Account_Entry_List.Year_Financial_Period_Id = " + Year_Financial_Period_Id + ") ";
            opp.insert_me(mysql);


            mysql = " UPDATE  Account_Entry_Details SET    Balance_Export_Value =Balance_Export_Value+" + Update_Balance_Value + " ";
            mysql = mysql + " FROM Account_Entry_Details INNER JOIN Account_Entry_List ON Account_Entry_Details.Account_Entry_List_ID = Account_Entry_List.ID ";
            mysql = mysql + "  WHERE   (Submain_Account_ID = " + Submain_Account_ID + ") AND (Balance_Import_Value=0) AND (Balance_Export_Value>0)";
            mysql = mysql + " AND (Account_Entry_List.IS_Transfer_To_Gl_Flage = 1) AND (Account_Entry_List.Year_Financial_Period_Id = " + Year_Financial_Period_Id + ") ";
            opp.insert_me(mysql);


            /////////////////////////////////


            mysql = " UPDATE  Account_Entry_Details SET    Equivalent_Balance_Import_Value =Equivalent_Balance_Import_Value+" + Update_Equivalent_Balance_Value + " ";
            mysql = mysql + " FROM Account_Entry_Details INNER JOIN Account_Entry_List ON Account_Entry_Details.Account_Entry_List_ID = Account_Entry_List.ID ";
            mysql = mysql + "  WHERE   (Submain_Account_ID = " + Submain_Account_ID + ") AND (Equivalent_Balance_Import_Value>0) AND (Equivalent_Balance_Export_Value= 0)";
            mysql = mysql + " AND (Account_Entry_List.IS_Transfer_To_Gl_Flage = 1) AND (Account_Entry_List.Year_Financial_Period_Id = " + Year_Financial_Period_Id + ") ";
            opp.insert_me(mysql);


            mysql = " UPDATE  Account_Entry_Details SET    Equivalent_Balance_Export_Value =Equivalent_Balance_Export_Value+" + Update_Equivalent_Balance_Value + " ";
            mysql = mysql + " FROM Account_Entry_Details INNER JOIN Account_Entry_List ON Account_Entry_Details.Account_Entry_List_ID = Account_Entry_List.ID ";
            mysql = mysql + "  WHERE   (Submain_Account_ID = " + Submain_Account_ID + ") AND (Equivalent_Balance_Import_Value=0) AND (Equivalent_Balance_Export_Value>0)";
            mysql = mysql + " AND (Account_Entry_List.IS_Transfer_To_Gl_Flage = 1) AND (Account_Entry_List.Year_Financial_Period_Id = " + Year_Financial_Period_Id + ") ";
            opp.insert_me(mysql);

        }

        public DataTable Get_Account_First_Balance(long Year_Financial_Period_Id, long Branch_Id)
        {
            //Submain_Account.Last_destruction,
            //Submain_Account.Import_Value,
            //Submain_Account.Export_Value, 
            //Submain_Account.ConnectWith_Gl_Flage, 
            string mysql = @"  SELECT DISTINCT 
                         Main_Account.SubAccount_Name + '  /  ' + Submain_Account.SubAccount_Name AS Name, 
                         Submain_Account.SubAccount_NO + '  ' + Submain_Account.SubAccount_Name AS Account_Name_No, Main_Account.SubAccount_Name AS MainAccount_Name, 
                         Main_Account.SubAccount_NO AS MainAccount_No, Main_Account.Submain_Account_Balance AS main_Account_Balance, 
                         Main_Account.Currency_Id AS main_Currency_Id, Main_Account.Notes AS main_Notes, Submain_Account.Id, Submain_Account.Main_Account_Id, 
                         Submain_Account.SubAccount_NO, Submain_Account.Code, Submain_Account.FirstSubCode, Submain_Account.lastPartIncrease, 
                         Submain_Account.SubAccount_Name, Submain_Account.IS_Last_Level, Submain_Account.Head_Main_Account, Submain_Account.Submain_Account_Balance, 
                         Submain_Account.Currency_Id, Submain_Account.Notes, Submain_Account.CanDelete, Submain_Account.Account_Follows, Submain_Account.destruction_date, 
                         Submain_Account.destruction,   
                         Submain_Account.Account_Nature_Type, Submain_Account.Accept_Cost_Center_Flage, Submain_Account.Cost_Center_Tree_ID, Submain_Account.Account_Level, 
                         Submain_Account.Account_Code, Submain_Account.Company_ID_For_GL, Submain_Account.Company_Name_For_GL, 
                         Submain_Account.Account_Customer_Type_Value, Submain_Account.Account_Customer_Type_ID, Submain_Account.Transform_Type, 
                           Submain_Account.Branch_ID, 
                         Account_Branches.Branch_Name, Currency.Currency_Name, Currency.Currency_Name_EN, Currency_1.Currency_Name AS Equivalent_Currency_Name, 
                         Currency_1.Currency_Name_EN AS Equivalent_Currency_Name_EN, Account_Entry_Master_Balance.Year_Financial_Period_Id, 
                         Account_Entry_Master_Balance.First_Balance_Madin, Account_Entry_Master_Balance.First_Balance_Da2n, 
                         Account_Entry_Master_Balance.Equivalent_First_Balance_Madin + Account_Entry_Master_Balance.Equivalent_First_Balance_Da2n AS Equivalent_First_Balance_Value,
                          Cost_Center_Tree.Name AS Cost_Center_Name
FROM            Submain_Account INNER JOIN
                         Account_Entry_Master_Balance ON Submain_Account.Id = Account_Entry_Master_Balance.Submain_Account_ID INNER JOIN
                         Account_Branches ON Account_Entry_Master_Balance.Branch_Id = Account_Branches.Id INNER JOIN
                         Currency ON Account_Entry_Master_Balance.Account_Currency_Id = Currency.Value INNER JOIN
                         Currency AS Currency_1 ON Account_Entry_Master_Balance.Equivalent_Account_Currency_Id = Currency_1.ID LEFT OUTER JOIN
                         Cost_Center_Tree ON Account_Entry_Master_Balance.Cost_Center_Tree_ID = Cost_Center_Tree.ID LEFT OUTER JOIN
                         Submain_Account AS Main_Account ON Submain_Account.Main_Account_Id = Main_Account.Id
WHERE         (Submain_Account.Id NOT IN
                             (SELECT        MainSubmain_Account.Id AS MAinAccount_Id
                                FROM            Submain_Account AS Submain_Account_1 INNER JOIN
                                                         Submain_Account AS MainSubmain_Account ON Submain_Account_1.Main_Account_Id = MainSubmain_Account.Id))";

            mysql = mysql + "  AND (Account_Entry_Master_Balance.Year_Financial_Period_Id = " + Year_Financial_Period_Id + ") ";
            if (Branch_Id > 0)
            {
                mysql = mysql + "  AND (Account_Entry_Master_Balance.Branch_Id = " + Branch_Id + ") ";
            }

            operations opp = new operations();
            DataTable dt = opp.return_DataTable(mysql);
            return dt;
        }
        public DataTable Get_Account_First_Balance(long  Year_Financial_Period_Id, long  Branch_Id, long Submain_Account_ID)
        {
            //Submain_Account.Last_destruction,
            //Submain_Account.Import_Value,
            //Submain_Account.Export_Value, 
            //Submain_Account.ConnectWith_Gl_Flage, 
            string mysql = @" SELECT DISTINCT 
                         Submain_Account.Id, Submain_Account.SubAccount_NO, Submain_Account.SubAccount_Name, Account_Entry_Master_Balance.Year_Financial_Period_Id, Account_Entry_Master_Balance.First_Balance_Madin, 
                         Account_Entry_Master_Balance.First_Balance_Da2n, Currency.Currency_Name, Currency.Currency_Name_EN, Account_Entry_Master_Balance.Factor, Account_Entry_Master_Balance.Equivalent_First_Balance_Madin, 
                         Account_Entry_Master_Balance.Equivalent_First_Balance_Da2n, Account_Entry_Master_Balance.Equivalent_Account_Currency_Id, Account_Entry_Master_Balance.Equivalent_First_Balance
FROM            Submain_Account INNER JOIN
                         Account_Entry_Master_Balance ON Submain_Account.Id = Account_Entry_Master_Balance.Submain_Account_ID INNER JOIN
                         Currency AS Currency ON Account_Entry_Master_Balance.Equivalent_Account_Currency_Id = Currency.[Value]
";

            mysql = mysql + "  WHERE    (Account_Entry_Master_Balance.Year_Financial_Period_Id = " + Year_Financial_Period_Id + ") ";
            if (Branch_Id > 0)
            {
                mysql = mysql + "  AND (Account_Entry_Master_Balance.Branch_Id = " + Branch_Id + ") ";
            }
            if (Submain_Account_ID > 0)
            {
                mysql = mysql + "  AND (Account_Entry_Master_Balance.Submain_Account_ID = " + Submain_Account_ID + ") ";
            }

            mysql = mysql + " ORDER BY Account_Entry_Master_Balance.Equivalent_Account_Currency_Id ";
            operations opp = new operations();
            DataTable dt = opp.return_DataTable(mysql);
            return dt;
        }
        public void SetCustomerFirstBalance(ref int Excp, long Customer_Id, decimal Balance, int Transform_Type, DateTime datBalance, string Notes)
        {
            operations opp = new  operations();
            SetCustomerFirstBalance(ref Excp, ref opp, Customer_Id, Balance, Transform_Type, datBalance, Notes);

        }
        public void SetCustomerFirstBalanceNew(ref int Excp, long Account_Customer_Type_ID, int Account_Customer_Type_Value, long  Year_Financial_Period_Id, decimal Balance, int Transform_Type, DateTime datBalance, string Notes)
        {

            //long Account_Customer_Type_ID = Customer_Id;
            //int Account_Customer_Type_Value = Common.Customer_Account_Type;

            Submain_AccountCLS sb = new Submain_AccountCLS();
            sb.Get_Submain_Account(Account_Customer_Type_ID, Account_Customer_Type_Value);

            Excp = 0;
            string Msg = "";
            long Submain_Account_ID = sb.sb.Id;//
            string SubAccount_Name = sb.sb.SubAccount_Name;
            int Currency_Id = Common.Entry_Currency_Id;// int.Parse(currency_List_21.cmbCurrencyName.SelectedValue.ToString());

            long  Branch_Id = Common.Branch_Id;// int.Parse(branch_List1.cmbBranch.SelectedValue.ToString());
            decimal Factor = 1;// txtEquivalentValue.Value;
            decimal First_Balance = Balance;
            decimal First_Balance_Equivalent = Balance;

            FirstBalance_CLS ff = new FirstBalance_CLS();
            ff.Account_Customer_Type_ID = Account_Customer_Type_ID;
            ff.Account_Customer_Type_Value = Account_Customer_Type_Value;


            ff.Save_FirstBalance(ref Excp, ref Msg, Submain_Account_ID, SubAccount_Name, Year_Financial_Period_Id, Branch_Id, Currency_Id, First_Balance, Transform_Type, Factor);


        }
        public void SetCustomerFirstBalance(ref int Excp, ref operations opp, long Customer_Id, decimal Balance, int Transform_Type, DateTime datBalance, string Notes)
        {
            bool IsFirst = IsDateFirstPeriod(ref opp, datBalance);

            Customers_AccountsCLS customers_AccountsCLS = new Customers_AccountsCLS();
            customers_AccountsCLS.SetCustomerFirstBalance(ref Excp, ref opp, IsFirst, Customer_Id, Balance, Transform_Type, datBalance, Notes);
            
        }

        public static void SetFirstBalanceForAllSuppliers()
        {
            try
            {

                operations opp = new operations();

                string mysql = @"SELECT Id FROM Supplier WHERE (Id NOT IN ";
                mysql = mysql + "(SELECT Supplier_Id FROM Suppliers_Accounts))";

                DataTable dt = opp.return_DataTable(mysql);
               

                if (dt.Rows.Count > 0)
                {
                    mysql = "";
                    DateTime datBalance = DateTime.Now.Date;
                    long  Year_Financial_Period_Id = Financial_PeriodCLS.Get_Year_Financial_Period_Id(datBalance);
                    for (int x = 0; x < dt.Rows.Count; x++)
                    {
                        long Supplier_Id = long.Parse(dt.Rows[x]["Id"].ToString());

                        mysql += " INSERT INTO Suppliers_Accounts (Supplier_Id, Reference_Id,Reference_Type_Table_Id, Export,Import, Balance, Transaction_Type, Transaction_Date,Reference_Type,Notes) ";
                        mysql += " VALUES(" + Supplier_Id + ",0," + Supplier_Id + ",0,0,0,1,'" + operations.dateFormat(datBalance) + "',-7,N'تم اضافة رصيد اول المدة بقيمة صفر اليا')";

                    }

                    opp.insert_me(mysql);
                }
               
            }
            catch (Exception ee)
            {

                
            }
        }
        public static void SetFirstBalanceForAllCustomers()
        {
            try
            {

                operations opp = new operations();

                string mysql = @"SELECT Id FROM Customer WHERE (Id NOT IN ";
                mysql = mysql + "(SELECT Customer_Id FROM Customers_Accounts))";

                DataTable dt = opp.return_DataTable(mysql);


                if (dt.Rows.Count > 0)
                {
                    mysql = "";
                    DateTime datBalance = DateTime.Now.Date;
                    long  Year_Financial_Period_Id = Financial_PeriodCLS.Get_Year_Financial_Period_Id(datBalance);
                    for (int x = 0; x < dt.Rows.Count; x++)
                    {
                        long Customer_Id = long.Parse(dt.Rows[x]["Id"].ToString());

                        mysql += " INSERT INTO Customers_Accounts (Customer_Id, Reference_Id,Reference_Type_Table_Id, Export,Import, Balance, Transaction_Type, Transaction_Date,Reference_Type,Notes) ";
                        mysql += " VALUES(" + Customer_Id + ",0," + Customer_Id + ",0,0,0,2,'" + operations.dateFormat(datBalance) + "',-7,N'تم اضافة رصيد اول المدة بقيمة صفر اليا')";

                    }

                    opp.insert_me(mysql);
                }

            }
            catch (Exception ee)
            {


            }
        }

        public void SetSupplierFirstBalance(ref int Excp, long Supplier_Id, decimal Balance, int Transform_Type, DateTime datBalance, string Notes)
        {
            operations opp = new operations();
            SetSupplierFirstBalance(ref Excp, ref opp, Supplier_Id, Balance, Transform_Type, datBalance, Notes);
        }
        public void SetSupplierFirstBalance(ref int Excp, ref operations opp, long Supplier_Id, decimal Balance, int Transform_Type, DateTime datBalance, string Notes)
        {
            bool IsFirst = IsDateFirstPeriod(ref opp, datBalance);
            Suppliers_AccountsCls suppliers_AccountsCls = new Suppliers_AccountsCls();
            suppliers_AccountsCls. SetSupplierFirstBalance(ref   Excp, ref   opp, IsFirst,  Supplier_Id,   Balance,   Transform_Type,   datBalance,   Notes);
            
        }
        void SetCustomer_SupplierAccountFirstBalance(ref operations opp, long SubAccount_Id, decimal Balance, int Transform_Type, long  Year_Financial_Period_Id, string Notes)
        {
            int Excp = 0;
             

            DateTime datBalance = GetDateFirstPeriod(ref opp, Year_Financial_Period_Id);

            if (Account_Customer_Type_Value == 2) ///حساب عميل
            {
                SetCustomerFirstBalance(ref Excp, ref opp, Account_Customer_Type_ID, Balance, Transform_Type, datBalance, Notes);

            }

            if (Account_Customer_Type_Value == 5) ///حساب مورد
            {
                SetSupplierFirstBalance(ref Excp, ref opp, Account_Customer_Type_ID, Balance, Transform_Type, datBalance, Notes);

            }
        }
        bool IsDateFirstPeriod(ref operations opp, DateTime dat)
        {
            bool IsFirst = false;
            string mysql = "Select * from Financial_Period   ORDER BY DateFrom ";
            DataTable dt = opp.return_DataTable(mysql);

            if (dt.Rows.Count > 0)
            {
                DateTime DateFrom = DateTime.Parse(dt.Rows[0]["DateFrom"].ToString());
                DateFrom = new DateTime(DateFrom.Year, DateFrom.Month, DateFrom.Day);

                dat = new DateTime(dat.Year, dat.Month, dat.Day);

                if (dat == DateFrom)
                {
                    IsFirst = true;
                }
            }


            return IsFirst;
        }
        DateTime GetDateFirstPeriod(ref operations opp, long  Year_Financial_Period_Id)
        {
            DateTime DateFrom = new DateTime();
            string mysql = "Select * from Financial_Period Where Id=" + Year_Financial_Period_Id + " ";
            DataTable dt = opp.return_DataTable(mysql);

            if (dt.Rows.Count > 0)
            {
                DateFrom = DateTime.Parse(dt.Rows[0]["DateFrom"].ToString());

            }


            return DateFrom;
        }
    }
}
