using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.Batching.Data.TestCases;

internal class T02CollectionLargerThanBatchLimit : IBatchingTestCase
{
    private const int ContentLength = 120 * 1024;

    public int TestCaseId => 2;

    public IReadOnlyList<ReportGenerated> Messages =>
        Enumerable.Range(1, 5).Select(number => new ReportGenerated(number, new string('x', ContentLength))).ToList();
}
