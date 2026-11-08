using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.Batching.Data.TestCases;

internal class T04OversizedSingleMessage : IBatchingTestCase
{
    public int TestCaseId => 4;

    public IReadOnlyList<ReportGenerated> Messages => [new(1, new string('x', 300 * 1024))];
}
