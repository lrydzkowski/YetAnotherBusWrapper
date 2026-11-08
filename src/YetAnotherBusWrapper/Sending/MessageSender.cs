using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using YetAnotherBusWrapper.Abstractions;
using YetAnotherBusWrapper.Naming;
using YetAnotherBusWrapper.Serialization;

namespace YetAnotherBusWrapper.Sending;

internal sealed partial class MessageSender<TMessage> : IMessageSender<TMessage>, IDisposable, IAsyncDisposable
{
    private readonly Action<ServiceBusMessage, TMessage>? _configureMessage;
    private readonly EntityKind _entityKind;
    private readonly Func<SenderEntityState> _getEntityState;
    private readonly ILogger<MessageSender<TMessage>> _logger;
    private readonly string _logicalEntityName;
    private readonly Lazy<ServiceBusSender> _sender;
    private readonly IMessageSerializer _serializer;

    public MessageSender(
        EntityKind entityKind,
        string logicalEntityName,
        Func<SenderEntityState> getEntityState,
        Func<ServiceBusSender> createSender,
        IMessageSerializer serializer,
        Action<ServiceBusMessage, TMessage>? configureMessage,
        ILogger<MessageSender<TMessage>> logger
    )
    {
        _entityKind = entityKind;
        _logicalEntityName = logicalEntityName;
        _getEntityState = getEntityState;
        _serializer = serializer;
        _configureMessage = configureMessage;
        _logger = logger;
        _sender = new Lazy<ServiceBusSender>(createSender);
    }

    public async ValueTask DisposeAsync()
    {
        if (_sender.IsValueCreated)
        {
            await _sender.Value.DisposeAsync();
        }
    }

    public void Dispose()
    {
        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    public Task<SendResult> SendAsync(
        TMessage message,
        SendOptions? options = null,
        CancellationToken cancellationToken = default
    )
    {
        return SendAsync([message], options, cancellationToken);
    }

    public async Task<SendResult> SendAsync(
        IReadOnlyCollection<TMessage> messages,
        SendOptions? options = null,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(messages);
        SenderEntityState entityState = _getEntityState();
        if (!entityState.Enabled)
        {
            return HandleDisabledEntity(entityState, messages.Count);
        }

        List<string> sentMessageIds = [];
        try
        {
            await SendMessagesAsync(messages, options, sentMessageIds, cancellationToken);
        }
        catch (Exception exception) when (entityState.BestEffort && exception is not OperationCanceledException)
        {
            LogSendFailed(exception, _entityKind.ToLogName(), _logicalEntityName, sentMessageIds.Count, messages.Count);

            return new SendResult(SendStatus.Failed, sentMessageIds);
        }

        return new SendResult(SendStatus.Sent, sentMessageIds);
    }

    private SendResult HandleDisabledEntity(SenderEntityState entityState, int messageCount)
    {
        if (!entityState.BestEffort)
        {
            throw new InvalidOperationException(
                $"Can't send to {_entityKind.ToLogName()} '{_logicalEntityName}' because it is disabled."
            );
        }

        LogSendSkipped(messageCount, _entityKind.ToLogName(), _logicalEntityName);

        return new SendResult(SendStatus.Skipped, []);
    }

    private async Task SendMessagesAsync(
        IReadOnlyCollection<TMessage> messages,
        SendOptions? options,
        List<string> sentMessageIds,
        CancellationToken cancellationToken
    )
    {
        if (messages.Count == 0)
        {
            return;
        }

        List<ServiceBusMessage> serviceBusMessages = messages
            .Select(message => CreateServiceBusMessage(message, options))
            .ToList();
        await SendInBatchesAsync(serviceBusMessages, sentMessageIds, cancellationToken);
    }

    private ServiceBusMessage CreateServiceBusMessage(TMessage message, SendOptions? options)
    {
        ServiceBusMessage serviceBusMessage = _serializer.Serialize(message);
        _configureMessage?.Invoke(serviceBusMessage, message);
        if (string.IsNullOrEmpty(serviceBusMessage.MessageId))
        {
            serviceBusMessage.MessageId = Guid.NewGuid().ToString();
        }

        if (options?.ScheduledEnqueueTime is { } scheduledEnqueueTime)
        {
            serviceBusMessage.ScheduledEnqueueTime = scheduledEnqueueTime;
        }

        return serviceBusMessage;
    }

    private async Task SendInBatchesAsync(
        IReadOnlyList<ServiceBusMessage> serviceBusMessages,
        List<string> sentMessageIds,
        CancellationToken cancellationToken
    )
    {
        ServiceBusMessageBatch batch = await _sender.Value.CreateMessageBatchAsync(cancellationToken);
        try
        {
            for (int index = 0; index < serviceBusMessages.Count; index++)
            {
                if (batch.TryAddMessage(serviceBusMessages[index]))
                {
                    continue;
                }

                if (batch.Count == 0)
                {
                    throw CreateMessageTooLargeException(batch, serviceBusMessages[index], index);
                }

                await SendBatchAsync(batch, serviceBusMessages, sentMessageIds, cancellationToken);
                batch.Dispose();
                batch = await _sender.Value.CreateMessageBatchAsync(cancellationToken);
                if (!batch.TryAddMessage(serviceBusMessages[index]))
                {
                    throw CreateMessageTooLargeException(batch, serviceBusMessages[index], index);
                }
            }

            await SendBatchAsync(batch, serviceBusMessages, sentMessageIds, cancellationToken);
        }
        finally
        {
            batch.Dispose();
        }
    }

    private InvalidOperationException CreateMessageTooLargeException(
        ServiceBusMessageBatch batch,
        ServiceBusMessage serviceBusMessage,
        int index
    )
    {
        return new InvalidOperationException(
            $"Message '{serviceBusMessage.MessageId}' at index {index} sent to '{_logicalEntityName}' doesn't fit "
            + $"into an empty batch of {batch.MaxSizeInBytes} bytes. Messages before it may already be sent."
        );
    }

    private async Task SendBatchAsync(
        ServiceBusMessageBatch batch,
        IReadOnlyList<ServiceBusMessage> serviceBusMessages,
        List<string> sentMessageIds,
        CancellationToken cancellationToken
    )
    {
        int firstMessageIndex = sentMessageIds.Count;
        await _sender.Value.SendMessagesAsync(batch, cancellationToken);
        sentMessageIds.AddRange(
            serviceBusMessages.Skip(firstMessageIndex).Take(batch.Count).Select(message => message.MessageId)
        );
        LogBatchSent(batch.Count, _logicalEntityName);
    }

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Sent a batch of {MessageCount} messages to '{LogicalEntityName}'"
    )]
    private partial void LogBatchSent(int messageCount, string logicalEntityName);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Skipped sending {MessageCount} messages to {EntityKind} '{LogicalEntityName}' because it is disabled"
    )]
    private partial void LogSendSkipped(int messageCount, string entityKind, string logicalEntityName);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message =
            "Failed to send messages to {EntityKind} '{LogicalEntityName}'; {SentMessageCount} of {MessageCount} messages were sent"
    )]
    private partial void LogSendFailed(
        Exception exception,
        string entityKind,
        string logicalEntityName,
        int sentMessageCount,
        int messageCount
    );
}
