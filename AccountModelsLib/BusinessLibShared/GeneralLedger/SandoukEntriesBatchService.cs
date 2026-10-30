using System;
using System.Collections.Generic;
using System.Data;

namespace AccountModelsLib.BusinessLibShared.GeneralLedger
{
    /// <summary>
    /// «إنشاء قيود الصندوق آلياً» لشاشة Razor (بدل General_Ledger.Make_Account_Entry_Automatic).
    /// يولّد القيد لأذون الصندوق التى لم يُنشأ لها قيد (Order_Money.IS_Posted_Flage = 0) بنفس دالة الإنشاء الموحَّدة
    /// Accounts_Entry_POST_CLS.Make_Entry_For_AccountsSandouk(orderId) بلا تعديل فيها.
    /// الفروق عن الديسكتوب (كلها لإصلاح عيوب فى الدفعة القديمة):
    ///  - إذن واحد لكل معاملة: فشل إذن لا يُسقط الباقى، وتقرير بما أُنشئ وما تعذّر؛
    ///  - الدفعة القديمة تعيد نجاحاً دائماً (تصفّر Excp فى آخرها) وتعيد استعمال رقم قيد واحد لكل الأذون؛ هنا يُحسب لكل إذن رقمه، ويُتحقق من النتيجة فعلاً.
    /// </summary>
    public class SandoukEntriesBatchService
    {
        private readonly Func<operations> _newOpp;

        public SandoukEntriesBatchService(Func<operations> newOpp = null) => _newOpp = newOpp ?? (() => new operations());

        public class PendingOrder
        {
            public long Id { get; set; }
            public long OrderNo { get; set; }
            public DateTime OrderDate { get; set; }
            public int TransactionType { get; set; }
            public decimal TotalValue { get; set; }
            public long BranchId { get; set; }
            public string BoxName { get; set; }
        }

        public class OrderOutcome
        {
            public long OrderId { get; set; }
            public long OrderNo { get; set; }
            public bool Created { get; set; }
            public long EntryId { get; set; }
            public string Message { get; set; }
            public List<string> Warnings { get; } = new List<string>();
        }

        public class BatchResult
        {
            public int Created { get; set; }
            public int Failed { get; set; }
            public List<OrderOutcome> Outcomes { get; } = new List<OrderOutcome>();
        }

        /// <summary>أذون الصندوق بلا قيد فى النطاق (فرع 0 = الكل).</summary>
        public List<PendingOrder> GetPending(DateTime from, DateTime to, long branchId)
        {
            string sql = "SELECT o.Id, o.Order_No, o.Order_Date, o.Transaction_type, o.Total_Value, o.Branch_Id, b.Bank_Name " +
                         "FROM Order_Money o LEFT OUTER JOIN Banks b ON b.id = o.Bank_ID " +
                         "WHERE o.IS_Posted_Flage = 0 AND o.Order_Date >= " + operations.Get_DateToInsert(from.Date) +
                         " AND o.Order_Date < " + operations.Get_DateToInsert(to.Date.AddDays(1));
            if (branchId > 0) sql += " AND o.Branch_Id = " + branchId;
            sql += " ORDER BY o.Order_Date, o.Order_No";
            var dt = _newOpp().return_DataTable(sql);
            var list = new List<PendingOrder>();
            foreach (DataRow r in dt.Rows)
                list.Add(new PendingOrder
                {
                    Id = Convert.ToInt64(r["Id"]),
                    OrderNo = r["Order_No"] == DBNull.Value ? 0 : Convert.ToInt64(r["Order_No"]),
                    OrderDate = Convert.ToDateTime(r["Order_Date"]),
                    TransactionType = r["Transaction_type"] == DBNull.Value ? 0 : Convert.ToInt32(r["Transaction_type"]),
                    TotalValue = r["Total_Value"] == DBNull.Value ? 0 : Convert.ToDecimal(r["Total_Value"]),
                    BranchId = r["Branch_Id"] == DBNull.Value ? 0 : Convert.ToInt64(r["Branch_Id"]),
                    BoxName = r["Bank_Name"] as string ?? ""
                });
            return list;
        }

        /// <summary>
        /// ينشئ القيود لكل أذون النطاق غير المقيَّدة. dryRun: ينفّذ كل شىء ثم يتراجع (للاختبار بلا أثر).
        /// callerOpp: معاملة المستدعى (لا يُعتمد ولا يُتراجع عنها هنا) — للاختبار الملغى.
        /// </summary>
        public BatchResult Run(DateTime from, DateTime to, long branchId, IEnumerable<long> onlyIds = null, bool dryRun = false, operations callerOpp = null)
        {
            ValidateRange(from, to);
            var pending = GetPending(from, to, branchId);
            HashSet<long> filter = onlyIds == null ? null : new HashSet<long>(onlyIds);
            var result = new BatchResult();
            foreach (var p in pending)
            {
                if (filter != null && !filter.Contains(p.Id)) continue;
                var outcome = CreateOne(p, dryRun, callerOpp);
                result.Outcomes.Add(outcome);
                if (outcome.Created) result.Created++; else result.Failed++;
            }
            return result;
        }

        private OrderOutcome CreateOne(PendingOrder p, bool dryRun, operations callerOpp)
        {
            var outcome = new OrderOutcome { OrderId = p.Id, OrderNo = p.OrderNo };
            bool own = callerOpp == null;
            var opp = callerOpp ?? _newOpp();
            if (own) opp.BeginTransaction();
            try
            {
                int excp = 0; string msg = "";
                new Accounts_Entry_POST_CLS().Make_Entry_For_AccountsSandouk(ref excp, ref msg, ref opp, p.Id);

                // الدالة القديمة تبتلع الأخطاء وتعيد Excp = 0 دائماً، فالمرجع الحقيقى للنجاح هو ربط الإذن بقيد موجود.
                var chk = opp.return_DataTable("SELECT IS_Posted_Flage, Account_Entry_List_ID FROM Order_Money WHERE Id = " + p.Id);
                long entryId = chk.Rows.Count == 0 || chk.Rows[0]["Account_Entry_List_ID"] == DBNull.Value ? 0 : Convert.ToInt64(chk.Rows[0]["Account_Entry_List_ID"]);
                bool flagged = chk.Rows.Count > 0 && chk.Rows[0]["IS_Posted_Flage"] != DBNull.Value && Convert.ToInt32(chk.Rows[0]["IS_Posted_Flage"]) == 1;
                if (excp != 0 || !flagged || entryId <= 0)
                {
                    if (own) opp.Rollback();
                    outcome.Message = string.IsNullOrWhiteSpace(msg) ? "تعذّر إنشاء القيد (لا توجد سطور صالحة للإذن)" : msg;
                    return outcome;
                }

                // الحارس المركزى فى الوضع التحذيرى: المخالفات تُسجَّل فى نتيجة الإذن ولا تمنع الإنشاء.
                try { foreach (var i in EntryValidator.ValidateSaved(opp, entryId)) outcome.Warnings.Add(i.ToString()); }
                catch (Exception vex) { outcome.Warnings.Add("تعذّر فحص القيد: " + vex.Message); }

                if (own) { if (dryRun) opp.Rollback(); else opp.Commit(); }
                outcome.Created = true;
                outcome.EntryId = entryId;
                return outcome;
            }
            catch (Exception ex)
            {
                if (own) { try { opp.Rollback(); } catch { } }
                outcome.Message = ex.Message;
                return outcome;
            }
        }

        // نفس تحققات الشاشة القديمة: التاريخان واقعان داخل فترة مالية معرَّفة، والبداية لا تتجاوز النهاية.
        private void ValidateRange(DateTime from, DateTime to)
        {
            if (from.Date > to.Date) throw new InvalidOperationException("تاريخ البداية بعد تاريخ النهاية");
            var dt = _newOpp().return_DataTable("SELECT COUNT(*) AS C FROM Financial_Period WHERE DateFrom <= " + operations.Get_DateToInsert(from.Date) +
                                                " AND DateTo >= " + operations.Get_DateToInsert(to.Date));
            if (Convert.ToInt32(dt.Rows[0]["C"]) == 0) throw new InvalidOperationException("النطاق المحدد يجب أن يقع داخل فترة مالية واحدة");
        }
    }
}
