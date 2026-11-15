using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using AccountERP.AI.Providers;
using AccountERP.AI.Scheduling;
using AccountERP.AI.SignIn;
using AccountERP.AI.Tools;
using AccountModelsLib;

namespace AccountERP.AI.Cashflow
{
    /// <summary>
    /// P8.7.2 (ai-assistant-p87-plan.md §1، و10–و17): خدمة التوقع (singleton) — الحساب، اللقطة، السرد، الدقة. تُستعمل من البوابة والأداة والمهمة والموجز.
    /// </summary>
    public sealed class AiCashflowService : IAiScheduledJob
    {
        public static readonly TimeSpan SnapshotTtl = TimeSpan.FromHours(6);
        public const int JobHour = 7;
        internal static readonly JsonSerializerOptions Json = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

        private readonly AiCashflowReader _reader;
        private readonly AiCashflowNarrator? _narrator;
        private readonly AiCashflowCLS _dal = new();
        private readonly AiProvidersCLS _providers = new();
        private DateTime _lastJobDate = DateTime.MinValue;
        private int _jobRunning;

        public string Name => "cashflow_weekly";
        public AiCashflowCLS Dal => _dal;
        public AiCashflowReader Reader => _reader;
        public Func<AiCashflowSettings>? SettingsSource { get; set; }

        public AiCashflowService(ReadOnlySqlExecutor sql, ProviderRegistry? registry)
        {
            _reader = new AiCashflowReader(sql);
            _narrator = registry == null ? null : new AiCashflowNarrator(registry);
        }

        public AiCashflowSettings Settings() => SettingsSource?.Invoke() ?? AiCashflowSettings.From(_providers.GetSettings());

        public static string BranchKey(IList<long>? branches) => branches == null ? "" : string.Join(",", branches.OrderBy(x => x).Select(x => x.ToString(CultureInfo.InvariantCulture)));

        public sealed class Result
        {
            public AiCashflowFacts Base { get; set; } = new();
            public AiCashflowFacts Conservative { get; set; } = new();
            public long RunId { get; set; }
            public DateTime? ComputedAt { get; set; }
            public string? Narrative { get; set; }
            public List<AiCashflowRecurringRow> Recurring { get; set; } = new();
        }

        /// <summary>اللقطة الحالية إن كانت حديثة (اليوم، &lt; 6 ساعات)، وإلا حساب جديد ولقطة جديدة.</summary>
        public async Task<Result> GetOrComputeAsync(AiServerUser server, IList<long>? branches, bool recompute, string? language, string source, CancellationToken ct)
        {
            string key = BranchKey(branches);
            var today = DateTime.Today;
            if (!recompute)
            {
                var last = _dal.LatestRun(server.UserId, key);
                if (last != null && last.AsOf.Date == today && last.CreatedAt.HasValue && DateTime.Now - last.CreatedAt.Value < SnapshotTtl)
                {
                    var parsed = Parse(last.FactsJson);
                    if (parsed != null)
                    {
                        parsed.RunId = last.Id; parsed.ComputedAt = last.CreatedAt; parsed.Narrative = last.Narrative;
                        if (string.IsNullOrWhiteSpace(parsed.Narrative) && language != null && _narrator != null)
                        {
                            parsed.Narrative = await _narrator.NarrateAsync(parsed.Base, parsed.Conservative, language, ct);
                            if (parsed.Narrative != null) _dal.SetNarrative(last.Id, parsed.Narrative, language);
                        }
                        return parsed;
                    }
                }
            }
            var res = await Task.Run(() => Compute(server, branches, today, ct), ct);
            if (language != null && _narrator != null) res.Narrative = await _narrator.NarrateAsync(res.Base, res.Conservative, language, ct);
            res.RunId = _dal.InsertRun(new AiCashflowRunRow
            {
                User_Id = server.UserId, BranchIds = key, AsOf = today, Scenario = "both", Source = source,
                FactsJson = Serialize(res), Narrative = res.Narrative, Language = language, Status = "Done",
            });
            res.ComputedAt = DateTime.Now;
            return res;
        }

        public Result Compute(AiServerUser server, IList<long>? branches, DateTime today, CancellationToken ct)
        {
            var s = Settings();
            var start = AiCashflowEngine.WeekStart(today);
            var adjustments = _dal.ActiveAdjustments(start, start.AddDays(7 * AiCashflowConst.Weeks), branches);
            var src = _reader.Read(server, branches, s, today, _dal.Exclusions(), adjustments, out var recurring, ct);
            return new Result
            {
                Base = AiCashflowEngine.Build(src, AiCashflowConst.ScenarioBase, s, today),
                Conservative = AiCashflowEngine.Build(src, AiCashflowConst.ScenarioConservative, s, today),
                Recurring = recurring,
            };
        }

        /// <summary>و11: لقطة الأسبوع الماضى مقابل الفعلى.</summary>
        public AiCashflowAccuracy? Accuracy(AiServerUser server, IList<long>? branches, CancellationToken ct)
        {
            var lastWeek = AiCashflowEngine.WeekStart(DateTime.Today).AddDays(-7);
            var run = _dal.RunForWeek(server.UserId, BranchKey(branches), lastWeek);
            var parsed = run == null ? null : Parse(run.FactsJson);
            if (parsed == null || parsed.Base.Weeks.Count == 0) return null;
            var w = parsed.Base.Weeks[0];
            var (ain, aout) = _reader.Actual(server, branches, run!.AsOf.Date, lastWeek.AddDays(7), ct);
            decimal fTot = w.Inflows + w.Outflows, aTot = ain + aout;
            double err = aTot == 0 ? 0 : Math.Round((double)(Math.Abs(w.Inflows - ain) + Math.Abs(w.Outflows - aout)) / (double)aTot * 100, 1);
            return new AiCashflowAccuracy { WeekStart = lastWeek, ForecastIn = w.Inflows, ActualIn = ain, ForecastOut = w.Outflows, ActualOut = aout, ErrorPct = err };
        }

        private static string Serialize(Result r) =>
            new JsonObject
            {
                ["base"] = JsonSerializer.SerializeToNode(r.Base, Json), ["conservative"] = JsonSerializer.SerializeToNode(r.Conservative, Json),
                ["recurring"] = JsonSerializer.SerializeToNode(r.Recurring, Json),
            }.ToJsonString(Json);

        public static Result? Parse(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            try
            {
                var o = JsonNode.Parse(json) as JsonObject;
                if (o == null) return null;
                return new Result
                {
                    Base = o["base"].Deserialize<AiCashflowFacts>(Json) ?? new(),
                    Conservative = o["conservative"].Deserialize<AiCashflowFacts>(Json) ?? new(),
                    Recurring = o["recurring"]?.Deserialize<List<AiCashflowRecurringRow>>(Json) ?? new(),
                };
            }
            catch (JsonException) { return null; }
        }

        // ───────────── و10: لقطة أسبوعية السبت 07:00 لكل مستخدم له لقطة خلال 30 يوماً ─────────────

        public async Task TickAsync(DateTime nowUtc, CancellationToken ct)
        {
            var local = nowUtc.ToLocalTime();
            if (local.DayOfWeek != DayOfWeek.Saturday || local.Hour < JobHour || _lastJobDate == local.Date) return;
            if (Interlocked.Exchange(ref _jobRunning, 1) == 1) return;
            try
            {
                _lastJobDate = local.Date;
                if (!Settings().Enabled) return;
                foreach (var (userId, key) in _dal.RecentUsers(30))
                {
                    ct.ThrowIfCancellationRequested();
                    try
                    {
                        var server = AiUserContextFactory.ForUser(userId);
                        if (!AiCashflowGateway.CanViewServer(server)) continue;
                        var branches = AiCashflowGateway.BranchesFor(server);
                        IList<long>? chosen = string.IsNullOrEmpty(key) ? branches
                            : key.Split(',').Select(x => long.Parse(x, CultureInfo.InvariantCulture)).Where(b => branches == null || branches.Contains(b)).ToList();
                        await GetOrComputeAsync(server, chosen, true, null, "weekly", ct);
                    }
                    catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
                    catch (Exception ex) { try { operations.DataDictionaryLogerException(ex); } catch { } }
                }
            }
            finally { Interlocked.Exchange(ref _jobRunning, 0); }
        }
    }

    /// <summary>P8.7.2: البوابة (Scoped، ويب فقط). الصلاحية بشاشة التوقع أو مفتاح القائمة أو المدير.</summary>
    public sealed class AiCashflowGateway : IAiCashflowGateway
    {
        private static readonly ConcurrentDictionary<long, (DateTime At, AiServerUser User)> Users = new();
        private readonly AiCashflowService _svc;
        private readonly Func<long, AiServerUser> _users;
        private readonly AiProvidersCLS _providers = new();

        public AiCashflowGateway(AiCashflowService svc, Func<long, AiServerUser>? users = null)
        {
            _svc = svc;
            _users = users ?? AiUserContextFactory.ForUser;
        }

        private AiServerUser Server(AiUserContext user)
        {
            if (user == null || user.UserId <= 0) throw new AiCashflowException("AIW_ErrNoAccess", "مستخدم غير معروف.");
            if (Users.TryGetValue(user.UserId, out var c) && DateTime.UtcNow - c.At < TimeSpan.FromMinutes(2)) return c.User;
            var s = _users(user.UserId);
            Users[user.UserId] = (DateTime.UtcNow, s);
            return s;
        }

        internal static bool IsAdmin(AiServerUser s) => s.Usable && (s.User.IsAdmin || s.HasMenuKey(AiSignInService.MenuKey) || s.HasRoute("/settings/ai"));

        internal static bool CanViewServer(AiServerUser s) =>
            s.Usable && (s.HasRoute(AiCashflowConst.Route) || s.HasMenuKey(AiCashflowConst.MenuKey) || IsAdmin(s));

        /// <summary>null = كل الفروع (المدير).</summary>
        internal static List<long>? BranchesFor(AiServerUser s) =>
            s.User.IsAdmin ? null : s.Run(() => new AccountModelsLib.BusinessLibShared.GeneralLedger.FinalAccountsService().GetBranches()).Select(b => b.Id).Distinct().ToList();

        private AiServerUser EnsureView(AiUserContext user)
        {
            var s = Server(user);
            if (!CanViewServer(s)) throw new AiCashflowException("AIW_ErrNoAccess", "لا تملك صلاحية توقع التدفق النقدى.");
            if (!_svc.Settings().Enabled) throw new AiCashflowException("AIW_ErrDisabled", "توقع التدفق النقدى موقوف من الإعدادات.");
            return s;
        }

        private AiServerUser EnsureAdmin(AiUserContext user)
        {
            var s = Server(user);
            if (!IsAdmin(s)) throw new AiCashflowException("AIW_ErrNotAdmin", "إعدادات التوقع للمدير فقط.");
            return s;
        }

        public bool CanView(AiUserContext user)
        {
            try { return CanViewServer(Server(user)); } catch { return false; }
        }

        public async Task<AiCashflowView> GetAsync(long branchId, bool recompute, string language, AiUserContext user, CancellationToken ct)
        {
            var s = EnsureView(user);
            var all = BranchesFor(s);
            IList<long>? branches = all;
            if (branchId > 0)
            {
                if (all != null && !all.Contains(branchId)) throw new AiCashflowException("AIW_ErrNoAccess", "فرع غير مسموح.");
                branches = new List<long> { branchId };
            }
            var r = await _svc.GetOrComputeAsync(s, branches, recompute, language, "manual", ct);
            var view = new AiCashflowView
            {
                Base = r.Base, Conservative = r.Conservative, RunId = r.RunId, ComputedAt = r.ComputedAt, Narrative = r.Narrative,
                Adjustments = _svc.Dal.ActiveAdjustments(AiCashflowEngine.WeekStart(DateTime.Today), AiCashflowEngine.WeekStart(DateTime.Today).AddDays(7 * AiCashflowConst.Weeks), branches),
            };
            try { view.Accuracy = await Task.Run(() => _svc.Accuracy(s, branches, ct), ct); } catch (Exception ex) when (ex is not OperationCanceledException) { }
            return view;
        }

        public long AddAdjustment(AiCashflowAdjustmentRow row, AiUserContext user)
        {
            var s = EnsureView(user);
            if (row == null || row.Amount <= 0) throw new AiCashflowException("AIW_ErrAmount", "المبلغ يجب أن يكون أكبر من صفر.");
            if (string.IsNullOrWhiteSpace(row.Description)) throw new AiCashflowException("AIW_ErrDescription", "الوصف مطلوب.");
            var start = AiCashflowEngine.WeekStart(DateTime.Today);
            var week = AiCashflowEngine.WeekStart(row.WeekStart);
            if (week < start || week >= start.AddDays(7 * AiCashflowConst.Weeks)) throw new AiCashflowException("AIW_ErrWeek", "الأسبوع خارج الأفق.");
            row.User_Id = s.UserId;
            row.WeekStart = week;
            row.Direction = row.Direction == "out" ? "out" : "in";
            return _svc.Dal.InsertAdjustment(row);
        }

        public bool CancelAdjustment(long id, AiUserContext user)
        {
            var s = EnsureView(user);
            return _svc.Dal.CancelAdjustment(id, s.UserId, IsAdmin(s));
        }

        public List<AiCashflowRecurringRow> Recurring(AiUserContext user)
        {
            var s = EnsureAdmin(user);
            var r = _svc.Compute(s, null, DateTime.Today, CancellationToken.None);
            return r.Recurring;
        }

        public void SetExcluded(long subAccountId, bool excluded, AiUserContext user)
        {
            var s = EnsureAdmin(user);
            _svc.Dal.SetExcluded(subAccountId, excluded, s.UserId);
        }

        public AiCashflowSettings GetSettings(AiUserContext user)
        {
            EnsureAdmin(user);
            return _svc.Settings();
        }

        public void SaveSettings(AiCashflowSettings v, AiUserContext user)
        {
            var s = EnsureAdmin(user);
            if (v == null) return;
            long by = s.UserId;
            string I(int x) => x.ToString(CultureInfo.InvariantCulture);
            _providers.SetSetting(AiSettingKeys.CashflowEnabled, v.Enabled ? "1" : "0", by);
            _providers.SetSetting(AiSettingKeys.CashflowIncludeCashSales, v.IncludeCashSales ? "1" : "0", by);
            _providers.SetSetting(AiSettingKeys.CashflowSupplierTermsDays, I(Math.Max(0, Math.Min(365, v.SupplierTermsDays))), by);
            _providers.SetSetting(AiSettingKeys.CashflowRecurringMinMonths, I(Math.Max(2, Math.Min(4, v.RecurringMinMonths))), by);
            _providers.SetSetting(AiSettingKeys.CashflowBaseDelayDays, I(Math.Max(0, Math.Min(120, v.BaseDelayDays))), by);
            _providers.SetSetting(AiSettingKeys.CashflowConservativeDelayDays, I(Math.Max(0, Math.Min(180, v.ConservativeDelayDays))), by);
            _providers.SetSetting(AiSettingKeys.CashflowConservativeCollectPct, I(Math.Max(0, Math.Min(100, v.ConservativeCollectPct))), by);
            _providers.SetSetting(AiSettingKeys.CashflowMinBalance, Math.Max(0, v.MinBalance).ToString("0.##", CultureInfo.InvariantCulture), by);
        }
    }
}
