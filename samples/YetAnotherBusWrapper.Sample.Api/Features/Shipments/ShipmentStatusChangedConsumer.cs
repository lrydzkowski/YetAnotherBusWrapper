using Microsoft.Extensions.Logging;
using YetAnotherBusWrapper.Abstractions;
using YetAnotherBusWrapper.Sample.Api.Features.Received;

namespace YetAnotherBusWrapper.Sample.Api.Features.Shipments;

public sealed partial class ShipmentStatusChangedConsumer : IMessageConsumer<ShipmentStatusChanged>
{
    private readonly ILogger<ShipmentStatusChangedConsumer> _logger;
    private readonly ReceivedMessageLog _receivedMessageLog;

    public ShipmentStatusChangedConsumer(
        ReceivedMessageLog receivedMessageLog,
        ILogger<ShipmentStatusChangedConsumer> logger
    )
    {
        _receivedMessageLog = receivedMessageLog;
        _logger = logger;
    }

    public Task ConsumeAsync(
        ShipmentStatusChanged message,
        MessageContext context,
        CancellationToken cancellationToken
    )
    {
        LogStatusChanged(message.ShipmentId, message.Sequence, message.Status, context.SessionId);
        _receivedMessageLog.Record(nameof(ShipmentStatusChangedConsumer), message, context);

        return Task.CompletedTask;
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Shipment '{ShipmentId}' status {Sequence} '{Status}' received in session '{SessionId}'"
    )]
    private partial void LogStatusChanged(string shipmentId, int sequence, string status, string? sessionId);
}
