using YetAnotherBusWrapper.Tests.Integration.Common.Consumers;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;
using YetAnotherBusWrapper.Tests.Integration.Common.Models;

namespace YetAnotherBusWrapper.Tests.Integration.Features.ConsumerProcessing.Data;

public interface IConsumerProcessingTestCase
{
    int TestCaseId { get; }
    ConsumerEntityKind EntityKind { get; }
    TimeSpan? LockDuration { get; }
    int? MaxConcurrentCalls { get; }
    TimeSpan? MaxAutoLockRenewalDuration { get; }
    HoldPlan HoldPlan { get; }
    IReadOnlyList<OrderPlaced> Messages { get; }
    int DeliveriesToWaitFor { get; }
}
