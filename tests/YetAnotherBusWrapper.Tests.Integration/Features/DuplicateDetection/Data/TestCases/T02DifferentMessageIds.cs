using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.DuplicateDetection.Data.TestCases;

internal class T02DifferentMessageIds : IDuplicateDetectionTestCase
{
    public int TestCaseId => 2;

    public TimeSpan DuplicateDetectionWindow => TimeSpan.FromMinutes(1);

    public IReadOnlyList<IReadOnlyList<OrderPlaced>> SendCalls =>
    [
        [new OrderPlaced("order-001", 1, OrderStatus.New)],
        [new OrderPlaced("order-002", 2, OrderStatus.Paid)]
    ];
}
