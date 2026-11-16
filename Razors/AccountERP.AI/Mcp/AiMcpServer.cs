using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using AccountERP.AI.SignIn;
using AccountERP.AI.Tools;
using AccountModelsLib;

namespace AccountERP.AI.Mcp
{
    /// <summary>
    /// P8.10.2 (ai-assistant-p810-plan.md م1، م2، م5–م9): خادم MCP للقراءة فقط — معالج JSON-RPC 2.0 يدوى صغير (Streamable HTTP، استجابة JSON واحدة بلا SSE):
    /// initialize، notifications/*، ping، tools/list، tools/call. الأدوات = قائمة سماح ثابتة ∩ ما يسمح به AiToolRegistry للمستخدم صاحب التوكن.
    /// قرار: لا حزمة ModelContextProtocol (فحص 1 لم يُنفَّذ) — الطرق الخمس كافية ومعروفة الحدود.
    /// </summary>
    public sealed class AiMcpServer
    {
        private static readonly ConcurrentDictionary<long, (DateTime At, AiServerUser User)> Users = new();
        private static readonly ConcurrentDictionary<long, DateTime> Touched = new();
        private readonly AiToolRegistry _tools;
        private readonly AiApiTokensCLS _tokens = new();
        private readonly AiProvidersCLS _providers = new();
        private readonly AiToolAuditCLS _audit = new();

        public Func<long, AiServerUser> UserFactory { get; set; } = AiUserContextFactory.ForUser;

        public AiMcpServer(AiToolRegistry tools) => _tools = tools;

        public AiMcpSettings Settings() => AiMcpSettings.From(_providers.GetSettings());

        /// <summary>م5: التوكن صالح ونشط ⇒ صفه؛ وإلا null مع سبب.</summary>
        public AiApiTokenRow? Authenticate(string? bearer, out string reason)
        {
            reason = "";
            string t = (bearer ?? "").Trim();
            if (!AiMcpTokenText.LooksValid(t)) { reason = "invalid_token"; return null; }
            var row = _tokens.FindByHash(AiMcpTokenText.Hash(t));
            if (row == null) { reason = "invalid_token"; return null; }
            if (row.RevokedAt != null) { reason = "revoked"; return null; }
            if (row.ExpiresAt <= DateTime.Now) { reason = "expired"; return null; }
            return row;
        }

        private AiServerUser? ServerUser(long userId)
        {
            if (Users.TryGetValue(userId, out var c) && DateTime.UtcNow - c.At < TimeSpan.FromSeconds(60)) return c.User;
            var s = UserFactory(userId);
            Users[userId] = (DateTime.UtcNow, s);
            return s;
        }

        private static bool HasAssistant(AiServerUser s) =>
            s.Usable && (s.User.IsAdmin || s.HasMenuKey("menuItemAiAssistant") || s.HasRoute("/assistant"));

        /// <summary>طلب واحد. null = إشعار (لا رد؛ HTTP 202).</summary>
        public async Task<JsonObject?> HandleAsync(JsonNode? request, AiApiTokenRow token, string? ip, CancellationToken ct)
        {
            if (request is not JsonObject req) return Error(null, -32600, "Invalid Request");
            JsonNode? id = req["id"]?.DeepClone();
            string method = req["method"] is JsonValue mv && mv.TryGetValue<string>(out var m) ? m : "";
            bool notification = !req.ContainsKey("id");
            if (method.StartsWith("notifications/", StringComparison.Ordinal) || notification) return null;

            // آخر استعمال: مرة فى الدقيقة على الأكثر لكل توكن
            if (!Touched.TryGetValue(token.Id, out var last) || DateTime.UtcNow - last > TimeSpan.FromMinutes(1))
            {
                Touched[token.Id] = DateTime.UtcNow;
                try { _tokens.Touch(token.Id, ip); } catch { }
            }

            var server = ServerUser(token.User_Id);
            if (server == null || !HasAssistant(server)) return Error(id, -32001, "User is inactive or has no AI assistant permission.");

            switch (method)
            {
                case "initialize":
                    return Result(id, new JsonObject
                    {
                        ["protocolVersion"] = req["params"]?["protocolVersion"]?.GetValue<string>() is { Length: > 0 } pv ? pv : AiMcpConst.ProtocolVersion,
                        ["capabilities"] = new JsonObject { ["tools"] = new JsonObject { ["listChanged"] = false } },
                        ["serverInfo"] = new JsonObject { ["name"] = AiMcpConst.ServerName, ["version"] = "1.0" },
                        ["instructions"] = "Read-only access to this ERP with the token owner's permissions. Amounts follow the ERP screens. No writes are possible.",
                    });
                case "ping":
                    return Result(id, new JsonObject());
                case "tools/list":
                {
                    var ctx = Context(server, ct);
                    var list = new JsonArray();
                    foreach (var d in Allowed(ctx, token))
                        list.Add(new JsonObject { ["name"] = d.Name, ["description"] = d.Description, ["inputSchema"] = d.Schema?.DeepClone() ?? new JsonObject { ["type"] = "object" } });
                    return Result(id, new JsonObject { ["tools"] = list });
                }
                case "tools/call":
                {
                    string name = req["params"]?["name"] is JsonValue nv && nv.TryGetValue<string>(out var n) ? n : "";
                    var args = req["params"]?["arguments"] as JsonObject ?? new JsonObject();
                    var ctx = Context(server, ct);
                    if (!Allowed(ctx, token).Any(d => d.Name == name)) return Error(id, -32602, "Unknown or not allowed tool: " + name);
                    var sw = Stopwatch.StartNew();
                    AiToolResult res;
                    try
                    {
                        using (UserSessionContext.Use(server.Session))
                            res = await _tools.ExecuteAsync(name, args.ToJsonString(), ctx);
                    }
                    catch (OperationCanceledException) { throw; }
                    catch (Exception ex)
                    {
                        res = new AiToolResult { Content = ex.Message, IsError = true, Outcome = AiToolOutcome.Error };
                    }
                    try
                    {
                        _audit.Insert(null, server.UserId, "mcp:" + name, Cut(args.ToJsonString(), 20000), res.Sql, res.RowCount, (int)sw.ElapsedMilliseconds,
                                      res.IsError ? AiToolOutcome.Error : res.Outcome, res.IsError ? Cut(res.Content, 2000) : null);
                    }
                    catch { }
                    var result = new JsonObject
                    {
                        ["content"] = new JsonArray(new JsonObject { ["type"] = "text", ["text"] = res.Content }),
                        ["isError"] = res.IsError,
                    };
                    if (!res.IsError && TryParseObject(res.Content) is JsonObject structured) result["structuredContent"] = structured;
                    return Result(id, result);
                }
                default:
                    return Error(id, -32601, "Method not found: " + method);
            }
        }

        private static AiToolContext Context(AiServerUser server, CancellationToken ct)
        {
            var probe = new AiToolContext { User = server.User, AllowedViews = new HashSet<string>() };
            return new AiToolContext { User = server.User, AllowedViews = AiToolContext.ViewsFor(server.User, probe.HasRoute), TaxFilter = server.TaxFilter, Ct = ct };
        }

        private IEnumerable<Chat.AiToolDef> Allowed(AiToolContext ctx, AiApiTokenRow token)
        {
            var s = Settings();
            bool sql = s.AllowSqlScope && token.HasScope(AiMcpConst.ScopeSql);
            return _tools.ToolsFor(ctx).Where(d => AiMcpConst.AllowList.Contains(d.Name) && (d.Name != "run_readonly_query" || sql));
        }

        private static JsonObject? TryParseObject(string? s)
        {
            if (string.IsNullOrWhiteSpace(s) || s.TrimStart()[0] != '{') return null;
            try { return JsonNode.Parse(s) as JsonObject; } catch (JsonException) { return null; }
        }

        public static JsonObject Result(JsonNode? id, JsonNode result) => new() { ["jsonrpc"] = "2.0", ["id"] = id, ["result"] = result };

        public static JsonObject Error(JsonNode? id, int code, string message) =>
            new() { ["jsonrpc"] = "2.0", ["id"] = id, ["error"] = new JsonObject { ["code"] = code, ["message"] = message } };

        private static string? Cut(string? s, int max) => s == null ? null : (s.Length > max ? s.Substring(0, max) : s);
    }

    /// <summary>P8.10.3: إدارة التوكنات للواجهة (Scoped، ويب فقط).</summary>
    public sealed class AiMcpGateway : IAiMcpGateway
    {
        private readonly AiApiTokensCLS _tokens = new();
        private readonly AiProvidersCLS _providers = new();
        private readonly Func<long, AiServerUser> _users;

        public AiMcpGateway(Func<long, AiServerUser>? users = null) => _users = users ?? AiUserContextFactory.ForUser;

        private AiServerUser Server(AiUserContext user)
        {
            if (user == null || user.UserId <= 0) throw new AiMcpException("AIM_ErrNoAccess", "مستخدم غير معروف.");
            return _users(user.UserId);
        }

        private static bool IsAdmin(AiServerUser s) => s.Usable && (s.User.IsAdmin || s.HasMenuKey(AiSignInService.MenuKey) || s.HasRoute("/settings/ai"));
        private static bool Assistant(AiServerUser s) => s.Usable && (s.User.IsAdmin || s.HasMenuKey("menuItemAiAssistant") || s.HasRoute("/assistant"));

        private AiServerUser EnsureAdmin(AiUserContext u)
        {
            var s = Server(u);
            if (!IsAdmin(s)) throw new AiMcpException("AIM_ErrNotAdmin", "للمدير فقط.");
            return s;
        }

        private AiMcpSettings Settings() => AiMcpSettings.From(_providers.GetSettings());

        public bool HasAssistant(AiUserContext user) { try { return Assistant(Server(user)); } catch { return false; } }

        public AiMcpSettings GetSettings(AiUserContext user)
        {
            Server(user);
            return Settings();
        }

        public void SaveSettings(AiMcpSettings v, AiUserContext user)
        {
            var s = EnsureAdmin(user);
            if (v == null) return;
            _providers.SetSetting(AiSettingKeys.McpEnabled, v.Enabled ? "1" : "0", s.UserId);
            _providers.SetSetting(AiSettingKeys.McpAllowSqlScope, v.AllowSqlScope ? "1" : "0", s.UserId);
            _providers.SetSetting(AiSettingKeys.McpTokenMaxDays, Math.Max(1, Math.Min(3650, v.TokenMaxDays)).ToString(System.Globalization.CultureInfo.InvariantCulture), s.UserId);
        }

        public AiApiTokenCreated CreateToken(string name, int days, bool sqlScope, AiUserContext user)
        {
            var s = Server(user);
            var st = Settings();
            if (!st.Enabled) throw new AiMcpException("AIM_ErrDisabled", "الوصول الخارجى (MCP) موقوف.");
            if (!Assistant(s)) throw new AiMcpException("AIM_ErrNoAccess", "لا تملك صلاحية المساعد الذكى.");
            name = (name ?? "").Trim();
            if (name.Length == 0) throw new AiMcpException("AIM_ErrName", "اسم التوكن مطلوب.");
            if (days < 1 || days > st.TokenMaxDays) throw new AiMcpException("AIM_ErrDays", "مدة غير مسموحة.");
            if (_tokens.ActiveCount(s.UserId) >= AiMcpConst.MaxActivePerUser) throw new AiMcpException("AIM_ErrMax", "وصلت للحد الأقصى من التوكنات النشطة.");
            if (sqlScope && !st.AllowSqlScope) throw new AiMcpException("AIM_ErrSqlScope", "نطاق الاستعلام الحر غير مسموح.");
            string token = AiMcpTokenText.NewToken();
            string scopes = sqlScope ? AiMcpConst.ScopeRead + "," + AiMcpConst.ScopeSql : AiMcpConst.ScopeRead;
            long id = _tokens.Insert(s.UserId, name, AiMcpTokenText.Hash(token), AiMcpTokenText.Prefix(token), scopes, DateTime.Now.AddDays(days));
            var row = _tokens.ForUser(s.UserId).FirstOrDefault(t => t.Id == id);
            return new AiApiTokenCreated { Row = row, Token = token };
        }

        public List<AiApiTokenRow> MyTokens(AiUserContext user) => _tokens.ForUser(Server(user).UserId);

        public List<AiApiTokenRow> AllTokens(AiUserContext user)
        {
            EnsureAdmin(user);
            return _tokens.All(500);
        }

        public bool Revoke(long id, AiUserContext user)
        {
            var s = Server(user);
            return _tokens.Revoke(id, s.UserId, IsAdmin(s));
        }
    }
}
