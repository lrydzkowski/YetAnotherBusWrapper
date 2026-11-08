using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.ScheduledEnqueue.Data;

public interface IScheduledEnqueueTestCase
{
    int TestCaseId { get; }
    IReadOnlyList<OrderPlaced> Messages { get; }
}
