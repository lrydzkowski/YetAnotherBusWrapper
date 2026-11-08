using Azure.Messaging.ServiceBus.Administration;

namespace YetAnotherBusWrapper.Tests.Integration.Common.Emulator;

public record TopicPropertiesDto(
    string Name,
    bool RequiresDuplicateDetection,
    TimeSpan DuplicateDetectionHistoryTimeWindow
)
{
    public static TopicPropertiesDto From(TopicProperties properties)
    {
        return new TopicPropertiesDto(
            properties.Name,
            properties.RequiresDuplicateDetection,
            properties.DuplicateDetectionHistoryTimeWindow
        );
    }
}
