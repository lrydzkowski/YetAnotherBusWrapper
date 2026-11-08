using Azure.Messaging.ServiceBus.Administration;

namespace YetAnotherBusWrapper.Tests.Integration.Features.TopicProvisioning.Data;

internal record CreateSubscriptionOptionsDto(
    string TopicName,
    string SubscriptionName,
    int MaxDeliveryCount,
    TimeSpan LockDuration,
    TimeSpan AutoDeleteOnIdle
)
{
    public static CreateSubscriptionOptionsDto From(CreateSubscriptionOptions options)
    {
        return new CreateSubscriptionOptionsDto(
            options.TopicName,
            options.SubscriptionName,
            options.MaxDeliveryCount,
            options.LockDuration,
            options.AutoDeleteOnIdle
        );
    }
}
