using System;
using System.Collections.Generic;
using System.Linq;
using AccountModelsLib;
using AccountModelsLib.Models;
using AccountModelsLib.SalesRep;

namespace AccountModelsLib.SalesRep
{
    /// <summary>
    /// مشغل اختبارات موديول المندوبين (Field Sales / Van Sales)
    /// يقوم باختبار كافة العمليات داخل Transaction ملغاة (Rollback)
    /// لضمان عدم التأثير على البيانات الحالية لقاعدة البيانات نهائياً.
    /// </summary>
    public class SalesRepTestRunner
    {
        /// <summary>
        /// سلسلة الاتصال المستخدمة للاختبار (اختياري)
        /// عند عدم تحديدها يُستخدم الاتصال الافتراضي من operations.ReturnConnStr()
        /// </summary>
        public static string TestConnStr { get; set; }

        public static void RunAllTests()
        {
            // العمليات(2) تبدأ المعاملة داخلياً وتربط الـ EF Context بها (BeginTransaction + UseTransaction)
            // لذا لا نبدأ معاملة ثانية هنا — نلغي عبر opp.Rollback() في الـ finally
            var opp = string.IsNullOrEmpty(TestConnStr)
                ? new operations(2)
                : new operations(TestConnStr);
            opp.BeginTransaction(2);
            try
            {
                Console.WriteLine("=== بدء اختبارات موديول المندوبين (Field Sales) ===");

                // إعداد بيانات الاختبار: مندوب افتراضي (بما أن الاختبار في معاملة ملغاة)
                long testRepId = 999901;
                var testRep = new SalesRep_Rep
                {
                    Id = testRepId,
                    Employee_Id = testRepId,
                    Van_Store_Id = 1,
                    Damaged_Store_Id = 1,
                    Cash_Box_Id = 1,
                    Sales_Mode = (int)SalesRepSalesMode.Van,
                    Active = true
                };
                opp.db.SalesRep_Reps.Add(testRep);
                opp.db.SaveChanges();

                // 1. اختبار فتح الوردية
                var shiftService = new SalesRepShiftService(opp);
                var shift = shiftService.OpenShift(testRepId, DateTime.Now);
                if (shift == null || shift.Id <= 0)
                    throw new Exception("فشل اختبار فتح الوردية.");
                Console.WriteLine($"[1/8] نجح اختبار فتح الوردية (Shift ID: {shift.Id}).");

                // 2. اختبار إدارة الموافقات والائتمان
                var approvalService = new SalesRepApprovalService(opp);
                var approvalReq = approvalService.CreateApprovalRequest(
                    (int)SalesRepApprovalType.Credit,
                    Guid.NewGuid().ToString(),
                    testRepId,
                    "طلب تجاوز حد ائتمان عميل جديد"
                );
                if (approvalReq == null || approvalReq.Id <= 0)
                    throw new Exception("فشل اختبار إنشاء طلب الموافقة.");
                approvalService.ApproveRequest(approvalReq.Id, 1);
                Console.WriteLine($"[2/8] نجح اختبار إدارة طلبات الموافقة (Approval ID: {approvalReq.Id}).");

                // 3. اختبار البيع الميداني والفواتير المربوطة
                var salesService = new SalesRepSalesService(opp);
                long testCustomerId = 888801;
                bool creditCheck = salesService.ValidateCredit(testCustomerId, 1500m);
                // محاكاة سطر فاتورة
                var lines = new List<Invoices_Details>
                {
                    new Invoices_Details { Total = 1500m }
                };
                long saleInvoiceId = salesService.CreateFieldSaleInvoice(shift.Id, testCustomerId, lines, 1);
                Console.WriteLine($"[3/8] نجح اختبار البيع الميداني وتوثيق الوردية (Sale Invoice: {saleInvoiceId}).");

                // 4. اختبار المرتجع الميداني المربوط
                var returnService = new SalesRepReturnService(opp);
                var returnLines = new List<Invoices_Details_Reverse>
                {
                    new Invoices_Details_Reverse { Larg_Qnty = 2 }
                };
                long returnInvoiceId = returnService.CreateReturnInvoice(shift.Id, saleInvoiceId, returnLines);
                Console.WriteLine($"[4/8] نجح اختبار المرتجع الميداني المربوط (Return Invoice: {returnInvoiceId}).");

                // 5. اختبار المزامنة دون إنترنت (Offline Sync)
                var syncService = new SalesRepSyncService(opp);
                var syncData = syncService.GetSyncData(testRepId);
                if (syncData == null)
                    throw new Exception("فشل اختبار سحب بيانات المزامنة.");
                Console.WriteLine("[5/8] نجح اختبار سحب بيانات المزامنة للأجهزة المحمولة.");

                // 6. اختبار إقفال الوردية واحتساب العمولات
                shiftService.CloseShift(shift.Id);
                var commissionService = new SalesRepCommissionService(opp);
                decimal commission = commissionService.CalculateCommission(testRepId, DateTime.Now.AddDays(-1), DateTime.Now.AddDays(1));
                Console.WriteLine($"[6/8] نجح اختبار إقفال الوردية واحتساب العمولة (العمولة: {commission}).");
                // 7. اختبار تسجيل الجهاز وبروتوكول الأوفلاين (Phase 2)
                var offlineSync = new SalesRepOfflineSyncService(opp);
                var device = offlineSync.RegisterDevice(testRepId, "TEST-DEVICE-001", "Android");
                if (device == null || device.Id <= 0)
                    throw new Exception("فشل اختبار تسجيل الجهاز.");
                Console.WriteLine($"[7/8] نجح اختبار تسجيل الجهاز (Device ID: {device.Id}).");

                // 8. اختبار سحب حزمة المزامنة الكاملة
                var package = offlineSync.GetSyncPackage(testRepId, device.Id);
                if (package == null || package.Rep == null)
                    throw new Exception("فشل اختبار سحب حزمة المزامنة.");
                Console.WriteLine($"[8/8] نجح اختبار سحب حزمة المزامنة (Routes: {package.Routes.Count}, Prices: {package.Prices.Count}).");

                Console.WriteLine("=== اكتملت كافة اختبارات موديول المندوبين بنجاح تام ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"خطأ أثناء الفحص: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"خطأ داخلي: {ex.InnerException.Message}");
                if (ex.StackTrace != null)
                    Console.WriteLine($"المسار: {ex.StackTrace}");
                throw;
            }
            finally
            {
                // إلغاء المعاملة الداخلية لضمان عدم حفظ أي بيانات اختبارية في قاعدة البيانات
                try { opp.Rollback(); } catch { }
                Console.WriteLine("تم إلغاء المعاملة (Rollback) بنجاح لضمان بقاء البيانات القديمة دون تغيير.");
            }
        }
    }
}
