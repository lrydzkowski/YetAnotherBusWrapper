using Microsoft.Extensions.Logging;
using YetAnotherBusWrapper.Abstractions;
using YetAnotherBusWrapper.Sample.Api.Features.Received;

namespace YetAnotherBusWrapper.Sample.Api.Features.Prices;

public sealed partial class PriceCacheConsumer : IMessageConsumer<PriceChanged>
{
    private readonly ILogger<PriceCacheConsumer> _logger;
    private readonly PriceCache _priceCache;
    private readonly ReceivedMessageLog _receivedMessageLog;

    public PriceCacheConsumer(
        PriceCache priceCache,
        ReceivedMessageLog receivedMessageLog,
        ILogger<PriceCacheConsumer> logger
    )
    {
        _priceCache = priceCache;
        _receivedMessageLog = receivedMessageLog;
        _logger = logger;
    }

    public Task ConsumeAsync(PriceChanged message, MessageContext context, CancellationToken cancellationToken)
    {
        _priceCache.Set(message.ProductId, message.Price);
        LogPriceCached(message.ProductId, message.Price);
        _receivedMessageLog.Record(nameof(PriceCacheConsumer), message, context);

        return Task.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Price {Price} cached for product '{ProductId}'")]
    private partial void LogPriceCached(string productId, decimal price);
}
