using Microsoft.Extensions.Logging;
using YetAnotherBusWrapper.Abstractions;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Common.Consumers;

public sealed partial class FailingConsumer : IMessageConsumer<OrderPlaced>
{
    private readonly int _consumerInstance;
    private readonly ConsumerFailurePlan _failurePlan;
    private readonly ILogger<FailingConsumer> _logger;
    private readonly DeliveryRecorder<OrderPlaced> _recorder;

    public FailingConsumer(
        DeliveryRecorder<OrderPlaced> recorder,
        ConsumerFailurePlan failurePlan,
        ILogger<FailingConsumer> logger
    )
    {
        _recorder = recorder;
        _failurePlan = failurePlan;
        _logger = logger;
        _consumerInstance = recorder.RegisterConsumerInstance();
    }

    public Task ConsumeAsync(OrderPlaced message, MessageContext context, CancellationToken cancellationToken)
    {
        _recorder.Record(new RecordedDelivery<OrderPlaced>(_consumerInstance, message, context));
        LogConsuming(message.OrderId, context.DeliveryCount);
        if (_failurePlan.ShouldFail(context.DeliveryCount))
        {
            throw new InvalidOperationException(
                $"Consumer failed for order '{message.OrderId}' on delivery {context.DeliveryCount}."
            );
        }

        return Task.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Consuming order '{OrderId}' on delivery {DeliveryCount}")]
    private partial void LogConsuming(string orderId, int deliveryCount);
}
