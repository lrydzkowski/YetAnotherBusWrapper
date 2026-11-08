using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.Batching.Data.TestCases;

internal class T05EmptyCollection : IBatchingTestCase
{
    public int TestCaseId => 5;

    public IReadOnlyList<ReportGenerated> Messages => [];
}
