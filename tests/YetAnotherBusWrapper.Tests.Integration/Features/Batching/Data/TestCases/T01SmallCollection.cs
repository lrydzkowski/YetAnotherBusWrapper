using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.Batching.Data.TestCases;

internal class T01SmallCollection : IBatchingTestCase
{
    public int TestCaseId => 1;

    public IReadOnlyList<ReportGenerated> Messages =>
    [
        new(1, "report-1"),
        new(2, "report-2"),
        new(3, "report-3")
    ];
}
