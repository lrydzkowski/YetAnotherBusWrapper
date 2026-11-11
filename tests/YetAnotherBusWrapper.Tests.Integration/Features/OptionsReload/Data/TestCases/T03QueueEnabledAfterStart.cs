using YetAnotherBusWrapper.Tests.Integration.Common.Context;

namespace YetAnotherBusWrapper.Tests.Integration.Features.OptionsReload.Data.TestCases;

internal class T03QueueEnabledAfterStart : IOptionsReloadTestCase
{
    public int TestCaseId => 3;

    public int ExpectedDeliveries => 0;

    public Dictionary<string, string?> GetInitialOptions(TestCaseScope scope)
    {
        return new Dictionary<string, string?> { ["Enabled"] = "false" };
    }

    public Dictionary<string, string?> GetChangedOptions(TestCaseScope scope)
    {
        return new Dictionary<string, string?> { ["Enabled"] = "true" };
    }
}
