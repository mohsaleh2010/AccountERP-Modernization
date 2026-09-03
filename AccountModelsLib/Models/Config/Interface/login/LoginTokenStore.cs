using System;
using System.Collections.Concurrent;

namespace AccountModelsLib.Models
{
    // 🟢 مخزن رموز مؤقتة (Singleton، ذاكرة فقط) يُستخدم فقط على الويب لعبور فجوة معمارية حقيقية فى
    // Blazor Server: لحظة نجاح الدخول نحن بالفعل داخل دائرة (Circuit) تفاعلية حية، والاستجابة الأصلية
    // (HTTP Response) التى كان يمكنها ضبط كوكي انتهت وأُرسلت للمتصفح منذ زمن - فلا يمكن ضبط كوكي دخول
    // دائم مباشرة من كود مكوّن الـ Login. بدلاً من إعادة إرسال اسم المستخدم/الباسورد مرة أخرى عبر الشبكة
    // لشاشة وسيطة تتحقق منها من جديد، نصدر رمزاً عشوائياً لمرة واحدة فقط صالحاً لثوانٍ معدودة فور نجاح
    // الدخول الحقيقي (المتحقق منه بالفعل بكلمة السر داخل AuthService.LoginAsync)، ثم يُستبدل فوراً بكوكي
    // دخول موقّع حقيقي عبر مسار /Auth/Persist (نفس أسلوب /Culture/SetCulture الموجود مسبقاً لكوكي اللغة).
    public class LoginTokenStore
    {
        private readonly ConcurrentDictionary<string, (int UserId, DateTime Expiry)> _tokens = new();
        private static readonly TimeSpan TokenLifetime = TimeSpan.FromSeconds(30);

        public string IssueToken(int userId)
        {
            var token = Guid.NewGuid().ToString("N");
            _tokens[token] = (userId, DateTime.UtcNow.Add(TokenLifetime));
            return token;
        }

        // 🟢 استبدال لمرة واحدة فقط: الرمز يُحذف فور محاولة استبداله سواء نجحت أو فشلت، فلا يمكن إعادة استخدامه
        public bool TryRedeem(string token, out int userId)
        {
            userId = 0;
            if (string.IsNullOrEmpty(token)) return false;

            if (_tokens.TryRemove(token, out var entry) && entry.Expiry > DateTime.UtcNow)
            {
                userId = entry.UserId;
                return true;
            }

            return false;
        }
    }
}
