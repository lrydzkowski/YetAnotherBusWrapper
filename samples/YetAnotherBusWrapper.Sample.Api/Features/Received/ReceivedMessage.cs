namespace YetAnotherBusWrapper.Sample.Api.Features.Received;

public sealed record ReceivedMessage(
    string Consumer,
    object Message,
    string MessageId,
    string CorrelationId,
    int DeliveryCount,
    string? SessionId,
    DateTimeOffset EnqueuedTime,
    DateTimeOffset ReceivedTime,
    IReadOnlyDictionary<string, object> ApplicationProperties
);
