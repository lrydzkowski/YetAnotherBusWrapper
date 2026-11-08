using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.BestEffortSending.Data.TestCases;

internal class T05OversizedMessageAfterFirstBatch : IBestEffortSendingTestCase
{
    public int TestCaseId => 5;

    public IReadOnlyList<ReportGenerated> QueueMessages =>
    [
        new(1, "report-1"),
        new(2, new string('x', 300 * 1024)),
        new(3, "report-3")
    ];
}
