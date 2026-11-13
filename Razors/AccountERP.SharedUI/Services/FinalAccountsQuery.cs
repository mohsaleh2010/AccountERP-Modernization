using System.Globalization;
using AccountModelsLib.BusinessLibShared.GeneralLedger;

namespace AccountERP.SharedUI.Services
{
    /// <summary>
    /// P3 المساعد الذكى (open_screen): باراميترات اختيارية فى رابط شاشات القوائم الختامية
    /// (?from=yyyy-MM-dd&amp;to=yyyy-MM-dd&amp;branch=&amp;run=1). إضافة فقط: بدونها تعمل الشاشات كما كانت (الفترة الحالية وفرع المستخدم).
    /// القيمة غير الصالحة أو خارج الفترات تُتجاهل بصمت فتبقى القيم الافتراضية.
    /// </summary>
    public static class FinalAccountsQuery
    {
        public static DateTime? Date(string? s) =>
            DateTime.TryParseExact((s ?? "").Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d : null;

        /// <summary>يطبّق الفترة والفرع من الرابط. from اختيارى (الافتراضى بداية الفترة التى تحتوى to).</summary>
        public static void Apply(string? fromQ, string? toQ, long? branchQ,
                                 List<FinalAccountsService.PeriodRow> periods, List<FinalAccountsService.Option> branches,
                                 ref long periodId, ref DateTime? from, ref DateTime? to, ref long branch)
        {
            var qf = Date(fromQ); var qt = Date(toQ);
            if (qt != null)
            {
                var anchor = qf ?? qt.Value;
                var p = periods.FirstOrDefault(x => anchor.Date >= x.DateFrom.Date && qt.Value.Date <= x.DateTo.Date);
                if (p != null && (qf == null || qf <= qt))
                {
                    periodId = p.Id;
                    from = qf ?? p.DateFrom;
                    to = qt;
                }
            }
            if (branchQ is long b && b > 0 && branches.Any(x => x.Id == b)) branch = b;
        }
    }
}
