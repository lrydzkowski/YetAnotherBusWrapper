using YetAnotherBusWrapper.Options;

namespace YetAnotherBusWrapper.Tests.Integration.Features.Provisioning.Data.TestCases;

internal class T07DeletionFailure : IProvisioningTestCase
{
    public int TestCaseId => 7;

    public void ConfigureQueue(QueueOptions options)
    {
        options.DeleteOnShutdown = true;
    }
}
