using YetAnotherBusWrapper.Options;

namespace YetAnotherBusWrapper.Tests.Integration.Features.Provisioning.Data.TestCases;

internal class T10UnresolvableQueueName : IProvisioningTestCase
{
    public int TestCaseId => 10;

    public void ConfigureQueue(QueueOptions options)
    {
        options.QueueName = "***";
        options.CreateOnStartup = true;
        options.DeleteOnShutdown = true;
    }
}
