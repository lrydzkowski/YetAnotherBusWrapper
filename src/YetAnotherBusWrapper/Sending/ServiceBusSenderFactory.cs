using Azure.Messaging.ServiceBus;
using YetAnotherBusWrapper.Clients;
using YetAnotherBusWrapper.Naming;
using YetAnotherBusWrapper.Options;

namespace YetAnotherBusWrapper.Sending;

internal sealed class ServiceBusSenderFactory
{
    private readonly ServiceBusClientProvider _clientProvider;
    private readonly EntityNameResolver _entityNameResolver;
    private readonly EntityOptionsStore _optionsStore;

    public ServiceBusSenderFactory(
        EntityOptionsStore optionsStore,
        ServiceBusClientProvider clientProvider,
        EntityNameResolver entityNameResolver
    )
    {
        _optionsStore = optionsStore;
        _clientProvider = clientProvider;
        _entityNameResolver = entityNameResolver;
    }

    public SenderEntityState GetQueueState(string logicalEntityName)
    {
        QueueOptions options = _optionsStore.GetQueue(logicalEntityName);

        return new SenderEntityState(options.Enabled, options.BestEffort);
    }

    public SenderEntityState GetTopicState(string logicalEntityName)
    {
        TopicOptions options = _optionsStore.GetTopic(logicalEntityName);

        return new SenderEntityState(options.Enabled, options.BestEffort);
    }

    public ServiceBusSender CreateQueueSender(string logicalEntityName)
    {
        QueueOptions options = _optionsStore.GetQueue(logicalEntityName);
        ServiceBusClient client = _clientProvider.GetClient(options.ConnectionString!, options.ClientRetry);

        return client.CreateSender(_entityNameResolver.ResolveQueueName(options));
    }

    public ServiceBusSender CreateTopicSender(string logicalEntityName)
    {
        TopicOptions options = _optionsStore.GetTopic(logicalEntityName);
        ServiceBusClient client = _clientProvider.GetClient(options.ConnectionString!, options.ClientRetry);

        return client.CreateSender(_entityNameResolver.ResolveTopicName(options));
    }
}
