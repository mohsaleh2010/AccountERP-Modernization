using AccountModelsLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AccountModelsLib;
using Microsoft.EntityFrameworkCore;


// BatchSalesService.cs



namespace AccountModelsLib
{
    public class BatchSalesService
    {
        private AccountModelsLib.operations _opp;

        public BatchSalesService()
        {
            _opp = new AccountModelsLib.operations();
        }
        public BatchSalesService(  AccountModelsLib.operations  opp)
        {
            _opp = opp;
        }

        public List<Customer> GetAllCustomers()
        {
            return _opp.db.Customer.ToList();
        }

        public List<Items> GetAllItems()
        {
            return _opp.db.Item.ToList();
        }
        public long? GetBatchListIdByBatchExpireDate(long batchId)
        {
            var batch = _opp.db.Unit_Item_ExpireDates
                .FirstOrDefault(b => b.Id == batchId);

            return batch?.Unit_Item_BatchGroupListId;
        }
        public List<Unit_Item_ExpireDate> GetAvailableBatchesForItem(long itemId)
        {
            return _opp.db.Unit_Item_ExpireDates
                .Where(b => b.Item_Id == itemId && b.Larg_Balance > 0)
                .ToList();
        }
        public List<Unit_Item_SalesDate> GetTempSalesByCustomer(long customerId)
        {
            return _opp.db.Unit_Item_SalesDates
                .Where(s => s.Customer_Id == customerId && s.IsProcessedAsInvoice == 0)
                .ToList();
        }
        public List<Unit_Item_SalesDate> GetTemporarySales(long customerId)
        {
            return _opp.db.Unit_Item_SalesDates
                .Where(s => s.Customer_Id == customerId && s.IsProcessedAsInvoice == 0).Include(s=>s.Unit_Item_BatchGroupList)
                .ToList();
        }
        public bool RemoveTemporarySale(long saleId)
        {
            var sale = _opp.db.Unit_Item_SalesDates.FirstOrDefault(s => s.Id == saleId);
            if (sale == null)
                return false;  // السجل غير موجود

            if (sale.Unit_Item_ExpireDateId.HasValue && sale.Larg_Balance.HasValue)
            {
                var batch = _opp.db.Unit_Item_ExpireDates.FirstOrDefault(b => b.Id == sale.Unit_Item_ExpireDateId.Value);
                if (batch != null)
                {
                    batch.Larg_Balance += sale.Larg_Balance.Value;
                }
            }

            _opp.db.Unit_Item_SalesDates.Remove(sale);
            _opp.db.SaveChanges();

            return true;  // تم الحذف بنجاح
        }


        public void AddSales(long customerId, List<(long BatchId, decimal SellQty)> salesItems)
        {
            //if (salesItems == null || salesItems.Count == 0)
            //    throw new ArgumentException("لا يوجد أصناف للحفظ");

            //var invoice = new Invoice
            //{
            //    Customer_Id = customerId,
            //    InvoiceDate = DateTime.Now,
            //    TotalAmount = 0
            //};
            //_opp.db.Invoices.Add(invoice);
            //_opp.SaveChanges();

            //decimal total = 0;

            //foreach (var (batchId, sellQty) in salesItems)
            //{
            //    var batch = _opp.db.Unit_Item_ExpireDates.FirstOrDefault(b => b.Id == batchId);
            //    if (batch != null && batch.Larg_Balance >= sellQty)
            //    {
            //        batch.Larg_Balance -= sellQty;

            //        var detail = new InvoiceDetail
            //        {
            //            Invoice_Id = invoice.Id,
            //            Item_Id = batch.Item_Id,
            //            Quantity = sellQty,
            //            Price = batch.P_Price ?? 0,
            //            Total = sellQty * (batch.P_Price ?? 0),
            //            BatchNo = batch.BatchNo
            //        };
            //        _opp.db.InvoiceDetails.Add(detail);
            //        total += detail.Total;
            //    }
            //}

            //invoice.TotalAmount = total;
            //_opp.SaveChanges();
        }
        public bool AddTemporarySale(Unit_Item_SalesDate sale)
        {
            try
            {
                if (sale == null)
                    throw new ArgumentNullException(nameof(sale));

                // تأكد من وجود بيانات أساسية
                if (!sale.Item_Id.HasValue || !sale.Unit_Item_ExpireDateId.HasValue || !sale.Customer_Id.HasValue)
                    throw new InvalidOperationException("البيانات الأساسية غير مكتملة");

                // التحقق من وجود الدُفعة وكفاية الرصيد
                var batch = _opp.db.Unit_Item_ExpireDates.FirstOrDefault(b => b.Id == sale.Unit_Item_ExpireDateId);
                if (batch == null)
                    throw new InvalidOperationException("الباتش غير موجود");

                if (sale.Larg_Balance > batch.Larg_Balance)
                    throw new InvalidOperationException("الكمية المطلوبة أكبر من المتاحة في المخزون");

                // إنقاص الكمية من الرصيد مؤقتاً
                batch.Larg_Balance -= sale.Larg_Balance ?? 0;

                sale.QntyValue = sale.Larg_Balance * sale.P_Price;
                sale.IsProcessedAsInvoice = 0;
                sale.SalesDate = DateTime.Now;

                // إضافة السطر المؤقت
                _opp.db.Unit_Item_SalesDates.Add(sale);
                _opp.db.SaveChanges();
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }
        public bool ProcessSalesToInvoice(long customerId, int userId, int branchId)
        {
             _opp.BeginTransaction();
            try
            {
                // جلب المبيعات المؤقتة لهذا العميل والتي لم تُرحل بعد
                var tempSales = _opp.db.Unit_Item_SalesDates
                    .Where(s => s.Customer_Id == customerId && s.IsProcessedAsInvoice == 0)
                    .ToList();

                if (!tempSales.Any())
                    return false; // لا يوجد مبيعات للترحيل

                // إنشاء فاتورة جديدة
                var invoice = new Invoices_List
                {
                    Customer_Id = customerId,
                    Invoice_Date = DateTime.Now,
                    User_Id = userId,
                    Branch_Id = branchId,
                    Total = 0,
                    Editable = 1,
                    Deleted = 0,
                    OperationDate = DateTime.Now
                };

                _opp.db.Invoices_List.Add(invoice);
                _opp.db.SaveChanges();

                decimal totalInvoice = 0;

                foreach (var sale in tempSales)
                {
                    // خصم الكميات من المخزون
                    var batch = _opp.db.Unit_Item_ExpireDates.FirstOrDefault(b => b.Id == sale.Unit_Item_ExpireDateId);
                    if (batch == null || batch.Larg_Balance < sale.Larg_Balance)
                    {
                        _opp.Rollback();
                        return false; // خطأ في المخزون
                    }

                    batch.Larg_Balance -= sale.Larg_Balance ?? 0;

                    // إضافة تفاصيل الفاتورة
                    var invoiceDetail = new Invoices_Details
                    {
                        Invoice_Id = invoice.Id,
                        Item_Id = sale.Item_Id,
                        Unit_Item_ExpireDate_Id = sale.Unit_Item_ExpireDateId,
                        Larg_Qnty = sale.Larg_Balance,
                        PriceUnit = sale.P_Price,
                        Total = (sale.Larg_Balance ?? 0) * (sale.P_Price ?? 0),
                        //BatchNo = sale.BatchNo,
                        //Store_Id = sale.Store_Id,
                        OperationDate = DateTime.Now,
                        User_Id = userId,
                        Branch_Id = branchId
                    };

                    _opp.db.Invoices_Details.Add(invoiceDetail);
                    totalInvoice += invoiceDetail.Total ?? 0;

                    // تحديث السطر بأنه تم ترحيله
                    sale.IsProcessedAsInvoice = 1;
                    sale.Invoices_ListId = invoice.Id;
                    sale.Invoices_DetailsId = invoiceDetail.Id;
                }

                invoice.Total = totalInvoice;

                _opp.db.SaveChanges();
                _opp.Commit();

                return true;
            }
            catch (Exception)
            {
                _opp.Rollback();
                throw;
            }
        }
        // تحميل العملاء
        public List<Customer> GetCustomers()
        {
            return _opp.db.Customer.Where(c => c != null).ToList();
        }

        // تحميل الأصناف
        public List<Items> GetItems()
        {
            return _opp.db.Item.Where(i => i != null).ToList();
        }

        // تحميل باتشات الصنف التي تحتوي على رصيد متاح
        public List<Unit_Item_ExpireDate> GetAvailableBatches(long itemId)
        {
            return _opp.db.Unit_Item_ExpireDates
                       .Include(b => b.Supplier)
                       .Where(b => b.Item_Id == itemId && b.Larg_Balance > 0)
                       .ToList();
        }

        // إضافة بيع مؤقت جديد
        //public bool AddTemporarySale(Unit_Item_SalesDate sale)
        //{
        //    try
        //    {
        //        // تحقق من وجود رصيد كافٍ
        //        var batch = _opp.db.Unit_Item_ExpireDates.FirstOrDefault(b => b.Id == sale.Unit_Item_ExpireDateId);
        //        if (batch == null) return false;

        //        if (batch.Larg_Balance < sale.Larg_Balance || batch.Item_No < sale.Item_No)
        //            return false; // لا يوجد رصيد كافٍ

        //        // أضف البيع المؤقت
        //        sale.IsProcessedAsInvoice = 0;
        //        _opp.db.Unit_Item_SalesDates.Add(sale);
        //        _opp.db.SaveChanges();
        //        return true;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        //// حذف بيع مؤقت
        //public bool RemoveTemporarySale(long saleId)
        //{
        //    var sale = _opp.db.Unit_Item_SalesDates.Find(saleId);
        //    if (sale == null) return false;
        //    _opp.db.Unit_Item_SalesDates.Remove(sale);
        //    _opp.db.SaveChanges();
        //    return true;
        //}

        // جلب المبيعات المؤقتة غير المرحلة لعميل معين (أو كلها)
        public List<Unit_Item_SalesDate> GetTemporarySales(long? customerId = null)
        {
            var query = _opp.db.Unit_Item_SalesDates
                        .Include(s => s.Item)
                        .Include(s => s.Unit_Item_BatchGroupList)
                        .Where(s => s.IsProcessedAsInvoice == 0);

            if (customerId.HasValue)
                query = query.Where(s => s.Customer_Id == customerId);

            return query.ToList();
        }

        // ترحيل المبيعات المؤقتة لفاتورة نهائية
        //public bool ProcessSalesToInvoice(long customerId, int userId, int branchId)
        //{
        //    try
        //    {
        //        var salesToProcess = _opp.db.Unit_Item_SalesDates
        //            .Where(s => s.Customer_Id == customerId && s.IsProcessedAsInvoice == 0)
        //            .ToList();

        //        if (!salesToProcess.Any())
        //            return false;

        //        // إنشاء فاتورة جديدة
        //        var invoice = new Invoices_List
        //        {
        //            Customer_Id = customerId,
        //            Invoice_Date = DateTime.Now,
        //            User_Id = userId,
        //            Branch_Id = branchId,
        //            Total = 0,
        //            Editable = 1,
        //            Deleted = 0
        //        };

        //        _opp.db.Invoices_List.Add(invoice);
        //        _opp.db.SaveChanges();

        //        decimal totalInvoice = 0;

        //        foreach (var sale in salesToProcess)
        //        {
        //            // تحديث رصيد الباتش
        //            var batch = _opp.db.Unit_Item_ExpireDates.Find(sale.Unit_Item_ExpireDateId);
        //            if (batch == null) continue;

        //            if (batch.Larg_Balance < sale.Larg_Balance || batch.Item_No < sale.Item_No)
        //                continue; // تخطي غير كافٍ

        //            batch.Larg_Balance -= sale.Larg_Balance ?? 0;
        //            batch.Item_No -= sale.Item_No ?? 0;

        //            // إنشاء تفاصيل الفاتورة
        //            var detail = new Invoices_Details
        //            {
        //                Invoice_Id = invoice.Id,
        //                Item_Id = sale.Item_Id,
        //                Larg_Qnty = sale.Larg_Balance,
        //                Small_Qnty = sale.Item_No,
        //                PriceUnit = sale.P_Price,
        //                Total = (sale.Larg_Balance ?? 0) * (sale.P_Price ?? 0),
        //                Unit_Item_ExpireDate_Id = sale.Unit_Item_ExpireDateId
        //            };

        //            _opp.db.Invoices_Details.Add(detail);

        //            totalInvoice += detail.Total ?? 0;

        //            // تعليم البيع كمُعالج
        //            sale.IsProcessedAsInvoice = 1;
        //            sale.Invoices_ListId = invoice.Id;
        //            sale.Invoices_DetailsId = detail.Id;
        //        }

        //        invoice.Total = totalInvoice;
        //        _opp.db.SaveChanges();

        //        return true;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}
    }
}


