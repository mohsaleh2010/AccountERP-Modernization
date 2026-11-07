using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using AccountModelsLib.Models;

namespace AccountModelsLib.BusinessLibShared.GeneralLedger
{
    /// <summary>
    /// الحسابات الختامية لشاشات Razor (خطة final-accounts-razor-plan.md): تغلّف محرك الديسكتوب كما هو
    /// (Mezan_MorgaaCLS.Get_Mezan_Morgaa_ByPeiodNew) بلا إعادة كتابة للمنطق لضمان تطابق الأرقام.
    /// المحرك يفرّغ ويملأ جدولين مؤقتين عالميين (Account_Entry_Master_Balance_MezanTemp / ..._TransactionTemp) فى كل عرض،
    /// لذلك كل استدعاء له يمر من قفل واحد على مستوى العملية حتى لا يفسد مستخدمان نتائج بعضهما.
    /// القفل لا يحمى من شاشة ديسكتوب مفتوحة بالتوازى (موثَّق فى الخطة).
    /// </summary>
    public class FinalAccountsService
    {
        // قفل عالمى للعملية: الجداول المؤقتة عالمية على مستوى القاعدة.
        private static readonly SemaphoreSlim EngineLock = new(1, 1);

        private readonly Func<operations> _newOpp;

        public FinalAccountsService(Func<operations> newOpp = null) => _newOpp = newOpp ?? (() => new operations());

        public record PeriodRow(long Id, string Name, DateTime DateFrom, DateTime DateTo, bool IsActive = false);
        public record Option(long Id, string Name);

        /// <summary>عملة «بالمكافىء» فى الديسكتوب (القيمة -1): كل القيود بما يعادلها بالعملة الأساسية.</summary>
        public const long EquivalentCurrencyId = -1;

        public List<PeriodRow> GetPeriods()
        {
            var dt = new Financial_PeriodCLS().Get_Financial_Period();
            return dt.Rows.Cast<DataRow>().Select(r => new PeriodRow(
                Convert.ToInt64(r["Id"]),
                dt.Columns.Contains("Financial_Period_Name") ? r["Financial_Period_Name"]?.ToString() ?? "" : "",
                Convert.ToDateTime(r["DateFrom"]), Convert.ToDateTime(r["DateTo"]),
                dt.Columns.Contains("IS_Active") && r["IS_Active"] != DBNull.Value && Convert.ToInt32(r["IS_Active"]) == 1)).ToList();
        }

        /// <summary>العملات: الاختيار الأول «بالمكافىء» (-1) ثم عملات الجدول بقيمتها (Value) كما فى Currency_List_2.</summary>
        public List<Option> GetCurrencies(string equivalentText)
        {
            var list = new List<Option> { new(EquivalentCurrencyId, equivalentText) };
            var dt = new CurrencyFactorsCLS_().GetCurrencyList();
            foreach (DataRow r in dt.Rows)
                list.Add(new Option(Convert.ToInt64(r["Value"]), r["Currency_Name"]?.ToString() ?? ""));
            return list;
        }

        public List<Option> GetBranches()
        {
            var dt = new Account_BrancheCLS().Get_DataUSER_DETAILES_Branches_Permission(false, Common.User_Id);
            return dt.Rows.Cast<DataRow>().Select(r => new Option(Convert.ToInt64(r["Id"]), r["Branch_Name"]?.ToString() ?? "")).ToList();
        }

        // ---------------- الأستاذ المساعد ----------------

        public record LedgerAccount(long Id, string Name, string Number, long TypeValue);

        /// <summary>اليوميات (Journal_Codes): Code / Journal_Name، والأول «كل اليوميات» = 0.</summary>
        public List<Option> GetJournals(string allText)
        {
            var list = new List<Option> { new(0, allText) };
            var dt = new operations().return_DataTable("Select * from Journal_Codes ORDER BY Code");
            foreach (DataRow r in dt.Rows) list.Add(new Option(Convert.ToInt64(r["Code"]), r["Journal_Name"]?.ToString() ?? ""));
            return list;
        }

        /// <summary>أنواع الحسابات (Account_Type الفعالة)، والأول «كل الحسابات» = 0.</summary>
        public List<Option> GetAccountTypes(string allText)
        {
            var list = new List<Option> { new(0, allText) };
            var dt = new operations().return_DataTable("Select * from Account_Type Where Active=1 ORDER BY Value");
            foreach (DataRow r in dt.Rows) list.Add(new Option(Convert.ToInt64(r["Value"]), r["Name"]?.ToString() ?? ""));
            return list;
        }

        /// <summary>
        /// شبكة الحسابات كما فى الديسكتوب (Get_MasterAccount_Entry_List): withTransactions=True ⇒ الحسابات التى لها قيود فى التاريخين واليومية
        /// والفرع والعملة والنوع؛ False ⇒ كل الحسابات الورقية. subAccountId>0 يقصرها على حساب واحد.
        /// </summary>
        public List<LedgerAccount> GetLedgerAccounts(DateTime from, DateTime to, long journal, long branch, long currency, int accountType, long subAccountId = 0, bool withTransactions = false)
        {
            var dt = new AccountEntry_CLS().Get_MasterAccount_Entry_List(from, to, journal, branch, currency, accountType, subAccountId, withTransactions);
            return dt.Rows.Cast<DataRow>().Select(r => new LedgerAccount(
                Convert.ToInt64(r["Submain_Account_ID"]),
                r["SubAccount_Name"]?.ToString()?.Trim() ?? "",
                r["SubAccount_NO"]?.ToString()?.Trim() ?? "",
                r.Table.Columns.Contains("Account_Customer_Type_Value") && r["Account_Customer_Type_Value"] != DBNull.Value ? Convert.ToInt64(r["Account_Customer_Type_Value"]) : 0)).ToList();
        }

        public class LedgerLine
        {
            public long EntryId { get; set; }
            public DateTime? Date { get; set; }
            public string EntryNo { get; set; } = "";
            public string Journal { get; set; } = "";
            public string Notes { get; set; } = "";
            public string OtherAccount { get; set; } = "";
            public string CostCenter { get; set; } = "";
            public string Currency { get; set; } = "";
            public string DocType { get; set; } = "";
            public string DocNo { get; set; } = "";
            public decimal Debit { get; set; }
            public decimal Credit { get; set; }
            public decimal BalanceDebit { get; set; }
            public decimal BalanceCredit { get; set; }
            /// <summary>0 حركة، 1 رصيد أول المدة/منقول، 2 إجمالى الحركة، 3 الرصيد المنقول الختامى (كما يضيفها المحرك).</summary>
            public int Kind { get; set; }
        }

        private static List<LedgerLine> BuildLedger(DataTable dt)
        {
            var lines = new List<LedgerLine>();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var r = dt.Rows[i];
                bool hasEntry = S(r, "Entry_No") != "" || (r.Table.Columns.Contains("Account_Entry_List_ID") && r["Account_Entry_List_ID"] != DBNull.Value);
                string notes = S(r, "Notes_Details");
                int kind = 0;
                if (!hasEntry)
                    kind = i == 0 ? 1 : (S(r, "Notes_Details") == "إجمالى الحركه" ? 2 : 3);
                lines.Add(new LedgerLine
                {
                    EntryId = r.Table.Columns.Contains("Account_Entry_List_ID") && r["Account_Entry_List_ID"] != DBNull.Value ? Convert.ToInt64(r["Account_Entry_List_ID"]) : 0,
                    Date = r.Table.Columns.Contains("Entry_Date") && r["Entry_Date"] != DBNull.Value ? Convert.ToDateTime(r["Entry_Date"]) : null,
                    EntryNo = S(r, "Entry_No"),
                    Journal = S(r, "Journal_Name"),
                    Notes = notes,
                    OtherAccount = S(r, "Master_Submain_Account_Name"),
                    CostCenter = S(r, "CostCenter_Name"),
                    Currency = S(r, "Currency_Name"),
                    DocType = S(r, "DocType"),
                    DocNo = S(r, "DocNo"),
                    Debit = D(r, "Import_Value"),
                    Credit = D(r, "Export_Value"),
                    BalanceDebit = D(r, "Balance_Import_Value"),
                    BalanceCredit = D(r, "Balance_Export_Value"),
                    Kind = kind,
                });
            }
            return lines;
        }

        /// <summary>الأستاذ المساعد عن فترة (Master_Account_Details2): بلا فترة مالية، التاريخان حران. يستدعى Get_MasterAccount_Entry_Details_Tarseed_NoPeriodNew.</summary>
        public List<LedgerLine> GetLedgerRange(long accountId, DateTime from, DateTime to, long currency, long journal, long branch, bool addBalanceAfter)
        {
            if (to.Date < from.Date) throw new ArgumentException("ToBeforeFrom");
            var dt = new AccountEntry_CLS().Get_MasterAccount_Entry_Details_Tarseed_NoPeriodNew(from, to, currency, journal, branch, accountId, addBalanceAfter);
            return BuildLedger(dt);
        }

        /// <summary>الأستاذ المساعد بالفترة المالية (Master_Account_DetailsTarseed22): رصيد أول المدة من الفترة المالية التى يقع فيها تاريخ البداية. يستدعى Get_MasterAccount_Entry_Details_TarseedNew.</summary>
        public List<LedgerLine> GetLedgerFiscal(long accountId, DateTime from, DateTime to, long currency, long journal, long branch)
        {
            if (to.Date < from.Date) throw new ArgumentException("ToBeforeFrom");
            DateTime periodFrom = from;
            long yearPeriodId = Financial_PeriodCLS.Get_Year_Financial_Period_Id(from, ref periodFrom);
            if (yearPeriodId == 0) throw new ArgumentException("NoFiscalPeriod");
            var dt = new AccountEntry_CLS().Get_MasterAccount_Entry_Details_TarseedNew(yearPeriodId, periodFrom, from, to, journal, branch, currency, accountId);
            return BuildLedger(dt);
        }

        // ---------------- الأستاذ المساعد لمراكز التكلفة بالفترة المالية (Master_Account_CostCenter_TarsesdFRM) ----------------

        public record CostCenterItem(long Id, string Code, string Name);

        /// <summary>
        /// شبكة المراكز كما فى الديسكتوب (Get_MasterAccount_CostCenter_Lis): المراكز التى لها أسطر قيود فى التاريخين.
        /// الديسكتوب يكتب مرشحات النوع/العملة/الحساب على Submain_Account دون ضمّه للاستعلام فيفشل عند اختيارها؛ هنا يُضمّ الجدول فتعمل.
        /// </summary>
        public List<CostCenterItem> GetCostCenterLedgerCenters(DateTime from, DateTime to, long journal, long branch, long currency, int accountType, long subAccountId = 0)
        {
            string sql = "SELECT DISTINCT c.Name, d.CostCenter_Id, c.Code_Str FROM Account_Entry_List l " +
                         "INNER JOIN Account_Entry_Details d ON l.Id = d.Account_Entry_List_ID " +
                         "INNER JOIN Cost_Center_Tree c ON d.CostCenter_Id = c.Id " +
                         "INNER JOIN Submain_Account a ON d.Submain_Account_ID = a.Id " +
                         "WHERE d.CostCenter_Id > 0 AND l.Entry_Date >= " + operations.Get_DateToInsert(from.Date) + " AND l.Entry_Date <= " + operations.Get_DateToInsert(to.Date);
            if (journal > 0) sql += " AND l.journal_Code = " + journal;
            if (branch > 0) sql += " AND l.Branch_Id_Entry = " + branch;
            if (accountType > 0) sql += " AND a.Account_Customer_Type_Value = " + accountType;
            if (currency > 0) sql += " AND a.Currency_Id = " + currency;
            if (subAccountId > 0) sql += " AND a.Id = " + subAccountId;
            var dt = new operations().return_DataTable(sql + " ORDER BY c.Code_Str");
            return dt.Rows.Cast<DataRow>().Select(r => new CostCenterItem(
                Convert.ToInt64(r["CostCenter_Id"]), r["Code_Str"]?.ToString()?.Trim() ?? "", r["Name"]?.ToString()?.Trim() ?? "")).ToList();
        }

        /// <summary>حركة مركز: يستدعى Get_MasterCostCenter_Details_TarseedNewNew بالفترة المالية المختارة (رصيد أول المدة من ترحيل المراكز + حركات الفترة حتى البداية).</summary>
        public List<LedgerLine> GetCostCenterLedger(PeriodRow period, long costCenterId, DateTime from, DateTime to, long currency, long journal, long branch, long accountId)
        {
            if (to.Date < from.Date) throw new ArgumentException("ToBeforeFrom");
            var dt = new AccountEntry_CLS().Get_MasterCostCenter_Details_TarseedNewNew(period.Id, period.DateFrom, from, to, journal, branch, costCenterId, currency, accountId);
            return BuildLedger(dt);
        }

        /// <summary>
        /// «تحديث رصيد أول المدة» (UpdateFirstBalanceCostCenter): يعيد بناء ترحيل أرصدة المراكز للفترة والفرع (جدول مشتق قابل لإعادة البناء، يحذف صفوف الفترة والفرع فقط).
        /// onlyIfEmpty=True ⇒ ينفَّذ فقط إن لم يكن للفترة والفرع ترحيل (فلا يحذف شيئاً). يعيد رسالة الخطأ أو null.
        /// </summary>
        public string RefreshCostCenterFirstBalance(PeriodRow period, long branch, long currency, int currencyCount, bool onlyIfEmpty)
        {
            EngineLock.Wait();
            try
            {
                if (onlyIfEmpty)
                {
                    var c = new operations().return_DataTable("SELECT COUNT(*) AS N FROM Account_Entry_Master_Balance_CostCenter WHERE Year_Financial_Period_Id = " + period.Id + " AND Branch_Id = " + branch);
                    if (c.Rows.Count > 0 && Convert.ToInt64(c.Rows[0]["N"]) > 0) return null;
                }
                int excp = 0; string msg = "";
                new Mezan_MorgaaCLS().UpdateFirstBalanceCostCenter(ref excp, ref msg, currency, branch, period.DateFrom, period.DateTo, period.Id, Math.Max(1, currencyCount));
                return excp != 0 ? msg : null;
            }
            finally { EngineLock.Release(); }
        }

        // ---------------- ميزان المراجعة ----------------

        /// <summary>0 كل الحسابات، 1 لها رصيد، 2 لها حركات بدون رصيد (rdBalanceAllZero / rdBalance / rdBalanceZeroTR).</summary>
        public enum BalanceFilter { All = 0, WithBalance = 1, MovementOnly = 2 }

        public class TrialBalanceRow
        {
            public long AccountId { get; set; }
            public string Code { get; set; } = "";
            public string AccountName { get; set; } = "";
            public string MainCode { get; set; } = "";
            public string MainAccountName { get; set; } = "";
            public decimal FirstMadin { get; set; }
            public decimal FirstDa2n { get; set; }
            public decimal Import { get; set; }
            public decimal Export { get; set; }
            public decimal TransactionMadin { get; set; }
            public decimal TransactionDa2n { get; set; }
            public decimal LastMadin { get; set; }
            public decimal LastDa2n { get; set; }
        }

        public class TrialBalanceGroup
        {
            public string MainCode { get; set; } = "";
            public string MainAccountName { get; set; } = "";
            public List<TrialBalanceRow> Rows { get; set; } = new();
            public TrialBalanceRow Total { get; set; } = new();
        }

        public class TrialBalanceResult
        {
            public List<TrialBalanceGroup> Groups { get; set; } = new();
            public TrialBalanceRow Total { get; set; } = new();
            public int RowCount => Groups.Sum(g => g.Rows.Count);
        }

        /// <summary>نفس تحققات ValidateDate فى الديسكتوب: التاريخان داخل الفترة المالية والنهاية لا تسبق البداية. يعيد رسالة الخطأ أو null.</summary>
        public static string ValidateRange(PeriodRow period, DateTime from, DateTime to)
        {
            if (period == null) return "PeriodRequired";
            if (from.Date < period.DateFrom.Date || from.Date > period.DateTo.Date) return "FromOutsidePeriod";
            if (to.Date < period.DateFrom.Date || to.Date > period.DateTo.Date) return "ToOutsidePeriod";
            if (to.Date < from.Date) return "ToBeforeFrom";
            return null;
        }

        /// <summary>
        /// يستدعى المحرك بنفس معاملات الديسكتوب: Is_To_Show_Prev = (بداية البحث بعد بداية الفترة)، وبقية العلامات False/0.
        /// المحرك لا يقبل فواصل ثابتة للمهلة؛ أى خطأ SQL يعود فى Msg ويُرمى كاستثناء.
        /// </summary>
        public TrialBalanceResult GetTrialBalance(PeriodRow period, long branchId, long currencyId, DateTime from, DateTime to, BalanceFilter filter)
        {
            var err = ValidateRange(period, from, to);
            if (err != null) throw new ArgumentException(err);

            DataTable dt;
            EngineLock.Wait();
            try
            {
                int excp = 0; string msg = "";
                bool showPrev = from.Date > period.DateFrom.Date;
                dt = new Mezan_MorgaaCLS().Get_Mezan_Morgaa_ByPeiodNew(ref excp, ref msg, showPrev, (int)filter, branchId, from, to,
                    0, currencyId, period.Id, false, false, period.DateFrom);
                if (!string.IsNullOrEmpty(msg)) throw new InvalidOperationException(msg);
            }
            finally { EngineLock.Release(); }

            return BuildTrialBalance(dt);
        }

        private static decimal D(DataRow r, string col) =>
            r.Table.Columns.Contains(col) && r[col] != DBNull.Value ? Convert.ToDecimal(r[col]) : 0m;

        private static string S(DataRow r, string col) =>
            r.Table.Columns.Contains(col) && r[col] != DBNull.Value ? r[col].ToString()?.Trim() ?? "" : "";

        internal static TrialBalanceResult BuildTrialBalance(DataTable dt)
        {
            var result = new TrialBalanceResult();
            TrialBalanceGroup group = null;
            foreach (DataRow r in dt.Rows)
            {
                var row = new TrialBalanceRow
                {
                    AccountId = r.Table.Columns.Contains("Submain_Account_ID") && r["Submain_Account_ID"] != DBNull.Value ? Convert.ToInt64(r["Submain_Account_ID"]) : 0,
                    Code = S(r, "Account_Code") != "" ? S(r, "Account_Code") : S(r, "SubAccount_NO"),
                    AccountName = S(r, "Account_Name"),
                    MainCode = S(r, "MainCode"),
                    MainAccountName = S(r, "MainAccount_Name"),
                    FirstMadin = D(r, "Final_Fist_Balance_Madin"),
                    FirstDa2n = D(r, "Final_Fist_Balance_Da2n"),
                    Import = D(r, "Final_Import_Value"),
                    Export = D(r, "Final_Export_Value"),
                    TransactionMadin = D(r, "Final_Transaction_Madin"),
                    TransactionDa2n = D(r, "Final_Transaction_Da2n"),
                    LastMadin = D(r, "Final_Last_Balance_Madin"),
                    LastDa2n = D(r, "Final_Last_Balance_Da2n"),
                };
                // مجموعات الحساب الرئيسى (MainCode) بترتيب ظهورها كما يعيدها المحرك
                if (group == null || group.MainCode != row.MainCode || group.MainAccountName != row.MainAccountName)
                {
                    group = new TrialBalanceGroup { MainCode = row.MainCode, MainAccountName = row.MainAccountName };
                    result.Groups.Add(group);
                }
                group.Rows.Add(row);
                Accumulate(group.Total, row);
                Accumulate(result.Total, row);
            }
            return result;
        }

        private static void Accumulate(TrialBalanceRow to, TrialBalanceRow r)
        {
            to.FirstMadin += r.FirstMadin; to.FirstDa2n += r.FirstDa2n;
            to.Import += r.Import; to.Export += r.Export;
            to.TransactionMadin += r.TransactionMadin; to.TransactionDa2n += r.TransactionDa2n;
            to.LastMadin += r.LastMadin; to.LastDa2n += r.LastDa2n;
        }
    }
}
