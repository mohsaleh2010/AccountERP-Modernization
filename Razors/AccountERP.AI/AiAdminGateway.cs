using System.Diagnostics;
using AccountERP.AI.Providers;
using AccountERP.AI.Security;
using AccountERP.AI.SignIn;
using AccountModelsLib;

namespace AccountERP.AI
{
    /// <summary>
    /// تنفيذ IAiAdminGateway لشاشة /settings/ai: يشفّر المفاتيح قبل الحفظ، يقنّعها عند العرض، يطبّق قواعد العنوان ووضع «محلى فقط»،
    /// ويستدعى الـ Adapter المناسب لاختبار الاتصال وجلب النماذج. لا يسجّل أى مفتاح.
    /// </summary>
    public sealed class AiAdminGateway : IAiAdminGateway
    {
        private readonly AiProvidersCLS _cls = new();
        private readonly AiKeyProtector _keys;
        private readonly ProviderRegistry _registry;
        private readonly AiSignInStore _signInStore;
        private readonly AiSignInService _signIn;

        public AiAdminGateway(AiKeyProtector keys, ProviderRegistry registry, AiSignInStore signInStore, AiSignInService signIn)
        {
            _keys = keys;
            _registry = registry;
            _signInStore = signInStore;
            _signIn = signIn;
        }

        public List<AiProviderView> GetProviders()
        {
            var counts = _cls.GetModels().GroupBy(m => m.Provider_Id).ToDictionary(g => g.Key, g => g.Count());
            return _cls.GetProviders().Select(p =>
            {
                var extra = ProviderRegistry.ReadExtra(p.ExtraJson);
                return new AiProviderView
                {
                    Id = p.Id,
                    ProviderType = p.ProviderType,
                    DisplayName = p.DisplayName,
                    BaseUrl = p.BaseUrl ?? "",
                    HasKey = !string.IsNullOrEmpty(p.ApiKeyEnc),
                    MaskedKey = AiKeyProtector.Mask(p.ApiKeyHint),
                    HasExtraHeaders = !string.IsNullOrEmpty(p.ExtraHeadersEnc),
                    ApiProtocol = string.IsNullOrWhiteSpace(p.ApiProtocol) ? AiApiProtocols.OpenAI : p.ApiProtocol,
                    AuthHeaderName = p.AuthHeaderName ?? "",
                    AzureDeployment = extra.Deployment ?? "",
                    AzureApiVersion = extra.ApiVersion ?? "",
                    TimeoutSeconds = p.TimeoutSeconds,
                    IsCustom = p.IsCustom,
                    IsEnabled = p.IsEnabled,
                    IsLocal = p.IsLocal,
                    LastTestAt = p.LastTestAt,
                    LastTestOk = p.LastTestOk,
                    LastTestMessage = p.LastTestMessage ?? "",
                    ModelCount = counts.TryGetValue(p.Id, out int c) ? c : 0,
                    AuthMethod = p.AuthMethod ?? "",
                    SignInAt = p.SignInAt,
                };
            }).ToList();
        }

        public long SaveProvider(AiProviderSaveRequest r, long userId)
        {
            var existing = r.Id == 0 ? null : _cls.GetProvider(r.Id) ?? throw new InvalidOperationException("المزوّد غير موجود (ربما حُذف).");
            string type = existing?.ProviderType ?? (r.ProviderType ?? "").Trim().ToLowerInvariant();
            if (type != AiProviderTypes.Custom && !AiProviderTypes.BuiltIn.Contains(type))
                throw new InvalidOperationException("نوع مزوّد غير معروف.");
            bool isCustom = type == AiProviderTypes.Custom;

            string name = (r.DisplayName ?? "").Trim();
            if (name.Length == 0) throw new InvalidOperationException("أدخل اسم المزوّد.");
            if (name.Length > 200) name = name[..200];

            string baseUrlText = string.IsNullOrWhiteSpace(r.BaseUrl) ? ProviderRegistry.DefaultBaseUrl(type) : r.BaseUrl.Trim();
            if (string.IsNullOrWhiteSpace(baseUrlText))
                throw new InvalidOperationException(type == AiProviderTypes.Azure ? "أدخل عنوان Azure OpenAI (https://<resource>.openai.azure.com)." : "أدخل عنوان الـ URL.");
            var uri = AiUrlPolicy.Validate(baseUrlText);

            string protocol = isCustom ? (r.ApiProtocol ?? AiApiProtocols.OpenAI).Trim().ToLowerInvariant() : null!;
            if (isCustom && protocol != AiApiProtocols.OpenAI && protocol != AiApiProtocols.Anthropic && protocol != AiApiProtocols.Gemini)
                throw new InvalidOperationException("بروتوكول غير معروف.");

            string? authHeader = string.IsNullOrWhiteSpace(r.AuthHeaderName) ? null : r.AuthHeaderName.Trim();
            if (authHeader != null) ProviderRegistry.ValidateHeaderName(authHeader);

            string? keyEnc = existing?.ApiKeyEnc, keyHint = existing?.ApiKeyHint, authMethod = existing?.AuthMethod;
            if (r.ClearKey) { keyEnc = null; keyHint = null; authMethod = null; }
            else if (!string.IsNullOrWhiteSpace(r.ApiKey))
            {
                string key = r.ApiKey.Trim();
                keyEnc = _keys.Protect(key);
                keyHint = AiKeyProtector.Hint(key);
                authMethod = AiAuthMethods.ApiKey;
            }

            string? headersEnc = existing?.ExtraHeadersEnc;
            if (r.ClearExtraHeaders) headersEnc = null;
            else if (!string.IsNullOrWhiteSpace(r.ExtraHeaders))
            {
                var parsed = ProviderRegistry.ParseHeaders(r.ExtraHeaders);   // يرفض الأسماء غير الصالحة قبل الحفظ
                headersEnc = parsed.Count == 0 ? null : _keys.Protect(string.Join("\n", parsed.Select(h => h.Key + ": " + h.Value)));
            }

            var row = new AiProviderRow
            {
                Id = r.Id,
                ProviderType = type,
                DisplayName = name,
                BaseUrl = uri.AbsoluteUri.TrimEnd('/'),
                ApiKeyEnc = keyEnc,
                ApiKeyHint = keyHint,
                ExtraJson = type == AiProviderTypes.Azure ? ProviderRegistry.WriteExtra(r.AzureDeployment, r.AzureApiVersion) : existing?.ExtraJson,
                ApiProtocol = isCustom ? protocol : null,
                AuthHeaderName = authHeader,
                ExtraHeadersEnc = headersEnc,
                TimeoutSeconds = Math.Clamp(r.TimeoutSeconds <= 0 ? 60 : r.TimeoutSeconds, 5, 600),
                IsCustom = isCustom,
                IsEnabled = r.IsEnabled,
                // «محلى» يُحسب من العنوان لا من اختيار المستخدم، حتى لا يتجاوز مزوّد سحابى وضع «محلى فقط».
                IsLocal = AiUrlPolicy.IsLocalHost(uri),
                AuthMethod = authMethod,
            };
            return _cls.SaveProvider(row, userId);
        }

        public void DeleteProvider(long providerId) => _cls.DeleteProvider(providerId);

        public async Task<AiTestResult> TestConnectionAsync(long providerId, CancellationToken ct = default)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                var models = await ListAsync(providerId, ct);
                sw.Stop();
                var result = new AiTestResult { Ok = true, ElapsedMs = sw.ElapsedMilliseconds, ModelCount = models.Count, Message = $"تم الاتصال بنجاح ({sw.ElapsedMilliseconds} ms) — {models.Count} نموذج متاح." };
                _cls.SaveTestResult(providerId, true, result.Message);
                return result;
            }
            catch (Exception ex) when (ex is AiProviderException or InvalidOperationException)
            {
                sw.Stop();
                _cls.SaveTestResult(providerId, false, ex.Message);
                return new AiTestResult { Ok = false, ElapsedMs = sw.ElapsedMilliseconds, Message = ex.Message };
            }
        }

        public async Task<List<AiRemoteModel>> FetchModelsAsync(long providerId, CancellationToken ct = default)
        {
            try { return (await ListAsync(providerId, ct)).OrderBy(m => m.ModelId, StringComparer.OrdinalIgnoreCase).ToList(); }
            catch (AiProviderException ex) { throw new InvalidOperationException(ex.Message); }
        }

        private async Task<List<AiRemoteModel>> ListAsync(long providerId, CancellationToken ct)
        {
            var p = _cls.GetProvider(providerId) ?? throw new InvalidOperationException("المزوّد غير موجود.");
            if (!p.IsEnabled) throw new InvalidOperationException("المزوّد معطّل؛ فعّله أولاً.");
            var settings = _cls.GetSettings();
            if (settings.TryGetValue(AiSettingKeys.LocalOnly, out var lo) && lo == "1" && !p.IsLocal)
                throw new InvalidOperationException("وضع «محلى فقط» مفعّل: لا يُسمح بالاتصال بمزوّد خارج الشبكة المحلية.");
            var conn = _registry.Connect(p);
            return await _registry.AdapterFor(conn.Protocol).ListModelsAsync(conn, ct);
        }

        public List<AiModelRow> GetModels() => _cls.GetModels();

        public long SaveModel(AiModelRow m, long userId)
        {
            m.ModelId = (m.ModelId ?? "").Trim();
            if (m.ModelId.Length == 0 || m.ModelId.Length > 200) throw new InvalidOperationException("أدخل معرّف النموذج.");
            if (m.Id == 0 && _cls.GetProvider(m.Provider_Id) == null) throw new InvalidOperationException("اختر المزوّد.");
            m.DisplayName = string.IsNullOrWhiteSpace(m.DisplayName) ? m.ModelId : m.DisplayName.Trim();
            if (m.InputPricePerM < 0 || m.OutputPricePerM < 0 || m.ContextTokens < 0) throw new InvalidOperationException("القيم الرقمية لا تكون سالبة.");
            return _cls.SaveModel(m, userId);
        }

        public void DeleteModel(long modelId) => _cls.DeleteModel(modelId);

        public List<AiTaskRoutingRow> GetRouting() => _cls.GetRouting();

        public void SaveRouting(List<AiTaskRoutingRow> rows, long userId)
        {
            var models = _cls.GetModels().ToDictionary(m => m.Id);
            var clean = new List<AiTaskRoutingRow>();
            foreach (var g in rows.Where(r => r.Model_Id != 0).GroupBy(r => r.TaskKey))
            {
                if (!AiTaskKeys.All.Contains(g.Key)) throw new InvalidOperationException("مهمة غير معروفة: " + g.Key);
                int priority = 1;
                foreach (var r in g.OrderBy(x => x.Priority))
                {
                    if (!models.TryGetValue(r.Model_Id, out var model)) throw new InvalidOperationException("نموذج غير موجود فى ربط المهام.");
                    if (r.MaxTokens is < 1 or > 128000) throw new InvalidOperationException("أقصى توكنز بين 1 و128000.");
                    clean.Add(new AiTaskRoutingRow
                    {
                        TaskKey = g.Key,
                        Model_Id = r.Model_Id,
                        Priority = priority++,
                        Effort = r.Effort ?? "",
                        MaxTokens = r.MaxTokens,
                        // العيّنة (temperature) تُحفظ فقط لنموذج يدعمها؛ نماذج Claude الحالية ترفضها بخطأ 400.
                        Temperature = model.SupportsTemperature ? r.Temperature : null,
                    });
                }
            }
            _cls.ReplaceRouting(clean, userId);
        }

        public Dictionary<string, string> GetSettings() => _cls.GetSettings();

        // ───────────── P0.1: الربط بتسجيل الدخول لدى المزوّد ─────────────

        public AiSignInStart StartSignIn(long providerId, string origin, long userId)
        {
            if (!_signIn.UserAllowed(userId)) throw new InvalidOperationException("لا تملك صلاحية شاشة إعدادات الذكاء الاصطناعى.");
            var p = _cls.GetProvider(providerId) ?? throw new InvalidOperationException("المزوّد غير موجود.");
            if (!AiProviderTypes.SupportsSignIn(p.ProviderType)) throw new InvalidOperationException("هذا المزوّد لا يدعم الربط بتسجيل الدخول؛ استخدم مفتاح API.");
            var settings = _cls.GetSettings();
            if (settings.TryGetValue(AiSettingKeys.LocalOnly, out var lo) && lo == "1")
                throw new InvalidOperationException("وضع «محلى فقط» مفعّل: لا يُسمح بالربط بمزوّد سحابى.");

            // وضع الرجوع التلقائى فقط إن كان التطبيق نفسه على https أو localhost (قيود عناوين الرجوع لدى المزوّدين)، وإلا وضع لصق الكود.
            string? callback = null;
            if (Uri.TryCreate(origin, UriKind.Absolute, out var o) && (o.Scheme == Uri.UriSchemeHttps || o.IsLoopback))
                callback = o.GetLeftPart(UriPartial.Authority) + OpenRouterSignIn.CallbackPath;

            var pending = _signInStore.Create(userId, providerId);
            return new AiSignInStart
            {
                Handle = pending.Handle,
                AuthorizeUrl = OpenRouterSignIn.BuildAuthorizeUrl(pending.Verifier, pending.State, callback),
                Mode = callback == null ? "code" : "callback",
                ExpiresAtUtc = pending.ExpiresAtUtc,
            };
        }

        public AiSignInStatus GetSignInStatus(string handle, long userId) => _signInStore.GetStatus(handle, userId);

        public async Task<AiSignInStatus> CompleteSignInWithCodeAsync(string handle, string code, long userId, CancellationToken ct = default)
        {
            var pending = _signInStore.ConsumeByHandle(handle, userId);
            if (pending == null)
            {
                var st = _signInStore.GetStatus(handle, userId);
                return st.State == "done" ? st : new AiSignInStatus { State = "expired", Message = "انتهت مهلة الربط أو استُخدم الكود من قبل؛ ابدأ تسجيل الدخول من جديد." };
            }
            return await _signIn.CompleteAsync(pending, code, ct);
        }

        public void Disconnect(long providerId, long userId)
        {
            if (_cls.GetProvider(providerId) == null) throw new InvalidOperationException("المزوّد غير موجود.");
            _cls.ClearKey(providerId, userId);
        }
    }
}
