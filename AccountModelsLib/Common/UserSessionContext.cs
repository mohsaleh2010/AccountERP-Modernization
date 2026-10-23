using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace AccountModelsLib
{
    /// <summary>
    /// قيم المستخدم الحالى (المرحلة 7 / م6): فى الويندوز فورمز كل عملية تخدم مستخدماً واحداً فتبقى حقول Common الثابتة كما هى،
    /// أما الويب وواجهة الموبايل فعملية واحدة تخدم مستخدمين متزامنين، فكان دخول أى مستخدم يكتب فوق Common.User_Id/Branch_Id للجميع.
    /// كل جلسة (دائرة Blazor أو طلب API) تحمل نسختها هنا، وخصائص Common تقرأ وتكتب فيها متى وُجدت، وإلا فالحقل الثابت كالأصل.
    /// </summary>
    public sealed class UserSession
    {
        public int Is_User_Admin { get; set; }
        public long User_Id { get; set; }
        public long Branch_Id { get; set; }
        public string User_Name { get; set; } = "";
        public string FullUserName { get; set; } = "";
        public int IsAdministration_AlterDBLogin { get; set; }
        public int Is_Force_Admin_Permission { get; set; }

        /// <summary>جلسة جديدة تبدأ بالقيم الافتراضية للعملية (نفس ما كان يراه مستخدم الويب قبل الدخول).</summary>
        public static UserSession FromProcessDefaults() => Common.SnapshotProcessUser();
    }

    public static class UserSessionContext
    {
        private static readonly AsyncLocal<UserSession> _current = new AsyncLocal<UserSession>();

        /// <summary>جلسة التدفق الحالى؛ null = عملية مستخدم واحد (الويندوز فورمز) أو خارج أى جلسة.</summary>
        public static UserSession Current
        {
            get => _current.Value;
            set => _current.Value = value;
        }

        /// <summary>
        /// يضبطها مضيف متعدد المستخدمين (الويب وواجهة الموبايل) بعد انتهاء التهيئة؛ بعدها أى وصول لقيم المستخدم بلا جلسة
        /// يعنى كوداً يعمل خارج دائرة/طلب مستخدم، فقيمته ستكون قيمة العملية لا مستخدماً حقيقياً.
        /// الويندوز فورمز والموبايل الهجين (مستخدم واحد) لا تضبطها فيبقى السلوك القديم كما هو.
        /// </summary>
        public static bool IsMultiUserHost { get; set; }

        /// <summary>جهة تسجيل القراءة بلا جلسة (المضيف يربطها بالـ Logger)؛ كل موضع استدعاء يُسجَّل مرة واحدة فقط.</summary>
        public static Action<string> MisuseReporter { get; set; }

        private static readonly ConcurrentDictionary<string, byte> _reportedSites = new ConcurrentDictionary<string, byte>();

        /// <summary>جلسة القراءة لخاصية فى Common؛ بلا جلسة فى مضيف متعدد المستخدمين تُسجَّل ثم تُرجع null (قيمة العملية) كالأصل.</summary>
        internal static UserSession ForRead([CallerMemberName] string property = "")
        {
            var s = _current.Value;
            if (s == null && IsMultiUserHost)
            {
                string stack = Environment.StackTrace;
                if (_reportedSites.TryAdd(property + "|" + stack, 0))
                {
                    try
                    {
                        (MisuseReporter ?? (m => Debug.WriteLine(m)))(
                            "Common." + property + " قُرئت خارج جلسة مستخدم فى مضيف متعدد المستخدمين - القيمة ليست لمستخدم حقيقى." + Environment.NewLine + stack);
                    }
                    catch { }
                }
            }
            return s;
        }

        /// <summary>جلسة الكتابة لخاصية فى Common؛ الكتابة بلا جلسة فى مضيف متعدد المستخدمين كانت ستغيّر القيمة لكل المستخدمين فتُرفض.</summary>
        internal static UserSession ForWrite([CallerMemberName] string property = "")
        {
            var s = _current.Value;
            if (s == null && IsMultiUserHost)
            {
                throw new InvalidOperationException(
                    "لا يجوز ضبط Common." + property + " خارج جلسة مستخدم فى الويب/الموبايل - كانت ستتغير لكل المستخدمين المتصلين.");
            }
            return s;
        }

        /// <summary>يضبط الجلسة لكتلة كود ثم يعيد السابقة (للخدمات الخلفية والاختبارات).</summary>
        public static IDisposable Use(UserSession session)
        {
            var previous = _current.Value;
            _current.Value = session;
            return new Restore(previous);
        }

        private sealed class Restore : IDisposable
        {
            private readonly UserSession _previous;
            private bool _done;
            public Restore(UserSession previous) { _previous = previous; }
            public void Dispose()
            {
                if (_done) return;
                _done = true;
                _current.Value = _previous;
            }
        }
    }
}
