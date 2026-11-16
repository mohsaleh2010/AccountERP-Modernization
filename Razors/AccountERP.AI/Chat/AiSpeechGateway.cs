using System.Globalization;
using AccountERP.AI.Providers;
using AccountERP.AI.SignIn;
using AccountModelsLib;

namespace AccountERP.AI.Chat
{
    /// <summary>
    /// P8.11.1 (ai-assistant-p811-plan.md ص1–ص9): تفريغ الصوت عبر ربط مهمة «الصوت» (speech). الصوت لا يُخزَّن: يُرسل للمزوّد ثم يُترك للمجمّع.
    /// LocalOnly يطبّقه CandidatesFor نفسه (ص5). الحدود: المدة فى المتصفح، والحجم ≤ 5 MB هنا. المستخدم يحتاج «المساعد الذكى».
    /// </summary>
    public sealed class AiSpeechGateway : IAiSpeechGateway
    {
        public static readonly TimeSpan Timeout = TimeSpan.FromSeconds(30);
        private readonly ProviderRegistry _registry;
        private readonly AiProvidersCLS _providers = new();
        private readonly Func<long, AiServerUser> _users;

        public AiSpeechGateway(ProviderRegistry registry, Func<long, AiServerUser>? users = null)
        {
            _registry = registry;
            _users = users ?? AiUserContextFactory.ForUser;
        }

        private AiSpeechSettings Settings() => AiSpeechSettings.From(_providers.GetSettings());

        private List<AiChatGateway.Candidate> Candidates(out string reason)
        {
            var list = AiChatGateway.CandidatesFor(_providers.GetRouting(), _providers.GetModels(), _providers.GetProviders(), _providers.GetSettings(),
                                                   new[] { AiTaskKeys.Speech }, out reason);
            return list.Where(c => HttpTranscription.Supports(_registry.Connect(c.Provider).Protocol)).ToList();
        }

        public AiSpeechConfig GetConfig(AiUserContext user)
        {
            var s = Settings();
            var cfg = new AiSpeechConfig { Enabled = s.Enabled, AllowBrowser = s.AllowBrowser, MaxSeconds = s.MaxSeconds, Language = s.Language };
            if (!s.Enabled) return cfg;
            try
            {
                var c = Candidates(out string reason);
                cfg.Ready = c.Count > 0;
                cfg.Reason = cfg.Ready ? null : (reason.Length > 0 ? reason : "no_speech_route");
            }
            catch (Exception ex) { cfg.Ready = false; cfg.Reason = ex.Message; }
            return cfg;
        }

        public async Task<string> TranscribeAsync(byte[] audio, string mime, AiUserContext user, CancellationToken ct)
        {
            var s = Settings();
            if (!s.Enabled) throw new AiSpeechException("AIV_ErrDisabled", "الإدخال الصوتى موقوف.");
            if (user == null || user.UserId <= 0) throw new AiSpeechException("AIV_ErrNoAccess", "مستخدم غير معروف.");
            var server = _users(user.UserId);
            if (!server.Usable || !(server.User.IsAdmin || server.HasMenuKey("menuItemAiAssistant") || server.HasRoute("/assistant")))
                throw new AiSpeechException("AIV_ErrNoAccess", "لا تملك صلاحية المساعد الذكى.");
            if (audio == null || audio.Length < 200) throw new AiSpeechException("AIV_ErrEmpty", "التسجيل فارغ أو قصير جداً.");
            if (audio.Length > AiSpeechSettings.MaxBytes) throw new AiSpeechException("AIV_ErrTooLarge", "التسجيل أكبر من الحد المسموح.");
            if (!HttpTranscription.AllowedMime(mime)) throw new AiSpeechException("AIV_ErrFormat", "صيغة صوت غير مدعومة.");
            var candidates = Candidates(out _);
            if (candidates.Count == 0) throw new AiSpeechException("AIV_ErrNoModel", "لا يوجد نموذج مربوط بمهمة «الصوت».");
            string prompt = "فاتورة، مبيعات، مشتريات، عميل، مورد، خزينة، ميزان المراجعة، قيد، صنف، مخزن، رصيد";
            string? last = null;
            foreach (var cand in candidates)
            {
                try
                {
                    var conn = _registry.Connect(cand.Provider);
                    string text = await HttpTranscription.TranscribeAsync(conn, cand.Model.ModelId, audio, mime, s.Language, prompt, Timeout, ct);
                    text = (text ?? "").Trim();
                    if (text.Length > 4000) text = text.Substring(0, 4000);
                    return text;
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
                catch (Exception ex) when (ex is AiProviderException or InvalidOperationException or HttpRequestException or OperationCanceledException)
                {
                    last = cand.Model.DisplayName + ": " + ex.Message;
                }
            }
            throw new AiSpeechException("AIV_ErrProvider", last ?? "تعذّر التفريغ.");
        }

        private AiServerUser EnsureAdmin(AiUserContext user)
        {
            var s = _users(user?.UserId ?? 0);
            if (!s.Usable || !(s.User.IsAdmin || s.HasMenuKey(AiSignInService.MenuKey) || s.HasRoute("/settings/ai")))
                throw new AiSpeechException("AIV_ErrNotAdmin", "للمدير فقط.");
            return s;
        }

        public AiSpeechSettings GetSettings(AiUserContext user)
        {
            EnsureAdmin(user);
            return Settings();
        }

        public void SaveSettings(AiSpeechSettings v, AiUserContext user)
        {
            var s = EnsureAdmin(user);
            if (v == null) return;
            _providers.SetSetting(AiSettingKeys.SpeechEnabled, v.Enabled ? "1" : "0", s.UserId);
            _providers.SetSetting(AiSettingKeys.SpeechAllowBrowser, v.AllowBrowser ? "1" : "0", s.UserId);
            _providers.SetSetting(AiSettingKeys.SpeechMaxSeconds, Math.Max(5, Math.Min(300, v.MaxSeconds)).ToString(CultureInfo.InvariantCulture), s.UserId);
            string lang = (v.Language ?? "ar").Trim();
            _providers.SetSetting(AiSettingKeys.SpeechLanguage, lang.Length is >= 2 and <= 10 ? lang : "ar", s.UserId);
        }
    }
}
