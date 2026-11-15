using System.Globalization;
using AccountERP.AI.Providers;
using AccountERP.AI.SignIn;
using AccountModelsLib;
using AccountModelsLib.SalesRep;

namespace AccountERP.AI.Collections
{
    /// <summary>
    /// P8.5.2/P8.5.3 — بوابة أولويات التحصيل (Scoped، ويب فقط — ت33). الصلاحيات تُبنى على الخادم بـ AiUserContextFactory.ForUser:
    /// العرض بشاشة /accounts/collections-priority (ت17)، الفروع والصلاحية الضريبية من جلسة المستخدم (ت15، ت16)، والإعدادات للمدير.
    /// الكتابة على AI_CollectionActions/Drafts فقط. الأخطاء AiCollectException بمفتاح resx.
    /// </summary>
    public sealed partial class AiCollectionsGateway : IAiCollectionsGateway
    {
        private readonly AiCollectionsScorer _scorer;
        private readonly ProviderRegistry? _registry;
        private readonly AiCollectCLS _dal;
        private readonly AiProvidersCLS _providers;
        private readonly Func<long, AiServerUser> _users;
        private readonly AiToolAuditCLS _audit = new();

        public Action<string, string, long>? SettingWriter { get; set; }
        /// <summary>اختبار: فروع المستخدم (null = كل الفروع).</summary>
        public Func<AiServerUser, List<long>?>? BranchSource { get; set; }
        /// <summary>اختبار: الصلاحية الضريبية (-1/0/1).</summary>
        public Func<AiServerUser, int>? TaxSource { get; set; }
        public Func<DateTime> Today { get; set; } = () => DateTime.Today;
        /// <summary>اختبار: إعدادات طابور الإشعارات (المزوّد وكود الدولة) بدل SalesRep_Notify_Settings.</summary>
        public Func<SalesRepNotifyService.SettingsView>? NotifySettingsSource { get; set; }
        /// <summary>اختبار: إعدادات بدل AI_Settings.</summary>
        public Func<AiCollectSettings>? SettingsSource { get; set; }

        public AiCollectionsGateway(AiCollectionsScorer scorer, ProviderRegistry registry) : this(scorer, registry, null, null, null) { }

        public AiCollectionsGateway(AiCollectionsScorer scorer, ProviderRegistry? registry, AiCollectCLS? dal, AiProvidersCLS? providers, Func<long, AiServerUser>? users)
        {
            _scorer = scorer;
            _registry = registry;
            _dal = dal ?? scorer.Dal;
            _providers = providers ?? new AiProvidersCLS();
            _users = users ?? AiUserContextFactory.ForUser;
        }

        public AiCollectSettings Settings() => SettingsSource?.Invoke() ?? AiCollectSettings.From(_providers.GetSettings());

        // ───────────── العرض ─────────────

        public AiCollectSummary Summary(AiUserContext user)
        {
            var server = EnsureView(user);
            var s = Settings();
            var res = _scorer.Compute(Scope(server), s, false, CancellationToken.None);
            var today = Today().Date;
            var high = res.Rows.Where(r => r.Priority == AiCollectPriority.High).ToList();
            var sum = new AiCollectSummary
            {
                OverdueTotal = res.Rows.Sum(r => r.Overdue), OverdueCustomers = res.Rows.Count(r => r.Overdue > AiCollectRules.Tolerance),
                HighCount = high.Count, HighAmount = high.Sum(r => r.Overdue),
                PromisesDueToday = res.Rows.Count(r => r.State == AiCollectState.Promised && r.PromiseDate.HasValue && r.PromiseDate.Value.Date == today),
                BrokenPromises = res.Rows.Count(r => r.State == AiCollectState.Broken),
                Buckets = new[] { res.Rows.Sum(r => r.NotDue), res.Rows.Sum(r => r.D1_30), res.Rows.Sum(r => r.D31_60), res.Rows.Sum(r => r.D61_90), res.Rows.Sum(r => r.DOver90), res.Rows.Sum(r => r.Unallocated) },
                MixedCurrency = res.Rows.Select(r => r.CurrencyId ?? 0).Distinct().Count() > 1,
                DraftAvailable = s.Draft, TaxRestricted = res.TaxRestricted, Truncated = res.Truncated, ComputedAt = res.ComputedAt,
                Weights = (decimal[])s.Weights.Clone(), BrokenPromiseBonus = s.BrokenPromiseBonus, MaxCustomers = s.MaxCustomers, ReminderMinDays = s.ReminderMinDays,
            };
            sum.SendAvailable = s.SendEnabled && NotifyProvider() != (int)SalesRepNotifyProvider.Off;
            if (sum.SendAvailable) sum.SendChannels = new List<int> { (int)SalesRepNotifyChannel.Sms, (int)SalesRepNotifyChannel.WhatsApp };
            return sum;
        }

        public AiCollectPage List(AiCollectFilter filter, bool refresh, AiUserContext user)
        {
            var server = EnsureView(user);
            filter ??= new AiCollectFilter();
            var scope = Scope(server);
            if (filter.BranchId != 0)
            {
                if (scope.BranchIds != null && !scope.BranchIds.Contains(filter.BranchId)) return new AiCollectPage();
                scope.BranchIds = new List<long> { filter.BranchId };
            }
            var res = _scorer.Compute(scope, Settings(), refresh, CancellationToken.None);
            IEnumerable<AiCollectRow> q = res.Rows;
            string state = (filter.State ?? "").Trim();
            if (state == AiCollectState.Due) q = q.Where(r => r.State == AiCollectState.Due || r.State == AiCollectState.Broken);
            else if (state.Length > 0 && state != "all") q = q.Where(r => r.State == state);
            if (!string.IsNullOrWhiteSpace(filter.Priority)) q = q.Where(r => r.Priority == filter.Priority.Trim());
            if (filter.OverdueOnly) q = q.Where(r => r.Overdue > AiCollectRules.Tolerance);
            if (filter.MinOverdue > 0) q = q.Where(r => r.Overdue >= filter.MinOverdue);
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                string t = filter.Search.Trim();
                q = q.Where(r => (r.Name ?? "").Contains(t, StringComparison.OrdinalIgnoreCase) || (r.Code ?? "").Contains(t, StringComparison.OrdinalIgnoreCase));
            }
            var all = q.ToList();
            int p = Math.Max(0, filter.Page);
            return new AiCollectPage
            {
                Items = all.Skip(p * AiCollectConst.PageSize).Take(AiCollectConst.PageSize).ToList(), Total = all.Count,
                Truncated = res.Truncated, TaxRestricted = res.TaxRestricted, ComputedAt = res.ComputedAt,
            };
        }

        public AiCollectDetail GetCustomer(long customerId, AiUserContext user)
        {
            var server = EnsureView(user);
            var s = Settings();
            var scope = Scope(server);
            var row = Find(server, scope, s, customerId);
            var d = new AiCollectDetail { Customer = row, Actions = _dal.ListForCustomer(customerId, 50) };
            if (server.HasRoute(AiCollectConst.SalesRoute)) d.Invoices = _scorer.Invoices(customerId, row.Balance, scope, s, CancellationToken.None);
            else d.InvoicesHidden = true;
            return d;
        }

        // ───────────── الإجراءات (P8.5.3) ─────────────

        public long AddAction(AiCollectActionInput input, AiUserContext user)
        {
            var server = EnsureView(user);
            if (input == null || !AiCollectActionTypes.IsUserType(input.ActionType))
                throw new AiCollectException(AiCollectErrors.ActionType, "نوع إجراء غير معروف.");
            var today = Today().Date;
            string? note = string.IsNullOrWhiteSpace(input.Note) ? null : input.Note.Trim();
            if (note != null && note.Length > AiCollectConst.NoteMaxChars) throw new AiCollectException(AiCollectErrors.NoteLength, "الملاحظة أطول من المسموح.");
            var row = Find(server, Scope(server), Settings(), input.CustomerId);
            var a = new AiCollectActionRow
            {
                Customer_Id = input.CustomerId, ActionType = input.ActionType, Note = note,
                BalanceAt = row.Balance, OverdueAt = row.Overdue, OldestDaysAt = row.OldestDays, ScoreAt = row.Score, DunningLevel = row.Level,
                Status = AiCollectActionStatus.Open,
            };
            switch (input.ActionType)
            {
                case AiCollectActionTypes.Promise:
                    if (!input.PromiseDate.HasValue || input.PromiseDate.Value.Date < today) throw new AiCollectException(AiCollectErrors.PromiseDate, "تاريخ الوعد يجب ألا يسبق اليوم.");
                    if (input.PromiseAmount.HasValue && (input.PromiseAmount.Value <= 0 || input.PromiseAmount.Value > row.Balance))
                        throw new AiCollectException(AiCollectErrors.PromiseAmount, "مبلغ الوعد يجب أن يكون أكبر من صفر ولا يتجاوز الرصيد.");
                    a.PromiseDate = input.PromiseDate.Value.Date;
                    a.PromiseAmount = input.PromiseAmount;
                    break;
                case AiCollectActionTypes.Snooze:
                    if (!input.NextActionDate.HasValue || input.NextActionDate.Value.Date <= today || input.NextActionDate.Value.Date > today.AddDays(AiCollectConst.MaxSnoozeDays))
                        throw new AiCollectException(AiCollectErrors.SnoozeDate, "تاريخ التأجيل بعد اليوم وخلال 90 يوماً.");
                    a.NextActionDate = input.NextActionDate.Value.Date;
                    break;
                case AiCollectActionTypes.Call:
                case AiCollectActionTypes.Note:
                    a.Status = AiCollectActionStatus.Closed;   // سجل لا يبقى «مفتوحاً»
                    break;
            }
            long id = _dal.InsertAction(a, server.UserId);
            if (a.ActionType is AiCollectActionTypes.Promise or AiCollectActionTypes.Snooze or AiCollectActionTypes.Dispute)
                _dal.CloseOpenOfType(input.CustomerId, a.ActionType, id, server.UserId);
            _scorer.Invalidate();
            return id;
        }

        /// <summary>cancel: لمن أنشأه أو المدير. إنهاء (Closed): لأى صاحب صلاحية.</summary>
        public void CloseAction(long actionId, bool cancel, AiUserContext user)
        {
            var server = EnsureView(user);
            var a = _dal.GetAction(actionId) ?? throw new AiCollectException(AiCollectErrors.NotFound, "الإجراء غير موجود.");
            Find(server, Scope(server), Settings(), a.Customer_Id);   // العميل ضمن نطاق المستخدم
            if (cancel && a.CreatedBy != server.UserId && !IsAdmin(server))
                throw new AiCollectException(AiCollectErrors.CancelNotAllowed, "الإلغاء لمن سجّل الإجراء أو للمدير.");
            if (!_dal.CloseAction(actionId, cancel ? AiCollectActionStatus.Cancelled : AiCollectActionStatus.Closed, server.UserId))
                throw new AiCollectException(AiCollectErrors.NotFound, "الإجراء ليس مفتوحاً.");
            _scorer.Invalidate();
        }

        // ───────────── الإدارة ─────────────

        public AiCollectSettings GetSettings(AiUserContext admin)
        {
            EnsureAdmin(admin);
            return Settings();
        }

        public void SaveSettings(AiCollectSettings settings, AiUserContext admin)
        {
            long uid = EnsureAdmin(admin);
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            var errors = settings.Validate();
            if (errors.Count > 0) throw new AiCollectException(errors[0], "إعدادات غير صالحة: " + string.Join(", ", errors));
            var write = SettingWriter ?? ((k, v, by) => _providers.SetSetting(k, v, by));
            foreach (var p in settings.ToPairs()) write(p.Key, p.Value, uid);
            _scorer.Invalidate();
        }

        // ───────────── مساعدات ─────────────

        public AiCollectScope Scope(AiServerUser server) => new()
        {
            UserId = server.UserId, IsAdmin = server.User.IsAdmin,
            BranchIds = BranchSource != null ? BranchSource(server)
                      : server.User.IsAdmin ? null
                      : server.Run(() => new AccountModelsLib.BusinessLibShared.GeneralLedger.FinalAccountsService().GetBranches()).Select(b => b.Id).Distinct().ToList(),
            TaxFilter = TaxSource != null ? TaxSource(server) : server.TaxFilter,
        };

        private AiCollectRow Find(AiServerUser server, AiCollectScope scope, AiCollectSettings s, long customerId) =>
            _scorer.Compute(scope, s, false, CancellationToken.None).Rows.FirstOrDefault(r => r.CustomerId == customerId)
            ?? throw new AiCollectException(AiCollectErrors.NotFound, "العميل ليس فى قائمة التحصيل.");

        private int NotifyProvider()
        {
            try { return (NotifySettingsSource?.Invoke() ?? new SalesRepNotifyService().GetSettings()).Provider; }
            catch (Exception ex) { try { operations.DataDictionaryLogerException(ex); } catch { } return (int)SalesRepNotifyProvider.Off; }
        }

        private static long Uid(AiUserContext user) =>
            user != null && user.UserId > 0 ? user.UserId : throw new AiCollectException(AiCollectErrors.NoPermission, "لا يوجد مستخدم.");

        private static bool IsAdmin(AiServerUser s) =>
            s.Usable && (s.User.IsAdmin || s.HasMenuKey(AiSignInService.MenuKey) || s.HasRoute("/settings/ai"));

        private AiServerUser EnsureView(AiUserContext user)
        {
            var server = _users(Uid(user));
            if (!server.Usable || !(server.HasRoute(AiCollectConst.Route) || server.HasMenuKey(AiCollectConst.MenuKey)))
                throw new AiCollectException(AiCollectErrors.NoPermission, "لا تملك صلاحية شاشة «أولويات التحصيل».");
            return server;
        }

        private long EnsureAdmin(AiUserContext user)
        {
            long uid = Uid(user);
            if (!IsAdmin(_users(uid))) throw new AiCollectException(AiCollectErrors.NotAdmin, "إعدادات أولويات التحصيل للمدير فقط.");
            return uid;
        }
    }
}
