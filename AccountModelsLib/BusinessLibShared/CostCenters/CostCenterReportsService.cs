using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib.BusinessLibShared.CostCenters
{
    /// <summary>
    /// تقارير مراكز التكلفة (المرحلة 5 من cost-centers-razor-plan.md). قراءة فقط، استعلام مباشر على Account_Entry_Details
    /// (البُعد على سطر القيد = مصدر الحقيقة ق1) بلا جداول Temp، والقيم بالمكافىء (Equivalent_*) كشاشات الأستاذ بعملة «بالمكافىء».
    /// الرصيد السابق = كل ما قبل تاريخ البداية (كما يفعل محرك الأستاذ المساعد: «رصيد منقول» من أول تاريخ).
    /// التجميع التراكمى (ق11): المركز يشمل أبناءه عند rollUp، ومجموع التقرير الكلى يُحسب من الحركات الفعلية فقط فلا يتكرر.
    /// </summary>
    public static class CostCenterReportsService
    {
        // ------------------------------------------------------------------ أدوات مشتركة

        private const string Debit = "ISNULL(d.Equivalent_Import_Value, 0)";
        private const string Credit = "ISNULL(d.Equivalent_Export_Value, 0)";

        /// <summary>معرفات المراكز التى يشملها التقرير: 0 = كل ما له مركز، وإلا المركز (مع أبنائه إن طُلب).</summary>
        private static string CenterFilter(operations opp, long centerId, bool withChildren)
        {
            if (centerId <= 0) return "ISNULL(d.CostCenter_Id, 0) > 0";
            var ids = new List<long> { centerId };
            if (withChildren) ids.AddRange(CostCenterService.Descendants(opp, centerId).Select(n => n.Id));
            return "d.CostCenter_Id IN (" + string.Join(",", ids) + ")";
        }

        private static string BranchFilter(long branchId) => branchId > 0 ? " AND l.Branch_Id_Entry = " + branchId : "";

        private const string FromJoin =
            " FROM Account_Entry_Details d INNER JOIN Account_Entry_List l ON l.ID = d.Account_Entry_List_ID " +
            "INNER JOIN Submain_Account a ON a.Id = d.Submain_Account_ID LEFT JOIN Cost_Center_Tree c ON c.Id = d.CostCenter_Id ";

        // ------------------------------------------------------------------ 1) أستاذ مركز التكلفة

        public class LedgerLine
        {
            /// <summary>0 حركة، 1 رصيد سابق، 2 إجمالى الفترة، 3 الرصيد الختامى.</summary>
            public int Kind { get; set; }
            public long EntryId { get; set; }
            public long EntryNo { get; set; }
            public DateTime? Date { get; set; }
            public string Journal { get; set; } = "";
            public string AccountNo { get; set; } = "";
            public string AccountName { get; set; } = "";
            public string CenterName { get; set; } = "";
            public string Notes { get; set; } = "";
            public decimal Debit { get; set; }
            public decimal Credit { get; set; }
            /// <summary>الرصيد الجارى (مدين موجب).</summary>
            public decimal Balance { get; set; }
        }

        private class LedgerRow
        {
            public long EntryId { get; set; }
            public long EntryNo { get; set; }
            public DateTime EntryDate { get; set; }
            public string Journal { get; set; }
            public string AccountNo { get; set; }
            public string AccountName { get; set; }
            public string CenterName { get; set; }
            public string Notes { get; set; }
            public decimal Debit { get; set; }
            public decimal Credit { get; set; }
        }

        private class Sums { public decimal Debit { get; set; } public decimal Credit { get; set; } }

        public static List<LedgerLine> Ledger(operations opp, long centerId, bool withChildren, DateTime from, DateTime to, long branchId, long accountId)
        {
            string where = CenterFilter(opp, centerId, withChildren) + BranchFilter(branchId) + (accountId > 0 ? " AND d.Submain_Account_ID = " + accountId : "");

            var prev = opp.db.Database.SqlQueryRaw<Sums>(
                "SELECT CAST(ISNULL(SUM(" + Debit + "), 0) AS decimal(18,3)) AS Debit, CAST(ISNULL(SUM(" + Credit + "), 0) AS decimal(18,3)) AS Credit" +
                FromJoin + "WHERE " + where + " AND l.Entry_Date < {0}", from.Date).AsEnumerable().First();

            var rows = opp.db.Database.SqlQueryRaw<LedgerRow>(
                "SELECT CAST(l.ID AS bigint) AS EntryId, CAST(ISNULL(l.Entry_No, 0) AS bigint) AS EntryNo, l.Entry_Date AS EntryDate, " +
                "ISNULL(l.Journal_Name, N'') AS Journal, ISNULL(a.Account_Code, N'') AS AccountNo, ISNULL(a.SubAccount_Name, N'') AS AccountName, " +
                "ISNULL(c.Name, N'') AS CenterName, ISNULL(NULLIF(d.Notes_Details, N''), ISNULL(l.Entry_Notes, N'')) AS Notes, " +
                "CAST(" + Debit + " AS decimal(18,3)) AS Debit, CAST(" + Credit + " AS decimal(18,3)) AS Credit" +
                FromJoin + "WHERE " + where + " AND l.Entry_Date >= {0} AND l.Entry_Date < {1} ORDER BY l.Entry_Date, l.Entry_No, d.Line_No",
                from.Date, to.Date.AddDays(1)).ToList();

            var res = new List<LedgerLine>();
            decimal bal = prev.Debit - prev.Credit;
            res.Add(new LedgerLine { Kind = 1, Debit = prev.Debit, Credit = prev.Credit, Balance = bal });
            decimal td = 0, tc = 0;
            foreach (var r in rows)
            {
                bal += r.Debit - r.Credit; td += r.Debit; tc += r.Credit;
                res.Add(new LedgerLine
                {
                    Kind = 0, EntryId = r.EntryId, EntryNo = r.EntryNo, Date = r.EntryDate, Journal = r.Journal, AccountNo = r.AccountNo,
                    AccountName = r.AccountName, CenterName = r.CenterName, Notes = r.Notes, Debit = r.Debit, Credit = r.Credit, Balance = bal
                });
            }
            res.Add(new LedgerLine { Kind = 2, Debit = td, Credit = tc, Balance = bal });
            res.Add(new LedgerLine { Kind = 3, Balance = bal });
            return res;
        }

        // ------------------------------------------------------------------ 2) ميزان مراجعة بالمراكز

        public class TbRow
        {
            public long AccountId { get; set; }
            public string AccountNo { get; set; } = "";
            public string AccountName { get; set; } = "";
            public decimal OpenDebit { get; set; }
            public decimal OpenCredit { get; set; }
            public decimal Debit { get; set; }
            public decimal Credit { get; set; }
            public decimal Open => OpenDebit - OpenCredit;
            public decimal Close => OpenDebit - OpenCredit + Debit - Credit;
            public void Add(TbRow o) { OpenDebit += o.OpenDebit; OpenCredit += o.OpenCredit; Debit += o.Debit; Credit += o.Credit; }
            public TbRow Clone() => (TbRow)MemberwiseClone();
        }

        public class TbGroup
        {
            public CostCenterService.Node Center { get; set; }
            public List<TbRow> Rows { get; set; } = new();
            public TbRow Total { get; set; } = new();
        }

        public class TbResult
        {
            public List<TbGroup> Groups { get; set; } = new();
            public TbRow Total { get; set; } = new();
        }

        private class TbRaw
        {
            public long CenterId { get; set; }
            public long AccountId { get; set; }
            public string AccountNo { get; set; }
            public string AccountName { get; set; }
            public decimal OpenDebit { get; set; }
            public decimal OpenCredit { get; set; }
            public decimal Debit { get; set; }
            public decimal Credit { get; set; }
        }

        public static TbResult TrialBalance(operations opp, long rootId, DateTime from, DateTime to, long branchId, bool rollUp)
        {
            string where = CenterFilter(opp, rootId, true) + BranchFilter(branchId) + " AND l.Entry_Date < {1}";
            var raw = opp.db.Database.SqlQueryRaw<TbRaw>(
                "SELECT CAST(d.CostCenter_Id AS bigint) AS CenterId, CAST(a.Id AS bigint) AS AccountId, ISNULL(a.Account_Code, N'') AS AccountNo, ISNULL(a.SubAccount_Name, N'') AS AccountName, " +
                "CAST(ISNULL(SUM(CASE WHEN l.Entry_Date < {0} THEN " + Debit + " ELSE 0 END), 0) AS decimal(18,3)) AS OpenDebit, " +
                "CAST(ISNULL(SUM(CASE WHEN l.Entry_Date < {0} THEN " + Credit + " ELSE 0 END), 0) AS decimal(18,3)) AS OpenCredit, " +
                "CAST(ISNULL(SUM(CASE WHEN l.Entry_Date >= {0} THEN " + Debit + " ELSE 0 END), 0) AS decimal(18,3)) AS Debit, " +
                "CAST(ISNULL(SUM(CASE WHEN l.Entry_Date >= {0} THEN " + Credit + " ELSE 0 END), 0) AS decimal(18,3)) AS Credit" +
                FromJoin + "WHERE " + where + " GROUP BY d.CostCenter_Id, a.Id, a.Account_Code, a.SubAccount_Name",
                from.Date, to.Date.AddDays(1)).ToList();

            var tree = CostCenterService.GetTree(opp);
            var byId = tree.ToDictionary(n => n.Id);

            var own = new Dictionary<long, Dictionary<long, TbRow>>();
            var res = new TbResult();
            foreach (var r in raw)
            {
                var row = new TbRow { AccountId = r.AccountId, AccountNo = r.AccountNo, AccountName = r.AccountName, OpenDebit = r.OpenDebit, OpenCredit = r.OpenCredit, Debit = r.Debit, Credit = r.Credit };
                if (!own.TryGetValue(r.CenterId, out var m)) own[r.CenterId] = m = new Dictionary<long, TbRow>();
                m[r.AccountId] = row;
                res.Total.Add(row);
            }

            var acc = own;
            if (rollUp)
            {
                // نسخ ثم طىّ كل عقدة فى أبيها من الأعمق للأعلى (كل عقدة تضم نفسها وأبناءها)
                acc = own.ToDictionary(kv => kv.Key, kv => kv.Value.ToDictionary(x => x.Key, x => x.Value.Clone()));
                foreach (var n in tree.OrderByDescending(x => x.Level))
                {
                    if (!acc.TryGetValue(n.Id, out var mine) || n.ParentId == 0 || !byId.ContainsKey(n.ParentId)) continue;
                    if (!acc.TryGetValue(n.ParentId, out var up)) acc[n.ParentId] = up = new Dictionary<long, TbRow>();
                    foreach (var kv in mine)
                    {
                        if (up.TryGetValue(kv.Key, out var ex)) ex.Add(kv.Value);
                        else up[kv.Key] = kv.Value.Clone();
                    }
                }
            }

            HashSet<long> scope = null;
            if (rootId > 0) { scope = new HashSet<long>(CostCenterService.Descendants(opp, rootId).Select(n => n.Id)) { rootId }; }
            foreach (var n in tree.OrderBy(x => x.CodeStr, StringComparer.Ordinal))
            {
                if (scope != null && !scope.Contains(n.Id)) continue;
                if (!acc.TryGetValue(n.Id, out var rows) || rows.Count == 0) continue;
                var g = new TbGroup { Center = n, Rows = rows.Values.OrderBy(x => x.AccountNo, StringComparer.Ordinal).ToList() };
                foreach (var x in g.Rows) g.Total.Add(x);
                res.Groups.Add(g);
            }
            return res;
        }

        // ------------------------------------------------------------------ 3) قائمة دخل بالمركز (ربحية)

        public class ProfitRow
        {
            public CostCenterService.Node Center { get; set; }
            public decimal Revenue { get; set; }
            public decimal Cost { get; set; }
            public decimal PrevRevenue { get; set; }
            public decimal PrevCost { get; set; }
            public decimal Profit => Revenue - Cost;
            public decimal PrevProfit => PrevRevenue - PrevCost;
            public decimal Margin => Revenue == 0 ? 0 : Profit / Revenue * 100m;
            public void Add(ProfitRow o) { Revenue += o.Revenue; Cost += o.Cost; PrevRevenue += o.PrevRevenue; PrevCost += o.PrevCost; }
        }

        public class ProfitResult
        {
            public List<ProfitRow> Rows { get; set; } = new();
            public ProfitRow Total { get; set; } = new();
            public DateTime? PrevFrom { get; set; }
            public DateTime? PrevTo { get; set; }
        }

        private class ProfitRaw
        {
            public long CenterId { get; set; }
            public decimal Revenue { get; set; }
            public decimal Cost { get; set; }
            public decimal PrevRevenue { get; set; }
            public decimal PrevCost { get; set; }
        }

        /// <summary>الأرباح والخسائر: الإيراد = حسابات Head_Main_Account=5 (دائن − مدين)، والتكلفة = =4 (مدين − دائن) — ERP_V1_3 §4.2.</summary>
        public static ProfitResult Profitability(operations opp, long rootId, DateTime from, DateTime to, long branchId, bool rollUp, bool compare)
        {
            var res = new ProfitResult();
            DateTime pFrom = from.Date, pTo = from.Date.AddDays(-1);
            if (compare)
            {
                int days = (int)(to.Date - from.Date).TotalDays;
                pFrom = pTo.AddDays(-days);
                res.PrevFrom = pFrom; res.PrevTo = pTo;
            }
            string cur = "l.Entry_Date >= {0} AND l.Entry_Date < {1}";
            string prv = compare ? "l.Entry_Date >= {2} AND l.Entry_Date < {3}" : "1 = 0";
            string where = CenterFilter(opp, rootId, true) + BranchFilter(branchId) + " AND a.Head_Main_Account IN (4, 5) AND (" + cur + (compare ? " OR " + prv : "") + ")";
            string net(string head, string sign, string range) =>
                "CAST(ISNULL(SUM(CASE WHEN a.Head_Main_Account = " + head + " AND " + range + " THEN " + sign + " ELSE 0 END), 0) AS decimal(18,3))";
            string credMinusDeb = Credit + " - " + Debit, debMinusCred = Debit + " - " + Credit;

            var args = compare
                ? new object[] { from.Date, to.Date.AddDays(1), pFrom, pTo.AddDays(1) }
                : new object[] { from.Date, to.Date.AddDays(1) };
            var raw = opp.db.Database.SqlQueryRaw<ProfitRaw>(
                "SELECT CAST(d.CostCenter_Id AS bigint) AS CenterId, " +
                net("5", credMinusDeb, cur) + " AS Revenue, " + net("4", debMinusCred, cur) + " AS Cost, " +
                net("5", credMinusDeb, prv) + " AS PrevRevenue, " + net("4", debMinusCred, prv) + " AS PrevCost" +
                FromJoin + "WHERE " + where + " GROUP BY d.CostCenter_Id", args).ToList();

            var tree = CostCenterService.GetTree(opp);
            var byId = tree.ToDictionary(n => n.Id);
            var acc = new Dictionary<long, ProfitRow>();
            foreach (var r in raw)
            {
                if (!byId.TryGetValue(r.CenterId, out var node)) continue;
                var row = new ProfitRow { Center = node, Revenue = r.Revenue, Cost = r.Cost, PrevRevenue = r.PrevRevenue, PrevCost = r.PrevCost };
                acc[r.CenterId] = row;
                res.Total.Add(row);
            }
            if (rollUp)
            {
                var copy = acc.ToDictionary(kv => kv.Key, kv => { var o = kv.Value; var c = new ProfitRow { Center = o.Center }; c.Add(o); return c; });
                foreach (var n in tree.OrderByDescending(x => x.Level))
                {
                    if (!copy.TryGetValue(n.Id, out var mine) || !byId.ContainsKey(n.ParentId)) continue;
                    if (!copy.TryGetValue(n.ParentId, out var up)) copy[n.ParentId] = up = new ProfitRow { Center = byId[n.ParentId] };
                    up.Add(mine);
                }
                acc = copy;
            }
            HashSet<long> scope = null;
            if (rootId > 0) scope = new HashSet<long>(CostCenterService.Descendants(opp, rootId).Select(n => n.Id)) { rootId };
            foreach (var n in tree.OrderBy(x => x.CodeStr, StringComparer.Ordinal))
            {
                if (scope != null && !scope.Contains(n.Id)) continue;
                if (!acc.TryGetValue(n.Id, out var row)) continue;
                if (row.Revenue == 0 && row.Cost == 0 && row.PrevRevenue == 0 && row.PrevCost == 0) continue;
                res.Rows.Add(row);
            }
            return res;
        }

        // ------------------------------------------------------------------ 4) تحليل تكاليف المشروع

        public class AmountRow
        {
            public string Name { get; set; } = "";
            public decimal Amount { get; set; }
            public decimal Percent { get; set; }
        }

        public class ProjectAnalysis
        {
            public string ProjectName { get; set; } = "";
            public decimal ContractValue { get; set; }
            public decimal TotalCost { get; set; }
            public decimal TotalRevenue { get; set; }
            public decimal CostPercentOfContract => ContractValue == 0 ? 0 : TotalCost / ContractValue * 100m;
            public List<AmountRow> ByGroup { get; set; } = new();
            public List<AmountRow> ByPhase { get; set; } = new();
        }

        private class GroupRaw
        {
            public long CenterId { get; set; }
            public string GroupName { get; set; }
            public decimal Cost { get; set; }
            public decimal Revenue { get; set; }
        }

        /// <summary>تكلفة المركز (مع كل أبنائه) مجمعة حسب مجموعة الحساب (الحساب الأب) وحسب المرحلة (الأبناء المباشرون).</summary>
        public static ProjectAnalysis AnalyzeProject(operations opp, long rootId, DateTime from, DateTime to, long branchId)
        {
            var tree = CostCenterService.GetTree(opp);
            var byId = tree.ToDictionary(n => n.Id);
            var res = new ProjectAnalysis { ProjectName = byId.TryGetValue(rootId, out var root) ? root.Name : "" };
            try { res.ContractValue = CostCenterService.GetProject(opp, rootId)?.ContractValue ?? 0; } catch { }

            var raw = opp.db.Database.SqlQueryRaw<GroupRaw>(
                "SELECT CAST(d.CostCenter_Id AS bigint) AS CenterId, " +
                "ISNULL((SELECT TOP 1 p.SubAccount_Name FROM Submain_Account p WHERE p.Id = a.Main_Account_Id), N'') AS GroupName, " +
                "CAST(ISNULL(SUM(CASE WHEN a.Head_Main_Account = 4 THEN " + Debit + " - " + Credit + " ELSE 0 END), 0) AS decimal(18,3)) AS Cost, " +
                "CAST(ISNULL(SUM(CASE WHEN a.Head_Main_Account = 5 THEN " + Credit + " - " + Debit + " ELSE 0 END), 0) AS decimal(18,3)) AS Revenue" +
                FromJoin + "WHERE " + CenterFilter(opp, rootId, true) + BranchFilter(branchId) +
                " AND a.Head_Main_Account IN (4, 5) AND l.Entry_Date >= {0} AND l.Entry_Date < {1} " +
                "GROUP BY d.CostCenter_Id, a.Main_Account_Id", from.Date, to.Date.AddDays(1)).ToList();

            var groups = new Dictionary<string, decimal>();
            var phases = new Dictionary<long, decimal>();
            foreach (var r in raw)
            {
                res.TotalCost += r.Cost; res.TotalRevenue += r.Revenue;
                if (r.Cost == 0) continue;
                string g = string.IsNullOrWhiteSpace(r.GroupName) ? "—" : r.GroupName;
                groups[g] = groups.GetValueOrDefault(g) + r.Cost;
                // المرحلة = الابن المباشر للمشروع الذى يقع المركز تحته (0 = حركات المشروع نفسه)
                long cur = r.CenterId, top = 0; int guard = 0;
                while (cur != rootId && byId.TryGetValue(cur, out var n) && guard++ < 50) { top = cur; cur = n.ParentId; }
                phases[cur == rootId ? top : 0] = phases.GetValueOrDefault(cur == rootId ? top : 0) + r.Cost;
            }
            decimal total = res.TotalCost == 0 ? 1 : res.TotalCost;
            res.ByGroup = groups.OrderByDescending(x => x.Value).Select(x => new AmountRow { Name = x.Key, Amount = x.Value, Percent = x.Value / total * 100m }).ToList();
            res.ByPhase = phases.OrderBy(x => x.Key == 0 ? "" : byId[x.Key].CodeStr, StringComparer.Ordinal)
                .Select(x => new AmountRow { Name = x.Key == 0 ? res.ProjectName : byId[x.Key].Name, Amount = x.Value, Percent = x.Value / total * 100m }).ToList();
            return res;
        }
    }
}
