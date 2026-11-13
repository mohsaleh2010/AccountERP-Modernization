using System.Net;
using AccountModelsLib.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AccountERP.AI.SignIn
{
    /// <summary>
    /// GET /ai/signin/callback?code=..&amp;state=.. — يعود إليه المزوّد بعد تسجيل الدخول (وضع الرجوع؛ يُستخدم حين يكون التطبيق https أو localhost).
    /// التحقق: state موجود وغير منتهٍ ويُستهلك مرة واحدة، كوكى دخول التطبيق يخص نفس المستخدم الذى بدأ الربط، والمستخدم ما زال يملك صلاحية الشاشة.
    /// الرد صفحة HTML ثابتة بلا أى سرّ، بـ no-store و no-referrer.
    /// </summary>
    public static class AiSignInEndpoints
    {
        public static RouteHandlerBuilder MapAiSignInEndpoints(this IEndpointRouteBuilder app) =>
            app.MapGet(OpenRouterSignIn.CallbackPath, async (HttpContext http, AiSignInStore store, AiSignInService service, IDataProtectionProvider dp) =>
            {
                http.Response.Headers.CacheControl = "no-store";
                http.Response.Headers["Referrer-Policy"] = "no-referrer";
                string? state = http.Request.Query["state"], code = http.Request.Query["code"];

                long cookieUser = 0;
                if (http.Request.Cookies.TryGetValue(AuthCookieConstants.CookieName, out var cookie))
                {
                    try { long.TryParse(dp.CreateProtector(AuthCookieConstants.ProtectorPurpose).Unprotect(cookie), out cookieUser); }
                    catch (System.Security.Cryptography.CryptographicException) { cookieUser = 0; }
                }

                var pending = store.ConsumeByState(state);
                if (pending == null)
                    return Page(false, "رابط الربط غير صالح أو انتهت مهلته (10 دقائق) أو استُخدم من قبل. ارجع لشاشة الإعدادات وأعد المحاولة.");
                if (cookieUser == 0 || cookieUser != pending.UserId)
                {
                    store.SetStatus(pending.Handle, new AccountModelsLib.AiSignInStatus { State = "failed", ProviderId = pending.ProviderId, Message = "اكتمل تسجيل الدخول من متصفح أو مستخدم مختلف؛ أعد المحاولة من نفس المتصفح." });
                    return Page(false, "هذا المتصفح غير مسجّل دخوله بنفس مستخدم النظام الذى بدأ الربط. أعد المحاولة من نفس المتصفح.");
                }
                var result = await service.CompleteAsync(pending, code, http.RequestAborted);
                return Page(result.State == "done", result.State == "done" ? "تم الربط بنجاح. يمكنك إغلاق هذه النافذة والعودة لشاشة الإعدادات." : result.Message);
            });

        private static IResult Page(bool ok, string message)
        {
            string color = ok ? "#1b7f3b" : "#b3261e";
            string icon = ok ? "✓" : "!";
            string html = $@"<!doctype html><html lang=""ar"" dir=""rtl""><head><meta charset=""utf-8""><meta name=""viewport"" content=""width=device-width,initial-scale=1"">
<title>ربط مزوّد الذكاء الاصطناعى</title>
<style>body{{font-family:system-ui,'Segoe UI',Tahoma,sans-serif;background:#f5f6f8;color:#1f2328;display:grid;place-items:center;min-height:100vh;margin:0;padding:16px}}
.c{{background:#fff;border-radius:16px;padding:32px 24px;max-width:440px;text-align:center;box-shadow:0 4px 24px rgba(0,0,0,.08)}}
.i{{width:56px;height:56px;border-radius:50%;display:grid;place-items:center;margin:0 auto 16px;font-size:28px;color:#fff;background:{color}}}
p{{line-height:1.7;font-size:16px;margin:0 0 20px}}button{{min-height:44px;padding:0 24px;border-radius:10px;border:1px solid #c9ccd1;background:#fff;font-size:15px;cursor:pointer}}
@media (prefers-color-scheme:dark){{body{{background:#16181c;color:#e6e6e6}}.c{{background:#202329}}button{{background:#2a2e35;color:#e6e6e6;border-color:#444}}}}</style></head>
<body><main class=""c""><div class=""i"" aria-hidden=""true"">{icon}</div><p role=""status"">{WebUtility.HtmlEncode(message)}</p>
<button type=""button"" onclick=""window.close()"">إغلاق النافذة</button></main></body></html>";
            return Results.Content(html, "text/html; charset=utf-8");
        }
    }
}
