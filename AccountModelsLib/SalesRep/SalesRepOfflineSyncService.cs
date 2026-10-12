using System;
using System.Collections.Generic;
using System.Linq;
using AccountModelsLib.SalesRep;
using AccountModelsLib.Models;

namespace AccountModelsLib.SalesRep
{
    /// <summary>
    /// مرحلة 2: بروتوكول المزامنة للأوفلاين
    /// يدير تبادل البيانات بين جهاز المندوب والسيرفر في حالة انقطاع الإنترنت.
    /// </summary>
    public class SalesRepOfflineSyncService
    {
        private readonly operations _opp;

        public SalesRepOfflineSyncService(operations opp)
        {
            _opp = opp;
        }

        /// <summary>
        /// تسجيل جهاز المندوب (أول تشغيل للتطبيق على الجهاز)
        /// </summary>
        public SalesRep_Device RegisterDevice(long repId, string deviceUid, string platform)
        {
            // التحقق من عدم تكرار الجهاز
            var existing = _opp.db.SalesRep_Devices
                .FirstOrDefault(d => d.Device_Uid == deviceUid && d.Rep_Id == repId);
            if (existing != null)
            {
                if (existing.Is_Blocked)
                    throw new InvalidOperationException("الجهاز محظور من النظام.");
                return existing;
            }

            var device = new SalesRep_Device
            {
                Id = CallSnowflakeIdGenerate.GenerateNewId(),
                Rep_Id = repId,
                Device_Uid = deviceUid,
                Platform = platform,
                Last_Sync_At = DateTime.Now,
                Is_Blocked = false,
                Number_Range_From = null,
                Number_Range_To = null,
                Number_Range_Next = null
            };
            _opp.db.SalesRep_Devices.Add(device);
            _opp.db.SaveChanges();
            return device;
        }

        /// <summary>
        /// سحب بيانات المزامنة الكاملة للمندوب (Pull)
        /// تُرسل للجهاز عند أول تسجيل وعند كل مزامنة ناجحة
        /// </summary>
        public SalesRepSyncPackage GetSyncPackage(long repId, long deviceId)
        {
            var device = _opp.db.SalesRep_Devices.FirstOrDefault(d => d.Id == deviceId && d.Rep_Id == repId);
            if (device == null) throw new InvalidOperationException("الجهاز غير مسجل.");
            if (device.Is_Blocked) throw new InvalidOperationException("الجهاز محظور.");

            // 1. بيانات المندوب
            var rep = _opp.db.SalesRep_Reps.FirstOrDefault(r => r.Id == repId);

            // 2. خطوط السير والعملاء
            var routes = _opp.db.SalesRep_Routes.Where(r => r.Rep_Id == repId).ToList();
            var routeIds = routes.Select(r => r.Id).ToList();
            var routeCustomers = _opp.db.SalesRep_Route_Customers
                .Where(rc => routeIds.Contains(rc.Route_Id)).ToList();

            // 3. أسعار البيع (قائمة الأسعار المرتبطة بالمندوب)
            var prices = new List<Price_Type_Sell_Values>();
            if (rep?.Price_Type_List_ID != null)
            {
                prices = _opp.db.Price_Type_Sell_Values
                    .Where(p => p.Price_Type_List_ID == rep.Price_Type_List_ID)
                    .ToList();
            }

            // 4. سياسات الائتمان والخصم
            var creditPolicies = _opp.db.SalesRep_Credit_Policies.ToList();
            var discountPolicies = _opp.db.SalesRep_Discount_Policies.ToList();

            // 5. أسباب عدم البيع والإرجاع
            var noSaleReasons = _opp.db.Lookup_SalesRep_NoSaleReasons.ToList();
            var returnReasons = _opp.db.Lookup_SalesRep_ReturnReasons.ToList();

            // 6. مستندات معلقة لم تُزامن بعد (Outbox)
            var pendingOutbox = _opp.db.SalesRep_Sync_Inboxes
                .Where(s => s.Device_Id == deviceId && s.Status == 0)
                .OrderBy(s => s.Received_At)
                .ToList();

            // تحديث آخر مزامنة
            device.Last_Sync_At = DateTime.Now;
            _opp.db.SaveChanges();

            return new SalesRepSyncPackage
            {
                Rep = rep,
                Routes = routes,
                RouteCustomers = routeCustomers,
                Prices = prices,
                CreditPolicies = creditPolicies,
                DiscountPolicies = discountPolicies,
                NoSaleReasons = noSaleReasons,
                ReturnReasons = returnReasons,
                PendingInbox = pendingOutbox,
                ServerTime = DateTime.Now
            };
        }

        /// <summary>
        /// استقبال المستندات من الجهاز (Push)
        /// تُستدعى عند عودة الاتصال لإنزال ما جمعه المندوب أثناء الأوفلاين
        /// </summary>
        public SalesRepSyncResult ProcessDevicePush(long deviceId, List<SalesRep_Sync_Inbox> pushedItems)
        {
            var result = new SalesRepSyncResult();
            var device = _opp.db.SalesRep_Devices.FirstOrDefault(d => d.Id == deviceId);
            if (device == null) throw new InvalidOperationException("الجهاز غير مسجل.");
            if (device.Is_Blocked) throw new InvalidOperationException("الجهاز محظور.");

            foreach (var item in pushedItems)
            {
                try
                {
                    // 1. التحقق من صحة الحزمة
                    if (item.Doc_Id <= 0)
                    {
                        result.Errors.Add($"مستند غير صالح: Doc_Id = {item.Doc_Id}");
                        continue;
                    }

                    // 2. توجيه الحركة حسب نوع المستند إلى المحرك المناسب
                    ProcessDocument(item);

                    // 3. تسجيل الاكتمال
                    item.Status = 1; // Processed
                    item.Processed_At = DateTime.Now;
                    result.Processed++;
                }
                catch (Exception ex)
                {
                    item.Status = 2; // Error
                    item.Error = ex.Message;
                    result.Errors.Add($"خطأ في معالجة المستند {item.Doc_Id}: {ex.Message}");
                }
            }

            _opp.db.SaveChanges();
            device.Last_Sync_At = DateTime.Now;
            _opp.db.SaveChanges();
            return result;
        }

        /// <summary>
        /// توجيه المستند حسب نوعه (فاتورة/مرتجع/تحصيل/طلبية)
        /// </summary>
        private void ProcessDocument(SalesRep_Sync_Inbox item)
        {
            switch (item.Doc_Type)
            {
                case (int)SalesRepDocType.Invoice:
                    // توجيه إلى محرك الفواتير
                    RouteToInvoiceEngine(item);
                    break;
                case (int)SalesRepDocType.Return:
                    // توجيه إلى محرك المرتجعات
                    RouteToReturnEngine(item);
                    break;
                case (int)SalesRepDocType.CashVoucher:
                    // توجيه إلى محرك السندات النقدية
                    RouteToCashVoucherEngine(item);
                    break;
                case (int)SalesRepDocType.CustomerOrder:
                    // توجيه إلى محرك طلبيات العملاء
                    RouteToCustomerOrderEngine(item);
                    break;
                default:
                    throw new InvalidOperationException($"نوع مستند غير مدعوم: {item.Doc_Type}");
            }
        }

        // ملاحظة: هذه الدوال تمثل نقاط الربط مع محركات ERP الحالية
        // وسيتم تكاملها بالكامل في المرحلة الثالثة
        private void RouteToInvoiceEngine(SalesRep_Sync_Inbox item)
        {
            // الربط بالمحرك الفعلي: Account_Entry_Invoice_CLS.Make_EntryForInvoices
            // يتم استدعاء المحرك الحقيقي مع تمرير opp ومرجع الفاتورة (item.Doc_Id / Payload).
            int excp = 0;
            string msg = "";
            var opp = _opp; // نسخة محلية لأن المحرك يأخذ ref operations
            var engine = new Account_Entry_Invoice_CLS();
            engine.Make_EntryForInvoices(
                ref excp,
                ref msg,
                ref opp,
                item.Doc_Id,
                DateTime.Now,
                DateTime.Now,
                0   // IS_Posted_Flage = 0 (غير مرحل حتى الآن)
            );
            if (excp != 0) throw new InvalidOperationException($"فشل قيد الفاتورة للمحرك: {msg}");

            // تسجيل ربط الوثيقة بالوردية (من الـ Payload أو استرجاع من الفاتورة)
            long shiftId = 0; // يُقرأ من Payload عند التكامل الكامل
            var link = new SalesRep_Document_Link
            {
                Id = CallSnowflakeIdGenerate.GenerateNewId(),
                Doc_Type = (int)SalesRepDocType.Invoice,
                Doc_Id = item.Doc_Id,
                Shift_Id = shiftId,
                Visit_Id = null,
                Doc_Guid = Guid.NewGuid(),
                User_Id = (int?)Common.User_Id,
                OperationDate = DateTime.Now,
                Insert_ServerDateTime = DateTime.Now,
                Branch_Id = (int?)Common.Branch_Id,
                ProcessNo = null
            };
            _opp.db.SalesRep_Document_Links.Add(link);
            _opp.db.SaveChanges();
        }
        private void RouteToReturnEngine(SalesRep_Sync_Inbox item)
        {
            // الربط بمحرك المرتجعات: Account_Entry_Invoice_CLS.Make_EntryForInvoices_Reverse
            int excp = 0;
            string msg = "";
            var opp = _opp;
            var engine = new Account_Entry_Invoice_CLS();
            engine.Make_EntryForInvoices_Reverse(
                ref excp,
                ref msg,
                ref opp,
                item.Doc_Id,
                DateTime.Now,
                DateTime.Now,
                -1  // إعادة العكس (Reverse = -1 لسجل المرتجع)
            );
            if (excp != 0) throw new InvalidOperationException($"فشل قيد المرتجع للمحرك: {msg}");

            // ربط أسطر المرتجع بالأسطر الأصلية عبر SalesRep_Invoice_Return_Links
            // (التفاصيل الدقيقة من Payload عند الربط الكامل)
            _opp.db.SaveChanges();
        }
               private void RouteToCashVoucherEngine(SalesRep_Sync_Inbox item)
        {
            // الربط بمحرك السندات النقدية: Money_Transaction_BoxOrderReportCLS
            // تُصدر الإذن متعدد الأسطر (Make_Transaction_BanksBox_NewSarf)
            // داخل معاملة واحدة — نفس تسلسل New_OrderMoney_SarfNew.SaveNewOrderFinally
            // يتم استدعاء CashVouchersService.SaveMultiVoucher عبر الـ Payload
            // (تفاصيل الأسطر من JSON عند التكامل الكامل)
            int excp = 0;
            string msg = "";
            var engine = new Money_Transaction_BoxOrderReportCLS();
            // ملاحظة: Make_Transaction_BanksBox_NewSarf يتطلب DataTable بأسطر
            // Money_Transaction_Box كامل — تُقرأ من Payload عند الربط النهائي
            _opp.db.SaveChanges();
        }
        private void RouteToCustomerOrderEngine(SalesRep_Sync_Inbox item)
        {
            // الربط بمحرك طلبيات العملاء — يتم عبر Orders / Customer_Order
            // (نقطة ربط مستقبلية داخل معاملة واحدة مع SalesRep_Invoice_Return_Links عند الاحتياج)
            _opp.db.SaveChanges();
        }
    }

    /// <summary>حزمة المزامنة الكاملة المنسحبة للجهاز.</summary>
    public class SalesRepSyncPackage
    {
        public SalesRep_Rep Rep { get; set; }
        public List<SalesRep_Route> Routes { get; set; } = new List<SalesRep_Route>();
        public List<SalesRep_Route_Customer> RouteCustomers { get; set; } = new List<SalesRep_Route_Customer>();
        public List<Price_Type_Sell_Values> Prices { get; set; } = new List<Price_Type_Sell_Values>();
        public List<SalesRep_Credit_Policy> CreditPolicies { get; set; } = new List<SalesRep_Credit_Policy>();
        public List<SalesRep_Discount_Policy> DiscountPolicies { get; set; } = new List<SalesRep_Discount_Policy>();
        public List<Lookup_SalesRep_NoSaleReason> NoSaleReasons { get; set; } = new List<Lookup_SalesRep_NoSaleReason>();
        public List<Lookup_SalesRep_ReturnReason> ReturnReasons { get; set; } = new List<Lookup_SalesRep_ReturnReason>();
        public List<SalesRep_Sync_Inbox> PendingInbox { get; set; } = new List<SalesRep_Sync_Inbox>();
        public DateTime ServerTime { get; set; }
    }

    /// <summary>نتيجة معالجة الدفع من الجهاز.</summary>
    public class SalesRepSyncResult
    {
        public int Processed { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}
