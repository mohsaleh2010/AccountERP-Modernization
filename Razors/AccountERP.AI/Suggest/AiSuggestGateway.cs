using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using AccountERP.AI.Knowledge;
using AccountERP.AI.Providers;
using AccountERP.AI.SignIn;
using AccountModelsLib;

namespace AccountERP.AI.Suggest
{
    /// <summary>
    /// P8.8.2 (ai-assistant-p88-plan.md §1، §3): بوابة الاقتراح (Scoped، ويب فقط). الترتيب: سوابق (تام/كلمات/تضمين) ثم النموذج عند الغموض فقط.
    /// المستخدم يُبنى على الخادم (AiUserContextFactory.ForUser، مخزّن دقيقتين) والسوابق تُرشَّح بفروعه. لا كتابة إلا على AI_Suggestions.
    /// </summary>
    public sealed class AiSuggestGateway : IAiSuggestGateway
    {
        public const string AssistantMenuKey = "menuItemAiAssistant";
        public const string AssistantRoute = "/assistant";
        public const int MaxModelPerUserPerDay = 300;
        private static readonly JsonSerializerOptions Json = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
        private static readonly ConcurrentDictionary<long, (DateTime At, AiServerUser User, List<long>? Branches)> Users = new();

        private readonly AiSuggestIndex _index;
        private readonly AiSuggestPicker _picker;
        private readonly AiEmbeddingService? _embed;
        private readonly AiProvidersCLS _providers;
        private readonly Func<long, AiServerUser> _users;

        public Func<AiSuggestSettings>? SettingsSource { get; set; }
        public Func<AiServerUser, List<long>?>? BranchSource { get; set; }
        public Func<DateTime> Now { get; set; } = () => DateTime.Now;

        public AiSuggestGateway(AiSuggestIndex index, AiSuggestPicker picker, AiEmbeddingService? embed)
            : this(index, picker, embed, null, null) { }

        public AiSuggestGateway(AiSuggestIndex index, AiSuggestPicker picker, AiEmbeddingService? embed, AiProvidersCLS? providers, Func<long, AiServerUser>? users)
        {
            _index = index;
            _picker = picker;
            _embed = embed;
            _providers = providers ?? new AiProvidersCLS();
            _users = users ?? AiUserContextFactory.ForUser;
        }

        private AiSuggestSettings Settings() => SettingsSource?.Invoke() ?? AiSuggestSettings.From(_providers.GetSettings());

        // ───────────── المستخدم والصلاحيات ─────────────

        private (AiServerUser User, List<long>? Branches) Server(AiUserContext user)
        {
            if (user == null || user.UserId <= 0) throw new UnauthorizedAccessException("مستخدم غير معروف.");
            if (Users.TryGetValue(user.UserId, out var c) && DateTime.UtcNow - c.At < TimeSpan.FromMinutes(2)) return (c.User, c.Branches);
            var s = _users(user.UserId);
            List<long>? branches = null;
            if (s.Usable)
            {
                try
                {
                    branches = BranchSource != null ? BranchSource(s)
                             : s.User.IsAdmin ? null
                             : s.Run(() => new AccountModelsLib.BusinessLibShared.GeneralLedger.FinalAccountsService().GetBranches()).Select(b => b.Id).Distinct().ToList();
                }
                catch (Exception ex) { try { operations.DataDictionaryLogerException(ex); } catch { } branches = new List<long> { user.BranchId }; }
            }
            Users[user.UserId] = (DateTime.UtcNow, s, branches);
            return (s, branches);
        }

        private static bool IsAdmin(AiServerUser s) =>
            s.Usable && (s.User.IsAdmin || s.HasMenuKey(AiSignInService.MenuKey) || s.HasRoute("/settings/ai"));

        private static bool HasAssistant(AiServerUser s) =>
            s.Usable && (s.User.IsAdmin || s.HasMenuKey(AssistantMenuKey) || s.HasRoute(AssistantRoute));

        private AiServerUser EnsureAdmin(AiUserContext user)
        {
            var s = Server(user).User;
            if (!IsAdmin(s)) throw new UnauthorizedAccessException("هذه الإعدادات للمدير فقط.");
            return s;
        }

        public bool IsEnabledFor(AiUserContext user)
        {
            try
            {
                var st = Settings();
                if (!st.Enabled || user == null || user.UserId <= 0) return false;
                var s = Server(user).User;
                return s.Usable && (st.ForAllUsers || HasAssistant(s));
            }
            catch (Exception) { return false; }
        }

        // ───────────── الاقتراح ─────────────

        public async Task<AiSuggestion?> SuggestAsync(AiSuggestRequest request, AiUserContext user, CancellationToken ct)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Text)) return null;
            string kind = request.Kind == AiSuggestKinds.Item ? AiSuggestKinds.Item : AiSuggestKinds.Expense;
            string text = request.Text.Trim();
            if (text.Length > 400) text = text.Substring(0, 400);
            if (AiSuggestText.Norm(text).Length < 3) return null;

            var st = Settings();
            if (!st.Enabled) return null;
            var (server, branches) = Server(user);
            if (!server.Usable || !(st.ForAllUsers || HasAssistant(server))) return null;

            var corpus = await Task.Run(() => _index.Get(kind, st), ct);
            if (corpus.Items.Count == 0) return null;

            Func<AiSuggestPrecedent, bool> filter = kind == AiSuggestKinds.Expense
                ? p => (branches == null || p.BranchId == 0 || branches.Contains(p.BranchId))
                       && (request.SubType == 0 || (p.SubType == 1) == (request.SubType == 1))
                : p => request.SubType == 0 || p.SubType == request.SubType;

            // التضمين اختيارى (مهلة 3 ث داخل الخدمة؛ أى فشل = بدونه)
            Dictionary<string, double>? sims = null;
            if (_embed != null)
            {
                try
                {
                    var (vec, modelId) = await _embed.EmbedQueryAsync(AiSuggestText.Norm(text), ct);
                    if (vec != null && modelId > 0) sims = AiSuggestIndex.Similar(vec, _index.Vectors(kind, modelId));
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
                catch (Exception) { sims = null; }
            }

            var ranked = AiSuggestRanker.Rank(text, corpus, Now(), filter, sims, 8);
            if (kind == AiSuggestKinds.Expense && ranked.Count > 0)
            {
                // ص10: لا اقتراح لحساب محذوف
                var exists = _index.Dal.ExistingSubAccounts(ranked.Select(r => r.TargetId));
                ranked = ranked.Where(r => exists.Contains(r.TargetId)).ToList();
                double tot = ranked.Sum(r => r.Score);
                foreach (var r in ranked) r.Share = tot <= 0 ? 0 : r.Score / tot;
            }
            if (ranked.Count == 0) return null;

            AiSuggestCandidate best = ranked[0];
            string source = ranked[0].ExactHit ? AiSuggestSources.Exact : AiSuggestSources.Neighbors;
            double confidence = AiSuggestRanker.Confidence(ranked);
            string? why = null;

            if (AiSuggestRanker.IsAmbiguous(ranked, st.AmbiguityMargin) && st.UseModel && ranked.Count >= 2
                && _index.Dal.CountModelToday(user.UserId) < MaxModelPerUserPerDay)
            {
                bool minimize = _providers.GetSettings().TryGetValue(AiSettingKeys.DataMinimization, out var dm) ? dm != "0" : true;
                var pick = await _picker.PickAsync(kind, text, ranked, minimize, ct);
                if (pick != null)
                {
                    best = ranked[pick.Index];
                    source = AiSuggestSources.Model;
                    why = pick.Why;
                    // ثقة النموذج: نصيب المرشح + نصف المتبقى حتى سقف 0.85 (اختيار بين متقاربين)
                    confidence = Math.Round(Math.Min(0.85, best.Share + (1 - best.Share) / 2), 4);
                }
            }
            if (confidence < st.MinConfidence) return null;

            var sug = new AiSuggestion
            {
                Kind = kind, TargetId = best.TargetId, TargetCode = best.TargetCode, TargetName = best.TargetName,
                SecondaryId = best.SecondaryId, SecondaryName = best.SecondaryName, Confidence = confidence, Source = source, Why = why,
                Evidence = best.Evidence,
            };
            try
            {
                sug.Id = _index.Dal.Insert(new AiSuggestionRow
                {
                    Kind = kind, User_Id = user.UserId, Branch_Id = request.BranchId, InputText = text, InputHash = AiSuggestText.Hash(AiSuggestText.Norm(text)),
                    SuggestedJson = JsonSerializer.Serialize(new { target = best.TargetId, code = best.TargetCode, name = best.TargetName, secondary = best.SecondaryId }, Json),
                    Source = source, Confidence = (decimal)confidence, Outcome = AiSuggestOutcomes.Shown, Screen = request.Screen,
                });
            }
            catch (Exception ex) { try { operations.DataDictionaryLogerException(ex); } catch { } }
            return sug;
        }

        public bool RecordOutcome(long suggestionId, string outcome, string finalJson, AiUserContext user)
        {
            if (suggestionId <= 0 || user == null || user.UserId <= 0) return false;
            try { return _index.Dal.RecordOutcome(suggestionId, user.UserId, outcome, finalJson); }
            catch (Exception ex) { try { operations.DataDictionaryLogerException(ex); } catch { } return false; }
        }

        // ───────────── الإعدادات والتبويب (للمدير) ─────────────

        public AiSuggestSettings GetSettings(AiUserContext user)
        {
            EnsureAdmin(user);
            return Settings();
        }

        public void SaveSettings(AiSuggestSettings settings, AiUserContext user)
        {
            var s = EnsureAdmin(user);
            if (settings == null) return;
            long by = s.UserId;
            _providers.SetSetting(AiSettingKeys.SuggestEnabled, settings.Enabled ? "1" : "0", by);
            _providers.SetSetting(AiSettingKeys.SuggestUseModel, settings.UseModel ? "1" : "0", by);
            _providers.SetSetting(AiSettingKeys.SuggestForAllUsers, settings.ForAllUsers ? "1" : "0", by);
            _providers.SetSetting(AiSettingKeys.SuggestMinConfidence, Math.Max(0, Math.Min(1, settings.MinConfidence)).ToString("0.##", CultureInfo.InvariantCulture), by);
            _providers.SetSetting(AiSettingKeys.SuggestHistoryMonths, Math.Max(1, Math.Min(120, settings.HistoryMonths)).ToString(CultureInfo.InvariantCulture), by);
            _providers.SetSetting(AiSettingKeys.SuggestMaxRows, Math.Max(100, Math.Min(500000, settings.MaxRows)).ToString(CultureInfo.InvariantCulture), by);
            Users.Clear();
        }

        public List<AiSuggestIndexStatus> IndexStatus(AiUserContext user)
        {
            EnsureAdmin(user);
            return new List<AiSuggestIndexStatus> { _index.Status(AiSuggestKinds.Expense), _index.Status(AiSuggestKinds.Item) };
        }

        public void RebuildIndex(AiUserContext user)
        {
            EnsureAdmin(user);
            _index.Invalidate();
            var st = Settings();
            _index.Get(AiSuggestKinds.Expense, st);
            _index.Get(AiSuggestKinds.Item, st);
        }

        public List<AiSuggestStats> Stats(int days, AiUserContext user)
        {
            EnsureAdmin(user);
            return _index.Dal.Stats(days);
        }
    }
}
