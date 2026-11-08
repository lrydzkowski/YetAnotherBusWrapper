namespace YetAnotherBusWrapper.Options;

public sealed class QueueConsumerOptions : IConsumerProcessingOptions
{
    public int MaxConcurrentCalls { get; set; } = 1;

    public int PrefetchCount { get; set; }

    public TimeSpan MaxAutoLockRenewalDuration { get; set; } = TimeSpan.FromMinutes(5);

    public bool SessionsEnabled { get; set; }

    public int MaxConcurrentSessions { get; set; } = 8;

    public TimeSpan? SessionIdleTimeout { get; set; }
}
