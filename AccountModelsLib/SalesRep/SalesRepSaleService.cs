using System;
using System.Collections.Generic;
using System.Linq;
using AccountERP.SharedUI.Services;
using AccountModelsLib.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib.SalesRep
{
    /// <summary>
    /// خطة المندوبين — المرحلة 3: البيع الفورى والتحصيل من المكتب/التابلت والموافقات (§6.3، §4.3).
    /// - البيع = فاتورة مبيعات عادية بمحرك الفاتورة الحالى (Make_Invoice_Update_CustomerAccountInTx): مخزن = السيارة، صندوق = العهدة،
    ///   Salesman_Id = موظف المندوب، فى معاملة واحدة مع الفحوص وربط الوردية. القيود والإذن النقدى من المحرك نفسه (§7)؛ هذه الخدمة لا تكتب قيداً.
    /// - الفحوص: وردية مفتوحة (لا بيع وهى قيد الإقفال أو مقفلة)، كل الأسطر من مخزن السيارة، الكمية ≤ رصيد السيارة المحسوب من الدفتر
    ///   (منع قاطع)، والخصم وحد الائتمان والتأخر = طلب موافقة يعتمده المشرف ثم يُستعمل مرة واحدة لنفس المستند وبحده.
    /// - التحصيل = إذن استلام نقدية من العميل على صندوق العهدة مربوط بالوردية.
    /// الربط بالوردية برأس المستند (Invoices_List.Id / Order_Money.Id) لا بمعرّف سطر الخزنة (قرار المستخدم 2026-10-16، بند 32).
    /// </summary>
    public class SalesRepSaleService
    {
        private readonly operations _external;
        public SalesRepSaleService(operations external = null) { _external = external; }

        public class SaleContext
        {
            public long Rep_Id { get; set; }
            public long Employee_Id { get; set; }
            public string Rep_Name { get; set; } = "";
            public long Branch_Id { get; set; }
            public long? Shift_Id { get; set; }
            public int? Shift_Status { get; set; }
            public long Van_Store_Id { get; set; }
            public string Van_Store_Name { get; set; } = "";
            public long Cash_Box_Id { get; set; }
            public string Cash_Box_Name { get; set; } = "";
            public long? Price_Type_List_ID { get; set; }
            public decimal? Max_Discount_Rate { get; set; }
            public decimal? Max_Cash_On_Hand { get; set; }
            public bool Allow_Credit_Sales { get; set; }
            public decimal Cash_On_Hand { get; set; }
            public long Damaged_Store_Id { get; set; }
            public string Damaged_Store_Name { get; set; } = "";
            public bool Allow_Blind_Return { get; set; }
            public int Return_Window_Days { get; set; }
            public bool CanSell => Shift_Status is (int)SalesRepShiftStatus.Open or (int)SalesRepShiftStatus.Reopened;
        }

        /// <summary>سطر للفحص: الكمية بالوحدة الصغرى (المباع + المجانى) ونسبة الخصم الفعلية على السطر.</summary>
        public class SaleLine
        {
            public long Unit_Item_Id { get; set; }
            public long Store_Id { get; set; }
            public decimal Qty_Small { get; set; }
            public decimal Discount_Rate { get; set; }
        }

        public class SaleInput
        {
            public Guid Doc_Guid { get; set; }
            public long Customer_Id { get; set; }
            public bool Is_Credit { get; set; }
            public decimal Net_Total { get; set; }
            public List<SaleLine> Lines { get; set; } = new();
            /// <summary>تسليم طلبية (المرحلة 5): السعر والخصم متفق عليهما فى الطلبية فلا يُفحص الخصم على سقوف المندوب.</summary>
            public bool Order_Priced { get; set; }
            /// <summary>أخذ طلبية (المرحلة 5): الأسطر من المخزن المصدر لا السيارة، ونقص الكمية تنبيه لا منع، والدفع يتحدد عند التسليم.</summary>
            public bool Order_Taking { get; set; }
            public long Source_Store_Id { get; set; }
            /// <summary>طريقة الدفع للبيع غير الآجل (0/1 = نقدى فى العهدة)؛ غير النقدى يدخل بنك الطريقة فلا يُحسب على حد النقدية.</summary>
            public long Payment_Method_Id { get; set; }
            public bool Is_Cash_Payment => Payment_Method_Id <= 0 || Payment_Method_Id == SalesRepShiftService.CashPaymentMethodId;
        }

        /// <summary>مخالفة تحتاج موافقة: النوع، والقيمة المطلوبة، والحد المسموح، ورقم الموافقة إن طُلبت، وحالتها.</summary>
        public class Violation
        {
            public SalesRepApprovalType Type { get; set; }
            public string Message { get; set; } = "";
            public decimal Requested { get; set; }
            public decimal Allowed { get; set; }
            public long? Unit_Item_Id { get; set; }
            public long? Approval_Id { get; set; }
            public int? Approval_Status { get; set; }
            public bool Covered => Approval_Status == (int)SalesRepApprovalStatus.Approved;
        }

        public class CheckResult
        {
            public List<Violation> Violations { get; set; } = new();
            public List<string> Warnings { get; set; } = new();
            public decimal Customer_Balance { get; set; }
            public decimal? Credit_Limit { get; set; }
            public bool Ok => Violations.All(v => v.Covered);
        }

        /// <summary>مستند مربوط بالوردية للعرض.</summary>
        public class ShiftDoc
        {
            public int Doc_Type { get; set; }
            public long Doc_Id { get; set; }
            public long Doc_No { get; set; }
            public DateTime? Doc_Date { get; set; }
            public string Party { get; set; } = "";
            public decimal Value { get; set; }
            public int Is_Credit { get; set; }
        }

        public class ApprovalRow
        {
            public long Id { get; set; }
            public int Type { get; set; }
            public int Status { get; set; }
            public long? Rep_Id { get; set; }
            public string Rep_Name { get; set; } = "";
            public long? Customer_Id { get; set; }
            public string Customer_Name { get; set; } = "";
            public decimal? Requested_Value { get; set; }
            public decimal? Allowed_Value { get; set; }
            public decimal? Approved_Value { get; set; }
            public string Reason { get; set; }
            public string Decision_Notes { get; set; }
            public DateTime? Insert_ServerDateTime { get; set; }
            public DateTime? Decided_At { get; set; }
            public long? Ref_Doc_Id { get; set; }
        }

        // ============================== السياق والفحص ==============================

        public SaleContext GetContext(long repId) => Read(opp => Context(opp, repId));

        /// <summary>
        /// يفحص البيع بلا حفظ. يرمى استثناء للمنع القاطع (لا وردية مفتوحة، سطر من مخزن آخر، كمية أكبر من رصيد السيارة)،
        /// ويرجع المخالفات التى تحتاج موافقة مع حالة موافقة كل منها لهذا المستند (Doc_Guid).
        /// </summary>
        public CheckResult Check(long repId, SaleInput input) => Read(opp => CheckCore(opp, Context(opp, repId), input));

        /// <summary>
        /// تنبيه حد النقدية بالعهدة (قرار المستخدم 2026-10-17: تحذير + تنبيه للمشرف بلا منع) قبل أى قبض نقدى: بيع، تحصيل، تسليم، عربون.
        /// يرجع قائمة فارغة لو لا حد أو لن يُتجاوز.
        /// </summary>
        public List<string> CashWarnings(long repId, decimal cashIn) => Read(opp =>
        {
            var w = CashLimitWarning(Context(opp, repId), cashIn);
            return w == null ? new List<string>() : new List<string> { w };
        });

        public static string CashLimitWarning(SaleContext ctx, decimal cashIn)
        {
            if (ctx.Max_Cash_On_Hand is not decimal maxCash || cashIn < 0) return null;
            decimal after = ctx.Cash_On_Hand + cashIn;
            if (after <= maxCash) return null;
            return $"النقدية بالعهدة بعد العملية {after:N2} تتجاوز الحد {maxCash:N2}؛ ورِّد النقدية (توريد جزئى من شاشة الوردية).";
        }

        /// <summary>يطلب موافقة لكل مخالفة ليس لها طلب قائم لهذا المستند ولا موافقة تغطى قيمتها. يرجع الفحص بعد الطلب.</summary>
        public CheckResult RequestApprovals(long repId, SaleInput input, string reason = null, SalesRepDocType docType = SalesRepDocType.SaleInvoice) => Tx(opp =>
        {
            var ctx = Context(opp, repId);
            var result = CheckCore(opp, ctx, input);
            foreach (var v in result.Violations.Where(v => v.Approval_Status != (int)SalesRepApprovalStatus.Pending && !v.Covered))
            {
                var a = new SalesRep_Approval
                {
                    Id = CallSnowflakeIdGenerate.GenerateNewId(),
                    Type = (int)v.Type,
                    Ref_Document_Guid = input.Doc_Guid,
                    Ref_Doc_Type = (int)docType,
                    Requested_By = Common.User_Id,
                    Status = (int)SalesRepApprovalStatus.Pending,
                    Reason = Trim((string.IsNullOrWhiteSpace(reason) ? "" : reason.Trim() + " — ") + v.Message, 1000),
                    Rep_Id = ctx.Rep_Id,
                    Shift_Id = ctx.Shift_Id,
                    Customer_Id = input.Customer_Id,
                    Requested_Value = v.Requested,
                    Allowed_Value = v.Allowed,
                };
                a.StampInsert(ctx.Branch_Id);
                opp.db.SalesRep_Approvals.Add(a);
            }
            opp.db.SaveChanges();
            return CheckCore(opp, ctx, input);
        });

        // ============================== البيع ==============================

        /// <summary>
        /// حفظ فاتورة البيع الفورى: يفرض مخزن السيارة وصندوق العهدة والمندوب والفرع على المستند، يعيد الفحص داخل المعاملة،
        /// يحفظ بمحرك الفاتورة، ثم يربط الفاتورة وإذن نقديتها بالوردية ويعلّم الموافقات المستعملة. يرجع رقم الفاتورة (Invoices_List.Id).
        /// </summary>
        /// <param name="prepaid">عربون طلبية يُسوَّى على الفاتورة (المرحلة 5): الفاتورة آجلة والفحص على (الصافى − العربون).</param>
        /// <param name="collectNow">مع العربون: الباقى يُحصَّل نقداً الآن (لا فحص ائتمان)، وإلا يبقى على الحساب.</param>
        /// <param name="orderPriced">فاتورة تسليم طلبية بسعرها المتفق عليه: لا فحص للخصم.</param>
        /// <param name="paymentMethodId">طريقة دفع البيع غير الآجل: النقدى (0/1) فى صندوق العهدة، وغيره فى بنك الطريقة والسطر يحمل الطريقة.</param>
        public long SaveSale(long repId, Guid docGuid, InvoicesOperationListCLS invL_O, InvoicesOperationCLS invD_O,
            decimal prepaid = 0, bool collectNow = false, bool orderPriced = false, long paymentMethodId = 0) => Tx(opp =>
        {
            var ctx = Context(opp, repId);
            if (!ctx.CanSell) throw new InvalidOperationException("لا وردية مفتوحة للمندوب؛ البيع يتم داخل وردية مفتوحة فقط.");
            if (invL_O?.invL == null || invD_O?.invDlst == null || invD_O.invDlst.Count == 0) throw new InvalidOperationException("الفاتورة بلا أصناف.");
            if ((invL_O.invL.Id) != 0) throw new InvalidOperationException("فاتورة المندوب تُنشأ جديدة فقط؛ التصحيح بمرتجع.");
            if (docGuid == Guid.Empty) throw new InvalidOperationException("معرّف المستند مطلوب.");

            var h = invL_O.invL;
            h.Salesman_Id = ctx.Employee_Id;
            h.Branch_Id = ctx.Branch_Id;
            bool credit = (h.Is_Invoice_Agile ?? 0) == 1;
            var receipt = SalesRepPaymentService.Resolve(opp, ctx, credit ? 0 : paymentMethodId);
            h.BanksId = credit ? 0 : receipt.BoxId;
            foreach (var d in invD_O.invDlst)
            {
                d.Store_Id = ctx.Van_Store_Id;
                d.Store_Name = ctx.Van_Store_Name;
            }

            var input = new SaleInput
            {
                Doc_Guid = docGuid, Customer_Id = h.Customer_Id ?? 0, Is_Credit = credit && !(prepaid > 0 && collectNow),
                Net_Total = Math.Max(0, (h.Net_Total ?? 0) - prepaid), Lines = LinesOf(opp, invL_O, invD_O), Order_Priced = orderPriced,
                Payment_Method_Id = receipt.MethodId,
            };
            var check = CheckCore(opp, ctx, input);
            if (!check.Ok)
                throw new SalesRepApprovalRequiredException(check.Violations.Where(v => !v.Covered).ToList());

            int excp = 0;
            string msg = "";
            new Requests_ModelCls().Make_Invoice_Update_CustomerAccountInTx(ref excp, ref msg, ref opp, invL_O, invD_O);
            long invoiceId = h.Id;
            if (excp != 0 || invoiceId <= 0) throw new InvalidOperationException(string.IsNullOrWhiteSpace(msg) ? "تعذر حفظ الفاتورة." : msg);

            Link(opp, ctx.Shift_Id.Value, SalesRepDocType.SaleInvoice, invoiceId, ctx.Branch_Id);
            // البيع النقدى يقبض فى صندوق العهدة (وغير النقدى فى بنك الطريقة): لو لم ينشئ المحرك إذن الفاتورة (MakeOrderMoneyFromInvoiceAutomaticWithoutForm = 0
            // حيث الديسكتوب يفتح شاشة الإذن) يُنشأ إذن استلام مربوط بالفاتورة بنفس مسار «إذن نقدية مرتبط بفاتورة» (نوع العملية + رقم الفاتورة).
            decimal net = Math.Round(h.Net_Total ?? 0, 2);
            if (!credit && net > 0 && VouchersOfInvoice(opp, invoiceId).Count == 0)
            {
                var vouchers = new CashVouchersService();
                vouchers.SavePartyVoucherInTx(opp, 1, 0, receipt.BoxId, (h.Invoice_Date ?? DateTime.Today).Date, 0,
                    (receipt.IsCash ? "بيع نقدى للمندوب " : "بيع " + receipt.MethodName + " للمندوب ") + ctx.Rep_Name,
                    net, h.Customer_Id ?? 0, vouchers.GetFactor(SalesRepPaymentService.CurrencyOf(opp, receipt.BoxId)), Common.Invoice_Type_Id, invoiceId,
                    receipt.MethodId, receipt.MethodName);
            }
            if (!credit) SalesRepPaymentService.TagVouchers(opp, VouchersOfInvoice(opp, invoiceId), receipt);
            var docs = new List<(SalesRepDocType, long)> { (SalesRepDocType.SaleInvoice, invoiceId) };
            foreach (long orderMoneyId in VouchersOfInvoice(opp, invoiceId))
            {
                Link(opp, ctx.Shift_Id.Value, SalesRepDocType.CashVoucher, orderMoneyId, ctx.Branch_Id);
                docs.Add((SalesRepDocType.CashVoucher, orderMoneyId));
            }
            SalesRepVisitService.Attach(opp, ctx.Shift_Id.Value, h.Customer_Id ?? 0, SalesRepVisitResult.Sale, docs.ToArray());

            var used = opp.db.SalesRep_Approvals.Where(a => a.Ref_Document_Guid == docGuid && a.Status == (int)SalesRepApprovalStatus.Approved && a.Ref_Doc_Id == null).ToList();
            foreach (var a in used) { a.Ref_Doc_Id = invoiceId; a.Shift_Id ??= ctx.Shift_Id; a.StampEdit(); }
            opp.db.SaveChanges();
            return invoiceId;
        });

        /// <summary>
        /// رأس فاتورة بيع المندوب من أسطر محسوبة (مشترك بين تسليم الطلبية وبيع الجهاز): العميل وحسابه، العملة (المعطاة أو عملة صندوق العهدة)،
        /// التواريخ، المستخدم والفرع والمندوب، أعلام الضريبة على مستوى السطر، ثم الإجماليات. الآجل والرقم والرصيد فى FinishSaleHeader.
        /// </summary>
        internal static void FillSaleHeader(operations opp, SaleContext ctx, InvoicesOperationListCLS invL, InvoicesOperationCLS invD,
            long customerId, long? currencyId, string notes, bool anyTax)
        {
            var h = invL.invL;
            var customer = opp.db.Database.SqlQuery<SalesRepReturnService.CustomerRow>($"SELECT Name, CAST(ISNULL(SubAccount_Id, 0) AS bigint) AS SubAccount_Id FROM Customer WHERE Id = {customerId}")
                .AsEnumerable().FirstOrDefault() ?? throw new InvalidOperationException("العميل غير موجود.");
            DateTime now = DateTime.Now;
            h.Invoice_Type = 0; h.Is_InvoiceKta3y = 0; h.IS_Make_Sarf_Item_BalanceFromInvoice = 1;
            h.Customer_Id = customerId; h.CustomerName = customer.Name ?? ""; h.SubAccount_Name = h.CustomerName; h.SubAccount_Id = customer.SubAccount_Id;
            h.Currency_id = currencyId ?? opp.db.Database.SqlQuery<long>($"SELECT CAST(ISNULL(currency_Id, 1) AS bigint) AS Value FROM Banks WHERE Id = {ctx.Cash_Box_Id}").AsEnumerable().FirstOrDefault();
            h.Currency = "";
            h.Invoice_Date = now; h.Invoice_DateTime = now; h.InvoiceDueDate = now; h.Insert_ServerDateTime = now; h.OperationDate = now.Date;
            h.Notes = Trim(notes, 500);
            h.User_Id = Common.User_Id; h.Branch_Id = ctx.Branch_Id; h.Salesman_Id = ctx.Employee_Id;
            h.PolicaNo = "0"; h.troodContainerNo = "0"; h.Customer_Notes = ""; h.Emp_Name = ""; h.ShipmentCompany_Name = "";
            h.Emp_Id = 0; h.ShipmentCompany_Id = 0; h.Shipment_Value = 0; h.AnotherPayment = 0; h.IS_Posted_Flage = 0;
            h.IsInvoiceTaxInfo = anyTax ? 1 : 0; h.Is_Tax_Per_Item = anyTax ? 1 : 0; h.TaxSales10 = 0; h.ATS05 = 0;
            InvoiceLineCalculator.ComputeHeaderTotals(h, invD.invDlst, anyTax);
        }

        /// <summary>يكمل الرأس بعد الإجماليات: آجل/نقدى، رقم الفاتورة التالى، رصيد العميل قبل وبعد، وتصفير القيم الفارغة.</summary>
        internal static void FinishSaleHeader(operations opp, SaleContext ctx, InvoicesOperationListCLS invL, InvoicesOperationCLS invD, bool agile)
        {
            var h = invL.invL;
            decimal net = Math.Round(h.Net_Total ?? 0, 2);
            h.Is_Invoice_Agile = agile ? 1 : 0;
            h.Invoice_Id = SalesRepOrderService.NextInvoiceNo(opp, ctx.Branch_Id, h.Invoice_Date ?? DateTime.Now);
            decimal before = CustomerBalance(opp, h.Customer_Id ?? 0);
            h.CustomerAccountBefor = before; h.CustomerAccountAfter = before + net;
            invL.Invoice_Type_Id = Common.Invoice_Type_Id;
            InvoiceLineCalculator.ZeroNullNumerics(h);
            foreach (var d in invD.invDlst) InvoiceLineCalculator.ZeroNullNumerics(d);
        }

        // ============================== التحصيل ==============================

        /// <summary>
        /// تحصيل من عميل مربوط بالوردية المفتوحة: النقدى فى صندوق العهدة، وغير النقدى فى بنك الطريقة والسطر يحمل الطريقة.
        /// يرجع رقم الإذن (Order_Money.Id).
        /// </summary>
        public long Collect(long repId, long customerId, decimal amount, string notes = null, long paymentMethodId = 0) => Tx(opp =>
        {
            var ctx = Context(opp, repId);
            if (!ctx.CanSell) throw new InvalidOperationException("لا وردية مفتوحة للمندوب؛ التحصيل يتم داخل وردية مفتوحة فقط.");
            if (customerId <= 0) throw new InvalidOperationException("اختر العميل.");
            if (amount <= 0) throw new InvalidOperationException("المبلغ يجب أن يكون أكبر من صفر.");
            var receipt = SalesRepPaymentService.Resolve(opp, ctx, paymentMethodId);
            var vouchers = new CashVouchersService();
            string text = string.IsNullOrWhiteSpace(notes) ? (receipt.IsCash ? "تحصيل المندوب " : "تحصيل " + receipt.MethodName + " — المندوب ") + ctx.Rep_Name : notes.Trim();
            var v = vouchers.SavePartyVoucherInTx(opp, 1, 0, receipt.BoxId, DateTime.Today, 0, text,
                Math.Round(amount, 2), customerId, vouchers.GetFactor(SalesRepPaymentService.CurrencyOf(opp, receipt.BoxId)), 0, 0, receipt.MethodId, receipt.MethodName);
            Link(opp, ctx.Shift_Id.Value, SalesRepDocType.CashVoucher, v.OrderId, ctx.Branch_Id);
            SalesRepVisitService.Attach(opp, ctx.Shift_Id.Value, customerId, SalesRepVisitResult.Collection, (SalesRepDocType.CashVoucher, v.OrderId));
            SalesRepNotifyService.Enqueue(opp, SalesRepNotifyEvent.PaymentReceived, customerId, repId, v.OrderId, null, Math.Round(amount, 2));
            return v.OrderId;
        });

        /// <summary>فواتير الوردية ومرتجعاتها (بالسالب) وإذوناتها النقدية وشيكاتها المحصّلة للعرض.</summary>
        /// التاريخ = وقت ربط المستند بالوردية (تاريخ إذن النقدية مخزَّن بلا وقت فكان يظهر 00:00)، وإلا تاريخ المستند.
        public List<ShiftDoc> GetShiftDocs(long shiftId) => Read(opp => opp.db.Database.SqlQuery<ShiftDoc>($@"
SELECT l.Doc_Type, l.Doc_Id, CAST(ISNULL(i.Invoice_Id, 0) AS bigint) AS Doc_No, ISNULL(l.Insert_ServerDateTime, i.Invoice_Date) AS Doc_Date, ISNULL(c.Name, N'') AS Party,
       CAST(ISNULL(i.Net_Total, 0) AS decimal(18,2)) AS Value, ISNULL(i.Is_Invoice_Agile, 0) AS Is_Credit
FROM SalesRep_Document_Links l JOIN Invoices_List i ON i.Id = l.Doc_Id LEFT JOIN Customer c ON c.Id = i.Customer_Id
WHERE l.Shift_Id = {shiftId} AND l.Doc_Type = {(int)SalesRepDocType.SaleInvoice}
UNION ALL
SELECT l.Doc_Type, l.Doc_Id, CAST(ISNULL(r.Invoice_Id, 0) AS bigint), ISNULL(l.Insert_ServerDateTime, r.Invoice_Date), ISNULL(c.Name, N''),
       -CAST(ISNULL(r.Net_Total, 0) AS decimal(18,2)), ISNULL(r.Is_Invoice_Agile, 0)
FROM SalesRep_Document_Links l JOIN Invoices_List_Reverse r ON r.Id = l.Doc_Id LEFT JOIN Customer c ON c.Id = r.Customer_Id
WHERE l.Shift_Id = {shiftId} AND l.Doc_Type = {(int)SalesRepDocType.SaleReturn}
UNION ALL
SELECT l.Doc_Type, l.Doc_Id, CAST(ISNULL(o.Order_No, 0) AS bigint), ISNULL(l.Insert_ServerDateTime, o.Order_Date), ISNULL(o.Man_Name, N''),
       CAST(ISNULL(o.Total_Value, 0) AS decimal(18,2)) * CASE WHEN o.Transaction_type = 2 THEN -1 ELSE 1 END, 0
FROM SalesRep_Document_Links l JOIN Order_Money o ON o.Id = l.Doc_Id
WHERE l.Shift_Id = {shiftId} AND l.Doc_Type = {(int)SalesRepDocType.CashVoucher}
UNION ALL
SELECT l.Doc_Type, l.Doc_Id, CAST(ISNULL(o.Order_No, 0) AS bigint), l.Insert_ServerDateTime, ISNULL(c.Name, N''),
       CAST(ISNULL(m.Check_Value, 0) AS decimal(18,2)), 0
FROM SalesRep_Document_Links l JOIN Checks_Memory m ON m.Id = l.Doc_Id LEFT JOIN Order_Money_Check o ON o.Id = m.Order_Mony_Id LEFT JOIN Customer c ON c.Id = m.Customer_Id
WHERE l.Shift_Id = {shiftId} AND l.Doc_Type = {(int)SalesRepDocType.Cheque}
ORDER BY Doc_Date DESC").ToList());

        // ============================== الموافقات ==============================

        public List<ApprovalRow> GetApprovals(bool pendingOnly, int take = 200) => Read(opp => opp.db.Database.SqlQuery<ApprovalRow>($@"
SELECT TOP ({take}) a.Id, a.Type, a.Status, a.Rep_Id, ISNULL(e.name, N'') AS Rep_Name, a.Customer_Id, ISNULL(c.Name, N'') AS Customer_Name,
       a.Requested_Value, a.Allowed_Value, a.Approved_Value, a.Reason, a.Decision_Notes, a.Insert_ServerDateTime, a.Decided_At, a.Ref_Doc_Id
FROM SalesRep_Approvals a
LEFT JOIN SalesRep_Reps r ON r.Id = a.Rep_Id LEFT JOIN Employee e ON e.Id = r.Employee_Id LEFT JOIN Customer c ON c.Id = a.Customer_Id
WHERE {(pendingOnly ? 1 : 0)} = 0 OR a.Status = {(int)SalesRepApprovalStatus.Pending}
ORDER BY CASE WHEN a.Status = 0 THEN 0 ELSE 1 END, a.Insert_ServerDateTime DESC").ToList());

        /// <summary>
        /// قرار المشرف. approvedValue: الحد المعتمد (افتراضياً المطلوب)؛ لا يقل عن المطلوب فى الموافقة (وإلا فهو رفض).
        /// المعتمِد لا يكون طالب الموافقة نفسه (فصل المهام).
        /// </summary>
        public void Decide(long approvalId, bool approve, string notes = null, decimal? approvedValue = null) => Tx(opp =>
        {
            var a = opp.db.SalesRep_Approvals.FirstOrDefault(x => x.Id == approvalId) ?? throw new InvalidOperationException("الطلب غير موجود.");
            if (a.Status != (int)SalesRepApprovalStatus.Pending) throw new InvalidOperationException("تم البت فى هذا الطلب.");
            if (a.Requested_By == Common.User_Id) throw new InvalidOperationException("لا يعتمد المستخدم طلباً قدّمه بنفسه.");
            if (approve && approvedValue < a.Requested_Value) throw new InvalidOperationException("الحد المعتمد أقل من المطلوب؛ ارفض الطلب بدلاً من ذلك.");
            a.Status = approve ? (int)SalesRepApprovalStatus.Approved : (int)SalesRepApprovalStatus.Rejected;
            a.Approver_Id = Common.User_Id;
            a.Approved_Value = approve ? (approvedValue ?? a.Requested_Value) : null;
            a.Decision_Notes = string.IsNullOrWhiteSpace(notes) ? null : Trim(notes.Trim(), 1000);
            a.Decided_At = DateTime.Now;
            a.StampEdit();
            opp.db.SaveChanges();
            return 0;
        });

        // ============================== داخلى ==============================

        internal static SaleContext Context(operations opp, long repId)
        {
            var rep = opp.db.SalesRep_Reps.AsNoTracking().FirstOrDefault(r => r.Id == repId) ?? throw new InvalidOperationException("ملف المندوب غير موجود.");
            if (!rep.Active) throw new InvalidOperationException("ملف المندوب موقوف.");
            var shift = opp.db.SalesRep_Shifts.AsNoTracking().FirstOrDefault(s => s.Rep_Id == repId && s.Status != (int)SalesRepShiftStatus.Closed);
            var names = opp.db.Database.SqlQuery<NamesRow>($@"
SELECT ISNULL((SELECT name FROM Employee WHERE Id = {rep.Employee_Id}), N'') AS Rep_Name,
       ISNULL((SELECT Store_Name FROM Store WHERE Id = {rep.Van_Store_Id}), N'') AS Van_Name,
       ISNULL((SELECT Bank_Name FROM Banks WHERE Id = {rep.Cash_Box_Id}), N'') AS Box_Name,
       ISNULL((SELECT Store_Name FROM Store WHERE Id = {rep.Damaged_Store_Id}), N'') AS Damaged_Name").AsEnumerable().First();
            return new SaleContext
            {
                Rep_Id = rep.Id, Employee_Id = rep.Employee_Id, Rep_Name = names.Rep_Name, Branch_Id = rep.Branch_Id ?? Common.Branch_Id,
                Shift_Id = shift?.Id, Shift_Status = shift?.Status,
                Van_Store_Id = rep.Van_Store_Id, Van_Store_Name = names.Van_Name, Cash_Box_Id = rep.Cash_Box_Id, Cash_Box_Name = names.Box_Name,
                Price_Type_List_ID = rep.Price_Type_List_ID, Max_Discount_Rate = rep.Max_Discount_Rate, Max_Cash_On_Hand = rep.Max_Cash_On_Hand,
                Allow_Credit_Sales = rep.Allow_Credit_Sales,
                Damaged_Store_Id = rep.Damaged_Store_Id, Damaged_Store_Name = names.Damaged_Name, Allow_Blind_Return = rep.Allow_Blind_Return,
                Return_Window_Days = rep.Return_Window_Days ?? SalesRepReturnService.DefaultReturnWindowDays,
                Cash_On_Hand = Common.RoundMe(new Money_Transaction_BoxOrderReportCLS().GetBank_Balance(ref opp, rep.Cash_Box_Id, true)),
            };
        }

        public class NamesRow { public string Rep_Name { get; set; } = ""; public string Van_Name { get; set; } = ""; public string Box_Name { get; set; } = ""; public string Damaged_Name { get; set; } = ""; }

        private CheckResult CheckCore(operations opp, SaleContext ctx, SaleInput input)
        {
            if (!ctx.CanSell) throw new InvalidOperationException("لا وردية مفتوحة للمندوب؛ البيع يتم داخل وردية مفتوحة فقط.");
            if (input.Customer_Id <= 0) throw new InvalidOperationException("اختر العميل.");
            if (input.Lines.Count == 0) throw new InvalidOperationException("الفاتورة بلا أصناف.");
            var result = new CheckResult();

            var engine = new StockMovementService(opp);
            var lineIds = input.Lines.Select(l => l.Unit_Item_Id).Distinct().ToList();
            var units = opp.db.Unit_item.AsNoTracking().Where(u => lineIds.Contains(u.id)).Select(u => new { u.id, u.Store_id, u.Item_Id }).ToList().ToDictionary(u => u.id);
            if (input.Order_Taking)
            {
                // ---- أخذ طلبية: كل الأسطر من المخزن المصدر (ليس مخزنى المندوب)، والنقص تنبيه فقط (الطلبية تُحجز ويُحمَّل لها لاحقاً) ----
                long src = input.Source_Store_Id;
                if (src <= 0 || src == ctx.Van_Store_Id || src == ctx.Damaged_Store_Id) throw new InvalidOperationException("اختر مخزن الطلبية (غير مخزنى المندوب).");
                if (input.Lines.Any(l => l.Store_Id != src) || units.Values.Any(u => u.Store_id != src) || units.Count != lineIds.Count)
                    throw new InvalidOperationException("كل أصناف الطلبية من المخزن المختار فقط.");
                foreach (var g in input.Lines.GroupBy(l => l.Unit_Item_Id))
                {
                    decimal need = g.Sum(l => l.Qty_Small), avail = engine.GetStoredBalanceSmall(g.Key) - ReservedForOthers(opp, g.Key, input.Customer_Id);
                    if (need > avail) result.Warnings.Add($"الكمية المطلوبة ({need:0.##}) أكبر من المتاح بالمخزن ({Math.Max(0, avail):0.##}) لأحد الأصناف؛ ستُحجز ويُدبَّر الباقى قبل التسليم.");
                }
            }
            // ---- المتاح على السيارة (منع قاطع): الرصيد المحسوب من دفتر المحرك ----
            else if (input.Lines.Any(l => l.Store_Id != ctx.Van_Store_Id)) throw new InvalidOperationException("كل أصناف فاتورة المندوب من مخزن سيارته فقط.");
            foreach (var g in input.Lines.Where(_ => !input.Order_Taking).GroupBy(l => l.Unit_Item_Id))
            {
                if (!units.TryGetValue(g.Key, out var u) || u.Store_id != ctx.Van_Store_Id) throw new InvalidOperationException("صنف ليس من مخزن سيارة المندوب.");
                // المتاح = رصيد السيارة − المحجوز لطلبيات عملاء آخرين مسندة للسيارة (§6.3؛ طلبيات نفس العميل لا تُخصم).
                decimal need = g.Sum(l => l.Qty_Small), balance = engine.GetComputedBalanceSmall(g.Key), reserved = ReservedForOthers(opp, g.Key, input.Customer_Id);
                if (need > balance - reserved)
                    throw new InvalidOperationException(reserved > 0
                        ? $"الكمية المطلوبة ({need:0.##}) أكبر من المتاح بالسيارة ({balance - reserved:0.##} = الرصيد {balance:0.##} − محجوز لطلبيات عملاء آخرين {reserved:0.##}) لأحد الأصناف."
                        : $"الكمية المطلوبة ({need:0.##}) أكبر من رصيد السيارة ({balance:0.##}) لأحد الأصناف.");
            }

            // ---- الخصم: الحد = أقل سقف بين المندوب وسياسات (المندوب، شريحة العميل، الصنف، مجموعة الصنف) ----
            long? group = opp.db.Database.SqlQuery<long?>($"SELECT Group_Id AS Value FROM Customer WHERE Id = {input.Customer_Id}").AsEnumerable().FirstOrDefault();
            var policies = opp.db.SalesRep_Discount_Policies.AsNoTracking().ToList();
            var itemGroups = opp.db.Database.SqlQueryRaw<ItemGroupRow>(
                "SELECT id AS Item_Id, CAST(ISNULL(Items_GroupsId, 0) AS bigint) AS Group_Id FROM Item WHERE id IN (" + string.Join(",", units.Values.Select(u => u.Item_Id ?? 0).Distinct().DefaultIfEmpty(0)) + ")")
                .ToList().ToDictionary(r => r.Item_Id, r => r.Group_Id);
            foreach (var l in input.Lines.Where(l => !input.Order_Priced && l.Discount_Rate > 0))
            {
                long itemId = units[l.Unit_Item_Id].Item_Id ?? 0;
                itemGroups.TryGetValue(itemId, out long itemGroup);
                var caps = new List<decimal>();
                if (ctx.Max_Discount_Rate != null) caps.Add(ctx.Max_Discount_Rate.Value);
                foreach (var p in policies)
                {
                    bool applies = (p.Scope == (int)SalesRepDiscountScope.Rep && p.Scope_Id == ctx.Rep_Id)
                                   || (p.Scope == (int)SalesRepDiscountScope.CustomerGroup && group != null && p.Scope_Id == group)
                                   || (p.Scope == (int)SalesRepDiscountScope.Item && p.Scope_Id == itemId)
                                   || (p.Scope == (int)SalesRepDiscountScope.ItemGroup && itemGroup > 0 && p.Scope_Id == itemGroup);
                    if (applies) caps.Add(p.Requires_Approval_Above ?? p.Max_Discount_Rate);
                }
                decimal cap = caps.Count == 0 ? 0 : caps.Min();
                decimal rate = Math.Round(l.Discount_Rate, 2);
                if (rate > cap)
                    result.Violations.Add(new Violation { Type = SalesRepApprovalType.Discount, Unit_Item_Id = l.Unit_Item_Id, Requested = rate, Allowed = cap,
                        Message = $"خصم {rate:0.##}% أعلى من المسموح {cap:0.##}%" });
            }
            // الحد المطلق (Max_Discount_Rate لسياسة لها حد موافقة): لا يُتجاوز حتى بموافقة.
            foreach (var l in input.Lines.Where(l => !input.Order_Priced && l.Discount_Rate > 0))
            {
                long itemId = units[l.Unit_Item_Id].Item_Id ?? 0;
                itemGroups.TryGetValue(itemId, out long itemGroup);
                var hard = policies.Where(p => p.Requires_Approval_Above != null && (
                        (p.Scope == (int)SalesRepDiscountScope.Rep && p.Scope_Id == ctx.Rep_Id)
                        || (p.Scope == (int)SalesRepDiscountScope.CustomerGroup && group != null && p.Scope_Id == group)
                        || (p.Scope == (int)SalesRepDiscountScope.Item && p.Scope_Id == itemId)
                        || (p.Scope == (int)SalesRepDiscountScope.ItemGroup && itemGroup > 0 && p.Scope_Id == itemGroup)))
                    .Select(p => p.Max_Discount_Rate).DefaultIfEmpty(decimal.MaxValue).Min();
                if (Math.Round(l.Discount_Rate, 2) > hard) throw new InvalidOperationException($"خصم {l.Discount_Rate:0.##}% يتجاوز الحد الأقصى {hard:0.##}% ولا يُعتمد.");
            }

            // ---- الائتمان (آجل): مسموح للمندوب، والرصيد بعد الفاتورة ≤ الحد (العميل يغلب الشريحة ق7)، ولا دين أقدم من المسموح ----
            result.Customer_Balance = CustomerBalance(opp, input.Customer_Id);
            var policy = opp.db.SalesRep_Credit_Policies.AsNoTracking().FirstOrDefault(p => p.Customer_Id == input.Customer_Id)
                         ?? (group == null ? null : opp.db.SalesRep_Credit_Policies.AsNoTracking().FirstOrDefault(p => p.Customer_Id == null && p.Group_Id == group));
            result.Credit_Limit = policy?.Credit_Limit;
            if (input.Is_Credit)
            {
                // أخذ الطلبية: طريقة الدفع تتحدد عند التسليم (ويُفحص هناك)، فلا يُمنع مندوب البيع النقدى من أخذ طلبية بلا عربون كامل.
                if (!ctx.Allow_Credit_Sales && !input.Order_Taking)
                    result.Violations.Add(new Violation { Type = SalesRepApprovalType.Credit, Requested = Math.Round(result.Customer_Balance + input.Net_Total, 2), Allowed = 0,
                        Message = "المندوب غير مسموح له بالبيع الآجل" });
                else if (policy != null && result.Customer_Balance + input.Net_Total > policy.Credit_Limit)
                    result.Violations.Add(new Violation { Type = SalesRepApprovalType.Credit, Requested = Math.Round(result.Customer_Balance + input.Net_Total, 2), Allowed = policy.Credit_Limit,
                        Message = $"رصيد العميل بعد الفاتورة {result.Customer_Balance + input.Net_Total:N2} يتجاوز حد الائتمان {policy.Credit_Limit:N2}" });
                if (policy?.Max_Overdue_Days is int maxDays && policy.Block_On_Overdue)
                {
                    int age = OldestUnpaidAgeDays(opp, input.Customer_Id, result.Customer_Balance);
                    if (age > maxDays)
                        result.Violations.Add(new Violation { Type = SalesRepApprovalType.Credit, Requested = age, Allowed = maxDays,
                            Message = $"على العميل دين متأخر {age} يوماً (المسموح {maxDays})" });
                }
            }

            // ---- حد النقدية بالعهدة: تنبيه بالتوريد (لا يمنع البيع) ----
            if (!input.Is_Credit && !input.Order_Taking && input.Is_Cash_Payment && CashLimitWarning(ctx, input.Net_Total) is string cashWarning)
                result.Warnings.Add(cashWarning);

            // ---- حالة الموافقات لهذا المستند: موافقة معتمدة غير مستعملة تغطى القيمة المطلوبة ----
            var approvals = opp.db.SalesRep_Approvals.AsNoTracking().Where(a => a.Ref_Document_Guid == input.Doc_Guid && a.Ref_Doc_Id == null).ToList();
            foreach (var v in result.Violations)
            {
                var mine = approvals.Where(a => a.Type == (int)v.Type).OrderByDescending(a => a.Insert_ServerDateTime).ToList();
                var ok = mine.FirstOrDefault(a => a.Status == (int)SalesRepApprovalStatus.Approved && (a.Approved_Value ?? a.Requested_Value ?? 0) >= v.Requested);
                var pending = mine.FirstOrDefault(a => a.Status == (int)SalesRepApprovalStatus.Pending && (a.Requested_Value ?? 0) >= v.Requested);
                var last = ok ?? pending ?? mine.FirstOrDefault(a => (a.Requested_Value ?? 0) >= v.Requested);
                v.Approval_Id = last?.Id;
                v.Approval_Status = last?.Status;
            }
            return result;
        }

        public class ItemGroupRow { public long Item_Id { get; set; } public long Group_Id { get; set; } }

        /// <summary>
        /// رصيد العميل بنفس مصادر شاشة الفاتورة (CustomerCLS.GetCustomerBalance / MakeCustomer_MokhtalatDataByGroupNew): حساب العميل + حساب المورد
        /// المرتبط عدا النوع 25، والعميل نشط ومن نوع العملاء — لكن باستعلام واحد على اتصال المعاملة. الدالة القديمة تفرّغ جدول Mokhtalat وتملؤه
        /// من اتصال آخر، فكانت تنتظر للأبد أقفال المعاملة نفسها على صف العميل (توقف الاختبارات/التسليم بعد بيع فى نفس المعاملة).
        /// </summary>
        internal static decimal CustomerBalance(operations opp, long customerId)
        {
            decimal bal = opp.db.Database.SqlQuery<decimal>($@"
SELECT CAST(ISNULL((SELECT SUM(ISNULL(a.Export, 0) - ISNULL(a.Import, 0)) FROM Customers_Accounts a WHERE a.Customer_Id = c.Id AND a.Reference_Type <> 25), 0)
          + ISNULL((SELECT SUM(ISNULL(sa.Export, 0) - ISNULL(sa.Import, 0)) FROM Suppliers_Accounts sa JOIN Supplier sp ON sp.Id = sa.Supplier_Id
                    WHERE sp.Ref_Cust_Supplier_Id = c.Id AND sp.Ref_Cust_Supplier_Id > 0 AND sa.Reference_Type <> 25), 0) AS decimal(18,4)) AS Value
FROM Customer c WHERE c.Id = {customerId} AND c.active = 1 AND c.Customer_Type = {Common.Customer_Type}").AsEnumerable().FirstOrDefault();
            return Common.RoundMe(bal);
        }

        /// <summary>
        /// عمر أقدم دين غير مسدد (FIFO): الرصيد المدين يُغطّى بأحدث الفواتير الآجلة أولاً، وأقدم فاتورة يدخلها الباقى هى المتأخرة؛
        /// العمر = اليوم − تاريخ استحقاقها (أو تاريخها). 0 لو لا رصيد مدين.
        /// </summary>
        private static int OldestUnpaidAgeDays(operations opp, long customerId, decimal balance)
        {
            if (balance <= 0) return 0;
            var rows = opp.db.Database.SqlQuery<AgeRow>($@"
SELECT CAST(ISNULL(Net_Total, 0) AS decimal(18,2)) AS Net, ISNULL(InvoiceDueDate, Invoice_Date) AS Due
FROM Invoices_List WHERE Customer_Id = {customerId} AND ISNULL(Is_Invoice_Agile, 0) = 1 ORDER BY Invoice_Date DESC, Id DESC").ToList();
            decimal left = balance;
            DateTime? oldest = null;
            foreach (var r in rows)
            {
                if (left <= 0) break;
                oldest = r.Due;
                left -= r.Net;
            }
            return oldest == null ? 0 : Math.Max(0, (int)(DateTime.Today - oldest.Value.Date).TotalDays);
        }

        public class AgeRow { public decimal Net { get; set; } public DateTime? Due { get; set; } }

        /// <summary>أسطر الفحص من الفاتورة: الكمية + المجانى بالوحدة الصغرى، والخصم الفعلى = نسبة خصم السطر + نسبة خصم الفاتورة.</summary>
        public static List<SaleLine> LinesOf(operations opp, InvoicesOperationListCLS invL_O, InvoicesOperationCLS invD_O)
        {
            var ids = invD_O.invDlst.Select(d => d.Unit_Item_Table_Id ?? 0).Distinct().ToList();
            var factors = opp.db.Unit_item.AsNoTracking().Where(u => ids.Contains(u.id)).ToList()
                .ToDictionary(u => u.id, u => (u.Small_unit ?? 0) <= 0 ? 1m : u.Small_unit.Value);
            decimal total = invD_O.invDlst.Sum(d => d.Total ?? 0);
            decimal listRate = total > 0 ? (invL_O.invL.List_Final_CommissionValue ?? 0) / total * 100 : 0;
            return invD_O.invDlst.Select(d =>
            {
                long id = d.Unit_Item_Table_Id ?? 0;
                decimal f = factors.TryGetValue(id, out var x) ? x : 1;
                decimal qty = ((d.Larg_Qnty ?? 0) + (d.LargItemsQnty_Free ?? 0)) * f + (d.Small_Qnty ?? 0) + (d.SmallItemsQnty_Free ?? 0);
                decimal lineRate = (d.Total ?? 0) > 0 ? (d.Net_Total_Item_Commission ?? 0) / d.Total.Value * 100 : 0;
                return new SaleLine { Unit_Item_Id = id, Store_Id = d.Store_Id ?? 0, Qty_Small = qty, Discount_Rate = Math.Round(lineRate + listRate, 4) };
            }).ToList();
        }

        /// <summary>رؤوس إذونات النقدية التى أنشأها محرك الفاتورة لها (Order_Money.Id الفريد، لا معرّف السطر).</summary>
        private static List<long> VouchersOfInvoice(operations opp, long invoiceId) => opp.db.Database.SqlQuery<long>($@"
SELECT DISTINCT CAST(Order_Mony_Id AS bigint) AS Value FROM Money_Transaction_Box
WHERE DataDictionaryProcessesListsId = {Common.Invoice_Type_Id} AND Reference_Type_Table_Id = {invoiceId} AND ISNULL(Order_Mony_Id, 0) > 0").ToList();

        /// <summary>المحجوز (بالوحدة الصغرى) على سطر مخزن لطلبيات مؤكدة/منفَّذة جزئياً لعملاء غير customerId = المطلوب − المنفَّذ − الملغى.</summary>
        internal static decimal ReservedForOthers(operations opp, long unitItemId, long customerId) => opp.db.Database.SqlQuery<decimal>($@"
SELECT CAST(ISNULL(SUM(
         (CASE WHEN ISNULL(l.Qty_Larg,0) - ISNULL(l.Fulfilled_Qty_Larg,0) - ISNULL(l.Cancelled_Qty_Larg,0) > 0 THEN ISNULL(l.Qty_Larg,0) - ISNULL(l.Fulfilled_Qty_Larg,0) - ISNULL(l.Cancelled_Qty_Larg,0) ELSE 0 END) * f.Factor
       + (CASE WHEN ISNULL(l.Qty_Small,0) - ISNULL(l.Fulfilled_Qty_Small,0) - ISNULL(l.Cancelled_Qty_Small,0) > 0 THEN ISNULL(l.Qty_Small,0) - ISNULL(l.Fulfilled_Qty_Small,0) - ISNULL(l.Cancelled_Qty_Small,0) ELSE 0 END)), 0) AS decimal(18,4)) AS Value
FROM Customer_Order_Lines l JOIN Customer_Orders o ON o.Id = l.Order_Id JOIN Unit_item u ON u.id = l.Unit_Item_Table_Id
CROSS APPLY (SELECT CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END AS Factor) f
WHERE l.Unit_Item_Table_Id = {unitItemId} AND o.Status IN ({(int)CustomerOrderStatus.Confirmed}, {(int)CustomerOrderStatus.PartiallyFulfilled})
  AND ISNULL(l.Line_Status, 0) <> {(int)CustomerOrderLineStatus.Cancelled} AND ISNULL(o.Customer_Id, 0) <> {customerId}").AsEnumerable().First();

        internal static void Link(operations opp, long shiftId, SalesRepDocType type, long docId, long branchId)
        {
            if (opp.db.SalesRep_Document_Links.Any(l => l.Doc_Type == (int)type && l.Doc_Id == docId)) return;
            var link = new SalesRep_Document_Link { Id = CallSnowflakeIdGenerate.GenerateNewId(), Doc_Type = (int)type, Doc_Id = docId, Shift_Id = shiftId };
            link.StampInsert(branchId);
            opp.db.SalesRep_Document_Links.Add(link);
            opp.db.SaveChanges();
        }

        internal static string Trim(string s, int max) => s == null || s.Length <= max ? s : s.Substring(0, max);

        /// <summary>
        /// شرط SQL يستبعد مخازن السيارات وتالفها من شاشات البيع العادية (تُباع فقط بوضع المندوب داخل ورديته). فارغ لو جدول المندوبين
        /// غير موجود بعد فى هذه القاعدة. storeColumn: عمود رقم المخزن فى الاستعلام (مثل Store.Id).
        /// </summary>
        public static string ExcludeRepStoresSql(operations opp, string storeColumn)
        {
            try
            {
                bool exists = opp.db.Database.SqlQueryRaw<int>("SELECT CASE WHEN OBJECT_ID(N'dbo.SalesRep_Reps', N'U') IS NULL THEN 0 ELSE 1 END AS Value").AsEnumerable().First() == 1;
                return exists ? $" AND {storeColumn} NOT IN (SELECT Van_Store_Id FROM SalesRep_Reps UNION SELECT Damaged_Store_Id FROM SalesRep_Reps)" : "";
            }
            catch { return ""; }
        }

        private T Read<T>(Func<operations, T> f) => f(_external ?? new operations());

        private T Tx<T>(Func<operations, T> f)
        {
            if (_external != null) return f(_external);
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

    /// <summary>البيع يحتاج موافقة المشرف على مخالفة أو أكثر لم تُعتمد بعد.</summary>
    public class SalesRepApprovalRequiredException : InvalidOperationException
    {
        public IReadOnlyList<SalesRepSaleService.Violation> Violations { get; }
        /// <param name="what">المستند المحتاج للموافقة فى الرسالة (البيع، الشيك...).</param>
        public SalesRepApprovalRequiredException(IReadOnlyList<SalesRepSaleService.Violation> violations, string what = "البيع")
            : base(what + " يحتاج موافقة المشرف: " + string.Join("؛ ", violations.Select(v => v.Message))) { Violations = violations; }
    }
}
