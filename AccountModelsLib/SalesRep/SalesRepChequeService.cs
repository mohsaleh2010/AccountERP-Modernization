using System;
using System.Collections.Generic;
using System.Linq;
using AccountCheckMoney;
using AccountModelsLib.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib.SalesRep
{
    /// <summary>
    /// خطة المندوبين — المرحلة 3: تحصيل الشيكات على الوردية (قرار المستخدم 2026-10-16).
    /// - الشيك يُسجَّل مباشرة فى حافظة الشيكات الواردة (Receipt = 1) بالمحرك الحالى Order_Receipt_ChecksNewNew.Add_New_Check
    ///   داخل معاملة الوردية: إذن استلام شيكات + Checks_Memory + القيد (مدين الحافظة / دائن العميل) + حركة حساب العميل + السجل.
    ///   هذه الخدمة لا تكتب قيداً ولا رصيداً بنفسها.
    /// - مسموح فقط لمندوب فى ملفه «يحصّل شيكات»، داخل وردية مفتوحة. استحقاق بعد «أقصى أيام لاستحقاق الشيك» = موافقة المشرف (تُستعمل مرة لنفس المستند).
    /// - الشيك يُربط بالوردية (SalesRep_Document_Links، النوع Cheque) ويبقى «مع المندوب» حتى يؤشّر المشرف تسليمه؛ الشيك غير المسلَّم يمنع إقفال الوردية.
    /// </summary>
    public class SalesRepChequeService
    {
        private readonly operations _external;
        public SalesRepChequeService(operations external = null) { _external = external; }

        public class ChequeInput
        {
            public Guid Doc_Guid { get; set; }
            public long Customer_Id { get; set; }
            public string Check_No { get; set; } = "";
            /// <summary>البنك المسحوب عليه (Banks بـ Is_Bank = 1).</summary>
            public long Bank_Id { get; set; }
            public DateTime Due_Date { get; set; }
            public decimal Amount { get; set; }
            public string Drawer_Name { get; set; }
            public string Notes { get; set; }
        }

        public class ChequeRow
        {
            public long Id { get; set; }
            public long Order_No { get; set; }
            public string Check_No { get; set; } = "";
            public string Bank_Name { get; set; } = "";
            public DateTime? Due_Date { get; set; }
            public decimal Amount { get; set; }
            public string Customer_Name { get; set; } = "";
            public int Receipt { get; set; }
            public DateTime? Collected_At { get; set; }
            public DateTime? Handed_Over_At { get; set; }
        }

        public class BankOption { public long Id { get; set; } public string Name { get; set; } = ""; }

        /// <summary>البنوك المسحوب عليها (نفس قائمة شاشة «إذن إستلام شيكات»).</summary>
        public List<BankOption> GetBanks() => Read(opp => opp.db.Database.SqlQuery<BankOption>($@"
SELECT CAST(Id AS bigint) AS Id, ISNULL(Bank_Name, N'') AS Name FROM Banks WHERE Is_Bank = 1 ORDER BY Bank_Name").ToList());

        /// <summary>يفحص الشيك بلا حفظ: يرمى للمنع القاطع ويرجع مخالفة الاستحقاق مع حالة موافقتها لهذا المستند.</summary>
        public SalesRepSaleService.CheckResult Check(long repId, ChequeInput input) => Read(opp => CheckCore(opp, repId, input));

        /// <summary>يطلب موافقة المشرف لمخالفة الاستحقاق إن لم يكن لها طلب قائم أو موافقة تغطيها. يرجع الفحص بعد الطلب.</summary>
        public SalesRepSaleService.CheckResult RequestApprovals(long repId, ChequeInput input, string reason = null) => Tx(opp =>
        {
            var ctx = SalesRepSaleService.Context(opp, repId);
            var result = CheckCore(opp, repId, input);
            foreach (var v in result.Violations.Where(v => v.Approval_Status != (int)SalesRepApprovalStatus.Pending && !v.Covered))
            {
                var a = new SalesRep_Approval
                {
                    Id = CallSnowflakeIdGenerate.GenerateNewId(),
                    Type = (int)v.Type,
                    Ref_Document_Guid = input.Doc_Guid,
                    Ref_Doc_Type = (int)SalesRepDocType.Cheque,
                    Requested_By = Common.User_Id,
                    Status = (int)SalesRepApprovalStatus.Pending,
                    Reason = SalesRepSaleService.Trim((string.IsNullOrWhiteSpace(reason) ? "" : reason.Trim() + " — ") + v.Message, 1000),
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
            return CheckCore(opp, repId, input);
        });

        /// <summary>
        /// تحصيل شيك من عميل على الوردية المفتوحة: يعيد الفحص داخل المعاملة، يسجّل الشيك فى حافظة الشيكات الواردة بالمحرك الحالى،
        /// يربطه بالوردية ويعلّم الموافقة المستعملة. يرجع رقم الشيك (Checks_Memory.Id).
        /// </summary>
        public long Collect(long repId, ChequeInput input) => Tx(opp =>
        {
            var ctx = SalesRepSaleService.Context(opp, repId);
            var check = CheckCore(opp, repId, input);
            if (!check.Ok) throw new SalesRepApprovalRequiredException(check.Violations.Where(v => !v.Covered).ToList(), "الشيك");

            var ac = new Account_Entry_Invoice_CLS();
            long hafzaId = ac.GetSubmain_Account_Id(ref opp, Common.Account_Type_Checks_Memory_Ward_Hafza);
            var hafza = Account(opp, hafzaId) ?? throw new InvalidOperationException("حساب «حافظة الشيكات الواردة» غير معرّف فى دليل الحسابات.");
            var cust = CustomerAccount(opp, input.Customer_Id);
            string bankName = opp.db.Database.SqlQuery<string>($"SELECT ISNULL(Bank_Name, N'') AS Value FROM Banks WHERE Id = {input.Bank_Id}").AsEnumerable().FirstOrDefault() ?? "";
            long cur = opp.db.Database.SqlQuery<long>($"SELECT CAST(ISNULL(currency_Id, 1) AS bigint) AS Value FROM Banks WHERE Id = {ctx.Cash_Box_Id}").AsEnumerable().FirstOrDefault();
            if (cur <= 0) cur = 1;
            var vouchers = new CashVouchersService();
            decimal factor = vouchers.GetFactor(cur);
            decimal amount = Math.Round(input.Amount, 2);
            string words = vouchers.Tafkeet(amount, cur) ?? "";
            DateTime today = DateTime.Today;
            // رقم الإذن داخل المعاملة (Make_New_Order_CheckNo يفتح اتصالاً آخر فيُحجب بما كُتب فى هذه المعاملة).
            long orderNo = opp.db.Database.SqlQuery<long>($@"SELECT CAST(ISNULL(MAX(Order_No), 0) + 1 AS bigint) AS Value FROM Order_Money_Check
WHERE Bank_Type = 1 AND Transaction_type = 1").AsEnumerable().First();
            string checkNo = Clean(input.Check_No, 50);
            string drawer = Clean(input.Drawer_Name, 200);
            string notes = Clean(string.IsNullOrWhiteSpace(input.Notes) ? "شيك محصّل بواسطة المندوب " + ctx.Rep_Name : input.Notes, 500);

            var engine = new Order_Receipt_ChecksNewNew { Branch_Id = ctx.Branch_Id };
            var or = engine.or;
            or.Order_No = orderNo;
            or.Order_Date = today;
            or.Man_Name = Clean(cust.Name, 200);
            or.Transaction_type = 1;
            or.Bank_ID = input.Bank_Id;
            or.Bank_Type = 1;
            or.User_Id = Common.User_Id;
            or.Branch_Id = ctx.Branch_Id;
            or.Order_Notes = notes;
            or.Currency_Id = cur;

            var chm = engine.chm;
            chm.Day_Date = today;
            chm.Due_Date = input.Due_Date.Date;
            chm.Transfer_Check_Id = 0;
            chm.IS_Posted_Flage = 0;
            chm.Currency_Id = cur;
            chm.Factor = factor;
            chm.Bank_id = input.Bank_Id;
            chm.Bank_Name = Clean(bankName, 200);
            chm.Check_No = checkNo;
            chm.Check_Notes = notes;
            chm.Check_Value = amount;
            chm.Check_Value_ = Clean(words, 500);
            chm.First_Drawer_Name = drawer ?? "";
            chm.Last_Drawer_Name = "";
            chm.Exchanger_Name = "";
            chm.Customer_Id = input.Customer_Id;
            chm.Supplier_Id = 0;
            // الوارد: «من» = الحافظة (مدين)، «إلى» = العميل (دائن)، والشيك على حساب العميل — مثل Check_DataFrmNewNew.
            chm.SubAccount_Id = cust.Id;
            chm.SubAccount_Name = Clean(cust.Name, 200);
            chm.SubAccount_Id_RelatedTo = cust.Id;
            chm.SubAccount_Name_RelatedTo = Clean(cust.Name, 200);
            chm.SubAccount_Id_RelatedFrom = hafza.Id;
            chm.SubAccount_Name_RelatedFrom = Clean(hafza.Name, 200);
            chm.Transaction_Type = 1;
            chm.User_Id = Common.User_Id;
            chm.Branch_Id = ctx.Branch_Id;

            engine.Account_Customer_Type_ID_From = hafza.Type_Id;
            engine.Account_Customer_Type_Value_From = hafza.Type_Value;
            engine.Account_Customer_Type_ID_To = cust.Type_Id;
            engine.Account_Customer_Type_Value_To = cust.Type_Value;
            engine.Transfer_Check = false;

            var list = engine.chmlist;
            list.Bank_id = input.Bank_Id;
            list.Bank_Name = chm.Bank_Name;
            list.Check_No = checkNo;
            list.Check_Notes = notes;
            list.Check_Value = amount;
            list.Check_Value_ = chm.Check_Value_;
            list.Day_Date = today;
            list.Due_Date = chm.Due_Date;
            list.Due_Check = "";
            list.First_Drawer_Name = chm.First_Drawer_Name;
            list.Last_Drawer_Name = "";
            list.Exchanger_Name = "";

            var cts = new Checks_MemoryTasweaCLS
            {
                SubAccount_Id_From_New = hafza.Id, SubAccount_Name_From_New = chm.SubAccount_Name_RelatedFrom,
                SubAccount_Id_To_New = cust.Id, SubAccount_Name_To_New = chm.SubAccount_Name_RelatedTo,
                Account_Customer_Type_Value_From_New = hafza.Type_Value, Account_Customer_Type_ID_From_New = hafza.Type_Id,
                Account_Customer_Type_Value_To_New = cust.Type_Value, Account_Customer_Type_ID_To_New = cust.Type_Id,
                Receipt_New = 1,
            };

            int excp = 0;
            string msg = "";
            engine.Add_New_Check(ref excp, ref msg, false, false, cts, opp);
            long chequeId = chm.Id;
            if (excp != 0 || chequeId <= 0) throw new InvalidOperationException(string.IsNullOrWhiteSpace(msg) ? "تعذر تسجيل الشيك." : msg);

            SalesRepSaleService.Link(opp, ctx.Shift_Id.Value, SalesRepDocType.Cheque, chequeId, ctx.Branch_Id);
            var link = opp.db.SalesRep_Document_Links.First(l => l.Doc_Type == (int)SalesRepDocType.Cheque && l.Doc_Id == chequeId);
            link.Doc_Guid = input.Doc_Guid;
            SalesRepVisitService.Attach(opp, ctx.Shift_Id.Value, input.Customer_Id, SalesRepVisitResult.Collection, (SalesRepDocType.Cheque, chequeId));
            var used = opp.db.SalesRep_Approvals.Where(a => a.Ref_Document_Guid == input.Doc_Guid && a.Status == (int)SalesRepApprovalStatus.Approved && a.Ref_Doc_Id == null).ToList();
            foreach (var a in used) { a.Ref_Doc_Id = chequeId; a.Shift_Id ??= ctx.Shift_Id; a.StampEdit(); }
            opp.db.SaveChanges();
            return chequeId;
        });

        /// <summary>شيكات الوردية وحالة تسليم كل منها.</summary>
        public List<ChequeRow> GetShiftCheques(long shiftId) => Read(opp => opp.db.Database.SqlQuery<ChequeRow>($@"
SELECT m.Id, CAST(ISNULL(o.Order_No, 0) AS bigint) AS Order_No, ISNULL(m.Check_No, N'') AS Check_No, ISNULL(m.Bank_Name, N'') AS Bank_Name, m.Due_Date,
       CAST(ISNULL(m.Check_Value, 0) AS decimal(18,2)) AS Amount, ISNULL(c.Name, N'') AS Customer_Name, ISNULL(m.Receipt, 0) AS Receipt,
       l.Insert_ServerDateTime AS Collected_At, l.Handed_Over_At
FROM SalesRep_Document_Links l JOIN Checks_Memory m ON m.Id = l.Doc_Id
LEFT JOIN Order_Money_Check o ON o.Id = m.Order_Mony_Id LEFT JOIN Customer c ON c.Id = m.Customer_Id
WHERE l.Shift_Id = {shiftId} AND l.Doc_Type = {(int)SalesRepDocType.Cheque}
ORDER BY l.Insert_ServerDateTime").ToList());

        /// <summary>
        /// تأشير المشرف على تسليم شيكات الوردية (أو إلغاء التأشير) قبل الإقفال. الوردية المقفلة لا تتغير.
        /// التسليم لا يغير حالة الشيك فى الحافظة (هو فيها من التحصيل)؛ هو إثبات استلام الورقة نفسها من المندوب.
        /// </summary>
        public int HandOver(long shiftId, IReadOnlyCollection<long> chequeIds, bool handedOver = true) => Tx(opp =>
        {
            var shift = opp.db.SalesRep_Shifts.AsNoTracking().FirstOrDefault(s => s.Id == shiftId) ?? throw new InvalidOperationException("الوردية غير موجودة.");
            if (shift.Status == (int)SalesRepShiftStatus.Closed) throw new InvalidOperationException("الوردية مقفلة.");
            if (chequeIds == null || chequeIds.Count == 0) throw new InvalidOperationException("اختر شيكاً واحداً على الأقل.");
            var ids = chequeIds.Distinct().ToList();
            var links = opp.db.SalesRep_Document_Links.Where(l => l.Shift_Id == shiftId && l.Doc_Type == (int)SalesRepDocType.Cheque && ids.Contains(l.Doc_Id)).ToList();
            if (links.Count != ids.Count) throw new InvalidOperationException("شيك أو أكثر ليس من شيكات هذه الوردية.");
            foreach (var l in links)
            {
                l.Handed_Over_At = handedOver ? DateTime.Now : null;
                l.Handed_Over_User_Id = handedOver ? Common.User_Id : null;
                l.StampEdit();
            }
            opp.db.SaveChanges();
            return links.Count;
        });

        /// <summary>عدد شيكات الوردية التى لم تُسلَّم بعد (تمنع الإقفال).</summary>
        internal static int NotHandedOver(operations opp, long shiftId) =>
            opp.db.SalesRep_Document_Links.Count(l => l.Shift_Id == shiftId && l.Doc_Type == (int)SalesRepDocType.Cheque && l.Handed_Over_At == null);

        // ============================== داخلى ==============================

        private SalesRepSaleService.CheckResult CheckCore(operations opp, long repId, ChequeInput input)
        {
            var ctx = SalesRepSaleService.Context(opp, repId);
            var rep = opp.db.SalesRep_Reps.AsNoTracking().First(r => r.Id == repId);
            if (!ctx.CanSell) throw new InvalidOperationException("لا وردية مفتوحة للمندوب؛ التحصيل يتم داخل وردية مفتوحة فقط.");
            if (!rep.Allow_Cheque_Collection) throw new InvalidOperationException("المندوب غير مسموح له بتحصيل الشيكات (ملف المندوب).");
            if (input == null) throw new InvalidOperationException("بيانات الشيك مطلوبة.");
            if (input.Doc_Guid == Guid.Empty) throw new InvalidOperationException("معرّف المستند مطلوب.");
            if (input.Customer_Id <= 0) throw new InvalidOperationException("اختر العميل.");
            CustomerAccount(opp, input.Customer_Id);
            if (string.IsNullOrWhiteSpace(input.Check_No)) throw new InvalidOperationException("رقم الشيك مطلوب.");
            // قيد الشيك فى المحرك يقرأ رقم الشيك كرقم (GetValuesAfterSetArabic)، وأرقام الشيكات أرقام فعلاً.
            if (!input.Check_No.Trim().All(char.IsAsciiDigit) || input.Check_No.Trim().Length > 30) throw new InvalidOperationException("رقم الشيك أرقام فقط (حتى 30 رقماً).");
            if (input.Amount <= 0) throw new InvalidOperationException("قيمة الشيك يجب أن تكون أكبر من صفر.");
            if (input.Bank_Id <= 0 || !opp.db.Database.SqlQuery<int>($"SELECT COUNT(*) AS Value FROM Banks WHERE Id = {input.Bank_Id} AND Is_Bank = 1").AsEnumerable().Any(n => n > 0))
                throw new InvalidOperationException("اختر البنك المسحوب عليه.");
            if (input.Due_Date == default) throw new InvalidOperationException("تاريخ الاستحقاق مطلوب.");
            string no = Clean(input.Check_No, 50);
            if (opp.db.Database.SqlQuery<int>($@"SELECT COUNT(*) AS Value FROM Checks_Memory WHERE Check_No = {no} AND Bank_id = {input.Bank_Id} AND Transaction_Type = 1")
                    .AsEnumerable().First() > 0)
                throw new InvalidOperationException($"الشيك رقم {no} على هذا البنك مسجَّل من قبل.");

            var result = new SalesRepSaleService.CheckResult();
            int days = (input.Due_Date.Date - DateTime.Today).Days;
            if (days < 0) result.Warnings.Add($"الشيك مستحق منذ {-days} يوماً؛ يُودَع بالبنك سريعاً.");
            if (rep.Max_Cheque_Due_Days is int max && days > max)
                result.Violations.Add(new SalesRepSaleService.Violation { Type = SalesRepApprovalType.ChequeDueDate, Requested = days, Allowed = max,
                    Message = $"استحقاق الشيك بعد {days} يوماً يتجاوز المسموح للمندوب ({max} يوماً)" });

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

        public class AccountRow { public long Id { get; set; } public string Name { get; set; } = ""; public int Type_Value { get; set; } public long Type_Id { get; set; } }

        private static AccountRow Account(operations opp, long subAccountId) => subAccountId <= 0 ? null : opp.db.Database.SqlQuery<AccountRow>($@"
SELECT s.Id, ISNULL(s.SubAccount_Name, N'') AS Name, ISNULL(s.Account_Customer_Type_Value, 0) AS Type_Value, CAST(ISNULL(s.Account_Customer_Type_ID, 0) AS bigint) AS Type_Id
FROM Submain_Account s WHERE s.Id = {subAccountId}").AsEnumerable().FirstOrDefault();

        /// <summary>حساب العميل فى الدليل؛ الطرف الدائن فى قيد الشيك ويُحدَّث رصيده بحركة الشيك.</summary>
        private static AccountRow CustomerAccount(operations opp, long customerId)
        {
            var row = opp.db.Database.SqlQuery<AccountRow>($@"
SELECT s.Id, ISNULL(c.Name, ISNULL(s.SubAccount_Name, N'')) AS Name, {Common.Customer_Account_Type} AS Type_Value, CAST(c.Id AS bigint) AS Type_Id
FROM Customer c JOIN Submain_Account s ON s.Id = c.SubAccount_Id WHERE c.Id = {customerId}").AsEnumerable().FirstOrDefault();
            return row ?? throw new InvalidOperationException("العميل غير موجود أو ليس له حساب فى دليل الحسابات.");
        }

        /// <summary>محرك الشيكات يدمج النصوص فى SQL؛ الفاصلة العليا تُستبدل حتى لا تكسر الجملة، والطول يُقص.</summary>
        private static string Clean(string s, int max) => s == null ? null : SalesRepSaleService.Trim(s.Trim().Replace("'", "’"), max);

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
}
