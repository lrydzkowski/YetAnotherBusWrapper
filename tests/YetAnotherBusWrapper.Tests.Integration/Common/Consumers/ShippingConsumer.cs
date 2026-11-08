using Microsoft.Extensions.DependencyInjection;
using YetAnotherBusWrapper.Abstractions;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Common.Consumers;

public sealed class ShippingConsumer : IMessageConsumer<OrderPlaced>
{
    public const string RecorderKey = "shipping";

    private readonly int _consumerInstance;
    private readonly DeliveryRecorder<OrderPlaced> _recorder;

    public ShippingConsumer([FromKeyedServices(RecorderKey)] DeliveryRecorder<OrderPlaced> recorder)
    {
        _recorder = recorder;
        _consumerInstance = recorder.RegisterConsumerInstance();
    }

    public Task ConsumeAsync(OrderPlaced message, MessageContext context, CancellationToken cancellationToken)
    {
        _recorder.Record(new RecordedDelivery<OrderPlaced>(_consumerInstance, message, context));

        return Task.CompletedTask;
    }
}
