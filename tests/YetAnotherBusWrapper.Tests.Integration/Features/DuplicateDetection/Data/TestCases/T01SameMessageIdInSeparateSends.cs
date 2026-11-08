using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.DuplicateDetection.Data.TestCases;

internal class T01SameMessageIdInSeparateSends : IDuplicateDetectionTestCase
{
    public int TestCaseId => 1;

    public TimeSpan DuplicateDetectionWindow => TimeSpan.FromMinutes(1);

    public IReadOnlyList<IReadOnlyList<OrderPlaced>> SendCalls =>
    [
        [new OrderPlaced("order-001", 1, OrderStatus.New)],
        [new OrderPlaced("order-001", 2, OrderStatus.Paid)]
    ];
}
