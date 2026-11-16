
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using Microsoft.Data.SqlClient;
using System.Data.OleDb;
  using AccountModelsLib; using AccountModelsLib.Models;

namespace AccountModelsLib
{
    public class Money_Financial_NoticeCLS
    {
        public Money_Financial_NoticeCLS()
        {
        }
        public static long Make_New_OrderNo(DateTime dat)
        {
            long OrderNo = 1;
            try
            {
                operations opp = new operations();
                int Reference_Type = Common.Taswea_Malia_Reference_Type;
                string mysql = "SELECT MAX(OrderNo) AS OrderNo FROM Money_Financial_Notice_Items ";
                mysql = mysql + " WHERE  Reference_Type=@Reference_Type ";
                //opp.cmd.Parameters.AddWithValue("@OrderDate", dat);
                opp.cmd.Parameters.AddWithValue("@Reference_Type", Reference_Type);

                DataTable dt = opp.return_DataTable(mysql);
                if (dt.Rows.Count > 0)
                {
                    if (dt.Rows[0]["OrderNo"].ToString() == "")
                    {
                        OrderNo = 1;
                    }
                    else
                    {
                        OrderNo = long.Parse(dt.Rows[0]["OrderNo"].ToString()) + 1;
                    }
                }
                else
                {
                    OrderNo = 1;
                }
                opp = null;

                return OrderNo;

            }
            catch (Exception e)
            {

                return OrderNo;

            }
        }

        public void Add_New_Financial_Notice(ref int Excp, long OrderNo, DateTime OrderDate, int Transaction_Type, string Transaction_TypeStr, decimal Order_Value, string Order_Value_Str, long Currency_Id, string Notes, long Customer_Id, long Supplier_Id, long Account_Id, int Account_Type, long SubAccount_Id, int Account_Selection_, decimal Balance_Before, long Compete_SubAccount_Id)
        {
            operations opp = new operations(1);
            Add_New_Financial_Notice(ref Excp, ref opp, OrderNo, OrderDate, Transaction_Type, Transaction_TypeStr, Order_Value, Order_Value_Str, Currency_Id, Notes, Customer_Id, Supplier_Id, Account_Id, Account_Type, SubAccount_Id, Account_Selection_, Balance_Before, Compete_SubAccount_Id);
            if (Excp == 0)
            {
                opp.Commit();
            }
            else
            {
                opp.Rollback();
            }
        }

        void Add_New_Financial_Notice(ref int Excp, ref operations opp, long OrderNo, DateTime OrderDate, int Transaction_Type, string Transaction_TypeStr, decimal Order_Value, string Order_Value_Str, long Currency_Id, string Notes, long Customer_Id, long Supplier_Id, long Account_Id, int Account_Type, long SubAccount_Id, int Account_Selection_, decimal Balance_Before, long Compete_SubAccount_Id)
        {


            //int Relation_Account_ID = 0;
            //long Order_Mony_Id = 0;
            int Supplier_Customer = Account_Selection_;

            long Money_Financial_Notice_Id = 0;
            string mysql = "";
            //decimal Balance_Before = Account_Selection.Supplier_Customer_Balance;
            decimal Balance_After = 0;
            long SubAccountRef = 0;
            if (Account_Type == Common.Customer_Account_Type)
            {
                SubAccountRef = Customer_Id;
                Balance_After = Balance_Before - Order_Value;
            }
            else if (Account_Type == Common.Supplier_Account_Type)
            {
                SubAccountRef = Supplier_Id;
                Balance_After = Balance_Before - Order_Value;
            }
            else
            {
                Balance_After = 0;
            }

            int Reference_Type = Common.Taswea_Malia_Reference_Type;

            try
            {

                mysql = "INSERT INTO Money_Financial_Notice_Items  (OrderNo, OrderDate, Transaction_Type,Transaction_TypeStr, Order_Value, Order_Value_Str, Account_Type, Account_Id, SubAccount_Id, Currency_Id, Notes,Compete_SubAccount_Id,Account_Selection,Customer_Id,Supplier_Id,Reference_Type) ";
                mysql = mysql + " VALUES(" + OrderNo + ",'" + operations.dateFormat(OrderDate) + "'," + Transaction_Type + ",'" + Transaction_TypeStr + "'," + Order_Value + ",N'" + Order_Value_Str + "'," + Account_Type + "," + Account_Id + "," + SubAccount_Id + "," + Currency_Id + ",'" + global::AccountModelsLib.SqlText.Q(Notes) + "'," + Compete_SubAccount_Id + "," + Account_Selection_ + "," + Customer_Id + "," + Supplier_Id + "," + Reference_Type + ")";

                // 🟢 Money_Financial_Notice_Items.Id ليست IDENTITY حقيقية (تحقَّق مباشرة عبر sys.columns)، لكن
                // Insert_me_Return_Last_Identify الأصلية تفترض @@IDENTITY طالما Common.Use_Auto_Identity_Column=1
                // (قيمتها الفعلية فى هذه القاعدة) - فكان الإدراج يفشل دائماً بصمت (NULL فى عمود Id غير القابل
                // للـNULL، يُبتلَع فى catch الخارجى). الإصلاح: InsertWithManualId (أسفل الملف) يستدعى نفس آلية
                // توليد/حقن Id الموجودة أصلاً فى MySqlParser صراحة، بصرف النظر عن قيمة العلم العالمى - بلا أى
                // تغيير فى بقية منطق العمل. نفس الإصلاح طُبِّق على إدراجَى Customers_Accounts/Suppliers_Accounts
                // أدناه (لهما نفس المشكلة بالضبط، لكن عمودهما القابل للـNULL يجعلها تفشل بصمت لا بصريح خطأ).
                Money_Financial_Notice_Id = InsertWithManualId(opp, mysql);


                //Add_AnotherStaticAccountOnSubmainTable(ref opp, Account_Id, Transaction_Type, Order_Value, SubAccountRef, Relation_Account_ID, OrderDate, AccS);




                decimal Balance = 0;
                //(Account_Selection == 0)             حسابات أخرى
                if (Account_Selection_ == 2)  //عملاء
                {
                    if (Transaction_Type == 1)  // أستلام نقدية
                    {
                        ///ليس رصيد أول المدة
                        if (Common.Balance_BeginingPeriod == 0)
                        {
                            mysql = "  UPDATE Customer SET Balance = Balance -  " + Order_Value + " , Balance_Actual = Balance_Actual - " + Order_Value + "  WHERE (Id = " + Customer_Id + ")";
                            // arr.Add(mysql);
                            opp.insert_me(mysql);
                        }
                        mysql = "Select * from Customer Where Id=" + Customer_Id;
                        DataTable dt = opp.return_DataTable(mysql);
                        if (dt.Rows.Count > 0)
                        {
                            Balance = decimal.Parse(dt.Rows[0]["Balance"].ToString());
                        }
                        else
                        { Balance = 0; }
                        mysql = " INSERT INTO Customers_Accounts (Notes,Customer_Id, Reference_Id, Reference_Type_Table_Id, Import , Balance, Reference_Type, Transaction_Type, Transaction_Date,Payment_Type) ";
                        mysql = mysql + " Values ('" + global::AccountModelsLib.SqlText.Q(Notes) + "'," + Customer_Id + "," + OrderNo + "," + Money_Financial_Notice_Id + "," + Order_Value + "," + Balance + "," + Reference_Type + "," + Transaction_Type + ",'" + operations.dateFormat(OrderDate) + "',0)";
                        InsertWithManualId(opp, mysql);
                    }
                    if (Transaction_Type == 2)  // صرف نقدية
                    { ///ليس رصيد أول المدة
                        if (Common.Balance_BeginingPeriod == 0)
                        {
                            mysql = "  UPDATE Customer SET Balance = Balance +  " + Order_Value + " , Balance_Actual = Balance_Actual + " + Order_Value + "  WHERE (Id = " + Customer_Id + ")";
                            // arr.Add(mysql);
                            opp.insert_me(mysql);
                        }
                        mysql = "Select * from Customer Where Id=" + Customer_Id;
                        DataTable dt = opp.return_DataTable(mysql);
                        if (dt.Rows.Count > 0)
                        {
                            Balance = decimal.Parse(dt.Rows[0]["Balance"].ToString());
                        }
                        else
                        { Balance = 0; }
                        mysql = " INSERT INTO Customers_Accounts (Notes,Customer_Id, Reference_Id, Reference_Type_Table_Id, Export , Balance, Reference_Type, Transaction_Type, Transaction_Date,Payment_Type) ";
                        mysql = mysql + " Values ('" + global::AccountModelsLib.SqlText.Q(Notes) + "'," + Customer_Id + "," + OrderNo + "," + Money_Financial_Notice_Id + "," + Order_Value + "," + Balance + "," + Reference_Type + "," + Transaction_Type + ",'" + operations.dateFormat(OrderDate) + "',0)";
                        InsertWithManualId(opp, mysql);
                    }
                }
                if (Account_Selection_ == 1)       // موردين
                {

                    if (Transaction_Type == 1)  //استلام نقدية //
                    {
                        ///ليس رصيد أول المدة
                        if (Common.Balance_BeginingPeriod == 0)
                        {
                            mysql = " UPDATE Supplier SET Balance = Balance +  " + Order_Value + "   WHERE (Id = " + Supplier_Id + ")";
                            // arr.Add(mysql);
                            opp.insert_me(mysql);
                        }
                        mysql = "Select * from Supplier Where Id=" + Supplier_Id;
                        DataTable dt = opp.return_DataTable(mysql);
                        if (dt.Rows.Count > 0)
                        {
                            Balance = decimal.Parse(dt.Rows[0]["Balance"].ToString());
                        }
                        else
                        { Balance = 0; }
                        mysql = " INSERT INTO Suppliers_Accounts  (Notes,Supplier_Id,Reference_Id, Reference_Type_Table_Id, Import,Balance,Reference_Type, Transaction_Type, Transaction_Date,Payment_Type) ";
                        mysql = mysql + " Values ('" + global::AccountModelsLib.SqlText.Q(Notes) + "'," + Supplier_Id + "," + OrderNo + "," + Money_Financial_Notice_Id + "," + Order_Value + "," + Balance + "," + Reference_Type + "," + Transaction_Type + ",'" + operations.dateFormat(OrderDate) + "',0)";
                        InsertWithManualId(opp, mysql);
                    }
                    ////////////***********************************************************************************
                    if (Transaction_Type == 2)        // صرف نقدية
                    {
                        ///ليس رصيد أول المدة
                        if (Common.Balance_BeginingPeriod == 0)
                        {
                            mysql = " UPDATE Supplier SET Balance = Balance -  " + Order_Value + "   WHERE (Id = " + Supplier_Id + ")";
                            //  arr.Add(mysql);
                            opp.insert_me(mysql);
                        }
                        mysql = "Select * from Supplier Where Id=" + Supplier_Id;
                        DataTable dt = opp.return_DataTable(mysql);
                        if (dt.Rows.Count > 0)
                        {
                            Balance = decimal.Parse(dt.Rows[0]["Balance"].ToString());
                        }
                        else
                        { Balance = 0; }
                        mysql = " INSERT INTO Suppliers_Accounts  (Notes,Supplier_Id,Reference_Id, Reference_Type_Table_Id,Export ,Balance,Reference_Type, Transaction_Type, Transaction_Date,Payment_Type) ";
                        mysql = mysql + " Values ('" + global::AccountModelsLib.SqlText.Q(Notes) + "'," + Supplier_Id + "," + OrderNo + "," + Money_Financial_Notice_Id + "," + Order_Value + "," + Balance + "," + Reference_Type + "," + Transaction_Type + ",'" + operations.dateFormat(OrderDate) + "',0)";
                        InsertWithManualId(opp, mysql);
                    }

                    Excp = 0;

                }

                string Msg = "";
                //انشاء القيد
                Account_Entry_Invoice_CLS aa = new Account_Entry_Invoice_CLS();
                aa.IsNotDeletePrevEntry = true;
                aa.Make_EntryForMoney_Financial_Notice(ref Excp, ref Msg, ref opp, Money_Financial_Notice_Id, OrderDate, OrderDate, -1);
                Account_Entry_Invoice_CLS.GuardTaswea(Excp, opp, Money_Financial_Notice_Id);
            }
            catch (Exception ee)
            {

                Excp = 1;

            }

        }

        // 🟢 مساعد تقنى محلى فقط (بلا أى منطق عمل جديد): يُستدعى لكل إدراج فى هذه الدالة على جدول من الجداول
        // الثلاثة غير الـIDENTITY (Money_Financial_Notice_Items/Customers_Accounts/Suppliers_Accounts) - راجع
        // التعليق أعلى إدراج Money_Financial_Notice_Items لتفاصيل السبب الجذرى الكامل. يستدعى نفس آلية توليد/حقن
        // Id الموجودة أصلاً فى MySqlParser.SmartEnsureIdInInsertCommandNew صراحة، بصرف النظر عن قيمة
        // Common.Use_Auto_Identity_Column العالمية، ثم يُنفِّذ الاستعلام المُعدَّل عبر opp.insert_me كالمعتاد.
        private static long InsertWithManualId(operations opp, string sql)
        {
            long generatedId = 0;
            sql = MySqlParser.SmartEnsureIdInInsertCommandNew(opp.db, sql, ref generatedId, isAutoIncrement: false);
            opp.insert_me(sql);
            return generatedId;
        }

        public void Delete_Financial_Notice(ref int Excp, DataTable dtOld)
        {
            operations opp = new operations(1);
            Delete_Financial_Notice(ref Excp, dtOld, ref opp);
            if (Excp == 0)
            {
                opp.Commit();
            }
            else
            {
                opp.Rollback();
            }

        }
        public void Delete_Financial_Notice(ref int Excp, DataTable dtOld, ref operations opp)
        {


            long Money_Financial_Notice_Id = long.Parse(dtOld.Rows[0]["Id"].ToString());
            string mysql = "";


            int Transaction_Type = int.Parse(dtOld.Rows[0]["Transaction_Type"].ToString());
            int Account_Selection_ = int.Parse(dtOld.Rows[0]["Account_Selection"].ToString());
            decimal Order_Value = decimal.Parse(dtOld.Rows[0]["Order_Value"].ToString());
            long Customer_Id = long.Parse(dtOld.Rows[0]["Customer_Id"].ToString());
            long Supplier_Id = long.Parse(dtOld.Rows[0]["Supplier_Id"].ToString());
            DateTime OrderDate = DateTime.Parse(dtOld.Rows[0]["OrderDate"].ToString());
            try
            {


                int Reference_Type = Common.Taswea_Malia_Reference_Type;

                mysql = "Delete From Money_Financial_Notice_Items Where Id= " + Money_Financial_Notice_Id;
                opp.insert_me(mysql);


                decimal Balance = 0;
                //(Account_Selection == 0)             حسابات أخرى
                if (Account_Selection_ == 2)  //عملاء
                {
                    mysql = " Select *  From Customers_Accounts Where Reference_Type_Table_Id=" + Money_Financial_Notice_Id + " And Reference_Type=" + Reference_Type + "";
                    DataTable dt = opp.return_DataTable(mysql);
                    long Customers_Accounts_Id = 0;

                    if (dt.Rows.Count > 0)
                    {
                        Customers_Accounts_Id = long.Parse(dt.Rows[0]["Id"].ToString());
                        //Customer_Id = long.Parse(dt.Rows[0]["Customer_Id"].ToString());
                    }

                    mysql = "Delete From Customers_Accounts Where Id=" + Customers_Accounts_Id;
                    opp.insert_me(mysql);
                    if (Transaction_Type == 1)  // أستلام نقدية
                    {
                        ///ليس رصيد أول المدة
                        if (Common.Balance_BeginingPeriod == 0)
                        {
                            mysql = "  UPDATE Customer SET Balance = Balance +  " + Order_Value + " , Balance_Actual = Balance_Actual - " + Order_Value + "  WHERE (Id = " + Customer_Id + ")";

                            opp.insert_me(mysql);
                        }

                        mysql = " Update  Customers_Accounts Set  Balance = Balance + " + Order_Value + " Where Customer_Id=" + Customer_Id + " AND  Id > " + Customers_Accounts_Id;
                        opp.insert_me(mysql);


                    }
                    if (Transaction_Type == 2)  // صرف نقدية
                    { ///ليس رصيد أول المدة
                        if (Common.Balance_BeginingPeriod == 0)
                        {
                            mysql = "  UPDATE Customer SET Balance = Balance -  " + Order_Value + " , Balance_Actual = Balance_Actual + " + Order_Value + "  WHERE (Id = " + Customer_Id + ")";
                            // arr.Add(mysql);
                            opp.insert_me(mysql);
                        }

                        mysql = " Update  Customers_Accounts Set  Balance = Balance - " + Order_Value + " Where  Customer_Id=" + Customer_Id + " AND  Id > " + Customers_Accounts_Id;
                        opp.insert_me(mysql);
                    }
                }
                if (Account_Selection_ == 1)       // موردين
                {
                    mysql = " Select *  From Suppliers_Accounts  Where Reference_Type_Table_Id=" + Money_Financial_Notice_Id + " And Reference_Type=20";
                    DataTable dt = opp.return_DataTable(mysql);
                    long Suppliers_Accounts_Id = 0;
                    if (dt.Rows.Count > 0)
                    {
                        Suppliers_Accounts_Id = long.Parse(dt.Rows[0]["Id"].ToString());
                    }
                    mysql = "Delete From Suppliers_Accounts Where Id=" + Suppliers_Accounts_Id;
                    opp.insert_me(mysql);
                    if (Transaction_Type == 1)  //استلام نقدية //
                    {
                        ///ليس رصيد أول المدة
                        if (Common.Balance_BeginingPeriod == 0)
                        {
                            mysql = " UPDATE Supplier SET Balance = Balance -  " + Order_Value + "   WHERE (Id = " + Supplier_Id + ")";
                            // arr.Add(mysql);
                            opp.insert_me(mysql);
                        }

                        mysql = " Update  Suppliers_Accounts Set  Balance = Balance - " + Order_Value + "  Where  Supplier_Id =" + Supplier_Id + " AND Id > " + Suppliers_Accounts_Id;
                        opp.insert_me(mysql);
                    }
                    ////////////***********************************************************************************
                    if (Transaction_Type == 2)        // صرف نقدية
                    {
                        ///ليس رصيد أول المدة
                        if (Common.Balance_BeginingPeriod == 0)
                        {
                            mysql = " UPDATE Supplier SET Balance = Balance + " + Order_Value + "   WHERE (Id = " + Supplier_Id + ")";
                            //  arr.Add(mysql);
                            opp.insert_me(mysql);
                        }
                        mysql = " Update  Suppliers_Accounts Set  Balance = Balance + " + Order_Value + " Where  Supplier_Id =" + Supplier_Id + " AND  Id > " + Suppliers_Accounts_Id;
                        opp.insert_me(mysql);
                    }
                }

                Excp = 0;

                string Msg = "";
                //انشاء القيد
                Account_Entry_Invoice_CLS aa = new Account_Entry_Invoice_CLS();
                aa.IsNotDeletePrevEntry = false ;
                aa.Make_EntryForMoney_Financial_Notice(ref Excp, ref Msg, ref opp, Money_Financial_Notice_Id, OrderDate, OrderDate, -1);
                Account_Entry_Invoice_CLS.GuardTaswea(Excp, opp, Money_Financial_Notice_Id);

            }

            catch (Exception ee)
            {

                Excp = 1;

            }

        }
        /*
        void _Add_AnotherStaticAccountOnSubmainTable(ref operations opp, long Account_Id, int Transaction_Type, decimal Cash_Value, long SubAccountRef, int Relation_Account_ID, DateTime Day_Date, Account_SelectionNew AccS)
        {
            string mysql = "";
            decimal Import_Value = 0;
            decimal Export_Value = 0;
            int Transaction_Type__ = 0;
            if (Transaction_Type == 1)
            {
                Export_Value = Cash_Value;
                Transaction_Type__ = 2;
            }
            else
            {
                Import_Value = Cash_Value;
                Transaction_Type__ = 1;
            }



            decimal Submain_Account_Balance = 0;
            if (AccS.Head_Main_Account == 1)
            {
                if (AccS.Transaction_Type == 1)
                {
                    Export_Value = 0;
                    Import_Value = Cash_Value;

                }
                else
                {
                    Export_Value = Cash_Value;
                    Import_Value = 0;

                }
                Transaction_Type__ = Transaction_Type;

                if (Transaction_Type == 1)
                {

                    mysql = " UPDATE Submain_Account SET Submain_Account_Balance = Submain_Account_Balance - " + Cash_Value + " WHERE (SubAccount_Id = " + Account_Id + ") ";

                    opp.insert_me(mysql);
                    Submain_Account_Balance = AccS.Submain_Account_Balance - (decimal)Cash_Value;

                }
                if (AccS.Transaction_Type == 2)
                {

                    mysql = " UPDATE Submain_Account SET Submain_Account_Balance = Submain_Account_Balance + " + Cash_Value + " WHERE (SubAccount_Id = " + Account_Id + ") ";

                    opp.insert_me(mysql);
                    Submain_Account_Balance = AccS.Submain_Account_Balance + (decimal)Cash_Value;
                }

                mysql = "INSERT INTO Account_Transaction_Details  (Account_ID, Main_Account_ID, Transaction_Date, Transaction_type, Import_Value, Export_Value, Transaction_Value,Balance, Relation_Main_Account_ID,  Relation_Account_ID) ";
                mysql = mysql + " VALUES (" + Account_Id + "," + AccS.MainSubAccount_ID + ",'" + operations.dateFormat(Day_Date) + "'," + Transaction_Type__ + "," + Import_Value + "," + Export_Value + "," + Cash_Value + "," + Submain_Account_Balance + ",40," + Relation_Account_ID + ")";

                opp.insert_me(mysql);
            }
            if (AccS.Head_Main_Account == 2)
            {
                if (AccS.Transaction_Type == 1)
                {
                    Export_Value = Cash_Value;
                    Import_Value = 0;

                }
                else
                {
                    Export_Value = 0;
                    Import_Value = Cash_Value;

                }
                Transaction_Type__ = AccS.Transaction_Type;

                if (AccS.Transaction_Type == 1)
                {

                    mysql = " UPDATE Submain_Account SET Submain_Account_Balance = Submain_Account_Balance + " + Cash_Value + " WHERE (SubAccount_Id = " + Account_Id + ") ";

                    opp.insert_me(mysql);

                    Submain_Account_Balance = AccS.Submain_Account_Balance + (decimal)Cash_Value;

                }
                if (AccS.Transaction_Type == 2)
                {

                    mysql = " UPDATE Submain_Account SET Submain_Account_Balance = Submain_Account_Balance - " + Cash_Value + " WHERE (SubAccount_Id = " + Account_Id + ") ";

                    opp.insert_me(mysql);
                    Submain_Account_Balance = AccS.Submain_Account_Balance - (decimal)Cash_Value;
                }

                mysql = "INSERT INTO Account_Transaction_Details  (Account_ID, Main_Account_ID, Transaction_Date, Transaction_type, Import_Value, Export_Value, Transaction_Value,Balance, Relation_Main_Account_ID,  Relation_Account_ID) ";
                mysql = mysql + " VALUES (" + Account_Id + "," + AccS.MainSubAccount_ID + ",'" + operations.dateFormat(Day_Date) + "'," + Transaction_Type__ + "," + Import_Value + "," + Export_Value + "," + Cash_Value + "," + Submain_Account_Balance + ",40," + Relation_Account_ID + ")";

                opp.insert_me(mysql);
            }
        }
        */
        public void Edit_Financial_Notice(ref int Excp, DataTable dtOld, long OrderNo, DateTime OrderDate, int Transaction_Type, string Transaction_TypeStr, decimal Order_Value, string Order_Value_Str, long Currency_Id, string Notes, long Customer_Id, long Supplier_Id, long Account_Id, int Account_Type, long SubAccount_Id, int Account_Selection_, decimal Balance_Before  , long Compete_SubAccount_Id)
        {
            operations opp = new operations(1);
            Delete_Financial_Notice(ref Excp, dtOld, ref opp);
            if (Excp == 0)
            {
                opp.Commit();
                operations opp2 = new operations(1);
                Add_New_Financial_Notice(ref Excp, ref opp2, OrderNo, OrderDate, Transaction_Type, Transaction_TypeStr, Order_Value, Order_Value_Str, Currency_Id, Notes, Customer_Id, Supplier_Id, Account_Id, Account_Type, SubAccount_Id, Account_Selection_, Balance_Before, Compete_SubAccount_Id);
                if (Excp == 0)
                {
                    opp2.Commit();
                }
            }
            else
            {
                opp.Rollback();
            }

        }
        public DataTable Get_Money_Financial_Notice(ref int Excp, long OrderNo, bool _dtFrom, DateTime dtFrom, bool _dtTo, DateTime dtTo)
        {
            int Reference_Type = Common.Taswea_Malia_Reference_Type;
            operations opp = new operations();
            string mysql = @"SELECT        Submain_Account.SubAccount_Name, Money_Financial_Notice_Items.*
FROM Money_Financial_Notice_Items INNER JOIN
                         Submain_Account ON Money_Financial_Notice_Items.SubAccount_Id = Submain_Account.Id ";

            mysql = mysql + " Where Money_Financial_Notice_Items.Reference_Type =" + Reference_Type + "";

            if (OrderNo > 0)
            {
                mysql = mysql + " And (Money_Financial_Notice_Items.OrderNo = " + OrderNo + ") ";
            }
            if (_dtFrom == true)
            {
                mysql = mysql + " And (Money_Financial_Notice_Items.OrderDate >=@dtFrom) ";
                opp.cmd.Parameters.AddWithValue("@dtFrom", dtFrom.Date);
            }
            if (_dtTo == true)
            {
                mysql = mysql + " And (Money_Financial_Notice_Items.OrderDate <=@dtTo) ";
                opp.cmd.Parameters.AddWithValue("@dtTo", dtTo.Date);
            }
            mysql = mysql + " Order By Money_Financial_Notice_Items.OrderNo";
            DataTable dt = opp.return_DataTable(mysql);
            return dt;
        }
        public DataTable Get_Money_Financial_NoticeById(ref int Excp, long Money_Financial_Notice_Id)
        {

            operations opp = new operations();
            string mysql = @"SELECT        Submain_Account.SubAccount_Name, Money_Financial_Notice_Items.*
FROM Money_Financial_Notice_Items INNER JOIN
                         Submain_Account ON Money_Financial_Notice_Items.SubAccount_Id = Submain_Account.Id ";

            mysql = mysql + " Where Money_Financial_Notice_Items.Id =" + Money_Financial_Notice_Id + "";


            DataTable dt = opp.return_DataTable(mysql);
            return dt;
        }
    }


}
