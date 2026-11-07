using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib.BusinessLibShared.CostCenters
{
    /// <summary>
    /// مركز تكلفة رأس الفاتورة (خطة مراكز التكلفة م3 دفعة 3، ق9): عمود Cost_Center_Id على رؤوس فواتير/مرتجعات المبيعات والمشتريات،
    /// ويُرحَّل على أسطر القيد الآلى للمستند التى يقبل حسابها مراكز (سياسة ≠ 0 والمركز ضمن المسموح له) — أى حسابات الإيراد/المصروف/التكلفة
    /// لا الذمم والنقدية ما لم تكن سياستها ≠ 0.
    /// بلا لمس لمحرك الحفظ ولا للـEF: الصفحة تضع القيمة بـ<see cref="SetPending"/> قبل الحفظ (فى نفس الخيط)، وبعد بناء القيد
    /// تستدعى الدوال المغلِّفة <see cref="Apply"/> فتكتب العمود وتحمّل أسطر القيد المبنية حديثاً داخل معاملة الحفظ نفسها.
    /// بلا قيمة معلّقة (ديسكتوب أو تعديل بلا اختيار) يبقى المخزَّن كما هو ويُعاد ترحيله على القيد المعاد بناؤه.
    /// </summary>
    public static class InvoiceCostCenter
    {
        public const string Table_Sales = "Invoices_List";
        public const string Table_SalesReturn = "Invoices_List_Reverse";
        public const string Table_Purchase = "Bills_List";
        public const string Table_PurchaseReturn = "Bills_List_Reverse";
        public const string Table_StoreOrder = "Order_Purshing";

        [ThreadStatic] private static long? _pending;

        /// <summary>يُستدعى داخل نفس الخيط الذى سيحفظ الفاتورة؛ 0 = بلا مركز (يمسح المخزَّن).</summary>
        public static void SetPending(long centerId) => _pending = centerId < 0 ? 0 : centerId;
        public static void ClearPending() => _pending = null;

        private static bool IsHeaderTable(string t) =>
            t == Table_Sales || t == Table_SalesReturn || t == Table_Purchase || t == Table_PurchaseReturn || t == Table_StoreOrder;

        private static bool HasColumn(operations opp, string table) =>
            opp.db.Database.SqlQueryRaw<int>("SELECT CASE WHEN COL_LENGTH(N'dbo." + table + "', N'Cost_Center_Id') IS NULL THEN 0 ELSE 1 END AS Value").AsEnumerable().First() == 1;

        /// <summary>مركز رأس المستند المخزَّن (0 = بلا).</summary>
        public static long GetHeaderCenter(operations opp, string table, long docId)
        {
            if (!IsHeaderTable(table) || docId <= 0 || !HasColumn(opp, table)) return 0;
            var dt = opp.return_DataTable("SELECT Cost_Center_Id FROM " + table + " WHERE Id = " + docId);
            return dt.Rows.Count > 0 && dt.Rows[0][0] != DBNull.Value ? Convert.ToInt64(dt.Rows[0][0]) : 0;
        }

        /// <summary>مركز الأصل الثابت (FixedAssetData.Cost_Center_Id) — 0 = بلا مركز أو العمود غير موجود.</summary>
        public static long GetAssetCenter(operations opp, long fixedAssetDataId)
        {
            if (fixedAssetDataId <= 0 || !HasColumn(opp, "FixedAssetData")) return 0;
            var dt = opp.return_DataTable("SELECT Cost_Center_Id FROM FixedAssetData WHERE Id = " + fixedAssetDataId);
            return dt.Rows.Count > 0 && dt.Rows[0][0] != DBNull.Value ? Convert.ToInt64(dt.Rows[0][0]) : 0;
        }

        /// <summary>يكتب مركز الأصل (0 = بلا مركز). يُستدعى داخل معاملة حفظ الأصل.</summary>
        public static void SetAssetCenter(operations opp, long fixedAssetDataId, long centerId)
        {
            if (fixedAssetDataId <= 0 || !HasColumn(opp, "FixedAssetData")) return;
            int e = 0;
            opp.update_me("UPDATE FixedAssetData SET Cost_Center_Id = " + (centerId > 0 ? centerId.ToString() : "NULL") + " WHERE Id = " + fixedAssetDataId, ref e);
        }

        /// <summary>يكتب مركز رأس المستند مباشرة (أذون المخازن: يُحفظ مع المسودة ويُرحَّل عند الترحيل). 0 = بلا مركز.</summary>
        public static void SetHeaderCenter(operations opp, string table, long docId, long centerId)
        {
            if (!IsHeaderTable(table) || docId <= 0 || !HasColumn(opp, table)) return;
            int e = 0;
            opp.update_me("UPDATE " + table + " SET Cost_Center_Id = " + (centerId > 0 ? centerId.ToString() : "NULL") + " WHERE Id = " + docId, ref e);
        }

        /// <summary>
        /// يحمّل مركزاً على أسطر قيد بعينه (عمولة المندوب، م3 دفعة 5) للأسطر الجديدة التى يقبل حسابها مراكز (سياسة ≠ 0 والمركز ضمن المسموح له).
        /// لا يكتب شيئاً على الرأس؛ فشله لا يُسقط حفظ المستند.
        /// </summary>
        public static void ApplyToEntry(operations opp, long entryId, long centerId)
        {
            if (entryId <= 0 || centerId <= 0) return;
            try
            {
                var node = CostCenterService.Get(opp, centerId);
                if (node == null || !node.IsLeaf || !node.IsOpen) return;
                var accounts = opp.return_DataTable("SELECT DISTINCT Submain_Account_ID FROM Account_Entry_Details WHERE Account_Entry_List_ID = " + entryId + " AND ISNULL(CostCenter_Id, 0) = 0");
                var ok = new List<long>();
                foreach (DataRow r in accounts.Rows)
                {
                    if (r[0] == DBNull.Value) continue;
                    long acc = Convert.ToInt64(r[0]);
                    var allowed = CostCenterService.AllowedFor(opp, acc);
                    if (allowed.Policy != 0 && allowed.Centers.Any(c => c.Id == centerId)) ok.Add(acc);
                }
                if (ok.Count == 0) return;
                int e = 0;
                opp.update_me("UPDATE Account_Entry_Details SET CostCenter_Id = " + centerId + ", CostCenter_Name = (SELECT TOP 1 Name FROM Cost_Center_Tree WHERE Id = " + centerId + ") " +
                              "WHERE Account_Entry_List_ID = " + entryId + " AND ISNULL(CostCenter_Id, 0) = 0 AND Submain_Account_ID IN (" + string.Join(",", ok) + ")", ref e);
            }
            catch { /* تحسين فوق القيد */ }
        }

        /// <summary>
        /// يُستدعى بعد بناء القيد الآلى للمستند: يكتب المركز المعلّق على الرأس (إن وُجد) ثم يحمّله على أسطر القيود
        /// (Reference_Type ضمن refTypes ورقم المستند) التى يسمح حسابها به. لا يفعل شيئاً إن فشل الحفظ أو لم يكن للمستند مركز.
        /// </summary>
        public static void Apply(operations opp, int excp, string table, int[] refTypes, long docId)
        {
            var pending = _pending;
            _pending = null;
            if (excp != 0 || docId <= 0 || !IsHeaderTable(table)) return;
            try
            {
                if (!HasColumn(opp, table)) return;
                int e = 0;
                if (pending.HasValue)
                {
                    opp.update_me("UPDATE " + table + " SET Cost_Center_Id = " + (pending.Value > 0 ? pending.Value.ToString() : "NULL") + " WHERE Id = " + docId, ref e);
                    if (e != 0) return;
                }
                long center = pending ?? GetHeaderCenter(opp, table, docId);
                if (center <= 0) return;

                var node = CostCenterService.Get(opp, center);
                if (node == null || !node.IsLeaf || !node.IsOpen) return;

                var accounts = opp.return_DataTable(
                    "SELECT DISTINCT d.Submain_Account_ID FROM Account_Entry_Details d INNER JOIN Account_Entry_List l ON l.ID = d.Account_Entry_List_ID " +
                    "WHERE l.Reference_Type IN (" + string.Join(",", refTypes) + ") AND l.Reference_Type_Table_Id = " + docId +
                    " AND ISNULL(d.CostCenter_Id, 0) = 0");
                var okAccounts = new List<long>();
                foreach (DataRow r in accounts.Rows)
                {
                    if (r[0] == DBNull.Value) continue;
                    long acc = Convert.ToInt64(r[0]);
                    var allowed = CostCenterService.AllowedFor(opp, acc);
                    if (allowed.Policy != 0 && allowed.Centers.Any(c => c.Id == center)) okAccounts.Add(acc);
                }
                if (okAccounts.Count == 0) return;
                opp.update_me("UPDATE d SET d.CostCenter_Id = " + center + ", d.CostCenter_Name = (SELECT TOP 1 Name FROM Cost_Center_Tree WHERE Id = " + center + ") " +
                              "FROM Account_Entry_Details d INNER JOIN Account_Entry_List l ON l.ID = d.Account_Entry_List_ID " +
                              "WHERE l.Reference_Type IN (" + string.Join(",", refTypes) + ") AND l.Reference_Type_Table_Id = " + docId +
                              " AND ISNULL(d.CostCenter_Id, 0) = 0 AND d.Submain_Account_ID IN (" + string.Join(",", okAccounts) + ")", ref e);
            }
            catch { /* تحميل المركز تحسين فوق القيد؛ فشله لا يُسقط حفظ الفاتورة (والحارس يحذّر من الأسطر بلا مركز) */ }
        }
    }
}
