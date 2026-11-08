using Azure.Messaging.ServiceBus;

namespace YetAnotherBusWrapper.Tests.Integration.Features.ScheduledEnqueue.Data;

internal record ScheduledMessageDto(string MessageId, string State, bool ScheduledForRequestedTime)
{
    private static readonly TimeSpan Tolerance = TimeSpan.FromSeconds(1);

    public static ScheduledMessageDto From(ServiceBusReceivedMessage message, DateTimeOffset requestedTime)
    {
        return new ScheduledMessageDto(
            message.MessageId,
            message.State.ToString(),
            (message.ScheduledEnqueueTime - requestedTime).Duration() < Tolerance
        );
    }
}
