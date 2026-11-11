using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Data.SqlClient; // أو System.Data.SqlClient في المشاريع القديمة

namespace AccountModelsLib
{
    public class SqlServerFullMigratorToMainServerCLS
    {

        // ⚙️ إعدادات الاتصال ومسار المخرجات
        // لا نصوص اتصال داخل المكتبة: البرنامج المستدعي يمررها من ملف إعداداته (Alter_DataBase: CommonFiles\ConnInfo.xml → Str عبر MigratorConnConfig)
        public string sourceConnectionString = "";
        public string destinationConnectionString = "";

        /// <summary>
        /// تدوير أرصدة العملاء قبل تاريخ 1 أغسطس 2026 وتحديث رصيد أول المدة لكل عميل وحذف الحركات السابقة
        /// </summary>
        public void RecalculateAndUpdateCustomerOpeningBalanceAndPurgeOldTransactions(ref operations opp, ref int Excp, ref string Msg, DateTime cutoffDate)
        {
            try
            {
                //DateTime cutoffDate = new DateTime(2026, 8, 1, 0, 0, 0);
                string cutoffDateStr = operations.Get_DateToInsert2(cutoffDate);

                // 1. جلب قائمة بمعرفات كل العملاء المتواجدين في جدول الحسابات
                string queryCustomers = "SELECT DISTINCT Customer_Id FROM Customers_Accounts WHERE Customer_Id IS NOT NULL";
                DataTable dtCustomers = opp.return_DataTable(queryCustomers);

                if (dtCustomers == null || dtCustomers.Rows.Count == 0)
                {
                    Msg = "لا توجد حركات أو عملاء في جدول Customers_Accounts";
                    Excp = 0;
                    return;
                }

                foreach (DataRow row in dtCustomers.Rows)
                {
                    long customerId = Convert.ToInt64(row["Customer_Id"]);

                    // 2. حساب إجمالي الحركات + رصيد أول المدة السابق لما قبل 1 أغسطس 2026
                    // ملاحظة: الرصيد الافتتاحي له Reference_Type = -7
                    string queryTotals = $@"
                SELECT 
                    ISNULL(SUM(Import), 0) AS TotalImport,
                    ISNULL(SUM(Export), 0) AS TotalExport
                FROM Customers_Accounts
                WHERE Customer_Id = {customerId}
                  AND Transaction_Date < '{cutoffDateStr}'";

                    DataTable dtTotals = opp.return_DataTable(queryTotals);

                    decimal totalImport = 0;
                    decimal totalExport = 0;

                    if (dtTotals != null && dtTotals.Rows.Count > 0)
                    {
                        totalImport = Convert.ToDecimal(dtTotals.Rows[0]["TotalImport"]);
                        totalExport = Convert.ToDecimal(dtTotals.Rows[0]["TotalExport"]);
                    }

                    // 3. حساب الفرق بين المدين والدائن
                    // في حسابات العملاء: Export = مدين (مبيعات/مستحقات على العميل)، Import = دائن (تحصيلات/دفعات مقدمة)
                    decimal netBalance = totalExport - totalImport;
                    decimal newExport = 0;
                    decimal newImport = 0;

                    if (netBalance > 0)
                    {
                        // رصيد مدين (على العميل)
                        newExport = netBalance;
                        newImport = 0;
                    }
                    else if (netBalance < 0)
                    {
                        // رصيد دائن (للعميل / دفعة مقدمة)
                        newExport = 0;
                        newImport = Math.Abs(netBalance);
                    }
                    else
                    {
                        // الحساب متزن تماماً
                        newExport = 0;
                        newImport = 0;
                    }

                    // 4. التحقق من وجود صف رصيد أول المدة السابق (Reference_Type = -7) للعميل
                    string queryCheckOB = $@"
                SELECT Id 
                FROM Customers_Accounts 
                WHERE Customer_Id = {customerId} 
                  AND Reference_Type = -7";

                    DataTable dtOB = opp.return_DataTable(queryCheckOB);

                    if (dtOB != null && dtOB.Rows.Count > 0)
                    {
                        // تحديث صف أول المدة القائم
                        long obRecordId = Convert.ToInt64(dtOB.Rows[0]["Id"]);
                        string updateSql = $@"
                    UPDATE Customers_Accounts 
                    SET 
                        Import = {newImport},
                        Export = {newExport},
                        Transaction_Date = '{cutoffDateStr}',
                        Update_ServerDateTime = GETDATE()
                    WHERE Id = {obRecordId} 
                      AND Reference_Type = -7";

                        opp.insert_me(updateSql);
                    }
                    else
                    {
                        // في حال عدم وجود صف أول مدة مسبق، يتم إدراجه كصف جديد
                        string insertSql = $@"
                    INSERT INTO Customers_Accounts 
                    (
                        Customer_Id,
                        Import,
                        Export,
                        Transaction_Date,
                        Reference_Type,
                        Insert_ServerDateTime,
                        Update_ServerDateTime
                    )
                    VALUES 
                    (
                        {customerId},
                        {newImport},
                        {newExport},
                        '{cutoffDateStr}',
                        -7,
                        GETDATE(),
                        GETDATE()
                    )";

                        opp.insert_me(insertSql);
                    }

                    // 5. حذف كافة العمليات السابقة لتاريخ 1 أغسطس 2026 مع استبعاد (Reference_Type = -7) بصراحة تامة
                    string deleteOldSql = $@"
                DELETE FROM Customers_Accounts 
                WHERE Customer_Id = {customerId} 
                  AND Transaction_Date < '{cutoffDateStr}' 
                  AND Reference_Type <> -7";

                    opp.insert_me(deleteOldSql);
                }

                Excp = 0;
                Msg = "تمت تسوية وتحديث أرصدة أول المدة لجميع العملاء وحذف الحركات السابقة لتاريخ 01-08-2026 بنجاح";
            }
            catch (Exception ex)
            {
                Excp = 1;
                Msg = "حدث خطأ أثناء تدوير أرصدة العملاء: " + ex.Message;
            }
        }
        /// <summary>
        /// تدوير أرصدة الموردين قبل تاريخ 1 أغسطس 2026 وتحديث رصيد أول المدة لكل مورد وحذف الحركات السابقة
        /// </summary>
        public void RecalculateAndUpdateSupplierOpeningBalanceAndPurgeOldTransactions(ref operations opp, ref int Excp, ref string Msg, DateTime cutoffDate)
        {
            try
            {
                //DateTime cutoffDate = new DateTime(2026, 8, 1, 0, 0, 0);
                string cutoffDateStr = operations.Get_DateToInsert2(cutoffDate);

                // 1. جلب قائمة بمعرفات الموردين المتواجدين في جدول الحسابات
                string querySuppliers = "SELECT DISTINCT Supplier_Id FROM Suppliers_Accounts WHERE Supplier_Id IS NOT NULL";
                DataTable dtSuppliers = opp.return_DataTable(querySuppliers);

                if (dtSuppliers == null || dtSuppliers.Rows.Count == 0)
                {
                    Msg = "لا توجد حركات أو موردين في جدول Suppliers_Accounts";
                    Excp = 0;
                    return;
                }

                foreach (DataRow row in dtSuppliers.Rows)
                {
                    long supplierId = Convert.ToInt64(row["Supplier_Id"]);
                    if(supplierId== 20025) {

                        }
                    // 2. حساب إجمالي الحركات + رصيد أول المدة السابق لما قبل 1 أغسطس 2026
                    string queryTotals = $@"
                SELECT 
                    ISNULL(SUM(Import), 0) AS TotalImport,
                    ISNULL(SUM(Export), 0) AS TotalExport
                FROM Suppliers_Accounts
                WHERE Supplier_Id = {supplierId}
                  AND Transaction_Date < '{cutoffDateStr}'";

                    DataTable dtTotals = opp.return_DataTable(queryTotals);

                    decimal totalImport = 0;
                    decimal totalExport = 0;

                    if (dtTotals != null && dtTotals.Rows.Count > 0)
                    {
                        totalImport = Convert.ToDecimal(dtTotals.Rows[0]["TotalImport"]);
                        totalExport = Convert.ToDecimal(dtTotals.Rows[0]["TotalExport"]);
                    }

                    // 3. حساب الفرق بين الدائن والمدين للمورد
                    // المورد دائن بحركات الشراء (Import) ومدين بالسداد (Export)
                    decimal netBalance = totalImport - totalExport;
                    decimal newImport = 0;
                    decimal newExport = 0;
                    decimal finalBalance = 0;

                    if (netBalance > 0)
                    {
                        // رصيد دائن (المورد له مستحقات)
                        newImport = netBalance;
                        newExport = 0;
                        finalBalance = netBalance;
                    }
                    else if (netBalance < 0)
                    {
                        // رصيد مدين (دفعنا له دفعات مقدمة)
                        newImport = 0;
                        newExport = Math.Abs(netBalance);
                        finalBalance = netBalance; // سالب
                    }
                    else
                    {
                        // الحساب خالص تماماً
                        newImport = 0;
                        newExport = 0;
                        finalBalance = 0;
                    }

                    // 4. التحقق من وجود صف رصيد أول المدة السابق (Reference_Type = -7) للمورد
                    string queryCheckOB = $@"
                SELECT Id 
                FROM Suppliers_Accounts 
                WHERE Supplier_Id = {supplierId} 
                  AND Reference_Type = -7";

                    DataTable dtOB = opp.return_DataTable(queryCheckOB);

                    if (dtOB != null && dtOB.Rows.Count > 0)
                    {
                        // تحديث صف أول المدة القائم
                        long obRecordId = Convert.ToInt64(dtOB.Rows[0]["Id"]);
                        string updateSql = $@"
                    UPDATE Suppliers_Accounts 
                    SET 
                        Import = {newImport},
                        Export = {newExport},
                        Balance = {finalBalance},
                        Transaction_Date = '{cutoffDateStr}',
                        Update_ServerDateTime = GETDATE()
                    WHERE Id = {obRecordId} 
                      AND Reference_Type = -7";

                        opp.insert_me(updateSql);
                    }
                    else
                    {
                        // في حال عدم وجود صف أول مدة مسبق، يتم إدراجه كصف جديد
                        string insertSql = $@"
                    INSERT INTO Suppliers_Accounts 
                    (
                        Supplier_Id,
                        Reference_Id,
                        Reference_Type_Table_Id,
                        Export,
                        Import,
                        Balance,
                        Transaction_Type,
                        Transaction_Date,
                        Reference_Type,
                        Notes,
                        Insert_ServerDateTime,
                        Update_ServerDateTime
                    )
                    VALUES 
                    (
                        {supplierId},
                        0,
                        {supplierId},
                        {newExport},
                        {newImport},
                        {finalBalance},
                        2,
                        '{cutoffDateStr}',
                        -7,
                        N'رصيد مدور حتى 01/08/2026',
                        GETDATE(),
                        GETDATE()
                    )";

                        opp.insert_me(insertSql);
                    }

                    // 5. حذف كافة العمليات السابقة لتاريخ 1 أغسطس 2026 مع استبعاد (Reference_Type = -7)
                    string deleteOldSql = $@"
                DELETE FROM Suppliers_Accounts 
                WHERE Supplier_Id = {supplierId} 
                  AND Transaction_Date < '{cutoffDateStr}' 
                  AND Reference_Type <> -7";

                    opp.insert_me(deleteOldSql);
                }

                Excp = 0;
                Msg = "تمت تسوية وتحديث أرصدة أول المدة لجميع الموردين وحذف العمليات السابقة لتاريخ 01-08-2026 بنجاح";
            }
            catch (Exception ex)
            {
                Excp = 1;
                Msg = "حدث خطأ أثناء تدوير أرصدة الموردين: " + ex.Message;
            }
        }



        /// <summary>
        /// ميثود لحذف الحركات والعمليات التاريخية لجميع الجداول حتى تاريخ محدد
        /// مع استثناء أرصدة أول المدة في حسابات العملاء والموردين (Reference_Type = -7)
        /// </summary>
        /// <param name="opp">كائن العمليات للاتصال بقاعدة البيانات</param>
        /// <param name="cutoffDate">تاريخ القطع المطلوب الحذف قبله</param>
        /// <param name="Excp">مؤشر حدوث خطأ (0: نجاح، 1: خطأ)</param>
        /// <param name="Msg">رسالة التقرير أو الخطأ</param>
        public void PurgeHistoricalTransactionsBeforeDate(ref operations opp, ref int Excp, ref string Msg, DateTime cutoffDate)
        {
            try
            {
                // 1. تنسيق التاريخ بصيغة قياسية آمنة لمحرك SQL Server (yyyy-MM-dd HH:mm:ss)
                // أو استخدام operations.Get_DateToInsert(cutoffDate) المعتمدة في مشروعك
                string dateStr = cutoffDate.ToString("yyyy-MM-dd HH:mm:ss");

                // 2. تجميع جمل الحذف بالترتيب الصحيح (حذف التفاصيل أولاً ثم الرؤوس لتجنب Foreign Key Conflicts)
                StringBuilder sbSql = new StringBuilder();

                

                // ----------------- مشتريات وتوريدات -----------------
                // حذف تفاصيل المشتريات
                sbSql.AppendLine($@"
            DELETE FROM Detailes_Purshing
            FROM Order_Purshing 
            INNER JOIN Detailes_Purshing ON Order_Purshing.Id = Detailes_Purshing.OrderTable_id
            WHERE (Order_Purshing.Order_date < CONVERT(DATETIME, '{dateStr}', 120));");

                // حذف رؤوس المشتريات
                sbSql.AppendLine($@"
            DELETE FROM Order_Purshing
            WHERE (Order_date < CONVERT(DATETIME, '{dateStr}', 120));");

                // حذف حركات المخازن (الصادر والوارد)
                sbSql.AppendLine($@"
            DELETE FROM Detailes_Purshing_Import_Export
            WHERE (Process_Date < CONVERT(DATETIME, '{dateStr}', 120));");

                // ----------------- حركة النقدية والصندوق -----------------
                // حذف تفاصيل حركة النقدية
                sbSql.AppendLine($@"
            DELETE FROM Money_Transaction_Box
            FROM Order_Money 
            INNER JOIN Money_Transaction_Box ON Order_Money.Id = Money_Transaction_Box.Order_Mony_Id
            WHERE (Order_Money.Order_Date < CONVERT(DATETIME, '{dateStr}', 120));");

                // حذف رؤوس حركة النقدية
                sbSql.AppendLine($@"
            DELETE FROM Order_Money
            WHERE (Order_Date < CONVERT(DATETIME, '{dateStr}', 120));");

                // ----------------- فواتير الشراء (Bills) -----------------
                // تفاصيل فواتير الشراء
                sbSql.AppendLine($@"
            DELETE FROM Bills_Details
            FROM Bills_List 
            INNER JOIN Bills_Details ON Bills_List.ID = Bills_Details.Invoice_Id
            WHERE (Bills_List.Invoice_Date < CONVERT(DATETIME, '{dateStr}', 120));");

                // رؤوس فواتير الشراء
                sbSql.AppendLine($@"
            DELETE FROM Bills_List
            WHERE (Invoice_Date < CONVERT(DATETIME, '{dateStr}', 120));");

                // ----------------- مرتجع الشراء (Bills Reverse) -----------------
                // تفاصيل مرتجع الشراء
                sbSql.AppendLine($@"
            DELETE FROM Bills_Details_Reverse
            FROM Bills_List_Reverse 
            INNER JOIN Bills_Details_Reverse ON Bills_List_Reverse.ID = Bills_Details_Reverse.Invoice_Id
            WHERE (Bills_List_Reverse.Invoice_Date < CONVERT(DATETIME, '{dateStr}', 120));");

                // رؤوس مرتجع الشراء
                sbSql.AppendLine($@"
            DELETE FROM Bills_List_Reverse
            WHERE (Invoice_Date < CONVERT(DATETIME, '{dateStr}', 120));");

                // ----------------- فواتير المبيعات (Invoices) -----------------
                // تفاصيل فواتير المبيعات
                sbSql.AppendLine($@"
            DELETE FROM Invoices_Details
            FROM Invoices_List 
            INNER JOIN Invoices_Details ON Invoices_List.ID = Invoices_Details.Invoice_Id
            WHERE (Invoices_List.Invoice_Date < CONVERT(DATETIME, '{dateStr}', 120));");

                // رؤوس فواتير المبيعات
                sbSql.AppendLine($@"
            DELETE FROM Invoices_List
            WHERE (Invoice_Date < CONVERT(DATETIME, '{dateStr}', 120));");

                // ----------------- مرتجع المبيعات (Invoices Reverse) -----------------
                // تفاصيل مرتجع المبيعات
                sbSql.AppendLine($@"
            DELETE FROM Invoices_Details_Reverse
            FROM Invoices_List_Reverse 
            INNER JOIN Invoices_Details_Reverse ON Invoices_List_Reverse.ID = Invoices_Details_Reverse.Invoice_Id
            WHERE (Invoices_List_Reverse.Invoice_Date < CONVERT(DATETIME, '{dateStr}', 120));");

                // رؤوس مرتجع المبيعات
                sbSql.AppendLine($@"
            DELETE FROM Invoices_List_Reverse
            WHERE (Invoice_Date < CONVERT(DATETIME, '{dateStr}', 120));");

                // ----------------- قيود اليومية العامة (GL Entries) -----------------
                // أسطر تفاصيل القيود
                sbSql.AppendLine($@"
            DELETE FROM Account_Entry_Details
            FROM Account_Entry_List 
            INNER JOIN Account_Entry_Details ON Account_Entry_List.Id = Account_Entry_Details.Account_Entry_List_ID
            WHERE (Account_Entry_List.Entry_Date < CONVERT(DATETIME, '{dateStr}', 120));");

                // رؤوس القيود
                sbSql.AppendLine($@"
            DELETE FROM Account_Entry_List
            WHERE (Entry_Date < CONVERT(DATETIME, '{dateStr}', 120));");

                // ----------------- حسابات العملاء والموردين المباشرة -----------------
                // حذف حركات العملاء مع حماية رصيد أول المدة
                sbSql.AppendLine($@"
            DELETE FROM Customers_Accounts
            WHERE (Reference_Type <> -7) 
              AND (Transaction_Date < CONVERT(DATETIME, '{dateStr}', 120));");

                // حذف حركات الموردين مع حماية رصيد أول المدة
                sbSql.AppendLine($@"
            DELETE FROM Suppliers_Accounts
            WHERE (Reference_Type <> -7) 
              AND (Transaction_Date < CONVERT(DATETIME, '{dateStr}', 120));");

                

                // 3. تنفيذ الاستعلام المجمّع دفعة واحدة
                opp.insert_me(sbSql.ToString());

                Excp = 0;
                Msg = $"تم حذف جميع الحركات والعمليات السابقة لتاريخ {cutoffDate:yyyy/MM/dd} بنجاح.";
            }
            catch (Exception ex)
            {
                Excp = 1;
                Msg = "حدث خطأ أثناء تنفيذ عملية الحذف: " + ex.Message;
            }
        }
    }
}