using YetAnotherBusWrapper.Options;

namespace YetAnotherBusWrapper.Tests.Integration.Features.Provisioning.Data.TestCases;

internal class T06DeleteOnShutdown : IProvisioningTestCase
{
    public int TestCaseId => 6;

    public void ConfigureQueue(QueueOptions options)
    {
        options.CreateOnStartup = true;
        options.DeleteOnShutdown = true;
    }
}
