using YetAnotherBusWrapper.Tests.Integration.Common.Context;

namespace YetAnotherBusWrapper.Tests.Integration.Features.OptionsReload.Data.TestCases;

internal class T01QueueNameChangedAfterStart : IOptionsReloadTestCase
{
    private const string RenamedQueueName = "renamed";

    public int TestCaseId => 1;

    public int ExpectedDeliveries => 1;

    public Dictionary<string, string?> GetInitialOptions(TestCaseScope scope)
    {
        return new Dictionary<string, string?> { ["DeleteOnShutdown"] = "true" };
    }

    public Dictionary<string, string?> GetChangedOptions(TestCaseScope scope)
    {
        return new Dictionary<string, string?> { ["QueueName"] = scope.GetQueueName(RenamedQueueName) };
    }

    public async Task PrepareAsync(TestCaseScope scope, CancellationToken cancellationToken)
    {
        await scope.CreateQueueAsync(RenamedQueueName, cancellationToken);
    }
}
