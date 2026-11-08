using Azure.Messaging.ServiceBus.Administration;

namespace YetAnotherBusWrapper.Tests.Integration.Features.TopicProvisioning.Data;

internal record CreateTopicOptionsDto(
    string Name,
    bool RequiresDuplicateDetection,
    TimeSpan DuplicateDetectionHistoryTimeWindow,
    TimeSpan AutoDeleteOnIdle
)
{
    public static CreateTopicOptionsDto From(CreateTopicOptions options)
    {
        return new CreateTopicOptionsDto(
            options.Name,
            options.RequiresDuplicateDetection,
            options.DuplicateDetectionHistoryTimeWindow,
            options.AutoDeleteOnIdle
        );
    }
}
