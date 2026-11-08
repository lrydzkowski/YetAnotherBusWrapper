namespace YetAnotherBusWrapper.Options;

public sealed class TopicOptions
{
    public bool Enabled { get; set; } = true;

    public string? ConnectionString { get; set; }

    public string? AdministrationConnectionString { get; set; }

    public string? TopicName { get; set; }

    public bool UseInstanceSuffix { get; set; }

    public bool CreateOnStartup { get; set; }

    public bool DeleteOnShutdown { get; set; }

    public bool FailStartupOnProvisioningError { get; set; }

    public TimeSpan DuplicateDetectionWindow { get; set; }

    public bool BestEffort { get; set; }

    public ClientRetryOptions ClientRetry { get; set; } = new();
}
