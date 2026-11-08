using YetAnotherBusWrapper.Abstractions;

namespace YetAnotherBusWrapper.Tests.Integration.Common.Consumers;

public sealed class RecordingConsumer<TMessage> : IMessageConsumer<TMessage>
{
    private readonly int _consumerInstance;
    private readonly DeliveryRecorder<TMessage> _recorder;

    public RecordingConsumer(DeliveryRecorder<TMessage> recorder)
    {
        _recorder = recorder;
        _consumerInstance = recorder.RegisterConsumerInstance();
    }

    public Task ConsumeAsync(TMessage message, MessageContext context, CancellationToken cancellationToken)
    {
        _recorder.Record(new RecordedDelivery<TMessage>(_consumerInstance, message, context));

        return Task.CompletedTask;
    }
}
