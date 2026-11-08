using Azure.Messaging.ServiceBus;

namespace YetAnotherBusWrapper.Tests.Integration.Common.Emulator;

public record PeekedMessageDto(
    string MessageId,
    string? CorrelationId,
    string? SessionId,
    string? ContentType,
    string Body,
    IReadOnlyDictionary<string, object> ApplicationProperties
)
{
    public static PeekedMessageDto From(ServiceBusReceivedMessage message)
    {
        return new PeekedMessageDto(
            message.MessageId,
            message.CorrelationId,
            message.SessionId,
            message.ContentType,
            message.Body.ToString(),
            message.ApplicationProperties
        );
    }
}
