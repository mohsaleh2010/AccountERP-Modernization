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

        public record Setting(Method Method, DateTime? StartDate, ValuationMethod Valuation = ValuationMethod.AverageCost);

        /// <summary>تقييم الكميات فى قيد البداية: متوسط التكلفة (الافتراضى) أو آخر سعر شراء.</summary>
        public enum ValuationMethod { AverageCost = 0, LastPurchase = 1 }

        /// <summary>Reference_Type لقيد بداية المخزون عند التحول للجرد المستمر (Reference_Type_Table_Id = رقم الفرع).</summary>
        public const int OpeningReferenceType = 3004;

        private const decimal Tolerance = 0.005m;

        // ============================== الإعداد ==============================

        public Setting GetSetting(long branchId)
        {
            return GetSettingWith(new operations(), branchId);
        }

        private static Setting GetSettingWith(operations opp, long branchId)
        {
            var dt = opp.return_DataTable("SELECT Method, Start_Date, Valuation_Method FROM Inventory_Accounting_Setting WHERE Account_Branch_Id = " + branchId.ToString(CultureInfo.InvariantCulture));
            if (dt.Rows.Count == 0) return new Setting(Method.Periodic, null);
            var r = dt.Rows[0];
            return new Setting(Convert.ToInt32(r["Method"]) == 1 ? Method.Continuous : Method.Periodic,
                r["Start_Date"] == DBNull.Value ? null : Convert.ToDateTime(r["Start_Date"]),
                Convert.ToInt32(r["Valuation_Method"]) == 1 ? ValuationMethod.LastPurchase : ValuationMethod.AverageCost);
        }

        /// <summary>يحفظ إعداد الفرع. المستمر يتطلب تاريخ بدء.</summary>
        public void SaveSetting(long branchId, Method method, DateTime? startDate, ValuationMethod valuation = ValuationMethod.AverageCost)
        {
            int v = (int)valuation;
            if (method == Method.Continuous && startDate == null) throw new InvalidOperationException("StartDateRequired");
            var opp = new operations();
            long userId = Common.User_Id;
            DateTime now = DateTime.Now;
            DateTime? start = method == Method.Continuous ? startDate!.Value.Date : null;
            int m = (int)method;
            opp.db.Database.ExecuteSqlInterpolated($@"
IF EXISTS (SELECT 1 FROM Inventory_Accounting_Setting WHERE Account_Branch_Id = {branchId})
    UPDATE Inventory_Accounting_Setting SET Method = {m}, Start_Date = {start}, Valuation_Method = {v}, User_Id = {userId}, Update_ServerDateTime = {now} WHERE Account_Branch_Id = {branchId}
ELSE
    INSERT INTO Inventory_Accounting_Setting (Account_Branch_Id, Method, Start_Date, Valuation_Method, User_Id, Update_ServerDateTime) VALUES ({branchId}, {m}, {start}, {v}, {userId}, {now})");
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

        // ============================== قيد بداية المخزون (شريحة 5) ==============================

        public record OpeningRow(long StoreId, string StoreName, decimal Target, decimal Posted, long AccountId)
        {
            public decimal Delta => Common.RoundMe(Target - Posted);
        }

        /// <summary>
        /// قيمة مخزون كل مخزن بالفرع قبل تاريخ البدء (الكميات من دفتر المحرك، والتكلفة بطريقة التقييم المختارة)
        /// مع ما قُيِّد سابقاً كقيد بداية. قراءة فقط.
        /// </summary>
        public List<OpeningRow> GetOpeningPreview(long branchId, operations? opp = null)
        {
            opp ??= new operations();
            var setting = GetSettingWith(opp, branchId);
            if (setting.Method != Method.Continuous || setting.StartDate == null) throw new InvalidOperationException("OpeningNeedsContinuous");
            string until = "'" + setting.StartDate.Value.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + "'";
            string costExpr = setting.Valuation == ValuationMethod.LastPurchase
                ? "ISNULL(NULLIF(c.Last_Purchase_Price, 0), c.Avg_Cost)"
                : "c.Avg_Cost";
            string br = branchId.ToString(CultureInfo.InvariantCulture);
            const string factor = "(CASE WHEN ISNULL(ui.Small_unit, 0) <= 0 THEN 1 ELSE ui.Small_unit END)";
            string sql = @"
SELECT s.id AS StoreId, s.Store_Name AS StoreName, ISNULL(s.SubAccount_Id, 0) AS Acc,
       SUM(q.Qty * ISNULL(" + costExpr + @", q.AvgLarge / q.Factor)) AS Val
FROM (
    SELECT ui.id AS UnitId, ui.Store_id, ui.Item_id, ISNULL(ui.P_priceAvgCost, 0) AS AvgLarge, " + factor + @" AS Factor,
           SUM(CASE WHEN x.Engine_Posted = 1 THEN ISNULL(x.Qty_Small, 0)
                    WHEN x.Transaction_Type = 1 THEN  (ISNULL(x.qua, 0) * " + factor + @" + ISNULL(x.Part_qua, 0))
                    WHEN x.Transaction_Type = 2 THEN -(ISNULL(x.qua, 0) * " + factor + @" + ISNULL(x.Part_qua, 0))
                    ELSE 0 END) AS Qty
    FROM Unit_item ui
    INNER JOIN Store st ON st.id = ui.Store_id AND st.Branch_Id = " + br + @"
    LEFT JOIN (SELECT d.Unit_item_Table_Id AS UnitId, MAX(d.Process_Date) AS OpeningFrom
               FROM Detailes_Purshing_Import_Export d
               WHERE d.ProcessNameId = " + StockMovementService.OpeningBalanceProcessId + @" AND d.Engine_Posted = 1 AND d.Reversal_Of_Id IS NULL
                 AND d.Process_Date < " + until + @"
                 AND NOT EXISTS (SELECT 1 FROM Detailes_Purshing_Import_Export r WHERE r.Reversal_Of_Id = d.Id)
               GROUP BY d.Unit_item_Table_Id) o ON o.UnitId = ui.id
    LEFT JOIN Detailes_Purshing_Import_Export x ON x.Unit_item_Table_Id = ui.id AND (x.Process_Date IS NULL OR x.Process_Date < " + until + @")
         AND (o.OpeningFrom IS NULL OR x.Process_Date >= o.OpeningFrom)
    GROUP BY ui.id, ui.Store_id, ui.Item_id, ui.P_priceAvgCost, ui.Small_unit
) q
INNER JOIN Store s ON s.id = q.Store_id
LEFT JOIN Item_Branch_Cost c ON c.Item_Id = q.Item_id AND c.Branch_Id = " + br + @"
GROUP BY s.id, s.Store_Name, s.SubAccount_Id
ORDER BY s.id";
            var dt = opp.return_DataTable(sql);
            var posted = opp.return_DataTable("SELECT Store_Id, SUM(Amount) AS Amt FROM Inventory_Opening_Entry WHERE Account_Branch_Id = " + br + " GROUP BY Store_Id");
            var postedBy = posted.Rows.Cast<DataRow>().ToDictionary(r => Convert.ToInt64(r["Store_Id"]), r => Convert.ToDecimal(r["Amt"]));
            var list = new List<OpeningRow>();
            foreach (DataRow r in dt.Rows)
            {
                long id = Convert.ToInt64(r["StoreId"]);
                decimal target = r["Val"] == DBNull.Value ? 0 : Common.RoundMe(Convert.ToDecimal(r["Val"]));
                postedBy.TryGetValue(id, out var p);
                if (target == 0 && p == 0) continue;
                list.Add(new OpeningRow(id, r["StoreName"]?.ToString()?.Trim() ?? "", target, p, Convert.ToInt64(r["Acc"])));
            }
            return list;
        }

        /// <summary>
        /// يقيّد قيد بداية المخزون بتاريخ (تاريخ البدء − يوم): مدين حساب كل مخزن / دائن حساب الرصيد الافتتاحى.
        /// عند إعادة الإنشاء يقيّد الفرق فقط عن المقيَّد سابقاً (الفرق السالب قيد معاكس)، فلا يتكرر ولا يُمسّ قيد قديم.
        /// يرجع رقم القيد أو 0 إن لا فرق. معاملة مستقلة.
        /// </summary>
        public long PostOpeningEntry(long branchId, operations? external = null)
        {
            var opp = external ?? new operations(1);
            bool own = external == null;
            try
            {
                var rows = GetOpeningPreview(branchId, opp).Where(r => r.Delta != 0).ToList();
                if (rows.Count == 0) { if (own) opp.Rollback(); return 0; }
                DateTime date = GetSettingWith(opp, branchId).StartDate!.Value.Date.AddDays(-1);

                var equity = new FirstBalance_CLS().GetOrCreateOpeningBalanceAccount(ref opp);
                if (equity == null || equity.Rows.Count == 0) throw new InvalidOperationException("حساب الرصيد الافتتاحى غير موجود.");
                long equityId = Convert.ToInt64(equity.Rows[0]["Id"]);

                var debit = new Dictionary<long, decimal>();
                var credit = new Dictionary<long, decimal>();
                decimal net = 0;
                foreach (var r in rows)
                {
                    if (r.AccountId <= 0) throw new InvalidOperationException("المخزن «" + r.StoreName + "» بلا حساب مخزون فى شجرة الحسابات؛ اربطه من «ربط المخازن بالحسابات».");
                    var side = r.Delta > 0 ? debit : credit;
                    side[r.AccountId] = (side.TryGetValue(r.AccountId, out var v) ? v : 0) + Math.Abs(r.Delta);
                    net += r.Delta;
                }
                if (net > 0) credit[equityId] = (credit.TryGetValue(equityId, out var c0) ? c0 : 0) + net;
                else if (net < 0) debit[equityId] = (debit.TryGetValue(equityId, out var d0) ? d0 : 0) - net;
                foreach (var k in debit.Keys.Intersect(credit.Keys).ToList())
                {
                    decimal m = Math.Min(debit[k], credit[k]);
                    debit[k] -= m; credit[k] -= m;
                    if (debit[k] <= 0) debit.Remove(k);
                    if (credit[k] <= 0) credit.Remove(k);
                }
                if (debit.Count == 0) { if (own) opp.Rollback(); return 0; }

                long entryCurrency = Common.Entry_Currency_Id <= 0 ? 1 : Common.Entry_Currency_Id;
                var cur = opp.return_DataTable("SELECT Currency_Name FROM Currency WHERE Value = " + entryCurrency.ToString(CultureInfo.InvariantCulture));
                string currencyName = cur.Rows.Count > 0 ? cur.Rows[0][0]?.ToString() ?? "" : "";
                string notes = "قيد بداية المخزون عند التحول للجرد المستمر (" + date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + ")";

                var aa = new AccountEntry_CLS { Reference_Type = OpeningReferenceType, Reference_Type_Table_Id = branchId };
                var dt = new DataTable();
                aa.Get_Empty_dt_Details(ref opp, ref dt);
                int excp = 0;
                decimal total = 0;
                foreach (var (accountId, amount) in debit) { AddLine(aa, opp, ref excp, dt, accountId, 1, amount, notes, entryCurrency, branchId); total += amount; }
                foreach (var (accountId, amount) in credit) AddLine(aa, opp, ref excp, dt, accountId, 2, amount, notes, entryCurrency, branchId);
                if (excp != 0) throw new InvalidOperationException("تعذر تجهيز أسطر قيد البداية.");
                for (int i = 0; i < dt.Rows.Count; i++) dt.Rows[i]["Line_No"] = i + 1;

                const long journalCode = 1;
                long entryNo = AccountEntry_CLS.NewEntry_No(ref opp, date, journalCode, 0);
                long entryId = 0;
                string msg = "";
                aa.Make_AccountEntry(ref excp, ref msg, ref entryId, ref opp, Common.User_Id, Common.User_Name, Common.Branch_Id, entryNo, date, journalCode, entryCurrency, notes,
                    Common.Transform_Type, dt, (double)total, branchId, 1, "", entryCurrency, currencyName, 1, 0);
                if (excp != 0 || entryId <= 0) throw new InvalidOperationException(string.IsNullOrWhiteSpace(msg) ? "تعذر إنشاء قيد البداية." : msg);

                long userId = Common.User_Id;
                foreach (var r in rows)
                {
                    decimal delta = r.Delta;
                    opp.db.Database.ExecuteSqlInterpolated($"INSERT INTO Inventory_Opening_Entry (Account_Branch_Id, Store_Id, Amount, As_Of_Date, Entry_Id, User_Id) VALUES ({branchId}, {r.StoreId}, {delta}, {date}, {entryId}, {userId})");
                }
                if (own) opp.Commit();
                return entryId;
            }
            catch
            {
                if (own) opp.Rollback();
                throw;
            }
        }

        // ============================== تكلفة المبيعات للفواتير (شريحة 4) ==============================

        /// <summary>Reference_Type لقيد تكلفة فاتورة مبيعات (Reference_Type_Table_Id = رقم الفاتورة).</summary>
        public const int InvoiceCogsReferenceType = 3005;
        /// <summary>Reference_Type لقيد عكس تكلفة مرتجع مبيعات.</summary>
        public const int InvoiceReturnCogsReferenceType = 3006;

        /// <summary>
        /// قيد تكلفة البضاعة المباعة لفاتورة بيع (أو مرتجع): قيد منفصل مرتبط بالفاتورة يُعاد بناؤه عند كل حفظ.
        /// يحذف قيد التكلفة السابق للفاتورة دائماً (لا أثر لمن ليسوا «مستمر»)، ثم لو الفرع «مستمر» وتاريخ الفاتورة ≥ تاريخ البدء
        /// وللفاتورة أسطر مخزنية غير محذوفة: البيع مدين حساب التكلفة / دائن حساب المخزن، والمرتجع العكس.
        /// الكمية بالوحدة الصغرى (الكبرى × المعامل + الصغرى + المجانى)، والتكلفة بطريقة التقييم المختارة فى الإعداد (متوسط أو آخر شراء).
        /// تُهمَل الخدمات والأصناف «بلا رصيد». ترمى استثناء عربياً لو نقص ربط حساب (فيرفض حفظ الفاتورة). يرجع رقم القيد أو 0.
        /// </summary>
        public long PostInvoiceCogsEntry(operations opp, long invoiceId, bool isReturn)
        {
            int refType = isReturn ? InvoiceReturnCogsReferenceType : InvoiceCogsReferenceType;
            string id = invoiceId.ToString(CultureInfo.InvariantCulture);
            // قاعدة لم تُرقَّ بعد (بلا الجدول) = دورى: لا قيد ولا حذف
            var has = opp.return_DataTable("SELECT CASE WHEN OBJECT_ID(N'dbo.Inventory_Accounting_Setting', N'U') IS NULL THEN 0 ELSE 1 END");
            if (Convert.ToInt32(has.Rows[0][0]) == 0) return 0;

            int excp = 0; string msg = "";
            new AccountEntry_CLS().Delete_EntryFromAutomatic(ref opp, ref excp, ref msg, invoiceId, refType, false);

            string listTbl = isReturn ? "Invoices_List_Reverse" : "Invoices_List";
            string detTbl = isReturn ? "Invoices_Details_Reverse" : "Invoices_Details";
            var head = opp.return_DataTable("SELECT Invoice_Id, Invoice_Date, ISNULL(Branch_Id, 0) AS Br, ISNULL(Is_DeletedProcessed, 0) AS Del FROM " + listTbl + " WHERE Id = " + id);
            if (head.Rows.Count == 0) return 0;
            var h = head.Rows[0];
            if (Convert.ToInt32(h["Del"]) == 1 || h["Invoice_Date"] == DBNull.Value) return 0;
            long branch = Convert.ToInt64(h["Br"]);
            if (branch <= 0) branch = Common.Branch_Id;
            DateTime date = Convert.ToDateTime(h["Invoice_Date"]).Date;
            string invoiceNo = h["Invoice_Id"]?.ToString() ?? "";
            if (!IsContinuous(opp, branch, date)) return 0;

            var setting = GetSettingWith(opp, branch);
            string cost = setting.Valuation == ValuationMethod.LastPurchase
                ? "ISNULL(NULLIF(c.Last_Purchase_Price, 0), c.Avg_Cost)"
                : "c.Avg_Cost";
            const string factor = "(CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END)";
            var vals = opp.return_DataTable(@"
SELECT u.Store_id AS StoreId,
       SUM(((ISNULL(d.Larg_Qnty, 0) + ISNULL(d.LargItemsQnty_Free, 0)) * " + factor + @" + ISNULL(d.Small_Qnty, 0) + ISNULL(d.SmallItemsQnty_Free, 0))
           * ISNULL(" + cost + @", ISNULL(u.P_priceAvgCost, 0) / " + factor + @")) AS Val
FROM " + detTbl + @" d
INNER JOIN Unit_item u ON u.id = d.Unit_Item_Table_Id
INNER JOIN Store st ON st.id = u.Store_id
LEFT JOIN Item it ON it.Id = u.Item_id
LEFT JOIN Item_Branch_Cost c ON c.Item_Id = u.Item_id AND c.Branch_Id = ISNULL(NULLIF(st.Branch_Id, 0), " + branch.ToString(CultureInfo.InvariantCulture) + @")
WHERE d.Invoice_Id = " + id + @" AND ISNULL(d.Item_DeletedFlage, 0) <> 1
  AND ISNULL(it.Is_ItemOrService, 0) <> 2 AND ISNULL(u.Is_ItemWithoutBalance, 0) <> 1
GROUP BY u.Store_id");

            var determination = new StoreAccountDeterminationCLS();
            var debit = new Dictionary<long, decimal>();
            var credit = new Dictionary<long, decimal>();
            long entryBranch = 0;
            foreach (DataRow r in vals.Rows)
            {
                decimal value = r["Val"] == DBNull.Value ? 0 : Common.RoundMe(Convert.ToDecimal(r["Val"]));
                if (Math.Abs(value) <= Tolerance) continue;
                long storeId = Convert.ToInt64(r["StoreId"]);
                var store = opp.return_DataTable("SELECT Store_Name AS name, ISNULL(StoreUsed_Type, 0) AS T, ISNULL(Branch_Id, 0) AS Br, ISNULL(SubAccount_Id, 0) AS Acc FROM Store WHERE id = " + storeId.ToString(CultureInfo.InvariantCulture));
                var s = store.Rows[0];
                string storeName = s["name"]?.ToString()?.Trim() ?? "";
                long inventoryAccount = Convert.ToInt64(s["Acc"]);
                long storeBranch = Convert.ToInt64(s["Br"]);
                if (inventoryAccount <= 0) throw new InvalidOperationException("المخزن «" + storeName + "» بلا حساب مخزون فى شجرة الحسابات؛ اربطه من «ربط المخازن بالحسابات».");
                long cogs = determination.ResolveAccount(storeBranch > 0 ? storeBranch : branch, storeId, Convert.ToInt32(s["T"]), StoreAccountRole.COGS, opp);
                if (cogs <= 0) throw new InvalidOperationException("لم يُحدَّد حساب «تكلفة البضاعة المباعة» للمخزن «" + storeName + "»؛ حدده من «ربط المخازن بالحسابات».");
                if (entryBranch == 0) entryBranch = storeBranch > 0 ? storeBranch : branch;
                var (dr, cr) = isReturn ? (inventoryAccount, cogs) : (cogs, inventoryAccount);
                debit[dr] = (debit.TryGetValue(dr, out var d0) ? d0 : 0) + value;
                credit[cr] = (credit.TryGetValue(cr, out var c0) ? c0 : 0) + value;
            }
            if (debit.Count == 0) return 0;

            long entryCurrency = Common.Entry_Currency_Id <= 0 ? 1 : Common.Entry_Currency_Id;
            var cur = opp.return_DataTable("SELECT Currency_Name FROM Currency WHERE Value = " + entryCurrency.ToString(CultureInfo.InvariantCulture));
            string currencyName = cur.Rows.Count > 0 ? cur.Rows[0][0]?.ToString() ?? "" : "";
            string notes = (isReturn ? "عكس تكلفة مرتجع مبيعات رقم " : "تكلفة بضاعة مباعة لفاتورة رقم ") + invoiceNo;

            var aa = new AccountEntry_CLS { Reference_Type = refType, Reference_Type_Table_Id = invoiceId };
            var dt = new DataTable();
            aa.Get_Empty_dt_Details(ref opp, ref dt);
            decimal total = 0;
            foreach (var (accountId, amount) in debit) { AddLine(aa, opp, ref excp, dt, accountId, 1, amount, notes, entryCurrency, entryBranch); total += amount; }
            foreach (var (accountId, amount) in credit) AddLine(aa, opp, ref excp, dt, accountId, 2, amount, notes, entryCurrency, entryBranch);
            if (excp != 0) throw new InvalidOperationException("تعذر تجهيز أسطر قيد التكلفة.");
            for (int i = 0; i < dt.Rows.Count; i++) dt.Rows[i]["Line_No"] = i + 1;

            const long journalCode = 1;
            long entryNo = AccountEntry_CLS.NewEntry_No(ref opp, date, journalCode, 0);
            long entryId = 0;
            aa.Make_AccountEntry(ref excp, ref msg, ref entryId, ref opp, Common.User_Id, Common.User_Name, Common.Branch_Id, entryNo, date, journalCode, entryCurrency, notes,
                Common.Transform_Type, dt, (double)total, entryBranch, 1, invoiceNo, entryCurrency, currencyName, 1, 0);
            if (excp != 0 || entryId <= 0) throw new InvalidOperationException(string.IsNullOrWhiteSpace(msg) ? "تعذر إنشاء قيد التكلفة." : msg);
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
