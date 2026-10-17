using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using AccountERP.SharedUI.Services;
using AccountModelsLib.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AccountModelsLib.SalesRep
{
    /// <summary>
    /// المرحلة 7 (أ): مزامنة جهاز المندوب.
    /// - الرفع: كل مستند بمعرّف Doc_Guid فريد يُسجَّل فى SalesRep_Sync_Inbox أولاً (إعادة الإرسال ترجع نفس النتيجة بلا أثر جديد)، ثم يُعالَج
    ///   بنفس خدمات الشاشات فى معاملته، وحالة صفه تتحدث فى نفس المعاملة. مخالفة تحتاج موافقة أو خطأ عمل (مخزون/وردية/حد) = PendingReview
    ///   مع إنشاء طلب الموافقة تلقائياً — لا رفض صامت؛ بيانات تالفة أو نوع مجهول = Error. المراجعة من المكتب: إعادة المعالجة أو الرفض.
    /// - التنزيل («الفرق من السيرفر»، قرار المستخدم 2026-10-17): لكل مجموعة بيانات بصمة كل صف استلمه الجهاز فى SalesRep_Device_Datasets؛
    ///   الجهاز يرسل آخر نسخة طبّقها، فإن طابقت يُرسَل المضاف/المعدَّل/المحذوف فقط، وإلا (أول مرة/رد ضاع/إعادة تثبيت) تنزيل كامل.
    ///   بلا أى تعديل على الجداول القديمة.
    /// المستندات تُسجَّل باسم مستخدم الجهاز وعلى فرع المندوب (UserSessionContext) فلا تتأثر بمستخدمى الويب المتزامنين.
    /// </summary>
    public class SalesRepSyncService
    {
        private readonly operations _external;
        public SalesRepSyncService(operations external = null) { _external = external; }

        // ============================== العقود ==============================

        public class PushDoc
        {
            public Guid Doc_Guid { get; set; }
            public int Doc_Type { get; set; }
            /// <summary>مدخل الخدمة بصيغة JSON (أسماء الخصائص كما فى C#).</summary>
            public string Payload { get; set; }
            public DateTime? Created_At { get; set; }
        }

        public class PushResult
        {
            public Guid Doc_Guid { get; set; }
            public int Status { get; set; }
            public string Error { get; set; }
            public long? Result_Ref_Id { get; set; }
            /// <summary>المستند وصل من قبل؛ النتيجة هى المسجلة ولم يتكرر أثره.</summary>
            public bool Duplicate { get; set; }
        }

        /// <summary>بيع فورى من الجهاز: سعر الوحدة الكبرى والخصم قيمةً ونسبة الضريبة كما على الجهاز (السعر لا يُعاد حسابه، م: سعر الجهاز يُحفظ).</summary>
        public class DeviceSaleLine
        {
            public long Unit_Item_Id { get; set; }
            public decimal Qty_Large { get; set; }
            public decimal Qty_Small { get; set; }
            public decimal Unit_Price { get; set; }
            public decimal Discount_Value { get; set; }
            public decimal Tax_Rate { get; set; }
            public string Notes { get; set; }
        }

        public class DeviceSaleInput
        {
            public long Customer_Id { get; set; }
            public bool Is_Credit { get; set; }
            public long Payment_Method_Id { get; set; }
            public string Notes { get; set; }
            public List<DeviceSaleLine> Lines { get; set; } = new();
        }

        public class DeviceCollectionInput
        {
            public long Customer_Id { get; set; }
            public decimal Amount { get; set; }
            public long Payment_Method_Id { get; set; }
            public string Notes { get; set; }
        }

        public class InboxRow
        {
            public long Id { get; set; }
            public long Device_Id { get; set; }
            public string Device_Name { get; set; } = "";
            public long? Rep_Id { get; set; }
            public string Rep_Name { get; set; } = "";
            public Guid Doc_Guid { get; set; }
            public int Doc_Type { get; set; }
            public int Status { get; set; }
            public string Error { get; set; }
            public long? Result_Ref_Id { get; set; }
            public DateTime Received_At { get; set; }
            public DateTime? Processed_At { get; set; }
            public DateTime? Device_Created_At { get; set; }
            public int Attempts { get; set; }
            public string Reviewed_By { get; set; } = "";
            public DateTime? Reviewed_At { get; set; }
            public string Review_Notes { get; set; }
            public string Payload { get; set; }
            // ملخص للعرض من المدخل
            public long Customer_Id { get; set; }
            public string Customer_Name { get; set; } = "";
            public decimal Amount { get; set; }
            public int Lines_Count { get; set; }
        }

        public class PullResult
        {
            public string Dataset { get; set; } = "";
            public long Version { get; set; }
            /// <summary>تنزيل كامل: الجهاز يستبدل المجموعة كلها بـ Rows.</summary>
            public bool Full { get; set; }
            public List<object> Rows { get; set; } = new();
            public List<long> Deleted { get; set; } = new();
            public int Total { get; set; }
        }

        public static readonly string[] Datasets =
            { "profile", "payment_methods", "return_reasons", "nosale_reasons", "banks", "customers", "items", "prices", "stock", "orders", "plan" };

        // ============================== الرفع ==============================

        public List<PushResult> Push(SalesRepDeviceService.DeviceIdentity id, IEnumerable<PushDoc> docs)
        {
            var list = (docs ?? Enumerable.Empty<PushDoc>()).ToList();
            if (list.Count > 100) throw new InvalidOperationException("أقصى 100 مستند فى الدفعة.");
            var results = new List<PushResult>();
            foreach (var d in list) results.Add(PushOne(id, d));
            Atomic(opp => { SalesRepDeviceService.Touch(opp, id.DeviceId); return 0; });
            return results;
        }

        private PushResult PushOne(SalesRepDeviceService.DeviceIdentity id, PushDoc doc)
        {
            if (doc == null || doc.Doc_Guid == Guid.Empty) return new PushResult { Status = (int)SalesRepSyncStatus.Error, Error = "معرّف المستند مطلوب." };
            var existing = Read(opp => opp.db.SalesRep_Sync_Inboxes.AsNoTracking().FirstOrDefault(i => i.Doc_Guid == doc.Doc_Guid));
            if (existing != null) return ResultOf(existing, id.DeviceId);
            long inboxId;
            try
            {
                inboxId = Atomic(opp =>
                {
                    var row = new SalesRep_Sync_Inbox
                    {
                        Id = CallSnowflakeIdGenerate.GenerateNewId(), Device_Id = id.DeviceId, Rep_Id = id.RepId, Doc_Guid = doc.Doc_Guid, Doc_Type = doc.Doc_Type,
                        Payload = doc.Payload, Status = (int)SalesRepSyncStatus.Received, Received_At = DateTime.Now, Device_Created_At = doc.Created_At,
                    };
                    using (UserSessionContext.Use(SalesRepDeviceService.SessionOf(id))) row.StampInsert(id.BranchId);
                    opp.db.SalesRep_Sync_Inboxes.Add(row);
                    opp.db.SaveChanges();
                    return row.Id;
                });
            }
            catch (DbUpdateException)
            {
                // سباق: نفس المستند وصل من طلب متزامن.
                existing = Read(opp => opp.db.SalesRep_Sync_Inboxes.AsNoTracking().FirstOrDefault(i => i.Doc_Guid == doc.Doc_Guid));
                if (existing != null) return ResultOf(existing, id.DeviceId);
                throw;
            }
            var processed = Process(inboxId, id);
            return ResultOf(processed, id.DeviceId, duplicate: false);
        }

        public PushResult GetStatus(SalesRepDeviceService.DeviceIdentity id, Guid docGuid) => Read(opp =>
        {
            var row = opp.db.SalesRep_Sync_Inboxes.AsNoTracking().FirstOrDefault(i => i.Doc_Guid == docGuid && i.Device_Id == id.DeviceId);
            return row == null ? null : ResultOf(row, id.DeviceId, duplicate: false);
        });

        private static PushResult ResultOf(SalesRep_Sync_Inbox row, long deviceId, bool duplicate = true)
        {
            if (row.Device_Id != deviceId)
                return new PushResult { Doc_Guid = row.Doc_Guid, Status = (int)SalesRepSyncStatus.Error, Error = "معرّف المستند مستعمل من جهاز آخر.", Duplicate = true };
            return new PushResult { Doc_Guid = row.Doc_Guid, Status = row.Status, Error = row.Error, Result_Ref_Id = row.Result_Ref_Id, Duplicate = duplicate };
        }

        /// <summary>
        /// يعالج صف الصندوق: المستند وحالة الصف فى معاملة واحدة. الفشل يُرجع المعاملة ثم يسجّل الحالة والخطأ (ويطلب الموافقة إن لزمت).
        /// </summary>
        private SalesRep_Sync_Inbox Process(long inboxId, SalesRepDeviceService.DeviceIdentity id)
        {
            using (UserSessionContext.Use(SalesRepDeviceService.SessionOf(id)))
            {
                try
                {
                    return Atomic(opp =>
                    {
                        var row = opp.db.SalesRep_Sync_Inboxes.First(i => i.Id == inboxId);
                        long refId = Dispatch(opp, id, row);
                        row.Status = (int)SalesRepSyncStatus.Processed;
                        row.Result_Ref_Id = refId;
                        row.Error = null;
                        row.Processed_At = DateTime.Now;
                        row.Attempts++;
                        row.StampEdit();
                        opp.db.SaveChanges();
                        return row;
                    });
                }
                catch (Exception ex)
                {
                    var (status, message) = Classify(ex);
                    if (ex is SalesRepApprovalRequiredException)
                    {
                        try { Atomic(opp => { RequestApprovals(opp, id, opp.db.SalesRep_Sync_Inboxes.AsNoTracking().First(i => i.Id == inboxId)); return 0; }); }
                        catch (Exception aex) { message += " (تعذر إنشاء طلب الموافقة: " + aex.Message + ")"; }
                    }
                    return Atomic(opp =>
                    {
                        var row = opp.db.SalesRep_Sync_Inboxes.First(i => i.Id == inboxId);
                        row.Status = (int)status;
                        row.Error = SalesRepSaleService.Trim(message, 4000);
                        row.Processed_At = DateTime.Now;
                        row.Attempts++;
                        row.StampEdit();
                        opp.db.SaveChanges();
                        return row;
                    });
                }
            }
        }

        /// <summary>أخطاء العمل (موافقة/مخزون/وردية/حد) تنتظر المراجعة؛ البيانات التالفة والأعطال خطأ.</summary>
        public static (SalesRepSyncStatus Status, string Message) Classify(Exception ex)
        {
            if (ex is SalesRepApprovalRequiredException ar)
                return (SalesRepSyncStatus.PendingReview, "يحتاج موافقة المشرف: " + string.Join("؛ ", ar.Violations.Select(v => v.Message)));
            if (ex is InvalidOperationException) return (SalesRepSyncStatus.PendingReview, ex.Message);
            if (ex is JsonException || ex is FormatException || ex is ArgumentException || ex is NotSupportedException) return (SalesRepSyncStatus.Error, "بيانات المستند غير صحيحة: " + ex.Message);
            var inner = ex;
            while (inner.InnerException != null) inner = inner.InnerException;
            return (SalesRepSyncStatus.Error, inner.Message);
        }

        private static T Payload<T>(SalesRep_Sync_Inbox row) where T : class =>
            JsonConvert.DeserializeObject<T>(row.Payload ?? "") ?? throw new JsonException("المدخل فارغ.");

        /// <summary>يمرر المستند لخدمة الشاشة المقابلة (نفس الفحوص والقيود) ويرجع رقم المستند الناتج.</summary>
        private static long Dispatch(operations opp, SalesRepDeviceService.DeviceIdentity id, SalesRep_Sync_Inbox row)
        {
            switch ((SalesRepSyncDocType)row.Doc_Type)
            {
                case SalesRepSyncDocType.Sale:
                {
                    var input = Payload<DeviceSaleInput>(row);
                    var ctx = SalesRepSaleService.Context(opp, id.RepId);
                    var (invL, invD) = BuildSale(opp, ctx, input);
                    return new SalesRepSaleService(opp).SaveSale(id.RepId, row.Doc_Guid, invL, invD, paymentMethodId: input.Is_Credit ? 0 : input.Payment_Method_Id);
                }
                case SalesRepSyncDocType.Return:
                {
                    var input = Payload<SalesRepReturnService.ReturnInput>(row);
                    input.Doc_Guid = row.Doc_Guid;
                    return new SalesRepReturnService(opp).SaveReturn(id.RepId, input);
                }
                case SalesRepSyncDocType.Collection:
                {
                    var input = Payload<DeviceCollectionInput>(row);
                    return new SalesRepSaleService(opp).Collect(id.RepId, input.Customer_Id, input.Amount, input.Notes, input.Payment_Method_Id);
                }
                case SalesRepSyncDocType.TakeOrder:
                {
                    var input = Payload<SalesRepOrderService.TakeOrderInput>(row);
                    input.Doc_Guid = row.Doc_Guid;
                    return new SalesRepOrderService(opp).TakeOrder(id.RepId, input).Order_Id;
                }
                case SalesRepSyncDocType.Delivery:
                {
                    var input = Payload<SalesRepOrderService.DeliveryInput>(row);
                    input.Doc_Guid = row.Doc_Guid;
                    return new SalesRepOrderService(opp).Deliver(id.RepId, input).Delivery_Id;
                }
                case SalesRepSyncDocType.Cheque:
                {
                    var input = Payload<SalesRepChequeService.ChequeInput>(row);
                    input.Doc_Guid = row.Doc_Guid;
                    return new SalesRepChequeService(opp).Collect(id.RepId, input);
                }
                case SalesRepSyncDocType.NoSale:
                    return new SalesRepVisitService(opp).RecordNoSale(id.RepId, Payload<SalesRepVisitService.NoSaleInput>(row));
                default:
                    throw new NotSupportedException($"نوع مستند غير معروف ({row.Doc_Type}).");
            }
        }

        /// <summary>ينشئ طلبات الموافقة لمخالفات المستند (يراها المشرف فى «موافقات المندوبين»)، ثم «إعادة المعالجة» بعد القرار.</summary>
        private static void RequestApprovals(operations opp, SalesRepDeviceService.DeviceIdentity id, SalesRep_Sync_Inbox row)
        {
            const string reason = "مستند من جهاز المندوب";
            switch ((SalesRepSyncDocType)row.Doc_Type)
            {
                case SalesRepSyncDocType.Sale:
                {
                    var input = Payload<DeviceSaleInput>(row);
                    var ctx = SalesRepSaleService.Context(opp, id.RepId);
                    var (invL, invD) = BuildSale(opp, ctx, input);
                    var receipt = SalesRepPaymentService.Resolve(opp, ctx, input.Is_Credit ? 0 : input.Payment_Method_Id);
                    new SalesRepSaleService(opp).RequestApprovals(id.RepId, new SalesRepSaleService.SaleInput
                    {
                        Doc_Guid = row.Doc_Guid, Customer_Id = input.Customer_Id, Is_Credit = input.Is_Credit, Net_Total = Math.Round(invL.invL.Net_Total ?? 0, 2),
                        Lines = SalesRepSaleService.LinesOf(opp, invL, invD), Payment_Method_Id = receipt.MethodId,
                    }, reason);
                    break;
                }
                case SalesRepSyncDocType.Return:
                {
                    var input = Payload<SalesRepReturnService.ReturnInput>(row);
                    input.Doc_Guid = row.Doc_Guid;
                    new SalesRepReturnService(opp).RequestApprovals(id.RepId, input, reason);
                    break;
                }
                case SalesRepSyncDocType.TakeOrder:
                {
                    var input = Payload<SalesRepOrderService.TakeOrderInput>(row);
                    input.Doc_Guid = row.Doc_Guid;
                    new SalesRepOrderService(opp).RequestTakeApprovals(id.RepId, input, reason);
                    break;
                }
                case SalesRepSyncDocType.Delivery:
                {
                    var input = Payload<SalesRepOrderService.DeliveryInput>(row);
                    input.Doc_Guid = row.Doc_Guid;
                    new SalesRepOrderService(opp).RequestApprovals(id.RepId, input, reason);
                    break;
                }
                case SalesRepSyncDocType.Cheque:
                {
                    var input = Payload<SalesRepChequeService.ChequeInput>(row);
                    input.Doc_Guid = row.Doc_Guid;
                    new SalesRepChequeService(opp).RequestApprovals(id.RepId, input, reason);
                    break;
                }
            }
        }

        private class UnitInfo
        {
            public long Item_Id { get; set; }
            public long Store_Id { get; set; }
            public string Item_Name { get; set; } = "";
            public string Code { get; set; } = "";
            public string B_Unit { get; set; } = "";
            public string S_Unit { get; set; } = "";
            public decimal Factor { get; set; } = 1;
        }

        /// <summary>فاتورة بيع من أسطر الجهاز على مخزن السيارة بنفس حساب سطر الشاشة (InvoiceLineCalculator) ورأس تسليم الطلبية.</summary>
        internal static (InvoicesOperationListCLS, InvoicesOperationCLS) BuildSale(operations opp, SalesRepSaleService.SaleContext ctx, DeviceSaleInput input)
        {
            if (input.Customer_Id <= 0) throw new InvalidOperationException("اختر العميل.");
            if (input.Lines == null || input.Lines.Count == 0) throw new InvalidOperationException("الفاتورة بلا أصناف.");
            var invL = new InvoicesOperationListCLS();
            var invD = new InvoicesOperationCLS(0);
            bool anyTax = false;
            foreach (var l in input.Lines)
            {
                if (l.Qty_Large < 0 || l.Qty_Small < 0 || l.Qty_Large + l.Qty_Small <= 0) throw new ArgumentException("كمية سطر غير صحيحة.");
                if (l.Unit_Price < 0 || l.Discount_Value < 0 || l.Tax_Rate < 0 || l.Tax_Rate > 100) throw new ArgumentException("سعر أو خصم أو ضريبة سطر غير صحيحة.");
                var u = opp.db.Database.SqlQuery<UnitInfo>($@"
SELECT CAST(ISNULL(u.Item_id, 0) AS bigint) AS Item_Id, CAST(ISNULL(u.Store_id, 0) AS bigint) AS Store_Id, ISNULL(i.Item_Name, N'') AS Item_Name,
       ISNULL(i.code, N'') AS Code, ISNULL(un.B_Unit, N'') AS B_Unit, ISNULL(un.S_unit, N'') AS S_Unit,
       CAST(CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END AS decimal(18,6)) AS Factor
FROM Unit_item u LEFT JOIN Item i ON i.id = u.Item_id LEFT JOIN unit un ON un.id = u.unit_id
WHERE u.id = {l.Unit_Item_Id}").AsEnumerable().FirstOrDefault() ?? throw new ArgumentException("صنف غير موجود.");
                if (u.Store_Id != ctx.Van_Store_Id) throw new InvalidOperationException($"«{u.Item_Name}» ليس على مخزن سيارة المندوب.");
                decimal taxRate = Math.Round(l.Tax_Rate, 2);
                anyTax |= taxRate > 0;
                var d = new IInvoices_Details
                {
                    Item_Id = u.Item_Id, Unit_Item_Table_Id = l.Unit_Item_Id, Invoice_Item_Name = u.Item_Name, code = u.Code,
                    Store_Id = ctx.Van_Store_Id, Store_Name = ctx.Van_Store_Name, UnitStr = u.B_Unit, UnitSmallStr = u.S_Unit,
                    Unit_Used_for_Price = 1, Make_Order = 0, Unit_Item_ExpireDate_Id = 0, Bills_Details_Id = 0, Item_Expire_Date = DateTime.Now.Date,
                    Tash3ela = "", StatementNo = "", OperationNo = "", Merchant_Name = "", Specification = "", NameSpecification = "", Notes = "",
                    Item_Notes = SalesRepSaleService.Trim(l.Notes?.Trim() ?? "", 500),
                    Item_TaxSales10 = taxRate, Item_ATS05 = 0, Larg_Qnty = l.Qty_Large, Small_Qnty = l.Qty_Small, PriceUnit = l.Unit_Price, Larg_Unit_Price = l.Unit_Price,
                    Item_CommissionRate = 0, Item_CommissionValue = Math.Round(l.Discount_Value, 2), Item_DeletedFlage = 0, Item_ProcessedFlage = 0,
                };
                InvoiceLineCalculator.ComputeLine(d, u.Factor);
                invD.invDlst.Add(d);
            }
            SalesRepSaleService.FillSaleHeader(opp, ctx, invL, invD, input.Customer_Id, null,
                "بيع من جهاز المندوب" + (string.IsNullOrWhiteSpace(input.Notes) ? "" : " — " + input.Notes.Trim()), anyTax);
            SalesRepSaleService.FinishSaleHeader(opp, ctx, invL, invD, input.Is_Credit);
            return (invL, invD);
        }

        // ============================== المراجعة ==============================

        public List<InboxRow> GetInbox(int? status, long? repId = null, int take = 300) => Read(opp =>
        {
            int st = status ?? -1;
            long rep = repId ?? 0;
            var rows = opp.db.Database.SqlQuery<InboxRow>($@"
SELECT TOP ({take}) i.Id, i.Device_Id, ISNULL(NULLIF(d.Device_Name, N''), ISNULL(d.Platform, N'')) AS Device_Name, i.Rep_Id, ISNULL(e.Name, N'') AS Rep_Name,
       i.Doc_Guid, i.Doc_Type, i.Status, i.Error, i.Result_Ref_Id, i.Received_At, i.Processed_At, i.Device_Created_At, i.Attempts,
       ISNULL(NULLIF(u.NAME, N''), ISNULL(u.USER_NAME, N'')) AS Reviewed_By, i.Reviewed_At, i.Review_Notes, i.Payload,
       CAST(0 AS bigint) AS Customer_Id, N'' AS Customer_Name, CAST(0 AS decimal(18,2)) AS Amount, 0 AS Lines_Count
FROM SalesRep_Sync_Inbox i
LEFT JOIN SalesRep_Devices d ON d.Id = i.Device_Id
LEFT JOIN SalesRep_Reps r ON r.Id = i.Rep_Id LEFT JOIN Employee e ON e.Id = r.Employee_Id
LEFT JOIN USER_DETAILES u ON u.USER_ID = i.Reviewed_By_User_Id
WHERE ({st} = -1 OR i.Status = {st}) AND ({rep} = 0 OR i.Rep_Id = {rep})
ORDER BY i.Received_At DESC").ToList();
            foreach (var r in rows) Describe(r);
            var ids = rows.Select(r => r.Customer_Id).Where(x => x > 0).Distinct().ToList();
            if (ids.Count > 0)
            {
                var names = opp.db.Database.SqlQueryRaw<SalesRepDeviceService.Option>(
                    $"SELECT Id, ISNULL(Name, N'') AS Name FROM Customer WHERE Id IN ({string.Join(",", ids)})").ToList().ToDictionary(c => c.Id, c => c.Name);
                foreach (var r in rows) if (names.TryGetValue(r.Customer_Id, out var n)) r.Customer_Name = n;
            }
            return rows;
        });

        /// <summary>ملخص المدخل للعرض: العميل والمبلغ وعدد الأسطر (بلا فشل لو المدخل تالف).</summary>
        private static void Describe(InboxRow r)
        {
            try
            {
                var j = JObject.Parse(r.Payload ?? "{}");
                r.Customer_Id = (long?)j["Customer_Id"] ?? 0;
                var lines = j["Lines"] as JArray;
                r.Lines_Count = lines?.Count ?? 0;
                switch ((SalesRepSyncDocType)r.Doc_Type)
                {
                    case SalesRepSyncDocType.Collection:
                    case SalesRepSyncDocType.Cheque:
                        r.Amount = (decimal?)j["Amount"] ?? 0;
                        break;
                    case SalesRepSyncDocType.Sale:
                        r.Amount = Math.Round(lines?.Sum(l => ((decimal?)l["Qty_Large"] ?? 0) * ((decimal?)l["Unit_Price"] ?? 0) - ((decimal?)l["Discount_Value"] ?? 0)) ?? 0, 2);
                        break;
                    case SalesRepSyncDocType.TakeOrder:
                        r.Amount = Math.Round(lines?.Sum(l => (decimal?)l["Line_Total"] ?? 0) ?? 0, 2);
                        break;
                }
            }
            catch { }
        }

        /// <summary>إعادة معالجة مستند ينتظر المراجعة أو فيه خطأ (بعد قرار الموافقة أو تصحيح السبب) باسم الجهاز ولو محظوراً الآن.</summary>
        public PushResult Retry(long inboxId)
        {
            var (row, id) = Read(opp =>
            {
                var r = opp.db.SalesRep_Sync_Inboxes.AsNoTracking().FirstOrDefault(i => i.Id == inboxId) ?? throw new InvalidOperationException("المستند غير موجود.");
                if (r.Status is not ((int)SalesRepSyncStatus.PendingReview or (int)SalesRepSyncStatus.Error))
                    throw new InvalidOperationException("يُعاد فقط مستند ينتظر المراجعة أو فيه خطأ.");
                return (r, IdentityOf(opp, r.Device_Id));
            });
            var processed = Process(row.Id, id);
            Atomic(opp =>
            {
                var r = opp.db.SalesRep_Sync_Inboxes.First(i => i.Id == inboxId);
                r.Reviewed_By_User_Id = Common.User_Id; r.Reviewed_At = DateTime.Now;
                opp.db.SaveChanges();
                return 0;
            });
            return ResultOf(processed, processed.Device_Id, duplicate: false);
        }

        /// <summary>رفض يدوى بسبب إلزامى؛ لا أثر محاسبى (المستند لم يُحفظ أصلاً) ويُبلَّغ الجهاز بالحالة.</summary>
        public void Reject(long inboxId, string notes) => Atomic(opp =>
        {
            if (string.IsNullOrWhiteSpace(notes)) throw new InvalidOperationException("سبب الرفض مطلوب.");
            var r = opp.db.SalesRep_Sync_Inboxes.FirstOrDefault(i => i.Id == inboxId) ?? throw new InvalidOperationException("المستند غير موجود.");
            if (r.Status is not ((int)SalesRepSyncStatus.PendingReview or (int)SalesRepSyncStatus.Error))
                throw new InvalidOperationException("يُرفض فقط مستند ينتظر المراجعة أو فيه خطأ.");
            r.Status = (int)SalesRepSyncStatus.Rejected;
            r.Reviewed_By_User_Id = Common.User_Id; r.Reviewed_At = DateTime.Now; r.Review_Notes = SalesRepSaleService.Trim(notes.Trim(), 500);
            r.StampEdit();
            opp.db.SaveChanges();
            return 0;
        });

        /// <summary>هوية جهاز للمعالجة من المكتب (بلا توكن، ولو محظوراً: مستنداته حدثت قبل الحظر).</summary>
        private static SalesRepDeviceService.DeviceIdentity IdentityOf(operations opp, long deviceId)
        {
            var d = opp.db.SalesRep_Devices.AsNoTracking().FirstOrDefault(x => x.Id == deviceId) ?? throw new InvalidOperationException("الجهاز غير موجود.");
            var rep = opp.db.SalesRep_Reps.AsNoTracking().FirstOrDefault(r => r.Id == d.Rep_Id) ?? throw new InvalidOperationException("المندوب غير موجود.");
            long user = d.Act_As_User_Id ?? Common.User_Id;
            string userName = opp.db.Database.SqlQuery<string>($"SELECT ISNULL(USER_NAME, N'') AS Value FROM USER_DETAILES WHERE USER_ID = {user}").AsEnumerable().FirstOrDefault() ?? "";
            return new SalesRepDeviceService.DeviceIdentity(d.Id, d.Rep_Id, rep.Branch_Id ?? Common.Branch_Id, user, userName, "");
        }

        // ============================== التنزيل ==============================

        private record SyncRow(long Key, object Data);

        /// <param name="since">آخر نسخة طبّقها الجهاز لهذه المجموعة؛ فارغ أو لا يطابق = تنزيل كامل.</param>
        public PullResult Pull(SalesRepDeviceService.DeviceIdentity id, string dataset, long? since) => Atomic(opp =>
        {
            dataset = (dataset ?? "").Trim().ToLowerInvariant();
            if (!Datasets.Contains(dataset)) throw new ArgumentException("مجموعة بيانات غير معروفة: " + dataset);
            List<SyncRow> rows;
            using (UserSessionContext.Use(SalesRepDeviceService.SessionOf(id))) rows = Build(opp, id, dataset);
            var hashes = new Dictionary<long, long>(rows.Count);
            foreach (var r in rows) hashes[r.Key] = RowHash(r.Data);

            var state = opp.db.SalesRep_Device_Datasets.FirstOrDefault(s => s.Device_Id == id.DeviceId && s.Dataset == dataset);
            bool full = since == null || state == null || state.Version != since.Value;
            var result = new PullResult { Dataset = dataset, Full = full, Total = rows.Count };
            if (full) result.Rows = rows.Select(r => r.Data).ToList();
            else
            {
                var old = Decode(state.State);
                result.Rows = rows.Where(r => !old.TryGetValue(r.Key, out long h) || h != hashes[r.Key]).Select(r => r.Data).ToList();
                result.Deleted = old.Keys.Where(k => !hashes.ContainsKey(k)).OrderBy(k => k).ToList();
                if (result.Rows.Count == 0 && result.Deleted.Count == 0)
                {
                    result.Version = state.Version;
                    SalesRepDeviceService.Touch(opp, id.DeviceId);
                    return result;
                }
            }
            if (state == null)
            {
                state = new SalesRep_Device_Dataset { Id = CallSnowflakeIdGenerate.GenerateNewId(), Device_Id = id.DeviceId, Dataset = dataset };
                using (UserSessionContext.Use(SalesRepDeviceService.SessionOf(id))) state.StampInsert(id.BranchId);
                opp.db.SalesRep_Device_Datasets.Add(state);
            }
            state.Version = CallSnowflakeIdGenerate.GenerateNewId();
            state.Row_Count = rows.Count;
            state.State = Encode(hashes);
            state.Synced_At = DateTime.Now;
            state.Update_ServerDateTime = state.Synced_At;
            opp.db.SaveChanges();
            SalesRepDeviceService.Touch(opp, id.DeviceId);
            result.Version = state.Version;
            return result;
        });

        private static List<SyncRow> Build(operations opp, SalesRepDeviceService.DeviceIdentity id, string dataset)
        {
            long repId = id.RepId;
            switch (dataset)
            {
                case "profile":
                {
                    var ctx = SalesRepSaleService.Context(opp, repId);
                    var rep = opp.db.SalesRep_Reps.AsNoTracking().First(r => r.Id == repId);
                    return new List<SyncRow> { new SyncRow(repId, new
                    {
                        Id = repId, ctx.Rep_Name, ctx.Employee_Id, ctx.Branch_Id, rep.Sales_Mode, ctx.Van_Store_Id, ctx.Van_Store_Name, ctx.Damaged_Store_Id, ctx.Damaged_Store_Name,
                        ctx.Cash_Box_Id, ctx.Cash_Box_Name, ctx.Cash_On_Hand, ctx.Price_Type_List_ID, ctx.Max_Discount_Rate, ctx.Max_Cash_On_Hand, ctx.Allow_Credit_Sales, ctx.Allow_Blind_Return,
                        ctx.Return_Window_Days, rep.Allow_Cheque_Collection, rep.Max_Cheque_Due_Days, ctx.Shift_Id, ctx.Shift_Status,
                    }) };
                }
                case "payment_methods":
                    return new SalesRepPaymentService(opp).GetMethodsForRep(repId).Select(m => new SyncRow(m.Id, new { m.Id, m.Name, Is_Cash = m.Id == SalesRepShiftService.CashPaymentMethodId })).ToList();
                case "return_reasons":
                    return new SalesRepReturnService(opp).GetReasons().Select(r => new SyncRow(r.Id, new { r.Id, r.Name, r.Is_Damaged, r.Requires_Photo, r.Requires_Batch })).ToList();
                case "nosale_reasons":
                    return new SalesRepVisitService(opp).GetReasons().Select(r => new SyncRow(r.Id, new { r.Id, r.Name, r.Requires_Photo })).ToList();
                case "banks":
                    return new SalesRepChequeService(opp).GetBanks().Select(b => new SyncRow(b.Id, new { b.Id, b.Name })).ToList();
                case "customers":
                    return opp.db.Database.SqlQueryRaw<CustomerSync>(CustomersSql(repId)).ToList().Select(c => new SyncRow(c.Id, c)).ToList();
                case "items":
                {
                    long store = SalesRepSaleService.Context(opp, repId).Van_Store_Id;
                    return opp.db.Database.SqlQuery<ItemSync>($@"
SELECT u.id AS Id, CAST(ISNULL(u.Item_id, 0) AS bigint) AS Item_Id, ISNULL(i.Item_Name, N'') AS Item_Name, ISNULL(i.code, N'') AS Code,
       ISNULL(i.BarCode2, N'') AS BarCode2, ISNULL(un.B_Unit, N'') AS B_Unit, ISNULL(un.S_unit, N'') AS S_Unit,
       CAST(CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END AS decimal(18,6)) AS Factor,
       CAST(ISNULL(NULLIF(u.S_price_LargeUnit, 0), ISNULL(i.S_price_LargeUnit, 0)) AS decimal(18,4)) AS Price_Large,
       CAST(ISNULL(NULLIF(u.S_price_SmallUnit, 0), ISNULL(i.S_price_SmallUnit, 0)) AS decimal(18,4)) AS Price_Small,
       CAST(ISNULL(i.TaxSales10_Value, 0) AS decimal(9,4)) AS Tax_Rate
FROM Unit_item u JOIN Item i ON i.id = u.Item_id LEFT JOIN unit un ON un.id = u.unit_id
WHERE u.Store_id = {store} AND i.Active_flage = 1").ToList().Select(x => new SyncRow(x.Id, x)).ToList();
                }
                case "prices":
                {
                    long store = SalesRepSaleService.Context(opp, repId).Van_Store_Id;
                    // أنواع أسعار عملاء المندوب + نوع المندوب، لأصناف مخزن سيارته فقط.
                    var typeIds = opp.db.Database.SqlQueryRaw<CustomerSync>(CustomersSql(repId)).ToList().Select(c => c.Price_Type_List_ID).Where(t => t > 0).ToHashSet();
                    var rep = opp.db.SalesRep_Reps.AsNoTracking().First(r => r.Id == repId);
                    if (rep.Price_Type_List_ID is > 0) typeIds.Add(rep.Price_Type_List_ID.Value);
                    if (typeIds.Count == 0) return new List<SyncRow>();
                    return opp.db.Database.SqlQueryRaw<PriceSync>($@"
SELECT p.ID AS Id, CAST(p.Item_ID AS bigint) AS Item_Id, CAST(p.Price_Type_List_ID AS bigint) AS Price_Type_List_ID, CAST(ISNULL(p.PriceUnit, 0) AS decimal(18,4)) AS Price
FROM Price_Type_Sell_Values p
WHERE p.Price_Type_List_ID IN ({string.Join(",", typeIds)}) AND p.Item_ID IN (SELECT u.Item_id FROM Unit_item u WHERE u.Store_id = {store})").ToList()
                        .Select(x => new SyncRow(x.Id, x)).ToList();
                }
                case "stock":
                {
                    long store = SalesRepSaleService.Context(opp, repId).Van_Store_Id;
                    return opp.db.Database.SqlQuery<StockSync>($@"
SELECT u.id AS Id, CAST(ISNULL(u.Full_qua, 0) AS decimal(18,4)) AS Full_Qua, CAST(ISNULL(u.D_Qua, 0) AS decimal(18,4)) AS D_Qua
FROM Unit_item u WHERE u.Store_id = {store}").ToList().Select(x => new SyncRow(x.Id, x)).ToList();
                }
                case "orders":
                {
                    var svc = new SalesRepOrderService(opp);
                    return svc.GetRepOrders(repId).Select(o => new SyncRow(o.Id, new { Order = o, svc.GetDelivery(repId, o.Id).Lines })).ToList();
                }
                case "plan":
                    return new SalesRepVisitService(opp).PlanFor(repId, DateTime.Today).Select(p => new SyncRow(p.Customer_Id, p)).ToList();
            }
            throw new ArgumentException("مجموعة بيانات غير معروفة: " + dataset);
        }

        public class CustomerSync
        {
            public long Id { get; set; }
            public string Name { get; set; } = "";
            public string Code { get; set; } = "";
            public string Address { get; set; } = "";
            public string Mobile { get; set; } = "";
            public string Tel { get; set; } = "";
            public long Price_Type_List_ID { get; set; }
            public decimal Balance { get; set; }
            public decimal? Latitude { get; set; }
            public decimal? Longitude { get; set; }
            public int? Geofence_Radius_M { get; set; }
        }

        public class ItemSync
        {
            public long Id { get; set; }
            public long Item_Id { get; set; }
            public string Item_Name { get; set; } = "";
            public string Code { get; set; } = "";
            public string BarCode2 { get; set; } = "";
            public string B_Unit { get; set; } = "";
            public string S_Unit { get; set; } = "";
            public decimal Factor { get; set; }
            public decimal Price_Large { get; set; }
            public decimal Price_Small { get; set; }
            public decimal Tax_Rate { get; set; }
        }

        public class PriceSync
        {
            public long Id { get; set; }
            public long Item_Id { get; set; }
            public long Price_Type_List_ID { get; set; }
            public decimal Price { get; set; }
        }

        public class StockSync
        {
            public long Id { get; set; }
            public decimal Full_Qua { get; set; }
            public decimal D_Qua { get; set; }
        }

        /// <summary>
        /// عملاء المندوب: عملاء خطوطه النشطة؛ مندوب بلا خطوط = كل العملاء النشطين (بيع فورى بلا خط سير). الرصيد بنفس مصادر CustomerBalance.
        /// </summary>
        private static string CustomersSql(long repId) => $@"
SELECT c.Id AS Id, ISNULL(c.Name, N'') AS Name, ISNULL(CAST(c.Code AS nvarchar(50)), N'') AS Code, ISNULL(c.Address, N'') AS Address,
       ISNULL(c.Mobile, N'') AS Mobile, ISNULL(c.Tel, N'') AS Tel, CAST(ISNULL(c.Price_Type_List_ID, 0) AS bigint) AS Price_Type_List_ID,
       CAST(ISNULL((SELECT SUM(ISNULL(a.Export, 0) - ISNULL(a.Import, 0)) FROM Customers_Accounts a WHERE a.Customer_Id = c.Id AND a.Reference_Type <> 25), 0)
          + ISNULL((SELECT SUM(ISNULL(sa.Export, 0) - ISNULL(sa.Import, 0)) FROM Suppliers_Accounts sa JOIN Supplier sp ON sp.Id = sa.Supplier_Id
                    WHERE sp.Ref_Cust_Supplier_Id = c.Id AND sp.Ref_Cust_Supplier_Id > 0 AND sa.Reference_Type <> 25), 0) AS decimal(18,2)) AS Balance,
       g.Latitude, g.Longitude, g.Geofence_Radius_M
FROM Customer c LEFT JOIN SalesRep_Customer_Geo g ON g.Customer_Id = c.Id
WHERE c.active = 1 AND c.Customer_Type = {Common.Customer_Type}
  AND (NOT EXISTS (SELECT 1 FROM SalesRep_Routes r JOIN SalesRep_Route_Customers rc ON rc.Route_Id = r.Id WHERE r.Rep_Id = {repId} AND r.Active = 1)
       OR c.Id IN (SELECT rc.Customer_Id FROM SalesRep_Routes r JOIN SalesRep_Route_Customers rc ON rc.Route_Id = r.Id WHERE r.Rep_Id = {repId} AND r.Active = 1))";

        /// <summary>بصمة الصف: أول 8 بايت من SHA-256 لتمثيله JSON.</summary>
        private static long RowHash(object data)
        {
            using var sha = SHA256.Create();
            byte[] h = sha.ComputeHash(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)));
            return BitConverter.ToInt64(h, 0);
        }

        private static byte[] Encode(Dictionary<long, long> map)
        {
            var bytes = new byte[map.Count * 16];
            int i = 0;
            foreach (var kv in map.OrderBy(k => k.Key))
            {
                BitConverter.GetBytes(kv.Key).CopyTo(bytes, i);
                BitConverter.GetBytes(kv.Value).CopyTo(bytes, i + 8);
                i += 16;
            }
            return bytes;
        }

        private static Dictionary<long, long> Decode(byte[] bytes)
        {
            var map = new Dictionary<long, long>();
            if (bytes == null) return map;
            for (int i = 0; i + 16 <= bytes.Length; i += 16) map[BitConverter.ToInt64(bytes, i)] = BitConverter.ToInt64(bytes, i + 8);
            return map;
        }

        // ============================== المعاملات ==============================

        private T Read<T>(Func<operations, T> f) => f(_external ?? new operations());

        /// <summary>
        /// وحدة ذرية: معاملة جديدة لكل وحدة، أو نقطة حفظ داخل معاملة خارجية (الاختبارات) يُرجع لها عند الفشل فلا يفسد الباقى.
        /// </summary>
        private T Atomic<T>(Func<operations, T> f)
        {
            if (_external != null)
            {
                string sp = "sr" + Guid.NewGuid().ToString("N").Substring(0, 20);
                _external.tns.Save(sp);
                try { return f(_external); }
                catch
                {
                    try { _external.tns.Rollback(sp); } catch { }
                    _external.db.ChangeTracker.Clear();
                    throw;
                }
            }
            var opp = new operations(2);
            try
            {
                var r = f(opp);
                opp.Commit();
                return r;
            }
            catch
            {
                try { opp.Rollback(); } catch { }
                throw;
            }
        }
    }
}
