using YetAnotherBusWrapper.Options;

namespace YetAnotherBusWrapper.Tests.Integration.Features.Provisioning.Data.TestCases;

internal class T03ExistingQueueLeftUnchanged : IProvisioningTestCase
{
    public int TestCaseId => 3;

    public bool QueueExistsBeforeStart => true;

    public void ConfigureQueue(QueueOptions options)
    {
        options.CreateOnStartup = true;
        options.MaxDeliveryCount = 5;
    }
}
