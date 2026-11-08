using YetAnotherBusWrapper.Options;

namespace YetAnotherBusWrapper.Tests.Integration.Features.TopicProvisioning.Data.TestCases;

internal class T02MaxDeliveryCountAndDuplicateDetection : ITopicProvisioningTestCase
{
    public int TestCaseId => 2;

    public void ConfigureTopic(TopicOptions options)
    {
        options.CreateOnStartup = true;
        options.DuplicateDetectionWindow = TimeSpan.FromMinutes(5);
    }

    public void ConfigureSubscription(string subscriptionLogicalName, SubscriptionOptions options)
    {
        options.CreateOnStartup = true;
        options.MaxDeliveryCount = subscriptionLogicalName == "billing" ? 3 : 7;
    }
}
