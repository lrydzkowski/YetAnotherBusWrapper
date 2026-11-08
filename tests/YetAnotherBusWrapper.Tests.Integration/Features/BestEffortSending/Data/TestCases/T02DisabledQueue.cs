using YetAnotherBusWrapper.Options;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.BestEffortSending.Data.TestCases;

internal class T02DisabledQueue : IBestEffortSendingTestCase
{
    public int TestCaseId => 2;

    public IReadOnlyList<ReportGenerated> QueueMessages => [new(1, "report-1")];

    public void ConfigureQueue(QueueOptions options)
    {
        options.Enabled = false;
        options.ConnectionString = "";
    }
}
