using Azure.Messaging.ServiceBus.Administration;
using YetAnotherBusWrapper.Options;

namespace YetAnotherBusWrapper.Tests.Integration.Features.Provisioning.Data.TestCases;

internal class T09ProvisioningFailureFailsStartup : IProvisioningTestCase
{
    public int TestCaseId => 9;

    public void ConfigureQueue(QueueOptions options)
    {
        options.CreateOnStartup = true;
        options.FailStartupOnProvisioningError = true;
    }

    public void ConfigureCreateOptions(CreateQueueOptions options)
    {
        throw new InvalidOperationException("Create options rejected.");
    }
}
