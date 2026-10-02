using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace AccountModelsLib.Models
{
    /// <summary>صف قائمة البنوك/الخزائن (يقابل الأعمدة التى تعرضها BanksFrm/MoneyBanksFrm/SandoukBranch فى WinForms).</summary>
    public class BankGridRow
    {
        public long Id { get; set; }
        public long SubAccount_Id { get; set; }
        public string Bank_Name { get; set; } = "";
        public long Branch_Id { get; set; }
        public string Branch_Name { get; set; } = "";
        public string Account_No { get; set; } = "";
        public string Account_Code { get; set; } = "";
        public decimal Last_Balance { get; set; }
        /// <summary>رصيد أول المدة الفعلى: من Financial_Period_Begining_Balance لو وُجد، وإلا Banks.Primary_balance (نفس منطق Bank_FirstBalancePeriod فى الأصل).</summary>
        public decimal Primary_Balance { get; set; }
        /// <summary>معرّف سطر رصيد أول المدة فى الفترة المالية، أو -1 لو لا يوجد (يحدد مسار Edit_BanksFirst_Balance).</summary>
        public long Financial_Period_Begining_Balance_Id { get; set; } = -1;
        public DateTime? FirstBalanceDate { get; set; }
        public int Is_Bank { get; set; }
        public int Default_Bank { get; set; }
        public long Currency_Id { get; set; }
        public int Supplier_Bank { get; set; }
    }

    /// <summary>
    /// 🟢 قراءات قوائم البنوك/الخزائن/صندوق الفروع لواجهة Razor (بطلب المستخدم: كود الأعمال فى AccountModelsLib لا داخل الصفحة).
    /// الكتابة (إضافة/حذف/رصيد أول المدة/الافتراضى/الفرع) تبقى عبر BanksClss القائمة بلا تعديل.
    /// </summary>
    public class BanksGridCLS
    {
        private const string BankColumns =
            @"b.id, b.SubAccount_Id, b.Bank_Name, ISNULL(b.Branch_Id, 0) AS Branch_Id, ISNULL(b.Branch_Name, '') AS Branch_Name,
              ISNULL(b.Account_No, '') AS Account_No, ISNULL(b.Account_Code, '') AS Account_Code, ISNULL(b.Last_Balance, 0) AS Last_Balance,
              ISNULL(b.Primary_balance, 0) AS Primary_balance, b.firstBalanceDate, ISNULL(b.Is_Bank, 0) AS Is_Bank,
              ISNULL(b.Default_Bank, 0) AS Default_Bank, ISNULL(b.Currency_Id, 0) AS Currency_Id, ISNULL(b.Supplier_Bank, 0) AS Supplier_Bank";

        /// <summary>البنوك (نوع الحساب 7) أو الصناديق (6) - يقابل BanksClss.Get_Banks(Account_Type_Value, BankSupplier) مع رصيد أول المدة الفعلى.</summary>
        public List<BankGridRow> GetBanksGrid(int accountTypeValue, bool includeSupplierBanks)
        {
            var opp = new operations();
            string sql = "SELECT " + BankColumns + " FROM Banks b WHERE b.Account_Type_Value = " + accountTypeValue;
            if (!includeSupplierBanks)
            {
                sql += " AND ISNULL(b.Supplier_Bank, 0) = 0";
            }
            sql += " ORDER BY ISNULL(b.Is_Bank, 0), ISNULL(b.Supplier_Bank, 0), b.Bank_Name";
            var rows = Read(opp.return_DataTable(sql));
            ApplyFirstBalancePeriod(rows, accountTypeValue);
            ApplyCashBoxComputedBalance(rows, ref opp);
            ApplyBankComputedBalance(rows, ref opp);
            return rows;
        }

        /// <summary>كل الصناديق (Is_Bank=0) مع اسم فرعها من Account_Branches - يقابل BanksClss.Get_SandoukList لشاشة صندوق الفروع.</summary>
        public List<BankGridRow> GetCashBoxesWithBranch()
        {
            var opp = new operations();
            string sql = "SELECT " + BankColumns.Replace("ISNULL(b.Branch_Name, '') AS Branch_Name", "ISNULL(ab.Branch_Name, '') AS Branch_Name") +
                         " FROM Banks b LEFT OUTER JOIN Account_Branches ab ON b.Branch_Id = ab.Id WHERE ISNULL(b.Is_Bank, 0) = 0 ORDER BY b.Bank_Name";
            var rows = Read(opp.return_DataTable(sql));
            // 🟢 نفس رصيد أول المدة الفعلى المطبَّق أصلاً فى GetBanksGrid (كانت غائبة عن مسار "صندوق
            // الفروع" تحديداً) - بتوضيح المستخدم (2026-09-29).
            ApplyFirstBalancePeriod(rows, Common.Sandouk_Account_Type);
            ApplyCashBoxComputedBalance(rows, ref opp);
            return rows;
        }

        // 🟢 بتوضيح المستخدم (2026-09-29): رصيد الصندوق الحالى المعروض فى شاشة البيانات الأساسية يجب أن
        // يُحسَب دائماً من حركات الخزنة الفعلية (نفس ما تفعله شاشات الديسك توب حياً)، وليس من العمود
        // المخزَّن Banks.Last_Balance الذى قد لا يكون محدَّثاً - بنفس تحذير الذاكرة الدائمة
        // balances-computed-from-movements. يُستبدَل هنا فقط لصفوف الصناديق (Is_Bank=0) عبر
        // Money_Transaction_BoxOrderReportCLS.GetBank_Balance(ref opp, Bank_Id, true) - نفس الدالة التى
        // زوَّدنا بها المستخدم. حساب البنوك (Is_Bank=1) له طريقة مختلفة ستُضاف لاحقاً، فيبقى العمود
        // المخزَّن كما هو لصفوف البنوك حتى ذلك الحين.
        private static void ApplyCashBoxComputedBalance(List<BankGridRow> rows, ref operations opp)
        {
            if (rows.Count == 0) return;
            var mt = new Money_Transaction_BoxOrderReportCLS();
            foreach (var row in rows)
            {
                if (row.Is_Bank == 0)
                {
                    row.Last_Balance = mt.GetBank_Balance(ref opp, row.Id, true);
                }
            }
        }

        // 🟢 رصيد البنك الحالى: بتوضيح المستخدم (2026-09-29) يُحسَب من نفس محرك ميزان المراجعة بالفترة
        // (Mezan_MorgaaCLS.Get_Mezan_Morgaa_ByPeiodNew - يقابل حرفياً شاشة الديسك توب
        // Accounts.General_LedgerAccount.Mezan_Morgaa_ByPeriodByAccountDeletePrevious) - أول المدة + إجمالى
        // الحركات = صافى الرصيد (مدين/دائن)، لا العمود المخزَّن. يُستدعى مرة واحدة فقط لكل تجميعة
        // (فرع، عملة) بدل مرة لكل بنك لتقليل عدد مرات إعادة بناء جداول التقرير المؤقتة المشتركة
        // (Account_Entry_Master_Balance_MezanTemp/TransactionTemp) - وهى جداول عادية غير معزولة بالجلسة،
        // فاستدعاءان متزامنان من مستخدمين مختلفين قد يتضاربان؛ نفس المخاطرة الموجودة أصلاً فى شاشة ميزان
        // المراجعة الحية نفسها، لم تُدخَل هنا حديثاً وإنما أُعيد استخدامها كما طلب المستخدم صراحةً.
        private static void ApplyBankComputedBalance(List<BankGridRow> rows, ref operations opp)
        {
            var bankRows = rows.Where(r => r.Is_Bank == 1).ToList();
            if (bankRows.Count == 0) return;

            DateTime periodFrom = DateTime.MinValue;
            long yearPeriodId = Financial_PeriodCLS.Get_Year_Financial_Period_Id(DateTime.Now, ref periodFrom);
            if (yearPeriodId <= 0) return;

            var mm = new Mezan_MorgaaCLS();
            int excp = 0;
            string msg = "";

            foreach (var group in bankRows.GroupBy(r => new { r.Branch_Id, r.Currency_Id }))
            {
                DataTable dt;
                try
                {
                    dt = mm.Get_Mezan_Morgaa_ByPeiodNew(ref excp, ref msg, ref opp,
                        Is_To_Show_Prev: false, Show_AccountWithBalance: 0, Branch_Id: group.Key.Branch_Id,
                        dtFom: periodFrom, dtTo: DateTime.Now, Submain_Account_ID: 0, Entry_Currency_Id: group.Key.Currency_Id,
                        Year_Financial_Period_Id: yearPeriodId, IS_Set_NewAccountInFirstBalanceZero: false,
                        AnyDatePeriod: false, Financial_Period_dtFom: periodFrom);
                }
                catch
                {
                    continue;
                }
                if (dt == null || dt.Rows.Count == 0 || !dt.Columns.Contains("Submain_Account_ID")) continue;

                foreach (var row in group)
                {
                    DataRow match = null;
                    foreach (DataRow r in dt.Rows)
                    {
                        if (Convert.ToInt64(r["Submain_Account_ID"]) == row.SubAccount_Id) { match = r; break; }
                    }
                    if (match == null) continue;

                    decimal madin = match["Final_Last_Balance_Madin"] == DBNull.Value ? 0 : Convert.ToDecimal(match["Final_Last_Balance_Madin"], System.Globalization.CultureInfo.InvariantCulture);
                    decimal da2n = match["Final_Last_Balance_Da2n"] == DBNull.Value ? 0 : Convert.ToDecimal(match["Final_Last_Balance_Da2n"], System.Globalization.CultureInfo.InvariantCulture);
                    row.Last_Balance = madin - da2n;
                }
            }
        }

        private static List<BankGridRow> Read(DataTable dt)
        {
            var list = new List<BankGridRow>();
            foreach (DataRow r in dt.Rows)
            {
                list.Add(new BankGridRow
                {
                    Id = Convert.ToInt64(r["id"]),
                    SubAccount_Id = r["SubAccount_Id"] == DBNull.Value ? 0 : Convert.ToInt64(r["SubAccount_Id"]),
                    Bank_Name = r["Bank_Name"]?.ToString() ?? "",
                    Branch_Id = Convert.ToInt64(r["Branch_Id"]),
                    Branch_Name = r["Branch_Name"]?.ToString() ?? "",
                    Account_No = r["Account_No"]?.ToString() ?? "",
                    Account_Code = r["Account_Code"]?.ToString() ?? "",
                    Last_Balance = Convert.ToDecimal(r["Last_Balance"], System.Globalization.CultureInfo.InvariantCulture),
                    Primary_Balance = Convert.ToDecimal(r["Primary_balance"], System.Globalization.CultureInfo.InvariantCulture),
                    FirstBalanceDate = r["firstBalanceDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["firstBalanceDate"]),
                    Is_Bank = Convert.ToInt32(r["Is_Bank"]),
                    Default_Bank = Convert.ToInt32(r["Default_Bank"]),
                    Currency_Id = Convert.ToInt64(r["Currency_Id"]),
                    Supplier_Bank = Convert.ToInt32(r["Supplier_Bank"]),
                });
            }
            return list;
        }

        // يقابل Bank_FirstBalancePeriod فى BanksFrm: لو للبنك/الصندوق سطر رصيد أول مدة فى الفترة المالية يُستخدم Import_Value منه (أحدث DateFrom).
        private static void ApplyFirstBalancePeriod(List<BankGridRow> rows, int accountTypeValue)
        {
            if (rows.Count == 0) return;
            var opp = new operations();
            var dt = opp.return_DataTable(
                "SELECT Id, SubAccount_Unit_Item_Id, Import_Value, DateFrom FROM Financial_Period_Begining_Balance WHERE Account_Type_Value = " + accountTypeValue);
            var latest = dt.Rows.Cast<DataRow>()
                .GroupBy(r => Convert.ToInt64(r["SubAccount_Unit_Item_Id"]))
                .ToDictionary(g => g.Key, g => g.OrderByDescending(r => r["DateFrom"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(r["DateFrom"])).First());
            foreach (var row in rows)
            {
                if (latest.TryGetValue(row.Id, out var fp))
                {
                    row.Primary_Balance = fp["Import_Value"] == DBNull.Value ? 0 : Convert.ToDecimal(fp["Import_Value"], System.Globalization.CultureInfo.InvariantCulture);
                    row.Financial_Period_Begining_Balance_Id = Convert.ToInt64(fp["Id"]);
                }
            }
        }
    }
}
