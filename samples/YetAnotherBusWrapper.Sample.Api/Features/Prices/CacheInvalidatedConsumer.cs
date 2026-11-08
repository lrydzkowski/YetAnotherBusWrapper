using Microsoft.Extensions.Logging;
using YetAnotherBusWrapper.Abstractions;
using YetAnotherBusWrapper.Sample.Api.Features.Received;

namespace YetAnotherBusWrapper.Sample.Api.Features.Prices;

public sealed partial class CacheInvalidatedConsumer : IMessageConsumer<CacheInvalidated>
{
    public const string ApplicationName = "yetanotherbuswrapper-sample";

    private readonly ILogger<CacheInvalidatedConsumer> _logger;
    private readonly PriceCache _priceCache;
    private readonly ReceivedMessageLog _receivedMessageLog;

    public CacheInvalidatedConsumer(
        PriceCache priceCache,
        ReceivedMessageLog receivedMessageLog,
        ILogger<CacheInvalidatedConsumer> logger
    )
    {
        _priceCache = priceCache;
        _receivedMessageLog = receivedMessageLog;
        _logger = logger;
    }

    public Task ConsumeAsync(CacheInvalidated message, MessageContext context, CancellationToken cancellationToken)
    {
        if (message.ApplicationName != ApplicationName)
        {
            LogInvalidationIgnored(message.ProductId, message.ApplicationName);

            return Task.CompletedTask;
        }

        _priceCache.Remove(message.ProductId);
        LogPriceInvalidated(message.ProductId);
        _receivedMessageLog.Record(nameof(CacheInvalidatedConsumer), message, context);

        return Task.CompletedTask;
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Cache invalidation of product '{ProductId}' for application '{ApplicationName}' ignored"
    )]
    private partial void LogInvalidationIgnored(string productId, string applicationName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Cached price of product '{ProductId}' removed")]
    private partial void LogPriceInvalidated(string productId);
}
