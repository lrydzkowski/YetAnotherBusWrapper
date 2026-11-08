namespace YetAnotherBusWrapper.Sample.Api.Features.Orders;

public sealed class OrderService
{
    private readonly IOrderNotifier _orderNotifier;

    public OrderService(IOrderNotifier orderNotifier)
    {
        _orderNotifier = orderNotifier;
    }

    public async Task<string> PlaceAsync(decimal total, CancellationToken cancellationToken)
    {
        OrderPlaced order = new(Guid.NewGuid().ToString(), total);
        await _orderNotifier.NotifyOrderPlacedAsync(order, cancellationToken);

        return order.OrderId;
    }
}
