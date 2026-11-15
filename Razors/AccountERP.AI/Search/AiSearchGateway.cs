using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using AccountERP.AI.Chat;
using AccountERP.AI.Tools;
using AccountModelsLib;
using AccountModelsLib.BusinessLibShared.GeneralLedger;

namespace AccountERP.AI.Search
{
    /// <summary>
    /// P8.2.3: بحث البيانات فى لوحة Ctrl+K (.claude/docs/ai-assistant-p82-plan.md §1–§3). بلا أى استدعاء نموذج (س1):
    /// عبارة SELECT واحدة (UNION ALL) مبنية من ثوابت وكل قيمة فيها معامل، عبر ReadOnlySqlExecutor (نفس مسار الحماية والـ Views وai_tax).
    /// النوع يُبحث فيه فقط إن كان الـ View مسموحاً والمستخدم يملك شاشة الفتح (س13)، والمستندات مقيّدة بفروع المستخدم (س14).
    /// Scoped: ذاكرة المثيل (سياق الأدوات والفروع 5 دقائق، الإعداد 60 ث) لنفس المستخدم.
    /// </summary>
    public sealed class AiSearchGateway : IAiSearchGateway
    {
        /// <summary>مصدر نوع نتيجة: الـ View والأعمدة وشاشة الفتح (§1).</summary>
        public sealed record KindSpec(string Kind, string View, string IdCol, string TitleCol, string CodeCol,
                                      string DateCol, string PartyCol, string OpenRoute, string ScreenKey, bool IsDocument);

        public static readonly KindSpec[] Specs =
        {
            new(AiSearchKinds.Customer, "v_customers", "customer_id", "customer_name", "customer_code", "", "", "/customers/info", "customer_info", false),
            new(AiSearchKinds.Supplier, "v_suppliers", "supplier_id", "supplier_name", "supplier_code", "", "", "/suppliers/info", "supplier_info", false),
            new(AiSearchKinds.Item, "v_items", "item_id", "item_name", "item_code", "", "", "/items", "item_card", false),
            new(AiSearchKinds.Sale, "v_sales", "sale_id", "invoice_no", "", "invoice_date", "customer_name", "/accounts/invoices/sales-invoice", "sales_invoice", true),
            new(AiSearchKinds.SaleReturn, "v_sales_returns", "return_id", "return_no", "", "return_date", "customer_name", "/accounts/invoices/sales-return", "sales_return", true),
            new(AiSearchKinds.Purchase, "v_purchases", "purchase_id", "invoice_no", "", "invoice_date", "supplier_name", "/accounts/invoices/purchase-invoice", "purchase_invoice", true),
            new(AiSearchKinds.PurchaseReturn, "v_purchase_returns", "return_id", "return_no", "", "return_date", "supplier_name", "/accounts/invoices/purchase-return", "purchase_return", true),
        };

        public static KindSpec? SpecOf(string kind) => Specs.FirstOrDefault(s => s.Kind == kind);

        private static readonly TimeSpan CtxTtl = TimeSpan.FromMinutes(5), SettingTtl = TimeSpan.FromSeconds(60);

        private readonly ReadOnlySqlExecutor _sql;
        private readonly AiSearchCLS _recent;
        private readonly AiToolAuditCLS _audit;

        // ───── خطافات الاختبار (harness فقط؛ null = السلوك الحقيقى) ─────
        public Func<AiUserContext, AiToolContext>? ContextSource { get; set; }
        public Func<AiUserContext, IReadOnlyList<long>>? BranchSource { get; set; }
        public Func<IReadOnlyDictionary<string, string>>? SettingsSource { get; set; }
        public int? TimeoutMsOverride { get; set; }
        /// <summary>آخر عبارة نُفّذت (للتشخيص والاختبار).</summary>
        public string? LastSql { get; private set; }

        private readonly object _lock = new();
        private long _ctxUser;
        private DateTime _ctxAt;
        private AiToolContext? _ctx;
        private List<long>? _branches;
        private DateTime _settingAt;
        private bool _settingOn = true;

        public AiSearchGateway(ReadOnlySqlExecutor sql, AiSearchCLS? recent = null, AiToolAuditCLS? audit = null)
        {
            _sql = sql;
            _recent = recent ?? new AiSearchCLS();
            _audit = audit ?? new AiToolAuditCLS();
        }

        // ───────────── الصلاحيات (س13) ─────────────

        public List<string> AllowedKinds(AiUserContext user) => AllowedKinds(Context(user));

        public static List<string> AllowedKinds(AiToolContext ctx)
        {
            var entityViews = new HashSet<string>(ctx.AllowedViews, StringComparer.OrdinalIgnoreCase);
            entityViews.UnionWith(AiDraftTools.ImpliedViews(ctx));
            var list = new List<string>();
            foreach (var s in Specs)
            {
                bool view = s.IsDocument ? ctx.AllowedViews.Contains(s.View) : entityViews.Contains(s.View);
                if (view && ctx.HasRoute(s.OpenRoute)) list.Add(s.Kind);
            }
            return list;
        }

        // ───────────── البحث ─────────────

        public async Task<AiSearchResult> SearchAsync(string text, AiUserContext user, CancellationToken ct)
        {
            var result = new AiSearchResult();
            var q = AiSearchText.Parse(text);
            if (q.AskOnly || !q.HasDataText || user == null || user.UserId <= 0) return result;
            if (!DataSearchEnabled()) { result.Disabled = true; return result; }

            var sw = Stopwatch.StartNew();
            var ctx = Context(user);
            var kinds = KindsFor(q, AllowedKinds(ctx));
            if (kinds.Count == 0) return result;

            IReadOnlyList<long> branches = Array.Empty<long>();
            if (kinds.Any(AiSearchKinds.IsDocument))
            {
                branches = Branches(user);
                if (branches.Count == 0)
                {
                    foreach (var k in kinds.Where(AiSearchKinds.IsDocument).ToList()) { kinds.Remove(k); result.SkippedKinds.Add(k); }
                    if (kinds.Count == 0) return result;
                }
            }

            var built = BuildQuery(q, kinds, branches);
            if (built == null) return result;
            var (sql, parameters, views) = built.Value;
            LastSql = sql;

            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeout.CancelAfter(TimeoutMsOverride ?? AiSearchConst.DataTimeoutMs);
            var session = ReadOnlySqlExecutor.SessionFor(ctx.TaxFilter, user.UserId);
            try
            {
                var token = timeout.Token;
                var r = await Task.Run(() =>
                {
                    token.ThrowIfCancellationRequested();
                    var res = _sql.Execute(sql, views, session, parameters, token);
                    // انتهت المهلة أثناء التنفيذ ولم يُلغَ الأمر فعلاً: النتيجة متأخرة فلا تُعرض.
                    token.ThrowIfCancellationRequested();
                    return res;
                }, token).ConfigureAwait(false);
                result.Hits = ToHits(r);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (OperationCanceledException)
            {
                result.TimedOut = true;
                Audit(user, q, sql, sw.ElapsedMilliseconds, "timeout");
            }
            catch (Exception ex)
            {
                operations.DataDictionaryLogerException(ex);
                result.Error = ex is AiSqlRejectedException ? ex.Message : "search failed";
                Audit(user, q, sql, sw.ElapsedMilliseconds, ex.Message);
            }
            sw.Stop();
            result.DurationMs = sw.ElapsedMilliseconds;
            return result;
        }

        /// <summary>الأنواع المسموحة ∩ النطاق (المستندات فقط مع رقم، والكيانات فقط مع كلمة ≥ حرفين أو رقم) — بترتيب المجموعات (س2).</summary>
        public static List<string> KindsFor(AiSearchQuery q, IEnumerable<string> allowed)
        {
            var set = new HashSet<string>(allowed);
            var list = new List<string>();
            foreach (var s in Specs)
            {
                if (!set.Contains(s.Kind)) continue;
                if (s.IsDocument)
                {
                    if (q.DocNumber == null) continue;
                    if (q.Scope != null && q.Scope != AiSearchKinds.DocScope) continue;
                }
                else
                {
                    if (q.Tokens.Count == 0) continue;
                    if (q.Scope != null && q.Scope != s.Kind) continue;
                }
                list.Add(s.Kind);
            }
            return list;
        }

        // ───────────── بناء العبارة (§3) ─────────────

        /// <summary>تطبيع الاسم داخل SQL مطابقاً لـ AiSearchText.Normalize فى الحروف (س9).</summary>
        public static string NX(string col)
        {
            string e = "LOWER(ISNULL(" + col + ", N''))";
            foreach (var (from, to) in SqlFolds) e = "REPLACE(" + e + ", N'" + from + "', N'" + to + "')";
            return e;
        }

        private static readonly (string From, string To)[] SqlFolds =
        {
            ("أ", "ا"), ("إ", "ا"), ("آ", "ا"), ("ٱ", "ا"), ("ة", "ه"), ("ى", "ي"), ("ی", "ي"), ("ک", "ك"), ("ـ", ""),
        };

        /// <summary>
        /// يبنى العبارة الموحّدة والمعاملات والـ Views. null = لا جزء يُبنى. branches فارغة ⇒ لا أجزاء مستندات.
        /// </summary>
        public static (string Sql, Dictionary<string, object> Params, HashSet<string> Views)? BuildQuery(AiSearchQuery q, IEnumerable<string> kinds, IReadOnlyList<long> branches)
        {
            var parts = new List<string>();
            var p = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            var views = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            string data = q.DataText ?? "";
            int n = 0;

            bool entityParams = false, docParams = false;
            foreach (var kind in kinds)
            {
                var s = SpecOf(kind);
                if (s == null) continue;
                n++;
                if (!s.IsDocument)
                {
                    if (q.Tokens.Count == 0) continue;
                    if (!entityParams)
                    {
                        entityParams = true;
                        p["@exact"] = data;
                        p["@qn"] = data;
                        p["@starts"] = AiSearchText.EscapeLike(data) + "%";
                        p["@cstarts"] = AiSearchText.EscapeLike(data) + "%";
                        for (int i = 0; i < q.Tokens.Count && i < AiSearchConst.MaxTokens; i++)
                            p["@t" + i] = "%" + AiSearchText.EscapeLike(q.Tokens[i]) + "%";
                    }
                    string name = NX(s.TitleCol), code = "CAST(" + s.CodeCol + " AS nvarchar(100))";
                    var tokenConds = Enumerable.Range(0, Math.Min(q.Tokens.Count, AiSearchConst.MaxTokens)).Select(i => name + " LIKE @t" + i);
                    parts.Add(
$@"SELECT * FROM (
  SELECT TOP ({AiSearchConst.PerGroup}) N'{s.Kind}' AS kind, {s.IdCol} AS id,
         CAST({s.TitleCol} AS nvarchar(300)) COLLATE DATABASE_DEFAULT AS title,
         {code} COLLATE DATABASE_DEFAULT AS code,
         CAST(NULL AS date) AS doc_date, CAST(NULL AS nvarchar(300)) AS party,
         CAST(NULL AS nvarchar(200)) AS branch, CAST(NULL AS decimal(28,4)) AS amount,
         CASE WHEN {code} = @exact THEN 0
              WHEN {name} = @qn THEN 1
              WHEN {name} LIKE @starts THEN 2
              WHEN {code} LIKE @cstarts THEN 3 ELSE 4 END AS rnk
  FROM ai.{s.View}
  WHERE ({string.Join(" AND ", tokenConds)})
     OR {code} LIKE @cstarts
  ORDER BY rnk, LEN({s.TitleCol}), {s.TitleCol}
) AS x{n}");
                    views.Add(s.View);
                }
                else
                {
                    if (q.DocNumber == null || branches.Count == 0) continue;
                    if (!docParams)
                    {
                        docParams = true;
                        p["@n"] = q.DocNumber.Value;
                        var ranges = AiSearchText.PrefixRanges(q.DocNumber.Value);
                        for (int i = 0; i < ranges.Count; i++) { p["@a" + (i + 1)] = ranges[i].From; p["@b" + (i + 1)] = ranges[i].To; }
                        for (int i = 0; i < branches.Count; i++) p["@br" + i] = branches[i];
                    }
                    int rangeCount = p.Keys.Count(k => k.StartsWith("@a", StringComparison.Ordinal));
                    var numberConds = new List<string> { s.TitleCol + " = @n" };
                    for (int i = 1; i <= rangeCount; i++) numberConds.Add(s.TitleCol + " BETWEEN @a" + i + " AND @b" + i);
                    string brList = string.Join(", ", Enumerable.Range(0, branches.Count).Select(i => "@br" + i));
                    parts.Add(
$@"SELECT * FROM (
  SELECT TOP ({AiSearchConst.PerGroup}) N'{s.Kind}' AS kind, {s.IdCol} AS id,
         CAST({s.TitleCol} AS nvarchar(30)) COLLATE DATABASE_DEFAULT AS title,
         CAST(NULL AS nvarchar(100)) AS code, {s.DateCol} AS doc_date,
         CAST({s.PartyCol} AS nvarchar(300)) COLLATE DATABASE_DEFAULT AS party,
         CAST(branch_name AS nvarchar(200)) COLLATE DATABASE_DEFAULT AS branch,
         CAST(net_total AS decimal(28,4)) AS amount,
         CASE WHEN {s.TitleCol} = @n THEN 0 ELSE 1 END AS rnk
  FROM ai.{s.View}
  WHERE ({string.Join(" OR ", numberConds)})
    AND (branch_id IN ({brList}) OR branch_id IS NULL OR branch_id IN (0, -1))
  ORDER BY rnk, {s.DateCol} DESC, {s.IdCol} DESC
) AS x{n}");
                    views.Add(s.View);
                }
            }
            if (parts.Count == 0) return null;
            return (string.Join("\nUNION ALL\n", parts), p, views);
        }

        /// <summary>الصفوف ⇒ نتائج مرتبة بالمجموعة ثم rnk ثم (الكيانات: طول الاسم ثم الاسم؛ المستندات: التاريخ تنازلياً ثم المعرّف).</summary>
        public static List<AiSearchHit> ToHits(AiQueryResult r)
        {
            int Col(string name) => r.Columns.FindIndex(c => string.Equals(c, name, StringComparison.OrdinalIgnoreCase));
            int cKind = Col("kind"), cId = Col("id"), cTitle = Col("title"), cCode = Col("code"), cDate = Col("doc_date"),
                cParty = Col("party"), cBranch = Col("branch"), cAmount = Col("amount"), cRnk = Col("rnk");
            var rows = new List<(int Group, int Rnk, AiSearchHit Hit)>();
            foreach (var row in r.Rows)
            {
                string kind = Convert.ToString(row[cKind], CultureInfo.InvariantCulture) ?? "";
                var spec = SpecOf(kind);
                if (spec == null || row[cId] == null) continue;
                long id = Convert.ToInt64(row[cId], CultureInfo.InvariantCulture);
                if (id <= 0) continue;
                string? url = UrlFor(spec, id);
                if (url == null) continue;
                var hit = new AiSearchHit
                {
                    Kind = kind,
                    Id = id,
                    Title = (Convert.ToString(row[cTitle], CultureInfo.InvariantCulture) ?? "").Trim(),
                    Code = Str(row[cCode]),
                    Date = row[cDate] is DateTime d ? d : null,
                    Party = Str(row[cParty]),
                    Branch = Str(row[cBranch]),
                    Amount = row[cAmount] == null ? null : Convert.ToDecimal(row[cAmount], CultureInfo.InvariantCulture),
                    Url = url,
                    RefKey = id.ToString(CultureInfo.InvariantCulture),
                };
                rows.Add((Array.IndexOf(Specs, spec), row[cRnk] == null ? 9 : Convert.ToInt32(row[cRnk], CultureInfo.InvariantCulture), hit));
            }
            return rows
                .OrderBy(x => x.Group).ThenBy(x => x.Rnk)
                .ThenBy(x => Specs[x.Group].IsDocument ? 0 : x.Hit.Title.Length)
                .ThenByDescending(x => x.Hit.Date ?? DateTime.MinValue)
                .ThenBy(x => Specs[x.Group].IsDocument ? "" : x.Hit.Title, StringComparer.Ordinal)
                .ThenByDescending(x => x.Hit.Id)
                .Select(x => x.Hit).ToList();
        }

        /// <summary>رابط الفتح من كتالوج الشاشات (س17) — لا نص حر فى الرابط.</summary>
        public static string? UrlFor(KindSpec spec, long id)
        {
            var screen = AiScreenCatalog.Find(spec.ScreenKey);
            if (screen == null) return null;
            string url = AiScreenCatalog.Build(screen, new JsonObject { [AiScreenCatalog.PId] = id }).Url;
            return AiSearchText.IsSafeInternalUrl(url) ? url : null;
        }

        private static string? Str(object? v)
        {
            string? s = v == null ? null : Convert.ToString(v, CultureInfo.InvariantCulture)?.Trim();
            return string.IsNullOrEmpty(s) ? null : s;
        }

        // ───────────── «الأخيرة» (س19–س21) ─────────────

        /// <summary>آخر ما فتحه المستخدم، بعد إسقاط أنواع البيانات التى لم يعد يملكها. الشاشات يعيد فحصها مستدعى الواجهة (قائمة المستخدم).</summary>
        public List<AiSearchRecentRow> GetRecent(AiUserContext user, int top)
        {
            if (user == null || user.UserId <= 0) return new();
            top = Math.Clamp(top, 1, AiSearchConst.RecentLoad);
            var allowed = new HashSet<string>(AllowedKinds(user));
            return _recent.List(user.UserId, top)
                .Where(r => AiSearchText.IsSafeInternalUrl(r.Url) && (r.Kind == AiSearchKinds.Screen || allowed.Contains(r.Kind)))
                .ToList();
        }

        public void Touch(string kind, string refKey, string title, string subtitle, string url, AiUserContext user)
        {
            if (user == null || user.UserId <= 0) throw new ArgumentException("user");
            if (!AiSearchKinds.IsRecentKind(kind)) throw new ArgumentException("kind");
            if (!AiSearchText.IsSafeInternalUrl(url)) throw new ArgumentException("url");
            _recent.Touch(user.UserId, kind, refKey, title, subtitle, url);
        }

        public void ClearRecent(long userId)
        {
            if (userId <= 0) throw new ArgumentException("userId");
            _recent.HideAll(userId);
        }

        // ───────────── الذاكرة والإعداد ─────────────

        private AiToolContext Context(AiUserContext user)
        {
            if (ContextSource != null) return ContextSource(user);
            lock (_lock)
            {
                if (_ctx != null && _ctxUser == user.UserId && DateTime.UtcNow - _ctxAt < CtxTtl) return _ctx;
            }
            var ctx = AiChatGateway.BuildToolContext(user, CancellationToken.None);
            lock (_lock)
            {
                if (_ctxUser != user.UserId) _branches = null;
                _ctx = ctx; _ctxUser = user.UserId; _ctxAt = DateTime.UtcNow;
            }
            return ctx;
        }

        private IReadOnlyList<long> Branches(AiUserContext user)
        {
            if (BranchSource != null) return BranchSource(user);
            lock (_lock)
            {
                if (_branches != null && _ctxUser == user.UserId && DateTime.UtcNow - _ctxAt < CtxTtl) return _branches;
            }
            List<long> list;
            try { list = new FinalAccountsService().GetBranches().Select(b => b.Id).Distinct().Take(200).ToList(); }
            catch (Exception ex) { operations.DataDictionaryLogerException(ex); list = new List<long>(); }
            lock (_lock)
            {
                if (_ctxUser == user.UserId) _branches = list;
            }
            return list;
        }

        private bool DataSearchEnabled()
        {
            if (SettingsSource != null) return Enabled(SettingsSource());
            lock (_lock)
            {
                if (_settingAt != default && DateTime.UtcNow - _settingAt < SettingTtl) return _settingOn;
            }
            bool on;
            try { on = Enabled(new AiProvidersCLS().GetSettings()); }
            catch (Exception ex) { operations.DataDictionaryLogerException(ex); on = true; }
            lock (_lock) { _settingOn = on; _settingAt = DateTime.UtcNow; }
            return on;
        }

        private static bool Enabled(IReadOnlyDictionary<string, string> s) =>
            !s.TryGetValue(AiSettingKeys.PaletteDataSearch, out var v) || (v ?? "").Trim() == "1";

        /// <summary>س22: صف تدقيق للخطأ والمهلة فقط.</summary>
        private void Audit(AiUserContext user, AiSearchQuery q, string sql, long ms, string error)
        {
            try
            {
                string args = JsonSerializer.Serialize(new { text = q.Raw, scope = q.Scope, doc = q.DocNumber },
                    new JsonSerializerOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
                _audit.Insert(null, user.UserId, AiSearchConst.AuditTool, args, sql, null, (int)Math.Min(ms, int.MaxValue), AiToolOutcome.Error, error);
            }
            catch (Exception ex) { operations.DataDictionaryLogerException(ex); }
        }
    }
}
