using YetAnotherBusWrapper.Options;

namespace YetAnotherBusWrapper.Tests.Integration.Features.Provisioning.Data.TestCases;

internal class T02CreateOnStartupWithDefaults : IProvisioningTestCase
{
    public int TestCaseId => 2;

    public void ConfigureQueue(QueueOptions options)
    {
        options.CreateOnStartup = true;
    }
}
