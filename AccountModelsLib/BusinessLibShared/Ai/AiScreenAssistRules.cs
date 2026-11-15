using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AccountModelsLib
{
    /// <summary>
    /// P8.9 (ai-assistant-p89-plan.md §1): دوال نقية بلا قاعدة بيانات — التحقق من الحمولة، سطر السياق المحيط، المصادر والأنواع، وكل النتائج بقواعد ثابتة.
    /// </summary>
    public static class AiScreenAssistRules
    {
        private static readonly CultureInfo C = CultureInfo.InvariantCulture;
        private static readonly string[] Filters = { "all", "with_balance", "movement_only" };
        private static readonly string[] Modes = { "totals", "closing", "normal", "period" };
        private static readonly DateTime MinDate = new DateTime(1990, 1, 1), MaxDate = new DateTime(2100, 12, 31);

        // ───────────── الحمولة ─────────────

        /// <summary>نسخة منظَّفة؛ نوع أو سؤال غريب أو تواريخ معكوسة أو أعداد سالبة ⇒ AiScreenAssistException(AIH_Err_BadRequest).</summary>
        public static AiScreenPayload Normalize(AiScreenPayload p)
        {
            if (p == null) throw Bad("الحمولة فارغة.");
            var r = p.Clone();
            r.Kind = (r.Kind ?? "").Trim();
            r.Question = (r.Question ?? "").Trim();
            if (!AiScreenKinds.IsKnown(r.Kind)) throw Bad("نوع شاشة غير معروف.");
            if (r.Question.Length > 0 && !AiScreenQuestions.For(r.Kind).Contains(r.Question, StringComparer.Ordinal)) throw Bad("سؤال غير معروف لهذه الشاشة.");
            r.Filter = Filters.Contains((r.Filter ?? "").Trim(), StringComparer.Ordinal) ? r.Filter.Trim() : "";
            r.Mode = Modes.Contains((r.Mode ?? "").Trim(), StringComparer.Ordinal) ? r.Mode.Trim() : "";
            if (r.From.HasValue) r.From = r.From.Value.Date;
            if (r.To.HasValue) r.To = r.To.Value.Date;
            if (r.From.HasValue && (r.From.Value < MinDate || r.From.Value > MaxDate)) throw Bad("تاريخ «من» خارج المدى.");
            if (r.To.HasValue && (r.To.Value < MinDate || r.To.Value > MaxDate)) throw Bad("تاريخ «إلى» خارج المدى.");
            if (r.From.HasValue && r.To.HasValue && r.From.Value > r.To.Value) throw Bad("تاريخ «من» بعد «إلى».");
            if (r.BranchId < 0 || r.CurrencyId < 0 || r.EntityId < 0) throw Bad("قيم سالبة.");
            // حقول لا تخص النوع تُفرَّغ حتى لا تغيّر البصمة.
            switch (r.Kind)
            {
                case AiScreenKinds.TrialBalance:
                    if (!r.From.HasValue || !r.To.HasValue || r.BranchId <= 0) { if (r.Question.Length > 0) throw Bad("الفترة والفرع مطلوبان."); }
                    if (r.Question == AiScreenQuestions.ExplainAccount && r.EntityId <= 0) throw Bad("الحساب مطلوب.");
                    if (r.Question == AiScreenQuestions.WhyUnbalanced) r.EntityId = 0;
                    break;
                case AiScreenKinds.CustomerStatement:
                    r.BranchId = 0; r.CurrencyId = 0; r.Filter = "";
                    if (r.Mode != "normal" && r.Mode != "period") r.Mode = "normal";
                    if (r.Question.Length > 0 && r.EntityId <= 0) throw Bad("العميل مطلوب.");
                    break;
                default:
                    r.BranchId = 0; r.CurrencyId = 0; r.Filter = ""; r.Mode = ""; r.From = null; r.To = null;
                    if (r.Question.Length > 0 && r.EntityId <= 0) throw Bad("المستند مطلوب.");
                    break;
            }
            return r;
        }

        private static AiScreenAssistException Bad(string msg) => new AiScreenAssistException(AiScreenAssistErrors.BadRequest, msg);

        /// <summary>سطر السياق المحيط (§2-ج): محددات فقط بلا أسماء ولا أرقام مالية.</summary>
        public static string Describe(AiScreenPayload p)
        {
            if (p == null || !AiScreenKinds.IsKnown(p.Kind)) return "";
            var parts = new List<string>();
            if (p.From.HasValue && p.To.HasValue) parts.Add("من " + D(p.From.Value) + " إلى " + D(p.To.Value));
            else if (p.From.HasValue) parts.Add("من " + D(p.From.Value));
            else if (p.To.HasValue) parts.Add("حتى " + D(p.To.Value));
            switch (p.Kind)
            {
                case AiScreenKinds.TrialBalance:
                    if (p.BranchId > 0) parts.Add("branch_id=" + p.BranchId.ToString(C));
                    parts.Add("currency_id=" + p.CurrencyId.ToString(C));
                    if (!string.IsNullOrEmpty(p.Filter)) parts.Add("filter=" + p.Filter);
                    if (p.EntityId > 0) parts.Add("account_id=" + p.EntityId.ToString(C));
                    break;
                case AiScreenKinds.CustomerStatement:
                    if (p.EntityId > 0) parts.Add("customer_id=" + p.EntityId.ToString(C));
                    break;
                case AiScreenKinds.SalesInvoice:
                    if (p.EntityId > 0) parts.Add("sale_id=" + p.EntityId.ToString(C));
                    break;
                case AiScreenKinds.SalesReturn:
                    if (p.EntityId > 0) parts.Add("return_id=" + p.EntityId.ToString(C));
                    break;
            }
            return "المعروض فيها الآن: " + p.Kind + (parts.Count > 0 ? " " + string.Join("، ", parts) : "") +
                   ". حين يقول المستخدم «هذا» أو «هذه الشاشة» فاستعمل هذه القيم مع أدواتك؛ لا أرقام هنا.";
        }

        /// <summary>المسار الأساسى للنوع.</summary>
        public static string RouteOf(string kind)
        {
            switch (kind)
            {
                case AiScreenKinds.TrialBalance: return AiScreenAssistConst.RTrialBalance;
                case AiScreenKinds.CustomerStatement: return AiScreenAssistConst.RStatementNormal;
                case AiScreenKinds.SalesInvoice: return AiScreenAssistConst.RInvoice;
                case AiScreenKinds.SalesReturn: return AiScreenAssistConst.RReturn;
                default: return "";
            }
        }

        /// <summary>أى مسار منها يمنح صلاحية النوع (ص6، §3).</summary>
        public static string[] RoutesOf(string kind)
        {
            switch (kind)
            {
                case AiScreenKinds.TrialBalance: return new[] { AiScreenAssistConst.RTrialBalance };
                case AiScreenKinds.CustomerStatement: return new[] { AiScreenAssistConst.RStatementNormal, AiScreenAssistConst.RStatementPeriod };
                case AiScreenKinds.SalesInvoice:
                case AiScreenKinds.SalesReturn: return new[] { AiScreenAssistConst.RSalesList, AiScreenAssistConst.RInvoice, AiScreenAssistConst.RReturn };
                default: return new string[0];
            }
        }

        /// <summary>النوع من مسار الصفحة (بلا معاملات الاستعلام)، أو فارغ.</summary>
        public static string KindOfRoute(string route)
        {
            string r = (route ?? "").Split('?')[0].TrimEnd('/');
            if (r.Equals(AiScreenAssistConst.RTrialBalance, StringComparison.OrdinalIgnoreCase)) return AiScreenKinds.TrialBalance;
            if (r.StartsWith("/accounts/customer-account-statement", StringComparison.OrdinalIgnoreCase)) return AiScreenKinds.CustomerStatement;
            if (r.StartsWith(AiScreenAssistConst.RInvoice, StringComparison.OrdinalIgnoreCase)) return AiScreenKinds.SalesInvoice;
            if (r.StartsWith(AiScreenAssistConst.RReturn, StringComparison.OrdinalIgnoreCase)) return AiScreenKinds.SalesReturn;
            return "";
        }

        /// <summary>مصدر حركة الحساب (§1-أ).</summary>
        public static string SourceOf(int referenceType, bool isOpening, bool isClosing)
        {
            if (isOpening) return "opening";
            if (isClosing) return "closing";
            switch (referenceType)
            {
                case 0: return "manual";
                case 1: case 7: return "sales";
                case 2: case 8: return "purchases";
                case 6: return "cash";
                default: return "other";
            }
        }

        /// <summary>نوع حركة العميل (§1-ب).</summary>
        public static string TypeOf(int? referenceType, string ledgerSource)
        {
            if (string.Equals(ledgerSource, "linked_supplier", StringComparison.OrdinalIgnoreCase)) return "linked";
            switch (referenceType ?? int.MinValue)
            {
                case 1: case 53: case -1: return "invoices";
                case 3: case 5: case 27: case 35: return "receipts";
                case 4: case 6: case 28: case 36: return "payments";
                case 7: return "returns";
                case -7: case -8: case 0: return "opening";
                default: return "other";
            }
        }

        // ───────────── الحساب ─────────────

        /// <summary>نسبة مقرّبة لمنزلة واحدة (صفر عند مقام صفر).</summary>
        public static decimal Pct(decimal part, decimal whole) =>
            whole == 0 ? 0 : Math.Round(part * 100m / whole, 1, MidpointRounding.AwayFromZero);

        /// <summary>
        /// الشرائح من الفواتير المفتوحة: [غير مستحق، 1–30، 31–60، 61–90، +90، غير مرتبط]. المجموع = الرصيد (الموجب).
        /// </summary>
        public static decimal[] Buckets(IList<AiCollectInvoice> open, decimal balance)
        {
            var b = new decimal[6];
            if (balance <= AiScreenAssistConst.Tolerance) return b;
            foreach (var i in open ?? new List<AiCollectInvoice>())
            {
                int d = i.DaysOverdue;
                int k = d <= 0 ? 0 : d <= 30 ? 1 : d <= 60 ? 2 : d <= 90 ? 3 : 4;
                b[k] += i.OpenAmount;
            }
            decimal alloc = b[0] + b[1] + b[2] + b[3] + b[4];
            b[5] = Math.Max(0, balance - alloc);
            if (b[5] <= AiScreenAssistConst.Tolerance) b[5] = 0;
            return b;
        }

        // ───────────── النتائج ─────────────

        public static AiScreenFinding F(string code, string severity, params string[] kv)
        {
            var f = new AiScreenFinding { Code = code, Severity = severity };
            for (int i = 0; i + 1 < kv.Length; i += 2) f.Values[kv[i]] = kv[i + 1];
            return f;
        }

        public static string Amt(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero).ToString("0.00", C);
        public static string Int(long v) => v.ToString(C);
        public static string P1(decimal v) => Math.Round(v, 1, MidpointRounding.AwayFromZero).ToString("0.#", C);
        public static string D(DateTime d) => d.ToString("yyyy-MM-dd", C);

        /// <summary>
        /// ميزان المراجعة (ص9، ص10): openDiff/moveDiff من المحرك؛ moveCount/moveTotal القيود غير المتزنة فى [من، إلى]؛ priorCount/priorTotal فى [بداية الفترة، من).
        /// </summary>
        public static List<AiScreenFinding> TrialBalanceFindings(decimal openDiff, decimal moveDiff, long currencyId, int moveCount, decimal moveTotal, int priorCount, decimal priorTotal)
        {
            var l = new List<AiScreenFinding>();
            decimal t = AiScreenAssistConst.Tolerance, m = AiScreenAssistConst.MatchTolerance;
            bool balanced = Math.Abs(openDiff) < t && Math.Abs(moveDiff) < t && Math.Abs(openDiff + moveDiff) < t;
            if (balanced) { l.Add(F("tb_balanced", AiScreenSeverity.Info)); return l; }
            if (currencyId > 0) { l.Add(F("tb_currency", AiScreenSeverity.Warn)); return l; }
            bool moveUnexplained = false;
            if (moveCount > 0)
            {
                l.Add(F("tb_move_entries", AiScreenSeverity.High, "count", Int(moveCount), "total", Amt(moveTotal)));
                if (Math.Abs(moveDiff - moveTotal) <= m) l.Add(F("tb_move_all", AiScreenSeverity.Info));
                else if (Math.Abs(moveDiff) >= t) l.Add(F("tb_move_rest", AiScreenSeverity.Warn, "rest", Amt(moveDiff - moveTotal)));
            }
            else if (Math.Abs(moveDiff) >= t) moveUnexplained = true;
            if (priorCount > 0) l.Add(F("tb_open_entries", AiScreenSeverity.High, "count", Int(priorCount), "total", Amt(priorTotal)));
            if (Math.Abs(openDiff - priorTotal) > m) l.Add(F("tb_open_stored", AiScreenSeverity.High, "total", Amt(openDiff - priorTotal)));
            if (l.Count == 0 || moveUnexplained) l.Add(F("tb_none", AiScreenSeverity.Warn));
            return l;
        }

        /// <summary>
        /// رصيد حساب (ص11). sources = (المصدر، مدين + دائن)؛ topEntry = أكبر قيد بمجموع مدينه ودائنه؛ manual = حركة القيود اليدوية وعددها.
        /// groupPct = null ⇒ مجموع المجموعة صفر. withDetail = false ⇒ لا نتائج تعتمد على التفصيل.
        /// </summary>
        public static List<AiScreenFinding> AccountFindings(decimal moveDebit, decimal moveCredit, decimal? groupPct,
            IList<KeyValuePair<string, decimal>> sources, string topEntryNo, decimal topEntryAmount, decimal manualAmount, int manualCount, bool withDetail)
        {
            var l = new List<AiScreenFinding>();
            decimal move = moveDebit + moveCredit;
            if (move < AiScreenAssistConst.Tolerance) l.Add(F("acc_no_movement", AiScreenSeverity.Info));
            if (groupPct.HasValue) l.Add(F("acc_group_share", AiScreenSeverity.Info, "pct", P1(groupPct.Value)));
            if (!withDetail || move < AiScreenAssistConst.Tolerance) return l;
            var top = (sources ?? new List<KeyValuePair<string, decimal>>()).OrderByDescending(s => s.Value).FirstOrDefault();
            if (top.Key != null && top.Value * 2 >= move) l.Add(F("acc_top_source", AiScreenSeverity.Info, "source", top.Key, "pct", P1(Pct(top.Value, move))));
            if (!string.IsNullOrEmpty(topEntryNo) && topEntryAmount * 2 >= move)
                l.Add(F("acc_dominant", AiScreenSeverity.Warn, "entry_no", topEntryNo, "pct", P1(Pct(topEntryAmount, move))));
            if (manualCount > 0 && manualAmount * 100 >= move * 30)
                l.Add(F("acc_manual", AiScreenSeverity.Warn, "pct", P1(Pct(manualAmount, move)), "count", Int(manualCount)));
            return l;
        }

        /// <summary>
        /// رصيد عميل (ص12، ص13). buckets من Buckets (أو null حين لا أعمار)؛ rangeNet = null حين المدى غير مقيَّد بتاريخ «من».
        /// </summary>
        public static List<AiScreenFinding> CustomerFindings(decimal balance, decimal[] buckets, int overdueCount, int oldestDays, int invoiceMonths,
            DateTime? lastReceipt, DateTime today, decimal? creditLimit, decimal? rangeNet)
        {
            var l = new List<AiScreenFinding>();
            decimal t = AiScreenAssistConst.Tolerance;
            if (Math.Abs(balance) < t) l.Add(F("cu_zero", AiScreenSeverity.Info));
            else if (balance < 0) l.Add(F("cu_credit_side", AiScreenSeverity.Info, "amount", Amt(-balance)));
            else
            {
                var b = buckets ?? new decimal[6];
                decimal overdueInv = b[1] + b[2] + b[3] + b[4];
                if (overdueInv > t)
                    l.Add(F("cu_overdue", oldestDays > 90 ? AiScreenSeverity.High : AiScreenSeverity.Warn, "amount", Amt(overdueInv), "days", Int(oldestDays), "count", Int(overdueCount)));
                if (b[5] > t) l.Add(F("cu_unallocated", AiScreenSeverity.Warn, "amount", Amt(b[5]), "months", Int(invoiceMonths)));
                if (lastReceipt.HasValue)
                {
                    int days = (today.Date - lastReceipt.Value.Date).Days;
                    if (days > 30) l.Add(F("cu_no_receipt", days > 90 ? AiScreenSeverity.High : AiScreenSeverity.Warn, "days", Int(days)));
                }
                else l.Add(F("cu_never_receipt", AiScreenSeverity.Warn));
                if (creditLimit.HasValue && balance > creditLimit.Value + t)
                    l.Add(F("cu_over_limit", AiScreenSeverity.High, "limit", Amt(creditLimit.Value), "amount", Amt(balance - creditLimit.Value)));
            }
            if (rangeNet.HasValue && Math.Abs(rangeNet.Value) >= t)
                l.Add(rangeNet.Value > 0 ? F("cu_range_up", AiScreenSeverity.Info, "amount", Amt(rangeNet.Value))
                                         : F("cu_range_down", AiScreenSeverity.Info, "amount", Amt(-rangeNet.Value)));
            return l;
        }

        /// <summary>
        /// فاتورة/مرتجع (ص14). linesTotal/linesTax = null ⇒ لا مقارنة (الفحص 8). علما الضريبة من الـ View فقط (القاعدة 11).
        /// </summary>
        public static List<AiScreenFinding> InvoiceFindings(bool isReturn, decimal total, decimal? linesTotal, decimal salesTax, decimal? linesTax,
            bool isTaxInvoice, bool isTaxPerItem, bool isCredit, decimal rest, DateTime? dueDate, DateTime today, string einvoiceState, bool isPosted)
        {
            var l = new List<AiScreenFinding>();
            decimal m = AiScreenAssistConst.MatchTolerance;
            if (linesTotal.HasValue && Math.Abs(linesTotal.Value - total) > m)
                l.Add(F("inv_lines_mismatch", AiScreenSeverity.High, "lines", Amt(linesTotal.Value), "total", Amt(total), "diff", Amt(linesTotal.Value - total)));
            if (isTaxPerItem)
            {
                l.Add(F("inv_tax_per_item", AiScreenSeverity.Info));
                if (linesTax.HasValue && Math.Abs(linesTax.Value - salesTax) > m)
                    l.Add(F("inv_tax_mismatch", AiScreenSeverity.High, "lines", Amt(linesTax.Value), "total", Amt(salesTax), "diff", Amt(linesTax.Value - salesTax)));
            }
            if (!isReturn && isCredit)
            {
                if (rest > AiScreenAssistConst.Tolerance)
                {
                    l.Add(F("inv_unpaid", AiScreenSeverity.Warn, "amount", Amt(rest)));
                    if (dueDate.HasValue && dueDate.Value.Date < today.Date)
                        l.Add(F("inv_overdue", AiScreenSeverity.High, "days", Int((today.Date - dueDate.Value.Date).Days)));
                }
                if (!dueDate.HasValue) l.Add(F("inv_no_due", AiScreenSeverity.Info));
            }
            if (isTaxInvoice)
            {
                if (einvoiceState == "not_sent") l.Add(F("inv_einv_not_sent", AiScreenSeverity.Warn));
                else if (einvoiceState == "sent") l.Add(F("inv_einv_sent", AiScreenSeverity.Info));
            }
            if (!isPosted) l.Add(F("inv_not_posted", AiScreenSeverity.Warn));
            return l;
        }

        /// <summary>أيام التأخير لفاتورة آجلة لها باقٍ (0 إن لا).</summary>
        public static int DaysOverdue(bool isCredit, decimal rest, DateTime? dueDate, DateTime today) =>
            isCredit && rest > AiScreenAssistConst.Tolerance && dueDate.HasValue && dueDate.Value.Date < today.Date ? (today.Date - dueDate.Value.Date).Days : 0;
    }
}
