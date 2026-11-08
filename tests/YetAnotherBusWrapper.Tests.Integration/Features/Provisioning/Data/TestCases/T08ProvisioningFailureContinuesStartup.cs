using Azure.Messaging.ServiceBus.Administration;
using YetAnotherBusWrapper.Options;

namespace YetAnotherBusWrapper.Tests.Integration.Features.Provisioning.Data.TestCases;

internal class T08ProvisioningFailureContinuesStartup : IProvisioningTestCase
{
    public int TestCaseId => 8;

    public void ConfigureQueue(QueueOptions options)
    {
        options.CreateOnStartup = true;
    }

    public void ConfigureCreateOptions(CreateQueueOptions options)
    {
        throw new InvalidOperationException("Create options rejected.");
    }
}
