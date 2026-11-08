using Microsoft.Extensions.Logging;
using YetAnotherBusWrapper.Abstractions;
using YetAnotherBusWrapper.Sample.Api.Features.Received;

namespace YetAnotherBusWrapper.Sample.Api.Features.Prices;

public sealed partial class PriceSearchIndexConsumer : IMessageConsumer<PriceChanged>
{
    private readonly ILogger<PriceSearchIndexConsumer> _logger;
    private readonly ReceivedMessageLog _receivedMessageLog;

    public PriceSearchIndexConsumer(ReceivedMessageLog receivedMessageLog, ILogger<PriceSearchIndexConsumer> logger)
    {
        _receivedMessageLog = receivedMessageLog;
        _logger = logger;
    }

    public Task ConsumeAsync(PriceChanged message, MessageContext context, CancellationToken cancellationToken)
    {
        LogPriceIndexed(message.ProductId, message.Price);
        _receivedMessageLog.Record(nameof(PriceSearchIndexConsumer), message, context);

        return Task.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Price {Price} indexed for product '{ProductId}'")]
    private partial void LogPriceIndexed(string productId, decimal price);
}
