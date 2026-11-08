using YetAnotherBusWrapper.Tests.Integration.Common.Consumers;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;
using YetAnotherBusWrapper.Tests.Integration.Common.Models;

namespace YetAnotherBusWrapper.Tests.Integration.Features.Sessions.Data;

public interface ISessionsTestCase
{
    int TestCaseId { get; }
    ConsumerEntityKind EntityKind { get; }
    int MaxConcurrentSessions { get; }
    TimeSpan? SessionIdleTimeout { get; }
    HoldPlan HoldPlan { get; }
    IReadOnlyList<OrderPlaced> Messages { get; }
}
