using AccountModelsLib;
using Microsoft.AspNetCore.Components.Server.Circuits;

namespace AccountERP.Web.Services
{
    // 🔐 م6 (المرحلة 7): يربط جلسة المستخدم (UserSession - Scoped لكل دائرة) بتدفق كل نشاط وارد للدائرة (حدث، JS interop،
    // تنقّل، رندر)، فتقرأ Common.User_Id/Branch_Id/... قيم مستخدم هذه الدائرة وحده بدل آخر مستخدم دخل على العملية كلها.
    // AuthService.LoginAsync يكتب Common.User_Id = ... كما هو، فيذهب للجلسة لا للحقل الثابت. Task.Run داخل النشاط يرث الجلسة.
    public sealed class UserSessionCircuitHandler : CircuitHandler
    {
        private readonly UserSession _session;

        public UserSessionCircuitHandler(UserSession session) { _session = session; }

        public override Func<CircuitInboundActivityContext, Task> CreateInboundActivityHandler(Func<CircuitInboundActivityContext, Task> next)
        {
            return context =>
            {
                UserSessionContext.Current = _session;
                return next(context);
            };
        }
    }
}
