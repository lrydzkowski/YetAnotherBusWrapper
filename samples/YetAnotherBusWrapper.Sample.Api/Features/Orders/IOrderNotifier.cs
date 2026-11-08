namespace YetAnotherBusWrapper.Sample.Api.Features.Orders;

public interface IOrderNotifier
{
    Task NotifyOrderPlacedAsync(OrderPlaced order, CancellationToken cancellationToken);
}
