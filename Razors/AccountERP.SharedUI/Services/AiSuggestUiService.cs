using AccountModelsLib;
using Microsoft.Extensions.DependencyInjection;

namespace AccountERP.SharedUI.Services
{
    /// <summary>
    /// P8.8.3 (ai-assistant-p88-plan.md §2): التصنيف التلقائى فى الواجهة (Scoped). غلاف رقيق فوق IAiSuggestGateway الذى يسجّله مضيف الويب فقط
    /// (MAUI: IsAvailable = false فلا شريحة). ص5: انتظار 600 مللى ث بعد آخر كتابة وإلغاء الطلب السابق لكل حقل (slot).
    /// </summary>
    public sealed class AiSuggestUiService
    {
        public static readonly TimeSpan Debounce = TimeSpan.FromMilliseconds(600);

        private readonly AssistantSessionService? _chat;
        private readonly IAiSuggestGateway? _gw;
        private readonly Dictionary<string, CancellationTokenSource> _pending = new(StringComparer.Ordinal);
        private bool? _enabled;
        private long _enabledUser;

        public AiSuggestUiService(IServiceProvider sp)
        {
            _chat = sp.GetService<AssistantSessionService>();
            _gw = sp.GetService<IAiSuggestGateway>();
        }

        /// <summary>البوابة + مستخدم + الميزة مفعّلة له (تُحسب مرة لكل دائرة ومستخدم).</summary>
        public bool IsAvailable
        {
            get
            {
                if (_gw == null || _chat == null || _chat.UserId <= 0) return false;
                if (_enabled == null || _enabledUser != _chat.UserId)
                {
                    try { _enabled = _gw.IsEnabledFor(_chat.BuildUser()); } catch { _enabled = false; }
                    _enabledUser = _chat.UserId;
                }
                return _enabled == true;
            }
        }

        public void Refresh() => _enabled = null;

        private AiUserContext User() => _chat?.BuildUser() ?? throw new InvalidOperationException("التصنيف التلقائى غير متاح.");

        /// <summary>يعيد null إن أُلغى (كتابة أحدث فى نفس الحقل) أو لا اقتراح.</summary>
        public async Task<AiSuggestion?> SuggestDebouncedAsync(string slot, AiSuggestRequest request)
        {
            if (!IsAvailable || request == null || string.IsNullOrWhiteSpace(request.Text) || request.Text.Trim().Length < 3) { Cancel(slot); return null; }
            var cts = new CancellationTokenSource();
            lock (_pending)
            {
                if (_pending.TryGetValue(slot, out var old)) { try { old.Cancel(); } catch { } }
                _pending[slot] = cts;
            }
            try
            {
                await Task.Delay(Debounce, cts.Token);
                var gw = _gw!; var user = User();
                var r = await Task.Run(async () => { AccountCultureGuard.EnsureInvariant(); return await gw.SuggestAsync(request, user, cts.Token); }, cts.Token);
                return cts.IsCancellationRequested ? null : r;
            }
            catch (OperationCanceledException) { return null; }
            catch (Exception ex)
            {
                try { operations.DataDictionaryLogerException(ex); } catch { }
                return null;
            }
            finally
            {
                lock (_pending)
                    if (_pending.TryGetValue(slot, out var cur) && ReferenceEquals(cur, cts)) _pending.Remove(slot);
            }
        }

        public void Cancel(string slot)
        {
            lock (_pending)
                if (_pending.TryGetValue(slot, out var old)) { try { old.Cancel(); } catch { } _pending.Remove(slot); }
        }

        /// <summary>تسجيل القرار (applied/dismissed/overridden) بلا انتظار.</summary>
        public void Record(long suggestionId, string outcome, string? finalJson = null)
        {
            if (_gw == null || _chat == null || suggestionId <= 0) return;
            var gw = _gw; AiUserContext user;
            try { user = User(); } catch { return; }
            _ = Task.Run(() =>
            {
                AccountCultureGuard.EnsureInvariant();
                try { gw.RecordOutcome(suggestionId, outcome, finalJson ?? "", user); } catch { }
            });
        }

        // ───────────── التبويب (المدير) ─────────────

        private static Task<T> Bg<T>(Func<T> f) => Task.Run(() => { AccountCultureGuard.EnsureInvariant(); return f(); });
        private IAiSuggestGateway Gw => _gw ?? throw new InvalidOperationException("التصنيف التلقائى غير متاح.");

        public bool HasGateway => _gw != null;

        public Task<AiSuggestSettings> GetSettingsAsync() { var gw = Gw; var u = User(); return Bg(() => gw.GetSettings(u)); }

        public async Task SaveSettingsAsync(AiSuggestSettings s)
        {
            var gw = Gw; var u = User();
            await Bg(() => { gw.SaveSettings(s, u); return true; });
            Refresh();
        }

        public Task<List<AiSuggestIndexStatus>> IndexStatusAsync() { var gw = Gw; var u = User(); return Bg(() => gw.IndexStatus(u)); }
        public Task RebuildAsync() { var gw = Gw; var u = User(); return Bg(() => { gw.RebuildIndex(u); return true; }); }
        public Task<List<AiSuggestStats>> StatsAsync(int days) { var gw = Gw; var u = User(); return Bg(() => gw.Stats(days, u)); }
    }
}
