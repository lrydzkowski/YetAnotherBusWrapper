using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.DuplicateDetection.Data;

public interface IDuplicateDetectionTestCase
{
    int TestCaseId { get; }
    TimeSpan DuplicateDetectionWindow { get; }
    IReadOnlyList<IReadOnlyList<OrderPlaced>> SendCalls { get; }
}
