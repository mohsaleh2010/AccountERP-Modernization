using System.Text.Json;
using System.Text.Json.Nodes;
using AccountERP.AI.Mcp;
using AccountModelsLib;

namespace AccountERP.Web.Api
{
    /// <summary>
    /// P8.10.2 (ai-assistant-p810-plan.md م1، م8، م11، م13): نقطة MCP للقراءة على /mcp (Streamable HTTP: POST JSON-RPC واحد ⇒ رد JSON واحد؛ لا SSE).
    /// الميزة موقوفة ⇒ 404. Origin من مضيف آخر ⇒ 403 (حماية DNS rebinding). توكن غير صالح ⇒ 401. حد 60 طلباً/دقيقة لكل توكن.
    /// </summary>
    public static class AiMcpEndpoint
    {
        public const string RateLimitPolicy = "ai-mcp";

        public static void AddAiMcpRateLimit(Microsoft.AspNetCore.RateLimiting.RateLimiterOptions options)
        {
            options.AddPolicy(RateLimitPolicy, httpContext =>
            {
                string auth = httpContext.Request.Headers.Authorization.ToString();
                string key = auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                    ? "t:" + AiMcpTokenText.Hash(auth.Substring(7).Trim())
                    : "ip:" + (httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");
                int limit = key.StartsWith("t:", StringComparison.Ordinal) ? 60 : 10;
                return System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(key,
                    _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions { PermitLimit = limit, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 });
            });
        }

        public static IEndpointRouteBuilder MapAiMcp(this IEndpointRouteBuilder app)
        {
            app.MapPost(AiMcpConst.Path, HandleAsync).DisableAntiforgery().RequireRateLimiting(RateLimitPolicy);
            app.MapGet(AiMcpConst.Path, (HttpContext http) =>
            {
                var server = http.RequestServices.GetService<AiMcpServer>();
                return server == null || !server.Settings().Enabled ? Results.NotFound() : Results.StatusCode(StatusCodes.Status405MethodNotAllowed);
            }).RequireRateLimiting(RateLimitPolicy);
            return app;
        }

        private static async Task<IResult> HandleAsync(HttpContext http, CancellationToken ct)
        {
            var server = http.RequestServices.GetService<AiMcpServer>();
            if (server == null || !server.Settings().Enabled) return Results.NotFound();

            // م13: Origin من مضيف مختلف يُرفض (عملاء MCP المحليون لا يرسلون Origin عادةً)
            string origin = http.Request.Headers.Origin.ToString();
            if (!string.IsNullOrEmpty(origin))
            {
                if (!Uri.TryCreate(origin, UriKind.Absolute, out var o) || !string.Equals(o.Host, http.Request.Host.Host, StringComparison.OrdinalIgnoreCase))
                    return Results.StatusCode(StatusCodes.Status403Forbidden);
            }
            if (!http.Request.IsHttps)
            {
                var ip = http.Connection.RemoteIpAddress;
                if (ip == null || !System.Net.IPAddress.IsLoopback(ip)) return Results.Json(new { error = "https_required" }, statusCode: StatusCodes.Status403Forbidden);
            }

            string auth = http.Request.Headers.Authorization.ToString();
            string bearer = auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ? auth.Substring(7) : "";
            var token = server.Authenticate(bearer, out string reason);
            if (token == null)
            {
                http.Response.Headers.WWWAuthenticate = "Bearer error=\"invalid_token\", error_description=\"" + reason + "\"";
                return Results.Json(AiMcpServer.Error(null, -32001, "Unauthorized: " + reason), statusCode: StatusCodes.Status401Unauthorized);
            }

            JsonNode? body;
            try { body = await JsonNode.ParseAsync(http.Request.Body, cancellationToken: ct); }
            catch (JsonException) { return Results.Json(AiMcpServer.Error(null, -32700, "Parse error"), statusCode: StatusCodes.Status400BadRequest); }
            if (body is JsonArray) return Results.Json(AiMcpServer.Error(null, -32600, "Batch requests are not supported"), statusCode: StatusCodes.Status400BadRequest);

            var res = await server.HandleAsync(body, token, http.Connection.RemoteIpAddress?.ToString(), ct);
            if (res == null) return Results.StatusCode(StatusCodes.Status202Accepted);
            return Results.Content(res.ToJsonString(), "application/json");
        }
    }
}
