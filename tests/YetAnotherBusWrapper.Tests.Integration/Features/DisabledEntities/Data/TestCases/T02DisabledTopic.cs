using YetAnotherBusWrapper.Options;

namespace YetAnotherBusWrapper.Tests.Integration.Features.DisabledEntities.Data.TestCases;

internal class T02DisabledTopic : IDisabledEntitiesTestCase
{
    public int TestCaseId => 2;

    public int ExpectedQueueDeliveries => 1;

    public int ExpectedBillingDeliveries => 0;

    public int ExpectedShippingDeliveries => 0;

    public void ConfigureTopic(TopicOptions options)
    {
        options.Enabled = false;
        options.ConnectionString = "";
        options.TopicName = null;
    }

    public void ConfigureSubscription(string subscriptionLogicalName, SubscriptionOptions options)
    {
        options.SubscriptionName = null;
    }
}
