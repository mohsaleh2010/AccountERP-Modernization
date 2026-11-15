using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using AccountERP.AI.Tools;
using AccountModelsLib;
using AccountModelsLib.BusinessLibShared.GeneralLedger;

namespace AccountERP.AI.Brief
{
    /// <summary>صندوق نقدية مسموح للمستلم (من CashVouchersService.GetBoxes كشاشات الخزينة).</summary>
    public sealed record AiBriefBox(long Id, string Name, long BranchId, long CurrencyId, string CurrencyName, string CurrencyNameEn);

    /// <summary>
    /// نطاق موجز لمستلم (ق11–ق13): الفروع والأقسام وصلاحية الفواتير الضريبية والصناديق كما يراها هو.
    /// المستخدمون ذوو النطاق المتطابق يتشاركون صف AI_Briefs واحداً (ScopeKey).
    /// </summary>
    public sealed class AiBriefScope
    {
        public required AiServerUser Server { get; init; }
        public long UserId => Server.UserId;
        public required List<long> BranchIds { get; init; }
        public required Dictionary<long, string> BranchNames { get; init; }
        /// <summary>الأقسام المعروضة (المسموحة ∩ اختيار الاشتراك)، مرتبة كـ AiBriefSections.All.</summary>
        public required List<string> Sections { get; init; }
        /// <summary>مجموعات الحقائق التى تُحسب: الأقسام المعروضة + ما تحتاجه التنبيهات المسموحة (stock للتنبيه negative_stock).</summary>
        public required List<string> FactGroups { get; init; }
        public required int TaxFilter { get; init; }
        public required List<AiBriefBox> Boxes { get; init; }
        public required HashSet<string> AllowedViews { get; init; }
        public required string ScopeKey { get; init; }

        public bool HasRoute(string route) => AiBriefScopes.HasRoute(Server.User, route);
        public bool Computes(string group) => FactGroups.Contains(group, StringComparer.OrdinalIgnoreCase);
        public bool Shows(string section) => Sections.Contains(section, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>نتيجة بناء النطاق: Scope أو سبب التخطى (للسجل).</summary>
    public sealed record AiBriefScopeResult(AiBriefScope? Scope, string? SkipReason);

    public static class AiBriefScopes
    {
        /// <summary>مجموعة حقائق خاصة بالتنبيهات فقط (ليست قسماً).</summary>
        public const string StockGroup = "stock";
        /// <summary>P8.4 (ش27): مجموعة حقائق تنبيه شذوذ القيود (ليست قسماً).</summary>
        public const string AnomalyGroup = "anomalies";
        /// <summary>P8.5 (ت31): مجموعة حقائق تنبيه أولويات التحصيل (ليست قسماً).</summary>
        public const string CollectGroup = "collect";

        public static class Skip
        {
            public const string NoUser = "no_user";
            public const string Inactive = "inactive";
            public const string NoMenu = "no_menu";
            public const string NoSections = "no_sections";
            public const string NoBranches = "no_branches";
        }

        /// <summary>نفس مطابقة AiToolContext.HasRoute (المسار بلا معاملات الاستعلام؛ المدير يملك الكل).</summary>
        public static bool HasRoute(AiUserContext? user, string route)
        {
            if (user == null) return false;
            if (user.IsAdmin) return true;
            foreach (var r in user.AllowedRoutes)
            {
                string path = (r ?? "").Split('?')[0].Trim().TrimEnd('/');
                if (path.Length > 0 && string.Equals(path, route.TrimEnd('/'), StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        /// <summary>
        /// الشاشة المانحة لكل قسم (§1). الآجل يقرأ ai.v_sales فيحتاج شاشة المبيعات أيضاً (صلاحية الـ View).
        /// التنبيهات «مسموحة» لمن يملك أى مصدر تنبيه؛ كل تنبيه يُفلتر وحده فى AiBriefAlerts.
        /// </summary>
        public static bool Grants(string section, Func<string, bool> hasRoute) => section switch
        {
            AiBriefSections.Sales => hasRoute(AiSchemaCatalog.RSales),
            AiBriefSections.Collections => hasRoute(AiSchemaCatalog.RCashVouchers) || hasRoute(AiSchemaCatalog.RCashTotals),
            AiBriefSections.Liquidity => hasRoute(AiSchemaCatalog.RCashTotals) || hasRoute(AiSchemaCatalog.RCashBoxStatement),
            AiBriefSections.Receivables => hasRoute(AiSchemaCatalog.RCustomerBalances) && hasRoute(AiSchemaCatalog.RSales),
            AiBriefSections.Purchases => hasRoute(AiSchemaCatalog.RPurchases),
            AiBriefSections.Alerts => AlertGroups(hasRoute).Count > 0,
            StockGroup => hasRoute(AiSchemaCatalog.RStock),
            AnomalyGroup => hasRoute(AiSchemaCatalog.REntryAnomalies),
            CollectGroup => hasRoute(AiSchemaCatalog.RCollections),
            _ => false,
        };

        /// <summary>مجموعات الحقائق التى تغذى تنبيهات مسموحة لهذا المستخدم.</summary>
        public static List<string> AlertGroups(Func<string, bool> hasRoute)
        {
            var list = new List<string>();
            foreach (var g in new[] { AiBriefSections.Sales, AiBriefSections.Collections, AiBriefSections.Liquidity, AiBriefSections.Receivables, StockGroup, AnomalyGroup, CollectGroup })
                if (g != AiBriefSections.Alerts && Grants(g, hasRoute)) list.Add(g);
            return list;
        }

        public static List<string> AllowedSections(Func<string, bool> hasRoute) =>
            AiBriefSections.All.Where(s => Grants(s, hasRoute)).ToList();

        /// <summary>
        /// يبنى النطاق لمستلم: مستخدم مفعّل + صلاحية قائمة الموجز + أقسام مسموحة ∩ الاشتراك (أو الافتراضى) + فروعه المسموحة ∩ الاشتراك + صناديقه.
        /// subscription = null ⇒ الافتراضى (كل الفروع المسموحة والأقسام الافتراضية).
        /// </summary>
        public static AiBriefScopeResult Resolve(AiServerUser server, AiBriefSubscriptionRow? subscription)
        {
            if (server == null || !server.Found) return new(null, Skip.NoUser);
            if (!server.Usable) return new(null, Skip.Inactive);
            if (!server.HasRoute(AiBriefConst.Route) && !server.HasMenuKey(AiBriefConst.MenuKey)) return new(null, Skip.NoMenu);

            Func<string, bool> hasRoute = r => HasRoute(server.User, r);
            var wanted = subscription?.Sections is { Count: > 0 } chosen ? chosen : AiBriefSections.Default.ToList();
            var sections = AiBriefSections.All.Where(s => wanted.Contains(s, StringComparer.OrdinalIgnoreCase) && Grants(s, hasRoute)).ToList();
            if (sections.Count == 0) return new(null, Skip.NoSections);

            var groups = new List<string>(sections.Where(s => s != AiBriefSections.Alerts));
            if (sections.Contains(AiBriefSections.Alerts))
                foreach (var g in AlertGroups(hasRoute))
                    if (!groups.Contains(g)) groups.Add(g);

            // الفروع المسموحة بجلسة المستخدم نفسه (المدير: كل الفروع).
            var allowed = server.Run(() => new FinalAccountsService().GetBranches());
            var names = allowed.GroupBy(b => b.Id).ToDictionary(g => g.Key, g => g.First().Name);
            var chosenBranches = subscription?.BranchIds ?? new List<long>();
            var branches = (chosenBranches.Count > 0 ? names.Keys.Where(chosenBranches.Contains) : names.Keys).OrderBy(x => x).ToList();
            if (branches.Count == 0) return new(null, Skip.NoBranches);

            var boxes = new List<AiBriefBox>();
            if (groups.Contains(AiBriefSections.Collections) || groups.Contains(AiBriefSections.Liquidity))
            {
                var dt = server.Run(() => new CashVouchersService().GetBoxes(server.UserId));
                foreach (DataRow r in dt.Rows)
                {
                    long id = Convert.ToInt64(r["Id"]);
                    long branchId = r["Branch_Id"] == DBNull.Value ? 0 : Convert.ToInt64(r["Branch_Id"]);
                    // صندوق فرع خارج النطاق يُستبعد؛ الصندوق المشترك (0 / -1) يبقى.
                    if (branchId > 0 && !branches.Contains(branchId)) continue;
                    boxes.Add(new AiBriefBox(id, Str(r, "Bank_Name"), branchId, Long(r, "currency_Id"), Str(r, "Currency_Name"), Str(r, "Currency_Name_EN")));
                }
                boxes = boxes.GroupBy(b => b.Id).Select(g => g.First()).OrderBy(b => b.Id).ToList();
            }

            int tax = server.TaxFilter;
            string key = ComputeKey(branches, sections, groups, tax, boxes.Select(b => b.Id), AiBriefConst.CollectorVersion);
            var views = AiToolContext.ViewsFor(server.User, hasRoute);
            return new(new AiBriefScope
            {
                Server = server, BranchIds = branches, BranchNames = names.Where(kv => branches.Contains(kv.Key)).ToDictionary(kv => kv.Key, kv => kv.Value),
                Sections = sections, FactGroups = groups, TaxFilter = tax, Boxes = boxes, AllowedViews = views, ScopeKey = key,
            }, null);
        }

        /// <summary>ق13: SHA-256 (hex صغير، 64 حرفاً) لكل ما يغيّر الأرقام أو ما يُعرض.</summary>
        public static string ComputeKey(IEnumerable<long> branches, IEnumerable<string> sections, IEnumerable<string> groups, int taxFilter, IEnumerable<long> boxIds, int collectorVersion)
        {
            string raw = string.Join("|",
                "v" + collectorVersion.ToString(CultureInfo.InvariantCulture),
                "b:" + AiBriefCsv.Join(branches),
                "s:" + AiBriefCsv.Join(sections),
                "g:" + AiBriefCsv.Join(groups),
                "t:" + taxFilter.ToString(CultureInfo.InvariantCulture),
                "x:" + AiBriefCsv.Join(boxIds));
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw))).ToLowerInvariant();
        }

        /// <summary>
        /// ق14: الفترة = اليوم السابق المكتمل؛ وإن انقطع الموجز تغطى الأيام منذ آخر موجز للنطاق بحد أقصى 7 أيام.
        /// lastCovered = آخر PeriodTo صدر لنفس النطاق قبل هذا اليوم (AiBriefCLS.LastCoveredDate).
        /// </summary>
        public static (DateTime From, DateTime To) Period(DateTime today, DateTime? lastCovered)
        {
            var to = today.Date.AddDays(-1);
            var from = to;
            if (lastCovered is DateTime last && last.Date < to)
            {
                from = last.Date.AddDays(1);
                var earliest = to.AddDays(-(AiBriefConst.MaxGapDays - 1));
                if (from < earliest) from = earliest;
            }
            return (from, to);
        }

        private static string Str(DataRow r, string c) => r.Table.Columns.Contains(c) && r[c] != DBNull.Value ? Convert.ToString(r[c]) ?? "" : "";
        private static long Long(DataRow r, string c) => r.Table.Columns.Contains(c) && r[c] != DBNull.Value && long.TryParse(Convert.ToString(r[c], CultureInfo.InvariantCulture), out var v) ? v : 0;
    }
}
