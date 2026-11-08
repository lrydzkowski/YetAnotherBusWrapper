using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.Consuming.Data;

public interface IConsumingTestCase
{
    int TestCaseId { get; }
    IReadOnlyList<OrderPlaced> Messages { get; }
    IReadOnlyList<OrderPlaced> MessagesSentAfterStop { get; }
}
