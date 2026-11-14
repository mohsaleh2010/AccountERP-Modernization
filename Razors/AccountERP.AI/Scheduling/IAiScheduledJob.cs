namespace AccountERP.AI.Scheduling
{
    /// <summary>
    /// مهمة دورية يديرها عامل واحد عام (P8 ق17: AiSchedulerWorker فى الويب كل 5 دقائق). الموجز اليومى أول مهمة؛ P8.4/P8.5/P8.6 تضيف مهامها.
    /// كل مهمة تقرر بنفسها هل حان وقتها (TickAsync رخيصة إن لم يحن)، وخطؤها لا يوقف العامل ولا المهام الأخرى.
    /// </summary>
    public interface IAiScheduledJob
    {
        /// <summary>اسم قصير للسجل.</summary>
        string Name { get; }

        /// <summary>نبضة: nowUtc من العامل (قابل للحقن فى الاختبار).</summary>
        Task TickAsync(DateTime nowUtc, CancellationToken ct);
    }
}
