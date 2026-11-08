using YetAnotherBusWrapper.HealthChecks;
using YetAnotherBusWrapper.Options;

namespace YetAnotherBusWrapper.Tests.Integration.Features.HealthChecks.Data.TestCases;

internal class T04MissingEntitiesWithPeekCheck : IHealthChecksTestCase
{
    public int TestCaseId => 4;

    public bool EntitiesExist => false;

    public void ConfigureQueue(QueueOptions options)
    {
        options.HealthCheck = HealthCheckKind.Peek;
    }

    public void ConfigureSubscription(string subscriptionLogicalName, SubscriptionOptions options)
    {
        options.HealthCheck = HealthCheckKind.Peek;
    }
}
