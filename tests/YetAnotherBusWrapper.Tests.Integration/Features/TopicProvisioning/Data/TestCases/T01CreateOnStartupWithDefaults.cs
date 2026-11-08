using YetAnotherBusWrapper.Options;

namespace YetAnotherBusWrapper.Tests.Integration.Features.TopicProvisioning.Data.TestCases;

internal class T01CreateOnStartupWithDefaults : ITopicProvisioningTestCase
{
    public int TestCaseId => 1;

    public void ConfigureTopic(TopicOptions options)
    {
        options.CreateOnStartup = true;
    }

    public void ConfigureSubscription(string subscriptionLogicalName, SubscriptionOptions options)
    {
        options.CreateOnStartup = true;
    }
}
