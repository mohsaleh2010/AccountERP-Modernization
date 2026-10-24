using System;
using System.Data;
using System.Linq;
using AccountModelsLib.Models;

namespace AccountModelsLib
{
    /// <summary>
    /// فاتورة مبيعات/مرتجع مبيعات «إجمالى» (بلا أصناف): تلفّ الدوال القديمة كما هى (InvoicesTotalCls / InvoicesTotalReverseCls بلا أى تعديل
    /// عليها)، وتضيف فقط: (1) علم الفاتورة الضريبية IsInvoiceTaxInfo الذى لا تكتبه الدوال القديمة (Invoice_Type لا يُستعمل كعلم ضريبة،
    /// يبقى رقم سلسلة الترقيم فقط)، (2) حفظ ما لا تحدّثه دالة التعديل القديمة (DAMGHAT ونسبتا الضريبة)، (3) قائمة الفواتير الإجمالية فقط
    /// (التى لا أصناف لها) بدل قائمة كل الفواتير. الأثر: رصيد العميل وحركته (Customers_Accounts) فقط — لا مخزون ولا قيود يومية.
    /// </summary>
    public class SalesTotalInvoicesService
    {
        private readonly bool _isReturn;
        public SalesTotalInvoicesService(bool isReturn) { _isReturn = isReturn; }

        private string ListTable => _isReturn ? "Invoices_List_Reverse" : "Invoices_List";
        private string DetailsTable => _isReturn ? "Invoices_Details_Reverse" : "Invoices_Details";

        public long NextInvoiceNo()
        {
            var inv = new InvoicesClss();
            return _isReturn ? inv.Make_New_Invoice_ReverseNo(0) : inv.Make_New_Invoice_No(0);
        }

        public bool InvoiceNoExists(long invoiceNo)
        {
            int excp = 0;
            return _isReturn ? new InvoicesTotalReverseCls().Check_InvoiceNo_Found(ref excp, invoiceNo) : new InvoicesClss().Check_InvoiceNo_Found(ref excp, invoiceNo);
        }

        /// <summary>الفواتير الإجمالية فقط (بلا أصناف) بالأحدث أولاً.</summary>
        public DataTable List(DateTime from, DateTime to, string customerLike)
        {
            var sql = "SELECT i.Id, i.Invoice_Id, i.Invoice_Date, i.Customer_Id, c.Name AS Customer_Name, ISNULL(i.Total,0) AS Total, ISNULL(i.Net_Total,0) AS Net_Total, " +
                      "ISNULL(i.IsInvoiceTaxInfo,0) AS IsInvoiceTaxInfo, i.Notes FROM " + ListTable + " i LEFT JOIN Customer c ON c.Id = i.Customer_Id " +
                      "WHERE NOT EXISTS (SELECT 1 FROM " + DetailsTable + " d WHERE d.Invoice_Id = i.Id) " +
                      "AND i.Invoice_Date >= '" + operations.dateFormat(from.Date) + "' AND i.Invoice_Date < '" + operations.dateFormat(to.Date.AddDays(1)) + "' ";
            if (!string.IsNullOrWhiteSpace(customerLike)) sql += "AND c.Name LIKE N'%" + customerLike.Trim().Replace("'", "''") + "%' ";
            sql += "ORDER BY i.Id DESC";
            return new operations().return_DataTable(sql);
        }

        public DataRow? GetOne(long id)
        {
            var dt = new operations().return_DataTable("SELECT * FROM " + ListTable + " WHERE Id = " + id);
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        /// <summary>يحفظ فاتورة إجمالية جديدة عبر insertinto القديمة ثم يضبط علم الضريبة. يعيد رقم سجل الفاتورة.</summary>
        public long Insert(long currencyId, DateTime date, decimal invoiceValue, decimal netTotal, decimal commissionPercent, decimal taxRate, decimal ats05Rate,
                           decimal taxValue, decimal ats05Value, long customerId, long invoiceNo, string notes, decimal damghat, bool isTax)
        {
            notes = (notes ?? "").Replace("'", "''");
            // ترتيب الوسائط كما فى الفورم القديم: (total = صافى الفاتورة النهائى، nettotal = قيمة الفاتورة قبل الضريبة).
            if (_isReturn)
                new InvoicesTotalReverseCls().insertinto(currencyId, date, netTotal, commissionPercent, taxRate, ats05Rate, taxValue, ats05Value, invoiceValue, customerId, invoiceNo, notes, 0, damghat);
            else
                new InvoicesTotalCls().insertinto(currencyId, date, netTotal, commissionPercent, taxRate, ats05Rate, taxValue, ats05Value, invoiceValue, customerId, invoiceNo, notes, 0, damghat);

            var opp = new operations();
            var dt = opp.return_DataTable("SELECT TOP 1 Id FROM " + ListTable + " WHERE Invoice_Id = " + invoiceNo + " AND Customer_Id = " + customerId + " ORDER BY Id DESC");
            if (dt.Rows.Count == 0) throw new InvalidOperationException("Invoice was not saved");
            var id = Convert.ToInt64(dt.Rows[0]["Id"]);
            opp.update_me("UPDATE " + ListTable + " SET IsInvoiceTaxInfo = " + (isTax ? 1 : 0) + ", Is_Tax_Per_Item = 0 WHERE Id = " + id);
            return id;
        }

        /// <summary>يعدّل عبر updateinvoice القديمة (لا تغيّر العميل) ثم يحدّث ما لا تحدّثه: DAMGHAT والنسب وعلم الضريبة.</summary>
        public void Update(long id, decimal oldNetTotal, long currencyId, DateTime date, decimal invoiceValue, decimal netTotal, decimal commissionPercent, decimal taxRate,
                           decimal ats05Rate, decimal taxValue, decimal ats05Value, long customerId, string notes, decimal damghat, bool isTax)
        {
            int excp = 0;
            notes = (notes ?? "").Replace("'", "''");
            if (_isReturn)
                new InvoicesTotalReverseCls().updateinvoice(ref excp, oldNetTotal, currencyId, date, invoiceValue, commissionPercent, taxValue, ats05Value, netTotal, customerId, id, notes, customerId, taxRate, ats05Rate, damghat);
            else
                new InvoicesTotalCls().updateinvoice(ref excp, oldNetTotal, currencyId, date, invoiceValue, commissionPercent, taxValue, ats05Value, netTotal, customerId, id, notes, customerId, taxRate, ats05Rate, damghat);
            if (excp != 0) throw new InvalidOperationException("Update failed");

            // DAMGHAT والنسب تُحفظ داخل updateinvoice الآن؛ يبقى علم الضريبة فقط (لا تحدّثه الدالة القديمة)
            new operations().update_me("UPDATE " + ListTable + " SET IsInvoiceTaxInfo = " + (isTax ? 1 : 0) + " WHERE Id = " + id);
        }

        public void Delete(long id, decimal netTotal, long invoiceNo, long customerId)
        {
            int excp = 0;
            if (_isReturn)
                new InvoicesTotalReverseCls().deleteinvoice(ref excp, netTotal, id.ToString(), invoiceNo.ToString(), customerId.ToString(), netTotal.ToString());
            else
                new InvoicesTotalCls().deleteinvoice(ref excp, netTotal, id.ToString(), invoiceNo.ToString(), customerId.ToString(), netTotal.ToString());
            if (excp != 0) throw new InvalidOperationException("Delete failed");
        }
    }
}
