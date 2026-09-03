using AccountModelsLib.Models;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;


namespace AccountModelsLib 
{
    public class BatchReceiptService
{
    private readonly AccountModelsLib.operations _opp;
    public Unit_Item_ExpireDateVM unit_Item_ExpireDateVM { get; set; }
    public BatchReceiptService(AccountModelsLib.operations opp)
    {
        _opp = opp;
    }

    /// <summary>
    /// الحصول على مجموعة دفعات مفتوحة لمورد في تاريخ اليوم
    /// </summary>
    public Unit_Item_BatchGroupList GetOpenBatchGroup(long supplierId, DateTime dtDay)
    {
            //return _opp.db.Unit_Item_BatchGroupLists
            //    .FirstOrDefault(g => g.Supplier_Id == supplierId
            //                      && DbFunctions.TruncateTime(g.ReceiptDate) == dtDay.Date
            //                       );

            return _opp.db.Unit_Item_BatchGroupLists
    .FirstOrDefault(g => g.Supplier_Id == supplierId
                    && g.ReceiptDate != null &&  g.ReceiptDate.Value.Date == dtDay.Date
                       );
        }
    public List<Unit_Item_BatchGroupList> GetBatchGroupLst(long supplierId, DateTime dtDay)
    {
        return _opp.db.Unit_Item_BatchGroupLists.Where(g => g.Supplier_Id == supplierId
                              && g.ReceiptDate != null && g.ReceiptDate.Value.Date == dtDay.Date)
            .ToList();
    }
    public Unit_Item_BatchGroupList GetBatchGroupById(long batchId)
    {
        return _opp.db.Unit_Item_BatchGroupLists.AsNoTracking()
            .Where(b => b.Id == batchId)

            .FirstOrDefault();

    }

    /// <summary>
    /// إنشاء مجموعة دفعات جديدة لمورد
    /// </summary>
    public Unit_Item_BatchGroupList CreateNewBatchGroup(long supplierId, DateTime dtDay)
    {
        
        var newGroup = new Unit_Item_BatchGroupList
        {
            BatchGroupId = Guid.NewGuid(),
            Supplier_Id = supplierId,
            GroupName = $"دفعة :  المجموعة : {dtDay:HH:mm}",
            ReceiptDate = dtDay,
            User_Id = Common.User_Id,
            Branch_Id = Common.Branch_Id,
            CreatedDate = dtDay,
            WahbaValue = 0,
            NawlounValue = 0,
            Total3MolaRate = 0,
            Total3Mola = 0,
            Value_3Mola = 0
        };

        _opp.db.Unit_Item_BatchGroupLists.Add(newGroup);
        _opp.db.SaveChanges();
        return newGroup;
    }

    /// <summary>
    /// إضافة باتشات إلى مجموعة محددة
    /// </summary>
    public void AddBatches(long batchGroupId, List<Unit_Item_ExpireDate> batches)
    {
        foreach (var batch in batches)
        {
            batch.Unit_Item_BatchGroupListId = batchGroupId;
            _opp.db.Unit_Item_ExpireDates.Add(batch);
        }
        _opp.db.SaveChanges();
    }

    /// <summary>
    /// تحديث القيم المالية لمجموعة
    /// </summary>
    public void UpdateFinancials(long batchGroupId, decimal wahba, decimal nawloun, decimal total3MolaRate)
    {
        var group = _opp.db.Unit_Item_BatchGroupLists.Find(batchGroupId);
        if (group != null)
        {
            group.WahbaValue = wahba;
            group.NawlounValue = nawloun;
            group.Total3MolaRate = total3MolaRate;

            // حساب إجمالي العُمولة
            var totalQuantity = _opp.db.Unit_Item_ExpireDates
                .Where(x => x.Unit_Item_BatchGroupListId == batchGroupId)
                .Sum(x => x.QntyValue);

            group.Total3Mola = totalQuantity * total3MolaRate;
            _opp.db.SaveChanges();
        }
    }
    public void SaveOrUpdateBatchSingle(long itemId, long supplierId, DateTime receiptDate, decimal qtyKg, decimal price, decimal itemNo)
    {
        var newBatch = new Unit_Item_ExpireDate
        {
            Item_Id = itemId,
            Supplier_Id = supplierId,
            Larg_Balance = qtyKg,
            Item_No = itemNo,
            P_Price = price,
            QntyValue = qtyKg * price,
            BatchNo = GetNextBatchNo(),
        };

        _opp.db.Unit_Item_ExpireDates.Add(newBatch);
        _opp.db.SaveChanges();
    }
    public void SaveOrUpdateBatchList(long supplierId, DateTime receiptDate, long storeId, List<Unit_Item_ExpireDate> batchItems, Unit_Item_BatchGroupList batchList)
    {
        if (batchList == null)
        {
            batchList = CreateNewBatchGroup(supplierId, receiptDate);
        }
        // البحث عن BatchList موجود لنفس المورد والتاريخ


        foreach (var item in batchItems)
        {
            var existing = _opp.db.Unit_Item_ExpireDates
                .FirstOrDefault(x => x.Item_Id == item.Item_Id && x.Unit_Item_BatchGroupListId == batchList.Id);


            //existing.Unit_Item_Table_Id =itemCLS.GetUnit_itemIdForDefaultStore(item.Item_Id ?? 0);

            if (existing != null)
            {
                item.Id = existing.Id;
                existing.Larg_Balance = existing.Larg_Balance ?? 0;
                existing.Small_Balance = existing.Small_Balance ?? 0;
                existing.Item_No = existing.Item_No ?? 0;
                existing.P_Price = existing.P_Price ?? 0;
                existing.Unit_Item_BatchGroupListId = batchList.Id;
                existing.Larg_Balance = item.Larg_Balance;
                existing.Item_No = item.Item_No;
                existing.P_Price = item.P_Price;
                existing.QntyValue = item.Larg_Balance * item.P_Price;
                existing.Tash3ela = $"استلام بتاريخ {receiptDate:yyyy-MM-dd}";
                existing.User_Id = Common.User_Id;
                existing.Branch_Id = Common.Branch_Id;
            }
            else
            {
                var newBatch = new Unit_Item_ExpireDate
                {
                    Item_Id = item.Item_Id,
                    Unit_Item_Table_Id = item.Unit_Item_Table_Id,
                    Supplier_Id = batchList.Supplier_Id,
                    Larg_Balance = item.Larg_Balance,
                    Item_No = item.Item_No,
                    P_Price = item.P_Price,
                    QntyValue = item.Larg_Balance * item.P_Price,
                    BatchNo = GetNextBatchNo(),

                    Tash3ela = $"استلام بتاريخ {receiptDate:yyyy-MM-dd}",
                    Unit_Item_BatchGroupListId = batchList.Id,
                    OperationDate = batchList.ReceiptDate,

                    User_Id = Common.User_Id,
                    Branch_Id = Common.Branch_Id
                };
                _opp.db.Unit_Item_ExpireDates.Add(newBatch);
                item.Id = newBatch.Id;
            }
        }

        _opp.db.SaveChanges();
    }

    public void SaveOrUpdateBatchList(long supplierId, DateTime receiptDate, long storeId, Unit_Item_ExpireDate batchItem, Unit_Item_BatchGroupList currentBatchGroup)
    {
        try
        {
            if (currentBatchGroup == null)
            {
                currentBatchGroup = CreateNewBatchGroup(supplierId, receiptDate);
            }
            Unit_Item_ExpireDate existing = null;
            long Unit_Item_ExpireDateId = 0;
            if (batchItem.Id > 0)
            {
                existing = _opp.db.Unit_Item_ExpireDates.Where(x => x.Id == batchItem.Id)
                    .FirstOrDefault(); 
             
            }
            //else
            //{
            //    existing = _opp.db.Unit_Item_ExpireDates
            //        .FirstOrDefault(x => x.Item_Id == batchItem.Item_Id && x.Supplier_Id == batchItem.Supplier_Id && DbFunctions.TruncateTime(x.OperationDate) == receiptDate.Date);
            //} 


            //existing.Unit_Item_Table_Id =itemCLS.GetUnit_itemIdForDefaultStore(item.Item_Id ?? 0);
          
            if (existing != null)
            {
                existing.OperationDate = receiptDate.Date;
                existing.Larg_Balance = batchItem.Larg_Balance;
                existing.Item_No = batchItem.Item_No;
                existing.P_Price = batchItem.P_Price;
                existing.QntyValue = batchItem.Larg_Balance * batchItem.P_Price;
                existing.Unit_Item_BatchGroupListId = currentBatchGroup.Id;
                _opp.db.Entry(existing).State = EntityState.Modified;
                _opp.db.SaveChanges();
                Unit_Item_ExpireDateId = existing.Id;

            }
            else
            {
                batchItem.OperationDate = receiptDate;
                batchItem.BatchNo = GetNextBatchNo();

                batchItem.Tash3ela = $"استلام بتاريخ {receiptDate:yyyy-MM-dd}";
                batchItem.User_Id = Common.User_Id;
                batchItem.Branch_Id = Common.Branch_Id;

                batchItem.Unit_Item_BatchGroupListId = currentBatchGroup.Id;
                _opp.db.Unit_Item_ExpireDates.Add(batchItem);
                _opp.db.SaveChanges();
                Unit_Item_ExpireDateId = batchItem.Id;
            }


            Unit_Item_ExpireDateCls uex = new Unit_Item_ExpireDateCls();
            unit_Item_ExpireDateVM = new Unit_Item_ExpireDateVM();

            //var existing_ = _opp.db.Unit_Item_ExpireDates.AsNoTracking().Include(s => s.Supplier).Include(i => i.Item).Where(x => x.Id == batchItem.Id);
            //var f = existing_.FirstOrDefault();
            var existing_ = _opp.db.Unit_Item_ExpireDates.AsNoTracking().Where(x => x.Id == Unit_Item_ExpireDateId);
            var ff = existing_.FirstOrDefault();
            //var f = ff.FirstOrDefault();
            if (ff != null)
            {
                unit_Item_ExpireDateVM = uex.GetUnit_Item_ExpireDateVM(ff);
            }
        }
        catch (Exception ee)
        {
            unit_Item_ExpireDateVM = null;
        }
    }
    public void DeleteBatchItem(ref int Excp, ref string Msg, long id)
    {
        try
        {

            var entitySales = _opp.db.Unit_Item_SalesDates.AsNoTracking().Where(i => i.Unit_Item_ExpireDateId == id).FirstOrDefault();
            if (entitySales != null)
            {
                Excp = 1;
                Msg = "لا يمكن الحذف نظرا لوجود عمليات بيع على الصنف";
                return;
            }
            var entity = _opp.db.Unit_Item_ExpireDates.Find(id);
            if (entity != null)
            {

                long Unit_Item_BatchListId = entity.Unit_Item_BatchGroupListId ?? 0;
                _opp.db.Unit_Item_ExpireDates.Remove(entity);

                Excp = 0;
                Msg = "تم   حذف الصنف";
                _opp.db.SaveChanges();
                //////////////////
                ///
                int count = _opp.db.Unit_Item_ExpireDates.Count(e => e.Unit_Item_BatchGroupListId == Unit_Item_BatchListId);
                if (count == 0)
                {
                    var unit_Item_BatchList = _opp.db.Unit_Item_BatchGroupLists.Where(i => i.Id == Unit_Item_BatchListId).FirstOrDefault();
                    _opp.db.Unit_Item_BatchGroupLists.Remove(unit_Item_BatchList);
                }
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

    public void DeleteAllBatchItem(ref int Excp, ref string Msg, long id)
    {
        try
        {

            var entitySales = _opp.db.Unit_Item_SalesDates.AsNoTracking().Where(i => i.Unit_Item_BatchGroupListId == id).FirstOrDefault();
            if (entitySales != null)
            {
                Excp = 1;
                Msg = "لا يمكن الحذف نظرا لوجود عمليات بيع على الصنف";
                return;
            }
            var entity = _opp.db.Unit_Item_ExpireDates.Where(i => i.Unit_Item_BatchGroupListId == id);
            if (entity != null)
            {

                //long Unit_Item_BatchListId = entity.Unit_Item_BatchListId ?? 0;
                _opp.db.Unit_Item_ExpireDates.RemoveRange(entity);

                Excp = 0;
                Msg = "تم   حذف الصنف";
                _opp.db.SaveChanges();
                //////////////////
                ///

                var unit_Item_BatchList = _opp.db.Unit_Item_BatchGroupLists.Where(i => i.Id == id).FirstOrDefault();
                _opp.db.Unit_Item_BatchGroupLists.Remove(unit_Item_BatchList);

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
    private long? GetUnitItemTableId(long itemId)
    {
        var unitItem = _opp.db.Unit_item.FirstOrDefault(u => u.Item_Id == itemId);
        return unitItem?.id;
    }
    // إضافة أو تحديث بيانات استلام باتش متعدد (BatchReceiptMultiple)
    public void SaveBatchReceiptMultiple(long supplierId, DateTime receiptDate, List<(long itemId, decimal largeQty, decimal smallQty, decimal price)> items)
    {
        //if (items == null || items.Count == 0)
        //    throw new ArgumentException("لا يوجد أصناف للاستلام.");

        //// البحث عن BatchList بنفس المورد والتاريخ
        //var batchList = _opp.db.Unit_Item_BatchGroupLists
        //    .Include(b => b.Unit_Item_ExpireDates)
        //    .FirstOrDefault(b => b.Supplier_Id == supplierId && b.ReceiptDate == receiptDate.Date);

        //if (batchList == null)
        //{
        //    batchList = new Unit_Item_BatchGroupList
        //    {
        //        Supplier_Id = supplierId,
        //        ReceiptDate = receiptDate,
        //        Unit_Item_ExpireDates = new List<Unit_Item_ExpireDate>()
        //    };
        //    _opp.db.Unit_Item_BatchGroupLists.Add(batchList);
        //    _opp.db.SaveChanges();
        //}

        //foreach (var item in items)
        //{
        //    // البحث عن وجود سجل للباتش والصنف ضمن BatchList
        //    var existingExpireDate = batchList.Unit_Item_ExpireDates
        //        .FirstOrDefault(u => u.Item_Id == item.itemId);

        //    if (existingExpireDate != null)
        //    {
        //        // تحديث الكميات والأسعار
        //        existingExpireDate.Larg_Balance += item.largeQty;
        //        existingExpireDate.Small_Balance += item.smallQty;
        //        existingExpireDate.P_Price = item.price; // تحديث السعر حسب الحاجة
        //        existingExpireDate.OperationDate = DateTime.Now;
        //    }
        //    else
        //    {
        //        // إضافة سجل جديد في Unit_Item_ExpireDate مرتبط بـ BatchList
        //        var newExpireDate = new Unit_Item_ExpireDate
        //        {
        //            Item_Id = item.itemId,
        //            Larg_Balance = item.largeQty,
        //            Small_Balance = item.smallQty,
        //            P_Price = item.price,
        //            Unit_Item_BatchGroupListId = batchList.Id,
        //            OperationDate = DateTime.Now,
        //            BatchNo = GetNextBatchNo(), // دالة تحسب رقم باتش جديد
        //        };
        //        _opp.db.Unit_Item_ExpireDates.Add(newExpireDate);

        //        // ربط السجل الجديد بالقائمة
        //        batchList.Unit_Item_ExpireDates.Add(newExpireDate);
        //    }
        //}

        //_opp.db.SaveChanges();
    }

    // دالة للحصول على رقم باتش جديد
    private long GetNextBatchNo()
    {
        var maxBatchNo = _opp.db.Unit_Item_ExpireDates.Max(b => (long?)b.BatchNo) ?? 0;
        return maxBatchNo + 1;
    }

    // حذف باتش معين مع تحديث الكميات في المخزون
    public bool RemoveBatchExpireDate(long expireDateId)
    {
        var expireDate = _opp.db.Unit_Item_ExpireDates.FirstOrDefault(e => e.Id == expireDateId);
        if (expireDate == null) return false;

        _opp.db.Unit_Item_ExpireDates.Remove(expireDate);
        _opp.db.SaveChanges();
        return true;
    }

    // استرجاع بيانات باتش حسب BatchListId
    public List<Unit_Item_ExpireDate> GetExpireDatesByBatchListId(long batchListId)
    {
        return _opp.db.Unit_Item_ExpireDates
            .Where(e => e.Unit_Item_BatchGroupListId == batchListId)
            .ToList();
    }

    // الحصول على قائمة BatchList لمورد معين وتاريخ محدد
    public Unit_Item_BatchGroupList GetBatchList(long supplierId, DateTime receiptDate)
    {
        return _opp.db.Unit_Item_BatchGroupLists
            .Include(b => b.Unit_Item_ExpireDates)
            .FirstOrDefault(b => b.Supplier_Id == supplierId && b.ReceiptDate == receiptDate.Date);
    }

    public Unit_Item_ExpireDateVM GetUnit_Item_ExpireDateVMById(long unit_Item_ExpireDateId)
    {
        Unit_Item_ExpireDateCls uex = new Unit_Item_ExpireDateCls();
        var existing_ = _opp.db.Unit_Item_ExpireDates.AsNoTracking().Where(x => x.Id == unit_Item_ExpireDateId);
        var ff = existing_.FirstOrDefault();
        if (ff != null)
        {
            unit_Item_ExpireDateVM = uex.GetUnit_Item_ExpireDateVM(ff);
        }
        return unit_Item_ExpireDateVM;
    }
}

}