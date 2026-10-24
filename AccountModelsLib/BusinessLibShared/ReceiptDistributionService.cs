using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using AccountModelsLib.Models;

namespace AccountModelsLib
{
    /// <summary>
    /// توزيع إذن قبض من عميل على فواتيره (شاشة Invoices_List_ReceiptDistribution الأصلية): يكتب أسطر Money_Transaction_BoxForInvoices
    /// لإذن واحد (حفظ = استبدال أسطر هذا الإذن فقط، حذف = إزالتها). لا يلمس الإذن نفسه ولا قيوده ولا أرصدة الصناديق.
    /// فروق مقصودة عن الأصل (كما تفعل أنظمة ERP الحديثة عند تطبيق الدفعة): المتبقى للتحقق يستثنى توزيع هذا الإذن نفسه فيجوز
    /// إعادة حفظ نفس المبلغ، والفاتورة يجب أن تخص عميل الإذن، ومجموع التوزيع لا يتعدى قيمة الإذن.
    /// </summary>
    public class ReceiptDistributionService
    {
        public record ReceiptRow(long BoxLineId, long OrderMoneyId, long OrderNo, DateTime? Date, long CustomerId, string Customer, string Bank, decimal Value, decimal Allocated);

        public record InvoiceRow(long Id, long InvoiceNo, DateTime? Date, DateTime? DueDate, string Customer, decimal Net, decimal PaidAll, decimal Remaining,
                                 decimal Allocated, long AllocationId, string Notes, string User);

        private const decimal Tolerance = 0.005m;

        /// <summary>أذون القبض من العملاء (Get_OrderCachFromCustomers الأصلية: سطر عميل Account_Type=2 ونوع 1) مع المُوزَّع منها.</summary>
        public List<ReceiptRow> SearchReceipts(long bankId, long customerId, DateTime? from, DateTime? to)
        {
            var sql = @"SELECT m.Id, m.Order_Mony_Id, ISNULL(m.Order_No,0) AS Order_No, m.Transaction_Date, m.Customer_Id, ISNULL(c.Name,'') AS CustomerName, ISNULL(b.Bank_Name,'') AS Bank_Name,
                               ISNULL(m.Transaction_Value,0) AS Val,
                               ISNULL((SELECT SUM(ISNULL(f.Transaction_Value,0)) FROM Money_Transaction_BoxForInvoices f WHERE f.Money_Transaction_BoxId = m.Id),0) AS Allocated
                        FROM Money_Transaction_Box m
                        LEFT JOIN Customer c ON c.Id = m.Customer_Id
                        LEFT JOIN Banks b ON b.Id = m.Bank_Id
                        WHERE m.Account_Type = 2 AND m.Transaction_Type = 1";
            if (bankId > 0) sql += " AND m.Bank_Id = " + bankId.ToString(CultureInfo.InvariantCulture);
            if (customerId > 0) sql += " AND m.Customer_Id = " + customerId.ToString(CultureInfo.InvariantCulture);
            if (from != null) sql += " AND CONVERT(DATE, m.Transaction_Date) >= '" + from.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + "'";
            if (to != null) sql += " AND CONVERT(DATE, m.Transaction_Date) <= '" + to.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + "'";
            sql += " ORDER BY m.Transaction_Date DESC, m.Id DESC";

            var dt = new operations().return_DataTable(sql);
            return dt.Rows.Cast<DataRow>().Select(r => new ReceiptRow(
                Convert.ToInt64(r["Id"]), r["Order_Mony_Id"] == DBNull.Value ? 0 : Convert.ToInt64(r["Order_Mony_Id"]), Convert.ToInt64(r["Order_No"]),
                r["Transaction_Date"] == DBNull.Value ? null : Convert.ToDateTime(r["Transaction_Date"]), Convert.ToInt64(r["Customer_Id"]),
                r["CustomerName"].ToString()!.Trim(), r["Bank_Name"].ToString()!.Trim(), Convert.ToDecimal(r["Val"]), Convert.ToDecimal(r["Allocated"]))).ToList();
        }

        public ReceiptRow? GetReceipt(long boxLineId) =>
            SearchReceipts(0, 0, null, null).FirstOrDefault(r => r.BoxLineId == boxLineId);

        /// <summary>فواتير عميل الإذن مع توزيع هذا الإذن عليها. unpaidOnly: المتبقى (بعد استثناء توزيع هذا الإذن) أكبر من صفر أو للإذن توزيع عليها.</summary>
        public List<InvoiceRow> GetInvoices(long boxLineId, long customerId, DateTime? from, DateTime? to, long invoiceNo, bool unpaidOnly)
        {
            var box = boxLineId.ToString(CultureInfo.InvariantCulture);
            var sql = @"SELECT i.Id, ISNULL(i.Invoice_Id,0) AS Invoice_Id, i.Invoice_Date, i.InvoiceDueDate, ISNULL(c.Name,'') AS CustomerName, ISNULL(i.Net_Total,0) AS Net,
                               ISNULL(i.Notes,'') AS Notes, ISNULL(u.USER_NAME,'') AS UserName,
                               ISNULL((SELECT SUM(ISNULL(f.Import_Value,0) - ISNULL(f.Export_Value,0)) FROM Money_Transaction_BoxForInvoices f WHERE f.Invoices_ListId = i.Id),0) AS PaidAll,
                               ISNULL((SELECT SUM(ISNULL(f.Transaction_Value,0)) FROM Money_Transaction_BoxForInvoices f WHERE f.Invoices_ListId = i.Id AND f.Money_Transaction_BoxId = " + box + @"),0) AS Allocated,
                               ISNULL((SELECT MAX(f.Id) FROM Money_Transaction_BoxForInvoices f WHERE f.Invoices_ListId = i.Id AND f.Money_Transaction_BoxId = " + box + @"),0) AS AllocationId
                        FROM Invoices_List i
                        INNER JOIN Customer c ON c.Id = i.Customer_Id
                        LEFT JOIN USER_DETAILES u ON u.Id = i.User_Id
                        WHERE ISNULL(i.Is_DeletedProcessed,0) <> 1 AND i.Customer_Id = " + customerId.ToString(CultureInfo.InvariantCulture);
            if (invoiceNo > 0) sql += " AND i.Invoice_Id = " + invoiceNo.ToString(CultureInfo.InvariantCulture);
            else
            {
                if (from != null) sql += " AND CONVERT(DATE, i.Invoice_Date) >= '" + from.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + "'";
                if (to != null) sql += " AND CONVERT(DATE, i.Invoice_Date) <= '" + to.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + "'";
            }
            var taxFlag = Common.PermissionToShowTaxInvoiceOrNotORAll();
            if (taxFlag > -1) sql += " AND i.IsInvoiceTaxInfo = " + taxFlag.ToString(CultureInfo.InvariantCulture);
            sql += " ORDER BY i.Invoice_Id";

            var dt = new operations().return_DataTable(sql);
            var list = new List<InvoiceRow>();
            foreach (DataRow r in dt.Rows)
            {
                var net = Convert.ToDecimal(r["Net"]);
                var paidAll = Convert.ToDecimal(r["PaidAll"]);
                var allocated = Convert.ToDecimal(r["Allocated"]);
                var remaining = Math.Max(0, net - (paidAll - allocated));
                if (unpaidOnly && remaining <= Tolerance && allocated <= 0) continue;
                list.Add(new InvoiceRow(Convert.ToInt64(r["Id"]), Convert.ToInt64(r["Invoice_Id"]),
                    r["Invoice_Date"] == DBNull.Value ? null : Convert.ToDateTime(r["Invoice_Date"]),
                    r["InvoiceDueDate"] == DBNull.Value ? null : Convert.ToDateTime(r["InvoiceDueDate"]),
                    r["CustomerName"].ToString()!.Trim(), net, paidAll, remaining, allocated, Convert.ToInt64(r["AllocationId"]),
                    r["Notes"].ToString() ?? "", r["UserName"].ToString() ?? ""));
            }
            return list;
        }

        /// <summary>
        /// يحفظ توزيع الإذن (يستبدل أسطره) فى SaveChanges واحد. allocations: رقم الفاتورة (Invoices_List.Id) ← المبلغ؛ المبالغ ≤ 0 تُتجاهل.
        /// يرمى InvalidOperationException برسالة إنجليزية قصيرة (تُترجم فى الصفحة).
        /// </summary>
        public int Save(long boxLineId, IDictionary<long, decimal> allocations)
        {
            var opp = new operations();
            var box = opp.db.Money_Transaction_Box.FirstOrDefault(m => m.Id == boxLineId && m.Account_Type == 2 && m.Transaction_Type == 1)
                      ?? throw new InvalidOperationException("ReceiptNotFound");
            var wanted = allocations.Where(a => a.Value > 0).ToDictionary(a => a.Key, a => Common.RoundMe(a.Value));
            var total = wanted.Values.Sum();
            if (total > (box.Transaction_Value ?? 0) + Tolerance) throw new InvalidOperationException("ExceedsReceipt");

            var invoiceIds = wanted.Keys.ToList();
            var invoices = opp.db.Invoices_List.Where(i => invoiceIds.Contains(i.Id)).ToDictionary(i => i.Id);
            var existingOther = opp.db.Money_Transaction_BoxForInvoices
                .Where(f => f.Money_Transaction_BoxId != boxLineId && f.Invoices_ListId != null && invoiceIds.Contains(f.Invoices_ListId.Value))
                .AsEnumerable().GroupBy(f => f.Invoices_ListId!.Value)
                .ToDictionary(g => g.Key, g => g.Sum(f => (f.Import_Value ?? 0) - (f.Export_Value ?? 0)));

            foreach (var (invoiceId, amount) in wanted)
            {
                if (!invoices.TryGetValue(invoiceId, out var inv)) throw new InvalidOperationException("InvoiceNotFound");
                if (inv.Customer_Id != box.Customer_Id) throw new InvalidOperationException("InvoiceOtherCustomer");
                var remaining = (inv.Net_Total ?? 0) - (existingOther.TryGetValue(invoiceId, out var p) ? p : 0);
                if (amount > remaining + Tolerance) throw new InvalidOperationException("ExceedsInvoice");
            }

            var old = opp.db.Money_Transaction_BoxForInvoices.Where(f => f.Money_Transaction_BoxId == boxLineId).ToList();
            if (old.Count > 0) opp.db.Money_Transaction_BoxForInvoices.RemoveRange(old);
            foreach (var (invoiceId, amount) in wanted)
            {
                opp.db.Money_Transaction_BoxForInvoices.Add(new Money_Transaction_BoxForInvoice
                {
                    Bills_ListId = 0,
                    Invoices_ListId = invoiceId,
                    Invoice_TypeId = Common.Invoice_Type_Id,
                    Money_Transaction_BoxId = boxLineId,
                    Transaction_Value = amount,
                    Transaction_Date = DateTime.Now,
                    Import_Value = amount,
                    Export_Value = 0,
                    Order_Mony_Id = box.Order_Mony_Id,
                    Customer_Id = box.Customer_Id,
                    Supplier_Id = box.Supplier_Id,
                    Order_Total_Value = box.Transaction_Value,
                    SubAccount_Id = box.SubAccount_Id,
                    Transaction_Type = box.Transaction_Type,
                    Bank_Id = box.Bank_Id,
                    Reference_Type_Table_Id = invoiceId,
                    DataDictionaryProcessesListsId = Common.Invoice_Type_Id,
                    Order_No = box.Order_No,
                });
            }
            opp.db.SaveChanges();
            return wanted.Count;
        }

        /// <summary>يزيل توزيع الإذن كله (DeleteMoney_Transaction_BoxForInvoice الأصلية) ويرجع عدد الأسطر المحذوفة.</summary>
        public int Delete(long boxLineId)
        {
            var opp = new operations();
            var old = opp.db.Money_Transaction_BoxForInvoices.Where(f => f.Money_Transaction_BoxId == boxLineId).ToList();
            if (old.Count == 0) return 0;
            opp.db.Money_Transaction_BoxForInvoices.RemoveRange(old);
            opp.db.SaveChanges();
            return old.Count;
        }
    }
}
