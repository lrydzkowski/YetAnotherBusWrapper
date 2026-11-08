using YetAnotherBusWrapper.Options;

namespace YetAnotherBusWrapper.Tests.Integration.Features.TopicProvisioning.Data.TestCases;

internal class T05DeleteOnShutdown : ITopicProvisioningTestCase
{
    public int TestCaseId => 5;

    public void ConfigureTopic(TopicOptions options)
    {
        options.CreateOnStartup = true;
        options.DeleteOnShutdown = true;
    }

    public void ConfigureSubscription(string subscriptionLogicalName, SubscriptionOptions options)
    {
        options.CreateOnStartup = true;
        options.DeleteOnShutdown = true;
    }
}
