using YetAnotherBusWrapper.Abstractions;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Common.Consumers;

public sealed class BusyConsumer : IMessageConsumer<OrderPlaced>
{
    private readonly BusyConsumerSignal _signal;

    public BusyConsumer(BusyConsumerSignal signal)
    {
        _signal = signal;
    }

    public async Task ConsumeAsync(OrderPlaced message, MessageContext context, CancellationToken cancellationToken)
    {
        _signal.MarkStarted();
        await Task.Delay(_signal.BusyDuration, CancellationToken.None);
    }
}
