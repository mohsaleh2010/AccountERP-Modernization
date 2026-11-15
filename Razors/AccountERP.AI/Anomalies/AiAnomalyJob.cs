using System.Globalization;
using AccountERP.AI.Scheduling;
using AccountERP.AI.Tools;
using AccountModelsLib;

namespace AccountERP.AI.Anomalies
{
    /// <summary>نتيجة نبضة (للاختبار والسجل).</summary>
    public sealed class AiAnomalyTickResult
    {
        /// <summary>disabled / no_activation / before_time / done / busy / retry_wait، أو "" إن جرى المسح المجدول.</summary>
        public string Skip { get; set; } = "";
        public DateTime LocalNow { get; set; }
        public AiAnomalyRunRow? Scheduled { get; set; }
        public List<AiAnomalyRunRow> QueuedRuns { get; } = new();
        public List<string> Errors { get; } = new();
    }

    /// <summary>
    /// P8.4.2 — مهمة المسح الليلى (Singleton؛ ai-assistant-p84-plan.md §3، ش3، ش5–ش7، ش16، ش28، ش29).
    /// النبضة: طلبات Queued أولاً (تاريخى) ⇒ مفعّل + تاريخ تفعيل + بعد الموعد بتوقيت الخادم ⇒ حجز s:yyyy-MM-dd ⇒
    /// النافذة [max(التفعيل، العلامة المائية)، الآن − 10 د) ⇒ المسح ⇒ Done يحرّك العلامة؛ الفشل لا يحرّكها (الفهرس الفريد يمنع التكرار عند الإعادة).
    /// Running عالق > 30 د يُستولى عليه، وFailed يُعاد بعد 15 د. حد التشغيل 10 دقائق.
    /// </summary>
    public sealed class AiAnomalyJob : IAiScheduledJob
    {
        private readonly AiAnomalyCLS _dal;
        private readonly AiProvidersCLS _providers;
        private readonly SemaphoreSlim _gate = new(1, 1);

        public AiAnomalyScanner Scanner { get; }
        public string Name => "entry_anomalies";

        /// <summary>ساعة UTC (قابلة للحقن فى الاختبار) — للتشغيل اليدوى والطلبات.</summary>
        public Func<DateTime> UtcClock { get; set; } = () => DateTime.UtcNow;
        /// <summary>اختيارى للاختبار: إعدادات بدل AI_Settings.</summary>
        public Func<AiAnomalySettings>? SettingsSource { get; set; }
        /// <summary>اختيارى للاختبار: يقصر معالجة الطلبات المنتظرة على صفوف بعينها.</summary>
        public Func<AiAnomalyRunRow, bool>? QueuedFilter { get; set; }
        /// <summary>اختيارى للاختبار: مفتاح المجدول بدل s:yyyy-MM-dd (فلا يحجز الاختبار يوم الإنتاج).</summary>
        public Func<DateTime, string>? ScheduleKey { get; set; }
        /// <summary>اختيارى للاختبار: TriggeredBy للمجدول (test لا يحرّك علامة الإنتاج).</summary>
        public string ScheduleTrigger { get; set; } = AiAnomalyTrigger.Schedule;
        /// <summary>اختيارى للاختبار: العلامة المائية بدل LastDoneTo.</summary>
        public Func<DateTime?>? WatermarkSource { get; set; }
        public TimeSpan RunTimeout { get; set; } = TimeSpan.FromMinutes(AiAnomalyConst.RunTimeoutMinutes);

        public AiAnomalyJob(ReadOnlySqlExecutor sql) : this(sql, null, null) { }

        public AiAnomalyJob(ReadOnlySqlExecutor sql, AiAnomalyCLS? dal, AiProvidersCLS? providers)
        {
            _dal = dal ?? new AiAnomalyCLS();
            _providers = providers ?? new AiProvidersCLS();
            Scanner = new AiAnomalyScanner(sql, _dal);
        }

        public AiAnomalyCLS Dal => _dal;

        public AiAnomalySettings Settings() => SettingsSource?.Invoke() ?? AiAnomalySettings.From(_providers.GetSettings());

        /// <summary>ش7: كل الأوقات بتوقيت الخادم المحلى (Insert_ServerDateTime = GETDATE() — فحص 9).</summary>
        public static DateTime Local(DateTime nowUtc) =>
            (nowUtc.Kind == DateTimeKind.Utc ? nowUtc : DateTime.SpecifyKind(nowUtc, DateTimeKind.Utc)).ToLocalTime();

        public Task TickAsync(DateTime nowUtc, CancellationToken ct) => RunScheduleAsync(nowUtc, ct);

        public async Task<AiAnomalyTickResult> RunScheduleAsync(DateTime nowUtc, CancellationToken ct)
        {
            var result = new AiAnomalyTickResult();
            if (!await _gate.WaitAsync(0, ct)) { result.Skip = "busy"; return result; }
            try
            {
                var local = Local(nowUtc);
                result.LocalNow = local;
                var settings = Settings();

                // 2) الطلبات المنتظرة (مسح تاريخى) أولاً بالترتيب.
                foreach (var q in _dal.ListQueued())
                {
                    if (QueuedFilter != null && !QueuedFilter(q)) continue;
                    ct.ThrowIfCancellationRequested();
                    try { var done = await RunQueuedCoreAsync(q, settings, ct); if (done != null) result.QueuedRuns.Add(done); }
                    catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
                    catch (Exception ex) { result.Errors.Add($"queued {q.RunKey}: {ex.Message}"); Log(ex); }
                }

                // 3) شروط المسح المجدول.
                if (!settings.Enabled) { result.Skip = "disabled"; return result; }
                if (!settings.ActivatedAt.HasValue) { result.Skip = "no_activation"; return result; }
                if (local.TimeOfDay < settings.ScanTime) { result.Skip = "before_time"; return result; }

                // 4) الحجز.
                string key = ScheduleKey?.Invoke(local.Date) ?? "s:" + local.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                var run = _dal.GetRunByKey(key);
                if (run == null)
                {
                    var row = new AiAnomalyRunRow { RunKey = key, TriggeredBy = ScheduleTrigger, Status = AiAnomalyRunStatus.Running, RulesCsv = Csv(settings) };
                    if (_dal.TryReserveRun(row, 0) > 0) run = row;
                    else { result.Skip = "busy"; return result; }
                }
                else if (run.Status == AiAnomalyRunStatus.Done) { result.Skip = "done"; return result; }
                else if (run.Status == AiAnomalyRunStatus.Running)
                {
                    if (!_dal.TakeOverStale(run.Id, AiAnomalyConst.StaleMinutes, 0)) { result.Skip = "busy"; return result; }
                }
                else if (run.Status == AiAnomalyRunStatus.Failed)
                {
                    if ((run.UpdatedAt ?? run.CreatedAt ?? DateTime.MinValue) >= DateTime.Now.AddMinutes(-AiAnomalyConst.FailedRetryMinutes)) { result.Skip = "retry_wait"; return result; }
                    if (!_dal.MarkRunning(run.Id, 0)) { result.Skip = "busy"; return result; }
                }
                else { result.Skip = "busy"; return result; }

                result.Scheduled = await ExecuteWindowAsync(run, settings, local, ct);
                return result;
            }
            finally { _gate.Release(); }
        }

        /// <summary>«تشغيل الآن» (مدير): نفس نافذة المجدول بمفتاح m:{id}؛ Done يحرّك العلامة المائية.</summary>
        public async Task<AiAnomalyRunRow> RunManualAsync(long adminId, CancellationToken ct)
        {
            var settings = Settings();
            if (!settings.Enabled || !settings.ActivatedAt.HasValue) throw new AiAnomalyException(AiAnomalyErrors.Disabled, "كشف الشذوذ غير مفعّل.");
            if (!await _gate.WaitAsync(0, ct)) throw new AiAnomalyException(AiAnomalyErrors.Busy, "مسح آخر يعمل الآن.");
            try
            {
                long id = CallSnowflakeIdGenerate.GenerateNewId();
                var row = new AiAnomalyRunRow
                {
                    Id = id, RunKey = "m:" + id.ToString(CultureInfo.InvariantCulture), TriggeredBy = AiAnomalyTrigger.Manual,
                    Status = AiAnomalyRunStatus.Running, RulesCsv = Csv(settings), RequestedBy = adminId,
                };
                if (_dal.TryReserveRun(row, adminId) == 0) throw new AiAnomalyException(AiAnomalyErrors.Busy, "تعذّر حجز التشغيل.");
                return await ExecuteWindowAsync(row, settings, Local(UtcClock()), ct);
            }
            finally { _gate.Release(); }
        }

        /// <summary>طلب منتظر (مسح تاريخى) — يمر بالبوابة نفسها.</summary>
        public async Task<AiAnomalyRunRow?> RunQueuedAsync(AiAnomalyRunRow queued, CancellationToken ct)
        {
            if (!await _gate.WaitAsync(0, ct)) throw new AiAnomalyException(AiAnomalyErrors.Busy, "مسح آخر يعمل الآن.");
            try { return await RunQueuedCoreAsync(queued, Settings(), ct); }
            finally { _gate.Release(); }
        }

        // ───────────── التنفيذ ─────────────

        private async Task<AiAnomalyRunRow> ExecuteWindowAsync(AiAnomalyRunRow run, AiAnomalySettings s, DateTime local, CancellationToken ct)
        {
            DateTime activated = s.ActivatedAt ?? local;
            DateTime? mark = WatermarkSource != null ? WatermarkSource() : _dal.LastDoneTo();
            DateTime from = mark.HasValue && mark.Value > activated ? mark.Value : activated;
            DateTime to = local.AddMinutes(-AiAnomalyConst.SettleMinutes);
            if (to <= from)
            {
                _dal.CompleteRun(run.Id, AiAnomalyRunStatus.Done, from, from, 0, 0, 0, false, 0, null, Csv(s), run.RequestedBy ?? 0);
                return _dal.GetRun(run.Id) ?? run;
            }
            // ش4: BaselineHistory = 0 ⇒ خط الأساس لا يقرأ ما قبل التفعيل.
            DateTime? floor = s.BaselineHistory ? null : activated;
            return await ScanAndCompleteAsync(run, s, from, to, byEntryDate: false, backfill: false, floor, setWindow: true, ct);
        }

        private async Task<AiAnomalyRunRow?> RunQueuedCoreAsync(AiAnomalyRunRow q, AiAnomalySettings s, CancellationToken ct)
        {
            if (!q.FromTs.HasValue || !q.ToTs.HasValue)
            {
                if (_dal.MarkRunning(q.Id, 0))
                    _dal.CompleteRun(q.Id, AiAnomalyRunStatus.Failed, null, null, 0, 0, 0, false, 0, "missing period", null, 0);
                return _dal.GetRun(q.Id);
            }
            if (!_dal.MarkRunning(q.Id, 0)) return null;
            // ش5: فترة بتاريخ القيد (to شامل) ⇒ [from, to + 1 يوم) على entry_date؛ لا يحرّك العلامة المائية.
            return await ScanAndCompleteAsync(q, s, q.FromTs.Value.Date, q.ToTs.Value.Date.AddDays(1), byEntryDate: true, backfill: true, null, setWindow: false, ct);
        }

        private async Task<AiAnomalyRunRow> ScanAndCompleteAsync(AiAnomalyRunRow run, AiAnomalySettings s, DateTime from, DateTime to, bool byEntryDate, bool backfill,
                                                                 DateTime? floor, bool setWindow, CancellationToken ct)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            long by = run.RequestedBy ?? 0;
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(RunTimeout);
            try
            {
                var r = await Task.Run(() => Scanner.Scan(from, to, s, byEntryDate, backfill, run.Id, s.MaxPerRun, cts.Token, floor), cts.Token);
                string? errors = r.Errors.Count == 0 && r.Notes.Count == 0 ? null
                    : string.Join("; ", r.Errors.Select(e => e.Key + ": " + e.Value).Concat(r.Notes));
                _dal.CompleteRun(run.Id, AiAnomalyRunStatus.Done, setWindow ? from : null, setWindow ? to : null, r.EntriesScanned, r.Found, r.Inserted, r.Truncated,
                                 (int)sw.ElapsedMilliseconds, errors, Csv(s), by);
                LastScan = r;
            }
            catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
            {
                string msg = ex is OperationCanceledException ? "timeout" : ex.Message;
                _dal.CompleteRun(run.Id, AiAnomalyRunStatus.Failed, setWindow ? from : null, null, 0, 0, 0, false, (int)sw.ElapsedMilliseconds, msg, Csv(s), by);
                Log(ex);
            }
            catch (OperationCanceledException)
            {
                _dal.CompleteRun(run.Id, AiAnomalyRunStatus.Failed, setWindow ? from : null, null, 0, 0, 0, false, (int)sw.ElapsedMilliseconds, "cancelled", Csv(s), by);
                throw;
            }
            return _dal.GetRun(run.Id) ?? run;
        }

        /// <summary>آخر نتيجة مسح (للاختبار).</summary>
        public AiAnomalyScanResult? LastScan { get; private set; }

        private static string Csv(AiAnomalySettings s) => string.Join(",", AiAnomalyRuleCodes.All.Where(s.RuleOn));

        private static void Log(Exception ex) { try { operations.DataDictionaryLogerException(ex); } catch { } }
    }
}
