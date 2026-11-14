using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using AccountERP.AI.Chat;
using AccountERP.AI.Providers;
using AccountERP.AI.Tools;
using AccountModelsLib;

namespace AccountERP.AI.Eval
{
    /// <summary>
    /// P7.4 (D14–D16): مشغّل التقييم فى الخلفية (Singleton). تشغيل جماعى واحد فى المرة؛ الحالات بالتتابع، وكل حالة تمر على النموذجين (إن كانا اثنين)
    /// قبل التالية فيبقى الجزئى صالحاً للمقارنة عند الإيقاف. كل حالة تُحفظ فور الحكم عليها؛ لا يكتب أى محادثة أو رسالة أو مسودة أو تقرير، ولا يغيّر الربط.
    /// المرجع يُحسب حياً مرة لكل حالة بنفس صلاحيات من أطلق التقييم. التشغيلات التى بقيت Running من عملية سابقة ⇒ Interrupted.
    /// </summary>
    public sealed class AiEvalRunner
    {
        public const int CaseTimeoutSeconds = 240;
        public const string ReferenceToolName = "eval_reference";
        /// <summary>تقدير التوكنز لكل حالة بلا تاريخ سابق للنموذج (تعليمات + أدوات + دورتان أو ثلاث).</summary>
        public const long DefaultInTokensPerCase = 18000, DefaultOutTokensPerCase = 900;

        private readonly ProviderRegistry _registry;
        private readonly AiToolRegistry _tools;
        private readonly AiEvalCLS _db;
        private readonly AiToolAuditCLS _audit = new();
        private readonly object _lock = new();
        private ActiveGroup? _active;
        private bool _staleChecked;
        private DateTime _seedSyncedUtc = DateTime.MinValue;
        private bool _seedOk;

        /// <summary>للاختبار: مهلة الحالة وانتظار إعادة المحاولة بعد 429.</summary>
        internal TimeSpan CaseTimeout { get; set; } = TimeSpan.FromSeconds(CaseTimeoutSeconds);
        internal TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(5);

        public AiEvalRunner(ProviderRegistry registry, AiToolRegistry tools) : this(registry, tools, null) { }

        internal AiEvalRunner(ProviderRegistry registry, AiToolRegistry tools, AiEvalCLS? db)
        {
            _registry = registry;
            _tools = tools;
            _db = db ?? new AiEvalCLS();
        }

        internal sealed class RunPlan
        {
            public required AiEvalRunRow Run { get; init; }
            public required AiChatGateway.Candidate Candidate { get; init; }
        }

        internal sealed class GroupPlan
        {
            public long GroupId { get; init; }
            public required AiUserContext User { get; init; }
            public required HashSet<string> AllowedViews { get; init; }
            public int TaxFilter { get; init; }
            public required List<AiEvalCaseRow> Cases { get; init; }
            public required List<RunPlan> Runs { get; init; }
            public int MaxRounds { get; init; } = 12;
            /// <summary>0 = بلا حد.</summary>
            public decimal MaxCost { get; init; }
            /// <summary>P7.7: عدد الأمثلة التلقائية لكل حالة (0 = بدونها).</summary>
            public int AutoExamples { get; init; }
        }

        private sealed class ActiveGroup
        {
            public long GroupId { get; init; }
            public long UserId { get; init; }
            public required CancellationTokenSource Cts { get; init; }
            public required List<long> RunIds { get; init; }
            public volatile string Current = "";
            public Task? Task { get; set; }
        }

        public long? ActiveGroupId() { lock (_lock) return _active?.GroupId; }

        public string CurrentCase(long groupId) { lock (_lock) return _active?.GroupId == groupId ? _active.Current : ""; }

        /// <summary>للاختبار: انتظار انتهاء التشغيل الجارى.</summary>
        internal Task WaitAsync(long groupId)
        {
            lock (_lock) return _active?.GroupId == groupId && _active.Task != null ? _active.Task : Task.CompletedTask;
        }

        /// <summary>مرة لكل عملية: التشغيلات التى بقيت Running (الخادم أُعيد تشغيله أثناءها) ⇒ Interrupted، ثم مزامنة الحالات المزروعة.</summary>
        public void EnsureReady()
        {
            lock (_lock)
            {
                if (!_staleChecked)
                {
                    try { _db.MarkInterrupted(_active?.RunIds ?? new List<long>()); _staleChecked = true; }
                    catch (Exception ex) { operations.DataDictionaryLogerException(ex); }
                }
            }
            SyncSeed();
        }

        /// <summary>
        /// الحالات المزروعة: إدراج الناقص بمفتاحه، وتحديث المزروع غير المعدّل إن رُفعت نسخته فى الكود. لا يحذف ولا يمس الحالات اليدوية أو المعدّلة.
        /// مرة لكل عملية (وإعادة المحاولة بعد دقيقة إن فشلت).
        /// </summary>
        public void SyncSeed(bool force = false)
        {
            lock (_lock)
            {
                if (!force && (_seedOk || DateTime.UtcNow - _seedSyncedUtc < TimeSpan.FromMinutes(1))) return;
                _seedSyncedUtc = DateTime.UtcNow;
                try
                {
                    var existing = _db.ListCases().ToDictionary(c => c.CaseKey, StringComparer.OrdinalIgnoreCase);
                    foreach (var s in AiEvalSeed.Cases)
                    {
                        var row = AiEvalSeed.ToRow(s);
                        if (!existing.TryGetValue(s.Key, out var cur)) _db.InsertCase(row, null);
                        else if (cur.Source == "seed" && !cur.IsEdited && cur.Version < s.Version) { row.Id = cur.Id; _db.UpdateFromSeed(row); }
                    }
                    _seedOk = true;
                }
                catch (Exception ex) { operations.DataDictionaryLogerException(ex); }
            }
        }

        /// <summary>يبدأ التشغيل الجماعى فى الخلفية، أو يعيد سبب الرفض (تشغيل آخر جارٍ).</summary>
        internal bool TryStart(GroupPlan plan, out string reason)
        {
            reason = "";
            lock (_lock)
            {
                if (_active != null) { reason = "يوجد تقييم يعمل الآن؛ انتظر انتهاءه أو أوقفه أولاً."; return false; }
                foreach (var rp in plan.Runs) _db.InsertRun(rp.Run);
                var active = new ActiveGroup
                {
                    GroupId = plan.GroupId, UserId = plan.User.UserId, Cts = new CancellationTokenSource(),
                    RunIds = plan.Runs.Select(r => r.Run.Id).ToList(),
                };
                _active = active;
                // Task.Run يأخذ سياق التنفيذ الحالى (جلسة المستخدم) كما هو.
                active.Task = Task.Run(() => ExecuteAsync(plan, active));
                return true;
            }
        }

        public bool Cancel(long groupId)
        {
            lock (_lock)
            {
                if (_active?.GroupId != groupId) return false;
                _active.Cts.Cancel();
                return true;
            }
        }

        private async Task ExecuteAsync(GroupPlan plan, ActiveGroup active)
        {
            var ct = active.Cts.Token;
            var live = plan.Runs.ToList();
            var gw = new AiChatGateway(_registry, _tools);
            try
            {
                foreach (var c in plan.Cases)
                {
                    if (ct.IsCancellationRequested || live.Count == 0) break;
                    active.Current = c.Question;
                    var (reference, refError) = await ReferenceAsync(c, plan, live[0].Run.Id, ct);
                    string examples = await ExamplesAsync(c, plan, ct);   // نفس الأمثلة للنموذجين
                    foreach (var rp in live.ToList())
                    {
                        if (ct.IsCancellationRequested) break;
                        var res = await RunCaseAsync(gw, plan, rp, c, reference, refError, ct, examples);
                        if (res == null) break;   // أُوقف: الحالة الجارية لا تُحفظ
                        Record(rp.Run, res);
                        if (plan.MaxCost > 0 && (rp.Run.CostEstimate ?? 0) >= plan.MaxCost)
                        {
                            _db.Finish(rp.Run.Id, AiEvalRunStatus.CostCap, $"توقّف عند حد التكلفة ({plan.MaxCost.ToString("0.##", CultureInfo.InvariantCulture)}).");
                            live.Remove(rp);
                        }
                    }
                }
                string status = ct.IsCancellationRequested ? AiEvalRunStatus.Cancelled : AiEvalRunStatus.Done;
                foreach (var rp in live) _db.Finish(rp.Run.Id, status, status == AiEvalRunStatus.Cancelled ? "أوقفه المدير." : null!);
            }
            catch (Exception ex)
            {
                operations.DataDictionaryLogerException(ex);
                foreach (var rp in live)
                    try { _db.Finish(rp.Run.Id, ct.IsCancellationRequested ? AiEvalRunStatus.Cancelled : AiEvalRunStatus.Failed, Cut("خطأ أثناء التقييم: " + ex.Message, 1000)); }
                    catch (Exception ex2) { operations.DataDictionaryLogerException(ex2); }
            }
            finally
            {
                lock (_lock) { if (ReferenceEquals(_active, active)) _active = null; }
                active.Cts.Dispose();
            }
        }

        private void Record(AiEvalRunRow run, AiEvalResultRow res)
        {
            _db.InsertResult(res);
            run.DoneCases++;
            if (res.Outcome == AiEvalOutcomes.Pass) run.PassedCases++;
            else if (res.Outcome == AiEvalOutcomes.Fail) run.FailedCases++;
            else if (res.Outcome == AiEvalOutcomes.Error) run.ErrorCases++;
            if (res.ToolOk) run.ToolOkCases++;
            if (res.InputTokens != null) run.InputTokens = (run.InputTokens ?? 0) + res.InputTokens.Value;
            if (res.OutputTokens != null) run.OutputTokens = (run.OutputTokens ?? 0) + res.OutputTokens.Value;
            if (res.CostEstimate != null) run.CostEstimate = (run.CostEstimate ?? 0) + res.CostEstimate.Value;
            run.DurationMs = (run.DurationMs ?? 0) + (res.DurationMs ?? 0);
            _db.UpdateProgress(run);
        }

        // ───────────── الحالة الواحدة ─────────────

        /// <summary>
        /// P7.7: كتلة الأمثلة التلقائية للحالة بصلاحيات من أطلق التقييم. يُستبعد المثال الذى هو الحالة نفسها (نفس السؤال، أو نفس الاستعلام المرجعى،
        /// أو من نفس الملاحظة) حتى لا يُقاس «حفظ الإجابة» بدل الفائدة الفعلية. لا يرمى (فشل البحث = بلا أمثلة).
        /// </summary>
        private async Task<string> ExamplesAsync(AiEvalCaseRow c, GroupPlan plan, CancellationToken ct)
        {
            if (plan.AutoExamples <= 0) return "";
            string q = string.Join(" ", Knowledge.AiKnowledgeService.Words(c.Question));
            bool Same(AiKnowledgeRow r) =>
                (c.Source_Ref_Id != null && r.Source == "feedback" && r.Source_Ref_Id == c.Source_Ref_Id)
                || string.Join(" ", Knowledge.AiKnowledgeService.Words(r.TitleAr)) == q
                || (!string.IsNullOrWhiteSpace(c.ReferenceSql) && NormSql(r.Content) == NormSql(c.ReferenceSql));
            var probe = new AiToolContext { User = plan.User, AllowedViews = plan.AllowedViews, TaxFilter = plan.TaxFilter, Ct = ct, IsEval = true };
            var ex = await _tools.KnowledgeService.ExamplesAsync(c.Question, plan.AllowedViews, probe.HasRoute, plan.AutoExamples, ct, Same);
            return Knowledge.AiKnowledgeService.ExamplesBlock(ex);
        }

        private static string NormSql(string? s) => string.Join(" ", (s ?? "").Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)).TrimEnd(';').ToLowerInvariant();

        private AiToolContext Context(GroupPlan plan, long runId, CancellationToken ct) => new()
        {
            User = plan.User, AllowedViews = plan.AllowedViews, TaxFilter = plan.TaxFilter, Ct = ct, IsEval = true, EvalRunId = runId,
        };

        /// <summary>يعيد null إن أُوقف التشغيل كله أثناء الحالة.</summary>
        private async Task<AiEvalResultRow?> RunCaseAsync(AiChatGateway gw, GroupPlan plan, RunPlan rp, AiEvalCaseRow c,
                                                          AiDataset? reference, string? refError, CancellationToken ct, string examples = "")
        {
            var sw = Stopwatch.StartNew();
            AiChatGateway.LoopResult loop;
            int attempt = 0;
            while (true)
            {
                using var caseCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                caseCts.CancelAfter(CaseTimeout);
                var ctx = Context(plan, rp.Run.Id, caseCts.Token);
                loop = await gw.LoopAsync(new List<AiChatGateway.Candidate> { rp.Candidate }, _ => new List<AiChatTurn> { new("user", c.Question + examples) },
                                          ctx, null, null, plan.MaxRounds, null!, null!, caseCts.Token);
                if (loop.Stopped && ct.IsCancellationRequested) return null;
                // 429 (حد المعدل) قبل أى نص أو أداة: إعادة واحدة بعد انتظار قصير.
                if (attempt == 0 && !loop.Stopped && loop.Error != null && loop.Error.Contains("429") && loop.Text.Length == 0 && loop.Rounds == 0)
                {
                    attempt++;
                    try { await Task.Delay(RetryDelay, ct); } catch (OperationCanceledException) { return null; }
                    continue;
                }
                break;
            }
            sw.Stop();

            var model = rp.Candidate.Model;
            bool refused = loop.Error == "refused" || (loop.Outcome?.Refused == true && loop.Text.Length == 0);
            AiEvalVerdict verdict;
            if (loop.Stopped)
                verdict = new(AiEvalOutcomes.Error, false, $"تجاوزت الحالة المهلة ({(int)CaseTimeout.TotalSeconds} ثانية).");
            else
                verdict = AiEvalJudge.Judge(new AiEvalJudgeInput
                {
                    Case = c,
                    Answer = loop.Text,
                    Tools = loop.Executed.Select(e => new AiEvalToolCall(e.Name, !e.Result.IsError && e.Result.Outcome == AiToolOutcome.Ok, e.Result.Dataset, e.Result.Outcome)).ToList(),
                    Asked = loop.Pending != null,
                    ProviderRefused = refused,
                    Error = refused ? null : loop.Error,
                    Reference = reference,
                    ReferenceError = refError,
                });

            decimal? cost = null;
            if (loop.AnyUsage && (model.InputPricePerM != null || model.OutputPricePerM != null))
                cost = Math.Round((loop.InTok * (model.InputPricePerM ?? 0) + loop.OutTok * (model.OutputPricePerM ?? 0)) / 1_000_000m, 6);
            string? reason = verdict.Reason;
            if (verdict.Outcome != AiEvalOutcomes.Pass && reason != null)
            {
                if (loop.RoundsExhausted) reason += $" (بلغ الحد الأقصى {plan.MaxRounds} دورة أدوات)";
                else if (loop.Outcome?.Truncated == true) reason += " (قُطع الرد عند حد التوكنز)";
            }
            return new AiEvalResultRow
            {
                Run_Id = rp.Run.Id,
                Case_Id = c.Id,
                CaseKey = c.CaseKey,
                CaseVersion = c.Version,
                Outcome = verdict.Outcome,
                ToolOk = verdict.ToolOk,
                FailReason = reason,
                AnswerText = Cut(loop.Text, 20000),
                ToolsJson = ToolsJson(loop),
                Rounds = loop.Rounds,
                InputTokens = loop.AnyUsage ? loop.InTok : null,
                OutputTokens = loop.AnyUsage ? loop.OutTok : null,
                CostEstimate = cost,
                DurationMs = (int)Math.Min(int.MaxValue, sw.ElapsedMilliseconds),
            };
        }

        private static readonly JsonSerializerOptions StoreJson = new() { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

        private static string ToolsJson(AiChatGateway.LoopResult loop)
        {
            var arr = new JsonArray();
            foreach (var e in loop.Executed)
            {
                var o = new JsonObject
                {
                    ["name"] = e.Name,
                    ["title"] = e.Result.Title,
                    ["outcome"] = e.Result.Outcome,
                    ["input"] = Cut(e.InputJson ?? "", 4000),
                    ["sql"] = e.Result.Sql == null ? null : Cut(e.Result.Sql, 4000),
                    ["rows"] = e.Result.RowCount,
                };
                if (e.Result.IsError) o["error"] = Cut(e.Result.Content, 1000);
                arr.Add(o);
            }
            if (loop.Pending != null) arr.Add(new JsonObject { ["name"] = AiToolRegistry.AskUserTool, ["outcome"] = "asked" });
            return arr.ToJsonString(StoreJson);
        }

        // ───────────── المرجع ─────────────

        /// <summary>D12: المرجع يُحسب حياً بنفس صلاحيات من أطلق التقييم (نفس Views ونفس فلتر الفواتير الضريبية) ويُسجَّل فى التدقيق.</summary>
        private async Task<(AiDataset? Data, string? Error)> ReferenceAsync(AiEvalCaseRow c, GroupPlan plan, long runId, CancellationToken ct)
        {
            string check = (c.CheckType ?? "").Trim().ToLowerInvariant();
            var sw = Stopwatch.StartNew();
            if (check == AiEvalCheckTypes.Result)
            {
                if (string.IsNullOrWhiteSpace(c.ReferenceSql)) return (null, null);
                try
                {
                    var session = new Dictionary<string, object> { ["ai_tax"] = plan.TaxFilter, ["ai_user"] = plan.User.UserId };
                    var r = _tools.Sql.Execute(c.ReferenceSql, plan.AllowedViews, session, null, ct);
                    Audit(plan, runId, c, c.ReferenceSql, r.Rows.Count, sw, AiToolOutcome.Ok, null);
                    if (r.Truncated) return (null, $"الاستعلام المرجعى أعاد أكثر من {ReadOnlySqlExecutor.MaxRows} صف؛ لا يمكن المقارنة.");
                    return (AiDataset.FromQuery(r), null);
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
                catch (Exception ex)
                {
                    string msg = ex is AiSqlRejectedException ? ex.Message : ex.InnerException?.Message ?? ex.Message;
                    Audit(plan, runId, c, c.ReferenceSql, null, sw, ex is AiSqlRejectedException ? AiToolOutcome.Denied : AiToolOutcome.Error, msg);
                    return (null, msg);
                }
            }
            if (check == AiEvalCheckTypes.Financial)
            {
                string tool = AiEvalJudge.Csv(c.ExpectedTools).FirstOrDefault() ?? "";
                if (tool.Length == 0 || string.IsNullOrWhiteSpace(c.ReferenceArgsJson)) return (null, null);
                string args = ResolveDates(c.ReferenceArgsJson, DateTime.Today);
                var res = await _tools.ExecuteAsync(tool, args, Context(plan, runId, ct));
                string? err = res.IsError ? ReadError(res.Content) : res.Dataset == null ? "الأداة لم تُعِد بيانات." : null;
                Audit(plan, runId, c, tool + " " + args, res.RowCount, sw, res.IsError ? res.Outcome : AiToolOutcome.Ok, err);
                return err != null ? (null, err) : (res.Dataset, null);
            }
            return (null, null);
        }

        private void Audit(GroupPlan plan, long runId, AiEvalCaseRow c, string sql, int? rows, Stopwatch sw, string outcome, string? error)
        {
            try
            {
                _audit.Insert(null, plan.User.UserId, ReferenceToolName, "{\"case\":\"" + c.CaseKey.Replace("\"", "") + "\"}", sql, rows,
                              (int)Math.Min(int.MaxValue, sw.ElapsedMilliseconds), outcome, error, runId);
            }
            catch (Exception ex) { operations.DataDictionaryLogerException(ex); }
        }

        /// <summary>العناصر النائبة للتواريخ فى ReferenceArgsJson.</summary>
        public static string ResolveDates(string json, DateTime today)
        {
            var ms = new DateTime(today.Year, today.Month, 1);
            string D(DateTime d) => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            return json.Replace("{today}", D(today))
                       .Replace("{month_start}", D(ms))
                       .Replace("{month_end}", D(ms.AddMonths(1).AddDays(-1)))
                       .Replace("{year_start}", D(new DateTime(today.Year, 1, 1)))
                       .Replace("{prev_month_start}", D(ms.AddMonths(-1)))
                       .Replace("{prev_month_end}", D(ms.AddDays(-1)));
        }

        private static string ReadError(string content)
        {
            try
            {
                using var doc = JsonDocument.Parse(content);
                if (doc.RootElement.ValueKind == JsonValueKind.Object && doc.RootElement.TryGetProperty("error", out var e)) return e.GetString() ?? content;
            }
            catch (JsonException) { }
            return Cut(content, 500);
        }

        private static string Cut(string s, int max) => s.Length > max ? s[..max] : s;
    }
}
