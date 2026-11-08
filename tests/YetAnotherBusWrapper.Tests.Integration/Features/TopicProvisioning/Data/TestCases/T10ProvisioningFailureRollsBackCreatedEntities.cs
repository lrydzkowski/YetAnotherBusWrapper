using Azure.Messaging.ServiceBus.Administration;
using YetAnotherBusWrapper.Options;

namespace YetAnotherBusWrapper.Tests.Integration.Features.TopicProvisioning.Data.TestCases;

internal class T10ProvisioningFailureRollsBackCreatedEntities : ITopicProvisioningTestCase
{
    public int TestCaseId => 10;

    public void ConfigureTopic(TopicOptions options)
    {
        options.CreateOnStartup = true;
        options.DeleteOnShutdown = true;
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
