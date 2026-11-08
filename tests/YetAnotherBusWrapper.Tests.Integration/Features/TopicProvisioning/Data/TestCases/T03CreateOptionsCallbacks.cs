using Azure.Messaging.ServiceBus.Administration;
using YetAnotherBusWrapper.Options;

namespace YetAnotherBusWrapper.Tests.Integration.Features.TopicProvisioning.Data.TestCases;

internal class T03CreateOptionsCallbacks : ITopicProvisioningTestCase
{
    public int TestCaseId => 3;

    public void ConfigureTopic(TopicOptions options)
    {
        options.CreateOnStartup = true;
    }

    public void ConfigureSubscription(string subscriptionLogicalName, SubscriptionOptions options)
    {
        options.CreateOnStartup = true;
    }

    public void ConfigureTopicCreateOptions(CreateTopicOptions options)
    {
        options.AutoDeleteOnIdle = TimeSpan.FromMinutes(10);
    }

    public void ConfigureSubscriptionCreateOptions(CreateSubscriptionOptions options)
    {
        options.AutoDeleteOnIdle = TimeSpan.FromMinutes(15);
        options.LockDuration = TimeSpan.FromSeconds(45);
    }
}
