using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.BestEffortSending.Data.TestCases;

internal class T04MissingQueue : IBestEffortSendingTestCase
{
    public int TestCaseId => 4;

    public bool QueueExists => false;

    public IReadOnlyList<ReportGenerated> QueueMessages => [new(1, "report-1")];
}
