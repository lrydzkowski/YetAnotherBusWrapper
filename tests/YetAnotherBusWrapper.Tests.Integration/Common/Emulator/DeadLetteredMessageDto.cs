using Azure.Messaging.ServiceBus;

namespace YetAnotherBusWrapper.Tests.Integration.Common.Emulator;

public record DeadLetteredMessageDto(
    string MessageId,
    string? CorrelationId,
    string Body,
    int DeliveryCount,
    string? DeadLetterReason
)
{
    public static DeadLetteredMessageDto From(ServiceBusReceivedMessage message)
    {
        return new DeadLetteredMessageDto(
            message.MessageId,
            message.CorrelationId,
            message.Body.ToString(),
            message.DeliveryCount,
            message.DeadLetterReason
        );
    }
}
