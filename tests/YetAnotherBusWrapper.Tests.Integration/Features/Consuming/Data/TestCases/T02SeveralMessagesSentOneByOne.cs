using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.Consuming.Data.TestCases;

internal class T02SeveralMessagesSentOneByOne : IConsumingTestCase
{
    public int TestCaseId => 2;

    public IReadOnlyList<OrderPlaced> Messages =>
    [
        new("order-001", 1, OrderStatus.New),
        new("order-002", 2, OrderStatus.Paid),
        new("order-003", 3, OrderStatus.New)
    ];

    public IReadOnlyList<OrderPlaced> MessagesSentAfterStop => [];
}
