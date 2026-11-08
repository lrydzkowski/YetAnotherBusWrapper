using YetAnotherBusWrapper.HealthChecks;
using YetAnotherBusWrapper.Options;

namespace YetAnotherBusWrapper.Tests.Integration.Features.HealthChecks.Data.TestCases;

internal class T07SessionEntitiesWithPeekCheck : IHealthChecksTestCase
{
    public int TestCaseId => 7;

    public bool EntitiesRequireSession => true;

    public void ConfigureQueue(QueueOptions options)
    {
        options.HealthCheck = HealthCheckKind.Peek;
    }

    public void ConfigureSubscription(string subscriptionLogicalName, SubscriptionOptions options)
    {
        options.HealthCheck = HealthCheckKind.Peek;
    }
}
