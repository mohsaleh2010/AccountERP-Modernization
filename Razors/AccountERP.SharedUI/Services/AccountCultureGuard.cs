using System.Globalization;

namespace AccountERP.SharedUI.Services
{
    // 🐛 حارس مؤقت ضد نفس خطأ الثقافة الموثَّق فى LanguageService.ApplyToCurrentThread: كل شاشات شجرة
    // الحسابات تبني/تُنفِّذ SQL خام عبر تجميع نصوص لأرقام (AccountModelsLib.Sub_Account/Submain_AccountCLS)
    // داخل Task.Run - وTask.Run يرث CurrentCulture السائدة وقتها. حتى بعد إصلاح ApplyToCurrentThread نفسها،
    // تبيّن أن شيئاً آخر (على الأرجح UseRequestLocalization فى Program.cs وقت طلب HTTP الذى يُنشئ دائرة
    // Blazor Server) يضبط CurrentCulture=ar-EG وتبقى سارية طوال عمر الدائرة بغض النظر عن ترتيب الرندرات - راجع
    // MODERNIZATION_NOTES.md لتفاصيل التشخيص الكامل (رقم سالب مثل -1 يتحوّل فعلياً إلى "؜-1" بعلامة اتجاه
    // Unicode غير مرئية Arabic Letter Mark عند تنسيقه بثقافة ar-EG، فيفشل أي WHERE Id=-1 بصمت). حتى يُراجَع
    // إصلاح جذري شامل لكل نقاط الاتصال بقاعدة البيانات فى المشروع، يستدعى هذا الحارس صراحة أول كل Task.Run
    // يبني SQL خام لهذه الشاشة تحديداً.
    public static class AccountCultureGuard
    {
        public static void EnsureInvariant()
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        }
    }
}
