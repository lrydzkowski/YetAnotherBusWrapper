using Azure.Messaging.ServiceBus.Administration;

namespace YetAnotherBusWrapper.Tests.Integration.Features.Provisioning.Data;

internal record CreateQueueOptionsDto(
    string Name,
    int MaxDeliveryCount,
    bool RequiresDuplicateDetection,
    TimeSpan DuplicateDetectionHistoryTimeWindow,
    TimeSpan LockDuration,
    TimeSpan AutoDeleteOnIdle
)
{
    public static CreateQueueOptionsDto From(CreateQueueOptions options)
    {
        return new CreateQueueOptionsDto(
            options.Name,
            options.MaxDeliveryCount,
            options.RequiresDuplicateDetection,
            options.DuplicateDetectionHistoryTimeWindow,
            options.LockDuration,
            options.AutoDeleteOnIdle
        );
    }
}
