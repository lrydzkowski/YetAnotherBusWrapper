using Azure.Messaging.ServiceBus.Administration;

namespace YetAnotherBusWrapper.Tests.Integration.Common.Emulator;

public record QueuePropertiesDto(
    string Name,
    int MaxDeliveryCount,
    bool RequiresDuplicateDetection,
    TimeSpan DuplicateDetectionHistoryTimeWindow,
    TimeSpan LockDuration
)
{
    public static QueuePropertiesDto From(QueueProperties properties)
    {
        return new QueuePropertiesDto(
            properties.Name,
            properties.MaxDeliveryCount,
            properties.RequiresDuplicateDetection,
            properties.DuplicateDetectionHistoryTimeWindow,
            properties.LockDuration
        );
    }
}
