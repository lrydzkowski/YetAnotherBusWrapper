using YetAnotherBusWrapper.Tests.Integration.Common.Consumers;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;
using YetAnotherBusWrapper.Tests.Integration.Common.Models;

namespace YetAnotherBusWrapper.Tests.Integration.Features.ConsumerProcessing.Data.TestCases;

internal class T04LockRenewedByDefault : IConsumerProcessingTestCase
{
    public int TestCaseId => 4;

    public ConsumerEntityKind EntityKind => ConsumerEntityKind.Queue;

    public TimeSpan? LockDuration => TimeSpan.FromSeconds(5);

    public int? MaxConcurrentCalls => null;

    public TimeSpan? MaxAutoLockRenewalDuration => null;

    public HoldPlan HoldPlan => HoldPlan.For(TimeSpan.FromSeconds(8));

    public IReadOnlyList<OrderPlaced> Messages => [new("order-001", 1, OrderStatus.New)];

    public int DeliveriesToWaitFor => 1;
}
