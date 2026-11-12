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

        /// <summary>
        /// عملة «بالمكافىء» = 0: كل القيود بما يعادلها بالعملة الأساسية (Equivalent_Import/Export_Value وصفوف الأرصدة بـ Equivalent_Account_Currency_Id = 0).
        /// هذا ما تمرره كل شاشات الديسكتوب الختامية (`Currency_Data_AllItems("بالمكافىء")` بمعامل واحد ⇒ "0" فى Currency_List_2)،
        /// والمحرك (Mezan_MorgaaCLS / AccountEntry_CLS) لا يعرف إلا &gt;0 أو ==0. القيمة -1 (النسخة ذات المعاملين) لا تستعملها أى شاشة ختامية
        /// وكانت تنتج استعلاماً ناقصاً (financial-statements-review-plan.md أ-1، ف2). لا توجد عملة بقيمة 0 فى جدول Currency.
        /// </summary>
        public const long EquivalentCurrencyId = 0;

        public List<PeriodRow> GetPeriods()
        {
            var dt = new Financial_PeriodCLS().Get_Financial_Period();
            return dt.Rows.Cast<DataRow>().Select(r => new PeriodRow(
                Convert.ToInt64(r["Id"]),
                dt.Columns.Contains("Financial_Period_Name") ? r["Financial_Period_Name"]?.ToString() ?? "" : "",
                Convert.ToDateTime(r["DateFrom"]), Convert.ToDateTime(r["DateTo"]),
                dt.Columns.Contains("IS_Active") && r["IS_Active"] != DBNull.Value && Convert.ToInt32(r["IS_Active"]) == 1)).ToList();
        }

        /// <summary>العملات: الاختيار الأول «بالمكافىء» (0) ثم عملات الجدول بقيمتها (Value) كما فى Currency_List_2.</summary>
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

        // ---------------- قائمة الدخل والتشغيل/المتاجرة/الأرباح والخسائر (م4) ----------------

        /// <summary>نوع صف القائمة كما يضيفه المحرك: حساب، رأس مجموعة (-20)، إجمالى مجموعة أو إجمالى عام (-10)، سطر مُرحَّل (بلا معرّف).</summary>
        public enum StatementRowKind { Account, GroupHeader, SubTotal, Total, Carry }

        public class StatementRow
        {
            public StatementRowKind Kind { get; set; }
            public long AccountId { get; set; }
            public string Code { get; set; } = "";
            /// <summary>النص كما يعيده المحرك (اسم الحساب/المجموعة بعد إزالة بادئة «إجمالى» فى صفوف إجمالى المجموعة).</summary>
            public string Name { get; set; } = "";
            /// <summary>مفتاح ترجمة للنصوص الثابتة فى المحرك (المرحَّل، الإجماليات، صافى الربح/الخسارة)، أو فارغ.</summary>
            public string LabelKey { get; set; } = "";
            public decimal Import { get; set; }
            public decimal Export { get; set; }
            public decimal LastMadin { get; set; }
            public decimal LastDa2n { get; set; }
        }

        public class IncomeStatementResult
        {
            public List<StatementRow> Rows { get; set; } = new();
            /// <summary>صافى الربح كما يعيده المحرك: إجمالى الدائن (الإيرادات) ناقص إجمالى المدين (المصروفات)؛ السالب خسارة.</summary>
            public decimal NetProfit { get; set; }
            public int AccountCount => Rows.Count(r => r.Kind == StatementRowKind.Account);
        }

        public class ProfitLossResult
        {
            public List<StatementRow> Operating { get; set; } = new();
            public List<StatementRow> Trading { get; set; } = new();
            public List<StatementRow> ProfitLoss { get; set; } = new();
        }

        // النصوص الثابتة فى Incoming_StatementCLS ⇒ مفاتيح ترجمة (المسافات تُوحَّد قبل المطابقة).
        private static readonly Dictionary<string, string> EngineLabels = new()
        {
            ["إجمالى الإيرادات"] = "ISRevenueTotal",
            ["إجمالى المصروفات"] = "ISExpenseTotal",
            ["صافى الربح"] = "ISNetProfit",
            ["صافى الخساره"] = "ISNetLoss",
            ["****الإجمالى***"] = "PLTotal",
            ["تكلفة التشغيل مرحل الى حساب المتاجرة"] = "PLCarryOpToTrading",
            ["تكلفة التشغيل - مرحل من حساب التشغيل"] = "PLCarryFromOp",
            ["مرحل من حساب المتاجره"] = "PLCarryFromTrading",
            ["مجمل الخسارة مرحل الى حساب أرباح وخسائر"] = "PLGrossLossToPL",
            ["مجمل الربح مرحل الى حساب أرباح وخسائر"] = "PLGrossProfitToPL",
            ["مرحل الى حساب أرباح وخسائر"] = "PLToPL",
            ["صافى خسارة الفتره مرحل الى الميزانيه"] = "PLNetLossToBS",
            ["صافى ربح الفتره مرحل الى الميزانيه"] = "PLNetProfitToBS",
            ["مرحل الى الميزانيه"] = "PLToBS",
        };

        private static string Norm(string s) => string.Join(" ", (s ?? "").Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries));

        private static decimal Dz(DataRow r, string col)
        {
            if (!r.Table.Columns.Contains(col) || r[col] == DBNull.Value) return 0m;
            var v = r[col];
            if (v is decimal d) return d;
            return decimal.TryParse(Convert.ToString(v, System.Globalization.CultureInfo.InvariantCulture),
                System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var x) ? x : 0m;
        }

        /// <summary>يحوّل جدول المحرك (بعد إضافاته للرؤوس والإجماليات والمرحَّل) إلى صفوف مكتوبة النوع بنفس الترتيب. يُسقط الصفوف الفارغة تماماً.</summary>
        internal static List<StatementRow> BuildStatement(DataTable dt)
        {
            var list = new List<StatementRow>();
            foreach (DataRow r in dt.Rows)
            {
                bool hasId = r.Table.Columns.Contains("Submain_Account_ID") && r["Submain_Account_ID"] != DBNull.Value;
                long id = hasId ? Convert.ToInt64(r["Submain_Account_ID"]) : 0;
                string name = Norm(S(r, "Account_Name"));
                var row = new StatementRow
                {
                    AccountId = id > 0 ? id : 0,
                    Code = S(r, "Account_Code"),
                    Import = Dz(r, "Final_Import_Value"),
                    Export = Dz(r, "Final_Export_Value"),
                    LastMadin = Dz(r, "Final_Last_Balance_Madin"),
                    LastDa2n = Dz(r, "Final_Last_Balance_Da2n"),
                };
                EngineLabels.TryGetValue(name, out var key);
                if (id == -20)
                {
                    row.Kind = StatementRowKind.GroupHeader;
                    row.Name = name.Trim('*', ' ');
                    if (row.Name == "") continue; // رأس فارغ يضيفه المحرك قبل صفوف الإجمالى/المرحَّل
                }
                else if (id == -10)
                {
                    row.Kind = key != null ? StatementRowKind.Total : StatementRowKind.SubTotal;
                    row.LabelKey = key ?? "";
                    row.Name = key != null ? name : (name.StartsWith("إجمالى") ? name.Substring("إجمالى".Length).Trim() : name);
                }
                else if (id > 0)
                {
                    row.Kind = StatementRowKind.Account;
                    row.Name = name;
                }
                else
                {
                    if (name == "" && row.Import == 0 && row.Export == 0 && row.LastMadin == 0 && row.LastDa2n == 0) continue;
                    row.Kind = StatementRowKind.Carry;
                    row.LabelKey = key ?? "";
                    row.Name = name;
                }
                list.Add(row);
            }
            return list;
        }

        /// <summary>
        /// قائمة الدخل (Incoming_StatementByDate): نفس خطوات الديسكتوب ⇒ يملأ المحرك الجدول المؤقت (Is_To_Show_Prev=False وكل الحسابات كما فى ShowData)
        /// ثم Incoming_StatementCLS.Get_Incoming_Statement بالحسابات ذات الرصيد (1) — الاثنان داخل القفل لأن الجدول المؤقت عالمى.
        /// لا يُستدعى UpdateFirstBalance (يعيد كتابة Account_Entry_Master_Balance)؛ نفس قرار ميزان المراجعة (الخطة §3.4).
        /// </summary>
        public IncomeStatementResult GetIncomeStatement(PeriodRow period, long branchId, long currencyId, DateTime from, DateTime to)
        {
            var err = ValidateRange(period, from, to);
            if (err != null) throw new ArgumentException(err);

            var dt = new DataTable(); var dtPrint = new DataTable();
            decimal profit;
            EngineLock.Wait();
            try
            {
                int excp = 0; string msg = "";
                new Mezan_MorgaaCLS().Get_Mezan_Morgaa_ByPeiodNew(ref excp, ref msg, false, 0, branchId, from, to, 0, currencyId, period.Id, false, false, period.DateFrom);
                if (!string.IsNullOrEmpty(msg)) throw new InvalidOperationException(msg);
                profit = new Incoming_StatementCLS().Get_Incoming_Statement(ref dt, ref dtPrint, branchId, period.Id, 1, currencyId);
            }
            finally { EngineLock.Release(); }

            return new IncomeStatementResult { Rows = BuildStatement(dt), NetProfit = profit };
        }

        /// <summary>
        /// حساب التشغيل والمتاجرة والأرباح والخسائر (Profit_Loss_StatFRMNew): يملأ المحرك الجدول المؤقت كما فى GetTOShowData
        /// (Is_To_Show_Prev = بداية البحث بعد بداية الفترة) ثم Get_Final_AccountsArba7 الذى يحسب الثلاثة معاً بكل الحسابات (0)، داخل القفل.
        /// لا يُستدعى UpdateFirstBalance (نفس قرار ميزان المراجعة).
        /// </summary>
        public ProfitLossResult GetProfitLoss(PeriodRow period, long branchId, long currencyId, DateTime from, DateTime to)
        {
            var err = ValidateRange(period, from, to);
            if (err != null) throw new ArgumentException(err);

            DataTable op = new(), tr = new(), pl = new();
            EngineLock.Wait();
            try
            {
                int excp = 0; string msg = "";
                bool showPrev = from.Date > period.DateFrom.Date;
                new Mezan_MorgaaCLS().Get_Mezan_Morgaa_ByPeiodNew(ref excp, ref msg, showPrev, 0, branchId, from, to, 0, currencyId, period.Id, false, false, period.DateFrom);
                if (!string.IsNullOrEmpty(msg)) throw new InvalidOperationException(msg);
                decimal madin = 0, da2n = 0;
                new Incoming_StatementCLS().Get_Final_AccountsArba7(ref op, ref tr, ref pl, ref madin, ref da2n, branchId, period.Id, currencyId, 0);
            }
            finally { EngineLock.Release(); }

            return new ProfitLossResult { Operating = BuildStatement(op), Trading = BuildStatement(tr), ProfitLoss = BuildStatement(pl) };
        }

        // ---------------- قائمة المركز المالى (م5) ----------------

        /// <summary>عملة «المكافىء» كما تمررها Get_Account_BalanceFRM2 فى الديسكتوب (0 ثابتة، بلا اختيار عملة).</summary>
        public const long BaseEquivalentCurrencyId = EquivalentCurrencyId;

        public enum BalanceSection { Assets = 1, Liabilities = 2, Equity = 3 }

        public class BalanceNode
        {
            public long AccountId { get; set; }
            public string Code { get; set; } = "";
            public string Name { get; set; } = "";
            /// <summary>مفتاح ترجمة لسطر صافى الربح/الخسارة الصناعى، أو فارغ.</summary>
            public string LabelKey { get; set; } = "";
            public int Depth { get; set; }
            public bool IsProfit { get; set; }
            /// <summary>الرصيد بطبيعة القسم: الأصول مدين صافى، الخصوم وحقوق الملكية دائن صافى (السالب رصيد عكسى).</summary>
            public decimal Amount { get; set; }
            public List<BalanceNode> Children { get; set; } = new();
            public bool IsLeaf => Children.Count == 0;
        }

        public class BalanceSheetSection
        {
            public BalanceSection Section { get; set; }
            public List<BalanceNode> Roots { get; set; } = new();
            public decimal Total { get; set; }
        }

        public class BalanceSheetResult
        {
            public BalanceSheetSection Assets { get; set; } = new() { Section = BalanceSection.Assets };
            public BalanceSheetSection Liabilities { get; set; } = new() { Section = BalanceSection.Liabilities };
            public BalanceSheetSection Equity { get; set; } = new() { Section = BalanceSection.Equity };
            /// <summary>صافى الربح من Get_ProfitFrom_Incoming_Statement (السالب خسارة)، داخل إجمالى حقوق الملكية.</summary>
            public decimal NetProfit { get; set; }
            public decimal TotalLiabilitiesAndEquity => Liabilities.Total + Equity.Total;
            public decimal Difference => Assets.Total - TotalLiabilitiesAndEquity;
            public bool IsBalanced => Math.Abs(Difference) < 0.005m;
            /// <summary>حسابات ميزانية لها قيود مرحَّلة فى الفترة والفرع وليس لها صف فى أرصدة أول المدة (لن تظهر بلا UpdateFirstBalance).</summary>
            public int MissingAccounts { get; set; }
            public int AccountCount { get; set; }
            public DateTime AsOf { get; set; }
        }

        /// <summary>
        /// قائمة المركز المالى (بدل Get_Account_BalanceFRM2): يملأ المحرك الجدول المؤقت بنفس معاملات الديسكتوب (الفترة كاملة، العملة 0، كل الحسابات)
        /// ثم تُقرأ حسابات الميزانية (Account_Follows = 4) بنفس شرط BalanceCLS، وصافى الربح من Get_ProfitFrom_Incoming_Statement — داخل القفل.
        /// لا يُستدعى BalanceCLS.GetBalanceData_WithLevel (يفرّغ جدولاً عالمياً ثانياً وينفّذ ALTER TABLE ويكرر صفوف الأب ويجمع المدين+الدائن بالقيمة المطلقة
        /// ويثبّت الحساب 3)؛ الشجرة تُبنى هنا بالصافى، والأقسام حسب Head_Main_Account لجذر الحساب. لا يُستدعى UpdateFirstBalance (الخطة §3.4).
        /// </summary>
        public BalanceSheetResult GetBalanceSheet(PeriodRow period, long branchId)
        {
            if (period == null) throw new ArgumentException("PeriodRequired");
            if (branchId <= 0) throw new ArgumentException("BranchRequired");

            DataTable leaves; decimal profit;
            EngineLock.Wait();
            try
            {
                int excp = 0; string msg = "";
                new Mezan_MorgaaCLS().Get_Mezan_Morgaa_ByPeiodNew(ref excp, ref msg, false, 0, branchId, period.DateFrom, period.DateTo, 0,
                    BaseEquivalentCurrencyId, period.Id, false, false, period.DateFrom);
                if (!string.IsNullOrEmpty(msg)) throw new InvalidOperationException(msg);
                leaves = _newOpp().return_DataTable(
                    "SELECT s.Id, m.Final_Last_Balance_Madin, m.Final_Last_Balance_Da2n FROM Account_Entry_Master_Balance_MezanTemp m " +
                    "INNER JOIN Submain_Account s ON s.Id = m.Submain_Account_ID " +
                    "WHERE m.Year_Financial_Period_Id = " + period.Id + " AND m.Branch_Id = " + branchId +
                    " AND s.Account_Follows = 4 AND m.Equivalent_Account_Currency_Id = " + BaseEquivalentCurrencyId);
                profit = new Incoming_StatementCLS().Get_ProfitFrom_Incoming_Statement(branchId, period.Id, 1, BaseEquivalentCurrencyId);
            }
            finally { EngineLock.Release(); }

            var accounts = _newOpp().return_DataTable(
                "SELECT Id, Main_Account_Id, SubAccount_Name, Account_Code, SubAccount_NO, Head_Main_Account FROM Submain_Account");
            var result = BuildBalanceSheet(accounts, leaves, profit);
            result.AsOf = period.DateTo;

            // تنبيه التغطية (قراءة فقط): حسابات ميزانية لها قيود مرحَّلة فى الفترة والفرع بلا صف فى Account_Entry_Master_Balance للعملة 0.
            var miss = _newOpp().return_DataTable(
                "SELECT COUNT(DISTINCT d.Submain_Account_ID) AS N FROM Account_Entry_Details d " +
                "INNER JOIN Account_Entry_List l ON d.Account_Entry_List_ID = l.Id " +
                "INNER JOIN Submain_Account s ON s.Id = d.Submain_Account_ID " +
                "WHERE l.Entry_No IS NOT NULL AND s.Account_Follows = 4 AND l.Branch_Id_Entry = " + branchId +
                " AND l.Entry_Date >= " + operations.Get_DateToInsert(period.DateFrom.Date) + " AND l.Entry_Date <= " + operations.Get_DateToInsert(period.DateTo.Date) +
                " AND NOT EXISTS (SELECT 1 FROM Account_Entry_Master_Balance b WHERE b.Submain_Account_ID = d.Submain_Account_ID" +
                " AND b.Year_Financial_Period_Id = " + period.Id + " AND b.Branch_Id = " + branchId + " AND b.Equivalent_Account_Currency_Id = " + BaseEquivalentCurrencyId + ")");
            if (miss.Rows.Count > 0 && miss.Rows[0]["N"] != DBNull.Value) result.MissingAccounts = Convert.ToInt32(miss.Rows[0]["N"]);
            return result;
        }

        /// <summary>
        /// يبنى الشجرة من دليل الحسابات وأرصدة الحسابات الورقية: صافى الحساب = مدين − دائن، والأب = مجموع صافى أبنائه (صف واحد لكل حساب).
        /// القسم = Head_Main_Account لجذر الحساب (Main_Account_Id = 0): 1 أصول، 2 خصوم، 3 حقوق ملكية؛ غير ذلك ⇒ أصول.
        /// صافى الربح سطر أخير فى حقوق الملكية وداخل إجماليها.
        /// </summary>
        internal static BalanceSheetResult BuildBalanceSheet(DataTable accounts, DataTable leaves, decimal profit)
        {
            var info = new Dictionary<long, (long Parent, string Name, string Code, int Head)>();
            foreach (DataRow r in accounts.Rows)
            {
                long id = Convert.ToInt64(r["Id"]);
                long parent = r["Main_Account_Id"] == DBNull.Value ? 0 : Convert.ToInt64(r["Main_Account_Id"]);
                int head = r["Head_Main_Account"] == DBNull.Value ? 0 : Convert.ToInt32(r["Head_Main_Account"]);
                info[id] = (parent == id ? 0 : parent, S(r, "SubAccount_Name"), S(r, "Account_Code") != "" ? S(r, "Account_Code") : S(r, "SubAccount_NO"), head);
            }

            var net = new Dictionary<long, decimal>();
            foreach (DataRow r in leaves.Rows)
            {
                long id = Convert.ToInt64(r["Id"]);
                net[id] = (net.TryGetValue(id, out var v) ? v : 0m) + D(r, "Final_Last_Balance_Madin") - D(r, "Final_Last_Balance_Da2n");
            }

            var nodes = new Dictionary<long, BalanceNode>();
            var roots = new List<BalanceNode>();
            BalanceNode Node(long id)
            {
                if (nodes.TryGetValue(id, out var n)) return n;
                info.TryGetValue(id, out var a);
                n = new BalanceNode { AccountId = id, Name = a.Name ?? "", Code = a.Code ?? "" };
                nodes[id] = n;
                return n;
            }

            // كل حساب له رصيد يُربط بسلسلة آبائه حتى الجذر (مع حماية من الدورات).
            foreach (var id in net.Keys)
            {
                var child = Node(id);
                var seen = new HashSet<long> { id };
                long cur = id;
                while (true)
                {
                    long parent = info.TryGetValue(cur, out var a) ? a.Parent : 0;
                    if (parent == 0 || !info.ContainsKey(parent) || !seen.Add(parent))
                    {
                        if (!roots.Contains(child)) roots.Add(child);
                        break;
                    }
                    bool existed = nodes.ContainsKey(parent);
                    var p = Node(parent);
                    if (!p.Children.Contains(child)) p.Children.Add(child);
                    if (existed) break; // باقى السلسلة مربوط سابقاً
                    child = p; cur = parent;
                }
            }

            decimal Sum(BalanceNode n, int depth)
            {
                n.Depth = depth;
                n.Amount = n.Children.Count == 0 ? (net.TryGetValue(n.AccountId, out var v) ? v : 0m)
                    : n.Children.Sum(c => Sum(c, depth + 1)) + (net.TryGetValue(n.AccountId, out var own) ? own : 0m);
                return n.Amount;
            }
            void Flip(BalanceNode n) { n.Amount = -n.Amount; n.Children.ForEach(Flip); }
            void Order(BalanceNode n) { n.Children = n.Children.OrderBy(c => c.Code).ThenBy(c => c.AccountId).ToList(); n.Children.ForEach(Order); }

            var result = new BalanceSheetResult { NetProfit = profit, AccountCount = net.Count };
            foreach (var root in roots.OrderBy(r => r.Code).ThenBy(r => r.AccountId))
            {
                Sum(root, 0);
                Order(root);
                int head = info.TryGetValue(root.AccountId, out var a) ? a.Head : 0;
                var section = head == 2 ? result.Liabilities : head == 3 ? result.Equity : result.Assets;
                if (section.Section != BalanceSection.Assets) Flip(root);
                section.Roots.Add(root);
            }

            result.Equity.Roots.Add(new BalanceNode
            {
                IsProfit = true, Depth = 0, Amount = profit,
                LabelKey = profit >= 0 ? "BSNetProfit" : "BSNetLoss",
            });
            foreach (var s in new[] { result.Assets, result.Liabilities, result.Equity })
                s.Total = s.Roots.Sum(r => r.Amount);
            return result;
        }

        /// <summary>تسطيح القسم للعرض: حتى العمق المطلوب (0 = الكل)، ويُخفى الصفرى إلا إن طُلب (سطر صافى الربح يظهر دائماً).</summary>
        public static IEnumerable<BalanceNode> Flatten(BalanceSheetSection s, int maxLevel, bool showZero)
        {
            IEnumerable<BalanceNode> Walk(BalanceNode n)
            {
                if (maxLevel > 0 && n.Depth + 1 > maxLevel) yield break;
                if (!n.IsProfit && !showZero && Math.Abs(n.Amount) < 0.005m) yield break;
                yield return n;
                foreach (var c in n.Children)
                    foreach (var x in Walk(c)) yield return x;
            }
            return s.Roots.SelectMany(Walk);
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

        // ---------------- طباعة شجرة الحسابات (م6، بدل Submain_AccountPrintFrm) ----------------

        public class ChartAccountRow
        {
            public long Id { get; set; }
            public long ParentId { get; set; }
            public string Number { get; set; } = "";
            public string Name { get; set; } = "";
            public string ParentNumber { get; set; } = "";
            public string ParentName { get; set; } = "";
            /// <summary>العمق من الجذر (الجذر = 1)، محسوب من الشجرة نفسها لا من Account_Level المخزَّن.</summary>
            public int Depth { get; set; }
            public long RootId { get; set; }
            public string RootName { get; set; } = "";
            public int Follows { get; set; }
            public bool IsLeaf { get; set; }
            public string Currency { get; set; } = "";
            public bool IsShared { get; set; }
        }

        /// <summary>
        /// شجرة الحسابات للطباعة (قراءة فقط). الأصل `Submain_AccountCLS.get_TreeToPrint` يصفّى بـ Branch_Id = الفرع فقط
        /// (فيسقط الحسابات المشتركة Branch_ID = -1) ويهمل مرشح المستوى؛ هنا تُضمّ المشتركة اختيارياً والمرشحات تُطبَّق فى الصفحة.
        /// الترتيب كالأصل (Head_Main_Account ثم AccountCode1..6)، و«فرعى» = ليس له أبناء فى الشجرة.
        /// branchId = 0 ⇒ كل الفروع.
        /// </summary>
        public List<ChartAccountRow> GetChartOfAccounts(long branchId, bool includeShared)
        {
            string where = branchId > 0
                ? " WHERE (s.Branch_ID = " + branchId + (includeShared ? " OR s.Branch_ID = -1" : "") + ")"
                : "";
            var dt = _newOpp().return_DataTable(
                "SELECT s.Id, ISNULL(s.Main_Account_Id, 0) AS Main_Account_Id, s.SubAccount_NO, s.SubAccount_Name, ISNULL(s.Account_Follows, 0) AS Account_Follows, " +
                "ISNULL(s.Branch_ID, 0) AS Branch_ID, c.Currency_Name FROM Submain_Account s LEFT OUTER JOIN Currency c ON s.Currency_Id = c.Value" + where +
                " ORDER BY s.Head_Main_Account, s.AccountCode1, s.AccountCode2, s.AccountCode3, s.AccountCode4, s.AccountCode5, s.AccountCode6, s.Id");
            // كل الحسابات (بلا مرشح فرع) لحساب الأب والجذر والعمق حتى لو كان الأب فى فرع آخر أو مشتركاً
            var all = _newOpp().return_DataTable("SELECT Id, ISNULL(Main_Account_Id, 0) AS Main_Account_Id, SubAccount_NO, SubAccount_Name FROM Submain_Account");
            return BuildChart(dt, all);
        }

        /// <summary>دالة نقية: تبنى صفوف الطباعة (الأب، الجذر، العمق، فرعى/رئيسى) من جدولين.</summary>
        public static List<ChartAccountRow> BuildChart(DataTable rows, DataTable all)
        {
            var parent = new Dictionary<long, long>();
            var info = new Dictionary<long, (string No, string Name)>();
            var hasChild = new HashSet<long>();
            foreach (DataRow r in all.Rows)
            {
                long id = Convert.ToInt64(r["Id"]), p = Convert.ToInt64(r["Main_Account_Id"]);
                parent[id] = p;
                info[id] = (r["SubAccount_NO"]?.ToString() ?? "", r["SubAccount_Name"]?.ToString() ?? "");
                if (p > 0) hasChild.Add(p);
            }

            var list = new List<ChartAccountRow>();
            foreach (DataRow r in rows.Rows)
            {
                long id = Convert.ToInt64(r["Id"]), p = Convert.ToInt64(r["Main_Account_Id"]);
                // صعود للجذر مع حماية من الحلقات والآباء غير الموجودين
                int depth = 1; long root = id; var seen = new HashSet<long> { id };
                while (parent.TryGetValue(root, out var up) && up > 0 && info.ContainsKey(up) && seen.Add(up)) { root = up; depth++; }
                info.TryGetValue(p, out var pi);
                info.TryGetValue(root, out var ri);
                list.Add(new ChartAccountRow
                {
                    Id = id,
                    ParentId = p,
                    Number = r["SubAccount_NO"]?.ToString() ?? "",
                    Name = r["SubAccount_Name"]?.ToString() ?? "",
                    ParentNumber = p > 0 ? pi.No ?? "" : "",
                    ParentName = p > 0 ? pi.Name ?? "" : "",
                    Depth = depth,
                    RootId = root,
                    RootName = ri.Name ?? "",
                    Follows = Convert.ToInt32(r["Account_Follows"]),
                    IsLeaf = !hasChild.Contains(id),
                    Currency = r["Currency_Name"] == DBNull.Value ? "" : r["Currency_Name"]?.ToString() ?? "",
                    IsShared = Convert.ToInt64(r["Branch_ID"]) == -1,
                });
            }
            return list;
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
