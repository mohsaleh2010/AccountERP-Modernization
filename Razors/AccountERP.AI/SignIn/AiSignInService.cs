using AccountERP.AI.Providers;
using AccountERP.AI.Security;
using AccountModelsLib;

namespace AccountERP.AI.SignIn
{
    /// <summary>
    /// إكمال ربط «تسجيل الدخول لدى المزوّد» (P0.1) — مشترك بين نقطة الرجوع (/ai/signin/callback) ووضع لصق الكود من الشاشة:
    /// يتحقق من صلاحية المستخدم، يبدّل الكود بمفتاح، يشفّره ويحفظه، ويسجّل النتيجة ليقرأها استعلام الشاشة.
    /// </summary>
    public sealed class AiSignInService
    {
        public const string MenuKey = "menuItemAiSettings";

        private readonly AiSignInStore _store;
        private readonly AiKeyProtector _keys;
        private readonly AiProvidersCLS _cls = new();

        public AiSignInService(AiSignInStore store, AiKeyProtector keys)
        {
            _store = store;
            _keys = keys;
        }

        public bool UserAllowed(long userId) => _cls.UserHasMenu(userId, MenuKey);

        public async Task<AiSignInStatus> CompleteAsync(AiSignInStore.Pending p, string? code, CancellationToken ct)
        {
            AiSignInStatus result;
            try
            {
                code = (code ?? "").Trim();
                if (code.Length == 0 || code.Length > 2000) throw new InvalidOperationException("الكود فارغ أو غير صالح.");
                if (!UserAllowed(p.UserId)) throw new InvalidOperationException("لا تملك صلاحية شاشة إعدادات الذكاء الاصطناعى.");
                var provider = _cls.GetProvider(p.ProviderId) ?? throw new InvalidOperationException("المزوّد غير موجود (ربما حُذف).");
                if (!AiProviderTypes.SupportsSignIn(provider.ProviderType)) throw new InvalidOperationException("هذا المزوّد لا يدعم الربط بتسجيل الدخول.");

                string key = await OpenRouterSignIn.ExchangeAsync(code, p.Verifier, ct);
                _cls.SaveSignInKey(p.ProviderId, _keys.Protect(key)!, AiKeyProtector.Hint(key)!, p.UserId);
                result = new AiSignInStatus { State = "done", ProviderId = p.ProviderId, Message = "تم الربط بنجاح." };
            }
            catch (Exception ex) when (ex is AiProviderException or InvalidOperationException)
            {
                result = new AiSignInStatus { State = "failed", ProviderId = p.ProviderId, Message = ex.Message };
            }
            catch (Exception ex)
            {
                operations.DataDictionaryLogerException(ex);
                result = new AiSignInStatus { State = "failed", ProviderId = p.ProviderId, Message = "تعذّر إكمال الربط؛ أعد المحاولة." };
            }
            _store.SetStatus(p.Handle, result);
            return result;
        }
    }
}
