namespace YetAnotherBusWrapper.Options;

public sealed class ClientRetryOptions
{
    public int MaxRetries { get; set; } = 2;

    public TimeSpan TryTimeout { get; set; } = TimeSpan.FromSeconds(10);

    public TimeSpan MaxDelay { get; set; } = TimeSpan.FromSeconds(2);
}
