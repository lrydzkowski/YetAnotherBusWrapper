using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.BestEffortSending.Data.TestCases;

internal class T01EnabledQueue : IBestEffortSendingTestCase
{
    public int TestCaseId => 1;

    public IReadOnlyList<ReportGenerated> QueueMessages => [new(1, "report-1"), new(2, "report-2")];
}
