using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace AccountModelsLib.BusinessLibShared.GeneralLedger
{
    /// <summary>
    /// خدمة القيد اليدوى لشاشات Razor الثلاث (بدل NewAccount_EntryFRM / NewAccount_EntryMadinDa2nFRM / NewSimpleAccount_EntryMadinDa2nFRM).
    /// الحفظ والتعديل والحذف عبر AccountEntry_CLS نفسه (Make_AccountEntry_LockedNo / Make_AccountEntry_Editable / Make_DeletEntry_Editable)
    /// مع الحارس المركزى EntryValidator. القيد اليدوى = Reference_Type 0 و IsAutomaticEntryOrManualy 2 كما فى الديسكتوب.
    /// قرارات: عدم الاتزان يمنع الحفظ دائماً (كالديسكتوب)؛ بقية المخالفات تحذير فى الوضع الافتراضى؛ تعديل السطر = حذفه وإضافته من جديد؛
    /// القيد المُرحَّل أو الآلى (Reference_Type &gt; 0) للقراءة فقط.
    /// </summary>
    public class ManualEntryService
    {
        public class Head
        {
            public long Id { get; set; }
            public long EntryNo { get; set; }
            public DateTime Date { get; set; } = DateTime.Today;
            public long JournalCode { get; set; }
            public long BranchId { get; set; }
            public string Notes { get; set; } = "";
            public string DocNo { get; set; } = "0";
            /// <summary>عملة القيد (الأسطر مُدخلة بها).</summary>
            public long EntryCurrencyId { get; set; }
            public string EntryCurrencyName { get; set; } = "";
            public decimal Factor { get; set; } = 1;
            public bool Posted { get; set; }
            public int ReferenceType { get; set; }
            public bool IsEditable => !Posted && ReferenceType == 0;
        }

        public class Line
        {
            public long DetailsId { get; set; }          // 0 = سطر جديد لم يُحفظ
            public long SubAccountId { get; set; }
            public string AccountName { get; set; } = "";
            public string AccountCode { get; set; } = "";
            public int AccountType { get; set; }          // Submain_Account.Account_Customer_Type_Value
            public long MainAccountId { get; set; }
            public long AccountCurrencyId { get; set; }
            public bool IsDebit { get; set; }
            public decimal Amount { get; set; }
            public long CostCenterId { get; set; }
            public string CostCenterName { get; set; } = "";
            public string Notes { get; set; } = "";
        }

        public class SaveResult
        {
            public bool Ok { get; set; }
            public long EntryId { get; set; }
            public long EntryNo { get; set; }
            public string Message { get; set; } = "";
            public List<EntryValidator.Issue> Warnings { get; } = new List<EntryValidator.Issue>();
        }

        private readonly Func<operations> _newOpp;
        public ManualEntryService(Func<operations> newOpp = null) => _newOpp = newOpp ?? (() => new operations());

        // ---------- بحث/قوائم ----------

        public DataTable GetJournals() => _newOpp().return_DataTable("SELECT Code, Journal_Name FROM Journal_Codes ORDER BY Code");

        public DataTable GetCurrencies() => _newOpp().return_DataTable("SELECT ID, Currency_Name FROM Currency WHERE Active = 1 ORDER BY ID");

        public decimal GetFactor(long entryCurrencyId)
        {
            if (entryCurrencyId == Common.Entry_Currency_Id || entryCurrencyId <= 0) return 1;
            try { return Common.RoundMe(new CurrencyFactorsCLS().GetFactors(Common.Entry_Currency_Id, entryCurrencyId)); }
            catch { return 1; }
        }

        public long NextEntryNo(DateTime date, long journalCode) => AccountEntry_CLS.NewEntry_No(date, (int)journalCode);

        public (string Code, long CurrencyId, int AcceptCostCenter, int AccountType, long MainAccountId) AccountInfo(long subAccountId)
        {
            var dt = _newOpp().return_DataTable("SELECT SubAccount_NO, Currency_Id, Accept_Cost_Center_Flage, Account_Customer_Type_Value, Main_Account_Id FROM Submain_Account WHERE Id = " + subAccountId);
            if (dt.Rows.Count == 0) return ("", 1, 0, 0, 0);
            var r = dt.Rows[0];
            long.TryParse(r["Currency_Id"]?.ToString(), out var cur);
            int.TryParse(r["Accept_Cost_Center_Flage"]?.ToString(), out var acc);
            int.TryParse(r["Account_Customer_Type_Value"]?.ToString(), out var typ);
            long.TryParse(r["Main_Account_Id"]?.ToString(), out var main);
            return (r["SubAccount_NO"]?.ToString() ?? "", cur == 0 ? 1 : cur, acc, typ, main);
        }

        // ---------- قراءة ----------

        public (Head Head, List<Line> Lines) Load(long entryId)
        {
            var dt = new AccountEntry_CLS().Get_Entry_Account(entryId);
            if (dt.Rows.Count == 0) return (null, new List<Line>());
            var r0 = dt.Rows[0];
            var head = new Head
            {
                Id = entryId,
                EntryNo = L(r0, "Entry_No"),
                Date = Convert.ToDateTime(r0["Entry_Date"]),
                JournalCode = L(r0, "journal_Code"),
                BranchId = L(r0, "Branch_Id_Entry"),
                Notes = r0["Entry_Notes"]?.ToString() ?? "",
                DocNo = r0["Doc_No"]?.ToString() ?? "",
                EntryCurrencyId = L(r0, "Entry_Currency_Id_List"),
                EntryCurrencyName = r0["Entry_Currency_Name"]?.ToString() ?? "",
                Factor = r0["Factor"] == DBNull.Value ? 1 : Convert.ToDecimal(r0["Factor"]),
                Posted = L(r0, "IS_Transfer_To_Gl_Flage") == 1,
                ReferenceType = (int)L(r0, "Reference_Type")
            };
            var lines = new List<Line>();
            foreach (DataRow r in dt.Rows)
            {
                decimal imp = D(r, "Import_Value"), exp = D(r, "Export_Value");
                lines.Add(new Line
                {
                    DetailsId = L(r, "Account_Entry_Details_Id"),
                    SubAccountId = L(r, "Submain_Account_ID"),
                    AccountName = r["SubAccount_Name"]?.ToString() ?? "",
                    AccountCode = r["SubAccount_NO"]?.ToString() ?? "",
                    AccountType = (int)L(r, "Account_Customer_Type_Value"),
                    MainAccountId = L(r, "MainAccount_Id"),
                    AccountCurrencyId = L(r, "Account_Currency_ID"),
                    IsDebit = imp > 0,
                    Amount = imp > 0 ? imp : exp,
                    CostCenterId = L(r, "CostCenter_Id"),
                    CostCenterName = r["CostCenter_Name"]?.ToString() ?? "",
                    Notes = r["Notes_Details"]?.ToString() ?? ""
                });
            }
            return (head, lines);
        }

        // ---------- حفظ جديد ----------

        public SaveResult SaveNew(Head h, IList<Line> lines)
        {
            var res = new SaveResult();
            string perm = "";
            if (!Common.PermissionToSaveEntryDateOutThisYear(ref perm, h.Date)) { res.Message = perm; return res; }
            if (h.JournalCode <= 0) { res.Message = "أدخل يومية القيد"; return res; }
            if (h.EntryCurrencyId <= 0) { res.Message = "أدخل عملة القيد"; return res; }

            var opp = _newOpp();
            DataTable dt = BuildDetails(opp, h, lines, 0);
            if (!CheckGuard(opp, dt, h.Date, res)) return res;

            long id = 0, no = 0; int excp = 0; string msg = "";
            var ac = new AccountEntry_CLS { Reference_Type = 0, Reference_Type_Table_Id = 0, IsAutomaticEntryOrManualy = 2 };
            ac.Make_AccountEntry_LockedNo(ref excp, ref msg, ref id, ref no, Common.User_Id, Common.User_Name, Common.Branch_Id, h.Date, h.JournalCode, Common.Entry_Currency_Id,
                h.Notes ?? "", Common.Transform_Type, dt, TotalEquivalent(dt), h.BranchId, 2, string.IsNullOrWhiteSpace(h.DocNo) ? "0" : h.DocNo, h.EntryCurrencyId, h.EntryCurrencyName ?? "", h.Factor);
            if (excp != 0) { res.Message = string.IsNullOrWhiteSpace(msg) ? "خطأ فى إنشاء القيد" : msg; return res; }
            res.Ok = true; res.EntryId = id; res.EntryNo = no;
            return res;
        }

        // ---------- تعديل ----------

        /// <summary>
        /// يعدّل رأس القيد ويحذف/يضيف أسطراً. lines = الحالة النهائية المطلوبة: سطر DetailsId &gt; 0 يُترك كما هو (لا تعديل فى مكانه)،
        /// وسطر DetailsId = 0 جديد، وأى سطر محفوظ غير موجود فى lines يُحذف.
        /// </summary>
        public SaveResult SaveEdit(Head h, IList<Line> lines)
        {
            var res = new SaveResult { EntryId = h.Id, EntryNo = h.EntryNo };
            var (cur, curLines) = Load(h.Id);
            if (cur == null) { res.Message = "القيد غير موجود"; return res; }
            if (!cur.IsEditable) { res.Message = cur.Posted ? "القيد مُرحَّل ولا يمكن تعديله (ألغِ الترحيل أولاً)" : "قيد آلى من مستند: يُعدَّل من مستنده"; return res; }
            string perm = "";
            if (!Common.PermissionToSaveEntryDateOutThisYear(ref perm, h.Date)) { res.Message = perm; return res; }

            var keep = new HashSet<long>(lines.Where(l => l.DetailsId > 0).Select(l => l.DetailsId));
            var deleted = new ArrayList();
            foreach (var l in curLines) if (!keep.Contains(l.DetailsId)) deleted.Add(l.DetailsId);

            var opp = _newOpp();
            // كل الأسطر (القائمة والجديدة) فى جدول واحد: القائمة Id_Test = 0 (لا تُمس) والجديدة Id_Test = رقم القيد.
            var existing = new AccountEntry_CLS().Get_Entry_Account(h.Id);
            var dt = existing.Clone();
            if (!dt.Columns.Contains("Id_Test")) dt.Columns.Add("Id_Test", typeof(long));
            foreach (DataRow r in existing.Rows)
                if (keep.Contains(L(r, "Account_Entry_Details_Id"))) { var n = dt.NewRow(); n.ItemArray = r.ItemArray; n["Id_Test"] = 0; dt.Rows.Add(n); }
            var newLines = lines.Where(l => l.DetailsId == 0).ToList();
            if (newLines.Count > 0)
            {
                var added = BuildDetails(opp, h, newLines, h.Id);
                foreach (DataRow r in added.Rows) { var n = dt.NewRow(); foreach (DataColumn c in added.Columns) if (dt.Columns.Contains(c.ColumnName)) n[c.ColumnName] = r[c]; dt.Rows.Add(n); }
            }
            if (!CheckGuard(opp, dt, h.Date, res)) return res;

            int excp = 0; string msg = "";
            new AccountEntry_CLS().Make_AccountEntry_Editable(ref excp, ref msg, h.Id, deleted, Common.User_Id, Common.User_Name, Common.Branch_Id, h.EntryNo, h.Date, h.JournalCode, Common.Entry_Currency_Id,
                h.Notes ?? "", Common.Transform_Type, dt, TotalEquivalent(dt), h.BranchId, 2, string.IsNullOrWhiteSpace(h.DocNo) ? "0" : h.DocNo, h.EntryCurrencyId, h.EntryCurrencyName ?? "", h.Factor);
            if (excp != 0) { res.Message = string.IsNullOrWhiteSpace(msg) ? "خطأ فى تعديل القيد" : msg; return res; }
            res.Ok = true;
            return res;
        }

        // ---------- حذف ----------

        public (bool Ok, string Message) Delete(long entryId)
        {
            if (!Common.IsHavePermission(Common.User_Id, 326, Common.Project_ID, Common.Branch_Id)) return (false, "ليس لديك صلاحية حذف القيود");
            var (head, _) = Load(entryId);
            if (head == null) return (false, "القيد غير موجود");
            if (!head.IsEditable) return (false, head.Posted ? "القيد مُرحَّل ولا يمكن حذفه (ألغِ الترحيل أولاً)" : "قيد آلى من مستند: يُحذف بحذف مستنده");
            int excp = 0; string msg = "";
            new AccountEntry_CLS().Make_DeletEntry_Editable(ref excp, ref msg, new AccountEntry_CLS().Get_Entry_Account(entryId), entryId);
            return excp == 0 ? (true, "") : (false, string.IsNullOrWhiteSpace(msg) ? "خطأ فى حذف القيد" : msg);
        }

        // ---------- داخلى ----------

        private DataTable BuildDetails(operations opp, Head h, IList<Line> lines, long idTest)
        {
            var ac = new AccountEntry_CLS();
            DataTable dt = null;
            ac.Get_Empty_dt_Details(ref opp, ref dt);
            // مدين أولاً ثم دائن (نفس ترتيب AddRow_WhenBuild الذى يدرج المدين قبل الدائن)
            foreach (var l in lines)
            {
                int excp = 0;
                ac.AddRow_WhenBuild(ref excp, ref dt, idTest, l.AccountCode, l.AccountType, l.MainAccountId, "", l.SubAccountId, l.AccountName, l.IsDebit ? 1 : 2, l.Amount, l.Notes ?? "",
                    h.Factor, l.AccountCurrencyId, Common.Entry_Currency_Id, l.CostCenterId, l.CostCenterName ?? "", h.BranchId, h.Factor);
                if (excp != 0) throw new InvalidOperationException("تعذّر بناء سطر القيد للحساب " + l.AccountName);
            }
            for (int i = 0; i < dt.Rows.Count; i++) dt.Rows[i]["Line_No"] = i + 1;
            return dt;
        }

        private bool CheckGuard(operations opp, DataTable dt, DateTime date, SaveResult res)
        {
            var issues = EntryValidator.Validate(opp, dt, date);
            var hard = issues.Where(i => i.Code == "BAL" || i.Code == "BALEQ" || i.Code == "LINES").ToList();
            if (hard.Count > 0 || EntryValidator.ShouldBlock(issues))
            {
                res.Message = string.Join(" | ", (hard.Count > 0 ? hard : issues).Select(i => i.ToString()));
                return false;
            }
            res.Warnings.AddRange(issues);
            return true;
        }

        private static double TotalEquivalent(DataTable dt)
        {
            decimal t = 0;
            foreach (DataRow r in dt.Rows) t += D(r, "Equivalent_Export_Value");
            return (double)t;
        }

        private static long L(DataRow r, string c) => r.Table.Columns.Contains(c) && r[c] != DBNull.Value && long.TryParse(r[c].ToString(), out var v) ? v : 0;
        private static decimal D(DataRow r, string c) => r.Table.Columns.Contains(c) && r[c] != DBNull.Value && decimal.TryParse(r[c].ToString(), out var v) ? v : 0m;
    }
}
