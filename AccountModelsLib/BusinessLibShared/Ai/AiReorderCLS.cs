using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib
{
    /// <summary>
    /// P8.6.2 (ai-assistant-p86-plan.md §1، §3): قراءة المدخلات (مبيعات أسبوعية، أرصدة، آخر مورد وسعر) بـ SELECT فقط على جداول المخزون والمبيعات
    /// بنفس معادلات GetNeeds (Unit_item، Item_Branch_Cost)، والكتابة على AI_Reorder* فقط.
    /// </summary>
    public class AiReorderCLS
    {
        private readonly Func<operations> _newOpp;

        public AiReorderCLS(Func<operations> newOpp = null)
        {
            _newOpp = newOpp ?? (() => new operations());
        }

        // ───────────── المدخلات ─────────────

        /// <summary>صفوف Unit_item النشطة ذات الرصيد (ط8 يُطبق لاحقاً على المبيعات). مفتاح القاموس = (صنف، مخزن).</summary>
        public Dictionary<(long Item, long Store), AiReorderInput> LoadUnits(int maxItems)
        {
            var map = new Dictionary<(long, long), AiReorderInput>();
            Exec($@"SELECT TOP ({Math.Max(1, maxItems).ToString(CultureInfo.InvariantCulture)}) u.id, u.Item_id, u.Store_id, ISNULL(s.Branch_Id, 0), s.Store_Name, i.Item_Name,
       ISNULL(ISNULL(u.Code, i.code), N''), ISNULL(un.B_Unit, N''), k.F,
       CAST(ISNULL(u.Full_qua, 0) * k.F + ISNULL(u.D_Qua, 0) AS decimal(18,4)), CAST(ISNULL(u.Upon_Request, 0) AS decimal(18,4)),
       CAST(ISNULL(bc.Last_Purchase_Price * k.F, NULLIF(u.P_price, 0)) AS decimal(18,6))
FROM Unit_item u
INNER JOIN Store s ON s.Id = u.Store_id
INNER JOIN Item i ON i.id = u.Item_id
LEFT JOIN unit un ON un.id = u.unit_id
CROSS APPLY (SELECT CAST(CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END AS decimal(18,6)) AS F) k
OUTER APPLY (SELECT TOP 1 b.Last_Purchase_Price FROM Item_Branch_Cost b WHERE b.Item_Id = u.Item_id AND b.Branch_Id = s.Branch_Id) bc
WHERE ISNULL(i.Active_flage, 1) = 1 AND ISNULL(u.active_flage, 1) = 1 AND ISNULL(i.Is_ItemWithoutBalance, 0) = 0
ORDER BY u.id", c => { }, r =>
            {
                var i = new AiReorderInput
                {
                    UnitItemId = L(r, 0), ItemId = L(r, 1), StoreId = L(r, 2), BranchId = L(r, 3), StoreName = S(r, 4), ItemName = S(r, 5),
                    ItemCode = S(r, 6), LargeUnit = S(r, 7), Factor = M(r, 8), OnHandSmall = M(r, 9), ManualReorderLarge = M(r, 10), LastPriceLarge = M(r, 11),
                };
                var key = (i.ItemId, i.StoreId);
                if (!map.ContainsKey(key)) map[key] = i;
            }, 300);
            return map;
        }

        /// <summary>صافى المبيعات الأسبوعى بالصغرى (مبيعات − مرتجعات) من start حتى end (بداية الأسبوع الحالى). wk = 0 للأسبوع الأقدم.</summary>
        public List<(long Item, long Store, int Week, double Qty)> LoadWeeklySales(DateTime start, DateTime end)
        {
            var list = new List<(long, long, int, double)>();
            string Part(string details, string head, string sign) => $@"
SELECT d.Item_Id AS item_id, d.Store_Id AS store_id, DATEDIFF(day, @start, l.Invoice_Date) / 7 AS wk,
       {sign}(ISNULL(d.Larg_Qnty, 0) * ISNULL(k.F, 1) + ISNULL(d.Small_Qnty, 0)) AS q
FROM {details} d
INNER JOIN {head} l ON l.ID = d.Invoice_Id
OUTER APPLY (SELECT TOP 1 CAST(CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END AS decimal(18,6)) AS F
             FROM Unit_item u WHERE u.Item_id = d.Item_Id AND u.Store_id = d.Store_Id) k
WHERE ISNULL(l.Is_DeletedProcessed, 0) <> 1 AND l.Invoice_Date >= @start AND l.Invoice_Date < @end AND d.Item_Id IS NOT NULL AND d.Store_Id IS NOT NULL";
            string sql = "SELECT item_id, store_id, wk, SUM(q) FROM (" + Part("Invoices_Details", "Invoices_List", "") + " UNION ALL " +
                         Part("Invoices_Details_Reverse", "Invoices_List_Reverse", "-") + ") x GROUP BY item_id, store_id, wk";
            Exec(sql, c => { Add(c, "@start", start.Date); Add(c, "@end", end.Date); },
                r => list.Add((L(r, 0), L(r, 1), Convert.ToInt32(r.GetValue(2), CultureInfo.InvariantCulture), Convert.ToDouble(r.GetValue(3), CultureInfo.InvariantCulture))), 300);
            return list;
        }

        /// <summary>آخر مورد لكل صنف من فواتير الشراء غير المحذوفة.</summary>
        public Dictionary<long, (long SupplierId, string Name)> LastSuppliers()
        {
            var map = new Dictionary<long, (long, string)>();
            Exec(@"SELECT x.Item_Id, x.Supplier_Id, x.Name FROM (
    SELECT d.Item_Id, l.Supplier_Id, sp.Name, ROW_NUMBER() OVER (PARTITION BY d.Item_Id ORDER BY l.Invoice_Date DESC, l.ID DESC) AS rn
    FROM Bills_Details d INNER JOIN Bills_List l ON l.ID = d.Invoice_Id LEFT JOIN Supplier sp ON sp.Id = l.Supplier_Id
    WHERE ISNULL(l.Deleted, 0) <> 1 AND ISNULL(l.Supplier_Id, 0) > 0 AND d.Item_Id IS NOT NULL) x
WHERE x.rn = 1", c => { }, r => map[L(r, 0)] = (L(r, 1), S(r, 2)), 120);
            return map;
        }

        // ───────────── التشغيلات ─────────────

        public long StartRun(string source, long by)
        {
            long id = CallSnowflakeIdGenerate.GenerateNewId();
            _newOpp().db.Database.ExecuteSqlInterpolated($@"
INSERT INTO AI_ReorderRuns (Id, Status, Source, CreatedBy, CreatedAt) VALUES ({id}, N'Running', {Cut(source, 20)}, {by}, {DateTime.Now})");
            return id;
        }

        public void FinishRun(long id, string status, int items, int suggested, int ms, string error) =>
            _newOpp().db.Database.ExecuteSqlInterpolated($@"
UPDATE AI_ReorderRuns SET Status = {Cut(status, 20)}, Items = {items}, Suggested = {suggested}, DurationMs = {ms}, ErrorText = {Cut(error, 500)},
       FinishedAt = {DateTime.Now}, UpdatedAt = {DateTime.Now} WHERE Id = {id}");

        public AiReorderRunRow LatestRun(bool doneOnly)
        {
            AiReorderRunRow row = null;
            Exec("SELECT TOP (1) Id, Status, Source, Items, Suggested, DurationMs, ErrorText, CreatedAt, FinishedAt FROM AI_ReorderRuns" +
                 (doneOnly ? " WHERE Status = N'Done'" : "") + " ORDER BY CreatedAt DESC", c => { }, r => row = new AiReorderRunRow
                 {
                     Id = L(r, 0), Status = S(r, 1), Source = S(r, 2), Items = I(r, 3), Suggested = I(r, 4), DurationMs = I(r, 5), ErrorText = S(r, 6),
                     CreatedAt = r.IsDBNull(7) ? (DateTime?)null : r.GetDateTime(7), FinishedAt = r.IsDBNull(8) ? (DateTime?)null : r.GetDateTime(8),
                 });
            return row;
        }

        public void InsertSuggestions(long runId, IEnumerable<AiReorderRow> rows, long by)
        {
            var db = _newOpp().db;
            foreach (var x in rows)
            {
                long id = CallSnowflakeIdGenerate.GenerateNewId();
                DateTime? so = x.StockoutDate.HasValue ? x.StockoutDate.Value.Date : (DateTime?)null;
                db.Database.ExecuteSqlInterpolated($@"
INSERT INTO AI_ReorderSuggestions (Id, Run_Id, UnitItem_Id, Item_Id, Store_Id, Branch_Id, ItemName, ItemCode, StoreName, LargeUnit, Factor, Method, AvgWeekly, Sigma,
       LeadDays, ReorderPoint, SafetyStock, OnHand, SuggestQtySmall, SuggestQtyLarge, ManualReorderLarge, LastSupplier_Id, LastSupplierName, LastPrice, EstCost,
       StockoutDate, Confidence, SeriesJson, Status, CreatedBy, CreatedAt)
VALUES ({id}, {runId}, {x.UnitItem_Id}, {x.Item_Id}, {x.Store_Id}, {x.Branch_Id}, {Cut(x.ItemName, 300)}, {Cut(x.ItemCode, 100)}, {Cut(x.StoreName, 200)},
       {Cut(x.LargeUnit, 50)}, {AiSqlParams.Dec(x.Factor, 18, 4)}, {Cut(x.Method, 20)}, {AiSqlParams.Dec(x.AvgWeekly, 18, 4)}, {AiSqlParams.Dec(x.Sigma, 18, 4)},
       {x.LeadDays}, {AiSqlParams.Dec(x.ReorderPoint, 18, 4)}, {AiSqlParams.Dec(x.SafetyStock, 18, 4)}, {AiSqlParams.Dec(x.OnHand, 18, 4)},
       {AiSqlParams.Dec(x.SuggestQtySmall, 18, 4)}, {AiSqlParams.Dec(x.SuggestQtyLarge, 18, 4)}, {AiSqlParams.Dec(x.ManualReorderLarge, 18, 4)},
       {x.LastSupplier_Id}, {Cut(x.LastSupplierName, 300)}, {AiSqlParams.Dec(x.LastPrice, 18, 4)}, {AiSqlParams.Dec(x.EstCost, 18, 3)},
       {so}, {Cut(x.Confidence, 10)}, {x.SeriesJson}, N'open', {by}, {DateTime.Now})");
            }
        }

        private const string Cols = @"Id, Run_Id, UnitItem_Id, Item_Id, Store_Id, Branch_Id, ItemName, ItemCode, StoreName, LargeUnit, Factor, Method, AvgWeekly, Sigma, LeadDays,
       ReorderPoint, SafetyStock, OnHand, SuggestQtySmall, SuggestQtyLarge, ManualReorderLarge, LastSupplier_Id, LastSupplierName, LastPrice, EstCost, StockoutDate,
       Confidence, SeriesJson, Status, Draft_Id";

        private static AiReorderRow Read(DbDataReader r) => new AiReorderRow
        {
            Id = L(r, 0), Run_Id = L(r, 1), UnitItem_Id = L(r, 2), Item_Id = L(r, 3), Store_Id = L(r, 4), Branch_Id = L(r, 5), ItemName = S(r, 6), ItemCode = S(r, 7),
            StoreName = S(r, 8), LargeUnit = S(r, 9), Factor = M(r, 10), Method = S(r, 11), AvgWeekly = M(r, 12), Sigma = M(r, 13), LeadDays = I(r, 14) ?? 0,
            ReorderPoint = M(r, 15), SafetyStock = M(r, 16), OnHand = M(r, 17), SuggestQtySmall = M(r, 18), SuggestQtyLarge = M(r, 19), ManualReorderLarge = M(r, 20),
            LastSupplier_Id = r.IsDBNull(21) ? (long?)null : L(r, 21), LastSupplierName = S(r, 22), LastPrice = r.IsDBNull(23) ? (decimal?)null : M(r, 23),
            EstCost = r.IsDBNull(24) ? (decimal?)null : M(r, 24), StockoutDate = r.IsDBNull(25) ? (DateTime?)null : r.GetDateTime(25), Confidence = S(r, 26),
            SeriesJson = S(r, 27), Status = S(r, 28), Draft_Id = r.IsDBNull(29) ? (long?)null : L(r, 29),
        };

        /// <summary>اقتراحات تشغيل (مقترحة الكمية &gt; 0) لمخازن مسموحة (null = الكل).</summary>
        public List<AiReorderRow> ListRun(long runId, IList<long> storeIds)
        {
            var list = new List<AiReorderRow>();
            Exec("SELECT " + Cols + " FROM AI_ReorderSuggestions WHERE Run_Id = @r AND SuggestQtyLarge > 0 ORDER BY CASE WHEN StockoutDate IS NULL THEN 1 ELSE 0 END, StockoutDate, ItemName",
                c => Add(c, "@r", runId), r =>
                {
                    var x = Read(r);
                    if (storeIds == null || storeIds.Contains(x.Store_Id)) list.Add(x);
                });
            return list;
        }

        public List<AiReorderRow> GetByIds(IList<long> ids)
        {
            var list = new List<AiReorderRow>();
            if (ids == null || ids.Count == 0) return list;
            string inList = string.Join(",", ids.Distinct().Take(1000).Select(x => x.ToString(CultureInfo.InvariantCulture)));
            Exec("SELECT " + Cols + " FROM AI_ReorderSuggestions WHERE Id IN (" + inList + ")", c => { }, r => list.Add(Read(r)));
            return list;
        }

        public bool SetStatus(long id, string status, long? draftId, long by) =>
            _newOpp().db.Database.ExecuteSqlInterpolated($@"
UPDATE AI_ReorderSuggestions SET Status = {Cut(status, 20)}, Draft_Id = COALESCE({draftId}, Draft_Id), UpdatedBy = {by}, UpdatedAt = {DateTime.Now} WHERE Id = {id}") == 1;

        // ───────────── مهل الموردين ─────────────

        public Dictionary<long, int> SupplierLeadMap()
        {
            var map = new Dictionary<long, int>();
            Exec("SELECT Supplier_Id, LeadDays FROM AI_ReorderSupplierLead", c => { }, r => map[L(r, 0)] = I(r, 1) ?? 0);
            return map;
        }

        public List<(long SupplierId, string Name, int LeadDays)> SupplierLeads()
        {
            var list = new List<(long, string, int)>();
            Exec("SELECT l.Supplier_Id, ISNULL(s.Name, N''), l.LeadDays FROM AI_ReorderSupplierLead l LEFT JOIN Supplier s ON s.Id = l.Supplier_Id ORDER BY s.Name",
                c => { }, r => list.Add((L(r, 0), S(r, 1), I(r, 2) ?? 0)));
            return list;
        }

        public void SetSupplierLead(long supplierId, int? leadDays, long by)
        {
            if (leadDays == null)
            {
                _newOpp().db.Database.ExecuteSqlInterpolated($"DELETE FROM AI_ReorderSupplierLead WHERE Supplier_Id = {supplierId}");
                return;
            }
            long id = CallSnowflakeIdGenerate.GenerateNewId();
            _newOpp().db.Database.ExecuteSqlInterpolated($@"
IF EXISTS (SELECT 1 FROM AI_ReorderSupplierLead WHERE Supplier_Id = {supplierId})
    UPDATE AI_ReorderSupplierLead SET LeadDays = {leadDays.Value}, UpdatedBy = {by}, UpdatedAt = {DateTime.Now} WHERE Supplier_Id = {supplierId}
ELSE
    INSERT INTO AI_ReorderSupplierLead (Id, Supplier_Id, LeadDays, CreatedBy, CreatedAt) VALUES ({id}, {supplierId}, {leadDays.Value}, {by}, {DateTime.Now})");
        }

        // ───────────── أدوات ─────────────

        private static long L(DbDataReader r, int i) => r.IsDBNull(i) ? 0 : Convert.ToInt64(r.GetValue(i), CultureInfo.InvariantCulture);
        private static int? I(DbDataReader r, int i) => r.IsDBNull(i) ? (int?)null : Convert.ToInt32(r.GetValue(i), CultureInfo.InvariantCulture);
        private static decimal M(DbDataReader r, int i) => r.IsDBNull(i) ? 0 : Convert.ToDecimal(r.GetValue(i), CultureInfo.InvariantCulture);
        private static string S(DbDataReader r, int i) => r.IsDBNull(i) ? null : Convert.ToString(r.GetValue(i), CultureInfo.InvariantCulture);
        private static string Cut(string s, int max) => s == null ? null : (s.Length > max ? s.Substring(0, max) : s);

        private static void Add(DbCommand cmd, string name, object value)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }

        private void Exec(string sql, Action<DbCommand> bind, Action<DbDataReader> row, int timeoutSeconds = 30)
        {
            var db = _newOpp().db;
            var conn = db.Database.GetDbConnection();
            bool wasClosed = conn.State != ConnectionState.Open;
            if (wasClosed) conn.Open();
            try
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.CommandTimeout = timeoutSeconds;
                    bind(cmd);
                    using (var r = cmd.ExecuteReader())
                        while (r.Read()) row(r);
                }
            }
            finally { if (wasClosed) conn.Close(); }
        }
    }
}
