using YetAnotherBusWrapper.Options;

namespace YetAnotherBusWrapper.Tests.Integration.Features.HealthChecks.Data.TestCases;

internal class T06DisabledSubscription : IHealthChecksTestCase
{
    public int TestCaseId => 6;

    public void ConfigureSubscription(string subscriptionLogicalName, SubscriptionOptions options)
    {
        if (subscriptionLogicalName != HealthChecksTests.BillingSubscriptionLogicalName)
        {
            return;
        }

        options.Enabled = false;
    }
}
