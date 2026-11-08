using YetAnotherBusWrapper.Options;

namespace YetAnotherBusWrapper.Tests.Integration.Features.TopicProvisioning.Data.TestCases;

internal class T09SubscriptionNameLongerThanLimit : ITopicProvisioningTestCase
{
    public int TestCaseId => 9;

    public void ConfigureTopic(TopicOptions options)
    {
        options.CreateOnStartup = true;
    }

    public void ConfigureSubscription(string subscriptionLogicalName, SubscriptionOptions options)
    {
        options.SubscriptionName = $"{subscriptionLogicalName}-{new string('a', 60)}";
        options.CreateOnStartup = true;
    }
}
