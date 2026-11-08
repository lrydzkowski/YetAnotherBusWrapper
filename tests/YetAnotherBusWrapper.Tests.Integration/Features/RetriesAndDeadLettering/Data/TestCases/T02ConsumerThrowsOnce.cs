using YetAnotherBusWrapper.Tests.Integration.Common.Consumers;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.RetriesAndDeadLettering.Data.TestCases;

internal class T02ConsumerThrowsOnce : IRetriesAndDeadLetteringTestCase
{
    public int TestCaseId => 2;

    public int MaxDeliveryCount => 3;

    public ConsumerFailurePlan FailurePlan => new(1);

    public IReadOnlyList<OrderPlaced> Messages => [new("order-001", 3, OrderStatus.Paid)];

    public IReadOnlyList<RawMessageDto> RawMessages => [];

    public int DeliveriesToWaitFor => 2;

    public int DeadLetteredMessagesToWaitFor => 0;
}
