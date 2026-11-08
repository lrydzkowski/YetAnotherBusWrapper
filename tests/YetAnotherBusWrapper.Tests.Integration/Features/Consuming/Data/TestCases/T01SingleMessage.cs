using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.Consuming.Data.TestCases;

internal class T01SingleMessage : IConsumingTestCase
{
    public int TestCaseId => 1;

    public IReadOnlyList<OrderPlaced> Messages => [new("order-001", 3, OrderStatus.Paid)];

    public IReadOnlyList<OrderPlaced> MessagesSentAfterStop => [];
}
