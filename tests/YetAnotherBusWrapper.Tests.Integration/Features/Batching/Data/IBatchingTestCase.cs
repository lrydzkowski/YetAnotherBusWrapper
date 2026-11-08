using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.Batching.Data;

public interface IBatchingTestCase
{
    int TestCaseId { get; }
    IReadOnlyList<ReportGenerated> Messages { get; }
}
