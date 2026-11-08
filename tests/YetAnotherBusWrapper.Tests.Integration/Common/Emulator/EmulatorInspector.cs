using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

namespace YetAnotherBusWrapper.Tests.Integration.Common.Emulator;

public sealed class EmulatorInspector
{
    private const int MaxPeekedMessages = 100;

    private readonly ServiceBusAdministrationClient _administrationClient;
    private readonly ServiceBusClient _client;

    public EmulatorInspector(ServiceBusAdministrationClient administrationClient, ServiceBusClient client)
    {
        _administrationClient = administrationClient;
        _client = client;
    }

    public async Task CreateQueueAsync(string queueName, CancellationToken cancellationToken)
    {
        await _administrationClient.CreateQueueAsync(queueName, cancellationToken);
    }

    public async Task CreateQueueAsync(CreateQueueOptions options, CancellationToken cancellationToken)
    {
        await _administrationClient.CreateQueueAsync(options, cancellationToken);
    }

    public async Task<QueuePropertiesDto?> GetQueueAsync(string queueName, CancellationToken cancellationToken)
    {
        bool exists = await _administrationClient.QueueExistsAsync(queueName, cancellationToken);
        if (!exists)
        {
            return null;
        }

        QueueProperties properties = await _administrationClient.GetQueueAsync(queueName, cancellationToken);

        return QueuePropertiesDto.From(properties);
    }

    public async Task<List<string>> GetQueueNamesAsync(string prefix, CancellationToken cancellationToken)
    {
        List<string> queueNames = [];
        await foreach (QueueProperties queue in _administrationClient.GetQueuesAsync(cancellationToken))
        {
            if (queue.Name.StartsWith(prefix, StringComparison.Ordinal))
            {
                queueNames.Add(queue.Name);
            }
        }

        return queueNames.Order(StringComparer.Ordinal).ToList();
    }

    public async Task DeleteQueueIfExistsAsync(string queueName, CancellationToken cancellationToken)
    {
        bool exists = await _administrationClient.QueueExistsAsync(queueName, cancellationToken);
        if (!exists)
        {
            return;
        }

        await _administrationClient.DeleteQueueAsync(queueName, cancellationToken);
    }

    public async Task<List<PeekedMessageDto>> PeekQueueAsync(string queueName, CancellationToken cancellationToken)
    {
        await using ServiceBusReceiver receiver = _client.CreateReceiver(queueName);

        return await PeekAsync(receiver, cancellationToken);
    }

    public async Task<IReadOnlyList<ServiceBusReceivedMessage>> PeekQueueMessagesAsync(
        string queueName,
        CancellationToken cancellationToken
    )
    {
        await using ServiceBusReceiver receiver = _client.CreateReceiver(queueName);

        return await receiver.PeekMessagesAsync(MaxPeekedMessages, cancellationToken: cancellationToken);
    }

    public async Task<List<DeadLetteredMessageDto>> PeekQueueDeadLetterAsync(
        string queueName,
        CancellationToken cancellationToken
    )
    {
        await using ServiceBusReceiver receiver = _client.CreateReceiver(
            queueName,
            new ServiceBusReceiverOptions { SubQueue = SubQueue.DeadLetter }
        );
        IReadOnlyList<ServiceBusReceivedMessage> messages = await receiver.PeekMessagesAsync(
            MaxPeekedMessages,
            cancellationToken: cancellationToken
        );

        return messages.Select(DeadLetteredMessageDto.From).ToList();
    }

    public async Task SendToQueueAsync(string queueName, ServiceBusMessage message, CancellationToken cancellationToken)
    {
        await using ServiceBusSender sender = _client.CreateSender(queueName);
        await sender.SendMessageAsync(message, cancellationToken);
    }

    public async Task CreateTopicAsync(string topicName, CancellationToken cancellationToken)
    {
        await _administrationClient.CreateTopicAsync(topicName, cancellationToken);
    }

    public async Task CreateTopicAsync(CreateTopicOptions options, CancellationToken cancellationToken)
    {
        await _administrationClient.CreateTopicAsync(options, cancellationToken);
    }

    public async Task CreateSubscriptionAsync(
        string topicName,
        string subscriptionName,
        CancellationToken cancellationToken
    )
    {
        await _administrationClient.CreateSubscriptionAsync(topicName, subscriptionName, cancellationToken);
    }

    public async Task CreateSubscriptionAsync(CreateSubscriptionOptions options, CancellationToken cancellationToken)
    {
        await _administrationClient.CreateSubscriptionAsync(options, cancellationToken);
    }

    public async Task<TopicPropertiesDto?> GetTopicAsync(string topicName, CancellationToken cancellationToken)
    {
        bool exists = await _administrationClient.TopicExistsAsync(topicName, cancellationToken);
        if (!exists)
        {
            return null;
        }

        TopicProperties properties = await _administrationClient.GetTopicAsync(topicName, cancellationToken);

        return TopicPropertiesDto.From(properties);
    }

    public async Task<List<SubscriptionPropertiesDto>> GetSubscriptionsAsync(
        string topicName,
        CancellationToken cancellationToken
    )
    {
        bool exists = await _administrationClient.TopicExistsAsync(topicName, cancellationToken);
        if (!exists)
        {
            return [];
        }

        List<SubscriptionPropertiesDto> subscriptions = [];
        await foreach (SubscriptionProperties subscription in _administrationClient.GetSubscriptionsAsync(
                           topicName,
                           cancellationToken
                       ))
        {
            subscriptions.Add(SubscriptionPropertiesDto.From(subscription));
        }

        return subscriptions.OrderBy(subscription => subscription.SubscriptionName, StringComparer.Ordinal).ToList();
    }

    public async Task<List<string>> GetTopicNamesAsync(string prefix, CancellationToken cancellationToken)
    {
        List<string> topicNames = [];
        await foreach (TopicProperties topic in _administrationClient.GetTopicsAsync(cancellationToken))
        {
            if (topic.Name.StartsWith(prefix, StringComparison.Ordinal))
            {
                topicNames.Add(topic.Name);
            }
        }

        return topicNames.Order(StringComparer.Ordinal).ToList();
    }

    public async Task DeleteTopicIfExistsAsync(string topicName, CancellationToken cancellationToken)
    {
        bool exists = await _administrationClient.TopicExistsAsync(topicName, cancellationToken);
        if (!exists)
        {
            return;
        }

        await _administrationClient.DeleteTopicAsync(topicName, cancellationToken);
    }

    public async Task<List<PeekedMessageDto>> PeekSubscriptionAsync(
        string topicName,
        string subscriptionName,
        CancellationToken cancellationToken
    )
    {
        await using ServiceBusReceiver receiver = _client.CreateReceiver(topicName, subscriptionName);

        return await PeekAsync(receiver, cancellationToken);
    }

    private static async Task<List<PeekedMessageDto>> PeekAsync(
        ServiceBusReceiver receiver,
        CancellationToken cancellationToken
    )
    {
        IReadOnlyList<ServiceBusReceivedMessage> messages = await receiver.PeekMessagesAsync(
            MaxPeekedMessages,
            cancellationToken: cancellationToken
        );

        return messages.Select(PeekedMessageDto.From).ToList();
    }
}
