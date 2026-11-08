using Azure.Messaging.ServiceBus.Administration;
using YetAnotherBusWrapper.Options;

namespace YetAnotherBusWrapper.Tests.Integration.Features.Provisioning.Data;

public interface IProvisioningTestCase
{
    int TestCaseId { get; }
    bool QueueExistsBeforeStart => false;
    bool QueueCreatedDuringProvisioning => false;
    void ConfigureQueue(QueueOptions options);

    void ConfigureCreateOptions(CreateQueueOptions options)
    {
    }
}
