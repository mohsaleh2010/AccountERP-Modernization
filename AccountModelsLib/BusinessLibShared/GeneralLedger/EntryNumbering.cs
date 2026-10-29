using System;
using System.Data;

namespace AccountModelsLib.BusinessLibShared.GeneralLedger
{
    /// <summary>
    /// ترقيم القيد الآمن (ق2): نفس قاعدة AccountEntry_CLS.NewEntry_No (MAX+1 لكل فترة مالية ولكل يومية إن كانت أكبر من 0)
    /// لكن القراءة داخل معاملة المستدعى بقفل UPDLOCK/HOLDLOCK، فلا يحصل مستخدمان على الرقم نفسه.
    /// يجب استدعاؤها داخل معاملة مفتوحة (opp) وإلا لا يبقى القفل بعد القراءة.
    /// </summary>
    public static class EntryNumbering
    {
        public static long Next(operations opp, DateTime entryDate, long journalCode)
        {
            long period = Financial_PeriodCLS.Get_Year_Financial_Period_Id(entryDate);
            string sql = "SELECT MAX(Entry_No) AS M FROM Account_Entry_List WITH (UPDLOCK, HOLDLOCK) WHERE Year_Financial_Period_Id = " + period;
            if (journalCode > 0) sql += " AND journal_Code = " + journalCode;
            DataTable dt = opp.return_DataTable(sql);
            if (dt.Rows.Count == 0 || dt.Rows[0]["M"] == DBNull.Value) return 1;
            return Convert.ToInt64(dt.Rows[0]["M"]) + 1;
        }
    }
}
