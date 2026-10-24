using AccountModelsLib.SalesRep;

namespace AccountERP.Web.Services
{
    /// <summary>
    /// مرسل إشعارات عملاء المندوبين (المرحلة 8): يفحص طابور SalesRep_Notify_Outbox كل نصف دقيقة ويرسل المستحق،
    /// فالإرسال من السيرفر لا من جهاز المندوب. فشل الإرسال أو غياب الجدول لا يوقف الخدمة (يُسجَّل ويُعاد الفحص).
    /// </summary>
    public sealed class SalesRepNotifyWorker : BackgroundService
    {
        private static readonly TimeSpan Interval = TimeSpan.FromSeconds(30);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // تأخير بدء قصير حتى يكتمل تحميل إعدادات النظام عند الإقلاع.
            try { await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken); } catch (OperationCanceledException) { return; }
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Run(() => { AccountERP.SharedUI.Services.AccountCultureGuard.EnsureInvariant(); new SalesRepNotifyService().ProcessPending(20); }, stoppingToken);
                }
                catch (OperationCanceledException) { return; }
                catch (Exception ee) { AccountModelsLib.operations.DataDictionaryLogerException(ee); }
                try { await Task.Delay(Interval, stoppingToken); } catch (OperationCanceledException) { return; }
            }
        }
    }
}
