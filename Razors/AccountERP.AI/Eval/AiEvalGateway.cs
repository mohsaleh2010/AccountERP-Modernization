using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using AccountERP.AI.Chat;
using AccountERP.AI.SignIn;
using AccountERP.AI.Tools;
using AccountModelsLib;

namespace AccountERP.AI.Eval
{
    /// <summary>
    /// P7.4 (D17، D18): بوابة التقييم. كل ما يكتب أو يصرف رصيداً للمدير فقط (صلاحية شاشة /settings/ai)، والتشغيل يُرفض بلا Confirmed.
    /// نموذج أو نموذجان بنفس Group_Id ونفس لقطة الحالات؛ النموذج يُمرَّر صريحاً (لا يُقرأ ربط المهام ولا يُعدَّل).
    /// </summary>
    public sealed class AiEvalGateway : IAiEvalGateway
    {
        private const int DefaultMaxTokens = 8000;
        private const int DefaultMaxCases = 100;
        private readonly AiEvalRunner _runner;
        private readonly AiToolRegistry _tools;
        private readonly AiProvidersCLS _providers = new();
        private readonly AiEvalCLS _db;

        public AiEvalGateway(AiEvalRunner runner, AiToolRegistry tools) : this(runner, tools, null) { }

        internal AiEvalGateway(AiEvalRunner runner, AiToolRegistry tools, AiEvalCLS? db)
        {
            _runner = runner;
            _tools = tools;
            _db = db ?? new AiEvalCLS();
        }

        private void EnsureAdmin(long userId)
        {
            if (userId <= 0 || !_providers.UserHasMenu(userId, AiSignInService.MenuKey))
                throw new InvalidOperationException("التقييم متاح لمن يملك صلاحية شاشة «إعدادات الذكاء الاصطناعى» فقط.");
        }

        // ───────────── الحالات ─────────────

        public List<AiEvalCaseRow> GetCases()
        {
            _runner.EnsureReady();
            return _db.ListCases();
        }

        public long SaveCase(AiEvalCaseSaveRequest r, long userId)
        {
            EnsureAdmin(userId);
            _runner.EnsureReady();
            var row = Validate(r);
            if (r.Id > 0)
            {
                var cur = _db.GetCase(r.Id) ?? throw new InvalidOperationException("الحالة غير موجودة.");
                row.Id = cur.Id;
                row.CaseKey = cur.CaseKey;
                row.Source = cur.Source;
                row.Source_Ref_Id = cur.Source_Ref_Id;
                // نسخة جديدة فقط إن تغيّر ما يُحكم به (المقارنة بين التشغيلات على CaseKey + CaseVersion).
                bool changed = cur.Question != row.Question || cur.CheckType != row.CheckType || (cur.ExpectedTools ?? "") != (row.ExpectedTools ?? "") ||
                               (cur.ForbiddenTools ?? "") != (row.ForbiddenTools ?? "") || (cur.ReferenceSql ?? "") != (row.ReferenceSql ?? "") ||
                               (cur.ReferenceArgsJson ?? "") != (row.ReferenceArgsJson ?? "") || (cur.ExpectedText ?? "") != (row.ExpectedText ?? "");
                row.Version = changed ? cur.Version + 1 : cur.Version;
                _db.UpdateByAdmin(row, userId);
                return row.Id;
            }
            row.Id = CallSnowflakeIdGenerate.GenerateNewId();
            row.Source = r.Source == "feedback" ? "feedback" : "manual";
            row.Source_Ref_Id = r.Source_Ref_Id;
            row.CaseKey = row.Source + "_" + row.Id.ToString(CultureInfo.InvariantCulture);
            row.Version = 1;
            _db.InsertCase(row, userId);
            return row.Id;
        }

        public void SetCaseActive(long id, bool active, long userId)
        {
            EnsureAdmin(userId);
            if (_db.GetCase(id) == null) throw new InvalidOperationException("الحالة غير موجودة.");
            _db.SetCaseActive(id, active, userId);
        }

        /// <summary>يتحقق من الحالة حسب نوع حكمها: الاستعلام المرجعى قراءة فقط على Views المساعد، والأدوات أسماء معروفة.</summary>
        internal static AiEvalCaseRow Validate(AiEvalCaseSaveRequest r)
        {
            string q = (r.Question ?? "").Trim();
            if (q.Length == 0) throw new InvalidOperationException("اكتب السؤال.");
            if (q.Length > 2000) throw new InvalidOperationException("السؤال أطول من 2000 حرف.");
            string cat = (r.Category ?? "").Trim().ToLowerInvariant();
            if (!AiEvalCategories.All.Contains(cat)) throw new InvalidOperationException("التصنيف غير معروف: " + r.Category);
            string check = (r.CheckType ?? "").Trim().ToLowerInvariant();
            if (!AiEvalCheckTypes.All.Contains(check)) throw new InvalidOperationException("نوع الحكم غير معروف: " + r.CheckType);
            string dialect = (r.Dialect ?? "").Trim().ToLowerInvariant() is "eg" ? "eg" : "msa";
            var expected = AiEvalJudge.Csv(r.ExpectedTools);
            var forbidden = AiEvalJudge.Csv(r.ForbiddenTools);
            foreach (var t in expected.Concat(forbidden))
                if (AiToolRegistry.TitleOf(t) == t) throw new InvalidOperationException("أداة غير معروفة: " + t);
            string sql = (r.ReferenceSql ?? "").Trim();
            string args = (r.ReferenceArgsJson ?? "").Trim();
            switch (check)
            {
                case AiEvalCheckTypes.Result:
                    if (sql.Length == 0) throw new InvalidOperationException("حالة «نتيجة» تحتاج استعلاماً مرجعياً.");
                    try { ReadOnlySqlExecutor.Validate(sql, new HashSet<string>(AiSchemaCatalog.Views.Select(v => v.Name), StringComparer.OrdinalIgnoreCase)); }
                    catch (AiSqlRejectedException ex) { throw new InvalidOperationException("الاستعلام المرجعى مرفوض: " + ex.Message); }
                    break;
                case AiEvalCheckTypes.Financial:
                    if (expected.Count != 1 || expected[0] is not ("get_trial_balance" or "get_financial_statement"))
                        throw new InvalidOperationException("حالة «مالية» تحتاج أداة واحدة: get_trial_balance أو get_financial_statement.");
                    try
                    {
                        if (JsonNode.Parse(AiEvalRunner.ResolveDates(args.Length == 0 ? "x" : args, DateTime.Today)) is not JsonObject)
                            throw new JsonException();
                    }
                    catch (JsonException) { throw new InvalidOperationException("المدخلات المرجعية يجب أن تكون كائن JSON (مثل {\"from\":\"{month_start}\",\"to\":\"{today}\"})."); }
                    break;
                case AiEvalCheckTypes.Tool:
                    if (expected.Count == 0) throw new InvalidOperationException("حالة «أداة» تحتاج أداة متوقعة واحدة على الأقل.");
                    break;
            }
            string? N(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
            return new AiEvalCaseRow
            {
                Question = q, Category = cat, Dialect = dialect, CheckType = check,
                ExpectedTools = expected.Count == 0 ? null : string.Join(",", expected),
                ForbiddenTools = forbidden.Count == 0 ? null : string.Join(",", forbidden),
                ReferenceSql = check == AiEvalCheckTypes.Result ? sql : null,
                ReferenceArgsJson = check == AiEvalCheckTypes.Financial ? args : null,
                ExpectedText = N(r.ExpectedText), Tags = N(r.Tags), Notes = N(r.Notes), IsActive = true,
            };
        }

        // ───────────── التشغيل ─────────────

        private sealed record Prepared(List<AiEvalCaseRow> Cases, List<(AiModelRow Model, AiProviderRow Provider, AiTaskRoutingRow Route)> Models,
                                       int MaxCases, decimal MaxCost, int MaxRounds, string Blocked, int AutoExamples);

        private Prepared Prepare(AiEvalStartRequest r)
        {
            r ??= new AiEvalStartRequest();
            var settings = _providers.GetSettings();
            int maxCases = settings.TryGetValue(AiSettingKeys.EvalMaxCasesPerRun, out var mc) && int.TryParse(mc, out int n) && n > 0 ? n : DefaultMaxCases;
            decimal maxCost = settings.TryGetValue(AiSettingKeys.EvalMaxCostPerRun, out var mx) &&
                              decimal.TryParse(mx, NumberStyles.Number, CultureInfo.InvariantCulture, out var d) && d > 0 ? d : 0;
            int maxRounds = settings.TryGetValue(AiSettingKeys.MaxToolRoundsPerMessage, out var mr) && int.TryParse(mr, out int rr) && rr is >= 1 and <= 30 ? rr : 12;
            bool localOnly = settings.TryGetValue(AiSettingKeys.LocalOnly, out var lo) && lo == "1";
            // P7.7: الطلب يحدد (0 = بدونها) وإلا الإعداد الحالى.
            int autoEx = r.AutoExamples is int ax ? Math.Clamp(ax, 0, AiSettingKeys.MaxAutoExamples) : AiChatGateway.AutoExamples(settings);

            var ids = (r.ModelIds ?? new List<long>()).Where(i => i > 0).Distinct().ToList();
            if (ids.Count is < 1 or > 2) throw new InvalidOperationException("اختر نموذجاً واحداً أو نموذجين للمقارنة.");
            var models = _providers.GetModels().ToDictionary(m => m.Id);
            var providers = _providers.GetProviders().ToDictionary(p => p.Id);
            var routing = _providers.GetRouting().Where(x => x.TaskKey == AiTaskKeys.Chat).OrderBy(x => x.Priority).ToList();
            var list = new List<(AiModelRow, AiProviderRow, AiTaskRoutingRow)>();
            foreach (long id in ids)
            {
                if (!models.TryGetValue(id, out var m)) throw new InvalidOperationException("النموذج غير موجود (ربما حُذف).");
                if (!m.IsEnabled) throw new InvalidOperationException($"النموذج «{m.DisplayName}» معطّل.");
                if (!providers.TryGetValue(m.Provider_Id, out var p) || !p.IsEnabled) throw new InvalidOperationException($"مزوّد النموذج «{m.DisplayName}» معطّل أو محذوف.");
                if (localOnly && !p.IsLocal) throw new InvalidOperationException($"وضع «محلى فقط» مفعّل والنموذج «{m.DisplayName}» سحابى.");
                if (!m.SupportsTools) throw new InvalidOperationException($"النموذج «{m.DisplayName}» لا يدعم الأدوات؛ التقييم يقيس استخدام الأدوات.");
                // الإعدادات: من الطلب، وإلا من ربط «محادثة» لنفس النموذج إن وُجد. صف ربط افتراضى فى الذاكرة فقط (لا يُحفظ).
                var chat = routing.FirstOrDefault(x => x.Model_Id == id);
                var route = new AiTaskRoutingRow
                {
                    TaskKey = AiTaskKeys.Chat, Model_Id = id, Priority = 0,
                    Effort = !string.IsNullOrWhiteSpace(r.Effort) ? r.Effort.Trim() : chat?.Effort ?? "",
                    MaxTokens = r.MaxTokens is > 0 ? r.MaxTokens : chat?.MaxTokens is > 0 ? chat.MaxTokens : DefaultMaxTokens,
                    Temperature = chat?.Temperature,
                };
                list.Add((m, p, route));
            }
            var cases = SelectCases(_db.ListCases(), r.Filter ?? new AiEvalFilter());
            string blocked = cases.Count == 0 ? "لا توجد حالات مفعّلة تطابق الاختيار."
                : cases.Count > maxCases ? $"عدد الحالات ({cases.Count}) أكبر من الحد ({maxCases}) لكل تشغيل. ضيّق الاختيار."
                : "";
            return new Prepared(cases, list, maxCases, maxCost, maxRounds, blocked, autoEx);
        }

        /// <summary>المفعّلة فقط، ثم الفلاتر، بترتيب ثابت (التصنيف ثم المفتاح) فيأخذ النموذجان نفس الحالات.</summary>
        internal static List<AiEvalCaseRow> SelectCases(IEnumerable<AiEvalCaseRow> all, AiEvalFilter f)
        {
            var q = all.Where(c => c.IsActive);
            if (f.CaseIds?.Count > 0) q = q.Where(c => f.CaseIds.Contains(c.Id));
            if (f.Categories?.Count > 0) q = q.Where(c => f.Categories.Contains(c.Category, StringComparer.OrdinalIgnoreCase));
            if (f.CheckTypes?.Count > 0) q = q.Where(c => f.CheckTypes.Contains(c.CheckType, StringComparer.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(f.Dialect)) q = q.Where(c => string.Equals(c.Dialect, f.Dialect.Trim(), StringComparison.OrdinalIgnoreCase));
            var list = q.OrderBy(c => Array.IndexOf(AiEvalCategories.All, c.Category) is int i && i >= 0 ? i : 99).ThenBy(c => c.CaseKey, StringComparer.Ordinal).ToList();
            return f.Limit is > 0 ? list.Take(f.Limit.Value).ToList() : list;
        }

        public AiEvalEstimate Estimate(AiEvalStartRequest r, AiUserContext user)
        {
            EnsureAdmin(user?.UserId ?? 0);
            _runner.EnsureReady();
            var p = Prepare(r);
            return EstimateOf(p);
        }

        private AiEvalEstimate EstimateOf(Prepared p)
        {
            var e = new AiEvalEstimate { CaseCount = p.Cases.Count, MaxCases = p.MaxCases, MaxCostPerRun = p.MaxCost, Blocked = p.Blocked, AutoExamples = p.AutoExamples };
            bool allPriced = true;
            decimal total = 0;
            foreach (var (m, _, _) in p.Models)
            {
                (double In, double Out)? hist = null;
                try { hist = _db.AvgTokensPerCase(m.Id); } catch (Exception ex) { operations.DataDictionaryLogerException(ex); }
                long inTok = (long)Math.Ceiling((hist?.In ?? DefaultIn) * p.Cases.Count), outTok = (long)Math.Ceiling((hist?.Out ?? DefaultOut) * p.Cases.Count);
                decimal? cost = m.InputPricePerM == null && m.OutputPricePerM == null ? null
                    : Math.Round((inTok * (m.InputPricePerM ?? 0) + outTok * (m.OutputPricePerM ?? 0)) / 1_000_000m, 4);
                e.Models.Add(new AiEvalModelEstimate { ModelId = m.Id, ModelName = m.DisplayName, InputTokens = inTok, OutputTokens = outTok, Cost = cost, Basis = hist != null ? "history" : "default" });
                if (cost == null) allPriced = false; else total += cost.Value;
                if (e.Blocked.Length == 0 && p.MaxCost > 0 && cost > p.MaxCost)
                    e.Blocked = $"التكلفة التقديرية لـ «{m.DisplayName}» ({cost.Value.ToString("0.##", CultureInfo.InvariantCulture)}) أكبر من حد التكلفة لكل تشغيل ({p.MaxCost.ToString("0.##", CultureInfo.InvariantCulture)}). قلّل عدد الحالات.";
            }
            e.TotalCost = allPriced ? total : null;
            return e;
        }

        private const double DefaultIn = AiEvalRunner.DefaultInTokensPerCase, DefaultOut = AiEvalRunner.DefaultOutTokensPerCase;

        public AiEvalStartResult Start(AiEvalStartRequest r, AiUserContext user)
        {
            long userId = user?.UserId ?? 0;
            EnsureAdmin(userId);
            if (r == null || !r.Confirmed) throw new InvalidOperationException("وافق أولاً على عدد الحالات والتكلفة التقديرية (التقييم يستهلك رصيداً حقيقياً).");
            _runner.EnsureReady();
            if (_runner.ActiveGroupId() != null) throw new InvalidOperationException("يوجد تقييم يعمل الآن؛ انتظر انتهاءه أو أوقفه أولاً.");
            var p = Prepare(r);
            var est = EstimateOf(p);
            if (est.Blocked.Length > 0) throw new InvalidOperationException(est.Blocked);

            // نفس صلاحيات المحادثة لمن أطلق التقييم (Views + فلتر الفواتير الضريبية)، تُحسب الآن فى سياق جلسته.
            var baseCtx = AiChatGateway.BuildToolContext(user!, CancellationToken.None);
            var evalCtx = new AiToolContext { User = user!, AllowedViews = baseCtx.AllowedViews, TaxFilter = baseCtx.TaxFilter, IsEval = true };
            string promptHash = PromptHash(evalCtx);
            long groupId = CallSnowflakeIdGenerate.GenerateNewId();
            var config = new JsonObject
            {
                ["case_ids"] = new JsonArray(p.Cases.Select(c => (JsonNode)JsonValue.Create(c.Id.ToString(CultureInfo.InvariantCulture))!).ToArray()),
                ["case_versions"] = new JsonObject(p.Cases.Select(c => KeyValuePair.Create(c.CaseKey, (JsonNode?)JsonValue.Create(c.Version)))),
                ["max_rounds"] = p.MaxRounds,
                ["max_cost"] = p.MaxCost,
                ["tax_filter"] = baseCtx.TaxFilter,
                ["is_admin"] = user!.IsAdmin,
                ["views"] = baseCtx.AllowedViews.Count,
                ["knowledge_auto_examples"] = p.AutoExamples,
                ["estimate"] = est.TotalCost,
            }.ToJsonString(Json);
            string filter = JsonSerializer.Serialize(r.Filter ?? new AiEvalFilter(), Json);
            var runs = p.Models.Select(x => new AiEvalRunner.RunPlan
            {
                Candidate = new AiChatGateway.Candidate(x.Route, x.Model, x.Provider),
                Run = new AiEvalRunRow
                {
                    Id = CallSnowflakeIdGenerate.GenerateNewId(), Group_Id = groupId, User_Id = userId, Model_Id = x.Model.Id,
                    ModelName = x.Model.DisplayName, ProviderName = x.Provider.DisplayName, Effort = x.Route.Effort, MaxTokens = x.Route.MaxTokens,
                    Status = AiEvalRunStatus.Running, FilterJson = filter, ConfigJson = config, PromptHash = promptHash, TotalCases = p.Cases.Count,
                    StartedAt = DateTime.Now,
                },
            }).ToList();
            var plan = new AiEvalRunner.GroupPlan
            {
                GroupId = groupId, User = user, AllowedViews = baseCtx.AllowedViews, TaxFilter = baseCtx.TaxFilter,
                Cases = p.Cases, Runs = runs, MaxRounds = p.MaxRounds, MaxCost = p.MaxCost, AutoExamples = p.AutoExamples,
            };
            if (!_runner.TryStart(plan, out string reason)) throw new InvalidOperationException(reason);
            return new AiEvalStartResult { GroupId = groupId, RunIds = runs.Select(x => x.Run.Id).ToList(), CaseCount = p.Cases.Count };
        }

        /// <summary>D18: بصمة التعليمات والأدوات (بلا سطر التاريخ) — اختلافها بين تشغيلين يُنبَّه عليه عند المقارنة.</summary>
        internal string PromptHash(AiToolContext ctx)
        {
            var tools = _tools.ToolsFor(ctx);
            string system = AiChatGateway.SystemPrompt(null, tools.Count > 0 ? ctx : null);
            system = string.Join("\n", system.Split('\n').Where(l => !l.StartsWith("تاريخ اليوم", StringComparison.Ordinal)));
            var sb = new StringBuilder(system);
            foreach (var t in tools) sb.Append('\n').Append(t.Name).Append('|').Append(t.Description).Append('|').Append(t.Schema.ToJsonString());
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(sb.ToString()))).ToLowerInvariant();
        }

        public void Cancel(long groupId, long userId)
        {
            EnsureAdmin(userId);
            if (!_runner.Cancel(groupId)) throw new InvalidOperationException("هذا التقييم لا يعمل الآن.");
        }

        public long? ActiveGroupId() => _runner.ActiveGroupId();

        public AiEvalProgress GetProgress(long groupId)
        {
            bool active = _runner.ActiveGroupId() == groupId;
            return new AiEvalProgress { GroupId = groupId, Active = active, CurrentCase = active ? _runner.CurrentCase(groupId) : "", Runs = _db.RunsOfGroup(groupId) };
        }

        public List<AiEvalRunRow> GetRuns(int top = 100)
        {
            _runner.EnsureReady();
            return _db.ListRuns(top);
        }

        public List<AiEvalResultRow> GetResults(long runId)
        {
            var list = _db.ListResults(runId);
            var j = _db.Judgements(runId);   // بقايا ب3
            foreach (var r in list)
                if (j.TryGetValue(r.Id, out var v)) { r.JudgeScore = v.Score; r.JudgeReason = v.Reason; }
            return list;
        }

        // ───────────── بقايا ب2، ب3 ─────────────

        private static readonly HashSet<string> Flags = new(new[] { AiSettingKeys.KnowledgeScreenChunks, AiSettingKeys.EvalLlmJudge });

        public bool GetFlag(string key) => Flags.Contains(key) && _providers.GetSettings().TryGetValue(key, out var v) && v == "1";

        public void SetFlag(string key, bool on, long userId)
        {
            if (!Flags.Contains(key)) throw new InvalidOperationException("إعداد غير معروف.");
            if (userId <= 0 || !_providers.UserHasMenu(userId, AiSignInService.MenuKey)) throw new UnauthorizedAccessException("للمدير فقط.");
            _providers.SetSetting(key, on ? "1" : "0", userId);
        }

        // ───────────── P7.7: الأمثلة التلقائية ─────────────

        public AiAutoExamplesStatus GetAutoExamplesStatus(int? count = null)
        {
            int current = AiChatGateway.AutoExamples(_providers.GetSettings());
            int n = Math.Clamp(count ?? (current > 0 ? current : 3), 1, AiSettingKeys.MaxAutoExamples);
            var st = new AiAutoExamplesStatus { Current = current, Count = n };
            try { st.ActiveExamples = _tools.KnowledgeService.ActiveExampleCount(); }
            catch (Exception ex) { operations.DataDictionaryLogerException(ex); }

            var done = _db.ListRuns(500).Where(r => r.Status == AiEvalRunStatus.Done && r.DoneCases > 0).ToList();   // الأحدث أولاً
            var on = done.FirstOrDefault(r => AiEvalConfig.AutoExamplesOf(r.ConfigJson) == n);
            if (on == null) return st;
            var off = done.FirstOrDefault(r => r.Model_Id == on.Model_Id && AiEvalConfig.AutoExamplesOf(r.ConfigJson) == 0);
            st.ModelName = on.ModelName;
            st.RunOnId = on.Id;
            st.OnAt = on.StartedAt;
            if (off == null) return st;
            st.RunOffId = off.Id;
            st.OffAt = off.StartedAt;
            var (common, passOn, passOff) = PairedPasses(_db.ListResults(on.Id), _db.ListResults(off.Id));
            st.CommonCases = common;
            st.PassOn = passOn;
            st.PassOff = passOff;
            st.HasEvidence = common >= AiAutoExamplesStatus.MinCommonCases;
            st.Improved = st.HasEvidence && passOn > passOff;
            return st;
        }

        /// <summary>P7.7 (D18): مقارنة مزدوجة على الحالات المشتركة بنفس المفتاح والإصدار (المتخطّاة لا تُحسب).</summary>
        internal static (int Common, int PassOn, int PassOff) PairedPasses(IEnumerable<AiEvalResultRow> on, IEnumerable<AiEvalResultRow> off)
        {
            var a = on.Where(x => x.Outcome != AiEvalOutcomes.Skipped).GroupBy(x => x.CaseKey).ToDictionary(g => g.Key, g => g.First());
            int common = 0, pOn = 0, pOff = 0;
            foreach (var y in off.Where(x => x.Outcome != AiEvalOutcomes.Skipped).GroupBy(x => x.CaseKey).Select(g => g.First()))
            {
                if (!a.TryGetValue(y.CaseKey, out var x) || x.CaseVersion != y.CaseVersion) continue;
                common++;
                if (x.Outcome == AiEvalOutcomes.Pass) pOn++;
                if (y.Outcome == AiEvalOutcomes.Pass) pOff++;
            }
            return (common, pOn, pOff);
        }

        public void SetAutoExamples(int count, bool overrideEvidence, long userId)
        {
            EnsureAdmin(userId);
            if (count < 0 || count > AiSettingKeys.MaxAutoExamples) throw new InvalidOperationException($"عدد الأمثلة من 0 إلى {AiSettingKeys.MaxAutoExamples}.");
            if (count > 0 && !overrideEvidence)
            {
                var st = GetAutoExamplesStatus(count);
                if (!st.Improved)
                    throw new InvalidOperationException(!st.HasEvidence
                        ? $"لا يوجد دليل من التقييم بعد: شغّل التقييم لنفس النموذج مرة بدون أمثلة ومرة بـ {count} أمثلة (على {AiAutoExamplesStatus.MinCommonCases} حالات مشتركة على الأقل)، أو أكّد التفعيل رغم ذلك."
                        : $"آخر مقارنة لم تُظهر تحسّناً ({st.PassOn} مقابل {st.PassOff} من {st.CommonCases}). أكّد التفعيل رغم ذلك إن أردت.");
            }
            _providers.SetSetting(AiSettingKeys.KnowledgeAutoExamples, count.ToString(CultureInfo.InvariantCulture), userId);
        }

        private static readonly JsonSerializerOptions Json = new() { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
    }
}
