using YetAnotherBusWrapper.Tests.Integration.Common.Consumers;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;
using YetAnotherBusWrapper.Tests.Integration.Common.Models;

namespace YetAnotherBusWrapper.Tests.Integration.Features.ConsumerProcessing.Data.TestCases;

internal class T01DefaultOptions : IConsumerProcessingTestCase
{
    public int TestCaseId => 1;

    public ConsumerEntityKind EntityKind => ConsumerEntityKind.Queue;

    public TimeSpan? LockDuration => null;

    public int? MaxConcurrentCalls => null;

    public TimeSpan? MaxAutoLockRenewalDuration => null;

    public HoldPlan HoldPlan => new(3, TimeSpan.FromSeconds(1));

    public IReadOnlyList<OrderPlaced> Messages =>
    [
        new("order-001", 1, OrderStatus.New),
        new("order-002", 2, OrderStatus.Paid),
        new("order-003", 3, OrderStatus.New)
    ];

    public int DeliveriesToWaitFor => 3;
}
