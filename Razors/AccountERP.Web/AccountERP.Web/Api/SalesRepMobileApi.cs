using System.Text.Json;
using AccountERP.SharedUI.Services;
using AccountModelsLib;
using AccountModelsLib.SalesRep;
using Microsoft.AspNetCore.DataProtection;

namespace AccountERP.Web.Api
{
    // 📱 المرحلة 7 (أ): واجهة جهاز المندوب تحت /api/v1 (قرار المستخدم 2026-10-17: «السيرفر أولاً»، «رمز تفعيل وPIN»).
    // - التفعيل برمز من المكتب + PIN، ثم دخول بتوكن تجديد الجهاز + PIN، وتجديد خلفى بلا PIN. لا كلمات مرور النظام عبر الإنترنت.
    // - توكن الوصول معتم موقَّع بـ Data Protection (30 دقيقة) يحمل رقم الجهاز وختمه؛ كل طلب يتحقق من الجهاز فى القاعدة فالحظر فورى.
    // - كل طلب موثَّق يعمل بجلسة مستخدم الجهاز (UserSessionContext) وعلى فرع المندوب، بثقافة ثابتة (SQL الخام بالأرقام).
    // المنطق كله فى SalesRepDeviceService/SalesRepSyncService (AccountModelsLib) — هنا الترجمة لـ HTTP فقط.
    public static class SalesRepMobileApi
    {
        public const string AuthRateLimitPolicy = "salesrep-auth";
        private const string ProtectorPurpose = "SalesRepMobile.AccessToken.v1";
        public static readonly TimeSpan AccessTokenLifetime = TimeSpan.FromMinutes(30);

        // أسماء الخصائص كما فى C# (Doc_Guid…) لتطابق مدخلات المستندات المرفوعة.
        private static readonly JsonSerializerOptions Json = new() { PropertyNamingPolicy = null };

        public class PushRequest
        {
            public List<PushDocDto> Docs { get; set; } = new();
        }

        public class PushDocDto
        {
            public Guid Doc_Guid { get; set; }
            public int Doc_Type { get; set; }
            public JsonElement Payload { get; set; }
            public DateTime? Created_At { get; set; }
        }

        public static IEndpointRouteBuilder MapSalesRepMobileApi(this IEndpointRouteBuilder app)
        {
            var api = app.MapGroup("/api/v1").DisableAntiforgery();

            api.MapPost("/devices/activate", (SalesRepDeviceService.ActivateRequest req, IDataProtectionProvider dp) =>
                Run(() => GrantResponse(new SalesRepDeviceService().Activate(req), dp))).RequireRateLimiting(AuthRateLimitPolicy);

            api.MapPost("/auth/login", (SalesRepDeviceService.LoginRequest req, IDataProtectionProvider dp) =>
                Run(() => GrantResponse(new SalesRepDeviceService().Login(req), dp))).RequireRateLimiting(AuthRateLimitPolicy);

            api.MapPost("/auth/refresh", (SalesRepDeviceService.LoginRequest req, IDataProtectionProvider dp) =>
                Run(() => GrantResponse(new SalesRepDeviceService().Refresh(req), dp))).RequireRateLimiting(AuthRateLimitPolicy);

            api.MapGet("/me", (HttpContext http, IDataProtectionProvider dp) =>
                Authed(http, dp, id => Results.Json(new { id.DeviceId, id.RepId, id.RepName, id.BranchId, Datasets = SalesRepSyncService.Datasets }, Json)));

            api.MapGet("/sync/pull", (HttpContext http, IDataProtectionProvider dp, string dataset, long? since) =>
                Authed(http, dp, id => Results.Json(new SalesRepSyncService().Pull(id, dataset, since), Json)));

            api.MapPost("/sync/push", (HttpContext http, IDataProtectionProvider dp, PushRequest req) =>
                Authed(http, dp, id =>
                {
                    var docs = (req?.Docs ?? new()).Select(d => new SalesRepSyncService.PushDoc
                    {
                        Doc_Guid = d.Doc_Guid, Doc_Type = d.Doc_Type, Created_At = d.Created_At,
                        Payload = d.Payload.ValueKind == JsonValueKind.Undefined ? null : d.Payload.GetRawText(),
                    });
                    return Results.Json(new { Results = new SalesRepSyncService().Push(id, docs) }, Json);
                }));

            api.MapGet("/sync/status/{docGuid:guid}", (HttpContext http, IDataProtectionProvider dp, Guid docGuid) =>
                Authed(http, dp, id =>
                {
                    var r = new SalesRepSyncService().GetStatus(id, docGuid);
                    return r == null
                        ? Results.Json(new { Error_Code = "not_found", Message = "المستند لم يصل من هذا الجهاز." }, Json, statusCode: StatusCodes.Status404NotFound)
                        : Results.Json(r, Json);
                }));

            return app;
        }

        private static IResult GrantResponse(SalesRepDeviceService.SessionGrant g, IDataProtectionProvider dp)
        {
            var protector = dp.CreateProtector(ProtectorPurpose).ToTimeLimitedDataProtector();
            string access = protector.Protect($"{g.Device_Id}|{g.Token_Stamp}", AccessTokenLifetime);
            return Results.Json(new
            {
                g.Device_Id, g.Rep_Id, g.Rep_Name, g.Refresh_Token, Access_Token = access, Expires_In = (int)AccessTokenLifetime.TotalSeconds,
            }, Json);
        }

        /// <summary>يتحقق من توكن الوصول ثم يشغّل الطلب بجلسة مستخدم الجهاز.</summary>
        private static IResult Authed(HttpContext http, IDataProtectionProvider dp, Func<SalesRepDeviceService.DeviceIdentity, IResult> f) => Run(() =>
        {
            string header = http.Request.Headers.Authorization.ToString();
            if (!header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                throw new SalesRepDeviceAuthException("invalid_token", "توكن الوصول مطلوب.");
            string payload;
            try
            {
                payload = dp.CreateProtector(ProtectorPurpose).ToTimeLimitedDataProtector().Unprotect(header.Substring(7).Trim());
            }
            catch
            {
                throw new SalesRepDeviceAuthException("invalid_token", "انتهت صلاحية توكن الوصول؛ جدّده.");
            }
            var parts = payload.Split('|');
            if (parts.Length != 2 || !long.TryParse(parts[0], out long deviceId) || !long.TryParse(parts[1], out long stamp))
                throw new SalesRepDeviceAuthException("invalid_token", "توكن الوصول غير صحيح.");
            var id = new SalesRepDeviceService().Authenticate(deviceId, stamp);
            UserSessionContext.Current = SalesRepDeviceService.SessionOf(id);
            return f(id);
        });

        /// <summary>ثقافة ثابتة لـ SQL الخام، وأخطاء العمل/المصادقة لحالات HTTP برسالة عربية للمندوب.</summary>
        private static IResult Run(Func<IResult> f)
        {
            AccountCultureGuard.EnsureInvariant();
            try
            {
                return f();
            }
            catch (SalesRepDeviceAuthException ex)
            {
                int code = ex.Code switch
                {
                    "blocked" or "rep_inactive" => StatusCodes.Status403Forbidden,
                    "pin_locked" => StatusCodes.Status423Locked,
                    "weak_pin" or "bad_request" => StatusCodes.Status400BadRequest,
                    _ => StatusCodes.Status401Unauthorized,
                };
                return Results.Json(new { Error_Code = ex.Code, ex.Message }, Json, statusCode: code);
            }
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
            {
                return Results.Json(new { Error_Code = "invalid", ex.Message }, Json, statusCode: StatusCodes.Status400BadRequest);
            }
        }
    }
}
