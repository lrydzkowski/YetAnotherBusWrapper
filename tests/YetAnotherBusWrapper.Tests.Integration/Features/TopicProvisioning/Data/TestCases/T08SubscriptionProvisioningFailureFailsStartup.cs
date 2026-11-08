using Azure.Messaging.ServiceBus.Administration;
using YetAnotherBusWrapper.Options;

namespace YetAnotherBusWrapper.Tests.Integration.Features.TopicProvisioning.Data.TestCases;

internal class T08SubscriptionProvisioningFailureFailsStartup : ITopicProvisioningTestCase
{
    public int TestCaseId => 8;

    public void ConfigureTopic(TopicOptions options)
    {
        options.CreateOnStartup = true;
    }

    public void ConfigureSubscription(string subscriptionLogicalName, SubscriptionOptions options)
    {
        options.CreateOnStartup = true;
        options.FailStartupOnProvisioningError = true;
    }

    public void ConfigureSubscriptionCreateOptions(CreateSubscriptionOptions options)
    {
        throw new InvalidOperationException("Create options rejected.");
    }
}
