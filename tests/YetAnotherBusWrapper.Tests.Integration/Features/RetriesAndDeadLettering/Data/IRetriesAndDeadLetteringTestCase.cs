using YetAnotherBusWrapper.Tests.Integration.Common.Consumers;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.RetriesAndDeadLettering.Data;

public interface IRetriesAndDeadLetteringTestCase
{
    int TestCaseId { get; }
    int MaxDeliveryCount { get; }
    ConsumerFailurePlan FailurePlan { get; }
    IReadOnlyList<OrderPlaced> Messages { get; }
    IReadOnlyList<RawMessageDto> RawMessages { get; }
    int DeliveriesToWaitFor { get; }
    int DeadLetteredMessagesToWaitFor { get; }
}
