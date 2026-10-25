// DailyItemSalesService.cs
using AccountModelsLib.Models;
using AccountModelsLib;
using System;
using System.Collections.Generic;
using System.Linq;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;


namespace AccountModelsLib
{
    public class DailyItemSalesService
    {
        //private operations _opp;

        //public DailyItemSalesService()
        //{
        //    _opp = new operations();
        //}

        private readonly operations _opp;

        public DailyItemSalesService()
        {
            _opp = new operations();
        }
        public DailyItemSalesService(ref operations opp)
        {
            _opp = opp;
        }

        public List<Supplier> GetSuppliers() => _opp.db.Suppliers.ToList();
        public List<Customer> GetCustomers() => _opp.db.Customer.ToList();
        public List<Items> GetItems() => _opp.db.Item.ToList();

        public List<Unit_Item_ExpireDate> GetBatchesBySupplier(long supplierId)
        {
            var unit_Item_ExpireDates = _opp.db.Unit_Item_ExpireDates
                .Where(b => b.Supplier_Id == supplierId)
                .ToList();

            return unit_Item_ExpireDates;
        }
        public Unit_Item_ExpireDate GetBatchesById(long Id)
        {
            var unit_Item_ExpireDates = _opp.db.Unit_Item_ExpireDates
                .Where(b => b.Id == Id)
                .FirstOrDefault();

            return unit_Item_ExpireDates;
        }
        public void DeleteAllSalesItem(ref int Excp, ref string Msg, long id)
        {
            try
            {

                var entitySales = _opp.db.Unit_Item_SalesDates.AsNoTracking().Where(i => i.Unit_Item_Sales_CustomerListId == id & i.IsProcessedAsInvoice == 1).FirstOrDefault();
                if (entitySales != null)
                {
                    Excp = 1;
                    Msg = "لا يمكن الحذف نظرا لوجود عمليات بيع وتمت الفاتورة";
                    return;
                }
                var entity = _opp.db.Unit_Item_SalesDates.Where(i => i.Unit_Item_Sales_CustomerListId == id);
                if (entity != null)
                {

                    //long Unit_Item_BatchListId = entity.Unit_Item_BatchListId ?? 0;
                    _opp.db.Unit_Item_SalesDates.RemoveRange(entity);

                    Excp = 0;
                    Msg = "تم   حذف الصنف";
                    _opp.db.SaveChanges();
                    //////////////////
                    ///

                    var unit_Item_Sales_CustomerList = _opp.db.Unit_Item_Sales_CustomerLists.Where(i => i.Id == id).FirstOrDefault();
                    _opp.db.Unit_Item_Sales_CustomerLists.Remove(unit_Item_Sales_CustomerList);

                    _opp.db.SaveChanges();
                    return;
                }
            }
            catch (Exception ex)
            {
                Excp = 1;
                Msg = "خطأ فى حذف الصنف";
                Msg = Msg + "\n\r";
                Msg = Msg + ex.Message;
                // يمكنك تسجيل الخطأ
            }


        }

        public void SetSales_Lists(Unit_Item_SalesDate sale)
        {
            DateTime dat = sale.SalesDate.Value.Date;
            // البحث عن SalesList موجود لنفس العميل والتاريخ
            //var Sales_CustomerLists = _opp.db.Unit_Item_Sales_CustomerLists.AsNoTracking()
            //        .FirstOrDefault(x => x.Customer_Id == sale.Customer_Id && x.SalesDate != null && DbFunctions.TruncateTime(x.SalesDate) == dat.Date);
            var Sales_CustomerLists = _opp.db.Unit_Item_Sales_CustomerLists.AsNoTracking()
               .FirstOrDefault(x => x.Customer_Id == sale.Customer_Id && x.SalesDate != null &&  x.SalesDate.Date == dat.Date);

            if (Sales_CustomerLists == null)
            {
                Sales_CustomerLists = new Unit_Item_Sales_CustomerList
                {
                    Customer_Id = sale.Customer_Id,
                    SalesDate = sale.SalesDate.Value
                };
                _opp.db.Unit_Item_Sales_CustomerLists.Add(Sales_CustomerLists);
                _opp.db.SaveChanges();
            }



            // البحث عن SalesList موجود لنفس الصنف والتاريخ
            //var Sales_ItemsLists = _opp.db.Unit_Item_Sales_ItemLists.AsNoTracking()
            //        .FirstOrDefault(x => x.Item_Id == sale.Item_Id && DbFunctions.TruncateTime(x.SalesDate) == dat);
            var Sales_ItemsLists = _opp.db.Unit_Item_Sales_ItemLists.AsNoTracking()
         .FirstOrDefault(x => x.Item_Id == sale.Item_Id &&  x.SalesDate.Date == dat);

            if (Sales_ItemsLists == null)
            {
                Sales_ItemsLists = new Unit_Item_Sales_ItemList
                {
                    Item_Id = sale.Item_Id,
                    Unit_Item_Table_Id = sale.Unit_Item_Table_Id,
                    SalesDate = sale.SalesDate.Value,
                    ItemSourceType = sale.ItemSourceType,
                    Unit_Item_ExpireDateId = sale.Unit_Item_ExpireDateId,
                    ItemNameSource = sale.ItemNameSource,
                };
                _opp.db.Unit_Item_Sales_ItemLists.Add(Sales_ItemsLists);
                _opp.db.SaveChanges();
            }


            sale.Unit_Item_Sales_CustomerListId = Sales_CustomerLists.Id;
            sale.Unit_Item_Sales_ItemListId = Sales_ItemsLists.Id;

        }
        public bool TryAddSale(ref long Unit_Item_Sales_ItemListId, int ItemSourceType, long ItemsourceId, Unit_Item_SalesDate salen)
        {
            decimal totalQty = salen.Larg_Balance ?? 0;

            SetSales_Lists(salen);
            //int ItemSourceType = (int)(ItemSourceTypeForSales)cmbItemSource.SelectedValue;
            //if (ItemSourceType == (int)ItemSourceTypeForSales.FromBatch) // من باتش وارد
            //    string sourceType = cmbItemSource.SelectedItem.ToString();
            if (ItemSourceType == (int)ItemSourceTypeForSales.FromBatch) // من باتش وارد
            {
                var batch = _opp.db.Unit_Item_ExpireDates.FirstOrDefault(b => b.Id == ItemsourceId);
                if (batch == null || (batch.Larg_Balance.HasValue && batch.Larg_Balance.Value < totalQty && batch.Larg_Balance > 0))
                {
                    return false;
                }

                if (batch.Larg_Balance.HasValue && batch.Larg_Balance > 0)
                {
                    batch.Larg_Balance -= totalQty;
                }
            }
            else
            {
                //var item = _opp.db.Unit_item.FirstOrDefault(u => u.Item_Id == ItemsourceId);
                //if (item == null || (item.Full_qua.HasValue && item.Full_qua.Value < totalQty))
                //{
                //    return false;
                //}
                //item.Full_qua -= totalQty;
            }
            salen.QntyValue = totalQty * salen.P_Price;
            if (salen.Id == 0)
            {
                _opp.db.Unit_Item_SalesDates.Add(salen);
            }
            else
            {
                //var unit_Item_SalesDate = _opp.db.Unit_Item_SalesDates.Find(salen.Id);
                //if (unit_Item_SalesDate != null)
                //{
                //    _opp.db.Entry(unit_Item_SalesDate).CurrentValues.SetValues(salen);

                //}

                _opp.db.Entry(salen).State = EntityState.Modified;
            }
            _opp.db.SaveChanges();
            Unit_Item_Sales_ItemListId = salen.Unit_Item_Sales_ItemListId ?? 0;
            return true;
        }
        public bool TryAddSaleForAll(ref long Unit_Item_Sales_ItemListId, Unit_Item_SalesDate salen)
        {
            int ItemSourceType = salen.ItemSourceType ?? 0;
            long ItemsourceId = salen.ItemSourceId ?? 0;
            decimal totalQty = salen.Larg_Balance ?? 0;

            SetSales_Lists(salen);
            //int ItemSourceType = (int)(ItemSourceTypeForSales)cmbItemSource.SelectedValue;
            //if (ItemSourceType == (int)ItemSourceTypeForSales.FromBatch) // من باتش وارد
            //    string sourceType = cmbItemSource.SelectedItem.ToString();
            if (ItemSourceType == (int)ItemSourceTypeForSales.FromBatch) // من باتش وارد
            {
                var batch = _opp.db.Unit_Item_ExpireDates.FirstOrDefault(b => b.Id == ItemsourceId);
                if (batch == null || (batch.Larg_Balance.HasValue && batch.Larg_Balance.Value < totalQty && batch.Larg_Balance > 0))
                {
                    return false;
                }

                if (batch.Larg_Balance.HasValue && batch.Larg_Balance > 0)
                {
                    batch.Larg_Balance -= totalQty;
                }
            }
            else
            {
                //var item = _opp.db.Unit_item.FirstOrDefault(u => u.Item_Id == ItemsourceId);
                //if (item == null || (item.Full_qua.HasValue && item.Full_qua.Value < totalQty))
                //{
                //    return false;
                //}
                //item.Full_qua -= totalQty;
            }
            salen.QntyValue = totalQty * salen.P_Price;
            if (salen.Id == 0)
            {
                _opp.db.Unit_Item_SalesDates.Add(salen);
            }
            else
            {
                //var unit_Item_SalesDate = _opp.db.Unit_Item_SalesDates.Find(salen.Id);
                //if (unit_Item_SalesDate != null)
                //{
                //    _opp.db.Entry(unit_Item_SalesDate).CurrentValues.SetValues(salen);

                //}

                _opp.db.Entry(salen).State = EntityState.Modified;
            }
            _opp.db.SaveChanges();
            Unit_Item_Sales_ItemListId = salen.Unit_Item_Sales_ItemListId ?? 0;
            return true;
        }

        public long AddSale(Unit_Item_SalesDate sale)
        {

            _opp.db.Unit_Item_SalesDates.Add(sale);
            _opp.db.SaveChanges();
            return sale.Id;

        }

        public List<Unit_Item_SalesDate> GetUnit_Item_SalesDate(int IsProcessedAsBill, DateTime SaleDate, int ItemSourceType, long ItemSourceId)
        {
            List<Unit_Item_SalesDate> SalesDateLst = new List<Unit_Item_SalesDate>();

            var query = _opp.db.Unit_Item_SalesDates.Where(i => i.ItemSourceType == ItemSourceType & i.ItemSourceId == ItemSourceId);
            //if (ItemSourceType == 1)
            //{
            //    query = query.Where(i => DbFunctions.TruncateTime(i.SalesDate) == SaleDate.Date);
            //}
            if (ItemSourceType == 1)
            {
                query = query.Where(i => i.SalesDate.Value.Date == SaleDate.Date);
            }
            if (IsProcessedAsBill > -1)
            {
                query = query.Where(i => i.IsProcessedAsBill == IsProcessedAsBill);
            }
            SalesDateLst = query.ToList();
            return SalesDateLst;
        }
        public List<Unit_Item_SalesDate> GetUnit_Item_SalesDateForAll(int IsProcessedAsBill, DateTime SaleDate, int ItemSourceType, long ItemSourceId, long Unit_Item_BatchGroupListId)
        {
            List<Unit_Item_SalesDate> SalesDateLst = new List<Unit_Item_SalesDate>();

            var query = _opp.db.Unit_Item_SalesDates.Where(i => i.ItemSourceType == ItemSourceType);
            //if (ItemSourceType == 1)
            //{
            //    query = query.Where(i => DbFunctions.TruncateTime(i.SalesDate) == SaleDate.Date);
            //}
            if (ItemSourceType == 1)
            {
                query = query.Where(i =>  i.SalesDate.Value.Date == SaleDate.Date);
            }
            if (ItemSourceId > 0)
            {
                query = query.Where(i => i.ItemSourceId == ItemSourceId);
            }
            if (Unit_Item_BatchGroupListId > 0)
            {
                query = query.Where(i => i.Unit_Item_BatchGroupListId == Unit_Item_BatchGroupListId);

            }
            if (IsProcessedAsBill > -1)
            {
                query = query.Where(i => i.IsProcessedAsBill == IsProcessedAsBill);
            }
            SalesDateLst = query.ToList();
            return SalesDateLst;
        }

        public List<Unit_Item_SalesDate> GetUnit_Item_CustomerSalesDate(int IsProcessedAsInvoice, bool _dtFrom, DateTime SaleDateFrom, bool _dtTo, DateTime SaleDateTo, long Customer_Id)
        {
            List<Unit_Item_SalesDate> SalesDateLst = new List<Unit_Item_SalesDate>();
            //Where(i => DbFunctions.TruncateTime(i.SalesDate) >= SaleDateFrom.Date & DbFunctions.TruncateTime(i.SalesDate) <= SaleDateTo.Date & i.Customer_Id == Customer_Id)
            var query = _opp.db.Unit_Item_SalesDates.AsQueryable();
            //if (_dtFrom == true)
            //{
            //    query = query.Where(i => DbFunctions.TruncateTime(i.SalesDate) >= SaleDateFrom.Date);
            //}
            //if (_dtTo == true)
            //{
            //    query = query.Where(i => DbFunctions.TruncateTime(i.SalesDate) <= SaleDateTo.Date);
            //}

            if (_dtFrom == true)
            {
                query = query.Where(i =>  i.SalesDate.Value.Date >= SaleDateFrom.Date);
            }
            if (_dtTo == true)
            {
                query = query.Where(i =>  i.SalesDate.Value.Date<= SaleDateTo.Date);
            }

            if (Customer_Id > 0)
            {
                query = query.Where(i => i.Customer_Id == Customer_Id);
            }
            if (IsProcessedAsInvoice > -1)
            {
                query = query.Where(i => i.IsProcessedAsInvoice == IsProcessedAsInvoice);
            }
            SalesDateLst = query.ToList();
            return SalesDateLst;
        }

        public List<Unit_Item_SalesDate> GetUnit_Item_SalesDatesByInvoiceId(long Invoice_List_Table_Id)
        {
            List<Unit_Item_SalesDate> SalesDateLst = new List<Unit_Item_SalesDate>();
            var query = _opp.db.Unit_Item_SalesDates.Include(i=>i.Invoices_List).AsQueryable();

            if (Invoice_List_Table_Id > 0)
            {
                query = query.Where(i => i.Invoices_ListId == Invoice_List_Table_Id);
            }

            SalesDateLst = query.ToList();
            return SalesDateLst;
        }
        public List<Unit_Item_SalesDateVM> GetUnit_Item_CustomerSalesDate(int ItemSourceType, long ItemSourceId, int IsProcessedAsInvoice, bool _DateFrom, DateTime SaleDateFrom, bool _DateTo, DateTime SaleDateTo, long Customer_Id)
        {

            List<Unit_Item_SalesDate> SalesDateLst = new List<Unit_Item_SalesDate>();

            var query = _opp.db.Unit_Item_SalesDates.Include(c => c.Customer).AsNoTracking().Where(i => i.Id > 0);
            if (_DateFrom == true)
            {
                query = query.Where(i =>  i.SalesDate.Value.Date >= SaleDateFrom.Date);
            }
            if (_DateTo == true)
            {
                query = query.Where(i =>  i.SalesDate.Value.Date<= SaleDateTo.Date);
            }
            if (IsProcessedAsInvoice > -1)
            {
                query = query.Where(i => i.IsProcessedAsInvoice == IsProcessedAsInvoice);
            }
            if (Customer_Id > 0)
            {
                query = query.Where(i => i.Customer_Id == Customer_Id);
            }
            if (ItemSourceId > 0)
            {
                query = query.Where(i => i.ItemSourceId == ItemSourceId);
                query = query.Where(i => i.ItemSourceType == ItemSourceType);
            }
            SalesDateLst = query.ToList();

            //List<Unit_Item_SalesDateVM> SalesDateLstVM = GetUnit_Item_SalesDateVM(query);
            List<Unit_Item_SalesDateVM> SalesDateLstVM = GetUnit_Item_SalesDateVM(SalesDateLst);
            return SalesDateLstVM;
        }

        public List<Unit_Item_SalesDateVM> GetUnit_Item_SalesDateVM(List<Unit_Item_SalesDate> query)
        {
            return query.Select((i, index) => new Unit_Item_SalesDateVM
            {
                Serial_ = index + 1, // عداد يبدأ من 1
                Id = i.Id,
                Customer_Id = i.Customer_Id,
                Item_Id = i.Item_Id,
                Larg_Balance = i.Larg_Balance,
                Item_No = i.Item_No,
                P_Price = i.P_Price,
                SalesDate = i.SalesDate,
                Branch_Id = i.Branch_Id,
                User_Id = i.User_Id,
                Unit_Item_ExpireDateId = i.Unit_Item_ExpireDateId,
                Unit_Item_BatchGroupListId = i.Unit_Item_BatchGroupListId,
                QntyValue = i.QntyValue,
                IsProcessedAsInvoice = i.IsProcessedAsInvoice,
                CustomerName = i.Customer?.Name ?? "غير متاح",
                Item_Name = i.Item_Name,
                ItemName_OnInvoice = i.ItemName_OnInvoice,
                ItemNameSource = i.ItemNameSource,


                SetCustomerAccountStr = i.IsProcessedAsInvoice == 0 ? " " : "تم",
                SetSupplierAccountStr = i.IsProcessedAsBill == 0 ? " " : "تم",
                IsProcessedAsInvoiceStr = i.IsProcessedAsInvoice == 0 ? " " : "تم",
                IsProcessedAsBillStr = i.IsProcessedAsBill == 0 ? " " : "تم",
                Invoices_DetailsId = i.Invoices_DetailsId ?? 0,
                Invoices_ListId = i.Invoices_ListId ?? 0,
                Bills_DetailsId = i.Bills_DetailsId ??0,
                Bills_ListId = i.Bills_ListId??0 ,


               Unit3Mola= i.Unit3Mola??0,
                Total3Mola = i.Total3Mola ?? 0,
                Total= i.Total ?? 0,
                Unit_Item_Sales_ItemListId = i.Unit_Item_Sales_ItemListId ?? 0,
                Unit_Item_Sales_CustomerListId = i.Unit_Item_Sales_CustomerListId ?? 0,
                ItemSourceType = i.ItemSourceType ?? 0,
                ItemSourceId = i.ItemSourceId ?? 0,
                ItemSourceTypeName = i.ItemSourceType == 1 ? "من باتش وارد" : "من مخزن",

            }).ToList();
        }
        private List<Unit_Item_SalesDateVM> GetUnit_Item_SalesDateVM__(IQueryable<Unit_Item_SalesDate> query)
        {
            return query   // لتحويل الاستعلام إلى LINQ to Objects
                .Select(i => GetUnit_Item_SalesDateVM_One(i))
                .ToList();
            //return query.Select(i => new GetUnit_Item_SalesDateVM_One(i)).ToList();
        }



        private Unit_Item_SalesDateVM GetUnit_Item_SalesDateVM_One(Unit_Item_SalesDate i)
        {
            return new Unit_Item_SalesDateVM
            {
                Id = i.Id,
                Customer_Id = i.Customer_Id,
                Item_Id = i.Item_Id,
                Larg_Balance = i.Larg_Balance,
                Item_No = i.Item_No,
                P_Price = i.P_Price,
                SalesDate = i.SalesDate,
                Branch_Id = i.Branch_Id,
                User_Id = i.User_Id,
                Unit_Item_ExpireDateId = i.Unit_Item_ExpireDateId,
                Unit_Item_BatchGroupListId = i.Unit_Item_BatchGroupListId,
                QntyValue = i.QntyValue,
                IsProcessedAsInvoice = i.IsProcessedAsInvoice,
                CustomerName = i.Customer.Name,
                Item_Name = i.Item_Name,
                ItemName_OnInvoice = i.ItemName_OnInvoice,
                ItemNameSource = i.ItemNameSource,
                //SetCustomerAccountStr = i.SetCustomerAccountStr,
                //SetSupplierAccountStr = i.SetSupplierAccountStr,
                //IsProcessedAsInvoiceStr = i.IsProcessedAsInvoiceStr,
                //IsProcessedAsBillStr = i.IsProcessedAsBillStr

            };
        }
        public List<Unit_Item_SalesDate> GetUnit_Item_SalesDate(long Unit_Item_Sales_ItemListId, int IsProcessedAsInvoice)
        {
            List<Unit_Item_SalesDate> SalesDateLst = new List<Unit_Item_SalesDate>();

            SalesDateLst = _opp.db.Unit_Item_SalesDates.Where(i => i.Unit_Item_Sales_ItemListId == Unit_Item_Sales_ItemListId & i.IsProcessedAsInvoice == IsProcessedAsInvoice).ToList();
            return SalesDateLst;
        }

        public List<Supplier> GetAllSuppliers()
        {
            return _opp.db.Suppliers.ToList();
        }

        public List<Customer> GetAllCustomers()
        {
            return _opp.db.Customer.ToList();
        }

        public List<Items> GetAllItems()
        {
            return _opp.db.Item.ToList();
        }

        public List<Unit_Item_ExpireDate> GetSupplierBatches(long supplierId)
        {
            return _opp.db.Unit_Item_ExpireDates
                .Where(b => b.Supplier_Id == supplierId && (b.Larg_Balance ?? 0) > 0)
                .ToList();
        }

        public Unit_item GetStoreBalance(long itemId, long storeId)
        {
            return _opp.db.Unit_item.FirstOrDefault(u => u.Item_Id == itemId && u.Store_id == storeId);
        }

        public bool CanSellFromBatch(long batchId, decimal qtyKg, decimal qtyCount)
        {
            var batch = _opp.db.Unit_Item_ExpireDates.FirstOrDefault(b => b.Id == batchId);
            if (batch == null) return false;

            if ((batch.Larg_Balance ?? 0) == 0)
                return true; // السماح بالبيع دون حد عند نفاد الرصيد

            decimal available = batch.Larg_Balance ?? 0;
            decimal requested = qtyKg + qtyCount;

            return requested <= available;
        }

        public bool CanSellFromStore(long itemId, decimal qtyKg, decimal qtyCount)
        {
            var storeItem = _opp.db.Unit_item.FirstOrDefault(s => s.Item_Id == itemId);
            if (storeItem == null) return false;

            decimal available = (storeItem.Full_qua ?? 0) + (storeItem.Item_No ?? 0);
            decimal requested = qtyKg + qtyCount;

            return requested <= available;
        }

        public void SaveSaleRow(DailyItemSaleRow saleRow)
        {
            var sale = new Unit_Item_SalesDate
            {
                Customer_Id = saleRow.CustomerId,
                Item_Id = saleRow.ItemId,
                Larg_Balance = saleRow.QtyKg,
                Item_No = saleRow.QtyCount,
                P_Price = saleRow.UnitPrice,
                SalesDate = saleRow.SaleDate,
                Branch_Id = saleRow.BranchId,
                User_Id = saleRow.UserId,
                Unit_Item_ExpireDateId = saleRow.BatchId,
                Unit_Item_BatchGroupListId = saleRow.BatchListId,
                QntyValue = saleRow.QtyKg + saleRow.QtyCount,
                IsProcessedAsInvoice = 0
            };

            // خصم الكمية من الباتش أو المخزن
            if (saleRow.BatchId.HasValue)
            {
                var batch = _opp.db.Unit_Item_ExpireDates.FirstOrDefault(b => b.Id == saleRow.BatchId);
                if (batch != null && batch.Larg_Balance > 0)
                {
                    batch.Larg_Balance -= saleRow.QtyKg + saleRow.QtyCount;
                }
            }
            else
            {
                var store = _opp.db.Unit_item.FirstOrDefault(s => s.Item_Id == saleRow.ItemId);
                if (store != null)
                {
                    store.Full_qua -= saleRow.QtyKg;
                    store.Item_No -= saleRow.QtyCount;
                }
            }

            _opp.db.Unit_Item_SalesDates.Add(sale);
            _opp.db.SaveChanges();
        }




        public List<Unit_Item_ExpireDate> GetBatchesBySupplier1(long supplierId)
        {
            return _opp.db.Unit_Item_ExpireDates
                .Where(b => b.Supplier_Id == supplierId && b.Larg_Balance > 0)
                .ToList();
        }


        public bool TryAddSale1(string sourceType, long sourceId, long customerId, decimal qtyKg, decimal qtyCount, decimal unitPrice, DateTime saleDate)
        {
            if (sourceType == "من باتش وارد")
            {
                var batch = _opp.db.Unit_Item_ExpireDates.FirstOrDefault(b => b.Id == sourceId);
                if (batch == null) return false;

                if (batch.Larg_Balance == 0 || (qtyKg + qtyCount) <= batch.Larg_Balance)
                {
                    var sale = new Unit_Item_SalesDate
                    {
                        Customer_Id = customerId,
                        Item_Id = batch.Item_Id,
                        Unit_Item_ExpireDateId = batch.Id,
                        Larg_Balance = qtyKg,
                        Item_No = qtyCount,
                        P_Price = unitPrice,
                        SalesDate = saleDate,
                        IsProcessedAsInvoice = 0,
                        User_Id = 1,
                        Branch_Id = 1,
                        QntyValue = qtyKg + qtyCount,
                        Unit_Item_BatchGroupListId = batch.Unit_Item_BatchGroupListId
                    };

                    _opp.db.Unit_Item_SalesDates.Add(sale);
                    batch.Larg_Balance -= (qtyKg + qtyCount);
                    _opp.db.SaveChanges();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else // من قائمة الأصناف
            {
                var stock = _opp.db.Unit_item.FirstOrDefault(u => u.Item_Id == sourceId);
                if (stock == null || stock.Full_qua < (qtyKg + qtyCount)) return false;

                var sale = new Unit_Item_SalesDate
                {
                    Customer_Id = customerId,
                    Item_Id = sourceId,
                    Larg_Balance = qtyKg,
                    Item_No = qtyCount,
                    P_Price = unitPrice,
                    SalesDate = saleDate,
                    IsProcessedAsInvoice = 0,
                    User_Id = 1,
                    Branch_Id = 1,
                    QntyValue = qtyKg + qtyCount
                };

                _opp.db.Unit_Item_SalesDates.Add(sale);
                stock.Full_qua -= (qtyKg + qtyCount);
                _opp.db.SaveChanges();
                return true;
            }
        }


        public bool UpdateSale(ref int Excp, ref string Msg, Unit_Item_SalesDate sale)
        {
            try
            {
                var existing = _opp.db.Unit_Item_SalesDates.Find(sale.Id);
                //if (existing == null) return false;

                if (existing == null)
                {
                    Excp = 1;
                    Msg = "الصنف غير موجود";
                    return false;
                }
                else
                {
                    if (existing.IsProcessedAsInvoice == 1)
                    {
                        Excp = 2;
                        Msg = "لا يمكن تعديل الصنف تم إضافتة فى فاتورة البيع";
                        return false;
                    }
                }


                _opp.db.Entry(existing).CurrentValues.SetValues(sale);
                existing.Edit_User_Id = Common.User_Id; // تدقيق التعديل
                //context.SaveChanges();


                //_opp.db.Entry(sale).State = EntityState.Modified;
                _opp.db.SaveChanges();
                Excp = 0;
                Msg = "تم تعديل الصنف";
                return true;
            }
            catch (Exception ee)
            {
                Excp = 10;
                Msg = "فشل في تعديل الصنف";
                return false;
            }

        }

        public bool DeleteSale(ref int Excp, ref string Msg, long saleId)
        {
            //var sale = _opp.db.Unit_Item_SalesDates.AsNoTracking().Where(i => i.Id == saleId).FirstOrDefault();
            try
            {
                var sale = _opp.db.Unit_Item_SalesDates.Find(saleId);
                if (sale == null)
                {
                    Excp = 1;
                    Msg = "الصنف غير موجود";
                    return false;
                }
                else
                {
                    if (sale.IsProcessedAsInvoice == 1)
                    {
                        Excp = 2;
                        Msg = "لا يمكن حذف الصنف تم إضافتة فى فاتورة البيع";
                        return false;
                    }
                    if (sale.IsProcessedAsBill == 1)
                    {
                        Excp = 2;
                        Msg = "لا يمكن حذف الصنف تم إضافتة فى فاتورة المشتريات للمورد";
                        return false;
                    }
                }
                _opp.db.Unit_Item_SalesDates.Remove(sale);
                _opp.db.SaveChanges();
                Excp = 0;
                Msg = "تم حذف الصف من قاعدة البيانات";
                return true;
            }
            catch (Exception ee)
            {
                Msg = "فشل في حذف الصف من قاعدة البيانات";
                Excp = 10;
                return false;
            }
        }

    }

    public class DailyItemSaleRow
    {
        public long? BatchId { get; set; }
        public long? BatchListId { get; set; }
        public long ItemId { get; set; }
        public long CustomerId { get; set; }
        public decimal QtyKg { get; set; }
        public decimal QtyCount { get; set; }
        public decimal UnitPrice { get; set; }
        public DateTime SaleDate { get; set; }
        public int UserId { get; set; }
        public int BranchId { get; set; }
    }
}

