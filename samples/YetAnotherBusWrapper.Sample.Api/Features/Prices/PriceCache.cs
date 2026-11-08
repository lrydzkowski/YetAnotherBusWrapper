using System.Collections.Concurrent;

namespace YetAnotherBusWrapper.Sample.Api.Features.Prices;

public sealed class PriceCache
{
    private readonly ConcurrentDictionary<string, decimal> _prices = new();

    public void Set(string productId, decimal price)
    {
        _prices[productId] = price;
    }

    public bool TryGet(string productId, out decimal price)
    {
        return _prices.TryGetValue(productId, out price);
    }

    public void Remove(string productId)
    {
        _prices.TryRemove(productId, out _);
    }
}
