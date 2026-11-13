using AccountModelsLib;

namespace AccountERP.SharedUI.Services
{
    /// <summary>
    /// P5 (ai-assistant-plan.md §6): ما تحتاجه شاشات المستندات لفتح مسودة المساعد (?aiDraft=): التحميل بشرط المالك والنوع والحالة،
    /// ثم الإغلاق بعد الحفظ الناجح. لا يكتب إلا فى AI_Drafts. الشاشة نفسها تبنى الأسطر بمنطقها.
    /// </summary>
    public static class AiDraftPageHelper
    {
        public sealed class LoadResult
        {
            public AiDraftRow? Row { get; init; }
            public AiDraftPayload? Payload { get; init; }
            /// <summary>مفتاح ترجمة الخطأ (المسودة غير موجودة / لنوع آخر)، أو null.</summary>
            public string? ErrorKey { get; init; }
            /// <summary>المسودة حُفظت من قبل: رقم المستند الناتج ومعرّفه.</summary>
            public bool AlreadyUsed { get; init; }
        }

        public static async Task<LoadResult> LoadAsync(long draftId, string kind, int orderType = 0)
        {
            long userId = Common.User_Id;
            var row = await Task.Run(() => { AccountCultureGuard.EnsureInvariant(); return new AiDraftsCLS().Get(draftId, userId); });
            if (row == null) return new LoadResult { ErrorKey = "AID_NotFound" };
            var payload = row.Payload;
            if (payload == null || !string.Equals(row.DraftType, kind, StringComparison.OrdinalIgnoreCase) || (orderType > 0 && payload.OrderType != orderType))
                return new LoadResult { Row = row, ErrorKey = "AID_WrongScreen" };
            if (row.Status == AiDraftStatus.Consumed) return new LoadResult { Row = row, Payload = payload, AlreadyUsed = true };
            if (row.Status != AiDraftStatus.Pending) return new LoadResult { Row = row, ErrorKey = "AID_Discarded" };
            return new LoadResult { Row = row, Payload = payload };
        }

        public static Task<List<AiDraftStockRow>> StockAsync(IEnumerable<long> unitItemIds)
        {
            var ids = unitItemIds.ToList();
            return Task.Run(() => { AccountCultureGuard.EnsureInvariant(); return new AiDraftsCLS().GetStock(ids); });
        }

        /// <summary>بعد نجاح حفظ المستند. لا يُسقط الشاشة لو فشل (المستند نفسه حُفظ).</summary>
        public static async Task MarkConsumedAsync(long draftId, long docId, string docNo)
        {
            if (draftId <= 0) return;
            long userId = Common.User_Id;
            try { await Task.Run(() => { AccountCultureGuard.EnsureInvariant(); new AiDraftsCLS().MarkConsumed(draftId, userId, docId, docNo); }); }
            catch (Exception ex) { operations.DataDictionaryLogerException(ex); }
        }

        public static async Task DiscardAsync(long draftId)
        {
            long userId = Common.User_Id;
            await Task.Run(() => { AccountCultureGuard.EnsureInvariant(); new AiDraftsCLS().Discard(draftId, userId); });
        }
    }
}
