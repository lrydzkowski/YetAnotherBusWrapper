using YetAnotherBusWrapper.Tests.Integration.Common.Context;

namespace YetAnotherBusWrapper.Tests.Integration.Features.OptionsReload.Data.TestCases;

internal class T02QueueDisabledAfterStart : IOptionsReloadTestCase
{
    public int TestCaseId => 2;

    public int ExpectedDeliveries => 1;

    public Dictionary<string, string?> GetInitialOptions(TestCaseScope scope)
    {
        return [];
    }

    public Dictionary<string, string?> GetChangedOptions(TestCaseScope scope)
    {
        return new Dictionary<string, string?> { ["Enabled"] = "false" };
    }
}
