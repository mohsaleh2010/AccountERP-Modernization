using System.Text.Json;

namespace AccountERP.SalesRepApp.Core
{
    public class SyncReport
    {
        public int Pushed { get; set; }
        public int Processed { get; set; }
        public int PendingReview { get; set; }
        public int Failed { get; set; }
        public int Rejected { get; set; }
        public Dictionary<string, int> Pulled { get; } = new();
        public bool Offline { get; set; }
        public string? Error { get; set; }
        public bool Ok => !Offline && Error == null;
    }

    /// <summary>
    /// المزامنة: (1) رفع الصادر بالترتيب على دفعات (إعادة الإرسال آمنة: السيرفر لا يكرر)، (2) متابعة حالة ما ينتظر المراجعة،
    /// (3) تنزيل الفرق لكل مجموعة. انقطاع الشبكة يوقف الدورة بلا فقد ويؤجل المحاولة بتأخير متزايد.
    /// </summary>
    public sealed class SyncEngine
    {
        public const int BatchSize = 20;
        /// <summary>المجموعات بترتيب التنزيل (= SalesRepSyncService.Datasets).</summary>
        public static readonly string[] Datasets =
            { "profile", "payment_methods", "return_reasons", "nosale_reasons", "banks", "customers", "items", "prices", "stock", "orders", "plan", "invoices", "order_items", "visits" };

        private readonly IRepApi _api;
        private readonly LocalStore _store;
        private readonly RepSession _session;
        private readonly Func<DateTime> _now;
        private readonly SemaphoreSlim _gate = new(1, 1);

        public SyncEngine(IRepApi api, LocalStore store, RepSession session, Func<DateTime>? now = null)
        {
            _api = api; _store = store; _session = session; _now = now ?? (() => DateTime.Now);
        }

        public bool Running => _gate.CurrentCount == 0;

        /// <param name="force">مزامنة يطلبها المندوب: ترفع كل الصادر فوراً؛ الخلفية (false) تحترم التأخير المتزايد بعد الفشل.</param>
        public async Task<SyncReport> SyncAsync(bool pull = true, bool force = true)
        {
            var report = new SyncReport();
            if (!await _gate.WaitAsync(0)) { report.Error = "مزامنة جارية."; return report; }
            try
            {
                string token = await _session.AccessTokenAsync();
                await PushAsync(token, report, force);
                await FollowUpAsync(token, report);
                if (pull)
                {
                    foreach (string ds in Datasets)
                    {
                        var r = await _api.PullAsync(token, ds, _store.Version(ds));
                        _store.ApplyPull(r);
                        report.Pulled[ds] = r.Rows.Count + r.Deleted.Count;
                    }
                    _session.MarkSynced();
                }
            }
            catch (RepOfflineException) { report.Offline = true; }
            catch (RepApiException e) { report.Error = e.Message; }
            finally { _gate.Release(); }
            return report;
        }

        private async Task PushAsync(string token, SyncReport report, bool force)
        {
            var due = _store.Outbox(openOnly: true)
                .Where(o => o.Status == OutboxStatus.Queued && (force || o.Next_Try_At == null || o.Next_Try_At <= _now())).ToList();
            foreach (var batch in due.Chunk(BatchSize))
            {
                var docs = batch.Select(o => new PushDoc
                {
                    Doc_Guid = o.Doc_Guid, Doc_Type = (int)o.Doc_Type, Created_At = o.Created_At, Doc_Seq = o.Doc_Seq,
                    Payload = JsonDocument.Parse(o.Payload).RootElement.Clone(),
                }).ToList();
                List<PushResult> results;
                try { results = await _api.PushAsync(token, docs); }
                catch (RepOfflineException)
                {
                    foreach (var o in batch)
                        _store.UpdateOutbox(o.Doc_Guid, OutboxStatus.Queued, o.Error, null, o.Attempts + 1, _now().Add(Backoff(o.Attempts + 1)));
                    throw;
                }
                var byGuid = results.ToDictionary(r => r.Doc_Guid);
                foreach (var o in batch)
                {
                    if (!byGuid.TryGetValue(o.Doc_Guid, out var r)) continue;
                    Apply(o, r, report);
                    report.Pushed++;
                }
            }
        }

        /// <summary>ما ينتظر المراجعة أو فيه خطأ: هل حفظه المكتب أو رفضه؟</summary>
        private async Task FollowUpAsync(string token, SyncReport report)
        {
            foreach (var o in _store.Outbox(openOnly: true).Where(o => o.Status != OutboxStatus.Queued).Take(50))
            {
                var r = await _api.StatusAsync(token, o.Doc_Guid);
                if (r == null) { _store.UpdateOutbox(o.Doc_Guid, OutboxStatus.Queued, o.Error, null, o.Attempts, null); continue; }   // ضاع على السيرفر: يُعاد رفعه
                if (r.Status != (int)o.Status || r.Error != o.Error) Apply(o, r, report);
            }
        }

        private void Apply(OutboxItem o, PushResult r, SyncReport report)
        {
            var status = (OutboxStatus)r.Status;
            _store.UpdateOutbox(o.Doc_Guid, status, r.Error, r.Result_Ref_Id, o.Attempts + 1, null);
            switch (status)
            {
                case OutboxStatus.Processed: report.Processed++; break;
                case OutboxStatus.PendingReview: report.PendingReview++; break;
                case OutboxStatus.Rejected: report.Rejected++; break;
                case OutboxStatus.Error: report.Failed++; break;
            }
        }

        /// <summary>1، 2، 4… دقائق حتى ساعة.</summary>
        public static TimeSpan Backoff(int attempts) => TimeSpan.FromMinutes(Math.Min(60, Math.Pow(2, Math.Max(0, attempts - 1))));
    }
}
