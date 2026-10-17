using System;
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
