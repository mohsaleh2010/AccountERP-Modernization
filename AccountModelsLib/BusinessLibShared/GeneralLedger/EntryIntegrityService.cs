using System;
using System.Collections.Generic;
using System.Data;

namespace AccountModelsLib.BusinessLibShared.GeneralLedger
{
    /// <summary>
    /// «فحص سلامة القيود» (م5): قراءة فقط — لا يعدّل أى بيانات. يكشف القيود غير المتزنة، أرقام القيود المكررة، القيود بلا سطور،
    /// القيود التى مرجعها مستند غير موجود، أذون الصندوق بلا قيد، المراجع غير المسجَّلة، وفرق المرحَّل عن أرصدة الاستاذ.
    /// كل فحص يعيد عدد الحالات وأول 100 حالة للعرض.
    /// </summary>
    public class EntryIntegrityService
    {
        public enum Level { Error, Warning, Info }

        public class Check
        {
            public string Code { get; set; }
            public Level Level { get; set; }
            public int Count { get; set; }
            public DataTable Rows { get; set; }
            public string Error { get; set; }
        }

        private const decimal Tol = 0.005m;
        private readonly Func<operations> _newOpp;
        public EntryIntegrityService(Func<operations> newOpp = null) => _newOpp = newOpp ?? (() => new operations());

        public List<Check> Run(DateTime from, DateTime to, long branchId)
        {
            string range = " WHERE l.Entry_Date >= " + operations.Get_DateToInsert(from.Date) + " AND l.Entry_Date < " + operations.Get_DateToInsert(to.Date.AddDays(1)) +
                           (branchId > 0 ? " AND l.Branch_Id_Entry = " + branchId : "");
            string rangeAnd = range.Replace(" WHERE ", " AND ");
            string tol = Tol.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var list = new List<Check>();

            list.Add(Query("UNBAL", Level.Error,
                "SELECT TOP 100 l.ID, l.Entry_No, l.Entry_Date, l.journal_Code, SUM(d.Import_Value) AS Debit, SUM(d.Export_Value) AS Credit, SUM(d.Import_Value) - SUM(d.Export_Value) AS Diff " +
                "FROM Account_Entry_List l INNER JOIN Account_Entry_Details d ON d.Account_Entry_List_ID = l.ID " + range +
                " GROUP BY l.ID, l.Entry_No, l.Entry_Date, l.journal_Code HAVING ABS(SUM(d.Import_Value) - SUM(d.Export_Value)) > " + tol + " ORDER BY l.Entry_Date DESC",
                "SELECT COUNT(*) FROM (SELECT l.ID FROM Account_Entry_List l INNER JOIN Account_Entry_Details d ON d.Account_Entry_List_ID = l.ID " + range +
                " GROUP BY l.ID HAVING ABS(SUM(d.Import_Value) - SUM(d.Export_Value)) > " + tol + ") x"));

            list.Add(Query("UNBALEQ", Level.Error,
                "SELECT TOP 100 l.ID, l.Entry_No, l.Entry_Date, l.journal_Code, SUM(d.Equivalent_Import_Value) AS Debit, SUM(d.Equivalent_Export_Value) AS Credit, SUM(d.Equivalent_Import_Value) - SUM(d.Equivalent_Export_Value) AS Diff " +
                "FROM Account_Entry_List l INNER JOIN Account_Entry_Details d ON d.Account_Entry_List_ID = l.ID " + range +
                " GROUP BY l.ID, l.Entry_No, l.Entry_Date, l.journal_Code HAVING ABS(SUM(d.Equivalent_Import_Value) - SUM(d.Equivalent_Export_Value)) > " + tol + " ORDER BY l.Entry_Date DESC",
                "SELECT COUNT(*) FROM (SELECT l.ID FROM Account_Entry_List l INNER JOIN Account_Entry_Details d ON d.Account_Entry_List_ID = l.ID " + range +
                " GROUP BY l.ID HAVING ABS(SUM(d.Equivalent_Import_Value) - SUM(d.Equivalent_Export_Value)) > " + tol + ") x"));

            list.Add(Query("DUPNO", Level.Error,
                "SELECT TOP 100 l.Year_Financial_Period_Id, l.journal_Code, l.Entry_No, COUNT(*) AS Cnt, MIN(l.Entry_Date) AS FirstDate " +
                "FROM Account_Entry_List l " + range + " GROUP BY l.Year_Financial_Period_Id, l.journal_Code, l.Entry_No HAVING COUNT(*) > 1 ORDER BY COUNT(*) DESC",
                "SELECT COUNT(*) FROM (SELECT 1 AS X FROM Account_Entry_List l " + range + " GROUP BY l.Year_Financial_Period_Id, l.journal_Code, l.Entry_No HAVING COUNT(*) > 1) x"));

            list.Add(Query("NOLINES", Level.Error,
                "SELECT TOP 100 l.ID, l.Entry_No, l.Entry_Date, l.journal_Code FROM Account_Entry_List l " + range +
                " AND NOT EXISTS (SELECT 1 FROM Account_Entry_Details d WHERE d.Account_Entry_List_ID = l.ID) ORDER BY l.Entry_Date DESC",
                "SELECT COUNT(*) FROM Account_Entry_List l " + range + " AND NOT EXISTS (SELECT 1 FROM Account_Entry_Details d WHERE d.Account_Entry_List_ID = l.ID)"));

            // قيد مرجعه مستند غير موجود (فواتير 1/7، مشتريات 2/8، سندات صندوق 6)
            string orphanCond = " AND ((l.Reference_Type IN (1, 7) AND NOT EXISTS (SELECT 1 FROM Invoices_List i WHERE i.ID = l.Reference_Type_Table_Id)) " +
                                "OR (l.Reference_Type IN (2, 8) AND NOT EXISTS (SELECT 1 FROM Bills_List b WHERE b.ID = l.Reference_Type_Table_Id)) " +
                                "OR (l.Reference_Type = 6 AND NOT EXISTS (SELECT 1 FROM Order_Money o WHERE o.Id = l.Reference_Type_Table_Id)))";
            list.Add(Query("ORPHAN", Level.Error,
                "SELECT TOP 100 l.ID, l.Entry_No, l.Entry_Date, l.journal_Code, l.Reference_Type, l.Reference_Type_Table_Id FROM Account_Entry_List l " + range + orphanCond + " ORDER BY l.Entry_Date DESC",
                "SELECT COUNT(*) FROM Account_Entry_List l " + range + orphanCond));

            // أذون صندوق بلا قيد، أو مُعلَّمة مقيَّدة ولا قيد لها
            string brOrder = branchId > 0 ? " AND o.Branch_Id = " + branchId : "";
            string orderRange = " WHERE o.Order_Date >= " + operations.Get_DateToInsert(from.Date) + " AND o.Order_Date < " + operations.Get_DateToInsert(to.Date.AddDays(1)) + brOrder;
            string orderCond = " AND (ISNULL(o.IS_Posted_Flage, 0) = 0 OR NOT EXISTS (SELECT 1 FROM Account_Entry_List e WHERE e.ID = o.Account_Entry_List_ID))";
            list.Add(Query("SANDOUK", Level.Warning,
                "SELECT TOP 100 o.Id, o.Order_No, o.Order_Date, o.Transaction_type, o.Total_Value, o.IS_Posted_Flage, o.Account_Entry_List_ID FROM Order_Money o " + orderRange + orderCond + " ORDER BY o.Order_Date DESC",
                "SELECT COUNT(*) FROM Order_Money o " + orderRange + orderCond));

            // فواتير مبيعات/مشتريات بلا قيد (معلومة: قد يكون طبيعياً إن كان الفرع لا يقيّد فواتيره آلياً)
            list.Add(Query("INVNOENTRY", Level.Info,
                "SELECT TOP 100 i.ID, i.Invoice_Date, i.Invoice_Type, i.Branch_Id FROM Invoices_List i WHERE ISNULL(i.Account_Entry_List_ID, 0) = 0 AND i.Invoice_Date >= " + operations.Get_DateToInsert(from.Date) +
                " AND i.Invoice_Date < " + operations.Get_DateToInsert(to.Date.AddDays(1)) + (branchId > 0 ? " AND i.Branch_Id = " + branchId : "") + " ORDER BY i.Invoice_Date DESC",
                "SELECT COUNT(*) FROM Invoices_List i WHERE ISNULL(i.Account_Entry_List_ID, 0) = 0 AND i.Invoice_Date >= " + operations.Get_DateToInsert(from.Date) +
                " AND i.Invoice_Date < " + operations.Get_DateToInsert(to.Date.AddDays(1)) + (branchId > 0 ? " AND i.Branch_Id = " + branchId : "")));
            list.Add(Query("BILLNOENTRY", Level.Info,
                "SELECT TOP 100 b.ID, b.Invoice_Date FROM Bills_List b WHERE ISNULL(b.Account_Entry_List_ID, 0) = 0 AND b.Invoice_Date >= " + operations.Get_DateToInsert(from.Date) +
                " AND b.Invoice_Date < " + operations.Get_DateToInsert(to.Date.AddDays(1)) + " ORDER BY b.Invoice_Date DESC",
                "SELECT COUNT(*) FROM Bills_List b WHERE ISNULL(b.Account_Entry_List_ID, 0) = 0 AND b.Invoice_Date >= " + operations.Get_DateToInsert(from.Date) +
                " AND b.Invoice_Date < " + operations.Get_DateToInsert(to.Date.AddDays(1))));

            // مراجع غير مسجَّلة فى EntrySourceRegistry
            var known = new List<string>();
            foreach (var s in EntrySourceRegistry.All) known.Add(s.ReferenceType.ToString());
            string unknownCond = " AND l.Reference_Type IS NOT NULL AND l.Reference_Type NOT IN (" + string.Join(",", known) + ")";
            list.Add(Query("UNKNOWNREF", Level.Info,
                "SELECT TOP 100 l.Reference_Type, COUNT(*) AS Cnt FROM Account_Entry_List l " + range + unknownCond + " GROUP BY l.Reference_Type ORDER BY COUNT(*) DESC",
                "SELECT COUNT(*) FROM Account_Entry_List l " + range + unknownCond));

            list.Add(LedgerDiff(branchId));
            return list;
        }

        // مجموع المكافئ المدين للقيود المرحَّلة مقابل مجموعه فى أرصدة الاستاذ، لكل فترة وفرع (الفرق = ترحيل ناقص/زائد أو قيد عُدِّل بعد الترحيل).
        private Check LedgerDiff(long branchId)
        {
            var c = new Check { Code = "LEDGER", Level = Level.Error };
            try
            {
                string br = branchId > 0 ? " AND l.Branch_Id_Entry = " + branchId : "";
                string brM = branchId > 0 ? " AND m.Branch_Id = " + branchId : "";
                string sql =
                    "SELECT TOP 100 COALESCE(p.Period, m.Period) AS Period, COALESCE(p.Branch, m.Branch) AS Branch, ISNULL(p.Posted, 0) AS PostedDebit, ISNULL(m.Ledger, 0) AS LedgerDebit, ISNULL(p.Posted, 0) - ISNULL(m.Ledger, 0) AS Diff FROM " +
                    "(SELECT l.Year_Financial_Period_Id AS Period, l.Branch_Id_Entry AS Branch, SUM(d.Equivalent_Import_Value) AS Posted FROM Account_Entry_List l INNER JOIN Account_Entry_Details d ON d.Account_Entry_List_ID = l.ID " +
                    "WHERE l.IS_Transfer_To_Gl_Flage = 1" + br + " GROUP BY l.Year_Financial_Period_Id, l.Branch_Id_Entry) p " +
                    "FULL OUTER JOIN (SELECT m.Year_Financial_Period_Id AS Period, m.Branch_Id AS Branch, SUM(m.Equivalent_Madin_Value) AS Ledger FROM Account_Entry_Master_Balance m WHERE 1 = 1" + brM + " GROUP BY m.Year_Financial_Period_Id, m.Branch_Id) m " +
                    "ON p.Period = m.Period AND p.Branch = m.Branch";
                var all = _newOpp().return_DataTable(sql);
                var bad = all.Clone();
                foreach (DataRow r in all.Rows)
                    if (Math.Abs(Convert.ToDecimal(r["Diff"])) > Tol) bad.ImportRow(r);
                c.Rows = bad; c.Count = bad.Rows.Count;
            }
            catch (Exception ex) { c.Error = ex.Message; c.Rows = new DataTable(); }
            return c;
        }

        private Check Query(string code, Level level, string rowsSql, string countSql)
        {
            var c = new Check { Code = code, Level = level };
            try
            {
                var opp = _newOpp();
                c.Count = Convert.ToInt32(opp.return_DataTable(countSql).Rows[0][0]);
                c.Rows = c.Count > 0 ? opp.return_DataTable(rowsSql) : new DataTable();
            }
            catch (Exception ex) { c.Error = ex.Message; c.Rows = new DataTable(); }
            return c;
        }
    }
}
