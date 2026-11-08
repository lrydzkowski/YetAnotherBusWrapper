using Microsoft.Extensions.Logging;
using YetAnotherBusWrapper.Abstractions;
using YetAnotherBusWrapper.Sample.Api.Features.Received;

namespace YetAnotherBusWrapper.Sample.Api.Features.Payments;

public sealed partial class PaymentRequestedConsumer : IMessageConsumer<PaymentRequested>
{
    private readonly ILogger<PaymentRequestedConsumer> _logger;
    private readonly ReceivedMessageLog _receivedMessageLog;

    public PaymentRequestedConsumer(ReceivedMessageLog receivedMessageLog, ILogger<PaymentRequestedConsumer> logger)
    {
        _receivedMessageLog = receivedMessageLog;
        _logger = logger;
    }

    public Task ConsumeAsync(PaymentRequested message, MessageContext context, CancellationToken cancellationToken)
    {
        if (context.DeliveryCount <= message.FailuresToSimulate)
        {
            throw new InvalidOperationException(
                $"Simulated failure {context.DeliveryCount} of {message.FailuresToSimulate} "
                + $"for payment '{message.PaymentId}'."
            );
        }

        LogPaymentProcessed(message.PaymentId, context.DeliveryCount);
        _receivedMessageLog.Record(nameof(PaymentRequestedConsumer), message, context);

        return Task.CompletedTask;
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Payment '{PaymentId}' processed on delivery {DeliveryCount}"
    )]
    private partial void LogPaymentProcessed(string paymentId, int deliveryCount);
}
