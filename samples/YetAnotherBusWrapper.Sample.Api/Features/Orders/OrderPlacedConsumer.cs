using YetAnotherBusWrapper.Abstractions;
using YetAnotherBusWrapper.Sample.Api.Features.Received;

namespace YetAnotherBusWrapper.Sample.Api.Features.Orders;

public sealed partial class OrderPlacedConsumer : IMessageConsumer<OrderPlaced>
{
    private readonly ILogger<OrderPlacedConsumer> _logger;
    private readonly ReceivedMessageLog _receivedMessageLog;

    public OrderPlacedConsumer(ReceivedMessageLog receivedMessageLog, ILogger<OrderPlacedConsumer> logger)
    {
        _receivedMessageLog = receivedMessageLog;
        _logger = logger;
    }

    public Task ConsumeAsync(OrderPlaced message, MessageContext context, CancellationToken cancellationToken)
    {
        LogOrderPlaced(message.OrderId, message.Total);
        _receivedMessageLog.Record(nameof(OrderPlacedConsumer), message, context);

        return Task.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Order '{OrderId}' placed with total {Total}")]
    private partial void LogOrderPlaced(string orderId, decimal total);
}
