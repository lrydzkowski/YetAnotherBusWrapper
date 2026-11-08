using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.DuplicateDetection.Data.TestCases;

internal class T03SameMessageIdInOneCollection : IDuplicateDetectionTestCase
{
    public int TestCaseId => 3;

    public TimeSpan DuplicateDetectionWindow => TimeSpan.FromMinutes(1);

    public IReadOnlyList<IReadOnlyList<OrderPlaced>> SendCalls =>
    [
        [
            new OrderPlaced("order-001", 1, OrderStatus.New),
            new OrderPlaced("order-002", 2, OrderStatus.New),
            new OrderPlaced("ORDER-001", 3, OrderStatus.Paid)
        ]
    ];
}
