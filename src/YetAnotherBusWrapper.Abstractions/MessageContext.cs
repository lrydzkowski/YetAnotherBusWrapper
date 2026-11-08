namespace YetAnotherBusWrapper.Abstractions;

public sealed class MessageContext
{
    public required string MessageId { get; init; }

    public required string CorrelationId { get; init; }

    public required int DeliveryCount { get; init; }

    public string? SessionId { get; init; }

    public required DateTimeOffset EnqueuedTime { get; init; }

    public required IReadOnlyDictionary<string, object> ApplicationProperties { get; init; }
}
