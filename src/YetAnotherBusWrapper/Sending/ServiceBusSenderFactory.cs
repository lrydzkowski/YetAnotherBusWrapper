using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using YetAnotherBusWrapper.Clients;
using YetAnotherBusWrapper.Naming;
using YetAnotherBusWrapper.Options;

namespace YetAnotherBusWrapper.Sending;

internal sealed class ServiceBusSenderFactory
{
    private readonly ServiceBusClientProvider _clientProvider;
    private readonly EntityNameResolver _entityNameResolver;
    private readonly IOptionsMonitor<QueueOptions> _queueOptionsMonitor;
    private readonly IOptionsMonitor<TopicOptions> _topicOptionsMonitor;

    public ServiceBusSenderFactory(
        IOptionsMonitor<QueueOptions> queueOptionsMonitor,
        IOptionsMonitor<TopicOptions> topicOptionsMonitor,
        ServiceBusClientProvider clientProvider,
        EntityNameResolver entityNameResolver
    )
    {
        _queueOptionsMonitor = queueOptionsMonitor;
        _topicOptionsMonitor = topicOptionsMonitor;
        _clientProvider = clientProvider;
        _entityNameResolver = entityNameResolver;
    }

    public SenderEntityState GetQueueState(string logicalEntityName)
    {
        QueueOptions options = _queueOptionsMonitor.Get(logicalEntityName);

        return new SenderEntityState(options.Enabled, options.BestEffort);
    }

    public SenderEntityState GetTopicState(string logicalEntityName)
    {
        TopicOptions options = _topicOptionsMonitor.Get(logicalEntityName);

        return new SenderEntityState(options.Enabled, options.BestEffort);
    }

    public ServiceBusSender CreateQueueSender(string logicalEntityName)
    {
        QueueOptions options = _queueOptionsMonitor.Get(logicalEntityName);
        ServiceBusClient client = _clientProvider.GetClient(options.ConnectionString!, options.ClientRetry);

        return client.CreateSender(_entityNameResolver.ResolveQueueName(options));
    }

    public ServiceBusSender CreateTopicSender(string logicalEntityName)
    {
        TopicOptions options = _topicOptionsMonitor.Get(logicalEntityName);
        ServiceBusClient client = _clientProvider.GetClient(options.ConnectionString!, options.ClientRetry);

        return client.CreateSender(_entityNameResolver.ResolveTopicName(options));
    }
}
