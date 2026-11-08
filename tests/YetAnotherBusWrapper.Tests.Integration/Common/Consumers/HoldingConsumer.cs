using YetAnotherBusWrapper.Abstractions;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Common.Consumers;

public sealed class HoldingConsumer : IMessageConsumer<OrderPlaced>
{
    private readonly int _consumerInstance;
    private readonly HoldPlan _holdPlan;
    private readonly DeliveryRecorder<OrderPlaced> _recorder;
    private readonly ConcurrencyTracker _tracker;

    public HoldingConsumer(DeliveryRecorder<OrderPlaced> recorder, ConcurrencyTracker tracker, HoldPlan holdPlan)
    {
        _recorder = recorder;
        _tracker = tracker;
        _holdPlan = holdPlan;
        _consumerInstance = recorder.RegisterConsumerInstance();
    }

    public async Task ConsumeAsync(OrderPlaced message, MessageContext context, CancellationToken cancellationToken)
    {
        await _tracker.HoldAsync(_holdPlan, cancellationToken);
        _recorder.Record(new RecordedDelivery<OrderPlaced>(_consumerInstance, message, context));
    }
}
