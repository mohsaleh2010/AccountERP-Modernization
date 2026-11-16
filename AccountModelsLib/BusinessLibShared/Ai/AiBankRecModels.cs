using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AccountModelsLib
{
    /// <summary>P8.12 (ai-assistant-p812-plan.md): ثوابت المطابقة البنكية.</summary>
    public static class AiBankConst
    {
        public const string Route = "/accounts/bank-reconciliation";
        public const string MenuKey = "menuItemAiBankRec";
        public const string KindExact = "exact";
        public const string KindGroup = "group";
        public const string KindModel = "model";
        public const string KindManual = "manual";
        public const string StatusSuggested = "suggested";
        public const string StatusConfirmed = "confirmed";
        public const string StatusRejected = "rejected";
        public const decimal Tolerance = 0.01m;
    }

    public class AiBankSettings
    {
        public bool Enabled { get; set; }
        public int MatchDays { get; set; } = 3;
        public int GroupMaxItems { get; set; } = 4;
        public bool UseModel { get; set; } = true;

        public static AiBankSettings From(IDictionary<string, string> s)
        {
            var r = new AiBankSettings();
            if (s == null) return r;
            string v; int d;
            r.Enabled = s.TryGetValue(AiSettingKeys.BankRecEnabled, out v) && v == "1";
            r.UseModel = !s.TryGetValue(AiSettingKeys.BankUseModel, out v) || v != "0";
            if (s.TryGetValue(AiSettingKeys.BankMatchDays, out v) && int.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out d)) r.MatchDays = Math.Max(0, Math.Min(15, d));
            if (s.TryGetValue(AiSettingKeys.BankGroupMaxItems, out v) && int.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out d)) r.GroupMaxItems = Math.Max(2, Math.Min(6, d));
            return r;
        }
    }

    /// <summary>تعيين الأعمدة (ب1): أسماء أعمدة الملف كما هى. Amount = عمود واحد بإشارة (سالب = سحب) بديلاً عن Debit/Credit.</summary>
    public class AiBankColumnMap
    {
        public string Date { get; set; }
        public string ValueDate { get; set; }
        public string Description { get; set; }
        public string Reference { get; set; }
        public string Debit { get; set; }
        public string Credit { get; set; }
        public string Amount { get; set; }
        public string Balance { get; set; }
        /// <summary>dmy (الافتراضى) / mdy / ymd.</summary>
        public string DateOrder { get; set; } = "dmy";
    }

    public class AiBankStatementRow
    {
        public long Id { get; set; }
        public long Bank_Id { get; set; }
        public string BankName { get; set; }
        public DateTime? PeriodFrom { get; set; }
        public DateTime? PeriodTo { get; set; }
        public decimal? OpeningBalance { get; set; }
        public decimal? ClosingBalance { get; set; }
        public string Source { get; set; }
        public string FileName { get; set; }
        public string Status { get; set; }
        public int LineCount { get; set; }
        public decimal? BalanceDiff { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    /// <summary>سطر الكشف. Debit = سحب (خارج من البنك)، Credit = إيداع (داخل).</summary>
    public class AiBankLine
    {
        public long Id { get; set; }
        public int LineNo { get; set; }
        public DateTime? Date { get; set; }
        public DateTime? ValueDate { get; set; }
        public string Description { get; set; }
        public string Reference { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal? Balance { get; set; }
        public decimal? Confidence { get; set; }
        /// <summary>موجب = إيداع، سالب = سحب.</summary>
        public decimal Signed => Credit - Debit;
    }

    /// <summary>حركة بنك فى الدفاتر (سطر سند خزينة بنكى). موجب = قبض (إيداع)، سالب = صرف.</summary>
    public class AiBankBookMove
    {
        public long LineId { get; set; }
        public long VoucherId { get; set; }
        public long VoucherNo { get; set; }
        public DateTime Date { get; set; }
        public decimal Signed { get; set; }
        public string Party { get; set; }
        public string Notes { get; set; }
        public bool Confirmed { get; set; }
    }

    public class AiBankMatchRow
    {
        public long Id { get; set; }
        public long Statement_Id { get; set; }
        public string Kind { get; set; }
        public List<int> Lines { get; set; } = new List<int>();
        public List<long> SystemRefs { get; set; } = new List<long>();
        public decimal Score { get; set; }
        public string Status { get; set; } = AiBankConst.StatusSuggested;
        public string Reason { get; set; }
    }

    /// <summary>تقرير التسوية (ب8).</summary>
    public class AiBankRecReport
    {
        public decimal BankClosing { get; set; }
        public decimal OutstandingDeposits { get; set; }
        public decimal OutstandingPayments { get; set; }
        public decimal BankOnlyIn { get; set; }
        public decimal BankOnlyOut { get; set; }
        public decimal AdjustedBank => BankClosing + OutstandingDeposits - OutstandingPayments;
        public decimal BookBalance { get; set; }
        public decimal Difference => AdjustedBank - BookBalance - (BankOnlyIn - BankOnlyOut);
    }

    public class AiBankView
    {
        public AiBankStatementRow Statement { get; set; }
        public List<AiBankLine> Lines { get; set; } = new List<AiBankLine>();
        public List<AiBankBookMove> Book { get; set; } = new List<AiBankBookMove>();
        public List<AiBankMatchRow> Matches { get; set; } = new List<AiBankMatchRow>();
        public AiBankRecReport Report { get; set; }
    }

    public class AiBankImportResult
    {
        public long StatementId { get; set; }
        public int Lines { get; set; }
        public bool Balanced { get; set; }
        public decimal? Diff { get; set; }
        public List<string> Warnings { get; set; } = new List<string>();
    }

    public interface IAiBankRecGateway
    {
        bool CanView(AiUserContext user);
        List<(long Id, string Name)> Banks(AiUserContext user);
        List<string> ReadHeaders(byte[] file, string fileName, AiUserContext user);
        AiBankColumnMap GetProfile(long bankId, AiUserContext user);
        AiBankImportResult Import(long bankId, byte[] file, string fileName, AiBankColumnMap map, decimal? opening, decimal? closing, AiUserContext user);
        System.Threading.Tasks.Task<AiBankImportResult> ImportExtractAsync(long bankId, long attachmentId, AiUserContext user, System.Threading.CancellationToken ct);
        List<AiBankStatementRow> Statements(long bankId, AiUserContext user);
        AiBankView Get(long statementId, AiUserContext user);
        System.Threading.Tasks.Task<int> MatchAsync(long statementId, AiUserContext user, System.Threading.CancellationToken ct);
        int Confirm(long statementId, IList<long> matchIds, AiUserContext user);
        bool Reject(long matchId, AiUserContext user);
        long ManualMatch(long statementId, IList<int> lines, IList<long> systemRefs, AiUserContext user);
        long JournalDraft(long statementId, int lineNo, long subAccountId, AiUserContext user);
        AiBankSettings GetSettings(AiUserContext user);
        void SaveSettings(AiBankSettings s, AiUserContext user);
    }

    public class AiBankException : InvalidOperationException
    {
        public string Code { get; }
        public AiBankException(string code, string message) : base(message) { Code = code ?? ""; }
    }

    /// <summary>
    /// P8.12.2 (ب3): المطابقة الثابتة (pure). (1) دقيقة: نفس المبلغ والاتجاه وتاريخ ± أيام، تفضيل تطابق المرجع ثم الأقرب تاريخاً — واحد لواحد.
    /// (2) تجميع: سطر كشف يقابل 2..N حركة دفاتر بنفس المجموع والاتجاه داخل نافذة 5 أيام (أو العكس). الحركات المؤكدة سابقاً مستبعدة (ب10).
    /// </summary>
    public static class AiBankMatcher
    {
        public static List<AiBankMatchRow> Match(IList<AiBankLine> lines, IList<AiBankBookMove> book, AiBankSettings s)
        {
            var result = new List<AiBankMatchRow>();
            var usedLines = new HashSet<int>();
            var usedBook = new HashSet<long>(book.Where(b => b.Confirmed).Select(b => b.LineId));
            var avail = book.Where(b => !b.Confirmed).ToList();

            // (1) دقيقة
            foreach (var l in lines.OrderBy(x => x.Date ?? DateTime.MaxValue))
            {
                if (l.Signed == 0 || l.Date == null) continue;
                var cands = avail.Where(b => !usedBook.Contains(b.LineId) && Math.Abs(b.Signed - l.Signed) <= AiBankConst.Tolerance
                                             && Math.Abs((b.Date - l.Date.Value).TotalDays) <= s.MatchDays).ToList();
                if (cands.Count == 0) continue;
                var best = cands.OrderByDescending(b => RefHit(l, b)).ThenBy(b => Math.Abs((b.Date - l.Date.Value).TotalDays)).First();
                bool refHit = RefHit(l, best);
                double days = Math.Abs((best.Date - l.Date.Value).TotalDays);
                decimal score = Math.Round(Math.Min(1m, 0.8m + (refHit ? 0.2m : 0m) - (decimal)days * 0.03m + (cands.Count == 1 ? 0.05m : 0m)), 4);
                result.Add(new AiBankMatchRow { Kind = AiBankConst.KindExact, Lines = { l.LineNo }, SystemRefs = { best.LineId }, Score = score,
                                                Reason = refHit ? "amount+date+reference" : "amount+date" });
                usedLines.Add(l.LineNo);
                usedBook.Add(best.LineId);
            }

            // (2) سطر كشف ⇐ عدة حركات
            foreach (var l in lines.Where(x => !usedLines.Contains(x.LineNo) && x.Date != null && x.Signed != 0))
            {
                var pool = avail.Where(b => !usedBook.Contains(b.LineId) && Math.Sign(b.Signed) == Math.Sign(l.Signed)
                                            && Math.Abs((b.Date - l.Date.Value).TotalDays) <= 5).OrderBy(b => b.Date).Take(15).ToList();
                var combo = Subset(pool.Select(b => b.Signed).ToList(), l.Signed, s.GroupMaxItems);
                if (combo == null) continue;
                var refs = combo.Select(i => pool[i].LineId).ToList();
                result.Add(new AiBankMatchRow { Kind = AiBankConst.KindGroup, Lines = { l.LineNo }, SystemRefs = refs, Score = 0.7m, Reason = "sum of " + refs.Count + " book lines" });
                usedLines.Add(l.LineNo);
                foreach (var r in refs) usedBook.Add(r);
            }

            // (3) حركة دفاتر ⇐ عدة أسطر كشف
            foreach (var b in avail.Where(x => !usedBook.Contains(x.LineId) && x.Signed != 0))
            {
                var pool = lines.Where(l => !usedLines.Contains(l.LineNo) && l.Date != null && Math.Sign(l.Signed) == Math.Sign(b.Signed)
                                            && Math.Abs((b.Date - l.Date.Value).TotalDays) <= 5).OrderBy(l => l.Date).Take(15).ToList();
                var combo = Subset(pool.Select(l => l.Signed).ToList(), b.Signed, s.GroupMaxItems);
                if (combo == null) continue;
                var ls = combo.Select(i => pool[i].LineNo).ToList();
                result.Add(new AiBankMatchRow { Kind = AiBankConst.KindGroup, Lines = ls, SystemRefs = { b.LineId }, Score = 0.7m, Reason = "sum of " + ls.Count + " statement lines" });
                foreach (var x in ls) usedLines.Add(x);
                usedBook.Add(b.LineId);
            }
            return result;
        }

        private static bool RefHit(AiBankLine l, AiBankBookMove b)
        {
            string r = Digits(l.Reference) ?? Digits(l.Description);
            if (string.IsNullOrEmpty(r) || r.Length < 3) return false;
            string vno = b.VoucherNo.ToString(CultureInfo.InvariantCulture);
            return r.Contains(vno) || (!string.IsNullOrEmpty(b.Notes) && b.Notes.Contains(r));
        }

        private static string Digits(string s)
        {
            if (string.IsNullOrEmpty(s)) return null;
            var d = new string(s.Where(char.IsDigit).ToArray());
            return d.Length == 0 ? null : d;
        }

        /// <summary>أول مجموعة جزئية (2..max) مجموعها = target (± التسامح). n ≤ 15.</summary>
        public static List<int> Subset(IList<decimal> values, decimal target, int max)
        {
            int n = values.Count;
            if (n < 2) return null;
            var stack = new List<int>();
            List<int> found = null;
            void Dfs(int start, decimal sum)
            {
                if (found != null) return;
                if (stack.Count >= 2 && Math.Abs(sum - target) <= AiBankConst.Tolerance) { found = new List<int>(stack); return; }
                if (stack.Count == max) return;
                for (int i = start; i < n && found == null; i++)
                {
                    stack.Add(i);
                    Dfs(i + 1, sum + values[i]);
                    stack.RemoveAt(stack.Count - 1);
                }
            }
            Dfs(0, 0);
            return found;
        }

        /// <summary>ب2: فحص الرصيد: الافتتاحى + الحركات = الختامى (± التسامح). null = لا أرصدة للفحص.</summary>
        public static decimal? BalanceDiff(decimal? opening, decimal? closing, IEnumerable<AiBankLine> lines)
        {
            if (opening == null || closing == null) return null;
            return Math.Round(opening.Value + lines.Sum(l => l.Signed) - closing.Value, 2);
        }
    }
}
