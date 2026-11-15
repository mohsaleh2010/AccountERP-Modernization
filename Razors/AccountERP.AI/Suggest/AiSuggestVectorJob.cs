using AccountERP.AI.Knowledge;
using AccountERP.AI.Scheduling;
using AccountModelsLib;

namespace AccountERP.AI.Suggest
{
    /// <summary>
    /// P8.8.2 (ai-assistant-p88-plan.md ص8): ليلاً (بعد 02:00 محلى، مرة يومياً) يضمّن النصوص المميزة الناقصة لكل نوع، حد 2000 نص/دورة.
    /// لا يعمل إن كانت الميزة موقوفة أو لا يوجد نموذج تضمين. الكتابة على AI_Suggestions_Vectors فقط.
    /// </summary>
    public sealed class AiSuggestVectorJob : IAiScheduledJob
    {
        public const int MaxTextsPerRun = 2000;
        public const int RunHour = 2;

        private readonly AiSuggestIndex _index;
        private readonly AiEmbeddingService _embed;
        private readonly AiProvidersCLS _providers;
        private DateTime _lastRunDate = DateTime.MinValue;
        private int _running;

        public string Name => "suggest_vectors";

        public AiSuggestVectorJob(AiSuggestIndex index, AiEmbeddingService embed, AiProvidersCLS? providers = null)
        {
            _index = index;
            _embed = embed;
            _providers = providers ?? new AiProvidersCLS();
        }

        public async Task TickAsync(DateTime nowUtc, CancellationToken ct)
        {
            var local = nowUtc.ToLocalTime();
            if (local.Hour < RunHour || _lastRunDate == local.Date) return;
            if (Interlocked.Exchange(ref _running, 1) == 1) return;
            try
            {
                var st = AiSuggestSettings.From(_providers.GetSettings());
                if (!st.Enabled) { _lastRunDate = local.Date; return; }
                await RunAsync(st, ct);
                _lastRunDate = local.Date;
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
            catch (Exception ex) { try { operations.DataDictionaryLogerException(ex); } catch { } _lastRunDate = local.Date; }
            finally { Interlocked.Exchange(ref _running, 0); }
        }

        /// <summary>يضمّن الناقص لنوعين؛ يعيد عدد النصوص المضافة.</summary>
        public async Task<int> RunAsync(AiSuggestSettings st, CancellationToken ct)
        {
            var route = _embed.CurrentRoute(out _);
            if (route == null) return 0;
            long modelId = route.Model.Id;
            try { _index.Dal.DeleteVectorsExceptModel(modelId); } catch (Exception ex) { try { operations.DataDictionaryLogerException(ex); } catch { } }
            int added = 0;
            foreach (var kind in new[] { AiSuggestKinds.Expense, AiSuggestKinds.Item })
            {
                var corpus = _index.Get(kind, st);
                var have = _index.Dal.VectorHashes(kind, modelId);
                var missing = corpus.ByNorm.Keys
                    .Select(n => (Norm: n, Hash: AiSuggestText.Hash(n)))
                    .Where(x => !have.Contains(x.Hash))
                    .OrderByDescending(x => corpus.ByNorm[x.Norm].Count)
                    .Take(Math.Max(0, MaxTextsPerRun - added)).ToList();
                if (missing.Count == 0) continue;
                for (int i = 0; i < missing.Count; i += 64)
                {
                    ct.ThrowIfCancellationRequested();
                    var batch = missing.Skip(i).Take(64).ToList();
                    var vecs = await _embed.EmbedDocumentsAsync(route, batch.Select(b => b.Norm).ToList(), ct);
                    for (int j = 0; j < batch.Count && j < vecs.Count; j++)
                    {
                        _index.Dal.InsertVector(kind, batch[j].Hash, batch[j].Norm, AiVectorMath.ToBytes(vecs[j]), modelId, vecs[j].Length);
                        added++;
                    }
                }
                if (added >= MaxTextsPerRun) break;
            }
            if (added > 0) _index.InvalidateVectors();
            return added;
        }
    }
}
