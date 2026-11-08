namespace YetAnotherBusWrapper.Options;

internal interface IConsumerProcessingOptions
{
    int MaxConcurrentCalls { get; }

    int PrefetchCount { get; }

    TimeSpan MaxAutoLockRenewalDuration { get; }

    bool SessionsEnabled { get; }

    int MaxConcurrentSessions { get; }

    TimeSpan? SessionIdleTimeout { get; }
}
