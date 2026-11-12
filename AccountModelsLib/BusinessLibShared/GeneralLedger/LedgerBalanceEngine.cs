using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;

namespace AccountModelsLib.BusinessLibShared.GeneralLedger
{
    /// <summary>
    /// محرك أرصدة للقراءة فقط (financial-statements-review-plan.md ب-1، ف3). بديل الويب لـ Mezan_MorgaaCLS.SetTemp_Mezan_Morgaa_ByPeiodNew:
    /// استعلام SELECT واحد بلا جداول مؤقتة ولا قفل ولا أى كتابة.
    /// <list type="bullet">
    /// <item>الافتتاحى المخزَّن من Account_Entry_Master_Balance (الفترة، الفرع، Equivalent_Account_Currency_Id = العملة؛ 0 = المكافىء)، مجموعاً لكل حساب
    /// (فى قاعدة التطوير صفوف مكررة لنفس الحساب؛ المحرك القديم كان يعرض كل صف منها).</item>
    /// <item>الحركة من سطور القيود (مصدر الحقيقة): Account_Entry_Details + Account_Entry_List بشرط Entry_No IS NOT NULL،
    /// بحدود نصف مفتوحة [بداية الفترة، من) للحركة السابقة و[من، إلى+1) لحركة المدى، فلا يسقط قيد بوقت فى يوم «إلى» (أ-4).</item>
    /// <item>FULL JOIN بين الاثنين: حساب له حركة بلا صف رصيد يظهر بافتتاحى صفر (أ-2)، وLEFT JOIN للحساب وأبيه فلا يسقط حساب بلا أب صالح.</item>
    /// </list>
    /// العملة &gt; 0: Import_Value/Export_Value للقيود بـ Entry_Currency_Id = العملة؛ العملة 0: Equivalent_Import/Export_Value لكل القيود (كما فى المحرك القديم).
    /// الفرع &lt;= 0 ⇒ كل الفروع (غير مستعمل فى الواجهة حتى ب-5).
    /// </summary>
    public class LedgerBalanceEngine
    {
        private readonly Func<operations> _newOpp;

        public LedgerBalanceEngine(Func<operations> newOpp = null) => _newOpp = newOpp ?? (() => new operations());

        public class Query
        {
            public long PeriodId { get; set; }
            /// <summary>بداية الفترة المالية: الحركة من هنا حتى «من» تُضاف للافتتاحى.</summary>
            public DateTime PeriodFrom { get; set; }
            public DateTime From { get; set; }
            /// <summary>آخر يوم مشمول (اليوم كله).</summary>
            public DateTime To { get; set; }
            public long BranchId { get; set; }
            /// <summary>0 = المكافىء.</summary>
            public long CurrencyId { get; set; }
            /// <summary>0 = كل الحسابات.</summary>
            public long AccountId { get; set; }
        }

        public class AccountBalance
        {
            public long AccountId { get; set; }
            public bool AccountExists { get; set; }
            public string Number { get; set; } = "";
            /// <summary>Account_Code وإلا SubAccount_NO (كعمود الكود فى الميزان القديم).</summary>
            public string Code { get; set; } = "";
            public string Name { get; set; } = "";
            public long ParentId { get; set; }
            public string ParentNumber { get; set; } = "";
            public string ParentName { get; set; } = "";
            public int HeadMainAccount { get; set; }
            public int AccountFollows { get; set; }
            public bool HasChildren { get; set; }
            /// <summary>صافى الافتتاحى المخزَّن (مدين − دائن).</summary>
            public decimal StoredOpening { get; set; }
            /// <summary>عدد صفوف الافتتاحى المخزَّنة (0 = لا صف؛ &gt;1 = مكرر).</summary>
            public int StoredOpeningRows { get; set; }
            public decimal PrevDebit { get; set; }
            public decimal PrevCredit { get; set; }
            public decimal Debit { get; set; }
            public decimal Credit { get; set; }

            /// <summary>الافتتاحى فى بداية المدى = المخزَّن + حركة ما قبل «من» داخل الفترة.</summary>
            public decimal Opening => StoredOpening + PrevDebit - PrevCredit;
            public decimal Movement => Debit - Credit;
            public decimal Closing => Opening + Movement;
        }

        public List<AccountBalance> GetBalances(Query q)
        {
            Validate(q);
            return Map(_newOpp().return_DataTable(BuildSql(q)));
        }

        public static void Validate(Query q)
        {
            if (q == null || q.PeriodId <= 0) throw new ArgumentException("PeriodRequired");
            if (q.To.Date < q.From.Date) throw new ArgumentException("ToBeforeFrom");
            if (q.From.Date < q.PeriodFrom.Date) throw new ArgumentException("FromOutsidePeriod");
        }

        /// <summary>تاريخ SQL بصيغة ISO ثابتة لا تتأثر بثقافة الخيط.</summary>
        internal static string SqlDate(DateTime d) => "CONVERT(DATETIME, '" + d.Date.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + "', 112)";

        internal static string BuildSql(Query q)
        {
            string inv(long v) => v.ToString(CultureInfo.InvariantCulture);
            bool byCurrency = q.CurrencyId > 0;
            string imp = byCurrency ? "d.Import_Value" : "d.Equivalent_Import_Value";
            string exp = byCurrency ? "d.Export_Value" : "d.Equivalent_Export_Value";
            string from = SqlDate(q.From), periodFrom = SqlDate(q.PeriodFrom), toExcl = SqlDate(q.To.Date.AddDays(1));

            var sb = new StringBuilder();
            sb.Append("WITH ob AS (SELECT b.Submain_Account_ID AS Id, SUM(ISNULL(b.First_Balance_Madin, 0) - ISNULL(b.First_Balance_Da2n, 0)) AS OpenNet, COUNT(*) AS OpenRows ");
            sb.Append("FROM Account_Entry_Master_Balance b WHERE b.Year_Financial_Period_Id = ").Append(inv(q.PeriodId));
            sb.Append(" AND b.Equivalent_Account_Currency_Id = ").Append(inv(q.CurrencyId));
            if (q.BranchId > 0) sb.Append(" AND b.Branch_Id = ").Append(inv(q.BranchId));
            if (q.AccountId > 0) sb.Append(" AND b.Submain_Account_ID = ").Append(inv(q.AccountId));
            sb.Append(" GROUP BY b.Submain_Account_ID), ");

            sb.Append("mv AS (SELECT d.Submain_Account_ID AS Id, ");
            sb.Append("SUM(CASE WHEN l.Entry_Date < ").Append(from).Append(" THEN ISNULL(").Append(imp).Append(", 0) ELSE 0 END) AS PrevDebit, ");
            sb.Append("SUM(CASE WHEN l.Entry_Date < ").Append(from).Append(" THEN ISNULL(").Append(exp).Append(", 0) ELSE 0 END) AS PrevCredit, ");
            sb.Append("SUM(CASE WHEN l.Entry_Date >= ").Append(from).Append(" THEN ISNULL(").Append(imp).Append(", 0) ELSE 0 END) AS Debit, ");
            sb.Append("SUM(CASE WHEN l.Entry_Date >= ").Append(from).Append(" THEN ISNULL(").Append(exp).Append(", 0) ELSE 0 END) AS Credit ");
            sb.Append("FROM Account_Entry_Details d INNER JOIN Account_Entry_List l ON d.Account_Entry_List_ID = l.Id ");
            sb.Append("WHERE l.Entry_No IS NOT NULL AND l.Entry_Date >= ").Append(periodFrom).Append(" AND l.Entry_Date < ").Append(toExcl);
            if (q.BranchId > 0) sb.Append(" AND l.Branch_Id_Entry = ").Append(inv(q.BranchId));
            if (byCurrency) sb.Append(" AND l.Entry_Currency_Id = ").Append(inv(q.CurrencyId));
            if (q.AccountId > 0) sb.Append(" AND d.Submain_Account_ID = ").Append(inv(q.AccountId));
            sb.Append(" GROUP BY d.Submain_Account_ID), ");

            sb.Append("k AS (SELECT COALESCE(ob.Id, mv.Id) AS Id, ISNULL(ob.OpenNet, 0) AS OpenNet, ISNULL(ob.OpenRows, 0) AS OpenRows, ");
            sb.Append("ISNULL(mv.PrevDebit, 0) AS PrevDebit, ISNULL(mv.PrevCredit, 0) AS PrevCredit, ISNULL(mv.Debit, 0) AS Debit, ISNULL(mv.Credit, 0) AS Credit ");
            sb.Append("FROM ob FULL OUTER JOIN mv ON ob.Id = mv.Id) ");

            sb.Append("SELECT k.Id, k.OpenNet, k.OpenRows, k.PrevDebit, k.PrevCredit, k.Debit, k.Credit, ");
            sb.Append("CASE WHEN s.Id IS NULL THEN 0 ELSE 1 END AS AccountExists, s.SubAccount_NO, s.Account_Code, s.SubAccount_Name, ");
            sb.Append("ISNULL(s.Main_Account_Id, 0) AS Main_Account_Id, ISNULL(s.Head_Main_Account, 0) AS Head_Main_Account, ISNULL(s.Account_Follows, 0) AS Account_Follows, ");
            sb.Append("p.SubAccount_NO AS Parent_NO, p.SubAccount_Name AS Parent_Name, ");
            sb.Append("CASE WHEN EXISTS (SELECT 1 FROM Submain_Account c WHERE c.Main_Account_Id = k.Id AND c.Id <> k.Id) THEN 1 ELSE 0 END AS HasChildren ");
            sb.Append("FROM k LEFT OUTER JOIN Submain_Account s ON s.Id = k.Id ");
            sb.Append("LEFT OUTER JOIN Submain_Account p ON p.Id = s.Main_Account_Id AND p.Id <> s.Id ");
            // نفس ترتيب الميزان القديم (Head_Main_Account ثم الأب ثم AccountCode1..6)
            sb.Append("ORDER BY ISNULL(s.Head_Main_Account, 0), ISNULL(s.Main_Account_Id, 0), s.AccountCode1, s.AccountCode2, s.AccountCode3, s.AccountCode4, s.AccountCode5, s.AccountCode6, k.Id");
            return sb.ToString();
        }

        /// <summary>دالة نقية: صفوف الاستعلام ⇒ أرصدة الحسابات بنفس الترتيب.</summary>
        public static List<AccountBalance> Map(DataTable dt)
        {
            var list = new List<AccountBalance>(dt.Rows.Count);
            foreach (DataRow r in dt.Rows)
            {
                string no = S(r, "SubAccount_NO"), code = S(r, "Account_Code");
                long id = Convert.ToInt64(r["Id"]);
                list.Add(new AccountBalance
                {
                    AccountId = id,
                    AccountExists = I(r, "AccountExists") == 1,
                    Number = no,
                    Code = code != "" ? code : no,
                    Name = S(r, "SubAccount_Name"),
                    ParentId = L(r, "Main_Account_Id") == id ? 0 : L(r, "Main_Account_Id"),
                    ParentNumber = S(r, "Parent_NO"),
                    ParentName = S(r, "Parent_Name"),
                    HeadMainAccount = I(r, "Head_Main_Account"),
                    AccountFollows = I(r, "Account_Follows"),
                    HasChildren = I(r, "HasChildren") == 1,
                    StoredOpening = D(r, "OpenNet"),
                    StoredOpeningRows = I(r, "OpenRows"),
                    PrevDebit = D(r, "PrevDebit"),
                    PrevCredit = D(r, "PrevCredit"),
                    Debit = D(r, "Debit"),
                    Credit = D(r, "Credit"),
                });
            }
            return list;
        }

        /// <summary>تقسيم صافٍ إلى (مدين، دائن) كما يفعل المحرك القديم: الموجب مدين والسالب دائن بالقيمة المطلقة.</summary>
        public static (decimal Debit, decimal Credit) Split(decimal net) => net >= 0 ? (net, 0m) : (0m, -net);

        private static string S(DataRow r, string c) => r.Table.Columns.Contains(c) && r[c] != DBNull.Value ? r[c].ToString()?.Trim() ?? "" : "";
        private static long L(DataRow r, string c) => r.Table.Columns.Contains(c) && r[c] != DBNull.Value ? Convert.ToInt64(r[c]) : 0;
        private static int I(DataRow r, string c) => r.Table.Columns.Contains(c) && r[c] != DBNull.Value ? Convert.ToInt32(r[c]) : 0;
        private static decimal D(DataRow r, string c) => r.Table.Columns.Contains(c) && r[c] != DBNull.Value ? Convert.ToDecimal(r[c]) : 0m;
    }
}
