using YetAnotherBusWrapper.Tests.Integration.Common.Consumers;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.RetriesAndDeadLettering.Data.TestCases;

internal class T01ConsumerAlwaysThrows : IRetriesAndDeadLetteringTestCase
{
    public int TestCaseId => 1;

    public int MaxDeliveryCount => 3;

    public ConsumerFailurePlan FailurePlan => ConsumerFailurePlan.Always;

    public IReadOnlyList<OrderPlaced> Messages => [new("order-001", 3, OrderStatus.Paid)];

    public IReadOnlyList<RawMessageDto> RawMessages => [];

    public int DeliveriesToWaitFor => 3;

    public int DeadLetteredMessagesToWaitFor => 1;
}
