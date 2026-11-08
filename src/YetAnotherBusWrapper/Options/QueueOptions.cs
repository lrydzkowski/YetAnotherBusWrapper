using YetAnotherBusWrapper.HealthChecks;

namespace YetAnotherBusWrapper.Options;

public sealed class QueueOptions
{
    public bool Enabled { get; set; } = true;

    public string? ConnectionString { get; set; }

    public string? AdministrationConnectionString { get; set; }

    public string? QueueName { get; set; }

    public bool UseInstanceSuffix { get; set; }

    public bool CreateOnStartup { get; set; }

    public bool DeleteOnShutdown { get; set; }

    public bool FailStartupOnProvisioningError { get; set; }

    public int MaxDeliveryCount { get; set; } = 10;

    public TimeSpan DuplicateDetectionWindow { get; set; }

    public bool BestEffort { get; set; }

    public HealthCheckKind HealthCheck { get; set; } = HealthCheckKind.Existence;

    public ClientRetryOptions ClientRetry { get; set; } = new();
}
