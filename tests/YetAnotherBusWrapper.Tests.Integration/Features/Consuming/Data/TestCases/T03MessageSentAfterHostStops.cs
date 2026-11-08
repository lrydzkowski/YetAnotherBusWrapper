using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.Consuming.Data.TestCases;

internal class T03MessageSentAfterHostStops : IConsumingTestCase
{
    public int TestCaseId => 3;

    public IReadOnlyList<OrderPlaced> Messages => [new("order-001", 1, OrderStatus.New)];

    public IReadOnlyList<OrderPlaced> MessagesSentAfterStop => [new("order-002", 2, OrderStatus.Paid)];
}
