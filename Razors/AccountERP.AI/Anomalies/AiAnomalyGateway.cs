using System.Globalization;
using AccountERP.AI.Providers;
using AccountERP.AI.SignIn;
using AccountERP.AI.Tools;
using AccountModelsLib;

namespace AccountERP.AI.Anomalies
{
    /// <summary>
    /// P8.4.3/P8.4.4 — بوابة شذوذ القيود (Scoped، للويب فقط — ش31). الصلاحيات تُبنى على الخادم بـ AiUserContextFactory.ForUser:
    /// العرض بشاشة /accounts/entries-anomalies أو مفتاح قائمتها (ش20) وفروع المستخدم (ش21)؛ الإعدادات والتشغيل للمدير
    /// (مدير النظام أو مالك /settings/ai). الأخطاء AiAnomalyException بمفتاح resx فى Code. القيود لا تُكتب أبداً.
    /// </summary>
    public sealed class AiAnomalyGateway : IAiAnomalyGateway
    {
        private static readonly HashSet<string> GlView = new(new[] { "v_gl_entries" }, StringComparer.OrdinalIgnoreCase);

        private readonly AiAnomalyJob _job;
        private readonly ProviderRegistry? _registry;
        private readonly AiAnomalyCLS _dal;
        private readonly AiProvidersCLS _providers;
        private readonly Func<long, AiServerUser> _users;
        private readonly AiToolAuditCLS _audit = new();

        /// <summary>اختيارى للاختبار: كتابة الإعداد بدل AI_Settings.</summary>
        public Action<string, string, long>? SettingWriter { get; set; }
        /// <summary>اختيارى للاختبار: فروع المستخدم (null = كل الفروع) بدل FinalAccountsService.</summary>
        public Func<AiServerUser, List<long>?>? BranchSource { get; set; }
        /// <summary>اختيارى للاختبار: لا يوقظ المهمة بعد طلب مسح تاريخى.</summary>
        public bool WakeJobOnBackfill { get; set; } = true;
        /// <summary>اختيارى للاختبار: «اليوم» بتوقيت الخادم.</summary>
        public Func<DateTime> Today { get; set; } = () => DateTime.Today;

        public AiAnomalyGateway(AiAnomalyJob job, ProviderRegistry registry) : this(job, registry, null, null, null) { }

        public AiAnomalyGateway(AiAnomalyJob job, ProviderRegistry? registry, AiAnomalyCLS? dal, AiProvidersCLS? providers, Func<long, AiServerUser>? users)
        {
            _job = job;
            _registry = registry;
            _dal = dal ?? job.Dal;
            _providers = providers ?? new AiProvidersCLS();
            _users = users ?? AiUserContextFactory.ForUser;
        }

        public ProviderRegistry? Registry => _registry;

        // ───────────── العرض ─────────────

        public AiAnomalySummary Summary(AiUserContext user)
        {
            var server = EnsureView(user);
            var s = _dal.Summary(Branches(server));
            s.Enabled = _job.Settings().Enabled;
            return s;
        }

        public AiAnomalyPage List(AiAnomalyFilter filter, AiUserContext user)
        {
            var server = EnsureView(user);
            var branches = Branches(server);
            filter ??= new AiAnomalyFilter();
            if (filter.BranchId != 0 && branches != null && !branches.Contains(filter.BranchId)) return new AiAnomalyPage();
            var page = _dal.ListEntries(filter, branches);
            bool canOpen = CanOpen(server);
            var live = LiveTotals(page.Items.Select(i => i.EntryId).ToList());
            foreach (var e in page.Items) Decorate(e, live, canOpen);
            return page;
        }

        public AiAnomalyEntryDetail GetEntry(long entryId, AiUserContext user)
        {
            var server = EnsureView(user);
            var view = _dal.GetEntryView(entryId, Branches(server))
                       ?? throw new AiAnomalyException(AiAnomalyErrors.NotFound, "القيد غير موجود فى قائمة الشذوذ.");
            var detail = new AiAnomalyEntryDetail { Entry = view };
            var flagged = view.Rules.Where(r => r.Line_Id.HasValue).Select(r => r.Line_Id!.Value).ToHashSet();
            var r = _job.Sql.Execute(@"SELECT TOP (200) line_id, line_no, account_id, account_no, account_name, debit, credit, debit_equivalent, credit_equivalent,
       line_notes, entry_notes, journal_name, branch_name
FROM ai.v_gl_entries WHERE entry_id = @e ORDER BY line_no, line_id", GlView, ReadOnlySqlExecutor.SessionFor(-1, 0),
                new Dictionary<string, object> { ["e"] = entryId }, CancellationToken.None);
            foreach (var row in r.Rows)
            {
                detail.Lines.Add(new AiAnomalyLineView
                {
                    LineId = L(row[0]), LineNo = row[1] == null ? null : Convert.ToInt32(row[1], CultureInfo.InvariantCulture),
                    AccountId = row[2] == null ? null : L(row[2]), AccountNo = Str(row[3]), AccountName = Str(row[4]),
                    Debit = M(row[5]), Credit = M(row[6]), DebitEquivalent = M(row[7]), CreditEquivalent = M(row[8]), Notes = Str(row[9]),
                    Flagged = flagged.Contains(L(row[0])),
                });
                detail.EntryNotes ??= Str(row[10]);
                view.JournalName ??= Str(row[11]);
                if (string.IsNullOrEmpty(view.BranchName)) view.BranchName = Str(row[12]);
            }
            var live = new Dictionary<long, decimal>();
            if (detail.Lines.Count > 0) live[entryId] = detail.Lines.Sum(l => l.DebitEquivalent);
            Decorate(view, live, CanOpen(server));
            return detail;
        }

        public void SetStatus(IList<long> entryIds, string status, string note, AiUserContext user)
        {
            var server = EnsureView(user);
            if (!AiAnomalyStatus.IsKnown(status)) throw new AiAnomalyException(AiAnomalyErrors.Range, "حالة غير معروفة.");
            if (entryIds == null || entryIds.Count == 0) return;
            string? n = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
            if (n != null && n.Length > 500) n = n.Substring(0, 500);
            int updated = _dal.SetStatus(entryIds, status, n!, server.UserId, Branches(server));
            if (updated == 0) throw new AiAnomalyException(AiAnomalyErrors.NotFound, "القيد غير موجود فى قائمة الشذوذ.");
        }

        /// <summary>
        /// P8.4.5 (ش24–ش26): صلاحية وفرع؛ الشرح موقوف ⇒ off؛ شرح مخزَّن (model/rejected) لنفس اللغة ولا regenerate ⇒ يُعاد بلا استدعاء؛
        /// regenerate للمدير فقط؛ الحد اليومى ⇒ capped (لا يُحفظ)؛ غير ذلك استدعاء واحد ثم الحفظ والتدقيق anomaly_explain.
        /// </summary>
        public async Task<AiAnomalyExplainRow> ExplainAsync(long entryId, string language, bool regenerate, AiUserContext user, CancellationToken ct)
        {
            var server = EnsureView(user);
            string lang = AiBriefConst.NormalizeLanguage(language);
            var s = _job.Settings();
            if (_dal.GetEntryView(entryId, Branches(server)) == null)
                throw new AiAnomalyException(AiAnomalyErrors.NotFound, "القيد غير موجود فى قائمة الشذوذ.");
            if (!s.Explain) return new AiAnomalyExplainRow { Entry_Id = entryId, Language = lang, Source = AiBriefNarrativeSource.Off };
            if (regenerate && !IsAdmin(server)) throw new AiAnomalyException(AiAnomalyErrors.NotAdmin, "إعادة الشرح للمدير فقط.");
            var existing = _dal.GetExplain(entryId, lang);
            if (!regenerate && existing != null && (existing.Source == AiBriefNarrativeSource.Model || existing.Source == AiBriefNarrativeSource.Rejected))
                return existing;
            if (_dal.ExplainCountSince(Today().Date) >= s.ExplainMaxPerDay)
                return new AiAnomalyExplainRow { Entry_Id = entryId, Language = lang, Source = AiBriefNarrativeSource.Capped };
            var detail = GetEntry(entryId, user);
            bool minimize = !(_providers.GetSettings().TryGetValue(AiSettingKeys.DataMinimization, out var dm) && dm == "0");
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var row = await Explainer.ExplainAsync(detail, lang, s, minimize, ct);
            row.Entry_Id = entryId;
            if (row.Source != AiBriefNarrativeSource.Off) _dal.SaveExplain(row, server.UserId);
            try
            {
                _audit.Insert(null, server.UserId, AiAnomalyConst.ExplainAuditTool,
                    System.Text.Json.JsonSerializer.Serialize(new { entry_id = entryId, language = lang, source = row.Source, model = row.ModelName, minimize }),
                    null, null, (int)sw.ElapsedMilliseconds, row.Source == AiBriefNarrativeSource.Model ? AiToolOutcome.Ok : AiToolOutcome.Error, row.RejectReason);
            }
            catch (Exception ex) { try { operations.DataDictionaryLogerException(ex); } catch { } }
            return row;
        }

        private AiAnomalyExplainer? _explainer;
        /// <summary>المفسّر (قابل للحقن فى الاختبار).</summary>
        public AiAnomalyExplainer Explainer
        {
            get => _explainer ??= new AiAnomalyExplainer(_registry ?? throw new InvalidOperationException("no provider registry"), _providers);
            set => _explainer = value;
        }

        // ───────────── الإدارة ─────────────

        public AiAnomalySettings GetSettings(AiUserContext admin)
        {
            EnsureAdmin(admin);
            return _job.Settings();
        }

        /// <summary>ش6: الانتقال 0→1 يكتب AnomalyActivatedAt = الآن دائماً؛ القيمة الواردة من الواجهة تُتجاهل.</summary>
        public void SaveSettings(AiAnomalySettings settings, AiUserContext admin)
        {
            long uid = EnsureAdmin(admin);
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            var errors = settings.Validate();
            if (errors.Count > 0) throw new AiAnomalyException(errors[0], "إعدادات غير صالحة: " + string.Join(", ", errors));
            var current = _job.Settings();
            var s = settings.Clone();
            DateTime now = DateTime.Now;
            s.ActivatedAt = s.Enabled && !current.Enabled ? new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second) : current.ActivatedAt;
            var write = SettingWriter ?? ((k, v, by) => _providers.SetSetting(k, v, by));
            foreach (var p in s.ToPairs()) write(p.Key, p.Value, uid);
        }

        public async Task<AiAnomalyRunRow> RunNowAsync(AiUserContext admin, CancellationToken ct)
        {
            long uid = EnsureAdmin(admin);
            return await _job.RunManualAsync(uid, ct);
        }

        /// <summary>ش5: مدير فقط، from ≤ to ≤ اليوم، حتى 92 يوماً (شاملة). يدرج Queued ويوقظ المهمة دون انتظار.</summary>
        public long QueueBackfill(DateTime from, DateTime to, AiUserContext admin)
        {
            long uid = EnsureAdmin(admin);
            DateTime f = from.Date, t = to.Date;
            if (t < f || t > Today().Date || (t - f).TotalDays + 1 > AiAnomalyConst.MaxBackfillDays)
                throw new AiAnomalyException(AiAnomalyErrors.BackfillRange, "الفترة يجب ألا تتجاوز 92 يوماً ولا تتخطى اليوم.");
            var s = _job.Settings();
            long id = CallSnowflakeIdGenerate.GenerateNewId();
            var row = new AiAnomalyRunRow
            {
                Id = id, RunKey = "b:" + id.ToString(CultureInfo.InvariantCulture), TriggeredBy = AiAnomalyTrigger.Backfill, Status = AiAnomalyRunStatus.Queued,
                FromTs = f, ToTs = t, RequestedBy = uid, RulesCsv = string.Join(",", AiAnomalyRuleCodes.All.Where(s.RuleOn)),
            };
            if (_dal.TryReserveRun(row, uid) == 0) throw new AiAnomalyException(AiAnomalyErrors.Busy, "تعذّر تسجيل الطلب.");
            if (WakeJobOnBackfill)
                _ = Task.Run(async () =>
                {
                    try { CultureInfo.CurrentCulture = CultureInfo.InvariantCulture; await _job.TickAsync(DateTime.UtcNow, CancellationToken.None); }
                    catch (Exception ex) { try { operations.DataDictionaryLogerException(ex); } catch { } }
                });
            return id;
        }

        public List<AiAnomalyRunRow> ListRuns(AiUserContext admin, int top)
        {
            EnsureAdmin(admin);
            return _dal.ListRuns(top <= 0 ? 30 : top);
        }

        // ───────────── مساعدات ─────────────

        /// <summary>الإجمالى الحى (مجموع المدين بالمكافئ) لكل قيد؛ الغائب = حُذف.</summary>
        private Dictionary<long, decimal> LiveTotals(List<long> ids)
        {
            var map = new Dictionary<long, decimal>();
            if (ids.Count == 0) return map;
            foreach (var chunk in ids.Distinct().Chunk(200))
            {
                var p = new Dictionary<string, object>();
                for (int i = 0; i < chunk.Length; i++) p["e" + i] = chunk[i];
                var r = _job.Sql.Execute("SELECT entry_id, SUM(debit_equivalent) AS total FROM ai.v_gl_entries WHERE entry_id IN ("
                    + string.Join(", ", chunk.Select((_, i) => "@e" + i)) + ") GROUP BY entry_id", GlView, ReadOnlySqlExecutor.SessionFor(-1, 0), p, CancellationToken.None);
                foreach (var row in r.Rows) map[L(row[0])] = M(row[1]);
            }
            return map;
        }

        /// <summary>ش18: لقطة مقابل الحى — «لم يعد موجوداً» أو «عُدِّل بعد الكشف» (فحص 4: مقارنة الإجمالى فقط).</summary>
        private static void Decorate(AiAnomalyEntryView e, Dictionary<long, decimal> live, bool canOpen)
        {
            if (live.TryGetValue(e.EntryId, out var t))
            {
                e.LiveTotal = t;
                e.Missing = false;
                e.Changed = Math.Abs(t - e.EntryTotal) > AiAnomalyConst.LineTolerance;
            }
            else { e.LiveTotal = null; e.Missing = true; e.Changed = false; }
            e.CanOpen = canOpen && !e.Missing;
        }

        private List<long>? Branches(AiServerUser server)
        {
            if (BranchSource != null) return BranchSource(server);
            if (server.User.IsAdmin) return null;
            return server.Run(() => new AccountModelsLib.BusinessLibShared.GeneralLedger.FinalAccountsService().GetBranches()).Select(b => b.Id).Distinct().ToList();
        }

        private static bool CanOpen(AiServerUser s) => s.HasRoute(AiAnomalyConst.EntryRoute) || s.HasRoute(AiAnomalyConst.EntriesListRoute);

        private static long Uid(AiUserContext user) =>
            user != null && user.UserId > 0 ? user.UserId : throw new AiAnomalyException(AiAnomalyErrors.NoPermission, "لا يوجد مستخدم.");

        private static bool IsAdmin(AiServerUser s) =>
            s.Usable && (s.User.IsAdmin || s.HasMenuKey(AiSignInService.MenuKey) || s.HasRoute("/settings/ai"));

        private AiServerUser EnsureView(AiUserContext user)
        {
            var server = _users(Uid(user));
            if (!server.Usable || !(server.HasRoute(AiAnomalyConst.Route) || server.HasMenuKey(AiAnomalyConst.MenuKey) || IsAdmin(server)))
                throw new AiAnomalyException(AiAnomalyErrors.NoPermission, "لا تملك صلاحية شاشة «شذوذ القيود».");
            return server;
        }

        private long EnsureAdmin(AiUserContext user)
        {
            long uid = Uid(user);
            if (!IsAdmin(_users(uid)))
                throw new AiAnomalyException(AiAnomalyErrors.NotAdmin, "إعدادات كشف الشذوذ للمدير فقط.");
            return uid;
        }

        private static long L(object? o) => o == null || o is DBNull ? 0 : Convert.ToInt64(o, CultureInfo.InvariantCulture);
        private static decimal M(object? o) => o == null || o is DBNull ? 0 : Convert.ToDecimal(o, CultureInfo.InvariantCulture);
        private static string? Str(object? o) => o == null || o is DBNull ? null : Convert.ToString(o, CultureInfo.InvariantCulture);
    }
}
