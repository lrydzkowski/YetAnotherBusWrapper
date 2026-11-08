using YetAnotherBusWrapper.Options;

namespace YetAnotherBusWrapper.Tests.Integration.Features.DisabledEntities.Data;

public interface IDisabledEntitiesTestCase
{
    int TestCaseId { get; }
    int ExpectedQueueDeliveries { get; }
    int ExpectedBillingDeliveries { get; }
    int ExpectedShippingDeliveries { get; }

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
