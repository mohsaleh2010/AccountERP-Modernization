using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using AccountModelsLib.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib
{
    /// <summary>
    /// قيود الجرد المستمر لأذون المخزن (خطة المخازن 01 §2.8، شريحتا 1 و2 من ب1). الإعداد لكل فرع فى Inventory_Accounting_Setting:
    /// لا صف أو Method = 0 «جرد دورى» (الافتراضى، لا قيد)، و1 «مستمر» من Start_Date فصاعداً.
    /// القيد يُبنى من صفوف دفتر المحرك (DPIE) للإذن بلقطة تكلفتها: لكل مخزن صافى القيمة؛ الوارد مدين حساب المخزن ودائن حساب الدور
    /// (StoreAccountDeterminationCLS)، والمنصرف عكسه. الإذن العكسى له صفوف دفتر معاكسة فيخرج قيده معاكساً تلقائياً.
    /// يعمل داخل معاملة المستدعى (opp)، ويرفض الترحيل (استثناء) بدل كتابة قيد ناقص لو نقص ربط حساب.
    /// الفواتير خارج هذه الشريحة (كود مشترك مع الديسكتوب — شريحة 4).
    /// </summary>
    public class InventoryPostingService
    {
        /// <summary>Reference_Type لقيد إذن مخزن (3001 عجز مندوب، 3002 عمولة مندوب).</summary>
        public const int ReferenceType = 3003;

        public enum Method { Periodic = 0, Continuous = 1 }

        public record Setting(Method Method, DateTime? StartDate);

        private const decimal Tolerance = 0.005m;

        // ============================== الإعداد ==============================

        public Setting GetSetting(long branchId)
        {
            var dt = new operations().return_DataTable("SELECT Method, Start_Date FROM Inventory_Accounting_Setting WHERE Account_Branch_Id = " + branchId.ToString(CultureInfo.InvariantCulture));
            if (dt.Rows.Count == 0) return new Setting(Method.Periodic, null);
            var r = dt.Rows[0];
            return new Setting(Convert.ToInt32(r["Method"]) == 1 ? Method.Continuous : Method.Periodic, r["Start_Date"] == DBNull.Value ? null : Convert.ToDateTime(r["Start_Date"]));
        }

        /// <summary>يحفظ إعداد الفرع. المستمر يتطلب تاريخ بدء.</summary>
        public void SaveSetting(long branchId, Method method, DateTime? startDate)
        {
            if (method == Method.Continuous && startDate == null) throw new InvalidOperationException("StartDateRequired");
            var opp = new operations();
            long userId = Common.User_Id;
            DateTime now = DateTime.Now;
            DateTime? start = method == Method.Continuous ? startDate!.Value.Date : null;
            int m = (int)method;
            opp.db.Database.ExecuteSqlInterpolated($@"
IF EXISTS (SELECT 1 FROM Inventory_Accounting_Setting WHERE Account_Branch_Id = {branchId})
    UPDATE Inventory_Accounting_Setting SET Method = {m}, Start_Date = {start}, User_Id = {userId}, Update_ServerDateTime = {now} WHERE Account_Branch_Id = {branchId}
ELSE
    INSERT INTO Inventory_Accounting_Setting (Account_Branch_Id, Method, Start_Date, User_Id, Update_ServerDateTime) VALUES ({branchId}, {m}, {start}, {userId}, {now})");
        }

        /// <summary>هل يُقيَّد إذن بهذا التاريخ فى هذا الفرع؟</summary>
        public static bool IsContinuous(operations opp, long branchId, DateTime date)
        {
            // قاعدة لم تُرقَّ بعد (بلا الجدول) = دورى، فلا يتعطل ترحيل الأذون
            var dt = opp.return_DataTable("IF OBJECT_ID(N'dbo.Inventory_Accounting_Setting', N'U') IS NULL SELECT TOP 0 CAST(0 AS int) AS Method, CAST(NULL AS date) AS Start_Date ELSE SELECT Method, Start_Date FROM Inventory_Accounting_Setting WHERE Account_Branch_Id = " + branchId.ToString(CultureInfo.InvariantCulture));
            if (dt.Rows.Count == 0) return false;
            var r = dt.Rows[0];
            return Convert.ToInt32(r["Method"]) == 1 && r["Start_Date"] != DBNull.Value && date.Date >= Convert.ToDateTime(r["Start_Date"]).Date;
        }

        // ============================== الدور حسب نوع الإذن ==============================

        /// <summary>دور الحساب المقابل لنوع الإذن (من StoreAccountDeterminationCLS.Roles)، أو null لنوع بلا قيد (كأول المدة 900/901).</summary>
        public static StoreAccountRole? RoleFor(int orderType)
        {
            var key = orderType.ToString(CultureInfo.InvariantCulture);
            var hit = StoreAccountDeterminationCLS.Roles.FirstOrDefault(r => (r.OrderTypes ?? "").Split(',').Select(x => x.Trim()).Contains(key));
            return hit?.Role;
        }

        // ============================== القيد ==============================

        /// <summary>
        /// يكتب قيد الإذن إن كان الفرع «مستمر» وتاريخ الإذن ≥ تاريخ البدء. يرجع رقم القيد أو 0 (لا قيد: دورى، نوع بلا دور، قيمة صفر، أو سبق قيده).
        /// يرمى InvalidOperationException برسالة عربية لو نقص حساب المخزن أو حساب الدور.
        /// </summary>
        public long PostOrderEntry(operations opp, long orderId)
        {
            var head = opp.return_DataTable("SELECT Order_Type, Order_date, Order_Purshing_No, ISNULL(Branch_ID, 0) AS Br FROM Order_Purshing WHERE Id = " + orderId.ToString(CultureInfo.InvariantCulture));
            if (head.Rows.Count == 0) return 0;
            var h = head.Rows[0];
            int orderType = h["Order_Type"] == DBNull.Value ? 0 : Convert.ToInt32(h["Order_Type"]);
            DateTime date = h["Order_date"] == DBNull.Value ? DateTime.Today : Convert.ToDateTime(h["Order_date"]).Date;
            long orderNo = h["Order_Purshing_No"] == DBNull.Value ? 0 : Convert.ToInt64(h["Order_Purshing_No"]);
            long orderBranch = Convert.ToInt64(h["Br"]);
            if (orderBranch <= 0) orderBranch = Common.Branch_Id;

            var role = RoleFor(orderType);
            if (role == null || !IsContinuous(opp, orderBranch, date)) return 0;

            var exists = opp.return_DataTable("SELECT COUNT(*) FROM Account_Entry_List WHERE Reference_Type = " + ReferenceType + " AND Reference_Type_Table_Id = " + orderId.ToString(CultureInfo.InvariantCulture));
            if (Convert.ToInt32(exists.Rows[0][0]) > 0) return 0;

            // صافى القيمة لكل مخزن من لقطة تكلفة صفوف الدفتر (موجب = وارد، سالب = منصرف)
            var vals = opp.return_DataTable(@"
SELECT ui.Store_id AS StoreId, SUM(ISNULL(d.Qty_Small, 0) * ISNULL(d.Unit_Cost, 0)) AS Val
FROM Detailes_Purshing_Import_Export d
INNER JOIN Unit_item ui ON ui.id = d.Unit_item_Table_Id
WHERE d.OrderTable_id = " + orderId.ToString(CultureInfo.InvariantCulture) + @" AND d.Engine_Posted = 1 AND ISNULL(d.Qty_Small, 0) <> 0
GROUP BY ui.Store_id");

            var determination = new StoreAccountDeterminationCLS();
            var debit = new Dictionary<long, decimal>();
            var credit = new Dictionary<long, decimal>();
            long entryBranch = 0;
            foreach (DataRow r in vals.Rows)
            {
                decimal value = Common.RoundMe(Convert.ToDecimal(r["Val"]));
                if (Math.Abs(value) <= Tolerance) continue;
                long storeId = Convert.ToInt64(r["StoreId"]);
                var store = opp.return_DataTable("SELECT Store_Name AS name, ISNULL(StoreUsed_Type, 0) AS T, ISNULL(Branch_Id, 0) AS Br, ISNULL(SubAccount_Id, 0) AS Acc FROM Store WHERE id = " + storeId.ToString(CultureInfo.InvariantCulture));
                if (store.Rows.Count == 0) throw new InvalidOperationException("المخزن غير موجود (رقم " + storeId + ").");
                var s = store.Rows[0];
                string storeName = s["name"]?.ToString()?.Trim() ?? "";
                long inventoryAccount = Convert.ToInt64(s["Acc"]);
                long storeBranch = Convert.ToInt64(s["Br"]);
                if (inventoryAccount <= 0) throw new InvalidOperationException("المخزن «" + storeName + "» بلا حساب مخزون فى شجرة الحسابات؛ اربطه من «ربط المخازن بالحسابات».");
                long counter = determination.ResolveAccount(storeBranch > 0 ? storeBranch : orderBranch, storeId, Convert.ToInt32(s["T"]), role.Value, opp);
                if (counter <= 0)
                {
                    var name = StoreAccountDeterminationCLS.Roles.First(x => x.Role == role.Value).Name;
                    throw new InvalidOperationException("لم يُحدَّد حساب «" + name + "» للمخزن «" + storeName + "»؛ حدده من «ربط المخازن بالحسابات».");
                }
                if (entryBranch == 0) entryBranch = storeBranch > 0 ? storeBranch : orderBranch;
                decimal abs = Math.Abs(value);
                var (dr, cr) = value > 0 ? (inventoryAccount, counter) : (counter, inventoryAccount);
                debit[dr] = (debit.TryGetValue(dr, out var d0) ? d0 : 0) + abs;
                credit[cr] = (credit.TryGetValue(cr, out var c0) ? c0 : 0) + abs;
            }
            if (debit.Count == 0) return 0;

            long entryCurrency = Common.Entry_Currency_Id <= 0 ? 1 : Common.Entry_Currency_Id;
            var cur = opp.return_DataTable("SELECT Currency_Name FROM Currency WHERE Value = " + entryCurrency.ToString(CultureInfo.InvariantCulture));
            string currencyName = cur.Rows.Count > 0 ? cur.Rows[0][0]?.ToString() ?? "" : "";
            string notes = "قيد مخزنى لإذن رقم " + orderNo.ToString(CultureInfo.InvariantCulture) + " (نوع " + orderType.ToString(CultureInfo.InvariantCulture) + ")";

            var aa = new AccountEntry_CLS { Reference_Type = ReferenceType, Reference_Type_Table_Id = orderId };
            var dt = new DataTable();
            aa.Get_Empty_dt_Details(ref opp, ref dt);
            int excp = 0;
            decimal total = 0;
            foreach (var (accountId, amount) in debit) { AddLine(aa, opp, ref excp, dt, accountId, 1, amount, notes, entryCurrency, entryBranch); total += amount; }
            foreach (var (accountId, amount) in credit) AddLine(aa, opp, ref excp, dt, accountId, 2, amount, notes, entryCurrency, entryBranch);
            if (excp != 0) throw new InvalidOperationException("تعذر تجهيز أسطر القيد المخزنى.");
            for (int i = 0; i < dt.Rows.Count; i++) dt.Rows[i]["Line_No"] = i + 1;

            const long journalCode = 1;
            long entryNo = AccountEntry_CLS.NewEntry_No(ref opp, date, journalCode, 0);
            long entryId = 0;
            string msg = "";
            aa.Make_AccountEntry(ref excp, ref msg, ref entryId, ref opp, Common.User_Id, Common.User_Name, Common.Branch_Id, entryNo, date, journalCode, entryCurrency, notes,
                Common.Transform_Type, dt, (double)total, entryBranch, 1, orderNo.ToString(CultureInfo.InvariantCulture), entryCurrency, currencyName, 1, 0);
            if (excp != 0 || entryId <= 0) throw new InvalidOperationException(string.IsNullOrWhiteSpace(msg) ? "تعذر إنشاء القيد المخزنى." : msg);
            return entryId;
        }

        private static void AddLine(AccountEntry_CLS aa, operations opp, ref int excp, DataTable dt, long subAccountId, int madinDa2n, decimal amount, string notes, long entryCurrency, long branchId)
        {
            if (excp != 0) return;
            var a = opp.return_DataTable("SELECT TOP 1 Id, ISNULL(Account_Customer_Type_Value, 0) AS T, SubAccount_Name, SubAccount_NO, ISNULL(Currency_Id, 1) AS Cur, ISNULL(Main_Account_Id, 0) AS M FROM Submain_Account WHERE Id = " + subAccountId.ToString(CultureInfo.InvariantCulture));
            if (a.Rows.Count == 0) throw new InvalidOperationException("حساب غير موجود فى الدليل (رقم " + subAccountId + ").");
            var r = a.Rows[0];
            long cur = Convert.ToInt64(r["Cur"]);
            if (cur <= 0) cur = 1;
            aa.AddRow_WhenBuild(ref excp, ref dt, 0, r["SubAccount_NO"]?.ToString()?.Trim() ?? "", Convert.ToInt32(r["T"]), Convert.ToInt64(r["M"]), "", subAccountId,
                r["SubAccount_Name"]?.ToString()?.Trim() ?? "", madinDa2n, amount, notes, 1, cur, entryCurrency, 0, "", branchId, 1);
        }
    }
}
