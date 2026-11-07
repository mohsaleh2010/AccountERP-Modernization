using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using AccountModelsLib.BusinessLibShared.CostCenters;
using AccountModelsLib.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib.BusinessLibShared.FixedAssets
{
    /// <summary>
    /// قيد الإهلاك (خطة مراكز التكلفة م3 دفعة 5): لكل سجل إهلاك محسوب فى SubMainAssets_Ehlak قيد مستقل
    /// مدين حساب مصروف الإهلاك (بمركز الأصل إن قبله الحساب) / دائن حساب مجمع الإهلاك، بتاريخ نهاية الفترة.
    /// الإعداد لكل فرع ويبدأ مغلقاً (لا صف = السلوك القديم: سجل بلا قيد). الربط بين السجل وقيده فى Fixed_Asset_Depreciation_Entry
    /// فيُحذف القيد مع حذف السجل من الشاشة. لا يُلمس أى سجل أو قيد قديم.
    /// </summary>
    public class AssetDepreciationPostingService
    {
        public const int ReferenceType = 3010;

        public class Setting
        {
            public bool PostEntries { get; set; }
            public long ExpenseAccountId { get; set; }
            public string ExpenseAccountName { get; set; } = "";
            public long AccumulatedAccountId { get; set; }
            public string AccumulatedAccountName { get; set; } = "";
        }

        public Setting GetSetting(long branchId)
        {
            var opp = new operations();
            var dt = opp.return_DataTable(@"IF OBJECT_ID(N'dbo.Fixed_Asset_Depreciation_Setting', N'U') IS NULL SELECT TOP 0 CAST(0 AS bit) AS Post_Entries, CAST(0 AS bigint) AS E, CAST(0 AS bigint) AS A, N'' AS EN, N'' AS AN ELSE
SELECT s.Post_Entries, ISNULL(s.Expense_Account_Id,0) AS E, ISNULL(s.Accumulated_Account_Id,0) AS A, ISNULL(e.SubAccount_Name,N'') AS EN, ISNULL(a.SubAccount_Name,N'') AS AN
FROM Fixed_Asset_Depreciation_Setting s LEFT JOIN Submain_Account e ON e.Id = s.Expense_Account_Id LEFT JOIN Submain_Account a ON a.Id = s.Accumulated_Account_Id
WHERE s.Account_Branch_Id = " + branchId.ToString(CultureInfo.InvariantCulture));
            if (dt.Rows.Count == 0) return new Setting();
            var r = dt.Rows[0];
            return new Setting
            {
                PostEntries = Convert.ToBoolean(r["Post_Entries"]),
                ExpenseAccountId = Convert.ToInt64(r["E"]), ExpenseAccountName = r["EN"].ToString() ?? "",
                AccumulatedAccountId = Convert.ToInt64(r["A"]), AccumulatedAccountName = r["AN"].ToString() ?? "",
            };
        }

        public void SaveSetting(long branchId, bool post, long expenseId, long accumulatedId)
        {
            if (post)
            {
                if (expenseId <= 0 || accumulatedId <= 0) throw new InvalidOperationException("حدد حساب مصروف الإهلاك وحساب مجمع الإهلاك.");
                if (expenseId == accumulatedId) throw new InvalidOperationException("حساب المصروف يختلف عن حساب المجمع.");
            }
            var opp = new operations();
            long userId = Common.User_Id;
            DateTime now = DateTime.Now;
            long? e = expenseId > 0 ? expenseId : null, a = accumulatedId > 0 ? accumulatedId : null;
            opp.db.Database.ExecuteSqlInterpolated($@"
IF EXISTS (SELECT 1 FROM Fixed_Asset_Depreciation_Setting WHERE Account_Branch_Id = {branchId})
    UPDATE Fixed_Asset_Depreciation_Setting SET Post_Entries = {post}, Expense_Account_Id = {e}, Accumulated_Account_Id = {a}, User_Id = {userId}, Update_ServerDateTime = {now} WHERE Account_Branch_Id = {branchId}
ELSE
    INSERT INTO Fixed_Asset_Depreciation_Setting (Account_Branch_Id, Post_Entries, Expense_Account_Id, Accumulated_Account_Id, User_Id, Update_ServerDateTime) VALUES ({branchId}, {post}, {e}, {a}, {userId}, {now})");
        }

        /// <summary>
        /// يقيّد سجلات الإهلاك المحسوبة للفرع والفترة التى لا قيد لها بعد. يرجع عدد القيود. لا يفعل شيئاً إن كان الإعداد مغلقاً.
        /// كل شىء فى معاملة واحدة: فشل قيد يلغى الكل (السجلات المحسوبة تبقى وتُقيَّد فى المحاولة التالية).
        /// </summary>
        public int PostForPeriod(long branchId, DateTime dateFrom, DateTime dateTo)
        {
            var setting = GetSetting(branchId);
            if (!setting.PostEntries) return 0;
            var opp = new operations(1);
            try
            {
                var rows = opp.return_DataTable($@"
SELECT h.Id, h.Assets_Account_ID, ISNULL(h.Period_Ehalk,0) AS Amount, ISNULL(a.SubAccount_Name, N'') AS AssetName, f.Id AS FixedId
FROM SubMainAssets_Ehlak h
INNER JOIN FixedAssetData f ON f.SubAccount_Id = h.Assets_Account_ID
INNER JOIN Submain_Account a ON a.Id = h.Assets_Account_ID
WHERE h.DateFrom = '{operations.dateFormat(dateFrom)}' AND h.DateTo = '{operations.dateFormat(dateTo)}' AND ISNULL(h.Period_Ehalk,0) > 0
  AND f.Branch_Id = {branchId}
  AND NOT EXISTS (SELECT 1 FROM Fixed_Asset_Depreciation_Entry x WHERE x.Ehlak_Id = h.Id)");
                int n = 0;
                foreach (DataRow r in rows.Rows)
                {
                    long ehlakId = Convert.ToInt64(r["Id"]);
                    long entryId = Entry(opp, ehlakId, Convert.ToDecimal(r["Amount"]), r["AssetName"].ToString() ?? "", dateFrom, dateTo, branchId, setting);
                    int ex = 0;
                    opp.update_me("INSERT INTO Fixed_Asset_Depreciation_Entry (Ehlak_Id, Entry_Id) VALUES (" + ehlakId + "," + entryId + ")", ref ex);
                    // مركز الأصل على سطر المصروف (إن قبله الحساب)
                    long center = InvoiceCostCenter.GetAssetCenter(opp, Convert.ToInt64(r["FixedId"]));
                    InvoiceCostCenter.ApplyToEntry(opp, entryId, center);
                    n++;
                }
                opp.Commit();
                return n;
            }
            catch
            {
                opp.Rollback();
                throw;
            }
        }

        /// <summary>يحذف قيود سجلات الإهلاك المحدَّدة (القيود التى أنشأها هذا النظام فقط) ويفك الربط. يُستدعى قبل حذف السجلات نفسها.</summary>
        public void DeleteEntries(IEnumerable<long> ehlakIds)
        {
            var ids = ehlakIds.Where(i => i > 0).ToList();
            if (ids.Count == 0) return;
            var opp = new operations(1);
            try
            {
                var dt = opp.return_DataTable("IF OBJECT_ID(N'dbo.Fixed_Asset_Depreciation_Entry', N'U') IS NULL SELECT TOP 0 CAST(0 AS bigint) AS Entry_Id ELSE SELECT Entry_Id FROM Fixed_Asset_Depreciation_Entry WHERE Ehlak_Id IN (" + string.Join(",", ids) + ")");
                var aa = new AccountEntry_CLS();
                foreach (DataRow r in dt.Rows)
                {
                    int excp = 0; string msg = "";
                    aa.Delete_Account_Entry(ref excp, ref msg, ref opp, Convert.ToInt64(r["Entry_Id"]));
                }
                if (dt.Rows.Count > 0)
                {
                    int e = 0;
                    opp.update_me("DELETE FROM Fixed_Asset_Depreciation_Entry WHERE Ehlak_Id IN (" + string.Join(",", ids) + ")", ref e);
                }
                opp.Commit();
            }
            catch
            {
                opp.Rollback();
                throw;
            }
        }

        private record AccountInfo(long Id, int Type, string Name, string Code, long CurrencyId, long MainAccountId);

        private static AccountInfo Account(operations opp, long id)
        {
            var dt = opp.return_DataTable("SELECT Id, ISNULL(Account_Customer_Type_Value, 0) AS T, SubAccount_Name, SubAccount_NO, ISNULL(Currency_Id, 1) AS Cur, ISNULL(Main_Account_Id, 0) AS M FROM Submain_Account WHERE Id = " + id);
            if (dt.Rows.Count == 0) return null;
            var r = dt.Rows[0];
            long cur = Convert.ToInt64(r["Cur"]);
            return new AccountInfo(Convert.ToInt64(r["Id"]), Convert.ToInt32(r["T"]), r["SubAccount_Name"]?.ToString()?.Trim() ?? "", r["SubAccount_NO"]?.ToString()?.Trim() ?? "",
                cur <= 0 ? 1 : cur, Convert.ToInt64(r["M"]));
        }

        private static long Entry(operations opp, long ehlakId, decimal value, string assetName, DateTime from, DateTime to, long branchId, Setting s)
        {
            var debit = Account(opp, s.ExpenseAccountId) ?? throw new InvalidOperationException("حساب مصروف الإهلاك غير موجود فى الدليل.");
            var credit = Account(opp, s.AccumulatedAccountId) ?? throw new InvalidOperationException("حساب مجمع الإهلاك غير موجود فى الدليل.");
            string notes = $"إهلاك الأصل {assetName} عن الفترة {from:yyyy-MM-dd} إلى {to:yyyy-MM-dd}";
            long entryCurrency = Common.Entry_Currency_Id <= 0 ? 1 : Common.Entry_Currency_Id;
            var curName = opp.return_DataTable("SELECT Currency_Name FROM Currency WHERE Value = " + entryCurrency);
            string currencyName = curName.Rows.Count > 0 ? curName.Rows[0][0]?.ToString() ?? "" : "";
            var aa = new AccountEntry_CLS { Reference_Type = ReferenceType, Reference_Type_Table_Id = ehlakId };
            var dt = new DataTable();
            aa.Get_Empty_dt_Details(ref opp, ref dt);
            int excp = 0;
            aa.AddRow_WhenBuild(ref excp, ref dt, 0, debit.Code, debit.Type, debit.MainAccountId, "", debit.Id, debit.Name, 1, value, notes, 1, debit.CurrencyId, entryCurrency, 0, "", branchId, 1);
            if (excp == 0) aa.AddRow_WhenBuild(ref excp, ref dt, 0, credit.Code, credit.Type, credit.MainAccountId, "", credit.Id, credit.Name, 2, value, notes, 1, credit.CurrencyId, entryCurrency, 0, "", branchId, 1);
            if (excp != 0) throw new InvalidOperationException("تعذر تجهيز أسطر قيد الإهلاك.");
            for (int i = 0; i < dt.Rows.Count; i++) dt.Rows[i]["Line_No"] = i + 1;

            const long journalCode = 1;
            long entryNo = AccountEntry_CLS.NewEntry_No(ref opp, to, journalCode, 0);
            long entryId = 0;
            string msg = "";
            aa.Make_AccountEntry(ref excp, ref msg, ref entryId, ref opp, Common.User_Id, Common.User_Name, Common.Branch_Id, entryNo, to, journalCode, entryCurrency, notes,
                Common.Transform_Type, dt, (double)value, branchId, 1, ehlakId.ToString(), entryCurrency, currencyName, 1, 0);
            if (excp != 0 || entryId <= 0) throw new InvalidOperationException(string.IsNullOrWhiteSpace(msg) ? "تعذر إنشاء قيد الإهلاك." : msg);
            return entryId;
        }
    }
}
