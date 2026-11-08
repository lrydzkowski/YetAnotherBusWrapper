using YetAnotherBusWrapper.Options;

namespace YetAnotherBusWrapper.Tests.Integration.Features.TopicProvisioning.Data.TestCases;

internal class T06SubscriptionDeletedOnShutdown : ITopicProvisioningTestCase
{
    public int TestCaseId => 6;

    public void ConfigureTopic(TopicOptions options)
    {
        options.CreateOnStartup = true;
    }

    public void ConfigureSubscription(string subscriptionLogicalName, SubscriptionOptions options)
    {
        options.CreateOnStartup = true;
        options.DeleteOnShutdown = subscriptionLogicalName == "shipping";
    }
}
