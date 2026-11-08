using YetAnotherBusWrapper.Options;

namespace YetAnotherBusWrapper.Tests.Integration.Features.Provisioning.Data.TestCases;

internal class T01CreateOnStartupNotSet : IProvisioningTestCase
{
    public int TestCaseId => 1;

    public void ConfigureQueue(QueueOptions options)
    {
    }
}
