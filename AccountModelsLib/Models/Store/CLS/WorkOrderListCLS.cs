using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace AccountModelsLib.Models
{
    /// <summary>
    /// Business layer for WorkOrderList (إنشاء / تعديل / جلب بيانات مساعدة)
    /// يحوي كل عمليات التحقق والتعامل مع DbContext عبر operations opp
    /// </summary>
    public class WorkOrderListCLS
    {
          operations opp  ;
        private CustomerCLS customerCLS = new CustomerCLS();

        public WorkOrderListCLS()
        {
            opp = new operations();
        }
        public WorkOrderListCLS(ref operations opp_)
        {
            opp = opp_;
        }
        /// <summary>
        /// جلب أمر تشغيل حسب المعرف
        /// </summary>
        public List<WorkOrderList> GetWorkOrderList(long WorkOrder_No,bool IsdtFrom,DateTime dtfrom, bool IsdtTo, DateTime dtto, int Status,int orderTypeVal)
        {
            var query = opp.db.WorkOrderList.AsNoTracking().AsQueryable();
            if (WorkOrder_No > 0)
            {
                query = query.Where(w => w.WorkOrder_No == WorkOrder_No);
            }
            else
            {
                if (IsdtFrom)
                { query = query.Where(w => w.WorkOrderDate.Value.Date >= dtfrom); }
                if (IsdtTo)
                { query = query.Where(w => w.WorkOrderDate.Value.Date <= dtto); }

                if (Status > 0)
                {
                    query = query.Where(w => w.Status == Status);
                }
                if (orderTypeVal > 0)
                {
                    query = query.Where(w => w.WorkOrderType == orderTypeVal);
                }
            }
            return query.ToList();
        }
        public WorkOrderList GetById(long id)
        {
            return opp.db.WorkOrderList
                .FirstOrDefault(w => w.Id == id);
        }
        /// <summary>
        /// جلب قائمة مبسطة من العملاء للاستخدام في ComboBox
        /// </summary>
        public List<Customer> GetCustomers()
        {
            // افتراض وجود جدول Customers مع CustomerId و CustomerName
         
            List<Customer> customers = customerCLS.GetCustomersAsNoTracking();
            return customers;
        }

        /// <summary>
        /// جلب قائمة المخازن (Stores) للاستخدام في ComboBox
        /// </summary>
        public List<Store> GetStores()
        {
            return opp.db.Stores
               
                .ToList();
        }

        /// <summary>
        /// جلب قائمة الماكينات/خطوط الإنتاج
        /// </summary>
        public List<Machin_list> GetMachines()
        {
            return opp.db.Machin_list
                
                .ToList();
        }

        /// <summary>
        /// جلب Items (المنتجات/الخامات) لاستخدامها في تفاصيل أمر الشغل
        /// </summary>
        public List<Items> GetItems()
        {
            return opp.db.Item
               
                .ToList();
        }

        /// <summary>
        /// توليد رقم أمر تشغيل تتابعي (مبسّط)
        /// </summary>
        public long GenerateNextWorkOrderNo()
        {
            var maxNo = opp.db.WorkOrderList.Max(w => (long?)w.WorkOrder_No) ?? 0;
            return maxNo + 1;
        }

        /// <summary>
        /// حفظ أمر تشغيل جديد أو تحديث أمر موجود
        /// يقوم بالتحقق من صحة الحقول ثم ينفذ Insert/Update داخل Transaction
        /// </summary>
        public void Save(WorkOrderList model, List<WorkOrderDetail> details)
        {
            // Validation عام
            if (string.IsNullOrWhiteSpace(model.WorkOrder_No.ToString()))
                throw new Exception("رقم أمر التشغيل مطلوب.");

            if (model.WorkOrderDate == null)
                throw new Exception("تاريخ الأمر مطلوب.");

            // OrderType logic: بناءً على نوع الجهة يجب أن يكون CustomerId أو StoreId أو MachineId محدد
            // افتراض وجود حقل OrderType كـ int في WorkOrderList (أضفه إن لم يكن)
            if (model.WorkOrderType == null)
                throw new Exception("نوع الجهة (OrderType) مطلوب.");

            // مثال تحقق: لو OrderType = Customer (2) يجب أن يكون Cust_Supplier_id معرف (Customer)
            // تعديل هذه الشروط حسب نموذجك الفعلي للأعمدة
            int orderType = model.WorkOrderType ?? 0;
            if (orderType == 2) // Customer
            {
                if (model.CustomerId <= 0)
                    throw new Exception("العميل مطلوب لأوامر WorkOrder من نوع عميل.");
            }
            //else if (orderType == 4 || orderType == 20) // Store or Department
            //{
            //    if (model.Id_To <= 0)
            //        throw new Exception("المخزن/القسم مطلوب لأوامر WorkOrder من نوع مخزن/قسم.");
            //}

            // تحقق التواريخ
            if (model.OperationDate != null && model.WorkOrderDate != null)
            {
                // لا مانع من ترك هذه المقارنة إن لزم
            }

            using (var txn = opp.db.Database.BeginTransaction())
            {
                try
                {
                    // Insert جديد
                    if (model.Id == 0)
                    {
                        // توليد رقم تسلسلي إن لم يكن مُزود
                        if (model.WorkOrder_No == 0)
                            model.WorkOrder_No = GenerateNextWorkOrderNo();

                        //opp.db.Order_Purshings.Add(model); // نستخدم نفس الجدول Order_Purshing حسب طلبك؟ 
                        // مع ملاحظة: أنت اخترت استخدام WorkOrderList سابقاً؛ إذا تستخدم WorkOrderList بدل Order_Purshing ضع opp.db.WorkOrderList.Add(model)
                        // سأفترض أنك تستخدم WorkOrderList table; عدّل السطر التالي إن كان اسم الـ DbSet مختلف:
                        opp.db.WorkOrderList.Add(model);
                        opp.db.SaveChanges();

                        // حفظ التفاصيل وربطها
                        if (details != null && details.Count > 0)
                        {
                            foreach (var d in details)
                            {
                                d.WorkOrderListId = model.Id;
                                opp.db.WorkOrderDetail.Add(d);
                            }
                            opp.db.SaveChanges();
                        }
                    }
                    else // Update
                    {
                        var existingWorkOrderList = opp.db.WorkOrderList.FirstOrDefault(w => w.Id == model.Id);
                        if (existingWorkOrderList == null) throw new Exception("أمر التشغيل غير موجود للحفظ.");
                        
                        opp.db.Entry(existingWorkOrderList).CurrentValues.SetValues(model);
                         

                        opp.db.SaveChanges();

                        // تفاصيل: أسهل مسار هو حذف تفاصيل القديمة وإضافة التفاصيل الجديدة (يمكن تحسينه لاحقًا)
                        // جلب التفاصيل القديمة من قاعدة البيانات
                        var oldDetails = opp.db.WorkOrderDetail
                            .Where(d => d.WorkOrderListId == model.Id)
                            .ToList();

                        if (details != null && details.Count > 0)
                        {
                            // Step 1 & 2: Update or Insert
                            foreach (var d in details)
                            {
                                if (d.Id > 0) // موجود قبل كده = Update
                                {
                                    var existing = oldDetails.FirstOrDefault(x => x.Id == d.Id);
                                    if (existing != null)
                                    {
                                        opp.db.Entry(existing).CurrentValues.SetValues(d);
                                       
                                        // باقي الأعمدة
                                    }
                                }
                                else // جديد = Insert
                                {
                                    d.WorkOrderListId = model.Id;
                                    opp.db.WorkOrderDetail.Add(d);
                                }
                            }

                            // Step 3: Delete (اللي مش موجود في الجريد)
                            var detailIdsInForm = details.Where(x => x.Id > 0).Select(x => x.Id).ToList();
                            var toDelete = oldDetails.Where(x => !detailIdsInForm.Contains(x.Id)).ToList();

                            if (toDelete.Any())
                            {
                                opp.db.WorkOrderDetail.RemoveRange(toDelete);
                            }

                            opp.db.SaveChanges();
                        }
                        else
                        {
                            // لو المستخدم مسح كل التفاصيل → نحذفهم كلهم من DB
                            if (oldDetails.Any())
                            {
                                opp.db.WorkOrderDetail.RemoveRange(oldDetails);
                                opp.db.SaveChanges();
                            }
                        }

                    }

                    txn.Commit();
                }
                catch (Exception)
                {
                    txn.Rollback();
                    throw;
                }
            }
        }
    }
}
