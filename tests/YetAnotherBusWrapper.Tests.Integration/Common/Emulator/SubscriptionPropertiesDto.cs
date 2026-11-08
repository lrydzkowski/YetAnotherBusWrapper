using Azure.Messaging.ServiceBus.Administration;

namespace YetAnotherBusWrapper.Tests.Integration.Common.Emulator;

public record SubscriptionPropertiesDto(
    string TopicName,
    string SubscriptionName,
    int MaxDeliveryCount,
    TimeSpan LockDuration
)
{
    public static SubscriptionPropertiesDto From(SubscriptionProperties properties)
    {
        return new SubscriptionPropertiesDto(
            properties.TopicName,
            properties.SubscriptionName,
            properties.MaxDeliveryCount,
            properties.LockDuration
        );
    }
}
