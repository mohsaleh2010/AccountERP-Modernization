using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AccountModelsLib
{
    /// <summary>
    /// P8.4 (ai-assistant-p84-plan.md ش1، §1): مصدر الحكم الوحيد لقواعد الشذوذ — دوال نقية بلا قاعدة بيانات.
    /// الماسح يجلب المرشحين بـ SQL ثم يعيد التحقق هنا قبل أى إدراج، فالحدّيات تُختبر بلا قاعدة.
    /// </summary>
    public static class AiAnomalyRules
    {
        /// <summary>(المبلغ − المتوسط) ÷ الانحراف مقرّباً لأربع منازل؛ null إن كان الانحراف صفراً أو سالباً أو غير رقمى.</summary>
        public static decimal? ZScore(decimal amount, double mean, double sd)
        {
            if (!(sd > 0) || double.IsNaN(sd) || double.IsInfinity(sd) || double.IsNaN(mean) || double.IsInfinity(mean)) return null;
            double z = ((double)amount - mean) / sd;
            if (double.IsNaN(z) || double.IsInfinity(z) || Math.Abs(z) > 1e12) return null;
            return Math.Round((decimal)z, 4, MidpointRounding.AwayFromZero);
        }

        /// <summary>ش9: الذيل الأعلى فقط، عينة ≥ MinSample، انحراف > 0، مبلغ ≥ MinAmount، z ≥ ZThreshold.</summary>
        public static bool IsOutlier(decimal amount, double mean, double sd, int n, AiAnomalySettings s, out decimal z)
        {
            z = 0;
            if (s == null || n < s.MinSample || amount < s.MinAmount) return false;
            var zz = ZScore(amount, mean, sd);
            if (!zz.HasValue) return false;
            z = zz.Value;
            return z >= s.ZThreshold;
        }

        /// <summary>
        /// ش12: عطلة مسجّلة، ثم يوم ليس من أيام العمل، ثم خارج [WorkStart, WorkEnd). الدوام قد يعبر منتصف الليل (WorkStart > WorkEnd).
        /// </summary>
        public static bool IsOffHours(DateTime insertedAt, AiAnomalySettings s, out string reason)
        {
            reason = null;
            if (s == null) return false;
            DateTime day = insertedAt.Date;
            if (s.Holidays != null && s.Holidays.Any(h => h.Date == day)) { reason = AiAnomalyOffReason.Holiday; return true; }
            if (s.WorkDays != null && s.WorkDays.Count > 0 && !s.WorkDays.Contains((int)insertedAt.DayOfWeek)) { reason = AiAnomalyOffReason.Weekend; return true; }
            if (!InWorkHours(insertedAt.TimeOfDay, s.WorkStart, s.WorkEnd)) { reason = AiAnomalyOffReason.Night; return true; }
            return false;
        }

        public static bool InWorkHours(TimeSpan t, TimeSpan start, TimeSpan end)
        {
            if (start == end) return true;
            return start < end ? t >= start && t < end : t >= start || t < end;
        }

        /// <summary>ش14: إجمالى ≥ RoundMinAmount ويقبل القسمة على RoundUnit.</summary>
        public static bool IsRoundLarge(decimal total, AiAnomalySettings s)
        {
            if (s == null || s.RoundUnit <= 0 || total <= 0 || total < s.RoundMinAmount) return false;
            return total % s.RoundUnit == 0;
        }

        /// <summary>
        /// ش13: مبلغ ≥ MinAmount على حساب استُعمل فى ≤ RareMaxUses قيود خلال خط الأساس. شرط العمر يُطبَّق فقط إن عُرف تاريخ الإنشاء
        /// (لا عمود له فى قاعدة النظام — فحص 1 — فالماسح يمرّر null).
        /// </summary>
        public static bool IsRare(int uses, decimal amount, DateTime? accountCreated, DateTime now, AiAnomalySettings s)
        {
            if (s == null || amount < s.MinAmount || uses > s.RareMaxUses) return false;
            if (accountCreated.HasValue && (now - accountCreated.Value).TotalDays < AiAnomalyConst.AccountMinAgeDays) return false;
            return true;
        }

        /// <summary>ش11: نفس مجموعة السطور (مجموعة متعددة) بتسامح 0.005 فى المدين والدائن، بأى ترتيب.</summary>
        public static bool SameLines(IList<AiAnomalyLineKey> a, IList<AiAnomalyLineKey> b)
        {
            if (a == null || b == null || a.Count == 0 || a.Count != b.Count) return false;
            var used = new bool[b.Count];
            foreach (var x in a.OrderBy(l => l.AccountId).ThenBy(l => l.Debit).ThenBy(l => l.Credit))
            {
                int hit = -1;
                for (int i = 0; i < b.Count; i++)
                {
                    if (used[i]) continue;
                    var y = b[i];
                    if (y.AccountId == x.AccountId && Math.Abs(y.Debit - x.Debit) <= AiAnomalyConst.LineTolerance
                        && Math.Abs(y.Credit - x.Credit) <= AiAnomalyConst.LineTolerance) { hit = i; break; }
                }
                if (hit < 0) return false;
                used[hit] = true;
            }
            return true;
        }

        /// <summary>ش15: خطورة القاعدة وحدها.</summary>
        public static string Severity(string rule, decimal? score, decimal total, AiAnomalySettings s)
        {
            decimal roundMin = s == null ? 50000 : s.RoundMinAmount;
            switch (rule)
            {
                case AiAnomalyRuleCodes.AmountOutlier: return score.HasValue && score.Value >= 6 ? AiAnomalySeverity.High : AiAnomalySeverity.Medium;
                case AiAnomalyRuleCodes.Duplicate: return AiAnomalySeverity.High;
                case AiAnomalyRuleCodes.RoundLarge: return roundMin > 0 && total >= roundMin * 10 ? AiAnomalySeverity.High : AiAnomalySeverity.Medium;
                case AiAnomalyRuleCodes.RareAccount: return AiAnomalySeverity.Medium;
                case AiAnomalyRuleCodes.OffHours: return roundMin > 0 && total >= roundMin ? AiAnomalySeverity.Medium : AiAnomalySeverity.Info;
                default: return AiAnomalySeverity.Info;
            }
        }

        /// <summary>ش15: خطورة القيد = أعلى قواعده، وترتفع درجة واحدة (حتى «عالية») إن أصابته قاعدتان مختلفتان فأكثر.</summary>
        public static string EntrySeverity(IEnumerable<AiAnomalyRow> rules)
        {
            var list = (rules ?? Enumerable.Empty<AiAnomalyRow>()).Where(r => r != null).ToList();
            if (list.Count == 0) return AiAnomalySeverity.Info;
            int rank = list.Max(r => AiAnomalySeverity.Rank(r.Severity));
            if (rank < 1) rank = 1;
            if (list.Select(r => r.RuleCode).Distinct().Count() >= 2) rank = Math.Min(3, rank + 1);
            return AiAnomalySeverity.FromRank(rank);
        }

        /// <summary>تواريخ yyyy-MM-dd مفصولة بفواصل (يتجاهل الفاسد)، مرتبة بلا تكرار.</summary>
        public static List<DateTime> ParseHolidays(string csv)
        {
            var list = new List<DateTime>();
            foreach (var part in (csv ?? "").Split(new[] { ',', ';', ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
            {
                DateTime d;
                if (DateTime.TryParseExact(part.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out d)) list.Add(d.Date);
            }
            return list.Distinct().OrderBy(d => d).ToList();
        }

        /// <summary>يقسم [from, to) عند كل منتصف ليل (ش29). from ≥ to ⇒ قائمة فارغة.</summary>
        public static List<(DateTime From, DateTime To)> DayChunks(DateTime from, DateTime to)
        {
            var list = new List<(DateTime From, DateTime To)>();
            DateTime cur = from;
            while (cur < to)
            {
                DateTime next = cur.Date.AddDays(1);
                if (next > to) next = to;
                list.Add((cur, next));
                cur = next;
            }
            return list;
        }
    }
}
