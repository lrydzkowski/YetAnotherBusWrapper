using Azure.Messaging.ServiceBus.Administration;
using YetAnotherBusWrapper.Options;

namespace YetAnotherBusWrapper.Tests.Integration.Features.TopicProvisioning.Data;

public interface ITopicProvisioningTestCase
{
    int TestCaseId { get; }
    bool EntitiesExistBeforeStart => false;
    void ConfigureTopic(TopicOptions options);
    void ConfigureSubscription(string subscriptionLogicalName, SubscriptionOptions options);

    void ConfigureTopicCreateOptions(CreateTopicOptions options)
    {
    }

    void ConfigureSubscriptionCreateOptions(CreateSubscriptionOptions options)
    {
    }
}
