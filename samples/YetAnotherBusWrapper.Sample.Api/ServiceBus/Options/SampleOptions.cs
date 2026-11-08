namespace YetAnotherBusWrapper.Sample.Api.ServiceBus.Options;

internal sealed class SampleOptions
{
    public const string Position = "Sample";

    public string? InstanceName { get; init; }

    public bool UseCustomMessageSerializer { get; init; }

    public bool DisableHostedServices { get; init; }
}
