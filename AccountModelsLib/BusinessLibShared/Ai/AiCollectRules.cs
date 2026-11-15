using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace AccountModelsLib
{
    /// <summary>
    /// P8.5 (ai-assistant-p85-plan.md ت1، ت4، ت9–ت13، ت19، ت20، §1): مصدر الحكم الوحيد للدرجة والأولوية والمستوى والحالة — دوال نقية بلا قاعدة بيانات.
    /// </summary>
    public static class AiCollectRules
    {
        public const decimal Tolerance = 0.005m;

        public static decimal Clamp01(decimal x) => x < 0 ? 0 : x > 1 ? 1 : x;

        /// <summary>المتأخر، غير المرتبط (يرفع أقدم تأخير إلى 91 — ت7)، DSO، أيام بلا تحصيل.</summary>
        public static void Derive(AiCollectFacts f, AiCollectSettings s, DateTime today)
        {
            if (f == null) return;
            decimal allocated = f.Allocated > 0 ? f.Allocated : f.NotDue + f.D1_30 + f.D31_60 + f.D61_90 + f.DOver90;
            f.Unallocated = Math.Max(0, f.Balance - allocated);
            if (f.Unallocated <= Tolerance) f.Unallocated = 0;
            f.Overdue = f.D1_30 + f.D31_60 + f.D61_90 + f.DOver90 + f.Unallocated;
            if (f.Unallocated > 0) f.OldestDays = Math.Max(f.OldestDays, AiCollectConst.UnallocatedMinDays);
            int window = s == null ? 180 : s.DsoWindowDays;
            f.Dso = f.CreditSales > 0 ? (int?)(int)Math.Round(f.Balance / f.CreditSales * window, MidpointRounding.AwayFromZero) : null;
            f.IdleDays = f.LastReceiptDate.HasValue ? (int?)Math.Max(0, (today.Date - f.LastReceiptDate.Value.Date).Days) : null;
        }

        /// <summary>العوامل الخمسة فى [0، 1]: عمر، مبلغ، سلوك، حد، آخر تحصيل.</summary>
        public static decimal[] Factors(AiCollectFacts f, AiCollectSettings s, DateTime today)
        {
            var r = new decimal[5];
            if (f == null || s == null) return r;
            r[0] = Clamp01((decimal)f.OldestDays / Math.Max(1, s.AgeCapDays));
            r[1] = Clamp01(f.Overdue / (s.AmountCap <= 0 ? 1 : s.AmountCap));
            r[2] = f.Dso.HasValue ? Clamp01((decimal)(f.Dso.Value - s.TermsDays) / Math.Max(1, s.DsoCapDays)) : 1;
            if (!f.CreditLimit.HasValue || f.Balance <= f.CreditLimit.Value) r[3] = 0;
            else if (f.CreditLimit.Value <= 0) r[3] = 1;
            else r[3] = 0.5m + 0.5m * Clamp01((f.Balance - f.CreditLimit.Value) / f.CreditLimit.Value);
            r[4] = f.IdleDays.HasValue ? Clamp01((decimal)(f.IdleDays.Value - s.IdleStartDays) / Math.Max(1, s.IdleCapDays)) : 1;
            return r;
        }

        /// <summary>
        /// ت12: 100 × Σ(w × f) ÷ Σw مقرّباً. بلا متأخر ⇒ سقف MediumScore − 1. وعد لم يُوفَ ⇒ + BrokenPromiseBonus حتى 100.
        /// </summary>
        public static AiCollectScore Score(AiCollectFacts f, AiCollectSettings s, DateTime today, bool brokenPromise)
        {
            var res = new AiCollectScore();
            if (f == null || s == null) return res;
            var w = s.Weights != null && s.Weights.Length == 5 ? s.Weights : new decimal[] { 35, 25, 15, 15, 10 };
            decimal sum = w.Sum();
            if (sum <= 0) { w = new decimal[] { 35, 25, 15, 15, 10 }; sum = 100; }
            var fac = Factors(f, s, today);
            decimal total = 0;
            for (int i = 0; i < 5; i++)
            {
                res.Parts[i] = Math.Round(w[i] * fac[i] * 100 / sum, 1, MidpointRounding.AwayFromZero);
                total += w[i] * fac[i];
            }
            int score = (int)Math.Round(100 * total / sum, MidpointRounding.AwayFromZero);
            if (f.Overdue <= Tolerance) score = Math.Min(score, s.MediumScore - 1);
            if (brokenPromise && s.BrokenPromiseBonus > 0) { score += s.BrokenPromiseBonus; res.BrokenBonus = true; }
            res.Score = Math.Max(0, Math.Min(100, score));
            res.Priority = Priority(res.Score, s);
            res.Level = Level(f.OldestDays, s);
            return res;
        }

        public static string Priority(int score, AiCollectSettings s) =>
            score >= s.HighScore ? AiCollectPriority.High : score >= s.MediumScore ? AiCollectPriority.Medium : AiCollectPriority.Low;

        /// <summary>ت20: 0 = لا تأخير، 1 ودّى، 2 حازم (من Level2Days)، 3 أخير (من Level3Days).</summary>
        public static int Level(int oldestDays, AiCollectSettings s)
        {
            if (oldestDays <= 0) return 0;
            if (oldestDays < s.Level2Days) return 1;
            if (oldestDays < s.Level3Days) return 2;
            return 3;
        }

        /// <summary>
        /// ت19: الحالة من الإجراءات المفتوحة بالأسبقية: متنازع، مؤجَّل، موعود، وعد لم يُوفَ، مستحق.
        /// الوعد لم يُوفَ = تاريخه فات، ولا إذن استلام منذ تسجيله، وفواته منذ ≤ BrokenPromiseDays.
        /// </summary>
        public static string State(IList<AiCollectActionRow> open, DateTime today, DateTime? lastReceipt, AiCollectSettings s, out bool broken)
        {
            broken = false;
            var list = (open ?? new List<AiCollectActionRow>()).Where(a => a != null && a.Status == AiCollectActionStatus.Open).ToList();
            DateTime d = today.Date;
            if (list.Any(a => a.ActionType == AiCollectActionTypes.Dispute)) return AiCollectState.Disputed;
            if (list.Any(a => a.ActionType == AiCollectActionTypes.Snooze && a.NextActionDate.HasValue && a.NextActionDate.Value.Date > d)) return AiCollectState.Snoozed;
            var promises = list.Where(a => a.ActionType == AiCollectActionTypes.Promise && a.PromiseDate.HasValue)
                               .OrderByDescending(a => a.CreatedAt ?? DateTime.MinValue).ToList();
            var p = promises.FirstOrDefault();
            if (p != null)
            {
                if (p.PromiseDate.Value.Date >= d) return AiCollectState.Promised;
                bool paidAfter = lastReceipt.HasValue && lastReceipt.Value.Date >= (p.CreatedAt ?? p.PromiseDate.Value).Date;
                if (!paidAfter && (d - p.PromiseDate.Value.Date).TotalDays <= s.BrokenPromiseDays)
                {
                    broken = true;
                    return AiCollectState.Broken;
                }
            }
            return AiCollectState.Due;
        }

        /// <summary>ت4: توزيع FIFO — الرصيد يغطى أحدث الفواتير أولاً؛ يعيد المفتوح فقط (بنفس ترتيب الإدخال: الأحدث أولاً).</summary>
        public static List<AiCollectInvoice> Fifo(decimal balance, IList<AiCollectInvoice> newestFirst, DateTime today)
        {
            var res = new List<AiCollectInvoice>();
            decimal left = balance;
            foreach (var inv in newestFirst ?? new List<AiCollectInvoice>())
            {
                if (left <= Tolerance) break;
                if (inv.NetTotal <= 0) continue;
                decimal open = Math.Min(inv.NetTotal, left);
                left -= open;
                res.Add(new AiCollectInvoice
                {
                    SaleId = inv.SaleId, InvoiceNo = inv.InvoiceNo, InvoiceDate = inv.InvoiceDate, Due = inv.Due, NetTotal = inv.NetTotal,
                    OpenAmount = open, DaysOverdue = Math.Max(0, (today.Date - inv.Due.Date).Days),
                });
            }
            return res;
        }

        /// <summary>بصمة حقائق المسودة: تتغير بالمبلغ أو الأيام أو المستوى أو اللغة (ت21).</summary>
        public static string FactsHash(AiCollectRow r, int level, string language)
        {
            var c = CultureInfo.InvariantCulture;
            string raw = string.Join("|", r == null ? "" : r.CustomerId.ToString(c), r == null ? "" : Math.Round(r.Balance, 2).ToString(c),
                r == null ? "" : Math.Round(r.Overdue, 2).ToString(c), r == null ? "" : r.OldestDays.ToString(c), r == null ? "" : r.OpenInvoices.ToString(c),
                level.ToString(c), language ?? "");
            using (var sha = SHA256.Create())
                return string.Concat(sha.ComputeHash(Encoding.UTF8.GetBytes(raw)).Select(b => b.ToString("x2", c)));
        }

        /// <summary>خمسة أوزان 0–100 مفصولة بفواصل؛ غير ذلك ⇒ null.</summary>
        public static decimal[] ParseWeights(string csv)
        {
            var parts = (csv ?? "").Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 5) return null;
            var r = new decimal[5];
            for (int i = 0; i < 5; i++)
            {
                decimal d;
                if (!decimal.TryParse(parts[i].Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out d) || d < 0 || d > 100) return null;
                r[i] = d;
            }
            return r.Sum() > 0 ? r : null;
        }
    }
}
