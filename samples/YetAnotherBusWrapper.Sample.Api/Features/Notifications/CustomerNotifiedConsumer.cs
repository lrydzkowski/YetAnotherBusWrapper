using Microsoft.Extensions.Logging;
using YetAnotherBusWrapper.Abstractions;
using YetAnotherBusWrapper.Sample.Api.Features.Received;

namespace YetAnotherBusWrapper.Sample.Api.Features.Notifications;

public sealed partial class CustomerNotifiedConsumer : IMessageConsumer<CustomerNotified>
{
    private readonly ILogger<CustomerNotifiedConsumer> _logger;
    private readonly ReceivedMessageLog _receivedMessageLog;

    public CustomerNotifiedConsumer(ReceivedMessageLog receivedMessageLog, ILogger<CustomerNotifiedConsumer> logger)
    {
        _receivedMessageLog = receivedMessageLog;
        _logger = logger;
    }

    public Task ConsumeAsync(CustomerNotified message, MessageContext context, CancellationToken cancellationToken)
    {
        LogEmailSent(message.CustomerId);
        _receivedMessageLog.Record(nameof(CustomerNotifiedConsumer), message, context);

        return Task.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Email notification sent to customer '{CustomerId}'")]
    private partial void LogEmailSent(string customerId);
}
