using YetAnotherBusWrapper.Options;

namespace YetAnotherBusWrapper.Tests.Integration.Features.DisabledEntities.Data.TestCases;

internal class T03DisabledSubscription : IDisabledEntitiesTestCase
{
    public int TestCaseId => 3;

    public int ExpectedQueueDeliveries => 1;

    public int ExpectedBillingDeliveries => 0;

    public int ExpectedShippingDeliveries => 1;

    public void ConfigureSubscription(string subscriptionLogicalName, SubscriptionOptions options)
    {
        if (subscriptionLogicalName != DisabledEntitiesTests.BillingSubscriptionLogicalName)
        {
            return;
        }

        options.Enabled = false;
        options.SubscriptionName = null;
    }
}
