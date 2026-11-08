using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using YetAnotherBusWrapper.Abstractions;
using YetAnotherBusWrapper.Registration;
using YetAnotherBusWrapper.Serialization;

namespace YetAnotherBusWrapper.Consuming;

internal sealed partial class ReceivedMessageDispatcher<TMessage> : IReceivedMessageDispatcher
{
    private readonly Type _consumerType;
    private readonly ILogger<ReceivedMessageDispatcher<TMessage>> _logger;
    private readonly string _logicalEntityName;
    private readonly IMessageSerializer _serializer;

    public ReceivedMessageDispatcher(
        ConsumerRegistration consumer,
        IMessageSerializer serializer,
        ILogger<ReceivedMessageDispatcher<TMessage>> logger
    )
    {
        _logicalEntityName = consumer.LogicalEntityName;
        _consumerType = consumer.ConsumerType;
        _serializer = serializer;
        _logger = logger;
    }

    public async Task DispatchAsync(
        IServiceProvider serviceProvider,
        ServiceBusReceivedMessage message,
        CancellationToken cancellationToken
    )
    {
        string correlationId = GetCorrelationId(message);
        using IDisposable? loggingScope = BeginMessageLoggingScope(message, correlationId);
        TMessage deserializedMessage = Deserialize(message);
        MessageContext context = CreateContext(message, correlationId);
        await ConsumeAsync(serviceProvider, deserializedMessage, context, cancellationToken);
    }

    private static string GetCorrelationId(ServiceBusReceivedMessage message)
    {
        return string.IsNullOrEmpty(message.CorrelationId) ? Guid.NewGuid().ToString() : message.CorrelationId;
    }

    private IDisposable? BeginMessageLoggingScope(ServiceBusReceivedMessage message, string correlationId)
    {
        return _logger.BeginScope(
            "CorrelationId: {CorrelationId}, MessageId: {MessageId}, LogicalEntityName: {LogicalEntityName}, DeliveryCount: {DeliveryCount}",
            correlationId,
            message.MessageId,
            _logicalEntityName,
            message.DeliveryCount
        );
    }

    private TMessage Deserialize(ServiceBusReceivedMessage message)
    {
        try
        {
            return _serializer.Deserialize<TMessage>(message);
        }
        catch (Exception exception)
        {
            LogDeserializationFailed(exception, message.MessageId, _logicalEntityName, typeof(TMessage).FullName);
            throw;
        }
    }

    private static MessageContext CreateContext(ServiceBusReceivedMessage message, string correlationId)
    {
        return new MessageContext
        {
            MessageId = message.MessageId,
            CorrelationId = correlationId,
            DeliveryCount = message.DeliveryCount,
            SessionId = message.SessionId,
            EnqueuedTime = message.EnqueuedTime,
            ApplicationProperties = message.ApplicationProperties
        };
    }

    private async Task ConsumeAsync(
        IServiceProvider serviceProvider,
        TMessage message,
        MessageContext context,
        CancellationToken cancellationToken
    )
    {
        try
        {
            IMessageConsumer<TMessage> consumer =
                (IMessageConsumer<TMessage>)serviceProvider.GetRequiredService(_consumerType);
            await consumer.ConsumeAsync(message, context, cancellationToken);
        }
        catch (Exception exception)
        {
            LogConsumerFailed(exception, _consumerType.FullName, context.MessageId, _logicalEntityName);
            throw;
        }
    }

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Failed to deserialize message '{MessageId}' from '{LogicalEntityName}' as '{MessageType}'"
    )]
    private partial void LogDeserializationFailed(
        Exception exception,
        string messageId,
        string logicalEntityName,
        string? messageType
    );

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Consumer '{ConsumerType}' failed to handle message '{MessageId}' from '{LogicalEntityName}'"
    )]
    private partial void LogConsumerFailed(
        Exception exception,
        string? consumerType,
        string messageId,
        string logicalEntityName
    );
}
