using Azure.Messaging.ServiceBus;

namespace YetAnotherBusWrapper.Consuming;

internal interface IReceivedMessageDispatcher
{
    Task DispatchAsync(
        IServiceProvider serviceProvider,
        ServiceBusReceivedMessage message,
        CancellationToken cancellationToken
    );
}
