using System.Text.Json;
using AccountERP.AI.Providers;
using AccountERP.AI.Scheduling;
using AccountERP.AI.Tools;
using AccountModelsLib;

namespace AccountERP.AI.Brief
{
    /// <summary>نتيجة نبضة واحدة (للاختبار والسجل).</summary>
    public sealed class AiBriefTickResult
    {
        /// <summary>سبب عدم التشغيل: disabled / not_weekday / before_time / busy، أو "" إن جرى.</summary>
        public string Skip { get; set; } = "";
        public DateTime LocalNow { get; set; }
        public int Subscribers { get; set; }
        public int Scopes { get; set; }
        public int Produced { get; set; }
        public int Deliveries { get; set; }
        /// <summary>مستخدم ← سبب تخطيه (delivered، no_menu، inactive …).</summary>
        public Dictionary<long, string> UserSkips { get; } = new();
        public List<string> Errors { get; } = new();
    }

    /// <summary>نتيجة إصدار موجز لنطاق.</summary>
    public sealed class AiBriefProduceResult
    {
        public AiBriefRow? Brief { get; set; }
        /// <summary>جُمعت الحقائق فى هذا النداء (حجز جديد أو استيلاء أو إعادة جمع).</summary>
        public bool Collected { get; set; }
        /// <summary>الصف قيد الجمع لدى نبضة/خادم آخر؛ لا شىء تم.</summary>
        public bool Busy { get; set; }
        public List<string> NarratedLanguages { get; } = new();
        public List<long> NewDeliveryIds { get; } = new();
    }

    /// <summary>
    /// P8.1.4 — مشغّل الموجز اليومى (Singleton؛ ai-assistant-p8-plan.md §3، ق13–ق16، ق25).
    /// النبضة: مفعّل + يوم عمل + بعد الموعد بمنطقة BriefTimeZone ⇒ لكل مشترك مفعّل لم يُسلَّم له موجز اليوم: ForUser ⇒ النطاق،
    /// ثم تجميع المشتركين بالبصمة: حجز بالإدراج (فريد BriefDate+ScopeKey) ⇒ جمع الحقائق ⇒ سرد مرة لكل لغة ⇒ تسليم مرة لكل مستلم ⇒ الإشارة.
    /// لا توليد بأثر رجعى (اليوم فقط، والفترة تغطى الفجوة ≤ 7 أيام). صف Running عالق > 10 د يُستولى عليه، وFailed يُعاد بعد 15 د.
    /// </summary>
    public sealed class AiBriefJob : IAiScheduledJob
    {
        public const int StaleMinutes = 10;
        public const int FailedRetryMinutes = 15;

        private readonly AiBriefCLS _dal;
        private readonly AiProvidersCLS _providers;
        private readonly AiBriefCollector _collector;
        private readonly Func<long, AiServerUser> _users;
        private readonly SemaphoreSlim _gate = new(1, 1);

        public AiBriefNarrator Narrator { get; }
        /// <summary>P8.5: حاسب أولويات التحصيل لمجموعة حقائق collect (يُضبط من DI).</summary>
        public Collections.AiCollectionsScorer? Collections { get => _collector.Collections; set => _collector.Collections = value; }
        public Cashflow.AiCashflowService? Cashflow { get => _collector.Cashflow; set => _collector.Cashflow = value; }
        /// <summary>حد كل نطاق (جمع + سرد) — §3: 60 ث.</summary>
        public TimeSpan ScopeTimeout { get; set; } = TimeSpan.FromSeconds(60);
        /// <summary>ساعة UTC (قابلة للحقن فى الاختبار) — تستعملها البوابة لـ «اليوم».</summary>
        public Func<DateTime> UtcClock { get; set; } = () => DateTime.UtcNow;

        /// <summary>اختيارى للاختبار: مصدر إعدادات بدل AI_Settings.</summary>
        public Func<AiBriefSettings>? SettingsSource { get; set; }
        /// <summary>اختيارى للاختبار: يقصر النبضة على اشتراكات بعينها (فلا يُصدر الاختبار موجزاً لمشترك حقيقى).</summary>
        public Func<AiBriefSubscriptionRow, bool>? SubscriptionFilter { get; set; }

        public string Name => "daily_brief";

        public AiBriefJob(ReadOnlySqlExecutor sql, ProviderRegistry registry) : this(sql, registry, null, null, null) { }

        public AiBriefJob(ReadOnlySqlExecutor sql, ProviderRegistry registry, AiBriefCLS? dal, AiProvidersCLS? providers, Func<long, AiServerUser>? users)
        {
            _dal = dal ?? new AiBriefCLS();
            _providers = providers ?? new AiProvidersCLS();
            _collector = new AiBriefCollector(sql);
            _users = users ?? AiUserContextFactory.ForUser;
            Narrator = new AiBriefNarrator(registry, _providers, _dal);
        }

        public AiBriefSettings Settings() => SettingsSource?.Invoke() ?? AiBriefSettings.From(_providers.GetSettings());

        public Task TickAsync(DateTime nowUtc, CancellationToken ct) => RunScheduleAsync(nowUtc, ct);

        // ───────────── الوقت (ق15) ─────────────

        /// <summary>معرّف Windows أو IANA. فارغ ⇒ false (توقيت الخادم).</summary>
        public static bool TryZone(string? id, out TimeZoneInfo zone)
        {
            zone = TimeZoneInfo.Local;
            if (string.IsNullOrWhiteSpace(id)) return false;
            try { zone = TimeZoneInfo.FindSystemTimeZoneById(id.Trim()); return true; }
            catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException) { }
            if (TimeZoneInfo.TryConvertIanaIdToWindowsId(id.Trim(), out var win))
                try { zone = TimeZoneInfo.FindSystemTimeZoneById(win); return true; } catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException) { }
            return false;
        }

        /// <summary>الوقت المحلى بمنطقة الموجز (فارغة أو غير صالحة ⇒ توقيت الخادم).</summary>
        public static DateTime LocalTime(DateTime nowUtc, string? timeZone)
        {
            var utc = nowUtc.Kind == DateTimeKind.Utc ? nowUtc : DateTime.SpecifyKind(nowUtc, DateTimeKind.Utc);
            return TryZone(timeZone, out var z) ? TimeZoneInfo.ConvertTimeFromUtc(utc, z) : utc.ToLocalTime();
        }

        public DateTime LocalNow(AiBriefSettings s) => LocalTime(UtcClock(), s.TimeZone);

        // ───────────── النبضة ─────────────

        public async Task<AiBriefTickResult> RunScheduleAsync(DateTime nowUtc, CancellationToken ct)
        {
            var result = new AiBriefTickResult();
            if (!await _gate.WaitAsync(0, ct)) { result.Skip = "busy"; return result; }
            try
            {
                var settings = Settings();
                var local = LocalTime(nowUtc, settings.TimeZone);
                result.LocalNow = local;
                if (!settings.Enabled) { result.Skip = "disabled"; return result; }
                if (!settings.WeekDays.Contains((int)local.DayOfWeek)) { result.Skip = "not_weekday"; return result; }
                if (local.TimeOfDay < settings.Time) { result.Skip = "before_time"; return result; }
                DateTime today = local.Date;

                var subs = _dal.ListSubscriptions(enabledOnly: true);
                if (SubscriptionFilter != null) subs = subs.Where(SubscriptionFilter).ToList();
                result.Subscribers = subs.Count;
                var groups = new Dictionary<string, (AiBriefScope Scope, List<(long UserId, string Lang)> To)>(StringComparer.Ordinal);
                foreach (var sub in subs)
                {
                    ct.ThrowIfCancellationRequested();
                    try
                    {
                        // تسليم اليوم تم (نبضة سابقة أو «تشغيل الآن») ⇒ لا ForUser ولا استعلام آخر.
                        if (_dal.ListDeliveries(sub.User_Id, today).Any(d => d.BriefDate.Date == today)) { result.UserSkips[sub.User_Id] = "delivered"; continue; }
                        var res = AiBriefScopes.Resolve(_users(sub.User_Id), sub);
                        if (res.Scope == null) { result.UserSkips[sub.User_Id] = res.SkipReason ?? "skip"; continue; }
                        if (!groups.TryGetValue(res.Scope.ScopeKey, out var g))
                            groups[res.Scope.ScopeKey] = g = (res.Scope, new List<(long, string)>());
                        g.To.Add((sub.User_Id, AiBriefConst.NormalizeLanguage(sub.Language)));
                    }
                    catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
                    catch (Exception ex)
                    {
                        result.UserSkips[sub.User_Id] = "error";
                        result.Errors.Add($"user {sub.User_Id}: {ex.Message}");
                        Log(ex);
                    }
                }

                result.Scopes = groups.Count;
                foreach (var (scope, to) in groups.Values)   // المستلمون بالتتابع (§3)
                {
                    ct.ThrowIfCancellationRequested();
                    try
                    {
                        var p = await ProduceAsync(scope, to, today, AiBriefTrigger.Schedule, settings, recollect: false, narrateLanguages: null, ct);
                        if (p.Collected) result.Produced++;
                        result.Deliveries += p.NewDeliveryIds.Count;
                    }
                    catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"scope {scope.ScopeKey[..8]}: {ex.Message}");
                        Log(ex);
                    }
                }
                return result;
            }
            finally { _gate.Release(); }
        }

        // ───────────── الإصدار لنطاق ─────────────

        /// <summary>
        /// يضمن موجز (today, ScopeKey) ثم يسرد ويسلّم. recollect = إعادة جمع صف موجود («تحديث الآن»/«تشغيل الآن»، الصف نفسه).
        /// narrateLanguages = لغات يُعاد سردها حتى لو وُجد سرد (للمدير)؛ وإلا تُسرد اللغات الناقصة فقط (استدعاء واحد لكل نطاق ولغة — ق25).
        /// </summary>
        public async Task<AiBriefProduceResult> ProduceAsync(AiBriefScope scope, IReadOnlyList<(long UserId, string Lang)> recipients, DateTime today,
                                                             string trigger, AiBriefSettings settings, bool recollect, IReadOnlyCollection<string>? narrateLanguages,
                                                             CancellationToken ct)
        {
            if (recipients == null || recipients.Count == 0) throw new ArgumentException("no recipients");
            var result = new AiBriefProduceResult();
            long by = recipients[0].UserId;   // ق26: أول مستلم فى النطاق
            today = today.Date;
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(ScopeTimeout);

            var brief = _dal.GetByScope(today, scope.ScopeKey);
            bool collect = false;
            AiBriefRow? previous = null;
            if (brief == null)
            {
                var (from, to) = AiBriefScopes.Period(today, _dal.LastCoveredDate(scope.ScopeKey, today));
                var row = new AiBriefRow
                {
                    BriefDate = today, PeriodFrom = from, PeriodTo = to, ScopeKey = scope.ScopeKey,
                    BranchIdsCsv = AiBriefCsv.Join(scope.BranchIds), SectionsCsv = AiBriefCsv.Join(scope.Sections),
                    TaxFilter = scope.TaxFilter, TriggeredBy = trigger,
                };
                if (_dal.TryReserve(row, by) > 0) { brief = row; collect = true; }
                else brief = _dal.GetByScope(today, scope.ScopeKey) ?? throw new InvalidOperationException("reserve lost and row not found");
            }
            if (!collect)
            {
                if (brief.Status == AiBriefStatus.Running)
                {
                    if (!_dal.TakeOverStale(brief.Id, StaleMinutes, by)) { result.Brief = brief; result.Busy = true; return result; }
                    collect = true;
                }
                else if (recollect || (brief.Status == AiBriefStatus.Failed && (brief.UpdatedAt ?? brief.CreatedAt ?? DateTime.MinValue) < DateTime.Now.AddMinutes(-FailedRetryMinutes)))
                {
                    previous = brief;
                    if (!_dal.MarkRunning(brief.Id, trigger, by)) { result.Brief = brief; result.Busy = true; return result; }
                    collect = true;
                }
            }

            if (collect)
            {
                result.Collected = true;
                try
                {
                    var r = await Task.Run(() => _collector.Collect(scope, brief.PeriodFrom, brief.PeriodTo, today, settings, by, cts.Token), cts.Token);
                    _dal.Complete(brief.Id, r.FactsJson, r.AlertsJson, r.Alerts.Count, r.ErrorsJson, r.Status(scope.FactGroups.Count), r.DurationMs, by);
                }
                catch (Exception ex)
                {
                    // لا يبقى الصف Running: إعادة جمع فاشلة تُرجع الحقائق السابقة كما هى مع الخطأ، والجمع الأول يصير Failed.
                    string err = JsonSerializer.Serialize(new Dictionary<string, string> { ["_"] = ex is OperationCanceledException ? (ct.IsCancellationRequested ? "cancelled" : "timeout") : ex.GetType().Name + ": " + ex.Message });
                    if (previous?.FactsJson != null && previous.Status is AiBriefStatus.Ready or AiBriefStatus.Partial)
                        _dal.Complete(brief.Id, previous.FactsJson, previous.AlertsJson, previous.AlertCount, err, previous.Status, previous.DurationMs ?? 0, by);
                    else
                        _dal.Complete(brief.Id, null, null, 0, err, AiBriefStatus.Failed, 0, by);
                    if (ct.IsCancellationRequested) throw;
                    if (ex is not OperationCanceledException) Log(ex);
                }
                brief = _dal.Get(brief.Id) ?? brief;
            }
            result.Brief = brief;
            if (brief.Status is not (AiBriefStatus.Ready or AiBriefStatus.Partial)) return result;   // Failed لا يُسلَّم

            // السرد: مرة لكل لغة مطلوبة فى النطاق. بعد إعادة الجمع: سرد نموذج لم يُعَد يُدقَّق على الحقائق الجديدة.
            var langs = recipients.Select(x => AiBriefConst.NormalizeLanguage(x.Lang)).Distinct().ToList();
            var force = new HashSet<string>((narrateLanguages ?? Array.Empty<string>()).Select(AiBriefConst.NormalizeLanguage));
            foreach (var lang in langs.Union(force))
            {
                var existing = _dal.GetNarrative(brief.Id, lang);
                if (existing != null && !force.Contains(lang))
                {
                    if (collect) Recheck(existing, brief, by);
                    continue;
                }
                try
                {
                    await Narrator.NarrateAndSaveAsync(brief, lang, settings, by, cts.Token);
                    result.NarratedLanguages.Add(lang);
                }
                catch (OperationCanceledException) when (!ct.IsCancellationRequested)
                {
                    // حد النطاق انتهى: الموجز يصدر بالقالب، والنبضة التالية لا تعيد السرد (لا صف) إلا لو طلبت لغة ناقصة.
                    _dal.SaveNarrative(new AiBriefNarrativeRow { Brief_Id = brief.Id, Language = lang, Source = AiBriefNarrativeSource.Error, RejectReason = "scope timeout" }, by);
                }
            }
            if (collect)
                foreach (var n in _dal.ListNarratives(brief.Id).Where(n => !langs.Contains(n.Language) && !force.Contains(n.Language)))
                    Recheck(n, brief, by);

            foreach (var (userId, lang) in recipients)
            {
                long id = _dal.AddDelivery(brief.Id, userId, AiBriefConst.NormalizeLanguage(lang), by);
                if (id > 0)
                {
                    result.NewDeliveryIds.Add(id);
                    AiBriefSignal.Raise(userId, id);
                }
            }
            return result;
        }

        /// <summary>سرد نموذج قديم لم يعد يطابق الحقائق الجديدة ⇒ rejected (الواجهة ترسم القالب).</summary>
        private void Recheck(AiBriefNarrativeRow n, AiBriefRow brief, long by)
        {
            if (n.Source != AiBriefNarrativeSource.Model || string.IsNullOrWhiteSpace(n.NarrativeText)) return;
            var facts = AiBriefCollector.ParseFacts(brief.FactsJson);
            if (facts == null) return;
            var alerts = AiBriefCollector.ParseAlerts(brief.AlertsJson);
            List<string> qs = new();
            try { qs = string.IsNullOrWhiteSpace(n.QuestionsJson) ? new() : JsonSerializer.Deserialize<List<string>>(n.QuestionsJson) ?? new(); } catch (JsonException) { }
            string payload = AiBriefNarrator.BuildPayload(facts, alerts, minimize: false).ToJsonString();
            var v = AiBriefNarrator.Verify(n.NarrativeText + "\n" + string.Join("\n", qs), facts, alerts, payload);
            if (v.Ok) return;
            n.Source = AiBriefNarrativeSource.Rejected;
            n.NarrativeText = null;
            n.QuestionsJson = null;
            string reason = "stale after refresh: " + v.Reason;
            n.RejectReason = reason.Length > 300 ? reason[..300] : reason;
            _dal.SaveNarrative(n, by);
        }

        private static void Log(Exception ex)
        {
            try { operations.DataDictionaryLogerException(ex); } catch { }
        }
    }
}
