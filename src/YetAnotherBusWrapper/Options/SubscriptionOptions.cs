using YetAnotherBusWrapper.HealthChecks;

namespace YetAnotherBusWrapper.Options;

public sealed class SubscriptionOptions : IConsumerProcessingOptions
{
    public bool Enabled { get; set; } = true;

    public string? SubscriptionName { get; set; }

    public bool UseInstanceSuffix { get; set; }

    public bool CreateOnStartup { get; set; }

    public bool DeleteOnShutdown { get; set; }

    public bool FailStartupOnProvisioningError { get; set; }

    public int MaxDeliveryCount { get; set; } = 10;

    public HealthCheckKind HealthCheck { get; set; } = HealthCheckKind.Existence;

    public int MaxConcurrentCalls { get; set; } = 1;

    public int PrefetchCount { get; set; }

    public TimeSpan MaxAutoLockRenewalDuration { get; set; } = TimeSpan.FromMinutes(5);

    public bool SessionsEnabled { get; set; }

    public int MaxConcurrentSessions { get; set; } = 8;

    public TimeSpan? SessionIdleTimeout { get; set; }
}
