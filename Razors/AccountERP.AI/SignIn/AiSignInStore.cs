using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using AccountModelsLib;

namespace AccountERP.AI.SignIn
{
    /// <summary>
    /// الحالة المعلّقة لربط «تسجيل الدخول لدى المزوّد» (P0.1) فى الذاكرة فقط: state + PKCE verifier + المستخدم + المزوّد.
    /// صلاحية 10 دقائق، استخدام مرة واحدة (استهلاك ذرّى)، وحد 5 طلبات معلّقة لكل مستخدم. إعادة تشغيل الخادم تُسقطها عمداً (سرّ مؤقت).
    /// الـ Handle (للاستعلام من الشاشة) قيمة عشوائية منفصلة عن state (الذى يمر عبر المتصفح والمزوّد).
    /// </summary>
    public sealed class AiSignInStore
    {
        public static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(10);
        private const int MaxPendingPerUser = 5;

        public sealed class Pending
        {
            public string State { get; init; } = "";
            public string Handle { get; init; } = "";
            public string Verifier { get; init; } = "";
            public long UserId { get; init; }
            public long ProviderId { get; init; }
            public DateTime ExpiresAtUtc { get; init; }
            public int Consumed;   // 0/1 عبر Interlocked
        }

        private readonly ConcurrentDictionary<string, Pending> _byState = new(StringComparer.Ordinal);
        private readonly ConcurrentDictionary<string, Pending> _byHandle = new(StringComparer.Ordinal);
        private readonly ConcurrentDictionary<string, (AiSignInStatus Status, long UserId, DateTime ExpiresAtUtc)> _status = new(StringComparer.Ordinal);

        public Pending Create(long userId, long providerId)
        {
            Sweep();
            if (_byState.Values.Count(p => p.UserId == userId && p.Consumed == 0) >= MaxPendingPerUser)
                throw new InvalidOperationException("طلبات ربط معلّقة كثيرة؛ انتظر دقائق ثم أعد المحاولة.");
            var p = new Pending
            {
                State = Random(32),
                Handle = Random(24),
                Verifier = Random(48),
                UserId = userId,
                ProviderId = providerId,
                ExpiresAtUtc = DateTime.UtcNow.Add(Lifetime),
            };
            _byState[p.State] = p;
            _byHandle[p.Handle] = p;
            _status[p.Handle] = (new AiSignInStatus { State = "pending", ProviderId = providerId }, userId, p.ExpiresAtUtc);
            return p;
        }

        /// <summary>يستهلك الطلب مرة واحدة بالـ state (نقطة الرجوع). null إن لم يوجد أو انتهى أو استُهلك.</summary>
        public Pending? ConsumeByState(string? state) => Consume(state == null ? null : _byState.GetValueOrDefault(state));

        /// <summary>يستهلك الطلب مرة واحدة بالـ Handle (وضع لصق الكود) بشرط نفس المستخدم.</summary>
        public Pending? ConsumeByHandle(string? handle, long userId)
        {
            var p = handle == null ? null : _byHandle.GetValueOrDefault(handle);
            return p != null && p.UserId == userId ? Consume(p) : null;
        }

        private Pending? Consume(Pending? p)
        {
            if (p == null || DateTime.UtcNow > p.ExpiresAtUtc) return null;
            if (Interlocked.Exchange(ref p.Consumed, 1) != 0) return null;
            _byState.TryRemove(p.State, out _);
            _byHandle.TryRemove(p.Handle, out _);
            return p;
        }

        public void SetStatus(string handle, AiSignInStatus status)
        {
            if (_status.TryGetValue(handle, out var cur))
                _status[handle] = (status, cur.UserId, cur.ExpiresAtUtc);
        }

        public AiSignInStatus GetStatus(string handle, long userId)
        {
            if (!_status.TryGetValue(handle ?? "", out var s) || s.UserId != userId)
                return new AiSignInStatus { State = "expired", Message = "انتهت جلسة الربط أو أُعيد تشغيل الخادم؛ أعد المحاولة." };
            if (s.Status.State == "pending" && DateTime.UtcNow > s.ExpiresAtUtc)
                return new AiSignInStatus { State = "expired", ProviderId = s.Status.ProviderId, Message = "انتهت مهلة الربط (10 دقائق)؛ أعد المحاولة." };
            return s.Status;
        }

        private void Sweep()
        {
            var now = DateTime.UtcNow;
            foreach (var kv in _byState.Where(kv => now > kv.Value.ExpiresAtUtc).ToList())
            {
                _byState.TryRemove(kv.Key, out _);
                _byHandle.TryRemove(kv.Value.Handle, out _);
            }
            // حالة النتيجة تبقى 10 دقائق بعد الانتهاء حتى تقرأها الشاشة.
            foreach (var kv in _status.Where(kv => now > kv.Value.ExpiresAtUtc.Add(Lifetime)).ToList())
                _status.TryRemove(kv.Key, out _);
        }

        private static string Random(int bytes) => Base64Url(RandomNumberGenerator.GetBytes(bytes));

        public static string Base64Url(byte[] data) => Convert.ToBase64String(data).TrimEnd('=').Replace('+', '-').Replace('/', '_');

        /// <summary>PKCE S256: base64url(SHA-256(verifier)).</summary>
        public static string Challenge(string verifier) => Base64Url(SHA256.HashData(Encoding.ASCII.GetBytes(verifier)));
    }
}
