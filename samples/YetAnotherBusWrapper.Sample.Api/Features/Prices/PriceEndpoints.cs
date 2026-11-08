using Microsoft.AspNetCore.Http.HttpResults;
using YetAnotherBusWrapper.Abstractions;

namespace YetAnotherBusWrapper.Sample.Api.Features.Prices;

internal static class PriceEndpoints
{
    public static void MapPriceEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/prices", ChangePriceAsync);
        endpoints.MapGet("/prices/{productId}", GetCachedPrice);
        endpoints.MapPost("/cache-invalidations", InvalidateCacheAsync);
    }

    private static async Task<Ok<SendResult>> ChangePriceAsync(
        ChangePriceRequest request,
        IMessageSender<PriceChanged> sender,
        CancellationToken cancellationToken
    )
    {
        SendResult result = await sender.SendAsync(
            new PriceChanged(request.ProductId, request.Price),
            cancellationToken: cancellationToken
        );

        return TypedResults.Ok(result);
    }

    private static Results<Ok<CachedPriceResponse>, NotFound> GetCachedPrice(string productId, PriceCache priceCache)
    {
        if (!priceCache.TryGet(productId, out decimal price))
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(new CachedPriceResponse(productId, price));
    }

    private static async Task<Ok<SendResult>> InvalidateCacheAsync(
        InvalidateCacheRequest request,
        IMessageSender<CacheInvalidated> sender,
        CancellationToken cancellationToken
    )
    {
        CacheInvalidated message = new(
            request.ApplicationName ?? CacheInvalidatedConsumer.ApplicationName,
            request.ProductId
        );
        SendResult result = await sender.SendAsync(message, cancellationToken: cancellationToken);

        return TypedResults.Ok(result);
    }
}

internal sealed record ChangePriceRequest(string ProductId, decimal Price);

internal sealed record InvalidateCacheRequest(string ProductId, string? ApplicationName);

internal sealed record CachedPriceResponse(string ProductId, decimal Price);
