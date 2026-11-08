using YetAnotherBusWrapper.Options;

namespace YetAnotherBusWrapper.Tests.Integration.Features.Provisioning.Data.TestCases;

internal class T11QueueCreatedDuringProvisioning : IProvisioningTestCase
{
    public int TestCaseId => 11;

    public bool QueueCreatedDuringProvisioning => true;

    public void ConfigureQueue(QueueOptions options)
    {
        options.CreateOnStartup = true;
        options.MaxDeliveryCount = 5;
    }
}
