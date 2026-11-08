using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.Batching.Data.TestCases;

internal class T03OversizedMessageInCollection : IBatchingTestCase
{
    public int TestCaseId => 3;

    public IReadOnlyList<ReportGenerated> Messages =>
    [
        new(1, "report-1"),
        new(2, new string('x', 300 * 1024)),
        new(3, "report-3")
    ];
}
