using System.Text.Json;
using AccountModelsLib;
using AccountModelsLib.SalesRep;

namespace AccountERP.AI.Collections
{
    /// <summary>
    /// P8.5.4 (ai-assistant-p85-plan.md ت21–ت29، §3 الإرسال): المسودة (قالب أو نموذج) والنسخ والإرسال المؤكَّد.
    /// الإرسال لا يكون إلا بضغطة المستخدم وconfirmed = true، وبعد كل ضوابط ت26 على الخادم؛ الرقم من الخادم فقط ولا يدخل التدقيق.
    /// </summary>
    public sealed partial class AiCollectionsGateway
    {
        private AiCollectionsDrafter? _drafter;
        public AiCollectionsDrafter Drafter
        {
            get => _drafter ??= new AiCollectionsDrafter(_registry ?? throw new InvalidOperationException("no provider registry"), _providers);
            set => _drafter = value;
        }

        /// <summary>اختبار: بيانات اتصال العميل بدل جدول Customer وسحب الموافقة.</summary>
        public Func<long, SalesRepNotifyService.ManualContact?>? ContactSource { get; set; }
        /// <summary>اختبار: إدراج الطابور (opp، القناة، العميل، مرجع الإجراء، الرقم، النص) ⇒ معرّف الصف.</summary>
        public Func<operations, int, long, long, string, string, long>? EnqueueSource { get; set; }

        public async Task<AiCollectDraft> DraftAsync(long customerId, string language, bool useModel, bool regenerate, AiUserContext user, CancellationToken ct)
        {
            var server = EnsureView(user);
            var s = Settings();
            var row = Find(server, Scope(server), s, customerId);
            string lang = AiBriefConst.NormalizeLanguage(language);
            int level = Math.Max(1, row.Level);
            var res = new AiCollectDraft { Level = level, Language = lang, Source = AiCollectDraftSource.Template };
            if (!useModel) return res;
            if (!s.Draft) { res.Source = AiCollectDraftSource.Off; return res; }
            string hash = AiCollectRules.FactsHash(row, level, lang);
            var existing = _dal.GetDraft(customerId, lang);
            if (!regenerate && existing != null && existing.FactsHash == hash
                && (existing.Source == AiCollectDraftSource.Model || existing.Source == AiCollectDraftSource.Rejected))
                return From(existing, level);
            if (_dal.DraftCountSince(Today().Date) >= s.DraftMaxPerDay) { res.Source = AiCollectDraftSource.Capped; return res; }
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var d = await Drafter.DraftAsync(row, level, lang, s, ct);
            if (d.Source != AiCollectDraftSource.Off) _dal.SaveDraft(d, server.UserId);
            AuditSafe(server.UserId, AiCollectConst.DraftAuditTool, new { customer_id = customerId, language = lang, level, source = d.Source, model = d.ModelName },
                      (int)sw.ElapsedMilliseconds, d.Source == AiCollectDraftSource.Model, d.RejectReason);
            return From(d, level);
        }

        private static AiCollectDraft From(AiCollectDraftRow d, int level) => new()
        {
            Text = d.Source == AiCollectDraftSource.Model ? d.DraftText : null, Source = d.Source, Level = level, Language = d.Language,
            ModelName = d.ModelName, RejectReason = d.RejectReason,
        };

        /// <summary>«نسخ»: يسجّل صف reminder_copied بالنص واللقطة؛ لا إرسال.</summary>
        public long LogCopied(long customerId, string text, string source, string language, AiUserContext user)
        {
            var server = EnsureView(user);
            var row = Find(server, Scope(server), Settings(), customerId);
            string t = CheckText(text);
            long id = _dal.InsertAction(Snapshot(row, AiCollectActionTypes.ReminderCopied, t, source, language), server.UserId);
            _scorer.Invalidate();
            return id;
        }

        public AiCollectSendPreview PreviewSend(long customerId, int channel, AiUserContext user)
        {
            var server = EnsureView(user);
            var s = Settings();
            var row = Find(server, Scope(server), s, customerId);
            var contact = Contact(customerId);
            string? block = Block(row, channel, s, contact);
            string phone = contact?.Phone ?? "";
            bool full = server.HasRoute(AiCollectConst.CustomersRoute);
            return new AiCollectSendPreview
            {
                CustomerName = row.Name ?? "", Channel = channel, CanSend = block == null, BlockCode = block,
                PhoneShown = phone.Length == 0 ? "" : full ? phone : "••••" + (phone.Length > 4 ? phone.Substring(phone.Length - 4) : phone),
            };
        }

        /// <summary>تسلسل §3: الضوابط ثم معاملة واحدة (إجراء + صف طابور + ربط). فشل أى خطوة ⇒ تراجع كامل.</summary>
        public AiCollectSendResult Send(long customerId, int channel, string text, string source, string language, bool confirmed, AiUserContext user)
        {
            var server = EnsureView(user);
            var s = Settings();
            if (!s.SendEnabled || NotifyProvider() == (int)SalesRepNotifyProvider.Off) throw new AiCollectException(AiCollectErrors.SendOff, "الإرسال غير مفعّل.");
            if (!confirmed) throw new AiCollectException(AiCollectErrors.NotConfirmed, "الإرسال يحتاج تأكيداً صريحاً.");
            string t = CheckText(text);
            var row = Find(server, Scope(server), s, customerId);
            var contact = Contact(customerId);
            string? block = Block(row, channel, s, contact);
            if (block != null) throw new AiCollectException(block, "لا يمكن الإرسال: " + block);

            var opp = new operations();
            long actionId, outboxId;
            using (var tx = opp.db.Database.BeginTransaction())
            {
                var dal = new AiCollectCLS(() => opp);
                var a = Snapshot(row, AiCollectActionTypes.ReminderSent, t, source, language);
                a.Channel = channel;
                actionId = dal.InsertAction(a, server.UserId);
                outboxId = EnqueueSource != null
                    ? EnqueueSource(opp, channel, customerId, actionId, contact!.Phone!, t)
                    : SalesRepNotifyService.EnqueueManual(opp, AiCollectConst.OutboxEventType, channel, customerId, actionId, contact!.Phone!, t);
                dal.SetOutboxId(actionId, outboxId);
                tx.Commit();
            }
            _scorer.Invalidate();
            AuditSafe(server.UserId, AiCollectConst.SendAuditTool, new { customer_id = customerId, channel, action_id = actionId, outbox_id = outboxId, chars = t.Length }, null, true, null);
            return new AiCollectSendResult { ActionId = actionId, OutboxId = outboxId };
        }

        // ───────────── مساعدات ─────────────

        /// <summary>ت26: سبب المنع (مفتاح resx) أو null.</summary>
        private string? Block(AiCollectRow row, int channel, AiCollectSettings s, SalesRepNotifyService.ManualContact? contact)
        {
            if (!s.SendEnabled || NotifyProvider() == (int)SalesRepNotifyProvider.Off) return AiCollectErrors.SendOff;
            if (channel != (int)SalesRepNotifyChannel.Sms && channel != (int)SalesRepNotifyChannel.WhatsApp) return AiCollectErrors.Channel;
            if (row.State == AiCollectState.Disputed) return AiCollectErrors.Disputed;
            var last = _dal.LastSentAt(row.CustomerId);
            if (last.HasValue && s.ReminderMinDays > 0 && last.Value > DateTime.Now.AddDays(-s.ReminderMinDays)) return AiCollectErrors.TooSoon;
            if (contact == null || string.IsNullOrWhiteSpace(contact.Phone)) return AiCollectErrors.NoPhone;
            if (contact.OptOutChannels.Contains(0) || contact.OptOutChannels.Contains(channel)) return AiCollectErrors.OptOut;
            return null;
        }

        private SalesRepNotifyService.ManualContact? Contact(long customerId)
        {
            if (ContactSource != null) return ContactSource(customerId);
            try { return SalesRepNotifyService.GetManualContact(new operations(), customerId); }
            catch (Exception ex) { try { operations.DataDictionaryLogerException(ex); } catch { } return null; }
        }

        private static string CheckText(string text)
        {
            string t = (text ?? "").Trim();
            if (t.Length == 0) throw new AiCollectException(AiCollectErrors.MessageEmpty, "نص الرسالة فارغ.");
            if (t.Length > AiCollectConst.MessageMaxChars) throw new AiCollectException(AiCollectErrors.MessageLength, "نص الرسالة أطول من 1000 حرف.");
            return t;
        }

        private static AiCollectActionRow Snapshot(AiCollectRow row, string type, string text, string source, string language) => new()
        {
            Customer_Id = row.CustomerId, ActionType = type, Status = AiCollectActionStatus.Closed, MessageText = text,
            MessageSource = string.IsNullOrWhiteSpace(source) ? AiCollectDraftSource.Template : source, Language = AiBriefConst.NormalizeLanguage(language),
            DunningLevel = Math.Max(1, row.Level), BalanceAt = row.Balance, OverdueAt = row.Overdue, OldestDaysAt = row.OldestDays, ScoreAt = row.Score,
        };

        private void AuditSafe(long userId, string tool, object args, int? ms, bool ok, string? error)
        {
            try { _audit.Insert(null, userId, tool, JsonSerializer.Serialize(args), null, null, ms, ok ? AiToolOutcome.Ok : AiToolOutcome.Error, error); }
            catch (Exception ex) { try { operations.DataDictionaryLogerException(ex); } catch { } }
        }
    }
}
