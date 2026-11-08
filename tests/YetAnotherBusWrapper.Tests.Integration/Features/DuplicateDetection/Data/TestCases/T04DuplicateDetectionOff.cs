using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.DuplicateDetection.Data.TestCases;

internal class T04DuplicateDetectionOff : IDuplicateDetectionTestCase
{
    public int TestCaseId => 4;

    public TimeSpan DuplicateDetectionWindow => TimeSpan.Zero;

    public IReadOnlyList<IReadOnlyList<OrderPlaced>> SendCalls =>
    [
        [new OrderPlaced("order-001", 1, OrderStatus.New)],
        [new OrderPlaced("order-001", 2, OrderStatus.Paid)]
    ];
}
