using YetAnotherBusWrapper.Tests.Integration.Common.Consumers;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;
using YetAnotherBusWrapper.Tests.Integration.Common.Models;

namespace YetAnotherBusWrapper.Tests.Integration.Features.ConsumerProcessing.Data.TestCases;

internal class T03SubscriptionMaxConcurrentCalls : IConsumerProcessingTestCase
{
    public int TestCaseId => 3;

    public ConsumerEntityKind EntityKind => ConsumerEntityKind.Subscription;

    public TimeSpan? LockDuration => null;

    public int? MaxConcurrentCalls => 3;

    public TimeSpan? MaxAutoLockRenewalDuration => null;

    public HoldPlan HoldPlan => new(3, TimeSpan.FromSeconds(5));

    public IReadOnlyList<OrderPlaced> Messages =>
    [
        new("order-001", 1, OrderStatus.New),
        new("order-002", 2, OrderStatus.Paid),
        new("order-003", 3, OrderStatus.New)
    ];

    public int DeliveriesToWaitFor => 3;
}
