using AccountERP.AI.Scheduling;

namespace AccountERP.Web.Services
{
    /// <summary>
    /// P8 ق17: عامل واحد عام لمهام المساعد الدورية (IAiScheduledJob) — الموجز اليومى أولها. بنمط SalesRepNotifyWorker:
    /// تأخير بدء 60 ث ثم نبضة كل 5 دقائق؛ كل مهمة داخل Task.Run بثقافة ثابتة، وخطؤها يُسجَّل ولا يوقف العامل ولا المهام الأخرى.
    /// </summary>
    public sealed class AiSchedulerWorker : BackgroundService
    {
        private static readonly TimeSpan StartDelay = TimeSpan.FromSeconds(60);
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);
        private readonly IEnumerable<IAiScheduledJob> _jobs;

        public AiSchedulerWorker(IEnumerable<IAiScheduledJob> jobs) => _jobs = jobs;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try { await Task.Delay(StartDelay, stoppingToken); } catch (OperationCanceledException) { return; }
            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (var job in _jobs)
                {
                    try
                    {
                        await Task.Run(async () =>
                        {
                            AccountERP.SharedUI.Services.AccountCultureGuard.EnsureInvariant();
                            await job.TickAsync(DateTime.UtcNow, stoppingToken);
                        }, stoppingToken);
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { return; }
                    catch (Exception ee) { AccountModelsLib.operations.DataDictionaryLogerException(ee); }
                }
                try { await Task.Delay(Interval, stoppingToken); } catch (OperationCanceledException) { return; }
            }
        }
    }
}
