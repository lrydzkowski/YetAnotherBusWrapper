using YetAnotherBusWrapper.Options;

namespace YetAnotherBusWrapper.Tests.Integration.Features.Provisioning.Data.TestCases;

internal class T04MaxDeliveryCountAndDuplicateDetection : IProvisioningTestCase
{
    public int TestCaseId => 4;

    public void ConfigureQueue(QueueOptions options)
    {
        options.CreateOnStartup = true;
        options.MaxDeliveryCount = 4;
        options.DuplicateDetectionWindow = TimeSpan.FromMinutes(2);
    }
}
