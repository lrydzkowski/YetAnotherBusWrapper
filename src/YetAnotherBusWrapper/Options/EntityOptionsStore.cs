using System.Collections.Concurrent;
using Microsoft.Extensions.Options;

namespace YetAnotherBusWrapper.Options;

internal sealed class EntityOptionsStore
{
    private readonly IOptionsFactory<QueueConsumerOptions> _queueConsumerOptionsFactory;
    private readonly ConcurrentDictionary<string, QueueConsumerOptions> _queueConsumers = new(StringComparer.Ordinal);
    private readonly IOptionsFactory<QueueOptions> _queueOptionsFactory;
    private readonly ConcurrentDictionary<string, QueueOptions> _queues = new(StringComparer.Ordinal);
    private readonly IOptionsFactory<SubscriptionOptions> _subscriptionOptionsFactory;
    private readonly ConcurrentDictionary<string, SubscriptionOptions> _subscriptions = new(StringComparer.Ordinal);
    private readonly IOptionsFactory<TopicOptions> _topicOptionsFactory;
    private readonly ConcurrentDictionary<string, TopicOptions> _topics = new(StringComparer.Ordinal);

    public EntityOptionsStore(
        IOptionsFactory<QueueOptions> queueOptionsFactory,
        IOptionsFactory<QueueConsumerOptions> queueConsumerOptionsFactory,
        IOptionsFactory<TopicOptions> topicOptionsFactory,
        IOptionsFactory<SubscriptionOptions> subscriptionOptionsFactory
    )
    {
        _queueOptionsFactory = queueOptionsFactory;
        _queueConsumerOptionsFactory = queueConsumerOptionsFactory;
        _topicOptionsFactory = topicOptionsFactory;
        _subscriptionOptionsFactory = subscriptionOptionsFactory;
    }

    public QueueOptions GetQueue(string logicalEntityName)
    {
        return _queues.GetOrAdd(logicalEntityName, _queueOptionsFactory.Create);
    }

    public QueueConsumerOptions GetQueueConsumer(string logicalEntityName)
    {
        return _queueConsumers.GetOrAdd(logicalEntityName, _queueConsumerOptionsFactory.Create);
    }

    public TopicOptions GetTopic(string logicalEntityName)
    {
        return _topics.GetOrAdd(logicalEntityName, _topicOptionsFactory.Create);
    }

    public SubscriptionOptions GetSubscription(string logicalEntityName)
    {
        return _subscriptions.GetOrAdd(logicalEntityName, _subscriptionOptionsFactory.Create);
    }
}
