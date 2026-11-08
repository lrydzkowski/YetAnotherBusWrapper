using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.ScheduledEnqueue.Data.TestCases;

internal class T01SingleMessage : IScheduledEnqueueTestCase
{
    public int TestCaseId => 1;

    public IReadOnlyList<OrderPlaced> Messages => [new("order-001", 1, OrderStatus.New)];
}
