using YetAnotherBusWrapper.Options;

namespace YetAnotherBusWrapper.Tests.Integration.Features.TopicProvisioning.Data.TestCases;

internal class T04InstanceSuffix : ITopicProvisioningTestCase
{
    public int TestCaseId => 4;

    public void ConfigureTopic(TopicOptions options)
    {
        options.CreateOnStartup = true;
        options.UseInstanceSuffix = true;
    }

    public void ConfigureSubscription(string subscriptionLogicalName, SubscriptionOptions options)
    {
        options.CreateOnStartup = true;
        options.UseInstanceSuffix = subscriptionLogicalName == "billing";
    }
}
