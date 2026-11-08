using YetAnotherBusWrapper.Abstractions;

namespace YetAnotherBusWrapper.Sample.Api.Features.Orders;

internal sealed class ServiceBusOrderNotifier : IOrderNotifier
{
    private readonly IMessageSender<OrderPlaced> _sender;

    public ServiceBusOrderNotifier(IMessageSender<OrderPlaced> sender)
    {
        _sender = sender;
    }

    public async Task NotifyOrderPlacedAsync(OrderPlaced order, CancellationToken cancellationToken)
    {
        await _sender.SendAsync(order, cancellationToken: cancellationToken);
    }
}
