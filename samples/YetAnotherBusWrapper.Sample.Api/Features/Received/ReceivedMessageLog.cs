using System.Collections.Concurrent;
using YetAnotherBusWrapper.Abstractions;

namespace YetAnotherBusWrapper.Sample.Api.Features.Received;

public sealed class ReceivedMessageLog
{
    private const int Capacity = 200;

    private readonly ConcurrentQueue<ReceivedMessage> _messages = new();
    private readonly TimeProvider _timeProvider;

    public ReceivedMessageLog(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public void Record(string consumer, object message, MessageContext context)
    {
        _messages.Enqueue(
            new ReceivedMessage(
                consumer,
                message,
                context.MessageId,
                context.CorrelationId,
                context.DeliveryCount,
                context.SessionId,
                context.EnqueuedTime,
                _timeProvider.GetUtcNow(),
                context.ApplicationProperties
            )
        );
        while (_messages.Count > Capacity)
        {
            _messages.TryDequeue(out _);
        }
    }

    public IReadOnlyList<ReceivedMessage> GetAll()
    {
        return [.. _messages];
    }
}
