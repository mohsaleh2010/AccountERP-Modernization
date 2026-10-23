using System;
using System.Data;
using System.Globalization;

namespace AccountModelsLib.BusinessLibShared.Store
{
    // 🟢 نسخة الموردين من متابعة الفواتير المستحقة. لا يوجد نظير لها فى سطح المكتب (شاشة Invoices_List_ReceiptDistributionReport للعملاء فقط)،
    // فبُنيت على نمط InvoicesClss.Get_Customers_Invoices: المدفوع = صافى حركات الخزنة Money_Transaction_Box المرتبطة بالفاتورة
    // (DataDictionaryProcessesListsId = Bill_Type_Id و Reference_Type_Table_Id = معرّف الفاتورة) باتجاه المورد (Export - Import)،
    // والمتبقى = الصافى - المدفوع (لا يقل عن صفر). جدول توزيع السندات Money_Transaction_BoxForInvoices غير مستخدم لفواتير المشتريات فى بيانات الشركة.
    public class SupplierBillsDueCLS
    {
        private const string PaidSql =
            "ISNULL((SELECT ISNULL(SUM(Export_Value), 0) - ISNULL(SUM(Import_Value), 0) FROM Money_Transaction_Box " +
            "WHERE (DataDictionaryProcessesListsId = {0}) AND (Reference_Type_Table_Id = Bills_List.Id)), 0)";

        // isInvoiceAgile: -1 الكل، 0 نقدى، 1 آجل. paidStatus: -1 الكل، 1 مسددة (المتبقى صفر)، 0 غير مسددة (المتبقى أكبر من صفر).
        // dueFrom/dueTo: تاريخ الاستحقاق (اختيارى، للفواتير التى لها تاريخ استحقاق فقط).
        public DataTable GetBills(long invoiceNo, long supplierId, bool useFrom, DateTime from, bool useTo, DateTime to,
                                  int isInvoiceAgile, int paidStatus, bool useDueFrom, DateTime dueFrom, bool useDueTo, DateTime dueTo)
        {
            var opp = new AccountModelsLib.operations();
            var paid = string.Format(CultureInfo.InvariantCulture, PaidSql, AccountModelsLib.Common.Bill_Type_Id);
            var still = "(Bills_List.Net_Total - " + paid + ")";
            var sql =
                "SELECT Bills_List.Id, Bills_List.Invoice_Id, Bills_List.Invoice_Date, Bills_List.InvoiceDueDate, Bills_List.Net_Total, Bills_List.Notes, " +
                "Bills_List.Is_Invoice_Agile, Supplier.Id AS Supplier_Id, Supplier.Name, USER_DETAILES.USER_NAME, " +
                paid + " AS PaymentValue, IIF(" + still + " > 0, " + still + ", 0) AS StillValues " +
                "FROM Bills_List INNER JOIN Supplier ON Bills_List.Supplier_Id = Supplier.Id " +
                "LEFT OUTER JOIN USER_DETAILES ON Bills_List.User_Id = USER_DETAILES.Id " +
                "WHERE (ISNULL(Bills_List.Deleted, 0) <> 1) ";
            if (invoiceNo > 0)
            {
                // الأصل يبحث بالرقم وحده (بلا مورد ولا تواريخ)
                sql += " AND (Bills_List.Invoice_Id = " + invoiceNo + ") ";
            }
            else
            {
                if (useFrom) sql += " AND (CONVERT(DATE, Bills_List.Invoice_Date) >= '" + AccountModelsLib.operations.dateFormat(from.Date) + "') ";
                if (useTo) sql += " AND (CONVERT(DATE, Bills_List.Invoice_Date) <= '" + AccountModelsLib.operations.dateFormat(to.Date) + "') ";
                if (useDueFrom) sql += " AND (CONVERT(DATE, Bills_List.InvoiceDueDate) >= '" + AccountModelsLib.operations.dateFormat(dueFrom.Date) + "') ";
                if (useDueTo) sql += " AND (CONVERT(DATE, Bills_List.InvoiceDueDate) <= '" + AccountModelsLib.operations.dateFormat(dueTo.Date) + "') ";
                if (supplierId > 0) sql += " AND (Supplier.Id = " + supplierId + ") ";
                if (isInvoiceAgile > -1) sql += " AND (Bills_List.Is_Invoice_Agile = " + isInvoiceAgile + ") ";
                if (paidStatus == 1) sql += " AND (" + still + " <= 0) ";
                if (paidStatus == 0) sql += " AND (" + still + " > 0) ";
            }
            sql += " ORDER BY Bills_List.Invoice_Id ";
            return opp.return_DataTable(sql);
        }

        // مدفوعات الفاتورة المحددة (سندات الصرف/القبض المرتبطة بها)، وتتضمن صف إجمالى بلا رقم إذن يتخطاه العرض.
        public DataTable GetPayments(long billId)
        {
            return new AccountModelsLib.Money_Transaction_BoxOrderReportCLS()
                .GetMoney_Box_TransactionInvoices(AccountModelsLib.Common.Bill_Type_Id, billId);
        }
    }
}
