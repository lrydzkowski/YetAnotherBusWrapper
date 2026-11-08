using YetAnotherBusWrapper.Tests.Integration.Common.Consumers;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;
using YetAnotherBusWrapper.Tests.Integration.Common.Models;

namespace YetAnotherBusWrapper.Tests.Integration.Features.Sessions.Data.TestCases;

internal class T01TwoSessionsOnQueue : ISessionsTestCase
{
    public int TestCaseId => 1;

    public ConsumerEntityKind EntityKind => ConsumerEntityKind.Queue;

    public int MaxConcurrentSessions => 2;

    public TimeSpan? SessionIdleTimeout => TimeSpan.FromSeconds(2);

    public HoldPlan HoldPlan => new(2, TimeSpan.FromSeconds(1));

    public IReadOnlyList<OrderPlaced> Messages =>
    [
        new("customer-a-001", 1, OrderStatus.New),
        new("customer-b-001", 1, OrderStatus.New),
        new("customer-a-002", 2, OrderStatus.Paid),
        new("customer-b-002", 2, OrderStatus.Paid),
        new("customer-a-003", 3, OrderStatus.New),
        new("customer-b-003", 3, OrderStatus.New)
    ];
}
