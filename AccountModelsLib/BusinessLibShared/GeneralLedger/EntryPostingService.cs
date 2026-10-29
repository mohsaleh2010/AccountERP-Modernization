using System;
using System.Data;

namespace AccountModelsLib.BusinessLibShared.GeneralLedger
{
    /// <summary>
    /// «ترحيل القيود» و«إلغاء الترحيل» لشاشة Razor (بدل General_Ledger.Tar7eel_Account_Entry).
    /// نفس الشروط والاستعلامات والترحيل الفعلى (Accounts_Entry_POST_CLS.Make_Post / Cancel_Post) بلا تعديل فيها،
    /// والفرق الوحيد: الإدارة الصريحة للمعاملة. Make_Post_WithNewTar7eel القديمة تفتح معاملتها داخلياً وعند الخطأ
    /// لا تتراجع وتعيد رسالة «تمت بنجاح»؛ هنا أى خطأ ⇒ تراجع كامل ورسالة الخطأ.
    /// </summary>
    public class EntryPostingService
    {
        private readonly Func<operations> _newOpp;

        public EntryPostingService(Func<operations> newOpp = null) => _newOpp = newOpp ?? (() => new operations());

        public record PostResult(int Entries, string Message);

        public class Counts
        {
            public int Unposted { get; set; }
            public int Posted { get; set; }
        }

        public class PeriodRow
        {
            public long Id { get; set; }
            public DateTime DateFrom { get; set; }
            public DateTime DateTo { get; set; }
        }

        public System.Collections.Generic.List<PeriodRow> GetPeriods()
        {
            var dt = _newOpp().return_DataTable("SELECT Id, DateFrom, DateTo FROM Financial_Period ORDER BY DateFrom DESC");
            var list = new System.Collections.Generic.List<PeriodRow>();
            foreach (DataRow r in dt.Rows)
                list.Add(new PeriodRow { Id = Convert.ToInt64(r["Id"]), DateFrom = Convert.ToDateTime(r["DateFrom"]), DateTo = Convert.ToDateTime(r["DateTo"]) });
            return list;
        }

        /// <summary>عدد القيود غير المرحَّلة والمرحَّلة فى النطاق (يومية 0 = الكل، فرع 0 = الكل).</summary>
        public Counts GetCounts(long journalCode, long branchId, DateTime from, DateTime to)
        {
            var dt = _newOpp().return_DataTable(
                "SELECT ISNULL(SUM(CASE WHEN IS_Transfer_To_Gl_Flage = 0 THEN 1 ELSE 0 END), 0) AS Unposted, " +
                "ISNULL(SUM(CASE WHEN IS_Transfer_To_Gl_Flage = 1 THEN 1 ELSE 0 END), 0) AS Posted FROM Account_Entry_List " + Condition(journalCode, branchId, from, to, null));
            return new Counts { Unposted = Convert.ToInt32(dt.Rows[0]["Unposted"]), Posted = Convert.ToInt32(dt.Rows[0]["Posted"]) };
        }

        /// <summary>
        /// يرحّل القيود غير المرحَّلة فى النطاق. callerOpp: معاملة المستدعى (للاختبار الملغى)؛ بدونها تفتح الدالة معاملتها وتعتمدها.
        /// </summary>
        public PostResult Post(long journalCode, long branchId, long periodId, DateTime from, DateTime to, operations callerOpp = null)
        {
            ValidateRange(periodId, from, to);
            bool own = callerOpp == null;
            var opp = callerOpp ?? _newOpp();
            if (own) opp.BeginTransaction();
            try
            {
                string cond = Condition(journalCode, branchId, from, to, false);
                var dt = opp.return_DataTable(
                    @" SELECT     Account_Entry_List.*, Account_Entry_Details.*,Account_Entry_Details.Id AS Account_Entry_Details_Id, Submain_Account.*
FROM         Account_Entry_List INNER JOIN
                      Account_Entry_Details ON Account_Entry_List.ID = Account_Entry_Details.Account_Entry_List_ID INNER JOIN
                      Submain_Account ON Account_Entry_Details.Submain_Account_ID = Submain_Account.Id " + cond + " Order by Account_Entry_Details.Id ");
                if (dt.Rows.Count == 0)
                {
                    if (own) opp.Rollback();
                    return new PostResult(0, "لا توجد قيود لترحيلها");
                }

                int entries = Convert.ToInt32(opp.return_DataTable("SELECT COUNT(*) AS C FROM Account_Entry_List " + cond).Rows[0]["C"]);
                string msg = "";
                new Accounts_Entry_POST_CLS().Make_Post(ref msg, dt, ref opp, 0, journalCode, branchId, from, to);
                opp.insert_me(" UPDATE    Account_Entry_List SET IS_Transfer_To_Gl_Flage = 1, IS_Transfer_To_Gl_Flage_Str = N'مرحل', Transfer_To_Gl_DateTime = "
                              + operations.Get_DateTimeToInsert(DateTime.Now.Date) + " " + cond);
                if (own) opp.Commit();
                return new PostResult(entries, "تمت عملية ترحيل القيود بنجاح");
            }
            catch
            {
                if (own) opp.Rollback();
                throw;
            }
        }

        /// <summary>
        /// يلغى ترحيل كل حسابات الفترة المالية للفرع (يصفّر أرصدة الاستاذ للفترة والفرع ويعيد القيود «غير مرحل») — كما فى Cancel_Post القديمة.
        /// </summary>
        public PostResult CancelPost(long periodId, long branchId, operations callerOpp = null)
        {
            if (periodId <= 0) throw new InvalidOperationException("يجب إختيار الفترة الماليه");
            if (branchId <= 0) throw new InvalidOperationException("يجب إختيار الفرع");
            bool own = callerOpp == null;
            var opp = callerOpp ?? _newOpp();
            if (own) opp.BeginTransaction();
            try
            {
                new Accounts_Entry_POST_CLS().Cancel_Post(ref opp, periodId, branchId);
                if (own) opp.Commit();
                return new PostResult(0, "تم إلغاء ترحيل كل حسابات الفترة الماليه المحدده");
            }
            catch
            {
                if (own) opp.Rollback();
                throw;
            }
        }

        // شروط الترحيل نفسها: فترة التاريخ + (غير مرحل) + يومية + فرع. posted: false غير مرحل، true مرحل، null بلا شرط.
        private static string Condition(long journalCode, long branchId, DateTime from, DateTime to, bool? posted)
        {
            string c = " WHERE (Account_Entry_List.Entry_Date >= " + operations.Get_DateToInsert(from) + ") AND (Account_Entry_List.Entry_Date <= " + operations.Get_DateToInsert(to) + ") ";
            if (posted.HasValue) c += " AND IS_Transfer_To_Gl_Flage=" + (posted.Value ? 1 : 0) + " ";
            if (journalCode > 0) c += " AND (Account_Entry_List.journal_Code = " + journalCode + ") ";
            if (branchId > 0) c += " AND (Account_Entry_List.Branch_Id_Entry = " + branchId + ") ";
            return c;
        }

        // نفس تحققات الشاشة القديمة: البداية كما فى الفترة والنهاية لا تتجاوزها.
        private void ValidateRange(long periodId, DateTime from, DateTime to)
        {
            if (periodId <= 0) throw new InvalidOperationException("يجب إختيار الفترة الماليه");
            var dt = _newOpp().return_DataTable("SELECT DateFrom, DateTo FROM Financial_Period WHERE Id = " + periodId);
            if (dt.Rows.Count == 0) throw new InvalidOperationException("الفترة الماليه غير موجودة");
            if (from.Date != Convert.ToDateTime(dt.Rows[0]["DateFrom"]).Date) throw new InvalidOperationException("تاريخ بدايه الفترة المحدد يجب كما فى الفترة الماليه");
            if (to.Date > Convert.ToDateTime(dt.Rows[0]["DateTo"]).Date) throw new InvalidOperationException("تاريخ نهاية الفترة المحدد يجب كما فى الفترة الماليه");
        }
    }
}
