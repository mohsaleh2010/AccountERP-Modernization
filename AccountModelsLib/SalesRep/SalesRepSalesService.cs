using System;
using System.Linq;
using System.Collections.Generic;
using AccountModelsLib.SalesRep;
using AccountModelsLib.Models;

namespace AccountModelsLib.SalesRep
{
    public class SalesRepSalesService
    {
        private readonly operations _opp;

        public SalesRepSalesService(operations opp)
        {
            _opp = opp;
        }

        public long CreateFieldSaleInvoice(long shiftId, long customerId, List<Invoices_Details> lines, long paymentMethodId)
        {
            // 1. التحقق من الوردية
            var shift = _opp.db.SalesRep_Shifts.FirstOrDefault(s => s.Id == shiftId && s.Status == (int)SalesRepShiftStatus.Open);
            if (shift == null) throw new InvalidOperationException("الوردية غير مفتوحة.");

            // 2. التحقق من المندوب والسياسات
            var rep = _opp.db.SalesRep_Reps.FirstOrDefault(r => r.Id == shift.Rep_Id);

            decimal invoiceTotal = lines.Sum(l => l.Total ?? 0);
            if (!ValidateCredit(customerId, invoiceTotal))
                throw new InvalidOperationException("تم تجاوز حد الائتمان للعميل.");

            // 3. إنشاء الفاتورة عبر المحرك (Account_Entry_Invoice_CLS.Make_EntryForInvoices)
            // يتم استدعاء المحرك الحقيقي من هنا عند الربط الكامل؛ حالياً يُعيد رقماً جديداً
            // ويُسجل ربط الوثيقة بالوردية — نفس نمط RouteToInvoiceEngine فى OfflineSyncService
            long invoiceId = CallSnowflakeIdGenerate.GenerateNewId();

            // 4. تسجيل الربط بالوردية
            var link = new SalesRep_Document_Link
            {
                Id = CallSnowflakeIdGenerate.GenerateNewId(),
                Doc_Type = (int)SalesRepDocType.Invoice,
                Doc_Id = invoiceId,
                Shift_Id = shiftId
            };
            _opp.db.SalesRep_Document_Links.Add(link);
            _opp.db.SaveChanges();

            return invoiceId;
        }

        // طريقة جديدة متوافقة — لا تلامس الكود الحالي للفواتير ولا تعدل على Invoices_List/Invoices_Details مباشرة
        public long CreateFieldSaleInvoiceDirect(long shiftId, long customerId, List<Invoices_Details> lines, long paymentMethodId)
        {
            // التحقق من الوردية (نفس النمط)
            var shift = _opp.db.SalesRep_Shifts.FirstOrDefault(s => s.Id == shiftId && s.Status == (int)SalesRepShiftStatus.Open);
            if (shift == null) throw new InvalidOperationException("الوردية غير مفتوحة.");

            // إنشاء الفاتورة عبر المحرك الجديد (Method جديد متوافق مع قواعد الفواتير دون تعديل على الكود الحالي)
            int excp = 0;
            string msg = "";
            long invoiceId = CallSnowflakeIdGenerate.GenerateNewId();

            var engine = new Account_Entry_Invoice_CLS();
            // استخدام المحرك الحقيقي عبر Make_EntryForInvoices مع مرجع جديد
            engine.Make_EntryForInvoices(ref excp, ref msg, ref _opp, invoiceId, DateTime.Now, DateTime.Now, 0);
            if (excp != 0) throw new InvalidOperationException($"فشل المحرك: {msg}");

            // تسجيل الربط بالوردية فقط — لا تعديل على جداول الفواتير الحالية
            var link = new SalesRep_Document_Link
            {
                Id = CallSnowflakeIdGenerate.GenerateNewId(),
                Doc_Type = (int)SalesRepDocType.Invoice,
                Doc_Id = invoiceId,
                Shift_Id = shiftId,
                User_Id = (int?)Common.User_Id,
                OperationDate = DateTime.Now,
                Insert_ServerDateTime = DateTime.Now,
                Branch_Id = (int?)Common.Branch_Id
            };
            _opp.db.SalesRep_Document_Links.Add(link);
            _opp.db.SaveChanges();
            return invoiceId;
        }

        public bool ValidateCredit(long customerId, decimal invoiceTotal)
        {
            var policy = _opp.db.SalesRep_Credit_Policies.FirstOrDefault(p => p.Customer_Id == customerId);
            if (policy == null) return true;

            decimal currentBalance = CalculateCustomerBalance(customerId);
            return (currentBalance + invoiceTotal) <= policy.Credit_Limit;
        }

        public bool ValidateDiscount(long repId, long scopeId, int scope, decimal discountRate, out decimal approvalRequiredAbove)
        {
            var policy = _opp.db.SalesRep_Discount_Policies.FirstOrDefault(p => p.Scope == scope && p.Scope_Id == scopeId);
            approvalRequiredAbove = policy?.Requires_Approval_Above ?? 0;

            if (policy != null && discountRate > policy.Max_Discount_Rate)
                return false;

            return discountRate <= (policy?.Requires_Approval_Above ?? 0);
        }

        private decimal CalculateCustomerBalance(long customerId)
        {
            // تنفيذ بسيط: الاعتماد على الإجمالي من الفواتير (لتحسين لاحقاً بالتكامل مع السجلات المحاسبية)
            return _opp.db.Invoices_List
                .Where(i => i.Customer_Id == customerId)
                .Sum(i => i.Net_Total ?? 0);
        }
    }
}
