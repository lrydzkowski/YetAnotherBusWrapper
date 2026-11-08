using YetAnotherBusWrapper.Tests.Integration.Common.Consumers;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;
using YetAnotherBusWrapper.Tests.Integration.Common.Models;

namespace YetAnotherBusWrapper.Tests.Integration.Features.Sessions.Data.TestCases;

internal class T02OneConcurrentSession : ISessionsTestCase
{
    public int TestCaseId => 2;

    public ConsumerEntityKind EntityKind => ConsumerEntityKind.Queue;

    public int MaxConcurrentSessions => 1;

    public TimeSpan? SessionIdleTimeout => TimeSpan.FromSeconds(1);

    public HoldPlan HoldPlan => new(2, TimeSpan.FromMilliseconds(500));

    public IReadOnlyList<OrderPlaced> Messages =>
    [
        new("customer-a-001", 1, OrderStatus.New),
        new("customer-b-001", 1, OrderStatus.New),
        new("customer-a-002", 2, OrderStatus.Paid),
        new("customer-b-002", 2, OrderStatus.Paid)
    ];
}
