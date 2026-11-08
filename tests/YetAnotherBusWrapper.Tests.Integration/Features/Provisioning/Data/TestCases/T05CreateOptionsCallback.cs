using Azure.Messaging.ServiceBus.Administration;
using YetAnotherBusWrapper.Options;

namespace YetAnotherBusWrapper.Tests.Integration.Features.Provisioning.Data.TestCases;

internal class T05CreateOptionsCallback : IProvisioningTestCase
{
    public int TestCaseId => 5;

    public void ConfigureQueue(QueueOptions options)
    {
        options.CreateOnStartup = true;
    }

    public void ConfigureCreateOptions(CreateQueueOptions options)
    {
        options.AutoDeleteOnIdle = TimeSpan.FromMinutes(10);
        options.LockDuration = TimeSpan.FromSeconds(45);
    }
}
