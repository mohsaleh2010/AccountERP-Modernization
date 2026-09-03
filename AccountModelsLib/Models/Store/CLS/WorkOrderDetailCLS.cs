using System;
using System.Collections.Generic;
using System.Linq;

namespace AccountModelsLib.Models
{
    /// <summary>
    /// Business layer for WorkOrderList (إنشاء / تعديل / جلب بيانات مساعدة)
    /// يحوي كل عمليات التحقق والتعامل مع DbContext عبر operations opp
    /// </summary>
    public class WorkOrderDetailCLS
    {
        private operations opp = new operations();

        /// <summary>
        /// جلب أمر تشغيل حسب المعرف
        /// </summary>
        public WorkOrderList GetById(long id)
        {
            return opp.db.WorkOrderList
                .FirstOrDefault(w => w.Id == id);
        }
        public List<WorkOrderDetail> GetByWorkOrderListId(long WorkOrderListId)
        {
            var details = opp.db.WorkOrderDetail.Where(d => d.WorkOrderListId == WorkOrderListId).ToList();
            return details;
        }
       

        /// <summary>
        /// جلب قائمة مبسطة من العملاء للاستخدام في ComboBox
        /// </summary>
        public List<dynamic> GetCustomers()
        {
            // افتراض وجود جدول Customers مع CustomerId و CustomerName
            return opp.db.Customer
                .Select(c => new { Value = c.Id, Text = c.Name })
                .ToList<dynamic>();
        }

        /// <summary>
        /// جلب قائمة المخازن (Stores) للاستخدام في ComboBox
        /// </summary>
        public List<dynamic> GetStores()
        {
            return opp.db.Stores
                .Select(s => new { Value = s.Id, Text = s.Store_Name })
                .ToList<dynamic>();
        }

        /// <summary>
        /// جلب قائمة الماكينات/خطوط الإنتاج
        /// </summary>
        public List<dynamic> GetMachines()
        {
            return opp.db.Machin_list
                .Select(m => new { Value = m.Id, Text = m.Machin_Name })
                .ToList<dynamic>();
        }

        /// <summary>
        /// جلب Items (المنتجات/الخامات) لاستخدامها في تفاصيل أمر الشغل
        /// </summary>
        public List<dynamic> GetItems()
        {
            return opp.db.Item
                .Select(i => new { Value = i.id, Text = i.Item_Name })
                .ToList<dynamic>();
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
                        var existing = opp.db.WorkOrderList.FirstOrDefault(w => w.Id == model.Id);
                        if (existing == null) throw new Exception("أمر التشغيل غير موجود للحفظ.");

                        // تحديث الحقول المرخص بتعديلها
                        existing.WorkOrderDate = model.WorkOrderDate;
                        existing.WorkOrder_No = model.WorkOrder_No;
                        
                        existing.Status = model.Status;
                        existing.StoreId = model.StoreId;
                        existing.StoreName = model.StoreName;
                        existing.CustomerId = model.CustomerId;
                        existing.CustomerName = model.CustomerName;
                        existing.EmployeeId = model.EmployeeId;
                        existing.EmployeeName = model.EmployeeName;
                        existing.NotesAll = model.NotesAll;
                        existing.WorkOrderType = model.WorkOrderType;
                        //existing.Id_To = model.Id_To;
                        //existing.Id_From = model.Id_From;

                        opp.db.SaveChanges();

                        // تفاصيل: أسهل مسار هو حذف تفاصيل القديمة وإضافة التفاصيل الجديدة (يمكن تحسينه لاحقًا)
                        var oldDetails = opp.db.WorkOrderDetail.Where(d => d.WorkOrderListId == model.Id).ToList();
                        if (oldDetails.Any())
                        {
                            opp.db.WorkOrderDetail.RemoveRange(oldDetails);
                            opp.db.SaveChanges();
                        }

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
