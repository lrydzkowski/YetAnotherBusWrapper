using YetAnotherBusWrapper.Options;

namespace YetAnotherBusWrapper.Tests.Integration.Features.HealthChecks.Data;

public interface IHealthChecksTestCase
{
    int TestCaseId { get; }
    bool EntitiesExist => true;
    bool EntitiesRequireSession => false;

    void ConfigureQueue(QueueOptions options)
    {
    }

    void ConfigureTopic(TopicOptions options)
    {
    }

    void ConfigureSubscription(string subscriptionLogicalName, SubscriptionOptions options)
    {
    }
}
