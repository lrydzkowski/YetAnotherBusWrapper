using Azure.Messaging.ServiceBus;
using YetAnotherBusWrapper.Clients;
using YetAnotherBusWrapper.Naming;
using YetAnotherBusWrapper.Options;

namespace YetAnotherBusWrapper.Consuming;

internal sealed class ServiceBusProcessorFactory
{
    private readonly ServiceBusClientProvider _clientProvider;
    private readonly EntityNameResolver _entityNameResolver;
    private readonly EntityOptionsStore _optionsStore;

    public ServiceBusProcessorFactory(
        EntityOptionsStore optionsStore,
        ServiceBusClientProvider clientProvider,
        EntityNameResolver entityNameResolver
    )
    {
        _optionsStore = optionsStore;
        _clientProvider = clientProvider;
        _entityNameResolver = entityNameResolver;
    }

    public bool IsQueueEnabled(string logicalEntityName)
    {
        return _optionsStore.GetQueue(logicalEntityName).Enabled;
    }

    public bool IsSubscriptionEnabled(string topicLogicalEntityName, string subscriptionLogicalEntityName)
    {
        return _optionsStore.GetTopic(topicLogicalEntityName).Enabled
            && _optionsStore.GetSubscription(subscriptionLogicalEntityName).Enabled;
    }

    public IProcessorAdapter CreateQueueProcessor(string logicalEntityName, ProcessorHandlers handlers)
    {
        QueueOptions options = _optionsStore.GetQueue(logicalEntityName);
        QueueConsumerOptions consumerOptions = _optionsStore.GetQueueConsumer(logicalEntityName);
        ServiceBusClient client = _clientProvider.GetClient(options.ConnectionString!, options.ClientRetry);
        string queueName = _entityNameResolver.ResolveQueueName(options);

        if (consumerOptions.SessionsEnabled)
        {
            return new ServiceBusSessionProcessorAdapter(
                client.CreateSessionProcessor(queueName, CreateSessionProcessorOptions(consumerOptions)),
                handlers
            );
        }

        return new ServiceBusProcessorAdapter(
            client.CreateProcessor(queueName, CreateProcessorOptions(consumerOptions)),
            handlers
        );
    }

    public IProcessorAdapter CreateSubscriptionProcessor(
        string topicLogicalEntityName,
        string subscriptionLogicalEntityName,
        ProcessorHandlers handlers
    )
    {
        TopicOptions topicOptions = _optionsStore.GetTopic(topicLogicalEntityName);
        SubscriptionOptions subscriptionOptions = _optionsStore.GetSubscription(subscriptionLogicalEntityName);
        ServiceBusClient client = _clientProvider.GetClient(topicOptions.ConnectionString!, topicOptions.ClientRetry);
        string topicName = _entityNameResolver.ResolveTopicName(topicOptions);
        string subscriptionName = _entityNameResolver.ResolveSubscriptionName(subscriptionOptions);

        if (subscriptionOptions.SessionsEnabled)
        {
            return new ServiceBusSessionProcessorAdapter(
                client.CreateSessionProcessor(
                    topicName,
                    subscriptionName,
                    CreateSessionProcessorOptions(subscriptionOptions)
                ),
                handlers
            );
        }

        return new ServiceBusProcessorAdapter(
            client.CreateProcessor(topicName, subscriptionName, CreateProcessorOptions(subscriptionOptions)),
            handlers
        );
    }

    private static ServiceBusProcessorOptions CreateProcessorOptions(IConsumerProcessingOptions options)
    {
        return new ServiceBusProcessorOptions
        {
            AutoCompleteMessages = true,
            MaxConcurrentCalls = options.MaxConcurrentCalls,
            PrefetchCount = options.PrefetchCount,
            MaxAutoLockRenewalDuration = options.MaxAutoLockRenewalDuration
        };
    }

    private static ServiceBusSessionProcessorOptions CreateSessionProcessorOptions(IConsumerProcessingOptions options)
    {
        return new ServiceBusSessionProcessorOptions
        {
            AutoCompleteMessages = true,
            PrefetchCount = options.PrefetchCount,
            MaxAutoLockRenewalDuration = options.MaxAutoLockRenewalDuration,
            MaxConcurrentSessions = options.MaxConcurrentSessions,
            SessionIdleTimeout = options.SessionIdleTimeout
        };
    }
}
