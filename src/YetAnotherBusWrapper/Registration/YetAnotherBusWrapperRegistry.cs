namespace YetAnotherBusWrapper.Registration;

internal sealed class YetAnotherBusWrapperRegistry
{
    private readonly Dictionary<string, Type> _queueConsumers = new(StringComparer.Ordinal);
    private readonly Dictionary<Type, QueueRegistration> _queues = [];
    private readonly Dictionary<string, SubscriptionRegistration> _subscriptions = new(StringComparer.Ordinal);
    private readonly Dictionary<Type, TopicRegistration> _topics = [];

    public IReadOnlyCollection<QueueRegistration> Queues => _queues.Values;

    public IReadOnlyCollection<TopicRegistration> Topics => _topics.Values;

    public IReadOnlyCollection<SubscriptionRegistration> Subscriptions => _subscriptions.Values;

    public bool HostedServicesDisabled { get; private set; }

    public void DisableHostedServices()
    {
        HostedServicesDisabled = true;
    }

    public void AddQueue(QueueRegistration registration)
    {
        EnsureMessageTypeIsNotRegistered(registration.MessageType, $"queue '{registration.LogicalEntityName}'");
        _queues.Add(registration.MessageType, registration);
    }

    public void AddTopic(TopicRegistration registration)
    {
        EnsureMessageTypeIsNotRegistered(registration.MessageType, $"topic '{registration.LogicalEntityName}'");
        _topics.Add(registration.MessageType, registration);
    }

    public void AddQueueConsumer(string queueLogicalEntityName, Type consumerType)
    {
        if (!_queueConsumers.TryAdd(queueLogicalEntityName, consumerType))
        {
            throw new InvalidOperationException(
                $"Consumer '{consumerType.FullName}' can't be registered for queue '{queueLogicalEntityName}' because "
                + $"consumer '{_queueConsumers[queueLogicalEntityName].FullName}' is already registered for it."
            );
        }
    }

    public void AddSubscription(SubscriptionRegistration registration)
    {
        if (!_subscriptions.TryAdd(registration.LogicalEntityName, registration))
        {
            throw new InvalidOperationException(
                $"Subscription '{registration.SubscriptionLogicalName}' is already registered for topic "
                + $"'{registration.TopicLogicalEntityName}'."
            );
        }
    }

    public SubscriptionRegistration? FindSubscription(string logicalEntityName)
    {
        return _subscriptions.GetValueOrDefault(logicalEntityName);
    }

    public string GetQueueLogicalEntityName(Type messageType)
    {
        if (_queues.TryGetValue(messageType, out QueueRegistration? registration))
        {
            return registration.LogicalEntityName;
        }

        if (_topics.TryGetValue(messageType, out TopicRegistration? topicRegistration))
        {
            throw new InvalidOperationException(
                $"Message type '{messageType.FullName}' is registered for topic '{topicRegistration.LogicalEntityName}', "
                + "not a queue. Register a topic consumer instead."
            );
        }

        throw new InvalidOperationException(
            $"Message type '{messageType.FullName}' has no registered queue. Register the queue before its consumer."
        );
    }

    public string GetTopicLogicalEntityName(Type messageType)
    {
        if (_topics.TryGetValue(messageType, out TopicRegistration? registration))
        {
            return registration.LogicalEntityName;
        }

        if (_queues.TryGetValue(messageType, out QueueRegistration? queueRegistration))
        {
            throw new InvalidOperationException(
                $"Message type '{messageType.FullName}' is registered for queue '{queueRegistration.LogicalEntityName}', "
                + "not a topic. Register a queue consumer instead."
            );
        }

        throw new InvalidOperationException(
            $"Message type '{messageType.FullName}' has no registered topic. Register the topic before its consumer."
        );
    }

    private void EnsureMessageTypeIsNotRegistered(Type messageType, string newEntity)
    {
        if (_queues.TryGetValue(messageType, out QueueRegistration? queueRegistration))
        {
            throw new InvalidOperationException(
                $"Message type '{messageType.FullName}' can't be registered for {newEntity} because it is already "
                + $"registered for queue '{queueRegistration.LogicalEntityName}'."
            );
        }

        if (_topics.TryGetValue(messageType, out TopicRegistration? topicRegistration))
        {
            throw new InvalidOperationException(
                $"Message type '{messageType.FullName}' can't be registered for {newEntity} because it is already "
                + $"registered for topic '{topicRegistration.LogicalEntityName}'."
            );
        }
    }
}
